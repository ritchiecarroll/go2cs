// ZzB9C1rProbeTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// G's batch-9 RE-review probes for claude/c1-reinterpret-no-gvm at 44d476ae0 (this branch's base): root kinds
// (field, element, a reference-bearing wrapper) through 3-hop chains and narrow-then-widen, and B9InexactFieldRootChainEqualsDirect,
// RED at 44d476ae0 (a field root whose order token is inexact: chain != direct) and green at hop tip 0195a29022.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

[TestClass]
public class ZzB9C1rProbeTests
{
    public struct Employee { public @string Name; public nint Age; }
    public struct Person { public @string Name; public nint Age; }
    public struct Staff { public @string Name; public nint Age; }
    public struct Pair { public Employee First; public Employee Second; }
    public struct Wrapper { public Employee Inner; }
    public struct Wide { public ulong A; public ulong B; }
    public struct Wide2 { public ulong C; public ulong D; }
    public struct Holder { public ulong X; public Wide W; public ulong Y; }

    private static ref Employee ᏑSecond(object p) => ref ((ж<Pair>)p).Value.Second;
    private static ref Wide ᏑW(object p) => ref ((ж<Holder>)p).Value.W;
    // A field accessor whose Go offset cannot be resolved (no Ꮡ/goref_ name): an INEXACT order token.
    private static ref Employee PlainSecond(object p) => ref ((ж<Pair>)p).Value.Second;

    private static void Gc() { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }

    [TestMethod]
    public void B9FieldRootThreeHop()
    {
        ж<Pair> pair = Ꮡ(new Pair { Second = new Employee { Name = "s", Age = 2 } });
        ж<Employee> second = pair.of(ᏑSecond);
        ж<Employee> back = second.Reinterpret<Employee, Person>().Reinterpret<Person, Staff>().Reinterpret<Staff, Employee>();
        ж<Staff> chain = second.Reinterpret<Employee, Person>().Reinterpret<Person, Staff>();
        ж<Staff> direct = second.Reinterpret<Employee, Staff>();
        Console.WriteLine($"B9FIELD3 same={ReferenceEquals(back, second)} back==second={back == second} fresh={back == pair.of(ᏑSecond)} chain==direct={chain == direct} hash={chain.GetHashCode() == direct.GetHashCode()}");
        Gc();
        chain.Value.Name = "z";
        back.Value.Age = 99;
        Console.WriteLine($"B9FIELD3 first={pair.Value.First.Name}/{pair.Value.First.Age} second={pair.Value.Second.Name}/{pair.Value.Second.Age}");
        Assert.IsTrue(back == second && second == back && back == pair.of(ᏑSecond));
        Assert.IsTrue(chain == direct && direct == chain);
        Assert.AreEqual("z", pair.Value.Second.Name.ToString());
        Assert.AreEqual((nint)99, pair.Value.Second.Age);
        Assert.AreEqual((nint)0, pair.Value.First.Age, "aliases s.f, never s");
    }

    [TestMethod]
    public void B9FieldRootNarrowThenWiden()
    {
        ж<Holder> h = Ꮡ(new Holder { X = 7, W = new Wide { A = 1, B = 2 }, Y = 8 });
        ж<Wide> w = h.of(ᏑW);
        ж<Wide2> via = w.Reinterpret<Wide, ulong>().Reinterpret<ulong, Wide2>();
        ж<Wide> backW = w.Reinterpret<Wide, ulong>().Reinterpret<ulong, Wide>();
        Console.WriteLine($"B9FIELDNW via==direct={via == w.Reinterpret<Wide, Wide2>()} backW==w={backW == w} sameW={ReferenceEquals(backW, w)}");
        Gc();
        via.Value.D = 42;
        via.Value.C = 41;
        Console.WriteLine($"B9FIELDNW X={h.Value.X} A={h.Value.W.A} B={h.Value.W.B} Y={h.Value.Y}");
        Assert.AreEqual(41UL, h.Value.W.A);
        Assert.AreEqual(42UL, h.Value.W.B);
        Assert.AreEqual(7UL, h.Value.X, "aliases h.W, never h");
        Assert.AreEqual(8UL, h.Value.Y);
        Assert.IsTrue(via == w.Reinterpret<Wide, Wide2>() && backW == w);
    }

