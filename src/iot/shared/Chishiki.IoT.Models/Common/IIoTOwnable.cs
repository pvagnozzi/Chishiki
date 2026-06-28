// -----------------------------------------------------------------------------
// File:        IIoTOwnable.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for IoT entities that belong to a specific user.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.IoT.Models.Common;

/// <summary>Defines the ownership contract for IoT entities.</summary>
public interface IIoTOwnable : IIoTEntity
{
    /// <summary>Gets the user that owns the entity.</summary>
    IIoTUser Owner { get; }
}
