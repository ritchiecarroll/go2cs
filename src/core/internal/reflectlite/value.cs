// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using abi = go.@internal.abi_package;
using goarch = go.@internal.goarch_package;
using unsafeheader = go.@internal.unsafeheader_package;
using Δruntime = runtime_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;
using go.@internal;

partial class reflectlite_package {

// Value is the reflection interface to a Go value.
//
// Not all methods apply to all kinds of values. Restrictions,
// if any, are noted in the documentation for each method.
// Use the Kind method to find out the kind of value before
// calling kind-specific methods. Calling a method
// inappropriate to the kind of type causes a run time panic.
//
// The zero Value represents no value.
// Its IsValid method returns false, its Kind method returns Invalid,
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
[GoType] partial struct Value {
    // typ_ holds the type of the value represented by a Value.
    // Access using the typ method to avoid escape of v.
    internal ж<abi.Type> typ_;
    // Pointer-valued data or, if flagIndir is set, pointer to data.
    // Valid when either flagIndir is set or typ.pointers() is true.
    internal @unsafe.Pointer ptr;
    // flag holds metadata about the value.
    // The lowest bits are flag bits:
    //	- flagStickyRO: obtained via unexported not embedded field, so read-only
    //	- flagEmbedRO: obtained via unexported embedded field, so read-only
    //	- flagIndir: val holds a pointer to the data
    //	- flagAddr: v.CanAddr is true (implies flagIndir)
    // Value cannot represent method values.
    // The next five bits give the Kind of the value.
    // This repeats typ.Kind() except for method values.
    // The remaining 23+ bits give a method number for method values.
    // If flag.kind() != Func, code can assume that flagMethod is unset.
    // If ifaceIndir(typ), code can assume that flagIndir is set.
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

internal static abiꓸKind kind(this flag f) {
    return ((abiꓸKind)(uint8)((uintptr)((flag)(f & flagKindMask))));
}

internal static flag ro(this flag f) {
    if ((flag)(f & flagRO) != 0) {
        return flagStickyRO;
    }
    return 0;
}

internal static ж<abi.Type> typ(this Value v) {
    // Types are either static (for compiler-created types) or
    // heap-allocated but always reachable (for reflection-created
    // types, held in the central map). So there is no need to
    // escape types. noescape here help avoid unnecessary escape
    // of v.
    return v.typ_;
}

// pointer returns the underlying pointer represented by v.
// v.Kind() must be Pointer, Map, Chan, Func, or UnsafePointer
internal static @unsafe.Pointer pointer(this Value v) {
    if (v.typ().Size() != goarch.PtrSize || !v.typ().Pointers()) {
        throw panic("can't call pointer on a non-pointer Value");
    }
    if ((flag)(v.flag & flagIndir) != 0) {
        return ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
    }
    return v.ptr;
}

// packEface converts v to the empty interface.
internal static any packEface(Value v) {
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
// a Value that does not support it. Such cases are documented
// in the description of each method.
[GoType] partial struct ValueError {
    public @string Method;
    public abiꓸKind Kind;
}

[GoRecv] public static @string Error(this ref ValueError e) {
    if (e.Kind == 0) {
        return "reflect: call of "u8 + e.Method + " on zero Value"u8;
    }
    return "reflect: call of "u8 + e.Method + " on "u8 + e.Kind.String() + " Value"u8;
}

// go2cs generated this placeholder — func methodName is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// mustBeExported panics if f records that the value was obtained using
// an unexported field.
internal static partial void mustBeExported(this flag f) {
    if (f == 0) {
        throw panic(Ꮡ(new ValueError(methodName(), 0)));
    }
    if ((flag)(f & flagRO) != 0) {
        throw panic("reflect: " + methodName() + " using value obtained using unexported field");
    }
}

// mustBeAssignable panics if f records that the value is not assignable,
// which is to say that either it was obtained using an unexported field
// or it is not addressable.
internal static partial void mustBeAssignable(this flag f) {
    if (f == 0) {
        throw panic(Ꮡ(new ValueError(methodName(), abi.Invalid)));
    }
    // Assignable if addressable and not read-only.
    if ((flag)(f & flagRO) != 0) {
        throw panic("reflect: " + methodName() + " using value obtained using unexported field");
    }
    if ((flag)(f & flagAddr) == 0) {
        throw panic("reflect: " + methodName() + " using unaddressable value");
    }
}

// CanSet reports whether the value of v can be changed.
// A Value can be changed only if it is addressable and was not
// obtained by the use of unexported struct fields.
// If CanSet returns false, calling Set or any type-specific
// setter (e.g., SetBool, SetInt) will panic.
public static bool CanSet(this Value v) {
    return (flag)(v.flag & ((flag)(flagAddr | flagRO))) == flagAddr;
}

// go2cs generated this placeholder — func Elem is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func valueInterface is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func IsNil is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// IsValid reports whether v represents a value.
// It returns false if v is the zero Value.
// If IsValid returns false, all other methods except String panic.
// Most functions and methods never return an invalid Value.
// If one does, its documentation states the conditions explicitly.
public static bool IsValid(this Value v) {
    return v.flag != 0;
}

// Kind returns v's Kind.
// If v is the zero Value (IsValid returns false), Kind returns Invalid.
public static abiꓸKind Kind(this Value v) {
    return v.kind();
}

// implemented in runtime:

//go:noescape
internal static partial nint chanlen(@unsafe.Pointer _);

//go:noescape
internal static partial nint maplen(@unsafe.Pointer _);

// go2cs generated this placeholder — func Len is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// NumMethod returns the number of exported methods in the value's method set.
internal static nint numMethod(this Value v) {
    if (v.typ() == nil) {
        throw panic(Ꮡ(new ValueError("reflectlite.Value.NumMethod"u8, abi.Invalid)));
    }
    return v.typ().NumMethod();
}

// go2cs generated this placeholder — func Set is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Type returns v's type.
public static ΔType Type(this Value v) {
    var f = v.flag;
    if (f == 0) {
        throw panic(Ꮡ(new ValueError("reflectlite.Value.Type"u8, abi.Invalid)));
    }
    // Method values not supported.
    return toRType(v.typ());
}

/*
 * constructors
 */
// implemented in package runtime

//go:noescape
internal static partial @unsafe.Pointer unsafe_New(ж<abi.Type> _);

// go2cs generated this placeholder — func ValueOf is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// assignTo returns a value v that can be assigned directly to typ.
// It panics if v is not assignable to typ.
// For a conversion to an interface type, target is a suggested scratch space to use.
internal static Value assignTo(this Value v, @string context, ж<abi.Type> Ꮡdst, @unsafe.Pointer target) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    // if v.flag&flagMethod != 0 {
    // 	v = makeMethodValue(context, v)
    // }
    switch (ᐧ) {
    case {} when directlyAssignable(Ꮡdst, v.typ()): {
        var fl = (flag)((flag)(v.flag & ((flag)(flagAddr | flagIndir))) | v.flag.ro());
        fl |= (flag)(((flag)(uintptr)(uint8)dst.Kind()));
        return new Value( // Overwrite type so that they match.
 // Same memory layout, so no harm done.
Ꮡdst, v.ptr, fl);
    }
    case {} when implements(Ꮡdst, v.typ()): {
        if (target == nil) {
            target = (uintptr)unsafe_New(Ꮡdst);
        }
        if (v.Kind() == abi.Interface && v.IsNil()) {
            // A nil ReadWriter passed to nil Reader is OK,
            // but using ifaceE2I below will panic.
            // Avoid the panic by returning a nil dst (e.g., Reader) explicitly.
            return new Value(Ꮡdst, nil, ((flag)(uintptr)(uint8)abi.Interface));
        }
        var x = valueInterface(v);
        if (Ꮡdst.NumMethod() == 0){
            ((ж<any>)(uintptr)(target)).ValueSlot = x;
        } else {
            ifaceE2I(Ꮡdst, x, target);
        }
        return new Value(Ꮡdst, target.Value, (flag)(flagIndir | ((flag)(uintptr)(uint8)abi.Interface)));
    }}

    // Failed.
    throw panic(context + ": value of type " + toRType(v.typ()).String() + " is not assignable to type " + toRType(Ꮡdst).String());
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

internal static partial void ifaceE2I(ж<abi.Type> t, any src, @unsafe.Pointer dst);

// typedmemmove copies a value of type t to dst from src.
//
//go:noescape
internal static partial void typedmemmove(ж<abi.Type> t, @unsafe.Pointer dst, @unsafe.Pointer src);

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

} // end reflectlite_package
