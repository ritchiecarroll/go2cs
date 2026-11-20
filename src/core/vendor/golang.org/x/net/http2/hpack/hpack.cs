// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package hpack implements HPACK, a compression format for
// efficiently representing HTTP header fields in the context of HTTP/2.
//
// See http://tools.ietf.org/html/draft-ietf-httpbis-header-compression-09
namespace go.vendor.golang.org.x.net.http2;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using static hpack_package;

partial class hpack_package {

// A DecodingError is something the spec defines as a decoding error.
partial struct DecodingError {
    public error Err;
}

} partial class hpack_package {

internal static @string Errorᴍ(DecodingError de) {
    return fmt.Sprintf("decoding error: %v"u8, de.Err);
}

} partial class hpack_package {

// An InvalidIndexError is returned when an encoder references a table
// entry before the static table or after the end of the dynamic table.
partial struct InvalidIndexError /*num:nint*/;

} partial class hpack_package {

internal static @string Errorᴍ(InvalidIndexError e) {
    return fmt.Sprintf("invalid indexed representation index %d"u8, (nint)e);
}

} partial class hpack_package {

// A HeaderField is a name-value pair. Both the name and value are
// treated as opaque sequences of octets.
partial struct HeaderField {
    public @string Name, Value;
    // Sensitive means that this header field should never be
    // indexed.
    public bool Sensitive;
}

} partial class hpack_package {

// IsPseudo reports whether the header field is an http2 pseudo header.
// That is, it reports whether it starts with a colon.
// It is not otherwise guaranteed to be a valid pseudo header field,
// though.
internal static bool IsPseudoᴍ(HeaderField hf) {
    return len(hf.Name) != 0 && hf.Name[0] == ':';
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string sensitiveˢᴍHeaderField => ref sensitiveˢᵇᴍHeaderField;
private static readonly @string sensitiveˢᵇᴍHeaderField = " (sensitive)"u8;

internal static @string Stringᴍ(HeaderField hf) {
    @string suffix = default;
    if (hf.Sensitive) {
        suffix = sensitiveˢᴍHeaderField;
    }
    return fmt.Sprintf("header field %q = %q%s"u8, hf.Name, hf.Value, suffix);
}

// Size returns the size of an entry per RFC 7541 section 4.1.
internal static uint32 Sizeᴍ(HeaderField hf) {
    // https://httpwg.org/specs/rfc7541.html#rfc.section.4.1
    // "The size of the dynamic table is the sum of the size of
    // its entries. The size of an entry is the sum of its name's
    // length in octets (as defined in Section 5.2), its value's
    // length in octets (see Section 5.2), plus 32.  The size of
    // an entry is calculated using the length of the name and
    // value without any Huffman encoding applied."

    // This can overflow if somebody makes a large HeaderField
    // Name and/or Value by hand, but we don't care, because that
    // won't happen on the wire because the encoding doesn't allow
    // it.
    return (uint32)(len(hf.Name) + len(hf.Value) + 32);
}

} partial class hpack_package {

// A Decoder is the decoding context for incremental processing of
// header blocks.
partial struct Decoder {
    internal dynamicTable dynTab;
    internal Action<HeaderField> emit;
    internal bool emitEnabled; // whether calls to emit are enabled
    internal nint maxStrLen;   // 0 means unlimited
    // buf is the unparsed buffer. It's only written to
    // saveBuf if it was truncated in the middle of a header
    // block. Because it's usually not owned, we can only
    // process it under Write.
    internal slice<byte> buf; // not owned; only valid during Write
    // saveBuf is previous data passed to Write which we weren't able
    // to fully parse before. Unlike buf, we own this data.
    internal bytes.Buffer saveBuf;
    internal bool firstField; // processing the first field of the header block
}

// NewDecoder returns a new decoder with the provided maximum dynamic
// table size. The emitFunc will be called for each valid field
// parsed, in the same goroutine as calls to Write, before Write returns.
public static ж<Decoder> NewDecoder(uint32 maxDynamicTableSize, Action<HeaderField> emitFunc) {
    var d = Ꮡ(new Decoder(
        emit: emitFunc,
        emitEnabled: true,
        firstField: true
    ));
    d.ᏑdynTab.Ꮡtable.init();
    d.dynTab.allowedMaxSize = maxDynamicTableSize;
    d.ᏑdynTab.setMaxSize(maxDynamicTableSize);
    return d;
}

// ErrStringLength is returned by Decoder.Write when the max string length
// (as configured by Decoder.SetMaxStringLength) would be violated.
public static error ErrStringLength = errors.New("hpack: string too long");

} partial class hpack_package {

// SetMaxStringLength sets the maximum size of a HeaderField name or
// value string. If a string exceeds this length (even after any
// decompression), Write will return ErrStringLength.
// A value of 0 means unlimited and is the default from NewDecoder.
internal static void SetMaxStringLengthᴍ(ref Decoder d, nint n) {
    d.maxStrLen = n;
}

// SetEmitFunc changes the callback used when new header fields
// are decoded.
// It must be non-nil. It does not affect EmitEnabled.
internal static void SetEmitFuncᴍ(ref Decoder d, Action<HeaderField> emitFunc) {
    d.emit = emitFunc;
}

// SetEmitEnabled controls whether the emitFunc provided to NewDecoder
// should be called. The default is true.
//
// This facility exists to let servers enforce MAX_HEADER_LIST_SIZE
// while still decoding and keeping in-sync with decoder state, but
// without doing unnecessary decompression or generating unnecessary
// garbage for header fields past the limit.
internal static void SetEmitEnabledᴍ(ref Decoder d, bool v) { d.emitEnabled = v; }

// EmitEnabled reports whether calls to the emitFunc provided to NewDecoder
// are currently enabled. The default is true.
internal static bool EmitEnabledᴍ(ref Decoder d) { return d.emitEnabled; }

// TODO: add method *Decoder.Reset(maxSize, emitFunc) to let callers re-use Decoders and their
// underlying buffers for garbage reasons.
internal static void SetMaxDynamicTableSizeᴍ(ref Decoder d, uint32 v) {
    d.dynTab.setMaxSize(v);
}

// SetAllowedMaxDynamicTableSize sets the upper bound that the encoded
// stream (via dynamic table size updates) may set the maximum size
// to.
internal static void SetAllowedMaxDynamicTableSizeᴍ(ref Decoder d, uint32 v) {
    d.dynTab.allowedMaxSize = v;
}

} partial class hpack_package {

partial struct dynamicTable {
    // https://httpwg.org/specs/rfc7541.html#rfc.section.2.3.2
    internal headerFieldTable table;
    internal uint32 size;           // in bytes
    internal uint32 maxSize;        // current maxSize
    internal uint32 allowedMaxSize; // maxSize may go up to this, inclusive
}

} partial class hpack_package {

internal static void setMaxSizeᴍ(ref dynamicTable dt, uint32 v) {
    dt.maxSize = v;
    dt.evict();
}

internal static void addᴍ(ref dynamicTable dt, HeaderField f) {
    dt.table.addEntry(f);
    dt.size += f.Size();
    dt.evict();
}

// If we're too big, evict old stuff.
internal static void evictᴍ(ref dynamicTable dt) {
    nint n = default;
    while (dt.size > dt.maxSize && n < dt.table.len()) {
        dt.size -= dt.table.ents[n].Size();
        n++;
    }
    dt.table.evictOldest(n);
}

} partial class hpack_package {

internal static nint maxTableIndexᴍ(ref Decoder d) {
    // This should never overflow. RFC 7540 Section 6.5.2 limits the size of
    // the dynamic table to 2^32 bytes, where each entry will occupy more than
    // one byte. Further, the staticTable has a fixed, small length.
    return d.dynTab.table.len() + staticTable.len();
}

internal static (HeaderField hf, bool ok) atᴍ(ref Decoder d, uint64 i) {
    HeaderField hf = default;
    bool ok = default;

    // See Section 2.3.3.
    if (i == 0) {
        return (hf, ok);
    }
    if (i <= (uint64)staticTable.len()) {
        return (staticTable.ents[i - 1], true);
    }
    if (i > (uint64)d.maxTableIndex()) {
        return (hf, ok);
    }
    // In the dynamic table, newer entries have lower indices.
    // However, dt.ents[0] is the oldest entry. Hence, dt.ents is
    // the reversed dynamic table.
    var dt = d.dynTab.table;
    return (dt.ents[dt.len() - ((nint)i - staticTable.len())], true);
}

// DecodeFull decodes an entire block.
//
// TODO: remove this method and make it incremental later? This is
// easier for debugging now.
internal static (slice<HeaderField>, error) DecodeFullᴍ(ж<Decoder> Ꮡd, slice<byte> p) {
    GoFrame ᒐ = default;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        ref var hf = ref heap<slice<HeaderField>>(out var Ꮡhf);
        var saveFunc = d.emit;
        var saveFuncʗ1 = saveFunc;
        defer(() => {
            Ꮡd.Value.emit = saveFuncʗ1;
        }, ref ᒐ);
        d.emit = (HeaderField f) => {
            Ꮡhf.ValueSlot = append(Ꮡhf.ValueSlot, f);
        };
        {
            var (_, err) = d.Write(p); if (err != nil) {
                return (default, err);
            }
        }
        {
            var err = d.Close(); if (err != nil) {
                return (default, err);
            }
        }
        return (hf, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string truncatedHeadersˢᴍDecoder => ref truncatedHeadersˢᵇᴍDecoder;
private static readonly @string truncatedHeadersˢᵇᴍDecoder = "truncated headers"u8;

// Close declares that the decoding is complete and resets the Decoder
// to be reused again for a new header block. If there is any remaining
// data in the decoder's buffer, Close returns an error.
internal static error Closeᴍ(ref Decoder d) {
    if (d.saveBuf.Len() > 0) {
        d.saveBuf.Reset();
        return new DecodingError(errors.New(truncatedHeadersˢᴍDecoder));
    }
    d.firstField = true;
    return default;
}

internal static (nint n, error err) Writeᴍ(ref Decoder d, slice<byte> p) {
    nint n = default;
    error err = default;

    if (len(p) == 0) {
        // Prevent state machine CPU attacks (making us redo
        // work up to the point of finding out we don't have
        // enough data)
        return (n, err);
    }
    // Only copy the data if we have to. Optimistically assume
    // that p will contain a complete header block.
    if (d.saveBuf.Len() == 0) {
        d.buf = p;
    } else {
        d.saveBuf.Write(p);
        d.buf = d.saveBuf.Bytes();
        d.saveBuf.Reset();
    }

    while (len(d.buf) > 0) {
        err = d.parseHeaderFieldRepr();
        if (AreEqual(err, errNeedMore)) {
            // Extra paranoia, making sure saveBuf won't
            // get too large. All the varint and string
            // reading code earlier should already catch
            // overlong things and return ErrStringLength,
            // but keep this as a last resort.
            const int64 varIntOverhead = 8; // conservative
            if (d.maxStrLen != 0 && (int64)len(d.buf) > 2 * ((int64)d.maxStrLen + varIntOverhead)) {
                return (0, ErrStringLength);
            }
            d.saveBuf.Write(d.buf);
            return (len(p), default);
        }
        d.firstField = false;
        if (err != nil) {
            break;
        }
    }
    return (len(p), err);
}

} partial class hpack_package {

// errNeedMore is an internal sentinel error value that means the
// buffer is truncated and we need to read more data before we can
// continue parsing.
internal static error errNeedMore = errors.New("need more data");

partial struct indexType /*num:nint*/;

internal static indexType indexedTrue => /* iota */ 0;
internal static indexType indexedFalse => 1;
internal static indexType indexedNever => 2;

} partial class hpack_package {

internal static bool indexedᴍ(indexType v) { return v == indexedTrue; }
internal static bool sensitiveᴍ(indexType v) { return v == indexedNever; }

} partial class hpack_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string invalidEncodingˢᴍDecoder => ref invalidEncodingˢᵇᴍDecoder;
private static readonly @string invalidEncodingˢᵇᴍDecoder = "invalid encoding"u8;

// returns errNeedMore if there isn't enough data available.
// any other error is fatal.
// consumes d.buf iff it returns nil.
// precondition: must be called with len(d.buf) > 0
internal static error parseHeaderFieldReprᴍ(ref Decoder d) {
    var b = d.buf[0];
    switch (ᐧ) {
    case {} when (byte)(b & 128) != 0: {
        // Indexed representation.
        // High bit set?
        // https://httpwg.org/specs/rfc7541.html#rfc.section.6.1
        return d.parseFieldIndexed();
    }
    case {} when (byte)(b & 192) == 64: {
        // 6.2.1 Literal Header Field with Incremental Indexing
        // 0b10xxxxxx: top two bits are 10
        // https://httpwg.org/specs/rfc7541.html#rfc.section.6.2.1
        return d.parseFieldLiteral(6, indexedTrue);
    }
    case {} when (byte)(b & 240) == 0: {
        // 6.2.2 Literal Header Field without Indexing
        // 0b0000xxxx: top four bits are 0000
        // https://httpwg.org/specs/rfc7541.html#rfc.section.6.2.2
        return d.parseFieldLiteral(4, indexedFalse);
    }
    case {} when (byte)(b & 240) == 16: {
        // 6.2.3 Literal Header Field never Indexed
        // 0b0001xxxx: top four bits are 0001
        // https://httpwg.org/specs/rfc7541.html#rfc.section.6.2.3
        return d.parseFieldLiteral(4, indexedNever);
    }
    case {} when (byte)(b & 224) == 32: {
        // 6.3 Dynamic Table Size Update
        // Top three bits are '001'.
        // https://httpwg.org/specs/rfc7541.html#rfc.section.6.3
        return d.parseDynamicTableSizeUpdate();
    }}

    return new DecodingError(errors.New(invalidEncodingˢᴍDecoder));
}

// (same invariants and behavior as parseHeaderFieldRepr)
internal static error parseFieldIndexedᴍ(ref Decoder d) {
    var buf = d.buf;
    (var idx, buf, var err) = readVarInt(7, buf);
    if (err != nil) {
        return err;
    }
    var (hf, ok) = d.at(idx);
    if (!ok) {
        return new DecodingError((InvalidIndexError)(nint)idx);
    }
    d.buf = buf;
    return d.callEmit(new HeaderField(Name: hf.Name, Value: hf.Value));
}

// (same invariants and behavior as parseHeaderFieldRepr)
internal static error parseFieldLiteralᴍ(ref Decoder d, uint8 n, indexType it) {
    var buf = d.buf;
    (var nameIdx, buf, var err) = readVarInt(n, buf);
    if (err != nil) {
        return err;
    }

    HeaderField hf = default;
    var wantStr = d.emitEnabled || it.indexed();
    undecodedString undecodedName = default;
    if (nameIdx > 0) {
        var (ihf, ok) = d.at(nameIdx);
        if (!ok) {
            return new DecodingError((InvalidIndexError)(nint)nameIdx);
        }
        hf.Name = ihf.Name;
    } else {
        (undecodedName, buf, err) = d.readString(buf);
        if (err != nil) {
            return err;
        }
    }
    (var undecodedValue, buf, err) = d.readString(buf);
    if (err != nil) {
        return err;
    }
    if (wantStr) {
        if (nameIdx <= 0) {
            (hf.Name, err) = d.decodeString(undecodedName);
            if (err != nil) {
                return err;
            }
        }
        (hf.Value, err) = d.decodeString(undecodedValue);
        if (err != nil) {
            return err;
        }
    }
    d.buf = buf;
    if (it.indexed()) {
        d.dynTab.add(hf);
    }
    hf.Sensitive = it.sensitive();
    return d.callEmit(hf);
}

internal static error callEmitᴍ(ref Decoder d, HeaderField hf) {
    if (d.maxStrLen != 0) {
        if (len(hf.Name) > d.maxStrLen || len(hf.Value) > d.maxStrLen) {
            return ErrStringLength;
        }
    }
    if (d.emitEnabled) {
        d.emit(hf);
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string dynamicTableSizeUpdateˢᴍDecoder => ref dynamicTableSizeUpdateˢᵇᴍDecoder;
private static readonly @string dynamicTableSizeUpdateˢᵇᴍDecoder = "dynamic table size update MUST occur at the beginning of a header block"u8;
private static ref readonly @string dynamicTableSizeUpdateˢ2ᴍDecoder => ref dynamicTableSizeUpdateˢ2ᵇᴍDecoder;
private static readonly @string dynamicTableSizeUpdateˢ2ᵇᴍDecoder = "dynamic table size update too large"u8;

// (same invariants and behavior as parseHeaderFieldRepr)
internal static error parseDynamicTableSizeUpdateᴍ(ref Decoder d) {
    // RFC 7541, sec 4.2: This dynamic table size update MUST occur at the
    // beginning of the first header block following the change to the dynamic table size.
    if (!d.firstField && d.dynTab.size > 0) {
        return new DecodingError(errors.New(dynamicTableSizeUpdateˢᴍDecoder));
    }

    var buf = d.buf;
    (var size, buf, var err) = readVarInt(5, buf);
    if (err != nil) {
        return err;
    }
    if (size > (uint64)d.dynTab.allowedMaxSize) {
        return new DecodingError(errors.New(dynamicTableSizeUpdateˢ2ᴍDecoder));
    }
    d.dynTab.setMaxSize((uint32)size);
    d.buf = buf;
    return default;
}

} partial class hpack_package {

internal static DecodingError errVarintOverflow = new DecodingError(errors.New("varint integer overflow"));

// readVarInt reads an unsigned variable length integer off the
// beginning of p. n is the parameter as described in
// https://httpwg.org/specs/rfc7541.html#rfc.section.5.1.
//
// n must always be between 1 and 8.
//
// The returned remain buffer is either a smaller suffix of p, or err != nil.
// The error is errNeedMore if p doesn't contain a complete integer.
internal static (uint64 i, slice<byte> remain, error err) readVarInt(byte n, slice<byte> p) {
    uint64 i = default;

    if (n < 1 || n > 8) {
        throw panic("bad n");
    }
    if (len(p) == 0) {
        return (0, p, errNeedMore);
    }
    i = (uint64)p[0];
    if (n < 8) {
        i &= (uint64)((((uint64)1).Lsh((uint64)n)) - 1);
    }
    if (i < (((uint64)1).Lsh((uint64)n)) - 1) {
        return (i, p[1..], default);
    }

    var origP = p;
    p = p[1..];
    uint64 m = default;
    while (len(p) > 0) {
        var b = p[0];
        p = p[1..];
        i += ((uint64)((byte)(b & 127))).Lsh(m);
        if ((byte)(b & 128) == 0) {
            return (i, p, default);
        }
        m += 7;
        if (m >= 63) { // TODO: proper overflow check. making this up.
            return (0, origP, errVarintOverflow);
        }
    }
    return (0, origP, errNeedMore);
}

} partial class hpack_package {

// readString reads an hpack string from p.
//
// It returns a reference to the encoded string data to permit deferring decode costs
// until after the caller verifies all data is present.
internal static (undecodedString u, slice<byte> remain, error err) readStringᴍ(ref Decoder d, slice<byte> p) {
    undecodedString u = default;
    error err = default;

    if (len(p) == 0) {
        return (u, p, errNeedMore);
    }
    var isHuff = (byte)(p[0] & 128) != 0;
    (var strLen, p, err) = readVarInt(7, p);
    if (err != nil) {
        return (u, p, err);
    }
    if (d.maxStrLen != 0 && strLen > (uint64)d.maxStrLen) {
        // Returning an error here means Huffman decoding errors
        // for non-indexed strings past the maximum string length
        // are ignored, but the server is returning an error anyway
        // and because the string is not indexed the error will not
        // affect the decoding state.
        return (u, default, ErrStringLength);
    }
    if ((uint64)len(p) < strLen) {
        return (u, p, errNeedMore);
    }
    u.isHuff = isHuff;
    u.b = p.slice(0, (nint)(strLen));
    return (u, p.slice((nint)(strLen)), default);
}

} partial class hpack_package {

partial struct undecodedString {
    internal bool isHuff;
    internal slice<byte> b;
}

} partial class hpack_package {

internal static (@string, error) decodeStringᴍ(ref Decoder d, undecodedString u) {
    if (!u.isHuff) {
        return ((@string)u.b, default);
    }
    var buf = ᏑbufPool.Get()._<ж<bytes.Buffer>>();
    buf.Reset(); // don't trust others
    @string s = default;
    var err = huffmanDecode(buf, d.maxStrLen, u.b);
    if (err == nil) {
        s = buf.String();
    }
    buf.Reset(); // be nice to GC
    ᏑbufPool.Put(buf.OrTypedNil());
    return (s, err);
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class hpackᴛDecodingError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this DecodingError de) => hpack_package.Errorᴍ(de);
}
partial class hpackᴛInvalidIndexError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this InvalidIndexError e) => hpack_package.Errorᴍ(e);
}
partial class hpackᴛHeaderField {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsPseudo(this HeaderField hf) => hpack_package.IsPseudoᴍ(hf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this HeaderField hf) => hpack_package.Stringᴍ(hf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint32 Size(this HeaderField hf) => hpack_package.Sizeᴍ(hf);
}
partial class hpackᴛDecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMaxStringLength(this ref Decoder d, nint n) => hpack_package.SetMaxStringLengthᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetEmitFunc(this ref Decoder d, Action<HeaderField> emitFunc) => hpack_package.SetEmitFuncᴍ(ref d, emitFunc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetEmitEnabled(this ref Decoder d, bool v) => hpack_package.SetEmitEnabledᴍ(ref d, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool EmitEnabled(this ref Decoder d) => hpack_package.EmitEnabledᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMaxDynamicTableSize(this ref Decoder d, uint32 v) => hpack_package.SetMaxDynamicTableSizeᴍ(ref d, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetAllowedMaxDynamicTableSize(this ref Decoder d, uint32 v) => hpack_package.SetAllowedMaxDynamicTableSizeᴍ(ref d, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint maxTableIndex(this ref Decoder d) => hpack_package.maxTableIndexᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (HeaderField hf, bool ok) at(this ref Decoder d, uint64 i) => hpack_package.atᴍ(ref d, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<HeaderField>, error) DecodeFull(this ж<Decoder> Ꮡd, slice<byte> p) => hpack_package.DecodeFullᴍ(Ꮡd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ref Decoder d) => hpack_package.Closeᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) Write(this ref Decoder d, slice<byte> p) => hpack_package.Writeᴍ(ref d, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseHeaderFieldRepr(this ref Decoder d) => hpack_package.parseHeaderFieldReprᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseFieldIndexed(this ref Decoder d) => hpack_package.parseFieldIndexedᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseFieldLiteral(this ref Decoder d, uint8 n, indexType it) => hpack_package.parseFieldLiteralᴍ(ref d, n, it);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error callEmit(this ref Decoder d, HeaderField hf) => hpack_package.callEmitᴍ(ref d, hf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error parseDynamicTableSizeUpdate(this ref Decoder d) => hpack_package.parseDynamicTableSizeUpdateᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (undecodedString u, slice<byte> remain, error err) readString(this ref Decoder d, slice<byte> p) => hpack_package.readStringᴍ(ref d, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, error) decodeString(this ref Decoder d, undecodedString u) => hpack_package.decodeStringᴍ(ref d, u);
}
partial class hpackᴛdynamicTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setMaxSize(this ref dynamicTable dt, uint32 v) => hpack_package.setMaxSizeᴍ(ref dt, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref dynamicTable dt, HeaderField f) => hpack_package.addᴍ(ref dt, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void evict(this ref dynamicTable dt) => hpack_package.evictᴍ(ref dt);
}
partial class hpackᴛindexType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool indexed(this indexType v) => hpack_package.indexedᴍ(v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool sensitive(this indexType v) => hpack_package.sensitiveᴍ(v);
}
// </GlyphForwarders>
