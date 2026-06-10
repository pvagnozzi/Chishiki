// -----------------------------------------------------------------------------
// File:        InternalsVisibleTo.cs
// Author:      Piergiorgio Vagnozzi
// Description: Exposes internal ESPTool types to the focused test assembly.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Chishiki.ESPTool.Tests")]
