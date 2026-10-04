// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using static go.encoding.csv_package;

partial class csv_internal_test_package {

[GoType] internal partial struct readTest {
    public @string Name;
    public @string Input;
    public slice<slice<@string>> Output;
    public slice<slice<array<nint>>> Positions;
    public slice<error> Errors;
    // These fields are copied into the Reader
    public rune Comma;
    public rune Comment;
    public bool UseFieldsPerRecord; // false (default) means FieldsPerRecord is -1
    public nint FieldsPerRecord;
    public bool LazyQuotes;
    public bool TrimLeadingSpace;
    public bool ReuseRecord;
}

// In these tests, the §, ¶ and ∑ characters in readTest.Input are used to denote
// the start of a field, a record boundary and the position of an error respectively.
// They are removed before parsing and are used to verify the position
// information reported by FieldPos.
// Issue 19019
// Issue 21201
// Issue 19410
// λ and θ start with the same byte.
// This tests that the parser doesn't confuse such characters.
// The implementation may read each line in several chunks if it doesn't fit entirely
// in the read buffer, so we should test the code to handle that condition.
internal static slice<readTest> readTests;
internal static void initᴛreadTests() { readTests = new readTest[]{new(
    Name: "Simple"u8,
    Input: "§a,§b,§c\n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice()}.slice()
), new(
    Name: "CRLF"u8,
    Input: "§a,§b\r\n¶§c,§d\r\n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8}.slice(), new @string[]{"c"u8, "d"u8}.slice()}.slice()
), new(
    Name: "BareCR"u8,
    Input: "§a,§b\rc,§d\r\n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b\rc"u8, "d"u8}.slice()}.slice()
), new(
    Name: "RFC4180test"u8,
    Input: """
§#field1,§field2,§field3
¶§"aaa",§"bb
b",§"ccc"
¶§"a,a",§"b""bb",§"ccc"
¶§zzz,§yyy,§xxx

"""u8,
    Output: new slice<@string>[]{
        new @string[]{"#field1"u8, "field2"u8, "field3"u8}.slice(),
        new @string[]{"aaa"u8, "bb\nb"u8, "ccc"u8}.slice(),
        new @string[]{"a,a"u8, @"b""bb"u8, "ccc"u8}.slice(),
        new @string[]{"zzz"u8, "yyy"u8, "xxx"u8}.slice()
    }.slice(),
    UseFieldsPerRecord: true,
    FieldsPerRecord: 0
), new(
    Name: "NoEOLTest"u8,
    Input: "§a,§b,§c"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice()}.slice()
), new(
    Name: "Semicolon"u8,
    Input: "§a;§b;§c\n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice()}.slice(),
    Comma: (rune)';'
), new(
    Name: "MultiLine"u8,
    Input: """
§"two
line",§"one line",§"three
line
field"
"""u8,
    Output: new slice<@string>[]{new @string[]{"two\nline"u8, "one line"u8, "three\nline\nfield"u8}.slice()}.slice()
), new(
    Name: "BlankLine"u8,
    Input: "§a,§b,§c\n\n¶§d,§e,§f\n\n"u8,
    Output: new slice<@string>[]{
        new @string[]{"a"u8, "b"u8, "c"u8}.slice(),
        new @string[]{"d"u8, "e"u8, "f"u8}.slice()
    }.slice()
), new(
    Name: "BlankLineFieldCount"u8,
    Input: "§a,§b,§c\n\n¶§d,§e,§f\n\n"u8,
    Output: new slice<@string>[]{
        new @string[]{"a"u8, "b"u8, "c"u8}.slice(),
        new @string[]{"d"u8, "e"u8, "f"u8}.slice()
    }.slice(),
    UseFieldsPerRecord: true,
    FieldsPerRecord: 0
), new(
    Name: "TrimSpace"u8,
    Input: " §a,  §b,   §c\n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice()}.slice(),
    TrimLeadingSpace: true
), new(
    Name: "LeadingSpace"u8,
    Input: "§ a,§  b,§   c\n"u8,
    Output: new slice<@string>[]{new @string[]{" a"u8, "  b"u8, "   c"u8}.slice()}.slice()
), new(
    Name: "Comment"u8,
    Input: "#1,2,3\n§a,§b,§c\n#comment"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice()}.slice(),
    Comment: (rune)'#'
), new(
    Name: "NoComment"u8,
    Input: "§#1,§2,§3\n¶§a,§b,§c"u8,
    Output: new slice<@string>[]{new @string[]{"#1"u8, "2"u8, "3"u8}.slice(), new @string[]{"a"u8, "b"u8, "c"u8}.slice()}.slice()
), new(
    Name: "LazyQuotes"u8,
    Input: @"§a ""word"",§""1""2"",§a"",§""b"u8,
    Output: new slice<@string>[]{new @string[]{@"a ""word"""u8, @"1""2"u8, @"a"""u8, @"b"u8}.slice()}.slice(),
    LazyQuotes: true
), new(
    Name: "BareQuotes"u8,
    Input: @"§a ""word"",§""1""2"",§a"""u8,
    Output: new slice<@string>[]{new @string[]{@"a ""word"""u8, @"1""2"u8, @"a"""u8}.slice()}.slice(),
    LazyQuotes: true
), new(
    Name: "BareDoubleQuotes"u8,
    Input: @"§a""""b,§c"u8,
    Output: new slice<@string>[]{new @string[]{@"a""""b"u8, @"c"u8}.slice()}.slice(),
    LazyQuotes: true
), new(
    Name: "BadDoubleQuotes"u8,
    Input: @"§a∑""""b,c"u8,
    Errors: new error[]{new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrBareQuote)))}.slice()
), new(
    Name: "TrimQuote"u8,
    Input: @" §""a"",§"" b"",§c"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, " b"u8, "c"u8}.slice()}.slice(),
    TrimLeadingSpace: true
), new(
    Name: "BadBareQuote"u8,
    Input: @"§a ∑""word"",""b"""u8,
    Errors: new error[]{new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrBareQuote)))}.slice()
), new(
    Name: "BadTrailingQuote"u8,
    Input: @"§""a word"",b∑"""u8,
    Errors: new error[]{new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrBareQuote)))}.slice()
), new(
    Name: "ExtraneousQuote"u8,
    Input: @"§""a ∑""word"",""b"""u8,
    Errors: new error[]{new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))}.slice()
), new(
    Name: "BadFieldCount"u8,
    Input: "§a,§b,§c\n¶∑§d,§e"u8,
    Errors: new error[]{default!, new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrFieldCount)))}.slice(),
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice(), new @string[]{"d"u8, "e"u8}.slice()}.slice(),
    UseFieldsPerRecord: true,
    FieldsPerRecord: 0
), new(
    Name: "BadFieldCountMultiple"u8,
    Input: "§a,§b,§c\n¶∑§d,§e\n¶∑§f"u8,
    Errors: new error[]{default!, new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrFieldCount))), new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrFieldCount)))}.slice(),
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice(), new @string[]{"d"u8, "e"u8}.slice(), new @string[]{"f"u8}.slice()}.slice(),
    UseFieldsPerRecord: true,
    FieldsPerRecord: 0
), new(
    Name: "BadFieldCount1"u8,
    Input: @"§∑a,§b,§c"u8,
    Errors: new error[]{new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrFieldCount)))}.slice(),
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice()}.slice(),
    UseFieldsPerRecord: true,
    FieldsPerRecord: 2
), new(
    Name: "FieldCount"u8,
    Input: "§a,§b,§c\n¶§d,§e"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice(), new @string[]{"d"u8, "e"u8}.slice()}.slice()
), new(
    Name: "TrailingCommaEOF"u8,
    Input: "§a,§b,§c,§"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8, ""u8}.slice()}.slice()
), new(
    Name: "TrailingCommaEOL"u8,
    Input: "§a,§b,§c,§\n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8, ""u8}.slice()}.slice()
), new(
    Name: "TrailingCommaSpaceEOF"u8,
    Input: "§a,§b,§c, §"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8, ""u8}.slice()}.slice(),
    TrimLeadingSpace: true
), new(
    Name: "TrailingCommaSpaceEOL"u8,
    Input: "§a,§b,§c, §\n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8, ""u8}.slice()}.slice(),
    TrimLeadingSpace: true
), new(
    Name: "TrailingCommaLine3"u8,
    Input: "§a,§b,§c\n¶§d,§e,§f\n¶§g,§hi,§"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8}.slice(), new @string[]{"d"u8, "e"u8, "f"u8}.slice(), new @string[]{"g"u8, "hi"u8, ""u8}.slice()}.slice(),
    TrimLeadingSpace: true
), new(
    Name: "NotTrailingComma3"u8,
    Input: "§a,§b,§c,§ \n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8, "c"u8, " "u8}.slice()}.slice()
), new(
    Name: "CommaFieldTest"u8,
    Input: """
§x,§y,§z,§w
¶§x,§y,§z,§
¶§x,§y,§,§
¶§x,§,§,§
¶§,§,§,§
¶§"x",§"y",§"z",§"w"
¶§"x",§"y",§"z",§""
¶§"x",§"y",§"",§""
¶§"x",§"",§"",§""
¶§"",§"",§"",§""

"""u8,
    Output: new slice<@string>[]{
        new @string[]{"x"u8, "y"u8, "z"u8, "w"u8}.slice(),
        new @string[]{"x"u8, "y"u8, "z"u8, ""u8}.slice(),
        new @string[]{"x"u8, "y"u8, ""u8, ""u8}.slice(),
        new @string[]{"x"u8, ""u8, ""u8, ""u8}.slice(),
        new @string[]{""u8, ""u8, ""u8, ""u8}.slice(),
        new @string[]{"x"u8, "y"u8, "z"u8, "w"u8}.slice(),
        new @string[]{"x"u8, "y"u8, "z"u8, ""u8}.slice(),
        new @string[]{"x"u8, "y"u8, ""u8, ""u8}.slice(),
        new @string[]{"x"u8, ""u8, ""u8, ""u8}.slice(),
        new @string[]{""u8, ""u8, ""u8, ""u8}.slice()
    }.slice()
), new(
    Name: "TrailingCommaIneffective1"u8,
    Input: "§a,§b,§\n¶§c,§d,§e"u8,
    Output: new slice<@string>[]{
        new @string[]{"a"u8, "b"u8, ""u8}.slice(),
        new @string[]{"c"u8, "d"u8, "e"u8}.slice()
    }.slice(),
    TrimLeadingSpace: true
), new(
    Name: "ReadAllReuseRecord"u8,
    Input: "§a,§b\n¶§c,§d"u8,
    Output: new slice<@string>[]{
        new @string[]{"a"u8, "b"u8}.slice(),
        new @string[]{"c"u8, "d"u8}.slice()
    }.slice(),
    ReuseRecord: true
), new(
    Name: "StartLine1"u8,
    Input: "§a,\"b\nc∑\"d,e"u8,
    Errors: new error[]{new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))}.slice()
), new(
    Name: "StartLine2"u8,
    Input: "§a,§b\n¶§\"d\n\n,e∑"u8,
    Errors: new error[]{default!, new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))}.slice(),
    Output: new slice<@string>[]{new @string[]{"a"u8, "b"u8}.slice()}.slice()
), new(
    Name: "CRLFInQuotedField"u8,
    Input: "§A,§\"Hello\r\nHi\",§B\r\n"u8,
    Output: new slice<@string>[]{
        new @string[]{"A"u8, "Hello\nHi"u8, "B"u8}.slice()
    }.slice()
), new(
    Name: "BinaryBlobField"u8,
    Input: ((@string)(new byte[]{0xc2, 0xa7, 0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c, 0x2c, 0xc2, 0xa7, 0x61, 0x6b, 0x74, 0x61, 0x75})),
    Output: new slice<@string>[]{new @string[]{((@string)(new byte[]{0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c})), "aktau"u8}.slice()}.slice()
), new(
    Name: "TrailingCR"u8,
    Input: "§field1,§field2\r"u8,
    Output: new slice<@string>[]{new @string[]{"field1"u8, "field2"u8}.slice()}.slice()
), new(
    Name: "QuotedTrailingCR"u8,
    Input: "§\"field\"\r"u8,
    Output: new slice<@string>[]{new @string[]{"field"u8}.slice()}.slice()
), new(
    Name: "QuotedTrailingCRCR"u8,
    Input: "§\"field∑\"\r\r"u8,
    Errors: new error[]{new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))}.slice()
), new(
    Name: "FieldCR"u8,
    Input: "§field\rfield\r"u8,
    Output: new slice<@string>[]{new @string[]{"field\rfield"u8}.slice()}.slice()
), new(
    Name: "FieldCRCR"u8,
    Input: "§field\r\rfield\r\r"u8,
    Output: new slice<@string>[]{new @string[]{"field\r\rfield\r"u8}.slice()}.slice()
), new(
    Name: "FieldCRCRLF"u8,
    Input: "§field\r\r\n¶§field\r\r\n"u8,
    Output: new slice<@string>[]{new @string[]{"field\r"u8}.slice(), new @string[]{"field\r"u8}.slice()}.slice()
), new(
    Name: "FieldCRCRLFCR"u8,
    Input: "§field\r\r\n¶§\rfield\r\r\n\r"u8,
    Output: new slice<@string>[]{new @string[]{"field\r"u8}.slice(), new @string[]{"\rfield\r"u8}.slice()}.slice()
), new(
    Name: "FieldCRCRLFCRCR"u8,
    Input: "§field\r\r\n¶§\r\rfield\r\r\n¶§\r\r"u8,
    Output: new slice<@string>[]{new @string[]{"field\r"u8}.slice(), new @string[]{"\r\rfield\r"u8}.slice(), new @string[]{"\r"u8}.slice()}.slice()
), new(
    Name: "MultiFieldCRCRLFCRCR"u8,
    Input: "§field1,§field2\r\r\n¶§\r\rfield1,§field2\r\r\n¶§\r\r,§"u8,
    Output: new slice<@string>[]{
        new @string[]{"field1"u8, "field2\r"u8}.slice(),
        new @string[]{"\r\rfield1"u8, "field2\r"u8}.slice(),
        new @string[]{"\r\r"u8, ""u8}.slice()
    }.slice()
), new(
    Name: "NonASCIICommaAndComment"u8,
    Input: "§a£§b,c£ \t§d,e\n€ comment\n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "b,c"u8, "d,e"u8}.slice()}.slice(),
    TrimLeadingSpace: true,
    Comma: (rune)'£',
    Comment: (rune)'€'
), new(
    Name: "NonASCIICommaAndCommentWithQuotes"u8,
    Input: "§a€§\"  b,\"€§ c\nλ comment\n"u8,
    Output: new slice<@string>[]{new @string[]{"a"u8, "  b,"u8, " c"u8}.slice()}.slice(),
    Comma: (rune)'€',
    Comment: (rune)'λ'
), new(
    Name: "NonASCIICommaConfusion"u8,
    Input: "§\"abθcd\"λ§efθgh"u8,
    Output: new slice<@string>[]{new @string[]{"abθcd"u8, "efθgh"u8}.slice()}.slice(),
    Comma: (rune)'λ',
    Comment: (rune)'€'
), new(
    Name: "NonASCIICommentConfusion"u8,
    Input: "§λ\n¶§λ\nθ\n¶§λ\n"u8,
    Output: new slice<@string>[]{new @string[]{"λ"u8}.slice(), new @string[]{"λ"u8}.slice(), new @string[]{"λ"u8}.slice()}.slice(),
    Comment: (rune)'θ'
), new(
    Name: "QuotedFieldMultipleLF"u8,
    Input: "§\"\n\n\n\n\""u8,
    Output: new slice<@string>[]{new @string[]{"\n\n\n\n"u8}.slice()}.slice()
), new(
    Name: "MultipleCRLF"u8,
    Input: "\r\n\r\n\r\n\r\n"u8
), new(
    Name: "HugeLines"u8,
    Input: strings.Repeat("#ignore\n"u8, 10000) + "§"u8 + strings.Repeat("@"u8, 5000) + ",§"u8 + strings.Repeat("*"u8, 5000),
    Output: new slice<@string>[]{new @string[]{strings.Repeat("@"u8, 5000), strings.Repeat("*"u8, 5000)}.slice()}.slice(),
    Comment: (rune)'#'
), new(
    Name: "QuoteWithTrailingCRLF"u8,
    Input: "§\"foo∑\"bar\"\r\n"u8,
    Errors: new error[]{new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))}.slice()
), new(
    Name: "LazyQuoteWithTrailingCRLF"u8,
    Input: "§\"foo\"bar\"\r\n"u8,
    Output: new slice<@string>[]{new @string[]{@"foo""bar"u8}.slice()}.slice(),
    LazyQuotes: true
), new(
    Name: "DoubleQuoteWithTrailingCRLF"u8,
    Input: "§\"foo\"\"bar\"\r\n"u8,
    Output: new slice<@string>[]{new @string[]{@"foo""bar"u8}.slice()}.slice()
), new(
    Name: "EvenQuotes"u8,
    Input: @"§"""""""""""""""""u8,
    Output: new slice<@string>[]{new @string[]{@""""""""u8}.slice()}.slice()
), new(
    Name: "OddQuotes"u8,
    Input: @"§""""""""""""""∑"u8,
    Errors: new error[]{new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))}.slice()
), new(
    Name: "LazyOddQuotes"u8,
    Input: @"§"""""""""""""""u8,
    Output: new slice<@string>[]{new @string[]{@""""""""u8}.slice()}.slice(),
    LazyQuotes: true
), new(
    Name: "BadComma1"u8,
    Comma: (rune)'\n',
    Errors: new error[]{errInvalidDelim}.slice()
), new(
    Name: "BadComma2"u8,
    Comma: (rune)'\r',
    Errors: new error[]{errInvalidDelim}.slice()
), new(
    Name: "BadComma3"u8,
    Comma: (rune)'"',
    Errors: new error[]{errInvalidDelim}.slice()
), new(
    Name: "BadComma4"u8,
    Comma: utf8.RuneError,
    Errors: new error[]{errInvalidDelim}.slice()
), new(
    Name: "BadComment1"u8,
    Comment: (rune)'\n',
    Errors: new error[]{errInvalidDelim}.slice()
), new(
    Name: "BadComment2"u8,
    Comment: (rune)'\r',
    Errors: new error[]{errInvalidDelim}.slice()
), new(
    Name: "BadComment3"u8,
    Comment: utf8.RuneError,
    Errors: new error[]{errInvalidDelim}.slice()
), new(
    Name: "BadCommaComment"u8,
    Comma: (rune)'X',
    Comment: (rune)'X',
    Errors: new error[]{errInvalidDelim}.slice()
)
}.slice(); }

