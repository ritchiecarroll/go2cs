// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || freebsd || linux || windows
namespace go.@internal;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using testing = testing_package;
using time = time_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using global::go.unicode;
using static global::go.@internal.fuzz_package;

partial class fuzz_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object minimizeInputDidnTˢ = (@string)"minimizeInput didn't provide an error"u8;

internal partial struct TestMinimizeInput_testcase /*dyn*/ {
    internal @string name;
    internal Func<CorpusEntry, error> fn;
    internal slice<any> input;
    internal slice<any> expected;
}

public static void TestMinimizeInput(ж<testing.T> Ꮡt) {
    var cases = new TestMinimizeInput_testcase[]{
        new(
            name: "ones_byte"u8,
            fn: error (CorpusEntry e) => {
                var b = e.Values[0]._<slice<byte>>();
                nint ones = 0;
                foreach (var (_, v) in b) {
                    if (v == 1) {
                        ones++;
                    }
                }
                if (ones == 3) {
                    return fmt.Errorf("bad %v"u8, e.Values[0]);
                }
                return default!;
            },
            input: new any[]{new byte[]{0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice()}.slice(),
            expected: new any[]{new byte[]{1, 1, 1}.slice()}.slice()
        ),
        new(
            name: "single_bytes"u8,
            fn: error (CorpusEntry e) => {
                var b = e.Values[0]._<slice<byte>>();
                if (len(b) < 2) {
                    return default!;
                }
                if (len(b) == 2 && b[0] == 1 && b[1] == 2) {
                    return default!;
                }
                return fmt.Errorf("bad %v"u8, e.Values[0]);
            },
            input: new any[]{new byte[]{1, 2, 3, 4, 5}.slice()}.slice(),
            expected: new any[]{slice<byte>("00"u8)}.slice()
        ),
        new(
            name: "set_of_bytes"u8,
            fn: error (CorpusEntry e) => {
                var b = e.Values[0]._<slice<byte>>();
                if (len(b) < 3) {
                    return default!;
                }
                if (bytes.Equal(b, new byte[]{0, 1, 2, 3, 4, 5}.slice()) || bytes.Equal(b, new byte[]{0, 4, 5}.slice())) {
                    return fmt.Errorf("bad %v"u8, e.Values[0]);
                }
                return default!;
            },
            input: new any[]{new byte[]{0, 1, 2, 3, 4, 5}.slice()}.slice(),
            expected: new any[]{new byte[]{0, 4, 5}.slice()}.slice()
        ),
        new(
            name: "non_ascii_bytes"u8,
            fn: error (CorpusEntry e) => {
                var b = e.Values[0]._<slice<byte>>();
                if (len(b) == 3) {
                    return fmt.Errorf("bad %v"u8, e.Values[0]);
                }
                return default!;
            },
            input: new any[]{slice<byte>("ท"u8)}.slice(), // ท is 3 bytes

            expected: new any[]{slice<byte>("000"u8)}.slice()
        ),
        new(
            name: "ones_string"u8,
            fn: error (CorpusEntry e) => {
                @string b = e.Values[0]._<@string>();
                nint ones = 0;
                foreach (var (_, v) in b) {
                    if (v == (rune)'1') {
                        ones++;
                    }
                }
                if (ones == 3) {
                    return fmt.Errorf("bad %v"u8, e.Values[0]);
                }
                return default!;
            },
            input: new any[]{(@string)"001010001000000000000000000"u8}.slice(),
            expected: new any[]{(@string)"111"u8}.slice()
        ),
        new(
            name: "string_length"u8,
            fn: error (CorpusEntry e) => {
                @string b = e.Values[0]._<@string>();
                if (len(b) == 5) {
                    return fmt.Errorf("bad %v"u8, e.Values[0]);
                }
                return default!;
            },
            input: new any[]{(@string)"zzzzz"u8}.slice(),
            expected: new any[]{(@string)"00000"u8}.slice()
        ),
        new(
            name: "string_with_letter"u8,
            fn: error (CorpusEntry e) => {
                @string b = e.Values[0]._<@string>();
                var (r, _) = utf8.DecodeRune(slice<byte>(b));
                if (unicode.IsLetter(r)) {
                    return fmt.Errorf("bad %v"u8, e.Values[0]);
                }
                return default!;
            },
            input: new any[]{(@string)"ZZZZZ"u8}.slice(),
            expected: new any[]{(@string)"A"u8}.slice()
        )
    }.slice();
    foreach (var (_, tc) in cases) {
        ref var tcΔ1 = ref heap<TestMinimizeInput_testcase>(out var ᏑtcΔ1);
        tcΔ1 = tc;
        var tcʗ1 = tcΔ1;
        Ꮡt.Run(tcΔ1.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
                var tcʗ2 = tcʗ1;
            var ws = Ꮡ(new workerServer(
                fuzzFn: (CorpusEntry e) => (time.ΔSecond, tcʗ2.fn(e))
            ));
            var mem = Ꮡ(new sharedMem(region: new slice<byte>(100))); // big enough to hold value and header
            var vals = tcʗ1.input;
            var (success, err) = ws.minimizeInput(context.Background(), vals, mem, new minimizeArgs(nil));
            if (!success) {
                tΔ1.Errorf("minimizeInput did not succeed"u8);
            }
            if (err == default!) {
                tΔ1.Fatal(minimizeInputDidnTˢ);
            }
            {
                @string expected = fmt.Sprintf("bad %v"u8, tcʗ1.expected[0]); if (err.Error() != expected) {
                    tΔ1.Errorf("unexpected error: got %q, want %q"u8, err, expected);
                }
            }
            if (!reflect.DeepEqual(vals, tcʗ1.expected)) {
                tΔ1.Errorf("unexpected results: got %v, want %v"u8, vals, tcʗ1.expected);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ohnoˢ = "ohno"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;

// TestMinimizeFlaky checks that if we're minimizing an interesting
// input and a flaky failure occurs, that minimization was not indicated
// to be successful, and the error isn't returned (since it's flaky).
public static void TestMinimizeFlaky(ж<testing.T> Ꮡt) {
    var ws = Ꮡ(new workerServer(fuzzFn: (CorpusEntry e) => (time.ΔSecond, errors.New(ohnoˢ))
    ));
    var mem = Ꮡ(new sharedMem(region: new slice<byte>(100))); // big enough to hold value and header
    var vals = new any[]{slice<byte>(default!)}.slice();
    var args = new minimizeArgs(KeepCoverage: new slice<byte>(len(coverageSnapshot)));
    var (success, err) = ws.minimizeInput(context.Background(), vals, mem, args);
    if (success) {
        Ꮡt.Error(unexpectedSuccessˢ);
    }
    if (err != default!) {
        Ꮡt.Errorf("unexpected error: %v"u8, err);
    }
    {
        var count = mem.header().Value.count; if (count != 1) {
            Ꮡt.Errorf("count: got %d, want 1"u8, count);
        }
    }
}

} // end fuzz_internal_test_package
