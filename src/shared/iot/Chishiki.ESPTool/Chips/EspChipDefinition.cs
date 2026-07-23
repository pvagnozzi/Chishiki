// -----------------------------------------------------------------------------
// File:        EspChipDefinition.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines chip-specific ROM bootloader protocol behavior.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Buffers.Binary;
using Chishiki.ESPTool.Exceptions;

namespace Chishiki.ESPTool.Chips;

/// <summary>Provides chip-specific protocol configuration and payload builders.</summary>
internal abstract class EspChipDefinition
{
    private const uint SectorSize = 4096;

    private static readonly IReadOnlyDictionary<EspChipTarget, EspChipDefinition> Definitions = new Dictionary<EspChipTarget, EspChipDefinition>
    {
        [EspChipTarget.GenericRom] = new GenericRomChipDefinition(),
        [EspChipTarget.Esp32] = new KnownRomChipDefinition(EspChipTarget.Esp32, "ESP32", chipId: null, magicValue: 0x00F01D83, uartClockDividerRegister: 0x3FF40014),
        [EspChipTarget.Esp32S2] = new KnownRomChipDefinition(EspChipTarget.Esp32S2, "ESP32-S2", chipId: 2, magicValue: 0x000007C6, uartClockDividerRegister: 0x3F400014),
        [EspChipTarget.Esp32S3] = new KnownRomChipDefinition(EspChipTarget.Esp32S3, "ESP32-S3", chipId: 9, magicValue: null, uartClockDividerRegister: 0x60000014),
        [EspChipTarget.Esp32C3] = new KnownRomChipDefinition(EspChipTarget.Esp32C3, "ESP32-C3", chipId: 5, magicValue: null, uartClockDividerRegister: 0x60000014),
        [EspChipTarget.Esp32C2] = new KnownRomChipDefinition(EspChipTarget.Esp32C2, "ESP32-C2", chipId: 12, magicValue: null, uartClockDividerRegister: 0x60000014),
        [EspChipTarget.Esp32C6] = new KnownRomChipDefinition(EspChipTarget.Esp32C6, "ESP32-C6", chipId: 13, magicValue: null, uartClockDividerRegister: 0x60000014),
        [EspChipTarget.Esp32H2] = new KnownRomChipDefinition(EspChipTarget.Esp32H2, "ESP32-H2", chipId: 16, magicValue: null, uartClockDividerRegister: 0x60000014)
    };

    /// <summary>Gets the shared ROM register address used by esptool to detect legacy chips by magic value.</summary>
    public static uint ChipDetectMagicRegisterAddress => 0x40001000;

    #region Properties

    /// <summary>Gets the supported chip target represented by the definition.</summary>
    public abstract EspChipTarget Target { get; }

    /// <summary>Gets the human-readable chip name.</summary>
    public abstract string Name { get; }

    /// <summary>Gets the ROM status trailer size expected for this target.</summary>
    public abstract int StatusLength { get; }

    /// <summary>Gets the flash write block size used by the ROM loader.</summary>
    public abstract int FlashWriteBlockSize { get; }

    /// <summary>Gets the RAM write block size used while loading a stub image.</summary>
    public abstract int MemoryWriteBlockSize { get; }

    /// <summary>Gets the compressed-flash block size used by the stub loader.</summary>
    public abstract int StubFlashWriteBlockSize { get; }

    /// <summary>Gets a value indicating whether ROM baud-rate change is supported for the target.</summary>
    public abstract bool SupportsChangeBaudRate { get; }

    /// <summary>Gets the ROM chip identifier used by security-info based detection when available.</summary>
    public abstract uint? ChipId { get; }

    /// <summary>Gets the ROM magic value used by register-based detection when available.</summary>
    public abstract uint? MagicValue { get; }

    /// <summary>Gets the UART clock-divider register address when known for the target.</summary>
    public abstract uint? UartClockDividerRegister { get; }

    #endregion

    #region Factories

