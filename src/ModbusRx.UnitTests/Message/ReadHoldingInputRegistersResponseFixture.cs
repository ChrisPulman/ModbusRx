// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ModbusRx.Data;
using ModbusRx.Message;

namespace ModbusRx.UnitTests.Message;

/// <summary>Tests the ReadHoldingInputRegistersResponseFixture behavior.</summary>
public class ReadHoldingInputRegistersResponseFixture
{
    /// <summary>Reads the holding input registers response null data.</summary>
    [TUnit.Core.Test]
    public void ReadHoldingInputRegistersResponse_NullData() =>
        Assert.Throws<ArgumentNullException>(() => _ = new ReadHoldingInputRegistersResponse(0, 0, null!));

    /// <summary>Reads the holding registers response.</summary>
    [TUnit.Core.Test]
    public void ReadHoldingRegistersResponse()
    {
        var response = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 5, new RegisterCollection(1, 2));
        Assert.Equal(Modbus.ReadHoldingRegisters, response.FunctionCode);
        Assert.Equal(5, response.SlaveAddress);
        Assert.Equal(4, response.ByteCount);
        var col = new RegisterCollection(1, 2);
        Assert.Equal(col.NetworkBytes, response.Data.NetworkBytes);
    }

    /// <summary>Converts to string_readholdingregistersresponse.</summary>
    [TUnit.Core.Test]
    public void ToString_ReadHoldingRegistersResponse()
    {
        var response = new ReadHoldingInputRegistersResponse(Modbus.ReadHoldingRegisters, 1, new RegisterCollection(1));
        Assert.Equal("Read 1 holding registers.", response.ToString());
    }

    /// <summary>Reads the input registers response.</summary>
    [TUnit.Core.Test]
    public void ReadInputRegistersResponse()
    {
        var response = new ReadHoldingInputRegistersResponse(Modbus.ReadInputRegisters, 5, new RegisterCollection(1, 2));
        Assert.Equal(Modbus.ReadInputRegisters, response.FunctionCode);
        Assert.Equal(5, response.SlaveAddress);
        Assert.Equal(4, response.ByteCount);
        var col = new RegisterCollection(1, 2);
        Assert.Equal(col.NetworkBytes, response.Data.NetworkBytes);
    }

    /// <summary>Converts to string_readinputregistersresponse.</summary>
    [TUnit.Core.Test]
    public void ToString_ReadInputRegistersResponse()
    {
        var response = new ReadHoldingInputRegistersResponse(Modbus.ReadInputRegisters, 1, new RegisterCollection(1));
        Assert.Equal("Read 1 input registers.", response.ToString());
    }
}
