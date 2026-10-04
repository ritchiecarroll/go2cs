// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using sha1 = crypto.sha1_package;
using json = encoding.json_package;
using fmt = fmt_package;
using Δnet = net_package;
using httptest = go.net.http.httptest_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto;
using encoding;
using go.net.http;
using go.sync;
using http = go.net.http_package;
using static go.expvar_package;
using time = time_package;

partial class expvar_internal_test_package {

// RemoveAll removes all exported variables.
// This is for tests only.
public static void RemoveAll() {
    GoFrame ᒐ = default;
    try {
        Ꮡvars.of(global::go.expvar_package.Map.ᏑkeysMu).Lock();
        defer(Ꮡvars.of(global::go.expvar_package.Map.ᏑkeysMu).Unlock, ref ᒐ);
        foreach (var (_, k) in vars.keys) {
            Ꮡvars.of(global::go.expvar_package.Map.Ꮡm).Delete(k);
        }
        vars.keys = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingˢ = "missing"u8;

public static void TestNil(ж<testing.T> Ꮡt) {
    RemoveAll();
    var val = Get(missingˢ);
    if (val != default!) {
        Ꮡt.Errorf("got %v, want nil"u8, val);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string requestsˢ = "requests"u8;

public static void TestInt(ж<testing.T> Ꮡt) {
    RemoveAll();
    var reqs = NewInt(requestsˢ);
    {
        var i = reqs.Value(); if (i != 0) {
            Ꮡt.Errorf("reqs.Value() = %v, want 0"u8, i);
        }
    }
    if (reqs != Get(requestsˢ)._<ж<global::go.expvar_package.Int>>()) {
        Ꮡt.Errorf("Get() failed."u8);
    }
    reqs.Add(1);
    reqs.Add(3);
    {
        var i = reqs.Value(); if (i != 4) {
            Ꮡt.Errorf("reqs.Value() = %v, want 4"u8, i);
        }
    }
    {
        @string s = reqs.String(); if (s != "4"u8) {
            Ꮡt.Errorf("reqs.String() = %q, want \"4\""u8, s);
        }
    }
    reqs.Set(-2);
    {
        var i = reqs.Value(); if (i != -2) {
            Ꮡt.Errorf("reqs.Value() = %v, want -2"u8, i);
        }
    }
}

public static void BenchmarkIntAdd(ж<testing.B> Ꮡb) {
    ref var v = ref heap(new global::go.expvar_package.Int(), out var Ꮡv);
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            Ꮡv.Add(1);
        }
    });
}

public static void BenchmarkIntSet(ж<testing.B> Ꮡb) {
    ref var v = ref heap(new global::go.expvar_package.Int(), out var Ꮡv);
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            Ꮡv.Set(1);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string requestsFloatˢ = "requests-float"u8;

public static void TestFloat(ж<testing.T> Ꮡt) {
    RemoveAll();
    var reqs = NewFloat(requestsFloatˢ);
    if (reqs.of(global::go.expvar_package.Float.Ꮡf).Load() != 0) {
        Ꮡt.Errorf("reqs.f = %v, want 0"u8, reqs.of(global::go.expvar_package.Float.Ꮡf).Load());
    }
    if (reqs != Get(requestsFloatˢ)._<ж<global::go.expvar_package.Float>>()) {
        Ꮡt.Errorf("Get() failed."u8);
    }
    reqs.Add(1.5D);
    reqs.Add(1.25D);
    {
        var v = reqs.Value(); if (v != 2.75D) {
            Ꮡt.Errorf("reqs.Value() = %v, want 2.75"u8, v);
        }
    }
    {
        @string s = reqs.String(); if (s != "2.75"u8) {
            Ꮡt.Errorf("reqs.String() = %q, want \"4.64\""u8, s);
        }
    }
    reqs.Add(-2D);
    {
        var v = reqs.Value(); if (v != 0.75D) {
            Ꮡt.Errorf("reqs.Value() = %v, want 0.75"u8, v);
        }
    }
}

public static void BenchmarkFloatAdd(ж<testing.B> Ꮡb) {
    ref var f = ref heap(new global::go.expvar_package.Float(), out var Ꮡf);
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            Ꮡf.Add(1.0D);
        }
    });
}

