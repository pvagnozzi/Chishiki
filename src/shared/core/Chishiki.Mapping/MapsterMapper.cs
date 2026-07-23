// -----------------------------------------------------------------------------
// File:        MapsterMapper.cs
// Author:      Piergiorgio Vagnozzi
// Description: Mapper implementation using Mapster for object mapping operations. This class provides methods to map objects from one type to another using the Mapster library, which is a high-performance object mapping library for .NET. The MapTo methods allow you to either create a new instance of the destination type based on the source object or update an existing instance with values from the source object. This implementation is designed to be used as a service within an application, leveraging dependency injection and logging capabilities provided by the base Service class.
// Created:     2026-05-04
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Services;
using Microsoft.Extensions.Logging;

using InnerMapper = MapsterMapper.IMapper;

namespace Chishiki.Mapping;

/// <summary>Mapper implementation using Mapster for object mapping operations. This class provides methods to map objects from one type to another using the Mapster library, which is a high-performance object mapping library for .NET. The MapTo methods allow you to either create a new instance of the destination type based on the source object or update an existing instance with values from the source object. This implementation is designed to be used as a service within an application, leveraging dependency injection and logging capabilities provided by the base Service class.</summary>
/// <param name="mapper">Mapster instance.</param>
/// <param name="logger">Logger instance</param>
public partial class MapsterMapper(InnerMapper mapper, ILogger<MapsterMapper> logger) : Service(logger), IMapper
{
    /// <summary>Gets the inner mapper instance used for mapping operations within the containing class.</summary>
    protected InnerMapper Mapper => mapper;

    /// <inheritdoc/>
    public virtual TDest MapTo<TDest, TSource>(TSource source)
    {
        LogMappingStarted(typeof(TSource).Name, typeof(TDest).Name);
        try
        {
            var result = mapper.Map<TSource, TDest>(source);
            LogMappingCompleted(typeof(TSource).Name, typeof(TDest).Name);
            return result;
        }
        catch (Exception ex)
        {
            LogMappingFailed(typeof(TSource).Name, typeof(TDest).Name, ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public TDest MapTo<TDest, TSource>(TSource source, TDest dest)
    {
        LogMappingIntoStarted(typeof(TSource).Name, typeof(TDest).Name);
        try
        {
            var result = mapper.Map(source, dest);
            LogMappingIntoCompleted(typeof(TSource).Name, typeof(TDest).Name);
            return result;
        }
        catch (Exception ex)
        {
            LogMappingFailed(typeof(TSource).Name, typeof(TDest).Name, ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapping {SourceType} → {DestType}")]
    private partial void LogMappingStarted(string sourceType, string destType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapping {SourceType} → {DestType} completed")]
    private partial void LogMappingCompleted(string sourceType, string destType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapping {SourceType} into existing {DestType}")]
    private partial void LogMappingIntoStarted(string sourceType, string destType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapping {SourceType} into existing {DestType} completed")]
    private partial void LogMappingIntoCompleted(string sourceType, string destType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Mapping {SourceType} → {DestType} failed")]
    private partial void LogMappingFailed(string sourceType, string destType, Exception ex);
}
