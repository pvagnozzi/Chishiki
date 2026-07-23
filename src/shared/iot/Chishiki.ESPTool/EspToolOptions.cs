// -----------------------------------------------------------------------------
// File:        EspToolOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines configurable options for ESP ROM bootloader sessions.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Serial.Abstractions;

namespace Chishiki.ESPTool;

/// <summary>Represents the configuration used to open and operate an ESP ROM bootloader session.</summary>
public sealed class EspToolOptions
{
    /// <summary>Gets the serial port name used to connect to the target device.</summary>
    public required string PortName { get; init; }

    /// <summary>Gets the initial baud rate used for the session.</summary>
    public int BaudRate { get; init; } = 115200;

    /// <summary>Gets the preferred chip target profile to use for command and status parsing.</summary>
    public EspChipTarget ChipTarget { get; init; } = EspChipTarget.GenericRom;

    /// <summary>Gets a value indicating whether the session should try to detect the connected chip after synchronization.</summary>
    public bool AutoDetectChip { get; init; } = true;

    /// <summary>Gets the overall timeout applied while waiting for a command response.</summary>
    public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>Gets the overall timeout applied while waiting for sync responses.</summary>
    public TimeSpan SyncTimeout { get; init; } = TimeSpan.FromSeconds(8);

    /// <summary>Gets the number of retries used for regular commands.</summary>
    public int CommandRetries { get; init; } = 3;

    /// <summary>Gets the number of retries used for the initial bootloader synchronization command.</summary>
    public int SyncRetries { get; init; } = 7;

    /// <summary>Gets the serial driver read timeout used for short polling reads.</summary>
    public TimeSpan SerialReadTimeout { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Gets the serial driver write timeout.</summary>
    public TimeSpan SerialWriteTimeout { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>Gets the delay used while holding the target in reset during bootloader entry.</summary>
    public TimeSpan ResetPulseDuration { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Gets the delay used after releasing reset into the bootloader.</summary>
    public TimeSpan BootloaderReadyDelay { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Gets the delay used after explicit hard reset operations.</summary>
    public TimeSpan PostResetDelay { get; init; } = TimeSpan.FromMilliseconds(250);

    /// <summary>Gets the delay applied after a ROM baud-rate change before resuming communication.</summary>
    public TimeSpan BaudRateChangeDelay { get; init; } = TimeSpan.FromMilliseconds(50);

    #region Helpers

    /// <summary>Creates serial connection options for the configured session.</summary>
    /// <param name="baudRateOverride">An optional baud-rate override for reconnect scenarios.</param>
    /// <returns>A <see cref="SerialConnectionOptions"/> instance configured for binary ESP ROM communication.</returns>
    internal SerialConnectionOptions ToSerialConnectionOptions(int? baudRateOverride = null) => new()
    {
        PortName = PortName,
        BaudRate = baudRateOverride ?? BaudRate,
        ReadTimeout = Math.Max(1, (int)SerialReadTimeout.TotalMilliseconds),
        WriteTimeout = Math.Max(1, (int)SerialWriteTimeout.TotalMilliseconds),
        DtrEnable = false,
        RtsEnable = false,
        TextEncoding = System.Text.Encoding.ASCII,
        NewLine = "\n"
    };

    /// <summary>Creates a copy of the current options using a different baud rate.</summary>
    /// <param name="baudRate">The baud rate to apply to the copied options.</param>
    /// <returns>A new <see cref="EspToolOptions"/> instance.</returns>
    internal EspToolOptions WithBaudRate(int baudRate) => new()
    {
        PortName = PortName,
        BaudRate = baudRate,
        ChipTarget = ChipTarget,
        AutoDetectChip = AutoDetectChip,
        CommandTimeout = CommandTimeout,
        SyncTimeout = SyncTimeout,
        CommandRetries = CommandRetries,
        SyncRetries = SyncRetries,
        SerialReadTimeout = SerialReadTimeout,
        SerialWriteTimeout = SerialWriteTimeout,
        ResetPulseDuration = ResetPulseDuration,
        BootloaderReadyDelay = BootloaderReadyDelay,
        PostResetDelay = PostResetDelay,
        BaudRateChangeDelay = BaudRateChangeDelay
    };

    #endregion
}
