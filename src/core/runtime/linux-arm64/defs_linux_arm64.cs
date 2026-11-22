// Created by cgo -cdefs and converted (by hand) to Go
// ../cmd/cgo/cgo -cdefs defs_linux.go defs1_linux.go defs2_linux.go
namespace go;

using @unsafe = unsafe_package;
using static runtime_package;

partial class runtime_package {

internal static UntypedInt _EINTR => 0x4;
internal static UntypedInt _EAGAIN => 0xb;
internal static UntypedInt _ENOMEM => 0xc;
internal static UntypedInt _ENOSYS => 0x26;

internal static UntypedInt _PROT_NONE => 0x0;
internal static UntypedInt _PROT_READ => 0x1;
internal static UntypedInt _PROT_WRITE => 0x2;
internal static UntypedInt _PROT_EXEC => 0x4;

internal static UntypedInt _MAP_ANON => 0x20;
internal static UntypedInt _MAP_PRIVATE => 0x2;
internal static UntypedInt _MAP_FIXED => 0x10;

internal static UntypedInt _MADV_DONTNEED => 0x4;
internal static UntypedInt _MADV_FREE => 0x8;
internal static UntypedInt _MADV_HUGEPAGE => 0xe;
internal static UntypedInt _MADV_NOHUGEPAGE => 0xf;
internal static UntypedInt _MADV_COLLAPSE => 0x19;

internal static UntypedInt _SA_RESTART => 0x10000000;
internal static UntypedInt _SA_ONSTACK => 0x8000000;
internal static UntypedInt _SA_RESTORER => 0x0; // Only used on intel
internal static UntypedInt _SA_SIGINFO => 0x4;

internal static UntypedInt _SI_KERNEL => 0x80;
internal static UntypedInt _SI_TIMER => /* -0x2 */ -2;

internal static UntypedInt _SIGHUP => 0x1;
internal static UntypedInt _SIGINT => 0x2;
internal static UntypedInt _SIGQUIT => 0x3;
internal static UntypedInt _SIGILL => 0x4;
internal static UntypedInt _SIGTRAP => 0x5;
internal static UntypedInt _SIGABRT => 0x6;
internal static UntypedInt _SIGBUS => 0x7;
internal static UntypedInt _SIGFPE => 0x8;
internal static UntypedInt _SIGKILL => 0x9;
internal static UntypedInt _SIGUSR1 => 0xa;
internal static UntypedInt _SIGSEGV => 0xb;
internal static UntypedInt _SIGUSR2 => 0xc;
internal static UntypedInt _SIGPIPE => 0xd;
internal static UntypedInt _SIGALRM => 0xe;
internal static UntypedInt _SIGSTKFLT => 0x10;
internal static UntypedInt _SIGCHLD => 0x11;
internal static UntypedInt _SIGCONT => 0x12;
internal static UntypedInt _SIGSTOP => 0x13;
internal static UntypedInt _SIGTSTP => 0x14;
internal static UntypedInt _SIGTTIN => 0x15;
internal static UntypedInt _SIGTTOU => 0x16;
internal static UntypedInt _SIGURG => 0x17;
internal static UntypedInt _SIGXCPU => 0x18;
internal static UntypedInt _SIGXFSZ => 0x19;
internal static UntypedInt _SIGVTALRM => 0x1a;
internal static UntypedInt _SIGPROF => 0x1b;
internal static UntypedInt _SIGWINCH => 0x1c;
internal static UntypedInt _SIGIO => 0x1d;
internal static UntypedInt _SIGPWR => 0x1e;
internal static UntypedInt _SIGSYS => 0x1f;

internal static UntypedInt _SIGRTMIN => 0x20;

internal static UntypedInt _FPE_INTDIV => 0x1;
internal static UntypedInt _FPE_INTOVF => 0x2;
internal static UntypedInt _FPE_FLTDIV => 0x3;
internal static UntypedInt _FPE_FLTOVF => 0x4;
internal static UntypedInt _FPE_FLTUND => 0x5;
internal static UntypedInt _FPE_FLTRES => 0x6;
internal static UntypedInt _FPE_FLTINV => 0x7;
internal static UntypedInt _FPE_FLTSUB => 0x8;

internal static UntypedInt _BUS_ADRALN => 0x1;
internal static UntypedInt _BUS_ADRERR => 0x2;
internal static UntypedInt _BUS_OBJERR => 0x3;

internal static UntypedInt _SEGV_MAPERR => 0x1;
internal static UntypedInt _SEGV_ACCERR => 0x2;

internal static UntypedInt _ITIMER_REAL => 0x0;
internal static UntypedInt _ITIMER_VIRTUAL => 0x1;
internal static UntypedInt _ITIMER_PROF => 0x2;

internal static UntypedInt _CLOCK_THREAD_CPUTIME_ID => 0x3;

internal static UntypedInt _SIGEV_THREAD_ID => 0x4;

internal static UntypedInt _AF_UNIX => 0x1;
internal static UntypedInt _SOCK_DGRAM => 0x2;

partial struct timespec {
    internal int64 tv_sec;
    internal int64 tv_nsec;
}

} partial class runtimeᴛtimespec {

//go:nosplit
internal static void setNsec(this ref timespec ts, int64 ns) {
    ts.tv_sec = ns / 1000000000;
    ts.tv_nsec = ns % 1000000000;
}

} partial class runtime_package {

partial struct timeval {
    internal int64 tv_sec;
    internal int64 tv_usec;
}

} partial class runtimeᴛtimeval {

internal static void set_usec(this ref timeval tv, int32 x) {
    tv.tv_usec = (int64)x;
}

} partial class runtime_package {

partial struct sigactiont {
    internal uintptr sa_handler;
    internal uint64 sa_flags;
    internal uintptr sa_restorer;
    internal uint64 sa_mask;
}

partial struct siginfoFields {
    internal int32 si_signo;
    internal int32 si_errno;
    internal int32 si_code;
    // below here is a union; si_addr is the only field we use
    internal uint64 si_addr;
}

partial struct siginfo {
    /*embed*/ internal siginfoFields siginfoFields;
    // Pad struct to the max size in the kernel.
    internal array<byte> _ = new((uintptr)_si_max_size - /* unsafe.Sizeof(siginfoFields{}) */ (uintptr)24);
}

partial struct itimerspec {
    internal timespec it_interval;
    internal timespec it_value;
}

partial struct itimerval {
    internal timeval it_interval;
    internal timeval it_value;
}

partial struct sigeventFields {
    internal uintptr value;
    internal int32 signo;
    internal int32 notify;
    // below here is a union; sigev_notify_thread_id is the only field we use
    internal int32 sigev_notify_thread_id;
}

partial struct sigevent {
    /*embed*/ internal sigeventFields sigeventFields;
    // Pad struct to the max size in the kernel.
    internal array<byte> _ = new((uintptr)_sigev_max_size - /* unsafe.Sizeof(sigeventFields{}) */ (uintptr)24);
}

// Created by cgo -cdefs and then converted to Go by hand
// ../cmd/cgo/cgo -cdefs defs_linux.go defs1_linux.go defs2_linux.go
internal static UntypedInt _O_RDONLY => 0x0;
internal static UntypedInt _O_WRONLY => 0x1;
internal static UntypedInt _O_CREAT => 0x40;
internal static UntypedInt _O_TRUNC => 0x200;
internal static UntypedInt _O_NONBLOCK => 0x800;
internal static UntypedInt _O_CLOEXEC => 0x80000;

partial struct usigset {
    internal array<uint64> __val = new(16);
}

partial struct stackt {
    internal ж<byte> ss_sp;
    internal int32 ss_flags;
    internal array<byte> pad_cgo_0 = new(4);
    internal uintptr ss_size;
}

partial struct sigcontext {
    internal uint64 fault_address;
    /* AArch64 registers */
    internal array<uint64> regs = new(31);
    internal uint64 sp;
    internal uint64 pc;
    internal uint64 pstate;
    internal array<byte> _pad = new(8); // __attribute__((__aligned__(16)))
    internal array<byte> __reserved = new(4096);
}

partial struct sockaddr_un {
    internal uint16 family;
    internal array<byte> path = new(108);
}

partial struct ucontext {
    internal uint64 uc_flags;
    internal ж<ucontext> uc_link;
    internal stackt uc_stack;
    internal uint64 uc_sigmask;
    internal array<byte> _pad = new((1024 - 64) / 8);
    internal array<byte> _pad2 = new(8); // sigcontext must be aligned to 16-byte
    internal sigcontext uc_mcontext;
}

} // end runtime_package
