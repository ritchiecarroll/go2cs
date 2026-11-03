// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
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
    new(_SigThrow, "SIGEMT: emulate instruction executed"),
    /* 8 */
    new(_SigPanic + _SigUnblock, "SIGFPE: floating-point exception"),
    /* 9 */
    new(0, "SIGKILL: kill"),
    /* 10 */
    new(_SigPanic + _SigUnblock, "SIGBUS: bus error"),
    /* 11 */
    new(_SigPanic + _SigUnblock, "SIGSEGV: segmentation violation"),
    /* 12 */
    new(_SigThrow, "SIGSYS: bad system call"),
    /* 13 */
    new(_SigNotify, "SIGPIPE: write to broken pipe"),
    /* 14 */
    new(_SigNotify, "SIGALRM: alarm clock"),
    /* 15 */
    new(_SigNotify + _SigKill, "SIGTERM: termination"),
    /* 16 */
    new(_SigNotify + _SigIgn, "SIGURG: urgent condition on socket"),
    /* 17 */
    new(0, "SIGSTOP: stop"),
    /* 18 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTSTP: keyboard stop"),
    /* 19 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGCONT: continue after stop"),
    /* 20 */
    new(_SigNotify + _SigUnblock + _SigIgn, "SIGCHLD: child status has changed"),
    /* 21 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTTIN: background read from tty"),
    /* 22 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTTOU: background write to tty"),
    /* 23 */
    new(_SigNotify + _SigIgn, "SIGIO: i/o now possible"),
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
    new(_SigNotify + _SigIgn, "SIGINFO: status request from keyboard"),
    /* 30 */
    new(_SigNotify, "SIGUSR1: user-defined signal 1"),
    /* 31 */
    new(_SigNotify, "SIGUSR2: user-defined signal 2")
}.array()); }
internal static ref array<sigTabT> sigtable => ref ((StaticBox<array<sigTabT>>)Ꮡsigtable).ValueSlot;

} // end runtime_package
