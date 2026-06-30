// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.IO;
#else
using ModbusRx.IO;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Message;
#else
using ModbusRx.Message;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Device;
#else
namespace ModbusRx.Device;
#endif

/// <summary>Modbus slave device.</summary>
public abstract class ModbusSlave : ModbusDevice
{
    /// <summary>Initializes a new instance of the Modbus Slave class.</summary>
    /// <param name="unitId">The unit Id value.</param>
    /// <param name="transport">The transport value.</param>
    internal ModbusSlave(byte unitId, ModbusTransport transport)
        : base(transport)
    {
        DataStore = DataStoreFactory.CreateDefaultDataStore();
        UnitId = unitId;
    }

    /// <summary>Raised when a Modbus slave receives a request, before processing request function.</summary>
    /// <exception cref="InvalidModbusRequestException">The Modbus request was invalid, and an error response the specified exception should be sent.</exception>
    public event EventHandler<ModbusSlaveRequestEventArgs>? ModbusSlaveRequestReceived;

    /// <summary>Raised when a Modbus slave receives a write request, after processing the write portion of the function.</summary>
    /// <remarks>For Read/Write Multiple registers (function code 23), this method is raised after writing and before reading.</remarks>
    public event EventHandler<ModbusSlaveRequestEventArgs>? WriteComplete;

    /// <summary>Gets or sets the data store.</summary>
    public DataStore DataStore { get; set; }

    /// <summary>Gets or sets the unit ID.</summary>
    public byte UnitId { get; set; }

    /// <summary>Start slave listening for requests.</summary>
    /// <returns>A Task.</returns>
    public abstract Task ListenAsync();

    /// <summary>Defines the Read Discretes value.</summary>
    /// <param name="request">The read request.</param>
    /// <param name="dataStore">The data store to read from.</param>
    /// <param name="dataSource">The source collection.</param>
    /// <returns>The result.</returns>
    internal static ReadCoilsInputsResponse ReadDiscretes(
        ReadCoilsInputsRequest request,
        DataStore dataStore,
        ModbusDataCollection<bool> dataSource)
    {
        var data = DataStore.ReadData<DiscreteCollection, bool>(
            dataStore,
            dataSource,
            request.StartAddress,
            request.NumberOfPoints,
            dataStore.SyncRoot);

        return new ReadCoilsInputsResponse(
            request.FunctionCode,
            request.SlaveAddress,
            data.ByteCount,
            data);
    }

    /// <summary>Defines the Read Registers value.</summary>
    /// <param name="request">The read request.</param>
    /// <param name="dataStore">The data store to read from.</param>
    /// <param name="dataSource">The source collection.</param>
    /// <returns>The result.</returns>
    internal static ReadHoldingInputRegistersResponse ReadRegisters(
        ReadHoldingInputRegistersRequest? request,
        DataStore dataStore,
        ModbusDataCollection<ushort> dataSource)
    {
        var data = DataStore.ReadData<RegisterCollection, ushort>(
            dataStore,
            dataSource,
            request!.StartAddress,
            request.NumberOfPoints,
            dataStore.SyncRoot);

        return new ReadHoldingInputRegistersResponse(
            request.FunctionCode,
            request.SlaveAddress,
            data);
    }

    /// <summary>Defines the Write Single Coil value.</summary>
    /// <param name="request">The write request.</param>
    /// <param name="dataStore">The data store to write to.</param>
    /// <param name="dataSource">The destination collection.</param>
    /// <returns>The result.</returns>
    internal static WriteSingleCoilRequestResponse WriteSingleCoil(
        WriteSingleCoilRequestResponse request,
        DataStore dataStore,
        ModbusDataCollection<bool> dataSource)
    {
        DataStore.WriteData(
            dataStore,
            new DiscreteCollection(request.Data[0] == Modbus.CoilOn),
            dataSource,
            request.StartAddress,
            dataStore.SyncRoot);

        return request;
    }

    /// <summary>Defines the Write Multiple Coils value.</summary>
    /// <param name="request">The write request.</param>
    /// <param name="dataStore">The data store to write to.</param>
    /// <param name="dataSource">The destination collection.</param>
    /// <returns>The result.</returns>
    internal static WriteMultipleCoilsResponse WriteMultipleCoils(
        WriteMultipleCoilsRequest request,
        DataStore dataStore,
        ModbusDataCollection<bool> dataSource)
    {
        var coils = new bool[request.NumberOfPoints];
        for (var i = 0; i < coils.Length; i++)
        {
            coils[i] = request.Data[i];
        }

        DataStore.WriteData(
            dataStore,
            coils,
            dataSource,
            request.StartAddress,
            dataStore.SyncRoot);

        return new(
            request.SlaveAddress,
            request.StartAddress,
            request.NumberOfPoints);
    }

