// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using iter = iter_package;
using rand = go.math.rand.rand_package;
using static slices_package;
using testing = testing_package;
using go.math.rand;

partial class slices_test_package {

public static void TestAll(ж<testing.T> Ꮡt) {
    for (nint size = 0; size < 10; size++) {
        slice<nint> s = default!;
        foreach (var i in range(size)) {
            s = append(s, i);
        }
        nint ei = 0;
        nint ev = 0;
        nint cnt = 0;
        foreach (var (i, v) in range<nint, nint>(All<slice<nint>, nint>(s).Invoke)) {
            if (i != ei || v != ev) {
                Ꮡt.Errorf("at iteration %d got %d, %d want %d, %d"u8, cnt, i, v, ei, ev);
            }
            ei++;
            ev++;
            cnt++;
        }
        if (cnt != size) {
            Ꮡt.Errorf("read %d values expected %d"u8, cnt, size);
        }
    }
}

public static void TestBackward(ж<testing.T> Ꮡt) {
    for (nint size = 0; size < 10; size++) {
        slice<nint> s = default!;
        foreach (var i in range(size)) {
            s = append(s, i);
        }
        nint ei = size - 1;
        nint ev = size - 1;
        nint cnt = 0;
        foreach (var (i, v) in range<nint, nint>(Backward<slice<nint>, nint>(s).Invoke)) {
            if (i != ei || v != ev) {
                Ꮡt.Errorf("at iteration %d got %d, %d want %d, %d"u8, cnt, i, v, ei, ev);
            }
            ei--;
            ev--;
            cnt++;
        }
        if (cnt != size) {
            Ꮡt.Errorf("read %d values expected %d"u8, cnt, size);
        }
    }
}

public static void TestValues(ж<testing.T> Ꮡt) {
    for (nint size = 0; size < 10; size++) {
        slice<nint> s = default!;
        foreach (var i in range(size)) {
            s = append(s, i);
        }
        nint ev = 0;
        nint cnt = 0;
        foreach (var v in range<nint>(Values<slice<nint>, nint>(s).Invoke)) {
            if (v != ev) {
                Ꮡt.Errorf("at iteration %d got %d want %d"u8, cnt, v, ev);
            }
            ev++;
            cnt++;
        }
        if (cnt != size) {
            Ꮡt.Errorf("read %d values expected %d"u8, cnt, size);
        }
    }
}

internal static void testSeq(Func<nint, bool> yield) {
    for (nint i = 0; i < 10; i += 2) {
        if (!yield(i)) {
            return;
        }
    }
}

internal static slice<nint> testSeqResult = new nint[]{0, 2, 4, 6, 8}.slice();

public static void TestAppendSeq(ж<testing.T> Ꮡt) {
    var s = AppendSeq<slice<nint>, nint>(new nint[]{1, 2}.slice(), new iter.Seq<nint>(testSeq));
    var want = appendꓸꓸꓸ(new nint[]{1, 2}.slice(), testSeqResult);
    if (!Equal<slice<nint>, nint>(s, want)) {
        Ꮡt.Errorf("got %v, want %v"u8, s, want);
    }
}

public static void TestCollect(ж<testing.T> Ꮡt) {
    var s = Collect<nint>(new iter.Seq<nint>(testSeq));
    var want = testSeqResult;
    if (!Equal<slice<nint>, nint>(s, want)) {
        Ꮡt.Errorf("got %v, want %v"u8, s, want);
    }
}

internal static slice<slice<@string>> iterTests;
internal static void initᴛiterTests() { iterTests = new slice<@string>[]{
    default!,
    new @string[]{"a"u8}.slice(),
    new @string[]{"a"u8, "b"u8}.slice(),
    new @string[]{"b"u8, "a"u8}.slice(),
    strs[..]
}.slice(); }

public static void TestValuesAppendSeq(ж<testing.T> Ꮡt) {
    foreach (var (_, prefix) in iterTests) {
        foreach (var (_, s) in iterTests) {
            var got = AppendSeq(prefix, Values<slice<@string>, @string>(s));
            var want = appendꓸꓸꓸ(prefix, s);
            if (!Equal<slice<@string>, @string>(got, want)) {
                Ꮡt.Errorf("AppendSeq(%v, Values(%v)) == %v, want %v"u8, prefix, s, got, want);
            }
        }
    }
}

public static void TestValuesCollect(ж<testing.T> Ꮡt) {
    foreach (var (_, s) in iterTests) {
        var got = Collect(Values<slice<@string>, @string>(s));
        if (!Equal<slice<@string>, @string>(got, s)) {
            Ꮡt.Errorf("Collect(Values(%v)) == %v, want %v"u8, s, got, s);
        }
    }
}

public static void TestSorted(ж<testing.T> Ꮡt) {
    var s = Sorted(Values<slice<nint>, nint>(ints[..]));
    if (!IsSorted<slice<nint>, nint>(s)) {
        Ꮡt.Errorf("sorted %v"u8, ints);
        Ꮡt.Errorf("   got %v"u8, s);
    }
}

public static void TestSortedFunc(ж<testing.T> Ꮡt) {
    var s = SortedFunc(Values<slice<nint>, nint>(ints[..]), (nint a, nint b) => a - b);
    if (!IsSorted<slice<nint>, nint>(s)) {
        Ꮡt.Errorf("sorted %v"u8, ints);
        Ꮡt.Errorf("   got %v"u8, s);
    }
}

public static void TestSortedStableFunc(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    nint n = 1000;
    nint m = 100;
    var data = new intPairs(n);
    foreach (var (i, _) in data) {
        data[i].a = rand.IntN(m);
    }
    data.initB();
    var s = ((intPairs)SortedStableFunc<intPair>(Values<intPairs, intPair>(data), intPairCmp));
    if (!IsSortedFunc<intPairs, intPair>(s, intPairCmp)) {
        Ꮡt.Errorf("SortedStableFunc didn't sort %d ints"u8, n);
    }
    if (!s.inOrder(false)) {
        Ꮡt.Errorf("SortedStableFunc wasn't stable on %d ints"u8, n);
    }
    // iterVal converts a Seq2 to a Seq.
    iter.Seq<intPair> iterVal(iter.Seq2<nint, intPair> seq) => (iter.Seq<intPair>)((Func<intPair, bool> yield) => {
            foreach (var (_, v) in range<nint, intPair>(seq.Invoke)) {
                if (!yield(v)) {
                    return;
                }
            }
        });
    s = ((intPairs)SortedStableFunc<intPair>(iterVal(Backward<intPairs, intPair>(data)), intPairCmp));
    if (!IsSortedFunc<intPairs, intPair>(s, intPairCmp)) {
        Ꮡt.Errorf("SortedStableFunc didn't sort %d reverse ints"u8, n);
    }
    if (!s.inOrder(true)) {
        Ꮡt.Errorf("SortedStableFunc wasn't stable on %d reverse ints"u8, n);
    }
}

internal partial struct TestChunk_cases /*dyn*/ {
    internal @string name;
    internal slice<nint> s;
    internal nint n;
    internal slice<slice<nint>> chunks;
}

public static void TestChunk(ж<testing.T> Ꮡt) {
    var cases = new TestChunk_cases[]{
        new(
            name: "nil"u8,
            s: default!,
            n: 1,
            chunks: default!
        ),
        new(
            name: "empty"u8,
            s: new nint[]{}.slice(),
            n: 1,
            chunks: default!
        ),
        new(
            name: "short"u8,
            s: new nint[]{1, 2}.slice(),
            n: 3,
            chunks: new slice<nint>[]{new nint[]{1, 2}.slice()}.slice()
        ),
        new(
            name: "one"u8,
            s: new nint[]{1, 2}.slice(),
            n: 2,
            chunks: new slice<nint>[]{new nint[]{1, 2}.slice()}.slice()
        ),
        new(
            name: "even"u8,
            s: new nint[]{1, 2, 3, 4}.slice(),
            n: 2,
            chunks: new slice<nint>[]{new nint[]{1, 2}.slice(), new nint[]{3, 4}.slice()}.slice()
        ),
        new(
            name: "odd"u8,
            s: new nint[]{1, 2, 3, 4, 5}.slice(),
            n: 2,
            chunks: new slice<nint>[]{new nint[]{1, 2}.slice(), new nint[]{3, 4}.slice(), new nint[]{5}.slice()}.slice()
        )
    }.slice();
    foreach (var (_, vᴛ1) in cases) {
        ref var tc = ref heap(new TestChunk_cases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            slice<slice<nint>> chunks = default!;
            foreach (var c in range<slice<nint>>(Chunk<slice<nint>, nint>(tcʗ1.s, tcʗ1.n).Invoke)) {
                chunks = append(chunks, c);
            }
            if (!chunkEqual<slice<nint>, nint>(chunks, tcʗ1.chunks)) {
                tΔ1.Errorf("Chunk(%v, %d) = %v, want %v"u8, tcʗ1.s, tcʗ1.n, chunks, tcʗ1.chunks);
            }
            if (len(chunks) == 0) {
                return;
            }
            // Verify that appending to the end of the first chunk does not
            // clobber the beginning of the next chunk.
            var s = Clone<slice<nint>, nint>(tcʗ1.s);
            chunks[0] = append(chunks[0], (nint)(-1));
            if (!Equal<slice<nint>, nint>(s, tcʗ1.s)) {
                tΔ1.Errorf("slice was clobbered: %v, want %v"u8, s, tcʗ1.s);
            }
        });
    }
}

