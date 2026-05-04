// -----------------------------------------------------------------------------
// File:        Constructor.cs
// Author:      Piergiorgio Vagnozzi
// Description: Generic constructor helper classes for creating instances via compiled expression trees.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Linq.Expressions;

namespace Chishiki.Core;

/// <summary>Provides compiled constructor invocation for creating instances of type <typeparamref name="TResult"/> with a single argument.</summary>
/// <typeparam name="T1">The type of the first constructor argument.</typeparam>
/// <typeparam name="TResult">The type of the instance to construct.</typeparam>
public class Constructor<T1, TResult>
{

    private readonly Func<T1, TResult> _func;

    /// <summary>Initializes a new instance of the <see cref="Constructor{T1, TResult}"/> class.</summary>
    /// <exception cref="InvalidOperationException">Thrown when constructor not found for the specified type and parameter.</exception>
    public Constructor()
    {
        var ctor = typeof(TResult).GetConstructor([typeof(T1)]);
        if (ctor is null)
        {
            throw new InvalidOperationException($"Constructor not found for {typeof(TResult).Name} ({(typeof(T1).Name)})");
        }
        var param1 = Expression.Parameter(typeof(T1));
        var body = Expression.New(ctor, param1);
        _func = Expression.Lambda<Func<T1, TResult>>(body, param1).Compile();
    }

    /// <summary>Creates an instance of <typeparamref name="TResult"/> with the specified argument.</summary>
    /// <param name="arg1">The first constructor argument.</param>
    /// <returns>A new instance of <typeparamref name="TResult"/> constructed with the specified argument.</returns>
    public TResult Create(T1 arg1) => _func(arg1);
}


/// <summary>Provides compiled constructor invocation for creating instances of type <typeparamref name="TResult"/> with two arguments.</summary>
/// <typeparam name="T1">The type of the first constructor argument.</typeparam>
/// <typeparam name="T2">The type of the second constructor argument.</typeparam>
/// <typeparam name="TResult">The type of the instance to construct.</typeparam>
public class Constructor<T1, T2, TResult>
{

    private readonly Func<T1, T2, TResult> _func;

    /// <summary>Initializes a new instance of the <see cref="Constructor{T1, T2, TResult}"/> class.</summary>
    /// <exception cref="InvalidOperationException">Thrown when constructor not found for the specified type and parameters.</exception>
    public Constructor()
    {
        var ctor = typeof(TResult).GetConstructor([typeof(T1), typeof(T2)]);
        if (ctor is null)
        {
            throw new InvalidOperationException($"Constructor not found for {typeof(TResult).Name} ({(typeof(T1).Name)} {(typeof(T2).Name)})");
        }
        var param1 = Expression.Parameter(typeof(T1));
        var param2 = Expression.Parameter(typeof(T2));
        var body = Expression.New(ctor, param1, param2);
        _func = Expression.Lambda<Func<T1, T2, TResult>>(body, param1, param2).Compile();
    }

    /// <summary>Creates an instance of <typeparamref name="TResult"/> with the specified arguments.</summary>
    /// <param name="arg1">The first constructor argument.</param>
    /// <param name="arg2">The second constructor argument.</param>
    /// <returns>A new instance of <typeparamref name="TResult"/> constructed with the specified arguments.</returns>
    public TResult Create(T1 arg1, T2 arg2) => _func(arg1, arg2);
}

/// <summary>Provides compiled constructor invocation for creating instances of type <typeparamref name="TResult"/> with three arguments.</summary>
/// <typeparam name="T1">The type of the first constructor argument.</typeparam>
/// <typeparam name="T2">The type of the second constructor argument.</typeparam>
/// <typeparam name="T3">The type of the third constructor argument.</typeparam>
/// <typeparam name="TResult">The type of the instance to construct.</typeparam>
public class Constructor<T1, T2, T3, TResult>
{

    private readonly Func<T1, T2, T3, TResult> _func;

    /// <summary>Initializes a new instance of the <see cref="Constructor{T1, T2, T3, TResult}"/> class.</summary>
    /// <exception cref="InvalidOperationException">Thrown when constructor not found for the specified type and parameters.</exception>
    public Constructor()
    {
        var ctor = typeof(TResult).GetConstructor([typeof(T1), typeof(T2), typeof(T3)]);
        if (ctor is null)
        {
            throw new InvalidOperationException($"Constructor not found for {typeof(TResult).Name} ({(typeof(T1).Name)} {(typeof(T2).Name)} {(typeof(T3).Name)})");
        }
        var param1 = Expression.Parameter(typeof(T1));
        var param2 = Expression.Parameter(typeof(T2));
        var param3 = Expression.Parameter(typeof(T3));
        var body = Expression.New(ctor, param1, param2, param3);
        _func = Expression.Lambda<Func<T1, T2, T3, TResult>>(body, param1, param2, param3).Compile();
    }

