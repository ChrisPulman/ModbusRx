// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using CP.IO.Ports;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Unme.Common;
#else
using ModbusRx.Unme.Common;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.IO;
#else
namespace ModbusRx.IO;
#endif

/// <summary>Concrete Implementor - http://en.wikipedia.org/wiki/Bridge_Pattern.</summary>
internal sealed class TcpClientAdapter : IStreamResource
{
    /// <summary>Stores the tcp Client value.</summary>
    private TcpClientRx? _tcpClient;

    /// <summary>Initializes a new instance of the Tcp Client Adapter class.</summary>
    /// <param name="tcpClient">The tcp Client value.</param>
    public TcpClientAdapter(TcpClientRx tcpClient)
    {
        Debug.Assert(tcpClient is not null, "Argument tcpClient cannot be null.");

        _tcpClient = tcpClient;
    }

    /// <summary>Gets or sets the Infinite Timeout value.</summary>
    public int InfiniteTimeout => Timeout.Infinite;

    /// <summary>Gets or sets the Read Timeout value.</summary>
    public int ReadTimeout
    {
        get => _tcpClient!.ReadTimeout;
        set => _tcpClient!.ReadTimeout = value;
    }

    /// <summary>Gets or sets the Write Timeout value.</summary>
    public int WriteTimeout
    {
        get => _tcpClient!.WriteTimeout;
        set => _tcpClient!.WriteTimeout = value;
    }

    /// <summary>Executes the Write operation.</summary>
    /// <param name="buffer">The buffer value.</param>
    /// <param name="offset">The offset value.</param>
    /// <param name="count">The count value.</param>
    public void Write(byte[] buffer, int offset, int count) =>
        _tcpClient!.Write(buffer, offset, count);

    /// <summary>Executes the Read Async operation.</summary>
    /// <param name="buffer">The buffer value.</param>
    /// <param name="offset">The offset value.</param>
    /// <param name="count">The count value.</param>
    /// <returns>The result.</returns>
    public Task<int> ReadAsync(byte[] buffer, int offset, int count) =>
        _tcpClient!.ReadAsync(buffer, offset, count);

    /// <summary>Executes the Discard In Buffer operation.</summary>
    public void DiscardInBuffer() =>
        _tcpClient!.DiscardInBuffer();

    /// <summary>Executes the Dispose operation.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Executes the Dispose operation.</summary>
    /// <param name="disposing">The disposing value.</param>
    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        DisposableUtility.Dispose(ref _tcpClient);
    }
}
