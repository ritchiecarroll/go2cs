// SyntheticPCRegistryTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

// The naming rule under test reads a converted package's SHAPE — the class `<pkg>_package` in a
// namespace below `go` — so the fixtures have to be real classes of that shape rather than mocks.
// That is why this file uses block-scoped namespaces: a file-scoped one cannot declare a second.
namespace go.pcfixtures
{
    internal static class demo_package
    {
        internal static void Fn() { }

        internal static void Other() { }
    }

    // An INTERNAL test file compiles into the package itself, so Go names it `pcfixtures/demo`.
    internal static class demo_internal_test_package
    {
        internal static void Fn() { }
    }

    // An EXTERNAL test file is its own Go package, so the `_test` stays: `pcfixtures/demo_test`.
    internal static class demo_test_package
    {
        internal static void Fn() { }
    }
}

namespace go
{
    // A package at the root of the corpus namespace has no path prefix at all.
    internal static class rootdemo_package
    {
        internal static void Fn() { }
    }
}

namespace GolibTests
{
    /// <summary>
    /// Pins the synthetic-PC registry's four promises — unique, stable, span-resolvable, and never a
    /// real address — and the Go spelling its symbolizer answers with.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The registry exists because <c>internal/abi</c>'s <c>FuncPCABI0</c>/<c>FuncPCABIInternal</c>
    /// returned <c>default</c> for every function — a plausible, silent zero that made the corpus's
    /// missing PC→name mapping invisible, and made <c>internal/abi</c>'s own <c>TestFuncPC</c> pass by
    /// comparing 0 against 0. Design record: <c>docs/phase4/DESIGN-synthetic-pc-registry.md</c>.
    /// </para>
    /// <para>
    /// The span test is the load-bearing one. Callers do arithmetic on a PC and expect to stay inside
    /// the same function — runtime writes <c>FuncPCABI0(goexit) + sys.PCQuantum</c> and pprof writes
    /// <c>FuncPCABIInternal(lostProfileEvent) + 1</c> — so a registry that minted one value per
    /// function would resolve neither of the two expressions the corpus actually contains.
    /// </para>
    /// </remarks>
    [TestClass]
    public class SyntheticPCRegistryTests
    {
        // The documented minimum span per function. Pinned rather than read from the registry: the
        // guarantee is what callers rely on, so shrinking it is a contract change and should fail here.
        private const uint MinimumSpan = 4096;

        private static MethodInfo MethodOf(string type, string name) =>
            Type.GetType(type)!.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;

        private static MethodInfo DemoFn => MethodOf("go.pcfixtures.demo_package", "Fn");

        private static MethodInfo DemoOther => MethodOf("go.pcfixtures.demo_package", "Other");

        [TestMethod]
        public void TheSameFunctionAlwaysMintsTheSamePc()
        {
            nuint first = GoSyntheticPC.Of(DemoFn);
            nuint second = GoSyntheticPC.Of(DemoFn);

            Assert.AreEqual(first, second, "a function's PC must be stable for the life of the process");
        }

        [TestMethod]
        public void DifferentFunctionsNeverShareAPc()
        {
            Assert.AreNotEqual(GoSyntheticPC.Of(DemoFn), GoSyntheticPC.Of(DemoOther),
                "two functions sharing one PC would make every symbolized frame ambiguous");
        }

        [TestMethod]
        public void ADelegateAndItsMethodAgree()
        {
            // A PC identifies the FUNCTION, not the func value: a bare method group and a delegate
            // built over the same method are the same function and must answer the same PC. This is
            // the shape every call site has — `abi.FuncPCABI0(clone)` is a method group conversion.
            Action methodGroup = go.pcfixtures.demo_package.Fn;

            Assert.AreEqual(GoSyntheticPC.Of(DemoFn), GoSyntheticPC.Of(methodGroup),
                "a delegate over a method must answer that method's PC");
        }

        [TestMethod]
        public void ArithmeticOnAPcStaysInsideItsOwnFunction()
        {
            nuint pc = GoSyntheticPC.Of(DemoFn);
            MethodBase expected = DemoFn;

            foreach (uint offset in new uint[] { 0, 1, 8, 64, MinimumSpan - 1 })
            {
                Assert.AreEqual(expected, GoSyntheticPC.Resolve(pc + offset),
                    $"PC + {offset} must still resolve to the function it was minted for");
            }
        }

