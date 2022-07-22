// <copyright file="NModbusSerialRtuMasterDl06SlaveFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#if SERIAL
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusSerialRtuMasterDl06SlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusSerialMasterFixture" />
public class NModbusSerialRtuMasterDl06SlaveFixture : ModbusSerialMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NModbusSerialRtuMasterDl06SlaveFixture"/> class.
    /// </summary>
    public NModbusSerialRtuMasterDl06SlaveFixture()
    {
        MasterSerialPort = CreateAndOpenSerialPort("COM1");
        Master = ModbusSerialMaster.CreateRtu(MasterSerialPort);
    }

    /// <summary>
    /// Not supported by the DL06.
    /// </summary>
    public override void ReadWriteMultipleRegisters()
    {
    }

    /// <summary>
    /// Not supported by the DL06.
    /// </summary>
    public override void ReturnQueryData()
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
