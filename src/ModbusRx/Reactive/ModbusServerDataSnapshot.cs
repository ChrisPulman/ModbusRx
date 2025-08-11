// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Reactive;

/// <summary>
/// Represents a snapshot of Modbus server data at a point in time.
/// </summary>
public sealed class ModbusServerDataSnapshot : IEquatable<ModbusServerDataSnapshot>
{
    /// <summary>
    /// Gets or sets the holding registers data.
    /// </summary>
    public ushort[] HoldingRegisters { get; set; } = [];

    /// <summary>
    /// Gets or sets the input registers data.
    /// </summary>
    public ushort[] InputRegisters { get; set; } = [];

    /// <summary>
    /// Gets or sets the coils data.
    /// </summary>
    public bool[] Coils { get; set; } = [];

    /// <summary>
    /// Gets or sets the inputs data.
    /// </summary>
    public bool[] Inputs { get; set; } = [];

    /// <summary>
    /// Gets or sets the timestamp of this snapshot.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets a value indicating whether this snapshot is empty.
    /// </summary>
    public bool IsEmpty => HoldingRegisters.Length == 0 && InputRegisters.Length == 0 &&
                           Coils.Length == 0 && Inputs.Length == 0;

    /// <summary>
    /// Determines whether two snapshots are equal.
    /// </summary>
    /// <param name="left">The first snapshot to compare.</param>
    /// <param name="right">The second snapshot to compare.</param>
    /// <returns>True if the snapshots are equal; otherwise, false.</returns>
    public static bool operator ==(ModbusServerDataSnapshot? left, ModbusServerDataSnapshot? right) =>
        Equals(left, right);

    /// <summary>
    /// Determines whether two snapshots are not equal.
    /// </summary>
    /// <param name="left">The first snapshot to compare.</param>
    /// <param name="right">The second snapshot to compare.</param>
    /// <returns>True if the snapshots are not equal; otherwise, false.</returns>
    public static bool operator !=(ModbusServerDataSnapshot? left, ModbusServerDataSnapshot? right) =>
        !Equals(left, right);

    /// <summary>
    /// Determines whether the specified snapshot is equal to the current snapshot.
    /// </summary>
    /// <param name="other">The snapshot to compare with the current snapshot.</param>
    /// <returns>True if the specified snapshot is equal to the current snapshot; otherwise, false.</returns>
    public bool Equals(ModbusServerDataSnapshot? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ArraysEqual(HoldingRegisters, other.HoldingRegisters) &&
               ArraysEqual(InputRegisters, other.InputRegisters) &&
               ArraysEqual(Coils, other.Coils) &&
               ArraysEqual(Inputs, other.Inputs);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current snapshot.
    /// </summary>
    /// <param name="obj">The object to compare with the current snapshot.</param>
    /// <returns>True if the specified object is equal to the current snapshot; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as ModbusServerDataSnapshot);

    /// <summary>
    /// Returns the hash code for this snapshot.
    /// </summary>
    /// <returns>A hash code for this snapshot.</returns>
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            var hash = 17;
            hash = (hash * 23) + HoldingRegisters.Length;
            hash = (hash * 23) + InputRegisters.Length;
            hash = (hash * 23) + Coils.Length;
            hash = (hash * 23) + Inputs.Length;
            return hash;
        }
#else
        var hash = new HashCode();
        hash.Add(HoldingRegisters.Length);
        hash.Add(InputRegisters.Length);
        hash.Add(Coils.Length);
        hash.Add(Inputs.Length);
        return hash.ToHashCode();
#endif
    }

    private static bool ArraysEqual<T>(T[] array1, T[] array2)
        where T : IEquatable<T>
    {
        if (array1.Length != array2.Length)
        {
            return false;
        }

        for (var i = 0; i < array1.Length; i++)
        {
            if (!array1[i].Equals(array2[i]))
            {
                return false;
            }
        }

        return true;
    }
}
