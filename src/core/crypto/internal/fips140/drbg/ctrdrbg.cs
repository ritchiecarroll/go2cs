// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using bits = math.bits_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;
using math;

partial class drbg_package {

// Counter is an SP 800-90A Rev. 1 CTR_DRBG instantiated with AES-256.
//
// Per Table 3, it has a security strength of 256 bits, a seed size of 384 bits,
// a counter length of 128 bits, a reseed interval of 2^48 requests, and a
// maximum request size of 2^19 bits (2^16 bytes, 64 KiB).
//
// We support a narrow range of parameters that fit the needs of our RNG:
// AES-256, no derivation function, no personalization string, no prediction
// resistance, and 384-bit additional input.
partial struct Counter {
    // c is instantiated with K as the key and V as the counter.
    internal aes.CTR c;
    internal uint64 reseedCounter;
}

internal static UntypedInt keySize => /* 256 / 8 */ 32;
public static UntypedInt SeedSize => /* keySize + aes.BlockSize */ 48;
internal static UntypedInt reseedInterval => /* 1 << 48 */ 281474976710656;
internal static UntypedInt maxRequestSize => /* (1 << 19) / 8 */ 65536;

public static ж<Counter> NewCounter(/*[48]*/ ж<array<byte>> Ꮡentropy) {
    // CTR_DRBG_Instantiate_algorithm, per Section 10.2.1.3.1.
    fips140.RecordApproved();
    var K = new slice<byte>(keySize);
    var V = new slice<byte>(aes.ΔBlockSize);
    // V starts at 0, but is incremented in CTR_DRBG_Update before each use,
    // unlike AES-CTR where it is incremented after each use.
    V[len(V) - 1] = 1;
    var (cipher, err) = aes.New(K);
    if (err != default!) {
        throw panic(err);
    }
    var c = Ꮡ(new Counter(nil));
    c.Value.c = aes.NewCTR(cipher, V).Value;
    c.update(Ꮡentropy);
    c.Value.reseedCounter = 1;
    return c;
}

internal static void update(this ж<Counter> Ꮡc, /*[48]*/ ж<array<byte>> Ꮡseed) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var seed = ref Ꮡseed.DerefOrNull();

    // CTR_DRBG_Update, per Section 10.2.1.2.
    var temp = new slice<byte>(SeedSize);
    Ꮡc.of(Counter.Ꮡc).XORKeyStream(temp, seed[..]);
    var K = temp[..(int)(keySize)];
    var V = temp[(int)(keySize)..];
    // Again, we pre-increment V, like in NewCounter.
    increment(Ꮡ(array<byte>.Alias(V, 16)));
    var (cipher, err) = aes.New(K);
    if (err != default!) {
        throw panic(err);
    }
    c.c = aes.NewCTR(cipher, V).Value;
}

internal static void increment(/*[16]*/ ж<array<byte>> Ꮡv) {
    ref var v = ref Ꮡv.DerefOrNull();

    var hi = byteorder.BEUint64(v[..8]);
    var lo = byteorder.BEUint64(v[8..]);
    (lo, var c) = bits.Add64(lo, 1, 0);
    (hi, _) = bits.Add64(hi, 0, c);
    byteorder.BEPutUint64(v[..8], hi);
    byteorder.BEPutUint64(v[8..], lo);
}

public static void Reseed(this ж<Counter> Ꮡc, /*[48]*/ ж<array<byte>> Ꮡentropy, /*[48]*/ ж<array<byte>> ᏑadditionalInput) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var entropy = ref Ꮡentropy.DerefOrNull();
    ref var additionalInput = ref ᏑadditionalInput.DerefOrNull();

    // CTR_DRBG_Reseed_algorithm, per Section 10.2.1.4.1.
    fips140.RecordApproved();
    ref var seed = ref heap(new array<byte>(48), out var Ꮡseed);
    subtle.XORBytes(seed[..], entropy[..], additionalInput[..]);
    Ꮡc.update(Ꮡseed);
    c.reseedCounter = 1;
}

// Generate produces at most maxRequestSize bytes of random data in out.
public static bool /*reseedRequired*/ Generate(this ж<Counter> Ꮡc, slice<byte> @out, /*[48]*/ ж<array<byte>> ᏑadditionalInput) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var additionalInput = ref ᏑadditionalInput.DerefOrNull();

    // CTR_DRBG_Generate_algorithm, per Section 10.2.1.5.1.
    fips140.RecordApproved();
    if (len(@out) > maxRequestSize) {
        throw panic("crypto/drbg: internal error: request size exceeds maximum");
    }
    // Step 1.
    if (c.reseedCounter > reseedInterval) {
        return true;
    }
    // Step 2.
    if (ᏑadditionalInput != nil){
        Ꮡc.update(ᏑadditionalInput);
    } else {
        // If the additional input is null, the first CTR_DRBG_Update is
        // skipped, but the additional input is replaced with an all-zero string
        // for the second CTR_DRBG_Update.
        ᏑadditionalInput = Ꮡ(new array<byte>(48)); additionalInput = ref ᏑadditionalInput.DerefOrNull();
    }
    // Steps 3-5.
    clear(@out);
    Ꮡc.of(Counter.Ꮡc).XORKeyStream(@out, @out);
    aes.RoundToBlock(Ꮡc.of(Counter.Ꮡc));
    // Step 6.
    Ꮡc.update(ᏑadditionalInput);
    // Step 7.
    c.reseedCounter++;
    // Step 8.
    return false;
}

} // end drbg_package
