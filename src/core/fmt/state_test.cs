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

internal static fmt.State _ᴛ2ʗ = new testState();

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
    var s = new testState();
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

public static void TestFormatString(ж<Δtesting.T> t) {
    slice<TestFormatString_type> tests = slice<TestFormatString_type>([
        new(NO, NO, "", "%x"),
        new(NO, 3, "", "%.3x"),
        new(3, NO, "", "%3x"),
        new(7, 3, "", "%7.3x"),
        new(NO, NO, " +-#0", "% +-#0x"),
        new(7, 3, "+", "%+7.3x"),
        new(7, -3, "-", "%-7.-3x"),
        new(7, 3, " ", "% 7.3x"),
        new(7, 3, "#", "%#7.3x"),
        new(7, 3, "0", "%07.3x")
    ]);
    foreach (var (_, test) in range(tests)) {
        @string got = fmt.FormatString(mkState(test.width, test.prec, test.flags), 'x');
        if (got != test.result) {
            t.Errorf("%v: got %s", test, got);
        }
    }
}

} // end fmt_test_package
