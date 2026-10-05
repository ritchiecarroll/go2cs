// SelfContainingMapHolderTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A named map whose VALUE contains the wrapper itself by value cannot keep its golib <c>map&lt;K,V&gt;</c>
/// inline: .NET fails to load the wrapper type (a SIGSEGV in the type loader, or a TypeLoadException for
/// the struct shapes), measured on .NET 10.0.12 with go-cmp's <c>type M map[int]M</c>. The TypeGenerator
/// holds such a map in a StrongBox. One arm per containment branch (direct, through a struct field, two
/// struct levels, a fixed-size array, a slice, a map, a named slice wrapper, a generic wrapper), and one
/// control per reference kind the walk must stop at (pointer, channel, func, interface), whose wrappers
/// keep the inline field. The stub golib types below mirror golib's kinds: array, slice, map and channel
/// are structs, <c>ж&lt;T&gt;</c> is a class.
/// </summary>
[TestClass]
public class SelfContainingMapHolderTests
{
    private const string Source =
        """
        namespace go
        {
            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            public readonly struct array<T> { private readonly T[] items; }
            public readonly struct slice<T> { private readonly T[] items; }
            public readonly struct map<K, V> { private readonly object store; }
            public struct channel<T> { private object queue; }
            public abstract class ж<T> { }

            partial class demo_package
            {
                [GoType("map[nint, Direct]")] partial struct Direct;

                [GoType("map[nint, viaStructV]")] partial struct ViaStruct;

                [GoType] partial struct viaStructV {
                    internal nint n;
                    internal ViaStruct m;
                }

                [GoType("map[nint, viaNestedA]")] partial struct ViaNested;

                [GoType] partial struct viaNestedA {
                    internal viaNestedB b;
                }

                [GoType] partial struct viaNestedB {
                    internal ViaNested m;
                }

                [GoType("map[nint, array<ViaArray>]")] partial struct ViaArray;

                [GoType("map[nint, slice<ViaSlice>]")] partial struct ViaSlice;

                [GoType("map[nint, map<nint, ViaMap>]")] partial struct ViaMap;

                [GoType("map[nint, namedSlice]")] partial struct ViaNamed;

                [GoType("[]ViaNamed")] partial struct namedSlice;

                [GoType("map[nint, Generic<T>]")] partial struct Generic<T>;

                [GoType("map[nint, ж<ViaPtr>]")] partial struct ViaPtr;

                [GoType("map[nint, channel<ViaChan>]")] partial struct ViaChan;

                [GoType("map[nint, System.Func<ViaFunc>]")] partial struct ViaFunc;

                [GoType("map[nint, viaIfaceI]")] partial struct ViaIface;

                interface viaIfaceI {
                    ViaIface M();
                }

                [GoType("map[nint, viaPtrStruct]")] partial struct ViaPtrStruct;

                [GoType] partial struct viaPtrStruct {
                    internal ж<ViaPtrStruct> p;
                }

                [GoType("map[nint, nint]")] partial struct PlainMap;
            }
        }
        """;

    private const string Holder = "private readonly global::System.Runtime.CompilerServices.StrongBox<map<";

