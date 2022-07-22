// <copyright file="DiagnosticsRequestResponse.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics;
using System.Net;
using ModbusRx.Data;
using ModbusRx.Unme.Common;

namespace ModbusRx.Message;

internal class DiagnosticsRequestResponse : AbstractModbusMessageWithData<RegisterCollection>, IModbusMessage
{
    public DiagnosticsRequestResponse()
    {
    }

    public DiagnosticsRequestResponse(ushort subFunctionCode, byte slaveAddress, RegisterCollection data)
        : base(slaveAddress, Modbus.Diagnostics)
    {
        SubFunctionCode = subFunctionCode;
        Data = data;
    }

    public override int MinimumFrameSize => 6;

    public ushort SubFunctionCode
    {
        get => MessageImpl.SubFunctionCode!.Value;
        set => MessageImpl.SubFunctionCode = value;
    }

    public override string ToString()
    {
        Debug.Assert(
            SubFunctionCode == Modbus.DiagnosticsReturnQueryData,
            "Need to add support for additional sub-function.");

        return $"Diagnostics message, sub-function return query data - {Data}.";
    }

    protected override void InitializeUnique(byte[] frame)
    {
        SubFunctionCode = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        Data = new RegisterCollection(frame.Slice(4, 2).ToArray());
    }
}
