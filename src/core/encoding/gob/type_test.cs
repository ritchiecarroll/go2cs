// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using reflect = reflect_package;
using sync = sync_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using io = io_package;
using static go.encoding.gob_package;

partial class gob_internal_test_package {

[GoType] internal partial struct typeT {
    internal global::go.encoding.gob_package.typeId id;
    internal @string str;
}

internal static slice<typeT> basicTypes;
internal static void initᴛbasicTypes() { basicTypes = new typeT[]{
    new(tBool, "bool"u8),
    new(tInt, "int"u8),
    new(tUint, "uint"u8),
    new(tFloat, "float"u8),
    new(tBytes, "bytes"u8),
    new(tString, "string"u8)
}.slice(); }

internal static global::go.encoding.gob_package.ΔgobType getTypeUnlocked(@string name, reflectꓸType rt) {
    GoFrame ᒐ = default;
    try {
        ᏑtypeLock.Lock();
        defer(ᏑtypeLock.Unlock, ref ᒐ);
        var (t, err) = getBaseType(name, rt);
        if (err != default!) {
            throw panic("getTypeUnlocked: " + err.Error());
        }
        return t;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Sanity checks
public static void TestBasic(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in basicTypes) {
        if (tt.id.@string() != tt.str) {
            Ꮡt.Errorf("checkType: expected %q got %s"u8, tt.str, tt.id.@string());
        }
        if (tt.id == 0) {
            Ꮡt.Errorf("id for %q is zero"u8, tt.str);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string intˢ = "int"u8;
internal static readonly @string uintˢ = "uint"u8;

// Reregister some basic types to check registration is idempotent.
public static void TestReregistration(ж<testing.T> Ꮡt) {
    var newtyp = getTypeUnlocked(intˢ, reflect.TypeFor<nint>());
    if (!AreEqual(newtyp, tInt.gobType())) {
        Ꮡt.Errorf("reregistration of %s got new type"u8, newtyp.@string());
    }
    newtyp = getTypeUnlocked(uintˢ, reflect.TypeFor<nuint>());
    if (!AreEqual(newtyp, tUint.gobType())) {
        Ꮡt.Errorf("reregistration of %s got new type"u8, newtyp.@string());
    }
    newtyp = getTypeUnlocked(stringˢ, reflect.TypeFor<@string>());
    if (!AreEqual(newtyp, tString.gobType())) {
        Ꮡt.Errorf("reregistration of %s got new type"u8, newtyp.@string());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gooˢ = "goo"u8;
internal static readonly @string boolˢ = "[3]bool"u8;

public static void TestArrayType(ж<testing.T> Ꮡt) {
    array<nint> a3 = new(3);
    var a3int = getTypeUnlocked(fooˢ, reflect.TypeOf(a3));
    var newa3int = getTypeUnlocked(barˢ, reflect.TypeOf(a3));
    if (!AreEqual(a3int, newa3int)) {
        Ꮡt.Errorf("second registration of [3]int creates new type"u8);
    }
    array<nint> a4 = new(4);
    var a4int = getTypeUnlocked(gooˢ, reflect.TypeOf(a4));
    if (AreEqual(a3int, a4int)) {
        Ꮡt.Errorf("registration of [3]int creates same type as [4]int"u8);
    }
    array<bool> b3 = new(3);
    var a3bool = getTypeUnlocked(""u8, reflect.TypeOf(b3));
    if (AreEqual(a3int, a3bool)) {
        Ꮡt.Errorf("registration of [3]bool creates same type as [3]int"u8);
    }
    @string str = a3bool.@string();
    @string expected = boolˢ;
    if (str != expected) {
        Ꮡt.Errorf("array printed as %q; expected %q"u8, str, expected);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sliceˢ = "slice"u8;
internal static readonly @string slice1ˢ = "slice1"u8;
internal static readonly @string boolˢ2 = "[]bool"u8;

public static void TestSliceType(ж<testing.T> Ꮡt) {
    slice<nint> s = default!;
    var sint = getTypeUnlocked(sliceˢ, reflect.TypeOf(s));
    slice<nint> news = default!;
    var newsint = getTypeUnlocked(slice1ˢ, reflect.TypeOf(news));
    if (!AreEqual(sint, newsint)) {
        Ꮡt.Errorf("second registration of []int creates new type"u8);
    }
    slice<bool> b = default!;
    var sbool = getTypeUnlocked(""u8, reflect.TypeOf(b));
    if (AreEqual(sbool, sint)) {
        Ꮡt.Errorf("registration of []bool creates same type as []int"u8);
    }
    @string str = sbool.@string();
    @string expected = boolˢ2;
    if (str != expected) {
        Ꮡt.Errorf("slice printed as %q; expected %q"u8, str, expected);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mapˢ = "map"u8;
internal static readonly @string map1ˢ = "map1"u8;
internal static readonly @string mapStringBoolˢ = "map[string]bool"u8;

public static void TestMapType(ж<testing.T> Ꮡt) {
    map<@string, nint> m = default!;
    var mapStringInt = getTypeUnlocked(mapˢ, reflect.TypeOf(m));
    map<@string, nint> newm = default!;
    var newMapStringInt = getTypeUnlocked(map1ˢ, reflect.TypeOf(newm));
    if (!AreEqual(mapStringInt, newMapStringInt)) {
        Ꮡt.Errorf("second registration of map[string]int creates new type"u8);
    }
    map<@string, bool> b = default!;
    var mapStringBool = getTypeUnlocked(""u8, reflect.TypeOf(b));
    if (AreEqual(mapStringBool, mapStringInt)) {
        Ꮡt.Errorf("registration of map[string]bool creates same type as map[string]int"u8);
    }
    @string str = mapStringBool.@string();
    @string expected = mapStringBoolˢ;
    if (str != expected) {
        Ꮡt.Errorf("map printed as %q; expected %q"u8, str, expected);
    }
}

[GoType] public partial struct Bar {
    public @string X;
}

// This structure has pointers and refers to itself, making it a good test case.
[GoType] public partial struct Foo {
    public nint A;
    public int32 B; // will become int
    public @string C;
    public slice<byte> D;
    public ж<float64> E; // will become float64
    public ж<ж<ж<ж<float64>>>> F; // will become float64
    public ж<Bar> G;
    public ж<Bar> H; // should not interpolate the definition of Bar again
    public ж<Foo> I; // will not explode
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ2 = "Foo"u8;
internal static readonly @string fooStructAIntBIntCStringˢ = "Foo = struct { A int; B int; C string; D bytes; E float; F float; G Bar = struct { X string; }; H Bar; I Foo; }"u8;

public static void TestStructType(ж<testing.T> Ꮡt) {
    var sstruct = getTypeUnlocked(fooˢ2, reflect.TypeFor<Foo>());
    @string str = sstruct.@string();
    // If we can print it correctly, we built it correctly.
    @string expected = fooStructAIntBIntCStringˢ;
    if (str != expected) {
        Ꮡt.Errorf("struct printed as %q; expected %q"u8, str, expected);
    }
}

[GoType("dyn")] [GoLocalName("T")] internal partial struct TestRegistration_T {
    internal nint a;
}

// Should be OK to register the same type multiple times, as long as they're
// at the same level of indirection.
public static void TestRegistration(ж<testing.T> Ꮡt) {
    Register(@new<TestRegistration_T>());
    Register(@new<TestRegistration_T>());
}

[GoType] public partial struct N1 {
}

[GoType] public partial struct N2 {
}

[GoType("dyn")] internal partial struct TestRegistrationNaming_testCases {
    internal any t;
    internal @string name;
}

// See comment in type.go/Register.
public static void TestRegistrationNaming(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var testCases = new TestRegistrationNaming_testCases[]{
        new(Ꮡ(new N1(nil)), "*gob.N1"u8),
        new(new N2(nil), "encoding/gob.N2"u8)
    }.slice();
    foreach (var (_, tc) in testCases) {
        Register(tc.t);
        var tct = reflect.TypeOf(tc.t);
        var (ct, _) = ᏑnameToConcreteType.Load(tc.name);
        if (!AreEqual(ct, tct)) {
            Ꮡt.Errorf("nameToConcreteType[%q] = %v, want %v"u8, tc.name, ct, tct);
        }
        // concreteTypeToName is keyed off the base type.
        if (tct.Kind() == reflect.ΔPointer) {
            tct = tct.Elem();
        }
        {
            var (n, _) = ᏑconcreteTypeToName.Load(tct); if (!AreEqual(n, tc.name)) {
                Ꮡt.Errorf("concreteTypeToName[%v] got %v, want %v"u8, tct, n, tc.name);
            }
        }
    }
}

[GoType("dyn")] [GoLocalName("T2")] internal partial struct TestStressParallel_T2 {
    public nint A;
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestStressParallel(ж<testing.T> Ꮡt) {
    var c = new channel<bool>(0);
    const nint N = 10;
    for (nint i = 0; i < N; i++) {
        var cʗ1 = c;
        goǃ(() => {
            var p = @new<TestStressParallel_T2>();
            Register(p.OrTypedNil());
            var b = @new<bytes.Buffer>();
            var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
            var err = enc.Encode(p.OrTypedNil());
            if (err != default!) {
                Ꮡt.Error(encoderFailˢ, err);
            }
            var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
            err = dec.Decode(p.OrTypedNil());
            if (err != default!) {
                Ꮡt.Error(decoderFailˢ, err);
            }
            cʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < N; i++) {
        ᐸꟷ(c);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodeUnexpectedlyˢ = (@string)"decode unexpectedly succeeded"u8;

// Issue 23328. Note that this test name is known to cmd/dist/test.go.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestTypeRace(ж<testing.T> Ꮡt) {
    var c = new channel<bool>(0);
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 2; i++) {
        Ꮡwg.Add(1);
        var cʗ1 = c;
        goǃ((nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡbuf));
                var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡbuf));
                any x = default!;
                switch (iΔ1) {
                case 0: {
                    x = Ꮡ(new N1(nil));
                    break;
                }
                case 1: {
                    x = Ꮡ(new N2(nil));
                    break;
                }
                default: {
                    Ꮡt.Errorf("bad i %d"u8, iΔ1);
                    return;
                }}

                ref var m = ref heap<map<@string, @string>>(out var Ꮡm);
                m = new map<@string, @string>();
                ᐸꟷ(cʗ1);
                {
                    var err = enc.Encode(x); if (err != default!) {
                        Ꮡt.Error(err);
                        return;
                    }
                }
                {
                    var err = enc.Encode(x); if (err != default!) {
                        Ꮡt.Error(err);
                        return;
                    }
                }
                {
                    var err = dec.Decode(Ꮡm); if (err == default!) {
                        Ꮡt.Error(decodeUnexpectedlyˢ);
                        return;
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }
    close(c);
    Ꮡwg.Wait();
}

} // end gob_internal_test_package
