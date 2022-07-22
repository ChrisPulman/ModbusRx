// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Data;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// ModbusMessageWithDataFixture.
/// </summary>
public class ModbusMessageWithDataFixture
{
    /// <summary>
    /// Modbuses the message with data fixture ctor initializes properties.
    /// </summary>
    [Fact]
    public void ModbusMessageWithDataFixtureCtorInitializesProperties()
    {
        AbstractModbusMessageWithData<DiscreteCollection> message = new ReadCoilsInputsResponse(Modbus.ReadCoils, 10, 1, new DiscreteCollection(true, false, true));
        Assert.Equal(Modbus.ReadCoils, message.FunctionCode);
        Assert.Equal(10, message.SlaveAddress);
    }

    /// <summary>
    /// Protocols the data unit read coils response.
    /// </summary>
    [Fact]
    public void ProtocolDataUnitReadCoilsResponse()
    {
        AbstractModbusMessageWithData<DiscreteCollection> message = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 2, new DiscreteCollection(true));
        byte[] expectedResult = { 1, 2, 1 };
        Assert.Equal(expectedResult, message.ProtocolDataUnit);
    }

    /// <summary>
    /// Datas the read coils response.
    /// </summary>
    [Fact]
    public void DataReadCoilsResponse()
    {
        var col = new DiscreteCollection(false, true, false, true, false, true, false, false, false, false);
        AbstractModbusMessageWithData<DiscreteCollection> message = new ReadCoilsInputsResponse(Modbus.ReadCoils, 11, 1, col);
        Assert.Equal(col.Count, message.Data.Count);
        Assert.Equal(col.NetworkBytes, message.Data.NetworkBytes);
    }
}