        [TestMethod]
        public void APcOutsideTheRangeResolvesToNothing()
        {
            // The negative control, and it carries a second duty since the read-back landed: these
            // are the shapes of the corpus's OTHER two token spaces, and the registry must not answer
            // for either. `1, 2, 8` are caller-frame tokens (`s_callerRecords.Count`, small integers)
            // and must keep routing to runtime's caller table; `0x7FFF_FFFF` is managed-pointer
            // shaped (a 32-bit hash). Zero is the value the old `return default` produced, so a
            // registry answering it with a function would re-create the exact defect this replaces.
            foreach (nuint outside in new nuint[] { 0, 1, 2, 8, 0x1000, 0x7FFF_FFFF, uint.MaxValue })
            {
                Assert.IsNull(GoSyntheticPC.Resolve(outside), $"0x{outside:X} is not a synthetic PC");
                Assert.IsFalse(GoSyntheticPC.IsSynthetic(outside), $"0x{outside:X} is not a synthetic PC");
            }
        }

        [TestMethod]
        public void ATokenIsNeverAnAddressAnythingCouldBeMappedAt()
        {
            // The tokens are minted from the canonical HIGH half — the kernel half on x86-64, TTBR1
            // on arm64 — so a caller that dereferences one faults immediately instead of reading a
            // stranger's memory. The property is ASSERTED rather than demonstrated on purpose:
            // demonstrating it means faulting the test host.
            // 64-bit only, and the refusal below is the other half of the same rule: an earlier
            // draft narrowed the base to 0xF000_0000 on a 32-bit runtime, where it OVERLAPS
            // ManagedPointerTokens (an unconstrained 32-bit hash), so a resolver consulting both
            // could answer a pointer token as a function. The mint refuses rather than narrowing.
            if (IntPtr.Size != 8)
            {
                Assert.ThrowsException<PlatformNotSupportedException>(() => GoSyntheticPC.Of(DemoFn),
                    "a 32-bit runtime must be refused at the mint, not given a narrowed range");
                return;
            }

            nuint pc = GoSyntheticPC.Of(DemoFn);

            Assert.IsTrue(pc >= unchecked((nuint)0xFFFF_8000_0000_0000UL),
                "a token must sit in the non-user half of the address space");

            // Disjoint from the corpus's other token spaces BY CONSTRUCTION, which is what lets one
            // resolver answer for all of them: caller spans (RuntimeCallerPCSpanTests guards those) and
            // identity tokens (ManagedPointerTokens.IdentityBand, whose bit 47 is clear where a
            // synthetic PC's is set).
            Assert.IsFalse(ManagedPointerTokens.IsIdentityToken(pc),
                "a token must not collide with the identity-token space");
        }

        [TestMethod]
        public void TheNameIsSpelledTheWayGoSpellsIt()
        {
            Assert.AreEqual("pcfixtures/demo.Fn", GoSyntheticPC.GoNameOf(DemoFn));

            // An internal test file compiles INTO the package; an external one is its own package.
            // Getting this wrong would not fail loudly — it would quietly name a frame something Go
            // never prints — which is why both variants are pinned rather than one.
            Assert.AreEqual("pcfixtures/demo.Fn",
                GoSyntheticPC.GoNameOf(MethodOf("go.pcfixtures.demo_internal_test_package", "Fn")));

            Assert.AreEqual("pcfixtures/demo_test.Fn",
                GoSyntheticPC.GoNameOf(MethodOf("go.pcfixtures.demo_test_package", "Fn")));

            // A package directly under `go` has no path prefix.
            Assert.AreEqual("rootdemo.Fn", GoSyntheticPC.GoNameOf(MethodOf("go.rootdemo_package", "Fn")));
        }

        [TestMethod]
        public void NameOfAPcRoundTripsThroughTheRegistry()
        {
            Assert.AreEqual("pcfixtures/demo.Fn", GoSyntheticPC.NameOf(GoSyntheticPC.Of(DemoFn)));
            Assert.IsNull(GoSyntheticPC.NameOf(0), "zero names nothing");
        }
    }
}
