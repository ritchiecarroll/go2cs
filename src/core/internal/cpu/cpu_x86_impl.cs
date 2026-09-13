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
// for schedinit's slot". So doinit() is unreachable, `cpuid()` (x86 assembly, PartialStubGenerator
// stub) never actually throws, and EVERY `X86.Has*` stayed at its `false` zero value process-wide.
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
//   MAPPED (15)   HasAES Aes · HasPCLMULQDQ Pclmulqdq · HasAVX Avx · HasAVX2 Avx2
//                 HasAVX512F Avx512F · HasAVX512BW Avx512BW · HasAVX512VL Avx512F.VL
//                 HasBMI1 Bmi1 · HasBMI2 Bmi2 · HasFMA Fma · HasPOPCNT Popcnt
//                 HasSSE3 Sse3 · HasSSE41 Sse41 · HasSSE42 Sse42 · HasSSSE3 Ssse3
//   UNMAPPED (5)  HasADX, HasERMS, HasRDTSCP, HasSHA — no System.Runtime.Intrinsics.X86 surface in
//                 .NET 10 (Sha does not exist; verified by compile error, not assumed) — and
//                 HasOSXSAVE, which is an OS-support PRECONDITION for AVX rather than a feature:
//                 the runtime has already applied it, since Avx.IsSupported is false without it.
//                 These stay FALSE, which is the conservative direction: a consumer gated on them
//                 takes the portable path, exactly as it does today.
//
// NOT getGOAMD64level. The header above is right and this does not disturb it: that answers the
// BUILD's microarchitecture level (constant 1), this answers the HOST's capabilities. Go keeps them
// separate too, which is why doinit leaves the sse3/avx/avx512 knobs switchable at level 1.
// ---------------------------------------------------------------------------------------------

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
        X86.HasAVX512BW = Avx512BW.IsSupported;
        X86.HasAVX512VL = Avx512F.VL.IsSupported;
        X86.HasBMI1 = Bmi1.IsSupported;
        X86.HasBMI2 = Bmi2.IsSupported;
        X86.HasFMA = Fma.IsSupported;
        X86.HasPOPCNT = Popcnt.IsSupported;
        X86.HasSSE3 = Sse3.IsSupported;
        X86.HasSSE41 = Sse41.IsSupported;
        X86.HasSSE42 = Sse42.IsSupported;
        X86.HasSSSE3 = Ssse3.IsSupported;
    }
}
