// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Note: the file data_test.go that is generated should not be checked in.
//go:generate go run maketables.go triegen.go
//go:generate go test -tags test

// Package norm contains types and functions for normalizing Unicode strings.
namespace go.vendor.golang.org.x.text.unicode;

// import "golang.org/x/text/unicode/norm"
using utf8 = go.unicode.utf8_package;
using transform = go.vendor.golang.org.x.text.transform_package;
using go.unicode;
using go.vendor.golang.org.x.text;
using ꓸꓸꓸbyte = Span<byte>;

partial class norm_package {

partial struct Form /*num:nint*/;

public static Form NFC => /* iota */ 0;
public static Form NFD => 1;
public static Form NFKC => 2;
public static Form NFKD => 3;

// Bytes returns f(b). May return b if f(b) = b.
public static slice<byte> Bytes(this Form f, slice<byte> b) {
    var src = inputBytes(b);
    var ft = formTable[f];
    var (n, ok) = ft.quickSpan(src, 0, len(b), true);
    if (ok) {
        return b;
    }
    var @out = new slice<byte>(n, len(b));
    copy(@out, b.slice(0, n));
    ref var rb = ref heap<reorderBuffer>(out var Ꮡrb);
    rb = new reorderBuffer(f: ft.Value, src: src, nsrc: len(b), @out: @out, flushF: appendFlush);
    return doAppendInner(Ꮡrb, n);
}

// String returns f(s).
public static @string String(this Form f, @string s) {
    var src = inputString(s);
    var ft = formTable[f];
    var (n, ok) = ft.quickSpan(src, 0, len(s), true);
    if (ok) {
        return s;
    }
    var @out = new slice<byte>(n, len(s));
    copy(@out, s.slice(0, n));
    ref var rb = ref heap<reorderBuffer>(out var Ꮡrb);
    rb = new reorderBuffer(f: ft.Value, src: src, nsrc: len(s), @out: @out, flushF: appendFlush);
    return ((@string)doAppendInner(Ꮡrb, n));
}

// IsNormal returns true if b == f(b).
public static bool IsNormal(this Form f, slice<byte> b) {
    var src = inputBytes(b);
    var ft = formTable[f];
    var (bp, ok) = ft.quickSpan(src, 0, len(b), true);
    if (ok) {
        return true;
    }
    ref var rb = ref heap<reorderBuffer>(out var Ꮡrb);
    rb = new reorderBuffer(f: ft.Value, src: src, nsrc: len(b));
    rb.setFlusher(default!, cmpNormalBytes);
    while (bp < len(b)) {
        rb.@out = b.slice(bp);
        {
            bp = decomposeSegment(Ꮡrb, bp, true); if (bp < 0) {
                return false;
            }
        }
        (bp, _) = rb.f.quickSpan(rb.src, bp, len(b), true);
    }
    return true;
}

internal static bool cmpNormalBytes(ж<reorderBuffer> Ꮡrb) {
    ref var rb = ref Ꮡrb.DerefOrNull();

    var b = rb.@out;
    for (nint i = 0; i < rb.nrune; i++) {
        var info = rb.rune[i];
        if ((nint)info.size > len(b)) {
            return false;
        }
        var p = info.pos;
        var pe = (uint8)(p + info.size);
        for (; p < pe; p++) {
            if (b[0] != rb.@byte[p]) {
                return false;
            }
            b = b[1..];
        }
    }
    return true;
}

// IsNormalString returns true if s == f(s).
public static bool IsNormalString(this Form f, @string s) {
    var src = inputString(s);
    var ft = formTable[f];
    var (bp, ok) = ft.quickSpan(src, 0, len(s), true);
    if (ok) {
        return true;
    }
    ref var rb = ref heap<reorderBuffer>(out var Ꮡrb);
    rb = new reorderBuffer(f: ft.Value, src: src, nsrc: len(s));
    rb.setFlusher(default!, (ж<reorderBuffer> rbΔ1) => {
        for (nint i = 0; i < (~rbΔ1).nrune; i++) {
            var info = (~rbΔ1).rune[i];
            if (bp + (nint)info.size > len(s)) {
                return false;
            }
            var p = info.pos;
            var pe = (uint8)(p + info.size);
            for (; p < pe; p++) {
                if (s[bp] != (~rbΔ1).@byte[p]) {
                    return false;
                }
                bp++;
            }
        }
        return true;
    });
    while (bp < len(s)) {
        {
            bp = decomposeSegment(Ꮡrb, bp, true); if (bp < 0) {
                return false;
            }
        }
        (bp, _) = rb.f.quickSpan(rb.src, bp, len(s), true);
    }
    return true;
}

// patchTail fixes a case where a rune may be incorrectly normalized
// if it is followed by illegal continuation bytes. It returns the
// patched buffer and whether the decomposition is still in progress.
internal static bool patchTail(ж<reorderBuffer> Ꮡrb) {
    ref var rb = ref Ꮡrb.DerefOrNull();

    var (info, p) = lastRuneStart(ref nonnil(ref rb).f, rb.@out);
    if (p == -1 || info.size == 0) {
        return true;
    }
    nint end = p + (nint)info.size;
    nint extra = len(rb.@out) - end;
    if (extra > 0) {
        // Potentially allocating memory. However, this only
        // happens with ill-formed UTF-8.
        var x = new slice<byte>(0);
        x = appendꓸꓸꓸ(x, rb.@out.slice(len(rb.@out) - extra));
        rb.@out = rb.@out.slice(0, end);
        decomposeToLastBoundary(Ꮡrb);
        Ꮡrb.doFlush();
        rb.@out = appendꓸꓸꓸ(rb.@out, x);
        return false;
    }
    var buf = rb.@out.slice(p);
    rb.@out = rb.@out.slice(0, p);
    decomposeToLastBoundary(Ꮡrb);
    {
        ssState s = rb.ss.next(info); if (s == ssStarter){
            Ꮡrb.doFlush();
            rb.ss.first(info);
        } else 
        if (s == ssOverflow) {
            Ꮡrb.doFlush();
            rb.insertCGJ();
            rb.ss = 0;
        }
    }
    Ꮡrb.insertUnsafe(inputBytes(buf), 0, info);
    return true;
}

internal static nint appendQuick(ж<reorderBuffer> Ꮡrb, nint i) {
    ref var rb = ref Ꮡrb.DerefOrNull();

    if (rb.nsrc == i) {
        return i;
    }
    var (end, _) = rb.f.quickSpan(rb.src, i, rb.nsrc, true);
    rb.@out = rb.src.appendSlice(rb.@out, i, end);
    return end;
}

// Append returns f(append(out, b...)).
// The buffer out must be nil, empty, or equal to f(out).
public static slice<byte> Append(this Form f, slice<byte> @out, params ꓸꓸꓸbyte srcʗp) {
    var src = srcʗp.slice();

    return f.doAppend(@out, inputBytes(src), len(src));
}

internal static slice<byte> doAppend(this Form f, slice<byte> @out, input src, nint n) {
    if (n == 0) {
        return @out;
    }
    var ft = formTable[f];
    // Attempt to do a quickSpan first so we can avoid initializing the reorderBuffer.
    if (len(@out) == 0) {
        var (p, _) = ft.quickSpan(src, 0, n, true);
        @out = src.appendSlice(@out, 0, p);
        if (p == n) {
            return @out;
        }
        ref var rbΔ1 = ref heap<reorderBuffer>(out var ᏑrbΔ1);
        rbΔ1 = new reorderBuffer(f: ft.Value, src: src, nsrc: n, @out: @out, flushF: appendFlush);
        return doAppendInner(ᏑrbΔ1, p);
    }
    ref var rb = ref heap<reorderBuffer>(out var Ꮡrb);
    rb = new reorderBuffer(f: ft.Value, src: src, nsrc: n);
    return doAppend(Ꮡrb, @out, 0);
}

internal static slice<byte> doAppend(ж<reorderBuffer> Ꮡrb, slice<byte> @out, nint p) {
    ref var rb = ref Ꮡrb.DerefOrNull();

    rb.setFlusher(@out, appendFlush);
    var src = rb.src;
    nint n = rb.nsrc;
    var doMerge = len(@out) > 0;
    {
        nint q = src.skipContinuationBytes(p); if (q > p) {
            // Move leading non-starters to destination.
            rb.@out = src.appendSlice(rb.@out, p, q);
            p = q;
            doMerge = patchTail(Ꮡrb);
        }
    }
    var fd = Ꮡrb.of(reorderBuffer.Ꮡf);
    if (doMerge) {
        ΔProperties info = default!;
        if (p < n) {
            info = (~fd).info(src, p);
            if (!info.BoundaryBefore() || info.nLeadingNonStarters() > 0) {
                if (p == 0) {
                    decomposeToLastBoundary(Ꮡrb);
                }
                p = decomposeSegment(Ꮡrb, p, true);
            }
        }
        if (info.size == 0) {
            Ꮡrb.doFlush();
            // Append incomplete UTF-8 encoding.
            return src.appendSlice(rb.@out, p, n);
        }
        if (rb.nrune > 0) {
            return doAppendInner(Ꮡrb, p);
        }
    }
    p = appendQuick(Ꮡrb, p);
    return doAppendInner(Ꮡrb, p);
}

internal static slice<byte> doAppendInner(ж<reorderBuffer> Ꮡrb, nint p) {
    ref var rb = ref Ꮡrb.DerefOrNull();

    for (nint n = rb.nsrc; p < n; ) {
        p = decomposeSegment(Ꮡrb, p, true);
        p = appendQuick(Ꮡrb, p);
    }
    return rb.@out;
}

// AppendString returns f(append(out, []byte(s))).
// The buffer out must be nil, empty, or equal to f(out).
public static slice<byte> AppendString(this Form f, slice<byte> @out, @string src) {
    return f.doAppend(@out, inputString(src), len(src));
}

// QuickSpan returns a boundary n such that b[0:n] == f(b[0:n]).
// It is not guaranteed to return the largest such n.
public static nint QuickSpan(this Form f, slice<byte> b) {
    var (n, _) = formTable[f].quickSpan(inputBytes(b), 0, len(b), true);
    return n;
}

// Span implements transform.SpanningTransformer. It returns a boundary n such
// that b[0:n] == f(b[0:n]). It is not guaranteed to return the largest such n.
public static (nint n, error err) Span(this Form f, slice<byte> b, bool atEOF) {
    nint n = default!;
    error err = default!;

    (n, var ok) = formTable[f].quickSpan(inputBytes(b), 0, len(b), atEOF);
    if (n < len(b)) {
        if (!ok){
            err = go.vendor.golang.org.x.text.transform_package.ErrEndOfSpan;
        } else {
            err = go.vendor.golang.org.x.text.transform_package.ErrShortSrc;
        }
    }
    return (n, err);
}

// SpanString returns a boundary n such that s[0:n] == f(s[0:n]).
// It is not guaranteed to return the largest such n.
public static (nint n, error err) SpanString(this Form f, @string s, bool atEOF) {
    nint n = default!;
    error err = default!;

    (n, var ok) = formTable[f].quickSpan(inputString(s), 0, len(s), atEOF);
    if (n < len(s)) {
        if (!ok){
            err = go.vendor.golang.org.x.text.transform_package.ErrEndOfSpan;
        } else {
            err = go.vendor.golang.org.x.text.transform_package.ErrShortSrc;
        }
    }
    return (n, err);
}

// quickSpan returns a boundary n such that src[0:n] == f(src[0:n]) and
// whether any non-normalized parts were found. If atEOF is false, n will
// not point past the last segment if this segment might be become
// non-normalized by appending other runes.
internal static (nint n, bool ok) quickSpan(this ref formInfo f, input src, nint i, nint end, bool atEOF) {
    nint n = default!;

    uint8 lastCC = default!;
    var ss = ((streamSafe)0);
    nint lastSegStart = i;
    for (n = end; i < n; ) {
        {
            nint j = src.skipASCII(i, n); if (i != j) {
                i = j;
                lastSegStart = i - 1;
                lastCC = 0;
                ss = 0;
                continue;
            }
        }
        var info = f.info(src, i);
        if (info.size == 0) {
            if (atEOF) {
                // include incomplete runes
                return (n, true);
            }
            return (lastSegStart, true);
        }
        // This block needs to be before the next, because it is possible to
        // have an overflow for runes that are starters (e.g. with U+FF9E).
        var exprᴛ1 = ss.next(info);
        if (exprᴛ1 == ssStarter) {
            lastSegStart = i;
        }
        else if (exprᴛ1 == ssOverflow) {
            return (lastSegStart, false);
        }
        else if (exprᴛ1 == ssSuccess) {
            if (lastCC > info.ccc) {
                return (lastSegStart, false);
            }
        }

        if (f.composing){
            if (!info.isYesC()) {
                break;
            }
        } else {
            if (!info.isYesD()) {
                break;
            }
        }
        lastCC = info.ccc;
        i += (nint)info.size;
    }
    if (i == n) {
        if (!atEOF) {
            n = lastSegStart;
        }
        return (n, true);
    }
    return (lastSegStart, false);
}

// QuickSpanString returns a boundary n such that s[0:n] == f(s[0:n]).
// It is not guaranteed to return the largest such n.
public static nint QuickSpanString(this Form f, @string s) {
    var (n, _) = formTable[f].quickSpan(inputString(s), 0, len(s), true);
    return n;
}

// FirstBoundary returns the position i of the first boundary in b
// or -1 if b contains no boundary.
public static nint FirstBoundary(this Form f, slice<byte> b) {
    return f.firstBoundary(inputBytes(b), len(b));
}

internal static nint firstBoundary(this Form f, input src, nint nsrc) {
    nint i = src.skipContinuationBytes(0);
    if (i >= nsrc) {
        return -1;
    }
    var fd = formTable[f];
    var ss = ((streamSafe)0);
    // We should call ss.first here, but we can't as the first rune is
    // skipped already. This means FirstBoundary can't really determine
    // CGJ insertion points correctly. Luckily it doesn't have to.
    while (ᐧ) {
        var info = (~fd).info(src, i);
        if (info.size == 0) {
            return -1;
        }
        {
            ssState s = ss.next(info); if (s != ssSuccess) {
                return i;
            }
        }
        i += (nint)info.size;
        if (i >= nsrc) {
            if (!info.BoundaryAfter() && !ss.isMax()) {
                return -1;
            }
            return nsrc;
        }
    }
}

// FirstBoundaryInString returns the position i of the first boundary in s
// or -1 if s contains no boundary.
public static nint FirstBoundaryInString(this Form f, @string s) {
    return f.firstBoundary(inputString(s), len(s));
}

// NextBoundary reports the index of the boundary between the first and next
// segment in b or -1 if atEOF is false and there are not enough bytes to
// determine this boundary.
public static nint NextBoundary(this Form f, slice<byte> b, bool atEOF) {
    return f.nextBoundary(inputBytes(b), len(b), atEOF);
}

// NextBoundaryInString reports the index of the boundary between the first and
// next segment in b or -1 if atEOF is false and there are not enough bytes to
// determine this boundary.
public static nint NextBoundaryInString(this Form f, @string s, bool atEOF) {
    return f.nextBoundary(inputString(s), len(s), atEOF);
}

internal static nint nextBoundary(this Form f, input src, nint nsrc, bool atEOF) {
    if (nsrc == 0) {
        if (atEOF) {
            return 0;
        }
        return -1;
    }
    var fd = formTable[f];
    var info = (~fd).info(src, 0);
    if (info.size == 0) {
        if (atEOF) {
            return 1;
        }
        return -1;
    }
    var ss = ((streamSafe)0);
    ss.first(info);
    for (nint i = (nint)info.size; i < nsrc; i += (nint)info.size) {
        info = (~fd).info(src, i);
        if (info.size == 0) {
            if (atEOF) {
                return i;
            }
            return -1;
        }
        // TODO: Using streamSafe to determine the boundary isn't the same as
        // using BoundaryBefore. Determine which should be used.
        {
            ssState s = ss.next(info); if (s != ssSuccess) {
                return i;
            }
        }
    }
    if (!atEOF && !info.BoundaryAfter() && !ss.isMax()) {
        return -1;
    }
    return nsrc;
}

// LastBoundary returns the position i of the last boundary in b
// or -1 if b contains no boundary.
public static nint LastBoundary(this Form f, slice<byte> b) {
    return lastBoundary(ref (formTable[f]).DerefOrNull(), b);
}

internal static nint lastBoundary(ref formInfo fd, slice<byte> b) {
    nint i = len(b);
    var (info, p) = lastRuneStart(ref fd, b);
    if (p == -1) {
        return -1;
    }
    if (info.size == 0) {
        // ends with incomplete rune
        if (p == 0) {
            // starts with incomplete rune
            return -1;
        }
        i = p;
        (info, p) = lastRuneStart(ref fd, b.slice(0, i));
        if (p == -1) {
            // incomplete UTF-8 encoding or non-starter bytes without a starter
            return i;
        }
    }
    if (p + (nint)info.size != i) {
        // trailing non-starter bytes: illegal UTF-8
        return i;
    }
    if (info.BoundaryAfter()) {
        return i;
    }
    var ss = ((streamSafe)0);
    ssState v = ss.backwards(info);
    for (i = p; i >= 0 && v != ssStarter; i = p) {
        (info, p) = lastRuneStart(ref fd, b.slice(0, i));
        {
            v = ss.backwards(info); if (v == ssOverflow) {
                break;
            }
        }
        if (p + (nint)info.size != i) {
            if (p == -1) {
                // no boundary found
                return -1;
            }
            return i; // boundary after an illegal UTF-8 encoding
        }
    }
    return i;
}

// decomposeSegment scans the first segment in src into rb. It inserts 0x034f
// (Grapheme Joiner) when it encounters a sequence of more than 30 non-starters
// and returns the number of bytes consumed from src or iShortDst or iShortSrc.
internal static nint decomposeSegment(ж<reorderBuffer> Ꮡrb, nint sp, bool atEOF) {
    ref var rb = ref Ꮡrb.DerefOrNull();

    // Force one character to be consumed.
    var info = rb.f.info(rb.src, sp);
    if (info.size == 0) {
        return 0;
    }
    {
        ssState s = rb.ss.next(info); if (s == ssStarter){
            // TODO: this could be removed if we don't support merging.
            if (rb.nrune > 0) {
                goto end;
            }
        } else 
        if (s == ssOverflow) {
            rb.insertCGJ();
            goto end;
        }
    }
    {
        insertErr err = Ꮡrb.insertFlush(rb.src, sp, info); if (err != iSuccess) {
            return (nint)err;
        }
    }
    while (ᐧ) {
        sp += (nint)info.size;
        if (sp >= rb.nsrc) {
            if (!atEOF && !info.BoundaryAfter()) {
                return (nint)iShortSrc;
            }
            break;
        }
        info = rb.f.info(rb.src, sp);
        if (info.size == 0) {
            if (!atEOF) {
                return (nint)iShortSrc;
            }
            break;
        }
        {
            ssState s = rb.ss.next(info); if (s == ssStarter){
                break;
            } else 
            if (s == ssOverflow) {
                rb.insertCGJ();
                break;
            }
        }
        {
            insertErr err = Ꮡrb.insertFlush(rb.src, sp, info); if (err != iSuccess) {
                return (nint)err;
            }
        }
    }
end:
    if (!Ꮡrb.doFlush()) {
        return (nint)iShortDst;
    }
    return sp;
}

// lastRuneStart returns the runeInfo and position of the last
// rune in buf or the zero runeInfo and -1 if no rune was found.
internal static (ΔProperties, nint) lastRuneStart(ref formInfo fd, slice<byte> buf) {
    nint p = len(buf) - 1;
    for (; p >= 0 && !utf8.RuneStart(buf[p]); p--) {
    }
    if (p < 0) {
        return (new ΔProperties(nil), -1);
    }
    return (fd.info(inputBytes(buf), p), p);
}

// decomposeToLastBoundary finds an open segment at the end of the buffer
// and scans it into rb. Returns the buffer minus the last segment.
internal static void decomposeToLastBoundary(ж<reorderBuffer> Ꮡrb) {
    ref var rb = ref Ꮡrb.DerefOrNull();

    var fd = Ꮡrb.of(reorderBuffer.Ꮡf);
    var (info, i) = lastRuneStart(ref (fd).DerefOrNull(), rb.@out);
    if ((nint)info.size != len(rb.@out) - i) {
        // illegal trailing continuation bytes
        return;
    }
    if (info.BoundaryAfter()) {
        return;
    }
    array<ΔProperties> add = new(31); /* maxNonStarters + 1 */                                             // stores runeInfo in reverse order
    nint padd = 0;
    var ss = ((streamSafe)0);
    nint p = len(rb.@out);
    while (ᐧ) {
        add[padd] = info;
        ssState v = ss.backwards(info);
        if (v == ssOverflow) {
            // Note that if we have an overflow, it the string we are appending to
            // is not correctly normalized. In this case the behavior is undefined.
            break;
        }
        padd++;
        p -= (nint)info.size;
        if (v == ssStarter || p < 0) {
            break;
        }
        (info, i) = lastRuneStart(ref (fd).DerefOrNull(), rb.@out.slice(0, p));
        if ((nint)info.size != p - i) {
            break;
        }
    }
    rb.ss = ss;
    // Copy bytes for insertion as we may need to overwrite rb.out.
    array<byte> buf = new(128); /* maxBufferSize * utf8.UTFMax */
    var cp = buf.slice(0, copy(buf[..], rb.@out.slice(p)));
    rb.@out = rb.@out.slice(0, p);
    for (padd--; padd >= 0; padd--) {
        info = add[padd];
        Ꮡrb.insertUnsafe(inputBytes(cp), 0, info);
        cp = cp.slice(info.size);
    }
}

} // end norm_package
