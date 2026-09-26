// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Fork, exec, wait, etc.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted exec_windows.go output).
// Everything in this file except StartProcess is the converted output verbatim — the argument
// escaping, command-line building, environment-block building and path normalization are pure Go
// logic that converts faithfully. StartProcess alone cannot work as literally converted:
//
//   Go hands CreateProcessW a *_STARTUPINFOEXW — a struct whose fields are POINTERS into native
//   memory (lpDesktop, lpTitle, lpReserved2, the three std HANDLEs, and the attribute list). The
//   converted C# struct holds those fields as golib `ж<T>` boxes, which are managed CLASS
//   references: they are neither the right bytes at the right offsets for the Win32 layout, nor
//   marshalable at all — `unsafe.Sizeof(*si)` (golib's Marshal.SizeOf) throws outright with
//   "cannot be marshaled as an unmanaged structure; no meaningful size or offset can be computed."
//   The same applies to _PROC_THREAD_ATTRIBUTE_LIST, whose converted form wraps an `array<byte>`
//   class reference. This is the memory-layout / raw-metal case documented in
//   src/archived/Baseline-vs-FullConversion.md: a faithful conversion is impossible, so the declaration is
//   hand-owned.
//
// The implementation below is a straight transcription of exec_windows.go's StartProcess against
// blittable [StructLayout(LayoutKind.Sequential)] mirrors of STARTUPINFOEXW / PROCESS_INFORMATION /
// SECURITY_ATTRIBUTES and direct P/Invokes, preserving Go's ordering, validation and error results.
// It reuses the converted helpers (makeCmdLine, createEnvBlock, joinExeDirAndFName) and the
// converted scalar-only syscall wrappers (GetCurrentProcess, DuplicateHandle, CloseHandle), so only
// the struct-passing seam is native.
//
// Soundness improvement over the converted form: every buffer handed to CreateProcessW (application
// name, command line, environment block, working directory, handle list, attribute list) is copied
// into UNMANAGED memory for the duration of the call and freed in a finally. golib's ж→uintptr
// conversion can only produce a TRANSIENT pinned address (see the note in dll_windows.cs), which a
// compacting GC could invalidate mid-call; unmanaged copies remove that window entirely here.

using System;
using System.Runtime.InteropServices;

// Hand-owned native replacement of the converted exec_windows.go output — the converter skips
// regenerating a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

// [LibraryImport] requires /unsafe unconditionally (SYSLIB1062). This package's converted emission
// happens to need it too, so the flag is already true — but inheriting a requirement by luck is
// how it disappears: declare it in the file that has it and the union keeps it true whatever the
// emission does next.
[module: go.GoRequiresUnsafe]

namespace go;

using bytealg = @internal.bytealg_package;
using Δsync = sync_package;
using utf16 = unicode.utf16_package;
using @unsafe = unsafe_package;
using @internal;
using unicode;

partial class syscall_package {

// ForkLock is not used on Windows.
public static Δsync.RWMutex ForkLock;

// EscapeArg rewrites command line argument s as prescribed
// in https://msdn.microsoft.com/en-us/library/ms880421.
// This function returns "" (2 double quotes) if s is empty.
// Alternatively, these transformations are done:
//   - every back slash (\) is doubled, but only if immediately
//     followed by double quote (");
//   - every double quote (") is escaped by back slash (\);
//   - finally, s is wrapped with double quotes (arg -> "arg"),
//     but only if there is space or tab inside s.
public static @string EscapeArg(@string s) {
    if (len(s) == 0) {
        return @""""""u8;
    }
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (rune)'"' or (rune)'\\' or (rune)' ' or (rune)'\t': {
            var b = new slice<byte>(0, len(s) + 2);
            b = appendEscapeArg(b, s);
            return ((@string)b);
        }}

    }
    return s;
}