    /// <summary>Defines the Write Single Register value.</summary>
    /// <param name="request">The write request.</param>
    /// <param name="dataStore">The data store to write to.</param>
    /// <param name="dataSource">The destination collection.</param>
    /// <returns>The result.</returns>
    internal static WriteSingleRegisterRequestResponse WriteSingleRegister(
        WriteSingleRegisterRequestResponse request,
        DataStore dataStore,
        ModbusDataCollection<ushort> dataSource)
    {
        DataStore.WriteData(
            dataStore,
            request.Data,
            dataSource,
            request.StartAddress,
            dataStore.SyncRoot);

        return request;
    }

    /// <summary>Defines the Write Multiple Registers value.</summary>
    /// <param name="request">The write request.</param>
    /// <param name="dataStore">The data store to write to.</param>
    /// <param name="dataSource">The destination collection.</param>
    /// <returns>The result.</returns>
    internal static WriteMultipleRegistersResponse WriteMultipleRegisters(
        WriteMultipleRegistersRequest? request,
        DataStore dataStore,
        ModbusDataCollection<ushort> dataSource)
    {
        DataStore.WriteData(
            dataStore,
            request!.Data,
            dataSource,
            request.StartAddress,
            dataStore.SyncRoot);

        return new(
            request.SlaveAddress,
            request.StartAddress,
            request.NumberOfPoints);
    }

    /// <summary>Executes the Apply Request operation.</summary>
    /// <param name="request">The request value.</param>
    /// <returns>The result.</returns>
    internal IModbusMessage ApplyRequest(IModbusMessage request)
    {
        IModbusMessage response;

        try
        {
            Debug.WriteLine(request.ToString());
            var eventArgs = new ModbusSlaveRequestEventArgs(request);
            ModbusSlaveRequestReceived?.Invoke(this, eventArgs);

            switch (request.FunctionCode)
            {
                case Modbus.ReadCoils:
                    {
                        response = ReadDiscretes(
                                            (ReadCoilsInputsRequest)request,
                                            DataStore,
                                            DataStore.CoilDiscretes);
                        break;
                    }

                case Modbus.ReadInputs:
                    {
                        response = ReadDiscretes(
                                            (ReadCoilsInputsRequest)request,
                                            DataStore,
                                            DataStore.InputDiscretes);
                        break;
                    }

                case Modbus.ReadHoldingRegisters:
                    {
                        response = ReadRegisters(
                                            (ReadHoldingInputRegistersRequest)request,
                                            DataStore,
                                            DataStore.HoldingRegisters);
                        break;
                    }

                case Modbus.ReadInputRegisters:
                    {
                        response = ReadRegisters(
                                            (ReadHoldingInputRegistersRequest)request,
                                            DataStore,
                                            DataStore.InputRegisters);
                        break;
                    }

                case Modbus.Diagnostics:
                    {
                        response = request;
                        break;
                    }

                case Modbus.WriteSingleCoil:
                    {
                        response = WriteSingleCoil(
                                            (WriteSingleCoilRequestResponse)request,
                                            DataStore,
                                            DataStore.CoilDiscretes);
                        WriteComplete?.Invoke(this, eventArgs);
                        break;
                    }

                case Modbus.WriteSingleRegister:
                    {
                        response = WriteSingleRegister(
                                            (WriteSingleRegisterRequestResponse)request,
                                            DataStore,
                                            DataStore.HoldingRegisters);
                        WriteComplete?.Invoke(this, eventArgs);
                        break;
                    }

                case Modbus.WriteMultipleCoils:
                    {
                        response = WriteMultipleCoils(
                                            (WriteMultipleCoilsRequest)request,
                                            DataStore,
                                            DataStore.CoilDiscretes);
                        WriteComplete?.Invoke(this, eventArgs);
                        break;
                    }

                case Modbus.WriteMultipleRegisters:
                    {
                        response = WriteMultipleRegisters(
                                            (WriteMultipleRegistersRequest)request,
                                            DataStore,
                                            DataStore.HoldingRegisters);
                        WriteComplete?.Invoke(this, eventArgs);
                        break;
                    }

                case Modbus.ReadWriteMultipleRegisters:
                    {
                        var readWriteRequest = (ReadWriteMultipleRegistersRequest)request;
                        _ = WriteMultipleRegisters(
                            readWriteRequest.WriteRequest,
                            DataStore,
                            DataStore.HoldingRegisters);
                        WriteComplete?.Invoke(this, eventArgs);
                        response = ReadRegisters(
                            readWriteRequest.ReadRequest,
                            DataStore,
                            DataStore.HoldingRegisters);
                        break;
                    }

                default:
                    {
                        var msg = $"Unsupported function code {request.FunctionCode}.";
                        Debug.WriteLine(msg);
                        throw new InvalidModbusRequestException(Modbus.IllegalFunction);
                    }
            }
        }
        catch (InvalidModbusRequestException ex)
        {
            // Catches the exception for an illegal function or a custom exception from the ModbusSlaveRequestReceived event.
            response = new SlaveExceptionResponse(
                request.SlaveAddress,
                (byte)(Modbus.ExceptionOffset + request.FunctionCode),
                ex.ExceptionCode);
        }

        return response;
    }
}
