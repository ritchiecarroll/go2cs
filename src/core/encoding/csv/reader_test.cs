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

internal partial struct readTest {
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
internal static slice<readTest> readTests;
internal static void initᴛreadTests() { readTests = slice<readTest>([new(
    Name: "Simple",
    Input: "§a,§b,§c\n",
    Output: [["a", "b", "c"]]
), new(
    Name: "CRLF",
    Input: "§a,§b\r\n¶§c,§d\r\n",
    Output: [["a", "b"], ["c", "d"]]
), new(
    Name: "BareCR",
    Input: "§a,§b\rc,§d\r\n",
    Output: [["a", "b\rc", "d"]]
), new(
    Name: "RFC4180test",
    Input: """
§#field1,§field2,§field3
¶§"aaa",§"bb
b",§"ccc"
¶§"a,a",§"b""bb",§"ccc"
¶§zzz,§yyy,§xxx

"""u8.Raw(),
    Output: [
        ["#field1", "field2", "field3"],
        ["aaa", "bb\nb", "ccc"],
        ["a,a", @"b""bb", "ccc"],
        ["zzz", "yyy", "xxx"]
    ],
    UseFieldsPerRecord: true,
    FieldsPerRecord: 0
), new(
    Name: "NoEOLTest",
    Input: "§a,§b,§c",
    Output: [["a", "b", "c"]]
), new(
    Name: "Semicolon",
    Input: "§a;§b;§c\n",
    Output: [["a", "b", "c"]],
    Comma: ';'
), new(
    Name: "MultiLine",
    Input: """
§"two
line",§"one line",§"three
line
field"
"""u8.Raw(),
    Output: [["two\nline", "one line", "three\nline\nfield"]]
), new(
    Name: "BlankLine",
    Input: "§a,§b,§c\n\n¶§d,§e,§f\n\n",
    Output: [
        ["a", "b", "c"],
        ["d", "e", "f"]
    ]
), new(
    Name: "BlankLineFieldCount",
    Input: "§a,§b,§c\n\n¶§d,§e,§f\n\n",
    Output: [
        ["a", "b", "c"],
        ["d", "e", "f"]
    ],
    UseFieldsPerRecord: true,
    FieldsPerRecord: 0
), new(
    Name: "TrimSpace",
    Input: " §a,  §b,   §c\n",
    Output: [["a", "b", "c"]],
    TrimLeadingSpace: true
), new(
    Name: "LeadingSpace",
    Input: "§ a,§  b,§   c\n",
    Output: [[" a", "  b", "   c"]]
), new(
    Name: "Comment",
    Input: "#1,2,3\n§a,§b,§c\n#comment",
    Output: [["a", "b", "c"]],
    Comment: '#'
), new(
    Name: "NoComment",
    Input: "§#1,§2,§3\n¶§a,§b,§c",
    Output: [["#1", "2", "3"], ["a", "b", "c"]]
), new(
    Name: "LazyQuotes",
    Input: @"§a ""word"",§""1""2"",§a"",§""b",
    Output: [[@"a ""word""", @"1""2", @"a""", @"b"]],
    LazyQuotes: true
), new(
    Name: "BareQuotes",
    Input: @"§a ""word"",§""1""2"",§a""",
    Output: [[@"a ""word""", @"1""2", @"a"""]],
    LazyQuotes: true
), new(
    Name: "BareDoubleQuotes",
    Input: @"§a""""b,§c",
    Output: [[@"a""""b", @"c"]],
    LazyQuotes: true
), new(
    Name: "BadDoubleQuotes",
    Input: @"§a∑""""b,c",
    Errors: [new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrBareQuote)))]
), new(
    Name: "TrimQuote",
    Input: @" §""a"",§"" b"",§c",
    Output: [["a", " b", "c"]],
    TrimLeadingSpace: true
), new(
    Name: "BadBareQuote",
    Input: @"§a ∑""word"",""b""",
    Errors: [new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrBareQuote)))]
), new(
    Name: "BadTrailingQuote",
    Input: @"§""a word"",b∑""",
    Errors: [new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrBareQuote)))]
), new(
    Name: "ExtraneousQuote",
    Input: @"§""a ∑""word"",""b""",
    Errors: [new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))]
), new(
    Name: "BadFieldCount",
    Input: "§a,§b,§c\n¶∑§d,§e",
    Errors: [default, new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrFieldCount)))],
    Output: [["a", "b", "c"], ["d", "e"]],
    UseFieldsPerRecord: true,
    FieldsPerRecord: 0
), new(
    Name: "BadFieldCountMultiple",
    Input: "§a,§b,§c\n¶∑§d,§e\n¶∑§f",
    Errors: [default, new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrFieldCount))), new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrFieldCount)))],
    Output: [["a", "b", "c"], ["d", "e"], ["f"]],
    UseFieldsPerRecord: true,
    FieldsPerRecord: 0
), new(
    Name: "BadFieldCount1",
    Input: @"§∑a,§b,§c",
    Errors: [new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrFieldCount)))],
    Output: [["a", "b", "c"]],
    UseFieldsPerRecord: true,
    FieldsPerRecord: 2
), new(
    Name: "FieldCount",
    Input: "§a,§b,§c\n¶§d,§e",
    Output: [["a", "b", "c"], ["d", "e"]]
), new(
    Name: "TrailingCommaEOF",
    Input: "§a,§b,§c,§",
    Output: [["a", "b", "c", ""]]
), new(
    Name: "TrailingCommaEOL",
    Input: "§a,§b,§c,§\n",
    Output: [["a", "b", "c", ""]]
), new(
    Name: "TrailingCommaSpaceEOF",
    Input: "§a,§b,§c, §",
    Output: [["a", "b", "c", ""]],
    TrimLeadingSpace: true
), new(
    Name: "TrailingCommaSpaceEOL",
    Input: "§a,§b,§c, §\n",
    Output: [["a", "b", "c", ""]],
    TrimLeadingSpace: true
), new(
    Name: "TrailingCommaLine3",
    Input: "§a,§b,§c\n¶§d,§e,§f\n¶§g,§hi,§",
    Output: [["a", "b", "c"], ["d", "e", "f"], ["g", "hi", ""]],
    TrimLeadingSpace: true
), new(
    Name: "NotTrailingComma3",
    Input: "§a,§b,§c,§ \n",
    Output: [["a", "b", "c", " "]]
), new(
    Name: "CommaFieldTest",
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

"""u8.Raw(),
    Output: [
        ["x", "y", "z", "w"],
        ["x", "y", "z", ""],
        ["x", "y", "", ""],
        ["x", "", "", ""],
        ["", "", "", ""],
        ["x", "y", "z", "w"],
        ["x", "y", "z", ""],
        ["x", "y", "", ""],
        ["x", "", "", ""],
        ["", "", "", ""]
    ]
), new(
    Name: "TrailingCommaIneffective1",
    Input: "§a,§b,§\n¶§c,§d,§e",
    Output: [
        ["a", "b", ""],
        ["c", "d", "e"]
    ],
    TrimLeadingSpace: true
), new(
    Name: "ReadAllReuseRecord",
    Input: "§a,§b\n¶§c,§d",
    Output: [
        ["a", "b"],
        ["c", "d"]
    ],
    ReuseRecord: true
), new(
    Name: "StartLine1", // Issue 19019
    Input: "§a,\"b\nc∑\"d,e",
    Errors: [new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))]
), new(
    Name: "StartLine2",
    Input: "§a,§b\n¶§\"d\n\n,e∑",
    Errors: [default, new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))],
    Output: [["a", "b"]]
), new(
    Name: "CRLFInQuotedField", // Issue 21201
    Input: "§A,§\"Hello\r\nHi\",§B\r\n",
    Output: [
        ["A", "Hello\nHi", "B"]
    ]
), new(
    Name: "BinaryBlobField", // Issue 19410
    Input: ((@string)(new byte[]{0xc2, 0xa7, 0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c, 0x2c, 0xc2, 0xa7, 0x61, 0x6b, 0x74, 0x61, 0x75})),
    Output: [[((@string)(new byte[]{0x78, 0x30, 0x39, 0x41, 0xb4, 0x1c})), "aktau"]]
), new(
    Name: "TrailingCR",
    Input: "§field1,§field2\r",
    Output: [["field1", "field2"]]
), new(
    Name: "QuotedTrailingCR",
    Input: "§\"field\"\r",
    Output: [["field"]]
), new(
    Name: "QuotedTrailingCRCR",
    Input: "§\"field∑\"\r\r",
    Errors: [new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))]
), new(
    Name: "FieldCR",
    Input: "§field\rfield\r",
    Output: [["field\rfield"]]
), new(
    Name: "FieldCRCR",
    Input: "§field\r\rfield\r\r",
    Output: [["field\r\rfield\r"]]
), new(
    Name: "FieldCRCRLF",
    Input: "§field\r\r\n¶§field\r\r\n",
    Output: [["field\r"], ["field\r"]]
), new(
    Name: "FieldCRCRLFCR",
    Input: "§field\r\r\n¶§\rfield\r\r\n\r",
    Output: [["field\r"], ["\rfield\r"]]
), new(
    Name: "FieldCRCRLFCRCR",
    Input: "§field\r\r\n¶§\r\rfield\r\r\n¶§\r\r",
    Output: [["field\r"], ["\r\rfield\r"], ["\r"]]
), new(
    Name: "MultiFieldCRCRLFCRCR",
    Input: "§field1,§field2\r\r\n¶§\r\rfield1,§field2\r\r\n¶§\r\r,§",
    Output: [
        ["field1", "field2\r"],
        ["\r\rfield1", "field2\r"],
        ["\r\r", ""]
    ]
), new(
    Name: "NonASCIICommaAndComment",
    Input: "§a£§b,c£ \t§d,e\n€ comment\n",
    Output: [["a", "b,c", "d,e"]],
    TrimLeadingSpace: true,
    Comma: '£',
    Comment: '€'
), new(
    Name: "NonASCIICommaAndCommentWithQuotes",
    Input: "§a€§\"  b,\"€§ c\nλ comment\n",
    Output: [["a", "  b,", " c"]],
    Comma: '€',
    Comment: 'λ'
), new(
    // λ and θ start with the same byte.
    // This tests that the parser doesn't confuse such characters.
    Name: "NonASCIICommaConfusion",
    Input: "§\"abθcd\"λ§efθgh",
    Output: [["abθcd", "efθgh"]],
    Comma: 'λ',
    Comment: '€'
), new(
    Name: "NonASCIICommentConfusion",
    Input: "§λ\n¶§λ\nθ\n¶§λ\n",
    Output: [["λ"], ["λ"], ["λ"]],
    Comment: 'θ'
), new(
    Name: "QuotedFieldMultipleLF",
    Input: "§\"\n\n\n\n\"",
    Output: [["\n\n\n\n"]]
), new(
    Name: "MultipleCRLF",
    Input: "\r\n\r\n\r\n\r\n"
), new(
    // The implementation may read each line in several chunks if it doesn't fit entirely
    // in the read buffer, so we should test the code to handle that condition.
    Name: "HugeLines",
    Input: strings.Repeat("#ignore\n", 10000) + "§"u8 + strings.Repeat("@", 5000) + ",§"u8 + strings.Repeat("*", 5000),
    Output: slice<slice<@string>>([slice<@string>([strings.Repeat("@", 5000), strings.Repeat("*", 5000)])]),
    Comment: '#'
), new(
    Name: "QuoteWithTrailingCRLF",
    Input: "§\"foo∑\"bar\"\r\n",
    Errors: [new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))]
), new(
    Name: "LazyQuoteWithTrailingCRLF",
    Input: "§\"foo\"bar\"\r\n",
    Output: [[@"foo""bar"]],
    LazyQuotes: true
), new(
    Name: "DoubleQuoteWithTrailingCRLF",
    Input: "§\"foo\"\"bar\"\r\n",
    Output: [[@"foo""bar"]]
), new(
    Name: "EvenQuotes",
    Input: @"§""""""""""""""""",
    Output: [[@""""""""]]
), new(
    Name: "OddQuotes",
    Input: @"§""""""""""""""∑",
    Errors: [new global::go.encoding.csv_package.ParseErrorжerror(Ꮡ(new ParseError(Err: ErrQuote)))]
), new(
    Name: "LazyOddQuotes",
    Input: @"§""""""""""""""",
    Output: [[@""""""""]],
    LazyQuotes: true
), new(
    Name: "BadComma1",
    Comma: '\n',
    Errors: [errInvalidDelim]
), new(
    Name: "BadComma2",
    Comma: '\r',
    Errors: [errInvalidDelim]
), new(
    Name: "BadComma3",
    Comma: '"',
    Errors: [errInvalidDelim]
), new(
    Name: "BadComma4",
    Comma: utf8.RuneError,
    Errors: [errInvalidDelim]
), new(
    Name: "BadComment1",
    Comment: '\n',
    Errors: [errInvalidDelim]
), new(
    Name: "BadComment2",
    Comment: '\r',
    Errors: [errInvalidDelim]
), new(
    Name: "BadComment3",
    Comment: utf8.RuneError,
    Errors: [errInvalidDelim]
), new(
    Name: "BadCommaComment",
    Comma: 'X',
    Comment: 'X',
    Errors: [errInvalidDelim]
)
]); }

