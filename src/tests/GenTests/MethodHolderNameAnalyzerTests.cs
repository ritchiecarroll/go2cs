// MethodHolderNameAnalyzerTests.cs - Gbtc
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
/// MethodHolderNameAnalyzer, GO2CS0013: inside a class marked <c>[GoMethodHost]</c>, a simple name that the package
/// class also declares binds the package's member, and a name written through the file's alias of the package binds
/// the alias.
/// </summary>
/// <remarks>
/// RED before the analyzer read anything: every refused shape here compiles without a diagnostic and binds a member
/// other than the package's. A Go method's body reads its package's names by their simple names. In the holder those
/// names arrive through <c>using static</c>, behind the holder's own methods, the members of <c>go.builtin</c> and
/// the types of a namespace, so a missed qualification is not always a compile error.
/// </remarks>
[TestClass]
public class MethodHolderNameAnalyzerTests
{
    private const string Golib =
        """
        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public sealed class GoMethodHostAttribute(System.Type receiver) : System.Attribute
            {
                public System.Type Receiver { get; } = receiver;
            }

            public static class builtin
            {
                public static T max<T>(T a, T b) where T : System.IComparable<T> => a.CompareTo(b) > 0 ? a : b;
                public static int cap<T>(T value) => 0;
            }

            // A type of the namespace, as golib's own public types are.
            public struct Reader { public int fromNamespace; }
        }
        """;

    private const string Package =
        """
        namespace go
        {
            public static partial class time_package
            {
                public partial struct Time { public int Limit; public int v; }
                public partial struct Reader { public int fromPackage; }

                public static int Limit = 7;
                public static int Second = 1;
                public static int norm(Time t) => 1;
                public static double max(double a, double b) => a > b ? a : b;
                public static void time() { }
            }

            [GoMethodHost(typeof(time_package.Time))]
            public static partial class timeᴛTime;
        }
        """;

