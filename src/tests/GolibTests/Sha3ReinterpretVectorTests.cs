using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using sha3 = go.crypto.sha3_package;

namespace GolibTests;

[TestClass]
public class Sha3ReinterpretVectorTests
{
    // WHY THIS LIVES IN GolibTests, and why it exists at all.
    //
    // `vendor/golang.org/x/crypto/sha3`'s xor.go views its `[25]uint64` sponge state as `[200]byte`
    // (`ab := (*[25 * 64 / 8]byte)(unsafe.Pointer(&d.a))`) to absorb input and squeeze output. A
    // `byte[]` view over a `uint64[]` has no managed spelling, so the auto-conversion took the
    // raw-address route and dereferencing it fabricated an `array<byte>` backing reference out of
    // the keccak state's own DATA -- a fatal AccessViolationException that crypto/tls reached on
    // every TLS 1.3 ClientHello through mlkem768 key generation. `xor.cs` is hand-owned
    // (`[module: GoManualConversion]`) and takes crypto/subtle's remedy: MemoryMarshal.AsBytes over
    // `array<uint64>.ToSpan()`, a genuine ALIASING view of the same backing storage.
    //
    // That remedy has NO other guard. The vendored package carries no `_test.go` in GOROOT, and a
    // behavioral test cannot reach it -- the converter resolves an import of
    // `golang.org/x/crypto/sha3` to `core/golang.org/...`, not `core/vendor/golang.org/...`. Its
    // natural operational guard, `crypto/internal/mlkem768`'s suite, is blocked on two unrelated
    // test-conversion defects. So known-answer vectors it is, and they belong beside the golib
    // pointer tests because what they really prove is that the aliasing view WRITES THROUGH: a
    // snapshot instead of an alias yields a wrong digest, loudly, on the very first vector.
    // (GolibTests' own reference to `core/sort`, in GolibTests.csproj, is the precedent for an
    // MSTest tier binding a converted package.)
    //
    // ⚠ NEUTERED-FIX CONTROL — restoring the auto-converted `xor.cs` does not merely fail these
    // tests, it KILLS the test host with an AccessViolationException inside `slice<byte>`'s
    // constructor. That is the defect's whole character and why it cost a per-test census of
    // crypto/tls to localize; expect a dead host, not a red test, if you ever re-run that control.
    //
    // The short vectors are FIPS-202's own. The long ones are checked against the OS SHA-3
    // implementation, an oracle with no dependency on anything in this repository, and are chosen to
    // straddle the rate boundary (136 bytes for SHA3-256, 72 for SHA3-512) so the multi-block absorb
    // path -- the one that XORs into the state repeatedly and permutes between blocks -- is covered
    // rather than assumed.

    private static string hex(slice<byte> b)
    {
        StringBuilder sb = new();

        for (nint i = 0; i < b.Length; i++)
            sb.Append(b[i].ToString("x2"));

        return sb.ToString();
    }

    private static string hex(array<byte> a)
    {
        StringBuilder sb = new();

        for (nint i = 0; i < a.Length; i++)
            sb.Append(a[i].ToString("x2"));

        return sb.ToString();
    }

    private static byte[] pattern(int length, int step, int seed)
    {
        byte[] data = new byte[length];

        for (int i = 0; i < length; i++)
            data[i] = (byte)(i * step + seed);

        return data;
    }

    [TestMethod]
    public void FipsVectorsMatch()
    {
        Assert.AreEqual("a7ffc6f8bf1ed76651c14756a061d662f580ff4de43b49fa82d80a4b80f8434a",
            hex(sha3.Sum256(new slice<byte>(0))));

        Assert.AreEqual("3a985da74fe225b2045c172d6bd390bd855f086e3e9d525b46bfe24511431532",
            hex(sha3.Sum256(((@string)"abc").slice())));

        Assert.AreEqual("b751850b1a57168a5693cd924b6b096e08f621827444f70d884f5d0240d2712e" +
                        "10e116e9192af3c91a7ec57647e3934057340b4cf408d5a56592f8274eec53f0",
            hex(sha3.Sum512(((@string)"abc").slice())));
    }

    [TestMethod]
    public void ShakeVectorMatches()
    {
        // SHAKE drives the same xorIn/copyOut pair through a different rate and a squeeze longer
        // than one permutation's worth of output.
        //
        // 1.24 SPELLS THE SQUEEZE DIFFERENTLY, and this is the only call in the file that moved.
        // The vendored 1.23 form wrote into a caller's buffer -- ShakeSum256(hash, data) is
        // NewShake256(); Write(data); Read(hash) -- so the LENGTH was carried by len(hash). 1.24's
        // crypto/sha3 returns the squeeze instead: SumSHAKE256(data, length). Same function, same
        // vector; the 32 that was the buffer's length is now the argument.
        slice<byte> @out = sha3.SumSHAKE256(((@string)"abc").slice(), 32);

        Assert.AreEqual("483366601360a8771c6863080cc4114d8db44530f8f1e1ee4f94ea37e78b5739", hex(@out));
    }

    [TestMethod]
    public void MultiBlockAbsorbMatchesTheOsImplementation()
    {
        if (!SHA3_256.IsSupported || !SHA3_512.IsSupported)
        {
            Assert.Inconclusive("OS SHA-3 unavailable; the FIPS vector tests still cover the single-block path.");
            return;
        }

        // 135/136/137 straddle SHA3-256's 136-byte rate; every length also crosses SHA3-512's 72.
        foreach (int n in new[] { 135, 136, 137, 200, 1000, 4096 })
        {
            byte[] data = pattern(n, 31, 7);

            Assert.AreEqual(Convert.ToHexString(SHA3_256.HashData(data)).ToLowerInvariant(),
                hex(sha3.Sum256(new slice<byte>(data))), $"SHA3-256 at length {n}");

            Assert.AreEqual(Convert.ToHexString(SHA3_512.HashData(data)).ToLowerInvariant(),
                hex(sha3.Sum512(new slice<byte>(data))), $"SHA3-512 at length {n}");
        }
    }

    [TestMethod]
    public void UnalignedInputSliceMatchesTheOsImplementation()
    {
        if (!SHA3_256.IsSupported)
        {
            Assert.Inconclusive("OS SHA-3 unavailable.");
            return;
        }

        // The absorb reads the INPUT through MemoryMarshal.Cast as well, and a re-sliced `slice<byte>`
        // starts at an arbitrary offset in its backing array -- so the word-at-a-time read is
        // unaligned. Offset 13 is deliberately coprime with 8.
        byte[] backing = pattern(4096 + 13, 17, 3);

        Assert.AreEqual(Convert.ToHexString(SHA3_256.HashData(backing.AsSpan(13))).ToLowerInvariant(),
            hex(sha3.Sum256(new slice<byte>(backing)[13..])));
    }
}
