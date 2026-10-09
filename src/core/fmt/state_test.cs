// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δtesting = testing_package;
using static go.fmt_internal_test_package;

partial class fmt_test_package {

partial struct testState {
    internal nint width;
    internal bool widthOK;
    internal nint prec;
    internal bool precOK;
    internal map<nint, bool> flag;
}

internal static fmt.State _ᴛ2ʗ = new testState(nil);

internal static (nint n, error err) Write(this testState s, slice<byte> b) {
    throw panic("unimplemented");
}

internal static (nint wid, bool ok) Width(this testState s) {
    return (s.width, s.widthOK);
}

internal static (nint prec, bool ok) Precision(this testState s) {
    return (s.prec, s.precOK);
}

internal static bool Flag(this testState s, nint c) {
    return s.flag[c];
}

public static UntypedInt NO => -1000;

internal static testState mkState(nint w, nint p, @string flags) {
    var s = new testState(nil);
    if (w != NO) {
        s.width = w;
        s.widthOK = true;
    }
    if (p != NO) {
        s.prec = p;
        s.precOK = true;
    }
    s.flag = new map<nint, bool>();
    foreach (var (_, c) in flags) {
        s.flag[(nint)c] = true;
    }
    return s;
}

internal partial struct TestFormatString_type /*dyn*/ {
    internal nint width, prec;
    internal @string flags;
    internal @string result;
}

public static void TestFormatString(ж<Δtesting.T> Ꮡt) {
    slice<TestFormatString_type> tests = new TestFormatString_type[]{
        new(NO, NO, ""u8, "%x"u8),
        new(NO, 3, ""u8, "%.3x"u8),
        new(3, NO, ""u8, "%3x"u8),
        new(7, 3, ""u8, "%7.3x"u8),
        new(NO, NO, " +-#0"u8, "% +-#0x"u8),
        new(7, 3, "+"u8, "%+7.3x"u8),
        new(7, -3, "-"u8, "%-7.-3x"u8),
        new(7, 3, " "u8, "% 7.3x"u8),
        new(7, 3, "#"u8, "%#7.3x"u8),
        new(7, 3, "0"u8, "%07.3x"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        @string got = fmt.FormatString(mkState(test.width, test.prec, test.flags), (rune)'x');
        if (got != test.result) {
            Ꮡt.Errorf("%v: got %s"u8, test, got);
        }
    }
}

} // end fmt_test_package
