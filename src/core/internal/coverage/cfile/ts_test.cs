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
internal static readonly @string testGocoverdirˢ = "test.gocoverdir"u8;

internal static @string testGoCoverDir(ж<testing.T> Ꮡt) {
    {
        var f = flag.Lookup(testGocoverdirˢ); if (f != nil) {
            {
                @string dir = (~f).Value.String(); if (dir != ""u8) {
                    return dir;
                }
            }
        }
    }
    return Ꮡt.TempDir();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileTxtˢ = "file.txt"u8;
internal static readonly @string ofStatementsˢ = "of statements"u8;

// TestTestSupport does a basic verification of the functionality in
// ProcessCoverTestDir (doing this here as opposed to
// relying on other test paths will provide a better signal when
// running "go test -cover" for this package).
public static void TestTestSupport(ж<testing.T> Ꮡt) {
    if (testing.CoverMode() == ""u8) {
        return;
    }
    @string tgcd = testGoCoverDir(Ꮡt);
    Ꮡt.Logf("testing.testGoCoverDir() returns %s mode=%s\n"u8,
        tgcd, testing.CoverMode());

    @string textfile = filepath.Join(Ꮡt.TempDir(), fileTxtˢ);
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    var err = ProcessCoverTestDir(tgcd, textfile,
        testing.CoverMode(), ""u8, new cfile_internal_test_package.strings_BuilderжWriter(Ꮡsb), default!);
    if (err != default!) {
        Ꮡt.Fatalf("bad: %v"u8, err);
    }

    // Check for existence of text file.
    {
        var (inf, errΔ1) = os.Open(textfile); if (errΔ1 != default!){
            Ꮡt.Fatalf("problems opening text file %s: %v"u8, textfile, errΔ1);
        } else {
            inf.Close();
        }
    }

    // Check for percent output with expected tokens.
    @string strout = sb.String();
    @string want = ofStatementsˢ;
    if (!strings.Contains(strout, want)) {
        Ꮡt.Logf("output from run: %s\n"u8, strout);
        Ꮡt.Fatalf("percent output missing token: %q"u8, want);
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
public static void TestCoverageSnapshot(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoRun(new cfile_internal_test_package.testing_TжTB(Ꮡt));
    var args = new @string[]{"test"u8, "-tags"u8, "SELECT_USING_THIS_TAG"u8,
        "-cover"u8, "-run=TestCoverageSnapshotImpl"u8, "internal/coverage/cfile"u8}.slice();
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(Ꮡt)), args.ꓸꓸꓸ);
    {
        var (b, err) = cmd.CombinedOutput(); if (err != default!) {
            Ꮡt.Fatalf("go test failed (%v): %s"u8, err, b);
        }
    }
}

internal static readonly @string hellogo = """

package main

func main() {
  println("hello")
}

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloGoˢ = "hello.go"u8;
internal static readonly @string covmetaˢ = "covmeta"u8;
internal static readonly @string helloGoˢ2 = "hello.go:"u8;

// Returns a pair F,T where F is a meta-data file generated from
// "hello.go" above, and T is a token to look for that should be
// present in the coverage report from F.
internal static (@string, @string) genAuxMeta(ж<testing.T> Ꮡt, @string dstdir) {
    // Do a GOCOVERDIR=<tmp> go run hello.go
    @string src = filepath.Join(dstdir, helloGoˢ);
    {
        var errΔ1 = os.WriteFile(src, slice<byte>(hellogo), 511); if (errΔ1 != default!) {
            Ꮡt.Fatalf("write failed: %v"u8, errΔ1);
        }
    }
    var args = new @string[]{"run"u8, "-covermode="u8 + testing.CoverMode(), src}.slice();
    var cmd = exec.Command(testenv.GoToolPath(new cfile_internal_test_package.testing_TжTB(Ꮡt)), args.ꓸꓸꓸ);
    cmd.Value.Env = updateGoCoverDir(os.Environ(), dstdir, true);
    {
        var (b, errΔ2) = cmd.CombinedOutput(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("go run failed (%v): %s"u8, errΔ2, b);
        }
    }

    // Pick out the generated meta-data file.
    var (files, err) = os.ReadDir(dstdir);
    if (err != default!) {
        Ꮡt.Fatalf("reading %s: %v"u8, dstdir, err);
    }
    foreach (var (_, f) in files) {
        if (strings.HasPrefix(f.Name(), covmetaˢ)) {
            return (filepath.Join(dstdir, f.Name()), helloGoˢ2);
        }
    }
    Ꮡt.Fatalf("could not locate generated meta-data file"u8);
    return ("", "");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string othermetaˢ = "othermeta"u8;
internal static readonly @string file2Txtˢ = "file2.txt"u8;

public static void TestAuxMetaDataFiles(ж<testing.T> Ꮡt) {
    if (testing.CoverMode() == ""u8) {
        return;
    }
    testenv.MustHaveGoRun(new cfile_internal_test_package.testing_TжTB(Ꮡt));
    @string tgcd = testGoCoverDir(Ꮡt);
    Ꮡt.Logf("testing.testGoCoverDir() returns %s mode=%s\n"u8,
        tgcd, testing.CoverMode());

    @string td = Ꮡt.TempDir();

    // Manufacture a new, separate meta-data file not related to this
    // test. Contents are not important, just so long as the
    // packages/paths are different.
    @string othermetadir = filepath.Join(td, othermetaˢ);
    {
        var errΔ1 = os.Mkdir(othermetadir, 511); if (errΔ1 != default!) {
            Ꮡt.Fatalf("mkdir failed: %v"u8, errΔ1);
        }
    }
    var (mfile, token) = genAuxMeta(Ꮡt, othermetadir);

    // Write a metafiles file.
    @string metafiles = filepath.Join(tgcd, coverage.MetaFilesFileName);
    var mfc = new coverage.MetaFileCollection(
        ImportPaths: new @string[]{"command-line-arguments"u8}.slice(),
        MetaFileFragments: new @string[]{mfile}.slice()
    );
    var (jdata, err) = json.Marshal(mfc);
    if (err != default!) {
        Ꮡt.Fatalf("marshal MetaFileCollection: %v"u8, err);
    }
    {
        var errΔ2 = os.WriteFile(metafiles, jdata, 438); if (errΔ2 != default!) {
            Ꮡt.Fatalf("write failed: %v"u8, errΔ2);
        }
    }

    // Kick off guts of test.
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    @string textfile = filepath.Join(td, file2Txtˢ);
    err = ProcessCoverTestDir(tgcd, textfile,
        testing.CoverMode(), ""u8, new cfile_internal_test_package.strings_BuilderжWriter(Ꮡsb), default!);
    if (err != default!) {
        Ꮡt.Fatalf("bad: %v"u8, err);
    }
    {
        err = os.Remove(metafiles); if (err != default!) {
            Ꮡt.Fatalf("removing metafiles file: %v"u8, err);
        }
    }

    // Look for the expected things in the coverage profile.
    (var contents, err) = os.ReadFile(textfile);
    @string strc = (@string)contents;
    if (err != default!) {
        Ꮡt.Fatalf("problems reading text file %s: %v"u8, textfile, err);
    }
    if (!strings.Contains(strc, token)) {
        Ꮡt.Logf("content: %s\n"u8, (@string)contents);
        Ꮡt.Fatalf("cov profile does not contain aux meta content %q"u8, token);
    }
}

} // end cfile_internal_test_package
