// cpu_arm64_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// ---------------------------------------------------------------------------------------------
// ARM64 FEATURE DETECTION FOR AN ARM64 PROCESS, in a corpus converted for amd64.
//
// The corpus is converted once for darwin/amd64, and both mac RIDs run that one build, so on Apple
// silicon an amd64-converted internal/cpu runs in a native arm64 .NET process. cpu_x86_impl.cs's
// initializer returns there at once (no X86Base), which is right: Go's cpu.X86 is all false on arm64,
// because x86's doinit is not built. But nothing filled cpu.ARM64 either, and Go on the same Mac
// fills it in cpu_arm64.go's doinit and cpu_arm64_darwin.go's osInit. The consumers that read it:
//
//   - the runtime's hash selection (runtime/hash_impl.cs initHashAlgorithm, alginit's arm64 branch:
//     the AES hash when cpu.ARM64.HasAES), whose absence made TestMemHash32/64Equality and
//     TestMemHashGlobalSeed/aes diverge from Go on osx-arm64 (run 42138564497);
//   - crypto/tls's hasGCMAsmARM64 (HasAES && HasPMULL), which decides hasAESGCMHardwareSupport and so
//     the cipher-suite preference: with cpu.ARM64 unfilled the converted stack preferred
//     ChaCha20-Poly1305 where Go on the same Mac prefers AES-GCM. USER-VISIBLE: this file moves it to
//     Go's choice.
//
// So this file is cpu_arm64.go's doinit (the option table below, verbatim) and the stand-in for
// Initialize's slot, keyed on the PROCESS architecture rather than on the converted GOARCH constant:
// it runs only when RuntimeInformation.ProcessArchitecture is Arm64, so an x64 process on any OS is
// unchanged. osInit is per-OS in Go; darwin's is darwin/cpu_arm64_darwin_impl.cs. darwin is the only
// OS that ships an arm64 process, so on any other OS the partial below has no body and every flag
// keeps its zero value, the answer it gives today.
//
// GODEBUG: Go's Initialize runs doinit, then processOptions(env), with env from getGodebugEarly (GODEBUG
// off Windows, "" on it). This does the same, through the same GoCpuAppliesGodebug cpu_x86_impl.cs
// reads, so GODEBUG=cpu.aes=off clears ARM64.HasAES before the runtime reads it, as in Go.
// TestIssue66841 and TestMemHashGlobalSeed/noaes re-exec with exactly that knob to reach the fallback.
//
// NOT runtime.GOARCH. That stays the converted "amd64" (BOARD, the 2026-11-05 GOARCH row, OPEN). This
// file answers what the HOST can do for the consumers that ask cpu.ARM64; it changes no cpu.X86 flag
// and no GOARCH-folded branch.
// ---------------------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;

[module: go.GoManualConversion]

namespace go.@internal;

partial class cpu_package
{
    /// <summary>Whether this process runs as arm64, the condition the arm64 arm keys on.</summary>
    public static bool GoIsArm64Process => RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

    /// <summary>
    /// Fills <see cref="ARM64"/> on an arm64 process, standing in for the <c>schedinit</c> slot where
    /// Go calls <c>cpuinit</c> → <c>Initialize</c> → <c>doinit</c> (cpu_x86_impl.cs explains the slot).
    /// </summary>
    [ModuleInitializer]
    internal static void initArm64FeatureDetection()
    {
        if (!GoIsArm64Process)
            return;

        registerArm64Options();
        osInitArm64();

        if (GoCpuAppliesGodebug)
            processOptions(Environment.GetEnvironmentVariable("GODEBUG") ?? "");
    }

    // The OS half of Go's arm64 doinit: darwin/cpu_arm64_darwin_impl.cs on darwin, nothing elsewhere.
    static partial void osInitArm64();

    // The option table this process's Initialize registered: arm64's on an arm64 process, x86's otherwise.
    // GoCpuOptionsProbe restores it.
    private static void registerProcessOptions()
    {
        if (GoIsArm64Process)
            registerArm64Options();
        else
            registerX86Options();
    }

    // cpu_arm64.go doinit's option table.
    private static void registerArm64Options()
    {
        options = new option[]{
            new(Name: "aes"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasAES)),
            new(Name: "pmull"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasPMULL)),
            new(Name: "sha1"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA1)),
            new(Name: "sha2"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA2)),
            new(Name: "sha512"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA512)),
            new(Name: "sha3"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasSHA3)),
            new(Name: "crc32"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasCRC32)),
            new(Name: "atomics"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasATOMICS)),
            new(Name: "cpuid"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑHasCPUID)),
            new(Name: "isNeoverse"u8, Feature: ᏑARM64.of(ARM64ᴛ1.ᏑIsNeoverse))
        }.slice();
    }
}
