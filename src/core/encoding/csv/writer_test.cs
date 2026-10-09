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
internal static void initᴛwriteTests() { writeTests = new writeTestsᴛ1[]{
    new(Input: new slice<@string>[]{new @string[]{"abc"u8}.slice()}.slice(), Output: "abc\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"abc"u8}.slice()}.slice(), Output: "abc\r\n"u8, UseCRLF: true),
    new(Input: new slice<@string>[]{new @string[]{@"""abc"""u8}.slice()}.slice(), Output: @"""""""abc"""""""u8 + "\n"u8),
    new(Input: new slice<@string>[]{new @string[]{@"a""b"u8}.slice()}.slice(), Output: @"""a""""b"""u8 + "\n"u8),
    new(Input: new slice<@string>[]{new @string[]{@"""a""b"""u8}.slice()}.slice(), Output: @"""""""a""""b"""""""u8 + "\n"u8),
    new(Input: new slice<@string>[]{new @string[]{" abc"u8}.slice()}.slice(), Output: @""" abc"""u8 + "\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"abc,def"u8}.slice()}.slice(), Output: @"""abc,def"""u8 + "\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"abc"u8, "def"u8}.slice()}.slice(), Output: "abc,def\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"abc"u8}.slice(), new @string[]{"def"u8}.slice()}.slice(), Output: "abc\ndef\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"abc\ndef"u8}.slice()}.slice(), Output: "\"abc\ndef\"\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"abc\ndef"u8}.slice()}.slice(), Output: "\"abc\r\ndef\"\r\n"u8, UseCRLF: true),
    new(Input: new slice<@string>[]{new @string[]{"abc\rdef"u8}.slice()}.slice(), Output: "\"abcdef\"\r\n"u8, UseCRLF: true),
    new(Input: new slice<@string>[]{new @string[]{"abc\rdef"u8}.slice()}.slice(), Output: "\"abc\rdef\"\n"u8, UseCRLF: false),
    new(Input: new slice<@string>[]{new @string[]{""u8}.slice()}.slice(), Output: "\n"u8),
    new(Input: new slice<@string>[]{new @string[]{""u8, ""u8}.slice()}.slice(), Output: ",\n"u8),
    new(Input: new slice<@string>[]{new @string[]{""u8, ""u8, ""u8}.slice()}.slice(), Output: ",,\n"u8),
    new(Input: new slice<@string>[]{new @string[]{""u8, ""u8, "a"u8}.slice()}.slice(), Output: ",,a\n"u8),
    new(Input: new slice<@string>[]{new @string[]{""u8, "a"u8, ""u8}.slice()}.slice(), Output: ",a,\n"u8),
    new(Input: new slice<@string>[]{new @string[]{""u8, "a"u8, "a"u8}.slice()}.slice(), Output: ",a,a\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"a"u8, ""u8, ""u8}.slice()}.slice(), Output: "a,,\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"a"u8, ""u8, "a"u8}.slice()}.slice(), Output: "a,,a\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"a"u8, "a"u8, ""u8}.slice()}.slice(), Output: "a,a,\n"u8),
    new(Input: new slice<@string>[]{new @string[]{"a"u8, "a"u8, "a"u8}.slice()}.slice(), Output: "a,a,a\n"u8),
    new(Input: new slice<@string>[]{new @string[]{@"\."u8}.slice()}.slice(), Output: "\"\\.\"\n"u8),
    new(Input: new slice<@string>[]{new @string[]{((@string)(new byte[]{0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c})), "aktau"u8}.slice()}.slice(), Output: ((@string)(new byte[]{0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c, 0x2c, 0x61, 0x6b, 0x74, 0x61, 0x75, 0x0a}))),
    new(Input: new slice<@string>[]{new @string[]{((@string)(new byte[]{0x2c, 0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c})), "aktau"u8}.slice()}.slice(), Output: ((@string)(new byte[]{0x22, 0x2c, 0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c, 0x22, 0x2c, 0x61, 0x6b, 0x74, 0x61, 0x75, 0x0a}))),
    new(Input: new slice<@string>[]{new @string[]{"a"u8, "a"u8, ""u8}.slice()}.slice(), Output: "a|a|\n"u8, Comma: (rune)'|'),
    new(Input: new slice<@string>[]{new @string[]{","u8, ","u8, ""u8}.slice()}.slice(), Output: ",|,|\n"u8, Comma: (rune)'|'),
    new(Input: new slice<@string>[]{new @string[]{"foo"u8}.slice()}.slice(), Comma: (rune)'"', Error: errInvalidDelim)
}.slice(); }

public static void TestWrite(ж<testing.T> Ꮡt) {
    foreach (var (n, tt) in writeTests) {
        var b = Ꮡ(new strings.Builder(nil));
        var f = NewWriter(new csv_test_package.strings_BuilderжWriter(b));
        f.Value.UseCRLF = tt.UseCRLF;
        if (tt.Comma != 0) {
            f.Value.Comma = tt.Comma;
        }
        var err = f.WriteAll(tt.Input);
        if (!AreEqual(err, tt.Error)) {
            Ꮡt.Errorf("Unexpected error:\ngot  %v\nwant %v"u8, err, tt.Error);
        }
        @string @out = b.String();
        if (@out != tt.Output) {
            Ꮡt.Errorf("#%d: out=%q want %q"u8, n, @out, tt.Output);
        }
    }
}

internal partial struct errorWriter {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "Test"u8;

internal static (nint, error) Write(this errorWriter e, slice<byte> b) {
    return (0, errors.New(testˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorShouldNotBeNilˢ = (@string)"Error should not be nil"u8;

public static void TestError(ж<testing.T> Ꮡt) {
    var b = Ꮡ(new bytes.Buffer(nil));
    var f = NewWriter(new csv_test_package.bytes_BufferжWriter(b));
    f.Write(new @string[]{"abc"u8}.slice());
    f.Flush();
    var err = f.Error();
    if (err != default!) {
        Ꮡt.Errorf("Unexpected error: %s\n"u8, err);
    }
    f = NewWriter(new errorWriter(nil));
    f.Write(new @string[]{"abc"u8}.slice());
    f.Flush();
    err = f.Error();
    if (err == default!) {
        Ꮡt.Error(errorShouldNotBeNilˢ);
    }
}

internal static slice<slice<@string>> benchmarkWriteData = new slice<@string>[]{
    new @string[]{"abc"u8, "def"u8, "12356"u8, "1234567890987654311234432141542132"u8}.slice(),
    new @string[]{"abc"u8, "def"u8, "12356"u8, "1234567890987654311234432141542132"u8}.slice(),
    new @string[]{"abc"u8, "def"u8, "12356"u8, "1234567890987654311234432141542132"u8}.slice()
}.slice();

public static void BenchmarkWrite(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var w = NewWriter(new csv_test_package.bytes_BufferжWriter(Ꮡ(new bytes.Buffer(nil))));
        var err = w.WriteAll(benchmarkWriteData);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        w.Flush();
    }
}

} // end csv_internal_test_package
