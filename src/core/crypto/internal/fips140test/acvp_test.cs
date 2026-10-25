// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140test;

// A module wrapper adapting the Go FIPS module to the protocol used by the
// BoringSSL project's `acvptool`.
//
// The `acvptool` "lowers" the NIST ACVP server JSON test vectors into a simpler
// stdin/stdout protocol that can be implemented by a module shim. The tool
// will fork this binary, request the supported configuration, and then provide
// test cases over stdin, expecting results to be returned on stdout.
//
// See "Testing other FIPS modules"[0] from the BoringSSL ACVP.md documentation
// for a more detailed description of the protocol used between the acvptool
// and module wrappers.
//
// [0]: https://boringssl.googlesource.com/boringssl/+/refs/heads/master/util/fipstools/acvp/ACVP.md#testing-other-fips-modules
using bufio = bufio_package;
using bytes = bytes_package;
using elliptic = go.crypto.elliptic_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using entropy = go.crypto.@internal.entropy.v1_0_0.entropy_package;
using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
using gcm = go.crypto.@internal.fips140.aes.gcm_package;
using bigmod = go.crypto.@internal.fips140.bigmod_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using ecdh = go.crypto.@internal.fips140.ecdh_package;
using ecdsa = go.crypto.@internal.fips140.ecdsa_package;
using ed25519 = go.crypto.@internal.fips140.ed25519_package;
using edwards25519 = go.crypto.@internal.fips140.edwards25519_package;
using hkdf = go.crypto.@internal.fips140.hkdf_package;
using hmac = go.crypto.@internal.fips140.hmac_package;
using mlkem = go.crypto.@internal.fips140.mlkem_package;
using pbkdf2 = go.crypto.@internal.fips140.pbkdf2_package;
using rsa = go.crypto.@internal.fips140.rsa_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using sha512 = go.crypto.@internal.fips140.sha512_package;
using ssh = go.crypto.@internal.fips140.ssh_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using tls12 = go.crypto.@internal.fips140.tls12_package;
using tls13 = go.crypto.@internal.fips140.tls13_package;
using impl = go.crypto.@internal.impl_package;
using rand = go.crypto.rand_package;
// blank import: embed_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using big = math.big_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using exec = go.os.exec_package;
using fs = go.io.fs_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140.aes;
using go.os;
using math;
using nistec = go.crypto.@internal.fips140.nistec_package;
using path;

partial class fipstest_internal_test_package {

internal static bool noPAAPAI = os.Getenv("GONOPAAPAI"u8) == "1"u8;

// Use the capabilities, configuration and commands for the entropy source.
// This is used to test the separate entropy source in crypto/internal/entropy
// since the algorithm name alone can't indicate which to test.
internal static bool entropyTesting = os.Getenv("GOENTROPYSOURCEACVP"u8) == "1"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string acvpWrapperˢ = "ACVP_WRAPPER"u8;

public static void TestMain(ж<testing.M> Ꮡm) {
    if (noPAAPAI) {
        foreach (var (_, p) in impl.Packages()) {
            impl.Select(p, ""u8);
        }
    }
    if (os.Getenv(acvpWrapperˢ) == "1"u8){
        wrapperMain();
    } else {
        os.Exit(Ꮡm.Run());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object acvpWrapperMustBeRunWithˢ = (@string)"ACVP wrapper must be run with GODEBUG=fips140=on"u8;

internal static void wrapperMain() {
    if (!fips140.Enabled) {
        fmt.Fprintln(new os.FileжWriter(os.Stderr), acvpWrapperMustBeRunWithˢ);
        os.Exit(2);
    }
    {
        var err = processingLoop(new fipstest_internal_test_package.bufio_ReaderжReader(bufio.NewReader(new fipstest_internal_test_package.os_FileжReader(os.Stdin))), new os.FileжWriter(os.Stdout)); if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "processing error: %v\n"u8, err);
            os.Exit(1);
        }
    }
}

internal partial struct request {
    internal @string name;
    internal slice<slice<byte>> args;
}

// type commandHandler is a methodless func type — rendered inline as its base delegate

internal partial struct command {
    // requiredArgs enforces that an exact number of arguments are provided to the handler.
    internal nint requiredArgs;
    internal Func<slice<slice<byte>>, (slice<slice<byte>>, error)> handler;
}

internal partial struct ecdsaSigType /*num:nint*/;

internal static ecdsaSigType ecdsaSigTypeNormal => /* iota */ 0;
internal static ecdsaSigType ecdsaSigTypeDeterministic => 1;

internal partial struct aesDirection /*num:nint*/;

internal static aesDirection aesEncrypt => /* iota */ 0;
internal static aesDirection aesDecrypt => 1;

// Separate capabilities specific to testing the entropy source's SHA2-384 implementation.
// This implementation differs from the FIPS module's SHA2-384 in its supported input sizes.
// Set the GOENTROPYSOURCEACVP environment variable to use these capabilities in place of
// capabilitiesJson
//go:embed acvp_capabilities.entropy.json
internal static slice<byte> entropyCapabilitiesJson = go.embed_package.ΔEmbedBytes<byte>(typeof(fipstest_internal_test_package).Assembly, "go.embed/crypto/internal/fips140test_test/", "acvp_capabilities.entropy.json");

// commands should reflect what config says we support. E.g. adding a command here will be a NOP
// unless the configuration/acvp_capabilities.json indicates the command's associated algorithm
// is supported.
internal static map<@string, command> commands;
internal static void initᴛcommands() { commands = new map<@string, command>{
    ["getConfig"u8] = cmdGetConfig(),

    ["SHA2-224"u8] = cmdHashAft(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224())),
    ["SHA2-224/MCT"u8] = cmdHashMct(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224())),
    ["SHA2-256"u8] = cmdHashAft(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["SHA2-256/MCT"u8] = cmdHashMct(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["SHA2-384"u8] = cmdHashAft(new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["SHA2-384/MCT"u8] = cmdHashMct(new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["SHA2-512"u8] = cmdHashAft(new fipstest_internal_test_package.sha512_DigestжHash(sha512.New())),
    ["SHA2-512/MCT"u8] = cmdHashMct(new fipstest_internal_test_package.sha512_DigestжHash(sha512.New())),
    ["SHA2-512/224"u8] = cmdHashAft(new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_224())),
    ["SHA2-512/224/MCT"u8] = cmdHashMct(new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_224())),
    ["SHA2-512/256"u8] = cmdHashAft(new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_256())),
    ["SHA2-512/256/MCT"u8] = cmdHashMct(new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_256())),

