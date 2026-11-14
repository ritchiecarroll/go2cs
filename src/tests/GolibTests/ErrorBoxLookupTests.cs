using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using go;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeExtensions = go.golib.TypeExtensions;

namespace GolibTests;

// error<T> finds the Error method of a pointer-held T among T's own packages' host classes
// (docs/phase4/DESIGN-startup-method-sets.md, arm A2). It once asked `typeof(ж<T>).GetExtensionMethod("Error")`, which
// reads every extension method of every loaded package, because a box matches every box receiver by its generic
// definition. These tests hold the new lookup to what the old one BOUND, and name the one place they differ: a type
// with no method on exactly its own box, where the old lookup's answer was the first unrelated box's Error in scan
// order, so a generic type's pointer Error was found or missed by load order.
//
// The fixture: a package (`a2fx`) with a type that declares a pointer Error, one that declares none and a generic one,
// and an unrelated package (`a2fx.other`) that declares a pointer Error of its own.
[TestClass]
public class ErrorBoxLookupTests
{
    private const string PackageSource =
        """
        namespace go.efx
        {
            public struct oops { }
            public struct quiet { }
            public struct wrap<K> { }

            public static class efx_package
            {
                public static go.@string Error<K>(this go.ж<wrap<K>> w) => "wrap";
                public static go.@string Error(this ref oops o) => "oops";
                public static go.@string Error(this go.ж<oops> o) => "oops";
                public static go.@string Name(this go.ж<quiet> q) => "quiet";
            }
        }
        """;

    private const string OtherSource =
        """
        namespace go.eother
        {
            public struct loud { }

            public static class eother_package
            {
                public static go.@string Error(this go.ж<loud> l) => "loud";
            }
        }
        """;

    private sealed class FixtureContext() : AssemblyLoadContext("a2-error-lookup", isCollectible: false)
    {
        public readonly Dictionary<string, byte[]> Images = [];

        // golib and the framework come from the default context, so the fixture's ж<T> is golib's own.
        protected override Assembly? Load(AssemblyName name) =>
            name.Name is { } simple && Images.TryGetValue(simple, out byte[]? image) ? LoadFromStream(new MemoryStream(image)) : null;
    }

    private sealed record Fixture(Assembly Package, Assembly Other)
    {
        public Type Type(string name) => ((Assembly[])[Package, Other]).Select(assembly => assembly.GetType(name)).Single(type => type is not null)!;
    }

    private static readonly Lazy<Fixture> s_fixture = new(Build);

