// netpoll_kqueue_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The darwin runtime poller's break, under a managed host that never creates the poller.
//
// netpollBreak triggers the EVFILT_USER event netpollinit registers on kq, through wakeNetpoll(kq).
// netpollGenericInit is a no-op here (netpoll_impl.cs: there is no runtime-side poller and nothing to
// drain it), so kq stays -1. Once kevent's result protocol was restored (libccall_impl.cs, 2026-10-26),
// the converted body's kevent answered -EBADF and wakeNetpoll took Go's throw: "fatal error: runtime:
// netpollBreak write failed", which ended the darwin runtime row's test host at TestNetpollBreak and
// took 34 in-flight tests with it (mac run 40259307336). That is the path linux met once write1 had a
// body (linux/netpoll_epoll_impl.cs).
//
// netpollBreak therefore refuses by name, before the kevent, exactly as linux's and windows' do, and
// with the same leading words, since the cause is the same: the runtime poller the break would
// interrupt does not exist. Only the parenthetical names each flavour's missing object.
// TestNetpollBreak keeps its one loud, locatable failure, a Go panic, and the host lives.
//
// Hand-owned (no netpoll_kqueue_impl.go exists, so a reconvert never regenerates this file). The
// converter drops the auto form (manualConversionFuncs["runtime"], "netpollBreak" scoped to linux,
// windows and darwin in go2cs/manualTypeOperations.go).

using go.golib;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    internal static void netpollBreak() =>
        throw new PanicException("runtime: netpollBreak: the managed host has no runtime poller to break (netpollGenericInit creates no kqueue; see netpoll_impl.cs)");
}
