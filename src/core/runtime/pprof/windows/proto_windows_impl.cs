// proto_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// readMapping and readMainModuleMapping, hand-owned (manualConversionFuncs["runtime/pprof"]) so that a
// profile carries a mapping for this runtime's Go TEXT.
//
// Go's text is the executable's image, so Go reports the main module's address range and every Go PC
// falls inside it. This runtime's Go PCs are synthetic tokens (GoSyntheticPC's band, and the caller-span
// band below it) that no module the OS reports contains, so every location built from one had no
// mapping: TestConvertCPUProfile and TestConvertMemProfile compare exactly that, with function PCs, on
// windows. The main module's mapping is therefore runtime.GoSyntheticTextRange, with the real exe path
// and build ID, and readMapping lists it FIRST (ID 1) before the real modules, which follow unchanged.
// The one other change is that start and end come from that range. Every other line is Go's.
//
// Hand-owned (no proto_windows_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go.runtime;

using errors = errors_package;
using windows = @internal.syscall.windows_package;
using os = os_package;
using syscall = syscall_package;
using @internal.syscall;
using static pprof_package;

partial class pprof_package {

} partial class pprofᴛprofileBuilder {

// readMapping adds memory mapping information to the profile.
internal static void readMapping(this ж<profileBuilder> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        // go2cs: the Go text mapping first, so it is ID 1, as Go's main module is.
        var (textStart, textEnd, textExe, textBuildID, textErr) = readMainModuleMapping();
        if (textErr == default!) {
            b.addMappingEntry(textStart, textEnd, 0, textExe, textBuildID, false);
        }

        var (snap, err) = createModuleSnapshot();
        if (err != default!) {
            // pprof expects a map entry, so fake one, when we haven't added anything yet.
            b.addMappingEntry(0, 0, 0, ""u8, ""u8, true);
            return;
        }
        defer(() => {
            _ = syscall.CloseHandle(snap);
        }, ref ᒐ);
        ref var module = ref heap(new windows.ModuleEntry32(), out var Ꮡmodule);
        module.Size = (uint32)windows.SizeofModuleEntry32;
        err = windows.Module32First(snap, Ꮡmodule);
        if (err != default!) {
            // pprof expects a map entry, so fake one, when we haven't added anything yet.
            b.addMappingEntry(0, 0, 0, ""u8, ""u8, true);
            return;
        }
        while (err == default!) {
            @string exe = syscall.UTF16ToString(module.ExePath[..]);
            b.addMappingEntry(
                (uint64)module.ModBaseAddr,
                (uint64)module.ModBaseAddr + (uint64)module.ModBaseSize,
                0,
                exe,
                peBuildID(exe),
                false);
            err = windows.Module32Next(snap, Ꮡmodule);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} partial class pprof_package {

internal static (uint64 start, uint64 end, @string exe, @string buildID, error err) readMainModuleMapping() {
    uint64 start = default!;
    uint64 end = default!;
    @string exe = default!;
    @string buildID = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        (exe, err) = os.Executable();
        if (err != default!) {
            (start, end, exe, buildID, err) = (0, 0, "", "", err); goto ᒐdone;
        }
        (var snap, err) = createModuleSnapshot();
        if (err != default!) {
            (start, end, exe, buildID, err) = (0, 0, "", "", err); goto ᒐdone;
        }
        defer(() => {
            _ = syscall.CloseHandle(snap);
        }, ref ᒐ);
        ref var module = ref heap(new windows.ModuleEntry32(), out var Ꮡmodule);
        module.Size = (uint32)windows.SizeofModuleEntry32;
        err = windows.Module32First(snap, Ꮡmodule);
        if (err != default!) {
            (start, end, exe, buildID, err) = (0, 0, "", "", err); goto ᒐdone;
        }
        // go2cs: the Go text range, not the module's image range (see the header).
        var (textStart, textEnd) = runtime_package.GoSyntheticTextRange();
        (start, end, exe, buildID, err) = (textStart, textEnd, exe, peBuildID(exe), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (start, end, exe, buildID, err);
}

} // end pprof_package
