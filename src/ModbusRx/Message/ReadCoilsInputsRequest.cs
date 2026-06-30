// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Net;

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Message;
#else
namespace ModbusRx.Message;
#endif

/// <summary>Provides ReadCoilsInputsRequest functionality.</summary>
/// <seealso cref="AbstractModbusMessage" />
/// <seealso cref="IModbusRequest" />
public class ReadCoilsInputsRequest : AbstractModbusMessage, IModbusRequest
{
    /// <summary>Initializes a new instance of the <see cref="ReadCoilsInputsRequest"/> class.</summary>
    public ReadCoilsInputsRequest()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ReadCoilsInputsRequest"/> class.</summary>
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

    /// <summary>Gets or sets the start address.</summary>
/// <value>The start address.</value>
    public ushort StartAddress
    {
        get => MessageImpl.StartAddress!.Value;
        set => MessageImpl.StartAddress = value;
    }

    /// <inheritdoc/>
    public override int MinimumFrameSize => 6;

    /// <summary>Gets or sets the number of points.</summary>
    /// <exception cref="System.ArgumentOutOfRangeException">NumberOfPoints.</exception>
    /// The number of points.
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

        if (expectedByteCount == typedResponse?.ByteCount)
        {
            return;
        }

        var msg = $"Unexpected byte count. Expected {expectedByteCount}, received {typedResponse?.ByteCount}.";
        throw new IOException(msg);
    }

    /// <inheritdoc/>
    protected override void InitializeUnique(byte[] frame)
    {
        StartAddress = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        NumberOfPoints = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4));
    }
}
