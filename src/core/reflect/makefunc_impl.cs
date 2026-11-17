// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// MakeFunc implementation.
using go;
using System;
using abi = go.@internal.abi_package;

// Hand-finished conversion (the reflection bridge — Phase 4, MakeFunc). The auto body is runtime
// machinery end to end: it reinterprets the descriptor into a funcType sub-record no synthesized
// abi.Type has behind it (the box comes back zero, Kind 0), asks funcLayout for a stack map over
// that nothing ("reflect: funcLayout of non-func type <nil>" — net/http/httptrace's compose was the
// first operational hit), and pairs an assembly stub (makeFuncStub) with a closure context the
// managed runtime cannot execute. The managed form is the exact INVERSE of the hand-owned
// Value.Call (value_impl.cs): where Call marshals a slice<Value> into a delegate's DynamicInvoke,
// MakeFunc builds a delegate of the descriptor's carried System.Type (golib
// GoReflect.MakeGoFuncDelegate — expression-compiled, one factory per delegate type) whose
// invocation marshals its CLR arguments INTO a slice<Value> (makeTypedValue, so an interface-typed
// parameter reports Kind Interface and a [N]byte parameter carries the descriptor's funcParamDims
// cargo), runs fn, and marshals the result Values back out under the same assignability rule Call's
// arguments use (marshalIntoSlot — one renderer for both directions). The returned Value carries
// typ's OWN descriptor box, so Type() interns to the very wrapper the caller passed in (canonType
// keys on the sysType plus the dims cargo, and both ride the box). makeMethodValue's identical
// funcLayout read stays AUTO deliberately: it is only reachable through flagMethod, which the
// bridge never sets — Value.Method binds the receiver into an ordinary delegate instead
// (GoReflect.GoMethodValue), so no Value ever takes that path. The converter skips the auto
// MakeFunc via the manualConversionFuncs registry (go2cs/manualTypeOperations.go); this module
// marker also makes go2cs skip re-converting this file.
// See docs/phase4/DESIGN-reflection-bridge.md.

[module: GoManualConversion]

namespace go;

