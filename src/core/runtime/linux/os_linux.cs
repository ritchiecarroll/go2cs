// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using syscall = @internal.runtime.syscall_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// sigPerThreadSyscall is the same signal (SIGSETXID) used by glibc for
// per-thread syscalls on Linux. We use it for the same purpose in non-cgo
// binaries.
internal static UntypedInt sigPerThreadSyscall => /* _SIGRTMIN + 1 */ 33;

[GoType] partial struct mOS {
    // profileTimer holds the ID of the POSIX interval timer for profiling CPU
    // usage on this thread.
    //
    // It is valid when the profileTimerValid field is true. A thread
    // creates and manages its own timer, and these fields are read and written
    // only by this thread. But because some of the reads on profileTimerValid
    // are in signal handling code, this field should be atomic type.
    internal int32 profileTimer;
    internal atomic.Bool profileTimerValid;
    // needPerThreadSyscall indicates that a per-thread syscall is required
    // for doAllThreadsSyscall.
    internal atomic.Uint8 needPerThreadSyscall;
    // This is a pointer to a chunk of memory allocated with a special
    // mmap invocation in vgetrandomGetState().
    internal uintptr vgetrandomState;
    internal uint32 waitsema; // semaphore for parking on locks
}

//go:noescape
internal static partial int32 futex(@unsafe.Pointer addr, int32 op, uint32 val, @unsafe.Pointer ts, @unsafe.Pointer addr2, uint32 val3);

// Linux futex.
//
//	futexsleep(uint32 *addr, uint32 val)
//	futexwakeup(uint32 *addr)
//
// Futexsleep atomically checks if *addr == val and if so, sleeps on addr.
// Futexwakeup wakes up threads sleeping on addr.
// Futexsleep is allowed to wake up spuriously.
internal static UntypedInt _FUTEX_PRIVATE_FLAG => 128;
internal static UntypedInt _FUTEX_WAIT_PRIVATE => /* 0 | _FUTEX_PRIVATE_FLAG */ 128;
internal static UntypedInt _FUTEX_WAKE_PRIVATE => /* 1 | _FUTEX_PRIVATE_FLAG */ 129;

// Atomically,
//
//	if(*addr == val) sleep
//
// Might be woken up spuriously; that's allowed.
// Don't sleep longer than ns; ns < 0 means forever.
//
//go:nosplit
internal static void futexsleep(ж<uint32> Ꮡaddr, uint32 val, int64 ns) {
    // Some Linux kernels have a bug where futex of
    // FUTEX_WAIT returns an internal error code
    // as an errno. Libpthread ignores the return value
    // here, and so can we: as it says a few lines up,
    // spurious wakeups are allowed.
    if (ns < 0) {
        futex(@unsafe.Pointer.FromPinnedBox(Ꮡaddr), _FUTEX_WAIT_PRIVATE, val, nil, nil, 0);
        return;
    }
    ref var ts = ref heap(new timespec(), out var Ꮡts);
    ts.setNsec(ns);
    futex(@unsafe.Pointer.FromPinnedBox(Ꮡaddr), _FUTEX_WAIT_PRIVATE, val, @unsafe.Pointer.FromPinnedBox(Ꮡts), nil, 0);
}

// If any procs are sleeping on addr, wake up at most cnt.
//
//go:nosplit
internal static void futexwakeup(ж<uint32> Ꮡaddr, uint32 cnt) {
    ref var addr = ref Ꮡaddr.DerefOrNull();

    var ret = futex(@unsafe.Pointer.FromPinnedBox(Ꮡaddr), _FUTEX_WAKE_PRIVATE, cnt, nil, nil, 0);
    if (ret >= 0) {
        return;
    }
    // I don't know that futex wakeup can return
    // EAGAIN or EINTR, but if it does, it would be
    // safe to loop and call futex again.
    systemstack(() => {
        print((@string)"futexwakeup addr="u8, Ꮡaddr.OrTypedNil(), (@string)" returned "u8, ret, (@string)"\n"u8);
    });
    ((ж<int32>)(uintptr)((@unsafe.Pointer)(uintptr)0x1006)).Value = 0x1006;
}

