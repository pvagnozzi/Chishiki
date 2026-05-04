// -----------------------------------------------------------------------------
// File:        IUnitOfWorkFactory.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for creating Unit of Work instances.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Abstractions;

/// <summary>
/// Unit of work factory interface.
/// </summary>
/// <seealso cref="IDisposable" />
public interface IUnitOfWorkFactory : IDisposable
{
    /// <summary>
    /// Creates the unit of work.
    /// </summary>
    /// <returns></returns>
    IUnitOfWork CreateUnitOfWork();
}