// appendEscapeArg escapes the string s, as per escapeArg,
// appends the result to b, and returns the updated slice.
internal static slice<byte> appendEscapeArg(slice<byte> b, @string s) {
    if (len(s) == 0) {
        return append(b, ((@string)@""""""u8).ꓸꓸꓸ);
    }
    var needsBackslash = false;
    var hasSpace = false;
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (rune)'"' or (rune)'\\': {
            needsBackslash = true;
            break;
        }
        case (rune)' ' or (rune)'\t': {
            hasSpace = true;
            break;
        }}

    }
    if (!needsBackslash && !hasSpace) {
        // No special handling required; normal case.
        return append(b, s.ꓸꓸꓸ);
    }
    if (!needsBackslash) {
        // hasSpace is true, so we need to quote the string.
        b = append(b, (byte)((rune)'"'));
        b = append(b, s.ꓸꓸꓸ);
        return append(b, (byte)((rune)'"'));
    }
    if (hasSpace) {
        b = append(b, (byte)((rune)'"'));
    }
    nint slashes = 0;
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        switch (c) {
        default: {
            slashes = 0;
            break;
        }
        case (rune)'\\': {
            slashes++;
            break;
        }
        case (rune)'"': {
            for (; slashes > 0; slashes--) {
                b = append(b, (byte)((rune)'\\'));
            }
            b = append(b, (byte)((rune)'\\'));
            break;
        }}

        b = append(b, c);
    }
    if (hasSpace) {
        for (; slashes > 0; slashes--) {
            b = append(b, (byte)((rune)'\\'));
        }
        b = append(b, (byte)((rune)'"'));
    }
    return b;
}

// makeCmdLine builds a command line out of args by escaping "special"
// characters and joining the arguments with spaces.
internal static @string makeCmdLine(slice<@string> args) {
    slice<byte> b = default!;
    foreach (var (_, v) in args) {
        if (len(b) > 0) {
            b = append(b, (byte)((rune)' '));
        }
        b = appendEscapeArg(b, v);
    }
    return ((@string)b);
}

// createEnvBlock converts an array of environment strings into
// the representation required by CreateProcess: a sequence of NUL
// terminated strings followed by a nil.
// Last bytes are two UCS-2 NULs, or four NUL bytes.
// If any string contains a NUL, it returns (nil, EINVAL).
internal static (slice<uint16>, error) createEnvBlock(slice<@string> envv) {
    if (len(envv) == 0) {
        return (utf16.Encode(slice<rune>((@string)"\x00\x00")), default!);
    }
    nint length = default!;
    foreach (var (_, s) in envv) {
        if (bytealg.IndexByteString(s, 0) != -1) {
            return (default!, EINVAL);
        }
        length += len(s) + 1;
    }
    length += 1;
    var b = new slice<uint16>(0, length);
    foreach (var (_, s) in envv) {
        foreach (var (_, c) in s) {
            b = utf16.AppendRune(b, c);
        }
        b = utf16.AppendRune(b, 0);
    }
    b = utf16.AppendRune(b, 0);
    return (b, default!);
}

public static void CloseOnExec(ΔHandle fd) {
    SetHandleInformation(fd, HANDLE_FLAG_INHERIT, 0);
}

public static error /*err*/ SetNonblock(ΔHandle fd, bool nonblocking) {
    error err = default!;

    return default!;
}

// FullPath retrieves the full path of the specified file.
public static (@string path, error err) FullPath(@string name) {
    @string path = default!;
    error err = default!;

    (var p, err) = UTF16PtrFromString(name);
    if (err != default!) {
        return ("", err);
    }
    var n = (uint32)100;
    while (ᐧ) {
        var buf = new slice<uint16>((nint)(n));
        (n, err) = GetFullPathName(p, (uint32)len(buf), Ꮡ(buf, 0), nil);
        if (err != default!) {
            return ("", err);
        }
        if (n <= (uint32)len(buf)) {
            return (UTF16ToString(buf[..(int)(n)]), default!);
        }
    }
}

internal static bool isSlash(uint8 c) {
    return c == (rune)'\\' || c == (rune)'/';
}

internal static (@string name, error err) normalizeDir(@string dir) {
    @string name = default!;
    error err = default!;

    (var ndir, err) = FullPath(dir);
    if (err != default!) {
        return ("", err);
    }
    if (len(ndir) > 2 && isSlash(ndir[0]) && isSlash(ndir[1])) {
        // dir cannot have \\server\share\path form
        return ("", EINVAL);
    }
    return (ndir, default!);
}

