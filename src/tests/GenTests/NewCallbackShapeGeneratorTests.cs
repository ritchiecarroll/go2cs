// NewCallbackShapeGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// The registrations <see cref="NewCallbackShapeGenerator"/> emits for each shape a converted call site passes
/// to <c>syscall.NewCallback</c> (docs/phase4/DESIGN-newcallback-native-aot.md section 4).
/// </summary>
[TestClass]
public class NewCallbackShapeGeneratorTests
{
    // A stand-in for the converted syscall package, compiled as its own assembly, as the real one is.
    private const string SyscallStub =
        """
        namespace go;

        public static partial class syscall_package
        {
            public static uintptr NewCallback(object fn) => default;
            public static uintptr NewCallbackCDecl(object fn) => default;

            public static class GoNewCallbackShapes
            {
                public static void Register<TR>(System.Type t, System.Func<System.Delegate, System.Func<TR>> f) { }
                public static void Register<T1, TR>(System.Type t, System.Func<System.Delegate, System.Func<T1, TR>> f) { }
                public static void Register<T1, T2, TR>(System.Type t,
                    System.Func<System.Delegate, System.Func<T1, T2, TR>> f) { }
                public static void Register<T1, T2, T3, T4, TR>(System.Type t,
                    System.Func<System.Delegate, System.Func<T1, T2, T3, T4, TR>> f) { }
                public static void Refusal(System.Type t, int arity, bool hasResult, bool floatResult,
                    bool floatArgument) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class GoNewCallbackForwarderAttribute(System.Type holder, string method, int parameter)
                : System.Attribute { }
        }
        """;

    // An older syscall: NewCallback, and no registration.
    private const string OlderSyscallStub =
        """
        namespace go;

        public static partial class syscall_package
        {
            public static uintptr NewCallback(object fn) => default;
        }
        """;

    private static readonly CSharpParseOptions s_options = new(LanguageVersion.Preview);

