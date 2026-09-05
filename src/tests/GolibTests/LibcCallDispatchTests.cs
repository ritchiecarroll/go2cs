// LibcCallDispatchTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The darwin keystone's DISPATCH (docs/phase4/DESIGN-darwin-run-layer-2.md §7), measured on the
// fleet's own hardware: GoLibcCall is platform-neutral, so the same code that will call libSystem on
// a mac is driven here against glibc — arity, result width, the errno read through a resolved
// reader, and the runtime.libcCall args-struct protocol. Linux-only (glibc's names), excluded from
// compilation under any other $(GoTargetOS) exactly as LinuxSyscallClockTests is.
//
// What a green here proves and what it does not: the MECHANISM is right (a real function pointer is
// called with the right registers and its outcome read back per Go's own conventions); the
// libSystem resolution and the darwin trampolines' reachability stay a mac dispatch's to confirm.
[TestClass]
public class LibcCallDispatchTests
{
    private const int F_GETFL = 3;
    private const int EBADF = 9;

    private static nint s_libc;
    private static nint s_errnoLocation;

    private static nint Export(string symbol)
    {
        if (s_libc == 0)
            s_libc = NativeLibrary.Load("libc.so.6");

        return NativeLibrary.GetExport(s_libc, symbol);
    }

    private static nint ErrnoReader => s_errnoLocation != 0 ? s_errnoLocation : (s_errnoLocation = Export("__errno_location"));

    [TestMethod]
    public void ZeroArgumentCallReturnsTheResultRegister()
    {
        nuint pid = GoLibcCall.Call(Export("getpid"), ReadOnlySpan<nuint>.Empty, GoLibcErrnoRule.None, 0, out nuint errno);

        Assert.AreEqual((nuint)Environment.ProcessId, pid, "getpid() through the dispatcher must return this process's id");
        Assert.AreEqual((nuint)0, errno, "a rule of None never reads errno");
    }

    [TestMethod]
    public void ThreeArgumentCallSucceedsWithoutReadingErrno()
    {
        // fcntl(0, F_GETFL, 0): stdin's flags, never -1 in a test host.
        nuint flags = GoLibcCall.Call(Export("fcntl"), new nuint[] { 0, F_GETFL, 0 }, GoLibcErrnoRule.Int32MinusOne, ErrnoReader, out nuint errno);

        Assert.AreNotEqual(-1, unchecked((int)(uint)flags), "fcntl(0, F_GETFL) must succeed");
        Assert.AreEqual((nuint)0, errno, "no failure, no errno");
    }

    [TestMethod]
    public void FailureReadsErrnoThroughTheResolvedReader_TheInt32Rule()
    {
        // fcntl(-1, F_GETFL, 0): EBADF. The fd is an int32 -1 widened as Go's MOVL widens it — zero-
        // extended — which the callee reads as -1. This is the CMPL rule: the low 32 bits are -1.
        nuint r = GoLibcCall.Call(Export("fcntl"), new nuint[] { unchecked((nuint)(uint)(-1)), F_GETFL, 0 }, GoLibcErrnoRule.Int32MinusOne, ErrnoReader, out nuint errno);

        Assert.AreEqual(-1, unchecked((int)(uint)r), "fcntl on a bad descriptor returns -1");
        Assert.AreEqual((nuint)EBADF, errno, "errno must be EBADF, read through __errno_location");
    }

    [TestMethod]
    public void ANullPointerRuleReadsErrnoOnlyWhenTheResultIsNull()
    {
        // getcwd(NULL, 0) with glibc ALLOCATES (a GNU extension) and returns non-NULL; a size of 1
        // with a NULL buffer is the honest failure: ERANGE or EINVAL depending on libc — either way
        // NULL with errno set. The rule under test is the NULL test itself, not the errno value.
        nuint ok = GoLibcCall.Call(Export("getcwd"), new nuint[] { 0, 0 }, GoLibcErrnoRule.NullPointer, ErrnoReader, out nuint errnoOk);
        Assert.AreNotEqual((nuint)0, ok, "getcwd(NULL, 0) allocates under glibc");
        Assert.AreEqual((nuint)0, errnoOk);
        GoLibcCall.Call(Export("free"), new nuint[] { ok }, GoLibcErrnoRule.None, 0, out _);

        nuint fail = GoLibcCall.Call(Export("getcwd"), new nuint[] { 0, 1 }, GoLibcErrnoRule.NullPointer, ErrnoReader, out nuint errnoFail);
        Assert.AreEqual((nuint)0, fail, "getcwd(NULL, 1) must fail with NULL");
        Assert.AreNotEqual((nuint)0, errnoFail, "and errno must be set");
    }

    // The runtime.libcCall protocol, driven through the same shape runtime/darwin/sys_darwin.cs lifts:
    //     [GoType("dyn")] internal partial struct fcntl_args { internal int32 fd, cmd, arg; internal int32 ret, errno; }
    private struct FcntlArgs
    {
        internal int fd, cmd, arg;
        internal int ret, errno;
    }