internal static nint volToUpper(nint ch) {
    if ((rune)'a' <= ch && ch <= (rune)'z') {
        ch += (rune)'A' - (rune)'a';
    }
    return ch;
}

internal static (@string name, error err) joinExeDirAndFName(@string dir, @string p) {
    @string name = default!;
    error err = default!;

    if (len(p) == 0) {
        return ("", EINVAL);
    }
    if (len(p) > 2 && isSlash(p[0]) && isSlash(p[1])) {
        // \\server\share\path form
        return (p, default!);
    }
    if (len(p) > 1 && p[1] == (rune)':'){
        // has drive letter
        if (len(p) == 2) {
            return ("", EINVAL);
        }
        if (isSlash(p[2])){
            return (p, default!);
        } else {
            var (d, errΔ1) = normalizeDir(dir);
            if (errΔ1 != default!) {
                return ("", errΔ1);
            }
            if (volToUpper((nint)p[0]) == volToUpper((nint)d[0])){
                return FullPath(d + "\\" + p[2..]);
            } else {
                return FullPath(p);
            }
        }
    } else {
        // no drive letter
        var (d, errΔ2) = normalizeDir(dir);
        if (errΔ2 != default!) {
            return ("", errΔ2);
        }
        if (isSlash(p[0])){
            return FullPath(d[..2] + p);
        } else {
            return FullPath(d + "\\"u8 + p);
        }
    }
}

[GoType] partial struct ProcAttr {
    public @string Dir;
    public slice<@string> Env;
    public slice<uintptr> Files;
    public ж<SysProcAttr> Sys;
}

[GoType] partial struct SysProcAttr {
    public bool HideWindow;
    public @string CmdLine; // used if non-empty, else the windows command line is built by escaping the arguments passed to StartProcess
    public uint32 CreationFlags;
    public Token Token;               // if set, runs new process in the security context represented by the token
    public ж<SecurityAttributes> ProcessAttributes; // if set, applies these security attributes as the descriptor for the new process
    public ж<SecurityAttributes> ThreadAttributes; // if set, applies these security attributes as the descriptor for the main thread of the new process
    public bool NoInheritHandles;                // if set, no handles are inherited by the new process, not even the standard handles, contained in ProcAttr.Files, nor the ones contained in AdditionalInheritedHandles
    public slice<ΔHandle> AdditionalInheritedHandles;     // a list of additional handles, already marked as inheritable, that will be inherited by the new process
    public ΔHandle ParentProcess;            // if non-zero, the new process regards the process given by this handle as its parent process, and AdditionalInheritedHandles, if set, should exist in this parent process
}

internal static ж<ProcAttr> ᏑzeroProcAttr = new StandardBox<ProcAttr>(default(ProcAttr));
internal static ref ProcAttr zeroProcAttr => ref ᏑzeroProcAttr.Value;

internal static ж<SysProcAttr> ᏑzeroSysProcAttr = new StandardBox<SysProcAttr>(default(SysProcAttr));
internal static ref SysProcAttr zeroSysProcAttr => ref ᏑzeroSysProcAttr.Value;

// ---- native Win32 layout mirrors and entry points (see the file header) ----

// STARTUPINFOW / STARTUPINFOEXW exactly as Windows lays them out. The converted [GoType] structs
// (types_windows.cs StartupInfo / _STARTUPINFOEXW) hold their pointer fields as managed ж<T>
// boxes, so they can neither be sized nor passed; these are the blittable equivalents.
[StructLayout(LayoutKind.Sequential)]
private struct NativeStartupInfoW
{
    public uint32 Cb;
    public IntPtr Reserved;
    public IntPtr Desktop;
    public IntPtr Title;
    public uint32 X;
    public uint32 Y;
    public uint32 XSize;
    public uint32 YSize;
    public uint32 XCountChars;
    public uint32 YCountChars;
    public uint32 FillAttribute;
    public uint32 Flags;
    public uint16 ShowWindow;
    public uint16 Reserved2Length;
    public IntPtr Reserved2;
    public IntPtr StdInput;
    public IntPtr StdOutput;
    public IntPtr StdErr;
}

