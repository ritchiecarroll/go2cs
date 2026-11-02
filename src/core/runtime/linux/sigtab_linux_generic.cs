// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !mips && !mipsle && !mips64 && !mips64le && linux
namespace go;

partial class runtime_package {

[GoLazy] partial class lazyᴛsigtable { internal static readonly ж<array<sigTabT>> Ꮡsigtable = new StaticBox<array<sigTabT>>(new sigTabT[]{
    /* 0 */
    new(0, "SIGNONE: no trap"),
    /* 1 */
    new(_SigNotify + _SigKill, "SIGHUP: terminal line hangup"),
    /* 2 */
    new(_SigNotify + _SigKill, "SIGINT: interrupt"),
    /* 3 */
    new(_SigNotify + _SigThrow, "SIGQUIT: quit"),
    /* 4 */
    new(_SigThrow + _SigUnblock, "SIGILL: illegal instruction"),
    /* 5 */
    new(_SigThrow + _SigUnblock, "SIGTRAP: trace trap"),
    /* 6 */
    new(_SigNotify + _SigThrow, "SIGABRT: abort"),
    /* 7 */
    new(_SigPanic + _SigUnblock, "SIGBUS: bus error"),
    /* 8 */
    new(_SigPanic + _SigUnblock, "SIGFPE: floating-point exception"),
    /* 9 */
    new(0, "SIGKILL: kill"),
    /* 10 */
    new(_SigNotify, "SIGUSR1: user-defined signal 1"),
    /* 11 */
    new(_SigPanic + _SigUnblock, "SIGSEGV: segmentation violation"),
    /* 12 */
    new(_SigNotify, "SIGUSR2: user-defined signal 2"),
    /* 13 */
    new(_SigNotify, "SIGPIPE: write to broken pipe"),
    /* 14 */
    new(_SigNotify, "SIGALRM: alarm clock"),
    /* 15 */
    new(_SigNotify + _SigKill, "SIGTERM: termination"),
    /* 16 */
    new(_SigThrow + _SigUnblock, "SIGSTKFLT: stack fault"),
    /* 17 */
    new(_SigNotify + _SigUnblock + _SigIgn, "SIGCHLD: child status has changed"),
    /* 18 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGCONT: continue"),
    /* 19 */
    new(0, "SIGSTOP: stop, unblockable"),
    /* 20 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTSTP: keyboard stop"),
    /* 21 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTTIN: background read from tty"),
    /* 22 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTTOU: background write to tty"),
    /* 23 */
    new(_SigNotify + _SigIgn, "SIGURG: urgent condition on socket"),
    /* 24 */
    new(_SigNotify, "SIGXCPU: cpu limit exceeded"),
    /* 25 */
    new(_SigNotify, "SIGXFSZ: file size limit exceeded"),
    /* 26 */
    new(_SigNotify, "SIGVTALRM: virtual alarm clock"),
    /* 27 */
    new(_SigNotify + _SigUnblock, "SIGPROF: profiling alarm clock"),
    /* 28 */
    new(_SigNotify + _SigIgn, "SIGWINCH: window size change"),
    /* 29 */
    new(_SigNotify, "SIGIO: i/o now possible"),
    /* 30 */
    new(_SigNotify, "SIGPWR: power failure restart"),
    /* 31 */
    new(_SigThrow, "SIGSYS: bad system call"),
    /* 32 */
    new(_SigSetStack + _SigUnblock, "signal 32"), /* SIGCANCEL; see issue 6997 */
    /* 33 */
    new(_SigSetStack + _SigUnblock, "signal 33"), /* SIGSETXID; see issues 3871, 9400, 12498 */
    /* 34 */
    new(_SigSetStack + _SigUnblock, "signal 34"), /* musl SIGSYNCCALL; see issue 39343 */
    /* 35 */
    new(_SigNotify, "signal 35"),
    /* 36 */
    new(_SigNotify, "signal 36"),
    /* 37 */
    new(_SigNotify, "signal 37"),
    /* 38 */
    new(_SigNotify, "signal 38"),
    /* 39 */
    new(_SigNotify, "signal 39"),
    /* 40 */
    new(_SigNotify, "signal 40"),
    /* 41 */
    new(_SigNotify, "signal 41"),
    /* 42 */
    new(_SigNotify, "signal 42"),
    /* 43 */
    new(_SigNotify, "signal 43"),
    /* 44 */
    new(_SigNotify, "signal 44"),
    /* 45 */
    new(_SigNotify, "signal 45"),
    /* 46 */
    new(_SigNotify, "signal 46"),
    /* 47 */
    new(_SigNotify, "signal 47"),
    /* 48 */
    new(_SigNotify, "signal 48"),
    /* 49 */
    new(_SigNotify, "signal 49"),
    /* 50 */
    new(_SigNotify, "signal 50"),
    /* 51 */
    new(_SigNotify, "signal 51"),
    /* 52 */
    new(_SigNotify, "signal 52"),
    /* 53 */
    new(_SigNotify, "signal 53"),
    /* 54 */
    new(_SigNotify, "signal 54"),
    /* 55 */
    new(_SigNotify, "signal 55"),
    /* 56 */
    new(_SigNotify, "signal 56"),
    /* 57 */
    new(_SigNotify, "signal 57"),
    /* 58 */
    new(_SigNotify, "signal 58"),
    /* 59 */
    new(_SigNotify, "signal 59"),
    /* 60 */
    new(_SigNotify, "signal 60"),
    /* 61 */
    new(_SigNotify, "signal 61"),
    /* 62 */
    new(_SigNotify, "signal 62"),
    /* 63 */
    new(_SigNotify, "signal 63"),
    /* 64 */
    new(_SigNotify, "signal 64")
}.array()); }
internal static ref array<sigTabT> sigtable => ref ((StaticBox<array<sigTabT>>)Ꮡsigtable).ValueSlot;

} // end runtime_package
