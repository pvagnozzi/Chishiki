// -----------------------------------------------------------------------------
// File:        EspToolSession.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents an active ESP ROM bootloader session over a serial connection.
// Created:     2026-06-09
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using Chishiki.ESPTool.Chips;
using Chishiki.ESPTool.Exceptions;
using Chishiki.ESPTool.Protocol;
using Chishiki.Serial.Abstractions;

namespace Chishiki.ESPTool;

/// <summary>Represents an active ESP ROM bootloader session bound to a single serial connection.</summary>
public sealed class EspToolSession : Disposable
{
    private const uint SecureDownloadModeFlag = 1u << 2;

    private static readonly byte[] SyncPayload = CreateSyncPayload();

    private readonly ISerialConnectionFactory _serialConnectionFactory;

    private EspChipDefinition _chipDefinition;
    private EspProtocolTransport _transport;

    /// <summary>Initializes a new instance of the <see cref="EspToolSession"/> class.</summary>
    /// <param name="serialConnectionFactory">The serial connection factory used to recreate the connection when needed.</param>
    /// <param name="connection">The underlying serial connection.</param>
    /// <param name="options">The options used to create the session.</param>
    /// <param name="chipDefinition">The chip definition used by the session.</param>
    internal EspToolSession(ISerialConnectionFactory serialConnectionFactory, ISerialConnection connection, EspToolOptions options, EspChipDefinition chipDefinition)
    {
        _serialConnectionFactory = serialConnectionFactory ?? throw new ArgumentNullException(nameof(serialConnectionFactory));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _chipDefinition = chipDefinition ?? throw new ArgumentNullException(nameof(chipDefinition));
        _transport = CreateTransport(Connection, _chipDefinition);
        ChipIdentity = CreateConfiguredChipIdentity(_chipDefinition);
    }

    #region Properties

    /// <summary>Gets the session options.</summary>
    public EspToolOptions Options { get; private set; }

    /// <summary>Gets the resolved chip identity for the session.</summary>
    public EspChipIdentity ChipIdentity { get; private set; }

    /// <summary>Gets the target profile currently used by the session.</summary>
    public EspChipTarget ChipTarget => ChipIdentity.Target;

    /// <summary>Gets a value indicating whether the session has already entered the bootloader reset sequence.</summary>
    public bool IsBootloaderEntered { get; private set; }

    /// <summary>Gets a value indicating whether the session is synchronized with the ROM bootloader.</summary>
    public bool IsSynchronized { get; private set; }

    /// <summary>Gets a value indicating whether a stub loader has been uploaded and activated for the current session.</summary>
    public bool IsStubLoaderActive { get; private set; }

    /// <summary>Gets the most recently loaded stub image when one has been uploaded successfully.</summary>
    public EspStubImage? LoadedStubImage { get; private set; }

    /// <summary>Gets the underlying serial connection used by the session. The connection is owned by the session and will be disposed when the session is disposed.</summary>
    public ISerialConnection Connection { get; private set; }

    #endregion

    #region Operations

    /// <summary>Drives the common DTR/RTS reset sequence used to enter the ROM bootloader.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public async Task EnterBootloaderAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        EnsureOpen();

        Connection.DiscardInBuffer();
        Connection.DiscardOutBuffer();

        Connection.DtrEnable = false;
        Connection.RtsEnable = true;
        await Task.Delay(Options.ResetPulseDuration, cancellationToken).ConfigureAwait(false);

        Connection.DtrEnable = true;
        Connection.RtsEnable = false;
        await Task.Delay(Options.BootloaderReadyDelay, cancellationToken).ConfigureAwait(false);

        Connection.DtrEnable = false;
        await Task.Delay(Options.PostResetDelay, cancellationToken).ConfigureAwait(false);

