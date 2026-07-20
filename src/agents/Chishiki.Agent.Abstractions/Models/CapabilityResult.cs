// -----------------------------------------------------------------------------
// File:        CapabilityResult.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the outcome of executing a plugin capability.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Models;

/// <summary>Represents the outcome of executing a plugin capability.</summary>
/// <param name="Success">Indicates whether the capability executed without errors.</param>
/// <param name="Data">Optional result data returned by the capability. <c>null</c> when there is no output or on failure.</param>
/// <param name="Error">Optional error message when <paramref name="Success"/> is <c>false</c>.</param>
/// <param name="DurationMs">Wall-clock time in milliseconds taken to execute the capability.</param>
public sealed record CapabilityResult(
    bool Success,
    object? Data,
    string? Error,
    long DurationMs)
{
    #region Factory Methods

    /// <summary>Creates a successful result with the provided data and duration.</summary>
    /// <param name="data">The result data to return.</param>
    /// <param name="durationMs">Wall-clock execution time in milliseconds.</param>
    /// <returns>A new <see cref="CapabilityResult"/> indicating success.</returns>
    public static CapabilityResult Ok(object? data, long durationMs = 0) =>
        new(true, data, null, durationMs);

    /// <summary>Creates a failure result with the provided error message and duration.</summary>
    /// <param name="error">A description of the error that occurred.</param>
    /// <param name="durationMs">Wall-clock execution time in milliseconds.</param>
    /// <returns>A new <see cref="CapabilityResult"/> indicating failure.</returns>
    public static CapabilityResult Fail(string error, long durationMs = 0) =>
        new(false, null, error, durationMs);

    #endregion
}
