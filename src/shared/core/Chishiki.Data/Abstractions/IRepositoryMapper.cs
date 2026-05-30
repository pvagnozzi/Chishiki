// -----------------------------------------------------------------------------
// File:        IRepositoryMapper.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for mapping between entity types and repository implementations.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Reflection;

namespace Chishiki.Data.Abstractions;

/// <summary>Interface for mapping entity types to repository implementations.</summary>
public interface IRepositoryMapper
{
    /// <summary>Registers a repository type. .</summary>
    /// <param name="repositoryType">The repository type to register.</param>
    /// <returns>The mapper instance for fluent chaining.</returns>
    IRepositoryMapper Register(Type repositoryType);

    /// <summary>Registers all repository types from the specified assembly. .</summary>
    /// <param name="assembly">The assembly to scan for repository types.</param>
    /// <returns>The mapper instance for fluent chaining.</returns>
    IRepositoryMapper RegisterFromAssembly(Assembly assembly);

    /// <summary>Registers all repository types from the specified assemblies. .</summary>
    /// <param name="assemblies">The assemblies to scan for repository types.</param>
    /// <returns>The mapper instance for fluent chaining.</returns>
    IRepositoryMapper RegisterFromAssemblies(IEnumerable<Assembly> assemblies);

    /// <summary>Gets the repository type for the given entity type. .</summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The repository type for the entity, or null if not found.</returns>
    Type? GetRepositoryType(Type entityType);
}


