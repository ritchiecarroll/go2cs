using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// B0', the managed-derived slice of the native-box reference floor (W14, COORD ruling 2026-11-15): reading the WHOLE
/// value of a Go struct that holds a managed reference, through a native box derived from Go storage of another type,
/// is refused by name.
/// </summary>
/// <remarks>
/// <para>
/// sqlite's crypt_blowfish reads <c>*(*TBF_ctx)(unsafe.Pointer(&amp;[1042]uint32{...}))</c>. <c>TBF_ctx</c> is kept
/// out of inline arrays, so its fields are <c>array&lt;T&gt;</c> references, and <c>Reinterpret</c> takes the address
/// route: a <see cref="NativeBox{T}"/> at a real address inside the source's storage. Managed storage reachable by
/// address never holds a reference, so a whole-value read takes data bytes for references. Over filled data that is an
/// access violation, which ended the converted program in its static constructor (measured; the dereference census
/// read <c>B2b kind=Struct T=TBF_ctxᴛ1 source=retained</c>). Over zeroed data it reads null references and goes on, which
/// is no more an answer. Both now panic by name.
/// </para>
/// <para>
/// The arms are read over a ZEROED buffer. The filled case takes the test host down before the floor exists, so it
/// cannot be the red arm; zeroed is the same route and the same read, and it shows the read was answered when it
/// should have been refused.
/// </para>
/// <para>
/// Scope: only boxes derived from MANAGED storage (the census's source=retained or source=pinned). A native-origin box
/// (C memory, source=none) is P5's deferred B2 and is not refused here (the last arm). A carrier over the same box, which
/// reads a field at its Go offset or converts onward without reading the whole value, keeps working (the second arm).
/// </para>
/// </remarks>
[TestClass]
public unsafe class ManagedDerivedStructReadFloorTests
{
    private const int Words = 1042;

    // TBF_ctx's shape: kept out of inline arrays, so every array is an array<T> reference.
    public struct CtxFs
    {
        public array<uint32> FP;
        public array<array<uint32>> FS;
    }

    public struct Ctx
    {
        public array<array<uint32>> FPS;
        public CtxFs Fs;
    }

    // A struct whose Go layout is known (no array of unknown length): N at 0, S at 8, size 24.
    public struct Header
    {
        public uint32 N;
        public @string S;
    }

    // Spelled as go2cs-gen spells a field accessor: golib reads the Go field name off the delegate.
    private static ref uint32 ᏑN(ref Header value) => ref value.N;

    // No reference anywhere: a whole-value read over another type's storage is a byte read.
    public struct Pair
    {
        public uint32 A;
        public uint32 B;
    }

    private static PanicException? Attempt(Action read)
    {
        try
        {
            read();
            return null;
        }
        catch (PanicException panic)
        {
            return panic;
        }
    }

    private static void AssertRefusedByThisFloor(PanicException? panic, string typeName, string path)
    {
        Assert.IsNotNull(panic, $"{path}: a whole-value read of a struct holding references, over Go storage of another type, must be refused");

        string message = panic.Message ?? "";

        StringAssert.Contains(message, $"cannot read {typeName} whole", $"{path}: by this floor, naming the type");
        StringAssert.Contains(message, "holds managed references", $"{path}: the reason");
        StringAssert.Contains(message, $"are not {typeName}'s", $"{path}: the type is named where the bytes are said not to be its");
        Assert.IsFalse(message.Contains('{') || message.Contains('}'), $"{path}: no unformatted placeholder survives: {message}");
    }

    [TestMethod]
    public void AStructHoldingReferencesReadWholeOverGoStorageIsRefusedByName()
    {
        ж<array<uint32>> source = builtin.Ꮡ(new array<uint32>(Words));
        ж<Ctx> view = source.Reinterpret<array<uint32>, Ctx>();

        Assert.IsInstanceOfType(view, typeof(NativeBox<Ctx>), "precondition: the address route, or this arm measures another route");

        AssertRefusedByThisFloor(Attempt(() => _ = view.Value), nameof(Ctx), "read");
        AssertRefusedByThisFloor(Attempt(() => view.Value = default), nameof(Ctx), "write");

        for (int i = 0; i < Words; i++)
            Assert.AreEqual(0u, (uint)source.Value[i], $"the refused write stored into the source's element {i}");
    }

