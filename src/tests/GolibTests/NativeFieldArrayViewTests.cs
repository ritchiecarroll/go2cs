using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using syscall = go.syscall_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The NATIVE FIELD-VIEW door — Go's <c>(*[N]T)(unsafe.Pointer(&amp;p.f))</c> where <c>p</c> points at
/// native memory and <c>f</c>'s type is not <c>T</c>: the port alias of net's darwin
/// <c>cgoLookupServicePort</c> (Go net/cgo_unix.go:154/158), <c>docs/phase4/DESIGN-native-array-view.md</c>.
/// </summary>
/// <remarks>
/// <para>
/// TWO DEFECTS sit on that one line, and these arms are arranged so a fix for only the first cannot pass.
/// The first is the array view: <c>(ж&lt;array&lt;byte&gt;&gt;)(uintptr)</c> over a native address has no
/// <c>array&lt;T&gt;</c> to materialize, and the floor refuses it by name. The SECOND is the address
/// itself: <c>p.of(S.Ꮡf)</c> over a native root takes the field's address from the CLR slot, i.e. the
/// native base plus the field's CLR offset — and a converted struct that carries an <c>array&lt;&gt;</c>
/// field holds managed references, so the CLR lays it out automatically and that offset is not Go's.
/// A view-only fix therefore reads the wrong bytes with no error: for a sockaddr_in the length and
/// family bytes, so http's port reads 4098 instead of 80, and net never falls back to /etc/services.
/// </para>
/// <para>
/// WHICH STRUCT, measured rather than assumed. The CLR's automatic layout of a reference-bearing struct
/// orders fields by size, so whether Port lands on its Go offset depends on what precedes it. The linux and
/// windows <c>RawSockaddrInet4</c> open with a two-byte family and keep Port at 2 by coincidence (read in
/// <see cref="TheBuildFlavoursSockaddrKeepsPortAtGoOffsetTwo"/>); DARWIN's opens with two ONE-byte fields
/// (BSD's sin_len, sin_family) and puts Port at CLR offset 0, and its RawSockaddrInet6 at 8. Darwin is
/// where the site lives, but this assembly cannot load darwin's syscall: it does not build for that flavour,
/// and darwin's module initializer refuses a linux host. So the arms below root on
/// <see cref="DarwinRawSockaddrInet4"/>, darwin's converted declaration transcribed field for field over
/// golib's real <c>array&lt;T&gt;</c> -- static members do not touch instance layout -- and repoguard's
/// TestDarwinSockaddrTranscriptionMatchesItsDeclaration holds the transcription equal to
/// syscall/darwin/ztypes_darwin_amd64.cs, so it cannot drift from the struct it stands for.
/// </para>
/// </remarks>
[TestClass]
public class NativeFieldArrayViewTests
{
    private const int SockaddrBytes = 16;

    // BEGIN TRANSCRIPTION: syscall/darwin/ztypes_darwin_amd64.cs RawSockaddrInet4
    private struct DarwinRawSockaddrInet4
    {
        public uint8 Len;
        public uint8 Family;
        public uint16 Port;
        public array<byte> Addr = new(4); /* in_addr */
        public array<int8> Zero = new(8);

        public DarwinRawSockaddrInet4() { }
    }
    // END TRANSCRIPTION

    // Go's layout of the same declaration (Go ztypes_darwin_amd64.go): Len uint8 at 0, Family uint8 at 1,
    // Port uint16 at 2 -- the offset golib's Go layout must resolve for this site (asserted below).
    private const int DarwinGoOffsetOfPort = 2;

    // Spelled as go2cs-gen spells a field accessor (`Ꮡ` + the Go field name), which is how golib reads
    // the field's name back off the delegate to find its Go offset.
    private static ref uint16 ᏑPort(ref DarwinRawSockaddrInet4 value) => ref value.Port;

    private static nint DarwinClrOffsetOfPort()
    {
        DarwinRawSockaddrInet4 value = default;
        return Unsafe.ByteOffset(ref Unsafe.As<DarwinRawSockaddrInet4, byte>(ref value), ref Unsafe.As<uint16, byte>(ref value.Port));
    }

    // The field's GO offset in the build flavour's own struct, from the generator's Go layout
    // (GoReflect.GoFieldOffsets) -- the same layout the door resolves the offset from at run time.
    private static nint GoOffsetOfPort()
    {
        nint[] offsets = GoReflect.GoFieldOffsets(typeof(syscall.RawSockaddrInet4)) ?? throw new AssertFailedException("no Go layout for RawSockaddrInet4");
        GoReflect.GoFieldInfo[] fields = GoReflect.GoFields(typeof(syscall.RawSockaddrInet4));

        for (int i = 0; i < fields.Length && i < offsets.Length; i++)
        {
            if (fields[i].Name == "Port")
                return offsets[i];
        }

        throw new AssertFailedException("RawSockaddrInet4 has no Port field in its Go layout");
    }

