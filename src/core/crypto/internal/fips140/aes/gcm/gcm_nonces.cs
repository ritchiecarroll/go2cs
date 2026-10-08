// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.aes;

using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
using alias = go.crypto.@internal.fips140.alias_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using math = math_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;

partial class gcm_package {

// SealWithRandomNonce encrypts plaintext to out, and writes a random nonce to
// nonce. nonce must be 12 bytes, and out must be 16 bytes longer than plaintext.
// out and plaintext may overlap exactly or not at all. additionalData and out
// must not overlap.
//
// This complies with FIPS 140-3 IG C.H Scenario 2.
//
// Note that this is NOT a [cipher.AEAD].Seal method.
public static void SealWithRandomNonce(ж<GCM> Ꮡg, slice<byte> nonce, slice<byte> @out, slice<byte> plaintext, slice<byte> additionalData) {
    if ((uint64)len(plaintext) > (uint64)((uint64)((4294967296L) - 2) * (uint64)gcmBlockSize)) {
        throw panic("crypto/cipher: message too large for GCM");
    }
    if (len(nonce) != gcmStandardNonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCMWithRandomNonce");
    }
    if (len(@out) != len(plaintext) + (nint)gcmTagSize) {
        throw panic("crypto/cipher: incorrect output length given to GCMWithRandomNonce");
    }
    if (alias.InexactOverlap(@out, plaintext)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and input");
    }
    if (alias.AnyOverlap(@out, additionalData)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and additional data");
    }
    fips140.RecordApproved();
    drbg.Read(nonce);
    seal(@out, Ꮡg, nonce, plaintext, additionalData);
}

// NewGCMWithCounterNonce returns a new AEAD that works like GCM, but enforces
// the construction of deterministic nonces. The nonce must be 96 bits, the
// first 32 bits must be an encoding of the module name, and the last 64 bits
// must be a counter.
//
// This complies with FIPS 140-3 IG C.H Scenario 3.
public static (ж<GCMWithCounterNonce>, error) NewGCMWithCounterNonce(ж<aes.Block> Ꮡcipher) {
    var (g, err) = newGCM(Ꮡ(new GCM(nil)), Ꮡcipher, gcmStandardNonceSize, gcmTagSize);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new GCMWithCounterNonce(g: g.Value)), default!);
}

partial struct GCMWithCounterNonce {
    internal GCM g;
    internal bool ready;
    internal uint32 fixedName;
    internal uint64 start;
    internal uint64 next;
}

public static nint NonceSize(this ref GCMWithCounterNonce g) {
    return gcmStandardNonceSize;
}

public static nint Overhead(this ref GCMWithCounterNonce g) {
    return gcmTagSize;
}

public static slice<byte> Seal(this ж<GCMWithCounterNonce> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (len(nonce) != gcmStandardNonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    var counter = byteorder.BEUint64(nonce.slice(len(nonce) - 8));
    if (!g.ready) {
        // The first invocation sets the fixed name encoding and start counter.
        g.ready = true;
        g.start = counter;
        g.fixedName = byteorder.BEUint32(nonce[..4]);
    }
    if (g.fixedName != byteorder.BEUint32(nonce[..4])) {
        throw panic("crypto/cipher: incorrect module name given to GCMWithCounterNonce");
    }
    counter -= g.start;
    // Ensure the counter is monotonically increasing.
    if (counter == math.MaxUint64) {
        throw panic("crypto/cipher: counter wrapped");
    }
    if (counter < g.next) {
        throw panic("crypto/cipher: counter decreased");
    }
    g.next = counter + 1;
    fips140.RecordApproved();
    return Ꮡg.of(GCMWithCounterNonce.Ꮡg).sealAfterIndicator(dst, nonce, plaintext, data);
}

public static (slice<byte>, error) Open(this ж<GCMWithCounterNonce> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    fips140.RecordApproved();
    return Ꮡg.of(GCMWithCounterNonce.Ꮡg).Open(dst, nonce, ciphertext, data);
}

// NewGCMForTLS12 returns a new AEAD that works like GCM, but enforces the
// construction of nonces as specified in RFC 5288, Section 3 and RFC 9325,
// Section 7.2.1.
//
// This complies with FIPS 140-3 IG C.H Scenario 1.a.
public static (ж<GCMForTLS12>, error) NewGCMForTLS12(ж<aes.Block> Ꮡcipher) {
    var (g, err) = newGCM(Ꮡ(new GCM(nil)), Ꮡcipher, gcmStandardNonceSize, gcmTagSize);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new GCMForTLS12(g: g.Value)), default!);
}

partial struct GCMForTLS12 {
    internal GCM g;
    internal uint64 next;
}

public static nint NonceSize(this ref GCMForTLS12 g) {
    return gcmStandardNonceSize;
}

