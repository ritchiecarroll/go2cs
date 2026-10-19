using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The SAFETY FLOOR at the uintptr conversion door — <c>docs/phase4/DESIGN-native-array-view.md</c>
/// §4, landed as a PROVENANCE-tested refusal (q100).
/// </summary>
/// <remarks>
/// <para>
/// The fork this guards: <c>array&lt;E&gt;</c> is a MANAGED struct whose first field is an
/// <c>E[]</c> reference, and a native box materializes its value out of the pointed-at bytes — so
/// <c>(ж&lt;array&lt;E&gt;&gt;)(uintptr)addr</c> over genuinely-native memory reinterprets whatever
/// lives there AS A MANAGED REFERENCE and dereferences it. Zeroed memory reads a length of 0, a
/// silent wrong answer; filled memory reads a length off the data bytes and returns a number instead
/// of faulting, by luck.
/// </para>
/// <para>
/// ⚠ THE DISCRIMINATOR IS PROVENANCE, NOT T, and these arms are arranged to say so in both
/// directions. A floor keyed on T alone was ratified and then withdrawn on lane R's measured
/// disproof — 6 of 609 behavioral tests red, because pinned-managed round-trips over Go-legal
/// reinterprets arrive at this same operator. So the admitted arm below is as load-bearing as the
/// refused one: a pinned-managed address must still convert, because it RESOLVES.
/// </para>
/// <para>
/// This file does not duplicate <see cref="ArrayShapeReinterpretTests"/>. That one is the WITNESS
/// for the defect and exercises a pinned-managed address, which is why the floor leaves it
/// untouched — its note says it should be RE-READ rather than deleted if the arc lands, and the
/// reading is that its address has a provenance record and is admitted by design.
/// </para>
/// </remarks>
[TestClass]
public class NativeArrayViewFloorTests
{
    private const int NativeBytes = 64;

    /// <summary>
    /// THE CLASS, refused BY NAME: a genuinely-native address — nothing registered it — converted to
    /// a pointer whose pointee is an <c>array&lt;E&gt;</c>.
    /// </summary>
    [TestMethod]
    public void AGenuinelyNativeAddressAtAnArrayPointeeIsRefusedByName()
    {
        nint block = Marshal.AllocHGlobal(NativeBytes);

        try
        {
            // Filled, not zeroed, deliberately: this is the reading that used to return
            // -1414812757 rather than fault, and it is the one a reader must never see again.
            for (int i = 0; i < NativeBytes; i++)
                Marshal.WriteByte(block, i, 0xAB);

            PanicException panic = Assert.ThrowsException<PanicException>(
                () => _ = (ж<array<byte>>)(uintptr)(nuint)block,
                "a native address with no provenance record must not become an array<byte> view");

            string message = panic.Message ?? "";

            // CONCRETE rather than "it threw": any PanicException would satisfy a bare type
            // assertion, including one from an unrelated fault on the way in.
            StringAssert.Contains(message, "array<Byte>",
                "the refusal must name the SHAPE it refused, so the reader knows which conversion died");
            StringAssert.Contains(message, "no managed element storage",
                "the refusal must name the CAUSE, not merely decline");
            StringAssert.Contains(message, "DESIGN-native-array-view.md",
                "the refusal must point at the document, or the next reader re-derives the fork");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// THE SCOPE, in the other direction: the SAME native address at a NON-array pointee is
    /// admitted. The floor refuses a shape, not an address.
    /// </summary>
    /// <remarks>
    /// This arm is also the ANTI-VACUITY guard for the one above: if a native address could not
    /// pass this door at all, "the array form is refused" would prove nothing about the shape test.
    /// </remarks>
    [TestMethod]
    public void TheSameNativeAddressAtANonArrayPointeeIsAdmitted()
    {
        nint block = Marshal.AllocHGlobal(NativeBytes);

        try
        {
            for (int i = 0; i < NativeBytes; i++)
                Marshal.WriteByte(block, i, 0xAB);

            ж<byte> scalar = (ж<byte>)(uintptr)(nuint)block;

            Assert.IsNotNull(scalar, "a native address at a scalar pointee must still convert");
            Assert.AreEqual((nuint)block, scalar.NativeAddress,
                "and it must alias the address it was given, not a copy of what lives there");
            Assert.AreEqual((byte)0xAB, scalar.Value, "the alias must read through to the native bytes");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// ⚠ THE ARM THE WITHDRAWN FLOOR FAILED: a PINNED-MANAGED address at an array pointee must be
    /// ADMITTED, because <c>ManagedPointerTokens.Resolve</c> answers for it.
    /// </summary>
    /// <remarks>
    /// This is the shape 25 of the corpus's 70 array-typed raw-address sites have (measured 2026-09-16
    /// across linux, windows and darwin): the address comes from a pinned managed box, so it carries a
    /// provenance record and never reaches the refusal. A floor keyed on T would have refused every one
    /// of them, which is precisely how the type-tested form went 6 of 609 red.
    /// </remarks>
    [TestMethod]
    public void APinnedManagedAddressAtAnArrayPointeeIsAdmittedBecauseItResolves()
    {
        array<uint64> buffer = new(8);
        ж<array<uint64>> pinned = new StandardBox<array<uint64>>(buffer);

        // The round trip: taking the uintptr PINS the backing and registers the provenance record,
        // and the reverse conversion resolves it. A DIFFERENT array shape is used on the way back so
        // the recovered box cannot satisfy the operator's first arm — the conversion has to reach the
        // fall-through and be admitted there, which is exactly what the floor must not disturb.
        uintptr address = (uintptr)pinned;

        ж<array<uint32>> view = (ж<array<uint32>>)address;

        Assert.IsNotNull(view,
            "a pinned-managed address must still convert at an array pointee — refusing it is the " +
            "6-of-609 regression the type-tested floor was withdrawn for");

        GC.KeepAlive(pinned);
    }

    /// <summary>
    /// <c>array&lt;T&gt;.AliasPointer</c> over a NATIVE element does not reach the floor: it answers the native
    /// array door at the element's address, a window onto the bytes, as Go's <c>(*[N]T)(unsafe.Pointer(p))</c> is.
    /// </summary>
    /// <remarks>
    /// This arm used to require the refusal: <c>AliasPointer</c>'s raw-metal fallback,
    /// <c>(ж&lt;array&lt;T&gt;&gt;)(uintptr)element</c>, was the only route for an element with no managed storage,
    /// and it funneled through the floor. An element in native memory now takes the native arm ahead of that
    /// fallback (os's createMountPoint, an array pointer over a reparse buffer's PathBuffer), so its address is
    /// viewed, never materialized as a managed array. The floor itself is unchanged and the arms above still
    /// drive it through the operator.
    /// </remarks>
    [TestMethod]
    public void AliasPointerOverANativeElementWindowsItsBytesInsteadOfReachingTheFloor()
    {
        nint block = Marshal.AllocHGlobal(NativeBytes);

        try
        {
            for (int i = 0; i < NativeBytes; i++)
                Marshal.WriteByte(block, i, 0);

            ж<byte> element = (ж<byte>)(uintptr)(nuint)block;
            ж<array<byte>> view = array<byte>.AliasPointer(element, 4);

            view.ElementRef(2) = 0x5a;

            Assert.AreEqual((byte)0x5a, Marshal.ReadByte(block, 2), "the view writes the native byte in place");
            Assert.AreEqual((byte)0, Marshal.ReadByte(block, 4), "the view is the four bytes at the element, no more");
            Assert.ThrowsException<PanicException>(() => view.ElementRef(4), "the window is bounded by the length asked for");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }
}
