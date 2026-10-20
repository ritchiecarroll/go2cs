// syscall_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The syscall package's darwin keystone family, realized on golib's platform-neutral dispatcher.
//
// GO 1.26. The runtime-provided family is now TWO bodyless declarations, `syscalln` and `rawsyscalln`
// (runtime/sys_darwin.go's syscall_syscalln and syscall_rawsyscalln, pushed here by //go:linkname). Go
// 1.26 gives syscall, syscallX, syscall6, syscall6X, syscall9, rawSyscall, rawSyscall6, rawSyscall9 and
// syscallPtr Go bodies in syscall_darwin.go, which call those two and then apply errno, errnoX or
// errnoPtr to the result. Those bodies are converted, so this file no longer carries them. The runtime
// entry reads errno after EVERY call (`err = *errnoAddr`, success or not) and tests nothing itself, which
// is golib's GoLibcErrnoRule.AlwaysRead. The cooked and raw forms differ only by entersyscall/exitsyscall,
// which the managed host already discharges, so both are the same call here.
//
// The exported Syscall, Syscall6, Syscall9, RawSyscall and RawSyscall6 below keep their Go 1.24 shape:
// one libc call applying Go's failure test to the result. Their notes follow, unchanged.
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
// syscall_darwin.cs's syscalln and rawsyscalln take `params Span<uintptr>`; the arity reaching golib is
// the Go call's, at most nine.
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

// A pointer to a Go struct holding managed references arrives here as an order token; GoLibcCall.Call
// marshals it into a Go-layout native copy (NativeStructMarshal.CallLibc: nil pointer fields cross as 0,
// a non-nil one keeps the token and EFAULT; the Msghdr/Iovec and Kevent_t.Udata follow-ups are named there).
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

// The Go 1.26 runtime entries every converted keystone in syscall_darwin.cs calls.
internal static partial (uintptr r1, uintptr r2, Errno err) syscalln(uintptr fn, params Span<uintptr> argsʗp) {
    return calln(fn, argsʗp);
}

internal static partial (uintptr r1, uintptr r2, Errno err) rawsyscalln(uintptr fn, params Span<uintptr> argsʗp) {
    return calln(fn, argsʗp);
}

private static (uintptr r1, uintptr r2, Errno err) calln(uintptr fn, Span<uintptr> args) {
    if (args.Length > GoLibcCall.MaxArgs) {
        throw new ArgumentException($"go2cs: syscalln with {args.Length} arguments; the darwin family tops out at {GoLibcCall.MaxArgs}", nameof(args));
    }

    Span<nuint> words = stackalloc nuint[args.Length];

    for (int i = 0; i < args.Length; i++) {
        words[i] = args[i];
    }

    return call(fn, words, GoLibcErrnoRule.AlwaysRead);
}

// The two failure rules that have no exported twin, opened for the //go:linkname PULLS
// internal/syscall/unix makes of this family (its net_darwin_impl.cs, darwin increment 10 (a)):
// the Go-prefixed PUBLIC helper per operation. Since Go 1.26 each forwards to the converted Go body
// in syscall_darwin.cs. Syscall/Syscall6/Syscall9 still serve the Int32MinusOne pulls.
public static (uintptr r1, uintptr r2, Errno err) GoSyscall6X(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return syscall6X(fn, a1, a2, a3, a4, a5, a6);
}

public static (uintptr r1, uintptr r2, Errno err) GoSyscallPtr(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
    return syscallPtr(fn, a1, a2, a3);
}

} // end syscall_package
