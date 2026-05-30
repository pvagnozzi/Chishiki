// -----------------------------------------------------------------------------
// File:        RepositoryMapper.cs
// Author:      Piergiorgio Vagnozzi
// Description: Maps entity types to repository implementations for dynamic repository creation.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Reflection;
using Chishiki.Data.Abstractions;

namespace Chishiki.Data;

/// <summary>Repository mapper.</summary>
/// <seealso cref="IRepositoryMapper" />
public class RepositoryMapper : IRepositoryMapper
{
    /// <summary>The repository types .</summary>
    private readonly ConcurrentDictionary<Type, Type> _repositoryTypes = new();


    /// <summary>Initializes a new instance of the <see cref="RepositoryMapper"/> class. .</summary>
    /// <param name="assemblies">The assemblies.</param>
    public RepositoryMapper(IEnumerable<Assembly>? assemblies = null)
    {
        _ = RegisterFromAssemblies(assemblies?.ToArray() ?? []);
    }

    /// <summary>Registers the specified repository type for the given entity type. .</summary>
    /// <param name="repositoryType">Type of the repository implementing IRepository.</param>
    /// <returns>This mapper instance for method chaining.</returns>
    /// <exception cref="InvalidCastException">Thrown if repositoryType does not implement IRepository interface.</exception>
    public IRepositoryMapper Register(Type repositoryType)
    {
        var entityType = GetEntityType(repositoryType) ?? throw new InvalidCastException($"{repositoryType.FullName} is not a valid Repository type");
        _ = _repositoryTypes.TryAdd(entityType, repositoryType);
        return this;
    }

    /// <summary>Registers repositories from the specified assembly. .</summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>This mapper instance for method chaining.</returns>
    public IRepositoryMapper RegisterFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            var entityType = GetEntityType(type);
            if (entityType is null)
            {
                continue;
            }

            Register(entityType, type);
        }

        return this;
    }

    /// <summary>Registers a repository type for a specific entity type. .</summary>
    /// <param name="assemblies">The assemblies to scan for repository implementations.</param>
    /// <returns>This mapper instance for method chaining.</returns>
    public IRepositoryMapper RegisterFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            _ = RegisterFromAssembly(assembly);
        }

        return this;
    }

    /// <summary>Gets the repository type registered for the specified entity type. .</summary>
    /// <param name="entityType">Type of the entity.</param>
    /// <returns>The registered repository type, or null if no repository is registered for the entity type.</returns>
    public Type? GetRepositoryType(Type entityType) => _repositoryTypes.GetValueOrDefault(entityType);

    /// <summary>Determines whether [is valid repository type] [the specified repository]. .</summary>
    /// <param name="repository">The repository.</param>
    /// <param name="keyType">Type of the key.</param>
    /// <param name="entityType">Type of the entity.</param>
    /// <returns>
    ///   <c>true</c> if [is valid repository type] [the specified repository]; otherwise, <c>false</c>.
    /// </returns>
    protected virtual bool IsValidRepositoryType(Type repository, Type keyType, Type entityType) => !repository.IsGenericType;

    /// <summary>Registers the specified entity type. .</summary>
    /// <param name="entityType">Type of the entity.</param>
    /// <param name="repositoryType">Type of the repository.</param>
    private void Register(Type entityType, Type repositoryType) => _ = _repositoryTypes.TryAdd(entityType, repositoryType);

    /// <summary>Gets the type of the entity. .</summary>
    /// <param name="repositoryType">Type of the repository.</param>
    /// <returns></returns>
    private Type? GetEntityType(Type repositoryType)
    {
        if (!repositoryType.IsClass || repositoryType.IsAbstract)
        {
            return null;
        }

        var interfaceType = (from interFace in repositoryType.GetInterfaces()
                             where interFace.IsGenericType && interFace.GetGenericTypeDefinition() == typeof(IRepository<,>)
                             select interFace).FirstOrDefault();
        if (interfaceType is null)
        {
            return null;
        }

        var arguments = interfaceType.GetGenericArguments();
        var keyType = arguments[0];
        var entityType = arguments[1];

        return IsValidRepositoryType(repositoryType, keyType, entityType) ? entityType : null;
    }
}