public static void TestRead(ж<testing.T> Ꮡt) {
    (ж<global::go.encoding.csv_package.Reader>, slice<slice<array<nint>>>, map<nint, array<nint>>, @string) newReader(readTest tt) {
        var (positions, errPositions, input) = makePositions(tt.Input);
        var r = NewReader(new csv_test_package.strings_ReaderжReader(strings.NewReader(input)));
        if (tt.Comma != 0) {
            r.Value.Comma = tt.Comma;
        }
        r.Value.Comment = tt.Comment;
        if (tt.UseFieldsPerRecord){
            r.Value.FieldsPerRecord = tt.FieldsPerRecord;
        } else {
            r.Value.FieldsPerRecord = -1;
        }
        r.Value.LazyQuotes = tt.LazyQuotes;
        r.Value.TrimLeadingSpace = tt.TrimLeadingSpace;
        r.Value.ReuseRecord = tt.ReuseRecord;
        return (r, positions, errPositions, input);
    }
    foreach (var (_, vᴛ1) in readTests) {
        ref var tt = ref heap(new readTest(), out var Ꮡtt);
        tt = vᴛ1;

        var newReaderʗ1 = newReader;
        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (r, positions, errPositions, input) = newReaderʗ1(ttʗ1);
            var (@out, err) = r.ReadAll();
            {
                var wantErr = firstError(ttʗ1.Errors, positions, errPositions); if (wantErr != default!){
                    if (!reflect.DeepEqual(err, wantErr)) {
                        tΔ1.Fatalf("ReadAll() error mismatch:\ngot  %v (%#v)\nwant %v (%#v)"u8, err, err, wantErr, wantErr);
                    }
                    if (@out != default!) {
                        tΔ1.Fatalf("ReadAll() output:\ngot  %q\nwant nil"u8, @out);
                    }
                } else {
                    if (err != default!) {
                        tΔ1.Fatalf("unexpected Readall() error: %v"u8, err);
                    }
                    if (!reflect.DeepEqual(@out, ttʗ1.Output)) {
                        tΔ1.Fatalf("ReadAll() output:\ngot  %q\nwant %q"u8, @out, ttʗ1.Output);
                    }
                }
            }
            // Check input offset after call ReadAll()
            nint inputByteSize = len(input);
            var inputOffset = r.InputOffset();
            if (err == default! && (int64)inputByteSize != inputOffset) {
                tΔ1.Errorf("wrong input offset after call ReadAll():\ngot:  %d\nwant: %d\ninput: %s"u8, inputOffset, inputByteSize, input);
            }
            // Check field and error positions.
            (r, _, _, _) = newReaderʗ1(ttʗ1);
            for (nint recNum = 0; ᐧ ; recNum++) {
                var (rec, errΔ1) = r.Read();
                error wantErr = default!;
                if (recNum < len(ttʗ1.Errors) && ttʗ1.Errors[recNum] != default!){
                    wantErr = errorWithPosition(ttʗ1.Errors[recNum], recNum, positions, errPositions);
                } else 
                if (recNum >= len(ttʗ1.Output)) {
                    wantErr = io.EOF;
                }
                if (!reflect.DeepEqual(errΔ1, wantErr)) {
                    tΔ1.Fatalf("Read() error at record %d:\ngot %v (%#v)\nwant %v (%#v)"u8, recNum, errΔ1, errΔ1, wantErr, wantErr);
                }
                // ErrFieldCount is explicitly non-fatal.
                if (errΔ1 != default! && !errors.Is(errΔ1, ErrFieldCount)) {
                    if (recNum < len(ttʗ1.Output)) {
                        tΔ1.Fatalf("need more records; got %d want %d"u8, recNum, len(ttʗ1.Output));
                    }
                    break;
                }
                {
                    var (got, want) = (rec, ttʗ1.Output[recNum]); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                        tΔ1.Errorf("Read vs ReadAll mismatch;\ngot %q\nwant %q"u8, got, want);
                    }
                }
                var pos = positions[recNum];
                if (len(pos) != len(rec)) {
                    tΔ1.Fatalf("mismatched position length at record %d"u8, recNum);
                }
                foreach (var (i, _) in rec) {
                    var (line, col) = r.FieldPos(i);
                    {
                        var (got, want) = (new nint[]{line, col}.array(), pos[i].Clone()); if (got != want) {
                            tΔ1.Errorf("position mismatch at record %d, field %d;\ngot %v\nwant %v"u8, recNum, i, got, want);
                        }
                    }
                }
            }
        });
    }
}

