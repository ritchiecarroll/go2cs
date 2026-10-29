// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.aes;

using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
using alias = go.crypto.@internal.fips140.alias_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using errors = errors_package;
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
public static void SealWithRandomNonce(ж<GCM> g, slice<byte> nonce, slice<byte> @out, slice<byte> plaintext, slice<byte> additionalData) {
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
    seal(@out, g, nonce, plaintext, additionalData);
}

// NewGCMWithCounterNonce returns a new AEAD that works like GCM, but enforces
// the construction of deterministic nonces. The nonce must be 96 bits, the
// first 32 bits must be an encoding of the module name, and the last 64 bits
// must be a counter. The starting value of the counter is set on the first call
// to Seal, and each subsequent call must increment it as a big-endian uint64.
// If the counter reaches the starting value minus one, Seal will panic.
//
// This complies with FIPS 140-3 IG C.H Scenario 3.
public static (ж<GCMWithCounterNonce>, error) NewGCMWithCounterNonce(ж<aes.Block> cipher) {
    var (g, err) = newGCM(Ꮡ(new GCM()), cipher, gcmStandardNonceSize, gcmTagSize);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(new GCMWithCounterNonce(g: g.Value)), default);
}

// NewGCMForTLS12 returns a new AEAD that works like GCM, but enforces the
// construction of nonces as specified in RFC 5288, Section 3 and RFC 9325,
// Section 7.2.1.
//
// This complies with FIPS 140-3 IG C.H Scenario 1.a.
public static (ж<GCMWithCounterNonce>, error) NewGCMForTLS12(ж<aes.Block> cipher) {
    var (g, err) = newGCM(Ꮡ(new GCM()), cipher, gcmStandardNonceSize, gcmTagSize);
    if (err != nil) {
        return (default, err);
    }
    // TLS 1.2 counters always start at zero.
    return (Ꮡ(new GCMWithCounterNonce(g: g.Value, startReady: true)), default);
}

// NewGCMForSSH returns a new AEAD that works like GCM, but enforces the
// construction of nonces as specified in RFC 5647.
//
// This complies with FIPS 140-3 IG C.H Scenario 1.d.
public static (ж<GCMWithCounterNonce>, error) NewGCMForSSH(ж<aes.Block> cipher) {
    var (g, err) = newGCM(Ꮡ(new GCM()), cipher, gcmStandardNonceSize, gcmTagSize);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(new GCMWithCounterNonce(g: g.Value)), default);
}

partial struct GCMWithCounterNonce {
    internal GCM g;
    internal bool prefixReady;
    internal uint32 prefix;
    internal bool startReady;
    internal uint64 start;
    internal uint64 next;
}

public static nint NonceSize(this ref GCMWithCounterNonce g) {
    return gcmStandardNonceSize;
}

public static nint Overhead(this ref GCMWithCounterNonce g) {
    return gcmTagSize;
}

// Seal implements the [cipher.AEAD] interface, checking that the nonce prefix
// is stable and that the counter is strictly increasing.
//
// It is not safe for concurrent use.
public static slice<byte> Seal(this ж<GCMWithCounterNonce> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (len(nonce) != gcmStandardNonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }

    if (fips140.Enabled) {
        if (!g.prefixReady) {
            // The first invocation sets the fixed prefix.
            g.prefixReady = true;
            g.prefix = byteorder.BEUint32(nonce[..4]);
        }
        if (g.prefix != byteorder.BEUint32(nonce[..4])) {
            throw panic("crypto/cipher: GCM nonce prefix changed");
        }

        var counter = byteorder.BEUint64(nonce.slice(len(nonce) - 8));
        if (!g.startReady) {
            // The first invocation sets the starting counter, if not fixed.
            g.startReady = true;
            g.start = counter;
        }
        counter -= g.start;

        // Ensure the counter is strictly increasing.
        if (counter == math.MaxUint64) {
            throw panic("crypto/cipher: counter exhausted");
        }
        if (counter < g.next) {
            throw panic("crypto/cipher: counter decreased or remained the same");
        }
        g.next = counter + 1;
    }

    fips140.RecordApproved();
    return Ꮡg.of(GCMWithCounterNonce.Ꮡg).sealAfterIndicator(dst, nonce, plaintext, data);
}

public static (slice<byte>, error) Open(this ж<GCMWithCounterNonce> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    fips140.RecordApproved();
    return Ꮡg.of(GCMWithCounterNonce.Ꮡg).Open(dst, nonce, ciphertext, data);
}

// NewGCMWithXORCounterNonce returns a new AEAD that works like GCM, but
// enforces the construction of deterministic nonces. The nonce must be 96 bits,
// the first 32 bits must be an encoding of the module name, and the last 64
// bits must be a counter XOR'd with a fixed value. The module name and XOR mask
// can be set with [GCMWithCounterNonce.SetNoncePrefixAndMask], or they are set
// on the first call to Seal, assuming the counter starts at zero. Each
// subsequent call must increment the counter as a big-endian uint64. If the
// counter reaches 2⁶⁴ minus one, Seal will panic.
//
// This complies with FIPS 140-3 IG C.H Scenario 3.
public static (ж<GCMWithXORCounterNonce>, error) NewGCMWithXORCounterNonce(ж<aes.Block> cipher) {
    var (g, err) = newGCM(Ꮡ(new GCM()), cipher, gcmStandardNonceSize, gcmTagSize);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(new GCMWithXORCounterNonce(g: g.Value)), default);
}

