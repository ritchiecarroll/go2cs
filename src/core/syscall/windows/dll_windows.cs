// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

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
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class syscall_package {

// DLLError describes reasons for DLL load failures.
[GoType] partial struct DLLError {
    public error Err;
    public @string ObjName;
    public @string Msg;
}

[GoRecv] public static @string Error(this ref DLLError e) {
    return e.Msg;
}

[GoRecv] public static error Unwrap(this ref DLLError e) {
    return e.Err;
}

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
private static unsafe partial IntPtr win32GetProcAddress(IntPtr hModule, byte* lpProcName);

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
// arguments. Mirrors runtime.syscall_syscalln: r2 is only meaningful for floating-point
// returns (always 0 here) and err is GetLastError after the call (cleared first, so a
// succeeding API that does not touch last-error reports 0, matching Go).
private static unsafe (uintptr r1, uintptr r2, Errno err) syscalln(nuint fn, ReadOnlySpan<uintptr> a) {
    refuseTokenCallTarget(fn);

    if (a.Length > MaxSyscallArgs) {
        throw new ArgumentOutOfRangeException(nameof(a), "too many syscall arguments");
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

    (nuint r, int lastError) = anyToken
        ? NativeStructMarshal.CallDeep(words, fn, &callNative)
        : callNative(fn, words);

    return (r, 0, (Errno)(uint32)lastError);
}

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

private static unsafe (nuint r, int lastError) callNative(nuint fn, ReadOnlySpan<nuint> a) {
    nuint r;

    Marshal.SetLastSystemError(0);

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
        default: throw new ArgumentOutOfRangeException(nameof(a), "too many syscall arguments");
    }

    return (r, Marshal.GetLastSystemError());
}

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
    IntPtr h = win32LoadLibraryEx(new string((char*)(void*)filename), IntPtr.Zero, flags);

    if (h == IntPtr.Zero) {
        return (0, (Errno)(uint32)Marshal.GetLastSystemError());
    }

    return ((uintptr)(nuint)h, 0);
}

public static unsafe (uintptr proc, Errno err) getprocaddress(uintptr handle, ж<uint8> procname) {
    // Go's own argument, unchanged: the caller's NUL-terminated byte buffer, passed through.
    IntPtr p = win32GetProcAddress((IntPtr)(nint)(nuint)handle.Value, (byte*)(void*)procname);

    if (p == IntPtr.Zero) {
        return (0, (Errno)(uint32)Marshal.GetLastSystemError());
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
[GoType] partial struct DLL {
    public @string Name;
    public ΔHandle Handle;
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
        Handle: (ΔHandle)(uintptr)(nuint)h
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

// A Proc implements access to a procedure inside a [DLL].
[GoType] partial struct Proc {
    public ж<DLL> Dll;
    public @string Name;
    internal uintptr addr;
}

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
[GoType] partial struct LazyDLL {
    internal Δsync.Mutex mu;
    internal ж<DLL> dll; // non nil once DLL is loaded
    public @string Name;
}

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

// NewLazyDLL creates new [LazyDLL] associated with [DLL] file.
public static ж<LazyDLL> NewLazyDLL(@string name) {
    return Ꮡ(new LazyDLL(Name: name));
}

// A LazyProc implements access to a procedure inside a [LazyDLL].
// It delays the lookup until the [LazyProc.Addr], [LazyProc.Call], or [LazyProc.Find] method is called.
[GoType] partial struct LazyProc {
    internal Δsync.Mutex mu;
    public @string Name;
    internal ж<LazyDLL> l;
    internal ж<Proc> proc;
}

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

} // end syscall_package
