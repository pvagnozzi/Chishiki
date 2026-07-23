// -----------------------------------------------------------------------------
// File:        IPlant.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for a plant containing buildings and location metadata.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.IoT.Models.Common;

namespace Chishiki.IoT.Models.Buildings;

/// <summary>Defines a plant that groups buildings at an address and with an owner.</summary>
public interface IPlant : IBuildingEntity, IIoTAddress, IIoTOwnable
{
    /// <summary>Gets the buildings associated with the plant.</summary>
    IReadOnlyCollection<IBuilding> Buildings { get; }
}
