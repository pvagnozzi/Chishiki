// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Security.Contracts;

/// <summary>
/// Aggregates the outcome of a single scanner tool invocation.
/// </summary>
/// <param name="Tool">Name of the scanner tool that ran.</param>
/// <param name="Version">Version string of the scanner tool.</param>
/// <param name="ScannedAt">UTC timestamp when the scan completed.</param>
/// <param name="Findings">All findings produced during the scan.</param>
/// <param name="IsSuccess">Indicates whether the scanner process exited successfully.</param>
/// <param name="Error">Error message if the scan failed; otherwise <see langword="null"/>.</param>
public record ScanResult(
    string Tool,
    string Version,
    DateTimeOffset ScannedAt,
    IReadOnlyList<SecurityFinding> Findings,
    bool IsSuccess,
    string? Error = null
);
