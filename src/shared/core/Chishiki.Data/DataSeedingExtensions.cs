// -----------------------------------------------------------------------------
// File:        DataSeedingExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for CSV-based data seeding operations.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Chishiki.Data;

/// <summary>
/// Provides extension methods for CSV-based data seeding operations.
/// </summary>
public static class DataSeedingExtensions
{
    /// <summary>
    /// Imports from CSV.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <typeparam name="T">Data type.</typeparam>
    /// <returns>File rows</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IList<T> ImportFromCsv<T>(this TextReader reader, string delimiter = ";")
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = delimiter
        };
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<T>();
        return [.. records];
    }

    /// <summary>
    /// Imports records from a CSV-formatted string.
    /// </summary>
    /// <param name="csvContent">CSV content as a string.</param>
    /// <param name="delimiter">Field delimiter. Default is semicolon.</param>
    /// <typeparam name="T">The type of records to deserialize.</typeparam>
    /// <returns>A list of deserialized records parsed from the CSV content.</returns>
    public static IList<T> ImportFromCsv<T>(this string csvContent, string delimiter = ";")
    {
        using var reader = new StringReader(csvContent);
        return reader.ImportFromCsv<T>(delimiter);
    }
}


