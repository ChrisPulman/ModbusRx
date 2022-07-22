// <copyright file="WriteMultipleRegistersRequest.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Net;
using ModbusRx.Data;
using ModbusRx.Unme.Common;

namespace ModbusRx.Message;

/// <summary>
/// WriteMultipleRegistersRequest.
/// </summary>
/// <seealso cref="ModbusRx.Message.IModbusRequest" />
public class WriteMultipleRegistersRequest : AbstractModbusMessageWithData<RegisterCollection>, IModbusRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WriteMultipleRegistersRequest"/> class.
    /// </summary>
    public WriteMultipleRegistersRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteMultipleRegistersRequest"/> class.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="data">The data.</param>
    public WriteMultipleRegistersRequest(byte slaveAddress, ushort startAddress, RegisterCollection data)
        : base(slaveAddress, Modbus.WriteMultipleRegisters)
    {
        StartAddress = startAddress;
        NumberOfPoints = (ushort)data?.Count!;
        ByteCount = (byte)(data.Count * 2);
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

    /// <inheritdoc/>
    public override int MinimumFrameSize => 7;

    /// <inheritdoc/>
    public override string ToString()
    {
        var msg = $"Write {NumberOfPoints} holding registers starting at address {StartAddress}.";
        return msg;
    }

    /// <inheritdoc/>
    public void ValidateResponse(IModbusMessage response)
    {
        var typedResponse = (WriteMultipleRegistersResponse)response;

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
        Data = new RegisterCollection(frame.Slice(7, ByteCount).ToArray());
    }
}
