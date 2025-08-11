// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// ModbusRxSerialRtuSlaveFixture.
/// </summary>
[Collection("NetworkTests")]
public class ModbusRxSerialRtuSlaveFixture : NetworkTestBase
{
    /// <summary>
    /// Tests the modbus serial rtu slave bonus character verify timeout.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [SkippableFact]
    public async Task ModbusRxSerialRtuSlave_BonusCharacter_VerifyTimeout()
    {
        // Skip this test in CI environments as serial ports are not available
        Skip.IfNot(!IsRunningInCI, "Serial port tests require physical hardware not available in CI");

#if SERIAL
        var masterPort = ModbusRxMasterFixture.CreateAndOpenSerialPort(ModbusRxMasterFixture.DefaultMasterSerialPortName);
        var slavePort = ModbusRxMasterFixture.CreateAndOpenSerialPort(ModbusRxMasterFixture.DefaultSlaveSerialPortName);
        RegisterDisposable(masterPort);
        RegisterDisposable(slavePort);

        using var master = ModbusSerialMaster.CreateRtu(masterPort);
        using var slave = ModbusSerialSlave.CreateRtu(1, slavePort);
        master.Transport!.ReadTimeout = master.Transport.WriteTimeout = 1000;
        slave.DataStore = DataStoreFactory.CreateTestDataStore();

        var slaveTask = Task.Run(async () =>
        {
            try
            {
                await slave.ListenAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (ObjectDisposedException)
            {
                // Expected when disposed
            }
        });

        // Give slave time to start
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);

        // Assert successful communication
        Assert.Equal(new bool[] { false, true }, await master.ReadCoilsAsync(1, 1, 2));

        // Write "bonus" character
        masterPort.Write("*");

        // Assert successful communication
        Assert.Equal(new bool[] { false, true }, await master.ReadCoilsAsync(1, 1, 2));
#else
        // When SERIAL symbol is not defined, skip with explanation
        Skip.If(true, "SERIAL conditional compilation symbol not defined");
#endif
    }
}
