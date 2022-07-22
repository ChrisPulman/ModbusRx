// <copyright file="NModbusTcpMasterNModbusTcpSlaveFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Net.Sockets;
using CP.IO.Ports;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusTcpMasterNModbusTcpSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusMasterFixture" />
public class NModbusTcpMasterNModbusTcpSlaveFixture : ModbusMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NModbusTcpMasterNModbusTcpSlaveFixture"/> class.
    /// </summary>
    public NModbusTcpMasterNModbusTcpSlaveFixture()
    {
        SlaveTcp = new TcpListener(TcpHost, Port);
        SlaveTcp.Start();
        Slave = ModbusTcpSlave.CreateTcp(SlaveAddress, SlaveTcp);
        StartSlave();

        MasterTcp = new TcpClientRx(TcpHost.ToString(), Port);
        Master = ModbusIpMaster.CreateIp(MasterTcp);
    }
}
