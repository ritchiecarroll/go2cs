// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// regAssign is hand-owned for ONE arm. Everything else is Go's body, unchanged.
//
// The Struct arm iterates the descriptor's field list and returns TRUE when that list is empty --
// "every field was assigned to a register", with zero steps recorded. For a SYNTHESIZED struct
// descriptor, which carries no Fields, that is a vacuous success: the caller believes the argument
// went to registers, `abiSeq.stackBytes` stays 0, and funcLayout answers size/argsize/retOffset 0
// with empty pointer bitmaps. Measured as reflect's own TestFuncLayout, where `reflect_test.S`
// arrives with Size=32 and Fields.Length=0 and five assertions fail from that one silent win.
//
// "Empty means cannot see" is FALSE as a general rule, and the throw is written narrowly because of
// it: `struct{}` is legal and ubiquitous in Go (map[string]struct{}, signalling channels, the set
// idiom) and legitimately has no fields. The predicate is therefore `Fields.Length == 0 &&
// Size() > 0` -- a struct occupying 32 bytes with no fields is definitionally unseeable, while
// `struct{}` (0 fields, 0 size) passes through untouched and still returns true, as Go does.
//
// The ARRAY arm deliberately does NOT get the same treatment; the measurement is at the arm.
//
// Reachability, so nobody reads this as a production fix: funcLayout's only live caller is
// export_test's FuncLayout wrapper. The auto MakeFunc that calls it is displaced by the registry,
// and every other route needs flagMethod, which nothing in the package assigns. So this throw is
// insurance against a future silent pass, not a repair of a live path.
//
// See docs/phase4/DESIGN-descriptor-cargo.md.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class reflect_package {

[GoRecv] internal static bool regAssign(this ref abiSeq a, ж<abi.Type> Ꮡt, uintptr offset) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = ((ΔKind)(nuint)(uint8)t.Kind());
    if (exprᴛ1 == ΔUnsafePointer || exprᴛ1 == ΔPointer || exprᴛ1 == Chan || exprᴛ1 == Map || exprᴛ1 == Func) {
        return a.assignIntN(offset, t.Size(), 1, 0b1);
    }
    if (exprᴛ1 == ΔBool || exprᴛ1 == ΔInt || exprᴛ1 == ΔUint || exprᴛ1 == Int8 || exprᴛ1 == Uint8 || exprᴛ1 == Int16 || exprᴛ1 == Uint16 || exprᴛ1 == Int32 || exprᴛ1 == Uint32 || exprᴛ1 == Uintptr) {
        return a.assignIntN(offset, t.Size(), 1, 0b0);
    }
    if (exprᴛ1 == Int64 || exprᴛ1 == Uint64) {
        var exprᴛ2 = goarch.PtrSize;
        if (exprᴛ2 == 4) {
            return a.assignIntN(offset, 4, 2, 0b0);
        }
        if (exprᴛ2 == 8) {
            return a.assignIntN(offset, 8, 1, 0b0);
        }

    }
    else if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        return a.assignFloatN(offset, t.Size(), 1);
    }
    else if (exprᴛ1 == Complex64) {
        return a.assignFloatN(offset, 4, 2);
    }
    else if (exprᴛ1 == Complex128) {
        return a.assignFloatN(offset, 8, 2);
    }
    else if (exprᴛ1 == ΔString) {
        return a.assignIntN(offset, goarch.PtrSize, 2, 0b01);
    }
    else if (exprᴛ1 == ΔInterface) {
        return a.assignIntN(offset, goarch.PtrSize, 2, 0b10);
    }
    else if (exprᴛ1 == ΔSlice) {
        return a.assignIntN(offset, goarch.PtrSize, 3, 0b001);
    }
    else if (exprᴛ1 == Array) {
        // Same vacuous shape as the Struct arm below, and it IS separable -- but not by the two
        // accessors the arm holds. `Len` and `Size` are both 0 for a legal `[0]T` AND for an array
        // whose length was never known, which is where a first reading stops and concludes there is
        // no discriminator. The datum is on a THIRD field: the descriptor's own `arrayDims`, whose
        // declaration states the rule outright -- "Null = unknown ([0]T is [0])".
        //
        //     ArrayOf(0, uint8)                    Len 0 / Size 0 / arrayDims [0]     <- legal
        //     TypeOf([][6]uint8{}).Elem()          Len 0 / Size 0 / arrayDims null    <- unknown
        //     (an EMPTY slice's element: increment B seeds a present element and states the empty
        //     container as its boundary; before B, SliceOf(ArrayOf(6, uint8)).Elem() was this shape too)
        //
        // Measured distinct as reflect.Types (`[0]uint8` vs `[]uint8`, not equal), so the model
        // carries the difference and only this arm was blind to it.
        // Through the ACCESSOR that carries the cargo (abi.ArrayType synthesizes Len from arrayDims), not a
        // reinterpret of a synthesized abi.Type, whose arrayType view has no inline record and reads Len 0.
        var tt = Ꮡt.ArrayType();
        if (tt == nil || ((~tt).Len == 0 && Ꮡt.Value.arrayDims is null)) {
            throw panic("reflect: regAssign cannot read the length of array descriptor " +
                stringFor(Ꮡt) + " (Len=0 with no arrayDims cargo, i.e. UNKNOWN rather than [0]T); " +
                "any register assignment for it would be vacuous. See " +
                "docs/phase4/DESIGN-descriptor-cargo.md.");
        }
        var exprᴛ3 = (~tt).Len;
        if (exprᴛ3 == 0) {
            return true;
        }
        if (exprᴛ3 == 1) {
            return a.regAssign((~tt).Elem, // There's nothing to assign, so don't modify
 // a.steps but succeed so the caller doesn't
 // try to stack-assign this value.
 offset);
        }
        { /* default: */
            return false;
        }

    }
    else if (exprᴛ1 == Struct) {
        // Through the ACCESSOR that carries the cargo (abi.StructType synthesizes Fields from
        // GoReflect.GoFields with offsets and per-field dims), not a reinterpret of a synthesized abi.Type,
        // whose structType view has no inline field blob and reads Fields.Length 0 for EVERY synthesized
        // struct -- reflect_test.S's 32-byte/0-field reading. A declared struct answers correctly here;
        // only a descriptor the accessor cannot synthesize (nil), or one genuinely fieldless-but-sized,
        // is unseeable and throws.
        var st = Ꮡt.StructType();
        // The one behavioural difference from Go's body. A struct with SIZE but no FIELDS is a
        // descriptor this layer cannot read, not a struct with nothing in it; reporting success for
        // it is the silent pass this throw exists to convert into a diagnosis. `struct{}` reaches
        // here as 0 fields / 0 size and falls straight through to Go's loop, which returns true.
        if (st == nil || ((~st).Fields.Length == 0 && t.Size() > 0)) {
            throw panic("reflect: regAssign cannot read the fields of struct descriptor " +
                stringFor(Ꮡt) + " (Size=" + t.Size().ToString() + ", Fields=0); the descriptor " +
                "carries no field list, so any register assignment for it would be vacuous. See " +
                "docs/phase4/DESIGN-descriptor-cargo.md.");
        }
        foreach (var (i, _) in (~st).Fields) {
            var f = Ꮡ((~st).Fields, i);
            if (!a.regAssign((~f).Typ, offset + (~f).Offset)) {
                return false;
            }
        }
        return true;
    }
    else { /* default: */
        print((@string)"t.Kind == "u8, t.Kind(), (@string)"\n"u8);
        throw panic("unknown type kind");
    }

    throw panic("unhandled register assignment path");
}

