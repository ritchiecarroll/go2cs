// sig_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of crypto/internal/boring/sig's three "code signature" markers. Go
// declares them bodyless in sig.go and implements them ONLY in assembly (sig_amd64.s / sig_other.s)
// for every platform — there is NO purego variant, so no build tag can give them a Go body. go2cs
// therefore emits three bodyless `partial` methods and the PartialStubGenerator fills each with a
// throwing stub, which is why crypto/sha256's Sum256 died with "StandardCrypto: external (assembly
// or cgo) function is not implemented" before it could hash a single byte.
//
// An EMPTY body is the semantically exact port, not an approximation. Go's own comment says "The
// functions themselves are no-ops": each assembly stub is a two-byte jump over a fixed 29-byte magic
// sequence to a RET, and exists purely so a tool (rsc.io/goversion) can grep a linked BINARY to find
// out whether BoringCrypto / FIPS-only / standard crypto was linked in. That byte-signature purpose
// has no meaning in a managed assembly, and the observable runtime behavior of calling one — do
// nothing, return — is reproduced exactly here.

namespace go.crypto.@internal.boring;

partial class sig_package
{
    // BoringCrypto indicates that the BoringCrypto module is present.
    public static partial void BoringCrypto()
    {
    }

    // FIPSOnly indicates that package crypto/tls/fipsonly is present.
    public static partial void FIPSOnly()
    {
    }

    // StandardCrypto indicates that standard Go crypto is present.
    public static partial void StandardCrypto()
    {
    }
}
