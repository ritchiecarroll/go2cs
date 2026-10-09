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

internal partial struct TestValueSeq_tests /*dyn*/ {
    internal @string name;
    internal reflectꓸValue val;
    internal Action<ж<Δtesting.T>, iter.Seq<reflectꓸValue>> check;
}

public static void TestValueSeq(ж<Δtesting.T> Ꮡt) {
    var m = new map<@string, nint>{
        ["1"u8] = 1,
        ["2"u8] = 2,
        ["3"u8] = 3,
        ["4"u8] = 4
    };
    var c = new channel<nint>(3);
    foreach (var i in range(3)) {
        c.ᐸꟷ(i);
    }
    close(c);
        var mʗ1 = m;





    var tests = new TestValueSeq_tests[]{
        new("int"u8, ValueOf((nint)(4)), (ж<Δtesting.T> tΔ1, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ1.Fatalf("got %d, want %d"u8, v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ1.Fatalf("should loop four times"u8);
            }
        }),
        new("int8"u8, ValueOf((int8)4), (ж<Δtesting.T> tΔ2, iter.Seq<reflectꓸValue> s) => {
            var i = (int8)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Interface()._<int8>() != i) {
                    tΔ2.Fatalf("got %d, want %d"u8, v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ2.Fatalf("should loop four times"u8);
            }
        }),
        new("uint"u8, ValueOf((uint64)4), (ж<Δtesting.T> tΔ3, iter.Seq<reflectꓸValue> s) => {
            var i = (uint64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Uint() != i) {
                    tΔ3.Fatalf("got %d, want %d"u8, v.Uint(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ3.Fatalf("should loop four times"u8);
            }
        }),
        new("uint8"u8, ValueOf((uint8)4), (ж<Δtesting.T> tΔ4, iter.Seq<reflectꓸValue> s) => {
            var i = (uint8)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Interface()._<uint8>() != i) {
                    tΔ4.Fatalf("got %d, want %d"u8, v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ4.Fatalf("should loop four times"u8);
            }
        }),
        new("*[4]int"u8, ValueOf(Ꮡ(new nint[]{1, 2, 3, 4}.array())), (ж<Δtesting.T> tΔ5, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ5.Fatalf("got %d, want %d"u8, v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ5.Fatalf("should loop four times"u8);
            }
        }),
        new("[4]int"u8, ValueOf(new nint[]{1, 2, 3, 4}.array()), (ж<Δtesting.T> tΔ6, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ6.Fatalf("got %d, want %d"u8, v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ6.Fatalf("should loop four times"u8);
            }
        }),
        new("[]int"u8, ValueOf(new nint[]{1, 2, 3, 4}.slice()), (ж<Δtesting.T> tΔ7, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ7.Fatalf("got %d, want %d"u8, v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ7.Fatalf("should loop four times"u8);
            }
        }),
        new("string"u8, ValueOf((@string)"12语言"u8), (ж<Δtesting.T> tΔ8, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            var indexes = new int64[]{0, 1, 2, 5}.slice();
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != indexes[(nint)(i)]) {
                    tΔ8.Fatalf("got %d, want %d"u8, v.Int(), indexes[(nint)(i)]);
                }
                i++;
            }
            if (i != 4) {
                tΔ8.Fatalf("should loop four times"u8);
            }
        }),
        new("map[string]int"u8, ValueOf(m), (ж<Δtesting.T> tΔ9, iter.Seq<reflectꓸValue> s) => {
            var copy = maps.Clone<map<@string, nint>, @string, nint>(mʗ1);
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                {
                    var (_, ok) = copy[v.String(), ꟷ]; if (!ok) {
                        tΔ9.Fatalf("unexpected %v"u8, v.Interface());
                    }
                }
                delete(copy, v.String());
            }
            if (len(copy) != 0) {
                tΔ9.Fatalf("should loop four times"u8);
            }
        }),
        new("chan int"u8, ValueOf(c), (ж<Δtesting.T> tΔ10, iter.Seq<reflectꓸValue> s) => {
            nint i = 0;
            var mΔ1 = new map<int64, bool>{
                [0] = false,
                [1] = false,
                [2] = false
            };
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                {
                    var (b, ok) = mΔ1[v.Int(), ꟷ]; if (!ok || b) {
                        tΔ10.Fatalf("unexpected %v"u8, v.Interface());
                    }
                }
                mΔ1[v.Int()] = true;
                i++;
            }
            if (i != 3) {
                tΔ10.Fatalf("should loop three times"u8);
            }
        }),
        new("func"u8, ValueOf((Func<nint, bool> yield) => {
            foreach (var i in range(4)) {
                if (!yield(i)) {
                    return;
                }
            }
        }), (ж<Δtesting.T> tΔ11, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ11.Fatalf("got %d, want %d"u8, v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ11.Fatalf("should loop four times"u8);
            }
        }),
        new("method"u8, ValueOf(new methodIter(nil)).Method(0), (ж<Δtesting.T> tΔ12, iter.Seq<reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != i) {
                    tΔ12.Fatalf("got %d, want %d"u8, v.Int(), i);
                }
                i++;
            }
            if (i != 4) {
                tΔ12.Fatalf("should loop four times"u8);
            }
        }),
        new("type N int8"u8, ValueOf(((N)4)), (ж<Δtesting.T> tΔ13, iter.Seq<reflectꓸValue> s) => {
            var i = ((N)0);
            foreach (var v in range<reflectꓸValue>(s.Invoke)) {
                if (v.Int() != (int64)(int8)i) {
                    tΔ13.Fatalf("got %d, want %d"u8, v.Int(), i);
                }
                i++;
                if (!AreEqual(v.Type(), Δreflect.TypeOf(i))) {
                    tΔ13.Fatalf("got %s, want %s"u8, v.Type(), Δreflect.TypeOf(i));
                }
            }
            if (i != 4) {
                tΔ13.Fatalf("should loop four times"u8);
            }
        })
    }.slice();
    foreach (var (_, tc) in tests) {
        var seq = tc.val.Seq();
        tc.check(Ꮡt, seq);
    }
}

