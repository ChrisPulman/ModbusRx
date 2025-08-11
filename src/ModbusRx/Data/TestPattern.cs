// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Data;

/// <summary>
/// Test pattern types.
/// </summary>
public enum TestPattern
{
    /// <summary>Counting up from 0.</summary>
    CountingUp,

    /// <summary>Counting down to 0.</summary>
    CountingDown,

    /// <summary>Sine wave pattern.</summary>
    SineWave,

    /// <summary>Square wave pattern.</summary>
    SquareWave,

    /// <summary>Random values.</summary>
    Random,

    /// <summary>All zeros.</summary>
    AllZeros,

    /// <summary>All ones (max values).</summary>
    AllOnes
}
