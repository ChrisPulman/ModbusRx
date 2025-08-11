// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusUdpMasterNModbusUdpSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusRxMasterFixture" />
public class ModbusRxUdpMasterModbusRxUdpSlaveFixture : ModbusRxMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusRxUdpMasterModbusRxUdpSlaveFixture"/> class.
    /// </summary>
    public ModbusRxUdpMasterModbusRxUdpSlaveFixture()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes the UDP connections asynchronously with CI-safe port allocation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task InitializeAsync()
    {
        // Use dynamic port allocation to avoid conflicts in CI
        var port = await GetAvailablePortAsync();
        
        SlaveUdp = new UdpClientRx(port);
        Slave = ModbusUdpSlave.CreateUdp(SlaveUdp);
        StartSlave();

        // Give slave time to start listening
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);

        MasterUdp = new UdpClientRx();
        var endPoint = new IPEndPoint(TcpHost, port);
        MasterUdp.Connect(endPoint);
        Master = ModbusIpMaster.CreateIp(MasterUdp);
    }
}
