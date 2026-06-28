// -----------------------------------------------------------------------------
// File:        IIoTUser.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base contract for users that can own IoT entities.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.IoT.Models.Common;

/// <summary>Defines the base contract for an IoT user.</summary>
public interface IIoTUser : IIoTEntity, IIoTNamed;
