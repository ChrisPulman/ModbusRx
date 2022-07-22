// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Sockets;
using CP.IO.Ports;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusUdpMasterNModbusUdpSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusMasterFixture" />
public class NModbusUdpMasterNModbusUdpSlaveFixture : ModbusMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NModbusUdpMasterNModbusUdpSlaveFixture"/> class.
    /// </summary>
    public NModbusUdpMasterNModbusUdpSlaveFixture()
    {
        SlaveUdp = new UdpClientRx(Port);
        Slave = ModbusUdpSlave.CreateUdp(SlaveUdp);
        StartSlave();

        MasterUdp = new UdpClientRx();
        MasterUdp.Connect(DefaultModbusIPEndPoint);
        Master = ModbusIpMaster.CreateIp(MasterUdp);
    }
}
