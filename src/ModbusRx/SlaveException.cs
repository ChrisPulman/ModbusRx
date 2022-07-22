// <copyright file="SlaveException.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using ModbusRx.Message;

namespace ModbusRx;

/// <summary>
///     Represents slave errors that occur during communication.
/// </summary>
public class SlaveException : Exception
{
    private readonly SlaveExceptionResponse? _slaveExceptionResponse;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SlaveException" /> class.
    /// </summary>
    public SlaveException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SlaveException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public SlaveException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SlaveException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SlaveException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    internal SlaveException(SlaveExceptionResponse slaveExceptionResponse) =>
        _slaveExceptionResponse = slaveExceptionResponse;

    internal SlaveException(string message, SlaveExceptionResponse slaveExceptionResponse)
        : base(message) => _slaveExceptionResponse = slaveExceptionResponse;

    /// <summary>
    ///     Gets a message that describes the current exception.
    /// </summary>
    /// <value>
    ///     The error message that explains the reason for the exception, or an empty string.
    /// </value>
    public override string Message
    {
        get
        {
            var responseString = _slaveExceptionResponse is not null ? string.Concat(Environment.NewLine, _slaveExceptionResponse) : string.Empty;
            return string.Concat(base.Message, responseString);
        }
    }

    /// <summary>
    ///     Gets the response function code that caused the exception to occur, or 0.
    /// </summary>
    /// <value>The function code.</value>
    public byte FunctionCode =>
        _slaveExceptionResponse?.FunctionCode ?? 0;

    /// <summary>
    ///     Gets the slave exception code, or 0.
    /// </summary>
    /// <value>The slave exception code.</value>
    public byte SlaveExceptionCode =>
        _slaveExceptionResponse?.SlaveExceptionCode ?? 0;

    /// <summary>
    ///     Gets the slave address, or 0.
    /// </summary>
    /// <value>The slave address.</value>
    public byte SlaveAddress =>
        _slaveExceptionResponse?.SlaveAddress ?? 0;
}
