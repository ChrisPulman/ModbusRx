// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Message;

/// <summary>
///     Abstract Modbus message.
/// </summary>
public abstract class AbstractModbusMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractModbusMessage"/> class.
    ///     Abstract Modbus message.
    /// </summary>
    internal AbstractModbusMessage() => MessageImpl = new ModbusMessageImpl();

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractModbusMessage"/> class.
    ///     Abstract Modbus message.
    /// </summary>
    internal AbstractModbusMessage(byte slaveAddress, byte functionCode) => MessageImpl = new ModbusMessageImpl(slaveAddress, functionCode);

    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    /// <value>
    /// The transaction identifier.
    /// </value>
    public ushort TransactionId
    {
        get => MessageImpl.TransactionId;
        set => MessageImpl.TransactionId = value;
    }

    /// <summary>
    /// Gets or sets the function code.
    /// </summary>
    /// <value>
    /// The function code.
    /// </value>
    public byte FunctionCode
    {
        get => MessageImpl.FunctionCode;
        set => MessageImpl.FunctionCode = value;
    }

    /// <summary>
    /// Gets or sets the slave address.
    /// </summary>
    /// <value>
    /// The slave address.
    /// </value>
    public byte SlaveAddress
    {
        get => MessageImpl.SlaveAddress;
        set => MessageImpl.SlaveAddress = value;
    }

    /// <summary>
    /// Gets the message frame.
    /// </summary>
    /// <value>
    /// The message frame.
    /// </value>
    public byte[] MessageFrame =>
        MessageImpl.MessageFrame;

    /// <summary>
    /// Gets the protocol data unit.
    /// </summary>
    /// <value>
    /// The protocol data unit.
    /// </value>
    public virtual byte[] ProtocolDataUnit =>
        MessageImpl.ProtocolDataUnit;

    /// <summary>
    /// Gets the minimum size of the frame.
    /// </summary>
    /// <value>
    /// The minimum size of the frame.
    /// </value>
    public abstract int MinimumFrameSize { get; }

    internal ModbusMessageImpl MessageImpl { get; }

    /// <summary>
    /// Initializes the specified frame.
    /// </summary>
    /// <param name="frame">The frame.</param>
    public void Initialize(byte[] frame)
    {
        if (frame?.Length < MinimumFrameSize)
        {
            var msg = $"Message frame must contain at least {MinimumFrameSize} bytes of data.";
            throw new FormatException(msg);
        }

        MessageImpl.Initialize(frame!);
        InitializeUnique(frame!);
    }

    /// <summary>
    /// Initializes the unique.
    /// </summary>
    /// <param name="frame">The frame.</param>
    protected abstract void InitializeUnique(byte[] frame);
}
