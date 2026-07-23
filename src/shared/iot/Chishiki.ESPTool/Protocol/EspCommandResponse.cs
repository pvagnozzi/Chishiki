// -----------------------------------------------------------------------------
// File:        EspCommandResponse.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a parsed ROM bootloader command response.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool.Protocol;

/// <summary>Represents a parsed response packet returned by the ESP ROM bootloader.</summary>
/// <param name="Command">The command code associated with the response.</param>
/// <param name="Value">The 32-bit response value returned by the bootloader.</param>
/// <param name="Payload">The response payload excluding the protocol status trailer.</param>
internal sealed record EspCommandResponse(EspCommandCode Command, uint Value, byte[] Payload);
