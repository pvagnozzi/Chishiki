// -----------------------------------------------------------------------------
// File:        IDeviceModel.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for reusable IoT device model metadata.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.IoT.Models.Common;

namespace Chishiki.IoT.Models.Devices;

/// <summary>Defines metadata for a reusable IoT device model.</summary>
public interface IDeviceModel : IIoTEntity, IIoTNamed
{
    /// <summary>Gets the device instances currently associated with this model.</summary>
    IReadOnlyCollection<IDevice> Devices { get; }
}
