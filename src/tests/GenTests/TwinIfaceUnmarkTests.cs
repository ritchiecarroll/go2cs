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

    private static (string twins, Diagnostic[] errors) Run(string? unmark, string shape = "default")
    {
        CSharpParseOptions options = new(LanguageVersion.CSharp13);
        CSharpCompilation compilation = CSharpCompilation.Create("twiniface",
            [CSharpSyntaxTree.ParseText(Source, options, path: @"C:\twiniface\main.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new()
        {
            ["build_property.GoTwins"] = "true",
            ["build_property.GoTwinsIface"] = shape,
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

    // M:N row 3 (PromotedMethodFrames): a twin calls an interface member through the hidden interface's static helper,
    // `I.Mꞏcall(recv, …)`, which runs the plain member when the receiver is no twin implementer, and an implementer's
    // unforwarded slot runs its default body, which calls the plain member too. Both are real frames between the Go caller
    // and the method, so each carries go2cs-gen's [GeneratedCode], which the Go traceback skips (runtime managed_impl.cs,
    // classifyGoSourceFrame); without it `runtime.Caller(1)` inside the method named `main.Ptrꞏcall`, not the caller.
    [TestMethod]
    [DataRow("default", 2, DisplayName = "default shape: the two slot bodies")]
    [DataRow("hidden", 4, DisplayName = "hidden shape: the two helpers and the two slot bodies")]
    public void TheInterfaceCallHelperAndSlotBodyAreNoGoFrames(string shape, int expected)
    {
        (string twins, Diagnostic[] errors) = Run(null, shape);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));

        string[] lines = twins.Replace("\r\n", "\n").Split('\n');
        string[] declarations = ["Stringꞏcall(", "Closeꞏcall(", "Stringꞏtwin(", "Closeꞏtwin("];
        List<string> unstamped = [];
        int seen = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].TrimStart();

            // a member DECLARATION of the hidden interface (the helper is static; a slot body returns the twin result)
            if (!declarations.Any(declaration => line.Contains(declaration, StringComparison.Ordinal)) || line.StartsWith("return", StringComparison.Ordinal) ||
                !(line.StartsWith("static ", StringComparison.Ordinal) || line.StartsWith("global::", StringComparison.Ordinal)) || line.Contains("await ", StringComparison.Ordinal))
                continue;

            seen++;

            if (!lines[i - 1].Contains("GeneratedCode(\"go2cs-gen\"", StringComparison.Ordinal))
                unstamped.Add(line);
        }

        Assert.AreEqual(expected, seen, $"{shape}: the row read {seen} helper/slot declarations:\n{twins}");
        Assert.AreEqual(0, unstamped.Count, "a hidden interface's helper or slot body is a Go frame (no [GeneratedCode(\"go2cs-gen\")]):\n" + string.Join("\n", unstamped));
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
