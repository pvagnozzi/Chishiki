// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Security.Contracts;

/// <summary>Abstraction for a single security-scanner tool integration.</summary>
public interface IScannerService
{
    /// <summary>Gets the canonical name of the underlying scanner tool (e.g. "semgrep", "trivy").</summary>
    string ToolName { get; }

    /// <summary>Executes the scanner against the given target path and returns a structured result.</summary>
    /// <param name="targetPath">Absolute or relative path to the source tree or artefact to scan.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="ScanResult"/> containing all findings produced by the scanner.</returns>
    Task<ScanResult> ScanAsync(string targetPath, CancellationToken cancellationToken = default);
}
