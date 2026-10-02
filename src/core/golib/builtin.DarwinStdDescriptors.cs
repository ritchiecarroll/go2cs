// builtin.DarwinStdDescriptors.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace go;

// ---------------------------------------------------------------------------------------------
// DARWIN STANDARD-DESCRIPTOR HYGIENE - the darwin arm of builtin.LinuxStdDescriptors.cs. Read
// that file's header first: the WHY, the managed-dawn timing contract and the two load-bearing
// selection conditions are the same here, and are not repeated.
//
// WHAT DIFFERS ON DARWIN
//   The linux sweep finds the runtime's startup duplicates by comparing /proc/self/fd link targets,
//   and darwin has no /proc. So aliases are found by IDENTITY instead: a descriptor above 2 aliases a
//   standard stream when fstat reports the same (st_dev, st_ino) pair as descriptor 0, 1 or 2, and
//   it is closed only if it also carries FD_CLOEXEC (no inherited descriptor can). The candidates
//   come from /dev/fd, which darwin's fdesc filesystem lists per process.
//
// WHY IT EXISTS - MEASURED, NOT ASSUMED
//   The pipe-EOF-barrier witness (behavioral StdoutCloseEofBarrier) hangs to the run timeout on BOTH
//   mac legs (run 36953433720, 300 s, osx-arm64 and osx-x64, 2026-10-02), exactly the deadlock the
//   linux sweep cured, while that sweep returns at once unless OperatingSystem.IsLinux().
//
// THE fstat LAYOUT
//   arm64 darwin exports one struct-stat layout, as `fstat`. x86_64 darwin's bare `fstat` is the
//   LEGACY 32-bit-inode layout, and the 64-bit-inode one is `fstat64` -- the same split Go's own
//   syscall links (zsyscall_darwin_amd64.go: libc_fstat64; zsyscall_darwin_arm64.go: libc_fstat).
//   Both 64-bit-inode layouts carry st_dev (int32) at offset 0 and st_ino (uint64) at offset 8, and
//   so does glibc's struct stat on x86_64 and aarch64 (st_dev is 64 bits there; its low 32 are read,
//   which identifies a device the same way within one process). That shared prefix is what lets the
//   selection run, and be tested, on linux (GolibTests DarwinStdDescriptorContractTests).
// ---------------------------------------------------------------------------------------------
public static partial class builtin
{
    private const int StatBufferSize = 256; // struct stat is 144 bytes on darwin, 144/128 on glibc

    [LibraryImport("libc", EntryPoint = "fstat")]
    private static partial int sys_fstat(int fd, Span<byte> buffer);

    [LibraryImport("libc", EntryPoint = "fstat64")]
    private static partial int sys_fstat64(int fd, Span<byte> buffer);

    // Called from InitializeGoLib beside the linux sweep, before anything can touch System.Console;
    // the timing contract is builtin.LinuxStdDescriptors.cs's.
    private static void InitializeDarwinStdDescriptorHygiene()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        try
        {
            foreach (int fd in FindStartupStdAliasesByIdentity())
                sys_close_fd(fd);
        }
        catch
        {
            // The linux sweep's posture: an unusual host keeps the runtime duplicates, and
            // standard-stream close semantics stay as they were. Never fail module initialization
            // for a parity measure.
        }
    }

    /// <summary>
    /// Returns every descriptor above 2 that aliases descriptor 0, 1 or 2 by fstat identity and
    /// carries FD_CLOEXEC. It closes nothing, so a caller that has already touched System.Console
    /// (a test host) can ask safely.
    /// </summary>
    internal static List<int> FindStartupStdAliasesByIdentity()
    {
        List<int> aliases = [];
        List<(int dev, ulong ino)> standard = [];

        for (int fd = 0; fd <= 2; fd++)
        {
            if (TryDescriptorIdentity(fd, out (int dev, ulong ino) identity))
                standard.Add(identity);
        }

        if (standard.Count == 0)
            return aliases;

        foreach (string entry in Directory.GetFiles("/dev/fd"))
        {
            if (!int.TryParse(Path.GetFileName(entry), out int fd) || fd <= 2)
                continue;

            // A descriptor that closed since the listing (the listing's own directory handle is
            // one) simply fails here and is skipped.
            if (!TryDescriptorIdentity(fd, out (int dev, ulong ino) identity) || !standard.Contains(identity))
                continue;

            int flags = sys_fcntl_getfd(fd, F_GETFD);

            if (flags < 0 || (flags & FD_CLOEXEC) == 0)
                continue;

            aliases.Add(fd);
        }

        return aliases;
    }

    /// <summary>
    /// Reads a descriptor's (st_dev, st_ino) through the platform's 64-bit-inode fstat.
    /// </summary>
    internal static bool TryDescriptorIdentity(int fd, out (int dev, ulong ino) identity)
    {
        Span<byte> buffer = stackalloc byte[StatBufferSize];
        buffer.Clear();

        bool legacyLayout = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.X64;
        int result = legacyLayout ? sys_fstat64(fd, buffer) : sys_fstat(fd, buffer);

        if (result != 0)
        {
            identity = default;
            return false;
        }

        identity = (BitConverter.ToInt32(buffer[..4]), BitConverter.ToUInt64(buffer.Slice(8, 8)));
        return true;
    }
}
