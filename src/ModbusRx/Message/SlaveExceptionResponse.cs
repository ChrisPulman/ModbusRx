// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Message;
#else
namespace ModbusRx.Message;
#endif

/// <summary>Provides SlaveExceptionResponse functionality.</summary>
/// <seealso cref="AbstractModbusMessage" />
/// <seealso cref="IModbusMessage" />
public class SlaveExceptionResponse : AbstractModbusMessage, IModbusMessage
{
    /// <summary>Stores the exception Messages value.</summary>
    private static readonly Dictionary<byte, string> _exceptionMessages = CreateExceptionMessages();

    /// <summary>Initializes a new instance of the <see cref="SlaveExceptionResponse"/> class.</summary>
    public SlaveExceptionResponse()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SlaveExceptionResponse"/> class.</summary>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="functionCode">The function code.</param>
    /// <param name="exceptionCode">The exception code.</param>
    public SlaveExceptionResponse(byte slaveAddress, byte functionCode, byte exceptionCode)
        : base(slaveAddress, functionCode) => SlaveExceptionCode = exceptionCode;

    /// <inheritdoc/>
    public override int MinimumFrameSize => 3;

    /// <summary>Gets or sets the slave exception code.</summary>
/// <value>The slave exception code.</value>
    public byte SlaveExceptionCode
    {
        get => MessageImpl.ExceptionCode!.Value;
        set => MessageImpl.ExceptionCode = value;
    }

    /// <summary>Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.</summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
        var msg = _exceptionMessages.TryGetValue(SlaveExceptionCode, out var value)
            ? value
            : Resources.Unknown;

        return $"Function Code: {FunctionCode}{Environment.NewLine}Exception Code: {SlaveExceptionCode} - {msg}";
    }

    /// <summary>Executes the Create Exception Messages operation.</summary>
    /// <returns>The result.</returns>
    internal static Dictionary<byte, string> CreateExceptionMessages() =>
        new(9)
        {
            { 1, Resources.IllegalFunction },
            { 2, Resources.IllegalDataAddress },
            { 3, Resources.IllegalDataValue },
            { 4, Resources.SlaveDeviceFailure },
            { 5, Resources.Acknowlege },
            { 6, Resources.SlaveDeviceBusy },
            { 8, Resources.MemoryParityError },
            { 10, Resources.GatewayPathUnavailable },
            { 11, Resources.GatewayTargetDeviceFailedToRespond }
        };

    /// <inheritdoc/>
    protected override void InitializeUnique(byte[] frame)
    {
        if (frame is null)
        {
            throw new ArgumentNullException(nameof(frame));
        }

        if (FunctionCode <= Modbus.ExceptionOffset)
        {
            throw new FormatException(Resources.SlaveExceptionResponseInvalidFunctionCode);
        }

        SlaveExceptionCode = frame[2];
    }
}
