// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Device;
#else
using ModbusRx.Device;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Message;
#else
using ModbusRx.Message;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive
#else
namespace ModbusRx
#endif
{
    /// <summary>Provides ModbusTcpSlaveExtensions functionality.</summary>
    public static class ModbusTcpSlaveExtensions
    {
        /// <summary>Provides write adapters for TCP slave observable streams.</summary>
        /// <param name="slave">The slave stream.</param>
        extension(IObservable<ModbusTcpSlave> slave)
        {
            /// <summary>Writes the Input Registers.</summary>
            /// <param name="startAddress">The start address.</param>
            /// <param name="valuesToWrite">The values to write.</param>
            /// <returns>Observable ModbusTcpSlave.</returns>
            public IObservable<ModbusTcpSlave> WriteInputRegisters(ushort startAddress, IObservable<ushort[]> valuesToWrite)
            {
                _ = slave.CombineLatest(
                    valuesToWrite,
                    (currentSlave, data) => (currentSlave, data)).Subscribe(source => ModbusSlave.WriteMultipleRegisters(new WriteMultipleRegistersRequest(1, startAddress, new RegisterCollection(source.data)), source.currentSlave.DataStore, source.currentSlave.DataStore.InputRegisters));
                return slave;
            }

            /// <summary>Writes the holding registers.</summary>
            /// <param name="startAddress">The start address.</param>
            /// <param name="valuesToWrite">The values to write.</param>
            /// <returns>Observable ModbusTcpSlave.</returns>
            public IObservable<ModbusTcpSlave> WriteHoldingRegisters(ushort startAddress, IObservable<ushort[]> valuesToWrite)
            {
                _ = slave.CombineLatest(
                    valuesToWrite,
                    (currentSlave, data) => (currentSlave, data)).Subscribe(source => ModbusSlave.WriteMultipleRegisters(new WriteMultipleRegistersRequest(1, startAddress, new RegisterCollection(source.data)), source.currentSlave.DataStore, source.currentSlave.DataStore.HoldingRegisters));
                return slave;
            }

            /// <summary>Writes the coil discretes.</summary>
            /// <param name="startAddress">The start address.</param>
            /// <param name="valuesToWrite">The values to write.</param>
            /// <returns>Observable ModbusTcpSlave.</returns>
            public IObservable<ModbusTcpSlave> WriteCoilDiscretes(ushort startAddress, IObservable<bool[]> valuesToWrite)
            {
                _ = slave.CombineLatest(
                    valuesToWrite,
                    (currentSlave, data) => (currentSlave, data)).Subscribe(source => ModbusSlave.WriteMultipleCoils(new WriteMultipleCoilsRequest(1, startAddress, new DiscreteCollection(source.data)), source.currentSlave.DataStore, source.currentSlave.DataStore.CoilDiscretes));
                return slave;
            }

            /// <summary>Writes the input discretes.</summary>
            /// <param name="startAddress">The start address.</param>
            /// <param name="valuesToWrite">The values to write.</param>
            /// <returns>Observable ModbusTcpSlave.</returns>
            public IObservable<ModbusTcpSlave> WriteInputDiscretes(ushort startAddress, IObservable<bool[]> valuesToWrite)
            {
                _ = slave.CombineLatest(
                    valuesToWrite,
                    (currentSlave, data) => (currentSlave, data)).Subscribe(source => ModbusSlave.WriteMultipleCoils(new WriteMultipleCoilsRequest(1, startAddress, new DiscreteCollection(source.data)), source.currentSlave.DataStore, source.currentSlave.DataStore.InputDiscretes));
                return slave;
            }
        }
    }
}