// NewGCMForTLS13 returns a new AEAD that works like GCM, but enforces the
// construction of nonces as specified in RFC 8446, Section 5.3.
//
// This complies with FIPS 140-3 IG C.H Scenario 1.a.
public static (ж<GCMWithXORCounterNonce>, error) NewGCMForTLS13(ж<aes.Block> cipher) {
    var (g, err) = newGCM(Ꮡ(new GCM()), cipher, gcmStandardNonceSize, gcmTagSize);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(new GCMWithXORCounterNonce(g: g.Value)), default);
}

// NewGCMForHPKE returns a new AEAD that works like GCM, but enforces the
// construction of nonces as specified in RFC 9180, Section 5.2.
//
// This complies with FIPS 140-3 IG C.H Scenario 5.
public static (ж<GCMWithXORCounterNonce>, error) NewGCMForHPKE(ж<aes.Block> cipher) {
    var (g, err) = newGCM(Ꮡ(new GCM()), cipher, gcmStandardNonceSize, gcmTagSize);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(new GCMWithXORCounterNonce(g: g.Value)), default);
}

// NewGCMForQUIC returns a new AEAD that works like GCM, but enforces the
// construction of nonces as specified in RFC 9001, Section 5.3.
//
// Unlike in TLS 1.3, the QUIC nonce counter does not always start at zero, as
// the packet number does not reset on key updates, so the XOR mask must be
// provided explicitly instead of being learned on the first Seal call. Note
// that the nonce passed to Seal must already be XOR'd with the IV, the IV is
// provided here only to allow Seal to enforce that the counter is strictly
// increasing.
//
// This complies with FIPS 140-3 IG C.H Scenario 5.
public static (ж<GCMWithXORCounterNonce>, error) NewGCMForQUIC(ж<aes.Block> cipher, slice<byte> iv) {
    var (g, err) = newGCM(Ꮡ(new GCM()), cipher, gcmStandardNonceSize, gcmTagSize);
    if (err != nil) {
        return (default, err);
    }
    var gcm = Ꮡ(new GCMWithXORCounterNonce(g: g.Value));
    {
        var errΔ1 = gcm.SetNoncePrefixAndMask(iv); if (errΔ1 != nil) {
            return (default, errΔ1);
        }
    }
    return (gcm, default);
}

partial struct GCMWithXORCounterNonce {
    internal GCM g;
    internal bool ready;
    internal uint32 prefix;
    internal uint64 mask;
    internal uint64 next;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoCipherIncorrectˢ = "crypto/cipher: incorrect nonce length given to SetNoncePrefixAndMask"u8;
private static readonly @string cryptoCipherˢ = "crypto/cipher: SetNoncePrefixAndMask called twice or after first Seal"u8;

// SetNoncePrefixAndMask sets the fixed prefix and XOR mask for the nonces used
// in Seal. It must be called before the first call to Seal.
//
// The first 32 bits of nonce are used as the fixed prefix, and the last 64 bits
// are used as the XOR mask.
//
// Note that Seal expects the nonce to be already XOR'd with the mask. The mask
// is provided here only to allow Seal to enforce that the counter is strictly
// increasing.
public static error SetNoncePrefixAndMask(this ref GCMWithXORCounterNonce g, slice<byte> nonce) {
    if (len(nonce) != gcmStandardNonceSize) {
        return errors.New(cryptoCipherIncorrectˢ);
    }
    if (g.ready) {
        return errors.New(cryptoCipherˢ);
    }
    g.prefix = byteorder.BEUint32(nonce[..4]);
    g.mask = byteorder.BEUint64(nonce[4..]);
    g.ready = true;
    return default;
}

public static nint NonceSize(this ref GCMWithXORCounterNonce g) {
    return gcmStandardNonceSize;
}

public static nint Overhead(this ref GCMWithXORCounterNonce g) {
    return gcmTagSize;
}

// Seal implements the [cipher.AEAD] interface, checking that the nonce prefix
// is stable and that the counter is strictly increasing.
//
// It is not safe for concurrent use.
public static slice<byte> Seal(this ж<GCMWithXORCounterNonce> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (len(nonce) != gcmStandardNonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }

    if (fips140.Enabled) {
        var counter = byteorder.BEUint64(nonce.slice(len(nonce) - 8));
        if (!g.ready) {
            // In the first call, if [GCMWithXORCounterNonce.SetNoncePrefixAndMask]
            // wasn't used, we assume the counter is zero to learn the XOR mask and
            // fixed prefix.
            g.ready = true;
            g.mask = counter;
            g.prefix = byteorder.BEUint32(nonce[..4]);
        }
        if (g.prefix != byteorder.BEUint32(nonce[..4])) {
            throw panic("crypto/cipher: GCM nonce prefix changed");
        }
        counter ^= (uint64)(g.mask);

        // Ensure the counter is strictly increasing.
        if (counter == math.MaxUint64) {
            throw panic("crypto/cipher: counter exhausted");
        }
        if (counter < g.next) {
            throw panic("crypto/cipher: counter decreased or remained the same");
        }
        g.next = counter + 1;
    }

    fips140.RecordApproved();
    return Ꮡg.of(GCMWithXORCounterNonce.Ꮡg).sealAfterIndicator(dst, nonce, plaintext, data);
}

public static (slice<byte>, error) Open(this ж<GCMWithXORCounterNonce> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    fips140.RecordApproved();
    return Ꮡg.of(GCMWithXORCounterNonce.Ꮡg).Open(dst, nonce, ciphertext, data);
}

} // end gcm_package
