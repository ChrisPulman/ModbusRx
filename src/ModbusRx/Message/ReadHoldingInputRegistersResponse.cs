// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Data;
using ModbusRx.Unme.Common;

namespace ModbusRx.Message;

/// <summary>
/// ReadHoldingInputRegistersResponse.
/// </summary>
/// <seealso cref="ModbusRx.Message.IModbusMessage" />
public class ReadHoldingInputRegistersResponse : AbstractModbusMessageWithData<RegisterCollection>, IModbusMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadHoldingInputRegistersResponse"/> class.
    /// </summary>
    public ReadHoldingInputRegistersResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadHoldingInputRegistersResponse"/> class.
    /// </summary>
    /// <param name="functionCode">The function code.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="data">The data.</param>
    /// <exception cref="System.ArgumentNullException">data.</exception>
    public ReadHoldingInputRegistersResponse(byte functionCode, byte slaveAddress, RegisterCollection data)
        : base(slaveAddress, functionCode)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        ByteCount = data.ByteCount;
        Data = data;
    }

    /// <summary>
    /// Gets or sets the byte count.
    /// </summary>
    /// <value>
    /// The byte count.
    /// </value>
    public byte ByteCount
    {
        get => MessageImpl.ByteCount!.Value;
        set => MessageImpl.ByteCount = value;
    }

    /// <inheritdoc/>
    public override int MinimumFrameSize => 3;

    /// <inheritdoc/>
    public override string ToString() =>
        $"Read {Data.Count} {(FunctionCode == Modbus.ReadHoldingRegisters ? "holding" : "input")} registers.";

    /// <inheritdoc/>
    protected override void InitializeUnique(byte[] frame)
    {
        if (frame?.Length < MinimumFrameSize + frame![2])
        {
            throw new FormatException("Message frame does not contain enough bytes.");
        }

        ByteCount = frame[2];
        Data = new RegisterCollection(frame.Slice(3, ByteCount).ToArray());
    }
}
