// <copyright file="WriteMultipleCoilsRequest.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Net;
using ModbusRx.Data;
using ModbusRx.Unme.Common;

namespace ModbusRx.Message;

/// <summary>
///     Write Multiple Coils request.
/// </summary>
public class WriteMultipleCoilsRequest : AbstractModbusMessageWithData<DiscreteCollection>, IModbusRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WriteMultipleCoilsRequest"/> class.
    ///     Write Multiple Coils request.
    /// </summary>
    public WriteMultipleCoilsRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteMultipleCoilsRequest" /> class.
    /// Write Multiple Coils request.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="data">The data.</param>
    public WriteMultipleCoilsRequest(byte slaveAddress, ushort startAddress, DiscreteCollection data)
        : base(slaveAddress, Modbus.WriteMultipleCoils)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        StartAddress = startAddress;
        NumberOfPoints = (ushort)data.Count;
        ByteCount = (byte)((data.Count + 7) / 8);
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
                throw new ArgumentOutOfRangeException("NumberOfPoints", msg);
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

    /// <inheritdoc/>
    public override int MinimumFrameSize => 7;

    /// <inheritdoc/>
    public override string ToString() =>
        $"Write {NumberOfPoints} coils starting at address {StartAddress}.";

    /// <inheritdoc/>
    public void ValidateResponse(IModbusMessage response)
    {
        var typedResponse = (WriteMultipleCoilsResponse)response;

        if (StartAddress != typedResponse?.StartAddress)
        {
            var msg = $"Unexpected start address in response. Expected {StartAddress}, received {typedResponse?.StartAddress}.";
            throw new IOException(msg);
        }

        if (NumberOfPoints != typedResponse.NumberOfPoints)
        {
            var msg = $"Unexpected number of points in response. Expected {NumberOfPoints}, received {typedResponse.NumberOfPoints}.";
            throw new IOException(msg);
        }
    }

    /// <inheritdoc/>
    protected override void InitializeUnique(byte[] frame)
    {
        if (frame?.Length < MinimumFrameSize + frame![6])
        {
            throw new FormatException("Message frame does not contain enough bytes.");
        }

        StartAddress = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        NumberOfPoints = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4));
        ByteCount = frame[6];
        Data = new DiscreteCollection(frame.Slice(7, ByteCount).ToArray());
    }
}
