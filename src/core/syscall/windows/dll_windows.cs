// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// Parts of this file are derived from or inspired by Go, whose notice they carry above (license of record:
// LICENSES/BSD-3-Clause.txt):
//
// PROVENANCE, part by part:
// - DLLError.Error, DLLError.Unwrap, LoadDLL, MustLoadDLL, DLL.FindProc, DLL.MustFindProc, DLL.Release, Proc.Addr,
//   Proc.Call, LazyDLL.mustLoad, LazyDLL.Handle, LazyDLL.NewProc, NewLazyDLL, LazyProc.mustFind, LazyProc.Addr,
//   LazyProc.Call: DERIVED FROM src/syscall/dll_windows.go, Go 1.26.11, funcs LoadDLL, MustLoadDLL, NewLazyDLL;
//   methods DLLError.Error, DLLError.Unwrap, DLL.FindProc, DLL.MustFindProc, DLL.Release, Proc.Addr, Proc.Call,
//   LazyDLL.mustLoad, LazyDLL.Handle, LazyDLL.NewProc, LazyProc.mustFind, LazyProc.Addr, LazyProc.Call. The converted
//   output.
// - LazyDLL.Load, LazyProc.Find, loadlibrary, loadsystemlibrary, getprocaddress: INSPIRED BY
//   src/syscall/dll_windows.go, Go 1.26.11, funcs loadlibrary, loadsystemlibrary, getprocaddress; methods
//   LazyDLL.Load, LazyProc.Find. The lazy loads use a double-checked lock in place of atomic pointer loads, and the
//   library calls are P/Invokes.
// - syscalln: bodies for the bodyless Go declarations of the same names in src/syscall/dll_windows.go. Original go2cs
//   code.
// - Syscall, Syscall6, Syscall9, Syscall12, Syscall15, Syscall18, SyscallN: original go2cs code. Written as
//   forwarders to syscalln while Go declared these bodyless (through Go 1.25.14); Go 1.26.11 gives them bodies with
//   the same contract.
// - callNative, loadLibraryImpl and the token-call refusals: original go2cs code.
// - StdCallTrampoline: DERIVED FROM src/internal/runtime/syscall/windows/asm_windows_amd64.s, Go 1.26.11, TEXT
//   asmstdcall; rewritten for the Windows x64 calling convention (a fixed frame the unwinder can describe).
//   StdCallInfo: DERIVED FROM src/internal/runtime/syscall/windows/syscall_windows.go, Go 1.26.11, type StdCallInfo.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted dll_windows.go output).
// Two parts of this file cannot work as literally converted:
//
//  1. The Syscall/SyscallN trampolines and loadlibrary/getprocaddress are runtime-provided
//     assembly (../runtime/syscall_windows.go), emitted as bodyless partials that the
//     PartialStubGenerator turns into throwing stubs. Here they are real: LoadLibraryExW /
//     GetProcAddress via P/Invoke, and an unmanaged function-pointer calli dispatch for the
//     system-call trampolines (Windows x64 has a single calling convention, so a switch on
//     argument count covers every arity).
//  2. LazyDLL.Load / LazyProc.Find use Go's non-racy pattern of atomically reading a pointer
//     FIELD through a raw address — atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&d.dll)))
//     — a managed-referent uintptr round-trip that cannot alias a C# object-reference field
//     (see src/archived/Baseline-vs-FullConversion.md, the S1 fork). Here the lazy init is a plain
//     double-checked lock; CLR reference assignment is atomic, so readers never see a torn value.
//
// Soundness note (superseded 2026-08-30 — the prior version of this note, quoted in full in
// docs/phase4/DESIGN-syscall-pinning.md, described the hazard this one now describes as CLOSED).
//
// A pointer-derived argument to any entry point that funnels through syscalln below (every named
// Syscall/Syscall6/…/SyscallN, and Proc.Call/LazyProc.Call) is NOT resolved or pinned inside this
// file at all — the caller's own converted statement does the work, before syscalln is ever
// reached. convSyscallFunnelCall (src/go2cs/syscallKeepAliveAnalysis.go) captures each pointer-
// derived argument's ж<T> box into a statement-scoped temp — `var ᴋ0 = Ꮡbuf; … syscalln(…, (uintptr)
// ᴋ0, …); GC.KeepAlive(ᴋ0);` — so by the time execution reaches the calli dispatch below, every
// live pointer argument is already held by a named local in a frame that has not yet returned, and
// stays held until after this call returns. That is an ordinary CLR liveness guarantee, not a pin:
// nothing here needs to resolve an address back to an object, because the object was never let go.
// The census guard (src/syscall-keepalive-census.ps1) re-verifies corpus-wide, on demand, that
// every funnel call site the converter can see still carries this pattern.
//
// This replaces two earlier, weaker answers to the same hazard, in order:
//   1. The ORIGINAL converted form cast a pointer straight to uintptr with nothing left
//      referencing the box afterward — genuinely unsound: measured to corrupt heap memory under
//      sustained adversarial GC pressure (a background thread forcing continuous blocking,
//      compacting Gen2 collections) in well under 2,000,000 iterations.
//   2. A Windows port of Linux's resolve-based "keystone tether" (see
//      internal/runtime/syscall/linux/syscall_linux_impl.cs) was implemented, measured under the
//      same pressure, and REJECTED: write-disabled it was clean, but the resolve step — looking
//      an address back up in ManagedPointerTokens after the box may already have been collected —
//      missed 68% of the time (639,432 hits / 1,360,568 misses over 2,000,000 write-disabled
//      iterations), and write-enabled it crashed. A miss is GC.KeepAlive(null), a documented
//      no-op, so a miss is silent zero protection. Full data: docs/phase4/DESIGN-syscall-pinning.md.
// The call-site closure above has no resolve step to miss — verified clean at 2,000,000
// write-enabled iterations under the identical harness, with nothing to count.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Hand-owned native replacement of the converted dll_windows.go output — the converter skips
// regenerating a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

// [LibraryImport] requires /unsafe unconditionally (SYSLIB1062); this file's function-pointer
// trampoline needs it independently. Declared rather than inherited — see exec_windows.cs.
[module: go.GoRequiresUnsafe]

namespace go;

using sysdll = @internal.syscall.windows.sysdll_package;
using Δsync = sync_package;
using ꓸꓸꓸuintptr = go.variadic<uintptr>;
using static syscall_package;

