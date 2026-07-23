// -----------------------------------------------------------------------------
// File:        IDataProviderFactory.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for creating database provider instances.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Abstractions;

/// <summary>Data provider factory interface.</summary>
public interface IDataProviderFactory : IDisposable
{
    /// <summary>Creates the data provider. .</summary>
    /// <returns>Data provider.</returns>
    IDataProvider CreateDataProvider();
}


