// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO.Ports;
using CP.IO.Ports;

namespace ModbusRx.IO;

/// <summary>
///     Concrete Implementor - http://en.wikipedia.org/wiki/Bridge_Pattern.
/// </summary>
public class SerialPortAdapter : IStreamResource
{
    /// <summary>
    /// Creates new line.
    /// </summary>
    private const string NewLine = "\r\n";
    /// <summary>
    /// The serial port.
    /// </summary>
    private SerialPortRx _serialPort;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerialPortAdapter"/> class.
    /// </summary>
    /// <param name="serialPort">The serial port.</param>
    public SerialPortAdapter(SerialPortRx serialPort)
    {
        Debug.Assert(serialPort is not null, "Argument serialPort cannot be null.");

        _serialPort = serialPort!;
        _serialPort.NewLine = NewLine;
    }

    /// <summary>
    /// Gets indicates that no timeout should occur.
    /// </summary>
    public int InfiniteTimeout => SerialPort.InfiniteTimeout;

    /// <summary>
    /// Gets or sets the number of milliseconds before a timeout occurs when a read operation does not finish.
    /// </summary>
    public int ReadTimeout
    {
        get => _serialPort.ReadTimeout;
        set => _serialPort.ReadTimeout = value;
    }

    /// <summary>
    /// Gets or sets the number of milliseconds before a timeout occurs when a write operation does not finish.
    /// </summary>
    public int WriteTimeout
    {
        get => _serialPort.WriteTimeout;
        set => _serialPort.WriteTimeout = value;
    }

    /// <summary>
    /// Purges the receive buffer.
    /// </summary>
    public void DiscardInBuffer() =>
        _serialPort.DiscardInBuffer();

    /// <summary>
    /// Reads a number of bytes from the input buffer and writes those bytes into a byte array at the specified offset.
    /// </summary>
    /// <param name="buffer">The byte array to write the input to.</param>
    /// <param name="offset">The offset in the buffer array to begin writing.</param>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>
    /// The number of bytes read.
    /// </returns>
    public Task<int> ReadAsync(byte[] buffer, int offset, int count) =>
        _serialPort.ReadAsync(buffer, offset, count);

    /// <summary>
    /// Writes a specified number of bytes to the port from an output buffer, starting at the specified offset.
    /// </summary>
    /// <param name="buffer">The byte array that contains the data to write to the port.</param>
    /// <param name="offset">The offset in the buffer array to begin writing.</param>
    /// <param name="count">The number of bytes to write.</param>
    public void Write(byte[] buffer, int offset, int count) =>
        _serialPort.Write(buffer, offset, count);

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _serialPort?.Dispose();
            _serialPort = null!;
        }
    }
}
