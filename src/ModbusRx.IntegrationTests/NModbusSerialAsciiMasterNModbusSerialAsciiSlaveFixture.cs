// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if SERIAL
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusSerialAsciiMasterNModbusSerialAsciiSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusSerialMasterFixture" />
public class NModbusSerialAsciiMasterNModbusSerialAsciiSlaveFixture : ModbusSerialMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NModbusSerialAsciiMasterNModbusSerialAsciiSlaveFixture"/> class.
    /// </summary>
    public NModbusSerialAsciiMasterNModbusSerialAsciiSlaveFixture()
    {
        MasterSerialPort = CreateAndOpenSerialPort(DefaultMasterSerialPortName);
        Master = ModbusSerialMaster.CreateAscii(MasterSerialPort);
        SetupSlaveSerialPort();
        Slave = ModbusSerialSlave.CreateAscii(SlaveAddress, SlaveSerialPort!);

        StartSlave();
    }

    /// <summary>
    /// Reads the coils.
    /// </summary>
    [Fact]
    public override void ReadCoils() =>
        base.ReadCoils();

    /// <summary>
    /// Reads the inputs.
    /// </summary>
    [Fact]
    public override void ReadInputs() =>
        base.ReadInputs();

    /// <summary>
    /// Reads the holding registers.
    /// </summary>
    [Fact]
    public override void ReadHoldingRegisters() =>
        base.ReadHoldingRegisters();

    /// <summary>
    /// Reads the input registers.
    /// </summary>
    [Fact]
    public override void ReadInputRegisters() =>
        base.ReadInputRegisters();

    /// <summary>
    /// Writes the single coil.
    /// </summary>
    [Fact]
    public override void WriteSingleCoil() =>
        base.WriteSingleCoil();

    /// <summary>
    /// Writes the multiple coils.
    /// </summary>
    [Fact]
    public override void WriteMultipleCoils() =>
        base.WriteMultipleCoils();

    /// <summary>
    /// Writes the single register.
    /// </summary>
    [Fact]
    public override void WriteSingleRegister() =>
        base.WriteSingleRegister();

    /// <summary>
    /// Writes the multiple registers.
    /// </summary>
    [Fact]
    public override void WriteMultipleRegisters() =>
        base.WriteMultipleRegisters();

    /// <summary>
    /// Reads the write multiple registers.
    /// </summary>
    [Fact]
    public override void ReadWriteMultipleRegisters() =>
        base.ReadWriteMultipleRegisters();

    /// <summary>
    /// Returns the query data.
    /// </summary>
    [Fact]
    public override void ReturnQueryData() =>
        base.ReturnQueryData();
}
#endif
