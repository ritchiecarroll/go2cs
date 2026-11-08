// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using fmt = fmt_package;
using fnv = hash.fnv_package;
using coverage = go.@internal.coverage_package;
using pods = go.@internal.coverage.pods_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using testing = testing_package;
using fs = io.fs_package;
using go.@internal;
using go.@internal.coverage;
using hash;
using hash = hash_package;
using path;

partial class pods_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string blahTxtˢ => ref blahTxtˢᵇ;
private static readonly @string blahTxtˢᵇ = "blah.txt"u8;
private static ref readonly @string somethingExeˢ => ref somethingExeˢᵇ;
private static readonly @string somethingExeˢᵇ = "something.exe"u8;
private static ref readonly @string orphanˢ => ref orphanˢᵇ;
private static readonly @string orphanˢᵇ = "orphan"u8;
private static ref readonly @string devNullˢ => ref devNullˢᵇ;
private static readonly @string devNullˢᵇ = "/dev/null"u8;

public static void TestPodCollection(ж<testing.T> t) {
    //testenv.MustHaveGoBuild(t)

    @string mkdir(@string d, fs.FileMode perm) {
        @string dp = filepath.Join(t.TempDir(), d);
        var errΔ1 = os.Mkdir(dp, perm); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        return dp;
    }

    @string mkfile(@string d, @string fn) {
        @string fp = filepath.Join(d, fn);
        var errΔ2 = os.WriteFile(fp, slice<byte>("foo"u8), 438); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
        return fp;
    }

    var mkfileʗ1 = mkfile;
    @string mkmeta(@string dir, @string tag) {
        var h = fnv.New128a();
        h.Write(slice<byte>(tag));
        var hash = h.Sum(default);
        @string fn = fmt.Sprintf("%s.%x"u8, coverage.MetaFilePref, hash);
        return mkfileʗ1(dir, fn);
    }

    var mkfileʗ2 = mkfile;
    @string mkcounter(@string dir, @string tag, nint nt, nint pid) {
        var h = fnv.New128a();
        h.Write(slice<byte>(tag));
        var hash = h.Sum(default);
        @string fn = fmt.Sprintf(coverage.CounterFileTempl, coverage.CounterFilePref, hash, pid, nt);
        return mkfileʗ2(dir, fn);
    }

    @string trim(@string path) {
        @string b = filepath.Base(path);
        @string d = filepath.Dir(path);
        @string db = filepath.Base(d);
        return db + "/"u8 + b;
    }

    var trimʗ1 = trim;
    @string podToString(pods.Pod p) {
        @string rv = trimʗ1(p.MetaFile) + " [\n"u8;
        foreach (var (k, df) in range(p.CounterDataFiles)) {
            rv += trimʗ1(df);
            if (p.Origins != nil) {
                rv += fmt.Sprintf(" o:%d"u8, p.Origins[k]);
            }
            rv += "\n"u8;
        }
        return rv + "]"u8;
    }

    // Create a couple of directories.
    @string o1 = mkdir("o1", 511);
    @string o2 = mkdir("o2", 511);

    // Add some random files (not coverage related)
    mkfile(o1, blahTxtˢ);
    mkfile(o1, somethingExeˢ);

    // Add a meta-data file with two counter files to first dir.
    mkmeta(o1, "m1");
    mkcounter(o1, "m1", 1, 42);
    mkcounter(o1, "m1", 2, 41);
    mkcounter(o1, "m1", 2, 40);

    // Add a counter file with no associated meta file.
    mkcounter(o1, orphanˢ, 9, 39);

    // Add a meta-data file with three counter files to second dir.
    mkmeta(o2, "m2");
    mkcounter(o2, "m2", 1, 38);
    mkcounter(o2, "m2", 2, 37);
    mkcounter(o2, "m2", 3, 36);

    // Add a duplicate of the first meta-file and a corresponding
    // counter file to the second dir. This is intended to capture
    // the scenario where we have two different runs of the same
    // coverage-instrumented binary, but with the output files
    // sent to separate directories.
    mkmeta(o2, "m1");
    mkcounter(o2, "m1", 11, 35);

    // Collect pods.
    var (podlist, err) = pods.CollectPods([o1, o2], true);
    if (err != nil) {
        t.Fatal(err);
    }

    // Verify pods
    if (len(podlist) != 2) {
        t.Fatalf("expected 2 pods got %d pods", len(podlist));
    }

    foreach (var (k, p) in range(podlist)) {
        t.Logf("%d: mf=%s\n", k, p.MetaFile);
    }

    var expected = slice<@string>([
        """
o1/covmeta.0880952782ab1be95aa0733055a4d06b [
o1/covcounters.0880952782ab1be95aa0733055a4d06b.40.2 o:0
o1/covcounters.0880952782ab1be95aa0733055a4d06b.41.2 o:0
o1/covcounters.0880952782ab1be95aa0733055a4d06b.42.1 o:0
o2/covcounters.0880952782ab1be95aa0733055a4d06b.35.11 o:1
]
"""u8.Raw(),
        """
o2/covmeta.0880952783ab1be95aa0733055a4d1a6 [
o2/covcounters.0880952783ab1be95aa0733055a4d1a6.36.3 o:1
o2/covcounters.0880952783ab1be95aa0733055a4d1a6.37.2 o:1
o2/covcounters.0880952783ab1be95aa0733055a4d1a6.38.1 o:1
]
"""u8.Raw()
    ]);
    foreach (var (k, exp) in range(expected)) {
        @string got = podToString(podlist[k]);
        if (exp != got) {
            t.Errorf("pod %d: expected:\n%s\ngot:\n%s", k, exp, got);
        }
    }

    // Check handling of bad/unreadable dir.
    if (runtime.GOOS == "linux"u8) {
        @string dbad = devNullˢ;
        (_, err) = pods.CollectPods([dbad], true);
        if (err == nil) {
            t.Errorf("executed error due to unreadable dir");
        }
    }
}

} // end pods_test_package