// firstError returns the first non-nil error in errs,
// with the position adjusted according to the error's
// index inside positions.
internal static error firstError(slice<error> errs, slice<slice<array<nint>>> positions, map<nint, array<nint>> errPositions) {
    foreach (var (i, err) in errs) {
        if (err != default!) {
            return errorWithPosition(err, i, positions, errPositions);
        }
    }
    return default!;
}

internal static error errorWithPosition(error err, nint recNum, slice<slice<array<nint>>> positions, map<nint, array<nint>> errPositions) {
    var (parseErr, ok) = err._<ж<global::go.encoding.csv_package.ParseError>>(ᐧ);
    if (!ok) {
        return err;
    }
    if (recNum >= len(positions)) {
        throw panic(fmt.Errorf("no positions found for error at record %d"u8, recNum));
    }
    (var errPos, ok) = errPositions[recNum, () => new array<nint>(2), ꟷ];
    if (!ok) {
        throw panic(fmt.Errorf("no error position found for error at record %d"u8, recNum));
    }
    ref var parseErr1 = ref heap<global::go.encoding.csv_package.ParseError>(out var ᏑparseErr1);
    parseErr1 = parseErr.Value;
    parseErr1.StartLine = positions[recNum][0][0];
    parseErr1.Line = errPos[0];
    parseErr1.Column = errPos[1];
    return new global::go.encoding.csv_package.ParseErrorжerror(ᏑparseErr1);
}

