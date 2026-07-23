// -----------------------------------------------------------------------------
// File:        IIoTAddress.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract representing a normalized physical and geo-referenced address.
// Created:     2026-06-14
// Modified:    2026-06-14
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.IoT.Models.Common;

/// <summary>Defines a full physical address with geospatial coordinates.</summary>
public interface IIoTAddress
{
    /// <summary>Gets the country associated with the address.</summary>
    ICountry Country { get; }

    /// <summary>Gets the province or state component of the address.</summary>
    string Province { get; }

    /// <summary>Gets the city component of the address.</summary>
    string City { get; }

    /// <summary>Gets the street component of the address.</summary>
    string Street { get; }

    /// <summary>Gets the postal code component of the address.</summary>
    string PostalCode { get; }

    /// <summary>Gets the latitude in decimal degrees.</summary>
    double Latitude { get; }

    /// <summary>Gets the longitude in decimal degrees.</summary>
    double Longitude { get; }

    /// <summary>Gets the altitude in meters.</summary>
    double Altitude { get; }
}
