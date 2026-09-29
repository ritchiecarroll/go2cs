// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text.template;

using fmt = fmt_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using ꓸꓸꓸany = Span<any>;

partial class parse_package {

// item represents a token or text string returned from the scanner.
[GoType] partial struct item {
    internal itemType typ; // The type of this item.
    internal Pos pos;      // The starting position, in bytes, of this item in the input string.
    internal @string val;  // The value of this item.
    internal nint line;     // The line number at the start of this item.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string eofˢ = "EOF"u8;

internal static @string String(this item i) {
    switch (ᐧ) {
    case {} when i.typ == itemEOF: {
        return eofˢ;
    }
    case {} when i.typ == itemError: {
        return i.val;
    }
    case {} when i.typ > itemKeyword: {
        return fmt.Sprintf("<%s>"u8, i.val);
    }
    case {} when len(i.val) is > 10: {
        return fmt.Sprintf("%.10q..."u8, i.val);
    }}

    return fmt.Sprintf("%q"u8, i.val);
}

[GoType("num:nint")] partial struct itemType;

internal static itemType itemError => /* iota */ 0;       // error occurred; value is text of error
internal static itemType itemBool => 1;        // boolean constant
internal static itemType itemChar => 2;        // printable ASCII character; grab bag for comma etc.
internal static itemType itemCharConstant => 3; // character constant
internal static itemType itemComment => 4;     // comment text
internal static itemType itemComplex => 5;     // complex constant (1+2i); imaginary is just a number
internal static itemType itemAssign => 6;      // equals ('=') introducing an assignment
internal static itemType itemDeclare => 7;     // colon-equals (':=') introducing a declaration
internal static itemType itemEOF => 8;
internal static itemType itemField => 9; // alphanumeric identifier starting with '.'
internal static itemType itemIdentifier => 10; // alphanumeric identifier not starting with '.'
internal static itemType itemLeftDelim => 11; // left action delimiter
internal static itemType itemLeftParen => 12; // '(' inside action
internal static itemType itemNumber => 13; // simple number, including imaginary
internal static itemType itemPipe => 14; // pipe symbol
internal static itemType itemRawString => 15; // raw quoted string (includes quotes)
internal static itemType itemRightDelim => 16; // right action delimiter
internal static itemType itemRightParen => 17; // ')' inside action
internal static itemType itemSpace => 18; // run of spaces separating arguments
internal static itemType itemString => 19; // quoted string (includes quotes)
internal static itemType itemText => 20; // plain text
internal static itemType itemVariable => 21; // variable starting with '$', such as '$' or  '$1' or '$hello'
internal static itemType itemKeyword => 22; // used only to delimit the keywords
internal static itemType itemBlock => 23; // block keyword
internal static itemType itemBreak => 24; // break keyword
internal static itemType itemContinue => 25; // continue keyword
internal static itemType itemDot => 26; // the cursor, spelled '.'
internal static itemType itemDefine => 27; // define keyword
internal static itemType itemElse => 28; // else keyword
internal static itemType itemEnd => 29; // end keyword
internal static itemType itemIf => 30; // if keyword
internal static itemType itemNil => 31; // the untyped nil constant, easiest to treat as a keyword
internal static itemType itemRange => 32; // range keyword
internal static itemType itemTemplate => 33; // template keyword
internal static itemType itemWith => 34; // with keyword

internal static map<@string, itemType> key = new map<@string, itemType>{
    ["."u8] = itemDot,
    ["block"u8] = itemBlock,
    ["break"u8] = itemBreak,
    ["continue"u8] = itemContinue,
    ["define"u8] = itemDefine,
    ["else"u8] = itemElse,
    ["end"u8] = itemEnd,
    ["if"u8] = itemIf,
    ["range"u8] = itemRange,
    ["nil"u8] = itemNil,
    ["template"u8] = itemTemplate,
    ["with"u8] = itemWith
};

internal static UntypedInt eof => -1;

// Trimming spaces.
// If the action begins "{{- " rather than "{{", then all space/tab/newlines
// preceding the action are trimmed; conversely if it ends " -}}" the
// leading spaces are trimmed. This is done entirely in the lexer; the
// parser never sees it happen. We require an ASCII space (' ', \t, \r, \n)
// to be present to avoid ambiguity with things like "{{-3}}". It reads
// better with the space present anyway. For simplicity, only ASCII
// does the job.
internal static readonly @string spaceChars = " \t\r\n"u8; // These are the space characters defined by Go itself.

internal static UntypedInt trimMarker => /* '-' */ 45; // Attached to left/right delimiter, trims trailing spaces from preceding/following text.

internal static Pos trimMarkerLen => /* Pos(1 + 1) */ 2; // marker plus space before or after

internal delegate stateFn stateFn(ж<lexer> _);

// lexer holds the state of the scanner.
[GoType] partial struct lexer {
    internal @string name; // the name of the input; used only for error reports
    internal @string input; // the string being scanned
    internal @string leftDelim; // start of action marker
    internal @string rightDelim; // end of action marker
    internal Pos pos;    // current position in the input
    internal Pos start;    // start position of this item
    internal bool atEOF;   // we have hit the end of input and returned eof
    internal nint parenDepth;   // nesting depth of ( ) exprs
    internal nint line;   // 1+number of newlines seen
    internal nint startLine;   // start line of this item
    internal item item;   // item to return to parser
    internal bool insideAction;   // are we inside an action?
    internal lexOptions options;
}

// lexOptions control behavior of the lexer. All default to false.
[GoType] partial struct lexOptions {
    internal bool emitComment; // emit itemComment tokens.
    internal bool breakOK; // break keyword allowed
    internal bool continueOK; // continue keyword allowed
}

// next returns the next rune in the input.
[GoRecv] internal static rune next(this ref lexer l) {
    if ((nint)l.pos >= len(l.input)) {
        l.atEOF = true;
        return eof;
    }
    var (r, w) = utf8.DecodeRuneInString(l.input.slice(l.pos));
    l.pos += ((Pos)w);
    if (r == (rune)'\n') {
        l.line++;
    }
    return r;
}

// peek returns but does not consume the next rune in the input.
[GoRecv] internal static rune peek(this ref lexer l) {
    var r = l.next();
    l.backup();
    return r;
}

// backup steps back one rune.
[GoRecv] internal static void backup(this ref lexer l) {
    if (!l.atEOF && l.pos > 0) {
        var (r, w) = utf8.DecodeLastRuneInString(l.input.slice(0, l.pos));
        l.pos -= ((Pos)w);
        // Correct newline count.
        if (r == (rune)'\n') {
            l.line--;
        }
    }
}

// thisItem returns the item at the current input point with the specified type
// and advances the input.
[GoRecv] internal static item thisItem(this ref lexer l, itemType t) {
    var i = new item(t, l.start, l.input.slice(l.start, l.pos), l.startLine);
    l.start = l.pos;
    l.startLine = l.line;
    return i;
}

// emit passes the trailing text as an item back to the parser.
[GoRecv] internal static stateFn emit(this ref lexer l, itemType t) {
    return l.emitItem(l.thisItem(t));
}

// emitItem passes the specified item to the parser.
[GoRecv] internal static stateFn emitItem(this ref lexer l, item i) {
    l.item = i;
    return default!;
}

// ignore skips over the pending input before this point.
// It tracks newlines in the ignored text, so use it only
// for text that is skipped without calling l.next.
[GoRecv] internal static void ignore(this ref lexer l) {
    l.line += strings.Count(l.input.slice(l.start, l.pos), "\n"u8);
    l.start = l.pos;
    l.startLine = l.line;
}

// accept consumes the next rune if it's from the valid set.
[GoRecv] internal static bool accept(this ref lexer l, @string valid) {
    if (strings.ContainsRune(valid, l.next())) {
        return true;
    }
    l.backup();
    return false;
}

// acceptRun consumes a run of runes from the valid set.
[GoRecv] internal static void acceptRun(this ref lexer l, @string valid) {
    while (strings.ContainsRune(valid, l.next())) {
    }
    l.backup();
}

// errorf returns an error token and terminates the scan by passing
// back a nil pointer that will be the next state, terminating l.nextItem.
[GoRecv] internal static stateFn errorf(this ref lexer l, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    l.item = new item(itemError, l.start, fmt.Sprintf(format, args.ꓸꓸꓸ), l.startLine);
    l.start = 0;
    l.pos = 0;
    l.input = l.input[..0];
    return default!;
}

// nextItem returns the next item from the input.
// Called by the parser, not in the lexing goroutine.
internal static item nextItem(this ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.item = new item(itemEOF, l.pos, "EOF"u8, l.startLine);
    stateFn state = lexText;
    if (l.insideAction) {
        state = lexInsideAction;
    }
    while (ᐧ) {
        state = state(Ꮡl);
        if (state == default!) {
            return l.item;
        }
    }
}

// lex creates a new scanner for the input string.
internal static ж<lexer> lex(@string name, @string input, @string left, @string right) {
    if (left == ""u8) {
        left = leftDelim;
    }
    if (right == ""u8) {
        right = rightDelim;
    }
    var l = Ꮡ(new lexer(
        name: name,
        input: input,
        leftDelim: left,
        rightDelim: right,
        line: 1,
        startLine: 1,
        insideAction: false
    ));
    return l;
}

// state functions
internal static readonly @string leftDelim = "{{"u8;
internal static readonly @string rightDelim = "}}"u8;
internal static readonly @string leftComment = "/*"u8;
internal static readonly @string rightComment = "*/"u8;

// lexText scans until an opening action delimiter, "{{".
internal static stateFn lexText(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    {
        nint x = strings.Index(l.input.slice(l.pos), l.leftDelim); if (x >= 0) {
            if (x > 0) {
                l.pos += ((Pos)x);
                // Do we trim any trailing space?
                Pos trimLength = ((Pos)0);
                Pos delimEnd = l.pos + ((Pos)len(l.leftDelim));
                if (hasLeftTrimMarker(l.input.slice(delimEnd))) {
                    trimLength = rightTrimLength(l.input.slice(l.start, l.pos));
                }
                l.pos -= trimLength;
                l.line += strings.Count(l.input.slice(l.start, l.pos), "\n"u8);
                var i = l.thisItem(itemText);
                l.pos += trimLength;
                l.ignore();
                if (len(i.val) > 0) {
                    return l.emitItem(i);
                }
            }
            return lexLeftDelim;
        }
    }
    l.pos = ((Pos)len(l.input));
    // Correctly reached EOF.
    if (l.pos > l.start) {
        l.line += strings.Count(l.input.slice(l.start, l.pos), "\n"u8);
        return l.emit(itemText);
    }
    return l.emit(itemEOF);
}

// rightTrimLength returns the length of the spaces at the end of the string.
internal static Pos rightTrimLength(@string s) {
    return ((Pos)(len(s) - len(strings.TrimRight(s, spaceChars))));
}

// atRightDelim reports whether the lexer is at a right delimiter, possibly preceded by a trim marker.
[GoRecv] internal static (bool delim, bool trimSpaces) atRightDelim(this ref lexer l) {
    if (hasRightTrimMarker(l.input.slice(l.pos)) && strings.HasPrefix(l.input.slice(l.pos + trimMarkerLen), l.rightDelim)) {
        // With trim marker.
        return (true, true);
    }
    if (strings.HasPrefix(l.input.slice(l.pos), l.rightDelim)) {
        // Without trim marker.
        return (true, false);
    }
    return (false, false);
}

// leftTrimLength returns the length of the spaces at the beginning of the string.
internal static Pos leftTrimLength(@string s) {
    return ((Pos)(len(s) - len(strings.TrimLeft(s, spaceChars))));
}

// lexLeftDelim scans the left delimiter, which is known to be present, possibly with a trim marker.
// (The text to be trimmed has already been emitted.)
internal static stateFn lexLeftDelim(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.pos += ((Pos)len(l.leftDelim));
    var trimSpace = hasLeftTrimMarker(l.input.slice(l.pos));
    Pos afterMarker = ((Pos)0);
    if (trimSpace) {
        afterMarker = trimMarkerLen;
    }
    if (strings.HasPrefix(l.input.slice(l.pos + afterMarker), leftComment)) {
        l.pos += afterMarker;
        l.ignore();
        return lexComment;
    }
    var i = l.thisItem(itemLeftDelim);
    l.insideAction = true;
    l.pos += afterMarker;
    l.ignore();
    l.parenDepth = 0;
    return l.emitItem(i);
}

// lexComment scans a comment. The left comment marker is known to be present.
internal static stateFn lexComment(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    l.pos += ((Pos)len(leftComment));
    nint x = strings.Index(l.input.slice(l.pos), rightComment);
    if (x < 0) {
        return l.errorf("unclosed comment"u8);
    }
    l.pos += ((Pos)(x + len(rightComment)));
    var (delim, trimSpace) = l.atRightDelim();
    if (!delim) {
        return l.errorf("comment ends before closing delimiter"u8);
    }
    l.line += strings.Count(l.input.slice(l.start, l.pos), "\n"u8);
    var i = l.thisItem(itemComment);
    if (trimSpace) {
        l.pos += trimMarkerLen;
    }
    l.pos += ((Pos)len(l.rightDelim));
    if (trimSpace) {
        l.pos += leftTrimLength(l.input.slice(l.pos));
    }
    l.ignore();
    if (l.options.emitComment) {
        return l.emitItem(i);
    }
    return lexText;
}

// lexRightDelim scans the right delimiter, which is known to be present, possibly with a trim marker.
internal static stateFn lexRightDelim(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    var (_, trimSpace) = l.atRightDelim();
    if (trimSpace) {
        l.pos += trimMarkerLen;
        l.ignore();
    }
    l.pos += ((Pos)len(l.rightDelim));
    var i = l.thisItem(itemRightDelim);
    if (trimSpace) {
        l.pos += leftTrimLength(l.input.slice(l.pos));
        l.ignore();
    }
    l.insideAction = false;
    return l.emitItem(i);
}

// lexInsideAction scans the elements inside action delimiters.
internal static stateFn lexInsideAction(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    // Either number, quoted string, or identifier.
    // Spaces separate arguments; runs of spaces turn into itemSpace.
    // Pipe symbols separate and are emitted.
    var (delim, _) = l.atRightDelim();
    if (delim) {
        if (l.parenDepth == 0) {
            return lexRightDelim;
        }
        return l.errorf("unclosed left paren"u8);
    }
    {
        var r = l.next();
        var matchᴛ1 = false;
        if (r == eof) { matchᴛ1 = true;
            return l.errorf("unclosed action"u8);
        }
        if (isSpace(r)) { matchᴛ1 = true;
            l.backup(); // Put space back in case we have " -}}".
            return lexSpace;
        }
        if (r is (rune)'=') { matchᴛ1 = true;
            return l.emit(itemAssign);
        }
        if (r is (rune)':') { matchᴛ1 = true;
            if (l.next() != (rune)'=') {
                return l.errorf("expected :="u8);
            }
            return l.emit(itemDeclare);
        }
        if (r is (rune)'|') { matchᴛ1 = true;
            return l.emit(itemPipe);
        }
        if (r is (rune)'"') { matchᴛ1 = true;
            return lexQuote;
        }
        if (r is (rune)'`') { matchᴛ1 = true;
            return lexRawQuote;
        }
        if (r is (rune)'$') { matchᴛ1 = true;
            return lexVariable;
        }
        if (r is (rune)'\'') { matchᴛ1 = true;
            return lexChar;
        }
        if (r is (rune)'.') { matchᴛ1 = true;
            if (l.pos < ((Pos)len(l.input))) {
                // special look-ahead for ".field" so we don't break l.backup().
                var rΔ2 = l.input[l.pos];
                if (rΔ2 < (rune)'0' || (rune)'9' < rΔ2) {
                    return lexField;
                }
            } // '.' can start a number.
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (r == (rune)'+' || r == (rune)'-' || ((rune)'0' <= r && r <= (rune)'9'))) { matchᴛ1 = true;
            l.backup();
            return lexNumber;
        }
        if (isAlphaNumeric(r)) { matchᴛ1 = true;
            l.backup();
            return lexIdentifier;
        }
        if (r is (rune)'(') { matchᴛ1 = true;
            l.parenDepth++;
            return l.emit(itemLeftParen);
        }
        if (r is (rune)')') { matchᴛ1 = true;
            l.parenDepth--;
            if (l.parenDepth < 0) {
                return l.errorf("unexpected right paren"u8);
            }
            return l.emit(itemRightParen);
        }
        if (r <= unicode.MaxASCII && unicode.IsPrint(r)) {
            return l.emit(itemChar);
        }
        if (!matchᴛ1) { /* default: */
            return l.errorf("unrecognized character in action: %#U"u8, r);
        }
        return default!;
    }

}

// lexSpace scans a run of space characters.
// We have not consumed the first space, which is known to be present.
// Take care if there is a trim-marked right delimiter, which starts with a space.
internal static stateFn lexSpace(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    rune r = default!;
    nint numSpaces = default!;
    while (ᐧ) {
        r = l.peek();
        if (!isSpace(r)) {
            break;
        }
        l.next();
        numSpaces++;
    }
    // Be careful about a trim-marked closing delimiter, which has a minus
    // after a space. We know there is a space, so check for the '-' that might follow.
    if (hasRightTrimMarker(l.input.slice(l.pos - 1)) && strings.HasPrefix(l.input.slice(l.pos - 1 + trimMarkerLen), l.rightDelim)) {
        l.backup(); // Before the space.
        if (numSpaces == 1) {
            return lexRightDelim; // On the delim, so go right to that.
        }
    }
    return l.emit(itemSpace);
}

// lexIdentifier scans an alphanumeric.
internal static stateFn lexIdentifier(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    while (ᐧ) {
        {
            var r = l.next();
            switch (ᐧ) {
            case {} when isAlphaNumeric(r): {
                break;
            }
            default: {
                l.backup();
                @string word = l.input.slice(l.start, l.pos);
                if (!l.atTerminator()) {
                    // absorb.
                    return l.errorf("bad character %#U"u8, r);
                }
                switch (ᐧ) {
                case {} when key[word] > itemKeyword: {
                    itemType item = key[word];
                    if (item == itemBreak && !l.options.breakOK || item == itemContinue && !l.options.continueOK) {
                        return l.emit(itemIdentifier);
                    }
                    return l.emit(item);
                }
                case {} when word[0] is (rune)'.': {
                    return l.emit(itemField);
                }
                case {} when (word == "true"u8) || (word == "false"u8): {
                    return l.emit(itemBool);
                }
                default: {
                    return l.emit(itemIdentifier);
                }}

                break;
            }}
        }

    }
}

// lexField scans a field: .Alphanumeric.
// The . has been scanned.
internal static stateFn lexField(ж<lexer> Ꮡl) {
    return lexFieldOrVariable(Ꮡl, itemField);
}

// lexVariable scans a Variable: $Alphanumeric.
// The $ has been scanned.
internal static stateFn lexVariable(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (l.atTerminator()) {
        // Nothing interesting follows -> "$".
        return l.emit(itemVariable);
    }
    return lexFieldOrVariable(Ꮡl, itemVariable);
}

// lexFieldOrVariable scans a field or variable: [.$]Alphanumeric.
// The . or $ has been scanned.
internal static stateFn lexFieldOrVariable(ж<lexer> Ꮡl, itemType typ) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (l.atTerminator()) {
        // Nothing interesting follows -> "." or "$".
        if (typ == itemVariable) {
            return l.emit(itemVariable);
        }
        return l.emit(itemDot);
    }
    rune r = default!;
    while (ᐧ) {
        r = l.next();
        if (!isAlphaNumeric(r)) {
            l.backup();
            break;
        }
    }
    if (!l.atTerminator()) {
        return l.errorf("bad character %#U"u8, r);
    }
    return l.emit(typ);
}

// atTerminator reports whether the input is at valid termination character to
// appear after an identifier. Breaks .X.Y into two pieces. Also catches cases
// like "$x+2" not being acceptable without a space, in case we decide one
// day to implement arithmetic.
[GoRecv] internal static bool atTerminator(this ref lexer l) {
    var r = l.peek();
    if (isSpace(r)) {
        return true;
    }
    var exprᴛ1 = r;
    if (exprᴛ1 == eof || exprᴛ1 == (rune)'.' || exprᴛ1 == (rune)',' || exprᴛ1 == (rune)'|' || exprᴛ1 == (rune)':' || exprᴛ1 == (rune)')' || exprᴛ1 == (rune)'(') {
        return true;
    }

    return strings.HasPrefix(l.input.slice(l.pos), l.rightDelim);
}

// lexChar scans a character constant. The initial quote is already
// scanned. Syntax checking is done by the parser.
internal static stateFn lexChar(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

Loop:
    while (ᐧ) {
        var exprᴛ1 = l.next();
        var matchᴛ1 = false;
        if (exprᴛ1 is (rune)'\\') { matchᴛ1 = true;
            do {
                {
                    var r = l.next(); if (r != eof && r != (rune)'\n') {
                        break;
                    }
                }
                fallthrough = true;
            } while (false);
        }
        if (fallthrough || !matchᴛ1 && (exprᴛ1 == eof || exprᴛ1 == (rune)'\n')) {
            return l.errorf("unterminated character constant"u8);
        }
        else if (exprᴛ1 is (rune)'\'') { matchᴛ1 = true;
            goto break_Loop;
        }

continue_Loop:;
    }
break_Loop:;
    return l.emit(itemCharConstant);
}

// lexNumber scans a number: decimal, octal, hex, float, or imaginary. This
// isn't a perfect number scanner - for instance it accepts "." and "0x0.2"
// and "089" - but when it's wrong the input is invalid and the parser (via
// strconv) will notice.
internal static stateFn lexNumber(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (!l.scanNumber()) {
        return l.errorf("bad number syntax: %q"u8, l.input.slice(l.start, l.pos));
    }
    {
        var sign = l.peek(); if (sign == (rune)'+' || sign == (rune)'-') {
            // Complex: 1+2i. No spaces, must end in 'i'.
            if (!l.scanNumber() || l.input[l.pos - 1] != (rune)'i') {
                return l.errorf("bad number syntax: %q"u8, l.input.slice(l.start, l.pos));
            }
            return l.emit(itemComplex);
        }
    }
    return l.emit(itemNumber);
}

[GoRecv] internal static bool scanNumber(this ref lexer l) {
    // Optional leading sign.
    l.accept("+-"u8);
    // Is it hex?
    @string digits = "0123456789_"u8;
    if (l.accept("0"u8)) {
        // Note: Leading 0 does not mean octal in floats.
        if (l.accept("xX"u8)){
            digits = "0123456789abcdefABCDEF_"u8;
        } else 
        if (l.accept("oO"u8)){
            digits = "01234567_"u8;
        } else 
        if (l.accept("bB"u8)) {
            digits = "01_"u8;
        }
    }
    l.acceptRun(digits);
    if (l.accept("."u8)) {
        l.acceptRun(digits);
    }
    if (len(digits) == 10 + 1 && l.accept("eE"u8)) {
        l.accept("+-"u8);
        l.acceptRun("0123456789_"u8);
    }
    if (len(digits) == 16 + 6 + 1 && l.accept("pP"u8)) {
        l.accept("+-"u8);
        l.acceptRun("0123456789_"u8);
    }
    // Is it imaginary?
    l.accept("i"u8);
    // Next thing mustn't be alphanumeric.
    if (isAlphaNumeric(l.peek())) {
        l.next();
        return false;
    }
    return true;
}

// lexQuote scans a quoted string.
internal static stateFn lexQuote(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

Loop:
    while (ᐧ) {
        var exprᴛ1 = l.next();
        var matchᴛ1 = false;
        if (exprᴛ1 is (rune)'\\') { matchᴛ1 = true;
            do {
                {
                    var r = l.next(); if (r != eof && r != (rune)'\n') {
                        break;
                    }
                }
                fallthrough = true;
            } while (false);
        }
        if (fallthrough || !matchᴛ1 && (exprᴛ1 == eof || exprᴛ1 == (rune)'\n')) {
            return l.errorf("unterminated quoted string"u8);
        }
        else if (exprᴛ1 is (rune)'"') { matchᴛ1 = true;
            goto break_Loop;
        }

continue_Loop:;
    }
break_Loop:;
    return l.emit(itemString);
}

// lexRawQuote scans a raw quoted string.
internal static stateFn lexRawQuote(ж<lexer> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

Loop:
    while (ᐧ) {
        var exprᴛ1 = l.next();
        if (exprᴛ1 == eof) {
            return l.errorf("unterminated raw quoted string"u8);
        }
        if (exprᴛ1 is (rune)'`') {
            goto break_Loop;
        }

continue_Loop:;
    }
break_Loop:;
    return l.emit(itemRawString);
}

// isSpace reports whether r is a space character.
internal static bool isSpace(rune r) {
    return r == (rune)' ' || r == (rune)'\t' || r == (rune)'\r' || r == (rune)'\n';
}

// isAlphaNumeric reports whether r is an alphabetic, digit, or underscore.
internal static bool isAlphaNumeric(rune r) {
    return r == (rune)'_' || unicode.IsLetter(r) || unicode.IsDigit(r);
}

internal static bool hasLeftTrimMarker(@string s) {
    return len(s) >= 2 && s[0] == trimMarker && isSpace((rune)s[1]);
}

internal static bool hasRightTrimMarker(@string s) {
    return len(s) >= 2 && isSpace((rune)s[0]) && s[1] == trimMarker;
}

} // end parse_package
