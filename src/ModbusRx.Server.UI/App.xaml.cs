// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Splat;

namespace ModbusRx.Server.UI;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App() =>
        Locator.CurrentMutable.RegisterConstant<ILogger>(new DebugLogger());
}
