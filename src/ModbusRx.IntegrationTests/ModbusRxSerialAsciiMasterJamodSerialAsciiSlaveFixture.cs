// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if JAMOD
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// ModbusRxSerialAsciiMasterJamodSerialAsciiSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusMasterFixture" />
public class ModbusRxSerialAsciiMasterJamodSerialAsciiSlaveFixture : ModbusMasterFixture
{
    private const string Program = $"SerialSlave {DefaultSlaveSerialPortName} ASCII";

    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusRxSerialAsciiMasterJamodSerialAsciiSlaveFixture"/> class.
    /// </summary>
    public ModbusRxSerialAsciiMasterJamodSerialAsciiSlaveFixture()
    {
        StartJamodSlave(Program);

        MasterSerialPort = CreateAndOpenSerialPort(DefaultMasterSerialPortName);
        Master = ModbusSerialMaster.CreateAscii(MasterSerialPort);
    }

    /// <summary>
    /// Jamod slave does not support this function.
    /// </summary>
    public override void ReadWriteMultipleRegisters()
    {
    }

    /// <summary>
    /// Reads the coils.
    /// </summary>
    [TUnit.Core.Test]
    public override void ReadCoils() =>
        base.ReadCoils();
}
#endif
