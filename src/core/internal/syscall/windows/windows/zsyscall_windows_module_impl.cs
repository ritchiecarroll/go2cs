// zsyscall_windows_module_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The MODULE-ENUMERATION member of the syscall struct-passing class -- the wrappers whose STRUCT
// cannot cross the managed/native boundary by address.
//
// The class, its failure mode and its remedy are documented once, in
// syscall/windows/zsyscall_windows_impl.cs (GetTimeZoneInformation, findFirstFile1/findNextFile1,
// Process32First/Process32Next); that file is the reference and this one does not restate it. The
// short form: a converted struct holding golib `array<T>` or `ж<T>` fields is a CLR auto-layout
// record with MANAGED REFERENCES where the native record has INLINE storage, so handing the kernel
// its address makes the kernel write the native record's full length over a much smaller managed
// object.
//
// THIS ONE, in numbers. MODULEENTRY32W is 1080 bytes ending in szModule[256] and szExePath[260]
// INLINE. The converted ModuleEntry32 (internal/syscall/windows/windows/syscall_windows.cs) holds
// both as `array<uint16>` references, so the record is roughly 64 bytes and every field past
// ProccntUsage sits at the wrong offset. The generated wrapper handed kernel32
// `(uintptr)ᏑmoduleEntry`, and the caller had already set `module.Size = SizeofModuleEntry32` --
// which the converter folds from Go's `unsafe.Sizeof` to the NATIVE 1080 (syscall_windows.cs:210) --
// so Module32FirstW wrote a full 1080-byte native record over that ~64-byte box: about a kilobyte
// of GC heap past the object, with module-path characters left where the ExePath reference belongs.
//
// WHO REACHED IT, and how it announced itself. runtime/pprof's newProfileBuilder calls readMapping
// on EVERY profile it builds, and readMapping's very next statement after Module32First is
// `syscall.UTF16ToString(module.ExePath[:])`. Measured at master (Release, tiering off):
//
//     System.AccessViolationException: Attempted to read or write protected memory.
//        at go.slice`1[UInt16]..ctor(UInt16[], IntPtr, IntPtr, IntPtr)
//        at go.array`1[UInt16].get_Item(System.Range)
//        at go.runtime.pprof_package.readMapping(ж<profileBuilder>)
//        at go.runtime.pprof_package.newProfileBuilder(Writer)
//        at translateCPUProfile -> TestConvertCPUProfileNoSamples
//
// -- the same manifestation findFirstFile1 produced one package over, for the same reason. When the
// fabricated reference happens to resolve instead, the corruption surfaces DOWNSTREAM and blames a
// package that has nothing to do with it: the ungated census run died in `internal/testlog`'s class
// constructor, reached from peBuildID two statements later.
//
// Both wrappers receive the record as a TYPED `*ModuleEntry32`, so the ordinary mirror remedy
// applies -- exactly as this package's zsyscall_windows_impl.cs header predicted when it listed
// Module32First/Module32Next among the four same-shape wrappers left for the suite that would reach
// them. (NetShareAdd, in that same file, is the one member the remedy cannot reach, because its
// record arrives as a raw byte address with the managed identity already gone.)
//
// The CALL is unchanged from the generated body -- same LazyProc, same `syscall.Syscall`, same
// `errnoErr(e1)` on the r1 == 0 path -- and only the memory the second argument names is different.

using System;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_module_impl.go exists, so a reconvert never regenerates this
// file). The two declarations it replaces are registered in the converter's manualConversionFuncs,
// which is what turns the generated bodies into placeholders.
[module: go.GoManualConversion]

// The mirror's `fixed` buffers and its address are pointer work. Declared rather than inherited --
// see net_windows_impl.cs.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = go.syscall_package;

partial class windows_package
{
    // MAX_MODULE_NAME32 + 1 and MAX_PATH as the MODULEENTRY32W layout uses them. This package's own
    // MAX_MODULE_NAME32 and syscall's MAX_PATH are UntypedInt properties, not compile-time
    // constants, and a `fixed` buffer needs one.
    private const int moduleNameLength = 256;
    private const int modulePathLength = 260;

    // MODULEENTRY32W exactly as Windows lays it out: 1080 bytes with szModule[256] and
    // szExePath[260] inline. `fixed` is what keeps them inline -- a C# array field would be another
    // managed reference, which is the whole bug -- so the struct is blittable and needs no
    // marshalling layer. modBaseAddr and hModule are pointer-width, so the sequential layout pads
    // them to 8-byte boundaries on x64 just as the native header does, putting szModule at 48 and
    // szExePath at 560.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeModuleEntry32
    {
        public uint32 Size;
        public uint32 ModuleID;
        public uint32 ProcessID;
        public uint32 GlblcntUsage;
        public uint32 ProccntUsage;
        public nuint ModBaseAddr;
        public uint32 ModBaseSize;
        public nuint ModuleHandle;
        public fixed uint16 Module[moduleNameLength];
        public fixed uint16 ExePath[modulePathLength];
    }

