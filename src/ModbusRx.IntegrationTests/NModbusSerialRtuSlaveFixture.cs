// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if SERIAL

using System.Threading;
using ModbusRx.Data;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusSerialRtuSlaveFixture.
/// </summary>
public class NModbusSerialRtuSlaveFixture
{
    /// <summary>
    /// ns the modbus serial rtu slave bonus character verify timeout.
    /// </summary>
    [Fact]
    public void NModbusSerialRtuSlave_BonusCharacter_VerifyTimeout()
    {
        var masterPort = ModbusMasterFixture.CreateAndOpenSerialPort(ModbusMasterFixture.DefaultMasterSerialPortName);
        var slavePort = ModbusMasterFixture.CreateAndOpenSerialPort(ModbusMasterFixture.DefaultSlaveSerialPortName);

        using var master = ModbusSerialMaster.CreateRtu(masterPort);
        using var slave = ModbusSerialSlave.CreateRtu(1, slavePort);
        master.Transport.ReadTimeout = master.Transport.WriteTimeout = 1000;
        slave.DataStore = DataStoreFactory.CreateTestDataStore();

        var slaveThread = new Thread(async () => await slave.ListenAsync())
        {
            IsBackground = true
        };
        slaveThread.Start();

        // assert successful communication
        Assert.Equal(new bool[] { false, true }, master.ReadCoils(1, 1, 2));

        // write "bonus" character
        masterPort.Write("*");

        // assert successful communication
        Assert.Equal(new bool[] { false, true }, master.ReadCoils(1, 1, 2));
    }
}
#endif
