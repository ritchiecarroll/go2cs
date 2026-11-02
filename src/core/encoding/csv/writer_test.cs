// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using errors = errors_package;
using strings = strings_package;
using testing = testing_package;
using io = io_package;
using static go.encoding.csv_package;

partial class csv_internal_test_package {

partial struct writeTestsᴛ1 /*dyn*/ {
    public slice<slice<@string>> Input;
    public @string Output;
    public error Error;
    public bool UseCRLF;
    public rune Comma;
}
internal static slice<writeTestsᴛ1> writeTests;
internal static void initᴛwriteTests() { writeTests = slice<writeTestsᴛ1>([
    new(Input: slice<slice<@string>>([slice<@string>(["abc"])]), Output: "abc\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["abc"])]), Output: "abc\r\n", UseCRLF: true),
    new(Input: slice<slice<@string>>([slice<@string>([@"""abc"""])]), Output: @"""""""abc"""""""u8 + "\n"u8),
    new(Input: slice<slice<@string>>([slice<@string>([@"a""b"])]), Output: @"""a""""b"""u8 + "\n"u8),
    new(Input: slice<slice<@string>>([slice<@string>([@"""a""b"""])]), Output: @"""""""a""""b"""""""u8 + "\n"u8),
    new(Input: slice<slice<@string>>([slice<@string>([" abc"])]), Output: @""" abc"""u8 + "\n"u8),
    new(Input: slice<slice<@string>>([slice<@string>(["abc,def"])]), Output: @"""abc,def"""u8 + "\n"u8),
    new(Input: slice<slice<@string>>([slice<@string>(["abc", "def"])]), Output: "abc,def\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["abc"]), slice<@string>(["def"])]), Output: "abc\ndef\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["abc\ndef"])]), Output: "\"abc\ndef\"\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["abc\ndef"])]), Output: "\"abc\r\ndef\"\r\n", UseCRLF: true),
    new(Input: slice<slice<@string>>([slice<@string>(["abc\rdef"])]), Output: "\"abcdef\"\r\n", UseCRLF: true),
    new(Input: slice<slice<@string>>([slice<@string>(["abc\rdef"])]), Output: "\"abc\rdef\"\n", UseCRLF: false),
    new(Input: slice<slice<@string>>([slice<@string>([""])]), Output: "\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["", ""])]), Output: ",\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["", "", ""])]), Output: ",,\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["", "", "a"])]), Output: ",,a\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["", "a", ""])]), Output: ",a,\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["", "a", "a"])]), Output: ",a,a\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["a", "", ""])]), Output: "a,,\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["a", "", "a"])]), Output: "a,,a\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["a", "a", ""])]), Output: "a,a,\n"),
    new(Input: slice<slice<@string>>([slice<@string>(["a", "a", "a"])]), Output: "a,a,a\n"),
    new(Input: slice<slice<@string>>([slice<@string>([@"\."])]), Output: "\"\\.\"\n"),
    new(Input: slice<slice<@string>>([slice<@string>([((@string)(new byte[]{0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c})), "aktau"])]), Output: ((@string)(new byte[]{0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c, 0x2c, 0x61, 0x6b, 0x74, 0x61, 0x75, 0x0a}))),
    new(Input: slice<slice<@string>>([slice<@string>([((@string)(new byte[]{0x2c, 0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c})), "aktau"])]), Output: ((@string)(new byte[]{0x22, 0x2c, 0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c, 0x22, 0x2c, 0x61, 0x6b, 0x74, 0x61, 0x75, 0x0a}))),
    new(Input: slice<slice<@string>>([slice<@string>(["a", "a", ""])]), Output: "a|a|\n", Comma: '|'),
    new(Input: slice<slice<@string>>([slice<@string>([",", ",", ""])]), Output: ",|,|\n", Comma: '|'),
    new(Input: slice<slice<@string>>([slice<@string>(["foo"])]), Comma: '"', Error: errInvalidDelim)
]); }

public static void TestWrite(ж<testing.T> t) {
    foreach (var (n, tt) in writeTests) {
        var b = Ꮡ(new strings.Builder());
        var f = NewWriter(new csv_test_package.strings_BuilderжWriter(b));
        f.UseCRLF = tt.UseCRLF;
        if (tt.Comma != 0) {
            f.Comma = tt.Comma;
        }
        var err = f.WriteAll(tt.Input);
        if (!AreEqual(err, tt.Error)) {
            t.Errorf("Unexpected error:\ngot  %v\nwant %v", err, tt.Error);
        }
        @string @out = b.String();
        if (@out != tt.Output) {
            t.Errorf("#%d: out=%q want %q", n, @out, tt.Output);
        }
    }
}

internal partial struct errorWriter {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "Test"u8;

internal static (nint, error) Write(this errorWriter e, slice<byte> b) {
    return (0, errors.New(testˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorShouldNotBeNilˢ = (@string)"Error should not be nil"u8;

public static void TestError(ж<testing.T> t) {
    var b = Ꮡ(new bytes.Buffer());
    var f = NewWriter(new csv_test_package.bytes_BufferжWriter(b));
    f.Write(slice<@string>(["abc"]));
    f.Flush();
    var err = f.Error();

    if (err != nil) {
        t.Errorf("Unexpected error: %s\n", err);
    }

    f = NewWriter(new errorWriter());
    f.Write(slice<@string>(["abc"]));
    f.Flush();
    err = f.Error();

    if (err == nil) {
        t.Error(errorShouldNotBeNilˢ);
    }
}

internal static slice<slice<@string>> benchmarkWriteData = slice<slice<@string>>([
    slice<@string>(["abc", "def", "12356", "1234567890987654311234432141542132"]),
    slice<@string>(["abc", "def", "12356", "1234567890987654311234432141542132"]),
    slice<@string>(["abc", "def", "12356", "1234567890987654311234432141542132"])
]);

public static void BenchmarkWrite(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var w = NewWriter(new csv_test_package.bytes_BufferжWriter(Ꮡ(new bytes.Buffer())));
        var err = w.WriteAll(benchmarkWriteData);
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        w.Flush();
    }
}

} // end csv_internal_test_package