        Connection.DiscardInBuffer();
        Connection.DiscardOutBuffer();
        IsBootloaderEntered = true;
        IsSynchronized = false;
        IsStubLoaderActive = false;
        LoadedStubImage = null;
    }

    /// <summary>Sends the ROM loader sync command and validates the response.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The response value returned by the ROM loader.</returns>
    /// <exception cref="EspToolException">Thrown when the session has not entered the bootloader sequence.</exception>
    public async Task<uint> SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        EnsureOpen();

        if (!IsBootloaderEntered)
        {
            throw new EspToolException("The session must enter bootloader mode before synchronizing.");
        }

        var response = await _transport.ExecuteCommandAsync(EspCommandCode.Sync, SyncPayload, checksum: 0, Options.SyncTimeout, Options.SyncRetries, cancellationToken).ConfigureAwait(false);
        Connection.DiscardInBuffer();
        IsSynchronized = true;
        return response.Value;
    }

    /// <summary>Detects and applies the most specific chip definition that can be resolved from the connected target.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The resolved chip identity.</returns>
    /// <exception cref="EspToolException">Thrown when the session has not been synchronized.</exception>
    public async Task<EspChipIdentity> DetectChipAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        EnsureSynchronized();

        var securityInfo = await TryGetSecurityInfoAsync(cancellationToken).ConfigureAwait(false);
        if (securityInfo?.ChipId is uint chipId)
        {
            var detectedDefinition = EspChipDefinition.TryCreateFromChipId(chipId);
            if (detectedDefinition is not null)
            {
                var identity = new EspChipIdentity
                {
                    Target = detectedDefinition.Target,
                    Name = detectedDefinition.Name,
                    DetectionSource = EspChipDetectionSource.ChipId,
                    ChipId = chipId,
                    IsSecureDownloadMode = securityInfo.IsSecureDownloadMode
                };

                ApplyChipIdentity(detectedDefinition, identity);
                return identity;
            }
        }

        try
        {
            var magicValue = await ReadRegisterAsync(EspChipDefinition.ChipDetectMagicRegisterAddress, cancellationToken).ConfigureAwait(false);
            var detectedDefinition = EspChipDefinition.TryCreateFromMagicValue(magicValue);
            if (detectedDefinition is not null)
            {
                var identity = new EspChipIdentity
                {
                    Target = detectedDefinition.Target,
                    Name = detectedDefinition.Name,
                    DetectionSource = EspChipDetectionSource.MagicValue,
                    ChipId = securityInfo?.ChipId,
                    MagicValue = magicValue,
                    IsSecureDownloadMode = securityInfo?.IsSecureDownloadMode ?? false
                };

                ApplyChipIdentity(detectedDefinition, identity);
                return identity;
            }

            return ApplyFallbackChipIdentity(securityInfo, magicValue);
        }
        catch (EspToolProtocolException)
        {
            return ApplyFallbackChipIdentity(securityInfo, magicValue: null);
        }
        catch (EspToolTimeoutException)
        {
            return ApplyFallbackChipIdentity(securityInfo, magicValue: null);
        }
    }

    /// <summary>Reads a 32-bit register value from the target ROM.</summary>
    /// <param name="address">The target register address.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The 32-bit register value returned by the target.</returns>
    /// <exception cref="EspToolException">Thrown when the session has not been synchronized.</exception>
    public async Task<uint> ReadRegisterAsync(uint address, CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        EnsureSynchronized();

        var payload = _chipDefinition.BuildReadRegisterPayload(address);
        var response = await _transport.ExecuteCommandAsync(EspCommandCode.ReadRegister, payload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
        return response.Value;
    }

    /// <summary>Writes a 32-bit register value through the ROM bootloader command surface.</summary>
    /// <param name="address">The target register address.</param>
    /// <param name="value">The register value to write.</param>
    /// <param name="mask">The bit mask applied by the bootloader.</param>
    /// <param name="delayUs">The delay in microseconds applied after the write.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="EspToolException">Thrown when the session has not been synchronized.</exception>
    public async Task WriteRegisterAsync(uint address, uint value, uint mask = 0xFFFFFFFF, uint delayUs = 0, CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        EnsureSynchronized();

        var payload = _chipDefinition.BuildWriteRegisterPayload(address, value, mask, delayUs);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.WriteRegister, payload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Loads an embedded upstream esptool stub artifact for the active chip target.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The loaded stub image.</returns>
    /// <exception cref="EspToolException">Thrown when the session is not synchronized or no embedded stub exists for the active target.</exception>
    public async Task<EspStubImage> LoadEmbeddedStubAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        EnsureSynchronized();

        var stubImage = await EspToolEmbeddedStubCatalog.LoadAsync(ChipTarget, cancellationToken).ConfigureAwait(false);
        await LoadStubAsync(stubImage, cancellationToken).ConfigureAwait(false);
        return stubImage;
    }

    /// <summary>Loads a stub-loader image into RAM and starts execution at the supplied entry point.</summary>
    /// <param name="stubImage">The stub-loader image to upload.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stubImage"/> is <see langword="null"/>.</exception>
    /// <exception cref="EspToolException">Thrown when the session is not synchronized or the image is invalid.</exception>
    public async Task LoadStubAsync(EspStubImage stubImage, CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(stubImage);
        EnsureSynchronized();

        if (stubImage.Segments.Count == 0)
        {
            throw new EspToolException("The stub image must contain at least one RAM segment.");
        }

        for (var segmentIndex = 0; segmentIndex < stubImage.Segments.Count; segmentIndex++)
        {
            var segment = stubImage.Segments[segmentIndex] ?? throw new EspToolException($"Stub segment index {segmentIndex} is null.");
            if (segment.Data.Length == 0)
            {
                throw new EspToolException($"Stub segment at address 0x{segment.LoadAddress:X8} cannot be empty.");
            }

            await LoadMemorySegmentAsync(segment, cancellationToken).ConfigureAwait(false);
        }

        var memoryEndPayload = _chipDefinition.BuildMemoryEndPayload(stubImage.EntryPoint, execute: true);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.MemoryEnd, memoryEndPayload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);

        IsStubLoaderActive = true;
        LoadedStubImage = stubImage;
    }

    /// <summary>Changes the active ROM baud rate and reconnects through the serial-connection factory.</summary>
    /// <param name="baudRate">The baud rate to apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="baudRate"/> is less than or equal to zero.</exception>
    /// <exception cref="EspToolException">Thrown when the session has not been synchronized or the target does not support ROM baud-rate changes.</exception>
    public async Task ChangeBaudRateAsync(int baudRate, CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baudRate);
        EnsureSynchronized();

        if (!_chipDefinition.SupportsChangeBaudRate)
        {
            throw new EspToolException($"Chip target '{ChipTarget}' does not support ROM baud-rate changes.");
        }

        if (baudRate == Options.BaudRate)
        {
            return;
        }

        var payload = _chipDefinition.BuildChangeBaudRatePayload(baudRate, Options.BaudRate);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.ChangeBaudRate, payload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
        await ReconnectAsync(baudRate, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Erases the entire SPI flash chip through the active stub loader.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="EspToolException">Thrown when the session is not synchronized or no stub loader is active.</exception>
    public async Task EraseFlashAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        EnsureStubLoaderActive();

        var payload = _chipDefinition.BuildEraseFlashPayload();
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.EraseFlash, payload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Erases a specific SPI flash region through the active stub loader.</summary>
    /// <param name="flashOffset">The flash offset where the erase should start.</param>
    /// <param name="eraseSize">The number of bytes to erase.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="eraseSize"/> is zero.</exception>
    /// <exception cref="EspToolException">Thrown when the session is not synchronized or no stub loader is active.</exception>
    public async Task EraseRegionAsync(uint flashOffset, uint eraseSize, CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        ArgumentOutOfRangeException.ThrowIfZero(eraseSize);
        EnsureStubLoaderActive();

        var payload = _chipDefinition.BuildEraseRegionPayload(flashOffset, eraseSize);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.EraseRegion, payload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Writes a firmware image to the target flash memory.</summary>
    /// <param name="request">The flash write request.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="EspToolException">Thrown when the session has not been synchronized.</exception>
    public async Task FlashAsync(EspFlashRequest request, CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(request);
        EnsureOpen();
        EnsureSynchronized();

        if (request.ImageData.Length == 0)
        {
            throw new EspToolException("The flash image cannot be empty.");
        }

        if (request.UseCompression)
        {
            await FlashCompressedAsync(request, cancellationToken).ConfigureAwait(false);
            return;
        }

        await FlashRawAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reads a flash region through the active stub loader and validates the returned data digest.</summary>
    /// <param name="flashOffset">The flash offset where reading should start.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The bytes read from target flash.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is zero.</exception>
    /// <exception cref="EspToolException">Thrown when the session has not been synchronized, no stub loader is active, no safe ROM flash-read fallback is available, or the returned digest does not match the received bytes.</exception>
    public async Task<byte[]> ReadFlashAsync(uint flashOffset, uint length, CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        ArgumentOutOfRangeException.ThrowIfZero(length);
        EnsureSynchronized();

        if (!IsStubLoaderActive)
        {
            throw CreateReadFlashWithoutStubException();
        }

        return await ReadFlashThroughStubAsync(flashOffset, length, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reads the MD5 digest reported by the target for a specific flash region.</summary>
    /// <param name="flashOffset">The flash offset where hashing should start.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The normalized lower-case hexadecimal MD5 digest.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is zero.</exception>
    /// <exception cref="EspToolException">Thrown when the session has not been synchronized, no stub loader is active, or no safe ROM flash-read fallback is available.</exception>
    public async Task<string> ReadFlashMd5Async(uint flashOffset, uint length, CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        ArgumentOutOfRangeException.ThrowIfZero(length);
        EnsureSynchronized();

        if (!IsStubLoaderActive)
        {
            throw CreateReadFlashWithoutStubException();
        }

        return await ReadFlashMd5ThroughStubAsync(flashOffset, length, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Verifies a flash region by comparing the target MD5 digest against the local firmware image.</summary>
    /// <param name="request">The flash request describing the image and offset to verify.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A structured verification result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="EspToolException">Thrown when the session has not been synchronized, no stub loader is active, or no safe ROM flash-read fallback is available.</exception>
    public async Task<EspFlashVerificationResult> VerifyFlashAsync(EspFlashRequest request, CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(request);

        if (request.ImageData.Length == 0)
        {
            throw new EspToolException("The flash image cannot be empty.");
        }

        // MD5 is required by the ESP32/ESP8266 hardware flashing protocol for checksum verification.
        // This is not used for cryptographic security, but for data integrity checks during hardware communication.
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
        var expectedMd5 = Convert.ToHexStringLower(MD5.HashData(request.ImageData));
#pragma warning restore CA5351
        var actualMd5 = await ReadFlashMd5Async(request.FlashOffset, (uint)request.ImageData.Length, cancellationToken).ConfigureAwait(false);

        return new EspFlashVerificationResult
        {
            FlashOffset = request.FlashOffset,
            Length = (uint)request.ImageData.Length,
            ExpectedMd5 = expectedMd5,
            ActualMd5 = actualMd5
        };
    }

    /// <summary>Performs a simple hard reset sequence on the target device.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        EnsureOpen();

        Connection.DtrEnable = false;
        Connection.RtsEnable = true;
        await Task.Delay(Options.ResetPulseDuration, cancellationToken).ConfigureAwait(false);
        Connection.RtsEnable = false;
        await Task.Delay(Options.PostResetDelay, cancellationToken).ConfigureAwait(false);

        IsBootloaderEntered = false;
        IsSynchronized = false;
        IsStubLoaderActive = false;
        LoadedStubImage = null;
    }

    #endregion

    #region Dispose

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        try
        {
            if (Connection.IsOpen)
            {
                Connection.Close();
            }
        }
        finally
        {
            Connection.Dispose();
        }
    }

    #endregion

    #region Internal Helpers

    /// <summary>Applies a configured chip target when detection is disabled or inconclusive.</summary>
    /// <param name="chipTarget">The chip target to apply.</param>
    internal void UseChipTarget(EspChipTarget chipTarget)
    {
        var chipDefinition = EspChipDefinition.Create(chipTarget);
        ApplyChipIdentity(chipDefinition, CreateConfiguredChipIdentity(chipDefinition));
    }

    #endregion

    #region Private Helpers

    /// <summary>Creates the ROM loader synchronization payload.</summary>
    /// <returns>The standard ESP ROM sync payload.</returns>
    private static byte[] CreateSyncPayload()
    {
        var payload = new byte[36];
        payload[0] = 0x07;
        payload[1] = 0x07;
        payload[2] = 0x12;
        payload[3] = 0x20;
        Array.Fill(payload, (byte)0x55, 4, 32);
        return payload;
    }

    /// <summary>Creates a configured chip-identity object for the provided definition.</summary>
    /// <param name="chipDefinition">The chip definition to describe.</param>
    /// <returns>A configured chip identity.</returns>
    private static EspChipIdentity CreateConfiguredChipIdentity(EspChipDefinition chipDefinition) => new()
    {
        Target = chipDefinition.Target,
        Name = chipDefinition.Name,
        DetectionSource = EspChipDetectionSource.Configured,
        ChipId = chipDefinition.ChipId,
        MagicValue = chipDefinition.MagicValue
    };

    /// <summary>Creates a protocol transport for the active connection and chip definition.</summary>
    /// <param name="connection">The connection to use.</param>
    /// <param name="chipDefinition">The chip definition to use.</param>
    /// <returns>A configured <see cref="EspProtocolTransport"/> instance.</returns>
    private static EspProtocolTransport CreateTransport(ISerialConnection connection, EspChipDefinition chipDefinition) => new(connection, chipDefinition.StatusLength);

    /// <summary>Applies the resolved chip identity and refreshes transport configuration as needed.</summary>
    /// <param name="chipDefinition">The resolved chip definition.</param>
    /// <param name="chipIdentity">The resolved chip identity.</param>
    private void ApplyChipIdentity(EspChipDefinition chipDefinition, EspChipIdentity chipIdentity)
    {
        _chipDefinition = chipDefinition;
        ChipIdentity = chipIdentity;
        _transport = CreateTransport(Connection, chipDefinition);
    }

    /// <summary>Applies the generic fallback chip identity.</summary>
    /// <param name="securityInfo">The parsed security information when available.</param>
    /// <param name="magicValue">The ROM magic value when available.</param>
    /// <returns>The applied fallback identity.</returns>
    private EspChipIdentity ApplyFallbackChipIdentity(SecurityInfo? securityInfo, uint? magicValue)
    {
        var genericDefinition = EspChipDefinition.Create(EspChipTarget.GenericRom);
        var identity = new EspChipIdentity
        {
            Target = genericDefinition.Target,
            Name = genericDefinition.Name,
            DetectionSource = EspChipDetectionSource.Fallback,
            ChipId = securityInfo?.ChipId,
            MagicValue = magicValue,
            IsSecureDownloadMode = securityInfo?.IsSecureDownloadMode ?? false
        };

        ApplyChipIdentity(genericDefinition, identity);
        return identity;
    }

    /// <summary>Attempts to read ROM security information from the connected target.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The parsed security information, or <see langword="null"/> when the command is unavailable on the current ROM.</returns>
    private async Task<SecurityInfo?> TryGetSecurityInfoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _transport.ExecuteCommandAsync(EspCommandCode.GetSecurityInfo, ReadOnlyMemory<byte>.Empty, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
            return ParseSecurityInfo(response.Payload);
        }
        catch (EspToolProtocolException)
        {
            return null;
        }
        catch (EspToolTimeoutException)
        {
            return null;
        }
    }

    /// <summary>Parses the payload returned by the ROM security-info command.</summary>
    /// <param name="payload">The security-info payload without the status trailer.</param>
    /// <returns>The parsed security information.</returns>
    /// <exception cref="EspToolException">Thrown when the payload length is not recognized.</exception>
    private static SecurityInfo ParseSecurityInfo(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 12)
        {
            var flags = BinaryPrimitives.ReadUInt32LittleEndian(payload.Slice(0, 4));
            return new SecurityInfo(flags, chipId: null);
        }

        if (payload.Length == 20)
        {
            var flags = BinaryPrimitives.ReadUInt32LittleEndian(payload.Slice(0, 4));
            var chipId = BinaryPrimitives.ReadUInt32LittleEndian(payload.Slice(12, 4));
            return new SecurityInfo(flags, chipId);
        }

        throw new EspToolException($"Received an unexpected security-info payload length of {payload.Length} bytes.");
    }

    /// <summary>Loads one stub-image segment into RAM.</summary>
    /// <param name="segment">The segment to load.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    private async Task LoadMemorySegmentAsync(EspStubSegment segment, CancellationToken cancellationToken)
    {
        var blockSize = _chipDefinition.MemoryWriteBlockSize;
        var memoryBeginPayload = _chipDefinition.BuildMemoryBeginPayload(segment.Data.Length, segment.LoadAddress, blockSize);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.MemoryBegin, memoryBeginPayload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);

        var sequence = 0u;
        for (var offset = 0; offset < segment.Data.Length; offset += blockSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = Math.Min(blockSize, segment.Data.Length - offset);
            var block = segment.Data.AsSpan(offset, length).ToArray();
            var payload = _chipDefinition.BuildMemoryDataPayload(block, sequence);
            var checksum = EspChecksum.Compute(block);
            _ = await _transport.ExecuteCommandAsync(EspCommandCode.MemoryData, payload, checksum, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
            sequence++;
        }
    }

    /// <summary>Reads a flash region through the active stub loader and validates the returned data digest.</summary>
    /// <param name="flashOffset">The flash offset where reading should start.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The bytes read from target flash.</returns>
    /// <exception cref="EspToolException">Thrown when the returned digest does not match the received bytes.</exception>
    private async Task<byte[]> ReadFlashThroughStubAsync(uint flashOffset, uint length, CancellationToken cancellationToken)
    {
        var payload = _chipDefinition.BuildReadFlashPayload(flashOffset, length);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.ReadFlash, payload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);

        using var outputStream = new MemoryStream(checked((int)length));
        while (outputStream.Length < length)
        {
            var packet = await _transport.ReadSlipPacketAsync(Options.CommandTimeout, cancellationToken).ConfigureAwait(false);
            outputStream.Write(packet, 0, packet.Length);

            if (outputStream.Length < length && packet.Length < 4096)
            {
                throw new EspToolException($"Corrupt data, expected 0x1000 bytes but received 0x{packet.Length:X} bytes.");
            }

            var acknowledgement = new byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(acknowledgement, (uint)outputStream.Length);
            await _transport.WriteSlipPacketAsync(acknowledgement, cancellationToken).ConfigureAwait(false);
        }

        if (outputStream.Length > length)
        {
            throw new EspToolException("Read more flash data than expected.");
        }

        var data = outputStream.ToArray();
        var digestPacket = await _transport.ReadSlipPacketAsync(Options.CommandTimeout, cancellationToken).ConfigureAwait(false);
        var expectedDigest = NormalizeMd5Digest(digestPacket);
        // MD5 is required by the ESP32/ESP8266 hardware flashing protocol for checksum verification.
        // This is not used for cryptographic security, but for data integrity checks during hardware communication.
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
        var actualDigest = Convert.ToHexStringLower(MD5.HashData(data));
#pragma warning restore CA5351
        if (!string.Equals(expectedDigest, actualDigest, StringComparison.OrdinalIgnoreCase))
        {
            throw new EspToolException($"Digest mismatch: expected {expectedDigest}, got {actualDigest}.");
        }

        return data;
    }

    /// <summary>Reads the MD5 digest reported by the target for a specific flash region through the active stub loader.</summary>
    /// <param name="flashOffset">The flash offset where hashing should start.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The normalized lower-case hexadecimal MD5 digest.</returns>
    private async Task<string> ReadFlashMd5ThroughStubAsync(uint flashOffset, uint length, CancellationToken cancellationToken)
    {
        var payload = _chipDefinition.BuildFlashMd5Payload(flashOffset, length);
        var response = await _transport.ExecuteCommandAsync(EspCommandCode.FlashMd5, payload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
        return NormalizeMd5Digest(response.Payload);
    }

    /// <summary>Creates a clear exception for no-stub flash-read requests when no safe ROM fallback is available.</summary>
    /// <returns>The exception describing the unsupported no-stub flash-read state.</returns>
    private EspToolException CreateReadFlashWithoutStubException()
    {
        if (ChipIdentity.IsSecureDownloadMode)
        {
            return new EspToolException($"The requested operation requires an active stub loader. Chip target '{ChipTarget}' is in secure download mode and no ROM flash-read fallback is available.");
        }

        if (EspToolEmbeddedStubCatalog.SupportedTargets.Contains(ChipTarget))
        {
            return new EspToolException($"The requested operation requires an active stub loader. No ROM flash-read fallback is available for chip target '{ChipTarget}'. Call LoadEmbeddedStubAsync() before reading flash.");
        }

        return new EspToolException($"The requested operation requires an active stub loader. No ROM flash-read fallback or embedded stub artifact is available for chip target '{ChipTarget}'.");
    }

    /// <summary>Writes a raw firmware image using the ROM flash commands.</summary>
    /// <param name="request">The flash request to execute.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    private async Task FlashRawAsync(EspFlashRequest request, CancellationToken cancellationToken)
    {
        var flashBeginPayload = _chipDefinition.BuildFlashBeginPayload(request.ImageData.Length, request.FlashOffset);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.FlashBegin, flashBeginPayload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);

        var blockSize = _chipDefinition.FlashWriteBlockSize;
        var sequence = 0u;

        for (var offset = 0; offset < request.ImageData.Length; offset += blockSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = Math.Min(blockSize, request.ImageData.Length - offset);
            var paddedBlock = _chipDefinition.PadFlashBlock(request.ImageData.AsSpan(offset, length));
            var flashDataPayload = _chipDefinition.BuildFlashDataPayload(paddedBlock, sequence);
            var checksum = EspChecksum.Compute(paddedBlock);

            _ = await _transport.ExecuteCommandAsync(EspCommandCode.FlashData, flashDataPayload, checksum, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
            sequence++;
        }

        var flashEndPayload = _chipDefinition.BuildFlashEndPayload(request.ResetAfterFlash);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.FlashEnd, flashEndPayload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);

        FinalizeFlash(request.ResetAfterFlash);
    }

    /// <summary>Writes a compressed firmware image through the active stub loader.</summary>
    /// <param name="request">The flash request to execute.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    private async Task FlashCompressedAsync(EspFlashRequest request, CancellationToken cancellationToken)
    {
        EnsureStubLoaderActive();

        var compressedImage = CompressImageData(request.ImageData);
        var flashBeginPayload = _chipDefinition.BuildFlashDeflateBeginPayload(request.ImageData.Length, compressedImage.Length, request.FlashOffset);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.FlashDeflateBegin, flashBeginPayload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);

        var blockSize = _chipDefinition.StubFlashWriteBlockSize;
        var sequence = 0u;

        for (var offset = 0; offset < compressedImage.Length; offset += blockSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = Math.Min(blockSize, compressedImage.Length - offset);
            var block = compressedImage.AsSpan(offset, length).ToArray();
            var flashDataPayload = _chipDefinition.BuildFlashDeflateDataPayload(block, sequence);
            var checksum = EspChecksum.Compute(block);

            _ = await _transport.ExecuteCommandAsync(EspCommandCode.FlashDeflateData, flashDataPayload, checksum, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);
            sequence++;
        }

        var flashEndPayload = _chipDefinition.BuildFlashDeflateEndPayload(request.ResetAfterFlash);
        _ = await _transport.ExecuteCommandAsync(EspCommandCode.FlashDeflateEnd, flashEndPayload, checksum: 0, Options.CommandTimeout, Options.CommandRetries, cancellationToken).ConfigureAwait(false);

        FinalizeFlash(request.ResetAfterFlash);
    }

    /// <summary>Compresses firmware bytes for stub-loader flashing.</summary>
    /// <param name="imageData">The raw firmware bytes.</param>
    /// <returns>The compressed firmware bytes.</returns>
    private static byte[] CompressImageData(byte[] imageData)
    {
        using var outputStream = new MemoryStream();
        using (var compressionStream = new DeflateStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
        {
            compressionStream.Write(imageData, 0, imageData.Length);
        }

        return outputStream.ToArray();
    }

    /// <summary>Normalizes an MD5 digest returned by the stub loader.</summary>
    /// <param name="payload">The payload returned by the flash-MD5 command.</param>
    /// <returns>The normalized lower-case hexadecimal MD5 digest.</returns>
    /// <exception cref="EspToolException">Thrown when the payload format is not recognized.</exception>
    private static string NormalizeMd5Digest(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 16)
        {
            return Convert.ToHexStringLower(payload);
        }

        if (payload.Length == 32)
        {
            var digest = System.Text.Encoding.ASCII.GetString(payload).TrimEnd('\0');
            if (digest.Length == 32 && digest.All(static value => Uri.IsHexDigit(value)))
            {
                return digest.ToLowerInvariant();
            }
        }

        throw new EspToolException($"Received an unexpected flash-MD5 payload length of {payload.Length} bytes.");
    }

    /// <summary>Applies post-flash state transitions.</summary>
    /// <param name="resetAfterFlash">A value indicating whether the target reboots after flashing.</param>
    private void FinalizeFlash(bool resetAfterFlash)
    {
        if (resetAfterFlash)
        {
            IsSynchronized = false;
            IsBootloaderEntered = false;
            IsStubLoaderActive = false;
            LoadedStubImage = null;
        }
    }

    /// <summary>Reconnects the session using a new baud rate.</summary>
    /// <param name="baudRate">The baud rate to apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    private async Task ReconnectAsync(int baudRate, CancellationToken cancellationToken)
    {
        var updatedOptions = Options.WithBaudRate(baudRate);
        var updatedConnection = _serialConnectionFactory.CreateConnection(updatedOptions.PortName, updatedOptions.ToSerialConnectionOptions());

        try
        {
            if (Connection.IsOpen)
            {
                await Connection.CloseAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            Connection.Dispose();
        }

        Connection = updatedConnection;
        Options = updatedOptions;
        _transport = CreateTransport(Connection, _chipDefinition);

        try
        {
            await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await Task.Delay(Options.BaudRateChangeDelay, cancellationToken).ConfigureAwait(false);
            Connection.DiscardInBuffer();
            Connection.DiscardOutBuffer();
        }
        catch
        {
            IsSynchronized = false;
            IsStubLoaderActive = false;
            LoadedStubImage = null;
            Connection.Dispose();
            throw;
        }
    }

    /// <summary>Ensures the serial connection is currently open.</summary>
    /// <exception cref="EspToolException">Thrown when the serial connection is closed.</exception>
    private void EnsureOpen()
    {
        if (!Connection.IsOpen)
        {
            throw new EspToolException("The underlying serial connection is not open.");
        }
    }

    /// <summary>Ensures the session is synchronized with the ROM bootloader.</summary>
    /// <exception cref="EspToolException">Thrown when the session has not yet synchronized.</exception>
    private void EnsureSynchronized()
    {
        EnsureOpen();

        if (!IsSynchronized)
        {
            throw new EspToolException("The session must be synchronized before this operation can be executed.");
        }
    }

    /// <summary>Ensures the advanced stub-loader command surface is available.</summary>
    /// <exception cref="EspToolException">Thrown when no stub loader is active for the current session.</exception>
    private void EnsureStubLoaderActive()
    {
        EnsureSynchronized();

        if (!IsStubLoaderActive)
        {
            throw new EspToolException("The requested operation requires an active stub loader.");
        }
    }

    /// <summary>Represents the parsed subset of ROM security information needed for chip detection.</summary>
    /// <param name="flags">The raw ROM security flags.</param>
    /// <param name="chipId">The ROM chip identifier when present.</param>
    private sealed class SecurityInfo(uint flags, uint? chipId)
    {
        /// <summary>Gets the raw security flags returned by the ROM.</summary>
        public uint Flags { get; } = flags;

        /// <summary>Gets the ROM chip identifier when available.</summary>
        public uint? ChipId { get; } = chipId;

        /// <summary>Gets a value indicating whether secure download mode is enabled.</summary>
        public bool IsSecureDownloadMode => (Flags & SecureDownloadModeFlag) != 0;
    }

    #endregion
}
