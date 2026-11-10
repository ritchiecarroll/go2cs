// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cpu = @internal.cpu_package;
using @internal;

partial class runtime_package {

internal static uint8 memmoveBits;

// avxSupported indicates that the CPU supports AVX instructions.
internal static UntypedInt avxSupported => /* 1 << 0 */ 1;

// repmovsPreferred indicates that REP MOVSx instruction is more
// efficient on the CPU.
internal static UntypedInt repmovsPreferred => /* 1 << 1 */ 2;

/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ internal static void initΔ1() {
    // Here we assume that on modern CPUs with both FSRM and ERMS features,
    // copying data blocks of 2KB or larger using the REP MOVSB instruction
    // will be more efficient to avoid having to keep up with CPU generations.
    // Therefore, we may retain a BlockList mechanism to ensure that microarchitectures
    // that do not fit this case may appear in the future.
    // We enable it on Intel CPUs first, and we may support more platforms
    // in the future.
    var isERMSNiceCPU = isIntel;
    var useREPMOV = isERMSNiceCPU && cpu.X86.HasERMS && cpu.X86.HasFSRM;
    if (cpu.X86.HasAVX) {
        memmoveBits |= (uint8)(avxSupported);
    }
    if (useREPMOV) {
        memmoveBits |= (uint8)(repmovsPreferred);
    }
}

} // end runtime_package