internal static int32 getproccount() {
    // This buffer is huge (8 kB) but we are on the system stack
    // and there should be plenty of space (64 kB).
    // Also this is a leaf, so we're not holding up the memory for long.
    // See golang.org/issue/11823.
    // The suggested behavior here is to keep trying with ever-larger
    // buffers, but we don't have a dynamic memory allocator at the
    // moment, so that's a bit tricky and seems like overkill.
    UntypedInt maxCPUs = /* 64 * 1024 */ 65536;
    ref var buf = ref heap(new array<byte>(8192), out var Ꮡbuf);
    var r = sched_getaffinity(0, /* unsafe.Sizeof(buf) */ (uintptr)8192, Ꮡbuf.at<byte>(0));
    if (r < 0) {
        return 1;
    }
    var n = (int32)0;
    foreach (var (_, vᴛ1) in buf.slice(0, r)) {
        var v = vᴛ1;

        while (v != 0) {
            n += (int32)((byte)(v & 1));
            v >>= (int)(1);
        }
    }
    if (n == 0) {
        n = 1;
    }
    return n;
}

// Clone, the Linux rfork.
internal static UntypedInt _CLONE_VM => 0x100;

internal static UntypedInt _CLONE_FS => 0x200;

internal static UntypedInt _CLONE_FILES => 0x400;

internal static UntypedInt _CLONE_SIGHAND => 0x800;

internal static UntypedInt _CLONE_PTRACE => 0x2000;

internal static UntypedInt _CLONE_VFORK => 0x4000;

internal static UntypedInt _CLONE_PARENT => 0x8000;

internal static UntypedInt _CLONE_THREAD => 0x10000;

internal static UntypedInt _CLONE_NEWNS => 0x20000;

internal static UntypedInt _CLONE_SYSVSEM => 0x40000;

internal static UntypedInt _CLONE_SETTLS => 0x80000;

internal static UntypedInt _CLONE_PARENT_SETTID => 0x100000;

internal static UntypedInt _CLONE_CHILD_CLEARTID => 0x200000;

internal static UntypedInt _CLONE_UNTRACED => 0x800000;

internal static UntypedInt _CLONE_CHILD_SETTID => 0x1000000;

internal static UntypedInt _CLONE_STOPPED => 0x2000000;

internal static UntypedInt _CLONE_NEWUTS => 0x4000000;

internal static UntypedInt _CLONE_NEWIPC => 0x8000000;
// As of QEMU 2.8.0 (5ea2fc84d), user emulation requires all six of these
// flags to be set when creating a thread; attempts to share the other
// five but leave SYSVSEM unshared will fail with -EINVAL.
//
// In non-QEMU environments CLONE_SYSVSEM is inconsequential as we do not
// use System V semaphores.
/* share memory */
/* share cwd, etc */
/* share fd table */
/* share sig handler table */
/* share SysV semaphore undo lists (see issue #20763) */

internal static UntypedInt cloneFlags => /* _CLONE_VM |
	_CLONE_FS |
	_CLONE_FILES |
	_CLONE_SIGHAND |
	_CLONE_SYSVSEM |
	_CLONE_THREAD */ 331520; /* revisit - okay for now */

//go:noescape
internal static partial int32 clone(int32 flags, @unsafe.Pointer stk, @unsafe.Pointer mp, @unsafe.Pointer gp, @unsafe.Pointer fn);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newosprocˢ = "newosproc"u8;

// May run with m.p==nil, so write barriers are not allowed.
//
//go:nowritebarrier
internal static void newosproc(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    @unsafe.Pointer stk = (@unsafe.Pointer)(~mp.g0).stack.hi;
    /*
	 * note: strace gets confused if we use CLONE_PTRACE here.
	 */
    if (false) {
        print((@string)"newosproc stk="u8, @unsafe.Pointer.OrTypedNil(stk), (@string)" m="u8, Ꮡmp.OrTypedNil(), (@string)" g="u8, mp.g0.OrTypedNil(), (@string)" clone="u8, abi.FuncPCABI0(clone), (@string)" id="u8, mp.id, (@string)" ostk="u8, Ꮡ(mp), (@string)"\n"u8);
    }
    // Disable signals during clone, so that the new thread starts
    // with signals disabled. It will enable them in minit.
    ref var oset = ref heap(new sigset(), out var Ꮡoset);
    sigprocmask(_SIG_SETMASK, Ꮡsigset_all, Ꮡoset);
    var ret = retryOnEAGAIN(() => {
        var r = clone(cloneFlags, stk, @unsafe.Pointer.FromPinnedBox(Ꮡmp), @unsafe.Pointer.FromPinnedBox(Ꮡmp.Value.g0), (@unsafe.Pointer)abi.FuncPCABI0(mstart));
        // clone returns positive TID, negative errno.
        // We don't care about the TID.
        if (r >= 0) {
            return 0;
        }
        return -r;
    });
    sigprocmask(_SIG_SETMASK, Ꮡoset, ж<sigset>.NilBoxOfDims(2L));
    if (ret != 0) {
        print((@string)"runtime: failed to create new OS thread (have "u8, mcount(), (@string)" already; errno="u8, ret, (@string)")\n"u8);
        if (ret == _EAGAIN) {
            println((@string)"runtime: may need to increase max user processes (ulimit -u)"u8);
        }
        @throw(newosprocˢ);
    }
}

