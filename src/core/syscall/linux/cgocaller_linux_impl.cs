// cgocaller_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The `cgocaller` keystone, Linux half — DESIGN-cgocaller-keystone.md §2, increment 1.
//
// syscall_linux.go gives every credential setter TWO implementations selected on one nil test:
//
//     func Setegid(egid int) (err error) {
//         if cgo_libc_setegid == nil {
//             if _, _, e1 := AllThreadsSyscall(SYS_SETRESGID, minus1, uintptr(egid), minus1); e1 != 0 { … }
//         } else if ret := cgocaller(cgo_libc_setegid, uintptr(egid)); ret != 0 { … }
//     }
//
// The converted corpus took the FIRST branch, reaching runtime_doAllThreadsSyscall, whose banked
// hand-own answers ENOTSUP by design (syscall_linux_impl.cs — a managed host owns threads Go's
// runtime never sees, so it cannot honestly promise a process-wide broadcast). That is the right
// answer for the RAW api and an impossible one for these nine wrappers: whichever branch a Go build
// takes, the wrappers WORK. All 21 rows of TestSetuidEtc failed with one string,
// "operation not supported", across all nine functions.
//
// This file supplies the second branch, and in doing so makes the ENOTSUP hand-own COHERENT rather
// than a lone gap: Go has exactly two configurations here, and after this the converted corpus is
// unambiguously the cgo-linked one — the nine wrappers on libc's broadcast AND the raw
// AllThreadsSyscall answering ENOTSUP, which is precisely what Go's own tests expect of a cgo build
// ("t.Skip(\"AllThreadsSyscall disabled with cgo\")"). Nothing here touches AllThreadsSyscall or
// runtime_doAllThreadsSyscall; that is a hard requirement of the design, not a side effect.
//
// WHY THE SHIMS EXIST, rather than pointing the nine at libc directly. Go's own cgo_libc_* do not
// point at libc either: they point at shims in runtime/cgo/linux_syscall.c that apply
//
//     #define SET_RETVAL(fn) \
//       uintptr_t ret = (uintptr_t) fn ; \
//       if (ret == (uintptr_t) -1) {     \
//         x->retval = (uintptr_t) errno; \
//       } else                           \
//         x->retval = ret
//
// i.e. the shim returns ERRNO on failure, not -1 — which is what lets syscall_linux.go write
// `if ret := cgocaller(...); ret != 0 { err = errnoErr(Errno(ret)) }`. Porting that convention into
// managed shims (rather than into cgocaller) buys two things. It keeps `cgocaller` a pure uintptr
// bridge with no errno semantics, which is what the darwin consumer (§3) reuses unchanged. And it
// makes the errno capture correct for free: a raw `delegate* unmanaged<…>` call cannot use
// SetLastError, so a cgocaller-side convention would have to read __errno_location() AFTER the call
// and hope nothing on the thread clobbered it in the window; inside a [LibraryImport(SetLastError =
// true)] shim the runtime captures errno at the call boundary and GetLastPInvokeError reads it back
// with no window at all.
//
// ⚠ WHY Setgroups IS HAND-OWNED HERE and the other eight are not. Setgroups is the only one of the
// nine that passes a POINTER (&a[0], into a Go slice of _Gid_t). The generated body hands that to
// the shim as `(uintptr)Ꮡ(a, 0)`, and golib's uintptr operator on a box pins the storage — but the
// pin lasts for the BOX's lifetime, and the call site passes the ADDRESS, not the box. MEASURED
// (2026-09-03, four-arm probe, movement control firing first so "stable" could mean something):
// with only the uintptr kept, a compacting GC MOVED the array and the old address read zeroes;
// with the box held, the address was stable and the value intact. The array is therefore
// collectable during the libc call, so the argument is copied into unmanaged memory that lives for
// the whole call and is freed in a finally — the same seam rule exec_unix.cs's Exec applies to
// argv/envp, one function over. cgocaller stays pointer-agnostic: it takes uintptrs and cannot tell
// a pointer from an integer, so it could not do this correctly even if it were the tidier place.
//
// Symbol resolution is DEFERRED: the module initializer stores managed shim addresses only, and
// each [LibraryImport] binds its libc symbol on first call. A platform missing one of these symbols
// therefore fails at the call that needs it rather than at module load. The musl caveat recorded in
// internal/runtime/syscall/linux/syscall_linux_impl.cs applies here unchanged.
//
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly. It lives in linux/ because every declaration it
// implements is linux-only.