    ["SHA3-256"u8] = cmdHashAft(new fipstest_internal_test_package.sha3_DigestжHash(sha3.New256())),
    ["SHA3-256/MCT"u8] = cmdSha3Mct(new fipstest_internal_test_package.sha3_DigestжHash(sha3.New256())),
    ["SHA3-224"u8] = cmdHashAft(new fipstest_internal_test_package.sha3_DigestжHash(sha3.New224())),
    ["SHA3-224/MCT"u8] = cmdSha3Mct(new fipstest_internal_test_package.sha3_DigestжHash(sha3.New224())),
    ["SHA3-384"u8] = cmdHashAft(new fipstest_internal_test_package.sha3_DigestжHash(sha3.New384())),
    ["SHA3-384/MCT"u8] = cmdSha3Mct(new fipstest_internal_test_package.sha3_DigestжHash(sha3.New384())),
    ["SHA3-512"u8] = cmdHashAft(new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512())),
    ["SHA3-512/MCT"u8] = cmdSha3Mct(new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512())),

    // Note: the "/ENTROPY" suffix is our own creation, and applied conditionally
    // based on the environment variable that indicates our acvp_test module wrapper
    // is being used for evaluating the separate SHA-384 implementation for the
    // CPU jitter entropy conditioning. Set GOENTROPYSOURCEACVP=1 to use these commands
    // in place of SHA2-384.
    ["SHA2-384/ENTROPY"u8] = cmdEntropyHashEntropySha384Aft(),
    ["SHA2-384/MCT/ENTROPY"u8] = cmdEntropyHashEntropySha384Mct(),

    // Note: SHAKE AFT and VOT test types can be handled by the same command
    // handler impl, but use distinct acvptool command names, and so are
    // registered twice with the same digest: once under "SHAKE-xxx" for AFT,
    // and once under"SHAKE-xxx/VOT" for VOT.
    ["SHAKE-128"u8] = cmdShakeAftVot(sha3.NewShake128()),
    ["SHAKE-128/VOT"u8] = cmdShakeAftVot(sha3.NewShake128()),
    ["SHAKE-128/MCT"u8] = cmdShakeMct(sha3.NewShake128()),
    ["SHAKE-256"u8] = cmdShakeAftVot(sha3.NewShake256()),
    ["SHAKE-256/VOT"u8] = cmdShakeAftVot(sha3.NewShake256()),
    ["SHAKE-256/MCT"u8] = cmdShakeMct(sha3.NewShake256()),

    ["cSHAKE-128"u8] = cmdCShakeAft((slice<byte> N, slice<byte> S) => sha3.NewCShake128(N, S)),
    ["cSHAKE-128/MCT"u8] = cmdCShakeMct((slice<byte> N, slice<byte> S) => sha3.NewCShake128(N, S)),
    ["cSHAKE-256"u8] = cmdCShakeAft((slice<byte> N, slice<byte> S) => sha3.NewCShake256(N, S)),
    ["cSHAKE-256/MCT"u8] = cmdCShakeMct((slice<byte> N, slice<byte> S) => sha3.NewCShake256(N, S)),

    ["HMAC-SHA2-224"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224())),
    ["HMAC-SHA2-256"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["HMAC-SHA2-384"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["HMAC-SHA2-512"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New())),
    ["HMAC-SHA2-512/224"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_224())),
    ["HMAC-SHA2-512/256"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_256())),
    ["HMAC-SHA3-224"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New224())),
    ["HMAC-SHA3-256"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New256())),
    ["HMAC-SHA3-384"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New384())),
    ["HMAC-SHA3-512"u8] = cmdHmacAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512())),

    ["HKDF/SHA2-224"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224())),
    ["HKDF/SHA2-256"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["HKDF/SHA2-384"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["HKDF/SHA2-512"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New())),
    ["HKDF/SHA2-512/224"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_224())),
    ["HKDF/SHA2-512/256"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_256())),
    ["HKDF/SHA3-224"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New224())),
    ["HKDF/SHA3-256"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New256())),
    ["HKDF/SHA3-384"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New384())),
    ["HKDF/SHA3-512"u8] = cmdHkdfAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512())),

    ["HKDFExtract/SHA2-256"u8] = cmdHkdfExtractAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["HKDFExtract/SHA2-384"u8] = cmdHkdfExtractAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["HKDFExpandLabel/SHA2-256"u8] = cmdHkdfExpandLabelAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["HKDFExpandLabel/SHA2-384"u8] = cmdHkdfExpandLabelAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),

    ["PBKDF"u8] = cmdPbkdf(),

    ["ML-KEM-768/keyGen"u8] = cmdMlKem768KeyGenAft(),
    ["ML-KEM-768/encap"u8] = cmdMlKem768EncapAft(),
    ["ML-KEM-768/decap"u8] = cmdMlKem768DecapAft(),
    ["ML-KEM-1024/keyGen"u8] = cmdMlKem1024KeyGenAft(),
    ["ML-KEM-1024/encap"u8] = cmdMlKem1024EncapAft(),
    ["ML-KEM-1024/decap"u8] = cmdMlKem1024DecapAft(),

    ["hmacDRBG/SHA2-224"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224())),
    ["hmacDRBG/SHA2-256"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["hmacDRBG/SHA2-384"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["hmacDRBG/SHA2-512"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New())),
    ["hmacDRBG/SHA2-512/224"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_224())),
    ["hmacDRBG/SHA2-512/256"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_256())),
    ["hmacDRBG/SHA3-224"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New224())),
    ["hmacDRBG/SHA3-256"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New256())),
    ["hmacDRBG/SHA3-384"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New384())),
    ["hmacDRBG/SHA3-512"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512())),

    ["EDDSA/keyGen"u8] = cmdEddsaKeyGenAft(),
    ["EDDSA/keyVer"u8] = cmdEddsaKeyVerAft(),
    ["EDDSA/sigGen"u8] = cmdEddsaSigGenAftBft(),
    ["EDDSA/sigVer"u8] = cmdEddsaSigVerAft(),

    ["ECDSA/keyGen"u8] = cmdEcdsaKeyGenAft(),
    ["ECDSA/keyVer"u8] = cmdEcdsaKeyVerAft(),
    ["ECDSA/sigGen"u8] = cmdEcdsaSigGenAft(ecdsaSigTypeNormal),
    ["ECDSA/sigVer"u8] = cmdEcdsaSigVerAft(),
    ["DetECDSA/sigGen"u8] = cmdEcdsaSigGenAft(ecdsaSigTypeDeterministic),

    ["AES-CBC/encrypt"u8] = cmdAesCbc(aesEncrypt),
    ["AES-CBC/decrypt"u8] = cmdAesCbc(aesDecrypt),
    ["AES-CTR/encrypt"u8] = cmdAesCtr(aesEncrypt),
    ["AES-CTR/decrypt"u8] = cmdAesCtr(aesDecrypt),
    ["AES-GCM/seal"u8] = cmdAesGcmSeal(false),
    ["AES-GCM/open"u8] = cmdAesGcmOpen(false),
    ["AES-GCM-randnonce/seal"u8] = cmdAesGcmSeal(true),
    ["AES-GCM-randnonce/open"u8] = cmdAesGcmOpen(true),

    ["CMAC-AES"u8] = cmdCmacAesAft(),
    ["CMAC-AES/verify"u8] = cmdCmacAesVerifyAft(),

    // Note: Only SHA2-256, SHA2-384 and SHA2-512 are valid hash functions for TLSKDF.
    // 		 See https://pages.nist.gov/ACVP/draft-celi-acvp-kdf-tls.html#section-7.2.1
    ["TLSKDF/1.2/SHA2-256"u8] = cmdTlsKdf12Aft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["TLSKDF/1.2/SHA2-384"u8] = cmdTlsKdf12Aft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["TLSKDF/1.2/SHA2-512"u8] = cmdTlsKdf12Aft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New())),

    // Note: only SHA2-224, SHA2-256, SHA2-384 and SHA2-512 are valid hash functions for SSHKDF.
    // 		 See https://pages.nist.gov/ACVP/draft-celi-acvp-kdf-ssh.html#section-7.2.1
    ["SSHKDF/SHA2-224/client"u8] = cmdSshKdfAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224()), ssh.ClientKeys),
    ["SSHKDF/SHA2-224/server"u8] = cmdSshKdfAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224()), ssh.ServerKeys),
    ["SSHKDF/SHA2-256/client"u8] = cmdSshKdfAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), ssh.ClientKeys),
    ["SSHKDF/SHA2-256/server"u8] = cmdSshKdfAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), ssh.ServerKeys),
    ["SSHKDF/SHA2-384/client"u8] = cmdSshKdfAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384()), ssh.ClientKeys),
    ["SSHKDF/SHA2-384/server"u8] = cmdSshKdfAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384()), ssh.ServerKeys),
    ["SSHKDF/SHA2-512/client"u8] = cmdSshKdfAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New()), ssh.ClientKeys),
    ["SSHKDF/SHA2-512/server"u8] = cmdSshKdfAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New()), ssh.ServerKeys),

    ["ECDH/P-224"u8] = cmdEcdhAftVal<ecdh.P224PointжPoint>(ecdh.P224()),
    ["ECDH/P-256"u8] = cmdEcdhAftVal<ecdh.P256PointжPoint>(ecdh.P256()),
    ["ECDH/P-384"u8] = cmdEcdhAftVal<ecdh.P384PointжPoint>(ecdh.P384()),
    ["ECDH/P-521"u8] = cmdEcdhAftVal<ecdh.P521PointжPoint>(ecdh.P521()),

    ["ctrDRBG/AES-256"u8] = cmdCtrDrbgAft(),
    ["ctrDRBG-reseed/AES-256"u8] = cmdCtrDrbgReseedAft(),

    ["RSA/keyGen"u8] = cmdRsaKeyGenAft(),

    ["RSA/sigGen/SHA2-224/pkcs1v1.5"u8] = cmdRsaSigGenAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224()), "SHA-224"u8, false),
    ["RSA/sigGen/SHA2-256/pkcs1v1.5"u8] = cmdRsaSigGenAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), "SHA-256"u8, false),
    ["RSA/sigGen/SHA2-384/pkcs1v1.5"u8] = cmdRsaSigGenAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384()), "SHA-384"u8, false),
    ["RSA/sigGen/SHA2-512/pkcs1v1.5"u8] = cmdRsaSigGenAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New()), "SHA-512"u8, false),
    ["RSA/sigGen/SHA2-224/pss"u8] = cmdRsaSigGenAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224()), "SHA-224"u8, true),
    ["RSA/sigGen/SHA2-256/pss"u8] = cmdRsaSigGenAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), "SHA-256"u8, true),
    ["RSA/sigGen/SHA2-384/pss"u8] = cmdRsaSigGenAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384()), "SHA-384"u8, true),
    ["RSA/sigGen/SHA2-512/pss"u8] = cmdRsaSigGenAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New()), "SHA-512"u8, true),

    ["RSA/sigVer/SHA2-224/pkcs1v1.5"u8] = cmdRsaSigVerAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224()), "SHA-224"u8, false),
    ["RSA/sigVer/SHA2-256/pkcs1v1.5"u8] = cmdRsaSigVerAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), "SHA-256"u8, false),
    ["RSA/sigVer/SHA2-384/pkcs1v1.5"u8] = cmdRsaSigVerAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384()), "SHA-384"u8, false),
    ["RSA/sigVer/SHA2-512/pkcs1v1.5"u8] = cmdRsaSigVerAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New()), "SHA-512"u8, false),
    ["RSA/sigVer/SHA2-224/pss"u8] = cmdRsaSigVerAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224()), "SHA-224"u8, true),
    ["RSA/sigVer/SHA2-256/pss"u8] = cmdRsaSigVerAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), "SHA-256"u8, true),
    ["RSA/sigVer/SHA2-384/pss"u8] = cmdRsaSigVerAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384()), "SHA-384"u8, true),
    ["RSA/sigVer/SHA2-512/pss"u8] = cmdRsaSigVerAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New()), "SHA-512"u8, true),

    ["KDF-counter"u8] = cmdKdfCounterAft(),
    ["KDF-feedback"u8] = cmdKdfFeedbackAft(),

    ["OneStepNoCounter/HMAC-SHA2-224"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224())),
    ["OneStepNoCounter/HMAC-SHA2-256"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["OneStepNoCounter/HMAC-SHA2-384"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["OneStepNoCounter/HMAC-SHA2-512"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New())),
    ["OneStepNoCounter/HMAC-SHA2-512/224"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_224())),
    ["OneStepNoCounter/HMAC-SHA2-512/256"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_256())),
    ["OneStepNoCounter/HMAC-SHA3-224"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New224())),
    ["OneStepNoCounter/HMAC-SHA3-256"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New256())),
    ["OneStepNoCounter/HMAC-SHA3-384"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New384())),
    ["OneStepNoCounter/HMAC-SHA3-512"u8] = cmdOneStepNoCounterHmacAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512())),

    ["KTS-IFC/SHA2-224/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224())),
    ["KTS-IFC/SHA2-224/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224())),
    ["KTS-IFC/SHA2-256/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["KTS-IFC/SHA2-256/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New())),
    ["KTS-IFC/SHA2-384/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["KTS-IFC/SHA2-384/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384())),
    ["KTS-IFC/SHA2-512/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New())),
    ["KTS-IFC/SHA2-512/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New())),
    ["KTS-IFC/SHA2-512/224/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_224())),
    ["KTS-IFC/SHA2-512/224/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_224())),
    ["KTS-IFC/SHA2-512/256/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_256())),
    ["KTS-IFC/SHA2-512/256/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_256())),
    ["KTS-IFC/SHA3-224/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New224())),
    ["KTS-IFC/SHA3-224/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New224())),
    ["KTS-IFC/SHA3-256/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New256())),
    ["KTS-IFC/SHA3-256/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New256())),
    ["KTS-IFC/SHA3-384/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New384())),
    ["KTS-IFC/SHA3-384/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New384())),
    ["KTS-IFC/SHA3-512/initiator"u8] = cmdKtsIfcInitiatorAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512())),
    ["KTS-IFC/SHA3-512/responder"u8] = cmdKtsIfcResponderAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512()))
}; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sha2384ˢ = "SHA2-384"u8;

internal static error processingLoop(io.Reader reader, io.Writer writer) {
    // Per ACVP.md:
    //   The protocol is request–response: the subprocess only speaks in response to a request
    //   and there is exactly one response for every request.
    while (ᐧ) {
        var (req, err) = readRequest(reader);
        if (errors.Is(err, io.EOF)){
            break;
        } else 
        if (err != default!) {
            return fmt.Errorf("reading request: %w"u8, err);
        }

        if (entropyTesting && strings.HasPrefix((~req).name, sha2384ˢ)) {
            req.Value.name = fmt.Sprintf("%s/ENTROPY"u8, (~req).name);
        }

        var (cmd, exists) = commands[(~req).name, ꟷ];
        if (!exists) {
            return fmt.Errorf("unknown command: %q"u8, (~req).name);
        }

        {
            nint gotArgs = len((~req).args); if (gotArgs != cmd.requiredArgs) {
                return fmt.Errorf("command %q expected %d args, got %d"u8, (~req).name, cmd.requiredArgs, gotArgs);
            }
        }

        (var response, err) = cmd.handler((~req).args);
        if (err != default!) {
            return fmt.Errorf("command %q failed: %w"u8, (~req).name, err);
        }

        {
            err = writeResponse(writer, response); if (err != default!) {
                return fmt.Errorf("command %q response failed: %w"u8, (~req).name, err);
            }
        }
    }

    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidRequestZeroArgsˢ = "invalid request: zero args"u8;

internal static (ж<request>, error) readRequest(io.Reader reader) {
    // Per ACVP.md:
    //   Requests consist of one or more byte strings and responses consist
    //   of zero or more byte strings. A request contains: the number of byte
    //   strings, the length of each byte string, and the contents of each byte
    //   string. All numbers are 32-bit little-endian and values are
    //   concatenated in the order specified.
    ref var numArgs = ref heap(new uint32(), out var ᏑnumArgs);
    {
        var errΔ1 = binary.Read(reader, binary.LittleEndian, ᏑnumArgs); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (numArgs == 0) {
        return (default!, errors.New(invalidRequestZeroArgsˢ));
    }

    var (args, err) = readArgs(reader, numArgs);
    if (err != default!) {
        return (default!, err);
    }

    return (Ꮡ(new request(
        name: ((@string)args[0]),
        args: args[1..]
    )), default!);
}

internal static (slice<slice<byte>>, error) readArgs(io.Reader reader, uint32 requiredArgs) {
    var argLengths = new slice<uint32>((nint)(requiredArgs));
    var args = new slice<slice<byte>>((nint)(requiredArgs));

    foreach (var i in range(argLengths)) {
        {
            var err = binary.Read(reader, binary.LittleEndian, Ꮡ(argLengths, i)); if (err != default!) {
                return (default!, fmt.Errorf("invalid request: failed to read %d-th arg len: %w"u8, i, err));
            }
        }
    }

    foreach (var (i, length) in argLengths) {
        var buf = new slice<byte>((nint)(length));
        {
            var (_, err) = io.ReadFull(reader, buf); if (err != default!) {
                return (default!, fmt.Errorf("invalid request: failed to read %d-th arg data: %w"u8, i, err));
            }
        }
        args[i] = buf;
    }

    return (args, default!);
}

internal static error writeResponse(io.Writer writer, slice<slice<byte>> args) {
    // See `readRequest` for details on the base format. Per ACVP.md:
    //   A response has the same format except that there may be zero byte strings
    //   and the first byte string has no special meaning.
    var numArgs = (uint32)len(args);
    {
        var err = binary.Write(writer, binary.LittleEndian, numArgs); if (err != default!) {
            return fmt.Errorf("writing arg count: %w"u8, err);
        }
    }

    foreach (var (i, arg) in args) {
        {
            var err = binary.Write(writer, binary.LittleEndian, (uint32)len(arg)); if (err != default!) {
                return fmt.Errorf("writing %d-th arg length: %w"u8, i, err);
            }
        }
    }

    foreach (var (i, b) in args) {
        {
            var (_, err) = writer.Write(b); if (err != default!) {
                return fmt.Errorf("writing %d-th arg data: %w"u8, i, err);
            }
        }
    }

    return default!;
}

// "All implementations must support the getConfig command
// which takes no arguments and returns a single byte string
// which is a JSON blob of ACVP algorithm configuration."
internal static command cmdGetConfig() {
    // If GOENTROPYSOURCEACVP is set, then use the entropyCapabilitiesJson
    // instead of capabilitiesJson.
    var capabilities = new slice<byte>[]{capabilitiesJson}.slice();
    if (entropyTesting) {
        capabilities = new slice<byte>[]{entropyCapabilitiesJson}.slice();
    }

        var capabilitiesʗ1 = capabilities;
    return new command(
        handler: (slice<slice<byte>> args) => (capabilitiesʗ1, default!)
    );
}

// cmdHashAft returns a command handler for the specified hash
// algorithm for algorithm functional test (AFT) test cases.
//
// This shape of command expects a message as the sole argument,
// and writes the resulting digest as a response.
//
// See https://pages.nist.gov/ACVP/draft-celi-acvp-sha.html
internal static command cmdHashAft(hash.Hash h) {
    return new command(
        requiredArgs: 1, // Message to hash.
        handler: (slice<slice<byte>> args) => {
            h.Reset();
            h.Write(args[0]);
            var digest = new slice<byte>(0, h.Size());
            digest = h.Sum(digest);

            return (new slice<byte>[]{digest}.slice(), default!);
        }
    );
}

// cmdHashMct returns a command handler for the specified hash
// algorithm for monte carlo test (MCT) test cases.
//
// This shape of command expects a seed as the sole argument,
// and writes the resulting digest as a response. It implements
// the "standard" flavour of the MCT, not the "alternative".
//
// This algorithm was ported from `HashMCT` in BSSL's `modulewrapper.cc`
// Note that it differs slightly from the upstream NIST MCT[0] algorithm
// in that it does not perform the outer 100 iterations itself. See
// footnote #1 in the ACVP.md docs[1], the acvptool handles this.
//
// [0]: https://pages.nist.gov/ACVP/draft-celi-acvp-sha.html#section-6.2
// [1]: https://boringssl.googlesource.com/boringssl/+/refs/heads/master/util/fipstools/acvp/ACVP.md#testing-other-fips-modules
internal static command cmdHashMct(hash.Hash h) {
    return new command(
        requiredArgs: 1, // Seed message.
        handler: (slice<slice<byte>> args) => {
            nint hSize = h.Size();
            var seed = args[0];

            {
                nint seedLen = len(seed); if (seedLen != hSize) {
                    return (default!, fmt.Errorf("invalid seed size: expected %d got %d"u8, hSize, seedLen));
                }
            }

            var digest = new slice<byte>(0, hSize);
            var buf = new slice<byte>(0, 3 * hSize);
            buf = appendꓸꓸꓸ(buf, seed);
            buf = appendꓸꓸꓸ(buf, seed);
            buf = appendꓸꓸꓸ(buf, seed);

            for (nint i = 0; i < 1000; i++) {
                h.Reset();
                h.Write(buf);
                digest = h.Sum(digest[..0]);

                copy(buf, buf.slice(hSize));
                copy(buf.slice(2 * hSize), digest);
            }

            return (new slice<byte>[]{buf.slice(hSize * 2)}.slice(), default!);
        }
    );
}

// cmdEntropyHashEntropySha384Aft returns a command handler that tests the
// entropy package's SHA2-384 digest for AFT inputs.
internal static command cmdEntropyHashEntropySha384Aft() {
    return new command(
        requiredArgs: 1, // Message to hash.
        handler: (slice<slice<byte>> args) => {
            var digest = entropy.TestingOnlySHA384(args[0]);
            return (new slice<byte>[]{digest[..]}.slice(), default!);
        }
    );
}

// cmdEntropyHashEntropySha384Mct returns a command handler that tests the
// entropy package's SHA2-384 digest for MCT inputs.
internal static command cmdEntropyHashEntropySha384Mct() {
    return new command(
        requiredArgs: 1, // Seed message.
        handler: (slice<slice<byte>> args) => {
            nint hSize = 48;
            var seed = args[0];

            var digest = new slice<byte>(0, hSize);
            var buf = new slice<byte>(0, 3 * hSize);
            buf = appendꓸꓸꓸ(buf, seed);
            buf = appendꓸꓸꓸ(buf, seed);
            buf = appendꓸꓸꓸ(buf, seed);

            for (nint i = 0; i < 1000; i++) {
                var digestRaw = entropy.TestingOnlySHA384(buf);
                digest = digestRaw.slice(0, hSize);

                copy(buf, buf.slice(hSize));
                copy(buf.slice(2 * hSize), digest);
            }

            return (new slice<byte>[]{buf.slice(hSize * 2)}.slice(), default!);
        }
    );
}

// cmdSha3Mct returns a command handler for the specified hash
// algorithm for SHA-3 monte carlo test (MCT) test cases.
//
// This shape of command expects a seed as the sole argument,
// and writes the resulting digest as a response. It implements
// the "standard" flavour of the MCT, not the "alternative".
//
// This algorithm was ported from the "standard" MCT algorithm
// specified in  draft-celi-acvp-sha3[0]. Note this differs from
// the SHA2-* family of MCT tests handled by cmdHashMct. However,
// like that handler it does not perform the outer 100 iterations.
//
// [0]: https://pages.nist.gov/ACVP/draft-celi-acvp-sha3.html#section-6.2.1
internal static command cmdSha3Mct(hash.Hash h) {
    return new command(
        requiredArgs: 1, // Seed message.
        handler: (slice<slice<byte>> args) => {
            var seed = args[0];
            var md = new slice<slice<byte>>(1001);
            md[0] = seed;

            for (nint i = 1; i <= 1000; i++) {
                h.Reset();
                h.Write(md[i - 1]);
                md[i] = h.Sum(default!);
            }

            return (new slice<byte>[]{md[1000]}.slice(), default!);
        }
    );
}

internal static command cmdShakeAftVot(ж<sha3.SHAKE> Ꮡh) {
    return new command(
        requiredArgs: 2, // Message, output length (bytes)
        handler: (slice<slice<byte>> args) => {
            var msg = args[0];

            var outLenBytes = binary.LittleEndian.Uint32(args[1]);
            var digest = new slice<byte>((nint)(outLenBytes));

            Ꮡh.Reset();
            Ꮡh.Write(msg);
            Ꮡh.Read(digest);

            return (new slice<byte>[]{digest}.slice(), default!);
        }
    );
}

internal static command cmdShakeMct(ж<sha3.SHAKE> Ꮡh) {
    return new command(
        requiredArgs: 4, // Seed message, min output length (bytes), max output length (bytes), output length (bytes)
        handler: (slice<slice<byte>> args) => {
            var md = args[0];
            var minOutBytes = binary.LittleEndian.Uint32(args[1]);
            var maxOutBytes = binary.LittleEndian.Uint32(args[2]);

            var outputLenBytes = binary.LittleEndian.Uint32(args[3]);
            if (outputLenBytes < 2) {
                return (default!, fmt.Errorf("invalid output length: %d"u8, outputLenBytes));
            }

            var rangeBytes = maxOutBytes - minOutBytes + 1;
            if (rangeBytes == 0) {
                return (default!, fmt.Errorf("invalid maxOutBytes and minOutBytes: %d, %d"u8, maxOutBytes, minOutBytes));
            }

            for (nint i = 0; i < 1000; i++) {
                // "The MSG[i] input to SHAKE MUST always contain at least 128 bits. If this is not the case
                // as the previous digest was too short, append empty bits to the rightmost side of the digest."
                nint boundary = min(len(md), 16);
                var msg = new slice<byte>(16);
                copy(msg, md.slice(0, boundary));

                //  MD[i] = SHAKE(MSG[i], OutputLen * 8)
                Ꮡh.Reset();
                Ꮡh.Write(msg);
                var digest = new slice<byte>((nint)(outputLenBytes));
                Ꮡh.Read(digest);
                md = digest;

                // RightmostOutputBits = 16 rightmost bits of MD[i] as an integer
                // OutputLen = minOutBytes + (RightmostOutputBits % Range)
                var rightmostOutput = (uint32)md[outputLenBytes - 2] << 8 | (uint32)md[outputLenBytes - 1];
                outputLenBytes = minOutBytes + (rightmostOutput % rangeBytes);
            }

            var encodedOutputLenBytes = new slice<byte>(4);
            binary.LittleEndian.PutUint32(encodedOutputLenBytes, outputLenBytes);

            return (new slice<byte>[]{md, encodedOutputLenBytes}.slice(), default!);
        }
    );
}

internal static command cmdCShakeAft(Func<slice<byte>, slice<byte>, ж<sha3.SHAKE>> hFn) {
    return new command(
        requiredArgs: 4, // Message, output length bytes, function name, customization
        handler: (slice<slice<byte>> args) => {
            var msg = args[0];
            var outLenBytes = binary.LittleEndian.Uint32(args[1]);
            var functionName = args[2];
            var customization = args[3];

            var h = hFn(functionName, customization);
            h.Write(msg);

            var @out = new slice<byte>((nint)(outLenBytes));
            h.Read(@out);

            return (new slice<byte>[]{@out}.slice(), default!);
        }
    );
}

internal static command cmdCShakeMct(Func<slice<byte>, slice<byte>, ж<sha3.SHAKE>> hFn) {
    return new command(
        requiredArgs: 6, // Message, min output length (bits), max output length (bits), output length (bits), increment (bits), customization
        handler: (slice<slice<byte>> args) => {
            var message = args[0];
            var minOutLenBytes = binary.LittleEndian.Uint32(args[1]);
            var maxOutLenBytes = binary.LittleEndian.Uint32(args[2]);
            var outputLenBytes = binary.LittleEndian.Uint32(args[3]);
            var incrementBytes = binary.LittleEndian.Uint32(args[4]);
            var customization = args[5];

            if (outputLenBytes < 2) {
                return (default!, fmt.Errorf("invalid output length: %d"u8, outputLenBytes));
            }

            var rangeBits = (maxOutLenBytes * 8 - minOutLenBytes * 8) + 1;
            if (rangeBits == 0) {
                return (default!, fmt.Errorf("invalid maxOutLenBytes and minOutLenBytes: %d, %d"u8, maxOutLenBytes, minOutLenBytes));
            }

            // cSHAKE Monte Carlo test inner loop:
            //   https://pages.nist.gov/ACVP/draft-celi-acvp-xof.html#section-6.2.1
            for (nint i = 0; i < 1000; i++) {
                // InnerMsg = Left(Output[i-1] || ZeroBits(128), 128);
                nint boundary = min(len(message), 16);
                var innerMsg = new slice<byte>(16);
                copy(innerMsg, message.slice(0, boundary));

                // Output[i] = CSHAKE(InnerMsg, OutputLen, FunctionName, Customization);
                var h = hFn(default!, customization); // Note: function name fixed to "" for MCT.
                h.Write(innerMsg);
                var digest = new slice<byte>((nint)(outputLenBytes));
                h.Read(digest);
                message = digest;

                // Rightmost_Output_bits = Right(Output[i], 16);
                var rightmostOutput = digest.slice((nint)(outputLenBytes - 2));
                // IMPORTANT: the specification says:
                //   NOTE: For the "Rightmost_Output_bits % Range" operation, the Rightmost_Output_bits bit string
                //   should be interpretted as a little endian-encoded number.
                // This is **a lie**! It has to be interpreted as a big-endian number.
                var rightmostOutputBE = binary.BigEndian.Uint16(rightmostOutput);

                // OutputLen = MinOutLen + (floor((Rightmost_Output_bits % Range) / OutLenIncrement) * OutLenIncrement);
                var incrementBits = incrementBytes * 8;
                var outputLenBits = (minOutLenBytes * 8) + (((uint32)rightmostOutputBE % rangeBits) / incrementBits) * incrementBits;
                outputLenBytes = outputLenBits / 8;

                // Customization = BitsToString(InnerMsg || Rightmost_Output_bits);
                var msgWithBits = appendꓸꓸꓸ(innerMsg, rightmostOutput);
                customization = new slice<byte>(len(msgWithBits));
                foreach (var (iΔ1, b) in msgWithBits) {
                    customization[iΔ1] = (byte)((b % 26) + 65);
                }
            }

            var encodedOutputLenBytes = new slice<byte>(4);
            binary.LittleEndian.PutUint32(encodedOutputLenBytes, outputLenBytes);

            return (new slice<byte>[]{message, encodedOutputLenBytes, customization}.slice(), default!);
        }
    );
}

internal static command cmdHmacAft(Func<hash.Hash> h) {
    return new command(
        requiredArgs: 2, // Message and key
        handler: (slice<slice<byte>> args) => {
            var msg = args[0];
            var key = args[1];
            var mac = hmac.New(h, key);
            mac.Write(msg);
            return (new slice<byte>[]{mac.Sum(default!)}.slice(), default!);
        }
    );
}

internal static command cmdHkdfAft(Func<hash.Hash> h) {
    return new command(
        requiredArgs: 4, // Key, salt, info, length bytes
        handler: (slice<slice<byte>> args) => {
            var key = args[0];
            var salt = args[1];
            var info = args[2];
            nint keyLen = (nint)binary.LittleEndian.Uint32(args[3]);

            return (new slice<byte>[]{hkdf.Key(h, key, salt, (@string)info, keyLen)}.slice(), default!);
        }
    );
}

internal static command cmdHkdfExtractAft(Func<hash.Hash> h) {
    return new command(
        requiredArgs: 2, // secret, salt
        handler: (slice<slice<byte>> args) => {
            var secret = args[0];
            var salt = args[1];

            return (new slice<byte>[]{hkdf.Extract(h, secret, salt)}.slice(), default!);
        }
    );
}

internal static command cmdHkdfExpandLabelAft(Func<hash.Hash> h) {
    return new command(
        requiredArgs: 4, // output length, secret, label, transcript hash
        handler: (slice<slice<byte>> args) => {
            nint keyLen = (nint)binary.LittleEndian.Uint32(args[0]);
            var secret = args[1];
            var label = args[2];
            var transcriptHash = args[3];

            return (new slice<byte>[]{tls13.ExpandLabel(h, secret, (@string)label, transcriptHash, keyLen)}.slice(), default!);
        }
    );
}

internal static command cmdPbkdf() {
    return new command(
        // Hash name, key length, salt, password, iteration count
        requiredArgs: 5,
        handler: (slice<slice<byte>> args) => {
            var (h, err) = lookupHash((@string)args[0]);
            if (err != default!) {
                return (default!, fmt.Errorf("PBKDF2 failed: %w"u8, err));
            }

            var keyLen = binary.LittleEndian.Uint32(args[1]) / 8;
            var salt = args[2];
            var password = args[3];
            var iterationCount = binary.LittleEndian.Uint32(args[4]);

            (var derivedKey, err) = pbkdf2.Key(h, (@string)password, salt, (nint)iterationCount, (nint)keyLen);
            if (err != default!) {
                return (default!, fmt.Errorf("PBKDF2 failed: %w"u8, err));
            }

            return (new slice<byte>[]{derivedKey}.slice(), default!);
        }
    );
}

internal static command cmdEddsaKeyGenAft() {
    return new command(
        requiredArgs: 1, // Curve name
        handler: (slice<slice<byte>> args) => {
            if (((sstring)args[0]) != "ED-25519"u8) {
                return (default!, fmt.Errorf("unsupported EDDSA curve: %q"u8, args[0]));
            }

            var (sk, err) = ed25519.GenerateKey();
            if (err != default!) {
                return (default!, fmt.Errorf("generating EDDSA keypair: %w"u8, err));
            }

            // EDDSA/keyGen/AFT responses are d & q, described[0] as:
            //   d	The encoded private key point
            //   q	The encoded public key point
            //
            // Contrary to the description of a "point", d is the private key
            // seed bytes per FIPS.186-5[1] A.2.3.
            //
            // [0]: https://pages.nist.gov/ACVP/draft-celi-acvp-eddsa.html#section-9.1
            // [1]: https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-5.pdf
            return (new slice<byte>[]{sk.Seed(), sk.PublicKey()}.slice(), default!);
        }
    );
}

internal static command cmdEddsaKeyVerAft() {
    return new command(
        requiredArgs: 2, // Curve name, Q
        handler: (slice<slice<byte>> args) => {
            if (((sstring)args[0]) != "ED-25519"u8) {
                return (default!, fmt.Errorf("unsupported EDDSA curve: %q"u8, args[0]));
            }

            // Verify the point is on the curve. The higher-level ed25519 API does
            // this at signature verification time so we have to use the lower-level
            // edwards25519 package to do it here in absence of a signature to verify.
            {
                var (_, err) = @new<edwards25519.Point>().SetBytes(args[1]); if (err != default!) {
                    return (new slice<byte>[]{new byte[]{0}.slice()}.slice(), default!);
                }
            }

            return (new slice<byte>[]{new byte[]{1}.slice()}.slice(), default!);
        }
    );
}

internal static command cmdEddsaSigGenAftBft() {
    return new command(
        requiredArgs: 5, // Curve name, private key seed, message, prehash, context
        handler: (slice<slice<byte>> args) => {
            if (((sstring)args[0]) != "ED-25519"u8) {
                return (default!, fmt.Errorf("unsupported EDDSA curve: %q"u8, args[0]));
            }

            var (sk, err) = ed25519.NewPrivateKeyFromSeed(args[1]);
            if (err != default!) {
                return (default!, fmt.Errorf("error creating private key: %w"u8, err));
            }
            var msg = args[2];
            var prehash = args[3];
            @string context = (@string)args[4];

            slice<byte> sig = default!;
            if (prehash[0] == 1){
                var h = sha512.New();
                h.Write(msg);
                msg = h.Sum(default!);

                // With ed25519 the context is only specified for sigGen tests when using prehashing.
                // See https://pages.nist.gov/ACVP/draft-celi-acvp-eddsa.html#section-8.6
                (sig, err) = ed25519.SignPH(sk, msg, context);
                if (err != default!) {
                    return (default!, fmt.Errorf("error signing message: %w"u8, err));
                }
            } else {
                sig = ed25519.Sign(sk, msg);
            }

            return (new slice<byte>[]{sig}.slice(), default!);
        }
    );
}

internal static command cmdEddsaSigVerAft() {
    return new command(
        requiredArgs: 5, // Curve name, message, public key, signature, prehash
        handler: (slice<slice<byte>> args) => {
            if (((sstring)args[0]) != "ED-25519"u8) {
                return (default!, fmt.Errorf("unsupported EDDSA curve: %q"u8, args[0]));
            }

            var msg = args[1];
            var (pk, err) = ed25519.NewPublicKey(args[2]);
            if (err != default!) {
                return (default!, fmt.Errorf("invalid public key: %w"u8, err));
            }
            var sig = args[3];
            var prehash = args[4];

            if (prehash[0] == 1){
                var h = sha512.New();
                h.Write(msg);
                msg = h.Sum(default!);
                // Context is only specified for sigGen, not sigVer.
                // See https://pages.nist.gov/ACVP/draft-celi-acvp-eddsa.html#section-8.6
                err = ed25519.VerifyPH(pk, msg, sig, ""u8);
            } else {
                err = ed25519.Verify(pk, msg, sig);
            }

            if (err != default!) {
                return (new slice<byte>[]{new byte[]{0}.slice()}.slice(), default!);
            }

            return (new slice<byte>[]{new byte[]{1}.slice()}.slice(), default!);
        }
    );
}

internal static command cmdEcdsaKeyGenAft() {
    return new command(
        requiredArgs: 1, // Curve name
        handler: (slice<slice<byte>> args) => {
            var (curve, err) = lookupCurve((@string)args[0]);
            if (err != default!) {
                return (default!, err);
            }

            ж<ecdsa.PrivateKey> sk = default!;
            var exprᴛ1 = curve.Params();
            if (exprᴛ1 == elliptic.P224().Params()) {
                (sk, err) = ecdsa.GenerateKey<ecdsa.P224PointжPoint>(ecdsa.P224(), rand.Reader);
            }
            else if (exprᴛ1 == elliptic.P256().Params()) {
                (sk, err) = ecdsa.GenerateKey<ecdsa.P256PointжPoint>(ecdsa.P256(), rand.Reader);
            }
            else if (exprᴛ1 == elliptic.P384().Params()) {
                (sk, err) = ecdsa.GenerateKey<ecdsa.P384PointжPoint>(ecdsa.P384(), rand.Reader);
            }
            else if (exprᴛ1 == elliptic.P521().Params()) {
                (sk, err) = ecdsa.GenerateKey<ecdsa.P521PointжPoint>(ecdsa.P521(), rand.Reader);
            }
            else { /* default: */
                return (default!, fmt.Errorf("unsupported curve: %v"u8, curve));
            }

            if (err != default!) {
                return (default!, err);
            }

            var pubBytes = sk.PublicKey().Bytes();
            nint byteLen = ((~curve.Params()).BitSize + 7) / 8;

            return (new slice<byte>[]{
                sk.Bytes(),
                pubBytes.slice(1, 1 + byteLen),
                pubBytes.slice(1 + byteLen)
            }.slice(), default!);
        }
    );
}

internal static command cmdEcdsaKeyVerAft() {
    return new command(
        requiredArgs: 3, // Curve name, X, Y
        handler: (slice<slice<byte>> args) => {
            var (curve, err) = lookupCurve((@string)args[0]);
            if (err != default!) {
                return (default!, err);
            }

            var x = @new<bigꓸInt>().SetBytes(args[1]);
            var y = @new<bigꓸInt>().SetBytes(args[2]);

            if (curve.IsOnCurve(x, y)) {
                return (new slice<byte>[]{new byte[]{1}.slice()}.slice(), default!);
            }

            return (new slice<byte>[]{new byte[]{0}.slice()}.slice(), default!);
        }
    );
}

// pointFromAffine is used to convert the PublicKey to a nistec SetBytes input.
// Duplicated from crypto/ecdsa.go's pointFromAffine.
internal static (slice<byte>, error) pointFromAffine(elliptic.Curve curve, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    nint bitSize = curve.Params().Value.BitSize;
    // Reject values that would not get correctly encoded.
    if (x.Sign() < 0 || y.Sign() < 0) {
        return (default!, errors.New("negative coordinate"u8));
    }
    if (x.BitLen() > bitSize || y.BitLen() > bitSize) {
        return (default!, errors.New("overflowing coordinate"u8));
    }
    // Encode the coordinates and let SetBytes reject invalid points.
    nint byteLen = (bitSize + 7) / 8;
    var buf = new slice<byte>(1 + 2 * byteLen);
    buf[0] = 4; // uncompressed point
    x.FillBytes(buf.slice(1, 1 + byteLen));
    y.FillBytes(buf.slice(1 + byteLen, 1 + 2 * byteLen));
    return (buf, default!);
}

internal static (ж<ecdsa.Signature>, error) signEcdsa<P, H>(ж<ecdsa.Curve<P>> Ꮡc, Func<H> h, ecdsaSigType sigType, slice<byte> q, slice<byte> sk, slice<byte> digest)
    where P : ecdsa.Point<P>
    where H : hash.Hash
{
    var (priv, err) = ecdsa.NewPrivateKey(Ꮡc, sk, q);
    if (err != default!) {
        return (default!, fmt.Errorf("invalid private key: %w"u8, err));
    }

    ж<ecdsa.Signature> sig = default!;
    var exprᴛ1 = sigType;
    if (exprᴛ1 == ecdsaSigTypeNormal) {
        (sig, err) = ecdsa.Sign(Ꮡc, h, priv, rand.Reader, digest);
    }
    else if (exprᴛ1 == ecdsaSigTypeDeterministic) {
        (sig, err) = ecdsa.SignDeterministic(Ꮡc, h, priv, digest);
    }
    else { /* default: */
        return (default!, fmt.Errorf("unsupported signature type: %v"u8, sigType));
    }
    if (err != default!) {
        return (default!, fmt.Errorf("signing failed: %w"u8, err));
    }

    return (sig, default!);
}

internal static command cmdEcdsaSigGenAft(ecdsaSigType sigType) {
    return new command(
        requiredArgs: 4, // Curve name, private key, hash name, message
        handler: (slice<slice<byte>> args) => {
            var (curve, err) = lookupCurve((@string)args[0]);
            if (err != default!) {
                return (default!, err);
            }

            var sk = args[1];

            (var newH, err) = lookupHash((@string)args[2]);
            if (err != default!) {
                return (default!, err);
            }

            var msg = args[3];
            var hashFunc = newH();
            hashFunc.Write(msg);
            var digest = hashFunc.Sum(default!);

            var d = @new<bigꓸInt>().SetBytes(sk);
            var (x, y) = curve.ScalarBaseMult(d.Bytes());
            (var q, err) = pointFromAffine(curve, x, y);
            if (err != default!) {
                return (default!, err);
            }

            ж<ecdsa.Signature> sig = default!;
            var exprᴛ1 = curve.Params();
            if (exprᴛ1 == elliptic.P224().Params()) {
                (sig, err) = signEcdsa<ecdsa.P224PointжPoint, hash.Hash>(ecdsa.P224(), newH, sigType, q, sk, digest);
            }
            else if (exprᴛ1 == elliptic.P256().Params()) {
                (sig, err) = signEcdsa<ecdsa.P256PointжPoint, hash.Hash>(ecdsa.P256(), newH, sigType, q, sk, digest);
            }
            else if (exprᴛ1 == elliptic.P384().Params()) {
                (sig, err) = signEcdsa<ecdsa.P384PointжPoint, hash.Hash>(ecdsa.P384(), newH, sigType, q, sk, digest);
            }
            else if (exprᴛ1 == elliptic.P521().Params()) {
                (sig, err) = signEcdsa<ecdsa.P521PointжPoint, hash.Hash>(ecdsa.P521(), newH, sigType, q, sk, digest);
            }
            else { /* default: */
                return (default!, fmt.Errorf("unsupported curve: %v"u8, curve));
            }
            if (err != default!) {
                return (default!, err);
            }

            return (new slice<byte>[]{(~sig).R, (~sig).S}.slice(), default!);
        }
    );
}

internal static command cmdEcdsaSigVerAft() {
    return new command(
        requiredArgs: 7, // Curve name, hash name, message, X, Y, R, S
        handler: (slice<slice<byte>> args) => {
            var (curve, err) = lookupCurve((@string)args[0]);
            if (err != default!) {
                return (default!, err);
            }

            (var newH, err) = lookupHash((@string)args[1]);
            if (err != default!) {
                return (default!, err);
            }

            var msg = args[2];
            var hashFunc = newH();
            hashFunc.Write(msg);
            var digest = hashFunc.Sum(default!);

            var (x, y) = (args[3], args[4]);
            (var q, err) = pointFromAffine(curve, @new<bigꓸInt>().SetBytes(x), @new<bigꓸInt>().SetBytes(y));
            if (err != default!) {
                return (default!, fmt.Errorf("invalid x/y coordinates: %v"u8, err));
            }

            var signature = Ꮡ(new ecdsa.Signature(R: args[5], S: args[6]));

            var exprᴛ1 = curve.Params();
            if (exprᴛ1 == elliptic.P224().Params()) {
                err = verifyEcdsa<ecdsa.P224PointжPoint>(ecdsa.P224(), q, digest, signature);
            }
            else if (exprᴛ1 == elliptic.P256().Params()) {
                err = verifyEcdsa<ecdsa.P256PointжPoint>(ecdsa.P256(), q, digest, signature);
            }
            else if (exprᴛ1 == elliptic.P384().Params()) {
                err = verifyEcdsa<ecdsa.P384PointжPoint>(ecdsa.P384(), q, digest, signature);
            }
            else if (exprᴛ1 == elliptic.P521().Params()) {
                err = verifyEcdsa<ecdsa.P521PointжPoint>(ecdsa.P521(), q, digest, signature);
            }
            else { /* default: */
                return (default!, fmt.Errorf("unsupported curve: %v"u8, curve));
            }

            if (err == default!) {
                return (new slice<byte>[]{new byte[]{1}.slice()}.slice(), default!);
            }

            return (new slice<byte>[]{new byte[]{0}.slice()}.slice(), default!);
        }
    );
}

internal static error verifyEcdsa<P>(ж<ecdsa.Curve<P>> Ꮡc, slice<byte> q, slice<byte> digest, ж<ecdsa.Signature> Ꮡsig)
    where P : ecdsa.Point<P>
{
    var (pub, err) = ecdsa.NewPublicKey(Ꮡc, q);
    if (err != default!) {
        return fmt.Errorf("invalid public key: %w"u8, err);
    }

    return ecdsa.Verify(Ꮡc, pub, digest, Ꮡsig);
}

internal static (Func<hash.Hash>, error) lookupHash(@string name) {
    Func<hash.Hash> h = default!;

    var exprᴛ1 = name;
    if (exprᴛ1 == "SHA2-224"u8) {
        h = () => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New224());
    }
    else if (exprᴛ1 == "SHA2-256"u8) {
        h = () => new fipstest_internal_test_package.sha256_DigestжHash(sha256.New());
    }
    else if (exprᴛ1 == "SHA2-384"u8) {
        h = () => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New384());
    }
    else if (exprᴛ1 == "SHA2-512"u8) {
        h = () => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New());
    }
    else if (exprᴛ1 == "SHA2-512/224"u8) {
        h = () => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_224());
    }
    else if (exprᴛ1 == "SHA2-512/256"u8) {
        h = () => new fipstest_internal_test_package.sha512_DigestжHash(sha512.New512_256());
    }
    else if (exprᴛ1 == "SHA3-224"u8) {
        h = () => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New224());
    }
    else if (exprᴛ1 == "SHA3-256"u8) {
        h = () => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New256());
    }
    else if (exprᴛ1 == "SHA3-384"u8) {
        h = () => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New384());
    }
    else if (exprᴛ1 == "SHA3-512"u8) {
        h = () => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512());
    }
    else { /* default: */
        return (default!, fmt.Errorf("unknown hash name: %q"u8, name));
    }

    return (h, default!);
}

internal static command cmdMlKem768KeyGenAft() {
    return new command(
        requiredArgs: 1, // Seed
        handler: (slice<slice<byte>> args) => {
            var seed = args[0];

            var (dk, err) = mlkem.NewDecapsulationKey768(seed);
            if (err != default!) {
                return (default!, fmt.Errorf("generating ML-KEM 768 decapsulation key: %w"u8, err));
            }

            // Important: we must return the full encoding of dk, not the seed.
            return (new slice<byte>[]{dk.EncapsulationKey().Bytes(), mlkem.TestingOnlyExpandedBytes768(dk)}.slice(), default!);
        }
    );
}

internal static command cmdMlKem768EncapAft() {
    return new command(
        requiredArgs: 2, // Public key, entropy
        handler: (slice<slice<byte>> args) => {
            var pk = args[0];
            var entropy = args[1];

            var (ek, err) = mlkem.NewEncapsulationKey768(pk);
            if (err != default!) {
                return (default!, fmt.Errorf("generating ML-KEM 768 encapsulation key: %w"u8, err));
            }

            if (len(entropy) != 32) {
                return (default!, fmt.Errorf("wrong entropy length: got %d, want 32"u8, len(entropy)));
            }

            var (sharedKey, ct) = ek.EncapsulateInternal(Ꮡ(array<byte>.Alias(entropy[..32], 32)));

            return (new slice<byte>[]{ct, sharedKey}.slice(), default!);
        }
    );
}

internal static command cmdMlKem768DecapAft() {
    return new command(
        requiredArgs: 2, // Private key, ciphertext
        handler: (slice<slice<byte>> args) => {
            var pk = args[0];
            var ct = args[1];

            var (dk, err) = mlkem.TestingOnlyNewDecapsulationKey768(pk);
            if (err != default!) {
                return (default!, fmt.Errorf("generating ML-KEM 768 decapsulation key: %w"u8, err));
            }

            (var sharedKey, err) = dk.Decapsulate(ct);
            if (err != default!) {
                return (default!, fmt.Errorf("decapsulating ML-KEM 768 ciphertext: %w"u8, err));
            }

            return (new slice<byte>[]{sharedKey}.slice(), default!);
        }
    );
}

internal static command cmdMlKem1024KeyGenAft() {
    return new command(
        requiredArgs: 1, // Seed
        handler: (slice<slice<byte>> args) => {
            var seed = args[0];

            var (dk, err) = mlkem.NewDecapsulationKey1024(seed);
            if (err != default!) {
                return (default!, fmt.Errorf("generating ML-KEM 1024 decapsulation key: %w"u8, err));
            }

            // Important: we must return the full encoding of dk, not the seed.
            return (new slice<byte>[]{dk.EncapsulationKey().Bytes(), mlkem.TestingOnlyExpandedBytes1024(dk)}.slice(), default!);
        }
    );
}

internal static command cmdMlKem1024EncapAft() {
    return new command(
        requiredArgs: 2, // Public key, entropy
        handler: (slice<slice<byte>> args) => {
            var pk = args[0];
            var entropy = args[1];

            var (ek, err) = mlkem.NewEncapsulationKey1024(pk);
            if (err != default!) {
                return (default!, fmt.Errorf("generating ML-KEM 1024 encapsulation key: %w"u8, err));
            }

            if (len(entropy) != 32) {
                return (default!, fmt.Errorf("wrong entropy length: got %d, want 32"u8, len(entropy)));
            }

            var (sharedKey, ct) = ek.EncapsulateInternal(Ꮡ(array<byte>.Alias(entropy[..32], 32)));

            return (new slice<byte>[]{ct, sharedKey}.slice(), default!);
        }
    );
}

internal static command cmdMlKem1024DecapAft() {
    return new command(
        requiredArgs: 2, // Private key, ciphertext
        handler: (slice<slice<byte>> args) => {
            var pk = args[0];
            var ct = args[1];

            var (dk, err) = mlkem.TestingOnlyNewDecapsulationKey1024(pk);
            if (err != default!) {
                return (default!, fmt.Errorf("generating ML-KEM 1024 decapsulation key: %w"u8, err));
            }

            (var sharedKey, err) = dk.Decapsulate(ct);
            if (err != default!) {
                return (default!, fmt.Errorf("decapsulating ML-KEM 1024 ciphertext: %w"u8, err));
            }

            return (new slice<byte>[]{sharedKey}.slice(), default!);
        }
    );
}

internal static (elliptic.Curve, error) lookupCurve(@string name) {
    elliptic.Curve c = default!;

    var exprᴛ1 = name;
    if (exprᴛ1 == "P-224"u8) {
        c = elliptic.P224();
    }
    else if (exprᴛ1 == "P-256"u8) {
        c = elliptic.P256();
    }
    else if (exprᴛ1 == "P-384"u8) {
        c = elliptic.P384();
    }
    else if (exprᴛ1 == "P-521"u8) {
        c = elliptic.P521();
    }
    else { /* default: */
        return (default!, fmt.Errorf("unknown curve name: %q"u8, name));
    }

    return (c, default!);
}

internal static command cmdAesCbc(aesDirection direction) {
    return new command(
        requiredArgs: 4, // Key, ciphertext or plaintext, IV, num iterations
        handler: (slice<slice<byte>> args) => {
            if (direction != aesEncrypt && direction != aesDecrypt) {
                throw panic("invalid AES direction");
            }

            var key = args[0];
            var input = args[1];
            var iv = args[2];
            var numIterations = binary.LittleEndian.Uint32(args[3]);

            var (blockCipher, err) = aes.New(key);
            if (err != default!) {
                return (default!, fmt.Errorf("creating AES block cipher with key len %d: %w"u8, len(key), err));
            }

            if (len(input) % blockCipher.BlockSize() != 0 || len(input) == 0) {
                return (default!, fmt.Errorf("invalid ciphertext/plaintext size %d: not a multiple of block size %d"u8,
                    len(input), blockCipher.BlockSize()));
            }

            if (blockCipher.BlockSize() != len(iv)) {
                return (default!, fmt.Errorf("invalid IV size: expected %d, got %d"u8, blockCipher.BlockSize(), len(iv)));
            }

            var result = new slice<byte>(len(input));
            var prevResult = new slice<byte>(len(input));
            var prevInput = new slice<byte>(len(input));

            for (var i = (uint32)0; i < numIterations; i++) {
                copy(prevResult, result);

                if (i > 0) {
                    if (direction == aesEncrypt){
                        copy(iv, result);
                    } else {
                        copy(iv, prevInput);
                    }
                }

                if (direction == aesEncrypt){
                    var cbcEnc = aes.NewCBCEncrypter(blockCipher, new array<byte>(iv, 16));
                    cbcEnc.CryptBlocks(result, input);
                } else {
                    var cbcDec = aes.NewCBCDecrypter(blockCipher, new array<byte>(iv, 16));
                    cbcDec.CryptBlocks(result, input);
                }

                if (direction == aesDecrypt) {
                    copy(prevInput, input);
                }

                if (i == 0){
                    copy(input, iv);
                } else {
                    copy(input, prevResult);
                }
            }

            return (new slice<byte>[]{result, prevResult}.slice(), default!);
        }
    );
}

internal static command cmdAesCtr(aesDirection direction) {
    return new command(
        requiredArgs: 4, // Key, ciphertext or plaintext, initial counter, num iterations (constant 1)
        handler: (slice<slice<byte>> args) => {
            if (direction != aesEncrypt && direction != aesDecrypt) {
                throw panic("invalid AES direction");
            }

            var key = args[0];
            var input = args[1];
            var iv = args[2];
            var numIterations = binary.LittleEndian.Uint32(args[3]);
            if (numIterations != 1) {
                return (default!, fmt.Errorf("invalid num iterations: expected 1, got %d"u8, numIterations));
            }

            if (len(iv) != aes.ΔBlockSize) {
                return (default!, fmt.Errorf("invalid IV size: expected %d, got %d"u8, (nint)(aes.ΔBlockSize), len(iv)));
            }

            var (blockCipher, err) = aes.New(key);
            if (err != default!) {
                return (default!, fmt.Errorf("creating AES block cipher with key len %d: %w"u8, len(key), err));
            }

            var result = new slice<byte>(len(input));
            var stream = aes.NewCTR(blockCipher, iv);
            stream.XORKeyStream(result, input);

            return (new slice<byte>[]{result}.slice(), default!);
        }
    );
}

internal static command cmdAesGcmSeal(bool randNonce) {
    return new command(
        requiredArgs: 5, // tag len, key, plaintext, nonce (empty for randNonce), additional data
        handler: (slice<slice<byte>> args) => {
            var tagLen = binary.LittleEndian.Uint32(args[0]);
            var key = args[1];
            var plaintext = args[2];
            var nonce = args[3];
            var additionalData = args[4];

            var (blockCipher, err) = aes.New(key);
            if (err != default!) {
                return (default!, fmt.Errorf("creating AES block cipher with key len %d: %w"u8, len(key), err));
            }

            (var aesGCM, err) = gcm.New(blockCipher, 12, (nint)tagLen);
            if (err != default!) {
                return (default!, fmt.Errorf("creating AES-GCM with tag len %d: %w"u8, tagLen, err));
            }

            slice<byte> ct = default!;
            if (!randNonce){
                ct = aesGCM.Seal(default!, nonce, plaintext, additionalData);
            } else {
                array<byte> internalNonce = new(12);
                ct = new slice<byte>(len(plaintext) + 16);
                gcm.SealWithRandomNonce(aesGCM, internalNonce[..], ct, plaintext, additionalData);
                // acvptool expects the internally generated nonce to be appended to the end of the ciphertext.
                ct = appendꓸꓸꓸ(ct, internalNonce[..]);
            }

            return (new slice<byte>[]{ct}.slice(), default!);
        }
    );
}

internal static command cmdAesGcmOpen(bool randNonce) {
    return new command(
        requiredArgs: 5, // tag len, key, ciphertext, nonce (empty for randNonce), additional data
        handler: (slice<slice<byte>> args) => {
            var tagLen = binary.LittleEndian.Uint32(args[0]);
            var key = args[1];
            var ciphertext = args[2];
            var nonce = args[3];
            var additionalData = args[4];

            var (blockCipher, err) = aes.New(key);
            if (err != default!) {
                return (default!, fmt.Errorf("creating AES block cipher with key len %d: %w"u8, len(key), err));
            }

            (var aesGCM, err) = gcm.New(blockCipher, 12, (nint)tagLen);
            if (err != default!) {
                return (default!, fmt.Errorf("creating AES-GCM with tag len %d: %w"u8, tagLen, err));
            }

            if (randNonce) {
                // for randNonce tests acvptool appends the nonce to the end of the ciphertext.
                nonce = ciphertext.slice(len(ciphertext) - 12);
                ciphertext = ciphertext.slice(0, len(ciphertext) - 12);
            }

            (var pt, err) = aesGCM.Open(default!, nonce, ciphertext, additionalData);
            if (err != default!) {
                return (new slice<byte>[]{new byte[]{0}.slice(), default!}.slice(), default!);
            }

            return (new slice<byte>[]{new byte[]{1}.slice(), pt}.slice(), default!);
        }
    );
}

internal static command cmdCmacAesAft() {
    return new command(
        requiredArgs: 3, // Number of output bytes, key, message
        handler: (slice<slice<byte>> args) => {
            // safe to truncate to int based on our capabilities describing a max MAC output len of 128 bits.
            nint outputLen = (nint)binary.LittleEndian.Uint32(args[0]);
            var key = args[1];
            var message = args[2];

            var (blockCipher, err) = aes.New(key);
            if (err != default!) {
                return (default!, fmt.Errorf("creating AES block cipher with key len %d: %w"u8, len(key), err));
            }

            var cmac = gcm.NewCMAC(blockCipher);
            var tag = cmac.MAC(message);

            if (outputLen > len(tag)) {
                return (default!, fmt.Errorf("invalid output length: expected %d, got %d"u8, outputLen, len(tag)));
            }

            return (new slice<byte>[]{tag.slice(0, outputLen)}.slice(), default!);
        }
    );
}

internal static command cmdCmacAesVerifyAft() {
    return new command(
        requiredArgs: 3, // Key, message, claimed MAC
        handler: (slice<slice<byte>> args) => {
            var key = args[0];
            var message = args[1];
            var claimedMAC = args[2];

            var (blockCipher, err) = aes.New(key);
            if (err != default!) {
                return (default!, fmt.Errorf("creating AES block cipher with key len %d: %w"u8, len(key), err));
            }

            var cmac = gcm.NewCMAC(blockCipher);
            var tag = cmac.MAC(message);

            if (subtle.ConstantTimeCompare(tag.slice(0, len(claimedMAC)), claimedMAC) != 1) {
                return (new slice<byte>[]{new byte[]{0}.slice()}.slice(), default!);
            }

            return (new slice<byte>[]{new byte[]{1}.slice()}.slice(), default!);
        }
    );
}

internal static command cmdTlsKdf12Aft(Func<hash.Hash> h) {
    return new command(
        requiredArgs: 5, // Number output bytes, secret, label, seed1, seed2
        handler: (slice<slice<byte>> args) => {
            var outputLen = binary.LittleEndian.Uint32(args[0]);
            var secret = args[1];
            @string label = (@string)args[2];
            var seed1 = args[3];
            var seed2 = args[4];

            return (new slice<byte>[]{tls12.PRF(h, secret, label, appendꓸꓸꓸ(seed1, seed2), (nint)outputLen)}.slice(), default!);
        }
    );
}

internal static command cmdSshKdfAft(Func<hash.Hash> hFunc, ssh.Direction direction) {
        var directionʗ1 = direction;
    return new command(
        requiredArgs: 4, // K, H, SessionID, cipher
        handler: (slice<slice<byte>> args) => {
            var k = args[0];
            var h = args[1];
            var sessionID = args[2];
            @string cipher = (@string)args[3];

            nint keyLen = default!;
            var exprᴛ1 = cipher;
            if (exprᴛ1 == "AES-128"u8) {
                keyLen = 16;
            }
            else if (exprᴛ1 == "AES-192"u8) {
                keyLen = 24;
            }
            else if (exprᴛ1 == "AES-256"u8) {
                keyLen = 32;
            }
            else { /* default: */
                return (default!, fmt.Errorf("unsupported cipher: %q"u8, cipher));
            }

            var (ivKey, encKey, intKey) = ssh.Keys(hFunc, directionʗ1, k, h, sessionID, 16, keyLen, hFunc().Size());
            return (new slice<byte>[]{ivKey, encKey, intKey}.slice(), default!);
        }
    );
}

internal static command cmdEcdhAftVal<P>(ж<ecdh.Curve<P>> Ꮡcurve)
    where P : ecdh.Point<P>
{
    return new command(
        requiredArgs: 3, // X, Y, private key (empty for Val type tests)
        handler: (slice<slice<byte>> args) => {
            var peerX = args[0];
            var peerY = args[1];
            var rawSk = args[2];

            var uncompressedPk = appendꓸꓸꓸ(new byte[]{4}.slice(), appendꓸꓸꓸ(peerX, peerY)); // 4 for uncompressed point format
            var (pk, err) = ecdh.NewPublicKey(Ꮡcurve, uncompressedPk);
            if (err != default!) {
                return (default!, fmt.Errorf("invalid peer public key x,y: %v"u8, err));
            }

            ж<ecdh.PrivateKey> sk = default!;
            if (len(rawSk) > 0){
                (sk, err) = ecdh.NewPrivateKey(Ꮡcurve, rawSk);
            } else {
                (sk, err) = ecdh.GenerateKey(Ꮡcurve, rand.Reader);
            }
            if (err != default!) {
                return (default!, fmt.Errorf("private key error: %v"u8, err));
            }

            var pubBytes = sk.PublicKey().Bytes();
            nint coordLen = (len(pubBytes) - 1) / 2;
            var x = pubBytes.slice(1, 1 + coordLen);
            var y = pubBytes.slice(1 + coordLen);

            (var secret, err) = ecdh.ECDH(Ꮡcurve, sk, pk);
            if (err != default!) {
                return (default!, fmt.Errorf("key agreement failed: %v"u8, err));
            }

            return (new slice<byte>[]{x, y, secret}.slice(), default!);
        }
    );
}

internal static command cmdHmacDrbgAft(Func<hash.Hash> h) {
    return new command(
        requiredArgs: 6, // Output length, entropy, personalization, ad1, ad2, nonce
        handler: (slice<slice<byte>> args) => {
            var outLen = binary.LittleEndian.Uint32(args[0]);
            var entropy = args[1];
            var personalization = args[2];
            var ad1 = args[3];
            var ad2 = args[4];
            var nonce = args[5];

            // Our capabilities describe no additional data support.
            if (len(ad1) != 0 || len(ad2) != 0) {
                return (default!, errors.New("additional data not supported"u8));
            }

            // Our capabilities describe no prediction resistance (requires reseed) and no reseed.
            // So the test procedure is:
            //   * Instantiate DRBG
            //   * Generate but don't output
            //   * Generate output
            //   * Uninstantiate
            // See Table 7 in draft-vassilev-acvp-drbg
            var @out = new slice<byte>((nint)(outLen));
            var drbg = ecdsa.TestingOnlyNewDRBG(h, entropy, nonce, personalization);
            drbg.Generate(@out);
            drbg.Generate(@out);

            return (new slice<byte>[]{@out}.slice(), default!);
        }
    );
}

internal static command cmdCtrDrbgAft() {
    return new command(
        requiredArgs: 6, // Output length, entropy, personalization, ad1, ad2, nonce
        handler: (slice<slice<byte>> args) => new acvpCtrDrbg(
                outLen: binary.LittleEndian.Uint32(args[0]),
                entropy: args[1],
                personalization: args[2],
                ad1: args[3],
                ad2: args[4],
                nonce: args[5]
            ).process()
    );
}

internal static command cmdCtrDrbgReseedAft() {
    return new command(
        requiredArgs: 8, // Output length, entropy, personalization, reseedAD, reseedEntropy, ad1, ad2, nonce
        handler: (slice<slice<byte>> args) => new acvpCtrDrbg(
                outLen: binary.LittleEndian.Uint32(args[0]),
                entropy: args[1],
                personalization: args[2],
                reseedAd: args[3],
                reseedEntropy: args[4],
                ad1: args[5],
                ad2: args[6],
                nonce: args[7]
            ).process()
    );
}

internal partial struct acvpCtrDrbg {
    internal uint32 outLen;
    internal slice<byte> entropy;
    internal slice<byte> personalization;
    internal slice<byte> ad1;
    internal slice<byte> ad2;
    internal slice<byte> nonce;
    internal slice<byte> reseedAd; // May be empty for no reseed
    internal slice<byte> reseedEntropy; // May be empty for no reseed
}

internal static (slice<slice<byte>>, error) process(this acvpCtrDrbg args) {
    // Our capability describes no personalization support.
    if (len(args.personalization) > 0) {
        return (default!, errors.New("personalization string not supported"u8));
    }

    // Our capability describes no derivation function support, so the nonce
    // should be empty.
    if (len(args.nonce) > 0) {
        return (default!, errors.New("unexpected nonce value"u8));
    }

    // Our capability describes entropy input len of 384 bits.
    var (entropy, err) = require48Bytes(args.entropy);
    if (err != default!) {
        return (default!, fmt.Errorf("entropy: %w"u8, err));
    }

    // Our capability describes additional input len of 384 bits.
    (var ad1, err) = require48Bytes(args.ad1);
    if (err != default!) {
        return (default!, fmt.Errorf("AD1: %w"u8, err));
    }
    (var ad2, err) = require48Bytes(args.ad2);
    if (err != default!) {
        return (default!, fmt.Errorf("AD2: %w"u8, err));
    }

    var withReseed = len(args.reseedAd) > 0;
    ж<array<byte>> reseedAd = default!;
    ж<array<byte>> reseedEntropy = default!;
    if (withReseed) {
        // Ditto RE: entropy and additional data lengths for reseeding.
        {
            (reseedAd, err) = require48Bytes(args.reseedAd); if (err != default!) {
                return (default!, fmt.Errorf("reseed AD: %w"u8, err));
            }
        }
        {
            (reseedEntropy, err) = require48Bytes(args.reseedEntropy); if (err != default!) {
                return (default!, fmt.Errorf("reseed entropy: %w"u8, err));
            }
        }
    }

    // Our capabilities describe no prediction resistance and allow both
    // reseed and no reseed, so the test procedure is:
    //   * Instantiate DRBG
    //   * Reseed (if enabled)
    //   * Generate but don't output
    //   * Generate output
    //   * Uninstantiate
    // See Table 7 in draft-vassilev-acvp-drbg
    var @out = new slice<byte>((nint)(args.outLen));
    var ctrDrbg = drbg.NewCounter(entropy);
    if (withReseed) {
        ctrDrbg.Reseed(reseedEntropy, reseedAd);
    }
    ctrDrbg.Generate(@out, ad1);
    ctrDrbg.Generate(@out, ad2);

    return (new slice<byte>[]{@out}.slice(), default!);
}

// Verify input is 48 byte slice, and cast it to a pointer to a fixed-size array
// of 48 bytes, or return an error.
internal static (ж<array<byte>>, error) require48Bytes(slice<byte> input) {
    {
        nint inputLen = len(input); if (inputLen != 48) {
            return (ж<array<byte>>.NilBoxOfDims(48L), fmt.Errorf("invalid length: %d"u8, inputLen));
        }
    }
    return (Ꮡ(array<byte>.Alias(input, 48)), default!);
}

internal static command cmdKdfCounterAft() {
    return new command(
        requiredArgs: 5, // Number output bytes, PRF name, counter location string, key, number of counter bits
        handler: (slice<slice<byte>> args) => {
            var outputBytes = binary.LittleEndian.Uint32(args[0]);
            var prf = args[1];
            var counterLocation = args[2];
            var key = args[3];
            var counterBits = binary.LittleEndian.Uint32(args[4]);

            if (outputBytes != 32) {
                return (default!, fmt.Errorf("KDF received unsupported output length %d bytes"u8, outputBytes));
            }
            if (!bytes.Equal(prf, slice<byte>("CMAC-AES128"u8)) && !bytes.Equal(prf, slice<byte>("CMAC-AES192"u8)) && !bytes.Equal(prf, slice<byte>("CMAC-AES256"u8))) {
                return (default!, fmt.Errorf("KDF received unsupported PRF %q"u8, (@string)prf));
            }
            if (!bytes.Equal(counterLocation, slice<byte>("before fixed data"u8))) {
                return (default!, fmt.Errorf("KDF received unsupported counter location %q"u8, (@string)counterLocation));
            }
            // The spec doesn't describe the "deferred" property for a KDF counterMode test case.
            // BoringSSL's acvptool sends an empty key when deferred=true, but with the capabilities
            // we register all test cases ahve deferred=false and provide a key from the populated
            // keyIn property.
            if (len(key) == 0) {
                return (default!, errors.New("deferred test cases are not supported"u8));
            }
            if (counterBits != 16) {
                return (default!, fmt.Errorf("KDF received unsupported counter length %d"u8, counterBits));
            }

            var (block, err) = aes.New(key);
            if (err != default!) {
                return (default!, fmt.Errorf("failed to create cipher: %v"u8, err));
            }
            var kdf = gcm.NewCounterKDF(block);

            byte label = default!;
            array<byte> context = new(12);
            rand.Reader.Read(context[..]);

            var result = kdf.DeriveKey(label, context);

            var fixedData = new slice<byte>(1 + 1 + 12); // 1 byte label, 1 null byte, 12 bytes context.
            fixedData[0] = label;
            copy(fixedData[2..], context[..]);

            return (new slice<byte>[]{key, fixedData, result[..]}.slice(), default!);
        }
    );
}

internal static command cmdKdfFeedbackAft() {
    return new command(
        requiredArgs: 5, // Number output bytes, PRF name, counter location string, key, number of counter bits, IV
        handler: (slice<slice<byte>> args) => {
            // The max supported output len for the KDF algorithm type is 4096 bits, making an int cast
            // here safe.
            // See https://pages.nist.gov/ACVP/draft-celi-acvp-kbkdf.html#section-7.3.2
            nint outputBytes = (nint)binary.LittleEndian.Uint32(args[0]);
            @string prf = (@string)args[1];
            var counterLocation = args[2];
            var key = args[3];
            var counterBits = binary.LittleEndian.Uint32(args[4]);

            if (!strings.HasPrefix(prf, "HMAC-"u8)) {
                return (default!, fmt.Errorf("feedback KDF received unsupported PRF %q"u8, prf));
            }
            prf = prf[(int)(len("HMAC-"))..];

            var (h, err) = lookupHash(prf);
            if (err != default!) {
                return (default!, fmt.Errorf("feedback KDF received unsupported PRF %q: %w"u8, prf, err));
            }

            if (!bytes.Equal(counterLocation, slice<byte>("after fixed data"u8))) {
                return (default!, fmt.Errorf("feedback KDF received unsupported counter location %q"u8, (@string)counterLocation));
            }

            // The spec doesn't describe the "deferred" property for a KDF counterMode test case.
            // BoringSSL's acvptool sends an empty key when deferred=true, but with the capabilities
            // we register all test cases have deferred=false and provide a key from the populated
            // keyIn property.
            if (len(key) == 0) {
                return (default!, errors.New("deferred test cases are not supported"u8));
            }

            if (counterBits != 8) {
                return (default!, fmt.Errorf("feedback KDF received unsupported counter length %d"u8, counterBits));
            }

            array<byte> context = new(12);
            rand.Reader.Read(context[..]);
            var fixedData = new slice<byte>(1 + 1 + 12); // 1 byte label (we pick null), 1 null byte, 12 bytes context.
            copy(fixedData[2..], context[..]);

            var result = hkdf.Expand(h, key, (@string)(fixedData[..]), outputBytes);

            return (new slice<byte>[]{key, fixedData[..], result[..]}.slice(), default!);
        }
    );
}

internal static command cmdRsaKeyGenAft() {
    return new command(
        requiredArgs: 1, // Modulus bit-size
        handler: (slice<slice<byte>> args) => {
            var bitSize = binary.LittleEndian.Uint32(args[0]);

            var (key, err) = getRSAKey((nint)bitSize);
            if (err != default!) {
                return (default!, fmt.Errorf("generating RSA key: %w"u8, err));
            }

            var (N, e, d, P, Q, _, _, _) = key.Export();

            var eBytes = new slice<byte>(4);
            binary.BigEndian.PutUint32(eBytes, (uint32)e);

            return (new slice<byte>[]{eBytes, P, Q, N, d}.slice(), default!);
        }
    );
}

internal static command cmdRsaSigGenAft(Func<hash.Hash> hashFunc, @string hashName, bool pss) {
    return new command(
        requiredArgs: 2, // Modulus bit-size, message
        handler: (slice<slice<byte>> args) => {
            var bitSize = binary.LittleEndian.Uint32(args[0]);
            var msg = args[1];

            var (key, err) = getRSAKey((nint)bitSize);
            if (err != default!) {
                return (default!, fmt.Errorf("generating RSA key: %w"u8, err));
            }

            var h = hashFunc();
            h.Write(msg);
            var digest = h.Sum(default!);

            slice<byte> sig = default!;
            if (!pss){
                (sig, err) = rsa.SignPKCS1v15(key, hashName, digest);
                if (err != default!) {
                    return (default!, fmt.Errorf("signing RSA message: %w"u8, err));
                }
            } else {
                (sig, err) = rsa.SignPSS(rand.Reader, key, hashFunc(), digest, h.Size());
                if (err != default!) {
                    return (default!, fmt.Errorf("signing RSA message: %w"u8, err));
                }
            }

            var (N, e, _, _, _, _, _, _) = key.Export();
            var eBytes = new slice<byte>(4);
            binary.BigEndian.PutUint32(eBytes, (uint32)e);

            return (new slice<byte>[]{N, eBytes, sig}.slice(), default!);
        }
    );
}

internal static command cmdRsaSigVerAft(Func<hash.Hash> hashFunc, @string hashName, bool pss) {
    return new command(
        requiredArgs: 4, // n, e, message, signature
        handler: (slice<slice<byte>> args) => {
            var nBytes = args[0];
            var eBytes = args[1];
            var msg = args[2];
            var sig = args[3];

            var paddedE = new slice<byte>(4);
            copy(paddedE.slice(4 - len(eBytes)), eBytes);
            ref var e = ref heap<nint>(out var Ꮡe);
            e = (nint)binary.BigEndian.Uint32(paddedE);

            var (n, err) = bigmod.NewModulus(nBytes);
            if (err != default!) {
                return (default!, fmt.Errorf("invalid RSA modulus: %w"u8, err));
            }

            var pub = Ꮡ(new rsaꓸPublicKey(
                N: n,
                E: e
            ));

            var h = hashFunc();
            h.Write(msg);
            var digest = h.Sum(default!);

            if (!pss){
                err = rsa.VerifyPKCS1v15(pub, hashName, digest, sig);
            } else {
                err = rsa.VerifyPSS(pub, hashFunc(), digest, sig);
            }
            if (err != default!) {
                return (new slice<byte>[]{new byte[]{0}.slice()}.slice(), default!);
            }

            return (new slice<byte>[]{new byte[]{1}.slice()}.slice(), default!);
        }
    );
}

// rsaKeyCache caches generated keys by modulus bit-size.
internal static map<nint, ж<rsa.PrivateKey>> rsaKeyCache = new map<nint, ж<rsa.PrivateKey>>{};

// getRSAKey returns a cached RSA private key with the specified modulus bit-size
// or generates one if necessary.
internal static (ж<rsa.PrivateKey>, error) getRSAKey(nint bits) {
    {
        var (keyΔ1, exists) = rsaKeyCache[bits, ꟷ]; if (exists) {
            return (keyΔ1, default!);
        }
    }

    var (key, err) = rsa.GenerateKey(rand.Reader, bits);
    if (err != default!) {
        return (default!, err);
    }

    rsaKeyCache[bits] = key;
    return (key, default!);
}

internal static command cmdOneStepNoCounterHmacAft(Func<hash.Hash> h) {
    return new command(
        requiredArgs: 4, // key, info, salt, outBytes
        handler: (slice<slice<byte>> args) => {
            var key = args[0];
            var info = args[1];
            var salt = args[2];
            var outBytes = binary.LittleEndian.Uint32(args[3]);

            var mac = hmac.New(h, salt);
            mac.Size();

            if (outBytes != (uint32)mac.Size()) {
                return (default!, fmt.Errorf("invalid output length: got %d, want %d"u8, outBytes, mac.Size()));
            }

            var data = new slice<byte>(0, len(key) + len(info));
            data = appendꓸꓸꓸ(data, key);
            data = appendꓸꓸꓸ(data, info);

            mac.Write(data);
            var @out = mac.Sum(default!);

            return (new slice<byte>[]{@out}.slice(), default!);
        }
    );
}

internal static command cmdKtsIfcInitiatorAft(Func<hash.Hash> h) {
    return new command(
        requiredArgs: 3, // output bytes, n bytes, e bytes
        handler: (slice<slice<byte>> args) => {
            var outputBytes = binary.LittleEndian.Uint32(args[0]);
            var nBytes = args[1];
            var eBytes = args[2];

            var (n, err) = bigmod.NewModulus(nBytes);
            if (err != default!) {
                return (default!, fmt.Errorf("invalid RSA modulus: %w"u8, err));
            }

            var paddedE = new slice<byte>(4);
            copy(paddedE.slice(4 - len(eBytes)), eBytes);
            ref var e = ref heap<nint>(out var Ꮡe);
            e = (nint)binary.BigEndian.Uint32(paddedE);
            if (e != 0x10001) {
                return (default!, errors.New("e must be 0x10001"u8));
            }

            var pub = Ꮡ(new rsaꓸPublicKey(
                N: n,
                E: e
            ));

            var dkm = new slice<byte>((nint)(outputBytes));
            {
                var (_, errΔ1) = rand.Read(dkm); if (errΔ1 != default!) {
                    return (default!, fmt.Errorf("failed to generate random DKM: %v"u8, errΔ1));
                }
            }

            (var iutC, err) = rsa.EncryptOAEP(h(), h(), rand.Reader, pub, dkm, default!);
            if (err != default!) {
                return (default!, fmt.Errorf("OAEP encryption failed: %v"u8, err));
            }

            return (new slice<byte>[]{iutC, dkm}.slice(), default!);
        }
    );
}

internal static command cmdKtsIfcResponderAft(Func<hash.Hash> h) {
    return new command(
        requiredArgs: 6, // n bytes, e bytes, p bytes, q bytes, d bytes, c bytes
        handler: (slice<slice<byte>> args) => {
            var nBytes = args[0];
            var eBytes = args[1];

            var pBytes = args[2];
            var qBytes = args[3];
            var dBytes = args[4];

            var cBytes = args[5];

            var paddedE = new slice<byte>(4);
            copy(paddedE.slice(4 - len(eBytes)), eBytes);
            nint e = (nint)binary.BigEndian.Uint32(paddedE);
            if (e != 0x10001) {
                return (default!, errors.New("e must be 0x10001"u8));
            }

            var (priv, err) = rsa.NewPrivateKey(nBytes, (nint)e, dBytes, pBytes, qBytes);
            if (err != default!) {
                return (default!, fmt.Errorf("failed to create private key: %v"u8, err));
            }

            (var dkm, err) = rsa.DecryptOAEP(h(), h(), priv, cBytes, default!);
            if (err != default!) {
                return (default!, fmt.Errorf("OAEP decryption failed: %v"u8, err));
            }

            return (new slice<byte>[]{dkm}.slice(), default!);
        }
    );
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object buildingAcvptoolˢ = (@string)"building acvptool"u8;
internal static readonly @string acvptoolExeˢ = "acvptool.exe"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string utilFipstoolsAcvpˢ = "./util/fipstools/acvp/acvptool"u8;
internal static readonly @string utilFipstoolsAcvpˢ2 = "util/fipstools/acvp/acvptool/test/check_expected.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bsslModuleᶜ = "boringssl.googlesource.com/boringssl.git"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bsslVersionᶜ = "v0.0.0-20251111011041-baaf868e6e8f"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string goAcvpModuleᶜ = "github.com/geomys/acvp-testdata"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string goAcvpVersionᶜ = "v0.0.0-20251201200548-d893de8b8b1c"u8;

public static void TestACVP(ж<testing.T> Ꮡt) {
    testenv.SkipIfShortAndSlow(new fipstest_internal_test_package.testing_TжTB(Ꮡt));

    @string bsslModule = bsslModuleᶜ;
    @string bsslVersion = bsslVersionᶜ;
    @string goAcvpModule = goAcvpModuleᶜ;
    @string goAcvpVersion = goAcvpVersionᶜ;

    // In crypto/tls/bogo_shim_test.go the test is skipped if run on a builder with runtime.GOOS == "windows"
    // due to flaky networking. It may be necessary to do the same here.

    // Stat the acvp test config file so the test will be re-run if it changes, invalidating cached results
    // from the old config.
    {
        var (_, errΔ1) = os.Stat(testConfigFile); if (errΔ1 != default!) {
            Ꮡt.Fatalf("failed to stat config file: %s"u8, errΔ1);
        }
    }

    // Fetch the BSSL module and use the JSON output to find the absolute path to the dir.
    @string bsslDir = cryptotest.FetchModule(Ꮡt, bsslModule, bsslVersion);

    Ꮡt.Log(buildingAcvptoolˢ);

    // Build the acvptool binary.
    @string toolPath = filepath.Join(Ꮡt.TempDir(), acvptoolExeˢ);
    var cmd = testenv.Command(new fipstest_internal_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new fipstest_internal_test_package.testing_TжTB(Ꮡt)),
        buildˢ,
        "-o", toolPath,
        utilFipstoolsAcvpˢ);
    cmd.Value.Dir = bsslDir;
    {
        var (outΔ1, errΔ2) = cmd.CombinedOutput(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("failed to build acvptool: %s\n%s"u8, errΔ2, outΔ1);
        }
    }

    // Similarly, fetch the ACVP data module that has vectors/expected answers.
    @string dataDir = cryptotest.FetchModule(Ꮡt, goAcvpModule, goAcvpVersion);

    var (cwd, err) = os.Getwd();
    if (err != default!) {
        Ꮡt.Fatalf("failed to fetch cwd: %s"u8, err);
    }
    @string configPath = filepath.Join(cwd, testConfigFile);
    Ꮡt.Logf("running check_expected.go\ncwd: %q\ndata_dir: %q\nconfig: %q\ntool: %q\nmodule-wrapper: %q\n"u8,
        cwd, dataDir, configPath, toolPath, testenv.Executable(new fipstest_internal_test_package.testing_TжTB(Ꮡt)));

    // Run the check_expected test driver using the acvptool we built, and this test binary as the
    // module wrapper. The file paths in the config file are specified relative to the dataDir root
    // so we run the command from that dir.
    var args = new @string[]{
        "run"u8,
        filepath.Join(bsslDir, utilFipstoolsAcvpˢ2),
        "-tool"u8,
        toolPath,
        // Note: module prefix must match Wrapper value in testConfigFile.
        "-module-wrappers"u8, "go:"u8 + testenv.Executable(new fipstest_internal_test_package.testing_TжTB(Ꮡt)),
        "-tests"u8, configPath
    }.slice();
    cmd = testenv.Command(new fipstest_internal_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new fipstest_internal_test_package.testing_TжTB(Ꮡt)), args.ꓸꓸꓸ);
    cmd.Value.Dir = dataDir;
    cmd.Value.Env = append(os.Environ(),
        "ACVP_WRAPPER=1"u8,
        "GODEBUG=fips140=on");
    (var @out, err) = cmd.CombinedOutput();
    Ꮡt.Logf("\n%s"u8, @out);
    if (err != default!) {
        Ꮡt.Fatalf("failed to run acvp tests: %s"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "test"u8;
internal static readonly @string expected1ArgsGot0ˢ = "expected 1 args, got 0"u8;

public static void TestTooFewArgs(ж<testing.T> Ꮡt) {
    commands[testˢ] = new command(
        requiredArgs: 1,
        handler: (slice<slice<byte>> args) => {
            {
                nint gotArgs = len(args); if (gotArgs != 1) {
                    return (default!, fmt.Errorf("expected 1 args, got %d"u8, gotArgs));
                }
            }
            return (default!, default!);
        }
    );

    ref var output = ref heap(new bytes.Buffer(), out var Ꮡoutput);
    var err = processingLoop(mockRequest(Ꮡt, testˢ, default!), new fipstest_internal_test_package.bytes_BufferжWriter(Ꮡoutput));
    if (err == default!) {
        Ꮡt.Fatalf("expected error, got nil"u8);
    }
    @string expectedErr = expected1ArgsGot0ˢ;
    if (!strings.Contains(err.Error(), expectedErr)) {
        Ꮡt.Errorf("expected error to contain %q, got %v"u8, expectedErr, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expected1ArgsGot2ˢ = "expected 1 args, got 2"u8;

public static void TestTooManyArgs(ж<testing.T> Ꮡt) {
    commands[testˢ] = new command(
        requiredArgs: 1,
        handler: (slice<slice<byte>> args) => {
            {
                nint gotArgs = len(args); if (gotArgs != 1) {
                    return (default!, fmt.Errorf("expected 1 args, got %d"u8, gotArgs));
                }
            }
            return (default!, default!);
        }
    );

    ref var output = ref heap(new bytes.Buffer(), out var Ꮡoutput);
    var err = processingLoop(mockRequest(
        Ꮡt, testˢ, new slice<byte>[]{slice<byte>("one"u8), slice<byte>("two"u8)}.slice()), new fipstest_internal_test_package.bytes_BufferжWriter(Ꮡoutput));
    if (err == default!) {
        Ꮡt.Fatalf("expected error, got nil"u8);
    }
    @string expectedErr = expected1ArgsGot2ˢ;
    if (!strings.Contains(err.Error(), expectedErr)) {
        Ꮡt.Errorf("expected error to contain %q, got %v"u8, expectedErr, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getConfigˢ = "getConfig"u8;

public static void TestGetConfig(ж<testing.T> Ꮡt) {
    ref var output = ref heap(new bytes.Buffer(), out var Ꮡoutput);
    var err = processingLoop(mockRequest(Ꮡt, getConfigˢ, default!), new fipstest_internal_test_package.bytes_BufferжWriter(Ꮡoutput));
    if (err != default!) {
        Ꮡt.Errorf("unexpected error: %v"u8, err);
    }

    var respArgs = readResponse(Ꮡt, new fipstest_internal_test_package.bytes_BufferжReader(Ꮡoutput));
    if (len(respArgs) != 1) {
        Ꮡt.Fatalf("expected 1 response arg, got %d"u8, len(respArgs));
    }

    if (!bytes.Equal(respArgs[0], capabilitiesJson)) {
        Ꮡt.Errorf("expected config %q, got %q"u8, (@string)capabilitiesJson, (@string)respArgs[0]);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sha2256ˢ = "SHA2-256"u8;

public static void TestSha2256(ж<testing.T> Ꮡt) {
    var testMessage = slice<byte>("gophers eat grass"u8);
    var expectedDigest = new byte[]{
        188, 142, 10, 214, 48, 236, 72, 143, 70, 216, 223, 205, 219, 69, 53, 29,
        205, 207, 162, 6, 14, 70, 113, 60, 251, 170, 201, 236, 119, 39, 141, 172
    }.slice();

    ref var output = ref heap(new bytes.Buffer(), out var Ꮡoutput);
    var err = processingLoop(mockRequest(Ꮡt, sha2256ˢ, new slice<byte>[]{testMessage}.slice()), new fipstest_internal_test_package.bytes_BufferжWriter(Ꮡoutput));
    if (err != default!) {
        Ꮡt.Errorf("unexpected error: %v"u8, err);
    }

    var respArgs = readResponse(Ꮡt, new fipstest_internal_test_package.bytes_BufferжReader(Ꮡoutput));
    if (len(respArgs) != 1) {
        Ꮡt.Fatalf("expected 1 response arg, got %d"u8, len(respArgs));
    }

    if (!bytes.Equal(respArgs[0], expectedDigest)) {
        Ꮡt.Errorf("expected digest %v, got %v"u8, expectedDigest, respArgs[0]);
    }
}

internal static io.Reader mockRequest(ж<testing.T> Ꮡt, @string cmd, slice<slice<byte>> args) {
    Ꮡt.Helper();

    var msgData = appendꓸꓸꓸ(new slice<byte>[]{slice<byte>(cmd)}.slice(), args);

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = writeResponse(new fipstest_internal_test_package.bytes_BufferжWriter(Ꮡbuf), msgData); if (err != default!) {
            Ꮡt.Fatalf("writeResponse error: %v"u8, err);
        }
    }

    return new fipstest_internal_test_package.bytes_BufferжReader(Ꮡbuf);
}

internal static slice<slice<byte>> readResponse(ж<testing.T> Ꮡt, io.Reader reader) {
    ref var numArgs = ref heap(new uint32(), out var ᏑnumArgs);
    {
        var errΔ1 = binary.Read(reader, binary.LittleEndian, ᏑnumArgs); if (errΔ1 != default!) {
            Ꮡt.Fatalf("failed to read response args count: %v"u8, errΔ1);
        }
    }

    var (args, err) = readArgs(reader, numArgs);
    if (err != default!) {
        Ꮡt.Fatalf("failed to read %d response args: %v"u8, numArgs, err);
    }

    return args;
}

} // end fipstest_internal_test_package
