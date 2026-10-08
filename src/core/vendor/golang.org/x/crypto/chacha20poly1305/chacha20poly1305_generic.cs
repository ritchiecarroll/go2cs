// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.crypto;

using binary = encoding.binary_package;
using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package;
using alias = go.vendor.golang.org.x.crypto.@internal.alias_package;
using poly1305 = go.vendor.golang.org.x.crypto.@internal.poly1305_package;
using encoding;
using go.vendor.golang.org.x.crypto;
using go.vendor.golang.org.x.crypto.@internal;

partial class chacha20poly1305_package {

internal static void writeWithPadding(ж<poly1305.MAC> Ꮡp, slice<byte> b) {
    Ꮡp.Write(b);
    {
        nint rem = len(b) % 16; if (rem != 0) {
            array<byte> buf = new(16);
            nint padLen = 16 - rem;
            Ꮡp.Write(buf.slice(0, padLen));
        }
    }
}

internal static void writeUint64(ж<poly1305.MAC> Ꮡp, nint n) {
    array<byte> buf = new(8);
    binary.LittleEndian.PutUint64(buf[..], (uint64)n);
    Ꮡp.Write(buf[..]);
}

internal static slice<byte> sealGeneric(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    var (ret, @out) = sliceForAppend(dst, len(plaintext) + (nint)poly1305.TagSize);
    var (ciphertext, tag) = (@out.slice(0, len(plaintext)), @out.slice(len(plaintext)));
    if (alias.InexactOverlap(@out, plaintext)) {
        throw panic("chacha20poly1305: invalid buffer overlap");
    }
    ref var polyKey = ref heap(new array<byte>(32), out var ᏑpolyKey);
    var (s, _) = chacha20.NewUnauthenticatedCipher(c.key[..], nonce);
    s.XORKeyStream(polyKey[..], polyKey[..]);
    s.SetCounter(1); // set the counter to 1, skipping 32 bytes
    s.XORKeyStream(ciphertext, plaintext);
    var p = poly1305.New(ᏑpolyKey);
    writeWithPadding(p, additionalData);
    writeWithPadding(p, ciphertext);
    writeUint64(p, len(additionalData));
    writeUint64(p, len(plaintext));
    p.Sum(tag[..0]);
    return ret;
}

internal static (slice<byte>, error) openGeneric(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    var tag = ciphertext.slice(len(ciphertext) - 16);
    ciphertext = ciphertext.slice(0, len(ciphertext) - 16);
    ref var polyKey = ref heap(new array<byte>(32), out var ᏑpolyKey);
    var (s, _) = chacha20.NewUnauthenticatedCipher(c.key[..], nonce);
    s.XORKeyStream(polyKey[..], polyKey[..]);
    s.SetCounter(1); // set the counter to 1, skipping 32 bytes
    var p = poly1305.New(ᏑpolyKey);
    writeWithPadding(p, additionalData);
    writeWithPadding(p, ciphertext);
    writeUint64(p, len(additionalData));
    writeUint64(p, len(ciphertext));
    var (ret, @out) = sliceForAppend(dst, len(ciphertext));
    if (alias.InexactOverlap(@out, ciphertext)) {
        throw panic("chacha20poly1305: invalid buffer overlap");
    }
    if (!p.Verify(tag)) {
        foreach (var (i, _) in @out) {
            @out[i] = 0;
        }
        return (default!, errOpen);
    }
    s.XORKeyStream(@out, ciphertext);
    return (ret, default!);
}

} // end chacha20poly1305_package
