// -----------------------------------------------------------------------------
// File:        IIoTNamed.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for IoT entities exposing human-readable naming metadata.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.IoT.Models.Common;

/// <summary>Defines naming metadata for IoT entities.</summary>
public interface IIoTNamed
{
    /// <summary>Gets the short code used to identify the entity.</summary>
    string Code { get; }

    /// <summary>Gets the display name of the entity.</summary>
    string Name { get; }

    /// <summary>Gets a functional description of the entity.</summary>
    string Description { get; }

    /// <summary>Gets the tags used to classify the entity.</summary>
    IReadOnlyCollection<string> Tags { get; }
}
