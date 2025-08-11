// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.IO;
using ModbusRx.Message;
using ModbusRx.UnitTests.Message;
using ModbusRx.Utility;
using Moq;
using Xunit;

namespace ModbusRx.UnitTests.IO;

/// <summary>
/// ModbusSerialTransportFixture.
/// </summary>
public class ModbusSerialTransportFixture
{
    /// <summary>
    /// Gets the stream resource.
    /// </summary>
    /// <value>
    /// The stream resource.
    /// </value>
    private static IStreamResource StreamResource => new Mock<IStreamResource>(MockBehavior.Strict).Object;

    /// <summary>
    /// Creates the response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CreateResponse()
    {
        var transport = new ModbusAsciiTransport(StreamResource);
        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadCoils, 2, 1, new DiscreteCollection(true, false, false, false, false, false, false, true));
        var lrc = ModbusUtility.CalculateLrc(expectedResponse.MessageFrame);
        var frame = Task.FromResult(new byte[] { 2, Modbus.ReadCoils, 1, 129, lrc });
        var response = await transport.CreateResponse<ReadCoilsInputsResponse>(frame);

        Assert.IsType<ReadCoilsInputsResponse>(response);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
    }

    /// <summary>
    /// Creates the response erroneous LRC.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CreateResponseErroneousLrcAsync()
    {
        var transport = new ModbusAsciiTransport(StreamResource) { CheckFrame = true };
        var frame = Task.FromResult(new byte[] { 19, Modbus.ReadCoils, 0, 0, 0, 2, 115 });

        await Assert.ThrowsAsync<IOException>(() => transport.CreateResponse<ReadCoilsInputsResponse>(frame));
    }

    /// <summary>
    /// Creates the response erroneous LRC do not check frame.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CreateResponseErroneousLrcDoNotCheckFrame()
    {
        var transport = new ModbusAsciiTransport(StreamResource) { CheckFrame = false };
        var frame = Task.FromResult(new byte[] { 19, Modbus.ReadCoils, 0, 0, 0, 2, 115 });

        await transport.CreateResponse<ReadCoilsInputsResponse>(frame);
    }

    /// <summary>
    /// When using the serial RTU protocol the beginning of the message could get mangled leading to an unsupported message type.
    /// We want to be sure to try the message again so clear the RX buffer and try again.
    /// </summary>
    [Fact]
    public void UnicastMessage_PurgeReceiveBuffer()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        var serialResource = mock.Object;
        var transport = new ModbusRtuTransport(serialResource);

        mock.Setup(s => s.DiscardInBuffer());
        mock.Setup(s => s.Write(It.IsAny<byte[]>(), 0, 0));

        serialResource.DiscardInBuffer();
        serialResource.Write(null!, 0, 0);

        // mangled response
        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 4), 0, 4).Result).Returns(4);

        serialResource.DiscardInBuffer();
        serialResource.Write(null!, 0, 0);

        // normal response
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 2, 1, new DiscreteCollection(true, false, true, false, false, false, false, false));

        // write request
        mock.Setup(s => s.Write(It.Is<byte[]>(x => x.Length == 8), 0, 8));

        // read header
        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 4), 0, 4).Result)
            .Returns((byte[] buf, int offset, int count) =>
            {
                Array.Copy(response.MessageFrame, 0, buf, 0, 4);
                return 4;
            });

        // read remainder
        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 2), 0, 2).Result)
            .Returns((byte[] buf, int offset, int count) =>
            {
                Array.Copy(ModbusUtility.CalculateCrc(response.MessageFrame), 0, buf, 0, 2);
                return 2;
            });

        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 3, 4);
        var actualResponse = transport.UnicastMessage<ReadCoilsInputsResponse>(request);

        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(response, actualResponse);
        mock.VerifyAll();
    }
}
