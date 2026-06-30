// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>Tests the NModbusSerialAsciiMasterFixture behavior.</summary>
public class ModbusRxSerialAsciiMasterFixture : NetworkTestBase
{
    /// <summary>Tests the modbus ASCII master read timeout.</summary>
    [TUnit.Core.Test]
    public void ModbusRxAsciiMaster_ReadTimeout()
    {
        // Skip this test in CI environments as serial ports are not available
        Skip.IfNot(!IsRunningInCI, "Serial port tests require physical hardware not available in CI");

#if SERIAL
        var port = ModbusRxMasterFixture.CreateAndOpenSerialPort(ModbusRxMasterFixture.DefaultMasterSerialPortName);
        RegisterDisposable(port);
        
        using IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(port);
        master.Transport!.ReadTimeout = master.Transport.WriteTimeout = 1000;
        Assert.Throws<TimeoutException>(() => master.ReadCoils(100, 1, 1));
#else
        // When SERIAL symbol is not defined, skip with explanation
        Skip.If(true, "SERIAL conditional compilation symbol not defined");
#endif
    }
}
