// hash_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.memhash / memhash32 / memhash64 / strhash — the FLAT hash-stub family, bodied over Go's
// own fallback arithmetic (hash64.go, the wyhash schedule) applied to the bytes the managed
// referent actually holds.
//
// Go declares these four in alg.go and implements them in assembly (asm_amd64.s: each is a
// dispatcher that jumps to aeshashbody when useAeshash is set and to the pure-Go *Fallback body
// otherwise), so the converted declarations in alg.cs are bodyless partials and PartialStubGenerator
// fills them with a throw. On the Linux runtime row's bill (2026-09-04) that throw was the second
// stub family by rows — memhash 8, strhash 2, memhash32/64 one each, plus the TestTraceMapConcurrent
// death — behind getg alone.
//
// WHY THE CONVERTED FALLBACKS ARE NOT SIMPLY CALLED. memhashFallback/memhash32Fallback/
// memhash64Fallback/strhashFallback ARE in the corpus (hash64.cs, alg.cs), but they read memory
// through readUnaligned32/64, whose emitted form casts the unsafe.Pointer to a ж<array<byte>> —
// and that cast is only honest when the pointer's referent IS an array<byte>. Measured on a scratch
// probe over golib's public API (2026-09-04, Release, tiering off): a pointer minted from a uint64
// box resolves to its StandardBox<uint64>, the cast to a different T mints a NativeBox over the
// address, and the read panics `index out of range [0] with length 0`. So the bodies here recover
// the referent THEMSELVES and hash its bytes; the arithmetic below is hash64.go's, ported verbatim,
// so memhash32(p, h) == memhash(p, h, 4) and memhash64(p, h) == memhash(p, h, 8) hold by
// construction exactly as Go's TestMemHash32Equality/TestMemHash64Equality assert.
//
// WHAT A POINTER CAN BE HERE (the recovery rule, in order):
//   1. NOT an unsafe.Pointer at all. The runtime's own bytesHash reads `(*slice)(unsafe.Pointer(&b))`
//      and stringHash's strhashFallback reads `(*stringStruct)(a)`; golib's Reinterpret cannot alias
//      a golib slice<T>/@string onto those headers (five fields and a T[] against three), so it mints
//      a NativeBox over the PINNED managed struct, and the header's `array`/`str` field reads back the
//      m_array bits as a Pointer REFERENCE — a reference whose real runtime type is System.Byte[]
//      (probe arm A: `array` -> System.Byte[], len 16, cap 8 where Go says 8 and 24). Dereferencing
//      it is a native SIGSEGV with empty stderr (the bill's TestMapBuckets signature). Every body
//      therefore checks the argument's REAL method table first — GetType() reads it without touching
//      a field — and PANICS naming the class instead of dereferencing it. That turns a host-killer
//      into a named red row; it does not make the row pass. The slice/string HEADER seam that would
//      make bytesHash/stringHash honest is sized as its own increment (see the board, 2026-09-04).
//   2. A Pointer that RETAINS its box (unsafe.Pointer.FromPinnedBox/FromBox keep the ж<T> it was
//      minted from): an ElemRefBox<byte> into a byte[] (unsafe.StringData(s), &b[i]), an array<byte>
//      box (&[N]byte), an unmanaged scalar box, a @string box (strhash only — its CONTENT, which is
//      what strhashFallback hashes through x.str/x.len).
//   3. A Pointer that carries only the NUMBER (the `(uintptr)` bridge the converter emits around
//      unsafe.Pointer-valued calls strips retention): the provenance record resolves it back to its
//      box for a pinnable (unmanaged) pointee — probe arm E — and answers null for a reference-bearing
//      one until Q44's token registry lands (SUB-Q42's measured class). A null here is a PANIC
//      naming the function: the class-B/C refusal rule, never a plausible number.
//      ⚠ The number resolves only WHILE THE BOX LIVES (validate-on-read: alive and still pinned
//      there, the pin living on the box), and the emitted int32Hash/int64Hash mint the box in their
//      own frame and keep no reference past the mint — so under Release with tiering off a
//      collection between the mint and this body's resolve retires the entry and the call REFUSES.
//      Measured 2026-09-05 on TestSmhasherWindowed: ~2M Int32Hash calls, the first one a GC
//      interrupted panicked by name. Deterministic in the guard (mint in a NoInlining frame, drop,
//      GC, resolve). The remedy is retention THROUGH the bridge, upstream of this file.
//   Anything else (a header box, a struct box holding references, a foreign type) is refused by
//   name with its runtime type in the message.
//
// hashkey. Go seeds hashkey[0..3] in alginit (the non-AES branch) from bootstrapRand during
// schedinit; the converted scheduler never runs schedinit, so the four words are zero at master.
// The seeding is done here at first use from the OS CSPRNG — the same per-process randomness Go's
// branch provides (it is what TestMemHashGlobalSeed's cross-process property rests on) and the
// goenvs/cpu precedent of a hand-own performing the init the scheduler would have. Idempotent, and
// a non-zero key already present (alginit having run) is left alone.
//
// AESHASH. Go's alginit installs the AES hash when `(GOARCH == "386" || GOARCH == "amd64") &&
// cpu.X86.HasAES && cpu.X86.HasSSSE3 && cpu.X86.HasSSE41` (and on arm64 when cpu.ARM64.HasAES), and
// then memhash/memhash32/memhash64/strhash jump to asm_amd64.s's aeshashbody instead of the fallback.
// initHashAlgorithm below runs that same selection once, at load (schedinit never runs here), over
// the same flags: internal/cpu fills cpu.X86 from System.Runtime.Intrinsics and applies GODEBUG's
// cpu.* options (so GODEBUG=cpu.aes=off selects the fallback, as TestMemHashGlobalSeed/noaes and
// TestIssue66841 rely on). The selected branch seeds its own key exactly as Go's does: aeskeysched's
// 128 random bytes for AES, the four hashkey words for the fallback. The AES bodies are
// aeshashbody, memhash32 and memhash64 ported instruction for instruction onto
// System.Runtime.Intrinsics.X86.Aes: Go's `AESENC src, dst` is `dst = Aes.Encrypt(dst, src)`. They are
// bit-reproducible against Go for the same key schedule (GolibTests' AesHashMatchesGo compares a fixed
// schedule against a table taken from Go itself). The corpus is converted for GOARCH=amd64, so on an
// arm64 host cpu.X86's flags are false and Go's own condition selects the fallback: the arm64
// aeshashbody (asm_arm64.s, a different schedule) is not reachable from this corpus and not ported.
// Consequence, matching Go on an AES host: TestMemHash32/64Equality SKIP, TestMemHashGlobalSeed/aes
// runs, and the Smhasher tests that Go enables only under AES run too.
//
// 64-bit only: this is hash64.go. hash32.go's schedule is not ported; a 32-bit host throws at first
// use rather than hashing with the wrong constants.
//
// SCOPE — exactly the four flat bodyless partials named in the first line. NOT here: getg (Q40/Q47),
// memmove, getfp, testSPWrite, memclrNoHeapPointers and every other flat stub in stubs.cs; the AES
// path (aeshashbody, initAlgAES); `fastrand`, which is not a runtime stub at all (rand_test.go's own
// pull of the push-renamed legacy_fastrand lands in the TEST assembly, and behind any forwarder it
// reaches rand() -> getg().m.chacha8); and the two header-reading adapters bytesHash/stringHash,
// which stay converted and are refused by rule 1 above until the header seam lands.

