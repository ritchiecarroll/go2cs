// syscall_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The Linux kernel boundary — the one declaration the entire converted Linux syscall surface
// funnels through.
//
// Go implements Syscall6 in assembly (internal/runtime/syscall/asm_linux_amd64.s): it loads the
// call number into RAX and a1..a6 into RDI, RSI, RDX, R10, R8, R9, executes SYSCALL, and reports
// RAX and RDX. There is no Go body and no linkname anywhere — this is raw metal, so the converted
// declaration is bodyless and PartialStubGenerator was throwing NotImplementedException from it.
// That single throw is what stopped every Linux program during `syscall.init()`, which calls
// Getrlimit(RLIMIT_NOFILE) before `os` is usable, hence before `fmt.Println` can run at all
// (docs/phase4/FINDING-linux-run-layer.md §4).
//
// The managed realization binds glibc's syscall(2) rather than reproducing the instruction. That
// is a deliberate choice between two shapes the finding put to the user, who ruled option A: ONE
// P/Invoke lights up the whole generated wrapper surface at once (open, read, write, close, stat,
// getrlimit, … in zsyscall_linux_amd64.cs, plus this package's own epoll/eventfd helpers), where a
// per-call mapping onto .NET APIs would be N hand-owns, each of which must independently guess at
// semantics the kernel already defines exactly.
//
// Three details make the binding faithful rather than approximate; each was MEASURED on
// linux/amd64 (glibc 2.35, .NET 9) before being relied on, not argued from the ABI documents:
//
//  1. THE VARIADIC. C declares `long syscall(long number, ...)`. Declaring it here with seven
//     fixed native ints is correct on the SysV AMD64 ABI because integer-class variadic arguments
//     ride exactly the same registers as fixed ones (RDI, RSI, RDX, RCX, R8, R9) with the seventh
//     spilling to the stack — which is precisely where glibc's hand-written syscall.S reads a6
//     from. Verified end-to-end with a real six-argument call: mmap(NULL, 4096, PROT_READ|WRITE,
//     MAP_PRIVATE|ANONYMOUS, -1, 0) returned a live mapping that munmap then released. (AL, which
//     a true variadic call would set to the vector-register count, is unused by syscall.S.)
//
//  2. r2. Go's contract returns (r1, r2, errno), and r2 is RDX — which libc's wrapper cannot hand
//     back. It does not have to: the Linux x86-64 syscall convention clobbers only RCX and R11, so
//     RDX still holds whatever entered the kernel, and the asm's `MOVQ DX, BX` therefore observes
//     a3 unchanged. Returning a3 is not a stand-in for r2 on this architecture, it IS r2. Probed
//     directly: syscall.Syscall6(SYS_GETPID, …, a3=0xDEADBEEF, …) returns r2=0xdeadbeef under the
//     real Go runtime. On the failure path the asm zeroes r2, which the branch below mirrors.
//
//  3. ERRNO. libc collapses the kernel's entire [-4095, -1] error band to a -1 return and reports
//     the positive errno out of band, which is the same number Go's asm produces by negating the
//     raw return — so the two agree on the value, and SetLastError lets the CLR capture it before
//     any managed code can perturb it. Probed: openat(AT_FDCWD, NULL) returns -1 with
//     Marshal.GetLastPInvokeError() == 14 (EFAULT).
//
// ONE known divergence, inherent to routing through libc rather than the instruction: a syscall
// that legitimately RETURNS -1 as a success value is indistinguishable from a failure here, and
// would report a stale errno. Go's asm does not have this ambiguity because it tests the raw
// return against the whole error band. No Linux syscall reached by the converted corpus returns -1
// on success, so this is a documented edge rather than an active defect; the only true fix is an
// instruction-level bottom, which needs assembly the managed model cannot express.
//
// PORTABILITY, for the arm64 increment. Both the register mapping and r2's meaning are
// architecture-specific: asm_linux_arm64.s puts a1..a6 in X0..X5 with the number in X8 and reads
// r2 from X1 — which holds a2 on entry, not a3 — so an arm64 flavor of this file must echo a2 and
// must re-run probe (2) there rather than inherit this one's answer. The variadic question is also
// live on that target: standard AAPCS64 passes variadic integer arguments in the same registers as
// named ones, but Apple's arm64 ABI deliberately does not, so the "declare it fixed" shortcut is a
// per-platform judgment, not a general one. Library resolution is a third: LibraryImport("libc")
// binds on glibc, and a musl target (Alpine) would likely need a NativeLibrary.SetDllImportResolver
// fallback.
//
// THE BINDING IS SOURCE-GENERATED ([LibraryImport]), not runtime-marshalled ([DllImport]). For this
// all-`nint` signature the two produce an equivalent native call, so the change buys nothing HERE —
// it is taken for what it does as the surface grows. The whole residual risk of routing Go's kernel
// boundary through managed structs is per-struct LAYOUT (see the pointer-half note above), and
// [DllImport] answers a non-blittable signature by silently marshalling a COPY: the kernel then
// writes into a temporary the caller never reads, which is a wrong ANSWER with no diagnostic. The
// source generator refuses to emit that call at all, so the same mistake becomes a compile error
// with a line number. That guard is the entire reason this file's one-line form matters.

