// syscall_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The scheduler brackets Go wraps every syscall in — and why the managed model's faithful
// realization of them is to do nothing.
//
// syscall_linux.go declares them as pulls from the runtime:
//
//     //go:linkname runtime_entersyscall runtime.entersyscall
//     func runtime_entersyscall()
//     //go:linkname runtime_exitsyscall runtime.exitsyscall
//     func runtime_exitsyscall()
//
// and every non-Raw wrapper in the package brackets its kernel call with the pair. With no body
// they took throwing PartialStubGenerator stubs, so the FIRST syscall any Linux program makes
// through the ordinary (non-Raw) path died — `fmt.Println` → os.File.Write → internal/poll →
// syscall.Write → syscall.Syscall → runtime_entersyscall, i.e. hello-world's actual output call.
//
// WHY NOT FORWARD. The pull has a real Go body, so forwarding is the shape this converter reaches
// for first (linknameForwardTargets). It is not available here, and not marginally: runtime's
// entersyscall opens with `getcallerfp()` / `getcallerpc()` / `getcallersp()` — raw-metal frame
// intrinsics with no managed realization — and hands them to reentersyscall, which drives the P
// state machine across `sched`, `mp.oldp` and `casgstatus`. None of that state is populated in the
// managed model. A forwarder would fault on the first intrinsic.
//
// WHY NOTHING IS THE RIGHT ANSWER, rather than a stub-shaped omission. The pair's entire job is to
// release the P around a blocking call so the scheduler can run other goroutines on another M
// while this one sits in the kernel, and to reacquire one afterwards. It is bookkeeping in the
// strict sense: both are `func()`, they compute nothing any caller consumes, and no converted code
// anywhere reads state they would have set. The obligation they discharge is real, but in a
// converted program the HOST already discharges it — a goroutine is a .NET thread, a blocking
// syscall blocks that thread exactly as the CLR expects, and the thread pool's own injection is
// what keeps other work running meanwhile. So this is not the "plausible answer" failure mode the
// project rules against (there is no answer to fabricate — the return type is void); it is the
// managed model already satisfying the contract by a different mechanism, which is the same
// judgment syscall_impl.cs records for runtimeSetenv/runtimeUnsetenv one folder up.
//
// SCOPE. This file covers the pair the ordinary syscall path needs, and — since 2026-08-22 — the
// one further raw bottom that something genuinely needed: rawSyscallNoError (below). The remaining
// bodyless declarations in syscall_linux.cs — rawVforkSyscall, runtime_doAllThreadsSyscall,
// cgocaller — are still separate questions with separate answers (a fork-time raw bottom, a
// whole-process operation, the cgo boundary) and stay announcing stubs until something genuinely
// needs them. Catalogued in docs/phase4/FINDING-linux-run-layer.md §5.
//
// rawSyscallNoError — WHY NOW. Go declares it in syscall_linux.go and implements it in
// asm_linux_amd64.s as a bare SYSCALL that returns AX and DX with NO errno handling, for the
// generated "NoError" family that cannot fail: Getpid, Getppid, Gettid, Getuid, Geteuid, Getgid,
// Getegid, Umask (zsyscall_linux_amd64.go), plus forkExec's own getpid/getppid. The 2026-08-22
// Linux roster re-run measured what the announcing stub costs once the poll seam is open:
// os.Getuid → NotImplementedException at the top of os/user.Current (poisoning its sync.Once, so
// every later call NREs — archive/tar's whole uid→name path), time.interrupt's Kill(Getpid()) in
// TestSleep, and os/exec's test host dying in init before a single verdict. The body is the
// keystone binding once more: RawSyscall6 minus the errno, which is exactly what the asm returns.
//
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly and matches the other hand-owned files in this package.
// It lives in linux/ rather than flat because the declarations it implements are linux-only
// (syscall_linux.go): a flat implementing part would have no defining declaration on Windows.

[module: go.GoManualConversion]

namespace go;

partial class syscall_package {

// Releases the P around a blocking kernel call. See the file header: the CLR's thread model
// already provides what this exists to provide, and the function returns nothing.
internal static partial void runtime_entersyscall() {
}

// Reacquires one afterwards. Same reasoning, same shape.
internal static partial void runtime_exitsyscall() {
}

// rawSyscallNoError: asm_linux_amd64.s's `SYSCALL; MOVQ AX, r1; MOVQ DX, r2` — the keystone
// binding with the errno word dropped. The callers are the generated "NoError" family (getpid,
// getuid, … — syscalls that cannot fail), so the dropped errno is not a swallowed error; it is the
// contract. No enter/exitsyscall bracket, exactly as Go's raw path has none.
internal static partial (uintptr r1, uintptr r2) rawSyscallNoError(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    var (r1, r2, _) = @internal.runtime.syscall_package.Syscall6(trap, a1, a2, a3, 0, 0, 0);
    return (r1, r2);
}

// AllThreadsSyscall's kernel half. Go's contract: the runtime stops the world and replays the
// syscall on EVERY thread (setuid/setgid class calls must change credentials process-wide), and a
// process that CANNOT make that guarantee answers ENOTSUP — which is exactly what Go's own
// cgo-enabled binaries do (syscall_linux.go: `if cgo_libc_setegid != nil { return minus1, minus1,
// ENOTSUP }`), because foreign threads exist that the runtime cannot broadcast over. A managed
// host is that shape taken further — the CLR owns threads Go's runtime never sees — so ENOTSUP is
// the faithful Go answer, not a stub: callers get the same errno a cgo build gives them, and Go's
// own TestAllThreadsSyscallSignals skips on it identically on both sides. (The previous state was
// a throwing PartialStubGenerator stub, which turned that skip into an infrastructure-error.)
internal static partial (uintptr r1, uintptr r2, uintptr err) runtime_doAllThreadsSyscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return (~(uintptr)0, ~(uintptr)0, (uintptr)ENOTSUP);
}

} // end syscall_package
