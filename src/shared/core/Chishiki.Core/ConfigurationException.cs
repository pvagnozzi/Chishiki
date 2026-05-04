// -----------------------------------------------------------------------------
// File:        ConfigurationException.cs
// Author:      Piergiorgio Vagnozzi
// Description: Exception thrown when configuration is missing, invalid, or cannot be loaded.
// Created:     2024-11-19
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Core;

/// <summary>Exception thrown when configuration is missing, invalid, or cannot be loaded.</summary>
/// <param name="message">The exception message describing the configuration issue.</param>
/// <param name="innerException">Optional inner exception that caused this configuration failure.</param>
public class ConfigurationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
