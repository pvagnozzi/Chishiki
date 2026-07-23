// -----------------------------------------------------------------------------
// File:        EspFlashVerificationResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Describes the result of a flash verification operation.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool;

/// <summary>Represents the outcome of comparing a local firmware image with the target flash contents.</summary>
public sealed class EspFlashVerificationResult
{
    /// <summary>Gets the flash offset used for the verification request.</summary>
    public required uint FlashOffset { get; init; }

    /// <summary>Gets the number of bytes that were verified.</summary>
    public required uint Length { get; init; }

    /// <summary>Gets the expected MD5 digest computed from the local image.</summary>
    public required string ExpectedMd5 { get; init; }

    /// <summary>Gets the MD5 digest reported by the target.</summary>
    public required string ActualMd5 { get; init; }

    /// <summary>Gets a value indicating whether the digests match.</summary>
    public bool IsMatch => string.Equals(ExpectedMd5, ActualMd5, StringComparison.OrdinalIgnoreCase);
}