    /// <summary>
    /// Native transcription of the generated <c>Module32First</c> wrapper — see the file header for
    /// why it cannot be a literal conversion.
    /// </summary>
    /// <remarks>
    /// <c>moduleEntry</c> is left untouched on failure, as in Go, because the kernel did not write
    /// it — which is what lets <c>readMapping</c>'s <c>err != nil</c> arm fall back to its fake
    /// mapping entry without reading a half-filled record.
    /// </remarks>
    public static unsafe error /*err*/ Module32First(syscallꓸHandle snapshot, ж<ModuleEntry32> ᏑmoduleEntry) {
        NativeModuleEntry32 native = default;

        // dwSize is an INPUT: Module32FirstW rejects the call unless it is the native record size.
        // The native layout owns it here, where that layout is stated, for the reason Process32First
        // gives — Go computes it from `unsafe.Sizeof`, which a conversion can answer with the
        // MANAGED size. It happens to agree with the caller today (the converter folded
        // SizeofModuleEntry32 to Go's 1080), so this is belt-and-braces rather than a divergence.
        native.Size = (uint32)sizeof(NativeModuleEntry32);

        var (r1, _, e1) = syscall.Syscall(procModule32FirstW.Addr(), 2, (uintptr)snapshot, (uintptr)(void*)(&native), 0);

        if (r1 == 0) {
            return errnoErr(e1);
        }

        copyNativeModuleEntry(&native, ᏑmoduleEntry);

        return default!;
    }

    /// <summary>
    /// Native transcription of the generated <c>Module32Next</c> wrapper — see
    /// <c>Module32First</c>.
    /// </summary>
    /// <remarks>
    /// The ordinary end of an enumeration arrives as ERROR_NO_MORE_FILES, and <c>readMapping</c>'s
    /// loop terminates on any non-nil error, so the last error is reported faithfully rather than
    /// flattened. A FRESH mirror per call is correct because the enumeration state lives in the
    /// snapshot handle, not in the entry — Go reuses one entry only because it is the caller's
    /// buffer.
    /// </remarks>
    public static unsafe error /*err*/ Module32Next(syscallꓸHandle snapshot, ж<ModuleEntry32> ᏑmoduleEntry) {
        NativeModuleEntry32 native = default;

        native.Size = (uint32)sizeof(NativeModuleEntry32);

        var (r1, _, e1) = syscall.Syscall(procModule32NextW.Addr(), 2, (uintptr)snapshot, (uintptr)(void*)(&native), 0);

        if (r1 == 0) {
            return errnoErr(e1);
        }

        copyNativeModuleEntry(&native, ᏑmoduleEntry);

        return default!;
    }

    // Copies the native record into the converted ModuleEntry32. Both name buffers are copied WHOLE,
    // NULs included, for the reason Process32First's copy gives: one entry is reused across a whole
    // Module32Next walk, and Go reads it as `UTF16ToString(module.ExePath[:])`, which stops at the
    // first NUL. Size is reported as the NATIVE size, which is what a Go caller reading it back
    // would mean by it.
    private static unsafe void copyNativeModuleEntry(NativeModuleEntry32* native, ж<ModuleEntry32> ᏑmoduleEntry) {
        ref var moduleEntry = ref ᏑmoduleEntry.Value;

        moduleEntry.Size = native->Size;
        moduleEntry.ModuleID = native->ModuleID;
        moduleEntry.ProcessID = native->ProcessID;
        moduleEntry.GlblcntUsage = native->GlblcntUsage;
        moduleEntry.ProccntUsage = native->ProccntUsage;
        moduleEntry.ModBaseAddr = native->ModBaseAddr;
        moduleEntry.ModBaseSize = native->ModBaseSize;
        moduleEntry.ModuleHandle = native->ModuleHandle;

        copyNativeModuleName(native->Module, ref moduleEntry.Module, moduleNameLength);
        copyNativeModuleName(native->ExePath, ref moduleEntry.ExePath, modulePathLength);
    }

    // Copies a native WCHAR[length] buffer into the converted struct's `array<uint16>` field. The
    // destination is (re)allocated when it is not already that long, so a record that reached here
    // as `default` — its field initializer never having run — is filled rather than dereferenced.
    private static unsafe void copyNativeModuleName(uint16* source, ref array<uint16> destination, nint length) {
        if (destination.Length != length) {
            destination = new array<uint16>(length);
        }

        for (nint i = 0; i < length; i++) {
            destination[i] = source[i];
        }
    }
}
