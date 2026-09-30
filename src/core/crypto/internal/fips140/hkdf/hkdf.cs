// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using hmac = go.crypto.@internal.fips140.hmac_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class hkdf_package {

public static slice<byte> Extract<H>(Func<H> h, slice<byte> secret, slice<byte> salt)
    where H : fips140.Hash
{
    if (len(secret) < 112 / 8) {
        fips140.RecordNonApproved();
    }
    if (salt == default!) {
        salt = new slice<byte>(h().Size());
    }
    var extractor = hmac.New(h, salt);
    hmac.MarkAsUsedInKDF(extractor);
    extractor.Write(secret);
    return extractor.Sum(default!);
}

public static slice<byte> Expand<H>(Func<H> h, slice<byte> pseudorandomKey, @string info, nint keyLen)
    where H : fips140.Hash
{
    var @out = new slice<byte>(0, keyLen);
    var expander = hmac.New(h, pseudorandomKey);
    hmac.MarkAsUsedInKDF(expander);
    uint8 counter = default!;
    slice<byte> buf = default!;
    while (len(@out) < keyLen) {
        counter++;
        if (counter == 0) {
            throw panic("hkdf: counter overflow");
        }
        if (counter > 1) {
            expander.Reset();
        }
        expander.Write(buf);
        expander.Write(slice<byte>(info));
        expander.Write(new byte[]{counter}.slice());
        buf = expander.Sum(buf[..0]);
        nint remain = keyLen - len(@out);
        remain = min(remain, len(buf));
        @out = appendꓸꓸꓸ(@out, buf.slice(0, remain));
    }
    return @out;
}

public static slice<byte> Key<H>(Func<H> h, slice<byte> secret, slice<byte> salt, @string info, nint keyLen)
    where H : fips140.Hash
{
    var prk = Extract(h, secret, salt);
    return Expand(h, prk, info, keyLen);
}

} // end hkdf_package
