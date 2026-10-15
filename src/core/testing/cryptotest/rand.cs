// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cryptotest provides deterministic random source testing.
namespace go.testing;

using cryptorand = crypto.rand_package;
using byteorder = @internal.byteorder_package;
using io = io_package;
using mathrand = math.rand.rand_package;
using sync = sync_package;
using testing = testing_package;
// blank import: crypto.rand_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @internal;
using math.rand;
using rand = math.rand.rand_package;

partial class cryptotest_package {

//go:linkname randSetTestingReader crypto/internal/rand.SetTestingReader
internal static partial void randSetTestingReader(io.Reader r);

//go:linkname testingCheckParallel testing.checkParallel
internal static partial void testingCheckParallel(ж<testing.T> t);

// SetGlobalRandom sets a global, deterministic cryptographic randomness source
// for the duration of test t. It affects crypto/rand, and all implicit sources
// of cryptographic randomness in the crypto/... packages.
//
// SetGlobalRandom may be called multiple times in the same test to reset the
// random stream or change the seed.
//
// Because SetGlobalRandom affects the whole process, it cannot be used in
// parallel tests or tests with parallel ancestors.
//
// Note that the way cryptographic algorithms use randomness is generally not
// specified and may change over time. Thus, if a test expects a specific output
// from a cryptographic function, it may fail in the future even if it uses
// SetGlobalRandom.
//
// SetGlobalRandom is not supported when building against the Go Cryptographic
// Module v1.0.0 (i.e. when [crypto/fips140.Version] returns "v1.0.0").
public static void SetGlobalRandom(ж<testing.T> Ꮡt, uint64 seed) {
    if (Ꮡt == nil) {
        throw panic("cryptotest: SetGlobalRandom called with a nil *testing.T");
    }
    if (!testing.Testing()) {
        throw panic("cryptotest: SetGlobalRandom used in a non-test binary");
    }
    testingCheckParallel(Ꮡt);

    array<byte> s = new(32);
    byteorder.LEPutUint64(s[..8], seed);
    var r = Ꮡ(new lockedReader(r: mathrand.NewChaCha8(s)));

    randSetTestingReader(new lockedReaderжReader(r));
    var previous = cryptorand.Reader;
    cryptorand.Reader = new lockedReaderжReader(r);
    var previousʗ1 = previous;
    Ꮡt.Cleanup(() => {
        cryptorand.Reader = previousʗ1;
        randSetTestingReader(default!);
    });
}

partial struct lockedReader {
    public partial ref go.sync_package.Mutex Mutex { get; }
    internal ж<mathrand.ChaCha8> r;
}

internal static (nint n, error err) Read(this ж<lockedReader> Ꮡlr, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var lr = ref Ꮡlr.DerefOrNull();

        Ꮡlr.of(lockedReader.ᏑMutex).Lock();
        defer(Ꮡlr.of(lockedReader.ᏑMutex).Unlock, ref ᒐ);
        (n, err) = lr.r.Read(b);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (n, err);
}

} // end cryptotest_package