// makePositions returns the expected field positions of all
// the fields in text, the positions of any errors, and the text with the position markers
// removed.
//
// The start of each field is marked with a § symbol;
// CSV lines are separated by ¶ symbols;
// Error positions are marked with ∑ symbols.
internal static (slice<slice<array<nint>>>, map<nint, array<nint>>, @string) makePositions(@string text) {
    var buf = new slice<byte>(0, len(text));
    slice<slice<array<nint>>> positions = default!;
    var errPositions = new map<nint, array<nint>>();
    nint line = 1;
    nint col = 1;
    nint recNum = 0;
    while (len(text) > 0) {
        var (r, size) = utf8.DecodeRuneInString(text);
        switch (r) {
        case (rune)'\n': {
            line++;
            col = 1;
            buf = append(buf, (byte)((rune)'\n'));
            break;
        }
        case (rune)'§': {
            if (len(positions) == 0) {
                positions = append(positions, GoReflect.WithElemDims(new array<nint>[]{}.slice(), 2));
            }
            positions[len(positions) - 1] = append(positions[len(positions) - 1], new nint[]{line, col}.array());
            break;
        }
        case (rune)'¶': {
            positions = append(positions, GoReflect.WithElemDims(new array<nint>[]{}.slice(), 2));
            recNum++;
            break;
        }
        case (rune)'∑': {
            errPositions[recNum] = new nint[]{line, col}.array();
            break;
        }
        default: {
            buf = append(buf, text.slice(0, size).ꓸꓸꓸ);
            col += size;
            break;
        }}

        text = text.slice(size);
    }
    return (positions, errPositions, ((@string)buf));
}

