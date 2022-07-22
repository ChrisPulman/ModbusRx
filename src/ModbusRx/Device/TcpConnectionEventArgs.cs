// <copyright file="TcpConnectionEventArgs.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ModbusRx.Device;

internal class TcpConnectionEventArgs : EventArgs
{
    public TcpConnectionEventArgs(string endPoint)
    {
        if (endPoint == null)
        {
            throw new ArgumentNullException(nameof(endPoint));
        }

        if (endPoint == string.Empty)
        {
            throw new ArgumentException(Resources.EmptyEndPoint);
        }

        EndPoint = endPoint;
    }

    public string EndPoint { get; set; }
}
