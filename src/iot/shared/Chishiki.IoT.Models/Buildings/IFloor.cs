// -----------------------------------------------------------------------------
// File:        IFloor.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for a floor within a building hierarchy.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.IoT.Models.Buildings;

/// <summary>Defines a floor contained in a building.</summary>
public interface IFloor : IBuildingEntity
{
    /// <summary>Gets the building that contains the floor.</summary>
    IBuilding Building { get; }

    /// <summary>Gets the rooms that belong to the floor.</summary>
    IReadOnlyCollection<IRoom> Rooms { get; }
}
