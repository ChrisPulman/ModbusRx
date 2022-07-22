// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.IO;

namespace ModbusRx.Device;

/// <summary>
///     Modbus Serial Master device.
/// </summary>
public interface IModbusSerialMaster : IModbusMaster
{
    /// <summary>
    ///     Gets transport for used by this master.
    /// </summary>
    new ModbusSerialTransport? Transport { get; }

    /// <summary>
    ///     Serial Line only.
    ///     Diagnostic function which loops back the original data.
    ///     NModbus only supports looping back one ushort value, this is a
    ///     limitation of the "Best Effort" implementation of the RTU protocol.
    /// </summary>
    /// <param name="slaveAddress">Address of device to test.</param>
    /// <param name="data">Data to return.</param>
    /// <returns>Return true if slave device echoed data.</returns>
    bool ReturnQueryData(byte slaveAddress, ushort data);
}
