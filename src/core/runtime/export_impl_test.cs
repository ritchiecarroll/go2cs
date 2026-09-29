// export_impl_test.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-owned bodies for runtime's export_test.go -- the TEST-file companion (the `*_impl_test.cs`
// convention reflect's export_impl_test.cs set: the `_test.cs` suffix keeps it out of the production
// csproj, and testConversion globs it into runtime.tests.csproj).
//
// SemNwait (export_test.go) reads semtable.rootFor(addr).nwait, the semaRoot treap's waiter count. The
// runtime's semaphore is RuntimeSemaphore now (sema_impl.cs) and nothing maintains the treap, so the
// converted read would answer 0 forever and TestSemaHandoff's `for SemNwait(&sema) == 0 { Gosched() }`
// would spin for good. The same question is answered from the queue that does exist.
//
// UserArena.New (export_test.go) reads the interface's type word and writes its data word through
// efaceOf, which is inert here: an `any` is one object reference, not an {_type, data} pair, so the
// converted body saw a nil type and its result could never reach the caller. The type is the
// interface value's dynamic type, read through abi's bridge exactly as the arena reads it
// (arena_impl.cs), and the allocation is stored into the interface itself.
//
// Three helpers REFUSE BY NAME at their representational point (coordinator ruling 2026-09-28 22:01,
// the P1 disclosure seat), so the tests that reach them fail on a text that says why rather than on an
// anonymous nil dereference or an intrinsic stub. KeepNArenaHints walks mheap_.arenaHints, which the
// managed model never builds because it has no Go heap arenas. TracebackSystemstack recurses through
// systemstack and unwinds from sys.GetCallerPC/GetCallerSP, expecting the traceback to cross the
// system-stack switch; a goroutine here is a CLR thread with one stack, so there is no switch to
// cross. G0StackOverflow shrinks g0's stack bounds and recurses until morestack reports the overflow
// on g0; a CLR thread has no g0 stack. Q53 (docs/phase4/DESIGN-getcallerpc.md) would give the two
// intrinsics bodies, which moves nothing here: the refusals sit behind them, at the missing stacks.
//
// Hand-owned (no export_impl_test.go exists, so a reconvert never regenerates this file).

namespace go;

using abi = global::go.@internal.abi_package;
using static global::go.runtime_package;

partial class runtime_internal_test_package
{
    public static uint32 SemNwait(ж<uint32> Ꮡaddr) => GoSemaWaiters(Ꮡaddr);

    [GoRecv] public static void New(this ref UserArena a, ж<any> Ꮡout)
    {
        ж<abi.Type> typ = abi.TypeOf(Ꮡout.Value);

        if (typ == nil || (abiꓸKind)((~typ).Kind_ & abi.KindMask) != abi.Pointer) {
            throw panic("new result of non-ptr type");
        }

        Ꮡout.Value = userArenaNew(ref a.arena.Value, abi.Elem(typ));
    }

    public static void KeepNArenaHints(nint n) =>
        throw panic("runtime: KeepNArenaHints: the managed host has no Go heap arenas, so there are no arena hints to keep");

    public static nint TracebackSystemstack(slice<uintptr> stk, nint i) =>
        throw panic("runtime: TracebackSystemstack: goroutines are CLR threads with one stack, so no traceback crosses a system-stack switch");

    public static void G0StackOverflow() =>
        throw panic("runtime: G0StackOverflow: goroutines are CLR threads with no g0 stack to overflow");
}
