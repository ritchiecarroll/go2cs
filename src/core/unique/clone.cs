// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs HAND-OWNED FILE (replaces the converted clone.go output). Only clone<T> departs from the
// conversion; makeCloneSeq and the cloneSeq builders below are kept in their converted form, because
// they are pure descriptor arithmetic that unique's own TestMakeCloneSeq validates.
//
// WHY clone<T> CANNOT BE THE CONVERTED FORM. Go rewrites every string field of `value` in place
// through raw ABI offsets:
//
//     ps := (*string)(unsafe.Pointer(uintptr(unsafe.Pointer(&value)) + offset))
//     *ps = stringslite.Clone(*ps)
//
// The converted form of that line addressed a movable ж<T> heap box by interior address plus a GO
// ABI offset — but the box is a CLR object whose field layout is unrelated to Go's ABI, so for any
// T whose strings sit at nonzero offsets the @string read/write landed on the box's OWN fields.
// While @string was a single 8-byte reference, every mislaid store was one aligned pointer-sized
// slot: silently type-confusing, but nothing the collector tripped over. When @string became an
// offset/length WINDOW (fc6d8c179, r57c) the store widened to 16 bytes and its integer tail landed
// in a GC-scanned reference slot of the box; the next collection walked a garbage pointer and the
// process fail-fasted with COR_E_EXECUTIONENGINE (0x80131506) — and unique's own drainMaps FORCES
// that collection via runtime.GC() in every TestHandle subtest, so the test host died with zero
// verdicts. Bisected and root-caused 2026-08-12 (board: the scout-batch-1 `unique` entry); the
// mechanism reproduces in ~25 lines against golib alone, and the same program against pre-window
// golib corrupts values without faulting — the two eras of one defect.
//
// WHAT THIS DOES INSTEAD — the documented S1 managed-referent fork. clone's contract is "makes a
// copy of value, and MAY update string values found in value with a cloned version of those
// strings": the cloning is a retention optimization (an interned handle must not keep a large
// parent string alive), never a semantic requirement.
//   * T == string (the singleStringClone case): return the right-sized copy Go would produce —
//     expressible with no address arithmetic, and worth more now that @string is a window over
//     possibly-shared backing.
//   * aggregate T (strings behind struct/array fields): return value unchanged. The observable
//     difference from Go is retention only — the interned value's strings keep sharing their
//     original backing — never equality, identity, or intern-map drainage, which are what
//     unique.Make is about.
// makeCloneSeq still computes the real offsets (TestMakeCloneSeq covers them); they are simply
// never used to address managed memory.

using System.Runtime.CompilerServices;

[module: go.GoManualConversion]

namespace go;

using abi = @internal.abi_package;
using stringslite = @internal.stringslite_package;
using @internal;

partial class unique_package {

// clone makes a copy of value, and may update string values found in value
// with a cloned version of those strings. The purpose of explicitly cloning
// strings is to avoid accidentally giving a large string a long lifetime.
//
// Note that this will clone strings in structs and arrays found in value,
// and will clone value if it itself is a string. It will not, however, clone
// strings if value is of interface or slice type (that is, found via an
// indirection).
internal static T clone<T>(T value, ж<cloneSeq> Ꮡseq)
{
    // A value that IS a string takes Go's clone verbatim: one right-sized copy. The typeof test is
    // a JIT-time constant per instantiation, and Unsafe.As over the proven-identical T is the golib
    // idiom for it (see ByteSeqExtensions.ToGoString).
    if (typeof(T) == typeof(@string))
    {
        ref @string s = ref Unsafe.As<T, @string>(ref value);
        s = stringslite.Clone(s);
    }

    // Strings behind struct or array fields stay shared — see the file header for why the
    // converted offset walk cannot run against managed layout, and what identity leaves observable
    // (retention only).
    return value;
}

// singleStringClone describes how to clone a single string.
internal static cloneSeq singleStringClone = new cloneSeq(stringOffsets: new uintptr[]{0}.slice());

// cloneSeq describes how to clone a value of a particular type.
[GoType] partial struct cloneSeq {
    internal slice<uintptr> stringOffsets;
}

// makeCloneSeq creates a cloneSeq for a type.
internal static cloneSeq makeCloneSeq(ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (Ꮡtyp == nil) {
        return new cloneSeq(nil);
    }
    if (typ.Kind() == abi.ΔString) {
        return singleStringClone;
    }
    ref var seq = ref heap(new cloneSeq(), out var Ꮡseq);
    var exprᴛ1 = typ.Kind();
    if (exprᴛ1 == abi.Struct) {
        buildStructCloneSeq(Ꮡtyp, Ꮡseq, 0);
    }
    else if (exprᴛ1 == abi.Array) {
        buildArrayCloneSeq(Ꮡtyp, Ꮡseq, 0);
    }

    return seq;
}

// buildStructCloneSeq populates a cloneSeq for an abi.Type that has Kind abi.Struct.
internal static void buildStructCloneSeq(ж<abi.Type> Ꮡtyp, ж<cloneSeq> Ꮡseq, uintptr baseOffset) {
    ref var seq = ref Ꮡseq.DerefOrNull();

    var styp = Ꮡtyp.StructType();
    foreach (var i in range((~styp).Fields)) {
        var f = Ꮡ((~styp).Fields, i);
        var exprᴛ1 = (~f).Typ.Kind();
        if (exprᴛ1 == abi.ΔString) {
            seq.stringOffsets = append(seq.stringOffsets, baseOffset + (~f).Offset);
        }
        else if (exprᴛ1 == abi.Struct) {
            buildStructCloneSeq((~f).Typ, Ꮡseq, baseOffset + (~f).Offset);
        }
        else if (exprᴛ1 == abi.Array) {
            buildArrayCloneSeq((~f).Typ, Ꮡseq, baseOffset + (~f).Offset);
        }

    }
}

// buildArrayCloneSeq populates a cloneSeq for an abi.Type that has Kind abi.Array.
internal static void buildArrayCloneSeq(ж<abi.Type> Ꮡtyp, ж<cloneSeq> Ꮡseq, uintptr baseOffset) {
    ref var seq = ref Ꮡseq.DerefOrNull();

    var atyp = Ꮡtyp.ArrayType();
    var etyp = atyp.Value.Elem;
    var offset = baseOffset;
    foreach (var _ᴛ1 in range<uintptr>((~atyp).Len)) {
        var exprᴛ1 = etyp.Kind();
        if (exprᴛ1 == abi.ΔString) {
            seq.stringOffsets = append(seq.stringOffsets, offset);
        }
        else if (exprᴛ1 == abi.Struct) {
            buildStructCloneSeq(etyp, Ꮡseq, offset);
        }
        else if (exprᴛ1 == abi.Array) {
            buildArrayCloneSeq(etyp, Ꮡseq, offset);
        }

        offset += etyp.Size();
        var align = (uintptr)etyp.FieldAlign();
        offset = (uintptr)((offset + align - 1) & ~(align - 1));
    }
}

} // end unique_package
