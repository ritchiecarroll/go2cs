// syscall_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The syscall package's darwin keystone family — twelve bodyless declarations whose Go bodies are
// runtime's assembly (`runtime·syscall`, `syscallX`, `syscallPtr`, `syscall6`, `syscall6X`,
// `syscall9`, pushed here by //go:linkname), realized on golib's platform-neutral dispatcher.
//
// On darwin `fn`/`trap` is not a syscall number but the address of a libc function — every generated
// wrapper in zsyscall_darwin_amd64.cs passes FuncPCABI0(libc_<sym>_trampoline), which the class-B
// resolver turns into the exported symbol's real address — so each body is one indirect C call with
// the arguments as they arrive, and Go's own failure test applied to the result, read off
// sys_darwin_amd64.s (docs/phase4/DESIGN-darwin-run-layer.md §2, DESIGN-darwin-run-layer-2.md §7.1):
//
//     syscall / syscall6 / syscall9  — CMPL AX, $-1  (the low 32 bits)      -> Int32MinusOne
//     syscallX / syscall6X           — CMPQ AX, $-1  (all 64)               -> Int64MinusOne
//     syscallPtr                     — TESTQ AX, AX  (NULL is the error)    -> NullPointer
//
// and on failure errno through __error(). `raw` and cooked variants are the same call here: the
// entersyscall/exitsyscall brackets the cooked ones add are the P-release bookkeeping the managed
// host already discharges (syscall_linux_impl.cs's reasoning, one platform over).
//
// r2 is reported as 0: the second return register is read by no darwin wrapper in the corpus
// (censused over zsyscall_darwin_amd64.cs, 0 readers), and a managed indirect call cannot observe
// DX. Stated here so it is a known gap, not a surprise.
//
// Hand-owned companion: these are `partial` bodies pairing with the bodyless declarations in
// syscall_darwin.cs and syscall_darwin_amd64.cs, so no converter change and no corpus footprint.
// Since darwin increment 10 (a) it also carries the two public doors (GoSyscall6X, GoSyscallPtr)
// through which internal/syscall/unix's linkname pulls of the family reach the rules that have no
// exported twin -- see the bottom of the file.
[module: go.GoManualConversion]

namespace go;

partial class syscall_package {

private const string libSystemPath = "/usr/lib/libSystem.B.dylib";

private static nint s_errnoReader;

private static nint errnoReader() {
    if (s_errnoReader == 0) {
        s_errnoReader = GoCgoDynamicImports.Resolve("__error", libSystemPath);
    }

    return s_errnoReader;
}

private static (uintptr r1, uintptr r2, Errno err) call(uintptr fn, ReadOnlySpan<nuint> args, GoLibcErrnoRule rule) {
    nuint r1 = GoLibcCall.Call(unchecked((nint)(nuint)fn), args, rule, errnoReader(), out nuint errno);
    return ((uintptr)r1, (uintptr)0, (Errno)(uintptr)errno);
}

// The exported family: `trap` is a libc address on darwin exactly as `fn` is below.
public static partial (uintptr r1, uintptr r2, Errno err) Syscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    return call(trap, stackalloc nuint[] { a1, a2, a3 }, GoLibcErrnoRule.Int32MinusOne);
}

public static partial (uintptr r1, uintptr r2, Errno err) Syscall6(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return call(trap, stackalloc nuint[] { a1, a2, a3, a4, a5, a6 }, GoLibcErrnoRule.Int32MinusOne);
}

public static partial (uintptr r1, uintptr r2, Errno err) Syscall9(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9) {
    return call(trap, stackalloc nuint[] { a1, a2, a3, a4, a5, a6, a7, a8, a9 }, GoLibcErrnoRule.Int32MinusOne);
}

public static partial (uintptr r1, uintptr r2, Errno err) RawSyscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    return call(trap, stackalloc nuint[] { a1, a2, a3 }, GoLibcErrnoRule.Int32MinusOne);
}

public static partial (uintptr r1, uintptr r2, Errno err) RawSyscall6(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return call(trap, stackalloc nuint[] { a1, a2, a3, a4, a5, a6 }, GoLibcErrnoRule.Int32MinusOne);
}

// The runtime-provided family the generated wrappers call.
internal static partial (uintptr r1, uintptr r2, Errno err) syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    return call(fn, stackalloc nuint[] { a1, a2, a3 }, GoLibcErrnoRule.Int32MinusOne);
}

internal static partial (uintptr r1, uintptr r2, Errno err) syscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return call(fn, stackalloc nuint[] { a1, a2, a3, a4, a5, a6 }, GoLibcErrnoRule.Int32MinusOne);
}

internal static partial (uintptr r1, uintptr r2, Errno err) syscall6X(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return call(fn, stackalloc nuint[] { a1, a2, a3, a4, a5, a6 }, GoLibcErrnoRule.Int64MinusOne);
}

internal static partial (uintptr r1, uintptr r2, Errno err) syscallX(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    return call(fn, stackalloc nuint[] { a1, a2, a3 }, GoLibcErrnoRule.Int64MinusOne);
}

internal static partial (uintptr r1, uintptr r2, Errno err) rawSyscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    return call(fn, stackalloc nuint[] { a1, a2, a3 }, GoLibcErrnoRule.Int32MinusOne);
}

internal static partial (uintptr r1, uintptr r2, Errno err) rawSyscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return call(fn, stackalloc nuint[] { a1, a2, a3, a4, a5, a6 }, GoLibcErrnoRule.Int32MinusOne);
}

internal static partial (uintptr r1, uintptr r2, Errno err) syscallPtr(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    return call(fn, stackalloc nuint[] { a1, a2, a3 }, GoLibcErrnoRule.NullPointer);
}

// The two failure rules that have no exported twin, opened for the //go:linkname PULLS
// internal/syscall/unix makes of this family (its net_darwin_impl.cs, darwin increment 10 (a)):
// the Go-prefixed PUBLIC helper per operation, the dispatcher staying private to this file.
// Syscall/Syscall6/Syscall9 already serve the Int32MinusOne pulls, being the same call.
public static (uintptr r1, uintptr r2, Errno err) GoSyscall6X(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return syscall6X(fn, a1, a2, a3, a4, a5, a6);
}

public static (uintptr r1, uintptr r2, Errno err) GoSyscallPtr(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    return syscallPtr(fn, a1, a2, a3);
}

} // end syscall_package
