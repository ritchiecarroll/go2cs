// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package drbg provides cryptographically secure random bytes
// usable by FIPS code. In FIPS mode it uses an SP 800-90A Rev. 1
// Deterministic Random Bit Generator (DRBG). Otherwise,
// it uses the operating system's random number generator.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using sysrand = go.crypto.@internal.sysrand_package;
using io = io_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using go.crypto.@internal;
using go.sync;

partial class drbg_package {

// getEntropy is very slow (~500µs), so we don't want it on the hot path.
// We keep both a persistent DRBG instance and a pool of additional instances.
// Occasional uses will use drbgInstance, even if the pool was emptied since the
// last use. Frequent concurrent uses will fill the pool and use it.
internal static ж<atomic.Pointer<Counter>> ᏑdrbgInstance = new StaticBox<atomic.Pointer<Counter>>(default(atomic.Pointer<Counter>));
internal static ref atomic.Pointer<Counter> drbgInstance => ref ᏑdrbgInstance.Value;

internal static ж<sync.Pool> ᏑdrbgPool = new StaticBox<sync.Pool>(default(sync.Pool));
internal static ref sync.Pool drbgPool => ref ᏑdrbgPool.Value;
internal static void initᴛdrbgPool() { drbgPool = new sync.Pool(
    New: () => NewCounter(getEntropy()).OrTypedNil()
); }

// Read fills b with cryptographically secure random bytes. In FIPS mode, it
// uses an SP 800-90A Rev. 1 Deterministic Random Bit Generator (DRBG).
// Otherwise, it uses the operating system's random number generator.
public static void Read(slice<byte> b) {
    GoFrame ᒐ = default;
    try {
        if (testingReader != default!) {
            fips140.RecordNonApproved();
            // Avoid letting b escape in the non-testing case.
            var bb = new slice<byte>(len(b));
            testingReader.Read(bb);
            copy(b, bb);
            return;
        }

        if (!fips140.Enabled) {
            sysrand.Read(b);
            return;
        }

        // At every read, 128 random bits from the operating system are mixed as
        // additional input, to make the output as strong as non-FIPS randomness.
        // This is not credited as entropy for FIPS purposes, as allowed by Section
        // 8.7.2: "Note that a DRBG does not rely on additional input to provide
        // entropy, even though entropy could be provided in the additional input".
        var additionalInput = Ꮡ(new array<byte>(48));
        sysrand.Read(additionalInput.slice(0, 16));

        var drbg = ᏑdrbgInstance.Swap(nil);
        if (drbg == nil) {
            drbg = ᏑdrbgPool.Get()._<ж<Counter>>();
        }
        var drbgʗ1 = drbg;
        defer(() => {
            if (!ᏑdrbgInstance.CompareAndSwap(nil, drbgʗ1)) {
                ᏑdrbgPool.Put(drbgʗ1.OrTypedNil());
            }
        }, ref ᒐ);

        while (len(b) > 0) {
            nint size = min(len(b), (nint)(maxRequestSize));
            {
                var reseedRequired = drbg.Generate(b.slice(0, size), additionalInput); if (reseedRequired) {
                    // See SP 800-90A Rev. 1, Section 9.3.1, Steps 6-8, as explained in
                    // Section 9.3.2: if Generate reports a reseed is required, the
                    // additional input is passed to Reseed along with the entropy and
                    // then nulled before the next Generate call.
                    drbg.Reseed(getEntropy(), additionalInput);
                    additionalInput = ж<array<byte>>.NilBoxOfDims(48L);
                    continue;
                }
            }
            b = b.slice(size);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static io.Reader testingReader;

// SetTestingReader sets a global, deterministic cryptographic randomness source
// for testing purposes. Its Read method must never return an error, it must
// never return short, and it must be safe for concurrent use.
//
// This is only intended to be used by the testing/cryptotest package.
public static void SetTestingReader(io.Reader r) {
    testingReader = r;
}

// DefaultReader is a sentinel type, embedded in the default
// [crypto/rand.Reader], used to recognize it when passed to
// APIs that accept a rand io.Reader.
//
// Any Reader that implements this interface is assumed to
// call [Read] as its Read method.
partial interface DefaultReader {
    void defaultReader();
}

// ReadWithReader uses Reader to fill b with cryptographically secure random
// bytes. It is intended for use in APIs that expose a rand io.Reader.
public static error ReadWithReader(io.Reader r, slice<byte> b) {
    {
        var (_, ok) = r._<DefaultReader>(ᐧ); if (ok) {
            Read(b);
            return default!;
        }
    }

    fips140.RecordNonApproved();
    var (_, err) = io.ReadFull(r, b);
    return err;
}

} // end drbg_package
