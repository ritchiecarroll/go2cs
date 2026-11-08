// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using iter = iter_package;
using maps = maps_package;
using Δreflect = reflect_package;
using static reflect_package;
using Δtesting = testing_package;
using static global::go.reflect_internal_test_package;

partial class reflect_test_package {

partial struct N /*num:int8*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string seqˢ => ref seqˢᵇ;
private static readonly @string seqˢᵇ = "Seq"u8;

internal partial struct TestValueSeq_tests /*dyn*/ {
    internal @string name;
    internal reflectꓸValue val;
    internal Action<ж<Δtesting.T>, iter.Seq<reflectꓸValue>> check;
}

public static void TestValueSeq(ж<Δtesting.T> t) {
    var m = new map<@string, nint>{
        ["1"] = 1,
        ["2"] = 2,
        ["3"] = 3,
        ["4"] = 4
    };
    var c = new channel<nint>(3);
    foreach (var i in range(3)) {
        c.ᐸꟷ(i);
    }
    close(c);
        var mʗ1 = m;





    var tests = slice<TestValueSeq_tests>([
        new("int", ValueOf((nint)4), (ж<Δtesting.T> tΔ1, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ1.Fatalf("got %d, want %d", v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ1.Fatalf("should loop four times");
            }
        }),
        new("int8", ValueOf((int8)4), (ж<Δtesting.T> tΔ2, iter.Seq<reflectꓸValue> s) => {
            var i = (int8)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Interface()._<int8>() != i) {
                    tΔ2.Fatalf("got %d, want %d", v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ2.Fatalf("should loop four times");
            }
        }),
        new("uint", ValueOf((uint64)4), (ж<Δtesting.T> tΔ3, iter.Seq<reflectꓸValue> s) => {
            var i = (uint64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Uint() != i) {
                    tΔ3.Fatalf("got %d, want %d", v.Uint(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ3.Fatalf("should loop four times");
            }
        }),
        new("uint8", ValueOf((uint8)4), (ж<Δtesting.T> tΔ4, iter.Seq<reflectꓸValue> s) => {
            var i = (uint8)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Interface()._<uint8>() != i) {
                    tΔ4.Fatalf("got %d, want %d", v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ4.Fatalf("should loop four times");
            }
        }),
        new("*[4]int", ValueOf(Ꮡ(new nint[]{1, 2, 3, 4}.array())), (ж<Δtesting.T> tΔ5, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ5.Fatalf("got %d, want %d", v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ5.Fatalf("should loop four times");
            }
        }),
        new("[4]int", ValueOf(new nint[]{1, 2, 3, 4}.array()), (ж<Δtesting.T> tΔ6, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ6.Fatalf("got %d, want %d", v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ6.Fatalf("should loop four times");
            }
        }),
        new("[]int", ValueOf(slice<nint>([1, 2, 3, 4])), (ж<Δtesting.T> tΔ7, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ7.Fatalf("got %d, want %d", v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ7.Fatalf("should loop four times");
            }
        }),
        new("string", ValueOf("12语言"), (ж<Δtesting.T> tΔ8, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            var indexes = slice<int64>([0, 1, 2, 5]);
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != indexes[(nint)(i)]) {
                    tΔ8.Fatalf("got %d, want %d", v.Int(), indexes[(nint)(i)]);
                }
                i++;
            }
            if (i != 4) {
                tΔ8.Fatalf("should loop four times");
            }
        }),
        new("map[string]int", ValueOf(m), (ж<Δtesting.T> tΔ9, iter.Seq<reflectꓸValue> s) => {
            var copy = maps.Clone<map<@string, nint>, @string, nint>(mʗ1);
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                var (_, ok) = copy[v.String(), ꟷ]; if (!ok) {
                    tΔ9.Fatalf("unexpected %v", v.Interface());
                }
                delete(copy, v.String());
            }
            if (len(copy) != 0) {
                tΔ9.Fatalf("should loop four times");
            }
        }),
        new("chan int", ValueOf(c), (ж<Δtesting.T> tΔ10, iter.Seq<reflectꓸValue> s) => {
            nint i = 0;
            var mΔ1 = new map<int64, bool>{
                [0] = false,
                [1] = false,
                [2] = false
            };
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                var (b, ok) = mΔ1[v.Int(), ꟷ]; if (!ok || b) {
                    tΔ10.Fatalf("unexpected %v", v.Interface());
                }
                mΔ1[v.Int()] = true;
                i++;
            }
            if (i != 3) {
                tΔ10.Fatalf("should loop three times");
            }
        }),
        new("func", ValueOf((Func<nint, bool> yield) => {
            foreach (var i in range(4)) {
                if (!yield(i)) {
                    return;
                }
            }
        }), (ж<Δtesting.T> tΔ11, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ11.Fatalf("got %d, want %d", v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ11.Fatalf("should loop four times");
            }
        }),
        new("method", ValueOf(new methodIter()).MethodByName(seqˢ), (ж<Δtesting.T> tΔ12, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ12.Fatalf("got %d, want %d", v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ12.Fatalf("should loop four times");
            }
        }),
        new("type N int8", ValueOf((N)4), (ж<Δtesting.T> tΔ13, iter.Seq<reflectꓸValue> s) => {
            var i = (N)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != (int64)i) {
                    tΔ13.Fatalf("got %d, want %d", v.Int(), i);
                }
                i++;
                if (!AreEqual(v.Type(), Δreflect.TypeOf(i))) {
                    tΔ13.Fatalf("got %s, want %s", v.Type(), Δreflect.TypeOf(i));
                }
            }
            if (i != 4) {
                tΔ13.Fatalf("should loop four times");
            }
        })
    ]);
    foreach (var (_, tc) in range(tests)) {
        var seq = tc.val.Seq();
        tc.check(t, seq);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string seq2ˢ => ref seq2ˢᵇ;
private static readonly @string seq2ˢᵇ = "Seq2"u8;

internal partial struct TestValueSeq2_tests /*dyn*/ {
    internal @string name;
    internal reflectꓸValue val;
    internal Action<ж<Δtesting.T>, iter.Seq2<reflectꓸValue, reflectꓸValue>> check;
}

public static void TestValueSeq2(ж<Δtesting.T> t) {
    var m = new map<@string, nint>{
        ["1"] = 1,
        ["2"] = 2,
        ["3"] = 3,
        ["4"] = 4
    };
        var mʗ1 = m;





    var tests = slice<TestValueSeq2_tests>([
        new("*[4]int", ValueOf(Ꮡ(new nint[]{1, 2, 3, 4}.array())), (ж<Δtesting.T> tΔ1, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ1.Fatalf("got %d, want %d", v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ1.Fatalf("got %d, want %d", v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ1.Fatalf("should loop four times");
            }
        }),
        new("[4]int", ValueOf(new nint[]{1, 2, 3, 4}.array()), (ж<Δtesting.T> tΔ2, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ2.Fatalf("got %d, want %d", v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ2.Fatalf("got %d, want %d", v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ2.Fatalf("should loop four times");
            }
        }),
        new("[]int", ValueOf(slice<nint>([1, 2, 3, 4])), (ж<Δtesting.T> tΔ3, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ3.Fatalf("got %d, want %d", v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ3.Fatalf("got %d, want %d", v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ3.Fatalf("should loop four times");
            }
        }),
        new("string", ValueOf("12语言"), (ж<Δtesting.T> tΔ4, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            GoFrame ᒐ = default;
            try {
                var (next, stop) = iter.Pull2(s);
                var stopʗ1 = stop;
                defer(stopʗ1, ref ᒐ);
                var i = (int64)0;
                foreach (var (j, sΔ1) in (@string)"12语言") {
                    var (v1, v2, ok) = next();
                    if (!ok) {
                        tΔ4.Fatalf("should loop four times");
                    }
                    if (v1.Int() != (int64)j) {
                        tΔ4.Fatalf("got %d, want %d", v1.Int(), j);
                    }
                    if (!AreEqual(v2.Interface(), sΔ1)) {
                        tΔ4.Fatalf("got %v, want %v", v2.Interface(), sΔ1);
                    }
                    i++;
                }
                if (i != 4) {
                    tΔ4.Fatalf("should loop four times");
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }),
        new("map[string]int", ValueOf(m), (ж<Δtesting.T> tΔ5, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var copy = maps.Clone<map<@string, nint>, @string, nint>(mʗ1);
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                var (v, ok) = copy[v1.String(), ꟷ];
                if (!ok) {
                    tΔ5.Fatalf("unexpected %v", v1.String());
                }
                if (!AreEqual(v, v2.Interface())) {
                    tΔ5.Fatalf("got %v, want %d", v2.Interface(), v);
                }
                delete(copy, v1.String());
            }
            if (len(copy) != 0) {
                tΔ5.Fatalf("should loop four times");
            }
        }),
        new("func", ValueOf((Func<nint, nint, bool> f) => {
            foreach (var i in range(4)) {
                f(i, i + 1);
            }
        }), (ж<Δtesting.T> tΔ6, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ6.Fatalf("got %d, want %d", v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ6.Fatalf("got %d, want %d", v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ6.Fatalf("should loop four times");
            }
        }),
        new("method", ValueOf(new methodIter2()).MethodByName(seq2ˢ), (ж<Δtesting.T> tΔ7, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ7.Fatalf("got %d, want %d", v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ7.Fatalf("got %d, want %d", v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ7.Fatalf("should loop four times");
            }
        }),
        new("[4]N", ValueOf(new N[]{0, 1, 2, 3}.array()), (ж<Δtesting.T> tΔ8, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (N)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != (int64)i) {
                    tΔ8.Fatalf("got %d, want %d", v1.Int(), i);
                }
                if (v2.Int() != (int64)i) {
                    tΔ8.Fatalf("got %d, want %d", v2.Int(), i);
                }
                i++;
                if (!AreEqual(v2.Type(), Δreflect.TypeOf(i))) {
                    tΔ8.Fatalf("got %s, want %s", v2.Type(), Δreflect.TypeOf(i));
                }
            }
            if (i != 4) {
                tΔ8.Fatalf("should loop four times");
            }
        }),
        new("[]N", ValueOf(slice<N>([1, 2, 3, 4])), (ж<Δtesting.T> tΔ9, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (N)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != (int64)i) {
                    tΔ9.Fatalf("got %d, want %d", v1.Int(), i);
                }
                i++;
                if (v2.Int() != (int64)i) {
                    tΔ9.Fatalf("got %d, want %d", v2.Int(), i);
                }
                if (!AreEqual(v2.Type(), Δreflect.TypeOf(i))) {
                    tΔ9.Fatalf("got %s, want %s", v2.Type(), Δreflect.TypeOf(i));
                }
            }
            if (i != 4) {
                tΔ9.Fatalf("should loop four times");
            }
        })
    ]);
    foreach (var (_, tc) in range(tests)) {
        var seq = tc.val.Seq2();
        tc.check(t, seq);
    }
}

// methodIter is a type from which we can derive a method
// value that is an iter.Seq.
partial struct methodIter {
}

internal static void Seq(this methodIter _, Func<nint, bool> yield) {
    foreach (var i in range(4)) {
        if (!yield(i)) {
            return;
        }
    }
}

// For Type.CanSeq test.
internal static void NonSeq(this methodIter _, Action<nint> yield) { }

// methodIter2 is a type from which we can derive a method
// value that is an iter.Seq2.
partial struct methodIter2 {
}

internal static void Seq2(this methodIter2 _, Func<nint, nint, bool> yield) {
    foreach (var i in range(4)) {
        if (!yield(i, i + 1)) {
            return;
        }
    }
}

// For Type.CanSeq2 test.
internal static void NonSeq2(this methodIter2 _, Action<nint, nint> yield) { }

} // end reflect_test_package
