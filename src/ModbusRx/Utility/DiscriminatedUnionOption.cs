// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Utility;
#else
namespace ModbusRx.Utility;
#endif

/// <summary>Possible options for <see cref="DiscriminatedUnion{TA, TB}"/>.</summary>
public enum DiscriminatedUnionOption
{
    /// <summary>Option A.</summary>
    A,

    /// <summary>Option B.</summary>
    B,
}
