// <copyright file="ReadCoilsInputsResponse.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using ModbusRx.Data;
using ModbusRx.Unme.Common;

namespace ModbusRx.Message;

/// <summary>
/// ReadCoilsInputsResponse.
/// </summary>
/// <seealso cref="ModbusRx.Message.IModbusMessage" />
public class ReadCoilsInputsResponse : AbstractModbusMessageWithData<DiscreteCollection>, IModbusMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadCoilsInputsResponse"/> class.
    /// </summary>
    public ReadCoilsInputsResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadCoilsInputsResponse"/> class.
    /// </summary>
    /// <param name="functionCode">The function code.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="byteCount">The byte count.</param>
    /// <param name="data">The data.</param>
    public ReadCoilsInputsResponse(byte functionCode, byte slaveAddress, byte byteCount, DiscreteCollection data)
        : base(slaveAddress, functionCode)
    {
        ByteCount = byteCount;
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
        $"Read {Data.Count} {(FunctionCode == Modbus.ReadInputs ? "inputs" : "coils")} - {Data}.";

    /// <inheritdoc/>
    protected override void InitializeUnique(byte[] frame)
    {
        if (frame?.Length < 3 + frame![2])
        {
            throw new FormatException("Message frame data segment does not contain enough bytes.");
        }

        ByteCount = frame[2];
        Data = new DiscreteCollection(frame.Slice(3, ByteCount).ToArray());
    }
}
