// deadline_prepay_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written: pays the first read deadline's one-time costs when the poller starts, not when that
// deadline expires. Go's runtime is compiled ahead of time, so a Go program's first deadline returns
// as promptly as its later ones (0.3-0.5 ms late on linux). Here the first one also compiled its path
// on the way out: 9-11 ms late under the JIT and 6-8 ms under ReadyToRun, measured on linux by the
// first-deadline probe (branch claude/p2-first-deadline-probe). Under ReadyToRun every method still
// compiled on that path is a golib generic instantiated over this package's types (the fdMutex field
// view) or io's package-init boxes, which ReadyToRun does not pregenerate across assemblies.
//
// What runs, once per process, inside the poller's start (each seam's runtime_pollServerInit, which
// the first pollable FD.Init reaches through serverInit.Do):
//   - RuntimeHelpers.PrepareMethod over this package's arm, park, fire and return methods: each seam's
//     runtime_pollSetDeadline / pollBlock / deadlineFired family, and FD.Read down to pollDesc.wait;
//   - three calls on a spare, never-opened FD, legal in Go and touching no descriptor: SetReadDeadline
//     of the zero Time (incref/decref, then ErrNoDeadline, because no poll descriptor is attached),
//     readLock/readUnlock, and eofError -- they compile, and under ReadyToRun instantiate, the field
//     view of fd.fdmu that incref, decref and the lock wrappers reach;
//   - io's package initialization, which Go has always completed before internal/poll's;
//   - one no-op GoTimer two milliseconds out, so the timer service's wait, sleep and run path has run
//     once before a deadline needs it;
//   - on windows, the cancel and the aborted completion that follow a deadline's wake there
//     (windows/deadline_prepay_windows_impl.cs);
//   - any hook a package above this one registered for its own part of the path (os's return path).
// Nothing here arms a deadline or waits on a descriptor. Under Native AOT nothing is compiled at run
// time, so none of it runs.
//
// Go has no such step, so nothing in it may fail the FD.Init that started the poller. This package's
// part, the platform part and each hook run in their own try, and an exception ends only that one; its
// costs then fall on the first deadline, as they did before this file.
//
// It runs on the starting thread, so the first FD.Init pays it: about 10 ms under the JIT and under
// ReadyToRun alike, once per process, and only in a process that opens a pollable descriptor. Run on
// a background thread instead, it took that time off FD.Init but left the first deadline 4-5 ms late
// when the deadline was armed right after the poller started (the probe's cold arm), against Go's
// 0.3-0.5 ms; run here, that deadline reads 0.3-1.6 ms late.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GoTimer = go.golib.GoTimer;
using MonotonicClock = go.golib.MonotonicClock;

[module: go.GoManualConversion]

namespace go.@internal;

partial class poll_package
{
    // A package above internal/poll (os) adds its own part of the deadline path here, from its module
    // initializer. Each hook runs once, inside the poller's start, after this package's part. Go's
    // internal/poll has no such member. It is public only because os is another assembly and the corpus
    // grants no internals between converted packages, so it is hidden from editor completion instead.
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static event Action? DeadlinePathPrepay;

    // Called once from each flavor's runtime_pollServerInit, after the poller is running.
    private static void prepayDeadlinePath()
    {
        if (!RuntimeFeature.IsDynamicCodeCompiled)
            return;

        attempt(prepayOwnDeadlinePath);
        attempt(() => prepayPlatformDeadlinePath());

        if (DeadlinePathPrepay is not { } hooks)
            return;

        foreach (Action hook in hooks.GetInvocationList())
            attempt(hook);
    }

    private static void prepayOwnDeadlinePath()
    {
        GoTimer warmTimer = new(static _ => { }, null);
        warmTimer.Reset(MonotonicClock.Nanoseconds() + 2_000_000);

        prepare(
            runtime_pollSetDeadline, applyDeadlineLocked, armDeadlineLocked, stopDeadlineLocked, deadlineFired,
            runtime_pollReset, runtime_pollWait, pollBlock, modeState, runtimeNano, convertErr, Read,
            (pollDescFileFunc)prepareRead, (pollDescFileFunc)waitRead, (pollDescWaitFunc)wait,
            (pollDescPollableFunc)pollable);

        ж<FD> spare = Ꮡ(new FD());
        spare.SetReadDeadline(default);
        spare.readLock();
        spare.readUnlock();
        spare.DerefOrNull().eofError(0, ErrDeadlineExceeded);

        RuntimeHelpers.RunClassConstructor(typeof(io_package).TypeHandle);
    }

    // The part of the path only one flavor has. Only windows implements it, so elsewhere the call
    // compiles to nothing.
    static partial void prepayPlatformDeadlinePath();

    private static void attempt(Action part)
    {
        try
        {
            part();
        }
        catch (Exception)
        {
            // Leaves this part's costs to the first deadline.
        }
    }

    // The by-reference pollDesc methods FD.Read calls. Each has a ж<pollDesc> twin from the receiver
    // generator, so its name alone does not say which body to prepare.
    private delegate error pollDescFileFunc(ref pollDesc pd, bool isFile);
    private delegate error pollDescWaitFunc(ref pollDesc pd, nint mode, bool isFile);
    private delegate bool pollDescPollableFunc(ref pollDesc pd);

    private static void prepare(params Delegate[] methods)
    {
        foreach (Delegate method in methods)
            RuntimeHelpers.PrepareMethod(method.Method.MethodHandle);
    }
}
