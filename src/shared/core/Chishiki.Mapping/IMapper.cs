// -----------------------------------------------------------------------------
// File:        IMapper.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for mapping objects from one type to another.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Services;

namespace Chishiki.Mapping;

/// <summary>Mapping interface for transforming objects from one type to another. This interface defines methods for mapping a source object to a destination type, either by creating a new instance of the destination type or by populating an existing instance. The MapTo method that returns a new instance allows you to create a new object of the destination type based on the source object, while the MapTo method that takes an existing destination object allows you to update the properties of an existing object based on the source object. This interface can be implemented by mapping libraries or custom mapping logic to facilitate object transformation in applications.</summary>
public interface IMapper : IService
{
    /// <summary>Maps the specified source object to a new instance of the destination type. .</summary>
    /// <typeparam name="TDest">The type to map the source object to.</typeparam>
    /// <typeparam name="TSource">The type of the source object to map.</typeparam>
    /// <param name="source">The object to map to the destination type. Cannot be null.</param>
    /// <returns>A new instance of type TDest with values mapped from the source object.</returns>
    TDest MapTo<TDest, TSource>(TSource source);

    /// <summary>Maps values from the specified source object to the destination object of the given types. .</summary>
    /// <remarks>Both the source and destination objects must be provided and compatible with the mapping
    /// logic. The method updates the destination object in place; it does not create a new instance.</remarks>
    /// <typeparam name="TSource">The type of the source object from which values are mapped.</typeparam>
    /// <typeparam name="TDest">The type of the destination object to which values are mapped.</typeparam>    
    /// <param name="dest">The destination object that receives the mapped values. Must not be null.</param>
    /// <param name="source">The source object from which values are mapped. Must not be null.</param>
    /// <returns>A new instance of type TDest with values mapped from the source object.</returns>
    TDest MapTo<TDest, TSource>(TSource source, TDest dest);
}
