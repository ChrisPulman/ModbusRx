// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CP.IO.Ports;
using ModbusRx.Unme.Common;

namespace ModbusRx.IO;

/// <summary>
///     Concrete Implementor - http://en.wikipedia.org/wiki/Bridge_Pattern.
/// </summary>
internal class UdpClientAdapter : IStreamResource
{
    private UdpClientRx? _udpClient;

    public UdpClientAdapter(UdpClientRx udpClient)
    {
        if (udpClient == null)
        {
            throw new ArgumentNullException(nameof(udpClient));
        }

        _udpClient = udpClient;
    }

    public int InfiniteTimeout => _udpClient!.InfiniteTimeout;

    public int ReadTimeout
    {
        get => _udpClient!.ReadTimeout;
        set => _udpClient!.ReadTimeout = value;
    }

    public int WriteTimeout
    {
        get => _udpClient!.WriteTimeout;
        set => _udpClient!.WriteTimeout = value;
    }

    public void DiscardInBuffer()
    {
        // no-op
    }

    public Task<int> ReadAsync(byte[] buffer, int offset, int count) =>
        _udpClient!.ReadAsync(buffer, offset, count);

    public void Write(byte[] buffer, int offset, int count) =>
        _udpClient?.Write(buffer, offset, count);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposableUtility.Dispose(ref _udpClient);
        }
    }
}
