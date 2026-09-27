// MintedAddressSinkTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// A number this runtime MINTS in place of an address (bit 63 set) names no memory. Dereferenced
/// through a native box or a native slice view, it is refused as Go's nil-dereference panic on every
/// platform. Before the refusal, the read reached the hardware: a FATAL AccessViolation on Windows,
/// and a caught NullReferenceException on linux only for the non-canonical bands
/// (ManagedPointerTokens.NamesNoUserMemory has the measured matrix).
/// </summary>
/// <remarks>
/// One arm per band, as ruled (ledger f8f73cd0a9): a caller token, an unregistered order token, a
/// DEAD order token and a synthetic function PC. Each arm first proves its band's precondition, so a
/// green arm cannot come from a number that resolved to a box, and then shows the conversion is still
/// admitted: a native box over such a number is a deliberate CARRIER (the uintptr round-trip is
/// exact), and only the dereference is refused. Red first: with the three golib hunks withheld, each
/// arm run alone ended the Windows test host (AccessViolation), and on linux threw
/// NullReferenceException where the arm requires the Go panic.
/// </remarks>
[TestClass]
public class MintedAddressSinkTests
{
    // runtime.Caller's call-site band (runtime/managed_impl.cs s_callerSpanBase), one byte before the
    // token of span 15. This is exactly the number runtime's TestFunctionAlignmentTraceback read.
    private const ulong CallerTokenByteBefore = 0x8000_8000_0000_F7FEUL;

    // A tagged order-token shape (bit 63 set, bit 47 clear) that no box has ever held.
    private const ulong UnregisteredOrderToken = (1UL << 63) | (0x1234UL << 32);

    private struct RefBearing
    {
        public string S;
    }

    private static void AssertRefusedAsNilDereference(nuint number)
    {
        ж<byte> p = (ж<byte>)(uintptr)number;
        Assert.AreEqual(number, (nuint)(uintptr)p, "the conversion is admitted and the carrier round-trips exactly");

        PanicException read = Assert.ThrowsException<PanicException>(() => ~p);
        StringAssert.Contains(read.Message, "nil pointer dereference");

        ж<nuint> word = (ж<nuint>)(uintptr)number;
        PanicException wordRead = Assert.ThrowsException<PanicException>(() => word.ReadPointerWord());
        StringAssert.Contains(wordRead.Message, "nil pointer dereference");
    }

    [TestMethod]
    public void ACallerTokenIsRefusedAsANilDereference()
    {
        nuint number = (nuint)CallerTokenByteBefore;
        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(number), "the caller band is untagged (bit 47 set)");
        Assert.IsNull(ManagedPointerTokens.Resolve(number));

        AssertRefusedAsNilDereference(number);
    }

    [TestMethod]
    public void AnUnregisteredOrderTokenIsRefusedAsANilDereference()
    {
        nuint number = (nuint)UnregisteredOrderToken;
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(number));
        Assert.IsNull(ManagedPointerTokens.Resolve(number), "no box holds this token");

        AssertRefusedAsNilDereference(number);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nuint MintATokenAndLetItsBoxDie()
    {
        builtin.heap(new RefBearing { S = "x" }, out ж<RefBearing> box);
        nuint token = (nuint)(uintptr)box;
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(token), "a reference-bearing pointee answers an order token");
        return token;
    }

    [TestMethod]
    public void ADeadOrderTokenIsRefusedAsANilDereference()
    {
        nuint token = MintATokenAndLetItsBoxDie();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        if (ManagedPointerTokens.Resolve(token) is not null)
            Assert.Inconclusive("the token's box survived two collections, so the dead-token precondition does not hold here");

        AssertRefusedAsNilDereference(token);
    }

    [TestMethod]
    public void ASyntheticFunctionPCIsRefusedAsANilDereference()
    {
        Action target = ASyntheticFunctionPCIsRefusedAsANilDereference;
        nuint entry = GoSyntheticPC.Of(target);
        nuint lastByte = entry + 0xFFF;

        Assert.IsNotNull(GoSyntheticPC.Resolve(lastByte), "the last byte of the span still names the function");
        Assert.IsTrue((ulong)entry >= 0xFFFF_8000_0000_0000UL, "a canonical kernel-half address: fatal on BOTH OSes before");

        AssertRefusedAsNilDereference(lastByte);
    }

    [TestMethod]
    public void ANativeSliceViewOverAMintedBaseRefusesItsReads()
    {
        go.slice<byte> window = go.slice<byte>.OverNativeMemory((nuint)CallerTokenByteBefore, 4);

        StringAssert.Contains(Assert.ThrowsException<PanicException>(() => window[0]).Message, "nil pointer dereference");

        // Both enumerators read inline rather than through the indexer: the (index, value) one a Go range
        // lowers to, and the IEnumerable<T> one (ToString, LINQ).
        StringAssert.Contains(Assert.ThrowsException<PanicException>(() => { foreach ((nint _, byte _) in window) { } }).Message, "nil pointer dereference");
        StringAssert.Contains(Assert.ThrowsException<PanicException>(() => { foreach (byte _ in (System.Collections.Generic.IEnumerable<byte>)window) { } }).Message, "nil pointer dereference");

        // An EMPTY window reads nothing, and Go does not fault on one.
        go.slice<byte> empty = go.slice<byte>.OverNativeMemory((nuint)CallerTokenByteBefore, 0);
        foreach ((nint _, byte _) in empty)
            Assert.Fail("an empty window has no elements");
    }

    // P2's review arms (ledger 7eb8b61ab2): the two word WRITES, the bulk span, and the one path that
    // must NOT be refused.

    [TestMethod]
    public void TheWordWritesOverAMintedNumberAreRefusedToo()
    {
        ж<nuint> word = (ж<nuint>)(uintptr)(nuint)CallerTokenByteBefore;

        StringAssert.Contains(Assert.ThrowsException<PanicException>(() => word.ExchangePointerWord(1)).Message, "nil pointer dereference");
        StringAssert.Contains(Assert.ThrowsException<PanicException>(() => word.CompareExchangePointerWord(0, 1)).Message, "nil pointer dereference");
    }

    [TestMethod]
    public void ANonEmptySpanOverAMintedBaseIsRefused()
    {
        go.slice<byte> window = go.slice<byte>.OverNativeMemory((nuint)CallerTokenByteBefore, 4);

        StringAssert.Contains(Assert.ThrowsException<PanicException>(() => { _ = window.ToSpan(); }).Message, "nil pointer dereference");
        Assert.AreEqual(0, go.slice<byte>.OverNativeMemory((nuint)CallerTokenByteBefore, 0).ToSpan().Length, "an empty window's span is not refused");
    }

    [TestMethod]
    public void FormingAnElementAddressOverAMintedBaseIsNotRefused()
    {
        // The CONTROL: `&s[i]` forms a pointer and reads nothing, and Go does not fault on it. It must
        // answer the base plus the offset, exactly, and refuse nothing.
        go.slice<ulong> window = go.slice<ulong>.OverNativeMemory((nuint)CallerTokenByteBefore, 4);

        Assert.AreEqual((nuint)CallerTokenByteBefore, window.NativeElementAddress(0));
        Assert.AreEqual((nuint)CallerTokenByteBefore + 3 * sizeof(ulong), window.NativeElementAddress(3));
    }
}
