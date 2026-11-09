namespace go.github.com.itchyny;

using json = encoding.json_package;
using utf8 = go.unicode.utf8_package;
using encoding;
using go.unicode;

partial class gojq_package {

partial struct lexer {
    internal @string source;
    internal nint offset;
    internal ж<Query> result;
    internal @string token;
    internal nint tokenType;
    internal bool inString;
    internal error err;
}

internal static ж<lexer> newLexer(@string src) {
    return Ꮡ(new lexer(source: src));
}

internal static UntypedInt eof => -1;

[GoLazy] partial class lazyᴛkeywords { internal static map<@string, nint> keywords = new map<@string, nint>{
    ["or"] = tokOrOp,
    ["and"] = tokAndOp,
    ["module"] = tokModule,
    ["import"] = tokImport,
    ["include"] = tokInclude,
    ["def"] = tokDef,
    ["as"] = tokAs,
    ["label"] = tokLabel,
    ["break"] = tokBreak,
    ["null"] = tokNull,
    ["true"] = tokTrue,
    ["false"] = tokFalse,
    ["if"] = tokIf,
    ["then"] = tokThen,
    ["elif"] = tokElif,
    ["else"] = tokElse,
    ["end"] = tokEnd,
    ["try"] = tokTry,
    ["catch"] = tokCatch,
    ["reduce"] = tokReduce,
    ["foreach"] = tokForeach
}; }

internal static nint /*tokenType*/ Lex(this ж<lexer> Ꮡl, ж<yySymType> Ꮡlval) {
    nint tokenType = default;
    GoFrame ᒐ = default;
    try {
        ref var l = ref Ꮡl.DerefOrNull();
        ref var lval = ref Ꮡlval.DerefOrNull();

        defer(() => {
            Ꮡl.Value.tokenType = tokenType;
        }, ref ᒐ);
        if (len(l.source) == l.offset) {
            l.token = "";
            tokenType = eof; goto ᒐdone;
        }
        if (l.inString) {
            var (tok, str) = l.scanString(l.offset);
            lval.token = str;
            tokenType = tok; goto ᒐdone;
        }
        var (ch, iseof) = l.next();
        if (iseof) {
            l.token = "";
            tokenType = eof; goto ᒐdone;
        }
        switch (ᐧ) {
        case {} when isIdent(ch, false): {
            nint i = l.offset - 1;
            var (j, isModule) = l.scanIdentOrModule();
            l.token = l.source.slice(i, j);
            lval.token = l.token;
            if (isModule) {
                tokenType = tokModuleIdent; goto ᒐdone;
            }
            var (tok, ok) = keywords[l.token, ꟷ]; if (ok) {
                tokenType = tok; goto ᒐdone;
            }
            tokenType = tokIdent; goto ᒐdone;
        }
        case {} when isNumber(ch): {
            nint i = l.offset - 1;
            nint j = l.scanNumber(numberStateLead);
            if (j < 0) {
                l.token = l.source.slice(i, -j);
                tokenType = tokInvalid; goto ᒐdone;
            }
            l.token = l.source.slice(i, j);
            lval.token = l.token;
            tokenType = tokNumber; goto ᒐdone;
        }}

        switch (ch) {
        case (byte)'.': {
            var chΔ2 = l.peek();
            switch (ᐧ) {
            case {} when chΔ2 is (byte)'.': {
                l.offset++;
                l.token = "..";
                tokenType = tokRecurse; goto ᒐdone;
            }
            case {} when isIdent(chΔ2, false): {
                l.token = l.source.slice(l.offset - 1, l.scanIdent());
                lval.token = l.token[1..];
                tokenType = tokIndex; goto ᒐdone;
            }
            case {} when isNumber(chΔ2): {
                nint i = l.offset - 1;
                nint j = l.scanNumber(numberStateFloat);
                if (j < 0) {
                    l.token = l.source.slice(i, -j);
                    tokenType = tokInvalid; goto ᒐdone;
                }
                l.token = l.source.slice(i, j);
                lval.token = l.token;
                tokenType = tokNumber; goto ᒐdone;
            }
            default: {
                tokenType = '.'; goto ᒐdone;
            }}

            break;
        }
        case (byte)'$': {
            if (isIdent(l.peek(), false)) {
                nint i = l.offset - 1;
                var (j, isModule) = l.scanIdentOrModule();
                l.token = l.source.slice(i, j);
                lval.token = l.token;
                if (isModule) {
                    tokenType = tokModuleVariable; goto ᒐdone;
                }
                tokenType = tokVariable; goto ᒐdone;
            }
            break;
        }
        case (byte)'|': {
            if (l.peek() == '=') {
                l.offset++;
                l.token = "|=";
                lval.@operator = OpModify;
                tokenType = tokUpdateOp; goto ᒐdone;
            }
            break;
        }
        case (byte)'?': {
            if (l.peek() == '/') {
                l.offset++;
                if (l.peek() == '/') {
                    l.offset++;
                    l.token = "?//";
                    tokenType = tokDestAltOp; goto ᒐdone;
                }
                l.offset--;
            }
            break;
        }
        case (byte)'+': {
            if (l.peek() == '=') {
                l.offset++;
                l.token = "+=";
                lval.@operator = OpUpdateAdd;
                tokenType = tokUpdateOp; goto ᒐdone;
            }
            break;
        }
        case (byte)'-': {
            if (l.peek() == '=') {
                l.offset++;
                l.token = "-=";
                lval.@operator = OpUpdateSub;
                tokenType = tokUpdateOp; goto ᒐdone;
            }
            break;
        }
        case (byte)'*': {
            if (l.peek() == '=') {
                l.offset++;
                l.token = "*=";
                lval.@operator = OpUpdateMul;
                tokenType = tokUpdateOp; goto ᒐdone;
            }
            break;
        }
        case (byte)'/': {
            switch (l.peek()) {
            case (byte)'=': {
                l.offset++;
                l.token = "/=";
                lval.@operator = OpUpdateDiv;
                tokenType = tokUpdateOp; goto ᒐdone;
            }
            case (byte)'/': {
                l.offset++;
                if (l.peek() == '=') {
                    l.offset++;
                    l.token = "//=";
                    lval.@operator = OpUpdateAlt;
                    tokenType = tokUpdateOp; goto ᒐdone;
                }
                l.token = "//";
                lval.@operator = OpAlt;
                tokenType = tokAltOp; goto ᒐdone;
            }}

            break;
        }
        case (byte)'%': {
            if (l.peek() == '=') {
                l.offset++;
                l.token = "%=";
                lval.@operator = OpUpdateMod;
                tokenType = tokUpdateOp; goto ᒐdone;
            }
            break;
        }
        case (byte)'=': {
            if (l.peek() == '=') {
                l.offset++;
                l.token = "==";
                lval.@operator = OpEq;
                tokenType = tokCompareOp; goto ᒐdone;
            }
            l.token = "=";
            lval.@operator = OpAssign;
            tokenType = tokUpdateOp; goto ᒐdone;
        }
        case (byte)'!': {
            if (l.peek() == '=') {
                l.offset++;
                l.token = "!=";
                lval.@operator = OpNe;
                tokenType = tokCompareOp; goto ᒐdone;
            }
            break;
        }
        case (byte)'>': {
            if (l.peek() == '=') {
                l.offset++;
                l.token = ">=";
                lval.@operator = OpGe;
                tokenType = tokCompareOp; goto ᒐdone;
            }
            l.token = ">";
            lval.@operator = OpGt;
            tokenType = tokCompareOp; goto ᒐdone;
        }
        case (byte)'<': {
            if (l.peek() == '=') {
                l.offset++;
                l.token = "<=";
                lval.@operator = OpLe;
                tokenType = tokCompareOp; goto ᒐdone;
            }
            l.token = "<";
            lval.@operator = OpLt;
            tokenType = tokCompareOp; goto ᒐdone;
        }
        case (byte)'@': {
            if (isIdent(l.peek(), true)) {
                l.token = l.source.slice(l.offset - 1, l.scanIdent());
                lval.token = l.token;
                tokenType = tokFormat; goto ᒐdone;
            }
            break;
        }
        case (byte)'"': {
            var (tok, str) = l.scanString(l.offset - 1);
            lval.token = str;
            tokenType = tok; goto ᒐdone;
        }
        default: {
            if (ch >= utf8.RuneSelf) {
                var (r, size) = utf8.DecodeRuneInString(l.source.slice(l.offset - 1));
                l.offset += size - 1;
                l.token = (@string)r;
            }
            break;
        }}

        tokenType = (nint)ch;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return tokenType;
}

internal static (byte, bool) next(this ref lexer l) {
    for (;;) {
        var ch = l.source[l.offset];
        l.offset++;
        if (ch == '#') {
            if (l.skipComment()) {
                return (0, true);
            }
        } else if (!isWhite(ch)) {
            return (ch, false);
        } else if (len(l.source) == l.offset) {
            return (0, true);
        }
    }
}

internal static bool skipComment(this ref lexer l) {
    for (;;) {
        switch (l.peek()) {
        case 0: {
            return true;
        }
        case (byte)'\\': {
            l.offset++;
            switch (l.peek()) {
            case (byte)'\\' or (byte)'\n': {
                l.offset++;
                break;
            }
            case (byte)'\r': {
                l.offset++; if (l.peek() == '\n') {
                    l.offset++;
                }
                break;
            }}

            break;
        }
        case (byte)'\n' or (byte)'\r': {
            return false;
        }
        default: {
            l.offset++;
            break;
        }}

    }
}

internal static byte peek(this ref lexer l) {
    if (len(l.source) == l.offset) {
        return 0;
    }
    return l.source[l.offset];
}

internal static nint scanIdent(this ref lexer l) {
    while (isIdent(l.peek(), true)) {
        l.offset++;
    }
    return l.offset;
}

internal static (nint, bool) scanIdentOrModule(this ref lexer l) {
    nint index = l.scanIdent();
    bool isModule = default;
    if (l.peek() == ':') {
        l.offset++;
        if (l.peek() == ':') {
            l.offset++;
            if (isIdent(l.peek(), false)) {
                l.offset++;
                index = l.scanIdent();
                isModule = true;
            } else {
                l.offset -= 2;
            }
        } else {
            l.offset--;
        }
    }
    return (index, isModule);
}

internal static bool validVarName(this ref lexer l) {
    if (l.peek() != '$') {
        return false;
    }
    l.offset++;
    return isIdent(l.peek(), false) && l.scanIdent() == len(l.source);
}

internal static UntypedInt numberStateLead => iota;
internal static UntypedInt numberStateFloat => 1;
internal static UntypedInt numberStateExpLead => 2;
internal static UntypedInt numberStateExp => 3;

internal static nint scanNumber(this ref lexer l, nint state) {
    for (;;) {
        switch (state) {
        case var ᴛ when ᴛ == numberStateLead || ᴛ == numberStateFloat: {
            {
                var ch = l.peek(); if (isNumber(ch)) {
                    l.offset++;
                } else {
                    switch (ch) {
                    case (byte)'.': {
                        if (state != numberStateLead) {
                            l.offset++;
                            return -l.offset;
                        }
                        l.offset++;
                        state = numberStateFloat;
                        break;
                    }
                    case (byte)'e' or (byte)'E': {
                        l.offset++;
                        switch (l.peek()) {
                        case (byte)'-' or (byte)'+': {
                            l.offset++;
                            break;
                        }}

                        state = numberStateExpLead;
                        break;
                    }
                    default: {
                        if (isIdent(ch, false)) {
                            l.offset++;
                            return -l.offset;
                        }
                        return l.offset;
                    }}

                }
            }
            break;
        }
        case var ᴛ when ᴛ == numberStateExpLead || ᴛ == numberStateExp: {
            {
                var ch = l.peek(); if (!isNumber(ch)) {
                    if (isIdent(ch, false)) {
                        l.offset++;
                        return -l.offset;
                    }
                    if (state == numberStateExpLead) {
                        return -l.offset;
                    }
                    return l.offset;
                }
            }
            l.offset++;
            state = numberStateExp;
            break;
        }
        default: {
            throw panic(state);
            break;
        }}

    }
}

internal static bool validNumber(this ref lexer l) {
    var ch = l.peek();
    switch (ch) {
    case (byte)'+' or (byte)'-': {
        l.offset++;
        ch = l.peek();
        break;
    }}

    nint state = numberStateLead;
    if (ch == '.') {
        l.offset++;
        ch = l.peek();
        state = numberStateFloat;
    }
    return isNumber(ch) && l.scanNumber(state) == len(l.source);
}

internal static (nint, @string) scanString(this ref lexer l, nint start) {
    bool decode = default;
    nint controls = default;
    (@string, error) unquote(@string srcʗp, bool quote) {
        ref var src = ref heap(srcʗp, out var Ꮡsrc);
        if (!decode) {
            if (quote) {
                return (src, default);
            }
            return (src.slice(1, len(src) - 1), default);
        }
        slice<byte> buf = default;
        if (!quote && controls == 0) {
            buf = slice<byte>(src);
        } else {
            buf = quoteAndEscape(src, quote, controls);
        }
        var err = json.Unmarshal(buf, Ꮡsrc); if (err != nil) {
            return ("", err);
        }
        return (src, default);
    }
    for (nint i = l.offset; i < len(l.source); i++) {
        var ch = l.source[i];
        switch (ch) {
        case (byte)'\\': {
            i++; if (i >= len(l.source)) {
                break;
            }
            var exprᴛ1 = l.source[i];
            var matchᴛ1 = false;
            if (exprᴛ1 is (byte)'u') { matchᴛ1 = true;
                for (nint j = 1; j <= 4; j++) {
                    if (i + j >= len(l.source) || !isHex(l.source[i + j])) {
                        l.offset = i + j;
                        l.token = l.source.slice(i - 1, l.offset);
                        return (tokInvalidEscapeSequence, "");
                    }
                }
                i += 4;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && (exprᴛ1 is (byte)'"' or (byte)'/' or (byte)'\\' or (byte)'b' or (byte)'f' or (byte)'n' or (byte)'r' or (byte)'t')) { matchᴛ1 = true;
                decode = true;
            }
            else if (exprᴛ1 is (byte)'(') {
                if (!l.inString) {
                    l.inString = true;
                    return (tokStringStart, "");
                }
                if (i == l.offset + 1) {
                    l.offset += 2;
                    l.inString = false;
                    return (tokStringQuery, "");
                }
                l.offset = i - 1;
                l.token = l.source.slice(start, l.offset);
                var (str, err) = unquote(l.token, true);
                if (err != nil) {
                    return (tokInvalid, "");
                }
                return (tokString, str);
            }
            else if (!matchᴛ1) { /* default: */
                l.offset = i + 1;
                l.token = l.source.slice(l.offset - 2, l.offset);
                return (tokInvalidEscapeSequence, "");
            }

            break;
        }
        case (byte)'"': {
            if (!l.inString) {
                l.offset = i + 1;
                l.token = l.source.slice(start, l.offset);
                var (str, err) = unquote(l.token, false);
                if (err != nil) {
                    return (tokInvalid, "");
                }
                return (tokString, str);
            }
            if (i > l.offset) {
                l.offset = i;
                l.token = l.source.slice(start, l.offset);
                var (str, err) = unquote(l.token, true);
                if (err != nil) {
                    return (tokInvalid, "");
                }
                return (tokString, str);
            }
            l.inString = false;
            l.offset = i + 1;
            return (tokStringEnd, "");
        }
        default: {
            if (!decode) {
                decode = ch > '~';
            }
            if (ch < ' ') {
                controls++;
            }
            break;
        }}

    }
    l.offset = len(l.source);
    l.token = "";
    return (tokUnterminatedString, "");
}

internal static slice<byte> quoteAndEscape(@string src, bool quote, nint controls) {
    nint size = len(src) + controls * 5;
    if (quote) {
        size += 2;
    }
    var buf = new slice<byte>(size);
    nint j = default;
    if (quote) {
        buf[0] = (byte)'"';
        buf[len(buf) - 1] = (byte)'"';
        j++;
    }
    foreach (var i in range(len(src))) {
        var ch = src[i]; if (ch < ' ') {
            @string hex = "0123456789abcdef";
            copy(buf.slice(j), @"\u00"u8);
            buf[j + 4] = hex[(ch >> 4)];
            buf[j + 5] = hex[(byte)(ch & 0xF)];
            j += 6;
        } else {
            buf[j] = ch;
            j++;
        }
    }
    return buf;
}

partial struct ParseError {
    public nint Offset;
    public @string Token;
    internal nint tokenType;
}

public static @string Error(this ref ParseError err) {
    switch (err.tokenType) {
    case var ᴛ when ᴛ == eof: {
        return "unexpected EOF";
    }
    case var ᴛ when ᴛ == tokInvalid: {
        return "invalid token "u8 + jsonMarshal(err.Token);
    }
    case var ᴛ when ᴛ == tokInvalidEscapeSequence: {
        return @"invalid escape sequence """u8 + err.Token + @""" in string literal"u8;
    }
    case var ᴛ when ᴛ == tokUnterminatedString: {
        return "unterminated string literal";
    }
    default: {
        return "unexpected token "u8 + jsonMarshal(err.Token);
    }}

}

internal static void Error(this ref lexer l, @string _) {
    ref var offset = ref heap<nint>(out var Ꮡoffset);
    offset = l.offset;
    ref var token = ref heap<@string>(out var Ꮡtoken);
    token = l.token;
    if (l.tokenType != eof && l.tokenType < utf8.RuneSelf) {
        token = (@string)(rune)l.tokenType;
    }
    l.err = new ParseErrorжerror(Ꮡ(new ParseError(offset, token, l.tokenType)));
}

internal static bool isWhite(byte ch) {
    switch (ch) {
    case (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)' ': {
        return true;
    }
    default: {
        return false;
    }}

}

internal static bool isIdent(byte ch, bool tail) {
    return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_' || tail && isNumber(ch);
}

internal static bool isHex(byte ch) {
    return 'a' <= ch && ch <= 'f' || 'A' <= ch && ch <= 'F' || isNumber(ch);
}

internal static bool isNumber(byte ch) {
    return '0' <= ch && ch <= '9';
}

} // end gojq_package
