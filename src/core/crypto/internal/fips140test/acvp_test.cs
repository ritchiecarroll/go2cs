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
using cryptotest = go.crypto.@internal.cryptotest_package;
using fips140 = go.crypto.@internal.fips140_package;
using ecdsa = go.crypto.@internal.fips140.ecdsa_package;
using hmac = go.crypto.@internal.fips140.hmac_package;
using mlkem = go.crypto.@internal.fips140.mlkem_package;
using pbkdf2 = go.crypto.@internal.fips140.pbkdf2_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using sha512 = go.crypto.@internal.fips140.sha512_package;
// blank import: embed_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using exec = go.os.exec_package;
using fs = go.io.fs_package;
using go.@internal;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.os;
using path;

partial class fipstest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string acvpWrapperˢ = "ACVP_WRAPPER"u8;

public static void TestMain(ж<testing.M> Ꮡm) {
    if (os.Getenv(acvpWrapperˢ) == "1"u8){
        wrapperMain();
    } else {
        os.Exit(Ꮡm.Run());
    }
}

internal static void wrapperMain() {
    {
        var err = processingLoop(new fipstest_internal_test_package.bufio_ReaderжReader(bufio.NewReader(new fipstest_internal_test_package.os_FileжReader(os.Stdin))), new os.FileжWriter(os.Stdout)); if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "processing error: %v\n"u8, err);
            os.Exit(1);
        }
    }
}

[GoType] internal partial struct request {
    internal @string name;
    internal slice<slice<byte>> args;
}

// type commandHandler is a methodless func type — rendered inline as its base delegate

[GoType] internal partial struct command {
    // requiredArgs enforces that an exact number of arguments are provided to the handler.
    internal nint requiredArgs;
    internal Func<slice<slice<byte>>, (slice<slice<byte>>, error)> handler;
}

internal static slice<byte> capabilitiesJson = go.embed_package.ΔEmbedBytes<byte>(typeof(fipstest_internal_test_package).Assembly, "go.embed/crypto/internal/fips140test_test/", "acvp_capabilities.json");
internal static map<@string, command> commands = new map<@string, command>{
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
    ["hmacDRBG/SHA3-512"u8] = cmdHmacDrbgAft(() => new fipstest_internal_test_package.sha3_DigestжHash(sha3.New512()))
};

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
    foreach (var (i, _) in argLengths) {
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
    return new command(
        handler: (slice<slice<byte>> args) => (new slice<byte>[]{capabilitiesJson}.slice(), default!)
    );
}

