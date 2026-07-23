// -----------------------------------------------------------------------------
// File:        EspCommandCode.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the ROM bootloader command identifiers used by the library.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool.Protocol;

/// <summary>Defines the ROM bootloader commands used by this library.</summary>
internal enum EspCommandCode : byte
{
    /// <summary>Starts a flash write operation.</summary>
    FlashBegin = 0x02,

    /// <summary>Transfers one block of flash data.</summary>
    FlashData = 0x03,

    /// <summary>Completes a flash write operation.</summary>
    FlashEnd = 0x04,

    /// <summary>Begins a RAM write operation.</summary>
    MemoryBegin = 0x05,

    /// <summary>Completes a RAM write operation and optionally executes the loaded image.</summary>
    MemoryEnd = 0x06,

    /// <summary>Transfers one block of RAM data.</summary>
    MemoryData = 0x07,

    /// <summary>Synchronizes communication with the bootloader.</summary>
    Sync = 0x08,

    /// <summary>Writes a target memory register.</summary>
    WriteRegister = 0x09,

    /// <summary>Reads a target memory register.</summary>
    ReadRegister = 0x0A,

    /// <summary>Changes the active serial baud rate.</summary>
    ChangeBaudRate = 0x0F,

    /// <summary>Starts a compressed flash write operation, typically through the stub loader.</summary>
    FlashDeflateBegin = 0x10,

    /// <summary>Transfers one block of compressed flash data.</summary>
    FlashDeflateData = 0x11,

    /// <summary>Completes a compressed flash write operation.</summary>
    FlashDeflateEnd = 0x12,

    /// <summary>Reads the MD5 digest of a flash region, typically through the stub loader.</summary>
    FlashMd5 = 0x13,

    /// <summary>Reads device security information when supported by the ROM.</summary>
    GetSecurityInfo = 0x14,

    /// <summary>Erases the entire SPI flash chip through the stub loader.</summary>
    EraseFlash = 0xD0,

    /// <summary>Erases a specific SPI flash region through the stub loader.</summary>
    EraseRegion = 0xD1,

    /// <summary>Reads SPI flash contents through the stub loader.</summary>
    ReadFlash = 0xD2
}
