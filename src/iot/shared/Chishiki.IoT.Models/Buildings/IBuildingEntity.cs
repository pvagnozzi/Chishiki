// -----------------------------------------------------------------------------
// File:        IBuildingEntity.cs
// Author:      Piergiorgio Vagnozzi
// Description: Common contract for entities belonging to the building topology.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.IoT.Models.Common;

namespace Chishiki.IoT.Models.Buildings;

/// <summary>Defines the base contract for building-related entities.</summary>
public interface IBuildingEntity : IIoTEntity, IIoTNamed;
