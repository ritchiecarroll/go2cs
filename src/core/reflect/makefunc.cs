// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// MakeFunc implementation.
namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;
using Δsync = sync_package;

partial class reflect_package {

// makeFuncImpl is the closure value implementing the function
// returned by MakeFunc.
// The first three words of this type must be kept in sync with
// methodValue and runtime.reflectMethodValue.
// Any changes should be reflected in all three.
[GoType] partial struct makeFuncImpl {
    internal partial ref makeFuncCtxt makeFuncCtxt { get; }
    internal ж<funcType> ftyp;
    internal Func<slice<ΔValue>, slice<ΔValue>> fn;
}

// go2cs generated this placeholder — func MakeFunc is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// makeFuncImpl contains a stack map for use by the runtime

// makeFuncStub is an assembly function that is the code half of
// the function returned from MakeFunc. It expects a *callReflectFunc
// as its context register, and its job is to invoke callReflect(ctxt, frame)
// where ctxt is the context register and frame is a pointer to the first
// word in the passed-in argument frame.
internal static partial void makeFuncStub();

// The first 3 words of this type must be kept in sync with
// makeFuncImpl and runtime.reflectMethodValue.
// Any changes should be reflected in all three.
[GoType] partial struct methodValue {
    internal partial ref makeFuncCtxt makeFuncCtxt { get; }
    internal nint method;
    internal ΔValue rcvr;
}

// makeMethodValue converts v from the rcvr+method index representation
// of a method value to an actual method func value, which is
// basically the receiver value with a special bit set, into a true
// func value - a value holding an actual func. The output is
// semantically equivalent to the input as far as the user of package
// reflect can tell, but the true func representation can be handled
// by code like Convert and Interface and Assign.
internal static ΔValue makeMethodValue(@string op, ΔValue v) {
    if ((flag)(v.flag & flagMethod) == 0) {
        throw panic("reflect: internal error: invalid use of makeMethodValue");
    }
    // Ignoring the flagMethod bit, v describes the receiver, not the method type.
    var fl = (flag)(v.flag & ((flag)((flag)(flagRO | flagAddr) | flagIndir)));
    fl |= (flag)(((flag)(uintptr)(uint8)v.typ().Kind()));
    ref var rcvr = ref heap<ΔValue>(out var Ꮡrcvr);
    rcvr = new ΔValue(v.typ(), v.ptr, fl);
    // v.Type returns the actual type of the method value.
    var ftyp = v.Type()._<ж<rtype>>().Reinterpret<rtype, abi.Type>().FuncType();
    var code = methodValueCallCodePtr();
    // methodValue contains a stack map for use by the runtime
    var (_, _, abid) = funcLayout(ftyp, nil);
    var fv = Ꮡ(new methodValue(
        makeFuncCtxt: new makeFuncCtxt(
            fn: code,
            stack: abid.stackPtrs,
            argLen: abid.stackCallArgsSize,
            regPtrs: abid.inRegPtrs.Clone()
        ),
        method: ((nint)(uintptr)v.flag >> (int)(flagMethodShift)),
        rcvr: rcvr
    ));
    // Cause panic if method is not appropriate.
    // The panic would still happen during the call if we omit this,
    // but we want Interface() and other operations to fail early.
    methodReceiver(op, (~fv).rcvr, (~fv).method);
    return new ΔValue(ftyp.of(funcType.ᏑType).Common(), new @unsafe.Pointer(fv), (flag)((flag)(v.flag & flagRO) | ((flag)(uintptr)(nuint)Func)));
}

internal static uintptr methodValueCallCodePtr() {
    return abi.FuncPCABI0(methodValueCall);
}

// methodValueCall is an assembly function that is the code half of
// the function returned from makeMethodValue. It expects a *methodValue
// as its context register, and its job is to invoke callMethod(ctxt, frame)
// where ctxt is the context register and frame is a pointer to the first
// word in the passed-in argument frame.
internal static partial void methodValueCall();

// This structure must be kept in sync with runtime.reflectMethodValue.
// Any changes should be reflected in all both.
[GoType] partial struct makeFuncCtxt {
    internal uintptr fn;
    internal ж<bitVector> stack; // ptrmap for both stack args and results
    internal uintptr argLen;    // just args
    internal abi.IntArgRegBitmap regPtrs;
}

// moveMakeFuncArgPtrs uses ctxt.regPtrs to copy integer pointer arguments
// in args.Ints to args.Ptrs where the GC can see them.
//
// This is similar to what reflectcallmove does in the runtime, except
// that happens on the return path, whereas this happens on the call path.
//
// nosplit because pointers are being held in uintptr slots in args, so
// having our stack scanned now could lead to accidentally freeing
// memory.
//
//go:nosplit
internal static void moveMakeFuncArgPtrs(ж<makeFuncCtxt> Ꮡctxt, ж<abi.RegArgs> Ꮡargs) {
    ref var ctxt = ref Ꮡctxt.DerefOrNull();
    ref var args = ref Ꮡargs.DerefOrNull();

    foreach (var (i, arg) in args.Ints.ΔRangeSnapshot()) {
        // Avoid write barriers! Because our write barrier enqueues what
        // was there before, we might enqueue garbage.
        if (ctxt.regPtrs.Get(i)){
            (Ꮡargs.at(abi.RegArgs.ᏑPtrs, i).Reinterpret<@unsafe.Pointer, uintptr>()).Value = arg;
        } else {
            // We *must* zero this space ourselves because it's defined in
            // assembly code and the GC will scan these pointers. Otherwise,
            // there will be garbage here.
            (Ꮡargs.at(abi.RegArgs.ᏑPtrs, i).Reinterpret<@unsafe.Pointer, uintptr>()).Value = 0;
        }
    }
}

} // end reflect_package
