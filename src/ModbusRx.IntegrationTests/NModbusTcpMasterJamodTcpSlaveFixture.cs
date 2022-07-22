// <copyright file="NModbusTcpMasterJamodTcpSlaveFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#if JAMOD
using System.Net.Sockets;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusTcpMasterJamodTcpSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusMasterFixture" />
public class NModbusTcpMasterJamodTcpSlaveFixture : ModbusMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NModbusTcpMasterJamodTcpSlaveFixture"/> class.
    /// </summary>
    public NModbusTcpMasterJamodTcpSlaveFixture()
    {
        var program = $"TcpSlave {Port}";
        StartJamodSlave(program);

        MasterTcp = new TcpClientRx(TcpHost.ToString(), Port);
        Master = ModbusIpMaster.CreateIp(MasterTcp);
    }

    /// <summary>
    /// Not supported by the Jamod Slave.
    /// </summary>
    public override void ReadWriteMultipleRegisters()
    {
    }
}
#endif
