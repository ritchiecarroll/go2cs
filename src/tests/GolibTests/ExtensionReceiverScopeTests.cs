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

// The extension scan scoped by the receiver's HOME (docs/phase4/DESIGN-extension-receiver-scope.md): a question about
// a type reads only the host classes whose methods extend the assemblies a receiver matching it can belong to, and must
// answer exactly what the whole scan answered. The home is the RECEIVER's assembly, never the declaring one: a `-tests`
// host declares methods on its package's types (a whitebox test file's `Generate` on `clientHelloMsg`), and they stay in
// that package's answers.
//
// The fixture is that shape in miniature, compiled here: a package (`l3fx`), its `-tests` host (`l3fx.tests`) declaring
// methods on the package's types and on its own, an unrelated package (`l3fx.other`), and a receiver type the host was
// built against in one assembly (`l3fx.facade`) that has since moved to another (`l3fx.moved`), leaving a type forwarder.
//
// The scan is also scoped by the receiver's KEY (docs/phase4/DESIGN-startup-method-sets.md, arm A1): a question reads
// only the host classes whose receivers are its element, a type it derives from or an interface it implements, or carry
// no key. `l3fx.keys` holds one host class per receiver shape whose reading the key must agree with the method-set rule
// on: `Nullable<E>`, a generic base, a generic interface, and each public pointer-box subclass.
[TestClass]
public class ExtensionReceiverScopeTests
{
    private const string PackageSource =
        """
        namespace go.fx
        {
            public struct point { public int X; }
            public struct pair<K, V> { }
            public interface shape { }
            public struct circle : shape { }
            public class basecls<T> { }
            public class derived : basecls<int> { }
            public interface gshape<T> { }
            public struct gcircle : gshape<int> { }
        }
        """;

    private const string MovedSource = "namespace go.fx { public struct moved { } }";

    private const string ForwarderSource = "[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(go.fx.moved))]";

    // The listing is what go2cs-gen writes for this host (GenTests' ExtensionHostListTests holds the generator to it).
    private const string TestsSource =
        """
        [assembly: go.GoExtensionHostList(HomesListed = true)]
        [assembly: go.GoExtensionHost(typeof(go.fx.fx_internal_test_package), Homes = new[] { typeof(go.fx.moved), typeof(go.fx.point) })]
        [assembly: go.GoExtensionHost(typeof(go.fx.fx_test_package))]

        namespace go.fx
        {
            public static class fx_internal_test_package
            {
                public static int Generate(this point p) => 1;
                public static int Scale(this ref point p) => 2;
                public static int Scale(this ж<point> p) => 2;
                public static int First<K, V>(this pair<K, V> p) => 3;
                public static int Area(this shape s) => 4;
                public static int Moved(this moved m) => 5;
                public static int Own(this local l) => 6;
            }

            public static class fx_test_package
            {
                public static int Mine(this ж<local> l) => 7;
            }

            public struct local { }
        }
        """;

    private const string OtherSource =
        """
        namespace go.other
        {
            public struct thing { }

            public static class other_package
            {
                public static int Name(this thing t) => 8;
            }
        }
        """;

    // One class per receiver shape, so that no class is read on another receiver's account. No listing: the walk reads
    // these receivers' keys from the signatures.
    private const string KeysSource =
        """
        namespace go.fx
        {
            public static class keys_nullable_package { public static int Maybe(this point? p) => 9; }
            public static class keys_base_package { public static int Base(this basecls<int> b) => 10; }
            public static class keys_interface_package { public static int GArea(this gshape<int> s) => 11; }
            public static class keys_standard_package { public static int Standard(this StandardBox<point> p) => 12; }
            public static class keys_slotted_package { public static int Slotted(this SlottedStandardBox<point> p) => 13; }
            public static class keys_static_package { public static int Static(this StaticBox<point> p) => 14; }
            public static class keys_native_package { public static int Native(this NativeBox<point> p) => 15; }
            public static class keys_element_package { public static int Element(this ElemRefBox<point> p) => 16; }
        }
        """;

