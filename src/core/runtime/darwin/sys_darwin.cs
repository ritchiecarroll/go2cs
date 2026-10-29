// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class runtime_package {

internal static partial void libc_error_trampoline();

// go2cs generated this placeholder — func libc_error_addr is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// libcCallInfo is a structure used to pass parameters to the system call.
partial struct libcCallInfo {
    internal uintptr fn;
    internal uintptr n;      // number of parameters
    internal uintptr args;   // parameters
    internal uintptr r1, r2; // return values
}

// syscall_syscalln is a wrapper around the libc call with variable arguments.
//
//go:linkname syscall_syscalln syscall.syscalln
//go:nosplit
//go:uintptrkeepalive
internal static (uintptr r1, uintptr r2, uintptr err) syscall_syscalln(uintptr fn, params ꓸꓸꓸuintptr argsʗp) {
    uintptr r1 = default;
    uintptr r2 = default;
    uintptr err = default;
    var args = argsʗp.sslice();

    entersyscall();
    (r1, r2, err) = syscall_rawsyscalln(fn, args.ꓸꓸꓸ);
    exitsyscall();
    return (r1, r2, err);
}

// syscall_rawsyscalln is a wrapper around the libc call with variable arguments.
// The scheduler is not notified about the system call.
// The syscall is executed on the current goroutine thread rather than on a
// dedicated syscall thread.
//
//go:linkname syscall_rawsyscalln syscall.rawsyscalln
//go:nosplit
//go:uintptrkeepalive
internal static (uintptr r1, uintptr r2, uintptr err) syscall_rawsyscalln(uintptr fn, params ꓸꓸꓸuintptr argsʗp) {
    uintptr err = default;
    var args = argsʗp.slice();

    var c = Ꮡ(new libcCallInfo(
        fn: fn,
        n: (uintptr)len(args)
    ));
    if (c.n != 0) {
        c.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡ(args, 0)));
    }
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscallN_trampoline), @unsafe.Pointer.FromPinnedBox(c));
    {
        var gp = getg(); if (gp != nil && (~gp).m != nil && (~(~gp).m).errnoAddr != nil) {
            err = (uintptr)((~(~gp).m).errnoAddr.Value);
        } else {
            ref var errnoAddr = ref heap<ж<int32>>(out var ᏑerrnoAddr);
            libc_error_addr(ᏑerrnoAddr);
            err = (uintptr)(errnoAddr.Value);
        }
    }
    return (c.r1, c.r2, err);
}

internal static partial void syscallN_trampoline();

internal partial struct crypto_x509_syscall_args /*dyn*/ {
    internal uintptr fn, a1, a2, a3, a4, a5;
    internal float64 f1;
    internal uintptr r1;
}

// crypto_x509_syscall is used in crypto/x509/internal/macos to call into Security.framework and CF.

//go:linkname crypto_x509_syscall crypto/x509/internal/macos.syscall
//go:nosplit
internal static uintptr /*r1*/ crypto_x509_syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, float64 f1) {
    uintptr r1 = default;

    ref var args = ref heap<crypto_x509_syscall_args>(new crypto_x509_syscall_args(fn, a1, a2, a3, a4, a5, f1, r1), out var Ꮡargs);
    entersyscall();
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscall_x509), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    exitsyscall();
    return args.r1;
}

internal static partial void syscall_x509();

internal partial struct pthread_attr_init_args /*dyn*/ {
    internal uintptr attr;
}

