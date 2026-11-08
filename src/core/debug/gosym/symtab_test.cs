// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using fmt = fmt_package;
using testing = testing_package;
using static go.debug.gosym_package;

partial class gosym_internal_test_package {

internal static partial void assertString(ж<testing.T> t, @string dsc, @string @out, @string tgt) {
    if (@out != tgt) {
        t.Fatalf("Expected: %q Actual: %q for %s", tgt, @out, dsc);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string limitedReaderˢ => ref limitedReaderˢᵇ;
private static readonly @string limitedReaderˢᵇ = "(*LimitedReader)"u8;

public static void TestStandardLibPackage(ж<testing.T> t) {
    var s1 = new Sym(Name: "io.(*LimitedReader).Read");
    var s2 = new Sym(Name: "io.NewSectionReader");
    assertString(t, fmt.Sprintf("package of %q"u8, s1.Name), s1.PackageName(), "io");
    assertString(t, fmt.Sprintf("package of %q"u8, s2.Name), s2.PackageName(), "io");
    assertString(t, fmt.Sprintf("receiver of %q"u8, s1.Name), s1.ReceiverName(), limitedReaderˢ);
    assertString(t, fmt.Sprintf("receiver of %q"u8, s2.Name), s2.ReceiverName(), ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string debugGosymˢ2 => ref debugGosymˢ2ᵇ;
private static readonly @string debugGosymˢ2ᵇ = "debug/gosym"u8;
internal static ref readonly @string lineTableˢ => ref lineTableˢᵇ;
private static readonly @string lineTableˢᵇ = "(*LineTable)"u8;

public static void TestStandardLibPathPackage(ж<testing.T> t) {
    var s1 = new Sym(Name: "debug/gosym.(*LineTable).PCToLine");
    var s2 = new Sym(Name: "debug/gosym.NewTable");
    assertString(t, fmt.Sprintf("package of %q"u8, s1.Name), s1.PackageName(), debugGosymˢ2);
    assertString(t, fmt.Sprintf("package of %q"u8, s2.Name), s2.PackageName(), debugGosymˢ2);
    assertString(t, fmt.Sprintf("receiver of %q"u8, s1.Name), s1.ReceiverName(), lineTableˢ);
    assertString(t, fmt.Sprintf("receiver of %q"u8, s2.Name), s2.ReceiverName(), ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mainˢ => ref mainˢᵇ;
private static readonly @string mainˢᵇ = "main"u8;
internal static ref readonly @string valueIntˢ => ref valueIntˢᵇ;
private static readonly @string valueIntˢᵇ = "(*value[int])"u8;
internal static ref readonly @string setIntˢ => ref setIntˢᵇ;
private static readonly @string setIntˢᵇ = "set[int]"u8;
internal static ref readonly @string getˢ => ref getˢᵇ;
private static readonly @string getˢᵇ = "get"u8;
internal static ref readonly @string absDifferenceCDˢ => ref absDifferenceCDˢᵇ;
private static readonly @string absDifferenceCDˢᵇ = "absDifference[c/d.orderedAbs[float64]]"u8;
internal static ref readonly @string testfunctionShapeIntˢ => ref testfunctionShapeIntˢᵇ;
private static readonly @string testfunctionShapeIntˢᵇ = "testfunction[.shape.int]"u8;

public static void TestGenericNames(ж<testing.T> t) {
    var s1 = new Sym(Name: "main.set[int]");
    var s2 = new Sym(Name: "main.(*value[int]).get");
    var s3 = new Sym(Name: "a/b.absDifference[c/d.orderedAbs[float64]]");
    var s4 = new Sym(Name: "main.testfunction[.shape.int]");
    assertString(t, fmt.Sprintf("package of %q"u8, s1.Name), s1.PackageName(), mainˢ);
    assertString(t, fmt.Sprintf("package of %q"u8, s2.Name), s2.PackageName(), mainˢ);
    assertString(t, fmt.Sprintf("package of %q"u8, s3.Name), s3.PackageName(), "a/b");
    assertString(t, fmt.Sprintf("package of %q"u8, s4.Name), s4.PackageName(), mainˢ);
    assertString(t, fmt.Sprintf("receiver of %q"u8, s1.Name), s1.ReceiverName(), ""u8);
    assertString(t, fmt.Sprintf("receiver of %q"u8, s2.Name), s2.ReceiverName(), valueIntˢ);
    assertString(t, fmt.Sprintf("receiver of %q"u8, s3.Name), s3.ReceiverName(), ""u8);
    assertString(t, fmt.Sprintf("receiver of %q"u8, s4.Name), s4.ReceiverName(), ""u8);
    assertString(t, fmt.Sprintf("base of %q"u8, s1.Name), s1.BaseName(), setIntˢ);
    assertString(t, fmt.Sprintf("base of %q"u8, s2.Name), s2.BaseName(), getˢ);
    assertString(t, fmt.Sprintf("base of %q"u8, s3.Name), s3.BaseName(), absDifferenceCDˢ);
    assertString(t, fmt.Sprintf("base of %q"u8, s4.Name), s4.BaseName(), testfunctionShapeIntˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string githubComDockerDocKerPkgˢ => ref githubComDockerDocKerPkgˢᵇ;
private static readonly @string githubComDockerDocKerPkgˢᵇ = "github.com/docker/doc.ker/pkg/mflag"u8;
internal static ref readonly @string flagSetˢ => ref flagSetˢᵇ;
private static readonly @string flagSetˢᵇ = "(*FlagSet)"u8;

public static void TestRemotePackage(ж<testing.T> t) {
    var s1 = new Sym(Name: "github.com/docker/doc.ker/pkg/mflag.(*FlagSet).PrintDefaults");
    var s2 = new Sym(Name: "github.com/docker/doc.ker/pkg/mflag.PrintDefaults");
    assertString(t, fmt.Sprintf("package of %q"u8, s1.Name), s1.PackageName(), githubComDockerDocKerPkgˢ);
    assertString(t, fmt.Sprintf("package of %q"u8, s2.Name), s2.PackageName(), githubComDockerDocKerPkgˢ);
    assertString(t, fmt.Sprintf("receiver of %q"u8, s1.Name), s1.ReceiverName(), flagSetˢ);
    assertString(t, fmt.Sprintf("receiver of %q"u8, s2.Name), s2.ReceiverName(), ""u8);
}

internal partial struct TestIssue29551_tests /*dyn*/ {
    internal global::go.debug.gosym_package.Sym sym;
    internal @string pkgName;
}

public static void TestIssue29551(ж<testing.T> t) {
    var tests = slice<TestIssue29551_tests>([
        new(new Sym(goVersion: ver120, Name: "type:.eq.[9]debug/elf.intName"), ""),
        new(new Sym(goVersion: ver120, Name: "type:.hash.debug/elf.ProgHeader"), ""),
        new(new Sym(goVersion: ver120, Name: "type:.eq.runtime._panic"), ""),
        new(new Sym(goVersion: ver120, Name: "type:.hash.struct { runtime.gList; runtime.n int32 }"), ""),
        new(new Sym(goVersion: ver120, Name: "go:(*struct { sync.Mutex; math/big.table [64]math/big"), ""),
        new(new Sym(goVersion: ver120, Name: "go.uber.org/zap/buffer.(*Buffer).AppendString"), "go.uber.org/zap/buffer"),
        new(new Sym(goVersion: ver118, Name: "type..eq.[9]debug/elf.intName"), ""),
        new(new Sym(goVersion: ver118, Name: "type..hash.debug/elf.ProgHeader"), ""),
        new(new Sym(goVersion: ver118, Name: "type..eq.runtime._panic"), ""),
        new(new Sym(goVersion: ver118, Name: "type..hash.struct { runtime.gList; runtime.n int32 }"), ""),
        new(new Sym(goVersion: ver118, Name: "go.(*struct { sync.Mutex; math/big.table [64]math/big"), ""),
        // unfortunate
        new(new Sym(goVersion: ver118, Name: "go.uber.org/zap/buffer.(*Buffer).AppendString"), "")
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        var tc = vᴛ1;

        assertString(t, fmt.Sprintf("package of %q"u8, tc.sym.Name), tc.sym.PackageName(), tc.pkgName);
    }
}

} // end gosym_internal_test_package
