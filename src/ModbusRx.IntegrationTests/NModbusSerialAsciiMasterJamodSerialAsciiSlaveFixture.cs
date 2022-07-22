// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if JAMOD
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusSerialAsciiMasterJamodSerialAsciiSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusMasterFixture" />
public class NModbusSerialAsciiMasterJamodSerialAsciiSlaveFixture : ModbusMasterFixture
{
    private const string Program = $"SerialSlave {DefaultSlaveSerialPortName} ASCII";

    /// <summary>
    /// Initializes a new instance of the <see cref="NModbusSerialAsciiMasterJamodSerialAsciiSlaveFixture"/> class.
    /// </summary>
    public NModbusSerialAsciiMasterJamodSerialAsciiSlaveFixture()
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