public static void BenchmarkFloatSet(ж<testing.B> Ꮡb) {
    ref var f = ref heap(new global::go.expvar_package.Float(), out var Ꮡf);
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            Ꮡf.Set(1.0D);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myNameˢ = "my-name"u8;
internal static readonly @string mikeˢ = "Mike"u8;
internal static readonly @string mikeˢ2 = @"""Mike"""u8;
internal static readonly @string u003cˢ = "\"\\u003c\""u8;

public static void TestString(ж<testing.T> Ꮡt) {
    RemoveAll();
    var name = NewString(myNameˢ);
    {
        @string s = name.Value(); if (s != ""u8) {
            Ꮡt.Errorf(@"NewString(""my-name"").Value() = %q, want """""u8, s);
        }
    }
    name.Set(mikeˢ);
    {
        @string s = name.String();
        @string want = mikeˢ2; if (s != want) {
            Ꮡt.Errorf(@"after name.Set(""Mike""), name.String() = %q, want %q"u8, s, want);
        }
    }
    {
        @string s = name.Value();
        @string want = mikeˢ; if (s != want) {
            Ꮡt.Errorf(@"after name.Set(""Mike""), name.Value() = %q, want %q"u8, s, want);
        }
    }
    // Make sure we produce safe JSON output.
    name.Set("<"u8);
    {
        @string s = name.String();
        @string want = u003cˢ; if (s != want) {
            Ꮡt.Errorf(@"after name.Set(""<""), name.String() = %q, want %q"u8, s, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string redˢ = "red"u8;

public static void BenchmarkStringSet(ж<testing.B> Ꮡb) {
    ref var s = ref heap(new global::go.expvar_package.ΔString(), out var Ꮡs);
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            Ꮡs.Set(redˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bikeShedColorsˢ = "bike-shed-colors"u8;
internal static readonly @string blueˢ = "blue"u8;
internal static readonly @string chartreuseˢ = "chartreuse"u8;

public static void TestMapInit(ж<testing.T> Ꮡt) {
    RemoveAll();
    var colors = NewMap(bikeShedColorsˢ);
    colors.Add(redˢ, 1);
    colors.Add(blueˢ, 1);
    colors.Add(chartreuseˢ, 1);
    nint n = 0;
    colors.Do((global::go.expvar_package.KeyValue _) => {
        n++;
    });
    if (n != 3) {
        Ꮡt.Errorf("after three Add calls with distinct keys, Do should invoke f 3 times; got %v"u8, n);
    }
    colors.Init();
    n = 0;
    colors.Do((global::go.expvar_package.KeyValue _) => {
        n++;
    });
    if (n != 0) {
        Ꮡt.Errorf("after Init, Do should invoke f 0 times; got %v"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notfoundˢ = "notfound"u8;

public static void TestMapDelete(ж<testing.T> Ꮡt) {
    RemoveAll();
    var colors = NewMap(bikeShedColorsˢ);
    colors.Add(redˢ, 1);
    colors.Add(redˢ, 2);
    colors.Add(blueˢ, 4);
    nint n = 0;
    colors.Do((global::go.expvar_package.KeyValue _) => {
        n++;
    });
    if (n != 2) {
        Ꮡt.Errorf("after two Add calls with distinct keys, Do should invoke f 2 times; got %v"u8, n);
    }
    colors.Delete(redˢ);
    {
        var v = colors.Get(redˢ); if (v != default!) {
            Ꮡt.Errorf("removed red, Get should return nil; got %v"u8, v);
        }
    }
    n = 0;
    colors.Do((global::go.expvar_package.KeyValue _) => {
        n++;
    });
    if (n != 1) {
        Ꮡt.Errorf("removed red, Do should invoke f 1 times; got %v"u8, n);
    }
    colors.Delete(notfoundˢ);
    n = 0;
    colors.Do((global::go.expvar_package.KeyValue _) => {
        n++;
    });
    if (n != 1) {
        Ꮡt.Errorf("attempted to remove notfound, Do should invoke f 1 times; got %v"u8, n);
    }
    colors.Delete(blueˢ);
    colors.Delete(blueˢ);
    {
        var v = colors.Get(blueˢ); if (v != default!) {
            Ꮡt.Errorf("removed blue, Get should return nil; got %v"u8, v);
        }
    }
    n = 0;
    colors.Do((global::go.expvar_package.KeyValue _) => {
        n++;
    });
    if (n != 0) {
        Ꮡt.Errorf("all keys removed, Do should invoke f 0 times; got %v"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string greenMidoriˢ = @"green ""midori"""u8;
internal static readonly object colorsStringDidnTProduceˢ = (@string)"colors.String() didn't produce a map."u8;
internal static readonly object redKindIsNotANumberˢ = (@string)"red.Kind() is not a number."u8;

public static void TestMapCounter(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    RemoveAll();
    var colors = NewMap(bikeShedColorsˢ);
    colors.Add(redˢ, 1);
    colors.Add(redˢ, 2);
    colors.Add(blueˢ, 4);
    colors.AddFloat(greenMidoriˢ, 4.125D);
    {
        var xΔ1 = colors.Get(redˢ)._<ж<global::go.expvar_package.Int>>().Value(); if (xΔ1 != 3) {
            Ꮡt.Errorf("colors.m[\"red\"] = %v, want 3"u8, xΔ1);
        }
    }
    {
        var xΔ2 = colors.Get(blueˢ)._<ж<global::go.expvar_package.Int>>().Value(); if (xΔ2 != 4) {
            Ꮡt.Errorf("colors.m[\"blue\"] = %v, want 4"u8, xΔ2);
        }
    }
    {
        var xΔ3 = colors.Get(greenMidoriˢ)._<ж<global::go.expvar_package.Float>>().Value(); if (xΔ3 != 4.125D) {
            Ꮡt.Errorf("colors.m[`green \"midori\"] = %v, want 4.125"u8, xΔ3);
        }
    }
    // colors.String() should be '{"red":3, "blue":4}',
    // though the order of red and blue could vary.
    @string s = colors.String();
    ref var j = ref heap<any>(out var Ꮡj);
    var err = json.Unmarshal(slice<byte>(s), Ꮡj);
    if (err != default!) {
        Ꮡt.Errorf("colors.String() isn't valid JSON: %v"u8, err);
    }
    var (m, ok) = j._<map<@string, any>>(ᐧ);
    if (!ok) {
        Ꮡt.Error(colorsStringDidnTProduceˢ);
    }
    var red = m[redˢ];
    (var x, ok) = red._<float64>(ᐧ);
    if (!ok) {
        Ꮡt.Error(redKindIsNotANumberˢ);
    }
    if (x != 3D) {
        Ꮡt.Errorf("red = %v, want 3"u8, x);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string issue527719ˢ = "issue527719"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string keyᶜ = "key"u8;

public static void TestMapNil(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    RemoveAll();
    @string key = keyᶜ;
    var m = NewMap(issue527719ˢ);
    m.Set(key, default!);
    @string s = m.String();
    ref var j = ref heap<any>(out var Ꮡj);
    {
        var err = json.Unmarshal(slice<byte>(s), Ꮡj); if (err != default!) {
            Ꮡt.Fatalf("m.String() == %q isn't valid JSON: %v"u8, s, err);
        }
    }
    var (m2, ok) = j._<map<@string, any>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("m.String() produced %T, wanted a map"u8, j);
    }
    (var v, ok) = m2[key, ꟷ];
    if (!ok) {
        Ꮡt.Fatalf("missing %q in %v"u8, key, m2);
    }
    if (v != default!) {
        Ꮡt.Fatalf("m[%q] = %v, want nil"u8, key, v);
    }
}

public static void BenchmarkMapSet(ж<testing.B> Ꮡb) {
    var m = @new<global::go.expvar_package.Map>().Init();
    var v = @new<global::go.expvar_package.Int>();
    var mʗ1 = m;
    var vʗ1 = v;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            mʗ1.Set(redˢ, new global::go.expvar_package.IntжVar(vʗ1));
        }
    });
}

public static void BenchmarkMapSetDifferent(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var procKeys = new slice<slice<@string>>(Δruntime.GOMAXPROCS(0));
    foreach (var (i, _) in procKeys) {
        var keys = new slice<@string>(4);
        foreach (var (j, _) in keys) {
            keys[j] = fmt.Sprint(i, j);
        }
        procKeys[i] = keys;
    }
    var m = @new<global::go.expvar_package.Map>().Init();
    var v = @new<global::go.expvar_package.Int>();
    b.ResetTimer();
    ref var n = ref heap(new int32(), out var Ꮡn);
    var mʗ1 = m;
    var procKeysʗ1 = procKeys;
    var vʗ1 = v;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        nint i = (nint)(atomic.AddInt32(Ꮡn, 1) - 1) % len(procKeysʗ1);
        var keys = procKeysʗ1[i];
        while (pb.Next()) {
            foreach (var (_, k) in keys) {
                mʗ1.Set(k, new global::go.expvar_package.IntжVar(vʗ1));
            }
        }
    });
}

// BenchmarkMapSetDifferentRandom simulates such a case where the concerned
// keys of Map.Set are generated dynamically and as a result insertion is
// out of order and the number of the keys may be large.
public static void BenchmarkMapSetDifferentRandom(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var keys = new slice<@string>(100);
    foreach (var (i, _) in keys) {
        keys[i] = fmt.Sprintf("%x"u8, sha1.Sum(slice<byte>(fmt.Sprint(i))));
    }
    var v = @new<global::go.expvar_package.Int>();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var m = @new<global::go.expvar_package.Map>().Init();
        foreach (var (_, k) in keys) {
            m.Set(k, new global::go.expvar_package.IntжVar(v));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloˢ = "Hello, !"u8;

public static void BenchmarkMapSetString(ж<testing.B> Ꮡb) {
    var m = @new<global::go.expvar_package.Map>().Init();
    var v = @new<global::go.expvar_package.ΔString>();
    v.Set(helloˢ);
    var mʗ1 = m;
    var vʗ1 = v;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            mʗ1.Set(redˢ, new expvar_internal_test_package.expvar_ΔStringжVar(vʗ1));
        }
    });
}

