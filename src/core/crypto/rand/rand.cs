// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rand implements a cryptographically secure
// random number generator.
namespace go.crypto;

using boring = go.crypto.@internal.boring_package;
using fips140 = go.crypto.@internal.fips140_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using sysrand = go.crypto.@internal.sysrand_package;
using io = io_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class rand_package {

// Reader is a global, shared instance of a cryptographically
// secure random number generator. It is safe for concurrent use.
//
//   - On Linux, FreeBSD, Dragonfly, and Solaris, Reader uses getrandom(2).
//   - On legacy Linux (< 3.17), Reader opens /dev/urandom on first use.
//   - On macOS, iOS, and OpenBSD Reader, uses arc4random_buf(3).
//   - On NetBSD, Reader uses the kern.arandom sysctl.
//   - On Windows, Reader uses the ProcessPrng API.
//   - On js/wasm, Reader uses the Web Crypto API.
//   - On wasip1/wasm, Reader uses random_get.
//
// In FIPS 140-3 mode, the output passes through an SP 800-90A Rev. 1
// Deterministric Random Bit Generator (DRBG).
public static io.Reader Reader;

[GoInit] internal static void init() {
    if (boring.Enabled) {
        Reader = new boring_randReaderᴠReader(boring.RandReader);
        return;
    }
    Reader = new readerжReader(Ꮡ(new reader(nil)));
}

partial struct reader {
    /*embed*/ public go.crypto.@internal.fips140.drbg_package.DefaultReader DefaultReader;
}

internal static (nint n, error err) Read(this ref reader r, slice<byte> b) {
    boring.Unreachable();
    if (fips140.Enabled){
        drbg.Read(b);
    } else {
        sysrand.Read(b);
    }
    return (len(b), default!);
}

// fatal is [runtime.fatal], pushed via linkname.
//
//go:linkname fatal
internal static partial void fatal(@string _);

// Read fills b with cryptographically secure random bytes. It never returns an
// error, and always fills b entirely.
//
// Read calls [io.ReadFull] on [Reader] and crashes the program irrecoverably if
// an error is returned. The default Reader uses operating system APIs that are
// documented to never return an error on all but legacy Linux systems.
public static (nint n, error err) Read(slice<byte> b) {
    error err = default!;

    // We don't want b to escape to the heap, but escape analysis can't see
    // through a potentially overridden Reader, so we special-case the default
    // case which we can keep non-escaping, and in the general case we read into
    // a heap buffer and copy from it.
    {
        var (r, ok) = Reader._<ж<reader>>(ᐧ); if (ok){
            (_, err) = r.Read(b);
        } else {
            var bb = new slice<byte>(len(b));
            (_, err) = io.ReadFull(Reader, bb);
            copy(b, bb);
        }
    }
    if (err != default!) {
        fatal("crypto/rand: failed to read random data (see https://go.dev/issue/66821): "u8 + err.Error());
        throw panic("unreachable"); // To be sure.
    }
    return (len(b), default!);
}

} // end rand_package