partial class syscall_package {

// DLLError describes reasons for DLL load failures.
[GoType] public partial struct DLLError {
    public error Err;
    public @string ObjName;
    public @string Msg;
}

} partial class syscallᴛDLLError {

[GoRecv] public static @string Error(this ref DLLError e) {
    return e.Msg;
}

[GoRecv] public static error Unwrap(this ref DLLError e) {
    return e.Err;
}

} partial class syscall_package {

// ---- native Win32 entry points (the runtime-provided assembly in Go) ----

private const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;

// CharSet.Unicode becomes StringMarshalling.Utf16, which is the one case where the source-generated
// form is not merely equivalent: UTF-16 marshalling of a `string` is a PIN, not a transcode, so the
// generated stub hands LoadLibraryExW the string's own storage.
[LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
private static partial IntPtr win32LoadLibraryEx(string lpLibFileName, IntPtr hFile, uint dwFlags);

// GetProcAddress is the migration's one REJECTION that had to be answered with a different
// signature rather than a different attribute, and the rejection was right.
//
// The old declaration was `string` under CharSet.Ansi with BestFitMapping=false and
// ThrowOnUnmappableChar=true. [LibraryImport] supports neither of those two options at all, so the
// mechanical translation — StringMarshalling.Custom over AnsiStringMarshaller — would have kept the
// transcode while SILENTLY dropping the guard that made it safe: an unmappable rune stops throwing
// and becomes '?', i.e. a lookup of a DIFFERENT symbol.
//
// The right answer is that there was never a transcode to preserve. Go passes this entry point a
// `*byte` — `getprocaddress(handle uintptr, procname *byte)` — with no codepage step anywhere, and
// the caller below already HOLDS that NUL-terminated byte buffer. The old form decoded it through
// the ANSI codepage into UTF-16 and the marshaller re-encoded it back, a round trip that is lossy in
// both directions for any name outside the current codepage. Taking `byte*` deletes both halves and
// is byte-for-byte what Go does.
[LibraryImport("kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true)]
internal static unsafe partial IntPtr win32GetProcAddress(IntPtr hModule, byte* lpProcName);

// THE TOKEN DOOR (DESIGN-token-value-tag-refusal.md, outcome B).
//
// A reference-bearing pointee -- one whose converted struct holds a managed reference, so it has no
// address to give -- answers `(uintptr)` with an order TOKEN rather than an address. Handing that
// number to a native function writes the kernel's output somewhere that is not the caller's struct,
// or faults inside a system DLL: `rtlGetVersion`'s access violation in ntdll was this class, and it
// was found by a crash whose stack pointed away from the cause.
//
// This is the general door, so the NEXT member of the class is a caught panic naming itself instead.
// It reads the tag `AllocationBase` mints -- bit 63 set, bit 47 clear, non-canonical on x86-64 and
// therefore impossible for a real address, a HANDLE, a length or a flag word. `INVALID_HANDLE_VALUE`
// (-1) has bit 63 set and bit 47 set, so it passes; the guard pins that specifically.
//
// WHY HERE. Every arity -- Syscall, Syscall6, Syscall9, Syscall12, Syscall15, Syscall18, SyscallN
// and Proc.Call -- funnels through this one private helper, so one loop covers the whole surface;
// putting it on SyscallN alone would cover one of eight entries. `fn` is checked as well as the
// arguments: a token reaching the CALL TARGET is the same defect one step worse.
//
// SCOPE (amended 2026-10-16, the deep funnel). The call target is still refused here. An ARGUMENT token
// is no longer refused at this door: syscalln hands the call to NativeStructMarshal.CallDeep, which
// copies each token pointee to native memory at Go's layout -- pointer fields included, identity-mapped,
// a token naming an array element carrying the run to the end of its backing -- and decodes every copy
// back after the call. What has no native image (a string, slice, map, channel, func or interface
// field) is refused there, by name. Go 1.26 added auto-converted callers of this shape
// (WSADuplicateSocket under net.FileConn, SetEntriesInAcl under the os tests) that a hand-owned
// blittable mirror per wrapper would have to chase one at a time.
private static void refuseTokenCallTarget(nuint fn) {
    if (ManagedPointerTokens.IsTaggedToken(fn)) {
        throw panic("syscall: call target is a managed pointer token, not an address -- the " +
                    "pointee is reference-bearing and has no address to call");
    }
}

// The system-call trampoline: invoke the native function at trap with the given uintptr
// arguments. Mirrors runtime.syscall_syscalln: on amd64 r2 is XMM0 after the call, where a native
// function leaves a floating-point result (see StdCallTrampoline), and 0 on arm64, whose Go
// trampoline never writes it; err is GetLastError after the call (cleared first, so a succeeding
// API that does not touch last-error reports 0, matching Go).
private static unsafe (uintptr r1, uintptr r2, Errno err) syscalln(nuint fn, ReadOnlySpan<uintptr> a) {
    refuseTokenCallTarget(fn);

    // Go's limit and Go's text (runtime.syscall_syscalln): the words its trampoline's frame holds.
    if (a.Length > StdCallTrampoline.MaxArgs) {
        throw panic("runtime: SyscallN has too many arguments");
    }

    Span<nuint> words = stackalloc nuint[a.Length];
    bool anyToken = false;

    for (int i = 0; i < a.Length; i++) {
        words[i] = (nuint)a[i];
        anyToken |= ManagedPointerTokens.IsTaggedToken(words[i]);
    }

    // A reference-bearing pointee crosses as a deep native copy (NativeStructMarshal.CallDeep); every
    // other call passes its words as they are. Last-error is read inside the call, before any decode.
    if (anyToken) {
        refuseRetainingProc(fn);
    }

    (nuint r1, nuint r2, int lastError) = anyToken
        ? NativeStructMarshal.CallDeep(words, fn, &callNative)
        : callNative(fn, words);

    return (r1, r2, (Errno)(uint32)lastError);
}

// The words the C# call below passes: a go2cs limit, not Go's, reached only where the amd64 trampoline is not used
// (arm64, or the fallback in StdCallTrampoline's header), and refused by name there.
private const int MaxSyscallArgs = 18;

// RETAINED POINTERS. CallDeep frees every copy when the call returns, so a native function that KEEPS a
// pointer argument (or one reached through it) past the call would write freed memory. Such a function is
// refused by name before any copy is made. Keyed by "dll!proc"; empty today -- the 2026-10-16 census of
// the live funnel sites found no retaining callee. A name added here needs its own hand-owned wrapper.
private static readonly string[] RetainingProcs = [];

private static System.Collections.Generic.Dictionary<nuint, string>? s_retainingProcAddresses;

private static unsafe void refuseRetainingProc(nuint fn) {
    if (RetainingProcs.Length == 0) {
        return;
    }

    System.Collections.Generic.Dictionary<nuint, string>? addresses = System.Threading.Volatile.Read(ref s_retainingProcAddresses);

    if (addresses is null) {
        addresses = new System.Collections.Generic.Dictionary<nuint, string>();

        foreach (string name in RetainingProcs) {
            int bang = name.IndexOf('!');
            IntPtr module = win32LoadLibraryEx(name[..bang], IntPtr.Zero, LOAD_LIBRARY_SEARCH_SYSTEM32);

            if (module == IntPtr.Zero) {
                continue;
            }

            byte[] proc = System.Text.Encoding.ASCII.GetBytes(name[(bang + 1)..] + "\0");

            fixed (byte* procName = proc) {
                IntPtr address = win32GetProcAddress(module, procName);

                if (address != IntPtr.Zero) {
                    addresses[(nuint)(nint)address] = name;
                }
            }
        }

        System.Threading.Volatile.Write(ref s_retainingProcAddresses, addresses);
    }

    if (addresses.TryGetValue(fn, out string? retaining)) {
        throw panic($"syscall: {retaining} keeps a pointer argument past the call, so a Go value with no " +
                    "native address cannot be copied for it; it needs a hand-owned wrapper");
    }
}

private static unsafe (nuint r1, nuint r2, int lastError) callNative(nuint fn, ReadOnlySpan<nuint> a) {
    nuint r;
    int lastError;

    // A system call, entered as Go's syscall_SyscallN -> cgocall enters one: the runtime's entersyscall family moves
    // the g status to _Gsyscall and enters golib's transition seam, GoNativeCall (docs/phase4/DESIGN-native-call-seam.md
    // §12.2; COORD's Q2 ruling: one path per transition). Every SyscallN arity, and NativeStructMarshal.CallDeep,
    // come through here.
    delegate* unmanaged<StdCallInfo*, void> stdcall = StdCallTrampoline.Call;

    if (stdcall is null) {
        if (a.Length > MaxSyscallArgs) {
            throw panic($"syscall: {a.Length} arguments, more than the {MaxSyscallArgs} this port's C# call passes");
        }

        StdCallTrampoline.WarnUnavailable();
        Marshal.SetLastSystemError(0);
    }

    runtime_package.syscallEntersyscall();

    try {
        if (stdcall is not null) {
            // amd64: Go's own call shape, each of the first four words in its integer AND its XMM register,
            // and XMM0 back as r2. The trampoline clears and reads last-error itself, around the call.
            StdCallInfo call = default;
            call.Fn = fn;
            call.N = (nuint)a.Length;

            fixed (nuint* args = a) {
                call.Args = (nuint)args;
                stdcall(&call);
            }

            return (call.R1, call.R2, (int)(uint)call.Err);
        }

        switch (a.Length) {
            case 0: r = ((delegate* unmanaged<nuint>)fn)(); break;
            case 1: r = ((delegate* unmanaged<nuint, nuint>)fn)(a[0]); break;
            case 2: r = ((delegate* unmanaged<nuint, nuint, nuint>)fn)(a[0], a[1]); break;
            case 3: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2]); break;
            case 4: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3]); break;
            case 5: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4]); break;
            case 6: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5]); break;
            case 7: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6]); break;
            case 8: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7]); break;
            case 9: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8]); break;
            case 10: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9]); break;
            case 11: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10]); break;
            case 12: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11]); break;
            case 13: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12]); break;
            case 14: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13]); break;
            case 15: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14]); break;
            case 16: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15]); break;
            case 17: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15], a[16]); break;
            case 18: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15], a[16], a[17]); break;
            default: throw panic($"syscall: {a.Length} arguments, more than the {MaxSyscallArgs} this port's C# call passes");
        }

        // Read inside the call's window: nothing may run between the native call and its last-error.
        lastError = Marshal.GetLastSystemError();
    }
    finally {
        runtime_package.syscallExitsyscall();
    }

    return (r, 0, lastError);
}

