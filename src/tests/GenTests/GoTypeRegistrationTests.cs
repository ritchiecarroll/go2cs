// GoTypeRegistrationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards the trimming registry (docs/PLAN-golib-full-trim.md, stage 2; golib's GoTypeRegistry): every type the
/// TypeGenerator generates for is registered by its package's module initializer with a DynamicDependency for the
/// members golib reflects over, so a full trim keeps them. golib's reflection sites suppress their trim warnings
/// on the strength of this registry, so a type the generator generates for but does not register must fail HERE,
/// by name, not in a published program.
/// </summary>
[TestClass]
public class GoTypeRegistrationTests
{
    private const string Path = @"C:\go2cs\src\core\ctest\ctest.cs";

    // One of every kind the TypeGenerator generates for, plus two it must NOT register (a nested type and a
    // descriptor carrier are not Go types).
    private const string Package =
        """
        namespace go;

        [GoPackage("ctest")]
        public static partial class ctest_package
        {
            partial struct Plain { internal nint X; }

            partial struct Pair<T> { internal T First; internal T Second; }

            partial interface Shape { nint Area(); }

            partial struct Names /*[]@string*/;

            partial struct Index /*map[@string, nint]*/;

            partial struct Pipe /*chan nint*/;

            partial class Ref /*ж<nint>*/;

            [GoType("[]nint")] partial struct Marked;

            partial struct Outer
            {
                internal nint Y;

                partial struct Inner { internal nint Z; }
            }

            [GoLocalName("Token")] public interface Tokenᴅ { }
        }
        """;

    private static readonly (string type, DynamicallyAccessedMemberTypes members)[] Expected =
    [
        ("Plain", golib::go.GoTypeRegistry.StructMembers),
        ("Pair`1", golib::go.GoTypeRegistry.StructMembers),
        ("Shape", golib::go.GoTypeRegistry.InterfaceMembers),
        ("Names", golib::go.GoTypeRegistry.WrapperMembers),
        ("Index", golib::go.GoTypeRegistry.WrapperMembers),
        ("Pipe", golib::go.GoTypeRegistry.WrapperMembers),
        ("Ref", golib::go.GoTypeRegistry.WrapperMembers),
        ("Marked", golib::go.GoTypeRegistry.WrapperMembers),
        ("Outer", golib::go.GoTypeRegistry.StructMembers)
    ];

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        """;

    // Run the TypeGenerator and answer the package class as a SYMBOL of the output compilation. Symbols rather than
    // an emitted assembly: this Roslyn predates C# 14, the language a converted project builds with, so the struct
    // template's generated ToString does not bind here (an ambiguous string.Join under Preview) although it does in
    // every real build. The registration file itself must bind cleanly, and that is asserted.
    private static (INamedTypeSymbol packageClass, Compilation output) Build(string source)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("ctest",
            [CSharpSyntaxTree.ParseText(source, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\ctest\obj\ctest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        INamedTypeSymbol packageClass = output.GetTypeByMetadataName("go.ctest_package")!;
        Assert.IsNotNull(packageClass, "the fixture's package class");

        return (packageClass, output);
    }

    private static IEnumerable<IMethodSymbol> RegistrationMethods(INamedTypeSymbol packageClass) =>
        packageClass.GetMembers().OfType<IMethodSymbol>()
            .Where(method => method.IsStatic && method.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == nameof(ModuleInitializerAttribute)));

    // The registration: every module initializer of the package class, and the dependencies they carry, as
    // (type, member types). A generic type is named by its definition.
    private static (INamedTypeSymbol type, DynamicallyAccessedMemberTypes members)[] Registered(INamedTypeSymbol packageClass) =>
        RegistrationMethods(packageClass)
            .SelectMany(method => method.GetAttributes())
            .Where(attribute => attribute.AttributeClass?.Name == nameof(DynamicDependencyAttribute) && attribute.ConstructorArguments.Length == 2)
            .Select(attribute => ((INamedTypeSymbol)attribute.ConstructorArguments[1].Value!, (DynamicallyAccessedMemberTypes)(int)attribute.ConstructorArguments[0].Value!))
            .ToArray();

    // A type's metadata name within the package class: "Pair`1" for a generic one.
    private static string MetadataNameOf(INamedTypeSymbol type) => type.MetadataName;

    [TestMethod]
    public void EveryTypeTheGeneratorGeneratesForIsRegistered()
    {
        (INamedTypeSymbol packageClass, _) = Build(Package);
        (INamedTypeSymbol type, DynamicallyAccessedMemberTypes members)[] registered = Registered(packageClass);

        string[] unregistered = Expected.Select(expected => expected.type)
            .Where(name => !registered.Any(entry => MetadataNameOf(entry.type) == name && SymbolEqualityComparer.Default.Equals(entry.type.ContainingType, packageClass)))
            .ToArray();

        Assert.AreEqual(0, unregistered.Length, "generated for but NOT registered: " + string.Join(", ", unregistered));

        foreach ((string name, DynamicallyAccessedMemberTypes members) in Expected)
            Assert.AreEqual(members, registered.First(entry => MetadataNameOf(entry.type) == name).members, $"{name} is registered with its kind's members");

        INamedTypeSymbol pair = registered.First(entry => MetadataNameOf(entry.type) == "Pair`1").type;
        Assert.IsTrue(pair.IsUnboundGenericType, "a generic Go type is registered by its definition, Pair<>");
    }

    [TestMethod]
    public void NothingElseIsRegistered()
    {
        (INamedTypeSymbol packageClass, _) = Build(Package);
        string[] extra = Registered(packageClass).Select(entry => MetadataNameOf(entry.type))
            .Except(Expected.Select(expected => expected.type))
            .ToArray();

        Assert.AreEqual(0, extra.Length, "registered but not a Go type the generator generates for: " + string.Join(", ", extra));
    }

    [TestMethod]
    public void TheRegistrationBindsCleanly()
    {
        (INamedTypeSymbol packageClass, Compilation output) = Build(Package);
        IMethodSymbol[] methods = RegistrationMethods(packageClass).ToArray();

        Assert.AreEqual(1, methods.Length, "one registering module initializer per package class");

        SyntaxTree tree = methods[0].DeclaringSyntaxReferences.Single().SyntaxTree;
        Diagnostic[] errors = output.GetSemanticModel(tree).GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        Assert.AreEqual(0, errors.Length, "the registration file binds: " + string.Join("; ", errors.Take(3)));
        Assert.IsTrue(methods[0].ReturnsVoid && methods[0].Parameters.Length == 0, "a module initializer is a parameterless void method");
    }

    // The guard is the generator's own type list against the registry: a GoTypeAttribute marks every type the
    // generator generated for (re-emitted on a rule-selected type's part, written by hand on an attribute-selected
    // one), so a marked type with no registration is exactly the miss golib's suppressions must not survive.
    [TestMethod]
    public void EveryMarkedTypeIsRegistered()
    {
        (INamedTypeSymbol packageClass, _) = Build(Package);
        HashSet<INamedTypeSymbol> registered = Registered(packageClass).Select(entry => entry.type.OriginalDefinition).ToHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        string[] missing = packageClass.GetTypeMembers()
            .Where(type => type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "GoTypeAttribute"))
            .Where(type => !registered.Contains(type.OriginalDefinition))
            .Select(MetadataNameOf)
            .ToArray();

        Assert.AreEqual(0, missing.Length, "a [GoType] type with no registration: " + string.Join(", ", missing));
    }
}
