// <copyright file="ModbusSlaveFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#if SERIAL
using System.IO.Ports;
#endif

using System.Linq;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Message;
using ModbusRx.UnitTests.Message;
using ModbusRx.Unme.Common;
using Xunit;

namespace ModbusRx.UnitTests.Device;

/// <summary>
/// ModbusSlaveFixture.
/// </summary>
public class ModbusSlaveFixture
{
    private readonly DataStore _testDataStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusSlaveFixture"/> class.
    /// </summary>
    public ModbusSlaveFixture() => _testDataStore = DataStoreFactory.CreateTestDataStore();

    /// <summary>
    /// Reads the discretes coils.
    /// </summary>
    [Fact]
    public void ReadDiscretesCoils()
    {
        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 2, new DiscreteCollection(false, true, false, true, false, true, false, true, false));
        var response =
            ModbusSlave.ReadDiscretes(new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 9), _testDataStore, _testDataStore.CoilDiscretes);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(expectedResponse.ByteCount, response.ByteCount);
    }

    /// <summary>
    /// Reads the discretes inputs.
    /// </summary>
    [Fact]
    public void ReadDiscretesInputs()
    {
        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadInputs, 1, 2, new DiscreteCollection(true, false, true, false, true, false, true, false, true));
        var response = ModbusSlave.ReadDiscretes(new ReadCoilsInputsRequest(Modbus.ReadInputs, 1, 1, 9), _testDataStore, _testDataStore.InputDiscretes);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(expectedResponse.ByteCount, response.ByteCount);
    }

    /// <summary>
    /// Reads the registers holding registers.
    /// </summary>
    [Fact]
    public void ReadRegistersHoldingRegisters()
    {
        var expectedResponse = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1, 2, 3, 4, 5, 6));
        var response = ModbusSlave.ReadRegisters(new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 0, 6), _testDataStore, _testDataStore.HoldingRegisters);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(expectedResponse.ByteCount, response.ByteCount);
    }

    /// <summary>
    /// Reads the registers input registers.
    /// </summary>
    [Fact]
    public void ReadRegistersInputRegisters()
    {
        var expectedResponse = new ReadHoldingInputRegistersResponse(Modbus.ReadInputRegisters, 1, new RegisterCollection(10, 20, 30, 40, 50, 60));
        var response = ModbusSlave.ReadRegisters(new ReadHoldingInputRegistersRequest(Modbus.ReadInputRegisters, 1, 0, 6), _testDataStore, _testDataStore.InputRegisters);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(expectedResponse.ByteCount, response.ByteCount);
    }

    /// <summary>
    /// Writes the single coil.
    /// </summary>
    [Fact]
    public void WriteSingleCoil()
    {
        const ushort addressToWrite = 35;
        var valueToWrite = !_testDataStore.CoilDiscretes[addressToWrite + 1];
        var expectedResponse = new WriteSingleCoilRequestResponse(1, addressToWrite, valueToWrite);
        var response = ModbusSlave.WriteSingleCoil(new WriteSingleCoilRequestResponse(1, addressToWrite, valueToWrite), _testDataStore, _testDataStore.CoilDiscretes);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(valueToWrite, _testDataStore.CoilDiscretes[addressToWrite + 1]);
    }

    /// <summary>
    /// Writes the multiple coils.
    /// </summary>
    [Fact]
    public void WriteMultipleCoils()
    {
        const ushort startAddress = 35;
        const ushort numberOfPoints = 10;
        var val = !_testDataStore.CoilDiscretes[startAddress + 1];
        var expectedResponse = new WriteMultipleCoilsResponse(1, startAddress, numberOfPoints);
        var response =
            ModbusSlave.WriteMultipleCoils(new WriteMultipleCoilsRequest(1, startAddress, new DiscreteCollection(val, val, val, val, val, val, val, val, val, val)), _testDataStore, _testDataStore.CoilDiscretes);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
        Assert.Equal(new bool[] { val, val, val, val, val, val, val, val, val, val }, _testDataStore.CoilDiscretes.Slice(startAddress + 1, numberOfPoints).ToArray());
    }

    /// <summary>
    /// Writes the single register.
    /// </summary>
    [Fact]
    public void WriteSingleRegister()
    {
        const ushort startAddress = 35;
        const ushort value = 45;
        Assert.NotEqual(value, _testDataStore.HoldingRegisters[startAddress - 1]);
        var expectedResponse = new WriteSingleRegisterRequestResponse(1, startAddress, value);
        var response = ModbusSlave.WriteSingleRegister(new WriteSingleRegisterRequestResponse(1, startAddress, value), _testDataStore, _testDataStore.HoldingRegisters);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
    }

    /// <summary>
    /// Writes the multiple registers.
    /// </summary>
    [Fact]
    public void WriteMultipleRegisters()
    {
        const ushort startAddress = 35;
        var valuesToWrite = new ushort[] { 1, 2, 3, 4, 5 };
        Assert.NotEqual(valuesToWrite, _testDataStore.HoldingRegisters.Slice(startAddress - 1, valuesToWrite.Length).ToArray());
        var expectedResponse = new WriteMultipleRegistersResponse(1, startAddress, (ushort)valuesToWrite.Length);
        var response = ModbusSlave.WriteMultipleRegisters(new WriteMultipleRegistersRequest(1, startAddress, new RegisterCollection(valuesToWrite)), _testDataStore, _testDataStore.HoldingRegisters);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
    }

#if SERIAL
    /// <summary>
    /// ApplyRequest_VerifyModbusRequestReceivedEventIsFired.
    /// </summary>
    [Fact]
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

    /// <summary>
    /// Writes the multip coils make sure we do not write remainder.
    /// </summary>
    [Fact]
    public void WriteMultipCoils_MakeSureWeDoNotWriteRemainder()
    {
        // 0, false initialized data store
        var dataStore = DataStoreFactory.CreateDefaultDataStore();

        var request = new WriteMultipleCoilsRequest(1, 0, new DiscreteCollection(Enumerable.Repeat(true, 8).ToArray()))
        { NumberOfPoints = 2 };
        ModbusSlave.WriteMultipleCoils(request, dataStore, dataStore.CoilDiscretes);

        Assert.Equal(dataStore.CoilDiscretes.Slice(1, 8).ToArray(), new[] { true, true, false, false, false, false, false, false });
    }
}
