// PromotedHostClassTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A struct's forwarders for the methods it promotes from an embed of its own package are written into a class of
/// the struct's own, <c>{pkg}ᴛ{Struct}ᴛpkg</c> marked <c>[GoPromotedHost(typeof(Struct))]</c>, beside the package
/// class and never into it.
/// </summary>
/// <remarks>
/// <para>
/// RED before the class existed: every forwarder was a member of the package class, and N methods of one base struct
/// embedded by M structs put 2 x N x M methods there. CoreCLR loads no static class with more than 65,521 methods,
/// and one converted package class held 107,835 forwarders and threw TypeLoadException at its first use
/// (docs/phase4/DESIGN-promoted-forwarder-placement.md).
/// </para>
/// <para>
/// The tests run the real TypeGenerator. The upstream package of the metadata arm carries, hand-written, the class its
/// own generator run would have written, under a name no rule derives: a reader finds it by its attribute.
/// </para>
/// </remarks>
[TestClass]
public class PromotedHostClassTests
{
    private const string GolibSource =
        """
        namespace go
        {
            public class ж<T> { public ж(T value) { } public T Value = default!; }

            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            public class GoPackageAttribute : System.Attribute
            {
                public GoPackageAttribute(string name) { }
            }

            public class GoRecvAttribute : System.Attribute { }

            public class GoPromotedAttribute : System.Attribute { }

            public class GoPromotedHostAttribute : System.Attribute
            {
                public GoPromotedHostAttribute(System.Type @struct) { }
            }
        }
        """;

    // One base struct with a value method and a pointer method, embedded by an unexported struct, an exported one (named apart from it: a hint name is
    // compared without case), a
    // generic one, and three more that stand for the M of the N x M shape.
    internal const string Package =
        """
        using go;

        namespace go
        {
            [GoPackage("pa")]
            public static partial class pa_package
            {
                [GoType] partial struct inner { public int n; }

                internal static int Val(this inner i) => i.n;

                [GoRecv] internal static void Ptr(this ref inner i) { }

                [GoType] partial struct outer
                {
                    internal partial ref inner inner { get; }
                }

                [GoType] partial struct Public
                {
                    internal partial ref inner inner { get; }
                }

                [GoType] partial struct Box<T>
                {
                    internal partial ref inner inner { get; }
                }

                [GoType] partial struct m1 { internal partial ref inner inner { get; } }

                [GoType] partial struct m2 { internal partial ref inner inner { get; } }

                [GoType] partial struct m3 { internal partial ref inner inner { get; } }
            }
        }
        """;

    // Package pb as a consumer reads it from metadata: Mid embeds Base, and Mid's forwarder for Base's method is in
    // Mid's promoted-method class. {0} is the attribute that names Mid, or nothing.
    private const string UpstreamFormat =
        """
        namespace go
        {{
            [GoPackage("pb")]
            public static partial class pb_package
            {{
                public partial struct Base {{ public int N; }}

                public partial struct Mid
                {{
                    public Base ʗBase;
                }}

                public static string Name(this Base b) => "";
            }}

            {0}
            [global::System.CodeDom.Compiler.GeneratedCode("go2cs-gen", "1.0")]
            public static class forwardersOfMid
            {{
                [GoPromoted] public static string Name(this global::go.pb_package.Mid target) => "";
                public static string Name(this ж<global::go.pb_package.Mid> Ꮡtarget) => "";
            }}
        }}
        """;