    /// <summary>Creates a chip definition for the requested target.</summary>
    /// <param name="target">The target profile to create.</param>
    /// <returns>A chip definition matching <paramref name="target"/>.</returns>
    /// <exception cref="EspToolException">Thrown when the target is not supported.</exception>
    public static EspChipDefinition Create(EspChipTarget target)
    {
        if (Definitions.TryGetValue(target, out var definition))
        {
            return definition;
        }

        throw new EspToolException($"The chip target '{target}' is not supported by this version of Chishiki.ESPTool.");
    }

    /// <summary>Attempts to resolve a chip definition from a ROM chip identifier.</summary>
    /// <param name="chipId">The chip identifier returned by the target.</param>
    /// <returns>The matching chip definition, or <see langword="null"/> when no definition matches.</returns>
    public static EspChipDefinition? TryCreateFromChipId(uint chipId) => Definitions.Values.FirstOrDefault(definition => definition.ChipId == chipId);

    /// <summary>Attempts to resolve a chip definition from a ROM magic value.</summary>
    /// <param name="magicValue">The ROM magic value returned by the detection register.</param>
    /// <returns>The matching chip definition, or <see langword="null"/> when no definition matches.</returns>
    public static EspChipDefinition? TryCreateFromMagicValue(uint magicValue) => Definitions.Values.FirstOrDefault(definition => definition.MagicValue == magicValue);

    #endregion

    #region Payload Builders

    /// <summary>Builds the payload for the <c>FLASH_BEGIN</c> command.</summary>
    /// <param name="imageLength">The total firmware image length in bytes.</param>
    /// <param name="flashOffset">The destination flash offset.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildFlashBeginPayload(int imageLength, uint flashOffset)
    {
        var eraseSize = AlignUp((uint)imageLength, SectorSize);
        return BuildFlashBeginPayloadCore((uint)imageLength, eraseSize, flashOffset, FlashWriteBlockSize);
    }

    /// <summary>Builds the payload for the <c>FLASH_DEFL_BEGIN</c> command.</summary>
    /// <param name="uncompressedLength">The original firmware image length in bytes.</param>
    /// <param name="compressedLength">The compressed image length in bytes.</param>
    /// <param name="flashOffset">The destination flash offset.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildFlashDeflateBeginPayload(int uncompressedLength, int compressedLength, uint flashOffset)
    {
        var eraseSize = AlignUp((uint)uncompressedLength, SectorSize);
        return BuildFlashBeginPayloadCore((uint)compressedLength, eraseSize, flashOffset, StubFlashWriteBlockSize);
    }

    /// <summary>Builds the payload for the <c>FLASH_DATA</c> command.</summary>
    /// <param name="block">The block to write.</param>
    /// <param name="sequence">The zero-based sequence index.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildFlashDataPayload(ReadOnlySpan<byte> block, uint sequence) => BuildBlockPayload(block, sequence);

    /// <summary>Builds the payload for the <c>FLASH_DEFL_DATA</c> command.</summary>
    /// <param name="block">The compressed block to write.</param>
    /// <param name="sequence">The zero-based sequence index.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildFlashDeflateDataPayload(ReadOnlySpan<byte> block, uint sequence) => BuildBlockPayload(block, sequence);

