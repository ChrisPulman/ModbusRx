// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if SERIAL
using ModbusRx.Device;

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
    [TUnit.Core.Test]
    public override void ReadCoils() =>
        base.ReadCoils();
}
#endif
