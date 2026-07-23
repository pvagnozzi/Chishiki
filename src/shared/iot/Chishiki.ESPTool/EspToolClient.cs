// -----------------------------------------------------------------------------
// File:        EspToolClient.cs
// Author:      Piergiorgio Vagnozzi
// Description: Provides the public entry point for ESP ROM bootloader sessions.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.ESPTool.Chips;
using Chishiki.ESPTool.Exceptions;
using Chishiki.Serial.Abstractions;

namespace Chishiki.ESPTool;

/// <summary>Creates synchronized ESP ROM bootloader sessions using the configured serial abstraction.</summary>
/// <param name="serialConnectionFactory">The serial connection factory used to create serial sessions.</param>
public sealed class EspToolClient(ISerialConnectionFactory serialConnectionFactory)
{
    private readonly ISerialConnectionFactory _serialConnectionFactory = serialConnectionFactory ?? throw new ArgumentNullException(nameof(serialConnectionFactory));

    /// <summary>Opens a serial session, enters the ROM bootloader, synchronizes communication, and optionally detects the connected chip.</summary>
    /// <param name="options">The session options to use.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A ready-to-use synchronized <see cref="EspToolSession"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="EspToolException">Thrown when chip auto-detection conflicts with an explicitly requested chip target.</exception>
    public async Task<EspToolSession> OpenSessionAsync(EspToolOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var initialChipDefinition = EspChipDefinition.Create(options.AutoDetectChip ? EspChipTarget.GenericRom : options.ChipTarget);
        var connection = _serialConnectionFactory.CreateConnection(options.PortName, options.ToSerialConnectionOptions());

        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var session = new EspToolSession(_serialConnectionFactory, connection, options, initialChipDefinition);
            await session.EnterBootloaderAsync(cancellationToken).ConfigureAwait(false);
            _ = await session.SynchronizeAsync(cancellationToken).ConfigureAwait(false);

            if (options.AutoDetectChip)
            {
                var detectedChip = await session.DetectChipAsync(cancellationToken).ConfigureAwait(false);

                if (options.ChipTarget != EspChipTarget.GenericRom)
                {
                    if (detectedChip.Target != EspChipTarget.GenericRom && detectedChip.Target != options.ChipTarget)
                    {
                        throw new EspToolException($"Detected chip '{detectedChip.Name}' does not match the requested target '{options.ChipTarget}'.");
                    }

                    if (detectedChip.Target == EspChipTarget.GenericRom)
                    {
                        session.UseChipTarget(options.ChipTarget);
                    }
                }
            }

            return session;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
