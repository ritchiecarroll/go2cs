// cpu_x86_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// getGOAMD64level answers the amd64 MICROARCHITECTURE LEVEL THE BINARY WAS BUILT FOR — not the
// level the host CPU happens to support. Go's cpu_x86.s is a compile-time constant selected by the
// GOAMD64_vN define the toolchain sets from `go env GOAMD64`, whose fall-through is $1:
//
//     TEXT ·getGOAMD64level(SB),NOSPLIT,$0-4
//     #ifdef GOAMD64_v4  $4  #else #ifdef GOAMD64_v3  $3  #else #ifdef GOAMD64_v2  $2  #else  $1
//
// go2cs emits portable C#: there is no GOAMD64 define, no microarchitecture-gated emission, and no
// instruction-set floor above the amd64 baseline. The faithful answer is therefore the same
// constant Go's own assembly produces for a build with no GOAMD64_vN define — 1 — and it is a
// measured property of the emission rather than a placeholder. Probing the CPU here would answer a
// DIFFERENT question and be wrong in Go's terms: a v3 machine running a v1 build still reports 1,
// which is precisely why doinit keeps the sse3/avx/avx512 GODEBUG knobs switchable at level 1.
//
// The converter drops the auto form of this declaration (manualConversionFuncs["internal/cpu"] in
// go2cs/manualTypeOperations.go), leaving a placeholder comment at the site.
//
// Demonstrated consumers: doinit's option table, whose `level < 2/3/4` gates decide which cpu.*
// options remain switchable; and internal/cpu's own TestDisableSSE3, whose first line is
// `if GetGOAMD64level() > 1 { t.Skip(…) }` — the unimplemented PartialStubGenerator stub turned
// that guard into an infrastructure-error where Go reads 1 and walks on to a matching skip.

// ---------------------------------------------------------------------------------------------
// X86 FEATURE DETECTION — and why it needs a module initializer rather than a cpuid body.
//
// Go fills `cpu.X86` in doinit(), which cpuinit() calls from schedinit(). schedinit is Go's
// SCHEDULER BOOTSTRAP and go2cs never runs it — the same fact goenvs_impl.cs and goargs_impl.cs
// are written around, each with the comment that a module initializer is "the faithful stand-in
// for schedinit's slot". So doinit() is unreachable and EVERY `X86.Has*` stayed at its `false` zero
// value process-wide. doinit's own cpuid calls therefore never run; the one live caller outside it is
// Go 1.26's DataCacheSizes (datacache_x86.go), so cpuid() below has a managed body rather than the
// PartialStubGenerator stub that throws.
//
// That is not cosmetic. `crypto/tls` picks its TLS 1.3 cipher-suite ORDER from
// `hasAESGCMHardwareSupport`, which is `cpu.X86.HasAES && cpu.X86.HasPCLMULQDQ` — so with every
// flag false the converted stack negotiated CHACHA20-POLY1305 where Go on the same host negotiates
// AES_128_GCM, and every AES-NI/PCLMULQDQ-gated path in the corpus took its software fallback.
// Measured by PerfTlsHandshake's Verify phase: Go suite 0x1301, converted 0x1303, same host, same
// pinned config.
//
// DETECTION SOURCE. `System.Runtime.Intrinsics.X86` answers the same question cpuid does, already
// normalised by the runtime (an intrinsic is `IsSupported` only when the CPU has it AND the JIT
// will emit it), which is the honest managed analogue: a flag claiming hardware the runtime will
// not use would be a lie in the direction that matters. Per-flag census against Go's own
// cpu_x86.go, taken on this host before writing this:
//
//   MAPPED (23)   HasAES Aes · HasPCLMULQDQ Pclmulqdq · HasAVX Avx · HasAVX2 Avx2
//                 HasAVX512F Avx512F · HasAVX512BW Avx512BW · HasAVX512VL Avx512F.VL
//                 HasBMI1 Bmi1 · HasBMI2 Bmi2 · HasFMA Fma · HasPOPCNT Popcnt
//                 HasSSE3 Sse3 · HasSSE41 Sse41 · HasSSE42 Sse42 · HasSSSE3 Ssse3
//                 and, from Go 1.26's wider set: HasAVX512CD Avx512CD · HasAVX512DQ Avx512DQ
//                 HasAVX512VBMI Avx512Vbmi · HasAVX512VBMI2 Avx512Vbmi2 · HasAVX512GFNI Gfni.V512
//                 HasAVX512VPCLMULQDQ Pclmulqdq.V512 · HasAVXVNNI AvxVnni · HasGFNI Gfni;
//                 HasAVX512 is VIRTUAL (F+CD+BW+DQ+VL), derived after GODEBUG as Go's doDerived does.
//   UNMAPPED (10) HasADX, HasERMS, HasRDTSCP, HasSHA, and Go 1.26's HasVAES, HasAVX512VAES,
//                 HasAVX512VNNI, HasAVX512BITALG, HasAVX512VPOPCNTDQ — no System.Runtime.Intrinsics.X86
//                 surface in .NET 10 (Sha, Aes.V256/V512, Avx512Vnni, Avx512Bitalg and Avx512Vpopcntdq
//                 do not exist; verified by compile error, not assumed) — and HasOSXSAVE, which is
//                 an OS-support PRECONDITION for AVX rather than a feature: the runtime has already
//                 applied it, since Avx.IsSupported is false without it. These stay FALSE, which is
//                 the conservative direction: a consumer gated on them takes the portable path.
//   (HasFSRM is also unmapped and false, as before.)
//
// NOT getGOAMD64level. The header above is right and this does not disturb it: that answers the
// BUILD's microarchitecture level (constant 1), this answers the HOST's capabilities. Go keeps them
// separate too, which is why doinit leaves the sse3/avx/avx512 knobs switchable at level 1.
// ---------------------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using go;

