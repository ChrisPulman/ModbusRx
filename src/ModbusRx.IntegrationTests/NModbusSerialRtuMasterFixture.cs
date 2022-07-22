// <copyright file="NModbusSerialRtuMasterFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#if SERIAL
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusSerialRtuMasterFixture.
/// </summary>
public class NModbusSerialRtuMasterFixture
{
    /// <summary>
    /// ns the modbus rtu master read timeout.
    /// </summary>
    [Fact]
    public void NModbusRtuMaster_ReadTimeout()
    {
        var port = ModbusMasterFixture.CreateAndOpenSerialPort(ModbusMasterFixture.DefaultMasterSerialPortName);
        using var master = ModbusSerialMaster.CreateRtu(port);
        master.Transport.ReadTimeout = master.Transport.WriteTimeout = 1000;
        master.ReadCoils(100, 1, 1);
    }
}
#endif
