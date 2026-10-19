// GoGrowSliceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// golib's slice growth is Go's runtime.growslice: <c>nextslicecap</c>, then <c>roundupsize</c> over Go's
/// size classes for the element's Go size, with the malloc header a pointer-bearing object over 512 bytes
/// carries, and page rounding past 32 KiB. Every capacity <c>cap()</c> can observe after an append or a
/// <c>reflect.Value.Grow</c> is therefore Go's.
/// </summary>
/// <remarks>
/// The expected capacities are Go's own, recorded by testdata/growslice/main.go under <c>-gcflags='-N -l'</c>,
/// which keeps every append on the heap path (no stack backing store). repoguard's
/// TestGoGrowSliceExpectationsMatchTheToolchain re-runs that probe on the pinned toolchain and fails when
/// expected.txt drifts from it. RED before golib modeled growslice: every case past the first size class
/// differed (golib doubled below 1,024 elements, then grew by a quarter, with no size class).
/// </remarks>
[TestClass]
public class GoGrowSliceTests
{
    private struct S12 { public int a, b, c; }

    private struct S24 { public long a, b, c; }

    private struct P24 { public @string s; public long n; }

    private struct S40 { public long a, b, c, d, e; }

    private static string ExpectedPath([CallerFilePath] string thisFile = "") =>
        Path.Combine(Path.GetDirectoryName(thisFile)!, "testdata", "growslice", "expected.txt");

    private static Dictionary<string, long[]> Expected() =>
        File.ReadAllLines(ExpectedPath())
            .Where(line => line.Trim().Length > 0)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(words => words[0] + " " + words[1], words => words.Skip(2).Select(long.Parse).ToArray());

    // One element at a time, recording each capacity the slice takes (the probe's grows). An array element
    // appends its Go zero value, which has the array's length; default(array<E>) has none.
    private static long[] Grows<T>(int n) => Grows(n, () => default(T)!);

    private static long[] Grows<T>(int n, Func<T> zero)
    {
        slice<T> x = default!;
        List<long> caps = [];

        for (int i = 0; i < n; i++)
        {
            long old = cap(x);
            x = append(x, zero());

            if (cap(x) != old)
                caps.Add(cap(x));
        }

        return [.. caps];
    }

    private static readonly int[] OneShotLengths = [.. Enumerable.Range(1, 64),
        100, 255, 256, 257, 511, 512, 513, 1000, 1023, 1024, 1025, 4000, 4097, 8191, 8192, 8193,
        32760, 32761, 32768, 32769, 40000, 65536, 100000];

    // n elements in one append to a nil slice, for each n (the probe's oneShot).
    private static long[] OneShot<T>() =>
        [.. OneShotLengths.Select(n => (long)cap(append(default(slice<T>)!, new slice<T>(n).ꓸꓸꓸ)))];

    // reflect.Value.Grow one element past the capacity each time (the probe's reflectGrow).
    private static long[] ReflectGrow<T>(int n)
    {
        slice<T> s = default!;
        List<long> caps = [];

        for (int i = 0; i < n; i++)
        {
            s = s.slice(0, cap(s));
            s = (slice<T>)GoReflect.GrowSlice(s, typeof(T), 1)!;
            caps.Add(cap(s));
        }

        return [.. caps];
    }

    private static long[] MultiInt64x3()
    {
        slice<long> m = default!;
        List<long> caps = [];

        for (int i = 0; i < 400; i++)
        {
            long old = cap(m);
            m = append(m, 1L, 2L, 3L);

            if (cap(m) != old)
                caps.Add(cap(m));
        }

        return [.. caps];
    }

    private static long[] SpreadBytePlus5()
    {
        slice<byte> b = default!;
        slice<byte> five = new byte[] { 1, 2, 3, 4, 5 }.slice();
        List<long> caps = [];

        for (int i = 0; i < 800; i++)
        {
            long old = cap(b);
            b = append(b, five.ꓸꓸꓸ);

            if (cap(b) != old)
                caps.Add(cap(b));
        }

        return [.. caps];
    }

    private static long[] PastDoubleInt64()
    {
        slice<long> full = new(5, 5);
        full = append(full, new slice<long>(20).ꓸꓸꓸ);

        return [cap(full)];
    }

