// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using ModbusRx.Reactive;

namespace ModbusRx.DriverTest;

internal static class Program
{
    private static void Main(string[] args)
    {
        try
        {
            if (!System.IO.Ports.SerialPort.GetPortNames().Contains("COM1") || !System.IO.Ports.SerialPort.GetPortNames().Contains("COM2"))
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} COM1 or COM2 not available. Please set up virtual serial ports using com0com or similar.");
                return;
            }

            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Starting Modbus RTU Temperature Controller Emulator...");
            var emulator = new SerialDeviceEmulator("COM2", 1); // Pair with COM2 via com0com
            SpinWait.SpinUntil(() => false, 1000); // Wait for the emulator to start

            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Starting Modbus RTU Master...");
            var disposables = new CompositeDisposable();
            var dataRecived = 0;
            Create.SerialRtuMaster("COM1", 9600, 8, System.IO.Ports.Parity.None, System.IO.Ports.StopBits.One)
                .ReadHoldingRegisters(1, 0, 3)
                .Subscribe(
                    modbus =>
                {
                    for (var i = 0; i < modbus.data?.Length; i++)
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Register {i}={modbus.data[i]}");
                    }

                    dataRecived++;
                    if (dataRecived >= 5)
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Success: Received 5 data sets, exiting...");
                        disposables.Dispose();
                        emulator.Dispose();
                        Environment.Exit(0);
                    }
                },
                    ex => Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Error: {ex.Message}"))
                .DisposeWith(disposables);

            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Running : waiting for automatic exit after 5 data sets OR Timeout of 60 Seconds.");
            SpinWait.SpinUntil(() => false, 60000);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Failing: Exiting on user request.");
            disposables.Dispose();
            emulator.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {e.Message}");
        }
    }
}
