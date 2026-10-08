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
    internal uint64 si_addr;
    internal array<byte> si_value = new(8);
    internal int64 si_band;
    internal array<uint64> __pad = new(7);
}

partial struct timeval {
    internal int64 tv_sec;
    internal int32 tv_usec;
    internal array<byte> pad_cgo_0 = new(4);
}

internal static void set_usec(this ref timeval tv, int32 x) {
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
internal static void setNsec(this ref timespec ts, int64 ns) {
    ts.tv_sec = ns / 1000000000;
    ts.tv_nsec = ns % 1000000000;
}

partial struct fpcontrol {
    internal array<byte> pad_cgo_0 = new(2);
}

partial struct fpstatus {
    internal array<byte> pad_cgo_0 = new(2);
}

partial struct regmmst {
    internal array<int8> mmst_reg = new(10);
    internal array<int8> mmst_rsrv = new(6);
}

partial struct regxmm {
    internal array<int8> xmm_reg = new(16);
}

partial struct regs64 {
    internal uint64 rax;
    internal uint64 rbx;
    internal uint64 rcx;
    internal uint64 rdx;
    internal uint64 rdi;
    internal uint64 rsi;
    internal uint64 rbp;
    internal uint64 rsp;
    internal uint64 r8;
    internal uint64 r9;
    internal uint64 r10;
    internal uint64 r11;
    internal uint64 r12;
    internal uint64 r13;
    internal uint64 r14;
    internal uint64 r15;
    internal uint64 rip;
    internal uint64 rflags;
    internal uint64 cs;
    internal uint64 fs;
    internal uint64 gs;
}

partial struct floatstate64 {
    internal array<int32> fpu_reserved = new(2);
    internal fpcontrol fpu_fcw;
    internal fpstatus fpu_fsw;
    internal uint8 fpu_ftw;
    internal uint8 fpu_rsrv1;
    internal uint16 fpu_fop;
    internal uint32 fpu_ip;
    internal uint16 fpu_cs;
    internal uint16 fpu_rsrv2;
    internal uint32 fpu_dp;
    internal uint16 fpu_ds;
    internal uint16 fpu_rsrv3;
    internal uint32 fpu_mxcsr;
    internal uint32 fpu_mxcsrmask;
    internal regmmst fpu_stmm0;
    internal regmmst fpu_stmm1;
    internal regmmst fpu_stmm2;
    internal regmmst fpu_stmm3;
    internal regmmst fpu_stmm4;
    internal regmmst fpu_stmm5;
    internal regmmst fpu_stmm6;
    internal regmmst fpu_stmm7;
    internal regxmm fpu_xmm0;
    internal regxmm fpu_xmm1;
    internal regxmm fpu_xmm2;
    internal regxmm fpu_xmm3;
    internal regxmm fpu_xmm4;
    internal regxmm fpu_xmm5;
    internal regxmm fpu_xmm6;
    internal regxmm fpu_xmm7;
    internal regxmm fpu_xmm8;
    internal regxmm fpu_xmm9;
    internal regxmm fpu_xmm10;
    internal regxmm fpu_xmm11;
    internal regxmm fpu_xmm12;
    internal regxmm fpu_xmm13;
    internal regxmm fpu_xmm14;
    internal regxmm fpu_xmm15;
    internal array<int8> fpu_rsrv4 = new(96);
    internal int32 fpu_reserved1;
}

partial struct exceptionstate64 {
    internal uint16 trapno;
    internal uint16 cpu;
    internal uint32 err;
    internal uint64 faultvaddr;
}

partial struct mcontext64 {
    internal exceptionstate64 es;
    internal regs64 ss;
    internal floatstate64 fs;
    internal array<byte> pad_cgo_0 = new(4);
}

partial struct regs32 {
    internal uint32 eax;
    internal uint32 ebx;
    internal uint32 ecx;
    internal uint32 edx;
    internal uint32 edi;
    internal uint32 esi;
    internal uint32 ebp;
    internal uint32 esp;
    internal uint32 ss;
    internal uint32 eflags;
    internal uint32 eip;
    internal uint32 cs;
    internal uint32 ds;
    internal uint32 es;
    internal uint32 fs;
    internal uint32 gs;
}

partial struct floatstate32 {
    internal array<int32> fpu_reserved = new(2);
    internal fpcontrol fpu_fcw;
    internal fpstatus fpu_fsw;
    internal uint8 fpu_ftw;
    internal uint8 fpu_rsrv1;
    internal uint16 fpu_fop;
    internal uint32 fpu_ip;
    internal uint16 fpu_cs;
    internal uint16 fpu_rsrv2;
    internal uint32 fpu_dp;
    internal uint16 fpu_ds;
    internal uint16 fpu_rsrv3;
    internal uint32 fpu_mxcsr;
    internal uint32 fpu_mxcsrmask;
    internal regmmst fpu_stmm0;
    internal regmmst fpu_stmm1;
    internal regmmst fpu_stmm2;
    internal regmmst fpu_stmm3;
    internal regmmst fpu_stmm4;
    internal regmmst fpu_stmm5;
    internal regmmst fpu_stmm6;
    internal regmmst fpu_stmm7;
    internal regxmm fpu_xmm0;
    internal regxmm fpu_xmm1;
    internal regxmm fpu_xmm2;
    internal regxmm fpu_xmm3;
    internal regxmm fpu_xmm4;
    internal regxmm fpu_xmm5;
    internal regxmm fpu_xmm6;
    internal regxmm fpu_xmm7;
    internal array<int8> fpu_rsrv4 = new(224);
    internal int32 fpu_reserved1;
}

partial struct exceptionstate32 {
    internal uint16 trapno;
    internal uint16 cpu;
    internal uint32 err;
    internal uint32 faultvaddr;
}

partial struct mcontext32 {
    internal exceptionstate32 es;
    internal regs32 ss;
    internal floatstate32 fs;
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

partial struct machPort /*num:uint32*/;

partial struct machVMMapRead /*num:uint32*/;

partial struct machVMAddress /*num:uint64*/;

partial struct machVMSize /*num:uint64*/;

partial struct machVMRegionFlavour /*num:int32*/;

partial class machVMRegionInfo /*ж<int32>*/;

partial struct machMsgTypeNumber /*num:uint32*/;

} // end runtime_package
