// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Xunit;

namespace ModbusRx.UnitTests;

/// <summary>
/// InvalidModbusRequestExceptionFixture.
/// </summary>
public class InvalidModbusRequestExceptionFixture
{
    /// <summary>
    /// Constructors the with exception code.
    /// </summary>
    [TUnit.Core.Test]
    public void ConstructorWithExceptionCode()
    {
        var e = new InvalidModbusRequestException(Modbus.SlaveDeviceBusy);
        Assert.Equal($"Modbus exception code {Modbus.SlaveDeviceBusy}.", e.Message);
        Assert.Equal(Modbus.SlaveDeviceBusy, e.ExceptionCode);
        Assert.Null(e.InnerException);
    }

    /// <summary>
    /// Constructors the with exception code and inner exception.
    /// </summary>
    [TUnit.Core.Test]
    public void ConstructorWithExceptionCodeAndInnerException()
    {
        var inner = new IOException("Bar");
        var e = new InvalidModbusRequestException(42, inner);
        Assert.Equal("Modbus exception code 42.", e.Message);
        Assert.Equal(42, e.ExceptionCode);
        Assert.Same(inner, e.InnerException);
    }

    /// <summary>
    /// Constructors the with message and exception code.
    /// </summary>
    [TUnit.Core.Test]
    public void ConstructorWithMessageAndExceptionCode()
    {
        var e = new InvalidModbusRequestException("Hello World", Modbus.IllegalFunction);
        Assert.Equal("Hello World", e.Message);
        Assert.Equal(Modbus.IllegalFunction, e.ExceptionCode);
        Assert.Null(e.InnerException);
    }

    /// <summary>
    /// Constructors the with custom message and slave exception response.
    /// </summary>
    [TUnit.Core.Test]
    public void ConstructorWithCustomMessageAndSlaveExceptionResponse()
    {
        var inner = new IOException("Bar");
        var e = new InvalidModbusRequestException("Hello World", Modbus.IllegalDataAddress, inner);
        Assert.Equal("Hello World", e.Message);
        Assert.Equal(Modbus.IllegalDataAddress, e.ExceptionCode);
        Assert.Same(inner, e.InnerException);
    }
}
