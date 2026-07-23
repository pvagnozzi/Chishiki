// -----------------------------------------------------------------------------
// File:        CapabilityRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Encapsulates the input parameters for invoking a plugin capability.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Models;

/// <summary>Encapsulates the input parameters required to invoke a named plugin capability.</summary>
/// <param name="CapabilityName">The name of the capability to execute.</param>
/// <param name="Parameters">Key-value map of named parameters passed to the capability.</param>
/// <param name="Context">Optional caller-supplied context object passed through to the capability handler.</param>
public sealed record CapabilityRequest(
    string CapabilityName,
    Dictionary<string, object> Parameters,
    object? Context = null);
