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

public static void TestFuncPC(ж<testing.T> Ꮡt) {
    // Test that FuncPC* can get correct function PC.
    var pcFromAsm = abi_internal_test_package.FuncPCTestFnAddr;
    // Test FuncPC for locally defined function
    var pcFromGo = abi_internal_test_package.FuncPCTest();
    if (pcFromGo != pcFromAsm) {
        Ꮡt.Errorf("FuncPC returns wrong PC, want %x, got %x"u8, pcFromAsm, pcFromGo);
    }
    // Test FuncPC for imported function
    pcFromGo = abi.FuncPCABI0(abi_internal_test_package.FuncPCTestFn);
    if (pcFromGo != pcFromAsm) {
        Ꮡt.Errorf("FuncPC returns wrong PC, want %x, got %x"u8, pcFromAsm, pcFromGo);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "testdata"u8;
internal static readonly @string xGoˢ = "x.go"u8;
internal static readonly @string symabiˢ = "symabi"u8;
internal static readonly @string helloImportcfgˢ = "hello.importcfg"u8;
internal static readonly @string internalAbiˢ = "internal/abi"u8;
internal static readonly @string toolˢ = "tool"u8;
internal static readonly @string asmˢ = "asm"u8;
internal static readonly @string gensymabisˢ = "-gensymabis"u8;
internal static readonly @string compileˢ = "compile"u8;
internal static readonly @string symabisˢ = "-symabis"u8;

public static void TestFuncPCCompileError(ж<testing.T> Ꮡt) {
    // Test that FuncPC* on a function of a mismatched ABI is rejected.
    testenv.MustHaveGoBuild(new abi_test_package.testing_TжTB(Ꮡt));
    // We want to test internal package, which we cannot normally import.
    // Run the assembler and compiler manually.
    @string tmpdir = Ꮡt.TempDir();
    @string asmSrc = filepath.Join(testdataˢ, "x.s");
    @string goSrc = filepath.Join(testdataˢ, xGoˢ);
    @string symabi = filepath.Join(tmpdir, symabiˢ);
    @string obj = filepath.Join(tmpdir, "x.o");
    // Write an importcfg file for the dependencies of the package.
    @string importcfgfile = filepath.Join(tmpdir, helloImportcfgˢ);
    testenv.WriteImportcfg(new abi_test_package.testing_TжTB(Ꮡt), importcfgfile, default!, internalAbiˢ);
    // parse assembly code for symabi.
    var cmd = testenv.Command(new abi_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new abi_test_package.testing_TжTB(Ꮡt)), toolˢ, asmˢ, "-p=p", gensymabisˢ, "-o", symabi, asmSrc);
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go tool asm -gensymabis failed: %v\n%s"u8, err, @out);
    }
    // compile go code.
    cmd = testenv.Command(new abi_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new abi_test_package.testing_TжTB(Ꮡt)), toolˢ, compileˢ, "-importcfg=" + importcfgfile, "-p=p", symabisˢ, symabi, "-o", obj, goSrc);
    (@out, err) = cmd.CombinedOutput();
    if (err == default!) {
        Ꮡt.Fatalf("go tool compile did not fail"u8);
    }
    // Expect errors in line 17, 18, 20, no errors on other lines.
    var want = new @string[]{"x.go:17"u8, "x.go:18"u8, "x.go:20"u8}.slice();
    var got = strings.Split(((@string)@out), "\n"u8);
    if (got[len(got) - 1] == "") {
        got = got.slice(0, len(got) - 1); // remove last empty line
    }
    foreach (var (i, s) in got) {
        if (!strings.Contains(s, want[i])) {
            Ꮡt.Errorf("did not error on line %s"u8, want[i]);
        }
    }
    if (len(got) != len(want)) {
        Ꮡt.Errorf("unexpected number of errors, want %d, got %d"u8, len(want), len(got));
    }
    if (Ꮡt.Failed()) {
        Ꮡt.Logf("output:\n%s"u8, ((@string)@out));
    }
}

} // end abi_test_package
