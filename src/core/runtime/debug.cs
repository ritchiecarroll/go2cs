// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// go2cs generated this placeholder — func GOMAXPROCS is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// SetDefaultGOMAXPROCS updates the GOMAXPROCS setting to the runtime
// default, as described by [GOMAXPROCS], ignoring the GOMAXPROCS
// environment variable.
//
// SetDefaultGOMAXPROCS can be used to enable the default automatic updating
// GOMAXPROCS behavior if it has been disabled by the GOMAXPROCS
// environment variable or a prior call to [GOMAXPROCS], or to force an immediate
// update if the caller is aware of a change to the total logical CPU count, CPU
// affinity mask or cgroup quota.
public static void SetDefaultGOMAXPROCS() {
    // SetDefaultGOMAXPROCS conceptually means "[re]do what the runtime
    // would do at startup if the GOMAXPROCS environment variable were
    // unset." It still respects GODEBUG.

    var procs = defaultGOMAXPROCS(0);

    @lock(Ꮡsched.of(schedt.Ꮡlock));
    var curr = gomaxprocs;
    var custom = sched.customGOMAXPROCS;
    unlock(Ꮡsched.of(schedt.Ꮡlock));

    if (!custom && procs == curr) {
        // Nothing to do if we're already using automatic GOMAXPROCS
        // and the limit is unchanged.
        return;
    }

    var stw = stopTheWorldGC(stwGOMAXPROCS);

    // newprocs will be processed by startTheWorld
    //
    // TODO(prattmic): this could use a nicer API. Perhaps add it to the
    // stw parameter?
    newprocs = procs;
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    sched.customGOMAXPROCS = false;
    unlock(Ꮡsched.of(schedt.Ꮡlock));

    startTheWorldGC(stw);
}

// NumCPU returns the number of logical CPUs usable by the current process.
//
// The set of available CPUs is checked by querying the operating system
// at process startup. Changes to operating system CPU allocation after
// process startup are not reflected.
public static nint NumCPU() {
    return (nint)numCPUStartup;
}

// go2cs generated this placeholder — func NumCgoCall is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func totalMutexWaitTimeNanos is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func NumGoroutine is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:linkname debug_modinfo runtime/debug.modinfo
internal static @string debug_modinfo() {
    return modinfo;
}

// mayMoreStackPreempt is a maymorestack hook that forces a preemption
// at every possible cooperative preemption point.
//
// This is valuable to apply to the runtime, which can be sensitive to
// preemption points. To apply this to all preemption points in the
// runtime and runtime-like code, use the following in bash or zsh:
//
//	X=(-{gc,asm}flags={runtime/...,reflect,sync}=-d=maymorestack=runtime.mayMoreStackPreempt) GOFLAGS=${X[@]}
//
// This must be deeply nosplit because it is called from a function
// prologue before the stack is set up and because the compiler will
// call it from any splittable prologue (leading to infinite
// recursion).
//
// Ideally it should also use very little stack because the linker
// doesn't currently account for this in nosplit stack depth checking.
//
// Ensure mayMoreStackPreempt can be called for all ABIs.
//
//go:nosplit
//go:linkname mayMoreStackPreempt
internal static void mayMoreStackPreempt() {
    // Don't do anything on the g0 or gsignal stack.
    var gp = getg();
    if (gp == (~(~gp).m).g0 || gp == (~(~gp).m).gsignal) {
        return;
    }
    // Force a preemption, unless the stack is already poisoned.
    if ((~gp).stackguard0 < stackPoisonMin) {
        gp.Value.stackguard0 = stackPreempt;
    }
}

// mayMoreStackMove is a maymorestack hook that forces stack movement
// at every possible point.
//
// See mayMoreStackPreempt.
//
//go:nosplit
//go:linkname mayMoreStackMove
internal static void mayMoreStackMove() {
    // Don't do anything on the g0 or gsignal stack.
    var gp = getg();
    if (gp == (~(~gp).m).g0 || gp == (~(~gp).m).gsignal) {
        return;
    }
    // Force stack movement, unless the stack is already poisoned.
    if ((~gp).stackguard0 < stackPoisonMin) {
        gp.Value.stackguard0 = stackForceMove;
    }
}

// debugPinnerKeepUnpin is used to make runtime.(*Pinner).Unpin reachable.
internal static bool debugPinnerKeepUnpin = false;

// debugPinnerV1 returns a new Pinner that pins itself. This function can be
// used by debuggers to easily obtain a Pinner that will not be garbage
// collected (or moved in memory) even if no references to it exist in the
// target program. This pinner in turn can be used to extend this property
// to other objects, which debuggers can use to simplify the evaluation of
// expressions involving multiple call injections.
internal static ж<Pinner> debugPinnerV1() {
    var Δp = @new<Pinner>();
    Δp.Pin(@unsafe.Pointer.FromPinnedBox(Δp));
    if (debugPinnerKeepUnpin) {
        // Make Unpin reachable.
        Δp.Unpin();
    }
    return Δp;
}

} // end runtime_package
