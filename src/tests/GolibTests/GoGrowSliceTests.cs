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

    // One element at a time, recording each capacity the slice takes (the probe's grows).
    private static long[] Grows<T>(int n)
    {
        slice<T> x = default!;
        List<long> caps = [];

        for (int i = 0; i < n; i++)
        {
            long old = cap(x);
            x = append(x, default(T)!);

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

    // DISCLOSED: an unnamed array element has no Go size golib can derive (a managed array<T> carries no
    // length), so its slice grows to nextslicecap unrounded. Go gives [][3]byte one element cap 2
    // (roundupsize(3) = 8, and 8 / 3 = 2); golib gives 1, and records the type where a guard can read it.
    // When this goes red because golib gives 2, the class is fixed: drop this case and the remarks on
    // GoSizeClasses.UnroundedElementTypes.
    [TestMethod]
    public void AnUnnamedArrayElementGrowsUnroundedAndIsRecorded()
    {
        slice<array<byte>> x = append(default(slice<array<byte>>)!, new array<byte>(3));

        Assert.AreEqual(1L, (long)cap(x), "golib's disclosed capacity for [][3]byte after one append (Go's is 2)");
        CollectionAssert.Contains(GoSizeClasses.UnroundedElementTypes.ToArray(), typeof(array<byte>));
    }

    // The fallback is visible: whatever element types this process has grown unrounded so far, after every
    // recorded case has run, are the disclosed class only, and none of the cases' own element types.
    [TestMethod]
    public void OnlyUnnamedArrayElementsGrowUnrounded()
    {
        foreach (Func<long[]> replay in s_replays.Values)
            replay();

        Type[] unrounded = [.. GoSizeClasses.UnroundedElementTypes];
        Type[] outsideTheClass = [.. unrounded.Where(type => !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(array<>))];

        Assert.AreEqual(0, outsideTheClass.Length,
            $"element types grown without Go's size-class rounding: {string.Join(", ", outsideTheClass.Select(type => type.ToString()))}");

        Type[] recorded = [typeof(byte), typeof(int), typeof(long), typeof(@string), typeof(ж<nint>), typeof(S12), typeof(S24),
            typeof(P24), typeof(S40), typeof(EmptyStruct)];

        foreach (Type type in recorded)
            CollectionAssert.DoesNotContain(unrounded, type);
    }
}
