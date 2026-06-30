// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
using ModbusRx.Reactive.IO;
#else
using ModbusRx.IO;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Unme.Common;
#else
using ModbusRx.Unme.Common;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Device;
#else
namespace ModbusRx.Device;
#endif

/// <summary>Modbus device.</summary>
public abstract class ModbusDevice : ICancelable
{
    /// <summary>Stores the transport value.</summary>
    private ModbusTransport? _transport;

    /// <summary>Initializes a new instance of the Modbus Device class.</summary>
    /// <param name="transport">The transport value.</param>
    internal ModbusDevice(ModbusTransport transport) => _transport = transport;

    /// <summary>Gets the Modbus Transport.</summary>
    public ModbusTransport? Transport => _transport;

    /// <summary>Gets a value indicating whether gets a value that indicates whether the object is disposed.</summary>
    public bool IsDisposed { get; private set; }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources;
    ///     <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        DisposableUtility.Dispose(ref _transport);
        IsDisposed = true;
    }
}
