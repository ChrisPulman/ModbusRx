// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx;

/// <summary>
///     An exception that provides the exception code that will be sent in response to an invalid Modbus request.
/// </summary>
#pragma warning disable RCS1194 // Implement exception constructors.
public class InvalidModbusRequestException : Exception
#pragma warning restore RCS1194 // Implement exception constructors.
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidModbusRequestException" /> class with a specified Modbus exception code.
    /// </summary>
    /// <param name="exceptionCode">The Modbus exception code to provide to the slave.</param>
    public InvalidModbusRequestException(byte exceptionCode)
        : this(GetMessage(exceptionCode), exceptionCode)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidModbusRequestException" /> class with a specified error message and Modbus exception code.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exceptionCode">The Modbus exception code to provide to the slave.</param>
    public InvalidModbusRequestException(string message, byte exceptionCode)
        : this(message, exceptionCode, null!)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidModbusRequestException" /> class with a specified Modbus exception code and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="exceptionCode">The Modbus exception code to provide to the slave.</param>
    /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
    public InvalidModbusRequestException(byte exceptionCode, Exception innerException)
        : this(GetMessage(exceptionCode), exceptionCode, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidModbusRequestException" /> class with a specified Modbus exception code and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exceptionCode">The Modbus exception code to provide to the slave.</param>
    /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
    public InvalidModbusRequestException(string message, byte exceptionCode, Exception innerException)
        : base(message, innerException) => ExceptionCode = exceptionCode;

    /// <summary>
    ///     Gets the Modbus exception code to provide to the slave.
    /// </summary>
    public byte ExceptionCode { get; }

    private static string GetMessage(byte exceptionCode) =>
        $"Modbus exception code {exceptionCode}.";
}
