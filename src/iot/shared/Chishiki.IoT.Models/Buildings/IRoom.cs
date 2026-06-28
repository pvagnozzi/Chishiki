// -----------------------------------------------------------------------------
// File:        IRoom.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for a room that hosts IoT devices.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.IoT.Models.Buildings;

/// <summary>Defines a room located on a floor and containing IoT devices.</summary>
public interface IRoom : IBuildingEntity
{
    /// <summary>Gets the floor that contains the room.</summary>
    IFloor Floor { get; }
}