[module: go.GoManualConversion]

namespace go.@internal;

partial class cpu_package
{
    internal static int32 getGOAMD64level()
    {
        return 1;
    }

    /// <summary>
    /// The managed body of Go's <c>cpuid</c> (cpu_x86.s): the four registers the CPUID instruction
    /// returns for leaf <paramref name="eaxArg"/> and sub-leaf <paramref name="ecxArg"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="X86Base.CpuId(int, int)"/> executes the instruction itself, so the answers are the
    /// host's own, as in Go. Go builds this file only for 386 and amd64; on a CLR host where
    /// <see cref="X86Base.IsSupported"/> is false (Arm64, for instance) there is no instruction to
    /// run, and every leaf reads as all zeros, the answer a CPU gives for an unsupported leaf.
    /// </remarks>
    internal static partial (uint32 eax, uint32 ebx, uint32 ecx, uint32 edx) cpuid(uint32 eaxArg, uint32 ecxArg)
    {
        if (!X86Base.IsSupported)
            return (0, 0, 0, 0);

        (int eax, int ebx, int ecx, int edx) = X86Base.CpuId(unchecked((int)eaxArg), unchecked((int)ecxArg));

        return (unchecked((uint32)eax), unchecked((uint32)ebx), unchecked((uint32)ecx), unchecked((uint32)edx));
    }

    /// <summary>
    /// Fills <see cref="X86"/> from the runtime's own instruction-set support, standing in for the
    /// <c>schedinit</c> slot where Go calls <c>cpuinit</c> → <c>Initialize</c> → <c>doinit</c>.
    /// </summary>
    /// <remarks>
    /// A module initializer is the same stand-in <c>goenvs_impl.cs</c> and <c>goargs_impl.cs</c> use
    /// for the same reason, and it runs before any managed code in this assembly can read a flag —
    /// including a consumer in another assembly, whose first touch of <c>cpu_package</c> triggers
    /// this load. Only the flags with a real CLR equivalent are set; the rest keep the zero value,
    /// which is the same answer they give today.
    /// </remarks>
    [ModuleInitializer]
    internal static void initX86FeatureDetection()
    {
        if (!X86Base.IsSupported)
            return;

        X86.HasAES = Aes.IsSupported;
        X86.HasPCLMULQDQ = Pclmulqdq.IsSupported;
        X86.HasAVX = Avx.IsSupported;
        X86.HasAVX2 = Avx2.IsSupported;
        X86.HasAVX512F = Avx512F.IsSupported;
        X86.HasAVX512CD = Avx512CD.IsSupported;
        X86.HasAVX512BW = Avx512BW.IsSupported;
        X86.HasAVX512DQ = Avx512DQ.IsSupported;
        X86.HasAVX512VL = Avx512F.VL.IsSupported;
        X86.HasAVX512VBMI = Avx512Vbmi.IsSupported;
        X86.HasAVX512VBMI2 = Avx512Vbmi2.IsSupported;
        X86.HasAVX512GFNI = Gfni.V512.IsSupported;
        X86.HasAVX512VPCLMULQDQ = Pclmulqdq.V512.IsSupported;
        X86.HasAVXVNNI = AvxVnni.IsSupported;
        X86.HasGFNI = Gfni.IsSupported;
        X86.HasBMI1 = Bmi1.IsSupported;
        X86.HasBMI2 = Bmi2.IsSupported;
        X86.HasFMA = Fma.IsSupported;
        X86.HasPOPCNT = Popcnt.IsSupported;
        X86.HasSSE3 = Sse3.IsSupported;
        X86.HasSSE41 = Sse41.IsSupported;
        X86.HasSSE42 = Sse42.IsSupported;
        X86.HasSSSE3 = Ssse3.IsSupported;

        // Go's Initialize(env) runs doinit, which fills the option table on every OS, and then
        // processOptions(env). The env is runtime.getGodebugEarly's answer, which is GODEBUG only on
        // the Unix-like systems (aix, darwin, ios, dragonfly, freebsd, netbsd, openbsd, illumos,
        // solaris, linux) and "" everywhere else, Windows included. So off Windows, GODEBUG's cpu.*
        // options (cpu.aes=off, cpu.all=off, ...) turn features off before any consumer reads them:
        // the runtime's alginit picks its hash from these flags, and TestMemHashGlobalSeed/noaes and
        // TestIssue66841 re-exec with GODEBUG=cpu.aes=off to reach the fallback there. On Windows Go
        // ignores cpu.* entirely, and so does this.
        registerX86Options();

        if (GoCpuAppliesGodebug)
            processOptions(Environment.GetEnvironmentVariable("GODEBUG") ?? "");

        // Go 1.26's Initialize runs doDerived after processOptions, so a derived flag reflects the
        // options GODEBUG turned off (cpu.avx512bw=off clears the virtual HasAVX512 too).
        doDerived = deriveX86Features;
        doDerived();
    }

