// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.net.http2;

using io = io_package;

partial class hpack_package {

internal const uint32 uint32Max = /* ^uint32(0) */ 4294967295;
internal static UntypedInt initialHeaderTableSize => 4096;

partial struct Encoder {
    internal dynamicTable dynTab;
    // minSize is the minimum table size set by
    // SetMaxDynamicTableSize after the previous Header Table Size
    // Update.
    internal uint32 minSize;
    // maxSizeLimit is the maximum table size this encoder
    // supports. This will protect the encoder from too large
    // size.
    internal uint32 maxSizeLimit;
    // tableSizeUpdate indicates whether "Header Table Size
    // Update" is required.
    internal bool tableSizeUpdate;
    internal io.Writer w;
    internal slice<byte> buf;
}

// NewEncoder returns a new Encoder which performs HPACK encoding. An
// encoded data is written to w.
public static ж<Encoder> NewEncoder(io.Writer w) {
    var e = Ꮡ(new Encoder(
        minSize: uint32Max,
        maxSizeLimit: initialHeaderTableSize,
        tableSizeUpdate: false,
        w: w
    ));
    e.of(Encoder.ᏑdynTab).of(dynamicTable.Ꮡtable).init();
    e.of(Encoder.ᏑdynTab).setMaxSize(initialHeaderTableSize);
    return e;
}

// WriteField encodes f into a single Write to e's underlying Writer.
// This function may also produce bytes for "Header Table Size Update"
// if necessary. If produced, it is done before encoding f.
public static error WriteField(this ж<Encoder> Ꮡe, HeaderField f) {
    ref var e = ref Ꮡe.DerefOrNull();

    e.buf = e.buf[..0];
    if (e.tableSizeUpdate) {
        e.tableSizeUpdate = false;
        if (e.minSize < e.dynTab.maxSize) {
            e.buf = appendTableSize(e.buf, e.minSize);
        }
        e.minSize = uint32Max;
        e.buf = appendTableSize(e.buf, e.dynTab.maxSize);
    }
    var (idx, nameValueMatch) = Ꮡe.searchTable(f);
    if (nameValueMatch){
        e.buf = appendIndexed(e.buf, idx);
    } else {
        var indexing = e.shouldIndex(f);
        if (indexing) {
            e.dynTab.add(f);
        }
        if (idx == 0){
            e.buf = appendNewName(e.buf, f, indexing);
        } else {
            e.buf = appendIndexedName(e.buf, f, idx, indexing);
        }
    }
    var (n, err) = e.w.Write(e.buf);
    if (err == default! && n != builtin.len(e.buf)) {
        err = io.ErrShortWrite;
    }
    return err;
}

// searchTable searches f in both stable and dynamic header tables.
// The static header table is searched first. Only when there is no
// exact match for both name and value, the dynamic header table is
// then searched. If there is no match, i is 0. If both name and value
// match, i is the matched index and nameValueMatch becomes true. If
// only name matches, i points to that index and nameValueMatch
// becomes false.
internal static (uint64 i, bool nameValueMatch) searchTable(this ж<Encoder> Ꮡe, HeaderField f) {
    uint64 i = default!;
    bool nameValueMatch = default!;

    (i, nameValueMatch) = staticTable.search(f);
    if (nameValueMatch) {
        return (i, true);
    }
    (var j, nameValueMatch) = Ꮡe.of(Encoder.ᏑdynTab).of(dynamicTable.Ꮡtable).search(f);
    if (nameValueMatch || (i == 0 && j != 0)) {
        return (j + (uint64)staticTable.len(), nameValueMatch);
    }
    return (i, false);
}

// SetMaxDynamicTableSize changes the dynamic header table size to v.
// The actual size is bounded by the value passed to
// SetMaxDynamicTableSizeLimit.
public static void SetMaxDynamicTableSize(this ref Encoder e, uint32 v) {
    if (v > e.maxSizeLimit) {
        v = e.maxSizeLimit;
    }
    if (v < e.minSize) {
        e.minSize = v;
    }
    e.tableSizeUpdate = true;
    e.dynTab.setMaxSize(v);
}

// MaxDynamicTableSize returns the current dynamic header table size.
public static uint32 /*v*/ MaxDynamicTableSize(this ref Encoder e) {
    return e.dynTab.maxSize;
}

// SetMaxDynamicTableSizeLimit changes the maximum value that can be
// specified in SetMaxDynamicTableSize to v. By default, it is set to
// 4096, which is the same size of the default dynamic header table
// size described in HPACK specification. If the current maximum
// dynamic header table size is strictly greater than v, "Header Table
// Size Update" will be done in the next WriteField call and the
// maximum dynamic header table size is truncated to v.
public static void SetMaxDynamicTableSizeLimit(this ref Encoder e, uint32 v) {
    e.maxSizeLimit = v;
    if (e.dynTab.maxSize > v) {
        e.tableSizeUpdate = true;
        e.dynTab.setMaxSize(v);
    }
}

// shouldIndex reports whether f should be indexed.
internal static bool shouldIndex(this ref Encoder e, HeaderField f) {
    return !f.Sensitive && f.Size() <= e.dynTab.maxSize;
}

// appendIndexed appends index i, as encoded in "Indexed Header Field"
// representation, to dst and returns the extended buffer.
internal static slice<byte> appendIndexed(slice<byte> dst, uint64 i) {
    nint first = builtin.len(dst);
    dst = appendVarInt(dst, 7, i);
    dst[first] |= (byte)(0x80);
    return dst;
}

// appendNewName appends f, as encoded in one of "Literal Header field
// - New Name" representation variants, to dst and returns the
// extended buffer.
//
// If f.Sensitive is true, "Never Indexed" representation is used. If
// f.Sensitive is false and indexing is true, "Incremental Indexing"
// representation is used.
internal static slice<byte> appendNewName(slice<byte> dst, HeaderField f, bool indexing) {
    dst = append(dst, encodeTypeByte(indexing, f.Sensitive));
    dst = appendHpackString(dst, f.Name);
    return appendHpackString(dst, f.Value);
}

// appendIndexedName appends f and index i referring indexed name
// entry, as encoded in one of "Literal Header field - Indexed Name"
// representation variants, to dst and returns the extended buffer.
//
// If f.Sensitive is true, "Never Indexed" representation is used. If
// f.Sensitive is false and indexing is true, "Incremental Indexing"
// representation is used.
internal static slice<byte> appendIndexedName(slice<byte> dst, HeaderField f, uint64 i, bool indexing) {
    nint first = builtin.len(dst);
    byte n = default!;
    if (indexing){
        n = 6;
    } else {
        n = 4;
    }
    dst = appendVarInt(dst, n, i);
    dst[first] |= (byte)(encodeTypeByte(indexing, f.Sensitive));
    return appendHpackString(dst, f.Value);
}

// appendTableSize appends v, as encoded in "Header Table Size Update"
// representation, to dst and returns the extended buffer.
internal static slice<byte> appendTableSize(slice<byte> dst, uint32 v) {
    nint first = builtin.len(dst);
    dst = appendVarInt(dst, 5, (uint64)v);
    dst[first] |= (byte)(0x20);
    return dst;
}

// appendVarInt appends i, as encoded in variable integer form using n
// bit prefix, to dst and returns the extended buffer.
//
// See
// https://httpwg.org/specs/rfc7541.html#integer.representation
internal static slice<byte> appendVarInt(slice<byte> dst, byte n, uint64 i) {
    var k = (uint64)((((uint64)1).Lsh((uint64)(n))) - 1);
    if (i < k) {
        return append(dst, (byte)i);
    }
    dst = append(dst, (byte)k);
    i -= k;
    for (; i >= 128; i >>= (int)(7)) {
        dst = append(dst, (byte)((uint64)(0x80 | ((uint64)(i & 0x7f)))));
    }
    return append(dst, (byte)i);
}

// appendHpackString appends s, as encoded in "String Literal"
// representation, to dst and returns the extended buffer.
//
// s will be encoded in Huffman codes only when it produces strictly
// shorter byte string.
internal static slice<byte> appendHpackString(slice<byte> dst, @string s) {
    var huffmanLength = HuffmanEncodeLength(s);
    if (huffmanLength < (uint64)builtin.len(s)){
        nint first = builtin.len(dst);
        dst = appendVarInt(dst, 7, huffmanLength);
        dst = AppendHuffmanString(dst, s);
        dst[first] |= (byte)(0x80);
    } else {
        dst = appendVarInt(dst, 7, (uint64)builtin.len(s));
        dst = append(dst, s.ꓸꓸꓸ);
    }
    return dst;
}

// encodeTypeByte returns type byte. If sensitive is true, type byte
// for "Never Indexed" representation is returned. If sensitive is
// false and indexing is true, type byte for "Incremental Indexing"
// representation is returned. Otherwise, type byte for "Without
// Indexing" is returned.
internal static byte encodeTypeByte(bool indexing, bool sensitive) {
    if (sensitive) {
        return 0x10;
    }
    if (indexing) {
        return 0x40;
    }
    return 0;
}

} // end hpack_package
