// FieldAccessorTokenTests.cs - Gbtc
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
/// Parity seat #32 (P2, `&amp;p.f` read as `p.Ꮡf`): beside each field's <c>Ꮡf</c> accessor the TypeGenerator emits ONE
/// <c>static readonly FieldRefFunc&lt;T, F&gt; Ꮡf‿</c> on the struct itself, in the struct's own assembly. Every
/// `&amp;p.f` site in every assembly hands golib's view cache that same delegate object, so the cache compares by
/// reference on both runtimes (the design record's "#32 carry-over"; R's FieldView root cause, 2026-10-27).
/// </summary>
/// <remarks>
/// The token is built from the accessor's METHOD GROUP, never a lambda: golib reads the field's name off the
/// delegate's method (<c>FieldRefBox.FieldNameOf</c>, `Ꮡ` + the Go name). Its name ends in U+203F, a connector
/// punctuation character C# accepts and Go rejects, so no Go field's accessor can spell it (MapWrapperSet's door).
/// </remarks>
[TestClass]
public class FieldAccessorTokenTests
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
                    public ulong Count;
                    internal ushort @base;
                    internal ushort _;
                }

                [GoType] partial struct pair<T> {
                    public T First;
                }
            }
        }
        """;

    private static Dictionary<string, string> RunTypeGenerator()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("field-accessor-token-test",
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
    public void EachFieldGetsOneTokenBesideItsAccessorAtTheFieldsScope()
    {
        string plain = GeneratedFor(RunTypeGenerator(), "Plain");

        // The control: the accessor the token is built from.
        StringAssert.Contains(plain, "public static ref ulong ᏑCount(ref Plain instance) => ref instance.Count;");
        StringAssert.Contains(plain, "public static readonly global::go.FieldRefFunc<Plain, ulong> ᏑCount‿ = ᏑCount;");
        StringAssert.Contains(plain, "internal static readonly global::go.FieldRefFunc<Plain, ushort> Ꮡbase‿ = Ꮡbase;");
        Assert.IsFalse(plain.Contains("Ꮡ_‿"), "a blank field has no accessor and no token");
    }

    [TestMethod]
    public void AReadOnlyZeroSizeFieldsTokenWrapsItsSlotAccessor()
    {
        string carrier = GeneratedFor(RunTypeGenerator(), "Carrier");

        StringAssert.Contains(carrier, "internal static readonly global::go.FieldRefFunc<Carrier, global::go.demo_package.noCopy> Ꮡmarker‿ = Ꮡmarker;");
        StringAssert.Contains(carrier, "internal static readonly global::go.FieldRefFunc<Carrier, ulong> Ꮡvalue‿ = Ꮡvalue;");
    }

    [TestMethod]
    public void AGenericStructsTokenIsPerInstantiation()
    {
        string pair = GeneratedFor(RunTypeGenerator(), "pair_T_");

        StringAssert.Contains(pair, "public static readonly global::go.FieldRefFunc<pair<T>, T> ᏑFirst‿ = ᏑFirst;");
    }
}
