// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !s390x || purego
namespace go.crypto.@internal.fips140;

partial class ecdsa_package {

internal static (ж<Signature>, error) sign<P>(ref Curve<P> c, ref PrivateKey priv, ж<hmacDRBG> drbg, slice<byte> hash)
    where P : Point<P>
{
    return signGeneric(ref c, ref priv, drbg, hash);
}

internal static error verify<P>(ref Curve<P> c, ref ΔPublicKey pub, slice<byte> hash, ref Signature sig)
    where P : Point<P>
{
    return verifyGeneric(ref c, ref pub, hash, ref sig);
}

} // end ecdsa_package
