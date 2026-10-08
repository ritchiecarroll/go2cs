// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using sort = sort_package;
using strconv = strconv_package;

partial class dwarf_package {

// DWARF debug info is split into a sequence of compilation units.
// Each unit has its own abbreviation table and address size.
partial struct unit {
    internal Offset @base; // byte offset of header within the aggregate info
    internal Offset off; // byte offset of data within the aggregate info
    internal slice<byte> data;
    internal abbrevTable atable;
    internal nint asize;
    internal nint vers;
    internal uint8 utype; // DWARF 5 unit type
    internal bool is64;  // True for 64-bit DWARF format
}

// Implement the dataFormat interface.
internal static nint version(this ref unit u) {
    return u.vers;
}

internal static (bool, bool) dwarf64(this ref unit u) {
    return (u.is64, true);
}

internal static nint addrsize(this ref unit u) {
    return u.asize;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unitLengthOverflowˢ = "unit length overflow"u8;

internal static (slice<unit>, error) parseUnits(this ж<Data> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    // Count units.
    nint nunit = 0;
    var b = makeBuf(Ꮡd, new unknownFormat(nil), infoˢ, 0, d.info);
    while (len(b.data) > 0) {
        var (lenΔ1, _) = b.unitLength();
        if (lenΔ1 != ((Offset)(uint32)lenΔ1)) {
            b.error(unitLengthOverflowˢ);
            break;
        }
        b.skip((nint)(uint32)lenΔ1);
        if (lenΔ1 > 0) {
            nunit++;
        }
    }
    if (b.err != default!) {
        return (default!, b.err);
    }
    // Again, this time writing them down.
    b = makeBuf(Ꮡd, new unknownFormat(nil), infoˢ, 0, d.info);
    var units = new slice<unit>(nunit);
    foreach (var (i, _) in units) {
        var u = Ꮡ(units, i);
        u.Value.@base = b.off;
        Offset n = default!;
        if (b.err != default!) {
            return (default!, b.err);
        }
        while (n == 0) {
            (n, u.Value.is64) = b.unitLength();
        }
        var dataOff = b.off;
        var vers = b.uint16();
        if (vers < 2 || vers > 5) {
            b.error("unsupported DWARF version "u8 + strconv.Itoa((nint)vers));
            break;
        }
        u.Value.vers = (nint)vers;
        if (vers >= 5) {
            u.Value.utype = b.uint8();
            u.Value.asize = (nint)b.uint8();
        }
        uint64 abbrevOff = default!;
        if ((~u).is64){
            abbrevOff = b.uint64();
        } else {
            abbrevOff = (uint64)b.uint32();
        }
        var (atable, err) = Ꮡd.parseAbbrev(abbrevOff, (~u).vers);
        if (err != default!) {
            if (b.err == default!) {
                b.err = err;
            }
            break;
        }
        u.Value.atable = atable;
        if (vers < 5) {
            u.Value.asize = (nint)b.uint8();
        }
        var exprᴛ1 = (~u).utype;
        if (exprᴛ1 == utSkeleton || exprᴛ1 == utSplitCompile) {
            b.uint64(); // unit ID
        }
        else if (exprᴛ1 == utType || exprᴛ1 == utSplitType) {
            b.uint64(); // type signature
            if ((~u).is64){
                // type offset
                b.uint64();
            } else {
                b.uint32();
            }
        }

        u.Value.off = b.off;
        u.Value.data = b.bytes((nint)(uint32)(n - (b.off - dataOff)));
    }
    if (b.err != default!) {
        return (default!, b.err);
    }
    return (units, default!);
}

// offsetToUnit returns the index of the unit containing offset off.
// It returns -1 if no unit contains this offset.
internal static nint offsetToUnit(this ж<Data> Ꮡd, Offset off) {
    ref var d = ref Ꮡd.DerefOrNull();

    // Find the unit after off
    nint next = sort.Search(len(d.unit), (nint i) => Ꮡd.Value.unit[i].off > off);
    if (next == 0) {
        return -1;
    }
    var u = Ꮡ(d.unit, next - 1);
    if ((~u).off <= off && off < (~u).off + ((Offset)(uint32)len((~u).data))) {
        return next - 1;
    }
    return -1;
}

} // end dwarf_package
