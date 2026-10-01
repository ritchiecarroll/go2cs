using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using alias = go.crypto.@internal.fips140.alias_package;

namespace GolibTests;

// crypto/internal/alias is the corpus's memory-ALIASING predicate, and Go writes it by ORDERING
// element addresses:
//
//     func AnyOverlap(x, y []byte) bool {
//         return len(x) > 0 && len(y) > 0 &&
//             uintptr(unsafe.Pointer(&x[0])) <= uintptr(unsafe.Pointer(&y[len(y)-1])) &&
//             uintptr(unsafe.Pointer(&y[0])) <= uintptr(unsafe.Pointer(&x[len(x)-1]))
//     }
//
// The converter emits that literally, as four `(uintptr)Ꮡ(…)` conversions, and the package's banked
// row is ONE verdict — so nothing in the corpus states what these two functions must answer. This
// file states it, and it exists because a net/http sweep on the i7 died mid-stream inside
// crypto/tls's (*Conn).Read with the panic text `crypto/aes: invalid buffer overlap`, which only
// crypto/aes's `alias.InexactOverlap(dst[:BlockSize], src[:BlockSize])` guard can raise. That path
// is NEW on the corpus: since internal/cpu began reporting real x86 features (acc79ab48), crypto/tls's
// `hasGCMAsmAMD64 = cpu.X86.HasAES && cpu.X86.HasPCLMULQDQ` is true, TLS negotiates AES-GCM where it
// used to pick ChaCha20-Poly1305, and the pure-Go GCM counter mode calls Encrypt(mask[:], counter[:])
// on two distinct [16]byte buffers once per block.
//
// ────────────────────────────────────────────────────────────────────────────────────────────────
// TWO CANDIDATE MECHANISMS WERE MEASURED AND BOTH ARE FALSE. Recorded here, at the gate, so the next
// reader meets the result instead of repeating the attempt (i7, Debug, net10.0, 2026-09-03).
//
// (1) "ORDER-TOKEN COLLISION." The proposal was that `(uintptr)Ꮡ(x, i)` yields golib's
//     PointerOrderToken — `AllocationBase(RuntimeHelpers.GetHashCode(storage)) + index`, i.e. a
//     26-bit identity hash shifted into the high half — so two distinct arrays sharing a hash would
//     share a 4 GiB span, interleave, and answer AnyOverlap true.
//
//     FALSE, and not marginally: the uintptr operator on an element box never consults the token at
//     all. `ж<T>`'s `implicit operator uintptr` takes the element box's non-native, non-fixed-array
//     path — EnsureStableAddress() then `fixed (void* ptr = &value.Value)` — so it PINS the canonical
//     backing array and returns the REAL machine address. Measured on one 16-byte buffer:
//
//         element 0  -> 0x0000013713188418      element 15 -> 0x0000013713188427   (delta 15)
//         that buffer's PointerOrderToken       -> 0x036C7B3C00000000
//
//     The premise is separately true and still does not reach the predicate: the identity hash IS
//     26 bits (OR-mask over 400,000 arrays = 0x03FFFFFF) and DOES collide (1,124 collisions in
//     400,000 live arrays, ~2^13 as predicted). A colliding pair was constructed, its two boxes were
//     confirmed to report the SAME token (0x02C2C76600000000), and AnyOverlap over that pair still
//     answered FALSE — because it is comparing addresses, not tokens. `TokenCollisionDoesNotReachThePredicate`
//     below is that falsification kept as a live assertion.
//
// (2) "PIN/GC RACE." The fallback proposal was that the four takes are four SEPARATE boxes, each
//     pinning only for its own lifetime, so a compacting collection between two takes leaves the
//     four numbers describing different heap layouts. Architecturally that is the right worry, and
//     it did not reproduce in any construction tried:
//
//         distinct 16-byte buffers, AnyOverlap in a loop
//           against a thread hammering GC.Collect(2, Forced, blocking, compacting)
//           + WaitForPendingFinalizers            31,000 calls / 52,833 gen2 GCs -> 0 false positives
//         real AES-GCM Seal+Open (aes.NewCipher -> cipher.NewGCM), 4 threads,
//           same collector thread                  1,600 ops / 41,824 gen2 GCs   -> 0 panics
//         the crypto/tls record shape: the aes SELF-alias Encrypt(key[:], key[:]),
//           a fresh NewGCM per iteration, and in-place Open(payload[:0], …, payload, …),
//           same collector thread                 30,000 ops / 123,051 gen2 GCs  -> 0 panics
//         a buffer's element-0 address across 8 forced compacting gen2 collections
//           with finalizers drained                                              -> DID NOT MOVE
//
//     200,000+ address takes under 200,000+ forced compacting collections produced not one wrong
//     answer. So the race is not demonstrated, and "the emitted form is unsound" is a reading of the
//     code that this instrument could not turn into a failure.
//
// THE MECHANISM IS THEREFORE UNESTABLISHED. What is established is the population of things it is
// NOT, and the contract below, which nothing else in the corpus asserts. A remedy was drafted for
// the address route — golib `slice<T>.Overlaps` (canonical backing identity + absolute index-range
// intersection) with `crypto/internal/alias.AnyOverlap` and `slices.overlaps` displaced onto it via
// manualConversionFuncs — and HELD unbanked under the non-reproducible-motivating-failure rule: it
// is a converter + golib change whose only demonstrated failure could not be reproduced, and a fix
// with no red to turn green is speculative machinery. Whoever takes the root next should start from
// the eliminations above, and should note that `slices.overlaps` carries a SECOND, GC-independent
// defect the crypto sibling cannot have: it is generic, and when E is a Go ARRAY type
// `Ꮡ(a, i).Value` is an `array<T>` — an `IArray` that is not an `ISlice` — so the uintptr operator
// takes its fixed-array branch and returns the address of that element's OWN inner storage rather
// than of its slot in the outer backing. That one is read from the code and is NOT measured here.
// ────────────────────────────────────────────────────────────────────────────────────────────────
[TestClass]
public class AliasOverlapTests
{
    private static slice<byte> Window(byte[] backing, int low, int high) =>
        backing.slice(low, high);

