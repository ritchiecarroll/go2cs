// StackAppendBufferAnalyzerTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// StackAppendBufferAnalyzer, GO2CS0007: a call that binds golib's stack-append overload passes the buffer as a plain
/// local of the calling method or lambda, captured by no lambda, so the used flag the overload sets through the
/// <c>in</c> reference is that frame's own.
/// </summary>
/// <remarks>
/// RED before the analyzer existed: every refused shape compiled clean. The worst case it guards is a captured
/// buffer, which a lambda would copy (a snapshot), so a second empty append in the same call would take the buffer
/// again.
/// </remarks>
[TestClass]
public class StackAppendBufferAnalyzerTests
{
    private const string Golib =
        """
        namespace go
        {
            public struct slice<T> { }

            public struct stackAppend<T>
            {
                internal bool used;
            }

            public static class builtin
            {
                public static slice<T> append<T>(in stackAppend<T> buffer, slice<T> slice, params System.ReadOnlySpan<T> elems) => slice;
            }
        }
        """;

    private static ImmutableArray<Diagnostic> Analyze(string members)
    {
        string source =
            $$"""
            using static go.builtin;

            namespace go
            {
                public static partial class main_package
                {
                    {{members}}
                }
            }
            """;

        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create(
            "stack-append-buffer-test",
            [
                CSharpSyntaxTree.ParseText(Golib, path: "golib_stand_in_impl.cs"),
                CSharpSyntaxTree.ParseText(source, path: "main.cs")
            ],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] errors = [.. compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        Assert.AreEqual(0, errors.Length, "the fixture must compile: " + string.Join("; ", errors.Select(error => error.ToString())));

        return compilation.WithAnalyzers([new StackAppendBufferAnalyzer()]).GetAnalyzerDiagnosticsAsync().Result;
    }

    [TestMethod]
    [DataRow(
        """
        public static void F()
        {
            var xᴷ = new stackAppend<long>();
            slice<long> x = default;
            for (int i = 0; i < 5; i++)
                x = append(xᴷ, x, i);
        }
        """, DisplayName = "a plain local of the method")]
    [DataRow(
        """
        public static System.Func<int> F() => () =>
        {
            var wᴷ = new stackAppend<long>();
            slice<long> w = default;
            w = append(wᴷ, w, 1L);
            return 0;
        };
        """, DisplayName = "a plain local of the lambda that appends")]
    public void ThePlainLocalOfTheAppendingFrameIsAccepted(string members)
    {
        ImmutableArray<Diagnostic> diagnostics = Analyze(members);

        Assert.AreEqual(0, diagnostics.Length, string.Join("; ", diagnostics));
    }

    [TestMethod]
    [DataRow(
        """
        private static readonly stackAppend<long> xᴷ = new();
        public static void F() { slice<long> x = default; x = append(xᴷ, x, 1L); }
        """, "a readonly field", DisplayName = "a readonly field")]
    [DataRow(
        """
        public static void F() { slice<long> x = default; x = append(new stackAppend<long>(), x, 1L); }
        """, "a new value, not a local", DisplayName = "an inline new stackAppend<T>()")]
    [DataRow(
        """
        public static void F()
        {
            var xᴷ = new stackAppend<long>();
            slice<long> x = default;
            System.Action snapshot = () => { var copy = xᴷ; };
            x = append(xᴷ, x, 1L);
            snapshot();
        }
        """, "a local a lambda captures", DisplayName = "a local a lambda captures")]
    [DataRow(
        """
        public static void F(stackAppend<long>[] buffers)
        {
            slice<long> x = default;
            foreach (var xᴷ in buffers)
                x = append(xᴷ, x, 1L);
        }
        """, "a foreach variable", DisplayName = "a foreach variable")]
    [DataRow(
        """
        public static void F()
        {
            var xᴷ = new stackAppend<long>();
            slice<long> x = default;
            System.Action grow = () => { x = append(xᴷ, x, 1L); };
            grow();
        }
        """, "a local of another method or lambda", DisplayName = "the outer frame's local, appended inside a lambda")]
    [DataRow(
        """
        public static void F(stackAppend<long> xᴷ) { slice<long> x = default; x = append(xᴷ, x, 1L); }
        """, "a parameter", DisplayName = "a parameter")]
    public void AnyOtherArgumentIsGO2CS0007(string members, string reason)
    {
        Diagnostic[] refused = [.. Analyze(members).Where(diagnostic => diagnostic.Id == "GO2CS0007")];

        Assert.AreEqual(1, refused.Length, string.Join("; ", refused.Select(diagnostic => diagnostic.ToString())));
        StringAssert.Contains(refused[0].GetMessage(), reason);
        Assert.AreEqual(DiagnosticSeverity.Error, refused[0].Severity);
    }
}