[StructLayout(LayoutKind.Sequential)]
private struct NativeStartupInfoExW
{
    public NativeStartupInfoW StartupInfo;
    public IntPtr ProcThreadAttributeList;
}

[StructLayout(LayoutKind.Sequential)]
private struct NativeProcessInformation
{
    public IntPtr Process;
    public IntPtr Thread;
    public uint32 ProcessId;
    public uint32 ThreadId;
}

[StructLayout(LayoutKind.Sequential)]
private struct NativeSecurityAttributes
{
    public uint32 Length;
    public IntPtr SecurityDescriptor;
    public int32 InheritHandle;
}

// The source generator accepted all five of these UNCHANGED, and that is the result worth
// recording rather than the syntax. Every parameter is already a native word because this file
// deliberately hands CreateProcessW unmanaged copies of every buffer (the soundness note in the
// header) — so the "nothing managed crosses this boundary" discipline the file follows by
// convention is, from here on, checked by the compiler.
[LibraryImport("kernel32.dll", EntryPoint = "CreateProcessW", SetLastError = true)]
private static partial int win32CreateProcess(IntPtr applicationName, IntPtr commandLine, IntPtr processAttributes, IntPtr threadAttributes, int inheritHandles, uint32 creationFlags, IntPtr environment, IntPtr currentDirectory, IntPtr startupInfo, IntPtr processInformation);

[LibraryImport("advapi32.dll", EntryPoint = "CreateProcessAsUserW", SetLastError = true)]
private static partial int win32CreateProcessAsUser(IntPtr token, IntPtr applicationName, IntPtr commandLine, IntPtr processAttributes, IntPtr threadAttributes, int inheritHandles, uint32 creationFlags, IntPtr environment, IntPtr currentDirectory, IntPtr startupInfo, IntPtr processInformation);

[LibraryImport("kernel32.dll", EntryPoint = "InitializeProcThreadAttributeList", SetLastError = true)]
private static partial int win32InitializeProcThreadAttributeList(IntPtr attributeList, int32 attributeCount, int32 flags, ref IntPtr size);

[LibraryImport("kernel32.dll", EntryPoint = "UpdateProcThreadAttribute", SetLastError = true)]
private static partial int win32UpdateProcThreadAttribute(IntPtr attributeList, uint32 flags, IntPtr attribute, IntPtr value, IntPtr size, IntPtr previousValue, IntPtr returnSize);

[LibraryImport("kernel32.dll", EntryPoint = "DeleteProcThreadAttributeList")]
private static partial void win32DeleteProcThreadAttributeList(IntPtr attributeList);

// Copies a Go string to a NUL-terminated unmanaged UTF-16 buffer. Mirrors UTF16PtrFromString:
// a string containing a NUL is rejected with EINVAL (Windows would silently truncate).
private static (IntPtr ptr, error err) allocUTF16(@string s) {
    string value = s.ToString();

    if (value.IndexOf('\0') >= 0) {
        return (IntPtr.Zero, EINVAL);
    }

    return (Marshal.StringToHGlobalUni(value), default!);
}

// go2cs HOST SEAM (not in Go): a composer the converted-test host installs, consulted with the
// resolved executable path and the child's environment just before the environment block is built.
// It returns null for "unchanged", and a program never has one. It exists for one measured case: a
// host staging fixture trees as JUNCTIONS (no symbolic-link privilege) publishes
// GODEBUG=winsymlink=0, and a test that starts the Go toolchain with its OWN GODEBUG replaces it,
// because os/exec keeps the last duplicate key. The host's composer (testing's
// PackageAncestry.ApplyJunctionGodebug) puts the setting back into a TOOLCHAIN child's GODEBUG only;
// see there. Installed and cleared by reflection, since testing does not reference syscall. Plain CLR
// types keep that reflective contract free of golib's.
internal static Func<string, string[], string[]?>? childEnvironmentComposer;