    [TestMethod]
    public void DistinctStorageNeverOverlaps()
    {
        // Two allocations cannot share memory, whatever their addresses happen to be. This is the
        // direction crypto/aes turns into `panic("crypto/aes: invalid buffer overlap")`, and the
        // direction the observed net/http failure would have to travel.
        byte[] a = new byte[16];
        byte[] b = new byte[16];

        slice<byte> x = a.slice();
        slice<byte> y = b.slice();

        Assert.IsFalse(alias.AnyOverlap(x, y), "two distinct backing arrays share no memory");
        Assert.IsFalse(alias.InexactOverlap(x, y), "two distinct backing arrays cannot inexactly overlap");

        GC.KeepAlive(a);
        GC.KeepAlive(b);
    }

    [TestMethod]
    public void SharedStorageWithIntersectingRangesOverlaps()
    {
        // The TRUE case, so a future structural answer cannot collapse into "never overlaps" — the
        // failure mode that would make slices.Insert/Replace take an in-place copy over storage that
        // really does alias, with nothing to report it.
        byte[] backing = new byte[32];

        slice<byte> x = Window(backing, 0, 20);
        slice<byte> y = Window(backing, 10, 32);

        Assert.IsTrue(alias.AnyOverlap(x, y), "windows [0,20) and [10,32) of one array share elements 10..19");
        Assert.IsTrue(alias.AnyOverlap(y, x), "AnyOverlap is symmetric");
        Assert.IsTrue(alias.InexactOverlap(x, y), "they overlap and do not start at the same element");

        GC.KeepAlive(backing);
    }

    [TestMethod]
    public void SharedStorageWithDisjointRangesDoesNotOverlap()
    {
        // Adjacent-but-disjoint windows of ONE array: the case an index-range answer must get right
        // and a same-array shortcut would get wrong.
        byte[] backing = new byte[32];

        slice<byte> x = Window(backing, 0, 16);
        slice<byte> y = Window(backing, 16, 32);

        Assert.IsFalse(alias.AnyOverlap(x, y), "windows [0,16) and [16,32) touch but share no element");
        Assert.IsFalse(alias.InexactOverlap(x, y), "no overlap means no inexact overlap");

        GC.KeepAlive(backing);
    }

    [TestMethod]
    public void ExactAliasIsAnyOverlapButNotInexactOverlap()
    {
        // THE contract crypto/cipher and crypto/aes lean on. Go permits in-place work — gcm.Open is
        // called as Open(payload[:0], nonce, payload, ad) from crypto/tls/conn.go, and newGCM itself
        // runs cipher.Encrypt(key[:], key[:]) — and it is InexactOverlap's `&x[0] == &y[0]` early-out
        // that permits it. In the emission that early-out is `Ꮡ(x, 0) == Ꮡ(y, 0)`, i.e. ж pointer
        // equality, which is ALREADY structural: ElemRefBox.Equals compares canonical storage
        // identity and absolute index and never an address. If that ever regressed to an address
        // comparison, every AES-GCM record would panic.
        byte[] backing = new byte[32];

        slice<byte> x = Window(backing, 4, 20);
        slice<byte> y = Window(backing, 4, 20);

        Assert.IsTrue(alias.AnyOverlap(x, y), "a window overlaps itself");
        Assert.IsFalse(alias.InexactOverlap(x, y), "windows starting at the same element alias EXACTLY — Go permits it");

        // The same relation through two different route to one element: the whole-array window and a
        // sub-window that starts where it starts.
        slice<byte> whole = backing.slice();
        slice<byte> prefix = Window(backing, 0, 8);

        Assert.IsFalse(alias.InexactOverlap(whole, prefix), "a prefix starts at the same element as the whole window");

        GC.KeepAlive(backing);
    }