    private static List<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return
        [
            .. trusted.Split(System.IO.Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path)),
            MetadataReference.CreateFromFile(typeof(golib::go.uintptr).Assembly.Location)
        ];
    }

    private static MetadataReference Library(string name, string source, List<MetadataReference> references,
        bool generate = false)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source, s_options)],
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        if (generate)
        {
            CSharpGeneratorDriver.Create(new NewCallbackShapeGenerator()).WithUpdatedParseOptions(s_options)
                .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

            return output.ToMetadataReference();
        }

        return compilation.ToMetadataReference();
    }

    // Runs the generator over `program` against a syscall stub; returns the generated text (or null) and the
    // compile errors of the result.
    private static (string? generated, string[] errors) Run(string program, string syscall = SyscallStub,
        params (string Name, string Source, bool Generate)[] libraries)
    {
        List<MetadataReference> references = References();
        references.Add(Library("syscall", syscall, references));

        foreach ((string name, string source, bool generate) in libraries)
            references.Add(Library(name, source, references, generate));

        CSharpCompilation compilation = CSharpCompilation.Create("program",
            [CSharpSyntaxTree.ParseText(program, s_options)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new NewCallbackShapeGenerator()).WithUpdatedParseOptions(s_options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string? generated = output.SyntaxTrees.Select(tree => tree.ToString())
            .FirstOrDefault(source => source.Contains("ᴛNewCallbackShapes", StringComparison.Ordinal) ||
                                      source.Contains("GoNewCallbackForwarder(", StringComparison.Ordinal));

        string[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(diagnostic => diagnostic.ToString()).ToArray();

        return (generated, errors);
    }

    private static void AssertCompiles(string[] errors) =>
        Assert.AreEqual(0, errors.Length, "the generated registrations must compile: " + string.Join("; ", errors));

    private const string U = "global::go.uintptr";

    [TestMethod]
    public void EachConvertedShapeRegistersItsType()
    {
        // The shapes as the converter emits them (section 2's table): a lambda and a method group bare, a local,
        // a field of a named func type and a returned func through OrTypedNilFunc.
        (string? generated, string[] errors) = Run(
            """
            namespace go;

            using syscall = syscall_package;

            internal delegate uintptr cbT(uintptr p);

            public static partial class main_package
            {
                private static uintptr none() => default;
                private static uintptr add1(uintptr p) => p;
                private static uintptr two(uintptr a, uintptr b) => a;
                private static uintptr four(uintptr a, uintptr b, uintptr c, uintptr d) => a;
                private static System.Func<uintptr, uintptr, uintptr> make() => two;

                internal static void main()
                {
                    cbT fn = new cbT(add1);
                    var f = add1;
                    syscall.NewCallback(uintptr (uintptr p) => p);
                    syscall.NewCallback(add1);
                    syscall.NewCallback(none);
                    syscall.NewCallback((f).OrTypedNilFunc());
                    syscall.NewCallback((fn).OrTypedNilFunc());
                    syscall.NewCallbackCDecl((object)(make()).OrTypedNilFunc());
                    syscall.NewCallback(four);
                }
            }
            """);

        Assert.IsNotNull(generated, "a compilation that calls NewCallback gets registrations");
        AssertCompiles(errors);
        StringAssert.Contains(generated, "[global::System.Runtime.CompilerServices.ModuleInitializer]");
        StringAssert.Contains(generated, $"Register<{U}>(typeof(global::System.Func<{U}>), static d => (global::System.Func<{U}>)d);");
        StringAssert.Contains(generated,
            $"Register<{U}, {U}>(typeof(global::System.Func<{U}, {U}>), static d => (global::System.Func<{U}, {U}>)d);");
        StringAssert.Contains(generated,
            $"Register<{U}, {U}>(typeof(global::go.cbT), static d => {{ var f = (global::go.cbT)d; return (a1) => f(a1); }});");
        StringAssert.Contains(generated, $"Register<{U}, {U}, {U}>(typeof(global::System.Func<{U}, {U}, {U}>)");
        StringAssert.Contains(generated, $"Register<{U}, {U}, {U}, {U}, {U}>(typeof(global::System.Func<{U}, {U}, {U}, {U}, {U}>)");

        int once = generated.Split('\n').Count(line => line.Contains($"typeof(global::System.Func<{U}, {U}>)"));
        Assert.AreEqual(1, once, "a type passed at two sites is registered once");
    }

    [TestMethod]
    public void AShapeNoBinderTakesRegistersARefusalWithItsFacts()
    {
        (string? generated, string[] errors) = Run(
            """
            namespace go;

            public static partial class main_package
            {
                private static void noResult(uintptr p) { }
                private static uintptr three(uintptr a, double b, uintptr c) => a;

                internal static void main()
                {
                    syscall_package.NewCallback(noResult);
                    syscall_package.NewCallback(three);
                }
            }
            """);

        Assert.IsNotNull(generated);
        AssertCompiles(errors);
        StringAssert.Contains(generated, $"Refusal(typeof(global::System.Action<{U}>), 1, false, false, false);");
        StringAssert.Contains(generated,
            $"Refusal(typeof(global::System.Func<{U}, global::System.Double, {U}, {U}>), 3, true, false, true);");
    }

    [TestMethod]
    public void AForwarderIsMarkedAndItsCallsAreSitesInAnotherAssembly()
    {
        const string forwarder =
            """
            namespace go;

            public static partial class fwd_package
            {
                public static uintptr NewCallback(object fn) => syscall_package.NewCallback(fn);
            }
            """;

        (string? marked, _) = Run(forwarder);

        Assert.IsNotNull(marked, "a method passing its own object parameter to NewCallback is marked");
        StringAssert.Contains(marked,
            "[assembly: global::go.syscall_package.GoNewCallbackForwarder(typeof(global::go.fwd_package), \"NewCallback\", 0)]");

        (string? generated, string[] errors) = Run(
            """
            namespace go;

            public static partial class main_package
            {
                private static uintptr add1(uintptr p) => p;

                internal static void main() => fwd_package.NewCallback(add1);
            }
            """, SyscallStub, ("fwd", forwarder, true));

        Assert.IsNotNull(generated, "a call of a marked method in a referencing assembly is a site");
        AssertCompiles(errors);
        StringAssert.Contains(generated, $"Register<{U}, {U}>(typeof(global::System.Func<{U}, {U}>)");
    }

    [TestMethod]
    public void AGoMethodForwarderMatchesWhenCalledThroughItsReceiver()
    {
        // A Go method converts to an extension method whose receiver is parameter 0; its call binds the reduced
        // method, whose parameters leave the receiver out.
        const string forwarder =
            """
            namespace go;

            public partial struct Proc { }

            public static partial class fwd_package
            {
                public static uintptr NewCallback(this ref Proc p, object fn) => syscall_package.NewCallback(fn);
            }
            """;

        (string? marked, _) = Run(forwarder);

        Assert.IsNotNull(marked);
        StringAssert.Contains(marked,
            "[assembly: global::go.syscall_package.GoNewCallbackForwarder(typeof(global::go.fwd_package), \"NewCallback\", 1)]");

        (string? generated, string[] errors) = Run(
            """
            namespace go;

            public static partial class main_package
            {
                private static uintptr add1(uintptr p) => p;

                internal static void main()
                {
                    Proc p = default;
                    p.NewCallback(add1);
                }
            }
            """, SyscallStub, ("fwd", forwarder, true));

        Assert.IsNotNull(generated, "a receiver call of a marked Go method is a site");
        AssertCompiles(errors);
        StringAssert.Contains(generated, $"Register<{U}, {U}>(typeof(global::System.Func<{U}, {U}>)");
    }

    [TestMethod]
    public void APointerReceiverForwarderCalledOnABoxInItsOwnAssemblyIsASite()
    {
        // `func (p *Proc) NewCallback(fn any) uintptr` called as `p.NewCallback(add1)` with `p` a `*Proc`, in the
        // forwarder's own package: the converter declares the method on `this ref Proc`, and the call binds
        // RecvGenerator's `this ж<Proc>` overload, which this generator cannot see (P3's delta read, SHOULD 1).
        (string? generated, string[] errors) = Run(
            """
            namespace go;

            public static partial class main_package
            {
                public partial struct Proc { }

                public static uintptr NewCallback(this ref Proc p, object fn) => syscall_package.NewCallback(fn);

                private static uintptr add1(uintptr p) => p;

                internal static void main()
                {
                    ж<Proc> p = null!;
                    p.NewCallback(add1);
                }
            }
            """);

        Assert.IsNotNull(generated);
        StringAssert.Contains(generated,
            "[assembly: global::go.syscall_package.GoNewCallbackForwarder(typeof(global::go.main_package), \"NewCallback\", 1)]");
        StringAssert.Contains(generated, $"Register<{U}, {U}>(typeof(global::System.Func<{U}, {U}>)",
            "the call on the box is a site: " + generated);
        Assert.IsFalse(errors.Any(error => error.Contains("newcallback-shapes", StringComparison.Ordinal)),
            "the generated registrations must compile: " + string.Join("; ", errors));
    }

    [TestMethod]
    public void AMethodValueRegistersItsType()
    {
        // `syscall.NewCallback(w.wndProc)`: the converter passes a Go method value as a bare method group of the
        // reduced extension method, its receiver a pointer box (`p.pointerMethod`, MethodValueFuncNames).
        (string? generated, string[] errors) = Run(
            """
            namespace go;

            public sealed class window { }

            public readonly struct msgW { }

            public static partial class main_package
            {
                internal static uintptr wndProc(this window w, msgW m) => default;

                internal static void main()
                {
                    window w = new();
                    syscall_package.NewCallback(w.wndProc);
                }
            }
            """);

        Assert.IsNotNull(generated);
        AssertCompiles(errors);
        StringAssert.Contains(generated, $"Register<global::go.msgW, {U}>(typeof(global::System.Func<global::go.msgW, {U}>)");
    }

    [TestMethod]
    public void APointerMethodValueRegistersItsType()
    {
        // `w.wndProc` with `w` a `*window`: the converter declares wndProc on `this ref window` and RecvGenerator
        // adds the `this ж<window>` overload the call binds. That overload is another generator's output, which
        // this one cannot see, so here the call does not bind; the registration is still the overload's type, and
        // the generated file itself must compile.
        (string? generated, string[] errors) = Run(
            """
            namespace go;

            public readonly struct msgW { }

            public static partial class main_package
            {
                public partial struct window { }

                internal static uintptr wndProc(this ref window w, msgW m) => default;

                internal static void main()
                {
                    ж<window> w = null!;
                    syscall_package.NewCallback(w.wndProc);
                }
            }
            """);

        Assert.IsNotNull(generated);
        StringAssert.Contains(generated, $"Register<global::go.msgW, {U}>(typeof(global::System.Func<global::go.msgW, {U}>)");
        Assert.IsFalse(errors.Any(error => error.Contains("newcallback-shapes", StringComparison.Ordinal)),
            "the generated registrations must compile: " + string.Join("; ", errors));
    }

    [TestMethod]
    public void APointerInTheSignatureIsLeftUnregistered()
    {
        (string? generated, string[] errors) = Run(
            """
            namespace go;

            public ref struct span { }

            file delegate nuint fileLocal(nuint p);

            public static unsafe partial class main_package
            {
                private static nuint raw(void* p) => default;
                private static nuint byFunction(delegate* unmanaged<void> p) => default;
                private static nuint bySpan(span s) => default;

                internal static void main()
                {
                    syscall_package.NewCallback((System.Func<nint, nuint>)(p => default));
                    syscall_package.NewCallback(raw);
                    syscall_package.NewCallback(byFunction);
                    syscall_package.NewCallback(bySpan);
                    syscall_package.NewCallback((fileLocal)(p => p));
                }
            }
            """);

        AssertCompiles(errors);
        Assert.IsNotNull(generated, "the nint shape still registers");
        Assert.IsFalse(generated.Contains("void*", StringComparison.Ordinal) || generated.Contains("delegate*", StringComparison.Ordinal),
            "a pointer or function-pointer type is never a registration's type argument: " + generated);
        Assert.IsFalse(
            generated.Contains("span", StringComparison.Ordinal) || generated.Contains("fileLocal", StringComparison.Ordinal),
            "a ref-like or file-local type is never a registration's type argument: " + generated);
    }

    [TestMethod]
    public void ShapesThatCannotBeNamedAreLeftUnregistered()
    {
        (string? generated, string[] errors) = Run(
            """
            namespace go;

            public static partial class main_package
            {
                private delegate uintptr byRef(ref uintptr p);

                internal static uintptr reg<T>(System.Func<T, uintptr> f) => syscall_package.NewCallback(f);

                internal static void main()
                {
                    syscall_package.NewCallback((ref uintptr p) => p);
                    other.call();
                }
            }

            internal static class other
            {
                private delegate uintptr hidden(uintptr p);

                internal static void call() => syscall_package.NewCallback((hidden)(p => p));
            }
            """);

        AssertCompiles(errors);
        Assert.IsNull(generated, "a type parameter, a compiler-made type and an inaccessible type register nothing: " +
                                 generated);
    }

    [TestMethod]
    public void AnOlderSyscallGetsNothing()
    {
        (string? generated, string[] errors) = Run(
            """
            namespace go;

            public static partial class main_package
            {
                private static uintptr add1(uintptr p) => p;

                internal static void main() => syscall_package.NewCallback(add1);
            }
            """, OlderSyscallStub);

        AssertCompiles(errors);
        Assert.IsNull(generated, "a syscall without GoNewCallbackShapes must not get calls it cannot bind");
    }
}
