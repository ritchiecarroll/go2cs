// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using errors = errors_package;

partial class trace_package {

// maxVarintLenN is the maximum length of a varint-encoded N-bit integer.
internal static UntypedInt maxVarintLen64 => 10;

internal static error errOverflow = errors.New("binary: varint overflows a 64-bit integer"u8);
internal static error errEOB = errors.New("binary: end of buffer"u8);

// TODO deduplicate this function.
internal static (uint64, nint, error) readUvarint(slice<byte> b) {
    uint64 x = default!;
    nuint s = default!;
    byte byt = default!;
    for (nint i = 0; i < maxVarintLen64 && i < len(b); i++) {
        byt = b[i];
        if (byt < 0x80) {
            if (i == (nint)(maxVarintLen64 - 1) && byt > 1) {
                return (x, i, errOverflow);
            }
            return ((uint64)(x | ((uint64)byt).Lsh(s)), i + 1, default!);
        }
        x |= (uint64)(((uint64)((byte)(byt & 0x7f))).Lsh(s));
        s += 7;
    }
    return (x, len(b), errOverflow);
}

// putUvarint encodes a uint64 into buf and returns the number of bytes written.
// If the buffer is too small, PutUvarint will panic.
// TODO deduplicate this function.
internal static nint putUvarint(slice<byte> buf, uint64 x) {
    nint i = 0;
    while (x >= 0x80) {
        buf[i] = (byte)((byte)x | 0x80);
        x >>= (int)(7);
        i++;
    }
    buf[i] = (byte)x;
    return i + 1;
}

} // end trace_package
