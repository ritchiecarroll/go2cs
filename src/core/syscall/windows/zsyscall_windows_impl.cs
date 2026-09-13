// zsyscall_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of the generated syscall wrappers whose STRUCT cannot cross the
// managed/native boundary by address.
//
// Go's GetTimeZoneInformation is a plain mksyscall wrapper:
//
//     r0, _, e1 := Syscall(procGetTimeZoneInformation.Addr(), 1, uintptr(unsafe.Pointer(tzi)), 0, 0)
//
// which works in Go because TIME_ZONE_INFORMATION is 172 bytes laid out exactly as Windows wants,
// with the two WCHAR[32] name buffers INLINE. The converted Timezoneinformation cannot be: its
// name fields are golib `array<uint16>` MANAGED REFERENCES, so the struct is roughly 64 bytes and
// its field offsets bear no relation to the native ones. Handing the kernel that address makes it
// write 172 bytes of native data over a smaller managed object — corrupting the heap past its end
// and leaving fabricated object references where the name arrays belong. The very next statement
// in Go's own zoneinfo_windows.go, `syscall.UTF16ToString(z.StandardName[:])`, then dies with an
// ACCESS_VIOLATION inside slice<ushort>..ctor. Because time.initLocal is what reaches this, EVERY
// converted program that calls Weekday / Location / Local on Windows crashed.
//
// findFirstFile1 / findNextFile1 are the same defect over a bigger record: the kernel writes a
// 592-byte WIN32_FIND_DATAW, whose cFileName[260] and cAlternateFileName[14] are 520 and 28 bytes
// of INLINE storage where the converted win32finddata1 has two one-word `array<uint16>` references.
// Both manifestations of the corruption were observed from path/filepath alone —
// `IndexOutOfRangeException` inside PinnedBuffer when the clobbered reference still resolved to
// something (normBase's `UTF16ToString(data.FileName[:])`), and an outright ACCESS_VIOLATION in
// `slice<ushort>..ctor` when it did not (copyFindData's `src.FileName[..]`).
//
// Process32First / Process32Next are the third member, and the one whose corruption is SILENT.
// PROCESSENTRY32W is 568 bytes ending in szExeFile[260] INLINE; the converted ProcessEntry32 holds
// that as one `array<uint16>` reference, so the record is ~56 bytes and every field past
// th32DefaultHeapID sits at the wrong offset. Nothing faults — the kernel simply writes a 568-byte
// record over a 56-byte object and the caller reads whichever fields happen to land — so
// syscall.Getppid's `pe.ParentProcessID` came back 0 and os's TestGetppid reported a child whose
// parent was process 0. A quiet wrong ANSWER is the worst shape this class takes: the crash cases
// above at least announce themselves. (dwSize is the mirror's business too — Go computes it as
// `unsafe.Sizeof(procEntry)`, which in the conversion is the MANAGED size and would have failed the
// call outright even with the layout right.)
//
// This is the same seam as exec_windows.go's StartProcess (_STARTUPINFOEXW) and takes the same
// remedy, described in src/archived/Baseline-vs-FullConversion.md "Child-process creation": a blittable
// mirror of the native layout, a direct P/Invoke, and an explicit field-for-field copy back into
// the converted struct. Note what does NOT need this: every other declaration in
// zsyscall_windows.go passes scalars and handles, which convert faithfully — only a struct
// carrying `ж<T>` or `array<T>` fields breaks.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_impl.go exists, so a reconvert never regenerates this file);
// marked for consistency with the other hand-owned operational files in this package.
[module: go.GoManualConversion]

