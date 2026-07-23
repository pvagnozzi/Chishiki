// -----------------------------------------------------------------------------
// File:        EspToolSessionTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers the focused ESPTool session behaviors added in the current tranche.
// Created:     2026-06-09
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Buffers.Binary;
using Chishiki.ESPTool.Chips;
using Chishiki.ESPTool.Exceptions;
using Chishiki.ESPTool.Protocol;
using Chishiki.ESPTool.Tests.Fakes;
using Chishiki.Serial.Abstractions;
using NUnit.Framework;

namespace Chishiki.ESPTool.Tests;

/// <summary>Provides focused unit tests for <see cref="EspToolSession"/>.</summary>
public sealed class EspToolSessionTests
{
    [Test]
    public async Task DetectChipAsyncUsesChipIdFromSecurityInfo()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.GetSecurityInfo, payload: BuildSecurityInfoPayload(flags: 0, chipId: 9));

        var session = CreateSession(factory, connection, EspChipTarget.GenericRom);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        var identity = await session.DetectChipAsync();

        Assert.That(identity.Target, Is.EqualTo(EspChipTarget.Esp32S3));
        Assert.That(identity.DetectionSource, Is.EqualTo(EspChipDetectionSource.ChipId));
        Assert.That(identity.ChipId, Is.EqualTo((uint)9));
    }

    [Test]
    public async Task LoadStubAsyncLoadsSegmentsAndMarksStubLoaderActive()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        var stubImage = new EspStubImage
        {
            EntryPoint = 0x40000000,
            Segments =
            [
                new EspStubSegment
                {
                    LoadAddress = 0x3FFAE000,
                    Data = [0x01, 0x02, 0x03, 0x04]
                }
            ]
        };

        await session.LoadStubAsync(stubImage);
        var commands = DecodeWrittenCommands(connection.WrittenBuffers);

        Assert.That(session.IsStubLoaderActive, Is.True);
        Assert.That(session.LoadedStubImage, Is.SameAs(stubImage));
        Assert.That(commands, Is.EqualTo(new[] { EspCommandCode.Sync, EspCommandCode.MemoryBegin, EspCommandCode.MemoryData, EspCommandCode.MemoryEnd }));
    }

    [Test]
    public async Task LoadStubAsyncWithoutSegmentsThrowsClearException()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        var exception = Assert.ThrowsAsync<EspToolException>(async () => await session.LoadStubAsync(new EspStubImage { EntryPoint = 0x40000000, Segments = [] }));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("must contain at least one RAM segment"));
    }

    [Test]
    public async Task ChangeBaudRateAsyncReconnectsUsingFactory()
    {
        var factory = new FakeSerialConnectionFactory();
        var initialConnection = new FakeSerialConnection(CreateSerialOptions());
        var reconnectedConnection = new FakeSerialConnection(CreateSerialOptions(921600));
        factory.EnqueueConnection(reconnectedConnection);

        await initialConnection.OpenAsync();
        initialConnection.QueueResponse(EspCommandCode.Sync);
        initialConnection.QueueResponse(EspCommandCode.ChangeBaudRate);

        var session = CreateSession(factory, initialConnection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        await session.ChangeBaudRateAsync(921600);

        Assert.That(session.Connection, Is.SameAs(reconnectedConnection));
        Assert.That(session.Options.BaudRate, Is.EqualTo(921600));
        Assert.That(factory.CreatedOptions.Any(options => options.BaudRate == 921600), Is.True);
        Assert.That(reconnectedConnection.OpenCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ChangeBaudRateAsyncWhenAlreadyUsingRequestedRateDoesNotReconnect()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());

        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        await session.ChangeBaudRateAsync(115200);

        Assert.That(session.Connection, Is.SameAs(connection));
        Assert.That(factory.CreatedOptions, Is.Empty);
        Assert.That(DecodeWrittenCommands(connection.WrittenBuffers), Is.EqualTo(new[] { EspCommandCode.Sync }));
    }

    [Test]
    public async Task LoadStubAsyncLargeSegmentSplitsIntoMultipleMemoryDataCommands()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        var stubImage = new EspStubImage
        {
            EntryPoint = 0x40000000,
            Segments =
            [
                new EspStubSegment
                {
                    LoadAddress = 0x3FFAE000,
                    Data = Enumerable.Range(0, 0x1800 + 17).Select(static value => (byte)(value % 256)).ToArray()
                }
            ]
        };

        await session.LoadStubAsync(stubImage);

        var memoryDataFrames = GetCommandFrames(connection.WrittenBuffers, EspCommandCode.MemoryData);
        Assert.That(memoryDataFrames, Has.Count.EqualTo(2));
        Assert.That(ReadBlockSequence(memoryDataFrames[0]), Is.EqualTo((uint)0));
        Assert.That(ReadBlockSequence(memoryDataFrames[1]), Is.EqualTo((uint)1));
        Assert.That(ReadBlockLength(memoryDataFrames[0]), Is.EqualTo((uint)0x1800));
        Assert.That(ReadBlockLength(memoryDataFrames[1]), Is.EqualTo((uint)17));
        Assert.That(ReadBlockData(memoryDataFrames[1]).Length, Is.EqualTo(17));
        Assert.That(ReadBlockData(memoryDataFrames[1]), Is.EqualTo(stubImage.Segments[0].Data[^17..]));
    }

    [Test]
    public async Task EraseRegionAsyncUsesStubEraseCommand()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);
        connection.QueueResponse(EspCommandCode.EraseRegion);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();
        await session.LoadStubAsync(CreateStubImage());

        await session.EraseRegionAsync(0x1000, 0x2000);
        var commands = DecodeWrittenCommands(connection.WrittenBuffers);

        Assert.That(commands, Does.Contain(EspCommandCode.EraseRegion));
    }

    [Test]
    public async Task FlashAsyncWithCompressionUsesStubDeflateCommands()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);
        connection.QueueResponse(EspCommandCode.FlashDeflateBegin);
        connection.QueueResponse(EspCommandCode.FlashDeflateData);
        connection.QueueResponse(EspCommandCode.FlashDeflateEnd);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();
        await session.LoadStubAsync(CreateStubImage());

        await session.FlashAsync(new EspFlashRequest
        {
            FlashOffset = 0x1000,
            ImageData = [0x11, 0x22, 0x33, 0x44],
            UseCompression = true,
            ResetAfterFlash = false
        });

        var commands = DecodeWrittenCommands(connection.WrittenBuffers);

        Assert.That(commands, Does.Contain(EspCommandCode.FlashDeflateBegin));
        Assert.That(commands, Does.Contain(EspCommandCode.FlashDeflateData));
        Assert.That(commands, Does.Contain(EspCommandCode.FlashDeflateEnd));
        Assert.That(session.IsStubLoaderActive, Is.True);
    }

    [Test]
    public async Task FlashAsyncRawLargeImageSplitsIntoMultipleFlashDataCommandsAndPadsFinalBlock()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.FlashBegin);
        connection.QueueResponse(EspCommandCode.FlashData);
        connection.QueueResponse(EspCommandCode.FlashData);
        connection.QueueResponse(EspCommandCode.FlashEnd);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        var imageData = Enumerable.Range(0, 1025).Select(static value => (byte)(value % 256)).ToArray();
        await session.FlashAsync(new EspFlashRequest
        {
            FlashOffset = 0x1000,
            ImageData = imageData,
            ResetAfterFlash = false
        });

        var flashDataFrames = GetCommandFrames(connection.WrittenBuffers, EspCommandCode.FlashData);
        Assert.That(flashDataFrames, Has.Count.EqualTo(2));
        Assert.That(ReadBlockSequence(flashDataFrames[0]), Is.EqualTo((uint)0));
        Assert.That(ReadBlockSequence(flashDataFrames[1]), Is.EqualTo((uint)1));
        Assert.That(ReadBlockLength(flashDataFrames[0]), Is.EqualTo((uint)1024));
        Assert.That(ReadBlockLength(flashDataFrames[1]), Is.EqualTo((uint)1024));

        var finalBlock = ReadBlockData(flashDataFrames[1]);
        Assert.That(finalBlock.Length, Is.EqualTo(1024));
        Assert.That(finalBlock[0], Is.EqualTo(imageData[^1]));
        Assert.That(finalBlock[1..], Is.All.EqualTo((byte)0xFF));
    }

    [Test]
    public async Task WriteRegisterAsyncUsesWriteRegisterCommand()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.WriteRegister);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        await session.WriteRegisterAsync(0x3FF40014, 0x00000001, mask: 0xFFFFFFFF, delayUs: 5);

        var commands = DecodeWrittenCommands(connection.WrittenBuffers);
        Assert.That(commands, Does.Contain(EspCommandCode.WriteRegister));
    }

    [Test]
    public async Task ReadFlashMd5AsyncReturnsNormalizedDigest()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);
        connection.QueueResponse(EspCommandCode.FlashMd5, payload: Convert.FromHexString("00112233445566778899AABBCCDDEEFF"));

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();
        await session.LoadStubAsync(CreateStubImage());

        var digest = await session.ReadFlashMd5Async(0x1000, 0x1000);

        Assert.That(digest, Is.EqualTo("00112233445566778899aabbccddeeff"));
    }

    [Test]
    public async Task ReadFlashAsyncWithoutStubThrowsClearUnsupportedException()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        var exception = Assert.ThrowsAsync<EspToolException>(async () => await session.ReadFlashAsync(0x2000, 4));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("No ROM flash-read fallback is available for chip target 'Esp32'"));
        Assert.That(exception.Message, Does.Contain("LoadEmbeddedStubAsync()"));
    }

    [Test]
    public async Task ReadFlashMd5AsyncWithoutStubThrowsClearUnsupportedException()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        var exception = Assert.ThrowsAsync<EspToolException>(async () => await session.ReadFlashMd5Async(0x2000, 4));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("No ROM flash-read fallback is available for chip target 'Esp32'"));
        Assert.That(exception.Message, Does.Contain("LoadEmbeddedStubAsync()"));
    }

    [Test]
    public async Task VerifyFlashAsyncReturnsMatchingResult()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);

        var imageData = new byte[] { 0x10, 0x20, 0x30, 0x40 };
