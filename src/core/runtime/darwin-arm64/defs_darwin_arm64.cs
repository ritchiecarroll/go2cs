// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_darwin.go
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal static UntypedInt _EINTR => 0x4;
internal static UntypedInt _EFAULT => 0xe;
internal static UntypedInt _EAGAIN => 0x23;
internal static UntypedInt _ETIMEDOUT => 0x3c;

internal static UntypedInt _PROT_NONE => 0x0;
internal static UntypedInt _PROT_READ => 0x1;
internal static UntypedInt _PROT_WRITE => 0x2;
internal static UntypedInt _PROT_EXEC => 0x4;

internal static UntypedInt _MAP_ANON => 0x1000;
internal static UntypedInt _MAP_PRIVATE => 0x2;
internal static UntypedInt _MAP_FIXED => 0x10;

internal static UntypedInt _MADV_DONTNEED => 0x4;
internal static UntypedInt _MADV_FREE => 0x5;
internal static UntypedInt _MADV_FREE_REUSABLE => 0x7;
internal static UntypedInt _MADV_FREE_REUSE => 0x8;

internal static UntypedInt _SA_SIGINFO => 0x40;
internal static UntypedInt _SA_RESTART => 0x2;
internal static UntypedInt _SA_ONSTACK => 0x1;
internal static UntypedInt _SA_USERTRAMP => 0x100;
internal static UntypedInt _SA_64REGSET => 0x200;

internal static UntypedInt _SIGHUP => 0x1;
internal static UntypedInt _SIGINT => 0x2;
internal static UntypedInt _SIGQUIT => 0x3;
internal static UntypedInt _SIGILL => 0x4;
internal static UntypedInt _SIGTRAP => 0x5;
internal static UntypedInt _SIGABRT => 0x6;
internal static UntypedInt _SIGEMT => 0x7;
internal static UntypedInt _SIGFPE => 0x8;
internal static UntypedInt _SIGKILL => 0x9;
internal static UntypedInt _SIGBUS => 0xa;
internal static UntypedInt _SIGSEGV => 0xb;
internal static UntypedInt _SIGSYS => 0xc;
internal static UntypedInt _SIGPIPE => 0xd;
internal static UntypedInt _SIGALRM => 0xe;
internal static UntypedInt _SIGTERM => 0xf;
internal static UntypedInt _SIGURG => 0x10;
internal static UntypedInt _SIGSTOP => 0x11;
internal static UntypedInt _SIGTSTP => 0x12;
internal static UntypedInt _SIGCONT => 0x13;
internal static UntypedInt _SIGCHLD => 0x14;
internal static UntypedInt _SIGTTIN => 0x15;
internal static UntypedInt _SIGTTOU => 0x16;
internal static UntypedInt _SIGIO => 0x17;
internal static UntypedInt _SIGXCPU => 0x18;
internal static UntypedInt _SIGXFSZ => 0x19;
internal static UntypedInt _SIGVTALRM => 0x1a;
internal static UntypedInt _SIGPROF => 0x1b;
internal static UntypedInt _SIGWINCH => 0x1c;
internal static UntypedInt _SIGINFO => 0x1d;
internal static UntypedInt _SIGUSR1 => 0x1e;
internal static UntypedInt _SIGUSR2 => 0x1f;

internal static UntypedInt _FPE_INTDIV => 0x7;
internal static UntypedInt _FPE_INTOVF => 0x8;
internal static UntypedInt _FPE_FLTDIV => 0x1;
internal static UntypedInt _FPE_FLTOVF => 0x2;
internal static UntypedInt _FPE_FLTUND => 0x3;
internal static UntypedInt _FPE_FLTRES => 0x4;
internal static UntypedInt _FPE_FLTINV => 0x5;
internal static UntypedInt _FPE_FLTSUB => 0x6;

internal static UntypedInt _BUS_ADRALN => 0x1;
internal static UntypedInt _BUS_ADRERR => 0x2;
internal static UntypedInt _BUS_OBJERR => 0x3;

internal static UntypedInt _SEGV_MAPERR => 0x1;
internal static UntypedInt _SEGV_ACCERR => 0x2;

internal static UntypedInt _ITIMER_REAL => 0x0;
internal static UntypedInt _ITIMER_VIRTUAL => 0x1;
internal static UntypedInt _ITIMER_PROF => 0x2;

internal static UntypedInt _EV_ADD => 0x1;
internal static UntypedInt _EV_DELETE => 0x2;
internal static UntypedInt _EV_ENABLE => 0x4;
internal static UntypedInt _EV_DISABLE => 0x8;
internal static UntypedInt _EV_CLEAR => 0x20;
internal static UntypedInt _EV_RECEIPT => 0x40;
internal static UntypedInt _EV_ERROR => 0x4000;
internal static UntypedInt _EV_EOF => 0x8000;
internal static UntypedInt _EVFILT_READ => /* -0x1 */ -1;
internal static UntypedInt _EVFILT_WRITE => /* -0x2 */ -2;
internal static UntypedInt _EVFILT_USER => /* -0xa */ -10;

