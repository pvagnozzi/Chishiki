// -----------------------------------------------------------------------------
// File:        IPropertyAudit.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for recording old and new values of a changed audited property.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Data.Audit;

/// <summary>Interface for property-level audit tracking.</summary>
public interface IPropertyAudit
{
    /// <summary>Gets the name of the property that changed. .</summary>
    string PropertyName { get; }

    /// <summary>Gets the old value of the property before the change. .</summary>
    string? OldValue { get; }

    /// <summary>Gets the new value of the property after the change. .</summary>
    string? NewValue { get; }
}
