// <copyright file="IModbusMessageDataCollection.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ModbusRx.Data;

/// <summary>
///     Modbus message containing data.
/// </summary>
public interface IDataCollection
{
    /// <summary>
    ///     Gets the network bytes.
    /// </summary>
    byte[] NetworkBytes { get; }

    /// <summary>
    ///     Gets the byte count.
    /// </summary>
    byte ByteCount { get; }
}
