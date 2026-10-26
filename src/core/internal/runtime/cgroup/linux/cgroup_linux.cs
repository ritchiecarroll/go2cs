// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using linux = go.@internal.runtime.syscall.linux_package;
using go.@internal.runtime.syscall;

partial class cgroup_package {

// Include explicit NUL to be sure we include it in the slice.
internal static readonly @string v2MaxFile = "/cpu.max\x00"u8;
internal static readonly @string v1QuotaFile = "/cpu.cfs_quota_us\x00"u8;
internal static readonly @string v1PeriodFile = "/cpu.cfs_period_us\x00"u8;

// CPU owns the FDs required to read the CPU limit from a cgroup.
partial struct CPU {
    internal Version version;
    // For cgroup v1, this is cpu.cfs_quota_us.
    // For cgroup v2, this is cpu.max.
    internal nint quotaFD;
    // For cgroup v1, this is cpu.cfs_period_us.
    // For cgroup v2, this is unused.
    internal nint periodFD;
}

public static void Close(this CPU c) {
    var exprᴛ1 = c.version;
    if (exprᴛ1 == V1) {
        linux.Close(c.quotaFD);
        linux.Close(c.periodFD);
    }
    else if (exprᴛ1 == V2) {
        linux.Close(c.quotaFD);
    }
    else { /* default: */
        @throw(impossibleCgroupVersionˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeCgroupInvalidˢ = "runtime: cgroup invalid buffer length"u8;

internal static void checkBufferSize(slice<byte> s, nint size) {
    if (len(s) != size) {
        println((@string)"runtime: cgroup buffer length"u8, len(s), (@string)"want"u8, size);
        @throw(runtimeCgroupInvalidˢ);
    }
}

// OpenCPU returns a CPU for the CPU cgroup containing the current process, or
// ErrNoCgroup if the process is not in a CPU cgroup.
//
// scratch must have length ScratchSize.
public static (CPU, error) OpenCPU(slice<byte> scratch) {
    checkBufferSize(scratch, ScratchSize);

    var @base = scratch[..(int)(PathSize)];
    var scratch2 = scratch[(int)(PathSize)..];

    var (n, version, err) = FindCPU(@base, scratch2);
    if (err != default!) {
        return (new CPU(nil), err);
    }

    var exprᴛ1 = version;
    if (exprᴛ1 == (Version)(1)) {
        nint n2 = copy(@base.slice(n), v1QuotaFile);
        var path = @base.slice(0, n + n2);
        var (quotaFD, errno) = linux.Open(Ꮡ(path, 0), (nint)((nint)linux.O_RDONLY | (nint)linux.O_CLOEXEC), 0);
        if (errno != 0) {
            // This may fail if this process was migrated out of
            // the cgroup found by FindCPU and that cgroup has been
            // deleted.
            return (new CPU(nil), errSyscallFailed);
        }

        n2 = copy(@base.slice(n), v1PeriodFile);
        path = @base.slice(0, n + n2);
        (var periodFD, errno) = linux.Open(Ꮡ(path, 0), (nint)((nint)linux.O_RDONLY | (nint)linux.O_CLOEXEC), 0);
        if (errno != 0) {
            // This may fail if this process was migrated out of
            // the cgroup found by FindCPU and that cgroup has been
            // deleted.
            return (new CPU(nil), errSyscallFailed);
        }

        var c = new CPU(
            version: 1,
            quotaFD: quotaFD,
            periodFD: periodFD
        );
        return (c, default!);
    }
    if (exprᴛ1 == (Version)(2)) {
        nint n2 = copy(@base.slice(n), v2MaxFile);
        var path = @base.slice(0, n + n2);
        var (maxFD, errno) = linux.Open(Ꮡ(path, 0), (nint)((nint)linux.O_RDONLY | (nint)linux.O_CLOEXEC), 0);
        if (errno != 0) {
            // This may fail if this process was migrated out of
            // the cgroup found by FindCPU and that cgroup has been
            // deleted.
            return (new CPU(nil), errSyscallFailed);
        }

        var c = new CPU(
            version: 2,
            quotaFD: maxFD,
            periodFD: -1
        );
        return (c, default!);
    }
    { /* default: */
        @throw(impossibleCgroupVersionˢ);
        throw panic("unreachable");
    }
}

// Returns average CPU throughput limit from the cgroup, or ok false if there
// is no limit.
public static (float64, bool, error) ReadCPULimit(CPU c) {
    var exprᴛ1 = c.version;
    if (exprᴛ1 == (Version)(1)) {
        var (quota, err) = readV1Number(c.quotaFD);
        if (err != default!) {
            return (0D, false, errMalformedFile);
        }

        if (quota < 0) {
            // No limit.
            return (0D, false, default!);
        }

        (var period, err) = readV1Number(c.periodFD);
        if (err != default!) {
            return (0D, false, errMalformedFile);
        }

        return ((float64)quota / (float64)period, true, default!);
    }
    if (exprᴛ1 == (Version)(2)) {
        // quotaFD is the cpu.max FD.
        return readV2Limit(c.quotaFD);
    }
    { /* default: */
        @throw(impossibleCgroupVersionˢ);
        throw panic("unreachable");
    }
}

// Returns the value from the quota/period file.
internal static (int64, error) readV1Number(nint fd) {
    // The format of the file is "<value>\n" where the value is in
    // int64 microseconds and, if quota, may be -1 to indicate no limit.
    //
    // MaxInt64 requires 19 bytes to display in base 10, thus the
    // conservative max size of this file is 19 + 1 (newline) = 20 bytes.
    // We'll provide a bit more for good measure.
    //
    // Always read from the beginning of the file to get a fresh value.
    array<byte> b = new(64);
    var (n, errno) = linux.Pread(fd, b[..], 0);
    if (errno != 0) {
        return (0, errSyscallFailed);
    }
    if (n == len(b)) {
        return (0, errMalformedFile);
    }

    var buf = b.slice(0, n);
    return parseV1Number(buf);
}

// Returns CPU throughput limit, or ok false if there is no limit.
internal static (float64, bool, error) readV2Limit(nint fd) {
    // The format of the file is "<quota> <period>\n" where quota and
    // period are microseconds and quota may be "max" to indicate no limit.
    //
    // Note that the kernel is inconsistent about whether the values are
    // uint64 or int64: values are parsed as uint64 but printed as int64.
    // See kernel/sched/core.c:cpu_max_{show,write}.
    //
    // In practice, the kernel limits the period to 1s (1000000us) (see
    // max_cfs_quota_period), and the quota to (1<<44)us (see
    // max_cfs_runtime), so these values can't get large enough for the
    // distinction to matter.
    //
    // MaxInt64 requires 19 bytes to display in base 10, thus the
    // conservative max size of this file is 19 + 19 + 1 (space) + 1
    // (newline) = 40 bytes. We'll provide a bit more for good measure.
    //
    // Always read from the beginning of the file to get a fresh value.
    array<byte> b = new(64);
    var (n, errno) = linux.Pread(fd, b[..], 0);
    if (errno != 0) {
        return (0D, false, errSyscallFailed);
    }
    if (n == len(b)) {
        return (0D, false, errMalformedFile);
    }

    var buf = b.slice(0, n);
    return parseV2Limit(buf);
}

// FindCPU finds the path to the CPU cgroup that this process is a member of
// and places it in out. scratch is a scratch buffer for internal use.
//
// out must have length PathSize. scratch must have length ParseSize.
//
// Returns the number of bytes written to out and the cgroup version (1 or 2).
//
// Returns ErrNoCgroup if the process is not in a CPU cgroup.
public static (nint, Version, error) FindCPU(slice<byte> @out, slice<byte> scratch) {
    checkBufferSize(@out, PathSize);
    checkBufferSize(scratch, ParseSize);

    // The cgroup path is <cgroup mount point> + <relative path>.
    // relative path is the cgroup relative to the mount root.

    var (n, version, err) = FindCPUCgroup(@out, scratch);
    if (err != default!) {
        return (0, 0, err);
    }

    (n, err) = FindCPUMountPoint(@out, @out.slice(0, n), version, scratch);
    return (n, version, err);
}

// FindCPUCgroup finds the path to the CPU cgroup that this process is a member of
// and places it in out. scratch is a scratch buffer for internal use.
//
// out must have length PathSize. scratch must have length ParseSize.
//
// Returns the number of bytes written to out and the cgroup version (1 or 2).
//
// Returns ErrNoCgroup if the process is not in a CPU cgroup.
public static (nint, Version, error) FindCPUCgroup(slice<byte> @out, slice<byte> scratch) {
    var path = slice<byte>("/proc/self/cgroup\x00"u8);
    var (fd, errno) = linux.Open(Ꮡ(path, 0), (nint)((nint)linux.O_RDONLY | (nint)linux.O_CLOEXEC), 0);
    if (errno == linux.ENOENT){
        return (0, 0, ErrNoCgroup);
    } else 
    if (errno != 0) {
        return (0, 0, errSyscallFailed);
    }

    // The relative path always starts with /, so we can directly append it
    // to the mount point.
    var (n, version, err) = parseCPUCgroup(fd, linux.Read, @out[..], scratch);
    if (err != default!) {
        linux.Close(fd);
        return (0, 0, err);
    }

    linux.Close(fd);
    return (n, version, default!);
}

// FindCPUMountPoint finds the mount point containing the specified cgroup and
// version with cpu controller, and compose the full path to the cgroup in out.
// scratch is a scratch buffer for internal use.
//
// out must have length PathSize, may overlap with cgroup.
// scratch must have length ParseSize.
//
// Returns the number of bytes written to out.
//
// Returns ErrNoCgroup if no matching mount point is found.
public static (nint, error) FindCPUMountPoint(slice<byte> @out, slice<byte> cgroup, Version version, slice<byte> scratch) {
    checkBufferSize(@out, PathSize);
    checkBufferSize(scratch, ParseSize);

    var path = slice<byte>("/proc/self/mountinfo\x00"u8);
    var (fd, errno) = linux.Open(Ꮡ(path, 0), (nint)((nint)linux.O_RDONLY | (nint)linux.O_CLOEXEC), 0);
    if (errno == linux.ENOENT){
        return (0, ErrNoCgroup);
    } else 
    if (errno != 0) {
        return (0, errSyscallFailed);
    }

    var (n, err) = parseCPUMount(fd, linux.Read, @out, cgroup, version, scratch);
    if (err != default!) {
        linux.Close(fd);
        return (0, err);
    }
    linux.Close(fd);

    return (n, default!);
}

} // end cgroup_package