[module: go.GoManualConversion]

namespace go;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Aes = System.Runtime.Intrinsics.X86.Aes;
using System.Security.Cryptography;
using System.Threading;
using go.golib;
using @unsafe = go.unsafe_package;
using cpu = go.@internal.cpu_package;

partial class runtime_package {

// hash64.go's m5. The converted hash64.cs carries the same value as `m5`; it is spelled here so the
// bodies depend on nothing the converter emits.
private const ulong hashM5 = 0x1d8e4e27c47d124f;

private static readonly object s_hashKeyLock = new();
private static bool s_hashKeySeeded;

// Go's alginit non-AES branch: hashkey[i] = uintptr(bootstrapRand()). Performed at first use because
// schedinit never runs in the managed runtime; a key that is already non-zero is respected.
private static void ensureHashKey() {
    if (Volatile.Read(ref s_hashKeySeeded))
        return;

    lock (s_hashKeyLock) {
        if (s_hashKeySeeded)
            return;

        if (IntPtr.Size != 8)
            throw new PlatformNotSupportedException("runtime hash family: only the 64-bit schedule (hash64.go) is ported; a 32-bit host would need hash32.go's constants");

        bool allZero = true;
        for (int i = 0; i < 4; i++) {
            if (hashkey[i].Value != 0)
                allZero = false;
        }

        if (allZero) {
            Span<byte> random = stackalloc byte[32];
            RandomNumberGenerator.Fill(random);
            for (int i = 0; i < 4; i++)
                hashkey[i] = (nuint)BitConverter.ToUInt64(random.Slice(i * 8, 8));
        }

        Volatile.Write(ref s_hashKeySeeded, true);
    }
}

// Go's alginit, performed once at load: the AES branch when the CPU flags (after GODEBUG's cpu.*
// options) allow it, the fallback's hashkey otherwise. The fallback key is seeded either way, because
// the public fallback bodies below (GoMemhash and friends) are callable directly by guards.
[ModuleInitializer]
internal static void initHashAlgorithm() {
    if ((GOARCH == "386"u8 || GOARCH == "amd64"u8) && cpu.X86.HasAES && cpu.X86.HasSSSE3 && cpu.X86.HasSSE41 &&
        System.Runtime.Intrinsics.X86.Aes.IsSupported && System.Runtime.Intrinsics.X86.Sse41.IsSupported) {
        Span<byte> random = stackalloc byte[128];
        RandomNumberGenerator.Fill(random);
        for (int i = 0; i < 128; i++)
            aeskeysched[i] = random[i];
        useAeshash = true;
    }

    ensureHashKey();
}

// ---- aeshash: asm_amd64.s's aeshashbody, memhash32 and memhash64 over System.Runtime.Intrinsics ----

private static Vector128<byte> aesKey(ReadOnlySpan<byte> schedule, int block) =>
    Vector128.Create(schedule.Slice(block * 16, 16));

private static Vector128<byte> aesLoad(ReadOnlySpan<byte> data, int offset) =>
    Vector128.Create(data.Slice(offset, 16));

// AESENC X, X: one round keyed by the state itself
private static Vector128<byte> aesSelf(Vector128<byte> x) => Aes.Encrypt(x, x);

private static ulong aesLow(Vector128<byte> x) => x.AsUInt64().ToScalar();

/// <summary>
/// Go's <c>aeshashbody</c> (asm_amd64.s) over <paramref name="data"/> with <paramref name="seed"/> and
/// the 128-byte key <paramref name="schedule"/>: the hash <c>runtime.memhash</c> and
/// <c>runtime.strhash</c> answer on an AES host. Public so a guard can compare it against Go's own
/// output for a fixed schedule.
/// </summary>
public static ulong GoAeshash(ReadOnlySpan<byte> data, ulong seed, ReadOnlySpan<byte> schedule) {
    int n = data.Length;

    // MOVQ BX, X0; PINSRW $4, CX, X0; PSHUFHW $0, X0, X0: the seed, then the length's low 16 bits
    // four times in the high quadword
    ulong lengthWords = (ulong)(ushort)n * 0x0001000100010001UL;
    Vector128<byte> x1 = Vector128.Create(seed, lengthWords).AsByte();
    Vector128<byte> x0 = aesSelf(x1 ^ aesKey(schedule, 0));

    if (n == 0)
        return aesLow(aesSelf(x0));

    if (n <= 16) {
        Vector128<byte> block;

        if (n == 16) {
            block = aesLoad(data, 0);
        } else {
            // The masked (or page-end shuffled) load: the data bytes, zero above
            Span<byte> padded = stackalloc byte[16];
            data.CopyTo(padded);
            block = Vector128.Create((ReadOnlySpan<byte>)padded);
        }

        block ^= x0;
        return aesLow(aesSelf(aesSelf(aesSelf(block))));
    }

    if (n <= 32) {
        x1 = aesSelf(x1 ^ aesKey(schedule, 1));
        Vector128<byte> x2 = aesLoad(data, 0) ^ x0;
        Vector128<byte> x3 = aesLoad(data, n - 16) ^ x1;
        x2 = aesSelf(aesSelf(aesSelf(x2)));
        x3 = aesSelf(aesSelf(aesSelf(x3)));
        return aesLow(x2 ^ x3);
    }

    if (n <= 64) {
        Vector128<byte> s1 = aesSelf(x1 ^ aesKey(schedule, 1));
        Vector128<byte> s2 = aesSelf(x1 ^ aesKey(schedule, 2));
        Vector128<byte> s3 = aesSelf(x1 ^ aesKey(schedule, 3));
        Vector128<byte> x4 = aesSelf(aesSelf(aesSelf(aesLoad(data, 0) ^ x0)));
        Vector128<byte> x5 = aesSelf(aesSelf(aesSelf(aesLoad(data, 16) ^ s1)));
        Vector128<byte> x6 = aesSelf(aesSelf(aesSelf(aesLoad(data, n - 32) ^ s2)));
        Vector128<byte> x7 = aesSelf(aesSelf(aesSelf(aesLoad(data, n - 16) ^ s3)));
        return aesLow((x4 ^ x6) ^ (x5 ^ x7));
    }

    // 65 and up: seven more seeds from the unscrambled seed, X0 unchanged
    Span<Vector128<byte>> seeds = stackalloc Vector128<byte>[8];
    seeds[0] = x0;

    for (int i = 1; i < 8; i++)
        seeds[i] = aesSelf(x1 ^ aesKey(schedule, i));

    Span<Vector128<byte>> state = stackalloc Vector128<byte>[8];

    if (n <= 128) {
        for (int i = 0; i < 4; i++)
            state[i] = aesLoad(data, 16 * i) ^ seeds[i];

        for (int i = 4; i < 8; i++)
            state[i] = aesLoad(data, n - 64 + 16 * (i - 4)) ^ seeds[i];
    } else {
        // Start with the last (possibly overlapping) 128-byte block, then fold in every full block
        // from the front, (n - 1) / 128 of them
        for (int i = 0; i < 8; i++)
            state[i] = aesLoad(data, n - 128 + 16 * i) ^ seeds[i];

        int blocks = (n - 1) >> 7;

        for (int offset = 0; blocks != 0; blocks--, offset += 128) {
            for (int i = 0; i < 8; i++)
                state[i] = Aes.Encrypt(aesSelf(state[i]), aesLoad(data, offset + 16 * i));
        }
    }

    for (int i = 0; i < 8; i++)
        state[i] = aesSelf(aesSelf(aesSelf(state[i])));

    Vector128<byte> a = state[0] ^ state[4], b = state[1] ^ state[5], c = state[2] ^ state[6], d = state[3] ^ state[7];
    return aesLow((a ^ c) ^ (b ^ d));
}

/// <summary>Go's AES <c>memhash32</c>: the four data bytes in the seed's third dword, three rounds keyed
/// by the schedule's first three blocks.</summary>
public static ulong GoAeshash32(uint data, ulong seed, ReadOnlySpan<byte> schedule) {
    Vector128<byte> x = Vector128.Create(seed, (ulong)data).AsByte();
    x = Aes.Encrypt(x, aesKey(schedule, 0));
    x = Aes.Encrypt(x, aesKey(schedule, 1));
    return aesLow(Aes.Encrypt(x, aesKey(schedule, 2)));
}

/// <summary>Go's AES <c>memhash64</c>: the eight data bytes in the seed's high quadword, three rounds
/// keyed by the schedule's first three blocks.</summary>
public static ulong GoAeshash64(ulong data, ulong seed, ReadOnlySpan<byte> schedule) {
    Vector128<byte> x = Vector128.Create(seed, data).AsByte();
    x = Aes.Encrypt(x, aesKey(schedule, 0));
    x = Aes.Encrypt(x, aesKey(schedule, 1));
    return aesLow(Aes.Encrypt(x, aesKey(schedule, 2)));
}

/// <summary>Whether this process selected the AES hash (Go's <c>useAeshash</c>).</summary>
public static bool GoUsesAeshash => useAeshash;

private static ReadOnlySpan<byte> aesSchedule => aeskeysched.ToSpan();

// ---- the arithmetic: hash64.go verbatim over a byte span ----

private static ulong hashMix(ulong a, ulong b) {
    ulong hi = Math.BigMul(a, b, out ulong lo);
    return hi ^ lo;
}

// readUnaligned32/64 read with NATIVE endianness; every go2cs target is little-endian, and
// MemoryMarshal.Read is the native read.
private static ulong hashR4(ReadOnlySpan<byte> p) => MemoryMarshal.Read<uint>(p);
private static ulong hashR8(ReadOnlySpan<byte> p) => MemoryMarshal.Read<ulong>(p);

/// <summary>
/// Go's <c>memhashFallback</c> (hash64.go) over the bytes of <paramref name="data"/> with
/// <paramref name="seed"/>: the hash <c>runtime.memhash(p, seed, len(data))</c> answers for a
/// pointer to those bytes. Public so a guard can state the contract without minting a pointer.
/// </summary>
public static ulong GoMemhash(ReadOnlySpan<byte> data, ulong seed) {
    ensureHashKey();
    ulong k0 = hashkey[0].Value, k1 = hashkey[1].Value, k2 = hashkey[2].Value, k3 = hashkey[3].Value;

    ulong s = (ulong)data.Length;
    ulong a = 0, b = 0;
    seed ^= k0;

    if (s == 0) {
        return seed;
    } else if (s < 4) {
        a = data[0];
        a |= (ulong)data[(int)(s >> 1)] << 8;
        a |= (ulong)data[(int)(s - 1)] << 16;
    } else if (s == 4) {
        a = hashR4(data);
        b = a;
    } else if (s < 8) {
        a = hashR4(data);
        b = hashR4(data.Slice((int)(s - 4)));
    } else if (s == 8) {
        a = hashR8(data);
        b = a;
    } else if (s <= 16) {
        a = hashR8(data);
        b = hashR8(data.Slice((int)(s - 8)));
    } else {
        ulong l = s;
        ReadOnlySpan<byte> p = data;
        if (l > 48) {
            ulong seed1 = seed;
            ulong seed2 = seed;
            for (; l > 48; l -= 48) {
                seed = hashMix(hashR8(p) ^ k1, hashR8(p.Slice(8)) ^ seed);
                seed1 = hashMix(hashR8(p.Slice(16)) ^ k2, hashR8(p.Slice(24)) ^ seed1);
                seed2 = hashMix(hashR8(p.Slice(32)) ^ k3, hashR8(p.Slice(40)) ^ seed2);
                p = p.Slice(48);
            }
            seed ^= seed1 ^ seed2;
        }
        for (; l > 16; l -= 16) {
            seed = hashMix(hashR8(p) ^ k1, hashR8(p.Slice(8)) ^ seed);
            p = p.Slice(16);
        }
        // Go reads add(p, l-16) / add(p, l-8) with p already advanced by s-l bytes: that is the LAST
        // sixteen bytes of the whole input, overlapping the loop's tail whenever l < 16 (a wrapped
        // negative offset in span terms -- the guard's first red, 2026-09-04).
        a = hashR8(data.Slice((int)(s - 16)));
        b = hashR8(data.Slice((int)(s - 8)));
    }

    return hashMix(hashM5 ^ s, hashMix(a ^ k1, b ^ seed));
}

/// <summary>Go's <c>memhash32Fallback</c>: the hash of exactly four bytes.</summary>
public static ulong GoMemhash32(ReadOnlySpan<byte> four, ulong seed) {
    if (four.Length != 4)
        throw panic($"runtime.memhash32: {four.Length} bytes where the contract is 4");
    ensureHashKey();
    ulong a = hashR4(four);
    return hashMix(hashM5 ^ 4, hashMix(a ^ hashkey[1].Value, (a ^ seed) ^ hashkey[0].Value));
}

/// <summary>Go's <c>memhash64Fallback</c>: the hash of exactly eight bytes.</summary>
public static ulong GoMemhash64(ReadOnlySpan<byte> eight, ulong seed) {
    if (eight.Length != 8)
        throw panic($"runtime.memhash64: {eight.Length} bytes where the contract is 8");
    ensureHashKey();
    ulong a = hashR8(eight);
    return hashMix(hashM5 ^ 8, hashMix(a ^ hashkey[1].Value, (a ^ seed) ^ hashkey[0].Value));
}

/// <summary>The four hash-key words after seeding (a copy): the guard's evidence that the
/// alginit branch ran here.</summary>
public static ulong[] GoHashKey() {
    ensureHashKey();
    return [hashkey[0].Value, hashkey[1].Value, hashkey[2].Value, hashkey[3].Value];
}

// ---- recovery: the bytes a pointer names ----

// Rule 1: the argument's REAL method table. A header reinterpretation hands these bodies a reference
// whose static type is Pointer and whose object is a byte[] (or another managed array); GetType()
// reads the method table and nothing else, so it answers safely where any field read would fault.
private static void refuseNonPointer(string caller, @unsafe.Pointer p) {
    if (p is null)
        return;
    Type actual = ((object)p).GetType();
    if (!actual.IsAssignableTo(typeof(@unsafe.Pointer)))
        throw panic($"runtime.{caller}: the pointer argument is not an unsafe.Pointer but a {actual.FullName} reference — a slice/string HEADER read through a reinterpretation golib cannot alias (bytesHash's (*slice)(unsafe.Pointer(&b)), strhashFallback's (*stringStruct)(a)); dereferencing it would fault natively. The header seam is a separate increment.");
}

private static object? recoverReferent(@unsafe.Pointer p) {
    return p.RetainedSource ?? ManagedPointerTokens.Resolve(p.Value.Value);
}

private static ReadOnlySpan<byte> scalarBytes<T>(string caller, ж<T> box, ulong size) where T : unmanaged {
    Span<byte> all = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref box.Value, 1));
    if (size > (ulong)all.Length)
        throw panic($"runtime.{caller}: {size} bytes asked of a {typeof(T).Name} box that holds {all.Length}");
    return all.Slice(0, (int)size);
}