#pragma warning disable CA5351
        var expectedMd5 = Convert.ToHexStringLower(System.Security.Cryptography.MD5.HashData(imageData));
#pragma warning restore CA5351
        connection.QueueResponse(EspCommandCode.FlashMd5, payload: System.Text.Encoding.ASCII.GetBytes(expectedMd5));

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();
        await session.LoadStubAsync(CreateStubImage());

        var result = await session.VerifyFlashAsync(new EspFlashRequest
        {
            FlashOffset = 0x1000,
            ImageData = imageData,
            ResetAfterFlash = false
        });

        Assert.That(result.IsMatch, Is.True);
        Assert.That(result.ActualMd5, Is.EqualTo(expectedMd5));
        Assert.That(result.ExpectedMd5, Is.EqualTo(expectedMd5));
    }

    [Test]
    public async Task ReadFlashAsyncReturnsDataAndAcknowledgesProgress()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);
        connection.QueueResponse(EspCommandCode.ReadFlash);

        var flashData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
#pragma warning disable CA5351
        var digest = System.Security.Cryptography.MD5.HashData(flashData);
#pragma warning restore CA5351
        connection.QueueSlipPacket(flashData);
        connection.QueueSlipPacket(digest);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();
        await session.LoadStubAsync(CreateStubImage());

        var result = await session.ReadFlashAsync(0x2000, (uint)flashData.Length);
        var frames = DecodeWrittenFrames(connection.WrittenBuffers);

        Assert.That(result, Is.EqualTo(flashData));
        Assert.That(frames.Any(frame => frame.Length >= 2 && frame[0] == 0x00 && frame[1] == (byte)EspCommandCode.ReadFlash), Is.True);
        Assert.That(frames.Any(frame => frame.SequenceEqual(BitConverter.GetBytes((uint)flashData.Length))), Is.True);
    }

    [Test]
    public async Task LoadEmbeddedStubAsyncLoadsCatalogStubForActiveChip()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        var stubImage = await session.LoadEmbeddedStubAsync();

        Assert.That(session.IsStubLoaderActive, Is.True);
        Assert.That(stubImage, Is.Not.Null);
        Assert.That(stubImage.Segments, Is.Not.Empty);
        Assert.That(stubImage.Segments.All(segment => segment.Data.Length > 0), Is.True);
    }

    [Test]
    public async Task ReadFlashAsyncThrowsWhenDigestDoesNotMatch()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);
        connection.QueueResponse(EspCommandCode.ReadFlash);
        connection.QueueSlipPacket([0xDE, 0xAD, 0xBE, 0xEF]);
        connection.QueueSlipPacket(new byte[16]);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();
        await session.LoadStubAsync(CreateStubImage());

        Assert.ThrowsAsync<EspToolException>(async () => await session.ReadFlashAsync(0x2000, 4));
    }

    [Test]
    public async Task ReadFlashAsyncThrowsWhenIntermediateStubPacketIsShort()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);
        connection.QueueResponse(EspCommandCode.MemoryBegin);
        connection.QueueResponse(EspCommandCode.MemoryData);
        connection.QueueResponse(EspCommandCode.MemoryEnd);
        connection.QueueResponse(EspCommandCode.ReadFlash);
        connection.QueueSlipPacket([0xDE, 0xAD, 0xBE]);

        var session = CreateSession(factory, connection, EspChipTarget.Esp32);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();
        await session.LoadStubAsync(CreateStubImage());

        var exception = Assert.ThrowsAsync<EspToolException>(async () => await session.ReadFlashAsync(0x2000, 0x1001));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("Corrupt data"));
    }

    [Test]
    public async Task LoadEmbeddedStubAsyncThrowsForGenericRom()
    {
        var factory = new FakeSerialConnectionFactory();
        var connection = new FakeSerialConnection(CreateSerialOptions());
        await connection.OpenAsync();
        connection.QueueResponse(EspCommandCode.Sync);

        var session = CreateSession(factory, connection, EspChipTarget.GenericRom);
        await session.EnterBootloaderAsync();
        await session.SynchronizeAsync();

        var exception = Assert.ThrowsAsync<EspToolException>(async () => await session.LoadEmbeddedStubAsync());
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("No embedded stub artifact is available"));
    }

    [Test]
    public void StubLoaderFromJsonParsesUpstreamShape()
    {
        var json = """
                   {
                     "entry": 1074521688,
                     "text_start": 1074520064,
                     "text": "AQID",
                     "data_start": 1073605548,
                     "data": "BAUG",
                     "bss_start": 1073528832,
                     "bss_size": 2
                   }
                   """;

        var stubImage = EspToolStubLoader.FromJson(json);

        Assert.That(stubImage.EntryPoint, Is.EqualTo((uint)1074521688));
        Assert.That(stubImage.Segments, Has.Count.EqualTo(2));
        Assert.That(stubImage.Segments[0].LoadAddress, Is.EqualTo((uint)1074520064));
        Assert.That(stubImage.Segments[0].Data, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03 }));
        Assert.That(stubImage.Segments[1].LoadAddress, Is.EqualTo((uint)1073528832));
        Assert.That(stubImage.Segments[1].Data, Is.EqualTo(new byte[] { 0x04, 0x05, 0x06, 0x00, 0x00 }));
    }

    private static EspToolSession CreateSession(FakeSerialConnectionFactory factory, FakeSerialConnection connection, EspChipTarget chipTarget)
    {
        var options = new EspToolOptions
        {
            PortName = "COM1",
            ChipTarget = chipTarget,
            AutoDetectChip = false,
            ResetPulseDuration = TimeSpan.Zero,
            BootloaderReadyDelay = TimeSpan.Zero,
            PostResetDelay = TimeSpan.Zero,
            BaudRateChangeDelay = TimeSpan.Zero,
            CommandTimeout = TimeSpan.FromMilliseconds(25),
            SyncTimeout = TimeSpan.FromMilliseconds(25)
        };

        return new EspToolSession(factory, connection, options, EspChipDefinition.Create(chipTarget));
    }

    private static SerialConnectionOptions CreateSerialOptions(int baudRate = 115200) => new()
    {
        PortName = "COM1",
        BaudRate = baudRate
    };

    private static EspStubImage CreateStubImage() => new()
    {
        EntryPoint = 0x40000000,
        Segments =
        [
            new EspStubSegment
            {
                LoadAddress = 0x3FFAE000,
                Data = [0xAA, 0xBB, 0xCC, 0xDD]
            }
        ]
    };

    private static byte[] BuildSecurityInfoPayload(uint flags, uint chipId)
    {
        var payload = new byte[20];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(0, 4), flags);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(12, 4), chipId);
        return payload;
    }

    private static List<EspCommandCode> DecodeWrittenCommands(IEnumerable<byte[]> writtenBuffers)
    {
        var commands = new List<EspCommandCode>();
        foreach (var decodedFrame in DecodeWrittenFrames(writtenBuffers))
        {
            if (decodedFrame.Length >= 2)
            {
                commands.Add((EspCommandCode)decodedFrame[1]);
            }
        }

        return commands;
    }

    private static List<byte[]> GetCommandFrames(IEnumerable<byte[]> writtenBuffers, EspCommandCode command) =>
        DecodeWrittenFrames(writtenBuffers)
            .Where(frame => frame.Length >= 2 && frame[1] == (byte)command)
            .ToList();

    private static uint ReadBlockLength(byte[] frame) => BinaryPrimitives.ReadUInt32LittleEndian(frame.AsSpan(8, 4));

    private static uint ReadBlockSequence(byte[] frame) => BinaryPrimitives.ReadUInt32LittleEndian(frame.AsSpan(12, 4));

    private static byte[] ReadBlockData(byte[] frame) => frame[24..];

    private static List<byte[]> DecodeWrittenFrames(IEnumerable<byte[]> writtenBuffers)
    {
        var frames = new List<byte[]>();
        foreach (var writtenBuffer in writtenBuffers)
        {
            frames.Add(DecodeSlipFrame(writtenBuffer));
        }

        return frames;
    }

    private static byte[] DecodeSlipFrame(byte[] frame)
    {
        var decoded = new List<byte>();
        for (var index = 1; index < frame.Length - 1; index++)
        {
            var value = frame[index];
            if (value == EspSlipCodec.Escape)
            {
                index++;
                decoded.Add(EspSlipCodec.DecodeEscapedByte(frame[index]));
                continue;
            }

            decoded.Add(value);
        }

        return [.. decoded];
    }
}
