﻿// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.IO;
using ModbusRx.Message;
using ModbusRx.Utility;
using Moq;
using Xunit;

namespace ModbusRx.UnitTests.IO;

/// <summary>
/// ModbusRtuTransportFixture.
/// </summary>
public class ModbusRtuTransportFixture
{
    /// <summary>
    /// Gets the stream resource.
    /// </summary>
    /// <value>
    /// The stream resource.
    /// </value>
    private static IStreamResource StreamResource => new Mock<IStreamResource>(MockBehavior.Strict).Object;

    /// <summary>
    /// Builds the message frame.
    /// </summary>
    [Fact]
    public void BuildMessageFrame()
    {
        byte[] message = { 17, Modbus.ReadCoils, 0, 19, 0, 37, 14, 132 };
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 17, 19, 37);
        var transport = new ModbusRtuTransport(StreamResource);

        Assert.Equal(message, transport.BuildMessageFrame(request));
    }

    /// <summary>
    /// Responses the bytes to read coils.
    /// </summary>
    [Fact]
    public void ResponseBytesToReadCoils()
    {
        byte[] frameStart = { 0x11, 0x01, 0x05, 0xCD, 0x6B, 0xB2, 0x0E, 0x1B };
        Assert.Equal(6, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>
    /// Responses the bytes to read coils no data.
    /// </summary>
    [Fact]
    public void ResponseBytesToReadCoilsNoData()
    {
        byte[] frameStart = { 0x11, 0x01, 0x00, 0x00, 0x00 };
        Assert.Equal(1, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>
    /// Responses the bytes to read write coils response.
    /// </summary>
    [Fact]
    public void ResponseBytesToReadWriteCoilsResponse()
    {
        byte[] frameStart = { 0x11, 0x0F, 0x00, 0x13, 0x00, 0x0A, 0, 0 };
        Assert.Equal(4, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>
    /// Responses the bytes to read diagnostics.
    /// </summary>
    [Fact]
    public void ResponseBytesToReadDiagnostics()
    {
        byte[] frameStart = { 0x01, 0x08, 0x00, 0x00 };
        Assert.Equal(4, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>
    /// Responses the bytes to read slave exception.
    /// </summary>
    [Fact]
    public void ResponseBytesToReadSlaveException()
    {
        byte[] frameStart = { 0x01, Modbus.ExceptionOffset + 1, 0x01 };
        Assert.Equal(1, ModbusRtuTransport.ResponseBytesToRead(frameStart));
    }

    /// <summary>
    /// Responses the bytes to read invalid function code.
    /// </summary>
    [Fact]
    public void ResponseBytesToReadInvalidFunctionCode()
    {
        byte[] frame = { 0x11, 0x16, 0x00, 0x01, 0x00, 0x02, 0x04 };
        Assert.Throws<NotImplementedException>(() => ModbusRtuTransport.ResponseBytesToRead(frame));
    }

    /// <summary>
    /// Requests the bytes to read diagnostics.
    /// </summary>
    [Fact]
    public void RequestBytesToReadDiagnostics()
    {
        byte[] frame = { 0x01, 0x08, 0x00, 0x00, 0xA5, 0x37, 0, 0 };
        Assert.Equal(1, ModbusRtuTransport.RequestBytesToRead(frame));
    }

    /// <summary>
    /// Requests the bytes to read coils.
    /// </summary>
    [Fact]
    public void RequestBytesToReadCoils()
    {
        byte[] frameStart = { 0x11, 0x01, 0x00, 0x13, 0x00, 0x25 };
        Assert.Equal(1, ModbusRtuTransport.RequestBytesToRead(frameStart));
    }

    /// <summary>
    /// Requests the bytes to read write coils request.
    /// </summary>
    [Fact]
    public void RequestBytesToReadWriteCoilsRequest()
    {
        byte[] frameStart = { 0x11, 0x0F, 0x00, 0x13, 0x00, 0x0A, 0x02, 0xCD, 0x01 };
        Assert.Equal(4, ModbusRtuTransport.RequestBytesToRead(frameStart));
    }

    /// <summary>
    /// Requests the bytes to read write multiple holding registers.
    /// </summary>
    [Fact]
    public void RequestBytesToReadWriteMultipleHoldingRegisters()
    {
        byte[] frameStart = { 0x11, 0x10, 0x00, 0x01, 0x00, 0x02, 0x04 };
        Assert.Equal(6, ModbusRtuTransport.RequestBytesToRead(frameStart));
    }

    /// <summary>
    /// Requests the bytes to read invalid function code.
    /// </summary>
    [Fact]
    public void RequestBytesToReadInvalidFunctionCode()
    {
        byte[] frame = { 0x11, 0xFF, 0x00, 0x01, 0x00, 0x02, 0x04 };
        Assert.Throws<NotImplementedException>(() => ModbusRtuTransport.RequestBytesToRead(frame));
    }

    /// <summary>
    /// Checksumses the match succeed.
    /// </summary>
    [Fact]
    public void ChecksumsMatchSucceed()
    {
        var transport = new ModbusRtuTransport(StreamResource);
        var message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 17, 19, 37);
        byte[] frame = { 17, Modbus.ReadCoils, 0, 19, 0, 37, 14, 132 };

        Assert.True(transport.ChecksumsMatch(message, frame));
    }

    /// <summary>
    /// Checksumses the match fail.
    /// </summary>
    [Fact]
    public void ChecksumsMatchFail()
    {
        var transport = new ModbusRtuTransport(StreamResource);
        var message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 17, 19, 38);
        byte[] frame = { 17, Modbus.ReadCoils, 0, 19, 0, 37, 14, 132 };

        Assert.False(transport.ChecksumsMatch(message, frame));
    }

    /// <summary>
    /// Reads the response.
    /// </summary>
    [Fact]
    public async void ReadResponse()
    {
        var mock = new Mock<ModbusRtuTransport>(StreamResource) { CallBase = true };
        var transport = mock.Object;

        mock.Setup(t => t.Read(ModbusRtuTransport.ResponseFrameStartLength)).Returns(new byte[] { 1, 1, 1, 0 });
        mock.Setup(t => t.Read(2)).Returns(new byte[] { 81, 136 });

        var response = await transport.ReadResponse<ReadCoilsInputsResponse>();
        Assert.IsType<ReadCoilsInputsResponse>(response);

        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, new DiscreteCollection(false));
        Assert.Equal(expectedResponse.MessageFrame, response.MessageFrame);

        mock.VerifyAll();
    }

    /// <summary>
    /// Reads the response slave exception.
    /// </summary>
    [Fact]
    public async void ReadResponseSlaveException()
    {
        var mock = new Mock<ModbusRtuTransport>(StreamResource) { CallBase = true };
        var transport = mock.Object;

        byte[] messageFrame = { 0x01, 0x81, 0x02 };
        var crc = ModbusUtility.CalculateCrc(messageFrame);

        mock.Setup(t => t.Read(ModbusRtuTransport.ResponseFrameStartLength))
            .Returns(Enumerable.Concat(messageFrame, new byte[] { crc[0] }).ToArray());

        mock.Setup(t => t.Read(1))
            .Returns(new byte[] { crc[1] });

        var response = await transport.ReadResponse<ReadCoilsInputsResponse>();
        Assert.IsType<SlaveExceptionResponse>(response);

        var expectedResponse = new SlaveExceptionResponse(0x01, 0x81, 0x02);
        Assert.Equal(expectedResponse.MessageFrame, response.MessageFrame);

        mock.VerifyAll();
    }

    /// <summary>
    /// We want to throw an IOException for any message w/ an invalid checksum,
    /// this must preceed throwing a SlaveException based on function code &gt; 127.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadResponseSlaveExceptionWithErroneousLrcAsync()
    {
        var mock = new Mock<ModbusRtuTransport>(StreamResource) { CallBase = true };
        var transport = mock.Object;

        byte[] messageFrame = { 0x01, 0x81, 0x02 };

        // invalid crc
        byte[] crc = { 0x9, 0x9 };

        mock.Setup(t => t.Read(ModbusRtuTransport.ResponseFrameStartLength))
            .Returns(Enumerable.Concat(messageFrame, new byte[] { crc[0] }).ToArray());

        mock.Setup(t => t.Read(1))
            .Returns(new byte[] { crc[1] });

        await Assert.ThrowsAsync<IOException>(() => transport.ReadResponse<ReadCoilsInputsResponse>());

        mock.VerifyAll();
    }

    /// <summary>
    /// Reads the request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadRequestAsync()
    {
        var mock = new Mock<ModbusRtuTransport>(StreamResource) { CallBase = true };
        var transport = mock.Object;

        mock.Setup(t => t.Read(ModbusRtuTransport.RequestFrameStartLength))
            .Returns(new byte[] { 1, 1, 1, 0, 1, 0, 0 });

        mock.Setup(t => t.Read(1))
            .Returns(new byte[] { 5 });

        Assert.Equal(new byte[] { 1, 1, 1, 0, 1, 0, 0, 5 }, await transport.ReadRequest());

        mock.VerifyAll();
    }

    /// <summary>
    /// Reads this instance.
    /// </summary>
    [Fact]
    public void Read()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);

        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 5), 0, 5).Result)
            .Returns((byte[] buf, int offset, int count) =>
            {
                Array.Copy(new byte[] { 2, 2, 2 }, buf, 3);
                return 3;
            });

        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 5), 3, 2).Result)
            .Returns((byte[] buf, int offset, int count) =>
            {
                Array.Copy(new byte[] { 3, 3 }, 0, buf, 3, 2);
                return 2;
            });

        var transport = new ModbusRtuTransport(mock.Object);
        Assert.Equal(new byte[] { 2, 2, 2, 3, 3 }, transport.Read(5));

        mock.VerifyAll();
    }
}