public static void BenchmarkMapAddSame(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            var m = @new<global::go.expvar_package.Map>().Init();
            m.Add(redˢ, 1);
            m.Add(redˢ, 1);
            m.Add(redˢ, 1);
            m.Add(redˢ, 1);
        }
    });
}

public static void BenchmarkMapAddDifferent(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var procKeys = new slice<slice<@string>>(Δruntime.GOMAXPROCS(0));
    foreach (var (i, _) in procKeys) {
        var keys = new slice<@string>(4);
        foreach (var (j, _) in keys) {
            keys[j] = fmt.Sprint(i, j);
        }
        procKeys[i] = keys;
    }
    b.ResetTimer();
    ref var n = ref heap(new int32(), out var Ꮡn);
    var procKeysʗ1 = procKeys;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        nint i = (nint)(atomic.AddInt32(Ꮡn, 1) - 1) % len(procKeysʗ1);
        var keys = procKeysʗ1[i];
        while (pb.Next()) {
            var m = @new<global::go.expvar_package.Map>().Init();
            foreach (var (_, k) in keys) {
                m.Add(k, 1);
            }
        }
    });
}

// BenchmarkMapAddDifferentRandom simulates such a case where that the concerned
// keys of Map.Add are generated dynamically and as a result insertion is out of
// order and the number of the keys may be large.
public static void BenchmarkMapAddDifferentRandom(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var keys = new slice<@string>(100);
    foreach (var (i, _) in keys) {
        keys[i] = fmt.Sprintf("%x"u8, sha1.Sum(slice<byte>(fmt.Sprint(i))));
    }
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var m = @new<global::go.expvar_package.Map>().Init();
        foreach (var (_, k) in keys) {
            m.Add(k, 1);
        }
    }
}

