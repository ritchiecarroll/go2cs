// DarwinKeystoneArgsRecoveryTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// The darwin keystone's runtime dispatch bottom (docs/phase4/DESIGN-darwin-run-layer-2.md §7):
// Go's darwin libc trampolines are called as `libcCall(FuncPCABI0(x_trampoline), unsafe.Pointer(&args))`
// where `args` is a per-call-site struct the trampoline's assembly unpacks BY OFFSET. The converted
// corpus lifts that struct as a real C# type (runtime/darwin/sys_darwin.cs: fcntl_args, nanotime1_r,
// the syscall_*_args family) and passes it as `@unsafe.Pointer.FromPinnedBox(Ꮡargs)` — a NUMBER, with the type
// erased. A displaced managed libcCall therefore has to get back from that number to the lifted type
// to know the call's argument layout.
//
// §7.2 says it can, through machinery that already exists: the mint binds the
// (uintptr) constructor through ж<T>'s implicit uintptr operator, whose value-type path pins the box
// and REGISTERS it (ManagedPointerTokens.RegisterPinned inside the fixed, ж.cs:662-669), and
// ManagedPointerTokens.Resolve is public. That was written as a CODE READ with this probe named as
// the cut's first acceptance step. The core round trip is already a banked guard one step removed
// (NativeAddressStabilityTests.PinnedConversionRegistersItsProvenance, PointerProvenanceTests
// .StructSlotAddressResolvesToItsBox); what those do not cover, and this file does, is
//
//   1. the EXACT emission form — the box travels through `@unsafe.Pointer.FromPinnedBox(Ꮡargs)`, and the
//      dispatcher sees only the Pointer;
//   2. the step after recovery — the layout read off the recovered type by reflection, which is
//      what the dispatcher hands to the native call;
//   3. the BOUND — a lifted args struct that carries a managed reference (mmap_args's unsafe.Pointer
//      fields, mach_vm_region_args's ж<...> out-parameters, proc_regionfilename_args's ж<byte>: 3 of
//      the 13 lifted darwin structs, censused 2026-09-03) has no pinnable storage, so the pin path
//      declines. Until Q44 that meant the number could not resolve at all; under the token
//      (docs/phase4/DESIGN-managed-pointer-token.md, cut 2026-09-05) it resolves — to its own box and
//      never another — but it is still not an ADDRESS, and a reference-bearing layout is still not a
//      native one. For those three the dispatcher must still FAIL LOUDLY (§7.3, "never a default"),
//      and the design's named fallback — a converter-emitted per-symbol layout record — is still the
//      remedy. This arm is what keeps that bound a statement rather than a mid-cut surprise.
//
// MEASURED 2026-09-03 (Linux host, GolibTests linux flavor, Debug, 5/5, zero aborts) — the prediction on
// record in the commit that added this file held exactly: arms 1-3 and 5 PASS, and arm 4 took the
// `recovered is null` branch (a reference-bearing shape does NOT resolve), so §7.2's bound stands as
// stated. The instrument can fail in both directions — arms 1-3 need a non-null resolve, arms 4-5 a
// null one — which is what makes the green a measurement rather than a tautology.
//
// AMENDED 2026-09-05 (Q44, the managed pointer token). Arm 4's `recovered is null` branch is the
// pre-Q44 reading; the token arm in ж.cs's address-take operators registers a reference-bearing
// box's order token, so the arm now takes the branch it already carried for exactly this case
// (`AreSame`, once followed by an Inconclusive saying "record the mechanism before relying on it")
// and is rewritten as a PASS assertion recording that mechanism — the number is the box's
// PointerOrderToken, it resolves to the box, and the box is pinned nowhere. Arms 1-3 and 5 are
// unchanged in what they measure. Prediction for this cut's run: 5/5 PASS, both configurations.
//
// Reference-free by construction, the way the corpus's fcntl_args is:
//     [GoType("dyn")] internal partial struct fcntl_args { internal int32 fd, cmd, arg; internal int32 ret, errno; }
// ⚠ The emission form this file mirrors CHANGED on 2026-09-04 and the prose above is updated with it.
// It was `new @unsafe.Pointer(Ꮡargs)`, which binds the implicit ж→uintptr conversion into
// Pointer(uintptr): that conversion pins the box and registers the provenance record — which is what
// recovery reads — but the resulting Pointer retained NOTHING, so the pinned box was collectible the
// instant the mint returned and the weak provenance entry could be emptied under a live call. The
// converter now mints through `@unsafe.Pointer.FromPinnedBox`, which takes the address from the same
// conversion (so every arm below is unchanged in what it measures) and retains the box, which makes
// this recovery strictly more robust: the entry cannot be collected while the Pointer is reachable.
[TestClass]
public class DarwinKeystoneArgsRecoveryTests
{
    // The shape of runtime/darwin/sys_darwin.cs's fcntl_args: three ins, two outs, all int32.
    private struct FcntlArgsShape
    {
        internal int fd, cmd, arg;
        internal int ret, errno;
    }