    /// <summary>Builds the payload for the <c>FLASH_END</c> command.</summary>
    /// <param name="resetAfterFlash">A value indicating whether the target should reboot after flashing.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildFlashEndPayload(bool resetAfterFlash)
    {
        var payload = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, resetAfterFlash ? 0u : 1u);
        return payload;
    }

    /// <summary>Builds the payload for the <c>FLASH_DEFL_END</c> command.</summary>
    /// <param name="resetAfterFlash">A value indicating whether the target should reboot after flashing.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildFlashDeflateEndPayload(bool resetAfterFlash) => BuildFlashEndPayload(resetAfterFlash);

    /// <summary>Builds the payload for the <c>MEM_BEGIN</c> command.</summary>
    /// <param name="imageLength">The total segment length in bytes.</param>
    /// <param name="loadAddress">The RAM address where the segment must be loaded.</param>
    /// <param name="blockSize">The RAM block size to use.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildMemoryBeginPayload(int imageLength, uint loadAddress, int blockSize)
    {
        var blockCount = (uint)((imageLength + blockSize - 1) / blockSize);
        var payload = new byte[16];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), (uint)imageLength);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), blockCount);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(8, 4), (uint)blockSize);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(12, 4), loadAddress);
        return payload;
    }

    /// <summary>Builds the payload for the <c>MEM_DATA</c> command.</summary>
    /// <param name="block">The RAM block to write.</param>
    /// <param name="sequence">The zero-based sequence index.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildMemoryDataPayload(ReadOnlySpan<byte> block, uint sequence) => BuildBlockPayload(block, sequence);

    /// <summary>Builds the payload for the <c>MEM_END</c> command.</summary>
    /// <param name="entryPoint">The entry point to execute.</param>
    /// <param name="execute">A value indicating whether execution should start immediately.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildMemoryEndPayload(uint entryPoint, bool execute)
    {
        var payload = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), execute ? 0u : 1u);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), entryPoint);
        return payload;
    }

    /// <summary>Builds the payload for the <c>READ_REG</c> command.</summary>
    /// <param name="address">The register address to read.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildReadRegisterPayload(uint address)
    {
        var payload = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, address);
        return payload;
    }

    /// <summary>Builds the payload for the <c>WRITE_REG</c> command.</summary>
    /// <param name="address">The register address to write.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="mask">The bit mask applied by the bootloader.</param>
    /// <param name="delayUs">The delay in microseconds applied after the write.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildWriteRegisterPayload(uint address, uint value, uint mask, uint delayUs)
    {
        var payload = new byte[16];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), address);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), value);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(8, 4), mask);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(12, 4), delayUs);
        return payload;
    }

    /// <summary>Builds the payload for the <c>CHANGE_BAUDRATE</c> command.</summary>
    /// <param name="baudRate">The requested baud rate.</param>
    /// <param name="previousBaudRate">The previously active baud rate when required by the loader variant.</param>
    /// <returns>The serialized command payload.</returns>
    /// <exception cref="EspToolException">Thrown when the target does not support baud-rate changes.</exception>
    public virtual byte[] BuildChangeBaudRatePayload(int baudRate, int previousBaudRate)
    {
        if (!SupportsChangeBaudRate)
        {
            throw new EspToolException($"Chip target '{Target}' does not support ROM baud-rate changes.");
        }

        var payload = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), (uint)baudRate);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), (uint)previousBaudRate);
        return payload;
    }

    /// <summary>Builds the payload for the <c>ERASE_REGION</c> command.</summary>
    /// <param name="flashOffset">The flash offset to erase.</param>
    /// <param name="eraseSize">The number of bytes to erase.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildEraseRegionPayload(uint flashOffset, uint eraseSize)
    {
        var payload = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), flashOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), eraseSize);
        return payload;
    }

    /// <summary>Builds the payload for the <c>FLASH_MD5</c> command.</summary>
    /// <param name="flashOffset">The flash offset where hashing should start.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildFlashMd5Payload(uint flashOffset, uint length)
    {
        var payload = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), flashOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), length);
        return payload;
    }

    /// <summary>Builds the payload for the <c>READ_FLASH</c> command.</summary>
    /// <param name="flashOffset">The flash offset where reading should start.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildReadFlashPayload(uint flashOffset, uint length)
    {
        var payload = new byte[16];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), flashOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), length);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(8, 4), SectorSize);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(12, 4), 64u);
        return payload;
    }

    /// <summary>Builds the payload for the <c>ERASE_FLASH</c> command.</summary>
    /// <returns>The serialized command payload.</returns>
    public virtual byte[] BuildEraseFlashPayload() => [];

    /// <summary>Pads the final flash block to the required ROM loader write size.</summary>
    /// <param name="block">The source block to pad.</param>
    /// <returns>A padded block that matches <see cref="FlashWriteBlockSize"/>.</returns>
    public virtual byte[] PadFlashBlock(ReadOnlySpan<byte> block)
    {
        if (block.Length == FlashWriteBlockSize)
        {
            return block.ToArray();
        }

        var paddedBlock = Enumerable.Repeat((byte)0xFF, FlashWriteBlockSize).ToArray();
        block.CopyTo(paddedBlock);
        return paddedBlock;
    }

    #endregion

    #region Private Helpers

    /// <summary>Builds a standard flash-begin style payload.</summary>
    /// <param name="writeLength">The number of bytes that will be transmitted.</param>
    /// <param name="eraseSize">The number of bytes that must be erased before writing.</param>
    /// <param name="flashOffset">The destination flash offset.</param>
    /// <param name="blockSize">The block size used to transmit data.</param>
    /// <returns>The serialized command payload.</returns>
    private static byte[] BuildFlashBeginPayloadCore(uint writeLength, uint eraseSize, uint flashOffset, int blockSize)
    {
        var blockCount = (writeLength + (uint)blockSize - 1) / (uint)blockSize;
        var payload = new byte[16];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), eraseSize);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), blockCount);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(8, 4), (uint)blockSize);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(12, 4), flashOffset);
        return payload;
    }

    /// <summary>Builds a standard block-transfer payload shared by flash and memory write commands.</summary>
    /// <param name="block">The data block to transfer.</param>
    /// <param name="sequence">The zero-based block sequence index.</param>
    /// <returns>The serialized command payload.</returns>
    private static byte[] BuildBlockPayload(ReadOnlySpan<byte> block, uint sequence)
    {
        var payload = new byte[16 + block.Length];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), (uint)block.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(4, 4), sequence);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(8, 4), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(12, 4), 0);
        block.CopyTo(payload.AsSpan(16));
        return payload;
    }

    /// <summary>Aligns a value to a specific boundary.</summary>
    /// <param name="value">The value to align.</param>
    /// <param name="alignment">The required alignment.</param>
    /// <returns>The aligned value.</returns>
    protected static uint AlignUp(uint value, uint alignment) => ((value + alignment - 1) / alignment) * alignment;

    /// <summary>Provides the current generic ROM bootloader implementation.</summary>
    private sealed class GenericRomChipDefinition : EspChipDefinition
    {
        /// <inheritdoc/>
        public override EspChipTarget Target => EspChipTarget.GenericRom;

        /// <inheritdoc/>
        public override string Name => "Generic ROM bootloader";

        /// <inheritdoc/>
        public override int StatusLength => 4;

        /// <inheritdoc/>
        public override int FlashWriteBlockSize => 1024;

        /// <inheritdoc/>
        public override int MemoryWriteBlockSize => 0x1800;

        /// <inheritdoc/>
        public override int StubFlashWriteBlockSize => 0x4000;

        /// <inheritdoc/>
        public override bool SupportsChangeBaudRate => false;

        /// <inheritdoc/>
        public override uint? ChipId => null;

        /// <inheritdoc/>
        public override uint? MagicValue => null;

        /// <inheritdoc/>
        public override uint? UartClockDividerRegister => null;
    }

    /// <summary>Provides a data-driven ROM bootloader definition for a known ESP target.</summary>
    /// <param name="target">The supported target.</param>
    /// <param name="name">The human-readable chip name.</param>
    /// <param name="chipId">The security-info chip identifier when available.</param>
    /// <param name="magicValue">The ROM magic value when available.</param>
    /// <param name="uartClockDividerRegister">The UART clock-divider register address when known.</param>
    private sealed class KnownRomChipDefinition(
        EspChipTarget target,
        string name,
        uint? chipId,
        uint? magicValue,
        uint? uartClockDividerRegister) : EspChipDefinition
    {
        /// <inheritdoc/>
        public override EspChipTarget Target => target;

        /// <inheritdoc/>
        public override string Name => name;

        /// <inheritdoc/>
        public override int StatusLength => 4;

        /// <inheritdoc/>
        public override int FlashWriteBlockSize => 1024;

        /// <inheritdoc/>
        public override int MemoryWriteBlockSize => 0x1800;

        /// <inheritdoc/>
        public override int StubFlashWriteBlockSize => 0x4000;

        /// <inheritdoc/>
        public override bool SupportsChangeBaudRate => true;

        /// <inheritdoc/>
        public override uint? ChipId => chipId;

        /// <inheritdoc/>
        public override uint? MagicValue => magicValue;

        /// <inheritdoc/>
        public override uint? UartClockDividerRegister => uartClockDividerRegister;
    }

    #endregion
}
