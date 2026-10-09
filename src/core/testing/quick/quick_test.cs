// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using rand = go.math.rand_package;
using reflect = reflect_package;
using testing = testing_package;
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
        return default!;
    }
    ref var b = ref heap<nint>(out var Ꮡb);
    b = a;
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

internal static void reportError(@string property, error err, ж<testing.T> Ꮡt) {
    if (err != default!) {
        Ꮡt.Errorf("%s: %s"u8, property, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fArrayˢ = "fArray"u8;
internal static readonly @string fArrayAliasˢ = "fArrayAlias"u8;
internal static readonly @string fBoolˢ = "fBool"u8;
internal static readonly @string fBoolAliasˢ = "fBoolAlias"u8;
internal static readonly @string fFloat32ˢ = "fFloat32"u8;
internal static readonly @string fFloat32Aliasˢ = "fFloat32Alias"u8;
internal static readonly @string fFloat64ˢ = "fFloat64"u8;
internal static readonly @string fFloat64Aliasˢ = "fFloat64Alias"u8;
internal static readonly @string fComplex64ˢ = "fComplex64"u8;
internal static readonly @string fComplex64Aliasˢ = "fComplex64Alias"u8;
internal static readonly @string fComplex128ˢ = "fComplex128"u8;
internal static readonly @string fComplex128Aliasˢ = "fComplex128Alias"u8;
internal static readonly @string fInt16ˢ = "fInt16"u8;
internal static readonly @string fInt16Aliasˢ = "fInt16Alias"u8;
internal static readonly @string fInt32ˢ = "fInt32"u8;
internal static readonly @string fInt32Aliasˢ = "fInt32Alias"u8;
internal static readonly @string fInt64ˢ = "fInt64"u8;
internal static readonly @string fInt64Aliasˢ = "fInt64Alias"u8;
internal static readonly @string fInt8ˢ = "fInt8"u8;
internal static readonly @string fInt8Aliasˢ = "fInt8Alias"u8;
internal static readonly @string fIntˢ = "fInt"u8;
internal static readonly @string fIntAliasˢ = "fIntAlias"u8;
internal static readonly @string fMapˢ = "fMap"u8;
internal static readonly @string fMapAliasˢ = "fMapAlias"u8;
internal static readonly @string fPtrˢ = "fPtr"u8;
internal static readonly @string fPtrAliasˢ = "fPtrAlias"u8;
internal static readonly @string fSliceˢ = "fSlice"u8;
internal static readonly @string fSliceAliasˢ = "fSliceAlias"u8;
internal static readonly @string fStringˢ = "fString"u8;
internal static readonly @string fStringAliasˢ = "fStringAlias"u8;
internal static readonly @string fStructˢ = "fStruct"u8;
internal static readonly @string fStructAliasˢ = "fStructAlias"u8;
internal static readonly @string fUint16ˢ = "fUint16"u8;
internal static readonly @string fUint16Aliasˢ = "fUint16Alias"u8;
internal static readonly @string fUint32ˢ = "fUint32"u8;
internal static readonly @string fUint32Aliasˢ = "fUint32Alias"u8;
internal static readonly @string fUint64ˢ = "fUint64"u8;
internal static readonly @string fUint64Aliasˢ = "fUint64Alias"u8;
internal static readonly @string fUint8ˢ = "fUint8"u8;
internal static readonly @string fUint8Aliasˢ = "fUint8Alias"u8;
internal static readonly @string fUintˢ = "fUint"u8;
internal static readonly @string fUintAliasˢ = "fUintAlias"u8;
internal static readonly @string fUintptrˢ = "fUintptr"u8;
internal static readonly @string fUintptrAliasˢ = "fUintptrAlias"u8;

public static void TestCheckEqual(ж<testing.T> Ꮡt) {
    reportError(fArrayˢ, CheckEqual(fArray, fArray, nil), Ꮡt);
    reportError(fArrayAliasˢ, CheckEqual(fArrayAlias, fArrayAlias, nil), Ꮡt);
    reportError(fBoolˢ, CheckEqual(fBool, fBool, nil), Ꮡt);
    reportError(fBoolAliasˢ, CheckEqual(fBoolAlias, fBoolAlias, nil), Ꮡt);
    reportError(fFloat32ˢ, CheckEqual(fFloat32, fFloat32, nil), Ꮡt);
    reportError(fFloat32Aliasˢ, CheckEqual(fFloat32Alias, fFloat32Alias, nil), Ꮡt);
    reportError(fFloat64ˢ, CheckEqual(fFloat64, fFloat64, nil), Ꮡt);
    reportError(fFloat64Aliasˢ, CheckEqual(fFloat64Alias, fFloat64Alias, nil), Ꮡt);
    reportError(fComplex64ˢ, CheckEqual(fComplex64, fComplex64, nil), Ꮡt);
    reportError(fComplex64Aliasˢ, CheckEqual(fComplex64Alias, fComplex64Alias, nil), Ꮡt);
    reportError(fComplex128ˢ, CheckEqual(fComplex128, fComplex128, nil), Ꮡt);
    reportError(fComplex128Aliasˢ, CheckEqual(fComplex128Alias, fComplex128Alias, nil), Ꮡt);
    reportError(fInt16ˢ, CheckEqual(fInt16, fInt16, nil), Ꮡt);
    reportError(fInt16Aliasˢ, CheckEqual(fInt16Alias, fInt16Alias, nil), Ꮡt);
    reportError(fInt32ˢ, CheckEqual(fInt32, fInt32, nil), Ꮡt);
    reportError(fInt32Aliasˢ, CheckEqual(fInt32Alias, fInt32Alias, nil), Ꮡt);
    reportError(fInt64ˢ, CheckEqual(fInt64, fInt64, nil), Ꮡt);
    reportError(fInt64Aliasˢ, CheckEqual(fInt64Alias, fInt64Alias, nil), Ꮡt);
    reportError(fInt8ˢ, CheckEqual(fInt8, fInt8, nil), Ꮡt);
    reportError(fInt8Aliasˢ, CheckEqual(fInt8Alias, fInt8Alias, nil), Ꮡt);
    reportError(fIntˢ, CheckEqual(fInt, fInt, nil), Ꮡt);
    reportError(fIntAliasˢ, CheckEqual(fIntAlias, fIntAlias, nil), Ꮡt);
    reportError(fInt32ˢ, CheckEqual(fInt32, fInt32, nil), Ꮡt);
    reportError(fInt32Aliasˢ, CheckEqual(fInt32Alias, fInt32Alias, nil), Ꮡt);
    reportError(fMapˢ, CheckEqual(fMap, fMap, nil), Ꮡt);
    reportError(fMapAliasˢ, CheckEqual(fMapAlias, fMapAlias, nil), Ꮡt);
    reportError(fPtrˢ, CheckEqual(fPtr, fPtr, nil), Ꮡt);
    reportError(fPtrAliasˢ, CheckEqual(fPtrAlias, fPtrAlias, nil), Ꮡt);
    reportError(fSliceˢ, CheckEqual(fSlice, fSlice, nil), Ꮡt);
    reportError(fSliceAliasˢ, CheckEqual(fSliceAlias, fSliceAlias, nil), Ꮡt);
    reportError(fStringˢ, CheckEqual(fString, fString, nil), Ꮡt);
    reportError(fStringAliasˢ, CheckEqual(fStringAlias, fStringAlias, nil), Ꮡt);
    reportError(fStructˢ, CheckEqual(fStruct, fStruct, nil), Ꮡt);
    reportError(fStructAliasˢ, CheckEqual(fStructAlias, fStructAlias, nil), Ꮡt);
    reportError(fUint16ˢ, CheckEqual(fUint16, fUint16, nil), Ꮡt);
    reportError(fUint16Aliasˢ, CheckEqual(fUint16Alias, fUint16Alias, nil), Ꮡt);
    reportError(fUint32ˢ, CheckEqual(fUint32, fUint32, nil), Ꮡt);
    reportError(fUint32Aliasˢ, CheckEqual(fUint32Alias, fUint32Alias, nil), Ꮡt);
    reportError(fUint64ˢ, CheckEqual(fUint64, fUint64, nil), Ꮡt);
    reportError(fUint64Aliasˢ, CheckEqual(fUint64Alias, fUint64Alias, nil), Ꮡt);
    reportError(fUint8ˢ, CheckEqual(fUint8, fUint8, nil), Ꮡt);
    reportError(fUint8Aliasˢ, CheckEqual(fUint8Alias, fUint8Alias, nil), Ꮡt);
    reportError(fUintˢ, CheckEqual(fUint, fUint, nil), Ꮡt);
    reportError(fUintAliasˢ, CheckEqual(fUintAlias, fUintAlias, nil), Ꮡt);
    reportError(fUintptrˢ, CheckEqual(fUintptr, fUintptr, nil), Ꮡt);
    reportError(fUintptrAliasˢ, CheckEqual(fUintptrAlias, fUintptrAlias, nil), Ꮡt);
}

// This tests that ArbitraryValue is working by checking that all the arbitrary
// values of type MyStruct have x = 42.
internal partial struct myStruct {
    internal nint x;
}

internal static reflectꓸValue Generate(this myStruct m, ж<rand.Rand> Ꮡr, nint _) {
    return reflect.ValueOf(new myStruct(x: 42));
}

internal static bool myStructProperty(myStruct @in) {
    return @in.x == 42;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myStructPropertyˢ = "myStructProperty"u8;

public static void TestCheckProperty(ж<testing.T> Ꮡt) {
    reportError(myStructPropertyˢ, Check(myStructProperty, nil), Ꮡt);
}

public static void TestFailure(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var f = (nint x) => false;
    var err = Check((f).OrTypedNilFunc(), nil);
    if (err == default!) {
        Ꮡt.Errorf("Check didn't return an error"u8);
    }
    {
        var (_, ok) = err._<ж<global::go.testing.quick_package.CheckError>>(ᐧ); if (!ok) {
            Ꮡt.Errorf("Error was not a CheckError: %s"u8, err);
        }
    }
    err = CheckEqual(fUint, fUint32, nil);
    if (err == default!) {
        Ꮡt.Errorf("#1 CheckEqual didn't return an error"u8);
    }
    {
        var (_, ok) = err._<SetupError>(ᐧ); if (!ok) {
            Ꮡt.Errorf("#1 Error was not a SetupError: %s"u8, err);
        }
    }
    err = CheckEqual((nint x, nint y) => {
    }, (nint x) => {
    }, nil);
    if (err == default!) {
        Ꮡt.Errorf("#2 CheckEqual didn't return an error"u8);
    }
    {
        var (_, ok) = err._<SetupError>(ᐧ); if (!ok) {
            Ꮡt.Errorf("#2 Error was not a SetupError: %s"u8, err);
        }
    }
    err = CheckEqual(nint (nint x) => 0, int32 (nint x) => 0, nil);
    if (err == default!) {
        Ꮡt.Errorf("#3 CheckEqual didn't return an error"u8);
    }
    {
        var (_, ok) = err._<SetupError>(ᐧ); if (!ok) {
            Ꮡt.Errorf("#3 Error was not a SetupError: %s"u8, err);
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
public static void TestRecursive(ж<testing.T> Ꮡt) {
    var f = (TestRecursive_R r) => true;
    Check((f).OrTypedNilFunc(), nil);
}

public static void TestEmptyStruct(ж<testing.T> Ꮡt) {
    var f = (EmptyStruct _) => true;
    Check((f).OrTypedNilFunc(), nil);
}

public partial struct A {
    public ж<B> B;
}

public partial struct B {
    public ж<A> A;
}

public static void TestMutuallyRecursive(ж<testing.T> Ꮡt) {
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
public static void TestNonZeroSliceAndMap(ж<testing.T> Ꮡt) {
    var f = (TestNonZeroSliceAndMap_Q q) => q.M != default! && q.S != default!;
    var err = Check((f).OrTypedNilFunc(), nil);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

public static void TestInt64(ж<testing.T> Ꮡt) {
    int64 lo = default!;
    int64 hi = default!;
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
    if (((uint64)lo >> (int)(62)) == 0 || ((uint64)hi >> (int)(62)) == 0) {
        Ꮡt.Errorf("int64 returned range %#016x,%#016x; does not look like full range"u8, lo, hi);
    }
}

} // end quick_internal_test_package