// What the amd64 trampoline reads and writes, at fixed offsets: Fn 0, N 8, Args 16, R1 24, R2 32, Err 40 (Go's
// StdCallInfo, internal/runtime/syscall/windows/syscall_windows.go, in the same order).
[StructLayout(LayoutKind.Sequential)]
private struct StdCallInfo {
    public nuint Fn;
    public nuint N;
    public nuint Args;
    public nuint R1;
    public nuint R2;
    public nuint Err;
}

// THE AMD64 CALL SHAPE (docs/phase4/DESIGN-syscall-windows-amd64-trampoline.md). Go's windows/amd64 trampoline
// (asmstdcall) loads each of the first four argument words into BOTH its integer register (RCX, RDX, R8, R9) and its
// XMM register (XMM0-XMM3), because the Windows x64 convention passes a floating-point argument in the XMM register
// of its position, and it stores XMM0, where a floating-point result is left, as r2. A Go program calls a C function
// that takes or returns a double through Proc.Call or SyscallN that way (math.Float64bits words in,
// math.Float64frombits(r2) out); modernc.org/libc's strtod does. A C# function pointer typed over nuint passes
// integer registers only and reads RAX only, and no single C# call returns both RAX and XMM0, so the shape is Go's,
// in machine code. Image below is the whole page: asmstdcall for the Windows x64 convention, its UNWIND_INFO and its
// RUNTIME_FUNCTION, assembled from this GNU as source (syscallTrampolineBytes_test.go re-assembles it and pins the
// bytes):
//
// ASM-BEGIN
// .intel_syntax noprefix
// .text
// stub:
// push rsi
// push rdi
// push rbx
// sub rsp, 336
// mov rbx, rcx
// lea rax, [rsp+368]
// cmp rbx, rax
// jb bad_block
// lea rcx, [rbx+48]
// cmp rcx, qword ptr gs:[0x08]
// ja bad_block
// mov rcx, [rbx+8]
// cmp rcx, 42
// ja too_many
// mov rax, [rip + guard_check]
// test rax, rax
// jz checked
// mov rcx, [rbx]
// call rax
// mov rcx, [rbx+8]
// checked:
// mov rsi, [rbx+16]
// mov rdi, rsp
// cld
// rep movsq
// mov rcx, [rsp]
// movq xmm0, rcx
// mov rdx, [rsp+8]
// movq xmm1, rdx
// mov r8, [rsp+16]
// movq xmm2, r8
// mov r9, [rsp+24]
// movq xmm3, r9
// mov r10, gs:[0x30]
// mov dword ptr [r10+0x68], 0
// mov rax, [rbx]
// call rax
// mov [rbx+24], rax
// movq [rbx+32], xmm0
// mov r10, gs:[0x30]
// mov eax, dword ptr [r10+0x68]
// mov [rbx+40], rax
// add rsp, 336
// pop rbx
// pop rdi
// pop rsi
// ret
// too_many:
// int3
// bad_block:
// mov ecx, 5
// int 0x29
// stub_end:
// .balign 4, 0xcc
// unwind_info:
// .byte 0x01, 10, 5, 0
// .byte 10, 0x01
// .short 42
// .byte 3, 0x30
// .byte 2, 0x70
// .byte 1, 0x60
// .short 0
// runtime_function:
// .long stub - stub, stub_end - stub, unwind_info - stub
// .balign 8, 0xcc
// guard_check:
// .quad 0
// ASM-END
//
// The prolog saves rsi, rdi and rbx and reserves 336 bytes, Go's MaxArgs (42) words, leaving RSP 16-byte aligned at
// the call. The StdCallInfo must lie whole on the live part of the calling thread's stack: at or above rsp+368, the
// caller's side of the stub's frame (336 reserved, three pushes and the return address), and below the TEB's
// StackBase (gs:[0x08]). syscalln's is a local, so it always does. A block anywhere else ends the process with
// __fastfail(FAST_FAIL_INVALID_ARG), 0xc0000409, which no handler in the process can catch. The bound is the frame
// and not the TEB's StackLimit because the stub reads Fn twice, for the CFG check and for the call, and N twice,
// and between those reads its own copy of the arguments writes [rsp, rsp+8N): a block in dead stack below the
// caller could be rewritten after the check (P3's read of revision 5). A block within 48 bytes of 2^64 wraps the
// `lea` and passes the StackBase compare; the first read from it then faults, so nothing is called. A count above
// 42 reaches int3, as Go's asm crashes (syscalln refuses it before the stub, with Go's text).
// guard_check is the page's last 8 bytes: 0 in the image, and under Control Flow Guard the process image's own check
// routine, written once before the page is made executable and compared with the rest. When it is not 0 the stub
// calls it with Fn in RCX before anything else: the routine returns for a valid call target and ends the process
// with the CFG fast fail otherwise, so Fn is checked as the process's own indirect calls are. The stub reads the
// routine from its own read-only page, never from the caller's block or from writable memory, so no write a caller
// can make turns the check off. Its home space is the stub's own reserved stack, written afterwards; rbx, rsi and
// rdi survive it, and N is reloaded.
// The N words are copied to [rsp] and the first four loaded into both register files; gs:[0x30] is the TEB, whose
// LastErrorValue at 0x68 is cleared before the call and read after it, as Go's asm does; RAX goes to R1 and XMM0 to
// R2. Words 0-3 are loaded even when N < 4: they are the stub's own reserved stack, which the callee ignores.
//
// UNWIND_INFO (version 1, no handler, no frame register): a 10-byte prolog and five code slots padded to six. In
// reverse prolog order: at 10 UWOP_ALLOC_LARGE with a slot of 42 (336 / 8); at 3, 2 and 1 UWOP_PUSH_NONVOL of rbx
// (3), rdi (7) and rsi (6). The RUNTIME_FUNCTION record is at 220: begin 0, end 201, unwind info 204, relative to
// the page; guard_check is at 232.
//
// The page is written while it is not executable, made read-execute (never both at once), compared with the image,
// and registered with RtlAddFunctionTable, so an exception raised in a callback the native function makes
// (syscall.NewCallback) unwinds through this frame as it does through any compiled frame (WindowsNewCallback's
// callback-panic line); only then is the pointer published, once per process, through the type initializer. Under
// Control Flow Guard only offset 0 is a valid call target where the page can be narrowed (section 6 of the design).
// arm64 keeps the C# call: Go's arm64 trampoline passes integer registers only and never writes r2. A process that
// cannot map executable memory (Arbitrary Code Guard) keeps it too, and says so once on stderr.
private static unsafe class StdCallTrampoline {
    internal static readonly delegate* unmanaged<StdCallInfo*, void> Call = Build(out s_unavailable);