    private const string Consumer =
        """
        using go;

        namespace go
        {
            [GoPackage("main")]
            public static partial class main_package
            {
                [GoType] partial struct Outer
                {
                    public partial ref pb_package.Mid Mid { get; }
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

    // Compiles golib and each upstream package in turn, then runs the TypeGenerator over the package under test.
    internal static Dictionary<string, string> Generate(string source, params string[] upstream)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);
        List<MetadataReference> references = [.. CoreReferences()];

        foreach ((string each, int index) in upstream.Prepend(GolibSource).Select((each, index) => (each, index)))
        {
            CSharpCompilation compilation = CSharpCompilation.Create($"ph-upstream{index}", [CSharpSyntaxTree.ParseText(each, parseOptions)], references, libraryOptions);

            using MemoryStream image = new();
            EmitResult emitted = compilation.Emit(image);
            Assert.IsTrue(emitted.Success, $"upstream {index} must emit clean: {string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");

            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
        }

        CSharpCompilation consumer = CSharpCompilation.Create("ph-main", [CSharpSyntaxTree.ParseText(source, parseOptions)], references, libraryOptions);
        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new TypeGenerator()).RunGenerators(consumer).GetRunResult();

        foreach (GeneratorRunResult each in result.Results)
            Assert.IsNull(each.Exception, $"the generator must not throw: {each.Exception}");

        Assert.AreEqual(0, result.Diagnostics.Length, $"the generator must report no diagnostics: {string.Join("; ", result.Diagnostics)}");

        return result.Results.SelectMany(generator => generator.GeneratedSources).ToDictionary(generated => generated.HintName, generated => generated.SourceText.ToString());
    }

    private static string GeneratedFor(Dictionary<string, string> sources, string structName)
    {
        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains($"_package.{structName}.g.cs"));
        Assert.IsNotNull(key, $"the TypeGenerator must generate for {structName}; got: {string.Join(", ", sources.Keys)}");
        return sources[key!];
    }

    // The static extension methods each class of a generated source declares, by the class's name.
    private static Dictionary<string, List<MethodDeclarationSyntax>> ExtensionMethods(string generated) =>
        CSharpSyntaxTree.ParseText(generated).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(method => method.Parent is ClassDeclarationSyntax && method.ParameterList.Parameters.FirstOrDefault()?.Modifiers.Any(SyntaxKind.ThisKeyword) == true)
            .GroupBy(method => ((ClassDeclarationSyntax)method.Parent!).Identifier.Text)
            .ToDictionary(group => group.Key, group => group.ToList());

    [TestMethod]
    public void SamePackageForwardersAreWrittenIntoTheStructsOwnClass()
    {
        string generated = GeneratedFor(Generate(Package), "outer");
        Dictionary<string, List<MethodDeclarationSyntax>> methods = ExtensionMethods(generated);

        Assert.IsFalse(methods.ContainsKey("pa_package"), "the package class holds no forwarder: " + generated);
        Assert.IsTrue(methods.ContainsKey("paᴛouterᴛpkg"), "the struct's own class holds its forwarders: " + generated);

        // A value method and a pointer method, each in its value form and its pointer form.
        Assert.AreEqual(4, methods["paᴛouterᴛpkg"].Count, generated);

        // The class is as visible as its struct, names the struct for a reader of metadata, and is listed for golib's
        // extension-method scan.
        StringAssert.Contains(generated, "[global::go.GoPromotedHost(typeof(global::go.pa_package.outer)), ");
        StringAssert.Contains(generated, "internal static class paᴛouterᴛpkg");
        StringAssert.Contains(generated, "typeof(global::go.paᴛouterᴛpkg)");

        // Outside the package class a forwarder names its struct in full.
        StringAssert.Contains(generated, "[global::go.GoPromoted] internal static int Val(this global::go.pa_package.outer ");
        StringAssert.Contains(generated, "internal static int Val(this ж<global::go.pa_package.outer> Ꮡ");
        StringAssert.Contains(generated, "[global::go.GoPromoted] [global::go.GoRecv] internal static void Ptr(this ref global::go.pa_package.outer ");
        StringAssert.Contains(generated, "internal static void Ptr(this ж<global::go.pa_package.outer> Ꮡ");
    }

    [TestMethod]
    public void AnExportedStructsClassIsPublicAndAGenericStructIsNamedUnbound()
    {
        Dictionary<string, string> sources = Generate(Package);

        string exported = GeneratedFor(sources, "Public");
        StringAssert.Contains(exported, "[global::go.GoPromotedHost(typeof(global::go.pa_package.Public)), ");
        StringAssert.Contains(exported, "public static class paᴛPublicᴛpkg");

        string generic = GeneratedFor(sources, "Box_T_");
        StringAssert.Contains(generic, "[global::go.GoPromotedHost(typeof(global::go.pa_package.Box<>)), ");
        StringAssert.Contains(generic, "public static class paᴛBoxᴛpkg");
        StringAssert.Contains(generic, " Val<T>(this global::go.pa_package.Box<T> ");
    }

    // A struct that declares the one method its embed would promote: there is nothing to forward.
    private const string Overridden =
        """
        using go;

        namespace go
        {
            [GoPackage("pc")]
            public static partial class pc_package
            {
                [GoType] partial struct inner { public int n; }

                internal static int Val(this inner i) => i.n;

                [GoType] partial struct Over
                {
                    internal partial ref inner inner { get; }
                }

