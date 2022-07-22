// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using ModbusRx.Data;
using Xunit;

namespace ModbusRx.UnitTests.Data;

/// <summary>
/// DataStoreFixture.
/// </summary>
public class DataStoreFixture
{
    /// <summary>
    /// Reads the data.
    /// </summary>
    [Fact]
    public void ReadData()
    {
        var slaveCol = new ModbusDataCollection<ushort>(0, 1, 2, 3, 4, 5, 6);
        var result = DataStore.ReadData<RegisterCollection, ushort>(new DataStore(), slaveCol, 1, 3, new object());
        Assert.Equal(new ushort[] { 1, 2, 3 }, result.ToArray());
    }

    /// <summary>
    /// Reads the data start address too large.
    /// </summary>
    [Fact]
    public void ReadDataStartAddressTooLarge() =>
        Assert.Throws<InvalidModbusRequestException>(() => DataStore.ReadData<DiscreteCollection, bool>(new DataStore(), new ModbusDataCollection<bool>(), 3, 2, new object()));

    /// <summary>
    /// Reads the data count too large.
    /// </summary>
    [Fact]
    public void ReadDataCountTooLarge() =>
        Assert.Throws<InvalidModbusRequestException>(() => DataStore.ReadData<DiscreteCollection, bool>(new DataStore(), new ModbusDataCollection<bool>(true, false, true, true), 1, 5, new object()));

    /// <summary>
    /// Reads the data start address zero.
    /// </summary>
    [Fact]
    public void ReadDataStartAddressZero() =>
        DataStore.ReadData<DiscreteCollection, bool>(new DataStore(), new ModbusDataCollection<bool>(true, false, true, true, true, true), 0, 5, new object());

    /// <summary>
    /// Writes the data single.
    /// </summary>
    [Fact]
    public void WriteDataSingle()
    {
        var destination = new ModbusDataCollection<bool>(true, true);
        var newValues = new DiscreteCollection(false);
        DataStore.WriteData(new DataStore(), newValues, destination, 0, new object());
        Assert.False(destination[1]);
    }

    /// <summary>
    /// Writes the data multiple.
    /// </summary>
    [Fact]
    public void WriteDataMultiple()
    {
        var destination = new ModbusDataCollection<bool>(false, false, false, false, false, false, true);
        var newValues = new DiscreteCollection(true, true, true, true);
        DataStore.WriteData(new DataStore(), newValues, destination, 0, new object());
        Assert.Equal(new bool[] { false, true, true, true, true, false, false, true }, destination.ToArray());
    }

    /// <summary>
    /// Writes the data too large.
    /// </summary>
    [Fact]
    public void WriteDataTooLarge()
    {
        var slaveCol = new ModbusDataCollection<bool>(true);
        var newValues = new DiscreteCollection(false, false);
        Assert.Throws<InvalidModbusRequestException>(() => DataStore.WriteData(new DataStore(), newValues, slaveCol, 1, new object()));
    }

    /// <summary>
    /// Writes the data start address zero.
    /// </summary>
    [Fact]
    public void WriteDataStartAddressZero() =>
        DataStore.WriteData(new DataStore(), new DiscreteCollection(false),            new ModbusDataCollection<bool>(true, true), 0, new object());

    /// <summary>
    /// Writes the data start address too large.
    /// </summary>
    [Fact]
    public void WriteDataStartAddressTooLarge() =>
        Assert.Throws<InvalidModbusRequestException>(() =>
        DataStore.WriteData(new DataStore(), new DiscreteCollection(true), new ModbusDataCollection<bool>(true), 2, new object()));

    /// <summary>
    /// http://modbus.org/docs/Modbus_Application_Protocol_V1_1b.pdf
    /// In the PDU Coils are addressed starting at zero. Therefore coils numbered 1-16 are addressed as 0-15.
    /// So reading Modbus address 0 should get you array index 1 in the DataStore.
    /// This implies that the DataStore array index 0 can never be used.
    /// </summary>
    [Fact]
    public void TestReadMapping()
    {
        var dataStore = DataStoreFactory.CreateDefaultDataStore();
        dataStore.HoldingRegisters.Insert(1, 45);
        dataStore.HoldingRegisters.Insert(2, 42);

        Assert.Equal(45, DataStore.ReadData<RegisterCollection, ushort>(dataStore, dataStore.HoldingRegisters, 0, 1, new object())[0]);
        Assert.Equal(42, DataStore.ReadData<RegisterCollection, ushort>(dataStore, dataStore.HoldingRegisters, 1, 1, new object())[0]);
    }

    /// <summary>
    /// Datas the store read from event read holding registers.
    /// </summary>
    [Fact]
    public void DataStoreReadFromEvent_ReadHoldingRegisters()
    {
        var dataStore = DataStoreFactory.CreateTestDataStore();

        var readFromEventFired = false;
        var writtenToEventFired = false;

        dataStore.DataStoreReadFrom += (obj, e) =>
        {
            readFromEventFired = true;
            Assert.Equal(3, e.StartAddress);
            Assert.Equal(new ushort[] { 4, 5, 6 }, e.Data?.B?.ToArray());
            Assert.Equal(ModbusDataType.HoldingRegister, e.ModbusDataType);
        };

        dataStore.DataStoreWrittenTo += (obj, e) => writtenToEventFired = true;

        DataStore.ReadData<RegisterCollection, ushort>(dataStore, dataStore.HoldingRegisters, 3, 3, new object());
        Assert.True(readFromEventFired);
        Assert.False(writtenToEventFired);
    }

