// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using fmt = fmt_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class norm_package {

// MaxSegmentSize is the maximum size of a byte buffer needed to consider any
// sequence of starter and non-starter runes for the purpose of normalization.
public static UntypedInt MaxSegmentSize => /* maxByteBufferSize */ 128;

// An Iter iterates over a string or byte slice, while normalizing it
// to a given Form.
partial struct Iter {
    internal reorderBuffer rb;
    internal array<byte> buf = new(maxByteBufferSize);
    internal ΔProperties info; // first character saved from previous iteration
    internal Func<ж<Iter>, slice<byte>> next; // implementation of next depends on form
    internal Func<ж<Iter>, slice<byte>> asciiF;
    internal nint p;   // current position in input source
    internal slice<byte> multiSeg; // remainder of multi-segment decomposition
}

// type iterFunc is a methodless func type — rendered inline as its base delegate

// Init initializes i to iterate over src after normalizing it to Form f.
public static void Init(this ref Iter i, Form f, slice<byte> src) {
    i.p = 0;
    if (len(src) == 0) {
        i.setDone();
        i.rb.nsrc = 0;
        return;
    }
    i.multiSeg = default!;
    i.rb.init(f, src);
    i.next = i.rb.f.nextMain;
    i.asciiF = nextASCIIBytes;
    i.info = i.rb.f.info(i.rb.src, i.p);
    i.rb.ss.first(i.info);
}

// InitString initializes i to iterate over src after normalizing it to Form f.
public static void InitString(this ref Iter i, Form f, @string src) {
    i.p = 0;
    if (len(src) == 0) {
        i.setDone();
        i.rb.nsrc = 0;
        return;
    }
    i.multiSeg = default!;
    i.rb.initString(f, src);
    i.next = i.rb.f.nextMain;
    i.asciiF = nextASCIIString;
    i.info = i.rb.f.info(i.rb.src, i.p);
    i.rb.ss.first(i.info);
}

// Seek sets the segment to be returned by the next call to Next to start
// at position p.  It is the responsibility of the caller to set p to the
// start of a segment.
public static (int64, error) Seek(this ref Iter i, int64 offset, nint whence) {
    int64 abs = default!;
    switch (whence) {
    case 0: {
        abs = offset;
        break;
    }
    case 1: {
        abs = (int64)i.p + offset;
        break;
    }
    case 2: {
        abs = (int64)i.rb.nsrc + offset;
        break;
    }
    default: {
        return (0, fmt.Errorf("norm: invalid whence"u8));
    }}

    if (abs < 0) {
        return (0, fmt.Errorf("norm: negative position"u8));
    }
    if ((nint)abs >= i.rb.nsrc) {
        i.setDone();
        return ((int64)i.p, default!);
    }
    i.p = (nint)abs;
    i.multiSeg = default!;
    i.next = i.rb.f.nextMain;
    i.info = i.rb.f.info(i.rb.src, i.p);
    i.rb.ss.first(i.info);
    return (abs, default!);
}

// returnSlice returns a slice of the underlying input type as a byte slice.
// If the underlying is of type []byte, it will simply return a slice.
// If the underlying is of type string, it will copy the slice to the buffer
// and return that.
internal static slice<byte> returnSlice(this ref Iter i, nint a, nint b) {
    if (i.rb.src.bytes == default!) {
        return i.buf.slice(0, copy(i.buf[..], i.rb.src.str.slice(a, b)));
    }
    return i.rb.src.bytes.slice(a, b);
}

// Pos returns the byte position at which the next call to Next will commence processing.
public static nint Pos(this ref Iter i) {
    return i.p;
}

internal static void setDone(this ref Iter i) {
    i.next = nextDone;
    i.p = i.rb.nsrc;
}

// Done returns true if there is no more input to process.
public static bool Done(this ref Iter i) {
    return i.p >= i.rb.nsrc;
}

// Next returns f(i.input[i.Pos():n]), where n is a boundary of i.input.
// For any input a and b for which f(a) == f(b), subsequent calls
// to Next will return the same segments.
// Modifying runes are grouped together with the preceding starter, if such a starter exists.
// Although not guaranteed, n will typically be the smallest possible n.
public static slice<byte> Next(this ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    return i.next(Ꮡi);
}

