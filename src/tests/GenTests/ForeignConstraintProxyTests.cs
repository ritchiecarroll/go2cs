// ForeignConstraintProxyTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A self-referential constraint instantiated over a pointer to an element NEITHER the instantiating package
/// NOR the interface's package declares: Go's <c>lib.Ord2[*elem.Node]</c> under <c>Ord2[T Lesser[T]]</c>, from
/// two packages b1 and main.
/// </summary>
/// <remarks>
/// <para>
/// The proxy is the type argument, so it is part of the instantiated type's identity: b1 and main must name ONE
/// proxy, and neither may declare it. The interface's package publishes the GENERIC proxy
/// <c>жLesser&lt;E&gt;</c> (ConstraintProxyGenerator), and every instantiating package binds its slots to the
/// element's Go methods from a module initializer (the ConstraintProxy record over a foreign interface).
/// </para>
/// <para>
/// These tests build the four assemblies separately, as the converter's projects are, run the REAL generators on
/// each, and EXECUTE the result: b1's value reaches main, the methods dispatch through the bound slots, both
/// binders run, and an element nobody bound panics naming the pair.
/// </para>
/// </remarks>
[TestClass]
public class ForeignConstraintProxyTests
{
    internal const string ShimSource =
        """
        namespace go
        {
            public class ж<T> { public ж(T value) { Value = value; } public T Value; }

            public struct slice<T> { }

            public struct map<K, V> { }

            public class channel<T> { }

            public readonly struct array<T> : System.Collections.Generic.IEnumerable<T>
            {
                private readonly T[] items;

                public array(T[] items) { this.items = items; }

                public int Length => items.Length;

                public T this[int index] => items[index];

                public System.Collections.Generic.IEnumerator<T> GetEnumerator() => ((System.Collections.Generic.IEnumerable<T>)items).GetEnumerator();

                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public interface IжAdapter { object? Box { get; } }

            public class PanicException(object? state) : System.Exception(state?.ToString()) { }

            public static partial class builtin { public static PanicException panic(object? state) => new(state); }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplementAttribute<TStruct, TInterface> : System.Attribute
            {
                public bool Promoted { get; set; }
                public bool Pointer { get; set; }
                public bool ConstraintProxy { get; set; }
            }
        }
        """;

    private const string LibSource =
        """
        using go;

        namespace go.example;

        public static partial class lib_package
        {
            public partial interface Lesser<T>
            {
                bool Less(T _);
            }

            public partial interface Picker<T>
            {
                (T, bool) Pick(T a);

                void Each(System.Action<T> f);
            }

            // THE CONTROL: a generic interface that constrains a DIFFERENT parameter publishes no proxy.
            public partial interface Getter<T>
            {
                T Get();
            }

            public partial struct Ord2<T>
                where T : Lesser<T>
            {
                public T V;
            }

            public static bool Below<T>(this Ord2<T> o, T t)
                where T : Lesser<T>
            {
                return o.V.Less(t);
            }

            public static bool Picks<T>(T a)
                where T : Picker<T>
            {
                (T p, bool ok) = a.Pick(a);
                int n = 0;
                a.Each(_ => n++);
                return ok && n == 1 && p.Equals(a);
            }

            public static T Got<T, U>(U u)
                where U : Getter<T>
            {
                return u.Get();
            }
        }
        """;

    private const string ElemSource =
        """
        using go;

        namespace go.example;

        public static partial class elem_package
        {
            public partial struct Node { public int K; }

            public static bool Less(this ж<Node> n, ж<Node> o) => n.Value.K < o.Value.K;

            public static (ж<Node>, bool) Pick(this ж<Node> n, ж<Node> a) => (a, true);

            public static void Each(this ж<Node> n, System.Action<ж<Node>> f) => f(n);

            // Bound by no package.
            public partial struct Other { public int K; }

            public static bool Less(this ж<Other> n, ж<Other> o) => n.Value.K < o.Value.K;

            // A GENERIC element: each instantiation is its own closed proxy, with its own slots.
            public partial struct Box<E> { public int K; }

            public static bool Less<E>(this ж<Box<E>> n, ж<Box<E>> o) => n.Value.K < o.Value.K;
        }
        """;

