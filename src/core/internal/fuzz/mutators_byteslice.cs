// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using binary = encoding.binary_package;
using encoding;

partial class fuzz_package {

// byteSliceRemoveBytes removes a random chunk of bytes from b.
internal static slice<byte> byteSliceRemoveBytes(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) <= 1) {
        return default!;
    }
    nint pos0 = m.rand(len(b));
    nint pos1 = pos0 + m.chooseLen(len(b) - pos0);
    copy(b.slice(pos0), b.slice(pos1));
    b = b.slice(0, len(b) - (pos1 - pos0));
    return b;
}

// byteSliceInsertRandomBytes inserts a chunk of random bytes into b at a random
// position.
internal static slice<byte> byteSliceInsertRandomBytes(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    nint pos = m.rand(len(b) + 1);
    nint n = m.chooseLen(1024);
    if (len(b) + n >= builtin.cap(b)) {
        return default!;
    }
    b = b.slice(0, len(b) + n);
    copy(b.slice(pos + n), b.slice(pos));
    for (nint i = 0; i < n; i++) {
        b[pos + i] = (byte)m.rand(256);
    }
    return b;
}

// byteSliceDuplicateBytes duplicates a chunk of bytes in b and inserts it into
// a random position.
internal static slice<byte> byteSliceDuplicateBytes(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) <= 1) {
        return default!;
    }
    nint src = m.rand(len(b));
    nint dst = m.rand(len(b));
    while (dst == src) {
        dst = m.rand(len(b));
    }
    nint n = m.chooseLen(len(b) - src);
    // Use the end of the slice as scratch space to avoid doing an
    // allocation. If the slice is too small abort and try something
    // else.
    if (len(b) + (n * 2) >= builtin.cap(b)) {
        return default!;
    }
    nint end = len(b);
    // Increase the size of b to fit the duplicated block as well as
    // some extra working space
    b = b.slice(0, end + (n * 2));
    // Copy the block of bytes we want to duplicate to the end of the
    // slice
    copy(b.slice(end + n), b.slice(src, src + n));
    // Shift the bytes after the splice point n positions to the right
    // to make room for the new block
    copy(b.slice(dst + n, end + n), b.slice(dst, end));
    // Insert the duplicate block into the splice point
    copy(b.slice(dst), b.slice(end + n));
    b = b.slice(0, end + n);
    return b;
}

// byteSliceOverwriteBytes overwrites a chunk of b with another chunk of b.
internal static slice<byte> byteSliceOverwriteBytes(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) <= 1) {
        return default!;
    }
    nint src = m.rand(len(b));
    nint dst = m.rand(len(b));
    while (dst == src) {
        dst = m.rand(len(b));
    }
    nint n = m.chooseLen(len(b) - src - 1);
    copy(b.slice(dst), b.slice(src, src + n));
    return b;
}

// byteSliceBitFlip flips a random bit in a random byte in b.
internal static slice<byte> byteSliceBitFlip(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) == 0) {
        return default!;
    }
    nint pos = m.rand(len(b));
    b[pos] ^= (byte)(((byte)1).Lsh((nuint)m.rand(8)));
    return b;
}

// byteSliceXORByte XORs a random byte in b with a random value.
internal static slice<byte> byteSliceXORByte(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) == 0) {
        return default!;
    }
    nint pos = m.rand(len(b));
    // In order to avoid a no-op (where the random value matches
    // the existing value), use XOR instead of just setting to
    // the random value.
    b[pos] ^= (byte)((byte)(1 + m.rand(255)));
    return b;
}

// byteSliceSwapByte swaps two random bytes in b.
internal static slice<byte> byteSliceSwapByte(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) <= 1) {
        return default!;
    }
    nint src = m.rand(len(b));
    nint dst = m.rand(len(b));
    while (dst == src) {
        dst = m.rand(len(b));
    }
    (b[src], b[dst]) = (b[dst], b[src]);
    return b;
}

// byteSliceArithmeticUint8 adds/subtracts from a random byte in b.
internal static slice<byte> byteSliceArithmeticUint8(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) == 0) {
        return default!;
    }
    nint pos = m.rand(len(b));
    var v = (byte)(m.rand(35) + 1);
    if (m.r.@bool()){
        b[pos] += v;
    } else {
        b[pos] -= v;
    }
    return b;
}

// byteSliceArithmeticUint16 adds/subtracts from a random uint16 in b.
internal static slice<byte> byteSliceArithmeticUint16(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) < 2) {
        return default!;
    }
    var v = (uint16)(m.rand(35) + 1);
    if (m.r.@bool()) {
        v = (uint16)(0 - v);
    }
    nint pos = m.rand(len(b) - 1);
    var enc = m.randByteOrder();
    enc.PutUint16(b.slice(pos), (uint16)(enc.Uint16(b.slice(pos)) + v));
    return b;
}

// byteSliceArithmeticUint32 adds/subtracts from a random uint32 in b.
internal static slice<byte> byteSliceArithmeticUint32(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) < 4) {
        return default!;
    }
    var v = (uint32)(m.rand(35) + 1);
    if (m.r.@bool()) {
        v = 0 - v;
    }
    nint pos = m.rand(len(b) - 3);
    var enc = m.randByteOrder();
    enc.PutUint32(b.slice(pos), enc.Uint32(b.slice(pos)) + v);
    return b;
}

