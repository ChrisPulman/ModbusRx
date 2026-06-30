// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Device;
#else
namespace ModbusRx.Device;
#endif

/// <summary>Provides Tcp Connection Event Args functionality.</summary>
internal sealed class TcpConnectionEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the Tcp Connection Event Args class.</summary>
    /// <param name="endPoint">The end Point value.</param>
    public TcpConnectionEventArgs(string endPoint)
    {
        if (endPoint is null)
        {
            throw new ArgumentNullException(nameof(endPoint));
        }

        if (endPoint.Length == 0)
        {
            throw new ArgumentException(Resources.EmptyEndPoint, nameof(endPoint));
        }

        EndPoint = endPoint;
    }

    /// <summary>Gets or sets the End Point value.</summary>
    public string EndPoint { get; set; }
}