    private const string B1Source =
        """
        using go;

        [assembly: GoImplement<global::go.example.elem_package.Node, global::go.example.lib_package.Lesser<global::go.example.elem_package.Node>>(ConstraintProxy = true)]
        [assembly: GoImplement<global::go.example.elem_package.Node, global::go.example.lib_package.Picker<global::go.example.elem_package.Node>>(ConstraintProxy = true)]
        [assembly: GoImplement<global::go.example.elem_package.Box<int>, global::go.example.lib_package.Lesser<global::go.example.elem_package.Box<int>>>(ConstraintProxy = true)]
        [assembly: GoImplement<global::go.example.elem_package.Box<string>, global::go.example.lib_package.Lesser<global::go.example.elem_package.Box<string>>>(ConstraintProxy = true)]

        namespace go.example;

        public static partial class b1_package
        {
            public static bool Boxes() =>
                new lib_package.Ord2<lib_package.жLesser<elem_package.Box<int>>> { V = new ж<elem_package.Box<int>>(new elem_package.Box<int> { K = 1 }) }.Below(new ж<elem_package.Box<int>>(new elem_package.Box<int> { K = 2 })) &&
                new lib_package.Ord2<lib_package.жLesser<elem_package.Box<string>>> { V = new ж<elem_package.Box<string>>(new elem_package.Box<string> { K = 1 }) }.Below(new ж<elem_package.Box<string>>(new elem_package.Box<string> { K = 2 }));

            public static lib_package.Ord2<lib_package.жLesser<elem_package.Node>> Make(int k) =>
                new() { V = new ж<elem_package.Node>(new elem_package.Node { K = k }) };

            public static bool Picks(int k) => lib_package.Picks<lib_package.жPicker<elem_package.Node>>(new ж<elem_package.Node>(new elem_package.Node { K = k }));
        }
        """;

    private const string MainSource =
        """
        using go;

        [assembly: GoImplement<global::go.example.elem_package.Node, global::go.example.lib_package.Lesser<global::go.example.elem_package.Node>>(ConstraintProxy = true)]

        namespace go.example;

        public static partial class main_package
        {
            // b1's value, received, assigned, compared and called; then main's own instantiation.
            public static string Run()
            {
                lib_package.Ord2<lib_package.жLesser<elem_package.Node>> a = b1_package.Make(1);
                ж<elem_package.Node> two = new(new elem_package.Node { K = 2 });
                var own = new lib_package.Ord2<lib_package.жLesser<elem_package.Node>> { V = new ж<elem_package.Node>(new elem_package.Node { K = 3 }) };
                object boxed = a;

                return $"{a.Below(two)} {own.Below(two)} {boxed is lib_package.Ord2<lib_package.жLesser<elem_package.Node>>} {b1_package.Picks(4)} {b1_package.Boxes()}";
            }

            public static string Unbound()
            {
                var o = new lib_package.Ord2<lib_package.жLesser<elem_package.Other>> { V = new ж<elem_package.Other>(new elem_package.Other { K = 1 }) };

                try
                {
                    return o.Below(new ж<elem_package.Other>(new elem_package.Other { K = 2 })).ToString();
                }
                catch (PanicException panic)
                {
                    return panic.Message;
                }
            }
        }
        """;

    private static readonly CSharpParseOptions s_parseOptions = new(LanguageVersion.Latest);

