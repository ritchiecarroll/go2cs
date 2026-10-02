// DarwinStdDescriptorContractTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// Drives the DARWIN standard-descriptor sweep's selection (golib builtin.DarwinStdDescriptors.cs)
/// on linux, against real descriptors this test creates: an FD_CLOEXEC duplicate of fd 1 must be
/// selected, while a plain duplicate of fd 1 (no FD_CLOEXEC, the shape of an inherited descriptor),
/// an unrelated close-on-exec pipe, and fds 0-2 themselves must not be.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this runs on linux.</b> Nothing on the fleet runs a darwin GolibTests host, so a
/// darwin-gated test here would never execute anywhere (DarwinSigmaskContractTests' reasoning).
/// The selection reads fstat's (st_dev, st_ino) prefix, which glibc's struct stat shares with
/// darwin's 64-bit-inode layout, so the same code path is exercised here end to end. The darwin
/// acceptance is the hosted behavioral witness StdoutCloseEofBarrier on both mac legs.
/// </para>
/// <para>
/// <b>What it does not prove</b>: that darwin's fstat64/fstat symbols resolve, and that the .NET
/// runtime's startup duplicates on macOS carry FD_CLOEXEC as they do on linux. Both are read by
/// the darwin witness, never here. The finder closes nothing, so asking from a test host that has
/// already touched System.Console is safe.
/// </para>
/// </remarks>
[TestClass]
public class DarwinStdDescriptorContractTests
{
    private const int F_SETFD = 2;
    private const int FD_CLOEXEC = 1;

    [DllImport("libc", SetLastError = true)]
    private static extern int dup(int fd);

    [DllImport("libc", SetLastError = true)]
    private static extern int fcntl(int fd, int cmd, int arg);

    [DllImport("libc", SetLastError = true)]
    private static extern int pipe(int[] fds);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);

    [TestMethod]
    public void CloseOnExecDuplicateOfStdoutIsSelected_PlainDuplicateAndUnrelatedAreNot()
    {
        int cloexecDup = dup(1);
        int plainDup = dup(1);
        int[] unrelated = new int[2];

        Assert.IsTrue(cloexecDup > 2 && plainDup > 2, $"dup(1) failed: {cloexecDup}, {plainDup} (errno {Marshal.GetLastPInvokeError()})");
        Assert.AreEqual(0, pipe(unrelated), $"pipe failed (errno {Marshal.GetLastPInvokeError()})");

        try
        {
            Assert.AreEqual(0, fcntl(cloexecDup, F_SETFD, FD_CLOEXEC));
            Assert.AreEqual(0, fcntl(unrelated[0], F_SETFD, FD_CLOEXEC));
            Assert.AreEqual(0, fcntl(unrelated[1], F_SETFD, FD_CLOEXEC));

            Assert.IsTrue(builtin.TryDescriptorIdentity(1, out var stdout), "fstat(1) failed");
            Assert.IsTrue(builtin.TryDescriptorIdentity(cloexecDup, out var dupIdentity), "fstat(dup) failed");
            Assert.AreEqual(stdout, dupIdentity, "a duplicate must share its original's (st_dev, st_ino)");
            Assert.IsTrue(builtin.TryDescriptorIdentity(unrelated[0], out var pipeIdentity), "fstat(pipe) failed");
            Assert.AreNotEqual(stdout, pipeIdentity, "a fresh pipe must not share stdout's identity");

            var selected = builtin.FindStartupStdAliasesByIdentity();

            CollectionAssert.Contains(selected, cloexecDup, "the FD_CLOEXEC alias of fd 1 must be selected");
            CollectionAssert.DoesNotContain(selected, plainDup, "an alias WITHOUT FD_CLOEXEC is an inherited descriptor and must survive");
            CollectionAssert.DoesNotContain(selected, unrelated[0], "a close-on-exec descriptor that aliases no standard stream must survive");
            CollectionAssert.DoesNotContain(selected, unrelated[1], "a close-on-exec descriptor that aliases no standard stream must survive");
            Assert.IsFalse(selected.Any(fd => fd <= 2), "fds 0-2 are never candidates");
        }
        finally
        {
            close(cloexecDup);
            close(plainDup);
            close(unrelated[0]);
            close(unrelated[1]);
        }
    }

    [TestMethod]
    public void IdentityOfAClosedDescriptorIsRefused()
    {
        int fd = dup(1);

        Assert.IsTrue(fd > 2);
        Assert.AreEqual(0, close(fd));
        Assert.IsFalse(builtin.TryDescriptorIdentity(fd, out _), "fstat on a closed descriptor must report failure, never a stale identity");
    }
}
