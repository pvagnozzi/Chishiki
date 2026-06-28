// -----------------------------------------------------------------------------
// File:        ICountry.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for countries used by IoT addresses.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.IoT.Models.Common;

/// <summary>Defines a country entity used in physical address information.</summary>
public interface ICountry : IIoTEntity, IIoTNamed;
