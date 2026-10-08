// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package debug contains facilities for programs to debug themselves while
// they are running.
namespace go.runtime;

using poll = @internal.poll_package;
using os = os_package;
using runtime = runtime_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;
using System.Runtime.CompilerServices;

partial class debug_package {

// PrintStack prints to standard error the stack trace returned by runtime.Stack.
public static partial void PrintStack() {
    os.Stderr.Write(Stack());
}

// Stack returns a formatted stack trace of the goroutine that calls it.
// It calls [runtime.Stack] with a large enough buffer to capture the entire trace.
public static partial slice<byte> Stack() {
    var buf = new slice<byte>(1024);
    while (ᐧ) {
        nint n = runtime.Stack(buf, false);
        if (n < len(buf)) {
            return buf.slice(0, n);
        }
        buf = new slice<byte>(2 * len(buf));
    }
}

// CrashOptions provides options that control the formatting of the
// fatal crash message.
partial struct CrashOptions {
}

/* for future expansion */

// SetCrashOutput configures a single additional file where unhandled
// panics and other fatal errors are printed, in addition to standard error.
// There is only one additional file: calling SetCrashOutput again overrides
// any earlier call.
// SetCrashOutput duplicates f's file descriptor, so the caller may safely
// close f as soon as SetCrashOutput returns.
// To disable this additional crash output, call SetCrashOutput(nil).
// If called concurrently with a crash, some in-progress output may be written
// to the old file even after an overriding SetCrashOutput returns.
public static error SetCrashOutput(ж<os.File> Ꮡf, CrashOptions opts) {
    ref var f = ref Ꮡf.DerefOrNull();

    var fd = ~(uintptr)0;
    if (Ꮡf != nil) {
        // The runtime will write to this file descriptor from
        // low-level routines during a panic, possibly without
        // a G, so we must call f.Fd() eagerly. This creates a
        // danger that the file descriptor is no longer
        // valid at the time of the write, because the caller
        // (incorrectly) called f.Close() and the kernel
        // reissued the fd in a later call to open(2), leading
        // to crashes being written to the wrong file.
        //
        // So, we duplicate the fd to obtain a private one
        // that cannot be closed by the user.
        // This also alleviates us from concerns about the
        // lifetime and finalization of f.
        // (DupCloseOnExec returns an fd, not a *File, so
        // there is no finalizer, and we are responsible for
        // closing it.)
        //
        // The new fd must be close-on-exec, otherwise if the
        // crash monitor is a child process, it may inherit
        // it, so it will never see EOF from the pipe even
        // when this process crashes.
        //
        // A side effect of Fd() is that it calls SetBlocking,
        // which is important so that writes of a crash report
        // to a full pipe buffer don't get lost.
        var (fd2, _, err) = poll.DupCloseOnExec((nint)Ꮡf.Fd());
        if (err != default!) {
            return err;
        }
        runtime.KeepAlive(Ꮡf.OrTypedNil()); // prevent finalization before dup
        fd = (uintptr)fd2;
    }
    {
        var prev = runtime_setCrashFD(fd); if (prev != ~(uintptr)0) {
            // We use NewFile+Close because it is portable
            // unlike syscall.Close, whose parameter type varies.
            os.NewFile(prev, ""u8).Close(); // ignore error
        }
    }
    return default!;
}

//go:linkname runtime_setCrashFD runtime.setCrashFD
internal static partial uintptr runtime_setCrashFD(uintptr _);

} // end debug_package