    // cpu_x86.go doinit's doDerived (Go 1.26): the VIRTUAL "AVX512" feature, F+CD+BW+DQ+VL, the set
    // GOAMD64=v4 implies and AVX10.1 includes.
    private static void deriveX86Features()
    {
        X86.HasAVX512 = X86.HasAVX512F && X86.HasAVX512CD && X86.HasAVX512BW && X86.HasAVX512DQ && X86.HasAVX512VL;
    }

    /// <summary>
    /// Whether this process applies GODEBUG's cpu.* options at start-up: runtime.getGodebugEarly
    /// answers GODEBUG only on the Unix-like systems, so everywhere but Windows among the corpus's
    /// targets.
    /// </summary>
    public static bool GoCpuAppliesGodebug => !OperatingSystem.IsWindows();

    /// <summary>
    /// GolibTests' probe (InternalCpuGodebugTests): applies <paramref name="godebug"/> through
    /// processOptions over doinit's option table exactly as start-up does off Windows, returns each
    /// option's name and resulting flag, then restores the flags and the table.
    /// </summary>
    public static (string name, bool enabled)[] GoCpuOptionsProbe(string godebug)
    {
        X86ᴛ1 saved = X86;

        try
        {
            registerX86Options();
            processOptions(godebug);

            (string, bool)[] result = new (string, bool)[len(options)];

            for (int i = 0; i < result.Length; i++)
                result[i] = (options[i].Name.ToString(), options[i].Feature.Value);

            return result;
        }
        finally
        {
            X86 = saved;
            registerX86Options();
        }
    }

    // cpu_x86.go doinit's option table, gated on getGOAMD64level exactly as doinit gates it.
    private static void registerX86Options()
    {
        options = new option[]{
            new(Name: "adx"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasADX)),
            new(Name: "aes"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAES)),
            new(Name: "erms"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasERMS)),
            new(Name: "fsrm"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasFSRM)),
            new(Name: "pclmulqdq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasPCLMULQDQ)),
            new(Name: "rdtscp"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasRDTSCP)),
            new(Name: "sha"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSHA)),
            new(Name: "vpclmulqdq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VPCLMULQDQ))
        }.slice();

        var level = getGOAMD64level();

        if (level < 2)
        {
            options = append(options,
                new option(Name: "popcnt"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasPOPCNT)),
                new option(Name: "sse3"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE3)),
                new option(Name: "sse41"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE41)),
                new option(Name: "sse42"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSE42)),
                new option(Name: "ssse3"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasSSSE3)));
        }

        if (level < 3)
        {
            options = append(options,
                new option(Name: "avx"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX)),
                new option(Name: "avx2"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX2)),
                new option(Name: "bmi1"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasBMI1)),
                new option(Name: "bmi2"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasBMI2)),
                new option(Name: "fma"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasFMA)));
        }

        if (level < 4)
        {
            options = append(options,
                new option(Name: "avx512f"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512F)),
                new option(Name: "avx512cd"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512CD)),
                new option(Name: "avx512bw"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512BW)),
                new option(Name: "avx512dq"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512DQ)),
                new option(Name: "avx512vl"u8, Feature: ᏑX86.of(X86ᴛ1.ᏑHasAVX512VL)));
        }
    }
}