    private static Dictionary<string, string> RunTypeGenerator()
    {
        string coreDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("self-containing-map-holder-test",
            [CSharpSyntaxTree.ParseText(Source, new CSharpParseOptions(LanguageVersion.Latest))],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Runtime.dll"))
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator());
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());
    }

    // The hint name of a generic type folds its parameter list into the name (Generic_T_), so a type is
    // found by its name followed by either separator.
    private static string GeneratedFor(Dictionary<string, string> sources, string type)
    {
        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains($".{type}.") || hint.Contains($".{type}_"));
        Assert.IsNotNull(key, $"the TypeGenerator must generate {type}; got: {string.Join(", ", sources.Keys)}");
        return sources[key!];
    }

    private static void AssertParses(string type, string generated)
    {
        string[] errors = CSharpSyntaxTree.ParseText(generated, new CSharpParseOptions(LanguageVersion.Latest))
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(diagnostic => diagnostic.ToString())
            .ToArray();

        Assert.AreEqual(0, errors.Length, $"the generated {type} must parse:\n{string.Join("\n", errors)}");
    }

    private static void AssertHolder(Dictionary<string, string> sources, string type, string mapType)
    {
        string generated = GeneratedFor(sources, type);

        StringAssert.Contains(generated, $"{Holder}{mapType[4..]}>? m_value;", $"{type} must hold its map in a StrongBox");
        StringAssert.Contains(generated, "Value => m_value is { } holder ? holder.Value : default;", $"{type}: a null holder must read as the nil map");
        StringAssert.Contains(generated, $"set {{ {mapType} target = Value; target[key] = value; }}", $"{type}: the indexer must write through a local");
        StringAssert.Contains(generated, $"=> m_value = new global::System.Runtime.CompilerServices.StrongBox<{mapType}>(new {mapType}(size));", $"{type}: the capacity constructor must store a holder");
        StringAssert.Contains(generated, "public override bool Equals(object? obj)", $"{type}: equality must compare the map, not the holder");
        AssertParses(type, generated);
    }

    private static void AssertInline(Dictionary<string, string> sources, string type, string mapType)
    {
        string generated = GeneratedFor(sources, type);

        StringAssert.Contains(generated, $"private readonly {mapType} m_value;", $"{type} must keep its map inline");
        StringAssert.Contains(generated, "set => m_value[key] = value;");
        Assert.IsFalse(generated.Contains("StrongBox"), $"{type} must not take the holder");
        AssertParses(type, generated);
    }

    [TestMethod]
    public void AMapWhoseValueIsItselfTakesTheHolder() =>
        AssertHolder(RunTypeGenerator(), "Direct", "map<nint, Direct>");

    [TestMethod]
    public void AMapReachingItselfThroughAStructFieldTakesTheHolder() =>
        AssertHolder(RunTypeGenerator(), "ViaStruct", "map<nint, viaStructV>");

    [TestMethod]
    public void AMapReachingItselfThroughTwoStructLevelsTakesTheHolder() =>
        AssertHolder(RunTypeGenerator(), "ViaNested", "map<nint, viaNestedA>");

    [TestMethod]
    public void AMapReachingItselfThroughAnArrayTakesTheHolder() =>
        AssertHolder(RunTypeGenerator(), "ViaArray", "map<nint, array<ViaArray>>");

    [TestMethod]
    public void AMapReachingItselfThroughASliceTakesTheHolder() =>
        AssertHolder(RunTypeGenerator(), "ViaSlice", "map<nint, slice<ViaSlice>>");

    [TestMethod]
    public void AMapReachingItselfThroughAMapTakesTheHolder() =>
        AssertHolder(RunTypeGenerator(), "ViaMap", "map<nint, map<nint, ViaMap>>");

    [TestMethod]
    public void AMapReachingItselfThroughANamedSliceWrapperTakesTheHolder() =>
        AssertHolder(RunTypeGenerator(), "ViaNamed", "map<nint, namedSlice>");

    [TestMethod]
    public void AGenericMapReachingItselfTakesTheHolder() =>
        AssertHolder(RunTypeGenerator(), "Generic", "map<nint, Generic<T>>");

    [TestMethod]
    public void AMapReachingItselfOnlyThroughAReferenceKeepsItsMapInline()
    {
        Dictionary<string, string> sources = RunTypeGenerator();

        AssertInline(sources, "ViaPtr", "map<nint, ж<ViaPtr>>");
        AssertInline(sources, "ViaChan", "map<nint, channel<ViaChan>>");
        AssertInline(sources, "ViaFunc", "map<nint, System.Func<ViaFunc>>");
        AssertInline(sources, "ViaIface", "map<nint, viaIfaceI>");
        AssertInline(sources, "ViaPtrStruct", "map<nint, viaPtrStruct>");
    }

    [TestMethod]
    public void AnOrdinaryMapKeepsItsMapInline() =>
        AssertInline(RunTypeGenerator(), "PlainMap", "map<nint, nint>");
}
