// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using rand = go.math.rand_package;
using reflect = reflect_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.math;
using static go.testing.quick_package;

partial class quick_internal_test_package {

internal static array<byte> fArray(/*[4]*/ array<byte> a) {
    a = a.Clone();

    return a.Clone();
}

public partial struct TestArrayAlias /*[4]byte*/;

internal static TestArrayAlias fArrayAlias(TestArrayAlias a) {
    a = a.Clone();

    return a.Clone();
}

internal static bool fBool(bool a) {
    return a;
}

public partial struct TestBoolAlias /*bool*/;

internal static TestBoolAlias fBoolAlias(TestBoolAlias a) {
    return a;
}

internal static float32 fFloat32(float32 a) {
    return a;
}

public partial struct TestFloat32Alias /*num:float32*/;

internal static TestFloat32Alias fFloat32Alias(TestFloat32Alias a) {
    return a;
}

internal static float64 fFloat64(float64 a) {
    return a;
}

public partial struct TestFloat64Alias /*num:float64*/;

internal static TestFloat64Alias fFloat64Alias(TestFloat64Alias a) {
    return a;
}

internal static complex64 fComplex64(complex64 a) {
    return a;
}

public partial struct TestComplex64Alias /*num:complex64*/;

internal static TestComplex64Alias fComplex64Alias(TestComplex64Alias a) {
    return a;
}

internal static complex128 fComplex128(complex128 a) {
    return a;
}

public partial struct TestComplex128Alias /*num:complex128*/;

internal static TestComplex128Alias fComplex128Alias(TestComplex128Alias a) {
    return a;
}

internal static int16 fInt16(int16 a) {
    return a;
}

public partial struct TestInt16Alias /*num:int16*/;

internal static TestInt16Alias fInt16Alias(TestInt16Alias a) {
    return a;
}

internal static int32 fInt32(int32 a) {
    return a;
}

public partial struct TestInt32Alias /*num:int32*/;

internal static TestInt32Alias fInt32Alias(TestInt32Alias a) {
    return a;
}

internal static int64 fInt64(int64 a) {
    return a;
}

public partial struct TestInt64Alias /*num:int64*/;

internal static TestInt64Alias fInt64Alias(TestInt64Alias a) {
    return a;
}

internal static int8 fInt8(int8 a) {
    return a;
}

public partial struct TestInt8Alias /*num:int8*/;

internal static TestInt8Alias fInt8Alias(TestInt8Alias a) {
    return a;
}

internal static nint fInt(nint a) {
    return a;
}

public partial struct TestIntAlias /*num:nint*/;

internal static TestIntAlias fIntAlias(TestIntAlias a) {
    return a;
}

internal static map<nint, nint> fMap(map<nint, nint> a) {
    return a;
}

public partial struct TestMapAlias /*map[nint, nint]*/;

internal static TestMapAlias fMapAlias(TestMapAlias a) {
    return a;
}

internal static ж<nint> fPtr(ж<nint> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (Ꮡa == nil) {
        return default;
    }
    ref var b = ref heap<nint>(a, out var Ꮡb);
    return Ꮡb;
}

public partial class TestPtrAlias /*ж<nint>*/;

internal static TestPtrAlias fPtrAlias(TestPtrAlias a) {
    return a;
}

internal static slice<byte> fSlice(slice<byte> a) {
    return a;
}

public partial struct TestSliceAlias /*[]byte*/;

internal static TestSliceAlias fSliceAlias(TestSliceAlias a) {
    return a;
}

internal static @string fString(@string a) {
    return a;
}

public partial struct TestStringAlias /*@string*/;

internal static TestStringAlias fStringAlias(TestStringAlias a) {
    return a;
}

public partial struct TestStruct {
    public nint A;
    public @string B;
}

internal static TestStruct fStruct(TestStruct a) {
    return a;
}

public partial struct TestStructAlias /*TestStruct*/;

internal static TestStructAlias fStructAlias(TestStructAlias a) {
    return a;
}

internal static uint16 fUint16(uint16 a) {
    return a;
}

public partial struct TestUint16Alias /*num:uint16*/;

internal static TestUint16Alias fUint16Alias(TestUint16Alias a) {
    return a;
}

internal static uint32 fUint32(uint32 a) {
    return a;
}

public partial struct TestUint32Alias /*num:uint32*/;

internal static TestUint32Alias fUint32Alias(TestUint32Alias a) {
    return a;
}

internal static uint64 fUint64(uint64 a) {
    return a;
}

public partial struct TestUint64Alias /*num:uint64*/;

internal static TestUint64Alias fUint64Alias(TestUint64Alias a) {
    return a;
}

internal static uint8 fUint8(uint8 a) {
    return a;
}

public partial struct TestUint8Alias /*num:uint8*/;

internal static TestUint8Alias fUint8Alias(TestUint8Alias a) {
    return a;
}

internal static nuint fUint(nuint a) {
    return a;
}

public partial struct TestUintAlias /*num:nuint*/;

internal static TestUintAlias fUintAlias(TestUintAlias a) {
    return a;
}

internal static uintptr fUintptr(uintptr a) {
    return a;
}

public partial struct TestUintptrAlias /*num:uintptr*/;

internal static TestUintptrAlias fUintptrAlias(TestUintptrAlias a) {
    return a;
}

internal static partial void reportError(@string property, error err, ж<testing.T> t) {
    if (err != nil) {
        t.Errorf("%s: %s", property, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fArrayˢ => ref fArrayˢᵇ;
private static readonly @string fArrayˢᵇ = "fArray"u8;
internal static ref readonly @string fArrayAliasˢ => ref fArrayAliasˢᵇ;
private static readonly @string fArrayAliasˢᵇ = "fArrayAlias"u8;
internal static ref readonly @string fBoolˢ => ref fBoolˢᵇ;
private static readonly @string fBoolˢᵇ = "fBool"u8;
internal static ref readonly @string fBoolAliasˢ => ref fBoolAliasˢᵇ;
private static readonly @string fBoolAliasˢᵇ = "fBoolAlias"u8;
internal static ref readonly @string fFloat32ˢ => ref fFloat32ˢᵇ;
private static readonly @string fFloat32ˢᵇ = "fFloat32"u8;
internal static ref readonly @string fFloat32Aliasˢ => ref fFloat32Aliasˢᵇ;
private static readonly @string fFloat32Aliasˢᵇ = "fFloat32Alias"u8;
internal static ref readonly @string fFloat64ˢ => ref fFloat64ˢᵇ;
private static readonly @string fFloat64ˢᵇ = "fFloat64"u8;
internal static ref readonly @string fFloat64Aliasˢ => ref fFloat64Aliasˢᵇ;
private static readonly @string fFloat64Aliasˢᵇ = "fFloat64Alias"u8;
internal static ref readonly @string fComplex64ˢ => ref fComplex64ˢᵇ;
private static readonly @string fComplex64ˢᵇ = "fComplex64"u8;
internal static ref readonly @string fComplex64Aliasˢ => ref fComplex64Aliasˢᵇ;
private static readonly @string fComplex64Aliasˢᵇ = "fComplex64Alias"u8;
internal static ref readonly @string fComplex128ˢ => ref fComplex128ˢᵇ;
private static readonly @string fComplex128ˢᵇ = "fComplex128"u8;
internal static ref readonly @string fComplex128Aliasˢ => ref fComplex128Aliasˢᵇ;
private static readonly @string fComplex128Aliasˢᵇ = "fComplex128Alias"u8;
internal static ref readonly @string fInt16ˢ => ref fInt16ˢᵇ;
private static readonly @string fInt16ˢᵇ = "fInt16"u8;
internal static ref readonly @string fInt16Aliasˢ => ref fInt16Aliasˢᵇ;
private static readonly @string fInt16Aliasˢᵇ = "fInt16Alias"u8;
internal static ref readonly @string fInt32ˢ => ref fInt32ˢᵇ;
private static readonly @string fInt32ˢᵇ = "fInt32"u8;
internal static ref readonly @string fInt32Aliasˢ => ref fInt32Aliasˢᵇ;
private static readonly @string fInt32Aliasˢᵇ = "fInt32Alias"u8;
internal static ref readonly @string fInt64ˢ => ref fInt64ˢᵇ;
private static readonly @string fInt64ˢᵇ = "fInt64"u8;
internal static ref readonly @string fInt64Aliasˢ => ref fInt64Aliasˢᵇ;
private static readonly @string fInt64Aliasˢᵇ = "fInt64Alias"u8;
internal static ref readonly @string fInt8ˢ => ref fInt8ˢᵇ;
private static readonly @string fInt8ˢᵇ = "fInt8"u8;
internal static ref readonly @string fInt8Aliasˢ => ref fInt8Aliasˢᵇ;
private static readonly @string fInt8Aliasˢᵇ = "fInt8Alias"u8;
internal static ref readonly @string fIntˢ => ref fIntˢᵇ;
private static readonly @string fIntˢᵇ = "fInt"u8;
internal static ref readonly @string fIntAliasˢ => ref fIntAliasˢᵇ;
private static readonly @string fIntAliasˢᵇ = "fIntAlias"u8;
internal static ref readonly @string fMapˢ => ref fMapˢᵇ;
private static readonly @string fMapˢᵇ = "fMap"u8;
internal static ref readonly @string fMapAliasˢ => ref fMapAliasˢᵇ;
private static readonly @string fMapAliasˢᵇ = "fMapAlias"u8;
internal static ref readonly @string fPtrˢ => ref fPtrˢᵇ;
private static readonly @string fPtrˢᵇ = "fPtr"u8;
internal static ref readonly @string fPtrAliasˢ => ref fPtrAliasˢᵇ;
private static readonly @string fPtrAliasˢᵇ = "fPtrAlias"u8;
internal static ref readonly @string fSliceˢ => ref fSliceˢᵇ;
private static readonly @string fSliceˢᵇ = "fSlice"u8;
internal static ref readonly @string fSliceAliasˢ => ref fSliceAliasˢᵇ;
private static readonly @string fSliceAliasˢᵇ = "fSliceAlias"u8;
internal static ref readonly @string fStringˢ => ref fStringˢᵇ;
private static readonly @string fStringˢᵇ = "fString"u8;
internal static ref readonly @string fStringAliasˢ => ref fStringAliasˢᵇ;
private static readonly @string fStringAliasˢᵇ = "fStringAlias"u8;
internal static ref readonly @string fStructˢ => ref fStructˢᵇ;
private static readonly @string fStructˢᵇ = "fStruct"u8;
internal static ref readonly @string fStructAliasˢ => ref fStructAliasˢᵇ;
private static readonly @string fStructAliasˢᵇ = "fStructAlias"u8;
internal static ref readonly @string fUint16ˢ => ref fUint16ˢᵇ;
private static readonly @string fUint16ˢᵇ = "fUint16"u8;
internal static ref readonly @string fUint16Aliasˢ => ref fUint16Aliasˢᵇ;
private static readonly @string fUint16Aliasˢᵇ = "fUint16Alias"u8;
internal static ref readonly @string fUint32ˢ => ref fUint32ˢᵇ;
private static readonly @string fUint32ˢᵇ = "fUint32"u8;
internal static ref readonly @string fUint32Aliasˢ => ref fUint32Aliasˢᵇ;
private static readonly @string fUint32Aliasˢᵇ = "fUint32Alias"u8;
internal static ref readonly @string fUint64ˢ => ref fUint64ˢᵇ;
private static readonly @string fUint64ˢᵇ = "fUint64"u8;
internal static ref readonly @string fUint64Aliasˢ => ref fUint64Aliasˢᵇ;
private static readonly @string fUint64Aliasˢᵇ = "fUint64Alias"u8;
internal static ref readonly @string fUint8ˢ => ref fUint8ˢᵇ;
private static readonly @string fUint8ˢᵇ = "fUint8"u8;
internal static ref readonly @string fUint8Aliasˢ => ref fUint8Aliasˢᵇ;
private static readonly @string fUint8Aliasˢᵇ = "fUint8Alias"u8;
internal static ref readonly @string fUintˢ => ref fUintˢᵇ;
private static readonly @string fUintˢᵇ = "fUint"u8;
internal static ref readonly @string fUintAliasˢ => ref fUintAliasˢᵇ;
private static readonly @string fUintAliasˢᵇ = "fUintAlias"u8;
internal static ref readonly @string fUintptrˢ => ref fUintptrˢᵇ;
private static readonly @string fUintptrˢᵇ = "fUintptr"u8;
internal static ref readonly @string fUintptrAliasˢ => ref fUintptrAliasˢᵇ;
private static readonly @string fUintptrAliasˢᵇ = "fUintptrAlias"u8;

public static void TestCheckEqual(ж<testing.T> t) {
    reportError(fArrayˢ, CheckEqual(fArray, fArray, nil), t);
    reportError(fArrayAliasˢ, CheckEqual(fArrayAlias, fArrayAlias, nil), t);
    reportError(fBoolˢ, CheckEqual(fBool, fBool, nil), t);
    reportError(fBoolAliasˢ, CheckEqual(fBoolAlias, fBoolAlias, nil), t);
    reportError(fFloat32ˢ, CheckEqual(fFloat32, fFloat32, nil), t);
    reportError(fFloat32Aliasˢ, CheckEqual(fFloat32Alias, fFloat32Alias, nil), t);
    reportError(fFloat64ˢ, CheckEqual(fFloat64, fFloat64, nil), t);
    reportError(fFloat64Aliasˢ, CheckEqual(fFloat64Alias, fFloat64Alias, nil), t);
    reportError(fComplex64ˢ, CheckEqual(fComplex64, fComplex64, nil), t);
    reportError(fComplex64Aliasˢ, CheckEqual(fComplex64Alias, fComplex64Alias, nil), t);
    reportError(fComplex128ˢ, CheckEqual(fComplex128, fComplex128, nil), t);
    reportError(fComplex128Aliasˢ, CheckEqual(fComplex128Alias, fComplex128Alias, nil), t);
    reportError(fInt16ˢ, CheckEqual(fInt16, fInt16, nil), t);
    reportError(fInt16Aliasˢ, CheckEqual(fInt16Alias, fInt16Alias, nil), t);
    reportError(fInt32ˢ, CheckEqual(fInt32, fInt32, nil), t);
    reportError(fInt32Aliasˢ, CheckEqual(fInt32Alias, fInt32Alias, nil), t);
    reportError(fInt64ˢ, CheckEqual(fInt64, fInt64, nil), t);
    reportError(fInt64Aliasˢ, CheckEqual(fInt64Alias, fInt64Alias, nil), t);
    reportError(fInt8ˢ, CheckEqual(fInt8, fInt8, nil), t);
    reportError(fInt8Aliasˢ, CheckEqual(fInt8Alias, fInt8Alias, nil), t);
    reportError(fIntˢ, CheckEqual(fInt, fInt, nil), t);
    reportError(fIntAliasˢ, CheckEqual(fIntAlias, fIntAlias, nil), t);
    reportError(fInt32ˢ, CheckEqual(fInt32, fInt32, nil), t);
    reportError(fInt32Aliasˢ, CheckEqual(fInt32Alias, fInt32Alias, nil), t);
    reportError(fMapˢ, CheckEqual(fMap, fMap, nil), t);
    reportError(fMapAliasˢ, CheckEqual(fMapAlias, fMapAlias, nil), t);
    reportError(fPtrˢ, CheckEqual(fPtr, fPtr, nil), t);
    reportError(fPtrAliasˢ, CheckEqual(fPtrAlias, fPtrAlias, nil), t);
    reportError(fSliceˢ, CheckEqual(fSlice, fSlice, nil), t);
    reportError(fSliceAliasˢ, CheckEqual(fSliceAlias, fSliceAlias, nil), t);
    reportError(fStringˢ, CheckEqual(fString, fString, nil), t);
    reportError(fStringAliasˢ, CheckEqual(fStringAlias, fStringAlias, nil), t);
    reportError(fStructˢ, CheckEqual(fStruct, fStruct, nil), t);
    reportError(fStructAliasˢ, CheckEqual(fStructAlias, fStructAlias, nil), t);
    reportError(fUint16ˢ, CheckEqual(fUint16, fUint16, nil), t);
    reportError(fUint16Aliasˢ, CheckEqual(fUint16Alias, fUint16Alias, nil), t);
    reportError(fUint32ˢ, CheckEqual(fUint32, fUint32, nil), t);
    reportError(fUint32Aliasˢ, CheckEqual(fUint32Alias, fUint32Alias, nil), t);
    reportError(fUint64ˢ, CheckEqual(fUint64, fUint64, nil), t);
    reportError(fUint64Aliasˢ, CheckEqual(fUint64Alias, fUint64Alias, nil), t);
    reportError(fUint8ˢ, CheckEqual(fUint8, fUint8, nil), t);
    reportError(fUint8Aliasˢ, CheckEqual(fUint8Alias, fUint8Alias, nil), t);
    reportError(fUintˢ, CheckEqual(fUint, fUint, nil), t);
    reportError(fUintAliasˢ, CheckEqual(fUintAlias, fUintAlias, nil), t);
    reportError(fUintptrˢ, CheckEqual(fUintptr, fUintptr, nil), t);
    reportError(fUintptrAliasˢ, CheckEqual(fUintptrAlias, fUintptrAlias, nil), t);
}

// This tests that ArbitraryValue is working by checking that all the arbitrary
// values of type MyStruct have x = 42.
internal partial struct myStruct {
    internal nint x;
}

internal static reflectꓸValue Generate(this myStruct m, ж<rand.Rand> r, nint _) {
    return reflect.ValueOf(new myStruct(x: 42));
}

internal static bool myStructProperty(myStruct @in) {
    return @in.x == 42;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myStructPropertyˢ => ref myStructPropertyˢᵇ;
private static readonly @string myStructPropertyˢᵇ = "myStructProperty"u8;

public static partial void TestCheckProperty(ж<testing.T> t) {
    reportError(myStructPropertyˢ, Check(myStructProperty, nil), t);
}

public static void TestFailure(ж<testing.T> t) {
    var f = (nint x) => false;
    var err = Check((f).OrTypedNilFunc(), nil);
    if (err == nil) {
        t.Errorf("Check didn't return an error");
    }
    {
        var (_, ok) = err._<ж<global::go.testing.quick_package.CheckError>>(ᐧ); if (!ok) {
            t.Errorf("Error was not a CheckError: %s", err);
        }
    }

    err = CheckEqual(fUint, fUint32, nil);
    if (err == nil) {
        t.Errorf("#1 CheckEqual didn't return an error");
    }
    {
        var (_, ok) = err._<SetupError>(ᐧ); if (!ok) {
            t.Errorf("#1 Error was not a SetupError: %s", err);
        }
    }

    err = CheckEqual((nint x, nint y) => {
    }, (nint x) => {
    }, nil);
    if (err == nil) {
        t.Errorf("#2 CheckEqual didn't return an error");
    }
    {
        var (_, ok) = err._<SetupError>(ᐧ); if (!ok) {
            t.Errorf("#2 Error was not a SetupError: %s", err);
        }
    }

    err = CheckEqual(nint (nint x) => 0, int32 (nint x) => 0, nil);
    if (err == nil) {
        t.Errorf("#3 CheckEqual didn't return an error");
    }
    {
        var (_, ok) = err._<SetupError>(ᐧ); if (!ok) {
            t.Errorf("#3 Error was not a SetupError: %s", err);
        }
    }
}

internal partial struct TestRecursive_R /*dyn*/ {
    public ж<TestRecursive_R> Ptr;
    public slice<ж<TestRecursive_R>> SliceP;
    public slice<TestRecursive_R> Slice;
    public map<nint, TestRecursive_R> Map;
    public map<nint, ж<TestRecursive_R>> MapP;
    public map<ж<TestRecursive_R>, ж<TestRecursive_R>> MapR;
    public slice<map<nint, TestRecursive_R>> SliceMap;
}

// Recursive data structures didn't terminate.
// Issues 8818 and 11148.
public static void TestRecursive(ж<testing.T> t) {
    var f = (TestRecursive_R r) => true;
    Check((f).OrTypedNilFunc(), nil);
}

public static void TestEmptyStruct(ж<testing.T> t) {
    var f = (EmptyStruct _) => true;
    Check((f).OrTypedNilFunc(), nil);
}

public partial struct A {
    public ж<B> B;
}

public partial struct B {
    public ж<A> A;
}

public static void TestMutuallyRecursive(ж<testing.T> t) {
    var f = (A a) => true;
    Check((f).OrTypedNilFunc(), nil);
}

internal partial struct TestNonZeroSliceAndMap_Q /*dyn*/ {
    public map<nint, nint> M;
    public slice<nint> S;
}

// Some serialization formats (e.g. encoding/pem) cannot distinguish
// between a nil and an empty map or slice, so avoid generating the
// zero value for these.
public static void TestNonZeroSliceAndMap(ж<testing.T> t) {
    var f = (TestNonZeroSliceAndMap_Q q) => q.M != nil && q.S != nil;
    var err = Check((f).OrTypedNilFunc(), nil);
    if (err != nil) {
        t.Fatal(err);
    }
}

public static void TestInt64(ж<testing.T> t) {
    int64 lo = default;
    int64 hi = default;
    var f = (int64 x) => {
        if (x < lo) {
            lo = x;
        }
        if (x > hi) {
            hi = x;
        }
        return true;
    };
    var cfg = Ꮡ(new Config(MaxCount: 10000));
    Check((f).OrTypedNilFunc(), cfg);
    if ((uint64)lo >> 62 == 0 || (uint64)hi >> 62 == 0) {
        t.Errorf("int64 returned range %#016x,%#016x; does not look like full range", lo, hi);
    }
}

} // end quick_internal_test_package