internal static slice<byte> nextASCIIBytes(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    nint p = i.p + 1;
    if (p >= i.rb.nsrc) {
        nint p0 = i.p;
        i.setDone();
        return i.rb.src.bytes.slice(p0, p);
    }
    if (i.rb.src.bytes[p] < utf8.RuneSelf) {
        nint p0 = i.p;
        i.p = p;
        return i.rb.src.bytes.slice(p0, p);
    }
    i.info = i.rb.f.info(i.rb.src, i.p);
    i.next = i.rb.f.nextMain;
    return i.next(Ꮡi);
}

internal static slice<byte> nextASCIIString(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    nint p = i.p + 1;
    if (p >= i.rb.nsrc) {
        i.buf[0] = i.rb.src.str[i.p];
        i.setDone();
        return i.buf[..1];
    }
    if (i.rb.src.str[p] < utf8.RuneSelf) {
        i.buf[0] = i.rb.src.str[i.p];
        i.p = p;
        return i.buf[..1];
    }
    i.info = i.rb.f.info(i.rb.src, i.p);
    i.next = i.rb.f.nextMain;
    return i.next(Ꮡi);
}

internal static slice<byte> nextHangul(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    nint p = i.p;
    nint next = p + (nint)hangulUTF8Size;
    if (next >= i.rb.nsrc){
        i.setDone();
    } else 
    if (i.rb.src.hangul(next) == 0) {
        i.rb.ss.next(i.info);
        i.info = i.rb.f.info(i.rb.src, i.p);
        i.next = i.rb.f.nextMain;
        return i.next(Ꮡi);
    }
    i.p = next;
    return i.buf.slice(0, decomposeHangul(i.buf[..], i.rb.src.hangul(p)));
}

internal static slice<byte> nextDone(ж<Iter> Ꮡi) {
    return default!;
}

// nextMulti is used for iterating over multi-segment decompositions
// for decomposing normal forms.
internal static slice<byte> nextMulti(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    nint j = 0;
    var d = i.multiSeg;
    // skip first rune
    for (j = 1; j < len(d) && !utf8.RuneStart(d[j]); j++) {
    }
    while (j < len(d)) {
        var info = i.rb.f.info(new input(bytes: d), j);
        if (info.BoundaryBefore()) {
            i.multiSeg = d.slice(j);
            return d.slice(0, j);
        }
        j += (nint)info.size;
    }
    // treat last segment as normal decomposition
    i.next = i.rb.f.nextMain;
    return i.next(Ꮡi);
}

// nextMultiNorm is used for iterating over multi-segment decompositions
// for composing normal forms.
internal static slice<byte> nextMultiNorm(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    nint j = 0;
    var d = i.multiSeg;
    while (j < len(d)) {
        var info = i.rb.f.info(new input(bytes: d), j);
        if (info.BoundaryBefore()) {
            i.rb.compose();
            var seg = i.buf.slice(0, i.rb.flushCopy(i.buf[..]));
            Ꮡi.of(Iter.Ꮡrb).insertUnsafe(new input(bytes: d), j, info);
            i.multiSeg = d.slice(j + (nint)info.size);
            return seg;
        }
        Ꮡi.of(Iter.Ꮡrb).insertUnsafe(new input(bytes: d), j, info);
        j += (nint)info.size;
    }
    i.multiSeg = default!;
    i.next = nextComposed;
    return doNormComposed(Ꮡi);
}

