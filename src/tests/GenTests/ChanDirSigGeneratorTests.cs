// ChanDirSigGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Pins what <see cref="ChanDirSigGenerator"/> reads from the converter's OWN rendering of a directional
/// channel (NEW-1b): the input is the committed emission of the ReflectFuncChanDir behavioral fixture, so a
/// change to how the converter spells <c>/*&lt;-*/</c> breaks these arms rather than passing a hand-written
/// copy of the old spelling.
/// </summary>
/// <remarks>
/// The fixture's other references (fmt, reflect, time, error) stay unresolved here; only the types a
/// [GoSigChanDir] entry names are declared, in a stub tree, so each entry's typeof renders as it does in a
/// real build.
/// </remarks>
[TestClass]
public class ChanDirSigGeneratorTests
{
    private const string Stubs = """
        namespace go;

        public sealed class channel<T> { }
        public sealed class ж<T> { }
        public readonly struct @string { }
        public struct EmptyStruct { }
        public enum GoChanDir : byte { Unstamped, Recv, Send, Both }
        public sealed class GoRecvAttribute : System.Attribute { }
        public sealed class GoTypeAttribute : System.Attribute { public GoTypeAttribute() { } public GoTypeAttribute(string type) { } }
        public sealed class GoChanDirAttribute : System.Attribute { public GoChanDirAttribute(GoChanDir dir) { } }
        """;

    private const string Recv = "global::go.GoChanDir.Recv";
    private const string Send = "global::go.GoChanDir.Send";
    private const string Unstamped = "global::go.GoChanDir.Unstamped";

    private static readonly Lazy<string[]> s_generated = new(() => RunOverFixture(FixtureSource()));

    // The same fixture as face lift A's converter emits it: a Go pointer receiver is an unmarked `this ref T`.
    private static readonly Lazy<string[]> s_generatedUnmarked = new(() => RunOverFixture(FixtureSource().Replace("[GoRecv] ", "")));