// StartProcess is the native transcription of exec_windows.go's StartProcess — see the file header
// for why this one declaration cannot be a literal conversion. Ordering, validation and returned
// errors follow the Go original exactly.
public static (nint pid, uintptr handle, error err) StartProcess(@string argv0, slice<@string> argv, ж<ProcAttr> Ꮡattr) {
    if (len(argv0) == 0) {
        return (0, 0, EWINDOWS);
    }
    if (Ꮡattr == nil) {
        Ꮡattr = ᏑzeroProcAttr;
    }

    ref var attr = ref Ꮡattr.Value;
    var sys = attr.Sys;

    if (sys == nil) {
        sys = ᏑzeroSysProcAttr;
    }
    if (len(attr.Files) > 3) {
        return (0, 0, EWINDOWS);
    }
    if (len(attr.Files) < 3) {
        return (0, 0, EINVAL);
    }
    if (len(attr.Dir) != 0) {
        // StartProcess assumes that argv0 is relative to attr.Dir,
        // because it implies Chdir(attr.Dir) before executing argv0.
        // Windows CreateProcess assumes the opposite: it looks for
        // argv0 relative to the current directory, and, only once the new
        // process is started, it does Chdir(attr.Dir). We are adjusting
        // for that difference here by making argv0 absolute.
        (argv0, var dirErr) = joinExeDirAndFName(attr.Dir, argv0);
        if (dirErr != default!) {
            return (0, 0, dirErr);
        }
    }

    // Windows CreateProcess takes the command line as a single string:
    // use attr.CmdLine if set, else build the command line by escaping
    // and joining each argument with spaces.
    @string cmdline = (~sys).CmdLine != ""u8 ? sys.Value.CmdLine : makeCmdLine(argv);

    // The host seam above: a nil Env inherits the process environment, which already carries whatever
    // the host published, so only an explicit environment is offered to the composer.
    slice<@string> env = attr.Env;
    Func<string, string[], string[]?>? composer = System.Threading.Volatile.Read(ref childEnvironmentComposer);

    if (composer is not null && env != nil) {
        string[] current = new string[len(env)];

        for (nint i = 0; i < len(env); i++) {
            current[i] = env[i].ToString();
        }

        string[]? composed = composer(argv0.ToString(), current);

        if (composed is not null) {
            @string[] replaced = new @string[composed.Length];

            for (int i = 0; i < composed.Length; i++) {
                replaced[i] = composed[i];
            }

            env = new slice<@string>(replaced);
        }
    }

    var (envBlock, envErr) = createEnvBlock(env);
    if (envErr != default!) {
        return (0, 0, envErr);
    }

    IntPtr argv0p = IntPtr.Zero;
    IntPtr argvp = IntPtr.Zero;
    IntPtr dirp = IntPtr.Zero;
    IntPtr envp = IntPtr.Zero;
    IntPtr handleListp = IntPtr.Zero;
    IntPtr attrList = IntPtr.Zero;
    IntPtr sip = IntPtr.Zero;
    IntPtr pip = IntPtr.Zero;
    IntPtr processAttrp = IntPtr.Zero;
    IntPtr threadAttrp = IntPtr.Zero;
    IntPtr parentProcessp = IntPtr.Zero;
    bool attrListInitialized = false;

    // The handles duplicated below are owned by this call until CreateProcess consumes them;
    // Go closes the duplicates through a deferred DuplicateHandle(DUPLICATE_CLOSE_SOURCE).
    var duplicated = new System.Collections.Generic.List<ΔHandle>();
    var (currentProcess, _) = GetCurrentProcess();
    var parentProcess = (~sys).ParentProcess != 0 ? sys.Value.ParentProcess : currentProcess;

    try {
        (argv0p, var nameErr) = allocUTF16(argv0);
        if (nameErr != default!) {
            return (0, 0, nameErr);
        }
        if (len(cmdline) != 0) {
            (argvp, var lineErr) = allocUTF16(cmdline);
            if (lineErr != default!) {
                return (0, 0, lineErr);
            }
        }
        if (len(attr.Dir) != 0) {
            (dirp, var dErr) = allocUTF16(attr.Dir);
            if (dErr != default!) {
                return (0, 0, dErr);
            }
        }

        // Duplicate the caller's three std handles into inheritable handles in the parent process,
        // exactly as Go does, and collect the non-zero ones for the inherit list.
        var fd = new ΔHandle[len(attr.Files)];

        for (nint i = 0; i < len(attr.Files); i++) {
            if (attr.Files[i] > 0) {
                ref var dup = ref heap(new ΔHandle(), out var Ꮡdup);
                var dupErr = DuplicateHandle(currentProcess, ((ΔHandle)attr.Files[i]), parentProcess, Ꮡdup, 0, true, DUPLICATE_SAME_ACCESS);
                if (dupErr != default!) {
                    return (0, 0, dupErr);
                }
                fd[i] = dup;
                duplicated.Add(dup);
            }
        }

        // The presence of a NULL handle in the list is enough to cause
        // PROC_THREAD_ATTRIBUTE_HANDLE_LIST to treat the entire list as empty, so remove NULL
        // handles. Additional inherited handles are appended first, matching Go.
        var inherit = new System.Collections.Generic.List<IntPtr>();

        foreach (var h in fd) {
            if (h != 0) {
                inherit.Add((IntPtr)(nint)(nuint)(uintptr)h);
            }
        }
        for (nint i = 0; i < len((~sys).AdditionalInheritedHandles); i++) {
            var h = (~sys).AdditionalInheritedHandles[i];
            if (h != 0) {
                inherit.Add((IntPtr)(nint)(nuint)(uintptr)h);
            }
        }

        bool willInheritHandles = inherit.Count > 0 && !(~sys).NoInheritHandles;

        // Two attribute slots, matching Go's newProcThreadAttributeList(2): the handle list and
        // an optional explicit parent process.
        IntPtr attrSize = IntPtr.Zero;
        win32InitializeProcThreadAttributeList(IntPtr.Zero, 2, 0, ref attrSize);

        if (attrSize == IntPtr.Zero) {
            return (0, 0, errnoErr((Errno)(uint32)Marshal.GetLastSystemError()));
        }

        attrList = Marshal.AllocHGlobal(attrSize);

        if (win32InitializeProcThreadAttributeList(attrList, 2, 0, ref attrSize) == 0) {
            return (0, 0, errnoErr((Errno)(uint32)Marshal.GetLastSystemError()));
        }

        attrListInitialized = true;

        if ((~sys).ParentProcess != 0) {
            parentProcessp = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(parentProcessp, (IntPtr)(nint)(nuint)(uintptr)sys.Value.ParentProcess);

            if (win32UpdateProcThreadAttribute(attrList, 0, (IntPtr)(nint)(uint32)_PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, parentProcessp, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero) == 0) {
                return (0, 0, errnoErr((Errno)(uint32)Marshal.GetLastSystemError()));
            }
        }

        // Do not accidentally inherit more than these handles.
        if (willInheritHandles) {
            handleListp = Marshal.AllocHGlobal(IntPtr.Size * inherit.Count);

            for (int i = 0; i < inherit.Count; i++) {
                Marshal.WriteIntPtr(handleListp, i * IntPtr.Size, inherit[i]);
            }

            if (win32UpdateProcThreadAttribute(attrList, 0, (IntPtr)(nint)(uint32)_PROC_THREAD_ATTRIBUTE_HANDLE_LIST, handleListp, (IntPtr)(IntPtr.Size * inherit.Count), IntPtr.Zero, IntPtr.Zero) == 0) {
                return (0, 0, errnoErr((Errno)(uint32)Marshal.GetLastSystemError()));
            }
        }

        NativeStartupInfoExW si = default;
        si.StartupInfo.Cb = (uint32)Marshal.SizeOf<NativeStartupInfoExW>();
        si.StartupInfo.Flags = STARTF_USESTDHANDLES;
        si.ProcThreadAttributeList = attrList;

        if ((~sys).HideWindow) {
            si.StartupInfo.Flags |= STARTF_USESHOWWINDOW;
            si.StartupInfo.ShowWindow = SW_HIDE;
        }

        si.StartupInfo.StdInput = (IntPtr)(nint)(nuint)(uintptr)fd[0];
        si.StartupInfo.StdOutput = (IntPtr)(nint)(nuint)(uintptr)fd[1];
        si.StartupInfo.StdErr = (IntPtr)(nint)(nuint)(uintptr)fd[2];

        sip = Marshal.AllocHGlobal(Marshal.SizeOf<NativeStartupInfoExW>());
        Marshal.StructureToPtr(si, sip, false);

        pip = Marshal.AllocHGlobal(Marshal.SizeOf<NativeProcessInformation>());

        processAttrp = allocSecurityAttributes((~sys).ProcessAttributes);
        threadAttrp = allocSecurityAttributes((~sys).ThreadAttributes);

        // The environment block is a sequence of NUL-terminated UTF-16 strings ending in a
        // double NUL — createEnvBlock already produced exactly that.
        envp = Marshal.AllocHGlobal(len(envBlock) * sizeof(uint16));

        for (nint i = 0; i < len(envBlock); i++) {
            Marshal.WriteInt16(envp, (int)i * sizeof(uint16), unchecked((short)envBlock[i]));
        }

        uint32 flags = (uint32)((~sys).CreationFlags | (uint32)CREATE_UNICODE_ENVIRONMENT | (uint32)_EXTENDED_STARTUPINFO_PRESENT);

        int ok = (~sys).Token != 0
            ? win32CreateProcessAsUser((IntPtr)(nint)(nuint)(uintptr)(~sys).Token, argv0p, argvp, processAttrp, threadAttrp, willInheritHandles ? 1 : 0, flags, envp, dirp, sip, pip)
            : win32CreateProcess(argv0p, argvp, processAttrp, threadAttrp, willInheritHandles ? 1 : 0, flags, envp, dirp, sip, pip);

        if (ok == 0) {
            return (0, 0, errnoErr((Errno)(uint32)Marshal.GetLastSystemError()));
        }

        NativeProcessInformation pi = Marshal.PtrToStructure<NativeProcessInformation>(pip);

        CloseHandle((ΔHandle)(uintptr)(nuint)(nint)pi.Thread);

        return (((nint)pi.ProcessId), ((uintptr)(nuint)(nint)pi.Process), default!);
    }
    finally {
        // Close this call's duplicates in the parent process — Go's deferred
        // DuplicateHandle(…, DUPLICATE_CLOSE_SOURCE). CreateProcess has already inherited them.
        foreach (var h in duplicated) {
            DuplicateHandle(parentProcess, h, (ΔHandle)(0), (ж<ΔHandle>)(nil), 0, false, DUPLICATE_CLOSE_SOURCE);
        }

        if (attrListInitialized) {
            win32DeleteProcThreadAttributeList(attrList);
        }

        freeAll(argv0p, argvp, dirp, envp, handleListp, attrList, sip, pip, processAttrp, threadAttrp, parentProcessp);
    }
}

