// proto_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// readMapping and readMainModuleMapping, hand-owned (manualConversionFuncs["runtime/pprof"]) so that a
// profile carries a mapping for this runtime's Go TEXT. The reason is proto_windows_impl.cs's: this
// runtime's Go PCs are synthetic tokens that no text segment mach_vm_region reports contains. The main
// module's mapping is runtime.GoSyntheticTextRange with the first text segment's file and build ID, and
// readMapping lists it FIRST (ID 1) before the real segments, which follow unchanged. The one other
// change is that start and end come from that range. Every other line is Go's.
//
// Hand-owned (no proto_darwin_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go.runtime;

using errors = errors_package;
using static pprof_package;

partial class pprof_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string machVMInfoFailedˢ = "machVMInfo failed"u8;

} partial class pprofᴛprofileBuilder {

// readMapping adds a mapping entry for the text region of the running process.
// It uses the mach_vm_region region system call to add mapping entries for the
// text region of the running process. Note that currently no attempt is
// made to obtain the buildID information.
internal static void readMapping(this ж<profileBuilder> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // go2cs: the Go text mapping first, so it is ID 1, as Go's main module is.
    var (textStart, textEnd, textExe, textBuildID, textErr) = readMainModuleMapping();
    if (textErr == default!) {
        b.addMappingEntry(textStart, textEnd, 0, textExe, textBuildID, false);
    }

    if (!machVMInfo(Ꮡb.addMapping)) {
        b.addMappingEntry(0, 0, 0, ""u8, ""u8, true);
    }
}

} partial class pprof_package {

internal static (uint64 start, uint64 end, @string exe, @string buildID, error err) readMainModuleMapping() {
    uint64 start = default!;
    uint64 end = default!;
    @string exe = default!;
    @string buildID = default!;

    var first = true;
    var ok = machVMInfo((uint64 lo, uint64 hi, uint64 off, @string @file, @string build) => {
        if (first) {
            (start, end) = (lo, hi);
            (exe, buildID) = (@file, build);
        }
        // May see multiple text segments if rosetta is used for running
        // the go toolchain itself.
        first = false;
    });
    if (!ok) {
        return (0, 0, "", "", errors.New(machVMInfoFailedˢ));
    }
    // go2cs: the Go text range, not the segment's (see the header).
    (start, end) = runtime_package.GoSyntheticTextRange();
    return (start, end, exe, buildID, default!);
}

} // end pprof_package
