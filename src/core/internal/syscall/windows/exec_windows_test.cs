// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows

// Hand-owned: TestRunAtLowIntegrity's own local getIntegrityLevelToken hands the kernel
// `uintptr(unsafe.Pointer(tml))` for a TOKEN_MANDATORY_LABEL -- the non-blittable-struct-by-
// address class syscall/windows/zsyscall_windows_impl.cs already answers, this time via a
// POINTER field rather than an array field. SID_AND_ATTRIBUTES.Sid converts to `ж<syscall.SID>`,
// a MANAGED REFERENCE; Windows expects a raw native SID address at that struct offset. Handing
// SetTokenInformation the managed struct's address puts an object reference where a pointer
// value belongs -- Windows rejects it building the low-integrity token:
// ERROR_INVALID_SID ("The security ID structure is invalid"), measured via TestRunAtLowIntegrity.
// `syscall.StringToSid`'s result is an opaque native-backed handle (same reasoning
// zsyscall_windows_ptrout_impl.cs gives for ConvertStringSidToSid -- SID is never read through
// in managed code), so its raw uintptr IS the real native SID address; the fix is a blittable
// NativeTokenMandatoryLabel mirror holding that address as a plain `nuint`, populated and handed
// over in place of the managed struct. Rest of this file is otherwise the ordinary conversion.
[module: go.GoManualConversion]

// The blittable mirror below needs an unsafe pointer field -- declared rather than inherited,
// per the convention zsyscall_windows_impl.cs establishes.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using fmt = fmt_package;
using Δwindows = go.@internal.syscall.windows_package;
using os = os_package;
using exec = go.os.exec_package;
using syscall = syscall_package;
using testing = testing_package;
using testenv = go.@internal.testenv_package;
using @unsafe = unsafe_package;
using go.@internal.syscall;
using go.os;
using io = io_package;

partial class windows_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goWantHelperProcessˢ = "GO_WANT_HELPER_PROCESS"u8;
internal static readonly @string testRunˢ = "-test.run=^TestRunAtLowIntegrity$"u8;

