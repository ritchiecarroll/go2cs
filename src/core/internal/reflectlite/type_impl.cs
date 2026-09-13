// type_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

namespace go.@internal;

partial class reflectlite_package
{
    // Implementation of resolveTypeOff
    internal static partial unsafe_package.Pointer resolveTypeOff(unsafe_package.Pointer rtype, int32 off)
    {
        return default!;
    }

    // Implementation of resolveNameOff
    internal static partial unsafe_package.Pointer resolveNameOff(unsafe_package.Pointer ptrInModule, int32 off)
    {
        return default!;
    }

    // ==== Phase-3 write-back: the errors.As TYPE surface (rtype.Elem/Implements/AssignableTo) ====
    // The auto forms read descriptor sub-records (ptrType.Elem, interface method tables) that the
    // Phase-1 synthetic abi.Type never populates — Elem panicked "Elem of invalid type" and
    // implements() reinterpreted the descriptor as an eface. Bridged over the abi.Type's carried
    // System.Type and the SAME golib method-set machinery emitted asserts use (GoReflect), so
    // reflection and direct asserts can never disagree. See docs/phase4/DESIGN-reflection-bridge.md.

    // Elem returns the element type of a pointer/slice/array/map/chan type.
    internal static ΔType Elem(this rtype t)
    {
        ж<abi_package.Type> self = t.Type;
        System.Type? st = self == nil ? null : self.Value.sysType;
        // A POINTER hands its pointee's channel direction down unshifted, the hop `new(chan<- T)`
        // takes to reach Elem().String(); nothing else carries the cargo through.
        GoChanDir[]? elemChanDirChain = st is not null && GoReflect.KindOf(st) == GoReflect.Pointer ? self.Value.chanDirChain : null;
        return toType(abi_package.synthType(GoReflect.ElementType(st), null, null, elemChanDirChain, null));
    }

    // Implements reports whether the type implements the interface type u (Go method-set rules:
    // nominal or structural via golib StructurallyImplements).
    internal static bool Implements(this rtype t, ΔType u)
    {
        if (u == default!)
            throw panic("reflect: nil type passed to Type.Implements");

        if (u.Kind() != Interface)
            throw panic("reflect: non-interface type passed to Type.Implements");

        return GoReflect.GoImplements(sysTypeOfLiteType(u), t.Type == nil ? null : t.Type.Value.sysType);
    }

    // AssignableTo is NO LONGER hand-owned here — the identity-on-the-managed-type restatement
    // was strictly narrower than Go's rule (it rejected `*int` ↔ `type IntPtr *int`, both
    // directions of reflectlite's own TestAssignableTo). Go's literal body now runs —
    // `directlyAssignable(uu.Type, t.Type) || implements(uu.Type, t.Type)` — over the two
    // bridged primitives below, mirroring reflect's retirement of the same hand-own one layer
    // down (see reflect/value_impl.cs, "type IDENTITY").

    // implements is the FREE function Go's own directlyAssignable/AssignableTo/assignTo route
    // through (rtype.Implements above is the public API boundary — it panics for a
    // non-interface argument where this one answers false, exactly as Go's does). The auto form
    // reinterprets the abi.Type as an interfaceType and reads .Methods off a promoted-embed box
    // that is DEFAULT behind a synthesized descriptor. Bridged over GoReflect.GoImplements —
    // the same method-set probe the emitted `_<T>` asserts, rtype.Implements, and reflect's own
    // free implements use — so no two routes can disagree about a method set.
    internal static bool implements(ж<abi_package.Type> ᏑT, ж<abi_package.Type> ᏑV)
    {
        if (ᏑT == nil || ᏑT.Value.Kind() != abi_package.Interface)
            return false;

        return GoReflect.GoImplements(ᏑT.Value.sysType, ᏑV == nil ? null : ᏑV.Value.sysType);
    }