internal static UntypedInt _NOTE_TRIGGER => 0x1000000;

internal static UntypedInt _PTHREAD_CREATE_DETACHED => 0x2;

internal static UntypedInt _PTHREAD_KEYS_MAX => 512;

internal static UntypedInt _F_GETFL => 0x3;
internal static UntypedInt _F_SETFL => 0x4;

internal static UntypedInt _O_WRONLY => 0x1;
internal static UntypedInt _O_NONBLOCK => 0x4;
internal static UntypedInt _O_CREAT => 0x200;
internal static UntypedInt _O_TRUNC => 0x400;

internal static UntypedInt _VM_REGION_BASIC_INFO_COUNT_64 => 0x9;
internal static UntypedInt _VM_REGION_BASIC_INFO_64 => 0x9;

partial struct stackt {
    internal ж<byte> ss_sp;
    internal uintptr ss_size;
    internal int32 ss_flags;
    internal array<byte> pad_cgo_0 = new(4);
}

partial struct sigactiont {
    internal array<byte> __sigaction_u = new(8);
    internal @unsafe.Pointer sa_tramp;
    internal uint32 sa_mask;
    internal int32 sa_flags;
}

partial struct usigactiont {
    internal array<byte> __sigaction_u = new(8);
    internal uint32 sa_mask;
    internal int32 sa_flags;
}

partial struct siginfo {
    internal int32 si_signo;
    internal int32 si_errno;
    internal int32 si_code;
    internal int32 si_pid;
    internal uint32 si_uid;
    internal int32 si_status;
    internal ж<byte> si_addr;
    internal array<byte> si_value = new(8);
    internal int64 si_band;
    internal array<uint64> __pad = new(7);
}

partial struct timeval {
    internal int64 tv_sec;
    internal int32 tv_usec;
    internal array<byte> pad_cgo_0 = new(4);
}

internal static void set_usec⁔(/*this*/ ref timeval tv, int32 x) {
    tv.tv_usec = x;
}

partial struct itimerval {
    internal timeval it_interval;
    internal timeval it_value;
}

partial struct timespec {
    internal int64 tv_sec;
    internal int64 tv_nsec;
}

//go:nosplit
internal static void setNsec⁔(/*this*/ ref timespec ts, int64 ns) {
    ts.tv_sec = ns / 1000000000;
    ts.tv_nsec = ns % 1000000000;
}

partial struct exceptionstate64 {
    internal uint64 far; // virtual fault addr
    internal uint32 esr; // exception syndrome
    internal uint32 exc; // number of arm exception taken
}

partial struct regs64 {
    internal array<uint64> x = new(29); // registers x0 to x28
    internal uint64 fp;                 // frame register, x29
    internal uint64 lr;                 // link register, x30
    internal uint64 sp;                 // stack pointer, x31
    internal uint64 pc;                 // program counter
    internal uint32 cpsr;               // current program status register
    internal uint32 __pad;
}

partial struct neonstate64 {
    internal array<uint64> v = new(64); // actually [32]uint128
    internal uint32 fpsr;
    internal uint32 fpcr;
}

partial struct mcontext64 {
    internal exceptionstate64 es;
    internal regs64 ss;
    internal neonstate64 ns;
}

partial struct ucontext {
    internal int32 uc_onstack;
    internal uint32 uc_sigmask;
    internal stackt uc_stack;
    internal ж<ucontext> uc_link;
    internal uint64 uc_mcsize;
    internal ж<mcontext64> uc_mcontext;
}

partial struct keventt {
    internal uint64 ident;
    internal int16 filter;
    internal uint16 flags;
    internal uint32 fflags;
    internal int64 data;
    internal ж<byte> udata;
}

partial struct pthread /*num:uintptr*/;
partial struct pthreadattr {
    public int64 X__sig;
    public array<int8> X__opaque = new(56);
}
partial struct pthreadmutex {
    public int64 X__sig;
    public array<int8> X__opaque = new(56);
}
partial struct pthreadmutexattr {
    public int64 X__sig;
    public array<int8> X__opaque = new(8);
}
partial struct pthreadcond {
    public int64 X__sig;
    public array<int8> X__opaque = new(40);
}
partial struct pthreadcondattr {
    public int64 X__sig;
    public array<int8> X__opaque = new(8);
}

partial struct machTimebaseInfo {
    internal uint32 numer;
    internal uint32 denom;
}

partial struct pthreadkey /*num:uint64*/;

partial struct machPort /*num:uint32*/;
partial struct machVMMapRead /*num:uint32*/;
partial struct machVMAddress /*num:uint64*/;
partial struct machVMSize /*num:uint64*/;
partial struct machVMRegionFlavour /*num:int32*/;
partial class machVMRegionInfo /*ж<int32>*/;
partial struct machMsgTypeNumber /*num:uint32*/;

} // end runtime_package