    [TestMethod]
    public void EmptyWindowsOverlapNothing()
    {
        // Go ignores the memory beyond the length: both predicates early-out on a zero-length side,
        // which is also what keeps the nil slice out of the address arithmetic.
        byte[] backing = new byte[16];

        slice<byte> empty = Window(backing, 4, 4);
        slice<byte> whole = backing.slice();
        slice<byte> nil = default;

        Assert.IsFalse(alias.AnyOverlap(empty, whole), "a zero-length window names no memory");
        Assert.IsFalse(alias.AnyOverlap(whole, empty), "…in either argument position");
        Assert.IsFalse(alias.AnyOverlap(nil, whole), "the nil slice names no memory");
        Assert.IsFalse(alias.InexactOverlap(empty, whole), "…and cannot inexactly overlap either");

        GC.KeepAlive(backing);
    }

    [TestMethod]
    public void TokenCollisionDoesNotReachThePredicate()
    {
        // The falsification of candidate mechanism (1), kept as a live assertion rather than as
        // prose. Before option 3, two DISTINCT arrays that shared a CLR identity hash reported the SAME
        // PointerOrderToken span — golib lifted the 26-bit hash into the high 32 bits and carried the
        // element index below it, so a collision made two unrelated buffers indistinguishable to
        // any consumer that orders by token. AnyOverlap is not such a consumer: it compares pinned
        // addresses. If a future change ever routes it through the token, this goes red, which is
        // the point of keeping it.
        byte[]? first = null;
        byte[]? second = null;
        int orMask = 0;

        var seen = new Dictionary<int, byte[]>(1 << 19);

        for (int i = 0; i < 400_000 && first is null; i++)
        {
            byte[] candidate = new byte[16];
            int hash = RuntimeHelpers.GetHashCode(candidate);
            orMask |= hash;

            if (seen.TryGetValue(hash, out byte[]? prior))
            {
                first = prior;
                second = candidate;
            }
            else
            {
                seen[hash] = candidate;
            }
        }

        if (first is null || second is null)
        {
            // Not a silent pass: a runtime with a wider identity hash legitimately has no collision
            // at this population, and the falsification simply cannot be staged there.
            Assert.Inconclusive(
                $"no identity-hash collision in 400,000 live arrays (hash OR-mask 0x{orMask:X8}) — " +
                "this runtime's identity hash is too wide to stage the token-collision falsification");
            return;
        }

        slice<byte> x = first.slice();
        slice<byte> y = second.slice();

        // POSITIVE CONTROL: the pair has to be real before its consequence means anything.
        Assert.AreNotSame(first, second, "the colliding pair must be two DISTINCT arrays");

        // The token half of the collision is GONE since option 3 (ruling 2026-09-30): a token base is the
        // allocation's unique id, not its identity hash, and that shared token was the defect it fixed. So
        // the pair that shares an identity hash must now get two tokens; the falsification below stands.
        Assert.AreNotEqual(Ꮡ(x, 0).PointerOrderToken, Ꮡ(y, 0).PointerOrderToken,
            "two live arrays sharing an identity hash must still get two order tokens (option 3)");

        // The falsification itself.
        Assert.IsFalse(alias.AnyOverlap(x, y),
            "two distinct arrays sharing an order-token span still share no memory — AnyOverlap reads " +
            "pinned addresses, not tokens, so a token collision cannot be the mechanism behind the " +
            "observed 'crypto/aes: invalid buffer overlap'");
        Assert.IsFalse(alias.InexactOverlap(x, y), "…and neither can it be behind an inexact overlap");

        GC.KeepAlive(seen);
    }

    [TestMethod]
    public void ElementAddressConversionYieldsAnAddressNotAToken()
    {
        // The measurement that settled which of the two candidate mechanisms was even applicable,
        // kept so the next reader does not have to re-take it. `(uintptr)Ꮡ(s, i)` on an element box
        // pins the canonical backing and returns the real machine address: consecutive byte elements
        // are exactly one apart, which a token — index in the LOW bits over an identity-hash base —
        // also satisfies, so the discriminating assertion is that the value is NOT the token.
        byte[] backing = new byte[16];
        slice<byte> x = backing.slice();

        nuint first = (nuint)(uintptr)Ꮡ(x, 0);
        nuint last = (nuint)(uintptr)Ꮡ(x, 15);
        nuint token = Ꮡ(x, 0).PointerOrderToken;

        Assert.AreEqual((nuint)15, last - first,
            "element 15 of a byte window is 15 bytes past element 0");
        Assert.AreNotEqual(token, first,
            "the uintptr conversion of an element box is its PINNED ADDRESS, not its PointerOrderToken — " +
            "the token route (identity hash << 32) is not on this path at all");
        Assert.AreEqual((nuint)0, token & 0xFFFFFFFF,
            "element 0's token carries index 0 in its low half, which is what makes it distinguishable " +
            "from the address");

        GC.KeepAlive(backing);
    }
}
