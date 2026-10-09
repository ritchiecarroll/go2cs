// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using Δsync = sync_package;
using @unsafe = unsafe_package;
using @internal;
using static global::go.reflect_package;

partial class reflect_internal_test_package {

// MakeRO returns a copy of v with the read-only flag set.
public static global::go.reflect_package.ΔValue MakeRO(global::go.reflect_package.ΔValue v) {
    v.flag |= (global::go.reflect_package.flag)(flagStickyRO);
    return v;
}

// IsRO reports whether v's read-only flag is set.
public static bool IsRO(global::go.reflect_package.ΔValue v) {
    return (global::go.reflect_package.flag)(v.flag & flagStickyRO) != 0;
}

public static ж<bool> CallGC;
internal static void initᴛCallGC() { CallGC = ᏑcallGC; }

// FuncLayout calls funcLayout and returns a subset of the results for testing.
//
// Bitmaps like stack, gc, inReg, and outReg are expanded such that each bit
// takes up one byte, so that writing out test cases is a little clearer.
// If ptrs is false, gc will be nil.
public static (global::go.reflect_package.ΔType frametype, uintptr argSize, uintptr retOffset, slice<byte> stack, slice<byte> gc, slice<byte> inReg, slice<byte> outReg, bool ptrs) FuncLayout(global::go.reflect_package.ΔType t, global::go.reflect_package.ΔType rcvr) {
    global::go.reflect_package.ΔType frametype = default!;
    uintptr argSize = default!;
    uintptr retOffset = default!;
    slice<byte> stack = default!;
    slice<byte> gc = default!;
    slice<byte> inReg = default!;
    slice<byte> outReg = default!;
    bool ptrs = default!;

    ж<abi.Type> ft = default!;
    global::go.reflect_package.abiDesc abid = new();
    if (rcvr != default!){
        (ft, _, abid) = funcLayout(t.common().FuncType(), rcvr.common());
    } else {
        (ft, _, abid) = funcLayout(t._<ж<global::go.reflect_package.rtype>>().Reinterpret<global::go.reflect_package.rtype, abi.Type>().FuncType(), nil);
    }
    // Extract size information.
    argSize = abid.stackCallArgsSize;
    retOffset = abid.retOffset;
    frametype = toType(ft);
    // Expand stack pointer bitmap into byte-map.
    for (var i = (uint32)0; i < (~abid.stackPtrs).n; i++) {
        stack = builtin.append(stack, (byte)(((~abid.stackPtrs).data[i / 8] >> (int)((i % 8))) & 1));
    }
    // Expand register pointer bitmaps into byte-maps.
    byte bool2byte(bool b) {
        if (b) {
            return 1;
        }
        return 0;
    }
    for (nint i = 0; i < intArgRegs; i++) {
        inReg = builtin.append(inReg, bool2byte(abid.inRegPtrs.Get(i)));
        outReg = builtin.append(outReg, bool2byte(abid.outRegPtrs.Get(i)));
    }
    // Expand frame type's GC bitmap into byte-map.
    ptrs = ft.Pointers();
    if (ptrs) {
        var nptrs = (~ft).PtrBytes / (uintptr)goarch.PtrSize;
        var gcdata = ft.GcSlice(0, (nptrs + 7) / 8);
        for (var i = (uintptr)0; i < nptrs; i++) {
            gc = builtin.append(gc, (byte)((gcdata[i / 8] >> (int)((i % 8))) & 1));
        }
    }
    return (frametype, argSize, retOffset, stack, gc, inReg, outReg, ptrs);
}

public static slice<@string> TypeLinks() {
    slice<@string> r = default!;
    var (sections, offset) = typelinks();
    foreach (var (i, offs) in offset) {
        @unsafe.Pointer rodata = sections[i];
        foreach (var (_, off) in offs) {
            var typ = (ж<global::go.reflect_package.rtype>)(uintptr)(resolveTypeOff(rodata, off));
            r = builtin.append(r, typ.String());
        }
    }
    return r;
}

public static Func<any, slice<byte>> GCBits = gcbits;

// go2cs generated this placeholder — func gcbits is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// provided by runtime
public partial struct EmbedWithUnexpMeth {
}

internal static void f(this EmbedWithUnexpMeth _) {
}

internal partial interface pinUnexpMeth {
    void f();
}

internal static pinUnexpMeth pinUnexpMethI = ((pinUnexpMeth)new EmbedWithUnexpMeth(nil));

public static ж<byte> FirstMethodNameBytes(global::go.reflect_package.ΔType t) {
    _ = pinUnexpMethI;
    var ut = t.uncommon();
    if (ut == nil) {
        throw panic("type has no methods");
    }
    var m = ut.Methods()[0];
    var mname = t._<ж<global::go.reflect_package.rtype>>().nameOff(m.Name);
    if ((byte)(mname.DataChecked(0, nameFlagFieldˢ).Value & ((byte)(1 << (int)(2)))) == 0) {
        throw panic("method name does not have pkgPath *string");
    }
    return mname.Bytes;
}

public partial struct OtherPkgFields {
    public nint OtherExported;
    internal nint otherUnexported;
}

// go2cs generated this placeholder — func IsExported is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static void ResolveReflectName(@string s) {
    resolveReflectName(newName(s, ""u8, false, false));
}

public partial struct Buffer {
    internal slice<byte> buf;
}

internal static void clearLayoutCache() {
    layoutCache = new Δsync.Map(nil);
}

public static (nint oldInts, nint oldFloats, uintptr oldFloatSize) SetArgRegs(nint ints, nint floats, uintptr floatSize) {
    nint oldInts = default!;
    nint oldFloats = default!;
    uintptr oldFloatSize = default!;

    oldInts = intArgRegs;
    oldFloats = floatArgRegs;
    oldFloatSize = floatRegSize;
    intArgRegs = ints;
    floatArgRegs = floats;
    floatRegSize = floatSize;
    clearLayoutCache();
    return (oldInts, oldFloats, oldFloatSize);
}

public static Func<uintptr> MethodValueCallCodePtr = methodValueCallCodePtr;

public static Func<slice<byte>, bool> InternalIsZero = isZero;

public static Func<global::go.reflect_package.ΔType, bool> IsRegularMemory = isRegularMemory;

} // end reflect_internal_test_package
