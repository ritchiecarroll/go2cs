// ValueEmbedStaticHopTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A struct that satisfies an interface only through a single VALUE embed of a FOREIGN struct gets
/// a value implementation that calls the embed's package-class static directly
/// (<c>global::go.net.netip_package.String(this.AddrPort)</c>), because the file does not import
/// that class's extensions. The class was spelled by prefixing <c>global::go.</c> to the embed's
/// written type name, which is right only when the name is written relative to <c>go</c>. A
/// converted TEST file writes it otherwise, and golang-jwt's root test host did not build:
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>internal test: <c>global::go.example.com.lib_package.Base</c>, so the call was spelled
///   <c>global::go.global::go.…</c> (CS7000, CS0234);</item>
///   <item>external test of a module whose path is not under <c>go</c>:
///   <c>go.example.com.lib_package.Base</c>, so the call was <c>global::go.go.…</c> (CS0234).</item>
/// </list>
/// The class is now read off the embed's symbol. The third arm writes the name relative to
/// <c>go</c>, the form the old spelling served; it reads the same before and after.
/// </remarks>
[TestClass]
public class ValueEmbedStaticHopTests
{
    private const string ProductionSource =
        """
        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplementAttribute<TStruct, TInterface> : System.Attribute
            {
                public bool Promoted { get; set; }
                public bool Pointer { get; set; }
            }
        }

        namespace go.example.com
        {
            public static partial class lib_package
            {
                public partial struct Base { public int N; }

                public static string Name(this Base b) => "base";

                public interface Namer
                {
                    string Name();
                }
            }
        }
        """;

    // The three spellings of one embed, one struct each, all in the consuming assembly.
    private const string ConsumingSource =
        """
        using go;

        [assembly: GoImplement<global::go.example.com.lib_internal_test_package.inner, global::go.example.com.lib_package.Namer>]
        [assembly: GoImplement<global::go.example.com.lib_test_package.outer, global::go.example.com.lib_package.Namer>]
        [assembly: GoImplement<global::go.example.com.lib_test_package.relative, global::go.example.com.lib_package.Namer>]

        namespace go.example.com
        {
            public static partial class lib_internal_test_package
            {
                public partial struct inner
                {
                    public partial ref global::go.example.com.lib_package.Base Base { get; }
                }
            }

            public static partial class lib_test_package
            {
                public partial struct outer
                {
                    public partial ref go.example.com.lib_package.Base Base { get; }
                }

                public partial struct relative
                {
                    public partial ref example.com.lib_package.Base Base { get; }
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

    private static Dictionary<string, string> RunImplementGenerator()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation production = CSharpCompilation.Create(
            "value-hop-production",
            [CSharpSyntaxTree.ParseText(ProductionSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = production.Emit(image);
        Assert.IsTrue(emitted.Success, $"production compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        CSharpCompilation consuming = CSharpCompilation.Create(
            "value-hop-test",
            [CSharpSyntaxTree.ParseText(ConsumingSource, parseOptions)],
            CoreReferences().Append(MetadataReference.CreateFromImage(image.ToArray())),
            libraryOptions);

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new ImplementGenerator()).RunGenerators(consuming).GetRunResult();

        foreach (GeneratorRunResult each in result.Results)
            Assert.IsNull(each.Exception, $"the generator must not throw: {each.Exception}");

        return result.Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());
    }

    private static string ValueImplementation(Dictionary<string, string> sources, string structName)
    {
        List<string> matches = sources
            .Where(entry => entry.Key.Contains($"_package.{structName}-") && !entry.Key.EndsWith("-ptr.g.cs"))
            .Select(entry => entry.Value)
            .ToList();

        Assert.AreEqual(1, matches.Count, $"one value implementation for {structName} — saw {matches.Count} of: {string.Join(" | ", sources.Keys)}");
        return matches[0];
    }

    [TestMethod]
    public void TheEmbedsPackageClassIsQualifiedOnceWhateverTheEmbedsSpelling()
    {
        Dictionary<string, string> sources = RunImplementGenerator();

        foreach (string structName in new[] { "inner", "outer", "relative" })
        {
            string implementation = ValueImplementation(sources, structName);

            StringAssert.Contains(implementation, "=> global::go.example.com.lib_package.Name(this.Base);",
                $"{structName}: the value implementation calls the embed's package-class static, qualified once");
            Assert.IsFalse(implementation.Contains("global::go.global::"), $"{structName}: a doubled global:: alias (CS7000)");
            Assert.IsFalse(implementation.Contains("global::go.go."), $"{structName}: a doubled go. segment (CS0234)");
        }
    }
}