internal partial struct TestChunkPanics_type /*dyn*/ {
    internal @string name;
    internal slice<EmptyStruct> x;
    internal nint n;
}

public static void TestChunkPanics(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestChunkPanics_type[]{
        new(
            name: "cannot be less than 1"u8,
            x: new slice<EmptyStruct>(0),
            n: 0
        )
    }.slice()) {
        ref var test = ref heap(new TestChunkPanics_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        if (!panics(() => {
            _ = Chunk<slice<EmptyStruct>, EmptyStruct>(testʗ1.x, testʗ1.n);
        })) {
            Ꮡt.Errorf("Chunk %s: got no panic, want panic"u8, test.name);
        }
    }
}

public static void TestChunkRange(ж<testing.T> Ꮡt) {
    // Verify Chunk iteration can be stopped.
    slice<slice<nint>> got = default!;
    foreach (var c in range<slice<nint>>(Chunk<slice<nint>, nint>(new nint[]{1, 2, 3, 4, -100}.slice(), 2).Invoke)) {
        if (len(got) == 2) {
            // Found enough values, break early.
            break;
        }
        got = append(got, c);
    }
    {
        var want = new slice<nint>[]{new nint[]{1, 2}.slice(), new nint[]{3, 4}.slice()}.slice(); if (!chunkEqual<slice<nint>, nint>(got, want)) {
            Ꮡt.Errorf("Chunk iteration did not stop, got %v, want %v"u8, got, want);
        }
    }
}

internal static bool chunkEqual<Slice, E>(slice<Slice> s1, slice<Slice> s2)
    where Slice : /* ~[]E */ ISlice<E>, ISupportMake<Slice>, ISliceWrap<Slice, E>, new()
{
    return EqualFunc<slice<Slice>, slice<Slice>, Slice, Slice>(s1, s2, Equal<Slice, E>);
}

} // end slices_test_package
