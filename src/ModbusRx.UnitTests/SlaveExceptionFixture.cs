// <copyright file="SlaveExceptionFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests;

/// <summary>
/// SlaveExceptionFixture.
/// </summary>
public class SlaveExceptionFixture
{
    /// <summary>
    /// Empties the constructor.
    /// </summary>
    [Fact]
    public void EmptyConstructor()
    {
        var e = new SlaveException();
        Assert.Equal($"Exception of type '{typeof(SlaveException).FullName}' was thrown.", e.Message);
        Assert.Equal(0, e.SlaveAddress);
        Assert.Equal(0, e.FunctionCode);
        Assert.Equal(0, e.SlaveExceptionCode);
        Assert.Null(e.InnerException);
    }

    /// <summary>
    /// Constructors the with message.
    /// </summary>
    [Fact]
    public void ConstructorWithMessage()
    {
        var e = new SlaveException("Hello World");
        Assert.Equal("Hello World", e.Message);
        Assert.Equal(0, e.SlaveAddress);
        Assert.Equal(0, e.FunctionCode);
        Assert.Equal(0, e.SlaveExceptionCode);
        Assert.Null(e.InnerException);
    }

    /// <summary>
    /// Constructors the with message and inner exception.
    /// </summary>
    [Fact]
    public void ConstructorWithMessageAndInnerException()
    {
        var inner = new IOException("Bar");
        var e = new SlaveException("Foo", inner);
        Assert.Equal("Foo", e.Message);
        Assert.Same(inner, e.InnerException);
        Assert.Equal(0, e.SlaveAddress);
        Assert.Equal(0, e.FunctionCode);
        Assert.Equal(0, e.SlaveExceptionCode);
    }

    /// <summary>
    /// Constructors the with slave exception response.
    /// </summary>
    [Fact]
    public void ConstructorWithSlaveExceptionResponse()
    {
        var response = new SlaveExceptionResponse(12, Modbus.ReadCoils, 1);
        var e = new SlaveException(response);

        Assert.Equal(12, e.SlaveAddress);
        Assert.Equal(Modbus.ReadCoils, e.FunctionCode);
        Assert.Equal(1, e.SlaveExceptionCode);
        Assert.Null(e.InnerException);

        Assert.Equal(
            $@"Exception of type '{typeof(SlaveException).FullName}' was thrown.{Environment.NewLine}Function Code: {response.FunctionCode}{Environment.NewLine}Exception Code: {response.SlaveExceptionCode} - {Resources.IllegalFunction}",
            e.Message);
    }

    /// <summary>
    /// Constructors the with custom message and slave exception response.
    /// </summary>
    [Fact]
    public void ConstructorWithCustomMessageAndSlaveExceptionResponse()
    {
        var response = new SlaveExceptionResponse(12, Modbus.ReadCoils, 2);
        var customMessage = "custom message";
        var e = new SlaveException(customMessage, response);

        Assert.Equal(12, e.SlaveAddress);
        Assert.Equal(Modbus.ReadCoils, e.FunctionCode);
        Assert.Equal(2, e.SlaveExceptionCode);
        Assert.Null(e.InnerException);

        Assert.Equal(
            $@"{customMessage}{Environment.NewLine}Function Code: {response.FunctionCode}{Environment.NewLine}Exception Code: {response.SlaveExceptionCode} - {Resources.IllegalDataAddress}",
            e.Message);
    }
}