// The bytes `size` long that `p` names, or a panic naming `caller` and why.
private static ReadOnlySpan<byte> referentBytes(string caller, @unsafe.Pointer p, ulong size) {
    refuseNonPointer(caller, p);

    if (size > int.MaxValue)
        throw panic($"runtime.{caller}: a {size}-byte hash is outside what a managed span can address");

    // Go reads nothing for a zero size, whatever the pointer names (computeHash hashes a pointer to
    // an empty struct{} with size 0)
    if (size == 0)
        return ReadOnlySpan<byte>.Empty;

    if (p is null || p.IsNull)
        throw panic($"runtime.{caller}: nil pointer with a non-zero size ({size})");

    object? referent = recoverReferent(p);
    if (referent is null)
        throw panic($"runtime.{caller}: the pointer carries no recoverable managed referent (a raw address, or a reference-bearing box the provenance record cannot resolve through the uintptr bridge before Q44)");

    switch (referent) {
        case ж<byte> elem: {
            ref byte first = ref elem.Value;
            if (elem.PinnableStorage is byte[] backing) {
                nint offset = Unsafe.ByteOffset(ref MemoryMarshal.GetArrayDataReference(backing), ref first);
                if (offset < 0 || (ulong)offset + size > (ulong)backing.Length)
                    throw panic($"runtime.{caller}: {size} bytes from element {offset} of a {backing.Length}-byte backing reads past its end");
                return MemoryMarshal.CreateReadOnlySpan(ref first, (int)size);
            }
            if (size > 1)
                throw panic($"runtime.{caller}: {size} bytes asked of a lone byte box");
            return MemoryMarshal.CreateReadOnlySpan(ref first, (int)size);
        }
        case ж<array<byte>> arr: {
            Span<byte> all = arr.Value.ToSpan();
            if (size > (ulong)all.Length)
                throw panic($"runtime.{caller}: {size} bytes asked of a [{all.Length}]byte");
            return all.Slice(0, (int)size);
        }
        case ж<@string>:
            throw panic($"runtime.{caller}: a string HEADER (unsafe.Pointer(&s)); Go's memhash over a string header hashes the (ptr, len) words, which the managed string does not have — string CONTENT is strhash's contract");
        case ж<sbyte> b: return scalarBytes(caller, b, size);
        case ж<ushort> b: return scalarBytes(caller, b, size);
        case ж<short> b: return scalarBytes(caller, b, size);
        case ж<uint> b: return scalarBytes(caller, b, size);
        case ж<int> b: return scalarBytes(caller, b, size);
        case ж<ulong> b: return scalarBytes(caller, b, size);
        case ж<long> b: return scalarBytes(caller, b, size);
        case ж<nuint> b: return scalarBytes(caller, b, size);
        case ж<nint> b: return scalarBytes(caller, b, size);
        case ж<uintptr> b: return scalarBytes(caller, b, size);
        case ж<float> b: return scalarBytes(caller, b, size);
        case ж<double> b: return scalarBytes(caller, b, size);
        case ж<bool> b: return scalarBytes(caller, b, size);
        default:
            throw panic($"runtime.{caller}: no byte view of a {referent.GetType().Name} referent (only byte element boxes, [N]byte boxes and unmanaged scalar boxes are admitted; a struct with references has no Go memory image here)");
    }
}

