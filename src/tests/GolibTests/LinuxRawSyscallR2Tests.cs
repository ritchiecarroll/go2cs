// LinuxRawSyscallR2Tests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using syscall = go.syscall_package;

namespace GolibTests;

/// <summary>
/// Guards r2, the second result of the converted Linux raw syscall, against Go's own assembly. Go's
/// internal/runtime/syscall/linux.Syscall6 reports the second return register after the trap: DX on amd64, which
/// still holds a3 (asm_linux_amd64.s, <c>MOVQ DX, BX</c>), and R1 on arm64, which still holds a2
/// (asm_linux_arm64.s, <c>MOVD R1, r2+64(FP)</c>). The converted funnel binds glibc's syscall(2), which cannot hand
/// either register back, so it echoes the argument that register held; this asserts it echoes the RIGHT one on the
/// architecture the process runs as.
/// </summary>
/// <remarks>
/// getpid takes no arguments, so a1..a3 reach the kernel unused and come back unchanged in their registers. Each
/// argument is a distinct pattern with both 32-bit halves set, so an echo of the wrong one, or of a truncation, reads
/// as a different number. Compiled for $(GoTargetOS)=linux only (the csproj removes it elsewhere); on a non-linux
/// host running that flavour it reports Inconclusive rather than a vacuous pass.
/// </remarks>
[TestClass]
public class LinuxRawSyscallR2Tests
{
    private static readonly uintptr A1 = unchecked((nuint)0x1111_0000_1111UL);
    private static readonly uintptr A2 = unchecked((nuint)0x2222_0000_2222UL);
    private static readonly uintptr A3 = unchecked((nuint)0x3333_0000_3333UL);

    // Go's register reading, per architecture: the x86 family's second return register (DX/EDX) carries a3 through
    // the trap; every other Linux architecture Go supports returns its a2 register as r2.
    private static uintptr GoR2 =>
        RuntimeInformation.ProcessArchitecture is Architecture.X64 or Architecture.X86 ? A3 : A2;

    [TestMethod]
    public void RawSyscall6ReturnsTheRegisterGoReturnsAsR2()
    {
        if (!OperatingSystem.IsLinux())
            Assert.Inconclusive("r2 is the Linux kernel ABI's second return register; this host is not Linux");

        var (r1, r2, err) = syscall.RawSyscall6(syscall.SYS_GETPID, A1, A2, A3, 0, 0, 0);

        Assert.AreEqual(default(syscall.Errno), err, "getpid cannot fail");
        Assert.AreEqual((nuint)Environment.ProcessId, (nuint)r1, "r1 is getpid's result");
        Assert.AreEqual((nuint)GoR2, (nuint)r2,
            $"r2 on {RuntimeInformation.ProcessArchitecture}: Go returns 0x{(ulong)(nuint)GoR2:x}, the converted call returned 0x{(ulong)(nuint)r2:x}");
    }

    [TestMethod]
    public void RawSyscallReturnsTheRegisterGoReturnsAsR2()
    {
        if (!OperatingSystem.IsLinux())
            Assert.Inconclusive("r2 is the Linux kernel ABI's second return register; this host is not Linux");

        var (r1, r2, err) = syscall.RawSyscall(syscall.SYS_GETPID, A1, A2, A3);

        Assert.AreEqual(default(syscall.Errno), err, "getpid cannot fail");
        Assert.AreEqual((nuint)Environment.ProcessId, (nuint)r1, "r1 is getpid's result");
        Assert.AreEqual((nuint)GoR2, (nuint)r2,
            $"r2 on {RuntimeInformation.ProcessArchitecture}: Go returns 0x{(ulong)(nuint)GoR2:x}, the converted call returned 0x{(ulong)(nuint)r2:x}");
    }
}
