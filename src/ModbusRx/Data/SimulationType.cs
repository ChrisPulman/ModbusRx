// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Data;

/// <summary>
/// Types of simulation patterns available.
/// </summary>
public enum SimulationType
{
    /// <summary>Random values.</summary>
    Random,

    /// <summary>Counting up pattern.</summary>
    CountingUp,

    /// <summary>Counting down pattern.</summary>
    CountingDown,

    /// <summary>Sine wave pattern.</summary>
    SineWave,

    /// <summary>Square wave pattern.</summary>
    SquareWave
}
