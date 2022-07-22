// <copyright file="ModbusMessageFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using System.Reflection;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// ModbusMessageFixture.
/// </summary>
public class ModbusMessageFixture
{
    /// <summary>
    /// Protocols the data unit read coils request.
    /// </summary>
    [Fact]
    public void ProtocolDataUnitReadCoilsRequest()
    {
        AbstractModbusMessage message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 100, 9);
        byte[] expectedResult = { Modbus.ReadCoils, 0, 100, 0, 9 };
        Assert.Equal(expectedResult, message.ProtocolDataUnit);
    }

    /// <summary>
    /// Messages the frame read coils request.
    /// </summary>
    [Fact]
    public void MessageFrameReadCoilsRequest()
    {
        AbstractModbusMessage message = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 2, 3);
        byte[] expectedMessageFrame = { 1, Modbus.ReadCoils, 0, 2, 0, 3 };
        Assert.Equal(expectedMessageFrame, message.MessageFrame);
    }

    /// <summary>
    /// Modbuses the message to string overriden.
    /// </summary>
    [Fact]
    public void ModbusMessageToStringOverriden()
    {
        var messageTypes = from message in typeof(AbstractModbusMessage).GetTypeInfo().Assembly.GetTypes()
                           let typeInfo = message.GetTypeInfo()
                           where !typeInfo.IsAbstract && typeInfo.IsSubclassOf(typeof(AbstractModbusMessage))
                           select message;

        foreach (var messageType in messageTypes)
        {
            Assert.NotNull(
                messageType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        }
    }

    /// <summary>
    /// Asserts the modbus message properties are equal.
    /// </summary>
    /// <param name="obj1">The obj1.</param>
    /// <param name="obj2">The obj2.</param>
    internal static void AssertModbusMessagePropertiesAreEqual(IModbusMessage obj1, IModbusMessage obj2)
    {
        Assert.Equal(obj1.FunctionCode, obj2.FunctionCode);
        Assert.Equal(obj1.SlaveAddress, obj2.SlaveAddress);
        Assert.Equal(obj1.MessageFrame, obj2.MessageFrame);
        Assert.Equal(obj1.ProtocolDataUnit, obj2.ProtocolDataUnit);
    }
}
