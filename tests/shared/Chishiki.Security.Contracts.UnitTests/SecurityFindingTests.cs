// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Security.Contracts;

namespace Chishiki.Security.Contracts.UnitTests;

[TestFixture, Category("Unit")]
public class SecurityFindingTests
{
    [Test]
    public void SecurityFinding_RecordEquality_WorksCorrectly()
    {
        var finding1 = new SecurityFinding(
            "semgrep", Severity.High, "sqli-001", "SQL Injection",
            "Raw string interpolation in SQL query", "src/Api.cs", 42, "CVE-2024-0001", "Use parameterized queries.");

        var finding2 = new SecurityFinding(
            "semgrep", Severity.High, "sqli-001", "SQL Injection",
            "Raw string interpolation in SQL query", "src/Api.cs", 42, "CVE-2024-0001", "Use parameterized queries.");

        Assert.That(finding1, Is.EqualTo(finding2));
    }

    [Test]
    public void SecurityFinding_DifferentSeverity_AreNotEqual()
    {
        var finding1 = new SecurityFinding(
            "semgrep", Severity.High, "sqli-001", "SQL Injection",
            "Description", "file.cs", null, null, null);

        var finding2 = finding1 with { Severity = Severity.Critical };

        Assert.That(finding1, Is.Not.EqualTo(finding2));
    }

    [Test]
    public void ScanResult_WithNoFindings_IsSuccessWithEmptyList()
    {
        var result = new ScanResult("trivy", "0.60.0", DateTimeOffset.UtcNow, [], IsSuccess: true);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Findings, Is.Empty);
            Assert.That(result.Error, Is.Null);
        });
    }

    [Test]
    public void Severity_Values_AreOrderedByRisk()
    {
        Assert.That((int)Severity.Info,     Is.LessThan((int)Severity.Low));
        Assert.That((int)Severity.Low,      Is.LessThan((int)Severity.Medium));
        Assert.That((int)Severity.Medium,   Is.LessThan((int)Severity.High));
        Assert.That((int)Severity.High,     Is.LessThan((int)Severity.Critical));
    }
}
