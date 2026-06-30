// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ModbusRx.Message;
using ModbusRx.Utility;

namespace ModbusRx.UnitTests.Utility;

/// <summary>Tests the ModbusUtilityFixture behavior.</summary>
public class ModbusUtilityFixture
{
    /// <summary>Gets the ASCII bytes from empty.</summary>
    [TUnit.Core.Test]
    public void GetAsciiBytesFromEmpty()
    {
        byte[] emptyBytes = [];
        ushort[] emptyRegisters = [];
        Assert.Equal(emptyBytes, ModbusUtility.GetAsciiBytes(emptyBytes));
        Assert.Equal(emptyBytes, ModbusUtility.GetAsciiBytes(emptyRegisters));
    }

    /// <summary>Gets the ASCII bytes from bytes.</summary>
    [TUnit.Core.Test]
    public void GetAsciiBytesFromBytes()
    {
        byte[] buf = { 2, 5 };
        byte[] expectedResult = { 48, 50, 48, 53 };
        var result = ModbusUtility.GetAsciiBytes(buf);
        Assert.Equal(expectedResult, result);
    }

    /// <summary>Gets the ASCII bytes from ushorts.</summary>
    [TUnit.Core.Test]
    public void GetAsciiBytesFromUshorts()
    {
        ushort[] buf = { 300, 400 };
        byte[] expectedResult = { 48, 49, 50, 67, 48, 49, 57, 48 };
        var result = ModbusUtility.GetAsciiBytes(buf);
        Assert.Equal(expectedResult, result);
    }

    /// <summary>Hexadecimals to bytes.</summary>
    [TUnit.Core.Test]
    public void HexToBytes() => Assert.Equal([ 255], ModbusUtility.HexToBytes("FF"));

    /// <summary>Hexadecimals to bytes2.</summary>
    [TUnit.Core.Test]
    public void HexToBytes2() => Assert.Equal([ 204, 255], ModbusUtility.HexToBytes("CCFF"));

    /// <summary>Hexadecimals to bytes empty.</summary>
    [TUnit.Core.Test]
    public void HexToBytesEmpty() => Assert.Equal([], ModbusUtility.HexToBytes(string.Empty));

    /// <summary>Hexadecimals to bytes null.</summary>
    [TUnit.Core.Test]
    public void HexToBytesNull() => Assert.Throws<ArgumentNullException>(() => ModbusUtility.HexToBytes(null!));

    /// <summary>Hexadecimals to bytes odd.</summary>
    [TUnit.Core.Test]
    public void HexToBytesOdd() => Assert.Throws<FormatException>(() => ModbusUtility.HexToBytes("CCF"));

    /// <summary>Calculates the CRC.</summary>
    [TUnit.Core.Test]
    public void CalculateCrc()
    {
        var result = ModbusUtility.CalculateCrc([ 1, 1]);
        Assert.Equal([ 193, 224], result);
    }

    /// <summary>Calculates the CRC2.</summary>
    [TUnit.Core.Test]
    public void CalculateCrc2()
    {
        var result = ModbusUtility.CalculateCrc([ 2, 1, 5, 0]);
        Assert.Equal([ 83, 12], result);
    }

    /// <summary>Calculates the CRC empty.</summary>
    [TUnit.Core.Test]
    public void CalculateCrcEmpty() => Assert.Equal([ 255, 255], ModbusUtility.CalculateCrc([]));

    /// <summary>Calculates the CRC null.</summary>
    [TUnit.Core.Test]
    public void CalculateCrcNull() => Assert.Throws<ArgumentNullException>(() => ModbusUtility.CalculateCrc(null!));

    /// <summary>Calculates the LRC.</summary>
    [TUnit.Core.Test]
    public void CalculateLrc()
    {
        var a = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 1, 10);
        Assert.Equal(243, ModbusUtility.CalculateLrc([ 1, 1, 0, 1, 0, 10]));
    }

    /// <summary>Calculates the LRC2.</summary>
    [TUnit.Core.Test]
    public void CalculateLrc2()
    {
        // : 02 01 0000 0001 FC
        var a = new ReadCoilsInputsRequest(Modbus.ReadCoils, 2, 0, 1);
        Assert.Equal(252, ModbusUtility.CalculateLrc([ 2, 1, 0, 0, 0, 1]));
    }

    /// <summary>Calculates the LRC null.</summary>
    [TUnit.Core.Test]
    public void CalculateLrcNull() => Assert.Throws<ArgumentNullException>(() => ModbusUtility.CalculateLrc(null!));

    /// <summary>Calculates the LRC empty.</summary>
    [TUnit.Core.Test]
    public void CalculateLrcEmpty() => Assert.Equal(0, ModbusUtility.CalculateLrc([]));

    /// <summary>Networks the bytes to host u int16.</summary>
    [TUnit.Core.Test]
    public void NetworkBytesToHostUInt16() => Assert.Equal([ 1, 2], ModbusUtility.NetworkBytesToHostUInt16([ 0, 1, 0, 2]));

    /// <summary>Networks the bytes to host u int16 null.</summary>
    [TUnit.Core.Test]
    public void NetworkBytesToHostUInt16Null() => Assert.Throws<ArgumentNullException>(() => ModbusUtility.NetworkBytesToHostUInt16(null!));

    /// <summary>Networks the bytes to host u int16 odd number of bytes.</summary>
    [TUnit.Core.Test]
    public void NetworkBytesToHostUInt16OddNumberOfBytes() => Assert.Throws<FormatException>(() => ModbusUtility.NetworkBytesToHostUInt16([ 1]));

    /// <summary>Networks the bytes to host u int16 empty bytes.</summary>
    [TUnit.Core.Test]
    public void NetworkBytesToHostUInt16EmptyBytes() => Assert.Equal([], ModbusUtility.NetworkBytesToHostUInt16([]));

    /// <summary>Gets the double.</summary>
    [TUnit.Core.Test]
    public void GetDouble()
    {
        Assert.Equal(0.0, ModbusUtility.GetDouble(0, 0, 0, 0));
        Assert.Equal(1.0, ModbusUtility.GetDouble(16_368, 0, 0, 0));
        Assert.Equal(Math.PI, ModbusUtility.GetDouble(16_393, 8699, 21_572, 11_544));
        Assert.Equal(500.625, ModbusUtility.GetDouble(16_511, 18_944, 0, 0));
    }

    /// <summary>Gets the single.</summary>
    [TUnit.Core.Test]
    public void GetSingle()
    {
        Assert.Equal(0F, ModbusUtility.GetSingle(0, 0));
        Assert.Equal(1F, ModbusUtility.GetSingle(16_256, 0));
        Assert.Equal(9999999F, ModbusUtility.GetSingle(19_224, 38_527));
        Assert.Equal(500.625F, ModbusUtility.GetSingle(17_402, 20_480));
    }

    /// <summary>Gets the u int32.</summary>
    [TUnit.Core.Test]
    public void GetUInt32()
    {
        Assert.Equal(0U, ModbusUtility.GetUInt32(0, 0));
        Assert.Equal(1U, ModbusUtility.GetUInt32(0, 1));
        Assert.Equal(45U, ModbusUtility.GetUInt32(0, 45));
        Assert.Equal(65_536U, ModbusUtility.GetUInt32(1, 0));
    }
}
