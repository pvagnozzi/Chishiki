// -----------------------------------------------------------------------------
// File:        IIoTEntity.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base contract for IoT entities with a Guid identifier and audit fields.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Models;

namespace Chishiki.IoT.Models.Common;

/// <summary>Defines the base contract for IoT entities.</summary>
public interface IIoTEntity : IEntity<Guid>;
