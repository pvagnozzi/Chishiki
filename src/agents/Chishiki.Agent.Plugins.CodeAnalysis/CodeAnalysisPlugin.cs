// -----------------------------------------------------------------------------
// File:        CodeAnalysisPlugin.cs
// Author:      Piergiorgio Vagnozzi
// Description: Built-in plugin providing Roslyn-based C# code analysis capabilities.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Globalization;
using System.Text;
using Chishiki.Agent.Abstractions.Models;
using Chishiki.Agent.Plugins.Sdk.Attributes;
using Chishiki.Agent.Plugins.Sdk.Base;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace Chishiki.Agent.Plugins.CodeAnalysis;

/// <summary>
/// Built-in plugin that exposes Roslyn-based code analysis capabilities:
/// complexity analysis, diagnostic scanning, and API surface extraction.
/// </summary>
[Plugin(
    "code-analysis",
    "Code Analysis",
    "1.0.0",
    "Roslyn-based C# code analysis: complexity, diagnostics, and API surface extraction.")]
public sealed partial class CodeAnalysisPlugin : PluginBase
{
    #region Capabilities
    /// <summary>Analyses the cyclomatic complexity of methods in the supplied C# source code.</summary>
    [Capability("analyze-complexity", "Computes cyclomatic complexity for each method in C# source.")]
    public static Task<CapabilityResult> AnalyzeComplexityAsync(CapabilityRequest request, CancellationToken cancellationToken)
    {
        if (!request.Parameters.TryGetValue("source", out var sourceObj) || sourceObj is not string source)
        {
            return Task.FromResult(CapabilityResult.Fail("Missing required parameter 'source' (C# source code)."));
        }

        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var root = tree.GetCompilationUnitRoot(cancellationToken);
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

        var sb = new StringBuilder();
        foreach (var method in methods)
        {
            var complexity = ComputeCyclomaticComplexity(method);
            var severity = complexity switch { > 10 => "HIGH", > 5 => "MEDIUM", _ => "LOW" };
            _ = sb.AppendLine(CultureInfo.InvariantCulture, $"{method.Identifier.Text}: complexity={complexity} ({severity})");
        }

        return Task.FromResult(CapabilityResult.Ok(new
        {
            MethodCount = methods.Count,
            Report = sb.ToString().TrimEnd(),
        }));
    }

    /// <summary>Returns compiler diagnostics (errors and warnings) for the supplied C# source.</summary>
    [Capability("find-diagnostics", "Returns Roslyn compiler errors and warnings for C# source code.")]
    public static Task<CapabilityResult> FindDiagnosticsAsync(CapabilityRequest request, CancellationToken cancellationToken)
    {
        if (!request.Parameters.TryGetValue("source", out var sourceObj) || sourceObj is not string source)
        {
            return Task.FromResult(CapabilityResult.Fail("Missing required parameter 'source'."));
        }

        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var compilation = CSharpCompilation.Create("analysis",
            [tree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics(cancellationToken)
            .Where(d => d.Severity >= DiagnosticSeverity.Warning)
            .Select(d => new
            {
                Severity = d.Severity.ToString(),
                d.Id,
                Message = d.GetMessage(CultureInfo.InvariantCulture),
                Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
            })
            .ToList();

        return Task.FromResult(CapabilityResult.Ok(new
        {
            DiagnosticCount = diagnostics.Count,
            Diagnostics = (object)diagnostics,
        }));
    }

    /// <summary>Extracts the public API surface (types, methods, properties) from C# source code.</summary>
    [Capability("extract-api", "Extracts public types, methods, and properties from C# source code.")]
    public static Task<CapabilityResult> ExtractApiAsync(CapabilityRequest request, CancellationToken cancellationToken)
    {
        if (!request.Parameters.TryGetValue("source", out var sourceObj) || sourceObj is not string source)
        {
            return Task.FromResult(CapabilityResult.Fail("Missing required parameter 'source'."));
        }

        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var root = tree.GetCompilationUnitRoot(cancellationToken);

        var types = root.DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Where(t => t.Modifiers.Any(SyntaxKind.PublicKeyword))
            .Select(t => new
            {
                Kind = t.Keyword.Text,
                Name = t.Identifier.Text,
                Methods = t.Members.OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword))
                    .Select(m => m.Identifier.Text).ToList(),
                Properties = t.Members.OfType<PropertyDeclarationSyntax>()
                    .Where(p => p.Modifiers.Any(SyntaxKind.PublicKeyword))
                    .Select(p => p.Identifier.Text).ToList(),
            })
            .ToList();

        return Task.FromResult(CapabilityResult.Ok(new { Types = (object)types }));
    }

    #endregion

    #region Lifecycle

    /// <inheritdoc/>
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        LogInitialised(Logger);
        return Task.CompletedTask;
    }

    #endregion

    #region Private Helpers

    private static int ComputeCyclomaticComplexity(MethodDeclarationSyntax method)
    {
        var complexity = 1;
        foreach (var node in method.DescendantNodes())
        {
            complexity += node switch
            {
                IfStatementSyntax => 1,
                WhileStatementSyntax => 1,
                ForStatementSyntax => 1,
                ForEachStatementSyntax => 1,
                DoStatementSyntax => 1,
                SwitchSectionSyntax => 1,
                CatchClauseSyntax => 1,
                ConditionalExpressionSyntax => 1,
                BinaryExpressionSyntax b when
                    b.IsKind(SyntaxKind.LogicalAndExpression) ||
                    b.IsKind(SyntaxKind.LogicalOrExpression) => 1,
                _ => 0,
            };
        }

        return complexity;
    }

    #endregion

    #region Logging

    [LoggerMessage(EventId = 4000, Level = LogLevel.Information,
        Message = "CodeAnalysis plugin initialised successfully.")]
    private static partial void LogInitialised(ILogger logger);

    #endregion
}