// Version of newosproc that doesn't require a valid G.
//
//go:nosplit
internal static void newosproc0(uintptr stacksize, @unsafe.Pointer fn) {
    @unsafe.Pointer Δstack = (uintptr)sysAlloc(stacksize, Ꮡmemstats.of(mstats.Ꮡstacks_sys));
    if (Δstack == nil) {
        writeErrStr(failallocatestack);
        exit(1);
    }
    var ret = clone(cloneFlags, (@unsafe.Pointer)((uintptr)Δstack + stacksize), nil, nil, fn);
    if (ret < 0) {
        writeErrStr(failthreadcreate);
        exit(1);
    }
}

internal static UntypedInt _AT_NULL => 0; // End of vector
internal static UntypedInt _AT_PAGESZ => 6; // System physical page size
internal static UntypedInt _AT_PLATFORM => 15; // string identifying platform
internal static UntypedInt _AT_HWCAP => 16; // hardware capability bit vector
internal static UntypedInt _AT_SECURE => 23; // secure mode boolean
internal static UntypedInt _AT_RANDOM => 25; // introduced in 2.6.29
internal static UntypedInt _AT_HWCAP2 => 26; // hardware capability bit vector 2

internal static ж<slice<byte>> ᏑprocAuxv = new StandardBox<slice<byte>>(slice<byte>("/proc/self/auxv\x00"u8));
internal static ref slice<byte> procAuxv => ref ᏑprocAuxv.ValueSlot;

internal static ж<array<byte>> Ꮡaddrspace_vec = new StandardBox<array<byte>>(new array<byte>(1));
internal static ref array<byte> addrspace_vec => ref Ꮡaddrspace_vec.Value;

internal static partial int32 mincore(@unsafe.Pointer addr, uintptr n, ж<byte> dst);

internal static ж<array<uintptr>> Ꮡauxvreadbuf = new StandardBox<array<uintptr>>(new array<uintptr>(128));
internal static ref array<uintptr> auxvreadbuf => ref Ꮡauxvreadbuf.Value;

internal static void sysargs(int32 argc, ж<ж<byte>> Ꮡargv) {
    ref var argv = ref Ꮡargv.DerefOrNull();

    var n = argc + 1;
    // skip over argv, envp to get to auxv
    while (argv_index(Ꮡargv, n) != nil) {
        n++;
    }
    // skip NULL separator
    n++;
    // now argv+n is auxv
    var auxvp = (ж<array<uintptr>>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(Ꮡargv), (uintptr)n * (uintptr)goarch.PtrSize));
    {
        nint pairsΔ1 = sysauxv((~auxvp)[..]); if (pairsΔ1 != 0) {
            auxv = (~auxvp).slice(0, pairsΔ1 * 2, pairsΔ1 * 2);
            return;
        }
    }
    // In some situations we don't get a loader-provided
    // auxv, such as when loaded as a library on Android.
    // Fall back to /proc/self/auxv.
    var fd = open(Ꮡ(procAuxv, 0), 0, /* O_RDONLY */
 0);
    if (fd < 0) {
        // On Android, /proc/self/auxv might be unreadable (issue 9229), so we fallback to
        // try using mincore to detect the physical page size.
        // mincore should return EINVAL when address is not a multiple of system page size.
        uintptr size = /* 256 << 10 */ 262144; // size of memory region to allocate
        var (Δp, err) = mmap(nil, size, (int32)((int32)_PROT_READ | (int32)_PROT_WRITE), (int32)((int32)_MAP_ANON | (int32)_MAP_PRIVATE), -1, 0);
        if (err != 0) {
            return;
        }
        uintptr nΔ1 = default!;
        for (nΔ1 = ((uintptr)4 << (int)(10)); nΔ1 < size; nΔ1 <<= (int)(1)) {
            var errΔ1 = mincore((@unsafe.Pointer)((uintptr)Δp + nΔ1), 1, Ꮡaddrspace_vec.at<byte>(0));
            if (errΔ1 == 0) {
                physPageSize = nΔ1;
                break;
            }
        }
        if (physPageSize == 0) {
            physPageSize = size;
        }
        munmap(Δp, size);
        return;
    }
    n = read(fd, (uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡauxvreadbuf.at<uintptr>(0))), (int32)/* unsafe.Sizeof(auxvreadbuf) */ (uintptr)1024);
    closefd(fd);
    if (n < 0) {
        return;
    }
    // Make sure buf is terminated, even if we didn't read
    // the whole file.
    auxvreadbuf[len(auxvreadbuf) - 2] = _AT_NULL;
    nint pairs = sysauxv(auxvreadbuf[..]);
    auxv = auxvreadbuf.slice(0, pairs * 2, pairs * 2);
}

