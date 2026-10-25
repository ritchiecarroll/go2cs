// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using fmt = fmt_package;
using cgroup = go.@internal.runtime.cgroup_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using go.@internal.runtime;
using static go.@internal.runtime.cgroup_internal_test_package;

partial class cgroup_test_package {

internal partial struct TestParseV1Number_tests /*dyn*/ {
    internal @string name;
    internal @string contents;
    internal int64 want;
    internal bool wantErr;
}

public static void TestParseV1Number(ж<testing.T> Ꮡt) {
    var tests = new TestParseV1Number_tests[]{
        new(
            name: "disabled"u8,
            contents: "-1\n"u8,
            want: -1
        ),
        new(
            name: "500000"u8,
            contents: "500000\n"u8,
            want: 500000
        ),
        new(
            name: "MaxInt64"u8,
            contents: "9223372036854775807\n"u8,
            want: 9223372036854775807L
        ),
        new(
            name: "missing-newline"u8,
            contents: "500000"u8,
            wantErr: true
        ),
        new(
            name: "not-a-number"u8,
            contents: "123max\n"u8,
            wantErr: true
        ),
        new(
            name: "v2"u8,
            contents: "1000 5000\n"u8,
            wantErr: true
        )
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestParseV1Number_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (got, err) = cgroup_internal_test_package.ParseV1Number(slice<byte>(tcʗ1.contents));
            if (tcʗ1.wantErr) {
                if (err == default!) {
                    tΔ1.Fatalf("parseV1Number got err nil want non-nil"u8);
                }
                return;
            }
            if (err != default!) {
                tΔ1.Fatalf("parseV1Number got err %v want nil"u8, err);
            }

            if (got != tcʗ1.want) {
                tΔ1.Errorf("parseV1Number got %d want %d"u8, got, tcʗ1.want);
            }
        });
    }
}

internal partial struct TestParseV2Limit_tests /*dyn*/ {
    internal @string name;
    internal @string contents;
    internal float64 want;
    internal bool wantOK;
    internal bool wantErr;
}

public static void TestParseV2Limit(ж<testing.T> Ꮡt) {
    var tests = new TestParseV2Limit_tests[]{
        new(
            name: "disabled"u8,
            contents: "max 100000\n"u8,
            wantOK: false
        ),
        new(
            name: "5"u8,
            contents: "500000 100000\n"u8,
            want: 5D,
            wantOK: true
        ),
        new(
            name: "0.5"u8,
            contents: "50000 100000\n"u8,
            want: 0.5D,
            wantOK: true
        ),
        new(
            name: "2.5"u8,
            contents: "250000 100000\n"u8,
            want: 2.5D,
            wantOK: true
        ),
        new(
            name: "MaxInt64"u8,
            contents: "9223372036854775807 9223372036854775807\n"u8,
            want: 1D,
            wantOK: true
        ),
        new(
            name: "missing-newline"u8,
            contents: "500000 100000"u8,
            wantErr: true
        ),
        new(
            name: "v1"u8,
            contents: "500000\n"u8,
            wantErr: true
        ),
        new(
            name: "quota-not-a-number"u8,
            contents: "500000us 100000\n"u8,
            wantErr: true
        ),
        new(
            name: "period-not-a-number"u8,
            contents: "500000 100000us\n"u8,
            wantErr: true
        )
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestParseV2Limit_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (got, gotOK, err) = cgroup_internal_test_package.ParseV2Limit(slice<byte>(tcʗ1.contents));
            if (tcʗ1.wantErr) {
                if (err == default!) {
                    tΔ1.Fatalf("parseV1Limit got err nil want non-nil"u8);
                }
                return;
            }
            if (err != default!) {
                tΔ1.Fatalf("parseV2Limit got err %v want nil"u8, err);
            }

            if (gotOK != tcʗ1.wantOK) {
                tΔ1.Errorf("parseV2Limit got ok %v want %v"u8, gotOK, tcʗ1.wantOK);
            }

            if (tcʗ1.wantOK && got != tcʗ1.want) {
                tΔ1.Errorf("parseV2Limit got %f want %f"u8, got, tcʗ1.want);
            }
        });
    }
}

