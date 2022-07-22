// <copyright file="WriteMultipleRegistersResponse.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Net;

namespace ModbusRx.Message;

/// <summary>
/// WriteMultipleRegistersResponse.
/// </summary>
/// <seealso cref="ModbusRx.Message.AbstractModbusMessage" />
/// <seealso cref="ModbusRx.Message.IModbusMessage" />
public class WriteMultipleRegistersResponse : AbstractModbusMessage, IModbusMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WriteMultipleRegistersResponse"/> class.
    /// </summary>
    public WriteMultipleRegistersResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteMultipleRegistersResponse"/> class.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    public WriteMultipleRegistersResponse(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        : base(slaveAddress, Modbus.WriteMultipleRegisters)
    {
        StartAddress = startAddress;
        NumberOfPoints = numberOfPoints;
    }

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

    /// <summary>
    /// Gets the minimum size of the frame.
    /// </summary>
    /// <value>
    /// The minimum size of the frame.
    /// </value>
    public override int MinimumFrameSize => 6;

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString() =>
        $"Wrote {NumberOfPoints} holding registers starting at address {StartAddress}.";

    /// <summary>
    /// Initializes the unique.
    /// </summary>
    /// <param name="frame">The frame.</param>
    protected override void InitializeUnique(byte[] frame)
    {
        StartAddress = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        NumberOfPoints = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4));
    }
}