// secureMode holds the value of AT_SECURE passed in the auxiliary vector.
internal static bool secureMode;

internal static nint /*pairs*/ sysauxv(slice<uintptr> auxv) {
    // Process the auxiliary vector entries provided by the kernel when the
    // program is executed. See getauxval(3).
    nint i = default!;
    for (; auxv[i] != _AT_NULL; i += 2) {
        var (tag, val) = (auxv[i], auxv[i + 1]);
        var exprᴛ1 = tag;
        if (exprᴛ1 == _AT_RANDOM) {
            startupRand = (~(ж<array<byte>>)(uintptr)((@unsafe.Pointer)val))[..];
        }
        else if (exprᴛ1 == _AT_PAGESZ) {
            physPageSize = val;
        }
        else if (exprᴛ1 == _AT_SECURE) {
            secureMode = val == 1;
        }

        // The kernel provides a pointer to 16 bytes of cryptographically
        // random data. Note that in cgo programs this value may have
        // already been used by libc at this point, and in particular glibc
        // and musl use the value as-is for stack and pointer protector
        // cookies from libc_start_main and/or dl_start. Also, cgo programs
        // may use the value after we do.
        archauxv(tag, val);
        vdsoauxv(tag, val);
    }
    return i / 2;
}

internal static ж<slice<byte>> ᏑsysTHPSizePath = new StandardBox<slice<byte>>(slice<byte>("/sys/kernel/mm/transparent_hugepage/hpage_pmd_size\x00"u8));
internal static ref slice<byte> sysTHPSizePath => ref ᏑsysTHPSizePath.ValueSlot;

internal static uintptr getHugePageSize() {
    ref var numbuf = ref heap(new array<byte>(20), out var Ꮡnumbuf);
    var fd = open(Ꮡ(sysTHPSizePath, 0), 0, /* O_RDONLY */
 0);
    if (fd < 0) {
        return 0;
    }
    @unsafe.Pointer ptr = (uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡnumbuf.at<byte>(0)));
    var n = read(fd, ptr, (int32)len(numbuf));
    closefd(fd);
    if (n <= 0) {
        return 0;
    }
    n--; // remove trailing newline
    var (v, ok) = atoi(slicebytetostringtmp((ж<byte>)(uintptr)(ptr), (nint)n));
    if (!ok || v < 0) {
        v = 0;
    }
    if ((nint)(v & (v - 1)) != 0) {
        // v is not a power of 2
        return 0;
    }
    return (uintptr)v;
}

internal static void osinit() {
    ncpu = getproccount();
    physHugePageSize = getHugePageSize();
    osArchInit();
    vgetrandomInit();
}

internal static ж<slice<byte>> Ꮡurandom_dev = new StandardBox<slice<byte>>(slice<byte>("/dev/urandom\x00"u8));
internal static ref slice<byte> urandom_dev => ref Ꮡurandom_dev.ValueSlot;

internal static nint readRandom(slice<byte> r) {
    // Note that all supported Linux kernels should provide AT_RANDOM which
    // populates startupRand, so this fallback should be unreachable.
    var fd = open(Ꮡ(urandom_dev, 0), 0, /* O_RDONLY */
 0);
    var n = read(fd, @unsafe.Pointer.FromPinnedBox(Ꮡ(r, 0)), (int32)len(r));
    closefd(fd);
    return (nint)n;
}

