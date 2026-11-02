// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using abi = go.@internal.abi_package;
using testenv = go.@internal.testenv_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using exec = os.exec_package;
using go.@internal;
using os;
using path;
using static go.@internal.abi_internal_test_package;

partial class abi_test_package {

public static void TestFuncPC(ж<testing.T> t) {
    // Test that FuncPC* can get correct function PC.
    var pcFromAsm = abi_internal_test_package.FuncPCTestFnAddr;

    // Test FuncPC for locally defined function
    var pcFromGo = abi_internal_test_package.FuncPCTest();
    if (pcFromGo != pcFromAsm) {
        t.Errorf("FuncPC returns wrong PC, want %x, got %x", pcFromAsm, pcFromGo);
    }

    // Test FuncPC for imported function
    pcFromGo = abi.FuncPCABI0(abi_internal_test_package.FuncPCTestFn);
    if (pcFromGo != pcFromAsm) {
        t.Errorf("FuncPC returns wrong PC, want %x, got %x", pcFromAsm, pcFromGo);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
internal static ref readonly @string xGoˢ => ref xGoˢᵇ;
private static readonly @string xGoˢᵇ = "x.go"u8;
internal static ref readonly @string symabiˢ => ref symabiˢᵇ;
private static readonly @string symabiˢᵇ = "symabi"u8;
internal static ref readonly @string helloImportcfgˢ => ref helloImportcfgˢᵇ;
private static readonly @string helloImportcfgˢᵇ = "hello.importcfg"u8;
internal static ref readonly @string internalAbiˢ => ref internalAbiˢᵇ;
private static readonly @string internalAbiˢᵇ = "internal/abi"u8;
internal static ref readonly @string toolˢ => ref toolˢᵇ;
private static readonly @string toolˢᵇ = "tool"u8;
internal static ref readonly @string asmˢ => ref asmˢᵇ;
private static readonly @string asmˢᵇ = "asm"u8;
internal static ref readonly @string gensymabisˢ => ref gensymabisˢᵇ;
private static readonly @string gensymabisˢᵇ = "-gensymabis"u8;
internal static ref readonly @string compileˢ => ref compileˢᵇ;
private static readonly @string compileˢᵇ = "compile"u8;
internal static ref readonly @string symabisˢ => ref symabisˢᵇ;
private static readonly @string symabisˢᵇ = "-symabis"u8;

public static void TestFuncPCCompileError(ж<testing.T> t) {
    // Test that FuncPC* on a function of a mismatched ABI is rejected.
    testenv.MustHaveGoBuild(new abi_test_package.testing_TжTB(t));

    // We want to test internal package, which we cannot normally import.
    // Run the assembler and compiler manually.
    @string tmpdir = t.TempDir();
    @string asmSrc = filepath.Join(testdataˢ, "x.s");
    @string goSrc = filepath.Join(testdataˢ, xGoˢ);
    @string symabi = filepath.Join(tmpdir, symabiˢ);
    @string obj = filepath.Join(tmpdir, "x.o");

    // Write an importcfg file for the dependencies of the package.
    @string importcfgfile = filepath.Join(tmpdir, helloImportcfgˢ);
    testenv.WriteImportcfg(new abi_test_package.testing_TжTB(t), importcfgfile, default, internalAbiˢ);

    // parse assembly code for symabi.
    var cmd = testenv.Command(new abi_test_package.testing_TжTB(t), testenv.GoToolPath(new abi_test_package.testing_TжTB(t)), toolˢ, asmˢ, "-p=p", gensymabisˢ, "-o", symabi, asmSrc);
    var (@out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("go tool asm -gensymabis failed: %v\n%s", err, @out);
    }

    // compile go code.
    cmd = testenv.Command(new abi_test_package.testing_TжTB(t), testenv.GoToolPath(new abi_test_package.testing_TжTB(t)), toolˢ, compileˢ, "-importcfg=" + importcfgfile, "-p=p", symabisˢ, symabi, "-o", obj, goSrc);
    (@out, err) = cmd.CombinedOutput();
    if (err == nil) {
        t.Fatalf("go tool compile did not fail");
    }

    // Expect errors in line 17, 18, 20, no errors on other lines.
    var want = slice<@string>(["x.go:17", "x.go:18", "x.go:20"]);
    var got = strings.Split((@string)@out, "\n");
    if (got[len(got) - 1] == "") {
        got = got.slice(0, len(got) - 1); // remove last empty line
    }
    foreach (var (i, s) in got) {
        if (!strings.Contains(s, want[i])) {
            t.Errorf("did not error on line %s", want[i]);
        }
    }
    if (len(got) != len(want)) {
        t.Errorf("unexpected number of errors, want %d, got %d", len(want), len(got));
    }
    if (t.Failed()) {
        t.Logf("output:\n%s", (@string)@out);
    }
}

} // end abi_test_package