    private sealed class FixtureContext() : AssemblyLoadContext("l3-receiver-scope", isCollectible: false)
    {
        public readonly Dictionary<string, byte[]> Images = [];

        // golib and the framework come from the default context, so the fixture's ж<T> is golib's own.
        protected override Assembly? Load(AssemblyName name) =>
            name.Name is { } simple && Images.TryGetValue(simple, out byte[]? image) ? LoadFromStream(new MemoryStream(image)) : null;
    }

    private sealed record Fixture(Assembly Package, Assembly Tests, Assembly Other, Assembly Moved, Assembly Keys)
    {
        public Assembly[] All => [Package, Tests, Other, Moved, Keys];

        public Type Type(string name) => All.Select(assembly => assembly.GetType(name)).Single(type => type is not null)!;
    }

    private static readonly Lazy<Fixture> s_fixture = new(Build);

    private static Fixture Build()
    {
        MetadataReference[] framework =
        [
            .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path)),
            MetadataReference.CreateFromFile(typeof(GoExtensionHostAttribute).Assembly.Location)
        ];

        byte[] package = Compile("l3fx", PackageSource, framework);
        byte[] moved = Compile("l3fx.moved", MovedSource, framework);
        byte[] facadeThen = Compile("l3fx.facade", MovedSource, framework);
        byte[] facadeNow = Compile("l3fx.facade", ForwarderSource, [.. framework, MetadataReference.CreateFromImage(moved)]);
        byte[] tests = Compile("l3fx.tests", TestsSource, [.. framework, MetadataReference.CreateFromImage(package), MetadataReference.CreateFromImage(facadeThen)]);
        byte[] other = Compile("l3fx.other", OtherSource, framework);
        byte[] keys = Compile("l3fx.keys", KeysSource, [.. framework, MetadataReference.CreateFromImage(package)]);

        FixtureContext context = new();
        context.Images["l3fx"] = package;
        context.Images["l3fx.moved"] = moved;
        context.Images["l3fx.facade"] = facadeNow;
        context.Images["l3fx.tests"] = tests;
        context.Images["l3fx.other"] = other;
        context.Images["l3fx.keys"] = keys;

        Assembly load(string name) => context.LoadFromAssemblyName(new AssemblyName(name));

        return new Fixture(load("l3fx"), load("l3fx.tests"), load("l3fx.other"), load("l3fx.moved"), load("l3fx.keys"));
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

    private static string Names(IEnumerable<Assembly> assemblies) =>
        string.Join(", ", assemblies.Select(assembly => assembly.GetName().Name).Order(StringComparer.Ordinal));

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

    // ---------------------------------------------------------------------------------------------
    // Homes
    // ---------------------------------------------------------------------------------------------

    // The metadata walk homes a bridge method under the package whose type it extends, and the host's own methods under
    // the host: each read from the method's signature, with no host class loaded.
    [TestMethod]
    public void TheWalkHomesABridgeUnderItsReceiversAssembly()
    {
        Fixture fixture = s_fixture.Value;
        TypeExtensions.ExtensionHost[] hosts = [.. TypeExtensions.WalkedExtensionHosts(fixture.Tests)!];

        TypeExtensions.ExtensionHost bridge = hosts.Single(host => host.Type.Name == "fx_internal_test_package");
        TypeExtensions.ExtensionHost own = hosts.Single(host => host.Type.Name == "fx_test_package");

        Assert.AreEqual("l3fx, l3fx.moved, l3fx.tests", Names(bridge.Homes), "the bridge host's homes");
        Assert.AreEqual("l3fx.tests", Names(own.Homes), "the host's own methods' home");
        Assert.IsFalse(bridge.AnyHome || own.AnyHome, "every receiver has a home");
    }

    // COORD's question: a receiver the host reaches through a TypeRef whose scope has since forwarded the type is homed
    // where the type loads, not in the scope the TypeRef names. The walk resolves such a receiver through the runtime's
    // binder, which follows the forwarder (ExportedType).
    [TestMethod]
    public void AForwardedReceiverIsHomedWhereItsTypeLoads()
    {
        Fixture fixture = s_fixture.Value;
        Type moved = fixture.Type("go.fx.moved");

        Assert.AreEqual(fixture.Moved, moved.Assembly, "control: the forwarder moved the type");

        TypeExtensions.ExtensionHost bridge = TypeExtensions.WalkedExtensionHosts(fixture.Tests)!.Single(host => host.Type.Name == "fx_internal_test_package");

        CollectionAssert.Contains(bridge.Homes, fixture.Moved);
        Assert.IsFalse(bridge.Homes.Any(home => home.GetName().Name == "l3fx.facade"), "homed in the forwarding scope: " + Names(bridge.Homes));

        MethodInfo[] methods = WithScanOf(fixture.All, () => TypeExtensions.ExtensionMethodsOf(moved, null));
        CollectionAssert.AreEqual(new[] { "Moved" }, methods.Select(method => method.Name).ToArray());
    }

    // The build-time list (Native AOT) names the same homes the walk reads from the signatures.
    [TestMethod]
    public void TheListNamesTheHomesTheWalkReads()
    {
        Fixture fixture = s_fixture.Value;
        IReadOnlyList<TypeExtensions.ExtensionHost> walked = TypeExtensions.WalkedExtensionHosts(fixture.Tests)!;
        IReadOnlyList<TypeExtensions.ExtensionHost> listed = TypeExtensions.ListedExtensionHostEntries(fixture.Tests)!;

        foreach (TypeExtensions.ExtensionHost host in walked)
        {
            TypeExtensions.ExtensionHost entry = listed.Single(candidate => candidate.Type == host.Type);

            Assert.IsFalse(entry.AnyHome, $"{host.Type.Name}: listed with its homes");
            Assert.AreEqual(Names(host.Homes.Append(fixture.Tests).Distinct()), Names(entry.Homes), $"{host.Type.Name}: the listed homes");
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Questions
    // ---------------------------------------------------------------------------------------------

    // A question about the package's type reads the package's and the host's classes that extend it, and never the
    // unrelated package's.
    [TestMethod]
    public void AQuestionReadsOnlyTheHostsOfItsHomes()
    {
        Fixture fixture = s_fixture.Value;
        List<string> read = [];

        List<MethodInfo> candidates = WithScanOf(fixture.All, () =>
        {
            TypeExtensions.HostReadProbeForTest = host => read.Add(host.FullName!);
            return TypeExtensions.CollectGoMethodSetCandidates(fixture.Type("go.fx.point"), true, null);
        });

        CollectionAssert.AreEquivalent(new[] { "Generate", "Scale" }, candidates.Where(method => method.DeclaringType!.Assembly == fixture.Tests).Select(method => method.Name).Distinct().ToArray(), "the *point method set's bridge methods");
        CollectionAssert.Contains(read, "go.fx.fx_internal_test_package", "control: the bridge host was read");
        CollectionAssert.DoesNotContain(read, "go.other.other_package", "a question about go.fx.point read an unrelated package's methods");
    }

    // Both consumers answer every fixture type exactly as the whole scan does: the bridge's methods, the forwarded
    // receiver's, the interface receiver reached by assignability, the generic receiver reached by its definition.
    [TestMethod]
    public void EveryFixtureAnswerIsTheFullScans()
    {
        Fixture fixture = s_fixture.Value;
        Type point = fixture.Type("go.fx.point");
        Type[] questions =
        [
            .. fixture.All.SelectMany(assembly => assembly.GetTypes()),
            typeof(Nullable<>).MakeGenericType(point),
            .. ((Type[])[typeof(ж<>), typeof(StandardBox<>), typeof(StaticBox<>), typeof(NativeBox<>), typeof(ElemRefBox<>)]).Select(box => box.MakeGenericType(point))
        ];

        string[] mismatches = WithScanOf(fixture.All, () => Mismatches(questions));
        Assert.AreEqual(0, mismatches.Length, string.Join("\n", mismatches));

        Assert.AreEqual("Area", string.Join(",", WithScanOf(fixture.All, () => TypeExtensions.ExtensionMethodsOf(fixture.Type("go.fx.circle"), null)).Select(method => method.Name)),
            "control: an interface receiver is reached by assignability");

        // A closed pair from the fixture's own generic definition.
        Type closedPair = fixture.Type("go.fx.pair`2").MakeGenericType(typeof(int), typeof(string));
        Assert.AreEqual("First", string.Join(",", WithScanOf(fixture.All, () => TypeExtensions.CollectGoMethodSetCandidates(closedPair, false, null)).Select(method => method.Name)),
            "control: a generic receiver is reached by its definition");
    }

    // ---------------------------------------------------------------------------------------------
    // Receiver keys
    // ---------------------------------------------------------------------------------------------

    // The walk keys each receiver by the type the method-set rule reads it as: a generic base or interface by its
    // definition, a box subclass by what it holds. `Nullable<E>` has no key, so its class is read for every question.
    [TestMethod]
    public void TheWalkKeysEachReceiverAsTheMethodSetRuleReadsIt()
    {
        Fixture fixture = s_fixture.Value;
        Dictionary<string, TypeExtensions.ExtensionHost> hosts = TypeExtensions.WalkedExtensionHosts(fixture.Keys)!.ToDictionary(host => host.Type.Name);

        TypeExtensions.ReceiverKey key(string name)
        {
            Type type = fixture.Type(name);
            return new TypeExtensions.ReceiverKey(type.Module, type.MetadataToken);
        }

        Assert.IsNull(hosts["keys_nullable_package"].Receivers, "Nullable<point> has no key");
        CollectionAssert.AreEqual(new[] { key("go.fx.basecls`1") }, hosts["keys_base_package"].Receivers, "basecls<int>");
        CollectionAssert.AreEqual(new[] { key("go.fx.gshape`1") }, hosts["keys_interface_package"].Receivers, "gshape<int>");

        foreach (string box in (string[])["keys_standard_package", "keys_slotted_package", "keys_static_package", "keys_native_package", "keys_element_package"])
        {
            CollectionAssert.AreEqual(new[] { key("go.fx.point") }, hosts[box].Receivers, box);
            CollectionAssert.AreEqual(new[] { fixture.Package }, hosts[box].Homes, $"{box}: homed with point");
        }
    }

    // Each receiver shape is reached by the question the method-set rule matches it to, and a class whose receivers the
    // question cannot match is not read.
    [TestMethod]
    public void AQuestionReadsOnlyTheHostsOfItsKeys()
    {
        Fixture fixture = s_fixture.Value;

        Assert.AreEqual("Maybe", answer(fixture.Type("go.fx.point"), false), "Nullable<point> receiver, point question (assignability)");
        Assert.AreEqual("Maybe", answer(typeof(Nullable<>).MakeGenericType(fixture.Type("go.fx.point")), false), "Nullable<point> question");
        Assert.AreEqual("Base", answer(fixture.Type("go.fx.derived"), false), "generic base");
        Assert.AreEqual("GArea", answer(fixture.Type("go.fx.gcircle"), false), "generic interface");
        Assert.AreEqual("Element, Maybe, Native, Scale, Slotted, Standard, Static", answer(fixture.Type("go.fx.point"), true), "box subclasses, *point question");

        List<string> read = [];

        WithScanOf(fixture.All, () =>
        {
            TypeExtensions.HostReadProbeForTest = host => read.Add(host.Name);
            return TypeExtensions.CollectGoMethodSetCandidates(fixture.Type("go.fx.point"), true, null);
        });

        CollectionAssert.Contains(read, "keys_native_package", "control: a box subclass's class was read");
        CollectionAssert.DoesNotContain(read, "keys_base_package", "a question about point read a class homed with it whose receivers are not point's");

        string answer(Type question, bool isPointer) =>
            string.Join(", ", WithScanOf(fixture.All, () => TypeExtensions.CollectGoMethodSetCandidates(question, isPointer, null))
                .Where(method => method.DeclaringType!.Assembly == fixture.Keys || method.Name == "Scale").Select(method => method.Name).Distinct().Order(StringComparer.Ordinal));
    }

    // error<T>'s own lookup (TypeAssertionError's Error, from error<T>'s static constructor) reads only the classes that
    // extend its pointee's keys.
    [TestMethod]
    public void TheBoxLookupReadsOnlyTheHostsOfItsPointeesKeys()
    {
        Fixture fixture = s_fixture.Value;
        Type point = fixture.Type("go.fx.point");
        Type box = typeof(ж<>).MakeGenericType(point);
        List<string> read = [];

        MethodInfo? found = WithScanOf(fixture.All, () =>
        {
            TypeExtensions.HostReadProbeForTest = host => read.Add(host.Name);
            return TypeExtensions.GetBoxExtensionMethod(point, box, "Scale");
        });

        Assert.AreEqual("fx_internal_test_package.Scale(ж`1)", found is null ? "none" : $"{found.DeclaringType!.Name}.{found.Name}({found.GetParameters()[0].ParameterType.Name})");
        CollectionAssert.DoesNotContain(read, "keys_base_package", "the box lookup read a class whose receivers are not its pointee's");
    }

    // Every converted src/core assembly this project reaches, with the fixture: for every type each declares, both
    // consumers' home-scoped answers equal the whole scan's, method for method and in order.
    [TestMethod]
    public void EveryReachableAnswerIsTheFullScans()
    {
        Assembly[] population = [.. ExtensionHostListTests.ReachableConvertedAssemblies(), .. s_fixture.Value.All];
        Type[] questions = [.. population.SelectMany(LoadableTypes)];

        string[] mismatches = WithScanOf(population, () => Mismatches(questions));

        Assert.AreEqual(0, mismatches.Length, $"{mismatches.Length} answers differ, first 20:\n{string.Join("\n", mismatches.Take(20))}");
        Assert.IsTrue(questions.Length > 10000, $"control: asked about {questions.Length} types");
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

    // The answers that differ, for each question type: its value and pointer method sets, and its extension methods.
    private static string[] Mismatches(Type[] questions)
    {
        (MethodInfo, Type)[] whole = TypeExtensions.GetExtensionMethods();
        List<string> mismatches = [];

        foreach (Type type in questions)
        {
            foreach (bool isPointer in (bool[])[false, true])
                compare($"{type.FullName} ({(isPointer ? "*X" : "X")} method set)", () => TypeExtensions.CollectGoMethodSetCandidates(type, isPointer, whole), () => TypeExtensions.CollectGoMethodSetCandidates(type, isPointer, null));

            compare($"{type.FullName} (extension methods)", () => TypeExtensions.ExtensionMethodsOf(type, whole), () => TypeExtensions.ExtensionMethodsOf(type, null));
        }

        return [.. mismatches];

        void compare(string question, Func<IReadOnlyList<MethodInfo>> full, Func<IReadOnlyList<MethodInfo>> scoped)
        {
            string expected = answer(full), actual = answer(scoped);

            if (expected != actual)
                mismatches.Add($"{question}: the whole scan answers [{expected}], the home-scoped scan [{actual}]");
        }

        static string answer(Func<IReadOnlyList<MethodInfo>> question)
        {
            try
            {
                return string.Join(", ", question().Select(method => $"{method.DeclaringType?.Name}.{method.Name}({string.Join(",", method.GetParameters().Select(p => p.ParameterType.Name))})"));
            }
            catch (Exception error)
            {
                return $"throws {error.GetType().Name}";
            }
        }
    }
}