// nextDecomposed is the implementation of Next for forms NFD and NFKD.
internal static slice<byte> /*next*/ nextDecomposed(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    nint outp = 0;
    nint inCopyStart = i.p;
    nint outCopyStart = 0;
    while (ᐧ) {
        {
            nint sz = (nint)i.info.size; if (sz <= 1){
                i.rb.ss = 0;
                nint p = i.p;
                i.p++; // ASCII or illegal byte.  Either way, advance by 1.
                if (i.p >= i.rb.nsrc){
                    i.setDone();
                    return i.returnSlice(p, i.p);
                } else 
                if (i.rb.src._byte(i.p) < utf8.RuneSelf) {
                    i.next = i.asciiF;
                    return i.returnSlice(p, i.p);
                }
                outp++;
            } else 
            {
                var d = i.info.Decomposition(); if (d != default!){
                    // Note: If leading CCC != 0, then len(d) == 2 and last is also non-zero.
                    // Case 1: there is a leftover to copy.  In this case the decomposition
                    // must begin with a modifier and should always be appended.
                    // Case 2: no leftover. Simply return d if followed by a ccc == 0 value.
                    nint p = outp + len(d);
                    if (outp > 0){
                        i.rb.src.copySlice(i.buf.slice(outCopyStart), inCopyStart, i.p);
                        // TODO: this condition should not be possible, but we leave it
                        // in for defensive purposes.
                        if (p > len(i.buf)) {
                            return i.buf.slice(0, outp);
                        }
                    } else 
                    if (i.info.multiSegment()) {
                        // outp must be 0 as multi-segment decompositions always
                        // start a new segment.
                        if (i.multiSeg == default!) {
                            i.multiSeg = d;
                            i.next = nextMulti;
                            return nextMulti(Ꮡi);
                        }
                        // We are in the last segment.  Treat as normal decomposition.
                        d = i.multiSeg;
                        i.multiSeg = default!;
                        p = len(d);
                    }
                    var prevCCΔ1 = i.info.tccc;
                    {
                        i.p += sz; if (i.p >= i.rb.nsrc){
                            i.setDone();
                            i.info = new ΔProperties(nil); // Force BoundaryBefore to succeed.
                        } else {
                            i.info = i.rb.f.info(i.rb.src, i.p);
                        }
                    }
                    var exprᴛ1 = i.rb.ss.next(i.info);
                    var matchᴛ1 = false;
                    if (exprᴛ1 == ssOverflow) {
                        i.next = nextCGJDecompose;
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ1 && exprᴛ1 == ssStarter) { matchᴛ1 = true;
                        if (outp > 0) {
                            copy(i.buf.slice(outp), d);
                            return i.buf.slice(0, p);
                        }
                        return d;
                    }

                    copy(i.buf.slice(outp), d);
                    outp = p;
                    (inCopyStart, outCopyStart) = (i.p, outp);
                    if (i.info.ccc < prevCCΔ1) {
                        goto doNorm;
                    }
                    continue;
                } else 
                {
                    var r = i.rb.src.hangul(i.p); if (r != 0){
                        outp = decomposeHangul(i.buf[..], r);
                        i.p += hangulUTF8Size;
                        (inCopyStart, outCopyStart) = (i.p, outp);
                        if (i.p >= i.rb.nsrc){
                            i.setDone();
                            break;
                        } else 
                        if (i.rb.src.hangul(i.p) != 0) {
                            i.next = nextHangul;
                            return i.buf.slice(0, outp);
                        }
                    } else {
                        nint p = outp + sz;
                        if (p > len(i.buf)) {
                            break;
                        }
                        outp = p;
                        i.p += sz;
                    }
                }
            }
        }
        if (i.p >= i.rb.nsrc) {
            i.setDone();
            break;
        }
        var prevCC = i.info.tccc;
        i.info = i.rb.f.info(i.rb.src, i.p);
        {
            ssState v = i.rb.ss.next(i.info); if (v == ssStarter){
                break;
            } else 
            if (v == ssOverflow) {
                i.next = nextCGJDecompose;
                break;
            }
        }
        if (i.info.ccc < prevCC) {
            goto doNorm;
        }
    }
    if (outCopyStart == 0){
        return i.returnSlice(inCopyStart, i.p);
    } else 
    if (inCopyStart < i.p) {
        i.rb.src.copySlice(i.buf.slice(outCopyStart), inCopyStart, i.p);
    }
    return i.buf.slice(0, outp);
doNorm:
    i.rb.src.copySlice(i.buf.slice(outCopyStart), // Insert what we have decomposed so far in the reorderBuffer.
 // As we will only reorder, there will always be enough room.
 inCopyStart, i.p);
    Ꮡi.of(Iter.Ꮡrb).insertDecomposed(i.buf.slice(0, outp));
    return doNormDecomposed(Ꮡi);
}

