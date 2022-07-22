// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if SERIAL
using ModbusRx.Extensions.Enron;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// EnronFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.NModbusSerialRtuMasterDl06SlaveFixture" />
public class EnronFixture : NModbusSerialRtuMasterDl06SlaveFixture
{
    /// <summary>
    /// Reads the holding registers32.
    /// </summary>
    [Fact]
    public virtual void ReadHoldingRegisters32()
    {
        var registers = Master?.ReadHoldingRegisters32(SlaveAddress, 104, 2);
        Assert.Equal(new uint[] { 0, 0 }, registers);
    }

    /// <summary>
    /// Reads the input registers32.
    /// </summary>
    [Fact]
    public virtual void ReadInputRegisters32()
    {
        var registers = Master?.ReadInputRegisters32(SlaveAddress, 104, 2);
        Assert.Equal(new uint[] { 0, 0 }, registers);
    }

    /// <summary>
    /// Writes the single register32.
    /// </summary>
    [Fact]
    public virtual void WriteSingleRegister32()
    {
        const ushort testAddress = 200;
        const uint testValue = 350;

        var originalValue = Master!.ReadHoldingRegisters32(SlaveAddress, testAddress, 1)[0];
        Master?.WriteSingleRegister32(SlaveAddress, testAddress, testValue);
        Assert.Equal(testValue, Master?.ReadHoldingRegisters32(SlaveAddress, testAddress, 1)[0]);
        Master?.WriteSingleRegister32(SlaveAddress, testAddress, originalValue);
        Assert.Equal(originalValue, Master!.ReadHoldingRegisters(SlaveAddress, testAddress, 1)[0]);
    }

    /// <summary>
    /// Writes the multiple registers32.
    /// </summary>
    [Fact]
    public virtual void WriteMultipleRegisters32()
    {
        const ushort testAddress = 120;
        var testValues = new uint[] { 10, 20, 30, 40, 50 };

        var originalValues = Master?.ReadHoldingRegisters32(SlaveAddress, testAddress, (ushort)testValues.Length);
        Master?.WriteMultipleRegisters32(SlaveAddress, testAddress, testValues);
        var newValues = Master?.ReadHoldingRegisters32(SlaveAddress, testAddress, (ushort)testValues.Length);
        Assert.Equal(testValues, newValues);
        Master?.WriteMultipleRegisters32(SlaveAddress, testAddress, originalValues!);
    }
}
#endif
