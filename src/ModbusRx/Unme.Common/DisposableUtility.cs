// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Unme.Common;
#else
namespace ModbusRx.Unme.Common;
#endif

/// <summary>Provides Disposable Utility functionality.</summary>
internal static class DisposableUtility
{
    /// <summary>Executes the Dispose operation.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="item">The item value.</param>
    public static void Dispose<T>(ref T? item)
        where T : class, IDisposable
    {
        if (item is null)
        {
            return;
        }

        item.Dispose();
        item = default;
    }
}
