// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Data;
#else
namespace ModbusRx.Data;
#endif

/// <summary>Boolean pattern types.</summary>
public enum BooleanPattern
{
    /// <summary>All true values.</summary>
    AllTrue,

    /// <summary>All false values.</summary>
    AllFalse,

    /// <summary>Alternating true/false pattern.</summary>
    Alternating,

    /// <summary>Random true/false values.</summary>
    Random
}
