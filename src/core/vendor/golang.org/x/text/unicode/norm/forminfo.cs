// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using binary = encoding.binary_package;
using encoding;

partial class norm_package {

// This file contains Form-specific logic and wrappers for data in tables.go.
// Rune info is stored in a separate trie per composing form. A composing form
// and its corresponding decomposing form share the same trie.  Each trie maps
// a rune to a uint16. The values take two forms.  For v >= 0x8000:
//   bits
//   15:    1 (inverse of NFD_QC bit of qcInfo)
//   13..7: qcInfo (see below). isYesD is always true (no decomposition).
//    6..0: ccc (compressed CCC value).
// For v < 0x8000, the respective rune has a decomposition and v is an index
// into a byte array of UTF-8 decomposition sequences and additional info and
// has the form:
//    <header> <decomp_byte>* [<tccc> [<lccc>]]
// The header contains the number of bytes in the decomposition (excluding this
// length byte). The two most significant bits of this length byte correspond
// to bit 5 and 4 of qcInfo (see below).  The byte sequence itself starts at v+1.
// The byte sequence is followed by a trailing and leading CCC if the values
// for these are not zero.  The value of v determines which ccc are appended
// to the sequences.  For v < firstCCC, there are none, for v >= firstCCC,
// the sequence is followed by a trailing ccc, and for v >= firstLeadingCC
// there is an additional leading ccc. The value of tccc itself is the
// trailing CCC shifted left 2 bits. The two least-significant bits of tccc
// are the number of trailing non-starters.
internal static UntypedInt qcInfoMask => 0x3F; // to clear all but the relevant bits in a qcInfo
internal static UntypedInt headerLenMask => 0x3F; // extract the length value from the header byte
internal static UntypedInt headerFlagsMask => 0xC0; // extract the qcInfo bits from the header byte

// Properties provides access to normalization properties of a rune.
[GoType] partial struct ΔProperties {
    internal uint8 pos;  // start position in reorderBuffer; used in composition.go
    internal uint8 size;  // length of UTF-8 encoding of this rune
    internal uint8 ccc;  // leading canonical combining class (ccc if not decomposition)
    internal uint8 tccc;  // trailing canonical combining class (ccc if not decomposition)
    internal uint8 nLead;  // number of leading non-starters.
    internal qcInfo flags; // quick check flags
    internal uint16 index;
}

// type lookupFunc is a methodless func type — rendered inline as its base delegate

// formInfo holds Form-specific functions and tables.
[GoType] partial struct formInfo {
    internal Form form;
    internal bool composing, compatibility; // form type
    internal Func<input, nint, ΔProperties> info;
    internal Func<ж<Iter>, slice<byte>> nextMain;
}

internal static slice<ж<formInfo>> formTable;
internal static void initᴛformTable() { formTable = new ж<formInfo>[]{Ꮡ(new formInfo(
    form: NFC,
    composing: true,
    compatibility: false,
    info: lookupInfoNFC,
    nextMain: nextComposed)), Ꮡ(new formInfo(
    form: NFD,
    composing: false,
    compatibility: false,
    info: lookupInfoNFC,
    nextMain: nextDecomposed)), Ꮡ(new formInfo(
    form: NFKC,
    composing: true,
    compatibility: true,
    info: lookupInfoNFKC,
    nextMain: nextComposed)), Ꮡ(new formInfo(
    form: NFKD,
    composing: false,
    compatibility: true,
    info: lookupInfoNFKC,
    nextMain: nextDecomposed))
}.slice(); }

// We do not distinguish between boundaries for NFC, NFD, etc. to avoid
// unexpected behavior for the user.  For example, in NFD, there is a boundary
// after 'a'.  However, 'a' might combine with modifiers, so from the application's
// perspective it is not a good boundary. We will therefore always use the
// boundaries for the combining variants.

// BoundaryBefore returns true if this rune starts a new segment and
// cannot combine with any rune on the left.
public static bool BoundaryBefore(this ΔProperties p) {
    if (p.ccc == 0 && !p.combinesBackward()) {
        return true;
    }
    // We assume that the CCC of the first character in a decomposition
    // is always non-zero if different from info.ccc and that we can return
    // false at this point. This is verified by maketables.
    return false;
}

// BoundaryAfter returns true if runes cannot combine with or otherwise
// interact with this or previous runes.
public static bool BoundaryAfter(this ΔProperties p) {
    // TODO: loosen these conditions.
    return p.isInert();
}

[GoType("num:uint8")] partial struct qcInfo;

internal static bool isYesC(this ΔProperties p) {
    return (qcInfo)(p.flags & 0x10) == 0;
}

internal static bool isYesD(this ΔProperties p) {
    return (qcInfo)(p.flags & 0x4) == 0;
}

internal static bool combinesForward(this ΔProperties p) {
    return (qcInfo)(p.flags & 0x20) != 0;
}

internal static bool combinesBackward(this ΔProperties p) {
    return (qcInfo)(p.flags & 0x8) != 0; // == isMaybe
}

internal static bool hasDecomposition(this ΔProperties p) {
    return (qcInfo)(p.flags & 0x4) != 0; // == isNoD
}

internal static bool isInert(this ΔProperties p) {
    return (qcInfo)(p.flags & (uint8)qcInfoMask) == 0 && p.ccc == 0;
}

internal static bool multiSegment(this ΔProperties p) {
    return p.index >= firstMulti && p.index < endMulti;
}

internal static uint8 nLeadingNonStarters(this ΔProperties p) {
    return p.nLead;
}

internal static uint8 nTrailingNonStarters(this ΔProperties p) {
    return (uint8)((qcInfo)(p.flags & 0x03));
}

// Decomposition returns the decomposition for the underlying rune
// or nil if there is none.
public static slice<byte> Decomposition(this ΔProperties p) {
    // TODO: create the decomposition for Hangul?
    if (p.index == 0) {
        return default!;
    }
    var i = p.index;
    var n = (byte)(decomps[i] & (byte)headerLenMask);
    i++;
    return decomps.slice(i, (uint16)(i + (uint16)n));
}

// Size returns the length of UTF-8 encoding of the rune.
public static nint Size(this ΔProperties p) {
    return (nint)p.size;
}

// CCC returns the canonical combining class of the underlying rune.
public static uint8 CCC(this ΔProperties p) {
    if (p.index >= firstCCCZeroExcept) {
        return 0;
    }
    return ccc[p.ccc];
}

// LeadCCC returns the CCC of the first rune in the decomposition.
// If there is no decomposition, LeadCCC equals CCC.
public static uint8 LeadCCC(this ΔProperties p) {
    return ccc[p.ccc];
}

// TrailCCC returns the CCC of the last rune in the decomposition.
// If there is no decomposition, TrailCCC equals CCC.
public static uint8 TrailCCC(this ΔProperties p) {
    return ccc[p.tccc];
}

internal static void buildRecompMap() {
    recompMap = new map<uint32, rune>(len(recompMapPacked) / 8);
    array<byte> buf = new(8);
    for (nint i = 0; i < len(recompMapPacked); i += 8) {
        copy(buf[..], recompMapPacked.slice(i, i + 8));
        var key = binary.BigEndian.Uint32(buf[..4]);
        var val = binary.BigEndian.Uint32(buf[4..]);
        recompMap[key] = (rune)val;
    }
}

// Recomposition
// We use 32-bit keys instead of 64-bit for the two codepoint keys.
// This clips off the bits of three entries, but we know this will not
// result in a collision. In the unlikely event that changes to
// UnicodeData.txt introduce collisions, the compiler will catch it.
// Note that the recomposition map for NFC and NFKC are identical.

// combine returns the combined rune or 0 if it doesn't exist.
//
// The caller is responsible for calling
// recompMapOnce.Do(buildRecompMap) sometime before this is called.
internal static rune combine(rune a, rune b) {
    var key = ((uint32)(uint16)a << (int)(16)) + (uint32)(uint16)b;
    if (recompMap == default!) {
        throw panic("caller error"); // see func comment
    }
    return recompMap[key];
}

internal static ΔProperties lookupInfoNFC(input b, nint i) {
    var (v, sz) = b.charinfoNFC(i);
    return compInfo(v, sz);
}

internal static ΔProperties lookupInfoNFKC(input b, nint i) {
    var (v, sz) = b.charinfoNFKC(i);
    return compInfo(v, sz);
}

// Properties returns properties for the first rune in s.
public static ΔProperties Properties(this Form f, slice<byte> s) {
    if (f == NFC || f == NFD) {
        var (ᴛ1, ᴛ2) = nfcData.lookup(s);
        return compInfo(ᴛ1, ᴛ2);
    }
    var (ᴛ3, ᴛ4) = nfkcData.lookup(s);
    return compInfo(ᴛ3, ᴛ4);
}

// PropertiesString returns properties for the first rune in s.
public static ΔProperties PropertiesString(this Form f, @string s) {
    if (f == NFC || f == NFD) {
        var (ᴛ5, ᴛ6) = nfcData.lookupString(s);
        return compInfo(ᴛ5, ᴛ6);
    }
    var (ᴛ7, ᴛ8) = nfkcData.lookupString(s);
    return compInfo(ᴛ7, ᴛ8);
}

// compInfo converts the information contained in v and sz
// to a Properties.  See the comment at the top of the file
// for more information on the format.
internal static ΔProperties compInfo(uint16 v, nint sz) {
    if (v == 0){
        return new ΔProperties(size: (uint8)sz);
    } else 
    if (v >= 0x8000) {
        var pΔ1 = new ΔProperties(
            size: (uint8)sz,
            ccc: (uint8)v,
            tccc: (uint8)v,
            flags: ((qcInfo)(uint8)((v >> (int)(8))))
        );
        if (pΔ1.ccc > 0 || pΔ1.combinesBackward()) {
            pΔ1.nLead = (uint8)((qcInfo)(pΔ1.flags & 0x3));
        }
        return pΔ1;
    }
    // has decomposition
    var h = decomps[v];
    var f = (qcInfo)(((((qcInfo)((byte)(h & (byte)headerFlagsMask))) >> (int)(2))) | 0x4);
    var p = new ΔProperties(size: (uint8)sz, flags: f, index: v);
    if (v >= firstCCC) {
        v += (uint16)((uint16)((byte)(h & (byte)headerLenMask)) + 1);
        var c = decomps[v];
        p.tccc = (uint8)((c >> (int)(2)));
        p.flags |= (qcInfo)(((qcInfo)((byte)(c & 0x3))));
        if (v >= firstLeadingCCC) {
            p.nLead = (uint8)((byte)(c & 0x3));
            if (v >= firstStarterWithNLead) {
                // We were tricked. Remove the decomposition.
                p.flags &= (qcInfo)(0x03);
                p.index = 0;
                return p;
            }
            p.ccc = decomps[(uint16)(v + 1)];
        }
    }
    return p;
}

} // end norm_package
