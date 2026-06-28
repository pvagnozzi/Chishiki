// -----------------------------------------------------------------------------
// File:        IBuilding.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for a building within a plant hierarchy.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.IoT.Models.Buildings;

/// <summary>Defines a building contained in an IoT plant.</summary>
public interface IBuilding : IBuildingEntity
{
    /// <summary>Gets the plant that contains the building.</summary>
    IPlant Plant { get; }

    /// <summary>Gets the floors that belong to the building.</summary>
    IReadOnlyCollection<IFloor> Floors { get; }
}