// nTimes is an io.Reader which yields the string s n times.
[GoType] internal partial struct nTimes {
    internal @string s;
    internal nint n;
    internal nint off;
}

[GoRecv] internal static (nint n, error err) Read(this ref nTimes r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    while (ᐧ) {
        if (r.n <= 0 || r.s == ""u8) {
            return (n, io.EOF);
        }
        nint n0 = copy(p, r.s.slice(r.off));
        p = p.slice(n0);
        n += n0;
        r.off += n0;
        if (r.off == len(r.s)) {
            r.off = 0;
            r.n--;
        }
        if (len(p) == 0) {
            return (n, err);
        }
    }
}

// benchmarkRead measures reading the provided CSV rows data.
// initReader, if non-nil, modifies the Reader before it's used.
internal static void benchmarkRead(ж<testing.B> Ꮡb, Action<ж<global::go.encoding.csv_package.Reader>> initReader, @string rows) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var r = NewReader(new csv_internal_test_package.nTimesжReader(Ꮡ(new nTimes(s: rows, n: b.N))));
    if (initReader != default!) {
        initReader(r);
    }
    while (ᐧ) {
        var (_, err) = r.Read();
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
    }
}

internal static readonly @string benchmarkCSVData = """
x,y,z,w
x,y,z,
x,y,,
x,,,
,,,
"x","y","z","w"
"x","y","z",""
"x","y","",""
"x","","",""
"","","",""

"""u8;

