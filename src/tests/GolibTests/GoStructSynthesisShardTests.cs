// GoStructSynthesisShardTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

// A synthesized type in a LATER shard must still reach an internal type of an assembly that grants the
// mint's name -- exactly what every converted csproj does for its unexported Go types. This test
// assembly grants it so arm (c) can prove the grant survives rotation.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("go2cs.SynthesizedStructs")]

namespace GolibTests;

// An internal type a synthesized array field is built over: the emitted zero constructor unboxes to
// array<ShardFriendProbe>, which is an access check the friend grant must satisfy.
internal struct ShardFriendProbe
{
    public long V;
}

/// <summary>
/// The SHARDED mint (GoStructSynthesis.ShardCapacity): one dynamic module made every StructOf O(n) in
/// the types already minted, so the mint rotates to a fresh same-named assembly every 128 types. These
/// arms pin what the rotation must NOT change -- identity, cross-shard references, the friend grant,
/// the pkgpath container, the delegate mint -- and the one thing it must: a per-type cost that does not
/// grow with the count.
/// </summary>
[TestClass]
public class GoStructSynthesisShardTests
{
    private static int s_unique;

    private static GoSynthField Field(string name, Type type, nint[]? dims = null, string dimsKey = "") =>
        new(name, type, "", false, dims, null, dimsKey);

    private static string Unique(string stem) => stem + Interlocked.Increment(ref s_unique);

    // More than one shard's worth of distinct mints, so whatever was minted before this call and
    // whatever is minted after it cannot share a module.
    private static void CrossAShard()
    {
        for (int i = 0; i < 2 * 128 + 8; i++)
            GoStructSynthesis.SynthesizeStructType([Field(Unique("Filler"), typeof(long))], "");
    }

    [TestMethod]
    public void A_OneShapeIsOneTypeAcrossAShardBoundary()
    {
        GoSynthField[] shape = [Field(Unique("Same"), typeof(long)), Field("B", typeof(int))];
        Type first = GoStructSynthesis.SynthesizeStructType(shape, "");

        CrossAShard();

        Type again = GoStructSynthesis.SynthesizeStructType(shape, "");
        Type later = GoStructSynthesis.SynthesizeStructType([Field(Unique("Later"), typeof(long))], "");

        Assert.AreNotSame(first.Assembly, later.Assembly, "the premise: the mint rotated between the two requests");
        Assert.AreSame(first, again, "StructOf of one shape is one Type, whichever shard is current -- the intern sits above the shards");
    }

    [TestMethod]
    public void B_ALaterShardTypeHoldsAnEarlierShardTypeAsAField()
    {
        Type inner = GoStructSynthesis.SynthesizeStructType([Field(Unique("X"), typeof(long))], "");
        CrossAShard();
        Type outer = GoStructSynthesis.SynthesizeStructType([Field(Unique("In"), inner)], "");

        Assert.AreNotSame(inner.Assembly, outer.Assembly, "the premise: the two types live in different shards");

        object innerValue = Activator.CreateInstance(inner)!;
        inner.GetFields()[0].SetValue(innerValue, 7L);

        object outerValue = Activator.CreateInstance(outer)!;
        outer.GetFields()[0].SetValue(outerValue, innerValue);

        object readBack = outer.GetFields()[0].GetValue(outerValue)!;
        Assert.AreEqual(7L, inner.GetFields()[0].GetValue(readBack), "a cross-shard field reads and writes like any other");
    }

    [TestMethod]
    public void C_ALaterShardStillReachesAnInternalTypeThroughTheFriendGrant()
    {
        Type early = GoStructSynthesis.SynthesizeStructType([Field(Unique("E"), typeof(long))], "");
        CrossAShard();

        // An ARRAY field is seeded by an emitted constructor whose IL unboxes to array<ShardFriendProbe>:
        // an access check on an internal type of THIS assembly, satisfied only by the grant above.
        Type t = GoStructSynthesis.SynthesizeStructType(
            [Field(Unique("Probes"), typeof(array<ShardFriendProbe>), [2], "[2]")], "");

        Assert.AreNotSame(early.Assembly, t.Assembly, "the premise: minted after a rotation");

        object value = Activator.CreateInstance(t)!;
        array<ShardFriendProbe> probes = (array<ShardFriendProbe>)t.GetFields()[0].GetValue(value)!;

        Assert.AreEqual(2, (int)probes.Length, "the emitted constructor ran -- the friend grant reached the later shard");
    }

    [TestMethod]
    public void D_APackageContainerIsRebuiltPerShardAndAnswersTheSamePath()
    {
        // DOTLESS, like GoStructSynthesisTests' container row: a dotted import path (example.com/x)
        // renders back with its dots as slashes in ANY shard -- the container is named through the
        // namespace, a pre-existing limitation this seat does not touch and does not test.
        const string pkg = "encoding/shardprobe";

        Type before = GoStructSynthesis.SynthesizeStructType([Field(Unique("a"), typeof(long))], pkg);
        CrossAShard();
        Type after = GoStructSynthesis.SynthesizeStructType([Field(Unique("b"), typeof(long))], pkg);

        Assert.AreNotSame(before.Assembly, after.Assembly, "the premise: the two types live in different shards");
        Assert.AreEqual(pkg, GoReflect.GoPackagePath(before), "the first shard's container answers the path");
        Assert.AreEqual(pkg, GoReflect.GoPackagePath(after), "and so does the rebuilt container in the next shard");
    }

    [TestMethod]
    public void E_TheMintCostDoesNotGrowWithTheCount()
    {
        static double timeBlock(int count)
        {
            Stopwatch clock = Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
                GoStructSynthesis.SynthesizeStructType([Field(Unique("Rate"), typeof(long))], "");

            return clock.Elapsed.TotalMilliseconds;
        }

        timeBlock(200); // warm the path
        double early = timeBlock(1000);

        for (int i = 0; i < 9; i++)
            timeBlock(1000);

        double late = timeBlock(1000);

        Console.WriteLine($"SHARDMINT early={early:F0} ms late={late:F0} ms per 1000 (after ~11k mints)");
        Assert.IsTrue(late < 2 * early + 50,
            $"a mint 10,000 types later cost {late:F0} ms per 1000 against {early:F0} -- the per-type cost is growing with the count (the O(n) the shards exist to bound)");
    }

    [TestMethod]
    public void F_ADelegateMintedAfterARotationIsOneTypeAndCallable()
    {
        Type early = GoStructSynthesis.SynthesizeStructType([Field(Unique("D"), typeof(long))], "");
        CrossAShard();

        // Past every declared family (Func/Action stop at 16, golib's ladder at 24), so it is MINTED.
        Type[] parameters = new Type[30];
        Array.Fill(parameters, typeof(long));

        Type first = GoDelegateSynthesis.SynthesizeDelegateType(parameters, typeof(long));
        Type again = GoDelegateSynthesis.SynthesizeDelegateType(parameters, typeof(long));

        Assert.AreNotSame(early.Assembly, first.Assembly, "the premise: the delegate was minted after a rotation");
        Assert.AreSame(first, again, "one signature is one delegate Type across shards");

        ParameterExpression[] args = new ParameterExpression[30];

        for (int i = 0; i < args.Length; i++)
            args[i] = Expression.Parameter(typeof(long), "p" + i);

        Delegate sum = Expression.Lambda(first, Expression.Add(args[0], args[29]), args).Compile();
        object[] values = new object[30];

        for (int i = 0; i < values.Length; i++)
            values[i] = (long)i;

        Assert.AreEqual(29L, sum.DynamicInvoke(values), "the minted delegate type is callable");
    }
}
