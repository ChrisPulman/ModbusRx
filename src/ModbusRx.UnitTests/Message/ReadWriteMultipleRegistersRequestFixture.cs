// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Data;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// ReadWriteMultipleRegistersRequestFixture.
/// </summary>
public class ReadWriteMultipleRegistersRequestFixture
{
    /// <summary>
    /// Reads the write multiple registers request.
    /// </summary>
    [Fact]
    public void ReadWriteMultipleRegistersRequest()
    {
        var writeCollection = new RegisterCollection(255, 255, 255);
        var request = new ReadWriteMultipleRegistersRequest(5, 3, 6, 14, writeCollection);
        Assert.Equal(Modbus.ReadWriteMultipleRegisters, request.FunctionCode);
        Assert.Equal(5, request.SlaveAddress);

        // test read
        Assert.NotNull(request.ReadRequest);
        Assert.Equal(request.SlaveAddress, request.ReadRequest!.SlaveAddress);
        Assert.Equal(3, request.ReadRequest.StartAddress);
        Assert.Equal(6, request.ReadRequest.NumberOfPoints);

        // test write
        Assert.NotNull(request.WriteRequest);
        Assert.Equal(request.SlaveAddress, request.WriteRequest!.SlaveAddress);
        Assert.Equal(14, request.WriteRequest.StartAddress);
        Assert.Equal(writeCollection.NetworkBytes, request.WriteRequest.Data.NetworkBytes);
    }

    /// <summary>
    /// Protocols the data unit.
    /// </summary>
    [Fact]
    public void ProtocolDataUnit()
    {
        var writeCollection = new RegisterCollection(255, 255, 255);
        var request = new ReadWriteMultipleRegistersRequest(5, 3, 6, 14, writeCollection);
        byte[] pdu =
        {
                0x17, 0x00, 0x03, 0x00, 0x06, 0x00, 0x0e, 0x00, 0x03, 0x06, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
        };
        Assert.Equal(pdu, request.ProtocolDataUnit);
    }

    /// <summary>
    /// Converts to string_readwritemultipleregistersrequest.
    /// </summary>
    [Fact]
    public void ToString_ReadWriteMultipleRegistersRequest()
    {
        var writeCollection = new RegisterCollection(255, 255, 255);
        var request = new ReadWriteMultipleRegistersRequest(5, 3, 6, 14, writeCollection);

        Assert.Equal(
            "Write 3 holding registers starting at address 14, and read 6 registers starting at address 3.",
            request.ToString());
    }
}