                internal static int Val(this Over o) => 1;
            }
        }
        """;

    // RED while the class was declared for the walk's text: crypto/ecdsa's PrivateKey, which declares all three of
    // PublicKey's methods, got a public class holding three comments. A class with no member is a type of the
    // package's public surface that nothing can bind, and taking it away later is a break.
    [TestMethod]
    public void AStructThatForwardsNothingGetsNoClass()
    {
        string generated = GeneratedFor(Generate(Overridden), "Over");

        Assert.IsFalse(generated.Contains("pcᴛOverᴛpkg"), "no class, and no extension-host row naming one: " + generated);
        Assert.IsFalse(generated.Contains("GoPromotedHost"), generated);
        Assert.AreEqual(0, ExtensionMethods(generated).Count, generated);

        // What was not promoted, and why, is still said in the file.
        StringAssert.Contains(generated, "\r\n// 'inner.Val' method mapped to overridden 'Over' receiver method");
    }

    // The N x M shape at a size a test can read: the package class gains no method however many structs embed the
    // base, and each struct's class holds two forms of each promoted method.
    [TestMethod]
    public void ThePackageClassGainsNothingHoweverManyStructsEmbedTheBase()
    {
        Dictionary<string, string> sources = Generate(Package);

        foreach ((string hint, string name) in new[] { ("outer", "outer"), ("Public", "Public"), ("Box_T_", "Box"), ("m1", "m1"), ("m2", "m2"), ("m3", "m3") })
        {
            Dictionary<string, List<MethodDeclarationSyntax>> methods = ExtensionMethods(GeneratedFor(sources, hint));

            Assert.IsFalse(methods.ContainsKey("pa_package"), $"{name}: the package class holds no forwarder");
            Assert.AreEqual(4, methods[$"paᴛ{name}ᴛpkg"].Count, $"{name}: two promoted methods, two forms each");
        }
    }

    [TestMethod]
    public void AMetadataReaderFindsTheClassByItsAttribute()
    {
        string upstream = string.Format(UpstreamFormat, "[GoPromotedHost(typeof(pb_package.Mid))]");
        string generated = GeneratedFor(Generate(Consumer, upstream), "Outer");

        // Outer embeds pb.Mid, whose Name is a forwarder in Mid's own class: read from there, it promotes on.
        StringAssert.Contains(generated, "public static class mainᴛOuterᴛxpkg");
        StringAssert.Contains(generated, "[global::go.GoPromoted] public static string Name(this global::go.main_package.Outer target) => target.Mid.Name();");
    }

    // The control: the same class without the attribute is no struct's promoted-method class, whatever its name.
    [TestMethod]
    public void AClassWithoutTheAttributeIsNotRead()
    {
        string generated = GeneratedFor(Generate(Consumer, string.Format(UpstreamFormat, "")), "Outer");

        Assert.IsFalse(generated.Contains(" Name("), "no forwarder is read from an unmarked class: " + generated);
    }

    // A generic base whose method names its type parameter, embedded with a type argument that does not exist when
    // the generator runs: the output of another go2cs-gen generator, as a constraint proxy is.
    private const string GeneratedArgument =
        """
        namespace go
        {
            [GoPackage("pd")]
            public static partial class pd_package
            {
                [GoType] internal partial struct curve<P> { public int n; }

                internal static int LabelOf<P>(this curve<P> c, P p) => c.n;

                [GoType] internal partial struct p9Curve
                {
                    /*embed*/ internal curve<p9жpoint> curve;
                }
            }
        }
        """;

    // RED when the class first left the package class: the forwarder names `p9жpoint` bare, which bound as a nested
    // type while the package class held the forwarder and binds nothing beside it (CS0246 x2, the behavioral
    // GenericEmbedPromotion under the go2cs.slnx build). The generated file's static using of the package class binds it.
    [TestMethod]
    public void AForwarderThatNamesAGeneratedTypeBindsItThroughThePackageClass()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        string generated = GeneratedFor(Generate(GeneratedArgument), "p9Curve");

        // The type is no symbol when the generator runs, so no full name can be written for it.
        StringAssert.Contains(generated, "(this global::go.pd_package.p9Curve target, p9жpoint p)");

        CompilationUnitSyntax unit = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(generated, parseOptions).GetRoot();
        ClassDeclarationSyntax holder = unit.DescendantNodes().OfType<ClassDeclarationSyntax>().Single(type => type.Identifier.ValueText == "pdᴛp9Curveᴛpkg");

        // The class under the using directives of its own file, beside the package and the type the other generator adds.
        string classFile = $"{string.Concat(unit.Usings.Select(directive => directive.ToFullString()))}\nnamespace go;\n{holder.ToFullString()}";
        const string Proxy = "namespace go { public static partial class pd_package { internal partial struct p9жpoint { } } }";

        List<MetadataReference> references = [.. CoreReferences()];
        CSharpCompilation golib = CSharpCompilation.Create("ph-golib", [CSharpSyntaxTree.ParseText(GolibSource, parseOptions)], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using MemoryStream golibImage = new();
        Assert.IsTrue(golib.Emit(golibImage).Success, "the stand-in golib must emit clean");
        references.Add(MetadataReference.CreateFromImage(golibImage.ToArray()));

        CSharpCompilation package = CSharpCompilation.Create("ph-proxy",
            [CSharpSyntaxTree.ParseText(GeneratedArgument, parseOptions), CSharpSyntaxTree.ParseText(Proxy, parseOptions), CSharpSyntaxTree.ParseText(classFile, parseOptions)],
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] errors = [.. package.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];

        Assert.AreEqual(0, errors.Length, "the promoted-method class binds every type it names: " + string.Join("; ", errors.Select(error => error.ToString())));
    }

    // A package as its build compiles it, with the embed in the field form the generator also reads. Val returns a
    // type with an exported name, so its forwarder is public.
    private const string Library =
        """
        namespace go
        {
            [GoPackage("pc")]
            public static partial class pc_package
            {
                [GoType] public partial struct Inner { public int n; }

