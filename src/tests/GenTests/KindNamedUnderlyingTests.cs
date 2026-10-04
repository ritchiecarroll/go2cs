// KindNamedUnderlyingTests.cs - Gbtc
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
/// A defined type over a NAMED type passes the underlying's name to the InheritedTypeTemplate as its TypeClass,
/// and the template's kind arms key on the six names "Array", "Slice", "Map", "Channel", "Pointer" and "Numeric".
/// A user type spelled exactly one of them (mapstructure's tests: `type MapCopy Map`, Map a struct) took that
/// kind's template, IDictionary and all (R's mapstructure reading, B3). These tests pin that such a wrapper
/// generates exactly what a wrapper over any other struct name generates, and that the underlying's name still
/// reaches the template where it is meant to (`type MyBool bool` prints lowercase).
/// </summary>
[TestClass]
public class KindNamedUnderlyingTests
{
    private static readonly string[] KindNames = ["Array", "Slice", "Map", "Channel", "Pointer", "Numeric"];

    private static string Source(string underlying) =>
        $$"""
        namespace go
        {
            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            partial class demo_package
            {
                [GoType] partial struct {{underlying}}
                {
                    public long X;
                }

                [GoType("{{underlying}}")] partial struct {{underlying}}Copy;

                [GoType("bool")] partial struct MyBool;
            }
        }
        """;

    private static Dictionary<string, string> RunTypeGenerator(string underlying)
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("kind-name-test",
            [CSharpSyntaxTree.ParseText(Source(underlying), new CSharpParseOptions(LanguageVersion.Latest))],
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

    private static string GeneratedFor(Dictionary<string, string> sources, string type)
    {
        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains($".{type}."));
        Assert.IsNotNull(key, $"the TypeGenerator must generate {type}; got: {string.Join(", ", sources.Keys)}");

        return sources[key!];
    }

    [TestMethod]
    public void AWrapperOverAKindNamedTypeGeneratesAsOverAnyOtherName()
    {
        string plain = GeneratedFor(RunTypeGenerator("Plain"), "PlainCopy");

        foreach (string kind in KindNames)
        {
            string generated = GeneratedFor(RunTypeGenerator(kind), $"{kind}Copy").Replace(kind, "Plain");
            Assert.AreEqual(plain, generated, $"a wrapper over a struct named {kind} must not take the {kind} kind's template");
        }
    }

    [TestMethod]
    public void TheUnderlyingNameStillReachesTheTemplate()
    {
        // CONTROL: the bool wrapper's TypeClass is its underlying's name, which prints Go's lowercase form.
        StringAssert.Contains(GeneratedFor(RunTypeGenerator("Plain"), "MyBool"), ".ToString().ToLowerInvariant()");
    }
}
