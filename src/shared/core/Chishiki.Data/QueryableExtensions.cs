// -----------------------------------------------------------------------------
// File:        QueryableExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for LINQ query operations on IQueryable.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Core;
using System.Linq.Expressions;
using Chishiki.Data.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using Chishiki.Data.Models;

namespace Chishiki.Data;

/// <summary>
/// Queryable extensions.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// The null expression.
    /// </summary>
    private static readonly ConstantExpression NullExpression = Expression.Constant(null);

    /// <summary>
    /// Orders ascending.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Ordered list.</returns>
    [DebuggerStepThrough]
    // ReSharper disable once MemberCanBePrivate.Global
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName) =>
        source.OrderByUsing(propertyName, "OrderBy");

    /// <summary>
    /// Orders the by descending.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Orderer list.</returns>
    [DebuggerStepThrough]
    // ReSharper disable once MemberCanBePrivate.Global
    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName) =>
        source.OrderByUsing(propertyName, "OrderByDescending");

    /// <summary>
    /// Then orders ascending.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Ordered list.</returns>
    [DebuggerStepThrough]
    // ReSharper disable once MemberCanBePrivate.Global
    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName) =>
        source.OrderByUsing(propertyName, "ThenBy");

    /// <summary>
    /// Then orders descending.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Ordered list.</returns>
    [DebuggerStepThrough]
    // ReSharper disable once MemberCanBePrivate.Global
    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string propertyName) =>
        source.OrderByUsing(propertyName, "ThenByDescending");

    /// <summary>
    /// Sorts by specified sort expressions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="sortExpressions">The sort expressions.</param>
    /// <returns>Ordered list.</returns>
    public static IQueryable<T> Sort<T>(this IQueryable<T> source, IEnumerable<ISortExpression> sortExpressions)
    {
        var sorts = sortExpressions.ToArray();
        if (sorts.Length == 0)
        {
            return source;
        }

        var firstSort = sorts[0];

        var result = firstSort.Descending
            ? source.OrderByDescending(firstSort.PropertyName)
            : source.OrderBy(firstSort.PropertyName);

        return sorts.Skip(1)
            .Aggregate(result, (current, sort) => sort.Descending
                ? current.ThenByDescending(firstSort.PropertyName)
                : current.ThenBy(firstSort.PropertyName));
    }

    /// <summary>
    /// Orders the queryable by the specified property name using reflection and dynamic LINQ.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source queryable.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="propertyName">Name of the property to order by. Supports nested properties with dot notation.</param>
    /// <param name="method">The LINQ method name ('OrderBy', 'OrderByDescending', 'ThenBy', 'ThenByDescending').</param>
    /// <returns>An ordered queryable.</returns>
    private static IOrderedQueryable<T> OrderByUsing<T>(this IQueryable<T> source, string propertyName, string method)
    {
        var parameter = Expression.Parameter(typeof(T), "item");
        Expression member = parameter;
        var currentType = typeof(T);

        // Gestione delle proprietà annidate e delle proprietà IEnumerable
        foreach (var part in propertyName.Split('.'))
        {
            var property = currentType.GetProperty(part) ?? throw new ArgumentException($"Property '{part}' not found on type '{currentType.Name}'");

            if (property.PropertyType != typeof(string) &&
                typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                var elementType = property.PropertyType.IsGenericType
                    ? property.PropertyType.GetGenericArguments()[0]
                    : typeof(object);

                member = Expression.Call(
                    typeof(Enumerable),
                    "FirstOrDefault",
                    [elementType],
                    Expression.PropertyOrField(member, part)
                );
                currentType = elementType;
            }
            else
            {
                member = Expression.PropertyOrField(member, part);
                currentType = property.PropertyType;
            }
        }

        var keySelector = Expression.Lambda(member, parameter);
        var methodCall = Expression.Call(
            typeof(Queryable),
            method,
            [parameter.Type, member.Type],
            source.Expression,
            Expression.Quote(keySelector)
        );

        return (IOrderedQueryable<T>)source.Provider.CreateQuery(methodCall);
    }

    /// <summary>
    /// Filters by the specified filters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="filters">The filters.</param>
    /// <returns>Filtered list.</returns>    
    public static IQueryable<T> Where<T>(this IQueryable<T> source, IEnumerable<IFilterConditionExpression> filters)
    {
        var filterConditionExpressions = filters.ToArray();
        return filterConditionExpressions.Length == 0
            ? source
            : filterConditionExpressions.Select(filter => filter.ToExpression<T>())
            .Aggregate(source, (current, expression) => current.Where(expression));
    }

    /// <summary>
    /// Creates a MemberExpression.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Member expression.</returns>
    [DebuggerStepThrough]
    public static Expression ToMemberExpression(this Expression parameter, string propertyName) =>
        propertyName.Split('.').Aggregate(parameter, Expression.PropertyOrField);

    /// <summary>
    /// Converts to expression.
    /// </summary>
    /// <typeparam name="T">Instance type.</typeparam>
    /// <param name="filterCondition">The filter condition.</param>
    /// <returns>Expression filter.</returns>
    public static Expression<Func<T, bool>> ToExpression<T>(this IFilterConditionExpression filterCondition)
    {
        var propertyName = filterCondition.PropertyName;
        var parameter = Expression.Parameter(typeof(T), "item");
        var propertyNameParts = propertyName.Split('.');
        var currentType = typeof(T);

        // Prepares the chain of properties
        var properties = new List<(string name, Type type, bool IsEnumerable)>();
        var containsEnumerable = false;
        var firstElement = false;
        foreach (var propertyNamePart in propertyNameParts)
        {
            var propertyInfo = currentType.GetProperty(propertyNamePart) ?? throw new InvalidExpressionException($"{propertyName} not found in {currentType}");
            var isEnumerable = !firstElement && propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.IsEnumerable();
            firstElement = false;
            if (isEnumerable)
            {
                containsEnumerable = true;
            }

            currentType = isEnumerable
                ? propertyInfo.PropertyType.GenericTypeArguments[0]
                : propertyInfo.PropertyType;

            properties.Add((propertyNamePart, currentType, isEnumerable));
        }

        if (!containsEnumerable)
        {
            var memberExpression = properties
                .Aggregate((Expression)parameter, (exp, a) =>
                {
                    var (name, _, _) = a;
                    return Expression.PropertyOrField(exp, name);
                });

            var expression =
                memberExpression.ToExpression(Expression.Constant(filterCondition.Value),
                    filterCondition.ConditionOperator, filterCondition.IgnoreCasing);
            return Expression.Lambda<Func<T, bool>>(expression, parameter);
        }
        else
        {
            // First, build the chain of properties up to the enumerable
            Expression memberExpression = parameter;
            var enumerableIndex = -1;

            for (var i = 0; i < properties.Count; i++)
            {
                if (properties[i].IsEnumerable)
                {
                    enumerableIndex = i;
                    memberExpression = Expression.PropertyOrField(memberExpression, properties[i].name);
                    break;
                }
                memberExpression = Expression.PropertyOrField(memberExpression, properties[i].name);
            }

            if (enumerableIndex >= 0)
            {
                // Create a parameter for the lambda inside Any
                var elementType = properties[enumerableIndex].type;
                var elementParam = Expression.Parameter(elementType, "element");

                // Build the property chain for the element inside the enumerable
                Expression elementExpr = elementParam;
                for (int i = enumerableIndex + 1; i < properties.Count; i++)
                {
                    elementExpr = Expression.PropertyOrField(elementExpr, properties[i].name);
                }

                // Create the condition expression for the element
                var conditionExpr = elementExpr.ToExpression(Expression.Constant(filterCondition.Value),
                    filterCondition.ConditionOperator, filterCondition.IgnoreCasing);

                // Create the Any call with the lambda
                var anyLambda = Expression.Lambda(conditionExpr, elementParam);
                var anyCall = Expression.Call(typeof(Enumerable), "Any", [elementType], memberExpression, anyLambda);

                return Expression.Lambda<Func<T, bool>>(anyCall, parameter);
            }
            else
            {
                throw new InvalidOperationException("Expected an enumerable property in the chain, but none was found.");
            }
        }
    }

    /// <summary>
    /// Converts to expression.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <param name="value">The value.</param>
    /// <param name="filterConditionOperator">The filter condition operator.</param>
    /// <param name="ignoreCasing"></param>
    /// <returns></returns>
    private static Expression ToExpression(this Expression member, Expression value,
        FilterConditionOperator filterConditionOperator, bool ignoreCasing)
    {
        // Check if member is a nullable type
        var memberType = member.Type;
        var valueType = value.Type;
        var isNullable = memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Nullable<>);

        // Handle null comparisons differently
        if (filterConditionOperator == FilterConditionOperator.IsNull)
        {
            return Expression.Equal(member, NullExpression);
        }

        if (filterConditionOperator == FilterConditionOperator.IsNotNull)
        {
            return Expression.NotEqual(member, NullExpression);
        }

        // If the member is nullable but the value is not, we need to handle type conversion                
        var nullableType = isNullable && !valueType.IsGenericType;
        var valueProperty = !nullableType ? member : Expression.Property(member, "Value");

        // Create the comparison with the unwrapped value
        Expression comparisonExpression = filterConditionOperator switch
        {
            FilterConditionOperator.Equal => Expression.Equal(valueProperty, value),
            FilterConditionOperator.NotEqual => Expression.NotEqual(valueProperty, value),
            FilterConditionOperator.Lesser => Expression.LessThan(valueProperty, value),
            FilterConditionOperator.LesserEqual => Expression.LessThanOrEqual(valueProperty, value),
            FilterConditionOperator.Greater => Expression.GreaterThan(valueProperty, value),
            FilterConditionOperator.GreaterEqual => Expression.GreaterThanOrEqual(valueProperty, value),
            FilterConditionOperator.Contains => valueProperty.ToCallExpression<string>(value, "Contains", ignoreCasing),
            FilterConditionOperator.NotContains => Expression.Not(valueProperty.ToCallExpression<string>(value, "Contains", ignoreCasing)),
            FilterConditionOperator.StartsWith => valueProperty.ToCallExpression<string>(value, "StartsWith", ignoreCasing),
            FilterConditionOperator.EndsWith => valueProperty.ToCallExpression<string>(value, "EndsWith", ignoreCasing),
            FilterConditionOperator.NotStartsWith => Expression.Not(valueProperty.ToCallExpression<string>(value, "StartsWith", ignoreCasing)),
            FilterConditionOperator.NotEndsWith => Expression.Not(valueProperty.ToCallExpression<string>(value, "EndsWith", ignoreCasing)),
            FilterConditionOperator.In => Expression.Equal(valueProperty, NullExpression),
            FilterConditionOperator.NotIn => Expression.Not(Expression.Equal(valueProperty, NullExpression)),
            _ => throw new InvalidDataException($"Operator {filterConditionOperator} is not supported for nullable types")
        };

        if (!isNullable)
        {
            return comparisonExpression;
        }

        var hasValueProperty = Expression.Property(member, "HasValue");
        return Expression.AndAlso(hasValueProperty, comparisonExpression);
    }


    /// <summary>
    /// Converts to CallExpression.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="memberExpression">The member expression.</param>
    /// <param name="valueExpression">The value expression.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="ignoreCasing"></param>
    /// <returns>Call expression</returns>
    /// <exception cref="InvalidDataException"></exception>
    private static MethodCallExpression ToCallExpression<T>(this Expression memberExpression,
        Expression valueExpression, string methodName, bool ignoreCasing)
    {
        var memberType = typeof(T);
        var methods = memberType.GetMethods();
        if (methods.Length == 0)
        {
            throw new InvalidDataException($"{memberType} has no method");
        }

        foreach (var method in methods.Where(s => s.Name == methodName))
        {
            try
            {
                if (!ignoreCasing)
                {
                    return Expression.Call(memberExpression, method, valueExpression);
                }

                var a = Expression.Call(memberExpression, "ToLower", null);
                var b = Expression.Call(valueExpression, "ToLower", null);
                return Expression.Call(a, method, b);

            }
            catch
            {
                // ignore
            }
        }

        throw new InvalidDataException($"{memberType} has no suitable for method {methodName}");
    }

    /// <summary>
    /// Converts to PagedList.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    public static PagedList<TEntity> ToPagedList<TKey, TEntity>(this IQueryable<TEntity> source,
        int pageSize = 10,
        int pageNumber = 0)
        where TEntity : class, IEntity<TKey>
    {
        var count = source.Count();
        var items = source.Skip(pageNumber * pageSize).Take(pageSize).ToList();
        return new PagedList<TEntity>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Converts to PagedList async.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static async Task<PagedList<TEntity>> ToPagedListAsync<TKey, TEntity>(
        this IQueryable<TEntity> source,
        int pageSize = 10,
        int pageNumber = 0,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TKey>
    {
        var count = await source.CountAsync(cancellationToken);
        var orderedQueryable = source.OrderMethodExists() ? source : source.OrderBy(s => s.Id);
        var items = await orderedQueryable
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<TEntity>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Converts to paged list with select expression.
    /// </summary>
    /// <param name="source">Source data.</param>
    /// <param name="selectExpression">Select expression.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="pageNumber">Page number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>Paged list.</returns>
    public static async Task<PagedList<TResult>> ToPagedListWithSelectAsync<TKey, TEntity, TResult>(
        this IQueryable<TEntity> source,
        Func<TEntity, int, TResult> selectExpression,
        int pageSize = 10,
        int pageNumber = 0,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity<TKey>
    {
        var res = await source.ToPagedListAsync<TKey, TEntity>(
            pageSize,
            pageNumber,
            cancellationToken);

        List<TResult> mappedItems = [.. res.Items.Select(selectExpression)];
        return new PagedList<TResult>(mappedItems, res.TotalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Includes the specified include expression.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="includeExpression">The include expression.</param>
    /// <returns>Expresion with include.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IQueryable<T> Include<T>(this IQueryable<T> source, IIncludeExpression includeExpression)
        where T : class
    {
        _ = source.Include(includeExpression.PropertyName);

        foreach (var childExpression in includeExpression.NestedExpressions)
        {
            _ = source.Include(childExpression);
        }

        return source;
    }

    /// <summary>
    /// Includes the specified include expressions.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="includeExpressions">The include expressions.</param>
    /// <returns>Filtered result.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IQueryable<T> Include<T>(this IQueryable<T> source,
        IEnumerable<IIncludeExpression> includeExpressions)
        where T : class
    {
        foreach (var includeExpression in includeExpressions)
        {
            _ = source.Include(includeExpression);
        }

        return source;
    }

    /// <summary>
    /// Determines whether an ordering method exists in the queryable expression.
    /// </summary>
    /// <typeparam name="T">The type of elements in the queryable.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <returns>True if an OrderBy or ThenBy method is found in the expression; otherwise, false.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static bool OrderMethodExists<T>(this IQueryable<T> source) =>
        OrderingMethodFinder.OrderMethodExists(source.Expression);

    /// <summary>
    /// Applies the specification to the query source.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The filtered and sorted queryable.</returns>
    public static IQueryable<TEntity> ApplySpecification<TKey, TEntity>(
        this IQueryable<TEntity> source,
        ISpecification<TKey, TEntity> specification)
        where TEntity : class, IEntity<TKey>
    {
        _ = source.Include(specification.Includes);

        if (specification.Where is not null)
        {
            source = source.Where(specification.Where);
        }

        source = source.Where(specification.Filters);
        source = source.Sort(specification.Sort);

        return source;
    }

    /// <summary>
    /// Internal visitor for detecting whether an expression tree contains ordering methods (OrderBy/ThenBy).
    /// </summary>
    private sealed class OrderingMethodFinder : ExpressionVisitor
    {
        /// <summary>
        /// Gets a value indicating whether an ordering method was found in the visited expression.
        /// </summary>
        public bool OrderingMethodFound { get; set; }

        /// <summary>
        /// Visits a method call expression and checks if it is an ordering method (OrderBy or ThenBy).
        /// </summary>
        /// <param name="node">The method call expression to visit.</param>
        /// <returns>The method call expression after visiting.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var name = node.Method.Name;

            if (node.Method.DeclaringType == typeof(Queryable) && (
                    name.StartsWith("OrderBy", StringComparison.Ordinal) ||
                    name.StartsWith("ThenBy", StringComparison.Ordinal)))
            {
                OrderingMethodFound = true;
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Determines if an ordering method exists in the specified expression.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>True if an ordering method is found; otherwise, false.</returns>
        public static bool OrderMethodExists(Expression expression)
        {
            var visitor = new OrderingMethodFinder();
            _ = visitor.Visit(expression);
            return visitor.OrderingMethodFound;
        }
    }
}



