// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive;
#else
namespace ModbusRx;
#endif

/// <summary>Defines constants related to the Modbus protocol.</summary>
internal static class Modbus
{
    /// <summary>Supported function codes.</summary>
    public const byte ReadCoils = 1;

    /// <summary>Defines the Read Inputs value.</summary>
    public const byte ReadInputs = 2;

    /// <summary>Defines the Read Holding Registers value.</summary>
    public const byte ReadHoldingRegisters = 3;

    /// <summary>Defines the Read Input Registers value.</summary>
    public const byte ReadInputRegisters = 4;

    /// <summary>Defines the Write Single Coil value.</summary>
    public const byte WriteSingleCoil = 5;

    /// <summary>Defines the Write Single Register value.</summary>
    public const byte WriteSingleRegister = 6;

    /// <summary>Defines the Diagnostics value.</summary>
    public const byte Diagnostics = 8;

    /// <summary>Defines the Diagnostics Return Query Data value.</summary>
    public const ushort DiagnosticsReturnQueryData = 0;

    /// <summary>Defines the Write Multiple Coils value.</summary>
    public const byte WriteMultipleCoils = 15;

    /// <summary>Defines the Write Multiple Registers value.</summary>
    public const byte WriteMultipleRegisters = 16;

    /// <summary>Defines the Read Write Multiple Registers value.</summary>
    public const byte ReadWriteMultipleRegisters = 23;

    /// <summary>Defines the Maximum Discrete Request Response Size value.</summary>
    public const int MaximumDiscreteRequestResponseSize = 2040;

    /// <summary>Defines the Maximum Register Request Response Size value.</summary>
    public const int MaximumRegisterRequestResponseSize = 127;

    /// <summary>Modbus slave exception offset that is added to the function code, to flag an exception.</summary>
    public const byte ExceptionOffset = 128;

    /// <summary>Modbus slave exception codes.</summary>
    public const byte IllegalFunction = 1;

    /// <summary>Defines the Illegal Data Address value.</summary>
    public const byte IllegalDataAddress = 2;

    /// <summary>Defines the Acknowledge value.</summary>
    public const byte Acknowledge = 5;

    /// <summary>Defines the Slave Device Busy value.</summary>
    public const byte SlaveDeviceBusy = 6;

    /// <summary>Default setting for number of retries for IO operations.</summary>
    public const int DefaultRetries = 3;

    /// <summary>Default number of milliseconds to wait after encountering an ACKNOWLEGE or SLAVE DEVIC BUSY slave exception response.</summary>
    public const int DefaultWaitToRetryMilliseconds = 250;

    /// <summary>Default setting for IO timeouts in milliseconds.</summary>
    public const int DefaultTimeout = 1000;

    /// <summary>Smallest supported message frame size (sans checksum).</summary>
    public const int MinimumFrameSize = 2;

    /// <summary>Defines the Coil On value.</summary>
    public const ushort CoilOn = 0xFF00;

    /// <summary>Defines the Coil Off value.</summary>
    public const ushort CoilOff = 0x0000;

    /// <summary>IP slaves should be addressed by IP.</summary>
    public const byte DefaultIpSlaveUnitId = 0;

    /// <summary>An existing connection was forcibly closed by the remote host.</summary>
    public const int ConnectionResetByPeer = 10_054;

    /// <summary>Existing socket connection is being closed.</summary>
    public const int WSACancelBlockingCall = 10_004;

    /// <summary>Used by the ASCII tranport to indicate end of message.</summary>
    public const string NewLine = "\r\n";
}