    // Why an amd64 process has no trampoline, or null; written once, by the type initializer.
    private static readonly string? s_unavailable;

    private static int s_warned;

    // Go's MaxArgs (internal/runtime/syscall/windows/syscall_windows.go): the words the stub's frame holds.
    internal const int MaxArgs = 42;

    // Says once, on stderr, that floating-point arguments and results are not passed, the first time a call in an
    // amd64 process takes the C# call because the trampoline could not be built.
    internal static void WarnUnavailable() {
        if (s_unavailable is not null && System.Threading.Interlocked.Exchange(ref s_warned, 1) == 0) {
            Console.Error.WriteLine($"go2cs: syscall: the amd64 call trampoline is unavailable ({s_unavailable}); " +
                                    "floating-point arguments and results of Proc.Call and SyscallN are not passed (r2 reads 0)");
        }
    }

    internal static ReadOnlySpan<byte> Image => [
        // stub: the prolog
        0x56, 0x57, 0x53, 0x48, 0x81, 0xEC, 0x50, 0x01, 0x00, 0x00,
        // the StdCallInfo, whole above the stub's own frame and below StackBase, then Go's argument-count check
        0x48, 0x89, 0xCB, 0x48, 0x8D, 0x84, 0x24, 0x70, 0x01, 0x00, 0x00,
        0x48, 0x39, 0xC3, 0x0F, 0x82, 0xA4, 0x00, 0x00, 0x00,
        0x48, 0x8D, 0x4B, 0x30,
        0x65, 0x48, 0x3B, 0x0C, 0x25, 0x08, 0x00, 0x00, 0x00, 0x0F, 0x87, 0x91, 0x00, 0x00, 0x00,
        0x48, 0x8B, 0x4B, 0x08, 0x48, 0x83, 0xF9, 0x2A, 0x0F, 0x87, 0x82, 0x00, 0x00, 0x00,
        // the CFG check of Fn, when guard_check is not 0
        0x48, 0x8B, 0x05, 0xA2, 0x00, 0x00, 0x00, 0x48, 0x85, 0xC0, 0x74, 0x09,
        0x48, 0x8B, 0x0B, 0xFF, 0xD0, 0x48, 0x8B, 0x4B, 0x08,
        // the N words to the stack
        0x48, 0x8B, 0x73, 0x10, 0x48, 0x89, 0xE7, 0xFC, 0xF3, 0x48, 0xA5,
        // the first four words, in both register files
        0x48, 0x8B, 0x0C, 0x24, 0x66, 0x48, 0x0F, 0x6E, 0xC1,
        0x48, 0x8B, 0x54, 0x24, 0x08, 0x66, 0x48, 0x0F, 0x6E, 0xCA,
        0x4C, 0x8B, 0x44, 0x24, 0x10, 0x66, 0x49, 0x0F, 0x6E, 0xD0,
        0x4C, 0x8B, 0x4C, 0x24, 0x18, 0x66, 0x49, 0x0F, 0x6E, 0xD9,
        // SetLastError(0)
        0x65, 0x4C, 0x8B, 0x14, 0x25, 0x30, 0x00, 0x00, 0x00, 0x41, 0xC7, 0x42, 0x68, 0x00, 0x00, 0x00, 0x00,
        // the call, then r1 and r2
        0x48, 0x8B, 0x03, 0xFF, 0xD0,
        0x48, 0x89, 0x43, 0x18, 0x66, 0x0F, 0xD6, 0x43, 0x20,
        // GetLastError()
        0x65, 0x4C, 0x8B, 0x14, 0x25, 0x30, 0x00, 0x00, 0x00, 0x41, 0x8B, 0x42, 0x68, 0x48, 0x89, 0x43, 0x28,
        // the epilog, then too_many
        0x48, 0x81, 0xC4, 0x50, 0x01, 0x00, 0x00, 0x5B, 0x5F, 0x5E, 0xC3, 0xCC,
        // bad_block: __fastfail(FAST_FAIL_INVALID_ARG)
        0xB9, 0x05, 0x00, 0x00, 0x00, 0xCD, 0x29,
        // .balign 4, padding with int3
        0xCC, 0xCC, 0xCC,
        // unwind_info, at 204
        0x01, 0x0A, 0x05, 0x00, 0x0A, 0x01, 0x2A, 0x00, 0x03, 0x30, 0x02, 0x70, 0x01, 0x60, 0x00, 0x00,
        // runtime_function, at 220: begin 0, end 201, unwind info 204
        0x00, 0x00, 0x00, 0x00, 0xC9, 0x00, 0x00, 0x00, 0xCC, 0x00, 0x00, 0x00,
        // guard_check, at 232: 0 here, the CFG check routine on the page
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    ];