internal static void goenvs() {
    goenvs_unix();
}

// Called to do synchronous initialization of Go code built with
// -buildmode=c-archive or -buildmode=c-shared.
// None of the Go runtime is initialized.
//
//go:nosplit
//go:nowritebarrierrec
internal static void libpreinit() {
    initsig(true);
}

// Called to initialize a new m (including the bootstrap m).
// Called on the parent thread (main thread in case of bootstrap), can allocate memory.
internal static void mpreinit(ж<m> Ꮡmp) {
    ref var mp = ref Ꮡmp.DerefOrNull();

    mp.gsignal = malg(32 * 1024); // Linux wants >= 2K
    mp.gsignal.Value.m = Ꮡmp;
}

internal static partial uint32 gettid();

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate memory.
internal static void minit() {
    minitSignals();
    // Cgo-created threads and the bootstrap m are missing a
    // procid. We need this for asynchronous preemption and it's
    // useful in debuggers.
    getg().Value.m.Value.procid = (uint64)gettid();
}

// Called from dropm to undo the effect of an minit.
//
//go:nosplit
internal static void unminit() {
    unminitSignals();
    getg().Value.m.Value.procid = 0;
}

// Called from mexit, but not from dropm, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
//
// This always runs without a P, so //go:nowritebarrierrec is required.
//go:nowritebarrierrec
internal static void mdestroy(ref m mp) {
}

// #ifdef GOARCH_386
// #define sa_handler k_sa_handler
// #endif
internal static partial void sigreturn__sigaction();

internal static partial void sigtramp();

// Called via C ABI
internal static partial void cgoSigtramp();

//go:noescape
internal static partial void sigaltstack(ж<stackt> @new, ж<stackt> old);

//go:noescape
internal static partial void setitimer(int32 mode, ж<itimerval> @new, ж<itimerval> old);

//go:noescape
internal static partial int32 timer_create(int32 clockid, ж<sigevent> sevp, ж<int32> timerid);

//go:noescape
internal static partial int32 timer_settime(int32 timerid, int32 flags, ж<itimerspec> @new, ж<itimerspec> old);

//go:noescape
internal static partial int32 timer_delete(int32 timerid);

//go:noescape
internal static partial void rtsigprocmask(int32 how, ж<sigset> @new, ж<sigset> old, int32 size);

//go:nosplit
//go:nowritebarrierrec
internal static void sigprocmask(int32 how, ж<sigset> Ꮡnew, ж<sigset> Ꮡold) {
    rtsigprocmask(how, Ꮡnew, Ꮡold, (int32)/* unsafe.Sizeof(*new) */ (uintptr)8);
}

internal static partial void raise(uint32 sig);

internal static partial void raiseproc(uint32 sig);

//go:noescape
internal static partial int32 sched_getaffinity(uintptr pid, uintptr len, ж<byte> buf);

internal static partial void osyield();

//go:nosplit
internal static void osyield_no_g() {
    osyield();
}

internal static partial (int32 r, int32 w, int32 errno) pipe2(int32 flags);

//go:nosplit
public static (int32 ret, int32 errno) fcntl(int32 fd, int32 cmd, int32 arg) {
    var (r, _, err) = syscall.Syscall6(syscall.SYS_FCNTL, (uintptr)fd, (uintptr)cmd, (uintptr)arg, 0, 0, 0);
    return ((int32)r, (int32)err);
}

internal static UntypedInt _si_max_size => 128;
internal static UntypedInt _sigev_max_size => 64;

//go:nosplit
//go:nowritebarrierrec
internal static void setsig(uint32 i, uintptr fn) {
    ref var sa = ref heap(new sigactiont(), out var Ꮡsa);
    sa.sa_flags = (uint64)((UntypedInt)((UntypedInt)(_SA_SIGINFO | _SA_ONSTACK) | _SA_RESTORER) | (uint64)_SA_RESTART);
    sigfillset(ref sa.sa_mask);
    // Although Linux manpage says "sa_restorer element is obsolete and
    // should not be used". x86_64 kernel requires it. Only use it on
    // x86.
    if (GOARCH == "386"u8 || GOARCH == "amd64"u8) {
        sa.sa_restorer = abi.FuncPCABI0(sigreturn__sigaction);
    }
    if (fn == abi.FuncPCABIInternal(sighandler)) {
        // abi.FuncPCABIInternal(sighandler) matches the callers in signal_unix.go
        if (iscgo){
            fn = abi.FuncPCABI0(cgoSigtramp);
        } else {
            fn = abi.FuncPCABI0(sigtramp);
        }
    }
    sa.sa_handler = fn;
    sigaction(i, Ꮡsa, nil);
}

