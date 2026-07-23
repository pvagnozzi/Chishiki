// -----------------------------------------------------------------------------
// File:        EspChipTarget.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the supported ESP ROM loader targets.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool;

/// <summary>Identifies the ESP ROM loader profile to use for a session.</summary>
public enum EspChipTarget
{
    /// <summary>Uses the generic ROM bootloader profile implemented by this library.</summary>
    GenericRom = 0,

    /// <summary>Uses the ESP32 ROM bootloader profile.</summary>
    Esp32 = 1,

    /// <summary>Uses the ESP32-S2 ROM bootloader profile.</summary>
    Esp32S2 = 2,

    /// <summary>Uses the ESP32-S3 ROM bootloader profile.</summary>
    Esp32S3 = 3,

    /// <summary>Uses the ESP32-C3 ROM bootloader profile.</summary>
    Esp32C3 = 4,

    /// <summary>Uses the ESP32-C2 ROM bootloader profile.</summary>
    Esp32C2 = 5,

    /// <summary>Uses the ESP32-C6 ROM bootloader profile.</summary>
    Esp32C6 = 6,

    /// <summary>Uses the ESP32-H2 ROM bootloader profile.</summary>
    Esp32H2 = 7
}
