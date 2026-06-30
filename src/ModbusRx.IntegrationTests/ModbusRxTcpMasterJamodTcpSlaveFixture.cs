// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if JAMOD
using System.Net.Sockets;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusTcpMasterJamodTcpSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusMasterFixture" />
public class ModbusRxTcpMasterJamodTcpSlaveFixture : ModbusMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusRxTcpMasterJamodTcpSlaveFixture"/> class.
    /// </summary>
    public ModbusRxTcpMasterJamodTcpSlaveFixture()
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