    [TestMethod]
    public void ArgsStructDispatchWritesTheOutcomeBackThroughTheBox()
    {
        ref FcntlArgs args = ref heap<FcntlArgs>(out ж<FcntlArgs> Ꮡargs);
        args = new FcntlArgs { fd = 0, cmd = F_GETFL, arg = 0 };

        // What libcCall does: the pointer's number, resolved back to the box (the §7.2 recovery), then
        // dispatched. The recovery itself is guarded in DarwinKeystoneArgsRecoveryTests; here the
        // box is handed over directly so this test measures the dispatch and nothing else.
        GoLibcCall.DispatchArgsStruct(Export("fcntl"), Ꮡargs, ErrnoReader, "fcntl");

        Assert.AreNotEqual(-1, args.ret, "ret must receive fcntl's result");
        Assert.AreEqual(0, args.errno, "no failure, errno stays 0");

        args = new FcntlArgs { fd = -1, cmd = F_GETFL, arg = 0 };
        GoLibcCall.DispatchArgsStruct(Export("fcntl"), Ꮡargs, ErrnoReader, "fcntl");

        Assert.AreEqual(-1, args.ret, "a bad descriptor: ret is -1");
        Assert.AreEqual(EBADF, args.errno, "and errno is EBADF, written back into the struct the caller reads");
        GC.KeepAlive(args);
    }

    // Shape (d) of the darwin census (DESIGN-cgo-unsafe-args-block-lift.md section 1): Go's asmcgocall
    // returns the trampoline's AX as libcCall's int32 and twenty sys_darwin.cs sites read it, which the
    // dispatcher reported as 0 until increment 7. getpid discriminates by construction: a process id
    // is never 0, and it is known independently of the call.
    private struct GetpidArgs
    {
        internal int ret;
    }

    [TestMethod]
    public void ArgsStructDispatchReturnsTheResultRegister_TheSameValueItWritesToRet()
    {
        ref GetpidArgs args = ref heap<GetpidArgs>(out ж<GetpidArgs> Ꮡargs);

        nuint r = GoLibcCall.DispatchArgsStruct(Export("getpid"), Ꮡargs, ErrnoReader, "getpid");

        Assert.AreEqual(Environment.ProcessId, unchecked((int)(uint)r), "the dispatcher returns the result register -- what libcCall hands its caller");
        Assert.AreEqual(Environment.ProcessId, args.ret, "and the same value reaches ret, which the lifted family reads");
        GC.KeepAlive(args);
    }

    [TestMethod]
    public void ANullArgsBoxIsTheZeroArgumentTrampoline()
    {
        // libcCall(fn, nil): kqueue and issetugid pass no args struct at all. Before increment 7 a null
        // box was refused as "does not resolve to a managed args box", so neither ever reached its call.
        nuint r = GoLibcCall.DispatchArgsStruct(Export("getpid"), null, ErrnoReader, "kqueue");

        Assert.AreEqual(Environment.ProcessId, unchecked((int)(uint)r), "a null box makes the bare call and returns its register");
    }

    private struct ReferenceBearingArgs
    {
        internal go.unsafe_package.Pointer addr;
        internal nuint n;
        internal nint ret;
    }

    [TestMethod]
    public void AReferenceBearingFieldIsRefusedByName()
    {
        ref ReferenceBearingArgs args = ref heap<ReferenceBearingArgs>(out ж<ReferenceBearingArgs> Ꮡargs);

        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            GoLibcCall.DispatchArgsStruct(Export("getpid"), Ꮡargs, ErrnoReader, "mmap"));

