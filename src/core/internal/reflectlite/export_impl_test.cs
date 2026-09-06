// export_impl_test.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The hand-owned companion of export_test.go's conversion — the FIRST `*_impl_test.cs`
// companion (the `_test.cs` suffix keeps it under the production csproj's existing
// test-artifact exclusion; testConversion globs `*_impl_test.cs` into the tests project's
// compile items and the conversion digest).
//
// export_test.go hands the suite a reflection surface built from raw Value{typ, ptr, flag}
// triples over descriptor downcasts — `(*structType)(unsafe.Pointer(v.typ()))`, `add(v.ptr,
// field.Offset)` — neither of which the managed bridge populates: v.ptr is never a real
// address and nothing sits behind a ж<abi.Type> to downcast to, so Field panicked "index out
// of range" off a zero-read record for every struct. Mirrors reflect's hand-owned
// Value.Field / Zero (reflect/value_impl.cs) one layer down, over the SAME golib projections
// (GoFields / FieldAliasBox / ZeroValueOf), so the mini-bridge and the full bridge cannot
// disagree about a field walk. StructFieldType stays the literal conversion: it walks
// whatever StructType record it is handed, and the hand-owned TField hands it the
// SYNTHESIZED one (abi's Type.StructType()).

namespace go.@internal;

using static global::go.@internal.reflectlite_package;

partial class reflectlite_internal_test_package
{
    // Field returns the i'th field of the struct v — mirrors reflect's Value.Field: typed by
    // the field's STATIC Go type (an interface-typed field reports Kind Interface; a
    // nil-valued field is a VALID nil Value), ADDRESSABLE when v is (aliasing the parent box
    // through GoReflect.FieldAliasBox), and read-only for unexported fields with the parent's
    // sticky bit inherited (only flagStickyRO propagates; an unexported EMBED takes
    // flagEmbedRO — Go's two read-only bits are not interchangeable).
    public static global::go.@internal.reflectlite_package.Value Field(global::go.@internal.reflectlite_package.Value v, nint i)
    {
        System.Type? st = v.typ_ == nil ? null : v.typ_.Value.sysType;

        if (st is null || GoReflect.KindOf(st) != GoReflect.Struct)
            throw panic("reflect: Field of non-struct type");

        GoReflect.GoFieldInfo[] fields = GoReflect.GoFields(st);

        if ((nuint)i >= (nuint)fields.Length)
            throw panic("reflect: Field index out of range");

        GoReflect.GoFieldInfo f = fields[(int)i];
        global::go.@internal.reflectlite_package.flag ro =
            (global::go.@internal.reflectlite_package.flag)((global::go.@internal.reflectlite_package.flag)(v.flag & flagStickyRO) |
            (f.Exported ? default : f.Embedded ? flagEmbedRO : flagStickyRO));

        if (v.addrBox is not null)
        {
            global::go.@internal.reflectlite_package.Value elem = makeTypedValue(null, f.Type, f.ArrayDims, ro, f.ChanDir);

            elem.flag |= (global::go.@internal.reflectlite_package.flag)(flagAddr | flagIndir);
            elem.addrBox = GoReflect.FieldAliasBox(v.addrBox, f);

            return elem;
        }

        object? cur = v.live;

        if (cur is null)
            throw panic(Ꮡ(new ValueError("reflect.Value.Field"u8, v.kind())));

        return makeTypedValue(f.Read(cur), f.Type, f.ArrayDims, ro, f.ChanDir);
    }

    // TField returns the i'th field's TYPE — the type-side twin of Field. The literal form
    // reached the field table by the prefix-downcast idiom; this hands the literal
    // StructFieldType the SYNTHESIZED specialization instead, so the type-side walk and the
    // value-side walk read one projection.
    public static global::go.@internal.reflectlite_package.ΔType TField(global::go.@internal.reflectlite_package.ΔType typ, nint i)
    {
        var t = typ._<rtype>();

        if (t.Type.Value.Kind() != Struct)
            throw panic("reflect: Field of non-struct type");

        return StructFieldType(t.Type.StructType(), i);
    }

    // Zero returns a Value representing the zero value for the specified type — mirrors
    // reflect's Zero over the shared golib rule (GoReflect.ZeroValueOf): pointer kinds the
    // canonical typed-nil box, slice/map/chan kinds their nil container default, array kinds a
    // dims-sized backing. The literal form built the Value over unsafe_New's null and answered
    // the INVALID Value for every type.
    public static global::go.@internal.reflectlite_package.Value Zero(global::go.@internal.reflectlite_package.ΔType typ)
    {
        if (typ == default!)
            throw panic("reflect: Zero(nil)");

        ж<abi_package.Type> t = typ.common();
        System.Type? st = t == nil ? null : t.Value.sysType;

        if (st is null)
            throw panic("reflect: Zero of non-synthesized type");

        nint[]? dims = t.Value.arrayDims;
        // The fabricated zero carries the descriptor's CHANNEL DIRECTION as well as its array
        // dims -- this is the mini-bridge's half of the same rule, and the one TypeString reads
        // (`%T` of ToInterface(Zero(typ))).
        // The descriptor carries the direction as a PER-LEVEL CHAIN since increment 2b (bc2dbb7af); this
        // mini-bridge's zero takes the chain's HEAD, the outer channel's own direction, exactly as reflect's
        // chanDirOfReflectType reads it. (This hand-owned test companion was the one reader of the retired
        // scalar `chanDir`, compiled by no gate at master until E2b's banked-row sweep -- CS1061 since train 27.)
        GoChanDir chanDir = t.Value.chanDirChain is { Length: > 0 } chain ? chain[0] : GoChanDir.Unstamped;

        return makeTypedValue(GoReflect.ZeroValueOf(st, dims, chanDir), st, dims, default, chanDir);
    }
}
