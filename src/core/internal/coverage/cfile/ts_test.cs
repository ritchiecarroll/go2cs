// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using json = encoding.json_package;
using flag = flag_package;
using coverage = go.@internal.coverage_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using encoding;
using fs = go.io.fs_package;
using go.@internal;
using go.io;
using go.os;
using io = io_package;
using path;
using static go.@internal.coverage.cfile_package;

partial class cfile_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testGocoverdirˢ => ref testGocoverdirˢᵇ;
private static readonly @string testGocoverdirˢᵇ = "test.gocoverdir"u8;

internal static @string testGoCoverDir(ж<testing.T> t) {
    {
        var f = flag.Lookup(testGocoverdirˢ); if (f != nil) {
            {
                @string dir = (~f).Value.String(); if (dir != ""u8) {
                    return dir;
                }
            }
        }
    }
    return t.TempDir();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileTxtˢ => ref fileTxtˢᵇ;
private static readonly @string fileTxtˢᵇ = "file.txt"u8;
internal static ref readonly @string ofStatementsˢ => ref ofStatementsˢᵇ;
private static readonly @string ofStatementsˢᵇ = "of statements"u8;

// TestTestSupport does a basic verification of the functionality in
// ProcessCoverTestDir (doing this here as opposed to
// relying on other test paths will provide a better signal when
// running "go test -cover" for this package).
public static void TestTestSupport(ж<testing.T> t) {
    if (testing.CoverMode() == ""u8) {
        return;
    }
    @string tgcd = testGoCoverDir(t);
    t.Logf("testing.testGoCoverDir() returns %s mode=%s\n",
        tgcd, testing.CoverMode());

    @string textfile = filepath.Join(t.TempDir(), fileTxtˢ);
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    var err = ProcessCoverTestDir(tgcd, textfile,
        testing.CoverMode(), ""u8, new cfile_internal_test_package.strings_BuilderжWriter(Ꮡsb), default);
    if (err != nil) {
        t.Fatalf("bad: %v", err);
    }

    // Check for existence of text file.
    {
        var (inf, errΔ1) = os.Open(textfile); if (errΔ1 != nil) {
            t.Fatalf("problems opening text file %s: %v", textfile, errΔ1);
        } else {
            inf.Close();
        }
    }

    // Check for percent output with expected tokens.
    @string strout = sb.String();
    @string want = ofStatementsˢ;
    if (!strings.Contains(strout, want)) {
        t.Logf("output from run: %s\n", strout);
        t.Fatalf("percent output missing token: %q", want);
    }
}

// Kicks off a sub-test to verify that Snapshot() works properly.
// We do this as a separate shell-out, so as to avoid potential
// interactions with -coverpkg. For example, if you do
//
//	$ cd `go env GOROOT`
//	$ cd src/internal/coverage
//	$ go test -coverpkg=internal/coverage/decodecounter ./...
//	...
//	$
//
// The previous version of this test could fail due to the fact
// that "cfile" itself was not being instrumented, as in the
// scenario above.
public static void TestCoverageSnapshot(ж<testing.T> t) {
    testenv.MustHaveGoRun(new cfile_internal_test_package.testing_TжTB(t));
    var args = slice<@string>(["test", "-tags", "SELECT_USING_THIS_TAG",
        "-cover", "-run=TestCoverageSnapshotImpl", "internal/coverage/cfile"]);
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(t)), args.ꓸꓸꓸ);
    {
        var (b, err) = cmd.CombinedOutput(); if (err != nil) {
            t.Fatalf("go test failed (%v): %s", err, b);
        }
    }
}

