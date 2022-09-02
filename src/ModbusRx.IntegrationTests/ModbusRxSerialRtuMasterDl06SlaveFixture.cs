// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if SERIAL
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// ModbusRxSerialRtuMasterDl06SlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusSerialMasterFixture" />
public class ModbusRxSerialRtuMasterDl06SlaveFixture : ModbusSerialMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusRxSerialRtuMasterDl06SlaveFixture"/> class.
    /// </summary>
    public ModbusRxSerialRtuMasterDl06SlaveFixture()
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
