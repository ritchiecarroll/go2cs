// ProxyContainerForwardingTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go2cs.Tests.ForeignConstraintProxyTests;

namespace go2cs.Tests;

/// <summary>
/// A constraint proxy forwards every self-referential constraint method whose values can cross the proxy/box
/// boundary exactly, and refuses by name the ones whose values share the element's storage in Go.
/// </summary>
/// <remarks>
/// <para>
/// Exact: a delegate RESULT (`Next() func() N`, re-wrapped, a nil func kept nil), a delegate nested in a delegate
/// parameter (`Each(func(func(N) bool) bool)`), a NAMED func type (`iter.Seq[N]`), and a Go array in either direction (`Triple() [3]N`,
/// `Take([2]N)`), which Go copies, so an element-wise copy is exact.
/// </para>
/// <para>
/// Refused: a slice, map, channel or pointer of N, and a generic type carrying N through a parameter that is not
/// self-constrained (`Pairs() List[N]`). A proxy is a separate object per box, so a forwarded copy would compile and
/// break Go's aliasing. The forwarder has no slot and throws the converter's refusal, so the interface's own package
/// still compiles; the converter refuses every instantiation that would reach it (constraintProxyForwarding.go).
/// </para>
/// <para>
/// Each test builds the assemblies with the real generators and EXECUTES the result, through the generic proxy a
/// foreign interface publishes, and through the concrete proxy a same-package record emits.
/// </para>
/// </remarks>
[TestClass]
public class ProxyContainerForwardingTests
{
    private const string FlowLibSource =
        """
        using go;

        namespace go.example;

        public static partial class flow_package
        {
            public partial struct List<T> { }

            // A NAMED Go func type, as iter.Seq converts.
            public delegate void Seq<V>(System.Func<V, bool> yield);

            public partial interface Flow<N>
            {
                N Self();

                System.Func<N> Next();

                bool Each(System.Func<System.Func<N, bool>, bool> visit);

                array<N> Triple();

                bool Take(array<N> a);

                Seq<N> All();

                slice<N> Children();

                map<int, N> Index();

                void Feed(channel<N> ch);

                ж<N> Parent();

                List<N> Pairs();
            }

            public static string Drive<N>(N n, N stopped) where N : Flow<N>
            {
                System.Func<N> next = n.Next();
                string children;

                try
                {
                    n.Children();
                    children = "no refusal";
                }
                catch (PanicException panic)
                {
                    children = panic.Message;
                }

                int seen = 0;
                n.All()(x => { seen += x.Equals(n) ? 1 : 0; return true; });

                return string.Join(" ", new object[]
                {
                    n.Self().Equals(n),
                    next is not null && next().Equals(n),
                    stopped.Next() is null,
                    n.Each(pred => pred(n)),
                    n.Triple().Length == 3 && n.Triple()[2].Equals(n),
                    n.Take(new array<N>(new[] { n, n })),
                    seen == 2
                }) + " | " + children;
            }
        }
        """;

    // The element declares only the methods a proxy forwards: a refused method has no slot to bind.
    private const string FlowElemSource =
        """
        using go;

        namespace go.example;

        public static partial class fnode_package
        {
            public partial struct Node { public int K; }

            public static ж<Node> Self(this ж<Node> n) => n;

            public static System.Func<ж<Node>> Next(this ж<Node> n) => n.Value.K > 0 ? () => n : null!;

            public static bool Each(this ж<Node> n, System.Func<System.Func<ж<Node>, bool>, bool> visit) => visit(x => ReferenceEquals(x, n));

            public static array<ж<Node>> Triple(this ж<Node> n) => new(new[] { n, n, n });

            public static bool Take(this ж<Node> n, array<ж<Node>> a) => a.Length == 2 && ReferenceEquals(a[0], n) && ReferenceEquals(a[1], n);

            public static flow_package.Seq<ж<Node>> All(this ж<Node> n) => yield => { if (yield(n)) yield(n); };
        }
        """;

    private const string FlowMainSource =
        """
        using go;

        [assembly: GoImplement<global::go.example.fnode_package.Node, global::go.example.flow_package.Flow<global::go.example.fnode_package.Node>>(ConstraintProxy = true)]

        namespace go.example;

        public static partial class main_package
        {
            public static string Run() =>
                flow_package.Drive<flow_package.жFlow<fnode_package.Node>>(
                    new ж<fnode_package.Node>(new fnode_package.Node { K = 1 }),
                    new ж<fnode_package.Node>(new fnode_package.Node { K = 0 }));
        }
        """;