partial class reflect_package {

// MakeFunc returns a new function of the given [Type]
// that wraps the function fn. When called, that new function
// does the following:
//
//   - converts its arguments to a slice of Values.
//   - runs results := fn(args).
//   - returns the results as a slice of Values, one per formal result.
//
// The implementation fn can assume that the argument [Value] slice
// has the number and type of arguments given by typ.
// If typ describes a variadic function, the final Value is itself
// a slice representing the variadic arguments, as in the
// body of a variadic function. The result Value slice returned by fn
// must have the number and type of results given by typ.
//
// The [Value.Call] method allows the caller to invoke a typed function
// in terms of Values; in contrast, MakeFunc allows the caller to implement
// a typed function in terms of Values.
//
// The Examples section of the documentation includes an illustration
// of how to use MakeFunc to build a swap function for different types.
public static Value MakeFunc(Type typ, Func<slice<Value>, slice<Value>> fn) {
    if (typ == default! || typ.Kind() != Func) {
        throw panic("reflect: call of MakeFunc with non-Func type");
    }
    var Ꮡt = typ.common();
    System.Type? st = Ꮡt == nil ? null : Ꮡt.Value.sysType;
    if (st is null || !GoReflect.TryFuncShape(st, out System.Type[]? ins, out System.Type[]? outs, out bool isVariadic)) {
        throw panic("reflect: call of MakeFunc with non-Func type");
    }
    // A VARIADIC func (isVariadic) is no longer refused here: its delegate's tail lowers to a
    // byref-like `params Span<T>` an expression tree cannot carry, and MakeGoFuncDelegate now carries
    // it through the typed makeVariadicFunc{N}/makeVariadicAction{N} family (the reverse of
    // InvokeVariadic's call trampolines, GoReflect.MakeVariadicDelegate.cs). The invoker below is
    // shape-agnostic: `ins` already presents the variadic parameter as its slice, so a `func(int,
    // ...int)` arrives as two args -- the int and the packed `[]int` -- exactly as the trampoline packs it.
    _ = isVariadic;
    // A Go multi-return arrives back as the delegate's declared ValueTuple — captured from the
    // Invoke signature rather than re-derived from outs, so the packed tuple is the exact type the
    // delegate returns.
    //
    // Beyond SEVEN results the tuple NESTS: `ValueTuple<T1..T7, TRest>` where TRest is itself a
    // ValueTuple carrying the remainder, recursively. A flat `CreateInstance(returnType, values)`
    // cannot express that — it hands eight arguments to a constructor whose eighth parameter is a
    // tuple, not a value — so this shape used to refuse outright, on the stated grounds that no Go
    // func in the corpus returned eight values.
    //
    // That condition is now FALSE, which is exactly the retirement the refusal recorded: reflect's
    // own `TestReflectMakeFuncCallABI` is the demonstrated consumer, and it is 27 verdicts — the
    // largest single mismatch family in reflect's suite (measured 2026-08-31). The subtests report
    // as EMPTY rather than failed, because the parent throws before any of them produces a verdict,
    // which is why the family reads as absent rather than broken.
    System.Type returnType = st.GetMethod("Invoke")!.ReturnType;
    nint[]?[]? paramDims = Ꮡt.Value.funcParamDims;

    object? invoke(object?[] rawArgs) {
        var args = new slice<Value>(ins.Length);
        for (int i = 0; i < ins.Length; i++) {
            // Each argument is typed by the func's STATIC parameter type (Go's contract: fn can
            // assume the types given by typ) — an interface-typed parameter is a Kind Interface
            // Value over the dynamic argument, a nil pointer a VALID typed-nil Value, and an array
            // parameter carries the descriptor's per-parameter dims cargo (the one route a
            // `[32]byte` parameter's length reaches reflect at all — see rtype.In).
            nint[]? dims = paramDims is not null && i < paramDims.Length ? paramDims[i] : null;
            args[i] = makeTypedValue(rawArgs[i], ins[i], dims, default);
        }
        slice<Value> results = fn(args);
        if (len(results) != outs.Length) {
            throw panic("reflect: wrong return count from function created by MakeFunc");
        }
        if (outs.Length == 0) {
            return null;
        }
        object?[] marshalled = new object?[outs.Length];
        for (int i = 0; i < outs.Length; i++) {
            marshalled[i] = marshalMakeFuncResult(results[i], outs[i]);
        }
        return outs.Length == 1 ? marshalled[0] : packResultTuple(returnType, marshalled, 0);
    }

    Delegate del = GoReflect.MakeGoFuncDelegate(st, invoke);
    // The returned Value rides typ's OWN descriptor box (not a fresh synthType), so the dims cargo
    // survives and Type() interns back to the caller's wrapper — `MakeFunc(t, fn).Type() == t`.
    var v = new Value(Ꮡt, default!, ((flag)(uintptr)(nuint)Func));
    v.boxed = del;
    return v;
}

// The delegate's declared result tuple, built from the marshalled results starting at offset.
//
// A C# ValueTuple carries at most SEVEN values inline; an eighth generic argument is TRest, itself a
// ValueTuple holding the remainder, and the nesting repeats. So the shape is decided by the TYPE,
// never by the count: read how many generic arguments returnType actually has, fill the inline ones
// from the flat results, and when there is a TRest slot, recurse into it with the offset advanced by
// the seven just consumed. A Go func returning nine values lands as
// `ValueTuple<T1..T7, ValueTuple<T8, T9>>` and this builds exactly that.
//
// Driving off the type rather than off `outs.Length` is what makes it correct for every arity
// without a table: the delegate factory already chose the tuple shape, and this only has to agree
// with it. `CreateInstance` on each level is the same call the flat path always used.
private static object? packResultTuple(System.Type tupleType, object?[] values, int offset) {
    System.Type[] elements = tupleType.GetGenericArguments();
    object?[] args = new object?[elements.Length];
    int inline = elements.Length == 8 ? 7 : elements.Length;

    for (int i = 0; i < inline; i++) {
        args[i] = values[offset + i];
    }

    if (elements.Length == 8) {
        args[7] = packResultTuple(elements[7], values, offset + 7);
    }

    return System.Activator.CreateInstance(tupleType, args);
}

// One result Value, marshalled into the delegate's CLR result slot under the SAME assignability
// rule Value.Call's arguments use (marshalIntoSlot — an interface-typed result packs the way
// Interface() packs, a concrete one hands over the box it holds). The guards are Go's own, in Go's
// order: the zero Value and a read-only Value (obtained from an unexported field) are refused
// before assignability is asked at all.
private static object? marshalMakeFuncResult(Value result, System.Type want) {
    if (result.flag == 0) {
        throw panic("reflect: function created by MakeFunc returned zero Value");
    }
    if ((flag)(result.flag & flagRO) != 0) {
        throw panic("reflect: function created by MakeFunc returned value obtained from unexported field");
    }
    if (!marshalIntoSlot(result, want, out object? marshalled)) {
        throw panic("reflect.MakeFunc: value of type " + GoReflect.GoTypeName(result.live?.GetType()) +
                    " is not assignable to type " + GoReflect.GoTypeName(want));
    }
    return marshalled;
}

} // end reflect_package
