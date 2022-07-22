// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;

namespace ModbusRx.Message;

/// <summary>
/// ReadCoilsInputsRequest.
/// </summary>
/// <seealso cref="ModbusRx.Message.AbstractModbusMessage" />
/// <seealso cref="ModbusRx.Message.IModbusRequest" />
public class ReadCoilsInputsRequest : AbstractModbusMessage, IModbusRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadCoilsInputsRequest"/> class.
    /// </summary>
    public ReadCoilsInputsRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadCoilsInputsRequest"/> class.
    /// </summary>
    /// <param name="functionCode">The function code.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    public ReadCoilsInputsRequest(byte functionCode, byte slaveAddress, ushort startAddress, ushort numberOfPoints)
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
            if (value > Modbus.MaximumDiscreteRequestResponseSize)
            {
                var msg = $"Maximum amount of data {Modbus.MaximumDiscreteRequestResponseSize} coils.";
                throw new ArgumentOutOfRangeException(nameof(NumberOfPoints), msg);
            }

            MessageImpl.NumberOfPoints = value;
        }
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Read {NumberOfPoints} {(FunctionCode == Modbus.ReadCoils ? "coils" : "inputs")} starting at address {StartAddress}.";

    /// <inheritdoc/>
    public void ValidateResponse(IModbusMessage response)
    {
        var typedResponse = (ReadCoilsInputsResponse)response;

        // best effort validation - the same response for a request for 1 vs 6 coils (same byte count) will pass validation.
        var expectedByteCount = (NumberOfPoints + 7) / 8;

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
