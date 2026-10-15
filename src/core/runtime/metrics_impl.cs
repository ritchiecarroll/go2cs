// metrics_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// schedStatsAggregate.compute (metrics.go, Go 1.26: the scheduler metrics behind /sched/goroutines:*),
// hand-owned for ONE line, the runqempty precedent (runtime2_impl.cs) at a second site.
//
// Go counts a P's runnext slot as one more runnable goroutine by reading it as the number it hides:
// `next := atomic.Loaduintptr((*uintptr)(&p.runnext))`, then `if next != 0`. The managed guintptr holds the
// ж<g> box itself, so that view has no referent: golib refuses the reinterpret because the pointee
// carries a reference, the conversion falls to the address route, and Loaduintptr's Volatile.Read
// dereferences an order token (arm-2a; TestNoReinterpretOfAManagedReferentPointerType names the
// converted emission, which reinterprets the Δguintptr as a uintptr). All Go asks of the number is whether it is 0,
// i.e. whether runnext is nil, which here is the managed reference being null, read with the same
// acquire. Everything else below is the converted body, unchanged, so the scheduler's own accounting
// (the lock, the thread count, the per-P walk, the global queue, the syscall count) stays Go's.
//
// Hand-owned: registered in manualConversionFuncs["runtime"]["schedStatsAggregate.compute"], so a -stdlib
// reconvert emits the declaration as a placeholder and never regenerates the body over this file.

using System.Threading;
using atomic = go.@internal.runtime.atomic_package;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // compute populates the schedStatsAggregate with values from the runtime.
    internal static void compute(this ref schedStatsAggregate a) {
        // Lock the scheduler so the global run queue can't change and
        // the number of Ps can't change. This doesn't prevent the
        // local run queues from changing, so the results are still
        // approximate.
        @lock(Ꮡsched.of(schedt.Ꮡlock));

        // The total count of threads owned by Go is the number of Ms
        // minus extra Ms on the list or in use.
        a.threads = (uint64)mcount() - (uint64)ᏑextraMInUse.Load() - (uint64)ᏑextraMLength.Load();

        // Collect running/runnable from per-P run queues.
        a.gCreated += Ꮡsched.of(schedt.ᏑgoroutinesCreated).Load();
        foreach (var (_, Δp) in allp) {
            if (Δp == nil || (~Δp).status == _Pdead) {
                break;
            }
            a.gCreated += Δp.Value.goroutinesCreated;
            var exprᴛ1 = (~Δp).status;
            if (exprᴛ1 == _Prunning) {
                {
                    var (thread, ok) = setBlockOnExitSyscall(Δp); if (ok){
                        thread.resume();
                        a.gNonGo++;
                    } else {
                        a.gRunning++;
                    }
                }
            }
            else if (exprᴛ1 == _Pgcstop) {
                // The world is stopping or stopped.
                // This is fine. The results will be
                // slightly odd since nothing else
                // is running, but it will be accurate.
            }

            while (true) {
                var h = atomic.Load(Δp.of(runtime_package.Δp.Ꮡrunqhead));
                var t = atomic.Load(Δp.of(runtime_package.Δp.Ꮡrunqtail));
                // Go: `next := atomic.Loaduintptr((*uintptr)(&p.runnext))`, read only for `next != 0`. The
                // managed reference, read with the same acquire, answers that without a numeric view.
                var runnextSet = Volatile.Read(ref (~Δp).runnext.m_ref) is not null;
                var runnable = (int32)(t - h);
                if (atomic.Load(Δp.of(runtime_package.Δp.Ꮡrunqhead)) != h || runnable < 0) {
                    continue;
                }
                if (runnextSet) {
                    runnable++;
                }
                a.gRunnable += (uint64)runnable;
                break;
            }
        }

        // Global run queue.
        a.gRunnable += (uint64)sched.runq.size;

        // Account for Gs that are in _Gsyscall without a P.
        var nGsyscallNoP = Ꮡsched.of(schedt.ᏑnGsyscallNoP).Load();

        // nGsyscallNoP can go negative during temporary races.
        if (nGsyscallNoP >= 0) {
            a.gNonGo += (uint64)nGsyscallNoP;
        }

        // Compute the number of blocked goroutines. We have to
        // include system goroutines in this count because we included
        // them above.
        a.gTotal = (uint64)gcount(true);
        a.gWaiting = a.gTotal - (a.gRunning + a.gRunnable + a.gNonGo);
        if (a.gWaiting < 0) {
            a.gWaiting = 0;
        }

        unlock(Ꮡsched.of(schedt.Ꮡlock));
    }
}
