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
using ModbusRx.UnitTests.Message;
using ModbusRx.Utility;
using Moq;

namespace ModbusRx.UnitTests.IO;

/// <summary>Tests the ModbusSerialTransportFixture behavior.</summary>
public class ModbusSerialTransportFixture
{
    /// <summary>Gets the stream resource.</summary>
    /// <value>
    /// The stream resource.
    /// </value>
    private static IStreamResource StreamResource => new Mock<IStreamResource>(MockBehavior.Strict).Object;

    /// <summary>Creates the response.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task CreateResponse()
    {
        var transport = new ModbusAsciiTransport(StreamResource);
        var expectedResponse = new ReadCoilsInputsResponse(Modbus.ReadCoils, 2, 1, new DiscreteCollection(true, false, false, false, false, false, false, true));
        var lrc = ModbusUtility.CalculateLrc(expectedResponse.MessageFrame);
        var frame = Task.FromResult<byte[]>([ 2, Modbus.ReadCoils, 1, 129, lrc]);
        var response = await transport.CreateResponse<ReadCoilsInputsResponse>(frame);

        _ = Assert.IsType<ReadCoilsInputsResponse>(response);
        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(expectedResponse, response);
    }

    /// <summary>Creates the response erroneous LRC.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task CreateResponseErroneousLrcAsync()
    {
        var transport = new ModbusAsciiTransport(StreamResource) { CheckFrame = true };
        var frame = Task.FromResult<byte[]>([ 19, Modbus.ReadCoils, 0, 0, 0, 2, 115]);

        await Assert.ThrowsAsync<IOException>(() => transport.CreateResponse<ReadCoilsInputsResponse>(frame));
    }

    /// <summary>Creates the response erroneous LRC do not check frame.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task CreateResponseErroneousLrcDoNotCheckFrame()
    {
        var transport = new ModbusAsciiTransport(StreamResource) { CheckFrame = false };
        var frame = Task.FromResult<byte[]>([ 19, Modbus.ReadCoils, 0, 0, 0, 2, 115]);

        await transport.CreateResponse<ReadCoilsInputsResponse>(frame);
    }

    /// <summary>
    /// When using the serial RTU protocol the beginning of the message could get mangled leading to an unsupported message type.
    /// We want to be sure to try the message again so clear the RX buffer and try again.
    /// </summary>
    [TUnit.Core.Test]
    public void UnicastMessage_PurgeReceiveBuffer()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        var serialResource = mock.Object;
        var transport = new ModbusRtuTransport(serialResource);

        _ = mock.Setup(s => s.DiscardInBuffer());
        _ = mock.Setup(s => s.Write(It.IsAny<byte[]>(), 0, 0));

        serialResource.DiscardInBuffer();
        serialResource.Write(null!, 0, 0);

        serialResource.DiscardInBuffer();
        serialResource.Write(null!, 0, 0);

        // normal response
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 2, 1, new DiscreteCollection(true, false, true, false, false, false, false, false));
        var crc = ModbusUtility.CalculateCrc(response.MessageFrame);
        var responseFrame = new byte[response.MessageFrame.Length + crc.Length];
        Array.Copy(response.MessageFrame, responseFrame, response.MessageFrame.Length);
        Array.Copy(crc, 0, responseFrame, response.MessageFrame.Length, crc.Length);
        var responseBytes = new Queue<byte>(responseFrame);

        // write request
        _ = mock.Setup(s => s.Write(It.Is<byte[]>(x => x.Length == 8), 0, 8));

        // read response
        _ = mock.Setup(s => s.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), 1).Result)
            .Returns((byte[] buf, int offset, int count) =>
            {
                buf[offset] = responseBytes.Dequeue();
                return 1;
            });

        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 3, 4);
        var actualResponse = transport.UnicastMessage<ReadCoilsInputsResponse>(request);

        ModbusMessageFixture.AssertModbusMessagePropertiesAreEqual(response, actualResponse);
        mock.VerifyAll();
    }
}
