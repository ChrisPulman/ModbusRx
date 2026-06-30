// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Unme.Common;
#else
using ModbusRx.Unme.Common;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Message;
#else
namespace ModbusRx.Message;
#endif

/// <summary>Provides ReadHoldingInputRegistersResponse functionality.</summary>
/// <seealso cref="IModbusMessage" />
public class ReadHoldingInputRegistersResponse : AbstractModbusMessageWithData<RegisterCollection>, IModbusMessage
{
    /// <summary>Initializes a new instance of the <see cref="ReadHoldingInputRegistersResponse"/> class.</summary>
    public ReadHoldingInputRegistersResponse()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ReadHoldingInputRegistersResponse"/> class.</summary>
    /// <param name="functionCode">The function code.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="data">The data.</param>
    /// <exception cref="System.ArgumentNullException">data.</exception>
    public ReadHoldingInputRegistersResponse(byte functionCode, byte slaveAddress, RegisterCollection data)
        : base(slaveAddress, functionCode)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        ByteCount = data.ByteCount;
        Data = data;
    }

    /// <summary>Gets or sets the byte count.</summary>
/// <value>The byte count.</value>
    public byte ByteCount
    {
        get => MessageImpl.ByteCount!.Value;
        set => MessageImpl.ByteCount = value;
    }

    /// <inheritdoc/>
    public override int MinimumFrameSize => 3;

    /// <inheritdoc/>
    public override string ToString() =>
        $"Read {Data.Count} {(FunctionCode == Modbus.ReadHoldingRegisters ? "holding" : "input")} registers.";

    /// <inheritdoc/>
    protected override void InitializeUnique(byte[] frame)
    {
        if (frame?.Length < MinimumFrameSize + frame![2])
        {
            throw new FormatException("Message frame does not contain enough bytes.");
        }

        ByteCount = frame[2];
        if (ByteCount % 2 != 0)
        {
            throw new FormatException("Byte count must be even for register data.");
        }

        var data = new byte[ByteCount];
        Array.Copy(frame, 3, data, 0, data.Length);
        Data = new(data);
    }
}
