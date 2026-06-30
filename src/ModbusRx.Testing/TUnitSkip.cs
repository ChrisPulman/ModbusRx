// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ModbusRx.Testing;

/// <summary>Provides xUnit-style skip helpers backed by TUnit.</summary>
internal static class TUnitSkip
{
    /// <summary>Skips the current test when the condition is true.</summary>
    /// <param name="condition">The condition that controls the skip.</param>
    /// <param name="reason">The reason reported by TUnit.</param>
    public static void If(bool condition, string reason)
    {
        if (!condition)
        {
            return;
        }

        TUnit.Core.Skip.Test(reason ?? throw new ArgumentNullException(nameof(reason)));
    }

    /// <summary>Skips the current test when the condition is false.</summary>
    /// <param name="condition">The condition that controls the skip.</param>
    /// <param name="reason">The reason reported by TUnit.</param>
    public static void IfNot(bool condition, string reason)
    {
        if (condition)
        {
            return;
        }

        TUnit.Core.Skip.Test(reason ?? throw new ArgumentNullException(nameof(reason)));
    }
}
