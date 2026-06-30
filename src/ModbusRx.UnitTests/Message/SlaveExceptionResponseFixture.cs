// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ModbusRx.Message;

namespace ModbusRx.UnitTests.Message;

/// <summary>Tests the SlaveExceptionResponseFixture behavior.</summary>
public class SlaveExceptionResponseFixture
{
    /// <summary>Creates the slave exception response.</summary>
    [TUnit.Core.Test]
    public void CreateSlaveExceptionResponse()
    {
        var response = new SlaveExceptionResponse(11, Modbus.ReadCoils + Modbus.ExceptionOffset, 2);
        Assert.Equal(11, response.SlaveAddress);
        Assert.Equal(Modbus.ReadCoils + Modbus.ExceptionOffset, response.FunctionCode);
        Assert.Equal(2, response.SlaveExceptionCode);
    }

    /// <summary>Slaves the exception response pdu.</summary>
    [TUnit.Core.Test]
    public void SlaveExceptionResponsePDU()
    {
        var response = new SlaveExceptionResponse(11, Modbus.ReadCoils + Modbus.ExceptionOffset, 2);
        Assert.Equal([ response.FunctionCode, response.SlaveExceptionCode], response.ProtocolDataUnit);
    }
}
