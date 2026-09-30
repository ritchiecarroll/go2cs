// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ssh implements the SSH KDF as specified in RFC 4253,
// Section 7.2 and allowed by SP 800-135 Revision 1.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
// blank import: go.crypto.@internal.fips140.check_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.crypto.@internal;

partial class ssh_package {

[GoType] partial struct Direction {
    internal slice<byte> ivTag;
    internal slice<byte> keyTag;
    internal slice<byte> macKeyTag;
}

public static Direction ServerKeys;
public static Direction ClientKeys;

[GoInit] internal static void init() {
    ServerKeys = new Direction(new byte[]{(rune)'B'}.slice(), new byte[]{(rune)'D'}.slice(), new byte[]{(rune)'F'}.slice());
    ClientKeys = new Direction(new byte[]{(rune)'A'}.slice(), new byte[]{(rune)'C'}.slice(), new byte[]{(rune)'E'}.slice());
}

public static (slice<byte> ivKey, slice<byte> key, slice<byte> macKey) Keys<Hash>(Func<Hash> hash, Direction d, slice<byte> K, slice<byte> H, slice<byte> sessionID, nint ivKeyLen, nint keyLen, nint macKeyLen)
    where Hash : fips140.Hash
{
    slice<byte> ivKey = default!;
    slice<byte> key = default!;
    slice<byte> macKey = default!;

    var h = hash();
    var Hʗ1 = H;
    var Kʗ1 = K;
    var hʗ1 = h;
    var sessionIDʗ1 = sessionID;
    slice<byte> generateKeyMaterial(slice<byte> tag, nint length) {
        slice<byte> keyΔ1 = default!;
        while (len(keyΔ1) < length) {
            hʗ1.Reset();
            hʗ1.Write(Kʗ1);
            hʗ1.Write(Hʗ1);
            if (len(keyΔ1) == 0){
                hʗ1.Write(tag);
                hʗ1.Write(sessionIDʗ1);
            } else {
                hʗ1.Write(keyΔ1);
            }
            keyΔ1 = hʗ1.Sum(keyΔ1);
        }
        return keyΔ1.slice(0, length);
    }
    ivKey = generateKeyMaterial(d.ivTag, ivKeyLen);
    key = generateKeyMaterial(d.keyTag, keyLen);
    macKey = generateKeyMaterial(d.macKeyTag, macKeyLen);
    return (ivKey, key, macKey);
}

} // end ssh_package
