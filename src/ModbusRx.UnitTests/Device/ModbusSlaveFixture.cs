// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if SERIAL
using System.IO.Ports;
#endif

using System.Collections.Generic;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Message;
using ModbusRx.UnitTests.Message;
using ModbusRx.Unme.Common;

namespace ModbusRx.UnitTests.Device;

/// <summary>Tests the ModbusSlaveFixture behavior.</summary>
public class ModbusSlaveFixture
{
    /// <summary>The test data store used by slave operation tests.</summary>
    private readonly DataStore _testDataStore;

    /// <summary>Initializes a new instance of the <see cref="ModbusSlaveFixture"/> class.</summary>
    public ModbusSlaveFixture() => _testDataStore = DataStoreFactory.CreateTestDataStore();

    /// <summary>Reads the discretes coils.</summary>
    [TUnit.Core.Test]
    public void ReadDiscretesCoils()
    {
        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 2, new DiscreteCollection(false, true, false, true, false, true, false, true, false));
        var response =
            ModbusSlave.ReadDiscretes(new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 9), _testDataStore, _testDataStore.CoilDiscretes);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(expectedResponse.ByteCount, response.ByteCount);
    }

    /// <summary>Reads the discretes inputs.</summary>
    [TUnit.Core.Test]
    public void ReadDiscretesInputs()
    {
        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadInputs, 1, 2, new DiscreteCollection(true, false, true, false, true, false, true, false, true));
        var response = ModbusSlave.ReadDiscretes(new ReadCoilsInputsRequest(Modbus.ReadInputs, 1, 1, 9), _testDataStore, _testDataStore.InputDiscretes);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(expectedResponse.ByteCount, response.ByteCount);
    }

    /// <summary>Reads the registers holding registers.</summary>
    [TUnit.Core.Test]
    public void ReadRegistersHoldingRegisters()
    {
        var expectedResponse = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1, 2, 3, 4, 5, 6));
        var response = ModbusSlave.ReadRegisters(new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 0, 6), _testDataStore, _testDataStore.HoldingRegisters);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(expectedResponse.ByteCount, response.ByteCount);
    }

    /// <summary>Reads the registers input registers.</summary>
    [TUnit.Core.Test]
    public void ReadRegistersInputRegisters()
    {
        var expectedResponse = new ReadHoldingInputRegistersResponse(Modbus.ReadInputRegisters, 1, new RegisterCollection(10, 20, 30, 40, 50, 60));
        var response = ModbusSlave.ReadRegisters(new ReadHoldingInputRegistersRequest(Modbus.ReadInputRegisters, 1, 0, 6), _testDataStore, _testDataStore.InputRegisters);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(expectedResponse.ByteCount, response.ByteCount);
    }

    /// <summary>Writes the single coil.</summary>
    [TUnit.Core.Test]
    public void WriteSingleCoil()
    {
        const ushort addressToWrite = 35;
        var valueToWrite = !_testDataStore.CoilDiscretes[addressToWrite + 1];
        var expectedResponse = new WriteSingleCoilRequestResponse(1, addressToWrite, valueToWrite);
        var response = ModbusSlave.WriteSingleCoil(new WriteSingleCoilRequestResponse(1, addressToWrite, valueToWrite), _testDataStore, _testDataStore.CoilDiscretes);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(valueToWrite, _testDataStore.CoilDiscretes[addressToWrite + 1]);
    }

    /// <summary>Writes the multiple coils.</summary>
    [TUnit.Core.Test]
    public void WriteMultipleCoils()
    {
        const ushort startAddress = 35;
        const ushort numberOfPoints = 10;
        var val = !_testDataStore.CoilDiscretes[startAddress + 1];
        var expectedResponse = new WriteMultipleCoilsResponse(1, startAddress, numberOfPoints);
        var response =
            ModbusSlave.WriteMultipleCoils(new WriteMultipleCoilsRequest(1, startAddress, new DiscreteCollection(val, val, val, val, val, val, val, val, val, val)), _testDataStore, _testDataStore.CoilDiscretes);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal<IEnumerable<bool>>([val, val, val, val, val, val, val, val, val, val], _testDataStore.CoilDiscretes.Slice(startAddress + 1, numberOfPoints));
    }

    /// <summary>Writes the single register.</summary>
    [TUnit.Core.Test]
    public void WriteSingleRegister()
    {
        const ushort startAddress = 35;
        const ushort value = 45;
        Assert.NotEqual(value, _testDataStore.HoldingRegisters[startAddress - 1]);
        var expectedResponse = new WriteSingleRegisterRequestResponse(1, startAddress, value);
        var response = ModbusSlave.WriteSingleRegister(new WriteSingleRegisterRequestResponse(1, startAddress, value), _testDataStore, _testDataStore.HoldingRegisters);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
    }

    /// <summary>Writes the multiple registers.</summary>
    [TUnit.Core.Test]
    public void WriteMultipleRegisters()
    {
        const ushort startAddress = 35;
        ushort[] valuesToWrite = [1, 2, 3, 4, 5];
        Assert.NotEqual<IEnumerable<ushort>>(valuesToWrite, _testDataStore.HoldingRegisters.Slice(startAddress - 1, valuesToWrite.Length));
        var expectedResponse = new WriteMultipleRegistersResponse(1, startAddress, (ushort)valuesToWrite.Length);
        var response = ModbusSlave.WriteMultipleRegisters(new WriteMultipleRegistersRequest(1, startAddress, new RegisterCollection(valuesToWrite)), _testDataStore, _testDataStore.HoldingRegisters);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
    }

#if SERIAL
    /// <summary>
    /// ApplyRequest_VerifyModbusRequestReceivedEventIsFired.
    /// </summary>
    [TUnit.Core.Test]
    public void ApplyRequest_VerifyModbusRequestReceivedEventIsFired()
    {
        var eventFired = false;
        ModbusSlave slave = ModbusSerialSlave.CreateAscii(1, new SerialPort());
        var request = new WriteSingleRegisterRequestResponse(1, 1, 1);
        slave.ModbusSlaveRequestReceived += (obj, args) =>
        {
            eventFired = true;
            Assert.Equal(request, args.Message);
        };

        slave.ApplyRequest(request);
        Assert.True(eventFired);
    }
#endif

    /// <summary>Writes the multip coils make sure we do not write remainder.</summary>
    [TUnit.Core.Test]
    public void WriteMultipCoils_MakeSureWeDoNotWriteRemainder()
    {
        // 0, false initialized data store
        var dataStore = DataStoreFactory.CreateDefaultDataStore();

        var request = new WriteMultipleCoilsRequest(1, 0, new DiscreteCollection(CreateCoils(8, true)))
        { NumberOfPoints = 2 };
        _ = ModbusSlave.WriteMultipleCoils(request, dataStore, dataStore.CoilDiscretes);

        Assert.Equal<IEnumerable<bool>>([true, true, false, false, false, false, false, false], dataStore.CoilDiscretes.Slice(1, 8));
    }

    /// <summary>Creates a coil buffer filled with one value.</summary>
    /// <param name="count">The number of coils to create.</param>
    /// <param name="value">The coil value.</param>
    /// <returns>The populated coil buffer.</returns>
    private static bool[] CreateCoils(int count, bool value)
    {
        var result = new bool[count];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = value;
        }

        return result;
    }
}
