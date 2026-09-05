// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

[GoType("dyn")] internal partial struct syscall_syscall_args {
    internal uintptr fn, a1, a2, a3, r1, r2, err;
}

// The X versions of syscall expect the libc call to return a 64-bit result.
// Otherwise (the non-X version) expects a 32-bit result.
// This distinction is required because an error is indicated by returning -1,
// and we need to know whether to check 32 or 64 bits of the result.
// (Some libc functions that return 32 bits put junk in the upper 32 bits of AX.)

// golang.org/x/sys linknames syscall_syscall
// (in addition to standard package syscall).
// Do not remove or change the type signature.
//
//go:linkname syscall_syscall syscall.syscall
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    uintptr err = default!;

    ref var args = ref heap<syscall_syscall_args>(out var Ꮡargs);
    args = new syscall_syscall_args(fn, a1, a2, a3, r1, r2, err);
    entersyscall();
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscall), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    exitsyscall();
    return (args.r1, args.r2, args.err);
}

internal static partial void syscall();

[GoType("dyn")] internal partial struct syscall_syscallX_args {
    internal uintptr fn, a1, a2, a3, r1, r2, err;
}

//go:linkname syscall_syscallX syscall.syscallX
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_syscallX(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    uintptr err = default!;

    ref var args = ref heap<syscall_syscallX_args>(out var Ꮡargs);
    args = new syscall_syscallX_args(fn, a1, a2, a3, r1, r2, err);
    entersyscall();
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscallX), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    exitsyscall();
    return (args.r1, args.r2, args.err);
}

internal static partial void syscallX();

[GoType("dyn")] internal partial struct syscall_syscall6_args {
    internal uintptr fn, a1, a2, a3, a4, a5, a6, r1, r2, err;
}

// golang.org/x/sys linknames syscall.syscall6
// (in addition to standard package syscall).
// Do not remove or change the type signature.
//
// syscall.syscall6 is meant for package syscall (and x/sys),
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/tetratelabs/wazero
//
// See go.dev/issue/67401.
//
//go:linkname syscall_syscall6 syscall.syscall6
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_syscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    uintptr err = default!;

    ref var args = ref heap<syscall_syscall6_args>(out var Ꮡargs);
    args = new syscall_syscall6_args(fn, a1, a2, a3, a4, a5, a6, r1, r2, err);
    entersyscall();
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscall6), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    exitsyscall();
    return (args.r1, args.r2, args.err);
}

internal static partial void syscall6();

[GoType("dyn")] internal partial struct syscall_syscall9_args {
    internal uintptr fn;
    internal uintptr a1;
    internal uintptr a2;
    internal uintptr a3;
    internal uintptr a4;
    internal uintptr a5;
    internal uintptr a6;
    internal uintptr a7;
    internal uintptr a8;
    internal uintptr a9;
}

// golang.org/x/sys linknames syscall.syscall9
// (in addition to standard package syscall).
// Do not remove or change the type signature.
//
//go:linkname syscall_syscall9 syscall.syscall9
//go:nosplit
//go:cgo_unsafe_args
internal static (uintptr r1, uintptr r2, uintptr err) syscall_syscall9(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    uintptr err = default!;

    ref var args = ref heap(new syscall_syscall9_args(fn, a1, a2, a3, a4, a5, a6, a7, a8, a9), out var Ꮡargs);
    entersyscall();
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscall9), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    exitsyscall();
    return (r1, r2, err);
}

internal static partial void syscall9();

[GoType("dyn")] internal partial struct syscall_syscall6X_args {
    internal uintptr fn, a1, a2, a3, a4, a5, a6, r1, r2, err;
}

//go:linkname syscall_syscall6X syscall.syscall6X
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_syscall6X(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    uintptr err = default!;

    ref var args = ref heap<syscall_syscall6X_args>(out var Ꮡargs);
    args = new syscall_syscall6X_args(fn, a1, a2, a3, a4, a5, a6, r1, r2, err);
    entersyscall();
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscall6X), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    exitsyscall();
    return (args.r1, args.r2, args.err);
}