    /// <summary>Creates an instance of <typeparamref name="TResult"/> with the specified arguments.</summary>
    /// <param name="arg1">The first constructor argument.</param>
    /// <param name="arg2">The second constructor argument.</param>
    /// <param name="arg3">The third constructor argument.</param>
    /// <returns>A new instance of <typeparamref name="TResult"/> constructed with the specified arguments.</returns>
    public TResult Create(T1 arg1, T2 arg2, T3 arg3) => _func(arg1, arg2, arg3);
}

/// <summary>Provides compiled constructor invocation for creating instances of type <typeparamref name="TResult"/> with four arguments.</summary>
/// <typeparam name="T1">The type of the first constructor argument.</typeparam>
/// <typeparam name="T2">The type of the second constructor argument.</typeparam>
/// <typeparam name="T3">The type of the third constructor argument.</typeparam>
/// <typeparam name="T4">The type of the fourth constructor argument.</typeparam>
/// <typeparam name="TResult">The type of the instance to construct.</typeparam>
public class Constructor<T1, T2, T3, T4, TResult>
{

    private readonly Func<T1, T2, T3, T4, TResult> _func;

    /// <summary>Initializes a new instance of the <see cref="Constructor{T1, T2, T3, T4, TResult}"/> class.</summary>
    /// <exception cref="InvalidOperationException">Thrown when constructor not found for the specified type and parameters.</exception>
    public Constructor()
    {
        var ctor = typeof(TResult).GetConstructor([typeof(T1), typeof(T2), typeof(T3), typeof(T4)]);
        if (ctor is null)
        {
            throw new InvalidOperationException($"Constructor not found for {typeof(TResult).Name} ({(typeof(T1).Name)} {(typeof(T2).Name)}, {(typeof(T3).Name)} {(typeof(T4).Name)})");
        }
        var param1 = Expression.Parameter(typeof(T1));
        var param2 = Expression.Parameter(typeof(T2));
        var param3 = Expression.Parameter(typeof(T3));
        var param4 = Expression.Parameter(typeof(T4));
        var body = Expression.New(ctor, param1, param2, param3, param4);
        _func = Expression.Lambda<Func<T1, T2, T3, T4, TResult>>(body, param1, param2, param3, param4).Compile();
    }

    /// <summary>Creates an instance of <typeparamref name="TResult"/> with the specified arguments.</summary>
    /// <param name="arg1">The first constructor argument.</param>
    /// <param name="arg2">The second constructor argument.</param>
    /// <param name="arg3">The third constructor argument.</param>
    /// <param name="arg4">The fourth constructor argument.</param>
    /// <returns>A new instance of <typeparamref name="TResult"/> constructed with the specified arguments.</returns>
    public TResult Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => _func(arg1, arg2, arg3, arg4);
}

/// <summary>Provides compiled constructor invocation for creating instances of type <typeparamref name="TResult"/> with five arguments.</summary>
/// <typeparam name="T1">The type of the first constructor argument.</typeparam>
/// <typeparam name="T2">The type of the second constructor argument.</typeparam>
/// <typeparam name="T3">The type of the third constructor argument.</typeparam>
/// <typeparam name="T4">The type of the fourth constructor argument.</typeparam>
/// <typeparam name="T5">The type of the fifth constructor argument.</typeparam>
/// <typeparam name="TResult">The type of the instance to construct.</typeparam>
public class Constructor<T1, T2, T3, T4, T5, TResult>
{

    private readonly Func<T1, T2, T3, T4, T5, TResult> _func;

    /// <summary>Initializes a new instance of the <see cref="Constructor{T1, T2, T3, T4, T5, TResult}"/> class.</summary>
    /// <exception cref="InvalidOperationException">Thrown when constructor not found for the specified type and parameters.</exception>
    public Constructor()
    {
        var ctor = typeof(TResult).GetConstructor([typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)]);
        if (ctor is null)
        {
            throw new InvalidOperationException($"Constructor not found for {typeof(TResult).Name} ({(typeof(T1).Name)} {(typeof(T2).Name)}, {(typeof(T3).Name)} {(typeof(T4).Name)} {(typeof(T5).Name)})");
        }
        var param1 = Expression.Parameter(typeof(T1));
        var param2 = Expression.Parameter(typeof(T2));
        var param3 = Expression.Parameter(typeof(T3));
        var param4 = Expression.Parameter(typeof(T4));
        var param5 = Expression.Parameter(typeof(T5));
        var body = Expression.New(ctor, param1, param2, param3, param4, param5);
        _func = Expression.Lambda<Func<T1, T2, T3, T4, T5, TResult>>(body, param1, param2, param3, param4, param5).Compile();
    }

    /// <summary>Creates an instance of <typeparamref name="TResult"/> with the specified arguments.</summary>
    /// <param name="arg1">The first constructor argument.</param>
    /// <param name="arg2">The second constructor argument.</param>
    /// <param name="arg3">The third constructor argument.</param>
    /// <param name="arg4">The fourth constructor argument.</param>
    /// <param name="arg5">The fifth constructor argument.</param>
    /// <returns>A new instance of <typeparamref name="TResult"/> constructed with the specified arguments.</returns>
    public TResult Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => _func(arg1, arg2, arg3, arg4, arg5);
}