public static void TestRead(ж<testing.T> t) {
    (ж<global::go.encoding.csv_package.Reader>, slice<slice<array<nint>>>, map<nint, array<nint>>, @string) newReader(readTest tt) {
        var (positions, errPositions, input) = makePositions(tt.Input);
        var r = NewReader(new csv_test_package.strings_ReaderжReader(strings.NewReader(input)));

        if (tt.Comma != 0) {
            r.Comma = tt.Comma;
        }
        r.Comment = tt.Comment;
        if (tt.UseFieldsPerRecord) {
            r.FieldsPerRecord = tt.FieldsPerRecord;
        } else {
            r.FieldsPerRecord = -1;
        }
        r.LazyQuotes = tt.LazyQuotes;
        r.TrimLeadingSpace = tt.TrimLeadingSpace;
        r.ReuseRecord = tt.ReuseRecord;
        return (r, positions, errPositions, input);
    }

    foreach (var (_, vᴛ1) in range(readTests)) {
        ref var tt = ref heap(new readTest(), out var Ꮡtt);
        tt = vᴛ1;

        var newReaderʗ1 = newReader;
        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (r, positions, errPositions, input) = newReaderʗ1(ttʗ1);
            var (@out, err) = r.ReadAll();
            {
                var wantErr = firstError(ttʗ1.Errors, positions, errPositions); if (wantErr != nil) {
                    if (!reflect.DeepEqual(err, wantErr)) {
                        tΔ1.Fatalf("ReadAll() error mismatch:\ngot  %v (%#v)\nwant %v (%#v)", err, err, wantErr, wantErr);
                    }
                    if (@out != nil) {
                        tΔ1.Fatalf("ReadAll() output:\ngot  %q\nwant nil", @out);
                    }
                } else {
                    if (err != nil) {
                        tΔ1.Fatalf("unexpected Readall() error: %v", err);
                    }
                    if (!reflect.DeepEqual(@out, ttʗ1.Output)) {
                        tΔ1.Fatalf("ReadAll() output:\ngot  %q\nwant %q", @out, ttʗ1.Output);
                    }
                }
            }

            // Check input offset after call ReadAll()
            nint inputByteSize = len(input);
            var inputOffset = r.InputOffset();
            if (err == nil && (int64)inputByteSize != inputOffset) {
                tΔ1.Errorf("wrong input offset after call ReadAll():\ngot:  %d\nwant: %d\ninput: %s", inputOffset, inputByteSize, input);
            }

            // Check field and error positions.
            (r, _, _, _) = newReaderʗ1(ttʗ1);
            for (nint recNum = 0; ; recNum++) {
                var (rec, errΔ1) = r.Read();
                error wantErr = default;
                if (recNum < len(ttʗ1.Errors) && ttʗ1.Errors[recNum] != nil) {
                    wantErr = errorWithPosition(ttʗ1.Errors[recNum], recNum, positions, errPositions);
                } else if (recNum >= len(ttʗ1.Output)) {
                    wantErr = io.EOF;
                }
                if (!reflect.DeepEqual(errΔ1, wantErr)) {
                    tΔ1.Fatalf("Read() error at record %d:\ngot %v (%#v)\nwant %v (%#v)", recNum, errΔ1, errΔ1, wantErr, wantErr);
                }
                // ErrFieldCount is explicitly non-fatal.
                if (errΔ1 != nil && !errors.Is(errΔ1, ErrFieldCount)) {
                    if (recNum < len(ttʗ1.Output)) {
                        tΔ1.Fatalf("need more records; got %d want %d", recNum, len(ttʗ1.Output));
                    }
                    break;
                }
                {
                    var (got, want) = (rec, ttʗ1.Output[recNum]); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                        tΔ1.Errorf("Read vs ReadAll mismatch;\ngot %q\nwant %q", got, want);
                    }
                }
                var pos = positions[recNum];
                if (len(pos) != len(rec)) {
                    tΔ1.Fatalf("mismatched position length at record %d", recNum);
                }
                foreach (var i in range(len(rec))) {
                    var (line, col) = r.FieldPos(i);
                    var (got, want) = (new nint[]{line, col}.array(), pos[i].Clone()); if (got != want) {
                        tΔ1.Errorf("position mismatch at record %d, field %d;\ngot %v\nwant %v", recNum, i, got, want);
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
    foreach (var (i, err) in range(errs)) {
        if (err != nil) {
            return errorWithPosition(err, i, positions, errPositions);
        }
    }
    return default;
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
    ref var parseErr1 = ref heap<global::go.encoding.csv_package.ParseError>(parseErr.Value, out var ᏑparseErr1);
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
    var bufᴷ = new stackAppend<byte>();

    var buf = new slice<byte>(0, len(text));
    slice<slice<array<nint>>> positions = default;
    var errPositions = new map<nint, array<nint>>();
    nint line = 1;
    nint col = 1;
    nint recNum = 0;

    while (len(text) > 0) {
        var (r, size) = utf8.DecodeRuneInString(text);
        switch (r) {
        case '\n': {
            line++;
            col = 1;
            buf = append(bufᴷ, buf, (byte)'\n');
            break;
        }
        case '§': {
            if (len(positions) == 0) {
                positions = append(positions, GoReflect.WithElemDims(new array<nint>[]{}.slice(), 2));
            }
            positions[len(positions) - 1] = append(positions[len(positions) - 1], new nint[]{line, col}.array());
            break;
        }
        case '¶': {
            positions = append(positions, GoReflect.WithElemDims(new array<nint>[]{}.slice(), 2));
            recNum++;
            break;
        }
        case '∑': {
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
    return (positions, errPositions, (@string)buf);
}

// nTimes is an io.Reader which yields the string s n times.
internal partial struct nTimes {
    internal @string s;
    internal nint n;
    internal nint off;
}

internal static (nint n, error err) Read(this ref nTimes r, slice<byte> p) {
    nint n = default;
    error err = default;

    for (;;) {
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
    if (initReader != nil) {
        initReader(r);
    }
    for (;;) {
        var (_, err) = r.Read();
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
    }
}

internal static ref readonly @string benchmarkCSVData => ref benchmarkCSVDataᵇ;
private static readonly @string benchmarkCSVDataᵇ = """
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

"""u8.Raw();

public static void BenchmarkRead(ж<testing.B> b) {
    benchmarkRead(b, default, benchmarkCSVData);
}

public static void BenchmarkReadWithFieldsPerRecord(ж<testing.B> b) {
    benchmarkRead(b, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.FieldsPerRecord = 4;
    }, benchmarkCSVData);
}

public static void BenchmarkReadWithoutFieldsPerRecord(ж<testing.B> b) {
    benchmarkRead(b, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.FieldsPerRecord = -1;
    }, benchmarkCSVData);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xxxxxxxxxxxxxxxxˢ => ref xxxxxxxxxxxxxxxxˢᵇ;
private static readonly @string xxxxxxxxxxxxxxxxˢᵇ = """
xxxxxxxxxxxxxxxx,yyyyyyyyyyyyyyyy,zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz,wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww,vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
xxxxxxxxxxxxxxxxxxxxxxxx,yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy,zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz,wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww,vvvv
,,zzzz,wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww,vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx,yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy,zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz,wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww,vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

"""u8.Raw();

public static void BenchmarkReadLargeFields(ж<testing.B> b) {
    benchmarkRead(b, default, strings.Repeat(xxxxxxxxxxxxxxxxˢ, 3));
}

public static void BenchmarkReadReuseRecord(ж<testing.B> b) {
    benchmarkRead(b, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.ReuseRecord = true;
    }, benchmarkCSVData);
}

public static void BenchmarkReadReuseRecordWithFieldsPerRecord(ж<testing.B> b) {
    benchmarkRead(b, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.ReuseRecord = true;
        r.FieldsPerRecord = 4;
    }, benchmarkCSVData);
}

public static void BenchmarkReadReuseRecordWithoutFieldsPerRecord(ж<testing.B> b) {
    benchmarkRead(b, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.ReuseRecord = true;
        r.FieldsPerRecord = -1;
    }, benchmarkCSVData);
}

public static void BenchmarkReadReuseRecordLargeFields(ж<testing.B> b) {
    benchmarkRead(b, (ж<global::go.encoding.csv_package.Reader> r) => {
        r.ReuseRecord = true;
    }, strings.Repeat(xxxxxxxxxxxxxxxxˢ, 3));
}

} // end csv_internal_test_package
