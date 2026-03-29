// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Security.Contracts;

/// <summary>
/// Represents a single security finding produced by a scanner tool.
/// </summary>
/// <param name="Tool">Name of the tool that produced this finding.</param>
/// <param name="Severity">Severity classification of the finding.</param>
/// <param name="RuleId">Tool-specific rule or check identifier.</param>
/// <param name="Title">Short human-readable title of the finding.</param>
/// <param name="Description">Detailed description of the vulnerability or issue.</param>
/// <param name="FilePath">Source file path where the finding was detected.</param>
/// <param name="Line">Line number within <paramref name="FilePath"/>, or <see langword="null"/> if not applicable.</param>
/// <param name="Cve">Associated CVE identifier, if available.</param>
/// <param name="Remediation">Suggested remediation guidance, if available.</param>
public record SecurityFinding(
    string Tool,
    Severity Severity,
    string RuleId,
    string Title,
    string Description,
    string FilePath,
    int? Line,
    string? Cve,
    string? Remediation
);
