// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using Δboring = go.crypto.@internal.boring_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using randutil = go.crypto.@internal.randutil_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.@internal;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class rand_package {

partial struct reader {
    /*embed*/ public go.crypto.@internal.fips140.drbg_package.DefaultReader DefaultReader;
}

internal static (nint n, error err) Read(this reader r, slice<byte> b) {
    if (Δboring.Enabled) {
        {
            var (_, errΔ1) = Δboring.RandReader.Read(b); if (errΔ1 != default!) {
                throw panic("crypto/rand: boring RandReader failed: " + errΔ1.Error());
            }
        }
        return (len(b), default!);
    }
    drbg.Read(b);
    return (len(b), default!);
}

// Reader is an io.Reader that calls [drbg.Read].
//
// It should be used internally instead of [crypto/rand.Reader], because the
// latter can be set by applications outside of tests. These applications then
// risk breaking between Go releases, if the way the Reader is used changes.
public static io.Reader Reader = new reader(nil);

// SetTestingReader overrides all calls to [drbg.Read]. The Read method of
// r must never return an error or return short.
//
// SetTestingReader panics when building against Go Cryptographic Module v1.0.0.
//
// SetTestingReader is pulled by [testing/cryptotest.setGlobalRandom] via go:linkname.
//
//go:linkname SetTestingReader crypto/internal/rand.SetTestingReader
public static void SetTestingReader(io.Reader r) {
    fips140SetTestingReader(r);
}

internal static ж<godebug.Setting> cryptocustomrand = godebug.New("cryptocustomrand"u8);

// CustomReader returns [Reader] or, only if the GODEBUG setting
// "cryptocustomrand=1" is set, the provided io.Reader.
//
// If returning a non-default Reader, it calls [randutil.MaybeReadByte] on it.
public static io.Reader CustomReader(io.Reader r) {
    if (cryptocustomrand.Value() == "1"u8) {
        if (!IsDefaultReader(r)) {
            randutil.MaybeReadByte(r);
            cryptocustomrand.IncNonDefault();
        }
        return r;
    }
    return Reader;
}

// IsDefaultReader reports whether r is the default [crypto/rand.Reader].
//
// If true, the Read method of r can be assumed to call [drbg.Read].
public static bool IsDefaultReader(io.Reader r) {
    var (_, ok) = r._<drbg.DefaultReader>(ᐧ);
    return ok;
}

} // end rand_package