    // The shape of nanotime1_r: one int64 result and mach_timebase's numer/denom.
    private struct Nanotime1ResultShape
    {
        internal long t;
        internal uint numer, denom;
    }

    // The shape of mmap_args's reference-bearing members: an unsafe.Pointer field is a managed
    // reference (go.unsafe_package.Pointer is a class), which is exactly what disqualifies the
    // struct from the pin path.
    private struct MmapArgsShape
    {
        internal @unsafe.Pointer addr;
        internal uintptr n;
        internal int prot, flags, fd;
        internal uint off;
        internal @unsafe.Pointer ret1;
        internal nint ret2;
    }

    // The emission form, in one place so the guard tracks what the corpus actually does:
    //     ref var args = ref heap<fcntl_args>(out var Ꮡargs);
    //     libcCall(…, @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    private static @unsafe.Pointer EmitLibcCallArg<T>(ж<T> Ꮡargs)
    {
        return @unsafe.Pointer.FromPinnedBox(Ꮡargs);
    }

    // What the displaced libcCall does with the Pointer it is handed: the number, resolved.
    private static object? RecoverArgsBox(@unsafe.Pointer arg)
    {
        return ManagedPointerTokens.Resolve((nuint)(uintptr)arg);
    }

    [TestMethod]
    public void FcntlShapedArgsResolveBackToTheirBoxThroughTheEmissionForm()
    {
        ref FcntlArgsShape args = ref heap<FcntlArgsShape>(out ж<FcntlArgsShape> Ꮡargs);
        args = new FcntlArgsShape { fd = 1, cmd = 3, arg = 0 };

        @unsafe.Pointer arg = EmitLibcCallArg(Ꮡargs);

        // The dispatcher's whole question: from the Pointer alone, which box — and therefore which
        // lifted type — is this?
        object? recovered = RecoverArgsBox(arg);

        Assert.AreSame(Ꮡargs, recovered,
            "the number a libcCall argument carries must resolve to the box the call site pinned — without this the dispatcher cannot learn the call's layout");
        Assert.IsInstanceOfType(recovered, typeof(ж<FcntlArgsShape>),
            "the recovered box must carry the lifted args type, which is what the layout is read from");

        // And the value the trampoline would unpack is the one the call site wrote: the recovered
        // box aliases the SAME storage, not a copy.
        var box = (ж<FcntlArgsShape>)recovered!;
        Assert.AreEqual(3, box.Value.cmd, "the recovered box must alias the call site's storage");
        args.cmd = 4;
        Assert.AreEqual(4, box.Value.cmd, "a write through the call site's ref must be visible through the recovered box");

        GC.KeepAlive(args);
    }

    [TestMethod]
    public void RecoveredTypeYieldsTheTrampolineLayoutByOffset()
    {
        // Go's trampoline for fcntl reads fd at 0, cmd at 4, arg at 8 and writes ret at 12,
        // errno at 16 (sys_darwin_amd64.s: `MOVL 0(DI), DI; MOVL 4(DI), SI; MOVL 8(DI), DX; …
        // MOVL AX, 12(DI); MOVL AX, 16(DI)`). The layout the dispatcher hands the native call is
        // exactly the C# struct's own sequential layout, read by reflection from the recovered type.
        ref FcntlArgsShape args = ref heap<FcntlArgsShape>(out ж<FcntlArgsShape> Ꮡargs);
        object? recovered = RecoverArgsBox(EmitLibcCallArg(Ꮡargs));
        Assert.IsNotNull(recovered);

        Type argsType = recovered!.GetType().GetGenericArguments()[0];
        Assert.AreEqual(typeof(FcntlArgsShape), argsType, "the recovered box's type argument is the lifted args struct");

        FieldInfo[] fields = argsType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.AreEqual(5, fields.Length, "fcntl_args has five int32 members");

        // Declaration order and sequential offsets, which is what the trampoline's assembly assumes.
        string[] expectedNames = { "fd", "cmd", "arg", "ret", "errno" };
        for (int i = 0; i < expectedNames.Length; i++)
        {
            FieldInfo field = argsType.GetField(expectedNames[i], BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.IsNotNull(field, $"field {expectedNames[i]} must exist");
            Assert.AreEqual(i * 4, (int)Marshal.OffsetOf(argsType, expectedNames[i]),
                $"{expectedNames[i]} must sit at offset {i * 4}, where the trampoline reads it");
            Assert.AreEqual(typeof(int), field.FieldType);
        }

        Assert.AreEqual(20, Marshal.SizeOf(argsType), "five int32s: the native block is 20 bytes");
        Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<FcntlArgsShape>(),
            "fcntl_args is reference-free, which is what admits it to the pin path at all");

        GC.KeepAlive(args);
    }