/// <summary><c>runtime.memhash(p, seed, size)</c> for a pointer minted by the emitted code: the
/// referent is recovered per the file header and its bytes hashed with <see cref="GoMemhash"/>.</summary>
public static ulong GoMemhashPointer(@unsafe.Pointer p, ulong seed, ulong size) {
    return GoMemhash(referentBytes("memhash", p, size), seed);
}

/// <summary><c>runtime.memhash32(p, seed)</c> for a pointer to four bytes.</summary>
public static ulong GoMemhash32Pointer(@unsafe.Pointer p, ulong seed) {
    return GoMemhash32(referentBytes("memhash32", p, 4), seed);
}

/// <summary><c>runtime.memhash64(p, seed)</c> for a pointer to eight bytes.</summary>
public static ulong GoMemhash64Pointer(@unsafe.Pointer p, ulong seed) {
    return GoMemhash64(referentBytes("memhash64", p, 8), seed);
}

/// <summary><c>runtime.strhash(p, seed)</c> for a pointer to a string: Go's strhashFallback hashes
/// the string's content (x.str, x.len), so the referent must be a <c>@string</c> box.</summary>
public static ulong GoStrhashPointer(@unsafe.Pointer p, ulong seed) {
    return GoMemhash(strhashContent(p), seed);
}

