// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
using ModbusRx.Reactive.IO;
#else
using ModbusRx.IO;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Device;
#else
namespace ModbusRx.Device;
#endif

/// <summary>Modbus Serial Master device.</summary>
public interface IModbusSerialMaster : IModbusMaster
{
    /// <summary>Gets transport for used by this master.</summary>
    new ModbusSerialTransport? Transport { get; }

    /// <summary>Performs the serial-line return query diagnostic and verifies the echoed data.</summary>
    /// <param name="slaveAddress">Address of device to test.</param>
    /// <param name="data">Data to return.</param>
    /// <returns>Return true if slave device echoed data.</returns>
    bool ReturnQueryData(byte slaveAddress, ushort data);
}
