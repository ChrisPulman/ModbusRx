// <copyright file="WriteSingleRegisterRequestResponse.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics;
using System.Net;
using ModbusRx.Data;

namespace ModbusRx.Message;

/// <summary>
/// WriteSingleRegisterRequestResponse.
/// </summary>
public class WriteSingleRegisterRequestResponse : AbstractModbusMessageWithData<RegisterCollection>, IModbusRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WriteSingleRegisterRequestResponse"/> class.
    /// </summary>
    public WriteSingleRegisterRequestResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteSingleRegisterRequestResponse"/> class.
    /// </summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="registerValue">The register value.</param>
    public WriteSingleRegisterRequestResponse(byte slaveAddress, ushort startAddress, ushort registerValue)
        : base(slaveAddress, Modbus.WriteSingleRegister)
    {
        StartAddress = startAddress;
        Data = new RegisterCollection(registerValue);
    }

    /// <summary>
    /// Gets the minimum size of the frame.
    /// </summary>
    /// <value>
    /// The minimum size of the frame.
    /// </value>
    public override int MinimumFrameSize => 6;

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
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        Debug.Assert(Data is not null, "Argument Data cannot be null.");
        Debug.Assert(Data?.Count == 1, "Data should have a count of 1.");

        var msg = $"Write single holding register {Data![0]} at address {StartAddress}.";
        return msg;
    }

    /// <summary>
    /// Validate the specified response against the current request.
    /// </summary>
    /// <param name="response">The Modbus message.</param>
    public void ValidateResponse(IModbusMessage response)
    {
        var typedResponse = (WriteSingleRegisterRequestResponse)response;

        if (StartAddress != typedResponse?.StartAddress)
        {
            var msg = $"Unexpected start address in response. Expected {StartAddress}, received {typedResponse?.StartAddress}.";
            throw new IOException(msg);
        }

        if (Data.First() != typedResponse.Data.First())
        {
            var msg = $"Unexpected data in response. Expected {Data.First()}, received {typedResponse.Data.First()}.";
            throw new IOException(msg);
        }
    }

    /// <summary>
    /// Initializes the unique.
    /// </summary>
    /// <param name="frame">The frame.</param>
    protected override void InitializeUnique(byte[] frame)
    {
        StartAddress = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        Data = new RegisterCollection((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4)));
    }
}
