// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Contracts.Requests;

/// <summary>Parameters for scaffolding a new project from a hub template.</summary>
/// <param name="TemplateName">Name of the template to scaffold from.</param>
/// <param name="TargetDirectory">Absolute or relative path to the output directory.</param>
/// <param name="Parameters">Optional key-value pairs that override template variables.</param>
public record ScaffoldProjectRequest(
    string TemplateName,
    string TargetDirectory,
    IDictionary<string, string>? Parameters = null
);