public static nint Overhead(this ref GCMForTLS12 g) {
    return gcmTagSize;
}

public static slice<byte> Seal(this ж<GCMForTLS12> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (len(nonce) != gcmStandardNonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    var counter = byteorder.BEUint64(nonce.slice(len(nonce) - 8));
    // Ensure the counter is monotonically increasing.
    if (counter == math.MaxUint64) {
        throw panic("crypto/cipher: counter wrapped");
    }
    if (counter < g.next) {
        throw panic("crypto/cipher: counter decreased");
    }
    g.next = counter + 1;
    fips140.RecordApproved();
    return Ꮡg.of(GCMForTLS12.Ꮡg).sealAfterIndicator(dst, nonce, plaintext, data);
}

public static (slice<byte>, error) Open(this ж<GCMForTLS12> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    fips140.RecordApproved();
    return Ꮡg.of(GCMForTLS12.Ꮡg).Open(dst, nonce, ciphertext, data);
}

// NewGCMForTLS13 returns a new AEAD that works like GCM, but enforces the
// construction of nonces as specified in RFC 8446, Section 5.3.
public static (ж<GCMForTLS13>, error) NewGCMForTLS13(ж<aes.Block> Ꮡcipher) {
    var (g, err) = newGCM(Ꮡ(new GCM(nil)), Ꮡcipher, gcmStandardNonceSize, gcmTagSize);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new GCMForTLS13(g: g.Value)), default!);
}

partial struct GCMForTLS13 {
    internal GCM g;
    internal bool ready;
    internal uint64 mask;
    internal uint64 next;
}

public static nint NonceSize(this ref GCMForTLS13 g) {
    return gcmStandardNonceSize;
}

public static nint Overhead(this ref GCMForTLS13 g) {
    return gcmTagSize;
}

public static slice<byte> Seal(this ж<GCMForTLS13> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (len(nonce) != gcmStandardNonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    var counter = byteorder.BEUint64(nonce.slice(len(nonce) - 8));
    if (!g.ready) {
        // In the first call, the counter is zero, so we learn the XOR mask.
        g.ready = true;
        g.mask = counter;
    }
    counter ^= (uint64)(g.mask);
    // Ensure the counter is monotonically increasing.
    if (counter == math.MaxUint64) {
        throw panic("crypto/cipher: counter wrapped");
    }
    if (counter < g.next) {
        throw panic("crypto/cipher: counter decreased");
    }
    g.next = counter + 1;
    fips140.RecordApproved();
    return Ꮡg.of(GCMForTLS13.Ꮡg).sealAfterIndicator(dst, nonce, plaintext, data);
}

public static (slice<byte>, error) Open(this ж<GCMForTLS13> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    fips140.RecordApproved();
    return Ꮡg.of(GCMForTLS13.Ꮡg).Open(dst, nonce, ciphertext, data);
}

// NewGCMForSSH returns a new AEAD that works like GCM, but enforces the
// construction of nonces as specified in RFC 5647.
//
// This complies with FIPS 140-3 IG C.H Scenario 1.d.
public static (ж<GCMForSSH>, error) NewGCMForSSH(ж<aes.Block> Ꮡcipher) {
    var (g, err) = newGCM(Ꮡ(new GCM(nil)), Ꮡcipher, gcmStandardNonceSize, gcmTagSize);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new GCMForSSH(g: g.Value)), default!);
}

partial struct GCMForSSH {
    internal GCM g;
    internal bool ready;
    internal uint64 start;
    internal uint64 next;
}

public static nint NonceSize(this ref GCMForSSH g) {
    return gcmStandardNonceSize;
}

public static nint Overhead(this ref GCMForSSH g) {
    return gcmTagSize;
}

public static slice<byte> Seal(this ж<GCMForSSH> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (len(nonce) != gcmStandardNonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    var counter = byteorder.BEUint64(nonce.slice(len(nonce) - 8));
    if (!g.ready) {
        // In the first call we learn the start value.
        g.ready = true;
        g.start = counter;
    }
    counter -= g.start;
    // Ensure the counter is monotonically increasing.
    if (counter == math.MaxUint64) {
        throw panic("crypto/cipher: counter wrapped");
    }
    if (counter < g.next) {
        throw panic("crypto/cipher: counter decreased");
    }
    g.next = counter + 1;
    fips140.RecordApproved();
    return Ꮡg.of(GCMForSSH.Ꮡg).sealAfterIndicator(dst, nonce, plaintext, data);
}

public static (slice<byte>, error) Open(this ж<GCMForSSH> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    fips140.RecordApproved();
    return Ꮡg.of(GCMForSSH.Ꮡg).Open(dst, nonce, ciphertext, data);
}

} // end gcm_package
