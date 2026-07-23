// -----------------------------------------------------------------------------
// File:        IDevice.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for a physical IoT device and its placement in topology.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.IoT.Models.Common;

namespace Chishiki.IoT.Models.Devices;

/// <summary>Defines a physical IoT device.</summary>
public interface IDevice : IIoTEntity, IIoTNamed
{
    /// <summary>
    /// Gets the unique identifier of the device, which is used to distinguish it from other devices in the system.
    /// </summary>
    string DeviceId { get; }

    /// <summary>Gets the device model linked to this device instance.</summary>
    IDeviceModel Model { get; }
}
