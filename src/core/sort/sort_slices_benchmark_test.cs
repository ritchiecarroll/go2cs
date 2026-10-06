// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using rand = go.math.rand.rand_package;
using slices = slices_package;
using static sort_package;
using strconv = strconv_package;
using stringspkg = strings_package;
using testing = testing_package;
using go.math.rand;
using sort = sort_package;
using static go.sort_internal_test_package;
using strings = strings_package;

partial class sort_test_package {

// Benchmarks comparing sorting from the slices package with functions from
// the sort package (avoiding functions that are just forwarding to the slices
// package).
internal static slice<nint> makeRandomInts(nint n) {
    var r = rand.New(new rand.PCGжSource(rand.NewPCG(42, 0)));
    var ints = new slice<nint>(n);
    for (nint i = 0; i < n; i++) {
        ints[i] = r.IntN(n);
    }
    return ints;
}

internal static slice<nint> makeSortedInts(nint n) {
    var ints = new slice<nint>(n);
    for (nint i = 0; i < n; i++) {
        ints[i] = i;
    }
    return ints;
}

internal static slice<nint> makeReversedInts(nint n) {
    var ints = new slice<nint>(n);
    for (nint i = 0; i < n; i++) {
        ints[i] = n - i;
    }
    return ints;
}

internal static slice<@string> makeSortedStrings(nint n) {
    var x = new slice<@string>(n);
    for (nint i = 0; i < n; i++) {
        x[i] = strconv.Itoa(i);
    }
    Strings(x);
    return x;
}

public static UntypedInt N => 100_000;

public static void BenchmarkSortInts(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var ints = makeRandomInts(N);
        b.StartTimer();
        Sort((sort.Interface)(((sort.IntSlice)ints)));
    }
}

public static void BenchmarkSlicesSortInts(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var ints = makeRandomInts(N);
        b.StartTimer();
        slices.Sort<slice<nint>, nint>(ints);
    }
}

public static void BenchmarkSortIsSorted(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var ints = makeSortedInts(N);
        b.StartTimer();
        IsSorted(((sort.IntSlice)ints));
    }
}

public static void BenchmarkSlicesIsSorted(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var ints = makeSortedInts(N);
        b.StartTimer();
        slices.IsSorted<slice<nint>, nint>(ints);
    }
}

// makeRandomStrings generates n random strings with alphabetic runes of
// varying lengths.
internal static slice<@string> makeRandomStrings(nint n) {
    var r = rand.New(new rand.PCGжSource(rand.NewPCG(42, 0)));
    slice<rune> letters = slice<rune>((@string)"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
    var ss = new slice<@string>(n);
    for (nint i = 0; i < n; i++) {
        ref var sb = ref heap(new stringspkg.Builder(), out var Ꮡsb);
        nint slen = 2 + r.IntN(50);
        for (nint j = 0; j < slen; j++) {
            Ꮡsb.WriteRune(letters[r.IntN(len(letters))]);
        }
        ss[i] = sb.String();
    }
    return ss;
}

public static void BenchmarkSortStrings(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var ss = makeRandomStrings(N);
        b.StartTimer();
        Sort((sort.Interface)(((sort.StringSlice)ss)));
    }
}

public static void BenchmarkSlicesSortStrings(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var ss = makeRandomStrings(N);
        b.StartTimer();
        slices.Sort<slice<@string>, @string>(ss);
    }
}

public static void BenchmarkSortStrings_Sorted(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ss = makeSortedStrings(N);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Sort((sort.Interface)(((sort.StringSlice)ss)));
    }
}

public static void BenchmarkSlicesSortStrings_Sorted(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ss = makeSortedStrings(N);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        slices.Sort<slice<@string>, @string>(ss);
    }
}

// These benchmarks compare sorting a slice of structs with sort.Sort vs.
// slices.SortFunc.
[GoType] partial struct myStruct {
    internal @string a, b, c, d;
    internal nint n;
}

[GoType("[]ж<myStruct>")] partial struct myStructs;

internal static nint Len(this myStructs s) {
    return len(s);
}

internal static bool Less(this myStructs s, nint i, nint j) {
    return (~s[i]).n < (~s[j]).n;
}

internal static void Swap(this myStructs s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

internal static myStructs makeRandomStructs(nint n) {
    var r = rand.New(new rand.PCGжSource(rand.NewPCG(42, 0)));
    var structs = new slice<ж<myStruct>>(n);
    for (nint i = 0; i < n; i++) {
        structs[i] = Ꮡ(new myStruct(n: r.IntN(n)));
    }
    return structs;
}

public static void TestStructSorts(ж<testing.T> Ꮡt) {
    var ss = makeRandomStructs(200);
    var ss2 = new slice<ж<myStruct>>(len(ss));
    foreach (var (i, _) in ss) {
        ss2[i] = Ꮡ(new myStruct(n: (~ss[i]).n));
    }
    Sort(ss);
    slices.SortFunc(ss2, (ж<myStruct> a, ж<myStruct> b) => (~a).n - (~b).n);
    foreach (var (i, _) in ss) {
        if (ss[i].Value != ss2[i].Value) {
            Ꮡt.Fatalf("ints2 mismatch at %d; %v != %v"u8, i, ss[i].Value, ss2[i].Value);
        }
    }
}

public static void BenchmarkSortStructs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var ss = makeRandomStructs(N);
        b.StartTimer();
        Sort(ss);
    }
}

public static void BenchmarkSortFuncStructs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var cmpFunc = (ж<myStruct> a, ж<myStruct> bΔ1) => (~a).n - (~bΔ1).n;
    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var ss = makeRandomStructs(N);
        b.StartTimer();
        slices.SortFunc(ss, cmpFunc);
    }
}

} // end sort_test_package
