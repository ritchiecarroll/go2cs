// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using Δcrypto = crypto_package;
using rand = global::go.crypto.rand_package;
using rsa = global::go.crypto.rsa_package;
using x509 = global::go.crypto.x509_package;
using pem = encoding.pem_package;
using errors = errors_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using fs = global::go.io.fs_package;
using Δos = os_package;
using filepath = path.filepath_package;
using Δregexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using encoding;
using global::go.crypto;
using global::go.io;
using hash = hash_package;
using iter = iter_package;
using path;

partial class crypto_test_package {

partial struct messageSignerOnly {
    internal ж<rsa.PrivateKey> k;
}

internal static cryptoꓸPublicKey Public(this ref messageSignerOnly s) {
    return s.k.Public();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string unimplementedˢ = "unimplemented"u8;

internal static (slice<byte>, error) Sign(this ref messageSignerOnly s, Δio.Reader _Δp1, slice<byte> _Δp2, Δcrypto.SignerOpts _Δp3) {
    return (default!, errors.New(unimplementedˢ));
}

internal static (slice<byte>, error) SignMessage(this ref messageSignerOnly s, Δio.Reader rand, slice<byte> msg, Δcrypto.SignerOpts opts) {
    var h = opts.HashFunc().New();
    h.Write(msg);
    var digest = h.Sum(default!);
    return s.k.Sign(rand, digest, opts);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string beginRsaTestingKeyˢ = """
-----BEGIN RSA TESTING KEY-----
MIIEowIBAAKCAQEAsPnoGUOnrpiSqt4XynxA+HRP7S+BSObI6qJ7fQAVSPtRkqso
tWxQYLEYzNEx5ZSHTGypibVsJylvCfuToDTfMul8b/CZjP2Ob0LdpYrNH6l5hvFE
89FU1nZQF15oVLOpUgA7wGiHuEVawrGfey92UE68mOyUVXGweJIVDdxqdMoPvNNU
l86BU02vlBiESxOuox+dWmuVV7vfYZ79Toh/LUK43YvJh+rhv4nKuF7iHjVjBd9s
B6iDjj70HFldzOQ9r8SRI+9NirupPTkF5AKNe6kUhKJ1luB7S27ZkvB3tSTT3P59
3VVJvnzOjaA1z6Cz+4+eRvcysqhrRgFlwI9TEwIDAQABAoIBAEEYiyDP29vCzx/+
dS3LqnI5BjUuJhXUnc6AWX/PCgVAO+8A+gZRgvct7PtZb0sM6P9ZcLrweomlGezI
FrL0/6xQaa8bBr/ve/a8155OgcjFo6fZEw3Dz7ra5fbSiPmu4/b/kvrg+Br1l77J
aun6uUAs1f5B9wW+vbR7tzbT/mxaUeDiBzKpe15GwcvbJtdIVMa2YErtRjc1/5B2
BGVXyvlJv0SIlcIEMsHgnAFOp1ZgQ08aDzvilLq8XVMOahAhP1O2A3X8hKdXPyrx
IVWE9bS9ptTo+eF6eNl+d7htpKGEZHUxinoQpWEBTv+iOoHsVunkEJ3vjLP3lyI/
fY0NQ1ECgYEA3RBXAjgvIys2gfU3keImF8e/TprLge1I2vbWmV2j6rZCg5r/AS0u
pii5CvJ5/T5vfJPNgPBy8B/yRDs+6PJO1GmnlhOkG9JAIPkv0RBZvR0PMBtbp6nT
Y3yo1lwamBVBfY6rc0sLTzosZh2aGoLzrHNMQFMGaauORzBFpY5lU50CgYEAzPHl
u5DI6Xgep1vr8QvCUuEesCOgJg8Yh1UqVoY/SmQh6MYAv1I9bLGwrb3WW/7kqIoD
fj0aQV5buVZI2loMomtU9KY5SFIsPV+JuUpy7/+VE01ZQM5FdY8wiYCQiVZYju9X
Wz5LxMNoz+gT7pwlLCsC4N+R8aoBk404aF1gum8CgYAJ7VTq7Zj4TFV7Soa/T1eE
k9y8a+kdoYk3BASpCHJ29M5R2KEA7YV9wrBklHTz8VzSTFTbKHEQ5W5csAhoL5Fo
qoHzFFi3Qx7MHESQb9qHyolHEMNx6QdsHUn7rlEnaTTyrXh3ifQtD6C0yTmFXUIS
CW9wKApOrnyKJ9nI0HcuZQKBgQCMtoV6e9VGX4AEfpuHvAAnMYQFgeBiYTkBKltQ
XwozhH63uMMomUmtSG87Sz1TmrXadjAhy8gsG6I0pWaN7QgBuFnzQ/HOkwTm+qKw
AsrZt4zeXNwsH7QXHEJCFnCmqw9QzEoZTrNtHJHpNboBuVnYcoueZEJrP8OnUG3r
UjmopwKBgAqB2KYYMUqAOvYcBnEfLDmyZv9BTVNHbR2lKkMYqv5LlvDaBxVfilE0
2riO4p6BaAdvzXjKeRrGNEKoHNBpOSfYCOM16NjL8hIZB1CaV3WbT5oY+jp7Mzd5
7d56RZOE+ERK2uz/7JX9VSsM/LbH9pJibd4e8mikDS9ntciqOH/3
-----END RSA TESTING KEY-----
"""u8.Raw();
private static readonly @string testingKeyˢ = "TESTING KEY"u8;
private static readonly @string privateKeyˢ = "PRIVATE KEY"u8;

public static void TestSignMessage(ж<testing.T> Ꮡt) {
    var (block, _) = pem.Decode(slice<byte>(strings.ReplaceAll(
        beginRsaTestingKeyˢ, testingKeyˢ, privateKeyˢ)));
    var (k, _) = x509.ParsePKCS1PrivateKey((~block).Bytes);

    var msg = slice<byte>("hello :)"u8);

    var h = Δcrypto.SHA256.New();
    h.Write(msg);
    var digest = h.Sum(default!);

    var (sig, err) = Δcrypto.SignMessage(new rsa_PrivateKeyжSigner(k), rand.Reader, msg, new rsa_PSSOptionsжSignerOpts(Ꮡ(new rsa.PSSOptions(Hash: Δcrypto.SHA256))));
    if (err != default!) {
        Ꮡt.Fatalf("SignMessage failed with Signer: %s"u8, err);
    }
    {
        var errΔ1 = rsa.VerifyPSS(k.of(rsa.PrivateKey.ᏑPublicKey), Δcrypto.SHA256, digest, sig, nil); if (errΔ1 != default!) {
            Ꮡt.Errorf("VerifyPSS failed for Signer signature: %s"u8, errΔ1);
        }
    }

    (sig, err) = Δcrypto.SignMessage(new messageSignerOnlyжSigner(Ꮡ(new messageSignerOnly(k))), rand.Reader, msg, new rsa_PSSOptionsжSignerOpts(Ꮡ(new rsa.PSSOptions(Hash: Δcrypto.SHA256))));
    if (err != default!) {
        Ꮡt.Fatalf("SignMessage failed with MessageSigner: %s"u8, err);
    }
    {
        var errΔ2 = rsa.VerifyPSS(k.of(rsa.PrivateKey.ᏑPublicKey), Δcrypto.SHA256, digest, sig, nil); if (errΔ2 != default!) {
            Ꮡt.Errorf("VerifyPSS failed for MessageSigner signature: %s"u8, errΔ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string internalˢ = "internal"u8;
private static readonly @string boringˢ = "boring"u8;
private static readonly @string sigˢ = "sig"u8;
private static readonly @string sByteWordSˢ = @"(^|;)\s(BYTE|WORD)\s"u8;
private static readonly @string srcCryptoˢ = "src/crypto"u8;

public static void TestDisallowedAssemblyInstructions(ж<testing.T> Ꮡt) {
    // This test enforces the cryptography assembly policy rule that we do not
    // use BYTE or WORD instructions, since these instructions can obscure what
    // the assembly is actually doing. If we do not support specific
    // instructions in the assembler, we should not be using them until we do.
    //
    // Instead of using the output of the 'go tool asm' tool, we take the simple
    // approach and just search the text of .s files for usage of BYTE and WORD.
    // We do this because the assembler itself will sometimes insert WORD
    // instructions for things like function preambles etc.

    @string boringSigPath = filepath.Join(internalˢ, boringˢ, sigˢ);

    var (matcher, err) = Δregexp.Compile(sByteWordSˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var matcherʗ1 = matcher;
        var errΔ1 = filepath.WalkDir(filepath.Join(testenv.GOROOT(new testing_TжTB(Ꮡt)), srcCryptoˢ), (@string path, fs.DirEntry d, error errΔ2) => {
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
            if (d.IsDir() || !strings.HasSuffix(path, ".s"u8)) {
                return default!;
            }
            if (strings.Contains(path, boringSigPath)) {
                return default!;
            }

            (var f, errΔ2) = Δos.ReadFile(path);
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }

            nint i = 1;
            foreach (var line in range<slice<byte>>(bytes.Lines(f).Invoke)) {
                if (matcherʗ1.Match(line)) {
                    Ꮡt.Errorf("%s:%d assembly contains BYTE or WORD instruction (%q)"u8, path, i, (@string)line);
                }
                i++;
            }

            return default!;
        }); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
}

} // end crypto_test_package
