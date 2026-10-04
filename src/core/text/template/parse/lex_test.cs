// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text.template;

using fmt = fmt_package;
using testing = testing_package;
using static go.text.template.parse_package;

partial class parse_internal_test_package {

// keywords
// Make the types prettyprint.
internal static map<global::go.text.template.parse_package.itemType, @string> itemName = new map<global::go.text.template.parse_package.itemType, @string>{
    [itemError] = "error"u8,
    [itemBool] = "bool"u8,
    [itemChar] = "char"u8,
    [itemCharConstant] = "charconst"u8,
    [itemComment] = "comment"u8,
    [itemComplex] = "complex"u8,
    [itemDeclare] = ":="u8,
    [itemEOF] = "EOF"u8,
    [itemField] = "field"u8,
    [itemIdentifier] = "identifier"u8,
    [itemLeftDelim] = "left delim"u8,
    [itemLeftParen] = "("u8,
    [itemNumber] = "number"u8,
    [itemPipe] = "pipe"u8,
    [itemRawString] = "raw string"u8,
    [itemRightDelim] = "right delim"u8,
    [itemRightParen] = ")"u8,
    [itemSpace] = "space"u8,
    [itemString] = "string"u8,
    [itemVariable] = "variable"u8,
    [itemDot] = "."u8,
    [itemBlock] = "block"u8,
    [itemBreak] = "break"u8,
    [itemContinue] = "continue"u8,
    [itemDefine] = "define"u8,
    [itemElse] = "else"u8,
    [itemIf] = "if"u8,
    [itemEnd] = "end"u8,
    [itemNil] = "nil"u8,
    [itemRange] = "range"u8,
    [itemTemplate] = "template"u8,
    [itemWith] = "with"u8
};

internal static @string String(this global::go.text.template.parse_package.itemType i) {
    @string s = itemName[i];
    if (s == ""u8) {
        return fmt.Sprintf("item%d"u8, (nint)i);
    }
    return s;
}

[GoType] internal partial struct lexTest {
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

internal static global::go.text.template.parse_package.item tDot = mkItem(itemDot, "."u8);
internal static global::go.text.template.parse_package.item tBlock = mkItem(itemBlock, "block"u8);
internal static global::go.text.template.parse_package.item tEOF = mkItem(itemEOF, ""u8);
internal static global::go.text.template.parse_package.item tFor = mkItem(itemIdentifier, "for"u8);
internal static global::go.text.template.parse_package.item tLeft = mkItem(itemLeftDelim, "{{"u8);
internal static global::go.text.template.parse_package.item tLpar = mkItem(itemLeftParen, "("u8);
internal static global::go.text.template.parse_package.item tPipe = mkItem(itemPipe, "|"u8);
internal static global::go.text.template.parse_package.item tQuote = mkItem(itemString, @"""abc \n\t\"" """u8);
internal static global::go.text.template.parse_package.item tRange = mkItem(itemRange, "range"u8);
internal static global::go.text.template.parse_package.item tRight = mkItem(itemRightDelim, "}}"u8);
internal static global::go.text.template.parse_package.item tRpar = mkItem(itemRightParen, ")"u8);
internal static global::go.text.template.parse_package.item tSpace = mkItem(itemSpace, " "u8);
internal static @string raw = "`"u8 + @"abc\n\t\"" "u8 + "`"u8;
internal static @string rawNL = "`now is{{\n}}the time`"u8; // Contains newline inside raw quote.
internal static global::go.text.template.parse_package.item tRawQuote = mkItem(itemRawString, raw);
internal static global::go.text.template.parse_package.item tRawQuoteNL = mkItem(itemRawString, rawNL);

// errors
// Fixed bugs
// Many elements in an action blew the lookahead until
// we made lexInsideAction not loop.
// This one is an error that we can't catch because it breaks templates with
// minimized JavaScript. Should have fixed it before Go 1.1.
internal static slice<lexTest> lexTests = new lexTest[]{
    new("empty"u8, ""u8, new global::go.text.template.parse_package.item[]{tEOF}.slice()),
    new("spaces"u8, " \t\n"u8, new global::go.text.template.parse_package.item[]{mkItem(itemText, " \t\n"u8), tEOF}.slice()),
    new("text"u8, @"now is the time"u8, new global::go.text.template.parse_package.item[]{mkItem(itemText, "now is the time"u8), tEOF}.slice()),
    new("text with comment"u8, "hello-{{/* this is a comment */}}-world"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemText, "hello-"u8),
        mkItem(itemComment, "/* this is a comment */"u8),
        mkItem(itemText, "-world"u8),
        tEOF
    }.slice()),
    new("punctuation"u8, "{{,@% }}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemChar, ","u8),
        mkItem(itemChar, "@"u8),
        mkItem(itemChar, "%"u8),
        tSpace,
        tRight,
        tEOF
    }.slice()),
    new("parens"u8, "{{((3))}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        tLpar,
        tLpar,
        mkItem(itemNumber, "3"u8),
        tRpar,
        tRpar,
        tRight,
        tEOF
    }.slice()),
    new("empty action"u8, @"{{}}"u8, new global::go.text.template.parse_package.item[]{tLeft, tRight, tEOF}.slice()),
    new("for"u8, @"{{for}}"u8, new global::go.text.template.parse_package.item[]{tLeft, tFor, tRight, tEOF}.slice()),
    new("block"u8, @"{{block ""foo"" .}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft, tBlock, tSpace, mkItem(itemString, @"""foo"""u8), tSpace, tDot, tRight, tEOF
    }.slice()),
    new("quote"u8, @"{{""abc \n\t\"" ""}}"u8, new global::go.text.template.parse_package.item[]{tLeft, tQuote, tRight, tEOF}.slice()),
    new("raw quote"u8, "{{"u8 + raw + "}}"u8, new global::go.text.template.parse_package.item[]{tLeft, tRawQuote, tRight, tEOF}.slice()),
    new("raw quote with newline"u8, "{{"u8 + rawNL + "}}"u8, new global::go.text.template.parse_package.item[]{tLeft, tRawQuoteNL, tRight, tEOF}.slice()),
    new("numbers"u8, "{{1 02 0x14 0X14 -7.2i 1e3 1E3 +1.2e-4 4.2i 1+2i 1_2 0x1.e_fp4 0X1.E_FP4}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemNumber, "1"u8),
        tSpace,
        mkItem(itemNumber, "02"u8),
        tSpace,
        mkItem(itemNumber, "0x14"u8),
        tSpace,
        mkItem(itemNumber, "0X14"u8),
        tSpace,
        mkItem(itemNumber, "-7.2i"u8),
        tSpace,
        mkItem(itemNumber, "1e3"u8),
        tSpace,
        mkItem(itemNumber, "1E3"u8),
        tSpace,
        mkItem(itemNumber, "+1.2e-4"u8),
        tSpace,
        mkItem(itemNumber, "4.2i"u8),
        tSpace,
        mkItem(itemComplex, "1+2i"u8),
        tSpace,
        mkItem(itemNumber, "1_2"u8),
        tSpace,
        mkItem(itemNumber, "0x1.e_fp4"u8),
        tSpace,
        mkItem(itemNumber, "0X1.E_FP4"u8),
        tRight,
        tEOF
    }.slice()),
    new("characters"u8, @"{{'a' '\n' '\'' '\\' '\u00FF' '\xFF' '本'}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemCharConstant, @"'a'"u8),
        tSpace,
        mkItem(itemCharConstant, @"'\n'"u8),
        tSpace,
        mkItem(itemCharConstant, @"'\''"u8),
        tSpace,
        mkItem(itemCharConstant, @"'\\'"u8),
        tSpace,
        mkItem(itemCharConstant, @"'\u00FF'"u8),
        tSpace,
        mkItem(itemCharConstant, @"'\xFF'"u8),
        tSpace,
        mkItem(itemCharConstant, @"'本'"u8),
        tRight,
        tEOF
    }.slice()),
    new("bools"u8, "{{true false}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemBool, "true"u8),
        tSpace,
        mkItem(itemBool, "false"u8),
        tRight,
        tEOF
    }.slice()),
    new("dot"u8, "{{.}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        tDot,
        tRight,
        tEOF
    }.slice()),
    new("nil"u8, "{{nil}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemNil, "nil"u8),
        tRight,
        tEOF
    }.slice()),
    new("dots"u8, "{{.x . .2 .x.y.z}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemField, ".x"u8),
        tSpace,
        tDot,
        tSpace,
        mkItem(itemNumber, ".2"u8),
        tSpace,
        mkItem(itemField, ".x"u8),
        mkItem(itemField, ".y"u8),
        mkItem(itemField, ".z"u8),
        tRight,
        tEOF
    }.slice()),
    new("keywords"u8, "{{range if else end with}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemRange, "range"u8),
        tSpace,
        mkItem(itemIf, "if"u8),
        tSpace,
        mkItem(itemElse, "else"u8),
        tSpace,
        mkItem(itemEnd, "end"u8),
        tSpace,
        mkItem(itemWith, "with"u8),
        tRight,
        tEOF
    }.slice()),
    new("variables"u8, "{{$c := printf $ $hello $23 $ $var.Field .Method}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemVariable, "$c"u8),
        tSpace,
        mkItem(itemDeclare, ":="u8),
        tSpace,
        mkItem(itemIdentifier, "printf"u8),
        tSpace,
        mkItem(itemVariable, "$"u8),
        tSpace,
        mkItem(itemVariable, "$hello"u8),
        tSpace,
        mkItem(itemVariable, "$23"u8),
        tSpace,
        mkItem(itemVariable, "$"u8),
        tSpace,
        mkItem(itemVariable, "$var"u8),
        mkItem(itemField, ".Field"u8),
        tSpace,
        mkItem(itemField, ".Method"u8),
        tRight,
        tEOF
    }.slice()),
    new("variable invocation"u8, "{{$x 23}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemVariable, "$x"u8),
        tSpace,
        mkItem(itemNumber, "23"u8),
        tRight,
        tEOF
    }.slice()),
    new("pipeline"u8, @"intro {{echo hi 1.2 |noargs|args 1 ""hi""}} outro"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemText, "intro "u8),
        tLeft,
        mkItem(itemIdentifier, "echo"u8),
        tSpace,
        mkItem(itemIdentifier, "hi"u8),
        tSpace,
        mkItem(itemNumber, "1.2"u8),
        tSpace,
        tPipe,
        mkItem(itemIdentifier, "noargs"u8),
        tPipe,
        mkItem(itemIdentifier, "args"u8),
        tSpace,
        mkItem(itemNumber, "1"u8),
        tSpace,
        mkItem(itemString, @"""hi"""u8),
        tRight,
        mkItem(itemText, " outro"u8),
        tEOF
    }.slice()),
    new("declaration"u8, "{{$v := 3}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemVariable, "$v"u8),
        tSpace,
        mkItem(itemDeclare, ":="u8),
        tSpace,
        mkItem(itemNumber, "3"u8),
        tRight,
        tEOF
    }.slice()),
    new("2 declarations"u8, "{{$v , $w := 3}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemVariable, "$v"u8),
        tSpace,
        mkItem(itemChar, ","u8),
        tSpace,
        mkItem(itemVariable, "$w"u8),
        tSpace,
        mkItem(itemDeclare, ":="u8),
        tSpace,
        mkItem(itemNumber, "3"u8),
        tRight,
        tEOF
    }.slice()),
    new("field of parenthesized expression"u8, "{{(.X).Y}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        tLpar,
        mkItem(itemField, ".X"u8),
        tRpar,
        mkItem(itemField, ".Y"u8),
        tRight,
        tEOF
    }.slice()),
    new("trimming spaces before and after"u8, "hello- {{- 3 -}} -world"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemText, "hello-"u8),
        tLeft,
        mkItem(itemNumber, "3"u8),
        tRight,
        mkItem(itemText, "-world"u8),
        tEOF
    }.slice()),
    new("trimming spaces before and after comment"u8, "hello- {{- /* hello */ -}} -world"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemText, "hello-"u8),
        mkItem(itemComment, "/* hello */"u8),
        mkItem(itemText, "-world"u8),
        tEOF
    }.slice()),
    new("badchar"u8, "#{{\x01}}"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemText, "#"u8),
        tLeft,
        mkItem(itemError, "unrecognized character in action: U+0001"u8)
    }.slice()),
    new("unclosed action"u8, "{{"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemError, "unclosed action"u8)
    }.slice()),
    new("EOF in action"u8, "{{range"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        tRange,
        mkItem(itemError, "unclosed action"u8)
    }.slice()),
    new("unclosed quote"u8, "{{\"\n\"}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemError, "unterminated quoted string"u8)
    }.slice()),
    new("unclosed raw quote"u8, "{{`xx}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemError, "unterminated raw quoted string"u8)
    }.slice()),
    new("unclosed char constant"u8, "{{'\n}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemError, "unterminated character constant"u8)
    }.slice()),
    new("bad number"u8, "{{3k}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemError, @"bad number syntax: ""3k"""u8)
    }.slice()),
    new("unclosed paren"u8, "{{(3}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        tLpar,
        mkItem(itemNumber, "3"u8),
        mkItem(itemError, @"unclosed left paren"u8)
    }.slice()),
    new("extra right paren"u8, "{{3)}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        mkItem(itemNumber, "3"u8),
        mkItem(itemError, "unexpected right paren"u8)
    }.slice()),
    new("long pipeline deadlock"u8, "{{|||||}}"u8, new global::go.text.template.parse_package.item[]{
        tLeft,
        tPipe,
        tPipe,
        tPipe,
        tPipe,
        tPipe,
        tRight,
        tEOF
    }.slice()),
    new("text with bad comment"u8, "hello-{{/*/}}-world"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemText, "hello-"u8),
        mkItem(itemError, @"unclosed comment"u8)
    }.slice()),
    new("text with comment close separated from delim"u8, "hello-{{/* */ }}-world"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemText, "hello-"u8),
        mkItem(itemError, @"comment ends before closing delimiter"u8)
    }.slice()),
    new("unmatched right delimiter"u8, "hello-{.}}-world"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemText, "hello-{.}}-world"u8),
        tEOF
    }.slice())
}.slice();

// collect gathers the emitted items into a slice.
internal static slice<global::go.text.template.parse_package.item> /*items*/ collect(ж<lexTest> Ꮡt, @string left, @string right) {
    slice<global::go.text.template.parse_package.item> items = default!;

    ref var t = ref Ꮡt.DerefOrNull();
    var l = lex(t.name, t.input, left, right);
    l.Value.options = new lexOptions(
        emitComment: true,
        breakOK: true,
        continueOK: true
    );
    while (ᐧ) {
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
    foreach (var (k, _) in i1) {
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

public static void TestLex(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in lexTests) {
        ref var test = ref heap(new lexTest(), out var Ꮡtest);
        test = vᴛ1;

        var items = collect(Ꮡtest, ""u8, ""u8);
        if (!equal(items, test.items, false)) {
            Ꮡt.Errorf("%s: got\n\t%+v\nexpected\n\t%v"u8, test.name, items, test.items);
            return; // TODO
        }
        Ꮡt.Log(test.name, (@string)"OK"u8);
    }
}

// Some easy cases from above, but with delimiters $$ and @@
internal static slice<lexTest> lexDelimTests;
internal static void initᴛlexDelimTests() { lexDelimTests = new lexTest[]{
    new("punctuation"u8, "$$,@%{{}}@@"u8, new global::go.text.template.parse_package.item[]{
        tLeftDelim,
        mkItem(itemChar, ","u8),
        mkItem(itemChar, "@"u8),
        mkItem(itemChar, "%"u8),
        mkItem(itemChar, "{"u8),
        mkItem(itemChar, "{"u8),
        mkItem(itemChar, "}"u8),
        mkItem(itemChar, "}"u8),
        tRightDelim,
        tEOF
    }.slice()),
    new("empty action"u8, @"$$@@"u8, new global::go.text.template.parse_package.item[]{tLeftDelim, tRightDelim, tEOF}.slice()),
    new("for"u8, @"$$for@@"u8, new global::go.text.template.parse_package.item[]{tLeftDelim, tFor, tRightDelim, tEOF}.slice()),
    new("quote"u8, @"$$""abc \n\t\"" ""@@"u8, new global::go.text.template.parse_package.item[]{tLeftDelim, tQuote, tRightDelim, tEOF}.slice()),
    new("raw quote"u8, "$$"u8 + raw + "@@"u8, new global::go.text.template.parse_package.item[]{tLeftDelim, tRawQuote, tRightDelim, tEOF}.slice())
}.slice(); }

internal static global::go.text.template.parse_package.item tLeftDelim = mkItem(itemLeftDelim, "$$"u8);
internal static global::go.text.template.parse_package.item tRightDelim = mkItem(itemRightDelim, "@@"u8);

public static void TestDelims(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in lexDelimTests) {
        ref var test = ref heap(new lexTest(), out var Ꮡtest);
        test = vᴛ1;

        var items = collect(Ꮡtest, "$$"u8, "@@"u8);
        if (!equal(items, test.items, false)) {
            Ꮡt.Errorf("%s: got\n\t%v\nexpected\n\t%v"u8, test.name, items, test.items);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hubˢ = "{{hub"u8;
internal static readonly @string hostˢ = ".host"u8;
internal static readonly @string hubˢ2 = "hub}}"u8;

public static void TestDelimsAlphaNumeric(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var test = ref heap<lexTest>(out var Ꮡtest);
    test = new lexTest("right delimiter with alphanumeric start"u8, "{{hub .host hub}}"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemLeftDelim, hubˢ),
        mkItem(itemSpace, " "u8),
        mkItem(itemField, hostˢ),
        mkItem(itemSpace, " "u8),
        mkItem(itemRightDelim, hubˢ2),
        tEOF
    }.slice()
    );
    var items = collect(Ꮡtest, hubˢ, hubˢ2);
    if (!equal(items, test.items, false)) {
        Ꮡt.Errorf("%s: got\n\t%v\nexpected\n\t%v"u8, test.name, items, test.items);
    }
}

public static void TestDelimsAndMarkers(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var test = ref heap<lexTest>(out var Ꮡtest);
    test = new lexTest("delims that look like markers"u8, "{{- .x -}} {{- - .x - -}}"u8, new global::go.text.template.parse_package.item[]{
        mkItem(itemLeftDelim, "{{- "u8),
        mkItem(itemField, ".x"u8),
        mkItem(itemRightDelim, " -}}"u8),
        mkItem(itemLeftDelim, "{{- "u8),
        mkItem(itemField, ".x"u8),
        mkItem(itemRightDelim, " -}}"u8),
        tEOF
    }.slice()
    );
    var items = collect(Ꮡtest, "{{- "u8, " -}}"u8);
    if (!equal(items, test.items, false)) {
        Ꮡt.Errorf("%s: got\n\t%v\nexpected\n\t%v"u8, test.name, items, test.items);
    }
}

internal static slice<lexTest> lexPosTests = new lexTest[]{
    new("empty"u8, ""u8, new global::go.text.template.parse_package.item[]{new(itemEOF, 0, ""u8, 1)}.slice()),
    new("punctuation"u8, "{{,@%#}}"u8, new global::go.text.template.parse_package.item[]{
        new(itemLeftDelim, 0, "{{"u8, 1),
        new(itemChar, 2, ","u8, 1),
        new(itemChar, 3, "@"u8, 1),
        new(itemChar, 4, "%"u8, 1),
        new(itemChar, 5, "#"u8, 1),
        new(itemRightDelim, 6, "}}"u8, 1),
        new(itemEOF, 8, ""u8, 1)
    }.slice()),
    new("sample"u8, "0123{{hello}}xyz"u8, new global::go.text.template.parse_package.item[]{
        new(itemText, 0, "0123"u8, 1),
        new(itemLeftDelim, 4, "{{"u8, 1),
        new(itemIdentifier, 6, "hello"u8, 1),
        new(itemRightDelim, 11, "}}"u8, 1),
        new(itemText, 13, "xyz"u8, 1),
        new(itemEOF, 16, ""u8, 1)
    }.slice()),
    new("trimafter"u8, "{{x -}}\n{{y}}"u8, new global::go.text.template.parse_package.item[]{
        new(itemLeftDelim, 0, "{{"u8, 1),
        new(itemIdentifier, 2, "x"u8, 1),
        new(itemRightDelim, 5, "}}"u8, 1),
        new(itemLeftDelim, 8, "{{"u8, 2),
        new(itemIdentifier, 10, "y"u8, 2),
        new(itemRightDelim, 11, "}}"u8, 2),
        new(itemEOF, 13, ""u8, 2)
    }.slice()),
    new("trimbefore"u8, "{{x}}\n{{- y}}"u8, new global::go.text.template.parse_package.item[]{
        new(itemLeftDelim, 0, "{{"u8, 1),
        new(itemIdentifier, 2, "x"u8, 1),
        new(itemRightDelim, 3, "}}"u8, 1),
        new(itemLeftDelim, 6, "{{"u8, 2),
        new(itemIdentifier, 10, "y"u8, 2),
        new(itemRightDelim, 11, "}}"u8, 2),
        new(itemEOF, 13, ""u8, 2)
    }.slice()),
    new("longcomment"u8, "{{/*\n*/}}\n{{undefinedFunction \"test\"}}"u8, new global::go.text.template.parse_package.item[]{
        new(itemComment, 2, "/*\n*/"u8, 1),
        new(itemText, 9, "\n"u8, 2),
        new(itemLeftDelim, 10, "{{"u8, 3),
        new(itemIdentifier, 12, "undefinedFunction"u8, 3),
        new(itemSpace, 29, " "u8, 3),
        new(itemString, 30, "\"test\""u8, 3),
        new(itemRightDelim, 36, "}}"u8, 3),
        new(itemEOF, 38, ""u8, 3)
    }.slice())
}.slice();

// The other tests don't check position, to make the test cases easier to construct.
// This one does.
public static void TestPos(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in lexPosTests) {
        ref var test = ref heap(new lexTest(), out var Ꮡtest);
        test = vᴛ1;

        var items = collect(Ꮡtest, ""u8, ""u8);
        if (!equal(items, test.items, true)) {
            Ꮡt.Errorf("%s: got\n\t%v\nexpected\n\t%v"u8, test.name, items, test.items);
            if (len(items) == len(test.items)) {
                // Detailed print; avoid item.String() to expose the position value.
                foreach (var (i, _) in items) {
                    if (!equal(items.slice(i, i + 1), test.items.slice(i, i + 1), true)) {
                        var i1 = items[i];
                        var i2 = test.items[i];
                        Ꮡt.Errorf("\t#%d: got {%v %d %q %d} expected {%v %d %q %d}"u8,
                            i, i1.typ, i1.pos, i1.val, i1.line, i2.typ, i2.pos, i2.val, i2.line);
                    }
                }
            }
        }
    }
}

// parseLexer is a local version of parse that lets us pass in the lexer instead of building it.
// We expect an error, so the tree set and funcs list are explicitly nil.
internal static (ж<global::go.text.template.parse_package.Tree> tree, error err) parseLexer(this ж<global::go.text.template.parse_package.Tree> Ꮡt, ж<global::go.text.template.parse_package.lexer> Ꮡlex) {
    ж<global::go.text.template.parse_package.Tree> tree = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        defer(Ꮡt.recover, Ꮡerr, ref ᒐ);
        t.ParseName = t.Name;
        t.startParse(default!, Ꮡlex, new map<@string, ж<global::go.text.template.parse_package.Tree>>{});
        Ꮡt.parse();
        Ꮡt.add();
        t.stopParse();
        (tree, err) = (Ꮡt, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (tree, Ꮡerr.ValueSlot);
}

} // end parse_internal_test_package