    private static readonly Dictionary<string, Func<long[]>> s_replays = new()
    {
        ["grow byte"] = () => Grows<byte>(70000),
        ["grow int32"] = () => Grows<int>(20000),
        ["grow int64"] = () => Grows<long>(10000),
        ["grow string"] = () => Grows<@string>(3000),
        ["grow ptr"] = () => Grows<ж<nint>>(5000),
        ["grow s12"] = () => Grows<S12>(3000),
        ["grow s24"] = () => Grows<S24>(3000),
        ["grow p24"] = () => Grows<P24>(3000),
        ["grow s40"] = () => Grows<S40>(2000),
        ["grow empty"] = () => Grows<EmptyStruct>(20),
        ["grow arr3byte"] = () => Grows(20, () => new array<byte>(3)),
        ["grow arr2u64"] = () => Grows(600, () => new array<ulong>(2)),
        ["grow arr2x3byte"] = () => Grows(2000, () => new array<array<byte>>(2, () => new array<byte>(3))),
        ["grow arr3ptr"] = () => Grows(3000, () => new array<ж<nint>>(3)),
        ["grow arr0int"] = () => Grows(20, () => new array<nint>(0)),
        ["grow arr0x3int"] = () => Grows(10, () => new array<array<nint>>(0)),
        ["oneshot byte"] = OneShot<byte>,
        ["oneshot int64"] = OneShot<long>,
        ["oneshot string"] = OneShot<@string>,
        ["oneshot s24"] = OneShot<S24>,
        ["oneshot p24"] = OneShot<P24>,
        ["multi int64x3"] = MultiInt64x3,
        ["spread byte+5"] = SpreadBytePlus5,
        ["pastdouble int64"] = PastDoubleInt64,
        ["reflect byte"] = () => ReflectGrow<byte>(24),
        ["reflect int64"] = () => ReflectGrow<long>(24),
    };

    [TestMethod]
    public void EveryRecordedCaseHasAReplay()
    {
        CollectionAssert.AreEquivalent(Expected().Keys.ToArray(), s_replays.Keys.ToArray(),
            "testdata/growslice/expected.txt and the replays must name the same cases");
    }

    [TestMethod]
    [DataRow("grow byte")]
    [DataRow("grow int32")]
    [DataRow("grow int64")]
    [DataRow("grow string")]
    [DataRow("grow ptr")]
    [DataRow("grow s12")]
    [DataRow("grow s24")]
    [DataRow("grow p24")]
    [DataRow("grow s40")]
    [DataRow("grow empty")]
    [DataRow("grow arr3byte")]
    [DataRow("grow arr2u64")]
    [DataRow("grow arr2x3byte")]
    [DataRow("grow arr3ptr")]
    [DataRow("grow arr0int")]
    [DataRow("grow arr0x3int")]
    [DataRow("oneshot byte")]
    [DataRow("oneshot int64")]
    [DataRow("oneshot string")]
    [DataRow("oneshot s24")]
    [DataRow("oneshot p24")]
    [DataRow("multi int64x3")]
    [DataRow("spread byte+5")]
    [DataRow("pastdouble int64")]
    [DataRow("reflect byte")]
    [DataRow("reflect int64")]
    public void CapacitiesAreGos(string name)
    {
        long[] want = Expected()[name];
        long[] got = s_replays[name]();

        CollectionAssert.AreEqual(want, got, $"{name}: want {string.Join(" ", want)}\n got {string.Join(" ", got)}");
    }

    // An unnamed array element is sized from the appended VALUE: [][3]byte and [][4]byte are one C# type,
    // slice<array<byte>>, so a per-type answer cached by the first growth would be wrong for the next. The
    // expected capacities are Go 1.26.9's for the same appends (-gcflags='-N -l'): 2, 2, 8 and 3.
    [TestMethod]
    public void AnUnnamedArrayElementIsSizedFromTheValueItAppends()
    {
        Assert.AreEqual(2L, (long)cap(append(default(slice<array<byte>>)!, new array<byte>(3))), "[][3]byte, one element");
        Assert.AreEqual(2L, (long)cap(append(default(slice<array<byte>>)!, new array<byte>(4))), "[][4]byte, one element");
        Assert.AreEqual(8L, (long)cap(append(default(slice<array<byte>>)!, new array<byte>(1), new array<byte>(1), new array<byte>(1))),
            "[][1]byte, three elements (roundupsize(3) = 8)");
        Assert.AreEqual(3L, (long)cap(append(default(slice<array<byte>>)!, new array<byte>(5), new array<byte>(5), new array<byte>(5))),
            "[][5]byte, three elements (roundupsize(15) = 16, 16 / 5 = 3)");
    }

    // The fallback stays visible: after every recorded case has run, no element type has grown without Go's
    // size-class rounding in this process.
    [TestMethod]
    public void NoElementTypeGrowsUnrounded()
    {
        foreach (Func<long[]> replay in s_replays.Values)
            replay();

        Type[] unrounded = [.. GoSizeClasses.UnroundedElementTypes];

        Assert.AreEqual(0, unrounded.Length,
            $"element types grown without Go's size-class rounding: {string.Join(", ", unrounded.Select(type => type.ToString()))}");
    }
}
