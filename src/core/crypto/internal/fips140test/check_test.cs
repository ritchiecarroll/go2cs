// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140test;

using fips140 = go.crypto.@internal.fips140_package;
using static go.crypto.@internal.fips140.check_package;
using checktest = go.crypto.@internal.fips140.check.checktest_package;
using fmt = fmt_package;
using abi = go.@internal.abi_package;
using godebug = go.@internal.godebug_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using testing = testing_package;
using unicode = unicode_package;
using @unsafe = unsafe_package;
using exec = go.os.exec_package;
using go.@internal;
using go.crypto.@internal;
using go.crypto.@internal.fips140.check;
using go.os;
using ꓸꓸꓸnint = Span<nint>;

partial class fipstest_internal_test_package {

internal const bool enableFIPSTest = true;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fips140ˢ = "fips140"u8;
internal static readonly @string testRunTestFIPSCheckˢ = "-test.run=TestFIPSCheck"u8;

public static void TestFIPSCheckVerify(ж<testing.T> Ꮡt) {
    if (Verified) {
        Ꮡt.Logf("verified"u8);
        return;
    }
    if (godebug.New(fips140ˢ).Value() == "on"u8) {
        Ꮡt.Fatalf("GODEBUG=fips140=on but verification did not run"u8);
    }
    if (!enableFIPSTest) {
        return;
    }
    {
        var errΔ1 = fips140.Supported(); if (errΔ1 != default!) {
            Ꮡt.Skipf("skipping: %v"u8, errΔ1);
        }
    }
    var cmd = testenv.Command(new fipstest_internal_test_package.testing_TжTB(Ꮡt), os.Args[0], testVˢ, testRunTestFIPSCheckˢ);
    cmd.Value.Env = append(cmd.Environ(), "GODEBUG=fips140=on"u8);
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("GODEBUG=fips140=on %v failed: %v\n%s"u8, (~cmd).Args, err, @out);
    }
    Ꮡt.Logf("exec'ed GODEBUG=fips140=on and succeeded:\n%s"u8, @out);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string textˢ = "TEXT"u8;
internal static readonly @string staticTextˢ = "StaticText"u8;
internal static readonly @string rodataˢ = "RODATA"u8;
internal static readonly @string noptrdataˢ = "NOPTRDATA"u8;
internal static readonly @string staticDataˢ = "StaticData"u8;
internal static readonly @string dataˢ = "DATA"u8;
internal static readonly @string checktestTextˢ = "checktest.TEXT"u8;
internal static readonly @string checktestRodataˢ = "checktest.RODATA"u8;
internal static readonly @string checktestNoptrdataˢ = "checktest.NOPTRDATA"u8;
internal static readonly @string checktestDataˢ = "checktest.DATA"u8;
internal static readonly @string fmtPrintfˢ = "fmt.Printf"u8;
internal static readonly @string unicodeCategoriesˢ = "unicode.Categories"u8;
internal static readonly @string unicodeAsciiHexDigitˢ = "unicode.ASCII_Hex_Digit"u8;

public static void TestFIPSCheckInfo(ж<testing.T> Ꮡt) {
    if (!enableFIPSTest) {
        return;
    }
    {
        var err = fips140.Supported(); if (err != default!) {
            Ꮡt.Skipf("skipping: %v"u8, err);
        }
    }
    // Check that the checktest symbols are initialized properly.
    if (checktest.NOPTRDATA != 1) {
        Ꮡt.Errorf("checktest.NOPTRDATA = %d, want 1"u8, checktest.NOPTRDATA);
    }
    if (checktest.RODATA != 2) {
        Ꮡt.Errorf("checktest.RODATA = %d, want 2"u8, checktest.RODATA);
    }
    if (checktest.DATA.P != checktest.ᏑNOPTRDATA) {
        Ꮡt.Errorf("checktest.DATA.P = %p, want &checktest.NOPTRDATA (%p)"u8, checktest.DATA.P.OrTypedNil(), checktest.ᏑNOPTRDATA);
    }
    if (checktest.DATA.X != 3) {
        Ꮡt.Errorf("checktest.DATA.X = %d, want 3"u8, checktest.DATA.X);
    }
    if (checktest.NOPTRBSS != 0) {
        Ꮡt.Errorf("checktest.NOPTRBSS = %d, want 0"u8, checktest.NOPTRBSS);
    }
    if (checktest.BSS != nil) {
        Ꮡt.Errorf("checktest.BSS = %p, want nil"u8, checktest.BSS.OrTypedNil());
    }
    {
        var p = checktest.PtrStaticData(); if (p != nil && p.Value != 10) {
            Ꮡt.Errorf("*checktest.PtrStaticData() = %d, want 10"u8, p.Value);
        }
    }
    // Check that the checktest symbols are in the right go:fipsinfo sections.
    void sect(nint i, @string name, @unsafe.Pointer p) {
        var s = Linkinfo.Sects[i];
        if (!((uintptr)s.Start <= (uintptr)p && (uintptr)p < (uintptr)s.End)) {
            Ꮡt.Errorf("checktest.%s (%#x) not in section #%d (%#x..%#x)"u8, name, @unsafe.Pointer.OrTypedNil(p), i, @unsafe.Pointer.OrTypedNil(s.Start), @unsafe.Pointer.OrTypedNil(s.End));
        }
    }
    sect(0, textˢ, (@unsafe.Pointer)abi.FuncPCABIInternal(checktest.TEXT));
    {
        @unsafe.Pointer p = (uintptr)checktest.PtrStaticText(); if (p != nil) {
            sect(0, staticTextˢ, p);
        }
    }
    sect(1, rodataˢ, @unsafe.Pointer.FromPinnedBox(Ꮡ(checktest.RODATA)));
    sect(2, noptrdataˢ, @unsafe.Pointer.FromPinnedBox(checktest.ᏑNOPTRDATA));
    {
        var p = checktest.PtrStaticData(); if (p != nil) {
            sect(2, staticDataˢ, @unsafe.Pointer.FromPinnedBox(p));
        }
    }
    sect(3, dataˢ, @unsafe.Pointer.FromPinnedBox(Ꮡ(checktest.DATA)));
    // Check that some symbols are not in FIPS sections.
    void no(@string name, @unsafe.Pointer p, params ꓸꓸꓸnint ixʗp) {
        var ix = ixʗp.sslice();
        foreach (var (_, i) in ix) {
            var s = Linkinfo.Sects[i];
            if ((uintptr)s.Start <= (uintptr)p && (uintptr)p < (uintptr)s.End) {
                Ꮡt.Errorf("%s (%#x) unexpectedly in section #%d (%#x..%#x)"u8, name, @unsafe.Pointer.OrTypedNil(p), i, @unsafe.Pointer.OrTypedNil(s.Start), @unsafe.Pointer.OrTypedNil(s.End));
            }
        }
    }
    // Check that the symbols are not in unexpected sections (that is, no overlaps).
    no(checktestTextˢ, (@unsafe.Pointer)abi.FuncPCABIInternal(checktest.TEXT), 1, 2, 3);
    no(checktestRodataˢ, @unsafe.Pointer.FromPinnedBox(Ꮡ(checktest.RODATA)), 0, 2, 3);
    no(checktestNoptrdataˢ, @unsafe.Pointer.FromPinnedBox(checktest.ᏑNOPTRDATA), 0, 1, 3);
    no(checktestDataˢ, @unsafe.Pointer.FromPinnedBox(Ꮡ(checktest.DATA)), 0, 1, 2);
    // Check that non-FIPS symbols are not in any of the sections.
    no(fmtPrintfˢ, (@unsafe.Pointer)abi.FuncPCABIInternal(fmt.Printfᶠ), 0, 1, 2, 3); // TEXT
    no(unicodeCategoriesˢ, @unsafe.Pointer.FromPinnedBox(Ꮡ(unicode.Categories)), 0, 1, 2, 3); // BSS
    no(unicodeAsciiHexDigitˢ, @unsafe.Pointer.FromBox(Ꮡ(unicode.ASCII_Hex_Digit)), 0, 1, 2, 3); // DATA
    // Check that we have enough data in total.
    // On arm64 the fips sections in this test currently total 23 kB.
    var n = (uintptr)0;
    foreach (var (_, s) in Linkinfo.Sects.ΔRangeSnapshot()) {
        n += (uintptr)s.End - (uintptr)s.Start;
    }
    if (n < 16 * 1024) {
        Ꮡt.Fatalf("fips sections not big enough: %d, want at least 16 kB"u8, n);
    }
}

} // end fipstest_internal_test_package
