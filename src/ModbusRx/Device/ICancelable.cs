// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Device;
#else
namespace ModbusRx.Device;
#endif

/// <summary>Represents a disposable resource whose disposed state can be inspected.</summary>
public interface ICancelable : IDisposable
{
    /// <summary>Gets a value indicating whether the resource has been disposed.</summary>
    bool IsDisposed { get; }
}