// The *_trampoline functions convert from the Go calling convention to the C calling convention
// and then call the underlying libc function.  They are defined in sys_darwin_$ARCH.s.

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_attr_init(ж<pthreadattr> attr) {
    ref var args = ref heap(new pthread_attr_init_args((uintptr)attr.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_attr_init_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(attr.OrTypedNil());
    return ret;
}

internal static partial void pthread_attr_init_trampoline();

internal partial struct pthread_attr_getstacksize_args /*dyn*/ {
    internal uintptr attr;
    internal uintptr size;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_attr_getstacksize(ж<pthreadattr> attr, ж<uintptr> size) {
    ref var args = ref heap(new pthread_attr_getstacksize_args((uintptr)attr.OrTypedNil(), (uintptr)size.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_attr_getstacksize_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(attr.OrTypedNil());
    KeepAlive(size.OrTypedNil());
    return ret;
}

internal static partial void pthread_attr_getstacksize_trampoline();

internal partial struct pthread_attr_setdetachstate_args /*dyn*/ {
    internal uintptr attr;
    internal nint state;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_attr_setdetachstate(ж<pthreadattr> attr, nint state) {
    ref var args = ref heap(new pthread_attr_setdetachstate_args((uintptr)attr.OrTypedNil(), state), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_attr_setdetachstate_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(attr.OrTypedNil());
    return ret;
}

internal static partial void pthread_attr_setdetachstate_trampoline();

internal partial struct pthread_create_args /*dyn*/ {
    internal uintptr attr;
    internal uintptr start;
    internal uintptr arg;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_create(ж<pthreadattr> attr, uintptr start, @unsafe.Pointer arg) {
    ref var args = ref heap(new pthread_create_args((uintptr)attr.OrTypedNil(), start, (uintptr)arg), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_create_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(attr.OrTypedNil());
    KeepAlive(@unsafe.Pointer.OrTypedNil(arg)); // Just for consistency. Arg of course needs to be kept alive for the start function.
    return ret;
}

internal static partial void pthread_create_trampoline();

internal partial struct raise_args /*dyn*/ {
    internal uint32 sig;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void raise(uint32 sig) {
    ref var args = ref heap(new raise_args(sig), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(raise_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void raise_trampoline();

//go:nosplit
//go:cgo_unsafe_args
internal static pthread /*t*/ pthread_self() {
    ref var t = ref heap(new pthread(), out var Ꮡt);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_self_trampoline), @unsafe.Pointer.FromBox(Ꮡt));
    return t;
}

internal static partial void pthread_self_trampoline();

internal partial struct pthread_kill_args /*dyn*/ {
    internal uintptr t;
    internal uint32 sig;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void pthread_kill(pthread t, uint32 sig) {
    ref var args = ref heap(new pthread_kill_args((uintptr)t, sig), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_kill_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    return;
}

internal static partial void pthread_kill_trampoline();

// osinit_hack is a clumsy hack to work around Apple libc bugs
// causing fork+exec to hang in the child process intermittently.
// See go.dev/issue/33565 and go.dev/issue/56784 for a few reports.
//
// The stacks obtained from the hung child processes are in
// libSystem_atfork_child, which is supposed to reinitialize various
// parts of the C library in the new process.
//
// One common stack dies in _notify_fork_child calling _notify_globals
// (inlined) calling _os_alloc_once, because _os_alloc_once detects that
// the once lock is held by the parent process and then calls
// _os_once_gate_corruption_abort. The allocation is setting up the
// globals for the notification subsystem. See the source code at [1].
// To work around this, we can allocate the globals earlier in the Go
// program's lifetime, before any execs are involved, by calling any
// notify routine that is exported, calls _notify_globals, and doesn't do
// anything too expensive otherwise. notify_is_valid_token(0) fits the bill.
//
// The other common stack dies in xpc_atfork_child calling
// _objc_msgSend_uncached which ends up in
// WAITING_FOR_ANOTHER_THREAD_TO_FINISH_CALLING_+initialize. Of course,
// whatever thread the child is waiting for is in the parent process and
// is not going to finish anything in the child process. There is no
// public source code for these routines, so it is unclear exactly what
// the problem is. An Apple engineer suggests using xpc_date_create_from_current,
// which empirically does fix the problem.
//
// So osinit_hack_trampoline (in sys_darwin_$GOARCH.s) calls
// notify_is_valid_token(0) and xpc_date_create_from_current(), which makes the
// fork+exec hangs stop happening. If Apple fixes the libc bug in
// some future version of macOS, then we can remove this awful code.
//
//go:nosplit
internal static void osinit_hack() {
    if (GOOS == "darwin"u8) { // not ios
        libcCall((@unsafe.Pointer)abi.FuncPCABI0(osinit_hack_trampoline), nil);
    }
    return;
}

internal static partial void osinit_hack_trampoline();

// go2cs generated this placeholder — func mmap is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void mmap_trampoline();

// go2cs generated this placeholder — func munmap is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void munmap_trampoline();

internal partial struct madvise_args /*dyn*/ {
    internal uintptr addr;
    internal uintptr n;
    internal int32 flags;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void madvise(@unsafe.Pointer addr, uintptr n, int32 flags) {
    ref var args = ref heap(new madvise_args((uintptr)addr, n, flags), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(madvise_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(@unsafe.Pointer.OrTypedNil(addr)); // Just for consistency. Hopefully addr is not a Go address.
}

internal static partial void madvise_trampoline();

internal partial struct mlock_args /*dyn*/ {
    internal uintptr addr;
    internal uintptr n;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void mlock(@unsafe.Pointer addr, uintptr n) {
    ref var args = ref heap(new mlock_args((uintptr)addr, n), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(mlock_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(@unsafe.Pointer.OrTypedNil(addr)); // Just for consistency. Hopefully addr is not a Go address.
}

internal static partial void mlock_trampoline();

// go2cs generated this placeholder — func read is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void read_trampoline();

// go2cs generated this placeholder — func pipe is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void pipe_trampoline();

internal partial struct closefd_args /*dyn*/ {
    internal int32 fd;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 closefd(int32 fd) {
    ref var args = ref heap(new closefd_args(fd), out var Ꮡargs);

    return libcCall((@unsafe.Pointer)abi.FuncPCABI0(close_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void close_trampoline();

internal partial struct exit_args /*dyn*/ {
    internal int32 code;
}

// This is exported via linkname to assembly in runtime/cgo.
//
//go:nosplit
//go:cgo_unsafe_args
//go:linkname exit
internal static void exit(int32 code) {
    ref var args = ref heap(new exit_args(code), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(exit_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void exit_trampoline();

internal partial struct usleep_args /*dyn*/ {
    internal uint32 usec;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void usleep(uint32 usec) {
    ref var args = ref heap(new usleep_args(usec), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(usleep_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void usleep_trampoline();

//go:nosplit
//go:cgo_unsafe_args
internal static void usleep_no_g(uint32 usecʗp) {
    ref var usec = ref heap(usecʗp, out var Ꮡusec);

    asmcgocall_no_g((@unsafe.Pointer)abi.FuncPCABI0(usleep_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡusec));
}

// go2cs generated this placeholder — func write1 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void write_trampoline();

internal partial struct open_args /*dyn*/ {
    internal uintptr name;
    internal int32 mode;
    internal int32 perm;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 /*ret*/ open(ж<byte> name, int32 mode, int32 perm) {
    int32 ret = default;
    ref var args = ref heap(new open_args((uintptr)name.OrTypedNil(), mode, perm), out var Ꮡargs);

    ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(open_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(name.OrTypedNil());
    return ret;
}

internal static partial void open_trampoline();

// go2cs generated this placeholder — func nanotime1 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void nanotime_trampoline();

// walltime should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname walltime
//go:nosplit
//go:cgo_unsafe_args
internal static (int64, int32) walltime() {
    ref var t = ref heap(new timespec(), out var Ꮡt);
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(walltime_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡt));
    return (t.tv_sec, (int32)t.tv_nsec);
}

internal static partial void walltime_trampoline();

// go2cs generated this placeholder — func sigaction is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void sigaction_trampoline();

// go2cs generated this placeholder — func sigprocmask is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void sigprocmask_trampoline();

internal partial struct sigaltstack_args /*dyn*/ {
    internal uintptr @new;
    internal uintptr old;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void sigaltstack(ж<stackt> Ꮡnew, ж<stackt> old) {
    ref var @new = ref Ꮡnew.DerefOrNull();
    ref var args = ref heap(new sigaltstack_args((uintptr)Ꮡnew.OrTypedNil(), (uintptr)old.OrTypedNil()), out var Ꮡargs);

    if (Ꮡnew != nil && (@new.ss_flags & (int32)_SS_DISABLE) != 0 && @new.ss_size == 0) {
        // Despite the fact that Darwin's sigaltstack man page says it ignores the size
        // when SS_DISABLE is set, it doesn't. sigaltstack returns ENOMEM
        // if we don't give it a reasonable size.
        // ref: http://lists.llvm.org/pipermail/llvm-commits/Week-of-Mon-20140421/214296.html
        @new.ss_size = 32768;
    }
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(sigaltstack_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡnew.OrTypedNil());
    KeepAlive(old.OrTypedNil());
}

internal static partial void sigaltstack_trampoline();

internal partial struct raiseproc_args /*dyn*/ {
    internal uint32 sig;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void raiseproc(uint32 sig) {
    ref var args = ref heap(new raiseproc_args(sig), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(raiseproc_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void raiseproc_trampoline();

internal partial struct setitimer_args /*dyn*/ {
    internal int32 mode;
    internal uintptr @new;
    internal uintptr old;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void setitimer(int32 mode, ж<itimerval> @new, ж<itimerval> old) {
    ref var args = ref heap(new setitimer_args(mode, (uintptr)@new.OrTypedNil(), (uintptr)old.OrTypedNil()), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(setitimer_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(@new.OrTypedNil());
    KeepAlive(old.OrTypedNil());
}

internal static partial void setitimer_trampoline();

internal partial struct sysctl_args /*dyn*/ {
    internal uintptr mib;
    internal uint32 miblen;
    internal uintptr oldp;
    internal uintptr oldlenp;
    internal uintptr newp;
    internal uintptr newlen;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 sysctl(ж<uint32> mib, uint32 miblen, ж<byte> oldp, ж<uintptr> oldlenp, ж<byte> newp, uintptr newlen) {
    ref var args = ref heap(new sysctl_args((uintptr)mib.OrTypedNil(), miblen, (uintptr)oldp.OrTypedNil(), (uintptr)oldlenp.OrTypedNil(), (uintptr)newp.OrTypedNil(), newlen), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(sysctl_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(mib.OrTypedNil());
    KeepAlive(oldp.OrTypedNil());
    KeepAlive(oldlenp.OrTypedNil());
    KeepAlive(newp.OrTypedNil());
    return ret;
}

internal static partial void sysctl_trampoline();

internal partial struct sysctlbyname_args /*dyn*/ {
    internal uintptr name;
    internal uintptr oldp;
    internal uintptr oldlenp;
    internal uintptr newp;
    internal uintptr newlen;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 sysctlbyname(ж<byte> name, ж<byte> oldp, ж<uintptr> oldlenp, ж<byte> newp, uintptr newlen) {
    ref var args = ref heap(new sysctlbyname_args((uintptr)name.OrTypedNil(), (uintptr)oldp.OrTypedNil(), (uintptr)oldlenp.OrTypedNil(), (uintptr)newp.OrTypedNil(), newlen), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(sysctlbyname_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(name.OrTypedNil());
    KeepAlive(oldp.OrTypedNil());
    KeepAlive(oldlenp.OrTypedNil());
    KeepAlive(newp.OrTypedNil());
    return ret;
}

internal static partial void sysctlbyname_trampoline();

internal partial struct fcntl_args /*dyn*/ {
    internal int32 fd, cmd, arg;
    internal int32 ret, errno;
}

//go:nosplit
//go:cgo_unsafe_args
public static (int32 ret, int32 errno) fcntl(int32 fd, int32 cmd, int32 arg) {
    ref var args = ref heap<fcntl_args>(new fcntl_args(fd, cmd, arg, 0, 0), out var Ꮡargs);
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(fcntl_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    return (args.ret, args.errno);
}

internal static partial void fcntl_trampoline();

//go:nosplit
//go:cgo_unsafe_args
internal static int32 kqueue() {
    var v = libcCall((@unsafe.Pointer)abi.FuncPCABI0(kqueue_trampoline), nil);
    return v;
}

internal static partial void kqueue_trampoline();

internal partial struct kevent_args /*dyn*/ {
    internal int32 kq;
    internal uintptr ch;
    internal int32 nch;
    internal uintptr ev;
    internal int32 nev;
    internal uintptr ts;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 kevent(int32 kq, ж<keventt> ch, int32 nch, ж<keventt> ev, int32 nev, ж<timespec> ts) {
    ref var args = ref heap(new kevent_args(kq, (uintptr)ch.OrTypedNil(), nch, (uintptr)ev.OrTypedNil(), nev, (uintptr)ts.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(kevent_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(ch.OrTypedNil());
    KeepAlive(ev.OrTypedNil());
    KeepAlive(ts.OrTypedNil());
    return ret;
}

internal static partial void kevent_trampoline();

internal partial struct pthread_mutex_init_args /*dyn*/ {
    internal uintptr m;
    internal uintptr attr;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_mutex_init(ж<pthreadmutex> m, ж<pthreadmutexattr> attr) {
    ref var args = ref heap(new pthread_mutex_init_args((uintptr)m.OrTypedNil(), (uintptr)attr.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_mutex_init_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(m.OrTypedNil());
    KeepAlive(attr.OrTypedNil());
    return ret;
}

internal static partial void pthread_mutex_init_trampoline();

internal partial struct pthread_mutex_lock_args /*dyn*/ {
    internal uintptr m;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_mutex_lock(ж<pthreadmutex> m) {
    ref var args = ref heap(new pthread_mutex_lock_args((uintptr)m.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_mutex_lock_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(m.OrTypedNil());
    return ret;
}

internal static partial void pthread_mutex_lock_trampoline();

internal partial struct pthread_mutex_unlock_args /*dyn*/ {
    internal uintptr m;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_mutex_unlock(ж<pthreadmutex> m) {
    ref var args = ref heap(new pthread_mutex_unlock_args((uintptr)m.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_mutex_unlock_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(m.OrTypedNil());
    return ret;
}

internal static partial void pthread_mutex_unlock_trampoline();

internal partial struct pthread_cond_init_args /*dyn*/ {
    internal uintptr c;
    internal uintptr attr;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_cond_init(ж<pthreadcond> c, ж<pthreadcondattr> attr) {
    ref var args = ref heap(new pthread_cond_init_args((uintptr)c.OrTypedNil(), (uintptr)attr.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_cond_init_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(c.OrTypedNil());
    KeepAlive(attr.OrTypedNil());
    return ret;
}

internal static partial void pthread_cond_init_trampoline();

internal partial struct pthread_cond_wait_args /*dyn*/ {
    internal uintptr c;
    internal uintptr m;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_cond_wait(ж<pthreadcond> c, ж<pthreadmutex> m) {
    ref var args = ref heap(new pthread_cond_wait_args((uintptr)c.OrTypedNil(), (uintptr)m.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_cond_wait_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(c.OrTypedNil());
    KeepAlive(m.OrTypedNil());
    return ret;
}

internal static partial void pthread_cond_wait_trampoline();

internal partial struct pthread_cond_timedwait_relative_np_args /*dyn*/ {
    internal uintptr c;
    internal uintptr m;
    internal uintptr t;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_cond_timedwait_relative_np(ж<pthreadcond> c, ж<pthreadmutex> m, ж<timespec> t) {
    ref var args = ref heap(new pthread_cond_timedwait_relative_np_args((uintptr)c.OrTypedNil(), (uintptr)m.OrTypedNil(), (uintptr)t.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_cond_timedwait_relative_np_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(c.OrTypedNil());
    KeepAlive(m.OrTypedNil());
    KeepAlive(t.OrTypedNil());
    return ret;
}

internal static partial void pthread_cond_timedwait_relative_np_trampoline();

internal partial struct pthread_cond_signal_args /*dyn*/ {
    internal uintptr c;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_cond_signal(ж<pthreadcond> c) {
    ref var args = ref heap(new pthread_cond_signal_args((uintptr)c.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_cond_signal_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(c.OrTypedNil());
    return ret;
}

internal static partial void pthread_cond_signal_trampoline();

internal partial struct arc4random_buf_args /*dyn*/ {
    internal uintptr Δp;
    internal int32 n;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void arc4random_buf(@unsafe.Pointer Δp, int32 n) {
    ref var args = ref heap(new arc4random_buf_args((uintptr)Δp, n), out var Ꮡargs);

    // arc4random_buf() never fails, per its man page, so it's safe to ignore the return value.
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(arc4random_buf_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(@unsafe.Pointer.OrTypedNil(Δp));
}

internal static partial void arc4random_buf_trampoline();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exitThreadˢ = "exitThread"u8;

// Not used on Darwin, but must be defined.
internal static void exitThread(ref atomic.Uint32 wait) {
    @throw(exitThreadˢ);
}

//go:nosplit
internal static void setNonblock(int32 fd) {
    var (flags, _) = fcntl(fd, _F_GETFL, 0);
    if (flags != -1) {
        fcntl(fd, _F_SETFL, flags | (int32)_O_NONBLOCK);
    }
}

internal static int32 issetugid() {
    return libcCall((@unsafe.Pointer)abi.FuncPCABI0(issetugid_trampoline), nil);
}

internal static partial void issetugid_trampoline();

internal partial struct mach_vm_region_args /*dyn*/ {
    internal ж<uint64> address;
    internal ж<uint64> size;
    internal machVMRegionFlavour flavor;
    internal @unsafe.Pointer info;
    internal ж<machMsgTypeNumber> count;
    internal ж<machPort> object_name;
}

// mach_vm_region is used to obtain virtual memory mappings for use by the
// profiling system and is only exported to runtime/pprof. It is restricted
// to obtaining mappings for the current process.
//
//go:linkname mach_vm_region runtime/pprof.mach_vm_region
public static int32 mach_vm_region(ж<uint64> address, ж<uint64> region_size, @unsafe.Pointer info) {
    // kern_return_t mach_vm_region(
    // 	vm_map_read_t target_task,
    // 	mach_vm_address_t *address,
    // 	mach_vm_size_t *size,
    // 	vm_region_flavor_t flavor,
    // 	vm_region_info_t info,
    // 	mach_msg_type_number_t *infoCnt,
    // 	mach_port_t *object_name);
    ref var count = ref heap(new machMsgTypeNumber(), out var Ꮡcount);
    count = _VM_REGION_BASIC_INFO_COUNT_64;
    ref var object_name = ref heap(new machPort(), out var Ꮡobject_name);
    ref var args = ref heap<mach_vm_region_args>(new mach_vm_region_args(
        address: address,
        size: region_size,
        flavor: _VM_REGION_BASIC_INFO_64,
        info: info,
        count: Ꮡcount,
        object_name: Ꮡobject_name
    ), out var Ꮡargs);
    return libcCall((@unsafe.Pointer)abi.FuncPCABI0(mach_vm_region_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void mach_vm_region_trampoline();

internal partial struct proc_regionfilename_args /*dyn*/ {
    internal nint pid;
    internal uint64 address;
    internal ж<byte> buf;
    internal int64 bufSize;
}

//go:linkname proc_regionfilename runtime/pprof.proc_regionfilename
public static int32 proc_regionfilename(nint pid, uint64 address, ж<byte> buf, int64 buflen) {
    ref var args = ref heap<proc_regionfilename_args>(new proc_regionfilename_args(
        pid: pid,
        address: address,
        buf: buf,
        bufSize: buflen
    ), out var Ꮡargs);
    return libcCall((@unsafe.Pointer)abi.FuncPCABI0(proc_regionfilename_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void proc_regionfilename_trampoline();

// Tell the linker that the libc_* functions are to be found
// in a system library, with the libc_ prefix missing.
//go:cgo_import_dynamic libc_pthread_attr_init pthread_attr_init "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_attr_getstacksize pthread_attr_getstacksize "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_attr_setdetachstate pthread_attr_setdetachstate "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_create pthread_create "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_self pthread_self "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_kill pthread_kill "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_exit _exit "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_raise raise "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_open open "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_close close "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_read read "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_write write "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pipe pipe "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_mmap mmap "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_munmap munmap "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_madvise madvise "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_mlock mlock "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_error __error "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_usleep usleep "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_proc_regionfilename proc_regionfilename "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_mach_task_self_ mach_task_self_ "/usr/lib/libSystem.B.dylib""
//go:cgo_import_dynamic libc_mach_vm_region mach_vm_region "/usr/lib/libSystem.B.dylib""
//go:cgo_import_dynamic libc_mach_timebase_info mach_timebase_info "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_mach_absolute_time mach_absolute_time "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_clock_gettime clock_gettime "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_sigaction sigaction "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_sigmask pthread_sigmask "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_sigaltstack sigaltstack "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_getpid getpid "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_kill kill "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_setitimer setitimer "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_sysctl sysctl "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_sysctlbyname sysctlbyname "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_fcntl fcntl "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_kqueue kqueue "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_kevent kevent "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_mutex_init pthread_mutex_init "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_mutex_lock pthread_mutex_lock "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_mutex_unlock pthread_mutex_unlock "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_cond_init pthread_cond_init "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_cond_wait pthread_cond_wait "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_cond_timedwait_relative_np pthread_cond_timedwait_relative_np "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_cond_signal pthread_cond_signal "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_arc4random_buf arc4random_buf "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_notify_is_valid_token notify_is_valid_token "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_xpc_date_create_from_current xpc_date_create_from_current "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_issetugid issetugid "/usr/lib/libSystem.B.dylib"

} // end runtime_package
