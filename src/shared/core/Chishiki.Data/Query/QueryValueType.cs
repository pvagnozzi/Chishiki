// -----------------------------------------------------------------------------
// File:        QueryValueType.cs
// Author:      Piergiorgio Vagnozzi
// Description: Enumeration of supported value types for query filter parameters.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Query;

/// <summary>Query value type enumeration.</summary>
#pragma warning disable CA1720 // Identifier contains type name — intentional domain names
public enum QueryValueType
{
    /// <summary>String value.</summary>
    String,
    /// <summary>Integer value.</summary>
    Integer,
    /// <summary>Double value.</summary>
    Double,
    /// <summary>Decimal value.</summary>
    Decimal,
    /// <summary>DateTimeOffset value.</summary>
    DateTimeOffset,
    /// <summary>Boolean value.</summary>
    Boolean,
    /// <summary>Guid value.</summary>
    Guid,
    /// <summary>Enum value (requires TypeValueEnum to be set).</summary>
    Enum,
}
#pragma warning restore CA1720
