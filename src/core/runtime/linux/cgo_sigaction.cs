// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Support for sanitizers. See runtime/cgo/sigaction.go.
// Also used on linux/386 to clear the SA_RESTORER flag
// when using cgo; see issue #75253.
//go:build (linux && (386 || amd64 || arm64 || loong64 || ppc64le)) || (freebsd && amd64)
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// _cgo_sigaction is filled in by runtime/cgo when it is linked into the
// program, so it is only non-nil when using cgo.
//
//go:linkname _cgo_sigaction _cgo_sigaction
internal static @unsafe.Pointer _cgo_sigaction;

//go:nosplit
//go:nowritebarrierrec
internal static void sigaction(uint32 sigʗp, ж<sigactiont> @new, ж<sigactiont> old) {
    ref var sig = ref heap(sigʗp, out var Ꮡsig);

    // racewalk.go avoids adding sanitizing instrumentation to package runtime,
    // but we might be calling into instrumented C functions here,
    // so we need the pointer parameters to be properly marked.
    //
    // Mark the input as having been written before the call
    // and the output as read after.
    if (msanenabled && @new != nil) {
        msanwrite(@unsafe.Pointer.FromPinnedBox(@new), /* unsafe.Sizeof(*new) */ (uintptr)32);
    }
    if (asanenabled && @new != nil) {
        asanwrite(@unsafe.Pointer.FromPinnedBox(@new), /* unsafe.Sizeof(*new) */ (uintptr)32);
    }
    if (_cgo_sigaction == nil || inForkedChild) {
        sysSigaction(sig, @new, old);
    } else {
        // We need to call _cgo_sigaction, which means we need a big enough stack
        // for C.  To complicate matters, we may be in libpreinit (before the
        // runtime has been initialized) or in an asynchronous signal handler (with
        // the current thread in transition between goroutines, or with the g0
        // system stack already in use).

        int32 ret = default;

        fixSigactionForCgo(@new);

        ж<g> g = default;
        if (mainStarted) {
            g = getg();
        }
        var sp = (uintptr)Ꮡsig;
        switch (ᐧ) {
        case {} when g == nil: {
            // No g: we're on a C stack or a signal stack.
            ret = callCgoSigaction((uintptr)sig, @new, old);
            break;
        }
        case {} when sp < (~g).stack.lo || sp >= (~g).stack.hi: {
            // We're no longer on g's stack, so we must be handling a signal.  It's
            // possible that we interrupted the thread during a transition between g
            // and g0, so we should stay on the current stack to avoid corrupting g0.
            ret = callCgoSigaction((uintptr)sig, @new, old);
            break;
        }
        default: {
            // We're running on g's stack, so either we're not in a signal handler or
            // the signal handler has set the correct g.  If we're on gsignal or g0,
            // systemstack will make the call directly; otherwise, it will switch to
            // g0 to ensure we have enough room to call a libc function.
            //
            // The function literal that we pass to systemstack is not nosplit, but
            // that's ok: we'll be running on a fresh, clean system stack so the stack
            // check will always succeed anyway.
            systemstack(() => {
                ret = callCgoSigaction((uintptr)Ꮡsig.Value, @new, old);
            });
            break;
        }}

        const int32 EINVAL = 22;
        if (ret == EINVAL) {
            // libc reserves certain signals — normally 32-33 — for pthreads, and
            // returns EINVAL for sigaction calls on those signals.  If we get EINVAL,
            // fall back to making the syscall directly.
            sysSigaction(sig, @new, old);
        }
    }

    if (msanenabled && old != nil) {
        msanread(@unsafe.Pointer.FromPinnedBox(old), /* unsafe.Sizeof(*old) */ (uintptr)32);
    }
    if (asanenabled && old != nil) {
        asanread(@unsafe.Pointer.FromPinnedBox(old), /* unsafe.Sizeof(*old) */ (uintptr)32);
    }
}

// callCgoSigaction calls the sigaction function in the runtime/cgo package
// using the GCC calling convention. It is implemented in assembly.
//
//go:noescape
internal static partial int32 callCgoSigaction(uintptr sig, ж<sigactiont> @new, ж<sigactiont> old);

} // end runtime_package