// ==== addTypeBits — the same defect as regAssign, one call over ================================
//
// Both of funcLayout's pointer bitmaps come through here: newAbiDesc fills abid.stackPtrs with
// addTypeBits(stackPtrs, stkStep.stkOff, arg) for every stack-assigned argument and result, and the
// frame type's GCData is that same bitvector (funcLayout aliases x.GCData = &abid.stackPtrs.data[0]).
// The auto body's Array and Struct arms raw-reinterpret the descriptor — Reinterpret<abi.Type, arrayType>
// / structType — and on a synthesized descriptor the reinterpreted record reads Len 0 and no Fields,
// so no bit is ever appended. Measured after R1 fixed regAssign's sizes: TestFuncLayout/func(reflect_test.S)
// still reported stack=[] and gc=[] where Go wants [0 0 1 1]. Same remedy as regAssign: read the
// descriptor through the abi accessors, which synthesize the record from the managed type, and fail
// LOUD when a descriptor cannot be read rather than hand back an empty bitmap that looks like "no
// pointers". The scalar arms are the auto emission verbatim.
internal static void addTypeBits(ж<bitVector> Ꮡbv, uintptr offset, ж<abi.Type> Ꮡt) {
    ref var bv = ref Ꮡbv.DerefOrNull();
    ref var t = ref Ꮡt.DerefOrNull();

    if (!t.Pointers()) {
        return;
    }
    var exprᴛ1 = ((ΔKind)(nuint)((uint8)((abiꓸKind)(t.Kind_ & abi.KindMask))));
    if (exprᴛ1 == Chan || exprᴛ1 == Func || exprᴛ1 == Map || exprᴛ1 == ΔPointer || exprᴛ1 == ΔSlice || exprᴛ1 == ΔString || exprᴛ1 == ΔUnsafePointer) {
        while (bv.n < (uint32)(offset / (uintptr)goarch.PtrSize)) {
            // 1 pointer at start of representation
            bv.append(0);
        }
        bv.append(1);
    }
    else if (exprᴛ1 == ΔInterface) {
        while (bv.n < (uint32)(offset / (uintptr)goarch.PtrSize)) {
            // 2 pointers
            bv.append(0);
        }
        bv.append(1);
        bv.append(1);
    }
    else if (exprᴛ1 == Array) {
        var tt = Ꮡt.ArrayType();
        if (tt == nil) {
            throw panic("reflect: addTypeBits cannot read the array descriptor " + stringFor(Ꮡt));
        }
        for (nint i = 0; i < (nint)(~tt).Len; i++) {
            // repeat inner type
            addTypeBits(Ꮡbv, offset + (uintptr)i * (~(~tt).Elem).Size_, (~tt).Elem);
        }
    }
    else if (exprᴛ1 == Struct) {
        var st = Ꮡt.StructType();
        if (st == nil) {
            throw panic("reflect: addTypeBits cannot read the fields of struct descriptor " + stringFor(Ꮡt));
        }
        foreach (var (i, _) in (~st).Fields) {
            // apply fields
            var f = Ꮡ((~st).Fields, i);
            addTypeBits(Ꮡbv, offset + (~f).Offset, (~f).Typ);
        }
    }

}

}
