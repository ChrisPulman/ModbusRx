// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.IO;
using ModbusRx.Message;
using ModbusRx.UnitTests.Message;
using Moq;

namespace ModbusRx.UnitTests.IO;

/// <summary>Tests the ModbusTcpTransportFixture behavior.</summary>
public class ModbusTcpTransportFixture
{
    /// <summary>Gets the stream resource mock.</summary>
    /// <value>
    /// The stream resource mock.
    /// </value>
    private static IStreamResource StreamResourceMock => new Mock<IStreamResource>(MockBehavior.Strict).Object;

    /// <summary>Builds the message frame.</summary>
    [TUnit.Core.Test]
    public void BuildMessageFrame()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);
        var message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 10, 5);

        var result = transport.BuildMessageFrame(message);
        Assert.Equal([ 0, 0, 0, 0, 0, 6, 2, 1, 0, 10, 0, 5], result);
    }

    /// <summary>Gets the mbap header.</summary>
    [TUnit.Core.Test]
    public void GetMbapHeader()
    {
        var message = new WriteMultipleRegistersRequest(3, 1, MessageUtility.CreateDefaultCollection<RegisterCollection, ushort>(0, 120));
        message.TransactionId = 45;
        Assert.Equal([ 0, 45, 0, 0, 0, 247, 3], ModbusIpTransport.GetMbapHeader(message));
    }

    /// <summary>Writes this instance.</summary>
    [TUnit.Core.Test]
    public void Write()
    {
        var streamMock = new Mock<IStreamResource>(MockBehavior.Strict);
        var transport = new ModbusIpTransport(streamMock.Object);
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 3);

        _ = streamMock.Setup(s => s.Write(It.IsNotNull<byte[]>(), 0, 12));

        transport.Write(request);

        Assert.Equal(1, request.TransactionId);

        streamMock.VerifyAll();
    }

    /// <summary>Reads the request response.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadRequestResponse()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 3);
        var calls = 0;
        var unitAndPdu = new byte[request.ProtocolDataUnit.Length + 1];
        unitAndPdu[0] = 1;
        Array.Copy(request.ProtocolDataUnit, 0, unitAndPdu, 1, request.ProtocolDataUnit.Length);
        byte[][] source =
        {
                new byte[] { 45, 63, 0, 0, 0, 6 },
                unitAndPdu,
        };

        _ = mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 0, 6).Result)
            .Returns((byte[] buf, int offset, int count) =>
            {
                Array.Copy(source[calls++], buf, 6);
                return 6;
            });

        Assert.Equal(
            [ 45, 63, 0, 0, 0, 6, 1, 1, 0, 1, 0, 3],
            await ModbusIpTransport.ReadRequestResponse(mock.Object));

        mock.VerifyAll();
    }

    /// <summary>Reads the request response connection aborted while reading mbap header.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadRequestResponse_ConnectionAbortedWhileReadingMBAPHeaderAsync()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        _ = mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 0, 6).Result).Returns(3);
        _ = mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 3, 3).Result).Returns(0);

        await Assert.ThrowsAsync<IOException>(() => ModbusIpTransport.ReadRequestResponse(mock.Object));
        mock.VerifyAll();
    }

    /// <summary>Reads the request response connection aborted while reading message frame.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadRequestResponse_ConnectionAbortedWhileReadingMessageFrameAsync()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);

        _ = mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 0, 6).Result).Returns(6);
        _ = mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 0, 6).Result).Returns(3);
        _ = mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 6), 3, 3).Result).Returns(0);

        await Assert.ThrowsAsync<IOException>(() => ModbusIpTransport.ReadRequestResponse(mock.Object));
        mock.VerifyAll();
    }

    /// <summary>Gets the new transaction identifier.</summary>
    [TUnit.Core.Test]
    public void GetNewTransactionId()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);

        Assert.Equal(1, transport.GetNewTransactionId());
        Assert.Equal(2, transport.GetNewTransactionId());
    }

    /// <summary>Called when [should retry response returns true if within threshold].</summary>
    [TUnit.Core.Test]
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

    /// <summary>Called when [should retry response returns false if threshold disabled].</summary>
    [TUnit.Core.Test]
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

    /// <summary>Called when [should retry response returns false if equal transaction identifier].</summary>
    [TUnit.Core.Test]
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

    /// <summary>Called when [should retry response returns false if outside threshold].</summary>
    [TUnit.Core.Test]
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

    /// <summary>Validates the response mismatching transaction ids.</summary>
    [TUnit.Core.Test]
    public void ValidateResponse_MismatchingTransactionIds()
    {
        var transport = new ModbusIpTransport(StreamResourceMock);

        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 1);
        request.TransactionId = 5;
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, null!);
        response.TransactionId = 6;

        _ = Assert.Throws<IOException>(() => transport.ValidateResponse(request, response));
    }

    /// <summary>Validates the response.</summary>
    [TUnit.Core.Test]
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
