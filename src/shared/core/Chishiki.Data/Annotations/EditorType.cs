// -----------------------------------------------------------------------------
// File:        EditorType.cs
// Author:      Piergiorgio Vagnozzi
// Description: Enumeration of editor types for property editing in UI scaffolding.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Annotations;

/// <summary>Enumerates the types of editors that can be used to edit a property.</summary>
public enum EditorType
{
    Text,
    Number,
    Date,
    Time,
    DateTime,
    Email,
    Url,
    Phone,
    Password,
    Hidden,
    Multiline,
    Checkbox,
    Radio,
    Select,
    File,
    Image,
    Color,
    Range,
    Search,
}