    // The committed fixture emission, found by walking up from the test binary to the repository's src.
    private static string FixtureSource()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, "src", "tests", "Behavioral", "ReflectFuncChanDir", "main.cs");

            if (File.Exists(candidate))
                return File.ReadAllText(candidate);
        }

        throw new FileNotFoundException("src/tests/Behavioral/ReflectFuncChanDir/main.cs not found above " + AppContext.BaseDirectory);
    }

    private static string[] RunOverFixture(string fixture) => RunWithDiagnostics(fixture).generated;

    private static (string[] generated, Diagnostic[] diagnostics) RunWithDiagnostics(string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create("test",
            [CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(Stubs)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ChanDirSigGenerator());
        GeneratorDriverRunResult result = driver.RunGenerators(compilation).GetRunResult();
        return (result.GeneratedTrees.Select(tree => tree.ToString()).ToArray(), result.Diagnostics.ToArray());
    }

    private static string Entry(string method, string[] parameterTypes, string[] parameterDirs, string[] resultDirs) =>
        $"[global::go.GoSigChanDir(\"{method}\", new global::System.Type[] {{ {string.Join(", ", parameterTypes)} }}, " +
        $"new global::go.GoChanDir[] {{ {string.Join(", ", parameterDirs)} }}, " +
        $"new global::go.GoChanDir[] {{ {string.Join(", ", resultDirs)} }})]";

    private static string PackageClassPart() => PackageClassPart(s_generated.Value);

    private static string PackageClassPart(string[] generated) =>
        generated.Single(source => source.Contains("partial class main_package") && !source.Contains("partial interface"));

    [TestMethod]
    public void ARecvParameterIsReadBeforeTheType()
    {
        string part = PackageClassPart();
        StringAssert.Contains(part, Entry("recvLocal", ["typeof(global::go.channel<nint>)"], [Recv], []));
        Assert.IsFalse(part.Contains("\"bidiLocal\""), "a bidirectional channel carries no direction, so it gets no entry");
    }

    [TestMethod]
    public void ASendParameterIsReadBetweenTheNameAndItsTypeArguments()
    {
        StringAssert.Contains(PackageClassPart(), Entry("sendOnly", ["typeof(global::go.channel<global::go.@string>)"], [Send], []));
    }

    [TestMethod]
    public void ANestedDirectionCarriesTheOuterChannelOnly()
    {
        // chan<- <-chan int: the inner receive marker is the recorded residual and must not surface.
        StringAssert.Contains(PackageClassPart(), Entry("nestedDir", ["typeof(global::go.channel<global::go.channel<nint>>)"], [Send], []));
    }

    [TestMethod]
    public void AnInterfaceMemberIsStampedOnItsInterfaceInsideThePackageClass()
    {
        string part = s_generated.Value.Single(source => source.Contains("partial interface Notifier"));
        string expected = string.Join("\r\n",
            "partial class main_package",
            "{",
            "    " + Entry("Done", [], [], [Recv]),
            "    partial interface Notifier",
            "    {",
            "    }",
            "}");
        StringAssert.Contains(part, expected);
    }

    [TestMethod]
    public void AnExtensionMethodIsKeyedWithItsReceiver()
    {
        string part = PackageClassPart();
        StringAssert.Contains(part, Entry("Equal", ["typeof(global::go.main_package.R)", "typeof(global::go.channel<bool>)"], [Unstamped, Recv], [Unstamped]));
        StringAssert.Contains(part, Entry("Equal", ["typeof(global::go.main_package.AssignD)", "typeof(global::go.channel<bool>)"], [Unstamped, Recv], [Unstamped]));
        StringAssert.Contains(part, Entry("mixed", ["typeof(nint)", "typeof(global::go.channel<nint>)"], [Unstamped, Send], [Recv]));
        StringAssert.Contains(part, Entry("two", [], [], [Recv, Unstamped]));
    }

    [TestMethod]
    public void AGoRecvMethodIsAlsoKeyedOnItsBoxedReceiverOverload()
    {
        string part = PackageClassPart();
        StringAssert.Contains(part, Entry("Feed", ["typeof(global::go.main_package.S)", "typeof(global::go.channel<nint>)"], [Unstamped, Send], []));
        StringAssert.Contains(part, Entry("Feed", ["typeof(global::go.ж<global::go.main_package.S>)", "typeof(global::go.channel<nint>)"], [Unstamped, Send], []));
    }

    // Face lift A: the converter writes no [GoRecv], so a pointer receiver is known by its unmarked `this ref`
    // alone (MethodDeclarationSyntaxExtensions.IsPointerSetMethod), and still gets its boxed-receiver entry.
    [TestMethod]
    public void AnUnmarkedRefReceiverIsAlsoKeyedOnItsBoxedReceiverOverload()
    {
        string part = PackageClassPart(s_generatedUnmarked.Value);
        StringAssert.Contains(part, Entry("Feed", ["typeof(global::go.main_package.S)", "typeof(global::go.channel<nint>)"], [Unstamped, Send], []));
        StringAssert.Contains(part, Entry("Feed", ["typeof(global::go.ж<global::go.main_package.S>)", "typeof(global::go.channel<nint>)"], [Unstamped, Send], []));
    }

    // Face lift D2: a generic method is carried. A type built from a type parameter is keyed by its open definition,
    // a bare type parameter by null, and a generic pointer receiver's boxed overload by the open ж<>.
    [TestMethod]
    public void AGenericMethodIsKeyedByOpenDefinitions()
    {
        const string source = """
            namespace go;

            public static partial class gen_package {
                public partial struct Box<T> { }

                internal static void Take<T>(/*<-*/channel<T> c) { }

                internal static void Put<T>(this ref Box<T> b, T x, channel/*<-*/<T> c) { }
            }
            """;

        (string[] generated, Diagnostic[] diagnostics) = RunWithDiagnostics(source);
        Assert.AreEqual(0, diagnostics.Length, string.Join("\n", diagnostics.Select(diagnostic => diagnostic.ToString())));

        string part = generated.Single(text => text.Contains("partial class gen_package"));
        StringAssert.Contains(part, Entry("Take", ["typeof(global::go.channel<>)"], [Recv], []));
        StringAssert.Contains(part, Entry("Put", ["typeof(global::go.gen_package.Box<>)", "null", "typeof(global::go.channel<>)"], [Unstamped, Unstamped, Send], []));
        StringAssert.Contains(part, Entry("Put", ["typeof(global::go.ж<>)", "null", "typeof(global::go.channel<>)"], [Unstamped, Unstamped, Send], []));
    }

    // A key that would also match another method of the declaring type is refused at compile time, naming both,
    // never written for golib to refuse at run time.
    [TestMethod]
    public void AKeyThatMatchesTwoMethodsIsAnErrorNamingBoth()
    {
        const string source = """
            namespace go;

            public static partial class gen_package {
                internal static void Take<T>(/*<-*/channel<T> c) { }

                internal static void Take(channel<nint> c) { }
            }
            """;

        (string[] generated, Diagnostic[] diagnostics) = RunWithDiagnostics(source);
        Diagnostic refusal = diagnostics.Single(diagnostic => diagnostic.Id == "GO2CS0003");
        Assert.AreEqual(DiagnosticSeverity.Error, refusal.Severity);
        StringAssert.Contains(refusal.GetMessage(), "Take<T>");
        StringAssert.Contains(refusal.GetMessage(), "Take(go.channel<nint>)");
        Assert.IsFalse(generated.Any(text => text.Contains("GoSigChanDir(\"Take\"")), "no entry is written for a refused key");
    }
}