    private static Fixture Build()
    {
        MetadataReference[] references =
        [
            .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path)),
            MetadataReference.CreateFromFile(typeof(GoExtensionHostAttribute).Assembly.Location)
        ];

        FixtureContext context = new();
        context.Images["a2fx"] = Compile("a2fx", PackageSource, references);
        context.Images["a2fx.other"] = Compile("a2fx.other", OtherSource, references);

        return new Fixture(context.LoadFromAssemblyName(new AssemblyName("a2fx")), context.LoadFromAssemblyName(new AssemblyName("a2fx.other")));
    }

    private static byte[] Compile(string name, string source, IEnumerable<MetadataReference> references)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Disable));

        using MemoryStream image = new();
        EmitResult emitted = compilation.Emit(image);

        Assert.IsTrue(emitted.Success, $"fixture {name}: " + string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Take(3)));

        return image.ToArray();
    }

    private static T WithScanOf<T>(Assembly[] assemblies, Func<T> question)
    {
        TypeExtensions.AssemblySourceForTest = () => assemblies;
        TypeExtensions.ResetExtensionScanForTest();

        try
        {
            return question();
        }
        finally
        {
            TypeExtensions.AssemblySourceForTest = null;
            TypeExtensions.HostReadProbeForTest = null;
            TypeExtensions.ResetExtensionScanForTest();
        }
    }

    private static Type Box(Type pointee) => typeof(ж<>).MakeGenericType(pointee);

    // The lookup error<T> made before: the whole scan's best "Error" for the box, and, when that is an open generic
    // method, the type's own from the method-set candidate source.
    private static MethodInfo? WholeScanLookup(Type target)
    {
        MethodInfo? method = TypeExtensions.GetExtensionMethod(Box(target), "Error");

        if (method is not { ContainsGenericParameters: true })
            return method;

        AdapterBinder.ResolveReceiverMethods(target, "Error", out MethodInfo? byPtr, out _);

        return byPtr;
    }

    // What error<T> keeps of a lookup: the method, when error<T>'s own pointer delegate binds it, and nothing otherwise.
    private static MethodInfo? Bound(MethodInfo? method, Type target)
    {
        if (method is null)
            return null;

        Type delegateType = typeof(error<>).GetNestedType("ErrorByPtr", BindingFlags.NonPublic)!.MakeGenericType(target);

        return TypeExtensions.CreateStaticDelegate(method, delegateType) is null ? null : method;
    }

    private static string Describe(MethodInfo? method) =>
        method is null ? "nothing" : $"{method.DeclaringType?.FullName}.{method.Name}({method.GetParameters()[0].ParameterType})";

    // A type's own pointer Error is found, and no other package's host class is read to find it.
    [TestMethod]
    public void APointersErrorIsReadFromItsOwnPackage()
    {
        Fixture fixture = s_fixture.Value;
        Type oops = fixture.Type("go.efx.oops");
        List<string> read = [];

        MethodInfo? found = WithScanOf([fixture.Other, fixture.Package], () =>
        {
            TypeExtensions.HostReadProbeForTest = host => read.Add(host.FullName!);
            return ErrorBinding.PointerMethod(oops, Box(oops));
        });

        Assert.AreEqual("go.efx.efx_package.Error(go.ж`1[go.efx.oops])", Describe(found));
        CollectionAssert.Contains(read, "go.efx.efx_package", "control: the type's own package class was read");
        CollectionAssert.DoesNotContain(read, "go.eother.eother_package", "a question about go.efx.oops read an unrelated package's methods");
    }

    // A type with no pointer Error binds none. The whole-scan lookup answered an unrelated box's Error here, which the
    // delegate then refused; the two agree on what is bound.
    [TestMethod]
    public void ATypeWithoutAPointerErrorBindsNone()
    {
        Fixture fixture = s_fixture.Value;
        Type quiet = fixture.Type("go.efx.quiet");

        (MethodInfo? before, MethodInfo? after) = WithScanOf([fixture.Other, fixture.Package], () =>
            (WholeScanLookup(quiet), ErrorBinding.PointerMethod(quiet, Box(quiet))));

        Assert.AreEqual("go.eother.eother_package.Error(go.ж`1[go.eother.loud])", Describe(before), "control: the whole scan answered another type's method");
        Assert.IsNull(Bound(before, quiet), "control: which error<T> did not bind");
        Assert.IsNull(after);
    }

    // THE DISCLOSED DIFFERENCE. A generic type's pointer Error is declared open, so no receiver equals the asked box and
    // the whole-scan lookup took the first box Error in scan order: the type's own only when that was the first box
    // Error of the whole scan (the fixture declares it first in its class for that reason), and another type's, which
    // does not bind, otherwise. The lookup now answers the type's own either way.
    [TestMethod]
    public void AGenericTypesPointerErrorDoesNotDependOnScanOrder()
    {
        Fixture fixture = s_fixture.Value;
        Type wrap = fixture.Type("go.efx.wrap`1").MakeGenericType(typeof(int));
        const string Own = "go.efx.efx_package.Error(go.ж`1[go.efx.wrap`1[System.Int32]])";

        foreach (Assembly[] order in (Assembly[][])[[fixture.Package, fixture.Other], [fixture.Other, fixture.Package]])
        {
            MethodInfo? after = WithScanOf(order, () => Bound(ErrorBinding.PointerMethod(wrap, Box(wrap)), wrap));
            Assert.AreEqual(Own, Describe(after), $"scan order {order[0].GetName().Name} first");
        }

        MethodInfo? ownFirst = WithScanOf([fixture.Package, fixture.Other], () => Bound(WholeScanLookup(wrap), wrap));
        MethodInfo? otherFirst = WithScanOf([fixture.Other, fixture.Package], () => Bound(WholeScanLookup(wrap), wrap));

        Assert.AreEqual(Own, Describe(ownFirst), "control: the whole-scan lookup bound it when it was the first box Error scanned");
        Assert.IsNull(otherFirst, "control: and bound nothing when another type's box Error came first");
    }

    // Every type of every converted src/core assembly this project reaches, with the fixture: what error<T> binds for
    // a pointer is what the whole-scan lookup bound. Generic definitions are not asked (they have no closed box here;
    // the test above is their case).
    [TestMethod]
    public void EveryReachableBindingIsTheWholeScans()
    {
        Fixture fixture = s_fixture.Value;
        Assembly[] population = [.. ExtensionHostListTests.ReachableConvertedAssemblies(), fixture.Package, fixture.Other];
        int asked = 0, bound = 0;

        string[] mismatches = WithScanOf(population, () =>
        {
            List<string> found = [];

            foreach (Type type in population.SelectMany(LoadableTypes))
            {
                if (type.IsGenericTypeDefinition || type.ContainsGenericParameters || type.IsByRefLike || type.IsPointer || type.IsAbstract && type.IsSealed)
                    continue;

                Type box;

                try
                {
                    box = Box(type);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                asked++;

                MethodInfo? before = Bound(WholeScanLookup(type), type), after = Bound(ErrorBinding.PointerMethod(type, box), type);

                if (after is not null)
                    bound++;

                if (before != after)
                    found.Add($"{type.FullName}: the whole-scan lookup bound {Describe(before)}, the lookup now binds {Describe(after)}");
            }

            return found.ToArray();
        });

        Assert.AreEqual(0, mismatches.Length, $"{mismatches.Length} bindings differ, first 20:\n{string.Join("\n", mismatches.Take(20))}");
        Assert.IsTrue(asked > 5000, $"control: asked about {asked} types");
        Console.WriteLine($"asked {asked}, bound {bound}");
        Assert.IsTrue(bound > 30, $"control: {bound} of {asked} types bind a pointer Error");
    }

    private static IEnumerable<Type> LoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException error)
        {
            return error.Types.OfType<Type>();
        }
    }
}