        StringAssert.Contains(ex.Message, "mmap", "the refusal names the symbol");
        StringAssert.Contains(ex.Message, "addr", "and the field it could not place");
        GC.KeepAlive(args);
    }

    [TestMethod]
    public void ANullFunctionPointerIsRefused_NeverCalled()
    {
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            GoLibcCall.Call(0, ReadOnlySpan<nuint>.Empty, GoLibcErrnoRule.None, 0, out _));

        StringAssert.Contains(ex.Message, "never resolved");
    }

    // ---- the BLOCK LIFT's dispatcher arms (DESIGN-cgo-unsafe-args-block-lift.md section 7), on glibc ----
    //
    // The converter now synthesizes the whole parameter block for darwin's `&first` libcCall sites
    // (cgoUnsafeArgsLift.go), so the dispatcher's register placement is what those sites stand on.
    // Each arm discriminates BY CONSTRUCTION: kill(getpid(), 0) is 0 only if both registers were
    // placed; clock_gettime fills a timespec only through the address the block carried; the two
    // per-symbol FORMS (walltime's block-by-address, pthread_self's result-into-block) are measured
    // against glibc's own clock_gettime and pthread_self. Neutered once each before being believed
    // (the fields placed in reverse; a uintptr field placed as 0; the leading register dropped; the
    // store skipped) -- every neuter RED by name.

    private const int CLOCK_REALTIME = 0;

    private struct KillArgs
    {
        internal int pid;
        internal int sig;
    }

    [TestMethod]
    public void ATwoFieldBlockPlacesEveryRegister_KillOfSelfWithSignalZero()
    {
        ref KillArgs args = ref heap(new KillArgs { pid = Environment.ProcessId, sig = 0 }, out ж<KillArgs> Ꮡargs);

        nuint r = GoLibcCall.DispatchArgsStruct(Export("kill"), Ꮡargs, ErrnoReader, "kill");

        Assert.AreEqual(0, unchecked((int)(uint)r), "kill(getpid(), 0) succeeds only when the SECOND register carried the 0 (a stale one is EINVAL/ESRCH)");
        GC.KeepAlive(args);
    }

    private struct Timespec
    {
        internal long tv_sec;
        internal long tv_nsec;
    }

    private struct ClockGettimeArgs
    {
        internal int clk;
        internal uintptr ts;
    }

    [TestMethod]
    public void APointerFieldRidesAsThePinnedAddress_ClockGettimeFillsTheBoxedTimespec()
    {
        ref Timespec ts = ref heap(new Timespec(), out ж<Timespec> Ꮡts);
        ref ClockGettimeArgs args = ref heap(new ClockGettimeArgs { clk = CLOCK_REALTIME, ts = (uintptr)Ꮡts }, out ж<ClockGettimeArgs> Ꮡargs);

        nuint r = GoLibcCall.DispatchArgsStruct(Export("clock_gettime"), Ꮡargs, ErrnoReader, "clock_gettime");

        Assert.AreEqual(0, unchecked((int)(uint)r), "clock_gettime(CLOCK_REALTIME, &ts) succeeds");
        Assert.IsTrue(ts.tv_sec > 1_600_000_000L, $"tv_sec read back THROUGH THE BOX must be the wall clock, got {ts.tv_sec} -- the address libc wrote was the pinned box");
        GC.KeepAlive(ts);
        GC.KeepAlive(args);
    }

    [TestMethod]
    public void TheBlockAddressForm_ClockGettimeWritesTheBlockItself()
    {
        ref Timespec ts = ref heap(new Timespec(), out ж<Timespec> Ꮡts);

        nuint r = GoLibcCall.CallWithBlockAddress(Export("clock_gettime"), (nuint)(uintptr)Ꮡts, [(nuint)CLOCK_REALTIME], ErrnoReader, "clock_gettime");

        Assert.AreEqual(0, unchecked((int)(uint)r), "the block passed by address behind the leading constant");
        Assert.IsTrue(ts.tv_sec > 1_600_000_000L, $"and filled in place, got {ts.tv_sec}");
        GC.KeepAlive(ts);
    }

    [TestMethod]
    public void TheBlockAddressFormRefusesReferenceBearingStorageByName()
    {
        // Q44: a reference-bearing block hands out its ORDER TOKEN, never an address -- libc must not be
        // handed it (the vDSO would write through it). The refusal is the whole test: its negative is a
        // native fault of the host, which is why this arm carries no neuter.
        //
        // The PREMISE is Q44's token arm (claude/c2-q44-cut, train 30). On a tree without it a
        // reference-bearing box's number is not its order token, the refusal-by-name cannot fire, and
        // handing that number to clock_gettime would write 16 bytes through whatever it denotes -- so
        // the arm is INCONCLUSIVE there by name, never run: measured 2026-09-05 on the lift's own base
        // (5546202355, the Q52 bridge; the number read 140455141593320 against a token of
        // 104361395985842176) and green on the Q44 tree, where the premise holds.
        ref ReferenceBearingArgs block = ref heap<ReferenceBearingArgs>(out ж<ReferenceBearingArgs> Ꮡblock);
        nuint token = (nuint)(uintptr)Ꮡblock;
        if (token != Ꮡblock.PointerOrderToken)
        {
            GC.KeepAlive(block);
            Assert.Inconclusive("the token arm (Q44) is not on this tree: a reference-bearing box's number is not its order token here, so the refusal-by-name cannot be exercised");
        }

        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            GoLibcCall.CallWithBlockAddress(Export("clock_gettime"), token, [(nuint)CLOCK_REALTIME], ErrnoReader, "clock_gettime"));

        StringAssert.Contains(ex.Message, "clock_gettime", "the refusal names the symbol");
        StringAssert.Contains(ex.Message, "order token", "and the reason");
        GC.KeepAlive(block);
    }

    private struct PthreadWord
    {
        internal nuint id;
    }

    [TestMethod]
    public void TheResultIntoBlockForm_PthreadSelfLandsInTheFirstWord()
    {
        nint pthreadSelf = Export("pthread_self");
        nuint direct = GoLibcCall.Call(pthreadSelf, ReadOnlySpan<nuint>.Empty, GoLibcErrnoRule.None, 0, out _);

        ref PthreadWord t = ref heap(new PthreadWord(), out ж<PthreadWord> Ꮡt);
        nuint r = GoLibcCall.CallStoringResultIntoBlock(pthreadSelf, Ꮡt, ErrnoReader, "pthread_self");

        Assert.AreNotEqual((nuint)0, direct, "a thread id is never 0");
        Assert.AreEqual(direct, r, "the form returns the register");
        Assert.AreEqual(direct, t.id, "and stores it into the block's first word, read back through the box");
        GC.KeepAlive(t);
    }
}
