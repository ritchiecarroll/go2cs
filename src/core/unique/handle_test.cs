// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using testEface = object;

namespace go;

using fmt = fmt_package;
using abi = @internal.abi_package;
using asan = @internal.asan_package;
using msan = @internal.msan_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using rand = math.rand.rand_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using math.rand;
using static go.unique_package;
using ꓸꓸꓸany = Span<any>;

partial class unique_internal_test_package {

// Set up special types. Because the internal maps are sharded by type,
// this will ensure that we're not overlapping with other tests.
internal partial struct testString /*@string*/;

internal partial struct testIntArray /*[4]nint*/;

internal partial struct testStringArray /*[3]@string*/;

internal partial struct testStringStruct {
    internal @string a;
}

internal partial struct testStringStructArrayStruct {
    internal array<testStringStruct> s = new(2);
}

internal partial struct testStruct {
    internal float64 z;
    internal @string b;
}

internal partial struct testZeroSize {
}

internal partial struct testNestedHandle {
    internal global::go.unique_package.Handle<testNestedHandle> next;
    internal array<nint> arr = new(6);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;

public static void TestHandle(ж<testing.T> t) {
    testHandle<testString, testString>(t, (testString)(@string)fooˢ);
    testHandle<testString, testString>(t, (testString)(@string)barˢ);
    testHandle<testString, testString>(t, (testString)(@string)""u8);
    testHandle<testIntArray, testIntArray>(t, new testIntArray(new nint[]{7, 77, 777, 7777}.array()));
    testHandle<testEface, testEfaceᴅ>(t, (testEface)default);
    testHandle<testStringArray, testStringArray>(t, new testStringArray(new @string[]{"a", "b", "c"}.array()));
    testHandle<testStringStruct, testStringStruct>(t, new testStringStruct("x"));
    testHandle<testStringStructArrayStruct, testStringStructArrayStruct>(t, new testStringStructArrayStruct(
        s: new testStringStruct[]{new("y"), new("z")}.array()
    ));
    testHandle<testStruct, testStruct>(t, new testStruct(0.5D, "184"));
    testHandle<testEface, testEfaceᴅ>(t, (testEface)(@string)("hello"u8));
    testHandle<testZeroSize, testZeroSize>(t, new testZeroSize());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object v0ValueV1Valueˢ = (@string)"v0.Value != v1.Value"u8;
internal static readonly object v0V1ˢ = (@string)"v0 != v1"u8;

internal static void testHandle<T, Tᴺ>(ж<testing.T> t, T value) {
    @string name = reflect.TypeFor<Tᴺ>().Name();
    t.Run(fmt.Sprintf("%s/%#v"u8, name, (value).OrTypedNilOf()), (ж<testing.T> tΔ1) => {
        var v0 = Make<T>(value);
        var v1 = Make<T>(value);

        if (!AreEqual(v0.Value(), v1.Value())) {
            tΔ1.Error(v0ValueV1Valueˢ);
        }
        if (!AreEqual(v0.Value(), value)) {
            tΔ1.Errorf("v0.Value not %#v", (value).OrTypedNilOf());
        }
        if (v0 != v1) {
            tΔ1.Error(v0V1ˢ);
        }

        drainMaps<T>(tΔ1);
        checkMapsFor(tΔ1, value);
    });
}

// drainMaps ensures that the internal maps are drained.
internal static void drainMaps<T>(ж<testing.T> t) {
    t.Helper();

    if (@unsafe.Sizeof((@new<T>()).ValueSlot) == 0) {
        return; // zero-size types are not inserted.
    }
    drainCleanupQueue(t);
}

internal static partial void drainCleanupQueue(ж<testing.T> t) {
    t.Helper();

    Δruntime.GC(); // Queue up the cleanups.
    runtime_blockUntilEmptyCleanupQueue((int64)((time.Duration)(5000000000L)));
}

internal static void checkMapsFor<T>(ж<testing.T> t, T value) {
    // Manually load the value out of the map.
    var typ = abi.TypeFor<T>();
    var (a, ok) = ᏑuniqueMaps.Load(typ);
    if (!ok) {
        return;
    }
    var m = a._<ж<global::go.unique_package.uniqueMap<T>>>();
    var p = m.Load(value);
    if (p != nil) {
        t.Errorf("value %v still referenced by a handle (or tiny block?): internal pointer %p", (value).OrTypedNilOf(), p.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object stringWasImproperlyˢ = (@string)"string was improperly retained"u8;

public static void TestMakeClonesStrings(ж<testing.T> t) {
    @string s = strings.Clone("abcdefghijklmnopqrstuvwxyz"); // N.B. Must be big enough to not be tiny-allocated.
    var ran = new channel<bool>(0);
    Δruntime.AddCleanup(@unsafe.StringData(s), (channel<bool> ch) => {
        ch.ᐸꟷ(true);
    }, ran);
    var h = Make<@string>(s);

    // Clean up s (hopefully) and run the cleanup.
    Δruntime.GC();

    var selᴛ1 = time.After(1 * time.ΔSecond);
    var selᴛ2 = ran;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        t.Fatal(stringWasImproperlyˢ);
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        break;
    }}
    Δruntime.KeepAlive(h);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unsafeStringImproperlyˢ = (@string)"unsafe string improperly retained internally"u8;

public static void TestHandleUnsafeString(ж<testing.T> t) {
    var testDataᴷ = new stackAppend<@string>();
    var handlesᴷ = new stackAppend<global::go.unique_package.Handle<@string>>();

    slice<@string> testData = default;
    foreach (var i in range(1024)) {
        testData = append(testDataᴷ, testData, strconv.Itoa(i));
    }
    slice<byte> buf = default;
    slice<global::go.unique_package.Handle<@string>> handles = default;
    foreach (var (_, s) in testData) {
        if (len(buf) < len(s)) {
            buf = new slice<byte>(len(s) * 2);
        }
        copy(buf, s);
        @string sbuf = @unsafe.String(Ꮡ(buf, 0), len(s));
        handles = append(handlesᴷ, handles, Make<@string>(sbuf));
    }
    foreach (var (i, s) in testData) {
        var h = Make<@string>(s);
        if (handles[i].Value() != h.Value()) {
            t.Fatal(unsafeStringImproperlyˢ);
        }
    }
}

internal static testNestedHandle nestHandle(testNestedHandle n) {
    n = n.ΔClone();

    return new testNestedHandle(
        next: Make<testNestedHandle>(n),
        arr: n.arr.Clone()
    );
}

public static void TestNestedHandle(ж<testing.T> t) {
    var n0 = new testNestedHandle(arr: new nint[]{1, 2, 3, 4, 5, 6}.array());
    var n1 = nestHandle(n0);
    var n2 = nestHandle(n1);
    var n3 = nestHandle(n2);

    {
        var v = n3.next.Value(); if (v != n2) {
            t.Errorf("n3.Value != n2: %#v vs. %#v", v, n2);
        }
    }
    {
        var v = n2.next.Value(); if (v != n1) {
            t.Errorf("n2.Value != n1: %#v vs. %#v", v, n1);
        }
    }
    {
        var v = n1.next.Value(); if (v != n0) {
            t.Errorf("n1.Value != n0: %#v vs. %#v", v, n0);
        }
    }

    // In a good implementation, the entire chain, down to the bottom-most
    // value, should all be gone after we drain the maps.
    drainMaps<testNestedHandle>(t);
    checkMapsFor(t, n0);
}

// Implemented in runtime.
//
// Used only by tests.
//
//go:linkname runtime_blockUntilEmptyCleanupQueue
/*linkname*/ internal static partial bool runtime_blockUntilEmptyCleanupQueue(int64 timeout) {
    return Δruntime.unique_runtime_blockUntilEmptyCleanupQueue(timeout);
}

internal static nint randomNumber = rand.IntN(1000000) + 1000000;
internal static slice<byte> heapBytes = newHeapBytes();
internal static @string heapString = newHeapString();

internal static global::go.unique_package.Handle<@string> stringHandle;
internal static global::go.unique_package.Handle<nint> intHandle;
internal static global::go.unique_package.Handle<any> anyHandle;
internal static global::go.unique_package.Handle<array<@string>> pairHandle;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string thisStringIsStaticallyˢ => ref thisStringIsStaticallyˢᵇ;
private static readonly @string thisStringIsStaticallyˢᵇ = "this string is statically allocated"u8;

internal partial struct TestMakeAllocs_tests /*dyn*/ {
    internal @string name;
    internal nint allocs;
    internal Action f;
}

public static void TestMakeAllocs(ж<testing.T> t) {
    Actionꓸꓸꓸ<@string, any> errorf = (@string p1, params ꓸꓸꓸany p2) => t.Errorf(p1, p2);
    if (race.Enabled || msan.Enabled || asan.Enabled || testenv.OptimizationOff()) {
        errorf = (@string p1, params ꓸꓸꓸany p2) => t.Logf(p1, p2);
    }

    var tests = slice<TestMakeAllocs_tests>([
        new(name: "create heap bytes", allocs: 1, f: () => {
            heapBytes = newHeapBytes();
        }),

        new(name: "create heap string", allocs: 2, f: () => {
            heapString = newHeapString();
        }),

        new(name: "static string", allocs: 0, f: () => {
            stringHandle = Make<@string>(thisStringIsStaticallyˢ);
        }),

        new(name: "heap string", allocs: 0, f: () => {
            stringHandle = Make<@string>(heapString);
        }),

        new(name: "stack string", allocs: 0, f: () => {
            array<byte> b = new(16);
            b[8] = (byte)'a';
            stringHandle = Make<@string>((@string)(b[..]));
        }),

        new(name: "bytes", allocs: 0, f: () => {
            stringHandle = Make<@string>((@string)heapBytes);
        }),

        new(name: "bytes truncated short", allocs: 0, f: () => {
            stringHandle = Make<@string>((@string)(heapBytes[..16]));
        }),

        new(name: "bytes truncated long", allocs: 0, f: () => {
            stringHandle = Make<@string>((@string)(heapBytes[..40]));
        }),

        new(name: "string to any", allocs: 1, f: () => {
            anyHandle = Make<any>(heapString);
        }),

        new(name: "large number", allocs: 0, f: () => {
            intHandle = Make<nint>(randomNumber);
        }),

        new(name: "large number to any", allocs: 1, f: () => {
            anyHandle = Make<any>(randomNumber);
        }),

        new(name: "pair", allocs: 0, f: () => {
            pairHandle = Make<array<@string>>(new @string[]{heapString, heapString}.array());
        }),

        new(name: "pair from stack", allocs: 0, f: () => {
            array<byte> b = new(16);
            b[8] = (byte)'a';
            pairHandle = Make<array<@string>>(new @string[]{(@string)(b[..]), (@string)(b[..])}.array());
        }),

        new(name: "pair to any", allocs: 1, f: () => {
            anyHandle = Make<any>(new @string[]{heapString, heapString}.array());
        })
    ]);

    foreach (var (_, tt) in tests) {
        var allocs = testing.AllocsPerRun(100, tt.f);
        if (allocs != (float64)tt.allocs) {
            errorf("%s: got %v allocs, want %v", tt.name, allocs, tt.allocs);
        }
    }
}

//go:noinline
internal static partial slice<byte> newHeapBytes() {
    const nint N = 100;
    var b = new slice<byte>(N);
    foreach (var i in range(b)) {
        b[i] = (byte)i;
    }
    return b;
}

//go:noinline
internal static partial @string newHeapString() {
    return (@string)newHeapBytes();
}

} // end unique_internal_test_package
