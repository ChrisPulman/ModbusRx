// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Data;
#else
namespace ModbusRx.Data;
#endif

/// <summary>Modbus message containing data.</summary>
public interface IDataCollection
{
    /// <summary>Gets the network bytes.</summary>
    byte[] NetworkBytes { get; }

    /// <summary>Gets the byte count.</summary>
    byte ByteCount { get; }
}
