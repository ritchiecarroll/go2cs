// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// DWARF debug information entry parser.
// An entry is a sequence of data items of a given format.
// The first word in the entry is an index into what DWARF
// calls the ``abbreviation table.''  An abbreviation is really
// just a type descriptor: it's an array of attribute tag/value format pairs.
namespace go.debug;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using strconv = strconv_package;
using encoding;

partial class dwarf_package {

// a single entry's description: a sequence of attributes
[GoType] partial struct abbrev {
    internal Tag tag;
    internal bool children;
    internal slice<afield> field;
}

[GoType] partial struct afield {
    internal Attr attr;
    internal format fmt;
    internal Class @class;
    internal int64 val; // for formImplicitConst
}

[GoType("map[uint32, abbrev]")] partial struct abbrevTable;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abbrevˢ = "abbrev"u8;

// parseAbbrev returns the abbreviation table that starts at byte off
// in the .debug_abbrev section.
internal static (abbrevTable, error) parseAbbrev(this ж<Data> Ꮡd, uint64 off, nint vers) {
    ref var d = ref Ꮡd.DerefOrNull();

    {
        var (mΔ1, ok) = d.abbrevCache[off, ꟷ]; if (ok) {
            return (mΔ1, default!);
        }
    }
    var data = d.abbrev;
    if (off > (uint64)len(data)){
        data = default!;
    } else {
        data = data.slice((nint)(off));
    }
    ref var b = ref heap<buf>(out var Ꮡb);
    b = makeBuf(Ꮡd, new unknownFormat(nil), abbrevˢ, 0, data);
    // Error handling is simplified by the buf getters
    // returning an endless stream of 0s after an error.
    var m = new abbrevTable(0);
    while (ᐧ) {
        // Table ends with id == 0.
        var id = (uint32)b.@uint();
        if (id == 0) {
            break;
        }
        // Walk over attributes, counting.
        nint n = 0;
        var b1 = b; // Read from copy of b.
        b1.@uint();
        b1.uint8();
        while (ᐧ) {
            var tag = b1.@uint();
            var fmt = b1.@uint();
            if (tag == 0 && fmt == 0) {
                break;
            }
            if (((format)(uint32)fmt) == formImplicitConst) {
                b1.@int();
            }
            n++;
        }
        if (b1.err != default!) {
            return (default!, b1.err);
        }
        // Walk over attributes again, this time writing them down.
        abbrev a = default!;
        a.tag = ((Tag)(uint32)b.@uint());
        a.children = b.uint8() != 0;
        a.field = new slice<afield>(n);
        foreach (var (i, _) in a.field) {
            a.field[i].attr = ((Attr)(uint32)b.@uint());
            a.field[i].fmt = ((format)(uint32)b.@uint());
            a.field[i].@class = formToClass(a.field[i].fmt, a.field[i].attr, vers, Ꮡb);
            if (a.field[i].fmt == formImplicitConst) {
                a.field[i].val = b.@int();
            }
        }
        b.@uint();
        b.@uint();
        m[id] = a;
    }
    if (b.err != default!) {
        return (default!, b.err);
    }
    d.abbrevCache[off] = m;
    return (m, default!);
}

// attrIsExprloc indicates attributes that allow exprloc values that
// are encoded as block values in DWARF 2 and 3. See DWARF 4, Figure
// 20.
internal static map<Attr, bool> attrIsExprloc = new map<Attr, bool>{
    [AttrLocation] = true,
    [AttrByteSize] = true,
    [AttrBitOffset] = true,
    [AttrBitSize] = true,
    [AttrStringLength] = true,
    [AttrLowerBound] = true,
    [AttrReturnAddr] = true,
    [AttrStrideSize] = true,
    [AttrUpperBound] = true,
    [AttrCount] = true,
    [AttrDataMemberLoc] = true,
    [AttrFrameBase] = true,
    [AttrSegment] = true,
    [AttrStaticLink] = true,
    [AttrUseLocation] = true,
    [AttrVtableElemLoc] = true,
    [AttrAllocated] = true,
    [AttrAssociated] = true,
    [AttrDataLocation] = true,
    [AttrStride] = true
};

// The following are new in DWARF 5.
// attrPtrClass indicates the *ptr class of attributes that have
// encoding formSecOffset in DWARF 4 or formData* in DWARF 2 and 3.
internal static map<Attr, Class> attrPtrClass = new map<Attr, Class>{
    [AttrLocation] = ClassLocListPtr,
    [AttrStmtList] = ClassLinePtr,
    [AttrStringLength] = ClassLocListPtr,
    [AttrReturnAddr] = ClassLocListPtr,
    [AttrStartScope] = ClassRangeListPtr,
    [AttrDataMemberLoc] = ClassLocListPtr,
    [AttrFrameBase] = ClassLocListPtr,
    [AttrMacroInfo] = ClassMacPtr,
    [AttrSegment] = ClassLocListPtr,
    [AttrStaticLink] = ClassLocListPtr,
    [AttrUseLocation] = ClassLocListPtr,
    [AttrVtableElemLoc] = ClassLocListPtr,
    [AttrRanges] = ClassRangeListPtr,
    [AttrStrOffsetsBase] = ClassStrOffsetsPtr,
    [AttrAddrBase] = ClassAddrPtr,
    [AttrRnglistsBase] = ClassRngListsPtr,
    [AttrLoclistsBase] = ClassLocListPtr
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotDetermineClassOfˢ = "cannot determine class of unknown attribute form"u8;

// formToClass returns the DWARF 4 Class for the given form. If the
// DWARF version is less then 4, it will disambiguate some forms
// depending on the attribute.
internal static Class formToClass(format form, Attr attr, nint vers, ж<buf> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var exprᴛ1 = form;
    if (exprᴛ1 == formIndirect) {
        return ClassUnknown;
    }
    if (exprᴛ1 == formAddr || exprᴛ1 == formAddrx || exprᴛ1 == formAddrx1 || exprᴛ1 == formAddrx2 || exprᴛ1 == formAddrx3 || exprᴛ1 == formAddrx4) {
        return ClassAddress;
    }
    if (exprᴛ1 == formDwarfBlock1 || exprᴛ1 == formDwarfBlock2 || exprᴛ1 == formDwarfBlock4 || exprᴛ1 == formDwarfBlock) {
        if (attrIsExprloc[attr]) {
            // In DWARF 2 and 3, ClassExprLoc was encoded as a
            // block. DWARF 4 distinguishes ClassBlock and
            // ClassExprLoc, but there are no attributes that can
            // be both, so we also promote ClassBlock values in
            // DWARF 4 that should be ClassExprLoc in case
            // producers get this wrong.
            return ClassExprLoc;
        }
        return ClassBlock;
    }
    if (exprᴛ1 == formData1 || exprᴛ1 == formData2 || exprᴛ1 == formData4 || exprᴛ1 == formData8 || exprᴛ1 == formSdata || exprᴛ1 == formUdata || exprᴛ1 == formData16 || exprᴛ1 == formImplicitConst) {
        {
            var (@class, ok) = attrPtrClass[attr, ꟷ]; if (vers < 4 && ok) {
                // In DWARF 2 and 3, ClassPtr was encoded as a
                // constant. Unlike ClassExprLoc/ClassBlock, some
                // DWARF 4 attributes need to distinguish Class*Ptr
                // from ClassConstant, so we only do this promotion
                // for versions 2 and 3.
                return @class;
            }
        }
        return ClassConstant;
    }
    if (exprᴛ1 == formFlag || exprᴛ1 == formFlagPresent) {
        return ClassFlag;
    }
    if (exprᴛ1 == formRefAddr || exprᴛ1 == formRef1 || exprᴛ1 == formRef2 || exprᴛ1 == formRef4 || exprᴛ1 == formRef8 || exprᴛ1 == formRefUdata || exprᴛ1 == formRefSup4 || exprᴛ1 == formRefSup8) {
        return ClassReference;
    }
    if (exprᴛ1 == formRefSig8) {
        return ClassReferenceSig;
    }
    if (exprᴛ1 == formString || exprᴛ1 == formStrp || exprᴛ1 == formStrx || exprᴛ1 == formStrpSup || exprᴛ1 == formLineStrp || exprᴛ1 == formStrx1 || exprᴛ1 == formStrx2 || exprᴛ1 == formStrx3 || exprᴛ1 == formStrx4) {
        return ClassString;
    }
    if (exprᴛ1 == formSecOffset) {
        {
            var (@class, ok) = attrPtrClass[attr, ꟷ]; if (ok) {
                // DWARF 4 defines four *ptr classes, but doesn't
                // distinguish them in the encoding. Disambiguate
                // these classes using the attribute.
                return @class;
            }
        }
        return ClassUnknown;
    }
    if (exprᴛ1 == formExprloc) {
        return ClassExprLoc;
    }
    if (exprᴛ1 == formGnuRefAlt) {
        return ClassReferenceAlt;
    }
    if (exprᴛ1 == formGnuStrpAlt) {
        return ClassStringAlt;
    }
    if (exprᴛ1 == formLoclistx) {
        return ClassLocList;
    }
    if (exprᴛ1 == formRnglistx) {
        return ClassRngList;
    }
    { /* default: */
        b.error(cannotDetermineClassOfˢ);
        return 0;
    }

}

// An entry is a sequence of attribute/value pairs.
[GoType] partial struct Entry {
    public Offset Offset; // offset of Entry in DWARF info
    public Tag Tag;    // tag (kind of Entry)
    public bool Children;   // whether Entry is followed by children
    public slice<Field> Field;
}

// A Field is a single attribute/value pair in an [Entry].
//
// A value can be one of several "attribute classes" defined by DWARF.
// The Go types corresponding to each class are:
//
//	DWARF class       Go type        Class
//	-----------       -------        -----
//	address           uint64         ClassAddress
//	block             []byte         ClassBlock
//	constant          int64          ClassConstant
//	flag              bool           ClassFlag
//	reference
//	  to info         dwarf.Offset   ClassReference
//	  to type unit    uint64         ClassReferenceSig
//	string            string         ClassString
//	exprloc           []byte         ClassExprLoc
//	lineptr           int64          ClassLinePtr
//	loclistptr        int64          ClassLocListPtr
//	macptr            int64          ClassMacPtr
//	rangelistptr      int64          ClassRangeListPtr
//
// For unrecognized or vendor-defined attributes, [Class] may be
// [ClassUnknown].
[GoType] partial struct Field {
    public Attr Attr;
    public any Val;
    public Class Class;
}

[GoType("num:nint")] partial struct Class;

public static Class ClassUnknown => /* iota */ 0;
public static Class ClassAddress => 1;
public static Class ClassBlock => 2;
public static Class ClassConstant => 3;
public static Class ClassExprLoc => 4;
public static Class ClassFlag => 5;
public static Class ClassLinePtr => 6;
public static Class ClassLocListPtr => 7;
public static Class ClassMacPtr => 8;
public static Class ClassRangeListPtr => 9;
public static Class ClassReference => 10;
public static Class ClassReferenceSig => 11;
public static Class ClassString => 12;
public static Class ClassReferenceAlt => 13;
public static Class ClassStringAlt => 14;
public static Class ClassAddrPtr => 15;
public static Class ClassLocList => 16;
public static Class ClassRngList => 17;
public static Class ClassRngListsPtr => 18;
public static Class ClassStrOffsetsPtr => 19;

//go:generate stringer -type=Class
public static @string GoString(this Class i) {
    return "dwarf."u8 + i.String();
}

// Val returns the value associated with attribute [Attr] in [Entry],
// or nil if there is no such attribute.
//
// A common idiom is to merge the check for nil return with
// the check that the value has the expected dynamic type, as in:
//
//	v, ok := e.Val(AttrSibling).(int64)
[GoRecv] public static any Val(this ref Entry e, Attr a) {
    {
        var f = e.AttrField(a); if (f != nil) {
            return (~f).Val;
        }
    }
    return default!;
}

// AttrField returns the [Field] associated with attribute [Attr] in
// [Entry], or nil if there is no such attribute.
[GoRecv] public static ж<Field> AttrField(this ref Entry e, Attr a) {
    foreach (var (i, f) in e.Field) {
        if (f.Attr == a) {
            return Ꮡ(e.Field, i);
        }
    }
    return default!;
}

[GoType("num:uint32")] partial struct Offset;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownAbbreviationTableˢ = "unknown abbreviation table index"u8;
internal static readonly @string dwFormStrxOffsetOutOfˢ = "DW_FORM_strx offset out of range"u8;
internal static readonly @string strOffsetsˢ = "str_offsets"u8;
internal static readonly @string dwFormStrxIndirectOffsetˢ = "DW_FORM_strx indirect offset out of range"u8;
internal static readonly @string strˢ = "str"u8;
internal static readonly @string dwFormRnglistxOffsetOutˢ = "DW_FORM_rnglistx offset out of range"u8;
internal static readonly @string rnglistsˢ = "rnglists"u8;
internal static readonly @string dwFormRnglistxIndirectˢ = "DW_FORM_rnglistx indirect offset out of range"u8;
internal static readonly @string dwFormAddrxWithNoDebugˢ = "DW_FORM_addrx with no .debug_addr section"u8;
internal static readonly @string unknownVersionForDwFormˢ = "unknown version for DW_FORM_ref_addr"u8;
internal static readonly @string unknownSizeForDwFormRefˢ = "unknown size for DW_FORM_ref_addr"u8;
internal static readonly @string unknownSizeForDwFormStrpˢ = "unknown size for DW_FORM_strp/line_strp"u8;
internal static readonly @string dwFormStrpLineStrpOffsetˢ = "DW_FORM_strp/line_strp offset out of range"u8;
internal static readonly @string dwFormLineStrpWithNoˢ = "DW_FORM_line_strp with no .debug_line_str section"u8;
internal static readonly @string lineStrˢ = "line_str"u8;
internal static readonly @string dwFormStrxWithNoDebugStrˢ = "DW_FORM_strx with no .debug_str_offsets section"u8;
internal static readonly @string unknownOffsetSizeForDwˢ = "unknown offset size for DW_FORM_strx"u8;
internal static readonly @string unknownSizeForDwFormStrpˢ2 = "unknown size for DW_FORM_strp_sup"u8;

// If we are currently parsing the compilation unit,
// we can't evaluate Addrx or Strx until we've seen the
// relevant base entry.
[GoType("dyn")] internal partial struct entry_delayed {
    internal nint idx;
    internal uint64 off;
    internal format fmt;
}

// Entry reads a single entry from buf, decoding
// according to the given abbreviation table.
internal static ж<Entry> entry(this ж<buf> Ꮡb, ж<Entry> Ꮡcu, abbrevTable atab, Offset ubase, nint vers) {
    ref var b = ref Ꮡb.DerefOrNull();
    ref var cu = ref Ꮡcu.DerefOrNull();

    ref var off = ref heap<Offset>(out var Ꮡoff);
    off = b.off;
    var id = (uint32)b.@uint();
    if (id == 0) {
        return Ꮡ(new Entry(nil));
    }
    var (a, ok) = atab[id, ꟷ];
    if (!ok) {
        b.error(unknownAbbreviationTableˢ);
        return default!;
    }
    var e = Ꮡ(new Entry(
        Offset: off,
        Tag: a.tag,
        Children: a.children,
        Field: new slice<Field>(len(a.field))
    ));
    slice<entry_delayed> delay = default!;
    @string resolveStrx(uint64 strBase, uint64 offΔ1) {
        offΔ1 += strBase;
        if ((uint64)(nint)offΔ1 != offΔ1) {
            Ꮡb.Value.error(dwFormStrxOffsetOutOfˢ);
        }
        var b1 = makeBuf(Ꮡb.Value.dwarf, Ꮡb.Value.format, strOffsetsˢ, 0, (~Ꮡb.Value.dwarf).strOffsets);
        b1.skip((nint)offΔ1);
        var (is64, _) = Ꮡb.Value.format.dwarf64();
        if (is64){
            offΔ1 = b1.uint64();
        } else {
            offΔ1 = (uint64)b1.uint32();
        }
        if (b1.err != default!) {
            Ꮡb.Value.err = b1.err;
            return ""u8;
        }
        if ((uint64)(nint)offΔ1 != offΔ1) {
            Ꮡb.Value.error(dwFormStrxIndirectOffsetˢ);
        }
        b1 = makeBuf(Ꮡb.Value.dwarf, Ꮡb.Value.format, strˢ, 0, (~Ꮡb.Value.dwarf).str);
        b1.skip((nint)offΔ1);
        @string val = b1.@string();
        if (b1.err != default!) {
            Ꮡb.Value.err = b1.err;
        }
        return val;
    }
    uint64 resolveRnglistx(uint64 rnglistsBase, uint64 offΔ2) {
        var (is64, _) = Ꮡb.Value.format.dwarf64();
        if (is64){
            offΔ2 *= 8;
        } else {
            offΔ2 *= 4;
        }
        offΔ2 += rnglistsBase;
        if ((uint64)(nint)offΔ2 != offΔ2) {
            Ꮡb.Value.error(dwFormRnglistxOffsetOutˢ);
        }
        var b1 = makeBuf(Ꮡb.Value.dwarf, Ꮡb.Value.format, rnglistsˢ, 0, (~Ꮡb.Value.dwarf).rngLists);
        b1.skip((nint)offΔ2);
        if (is64){
            offΔ2 = b1.uint64();
        } else {
            offΔ2 = (uint64)b1.uint32();
        }
        if (b1.err != default!) {
            Ꮡb.Value.err = b1.err;
            return (uint64)(0);
        }
        if ((uint64)(nint)offΔ2 != offΔ2) {
            Ꮡb.Value.error(dwFormRnglistxIndirectˢ);
        }
        return rnglistsBase + offΔ2;
    }
    foreach (var (i, _) in (~e).Field) {
        (~e).Field[i].Attr = a.field[i].attr;
        (~e).Field[i].Class = a.field[i].@class;
        var fmt = a.field[i].fmt;
        if (fmt == formIndirect) {
            fmt = ((format)(uint32)b.@uint());
            (~e).Field[i].Class = formToClass(fmt, a.field[i].attr, vers, Ꮡb);
        }
        any val = default!;
        var exprᴛ1 = fmt;
        if (exprᴛ1 == formAddr) {
            val = b.addr();
        }
        else if (exprᴛ1 == formAddrx || exprᴛ1 == formAddrx1 || exprᴛ1 == formAddrx2 || exprᴛ1 == formAddrx3 || exprᴛ1 == formAddrx4) {
            do {
// address
                uint64 offΔ6 = default!;
                var exprᴛ2 = fmt;
                if (exprᴛ2 == formAddrx) {
                    offΔ6 = b.@uint();
                }
                else if (exprᴛ2 == formAddrx1) {
                    offΔ6 = (uint64)b.uint8();
                }
                else if (exprᴛ2 == formAddrx2) {
                    offΔ6 = (uint64)b.uint16();
                }
                else if (exprᴛ2 == formAddrx3) {
                    offΔ6 = (uint64)b.uint24();
                }
                else if (exprᴛ2 == formAddrx4) {
                    offΔ6 = (uint64)b.uint32();
                }

                if ((~b.dwarf).addr == default!) {
                    b.error(dwFormAddrxWithNoDebugˢ);
                }
                if (b.err != default!) {
                    return default!;
                }
                // We have to adjust by the offset of the
                // compilation unit. This won't work if the
                // program uses Reader.Seek to skip over the
                // unit. Not much we can do about that.
                int64 addrBase = default!;
                if (Ꮡcu != nil){
                    (addrBase, _) = cu.Val(AttrAddrBase)._<int64>(ᐧ);
                } else 
                if (a.tag == TagCompileUnit) {
                    delay = append(delay, new entry_delayed(i, offΔ6, formAddrx));
                    break;
                }
                error err = default!;
                (val, err) = b.dwarf.debugAddr(b.format, (uint64)addrBase, offΔ6);
                if (err != default!) {
                    if (b.err == default!) {
                        b.err = err;
                    }
                    return default!;
                }
            } while (false);
        }
        else if (exprᴛ1 == formDwarfBlock1) {
            val = b.bytes((nint)b.uint8());
        }
        else if (exprᴛ1 == formDwarfBlock2) {
            val = b.bytes((nint)b.uint16());
        }
        else if (exprᴛ1 == formDwarfBlock4) {
            val = b.bytes((nint)b.uint32());
        }
        else if (exprᴛ1 == formDwarfBlock) {
            val = b.bytes((nint)b.@uint());
        }
        else if (exprᴛ1 == formData1) {
            val = (int64)b.uint8();
        }
        else if (exprᴛ1 == formData2) {
            val = (int64)b.uint16();
        }
        else if (exprᴛ1 == formData4) {
            val = (int64)b.uint32();
        }
        else if (exprᴛ1 == formData8) {
            val = (int64)b.uint64();
        }
        else if (exprᴛ1 == formData16) {
            val = b.bytes(16);
        }
        else if (exprᴛ1 == formSdata) {
            val = (int64)b.@int();
        }
        else if (exprᴛ1 == formUdata) {
            val = (int64)b.@uint();
        }
        else if (exprᴛ1 == formImplicitConst) {
            val = a.field[i].val;
        }
        else if (exprᴛ1 == formFlag) {
            val = b.uint8() == 1;
        }
        else if (exprᴛ1 == formFlagPresent) {
            val = true;
        }
        else if (exprᴛ1 == formRefAddr) {
            nint versΔ2 = b.format.version();
            if (versΔ2 == 0){
                // block
                // constant
                // flag
                // New in DWARF 4.
                // The attribute is implicitly indicated as present, and no value is
                // encoded in the debugging information entry itself.
                // reference to other entry
                b.error(unknownVersionForDwFormˢ);
            } else 
            if (versΔ2 == 2){
                val = ((Offset)(uint32)b.addr());
            } else {
                var (is64, known) = b.format.dwarf64();
                if (!known){
                    b.error(unknownSizeForDwFormRefˢ);
                } else 
                if (is64){
                    val = ((Offset)(uint32)b.uint64());
                } else {
                    val = ((Offset)b.uint32());
                }
            }
        }
        else if (exprᴛ1 == formRef1) {
            val = ((Offset)(uint32)b.uint8()) + ubase;
        }
        else if (exprᴛ1 == formRef2) {
            val = ((Offset)(uint32)b.uint16()) + ubase;
        }
        else if (exprᴛ1 == formRef4) {
            val = ((Offset)b.uint32()) + ubase;
        }
        else if (exprᴛ1 == formRef8) {
            val = ((Offset)(uint32)b.uint64()) + ubase;
        }
        else if (exprᴛ1 == formRefUdata) {
            val = ((Offset)(uint32)b.@uint()) + ubase;
        }
        else if (exprᴛ1 == formString) {
            val = b.@string();
        }
        else if (exprᴛ1 == formStrp || exprᴛ1 == formLineStrp) {
// string
            uint64 offΔ7 = default!;        // offset into .debug_str
            var (is64, known) = b.format.dwarf64();
            if (!known){
                b.error(unknownSizeForDwFormStrpˢ);
            } else 
            if (is64){
                offΔ7 = b.uint64();
            } else {
                offΔ7 = (uint64)b.uint32();
            }
            if ((uint64)(nint)offΔ7 != offΔ7) {
                b.error(dwFormStrpLineStrpOffsetˢ);
            }
            if (b.err != default!) {
                return default!;
            }
            buf b1 = default!;
            if (fmt == formStrp){
                b1 = makeBuf(b.dwarf, b.format, strˢ, 0, (~b.dwarf).str);
            } else {
                if (len((~b.dwarf).lineStr) == 0) {
                    b.error(dwFormLineStrpWithNoˢ);
                    return default!;
                }
                b1 = makeBuf(b.dwarf, b.format, lineStrˢ, 0, (~b.dwarf).lineStr);
            }
            b1.skip((nint)offΔ7);
            val = b1.@string();
            if (b1.err != default!) {
                b.err = b1.err;
                return default!;
            }
        }
        else if (exprᴛ1 == formStrx || exprᴛ1 == formStrx1 || exprᴛ1 == formStrx2 || exprᴛ1 == formStrx3 || exprᴛ1 == formStrx4) {
            do {
                uint64 offΔ8 = default!;
                var exprᴛ3 = fmt;
                if (exprᴛ3 == formStrx) {
                    offΔ8 = b.@uint();
                }
                else if (exprᴛ3 == formStrx1) {
                    offΔ8 = (uint64)b.uint8();
                }
                else if (exprᴛ3 == formStrx2) {
                    offΔ8 = (uint64)b.uint16();
                }
                else if (exprᴛ3 == formStrx3) {
                    offΔ8 = (uint64)b.uint24();
                }
                else if (exprᴛ3 == formStrx4) {
                    offΔ8 = (uint64)b.uint32();
                }

                if (len((~b.dwarf).strOffsets) == 0) {
                    b.error(dwFormStrxWithNoDebugStrˢ);
                }
                var (is64, known) = b.format.dwarf64();
                if (!known) {
                    b.error(unknownOffsetSizeForDwˢ);
                }
                if (b.err != default!) {
                    return default!;
                }
                if (is64){
                    offΔ8 *= 8;
                } else {
                    offΔ8 *= 4;
                }
                // We have to adjust by the offset of the
                // compilation unit. This won't work if the
                // program uses Reader.Seek to skip over the
                // unit. Not much we can do about that.
                int64 strBase = default!;
                if (Ꮡcu != nil){
                    (strBase, _) = cu.Val(AttrStrOffsetsBase)._<int64>(ᐧ);
                } else 
                if (a.tag == TagCompileUnit) {
                    delay = append(delay, new entry_delayed(i, offΔ8, formStrx));
                    break;
                }
                val = resolveStrx((uint64)strBase, offΔ8);
            } while (false);
        }
        else if (exprᴛ1 == formStrpSup) {
            var (is64, known) = b.format.dwarf64();
            if (!known){
                b.error(unknownSizeForDwFormStrpˢ2);
            } else 
            if (is64){
                val = b.uint64();
            } else {
                val = b.uint32();
            }
        }
        else if (exprᴛ1 == formSecOffset || exprᴛ1 == formGnuRefAlt || exprᴛ1 == formGnuStrpAlt) {
            var (is64, known) = b.format.dwarf64();
            if (!known){
                // lineptr, loclistptr, macptr, rangelistptr
                // New in DWARF 4, but clang can generate them with -gdwarf-2.
                // Section reference, replacing use of formData4 and formData8.
                b.error("unknown size for form 0x"u8 + strconv.FormatInt((int64)(uint32)fmt, 16));
            } else 
            if (is64){
                val = (int64)b.uint64();
            } else {
                val = (int64)b.uint32();
            }
        }
        else if (exprᴛ1 == formExprloc) {
            val = b.bytes((nint)b.@uint());
        }
        else if (exprᴛ1 == formRefSig8) {
            val = b.uint64();
        }
        else if (exprᴛ1 == formRefSup4) {
            val = b.uint32();
        }
        else if (exprᴛ1 == formRefSup8) {
            val = b.uint64();
        }
        else if (exprᴛ1 == formLoclistx) {
            val = b.@uint();
        }
        else if (exprᴛ1 == formRnglistx) {
            do {
                var offΔ9 = b.@uint();
// exprloc
// New in DWARF 4.
// reference
// New in DWARF 4.
// 64-bit type signature.
// loclist
// rnglist

                // We have to adjust by the rnglists_base of
                // the compilation unit. This won't work if
                // the program uses Reader.Seek to skip over
                // the unit. Not much we can do about that.
                int64 rnglistsBase = default!;
                if (Ꮡcu != nil){
                    (rnglistsBase, _) = cu.Val(AttrRnglistsBase)._<int64>(ᐧ);
                } else 
                if (a.tag == TagCompileUnit) {
                    delay = append(delay, new entry_delayed(i, offΔ9, formRnglistx));
                    break;
                }
                val = resolveRnglistx((uint64)rnglistsBase, offΔ9);
            } while (false);
        }
        else { /* default: */
            b.error("unknown entry attr format 0x"u8 + strconv.FormatInt((int64)(uint32)fmt, 16));
        }

        (~e).Field[i].Val = val;
    }
    if (b.err != default!) {
        return default!;
    }
    foreach (var (_, del) in delay) {
        var exprᴛ4 = del.fmt;
        if (exprᴛ4 == formAddrx) {
            var (addrBase, _) = e.Val(AttrAddrBase)._<int64>(ᐧ);
            var (val, err) = b.dwarf.debugAddr(b.format, (uint64)addrBase, del.off);
            if (err != default!) {
                b.err = err;
                return default!;
            }
            (~e).Field[del.idx].Val = val;
        }
        else if (exprᴛ4 == formStrx) {
            var (strBase, _) = e.Val(AttrStrOffsetsBase)._<int64>(ᐧ);
            (~e).Field[del.idx].Val = resolveStrx((uint64)strBase, del.off);
            if (b.err != default!) {
                return default!;
            }
        }
        else if (exprᴛ4 == formRnglistx) {
            var (rnglistsBase, _) = e.Val(AttrRnglistsBase)._<int64>(ᐧ);
            (~e).Field[del.idx].Val = resolveRnglistx((uint64)rnglistsBase, del.off);
            if (b.err != default!) {
                return default!;
            }
        }

    }
    return e;
}

// A Reader allows reading [Entry] structures from a DWARF “info” section.
// The [Entry] structures are arranged in a tree. The [Reader.Next] function
// return successive entries from a pre-order traversal of the tree.
// If an entry has children, its Children field will be true, and the children
// follow, terminated by an [Entry] with [Tag] 0.
[GoType] partial struct ΔReader {
    internal buf b;
    internal ж<Data> d;
    internal error err;
    internal nint unit;
    internal bool lastUnit;   // set if last entry returned by Next is TagCompileUnit/TagPartialUnit
    internal bool lastChildren;   // .Children of last entry returned by Next
    internal Offset lastSibling; // .Val(AttrSibling) of last entry returned by Next
    internal ж<Entry> cu; // current compilation unit
}

// Reader returns a new Reader for [Data].
// The reader is positioned at byte offset 0 in the DWARF “info” section.
public static ж<ΔReader> Reader(this ж<Data> Ꮡd) {
    var r = Ꮡ(new ΔReader(d: Ꮡd));
    r.Seek(0);
    return r;
}

// AddressSize returns the size in bytes of addresses in the current compilation
// unit.
[GoRecv] public static nint AddressSize(this ref ΔReader r) {
    return (~r.d).unit[r.unit].asize;
}

// ByteOrder returns the byte order in the current compilation unit.
[GoRecv] public static binary.ByteOrder ByteOrder(this ref ΔReader r) {
    return r.b.order;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string infoˢ = "info"u8;
internal static readonly @string offsetOutOfRangeˢ = "offset out of range"u8;

// Seek positions the [Reader] at offset off in the encoded entry stream.
// Offset 0 can be used to denote the first entry.
[GoRecv] public static void Seek(this ref ΔReader r, Offset off) {
    var d = r.d;
    r.err = default!;
    r.lastChildren = false;
    if (off == 0) {
        if (len((~d).unit) == 0) {
            return;
        }
        var uΔ1 = Ꮡ((~d).unit, 0);
        r.unit = 0;
        r.b = makeBuf(r.d, new unitжdataFormat(uΔ1), infoˢ, (~uΔ1).off, (~uΔ1).data);
        r.cu = default!;
        return;
    }
    nint i = d.offsetToUnit(off);
    if (i == -1) {
        r.err = errors.New(offsetOutOfRangeˢ);
        return;
    }
    if (i != r.unit) {
        r.cu = default!;
    }
    var u = Ꮡ((~d).unit, i);
    r.unit = i;
    r.b = makeBuf(r.d, new unitжdataFormat(u), infoˢ, off, (~u).data.slice((nint)(uint32)(off - (~u).off)));
}

// maybeNextUnit advances to the next unit if this one is finished.
[GoRecv] internal static void maybeNextUnit(this ref ΔReader r) {
    while (len(r.b.data) == 0 && r.unit + 1 < len((~r.d).unit)) {
        r.nextUnit();
    }
}

// nextUnit advances to the next unit.
[GoRecv] internal static void nextUnit(this ref ΔReader r) {
    r.unit++;
    var u = Ꮡ((~r.d).unit, r.unit);
    r.b = makeBuf(r.d, new unitжdataFormat(u), infoˢ, (~u).off, (~u).data);
    r.cu = default!;
}

// Next reads the next entry from the encoded entry stream.
// It returns nil, nil when it reaches the end of the section.
// It returns an error if the current offset is invalid or the data at the
// offset cannot be decoded as a valid [Entry].
public static (ж<Entry>, error) Next(this ж<ΔReader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.err != default!) {
        return (default!, r.err);
    }
    r.maybeNextUnit();
    if (len(r.b.data) == 0) {
        return (default!, default!);
    }
    var u = Ꮡ((~r.d).unit, r.unit);
    var e = Ꮡr.of(dwarf_package.ΔReader.Ꮡb).entry(r.cu, (~u).atable, (~u).@base, (~u).vers);
    if (r.b.err != default!) {
        r.err = r.b.err;
        return (default!, r.err);
    }
    r.lastUnit = false;
    if (e != nil){
        r.lastChildren = e.Value.Children;
        if (r.lastChildren) {
            (r.lastSibling, _) = e.Val(AttrSibling)._<Offset>(ᐧ);
        }
        if ((~e).Tag == TagCompileUnit || (~e).Tag == TagPartialUnit) {
            r.lastUnit = true;
            r.cu = e;
        }
    } else {
        r.lastChildren = false;
    }
    return (e, default!);
}

// SkipChildren skips over the child entries associated with
// the last [Entry] returned by [Reader.Next]. If that [Entry] did not have
// children or [Reader.Next] has not been called, SkipChildren is a no-op.
public static void SkipChildren(this ж<ΔReader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.err != default! || !r.lastChildren) {
        return;
    }
    // If the last entry had a sibling attribute,
    // that attribute gives the offset of the next
    // sibling, so we can avoid decoding the
    // child subtrees.
    if (r.lastSibling >= r.b.off) {
        r.Seek(r.lastSibling);
        return;
    }
    if (r.lastUnit && r.unit + 1 < len((~r.d).unit)) {
        r.nextUnit();
        return;
    }
    while (ᐧ) {
        var (e, err) = Ꮡr.Next();
        if (err != default! || e == nil || (~e).Tag == 0) {
            break;
        }
        if ((~e).Children) {
            Ꮡr.SkipChildren();
        }
    }
}

// clone returns a copy of the reader. This is used by the typeReader
// interface.
[GoRecv] internal static typeReader clone(this ref ΔReader r) {
    return new ΔReaderжtypeReader(r.d.Reader());
}

// offset returns the current buffer offset. This is used by the
// typeReader interface.
[GoRecv] internal static Offset offset(this ref ΔReader r) {
    return r.b.off;
}

// SeekPC returns the [Entry] for the compilation unit that includes pc,
// and positions the reader to read the children of that unit.  If pc
// is not covered by any unit, SeekPC returns [ErrUnknownPC] and the
// position of the reader is undefined.
//
// Because compilation units can describe multiple regions of the
// executable, in the worst case SeekPC must search through all the
// ranges in all the compilation units. Each call to SeekPC starts the
// search at the compilation unit of the last call, so in general
// looking up a series of PCs will be faster if they are sorted. If
// the caller wishes to do repeated fast PC lookups, it should build
// an appropriate index using the Ranges method.
public static (ж<Entry>, error) SeekPC(this ж<ΔReader> Ꮡr, uint64 pc) {
    ref var r = ref Ꮡr.DerefOrNull();

    nint unit = r.unit;
    for (nint i = 0; i < len((~r.d).unit); i++) {
        if (unit >= len((~r.d).unit)) {
            unit = 0;
        }
        r.err = default!;
        r.lastChildren = false;
        r.unit = unit;
        r.cu = default!;
        var u = Ꮡ((~r.d).unit, unit);
        r.b = makeBuf(r.d, new unitжdataFormat(u), infoˢ, (~u).off, (~u).data);
        var (e, err) = Ꮡr.Next();
        if (err != default!) {
            return (default!, err);
        }
        if (e == nil || (~e).Tag == 0) {
            return (default!, ErrUnknownPC);
        }
        (var ranges, err) = r.d.Ranges(e);
        if (err != default!) {
            return (default!, err);
        }
        foreach (var (_, vᴛ1) in ranges) {
            var pcs = vᴛ1.Clone();

            if (pcs[0] <= pc && pc < pcs[1]) {
                return (e, default!);
            }
        }
        unit++;
    }
    return (default!, ErrUnknownPC);
}

// Ranges returns the PC ranges covered by e, a slice of [low,high) pairs.
// Only some entry types, such as [TagCompileUnit] or [TagSubprogram], have PC
// ranges; for others, this will return nil with no error.
public static (slice<array<uint64>>, error) Ranges(this ж<Data> Ꮡd, ж<Entry> Ꮡe) {
    ref var d = ref Ꮡd.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    slice<array<uint64>> ret = default!;
    var (low, lowOK) = e.Val(AttrLowpc)._<uint64>(ᐧ);
    uint64 high = default!;
    bool highOK = default!;
    var highField = e.AttrField(AttrHighpc);
    if (highField != nil) {
        var exprᴛ1 = (~highField).Class;
        if (exprᴛ1 == ClassAddress) {
            (high, highOK) = (~highField).Val._<uint64>(ᐧ);
        }
        else if (exprᴛ1 == ClassConstant) {
            var (off, ok) = (~highField).Val._<int64>(ᐧ);
            if (ok) {
                high = low + (uint64)off;
                highOK = true;
            }
        }

    }
    if (lowOK && highOK) {
        ret = append(ret, new uint64[]{low, high}.array());
    }
    ж<unit> u = default!;
    {
        nint uidx = Ꮡd.offsetToUnit(e.Offset); if (uidx >= 0 && uidx < len(d.unit)) {
            u = Ꮡ(d.unit, uidx);
        }
    }
    if (u != nil && (~u).vers >= 5 && d.rngLists != default!) {
        // DWARF version 5 and later
        var field = e.AttrField(AttrRanges);
        if (field == nil) {
            return (ret, default!);
        }
        var exprᴛ2 = (~field).Class;
        if (exprᴛ2 == ClassRangeListPtr) {
            var (rangesΔ2, rangesOKΔ2) = (~field).Val._<int64>(ᐧ);
            if (!rangesOKΔ2) {
                return (ret, default!);
            }
            var (cu, @base, err) = Ꮡd.baseAddressForEntry(Ꮡe);
            if (err != default!) {
                return (default!, err);
            }
            return Ꮡd.dwarf5Ranges(u, cu, @base, rangesΔ2, ret);
        }
        if (exprᴛ2 == ClassRngList) {
            var (rnglist, ok) = (~field).Val._<uint64>(ᐧ);
            if (!ok) {
                return (ret, default!);
            }
            var (cu, @base, err) = Ꮡd.baseAddressForEntry(Ꮡe);
            if (err != default!) {
                return (default!, err);
            }
            return Ꮡd.dwarf5Ranges(u, cu, @base, (int64)rnglist, ret);
        }
        { /* default: */
            return (ret, default!);
        }

    }
    // DWARF version 2 through 4
    var (ranges, rangesOK) = e.Val(AttrRanges)._<int64>(ᐧ);
    if (rangesOK && d.ranges != default!) {
        var (_, @base, err) = Ꮡd.baseAddressForEntry(Ꮡe);
        if (err != default!) {
            return (default!, err);
        }
        return Ꮡd.dwarf2Ranges(u, @base, ranges, ret);
    }
    return (ret, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noUnitForEntryˢ = "no unit for entry"u8;

// baseAddressForEntry returns the initial base address to be used when
// looking up the range list of entry e.
// DWARF specifies that this should be the lowpc attribute of the enclosing
// compilation unit, however comments in gdb/dwarf2read.c say that some
// versions of GCC use the entrypc attribute, so we check that too.
internal static (ж<Entry>, uint64, error) baseAddressForEntry(this ж<Data> Ꮡd, ж<Entry> Ꮡe) {
    ref var d = ref Ꮡd.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    ж<Entry> cu = default!;
    if (e.Tag == TagCompileUnit){
        cu = Ꮡe;
    } else {
        nint i = Ꮡd.offsetToUnit(e.Offset);
        if (i == -1) {
            return (default!, 0, errors.New(noUnitForEntryˢ));
        }
        var u = Ꮡ(d.unit, i);
        ref var b = ref heap<buf>(out var Ꮡb);
        b = makeBuf(Ꮡd, new unitжdataFormat(u), infoˢ, (~u).off, (~u).data);
        cu = Ꮡb.entry(nil, (~u).atable, (~u).@base, (~u).vers);
        if (b.err != default!) {
            return (default!, 0, b.err);
        }
    }
    {
        var (cuEntry, cuEntryOK) = cu.Val(AttrEntrypc)._<uint64>(ᐧ); if (cuEntryOK){
            return (cu, cuEntry, default!);
        } else 
        {
            var (cuLow, cuLowOK) = cu.Val(AttrLowpc)._<uint64>(ᐧ); if (cuLowOK) {
                return (cu, cuLow, default!);
            }
        }
    }
    return (cu, 0, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rangesˢ = "ranges"u8;

internal static (slice<array<uint64>>, error) dwarf2Ranges(this ж<Data> Ꮡd, ж<unit> Ꮡu, uint64 @base, int64 ranges, slice<array<uint64>> ret) {
    ref var d = ref Ꮡd.DerefOrNull();
    ref var u = ref Ꮡu.DerefOrNull();

    if (ranges < 0 || ranges > (int64)len(d.ranges)) {
        return (default!, fmt.Errorf("invalid range offset %d (max %d)"u8, ranges, len(d.ranges)));
    }
    var buf = makeBuf(Ꮡd, new unitжdataFormat(Ꮡu), rangesˢ, ((Offset)(uint32)ranges), d.ranges.slice((nint)(ranges)));
    while (len(buf.data) > 0) {
        var low = buf.addr();
        var high = buf.addr();
        if (low == 0 && high == 0) {
            break;
        }
        if (low == (~(uint64)0).Rsh((nuint)((8 - u.addrsize()) * 8))){
            @base = high;
        } else {
            ret = append(ret, new uint64[]{@base + low, @base + high}.array());
        }
    }
    return (ret, default!);
}

// dwarf5Ranges interprets a debug_rnglists sequence, see DWARFv5 section
// 2.17.3 (page 53).
internal static (slice<array<uint64>>, error) dwarf5Ranges(this ж<Data> Ꮡd, ж<unit> Ꮡu, ж<Entry> Ꮡcu, uint64 @base, int64 ranges, slice<array<uint64>> ret) {
    ref var d = ref Ꮡd.DerefOrNull();
    ref var cu = ref Ꮡcu.DerefOrNull();

    if (ranges < 0 || ranges > (int64)len(d.rngLists)) {
        return (default!, fmt.Errorf("invalid rnglist offset %d (max %d)"u8, ranges, len(d.ranges)));
    }
    int64 addrBase = default!;
    if (Ꮡcu != nil) {
        (addrBase, _) = cu.Val(AttrAddrBase)._<int64>(ᐧ);
    }
    var buf = makeBuf(Ꮡd, new unitжdataFormat(Ꮡu), rnglistsˢ, 0, d.rngLists);
    buf.skip((nint)ranges);
    while (ᐧ) {
        var opcode = buf.uint8();
        var exprᴛ1 = opcode;
        if (exprᴛ1 == rleEndOfList) {
            if (buf.err != default!) {
                return (default!, buf.err);
            }
            return (ret, default!);
        }
        if (exprᴛ1 == rleBaseAddressx) {
            var baseIdx = buf.@uint();
            error err = default!;
            (@base, err) = Ꮡd.debugAddr(new unitжdataFormat(Ꮡu), (uint64)addrBase, baseIdx);
            if (err != default!) {
                return (default!, err);
            }
        }
        else if (exprᴛ1 == rleStartxEndx) {
            var startIdx = buf.@uint();
            var endIdx = buf.@uint();
            var (start, err) = Ꮡd.debugAddr(new unitжdataFormat(Ꮡu), (uint64)addrBase, startIdx);
            if (err != default!) {
                return (default!, err);
            }
            (var end, err) = Ꮡd.debugAddr(new unitжdataFormat(Ꮡu), (uint64)addrBase, endIdx);
            if (err != default!) {
                return (default!, err);
            }
            ret = append(ret, new uint64[]{start, end}.array());
        }
        else if (exprᴛ1 == rleStartxLength) {
            var startIdx = buf.@uint();
            var lenΔ2 = buf.@uint();
            var (start, err) = Ꮡd.debugAddr(new unitжdataFormat(Ꮡu), (uint64)addrBase, startIdx);
            if (err != default!) {
                return (default!, err);
            }
            ret = append(ret, new uint64[]{start, start + lenΔ2}.array());
        }
        else if (exprᴛ1 == rleOffsetPair) {
            var off1 = buf.@uint();
            var off2 = buf.@uint();
            ret = append(ret, new uint64[]{@base + off1, @base + off2}.array());
        }
        else if (exprᴛ1 == rleBaseAddress) {
            @base = buf.addr();
        }
        else if (exprᴛ1 == rleStartEnd) {
            var start = buf.addr();
            var end = buf.addr();
            ret = append(ret, new uint64[]{start, end}.array());
        }
        else if (exprᴛ1 == rleStartLength) {
            var start = buf.addr();
            var lenΔ3 = buf.@uint();
            ret = append(ret, new uint64[]{start, start + lenΔ3}.array());
        }

    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addrˢ = "addr"u8;

// debugAddr returns the address at idx in debug_addr
internal static (uint64, error) debugAddr(this ж<Data> Ꮡd, dataFormat format, uint64 addrBase, uint64 idx) {
    ref var d = ref Ꮡd.DerefOrNull();

    var off = idx * (uint64)format.addrsize() + addrBase;
    if ((uint64)(nint)off != off) {
        return (0, errors.New(offsetOutOfRangeˢ));
    }
    var b = makeBuf(Ꮡd, format, addrˢ, 0, d.addr);
    b.skip((nint)off);
    var val = b.addr();
    if (b.err != default!) {
        return (0, b.err);
    }
    return (val, default!);
}

} // end dwarf_package
