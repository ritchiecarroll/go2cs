// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !fips140v1.0
namespace go.crypto.@internal.fips140test;

using mldsa = go.crypto.@internal.fips140.mldsa_package;
// blank import: embed_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using fmt = fmt_package;
using go.crypto.@internal.fips140;

partial class fipstest_internal_test_package {

//go:embed acvp_capabilities_fips140v1.26.json
internal static slice<byte> capabilitiesJson = go.embed_package.ΔEmbedBytes<byte>(typeof(fipstest_internal_test_package).Assembly, "go.embed/crypto/internal/fips140test_test/", "acvp_capabilities_fips140v1.26.json");

internal static @string testConfigFile = "acvp_test_fips140v1.26.config.json"u8;

[GoInit] internal static void init() {
    commands["ML-DSA-44/keyGen"u8] = cmdMlDsaKeyGenAft(mldsa.NewPrivateKey44);
    commands["ML-DSA-65/keyGen"u8] = cmdMlDsaKeyGenAft(mldsa.NewPrivateKey65);
    commands["ML-DSA-87/keyGen"u8] = cmdMlDsaKeyGenAft(mldsa.NewPrivateKey87);
    commands["ML-DSA-44/sigGen"u8] = cmdMlDsaSigGenAft();
    commands["ML-DSA-65/sigGen"u8] = cmdMlDsaSigGenAft();
    commands["ML-DSA-87/sigGen"u8] = cmdMlDsaSigGenAft();
    commands["ML-DSA-44/sigVer"u8] = cmdMlDsaSigVerAft(mldsa.NewPublicKey44);
    commands["ML-DSA-65/sigVer"u8] = cmdMlDsaSigVerAft(mldsa.NewPublicKey65);
    commands["ML-DSA-87/sigVer"u8] = cmdMlDsaSigVerAft(mldsa.NewPublicKey87);
}

internal static command cmdMlDsaKeyGenAft(Func<slice<byte>, (ж<mldsa.PrivateKey>, error)> keyGen) {
    return new command(
        requiredArgs: 1, // Seed
        handler: (slice<slice<byte>> args) => {
            var seed = args[0];

            var (sk, err) = keyGen(seed);
            if (err != default!) {
                return (default!, fmt.Errorf("generating ML-DSA 44 private key: %w"u8, err));
            }

            // Important: we must return the full encoding of sk, not the seed.
            return (new slice<byte>[]{sk.PublicKey().Bytes(), mldsa.TestingOnlyPrivateKeySemiExpandedBytes(sk)}.slice(), default!);
        }
    );
}

internal static command cmdMlDsaSigGenAft() {
    return new command(
        requiredArgs: 5, // secret key, message, randomizer, mu, context
        handler: (slice<slice<byte>> args) => {
            var skSmiExpanded = args[0];
            var message = args[1]; // Optional, exclusive with mu
            var randomizer = args[2]; // Optional
            @string context = (@string)args[3]; // Optional
            var mu = args[4]; // Optional, exclusive with message

            var (sk, err) = mldsa.TestingOnlyNewPrivateKeyFromSemiExpanded(skSmiExpanded);
            if (err != default!) {
                return (default!, fmt.Errorf("making ML-DSA private key from semi-expanded form: %w"u8, err));
            }

            var haveMessage = len(message) != 0;
            var haveRandomizer = len(randomizer) != 0;
            var haveMu = len(mu) != 0;

            slice<byte> sig = default!;
            if (haveMessage && !haveRandomizer && !haveMu){
                (sig, err) = mldsa.SignDeterministic(sk, message, context);
            } else 
            if (haveMessage && haveRandomizer && !haveMu){
                (sig, err) = mldsa.TestingOnlySignWithRandom(sk, message, context, randomizer);
            } else 
            if (!haveMessage && !haveRandomizer && haveMu){
                (sig, err) = mldsa.SignExternalMuDeterministic(sk, mu);
            } else 
            if (!haveMessage && haveRandomizer && haveMu){
                (sig, err) = mldsa.TestingOnlySignExternalMuWithRandom(sk, mu, randomizer);
            } else {
                return (default!, fmt.Errorf(
                    "unsupported ML-DSA sigGen args: have message=%v have randomizer=%v haveMu=%v haveContext=%v"u8,
                    haveMessage, haveRandomizer, haveMu, len(context) != 0));
            }

            if (err != default!) {
                return (default!, fmt.Errorf("creating deterministic ML-DSA signature: %w"u8, err));
            }

            return (new slice<byte>[]{sig}.slice(), default!);
        }
    );
}

internal static command cmdMlDsaSigVerAft(Func<slice<byte>, (ж<mldsaꓸPublicKey>, error)> pubKey) {
    return new command(
        requiredArgs: 5, // public key, message, signature, context, mu
        handler: (slice<slice<byte>> args) => {
            var pkRaw = args[0];
            var message = args[1]; // Optional, exclusive with mu
            var signature = args[2];
            @string context = (@string)args[3]; // Optional
            var mu = args[4]; // Optional, exclusive with message

            var (pk, err) = pubKey(pkRaw);
            if (err != default!) {
                return (default!, fmt.Errorf("loading ML-DSA public key: %w"u8, err));
            }

            var haveMessage = len(message) != 0;
            var haveMu = len(mu) != 0;
            if (haveMessage && !haveMu){
                err = mldsa.Verify(pk, message, signature, context);
            } else 
            if (!haveMessage && haveMu){
                err = mldsa.VerifyExternalMu(pk, mu, signature);
            } else {
                return (default!, fmt.Errorf(
                    "unsupported ML-DSA sigVer args: have message=%v haveMu=%v haveContext=%v"u8,
                    haveMessage, haveMu, len(context) != 0));
            }

            if (err != default!) {
                return (new slice<byte>[]{new byte[]{0}.slice()}.slice(), default!);
            }

            return (new slice<byte>[]{new byte[]{1}.slice()}.slice(), default!);
        }
    );
}

} // end fipstest_internal_test_package
