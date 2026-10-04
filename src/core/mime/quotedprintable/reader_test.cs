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

[GoType("dyn")] internal partial struct TestReader_tests {
    internal @string @in, want;
    internal any err;
}

public static void TestReader(ж<testing.T> Ꮡt) {
    var tests = new TestReader_tests[]{
        new(@in: ""u8, want: ""u8),
        new(@in: "foo bar"u8, want: "foo bar"u8),
        new(@in: "foo bar=3D"u8, want: "foo bar="u8),
        new(@in: "foo bar=3d"u8, want: "foo bar="u8), // lax.

        new(@in: "foo bar=\n"u8, want: "foo bar"u8),
        new(@in: "foo bar\n"u8, want: "foo bar\n"u8), // somewhat lax.

        new(@in: "foo bar=0"u8, want: "foo bar=0"u8), // lax

        new(@in: "foo bar=0D=0A"u8, want: "foo bar\r\n"u8),
        new(@in: " A B        \r\n C "u8, want: " A B\r\n C"u8),
        new(@in: " A B =\r\n C "u8, want: " A B  C"u8),
        new(@in: " A B =\n C "u8, want: " A B  C"u8), // lax. treating LF as CRLF

        new(@in: "foo=\nbar"u8, want: "foobar"u8),
        new(@in: ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x00, 0x62, 0x61, 0x72})), want: "foo"u8, err: (@string)"quotedprintable: invalid unescaped byte 0x00 in body"u8),
        new(@in: ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x20, 0x62, 0x61, 0x72, 0xff})), want: ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x20, 0x62, 0x61, 0x72, 0xff}))), // Equal sign.

        new(@in: "=3D30\n"u8, want: "=30\n"u8),
        new(@in: "=00=FF0=\n"u8, want: ((@string)(new byte[]{0x00, 0xff, 0x30}))), // Trailing whitespace

        new(@in: "foo  \n"u8, want: "foo\n"u8),
        new(@in: "foo  \n\nfoo =\n\nfoo=20\n\n"u8, want: "foo\n\nfoo \nfoo \n\n"u8), // Tests that we allow bare \n and \r through, despite it being strictly
 // not permitted per RFC 2045, Section 6.7 Page 22 bullet (4).

        new(@in: "foo\nbar"u8, want: "foo\nbar"u8),
        new(@in: "foo\rbar"u8, want: "foo\rbar"u8),
        new(@in: "foo\r\nbar"u8, want: "foo\r\nbar"u8), // Different types of soft line-breaks.

        new(@in: "foo=\r\nbar"u8, want: "foobar"u8),
        new(@in: "foo=\nbar"u8, want: "foobar"u8),
        new(@in: "foo=\rbar"u8, want: "foo"u8, err: (@string)"quotedprintable: invalid hex byte 0x0d"u8),
        new(@in: "foo=\r\r\r \nbar"u8, want: "foo"u8, err: (@string)@"quotedprintable: invalid bytes after =: ""\r\r\r \n"""u8), // Issue 15486, accept trailing soft line-break at end of input.

        new(@in: "foo="u8, want: "foo"u8),
        new(@in: "="u8, want: ""u8, err: (@string)@"quotedprintable: invalid bytes after =: """""u8), // Example from RFC 2045:

        new(@in: "Now's the time =\n"u8 + "for all folk to come=\n"u8 + " to the aid of their country."u8,
            want: "Now's the time for all folk to come to the aid of their country."u8),
        new(@in: "accept UTF-8 right quotation mark: ’"u8,
            want: "accept UTF-8 right quotation mark: ’"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var (_, err) = io.Copy(new quotedprintable_test_package.strings_BuilderжWriter(Ꮡbuf), new quotedprintable_test_package.quotedprintable_ReaderжReader(NewReader(new quotedprintable_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)))));
        {
            @string got = buf.String(); if (got != tt.want) {
                Ꮡt.Errorf("for %q, got %q; want %q"u8, tt.@in, got, tt.want);
            }
        }
        switch (tt.err.type()) {
        case null: {
            if (err != default!) {
                Ꮡt.Errorf("for %q, got unexpected error: %v"u8, tt.@in, err);
            }
            break;
        }
        case @string verr: {
            {
                @string got = fmt.Sprint(err); if (got != verr) {
                    Ꮡt.Errorf("for %q, got error %q; want %q"u8, tt.@in, got, verr);
                }
            }
            break;
        }
        case {} Δverr when Δverr._<error>(out var verr): {
            if (!AreEqual(err, verr)) {
                Ꮡt.Errorf("for %q, got error %q; want %q"u8, tt.@in, err, verr);
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

internal static ж<bool> useQprint = flag.Bool("qprint"u8, false, "Compare against the 'qprint' program."u8);

internal static ж<regexp.Regexp> badSoftRx = regexp.MustCompile(@"=([^\r\n]+?\n)|([^\r\n]+$)|(\r$)|(\r[^\n]+\n)|( \r\n)"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string qprintˢ = "qprint"u8;
internal static readonly @string invalidBytesAfterˢ = "invalid bytes after =:"u8;
internal static readonly @string invalidBytesAfterˢ2 = "invalid bytes after ="u8;
internal static readonly @string invalidHexByteˢ = "invalid hex byte "u8;
internal static readonly @string unexpectedEofˢ = "unexpected EOF"u8;
internal static readonly @string ok28934InvalidBytesAfterˢ = """
OK: 28934
invalid bytes after =: 3949
quotedprintable: invalid hex byte 0x0d: 2048
unexpected EOF: 194
"""u8;
internal static readonly @string ok896InvalidBytesAfterˢ = """
OK: 896
invalid bytes after =: 100
quotedprintable: invalid hex byte 0x0d: 26
unexpected EOF: 3
"""u8;

public static void TestExhaustive(ж<testing.T> Ꮡt) {
    if (useQprint.Value) {
        var (_, err) = exec.LookPath(qprintˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Error looking for qprint: %v"u8, err);
        }
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var res = new map<@string, nint>();
    nint n = 6;
    if (testing.Short()) {
        n = 4;
    }
    var resʗ1 = res;
    everySequence(""u8, "0A \r\n="u8, n, [MethodImpl(MethodImplOptions.NoInlining)] (@string s) => {
        if (strings.HasSuffix(s, "="u8) || strings.Contains(s, "=="u8)) {
            return;
        }
        Ꮡbuf.Value.Reset();
        var (_, err) = io.Copy(new quotedprintable_test_package.strings_BuilderжWriter(Ꮡbuf), new quotedprintable_test_package.quotedprintable_ReaderжReader(NewReader(new quotedprintable_test_package.strings_ReaderжReader(strings.NewReader(s)))));
        if (err != default!) {
            @string errStr = err.Error();
            if (strings.Contains(errStr, invalidBytesAfterˢ)) {
                errStr = invalidBytesAfterˢ2;
            }
            resʗ1[errStr]++;
            if (strings.Contains(errStr, invalidHexByteˢ)) {
                if (strings.HasSuffix(errStr, "0x20"u8) && (strings.Contains(s, "=0 "u8) || strings.Contains(s, "=A "u8) || strings.Contains(s, "= "u8))) {
                    return;
                }
                if (strings.HasSuffix(errStr, "0x3d"u8) && (strings.Contains(s, "=0="u8) || strings.Contains(s, "=A="u8))) {
                    return;
                }
                if (strings.HasSuffix(errStr, "0x0a"u8) || strings.HasSuffix(errStr, "0x0d"u8)) {
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
            Ꮡt.Errorf("decode(%q) = %v"u8, s, err);
            return;
        }
        if (useQprint.Value) {
            var cmd = exec.Command(qprintˢ, "-d"u8);
            cmd.Value.Stdin = new quotedprintable_test_package.strings_ReaderжReader(strings.NewReader(s));
            var (stderr, errΔ1) = cmd.StderrPipe();
            if (errΔ1 != default!) {
                throw panic(errΔ1);
            }
            var qpres = new channel<any>(2);
            var cmdʗ1 = cmd;
            var qpresʗ1 = qpres;
            var stderrʗ1 = stderr;
            goǃ(() => {
                var br = bufio.NewReader(stderrʗ1);
                var (sΔ1, _) = br.ReadString((rune)'\n');
                if (sΔ1 != ""u8) {
                    qpresʗ1.ᐸꟷ(errors.New(sΔ1));
                    if ((~cmdʗ1).Process != nil) {
                        // It can get stuck on invalid input, like:
                        // echo -n "0000= " | qprint -d
                        (~cmdʗ1).Process.Kill();
                    }
                }
            });
            var cmdʗ2 = cmd;
            var qpresʗ2 = qpres;
            goǃ(() => {
                var (wantΔ1, errΔ2) = cmdʗ2.Output();
                if (errΔ2 == default!) {
                    qpresʗ2.ᐸꟷ(wantΔ1);
                }
            });
            var selᴛ1 = qpres;
            var selᴛ2 = time.After((time.Duration)(5000000000L));
            switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out var gotΔ1): {
                {
                    var (wantΔ2, ok) = gotΔ1._<slice<byte>>(ᐧ); if (ok){
                        if (((@string)wantΔ2) != Ꮡbuf.Value.String()) {
                            Ꮡt.Errorf("go decode(%q) = %q; qprint = %q"u8, s, wantΔ2, Ꮡbuf.Value.String());
                        }
                    } else {
                        Ꮡt.Logf("qprint -d(%q) = %v"u8, s, gotΔ1);
                    }
                }
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out _): {
                Ꮡt.Logf("qprint timeout on %q"u8, s);
                break;
            }}
        }
        resʗ1["OK"u8]++;
    });
    slice<@string> outcomes = default!;
    foreach (var (k, v) in res) {
        outcomes = append(outcomes, fmt.Sprintf("%v: %d"u8, k, v));
    }
    slices.Sort<slice<@string>, @string>(outcomes);
    @string got = strings.Join(outcomes, "\n"u8);
    @string want = ok28934InvalidBytesAfterˢ;
    if (testing.Short()) {
        want = ok896InvalidBytesAfterˢ;
    }
    if (got != want) {
        Ꮡt.Errorf("Got:\n%s\nWant:\n%s"u8, got, want);
    }
}

} // end quotedprintable_internal_test_package