internal static ref readonly @string hellogo => ref hellogoᵇ;
private static readonly @string hellogoᵇ = """

package main

func main() {
  println("hello")
}

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloGoˢ => ref helloGoˢᵇ;
private static readonly @string helloGoˢᵇ = "hello.go"u8;
internal static ref readonly @string covmetaˢ => ref covmetaˢᵇ;
private static readonly @string covmetaˢᵇ = "covmeta"u8;
internal static ref readonly @string helloGoˢ2 => ref helloGoˢ2ᵇ;
private static readonly @string helloGoˢ2ᵇ = "hello.go:"u8;

// Returns a pair F,T where F is a meta-data file generated from
// "hello.go" above, and T is a token to look for that should be
// present in the coverage report from F.
internal static (@string, @string) genAuxMeta(ж<testing.T> t, @string dstdir) {
    // Do a GOCOVERDIR=<tmp> go run hello.go
    @string src = filepath.Join(dstdir, helloGoˢ);
    {
        var errΔ1 = os.WriteFile(src, slice<byte>(hellogo), 511); if (errΔ1 != nil) {
            t.Fatalf("write failed: %v", errΔ1);
        }
    }
    var args = slice<@string>(["run", "-covermode="u8 + testing.CoverMode(), src]);
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(t)), args.ꓸꓸꓸ);
    cmd.Env = updateGoCoverDir(os.Environ(), dstdir, true);
    {
        var (b, errΔ2) = cmd.CombinedOutput(); if (errΔ2 != nil) {
            t.Fatalf("go run failed (%v): %s", errΔ2, b);
        }
    }

    // Pick out the generated meta-data file.
    var (files, err) = os.ReadDir(dstdir);
    if (err != nil) {
        t.Fatalf("reading %s: %v", dstdir, err);
    }
    foreach (var (_, f) in files) {
        if (strings.HasPrefix(f.Name(), covmetaˢ)) {
            return (filepath.Join(dstdir, f.Name()), helloGoˢ2);
        }
    }
    t.Fatalf("could not locate generated meta-data file");
    return ("", "");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string othermetaˢ => ref othermetaˢᵇ;
private static readonly @string othermetaˢᵇ = "othermeta"u8;
internal static ref readonly @string file2Txtˢ => ref file2Txtˢᵇ;
private static readonly @string file2Txtˢᵇ = "file2.txt"u8;

public static void TestAuxMetaDataFiles(ж<testing.T> t) {
    if (testing.CoverMode() == ""u8) {
        return;
    }
    testenv.MustHaveGoRun(new cfile_internal_test_package.testing_TжTB(t));
    @string tgcd = testGoCoverDir(t);
    t.Logf("testing.testGoCoverDir() returns %s mode=%s\n",
        tgcd, testing.CoverMode());

    @string td = t.TempDir();

    // Manufacture a new, separate meta-data file not related to this
    // test. Contents are not important, just so long as the
    // packages/paths are different.
    @string othermetadir = filepath.Join(td, othermetaˢ);
    {
        var errΔ1 = os.Mkdir(othermetadir, 511); if (errΔ1 != nil) {
            t.Fatalf("mkdir failed: %v", errΔ1);
        }
    }
    var (mfile, token) = genAuxMeta(t, othermetadir);

    // Write a metafiles file.
    @string metafiles = filepath.Join(tgcd, coverage.MetaFilesFileName);
    var mfc = new coverage.MetaFileCollection(
        ImportPaths: slice<@string>(["command-line-arguments"]),
        MetaFileFragments: slice<@string>([mfile])
    );
    var (jdata, err) = json.Marshal(mfc);
    if (err != nil) {
        t.Fatalf("marshal MetaFileCollection: %v", err);
    }
    {
        var errΔ2 = os.WriteFile(metafiles, jdata, 438); if (errΔ2 != nil) {
            t.Fatalf("write failed: %v", errΔ2);
        }
    }

    // Kick off guts of test.
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    @string textfile = filepath.Join(td, file2Txtˢ);
    err = ProcessCoverTestDir(tgcd, textfile,
        testing.CoverMode(), ""u8, new cfile_internal_test_package.strings_BuilderжWriter(Ꮡsb), default);
    if (err != nil) {
        t.Fatalf("bad: %v", err);
    }
    {
        err = os.Remove(metafiles); if (err != nil) {
            t.Fatalf("removing metafiles file: %v", err);
        }
    }

    // Look for the expected things in the coverage profile.
    (var contents, err) = os.ReadFile(textfile);
    @string strc = (@string)contents;
    if (err != nil) {
        t.Fatalf("problems reading text file %s: %v", textfile, err);
    }
    if (!strings.Contains(strc, token)) {
        t.Logf("content: %s\n", (@string)contents);
        t.Fatalf("cov profile does not contain aux meta content %q", token);
    }
}

} // end cfile_internal_test_package
