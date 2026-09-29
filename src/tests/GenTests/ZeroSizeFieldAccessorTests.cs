// ZeroSizeFieldAccessorTests.cs - Gbtc
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
/// A17 (COORD ruling 2026-09-28): the converter now admits NAMED Go zero-size fields to the zero-size-field
/// layout arc, emitted readonly at Go's offset under <c>[StructLayout(LayoutKind.Explicit)]</c>. A writable
/// ref to a readonly field is CS8160, so the TypeGenerator's <c>Ꮡ&lt;field&gt;</c> for such a member answers
/// golib's shared per-type slot (<c>GoZeroSizeSlot&lt;T&gt;.Ref</c>); every other member keeps
/// <c>ref instance.&lt;field&gt;</c>, and a struct without the explicit layout is untouched.
/// </summary>
[TestClass]
public class ZeroSizeFieldAccessorTests
{
    private const string Source =
        """
        using System.Runtime.InteropServices;

        namespace go
        {
            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            partial class demo_package
            {
                [GoType] partial struct noCopy {
                }

                [GoType] [StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Carrier {
                    [FieldOffset(0)] internal readonly noCopy marker;
                    [FieldOffset(0)] internal ulong value;
                }

                [GoType] partial struct Plain {
                    internal noCopy marker;
                    internal ulong value;
                }
            }
        }
        """;

    private static Dictionary<string, string> RunTypeGenerator()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("zero-size-accessor-test",
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

    private static string GeneratedFor(Dictionary<string, string> sources, string type)
    {
        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains($".{type}."));
        Assert.IsNotNull(key, $"the TypeGenerator must generate {type}; got: {string.Join(", ", sources.Keys)}");
        return sources[key!];
    }

    [TestMethod]
    public void AReadOnlyZeroSizeFieldUnderExplicitLayoutAnswersTheSharedSlot()
    {
        string carrier = GeneratedFor(RunTypeGenerator(), "Carrier");

        StringAssert.Contains(carrier, "Ꮡmarker(ref Carrier instance) => ref global::go.GoZeroSizeSlot<global::go.demo_package.noCopy>.Ref;");
        StringAssert.Contains(carrier, "Ꮡvalue(ref Carrier instance) => ref instance.value;");
        Assert.IsFalse(carrier.Contains("ref instance.marker"), "a writable ref to the readonly field would be CS8160");
    }

    [TestMethod]
    public void AStructWithoutTheExplicitLayoutKeepsItsFieldRefs()
    {
        string plain = GeneratedFor(RunTypeGenerator(), "Plain");

        StringAssert.Contains(plain, "Ꮡmarker(ref Plain instance) => ref instance.marker;");
        Assert.IsFalse(plain.Contains("GoZeroSizeSlot"), "only the arc's readonly zero-size fields take the slot");
    }
}
