// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using ModbusRx.IO;
using ModbusRx.Unme.Common;

namespace ModbusRx.Device;

/// <summary>
///     Modbus device.
/// </summary>
public abstract class ModbusDevice : ICancelable
{
    private ModbusTransport? _transport;

    internal ModbusDevice(ModbusTransport transport) => _transport = transport;

    /// <summary>
    ///     Gets the Modbus Transport.
    /// </summary>
    public ModbusTransport? Transport => _transport;

    /// <summary>
    /// Gets a value indicating whether gets a value that indicates whether the object is disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    ///     Releases unmanaged and - optionally - managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources;
    ///     <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposableUtility.Dispose(ref _transport);
            IsDisposed = true;
        }
    }
}
