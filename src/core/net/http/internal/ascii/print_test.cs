// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http.@internal;

using testing = testing_package;
using static go.net.http.@internal.ascii_package;

partial class ascii_internal_test_package {

internal partial struct TestEqualFold_type /*dyn*/ {
    internal @string name;
    internal @string a, b;
    internal bool want;
}

public static void TestEqualFold(ж<testing.T> t) {
    slice<TestEqualFold_type> tests = slice<TestEqualFold_type>([
        new(
            name: "empty",
            want: true
        ),
        new(
            name: "simple match",
            a: "CHUNKED",
            b: "chunked",
            want: true
        ),
        new(
            name: "same string",
            a: "chunked",
            b: "chunked",
            want: true
        ),
        new(
            name: "Unicode Kelvin symbol",
            a: "chunKed", // This "K" is 'KELVIN SIGN' (\u212A)
            b: "chunked",
            want: false
        )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestEqualFold_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = EqualFold(ttʗ1.a, ttʗ1.b); if (got != ttʗ1.want) {
                tΔ1.Errorf("AsciiEqualFold(%q,%q): got %v want %v", ttʗ1.a, ttʗ1.b, got, ttʗ1.want);
            }
        });
    }
}

internal partial struct TestIsPrint_type /*dyn*/ {
    internal @string name;
    internal @string @in;
    internal bool want;
}

public static void TestIsPrint(ж<testing.T> t) {
    slice<TestIsPrint_type> tests = slice<TestIsPrint_type>([
        new(
            name: "empty",
            want: true
        ),
        new(
            name: "ASCII low",
            @in: "This is a space: ' '",
            want: true
        ),
        new(
            name: "ASCII high",
            @in: "This is a tilde: '~'",
            want: true
        ),
        new(
            name: "ASCII low non-print",
            @in: "This is a unit separator: \x1F",
            want: false
        ),
        new(
            name: "Ascii high non-print",
            @in: "This is a Delete: \x7F",
            want: false
        ),
        new(
            name: "Unicode letter",
            @in: "Today it's 280K outside: it's freezing!", // This "K" is 'KELVIN SIGN' (\u212A)
            want: false
        ),
        new(
            name: "Unicode emoji",
            @in: "Gophers like 🧀",
            want: false
        )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestIsPrint_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = IsPrint(ttʗ1.@in); if (got != ttʗ1.want) {
                tΔ1.Errorf("IsASCIIPrint(%q): got %v want %v", ttʗ1.@in, got, ttʗ1.want);
            }
        });
    }
}

} // end ascii_internal_test_package