internal static Func<nint, slice<byte>, (nint, uintptr)> readString(@string contents) {
    var r = strings.NewReader(contents);
    var rʗ1 = r;
    return (nint fd, slice<byte> b) => {
        var (n, err) = rʗ1.Read(b);
        if (err != default! && !AreEqual(err, io.EOF)) {
            uintptr dummyErrno = 42;
            return (n, dummyErrno);
        }
        return (n, (uintptr)(0));
    };
}

internal partial struct TestParseCPUCgroup_tests /*dyn*/ {
    internal @string name;
    internal @string contents;
    internal @string want;
    internal cgroup.Version wantVer;
    internal bool wantErr;
}

public static void TestParseCPUCgroup(ж<testing.T> Ꮡt) {
    @string veryLongPathName = strings.Repeat("a"u8, cgroup.PathSize + 10);
    @string evenLongerPathName = strings.Repeat("a"u8, cgroup.ParseSize + 10);

    var tests = new TestParseCPUCgroup_tests[]{
        new(
            name: "empty"u8,
            contents: ""u8,
            wantErr: true
        ),
        new(
            name: "too-long"u8,
            contents: "0::/"u8 + veryLongPathName + "\n"u8,
            wantErr: true
        ),
        new(
            name: "too-long-line"u8,
            contents: "0::/"u8 + evenLongerPathName + "\n"u8,
            wantErr: true
        ),
        new(
            name: "v1"u8,
            contents: """
2:cpu,cpuacct:/a/b/cpu
1:blkio:/a/b/blkio

"""u8.Raw(),
            want: "/a/b/cpu"u8,
            wantVer: cgroup.V1
        ),
        new(
            name: "v2"u8,
            contents: "0::/a/b/c\n"u8,
            want: "/a/b/c"u8,
            wantVer: cgroup.V2
        ),
        new(
            name: "mixed"u8,
            contents: """
2:cpu,cpuacct:/a/b/cpu
1:blkio:/a/b/blkio
0::/a/b/v2

"""u8.Raw(),
            want: "/a/b/cpu"u8,
            wantVer: cgroup.V1
        )
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestParseCPUCgroup_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            array<byte> got = new(4096); /* cgroup.PathSize */
            array<byte> scratch = new(65536); /* cgroup.ParseSize */
            var (n, gotVer, err) = cgroup_internal_test_package.ParseCPUCgroup(0, readString(tcʗ1.contents), got[..], scratch[..]);
            if ((err != default!) != tcʗ1.wantErr) {
                tΔ1.Fatalf("parseCPURelativePath got err %v want %v"u8, err, tcʗ1.wantErr);
            }

            if (gotVer != tcʗ1.wantVer) {
                tΔ1.Errorf("parseCPURelativePath got cgroup version %d want %d"u8, gotVer, tcʗ1.wantVer);
            }

            if (((sstring)(got.slice(0, n))) != tcʗ1.want) {
                tΔ1.Errorf("parseCPURelativePath got %q want %q"u8, (@string)(got.slice(0, n)), tcʗ1.want);
            }
        });
    }
}

public static void TestParseCPUCgroupMalformed(ж<testing.T> Ꮡt) {
    foreach (var (_, contents) in new @string[]{
        "\n"u8,
        "0\n"u8,
        "0:\n"u8,
        "0::\n"u8,
        "0::a\n"u8
    }.slice()) {
        Ꮡt.Run(""u8, (ж<testing.T> tΔ1) => {
            array<byte> got = new(4096); /* cgroup.PathSize */
            array<byte> scratch = new(65536); /* cgroup.ParseSize */
            var (n, v, err) = cgroup_internal_test_package.ParseCPUCgroup(0, readString(contents), got[..], scratch[..]);
            if (!AreEqual(err, cgroup_internal_test_package.ErrMalformedFile)) {
                tΔ1.Errorf("ParseCPUCgroup got %q (v%d), %v, want ErrMalformedFile"u8, (@string)(got.slice(0, n)), v, err);
            }
        });
    }
}

internal partial struct TestContainsCPU_tests /*dyn*/ {
    internal @string @in;
    internal bool want;
}