// The content of the string a strhash pointer names, or a panic naming why not.
private static ReadOnlySpan<byte> strhashContent(@unsafe.Pointer p) {
    refuseNonPointer("strhash", p);

    if (p is null || p.IsNull)
        throw panic("runtime.strhash: nil string pointer");

    object? referent = recoverReferent(p);
    if (referent is ж<@string> str)
        return str.Value.ToSpan();

    if (referent is null)
        throw panic("runtime.strhash: the pointer carries no recoverable managed referent — a @string box is reference-bearing, so the provenance record cannot resolve it through the uintptr bridge the emitted stringHash uses (SUB-Q42's class, Q44's fix); stringHash stays red until the header seam or Q44 lands");

    throw panic($"runtime.strhash: the referent is a {referent.GetType().Name}, not a string box");
}

// The four dispatchers, as asm_amd64.s's: aeshashbody when useAeshash is set, the fallback otherwise.

internal static partial uintptr memhash(@unsafe.Pointer Δp, uintptr h, uintptr s) {
    if (useAeshash)
        return (nuint)GoAeshash(referentBytes("memhash", Δp, s.Value), h.Value, aesSchedule);

    return (nuint)GoMemhashPointer(Δp, h.Value, s.Value);
}

internal static partial uintptr memhash32(@unsafe.Pointer Δp, uintptr h) {
    if (useAeshash)
        return (nuint)GoAeshash32(MemoryMarshal.Read<uint>(referentBytes("memhash32", Δp, 4)), h.Value, aesSchedule);

    return (nuint)GoMemhash32Pointer(Δp, h.Value);
}

internal static partial uintptr memhash64(@unsafe.Pointer Δp, uintptr h) {
    if (useAeshash)
        return (nuint)GoAeshash64(MemoryMarshal.Read<ulong>(referentBytes("memhash64", Δp, 8)), h.Value, aesSchedule);

    return (nuint)GoMemhash64Pointer(Δp, h.Value);
}

internal static partial uintptr strhash(@unsafe.Pointer Δp, uintptr h) {
    if (useAeshash)
        return (nuint)GoAeshash(strhashContent(Δp), h.Value, aesSchedule);

    return (nuint)GoStrhashPointer(Δp, h.Value);
}

} // end runtime_package