//go:nosplit
//go:nowritebarrierrec
internal static void setsigstack(uint32 i) {
    ref var sa = ref heap(new sigactiont(), out var Ꮡsa);
    sigaction(i, nil, Ꮡsa);
    if ((uint64)(sa.sa_flags & (uint64)_SA_ONSTACK) != 0) {
        return;
    }
    sa.sa_flags |= (uint64)(_SA_ONSTACK);
    sigaction(i, Ꮡsa, nil);
}

//go:nosplit
//go:nowritebarrierrec
internal static uintptr getsig(uint32 i) {
    ref var sa = ref heap(new sigactiont(), out var Ꮡsa);
    sigaction(i, nil, Ꮡsa);
    return sa.sa_handler;
}

// setSignalstackSP sets the ss_sp field of a stackt.
//
//go:nosplit
internal static void setSignalstackSP(ж<stackt> Ꮡs, uintptr sp) {
    (Ꮡs.of(stackt.Ꮡss_sp).Reinterpret<ж<byte>, uintptr>()).Value = sp;
}

//go:nosplit
[GoRecv] internal static void fixsigcode(this ref sigctxt c, uint32 sig) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sigactionFailedˢ = "sigaction failed"u8;

// sysSigaction calls the rt_sigaction system call.
//
//go:nosplit
internal static void sysSigaction(uint32 sig, ж<sigactiont> Ꮡnew, ж<sigactiont> Ꮡold) {
    if (rt_sigaction((uintptr)sig, Ꮡnew, Ꮡold, /* unsafe.Sizeof(sigactiont{}.sa_mask) */ (uintptr)8) != 0) {
        // Workaround for bugs in QEMU user mode emulation.
        //
        // QEMU turns calls to the sigaction system call into
        // calls to the C library sigaction call; the C
        // library call rejects attempts to call sigaction for
        // SIGCANCEL (32) or SIGSETXID (33).
        //
        // QEMU rejects calling sigaction on SIGRTMAX (64).
        //
        // Just ignore the error in these case. There isn't
        // anything we can do about it anyhow.
        if (sig != 32 && sig != 33 && sig != 64) {
            // Use system stack to avoid split stack overflow on ppc64/ppc64le.
            systemstack(() => {
                @throw(sigactionFailedˢ);
            });
        }
    }
}

// rt_sigaction is implemented in assembly.
//
//go:noescape
internal static partial int32 rt_sigaction(uintptr sig, ж<sigactiont> @new, ж<sigactiont> old, uintptr size);

internal static partial nint getpid();

internal static partial void tgkill(nint tgid, nint tid, nint sig);

// signalM sends a signal to mp.
internal static void signalM(ref m mp, nint sig) {
    tgkill(getpid(), (nint)mp.procid, sig);
}

// validSIGPROF compares this signal delivery's code against the signal sources
// that the profiler uses, returning whether the delivery should be processed.
// To be processed, a signal delivery from a known profiling mechanism should
// correspond to the best profiling mechanism available to this thread. Signals
// from other sources are always considered valid.
//
//go:nosplit
internal static bool validSIGPROF(ж<m> Ꮡmp, ж<sigctxt> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var code = (int32)c.sigcode();
    var setitimer = code == _SI_KERNEL;
    var timer_create = code == _SI_TIMER;
    if (!(setitimer || timer_create)) {
        // The signal doesn't correspond to a profiling mechanism that the
        // runtime enables itself. There's no reason to process it, but there's
        // no reason to ignore it either.
        return true;
    }
    if (Ꮡmp == nil) {
        // Since we don't have an M, we can't check if there's an active
        // per-thread timer for this thread. We don't know how long this thread
        // has been around, and if it happened to interact with the Go scheduler
        // at a time when profiling was active (causing it to have a per-thread
        // timer). But it may have never interacted with the Go scheduler, or
        // never while profiling was active. To avoid double-counting, process
        // only signals from setitimer.
        //
        // When a custom cgo traceback function has been registered (on
        // platforms that support runtime.SetCgoTraceback), SIGPROF signals
        // delivered to a thread that cannot find a matching M do this check in
        // the assembly implementations of runtime.cgoSigtramp.
        return setitimer;
    }
    // Having an M means the thread interacts with the Go scheduler, and we can
    // check whether there's an active per-thread timer for this thread.
    if (Ꮡmp.of(m.ᏑprofileTimerValid).Load()) {
        // If this M has its own per-thread CPU profiling interval timer, we
        // should track the SIGPROF signals that come from that timer (for
        // accurate reporting of its CPU usage; see issue 35057) and ignore any
        // that it gets from the process-wide setitimer (to not over-count its
        // CPU consumption).
        return timer_create;
    }
    // No active per-thread timer means the only valid profiler is setitimer.
    return setitimer;
}

