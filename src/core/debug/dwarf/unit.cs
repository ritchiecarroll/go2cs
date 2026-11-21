// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using sort = sort_package;
using strconv = strconv_package;
using static dwarf_package;

partial class dwarf_package {

// DWARF debug info is split into a sequence of compilation units.
// Each unit has its own abbreviation table and address size.
partial struct unit {
    internal Offset @base; // byte offset of header within the aggregate info
    internal Offset off;   // byte offset of data within the aggregate info
    internal slice<byte> data;
    internal abbrevTable atable;
    /*embed*/ internal ж<unit5> unit5; // info specific to DWARF 5 units
    internal nint asize;
    internal nint vers;
    internal bool is64;   // True for 64-bit DWARF format
    internal uint8 utype; // DWARF 5 unit type
}

partial struct unit5 {
    internal uint64 addrBase;
    internal uint64 strOffsetsBase;
    internal uint64 rngListsBase;
    internal uint64 locListsBase;
}

} partial class dwarf_package {

// Implement the dataFormat interface.
[GoMethodBody("version")] internal static nint versionᴍ(ref unit u) {
    return u.vers;
}

[GoMethodBody("dwarf64")] internal static (bool, bool) dwarf64ᴍ(ref unit u) {
    return (u.is64, true);
}

[GoMethodBody("addrsize")] internal static nint addrsizeᴍ(ref unit u) {
    return u.asize;
}

[GoMethodBody("addrBase")] internal static uint64 addrBaseᴍ(ref unit u) {
    if (u.unit5 != nil) {
        return u.unit5.addrBase;
    }
    return 0;
}

[GoMethodBody("strOffsetsBase")] internal static uint64 strOffsetsBaseᴍ(ref unit u) {
    if (u.unit5 != nil) {
        return u.unit5.strOffsetsBase;
    }
    return 0;
}

[GoMethodBody("rngListsBase")] internal static uint64 rngListsBaseᴍ(ref unit u) {
    if (u.unit5 != nil) {
        return u.unit5.rngListsBase;
    }
    return 0;
}

[GoMethodBody("locListsBase")] internal static uint64 locListsBaseᴍ(ref unit u) {
    if (u.unit5 != nil) {
        return u.unit5.locListsBase;
    }
    return 0;
}

} partial class dwarf_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unitLengthOverflowˢᴍData => ref unitLengthOverflowˢᵇᴍData;
private static readonly @string unitLengthOverflowˢᵇᴍData = "unit length overflow"u8;

[GoMethodBody("parseUnits")] internal static (slice<unit>, error) parseUnitsᴍ(ж<Data> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    // Count units.
    nint nunit = 0;
    var b = makeBuf(Ꮡd, new unknownFormat(), infoˢᴍData, 0, d.info);
    while (len(b.data) > 0) {
        var (lenΔ1, _) = b.unitLength();
        if (lenΔ1 != ((Offset)(uint32)lenΔ1)) {
            b.error(unitLengthOverflowˢᴍData);
            break;
        }
        b.skip((nint)lenΔ1);
        if (lenΔ1 > 0) {
            nunit++;
        }
    }
    if (b.err != nil) {
        return (default, b.err);
    }

    // Again, this time writing them down.
    b = makeBuf(Ꮡd, new unknownFormat(), infoˢᴍData, 0, d.info);
    var units = new slice<unit>(nunit, () => new(nil));
    foreach (var i in range(len(units))) {
        var u = Ꮡ(units, i);
        u.@base = b.off;
        Offset n = default;
        if (b.err != nil) {
            return (default, b.err);
        }
        while (n == 0) {
            (n, u.is64) = b.unitLength();
        }
        var dataOff = b.off;
        var vers = b.uint16();
        if (vers < 2 || vers > 5) {
            b.error("unsupported DWARF version "u8 + strconv.Itoa((nint)vers));
            break;
        }
        u.vers = (nint)vers;
        if (vers >= 5) {
            u.utype = b.uint8();
            u.asize = (nint)b.uint8();
        }
        uint64 abbrevOff = default;
        if (u.is64) {
            abbrevOff = b.uint64();
        } else {
            abbrevOff = (uint64)b.uint32();
        }
        var (atable, err) = Ꮡd.parseAbbrev(abbrevOff, u.vers);
        if (err != nil) {
            if (b.err == nil) {
                b.err = err;
            }
            break;
        }
        u.atable = atable;
        if (vers < 5) {
            u.asize = (nint)b.uint8();
        }

        switch (u.utype) {
        case var ᴛ when ᴛ == utSkeleton || ᴛ == utSplitCompile: {
            b.uint64(); // unit ID
            break;
        }
        case var ᴛ when ᴛ == utType || ᴛ == utSplitType: {
            b.uint64();   // type signature
            if (u.is64) { // type offset
                b.uint64();
            } else {
                b.uint32();
            }
            break;
        }}

        u.off = b.off;
        u.data = b.bytes((nint)(n - (b.off - dataOff)));
    }
    if (b.err != nil) {
        return (default, b.err);
    }
    return (units, default);
}

// offsetToUnit returns the index of the unit containing offset off.
// It returns -1 if no unit contains this offset.
[GoMethodBody("offsetToUnit")] internal static nint offsetToUnitᴍ(ж<Data> Ꮡd, Offset off) {
    ref var d = ref Ꮡd.DerefOrNull();

    // Find the unit after off
    nint next = sort.Search(len(d.unit), (nint i) => Ꮡd.Value.unit[i].off > off);
    if (next == 0) {
        return -1;
    }
    var u = Ꮡ(d.unit, next - 1);
    if (u.off <= off && off < u.off + ((Offset)(uint32)len(u.data))) {
        return next - 1;
    }
    return -1;
}

[GoMethodBody("collectDwarf5BaseOffsets")] internal static error collectDwarf5BaseOffsetsᴍ(ж<Data> Ꮡd, ж<unit> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    if (u.unit5 == nil) {
        throw panic("expected unit5 to be set up already");
    }
    ref var b = ref heap<buf>(makeBuf(Ꮡd, new unitжdataFormat(Ꮡu), infoˢᴍData, u.off, u.data), out var Ꮡb);
    var cu = Ꮡb.entry(nil, Ꮡu);
    if (cu == nil) {
        // Unknown abbreviation table entry or some other fatal
        // problem; bail early on the assumption that this will be
        // detected at some later point.
        return b.err;
    }
    if (cu.Val(AttrAddrBase)._<int64>(out var iAddrBase)) {
        u.unit5.addrBase = (uint64)iAddrBase;
    }
    if (cu.Val(AttrStrOffsetsBase)._<int64>(out var iStrOffsetsBase)) {
        u.unit5.strOffsetsBase = (uint64)iStrOffsetsBase;
    }
    if (cu.Val(AttrRnglistsBase)._<int64>(out var iRngListsBase)) {
        u.unit5.rngListsBase = (uint64)iRngListsBase;
    }
    if (cu.Val(AttrLoclistsBase)._<int64>(out var iLocListsBase)) {
        u.unit5.locListsBase = (uint64)iLocListsBase;
    }
    return default;
}

}
