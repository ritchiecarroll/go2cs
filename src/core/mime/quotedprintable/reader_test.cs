// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using bufio = bufio_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using io = io_package;
using exec = os.exec_package;
using regexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using os;
using static go.mime.quotedprintable_package;

partial class quotedprintable_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object quotedprintableInvalidˢ = (@string)"quotedprintable: invalid unescaped byte 0x00 in body"u8;
internal static readonly object quotedprintableInvalidˢ2 = (@string)"quotedprintable: invalid hex byte 0x0d"u8;
internal static readonly object quotedprintableInvalidˢ3 = (@string)@"quotedprintable: invalid bytes after =: ""\r\r\r \n"""u8;
internal static readonly object quotedprintableInvalidˢ4 = (@string)@"quotedprintable: invalid bytes after =: """""u8;

internal partial struct TestReader_tests /*dyn*/ {
    internal @string @in, want;
    internal any err;
}

public static void TestReader(ж<testing.T> t) {
    var tests = slice<TestReader_tests>([
        new(@in: "", want: ""),
        new(@in: "foo bar", want: "foo bar"),
        new(@in: "foo bar=3D", want: "foo bar="),
        new(@in: "foo bar=3d", want: "foo bar="), // lax.
        new(@in: "foo bar=\n", want: "foo bar"),
        new(@in: "foo bar\n", want: "foo bar\n"), // somewhat lax.
        new(@in: "foo bar=0", want: "foo bar=0"), // lax
        new(@in: "foo bar=0D=0A", want: "foo bar\r\n"),
        new(@in: " A B        \r\n C ", want: " A B\r\n C"),
        new(@in: " A B =\r\n C ", want: " A B  C"),
        new(@in: " A B =\n C ", want: " A B  C"), // lax. treating LF as CRLF
        new(@in: "foo=\nbar", want: "foobar"),
        new(@in: ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x00, 0x62, 0x61, 0x72})), want: "foo", err: quotedprintableInvalidˢ),
        new(@in: ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x20, 0x62, 0x61, 0x72, 0xff})), want: ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x20, 0x62, 0x61, 0x72, 0xff}))),

        // Equal sign.
        new(@in: "=3D30\n", want: "=30\n"),
        new(@in: "=00=FF0=\n", want: ((@string)(new byte[]{0x00, 0xff, 0x30}))),

        // Trailing whitespace
        new(@in: "foo  \n", want: "foo\n"),
        new(@in: "foo  \n\nfoo =\n\nfoo=20\n\n", want: "foo\n\nfoo \nfoo \n\n"),

        // Tests that we allow bare \n and \r through, despite it being strictly
        // not permitted per RFC 2045, Section 6.7 Page 22 bullet (4).
        new(@in: "foo\nbar", want: "foo\nbar"),
        new(@in: "foo\rbar", want: "foo\rbar"),
        new(@in: "foo\r\nbar", want: "foo\r\nbar"),

        // Different types of soft line-breaks.
        new(@in: "foo=\r\nbar", want: "foobar"),
        new(@in: "foo=\nbar", want: "foobar"),
        new(@in: "foo=\rbar", want: "foo", err: quotedprintableInvalidˢ2),
        new(@in: "foo=\r\r\r \nbar", want: "foo", err: quotedprintableInvalidˢ3),
        // Issue 15486, accept trailing soft line-break at end of input.
        new(@in: "foo=", want: "foo"),
        new(@in: "=", want: "", err: quotedprintableInvalidˢ4),

        // Example from RFC 2045:
        new(@in: "Now's the time =\n"u8 + "for all folk to come=\n"u8 + " to the aid of their country."u8,
            want: "Now's the time for all folk to come to the aid of their country."),
        new(@in: "accept UTF-8 right quotation mark: ’",
            want: "accept UTF-8 right quotation mark: ’"),

        // Transport padding
        new(@in: "foo= \r\nbar", want: "foobar"),
        new(@in: "foo=\t \r\nbar", want: "foobar")
    ]);
    foreach (var (_, tt) in tests) {
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var (_, err) = io.Copy(new quotedprintable_test_package.strings_BuilderжWriter(Ꮡbuf), new quotedprintable_test_package.quotedprintable_ReaderжReader(NewReader(new quotedprintable_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)))));
        {
            @string got = buf.String(); if (got != tt.want) {
                t.Errorf("for %q, got %q; want %q", tt.@in, got, tt.want);
            }
        }
        switch (tt.err.type()) {
        case null: {
            if (err != nil) {
                t.Errorf("for %q, got unexpected error: %v", tt.@in, err);
            }
            break;
        }
        case @string verr: {
            {
                @string got = fmt.Sprint(err); if (got != verr) {
                    t.Errorf("for %q, got error %q; want %q", tt.@in, got, verr);
                }
            }
            break;
        }
        case {} Δverr when Δverr._<error>(out var verr): {
            if (!AreEqual(err, verr)) {
                t.Errorf("for %q, got error %q; want %q", tt.@in, err, verr);
            }
            break;
        }}
    }
}

internal static void everySequence(@string @base, @string alpha, nint length, Action<@string> fn) {
    if (len(@base) == length) {
        fn(@base);
        return;
    }
    for (nint i = 0; i < len(alpha); i++) {
        everySequence(@base + alpha.slice(i, i + 1), alpha, length, fn);
    }
}

internal static ж<bool> useQprint = flag.Bool("qprint", false, "Compare against the 'qprint' program.");

internal static ж<regexp.Regexp> badSoftRx = regexp.MustCompile(@"=([^\r\n]+?\n)|([^\r\n]+$)|(\r$)|(\r[^\n]+\n)|( \r\n)");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string qprintˢ => ref qprintˢᵇ;
private static readonly @string qprintˢᵇ = "qprint"u8;
internal static ref readonly @string invalidBytesAfterˢ => ref invalidBytesAfterˢᵇ;
private static readonly @string invalidBytesAfterˢᵇ = "invalid bytes after =:"u8;
internal static ref readonly @string invalidBytesAfterˢ2 => ref invalidBytesAfterˢ2ᵇ;
private static readonly @string invalidBytesAfterˢ2ᵇ = "invalid bytes after ="u8;
internal static ref readonly @string invalidHexByteˢ => ref invalidHexByteˢᵇ;
private static readonly @string invalidHexByteˢᵇ = "invalid hex byte "u8;
internal static ref readonly @string unexpectedEofˢ => ref unexpectedEofˢᵇ;
private static readonly @string unexpectedEofˢᵇ = "unexpected EOF"u8;
internal static ref readonly @string ok30638InvalidBytesAfterˢ => ref ok30638InvalidBytesAfterˢᵇ;
private static readonly @string ok30638InvalidBytesAfterˢᵇ = """
OK: 30638
invalid bytes after =: 2243
quotedprintable: invalid hex byte 0x0d: 2050
unexpected EOF: 194
"""u8.Raw();
internal static ref readonly @string ok935InvalidBytesAfter61ˢ => ref ok935InvalidBytesAfter61ˢᵇ;
private static readonly @string ok935InvalidBytesAfter61ˢᵇ = """
OK: 935
invalid bytes after =: 61
quotedprintable: invalid hex byte 0x0d: 26
unexpected EOF: 3
"""u8.Raw();

public static void TestExhaustive(ж<testing.T> t) {
    var outcomesᴷ = new stackAppend<@string>();

    if (useQprint.Value) {
        var (_, err) = exec.LookPath(qprintˢ);
        if (err != nil) {
            t.Fatalf("Error looking for qprint: %v", err);
        }
    }

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var res = new map<@string, nint>();
    nint n = 6;
    if (testing.Short()) {
        n = 4;
    }
    var resʗ1 = res;
    everySequence(""u8, "0A \r\n=", n, [MethodImpl(MethodImplOptions.NoInlining)] (@string s) => {
        if (strings.HasSuffix(s, "=") || strings.Contains(s, "==")) {
            return;
        }
        Ꮡbuf.Value.Reset();
        var (_, err) = io.Copy(new quotedprintable_test_package.strings_BuilderжWriter(Ꮡbuf), new quotedprintable_test_package.quotedprintable_ReaderжReader(NewReader(new quotedprintable_test_package.strings_ReaderжReader(strings.NewReader(s)))));
        if (err != nil) {
            @string errStr = err.Error();
            if (strings.Contains(errStr, invalidBytesAfterˢ)) {
                errStr = invalidBytesAfterˢ2;
            }
            resʗ1[errStr]++;
            if (strings.Contains(errStr, invalidHexByteˢ)) {
                if (strings.HasSuffix(errStr, "0x20") && (strings.Contains(s, "=0 ") || strings.Contains(s, "=A ") || strings.Contains(s, "= "))) {
                    return;
                }
                if (strings.HasSuffix(errStr, "0x3d") && (strings.Contains(s, "=0=") || strings.Contains(s, "=A="))) {
                    return;
                }
                if (strings.HasSuffix(errStr, "0x0a") || strings.HasSuffix(errStr, "0x0d")) {
                    // bunch of cases; since whitespace at the end of a line before \n is removed.
                    return;
                }
            }
            if (strings.Contains(errStr, unexpectedEofˢ)) {
                return;
            }
            if (errStr == "invalid bytes after ="u8 && badSoftRx.MatchString(s)) {
                return;
            }
            t.Errorf("decode(%q) = %v", s, err);
            return;
        }
        if (useQprint.Value) {
            var cmd = exec.Command(qprintˢ, "-d");
            cmd.Stdin = new quotedprintable_test_package.strings_ReaderжReader(strings.NewReader(s));
            var (stderr, errΔ1) = cmd.StderrPipe();
            if (errΔ1 != nil) {
                throw panic(errΔ1);
            }
            var qpres = new channel<any>(2);
            var cmdʗ1 = cmd;
            var qpresʗ1 = qpres;
            var stderrʗ1 = stderr;
            goǃ(() => {
                var br = bufio.NewReader(stderrʗ1);
                var (sΔ1, _) = br.ReadString((byte)'\n');
                if (sΔ1 != ""u8) {
                    qpresʗ1.ᐸꟷ(errors.New(sΔ1));
                    if (cmdʗ1.Process != nil) {
                        // It can get stuck on invalid input, like:
                        // echo -n "0000= " | qprint -d
                        cmdʗ1.Process.Kill();
                    }
                }
            });
            var cmdʗ2 = cmd;
            var qpresʗ2 = qpres;
            goǃ(() => {
                var (wantΔ1, errΔ2) = cmdʗ2.Output();
                if (errΔ2 == nil) {
                    qpresʗ2.ᐸꟷ(wantΔ1);
                }
            });
            var selᴛ1 = qpres;
            var selᴛ2 = time.After((time.Duration)(5000000000L));
            switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out var gotΔ1): {
                {
                    var (wantΔ2, ok) = gotΔ1._<slice<byte>>(ᐧ); if (ok) {
                        if (((@string)wantΔ2) != Ꮡbuf.Value.String()) {
                            t.Errorf("go decode(%q) = %q; qprint = %q", s, wantΔ2, Ꮡbuf.Value.String());
                        }
                    } else {
                        t.Logf("qprint -d(%q) = %v", s, gotΔ1);
                    }
                }
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out _): {
                t.Logf("qprint timeout on %q", s);
                break;
            }}
        }
        resʗ1["OK"]++;
    });
    slice<@string> outcomes = default;
    foreach (var (k, v) in res) {
        outcomes = append(outcomesᴷ, outcomes, fmt.Sprintf("%v: %d"u8, k, v));
    }
    slices.Sort<slice<@string>, @string>(outcomes);
    @string got = strings.Join(outcomes, "\n");
    @string want = ok30638InvalidBytesAfterˢ;
    if (testing.Short()) {
        want = ok935InvalidBytesAfter61ˢ;
    }

    if (got != want) {
        t.Errorf("Got:\n%s\nWant:\n%s", got, want);
    }
}

} // end quotedprintable_internal_test_package