    [TestMethod]
    public void MixedWidthResultStructResolvesAndLaysOutAsGoExpects()
    {
        // nanotime1_r: `t int64; numer, denom uint32` — mach_timebase_info's two words after the
        // 8-byte time. Offsets 0/8/12, size 16, matching the trampoline's `MOVQ AX, 0(DI); MOVL …, 8(DI); MOVL …, 12(DI)`.
        ref Nanotime1ResultShape r = ref heap<Nanotime1ResultShape>(out ж<Nanotime1ResultShape> Ꮡr);
        object? recovered = RecoverArgsBox(EmitLibcCallArg(Ꮡr));

        Assert.AreSame(Ꮡr, recovered, "a result-only args struct resolves exactly as an argument struct does");

        Type t = typeof(Nanotime1ResultShape);
        Assert.AreEqual(0, (int)Marshal.OffsetOf(t, "t"));
        Assert.AreEqual(8, (int)Marshal.OffsetOf(t, "numer"));
        Assert.AreEqual(12, (int)Marshal.OffsetOf(t, "denom"));
        Assert.AreEqual(16, Marshal.SizeOf(t));

        GC.KeepAlive(r);
    }

    [TestMethod]
    public void ReferenceBearingArgsStructResolvesThroughItsToken_TheBoundQ44Narrowed()
    {
        // Once the NEGATIVE arm, banked stating the keystone design's §7.2 bound: a lifted args
        // struct carrying a managed reference has no pinnable storage (StandardBox gates
        // PinnableStorage on !IsReferenceOrContainsReferences<T>), so EnsureStableAddress pinned
        // nothing, IsPinnedAt answered false, Resolve's validate-on-read MISSED, and the number the
        // dispatcher held could not be traced back to a type. Q44 narrowed the bound: the address take
        // hands such a box its own ORDER TOKEN, registered in ManagedPointerTokens, so the number DOES
        // trace back — to the box itself and never to another. What the bound still says, and this
        // arm still asserts, is that the number is not an ADDRESS: the box is pinned nowhere, and a
        // reference-bearing layout is not a native one — for the three lifted darwin structs in this
        // class (mmap_args, mach_vm_region_args, proc_regionfilename_args) the per-symbol layout
        // record remains the dispatcher's remedy; the token turns the miss into a resolve, not the
        // struct into something libc can read.
        Assert.IsTrue(RuntimeHelpers.IsReferenceOrContainsReferences<MmapArgsShape>(),
            "the control's premise: this shape carries a managed reference (the unsafe.Pointer fields)");

        ref MmapArgsShape args = ref heap<MmapArgsShape>(out ж<MmapArgsShape> Ꮡargs);
        args = new MmapArgsShape { n = 4096, prot = 3 };

        @unsafe.Pointer arg = EmitLibcCallArg(Ꮡargs);
        nuint number = (nuint)(uintptr)arg;
        object? recovered = RecoverArgsBox(arg);

        Assert.IsNotNull(recovered,
            "a reference-bearing args box must resolve under the token arm — a null here is the pre-Q44 miss coming back");
        Assert.AreSame(Ꮡargs, recovered,
            "a reference-bearing box that resolves must resolve to ITSELF — a foreign box here would hand the dispatcher a wrong layout");

        Assert.AreEqual(Ꮡargs.PointerOrderToken, number,
            "the number is the box's order token, never a heap address");
        Assert.IsFalse(((INilPointer)Ꮡargs).IsPinnedAt(number),
            "a reference-bearing box must not claim to be pinned at the number it reported — nothing was pinned");

        GC.KeepAlive(args);
    }

    [TestMethod]
    public void AnUnpinnedNumberMisses_SoTheDispatcherCanFailLoudly()
    {
        // §7.3's third row: a Pointer whose number was never a pinned managed address (a raw
        // uintptr the way FuncPCABI0's `default` would produce one, or a genuinely native address)
        // must resolve to nothing, so the dispatcher's "throw naming the trampoline" path has a
        // reliable predicate — Resolve == null — rather than a guess.
        var fromNowhere = new @unsafe.Pointer((uintptr)0x4242);
        Assert.IsNull(RecoverArgsBox(fromNowhere), "a number no conversion pinned must MISS");

        var nilArg = new @unsafe.Pointer(nil);
        Assert.IsNull(RecoverArgsBox(nilArg), "the nil pointer must MISS (address 0)");
    }
}