    internal static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Linq.dll"));
    }

    internal sealed record Built(Dictionary<string, byte[]> Images, Dictionary<string, List<GeneratedSourceResult>> Generated);

    // Compiles one assembly over the images before it, through both generators, and keeps its image and generated sources.
    internal static void Build(Built built, string name, string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            name,
            [CSharpSyntaxTree.ParseText(source, s_parseOptions)],
            CoreReferences().Concat(built.Images.Values.Select(image => MetadataReference.CreateFromImage(image))),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ConstraintProxyGenerator(), new ImplementGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation updated, out ImmutableArray<Diagnostic> _);

        built.Generated[name] = driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources).ToList();

        using MemoryStream image = new();
        EmitResult emitted = updated.Emit(image);

        Assert.IsTrue(emitted.Success, $"{name} must compile clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");
        built.Images[name] = image.ToArray();
    }

    private static Built BuildAll()
    {
        Built built = new([], []);

        Build(built, "shims", ShimSource);
        Build(built, "lib", LibSource);
        Build(built, "elem", ElemSource);
        Build(built, "b1", B1Source);
        Build(built, "main", MainSource);

        return built;
    }

    private sealed class ImageContext(Dictionary<string, byte[]> images) : AssemblyLoadContext(isCollectible: true)
    {
        protected override Assembly? Load(AssemblyName name) =>
            name.Name is { } simple && images.TryGetValue(simple, out byte[]? image) ? LoadFromStream(new MemoryStream(image)) : null;
    }

    internal static string Invoke(Built built, string method)
    {
        ImageContext context = new(built.Images);

        try
        {
            Assembly main = context.LoadFromAssemblyName(new AssemblyName("main"));
            return (string)main.GetType("go.example.main_package", throwOnError: true)!.GetMethod(method)!.Invoke(null, null)!;
        }
        finally
        {
            context.Unload();
        }
    }

    internal static string Source(Built built, string assembly, string hintFragment) =>
        built.Generated[assembly].FirstOrDefault(source => source.HintName.Contains(hintFragment)).SourceText?.ToString() ?? "";

    /// <summary>THE SUBJECT: b1's value and main's own dispatch through the bound slots, and are one type.</summary>
    [TestMethod]
    public void AForeignElementDispatchesThroughTheInterfacePackagesGenericProxy()
    {
        Assert.AreEqual("True False True True True", Invoke(BuildAll(), "Run"));
    }

    /// <summary>The interface's package publishes the proxy; no instantiating package declares one of its own.</summary>
    [TestMethod]
    public void OnlyTheInterfacePackagePublishesTheProxy()
    {
        Built built = BuildAll();

        StringAssert.Contains(Source(built, "lib", "жLesser-generic-proxy"), "public sealed class жLesser<E> : global::go.example.lib_package.Lesser<жLesser<E>>, IжAdapter");
        StringAssert.Contains(Source(built, "lib", "жPicker-generic-proxy"), "public sealed class жPicker<E>");
        Assert.AreEqual("", Source(built, "lib", "жGetter"), "a constraint over a different parameter publishes no proxy");

        foreach (string consumer in new[] { "b1", "main" })
            Assert.IsFalse(built.Generated[consumer].Any(source => source.HintName.Contains("proxy")), $"{consumer} must declare no proxy");
    }

    /// <summary>Both instantiating packages bind; every binder binds the same methods, so the second changes nothing.</summary>
    [TestMethod]
    public void EveryInstantiatingPackageBindsTheSlots()
    {
        Built built = BuildAll();
        const string Binding = "global::go.example.lib_package.жLesser<global::go.example.elem_package.Node>.ᴛLess = static (recv, ᴛ0) => recv.Less(ᴛ0);";

        StringAssert.Contains(Source(built, "b1", "NodeжLesser-bind"), Binding);
        StringAssert.Contains(Source(built, "main", "NodeжLesser-bind"), Binding);
        StringAssert.Contains(Source(built, "b1", "NodeжPicker-bind"), "ᴛEach = static (recv, ᴛ0) => recv.Each(ᴛ0);");
    }

    /// <summary>An element no package bound panics naming the proxy, the element, the interface and the method.</summary>
    [TestMethod]
    public void AnUnboundSlotPanicsByName()
    {
        Assert.AreEqual("go2cs: constraint proxy жLesser<go.example.elem_package+Other>: no package instantiating Lesser[*Other] bound method Less", Invoke(BuildAll(), "Unbound"));
    }

    // Go 1.26's GraphNode <-> Graph cycle (behavioral SelfRefGenerics): an interface constraining ITSELF whose method names
    // a generic CONSTRAINED by it, `Owner() *Graph[N]` with `Graph[N GraphNode[N]]`.
    private const string CycleLibSource =
        """
        using go;

        namespace go.example;

        public static partial class graph_package
        {
            public partial interface GraphNode<N> where N : GraphNode<N>
            {
                N Next();

                ж<Graph<N>> Owner();
            }

            public partial struct Graph<N> where N : GraphNode<N> { public N head; }
        }
        """;

    private const string CycleElemSource =
        """
        using go;

        namespace go.example;

        public static partial class gnode_package
        {
            public partial struct GNode { public int K; }

            public static ж<GNode> Next(this ж<GNode> n) => n;

            public static ж<graph_package.Graph<graph_package.жGraphNode<GNode>>> Owner(this ж<GNode> n) => null!;
        }
        """;

    private const string CycleConsumerSource =
        """
        using go;

        [assembly: GoImplement<global::go.example.gnode_package.GNode, global::go.example.graph_package.GraphNode<global::go.example.graph_package.жGraphNode<global::go.example.gnode_package.GNode>>>(ConstraintProxy = true)]

        namespace go.example;

        public static partial class gmain_package
        {
            public static global::go.example.graph_package.Graph<global::go.example.graph_package.жGraphNode<gnode_package.GNode>> Make() => default;
        }
        """;

    /// <summary>
    /// A slot names the self parameter as the PROXY wherever it is a type argument whose parameter carries the
    /// self-referential constraint, as the converter renders `*Graph[*GNode]` in the element's own method; everywhere
    /// else (a bare result, a slice element) it is the BOX. A box there was CS0311 in the interface's own assembly
    /// (`Graph<ж<E>>` does not satisfy `where N : GraphNode<N>`), which broke SelfRefGenerics; the consumer's binder
    /// lambdas then bind the element's methods exactly as their signatures read.
    /// </summary>
    [TestMethod]
    public void ASlotNamesTheProxyWhereTheConstraintRequiresIt()
    {
        Built built = new([], []);

        Build(built, "shims", ShimSource);
        Build(built, "graph", CycleLibSource);
        Build(built, "gnode", CycleElemSource);
        Build(built, "gmain", CycleConsumerSource);

        string proxy = Source(built, "graph", "жGraphNode-generic-proxy");

        StringAssert.Contains(proxy, "global::System.Func<ж<E>, global::go.ж<global::go.example.graph_package.Graph<жGraphNode<E>>>> ᴛOwner");
        StringAssert.Contains(proxy, "global::System.Func<ж<E>, ж<E>> ᴛNext");
    }
}
