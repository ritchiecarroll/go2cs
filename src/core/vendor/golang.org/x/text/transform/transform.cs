// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package transform provides reader and writer wrappers that transform the
// bytes passing through as well as various transformations. Example
// transformations provided by other packages include normalization and
// conversion between character sets.
namespace go.vendor.golang.org.x.text;

// import "golang.org/x/text/transform"
using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using utf8 = unicode.utf8_package;
using unicode;
using ꓸꓸꓸTransformer = Span<transform_package.Transformer>;

partial class transform_package {

public static error ErrShortDst = errors.New("transform: short destination buffer"u8);
public static error ErrShortSrc = errors.New("transform: short source buffer"u8);
public static error ErrEndOfSpan = errors.New("transform: input and output are not identical"u8);
internal static error errInconsistentByteCount = errors.New("transform: inconsistent byte count returned"u8);
internal static error errShortInternal = errors.New("transform: short internal buffer"u8);

// Transformer transforms bytes.
partial interface Transformer {
    // Transform writes to dst the transformed bytes read from src, and
    // returns the number of dst bytes written and src bytes read. The
    // atEOF argument tells whether src represents the last bytes of the
    // input.
    //
    // Callers should always process the nDst bytes produced and account
    // for the nSrc bytes consumed before considering the error err.
    //
    // A nil error means that all of the transformed bytes (whether freshly
    // transformed from src or left over from previous Transform calls)
    // were written to dst. A nil error can be returned regardless of
    // whether atEOF is true. If err is nil then nSrc must equal len(src);
    // the converse is not necessarily true.
    //
    // ErrShortDst means that dst was too short to receive all of the
    // transformed bytes. ErrShortSrc means that src had insufficient data
    // to complete the transformation. If both conditions apply, then
    // either error may be returned. Other than the error conditions listed
    // here, implementations are free to report other errors that arise.
    (nint nDst, nint nSrc, error err) Transform(slice<byte> dst, slice<byte> src, bool atEOF);
    // Reset resets the state and allows a Transformer to be reused.
    void Reset();
}

// SpanningTransformer extends the Transformer interface with a Span method
// that determines how much of the input already conforms to the Transformer.
partial interface SpanningTransformer :
    Transformer
{
    // Span returns a position in src such that transforming src[:n] results in
    // identical output src[:n] for these bytes. It does not necessarily return
    // the largest such n. The atEOF argument tells whether src represents the
    // last bytes of the input.
    //
    // Callers should always account for the n bytes consumed before
    // considering the error err.
    //
    // A nil error means that all input bytes are known to be identical to the
    // output produced by the Transformer. A nil error can be returned
    // regardless of whether atEOF is true. If err is nil, then n must
    // equal len(src); the converse is not necessarily true.
    //
    // ErrEndOfSpan means that the Transformer output may differ from the
    // input after n bytes. Note that n may be len(src), meaning that the output
    // would contain additional bytes after otherwise identical output.
    // ErrShortSrc means that src had insufficient data to determine whether the
    // remaining bytes would change. Other than the error conditions listed
    // here, implementations are free to report other errors that arise.
    //
    // Calling Span can modify the Transformer state as a side effect. In
    // effect, it does the transformation just as calling Transform would, only
    // without copying to a destination buffer and only up to a point it can
    // determine the input and output bytes are the same. This is obviously more
    // limited than calling Transform, but can be more efficient in terms of
    // copying and allocating buffers. Calls to Span and Transform may be
    // interleaved.
    (nint n, error err) Span(slice<byte> src, bool atEOF);
}

// NopResetter can be embedded by implementations of Transformer to add a nop
// Reset method.
partial struct NopResetter {
}

// Reset implements the Reset method of the Transformer interface.
public static void Reset(this NopResetter _) {
}

// Reader wraps another io.Reader by transforming the bytes read.
partial struct Reader {
    internal io.Reader r;
    internal Transformer t;
    internal error err;
    // dst[dst0:dst1] contains bytes that have been transformed by t but
    // not yet copied out via Read.
    internal slice<byte> dst;
    internal nint dst0, dst1;
    // src[src0:src1] contains bytes that have been read from r but not
    // yet transformed through t.
    internal slice<byte> src;
    internal nint src0, src1;
    // transformComplete is whether the transformation is complete,
    // regardless of whether or not it was successful.
    internal bool transformComplete;
}

internal static UntypedInt defaultBufSize => 4096;

// NewReader returns a new Reader that wraps r by transforming the bytes read
// via t. It calls Reset on t.
public static ж<Reader> NewReader(io.Reader r, Transformer t) {
    t.Reset();
    return Ꮡ(new Reader(
        r: r,
        t: t,
        dst: new slice<byte>(defaultBufSize),
        src: new slice<byte>(defaultBufSize)
    ));
}

// Read implements the io.Reader interface.
public static (nint, error) Read(this ref Reader r, slice<byte> p) {
    nint n = 0;
    var err = ((error)default!);
    while (ᐧ) {
        // Copy out any transformed bytes and return the final error if we are done.
        if (r.dst0 != r.dst1){
            n = copy(p, r.dst.slice(r.dst0, r.dst1));
            r.dst0 += n;
            if (r.dst0 == r.dst1 && r.transformComplete) {
                return (n, r.err);
            }
            return (n, default!);
        } else 
        if (r.transformComplete) {
            return (0, r.err);
        }
        // Try to transform some source bytes, or to flush the transformer if we
        // are out of source bytes. We do this even if r.r.Read returned an error.
        // As the io.Reader documentation says, "process the n > 0 bytes returned
        // before considering the error".
        if (r.src0 != r.src1 || r.err != default!) {
            r.dst0 = 0;
            (r.dst1, n, err) = r.t.Transform(r.dst, r.src.slice(r.src0, r.src1), AreEqual(r.err, io.EOF));
            r.src0 += n;
            switch (ᐧ) {
            case {} when err == default!: {
                if (r.src0 != r.src1) {
                    r.err = errInconsistentByteCount;
                }
                r.transformComplete = r.err != default!;
                continue;
                break;
            }
            case {} when AreEqual(err, ErrShortDst) && (r.dst1 != 0 || n != 0): {
                continue;
                break;
            }
            case {} when AreEqual(err, ErrShortSrc) && r.src1 - r.src0 != len(r.src) && r.err == default!: {
                break;
            }
            default: {
                r.transformComplete = true;
                if (r.err == default! || AreEqual(r.err, io.EOF)) {
                    // The Transform call was successful; we are complete if we
                    // cannot read more bytes into src.
                    // Make room in dst by copying out, and try again.
                    // Read more bytes into src via the code below, and try again.
                    // The reader error (r.err) takes precedence over the
                    // transformer error (err) unless r.err is nil or io.EOF.
                    r.err = err;
                }
                continue;
                break;
            }}

        }
        // Move any untransformed source bytes to the start of the buffer
        // and read more bytes.
        if (r.src0 != 0) {
            (r.src0, r.src1) = (0, copy(r.src, r.src.slice(r.src0, r.src1)));
        }
        (n, r.err) = r.r.Read(r.src.slice(r.src1));
        r.src1 += n;
    }
}

// TODO: implement ReadByte (and ReadRune??).

// Writer wraps another io.Writer by transforming the bytes read.
// The user needs to call Close to flush unwritten bytes that may
// be buffered.
partial struct Writer {
    internal io.Writer w;
    internal Transformer t;
    internal slice<byte> dst;
    // src[:n] contains bytes that have not yet passed through t.
    internal slice<byte> src;
    internal nint n;
}

// NewWriter returns a new Writer that wraps w by transforming the bytes written
// via t. It calls Reset on t.
public static ж<Writer> NewWriter(io.Writer w, Transformer t) {
    t.Reset();
    return Ꮡ(new Writer(
        w: w,
        t: t,
        dst: new slice<byte>(defaultBufSize),
        src: new slice<byte>(defaultBufSize)
    ));
}

// Write implements the io.Writer interface. If there are not enough
// bytes available to complete a Transform, the bytes will be buffered
// for the next write. Call Close to convert the remaining bytes.
public static (nint n, error err) Write(this ref Writer w, slice<byte> data) {
    nint n = default!;

    var src = data;
    if (w.n > 0) {
        // Append bytes from data to the last remainder.
        // TODO: limit the amount copied on first try.
        n = copy(w.src.slice(w.n), data);
        w.n += n;
        src = w.src.slice(0, w.n);
    }
    while (ᐧ) {
        var (nDst, nSrc, errΔ1) = w.t.Transform(w.dst, src, false);
        {
            var (_, werr) = w.w.Write(w.dst.slice(0, nDst)); if (werr != default!) {
                return (n, werr);
            }
        }
        src = src.slice(nSrc);
        if (w.n == 0){
            n += nSrc;
        } else 
        if (len(src) <= n) {
            // Enough bytes from w.src have been consumed. We make src point
            // to data instead to reduce the copying.
            w.n = 0;
            n -= len(src);
            src = data.slice(n);
            if (n < len(data) && (errΔ1 == default! || AreEqual(errΔ1, ErrShortSrc))) {
                continue;
            }
        }
        var exprᴛ1 = errΔ1;
        if (AreEqual(exprᴛ1, ErrShortDst)) {
            if (nDst > 0 || nSrc > 0) {
                // This error is okay as long as we are making progress.
                continue;
            }
        }
        else if (AreEqual(exprᴛ1, ErrShortSrc)) {
            if (len(src) < len(w.src)){
                nint m = copy(w.src, src);
                // If w.n > 0, bytes from data were already copied to w.src and n
                // was already set to the number of bytes consumed.
                if (w.n == 0) {
                    n += m;
                }
                w.n = m;
                errΔ1 = default!;
            } else 
            if (nDst > 0 || nSrc > 0) {
                // Not enough buffer to store the remainder. Keep processing as
                // long as there is progress. Without this case, transforms that
                // require a lookahead larger than the buffer may result in an
                // error. This is not something one may expect to be common in
                // practice, but it may occur when buffers are set to small
                // sizes during testing.
                continue;
            }
        }
        else if (AreEqual(exprᴛ1, default!)) {
            if (w.n > 0) {
                errΔ1 = errInconsistentByteCount;
            }
        }

        return (n, errΔ1);
    }
}

// Close implements the io.Closer interface.
public static error Close(this ref Writer w) {
    var src = w.src.slice(0, w.n);
    while (ᐧ) {
        var (nDst, nSrc, err) = w.t.Transform(w.dst, src, true);
        {
            var (_, werr) = w.w.Write(w.dst.slice(0, nDst)); if (werr != default!) {
                return werr;
            }
        }
        if (!AreEqual(err, ErrShortDst)) {
            return err;
        }
        src = src.slice(nSrc);
    }
}

partial struct nop {
    public partial ref NopResetter NopResetter { get; }
}

internal static (nint nDst, nint nSrc, error err) Transform(this nop _, slice<byte> dst, slice<byte> src, bool atEOF) {
    error err = default!;

    nint n = copy(dst, src);
    if (n < len(src)) {
        err = ErrShortDst;
    }
    return (n, n, err);
}

internal static (nint n, error err) Span(this nop _, slice<byte> src, bool atEOF) {
    return (len(src), default!);
}

partial struct discard {
    public partial ref NopResetter NopResetter { get; }
}

internal static (nint nDst, nint nSrc, error err) Transform(this discard _, slice<byte> dst, slice<byte> src, bool atEOF) {
    return (0, len(src), default!);
}

public static Transformer Discard = new discard(nil);
public static SpanningTransformer Nop = new nop(nil);

// chain is a sequence of links. A chain with N Transformers has N+1 links and
// N+1 buffers. Of those N+1 buffers, the first and last are the src and dst
// buffers given to chain.Transform and the middle N-1 buffers are intermediate
// buffers owned by the chain. The i'th link transforms bytes from the i'th
// buffer chain.link[i].b at read offset chain.link[i].p to the i+1'th buffer
// chain.link[i+1].b at write offset chain.link[i+1].n, for i in [0, N).
partial struct chain {
    internal slice<link> link;
    internal error err;
    // errStart is the index at which the error occurred plus 1. Processing
    // errStart at this level at the next call to Transform. As long as
    // errStart > 0, chain will not consume any more source bytes.
    internal nint errStart;
}

internal static void fatalError(this ref chain c, nint errIndex, error err) {
    {
        nint i = errIndex + 1; if (i > c.errStart) {
            c.errStart = i;
            c.err = err;
        }
    }
}

partial struct link {
    internal Transformer t;
    // b[p:n] holds the bytes to be transformed by t.
    internal slice<byte> b;
    internal nint p;
    internal nint n;
}

internal static slice<byte> src(this ref link l) {
    return l.b.slice(l.p, l.n);
}

internal static slice<byte> dst(this ref link l) {
    return l.b.slice(l.n);
}

// Chain returns a Transformer that applies t in sequence.
public static Transformer Chain(params ꓸꓸꓸTransformer tʗp) {
    var t = tʗp.sslice();

    if (len(t) == 0) {
        return new nop(nil);
    }
    var c = Ꮡ(new chain(link: new slice<link>(len(t) + 1)));
    foreach (var (i, tt) in t) {
        (~c).link[i].t = tt;
    }
    // Allocate intermediate buffers.
    var b = GoReflect.WithElemDims(new slice<array<byte>>(len(t) - 1, () => new(4096)), 4096);
    foreach (var (i, _) in b) {
        (~c).link[i + 1].b = b[i][..];
    }
    return new chainжTransformer(c);
}

// Reset resets the state of Chain. It calls Reset on all the Transformers.
internal static void Reset(this ref chain c) {
    foreach (var (i, l) in c.link) {
        if (l.t != default!) {
            l.t.Reset();
        }
        (c.link[i].p, c.link[i].n) = (0, 0);
    }
}

// TODO: make chain use Span (is going to be fun to implement!)

// Transform applies the transformers of c in sequence.
internal static (nint nDst, nint nSrc, error err) Transform(this ref chain c, slice<byte> dst, slice<byte> src, bool atEOF) {
    error err = default!;

    // Set up src and dst in the chain.
    var srcL = Ꮡ(c.link, 0);
    var dstL = Ꮡ(c.link, len(c.link) - 1);
    (srcL.Value.b, srcL.Value.p, srcL.Value.n) = (src, 0, len(src));
    (dstL.Value.b, dstL.Value.n) = (dst, 0);
    bool lastFull = default!;                // for detecting progress
    bool needProgress = default!;
    // i is the index of the next Transformer to apply, for i in [low, high].
    // low is the lowest index for which c.link[low] may still produce bytes.
    // high is the highest index for which c.link[high] has a Transformer.
    // The error returned by Transform determines whether to increase or
    // decrease i. We try to completely fill a buffer before converting it.
    for ((nint low, nint i, nint high) = (c.errStart, c.errStart, len(c.link) - 2); low <= i && i <= high; ) {
        var (@in, @out) = (Ꮡ(c.link, i), Ꮡ(c.link, i + 1));
        var (nDstΔ1, nSrcΔ1, err0) = (~@in).t.Transform(@out.dst(), @in.src(), atEOF && low == i);
        @out.Value.n += nDstΔ1;
        @in.Value.p += nSrcΔ1;
        if (i > 0 && (~@in).p == (~@in).n) {
            (@in.Value.p, @in.Value.n) = (0, 0);
        }
        (needProgress, lastFull) = (lastFull, false);
        var exprᴛ1 = err0;
        var matchᴛ1 = false;
        if (AreEqual(exprᴛ1, ErrShortDst)) { matchᴛ1 = true;
            if (i == high) {
                // Process the destination buffer next. Return if we are already
                // at the high index.
                return ((~dstL).n, (~srcL).p, ErrShortDst);
            }
            if ((~@out).n != 0) {
                i++;
                // If the Transformer at the next index is not able to process any
                // source bytes there is nothing that can be done to make progress
                // and the bytes will remain unprocessed. lastFull is used to
                // detect this and break out of the loop with a fatal error.
                lastFull = true;
                continue;
            }
            c.fatalError(i, // The destination buffer was too small, but is completely empty.
 // Return a fatal error as this transformation can never complete.
 errShortInternal);
        }
        else if (AreEqual(exprᴛ1, ErrShortSrc)) { matchᴛ1 = true;
            do {
                if (i == 0) {
                    // Save ErrShortSrc in err. All other errors take precedence.
                    err = ErrShortSrc;
                    break;
                }
                if (needProgress && nSrcΔ1 == 0 || (~@in).n - (~@in).p == len((~@in).b)) {
                    // Source bytes were depleted before filling up the destination buffer.
                    // Verify we made some progress, move the remaining bytes to the errStart
                    // and try to get more source bytes.
                    // There were not enough source bytes to proceed while the source
                    // buffer cannot hold any more bytes. Return a fatal error as this
                    // transformation can never complete.
                    c.fatalError(i, errShortInternal);
                    break;
                }
                (@in.Value.p, @in.Value.n) = (0, copy((~@in).b, // in.b is an internal buffer and we can make progress.
 @in.src()));
                fallthrough = true;
            } while (false);
        }
        if (fallthrough || !matchᴛ1 && AreEqual(exprᴛ1, default!)) {
            if (i > low) {
                // if i == low, we have depleted the bytes at index i or any lower levels.
                // In that case we increase low and i. In all other cases we decrease i to
                // fetch more bytes before proceeding to the next index.
                i--;
                continue;
            }
        }
        else if (!matchᴛ1) { /* default: */
            c.fatalError(i, err0);
        }

        // Exhausted level low or fatal error: increase low and continue
        // to process the bytes accepted so far.
        i++;
        low = i;
    }
    // If c.errStart > 0, this means we found a fatal error.  We will clear
    // all upstream buffers. At this point, no more progress can be made
    // downstream, as Transform would have bailed while handling ErrShortDst.
    if (c.errStart > 0) {
        for (nint i = 1; i < c.errStart; i++) {
            (c.link[i].p, c.link[i].n) = (0, 0);
        }
        (err, c.errStart, c.err) = (c.err, 0, default!);
    }
    return ((~dstL).n, (~srcL).p, err);
}

// Deprecated: Use runes.Remove instead.
public static Transformer RemoveFunc(Func<rune, bool> f) {
    return new removeFᴠTransformer(NilSafeDelegateConversion<removeF, Func<rune, bool>>(f));
}

internal delegate bool removeF(rune r);

internal static void Reset(this removeF _) {
}

// Transform implements the Transformer interface.
internal static (nint nDst, nint nSrc, error err) Transform(this removeF t, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default!;
    nint nSrc = default!;
    error err = default!;

    for ((var r, nint sz) = ((rune)0, 0); len(src) > 0; src = src.slice(sz)) {
        {
            r = (rune)src[0]; if (r < utf8.RuneSelf){
                sz = 1;
            } else {
                (r, sz) = utf8.DecodeRune(src);
                if (sz == 1) {
                    // Invalid rune.
                    if (!atEOF && !utf8.FullRune(src)) {
                        err = ErrShortSrc;
                        break;
                    }
                    // We replace illegal bytes with RuneError. Not doing so might
                    // otherwise turn a sequence of invalid UTF-8 into valid UTF-8.
                    // The resulting byte sequence may subsequently contain runes
                    // for which t(r) is true that were passed unnoticed.
                    if (!t(r)) {
                        if (nDst + 3 > len(dst)) {
                            err = ErrShortDst;
                            break;
                        }
                        nDst += copy(dst.slice(nDst), "\uFFFD"u8);
                    }
                    nSrc++;
                    continue;
                }
            }
        }
        if (!t(r)) {
            if (nDst + sz > len(dst)) {
                err = ErrShortDst;
                break;
            }
            nDst += copy(dst.slice(nDst), src.slice(0, sz));
        }
        nSrc += sz;
    }
    return (nDst, nSrc, err);
}

// grow returns a new []byte that is longer than b, and copies the first n bytes
// of b to the start of the new slice.
internal static slice<byte> grow(slice<byte> b, nint n) {
    nint m = len(b);
    if (m <= 32){
        m = 64;
    } else 
    if (m <= 256){
        m *= 2;
    } else {
        m += (m >> (int)(1));
    }
    var buf = new slice<byte>(m);
    copy(buf, b.slice(0, n));
    return buf;
}

internal static UntypedInt initialBufSize => 128;

// String returns a string with the result of converting s[:n] using t, where
// n <= len(s). If err == nil, n will be len(s). It calls Reset on t.
public static (@string result, nint n, error err) String(Transformer t, @string s) {
    error err = default!;

    t.Reset();
    if (s == ""u8) {
        // Fast path for the common case for empty input. Results in about a
        // 86% reduction of running time for BenchmarkStringLowerEmpty.
        {
            var (_, _, errΔ1) = t.Transform(default!, default!, true); if (errΔ1 == default!) {
                return ("", 0, default!);
            }
        }
    }
    // Allocate only once. Note that both dst and src escape when passed to
    // Transform.
    var buf = new byte[]{}.array(256);
    var dst = buf.slice(0, initialBufSize, initialBufSize);
    var src = buf[(int)(initialBufSize)..(int)(2 * initialBufSize)];
    // The input string s is transformed in multiple chunks (starting with a
    // chunk size of initialBufSize). nDst and nSrc are per-chunk (or
    // per-Transform-call) indexes, pDst and pSrc are overall indexes.
    nint nDst = 0;
    nint nSrc = 0;
    nint pDst = 0;
    nint pSrc = 0;
    // pPrefix is the length of a common prefix: the first pPrefix bytes of the
    // result will equal the first pPrefix bytes of s. It is not guaranteed to
    // be the largest such value, but if pPrefix, len(result) and len(s) are
    // all equal after the final transform (i.e. calling Transform with atEOF
    // being true returned nil error) then we don't need to allocate a new
    // result string.
    nint pPrefix = 0;
    while (ᐧ) {
        // Invariant: pDst == pPrefix && pSrc == pPrefix.
        nint nΔ1 = copy(src, s.slice(pSrc));
        (nDst, nSrc, err) = t.Transform(dst, src.slice(0, nΔ1), pSrc + nΔ1 == len(s));
        pDst += nDst;
        pSrc += nSrc;
        // TODO:  let transformers implement an optional Spanner interface, akin
        // to norm's QuickSpan. This would even allow us to avoid any allocation.
        if (!bytes.Equal(dst.slice(0, nDst), src.slice(0, nSrc))) {
            break;
        }
        pPrefix = pSrc;
        if (AreEqual(err, ErrShortDst)){
            // A buffer can only be short if a transformer modifies its input.
            break;
        } else 
        if (AreEqual(err, ErrShortSrc)){
            if (nSrc == 0) {
                // No progress was made.
                break;
            }
        } else 
        if (err != default! || pPrefix == len(s)) {
            // Equal so far and !atEOF, so continue checking.
            return (((@string)(s.slice(0, pPrefix))), pPrefix, err);
        }
    }
    // Post-condition: pDst == pPrefix + nDst && pSrc == pPrefix + nSrc.
    // We have transformed the first pSrc bytes of the input s to become pDst
    // transformed bytes. Those transformed bytes are discontiguous: the first
    // pPrefix of them equal s[:pPrefix] and the last nDst of them equal
    // dst[:nDst]. We copy them around, into a new dst buffer if necessary, so
    // that they become one contiguous slice: dst[:pDst].
    if (pPrefix != 0) {
        var newDst = dst;
        if (pDst > len(newDst)) {
            newDst = new slice<byte>(len(s) + nDst - nSrc);
        }
        copy(newDst.slice(pPrefix, pDst), dst.slice(0, nDst));
        copy(newDst.slice(0, pPrefix), s.slice(0, pPrefix));
        dst = newDst;
    }
    // Prevent duplicate Transform calls with atEOF being true at the end of
    // the input. Also return if we have an unrecoverable error.
    if ((err == default! && pSrc == len(s)) || (err != default! && !AreEqual(err, ErrShortDst) && !AreEqual(err, ErrShortSrc))) {
        return (((@string)(dst.slice(0, pDst))), pSrc, err);
    }
    // Transform the remaining input, growing dst and src buffers as necessary.
    while (ᐧ) {
        nint nΔ2 = copy(src, s.slice(pSrc));
        var atEOF = pSrc + nΔ2 == len(s);
        var (nDstΔ1, nSrcΔ1, errΔ2) = t.Transform(dst.slice(pDst), src.slice(0, nΔ2), atEOF);
        pDst += nDstΔ1;
        pSrc += nSrcΔ1;
        // If we got ErrShortDst or ErrShortSrc, do not grow as long as we can
        // make progress. This may avoid excessive allocations.
        if (AreEqual(errΔ2, ErrShortDst)){
            if (nDstΔ1 == 0) {
                dst = grow(dst, pDst);
            }
        } else 
        if (AreEqual(errΔ2, ErrShortSrc)){
            if (atEOF) {
                return (((@string)(dst.slice(0, pDst))), pSrc, errΔ2);
            }
            if (nSrcΔ1 == 0) {
                src = grow(src, 0);
            }
        } else 
        if (errΔ2 != default! || pSrc == len(s)) {
            return (((@string)(dst.slice(0, pDst))), pSrc, errΔ2);
        }
    }
}

// Bytes returns a new byte slice with the result of converting b[:n] using t,
// where n <= len(b). If err == nil, n will be len(b). It calls Reset on t.
public static (slice<byte> result, nint n, error err) Bytes(Transformer t, slice<byte> b) {
    return doAppend(t, 0, new slice<byte>(len(b)), b);
}

// Append appends the result of converting src[:n] using t to dst, where
// n <= len(src), If err == nil, n will be len(src). It calls Reset on t.
public static (slice<byte> result, nint n, error err) Append(Transformer t, slice<byte> dst, slice<byte> src) {
    if (len(dst) == cap(dst)) {
        nint nΔ1 = len(src) + len(dst); // It is okay for this to be 0.
        var b = new slice<byte>(nΔ1);
        dst = b.slice(0, copy(b, dst));
    }
    return doAppend(t, len(dst), dst.slice(0, cap(dst)), src);
}

internal static (slice<byte> result, nint n, error err) doAppend(Transformer t, nint pDst, slice<byte> dst, slice<byte> src) {
    t.Reset();
    nint pSrc = 0;
    while (ᐧ) {
        var (nDst, nSrc, errΔ1) = t.Transform(dst.slice(pDst), src.slice(pSrc), true);
        pDst += nDst;
        pSrc += nSrc;
        if (!AreEqual(errΔ1, ErrShortDst)) {
            return (dst.slice(0, pDst), pSrc, errΔ1);
        }
        // Grow the destination buffer, but do not grow as long as we can make
        // progress. This may avoid excessive allocations.
        if (nDst == 0) {
            dst = grow(dst, pDst);
        }
    }
}

} // end transform_package