public static void TestContainsCPU(ж<testing.T> Ꮡt) {
    var tests = new TestContainsCPU_tests[]{
        new(
            @in: ""u8,
            want: false
        ),
        new(
            @in: ","u8,
            want: false
        ),
        new(
            @in: "cpu"u8,
            want: true
        ),
        new(
            @in: "memory,cpu"u8,
            want: true
        ),
        new(
            @in: "cpu,memory"u8,
            want: true
        ),
        new(
            @in: "memory,cpu,block"u8,
            want: true
        ),
        new(
            @in: "memory,cpuacct,block"u8,
            want: false
        )
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestContainsCPU_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.@in, (ж<testing.T> tΔ1) => {
            var got = cgroup_internal_test_package.ContainsCPU(slice<byte>(tcʗ1.@in));
            if (got != tcʗ1.want) {
                tΔ1.Errorf("containsCPU(%q) got %v want %v"u8, tcʗ1.@in, got, tcʗ1.want);
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string lowerPathᶜ = "/so/many/overlay/layers"u8;

internal partial struct TestParseCPUMount_tests /*dyn*/ {
    internal @string name;
    internal @string contents;
    internal @string cgroup;
    internal cgroup.Version version;
    internal @string want;
    internal bool wantErr;
}

public static void TestParseCPUMount(ж<testing.T> Ꮡt) {
    // Used for v2-longline. We want an overlayfs mount to have an option
    // so long that the entire line can't possibly fit in the scratch
    // buffer.
    @string lowerPath = lowerPathᶜ;
    @string overlayLongLowerDir = lowerPath;
    for (nint i = 0; len(overlayLongLowerDir) < cgroup.ScratchSize; i++) {
        overlayLongLowerDir += fmt.Sprintf(":%s%d"u8, lowerPath, i);
    }

    array<byte> longPath = new(4090);
    foreach (var i in range(longPath)) {
        longPath[i] = (byte)i;
    }
    @string escapedLongPath = escapePath((@string)(longPath[..]));
    if (len(escapedLongPath) <= cgroup.PathSize) {
        // ensure we actually support over PathSize long escaped path
        Ꮡt.Fatalf("escapedLongPath is too short to test"u8);
    }

    var tests = new TestParseCPUMount_tests[]{
        new(
            name: "empty"u8,
            contents: ""u8,
            wantErr: true
        ),
        new(
            name: "invalid-root"u8,
            contents: "56 22 0:40 /\\1 /sys/fs/cgroup/cpu rw - cgroup cgroup rw,cpu,cpuacct\n"u8,
            cgroup: "/"u8,
            version: cgroup.V1,
            wantErr: true
        ),
        new(
            name: "invalid-mount"u8,
            contents: "56 22 0:40 / /sys/fs/cgroup/\\1 rw - cgroup cgroup rw,cpu,cpuacct\n"u8,
            cgroup: "/"u8,
            version: cgroup.V1,
            wantErr: true
        ),
        new(
            name: "v1"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
49 22 0:37 / /sys/fs/cgroup/memory rw - cgroup cgroup rw,memory
54 22 0:38 / /sys/fs/cgroup/io rw - cgroup cgroup rw,io
56 22 0:40 / /sys/fs/cgroup/cpu rw - cgroup cgroup rw,cpu,cpuacct
58 22 0:42 / /sys/fs/cgroup/net rw - cgroup cgroup rw,net
59 22 0:43 / /sys/fs/cgroup/cpuset rw - cgroup cgroup rw,cpuset

"""u8.Raw(),
            cgroup: "/"u8,
            version: cgroup.V1,
            want: "/sys/fs/cgroup/cpu"u8
        ),
        new(
            name: "v2"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/"u8,
            version: cgroup.V2,
            want: "/sys/fs/cgroup"u8
        ),
        new(
            name: "mixed"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw
49 22 0:37 / /sys/fs/cgroup/memory rw - cgroup cgroup rw,memory
54 22 0:38 / /sys/fs/cgroup/io rw - cgroup cgroup rw,io
56 22 0:40 / /sys/fs/cgroup/cpu rw - cgroup cgroup rw,cpu,cpuacct
58 22 0:42 / /sys/fs/cgroup/net rw - cgroup cgroup rw,net
59 22 0:43 / /sys/fs/cgroup/cpuset rw - cgroup cgroup rw,cpuset

"""u8.Raw(),
            cgroup: "/"u8,
            version: cgroup.V1,
            want: "/sys/fs/cgroup/cpu"u8
        ),
        new(
            name: "mixed-choose-v2"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw
49 22 0:37 / /sys/fs/cgroup/memory rw - cgroup cgroup rw,memory
54 22 0:38 / /sys/fs/cgroup/io rw - cgroup cgroup rw,io
56 22 0:40 / /sys/fs/cgroup/cpu rw - cgroup cgroup rw,cpu,cpuacct
58 22 0:42 / /sys/fs/cgroup/net rw - cgroup cgroup rw,net
59 22 0:43 / /sys/fs/cgroup/cpuset rw - cgroup cgroup rw,cpuset

"""u8.Raw(),
            cgroup: "/"u8,
            version: cgroup.V2,
            want: "/sys/fs/cgroup"u8
        ),
        new(
            name: "v2-escaped"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/fs/cgroup/tab\011tab rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/"u8,
            version: cgroup.V2,
            want: @"/sys/fs/cgroup/tab	tab"u8
        ),
        new(
            // Overly long line on a different mount doesn't matter.
            name: "v2-longline"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
262 31 0:72 / /tmp/overlay2/0143e063b02f4801de9c847ad1c5ddc21fd2ead00653064d0c72ea967b248870/merged rw,relatime shared:729 - overlay overlay rw,lowerdir=
"""u8.Raw() + overlayLongLowerDir + """
,upperdir=/tmp/diff,workdir=/tmp/work
25 21 0:22 / /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/"u8,
            version: cgroup.V2,
            want: "/sys/fs/cgroup"u8
        ),
        new(
            name: "long-escaped-path"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/
"""u8.Raw() + escapedLongPath + """
 rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/"u8,
            version: cgroup.V2,
            want: "/sys/"u8 + ((sstring)(longPath[..]))
        ),
        new(
            name: "too-long-escaped-path"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/
"""u8.Raw() + escapedLongPath + """
 rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/container"u8, // compared to above, this makes the path too long
            version: cgroup.V2,
            wantErr: true
        ),
        new(
            name: "non-root_mount"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 /sand /unrelated/cgroup1 rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw
25 21 0:22 /stone /unrelated/cgroup2 rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw
25 21 0:22 /sandbox/container/group /sys/fs/cgroup/mygroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw
25 21 0:22 /sandbox /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw
25 21 0:22 / /ignored/second/match rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/sandbox/container"u8,
            version: cgroup.V2,
            want: "/sys/fs/cgroup/container"u8
        ),
        new(
            name: "v2-escaped-root"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 /tab\011tab /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/tab	tab/container"u8,
            version: cgroup.V2,
            want: @"/sys/fs/cgroup/container"u8
        ),
        new(
            name: "non-root_cgroup"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/sandbox/container"u8,
            version: cgroup.V2,
            want: "/sys/fs/cgroup/sandbox/container"u8
        ),
        new(
            name: "mixed_non-root"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 /sandbox /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw
49 22 0:37 /sandbox /sys/fs/cgroup/memory rw - cgroup cgroup rw,memory
54 22 0:38 /sandbox /sys/fs/cgroup/io rw - cgroup cgroup rw,io
56 22 0:40 /sand /unrelated/cgroup1 rw - cgroup cgroup rw,cpu,cpuacct
56 22 0:40 /stone /unrelated/cgroup2 rw - cgroup cgroup rw,cpu,cpuacct
56 22 0:40 /sandbox /sys/fs/cgroup/cpu rw - cgroup cgroup rw,cpu,cpuacct
56 22 0:40 /sandbox/container/group /sys/fs/cgroup/cpu/mygroup rw - cgroup cgroup rw,cpu,cpuacct
56 22 0:40 / /ignored/second/match rw - cgroup cgroup rw,cpu,cpuacct
58 22 0:42 /sandbox /sys/fs/cgroup/net rw - cgroup cgroup rw,net
59 22 0:43 /sandbox /sys/fs/cgroup/cpuset rw - cgroup cgroup rw,cpuset

"""u8.Raw(),
            cgroup: "/sandbox/container"u8,
            version: cgroup.V1,
            want: "/sys/fs/cgroup/cpu/container"u8
        ),
        new(
            // to see an example of this, for a PID in a cgroup namespace, run:
            // nsenter -t <PID> -C -- cat /proc/self/cgroup
            // nsenter -t <PID> -C -- grep cgroup /proc/self/mountinfo
            // /mnt can be generated with `mount --bind /sys/fs/cgroup/kubepods.slice /mnt`,
            // assuming PID is in cgroup /kubepods.slice
            name: "out_of_namespace"u8,
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
1243 61 0:26 /../../.. /mnt rw,nosuid,nodev,noexec,relatime shared:4 - cgroup2 cgroup2 rw
29 22 0:26 /../../../.. /sys/fs/cgroup rw,nosuid,nodev,noexec,relatime shared:4 - cgroup2 cgroup2 rw
"""u8.Raw(),
            cgroup: "/../../../../init.scope"u8,
            version: cgroup.V2,
            want: "/sys/fs/cgroup/init.scope"u8
        ),
        new(
            name: "out_of_namespace-root"u8, // the process is directly in the root cgroup
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
1243 61 0:26 /../../.. /mnt rw,nosuid,nodev,noexec,relatime shared:4 - cgroup2 cgroup2 rw
29 22 0:26 /../../../.. /sys/fs/cgroup rw,nosuid,nodev,noexec,relatime shared:4 - cgroup2 cgroup2 rw
"""u8.Raw(),
            cgroup: "/../../../.."u8,
            version: cgroup.V2,
            want: "/sys/fs/cgroup"u8
        )
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestParseCPUMount_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            array<byte> got = new(4096); /* cgroup.PathSize */
            array<byte> scratch = new(65536); /* cgroup.ParseSize */
            nint n = copy(got[..], tcʗ1.cgroup);
            (n, var err) = cgroup_internal_test_package.ParseCPUMount(0, readString(tcʗ1.contents), got[..],
                got.slice(0, n), tcʗ1.version, scratch[..]);
            if ((err != default!) != tcʗ1.wantErr) {
                tΔ1.Fatalf("parseCPUMount got err %v want %v"u8, err, tcʗ1.wantErr);
            }

            if (((sstring)(got.slice(0, n))) != tcʗ1.want) {
                tΔ1.Errorf("parseCPUMount got %q want %q"u8, (@string)(got.slice(0, n)), tcʗ1.want);
            }
        });
    }
}

public static void TestParseCPUMountMalformed(ж<testing.T> Ꮡt) {
    foreach (var (_, contents) in new @string[]{
        "\n"u8,
        "22\n"u8,
        "22 1 8:1\n"u8,
        "22 1 8:1 /\n"u8,
        "22 1 8:1 / /cgroup\n"u8,
        "22 1 8:1 / /cgroup rw\n"u8,
        "22 1 8:1 / /cgroup rw -\n"u8,
        "22 1 8:1 / /cgroup rw - \n"u8,
        "22 1 8:1 / /cgroup rw - cgroup\n"u8,
        "22 1 8:1 / /cgroup rw - cgroup cgroup\n"u8,
        "22 1 8:1 a /cgroup rw - cgroup cgroup cpu\n"u8
    }.slice()) {
        Ꮡt.Run(""u8, (ж<testing.T> tΔ1) => {
            array<byte> got = new(4096); /* cgroup.PathSize */
            array<byte> scratch = new(65536); /* cgroup.ParseSize */
            var (n, err) = cgroup_internal_test_package.ParseCPUMount(0, readString(contents), got[..], slice<byte>("/"u8), cgroup.V1, scratch[..]);
            if (!AreEqual(err, cgroup_internal_test_package.ErrMalformedFile)) {
                tΔ1.Errorf("parseCPUMount got %q, %v, want ErrMalformedFile"u8, (@string)(got.slice(0, n)), err);
            }
        });
    }
}

// escapePath performs escaping equivalent to Linux's show_path.
//
// That is, '\', ' ', '\t', and '\n' are converted to octal escape sequences,
// like '\040' for space.
internal static @string escapePath(@string s) {
    var outᴷ = new stackAppend<byte>();

    var @out = new slice<byte>(0, len(s));
    foreach (var (_, c) in slice<byte>(s)) {
        switch (c) {
        case (rune)'\\' or (rune)' ' or (rune)'\t' or (rune)'\n': {
            @out = fmt.Appendf(@out, "\\%03o"u8, c);
            break;
        }
        default: {
            @out = append(outᴷ, @out, c);
            break;
        }}
    }
    return (@string)@out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string escapePathˢ = "escapePath"u8;
internal static readonly @string unescapePathˢ = "unescapePath"u8;

internal partial struct TestEscapePath_tests /*dyn*/ {
    internal @string name;
    internal @string unescaped;
    internal @string escaped;
}

public static void TestEscapePath(ж<testing.T> Ꮡt) {
    var tests = new TestEscapePath_tests[]{
        new(
            name: "boring"u8,
            unescaped: @"/a/b/c"u8,
            escaped: @"/a/b/c"u8
        ),
        new(
            name: "space"u8,
            unescaped: @"/a/b b/c"u8,
            escaped: @"/a/b\040b/c"u8
        ),
        new(
            name: "tab"u8,
            unescaped: @"/a/b	b/c"u8,
            escaped: @"/a/b\011b/c"u8
        ),
        new(
            name: "newline"u8,
            unescaped: """
/a/b
b/c
"""u8.Raw(),
            escaped: @"/a/b\012b/c"u8
        ),
        new(
            name: "slash"u8,
            unescaped: @"/a/b\b/c"u8,
            escaped: @"/a/b\134b/c"u8
        ),
        new(
            name: "beginning"u8,
            unescaped: @"\b/c"u8,
            escaped: @"\134b/c"u8
        ),
        new(
            name: "ending"u8,
            unescaped: @"/a/\"u8,
            escaped: @"/a/\134"u8
        ),
        new(
            name: "non-utf8"u8,
            unescaped: ((@string)(new byte[]{0x2f, 0x61, 0x2f, 0x62, 0xff, 0x20, 0x2f, 0x63})),
            escaped: ((@string)(new byte[]{0x2f, 0x61, 0x2f, 0x62, 0xff, 0x5c, 0x30, 0x34, 0x30, 0x2f, 0x63}))
        )
    }.slice();

    var testsʗ1 = tests;
    Ꮡt.Run(escapePathˢ, (ж<testing.T> tΔ1) => {
        foreach (var (_, vᴛ1) in testsʗ1) {
            ref var tc = ref heap(new TestEscapePath_tests(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            tΔ1.Run(tc.name, (ж<testing.T> tΔ2) => {
                @string got = escapePath(tcʗ1.unescaped);
                if (got != tcʗ1.escaped) {
                    tΔ2.Errorf("escapePath got %q want %q"u8, got, tcʗ1.escaped);
                }
            });
        }
    });

    var testsʗ2 = tests;
    Ꮡt.Run(unescapePathˢ, (ж<testing.T> tΔ3) => {
        foreach (var (_, vᴛ2) in testsʗ2) {
            ref var tc = ref heap(new TestEscapePath_tests(), out var Ꮡtc);
            tc = vᴛ2;

            var tcʗ2 = tc;
            void runTest(slice<byte> @in, slice<byte> @out) {
                var (n, err) = cgroup_internal_test_package.UnescapePath(@out, @in);
                if (err != default!) {
                    tΔ3.Errorf("unescapePath got err %v want nil"u8, err);
                }
                @string got = (@string)(@out.slice(0, n));
                if (got != tcʗ2.unescaped) {
                    tΔ3.Errorf("unescapePath got %q want %q"u8, got, tcʗ2.escaped);
                }
            }
            var runTestʗ1 = runTest;
            var tcʗ3 = tc;
            tΔ3.Run(tc.name, (ж<testing.T> tΔ4) => {
                var @in = slice<byte>(tcʗ3.escaped);
                var @out = new slice<byte>(len(@in));
                runTestʗ1(@in, @out);
            });
            var runTestʗ2 = runTest;
            var tcʗ4 = tc;
            tΔ3.Run("inplace/"u8 + tc.name, (ж<testing.T> tΔ5) => {
                var @in = slice<byte>(tcʗ4.escaped);
                runTestʗ2(@in, @in);
            });
        }
    });
}

public static void TestUnescapeInvalidPath(ж<testing.T> Ꮡt) {
    foreach (var (_, @in) in new @string[]{
        @"/a/b\c"u8,
        @"/a/b\01"u8,
        @"/a/b\018"u8,
        @"/a/b\01c"u8,
        @"/a/b\777"u8,
        @"01234567890123456789"u8, // too long
        @"\001\002\003\004\005\006\007\010\011"u8 // too long
    }.slice()) {
        var @out = new slice<byte>(8);
        var outʗ1 = @out;
        Ꮡt.Run(@in, (ж<testing.T> tΔ1) => {
            var (_, err) = cgroup_internal_test_package.UnescapePath(outʗ1, slice<byte>(@in));
            if (err == default!) {
                tΔ1.Errorf("unescapePath got nil err, want non-nil"u8);
            }
        });
    }
}

} // end cgroup_test_package