// cmdHashAft returns a command handler for the specified hash
// algorithm for algorithm functional test (AFT) test cases.
//
// This shape of command expects a message as the sole argument,
// and writes the resulting digest as a response.
//
// See https://pages.nist.gov/ACVP/draft-celi-acvp-sha.html
internal static command cmdHashAft(fips140.Hash h) {
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
internal static command cmdHashMct(fips140.Hash h) {
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
internal static command cmdSha3Mct(fips140.Hash h) {
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

internal static command cmdHmacAft(Func<fips140.Hash> h) {
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

internal static command cmdPbkdf() {
    return new command( // Hash name, key length, salt, password, iteration count

        requiredArgs: 5,
        handler: (slice<slice<byte>> args) => {
            var (h, err) = lookupHash(((@string)args[0]));
            if (err != default!) {
                return (default!, fmt.Errorf("PBKDF2 failed: %w"u8, err));
            }
            var keyLen = binary.LittleEndian.Uint32(args[1]) / 8;
            var salt = args[2];
            var password = args[3];
            var iterationCount = binary.LittleEndian.Uint32(args[4]);
            (var derivedKey, err) = pbkdf2.Key(h, ((@string)password), salt, (nint)iterationCount, (nint)keyLen);
            if (err != default!) {
                return (default!, fmt.Errorf("PBKDF2 failed: %w"u8, err));
            }
            return (new slice<byte>[]{derivedKey}.slice(), default!);
        }
    );
}

internal static (Func<fips140.Hash>, error) lookupHash(@string name) {
    Func<fips140.Hash> h = default!;
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

internal static command cmdHmacDrbgAft(Func<fips140.Hash> h) {
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string acvpTestConfigJsonˢ = "acvp_test.config.json"u8;
internal static readonly object buildingAcvptoolˢ = (@string)"building acvptool"u8;
internal static readonly @string acvptoolExeˢ = "acvptool.exe"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string utilFipstoolsAcvpˢ = "./util/fipstools/acvp/acvptool"u8;
internal static readonly @string utilFipstoolsAcvpˢ2 = "util/fipstools/acvp/acvptool/test/check_expected.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bsslModuleᶜ = "boringssl.googlesource.com/boringssl.git"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bsslVersionᶜ = "v0.0.0-20250108043213-d3f61eeacbf7"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string goAcvpModuleᶜ = "github.com/cpu/go-acvp"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string goAcvpVersionᶜ = "v0.0.0-20250102201911-6839fc40f9f8"u8;

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
        var (_, errΔ1) = os.Stat(acvpTestConfigJsonˢ); if (errΔ1 != default!) {
            Ꮡt.Fatalf("failed to stat config file: %s"u8, errΔ1);
        }
    }
    // Fetch the BSSL module and use the JSON output to find the absolute path to the dir.
    @string bsslDir = cryptotest.FetchModule(Ꮡt, bsslModule, bsslVersion);
    Ꮡt.Log(buildingAcvptoolˢ);
    // Build the acvptool binary.
    @string toolPath = filepath.Join(Ꮡt.TempDir(), acvptoolExeˢ);
    @string goTool = testenv.GoToolPath(new fipstest_internal_test_package.testing_TжTB(Ꮡt));
    var cmd = testenv.Command(new fipstest_internal_test_package.testing_TжTB(Ꮡt), goTool,
        buildˢ,
        "-o", toolPath,
        utilFipstoolsAcvpˢ);
    cmd.Value.Dir = bsslDir;
    var @out = Ꮡ(new strings.Builder(nil));
    cmd.Value.Stderr = new fipstest_internal_test_package.strings_BuilderжWriter(@out);
    {
        var errΔ2 = cmd.Run(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("failed to build acvptool: %s\n%s"u8, errΔ2, @out.String());
        }
    }
    // Similarly, fetch the ACVP data module that has vectors/expected answers.
    @string dataDir = cryptotest.FetchModule(Ꮡt, goAcvpModule, goAcvpVersion);
    var (cwd, err) = os.Getwd();
    if (err != default!) {
        Ꮡt.Fatalf("failed to fetch cwd: %s"u8, err);
    }
    @string configPath = filepath.Join(cwd, acvpTestConfigJsonˢ);
    Ꮡt.Logf("running check_expected.go\ncwd: %q\ndata_dir: %q\nconfig: %q\ntool: %q\nmodule-wrapper: %q\n"u8,
        cwd, dataDir, configPath, toolPath, os.Args[0]);
    // Run the check_expected test driver using the acvptool we built, and this test binary as the
    // module wrapper. The file paths in the config file are specified relative to the dataDir root
    // so we run the command from that dir.
    var args = new @string[]{
        "run"u8,
        filepath.Join(bsslDir, utilFipstoolsAcvpˢ2),
        "-tool"u8,
        toolPath, // Note: module prefix must match Wrapper value in acvp_test.config.json.

        "-module-wrappers"u8, "go:" + os.Args[0],
        "-tests"u8, configPath
    }.slice();
    cmd = testenv.Command(new fipstest_internal_test_package.testing_TжTB(Ꮡt), goTool, args.ꓸꓸꓸ);
    cmd.Value.Dir = dataDir;
    cmd.Value.Env = append(os.Environ(), "ACVP_WRAPPER=1"u8);
    (var output, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to run acvp tests: %s\n%s"u8, err, ((@string)output));
    }
    Ꮡt.Log(((@string)output));
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
        Ꮡt.Errorf("expected config %q, got %q"u8, ((@string)capabilitiesJson), ((@string)respArgs[0]));
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
