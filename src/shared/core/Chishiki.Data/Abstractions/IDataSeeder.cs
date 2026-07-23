// -----------------------------------------------------------------------------
// File:        IDataSeeder.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for data seeding operations.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Services;

namespace Chishiki.Data.Abstractions;

/// <summary>Data seeder interface.</summary>
/// <seealso cref="IService" />
public interface IDataSeeder : IService
{
    /// <summary>Seeds the data asynchronously. .</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeedDataAsync(CancellationToken cancellationToken = default);
}



