// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package saferio provides I/O functions that avoid allocating large
// amounts of memory unnecessarily. This is intended for packages that
// read data from an [io.Reader] where the size is part of the input
// data but the input may be corrupt, or may be provided by an
// untrustworthy attacker.
namespace go.@internal;

using io = io_package;
using @unsafe = unsafe_package;

partial class saferio_package {

// chunk is an arbitrary limit on how much memory we are willing
// to allocate without concern.
internal static UntypedInt chunk => /* 10 << 20 */ 10485760; // 10M

// ReadData reads n bytes from the input stream, but avoids allocating
// all n bytes if n is large. This avoids crashing the program by
// allocating all n bytes in cases where n is incorrect.
//
// The error is io.EOF only if no bytes were read.
// If an io.EOF happens after reading some but not all the bytes,
// ReadData returns io.ErrUnexpectedEOF.
public static (slice<byte>, error) ReadData(io.Reader r, uint64 n) {
    if ((int64)n < 0 || n != (uint64)(nint)n) {
        // n is too large to fit in int, so we can't allocate
        // a buffer large enough. Treat this as a read failure.
        return (default!, io.ErrUnexpectedEOF);
    }
    if (n < chunk) {
        var bufΔ1 = new slice<byte>((nint)(n));
        var (_, err) = io.ReadFull(r, bufΔ1);
        if (err != default!) {
            return (default!, err);
        }
        return (bufΔ1, default!);
    }
    slice<byte> buf = default!;
    var buf1 = new slice<byte>(chunk);
    while (n > 0) {
        var next = n;
        if (next > chunk) {
            next = chunk;
        }
        var (_, err) = io.ReadFull(r, buf1.slice(0, (nint)(next)));
        if (err != default!) {
            if (len(buf) > 0 && AreEqual(err, io.EOF)) {
                err = io.ErrUnexpectedEOF;
            }
            return (default!, err);
        }
        buf = appendꓸꓸꓸ(buf, buf1.slice(0, (nint)(next)));
        n -= next;
    }
    return (buf, default!);
}

// ReadDataAt reads n bytes from the input stream at off, but avoids
// allocating all n bytes if n is large. This avoids crashing the program
// by allocating all n bytes in cases where n is incorrect.
public static (slice<byte>, error) ReadDataAt(io.ReaderAt r, uint64 n, int64 off) {
    if ((int64)n < 0 || n != (uint64)(nint)n) {
        // n is too large to fit in int, so we can't allocate
        // a buffer large enough. Treat this as a read failure.
        return (default!, io.ErrUnexpectedEOF);
    }
    if (n < chunk) {
        var bufΔ1 = new slice<byte>((nint)(n));
        var (_, err) = r.ReadAt(bufΔ1, off);
        if (err != default!) {
            // io.SectionReader can return EOF for n == 0,
            // but for our purposes that is a success.
            if (!AreEqual(err, io.EOF) || n > 0) {
                return (default!, err);
            }
        }
        return (bufΔ1, default!);
    }
    slice<byte> buf = default!;
    var buf1 = new slice<byte>(chunk);
    while (n > 0) {
        var next = n;
        if (next > chunk) {
            next = chunk;
        }
        var (_, err) = r.ReadAt(buf1.slice(0, (nint)(next)), off);
        if (err != default!) {
            return (default!, err);
        }
        buf = appendꓸꓸꓸ(buf, buf1.slice(0, (nint)(next)));
        n -= next;
        off += (int64)next;
    }
    return (buf, default!);
}

// SliceCapWithSize returns the capacity to use when allocating a slice.
// After the slice is allocated with the capacity, it should be
// built using append. This will avoid allocating too much memory
// if the capacity is large and incorrect.
//
// A negative result means that the value is always too big.
public static nint SliceCapWithSize(uint64 size, uint64 c) {
    if ((int64)c < 0 || c != (uint64)(nint)c) {
        return -1;
    }
    if (size > 0 && c > (18446744073709551615UL) / size) {
        return -1;
    }
    if (c * size > chunk) {
        c = (uint64)chunk / size;
        if (c == 0) {
            c = 1;
        }
    }
    return (nint)c;
}

// SliceCap is like SliceCapWithSize but using generics.
public static nint SliceCap<E>(uint64 c) {
    E v = GoZero<E>();
    var size = (uint64)@unsafe.Sizeof(v);
    return SliceCapWithSize(size, c);
}

} // end saferio_package