// [LibraryImport] requires /unsafe unconditionally (SYSLIB1062); the blittable mirrors below need it
// independently for their `fixed` buffers. Declared rather than inherited — see exec_windows.cs.
//
// The source generator accepted all five declarations UNCHANGED, and that is why this file is the
// one worth reading for what the migration BUYS. Every one of them already takes a pointer to an
// explicitly blittable mirror, which is precisely the discipline the header above spends forty lines
// arguing for — and which, until now, only a reviewer enforced. Hand any of these the CONVERTED
// struct instead (the exact defect this file exists to repair, three times over) and the build now
// fails with a line number, where [DllImport] would have marshalled a copy and let the kernel write
// 172, 592 or 568 bytes into a temporary nobody reads.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // TIME_ZONE_INFORMATION exactly as Windows lays it out: 172 bytes, the two name buffers inline
    // as WCHAR[32]. `fixed` keeps them inline (a C# array field would be another reference, which
    // is the whole bug); the struct is therefore blittable and needs no marshalling layer.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeTimeZoneInformation
    {
        public int32 Bias;
        public fixed uint16 StandardName[32];
        public NativeSystemTime StandardDate;
        public int32 StandardBias;
        public fixed uint16 DaylightName[32];
        public NativeSystemTime DaylightDate;
        public int32 DaylightBias;
    }

    // SYSTEMTIME. Blittable already — the converted Systemtime has the same eight uint16 fields in
    // the same order — but mirrored here so the enclosing layout is stated in one place.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSystemTime
    {
        public uint16 Year;
        public uint16 Month;
        public uint16 DayOfWeek;
        public uint16 Day;
        public uint16 Hour;
        public uint16 Minute;
        public uint16 Second;
        public uint16 Milliseconds;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "GetTimeZoneInformation", SetLastError = true)]
    private static unsafe partial uint32 win32GetTimeZoneInformation(NativeTimeZoneInformation* tzi);

    // GetTimeZoneInformation is the native transcription of the generated wrapper — see the file
    // header for why it cannot be a literal conversion. Return values follow the Go original
    // exactly: rc is the raw TIME_ZONE_ID_* result, and only TIME_ZONE_ID_INVALID (0xffffffff)
    // produces an error.
    public static unsafe (uint32 rc, error err) GetTimeZoneInformation(ж<Timezoneinformation> Ꮡtzi) {
        NativeTimeZoneInformation native;
        uint32 rc = win32GetTimeZoneInformation(&native);

        if (rc == 0xffffffffU) {
            return (rc, errnoErr((Errno)(uint32)Marshal.GetLastSystemError()));
        }

        ref var tzi = ref Ꮡtzi.Value;

        tzi.Bias = native.Bias;
        tzi.StandardDate = toSystemtime(native.StandardDate);
        tzi.StandardBias = native.StandardBias;
        tzi.DaylightDate = toSystemtime(native.DaylightDate);
        tzi.DaylightBias = native.DaylightBias;

        // The name buffers are copied whole, NULs included: Go reads them as
        // `UTF16ToString(z.StandardName[:])`, which stops at the first NUL, and Windows pads the
        // remainder with NULs. Copying only up to the terminator would leave stale runes behind it
        // when a Timezoneinformation is reused.
        copyNativeName(native.StandardName, ref tzi.StandardName, 32);
        copyNativeName(native.DaylightName, ref tzi.DaylightName, 32);

        return (rc, default!);
    }

    private static Systemtime toSystemtime(NativeSystemTime value) {
        return new Systemtime{
            Year = value.Year,
            Month = value.Month,
            DayOfWeek = value.DayOfWeek,
            Day = value.Day,
            Hour = value.Hour,
            Minute = value.Minute,
            Second = value.Second,
            Milliseconds = value.Milliseconds
        };
    }

    // Copies a native WCHAR[length] buffer into the converted struct's `array<uint16>` field. The
    // destination is (re)allocated when it is not already that long, so a struct that reached here
    // as `default` — its field initializer never having run — is filled rather than dereferenced
    // through a null backing.
    private static unsafe void copyNativeName(uint16* source, ref array<uint16> destination, nint length) {
        if (destination.Length != length) {
            destination = new array<uint16>(length);
        }

        for (nint i = 0; i < length; i++) {
            destination[i] = source[i];
        }
    }

    // MAX_PATH as the WIN32_FIND_DATAW layout uses it. syscall's own MAX_PATH is an UntypedInt
    // property, not a compile-time constant, and a `fixed` buffer needs one.
    private const int maxPath = 260;

    // WIN32_FIND_DATAW exactly as Windows lays it out: 592 bytes, both name buffers inline as
    // WCHAR[260] and WCHAR[14]. `fixed` is what keeps them inline — a C# array field would be
    // another managed reference, which is the whole bug — so the struct is blittable and needs no
    // marshalling layer. Field names and order follow the converted win32finddata1, which is the
    // struct the copy below fills.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeWin32FindDataW
    {
        public uint32 FileAttributes;
        public NativeFiletime CreationTime;
        public NativeFiletime LastAccessTime;
        public NativeFiletime LastWriteTime;
        public uint32 FileSizeHigh;
        public uint32 FileSizeLow;
        public uint32 Reserved0;
        public uint32 Reserved1;
        public fixed uint16 FileName[maxPath];
        public fixed uint16 AlternateFileName[14];
    }

    // FILETIME. Blittable already — the converted Filetime has the same two uint32 fields in the
    // same order — but mirrored here so the enclosing layout is stated in one place.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeFiletime
    {
        public uint32 LowDateTime;
        public uint32 HighDateTime;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "FindFirstFileW", SetLastError = true)]
    private static unsafe partial nint win32FindFirstFileW(uint16* name, NativeWin32FindDataW* data);

    [LibraryImport("kernel32.dll", EntryPoint = "FindNextFileW", SetLastError = true)]
    private static unsafe partial int32 win32FindNextFileW(nint handle, NativeWin32FindDataW* data);

    // findFirstFile1 is the native transcription of the generated wrapper — see the file header for
    // why it cannot be a literal conversion. Results follow the Go original exactly: the raw HANDLE
    // is returned whatever it is, and only InvalidHandle produces an error. `data` is left untouched
    // on failure, as it is in Go, because the kernel did not write it.
    internal static unsafe (ΔHandle handle, error err) findFirstFile1(ж<uint16> Ꮡname, ж<win32finddata1> Ꮡdata) {
        NativeWin32FindDataW native;
        nint raw;

        // The name is the caller's NUL-terminated UTF-16 buffer, reached through a golib pointer box
        // whose uintptr conversion can only hand back a TRANSIENT pinned address (see the note in
        // dll_windows.cs). Pinning it HERE keeps it fixed for the whole call instead — `Value` on an
        // element box resolves to a ref INTO the caller's backing array, so `fixed` pins that array
        // and every rune of the name stays contiguous behind the pointer for the whole call.
        fixed (uint16* name = &Ꮡname.Value) {
            raw = win32FindFirstFileW(name, &native);
        }

        ΔHandle handle = (ΔHandle)(uintptr)raw;

        if (handle == InvalidHandle) {
            return (handle, errnoErr((Errno)(uint32)Marshal.GetLastSystemError()));
        }

        copyNativeFindData(&native, Ꮡdata);

        return (handle, default!);
    }

    // findNextFile1 is the native transcription of the generated wrapper — see findFirstFile1. The
    // ordinary end of an enumeration arrives here as ERROR_NO_MORE_FILES, which callers compare
    // against, so the last error has to be reported faithfully rather than flattened.
    internal static unsafe error /*err*/ findNextFile1(ΔHandle handle, ж<win32finddata1> Ꮡdata) {
        NativeWin32FindDataW native;

        if (win32FindNextFileW((nint)(nuint)(uintptr)handle, &native) == 0) {
            return errnoErr((Errno)(uint32)Marshal.GetLastSystemError());
        }

        copyNativeFindData(&native, Ꮡdata);

        return default!;
    }

    // Copies the native record the kernel just wrote into the converted win32finddata1. Both name
    // buffers are copied WHOLE, NULs included: Go reads them as `UTF16ToString(data.FileName[:])`,
    // which stops at the first NUL, and Windows pads the remainder — copying only up to the
    // terminator would leave the previous entry's runes behind it, and the same win32finddata1 is
    // reused across every FindNextFile of an enumeration.
    private static unsafe void copyNativeFindData(NativeWin32FindDataW* native, ж<win32finddata1> Ꮡdata) {
        ref var data = ref Ꮡdata.Value;

        data.FileAttributes = native->FileAttributes;
        data.CreationTime = toFiletime(native->CreationTime);
        data.LastAccessTime = toFiletime(native->LastAccessTime);
        data.LastWriteTime = toFiletime(native->LastWriteTime);
        data.FileSizeHigh = native->FileSizeHigh;
        data.FileSizeLow = native->FileSizeLow;
        // Reserved0 carries the reparse-point tag when FileAttributes has FILE_ATTRIBUTE_REPARSE_POINT
        // — path/filepath's EvalSymlinks reads it to tell IO_REPARSE_TAG_SYMLINK from a mount point —
        // and is documented UNDEFINED otherwise, so it is copied verbatim rather than normalized.
        data.Reserved0 = native->Reserved0;
        data.Reserved1 = native->Reserved1;

        copyNativeName(native->FileName, ref data.FileName, maxPath);
        copyNativeName(native->AlternateFileName, ref data.AlternateFileName, 14);
    }

    private static Filetime toFiletime(NativeFiletime value) {
        return new Filetime{
            LowDateTime = value.LowDateTime,
            HighDateTime = value.HighDateTime
        };
    }

    // PROCESSENTRY32W exactly as Windows lays it out: 568 bytes with szExeFile[MAX_PATH] inline.
    // th32DefaultHeapID is ULONG_PTR, so the sequential layout pads it to an 8-byte boundary on x64
    // just as the native header does.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeProcessEntry32
    {
        public uint32 Size;
        public uint32 Usage;
        public uint32 ProcessID;
        public nuint DefaultHeapID;
        public uint32 ModuleID;
        public uint32 Threads;
        public uint32 ParentProcessID;
        public int32 PriClassBase;
        public uint32 Flags;
        public fixed uint16 ExeFile[maxPath];
    }

    [LibraryImport("kernel32.dll", EntryPoint = "Process32FirstW", SetLastError = true)]
    private static unsafe partial int32 win32Process32First(nint snapshot, NativeProcessEntry32* entry);

    [LibraryImport("kernel32.dll", EntryPoint = "Process32NextW", SetLastError = true)]
    private static unsafe partial int32 win32Process32Next(nint snapshot, NativeProcessEntry32* entry);

    // Process32First is the native transcription of the generated wrapper — see the file header for
    // why it cannot be a literal conversion. `procEntry` is left untouched on failure, as in Go,
    // because the kernel did not write it.
    public static unsafe error /*err*/ Process32First(ΔHandle snapshot, ж<ProcessEntry32> ᏑprocEntry) {
        NativeProcessEntry32 native = default;

        // dwSize is an INPUT: Windows rejects the call unless it is the native record size. Go sets
        // it in getProcessEntry from `unsafe.Sizeof(procEntry)`, which the conversion answers with
        // the MANAGED size — so the native layout owns this field, here, where the layout is stated.
        native.Size = (uint32)sizeof(NativeProcessEntry32);

        if (win32Process32First((nint)(nuint)(uintptr)snapshot, &native) == 0) {
            return errnoErr((Errno)(uint32)Marshal.GetLastSystemError());
        }

        copyNativeProcessEntry(&native, ᏑprocEntry);

        return default!;
    }

    // Process32Next is the native transcription of the generated wrapper — see Process32First. The
    // ordinary end of an enumeration arrives as ERROR_NO_MORE_FILES, which the Go caller's loop
    // compares against, so the last error is reported faithfully rather than flattened.
    public static unsafe error /*err*/ Process32Next(ΔHandle snapshot, ж<ProcessEntry32> ᏑprocEntry) {
        NativeProcessEntry32 native = default;

        native.Size = (uint32)sizeof(NativeProcessEntry32);

        if (win32Process32Next((nint)(nuint)(uintptr)snapshot, &native) == 0) {
            return errnoErr((Errno)(uint32)Marshal.GetLastSystemError());
        }

        copyNativeProcessEntry(&native, ᏑprocEntry);

        return default!;
    }

    // Copies the native record into the converted ProcessEntry32. The name buffer is copied WHOLE,
    // NULs included, for the reason copyNativeFindData gives: one entry is reused across a whole
    // Process32Next walk, and Go reads it as `UTF16ToString(pe.ExeFile[:])`, which stops at the
    // first NUL. Size is reported as the NATIVE size, which is what a Go caller reading it back
    // would mean by it.
    private static unsafe void copyNativeProcessEntry(NativeProcessEntry32* native, ж<ProcessEntry32> ᏑprocEntry) {
        ref var procEntry = ref ᏑprocEntry.Value;

        procEntry.Size = native->Size;
        procEntry.Usage = native->Usage;
        procEntry.ProcessID = native->ProcessID;
        procEntry.DefaultHeapID = (uintptr)native->DefaultHeapID;
        procEntry.ModuleID = native->ModuleID;
        procEntry.Threads = native->Threads;
        procEntry.ParentProcessID = native->ParentProcessID;
        procEntry.PriClassBase = native->PriClassBase;
        procEntry.Flags = native->Flags;

        copyNativeName(native->ExeFile, ref procEntry.ExeFile, maxPath);
    }
}
