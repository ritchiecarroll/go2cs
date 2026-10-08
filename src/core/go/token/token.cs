// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package token defines constants representing the lexical tokens of the Go
// programming language and basic operations on tokens (printing, predicates).
namespace go.go;

using strconv = strconv_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using global::go.unicode;

partial class token_package {

partial struct Token /*num:nint*/;

// The list of tokens.
public static Token ILLEGAL => /* iota */ 0;

public static Token EOF => 1;

public static Token COMMENT => 2;

internal static Token literal_beg => 3;

public static Token IDENT => 4; // main

public static Token INT => 5; // 12345

public static Token FLOAT => 6; // 123.45

public static Token IMAG => 7; // 123.45i

public static Token CHAR => 8; // 'a'

public static Token STRING => 9; // "abc"

internal static Token literal_end => 10;

internal static Token operator_beg => 11;

public static Token ADD => 12; // +

public static Token SUB => 13; // -

public static Token MUL => 14; // *

public static Token QUO => 15; // /

public static Token REM => 16; // %

public static Token AND => 17; // &

public static Token OR => 18; // |

public static Token XOR => 19; // ^

public static Token SHL => 20; // <<

public static Token SHR => 21; // >>

public static Token AND_NOT => 22; // &^

public static Token ADD_ASSIGN => 23; // +=

public static Token SUB_ASSIGN => 24; // -=

public static Token MUL_ASSIGN => 25; // *=

public static Token QUO_ASSIGN => 26; // /=

public static Token REM_ASSIGN => 27; // %=

public static Token AND_ASSIGN => 28; // &=

public static Token OR_ASSIGN => 29; // |=

public static Token XOR_ASSIGN => 30; // ^=

public static Token SHL_ASSIGN => 31; // <<=

public static Token SHR_ASSIGN => 32; // >>=

public static Token AND_NOT_ASSIGN => 33; // &^=

public static Token LAND => 34; // &&

public static Token LOR => 35; // ||

public static Token ARROW => 36; // <-

public static Token INC => 37; // ++

public static Token DEC => 38; // --

public static Token EQL => 39; // ==

public static Token LSS => 40; // <

public static Token GTR => 41; // >

public static Token ASSIGN => 42; // =

public static Token NOT => 43; // !

public static Token NEQ => 44; // !=

public static Token LEQ => 45; // <=

public static Token GEQ => 46; // >=

public static Token DEFINE => 47; // :=

public static Token ELLIPSIS => 48; // ...

public static Token LPAREN => 49; // (

public static Token LBRACK => 50; // [

public static Token LBRACE => 51; // {

public static Token COMMA => 52; // ,

public static Token PERIOD => 53; // .

public static Token RPAREN => 54; // )

public static Token RBRACK => 55; // ]

public static Token RBRACE => 56; // }

public static Token SEMICOLON => 57; // ;

public static Token COLON => 58; // :

internal static Token operator_end => 59;

internal static Token keyword_beg => 60;

public static Token BREAK => 61;

public static Token CASE => 62;

public static Token CHAN => 63;

public static Token CONST => 64;

public static Token CONTINUE => 65;

public static Token DEFAULT => 66;

public static Token DEFER => 67;

public static Token ELSE => 68;

public static Token FALLTHROUGH => 69;

public static Token FOR => 70;

public static Token FUNC => 71;

public static Token GO => 72;

public static Token GOTO => 73;

public static Token IF => 74;

public static Token IMPORT => 75;

public static Token INTERFACE => 76;

public static Token MAP => 77;

public static Token PACKAGE => 78;

public static Token RANGE => 79;

public static Token RETURN => 80;

public static Token SELECT => 81;

public static Token STRUCT => 82;

public static Token SWITCH => 83;

public static Token TYPE => 84;

public static Token VAR => 85;

internal static Token keyword_end => 86;

internal static Token additional_beg => 87;

public static Token TILDE => 88;

internal static Token additional_end => 89;

internal static array<@string> tokens = new golib.SparseArray<@string>{
    [(int)ILLEGAL] = "ILLEGAL"u8,
    [(int)EOF] = "EOF"u8,
    [(int)COMMENT] = "COMMENT"u8,
    [(int)IDENT] = "IDENT"u8,
    [(int)INT] = "INT"u8,
    [(int)FLOAT] = "FLOAT"u8,
    [(int)IMAG] = "IMAG"u8,
    [(int)CHAR] = "CHAR"u8,
    [(int)STRING] = "STRING"u8,
    [(int)ADD] = "+"u8,
    [(int)SUB] = "-"u8,
    [(int)MUL] = "*"u8,
    [(int)QUO] = "/"u8,
    [(int)REM] = "%"u8,
    [(int)AND] = "&"u8,
    [(int)OR] = "|"u8,
    [(int)XOR] = "^"u8,
    [(int)SHL] = "<<"u8,
    [(int)SHR] = ">>"u8,
    [(int)AND_NOT] = "&^"u8,
    [(int)ADD_ASSIGN] = "+="u8,
    [(int)SUB_ASSIGN] = "-="u8,
    [(int)MUL_ASSIGN] = "*="u8,
    [(int)QUO_ASSIGN] = "/="u8,
    [(int)REM_ASSIGN] = "%="u8,
    [(int)AND_ASSIGN] = "&="u8,
    [(int)OR_ASSIGN] = "|="u8,
    [(int)XOR_ASSIGN] = "^="u8,
    [(int)SHL_ASSIGN] = "<<="u8,
    [(int)SHR_ASSIGN] = ">>="u8,
    [(int)AND_NOT_ASSIGN] = "&^="u8,
    [(int)LAND] = "&&"u8,
    [(int)LOR] = "||"u8,
    [(int)ARROW] = "<-"u8,
    [(int)INC] = "++"u8,
    [(int)DEC] = "--"u8,
    [(int)EQL] = "=="u8,
    [(int)LSS] = "<"u8,
    [(int)GTR] = ">"u8,
    [(int)ASSIGN] = "="u8,
    [(int)NOT] = "!"u8,
    [(int)NEQ] = "!="u8,
    [(int)LEQ] = "<="u8,
    [(int)GEQ] = ">="u8,
    [(int)DEFINE] = ":="u8,
    [(int)ELLIPSIS] = "..."u8,
    [(int)LPAREN] = "("u8,
    [(int)LBRACK] = "["u8,
    [(int)LBRACE] = "{"u8,
    [(int)COMMA] = ","u8,
    [(int)PERIOD] = "."u8,
    [(int)RPAREN] = ")"u8,
    [(int)RBRACK] = "]"u8,
    [(int)RBRACE] = "}"u8,
    [(int)SEMICOLON] = ";"u8,
    [(int)COLON] = ":"u8,
    [(int)BREAK] = "break"u8,
    [(int)CASE] = "case"u8,
    [(int)CHAN] = "chan"u8,
    [(int)CONST] = "const"u8,
    [(int)CONTINUE] = "continue"u8,
    [(int)DEFAULT] = "default"u8,
    [(int)DEFER] = "defer"u8,
    [(int)ELSE] = "else"u8,
    [(int)FALLTHROUGH] = "fallthrough"u8,
    [(int)FOR] = "for"u8,
    [(int)FUNC] = "func"u8,
    [(int)GO] = "go"u8,
    [(int)GOTO] = "goto"u8,
    [(int)IF] = "if"u8,
    [(int)IMPORT] = "import"u8,
    [(int)INTERFACE] = "interface"u8,
    [(int)MAP] = "map"u8,
    [(int)PACKAGE] = "package"u8,
    [(int)RANGE] = "range"u8,
    [(int)RETURN] = "return"u8,
    [(int)SELECT] = "select"u8,
    [(int)STRUCT] = "struct"u8,
    [(int)SWITCH] = "switch"u8,
    [(int)TYPE] = "type"u8,
    [(int)VAR] = "var"u8,
    [(int)TILDE] = "~"u8
}.array(89);

// String returns the string corresponding to the token tok.
// For operators, delimiters, and keywords the string is the actual
// token character sequence (e.g., for the token [ADD], the string is
// "+"). For all other tokens the string corresponds to the token
// constant name (e.g. for the token [IDENT], the string is "IDENT").
public static @string String(this Token tok) {
    @string s = ""u8;
    if (0 <= tok && tok < ((Token)len(tokens))) {
        s = tokens[tok];
    }
    if (s == ""u8) {
        s = "token("u8 + strconv.Itoa((nint)tok) + ")"u8;
    }
    return s;
}

// A set of constants for precedence-based expression parsing.
// Non-operators have lowest precedence, followed by operators
// starting with precedence 1 up to unary operators. The highest
// precedence serves as "catch-all" precedence for selector,
// indexing, and other operator and delimiter tokens.
public static UntypedInt LowestPrec => 0; // non-operators

public static UntypedInt UnaryPrec => 6;

public static UntypedInt HighestPrec => 7;

// Precedence returns the operator precedence of the binary
// operator op. If op is not a binary operator, the result
// is LowestPrecedence.
public static nint Precedence(this Token op) {
    var exprᴛ1 = op;
    if (exprᴛ1 == LOR) {
        return 1;
    }
    if (exprᴛ1 == LAND) {
        return 2;
    }
    if (exprᴛ1 == EQL || exprᴛ1 == NEQ || exprᴛ1 == LSS || exprᴛ1 == LEQ || exprᴛ1 == GTR || exprᴛ1 == GEQ) {
        return 3;
    }
    if (exprᴛ1 == ADD || exprᴛ1 == SUB || exprᴛ1 == OR || exprᴛ1 == XOR) {
        return 4;
    }
    if (exprᴛ1 == MUL || exprᴛ1 == QUO || exprᴛ1 == REM || exprᴛ1 == SHL || exprᴛ1 == SHR || exprᴛ1 == AND || exprᴛ1 == AND_NOT) {
        return 5;
    }

    return LowestPrec;
}

internal static map<@string, Token> keywords;

[GoInit] internal static void init() {
    keywords = new map<@string, Token>(keyword_end - (keyword_beg + 1));
    for (Token i = keyword_beg + 1; i < keyword_end; i++) {
        keywords[tokens[i]] = i;
    }
}

// Lookup maps an identifier to its keyword token or [IDENT] (if not a keyword).
public static Token Lookup(@string ident) {
    {
        var (tok, is_keyword) = keywords[ident, ꟷ]; if (is_keyword) {
            return tok;
        }
    }
    return IDENT;
}

// Predicates

// IsLiteral returns true for tokens corresponding to identifiers
// and basic type literals; it returns false otherwise.
public static bool IsLiteral(this Token tok) {
    return literal_beg < tok && tok < literal_end;
}

// IsOperator returns true for tokens corresponding to operators and
// delimiters; it returns false otherwise.
public static bool IsOperator(this Token tok) {
    return (operator_beg < tok && tok < operator_end) || tok == TILDE;
}

// IsKeyword returns true for tokens corresponding to keywords;
// it returns false otherwise.
public static bool IsKeyword(this Token tok) {
    return keyword_beg < tok && tok < keyword_end;
}

// IsExported reports whether name starts with an upper-case letter.
public static bool IsExported(@string name) {
    var (ch, _) = utf8.DecodeRuneInString(name);
    return unicode.IsUpper(ch);
}

// IsKeyword reports whether name is a Go keyword, such as "func" or "return".
public static bool IsKeyword(@string name) {
    // TODO: opt: use a perfect hash function instead of a global map.
    var (_, ok) = keywords[name, ꟷ];
    return ok;
}

// IsIdentifier reports whether name is a Go identifier, that is, a non-empty
// string made up of letters, digits, and underscores, where the first character
// is not a digit. Keywords are not identifiers.
public static bool IsIdentifier(@string name) {
    if (name == ""u8 || IsKeyword(name)) {
        return false;
    }
    foreach (var (i, c) in name) {
        if (!unicode.IsLetter(c) && c != (rune)'_' && (i == 0 || !unicode.IsDigit(c))) {
            return false;
        }
    }
    return true;
}

} // end token_package
