// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build goexperiment.regabiargs
namespace go;

using abi = @internal.abi_package;
using Δmath = math_package;
using rand = global::go.math.rand_package;
using Δreflect = reflect_package;
using Δruntime = runtime_package;
using Δtesting = testing_package;
using quick = global::go.testing.quick_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using global::go.math;
using global::go.testing;
using static global::go.reflect_internal_test_package;

partial class reflect_test_package {

// As of early May 2021 this is no longer necessary for amd64,
// but it remains in case this is needed for the next register abi port.
// TODO (1.18) If enabling register ABI on additional architectures turns out not to need this, remove it.
[GoType] partial struct MagicLastTypeNameForTestingRegisterABI {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string allRegsCallˢ = "AllRegsCall"u8;
internal static readonly @string regsAndStackCallˢ = "RegsAndStackCall"u8;
internal static readonly @string spillStructCallˢ = "SpillStructCall"u8;
internal static readonly @string valueRegMethodSpillIntˢ = "ValueRegMethodSpillInt"u8;
internal static readonly @string valueRegMethodSpillPtrˢ = "ValueRegMethodSpillPtr"u8;

public static void TestMethodValueCallABI(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Enable register-based reflect.Call and ensure we don't
        // use potentially incorrect cached versions by clearing
        // the cache before we start and after we're done.
        defer(ᴛ1 => reflect_internal_test_package.SetArgRegs(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), reflect_internal_test_package.SetArgRegs(abi.IntArgRegs, abi.FloatArgRegs, abi.EffectiveFloatRegSize), ref ᒐ);
        // This test is simple. Calling a method value involves
        // pretty much just plumbing whatever arguments in whichever
        // location through to reflectcall. They're already set up
        // for us, so there isn't a whole lot to do. Let's just
        // make sure that we can pass register and stack arguments
        // through. The exact combination is not super important.
        (ж<StructWithMethods>, any) makeMethodValue(@string method) {
            var sΔ1 = @new<StructWithMethods>();
            var v = Δreflect.ValueOf(sΔ1.OrTypedNil()).MethodByName(method);
            return (sΔ1, v.Interface());
        }
        var a0 = new StructFewRegs(
            10, 11, 12, 13,
            20.0D, 21.0D, 22.0D, 23.0D
        );
        var a1 = new uint64[]{100, 101, 102, 103}.array();
        var a2 = new StructFillRegs(
            1, 2, 3, 4, 5, 6, 7, 8, 9,
            1.0D, 2.0D, 3.0D, 4.0D, 5.0D, 6.0D, 7.0D, 8.0D, 9.0D, 10.0D, 11.0D, 12.0D, 13.0D, 14.0D, 15.0D
        );
        var (s, i) = makeMethodValue(allRegsCallˢ);
        var f0 = i._<Func<StructFewRegs, MagicLastTypeNameForTestingRegisterABI, StructFewRegs>>();
        var r0 = f0(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
        if (r0 != a0) {
            Ꮡt.Errorf("bad method value call: got %#v, want %#v"u8, r0, a0);
        }
        if ((~s).Value != 1) {
            Ꮡt.Errorf("bad method value call: failed to set s.Value: got %d, want %d"u8, (~s).Value, (nint)(1));
        }
        (s, i) = makeMethodValue(regsAndStackCallˢ);
        var f1 = i._<Func<StructFewRegs, array<uint64>, MagicLastTypeNameForTestingRegisterABI, (StructFewRegs, array<uint64>)>>();
        (r0, var r1) = f1(a0, a1, new MagicLastTypeNameForTestingRegisterABI(nil));
        if (r0 != a0) {
            Ꮡt.Errorf("bad method value call: got %#v, want %#v"u8, r0, a0);
        }
        if (r1 != a1) {
            Ꮡt.Errorf("bad method value call: got %#v, want %#v"u8, r1, a1);
        }
        if ((~s).Value != 2) {
            Ꮡt.Errorf("bad method value call: failed to set s.Value: got %d, want %d"u8, (~s).Value, (nint)(2));
        }
        (s, i) = makeMethodValue(spillStructCallˢ);
        var f2 = i._<Func<StructFillRegs, MagicLastTypeNameForTestingRegisterABI, StructFillRegs>>();
        var r2 = f2(a2, new MagicLastTypeNameForTestingRegisterABI(nil));
        if (r2 != a2) {
            Ꮡt.Errorf("bad method value call: got %#v, want %#v"u8, r2, a2);
        }
        if ((~s).Value != 3) {
            Ꮡt.Errorf("bad method value call: failed to set s.Value: got %d, want %d"u8, (~s).Value, (nint)(3));
        }
        (s, i) = makeMethodValue(valueRegMethodSpillIntˢ);
        var f3 = i._<Func<StructFillRegs, nint, MagicLastTypeNameForTestingRegisterABI, (StructFillRegs, nint)>>();
        var (r3a, r3b) = f3(a2, 42, new MagicLastTypeNameForTestingRegisterABI(nil));
        if (r3a != a2) {
            Ꮡt.Errorf("bad method value call: got %#v, want %#v"u8, r3a, a2);
        }
        if (r3b != 42) {
            Ꮡt.Errorf("bad method value call: got %#v, want %#v"u8, r3b, (nint)(42));
        }
        if ((~s).Value != 4) {
            Ꮡt.Errorf("bad method value call: failed to set s.Value: got %d, want %d"u8, (~s).Value, (nint)(4));
        }
        (s, i) = makeMethodValue(valueRegMethodSpillPtrˢ);
        var f4 = i._<Func<StructFillRegs, ж<byte>, MagicLastTypeNameForTestingRegisterABI, (StructFillRegs, ж<byte>)>>();
        ref var vb = ref heap<byte>(out var Ꮡvb);
        vb = (byte)10;
        var (r4a, r4b) = f4(a2, Ꮡvb, new MagicLastTypeNameForTestingRegisterABI(nil));
        if (r4a != a2) {
            Ꮡt.Errorf("bad method value call: got %#v, want %#v"u8, r4a, a2);
        }
        if (r4b != Ꮡvb) {
            Ꮡt.Errorf("bad method value call: got %#v, want %#v"u8, r4b.OrTypedNil(), Ꮡvb);
        }
        if ((~s).Value != 5) {
            Ꮡt.Errorf("bad method value call: failed to set s.Value: got %d, want %d"u8, (~s).Value, (nint)(5));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct StructWithMethods {
    public nint Value;
}

[GoType] partial struct StructFewRegs {
    internal nint a0, a1, a2, a3;
    internal float64 f0, f1, f2, f3;
}

[GoType] partial struct StructFillRegs {
    internal nint a0, a1, a2, a3, a4, a5, a6, a7, a8;
    internal float64 f0, f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14;
}

[GoRecv] public static StructFewRegs AllRegsCall(this ref StructWithMethods m, StructFewRegs s, MagicLastTypeNameForTestingRegisterABI _) {
    m.Value = 1;
    return s;
}

[GoRecv] public static (StructFewRegs, array<uint64>) RegsAndStackCall(this ref StructWithMethods m, StructFewRegs s, [GoArrayDims(4)] array<uint64> a, MagicLastTypeNameForTestingRegisterABI _) {
    a = a.Clone();

    m.Value = 2;
    return (s, a.Clone());
}

[GoRecv] public static StructFillRegs SpillStructCall(this ref StructWithMethods m, StructFillRegs s, MagicLastTypeNameForTestingRegisterABI _) {
    m.Value = 3;
    return s;
}

// When called as a method value, i is passed on the stack.
// When called as a method, i is passed in a register.
[GoRecv] public static (StructFillRegs, nint) ValueRegMethodSpillInt(this ref StructWithMethods m, StructFillRegs s, nint i, MagicLastTypeNameForTestingRegisterABI _) {
    m.Value = 4;
    return (s, i);
}

// When called as a method value, i is passed on the stack.
// When called as a method, i is passed in a register.
[GoRecv] public static (StructFillRegs, ж<byte>) ValueRegMethodSpillPtr(this ref StructWithMethods m, StructFillRegs s, ж<byte> Ꮡi, MagicLastTypeNameForTestingRegisterABI _) {
    m.Value = 5;
    return (s, Ꮡi);
}

public static void TestReflectCallABI(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Enable register-based reflect.Call and ensure we don't
        // use potentially incorrect cached versions by clearing
        // the cache before we start and after we're done.
        defer(ᴛ1 => reflect_internal_test_package.SetArgRegs(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), reflect_internal_test_package.SetArgRegs(abi.IntArgRegs, abi.FloatArgRegs, abi.EffectiveFloatRegSize), ref ᒐ);
        // Execute the functions defined below which all have the
        // same form and perform the same function: pass all arguments
        // to return values. The purpose is to test the call boundary
        // and make sure it works.
        var r = rand.New(rand.NewSource(genValueRandSeed));
        foreach (var (_, fn) in abiCallTestCases) {
            ref var fnΔ1 = ref heap<reflectꓸValue>(out var ᏑfnΔ1);
            fnΔ1 = Δreflect.ValueOf(fn);
            var fnʗ1 = fnΔ1;
            var rʗ1 = r;
            Ꮡt.Run(Δruntime.FuncForPC(fnΔ1.Pointer()).Name(), (ж<Δtesting.T> tΔ1) => {
                var typ = fnʗ1.Type();
                if (typ.Kind() != Δreflect.Func) {
                    tΔ1.Fatalf("test case is not a function, has type: %s"u8, typ.String());
                }
                if (typ.NumIn() != typ.NumOut()) {
                    tΔ1.Fatalf("test case has different number of inputs and outputs: %d in, %d out"u8, typ.NumIn(), typ.NumOut());
                }
                slice<reflectꓸValue> args = default!;
                for (nint i = 0; i < typ.NumIn(); i++) {
                    args = append(args, genValue(tΔ1, typ.In(i), rʗ1));
                }
                var results = fnʗ1.Call(args);
                foreach (var (i, _) in results) {
                    var (x, y) = (args[i].Interface(), results[i].Interface());
                    if (Δreflect.DeepEqual(x, y)) {
                        continue;
                    }
                    tΔ1.Errorf("arg and result %d differ: got %+v, want %+v"u8, i, y, x);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string onlyPointerInRegisterGCˢ = "OnlyPointerInRegisterGC"u8;

public static void TestReflectMakeFuncCallABI(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Enable register-based reflect.MakeFunc and ensure we don't
        // use potentially incorrect cached versions by clearing
        // the cache before we start and after we're done.
        defer(ᴛ1 => reflect_internal_test_package.SetArgRegs(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), reflect_internal_test_package.SetArgRegs(abi.IntArgRegs, abi.FloatArgRegs, abi.EffectiveFloatRegSize), ref ᒐ);
        // Execute the functions defined below which all have the
        // same form and perform the same function: pass all arguments
        // to return values. The purpose is to test the call boundary
        // and make sure it works.
        var r = rand.New(rand.NewSource(genValueRandSeed));
        var makeFuncHandler = (slice<reflectꓸValue> args) => {
            if (len(args) == 0) {
                return new reflectꓸValue[]{}.slice();
            }
            return args.slice(0, len(args) - 1); // The last Value is an empty magic value.
        };
        foreach (var (_, callFn) in abiMakeFuncTestCases) {
            var fnTyp = Δreflect.TypeOf(callFn).In(0);
            ref var fn = ref heap<reflectꓸValue>(out var Ꮡfn);
            fn = Δreflect.MakeFunc(fnTyp, makeFuncHandler);
            ref var callFnΔ1 = ref heap<reflectꓸValue>(out var ᏑcallFnΔ1);
            callFnΔ1 = Δreflect.ValueOf(callFn);
            var callFnʗ1 = callFnΔ1;
            var fnʗ1 = fn;
            var fnTypʗ1 = fnTyp;
            var rʗ1 = r;
            Ꮡt.Run(Δruntime.FuncForPC(callFnΔ1.Pointer()).Name(), (ж<Δtesting.T> tΔ1) => {
                var args = new reflectꓸValue[]{fnʗ1}.slice();
                for (nint i = 0; i < fnTypʗ1.NumIn() - 1; i++) {
                    /* last one is magic type */
                    args = append(args, genValue(tΔ1, fnTypʗ1.In(i), rʗ1));
                }
                var results = callFnʗ1.Call(args);
                foreach (var (i, _) in results) {
                    var (x, y) = (args[i + 1].Interface(), results[i].Interface());
                    if (Δreflect.DeepEqual(x, y)) {
                        continue;
                    }
                    tΔ1.Errorf("arg and result %d differ: got %+v, want %+v"u8, i, y, x);
                }
            });
        }
        Ꮡt.Run(onlyPointerInRegisterGCˢ, (ж<Δtesting.T> tΔ2) => {
            // This test attempts to induce a failure wherein
            // the last pointer to an object is passed via registers.
            // If makeFuncStub doesn't successfully store the pointer
            // to a location visible to the GC, the object should be
            // freed and then the next GC should notice that an object
            // was inexplicably revived.
            Func<ж<uint64>, MagicLastTypeNameForTestingRegisterABI, ж<uint64>> f = default!;
            var mkfn = Δreflect.MakeFunc(Δreflect.TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> args) => {
                (args[0].Interface()._<ж<uint64>>()).Value = 5;
                return args[..1];
            });
            var fn = mkfn.Interface()._<Func<ж<uint64>, MagicLastTypeNameForTestingRegisterABI, ж<uint64>>>();
            // Call the MakeFunc'd function while trying pass the only pointer
            // to a new heap-allocated uint64.
            reflect_internal_test_package.CallGC.Value = true;
            var x = fn(@new<uint64>(), new MagicLastTypeNameForTestingRegisterABI(nil));
            reflect_internal_test_package.CallGC.Value = false;
            // Check for bad pointers (which should be x if things went wrong).
            Δruntime.GC();
            // Sanity check x.
            if (x.Value != 5) {
                tΔ2.Fatalf("failed to set value in object"u8);
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TODO(mknyszek): Test passing interface values.
// TODO(mknyszek): Test passing unsafe.Pointer values.
// TODO(mknyszek): Test passing chan values.
internal static slice<any> abiCallTestCases = new any[]{
    passNone,
    passInt,
    passInt8,
    passInt16,
    passInt32,
    passInt64,
    passUint,
    passUint8,
    passUint16,
    passUint32,
    passUint64,
    passFloat32,
    passFloat64,
    passComplex64,
    passComplex128,
    passManyInt,
    passManyFloat64,
    passArray1,
    passArray,
    passArray1Mix,
    passString,
    passSlice,
    passPointer,
    passStruct1,
    passStruct2,
    passStruct3,
    passStruct4,
    passStruct5,
    passStruct6,
    passStruct7,
    passStruct8,
    passStruct9,
    passStruct10,
    passStruct11,
    passStruct12,
    passStruct13,
    passStruct14,
    passStruct15,
    pass2Struct1,
    passEmptyStruct,
    passStruct10AndSmall
}.slice();

// Functions for testing reflect function call functionality.

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static void passNone() {
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static nint passInt(nint a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static int8 passInt8(int8 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static int16 passInt16(int16 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static int32 passInt32(int32 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static int64 passInt64(int64 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static nuint passUint(nuint a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static uint8 passUint8(uint8 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static uint16 passUint16(uint16 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static uint32 passUint32(uint32 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static uint64 passUint64(uint64 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static float32 passFloat32(float32 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static float64 passFloat64(float64 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static complex64 passComplex64(complex64 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static complex128 passComplex128(complex128 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static array<uint32> passArray1([GoArrayDims(1)] array<uint32> a) {
    a = a.Clone();

    return a.Clone();
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static array<uintptr> passArray([GoArrayDims(2)] array<uintptr> a) {
    a = a.Clone();

    return a.Clone();
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (nint, array<uint32>, float64) passArray1Mix(nint a, [GoArrayDims(1)] array<uint32> b, float64 c) {
    b = b.Clone();

    return (a, b.Clone(), c);
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static @string passString(@string a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static slice<byte> passSlice(slice<byte> a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static ж<byte> passPointer(ж<byte> Ꮡa) {
    return Ꮡa;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (nint, nint, nint, nint, nint, nint, nint, nint, nint, nint) passManyInt(nint a, nint b, nint c, nint d, nint e, nint f, nint g, nint h, nint i, nint j) {
    return (a, b, c, d, e, f, g, h, i, j);
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64) passManyFloat64(float64 a, float64 b, float64 c, float64 d, float64 e, float64 f, float64 g, float64 h, float64 i, float64 j, float64 l, float64 m, float64 n, float64 o, float64 p, float64 q, float64 r, float64 s, float64 t) {
    return (a, b, c, d, e, f, g, h, i, j, l, m, n, o, p, q, r, s, t);
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct1 passStruct1(Struct1 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct2 passStruct2(Struct2 a) {
    a = a.ΔClone();

    return a.ΔClone();
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct3 passStruct3(Struct3 a) {
    a = a.ΔClone();

    return a.ΔClone();
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct4 passStruct4(Struct4 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct5 passStruct5(Struct5 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct6 passStruct6(Struct6 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct7 passStruct7(Struct7 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct8 passStruct8(Struct8 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct9 passStruct9(Struct9 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct10 passStruct10(Struct10 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct11 passStruct11(Struct11 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct12 passStruct12(Struct12 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct13 passStruct13(Struct13 a) {
    return a;
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct14 passStruct14(Struct14 a) {
    a = a.ΔClone();

    return a.ΔClone();
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct15 passStruct15(Struct15 a) {
    a = a.ΔClone();

    return a.ΔClone();
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (Struct1 x, Struct1 y) pass2Struct1(Struct1 a, Struct1 b) {
    return (a, b);
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (nint, EmptyStruct, float64) passEmptyStruct(nint a, EmptyStruct b, float64 c) {
    return (a, b, c);
}

// This test case forces a large argument to the stack followed by more
// in-register arguments.
//
//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (Struct10, byte, nuint) passStruct10AndSmall(Struct10 a, byte b, nuint c) {
    return (a, b, c);
}

// TODO(mknyszek): Test callArgsing interface values.
// TODO(mknyszek): Test callArgsing unsafe.Pointer values.
// TODO(mknyszek): Test callArgsing chan values.
internal static slice<any> abiMakeFuncTestCases = new any[]{
    callArgsNone,
    callArgsInt,
    callArgsInt8,
    callArgsInt16,
    callArgsInt32,
    callArgsInt64,
    callArgsUint,
    callArgsUint8,
    callArgsUint16,
    callArgsUint32,
    callArgsUint64,
    callArgsFloat32,
    callArgsFloat64,
    callArgsComplex64,
    callArgsComplex128,
    callArgsManyInt,
    callArgsManyFloat64,
    callArgsArray1,
    callArgsArray,
    callArgsArray1Mix,
    callArgsString,
    callArgsSlice,
    callArgsPointer,
    callArgsStruct1,
    callArgsStruct2,
    callArgsStruct3,
    callArgsStruct4,
    callArgsStruct5,
    callArgsStruct6,
    callArgsStruct7,
    callArgsStruct8,
    callArgsStruct9,
    callArgsStruct10,
    callArgsStruct11,
    callArgsStruct12,
    callArgsStruct13,
    callArgsStruct14,
    callArgsStruct15,
    callArgs2Struct1,
    callArgsEmptyStruct
}.slice();

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static void callArgsNone(Action<MagicLastTypeNameForTestingRegisterABI> f) {
    f(new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static nint callArgsInt(Func<nint, MagicLastTypeNameForTestingRegisterABI, nint> f, nint a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static int8 callArgsInt8(Func<int8, MagicLastTypeNameForTestingRegisterABI, int8> f, int8 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static int16 callArgsInt16(Func<int16, MagicLastTypeNameForTestingRegisterABI, int16> f, int16 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static int32 callArgsInt32(Func<int32, MagicLastTypeNameForTestingRegisterABI, int32> f, int32 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static int64 callArgsInt64(Func<int64, MagicLastTypeNameForTestingRegisterABI, int64> f, int64 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static nuint callArgsUint(Func<nuint, MagicLastTypeNameForTestingRegisterABI, nuint> f, nuint a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static uint8 callArgsUint8(Func<uint8, MagicLastTypeNameForTestingRegisterABI, uint8> f, uint8 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static uint16 callArgsUint16(Func<uint16, MagicLastTypeNameForTestingRegisterABI, uint16> f, uint16 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static uint32 callArgsUint32(Func<uint32, MagicLastTypeNameForTestingRegisterABI, uint32> f, uint32 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static uint64 callArgsUint64(Func<uint64, MagicLastTypeNameForTestingRegisterABI, uint64> f, uint64 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static float32 callArgsFloat32(Func<float32, MagicLastTypeNameForTestingRegisterABI, float32> f, float32 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static float64 callArgsFloat64(Func<float64, MagicLastTypeNameForTestingRegisterABI, float64> f, float64 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static complex64 callArgsComplex64(Func<complex64, MagicLastTypeNameForTestingRegisterABI, complex64> f, complex64 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static complex128 callArgsComplex128(Func<complex128, MagicLastTypeNameForTestingRegisterABI, complex128> f, complex128 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static array<uint32> callArgsArray1(Func<array<uint32>, MagicLastTypeNameForTestingRegisterABI, array<uint32>> f, [GoArrayDims(1)] array<uint32> a0) {
    a0 = a0.Clone();

    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static array<uintptr> callArgsArray(Func<array<uintptr>, MagicLastTypeNameForTestingRegisterABI, array<uintptr>> f, [GoArrayDims(2)] array<uintptr> a0) {
    a0 = a0.Clone();

    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (nint, array<uint32>, float64) callArgsArray1Mix(Func<nint, array<uint32>, float64, MagicLastTypeNameForTestingRegisterABI, (nint, array<uint32>, float64)> f, nint a0, [GoArrayDims(1)] array<uint32> a1, float64 a2) {
    a1 = a1.Clone();

    return f(a0, a1, a2, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static @string callArgsString(Func<@string, MagicLastTypeNameForTestingRegisterABI, @string> f, @string a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static slice<byte> callArgsSlice(Func<slice<byte>, MagicLastTypeNameForTestingRegisterABI, slice<byte>> f, slice<byte> a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static ж<byte> callArgsPointer(Func<ж<byte>, MagicLastTypeNameForTestingRegisterABI, ж<byte>> f, ж<byte> Ꮡa0) {
    return f(Ꮡa0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (nint, nint, nint, nint, nint, nint, nint, nint, nint, nint) callArgsManyInt(Func<nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, MagicLastTypeNameForTestingRegisterABI, (nint, nint, nint, nint, nint, nint, nint, nint, nint, nint)> f, nint a0, nint a1, nint a2, nint a3, nint a4, nint a5, nint a6, nint a7, nint a8, nint a9) {
    return f(a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (float64 r0, float64 r1, float64 r2, float64 r3, float64 r4, float64 r5, float64 r6, float64 r7, float64 r8, float64 r9, float64 r10, float64 r11, float64 r12, float64 r13, float64 r14, float64 r15, float64 r16, float64 r17, float64 r18) callArgsManyFloat64(Func<float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, MagicLastTypeNameForTestingRegisterABI, (float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64, float64)> f, float64 a0, float64 a1, float64 a2, float64 a3, float64 a4, float64 a5, float64 a6, float64 a7, float64 a8, float64 a9, float64 a10, float64 a11, float64 a12, float64 a13, float64 a14, float64 a15, float64 a16, float64 a17, float64 a18) {
    return f(a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17, a18, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct1 callArgsStruct1(Func<Struct1, MagicLastTypeNameForTestingRegisterABI, Struct1> f, Struct1 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct2 callArgsStruct2(Func<Struct2, MagicLastTypeNameForTestingRegisterABI, Struct2> f, Struct2 a0) {
    a0 = a0.ΔClone();

    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct3 callArgsStruct3(Func<Struct3, MagicLastTypeNameForTestingRegisterABI, Struct3> f, Struct3 a0) {
    a0 = a0.ΔClone();

    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct4 callArgsStruct4(Func<Struct4, MagicLastTypeNameForTestingRegisterABI, Struct4> f, Struct4 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct5 callArgsStruct5(Func<Struct5, MagicLastTypeNameForTestingRegisterABI, Struct5> f, Struct5 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct6 callArgsStruct6(Func<Struct6, MagicLastTypeNameForTestingRegisterABI, Struct6> f, Struct6 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct7 callArgsStruct7(Func<Struct7, MagicLastTypeNameForTestingRegisterABI, Struct7> f, Struct7 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct8 callArgsStruct8(Func<Struct8, MagicLastTypeNameForTestingRegisterABI, Struct8> f, Struct8 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct9 callArgsStruct9(Func<Struct9, MagicLastTypeNameForTestingRegisterABI, Struct9> f, Struct9 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct10 callArgsStruct10(Func<Struct10, MagicLastTypeNameForTestingRegisterABI, Struct10> f, Struct10 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct11 callArgsStruct11(Func<Struct11, MagicLastTypeNameForTestingRegisterABI, Struct11> f, Struct11 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct12 callArgsStruct12(Func<Struct12, MagicLastTypeNameForTestingRegisterABI, Struct12> f, Struct12 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct13 callArgsStruct13(Func<Struct13, MagicLastTypeNameForTestingRegisterABI, Struct13> f, Struct13 a0) {
    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct14 callArgsStruct14(Func<Struct14, MagicLastTypeNameForTestingRegisterABI, Struct14> f, Struct14 a0) {
    a0 = a0.ΔClone();

    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static Struct15 callArgsStruct15(Func<Struct15, MagicLastTypeNameForTestingRegisterABI, Struct15> f, Struct15 a0) {
    a0 = a0.ΔClone();

    return f(a0, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (Struct1 r0, Struct1 r1) callArgs2Struct1(Func<Struct1, Struct1, MagicLastTypeNameForTestingRegisterABI, (Struct1, Struct1)> f, Struct1 a0, Struct1 a1) {
    return f(a0, a1, new MagicLastTypeNameForTestingRegisterABI(nil));
}

//go:registerparams
//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static (nint, EmptyStruct, float64) callArgsEmptyStruct(Func<nint, EmptyStruct, float64, MagicLastTypeNameForTestingRegisterABI, (nint, EmptyStruct, float64)> f, nint a0, EmptyStruct a1, float64 a2) {
    return f(a0, a1, a2, new MagicLastTypeNameForTestingRegisterABI(nil));
}

// Struct1 is a simple integer-only aggregate struct.
[GoType] partial struct Struct1 {
    public nuint A, B, C;
}

// Struct2 is Struct1 but with an array-typed field that will
// force it to get passed on the stack.
[GoType] partial struct Struct2 {
    public nuint A, B, C;
    public array<uint32> D = new(2);
}

// Struct3 is Struct2 but with an anonymous array-typed field.
// This should act identically to Struct2.
[GoType] partial struct Struct3 {
    public nuint A, B, C;
    public array<uint32> D = new(2);
}

// Struct4 has byte-length fields that should
// each use up a whole registers.
[GoType] partial struct Struct4 {
    public int8 A, B;
    public uint8 C, D;
    public bool E;
}

// Struct5 is a relatively large struct
// with both integer and floating point values.
[GoType] partial struct Struct5 {
    public uint16 A;
    public int16 B;
    public uint32 C, D;
    public int32 E;
    public float32 F, G, H, I, J;
}

// Struct6 has a nested struct.
[GoType] partial struct Struct6 {
    public partial ref Struct1 Struct1 { get; }
}

// Struct7 is a struct with a nested array-typed field
// that cannot be passed in registers as a result.
[GoType] partial struct Struct7 {
    public partial ref Struct1 Struct1 { get; }
    public partial ref Struct2 Struct2 { get; }
}

// Struct8 is large aggregate struct type that may be
// passed in registers.
[GoType] partial struct Struct8 {
    public partial ref Struct5 Struct5 { get; }
    public partial ref Struct1 Struct1 { get; }
}

// Struct9 is a type that has an array type nested
// 2 layers deep, and as a result needs to be passed
// on the stack.
[GoType] partial struct Struct9 {
    public partial ref Struct1 Struct1 { get; }
    public partial ref Struct7 Struct7 { get; }
}

// Struct10 is a struct type that is too large to be
// passed in registers.
[GoType] partial struct Struct10 {
    public partial ref Struct5 Struct5 { get; }
    public partial ref Struct8 Struct8 { get; }
}

// Struct11 is a struct type that has several reference
// types in it.
[GoType] partial struct Struct11 {
    public map<@string, nint> X;
}

// Struct12 has Struct11 embedded into it to test more
// paths.
[GoType] partial struct Struct12 {
    public nint A;
    public partial ref Struct11 Struct11 { get; }
}

// Struct13 tests an empty field.
[GoType] [StructLayout(LayoutKind.Explicit, Size = 16)] partial struct Struct13 {
    [FieldOffset(0)] public nint A;
    [FieldOffset(8)] public readonly EmptyStruct X;
    [FieldOffset(8)] public nint B;
}

// Struct14 tests a non-zero-sized (and otherwise register-assignable)
// struct with a field that is a non-zero length array with zero-sized members.
[GoType] partial struct Struct14 {
    public uintptr A;
    public array<EmptyStruct> X = new(3);
    public float64 B;
}

[GoType("dyn")] partial struct Struct15_X {
    public array<EmptyStruct> Y = new(3);
}

// Struct15 tests a non-zero-sized (and otherwise register-assignable)
// struct with a struct field that is zero-sized but contains a
// non-zero length array with zero-sized members.
[GoType] partial struct Struct15 {
    public uintptr A;
    public Struct15_X X;
    public float64 B;
}

internal static UntypedInt genValueRandSeed => 0;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToGenerateValueˢ = (@string)"failed to generate value"u8;

// genValue generates a pseudorandom reflect.Value with type t.
// The reflect.Value produced by this function is always the same
// for the same type.
internal static reflectꓸValue genValue(ж<Δtesting.T> Ꮡt, reflectꓸType typ, ж<rand.Rand> Ꮡr) {
    // Re-seed and reset the PRNG because we want each value with the
    // same type to be the same random value.
    Ꮡr.Seed(genValueRandSeed);
    var (v, ok) = quick.Value(typ, Ꮡr);
    if (!ok) {
        Ꮡt.Fatal(failedToGenerateValueˢ);
    }
    return v;
}

public static void TestSignalingNaNArgument(ж<Δtesting.T> Ꮡt) {
    var v = Δreflect.ValueOf((float32 x) => {
        // make sure x is a signaling NaN.
        var u = Δmath.Float32bits(x);
        if (u != snan) {
            Ꮡt.Fatalf("signaling NaN not correct: %x\n"u8, u);
        }
    });
    v.Call(new reflectꓸValue[]{Δreflect.ValueOf(Δmath.Float32frombits(snan))}.slice());
}

public static void TestSignalingNaNReturn(ж<Δtesting.T> Ꮡt) {
    var v = Δreflect.ValueOf(float32 () => Δmath.Float32frombits(snan));
    ref var x = ref heap(new float32(), out var Ꮡx);
    Δreflect.ValueOf(Ꮡx).Elem().Set(v.Call(default!)[0]);
    // make sure x is a signaling NaN.
    var u = Δmath.Float32bits(x);
    if (u != snan) {
        Ꮡt.Fatalf("signaling NaN not correct: %x\n"u8, u);
    }
}

} // end reflect_test_package