using System.Runtime.InteropServices;

// Hand-owned (no syscall_linux_impl.go exists, so a reconvert never regenerates this file);
// marked for consistency with the other hand-owned operational files in the corpus.
[module: go.GoManualConversion]

// [LibraryImport] requires /unsafe unconditionally (SYSLIB1062) — the generated stub is written in
// terms of pointers even when, as here, every parameter is already a native int. The converter's own
// <AllowUnsafeBlocks> predicate sees only ITS emission, which for this package contains nothing
// unsafe, so the requirement has to be declared by the file that has it.
[module: go.GoRequiresUnsafe]

namespace go.@internal.runtime;

partial class syscall_package {

// glibc's syscall(2). Declared with the number plus six arguments as fixed native ints — see
// note (1) in the file header for why that is correct here despite the C declaration being
// variadic. EntryPoint keeps the managed name from colliding with the `syscall` namespace
// segment this package's own name contributes.
[LibraryImport("libc", EntryPoint = "syscall", SetLastError = true)]
private static partial nint libc_syscall(nint number, nint a1, nint a2, nint a3, nint a4, nint a5, nint a6);

// Bit-preserving bridges between Go's uintptr (a golib struct wrapping nuint) and the signed
// native int the C signature takes. Both directions are pure reinterpretation: the kernel's
// arguments and returns are word-sized bit patterns whose signedness is per-syscall, not
// per-ABI.
private static nint ToNative(uintptr value) => unchecked((nint)(nuint)value);

public static partial (uintptr r1, uintptr r2, uintptr errno) Syscall6(uintptr num, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    // THE KEYSTONE TETHER RETIRED (2026-08-30, docs/phase4/DESIGN-syscall-pinning.md §7/§7a) — a
    // resolve-based re-rooting used to stand here (rooted 2026-08-26 from os/exec's GC-mark
    // SIGSEGV), closing the ж→uintptr lifetime gap by looking each bare uintptr argument back up
    // in ManagedPointerTokens and KeepAlive-ing whatever it resolved to. Measured under the same
    // adversarial pressure that motivated it, that resolve step missed 68% of the time (its box
    // was already collected before Resolve looked for it) — silent zero protection on two calls
    // out of three, not a narrow residual window.
    //
    // The real fix is upstream, at every CALL SITE that can reach this funnel: converted code
    // never hands this function a raw uintptr derived from a Go pointer without ALSO holding that
    // pointer's box in a statement-scoped temp through the call and releasing it via
    // GC.KeepAlive afterward (convSyscallFunnelCall, src/go2cs/syscallKeepAliveAnalysis.go) — an
    // ordinary CLR liveness guarantee, not a resolve, so there is nothing here to look up and
    // nothing to miss. Confirmed corpus-wide: a full GOOS=linux reconvert and build is clean
    // (0 errors, 307 projects), and this file's own direct callers
    // (runtime/os_linux.go, runtime/netpoll_epoll.go) pass no pointer-derived argument at all —
    // Go's own unsafe.Pointer rule (4) does not even apply to them. There is nothing left for a
    // tether at this boundary to protect.
    nint result = libc_syscall(ToNative(num), ToNative(a1), ToNative(a2), ToNative(a3), ToNative(a4), ToNative(a5), ToNative(a6));

    // r1 is the raw return in both outcomes: on failure libc returns -1 and so does Go's asm
    // (`MOVQ $-1, AX`), so the same expression serves both branches.
    uintptr r1 = new uintptr(unchecked((nuint)result));

    return result == -1
        ? (r1, default(uintptr), new uintptr((nuint)Marshal.GetLastPInvokeError()))
        : (r1, a3, default(uintptr));
}

} // end syscall_package
