// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ModbusRx.Message;

namespace ModbusRx.UnitTests.Message;

/// <summary>Tests the ModbusMessageImplFixture behavior.</summary>
public class ModbusMessageImplFixture
{
    /// <summary>Modbuses the message ctor initializes properties.</summary>
    [TUnit.Core.Test]
    public void ModbusMessageCtorInitializesProperties()
    {
        var messageImpl = new ModbusMessageImpl(5, Modbus.ReadCoils);
        Assert.Equal(5, messageImpl.SlaveAddress);
        Assert.Equal(Modbus.ReadCoils, messageImpl.FunctionCode);
    }

    /// <summary>Initializes this instance.</summary>
    [TUnit.Core.Test]
    public void Initialize()
    {
        var messageImpl = new ModbusMessageImpl();
        messageImpl.Initialize([ 1, 2, 9, 9, 9, 9]);
        Assert.Equal(1, messageImpl.SlaveAddress);
        Assert.Equal(2, messageImpl.FunctionCode);
    }

    /// <summary>Checcks the initialize frame null.</summary>
    [TUnit.Core.Test]
    public void ChecckInitializeFrameNull()
    {
        var messageImpl = new ModbusMessageImpl();
        _ = Assert.Throws<ArgumentNullException>(() => messageImpl.Initialize(null!));
    }

    /// <summary>Initializes the invalid frame.</summary>
    [TUnit.Core.Test]
    public void InitializeInvalidFrame()
    {
        var messageImpl = new ModbusMessageImpl();
        _ = Assert.Throws<FormatException>(() => messageImpl.Initialize([ 1]));
    }

    /// <summary>Protocols the data unit.</summary>
    [TUnit.Core.Test]
    public void ProtocolDataUnit()
    {
        var messageImpl = new ModbusMessageImpl(11, Modbus.ReadCoils);
        byte[] expectedResult = { Modbus.ReadCoils };
        Assert.Equal(expectedResult, messageImpl.ProtocolDataUnit);
    }

    /// <summary>Messages the frame.</summary>
    [TUnit.Core.Test]
    public void MessageFrame()
    {
        var messageImpl = new ModbusMessageImpl(11, Modbus.ReadHoldingRegisters);
        byte[] expectedMessageFrame = { 11, Modbus.ReadHoldingRegisters };
        Assert.Equal(expectedMessageFrame, messageImpl.MessageFrame);
    }
}
