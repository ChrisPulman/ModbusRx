// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Data;

namespace ModbusRx.Message;

/// <summary>
/// AbstractModbusMessageWithData.
/// </summary>
/// <typeparam name="TData">The type of the data.</typeparam>
/// <seealso cref="ModbusRx.Message.AbstractModbusMessage" />
public abstract class AbstractModbusMessageWithData<TData> : AbstractModbusMessage
    where TData : IDataCollection
{
    internal AbstractModbusMessageWithData()
    {
    }

    internal AbstractModbusMessageWithData(byte slaveAddress, byte functionCode)
        : base(slaveAddress, functionCode)
    {
    }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>
    /// The data.
    /// </value>
    public TData Data
    {
        get => (TData)MessageImpl.Data!;
        set => MessageImpl.Data = value;
    }
}
