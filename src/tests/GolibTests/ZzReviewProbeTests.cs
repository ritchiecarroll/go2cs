// ZzReviewProbeTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// G's batch-9 review probes for claude/c1-reinterpret-no-gvm (0605e939f), verbatim: green at hop tip 0195a29022,
// red at 0605e939f on the three Assert-bearing arms; the GVM census arm only prints.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

[TestClass]
public class ZzReviewProbeTests
{
    public struct Employee { public @string Name; public nint Age; }
    public struct Person { public @string Name; public nint Age; }
    public struct Staff { public @string Name; public nint Age; }
    public struct Wide { public ulong A; public ulong B; }
    public struct Wide2 { public ulong C; public ulong D; }

    [TestMethod]
    public void ZzThreeHopChainBackToRootEqualsRoot()
    {
        ж<Employee> e = Ꮡ(new Employee { Name = "ann", Age = 30 });
        ж<Employee> back = e.Reinterpret<Employee, Person>().Reinterpret<Person, Staff>().Reinterpret<Staff, Employee>();
        Console.WriteLine($"PROBE3HOP same={ReferenceEquals(back, e)} back==e={back == e} e==back={e == back} type={back.GetType().Name}");
        Assert.IsTrue(back == e, "(*Employee)((*Staff)((*Person)(e))) == e");
        Assert.IsTrue(e == back, "e == (*Employee)((*Staff)((*Person)(e)))");
    }

    [TestMethod]
    public void ZzThreeHopChainBackToRootEqualsRoot_Element()
    {
        slice<Employee> s = new Employee[] { new() { Name = "bob", Age = 1 }, new() { Name = "cat", Age = 2 } }.slice();
        ж<Employee> el = Ꮡ(s, 1);
        ж<Employee> back = el.Reinterpret<Employee, Person>().Reinterpret<Person, Staff>().Reinterpret<Staff, Employee>();
        Console.WriteLine($"PROBE3HOPELEM same={ReferenceEquals(back, el)} back==el={back == el} el==back={el == back}");
        Assert.IsTrue(back == el && el == back, "3-hop element round trip");
    }

    [TestMethod]
    public void ZzNarrowThenWidenToAnotherTypeAliasesRoot()
    {
        ж<Wide> w = Ꮡ(new Wide { A = 1, B = 2 });
        ж<Wide2> w2;
        try
        {
            w2 = w.Reinterpret<Wide, ulong>().Reinterpret<ulong, Wide2>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PROBEWIDEN threw {ex.GetType().Name}: {ex.Message}");
            throw;
        }
        Console.WriteLine($"PROBEWIDEN type={w2.GetType().Name}");
        w2.Value.D = 42;
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Console.WriteLine($"PROBEWIDEN B={w.Value.B} D={w2.Value.D}");
        Assert.AreEqual(42UL, w.Value.B, "write through (*Wide2)((*uint64)(w)) reaches w.B");
    }

    [TestMethod]
    public void ZzCensusEveryGenericVirtualMethodInGolib()
    {
        Assembly golib = typeof(ж<>).Assembly;
        var hits = golib.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(m => m.IsGenericMethodDefinition && m.IsVirtual)
                .Select(m => $"{(t.IsInterface ? "iface" : "class")} generic={t.IsGenericTypeDefinition} {t.FullName}: {m}"))
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        foreach (string h in hits)
            Console.WriteLine("PROBEGVM " + h);
    }
}
