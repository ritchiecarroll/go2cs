// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bufio = bufio_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;

partial class lzw_package {

// A writer is a buffered, flushable writer.
partial interface writer :
    io.ByteWriter
{
    error Flush();
}

internal static UntypedInt maxCode => /* 1<<12 - 1 */ 4095;
internal static UntypedInt invalidCode => /* 1<<32 - 1 */ 4294967295;
internal static UntypedInt tableSize => /* 4 * 1 << 12 */ 16384;
internal static UntypedInt tableMask => /* tableSize - 1 */ 16383;
internal static UntypedInt invalidEntry => 0;

// Writer is an LZW compressor. It writes the compressed form of the data
// to an underlying writer (see [NewWriter]).
partial struct Writer {
    // w is the writer that compressed bytes are written to.
    internal writer w;
    // litWidth is the width in bits of literal codes.
    internal nuint litWidth;
    // order, write, bits, nBits and width are the state for
    // converting a code stream into a byte stream.
    internal Order order;
    internal Func<ж<Writer>, uint32, error> write;
    internal nuint nBits;
    internal nuint width;
    internal uint32 bits;
    // hi is the code implied by the next code emission.
    // overflow is the code at which hi overflows the code width.
    internal uint32 hi, overflow;
    // savedCode is the accumulated code at the end of the most recent Write
    // call. It is equal to invalidCode if there was no such call.
    internal uint32 savedCode;
    // err is the first error encountered during writing. Closing the writer
    // will make any future Write calls return errClosed
    internal error err;
    // table is the hash table from 20-bit keys to 12-bit values. Each table
    // entry contains key<<12|val and collisions resolve by linear probing.
    // The keys consist of a 12-bit code prefix and an 8-bit byte suffix.
    // The values are a 12-bit code.
    internal array<uint32> table = new(tableSize);
}

// writeLSB writes the code c for "Least Significant Bits first" data.
internal static error writeLSB(this ref Writer w, uint32 c) {
    w.bits |= (uint32)(c.Lsh(w.nBits));
    w.nBits += w.width;
    while (w.nBits >= 8) {
        {
            var err = w.w.WriteByte((uint8)w.bits); if (err != default!) {
                return err;
            }
        }
        w.bits >>= (int)(8);
        w.nBits -= 8;
    }
    return default!;
}

// writeMSB writes the code c for "Most Significant Bits first" data.
internal static error writeMSB(this ref Writer w, uint32 c) {
    w.bits |= (uint32)(c.Lsh((32 - w.width - w.nBits)));
    w.nBits += w.width;
    while (w.nBits >= 8) {
        {
            var err = w.w.WriteByte((uint8)((w.bits >> (int)(24)))); if (err != default!) {
                return err;
            }
        }
        w.bits <<= (int)(8);
        w.nBits -= 8;
    }
    return default!;
}

// errOutOfCodes is an internal error that means that the writer has run out
// of unused codes and a clear code needs to be sent next.
internal static error errOutOfCodes = errors.New("lzw: out of codes"u8);

// incHi increments e.hi and checks for both overflow and running out of
// unused codes. In the latter case, incHi sends a clear code, resets the
// writer state and returns errOutOfCodes.
internal static error incHi(this ж<Writer> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    w.hi++;
    if (w.hi == w.overflow) {
        w.width++;
        w.overflow <<= (int)(1);
    }
    if (w.hi == maxCode) {
        var clear = ((uint32)1).Lsh(w.litWidth);
        {
            var err = w.write(Ꮡw, clear); if (err != default!) {
                return err;
            }
        }
        w.width = w.litWidth + 1;
        w.hi = clear + 1;
        w.overflow = (clear << (int)(1));
        foreach (var (i, _) in w.table) {
            w.table[i] = invalidEntry;
        }
        return errOutOfCodes;
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lzwInputByteTooLargeForˢ = "lzw: input byte too large for the litWidth"u8;

// Write writes a compressed representation of p to w's underlying writer.
public static (nint n, error err) Write(this ж<Writer> Ꮡw, slice<byte> p) {
    nint n = default!;

    ref var w = ref Ꮡw.DerefOrNull();
    if (w.err != default!) {
        return (0, w.err);
    }
    if (len(p) == 0) {
        return (0, default!);
    }
    {
        var maxLit = (uint8)(((uint8)1).Lsh(w.litWidth) - 1); if (maxLit != 0xff) {
            foreach (var (_, x) in p) {
                if (x > maxLit) {
                    w.err = errors.New(lzwInputByteTooLargeForˢ);
                    return (0, w.err);
                }
            }
        }
    }
    n = len(p);
    var code = w.savedCode;
    if (code == invalidCode) {
        // This is the first write; send a clear code.
        // https://www.w3.org/Graphics/GIF/spec-gif89a.txt Appendix F
        // "Variable-Length-Code LZW Compression" says that "Encoders should
        // output a Clear code as the first code of each image data stream".
        //
        // LZW compression isn't only used by GIF, but it's cheap to follow
        // that directive unconditionally.
        var clear = ((uint32)1).Lsh(w.litWidth);
        {
            var errΔ1 = w.write(Ꮡw, clear); if (errΔ1 != default!) {
                return (0, errΔ1);
            }
        }
        // After the starting clear code, the next code sent (for non-empty
        // input) is always a literal code.
        (code, p) = ((uint32)p[0], p[1..]);
    }
loop:
    foreach (var (_, x) in p) {
        var literal = (uint32)x;
        var key = (uint32)((code << (int)(8)) | literal);
        // If there is a hash table hit for this key then we continue the loop
        // and do not emit a code yet.
        var hash = (uint32)(((uint32)((key >> (int)(12)) ^ key)) & (uint32)tableMask);
        for (var (h, t) = (hash, w.table[hash]); t != invalidEntry; ) {
            if (key == (t >> (int)(12))) {
                code = (uint32)(t & (uint32)maxCode);
                goto continue_loop;
            }
            h = (uint32)((h + 1) & (uint32)tableMask);
            t = w.table[h];
        }
        // Otherwise, write the current code, and literal becomes the start of
        // the next emitted code.
        {
            w.err = w.write(Ꮡw, code); if (w.err != default!) {
                return (0, w.err);
            }
        }
        code = literal;
        // Increment e.hi, the next implied code. If we run out of codes, reset
        // the writer state (including clearing the hash table) and continue.
        {
            var err1 = Ꮡw.incHi(); if (err1 != default!) {
                if (AreEqual(err1, errOutOfCodes)) {
                    continue;
                }
                w.err = err1;
                return (0, w.err);
            }
        }
        // Otherwise, insert key -> e.hi into the map that e.table represents.
        while (ᐧ) {
            if (w.table[hash] == invalidEntry) {
                w.table[hash] = (uint32)(((key << (int)(12))) | w.hi);
                break;
            }
            hash = (uint32)((hash + 1) & (uint32)tableMask);
        }
continue_loop:;
    }
break_loop:;
    w.savedCode = code;
    return (n, default!);
}

// Close closes the [Writer], flushing any pending output. It does not close
// w's underlying writer.
public static error Close(this ж<Writer> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    if (w.err != default!) {
        if (AreEqual(w.err, errClosed)) {
            return default!;
        }
        return w.err;
    }
    // Make any future calls to Write return errClosed.
    w.err = errClosed;
    // Write the savedCode if valid.
    if (w.savedCode != invalidCode){
        {
            var err = w.write(Ꮡw, w.savedCode); if (err != default!) {
                return err;
            }
        }
        {
            var err = Ꮡw.incHi(); if (err != default! && !AreEqual(err, errOutOfCodes)) {
                return err;
            }
        }
    } else {
        // Write the starting clear code, as w.Write did not.
        var clear = ((uint32)1).Lsh(w.litWidth);
        {
            var err = w.write(Ꮡw, clear); if (err != default!) {
                return err;
            }
        }
    }
    // Write the eof code.
    var eof = ((uint32)1).Lsh(w.litWidth) + 1;
    {
        var err = w.write(Ꮡw, eof); if (err != default!) {
            return err;
        }
    }
    // Write the final bits.
    if (w.nBits > 0) {
        if (w.order == MSB) {
            w.bits >>= (int)(24);
        }
        {
            var err = w.w.WriteByte((uint8)w.bits); if (err != default!) {
                return err;
            }
        }
    }
    return w.w.Flush();
}

// Reset clears the [Writer]'s state and allows it to be reused again
// as a new [Writer].
public static void Reset(this ref Writer w, io.Writer dst, Order order, nint litWidth) {
    w = new Writer(nil);
    w.init(dst, order, litWidth);
}

// NewWriter creates a new [io.WriteCloser].
// Writes to the returned [io.WriteCloser] are compressed and written to w.
// It is the caller's responsibility to call Close on the WriteCloser when
// finished writing.
// The number of bits to use for literal codes, litWidth, must be in the
// range [2,8] and is typically 8. Input bytes must be less than 1<<litWidth.
//
// It is guaranteed that the underlying type of the returned [io.WriteCloser]
// is a *[Writer].
public static io.WriteCloser NewWriter(io.Writer w, Order order, nint litWidth) {
    return new WriterжWriteCloser(newWriter(w, order, litWidth));
}

internal static ж<Writer> newWriter(io.Writer dst, Order order, nint litWidth) {
    var w = @new<Writer>();
    w.init(dst, order, litWidth);
    return w;
}

internal static void init(this ref Writer w, io.Writer dst, Order order, nint litWidth) {
    var exprᴛ1 = order;
    if (exprᴛ1 == LSB) {
        w.write = ((Func<ж<Writer>, uint32, error>)(writeLSB));
    }
    else if (exprᴛ1 == MSB) {
        w.write = ((Func<ж<Writer>, uint32, error>)(writeMSB));
    }
    else { /* default: */
        w.err = errors.New(lzwUnknownOrderˢ);
        return;
    }

    if (litWidth < 2 || 8 < litWidth) {
        w.err = fmt.Errorf("lzw: litWidth %d out of range"u8, litWidth);
        return;
    }
    var (bw, ok) = dst._<writer>(ᐧ);
    if (!ok && dst != default!) {
        bw = new bufio_Writerжwriter(bufio.NewWriter(dst));
    }
    w.w = bw;
    nuint lw = (nuint)litWidth;
    w.order = order;
    w.width = 1 + lw;
    w.litWidth = lw;
    w.hi = ((uint32)1).Lsh(lw) + 1;
    w.overflow = ((uint32)1).Lsh((lw + 1));
    w.savedCode = invalidCode;
}

} // end lzw_package
