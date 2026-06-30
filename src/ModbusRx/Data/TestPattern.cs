// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Data;
#else
namespace ModbusRx.Data;
#endif

/// <summary>Test pattern types.</summary>
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
