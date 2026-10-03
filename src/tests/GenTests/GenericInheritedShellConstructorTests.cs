// GenericInheritedShellConstructorTests.cs - Gbtc
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
/// A generic defined slice, map or channel type (<c>type Set[T comparable] map[T]void</c>, the hashset
/// module) reaches the TypeGenerator as <c>partial struct Set&lt;T&gt;</c>. A C# constructor is named
/// without the type's parameters, so the Slice, Map and Channel templates' capacity constructors must
/// spell the bare name, as the base template's own constructors already do; spelled <c>Set&lt;T&gt;(...)</c>
/// the generated file does not parse. A non-generic shell's emission is unchanged.
/// </summary>
[TestClass]
public class GenericInheritedShellConstructorTests
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

            partial class demo_package
            {
                [GoType] partial struct empty {
                }

                [GoType("map[K, V]")] partial struct Pair<K, V>;

                [GoType("map[T, empty]")] partial struct Set<T>;

                [GoType("chan T")] partial struct Pipe<T>;

                [GoType("[]T")] partial struct List<T>;

                [GoType("map[int, int]")] partial struct PlainMap;

                [GoType("chan int")] partial struct PlainPipe;

                [GoType("[]int")] partial struct PlainList;
            }
        }
        """;

    private static Dictionary<string, string> RunTypeGenerator()
    {
        string coreDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("generic-shell-ctor-test",
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

    // The hint name of a generic type folds its parameter list into the name (Pair_K_V_), so a type is
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

    [TestMethod]
    public void AGenericMapShellNamesItsCapacityConstructorWithoutTypeParameters()
    {
        Dictionary<string, string> sources = RunTypeGenerator();

        foreach ((string type, string arguments) in new[] { ("Pair", "<K, V>"), ("Set", "<T>") })
        {
            string generated = GeneratedFor(sources, type);

            StringAssert.Contains(generated, $"public {type}(nint size) => m_value = new map<");
            Assert.IsFalse(generated.Contains($"public {type}{arguments}("), $"a constructor named {type}{arguments} does not parse");
            AssertParses(type, generated);
        }
    }

    [TestMethod]
    public void AGenericChannelShellNamesItsCapacityConstructorWithoutTypeParameters()
    {
        string generated = GeneratedFor(RunTypeGenerator(), "Pipe");

        StringAssert.Contains(generated, "public Pipe(nint size) => m_value = new channel<T>(size);");
        Assert.IsFalse(generated.Contains("public Pipe<T>("), "a constructor named Pipe<T> does not parse");
        AssertParses("Pipe", generated);
    }

    [TestMethod]
    public void AGenericSliceShellNamesItsLengthConstructorWithoutTypeParameters()
    {
        string generated = GeneratedFor(RunTypeGenerator(), "List");

        StringAssert.Contains(generated, "public List(nint length, nint capacity = -1, nint low = 0) => m_value = new slice<T>(length, capacity, low);");
        Assert.IsFalse(generated.Contains("public List<T>("), "a constructor named List<T> does not parse");
        AssertParses("List", generated);
    }

    [TestMethod]
    public void ANonGenericShellKeepsItsConstructors()
    {
        Dictionary<string, string> sources = RunTypeGenerator();

        StringAssert.Contains(GeneratedFor(sources, "PlainMap"), "public PlainMap(nint size) => m_value = new map<int, int>(size);");
        StringAssert.Contains(GeneratedFor(sources, "PlainPipe"), "public PlainPipe(nint size) => m_value = new channel<int>(size);");
        StringAssert.Contains(GeneratedFor(sources, "PlainList"), "public PlainList(nint length, nint capacity = -1, nint low = 0) => m_value = new slice<int>(length, capacity, low);");

        foreach (string type in new[] { "PlainMap", "PlainPipe", "PlainList" })
            AssertParses(type, GeneratedFor(sources, type));
    }
}