    // A sockaddr_in image as darwin's libc hands it back for http on loopback: sin_len 16, AF_INET, the
    // port in NETWORK byte order at Go offset 2. The bytes ahead of the port are non-zero, so a read at
    // the wrong offset cannot read 80.
    private static nint NativeSockaddrForHttp()
    {
        nint block = Marshal.AllocHGlobal(SockaddrBytes);

        for (int i = 0; i < SockaddrBytes; i++)
            Marshal.WriteByte(block, i, 0);

        Marshal.WriteByte(block, 0, 0x10);      // sin_len
        Marshal.WriteByte(block, 1, 0x02);      // AF_INET
        Marshal.WriteByte(block, 2, 0x00);      // port 80, big-endian
        Marshal.WriteByte(block, 3, 0x50);
        Marshal.WriteByte(block, 4, 127);       // 127.0.0.1
        Marshal.WriteByte(block, 7, 1);

        return block;
    }

    /// <summary>
    /// THE RED: the port alias over a NATIVE darwin sockaddr views the port at its GO offset and reads 80.
    /// </summary>
    /// <remarks>
    /// The arm runs the emission's own shape -- the door, else today's route -- and when the door answers
    /// nothing it takes the address today's route computes (the field view's, without the floor's refusal),
    /// which is exactly what a view-only fix would view. That is how it fails BY NAME on the CLR offset
    /// rather than on the floor's panic.
    /// </remarks>
    [TestMethod]
    public void ThePortAliasOverANativeDarwinSockaddrViewsThePortAtItsGoOffset()
    {
        nint clrOffset = DarwinClrOffsetOfPort();
        nint block = NativeSockaddrForHttp();

        try
        {
            ж<DarwinRawSockaddrInet4> sa = (ж<DarwinRawSockaddrInet4>)(uintptr)(nuint)block;

            Assert.IsTrue(sa.IsNative, "the root must be a native box, or this arm measures nothing native");

            ж<array<byte>>? p = builtin.NativeFieldArrayPointer<byte>(sa.of(ᏑPort), 2);
            nuint viewed = p is not null ? p.NativeAddress : (nuint)(uintptr)sa.of(ᏑPort);

            Assert.AreEqual((nuint)block + DarwinGoOffsetOfPort, viewed,
                $"Port at CLR offset {clrOffset}, Go offset {DarwinGoOffsetOfPort}: the port alias viewed base+{(nint)(viewed - (nuint)block)}, " +
                "so it reads the bytes the CLR layout puts there rather than the port libc wrote");

            int port = p!.ElementRef(0) << 8 | p.ElementRef(1);

            Assert.AreEqual(80, port, "the alias must read the port libc wrote, in network byte order");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// THE WITNESS for the second defect: darwin's converted sockaddr does not keep Port at its Go offset,
    /// so no address taken through a field view of a native root of it is Go's.
    /// </summary>
    /// <remarks>
    /// If this ever FAILS, the converted struct has gained a native layout (an inline representation of its
    /// <c>array&lt;&gt;</c> fields, say) and the door's offset argument is no longer the only correct address
    /// -- re-read this file rather than deleting the arm.
    /// </remarks>
    [TestMethod]
    public void TheDarwinSockaddrPutsPortAtACLROffsetThatIsNotGos()
    {
        nint clrOffset = DarwinClrOffsetOfPort();

        Assert.AreNotEqual((nint)DarwinGoOffsetOfPort, clrOffset, $"Port at CLR offset {clrOffset}, Go offset {DarwinGoOffsetOfPort}");
    }

    /// <summary>
    /// The door's offset comes from golib's Go layout of the struct, resolved at run time -- so it must
    /// agree with Go's layout of darwin's declaration. A folded literal would have made one source file
    /// emit three ways (runtime's m.cheaprand sits at three different offsets across the flavours).
    /// </summary>
    [TestMethod]
    public void GolibsGoLayoutPutsDarwinsPortAtGoOffsetTwo()
    {
        nint[] offsets = GoReflect.GoFieldOffsets(typeof(DarwinRawSockaddrInet4)) ?? throw new AssertFailedException("no Go layout for darwin's RawSockaddrInet4");
        GoReflect.GoFieldInfo[] fields = GoReflect.GoFields(typeof(DarwinRawSockaddrInet4));
        int port = Array.FindIndex(fields, field => field.Name == "Port");

        Assert.IsTrue(port >= 0, "the Go layout projects no Port field");
        Assert.AreEqual((nint)DarwinGoOffsetOfPort, offsets[port], "golib's Go layout disagrees with Go's for darwin's RawSockaddrInet4");
    }

    /// <summary>
    /// The build flavour's OWN converted sockaddr: Go's layout puts Port at 2 in every flavour, and the
    /// generator's Go layout agrees with the converter's fold. Its CLR offset is a reading, logged, not
    /// asserted -- it coincides with Go's on linux and windows.
    /// </summary>
    [TestMethod]
    public void TheBuildFlavoursSockaddrKeepsPortAtGoOffsetTwo()
    {
        syscall.RawSockaddrInet4 value = default;
        nint clrOffset = Unsafe.ByteOffset(ref Unsafe.As<syscall.RawSockaddrInet4, byte>(ref value), ref Unsafe.As<uint16, byte>(ref value.Port));

        Assert.AreEqual(2, (int)GoOffsetOfPort(), "Port is at Go offset 2 in every flavour's RawSockaddrInet4");
        Console.WriteLine($"this flavour's RawSockaddrInet4: Port at CLR offset {clrOffset}, Go offset 2");
    }

    /// <summary>
    /// The helper the converter EMITS, <c>@unsafe.ArrayPointer&lt;T&gt;.Of(field, N)</c>, which names the field
    /// once: a native root reaches the door, and every other root gets exactly what the raw route gives.
    /// </summary>
    [TestMethod]
    public void TheEmittedHelperTakesTheDoorForANativeRootAndTheRawRouteOtherwise()
    {
        nint block = NativeSockaddrForHttp();

        try
        {
            ж<DarwinRawSockaddrInet4> sa = (ж<DarwinRawSockaddrInet4>)(uintptr)(nuint)block;
            ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(sa.of(ᏑPort), 2);

            Assert.AreEqual((nuint)block + DarwinGoOffsetOfPort, p.NativeAddress, "a native root must take the door, at the Go offset");
            Assert.AreEqual(80, p.ElementRef(0) << 8 | p.ElementRef(1), "and read the port libc wrote");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }

        // A managed root: the helper and the raw route must agree, box for box or panic for panic.
        ж<syscall.RawSockaddrInet4> managed = new StandardBox<syscall.RawSockaddrInet4>(default);

        Assert.AreEqual(
            Outcome(() => (ж<array<byte>>)(uintptr)@unsafe.Pointer.FromPinnedBox(managed.of(syscall.RawSockaddrInet4.ᏑPort))),
            Outcome(() => @unsafe.ArrayPointer<byte>.Of(managed.of(syscall.RawSockaddrInet4.ᏑPort), 2)),
            "a managed root must take today's raw route through the helper, unchanged");
    }

    private static string Outcome(Func<ж<array<byte>>> route)
    {
        try
        {
            return "box " + route().GetType().Name;
        }
        catch (Exception exception)
        {
            return "threw " + exception.GetType().Name + ": " + exception.Message;
        }
    }

    /// <summary>
    /// A MANAGED root is not the door's: it answers <c>null</c>, and the emission falls back to today's
    /// route unchanged.
    /// </summary>
    [TestMethod]
    public void AManagedRootTakesTheFallback()
    {
        ж<syscall.RawSockaddrInet4> sa = new StandardBox<syscall.RawSockaddrInet4>(default);

        Assert.AreEqual((nuint)0, sa.NativeAddress, "a managed box has no native address");
        Assert.IsNull(builtin.NativeFieldArrayPointer<byte>(sa.of(syscall.RawSockaddrInet4.ᏑPort), 2),
            "the door must decline a managed root, so the fallback keeps today's route");
    }

    /// <summary>
    /// The read the converter emits for such a local, <c>p.ElementRef(i)</c>, reaches a NATIVE array's
    /// elements in place — a native array pointer has no <c>array&lt;T&gt;</c> to index through
    /// <c>Value</c>, which refuses by design.
    /// </summary>
    [TestMethod]
    public void ElementRefReadsAndWritesANativeArrayInPlace()
    {
        nint block = Marshal.AllocHGlobal(4);

        try
        {
            for (int i = 0; i < 4; i++)
                Marshal.WriteByte(block, i, (byte)(0x10 + i));

            ж<array<byte>> p = builtin.NativeArrayPointer<byte>((nuint)block, 4);

            Assert.AreEqual((byte)0x11, p.ElementRef(1), "a read must reach the native byte");

            p.ElementRef(2) = 0x7F;

            Assert.AreEqual((byte)0x7F, Marshal.ReadByte(block, 2), "a write must land in the native block");
            Assert.AreEqual((byte)0x13, p.ElementRef((nint)3), "the nint overload reads the same block");
            Assert.AreEqual((byte)0x10, p.ElementRef(0UL), "the ulong overload reads the same block");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// An index past the minted length panics with GO's bounds message, as <c>array&lt;T&gt;</c>'s own
    /// indexer does — the length handed to the door is load-bearing.
    /// </summary>
    [TestMethod]
    public void ElementRefRefusesAnIndexPastTheMintedLengthWithGosMessage()
    {
        nint block = Marshal.AllocHGlobal(4);

        try
        {
            ж<array<byte>> p = builtin.NativeArrayPointer<byte>((nuint)block, 4);

            PanicException panic = Assert.ThrowsException<PanicException>(() => _ = p.ElementRef(4));

            StringAssert.Contains(panic.Message ?? "", "index out of range [4] with length 4");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// The same read over a MANAGED array pointer aliases its storage exactly as <c>p.Value[i]</c> does —
    /// the fallback root's path, unchanged.
    /// </summary>
    [TestMethod]
    public void ElementRefOverAManagedArrayAliasesItsStorage()
    {
        ж<array<uint32>> p = new StandardBox<array<uint32>>(new array<uint32>(2));

        p.ElementRef(1) = 5;

        Assert.AreEqual(5u, p.Value[1], "a write through ElementRef must be visible through Value");
        Assert.AreEqual(5u, p.ElementRef((nint)1));
    }
}