    // haveIdenticalUnderlyingType is THE seat of Go's type-identity relation, mirrored from
    // reflect's bridged walk (reflect/value_impl.cs) arm for arm: the scalar arm needs nothing
    // and Array/Chan/Map/Pointer/Slice recurse through the accessors internal/abi synthesizes,
    // while the STRUCT, FUNC and INTERFACE arms of the literal form reached their operands by
    // the prefix-downcast idiom and answered TRUE off zero-read records — a false positive in
    // an identity relation, which every caller reads as permission. The struct walk reads the
    // SAME GoFields projection the field surface hands out; the func walk reads the SAME
    // TryFuncShape rtype's signature surface reads; the interface arm proves methodless only
    // for `object` (Go's any) — the conservative direction.
    internal static bool haveIdenticalUnderlyingType(ж<abi_package.Type> ᏑT, ж<abi_package.Type> ᏑV, bool cmpTags)
    {
        if (ᏑT == ᏑV)
            return true;

        if (ᏑT == nil || ᏑV == nil)
            return false;

        abiꓸKind kind = ᏑT.Value.Kind();

        if (kind != ᏑV.Value.Kind())
            return false;

        // Non-composite types of equal kind have the same underlying type (the predefined
        // instance).
        if (abi_package.Bool <= kind && kind <= abi_package.Complex128 || kind == abi_package.ΔString || kind == abi_package.UnsafePointer)
            return true;

        // Composite types — Go's switch, in Go's order.
        if (kind == abi_package.Array)
            return ᏑT.Len() == ᏑV.Len() && haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), cmpTags);

        // Go's chan arm is TWO rules, and the first one had been dropped here: a BIDIRECTIONAL x is
        // assignable to any channel type with the same element, which is what makes
        // `var r <-chan int = make(chan int)` legal. It was invisible while every ChanDir() answered
        // BothDir — both rules then agreed for every pair — and became load-bearing the moment the
        // direction became real cargo (2026-08-20).
        if (kind == abi_package.Chan) {
            if (ᏑV.ChanDir() == abi_package.BothDir && haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), cmpTags))
                return true;

            return ᏑV.ChanDir() == ᏑT.ChanDir() && haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), cmpTags);
        }

        if (kind == abi_package.Func)
            return haveIdenticalFuncShape(ᏑT, ᏑV, cmpTags);

        if (kind == abi_package.Interface)
            return ᏑT.Value.sysType == typeof(object) && ᏑV.Value.sysType == typeof(object);

        if (kind == abi_package.Map)
            return haveIdenticalType(ᏑT.Key(), ᏑV.Key(), cmpTags) && haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), cmpTags);

        if (kind == abi_package.Pointer || kind == abi_package.Slice)
            return haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), cmpTags);

        if (kind == abi_package.Struct)
            return haveIdenticalStructShape(ᏑT, ᏑV, cmpTags);

        return false;
    }

    // haveIdenticalFuncShape mirrors reflect's: the delegate Invoke signature both sides'
    // NumIn/In/NumOut/Out read, plus variadicity, with parameter ARRAY DIMS riding the
    // descriptor's funcParamDims cargo.
    private static bool haveIdenticalFuncShape(ж<abi_package.Type> ᏑT, ж<abi_package.Type> ᏑV, bool cmpTags)
    {
        System.Type? ts = ᏑT.Value.sysType;
        System.Type? vs = ᏑV.Value.sysType;

        if (ts is null || vs is null ||
            !GoReflect.TryFuncShape(ts, out System.Type[]? tin, out System.Type[]? tout, out bool tVariadic) ||
            !GoReflect.TryFuncShape(vs, out System.Type[]? vin, out System.Type[]? vout, out bool vVariadic))
        {
            return false;
        }

        if (tin!.Length != vin!.Length || tout!.Length != vout!.Length || tVariadic != vVariadic)
            return false;

        nint[]?[]? tParamDims = ᏑT.Value.funcParamDims;
        nint[]?[]? vParamDims = ᏑV.Value.funcParamDims;

        for (int i = 0; i < tin.Length; i++)
        {
            ж<abi_package.Type> tp = abi_package.synthType(tin[i], funcParamDimsAt(tParamDims, i));
            ж<abi_package.Type> vp = abi_package.synthType(vin[i], funcParamDimsAt(vParamDims, i));

            if (!haveIdenticalType(tp, vp, cmpTags))
                return false;
        }

        for (int i = 0; i < tout.Length; i++)
        {
            if (!haveIdenticalType(abi_package.synthType(tout[i]), abi_package.synthType(vout[i]), cmpTags))
                return false;
        }

        return true;
    }

    private static nint[]? funcParamDimsAt(nint[]?[]? paramDims, int i)
    {
        return paramDims is not null && i < paramDims.Length ? paramDims[i] : null;
    }

    // haveIdenticalStructShape mirrors reflect's field loop over GoReflect.GoFields: field
    // COUNT, the struct's PkgPath (set when it hides an unexported field), and per field the
    // NAME, EMBEDDEDNESS, TAG (only when cmpTags), TYPE and OFFSET (only when both sides can
    // compute a layout).
    private static bool haveIdenticalStructShape(ж<abi_package.Type> ᏑT, ж<abi_package.Type> ᏑV, bool cmpTags)
    {
        System.Type? ts = ᏑT.Value.sysType;
        System.Type? vs = ᏑV.Value.sysType;

        if (ts is null || vs is null)
            return false;

        GoReflect.GoFieldInfo[] tFields = GoReflect.GoFields(ts);
        GoReflect.GoFieldInfo[] vFields = GoReflect.GoFields(vs);

        if (tFields.Length != vFields.Length)
            return false;

        if (structTypePkgPath(ts, tFields) != structTypePkgPath(vs, vFields))
            return false;

        nint[]? tOffsets = GoReflect.GoFieldOffsets(ts);
        nint[]? vOffsets = GoReflect.GoFieldOffsets(vs);
        bool compareOffsets = tOffsets is not null && vOffsets is not null;

        for (int i = 0; i < tFields.Length; i++)
        {
            GoReflect.GoFieldInfo tf = tFields[i];
            GoReflect.GoFieldInfo vf = vFields[i];

            if (tf.Name != vf.Name || tf.Embedded != vf.Embedded)
                return false;

            if (cmpTags && tf.Tag != vf.Tag)
                return false;

            if (!haveIdenticalType(structFieldDescriptor(tf), structFieldDescriptor(vf), cmpTags))
                return false;

            if (compareOffsets && tOffsets![i] != vOffsets![i])
                return false;
        }

        return true;
    }

    private static @string structTypePkgPath(System.Type st, GoReflect.GoFieldInfo[] fields)
    {
        foreach (GoReflect.GoFieldInfo f in fields)
        {
            if (!f.Exported)
                return (@string)GoReflect.GoPackagePath(st);
        }

        return "";
    }

    private static ж<abi_package.Type> structFieldDescriptor(GoReflect.GoFieldInfo f)
    {
        nint[]? dims = GoReflect.KindOf(f.Type) == GoReflect.Array ? f.ArrayDims : null;
        GoChanDir fieldDir = GoReflect.KindOf(f.Type) == GoReflect.Chan ? f.ChanDir : GoChanDir.Unstamped;
        return abi_package.synthType(f.Type, dims, null, fieldDir);
    }

    // PkgPath answers a DEFINED type's import path over the same GoReflect machinery reflect's
    // side uses (GoPackagePath reads the managed nesting the converter emits). The literal form
    // read the uncommonType's PkgPath name offset out of the linker name blob and answered ""
    // for every type. An UNNAMED type (a lift, a raw container, `any`) answers Go's "" through
    // the HasGoName gate; a predeclared type answers "" because nothing declares it under the
    // `go.` emission root.
    internal static @string PkgPath(this rtype t)
    {
        System.Type? st = t.Type == nil ? null : t.Type.Value.sysType;

        if (st is null || !GoReflect.HasGoName(st))
            return ""u8;

        return (@string)GoReflect.GoPackagePath(st);
    }

    // ==== The type NAME (rtype.String) ====
    // The auto form reads a name OFFSET into the linker-built name blob — `t.nameOff(t.Str).Name()`
    // — which a synthesized descriptor never populates, so every reflectlite.TypeOf(x).String()
    // answered "". Silently: "" is a legal name for an unnamed type, so nothing panicked and the
    // empty string simply propagated into whatever the caller was building (context's `stringify`
    // fallback printed `WithValue(, c1k1)` where Go prints `WithValue(context_test.key1, c1k1)`).
    // Answered from the descriptor's carried System.Type through the SAME golib naming that backs
    // reflect's hand-owned rtype.String and %T, so the full bridge and the mini-bridge cannot
    // disagree about what a type is called. Array dims ride along as they do on the reflect side —
    // a descriptor that knows its length renders Go's [N]T rather than []T.

    // String returns the Go source type string (`context_test.key1`, `[]int`, `*T`).
    internal static @string String(this rtype t)
    {
        if (t.Type == nil)
            return (@string)GoReflect.GoTypeName(null);

        return (@string)GoReflect.GoTypeName(t.Type.Value.sysType, t.Type.Value.arrayDims, t.Type.Value.chanDirChain, null);
    }

    // sysTypeOfLiteType recovers the managed System.Type a reflectlite Type wrapper describes
    // (the rtype's abi.Type carries it — synthType stamped it).
    private static System.Type? sysTypeOfLiteType(ΔType u)
    {
        var (rt, ok) = u._<rtype>(ᐧ);
        return ok && rt.Type != nil ? rt.Type.Value.sysType : null;
    }
}
