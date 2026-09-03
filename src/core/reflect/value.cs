// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using itoa = @internal.itoa_package;
using unsafeheader = @internal.unsafeheader_package;
using Δmath = math_package;
using Δruntime = runtime_package;
using @unsafe = unsafe_package;
using @internal;
using Δsync = sync_package;

partial class reflect_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸerrors() {
    builtin.initPackage(typeof(errors_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸmath() {
    builtin.initPackage(typeof(math_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸruntime() {
    builtin.initPackage(typeof(runtime_package));
}

// Value is the reflection interface to a Go value.
//
// Not all methods apply to all kinds of values. Restrictions,
// if any, are noted in the documentation for each method.
// Use the Kind method to find out the kind of value before
// calling kind-specific methods. Calling a method
// inappropriate to the kind of type causes a run time panic.
//
// The zero Value represents no value.
// Its [Value.IsValid] method returns false, its Kind method returns [Invalid],
// its String method returns "<invalid Value>", and all other methods panic.
// Most functions and methods never return an invalid value.
// If one does, its documentation states the conditions explicitly.
//
// A Value can be used concurrently by multiple goroutines provided that
// the underlying Go value can be used concurrently for the equivalent
// direct operations.
//
// To compare two Values, compare the results of the Interface method.
// Using == on two Values does not compare the underlying values
// they represent.
[GoType] partial struct ΔValue {
    // typ_ holds the type of the value represented by a Value.
    // Access using the typ method to avoid escape of v.
    internal ж<abi.Type> typ_;
    // Pointer-valued data or, if flagIndir is set, pointer to data.
    // Valid when either flagIndir is set or typ.pointers() is true.
    internal @unsafe.Pointer ptr;
    // flag holds metadata about the value.
    //
    // The lowest five bits give the Kind of the value, mirroring typ.Kind().
    //
    // The next set of bits are flag bits:
    //	- flagStickyRO: obtained via unexported not embedded field, so read-only
    //	- flagEmbedRO: obtained via unexported embedded field, so read-only
    //	- flagIndir: val holds a pointer to the data
    //	- flagAddr: v.CanAddr is true (implies flagIndir and ptr is non-nil)
    //	- flagMethod: v is a method value.
    // If ifaceIndir(typ), code can assume that flagIndir is set.
    //
    // The remaining 22+ bits give a method number for method values.
    // If flag.kind() != Func, code can assume that flagMethod is unset.
    internal partial ref flag flag { get; }
}

[GoType("num:uintptr")] partial struct flag;

// A method value represents a curried method invocation
// like r.Read for some receiver r. The typ+val+flag bits describe
// the receiver r, but the flag's Kind bits say Func (methods are
// functions), and the top bits of the flag give the method number
// in r's type's method table.
internal static UntypedInt flagKindWidth => 5; // there are 27 kinds
internal static flag flagKindMask => /* 1<<flagKindWidth - 1 */ 31;
internal static flag flagStickyRO => /* 1 << 5 */ 32;
internal static flag flagEmbedRO => /* 1 << 6 */ 64;
internal static flag flagIndir => /* 1 << 7 */ 128;
internal static flag flagAddr => /* 1 << 8 */ 256;
internal static flag flagMethod => /* 1 << 9 */ 512;
internal static UntypedInt flagMethodShift => 10;
internal static flag flagRO => /* flagStickyRO | flagEmbedRO */ 96;

internal static ΔKind kind(this flag f) {
    return ((ΔKind)(nuint)((uintptr)((flag)(f & flagKindMask))));
}

internal static flag ro(this flag f) {
    if ((flag)(f & flagRO) != 0) {
        return flagStickyRO;
    }
    return 0;
}

// typ returns the *abi.Type stored in the Value. This method is fast,
// but it doesn't always return the correct type for the Value.
// See abiType and Type, which do return the correct type.
internal static ж<abi.Type> typ(this ΔValue v) {
    // Types are either static (for compiler-created types) or
    // heap-allocated but always reachable (for reflection-created
    // types, held in the central map). So there is no need to
    // escape types. noescape here help avoid unnecessary escape
    // of v.
    return v.typ_;
}

// pointer returns the underlying pointer represented by v.
// v.Kind() must be Pointer, Map, Chan, Func, or UnsafePointer
// if v.Kind() == Pointer, the base type must not be not-in-heap.
internal static @unsafe.Pointer pointer(this ΔValue v) {
    if (v.typ().Size() != goarch.PtrSize || !v.typ().Pointers()) {
        throw panic("can't call pointer on a non-pointer Value");
    }
    if ((flag)(v.flag & flagIndir) != 0) {
        return ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
    }
    return v.ptr;
}

// packEface converts v to the empty interface.
internal static any packEface(ΔValue v) {
    var t = v.typ();
    ref var i = ref heap<any>(out var Ꮡi);
    var e = Ꮡi.Reinterpret<any, abi.EmptyInterface>();
    // First, fill in the data portion of the interface.
    switch (ᐧ) {
    case {} when t.IfaceIndir(): {
        if ((flag)(v.flag & flagIndir) == 0) {
            throw panic("bad indir");
        }
        @unsafe.Pointer ptr = v.ptr;
        if ((flag)(v.flag & flagAddr) != 0) {
            // Value is indirect, and so is the interface we're making.
            @unsafe.Pointer c = (uintptr)unsafe_New(t);
            typedmemmove(t, c, ptr);
            ptr = c;
        }
        e.Value.Data = ptr;
        break;
    }
    case {} when (flag)(v.flag & flagIndir) != 0: {
        e.Value.Data = ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
        break;
    }
    default: {
        e.Value.Data = v.ptr;
        break;
    }}

    // Value is indirect, but interface is direct. We need
    // to load the data at v.ptr into the interface data word.
    // Value is direct, and so is the interface.
    // Now, fill in the type portion. We're very careful here not
    // to have any operation between the e.word and e.typ assignments
    // that would let the garbage collector observe the partially-built
    // interface value.
    e.Value.Type = t;
    return i;
}

// go2cs generated this placeholder — func unpackEface is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// A ValueError occurs when a Value method is invoked on
// a [Value] that does not support it. Such cases are documented
// in the description of each method.
[GoType] partial struct ValueError {
    public @string Method;
    public ΔKind Kind;
}

[GoRecv] public static @string Error(this ref ValueError e) {
    if (e.Kind == 0) {
        return "reflect: call of "u8 + e.Method + " on zero Value"u8;
    }
    return "reflect: call of "u8 + e.Method + " on "u8 + e.Kind.String() + " Value"u8;
}

// go2cs generated this placeholder — func valueMethodName is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// nonEmptyInterface is the header for an interface value with methods.
[GoType] partial struct nonEmptyInterface {
    internal ж<abi.ITab> itab;
    internal @unsafe.Pointer word;
}

// go2cs generated this placeholder — func mustBe is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func mustBeExported is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func mustBeExportedSlow is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func mustBeAssignable is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func mustBeAssignableSlow is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Addr is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Bool is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static void panicNotBool(this ΔValue v) {
    v.mustBe(ΔBool);
}

internal static ж<abi.Type> bytesType = rtypeOf((slice<byte>)(default!));

// go2cs generated this placeholder — func Bytes is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static slice<byte> bytesSlow(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == ΔSlice) {
        if (v.typ().Elem().Kind() != abi.Uint8) {
            throw panic("reflect.Value.Bytes of non-byte slice");
        }
        return ~(ж<slice<byte>>)(uintptr)(v.ptr);
    }
    if (exprᴛ1 == Array) {
        if (v.typ().Elem().Kind() != abi.Uint8) {
            // Slice is always bigger than a word; assume flagIndir.
            throw panic("reflect.Value.Bytes of non-byte array");
        }
        if (!v.CanAddr()) {
            throw panic("reflect.Value.Bytes of unaddressable byte array");
        }
        var p = (ж<byte>)(uintptr)(v.ptr);
        nint n = (nint)(v.typ().Reinterpret<abi.Type, arrayType>()).Value.Len;
        return @unsafe.Slice(p, n);
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Bytes"u8, v.kind())));
}

// go2cs generated this placeholder — func runes is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// CanAddr reports whether the value's address can be obtained with [Value.Addr].
// Such values are called addressable. A value is addressable if it is
// an element of a slice, an element of an addressable array,
// a field of an addressable struct, or the result of dereferencing a pointer.
// If CanAddr returns false, calling [Value.Addr] will panic.
public static bool CanAddr(this ΔValue v) {
    return (flag)(v.flag & flagAddr) != 0;
}

// CanSet reports whether the value of v can be changed.
// A [Value] can be changed only if it is addressable and was not
// obtained by the use of unexported struct fields.
// If CanSet returns false, calling [Value.Set] or any type-specific
// setter (e.g., [Value.SetBool], [Value.SetInt]) will panic.
public static bool CanSet(this ΔValue v) {
    return (flag)(v.flag & ((flag)(flagAddr | flagRO))) == flagAddr;
}

// go2cs generated this placeholder — func Call is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func CallSlice is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static ж<bool> ᏑcallGC = new StandardBox<bool>(default(bool));
internal static ref bool callGC => ref ᏑcallGC.Value; // for testing; see TestCallMethodJump and TestCallArgLive

internal const bool debugReflectCall = false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectValueCallˢ = "reflect.Value.Call"u8;
internal static readonly @string precomputedStackArgˢ = "precomputed stack arg offset"u8;
internal static readonly @string precomputedValueOffsetˢ = "precomputed value offset"u8;
internal static readonly @string tvSize0ˢ = "tv.Size() != 0"u8;

internal static slice<ΔValue> call(this ΔValue v, @string op, slice<ΔValue> @in) {
    // Get function pointer, type.
    var t = v.typ().FuncType();
    @unsafe.Pointer fn = default!;
    ΔValue rcvr = new(nil);
    ж<abi.Type> rcvrtype = default!;
    if ((flag)(v.flag & flagMethod) != 0){
        rcvr = v;
        (rcvrtype, t, fn) = methodReceiver(op, v, ((nint)(uintptr)v.flag >> (int)(flagMethodShift)));
    } else 
    if ((flag)(v.flag & flagIndir) != 0){
        fn = ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
    } else {
        fn = v.ptr;
    }
    if (fn == nil) {
        throw panic("reflect.Value.Call: call of nil function");
    }
    var isSlice = op == "CallSlice"u8;
    nint n = t.NumIn();
    var isVariadic = t.IsVariadic();
    if (isSlice){
        if (!isVariadic) {
            throw panic("reflect: CallSlice of non-variadic function");
        }
        if (len(@in) < n) {
            throw panic("reflect: CallSlice with too few input arguments");
        }
        if (len(@in) > n) {
            throw panic("reflect: CallSlice with too many input arguments");
        }
    } else {
        if (isVariadic) {
            n--;
        }
        if (len(@in) < n) {
            throw panic("reflect: Call with too few input arguments");
        }
        if (!isVariadic && len(@in) > n) {
            throw panic("reflect: Call with too many input arguments");
        }
    }
    foreach (var (_, x) in @in) {
        if (x.Kind() == Invalid) {
            throw panic("reflect: " + op + " using zero Value argument");
        }
    }
    for (nint i = 0; i < n; i++) {
        {
            var (xt, targ) = (@in[i].Type(), t.In(i)); if (!xt.AssignableTo(new rtypeжΔType(toRType(targ)))) {
                throw panic("reflect: " + op + " using " + xt.String() + " as type " + stringFor(targ));
            }
        }
    }
    if (!isSlice && isVariadic) {
        // prepare slice for remaining values
        nint m = len(@in) - n;
        var Δslice = MakeSlice(new rtypeжΔType(toRType(t.In(n))), m, m);
        var elem = toRType(t.In(n)).Elem(); // FIXME cast to slice type and Elem()
        for (nint i = 0; i < m; i++) {
            var x = @in[n + i];
            {
                var xt = x.Type(); if (!xt.AssignableTo(elem)) {
                    throw panic("reflect: cannot use " + xt.String() + " as type " + elem.String() + " in " + op);
                }
            }
            Δslice.Index(i).Set(x);
        }
        var origIn = @in;
        @in = new slice<ΔValue>(n + 1, () => new(nil));
        copy(@in[..(int)(n)], origIn);
        @in[n] = Δslice;
    }
    nint nin = len(@in);
    if (nin != t.NumIn()) {
        throw panic("reflect.Value.Call: wrong argument count");
    }
    nint nout = t.NumOut();
    // Register argument space.
    ref var regArgs = ref heap(new abi.RegArgs(), out var ᏑregArgs);
    // Compute frame type.
    var (frametype, framePool, abid) = funcLayout(t, rcvrtype);
    // Allocate a chunk of memory for frame if needed.
    @unsafe.Pointer stackArgs = default!;
    if (frametype.Size() != 0) {
        if (nout == 0){
            stackArgs = framePool.Get()._<@unsafe.Pointer>();
        } else {
            // Can't use pool if the function has return values.
            // We will leak pointer to args in ret, so its lifetime is not scoped.
            stackArgs = (uintptr)unsafe_New(frametype);
        }
    }
    var frameSize = frametype.Size();
    if (debugReflectCall) {
        println((@string)"reflect.call"u8, stringFor(t.of(funcType.ᏑType)));
        abid.dump();
    }
    // Copy inputs into args.
    // Handle receiver.
    nint inStart = 0;
    if (rcvrtype != nil) {
        // Guaranteed to only be one word in size,
        // so it will only take up exactly 1 abiStep (either
        // in a register or on the stack).
        {
            var st = abid.call.steps[0];
            var exprᴛ1 = st.kind;
            var matchᴛ1 = false;
            if (exprᴛ1 == abiStepStack) { matchᴛ1 = true;
                storeRcvr(rcvr, stackArgs);
            }
            else if (exprᴛ1 == abiStepPointer) { matchᴛ1 = true;
                storeRcvr(rcvr, @unsafe.Pointer.FromBox(ᏑregArgs.at(abi.RegArgs.ᏑPtrs, st.ireg)));
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == abiStepIntReg) { matchᴛ1 = true;
                storeRcvr(rcvr, @unsafe.Pointer.FromBox(ᏑregArgs.at(abi.RegArgs.ᏑInts, st.ireg)));
            }
            else if (exprᴛ1 == abiStepFloatReg) {
                storeRcvr(rcvr, new @unsafe.Pointer(ᏑregArgs.at(abi.RegArgs.ᏑFloats, st.freg)));
            }
            else if (!matchᴛ1) { /* default: */
                throw panic("unknown ABI parameter kind");
            }
        }

        inStart = 1;
    }
    // Handle arguments.
    foreach (var (i, vᴛ1) in @in) {
        var vΔ1 = vᴛ1;

        vΔ1.mustBeExported();
        var targ = toRType(t.In(i));
        // TODO(mknyszek): Figure out if it's possible to get some
        // scratch space for this assignment check. Previously, it
        // was possible to use space in the argument frame.
        vΔ1 = vΔ1.assignTo(reflectValueCallˢ, targ.of(rtype.Ꮡt), nil);
stepsLoop:
        foreach (var (_, st) in abid.call.stepsForValue(i + inStart)) {
            var exprᴛ2 = st.kind;
            if (exprᴛ2 == abiStepStack) {
                @unsafe.Pointer addr = (uintptr)add(stackArgs, // Copy values to the "stack."
 st.stkOff, precomputedStackArgˢ);
                if ((flag)(vΔ1.flag & flagIndir) != 0){
                    typedmemmove(targ.of(rtype.Ꮡt), addr, vΔ1.ptr);
                } else {
                    ((ж<@unsafe.Pointer>)(uintptr)(addr)).Value = vΔ1.ptr;
                }
                goto break_stepsLoop;
            }
            else if (exprᴛ2 == abiStepIntReg || exprᴛ2 == abiStepPointer) {
                if ((flag)(vΔ1.flag & flagIndir) != 0){
                    // There's only one step for a stack-allocated value.
                    // Copy values to "integer registers."
                    @unsafe.Pointer offset = (uintptr)add(vΔ1.ptr, st.offset, precomputedValueOffsetˢ);
                    if (st.kind == abiStepPointer) {
                        // Duplicate this pointer in the pointer area of the
                        // register space. Otherwise, there's the potential for
                        // this to be the last reference to v.ptr.
                        regArgs.Ptrs[st.ireg] = ~(ж<@unsafe.Pointer>)(uintptr)(offset);
                    }
                    intToReg(ᏑregArgs, st.ireg, st.size, offset);
                } else {
                    if (st.kind == abiStepPointer) {
                        // See the comment in abiStepPointer case above.
                        regArgs.Ptrs[st.ireg] = vΔ1.ptr;
                    }
                    regArgs.Ints[st.ireg] = (uintptr)vΔ1.ptr;
                }
            }
            else if (exprᴛ2 == abiStepFloatReg) {
                if ((flag)(vΔ1.flag & flagIndir) == 0) {
                    // Copy values to "float registers."
                    throw panic("attempted to copy pointer to FP register");
                }
                @unsafe.Pointer offset = (uintptr)add(vΔ1.ptr, st.offset, precomputedValueOffsetˢ);
                floatToReg(ref regArgs, st.freg, st.size, offset);
            }
            else { /* default: */
                throw panic("unknown ABI part kind");
            }

continue_stepsLoop:;
        }
break_stepsLoop:;
    }
    // TODO(mknyszek): Remove this when we no longer have
    // caller reserved spill space.
    frameSize = align(frameSize, goarch.PtrSize);
    frameSize += abid.spill;
    // Mark pointers in registers for the return path.
    regArgs.ReturnIsPtr = abid.outRegPtrs.Clone();
    if (debugReflectCall) {
        regArgs.Dump();
    }
    // For testing; see TestCallArgLive.
    if (callGC) {
        Δruntime.GC();
    }
    // Call.
    call(frametype, fn, stackArgs, (uint32)frametype.Size(), (uint32)abid.retOffset, (uint32)frameSize, ᏑregArgs);
    // For testing; see TestCallMethodJump.
    if (callGC) {
        Δruntime.GC();
    }
    slice<ΔValue> ret = default!;
    if (nout == 0){
        if (stackArgs != nil) {
            typedmemclr(frametype, stackArgs);
            framePool.Put(stackArgs);
        }
    } else {
        if (stackArgs != nil) {
            // Zero the now unused input area of args,
            // because the Values returned by this function contain pointers to the args object,
            // and will thus keep the args object alive indefinitely.
            typedmemclrpartial(frametype, stackArgs, 0, abid.retOffset);
        }
        // Wrap Values around return values in args.
        ret = new slice<ΔValue>(nout, () => new(nil));
        for (nint i = 0; i < nout; i++) {
            var tv = t.Out(i);
            if (tv.Size() == 0) {
                // For zero-sized return value, args+off may point to the next object.
                // In this case, return the zero value instead.
                ret[i] = Zero(new rtypeжΔType(toRType(tv)));
                continue;
            }
            var steps = abid.ret.stepsForValue(i);
            {
                var st = steps[0]; if (st.kind == abiStepStack) {
                    // This value is on the stack. If part of a value is stack
                    // allocated, the entire value is according to the ABI. So
                    // just make an indirection into the allocated frame.
                    var fl = (flag)(flagIndir | ((flag)(uintptr)(uint8)tv.Kind()));
                    ret[i] = new ΔValue(tv, (uintptr)add(stackArgs, st.stkOff, tvSize0ˢ), fl);
                    // Note: this does introduce false sharing between results -
                    // if any result is live, they are all live.
                    // (And the space for the args is live as well, but as we've
                    // cleared that space it isn't as big a deal.)
                    continue;
                }
            }
            // Handle pointers passed in registers.
            if (!tv.IfaceIndir()) {
                // Pointer-valued data gets put directly
                // into v.ptr.
                if (steps[0].kind != abiStepPointer) {
                    print((@string)"kind="u8, steps[0].kind, (@string)", type="u8, stringFor(tv), (@string)"\n"u8);
                    throw panic("mismatch between ABI description and types");
                }
                ret[i] = new ΔValue(tv, regArgs.Ptrs[steps[0].ireg], ((flag)(uintptr)(uint8)tv.Kind()));
                continue;
            }
            // All that's left is values passed in registers that we need to
            // create space for and copy values back into.
            //
            // TODO(mknyszek): We make a new allocation for each register-allocated
            // value, but previously we could always point into the heap-allocated
            // stack frame. This is a regression that could be fixed by adding
            // additional space to the allocated stack frame and storing the
            // register-allocated return values into the allocated stack frame and
            // referring there in the resulting Value.
            @unsafe.Pointer s = (uintptr)unsafe_New(tv);
            foreach (var (_, st) in steps) {
                var exprᴛ3 = st.kind;
                if (exprᴛ3 == abiStepIntReg) {
                    @unsafe.Pointer offset = (uintptr)add(s, st.offset, precomputedValueOffsetˢ);
                    intFromReg(ᏑregArgs, st.ireg, st.size, offset);
                }
                else if (exprᴛ3 == abiStepPointer) {
                    @unsafe.Pointer sΔ2 = (uintptr)add(s, st.offset, precomputedValueOffsetˢ);
                    ((ж<@unsafe.Pointer>)(uintptr)(sΔ2)).Value = regArgs.Ptrs[st.ireg];
                }
                else if (exprᴛ3 == abiStepFloatReg) {
                    @unsafe.Pointer offset = (uintptr)add(s, st.offset, precomputedValueOffsetˢ);
                    floatFromReg(ᏑregArgs, st.freg, st.size, offset);
                }
                else if (exprᴛ3 == abiStepStack) {
                    throw panic("register-based return value has stack component");
                }
                else { /* default: */
                    throw panic("unknown ABI part kind");
                }

            }
            ret[i] = new ΔValue(tv, s.Value, (flag)(flagIndir | ((flag)(uintptr)(uint8)tv.Kind())));
        }
    }
    return ret;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typSize0ˢ = "typ.size > 0"u8;
internal static readonly @string ptrˢ = "1-ptr"u8;
internal static readonly @string reflectMakeFuncˢ = "reflect.MakeFunc"u8;

// callReflect is the call implementation used by a function
// returned by MakeFunc. In many ways it is the opposite of the
// method Value.call above. The method above converts a call using Values
// into a call of a function with a concrete argument frame, while
// callReflect converts a call of a function with a concrete argument
// frame into a call using Values.
// It is in this file so that it can be next to the call method above.
// The remainder of the MakeFunc implementation is in makefunc.go.
//
// NOTE: This function must be marked as a "wrapper" in the generated code,
// so that the linker can make it work correctly for panic and recover.
// The gc compilers know to do that for the name "reflect.callReflect".
//
// ctxt is the "closure" generated by MakeFunc.
// frame is a pointer to the arguments to that closure on the stack.
// retValid points to a boolean which should be set when the results
// section of frame is set.
//
// regs contains the argument values passed in registers and will contain
// the values returned from ctxt.fn in registers.
internal static void callReflect(ж<makeFuncImpl> Ꮡctxt, @unsafe.Pointer frame, ref bool retValid, ж<abi.RegArgs> Ꮡregs) {
    ref var ctxt = ref Ꮡctxt.DerefOrNull();
    ref var regs = ref Ꮡregs.DerefOrNull();

    if (callGC) {
        // Call GC upon entry during testing.
        // Getting our stack scanned here is the biggest hazard, because
        // our caller (makeFuncStub) could have failed to place the last
        // pointer to a value in regs' pointer space, in which case it
        // won't be visible to the GC.
        Δruntime.GC();
    }
    var ftyp = ctxt.ftyp;
    var f = ctxt.fn;
    var (_, _, abid) = funcLayout(ftyp, nil);
    // Copy arguments into Values.
    @unsafe.Pointer ptr = frame;
    var @in = new slice<ΔValue>(0, () => new(nil), (nint)(~ftyp).InCount);
    foreach (var (i, typ) in ftyp.InSlice()) {
        if (typ.Size() == 0) {
            @in = builtin.append(@in, Zero(new rtypeжΔType(toRType(typ))));
            continue;
        }
        var v = new ΔValue(typ, nil, ((flag)(uintptr)(uint8)typ.Kind()));
        var steps = abid.call.stepsForValue(i);
        {
            var st = steps[0]; if (st.kind == abiStepStack){
                if (typ.IfaceIndir()){
                    // value cannot be inlined in interface data.
                    // Must make a copy, because f might keep a reference to it,
                    // and we cannot let f keep a reference to the stack frame
                    // after this function returns, not even a read-only reference.
                    v.ptr = (uintptr)unsafe_New(typ);
                    if (typ.Size() > 0) {
                        typedmemmove(typ, v.ptr, (uintptr)add(ptr, st.stkOff, typSize0ˢ));
                    }
                    v.flag |= (flag)(flagIndir);
                } else {
                    v.ptr = ~(ж<@unsafe.Pointer>)(uintptr)((uintptr)add(ptr, st.stkOff, ptrˢ));
                }
            } else {
                if (typ.IfaceIndir()){
                    // All that's left is values passed in registers that we need to
                    // create space for the values.
                    v.flag |= (flag)(flagIndir);
                    v.ptr = (uintptr)unsafe_New(typ);
                    foreach (var (_, stΔ1) in steps) {
                        var exprᴛ1 = stΔ1.kind;
                        if (exprᴛ1 == abiStepIntReg) {
                            @unsafe.Pointer offset = (uintptr)add(v.ptr, stΔ1.offset, precomputedValueOffsetˢ);
                            intFromReg(Ꮡregs, stΔ1.ireg, stΔ1.size, offset);
                        }
                        else if (exprᴛ1 == abiStepPointer) {
                            @unsafe.Pointer s = (uintptr)add(v.ptr, stΔ1.offset, precomputedValueOffsetˢ);
                            ((ж<@unsafe.Pointer>)(uintptr)(s)).Value = regs.Ptrs[stΔ1.ireg];
                        }
                        else if (exprᴛ1 == abiStepFloatReg) {
                            @unsafe.Pointer offset = (uintptr)add(v.ptr, stΔ1.offset, precomputedValueOffsetˢ);
                            floatFromReg(Ꮡregs, stΔ1.freg, stΔ1.size, offset);
                        }
                        else if (exprᴛ1 == abiStepStack) {
                            throw panic("register-based return value has stack component");
                        }
                        else { /* default: */
                            throw panic("unknown ABI part kind");
                        }

                    }
                } else {
                    // Pointer-valued data gets put directly
                    // into v.ptr.
                    if (steps[0].kind != abiStepPointer) {
                        print((@string)"kind="u8, steps[0].kind, (@string)", type="u8, stringFor(typ), (@string)"\n"u8);
                        throw panic("mismatch between ABI description and types");
                    }
                    v.ptr = regs.Ptrs[steps[0].ireg];
                }
            }
        }
        @in = builtin.append(@in, v);
    }
    // Call underlying function.
    var @out = f(@in);
    nint numOut = ftyp.NumOut();
    if (len(@out) != numOut) {
        throw panic("reflect: wrong return count from function created by MakeFunc");
    }
    // Copy results back into argument frame and register space.
    if (numOut > 0) {
        foreach (var (i, typ) in ftyp.OutSlice()) {
            var v = @out[i];
            if (v.typ() == nil) {
                throw panic("reflect: function created by MakeFunc using " + funcName(f) + " returned zero Value");
            }
            if ((flag)(v.flag & flagRO) != 0) {
                throw panic("reflect: function created by MakeFunc using " + funcName(f) + " returned value obtained from unexported field");
            }
            if (typ.Size() == 0) {
                continue;
            }
            // Convert v to type typ if v is assignable to a variable
            // of type t in the language spec.
            // See issue 28761.
            //
            //
            // TODO(mknyszek): In the switch to the register ABI we lost
            // the scratch space here for the register cases (and
            // temporarily for all the cases).
            //
            // If/when this happens, take note of the following:
            //
            // We must clear the destination before calling assignTo,
            // in case assignTo writes (with memory barriers) to the
            // target location used as scratch space. See issue 39541.
            v = v.assignTo(reflectMakeFuncˢ, typ, nil);
stepsLoop:
            foreach (var (_, st) in abid.ret.stepsForValue(i)) {
                var exprᴛ2 = st.kind;
                if (exprᴛ2 == abiStepStack) {
                    @unsafe.Pointer addr = (uintptr)add(ptr, // Copy values to the "stack."
 st.stkOff, precomputedStackArgˢ);
                    if ((flag)(v.flag & flagIndir) != 0){
                        // Do not use write barriers. The stack space used
                        // for this call is not adequately zeroed, and we
                        // are careful to keep the arguments alive until we
                        // return to makeFuncStub's caller.
                        memmove(addr, v.ptr, st.size);
                    } else {
                        // This case must be a pointer type.
                        ((ж<uintptr>)(uintptr)(addr)).Value = (uintptr)v.ptr;
                    }
                    goto break_stepsLoop;
                }
                else if (exprᴛ2 == abiStepIntReg || exprᴛ2 == abiStepPointer) {
                    if ((flag)(v.flag & flagIndir) != 0){
                        // There's only one step for a stack-allocated value.
                        // Copy values to "integer registers."
                        @unsafe.Pointer offset = (uintptr)add(v.ptr, st.offset, precomputedValueOffsetˢ);
                        intToReg(Ꮡregs, st.ireg, st.size, offset);
                    } else {
                        // Only populate the Ints space on the return path.
                        // This is safe because out is kept alive until the
                        // end of this function, and the return path through
                        // makeFuncStub has no preemption, so these pointers
                        // are always visible to the GC.
                        regs.Ints[st.ireg] = (uintptr)v.ptr;
                    }
                }
                else if (exprᴛ2 == abiStepFloatReg) {
                    if ((flag)(v.flag & flagIndir) == 0) {
                        // Copy values to "float registers."
                        throw panic("attempted to copy pointer to FP register");
                    }
                    @unsafe.Pointer offset = (uintptr)add(v.ptr, st.offset, precomputedValueOffsetˢ);
                    floatToReg(ref (Ꮡregs).DerefOrNull(), st.freg, st.size, offset);
                }
                else { /* default: */
                    throw panic("unknown ABI part kind");
                }

continue_stepsLoop:;
            }
break_stepsLoop:;
        }
    }
    // Announce that the return values are valid.
    // After this point the runtime can depend on the return values being valid.
    retValid = true;
    // We have to make sure that the out slice lives at least until
    // the runtime knows the return values are valid. Otherwise, the
    // return values might not be scanned by anyone during a GC.
    // (out would be dead, and the return slots not yet alive.)
    Δruntime.KeepAlive(@out);
    // runtime.getArgInfo expects to be able to find ctxt on the
    // stack when it finds our caller, makeFuncStub. Make sure it
    // doesn't get garbage collected.
    Δruntime.KeepAlive(Ꮡctxt.OrTypedNil());
}

// methodReceiver returns information about the receiver
// described by v. The Value v may or may not have the
// flagMethod bit set, so the kind cached in v.flag should
// not be used.
// The return value rcvrtype gives the method's actual receiver type.
// The return value t gives the method type signature (without the receiver).
// The return value fn is a pointer to the method code.
internal static (ж<abi.Type> rcvrtype, ж<funcType> t, @unsafe.Pointer fn) methodReceiver(@string op, ΔValue v, nint methodIndex) {
    ж<abi.Type> rcvrtype = default!;
    ж<funcType> t = default!;
    @unsafe.Pointer fn = default!;

    nint i = methodIndex;
    if (v.typ().Kind() == abi.Interface){
        var tt = v.typ().Reinterpret<abi.Type, interfaceType>();
        if ((nuint)i >= (nuint)len((~tt).Methods)) {
            throw panic("reflect: internal error: invalid method index");
        }
        var m = Ꮡ((~tt).Methods, i);
        if (!tt.nameOff((~m).Name).IsExported()) {
            throw panic("reflect: " + op + " of unexported method");
        }
        var iface = (ж<nonEmptyInterface>)(uintptr)(v.ptr);
        if ((~iface).itab == nil) {
            throw panic("reflect: " + op + " of method on nil interface value");
        }
        rcvrtype = iface.Value.itab.Value.Type;
        fn = @unsafe.Pointer.FromBox(Ꮡ(@unsafe.Slice((~iface).itab.at(abi.ITab.ᏑFun, 0), i + 1), i));
        t = tt.typeOff((~m).Typ).FuncType();
    } else {
        rcvrtype = v.typ();
        var ms = v.typ().ExportedMethods();
        if ((nuint)i >= (nuint)len(ms)) {
            throw panic("reflect: internal error: invalid method index");
        }
        var m = ms[i];
        if (!nameOffFor(v.typ(), m.Name).IsExported()) {
            throw panic("reflect: " + op + " of unexported method");
        }
        ref var ifn = ref heap<@unsafe.Pointer>(out var Ꮡifn);
        ifn = (uintptr)textOffFor(v.typ(), m.Ifn);
        fn = @unsafe.Pointer.FromBox(Ꮡifn);
        t = typeOffFor(v.typ(), m.Mtyp).FuncType();
    }
    return (rcvrtype, t, fn);
}

// v is a method receiver. Store at p the word which is used to
// encode that receiver at the start of the argument list.
// Reflect uses the "interface" calling convention for
// methods, which always uses one word to record the receiver.
internal static void storeRcvr(ΔValue v, @unsafe.Pointer p) {
    var t = v.typ();
    if (t.Kind() == abi.Interface){
        // the interface data word becomes the receiver word
        var iface = (ж<nonEmptyInterface>)(uintptr)(v.ptr);
        ((ж<@unsafe.Pointer>)(uintptr)(p)).Value = iface.Value.word;
    } else 
    if ((flag)(v.flag & flagIndir) != 0 && !t.IfaceIndir()){
        ((ж<@unsafe.Pointer>)(uintptr)(p)).Value = ((ж<@unsafe.Pointer>)(uintptr)(v.ptr)).Value;
    } else {
        ((ж<@unsafe.Pointer>)(uintptr)(p)).Value = v.ptr;
    }
}

// align returns the result of rounding x up to a multiple of n.
// n must be a power of two.
internal static uintptr align(uintptr x, uintptr n) {
    return (uintptr)((x + n - 1) & ~(n - 1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string callˢ = "call"u8;
internal static readonly @string precomputedStackOffsetˢ = "precomputed stack offset"u8;
internal static readonly @string valueFrameSSizeRetOffsetˢ = "valueFrame's size > retOffset"u8;
internal static readonly @string methodFrameSSizeˢ = "methodFrame's size > retOffset"u8;

// callMethod is the call implementation used by a function returned
// by makeMethodValue (used by v.Method(i).Interface()).
// It is a streamlined version of the usual reflect call: the caller has
// already laid out the argument frame for us, so we don't have
// to deal with individual Values for each argument.
// It is in this file so that it can be next to the two similar functions above.
// The remainder of the makeMethodValue implementation is in makefunc.go.
//
// NOTE: This function must be marked as a "wrapper" in the generated code,
// so that the linker can make it work correctly for panic and recover.
// The gc compilers know to do that for the name "reflect.callMethod".
//
// ctxt is the "closure" generated by makeMethodValue.
// frame is a pointer to the arguments to that closure on the stack.
// retValid points to a boolean which should be set when the results
// section of frame is set.
//
// regs contains the argument values passed in registers and will contain
// the values returned from ctxt.fn in registers.
internal static void callMethod(ж<methodValue> Ꮡctxt, @unsafe.Pointer frame, ref bool retValid, ж<abi.RegArgs> Ꮡregs) {
    ref var ctxt = ref Ꮡctxt.DerefOrNull();
    ref var regs = ref Ꮡregs.DerefOrNull();

    var rcvr = ctxt.rcvr;
    var (rcvrType, valueFuncType, methodFn) = methodReceiver(callˢ, rcvr, ctxt.method);
    // There are two ABIs at play here.
    //
    // methodValueCall was invoked with the ABI assuming there was no
    // receiver ("value ABI") and that's what frame and regs are holding.
    //
    // Meanwhile, we need to actually call the method with a receiver, which
    // has its own ABI ("method ABI"). Everything that follows is a translation
    // between the two.
    var (_, _, valueABI) = funcLayout(valueFuncType, nil);
    @unsafe.Pointer valueFrame = frame;
    var valueRegs = Ꮡregs;
    var (methodFrameType, methodFramePool, methodABI) = funcLayout(valueFuncType, rcvrType);
    // Make a new frame that is one word bigger so we can store the receiver.
    // This space is used for both arguments and return values.
    @unsafe.Pointer methodFrame = methodFramePool.Get()._<@unsafe.Pointer>();
    ref var methodRegs = ref heap(new abi.RegArgs(), out var ᏑmethodRegs);
    // Deal with the receiver. It's guaranteed to only be one word in size.
    {
        var st = methodABI.call.steps[0];
        var exprᴛ1 = st.kind;
        var matchᴛ1 = false;
        if (exprᴛ1 == abiStepStack) { matchᴛ1 = true;
            storeRcvr(rcvr, // Only copy the receiver to the stack if the ABI says so.
 // Otherwise, it'll be in a register already.
 methodFrame);
        }
        else if (exprᴛ1 == abiStepPointer) { matchᴛ1 = true;
            storeRcvr(rcvr, // Put the receiver in a register.
 @unsafe.Pointer.FromBox(ᏑmethodRegs.at(abi.RegArgs.ᏑPtrs, st.ireg)));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == abiStepIntReg) { matchᴛ1 = true;
            storeRcvr(rcvr, @unsafe.Pointer.FromBox(ᏑmethodRegs.at(abi.RegArgs.ᏑInts, st.ireg)));
        }
        else if (exprᴛ1 == abiStepFloatReg) {
            storeRcvr(rcvr, new @unsafe.Pointer(ᏑmethodRegs.at(abi.RegArgs.ᏑFloats, st.freg)));
        }
        else if (!matchᴛ1) { /* default: */
            throw panic("unknown ABI parameter kind");
        }
    }

    // Translate the rest of the arguments.
    foreach (var (i, t) in valueFuncType.InSlice()) {
        var valueSteps = valueABI.call.stepsForValue(i);
        var methodSteps = methodABI.call.stepsForValue(i + 1);
        // Zero-sized types are trivial: nothing to do.
        if (len(valueSteps) == 0) {
            if (len(methodSteps) != 0) {
                throw panic("method ABI and value ABI do not align");
            }
            continue;
        }
        // There are four cases to handle in translating each
        // argument:
        // 1. Stack -> stack translation.
        // 2. Stack -> registers translation.
        // 3. Registers -> stack translation.
        // 4. Registers -> registers translation.
        // If the value ABI passes the value on the stack,
        // then the method ABI does too, because it has strictly
        // fewer arguments. Simply copy between the two.
        {
            var vStep = valueSteps[0]; if (vStep.kind == abiStepStack) {
                var mStep = methodSteps[0];
                // Handle stack -> stack translation.
                if (mStep.kind == abiStepStack) {
                    if (vStep.size != mStep.size) {
                        throw panic("method ABI and value ABI do not align");
                    }
                    typedmemmove(t,
                        (uintptr)add(methodFrame, mStep.stkOff, precomputedStackOffsetˢ),
                        (uintptr)add(valueFrame, vStep.stkOff, precomputedStackOffsetˢ));
                    continue;
                }
                // Handle stack -> register translation.
                foreach (var (_, mStepΔ1) in methodSteps) {
                    @unsafe.Pointer from = (uintptr)add(valueFrame, vStep.stkOff + mStepΔ1.offset, precomputedStackOffsetˢ);
                    var exprᴛ2 = mStepΔ1.kind;
                    var matchᴛ2 = false;
                    if (exprᴛ2 == abiStepPointer) { matchᴛ2 = true;
                        methodRegs.Ptrs[mStepΔ1.ireg] = ~(ж<@unsafe.Pointer>)(uintptr)(from); // We need to make sure this ends up in Ints, too.
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ2 && exprᴛ2 == abiStepIntReg) { matchᴛ2 = true;
                        intToReg(ᏑmethodRegs, // Do the pointer copy directly so we get a write barrier.
 mStepΔ1.ireg, mStepΔ1.size, from);
                    }
                    else if (exprᴛ2 == abiStepFloatReg) {
                        floatToReg(ref methodRegs, mStepΔ1.freg, mStepΔ1.size, from);
                    }
                    else if (!matchᴛ2) { /* default: */
                        throw panic("unexpected method step");
                    }

                }
                continue;
            }
        }
        // Handle register -> stack translation.
        {
            var mStep = methodSteps[0]; if (mStep.kind == abiStepStack) {
                foreach (var (_, vStep) in valueSteps) {
                    @unsafe.Pointer to = (uintptr)add(methodFrame, mStep.stkOff + vStep.offset, precomputedStackOffsetˢ);
                    var exprᴛ3 = vStep.kind;
                    if (exprᴛ3 == abiStepPointer) {
                        ((ж<@unsafe.Pointer>)(uintptr)(to)).Value = (~valueRegs).Ptrs[vStep.ireg];
                    }
                    else if (exprᴛ3 == abiStepIntReg) {
                        intFromReg(valueRegs, // Do the pointer copy directly so we get a write barrier.
 vStep.ireg, vStep.size, to);
                    }
                    else if (exprᴛ3 == abiStepFloatReg) {
                        floatFromReg(valueRegs, vStep.freg, vStep.size, to);
                    }
                    else { /* default: */
                        throw panic("unexpected value step");
                    }

                }
                continue;
            }
        }
        // Handle register -> register translation.
        if (len(valueSteps) != len(methodSteps)) {
            // Because it's the same type for the value, and it's assigned
            // to registers both times, it should always take up the same
            // number of registers for each ABI.
            throw panic("method ABI and value ABI don't align");
        }
        foreach (var (iΔ1, vStep) in valueSteps) {
            var mStep = methodSteps[iΔ1];
            if (mStep.kind != vStep.kind) {
                throw panic("method ABI and value ABI don't align");
            }
            var exprᴛ4 = vStep.kind;
            var matchᴛ3 = false;
            if (exprᴛ4 == abiStepPointer) { matchᴛ3 = true;
                methodRegs.Ptrs[mStep.ireg] = (~valueRegs).Ptrs[vStep.ireg];
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ3 && exprᴛ4 == abiStepIntReg) { matchᴛ3 = true;
                methodRegs.Ints[mStep.ireg] = (~valueRegs).Ints[vStep.ireg];
            }
            else if (exprᴛ4 == abiStepFloatReg) {
                methodRegs.Floats[mStep.freg] = (~valueRegs).Floats[vStep.freg];
            }
            else if (!matchᴛ3) { /* default: */
                throw panic("unexpected value step");
            }

        }
    }
    // Copy this too, so we get a write barrier.
    var methodFrameSize = methodFrameType.Size();
    // TODO(mknyszek): Remove this when we no longer have
    // caller reserved spill space.
    methodFrameSize = align(methodFrameSize, goarch.PtrSize);
    methodFrameSize += methodABI.spill;
    // Mark pointers in registers for the return path.
    methodRegs.ReturnIsPtr = methodABI.outRegPtrs.Clone();
    // Call.
    // Call copies the arguments from scratch to the stack, calls fn,
    // and then copies the results back into scratch.
    call(methodFrameType, methodFn, methodFrame, (uint32)methodFrameType.Size(), (uint32)methodABI.retOffset, (uint32)methodFrameSize, ᏑmethodRegs);
    // Copy return values.
    //
    // This is somewhat simpler because both ABIs have an identical
    // return value ABI (the types are identical). As a result, register
    // results can simply be copied over. Stack-allocated values are laid
    // out the same, but are at different offsets from the start of the frame
    // Ignore any changes to args.
    // Avoid constructing out-of-bounds pointers if there are no return values.
    // because the arguments may be laid out differently.
    if (valueRegs != nil) {
        valueRegs.Value = methodRegs.ΔClone();
    }
    {
        var retSize = methodFrameType.Size() - methodABI.retOffset; if (retSize > 0) {
            @unsafe.Pointer valueRet = (uintptr)add(valueFrame, valueABI.retOffset, valueFrameSSizeRetOffsetˢ);
            @unsafe.Pointer methodRet = (uintptr)add(methodFrame, methodABI.retOffset, methodFrameSSizeˢ);
            // This copies to the stack. Write barriers are not needed.
            memmove(valueRet, methodRet, retSize);
        }
    }
    // Tell the runtime it can now depend on the return values
    // being properly initialized.
    retValid = true;
    // Clear the scratch space and put it back in the pool.
    // This must happen after the statement above, so that the return
    // values will always be scanned by someone.
    typedmemclr(methodFrameType, methodFrame);
    methodFramePool.Put(methodFrame);
    // See the comment in callReflect.
    Δruntime.KeepAlive(Ꮡctxt.OrTypedNil());
    // Keep valueRegs alive because it may hold live pointer results.
    // The caller (methodValueCall) has it as a stack object, which is only
    // scanned when there is a reference to it.
    Δruntime.KeepAlive(valueRegs.OrTypedNil());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string closureˢ = "closure"u8;

// funcName returns the name of f, for use in error messages.
internal static @string funcName(Func<slice<ΔValue>, slice<ΔValue>> fʗp) {
    ref var f = ref heap(fʗp, out var Ꮡf);

    var pc = ~Ꮡf.Reinterpret<Func<slice<ΔValue>, slice<ΔValue>>, uintptr>();
    var rf = Δruntime.FuncForPC(pc);
    if (rf != nil) {
        return rf.Name();
    }
    return closureˢ;
}

// go2cs generated this placeholder — func Cap is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static nint capNonSlice(this ΔValue v) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Array) {
        return v.typ().Len();
    }
    if (exprᴛ1 == Chan) {
        return chancap((uintptr)v.pointer());
    }
    if (exprᴛ1 == Ptr) {
        if (v.typ().Elem().Kind() == abi.Array) {
            return v.typ().Elem().Len();
        }
        throw panic("reflect: call of reflect.Value.Cap on ptr to non-array Value");
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Cap"u8, v.kind())));
}

// go2cs generated this placeholder — func Close is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// CanComplex reports whether [Value.Complex] can be used without panicking.
public static bool CanComplex(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == Complex64 || exprᴛ1 == Complex128) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// go2cs generated this placeholder — func Complex is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Elem is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Field is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// FieldByIndex returns the nested field corresponding to index.
// It panics if evaluation requires stepping through a nil
// pointer or a field that is not a struct.
public static ΔValue FieldByIndex(this ΔValue v, slice<nint> index) {
    if (len(index) == 1) {
        return v.Field(index[0]);
    }
    v.mustBe(Struct);
    foreach (var (i, x) in index) {
        if (i > 0) {
            if (v.Kind() == ΔPointer && v.typ().Elem().Kind() == abi.Struct) {
                if (v.IsNil()) {
                    throw panic("reflect: indirection through nil pointer to embedded struct");
                }
                v = v.Elem();
            }
        }
        v = v.Field(x);
    }
    return v;
}

// FieldByIndexErr returns the nested field corresponding to index.
// It returns an error if evaluation requires stepping through a nil
// pointer, but panics if it must step through a field that
// is not a struct.
public static (ΔValue, error) FieldByIndexErr(this ΔValue v, slice<nint> index) {
    if (len(index) == 1) {
        return (v.Field(index[0]), default!);
    }
    v.mustBe(Struct);
    foreach (var (i, x) in index) {
        if (i > 0) {
            if (v.Kind() == Ptr && v.typ().Elem().Kind() == abi.Struct) {
                if (v.IsNil()) {
                    return (new ΔValue(nil), errors.New("reflect: indirection through nil pointer to embedded struct field "u8 + nameFor(v.typ().Elem())));
                }
                v = v.Elem();
            }
        }
        v = v.Field(x);
    }
    return (v, default!);
}

// FieldByName returns the struct field with the given name.
// It returns the zero Value if no field was found.
// It panics if v's Kind is not [Struct].
public static ΔValue FieldByName(this ΔValue v, @string name) {
    v.mustBe(Struct);
    {
        var (f, ok) = toRType(v.typ()).FieldByName(name); if (ok) {
            return v.FieldByIndex(f.Index);
        }
    }
    return new ΔValue(nil);
}

// FieldByNameFunc returns the struct field with a name
// that satisfies the match function.
// It panics if v's Kind is not [Struct].
// It returns the zero Value if no field was found.
public static ΔValue FieldByNameFunc(this ΔValue v, Func<@string, bool> match) {
    {
        var (f, ok) = toRType(v.typ()).FieldByNameFunc(match); if (ok) {
            return v.FieldByIndex(f.Index);
        }
    }
    return new ΔValue(nil);
}

// CanFloat reports whether [Value.Float] can be used without panicking.
public static bool CanFloat(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// go2cs generated this placeholder — func Float is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static ж<abi.Type> uint8Type = rtypeOf((uint8)0);

// go2cs generated this placeholder — func Index is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// CanInt reports whether Int can be used without panicking.
public static bool CanInt(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// go2cs generated this placeholder — func Int is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// CanInterface reports whether [Value.Interface] can be used without panicking.
public static bool CanInterface(this ΔValue v) {
    if (v.flag == 0) {
        throw panic(Ꮡ(new ValueError("reflect.Value.CanInterface"u8, Invalid)));
    }
    return (flag)(v.flag & flagRO) == 0;
}

// go2cs generated this placeholder — func Interface is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func valueInterface is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func InterfaceData is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func IsNil is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// IsValid reports whether v represents a value.
// It returns false if v is the zero Value.
// If [Value.IsValid] returns false, all other methods except String panic.
// Most functions and methods never return an invalid Value.
// If one does, its documentation states the conditions explicitly.
public static bool IsValid(this ΔValue v) {
    return v.flag != 0;
}

// go2cs generated this placeholder — func IsZero is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// isZero For all zeros, performance is not as good as
// return bytealg.Count(b, byte(0)) == len(b)
internal static bool isZero(slice<byte> b) {
    if (len(b) == 0) {
        return true;
    }
    const nint n = 32;
    // Align memory addresses to 8 bytes.
    while ((uintptr)Ꮡ(b, 0) % 8 != 0) {
        if (b[0] != 0) {
            return false;
        }
        b = b[1..];
        if (len(b) == 0) {
            return true;
        }
    }
    while (len(b) % 8 != 0) {
        if (b[len(b) - 1] != 0) {
            return false;
        }
        b = b[..(int)(len(b) - 1)];
    }
    if (len(b) == 0) {
        return true;
    }
    var w = @unsafe.Slice(Ꮡ(b, 0).Reinterpret<byte, uint64>(), len(b) / 8);
    while (len(w) % n != 0) {
        if (w[0] != 0) {
            return false;
        }
        w = w[1..];
    }
    while (len(w) >= n) {
        if (w[0] != 0 || w[1] != 0 || w[2] != 0 || w[3] != 0 || w[4] != 0 || w[5] != 0 || w[6] != 0 || w[7] != 0 || w[8] != 0 || w[9] != 0 || w[10] != 0 || w[11] != 0 || w[12] != 0 || w[13] != 0 || w[14] != 0 || w[15] != 0 || w[16] != 0 || w[17] != 0 || w[18] != 0 || w[19] != 0 || w[20] != 0 || w[21] != 0 || w[22] != 0 || w[23] != 0 || w[24] != 0 || w[25] != 0 || w[26] != 0 || w[27] != 0 || w[28] != 0 || w[29] != 0 || w[30] != 0 || w[31] != 0) {
            return false;
        }
        w = w[(int)(n)..];
    }
    return true;
}

// go2cs generated this placeholder — func SetZero is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Kind returns v's Kind.
// If v is the zero Value ([Value.IsValid] returns false), Kind returns Invalid.
public static ΔKind Kind(this ΔValue v) {
    return v.kind();
}

// go2cs generated this placeholder — func Len is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static nint lenNonSlice(this ΔValue v) {
    {
        ΔKind k = v.kind();
        var exprᴛ1 = k;
        if (exprᴛ1 == Array) {
            var tt = v.typ().Reinterpret<abi.Type, arrayType>();
            return (nint)(~tt).Len;
        }
        if (exprᴛ1 == Chan) {
            return chanlen((uintptr)v.pointer());
        }
        if (exprᴛ1 == Map) {
            return maplen((uintptr)v.pointer());
        }
        if (exprᴛ1 == ΔString) {
            return ((ж<unsafeheader.String>)(uintptr)(v.ptr)).Value.Len;
        }
        if (exprᴛ1 == Ptr) {
            if (v.typ().Elem().Kind() == abi.Array) {
                // String is bigger than a word; assume flagIndir.
                return v.typ().Elem().Len();
            }
            throw panic("reflect: call of reflect.Value.Len on ptr to non-array Value");
        }
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Len"u8, v.kind())));
}

internal static ж<abi.Type> stringType = rtypeOf((@string)""u8);

// go2cs generated this placeholder — func MapIndex is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MapKeys is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// hiter's structure matches runtime.hiter's structure.
// Having a clone here allows us to embed a map iterator
// inside type MapIter so that MapIters can be re-used
// without doing any allocations.
[GoType] partial struct hiter {
    internal @unsafe.Pointer key;
    internal @unsafe.Pointer elem;
    internal @unsafe.Pointer t;
    internal @unsafe.Pointer h;
    internal @unsafe.Pointer buckets;
    internal @unsafe.Pointer bptr;
    internal ж<slice<@unsafe.Pointer>> overflow;
    internal ж<slice<@unsafe.Pointer>> oldoverflow;
    internal uintptr startBucket;
    internal uint8 offset;
    internal bool wrapped;
    public uint8 B;
    internal uint8 i;
    internal uintptr bucket;
    internal uintptr checkBucket;
}

[GoRecv] internal static bool initialized(this ref hiter h) {
    return h.t != nil;
}

// A MapIter is an iterator for ranging over a map.
// See [Value.MapRange].
[GoType] partial struct MapIter {
    internal ΔValue m;
    internal hiter hiter;
}

// go2cs generated this placeholder — func Key is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetIterKey is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Value is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetIterValue is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Next is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Reset is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MapRange is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Force slow panicking path not inlined, so it won't add to the
// inlining budget of the caller.
// TODO: undo when the inliner is no longer bottom-up only.
//
//go:noinline
internal static void panicNotMap(this flag f) {
    f.mustBe(Map);
}

// copyVal returns a Value containing the map key or value at ptr,
// allocating a new variable as needed.
internal static ΔValue copyVal(ж<abi.Type> Ꮡtyp, flag fl, @unsafe.Pointer ptr) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (typ.IfaceIndir()) {
        // Copy result so future changes to the map
        // won't change the underlying value.
        @unsafe.Pointer c = (uintptr)unsafe_New(Ꮡtyp);
        typedmemmove(Ꮡtyp, c, ptr);
        return new ΔValue(Ꮡtyp, c.Value, (flag)(fl | flagIndir));
    }
    return new ΔValue(Ꮡtyp, ~(ж<@unsafe.Pointer>)(uintptr)(ptr), fl);
}

// go2cs generated this placeholder — func Method is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// NumMethod returns the number of methods in the value's method set.
//
// For a non-interface type, it returns the number of exported methods.
//
// For an interface type, it returns the number of exported and unexported methods.
public static nint NumMethod(this ΔValue v) {
    if (v.typ() == nil) {
        throw panic(Ꮡ(new ValueError("reflect.Value.NumMethod"u8, Invalid)));
    }
    if ((flag)(v.flag & flagMethod) != 0) {
        return 0;
    }
    return toRType(v.typ()).NumMethod();
}

// MethodByName returns a function value corresponding to the method
// of v with the given name.
// The arguments to a Call on the returned function should not include
// a receiver; the returned function will always use v as the receiver.
// It returns the zero Value if no method was found.
public static ΔValue MethodByName(this ΔValue v, @string name) {
    if (v.typ() == nil) {
        throw panic(Ꮡ(new ValueError("reflect.Value.MethodByName"u8, Invalid)));
    }
    if ((flag)(v.flag & flagMethod) != 0) {
        return new ΔValue(nil);
    }
    var (m, ok) = toRType(v.typ()).MethodByName(name);
    if (!ok) {
        return new ΔValue(nil);
    }
    return v.Method(m.Index);
}

// go2cs generated this placeholder — func NumField is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// OverflowComplex reports whether the complex128 x cannot be represented by v's type.
// It panics if v's Kind is not [Complex64] or [Complex128].
public static bool OverflowComplex(this ΔValue v, complex128 x) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Complex64) {
        return overflowFloat32(real(x)) || overflowFloat32(imag(x));
    }
    if (exprᴛ1 == Complex128) {
        return false;
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.OverflowComplex"u8, v.kind())));
}

// OverflowFloat reports whether the float64 x cannot be represented by v's type.
// It panics if v's Kind is not [Float32] or [Float64].
public static bool OverflowFloat(this ΔValue v, float64 x) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Float32) {
        return overflowFloat32(x);
    }
    if (exprᴛ1 == Float64) {
        return false;
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.OverflowFloat"u8, v.kind())));
}

internal static bool overflowFloat32(float64 x) {
    if (x < 0D) {
        x = -x;
    }
    return Δmath.MaxFloat32 < x && x <= Δmath.MaxFloat64;
}

// OverflowInt reports whether the int64 x cannot be represented by v's type.
// It panics if v's Kind is not [Int], [Int8], [Int16], [Int32], or [Int64].
public static bool OverflowInt(this ΔValue v, int64 x) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        var bitSize = v.typ().Size() * 8;
        var trunc = (x.Lsh((uint64)((64 - bitSize)))).Rsh((uint64)((64 - bitSize)));
        return x != trunc;
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.OverflowInt"u8, v.kind())));
}

// OverflowUint reports whether the uint64 x cannot be represented by v's type.
// It panics if v's Kind is not [Uint], [Uintptr], [Uint8], [Uint16], [Uint32], or [Uint64].
public static bool OverflowUint(this ΔValue v, uint64 x) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uintptr || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64) {
        var bitSize = v.typ_.Size() * 8; // ok to use v.typ_ directly as Size doesn't escape
        var trunc = (x.Lsh((uint64)((64 - bitSize)))).Rsh((uint64)((64 - bitSize)));
        return x != trunc;
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.OverflowUint"u8, v.kind())));
}

//go:nocheckptr
// This prevents inlining Value.Pointer when -d=checkptr is enabled,
// which ensures cmd/compile can recognize unsafe.Pointer(v.Pointer())
// and make an exception.
// go2cs generated this placeholder — func Pointer is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Recv receives and returns a value from the channel v.
// It panics if v's Kind is not [Chan].
// The receive blocks until a value is ready.
// The boolean value ok is true if the value x corresponds to a send
// on the channel, false if it is a zero value received because the channel is closed.
public static (ΔValue x, bool ok) Recv(this ΔValue v) {
    v.mustBe(Chan);
    v.mustBeExported();
    return v.recv(false);
}

// go2cs generated this placeholder — func recv is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Send sends x on the channel v.
// It panics if v's kind is not [Chan] or if x's type is not the same type as v's element type.
// As in Go, x's value must be assignable to the channel's element type.
public static void Send(this ΔValue v, ΔValue x) {
    v.mustBe(Chan);
    v.mustBeExported();
    v.send(x, false);
}

// go2cs generated this placeholder — func send is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Set is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetBool is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetBytes is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func setRunes is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetComplex is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetFloat is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetInt is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetLen is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// SetCap sets v's capacity to n.
// It panics if v's Kind is not [Slice] or if n is smaller than the length or
// greater than the capacity of the slice.
public static void SetCap(this ΔValue v, nint n) {
    v.mustBeAssignable();
    v.mustBe(ΔSlice);
    var s = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    if (n < (~s).Len || n > (~s).Cap) {
        throw panic("reflect: slice capacity out of range in SetCap");
    }
    s.Value.Cap = n;
}

// go2cs generated this placeholder — func SetMapIndex is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SetUint is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// SetPointer sets the [unsafe.Pointer] value v to x.
// It panics if v's Kind is not [UnsafePointer].
public static void SetPointer(this ΔValue v, @unsafe.Pointer x) {
    v.mustBeAssignable();
    v.mustBe(ΔUnsafePointer);
    ((ж<@unsafe.Pointer>)(uintptr)(v.ptr)).Value = x;
}

// go2cs generated this placeholder — func SetString is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Slice is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Slice3 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func String is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidValueˢ = "<invalid Value>"u8;

internal static @string stringNonString(this ΔValue v) {
    if (v.kind() == Invalid) {
        return invalidValueˢ;
    }
    // If you call String on a reflect.Value of other type, it's better to
    // print something than to panic. Useful in debugging.
    return "<"u8 + v.Type().String() + " Value>"u8;
}

// TryRecv attempts to receive a value from the channel v but will not block.
// It panics if v's Kind is not [Chan].
// If the receive delivers a value, x is the transferred value and ok is true.
// If the receive cannot finish without blocking, x is the zero Value and ok is false.
// If the channel is closed, x is the zero value for the channel's element type and ok is false.
public static (ΔValue x, bool ok) TryRecv(this ΔValue v) {
    v.mustBe(Chan);
    v.mustBeExported();
    return v.recv(true);
}

// TrySend attempts to send x on the channel v but will not block.
// It panics if v's Kind is not [Chan].
// It reports whether the value was sent.
// As in Go, x's value must be assignable to the channel's element type.
public static bool TrySend(this ΔValue v, ΔValue x) {
    v.mustBe(Chan);
    v.mustBeExported();
    return v.send(x, true);
}

// go2cs generated this placeholder — func Type is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:noinline
internal static ΔType typeSlow(this ΔValue v) {
    return new rtypeжΔType(toRType(v.abiTypeSlow()));
}

internal static ж<abi.Type> abiType(this ΔValue v) {
    if (v.flag != 0 && (flag)(v.flag & flagMethod) == 0) {
        return v.typ();
    }
    return v.abiTypeSlow();
}

internal static ж<abi.Type> abiTypeSlow(this ΔValue v) {
    if (v.flag == 0) {
        throw panic(Ꮡ(new ValueError("reflect.Value.Type"u8, Invalid)));
    }
    var typ = v.typ();
    if ((flag)(v.flag & flagMethod) == 0) {
        return v.typ();
    }
    // Method value.
    // v.typ describes the receiver, not the method type.
    nint i = ((nint)(uintptr)v.flag >> (int)(flagMethodShift));
    if (v.typ().Kind() == abi.Interface) {
        // Method on interface.
        var tt = typ.Reinterpret<abi.Type, interfaceType>();
        if ((nuint)i >= (nuint)len((~tt).Methods)) {
            throw panic("reflect: internal error: invalid method index");
        }
        var mΔ1 = Ꮡ((~tt).Methods, i);
        return typeOffFor(typ, (~mΔ1).Typ);
    }
    // Method on concrete type.
    var ms = typ.ExportedMethods();
    if ((nuint)i >= (nuint)len(ms)) {
        throw panic("reflect: internal error: invalid method index");
    }
    var m = ms[i];
    return typeOffFor(typ, m.Mtyp);
}

// CanUint reports whether [Value.Uint] can be used without panicking.
public static bool CanUint(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// go2cs generated this placeholder — func Uint is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:nocheckptr
// This prevents inlining Value.UnsafeAddr when -d=checkptr is enabled,
// which ensures cmd/compile can recognize unsafe.Pointer(v.UnsafeAddr())
// and make an exception.

// UnsafeAddr returns a pointer to v's data, as a uintptr.
// It panics if v is not addressable.
//
// It's preferred to use uintptr(Value.Addr().UnsafePointer()) to get the equivalent result.
public static uintptr UnsafeAddr(this ΔValue v) {
    if (v.typ() == nil) {
        throw panic(Ꮡ(new ValueError("reflect.Value.UnsafeAddr"u8, Invalid)));
    }
    if ((flag)(v.flag & flagAddr) == 0) {
        throw panic("reflect.Value.UnsafeAddr of unaddressable value");
    }
    // The compiler loses track as it converts to uintptr. Force escape.
    escapes(v.ptr);
    return (uintptr)v.ptr;
}

// go2cs generated this placeholder — func UnsafePointer is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// StringHeader is the runtime representation of a string.
// It cannot be used safely or portably and its representation may
// change in a later release.
// Moreover, the Data field is not sufficient to guarantee the data
// it references will not be garbage collected, so programs must keep
// a separate, correctly typed pointer to the underlying data.
//
// Deprecated: Use unsafe.String or unsafe.StringData instead.
[GoType] partial struct StringHeader {
    public uintptr Data;
    public nint Len;
}

// SliceHeader is the runtime representation of a slice.
// It cannot be used safely or portably and its representation may
// change in a later release.
// Moreover, the Data field is not sufficient to guarantee the data
// it references will not be garbage collected, so programs must keep
// a separate, correctly typed pointer to the underlying data.
//
// Deprecated: Use unsafe.Slice or unsafe.SliceData instead.
[GoType] partial struct SliceHeader {
    public uintptr Data;
    public nint Len;
    public nint Cap;
}

internal static void typesMustMatch(@string what, ΔType t1, ΔType t2) {
    if (!AreEqual(t1, t2)) {
        throw panic(what + ": " + t1.String() + " != " + t2.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string iLenˢ = "i < len"u8;

// arrayAt returns the i-th element of p,
// an array whose elements are eltSize bytes wide.
// The array pointed at by p must have at least i+1 elements:
// it is invalid (but impossible to check here) to pass i >= len,
// because then the result will point outside the array.
// whySafe must explain why i < len. (Passing "i < len" is fine;
// the benefit is to surface this assumption at the call site.)
internal static @unsafe.Pointer arrayAt(@unsafe.Pointer p, nint i, uintptr eltSize, @string whySafe) {
    return (uintptr)add(p, (uintptr)i * eltSize, iLenˢ);
}

// go2cs generated this placeholder — func Grow is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// grow is identical to Grow but does not check for assignability.
internal static void grow(this ΔValue v, nint n) {
    var p = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    switch (ᐧ) {
    case {} when n is < 0: {
        throw panic("reflect.Value.Grow: negative len");
        break;
    }
    case {} when (~p).Len + n is < 0: {
        throw panic("reflect.Value.Grow: slice overflow");
        break;
    }
    case {} when (~p).Len + n > (~p).Cap: {
        var t = v.typ().Elem();
        p.Value = growslice(t, p.Value, n);
        break;
    }}

}

// go2cs generated this placeholder — func extendSlice is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Clear is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Append is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func AppendSlice is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Copy is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// A runtimeSelect is a single case passed to rselect.
// This must match ../runtime/select.go:/runtimeSelect
[GoType] partial struct runtimeSelect {
    internal SelectDir dir;      // SelectSend, SelectRecv or SelectDefault
    internal ж<rtype> typ;      // channel type
    internal @unsafe.Pointer ch; // channel
    internal @unsafe.Pointer val; // ptr to data (SendDir) or ptr to receive buffer (RecvDir)
}

// rselect runs a select. It returns the index of the chosen case.
// If the case was a receive, val is filled in with the received value.
// The conventional OK bool indicates whether the receive corresponds
// to a sent value.
//
// rselect generally doesn't escape the runtimeSelect slice, except
// that for the send case the value to send needs to escape. We don't
// have a way to represent that in the function signature. So we handle
// that with a forced escape in function Select.
//
//go:noescape
internal static partial (nint chosen, bool recvOK) rselect(slice<runtimeSelect> _);

[GoType("num:nint")] partial struct SelectDir;

// NOTE: These values must match ../runtime/select.go:/selectDir.
internal static SelectDir _ᴛ1ʗ => /* iota */ 0;
public static SelectDir SelectSend => 1; // case Chan <- Send
public static SelectDir SelectRecv => 2; // case <-Chan:
public static SelectDir SelectDefault => 3; // default

// A SelectCase describes a single case in a select operation.
// The kind of case depends on Dir, the communication direction.
//
// If Dir is SelectDefault, the case represents a default case.
// Chan and Send must be zero Values.
//
// If Dir is SelectSend, the case represents a send operation.
// Normally Chan's underlying value must be a channel, and Send's underlying value must be
// assignable to the channel's element type. As a special case, if Chan is a zero Value,
// then the case is ignored, and the field Send will also be ignored and may be either zero
// or non-zero.
//
// If Dir is [SelectRecv], the case represents a receive operation.
// Normally Chan's underlying value must be a channel and Send must be a zero Value.
// If Chan is a zero Value, then the case is ignored, but Send must still be a zero Value.
// When a receive operation is selected, the received Value is returned by Select.
[GoType] partial struct SelectCase {
    public SelectDir Dir; // direction of case
    public ΔValue Chan;     // channel to use (for send or receive)
    public ΔValue Send;     // value to send (for send)
}

// go2cs generated this placeholder — func Select is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

/*
 * constructors
 */
// implemented in package runtime

//go:noescape
internal static partial @unsafe.Pointer unsafe_New(ж<abi.Type> _);

//go:noescape
internal static partial @unsafe.Pointer unsafe_NewArray(ж<abi.Type> _Δp0, nint _Δp1);

// go2cs generated this placeholder — func MakeSlice is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func SliceAt is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MakeChan is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MakeMap is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func MakeMapWithSize is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Indirect returns the value that v points to.
// If v is a nil pointer, Indirect returns a zero Value.
// If v is not a pointer, Indirect returns v.
public static ΔValue Indirect(ΔValue v) {
    if (v.Kind() != ΔPointer) {
        return v;
    }
    return v.Elem();
}

// go2cs generated this placeholder — func ValueOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Zero is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:linkname zeroVal runtime.zeroVal
internal static ж<array<byte>> ᏑzeroVal = new StandardBox<array<byte>>(new array<byte>(1024));
internal static ref array<byte> zeroVal => ref ᏑzeroVal.Value;

// go2cs generated this placeholder — func New is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func NewAt is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// assignTo returns a value v that can be assigned directly to dst.
// It panics if v is not assignable to dst.
// For a conversion to an interface type, target, if not nil,
// is a suggested scratch space to use.
// target must be initialized memory (or nil).
internal static ΔValue assignTo(this ΔValue v, @string context, ж<abi.Type> Ꮡdst, @unsafe.Pointer target) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    if ((flag)(v.flag & flagMethod) != 0) {
        v = makeMethodValue(context, v);
    }
    switch (ᐧ) {
    case {} when directlyAssignable(Ꮡdst, v.typ()): {
        var fl = (flag)((flag)(v.flag & ((flag)(flagAddr | flagIndir))) | v.flag.ro());
        fl |= (flag)(((flag)(uintptr)(uint8)dst.Kind()));
        return new ΔValue( // Overwrite type so that they match.
 // Same memory layout, so no harm done.
Ꮡdst, v.ptr, fl);
    }
    case {} when implements(Ꮡdst, v.typ()): {
        if (v.Kind() == ΔInterface && v.IsNil()) {
            // A nil ReadWriter passed to nil Reader is OK,
            // but using ifaceE2I below will panic.
            // Avoid the panic by returning a nil dst (e.g., Reader) explicitly.
            return new ΔValue(Ꮡdst, nil, ((flag)(uintptr)(nuint)ΔInterface));
        }
        var x = valueInterface(v, false);
        if (target == nil) {
            target.Value = (uintptr)unsafe_New(Ꮡdst);
        }
        if (Ꮡdst.NumMethod() == 0){
            ((ж<any>)(uintptr)(target)).ValueSlot = x;
        } else {
            ifaceE2I(Ꮡdst, x, target);
        }
        return new ΔValue(Ꮡdst, target.Value, (flag)(flagIndir | ((flag)(uintptr)(nuint)ΔInterface)));
    }}

    // Failed.
    throw panic(context + ": value of type " + stringFor(v.typ()) + " is not assignable to type " + stringFor(Ꮡdst));
}

// go2cs generated this placeholder — func Convert is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// CanConvert reports whether the value v can be converted to type t.
// If v.CanConvert(t) returns true then v.Convert(t) will not panic.
public static bool CanConvert(this ΔValue v, ΔType t) {
    var vt = v.Type();
    if (!vt.ConvertibleTo(t)) {
        return false;
    }
    // Converting from slice to array or to pointer-to-array can panic
    // depending on the value.
    switch (ᐧ) {
    case {} when vt.Kind() == ΔSlice && t.Kind() == Array: {
        if (t.Len() > v.Len()) {
            return false;
        }
        break;
    }
    case {} when vt.Kind() == ΔSlice && t.Kind() == ΔPointer && t.Elem().Kind() == Array: {
        nint n = t.Elem().Len();
        if (n > v.Len()) {
            return false;
        }
        break;
    }}

    return true;
}

// Comparable reports whether the value v is comparable.
// If the type of v is an interface, this checks the dynamic type.
// If this reports true then v.Interface() == x will not panic for any x,
// nor will v.Equal(u) for any Value u.
public static bool Comparable(this ΔValue v) {
    ΔKind k = v.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Invalid) {
        return false;
    }
    if (exprᴛ1 == Array) {
        var exprᴛ2 = v.Type().Elem().Kind();
        if (exprᴛ2 == ΔInterface || exprᴛ2 == Array || exprᴛ2 == Struct) {
            for (nint i = 0; i < v.Type().Len(); i++) {
                if (!v.Index(i).Comparable()) {
                    return false;
                }
            }
            return true;
        }

        return v.Type().Comparable();
    }
    if (exprᴛ1 == ΔInterface) {
        return v.IsNil() || v.Elem().Comparable();
    }
    if (exprᴛ1 == Struct) {
        for (nint i = 0; i < v.NumField(); i++) {
            if (!v.Field(i).Comparable()) {
                return false;
            }
        }
        return true;
    }
    { /* default: */
        return v.Type().Comparable();
    }

}

// Equal reports true if v is equal to u.
// For two invalid values, Equal will report true.
// For an interface value, Equal will compare the value within the interface.
// Otherwise, If the values have different types, Equal will report false.
// Otherwise, for arrays and structs Equal will compare each element in order,
// and report false if it finds non-equal elements.
// During all comparisons, if values of the same type are compared,
// and the type is not comparable, Equal will panic.
public static bool Equal(this ΔValue v, ΔValue u) {
    if (v.Kind() == ΔInterface) {
        v = v.Elem();
    }
    if (u.Kind() == ΔInterface) {
        u = u.Elem();
    }
    if (!v.IsValid() || !u.IsValid()) {
        return v.IsValid() == u.IsValid();
    }
    if (v.Kind() != u.Kind() || !AreEqual(v.Type(), u.Type())) {
        return false;
    }
    // Handle each Kind directly rather than calling valueInterface
    // to avoid allocating.
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == ΔBool) {
        return v.Bool() == u.Bool();
    }
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        return v.Int() == u.Int();
    }
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr) {
        return v.Uint() == u.Uint();
    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        return v.Float() == u.Float();
    }
    if (exprᴛ1 == Complex64 || exprᴛ1 == Complex128) {
        return v.Complex() == u.Complex();
    }
    if (exprᴛ1 == ΔString) {
        return v.String() == u.String();
    }
    if (exprᴛ1 == Chan || exprᴛ1 == ΔPointer || exprᴛ1 == ΔUnsafePointer) {
        return v.Pointer() == u.Pointer();
    }
    if (exprᴛ1 == Array) {
        do {
            nint vl = v.Len();
            if (vl == 0) {
                // u and v have the same type so they have the same length
                // panic on [0]func()
                if (!v.Type().Elem().Comparable()) {
                    break;
                }
                return true;
            }
            for (nint i = 0; i < vl; i++) {
                if (!v.Index(i).Equal(u.Index(i))) {
                    return false;
                }
            }
            return true;
        } while (false);
    }
    else if (exprᴛ1 == Struct) {
        nint nf = v.NumField();
        for (nint i = 0; i < nf; i++) {
            // u and v have the same type so they have the same fields
            if (!v.Field(i).Equal(u.Field(i))) {
                return false;
            }
        }
        return true;
    }
    else if (exprᴛ1 == Func || exprᴛ1 == Map || exprᴛ1 == ΔSlice) {
        do {
            break;
        } while (false);
    }
    else { /* default: */
        throw panic("reflect.Value.Equal: invalid Kind");
    }

    throw panic("reflect.Value.Equal: values of type " + v.Type().String() + " are not comparable");
}

// convertOp returns the function to convert a value of type src
// to a value of type dst. If the conversion is illegal, convertOp returns nil.
internal static Func<ΔValue, ΔType, ΔValue> convertOp(ж<abi.Type> Ꮡdst, ж<abi.Type> Ꮡsrc) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    var exprᴛ1 = ((ΔKind)(nuint)(uint8)src.Kind());
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        var exprᴛ2 = ((ΔKind)(nuint)(uint8)dst.Kind());
        if (exprᴛ2 == ΔInt || exprᴛ2 == Int8 || exprᴛ2 == Int16 || exprᴛ2 == Int32 || exprᴛ2 == Int64 || exprᴛ2 == ΔUint || exprᴛ2 == Uint8 || exprᴛ2 == Uint16 || exprᴛ2 == Uint32 || exprᴛ2 == Uint64 || exprᴛ2 == Uintptr) {
            return cvtInt;
        }
        if (exprᴛ2 == Float32 || exprᴛ2 == Float64) {
            return cvtIntFloat;
        }
        if (exprᴛ2 == ΔString) {
            return cvtIntString;
        }

    }
    else if (exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr) {
        var exprᴛ3 = ((ΔKind)(nuint)(uint8)dst.Kind());
        if (exprᴛ3 == ΔInt || exprᴛ3 == Int8 || exprᴛ3 == Int16 || exprᴛ3 == Int32 || exprᴛ3 == Int64 || exprᴛ3 == ΔUint || exprᴛ3 == Uint8 || exprᴛ3 == Uint16 || exprᴛ3 == Uint32 || exprᴛ3 == Uint64 || exprᴛ3 == Uintptr) {
            return cvtUint;
        }
        if (exprᴛ3 == Float32 || exprᴛ3 == Float64) {
            return cvtUintFloat;
        }
        if (exprᴛ3 == ΔString) {
            return cvtUintString;
        }

    }
    else if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        var exprᴛ4 = ((ΔKind)(nuint)(uint8)dst.Kind());
        if (exprᴛ4 == ΔInt || exprᴛ4 == Int8 || exprᴛ4 == Int16 || exprᴛ4 == Int32 || exprᴛ4 == Int64) {
            return cvtFloatInt;
        }
        if (exprᴛ4 == ΔUint || exprᴛ4 == Uint8 || exprᴛ4 == Uint16 || exprᴛ4 == Uint32 || exprᴛ4 == Uint64 || exprᴛ4 == Uintptr) {
            return cvtFloatUint;
        }
        if (exprᴛ4 == Float32 || exprᴛ4 == Float64) {
            return cvtFloat;
        }

    }
    else if (exprᴛ1 == Complex64 || exprᴛ1 == Complex128) {
        var exprᴛ5 = ((ΔKind)(nuint)(uint8)dst.Kind());
        if (exprᴛ5 == Complex64 || exprᴛ5 == Complex128) {
            return cvtComplex;
        }

    }
    else if (exprᴛ1 == ΔString) {
        if (dst.Kind() == abi.Slice && pkgPathFor(Ꮡdst.Elem()) == ""u8) {
            var exprᴛ6 = ((ΔKind)(nuint)(uint8)Ꮡdst.Elem().Kind());
            if (exprᴛ6 == Uint8) {
                return cvtStringBytes;
            }
            if (exprᴛ6 == Int32) {
                return cvtStringRunes;
            }

        }
    }
    else if (exprᴛ1 == ΔSlice) {
        if (dst.Kind() == abi.ΔString && pkgPathFor(Ꮡsrc.Elem()) == ""u8) {
            var exprᴛ7 = ((ΔKind)(nuint)(uint8)Ꮡsrc.Elem().Kind());
            if (exprᴛ7 == Uint8) {
                return cvtBytesString;
            }
            if (exprᴛ7 == Int32) {
                return cvtRunesString;
            }

        }
        if (dst.Kind() == abi.Pointer && Ꮡdst.Elem().Kind() == abi.Array && Ꮡsrc.Elem() == Ꮡdst.Elem().Elem()) {
            // "x is a slice, T is a pointer-to-array type,
            // and the slice and array types have identical element types."
            return cvtSliceArrayPtr;
        }
        if (dst.Kind() == abi.Array && Ꮡsrc.Elem() == Ꮡdst.Elem()) {
            // "x is a slice, T is an array type,
            // and the slice and array types have identical element types."
            return cvtSliceArray;
        }
    }
    else if (exprᴛ1 == Chan) {
        if (dst.Kind() == abi.Chan && specialChannelAssignability(Ꮡdst, Ꮡsrc)) {
            return cvtDirect;
        }
    }

    // dst and src have same underlying type.
    if (haveIdenticalUnderlyingType(Ꮡdst, Ꮡsrc, false)) {
        return cvtDirect;
    }
    // dst and src are non-defined pointer types with same underlying base type.
    if (dst.Kind() == abi.Pointer && nameFor(Ꮡdst) == ""u8 && src.Kind() == abi.Pointer && nameFor(Ꮡsrc) == ""u8 && haveIdenticalUnderlyingType(elem(Ꮡdst), elem(Ꮡsrc), false)) {
        return cvtDirect;
    }
    if (implements(Ꮡdst, Ꮡsrc)) {
        if (src.Kind() == abi.Interface) {
            return cvtI2I;
        }
        return cvtT2I;
    }
    return default!;
}

// makeInt returns a Value of type t equal to bits (possibly truncated),
// where t is a signed or unsigned int type.
internal static ΔValue makeInt(flag f, uint64 bits, ΔType t) {
    var typ = t.common();
    @unsafe.Pointer ptr = (uintptr)unsafe_New(typ);
    var exprᴛ1 = typ.Size();
    if (exprᴛ1 == 1) {
        ((ж<uint8>)(uintptr)(ptr)).Value = (uint8)bits;
    }
    else if (exprᴛ1 == 2) {
        ((ж<uint16>)(uintptr)(ptr)).Value = (uint16)bits;
    }
    else if (exprᴛ1 == 4) {
        ((ж<uint32>)(uintptr)(ptr)).Value = (uint32)bits;
    }
    else if (exprᴛ1 == 8) {
        ((ж<uint64>)(uintptr)(ptr)).Value = bits;
    }

    return new ΔValue(typ, ptr.Value, (flag)((flag)(f | flagIndir) | ((flag)(uintptr)(uint8)typ.Kind())));
}

// makeFloat returns a Value of type t equal to v (possibly truncated to float32),
// where t is a float32 or float64 type.
internal static ΔValue makeFloat(flag f, float64 v, ΔType t) {
    var typ = t.common();
    @unsafe.Pointer ptr = (uintptr)unsafe_New(typ);
    var exprᴛ1 = typ.Size();
    if (exprᴛ1 == 4) {
        ((ж<float32>)(uintptr)(ptr)).Value = (float32)v;
    }
    else if (exprᴛ1 == 8) {
        ((ж<float64>)(uintptr)(ptr)).Value = v;
    }

    return new ΔValue(typ, ptr.Value, (flag)((flag)(f | flagIndir) | ((flag)(uintptr)(uint8)typ.Kind())));
}

// makeFloat32 returns a Value of type t equal to v, where t is a float32 type.
internal static ΔValue makeFloat32(flag f, float32 v, ΔType t) {
    var typ = t.common();
    @unsafe.Pointer ptr = (uintptr)unsafe_New(typ);
    ((ж<float32>)(uintptr)(ptr)).Value = v;
    return new ΔValue(typ, ptr.Value, (flag)((flag)(f | flagIndir) | ((flag)(uintptr)(uint8)typ.Kind())));
}

// makeComplex returns a Value of type t equal to v (possibly truncated to complex64),
// where t is a complex64 or complex128 type.
internal static ΔValue makeComplex(flag f, complex128 v, ΔType t) {
    var typ = t.common();
    @unsafe.Pointer ptr = (uintptr)unsafe_New(typ);
    var exprᴛ1 = typ.Size();
    if (exprᴛ1 == 8) {
        ((ж<complex64>)(uintptr)(ptr)).Value = (complex64)v;
    }
    else if (exprᴛ1 == 16) {
        ((ж<complex128>)(uintptr)(ptr)).Value = v;
    }

    return new ΔValue(typ, ptr.Value, (flag)((flag)(f | flagIndir) | ((flag)(uintptr)(uint8)typ.Kind())));
}

internal static ΔValue makeString(flag f, @string v, ΔType t) {
    var ret = New(t).Elem();
    ret.SetString(v);
    ret.flag = (flag)((flag)(ret.flag & ~flagAddr) | f);
    return ret;
}

internal static ΔValue makeBytes(flag f, slice<byte> v, ΔType t) {
    var ret = New(t).Elem();
    ret.SetBytes(v);
    ret.flag = (flag)((flag)(ret.flag & ~flagAddr) | f);
    return ret;
}

internal static ΔValue makeRunes(flag f, slice<rune> v, ΔType t) {
    var ret = New(t).Elem();
    ret.setRunes(v);
    ret.flag = (flag)((flag)(ret.flag & ~flagAddr) | f);
    return ret;
}

// These conversion functions are returned by convertOp
// for classes of conversions. For example, the first function, cvtInt,
// takes any value v of signed int type and returns the value converted
// to type t, where t is any signed or unsigned int type.

// convertOp: intXX -> [u]intXX
internal static ΔValue cvtInt(ΔValue v, ΔType t) {
    return makeInt(v.flag.ro(), (uint64)v.Int(), t);
}

// convertOp: uintXX -> [u]intXX
internal static ΔValue cvtUint(ΔValue v, ΔType t) {
    return makeInt(v.flag.ro(), v.Uint(), t);
}

// convertOp: floatXX -> intXX
internal static ΔValue cvtFloatInt(ΔValue v, ΔType t) {
    return makeInt(v.flag.ro(), (uint64)(int64)v.Float(), t);
}

// convertOp: floatXX -> uintXX
internal static ΔValue cvtFloatUint(ΔValue v, ΔType t) {
    return makeInt(v.flag.ro(), (uint64)v.Float(), t);
}

// convertOp: intXX -> floatXX
internal static ΔValue cvtIntFloat(ΔValue v, ΔType t) {
    return makeFloat(v.flag.ro(), (float64)v.Int(), t);
}

// convertOp: uintXX -> floatXX
internal static ΔValue cvtUintFloat(ΔValue v, ΔType t) {
    return makeFloat(v.flag.ro(), (float64)v.Uint(), t);
}

// convertOp: floatXX -> floatXX
internal static ΔValue cvtFloat(ΔValue v, ΔType t) {
    if (v.Type().Kind() == Float32 && t.Kind() == Float32) {
        // Don't do any conversion if both types have underlying type float32.
        // This avoids converting to float64 and back, which will
        // convert a signaling NaN to a quiet NaN. See issue 36400.
        return makeFloat32(v.flag.ro(), ~(ж<float32>)(uintptr)(v.ptr), t);
    }
    return makeFloat(v.flag.ro(), v.Float(), t);
}

// convertOp: complexXX -> complexXX
internal static ΔValue cvtComplex(ΔValue v, ΔType t) {
    return makeComplex(v.flag.ro(), v.Complex(), t);
}

// convertOp: intXX -> string
internal static ΔValue cvtIntString(ΔValue v, ΔType t) {
    @string s = "\uFFFD"u8;
    {
        var x = v.Int(); if ((int64)(rune)x == x) {
            s = ((@string)(rune)x);
        }
    }
    return makeString(v.flag.ro(), s, t);
}

// convertOp: uintXX -> string
internal static ΔValue cvtUintString(ΔValue v, ΔType t) {
    @string s = "\uFFFD"u8;
    {
        var x = v.Uint(); if ((uint64)(rune)x == x) {
            s = ((@string)(rune)x);
        }
    }
    return makeString(v.flag.ro(), s, t);
}

// convertOp: []byte -> string
internal static ΔValue cvtBytesString(ΔValue v, ΔType t) {
    return makeString(v.flag.ro(), ((@string)v.Bytes()), t);
}

// convertOp: string -> []byte
internal static ΔValue cvtStringBytes(ΔValue v, ΔType t) {
    return makeBytes(v.flag.ro(), slice<byte>(v.String()), t);
}

// convertOp: []rune -> string
internal static ΔValue cvtRunesString(ΔValue v, ΔType t) {
    return makeString(v.flag.ro(), ((@string)v.runes()), t);
}

// convertOp: string -> []rune
internal static ΔValue cvtStringRunes(ΔValue v, ΔType t) {
    return makeRunes(v.flag.ro(), slice<rune>(v.String()), t);
}

// convertOp: []T -> *[N]T
internal static ΔValue cvtSliceArrayPtr(ΔValue v, ΔType t) {
    nint n = t.Elem().Len();
    if (n > v.Len()) {
        throw panic("reflect: cannot convert slice with length " + itoa.Itoa(v.Len()) + " to pointer to array with length " + itoa.Itoa(n));
    }
    var h = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    return new ΔValue(t.common(), (~h).Data, (flag)((flag)(v.flag & ~((flag)((flag)(flagIndir | flagAddr) | flagKindMask))) | ((flag)(uintptr)(nuint)ΔPointer)));
}

// convertOp: []T -> [N]T
internal static ΔValue cvtSliceArray(ΔValue v, ΔType t) {
    nint n = t.Len();
    if (n > v.Len()) {
        throw panic("reflect: cannot convert slice with length " + itoa.Itoa(v.Len()) + " to array with length " + itoa.Itoa(n));
    }
    var h = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    var typ = t.common();
    @unsafe.Pointer ptr = h.Value.Data;
    @unsafe.Pointer c = (uintptr)unsafe_New(typ);
    typedmemmove(typ, c, ptr);
    ptr = c;
    return new ΔValue(typ, ptr.Value, (flag)((flag)(v.flag & ~((flag)(flagAddr | flagKindMask))) | ((flag)(uintptr)(nuint)Array)));
}

// convertOp: direct copy
internal static ΔValue cvtDirect(ΔValue v, ΔType typ) {
    var f = v.flag;
    var t = typ.common();
    @unsafe.Pointer ptr = v.ptr;
    if ((flag)(f & flagAddr) != 0) {
        // indirect, mutable word - make a copy
        @unsafe.Pointer c = (uintptr)unsafe_New(t);
        typedmemmove(t, c, ptr);
        ptr = c;
        f &= unchecked((flag)~(flag)(flagAddr));
    }
    return new ΔValue(t, ptr.Value, (flag)(v.flag.ro() | f)); // v.flag.ro()|f == f?
}

// convertOp: concrete -> interface
internal static ΔValue cvtT2I(ΔValue v, ΔType typ) {
    @unsafe.Pointer target = (uintptr)unsafe_New(typ.common());
    var x = valueInterface(v, false);
    if (typ.NumMethod() == 0){
        ((ж<any>)(uintptr)(target)).ValueSlot = x;
    } else {
        ifaceE2I(typ.common(), x, target);
    }
    return new ΔValue(typ.common(), target.Value, (flag)((flag)(v.flag.ro() | flagIndir) | ((flag)(uintptr)(nuint)ΔInterface)));
}

// convertOp: interface -> interface
internal static ΔValue cvtI2I(ΔValue v, ΔType typ) {
    if (v.IsNil()) {
        var ret = Zero(typ);
        ret.flag |= (flag)(v.flag.ro());
        return ret;
    }
    return cvtT2I(v.Elem(), typ);
}

// implemented in ../runtime
//
//go:noescape
internal static partial nint chancap(@unsafe.Pointer ch);

//go:noescape
internal static partial void chanclose(@unsafe.Pointer ch);

//go:noescape
internal static partial nint chanlen(@unsafe.Pointer ch);

// Note: some of the noescape annotations below are technically a lie,
// but safe in the context of this package. Functions like chansend0
// and mapassign0 don't escape the referent, but may escape anything
// the referent points to (they do shallow copies of the referent).
// We add a 0 to their names and wrap them in functions with the
// proper escape behavior.

//go:noescape
internal static partial (bool selected, bool received) chanrecv(@unsafe.Pointer ch, bool nb, @unsafe.Pointer val);

//go:noescape
internal static partial bool chansend0(@unsafe.Pointer ch, @unsafe.Pointer val, bool nb);

internal static bool chansend(@unsafe.Pointer ch, @unsafe.Pointer val, bool nb) {
    contentEscapes(val);
    return chansend0(ch, val, nb);
}

internal static partial @unsafe.Pointer /*ch*/ makechan(ж<abi.Type> typ, nint size);

internal static partial @unsafe.Pointer /*m*/ makemap(ж<abi.Type> t, nint cap);

//go:noescape
internal static partial @unsafe.Pointer /*val*/ mapaccess(ж<abi.Type> t, @unsafe.Pointer m, @unsafe.Pointer key);

//go:noescape
internal static partial @unsafe.Pointer /*val*/ mapaccess_faststr(ж<abi.Type> t, @unsafe.Pointer m, @string key);

//go:noescape
internal static partial void mapassign0(ж<abi.Type> t, @unsafe.Pointer m, @unsafe.Pointer key, @unsafe.Pointer val);

// mapassign should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/modern-go/reflect2
//   - github.com/goccy/go-json
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapassign
internal static void mapassign(ж<abi.Type> Ꮡt, @unsafe.Pointer m, @unsafe.Pointer key, @unsafe.Pointer val) {
    contentEscapes(key);
    contentEscapes(val);
    mapassign0(Ꮡt, m, key, val);
}

//go:noescape
internal static partial void mapassign_faststr0(ж<abi.Type> t, @unsafe.Pointer m, @string key, @unsafe.Pointer val);

internal static void mapassign_faststr(ж<abi.Type> Ꮡt, @unsafe.Pointer m, @string keyʗp, @unsafe.Pointer val) {
    ref var key = ref heap(keyʗp, out var Ꮡkey);

    contentEscapes((Ꮡkey.Reinterpret<@string, unsafeheader.String>()).Value.Data);
    contentEscapes(val);
    mapassign_faststr0(Ꮡt, m, key, val);
}

//go:noescape
internal static partial void mapdelete(ж<abi.Type> t, @unsafe.Pointer m, @unsafe.Pointer key);

//go:noescape
internal static partial void mapdelete_faststr(ж<abi.Type> t, @unsafe.Pointer m, @string key);

//go:noescape
internal static partial void mapiterinit(ж<abi.Type> t, @unsafe.Pointer m, ж<hiter> it);

//go:noescape
internal static partial @unsafe.Pointer /*key*/ mapiterkey(ж<hiter> it);

//go:noescape
internal static partial @unsafe.Pointer /*elem*/ mapiterelem(ж<hiter> it);

//go:noescape
internal static partial void mapiternext(ж<hiter> it);

//go:noescape
internal static partial nint maplen(@unsafe.Pointer m);

internal static partial void mapclear(ж<abi.Type> t, @unsafe.Pointer m);

// call calls fn with "stackArgsSize" bytes of stack arguments laid out
// at stackArgs and register arguments laid out in regArgs. frameSize is
// the total amount of stack space that will be reserved by call, so this
// should include enough space to spill register arguments to the stack in
// case of preemption.
//
// After fn returns, call copies stackArgsSize-stackRetOffset result bytes
// back into stackArgs+stackRetOffset before returning, for any return
// values passed on the stack. Register-based return values will be found
// in the same regArgs structure.
//
// regArgs must also be prepared with an appropriate ReturnIsPtr bitmap
// indicating which registers will contain pointer-valued return values. The
// purpose of this bitmap is to keep pointers visible to the GC between
// returning from reflectcall and actually using them.
//
// If copying result bytes back from the stack, the caller must pass the
// argument frame type as stackArgsType, so that call can execute appropriate
// write barriers during the copy.
//
// Arguments passed through to call do not escape. The type is used only in a
// very limited callee of call, the stackArgs are copied, and regArgs is only
// used in the call frame.
//
//go:noescape
//go:linkname call runtime.reflectcall
internal static partial void call(ж<abi.Type> stackArgsType, @unsafe.Pointer f, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void ifaceE2I(ж<abi.Type> t, any src, @unsafe.Pointer dst);

// memmove copies size bytes to dst from src. No write barriers are used.
//
//go:noescape
internal static partial void memmove(@unsafe.Pointer dst, @unsafe.Pointer src, uintptr size);

// typedmemmove copies a value of type t to dst from src.
//
//go:noescape
internal static partial void typedmemmove(ж<abi.Type> t, @unsafe.Pointer dst, @unsafe.Pointer src);

// typedmemclr zeros the value at ptr of type t.
//
//go:noescape
internal static partial void typedmemclr(ж<abi.Type> t, @unsafe.Pointer ptr);

// typedmemclrpartial is like typedmemclr but assumes that
// dst points off bytes into the value and only clears size bytes.
//
//go:noescape
internal static partial void typedmemclrpartial(ж<abi.Type> t, @unsafe.Pointer ptr, uintptr off, uintptr size);

// typedslicecopy copies a slice of elemType values from src to dst,
// returning the number of elements copied.
//
//go:noescape
internal static partial nint typedslicecopy(ж<abi.Type> t, unsafeheader.Slice dst, unsafeheader.Slice src);

// typedarrayclear zeroes the value at ptr of an array of elemType,
// only clears len elem.
//
//go:noescape
internal static partial void typedarrayclear(ж<abi.Type> elemType, @unsafe.Pointer ptr, nint len);

//go:noescape
internal static partial uintptr typehash(ж<abi.Type> t, @unsafe.Pointer p, uintptr h);

internal static partial bool verifyNotInHeapPtr(uintptr p);

//go:noescape
internal static partial unsafeheader.Slice growslice(ж<abi.Type> t, unsafeheader.Slice old, nint num);

//go:noescape
internal static partial void unsafeslice(ж<abi.Type> t, @unsafe.Pointer ptr, nint len);

// Dummy annotation marking that the value x escapes,
// for use in cases where the reflect code is so clever that
// the compiler cannot follow.
internal static void escapes(any x) {
    if (dummy.b) {
        dummy.x = x;
    }
}


[GoType("dyn")] partial struct dummyᴛ1 {
    internal bool b;
    internal any x;
}
internal static dummyᴛ1 dummy;

// Dummy annotation marking that the content of value x
// escapes (i.e. modeling roughly heap=*x),
// for use in cases where the reflect code is so clever that
// the compiler cannot follow.
internal static void contentEscapes(@unsafe.Pointer x) {
    if (dummy.b) {
        escapes(~(ж<any>)(uintptr)(x)); // the dereference may not always be safe, but never executed
    }
}

// This is just a wrapper around abi.NoEscape. The inlining heuristics are
// finnicky and for whatever reason treat the local call to noescape as much
// lower cost with respect to the inliner budget. (That is, replacing calls to
// noescape with abi.NoEscape will cause inlining tests to fail.)
//
//go:nosplit
internal static @unsafe.Pointer noescape(@unsafe.Pointer p) {
    return (uintptr)abi.NoEscape(p);
}

} // end reflect_package