    // The same shapes declared, recorded and driven in ONE package: the record emits the CONCRETE proxy NodeжFlow.
    private const string LocalSource =
        """
        using go;

        [assembly: GoImplement<global::go.example.main_package.Node, global::go.example.main_package.Flow<global::go.example.main_package.Node>>(ConstraintProxy = true)]

        namespace go.example;

        public static partial class main_package
        {
            public delegate void Seq<V>(System.Func<V, bool> yield);

            public partial interface Flow<N>
            {
                N Self();

                System.Func<N> Next();

                bool Each(System.Func<System.Func<N, bool>, bool> visit);

                array<N> Triple();

                bool Take(array<N> a);

                Seq<N> All();

                slice<N> Children();
            }

            public partial struct Node { public int K; }

            public static ж<Node> Self(this ж<Node> n) => n;

            public static System.Func<ж<Node>> Next(this ж<Node> n) => n.Value.K > 0 ? () => n : null!;

            public static bool Each(this ж<Node> n, System.Func<System.Func<ж<Node>, bool>, bool> visit) => visit(x => ReferenceEquals(x, n));

            public static array<ж<Node>> Triple(this ж<Node> n) => new(new[] { n, n, n });

            public static bool Take(this ж<Node> n, array<ж<Node>> a) => a.Length == 2 && ReferenceEquals(a[0], n) && ReferenceEquals(a[1], n);

            public static Seq<ж<Node>> All(this ж<Node> n) => yield => { if (yield(n)) yield(n); };

            public static string Drive<N>(N n, N stopped) where N : Flow<N>
            {
                System.Func<N> next = n.Next();
                string children;

                try
                {
                    n.Children();
                    children = "no refusal";
                }
                catch (PanicException panic)
                {
                    children = panic.Message;
                }

                int seen = 0;
                n.All()(x => { seen += x.Equals(n) ? 1 : 0; return true; });

                return string.Join(" ", new object[]
                {
                    n.Self().Equals(n),
                    next is not null && next().Equals(n),
                    stopped.Next() is null,
                    n.Each(pred => pred(n)),
                    n.Triple().Length == 3 && n.Triple()[2].Equals(n),
                    n.Take(new array<N>(new[] { n, n })),
                    seen == 2
                }) + " | " + children;
            }

            public static string Run() =>
                Drive<NodeжFlow>(new ж<Node>(new Node { K = 1 }), new ж<Node>(new Node { K = 0 }));
        }
        """;

    private static Built BuildForeign()
    {
        Built built = new([], []);

        Build(built, "shims", ShimSource);
        Build(built, "flow", FlowLibSource);
        Build(built, "fnode", FlowElemSource);
        Build(built, "main", FlowMainSource);

        return built;
    }

    private const string ChildrenRefusal = "Flow.Children (returns slice<N>), which in Go shares the element's own storage; a proxy is a separate object per pointer and cannot forward it, so go2cs refuses every instantiation of Flow over a pointer element";

    /// <summary>
    /// Through the GENERIC proxy: Self, the delegate result and its nil, the nested delegate, the array result and
    /// parameter, and a NAMED delegate (iter.Seq's shape) all forward exactly; Children throws the refusal by name.
    /// </summary>
    [TestMethod]
    public void TheGenericProxyForwardsEveryExactShapeAndRefusesChildrenByName()
    {
        Assert.AreEqual($"True True True True True True True | go2cs: constraint proxy жFlow<E>: {ChildrenRefusal}", Invoke(BuildForeign(), "Run"));
    }

    /// <summary>Through the CONCRETE proxy a same-package record emits: the same answers.</summary>
    [TestMethod]
    public void TheConcreteProxyForwardsEveryExactShapeAndRefusesChildrenByName()
    {
        Built built = new([], []);

        Build(built, "shims", ShimSource);
        Build(built, "main", LocalSource);

        Assert.AreEqual($"True True True True True True True | go2cs: constraint proxy NodeжFlow: {ChildrenRefusal}", Invoke(built, "Run"));
    }

    /// <summary>
    /// Every shape that shares the element's storage is refused by name and has no slot; every exact shape has one,
    /// and the delegate result keeps a nil func nil.
    /// </summary>
    [TestMethod]
    public void EverySharingShapeIsRefusedAndHasNoSlot()
    {
        string proxy = Source(BuildForeign(), "flow", "жFlow-generic-proxy");

        foreach ((string method, string position) in new[] { ("Children", "returns slice<N>"), ("Index", "returns map<int, N>"), ("Feed", "takes channel<N>"), ("Parent", "returns ж<N>"), ("Pairs", "returns List<N>") })
        {
            StringAssert.Contains(proxy, $"Flow.{method} ({position}), which in Go shares the element's own storage");
            Assert.IsFalse(proxy.Contains($"ᴛ{method} ="), $"the refused {method} must have no slot to bind:\n{proxy}");
        }

        foreach (string forwarded in new[] { "Self", "Next", "Each", "Triple", "Take", "All" })
            StringAssert.Contains(proxy, $"ᴛ{forwarded} = static");

        StringAssert.Contains(proxy, "ᴛNext(m_box) is { } ᴛf0 ? new System.Func<жFlow<E>>(() => ᴛf0()) : null");
    }
}
