// cpu_arm64_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.
//
// osInit below is derived from Go, whose notice it carries (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE, part by part:
// - osInit: DERIVED FROM src/internal/cpu/cpu_arm64_darwin.go, Go 1.26.11, func osInit. A port: Go's statements in
//   Go's order, Go's comments verbatim, with the one ruled departure below (HasDIT).
// - initDarwinArm64FeatureDetection: INSPIRED BY src/runtime/proc.go, Go 1.26.11, funcs cpuinit (its
//   cpu.Initialize(env) call) and getGodebugEarly (GODEBUG from the environment). Same behaviour, go2cs's own
//   shape: a module initializer that reads the process environment.

// darwin/arm64's osInit: cpu_arm64_darwin.go, displaced for the darwin/arm64 TARGET alone
// (manualConversionFuncs["internal/cpu"]["osInit"], src/go2cs/manualTypeOperations.go), so the converted
// cpu_arm64_darwin.cs in this folder carries a placeholder and cpu_arm64.cs's doinit calls this body. Its
// principal is that placeholder file, which only darwin/arm64 emits, so the merge homes it here.
//
// Go's body, with ONE DEPARTURE, RULED (COORD, 2026-11-06, the AES trio seat's Q2; kept for this flavour by
// docs/phase4/DESIGN-darwin-arm64-flavour.md section 5, ruling R6): HasDIT stays FALSE. Go reads it from
// hw.optional.arm.FEAT_DIT, and Apple silicon reports it. But a capability flag must describe what the
// CONVERTED runtime can do, not the silicon: crypto/subtle.WithDataIndependentTiming enables DIT by setting
// PSTATE.DIT, and the .NET runtime exposes no control for PSTATE.DIT (internal/runtime/sys/no_dit.cs:
// DITSupported is false). HasDIT=true would let a program that reads the flag believe DIT had been enabled
// when it had not: a silent false security claim. darwin/amd64 on an arm64 process applies the same ruling
// (darwin-amd64/cpu_arm64_process_darwin_impl.cs).
//
// The sysctl reads go through the converted sysctlEnabled (darwin/cpu_darwin.cs) and
// darwin/cpu_darwin_impl.cs's sysctlbynameInt32, the same libSystem call Go's runtime push makes.
//
// START-UP. In Go the runtime reaches this body before any package init reads a flag: schedinit calls cpuinit,
// which calls cpu.Initialize(getGodebugEarly()), whose doinit registers cpu_arm64.go's option table, calls osInit,
// and then processOptions applies GODEBUG's cpu.* options. go2cs never runs schedinit, so initDarwinArm64 below is
// that slot: a module initializer, as every other flavour's internal/cpu carries one (amd64/cpu_x86_impl.cs explains
// the slot; linux-arm64/cpu_arm64_hwcap_impl.cs is this one's twin). It calls the CONVERTED Initialize, so the
// option table and processOptions are Go's own. getGodebugEarly answers GODEBUG on darwin as on linux, so
// cpu.aes=off clears ARM64.HasAES before the runtime's alginit reads it, as in Go. HasDIT needs no clearing after
// processOptions: no GODEBUG option names it, and osInit leaves it false. Without this initializer every
// cpu.ARM64 flag of this flavour read false on Apple silicon, and the AES hash, crypto and bytealg took the
// fallbacks Go does not take there (P6's D5 golib-tests reading on osx-arm64, 2026-11-25; COORD ruled the fold
// into D2). src/go2cs/cpuStartupFlavours_test.go requires such an initializer in every flavour, and GolibTests'
// DarwinArm64CpuStartupTests reads it on Apple silicon through GoDarwinArm64OptionsProbe below.
[module: go.GoManualConversion]

namespace go.@internal;

using System;
using System.Runtime.CompilerServices;

partial class cpu_package {

/// <summary>
/// Runs the converted <see cref="Initialize"/> over the process's GODEBUG, standing in for the <c>schedinit</c>
/// slot where Go calls <c>cpuinit</c> (the header's START-UP).
/// </summary>
[ModuleInitializer]
internal static void initDarwinArm64FeatureDetection() {
    Initialize(Environment.GetEnvironmentVariable("GODEBUG") ?? "");
}

/// <summary>
/// GolibTests' probe (DarwinArm64CpuStartupTests, the darwin twin of linux-arm64's GoLinuxArm64OptionsProbe): applies
/// <paramref name="godebug"/> exactly as start-up does. The converted doinit registers cpu_arm64.go's option table and
/// calls osInit above; processOptions applies the GODEBUG options. The probe returns each option's name and resulting
/// flag, then restores the flags and the table. Re-running doinit gives a fresh table, because processOptions leaves its
/// Specified/Enable marks on the options it read.
/// </summary>
public static (string name, bool enabled)[] GoDarwinArm64OptionsProbe(string godebug) {
    ARM64ᴛ1 saved = ARM64;
    slice<option> savedOptions = options;

    try {
        doinit();
        processOptions(godebug);

        (string, bool)[] result = new (string, bool)[len(options)];

        for (int i = 0; i < result.Length; i++)
            result[i] = (options[i].Name.ToString(), options[i].Feature.Value);

        return result;
    }
    finally {
        ARM64 = saved;
        options = savedOptions;
    }
}

internal static void osInit() {
    // macOS 12 moved these to the hw.optional.arm tree, but as of Go 1.24 we
    // still support macOS 11. See [Determine Encryption Capabilities].
    //
    // [Determine Encryption Capabilities]: https://developer.apple.com/documentation/kernel/1387446-sysctlbyname/determining_instruction_set_characteristics#3918855
    ARM64.HasATOMICS = sysctlEnabled(slice<byte>("hw.optional.armv8_1_atomics\x00"u8));
    ARM64.HasCRC32 = sysctlEnabled(slice<byte>("hw.optional.armv8_crc32\x00"u8));
    ARM64.HasSHA512 = sysctlEnabled(slice<byte>("hw.optional.armv8_2_sha512\x00"u8));
    ARM64.HasSHA3 = sysctlEnabled(slice<byte>("hw.optional.armv8_2_sha3\x00"u8));

    // ARM64.HasDIT stays false: the header's ruled departure. Go reads hw.optional.arm.FEAT_DIT here.

    // There are no hw.optional sysctl values for the below features on macOS 11
    // to detect their supported state dynamically (although they are available
    // in the hw.optional.arm tree on macOS 12). Assume the CPU features that
    // Apple Silicon M1 supports to be available on all future iterations.
    ARM64.HasAES = true;
    ARM64.HasPMULL = true;
    ARM64.HasSHA1 = true;
    ARM64.HasSHA2 = true;
}

} // end cpu_package
