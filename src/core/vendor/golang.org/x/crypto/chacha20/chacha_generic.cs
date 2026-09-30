// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package chacha20 implements the ChaCha20 and XChaCha20 encryption algorithms
// as specified in RFC 8439 and draft-irtf-cfrg-xchacha-01.
namespace go.vendor.golang.org.x.crypto;

using cipher = go.crypto.cipher_package;
using binary = encoding.binary_package;
using errors = errors_package;
using bits = math.bits_package;
using alias = go.vendor.golang.org.x.crypto.@internal.alias_package;
using encoding;
using go.crypto;
using go.vendor.golang.org.x.crypto.@internal;
using math;

partial class chacha20_package {

public static UntypedInt KeySize => 32;
public static UntypedInt NonceSize => 12;
public static UntypedInt NonceSizeX => 24;

// Cipher is a stateful instance of ChaCha20 or XChaCha20 using a particular key
// and nonce. A *Cipher implements the cipher.Stream interface.
[GoType] partial struct Cipher {
    // The ChaCha20 state is 16 words: 4 constant, 8 of key, 1 of counter
    // (incremented after each block), and 3 of nonce.
    internal array<uint32> key = new(8);
    internal uint32 counter;
    internal array<uint32> nonce = new(3);
    // The last len bytes of buf are leftover key stream bytes from the previous
    // XORKeyStream invocation. The size of buf depends on how many blocks are
    // computed at a time by xorKeyStreamBlocks.
    internal array<byte> buf = new(bufSize);
    internal nint len;
    // overflow is set when the counter overflowed, no more blocks can be
    // generated, and the next XORKeyStream call should panic.
    internal bool overflow;
    // The counter-independent results of the first round are cached after they
    // are computed the first time.
    internal bool precompDone;
    internal uint32 p1, p5, p9, p13;
    internal uint32 p2, p6, p10, p14;
    internal uint32 p3, p7, p11, p15;
}

internal static cipher.Stream _ᴛ1ʗ = new CipherжStream(((ж<Cipher>)nil));

// NewUnauthenticatedCipher creates a new ChaCha20 stream cipher with the given
// 32 bytes key and a 12 or 24 bytes nonce. If a nonce of 24 bytes is provided,
// the XChaCha20 construction will be used. It returns an error if key or nonce
// have any other length.
//
// Note that ChaCha20, like all stream ciphers, is not authenticated and allows
// attackers to silently tamper with the plaintext. For this reason, it is more
// appropriate as a building block than as a standalone encryption mechanism.
// Instead, consider using package golang.org/x/crypto/chacha20poly1305.
public static (ж<Cipher>, error) NewUnauthenticatedCipher(slice<byte> key, slice<byte> nonce) {
    // This function is split into a wrapper so that the Cipher allocation will
    // be inlined, and depending on how the caller uses the return value, won't
    // escape to the heap.
    var c = Ꮡ(new Cipher(nil));
    return newUnauthenticatedCipher(c, key, nonce);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string chacha20WrongKeySizeˢ = "chacha20: wrong key size"u8;
private static readonly @string chacha20WrongNonceSizeˢ = "chacha20: wrong nonce size"u8;

internal static (ж<Cipher>, error) newUnauthenticatedCipher(ж<Cipher> Ꮡc, slice<byte> key, slice<byte> nonce) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (len(key) != KeySize) {
        return (default!, errors.New(chacha20WrongKeySizeˢ));
    }
    if (len(nonce) == NonceSizeX){
        // XChaCha20 uses the ChaCha20 core to mix 16 bytes of the nonce into a
        // derived key, allowing it to operate on a nonce of 24 bytes. See
        // draft-irtf-cfrg-xchacha-01, Section 2.3.
        (key, _) = HChaCha20(key, nonce[0..16]);
        var cNonce = new slice<byte>(NonceSize);
        copy(cNonce[4..12], nonce[16..24]);
        nonce = cNonce;
    } else 
    if (len(nonce) != NonceSize) {
        return (default!, errors.New(chacha20WrongNonceSizeˢ));
    }
    (key, nonce) = (key[..(int)(KeySize)], nonce[..(int)(NonceSize)]); // bounds check elimination hint
    c.key = new uint32[]{
        binary.LittleEndian.Uint32(key[0..4]),
        binary.LittleEndian.Uint32(key[4..8]),
        binary.LittleEndian.Uint32(key[8..12]),
        binary.LittleEndian.Uint32(key[12..16]),
        binary.LittleEndian.Uint32(key[16..20]),
        binary.LittleEndian.Uint32(key[20..24]),
        binary.LittleEndian.Uint32(key[24..28]),
        binary.LittleEndian.Uint32(key[28..32])
    }.array();
    c.nonce = new uint32[]{
        binary.LittleEndian.Uint32(nonce[0..4]),
        binary.LittleEndian.Uint32(nonce[4..8]),
        binary.LittleEndian.Uint32(nonce[8..12])
    }.array();
    return (Ꮡc, default!);
}

// The constant first 4 words of the ChaCha20 state.
internal const uint32 j0 = 0x61707865;      // expa

internal const uint32 j1 = 0x3320646e;      // nd 3

internal const uint32 j2 = 0x79622d32;      // 2-by

internal const uint32 j3 = 0x6b206574;      // te k

internal static UntypedInt blockSize => 64;

// quarterRound is the core of ChaCha20. It shuffles the bits of 4 state words.
// It's executed 4 times for each of the 20 ChaCha20 rounds, operating on all 16
// words each round, in columnar or diagonal groups of 4 at a time.
internal static (uint32, uint32, uint32, uint32) quarterRound(uint32 a, uint32 b, uint32 c, uint32 d) {
    a += b;
    d ^= (uint32)(a);
    d = bits.RotateLeft32(d, 16);
    c += d;
    b ^= (uint32)(c);
    b = bits.RotateLeft32(b, 12);
    a += b;
    d ^= (uint32)(a);
    d = bits.RotateLeft32(d, 8);
    c += d;
    b ^= (uint32)(c);
    b = bits.RotateLeft32(b, 7);
    return (a, b, c, d);
}

// SetCounter sets the Cipher counter. The next invocation of XORKeyStream will
// behave as if (64 * counter) bytes had been encrypted so far.
//
// To prevent accidental counter reuse, SetCounter panics if counter is less
// than the current value.
//
// Note that the execution time of XORKeyStream is not independent of the
// counter value.
[GoRecv] public static void SetCounter(this ref Cipher s, uint32 counter) {
    // Internally, s may buffer multiple blocks, which complicates this
    // implementation slightly. When checking whether the counter has rolled
    // back, we must use both s.counter and s.len to determine how many blocks
    // we have already output.
    var outputCounter = s.counter - (uint32)s.len / (uint32)blockSize;
    if (s.overflow || counter < outputCounter) {
        throw panic("chacha20: SetCounter attempted to rollback counter");
    }
    // In the general case, we set the new counter value and reset s.len to 0,
    // causing the next call to XORKeyStream to refill the buffer. However, if
    // we're advancing within the existing buffer, we can save work by simply
    // setting s.len.
    if (counter < s.counter){
        s.len = (nint)(s.counter - counter) * (nint)blockSize;
    } else {
        s.counter = counter;
        s.len = 0;
    }
}

// XORKeyStream XORs each byte in the given slice with a byte from the
// cipher's key stream. Dst and src must overlap entirely or not at all.
//
// If len(dst) < len(src), XORKeyStream will panic. It is acceptable
// to pass a dst bigger than src, and in that case, XORKeyStream will
// only update dst[:len(src)] and will not touch the rest of dst.
//
// Multiple calls to XORKeyStream behave as if the concatenation of
// the src buffers was passed in a single run. That is, Cipher
// maintains state and does not reset at each XORKeyStream call.
[GoRecv] public static void XORKeyStream(this ref Cipher s, slice<byte> dst, slice<byte> src) {
    if (len(src) == 0) {
        return;
    }
    if (len(dst) < len(src)) {
        throw panic("chacha20: output smaller than input");
    }
    dst = dst.slice(0, len(src));
    if (alias.InexactOverlap(dst, src)) {
        throw panic("chacha20: invalid buffer overlap");
    }
    // First, drain any remaining key stream from a previous XORKeyStream.
    if (s.len != 0) {
        var keyStream = s.buf.slice((nint)bufSize - s.len);
        if (len(src) < len(keyStream)) {
            keyStream = keyStream.slice(0, len(src));
        }
        _ = src[len(keyStream) - 1]; // bounds check elimination hint
        foreach (var (i, b) in keyStream) {
            dst[i] = (byte)(src[i] ^ b);
        }
        s.len -= len(keyStream);
        (dst, src) = (dst.slice(len(keyStream)), src.slice(len(keyStream)));
    }
    if (len(src) == 0) {
        return;
    }
    // If we'd need to let the counter overflow and keep generating output,
    // panic immediately. If instead we'd only reach the last block, remember
    // not to generate any more output after the buffer is drained.
    var numBlocks = ((uint64)len(src) + (uint64)blockSize - 1) / (uint64)blockSize;
    if (s.overflow || (uint64)s.counter + numBlocks > ((uint64)1 << (int)(32))){
        throw panic("chacha20: counter overflow");
    } else 
    if ((uint64)s.counter + numBlocks == ((uint64)1 << (int)(32))) {
        s.overflow = true;
    }
    // xorKeyStreamBlocks implementations expect input lengths that are a
    // multiple of bufSize. Platform-specific ones process multiple blocks at a
    // time, so have bufSizes that are a multiple of blockSize.
    nint full = len(src) - len(src) % (nint)bufSize;
    if (full > 0) {
        s.xorKeyStreamBlocks(dst.slice(0, full), src.slice(0, full));
    }
    (dst, src) = (dst.slice(full), src.slice(full));
    // If using a multi-block xorKeyStreamBlocks would overflow, use the generic
    // one that does one block at a time.
    const uint64 blocksPerBuf = /* bufSize / blockSize */ 1;
    if ((uint64)s.counter + blocksPerBuf > ((uint64)1 << (int)(32))) {
        s.buf = new byte[]{}.array(64);
        nint numBlocksΔ1 = (len(src) + (nint)blockSize - 1) / (nint)blockSize;
        var buf = s.buf.slice((nint)bufSize - numBlocksΔ1 * (nint)blockSize);
        copy(buf, src);
        s.xorKeyStreamBlocksGeneric(buf, buf);
        s.len = len(buf) - copy(dst, buf);
        return;
    }
    // If we have a partial (multi-)block, pad it for xorKeyStreamBlocks, and
    // keep the leftover keystream for the next XORKeyStream invocation.
    if (len(src) > 0) {
        s.buf = new byte[]{}.array(64);
        copy(s.buf[..], src);
        s.xorKeyStreamBlocks(s.buf[..], s.buf[..]);
        s.len = (nint)bufSize - copy(dst, s.buf[..]);
    }
}

[GoRecv] internal static void xorKeyStreamBlocksGeneric(this ref Cipher s, slice<byte> dst, slice<byte> src) {
    if (len(dst) != len(src) || len(dst) % (nint)blockSize != 0) {
        throw panic("chacha20: internal error: wrong dst and/or src length");
    }
    // To generate each block of key stream, the initial cipher state
    // (represented below) is passed through 20 rounds of shuffling,
    // alternatively applying quarterRounds by columns (like 1, 5, 9, 13)
    // or by diagonals (like 1, 6, 11, 12).
    //
    //      0:cccccccc   1:cccccccc   2:cccccccc   3:cccccccc
    //      4:kkkkkkkk   5:kkkkkkkk   6:kkkkkkkk   7:kkkkkkkk
    //      8:kkkkkkkk   9:kkkkkkkk  10:kkkkkkkk  11:kkkkkkkk
    //     12:bbbbbbbb  13:nnnnnnnn  14:nnnnnnnn  15:nnnnnnnn
    //
    //            c=constant k=key b=blockcount n=nonce
    uint32 c0 = j0;
    uint32 c1 = j1;
    uint32 c2 = j2;
    uint32 c3 = j3;
    
    uint32 c4 = s.key[0];
    uint32 c5 = s.key[1];
    uint32 c6 = s.key[2];
    uint32 c7 = s.key[3];
    
    uint32 c8 = s.key[4];
    uint32 c9 = s.key[5];
    uint32 c10 = s.key[6];
    uint32 c11 = s.key[7];
    
    uint32 _ᴛ1 = s.counter;
    uint32 c13 = s.nonce[0];
    uint32 c14 = s.nonce[1];
    uint32 c15 = s.nonce[2];
    // Three quarters of the first round don't depend on the counter, so we can
    // calculate them here, and reuse them for multiple blocks in the loop, and
    // for future XORKeyStream invocations.
    if (!s.precompDone) {
        (s.p1, s.p5, s.p9, s.p13) = quarterRound(c1, c5, c9, c13);
        (s.p2, s.p6, s.p10, s.p14) = quarterRound(c2, c6, c10, c14);
        (s.p3, s.p7, s.p11, s.p15) = quarterRound(c3, c7, c11, c15);
        s.precompDone = true;
    }
    // A condition of len(src) > 0 would be sufficient, but this also
    // acts as a bounds check elimination hint.
    while (len(src) >= 64 && len(dst) >= 64) {
        // The remainder of the first column round.
        var (fcr0, fcr4, fcr8, fcr12) = quarterRound(c0, c4, c8, s.counter);
        // The second diagonal round.
        var (x0, x5, x10, x15) = quarterRound(fcr0, s.p5, s.p10, s.p15);
        var (x1, x6, x11, x12) = quarterRound(s.p1, s.p6, s.p11, fcr12);
        var (x2, x7, x8, x13) = quarterRound(s.p2, s.p7, fcr8, s.p13);
        var (x3, x4, x9, x14) = quarterRound(s.p3, fcr4, s.p9, s.p14);
        // The remaining 18 rounds.
        for (nint i = 0; i < 9; i++) {
            // Column round.
            (x0, x4, x8, x12) = quarterRound(x0, x4, x8, x12);
            (x1, x5, x9, x13) = quarterRound(x1, x5, x9, x13);
            (x2, x6, x10, x14) = quarterRound(x2, x6, x10, x14);
            (x3, x7, x11, x15) = quarterRound(x3, x7, x11, x15);
            // Diagonal round.
            (x0, x5, x10, x15) = quarterRound(x0, x5, x10, x15);
            (x1, x6, x11, x12) = quarterRound(x1, x6, x11, x12);
            (x2, x7, x8, x13) = quarterRound(x2, x7, x8, x13);
            (x3, x4, x9, x14) = quarterRound(x3, x4, x9, x14);
        }
        // Add back the initial state to generate the key stream, then
        // XOR the key stream with the source and write out the result.
        addXor(dst[0..4], src[0..4], x0, c0);
        addXor(dst[4..8], src[4..8], x1, c1);
        addXor(dst[8..12], src[8..12], x2, c2);
        addXor(dst[12..16], src[12..16], x3, c3);
        addXor(dst[16..20], src[16..20], x4, c4);
        addXor(dst[20..24], src[20..24], x5, c5);
        addXor(dst[24..28], src[24..28], x6, c6);
        addXor(dst[28..32], src[28..32], x7, c7);
        addXor(dst[32..36], src[32..36], x8, c8);
        addXor(dst[36..40], src[36..40], x9, c9);
        addXor(dst[40..44], src[40..44], x10, c10);
        addXor(dst[44..48], src[44..48], x11, c11);
        addXor(dst[48..52], src[48..52], x12, s.counter);
        addXor(dst[52..56], src[52..56], x13, c13);
        addXor(dst[56..60], src[56..60], x14, c14);
        addXor(dst[60..64], src[60..64], x15, c15);
        s.counter += 1;
        (src, dst) = (src[(int)(blockSize)..], dst[(int)(blockSize)..]);
    }
}

// HChaCha20 uses the ChaCha20 core to generate a derived key from a 32 bytes
// key and a 16 bytes nonce. It returns an error if key or nonce have any other
// length. It is used as part of the XChaCha20 construction.
public static (slice<byte>, error) HChaCha20(slice<byte> key, slice<byte> nonce) {
    // This function is split into a wrapper so that the slice allocation will
    // be inlined, and depending on how the caller uses the return value, won't
    // escape to the heap.
    var @out = new slice<byte>(32);
    return hChaCha20(@out, key, nonce);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string chacha20WrongHChaCha20ˢ = "chacha20: wrong HChaCha20 key size"u8;
private static readonly @string chacha20WrongHChaCha20ˢ2 = "chacha20: wrong HChaCha20 nonce size"u8;

internal static (slice<byte>, error) hChaCha20(slice<byte> @out, slice<byte> key, slice<byte> nonce) {
    if (len(key) != KeySize) {
        return (default!, errors.New(chacha20WrongHChaCha20ˢ));
    }
    if (len(nonce) != 16) {
        return (default!, errors.New(chacha20WrongHChaCha20ˢ2));
    }
    var (x0, x1, x2, x3) = (j0, j1, j2, j3);
    var x4 = binary.LittleEndian.Uint32(key[0..4]);
    var x5 = binary.LittleEndian.Uint32(key[4..8]);
    var x6 = binary.LittleEndian.Uint32(key[8..12]);
    var x7 = binary.LittleEndian.Uint32(key[12..16]);
    var x8 = binary.LittleEndian.Uint32(key[16..20]);
    var x9 = binary.LittleEndian.Uint32(key[20..24]);
    var x10 = binary.LittleEndian.Uint32(key[24..28]);
    var x11 = binary.LittleEndian.Uint32(key[28..32]);
    var x12 = binary.LittleEndian.Uint32(nonce[0..4]);
    var x13 = binary.LittleEndian.Uint32(nonce[4..8]);
    var x14 = binary.LittleEndian.Uint32(nonce[8..12]);
    var x15 = binary.LittleEndian.Uint32(nonce[12..16]);
    for (nint i = 0; i < 10; i++) {
        // Diagonal round.
        (x0, x4, x8, x12) = quarterRound(x0, x4, x8, x12);
        (x1, x5, x9, x13) = quarterRound(x1, x5, x9, x13);
        (x2, x6, x10, x14) = quarterRound(x2, x6, x10, x14);
        (x3, x7, x11, x15) = quarterRound(x3, x7, x11, x15);
        // Column round.
        (x0, x5, x10, x15) = quarterRound(x0, x5, x10, x15);
        (x1, x6, x11, x12) = quarterRound(x1, x6, x11, x12);
        (x2, x7, x8, x13) = quarterRound(x2, x7, x8, x13);
        (x3, x4, x9, x14) = quarterRound(x3, x4, x9, x14);
    }
    _ = @out[31]; // bounds check elimination hint
    binary.LittleEndian.PutUint32(@out[0..4], x0);
    binary.LittleEndian.PutUint32(@out[4..8], x1);
    binary.LittleEndian.PutUint32(@out[8..12], x2);
    binary.LittleEndian.PutUint32(@out[12..16], x3);
    binary.LittleEndian.PutUint32(@out[16..20], x12);
    binary.LittleEndian.PutUint32(@out[20..24], x13);
    binary.LittleEndian.PutUint32(@out[24..28], x14);
    binary.LittleEndian.PutUint32(@out[28..32], x15);
    return (@out, default!);
}

} // end chacha20_package
