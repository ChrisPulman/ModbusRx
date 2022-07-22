// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;

namespace ModbusRx.Message;

/// <summary>
/// ReadHoldingInputRegistersRequest.
/// </summary>
/// <seealso cref="ModbusRx.Message.AbstractModbusMessage" />
/// <seealso cref="ModbusRx.Message.IModbusRequest" />
public class ReadHoldingInputRegistersRequest : AbstractModbusMessage, IModbusRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadHoldingInputRegistersRequest"/> class.
    /// </summary>
    public ReadHoldingInputRegistersRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadHoldingInputRegistersRequest"/> class.
    /// </summary>
    /// <param name="functionCode">The function code.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    public ReadHoldingInputRegistersRequest(byte functionCode, byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        : base(slaveAddress, functionCode)
    {
        StartAddress = startAddress;
        NumberOfPoints = numberOfPoints;
    }

    /// <summary>
    /// Gets or sets the start address.
    /// </summary>
    /// <value>
    /// The start address.
    /// </value>
    public ushort StartAddress
    {
        get => MessageImpl.StartAddress!.Value;
        set => MessageImpl.StartAddress = value;
    }

    /// <inheritdoc/>
    public override int MinimumFrameSize => 6;

    /// <summary>
    /// Gets or sets the number of points.
    /// </summary>
    /// <value>
    /// The number of points.
    /// </value>
    /// <exception cref="System.ArgumentOutOfRangeException">NumberOfPoints.</exception>
    public ushort NumberOfPoints
    {
        get => MessageImpl.NumberOfPoints!.Value;
        set
        {
            if (value > Modbus.MaximumRegisterRequestResponseSize)
            {
                var msg = $"Maximum amount of data {Modbus.MaximumRegisterRequestResponseSize} registers.";
                throw new ArgumentOutOfRangeException(nameof(NumberOfPoints), msg);
            }

            MessageImpl.NumberOfPoints = value;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"Read {NumberOfPoints} {(FunctionCode == Modbus.ReadHoldingRegisters ? "holding" : "input")} registers starting at address {StartAddress}.";

    /// <inheritdoc/>
    public void ValidateResponse(IModbusMessage response)
    {
        var typedResponse = response as ReadHoldingInputRegistersResponse;
        Debug.Assert(typedResponse is not null, "Argument response should be of type ReadHoldingInputRegistersResponse.");
        var expectedByteCount = NumberOfPoints * 2;

        if (expectedByteCount != typedResponse?.ByteCount)
        {
            var msg = $"Unexpected byte count. Expected {expectedByteCount}, received {typedResponse?.ByteCount}.";
            throw new IOException(msg);
        }
    }

    /// <inheritdoc/>
    protected override void InitializeUnique(byte[] frame)
    {
        StartAddress = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        NumberOfPoints = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4));
    }
}
