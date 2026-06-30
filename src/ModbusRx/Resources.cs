// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive;
#else
namespace ModbusRx;
#endif

/// <summary>Provides Resources functionality.</summary>
internal static class Resources
{
    /// <summary>Executes the Acknowlege operation.</summary>
    public const string Acknowlege = "Specialized use in conjunction with programming commands.The server (or slave) has accepted the request and is processing it, but a long duration of time will be required to do so.This response is returned to prevent a timeout error from occurring in the client(or master). The client(or master) can next issue a Poll Program Complete message to determine if processing is completed.";

    /// <summary>Defines the Empty End Point value.</summary>
    public const string EmptyEndPoint = "Argument endPoint cannot be empty.";

    /// <summary>Defines the Gateway Path Unavailable value.</summary>
    public const string GatewayPathUnavailable = "Specialized use in conjunction with gateways, indicates that the gateway was unable to allocate an internal communication path from the input port to the output port for processing the request.Usually means that the gateway is misconfigured or overloaded.";

    /// <summary>Defines the Gateway Target Device Failed To Respond value.</summary>
    public const string GatewayTargetDeviceFailedToRespond = "Specialized use in conjunction with gateways, indicates that no response was obtained from the target device.Usually means that the device is not present on the network.";

    /// <summary>Defines the Hex Character Count Not Even value.</summary>
    public const string HexCharacterCountNotEven = "Hex string must have even number of characters.";

    /// <summary>Executes the Illegal Data Address operation.</summary>
    public const string IllegalDataAddress = "The data address received in the query is not an allowable address for the server (or slave). More specifically, the combination of reference number and transfer length is invalid.For a controller with 100 registers, the PDU addresses the first register as 0, and the last one as 99. If a request is submitted with a starting register address of 96 and a quantity of registers of 4, then this request will successfully operate(address-wise at least) on registers 96, 97, 98, 99. If a request is submitted with a starting register address of 96 and a quantity of registers of 5, then this request will fail with Exception Code 0x02 \"Illegal Data Address\" since it attempts to operate on registers 96, 97, 98, 99 and 100, and there is no register with address 100.";

    /// <summary>Executes the Illegal Data Value operation.</summary>
    public const string IllegalDataValue = "A value contained in the query data field is not an allowable value for server(or slave). This indicates a fault in the structure of the remainder of a complex request, such as that the implied length is incorrect.It specifically does NOT mean that a data item submitted for storage in a register has a value outside the expectation of the application program, since the MODBUS protocol is unaware of the significance of any particular value of any particular register.";

    /// <summary>Defines the Network Bytes Not Even value.</summary>
    public const string NetworkBytesNotEven = "Network bytes array length must be even.";

    /// <summary>Executes the Illegal Function operation.</summary>
    public const string IllegalFunction = "The function code received in the query is not an allowable action for the server (or slave). This may be because the function code is only applicable to newer devices, and was not implemented in the unit selected. It could also indicate that the server (or slave) is in the wrong state to process a request of this type, for example because it is unconfigured and is being asked to return register values.";

    /// <summary>Defines the Slave Device Busy value.</summary>
    public const string SlaveDeviceBusy = "Specialized use in conjunction with function codes 20 and 21 and reference type 6, to indicate that the extended file area is being processed and therefore the service is not available.";

    /// <summary>Executes the Slave Device Failure operation.</summary>
    public const string SlaveDeviceFailure = "An unrecoverable error occurred while the server (or slave) was attempting to perform the requested action.";

    /// <summary>Defines the Memory Parity Error value.</summary>
    public const string MemoryParityError = "Specialized use in conjunction with function codes 20 and 21 and reference type 6, to indicate that the extended file area has a parity error.";

    /// <summary>Executes the Acknowledge Slave Device Busy operation.</summary>
    public const string AcknowledgeSlaveDeviceBusy = "Specialized use in conjunction with programming commands. The server (or slave) has accepted the request and is processing it, but a long duration of time will be required to do so. This response is returned to prevent a timeout error from occurring in the client (or master). The client (or master) can next issue a Poll Program Complete message to determine if processing is completed.";

    /// <summary>Executes the Slave Negative Acknowledge operation.</summary>
    public const string SlaveNegativeAcknowledge = "Specialized use in conjunction with programming commands. The server (or slave) cannot perform the program function received in the query. This code is returned for an unsuccessful programming request using function code 13 or 14 decimal. The client (or master) should request diagnostic or error information from the server (or slave).";

    /// <summary>Defines the Timeout Not Supported value.</summary>
    public const string TimeoutNotSupported = "The compact framework UDP client does not support timeouts.";

    /// <summary>Defines the Udp Client Not Connected value.</summary>
    public const string UdpClientNotConnected = "UdpClientRx must be bound to a default remote host. Call the Connect method.";

    /// <summary>Defines the Wait Retry Greater Than Zero value.</summary>
    public const string WaitRetryGreaterThanZero = "WaitToRetryMilliseconds must be greater than 0.";

    /// <summary>Defines the Slave Exception Response Format value.</summary>
    public const string SlaveExceptionResponseFormat = "Function Code: {0}{1}Exception Code: {2} - {3}";

    /// <summary>Defines the Slave Exception Response Invalid Function Code value.</summary>
    public const string SlaveExceptionResponseInvalidFunctionCode = "Slave exception response invalid function code error.";

    /// <summary>Defines the Unknown value.</summary>
    public const string Unknown = "Unknown";
}
