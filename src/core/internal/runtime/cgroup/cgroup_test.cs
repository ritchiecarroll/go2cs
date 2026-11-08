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

public static void TestParseV1Number(ж<testing.T> t) {
    var tests = slice<TestParseV1Number_tests>([
        new(
            name: "disabled",
            contents: "-1\n",
            want: -1
        ),
        new(
            name: "500000",
            contents: "500000\n",
            want: 500000
        ),
        new(
            name: "MaxInt64",
            contents: "9223372036854775807\n",
            want: 9223372036854775807L
        ),
        new(
            name: "missing-newline",
            contents: "500000",
            wantErr: true
        ),
        new(
            name: "not-a-number",
            contents: "123max\n",
            wantErr: true
        ),
        new(
            name: "v2",
            contents: "1000 5000\n",
            wantErr: true
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestParseV1Number_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (got, err) = cgroup_internal_test_package.ParseV1Number(slice<byte>(tcʗ1.contents));
            if (tcʗ1.wantErr) {
                if (err == nil) {
                    tΔ1.Fatalf("parseV1Number got err nil want non-nil");
                }
                return;
            }
            if (err != nil) {
                tΔ1.Fatalf("parseV1Number got err %v want nil", err);
            }

            if (got != tcʗ1.want) {
                tΔ1.Errorf("parseV1Number got %d want %d", got, tcʗ1.want);
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

public static void TestParseV2Limit(ж<testing.T> t) {
    var tests = slice<TestParseV2Limit_tests>([
        new(
            name: "disabled",
            contents: "max 100000\n",
            wantOK: false
        ),
        new(
            name: "5",
            contents: "500000 100000\n",
            want: 5D,
            wantOK: true
        ),
        new(
            name: "0.5",
            contents: "50000 100000\n",
            want: 0.5D,
            wantOK: true
        ),
        new(
            name: "2.5",
            contents: "250000 100000\n",
            want: 2.5D,
            wantOK: true
        ),
        new(
            name: "MaxInt64",
            contents: "9223372036854775807 9223372036854775807\n",
            want: 1D,
            wantOK: true
        ),
        new(
            name: "missing-newline",
            contents: "500000 100000",
            wantErr: true
        ),
        new(
            name: "v1",
            contents: "500000\n",
            wantErr: true
        ),
        new(
            name: "quota-not-a-number",
            contents: "500000us 100000\n",
            wantErr: true
        ),
        new(
            name: "period-not-a-number",
            contents: "500000 100000us\n",
            wantErr: true
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestParseV2Limit_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (got, gotOK, err) = cgroup_internal_test_package.ParseV2Limit(slice<byte>(tcʗ1.contents));
            if (tcʗ1.wantErr) {
                if (err == nil) {
                    tΔ1.Fatalf("parseV1Limit got err nil want non-nil");
                }
                return;
            }
            if (err != nil) {
                tΔ1.Fatalf("parseV2Limit got err %v want nil", err);
            }

            if (gotOK != tcʗ1.wantOK) {
                tΔ1.Errorf("parseV2Limit got ok %v want %v", gotOK, tcʗ1.wantOK);
            }

            if (tcʗ1.wantOK && got != tcʗ1.want) {
                tΔ1.Errorf("parseV2Limit got %f want %f", got, tcʗ1.want);
            }
        });
    }
}

internal static Func<nint, slice<byte>, (nint, uintptr)> readString(@string contents) {
    var r = strings.NewReader(contents);
    var rʗ1 = r;
    return (nint fd, slice<byte> b) => {
        var (n, err) = rʗ1.Read(b);
        if (err != nil && !AreEqual(err, io.EOF)) {
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

public static void TestParseCPUCgroup(ж<testing.T> t) {
    @string veryLongPathName = strings.Repeat("a", cgroup.PathSize + 10);
    @string evenLongerPathName = strings.Repeat("a", cgroup.ParseSize + 10);

    var tests = slice<TestParseCPUCgroup_tests>([
        new(
            name: "empty",
            contents: "",
            wantErr: true
        ),
        new(
            name: "too-long",
            contents: "0::/"u8 + veryLongPathName + "\n"u8,
            wantErr: true
        ),
        new(
            name: "too-long-line",
            contents: "0::/"u8 + evenLongerPathName + "\n"u8,
            wantErr: true
        ),
        new(
            name: "v1",
            contents: """
2:cpu,cpuacct:/a/b/cpu
1:blkio:/a/b/blkio

"""u8.Raw(),
            want: "/a/b/cpu",
            wantVer: cgroup.V1
        ),
        new(
            name: "v2",
            contents: "0::/a/b/c\n",
            want: "/a/b/c",
            wantVer: cgroup.V2
        ),
        new(
            name: "mixed",
            contents: """
2:cpu,cpuacct:/a/b/cpu
1:blkio:/a/b/blkio
0::/a/b/v2

"""u8.Raw(),
            want: "/a/b/cpu",
            wantVer: cgroup.V1
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestParseCPUCgroup_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            array<byte> got = new(4096); /* cgroup.PathSize */
            array<byte> scratch = new(65536); /* cgroup.ParseSize */
            var (n, gotVer, err) = cgroup_internal_test_package.ParseCPUCgroup(0, readString(tcʗ1.contents), got[..], scratch[..]);
            if ((err != nil) != tcʗ1.wantErr) {
                tΔ1.Fatalf("parseCPURelativePath got err %v want %v", err, tcʗ1.wantErr);
            }

            if (gotVer != tcʗ1.wantVer) {
                tΔ1.Errorf("parseCPURelativePath got cgroup version %d want %d", gotVer, tcʗ1.wantVer);
            }

            if (((sstring)(got.slice(0, n))) != tcʗ1.want) {
                tΔ1.Errorf("parseCPURelativePath got %q want %q", (@string)(got.slice(0, n)), tcʗ1.want);
            }
        });
    }
}

public static void TestParseCPUCgroupMalformed(ж<testing.T> t) {
    foreach (var (_, contents) in range(slice<@string>([
        "\n",
        "0\n",
        "0:\n",
        "0::\n",
        "0::a\n"
    ]))) {
        t.Run(""u8, (ж<testing.T> tΔ1) => {
            array<byte> got = new(4096); /* cgroup.PathSize */
            array<byte> scratch = new(65536); /* cgroup.ParseSize */
            var (n, v, err) = cgroup_internal_test_package.ParseCPUCgroup(0, readString(contents), got[..], scratch[..]);
            if (!AreEqual(err, cgroup_internal_test_package.ErrMalformedFile)) {
                tΔ1.Errorf("ParseCPUCgroup got %q (v%d), %v, want ErrMalformedFile", (@string)(got.slice(0, n)), v, err);
            }
        });
    }
}

internal partial struct TestContainsCPU_tests /*dyn*/ {
    internal @string @in;
    internal bool want;
}

public static void TestContainsCPU(ж<testing.T> t) {
    var tests = slice<TestContainsCPU_tests>([
        new(
            @in: "",
            want: false
        ),
        new(
            @in: ",",
            want: false
        ),
        new(
            @in: "cpu",
            want: true
        ),
        new(
            @in: "memory,cpu",
            want: true
        ),
        new(
            @in: "cpu,memory",
            want: true
        ),
        new(
            @in: "memory,cpu,block",
            want: true
        ),
        new(
            @in: "memory,cpuacct,block",
            want: false
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestContainsCPU_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.@in, (ж<testing.T> tΔ1) => {
            var got = cgroup_internal_test_package.ContainsCPU(slice<byte>(tcʗ1.@in));
            if (got != tcʗ1.want) {
                tΔ1.Errorf("containsCPU(%q) got %v want %v", tcʗ1.@in, got, tcʗ1.want);
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string lowerPathᶜ => ref lowerPathᶜᵇ;
private static readonly @string lowerPathᶜᵇ = "/so/many/overlay/layers"u8;

internal partial struct TestParseCPUMount_tests /*dyn*/ {
    internal @string name;
    internal @string contents;
    internal @string cgroup;
    internal cgroup.Version version;
    internal @string want;
    internal bool wantErr;
}

public static void TestParseCPUMount(ж<testing.T> t) {
    // Used for v2-longline. We want an overlayfs mount to have an option
    // so long that the entire line can't possibly fit in the scratch
    // buffer.
    @string lowerPath = lowerPathᶜ;
    @string overlayLongLowerDir = lowerPath;
    for (nint i = 0; len(overlayLongLowerDir) < cgroup.ScratchSize; i++) {
        overlayLongLowerDir += fmt.Sprintf(":%s%d"u8, lowerPath, i);
    }

    array<byte> longPath = new(4090);
    foreach (var i in range(len(longPath))) {
        longPath[i] = (byte)i;
    }
    @string escapedLongPath = escapePath((@string)(longPath[..]));
    if (len(escapedLongPath) <= cgroup.PathSize) {
        // ensure we actually support over PathSize long escaped path
        t.Fatalf("escapedLongPath is too short to test");
    }

    var tests = slice<TestParseCPUMount_tests>([
        new(
            name: "empty",
            contents: "",
            wantErr: true
        ),
        new(
            name: "invalid-root",
            contents: "56 22 0:40 /\\1 /sys/fs/cgroup/cpu rw - cgroup cgroup rw,cpu,cpuacct\n",
            cgroup: "/",
            version: cgroup.V1,
            wantErr: true
        ),
        new(
            name: "invalid-mount",
            contents: "56 22 0:40 / /sys/fs/cgroup/\\1 rw - cgroup cgroup rw,cpu,cpuacct\n",
            cgroup: "/",
            version: cgroup.V1,
            wantErr: true
        ),
        new(
            name: "v1",
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
            cgroup: "/",
            version: cgroup.V1,
            want: "/sys/fs/cgroup/cpu"
        ),
        new(
            name: "v2",
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/",
            version: cgroup.V2,
            want: "/sys/fs/cgroup"
        ),
        new(
            name: "mixed",
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
            cgroup: "/",
            version: cgroup.V1,
            want: "/sys/fs/cgroup/cpu"
        ),
        new(
            name: "mixed-choose-v2",
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
            cgroup: "/",
            version: cgroup.V2,
            want: "/sys/fs/cgroup"
        ),
        new(
            name: "v2-escaped",
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/fs/cgroup/tab\011tab rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/",
            version: cgroup.V2,
            want: @"/sys/fs/cgroup/tab	tab"
        ),
        new(
            // Overly long line on a different mount doesn't matter.
            name: "v2-longline",
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
262 31 0:72 / /tmp/overlay2/0143e063b02f4801de9c847ad1c5ddc21fd2ead00653064d0c72ea967b248870/merged rw,relatime shared:729 - overlay overlay rw,lowerdir=
"""u8.Raw() + overlayLongLowerDir + """
,upperdir=/tmp/diff,workdir=/tmp/work
25 21 0:22 / /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/",
            version: cgroup.V2,
            want: "/sys/fs/cgroup"
        ),
        new(
            name: "long-escaped-path",
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/
"""u8.Raw() + escapedLongPath + """
 rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/",
            version: cgroup.V2,
            want: "/sys/"u8 + ((sstring)(longPath[..]))
        ),
        new(
            name: "too-long-escaped-path",
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/
"""u8.Raw() + escapedLongPath + """
 rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/container", // compared to above, this makes the path too long
            version: cgroup.V2,
            wantErr: true
        ),
        new(
            name: "non-root_mount",
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
            cgroup: "/sandbox/container",
            version: cgroup.V2,
            want: "/sys/fs/cgroup/container"
        ),
        new(
            name: "v2-escaped-root",
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 /tab\011tab /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/tab	tab/container",
            version: cgroup.V2,
            want: @"/sys/fs/cgroup/container"
        ),
        new(
            name: "non-root_cgroup",
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
25 21 0:22 / /sys/fs/cgroup rw,nosuid,nodev,noexec - cgroup2 cgroup2 rw

"""u8.Raw(),
            cgroup: "/sandbox/container",
            version: cgroup.V2,
            want: "/sys/fs/cgroup/sandbox/container"
        ),
        new(
            name: "mixed_non-root",
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
            cgroup: "/sandbox/container",
            version: cgroup.V1,
            want: "/sys/fs/cgroup/cpu/container"
        ),
        new(
            // to see an example of this, for a PID in a cgroup namespace, run:
            // nsenter -t <PID> -C -- cat /proc/self/cgroup
            // nsenter -t <PID> -C -- grep cgroup /proc/self/mountinfo
            // /mnt can be generated with `mount --bind /sys/fs/cgroup/kubepods.slice /mnt`,
            // assuming PID is in cgroup /kubepods.slice
            name: "out_of_namespace",
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
1243 61 0:26 /../../.. /mnt rw,nosuid,nodev,noexec,relatime shared:4 - cgroup2 cgroup2 rw
29 22 0:26 /../../../.. /sys/fs/cgroup rw,nosuid,nodev,noexec,relatime shared:4 - cgroup2 cgroup2 rw
"""u8.Raw(),
            cgroup: "/../../../../init.scope",
            version: cgroup.V2,
            want: "/sys/fs/cgroup/init.scope"
        ),
        new(
            name: "out_of_namespace-root", // the process is directly in the root cgroup
            contents: """
22 1 8:1 / / rw,relatime - ext4 /dev/root rw
20 22 0:19 / /proc rw,nosuid,nodev,noexec - proc proc rw
21 22 0:20 / /sys rw,nosuid,nodev,noexec - sysfs sysfs rw
1243 61 0:26 /../../.. /mnt rw,nosuid,nodev,noexec,relatime shared:4 - cgroup2 cgroup2 rw
29 22 0:26 /../../../.. /sys/fs/cgroup rw,nosuid,nodev,noexec,relatime shared:4 - cgroup2 cgroup2 rw
"""u8.Raw(),
            cgroup: "/../../../..",
            version: cgroup.V2,
            want: "/sys/fs/cgroup"
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestParseCPUMount_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            array<byte> got = new(4096); /* cgroup.PathSize */
            array<byte> scratch = new(65536); /* cgroup.ParseSize */
            nint n = copy(got[..], tcʗ1.cgroup);
            (n, var err) = cgroup_internal_test_package.ParseCPUMount(0, readString(tcʗ1.contents), got[..],
                got.slice(0, n), tcʗ1.version, scratch[..]);
            if ((err != nil) != tcʗ1.wantErr) {
                tΔ1.Fatalf("parseCPUMount got err %v want %v", err, tcʗ1.wantErr);
            }

            if (((sstring)(got.slice(0, n))) != tcʗ1.want) {
                tΔ1.Errorf("parseCPUMount got %q want %q", (@string)(got.slice(0, n)), tcʗ1.want);
            }
        });
    }
}

public static void TestParseCPUMountMalformed(ж<testing.T> t) {
    foreach (var (_, contents) in range(slice<@string>([
        "\n",
        "22\n",
        "22 1 8:1\n",
        "22 1 8:1 /\n",
        "22 1 8:1 / /cgroup\n",
        "22 1 8:1 / /cgroup rw\n",
        "22 1 8:1 / /cgroup rw -\n",
        "22 1 8:1 / /cgroup rw - \n",
        "22 1 8:1 / /cgroup rw - cgroup\n",
        "22 1 8:1 / /cgroup rw - cgroup cgroup\n",
        "22 1 8:1 a /cgroup rw - cgroup cgroup cpu\n"
    ]))) {
        t.Run(""u8, (ж<testing.T> tΔ1) => {
            array<byte> got = new(4096); /* cgroup.PathSize */
            array<byte> scratch = new(65536); /* cgroup.ParseSize */
            var (n, err) = cgroup_internal_test_package.ParseCPUMount(0, readString(contents), got[..], slice<byte>("/"u8), cgroup.V1, scratch[..]);
            if (!AreEqual(err, cgroup_internal_test_package.ErrMalformedFile)) {
                tΔ1.Errorf("parseCPUMount got %q, %v, want ErrMalformedFile", (@string)(got.slice(0, n)), err);
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
    foreach (var (_, c) in range(slice<byte>(s))) {
        switch (c) {
        case (byte)'\\' or (byte)' ' or (byte)'\t' or (byte)'\n': {
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
internal static ref readonly @string escapePathˢ => ref escapePathˢᵇ;
private static readonly @string escapePathˢᵇ = "escapePath"u8;
internal static ref readonly @string unescapePathˢ => ref unescapePathˢᵇ;
private static readonly @string unescapePathˢᵇ = "unescapePath"u8;

internal partial struct TestEscapePath_tests /*dyn*/ {
    internal @string name;
    internal @string unescaped;
    internal @string escaped;
}

public static void TestEscapePath(ж<testing.T> t) {
    var tests = slice<TestEscapePath_tests>([
        new(
            name: "boring",
            unescaped: @"/a/b/c",
            escaped: @"/a/b/c"
        ),
        new(
            name: "space",
            unescaped: @"/a/b b/c",
            escaped: @"/a/b\040b/c"
        ),
        new(
            name: "tab",
            unescaped: @"/a/b	b/c",
            escaped: @"/a/b\011b/c"
        ),
        new(
            name: "newline",
            unescaped: """
/a/b
b/c
"""u8.Raw(),
            escaped: @"/a/b\012b/c"
        ),
        new(
            name: "slash",
            unescaped: @"/a/b\b/c",
            escaped: @"/a/b\134b/c"
        ),
        new(
            name: "beginning",
            unescaped: @"\b/c",
            escaped: @"\134b/c"
        ),
        new(
            name: "ending",
            unescaped: @"/a/\",
            escaped: @"/a/\134"
        ),
        new(
            name: "non-utf8",
            unescaped: ((@string)(new byte[]{0x2f, 0x61, 0x2f, 0x62, 0xff, 0x20, 0x2f, 0x63})),
            escaped: ((@string)(new byte[]{0x2f, 0x61, 0x2f, 0x62, 0xff, 0x5c, 0x30, 0x34, 0x30, 0x2f, 0x63}))
        )
    ]);

    var testsʗ1 = tests;
    t.Run(escapePathˢ, (ж<testing.T> tΔ1) => {
        foreach (var (_, vᴛ1) in range(testsʗ1)) {
            ref var tc = ref heap(new TestEscapePath_tests(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            tΔ1.Run(tc.name, (ж<testing.T> tΔ2) => {
                @string got = escapePath(tcʗ1.unescaped);
                if (got != tcʗ1.escaped) {
                    tΔ2.Errorf("escapePath got %q want %q", got, tcʗ1.escaped);
                }
            });
        }
    });

    var testsʗ2 = tests;
    t.Run(unescapePathˢ, (ж<testing.T> tΔ3) => {
        foreach (var (_, vᴛ2) in range(testsʗ2)) {
            ref var tc = ref heap(new TestEscapePath_tests(), out var Ꮡtc);
            tc = vᴛ2;

            var tcʗ2 = tc;
            void runTest(slice<byte> @in, slice<byte> @out) {
                var (n, err) = cgroup_internal_test_package.UnescapePath(@out, @in);
                if (err != nil) {
                    tΔ3.Errorf("unescapePath got err %v want nil", err);
                }
                @string got = (@string)(@out.slice(0, n));
                if (got != tcʗ2.unescaped) {
                    tΔ3.Errorf("unescapePath got %q want %q", got, tcʗ2.escaped);
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

public static void TestUnescapeInvalidPath(ж<testing.T> t) {
    foreach (var (_, @in) in range(slice<@string>([
        @"/a/b\c",
        @"/a/b\01",
        @"/a/b\018",
        @"/a/b\01c",
        @"/a/b\777",
        @"01234567890123456789",                // too long
        @"\001\002\003\004\005\006\007\010\011" // too long
    ]))) {
        var @out = new slice<byte>(8);
        var outʗ1 = @out;
        t.Run(@in, (ж<testing.T> tΔ1) => {
            var (_, err) = cgroup_internal_test_package.UnescapePath(outʗ1, slice<byte>(@in));
            if (err == nil) {
                tΔ1.Errorf("unescapePath got nil err, want non-nil");
            }
        });
    }
}

} // end cgroup_test_package
