// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text.template;

using fmt = fmt_package;
using testing = testing_package;
using static go.text.template.parse_package;

partial class parse_internal_test_package {

// Make the types prettyprint.
internal static map<global::go.text.template.parse_package.itemType, @string> itemName = new map<global::go.text.template.parse_package.itemType, @string>{
    [itemError] = "error",
    [itemBool] = "bool",
    [itemChar] = "char",
    [itemCharConstant] = "charconst",
    [itemComment] = "comment",
    [itemComplex] = "complex",
    [itemDeclare] = ":=",
    [itemEOF] = "EOF",
    [itemField] = "field",
    [itemIdentifier] = "identifier",
    [itemLeftDelim] = "left delim",
    [itemLeftParen] = "(",
    [itemNumber] = "number",
    [itemPipe] = "pipe",
    [itemRawString] = "raw string",
    [itemRightDelim] = "right delim",
    [itemRightParen] = ")",
    [itemSpace] = "space",
    [itemString] = "string",
    [itemVariable] = "variable",

    // keywords
    [itemDot] = ".",
    [itemBlock] = "block",
    [itemBreak] = "break",
    [itemContinue] = "continue",
    [itemDefine] = "define",
    [itemElse] = "else",
    [itemIf] = "if",
    [itemEnd] = "end",
    [itemNil] = "nil",
    [itemRange] = "range",
    [itemTemplate] = "template",
    [itemWith] = "with"
};

internal static @string String(this global::go.text.template.parse_package.itemType i) {
    @string s = itemName[i];
    if (s == ""u8) {
        return fmt.Sprintf("item%d"u8, (nint)i);
    }
    return s;
}

internal partial struct lexTest {
    internal @string name;
    internal @string input;
    internal slice<global::go.text.template.parse_package.item> items;
}

internal static global::go.text.template.parse_package.item mkItem(global::go.text.template.parse_package.itemType typ, @string text) {
    return new item(
        typ: typ,
        val: text
    );
}

internal static global::go.text.template.parse_package.item tDot = mkItem(itemDot, ".");
internal static global::go.text.template.parse_package.item tBlock = mkItem(itemBlock, "block");
internal static global::go.text.template.parse_package.item tEOF = mkItem(itemEOF, "");
internal static global::go.text.template.parse_package.item tFor = mkItem(itemIdentifier, "for");
internal static global::go.text.template.parse_package.item tLeft = mkItem(itemLeftDelim, "{{");
internal static global::go.text.template.parse_package.item tLpar = mkItem(itemLeftParen, "(");
internal static global::go.text.template.parse_package.item tPipe = mkItem(itemPipe, "|");
internal static global::go.text.template.parse_package.item tQuote = mkItem(itemString, @"""abc \n\t\"" """);
internal static global::go.text.template.parse_package.item tRange = mkItem(itemRange, "range");
internal static global::go.text.template.parse_package.item tRight = mkItem(itemRightDelim, "}}");
internal static global::go.text.template.parse_package.item tRpar = mkItem(itemRightParen, ")");
internal static global::go.text.template.parse_package.item tSpace = mkItem(itemSpace, " ");
internal static @string raw = "`"u8 + @"abc\n\t\"" "u8 + "`"u8;
internal static @string rawNL = "`now is{{\n}}the time`"; // Contains newline inside raw quote.
internal static global::go.text.template.parse_package.item tRawQuote = mkItem(itemRawString, raw);
internal static global::go.text.template.parse_package.item tRawQuoteNL = mkItem(itemRawString, rawNL);

internal static slice<lexTest> lexTests = slice<lexTest>([
    new("empty", "", slice<global::go.text.template.parse_package.item>([tEOF])),
    new("spaces", " \t\n", slice<global::go.text.template.parse_package.item>([mkItem(itemText, " \t\n"), tEOF])),
    new("text", @"now is the time", slice<global::go.text.template.parse_package.item>([mkItem(itemText, "now is the time"), tEOF])),
    new("text with comment", "hello-{{/* this is a comment */}}-world", slice<global::go.text.template.parse_package.item>([
        mkItem(itemText, "hello-"),
        mkItem(itemComment, "/* this is a comment */"),
        mkItem(itemText, "-world"),
        tEOF
    ])),
    new("punctuation", "{{,@% }}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemChar, ","),
        mkItem(itemChar, "@"),
        mkItem(itemChar, "%"),
        tSpace,
        tRight,
        tEOF
    ])),
    new("parens", "{{((3))}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        tLpar,
        tLpar,
        mkItem(itemNumber, "3"),
        tRpar,
        tRpar,
        tRight,
        tEOF
    ])),
    new("empty action", @"{{}}", slice<global::go.text.template.parse_package.item>([tLeft, tRight, tEOF])),
    new("for", @"{{for}}", slice<global::go.text.template.parse_package.item>([tLeft, tFor, tRight, tEOF])),
    new("block", @"{{block ""foo"" .}}", slice<global::go.text.template.parse_package.item>([
        tLeft, tBlock, tSpace, mkItem(itemString, @"""foo"""), tSpace, tDot, tRight, tEOF
    ])),
    new("quote", @"{{""abc \n\t\"" ""}}", slice<global::go.text.template.parse_package.item>([tLeft, tQuote, tRight, tEOF])),
    new("raw quote", "{{"u8 + raw + "}}"u8, slice<global::go.text.template.parse_package.item>([tLeft, tRawQuote, tRight, tEOF])),
    new("raw quote with newline", "{{"u8 + rawNL + "}}"u8, slice<global::go.text.template.parse_package.item>([tLeft, tRawQuoteNL, tRight, tEOF])),
    new("numbers", "{{1 02 0x14 0X14 -7.2i 1e3 1E3 +1.2e-4 4.2i 1+2i 1_2 0x1.e_fp4 0X1.E_FP4}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemNumber, "1"),
        tSpace,
        mkItem(itemNumber, "02"),
        tSpace,
        mkItem(itemNumber, "0x14"),
        tSpace,
        mkItem(itemNumber, "0X14"),
        tSpace,
        mkItem(itemNumber, "-7.2i"),
        tSpace,
        mkItem(itemNumber, "1e3"),
        tSpace,
        mkItem(itemNumber, "1E3"),
        tSpace,
        mkItem(itemNumber, "+1.2e-4"),
        tSpace,
        mkItem(itemNumber, "4.2i"),
        tSpace,
        mkItem(itemComplex, "1+2i"),
        tSpace,
        mkItem(itemNumber, "1_2"),
        tSpace,
        mkItem(itemNumber, "0x1.e_fp4"),
        tSpace,
        mkItem(itemNumber, "0X1.E_FP4"),
        tRight,
        tEOF
    ])),
    new("characters", @"{{'a' '\n' '\'' '\\' '\u00FF' '\xFF' '本'}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemCharConstant, @"'a'"),
        tSpace,
        mkItem(itemCharConstant, @"'\n'"),
        tSpace,
        mkItem(itemCharConstant, @"'\''"),
        tSpace,
        mkItem(itemCharConstant, @"'\\'"),
        tSpace,
        mkItem(itemCharConstant, @"'\u00FF'"),
        tSpace,
        mkItem(itemCharConstant, @"'\xFF'"),
        tSpace,
        mkItem(itemCharConstant, @"'本'"),
        tRight,
        tEOF
    ])),
    new("bools", "{{true false}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemBool, "true"),
        tSpace,
        mkItem(itemBool, "false"),
        tRight,
        tEOF
    ])),
    new("dot", "{{.}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        tDot,
        tRight,
        tEOF
    ])),
    new("nil", "{{nil}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemNil, "nil"),
        tRight,
        tEOF
    ])),
    new("dots", "{{.x . .2 .x.y.z}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemField, ".x"),
        tSpace,
        tDot,
        tSpace,
        mkItem(itemNumber, ".2"),
        tSpace,
        mkItem(itemField, ".x"),
        mkItem(itemField, ".y"),
        mkItem(itemField, ".z"),
        tRight,
        tEOF
    ])),
    new("keywords", "{{range if else end with}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemRange, "range"),
        tSpace,
        mkItem(itemIf, "if"),
        tSpace,
        mkItem(itemElse, "else"),
        tSpace,
        mkItem(itemEnd, "end"),
        tSpace,
        mkItem(itemWith, "with"),
        tRight,
        tEOF
    ])),
    new("variables", "{{$c := printf $ $hello $23 $ $var.Field .Method}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemVariable, "$c"),
        tSpace,
        mkItem(itemDeclare, ":="),
        tSpace,
        mkItem(itemIdentifier, "printf"),
        tSpace,
        mkItem(itemVariable, "$"),
        tSpace,
        mkItem(itemVariable, "$hello"),
        tSpace,
        mkItem(itemVariable, "$23"),
        tSpace,
        mkItem(itemVariable, "$"),
        tSpace,
        mkItem(itemVariable, "$var"),
        mkItem(itemField, ".Field"),
        tSpace,
        mkItem(itemField, ".Method"),
        tRight,
        tEOF
    ])),
    new("variable invocation", "{{$x 23}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemVariable, "$x"),
        tSpace,
        mkItem(itemNumber, "23"),
        tRight,
        tEOF
    ])),
    new("pipeline", @"intro {{echo hi 1.2 |noargs|args 1 ""hi""}} outro", slice<global::go.text.template.parse_package.item>([
        mkItem(itemText, "intro "),
        tLeft,
        mkItem(itemIdentifier, "echo"),
        tSpace,
        mkItem(itemIdentifier, "hi"),
        tSpace,
        mkItem(itemNumber, "1.2"),
        tSpace,
        tPipe,
        mkItem(itemIdentifier, "noargs"),
        tPipe,
        mkItem(itemIdentifier, "args"),
        tSpace,
        mkItem(itemNumber, "1"),
        tSpace,
        mkItem(itemString, @"""hi"""),
        tRight,
        mkItem(itemText, " outro"),
        tEOF
    ])),
    new("declaration", "{{$v := 3}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemVariable, "$v"),
        tSpace,
        mkItem(itemDeclare, ":="),
        tSpace,
        mkItem(itemNumber, "3"),
        tRight,
        tEOF
    ])),
    new("2 declarations", "{{$v , $w := 3}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemVariable, "$v"),
        tSpace,
        mkItem(itemChar, ","),
        tSpace,
        mkItem(itemVariable, "$w"),
        tSpace,
        mkItem(itemDeclare, ":="),
        tSpace,
        mkItem(itemNumber, "3"),
        tRight,
        tEOF
    ])),
    new("field of parenthesized expression", "{{(.X).Y}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        tLpar,
        mkItem(itemField, ".X"),
        tRpar,
        mkItem(itemField, ".Y"),
        tRight,
        tEOF
    ])),
    new("trimming spaces before and after", "hello- {{- 3 -}} -world", slice<global::go.text.template.parse_package.item>([
        mkItem(itemText, "hello-"),
        tLeft,
        mkItem(itemNumber, "3"),
        tRight,
        mkItem(itemText, "-world"),
        tEOF
    ])),
    new("trimming spaces before and after comment", "hello- {{- /* hello */ -}} -world", slice<global::go.text.template.parse_package.item>([
        mkItem(itemText, "hello-"),
        mkItem(itemComment, "/* hello */"),
        mkItem(itemText, "-world"),
        tEOF
    ])),
    // errors
    new("badchar", "#{{\x01}}", slice<global::go.text.template.parse_package.item>([
        mkItem(itemText, "#"),
        tLeft,
        mkItem(itemError, "unrecognized character in action: U+0001")
    ])),
    new("unclosed action", "{{", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemError, "unclosed action")
    ])),
    new("EOF in action", "{{range", slice<global::go.text.template.parse_package.item>([
        tLeft,
        tRange,
        mkItem(itemError, "unclosed action")
    ])),
    new("unclosed quote", "{{\"\n\"}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemError, "unterminated quoted string")
    ])),
    new("unclosed raw quote", "{{`xx}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemError, "unterminated raw quoted string")
    ])),
    new("unclosed char constant", "{{'\n}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemError, "unterminated character constant")
    ])),
    new("bad number", "{{3k}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemError, @"bad number syntax: ""3k""")
    ])),
    new("unclosed paren", "{{(3}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        tLpar,
        mkItem(itemNumber, "3"),
        mkItem(itemError, @"unclosed left paren")
    ])),
    new("extra right paren", "{{3)}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        mkItem(itemNumber, "3"),
        mkItem(itemError, "unexpected right paren")
    ])),

    // Fixed bugs
    // Many elements in an action blew the lookahead until
    // we made lexInsideAction not loop.
    new("long pipeline deadlock", "{{|||||}}", slice<global::go.text.template.parse_package.item>([
        tLeft,
        tPipe,
        tPipe,
        tPipe,
        tPipe,
        tPipe,
        tRight,
        tEOF
    ])),
    new("text with bad comment", "hello-{{/*/}}-world", slice<global::go.text.template.parse_package.item>([
        mkItem(itemText, "hello-"),
        mkItem(itemError, @"unclosed comment")
    ])),
    new("text with comment close separated from delim", "hello-{{/* */ }}-world", slice<global::go.text.template.parse_package.item>([
        mkItem(itemText, "hello-"),
        mkItem(itemError, @"comment ends before closing delimiter")
    ])),
    // This one is an error that we can't catch because it breaks templates with
    // minimized JavaScript. Should have fixed it before Go 1.1.
    new("unmatched right delimiter", "hello-{.}}-world", slice<global::go.text.template.parse_package.item>([
        mkItem(itemText, "hello-{.}}-world"),
        tEOF
    ]))
]);

// collect gathers the emitted items into a slice.
internal static slice<global::go.text.template.parse_package.item> /*items*/ collect(ж<lexTest> Ꮡt, @string left, @string right) {
    slice<global::go.text.template.parse_package.item> items = default;
    ref var t = ref Ꮡt.DerefOrNull();

    var l = lex(t.name, t.input, left, right);
    l.options = new lexOptions(
        emitComment: true,
        breakOK: true,
        continueOK: true
    );
    for (;;) {
        var item = l.nextItem();
        items = builtin.append(items, item);
        if (item.typ == itemEOF || item.typ == itemError) {
            break;
        }
    }
    return items;
}

internal static bool equal(slice<global::go.text.template.parse_package.item> i1, slice<global::go.text.template.parse_package.item> i2, bool checkPos) {
    if (len(i1) != len(i2)) {
        return false;
    }
    foreach (var k in range(i1)) {
        if (i1[k].typ != i2[k].typ) {
            return false;
        }
        if (i1[k].val != i2[k].val) {
            return false;
        }
        if (checkPos && i1[k].pos != i2[k].pos) {
            return false;
        }
        if (checkPos && i1[k].line != i2[k].line) {
            return false;
        }
    }
    return true;
}

public static void TestLex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in lexTests) {
        ref var test = ref heap(new lexTest(), out var Ꮡtest);
        test = vᴛ1;

        var items = collect(Ꮡtest, ""u8, ""u8);
        if (!equal(items, test.items, false)) {
            t.Errorf("%s: got\n\t%+v\nexpected\n\t%v", test.name, items, test.items);
            return; // TODO
        }
        t.Log(test.name, (@string)"OK");
    }
}

// Some easy cases from above, but with delimiters $$ and @@
internal static slice<lexTest> lexDelimTests;
internal static void initᴛlexDelimTests() { lexDelimTests = slice<lexTest>([
    new("punctuation", "$$,@%{{}}@@", slice<global::go.text.template.parse_package.item>([
        tLeftDelim,
        mkItem(itemChar, ","),
        mkItem(itemChar, "@"),
        mkItem(itemChar, "%"),
        mkItem(itemChar, "{"),
        mkItem(itemChar, "{"),
        mkItem(itemChar, "}"),
        mkItem(itemChar, "}"),
        tRightDelim,
        tEOF
    ])),
    new("empty action", @"$$@@", slice<global::go.text.template.parse_package.item>([tLeftDelim, tRightDelim, tEOF])),
    new("for", @"$$for@@", slice<global::go.text.template.parse_package.item>([tLeftDelim, tFor, tRightDelim, tEOF])),
    new("quote", @"$$""abc \n\t\"" ""@@", slice<global::go.text.template.parse_package.item>([tLeftDelim, tQuote, tRightDelim, tEOF])),
    new("raw quote", "$$"u8 + raw + "@@"u8, slice<global::go.text.template.parse_package.item>([tLeftDelim, tRawQuote, tRightDelim, tEOF]))
]); }

internal static global::go.text.template.parse_package.item tLeftDelim = mkItem(itemLeftDelim, "$$");
internal static global::go.text.template.parse_package.item tRightDelim = mkItem(itemRightDelim, "@@");

public static void TestDelims(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in lexDelimTests) {
        ref var test = ref heap(new lexTest(), out var Ꮡtest);
        test = vᴛ1;

        var items = collect(Ꮡtest, "$$", "@@");
        if (!equal(items, test.items, false)) {
            t.Errorf("%s: got\n\t%v\nexpected\n\t%v", test.name, items, test.items);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hubˢ => ref hubˢᵇ;
private static readonly @string hubˢᵇ = "{{hub"u8;
internal static ref readonly @string hostˢ => ref hostˢᵇ;
private static readonly @string hostˢᵇ = ".host"u8;
internal static ref readonly @string hubˢ2 => ref hubˢ2ᵇ;
private static readonly @string hubˢ2ᵇ = "hub}}"u8;

public static void TestDelimsAlphaNumeric(ж<testing.T> t) {
    ref var test = ref heap<lexTest>(new lexTest("right delimiter with alphanumeric start", "{{hub .host hub}}", slice<global::go.text.template.parse_package.item>([
        mkItem(itemLeftDelim, hubˢ),
        mkItem(itemSpace, " "),
        mkItem(itemField, hostˢ),
        mkItem(itemSpace, " "),
        mkItem(itemRightDelim, hubˢ2),
        tEOF
    ])
    ), out var Ꮡtest);
    var items = collect(Ꮡtest, hubˢ, hubˢ2);

    if (!equal(items, test.items, false)) {
        t.Errorf("%s: got\n\t%v\nexpected\n\t%v", test.name, items, test.items);
    }
}

public static void TestDelimsAndMarkers(ж<testing.T> t) {
    ref var test = ref heap<lexTest>(new lexTest("delims that look like markers", "{{- .x -}} {{- - .x - -}}", slice<global::go.text.template.parse_package.item>([
        mkItem(itemLeftDelim, "{{- "),
        mkItem(itemField, ".x"),
        mkItem(itemRightDelim, " -}}"),
        mkItem(itemLeftDelim, "{{- "),
        mkItem(itemField, ".x"),
        mkItem(itemRightDelim, " -}}"),
        tEOF
    ])
    ), out var Ꮡtest);
    var items = collect(Ꮡtest, "{{- ", " -}}");

    if (!equal(items, test.items, false)) {
        t.Errorf("%s: got\n\t%v\nexpected\n\t%v", test.name, items, test.items);
    }
}

internal static slice<lexTest> lexPosTests = slice<lexTest>([
    new("empty", "", slice<global::go.text.template.parse_package.item>([new(itemEOF, 0, "", 1)])),
    new("punctuation", "{{,@%#}}", slice<global::go.text.template.parse_package.item>([
        new(itemLeftDelim, 0, "{{", 1),
        new(itemChar, 2, ",", 1),
        new(itemChar, 3, "@", 1),
        new(itemChar, 4, "%", 1),
        new(itemChar, 5, "#", 1),
        new(itemRightDelim, 6, "}}", 1),
        new(itemEOF, 8, "", 1)
    ])),
    new("sample", "0123{{hello}}xyz", slice<global::go.text.template.parse_package.item>([
        new(itemText, 0, "0123", 1),
        new(itemLeftDelim, 4, "{{", 1),
        new(itemIdentifier, 6, "hello", 1),
        new(itemRightDelim, 11, "}}", 1),
        new(itemText, 13, "xyz", 1),
        new(itemEOF, 16, "", 1)
    ])),
    new("trimafter", "{{x -}}\n{{y}}", slice<global::go.text.template.parse_package.item>([
        new(itemLeftDelim, 0, "{{", 1),
        new(itemIdentifier, 2, "x", 1),
        new(itemRightDelim, 5, "}}", 1),
        new(itemLeftDelim, 8, "{{", 2),
        new(itemIdentifier, 10, "y", 2),
        new(itemRightDelim, 11, "}}", 2),
        new(itemEOF, 13, "", 2)
    ])),
    new("trimbefore", "{{x}}\n{{- y}}", slice<global::go.text.template.parse_package.item>([
        new(itemLeftDelim, 0, "{{", 1),
        new(itemIdentifier, 2, "x", 1),
        new(itemRightDelim, 3, "}}", 1),
        new(itemLeftDelim, 6, "{{", 2),
        new(itemIdentifier, 10, "y", 2),
        new(itemRightDelim, 11, "}}", 2),
        new(itemEOF, 13, "", 2)
    ])),
    new("longcomment", "{{/*\n*/}}\n{{undefinedFunction \"test\"}}", slice<global::go.text.template.parse_package.item>([
        new(itemComment, 2, "/*\n*/", 1),
        new(itemText, 9, "\n", 2),
        new(itemLeftDelim, 10, "{{", 3),
        new(itemIdentifier, 12, "undefinedFunction", 3),
        new(itemSpace, 29, " ", 3),
        new(itemString, 30, "\"test\"", 3),
        new(itemRightDelim, 36, "}}", 3),
        new(itemEOF, 38, "", 3)
    ]))
]);

// The other tests don't check position, to make the test cases easier to construct.
// This one does.
public static void TestPos(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in lexPosTests) {
        ref var test = ref heap(new lexTest(), out var Ꮡtest);
        test = vᴛ1;

        var items = collect(Ꮡtest, ""u8, ""u8);
        if (!equal(items, test.items, true)) {
            t.Errorf("%s: got\n\t%v\nexpected\n\t%v", test.name, items, test.items);
            if (len(items) == len(test.items)) {
                // Detailed print; avoid item.String() to expose the position value.
                foreach (var i in range(items)) {
                    if (!equal(items.slice(i, i + 1), test.items.slice(i, i + 1), true)) {
                        var i1 = items[i];
                        var i2 = test.items[i];
                        t.Errorf("\t#%d: got {%v %d %q %d} expected {%v %d %q %d}",
                            i, i1.typ, i1.pos, i1.val, i1.line, i2.typ, i2.pos, i2.val, i2.line);
                    }
                }
            }
        }
    }
}

} // end parse_internal_test_package