    // Where the RUNTIME_FUNCTION record and the guard_check slot sit in Image (the ASM block's layout).
    private const int RuntimeFunctionOffset = 220;
    private const int GuardCheckOffset = 232;

    // The test switch that takes the fallback on any host, so its loud line and its integer calls are read where the
    // trampoline would build (DESIGN §6).
    private const string DisableVariable = "GO2CS_SYSCALL_TRAMPOLINE";

    // Never throws: a type initializer that threw would make every later syscall throw TypeInitializationException.
    // Any failure frees the page and leaves the pointer null, with the step for the loud line.
    private static delegate* unmanaged<StdCallInfo*, void> Build(out string? unavailable) {
        unavailable = null;

        if (RuntimeInformation.ProcessArchitecture != Architecture.X64) {
            return null;
        }

        nint page = 0;

        try {
            if (Environment.GetEnvironmentVariable(DisableVariable) == "off") {
                unavailable = $"disabled by {DisableVariable}=off";
                return null;
            }

            if (!controlFlowGuardEnabled(out bool cfg)) {
                unavailable = $"GetProcessMitigationPolicy failed, error {Marshal.GetLastPInvokeError()}";
                return null;
            }

            // Under CFG the stub checks Fn with the process's own check routine, which it reads from its own page's
            // guard_check slot, before the call, JIT or Native AOT, as a CFG-instrumented caller's indirect call does
            // (DESIGN §6). Narrowing the page to its entry is added
            // where it can be: a Native AOT process built with CFG checks its own indirect calls, and its call to
            // SetProcessValidCallTargets through the resolved pointer [LibraryImport] uses ends the process with a fast
            // fail (TestSyscallTrampolineCfgDotnetCaller), so there the page keeps the default protection, every
            // location valid. IsDynamicCodeSupported stands for "this caller's indirect calls are not checked": a wrong
            // "narrow" ends the process, a wrong "default" only leaves the page wider.
            nuint guardCheck = 0;

            if (cfg && (guardCheck = guardCheckRoutine()) == 0) {
                unavailable = "the process runs with Control Flow Guard and its image names no check routine";
                return null;
            }

            bool narrow = cfg && RuntimeFeature.IsDynamicCodeSupported;

            nuint size = (nuint)Environment.SystemPageSize;
            ReadOnlySpan<byte> image = Image;

            // Under CFG the page is allocated with every location an invalid call target (PAGE_TARGETS_INVALID needs
            // an executable protection, so nothing is written yet), made writable to take the image, then made
            // executable without updating its targets, and offset 0 alone is marked valid. Without CFG it is
            // allocated writable and made executable. Never writable and executable at once.
            page = narrow
                ? win32VirtualAlloc(0, size, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READ | PAGE_TARGETS_INVALID)
                : win32VirtualAlloc(0, size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            if (page == 0) {
                unavailable = $"VirtualAlloc failed, error {Marshal.GetLastPInvokeError()}";
                return null;
            }

            if (narrow && !win32VirtualProtect(page, size, PAGE_READWRITE, out _)) {
                string step = $"VirtualProtect to read-write failed, error {Marshal.GetLastPInvokeError()}";
                return release(ref page, step, out unavailable);
            }

            // The image, then the check routine into its guard_check slot, while the page is not executable.
            image.CopyTo(new Span<byte>((void*)page, image.Length));
            *(nuint*)(page + GuardCheckOffset) = guardCheck;

            if (!win32VirtualProtect(page, size, PAGE_EXECUTE_READ | (narrow ? PAGE_TARGETS_NO_UPDATE : 0), out _)) {
                string step = $"VirtualProtect to execute-read failed, error {Marshal.GetLastPInvokeError()}";
                return release(ref page, step, out unavailable);
            }

            if (!new ReadOnlySpan<byte>((void*)page, GuardCheckOffset).SequenceEqual(image[..GuardCheckOffset]) ||
                *(nuint*)(page + GuardCheckOffset) != guardCheck) {
                return release(ref page, "the mapped page differs from the image", out unavailable);
            }

            if (narrow) {
                CfgCallTargetInfo entry = new() { Offset = 0, Flags = CFG_CALL_TARGET_VALID };

                if (!win32SetProcessValidCallTargets(win32GetCurrentProcess(), page, size, 1, &entry)) {
                    string step = $"SetProcessValidCallTargets failed, error {Marshal.GetLastPInvokeError()}";
                    return release(ref page, step, out unavailable);
                }
            }

            win32FlushInstructionCache(win32GetCurrentProcess(), page, size);

            if (!win32RtlAddFunctionTable((uint*)(page + RuntimeFunctionOffset), 1, (ulong)page)) {
                return release(ref page, "RtlAddFunctionTable failed", out unavailable);
            }

            return (delegate* unmanaged<StdCallInfo*, void>)page;
        }
        catch (Exception e) {
            return release(ref page, $"{e.GetType().Name} ({e.Message})", out unavailable);
        }
    }

    // The process image's own CFG check routine (IMAGE_LOAD_CONFIG_DIRECTORY64.GuardCFCheckFunctionPointer, which the
    // loader fills), or 0 where the image carries none. Read from the loaded image's headers; no call is made.
    private static nuint guardCheckRoutine() {
        byte* image = (byte*)win32GetModuleHandle(0);

        if (image is null || *(ushort*)image != 0x5A4D) {
            return 0;
        }

        byte* nt = image + *(int*)(image + 0x3C);

        // "PE  ", and an optional header of the PE32+ form
        if (*(uint*)nt != 0x4550 || *(ushort*)(nt + 24) != 0x20B || *(uint*)(nt + 24 + 108) <= 10) {
            return 0;
        }

        // DataDirectory[IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG]
        byte* directory = nt + 24 + 112 + 10 * 8;
        uint rva = *(uint*)directory;

        if (rva == 0 || *(uint*)(directory + 4) < 0x78 || *(uint*)(image + rva) < 0x78) {
            return 0;
        }

        nuint* slot = *(nuint**)(image + rva + 0x70);
        return slot is null ? 0 : *slot;
    }

    private static delegate* unmanaged<StdCallInfo*, void> release(ref nint page, string step, out string unavailable) {
        unavailable = step;

        if (page != 0) {
            win32VirtualFree(page, 0, MEM_RELEASE);
            page = 0;
        }

        return null;
    }

    // Whether the process runs with Control Flow Guard (PROCESS_MITIGATION_CONTROL_FLOW_GUARD_POLICY, bit 0).
    private static bool controlFlowGuardEnabled(out bool enabled) {
        uint flags;

        if (!win32GetProcessMitigationPolicy(win32GetCurrentProcess(), ProcessControlFlowGuardPolicy, &flags, sizeof(uint))) {
            enabled = false;
            return false;
        }

        enabled = (flags & 1) != 0;
        return true;
    }

    // CFG_CALL_TARGET_INFO.
    [StructLayout(LayoutKind.Sequential)]
    private struct CfgCallTargetInfo {
        public nuint Offset;
        public nuint Flags;
    }

    private const uint MEM_COMMIT = 0x00001000;
    private const uint MEM_RESERVE = 0x00002000;
    private const uint MEM_RELEASE = 0x00008000;
    private const uint PAGE_READWRITE = 0x04;
    private const uint PAGE_EXECUTE_READ = 0x20;
    private const uint PAGE_TARGETS_INVALID = 0x40000000;   // VirtualAlloc's meaning of the bit
    private const uint PAGE_TARGETS_NO_UPDATE = 0x40000000; // VirtualProtect's meaning of the same bit
    private const nuint CFG_CALL_TARGET_VALID = 0x1;
    private const int ProcessControlFlowGuardPolicy = 7;
}

[LibraryImport("kernel32.dll", EntryPoint = "VirtualAlloc", SetLastError = true)]
private static partial nint win32VirtualAlloc(nint address, nuint size, uint allocationType, uint protect);

[LibraryImport("kernel32.dll", EntryPoint = "VirtualFree")]
[return: MarshalAs(UnmanagedType.Bool)]
private static partial bool win32VirtualFree(nint address, nuint size, uint freeType);

[LibraryImport("kernel32.dll", EntryPoint = "VirtualProtect", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
private static partial bool win32VirtualProtect(nint address, nuint size, uint newProtect, out uint oldProtect);

[LibraryImport("kernel32.dll", EntryPoint = "FlushInstructionCache")]
[return: MarshalAs(UnmanagedType.Bool)]
private static partial bool win32FlushInstructionCache(nint process, nint address, nuint size);

[LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW")]
private static partial nint win32GetModuleHandle(nint moduleName);

[LibraryImport("kernel32.dll", EntryPoint = "GetCurrentProcess")]
private static partial nint win32GetCurrentProcess();

[LibraryImport("kernel32.dll", EntryPoint = "RtlAddFunctionTable")]
[return: MarshalAs(UnmanagedType.Bool)]
private static unsafe partial bool win32RtlAddFunctionTable(uint* functionTable, uint entryCount, ulong baseAddress);

[LibraryImport("kernel32.dll", EntryPoint = "GetProcessMitigationPolicy", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
private static unsafe partial bool win32GetProcessMitigationPolicy(nint process, int policy, void* buffer, nuint length);

[LibraryImport("kernelbase.dll", EntryPoint = "SetProcessValidCallTargets", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
private static unsafe partial bool win32SetProcessValidCallTargets(nint process, nint virtualAddress, nuint regionSize, uint numberOfOffsets, void* offsetInformation);

// The runtime-provided DLL loader entry points that OTHER packages reach by symbol via
// `//go:linkname` — golang.org/x/sys/windows's LazyDLL/LazyProc route through `syscall.loadlibrary`
// / `syscall.loadsystemlibrary` / `syscall.getprocaddress` (a cross-package linkname the converter
// forwards to these). They take the raw UTF-16/ANSI pointers Go passes (the caller keeps the
// backing []uint16/[]byte alive across the synchronous call — the same transient-address window the
// trampoline above relies on). `public` so the forwarder can reach them across the assembly.
public static unsafe (uintptr handle, Errno err) loadlibrary(ж<uint16> filename) {
    return loadLibraryImpl(filename, 0);
}

public static unsafe (uintptr handle, Errno err) loadsystemlibrary(ж<uint16> filename) {
    return loadLibraryImpl(filename, LOAD_LIBRARY_SEARCH_SYSTEM32);
}

private static unsafe (uintptr handle, Errno err) loadLibraryImpl(ж<uint16> filename, uint flags) {
    string name = new string((char*)(void*)filename);
    IntPtr h;
    int lastError;

    // Go's syscall_loadlibrary reaches the loader through cgocall, so it is a system call: entersyscall around it,
    // and the last error read inside the window (DESIGN-native-call-seam §4.3).
    runtime_package.syscallEntersyscall();

    try {
        h = win32LoadLibraryEx(name, IntPtr.Zero, flags);
        lastError = Marshal.GetLastSystemError();
    }
    finally {
        runtime_package.syscallExitsyscall();
    }

    if (h == IntPtr.Zero) {
        return (0, (Errno)(uint32)lastError);
    }

    return ((uintptr)(nuint)h, 0);
}

public static unsafe (uintptr proc, Errno err) getprocaddress(uintptr handle, ж<uint8> procname) {
    // Go's own argument, unchanged: the caller's NUL-terminated byte buffer, passed through. A system call, as
    // loadLibraryImpl's is.
    IntPtr p;
    int lastError;

    runtime_package.syscallEntersyscall();

    try {
        p = win32GetProcAddress((IntPtr)(nint)(nuint)handle.Value, (byte*)(void*)procname);
        lastError = Marshal.GetLastSystemError();
    }
    finally {
        runtime_package.syscallExitsyscall();
    }

    if (p == IntPtr.Zero) {
        return (0, (Errno)(uint32)lastError);
    }

    return ((uintptr)(nuint)p, 0);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3) {
    return SyscallN(trap, a1, a2, a3);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall6(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall9(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6, a7, a8, a9);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall12(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall15(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12, uintptr a13, uintptr a14, uintptr a15) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall18(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12, uintptr a13, uintptr a14, uintptr a15, uintptr a16, uintptr a17, uintptr a18) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17, a18);
}

public static (uintptr r1, uintptr r2, Errno err) SyscallN(uintptr trap, params ꓸꓸꓸuintptr argsʗp) {
    return syscalln(trap, argsʗp);
}

// A DLL implements access to a single DLL.
[GoType] public partial struct DLL {
    public @string Name;
    public Handle Handle;
}

// LoadDLL loads the named DLL file into memory.
//
// If name is not an absolute path and is not a known system DLL used by
// Go, Windows will search for the named DLL in many locations, causing
// potential DLL preloading attacks.
//
// Use [LazyDLL] in golang.org/x/sys/windows for a secure way to
// load system DLLs.
public static (ж<DLL>, error) LoadDLL(@string name) {
    // A known system DLL is loaded from System32 only (LOAD_LIBRARY_SEARCH_SYSTEM32),
    // mirroring Go's loadsystemlibrary; anything else takes the default search order,
    // mirroring loadlibrary.
    uint flags = sysdll.IsSystemDLL[name] ? LOAD_LIBRARY_SEARCH_SYSTEM32 : 0u;
    IntPtr h = win32LoadLibraryEx(name.ToString(), IntPtr.Zero, flags);

    if (h == IntPtr.Zero) {
        ref var e = ref heap<Errno>(out var _);
        e = (Errno)(uint32)Marshal.GetLastSystemError();
        return (default!, new DLLErrorжerror(Ꮡ(new DLLError(
            Err: e,
            ObjName: name,
            Msg: "Failed to load "u8 + name + ": "u8 + e.Error()
        ))));
    }

    var d = Ꮡ(new DLL(
        Name: name,
        Handle: (Handle)(uintptr)(nuint)h
    ));
    return (d, default!);
}

// MustLoadDLL is like [LoadDLL] but panics if load operation fails.
public static ж<DLL> MustLoadDLL(@string name) {
    var (d, e) = LoadDLL(name);
    if (e != default!) {
        throw panic(e);
    }
    return d;
}

} partial class syscallᴛDLL {

// FindProc searches [DLL] d for procedure named name and returns [*Proc]
// if found. It returns an error if search fails.
public static unsafe (ж<Proc> proc, error err) FindProc(this ж<DLL> Ꮡd, @string name) {
    ref var d = ref Ꮡd.Value;

    // Go's FindProc calls BytePtrFromString(name) and hands getprocaddress the resulting *byte, so
    // the bytes Windows sees are the string's OWN bytes plus a terminator — not an ANSI transcode of
    // them. `new byte[n + 1]` is zero-filled, which supplies the NUL. (BytePtrFromString also
    // rejects an embedded NUL with EINVAL; that check was absent from this conversion before and is
    // left absent here rather than smuggled in with a mechanical change — an embedded NUL truncates
    // the name exactly as it did.)
    byte[] nameBytes = name;
    byte[] procName = new byte[nameBytes.Length + 1];
    nameBytes.CopyTo(procName, 0);

    IntPtr addr;

    fixed (byte* procNamePtr = procName) {
        addr = win32GetProcAddress((IntPtr)(nint)(nuint)(uintptr)d.Handle, procNamePtr);
    }

    if (addr == IntPtr.Zero) {
        ref var e = ref heap<Errno>(out var _);
        e = (Errno)(uint32)Marshal.GetLastSystemError();
        return (default!, new DLLErrorжerror(Ꮡ(new DLLError(
            Err: e,
            ObjName: name,
            Msg: "Failed to find "u8 + name + " procedure in "u8 + d.Name + ": "u8 + e.Error()
        ))));
    }

    var p = Ꮡ(new Proc(
        Dll: Ꮡd,
        Name: name,
        addr: (uintptr)(nuint)addr
    ));
    return (p, default!);
}

// MustFindProc is like [DLL.FindProc] but panics if search fails.
public static ж<Proc> MustFindProc(this ж<DLL> Ꮡd, @string name) {
    var (p, e) = Ꮡd.FindProc(name);
    if (e != default!) {
        throw panic(e);
    }
    return p;
}

// Release unloads [DLL] d from memory.
[GoRecv] public static error /*err*/ Release(this ref DLL d) {
    return FreeLibrary(d.Handle);
}

} partial class syscall_package {

// A Proc implements access to a procedure inside a [DLL].
[GoType] public partial struct Proc {
    public ж<DLL> Dll;
    public @string Name;
    internal uintptr addr;
}

} partial class syscallᴛProc {

// Addr returns the address of the procedure represented by p.
// The return value can be passed to Syscall to run the procedure.
[GoRecv] public static uintptr Addr(this ref Proc p) {
    return p.addr;
}

// Call executes procedure p with arguments a.
//
// The returned error is always non-nil, constructed from the result of GetLastError.
// Callers must inspect the primary return value to decide whether an error occurred
// (according to the semantics of the specific function being called) before consulting
// the error. The error always has type [Errno].
[GoRecv] public static (uintptr, uintptr, error) Call(this ref Proc p, params ꓸꓸꓸuintptr aʗp) {
    var (r1, r2, err) = SyscallN(p.Addr(), aʗp);
    return (r1, r2, err);
}

} partial class syscall_package {

// A LazyDLL implements access to a single [DLL].
// It will delay the load of the DLL until the first
// call to its [LazyDLL.Handle] method or to one of its
// [LazyProc]'s Addr method.
//
// LazyDLL is subject to the same DLL preloading attacks as documented
// on [LoadDLL].
//
// Use LazyDLL in golang.org/x/sys/windows for a secure way to
// load system DLLs.
[GoType] public partial struct LazyDLL {
    internal Δsync.Mutex mu;
    internal ж<DLL> dll; // non nil once DLL is loaded
    public @string Name;
}

} partial class syscallᴛLazyDLL {

// Load loads DLL file d.Name into memory. It returns an error if fails.
// Load will not try to load DLL, if it is already loaded into memory.
public static error Load(this ж<LazyDLL> Ꮡd) {
    ref var d = ref Ꮡd.Value;

    // CLR object-reference reads/writes are atomic, so the unlocked fast path never sees a
    // torn value (Go's version reads the field via atomic.LoadPointer for the same reason).
    if (d.dll != nil) {
        return default!;
    }

    Ꮡd.of(LazyDLL.Ꮡmu).Lock();
    try {
        if (d.dll == nil) {
            var (dll, e) = LoadDLL(d.Name);
            if (e != default!) {
                return e;
            }
            d.dll = dll;
        }
    }
    finally {
        Ꮡd.of(LazyDLL.Ꮡmu).Unlock();
    }

    return default!;
}

// mustLoad is like Load but panics if search fails.
internal static void mustLoad(this ж<LazyDLL> Ꮡd) {
    var e = Ꮡd.Load();
    if (e != default!) {
        throw panic(e);
    }
}

// Handle returns d's module handle.
public static uintptr Handle(this ж<LazyDLL> Ꮡd) {
    ref var d = ref Ꮡd.Value;

    Ꮡd.mustLoad();
    return (uintptr)(~d.dll).Handle;
}

// NewProc returns a [LazyProc] for accessing the named procedure in the [DLL] d.
public static ж<LazyProc> NewProc(this ж<LazyDLL> Ꮡd, @string name) {
    return Ꮡ(new LazyProc(l: Ꮡd, Name: name));
}

} partial class syscall_package {

// NewLazyDLL creates new [LazyDLL] associated with [DLL] file.
public static ж<LazyDLL> NewLazyDLL(@string name) {
    return Ꮡ(new LazyDLL(Name: name));
}

// A LazyProc implements access to a procedure inside a [LazyDLL].
// It delays the lookup until the [LazyProc.Addr], [LazyProc.Call], or [LazyProc.Find] method is called.
[GoType] public partial struct LazyProc {
    internal Δsync.Mutex mu;
    public @string Name;
    internal ж<LazyDLL> l;
    internal ж<Proc> proc;
}

} partial class syscallᴛLazyProc {

// Find searches [DLL] for procedure named p.Name. It returns
// an error if search fails. Find will not search procedure,
// if it is already found and loaded into memory.
public static error Find(this ж<LazyProc> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    // Same atomic-reference fast path as LazyDLL.Load above.
    if (p.proc != nil) {
        return default!;
    }

    Ꮡp.of(LazyProc.Ꮡmu).Lock();
    try {
        if (p.proc == nil) {
            var e = p.l.Load();
            if (e != default!) {
                return e;
            }
            var (proc, eΔ1) = (~p.l).dll.FindProc(p.Name);
            if (eΔ1 != default!) {
                return eΔ1;
            }
            p.proc = proc;
        }
    }
    finally {
        Ꮡp.of(LazyProc.Ꮡmu).Unlock();
    }

    return default!;
}

// mustFind is like Find but panics if search fails.
internal static void mustFind(this ж<LazyProc> Ꮡp) {
    var e = Ꮡp.Find();
    if (e != default!) {
        throw panic(e);
    }
}

// Addr returns the address of the procedure represented by p.
// The return value can be passed to Syscall to run the procedure.
public static uintptr Addr(this ж<LazyProc> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    Ꮡp.mustFind();
    return p.proc.Addr();
}

// Call executes procedure p with arguments a. See the documentation of
// Proc.Call for more information.
public static (uintptr r1, uintptr r2, error lastErr) Call(this ж<LazyProc> Ꮡp, params ꓸꓸꓸuintptr aʗp) {
    ref var p = ref Ꮡp.Value;

    Ꮡp.mustFind();
    return p.proc.Call(aʗp);
}

} partial class syscall_package {

} // end syscall_package