    [TestMethod]
    public void ACarrierOverTheSameBoxStillReadsAField()
    {
        array<uint32> words = new(Words);
        words[0] = 0x1234;
        ж<array<uint32>> source = builtin.Ꮡ(words);
        ж<Header> view = source.Reinterpret<array<uint32>, Header>();

        Assert.IsInstanceOfType(view, typeof(NativeBox<Header>), "precondition: the address route");
        Assert.AreNotEqual(0UL, (ulong)((uintptr)view).Value, "the number of a carrier is still its address");

        uint32 n = view.of<uint32>(ᏑN).Value;

        Assert.AreEqual(0x1234u, (uint)n, "a field at its Go offset reads the source's bytes, as Go reads them");

        // Header's Go layout is KNOWN, so its whole value is already refused by the layout floor, which runs first
        // (ClrLayoutMismatch: "field S is a managed reference where Go stores the value inline"). This floor covers the
        // struct whose layout cannot be proven, as TBF_ctx's cannot; the first arm reads that one.
        PanicException? whole = Attempt(() => _ = view.Value);

        Assert.IsNotNull(whole, "the same box's whole value is refused");
        StringAssert.Contains(whole.Message ?? "", nameof(Header), "by a floor that names the type");
    }

    [TestMethod]
    public void AReferenceFreeStructOverTheSameStorageStillReadsWhole()
    {
        array<uint32> words = new(Words);
        words[0] = 7;
        words[1] = 9;
        ж<Pair> view = builtin.Ꮡ(words).Reinterpret<array<uint32>, Pair>();

        Pair pair = view.Value;

        Assert.AreEqual(7u, (uint)pair.A);
        Assert.AreEqual(9u, (uint)pair.B);
    }

    // P5's check (2026-11-15): the census's two "UNPINNABLE T=System.Int64 root=go.FieldRefBox<System.IntPtr>" rows, built as
    // NativeBoxDereferenceCensusControlTests builds them. The source is a reference-free FIELD of a struct that holds a
    // reference: a real address nothing holds still, and the box retains it. The pointee is reference-free, so this floor
    // must not fire, directly or through a derivation.
    private struct Named
    {
        internal string name;
        internal nint scalar;

        internal static ref nint Ꮡscalar(ref Named instance) => ref instance.scalar;
    }

    [TestMethod]
    public void AnUnpinnableSourceWithAReferenceFreePointeeIsNotThisFloor()
    {
        nuint memory = (nuint)NativeMemory.AllocZeroed(64);

        try
        {
            ж<Named> container = new StandardBox<Named>(new Named { name = "tcp", scalar = 5 });
            ж<nint> field = container.of(Named.Ꮡscalar);

            Assert.AreEqual(PointerStorage.Unpinnable, field.StorageKind, "the premise: a field of a struct that holds a reference");

            PanicException? direct = Attempt(() => _ = new NativeBox<long>(memory, retainedSource: field).Value);
            PanicException? derived = Attempt(() => _ = new NativeBox<long>(memory + 8, retainedSource: new NativeBox<byte>(memory + 8, retainedSource: field)).Value);

            Assert.IsNull(direct, "directly: " + direct?.Message);
            Assert.IsNull(derived, "through a derivation: " + derived?.Message);
            GC.KeepAlive(container);
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void ANativeOriginReadIsNotThisFloor()
    {
        int bytes = 4096 * 4 + 1024;
        nint block = Marshal.AllocHGlobal(bytes);

        try
        {
            for (int i = 0; i < bytes; i++)
                Marshal.WriteByte(block, i, 0);

            ж<Ctx> view = (ж<Ctx>)(uintptr)(nuint)block;
            PanicException? panic = Attempt(() => _ = view.Value);

            Assert.IsFalse((panic?.Message ?? "").Contains("cannot read Ctx whole"),
                "C memory is P5's deferred B2 (source=none), not this floor: " + panic?.Message);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }
}
