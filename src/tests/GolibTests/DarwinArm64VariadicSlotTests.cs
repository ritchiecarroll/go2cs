// DarwinArm64VariadicSlotTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using System.Text;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// Pins where golib's libc dispatch places the variadic argument on darwin/arm64, mirroring Go's own
/// runtime entries in sys_darwin_arm64.s exactly, and proves the extra stack argument is harmless to a
/// callee that is not variadic.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why.</b> Apple silicon passes a variadic callee's variadic arguments on the STACK, not in x0-x7.
/// open(path, flags, ...), fcntl(fd, cmd, ...), ioctl(fd, req, ...) and openat(dirfd, path, flags, ...)
/// are variadic in libSystem, so a mode or flag argument passed only in a register is read from
/// whatever the stack holds: on osx-arm64 a file os.WriteFile created with 0o644 reopened as
/// "permission denied" (run 36976772120). Go's runtime stores the argument at [sp] as well, in two
/// shapes: runtime·syscall stores a3 (open, fcntl, ioctl); runtime·syscall6 and runtime·syscall9 store
/// a4 (openat). syscallX, syscallPtr and syscall6X store nothing.
/// </para>
/// <para>
/// The mapping is pure and is tested on every host. The call shape (eight register slots, then the
/// variadic value as the ninth argument, which AAPCS64 places at [sp]) can only place a stack slot on
/// arm64 itself. What this host CAN prove is the other half: the extra arguments are harmless to a
/// non-variadic callee, driven through glibc on linux, the arrangement LibcCallDispatchTests uses.
/// </para>
/// </remarks>
[TestClass]
public class DarwinArm64VariadicSlotTests
{
    [TestMethod]
    public void SyscallStoresTheThirdArgument()
    {
        // runtime·syscall: "put the 3rd arg on the stack as well" (open, fcntl, ioctl).
        Assert.AreEqual(2, GoLibcCall.VariadicStackSlot(3, GoLibcErrnoRule.Int32MinusOne, darwinArm64: true));
    }

    [TestMethod]
    public void Syscall6AndSyscall9StoreTheFourthArgument()
    {
        // runtime·syscall6 / runtime·syscall9: "openat, for which the 4th arg must be on the stack".
        Assert.AreEqual(3, GoLibcCall.VariadicStackSlot(6, GoLibcErrnoRule.Int32MinusOne, darwinArm64: true));
        Assert.AreEqual(3, GoLibcCall.VariadicStackSlot(9, GoLibcErrnoRule.Int32MinusOne, darwinArm64: true));
    }

    [TestMethod]
    public void TheXAndPtrEntriesStoreNothing()
    {
        // syscallX (3, all 64 bits), syscall6X (6, all 64 bits), syscallPtr (3, NULL is the error).
        Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(3, GoLibcErrnoRule.Int64MinusOne, darwinArm64: true));
        Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(6, GoLibcErrnoRule.Int64MinusOne, darwinArm64: true));
        Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(3, GoLibcErrnoRule.NullPointer, darwinArm64: true));
    }

    [TestMethod]
    public void NoOtherArityStoresAnything()
    {
        foreach (int arity in new[] { 0, 1, 2, 4, 5, 7, 8 })
            Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(arity, GoLibcErrnoRule.Int32MinusOne, darwinArm64: true), $"arity {arity}");
    }

    [TestMethod]
    public void OffDarwinArm64NothingMoves()
    {
        // The linux and darwin/amd64 dispatch is untouched: x86-64 passes variadic arguments in registers.
        foreach (int arity in new[] { 3, 6, 9 })
            Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(arity, GoLibcErrnoRule.Int32MinusOne, darwinArm64: false), $"arity {arity}");
    }

    [TestMethod]
    public unsafe void TheStackSlotShapeIsHarmlessToANonVariadicCallee()
    {
        if (!OperatingSystem.IsLinux())
            Assert.Inconclusive("drives glibc's strtol; linux-only");

        nint libc = NativeLibrary.Load("libc.so.6");
        nint strtol = NativeLibrary.GetExport(libc, "strtol");
        byte[] digits = Encoding.ASCII.GetBytes("12345\0");

        fixed (byte* p = digits)
        {
            // strtol(str, endptr, base): three named arguments, then the variadic-slot shape's padding and
            // the duplicated third argument. A non-variadic callee must read only its own three.
            nuint[] args = [(nuint)p, 0, 10];
            nuint r = GoLibcCall.CallWithVariadicStackSlot(strtol, args, args[2]);

            Assert.AreEqual((nuint)12345, r);
        }
    }
}
