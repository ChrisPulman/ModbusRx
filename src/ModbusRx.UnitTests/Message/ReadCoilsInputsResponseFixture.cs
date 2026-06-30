// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ModbusRx.Data;
using ModbusRx.Message;

namespace ModbusRx.UnitTests.Message;

/// <summary>Tests the ReadCoilsInputsResponseFixture behavior.</summary>
public class ReadCoilsInputsResponseFixture
{
    /// <summary>Creates the read coils response.</summary>
    [TUnit.Core.Test]
    public void CreateReadCoilsResponse()
    {
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 5, 2, new DiscreteCollection(true, true, true, true, true, true, false, false, true, true, false));
        Assert.Equal(Modbus.ReadCoils, response.FunctionCode);
        Assert.Equal(5, response.SlaveAddress);
        Assert.Equal(2, response.ByteCount);
        var col = new DiscreteCollection(true, true, true, true, true, true, false, false, true, true, false);
        Assert.Equal(col.NetworkBytes, response.Data.NetworkBytes);
    }

    /// <summary>Creates the read inputs response.</summary>
    [TUnit.Core.Test]
    public void CreateReadInputsResponse()
    {
        var response = new ReadCoilsInputsResponse(Modbus.ReadInputs, 5, 2, new DiscreteCollection(true, true, true, true, true, true, false, false, true, true, false));
        Assert.Equal(Modbus.ReadInputs, response.FunctionCode);
        Assert.Equal(5, response.SlaveAddress);
        Assert.Equal(2, response.ByteCount);
        var col = new DiscreteCollection(true, true, true, true, true, true, false, false, true, true, false);
        Assert.Equal(col.NetworkBytes, response.Data.NetworkBytes);
    }

    /// <summary>Converts to string_coils.</summary>
    [TUnit.Core.Test]
    public void ToString_Coils()
    {
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 5, 2, new DiscreteCollection(true, true, true, true, true, true, false, false, true, true, false));

        Assert.Equal("Read 11 coils - {1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 0}.", response.ToString());
    }

    /// <summary>Converts to string_inputs.</summary>
    [TUnit.Core.Test]
    public void ToString_Inputs()
    {
        var response = new ReadCoilsInputsResponse(Modbus.ReadInputs, 5, 2, new DiscreteCollection(true, true, true, true, true, true, false, false, true, true, false));

        Assert.Equal("Read 11 inputs - {1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 0}.", response.ToString());
    }
}
