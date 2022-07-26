﻿// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if SERIAL
using System;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusSerialAsciiMasterFixture.
/// </summary>
public class ModbusRxSerialAsciiMasterFixture
{
    /// <summary>
    /// ns the modbus ASCII master read timeout.
    /// </summary>
    [Fact]
    public void ModbusRxAsciiMaster_ReadTimeout()
    {
        var port = ModbusMasterFixture.CreateAndOpenSerialPort(ModbusMasterFixture.DefaultMasterSerialPortName);
        using IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(port);
        master.Transport.ReadTimeout = master.Transport.WriteTimeout = 1000;
        Assert.Throws<TimeoutException>(() => master.ReadCoils(100, 1, 1));
    }
}
#endif