// Copies a converted SecurityAttributes (blittable: two uint32s and a uintptr) into unmanaged
// memory for CreateProcess, or returns NULL for a nil pointer — Go passes the pointer straight
// through, and nil means "default security attributes".
private static IntPtr allocSecurityAttributes(ж<SecurityAttributes> Ꮡsa) {
    if (Ꮡsa == nil) {
        return IntPtr.Zero;
    }

    NativeSecurityAttributes native = new NativeSecurityAttributes {
        Length = (uint32)Marshal.SizeOf<NativeSecurityAttributes>(),
        SecurityDescriptor = (IntPtr)(nint)(nuint)(~Ꮡsa).SecurityDescriptor,
        InheritHandle = (int32)(~Ꮡsa).InheritHandle
    };

    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<NativeSecurityAttributes>());
    Marshal.StructureToPtr(native, ptr, false);
    return ptr;
}

private static void freeAll(params IntPtr[] pointers) {
    foreach (IntPtr ptr in pointers) {
        if (ptr != IntPtr.Zero) {
            Marshal.FreeHGlobal(ptr);
        }
    }
}

public static error /*err*/ Exec(@string argv0, slice<@string> argv, slice<@string> envv) {
    error err = default!;

    return EWINDOWS;
}

} // end syscall_package