internal partial struct TestValueSeq2_tests /*dyn*/ {
    internal @string name;
    internal reflectꓸValue val;
    internal Action<ж<Δtesting.T>, iter.Seq2<reflectꓸValue, reflectꓸValue>> check;
}

public static void TestValueSeq2(ж<Δtesting.T> Ꮡt) {
    var m = new map<@string, nint>{
        ["1"u8] = 1,
        ["2"u8] = 2,
        ["3"u8] = 3,
        ["4"u8] = 4
    };
        var mʗ1 = m;





    var tests = new TestValueSeq2_tests[]{
        new("*[4]int"u8, ValueOf(Ꮡ(new nint[]{1, 2, 3, 4}.array())), (ж<Δtesting.T> tΔ1, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ1.Fatalf("got %d, want %d"u8, v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ1.Fatalf("got %d, want %d"u8, v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ1.Fatalf("should loop four times"u8);
            }
        }),
        new("[4]int"u8, ValueOf(new nint[]{1, 2, 3, 4}.array()), (ж<Δtesting.T> tΔ2, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ2.Fatalf("got %d, want %d"u8, v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ2.Fatalf("got %d, want %d"u8, v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ2.Fatalf("should loop four times"u8);
            }
        }),
        new("[]int"u8, ValueOf(new nint[]{1, 2, 3, 4}.slice()), (ж<Δtesting.T> tΔ3, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ3.Fatalf("got %d, want %d"u8, v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ3.Fatalf("got %d, want %d"u8, v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ3.Fatalf("should loop four times"u8);
            }
        }),
        new("string"u8, ValueOf((@string)"12语言"u8), (ж<Δtesting.T> tΔ4, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            GoFrame ᒐ = default;
            try {
                var (next, stop) = iter.Pull2(s);
                var stopʗ1 = stop;
                defer(stopʗ1, ref ᒐ);
                var i = (int64)0;
                foreach (var (j, sΔ1) in (@string)"12语言"u8) {
                    var (v1, v2, ok) = next();
                    if (!ok) {
                        tΔ4.Fatalf("should loop four times"u8);
                    }
                    if (v1.Int() != (int64)j) {
                        tΔ4.Fatalf("got %d, want %d"u8, v1.Int(), j);
                    }
                    if (!AreEqual(v2.Interface(), sΔ1)) {
                        tΔ4.Fatalf("got %v, want %v"u8, v2.Interface(), sΔ1);
                    }
                    i++;
                }
                if (i != 4) {
                    tΔ4.Fatalf("should loop four times"u8);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }),
        new("map[string]int"u8, ValueOf(m), (ж<Δtesting.T> tΔ5, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var copy = maps.Clone<map<@string, nint>, @string, nint>(mʗ1);
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                var (v, ok) = copy[v1.String(), ꟷ];
                if (!ok) {
                    tΔ5.Fatalf("unexpected %v"u8, v1.String());
                }
                if (!AreEqual(v, v2.Interface())) {
                    tΔ5.Fatalf("got %v, want %d"u8, v2.Interface(), v);
                }
                delete(copy, v1.String());
            }
            if (len(copy) != 0) {
                tΔ5.Fatalf("should loop four times"u8);
            }
        }),
        new("func"u8, ValueOf((Func<nint, nint, bool> f) => {
            foreach (var i in range(4)) {
                f(i, i + 1);
            }
        }), (ж<Δtesting.T> tΔ6, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ6.Fatalf("got %d, want %d"u8, v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ6.Fatalf("got %d, want %d"u8, v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ6.Fatalf("should loop four times"u8);
            }
        }),
        new("method"u8, ValueOf(new methodIter2(nil)).Method(0), (ж<Δtesting.T> tΔ7, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = (int64)0;
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != i) {
                    tΔ7.Fatalf("got %d, want %d"u8, v1.Int(), i);
                }
                i++;
                if (v2.Int() != i) {
                    tΔ7.Fatalf("got %d, want %d"u8, v2.Int(), i);
                }
            }
            if (i != 4) {
                tΔ7.Fatalf("should loop four times"u8);
            }
        }),
        new("[4]N"u8, ValueOf(new N[]{0, 1, 2, 3}.array()), (ж<Δtesting.T> tΔ8, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = ((N)0);
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != (int64)(int8)i) {
                    tΔ8.Fatalf("got %d, want %d"u8, v1.Int(), i);
                }
                if (v2.Int() != (int64)(int8)i) {
                    tΔ8.Fatalf("got %d, want %d"u8, v2.Int(), i);
                }
                i++;
                if (!AreEqual(v2.Type(), Δreflect.TypeOf(i))) {
                    tΔ8.Fatalf("got %s, want %s"u8, v2.Type(), Δreflect.TypeOf(i));
                }
            }
            if (i != 4) {
                tΔ8.Fatalf("should loop four times"u8);
            }
        }),
        new("[]N"u8, ValueOf(new N[]{1, 2, 3, 4}.slice()), (ж<Δtesting.T> tΔ9, iter.Seq2<reflectꓸValue, reflectꓸValue> s) => {
            var i = ((N)0);
            foreach (var (v1, v2) in range<reflectꓸValue, reflectꓸValue>(s.Invoke)) {
                if (v1.Int() != (int64)(int8)i) {
                    tΔ9.Fatalf("got %d, want %d"u8, v1.Int(), i);
                }
                i++;
                if (v2.Int() != (int64)(int8)i) {
                    tΔ9.Fatalf("got %d, want %d"u8, v2.Int(), i);
                }
                if (!AreEqual(v2.Type(), Δreflect.TypeOf(i))) {
                    tΔ9.Fatalf("got %s, want %s"u8, v2.Type(), Δreflect.TypeOf(i));
                }
            }
            if (i != 4) {
                tΔ9.Fatalf("should loop four times"u8);
            }
        })
    }.slice();
    foreach (var (_, tc) in tests) {
        var seq = tc.val.Seq2();
        tc.check(Ꮡt, seq);
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

} // end reflect_test_package