    // The holder's part under test: `usings` are the file's using directives, `members` the holder's members.
    private static ImmutableArray<Diagnostic> Analyze(string usings, string members, string holder = "timeᴛTime")
    {
        string source =
            $$"""
            namespace go
            {
                using static go.time_package;
                using static go.builtin;
                {{usings}}

                public static partial class {{holder}}
                {
                    {{members}}
                }
            }
            """;

        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create(
            "method-holder-name-test",
            [
                CSharpSyntaxTree.ParseText(Golib, path: "golib_stand_in_impl.cs"),
                CSharpSyntaxTree.ParseText(Package, path: "package_info.cs"),
                CSharpSyntaxTree.ParseText(source, path: "time.cs")
            ],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] errors = [.. compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        Assert.AreEqual(0, errors.Length, "the fixture must compile: " + string.Join("; ", errors.Select(error => error.ToString())));

        return compilation.WithAnalyzers([new MethodHolderNameAnalyzer()]).GetAnalyzerDiagnosticsAsync().Result;
    }

    [TestMethod]
    [DataRow("",
        """
        public static int norm(this Time t) => time_package.norm(t) + time_package.Limit;
        """, DisplayName = "the package's members through the package class")]
    [DataRow("using time = go.time_package;",
        """
        public static int norm(this Time t) => time.norm(t) + time.Limit;
        public static double max(this Time t) => time.max(1, 2);
        """, DisplayName = "the package's members through the file's alias")]
    [DataRow("using time = go.time_package;",
        """
        public static int Limit(this Time t) => time.Limit;
        public static void M(this Time t) { time.time(); time(); }
        """, DisplayName = "the alias beside a package function of the alias's name")]
    [DataRow("",
        """
        public static int norm(this Time t) => 2;
        public static int twice(this Time t) => t.norm() + t.norm();
        """, DisplayName = "a method of the holder through its receiver")]
    [DataRow("",
        """
        public static int helper(this Time t) => 2;
        public static int twice(this Time t) => helper(t) + Second;
        """, DisplayName = "simple names the package and the holder do not share")]
    [DataRow("",
        """
        public static int norm(this Time t) => cap(t);
        public static int size(this Time t, time_package.Reader r) => r.fromPackage + default(go.Reader).fromNamespace;
        """, DisplayName = "a builtin the package does not declare, and both Reader types qualified")]
    [DataRow("using time = go.time_package;",
        """
        public static int M(this Time t, Time time) => time.v;
        """, DisplayName = "a parameter of the alias's name selecting a member the package does not declare")]
    [DataRow("",
        """
        public static int M(this Time t, Time time) => time.Limit;
        public static int N(this Time t) { Time time = t; return time.Limit; }
        """, DisplayName = "a file without the alias: the local's member is the only reading")]
    public void QualifiedAndUnsharedNamesAreAccepted(string usings, string members)
    {
        ImmutableArray<Diagnostic> diagnostics = Analyze(usings, members);

        Assert.AreEqual(0, diagnostics.Length, string.Join("; ", diagnostics));
    }

    [TestMethod]
    [DataRow("",
        """
        public static int norm(this Time t) => 2;
        public static int twice(this Time t) => norm(t);
        """, "norm", "the method 'go.timeᴛTime.norm(go.time_package.Time)'", DisplayName = "a call that reads as the package function binds the holder's method")]
    [DataRow("",
        """
        public static int norm(this Time t) => 2;
        public static System.Func<Time, int> value(this Time t) => norm;
        """, "norm", "the method 'go.timeᴛTime.norm(go.time_package.Time)'", DisplayName = "a method group")]
    [DataRow("",
        """
        public static int larger(this Time t) => max(1, 2);
        """, "max", "the go.builtin member 'max'", DisplayName = "a call of the package's max binds builtin's generic")]
    [DataRow("",
        """
        public static int size(this Time t) { Reader r = default; return r.fromNamespace; }
        """, "Reader", "the type 'go.Reader'", DisplayName = "the package's type name binds the namespace's type in a body")]
    [DataRow("",
        """
        public static int size(this Time t, Reader r) => r.fromNamespace;
        """, "Reader", "the type 'go.Reader'", DisplayName = "the package's type name binds the namespace's type in a signature")]
    [DataRow("using time = go.time_package;",
        """
        public static int M(this Time t, Time time) => time.Limit;
        """, "time", "the parameter 'time'", DisplayName = "the alias binds a parameter whose type has the member")]
    [DataRow("using time = go.time_package;",
        """
        public static int M(this Time t) { Time time = t; return time.Limit; }
        """, "time", "the local 'time'", DisplayName = "the alias binds a local whose type has the member")]
    public void ASharedNameThatBindsElsewhereIsGO2CS0013(string usings, string members, string name, string bound)
    {
        Diagnostic[] refused = [.. Analyze(usings, members).Where(diagnostic => diagnostic.Id == "GO2CS0013")];

        Assert.AreEqual(1, refused.Length, string.Join("; ", refused.Select(diagnostic => diagnostic.ToString())));
        StringAssert.Contains(refused[0].GetMessage(), $"'{name}' binds {bound}");
        StringAssert.Contains(refused[0].GetMessage(), "'time_package'");
        Assert.AreEqual(DiagnosticSeverity.Error, refused[0].Severity);
        Assert.AreEqual(name, refused[0].Location.SourceTree!.GetText().ToString(refused[0].Location.SourceSpan));
    }

    [TestMethod]
    public void AClassWithoutTheAttributeIsNotRead()
    {
        // The same bodies in a class that is not a method holder: its names are its author's.
        ImmutableArray<Diagnostic> diagnostics = Analyze("using time = go.time_package;",
            """
            public static int norm(this Time t) => 2;
            public static int twice(this Time t) => norm(t) + max(1, 2);
            public static int M(this Time t, Time time, Reader r) => time.Limit + r.fromNamespace;
            """, holder: "time_helpers");

        Assert.AreEqual(0, diagnostics.Length, string.Join("; ", diagnostics));
    }
}