// go2cs generated this placeholder — func setProcessCPUProfiler is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func setThreadCPUProfiler is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// perThreadSyscallArgs contains the system call number, arguments, and
// expected return values for a system call to be executed on all threads.
[GoType] partial struct perThreadSyscallArgs {
    internal uintptr trap;
    internal uintptr a1;
    internal uintptr a2;
    internal uintptr a3;
    internal uintptr a4;
    internal uintptr a5;
    internal uintptr a6;
    internal uintptr r1;
    internal uintptr r2;
}

// perThreadSyscall is the system call to execute for the ongoing
// doAllThreadsSyscall.
//
// perThreadSyscall may only be written while mp.needPerThreadSyscall == 0 on
// all Ms.
internal static perThreadSyscallArgs perThreadSyscall;

// go2cs generated this placeholder — func syscall_runtime_doAllThreadsSyscall is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string allThreadsSyscall6ˢ = "AllThreadsSyscall6 results differ between threads; runtime corrupted"u8;

// runPerThreadSyscall runs perThreadSyscall for this M if required.
//
// This function throws if the system call returns with anything other than the
// expected values.
//
//go:nosplit
internal static void runPerThreadSyscall() {
    var gp = getg();
    if ((~gp).m.of(m.ᏑneedPerThreadSyscall).Load() == 0) {
        return;
    }
    var args = perThreadSyscall;
    var (r1, r2, errno) = syscall.Syscall6(args.trap, args.a1, args.a2, args.a3, args.a4, args.a5, args.a6);
    if (GOARCH == "ppc64"u8 || GOARCH == "ppc64le"u8) {
        // TODO(https://go.dev/issue/51192 ): ppc64 doesn't use r2.
        r2 = 0;
    }
    if (errno != 0 || r1 != args.r1 || r2 != args.r2) {
        print((@string)"trap:"u8, args.trap, (@string)", a123456=["u8, args.a1, (@string)","u8, args.a2, (@string)","u8, args.a3, (@string)","u8, args.a4, (@string)","u8, args.a5, (@string)","u8, args.a6, (@string)"]\n"u8);
        print((@string)"results: got {r1="u8, r1, (@string)",r2="u8, r2, (@string)",errno="u8, errno, (@string)"}, want {r1="u8, args.r1, (@string)",r2="u8, args.r2, (@string)",errno=0}\n"u8);
        fatal(allThreadsSyscall6ˢ);
    }
    (~gp).m.of(m.ᏑneedPerThreadSyscall).Store(0);
}

internal static UntypedInt _SI_USER => 0;
internal static UntypedInt _SI_TKILL => -6;
internal static UntypedInt _SYS_SECCOMP => 1;

// sigFromUser reports whether the signal was sent because of a call
// to kill or tgkill.
//
//go:nosplit
[GoRecv] internal static bool sigFromUser(this ref sigctxt c) {
    var code = (int32)c.sigcode();
    return code == _SI_USER || code == _SI_TKILL;
}

// sigFromSeccomp reports whether the signal was sent from seccomp.
//
//go:nosplit
[GoRecv] internal static bool sigFromSeccomp(this ref sigctxt c) {
    var code = (int32)c.sigcode();
    return code == _SYS_SECCOMP;
}

//go:nosplit
internal static (int32 ret, int32 errno) mprotect(@unsafe.Pointer addr, uintptr n, int32 prot) {
    var ᴋ0 = addr;
        var (r, _, err) = syscall.Syscall6(syscall.SYS_MPROTECT, (uintptr)ᴋ0, n, (uintptr)prot, 0, 0, 0);
    System.GC.KeepAlive(ᴋ0);
    return ((int32)r, (int32)err);
}

} // end runtime_package
