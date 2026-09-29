// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Counter (CTR) mode.
// CTR converts a block cipher into a stream cipher by
// repeatedly encrypting an incrementing counter and
// xoring the resulting stream of data with the input.
// See NIST SP 800-38A, pp 13-15
namespace go.crypto;

using bytes = bytes_package;
using aes = go.crypto.@internal.fips140.aes_package;
using alias = go.crypto.@internal.fips140.alias_package;
using fips140only = go.crypto.@internal.fips140only_package;
using subtle = go.crypto.subtle_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class cipher_package {

[GoType] partial struct ctr {
    internal Block b;
    internal slice<byte> Δctr;
    internal slice<byte> @out;
    internal nint outUsed;
}

internal static UntypedInt streamBufferSize => 512;

// ctrAble is an interface implemented by ciphers that have a specific optimized
// implementation of CTR. crypto/aes doesn't use this anymore, and we'd like to
// eventually remove it.
[GoType] partial interface ctrAble {
    Stream NewCTR(slice<byte> iv);
}

// NewCTR returns a [Stream] which encrypts/decrypts using the given [Block] in
// counter mode. The length of iv must be the same as the [Block]'s block size.
public static Stream NewCTR(Block block, slice<byte> iv) {
    {
        var (blockΔ1, ok) = block._<ж<aes.Block>>(ᐧ); if (ok) {
            return new aesCtrWrapper(aes.NewCTR(blockΔ1, iv));
        }
    }
    if (fips140only.Enabled) {
        throw panic("crypto/cipher: use of CTR with non-AES ciphers is not allowed in FIPS 140-only mode");
    }
    {
        var (ctr, ok) = block._<ctrAble>(ᐧ); if (ok) {
            return ctr.NewCTR(iv);
        }
    }
    if (len(iv) != block.BlockSize()) {
        throw panic("cipher.NewCTR: IV length must equal block size");
    }
    nint bufSize = streamBufferSize;
    if (bufSize < block.BlockSize()) {
        bufSize = block.BlockSize();
    }
    return new ctrжStream(Ꮡ(new ctr(
        b: block,
        Δctr: bytes.Clone(iv),
        @out: new slice<byte>(0, bufSize),
        outUsed: 0
    )));
}

// aesCtrWrapper hides extra methods from aes.CTR.
[GoType] partial struct aesCtrWrapper {
    internal ж<aes.CTR> c;
}

internal static void XORKeyStream(this aesCtrWrapper x, slice<byte> dst, slice<byte> src) {
    x.c.XORKeyStream(dst, src);
}

[GoRecv] internal static void refill(this ref ctr x) {
    nint remain = len(x.@out) - x.outUsed;
    copy(x.@out, x.@out.slice(x.outUsed));
    x.@out = x.@out.slice(0, cap(x.@out));
    nint bs = x.b.BlockSize();
    while (remain <= len(x.@out) - bs) {
        x.b.Encrypt(x.@out.slice(remain), x.Δctr);
        remain += bs;
        // Increment counter
        for (nint i = len(x.Δctr) - 1; i >= 0; i--) {
            x.Δctr[i]++;
            if (x.Δctr[i] != 0) {
                break;
            }
        }
    }
    x.@out = x.@out.slice(0, remain);
    x.outUsed = 0;
}

[GoRecv] internal static void XORKeyStream(this ref ctr x, slice<byte> dst, slice<byte> src) {
    if (len(dst) < len(src)) {
        throw panic("crypto/cipher: output smaller than input");
    }
    if (alias.InexactOverlap(dst.slice(0, len(src)), src)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    {
        var (_, ok) = x.b._<ж<aes.Block>>(ᐧ); if (ok) {
            throw panic("crypto/cipher: internal error: generic CTR used with AES");
        }
    }
    while (len(src) > 0) {
        if (x.outUsed >= len(x.@out) - x.b.BlockSize()) {
            x.refill();
        }
        nint n = subtle.XORBytes(dst, src, x.@out.slice(x.outUsed));
        dst = dst.slice(n);
        src = src.slice(n);
        x.outUsed += n;
    }
}

} // end cipher_package
