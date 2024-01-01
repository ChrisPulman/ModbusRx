// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.IO;
using ModbusRx.Message;
using ModbusRx.UnitTests.Message;
using Moq;
using Xunit;

namespace ModbusRx.UnitTests.IO;

/// <summary>
/// ModbusTcpTransportFixture.
/// </summary>
public class ModbusTcpTransportFixture
{
    /// <summary>
    /// Gets the stream resource mock.
    /// </summary>
    /// <value>
    /// The stream resource mock.
    /// </value>
    private static IStreamResource StreamResourceMock => new Mock<IStreamResource>(MockBehavior.Strict).Object;

    /// <summary>
    /// Builds the message frame.
    /// </summary>
    [Fact]
    public void BuildMessageFrame()
    {
        var mock = new Mock<ModbusIpTransport>(StreamResourceMock) { CallBase = true };
        var message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 10, 5);

        var result = mock.Object.BuildMessageFrame(message);
        Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 6, 2, 1, 0, 10, 0, 5 }, result);
        mock.VerifyAll();
    }

    /// <summary>
    /// Gets the mbap header.
    /// </summary>
    [Fact]
    public void GetMbapHeader()
    {
        var message = new WriteMultipleRegistersRequest(3, 1, MessageUtility.CreateDefaultCollection<RegisterCollection, ushort>(0, 120));
        message.TransactionId = 45;
        Assert.Equal(new byte[] { 0, 45, 0, 0, 0, 247, 3 }, ModbusIpTransport.GetMbapHeader(message));
    }

    /// <summary>
    /// Writes this instance.
    /// </summary>
    [Fact]
    public void Write()
    {
        var streamMock = new Mock<IStreamResource>(MockBehavior.Strict);
        var mock = new Mock<ModbusIpTransport>(streamMock.Object) { CallBase = true };
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 3);

        streamMock.Setup(s => s.Write(It.IsNotNull<byte[]>(), 0, 12));

        mock.Setup(t => t.GetNewTransactionId()).Returns(ushort.MaxValue);

        mock.Object.Write(request);

        Assert.Equal(ushort.MaxValue, request.TransactionId);

        mock.VerifyAll();
        streamMock.VerifyAll();
    }

    /// <summary>
    /// Reads the request response.
    /// </summary>
    [Fact]
    public async void ReadRequestResponse()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 3);
        var calls = 0;
        byte[][] source =
        {
                new byte[] { 45, 63, 0, 0, 0, 6 },
                new byte[] { 1 }.Concat(request.ProtocolDataUnit).ToArray(),
        };

        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 0, 6).Result)
            .Returns((byte[] buf, int offset, int count) =>
            {
                Array.Copy(source[calls++], buf, 6);
                return 6;
            });

        Assert.Equal(
            new byte[] { 45, 63, 0, 0, 0, 6, 1, 1, 0, 1, 0, 3 }, await ModbusIpTransport.ReadRequestResponse(mock.Object));

        mock.VerifyAll();
    }

    /// <summary>
    /// Reads the request response connection aborted while reading mbap header.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadRequestResponse_ConnectionAbortedWhileReadingMBAPHeaderAsync()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 0, 6).Result).Returns(3);
        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 3, 3).Result).Returns(0);

        await Assert.ThrowsAsync<IOException>(() => ModbusIpTransport.ReadRequestResponse(mock.Object));
        mock.VerifyAll();
    }

    /// <summary>
    /// Reads the request response connection aborted while reading message frame.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadRequestResponse_ConnectionAbortedWhileReadingMessageFrameAsync()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);

        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 0, 6).Result).Returns(6);
        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 0, 6).Result).Returns(3);
        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 3, 3).Result).Returns(0);

        await Assert.ThrowsAsync<IOException>(() => ModbusIpTransport.ReadRequestResponse(mock.Object));
        mock.VerifyAll();
    }

    /// <summary>
    /// Gets the new transaction identifier.
    /// </summary>
    [Fact]
    public void GetNewTransactionId()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);

        Assert.Equal(1, transport.GetNewTransactionId());
        Assert.Equal(2, transport.GetNewTransactionId());
    }

    /// <summary>
    /// Called when [should retry response returns true if within threshold].
    /// </summary>
    [Fact]
    public void OnShouldRetryResponse_ReturnsTrue_IfWithinThreshold()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 1);
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, null!);

        request.TransactionId = 5;
        response.TransactionId = 4;
        transport.RetryOnOldResponseThreshold = 3;

        Assert.True(transport.OnShouldRetryResponse(request, response));
    }

    /// <summary>
    /// Called when [should retry response returns false if threshold disabled].
    /// </summary>
    [Fact]
    public void OnShouldRetryResponse_ReturnsFalse_IfThresholdDisabled()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 1);
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, null!);

        request.TransactionId = 5;
        response.TransactionId = 4;
        transport.RetryOnOldResponseThreshold = 0;

        Assert.False(transport.OnShouldRetryResponse(request, response));
    }

    /// <summary>
    /// Called when [should retry response returns false if equal transaction identifier].
    /// </summary>
    [Fact]
    public void OnShouldRetryResponse_ReturnsFalse_IfEqualTransactionId()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 1);
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, null!);

        request.TransactionId = 5;
        response.TransactionId = 5;
        transport.RetryOnOldResponseThreshold = 3;

        Assert.False(transport.OnShouldRetryResponse(request, response));
    }

    /// <summary>
    /// Called when [should retry response returns false if outside threshold].
    /// </summary>
    [Fact]
    public void OnShouldRetryResponse_ReturnsFalse_IfOutsideThreshold()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 1);
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, null!);

        request.TransactionId = 5;
        response.TransactionId = 2;
        transport.RetryOnOldResponseThreshold = 3;

        Assert.False(transport.OnShouldRetryResponse(request, response));
    }

    /// <summary>
    /// Validates the response mismatching transaction ids.
    /// </summary>
    [Fact]
    public void ValidateResponse_MismatchingTransactionIds()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);

        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 1);
        request.TransactionId = 5;
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, null!);
        response.TransactionId = 6;

        Assert.Throws<IOException>(() => transport.ValidateResponse(request, response));
    }

    /// <summary>
    /// Validates the response.
    /// </summary>
    [Fact]
    public void ValidateResponse()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);

        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 1);
        request.TransactionId = 5;
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, null!);
        response.TransactionId = 5;

        // no exception is thrown
        transport.ValidateResponse(request, response);
    }
}
