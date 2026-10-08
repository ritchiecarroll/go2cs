// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package dwarf provides access to DWARF debugging information loaded from
executable files, as defined in the DWARF 2.0 Standard at
http://dwarfstd.org/doc/dwarf-2.0.0.pdf.

# Security

This package is not designed to be hardened against adversarial inputs, and is
outside the scope of https://go.dev/security/policy. In particular, only basic
validation is done when parsing object files. As such, care should be taken when
parsing untrusted inputs, as parsing malformed files may consume significant
resources, or cause panics.
*/
namespace go.debug;

using binary = encoding.binary_package;
using errors = errors_package;
using encoding;

partial class dwarf_package {

// Data represents the DWARF debugging information
// loaded from an executable file (for example, an ELF or Mach-O executable).
partial struct Data {
    // raw data
    internal slice<byte> abbrev;
    internal slice<byte> aranges;
    internal slice<byte> frame;
    internal slice<byte> info;
    internal slice<byte> line;
    internal slice<byte> pubnames;
    internal slice<byte> ranges;
    internal slice<byte> str;
    // New sections added in DWARF 5.
    internal slice<byte> addr;
    internal slice<byte> lineStr;
    internal slice<byte> strOffsets;
    internal slice<byte> rngLists;
    // parsed data
    internal map<uint64, abbrevTable> abbrevCache;
    internal bool bigEndian;
    internal binary.ByteOrder order;
    internal map<Offset, ΔType> typeCache;
    internal map<uint64, ж<typeUnit>> typeSigs;
    internal slice<unit> unit;
}

internal static error errSegmentSelector = errors.New("non-zero segment_selector size not supported"u8);

// New returns a new [Data] object initialized from the given parameters.
// Rather than calling this function directly, clients should typically use
// the DWARF method of the File type of the appropriate package [debug/elf],
// [debug/macho], or [debug/pe].
//
// The []byte arguments are the data from the corresponding debug section
// in the object file; for example, for an ELF object, abbrev is the contents of
// the ".debug_abbrev" section.
public static (ж<Data>, error) New(slice<byte> abbrev, slice<byte> aranges, slice<byte> frame, slice<byte> info, slice<byte> line, slice<byte> pubnames, slice<byte> ranges, slice<byte> str) {
    var d = Ꮡ(new Data(
        abbrev: abbrev,
        aranges: aranges,
        frame: frame,
        info: info,
        line: line,
        pubnames: pubnames,
        ranges: ranges,
        str: str,
        abbrevCache: new map<uint64, abbrevTable>(),
        typeCache: new map<Offset, ΔType>(),
        typeSigs: new map<uint64, ж<typeUnit>>()
    ));
    // Sniff .debug_info to figure out byte order.
    // 32-bit DWARF: 4 byte length, 2 byte version.
    // 64-bit DWARf: 4 bytes of 0xff, 8 byte length, 2 byte version.
    if (len((~d).info) < 6) {
        return (default!, new DecodeError("info"u8, ((Offset)(uint32)len((~d).info)), "too short"u8));
    }
    nint offset = 4;
    if ((~d).info[0] == 0xff && (~d).info[1] == 0xff && (~d).info[2] == 0xff && (~d).info[3] == 0xff) {
        if (len((~d).info) < 14) {
            return (default!, new DecodeError("info"u8, ((Offset)(uint32)len((~d).info)), "too short"u8));
        }
        offset = 12;
    }
    // Fetch the version, a tiny 16-bit number (1, 2, 3, 4, 5).
    var (x, y) = ((~d).info[offset], (~d).info[offset + 1]);
    switch (ᐧ) {
    case {} when x == 0 && y == 0: {
        return (default!, new DecodeError("info"u8, 4, "unsupported version 0"u8));
    }
    case {} when x is 0: {
        d.Value.bigEndian = true;
        d.Value.order = binary.BigEndian;
        break;
    }
    case {} when y is 0: {
        d.Value.bigEndian = false;
        d.Value.order = binary.LittleEndian;
        break;
    }
    default: {
        return (default!, new DecodeError("info"u8, 4, "cannot determine byte order"u8));
    }}

    var (u, err) = d.parseUnits();
    if (err != default!) {
        return (default!, err);
    }
    d.Value.unit = u;
    return (d, default!);
}

// AddTypes will add one .debug_types section to the DWARF data. A
// typical object with DWARF version 4 debug info will have multiple
// .debug_types sections. The name is used for error reporting only,
// and serves to distinguish one .debug_types section from another.
public static error AddTypes(this ж<Data> Ꮡd, @string name, slice<byte> types) {
    return Ꮡd.parseTypes(name, types);
}

// AddSection adds another DWARF section by name. The name should be a
// DWARF section name such as ".debug_addr", ".debug_str_offsets", and
// so forth. This approach is used for new DWARF sections added in
// DWARF 5 and later.
public static error AddSection(this ref Data d, @string name, slice<byte> contents) {
    error err = default!;
    var exprᴛ1 = name;
    if (exprᴛ1 == ".debug_addr"u8) {
        d.addr = contents;
    }
    else if (exprᴛ1 == ".debug_line_str"u8) {
        d.lineStr = contents;
    }
    else if (exprᴛ1 == ".debug_str_offsets"u8) {
        d.strOffsets = contents;
    }
    else if (exprᴛ1 == ".debug_rnglists"u8) {
        d.rngLists = contents;
    }

    // Just ignore names that we don't yet support.
    return err;
}

} // end dwarf_package
