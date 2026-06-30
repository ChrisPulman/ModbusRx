// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Net;

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Message;
#else
namespace ModbusRx.Message;
#endif

/// <summary>Provides WriteMultipleCoilsResponse functionality.</summary>
/// <seealso cref="AbstractModbusMessage" />
/// <seealso cref="IModbusMessage" />
public class WriteMultipleCoilsResponse : AbstractModbusMessage, IModbusMessage
{
    /// <summary>Initializes a new instance of the <see cref="WriteMultipleCoilsResponse"/> class.</summary>
    public WriteMultipleCoilsResponse()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WriteMultipleCoilsResponse"/> class.</summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    public WriteMultipleCoilsResponse(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        : base(slaveAddress, Modbus.WriteMultipleCoils)
    {
        StartAddress = startAddress;
        NumberOfPoints = numberOfPoints;
    }

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
                throw new ArgumentOutOfRangeException("NumberOfPoints", msg);
            }

            MessageImpl.NumberOfPoints = value;
        }
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

    /// <inheritdoc/>
    public override string ToString() =>
        $"Wrote {NumberOfPoints} coils starting at address {StartAddress}.";

    /// <inheritdoc/>
    protected override void InitializeUnique(byte[] frame)
    {
        StartAddress = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        NumberOfPoints = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4));
    }
}