internal static partial void syscall6X();

[GoType("dyn")] internal partial struct syscall_syscallPtr_args {
    internal uintptr fn, a1, a2, a3, r1, r2, err;
}

// golang.org/x/sys linknames syscall.syscallPtr
// (in addition to standard package syscall).
// Do not remove or change the type signature.
//
//go:linkname syscall_syscallPtr syscall.syscallPtr
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_syscallPtr(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    uintptr err = default!;

    ref var args = ref heap<syscall_syscallPtr_args>(out var Ꮡargs);
    args = new syscall_syscallPtr_args(fn, a1, a2, a3, r1, r2, err);
    entersyscall();
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscallPtr), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    exitsyscall();
    return (args.r1, args.r2, args.err);
}

internal static partial void syscallPtr();

[GoType("dyn")] internal partial struct syscall_rawSyscall_args {
    internal uintptr fn, a1, a2, a3, r1, r2, err;
}

// golang.org/x/sys linknames syscall_rawSyscall
// (in addition to standard package syscall).
// Do not remove or change the type signature.
//
//go:linkname syscall_rawSyscall syscall.rawSyscall
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_rawSyscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    uintptr err = default!;

    ref var args = ref heap<syscall_rawSyscall_args>(out var Ꮡargs);
    args = new syscall_rawSyscall_args(fn, a1, a2, a3, r1, r2, err);
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscall), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    return (args.r1, args.r2, args.err);
}

[GoType("dyn")] internal partial struct syscall_rawSyscall6_args {
    internal uintptr fn, a1, a2, a3, a4, a5, a6, r1, r2, err;
}

// golang.org/x/sys linknames syscall_rawSyscall6
// (in addition to standard package syscall).
// Do not remove or change the type signature.
//
//go:linkname syscall_rawSyscall6 syscall.rawSyscall6
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_rawSyscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    uintptr err = default!;

    ref var args = ref heap<syscall_rawSyscall6_args>(out var Ꮡargs);
    args = new syscall_rawSyscall6_args(fn, a1, a2, a3, a4, a5, a6, r1, r2, err);
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscall6), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    return (args.r1, args.r2, args.err);
}

[GoType("dyn")] internal partial struct crypto_x509_syscall_args {
    internal uintptr fn, a1, a2, a3, a4, a5;
    internal float64 f1;
    internal uintptr r1;
}

// crypto_x509_syscall is used in crypto/x509/internal/macos to call into Security.framework and CF.

//go:linkname crypto_x509_syscall crypto/x509/internal/macos.syscall
//go:nosplit
internal static uintptr /*r1*/ crypto_x509_syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, float64 f1) {
    uintptr r1 = default!;

    ref var args = ref heap<crypto_x509_syscall_args>(out var Ꮡargs);
    args = new crypto_x509_syscall_args(fn, a1, a2, a3, a4, a5, f1, r1);
    entersyscall();
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(syscall_x509), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    exitsyscall();
    return args.r1;
}

internal static partial void syscall_x509();

[GoType("dyn")] internal partial struct pthread_attr_init_args {
    internal uintptr attr;
}

