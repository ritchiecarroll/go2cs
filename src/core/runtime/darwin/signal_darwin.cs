// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

internal static ж<array<sigTabT>> Ꮡsigtable = new StandardBox<array<sigTabT>>(new sigTabT[]{
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
    new(_SigThrow, "SIGEMT: emulate instruction executed"u8),
    /* 8 */
    new(_SigPanic + _SigUnblock, "SIGFPE: floating-point exception"u8),
    /* 9 */
    new(0, "SIGKILL: kill"u8),
    /* 10 */
    new(_SigPanic + _SigUnblock, "SIGBUS: bus error"u8),
    /* 11 */
    new(_SigPanic + _SigUnblock, "SIGSEGV: segmentation violation"u8),
    /* 12 */
    new(_SigThrow, "SIGSYS: bad system call"u8),
    /* 13 */
    new(_SigNotify, "SIGPIPE: write to broken pipe"u8),
    /* 14 */
    new(_SigNotify, "SIGALRM: alarm clock"u8),
    /* 15 */
    new(_SigNotify + _SigKill, "SIGTERM: termination"u8),
    /* 16 */
    new(_SigNotify + _SigIgn, "SIGURG: urgent condition on socket"u8),
    /* 17 */
    new(0, "SIGSTOP: stop"u8),
    /* 18 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTSTP: keyboard stop"u8),
    /* 19 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGCONT: continue after stop"u8),
    /* 20 */
    new(_SigNotify + _SigUnblock + _SigIgn, "SIGCHLD: child status has changed"u8),
    /* 21 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTTIN: background read from tty"u8),
    /* 22 */
    new(_SigNotify + _SigDefault + _SigIgn, "SIGTTOU: background write to tty"u8),
    /* 23 */
    new(_SigNotify + _SigIgn, "SIGIO: i/o now possible"u8),
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
    new(_SigNotify + _SigIgn, "SIGINFO: status request from keyboard"u8),
    /* 30 */
    new(_SigNotify, "SIGUSR1: user-defined signal 1"u8),
    /* 31 */
    new(_SigNotify, "SIGUSR2: user-defined signal 2"u8)
}.array());
internal static ref array<sigTabT> sigtable => ref Ꮡsigtable.Value;

} // end runtime_package
