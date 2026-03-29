// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Security.Contracts;

/// <summary>Classifies the severity level of a security finding.</summary>
public enum Severity
{
    /// <summary>Informational — no immediate risk; used for context and awareness.</summary>
    Info,

    /// <summary>Low severity — minimal exploitability or impact.</summary>
    Low,

    /// <summary>Medium severity — moderate risk that should be addressed in a standard release cycle.</summary>
    Medium,

    /// <summary>High severity — significant risk requiring prompt remediation.</summary>
    High,

    /// <summary>Critical severity — immediate exploitation risk; must be fixed urgently.</summary>
    Critical
}