// The *_trampoline functions convert from the Go calling convention to the C calling convention
// and then call the underlying libc function.  They are defined in sys_darwin_$ARCH.s.

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_attr_init(ж<pthreadattr> Ꮡattr) {
    ref var args = ref heap(new pthread_attr_init_args((uintptr)Ꮡattr.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_attr_init_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡattr.OrTypedNil());
    return ret;
}

internal static partial void pthread_attr_init_trampoline();

[GoType("dyn")] internal partial struct pthread_attr_getstacksize_args {
    internal uintptr attr;
    internal uintptr size;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_attr_getstacksize(ж<pthreadattr> Ꮡattr, ж<uintptr> Ꮡsize) {
    ref var args = ref heap(new pthread_attr_getstacksize_args((uintptr)Ꮡattr.OrTypedNil(), (uintptr)Ꮡsize.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_attr_getstacksize_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡattr.OrTypedNil());
    KeepAlive(Ꮡsize.OrTypedNil());
    return ret;
}

internal static partial void pthread_attr_getstacksize_trampoline();

[GoType("dyn")] internal partial struct pthread_attr_setdetachstate_args {
    internal uintptr attr;
    internal nint state;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_attr_setdetachstate(ж<pthreadattr> Ꮡattr, nint state) {
    ref var args = ref heap(new pthread_attr_setdetachstate_args((uintptr)Ꮡattr.OrTypedNil(), state), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_attr_setdetachstate_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡattr.OrTypedNil());
    return ret;
}

internal static partial void pthread_attr_setdetachstate_trampoline();

[GoType("dyn")] internal partial struct pthread_create_args {
    internal uintptr attr;
    internal uintptr start;
    internal uintptr arg;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_create(ж<pthreadattr> Ꮡattr, uintptr start, @unsafe.Pointer arg) {
    ref var args = ref heap(new pthread_create_args((uintptr)Ꮡattr.OrTypedNil(), start, (uintptr)arg), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_create_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡattr.OrTypedNil());
    KeepAlive(arg); // Just for consistency. Arg of course needs to be kept alive for the start function.
    return ret;
}

internal static partial void pthread_create_trampoline();

[GoType("dyn")] internal partial struct raise_args {
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

[GoType("dyn")] internal partial struct pthread_kill_args {
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
    if (GOOS == "darwin"u8) {
        // not ios
        libcCall((@unsafe.Pointer)abi.FuncPCABI0(osinit_hack_trampoline), nil);
    }
    return;
}

internal static partial void osinit_hack_trampoline();

[GoType("dyn")] internal partial struct mmap_args {
    internal @unsafe.Pointer addr;
    internal uintptr n;
    internal int32 prot, flags, fd;
    internal uint32 off;
    internal @unsafe.Pointer ret1;
    internal nint ret2;
}

// mmap is used to do low-level memory allocation via mmap. Don't allow stack
// splits, since this function (used by sysAlloc) is called in a lot of low-level
// parts of the runtime and callers often assume it won't acquire any locks.
//
//go:nosplit
internal static (@unsafe.Pointer, nint) mmap(@unsafe.Pointer addr, uintptr n, int32 prot, int32 flags, int32 fd, uint32 off) {
    ref var args = ref heap<mmap_args>(out var Ꮡargs);
    args = new mmap_args(addr.Value, n, prot, flags, fd, off, nil, 0);
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(mmap_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    return (args.ret1, args.ret2);
}

internal static partial void mmap_trampoline();

[GoType("dyn")] internal partial struct munmap_args {
    internal uintptr addr;
    internal uintptr n;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void munmap(@unsafe.Pointer addr, uintptr n) {
    ref var args = ref heap(new munmap_args((uintptr)addr, n), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(munmap_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(addr); // Just for consistency. Hopefully addr is not a Go address.
}

internal static partial void munmap_trampoline();

[GoType("dyn")] internal partial struct madvise_args {
    internal uintptr addr;
    internal uintptr n;
    internal int32 flags;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void madvise(@unsafe.Pointer addr, uintptr n, int32 flags) {
    ref var args = ref heap(new madvise_args((uintptr)addr, n, flags), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(madvise_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(addr); // Just for consistency. Hopefully addr is not a Go address.
}

internal static partial void madvise_trampoline();

[GoType("dyn")] internal partial struct mlock_args {
    internal uintptr addr;
    internal uintptr n;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void mlock(@unsafe.Pointer addr, uintptr n) {
    ref var args = ref heap(new mlock_args((uintptr)addr, n), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(mlock_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(addr); // Just for consistency. Hopefully addr is not a Go address.
}

internal static partial void mlock_trampoline();

// go2cs generated this placeholder — func read is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void read_trampoline();

// go2cs generated this placeholder — func pipe is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void pipe_trampoline();

[GoType("dyn")] internal partial struct closefd_args {
    internal int32 fd;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 closefd(int32 fd) {
    ref var args = ref heap(new closefd_args(fd), out var Ꮡargs);

    return libcCall((@unsafe.Pointer)abi.FuncPCABI0(close_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void close_trampoline();

[GoType("dyn")] internal partial struct exit_args {
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

[GoType("dyn")] internal partial struct usleep_args {
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

[GoType("dyn")] internal partial struct open_args {
    internal uintptr name;
    internal int32 mode;
    internal int32 perm;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 /*ret*/ open(ж<byte> Ꮡname, int32 mode, int32 perm) {
    int32 ret = default!;

    ref var args = ref heap(new open_args((uintptr)Ꮡname.OrTypedNil(), mode, perm), out var Ꮡargs);
    ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(open_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡname.OrTypedNil());
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

[GoType("dyn")] internal partial struct sigaltstack_args {
    internal uintptr @new;
    internal uintptr old;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void sigaltstack(ж<stackt> Ꮡnew, ж<stackt> Ꮡold) {
    ref var @new = ref Ꮡnew.DerefOrNull();

    ref var args = ref heap(new sigaltstack_args((uintptr)Ꮡnew.OrTypedNil(), (uintptr)Ꮡold.OrTypedNil()), out var Ꮡargs);
    if (Ꮡnew != nil && (int32)(@new.ss_flags & (int32)_SS_DISABLE) != 0 && @new.ss_size == 0) {
        // Despite the fact that Darwin's sigaltstack man page says it ignores the size
        // when SS_DISABLE is set, it doesn't. sigaltstack returns ENOMEM
        // if we don't give it a reasonable size.
        // ref: http://lists.llvm.org/pipermail/llvm-commits/Week-of-Mon-20140421/214296.html
        @new.ss_size = 32768;
    }
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(sigaltstack_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡnew.OrTypedNil());
    KeepAlive(Ꮡold.OrTypedNil());
}

internal static partial void sigaltstack_trampoline();

[GoType("dyn")] internal partial struct raiseproc_args {
    internal uint32 sig;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void raiseproc(uint32 sig) {
    ref var args = ref heap(new raiseproc_args(sig), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(raiseproc_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void raiseproc_trampoline();

[GoType("dyn")] internal partial struct setitimer_args {
    internal int32 mode;
    internal uintptr @new;
    internal uintptr old;
}

//go:nosplit
//go:cgo_unsafe_args
internal static void setitimer(int32 mode, ж<itimerval> Ꮡnew, ж<itimerval> Ꮡold) {
    ref var args = ref heap(new setitimer_args(mode, (uintptr)Ꮡnew.OrTypedNil(), (uintptr)Ꮡold.OrTypedNil()), out var Ꮡargs);

    libcCall((@unsafe.Pointer)abi.FuncPCABI0(setitimer_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡnew.OrTypedNil());
    KeepAlive(Ꮡold.OrTypedNil());
}

internal static partial void setitimer_trampoline();

[GoType("dyn")] internal partial struct sysctl_args {
    internal uintptr mib;
    internal uint32 miblen;
    internal uintptr oldp;
    internal uintptr oldlenp;
    internal uintptr newp;
    internal uintptr newlen;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 sysctl(ж<uint32> Ꮡmib, uint32 miblen, ж<byte> Ꮡoldp, ж<uintptr> Ꮡoldlenp, ж<byte> Ꮡnewp, uintptr newlen) {
    ref var args = ref heap(new sysctl_args((uintptr)Ꮡmib.OrTypedNil(), miblen, (uintptr)Ꮡoldp.OrTypedNil(), (uintptr)Ꮡoldlenp.OrTypedNil(), (uintptr)Ꮡnewp.OrTypedNil(), newlen), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(sysctl_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡmib.OrTypedNil());
    KeepAlive(Ꮡoldp.OrTypedNil());
    KeepAlive(Ꮡoldlenp.OrTypedNil());
    KeepAlive(Ꮡnewp.OrTypedNil());
    return ret;
}

internal static partial void sysctl_trampoline();

[GoType("dyn")] internal partial struct sysctlbyname_args {
    internal uintptr name;
    internal uintptr oldp;
    internal uintptr oldlenp;
    internal uintptr newp;
    internal uintptr newlen;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 sysctlbyname(ж<byte> Ꮡname, ж<byte> Ꮡoldp, ж<uintptr> Ꮡoldlenp, ж<byte> Ꮡnewp, uintptr newlen) {
    ref var args = ref heap(new sysctlbyname_args((uintptr)Ꮡname.OrTypedNil(), (uintptr)Ꮡoldp.OrTypedNil(), (uintptr)Ꮡoldlenp.OrTypedNil(), (uintptr)Ꮡnewp.OrTypedNil(), newlen), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(sysctlbyname_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡname.OrTypedNil());
    KeepAlive(Ꮡoldp.OrTypedNil());
    KeepAlive(Ꮡoldlenp.OrTypedNil());
    KeepAlive(Ꮡnewp.OrTypedNil());
    return ret;
}

internal static partial void sysctlbyname_trampoline();

[GoType("dyn")] internal partial struct fcntl_args {
    internal int32 fd, cmd, arg;
    internal int32 ret, errno;
}

//go:nosplit
//go:cgo_unsafe_args
public static (int32 ret, int32 errno) fcntl(int32 fd, int32 cmd, int32 arg) {
    ref var args = ref heap<fcntl_args>(out var Ꮡargs);
    args = new fcntl_args(fd, cmd, arg, 0, 0);
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

[GoType("dyn")] internal partial struct kevent_args {
    internal int32 kq;
    internal uintptr ch;
    internal int32 nch;
    internal uintptr ev;
    internal int32 nev;
    internal uintptr ts;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 kevent(int32 kq, ж<keventt> Ꮡch, int32 nch, ж<keventt> Ꮡev, int32 nev, ж<timespec> Ꮡts) {
    ref var args = ref heap(new kevent_args(kq, (uintptr)Ꮡch.OrTypedNil(), nch, (uintptr)Ꮡev.OrTypedNil(), nev, (uintptr)Ꮡts.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(kevent_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡch.OrTypedNil());
    KeepAlive(Ꮡev.OrTypedNil());
    KeepAlive(Ꮡts.OrTypedNil());
    return ret;
}

internal static partial void kevent_trampoline();

[GoType("dyn")] internal partial struct pthread_mutex_init_args {
    internal uintptr m;
    internal uintptr attr;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_mutex_init(ж<pthreadmutex> Ꮡm, ж<pthreadmutexattr> Ꮡattr) {
    ref var args = ref heap(new pthread_mutex_init_args((uintptr)Ꮡm.OrTypedNil(), (uintptr)Ꮡattr.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_mutex_init_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡm.OrTypedNil());
    KeepAlive(Ꮡattr.OrTypedNil());
    return ret;
}

internal static partial void pthread_mutex_init_trampoline();

[GoType("dyn")] internal partial struct pthread_mutex_lock_args {
    internal uintptr m;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_mutex_lock(ж<pthreadmutex> Ꮡm) {
    ref var args = ref heap(new pthread_mutex_lock_args((uintptr)Ꮡm.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_mutex_lock_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡm.OrTypedNil());
    return ret;
}

internal static partial void pthread_mutex_lock_trampoline();

[GoType("dyn")] internal partial struct pthread_mutex_unlock_args {
    internal uintptr m;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_mutex_unlock(ж<pthreadmutex> Ꮡm) {
    ref var args = ref heap(new pthread_mutex_unlock_args((uintptr)Ꮡm.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_mutex_unlock_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡm.OrTypedNil());
    return ret;
}

internal static partial void pthread_mutex_unlock_trampoline();

[GoType("dyn")] internal partial struct pthread_cond_init_args {
    internal uintptr c;
    internal uintptr attr;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_cond_init(ж<pthreadcond> Ꮡc, ж<pthreadcondattr> Ꮡattr) {
    ref var args = ref heap(new pthread_cond_init_args((uintptr)Ꮡc.OrTypedNil(), (uintptr)Ꮡattr.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_cond_init_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡc.OrTypedNil());
    KeepAlive(Ꮡattr.OrTypedNil());
    return ret;
}

internal static partial void pthread_cond_init_trampoline();

[GoType("dyn")] internal partial struct pthread_cond_wait_args {
    internal uintptr c;
    internal uintptr m;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_cond_wait(ж<pthreadcond> Ꮡc, ж<pthreadmutex> Ꮡm) {
    ref var args = ref heap(new pthread_cond_wait_args((uintptr)Ꮡc.OrTypedNil(), (uintptr)Ꮡm.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_cond_wait_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡc.OrTypedNil());
    KeepAlive(Ꮡm.OrTypedNil());
    return ret;
}

internal static partial void pthread_cond_wait_trampoline();

[GoType("dyn")] internal partial struct pthread_cond_timedwait_relative_np_args {
    internal uintptr c;
    internal uintptr m;
    internal uintptr t;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_cond_timedwait_relative_np(ж<pthreadcond> Ꮡc, ж<pthreadmutex> Ꮡm, ж<timespec> Ꮡt) {
    ref var args = ref heap(new pthread_cond_timedwait_relative_np_args((uintptr)Ꮡc.OrTypedNil(), (uintptr)Ꮡm.OrTypedNil(), (uintptr)Ꮡt.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_cond_timedwait_relative_np_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡc.OrTypedNil());
    KeepAlive(Ꮡm.OrTypedNil());
    KeepAlive(Ꮡt.OrTypedNil());
    return ret;
}

internal static partial void pthread_cond_timedwait_relative_np_trampoline();

[GoType("dyn")] internal partial struct pthread_cond_signal_args {
    internal uintptr c;
}

//go:nosplit
//go:cgo_unsafe_args
internal static int32 pthread_cond_signal(ж<pthreadcond> Ꮡc) {
    ref var args = ref heap(new pthread_cond_signal_args((uintptr)Ꮡc.OrTypedNil()), out var Ꮡargs);

    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_cond_signal_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
    KeepAlive(Ꮡc.OrTypedNil());
    return ret;
}

internal static partial void pthread_cond_signal_trampoline();

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
        fcntl(fd, _F_SETFL, (int32)(flags | (int32)_O_NONBLOCK));
    }
}

internal static int32 issetugid() {
    return libcCall((@unsafe.Pointer)abi.FuncPCABI0(issetugid_trampoline), nil);
}

internal static partial void issetugid_trampoline();

[GoType("dyn")] internal partial struct mach_vm_region_args {
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
internal static int32 mach_vm_region(ж<uint64> Ꮡaddress, ж<uint64> Ꮡregion_size, @unsafe.Pointer info) {
    ref var address = ref Ꮡaddress.DerefOrNull();

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
    ref var args = ref heap<mach_vm_region_args>(out var Ꮡargs);
    args = new mach_vm_region_args(
        address: Ꮡaddress,
        size: Ꮡregion_size,
        flavor: _VM_REGION_BASIC_INFO_64,
        info: info,
        count: Ꮡcount,
        object_name: Ꮡobject_name
    );
    return libcCall((@unsafe.Pointer)abi.FuncPCABI0(mach_vm_region_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static partial void mach_vm_region_trampoline();

[GoType("dyn")] internal partial struct proc_regionfilename_args {
    internal nint pid;
    internal uint64 address;
    internal ж<byte> buf;
    internal int64 bufSize;
}

//go:linkname proc_regionfilename runtime/pprof.proc_regionfilename
internal static int32 proc_regionfilename(nint pid, uint64 address, ж<byte> Ꮡbuf, int64 buflen) {
    ref var args = ref heap<proc_regionfilename_args>(out var Ꮡargs);
    args = new proc_regionfilename_args(
        pid: pid,
        address: address,
        buf: Ꮡbuf,
        bufSize: buflen
    );
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
//go:cgo_import_dynamic libc_notify_is_valid_token notify_is_valid_token "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_xpc_date_create_from_current xpc_date_create_from_current "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_issetugid issetugid "/usr/lib/libSystem.B.dylib"

} // end runtime_package
