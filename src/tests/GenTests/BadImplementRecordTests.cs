// BadImplementRecordTests.cs - Gbtc
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
/// One malformed <c>[assembly: GoImplement&lt;…&gt;]</c> record must cost exactly its own adapter. The
/// ImplementGenerator THREW on a record whose second type argument is not an interface, and a
/// throwing generator contributes no output at all, so every adapter in the compilation went with it
/// (go-cmp's cmpopts: a record for a NAMED EMPTY interface, `GoImplement&lt;TestPanic_xᴛ3,
/// EmptyInterface&gt;(Promoted = true)` where EmptyInterface is <c>object</c>, CS8785, and an
/// unrelated CS0426 for an adapter that was never generated).
/// </summary>
[TestClass]
public class BadImplementRecordTests
{
    // One compilation, two records: a malformed one (the second argument is `object`, as a named
    // empty interface's alias makes it) and a good one beside it.
    private const string Source =
        """
        using go;

        [assembly: GoImplement<go.main_package.holder, object>(Promoted = true)]
        [assembly: GoImplement<go.main_package.name, go.main_package.Stringer>]

        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplementAttribute<TStruct, TInterface> : System.Attribute
            {
                public bool Promoted { get; set; }
                public bool Pointer { get; set; }
            }

            public static partial class main_package
            {
                public partial struct holder { public object EmptyInterface; }

                public partial struct name { public string s; }

                public static string String(this name n) => n.s;

                public partial interface Stringer
                {
                    string String();
                }
            }
        }
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    [TestMethod]
    public void AMalformedRecordCostsOnlyItsOwnAdapter()
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "bad-record",
            [CSharpSyntaxTree.ParseText(Source, new CSharpParseOptions(LanguageVersion.Latest))],
            CoreReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new ImplementGenerator()).RunGenerators(compilation).GetRunResult();
        GeneratorRunResult run = result.Results.Single();

        Assert.IsNull(run.Exception, $"one malformed record must not make the generator throw: {run.Exception}");

        List<Diagnostic> reported = [.. result.Diagnostics.Where(diagnostic => diagnostic.Id == "GO2CS0002")];
        Assert.AreEqual(1, reported.Count, $"the malformed record is reported once: {string.Join("; ", result.Diagnostics)}");
        StringAssert.Contains(reported[0].GetMessage(), "object");

        Assert.IsTrue(run.GeneratedSources.Any(source => source.HintName.Contains("name") && source.SourceText.ToString().Contains("Stringer")),
            $"the good record beside it still generates its adapter: {string.Join(", ", run.GeneratedSources.Select(source => source.HintName))}");
        Assert.IsFalse(run.GeneratedSources.Any(source => source.HintName.Contains("holder")),
            "the malformed record generates nothing");
    }
}
