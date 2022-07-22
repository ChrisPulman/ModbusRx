// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// SlaveExceptionResponseFixture.
/// </summary>
public class SlaveExceptionResponseFixture
{
    /// <summary>
    /// Creates the slave exception response.
    /// </summary>
    [Fact]
    public void CreateSlaveExceptionResponse()
    {
        var response = new SlaveExceptionResponse(11, Modbus.ReadCoils + Modbus.ExceptionOffset,            2);
        Assert.Equal(11, response.SlaveAddress);
        Assert.Equal(Modbus.ReadCoils + Modbus.ExceptionOffset, response.FunctionCode);
        Assert.Equal(2, response.SlaveExceptionCode);
    }

    /// <summary>
    /// Slaves the exception response pdu.
    /// </summary>
    [Fact]
    public void SlaveExceptionResponsePDU()
    {
        var response = new SlaveExceptionResponse(11, Modbus.ReadCoils + Modbus.ExceptionOffset,            2);
        Assert.Equal(new byte[] { response.FunctionCode, response.SlaveExceptionCode }, response.ProtocolDataUnit);
    }
}