public static void BenchmarkRead(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, default!, benchmarkCSVData);
}

public static void BenchmarkReadWithFieldsPerRecord(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.Value.FieldsPerRecord = 4;
    }, benchmarkCSVData);
}

public static void BenchmarkReadWithoutFieldsPerRecord(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.Value.FieldsPerRecord = -1;
    }, benchmarkCSVData);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xxxxxxxxxxxxxxxxˢ = """
xxxxxxxxxxxxxxxx,yyyyyyyyyyyyyyyy,zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz,wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww,vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
xxxxxxxxxxxxxxxxxxxxxxxx,yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy,zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz,wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww,vvvv
,,zzzz,wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww,vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx,yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy,zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz,wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww,vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

"""u8;

public static void BenchmarkReadLargeFields(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, default!, strings.Repeat(xxxxxxxxxxxxxxxxˢ, 3));
}

public static void BenchmarkReadReuseRecord(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.Value.ReuseRecord = true;
    }, benchmarkCSVData);
}

public static void BenchmarkReadReuseRecordWithFieldsPerRecord(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.Value.ReuseRecord = true;
        r.Value.FieldsPerRecord = 4;
    }, benchmarkCSVData);
}

public static void BenchmarkReadReuseRecordWithoutFieldsPerRecord(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.Value.ReuseRecord = true;
        r.Value.FieldsPerRecord = -1;
    }, benchmarkCSVData);
}

public static void BenchmarkReadReuseRecordLargeFields(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.Value.ReuseRecord = true;
    }, strings.Repeat(xxxxxxxxxxxxxxxxˢ, 3));
}

} // end csv_internal_test_package
