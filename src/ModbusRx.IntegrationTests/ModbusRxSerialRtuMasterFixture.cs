// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// ModbusRxSerialRtuMasterFixture.
/// </summary>
[Collection("NetworkTests")]
public class ModbusRxSerialRtuMasterFixture : NetworkTestBase
{
    /// <summary>
    /// Tests the modbus RTU master read timeout.
    /// </summary>
    [SkippableFact]
    public void ModbusRxRtuMaster_ReadTimeout()
    {
        // Skip this test in CI environments as serial ports are not available
        Skip.IfNot(!IsRunningInCI, "Serial port tests require physical hardware not available in CI");

#if SERIAL
        var port = ModbusRxMasterFixture.CreateAndOpenSerialPort(ModbusRxMasterFixture.DefaultMasterSerialPortName);
        RegisterDisposable(port);

        using var master = ModbusSerialMaster.CreateRtu(port);
        master.Transport!.ReadTimeout = master.Transport.WriteTimeout = 1000;
        master.ReadCoils(100, 1, 1);
#else
        // When SERIAL symbol is not defined, skip with explanation
        Skip.If(true, "SERIAL conditional compilation symbol not defined");
#endif
    }
}
