// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.aes;

using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class gcm_package {

// CounterKDF implements a KDF in Counter Mode instantiated with CMAC-AES,
// according to NIST SP 800-108 Revision 1 Update 1, Section 4.1.
//
// It produces a 256-bit output, and accepts a 8-bit Label and a 96-bit Context.
// It uses a counter of 16 bits placed before the fixed data. The fixed data is
// the sequence Label || 0x00 || Context. The L field is omitted, since the
// output key length is fixed.
//
// It's optimized for use in XAES-256-GCM (https://c2sp.org/XAES-256-GCM),
// rather than for exposing it to applications as a stand-alone KDF.
partial struct CounterKDF {
    internal CMAC mac;
}

// NewCounterKDF creates a new CounterKDF with the given key.
public static ж<CounterKDF> NewCounterKDF(ж<aes.Block> Ꮡb) {
    return Ꮡ(new CounterKDF(mac: NewCMAC(Ꮡb).Value.ΔClone()));
}

// DeriveKey derives a key from the given label and context.
public static array<byte> DeriveKey(this ж<CounterKDF> Ꮡkdf, byte label, /*[12]*/ array<byte> context) {
    context = context.Clone();

    fips140.RecordApproved();
    array<byte> output = new(32);
    array<byte> input = new(16); /* aes.ΔBlockSize */
    input[2] = label;
    copy(input[4..], context[..]);
    input[1] = 0x01; // i = 1
    var K1 = Ꮡkdf.of(CounterKDF.Ꮡmac).MAC(input[..]);
    input[1] = 0x02; // i = 2
    var K2 = Ꮡkdf.of(CounterKDF.Ꮡmac).MAC(input[..]);
    copy(output[..], K1[..]);
    copy(output[(int)(aes.ΔBlockSize)..], K2[..]);
    return output.Clone();
}

} // end gcm_package