internal static slice<byte> doNormDecomposed(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    while (ᐧ) {
        Ꮡi.of(Iter.Ꮡrb).insertUnsafe(i.rb.src, i.p, i.info);
        {
            i.p += (nint)i.info.size; if (i.p >= i.rb.nsrc) {
                i.setDone();
                break;
            }
        }
        i.info = i.rb.f.info(i.rb.src, i.p);
        if (i.info.ccc == 0) {
            break;
        }
        {
            ssState s = i.rb.ss.next(i.info); if (s == ssOverflow) {
                i.next = nextCGJDecompose;
                break;
            }
        }
    }
    // new segment or too many combining characters: exit normalization
    return i.buf.slice(0, i.rb.flushCopy(i.buf[..]));
}

internal static slice<byte> nextCGJDecompose(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    i.rb.ss = 0;
    i.rb.insertCGJ();
    i.next = nextDecomposed;
    i.rb.ss.first(i.info);
    var buf = doNormDecomposed(Ꮡi);
    return buf;
}

// nextComposed is the implementation of Next for forms NFC and NFKC.
internal static slice<byte> nextComposed(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    nint outp = 0;
    nint startp = i.p;
    uint8 prevCC = default!;
    while (ᐧ) {
        if (!i.info.isYesC()) {
            goto doNorm;
        }
        prevCC = i.info.tccc;
        nint sz = (nint)i.info.size;
        if (sz == 0) {
            sz = 1; // illegal rune: copy byte-by-byte
        }
        nint p = outp + sz;
        if (p > len(i.buf)) {
            break;
        }
        outp = p;
        i.p += sz;
        if (i.p >= i.rb.nsrc){
            i.setDone();
            break;
        } else 
        if (i.rb.src._byte(i.p) < utf8.RuneSelf) {
            i.rb.ss = 0;
            i.next = i.asciiF;
            break;
        }
        i.info = i.rb.f.info(i.rb.src, i.p);
        {
            ssState v = i.rb.ss.next(i.info); if (v == ssStarter){
                break;
            } else 
            if (v == ssOverflow) {
                i.next = nextCGJCompose;
                break;
            }
        }
        if (i.info.ccc < prevCC) {
            goto doNorm;
        }
    }
    return i.returnSlice(startp, i.p);
doNorm:
    i.p = startp;
    // reset to start position
    i.info = i.rb.f.info(i.rb.src, i.p);
    i.rb.ss.first(i.info);
    if (i.info.multiSegment()) {
        var d = i.info.Decomposition();
        var info = i.rb.f.info(new input(bytes: d), 0);
        Ꮡi.of(Iter.Ꮡrb).insertUnsafe(new input(bytes: d), 0, info);
        i.multiSeg = d.slice((nint)info.size);
        i.next = nextMultiNorm;
        return nextMultiNorm(Ꮡi);
    }
    i.rb.ss.first(i.info);
    Ꮡi.of(Iter.Ꮡrb).insertUnsafe(i.rb.src, i.p, i.info);
    return doNormComposed(Ꮡi);
}

internal static slice<byte> doNormComposed(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    // First rune should already be inserted.
    while (ᐧ) {
        {
            i.p += (nint)i.info.size; if (i.p >= i.rb.nsrc) {
                i.setDone();
                break;
            }
        }
        i.info = i.rb.f.info(i.rb.src, i.p);
        {
            ssState s = i.rb.ss.next(i.info); if (s == ssStarter){
                break;
            } else 
            if (s == ssOverflow) {
                i.next = nextCGJCompose;
                break;
            }
        }
        Ꮡi.of(Iter.Ꮡrb).insertUnsafe(i.rb.src, i.p, i.info);
    }
    i.rb.compose();
    var seg = i.buf.slice(0, i.rb.flushCopy(i.buf[..]));
    return seg;
}

internal static slice<byte> nextCGJCompose(ж<Iter> Ꮡi) {
    ref var i = ref Ꮡi.DerefOrNull();

    i.rb.ss = 0; // instead of first
    i.rb.insertCGJ();
    i.next = nextComposed;
    // Note that we treat any rune with nLeadingNonStarters > 0 as a non-starter,
    // even if they are not. This is particularly dubious for U+FF9E and UFF9A.
    // If we ever change that, insert a check here.
    i.rb.ss.first(i.info);
    Ꮡi.of(Iter.Ꮡrb).insertUnsafe(i.rb.src, i.p, i.info);
    return doNormComposed(Ꮡi);
}

} // end norm_package
