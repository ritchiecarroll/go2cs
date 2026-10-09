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

public static void TestEqualFold(ж<testing.T> Ꮡt) {
// This "K" is 'KELVIN SIGN' (\u212A)
    slice<TestEqualFold_type> tests = new TestEqualFold_type[]{
        new(
            name: "empty"u8,
            want: true
        ),
        new(
            name: "simple match"u8,
            a: "CHUNKED"u8,
            b: "chunked"u8,
            want: true
        ),
        new(
            name: "same string"u8,
            a: "chunked"u8,
            b: "chunked"u8,
            want: true
        ),
        new(
            name: "Unicode Kelvin symbol"u8,
            a: "chunKed"u8,
            b: "chunked"u8,
            want: false
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestEqualFold_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            {
                var got = EqualFold(ttʗ1.a, ttʗ1.b); if (got != ttʗ1.want) {
                    tΔ1.Errorf("AsciiEqualFold(%q,%q): got %v want %v"u8, ttʗ1.a, ttʗ1.b, got, ttʗ1.want);
                }
            }
        });
    }
}

internal partial struct TestIsPrint_type /*dyn*/ {
    internal @string name;
    internal @string @in;
    internal bool want;
}

public static void TestIsPrint(ж<testing.T> Ꮡt) {
// This "K" is 'KELVIN SIGN' (\u212A)
    slice<TestIsPrint_type> tests = new TestIsPrint_type[]{
        new(
            name: "empty"u8,
            want: true
        ),
        new(
            name: "ASCII low"u8,
            @in: "This is a space: ' '"u8,
            want: true
        ),
        new(
            name: "ASCII high"u8,
            @in: "This is a tilde: '~'"u8,
            want: true
        ),
        new(
            name: "ASCII low non-print"u8,
            @in: "This is a unit separator: \x1F"u8,
            want: false
        ),
        new(
            name: "Ascii high non-print"u8,
            @in: "This is a Delete: \x7F"u8,
            want: false
        ),
        new(
            name: "Unicode letter"u8,
            @in: "Today it's 280K outside: it's freezing!"u8,
            want: false
        ),
        new(
            name: "Unicode emoji"u8,
            @in: "Gophers like 🧀"u8,
            want: false
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestIsPrint_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            {
                var got = IsPrint(ttʗ1.@in); if (got != ttʗ1.want) {
                    tΔ1.Errorf("IsASCIIPrint(%q): got %v want %v"u8, ttʗ1.@in, got, ttʗ1.want);
                }
            }
        });
    }
}

} // end ascii_internal_test_package
