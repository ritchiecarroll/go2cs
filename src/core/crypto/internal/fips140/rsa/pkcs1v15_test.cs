// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using crypto = crypto_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = encoding.asn1_package;
using testing = testing_package;
using encoding;
using go.crypto.x509;
using static go.crypto.@internal.fips140.rsa_package;

partial class rsa_internal_test_package {

[GoType("dyn")] internal partial struct TestHashPrefixes_val {
    public pkix.AlgorithmIdentifier HashAlgorithm;
    public slice<byte> Hash;
}

public static void TestHashPrefixes(ж<testing.T> Ꮡt) {
    var prefixes = new map<crypto.Hash, asn1.ObjectIdentifier>{ // RFC 3370, Section 2.1 and 2.2
 //
 // sha-1 OBJECT IDENTIFIER ::= { iso(1) identified-organization(3)
 //      oiw(14) secsig(3) algorithm(2) 26 }
 //
 // md5 OBJECT IDENTIFIER ::= { iso(1) member-body(2) us(840)
 // 	rsadsi(113549) digestAlgorithm(2) 5 }

        [crypto.MD5] = new nint[]{1, 2, 840, 113549, 2, 5}.slice(),
        [crypto.SHA1] = new nint[]{1, 3, 14, 3, 2, 26}.slice(), // https://csrc.nist.gov/projects/computer-security-objects-register/algorithm-registration
 //
 // nistAlgorithms OBJECT IDENTIFIER ::= { joint-iso-ccitt(2) country(16) us(840)
 //          organization(1) gov(101) csor(3) nistAlgorithm(4) }
 //
 // hashAlgs OBJECT IDENTIFIER ::= { nistAlgorithms 2 }
 //
 // id-sha256 OBJECT IDENTIFIER ::= { hashAlgs 1 }
 // id-sha384 OBJECT IDENTIFIER ::= { hashAlgs 2 }
 // id-sha512 OBJECT IDENTIFIER ::= { hashAlgs 3 }
 // id-sha224 OBJECT IDENTIFIER ::= { hashAlgs 4 }
 // id-sha512-224 OBJECT IDENTIFIER ::= { hashAlgs 5 }
 // id-sha512-256 OBJECT IDENTIFIER ::= { hashAlgs 6 }
 // id-sha3-224 OBJECT IDENTIFIER ::= { hashAlgs 7 }
 // id-sha3-256 OBJECT IDENTIFIER ::= { hashAlgs 8 }
 // id-sha3-384 OBJECT IDENTIFIER ::= { hashAlgs 9 }
 // id-sha3-512 OBJECT IDENTIFIER ::= { hashAlgs 10 }

        [crypto.SHA224] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 4}.slice(),
        [crypto.SHA256] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 1}.slice(),
        [crypto.SHA384] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 2}.slice(),
        [crypto.SHA512] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 3}.slice(),
        [crypto.SHA512_224] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 5}.slice(),
        [crypto.SHA512_256] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 6}.slice(),
        [crypto.SHA3_224] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 7}.slice(),
        [crypto.SHA3_256] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 8}.slice(),
        [crypto.SHA3_384] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 9}.slice(),
        [crypto.SHA3_512] = new nint[]{2, 16, 840, 1, 101, 3, 4, 2, 10}.slice()
    };
    foreach (var (h, oid) in prefixes) {
        var (want, err) = asn1.Marshal(new TestHashPrefixes_val(
            HashAlgorithm: new pkix.AlgorithmIdentifier(
                Algorithm: oid,
                Parameters: asn1.NullRawValue
            ),
            Hash: new slice<byte>(h.Size())
        ));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        want = want.slice(0, len(want) - h.Size());
        var got = hashPrefixes[h.String()];
        if (!bytes.Equal(got, want)) {
            Ꮡt.Errorf("%s: got %x, want %x"u8, h, got, want);
        }
    }
}

} // end rsa_internal_test_package
