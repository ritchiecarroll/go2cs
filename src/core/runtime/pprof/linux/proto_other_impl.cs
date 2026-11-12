// proto_other_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// readMapping, hand-owned (manualConversionFuncs["runtime/pprof"]) so that a profile carries a mapping
// for this runtime's Go TEXT. The reason is proto_windows_impl.cs's: this runtime's Go PCs are synthetic
// tokens that no /proc/self/maps entry contains. Here the Go text mapping (runtime.GoSyntheticTextRange,
// with the executable's path and GNU build ID) is APPENDED after /proc/self/maps's entries, so their IDs
// do not move: linux's TestConvertCPUProfile and TestConvertMemProfile take their test PCs and expected
// mappings from /proc/self/maps by ID. When /proc/self/maps yields nothing, Go's fake entry already
// matches every address and nothing is appended. Every other line is Go's.
//
// Hand-owned (no proto_other_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go.runtime;

using os = os_package;
using static pprof_package;

partial class pprof_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string procSelfMapsˢ = "/proc/self/maps"u8;

} partial class pprofᴛprofileBuilder {

// readMapping reads /proc/self/maps and writes mappings to b.pb.
// It saves the address ranges of the mappings in b.mem for use
// when emitting locations.
internal static void readMapping(this ж<profileBuilder> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (data, _) = os.ReadFile(procSelfMapsˢ);
    parseProcSelfMaps(data, Ꮡb.addMapping);
    if (len(b.mem) == 0) {
        // pprof expects a map entry, so fake one.
        b.addMappingEntry(0, 0, 0, ""u8, ""u8, true);
        return;
    }

    // go2cs: the Go text mapping, after the real ones (see the header).
    var (exe, err) = os.Executable();
    if (err == default!) {
        var (buildID, _) = elfBuildID(exe);
        var (textStart, textEnd) = runtime_package.GoSyntheticTextRange();
        b.addMappingEntry(textStart, textEnd, 0, exe, buildID, false);
    }
}

} partial class pprof_package {

} // end pprof_package
