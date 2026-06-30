// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if SERIAL
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusSerialRtuMasterNModbusSerialRtuSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusSerialMasterFixture" />
public class NModbusSerialRtuMasterNModbusSerialRtuSlaveFixture : ModbusSerialMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NModbusSerialRtuMasterNModbusSerialRtuSlaveFixture"/> class.
    /// </summary>
    public NModbusSerialRtuMasterNModbusSerialRtuSlaveFixture()
    {
        SetupSlaveSerialPort();
        Slave = ModbusSerialSlave.CreateRtu(SlaveAddress, SlaveSerialPort!);
        StartSlave();

        MasterSerialPort = CreateAndOpenSerialPort(DefaultMasterSerialPortName);
        Master = ModbusSerialMaster.CreateRtu(MasterSerialPort);
    }

    /// <summary>
    /// Reads the coils.
    /// </summary>
    [TUnit.Core.Test]
    public override void ReadCoils() =>
        base.ReadCoils();

    /// <summary>
    /// Reads the holding registers.
    /// </summary>
    [TUnit.Core.Test]
    public override void ReadHoldingRegisters() =>
        base.ReadHoldingRegisters();

    /// <summary>
    /// Reads the inputs.
    /// </summary>
    [TUnit.Core.Test]
    public override void ReadInputs() =>
        base.ReadInputs();

    /// <summary>
    /// Writes the single coil.
    /// </summary>
    [TUnit.Core.Test]
    public override void WriteSingleCoil() =>
        base.WriteSingleCoil();

    /// <summary>
    /// Writes the multiple coils.
    /// </summary>
    [TUnit.Core.Test]
    public override void WriteMultipleCoils() =>
        base.WriteMultipleCoils();

    /// <summary>
    /// Writes the single register.
    /// </summary>
    [TUnit.Core.Test]
    public override void WriteSingleRegister() =>
        base.WriteSingleRegister();

    /// <summary>
    /// Writes the multiple registers.
    /// </summary>
    [TUnit.Core.Test]
    public override void WriteMultipleRegisters() =>
        base.WriteMultipleRegisters();

    /// <summary>
    /// Reads the write multiple registers.
    /// </summary>
    [TUnit.Core.Test, TUnit.Core.Skip("Need to fix RTU slave for this function code")]
    public override void ReadWriteMultipleRegisters() =>
        base.ReadWriteMultipleRegisters();

    /// <summary>
    /// Returns the query data.
    /// </summary>
    [TUnit.Core.Test]
    public override void ReturnQueryData() =>
        base.ReturnQueryData();
}
#endif
