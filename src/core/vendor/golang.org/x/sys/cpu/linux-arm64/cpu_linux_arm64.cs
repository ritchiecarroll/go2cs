// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.sys;

using strings = strings_package;
using syscall = syscall_package;

partial class cpu_package {

// HWCAP/HWCAP2 bits. These are exposed by Linux.
internal static UntypedInt hwcap_FP => /* 1 << 0 */ 1;
internal static UntypedInt hwcap_ASIMD => /* 1 << 1 */ 2;
internal static UntypedInt hwcap_EVTSTRM => /* 1 << 2 */ 4;
internal static UntypedInt hwcap_AES => /* 1 << 3 */ 8;
internal static UntypedInt hwcap_PMULL => /* 1 << 4 */ 16;
internal static UntypedInt hwcap_SHA1 => /* 1 << 5 */ 32;
internal static UntypedInt hwcap_SHA2 => /* 1 << 6 */ 64;
internal static UntypedInt hwcap_CRC32 => /* 1 << 7 */ 128;
internal static UntypedInt hwcap_ATOMICS => /* 1 << 8 */ 256;
internal static UntypedInt hwcap_FPHP => /* 1 << 9 */ 512;
internal static UntypedInt hwcap_ASIMDHP => /* 1 << 10 */ 1024;
internal static UntypedInt hwcap_CPUID => /* 1 << 11 */ 2048;
internal static UntypedInt hwcap_ASIMDRDM => /* 1 << 12 */ 4096;
internal static UntypedInt hwcap_JSCVT => /* 1 << 13 */ 8192;
internal static UntypedInt hwcap_FCMA => /* 1 << 14 */ 16384;
internal static UntypedInt hwcap_LRCPC => /* 1 << 15 */ 32768;
internal static UntypedInt hwcap_DCPOP => /* 1 << 16 */ 65536;
internal static UntypedInt hwcap_SHA3 => /* 1 << 17 */ 131072;
internal static UntypedInt hwcap_SM3 => /* 1 << 18 */ 262144;
internal static UntypedInt hwcap_SM4 => /* 1 << 19 */ 524288;
internal static UntypedInt hwcap_ASIMDDP => /* 1 << 20 */ 1048576;
internal static UntypedInt hwcap_SHA512 => /* 1 << 21 */ 2097152;
internal static UntypedInt hwcap_SVE => /* 1 << 22 */ 4194304;
internal static UntypedInt hwcap_ASIMDFHM => /* 1 << 23 */ 8388608;
internal static UntypedInt hwcap_DIT => /* 1 << 24 */ 16777216;

internal static UntypedInt hwcap2_SVE2 => /* 1 << 1 */ 2;
internal static UntypedInt hwcap2_I8MM => /* 1 << 13 */ 8192;

// linuxKernelCanEmulateCPUID reports whether we're running
// on Linux 4.11+. Ideally we'd like to ask the question about
// whether the current kernel contains
// https://git.kernel.org/pub/scm/linux/kernel/git/torvalds/linux.git/commit/?id=77c97b4ee21290f5f083173d957843b615abbff2
// but the version number will have to do.
internal static bool linuxKernelCanEmulateCPUID() {
    ref var un = ref heap(new syscall.Utsname(), out var Ꮡun);
    syscall.Uname(Ꮡun);
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    foreach (var (_, b) in range(un.Release[..])) {
        if (b == 0) {
            break;
        }
        Ꮡsb.WriteByte((byte)b);
    }
    var (major, minor, _, ok) = parseRelease(sb.String());
    return ok && (major > 4 || major == 4 && minor >= 11);
}

internal static void doinit() {
    var err = readHWCAP(); if (err != nil) {
        // We failed to read /proc/self/auxv. This can happen if the binary has
        // been given extra capabilities(7) with /bin/setcap.
        //
        // When this happens, we have two options. If the Linux kernel is new
        // enough (4.11+), we can read the arm64 registers directly which'll
        // trap into the kernel and then return back to userspace.
        //
        // But on older kernels, such as Linux 4.4.180 as used on many Synology
        // devices, calling readARM64Registers (specifically getisar0) will
        // cause a SIGILL and we'll die. So for older kernels, parse /proc/cpuinfo
        // instead.
        //
        // See golang/go#57336.
        if (linuxKernelCanEmulateCPUID()) {
            readARM64Registers();
        } else {
            readLinuxProcCPUInfo();
        }
        return;
    }

    // HWCAP feature bits
    ARM64.HasFP = isSet(hwCap, hwcap_FP);
    ARM64.HasASIMD = isSet(hwCap, hwcap_ASIMD);
    ARM64.HasEVTSTRM = isSet(hwCap, hwcap_EVTSTRM);
    ARM64.HasAES = isSet(hwCap, hwcap_AES);
    ARM64.HasPMULL = isSet(hwCap, hwcap_PMULL);
    ARM64.HasSHA1 = isSet(hwCap, hwcap_SHA1);
    ARM64.HasSHA2 = isSet(hwCap, hwcap_SHA2);
    ARM64.HasCRC32 = isSet(hwCap, hwcap_CRC32);
    ARM64.HasATOMICS = isSet(hwCap, hwcap_ATOMICS);
    ARM64.HasFPHP = isSet(hwCap, hwcap_FPHP);
    ARM64.HasASIMDHP = isSet(hwCap, hwcap_ASIMDHP);
    ARM64.HasCPUID = isSet(hwCap, hwcap_CPUID);
    ARM64.HasASIMDRDM = isSet(hwCap, hwcap_ASIMDRDM);
    ARM64.HasJSCVT = isSet(hwCap, hwcap_JSCVT);
    ARM64.HasFCMA = isSet(hwCap, hwcap_FCMA);
    ARM64.HasLRCPC = isSet(hwCap, hwcap_LRCPC);
    ARM64.HasDCPOP = isSet(hwCap, hwcap_DCPOP);
    ARM64.HasSHA3 = isSet(hwCap, hwcap_SHA3);
    ARM64.HasSM3 = isSet(hwCap, hwcap_SM3);
    ARM64.HasSM4 = isSet(hwCap, hwcap_SM4);
    ARM64.HasASIMDDP = isSet(hwCap, hwcap_ASIMDDP);
    ARM64.HasSHA512 = isSet(hwCap, hwcap_SHA512);
    ARM64.HasSVE = isSet(hwCap, hwcap_SVE);
    ARM64.HasASIMDFHM = isSet(hwCap, hwcap_ASIMDFHM);
    ARM64.HasDIT = isSet(hwCap, hwcap_DIT);

    // HWCAP2 feature bits
    ARM64.HasSVE2 = isSet(hwCap2, hwcap2_SVE2);
    ARM64.HasI8MM = isSet(hwCap2, hwcap2_I8MM);
}

internal static bool isSet(nuint hwc, nuint value) {
    return (hwc & value) != 0;
}

} // end cpu_package