public static void TestRunAtLowIntegrity(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (os.Getenv(goWantHelperProcessˢ) == "1"u8) {
            var (wil, errΔ1) = getProcessIntegrityLevel();
            if (errΔ1 != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "error: %s\n"u8, errΔ1.Error());
                os.Exit(9);
                return;
            }
            fmt.Printf("%s"u8, wil);
            os.Exit(0);
            return;
        }
        var cmd = exec.Command(testenv.Executable(new testing_TжTB(Ꮡt)), testRunˢ, "--");
        cmd.Value.Env = new @string[]{"GO_WANT_HELPER_PROCESS=1"u8}.slice();
        ref var token = ref heap<syscall.Token>(out var Ꮡtoken);
        (token, var err) = getIntegrityLevelToken(sidWilLow);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var tokenʗ1 = token;
        defer(() => tokenʗ1.Close(), ref ᒐ);
        cmd.Value.SysProcAttr = Ꮡ(new syscall.SysProcAttr(
            Token: token
        ));
        (var @out, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)@out) != sidWilLow) {
            Ꮡt.Fatalf("Child process did not run as low integrity level: %s"u8, ((@string)@out));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static readonly @string sidWilLow = @"S-1-16-4096"u8;

internal static (@string, error) getProcessIntegrityLevel() {
    GoFrame ᒐ = default;
    try {
        var (procToken, err) = syscall.OpenCurrentProcessToken();
        if (err != default!) {
            return ("", err);
        }
        defer(() => procToken.Close(), ref ᒐ);
        (var p, err) = tokenGetInfo(procToken, syscall.TokenIntegrityLevel, 64);
        if (err != default!) {
            return ("", err);
        }
        var sid = tokenIntegrityLabelSid(p);
        return sid.String();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (@unsafe.Pointer, error) tokenGetInfo(syscall.Token t, uint32 @class, nint initSize) {
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)initSize;
    while (ᐧ) {
        var b = new slice<byte>((nint)(n));
        var e = syscall.GetTokenInformation(t, @class, Ꮡ(b, 0), (uint32)len(b), Ꮡn);
        if (e == default!) {
            return (new @unsafe.Pointer(Ꮡ(b, 0)), default!);
        }
        if (!AreEqual(e, syscall.ERROR_INSUFFICIENT_BUFFER)) {
            return (default!, e);
        }
        if (n <= (uint32)len(b)) {
            return (default!, e);
        }
    }
}

internal static (syscall.Token, error) getIntegrityLevelToken(@string wns) {
    GoFrame ᒐ = default;
    try {
        ref var procToken = ref heap(new syscall.Token(), out var ᏑprocToken);
        ref var token = ref heap(new syscall.Token(), out var Ꮡtoken);
        var (proc, err) = syscall.GetCurrentProcess();
        if (err != default!) {
            return (0, err);
        }
        defer(syscall.CloseHandle, proc, ref ᒐ);
        err = syscall.OpenProcessToken(proc,
            (uint32)((UntypedInt)((UntypedInt)(syscall.TOKEN_DUPLICATE | syscall.TOKEN_ADJUST_DEFAULT) | syscall.TOKEN_QUERY) | (uint32)syscall.TOKEN_ASSIGN_PRIMARY),
            ᏑprocToken);
        if (err != default!) {
            return (0, err);
        }
        var procTokenʗ1 = procToken;
        defer(() => procTokenʗ1.Close(), ref ᒐ);
        (var sid, err) = syscall.StringToSid(wns);
        if (err != default!) {
            return (0, err);
        }
        err = Δwindows.DuplicateTokenEx(procToken, 0, nil, Δwindows.SecurityImpersonation,
            Δwindows.TokenPrimary, Ꮡtoken);
        if (err != default!) {
            return (0, err);
        }
        err = setTokenIntegrityLabel(token, sid);
        if (err != default!) {
            token.Close();
            return (0, err);
        }
        return (token, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// TOKEN_MANDATORY_LABEL exactly as Windows lays it out: Sid is a raw native pointer, not the
// managed reference the auto-conversion holds. `sid` is StringToSid's opaque native-backed
// handle -- SID is never read through in managed code (same reasoning
// zsyscall_windows_ptrout_impl.cs gives for ConvertStringSidToSid) -- so its raw uintptr IS the
// real native SID address, taken directly rather than through the reference-shaped field.
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
private struct NativeTokenMandatoryLabel
{
    public nuint Sid;
    public uint32 Attributes;
}

// setTokenIntegrityLabel is the native transcription of `tml.Label.Sid = sid; ...;
// SetTokenInformation(token, TokenIntegrityLevel, uintptr(unsafe.Pointer(tml)), tml.Size())` --
// see the file header for why it cannot be a literal conversion. Length matches Go's own
// Size(): sizeof(TOKEN_MANDATORY_LABEL) + GetLengthSid(sid).
private static unsafe error /*err*/ setTokenIntegrityLabel(syscall.Token token, ж<syscall.SID> sid) {
    NativeTokenMandatoryLabel native = new() {
        Sid = (nuint)(uintptr)sid,
        Attributes = Δwindows.SE_GROUP_INTEGRITY
    };

    uint32 size = (uint32)sizeof(NativeTokenMandatoryLabel) + syscall.GetLengthSid(sid);

    return Δwindows.SetTokenInformation(token, syscall.TokenIntegrityLevel, (uintptr)(&native), size);
}

// tokenIntegrityLabelSid is the native transcription of
// `(*syscall.SID)(unsafe.Pointer((*windows.TOKEN_MANDATORY_LABEL)(p).Label.Sid))` -- the read
// side of the same class. `p` addresses a GetTokenInformation-filled managed byte buffer holding
// a native TOKEN_MANDATORY_LABEL; reinterpreting it as the managed `Δwindows.TOKEN_MANDATORY_LABEL`
// would try to read a raw native pointer as a `ж<SID>` managed reference. Reading the raw
// pointer VALUE through the mirror and wrapping it as a native-backed box is the fix -- SID is
// never read through in managed code, so the native box is the correct and sufficient answer.
private static unsafe ж<syscall.SID> tokenIntegrityLabelSid(@unsafe.Pointer p) {
    var native = (NativeTokenMandatoryLabel*)(uintptr)p;
    return (ж<syscall.SID>)(void*)native->Sid;
}

} // end windows_test_package
