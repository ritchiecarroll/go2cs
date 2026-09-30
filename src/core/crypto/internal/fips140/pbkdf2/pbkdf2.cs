// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using hmac = go.crypto.@internal.fips140.hmac_package;
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class pbkdf2_package {

// divRoundUp divides x+y-1 by y, rounding up if the result is not whole.
// This function casts x and y to int64 in order to avoid cases where
// x+y would overflow int on systems where int is an int32. The result
// is an int, which is safe as (x+y-1)/y should always fit, regardless
// of the integer size.
internal static nint divRoundUp(nint x, nint y) {
    return (nint)(quo(((int64)x + (int64)y - 1), (int64)y));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pkbdf2KeyLengthMustBeˢ = "pkbdf2: keyLength must be larger than 0"u8;
private static readonly @string pbkdf2KeyLengthTooLongˢ = "pbkdf2: keyLength too long"u8;

public static (slice<byte>, error) Key<Hash>(Func<Hash> h, @string password, slice<byte> salt, nint iter, nint keyLength)
    where Hash : fips140.Hash
{
    setServiceIndicator(salt, keyLength);
    if (keyLength <= 0) {
        return (default!, errors.New(pkbdf2KeyLengthMustBeˢ));
    }
    var prf = hmac.New(h, slice<byte>(password));
    hmac.MarkAsUsedInKDF(prf);
    nint hashLen = prf.Size();
    nint numBlocks = divRoundUp(keyLength, hashLen);
    const int64 maxBlocks = /* int64(1<<32 - 1) */ 4294967295;
    if (keyLength + hashLen < keyLength || (int64)numBlocks > maxBlocks) {
        return (default!, errors.New(pbkdf2KeyLengthTooLongˢ));
    }
    array<byte> buf = new(4);
    var dk = new slice<byte>(0, numBlocks * hashLen);
    var U = new slice<byte>(hashLen);
    for (nint block = 1; block <= numBlocks; block++) {
        // N.B.: || means concatenation, ^ means XOR
        // for each block T_i = U_1 ^ U_2 ^ ... ^ U_iter
        // U_1 = PRF(password, salt || uint(i))
        prf.Reset();
        prf.Write(salt);
        buf[0] = (byte)((block >> (int)(24)));
        buf[1] = (byte)((block >> (int)(16)));
        buf[2] = (byte)((block >> (int)(8)));
        buf[3] = (byte)block;
        prf.Write(buf[..4]);
        dk = prf.Sum(dk);
        var T = dk.slice(len(dk) - hashLen);
        copy(U, T);
        // U_n = PRF(password, U_(n-1))
        for (nint n = 2; n <= iter; n++) {
            prf.Reset();
            prf.Write(U);
            U = U[..0];
            U = prf.Sum(U);
            foreach (var (x, _) in U) {
                T[x] ^= (byte)(U[x]);
            }
        }
    }
    return (dk.slice(0, keyLength), default!);
}

internal static void setServiceIndicator(slice<byte> salt, nint keyLength) {
    // The HMAC construction will handle the hash function considerations for the service
    // indicator. The remaining PBKDF2 considerations outlined by SP 800-132 pertain to
    // salt and keyLength.
    // The length of the randomly-generated portion of the salt shall be at least 128 bits.
    if (len(salt) < 128 / 8) {
        fips140.RecordNonApproved();
    }
    // Per FIPS 140-3 IG C.M, key lengths below 112 bits are only allowed for
    // legacy use (i.e. verification only) and we don't support that.
    if (keyLength < 112 / 8) {
        fips140.RecordNonApproved();
    }
    fips140.RecordApproved();
}

} // end pbkdf2_package
