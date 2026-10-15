// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cgrouptest provides best-effort helpers for running tests inside a
// cgroup.
namespace go.@internal;

using fmt = fmt_package;
using cgroup = go.@internal.runtime.cgroup_package;
using os = os_package;
using filepath = path.filepath_package;
using slices = slices_package;
using strconv = go.strconv_package;
using strings = strings_package;
using Δsyscall = syscall_package;
using testing = testing_package;
using fs = io.fs_package;
using go;
using go.@internal.runtime;
using path;

partial class cgrouptest_package {

partial struct CgroupV2 {
    internal @string orig;
    internal @string path;
}

public static @string Path(this ref CgroupV2 c) {
    return c.path;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cpuMaxˢ = "cpu.max"u8;

// Path to cpu.max.
public static @string CPUMaxPath(this ref CgroupV2 c) {
    return filepath.Join(c.path, cpuMaxˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string maxˢ = "max"u8;

// Set cpu.max. Pass -1 for quota to disable the limit.
public static error SetCPUMax(this ref CgroupV2 c, int64 quota, int64 period) {
    @string q = maxˢ;
    if (quota >= 0) {
        q = strconv.FormatInt(quota, 10);
    }
    @string buf = fmt.Sprintf("%s %d"u8, q, period);
    return os.WriteFile(c.CPUMaxPath(), slice<byte>(buf), 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cgroupSubtreeControlˢ = "cgroup.subtree_control"u8;
internal static readonly @string cpuˢ = "cpu"u8;
internal static readonly @string goCgrouptestˢ = "go-cgrouptest"u8;

// InCgroupV2 creates a new v2 cgroup, migrates the current process into it,
// and then calls fn. When fn returns, the current process is migrated back to
// the original cgroup and the new cgroup is destroyed.
//
// If a new cgroup cannot be created, the test is skipped.
//
// This must not be used in parallel tests, as it affects the entire process.
public static void InCgroupV2(ж<testing.T> Ꮡt, Action<ж<CgroupV2>> fn) {
    GoFrame ᒐ = default;
    try {
        ref var orig = ref heap<@string>(out var Ꮡorig);
        orig = findCurrent(Ꮡt);
        @string parent = findOwnedParent(Ꮡt, orig);

        // Make sure the parent allows children to control cpu.
        var (b, err) = os.ReadFile(filepath.Join(parent, cgroupSubtreeControlˢ));
        if (err != default!) {
            Ꮡt.Skipf("unable to read cgroup.subtree_control: %v"u8, err);
        }
        if (!slices.Contains(strings.Fields(((@string)b)), cpuˢ)) {
            // N.B. We should have permission to add cpu to
            // subtree_control, but it seems like a bad idea to change this
            // on a high-level cgroup that probably has lots of existing
            // children.
            Ꮡt.Skipf("Parent cgroup %s does not allow children to control cpu, only %q"u8, parent, ((@string)b));
        }

        ref var path = ref heap<@string>(out var Ꮡpath);
        (path, err) = os.MkdirTemp(parent, goCgrouptestˢ);
        if (err != default!) {
            Ꮡt.Skipf("unable to create cgroup directory: %v"u8, err);
        }
        // Important: defer cleanups so they run even in the event of panic.
        //
        // TODO(prattmic): Consider running everything in a subprocess just so
        // we can clean up if it throws or otherwise doesn't run the defers.
        var pathʗ1 = path;
        defer(() => {
            {
                var errΔ1 = os.Remove(pathʗ1); if (errΔ1 != default!) {
                    // Not much we can do, but at least inform of the
                    // problem.
                    Ꮡt.Errorf("Error removing cgroup directory: %v"u8, errΔ1);
                }
            }
        }, ref ᒐ);

        migrateTo(Ꮡt, path);
        defer(migrateTo, Ꮡt, orig, ref ᒐ);

        var c = Ꮡ(new CgroupV2(
            orig: orig,
            path: path
        ));
        fn(c);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Returns the filesystem path to the current cgroup the process is in.
internal static @string findCurrent(ж<testing.T> Ꮡt) {
    // Find the path to our current CPU cgroup. Currently this package is
    // only used for CPU cgroup testing, so the distinction of different
    // controllers doesn't matter.
    array<byte> scratch = new(65536); /* cgroup.ParseSize */
    var buf = new slice<byte>(cgroup.PathSize);
    var (n, ver, err) = cgroup.FindCPU(buf, scratch[..]);
    if (err != default!) {
        Ꮡt.Skipf("cgroup: unable to find current cgroup mount: %v"u8, err);
    }
    if (ver != cgroup.V2) {
        Ꮡt.Skipf("cgroup: running on cgroup v%d want v2"u8, ver);
    }
    return ((@string)(buf.slice(0, n)));
}

// Returns a parent directory in which we can create our own cgroup subdirectory.
internal static @string findOwnedParent(ж<testing.T> Ꮡt, @string orig) {
    // There are many ways cgroups may be set up on a system. We don't try
    // to cover all of them, just common ones.
    //
    // To start with, systemd:
    //
    // Our test process is likely running inside a user session, in which
    // case we are likely inside a cgroup that looks something like:
    //
    //   /sys/fs/cgroup/user.slice/user-1234.slice/user@1234.service/vte-spawn-1.scope/
    //
    // Possibly with additional slice layers between user@1234.service and
    // the leaf scope.
    //
    // On new enough kernel and systemd versions (exact versions unknown),
    // full unprivileged control of the user's cgroups is permitted
    // directly via the cgroup filesystem. Specifically, the
    // user@1234.service directory is owned by the user, as are all
    // subdirectories.
    // We want to create our own subdirectory that we can migrate into and
    // then manipulate at will. It is tempting to create a new subdirectory
    // inside the current cgroup we are already in, however that will likely
    // not work. cgroup v2 only allows processes to be in leaf cgroups. Our
    // current cgroup likely contains multiple processes (at least this one
    // and the cmd/go test runner). If we make a subdirectory and try to
    // move our process into that cgroup, then the subdirectory and parent
    // would both contain processes. Linux won't allow us to do that [1].
    //
    // Instead, we will simply walk up to the highest directory that our
    // user owns and create our new subdirectory. Since that directory
    // already has a bunch of subdirectories, it must not directly contain
    // and processes.
    //
    // (This would fall apart if we already in the highest directory we
    // own, such as if there was simply a single cgroup for the entire
    // user. Luckily systemd at least does not do this.)
    //
    // [1] Minor technicality: By default a new subdirectory has no cgroup
    // controller (they must be explicitly enabled in the parent's
    // cgroup.subtree_control). Linux will allow moving processes into a
    // subdirectory that has no controllers while there are still processes
    // in the parent, but it won't allow adding controller until the parent
    // is empty. As far as I tell, the only purpose of this is to allow
    // reorganizing processes into a new set of subdirectories and then
    // adding controllers once done.
    ref var stat = ref heap(new Δsyscall.Stat_t(), out var Ꮡstat);
    var err = Δsyscall.Stat(orig, Ꮡstat);
    if (err != default!) {
        Ꮡt.Fatalf("error stating orig cgroup: %v"u8, err);
    }

    nint uid = os.Getuid();
    @string prev = default!;
    @string cur = filepath.Dir(orig);
    while (cur != "/"u8) {
        ref var curStat = ref heap(new Δsyscall.Stat_t(), out var ᏑcurStat);
        err = Δsyscall.Stat(cur, ᏑcurStat);
        if (err != default!) {
            Ꮡt.Fatalf("error stating cgroup path: %v"u8, err);
        }

        if ((nint)curStat.Uid != uid || curStat.Dev != stat.Dev) {
            // Stop at first directory we don't own or filesystem boundary.
            break;
        }

        prev = cur;
        cur = filepath.Dir(cur);
    }

    if (prev == ""u8) {
        Ꮡt.Skipf("No parent cgroup owned by UID %d"u8, uid);
    }

    // We actually want the last directory where we were the owner.
    return prev;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cgroupProcsˢ = "cgroup.procs"u8;

// Migrate the current process to the cgroup directory dst.
internal static void migrateTo(ж<testing.T> Ꮡt, @string dst) {
    var pid = slice<byte>(strconv.FormatInt((int64)os.Getpid(), 10));
    {
        var err = os.WriteFile(filepath.Join(dst, cgroupProcsˢ), pid, 0); if (err != default!) {
            Ꮡt.Skipf("Unable to migrate into %s: %v"u8, dst, err);
        }
    }
}

} // end cgrouptest_package
