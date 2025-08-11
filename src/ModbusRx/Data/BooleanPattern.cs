// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Data;

/// <summary>
/// Boolean pattern types.
/// </summary>
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
