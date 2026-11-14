// EqualityWithoutBoxingTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// The generated equality members of a struct, of a defined type over a struct, and of a named map or
/// channel must answer without boxing. Before, a named map's or channel's <c>m == nil</c> emitted
/// <c>value.Equals(default(M))</c>, which the wrapper did not declare, so both values were boxed into
/// ValueType.Equals and its fields read by reflection; a struct declared <c>Equals(T)</c> without
/// <c>IEquatable&lt;T&gt;</c>, so a Dictionary keyed by it boxed every probe; and a defined type over a struct
/// declared no equality at all and fell back to ValueType.Equals and ValueType.GetHashCode (measured on
/// the converted TypeScript type checker, docs/phase4/SCOUT-ts-perf.md items S3 and S4).
/// </summary>
[TestClass]
public class EqualityWithoutBoxingTests
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
                [GoType] partial struct Point
                {
                    public long X;
                    public long Y;
                }

                [GoType("Point")] partial struct Key;

                [GoType("map[nint, nint]")] partial struct Table;

                [GoType("chan nint")] partial struct Queue;
            }
        }
        """;

    private static Dictionary<string, string> RunTypeGenerator()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("equality-without-boxing-test",
            [CSharpSyntaxTree.ParseText(Source, new CSharpParseOptions(LanguageVersion.Latest))],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator());
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());
    }

    private static string GeneratedFor(string type)
    {
        Dictionary<string, string> sources = RunTypeGenerator();
        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains($".{type}."));
        Assert.IsNotNull(key, $"the TypeGenerator must generate {type}; got: {string.Join(", ", sources.Keys)}");

        return sources[key!];
    }

    [TestMethod]
    public void ANamedMapComparesWithNilThroughTheMapItself()
    {
        string generated = GeneratedFor("Table");

        StringAssert.Contains(generated, "operator ==(Table value, NilType nil) => value.m_value == nil;");
        Assert.IsFalse(generated.Contains("value.Equals(default(Table))"), "a named map's == nil must not box into ValueType.Equals");
    }

    [TestMethod]
    public void ANamedChannelComparesWithNilThroughTheChannelItself()
    {
        string generated = GeneratedFor("Queue");

        StringAssert.Contains(generated, "operator ==(Queue value, NilType nil) => value.m_value == nil;");
        Assert.IsFalse(generated.Contains("value.Equals(default(Queue))"), "a named channel's == nil must not box into ValueType.Equals");
    }

    [TestMethod]
    public void AStructIsEquatableToItself()
    {
        string generated = GeneratedFor("Point");

        StringAssert.Contains(generated, "public bool Equals(Point other) =>");
        StringAssert.Contains(generated, "partial struct Point : global::go.IGoEquatable, global::System.IEquatable<Point>");
    }

    [TestMethod]
    public void ADefinedTypeOverAStructDeclaresItsOwnEquality()
    {
        string generated = GeneratedFor("Key");

        StringAssert.Contains(generated, "public bool Equals(Key other) => m_value.Equals(other.m_value);");
        StringAssert.Contains(generated, "public override bool Equals(object? obj) => obj is Key other && Equals(other);");
        StringAssert.Contains(generated, "public override int GetHashCode() => m_value.GetHashCode();");
        StringAssert.Contains(generated, "partial struct Key : global::go.IGoEquatable, global::System.IEquatable<Key>");
    }
}
