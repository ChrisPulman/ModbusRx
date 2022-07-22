// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using CP.IO.Ports;
using ModbusRx.Unme.Common;

namespace ModbusRx.IO;

/// <summary>
///     Concrete Implementor - http://en.wikipedia.org/wiki/Bridge_Pattern.
/// </summary>
internal class TcpClientAdapter : IStreamResource
{
    private TcpClientRx? _tcpClient;

    public TcpClientAdapter(TcpClientRx tcpClient)
    {
        Debug.Assert(tcpClient is not null, "Argument tcpClient cannot be null.");

        _tcpClient = tcpClient;
    }

    public int InfiniteTimeout => Timeout.Infinite;

    public int ReadTimeout
    {
        get => _tcpClient!.ReadTimeout;
        set => _tcpClient!.ReadTimeout = value;
    }

    public int WriteTimeout
    {
        get => _tcpClient!.WriteTimeout;
        set => _tcpClient!.WriteTimeout = value;
    }

    public void Write(byte[] buffer, int offset, int count) =>
        _tcpClient!.Write(buffer, offset, count);

    public Task<int> ReadAsync(byte[] buffer, int offset, int count) =>
        _tcpClient!.ReadAsync(buffer, offset, count);

    public void DiscardInBuffer() =>
        _tcpClient!.DiscardInBuffer();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposableUtility.Dispose(ref _tcpClient);
        }
    }
}