    /// <summary>
    /// Datas the store read from event read input registers.
    /// </summary>
    [Fact]
    public void DataStoreReadFromEvent_ReadInputRegisters()
    {
        var dataStore = DataStoreFactory.CreateTestDataStore();

        var readFromEventFired = false;
        var writtenToEventFired = false;

        dataStore.DataStoreReadFrom += (obj, e) =>
        {
            readFromEventFired = true;
            Assert.Equal(4, e.StartAddress);
            Assert.Equal(System.Array.Empty<ushort>(), e.Data?.B?.ToArray());
            Assert.Equal(ModbusDataType.InputRegister, e.ModbusDataType);
        };

        dataStore.DataStoreWrittenTo += (obj, e) => writtenToEventFired = true;

        DataStore.ReadData<RegisterCollection, ushort>(dataStore, dataStore.InputRegisters, 4, 0, new object());
        Assert.True(readFromEventFired);
        Assert.False(writtenToEventFired);
    }

    /// <summary>
    /// Datas the store read from event read inputs.
    /// </summary>
    [Fact]
    public void DataStoreReadFromEvent_ReadInputs()
    {
        var dataStore = DataStoreFactory.CreateTestDataStore();

        var readFromEventFired = false;
        var writtenToEventFired = false;

        dataStore.DataStoreReadFrom += (obj, e) =>
        {
            readFromEventFired = true;
            Assert.Equal(4, e.StartAddress);
            Assert.Equal(new bool[] { false }, e.Data?.A?.ToArray());
            Assert.Equal(ModbusDataType.Input, e.ModbusDataType);
        };

        dataStore.DataStoreWrittenTo += (obj, e) => writtenToEventFired = true;

        DataStore.ReadData<DiscreteCollection, bool>(dataStore, dataStore.InputDiscretes, 4, 1, new object());
        Assert.True(readFromEventFired);
        Assert.False(writtenToEventFired);
    }

    /// <summary>
    /// Datas the store written to event write coils.
    /// </summary>
    [Fact]
    public void DataStoreWrittenToEvent_WriteCoils()
    {
        var dataStore = DataStoreFactory.CreateTestDataStore();

        var readFromEventFired = false;
        var writtenToEventFired = false;

        dataStore.DataStoreWrittenTo += (obj, e) =>
        {
            writtenToEventFired = true;
            Assert.Equal(3, e.Data?.A?.Count);
            Assert.Equal(4, e.StartAddress);
            Assert.Equal(new[] { true, false, true }, e.Data?.A?.ToArray());
            Assert.Equal(ModbusDataType.Coil, e.ModbusDataType);
        };

        dataStore.DataStoreReadFrom += (obj, e) => readFromEventFired = true;

        DataStore.WriteData(dataStore, new DiscreteCollection(true, false, true), dataStore.CoilDiscretes, 4,            new object());
        Assert.False(readFromEventFired);
        Assert.True(writtenToEventFired);
    }

    /// <summary>
    /// Datas the store written to event write holding registers.
    /// </summary>
    [Fact]
    public void DataStoreWrittenToEvent_WriteHoldingRegisters()
    {
        var dataStore = DataStoreFactory.CreateTestDataStore();

        var readFromEventFired = false;
        var writtenToEventFired = false;

        dataStore.DataStoreWrittenTo += (obj, e) =>
        {
            writtenToEventFired = true;
            Assert.Equal(3, e.Data?.B?.Count);
            Assert.Equal(0, e.StartAddress);
            Assert.Equal(new ushort[] { 5, 6, 7 }, e.Data?.B?.ToArray());
            Assert.Equal(ModbusDataType.HoldingRegister, e.ModbusDataType);
        };

        dataStore.DataStoreReadFrom += (obj, e) => readFromEventFired = true;

        DataStore.WriteData(dataStore, new RegisterCollection(5, 6, 7), dataStore.HoldingRegisters, 0, new object());
        Assert.False(readFromEventFired);
        Assert.True(writtenToEventFired);
    }

    /// <summary>
    /// Updates this instance.
    /// </summary>
    [Fact]
    public void Update()
    {
        var newItems = new List<int>(new int[] { 4, 5, 6 });
        var destination = new List<int>(new int[] { 1, 2, 3, 7, 8, 9 });
        DataStore.Update<int>(newItems, destination, 3);
        Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6 }, destination.ToArray());
    }

    /// <summary>
    /// Updates the items too large.
    /// </summary>
    [Fact]
    public void UpdateItemsTooLarge()
    {
        var newItems = new List<int>(new int[] { 1, 2, 3, 7, 8, 9 });
        var destination = new List<int>(new int[] { 4, 5, 6 });
        Assert.Throws<InvalidModbusRequestException>(() => DataStore.Update<int>(newItems, destination, 3));
    }

    /// <summary>
    /// Updates the index of the negative.
    /// </summary>
    [Fact]
    public void UpdateNegativeIndex()
    {
        var newItems = new List<int>(new int[] { 1, 2, 3, 7, 8, 9 });
        var destination = new List<int>(new int[] { 4, 5, 6 });
        Assert.Throws<InvalidModbusRequestException>(() => DataStore.Update<int>(newItems, destination, -1));
    }
}