    [TestMethod]
    public void B9ElementRootNarrowThenWiden()
    {
        slice<Wide> s = new Wide[] { new() { A = 1, B = 2 }, new() { A = 3, B = 4 }, new() { A = 5, B = 6 } }.slice();
        ж<Wide> el = Ꮡ(s, 1);
        ж<Wide2> via = el.Reinterpret<Wide, ulong>().Reinterpret<ulong, Wide2>();
        ж<Wide2> direct = Ꮡ(s, 1).Reinterpret<Wide, Wide2>();
        ж<Wide> back = el.Reinterpret<Wide, ulong>().Reinterpret<ulong, Wide2>().Reinterpret<Wide2, Wide>();
        Console.WriteLine($"B9ELEMNW type={via.GetType().Name} via==direct={via == direct} hash={via.GetHashCode() == direct.GetHashCode()} back==el={back == el}");
        Gc();
        via.Value.D = 44;
        Console.WriteLine($"B9ELEMNW s0.B={s[0].B} s1.B={s[1].B} s2.A={s[2].A}");
        Assert.AreEqual(44UL, s[1].B);
        Assert.AreEqual(2UL, s[0].B);
        Assert.AreEqual(5UL, s[2].A);
        Assert.IsTrue(via == direct && back == el);
    }

    [TestMethod]
    public void B9RefBearingWrapperChain()
    {
        ж<Wrapper> o = Ꮡ(new Wrapper { Inner = new Employee { Name = "w", Age = 1 } });
        ж<Employee> v1 = o.Reinterpret<Wrapper, Employee>();
        ж<Person> v2 = v1.Reinterpret<Employee, Person>();
        ж<Wrapper> back = v2.Reinterpret<Person, Wrapper>();
        ж<Employee> e2 = v2.Reinterpret<Person, Employee>();
        Console.WriteLine($"B9WRAP back==o={back == o} sameO={ReferenceEquals(back, o)} backType={back.GetType().Name} e2==v1={e2 == v1} sameV1={ReferenceEquals(e2, v1)} hash={e2.GetHashCode() == v1.GetHashCode()}");
        Gc();
        v2.Value.Name = "after-gc";
        back.Value.Inner.Age = 77;
        Console.WriteLine($"B9WRAP name={o.Value.Inner.Name} age={o.Value.Inner.Age}");
        Assert.AreEqual("after-gc", o.Value.Inner.Name.ToString());
        Assert.AreEqual((nint)77, o.Value.Inner.Age);
        Assert.IsTrue(back == o && e2 == v1);
    }

    [TestMethod]
    public void B9InexactFieldRootChainEqualsDirect()
    {
        ж<Pair> pair = Ꮡ(new Pair());
        ж<Employee> f = pair.of(PlainSecond);
        ж<Staff> chain = f.Reinterpret<Employee, Person>().Reinterpret<Person, Staff>();
        ж<Staff> direct = f.Reinterpret<Employee, Staff>();
        Console.WriteLine($"B9INEXACT chain==direct={chain == direct} direct==chain={direct == chain} hash={chain.GetHashCode() == direct.GetHashCode()}");
        Assert.IsTrue(chain == direct, "(*Staff)((*Person)(f)) == (*Staff)(f)");
    }

    [TestMethod]
    public void B9GvmCensus()
    {
        foreach (Type t in typeof(ж<>).Assembly.GetTypes())
            foreach (var m in t.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly))
                if (m.IsGenericMethodDefinition && (m.IsVirtual || m.IsAbstract))
                    Console.WriteLine($"B9GVM {(t.IsInterface ? "iface" : "class")} {t.FullName}: {m}");
    }
}
