// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.DriverTest;

/// <summary>
/// DummyTemperatureController.
/// </summary>
public class DummyTemperatureController
{
    private readonly Random _rand = new();

    /// <summary>
    /// Gets the current temperature.
    /// </summary>
    /// <value>
    /// The current temperature.
    /// </value>
    public double CurrentTemperature { get; private set; } = 20.0;

    /// <summary>
    /// Gets or sets the setpoint.
    /// </summary>
    /// <value>
    /// The setpoint.
    /// </value>
    public double Setpoint { get; set; } = 50.0;

    /// <summary>
    /// Gets or sets the k.
    /// </summary>
    /// <value>
    /// The k.
    /// </value>
    public double K { get; set; } = 0.1; // Rate of change

    /// <summary>
    /// Updates this instance.
    /// </summary>
    public void Update()
    {
        // Add a little noise
        var noise = (_rand.NextDouble() - 0.5) * 0.2;

        // Move toward setpoint
        var delta = (Setpoint - CurrentTemperature) * K;

        CurrentTemperature += delta + noise;
    }

    /// <summary>
    /// Reads the register.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>A ushort.</returns>
    public ushort ReadRegister(ushort address) => address switch
    {
        0 => (ushort)(CurrentTemperature * 10), // e.g. 25.3°C → 253
        1 => (ushort)(Setpoint * 10),
        2 => (ushort)(K * 1000),
        _ => throw new ArgumentOutOfRangeException(nameof(address))
    };

    /// <summary>
    /// Writes the register.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="value">The value.</param>
    public void WriteRegister(ushort address, ushort value)
    {
        switch (address)
        {
            case 1:
                Setpoint = value / 10.0;
                break;
            case 2:
                K = value / 1000.0;
                break;
        }
    }
}