                [GoType] public partial struct Outer
                {
                    /*embed*/ public Inner Inner;
                }

                public static Inner Val(this Inner i) => i;

                public static int Own(this Outer o) => 1;
            }
        }
        """;

    // C# written by hand against the package, in a namespace of its own. {0} is its using directives.
    private const string CallerFormat =
        """
        {0}

        namespace app
        {{
            public static class Caller
            {{
                public static int Run()
                {{
                    Outer o = new Outer {{ Inner = new Inner {{ n = 41 }} }};
                    return o.Own() + o.Val().n;
                }}
            }}
        }}
        """;

    // The package compiled with the promoted-method class the generator writes for Outer, then the caller against it.
    private static Diagnostic[] CompileCaller(string usings)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);
        List<MetadataReference> references = [.. CoreReferences()];

        CSharpCompilation golib = CSharpCompilation.Create("ph-golib", [CSharpSyntaxTree.ParseText(GolibSource, parseOptions)], references, libraryOptions);
        using MemoryStream golibImage = new();
        Assert.IsTrue(golib.Emit(golibImage).Success, "the stand-in golib must emit clean");
        references.Add(MetadataReference.CreateFromImage(golibImage.ToArray()));

        string generated = GeneratedFor(Generate(Library), "Outer");

        ClassDeclarationSyntax holder = CSharpSyntaxTree.ParseText(generated, parseOptions).GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>().Single(type => type.Identifier.ValueText == "pcᴛOuterᴛpkg");

        // The struct's main generated part is left out, as in GoMethodSetTests: only its promoted-method class is taken.
        CSharpCompilation library = CSharpCompilation.Create("ph-library",
            [CSharpSyntaxTree.ParseText(Library, parseOptions), CSharpSyntaxTree.ParseText($"namespace go;\n{holder.ToFullString()}", parseOptions)],
            references, libraryOptions);

        using MemoryStream libraryImage = new();
        EmitResult emitted = library.Emit(libraryImage);
        Assert.IsTrue(emitted.Success, $"the package and its promoted-method class must emit clean: {string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");
        references.Add(MetadataReference.CreateFromImage(libraryImage.ToArray()));

        CSharpCompilation caller = CSharpCompilation.Create("ph-caller", [CSharpSyntaxTree.ParseText(string.Format(CallerFormat, usings), parseOptions)], references, libraryOptions);

        return [.. caller.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
    }

    // The consumer's view (docs/ConsumingGoFromCSharp.md): `using static` of the package class alone brings the types
    // and the methods they declare, and no method a struct promotes. Before 1.26.10.4 it brought those too.
    [TestMethod]
    public void UsingStaticAloneBringsDeclaredMethodsAndNoPromotedOne()
    {
        Diagnostic[] errors = CompileCaller("using static go.pc_package;");

        Assert.AreEqual(1, errors.Length, "only the promoted call fails to bind: " + string.Join("; ", errors.Select(error => error.ToString())));

        // CS1929: the only Val in scope is the one the embedded type declares, and an Outer is not its receiver.
        Assert.AreEqual("CS1929", errors[0].Id);
        StringAssert.Contains(errors[0].GetMessage(), "'Val'");
    }

    [TestMethod]
    public void TheNamespaceBringsThePromotedMethods()
    {
        Diagnostic[] errors = CompileCaller("using go;\nusing static go.pc_package;");

        Assert.AreEqual(0, errors.Length, "the namespace brings the struct's promoted-method class: " + string.Join("; ", errors.Select(error => error.ToString())));
    }
}
