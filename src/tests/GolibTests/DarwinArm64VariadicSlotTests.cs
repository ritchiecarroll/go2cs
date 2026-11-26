// DarwinArm64VariadicSlotTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
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
    public void Syscall6StoresTheFourthArgument()
    {
        // syscall6 -> syscalln -> runtime·syscallN_trampoline with n == 6: "openat ... historically called with
        // syscall6", so a4 goes on the stack as well.
        Assert.AreEqual(3, GoLibcCall.VariadicStackSlot(6, GoLibcErrnoRule.Int32MinusOne, darwinArm64: true));
    }

    [TestMethod]
    public void Syscall9PassesItsNinthArgumentAsItself()
    {
        // Go 1.26: syscall9 (syscall_darwin.go) is syscalln(fn, a1..a9), and runtime·syscallN_trampoline copies the
        // arguments past the eighth onto the stack and stores a3/a4 only for n == 3, 4 and 6 -- so a9 travels at
        // [sp] as itself and nothing replaces it. (Go 1.24's runtime·syscall9 stored a4 there instead.)
        Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(9, GoLibcErrnoRule.Int32MinusOne, darwinArm64: true));
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
    public void TheKeventTrampolineStoresNothing()
    {
        // kevent_trampoline (six arguments, -1 becomes -errno) loads its registers from the args block and
        // calls libc_kevent directly: no [sp] store, unlike runtime·syscall6 at the same arity.
        Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(6, GoLibcErrnoRule.NegatedErrno, darwinArm64: true));
    }

    [TestMethod]
    public void NoOtherArityStoresAnything()
    {
        foreach (int arity in new[] { 0, 1, 2, 4, 5, 7, 8, 9 })
            Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(arity, GoLibcErrnoRule.Int32MinusOne, darwinArm64: true), $"arity {arity}");
    }

    [TestMethod]
    public void OffDarwinArm64NothingMoves()
    {
        // The linux and darwin/amd64 dispatch is untouched: x86-64 passes variadic arguments in registers.
        foreach (int arity in new[] { 3, 6, 9 })
            Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(arity, GoLibcErrnoRule.Int32MinusOne, darwinArm64: false), $"arity {arity}");

        foreach (int arity in new[] { 3, 4, 6 })
            Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(arity, GoLibcErrnoRule.AlwaysRead, darwinArm64: false), $"arity {arity}");
    }

    [TestMethod]
    public void TheGo126SyscallNEntryKeysOnTheArityAlone()
    {
        // Go 1.26 runtime·syscallN_trampoline (sys_darwin_arm64.s): n == 3 stores a3, n == 4 and n == 6 store a4.
        // A ninth argument already sits at [sp] as itself, so n == 9 stores nothing extra.
        Assert.AreEqual(2, GoLibcCall.VariadicStackSlot(3, GoLibcErrnoRule.AlwaysRead, darwinArm64: true));
        Assert.AreEqual(3, GoLibcCall.VariadicStackSlot(4, GoLibcErrnoRule.AlwaysRead, darwinArm64: true));
        Assert.AreEqual(3, GoLibcCall.VariadicStackSlot(6, GoLibcErrnoRule.AlwaysRead, darwinArm64: true));

        foreach (int arity in new[] { 0, 1, 2, 5, 7, 8, 9 })
            Assert.AreEqual(-1, GoLibcCall.VariadicStackSlot(arity, GoLibcErrnoRule.AlwaysRead, darwinArm64: true), $"arity {arity}");
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

    // ---- The live arms (2026-11-24, P5, COORD's probe after the darwin/arm64 sizing): the tests above pin the mapping as
    // a pure function and the padding's harmlessness on glibc; these call libSystem itself through GoLibcCall.Call on a
    // mac, so the stack store is measured where it matters. Each live arm has a near-miss: the same call with 0 in the
    // stack slot while the register still holds the real value. On Apple silicon the callee reads [sp], so the near-miss
    // reads 0; on Intel it reads the register, so both read the value. Go's own answer is the live arm's: its
    // runtime·syscallN_trampoline stores the word at [sp] as well (sys_darwin_arm64.s).

    private const nuint DarwinOWronly = 0x1, DarwinONonblock = 0x4, DarwinOCreat = 0x200, DarwinOExcl = 0x800;
    private const nuint DarwinFGetfl = 3, DarwinFSetfl = 4;
    private const UnixFileMode Mode0640 = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead;

    private static nint LibSystem(string symbol) => NativeLibrary.GetExport(NativeLibrary.Load("/usr/lib/libSystem.B.dylib"), symbol);

    private static bool AppleSilicon => RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

    // open(path, O_CREAT|O_EXCL|O_WRONLY, 0640) with the umask cleared, then close: the mode C created the file with.
    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    private static unsafe UnixFileMode CreateWithMode0640(string path, bool zeroInTheStackSlot)
    {
        nint open = LibSystem("open"), close = LibSystem("close"), umask = LibSystem("umask"), errnoReader = LibSystem("__error");
        byte[] name = Encoding.UTF8.GetBytes(path + "\0");
        nuint saved = GoLibcCall.Call(umask, [0], GoLibcErrnoRule.None, errnoReader, out _);

        try
        {
            fixed (byte* p = name)
            {
                nuint[] args = [(nuint)p, DarwinOCreat | DarwinOExcl | DarwinOWronly, 0x1A0 /* 0640 */];
                nuint errno = 0;
                nuint fd = zeroInTheStackSlot ?
                    GoLibcCall.CallWithVariadicStackSlot(open, args, 0) :
                    GoLibcCall.Call(open, args, GoLibcErrnoRule.Int32MinusOne, errnoReader, out errno);

                Assert.AreNotEqual(-1, unchecked((int)(uint)fd), $"open failed, errno {errno}");
                GoLibcCall.Call(close, [fd], GoLibcErrnoRule.Int32MinusOne, errnoReader, out _);
            }
        }
        finally
        {
            GoLibcCall.Call(umask, [saved], GoLibcErrnoRule.None, errnoReader, out _);
        }

        return File.GetUnixFileMode(path);
    }

    [TestMethod]
    public void LiveOpenCreatesTheFileWithTheVariadicMode()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive("calls libSystem's open; macOS only (the osx-arm64 leg measures the stack slot, osx-x64 the register)");
            return;
        }

        string dir = Directory.CreateTempSubdirectory("go2cs-variadic-").FullName;

        try
        {
            UnixFileMode live = CreateWithMode0640(Path.Combine(dir, "live"), zeroInTheStackSlot: false);
            UnixFileMode nearMiss = CreateWithMode0640(Path.Combine(dir, "near-miss"), zeroInTheStackSlot: true);
            Console.WriteLine($"{RuntimeInformation.ProcessArchitecture}: live {Convert.ToString((int)live, 8)}, near-miss (0 at [sp]) {Convert.ToString((int)nearMiss, 8)}");

            Assert.AreEqual(Mode0640, live, "open through GoLibcCall.Call must create the file 0640, as Go does");
            Assert.AreEqual(AppleSilicon ? UnixFileMode.None : Mode0640, nearMiss,
                AppleSilicon ? "on Apple silicon open must read its mode from [sp]" : "on Intel open must read its mode from the register");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [TestMethod]
    public void LiveFcntlSetsTheVariadicFlag()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive("calls libSystem's fcntl; macOS only (the osx-arm64 leg measures the stack slot, osx-x64 the register)");
            return;
        }

        nint fcntl = LibSystem("fcntl"), errnoReader = LibSystem("__error");
        string path = Path.GetTempFileName();

        try
        {
            using Microsoft.Win32.SafeHandles.SafeFileHandle handle = File.OpenHandle(path, FileMode.Open, FileAccess.ReadWrite);
            nuint fd = (nuint)handle.DangerousGetHandle();

            nuint Flags() => GoLibcCall.Call(fcntl, [fd, DarwinFGetfl, 0], GoLibcErrnoRule.Int32MinusOne, errnoReader, out _);

            nuint before = Flags();
            Assert.AreEqual((nuint)0, before & DarwinONonblock, "a fresh descriptor is blocking");

            // The near-miss: O_NONBLOCK in the register, 0 at [sp].
            GoLibcCall.CallWithVariadicStackSlot(fcntl, [fd, DarwinFSetfl, before | DarwinONonblock], before & ~DarwinONonblock);
            nuint nearMiss = Flags();

            // The live call: Go's F_SETFL, through the dispatcher.
            GoLibcCall.Call(fcntl, [fd, DarwinFSetfl, before | DarwinONonblock], GoLibcErrnoRule.Int32MinusOne, errnoReader, out nuint errno);
            nuint live = Flags();
            Console.WriteLine($"{RuntimeInformation.ProcessArchitecture}: before 0x{before:x}, near-miss 0x{nearMiss:x}, live 0x{live:x} (errno {errno})");

            Assert.AreEqual(DarwinONonblock, live & DarwinONonblock, "F_SETFL through GoLibcCall.Call must set O_NONBLOCK, as Go's syscall.SetNonblock does");
            Assert.AreEqual(AppleSilicon ? (nuint)0 : DarwinONonblock, nearMiss & DarwinONonblock,
                AppleSilicon ? "on Apple silicon fcntl must read its argument from [sp]" : "on Intel fcntl must read its argument from the register");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
