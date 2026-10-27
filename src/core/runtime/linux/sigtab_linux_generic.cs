// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !mips && !mipsle && !mips64 && !mips64le && linux
namespace go;

partial class runtime_package {

[GoLazy] partial class lazyᴛsigtable { internal static readonly ж<array<sigTabT>> Ꮡsigtable = new StaticBox<array<sigTabT>>(new sigTabT[]{
    /* 0 */
    new(0, "SIGNONE: no trap"u8),
    /* 1 */
    new(_SigNotify + _SigKill, "SIGHUP: terminal line hangup"u8),
    /* 2 */
    new(_SigNotify + _SigKill, "SIGINT: interrupt"u8),
    /* 3 */
    new(_SigNotify + _SigThrow, "SIGQUIT: quit"u8),
    /* 4 */
    new(_SigThrow + _SigUnblock, "SIGILL: illegal instruction"u8),
    /* 5 */
    new(_SigThrow + _SigUnblock, "SIGTRAP: trace trap"u8),
    /* 6 */
    new(_SigNotify + _SigThrow, "SIGABRT: abort"u8),
    /* 7 */
    new(_SigPanic + _SigUnblock, "SIGBUS: bus error"u8),
    /* 8 */
    new(_SigPanic + _SigUnblock, "SIGFPE: floating-point exception"u8),
    /* 9 */
    new(0, "SIGKILL: kill"u8),
    /* 10 */
    new(_SigNotify, "SIGUSR1: user-defined signal 1"u8),
    /* 11 */
    new(_SigPanic + _SigUnblock, "SIGSEGV: segmentation violation"u8),
    /* 12 */
    new(_SigNotify, "SIGUSR2: user-defined signal 2"u8),
    /* 13 */
    new(_SigNotify, "SIGPIPE: write to broken pipe"u8),
    /* 14 */
    new(_SigNotify, "SIGALRM: alarm clock"u8),
    /* 15 */
    new(_SigNotify + _SigKill, "SIGTERM: termination"u8),
    /* 16 */
    new(_SigThrow + _SigUnblock, "SIGSTKFLT: stack fault"u8),
    /* 17 */
    new(_SigNotify + _SigUnblock + _SigIgn, "SIGCHLD: child status has changed"u8),
    /* 18 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGCONT: continue"u8),
    /* 19 */
    new(0, "SIGSTOP: stop, unblockable"u8),
    /* 20 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTSTP: keyboard stop"u8),
    /* 21 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTTIN: background read from tty"u8),
    /* 22 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTTOU: background write to tty"u8),
    /* 23 */
    new(_SigNotify + _SigIgn, "SIGURG: urgent condition on socket"u8),
    /* 24 */
    new(_SigNotify, "SIGXCPU: cpu limit exceeded"u8),
    /* 25 */
    new(_SigNotify, "SIGXFSZ: file size limit exceeded"u8),
    /* 26 */
    new(_SigNotify, "SIGVTALRM: virtual alarm clock"u8),
    /* 27 */
    new(_SigNotify + _SigUnblock, "SIGPROF: profiling alarm clock"u8),
    /* 28 */
    new(_SigNotify + _SigIgn, "SIGWINCH: window size change"u8),
    /* 29 */
    new(_SigNotify, "SIGIO: i/o now possible"u8),
    /* 30 */
    new(_SigNotify, "SIGPWR: power failure restart"u8),
    /* 31 */
    new(_SigThrow, "SIGSYS: bad system call"u8),
    /* 32 */
    new(_SigSetStack + _SigUnblock, "signal 32"u8), /* SIGCANCEL; see issue 6997 */
    /* 33 */
    new(_SigSetStack + _SigUnblock, "signal 33"u8), /* SIGSETXID; see issues 3871, 9400, 12498 */
    /* 34 */
    new(_SigSetStack + _SigUnblock, "signal 34"u8), /* musl SIGSYNCCALL; see issue 39343 */
    /* 35 */
    new(_SigNotify, "signal 35"u8),
    /* 36 */
    new(_SigNotify, "signal 36"u8),
    /* 37 */
    new(_SigNotify, "signal 37"u8),
    /* 38 */
    new(_SigNotify, "signal 38"u8),
    /* 39 */
    new(_SigNotify, "signal 39"u8),
    /* 40 */
    new(_SigNotify, "signal 40"u8),
    /* 41 */
    new(_SigNotify, "signal 41"u8),
    /* 42 */
    new(_SigNotify, "signal 42"u8),
    /* 43 */
    new(_SigNotify, "signal 43"u8),
    /* 44 */
    new(_SigNotify, "signal 44"u8),
    /* 45 */
    new(_SigNotify, "signal 45"u8),
    /* 46 */
    new(_SigNotify, "signal 46"u8),
    /* 47 */
    new(_SigNotify, "signal 47"u8),
    /* 48 */
    new(_SigNotify, "signal 48"u8),
    /* 49 */
    new(_SigNotify, "signal 49"u8),
    /* 50 */
    new(_SigNotify, "signal 50"u8),
    /* 51 */
    new(_SigNotify, "signal 51"u8),
    /* 52 */
    new(_SigNotify, "signal 52"u8),
    /* 53 */
    new(_SigNotify, "signal 53"u8),
    /* 54 */
    new(_SigNotify, "signal 54"u8),
    /* 55 */
    new(_SigNotify, "signal 55"u8),
    /* 56 */
    new(_SigNotify, "signal 56"u8),
    /* 57 */
    new(_SigNotify, "signal 57"u8),
    /* 58 */
    new(_SigNotify, "signal 58"u8),
    /* 59 */
    new(_SigNotify, "signal 59"u8),
    /* 60 */
    new(_SigNotify, "signal 60"u8),
    /* 61 */
    new(_SigNotify, "signal 61"u8),
    /* 62 */
    new(_SigNotify, "signal 62"u8),
    /* 63 */
    new(_SigNotify, "signal 63"u8),
    /* 64 */
    new(_SigNotify, "signal 64"u8)
}.array()); }
internal static ref array<sigTabT> sigtable => ref ((StaticBox<array<sigTabT>>)Ꮡsigtable).ValueSlot;

} // end runtime_package