// byteSliceArithmeticUint64 adds/subtracts from a random uint64 in b.
internal static slice<byte> byteSliceArithmeticUint64(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) < 8) {
        return default!;
    }
    var v = (uint64)(m.rand(35) + 1);
    if (m.r.@bool()) {
        v = 0 - v;
    }
    nint pos = m.rand(len(b) - 7);
    var enc = m.randByteOrder();
    enc.PutUint64(b.slice(pos), enc.Uint64(b.slice(pos)) + v);
    return b;
}

// byteSliceOverwriteInterestingUint8 overwrites a random byte in b with an interesting
// value.
internal static slice<byte> byteSliceOverwriteInterestingUint8(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) == 0) {
        return default!;
    }
    nint pos = m.rand(len(b));
    b[pos] = (byte)interesting8[m.rand(len(interesting8))];
    return b;
}

// byteSliceOverwriteInterestingUint16 overwrites a random uint16 in b with an interesting
// value.
internal static slice<byte> byteSliceOverwriteInterestingUint16(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) < 2) {
        return default!;
    }
    nint pos = m.rand(len(b) - 1);
    var v = (uint16)interesting16[m.rand(len(interesting16))];
    m.randByteOrder().PutUint16(b.slice(pos), v);
    return b;
}

// byteSliceOverwriteInterestingUint32 overwrites a random uint16 in b with an interesting
// value.
internal static slice<byte> byteSliceOverwriteInterestingUint32(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) < 4) {
        return default!;
    }
    nint pos = m.rand(len(b) - 3);
    var v = (uint32)interesting32[m.rand(len(interesting32))];
    m.randByteOrder().PutUint32(b.slice(pos), v);
    return b;
}

// byteSliceInsertConstantBytes inserts a chunk of constant bytes into a random position in b.
internal static slice<byte> byteSliceInsertConstantBytes(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) <= 1) {
        return default!;
    }
    nint dst = m.rand(len(b));
    // TODO(rolandshoemaker,katiehockman): 4096 was mainly picked
    // randomly. We may want to either pick a much larger value
    // (AFL uses 32768, paired with a similar impl to chooseLen
    // which biases towards smaller lengths that grow over time),
    // or set the max based on characteristics of the corpus
    // (libFuzzer sets a min/max based on the min/max size of
    // entries in the corpus and then picks uniformly from
    // that range).
    nint n = m.chooseLen(4096);
    if (len(b) + n >= builtin.cap(b)) {
        return default!;
    }
    b = b.slice(0, len(b) + n);
    copy(b.slice(dst + n), b.slice(dst));
    var rb = (byte)m.rand(256);
    for (nint i = dst; i < dst + n; i++) {
        b[i] = rb;
    }
    return b;
}

// byteSliceOverwriteConstantBytes overwrites a chunk of b with constant bytes.
internal static slice<byte> byteSliceOverwriteConstantBytes(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) <= 1) {
        return default!;
    }
    nint dst = m.rand(len(b));
    nint n = m.chooseLen(len(b) - dst);
    var rb = (byte)m.rand(256);
    for (nint i = dst; i < dst + n; i++) {
        b[i] = rb;
    }
    return b;
}

// byteSliceShuffleBytes shuffles a chunk of bytes in b.
internal static slice<byte> byteSliceShuffleBytes(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) <= 1) {
        return default!;
    }
    nint dst = m.rand(len(b));
    nint n = m.chooseLen(len(b) - dst);
    if (n <= 2) {
        return default!;
    }
    // Start at the end of the range, and iterate backwards
    // to dst, swapping each element with another element in
    // dst:dst+n (Fisher-Yates shuffle).
    for (nint i = n - 1; i > 0; i--) {
        nint j = m.rand(i + 1);
        (b[dst + i], b[dst + j]) = (b[dst + j], b[dst + i]);
    }
    return b;
}

// byteSliceSwapBytes swaps two chunks of bytes in b.
internal static slice<byte> byteSliceSwapBytes(ж<mutator> Ꮡm, slice<byte> b) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (len(b) <= 1) {
        return default!;
    }
    nint src = m.rand(len(b));
    nint dst = m.rand(len(b));
    while (dst == src) {
        dst = m.rand(len(b));
    }
    // Choose the random length as len(b) - max(src, dst)
    // so that we don't attempt to swap a chunk that extends
    // beyond the end of the slice
    nint max = dst;
    if (src > max) {
        max = src;
    }
    nint n = m.chooseLen(len(b) - max - 1);
    // Check that neither chunk intersect, so that we don't end up
    // duplicating parts of the input, rather than swapping them
    if (src > dst && dst + n >= src || dst > src && src + n >= dst) {
        return default!;
    }
    // Use the end of the slice as scratch space to avoid doing an
    // allocation. If the slice is too small abort and try something
    // else.
    if (len(b) + n >= builtin.cap(b)) {
        return default!;
    }
    nint end = len(b);
    b = b.slice(0, end + n);
    copy(b.slice(end), b.slice(dst, dst + n));
    copy(b.slice(dst), b.slice(src, src + n));
    copy(b.slice(src), b.slice(end));
    b = b.slice(0, end);
    return b;
}

} // end fuzz_package