public static void BenchmarkMapAddSameSteadyState(ж<testing.B> Ꮡb) {
    var m = @new<global::go.expvar_package.Map>().Init();
    var mʗ1 = m;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            mʗ1.Add(redˢ, 1);
        }
    });
}

public static void BenchmarkMapAddDifferentSteadyState(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var procKeys = new slice<slice<@string>>(Δruntime.GOMAXPROCS(0));
    foreach (var (i, _) in procKeys) {
        var keys = new slice<@string>(4);
        foreach (var (j, _) in keys) {
            keys[j] = fmt.Sprint(i, j);
        }
        procKeys[i] = keys;
    }
    var m = @new<global::go.expvar_package.Map>().Init();
    b.ResetTimer();
    ref var n = ref heap(new int32(), out var Ꮡn);
    var mʗ1 = m;
    var procKeysʗ1 = procKeys;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        nint i = (nint)(atomic.AddInt32(Ꮡn, 1) - 1) % len(procKeysʗ1);
        var keys = procKeysʗ1[i];
        while (pb.Next()) {
            foreach (var (_, k) in keys) {
                mʗ1.Add(k, 1);
            }
        }
    });
}

public static void TestFunc(ж<testing.T> Ꮡt) {
    RemoveAll();
    ref var x = ref heap<any>(out var Ꮡx);

    x = new @string[]{"a"u8, "b"u8}.slice();
    var f = new global::go.expvar_package.Func(() => Ꮡx.ValueSlot);
    {
        @string s = f.String();
        @string exp = @"[""a"",""b""]"u8; if (s != exp) {
            Ꮡt.Errorf(@"f.String() = %q, want %q"u8, s, exp);
        }
    }
    {
        var v = f.Value(); if (!reflect.DeepEqual(v, x)) {
            Ꮡt.Errorf(@"f.Value() = %q, want %q"u8, v, x);
        }
    }
    x = (nint)(17);
    {
        @string s = f.String();
        @string exp = @"17"u8; if (s != exp) {
            Ꮡt.Errorf(@"f.String() = %q, want %q"u8, s, exp);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string map1ˢ = "map1"u8;
internal static readonly @string map2ˢ = "map2"u8;
internal static readonly @string map1A1Z2Map2001122334455ˢ = """
{
"map1": {"a": 1, "z": 2},
"map2": {"0": 0, "1": 1, "2": 2, "3": 3, "4": 4, "5": 5, "6": 6, "7": 7, "8": 8}
}

"""u8;

public static void TestHandler(ж<testing.T> Ꮡt) {
    RemoveAll();
    var m = NewMap(map1ˢ);
    m.Add("a"u8, 1);
    m.Add("z"u8, 2);
    var m2 = NewMap(map2ˢ);
    for (nint i = 0; i < 9; i++) {
        m2.Add(strconv.Itoa(i), (int64)i);
    }
    var rr = httptest.NewRecorder();
    rr.Value.Body = @new<bytes.Buffer>();
    expvarHandler(new expvar_internal_test_package.httptest_ResponseRecorderжResponseWriter(rr), nil);
    @string want = map1A1Z2Map2001122334455ˢ;
    {
        @string got = (~rr).Body.String(); if (got != want) {
            Ꮡt.Errorf("HTTP handler wrote:\n%s\nWant:\n%s"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string str1ˢ = "str1"u8;
internal static readonly @string helloWorldˢ = "hello, world!"u8;
internal static readonly @string str2ˢ = "str2"u8;
internal static readonly @string fizzBuzzˢ = "fizz buzz"u8;

public static void BenchmarkMapString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var m = ref heap(new global::go.expvar_package.Map(), out var Ꮡm);
    ref var m1 = ref heap(new global::go.expvar_package.Map(), out var Ꮡm1);
    ref var m2 = ref heap(new global::go.expvar_package.Map(), out var Ꮡm2);
    Ꮡm.Set(map1ˢ, new global::go.expvar_package.MapжVar(Ꮡm1));
    Ꮡm1.Add("a"u8, 1);
    Ꮡm1.Add("z"u8, 2);
    Ꮡm.Set(map2ˢ, new global::go.expvar_package.MapжVar(Ꮡm2));
    for (nint i = 0; i < 9; i++) {
        Ꮡm2.Add(strconv.Itoa(i), (int64)i);
    }
    ref var s1 = ref heap(new global::go.expvar_package.ΔString(), out var Ꮡs1);
    ref var s2 = ref heap(new global::go.expvar_package.ΔString(), out var Ꮡs2);
    Ꮡm.Set(str1ˢ, new expvar_internal_test_package.expvar_ΔStringжVar(Ꮡs1));
    Ꮡs1.Set(helloWorldˢ);
    Ꮡm.Set(str2ˢ, new expvar_internal_test_package.expvar_ΔStringжVar(Ꮡs2));
    Ꮡs2.Set(fizzBuzzˢ);
    b.ResetTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        _ = Ꮡm.String();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ = "tcp"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void BenchmarkRealworldExpvarUsage(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        ref var bytesSent = ref heap(new global::go.expvar_package.Int(), out var ᏑbytesSent);
        ref var bytesRead = ref heap(new global::go.expvar_package.Int(), out var ᏑbytesRead);
        // The benchmark creates GOMAXPROCS client/server pairs.
        // Each pair creates 4 goroutines: client reader/writer and server reader/writer.
        // The benchmark stresses concurrent reading and writing to the same connection.
        // Such pattern is used in net/http and net/rpc.
        b.StopTimer();
        nint P = Δruntime.GOMAXPROCS(0);
        nint N = quo(b.N, P);
        nint W = 1000;
        // Setup P client/server connections.
        var clients = new slice<Δnet.Conn>(P);
        var servers = new slice<Δnet.Conn>(P);
        var (ln, err) = Δnet.Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡb.Fatalf("Listen failed: %v"u8, err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var done = new channel<bool>(1);
        var doneʗ1 = done;
        var lnʗ2 = ln;
        var serversʗ1 = servers;
        goǃ(() => {
            for (nint p = 0; p < P; p++) {
                var (s, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != default!) {
                    Ꮡb.Errorf("Accept failed: %v"u8, errΔ1);
                    doneʗ1.ᐸꟷ(false);
                    return;
                }
                serversʗ1[p] = s;
            }
            doneʗ1.ᐸꟷ(true);
        });
        for (nint p = 0; p < P; p++) {
            var (c, errΔ2) = Δnet.Dial(tcpˢ, ln.Addr().String());
            if (errΔ2 != default!) {
                ᐸꟷ(done);
                Ꮡb.Fatalf("Dial failed: %v"u8, errΔ2);
            }
            clients[p] = c;
        }
        if (!ᐸꟷ(done)) {
            Ꮡb.FailNow();
        }
        b.StartTimer();
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(4 * P);
        for (nint p = 0; p < P; p++) {
            // Client writer.
            goǃ((Δnet.Conn c) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                    for (nint i = 0; i < N; i++) {
                        var v = (byte)i;
                        for (nint w = 0; w < W; w++) {
                            v *= v;
                        }
                        buf[0] = v;
                        var (n, errΔ3) = c.Write(buf[..]);
                        if (errΔ3 != default!) {
                            Ꮡb.Errorf("Write failed: %v"u8, errΔ3);
                            return;
                        }
                        ᏑbytesSent.Add((int64)n);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, clients[p]);
            // Pipe between server reader and server writer.
            var pipe = new channel<byte>(128);
            // Server reader.
            var pipeʗ1 = pipe;
            goǃ((Δnet.Conn s) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                    for (nint i = 0; i < N; i++) {
                        var (n, errΔ4) = s.Read(buf[..]);
                        if (errΔ4 != default!) {
                            Ꮡb.Errorf("Read failed: %v"u8, errΔ4);
                            return;
                        }
                        ᏑbytesRead.Add((int64)n);
                        pipeʗ1.ᐸꟷ(buf[0]);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, servers[p]);
            // Server writer.
            var pipeʗ2 = pipe;
            goǃ((Δnet.Conn s) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                    for (nint i = 0; i < N; i++) {
                        var v = (byte)(ᐸꟷ(pipeʗ2));
                        for (nint w = 0; w < W; w++) {
                            v *= v;
                        }
                        buf[0] = v;
                        var (n, errΔ5) = s.Write(buf[..]);
                        if (errΔ5 != default!) {
                            Ꮡb.Errorf("Write failed: %v"u8, errΔ5);
                            return;
                        }
                        ᏑbytesSent.Add((int64)n);
                    }
                    s.Close();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, servers[p]);
            // Client reader.
            goǃ((Δnet.Conn c) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                    for (nint i = 0; i < N; i++) {
                        var (n, errΔ6) = c.Read(buf[..]);
                        if (errΔ6 != default!) {
                            Ꮡb.Errorf("Read failed: %v"u8, errΔ6);
                            return;
                        }
                        ᏑbytesRead.Add((int64)n);
                    }
                    c.Close();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, clients[p]);
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestAppendJSONQuote(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    slice<byte> b = default!;
    for (nint i = 0; i < 128; i++) {
        b = append(b, (byte)i);
    }
    b = append(b, ((@string)"\u2028\u2029"u8).ꓸꓸꓸ);
    @string got = ((@string)appendJSONQuote(default!, ((@string)(b[..]))));
    @string want = @""""u8 + @"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\t\n\u000b\u000c\r\u000e\u000f"u8 + @"\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f"u8 + @" !\""#$%\u0026'()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_"u8 + "`"u8 + @"abcdefghijklmnopqrstuvwxyz{|}~"u8 + "\x7f"u8 + @"\u2028\u2029"""u8;
    if (got != want) {
        Ꮡt.Errorf("appendJSONQuote mismatch:\ngot  %v\nwant %v"u8, got, want);
    }
}

} // end expvar_internal_test_package
