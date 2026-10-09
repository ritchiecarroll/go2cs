// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using testEface = object;

namespace go;

using fmt = fmt_package;
using abi = @internal.abi_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using static go.unique_package;
using weak = weak_package;

partial class unique_internal_test_package {

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string barˢ = "bar"u8;

public static void TestHandle(ж<testing.T> Ꮡt) {
    testHandle<testString, testString>(Ꮡt, ((testString)(@string)fooˢ));
    testHandle<testString, testString>(Ꮡt, ((testString)(@string)barˢ));
    testHandle<testString, testString>(Ꮡt, ((testString)(@string)""u8));
    testHandle<testIntArray, testIntArray>(Ꮡt, new testIntArray(new nint[]{7, 77, 777, 7777}.array()));
    testHandle<testEface, testEfaceᴅ>(Ꮡt, ((testEface)default!));
    testHandle<testStringArray, testStringArray>(Ꮡt, new testStringArray(new @string[]{"a"u8, "b"u8, "c"u8}.array()));
    testHandle<testStringStruct, testStringStruct>(Ꮡt, new testStringStruct("x"u8));
    testHandle<testStringStructArrayStruct, testStringStructArrayStruct>(Ꮡt, new testStringStructArrayStruct(
        s: new testStringStruct[]{new("y"u8), new("z"u8)}.array()
    ));
    testHandle<testStruct, testStruct>(Ꮡt, new testStruct(0.5D, "184"u8));
    testHandle<testEface, testEfaceᴅ>(Ꮡt, ((testEface)(@string)("hello"u8)));
    testHandle<testZeroSize, testZeroSize>(Ꮡt, new testZeroSize());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object v0ValueV1Valueˢ = (@string)"v0.Value != v1.Value"u8;
internal static readonly object v0V1ˢ = (@string)"v0 != v1"u8;

internal static void testHandle<T, Tᴺ>(ж<testing.T> Ꮡt, T value) {
    @string name = reflect.TypeFor<Tᴺ>().Name();
    Ꮡt.Run(fmt.Sprintf("%s/%#v"u8, name, value), (ж<testing.T> tΔ1) => {
        tΔ1.Parallel();
        var v0 = Make<T>(value);
        var v1 = Make<T>(value);
        if (!AreEqual(v0.Value(), v1.Value())) {
            tΔ1.Error(v0ValueV1Valueˢ);
        }
        if (!AreEqual(v0.Value(), value)) {
            tΔ1.Errorf("v0.Value not %#v"u8, value);
        }
        if (v0 != v1) {
            tΔ1.Error(v0V1ˢ);
        }
        drainMaps<T>(tΔ1);
        checkMapsFor(tΔ1, value);
    });
}

// drainMaps ensures that the internal maps are drained.
internal static void drainMaps<T>(ж<testing.T> Ꮡt) {
    Ꮡt.Helper();
    if (@unsafe.Sizeof((@new<T>()).ValueSlot) == 0) {
        return; // zero-size types are not inserted.
    }
    var wait = new channel<EmptyStruct>(1);
    // Set up a one-time notification for the next time the cleanup runs.
    // Note: this will only run if there's no other active cleanup, so
    // we can be sure that the next time cleanup runs, it'll see the new
    // notification.
    ᏑcleanupMu.Lock();
    var waitʗ1 = wait;
    cleanupNotify = append(cleanupNotify, () => {
        var selᴛ1 = waitʗ1.ᐸꟷ(new EmptyStruct(), ꓸꓸꓸ);
        switch (trySelect(selᴛ1)) {
        case 0: {
            break;
        }
        default: {
            break;
        }}
    });
    Δruntime.GC();
    ᏑcleanupMu.Unlock();
    // Wait until cleanup runs.
    ᐸꟷ(wait);
}

internal static void checkMapsFor<T>(ж<testing.T> Ꮡt, T value) {
    // Manually load the value out of the map.
    var typ = abi.TypeFor<T>();
    var (a, ok) = ᏑuniqueMaps.Load(typ);
    if (!ok) {
        return;
    }
    var m = a._<ж<global::go.unique_package.uniqueMap<T>>>();
    (var wp, ok) = m.of(global::go.unique_package.uniqueMap<T>.ᏑHashTrieMap).Load(value);
    if (!ok) {
        return;
    }
    if (wp.Value() != nil) {
        Ꮡt.Errorf("value %v still referenced a handle (or tiny block?) "u8, value);
        return;
    }
    Ꮡt.Errorf("failed to drain internal maps of %v"u8, value);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object stringWasImproperlyˢ = (@string)"string was improperly retained"u8;

public static void TestMakeClonesStrings(ж<testing.T> Ꮡt) {
    @string s = strings.Clone("abcdefghijklmnopqrstuvwxyz"u8); // N.B. Must be big enough to not be tiny-allocated.
    var ran = new channel<bool>(0);
    var ranʗ1 = ran;
    Δruntime.SetFinalizer(@unsafe.StringData(s).OrTypedNil(), (ж<byte> _) => {
        ranʗ1.ᐸꟷ(true);
    });
    var h = Make<@string>(s);
    // Clean up s (hopefully) and run the finalizer.
    Δruntime.GC();
    var selᴛ2 = time.After(1 * time.ΔSecond);
    var selᴛ3 = ran;
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out _): {
        Ꮡt.Fatal(stringWasImproperlyˢ);
        break;
    }
    case 1 when selᴛ3.ꟷᐳ(out _): {
        break;
    }}
    Δruntime.KeepAlive(h);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unsafeStringImproperlyˢ = (@string)"unsafe string improperly retained internally"u8;

public static void TestHandleUnsafeString(ж<testing.T> Ꮡt) {
    slice<@string> testData = default!;
    foreach (var i in range(1024)) {
        testData = append(testData, strconv.Itoa(i));
    }
    slice<byte> buf = default!;
    slice<global::go.unique_package.Handle<@string>> handles = default!;
    foreach (var (_, s) in testData) {
        if (len(buf) < len(s)) {
            buf = new slice<byte>(len(s) * 2);
        }
        copy(buf, s);
        @string sbuf = @unsafe.String(Ꮡ(buf, 0), len(s));
        handles = append(handles, Make<@string>(sbuf));
    }
    foreach (var (i, s) in testData) {
        var h = Make<@string>(s);
        if (handles[i].Value() != h.Value()) {
            Ꮡt.Fatal(unsafeStringImproperlyˢ);
        }
    }
}

} // end unique_internal_test_package
