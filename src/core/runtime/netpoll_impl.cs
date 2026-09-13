// netpoll_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The runtime network poller's one-time start-up, under a managed host that does not own a poller.
//
// WHERE THE POLLER ACTUALLY LIVES. internal/poll's ten //go:linkname entry points into the runtime
// are hand-owned on a managed poller (internal/poll/windows/runtime_netpoll_impl.cs, ruled
// 2026-08-13 in docs/phase4/DESIGN-netpoll-managed-poller.md §9). That design states, in terms, that
// runtime/netpoll.cs and runtime/<goos>/netpoll_*.cs "stay converted and DEAD; zero runtime edits" —
// and its reasoning is the reason this file is one function long rather than a poller: behind
// netpollinit sit runtime mutexes with lock-rank bookkeeping, pollcache over persistentalloc, the
// runtime timer engine, gopark with a commit callback, and g-pointer CAS protocols, none of which
// exists under the CLR. Decisively, Go's poller is only HALF an API — netpoll(delta), netpollBreak
// and netpollready are called by the SCHEDULER itself, from findRunnable and sysmon — so a
// perfectly-wired conversion would initialize a completion port that nothing ever drains, because
// the thread Go dedicates to draining it IS the scheduler.
//
// WHY THERE IS AN EDIT HERE ANYWAY. That ruling's scope was internal/poll's consumers. It did not
// cover runtime's OWN test suite, which reaches this function directly: export_test.go re-exports it
// as runtime.NetpollGenericInit, and netpoll_os_test.go calls it from a package-level init(). The
// converted test host therefore threw
//
//     System.NotImplementedException: getg: external (assembly or cgo) function is not implemented
//
// inside its static constructor, before a single test executed — netpollGenericInit → netpollinit →
// stdcall4 → getg() — and every verdict in the comparison came back empty, which is the whole-host
// mass-empty shape rather than a test failure. This file is that ruling's one narrow amendment and
// its entire extent: ONE function. Everything the design was actually about is untouched.
//
// WHY A NO-OP IS THE TRUE BODY. Two independent facts, both measured at the windows target:
//
//   1. Nothing reads the state this function would publish. `netpollinited()` has SIX call sites and
//      all six are proc.cs — findRunnable, sysmon, injectglist, stopm, checkdead — the scheduler the
//      managed host never enters. The single read of netpollInited outside netpoll.cs is time.cs's
//      runtime timer heap, which the managed model also does not run.
//
//   2. The honest behaviour is delivered by GO'S OWN GUARD, not by anything invented here. All three
//      flavors open netpoll(delay) with "the poller object was never created → return an empty
//      gList": iocphandle == _INVALID_HANDLE_VALUE (windows/netpoll_windows.cs), epfd == -1
//      (linux/netpoll_epoll.cs), kq == -1 (darwin/netpoll_kqueue.cs). Declining to create it leaves
//      each flavor's poll in its own already-correct nothing-to-report branch — which is the true
//      statement under go2cs, since no goroutine is ever parked on the runtime's poller and so none
//      can ever become runnable through it.
//
// That is the difference between an honest equivalence and a silencer: this does not suppress a
// failure, it declines to build an object with no reader and no drainer, and every downstream path
// then takes a branch Go itself wrote for exactly that condition.
//
// netpollInited IS DELIBERATELY LEFT AT ZERO. Setting it would assert "the runtime poller is up"
// while iocphandle stays invalid — an incoherent pair a later reader of that dead code would have to
// untangle. The truthful answer is that the RUNTIME's poller is not up and never will be. Polling is
// available to the corpus; it is simply not provided from here.
//
// NOT IMPLEMENTED HERE, ON PURPOSE:
//   netpollBreak — its auto body reaches stdcall4 → getg and keeps throwing, so proc_test.go's
//                  TestNetpollBreak fails as ONE loud, locatable row rather than going quietly
//                  green. That is stubs_impl.cs's standing rule for an unported path ("every other
//                  assembly stub deliberately keeps throwing, so an unported path fails loudly
//                  instead of silently doing nothing"), and it is the right outcome for a test whose
//                  premise — a poller wait that a break interrupts — the managed model does not
//                  have. Making it a no-op would buy a green that means nothing.
//   netpoll      — needs no hand-own at all: its own guard (fact 2 above) already returns the empty
//                  list on every flavor once init declines to create the poller.
//
// Hand-owned: there is no netpoll_impl.go, so a -stdlib reconvert never regenerates this file. The
// converter drops the auto form of netpollGenericInit (manualConversionFuncs["runtime"] in
// go2cs/manualTypeOperations.go), leaving a placeholder comment at the site in netpoll.cs.

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // netpollGenericInit is Go's idempotent "make sure the poller has started". Under go2cs there is
    // no runtime-side poller to start — see this file's header for why that is a true statement
    // rather than a convenient one — so the whole body is the absence of one.
    //
    // Go's body takes netpollInitLock and double-checks netpollInited before calling netpollinit().
    // Both are dropped WITH the work they guard: with nothing to initialize there is no
    // initialize-once to protect, and the lock's own lockInit/lockRankNetpollInit bookkeeping is
    // runtime-mutex machinery (lock_managed_impl.cs's territory) that would be acquired here purely
    // to guard a no-op. The function stays idempotent for the trivial reason.
    internal static void netpollGenericInit() {
    }
}
