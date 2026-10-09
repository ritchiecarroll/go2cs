// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using fmt = fmt_package;
using testing = testing_package;
using static go.debug.gosym_package;

partial class gosym_internal_test_package {

internal static void assertString(ж<testing.T> Ꮡt, @string dsc, @string @out, @string tgt) {
    if (@out != tgt) {
        Ꮡt.Fatalf("Expected: %q Actual: %q for %s"u8, tgt, @out, dsc);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string limitedReaderˢ = "(*LimitedReader)"u8;

public static void TestStandardLibPackage(ж<testing.T> Ꮡt) {
    var s1 = new Sym(Name: "io.(*LimitedReader).Read"u8);
    var s2 = new Sym(Name: "io.NewSectionReader"u8);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s1.Name), s1.PackageName(), "io"u8);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s2.Name), s2.PackageName(), "io"u8);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s1.Name), s1.ReceiverName(), limitedReaderˢ);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s2.Name), s2.ReceiverName(), ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string debugGosymˢ2 = "debug/gosym"u8;
internal static readonly @string lineTableˢ = "(*LineTable)"u8;

public static void TestStandardLibPathPackage(ж<testing.T> Ꮡt) {
    var s1 = new Sym(Name: "debug/gosym.(*LineTable).PCToLine"u8);
    var s2 = new Sym(Name: "debug/gosym.NewTable"u8);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s1.Name), s1.PackageName(), debugGosymˢ2);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s2.Name), s2.PackageName(), debugGosymˢ2);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s1.Name), s1.ReceiverName(), lineTableˢ);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s2.Name), s2.ReceiverName(), ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mainˢ = "main"u8;
internal static readonly @string valueIntˢ = "(*value[int])"u8;
internal static readonly @string setIntˢ = "set[int]"u8;
internal static readonly @string getˢ = "get"u8;
internal static readonly @string absDifferenceCDˢ = "absDifference[c/d.orderedAbs[float64]]"u8;
internal static readonly @string testfunctionShapeIntˢ = "testfunction[.shape.int]"u8;

public static void TestGenericNames(ж<testing.T> Ꮡt) {
    var s1 = new Sym(Name: "main.set[int]"u8);
    var s2 = new Sym(Name: "main.(*value[int]).get"u8);
    var s3 = new Sym(Name: "a/b.absDifference[c/d.orderedAbs[float64]]"u8);
    var s4 = new Sym(Name: "main.testfunction[.shape.int]"u8);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s1.Name), s1.PackageName(), mainˢ);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s2.Name), s2.PackageName(), mainˢ);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s3.Name), s3.PackageName(), "a/b"u8);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s4.Name), s4.PackageName(), mainˢ);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s1.Name), s1.ReceiverName(), ""u8);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s2.Name), s2.ReceiverName(), valueIntˢ);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s3.Name), s3.ReceiverName(), ""u8);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s4.Name), s4.ReceiverName(), ""u8);
    assertString(Ꮡt, fmt.Sprintf("base of %q"u8, s1.Name), s1.BaseName(), setIntˢ);
    assertString(Ꮡt, fmt.Sprintf("base of %q"u8, s2.Name), s2.BaseName(), getˢ);
    assertString(Ꮡt, fmt.Sprintf("base of %q"u8, s3.Name), s3.BaseName(), absDifferenceCDˢ);
    assertString(Ꮡt, fmt.Sprintf("base of %q"u8, s4.Name), s4.BaseName(), testfunctionShapeIntˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string githubComDockerDocKerPkgˢ = "github.com/docker/doc.ker/pkg/mflag"u8;
internal static readonly @string flagSetˢ = "(*FlagSet)"u8;

public static void TestRemotePackage(ж<testing.T> Ꮡt) {
    var s1 = new Sym(Name: "github.com/docker/doc.ker/pkg/mflag.(*FlagSet).PrintDefaults"u8);
    var s2 = new Sym(Name: "github.com/docker/doc.ker/pkg/mflag.PrintDefaults"u8);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s1.Name), s1.PackageName(), githubComDockerDocKerPkgˢ);
    assertString(Ꮡt, fmt.Sprintf("package of %q"u8, s2.Name), s2.PackageName(), githubComDockerDocKerPkgˢ);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s1.Name), s1.ReceiverName(), flagSetˢ);
    assertString(Ꮡt, fmt.Sprintf("receiver of %q"u8, s2.Name), s2.ReceiverName(), ""u8);
}

internal partial struct TestIssue29551_tests /*dyn*/ {
    internal global::go.debug.gosym_package.Sym sym;
    internal @string pkgName;
}

public static void TestIssue29551(ж<testing.T> Ꮡt) {
    var tests = new TestIssue29551_tests[]{
        new(new Sym(goVersion: ver120, Name: "type:.eq.[9]debug/elf.intName"u8), ""u8),
        new(new Sym(goVersion: ver120, Name: "type:.hash.debug/elf.ProgHeader"u8), ""u8),
        new(new Sym(goVersion: ver120, Name: "type:.eq.runtime._panic"u8), ""u8),
        new(new Sym(goVersion: ver120, Name: "type:.hash.struct { runtime.gList; runtime.n int32 }"u8), ""u8),
        new(new Sym(goVersion: ver120, Name: "go:(*struct { sync.Mutex; math/big.table [64]math/big"u8), ""u8),
        new(new Sym(goVersion: ver120, Name: "go.uber.org/zap/buffer.(*Buffer).AppendString"u8), "go.uber.org/zap/buffer"u8),
        new(new Sym(goVersion: ver118, Name: "type..eq.[9]debug/elf.intName"u8), ""u8),
        new(new Sym(goVersion: ver118, Name: "type..hash.debug/elf.ProgHeader"u8), ""u8),
        new(new Sym(goVersion: ver118, Name: "type..eq.runtime._panic"u8), ""u8),
        new(new Sym(goVersion: ver118, Name: "type..hash.struct { runtime.gList; runtime.n int32 }"u8), ""u8),
        new(new Sym(goVersion: ver118, Name: "go.(*struct { sync.Mutex; math/big.table [64]math/big"u8), ""u8), // unfortunate

        new(new Sym(goVersion: ver118, Name: "go.uber.org/zap/buffer.(*Buffer).AppendString"u8), ""u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        var tc = vᴛ1;

        assertString(Ꮡt, fmt.Sprintf("package of %q"u8, tc.sym.Name), tc.sym.PackageName(), tc.pkgName);
    }
}

} // end gosym_internal_test_package
