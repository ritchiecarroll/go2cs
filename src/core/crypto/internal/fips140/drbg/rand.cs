// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using entropy = go.crypto.@internal.entropy_package;
using fips140 = go.crypto.@internal.fips140_package;
using randutil = go.crypto.@internal.randutil_package;
using sysrand = go.crypto.@internal.sysrand_package;
using io = io_package;
using sync = sync_package;
using go.crypto.@internal;

partial class drbg_package {

internal static ж<sync.Pool> Ꮡdrbgs = new StandardBox<sync.Pool>(new sync.Pool(
    New: () => {
        ref var c = ref heap<ж<Counter>>(out var Ꮡc);
        entropy.Depleted(([GoArrayDims(48)] ж<array<byte>> seed) => {
            Ꮡc.ValueSlot = NewCounter(seed);
        });
        return Ꮡc.ValueSlot.OrTypedNil();
    }
));
internal static ref sync.Pool drbgs => ref Ꮡdrbgs.Value;

// Read fills b with cryptographically secure random bytes. In FIPS mode, it
// uses an SP 800-90A Rev. 1 Deterministic Random Bit Generator (DRBG).
// Otherwise, it uses the operating system's random number generator.
public static void Read(slice<byte> b) {
    GoFrame ᒐ = default;
    try {
        if (!fips140.Enabled) {
            sysrand.Read(b);
            return;
        }
        // At every read, 128 random bits from the operating system are mixed as
        // additional input, to make the output as strong as non-FIPS randomness.
        // This is not credited as entropy for FIPS purposes, as allowed by Section
        // 8.7.2: "Note that a DRBG does not rely on additional input to provide
        // entropy, even though entropy could be provided in the additional input".
        ref var additionalInput = ref heap<ж<array<byte>>>(out var ᏑadditionalInput);
        additionalInput = Ꮡ(new array<byte>(48));
        sysrand.Read((~additionalInput)[..16]);
        var drbg = Ꮡdrbgs.Get()._<ж<Counter>>();
        defer(Ꮡdrbgs.Put, drbg.OrTypedNil(), ref ᒐ);
        while (len(b) > 0) {
            nint size = min(len(b), (nint)(maxRequestSize));
            {
                var reseedRequired = drbg.Generate(b.slice(0, size), additionalInput); if (reseedRequired) {
                    // See SP 800-90A Rev. 1, Section 9.3.1, Steps 6-8, as explained in
                    // Section 9.3.2: if Generate reports a reseed is required, the
                    // additional input is passed to Reseed along with the entropy and
                    // then nulled before the next Generate call.
                    var drbgʗ1 = drbg;
                    entropy.Depleted(([GoArrayDims(48)] ж<array<byte>> seed) => {
                        drbgʗ1.Reseed(seed, ᏑadditionalInput.ValueSlot);
                    });
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

// DefaultReader is a sentinel type, embedded in the default
// [crypto/rand.Reader], used to recognize it when passed to
// APIs that accept a rand io.Reader.
partial interface DefaultReader {
    void defaultReader();
}

// ReadWithReader uses Reader to fill b with cryptographically secure random
// bytes. It is intended for use in APIs that expose a rand io.Reader.
//
// If Reader is not the default Reader from crypto/rand,
// [randutil.MaybeReadByte] and [fips140.RecordNonApproved] are called.
public static error ReadWithReader(io.Reader r, slice<byte> b) {
    {
        var (_, ok) = r._<DefaultReader>(ᐧ); if (ok) {
            Read(b);
            return default!;
        }
    }
    fips140.RecordNonApproved();
    randutil.MaybeReadByte(r);
    var (_, err) = io.ReadFull(r, b);
    return err;
}

// ReadWithReaderDeterministic is like ReadWithReader, but it doesn't call
// [randutil.MaybeReadByte] on non-default Readers.
public static error ReadWithReaderDeterministic(io.Reader r, slice<byte> b) {
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
