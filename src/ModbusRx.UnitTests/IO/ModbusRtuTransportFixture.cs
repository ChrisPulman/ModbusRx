// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.IO;
using ModbusRx.Message;
using ModbusRx.Utility;
using Moq;

namespace ModbusRx.UnitTests.IO;

/// <summary>Tests the ModbusRtuTransportFixture behavior.</summary>
public class ModbusRtuTransportFixture
{
    /// <summary>Gets the stream resource.</summary>
    /// <value>
    /// The stream resource.
    /// </value>
    private static IStreamResource StreamResource => new Mock<IStreamResource>(MockBehavior.Strict).Object;

    /// <summary>Builds the message frame.</summary>
    [TUnit.Core.Test]
    public void BuildMessageFrame()
    {
        byte[] message = { 17, Modbus.ReadCoils, 0, 19, 0, 37, 14, 132 };
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 17, 19, 37);
        var transport = new ModbusRtuTransport(StreamResource);

        Assert.Equal(message, transport.BuildMessageFrame(request));
    }

    /// <summary>Responses the bytes to read coils.</summary>
    [TUnit.Core.Test]
    public void ResponseBytesToReadCoils()
    {
        byte[] frameStart = { 0x11, 0x01, 0x05, 0xCD, 0x6B, 0xB2, 0x0E, 0x1B };
        Assert.Equal(6, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>Responses the bytes to read coils no data.</summary>
    [TUnit.Core.Test]
    public void ResponseBytesToReadCoilsNoData()
    {
        byte[] frameStart = { 0x11, 0x01, 0x00, 0x00, 0x00 };
        Assert.Equal(1, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>Responses the bytes to read write coils response.</summary>
    [TUnit.Core.Test]
    public void ResponseBytesToReadWriteCoilsResponse()
    {
        byte[] frameStart = { 0x11, 0x0F, 0x00, 0x13, 0x00, 0x0A, 0, 0 };
        Assert.Equal(4, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>Responses the bytes to read diagnostics.</summary>
    [TUnit.Core.Test]
    public void ResponseBytesToReadDiagnostics()
    {
        byte[] frameStart = { 0x01, 0x08, 0x00, 0x00 };
        Assert.Equal(4, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>Responses the bytes to read slave exception.</summary>
    [TUnit.Core.Test]
    public void ResponseBytesToReadSlaveException()
    {
        byte[] frameStart = { 0x01, Modbus.ExceptionOffset + 1, 0x01 };
        Assert.Equal(1, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>Responses the bytes to read invalid function code.</summary>
    [TUnit.Core.Test]
    public void ResponseBytesToReadInvalidFunctionCode()
    {
        byte[] frame = { 0x11, 0x16, 0x00, 0x01, 0x00, 0x02, 0x04 };
        _ = Assert.Throws<NotSupportedException>(() => ModbusRtuTransport.ResponseBytesToRead(frame));
    }

    /// <summary>Requests the bytes to read diagnostics.</summary>
    [TUnit.Core.Test]
    public void RequestBytesToReadDiagnostics()
    {
        byte[] frame = { 0x01, 0x08, 0x00, 0x00, 0xA5, 0x37, 0, 0 };
        Assert.Equal(1, ModbusRtuTransport.RequestBytesToRead(frame));
    }

    /// <summary>Requests the bytes to read coils.</summary>
    [TUnit.Core.Test]
    public void RequestBytesToReadCoils()
    {
        byte[] frameStart = { 0x11, 0x01, 0x00, 0x13, 0x00, 0x25 };
        Assert.Equal(1, ModbusRtuTransport.RequestBytesToRead(frameStart));
    }

    /// <summary>Requests the bytes to read write coils request.</summary>
    [TUnit.Core.Test]
    public void RequestBytesToReadWriteCoilsRequest()
    {
        byte[] frameStart = { 0x11, 0x0F, 0x00, 0x13, 0x00, 0x0A, 0x02, 0xCD, 0x01 };
        Assert.Equal(4, ModbusRtuTransport.RequestBytesToRead(frameStart));
    }

    /// <summary>Requests the bytes to read write multiple holding registers.</summary>
    [TUnit.Core.Test]
    public void RequestBytesToReadWriteMultipleHoldingRegisters()
    {
        byte[] frameStart = { 0x11, 0x10, 0x00, 0x01, 0x00, 0x02, 0x04 };
        Assert.Equal(6, ModbusRtuTransport.RequestBytesToRead(frameStart));
    }

    /// <summary>Requests the bytes to read invalid function code.</summary>
    [TUnit.Core.Test]
    public void RequestBytesToReadInvalidFunctionCode()
    {
        byte[] frame = { 0x11, 0xFF, 0x00, 0x01, 0x00, 0x02, 0x04 };
        _ = Assert.Throws<NotSupportedException>(() => ModbusRtuTransport.RequestBytesToRead(frame));
    }

    /// <summary>Checksumses the match succeed.</summary>
    [TUnit.Core.Test]
    public void ChecksumsMatchSucceed()
    {
        var transport = new ModbusRtuTransport(StreamResource);
        var message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 17, 19, 37);
        byte[] frame = { 17, Modbus.ReadCoils, 0, 19, 0, 37, 14, 132 };

        Assert.True(transport.ChecksumsMatch(message, frame));
    }

    /// <summary>Checksumses the match fail.</summary>
    [TUnit.Core.Test]
    public void ChecksumsMatchFail()
    {
        var transport = new ModbusRtuTransport(StreamResource);
        var message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 17, 19, 38);
        byte[] frame = { 17, Modbus.ReadCoils, 0, 19, 0, 37, 14, 132 };

        Assert.False(transport.ChecksumsMatch(message, frame));
    }

    /// <summary>Reads the response.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadResponse()
    {
        var mock = CreateReadStreamResource([1, 1, 1, 0, 81, 136]);
        var transport = new ModbusRtuTransport(mock.Object);

        var response = await transport.ReadResponse<ReadCoilsInputsResponse>();
        _ = Assert.IsType<ReadCoilsInputsResponse>(response);

        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, new DiscreteCollection(false));
        Assert.Equal(expectedResponse.MessageFrame, response.MessageFrame);

        mock.VerifyAll();
    }

    /// <summary>Reads the response slave exception.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadResponseSlaveException()
    {
        byte[] messageFrame = { 0x01, 0x81, 0x02 };
        var crc = ModbusUtility.CalculateCrc(messageFrame);
        var responseStart = new byte[messageFrame.Length + 1];
        Array.Copy(messageFrame, responseStart, messageFrame.Length);
        responseStart[messageFrame.Length] = crc[0];

        var mock = CreateReadStreamResource(CombineBytes(responseStart, crc[1]));
        var transport = new ModbusRtuTransport(mock.Object);

        var response = await transport.ReadResponse<ReadCoilsInputsResponse>();
        _ = Assert.IsType<SlaveExceptionResponse>(response);

        var expectedResponse = new SlaveExceptionResponse(0x01, 0x81, 0x02);
        Assert.Equal(expectedResponse.MessageFrame, response.MessageFrame);

        mock.VerifyAll();
    }

    /// <summary>
    /// We want to throw an IOException for any message w/ an invalid checksum,
    /// this must preceed throwing a SlaveException based on function code &gt; 127.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadResponseSlaveExceptionWithErroneousLrcAsync()
    {
        byte[] messageFrame = { 0x01, 0x81, 0x02 };

        // invalid crc
        byte[] crc = { 0x9, 0x9 };
        var responseStart = new byte[messageFrame.Length + 1];
        Array.Copy(messageFrame, responseStart, messageFrame.Length);
        responseStart[messageFrame.Length] = crc[0];

        var mock = CreateReadStreamResource(CombineBytes(responseStart, crc[1]));
        var transport = new ModbusRtuTransport(mock.Object);

        await Assert.ThrowsAsync<IOException>(() => transport.ReadResponse<ReadCoilsInputsResponse>());

        mock.VerifyAll();
    }

    /// <summary>Reads the request.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ReadRequestAsync()
    {
        var mock = CreateReadStreamResource([1, 1, 1, 0, 1, 0, 0, 5]);
        var transport = new ModbusRtuTransport(mock.Object);

        Assert.Equal([ 1, 1, 1, 0, 1, 0, 0, 5], await transport.ReadRequest());

        mock.VerifyAll();
    }

    /// <summary>Reads this instance.</summary>
    [TUnit.Core.Test]
    public void Read()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        var bytes = new Queue<byte>([ 2, 2, 2, 3, 3]);

        _ = mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 5), It.IsAny<int>(), 1).Result)
            .Returns((byte[] buf, int offset, int count) =>
            {
                buf[offset] = bytes.Dequeue();
                return 1;
            });

        var transport = new ModbusRtuTransport(mock.Object);
        Assert.Equal([ 2, 2, 2, 3, 3], transport.Read(5));

        mock.VerifyAll();
    }

    /// <summary>Combines bytes with a trailing byte.</summary>
    /// <param name="bytes">The bytes to copy.</param>
    /// <param name="trailingByte">The trailing byte.</param>
    /// <returns>The combined bytes.</returns>
    private static byte[] CombineBytes(byte[] bytes, byte trailingByte)
    {
        var combined = new byte[bytes.Length + 1];
        Array.Copy(bytes, combined, bytes.Length);
        combined[^1] = trailingByte;
        return combined;
    }

    /// <summary>Creates a stream resource that returns the provided bytes one byte at a time.</summary>
    /// <param name="bytes">The bytes to return.</param>
    /// <returns>The configured stream resource mock.</returns>
    private static Mock<IStreamResource> CreateReadStreamResource(byte[] bytes)
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        var queue = new Queue<byte>(bytes);

        _ = mock.Setup(s => s.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), 1).Result)
            .Returns((byte[] buffer, int offset, int count) =>
            {
                buffer[offset] = queue.Dequeue();
                return 1;
            });

        return mock;
    }
}
