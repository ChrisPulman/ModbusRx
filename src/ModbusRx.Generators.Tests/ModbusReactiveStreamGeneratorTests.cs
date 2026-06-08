// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ModbusRx.Generators;
using Xunit;

namespace ModbusRx.Generators.Tests;

/// <summary>
/// Tests for the reactive stream source generator.
/// </summary>
public class ModbusReactiveStreamGeneratorTests
{
    /// <summary>
    /// Verifies generated properties expose a matching observable and binding method.
    /// </summary>
    [TUnit.Core.Test]
    public void GeneratesPropertyObservableAndBinding()
    {
        const string source = """
using System;
using ModbusRx.Device;
using ModbusRx.Generators;

[ModbusReactiveDevice(ConnectionMember = "MasterStream")]
public partial class BoilerMap
{
    public IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> MasterStream { get; set; } = default!;

    [HoldingRegister(0)]
    public partial ushort? Temperature { get; private set; }
}
""";

        var result = RunGenerator(source);
        var generatedSource = string.Join(Environment.NewLine, result.GeneratedTrees.Select(tree => tree.GetText().ToString()));

        Assert.Contains("TemperatureObservable", generatedSource);
        Assert.Contains("BindGeneratedModbusStreams", generatedSource);
        Assert.Contains("ReadHoldingRegisters(this.MasterStream, 0, 1, 1000", generatedSource);
    }

    /// <summary>
    /// Verifies generated code compiles against ModbusRx and System.Reactive.
    /// </summary>
    [TUnit.Core.Test]
    public void GeneratedCodeCompiles()
    {
        const string source = """
using System;
using ModbusRx.Device;
using ModbusRx.Generators;

namespace Maps;

[ModbusReactiveDevice(ConnectionMember = "MasterStream")]
public partial class BoilerMap
{
    public IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> MasterStream { get; set; } = default!;

    [HoldingRegister(0)]
    public partial ushort? Temperature { get; private set; }

    [Coil(3)]
    public partial bool? Enabled { get; private set; }
}
""";

        var result = RunGenerator(source);
        var diagnostics = result.Compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.Empty(diagnostics);
    }

    private static GeneratorRunResult RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            GetReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var generator = new ModbusReactiveStreamGenerator();
        var driver = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], parseOptions: new CSharpParseOptions(LanguageVersion.Preview));
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.Empty(diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        return new GeneratorRunResult(driver.GetRunResult().GeneratedTrees, outputCompilation);
    }

    private static IEnumerable<MetadataReference> GetReferences()
    {
        var trustedPlatformAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?
            .Split(Path.PathSeparator)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => MetadataReference.CreateFromFile(path)) ?? [];

        foreach (var reference in trustedPlatformAssemblies)
        {
            yield return reference;
        }

        yield return MetadataReference.CreateFromFile(typeof(ModbusRx.Device.ModbusIpMaster).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(System.Reactive.Linq.Observable).Assembly.Location);
    }

    private sealed class GeneratorRunResult(IReadOnlyList<SyntaxTree> generatedTrees, Compilation compilation)
    {
        public IReadOnlyList<SyntaxTree> GeneratedTrees { get; } = generatedTrees;

        public Compilation Compilation { get; } = compilation;
    }
}
