// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using ModbusRx.IO;
using ModbusRx.Message;
using Moq;
using Xunit;

namespace ModbusRx.UnitTests.IO;

/// <summary>
/// ModbusAsciiTransportFixture.
/// </summary>
public class ModbusAsciiTransportFixture
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
        byte[] expected = { 58, 48, 50, 48, 49, 48, 48, 48, 48, 48, 48, 48, 49, 70, 67, 13, 10 };
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 0, 1);
        var actual = new ModbusAsciiTransport(StreamResource)
            .BuildMessageFrame(request);

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Reads the request response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadRequestResponseAsync()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        var stream = mock.Object;
        var transport = new ModbusAsciiTransport(stream);
        var calls = 0;
        var bytes = Encoding.ASCII.GetBytes(":110100130025B6\r\n");

        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 1), 0, 1).Result)
            .Returns((byte[] buffer, int offset, int count) =>
            {
                buffer[offset] = bytes[calls++];
                return 1;
            });

        Assert.Equal(new byte[] { 17, 1, 0, 19, 0, 37, 182 }, await transport.ReadRequestResponse());
        mock.VerifyAll();
    }

    /// <summary>
    /// Reads the request response not enough bytes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadRequestResponseNotEnoughBytesAsync()
    {
        var mock = new Mock<IStreamResource>(MockBehavior.Strict);
        var stream = mock.Object;
        var transport = new ModbusAsciiTransport(stream);
        var calls = 0;
        var bytes = Encoding.ASCII.GetBytes(":10\r\n");

        mock.Setup(s => s.ReadAsync(It.Is<byte[]>(x => x.Length == 1), 0, 1).Result)
            .Returns((byte[] buffer, int offset, int count) =>
            {
                buffer[offset] = bytes[calls++];
                return 1;
            });

        await Assert.ThrowsAsync<IOException>(() => transport.ReadRequestResponse());
        mock.VerifyAll();
    }

    /// <summary>
    /// Checksumses the match succeed.
    /// </summary>
    [Fact]
    public void ChecksumsMatchSucceed()
    {
        var transport = new ModbusAsciiTransport(StreamResource);
        var message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 17, 19, 37);
        byte[] frame = { 17, Modbus.ReadCoils, 0, 19, 0, 37, 182 };

        Assert.True(transport.ChecksumsMatch(message, frame));
    }

    /// <summary>
    /// Checksumses the match fail.
    /// </summary>
    [Fact]
    public void ChecksumsMatchFail()
    {
        var transport = new ModbusAsciiTransport(StreamResource);
        var message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 17, 19, 37);
        byte[] frame = { 17, Modbus.ReadCoils, 0, 19, 0, 37, 181 };

        Assert.False(transport.ChecksumsMatch(message, frame));
    }
}
