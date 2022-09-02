// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if JAMOD
using ModbusRx.Device;
using Xunit;

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
    [Fact]
    public override void ReadCoils() =>
        base.ReadCoils();
}
#endif
