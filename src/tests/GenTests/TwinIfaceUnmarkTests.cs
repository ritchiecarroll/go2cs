// TwinIfaceUnmarkTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using go2cs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenTests;

/// <summary>
/// M:N (experimental, opt-in), stage (c)'s one switch (COORD approved the plan, 2026-11-15; record 10.3): under the all-members rule
/// (GoTwinsIfaceAll) every admitted interface member carries the park mark, and GoTwinsIfaceUnmark names members that do
/// not: no twin slot is emitted for them, so a caller in any assembly calls them plain. The census's arm C reads the park
/// set with a short list unmarked. A wrong or stale mark is a cost, never a wrong answer (10.3).
/// </summary>
[TestClass]
public class TwinIfaceUnmarkTests
{
    private const string Source = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class fmt_package
        {
            public partial interface Stringer
            {
                @string String();
            }

            public partial interface Closer
            {
                void Close();
            }
        }

        public static partial class user_package
        {
            internal static channel<nint> ch;

            public static @string name(fmt_package.Stringer s)
            {
                ᐸꟷ(ch);
                return s.String();
            }

            public static void shut(fmt_package.Closer c)
            {
                ᐸꟷ(ch);
                c.Close();
            }
        }
        """;

    private static (string twins, Diagnostic[] errors) Run(string? unmark)
    {
        CSharpParseOptions options = new(LanguageVersion.CSharp13);
        CSharpCompilation compilation = CSharpCompilation.Create("twiniface",
            [CSharpSyntaxTree.ParseText(Source, options, path: @"C:\twiniface\main.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new()
        {
            ["build_property.GoTwins"] = "true",
            ["build_property.GoTwinsIface"] = "default",
            ["build_property.GoTwinsIfaceAll"] = "true"
        };

        if (unmark is not null)
            properties["build_property.GoTwinsIfaceUnmark"] = unmark;

        CSharpGeneratorDriver.Create([new TwinGenerator()], parseOptions: options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        global::go2cs.Tests.TwinStampTests.AssertStampsHold(output); // 7d: every stamp agrees with the compiler

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));

        if (Environment.GetEnvironmentVariable("GO2CS_DUMP_TWINS") is { Length: > 0 } dump)
            File.WriteAllText(dump, twins);

        // EMIT, not GetDiagnostics: an async lowering error is reported only while the state machine is built.
        Microsoft.CodeAnalysis.Emit.EmitResult emitted = output.Emit(Stream.Null);

        return (twins, emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray());
    }

    [TestMethod]
    public void EveryAdmittedMemberIsMarkedUnderTheAllMembersRule()
    {
        (string twins, Diagnostic[] errors) = Run(null);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        StringAssert.Contains(twins, "Stringꞏtwin(", "String carries the mark");
        StringAssert.Contains(twins, "Closeꞏtwin(", "Close carries the mark");
    }

    [TestMethod]
    public void AnUnmarkedMemberGetsNoTwinSlotAndIsCalledPlain()
    {
        (string twins, Diagnostic[] errors) = Run("go.fmt_package.Stringer.String");

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        Assert.IsFalse(twins.Contains("Stringꞏtwin(", StringComparison.Ordinal), "String is unmarked: no twin slot, no awaited call");
        StringAssert.Contains(twins, "Closeꞏtwin(", "Close keeps its mark");
        StringAssert.Contains(twins, "return s.String();", "name's twin calls String plain");
    }

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.IGoEquatable).Assembly.Location));
    }

    private sealed class BuildProperties(Dictionary<string, string> properties) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
    {
        private readonly Options m_global = new(properties);

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(SyntaxTree tree) => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(AdditionalText textFile) => m_global;

        private sealed class Options(Dictionary<string, string> values) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => values.TryGetValue(key, out value);
        }
    }
}
