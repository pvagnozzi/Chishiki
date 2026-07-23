// -----------------------------------------------------------------------------
// File:        ChangeAction.cs
// Author:      Piergiorgio Vagnozzi
// Description: Enumeration describing the supported entity change actions for audit tracking.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Data.Audit;

/// <summary>Enumeration of change actions for entity audit tracking.</summary>
public enum ChangeAction
{
    /// <summary>Entity was inserted.</summary>
    Inserted,

    /// <summary>Entity was updated.</summary>
    Updated,

    /// <summary>Entity was deleted.</summary>
    Deleted
}
