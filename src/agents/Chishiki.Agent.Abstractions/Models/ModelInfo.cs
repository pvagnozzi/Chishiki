// -----------------------------------------------------------------------------
// File:        ModelInfo.cs
// Author:      Piergiorgio Vagnozzi
// Description: Describes an AI model available through a provider, including capabilities and cost data.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Models;

/// <summary>
/// Describes an AI model available through a provider, including capability flags and pricing information.
/// </summary>
/// <param name="Id">The unique model identifier used when constructing requests.</param>
/// <param name="Name">The human-readable display name of the model.</param>
/// <param name="Provider">The identifier of the provider that hosts this model.</param>
/// <param name="ContextWindow">Maximum number of tokens the model can process in a single request (input + output).</param>
/// <param name="MaxOutputTokens">Maximum number of tokens the model can generate in a single response.</param>
/// <param name="SupportsFunctionCalling">Indicates whether the model supports function/tool calling.</param>
/// <param name="SupportsVision">Indicates whether the model supports image (vision) inputs.</param>
/// <param name="CostPer1KInputTokens">Cost in USD per 1 000 input/prompt tokens. Zero when unknown.</param>
/// <param name="CostPer1KOutputTokens">Cost in USD per 1 000 output/completion tokens. Zero when unknown.</param>
public sealed record ModelInfo(
    string Id,
    string Name,
    string Provider,
    int ContextWindow,
    int MaxOutputTokens,
    bool SupportsFunctionCalling,
    bool SupportsVision,
    decimal CostPer1KInputTokens,
    decimal CostPer1KOutputTokens);
