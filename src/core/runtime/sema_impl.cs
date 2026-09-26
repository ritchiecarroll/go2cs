// sema_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The runtime's own semaphore, runtime.semacquire1 / runtime.semrelease1 (sema.go).
//
// WHY. The converted semacquire1 parks the caller through Go's sudog machinery: acquireSudog reads the
// caller's P (`mp.p.ptr()`) for its per-P sudog cache. The managed runtime has no Ps -- every goroutine
// is its own thread and m.p is nil by construction (stubs_impl.cs, getg) -- so the first semacquire that
// has to WAIT dereferenced a nil P on a goroutine and the runtime row's test host died there
// (TestSemaHandoff, measured on linux 2026-09-24: 10,669 of 10,884 results, 124 top-level tests unrun).
//
// This file holds the guard's probe only; the bodies follow in the next commit.
//
// Hand-owned (no sema_impl.go exists, so a reconvert never regenerates this file).

using System;
using System.Linq;
using go.golib;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // ---- the guard's view (RuntimeSemaphoreTests) ----

    /// <summary>
    /// A goroutine acquires a zero semaphore through runtime's own semacquire, and this thread releases
    /// it with handoff through runtime's semrelease1. Returns whether the acquirer came back holding the
    /// permit, and the acquirer's failure by name if it died instead.
    /// </summary>
    public static (bool acquired, string? acquirerFailure) GoSemacquireReleaseProbe(int timeoutMs)
    {
        using System.Threading.ManualResetEventSlim done = new(false);
        ж<uint32> Ꮡsema = new StandardBox<uint32>(default(uint32));
        bool acquired = false;
        string? acquirerFailure = null;

        Goroutine.Start(() =>
        {
            try
            {
                semacquire(Ꮡsema);
                acquired = true;
            }
            catch (Exception ex)
            {
                // Name the runtime frame it died in: a bare NullReferenceException does not say where.
                string? frame = new System.Diagnostics.StackTrace(ex).GetFrames()
                    .Select(f => f.GetMethod())
                    .FirstOrDefault(m => m?.DeclaringType == typeof(runtime_package))?.Name;

                acquirerFailure = $"{ex.GetType().Name} in runtime.{frame ?? "?"}: {ex.Message}";
            }

            done.Set();
        });

        // The acquirer either parks (the release then wakes it) or has not reached the semaphore yet
        // (the release then leaves a permit it takes without parking). Both end with it holding the
        // permit; only a failure on its way in ends earlier, and that is what this wait watches for.
        done.Wait(200);

        if (acquirerFailure is not null)
            return (false, acquirerFailure);

        semrelease1(Ꮡsema, true, 0);

        if (!done.Wait(timeoutMs))
            throw new TimeoutException("the acquirer never returned from semacquire after the release");

        return (acquired, acquirerFailure);
    }
}