[module: go.GoManualConversion]

namespace go;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using @unsafe = unsafe_package;
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class syscall_package {

// ---- the nine libc entry points -----------------------------------------------------------------
// Each returns 0 on success and -1 with errno set, which is what SET_RETVAL below depends on. A
// nonzero SUCCESS would be read as an errno by syscall_linux.go's callers; none of these has one.

[LibraryImport("libc", EntryPoint = "setegid", SetLastError = true)]
private static partial int libcSetegid(uint egid);

[LibraryImport("libc", EntryPoint = "seteuid", SetLastError = true)]
private static partial int libcSeteuid(uint euid);

[LibraryImport("libc", EntryPoint = "setgid", SetLastError = true)]
private static partial int libcSetgid(uint gid);

[LibraryImport("libc", EntryPoint = "setuid", SetLastError = true)]
private static partial int libcSetuid(uint uid);

[LibraryImport("libc", EntryPoint = "setregid", SetLastError = true)]
private static partial int libcSetregid(uint rgid, uint egid);

[LibraryImport("libc", EntryPoint = "setreuid", SetLastError = true)]
private static partial int libcSetreuid(uint ruid, uint euid);

[LibraryImport("libc", EntryPoint = "setresgid", SetLastError = true)]
private static partial int libcSetresgid(uint rgid, uint egid, uint sgid);

[LibraryImport("libc", EntryPoint = "setresuid", SetLastError = true)]
private static partial int libcSetresuid(uint ruid, uint euid, uint suid);

[LibraryImport("libc", EntryPoint = "setgroups", SetLastError = true)]
private static unsafe partial int libcSetgroups(nuint size, uint* list);

// ---- SET_RETVAL, ported ---------------------------------------------------------------------------
// runtime/cgo/linux_syscall.c's macro: errno on failure, the result otherwise. The comparison is
// against -1 exactly as the C is, and GetLastPInvokeError reads the errno the [LibraryImport] stub
// captured at the call boundary.
private static nuint SetRetval(int ret) =>
    ret == -1 ? (nuint)(uint)Marshal.GetLastPInvokeError() : (nuint)(uint)ret;

// ---- the nine shims -------------------------------------------------------------------------------
// [UnmanagedCallersOnly] so each has a real native entry point for cgo_libc_* to hold, which is the
// shape Go's own pointers have. gid_t/uid_t are uint32 on Linux; the wrappers hand us the value
// already widened to uintptr, so the narrowing here is the C prototype's, not a loss.

[UnmanagedCallersOnly] private static nuint ShimSetegid(nuint egid) => SetRetval(libcSetegid((uint)egid));
[UnmanagedCallersOnly] private static nuint ShimSeteuid(nuint euid) => SetRetval(libcSeteuid((uint)euid));
[UnmanagedCallersOnly] private static nuint ShimSetgid(nuint gid) => SetRetval(libcSetgid((uint)gid));
[UnmanagedCallersOnly] private static nuint ShimSetuid(nuint uid) => SetRetval(libcSetuid((uint)uid));
[UnmanagedCallersOnly] private static nuint ShimSetregid(nuint rgid, nuint egid) => SetRetval(libcSetregid((uint)rgid, (uint)egid));
[UnmanagedCallersOnly] private static nuint ShimSetreuid(nuint ruid, nuint euid) => SetRetval(libcSetreuid((uint)ruid, (uint)euid));
[UnmanagedCallersOnly] private static nuint ShimSetresgid(nuint rgid, nuint egid, nuint sgid) => SetRetval(libcSetresgid((uint)rgid, (uint)egid, (uint)sgid));
[UnmanagedCallersOnly] private static nuint ShimSetresuid(nuint ruid, nuint euid, nuint suid) => SetRetval(libcSetresuid((uint)ruid, (uint)euid, (uint)suid));

// setgroups' second argument is a pointer. Setgroups below guarantees it addresses unmanaged memory
// that outlives this call, so the shim reinterprets it directly.
[UnmanagedCallersOnly]
private static unsafe nuint ShimSetgroups(nuint size, nuint list) => SetRetval(libcSetgroups(size, (uint*)list));

// ---- the keystone ----------------------------------------------------------------------------------
// An indirect call through a native function pointer, and nothing else. .NET has no variadic
// indirect call — Marshal.GetDelegateForFunctionPointer and calli both need a FIXED signature — so
// the managed side is necessarily arity-dispatched. Arities 1, 2 and 3 are what §2's nine callers
// use; §3 (darwin) widens this family rather than replacing it. Anything else refuses loudly rather
// than guessing a signature, which would corrupt the stack.
// NOT declared `unsafe`: the generated declaration in syscall_linux.cs is not, and CS0764 requires
// both parts to agree. The pointer work goes in an unsafe block instead.
internal static partial uintptr cgocaller(@unsafe.Pointer _Δp0, params ꓸꓸꓸuintptr ʗp) {
    unsafe {
        void* fn = (void*)_Δp0;

        switch (ʗp.Length) {
            case 1:
                return (uintptr)((delegate* unmanaged<nuint, nuint>)fn)(ʗp[0]);
            case 2:
                return (uintptr)((delegate* unmanaged<nuint, nuint, nuint>)fn)(ʗp[0], ʗp[1]);
            case 3:
                return (uintptr)((delegate* unmanaged<nuint, nuint, nuint, nuint>)fn)(ʗp[0], ʗp[1], ʗp[2]);
            default:
                throw new System.NotSupportedException(
                    $"syscall: cgocaller has no arity-{ʗp.Length} form; §2 uses 1-3 and a guessed signature would corrupt the stack");
        }
    }
}

// ---- making the nine pointers non-nil ---------------------------------------------------------------
// A module initializer, so the fields are populated before any code in this assembly can read them —
// the same shape runtime uses for goenvs/goargs. It stores managed shim addresses only; no libc
// symbol is resolved here (see the file header).
[ModuleInitializer]
internal static unsafe void InitCgoLibcPointers() {
    cgo_libc_setegid = (void*)(delegate* unmanaged<nuint, nuint>)&ShimSetegid;
    cgo_libc_seteuid = (void*)(delegate* unmanaged<nuint, nuint>)&ShimSeteuid;
    cgo_libc_setgid = (void*)(delegate* unmanaged<nuint, nuint>)&ShimSetgid;
    cgo_libc_setuid = (void*)(delegate* unmanaged<nuint, nuint>)&ShimSetuid;
    cgo_libc_setregid = (void*)(delegate* unmanaged<nuint, nuint, nuint>)&ShimSetregid;
    cgo_libc_setreuid = (void*)(delegate* unmanaged<nuint, nuint, nuint>)&ShimSetreuid;
    cgo_libc_setresgid = (void*)(delegate* unmanaged<nuint, nuint, nuint, nuint>)&ShimSetresgid;
    cgo_libc_setresuid = (void*)(delegate* unmanaged<nuint, nuint, nuint, nuint>)&ShimSetresuid;
    cgo_libc_setgroups = (void*)(delegate* unmanaged<nuint, nuint, nuint>)&ShimSetgroups;
}

// ---- the one displaced wrapper ------------------------------------------------------------------------
// Setgroups, hand-owned because of its pointer argument alone (file header). Line for line this is
// Go's Setgroups with one difference: the _Gid_t array is built in UNMANAGED memory rather than a
// managed slice, so the address handed to libc cannot move under a GC during the call. The cgo
// branch is unconditional here because the module initializer above makes cgo_libc_setgroups
// non-nil for the life of the process — the nil test Go writes has exactly one answer in this
// corpus, and writing the dead branch would be writing a branch no run can take.
public static unsafe error /*err*/ Setgroups(slice<nint> gids) {
    nuint n = (nuint)len(gids);

    if (n == 0) {
        nuint zret = ((delegate* unmanaged<nuint, nuint, nuint>)(void*)cgo_libc_setgroups)(0, 0);
        return zret != 0 ? errnoErr(((Errno)(uintptr)zret)) : default!;
    }

    uint* list = (uint*)NativeMemory.Alloc(n, sizeof(uint));

    try {
        for (nint i = 0; i < (nint)n; i++) {
            list[i] = (uint)gids[i];
        }

        nuint ret = ((delegate* unmanaged<nuint, nuint, nuint>)(void*)cgo_libc_setgroups)(n, (nuint)list);
        return ret != 0 ? errnoErr(((Errno)(uintptr)ret)) : default!;
    } finally {
        NativeMemory.Free(list);
    }
}

} // end syscall_package
