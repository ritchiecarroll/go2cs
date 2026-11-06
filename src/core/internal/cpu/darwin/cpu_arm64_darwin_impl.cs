// cpu_arm64_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// darwin's osInit for an arm64 process: cpu_arm64_darwin.go, which the darwin/amd64 conversion does not
// build. cpu_arm64_impl.cs calls it, and only on an arm64 process; it explains why the corpus needs it.
//
// The sysctl reads go through the converted sysctlEnabled (cpu_darwin.cs) and cpu_darwin_impl.cs's
// sysctlbynameInt32, the same libSystem call Go's runtime push makes. AES, PMULL, SHA1 and SHA2 are set
// unconditionally, as Go sets them ("Assume the CPU features that Apple Silicon M1 supports to be
// available on all future iterations").
//
// ONE DEPARTURE FROM GO, RULED (COORD, 2026-11-06, the AES trio seat's Q2): HasDIT stays FALSE. Go reads
// it from hw.optional.arm.FEAT_DIT, and Apple silicon reports it. But a capability flag must describe what
// the CONVERTED runtime can do, not the silicon: crypto/subtle.WithDataIndependentTiming enables DIT by
// setting PSTATE.DIT, and the .NET runtime exposes no control for PSTATE.DIT
// (internal/runtime/sys/no_dit.cs: DITSupported is false). HasDIT=true would let the DIT tests, and any
// program that reads the flag, believe DIT had been enabled when it had not: a silent false security
// claim. So the three crypto/subtle DIT tests keep skipping, under a darwin platform-skip disclosure
// naming that cause (crypto/subtle/go2cs_test_disclosures.json), and the BOARD row retires when .NET
// gains an API for PSTATE.DIT.
[module: go.GoManualConversion]

namespace go.@internal;

partial class cpu_package {

static partial void osInitArm64() {
    // macOS 12 moved these to the hw.optional.arm tree, but as of Go 1.24 we
    // still support macOS 11. See [Determine Encryption Capabilities].
    ARM64.HasATOMICS = sysctlEnabled(slice<byte>("hw.optional.armv8_1_atomics\x00"u8));
    ARM64.HasCRC32 = sysctlEnabled(slice<byte>("hw.optional.armv8_crc32\x00"u8));
    ARM64.HasSHA512 = sysctlEnabled(slice<byte>("hw.optional.armv8_2_sha512\x00"u8));
    ARM64.HasSHA3 = sysctlEnabled(slice<byte>("hw.optional.armv8_2_sha3\x00"u8));

    // ARM64.HasDIT stays false: the header's ruled departure.

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
