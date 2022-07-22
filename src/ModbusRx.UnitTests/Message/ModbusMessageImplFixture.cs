// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// ModbusMessageImplFixture.
/// </summary>
public class ModbusMessageImplFixture
{
    /// <summary>
    /// Modbuses the message ctor initializes properties.
    /// </summary>
    [Fact]
    public void ModbusMessageCtorInitializesProperties()
    {
        var messageImpl = new ModbusMessageImpl(5, Modbus.ReadCoils);
        Assert.Equal(5, messageImpl.SlaveAddress);
        Assert.Equal(Modbus.ReadCoils, messageImpl.FunctionCode);
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    [Fact]
    public void Initialize()
    {
        var messageImpl = new ModbusMessageImpl();
        messageImpl.Initialize(new byte[] { 1, 2, 9, 9, 9, 9 });
        Assert.Equal(1, messageImpl.SlaveAddress);
        Assert.Equal(2, messageImpl.FunctionCode);
    }

    /// <summary>
    /// Checcks the initialize frame null.
    /// </summary>
    [Fact]
    public void ChecckInitializeFrameNull()
    {
        var messageImpl = new ModbusMessageImpl();
        Assert.Throws<ArgumentNullException>(() => messageImpl.Initialize(null!));
    }

    /// <summary>
    /// Initializes the invalid frame.
    /// </summary>
    [Fact]
    public void InitializeInvalidFrame()
    {
        var messageImpl = new ModbusMessageImpl();
        Assert.Throws<FormatException>(() => messageImpl.Initialize(new byte[] { 1 }));
    }

    /// <summary>
    /// Protocols the data unit.
    /// </summary>
    [Fact]
    public void ProtocolDataUnit()
    {
        var messageImpl = new ModbusMessageImpl(11, Modbus.ReadCoils);
        byte[] expectedResult = { Modbus.ReadCoils };
        Assert.Equal(expectedResult, messageImpl.ProtocolDataUnit);
    }

    /// <summary>
    /// Messages the frame.
    /// </summary>
    [Fact]
    public void MessageFrame()
    {
        var messageImpl = new ModbusMessageImpl(11, Modbus.ReadHoldingRegisters);
        byte[] expectedMessageFrame = { 11, Modbus.ReadHoldingRegisters };
        Assert.Equal(expectedMessageFrame, messageImpl.MessageFrame);
    }
}
