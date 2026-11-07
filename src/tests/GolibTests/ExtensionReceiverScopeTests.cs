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

    private sealed class FixtureContext() : AssemblyLoadContext("l3-receiver-scope", isCollectible: false)
    {
        public readonly Dictionary<string, byte[]> Images = [];

        // golib and the framework come from the default context, so the fixture's ж<T> is golib's own.
        protected override Assembly? Load(AssemblyName name) =>
            name.Name is { } simple && Images.TryGetValue(simple, out byte[]? image) ? LoadFromStream(new MemoryStream(image)) : null;
    }

    private sealed record Fixture(Assembly Package, Assembly Tests, Assembly Other, Assembly Moved)
    {
        public Assembly[] All => [Package, Tests, Other, Moved];

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

        FixtureContext context = new();
        context.Images["l3fx"] = package;
        context.Images["l3fx.moved"] = moved;
        context.Images["l3fx.facade"] = facadeNow;
        context.Images["l3fx.tests"] = tests;
        context.Images["l3fx.other"] = other;

        Assembly load(string name) => context.LoadFromAssemblyName(new AssemblyName(name));

        return new Fixture(load("l3fx"), load("l3fx.tests"), load("l3fx.other"), load("l3fx.moved"));
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

        CollectionAssert.AreEquivalent(new[] { "Generate", "Scale" }, candidates.Select(method => method.Name).Distinct().ToArray(), "the *point method set");
        CollectionAssert.Contains(read, "go.fx.fx_internal_test_package", "control: the bridge host was read");
        CollectionAssert.DoesNotContain(read, "go.other.other_package", "a question about go.fx.point read an unrelated package's methods");
    }

    // Both consumers answer every fixture type exactly as the whole scan does: the bridge's methods, the forwarded
    // receiver's, the interface receiver reached by assignability, the generic receiver reached by its definition.
    [TestMethod]
    public void EveryFixtureAnswerIsTheFullScans()
    {
        Fixture fixture = s_fixture.Value;
        Type[] questions = [.. fixture.All.SelectMany(assembly => assembly.GetTypes())];

        string[] mismatches = WithScanOf(fixture.All, () => Mismatches(questions));
        Assert.AreEqual(0, mismatches.Length, string.Join("\n", mismatches));

        Assert.AreEqual("Area", string.Join(",", WithScanOf(fixture.All, () => TypeExtensions.ExtensionMethodsOf(fixture.Type("go.fx.circle"), null)).Select(method => method.Name)),
            "control: an interface receiver is reached by assignability");

        // A closed pair from the fixture's own generic definition.
        Type closedPair = fixture.Type("go.fx.pair`2").MakeGenericType(typeof(int), typeof(string));
        Assert.AreEqual("First", string.Join(",", WithScanOf(fixture.All, () => TypeExtensions.CollectGoMethodSetCandidates(closedPair, false, null)).Select(method => method.Name)),
            "control: a generic receiver is reached by its definition");
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
