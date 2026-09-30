// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using bytes = bytes_package;
using strings = strings_package;
using parse = text.template.parse_package;

partial class template_package {

// transitionFunc is the array of context transition functions for text nodes.
// A transition function takes a context and template text input, and returns
// the updated context and the number of bytes consumed from the front of the
// input.
internal static array<Func<context, slice<byte>, (context, nint)>> transitionFunc;
internal static void initᴛtransitionFunc() { transitionFunc = new golib.SparseArray<Func<context, slice<byte>, (context, nint)>>{
    [stateText] = tText,
    [stateTag] = tTag,
    [stateAttrName] = tAttrName,
    [stateAfterName] = tAfterName,
    [stateBeforeValue] = tBeforeValue,
    [stateHTMLCmt] = tHTMLCmt,
    [stateRCDATA] = tSpecialTagEnd,
    [stateAttr] = tAttr,
    [stateURL] = tURL,
    [stateSrcset] = tURL,
    [stateJS] = tJS,
    [stateJSDqStr] = tJSDelimited,
    [stateJSSqStr] = tJSDelimited,
    [stateJSRegexp] = tJSDelimited,
    [stateJSTmplLit] = tJSTmpl,
    [stateJSBlockCmt] = tBlockCmt,
    [stateJSLineCmt] = tLineCmt,
    [stateJSHTMLOpenCmt] = tLineCmt,
    [stateJSHTMLCloseCmt] = tLineCmt,
    [stateCSS] = tCSS,
    [stateCSSDqStr] = tCSSStr,
    [stateCSSSqStr] = tCSSStr,
    [stateCSSDqURL] = tCSSStr,
    [stateCSSSqURL] = tCSSStr,
    [stateCSSURL] = tCSSStr,
    [stateCSSBlockCmt] = tBlockCmt,
    [stateCSSLineCmt] = tLineCmt,
    [stateError] = tError
}.array(); }

internal static slice<byte> commentStart = slice<byte>("<!--"u8);

internal static slice<byte> commentEnd = slice<byte>("-->"u8);

// tText is the context transition function for the text state.
internal static (context, nint) tText(context c, slice<byte> s) {
    nint k = 0;
    while (ᐧ) {
        nint i = k + bytes.IndexByte(s.slice(k), (rune)'<');
        if (i < k || i + 1 == len(s)){
            return (c, len(s));
        } else 
        if (i + 4 <= len(s) && bytes.Equal(commentStart, s.slice(i, i + 4))) {
            return (new context(state: stateHTMLCmt), i + 4);
        }
        i++;
        var end = false;
        if (s[i] == (rune)'/') {
            if (i + 1 == len(s)) {
                return (c, len(s));
            }
            (end, i) = (true, i + 1);
        }
        var (j, e) = eatTagName(s, i);
        if (j != i) {
            if (end) {
                e = elementNone;
            }
            // We've found an HTML tag.
            return (new context(state: stateTag, element: e), j);
        }
        k = j;
    }
}

internal static array<state> elementContentType = new golib.SparseArray<state>{
    [elementNone] = stateText,
    [elementScript] = stateJS,
    [elementStyle] = stateCSS,
    [elementTextarea] = stateRCDATA,
    [elementTitle] = stateRCDATA
}.array();

// tTag is the context transition function for the tag state.
internal static (context, nint) tTag(context c, slice<byte> s) {
    // Find the attribute name.
    nint i = eatWhiteSpace(s, 0);
    if (i == len(s)) {
        return (c, len(s));
    }
    if (s[i] == (rune)'>') {
        return (new context(
            state: elementContentType[c.element],
            element: c.element
        ), i + 1);
    }
    var (j, err) = eatAttrName(s, i);
    if (err != nil) {
        return (new context(state: stateError, err: err), len(s));
    }
    var (state, attr) = (stateTag, attrNone);
    if (i == j) {
        return (new context(
            state: stateError,
            err: errorf(ErrBadHTML, default!, 0, "expected space, attr name, or end of tag, but got %q"u8, s.slice(i))
        ), len(s));
    }
    @string attrName = strings.ToLower(((@string)(s.slice(i, j))));
    if (c.element == elementScript && attrName == "type"u8){
        attr = attrScriptType;
    } else {
        var exprᴛ1 = attrType(attrName);
        if (exprᴛ1 == contentTypeURL) {
            attr = attrURL;
        }
        else if (exprᴛ1 == contentTypeCSS) {
            attr = attrStyle;
        }
        else if (exprᴛ1 == contentTypeJS) {
            attr = attrScript;
        }
        else if (exprᴛ1 == contentTypeSrcset) {
            attr = attrSrcset;
        }

    }
    if (j == len(s)){
        state = stateAttrName;
    } else {
        state = stateAfterName;
    }
    return (new context(state: state, element: c.element, attr: attr), j);
}

// tAttrName is the context transition function for stateAttrName.
internal static (context, nint) tAttrName(context c, slice<byte> s) {
    var (i, err) = eatAttrName(s, 0);
    if (err != nil){
        return (new context(state: stateError, err: err), len(s));
    } else 
    if (i != len(s)) {
        c.state = stateAfterName;
    }
    return (c, i);
}

// tAfterName is the context transition function for stateAfterName.
internal static (context, nint) tAfterName(context c, slice<byte> s) {
    // Look for the start of the value.
    nint i = eatWhiteSpace(s, 0);
    if (i == len(s)){
        return (c, len(s));
    } else 
    if (s[i] != (rune)'=') {
        // Occurs due to tag ending '>', and valueless attribute.
        c.state = stateTag;
        return (c, i);
    }
    c.state = stateBeforeValue;
    // Consume the "=".
    return (c, i + 1);
}

internal static array<state> attrStartStates = new golib.SparseArray<state>{
    [attrNone] = stateAttr,
    [attrScript] = stateJS,
    [attrScriptType] = stateAttr,
    [attrStyle] = stateCSS,
    [attrURL] = stateURL,
    [attrSrcset] = stateSrcset
}.array();

// tBeforeValue is the context transition function for stateBeforeValue.
internal static (context, nint) tBeforeValue(context c, slice<byte> s) {
    nint i = eatWhiteSpace(s, 0);
    if (i == len(s)) {
        return (c, len(s));
    }
    // Find the attribute delimiter.
    var delim = delimSpaceOrTagEnd;
    switch (s[i]) {
    case (rune)'\'': {
        (delim, i) = (delimSingleQuote, i + 1);
        break;
    }
    case (rune)'"': {
        (delim, i) = (delimDoubleQuote, i + 1);
        break;
    }}

    (c.state, c.delim) = (attrStartStates[c.attr], delim);
    return (c, i);
}

// tHTMLCmt is the context transition function for stateHTMLCmt.
internal static (context, nint) tHTMLCmt(context c, slice<byte> s) {
    {
        nint i = bytes.Index(s, commentEnd); if (i != -1) {
            return (new context(nil), i + 3);
        }
    }
    return (c, len(s));
}

// specialTagEndMarkers maps element types to the character sequence that
// case-insensitively signals the end of the special tag body.
internal static array<slice<byte>> specialTagEndMarkers = new golib.SparseArray<slice<byte>>{
    [elementScript] = slice<byte>("script"u8),
    [elementStyle] = slice<byte>("style"u8),
    [elementTextarea] = slice<byte>("textarea"u8),
    [elementTitle] = slice<byte>("title"u8)
}.array(5);

internal static slice<byte> specialTagEndPrefix = slice<byte>("</"u8);
internal static slice<byte> tagEndSeparators = slice<byte>("> \t\n\f/"u8);

// tSpecialTagEnd is the context transition function for raw text and RCDATA
// element states.
internal static (context, nint) tSpecialTagEnd(context c, slice<byte> s) {
    if (c.element != elementNone) {
        // script end tags ("</script") within script literals are ignored, so that
        // we can properly escape them.
        if (c.element == elementScript && (isInScriptLiteral(c.state) || isComment(c.state))) {
            return (c, len(s));
        }
        {
            nint i = indexTagEnd(s, specialTagEndMarkers[c.element]); if (i != -1) {
                return (new context(nil), i);
            }
        }
    }
    return (c, len(s));
}

// indexTagEnd finds the index of a special tag end in a case insensitive way, or returns -1
internal static nint indexTagEnd(slice<byte> s, slice<byte> tag) {
    nint res = 0;
    nint plen = len(specialTagEndPrefix);
    while (len(s) > 0) {
        // Try to find the tag end prefix first
        nint i = bytes.Index(s, specialTagEndPrefix);
        if (i == -1) {
            return i;
        }
        s = s.slice(i + plen);
        // Try to match the actual tag if there is still space for it
        if (len(tag) <= len(s) && bytes.EqualFold(tag, s.slice(0, len(tag)))) {
            s = s.slice(len(tag));
            // Check the tag is followed by a proper separator
            if (len(s) > 0 && bytes.IndexByte(tagEndSeparators, s[0]) != -1) {
                return res + i;
            }
            res += len(tag);
        }
        res += i + plen;
    }
    return -1;
}

// tAttr is the context transition function for the attribute state.
internal static (context, nint) tAttr(context c, slice<byte> s) {
    return (c, len(s));
}

// tURL is the context transition function for the URL state.
internal static (context, nint) tURL(context c, slice<byte> s) {
    if (bytes.ContainsAny(s, "#?"u8)){
        c.urlPart = urlPartQueryOrFrag;
    } else 
    if (len(s) != eatWhiteSpace(s, 0) && c.urlPart == urlPartNone) {
        // HTML5 uses "Valid URL potentially surrounded by spaces" for
        // attrs: https://www.w3.org/TR/html5/index.html#attributes-1
        c.urlPart = urlPartPreQuery;
    }
    return (c, len(s));
}

// tJS is the context transition function for the JS state.
internal static (context, nint) tJS(context c, slice<byte> s) {
    nint i = bytes.IndexAny(s, "\"`'/{}<-#"u8);
    if (i == -1) {
        // Entire input is non string, comment, regexp tokens.
        c.jsCtx = nextJSCtx(s, c.jsCtx);
        return (c, len(s));
    }
    c.jsCtx = nextJSCtx(s.slice(0, i), c.jsCtx);
    switch (s[i]) {
    case (rune)'"': {
        (c.state, c.jsCtx) = (stateJSDqStr, jsCtxRegexp);
        break;
    }
    case (rune)'\'': {
        (c.state, c.jsCtx) = (stateJSSqStr, jsCtxRegexp);
        break;
    }
    case (rune)'`': {
        (c.state, c.jsCtx) = (stateJSTmplLit, jsCtxRegexp);
        break;
    }
    case (rune)'/': {
        switch (ᐧ) {
        case {} when i + 1 < len(s) && s[i + 1] == (rune)'/': {
            (c.state, i) = (stateJSLineCmt, i + 1);
            break;
        }
        case {} when i + 1 < len(s) && s[i + 1] == (rune)'*': {
            (c.state, i) = (stateJSBlockCmt, i + 1);
            break;
        }
        case {} when c.jsCtx == jsCtxRegexp: {
            c.state = stateJSRegexp;
            break;
        }
        case {} when c.jsCtx == jsCtxDivOp: {
            c.jsCtx = jsCtxRegexp;
            break;
        }
        default: {
            return (new context(
                state: stateError,
                err: errorf(ErrSlashAmbig, default!, 0, "'/' could start a division or regexp: %.32q"u8, s.slice(i))
            ), len(s));
        }}

        break;
    }
    case (rune)'<': {
        if (i + 3 < len(s) && bytes.Equal(commentStart, // ECMAScript supports HTML style comments for legacy reasons, see Appendix
 // B.1.1 "HTML-like Comments". The handling of these comments is somewhat
 // confusing. Multi-line comments are not supported, i.e. anything on lines
 // between the opening and closing tokens is not considered a comment, but
 // anything following the opening or closing token, on the same line, is
 // ignored. As such we simply treat any line prefixed with "<!--" or "-->"
 // as if it were actually prefixed with "//" and move on.
 s.slice(i, i + 4))) {
            (c.state, i) = (stateJSHTMLOpenCmt, i + 3);
        }
        break;
    }
    case (rune)'-': {
        if (i + 2 < len(s) && bytes.Equal(commentEnd, s.slice(i, i + 3))) {
            (c.state, i) = (stateJSHTMLCloseCmt, i + 2);
        }
        break;
    }
    case (rune)'#': {
        if (i + 1 < len(s) && s[i + 1] == (rune)'!') {
            // ECMAScript also supports "hashbang" comment lines, see Section 12.5.
            (c.state, i) = (stateJSLineCmt, i + 1);
        }
        break;
    }
    case (rune)'{': {
        if (len(c.jsBraceDepth) == 0) {
            // We only care about tracking brace depth if we are inside of a
            // template literal.
            return (c, i + 1);
        }
        c.jsBraceDepth[len(c.jsBraceDepth) - 1]++;
        break;
    }
    case (rune)'}': {
        if (len(c.jsBraceDepth) == 0) {
            return (c, i + 1);
        }
        c.jsBraceDepth[len(c.jsBraceDepth) - 1]--;
        if (c.jsBraceDepth[len(c.jsBraceDepth) - 1] >= 0) {
            // There are no cases where a brace can be escaped in the JS context
            // that are not syntax errors, it seems. Because of this we can just
            // count "\}" as "}" and move on, the script is already broken as
            // fully fledged parsers will just fail anyway.
            return (c, i + 1);
        }
        c.jsBraceDepth = c.jsBraceDepth.slice(0, len(c.jsBraceDepth) - 1);
        c.state = stateJSTmplLit;
        break;
    }
    default: {
        throw panic("unreachable");
        break;
    }}

    return (c, i + 1);
}

internal static (context, nint) tJSTmpl(context c, slice<byte> s) {
    nint k = default!;
    while (ᐧ) {
        nint i = k + bytes.IndexAny(s.slice(k), "`\\$"u8);
        if (i < k) {
            break;
        }
        switch (s[i]) {
        case (rune)'\\': {
            i++;
            if (i == len(s)) {
                return (new context(
                    state: stateError,
                    err: errorf(ErrPartialEscape, default!, 0, "unfinished escape sequence in JS string: %q"u8, s)
                ), len(s));
            }
            break;
        }
        case (rune)'$': {
            if (len(s) >= i + 2 && s[i + 1] == (rune)'{') {
                c.jsBraceDepth = append(c.jsBraceDepth, (nint)(0));
                c.state = stateJS;
                return (c, i + 2);
            }
            break;
        }
        case (rune)'`': {
            c.state = stateJS;
            return (c, i + 1);
        }}

        // end
        k = i + 1;
    }
    return (c, len(s));
}

// tJSDelimited is the context transition function for the JS string and regexp
// states.
internal static (context, nint) tJSDelimited(context c, slice<byte> s) {
    @string specials = @"\"""u8;
    var exprᴛ1 = c.state;
    if (exprᴛ1 == stateJSSqStr) {
        specials = @"\'"u8;
    }
    else if (exprᴛ1 == stateJSRegexp) {
        specials = @"\/[]"u8;
    }

    nint k = 0;
    var inCharset = false;
    while (ᐧ) {
        nint i = k + bytes.IndexAny(s.slice(k), specials);
        if (i < k) {
            break;
        }
        switch (s[i]) {
        case (rune)'\\': {
            i++;
            if (i == len(s)) {
                return (new context(
                    state: stateError,
                    err: errorf(ErrPartialEscape, default!, 0, "unfinished escape sequence in JS string: %q"u8, s)
                ), len(s));
            }
            break;
        }
        case (rune)'[': {
            inCharset = true;
            break;
        }
        case (rune)']': {
            inCharset = false;
            break;
        }
        case (rune)'/': {
            if (i > 0 && i + 7 <= len(s) && bytes.Equal(bytes.ToLower(s.slice(i - 1, i + 7)), // If "</script" appears in a regex literal, the '/' should not
 // close the regex literal, and it will later be escaped to
 // "\x3C/script" in escapeText.
 slice<byte>("</script"u8))){
                i++;
            } else 
            if (!inCharset) {
                (c.state, c.jsCtx) = (stateJS, jsCtxDivOp);
                return (c, i + 1);
            }
            break;
        }
        default: {
            if (!inCharset) {
                // end delimiter
                (c.state, c.jsCtx) = (stateJS, jsCtxDivOp);
                return (c, i + 1);
            }
            break;
        }}

        k = i + 1;
    }
    if (inCharset) {
        // This can be fixed by making context richer if interpolation
        // into charsets is desired.
        return (new context(
            state: stateError,
            err: errorf(ErrPartialCharset, default!, 0, "unfinished JS regexp charset: %q"u8, s)
        ), len(s));
    }
    return (c, len(s));
}

internal static slice<byte> blockCommentEnd = slice<byte>("*/"u8);

// tBlockCmt is the context transition function for /*comment*/ states.
internal static (context, nint) tBlockCmt(context c, slice<byte> s) {
    nint i = bytes.Index(s, blockCommentEnd);
    if (i == -1) {
        return (c, len(s));
    }
    var exprᴛ1 = c.state;
    if (exprᴛ1 == stateJSBlockCmt) {
        c.state = stateJS;
    }
    else if (exprᴛ1 == stateCSSBlockCmt) {
        c.state = stateCSS;
    }
    else { /* default: */
        throw panic(c.state.String());
    }

    return (c, i + 2);
}

// tLineCmt is the context transition function for //comment states, and the JS HTML-like comment state.
internal static (context, nint) tLineCmt(context c, slice<byte> s) {
    @string lineTerminators = default!;
    state endState = default!;
    var exprᴛ1 = c.state;
    if (exprᴛ1 == stateJSLineCmt || exprᴛ1 == stateJSHTMLOpenCmt || exprᴛ1 == stateJSHTMLCloseCmt) {
        (lineTerminators, endState) = ("\n\r\u2028\u2029", stateJS);
    }
    else if (exprᴛ1 == stateCSSLineCmt) {
        (lineTerminators, endState) = ("\n\f\r", stateCSS);
    }
    else { /* default: */
        throw panic(c.state.String());
    }

    // Line comments are not part of any published CSS standard but
    // are supported by the 4 major browsers.
    // This defines line comments as
    //     LINECOMMENT ::= "//" [^\n\f\d]*
    // since https://www.w3.org/TR/css3-syntax/#SUBTOK-nl defines
    // newlines:
    //     nl ::= #xA | #xD #xA | #xD | #xC
    nint i = bytes.IndexAny(s, lineTerminators);
    if (i == -1) {
        return (c, len(s));
    }
    c.state = endState;
    // Per section 7.4 of EcmaScript 5 : https://es5.github.io/#x7.4
    // "However, the LineTerminator at the end of the line is not
    // considered to be part of the single-line comment; it is
    // recognized separately by the lexical grammar and becomes part
    // of the stream of input elements for the syntactic grammar."
    return (c, i);
}

// tCSS is the context transition function for the CSS state.
internal static (context, nint) tCSS(context c, slice<byte> s) {
    // CSS quoted strings are almost never used except for:
    // (1) URLs as in background: "/foo.png"
    // (2) Multiword font-names as in font-family: "Times New Roman"
    // (3) List separators in content values as in inline-lists:
    //    <style>
    //    ul.inlineList { list-style: none; padding:0 }
    //    ul.inlineList > li { display: inline }
    //    ul.inlineList > li:before { content: ", " }
    //    ul.inlineList > li:first-child:before { content: "" }
    //    </style>
    //    <ul class=inlineList><li>One<li>Two<li>Three</ul>
    // (4) Attribute value selectors as in a[href="http://example.com/"]
    //
    // We conservatively treat all strings as URLs, but make some
    // allowances to avoid confusion.
    //
    // In (1), our conservative assumption is justified.
    // In (2), valid font names do not contain ':', '?', or '#', so our
    // conservative assumption is fine since we will never transition past
    // urlPartPreQuery.
    // In (3), our protocol heuristic should not be tripped, and there
    // should not be non-space content after a '?' or '#', so as long as
    // we only %-encode RFC 3986 reserved characters we are ok.
    // In (4), we should URL escape for URL attributes, and for others we
    // have the attribute name available if our conservative assumption
    // proves problematic for real code.
    nint k = 0;
    while (ᐧ) {
        nint i = k + bytes.IndexAny(s.slice(k), @"(""'/"u8);
        if (i < k) {
            return (c, len(s));
        }
        switch (s[i]) {
        case (rune)'(': {
            var p = bytes.TrimRight(s.slice(0, i), // Look for url to the left.
 "\t\n\f\r "u8);
            if (endsWithCSSKeyword(p, urlˢ)) {
                nint j = len(s) - len(bytes.TrimLeft(s.slice(i + 1), "\t\n\f\r "u8));
                switch (ᐧ) {
                case {} when j != len(s) && s[j] == (rune)'"': {
                    (c.state, j) = (stateCSSDqURL, j + 1);
                    break;
                }
                case {} when j != len(s) && s[j] == (rune)'\'': {
                    (c.state, j) = (stateCSSSqURL, j + 1);
                    break;
                }
                default: {
                    c.state = stateCSSURL;
                    break;
                }}

                return (c, j);
            }
            break;
        }
        case (rune)'/': {
            if (i + 1 < len(s)) {
                switch (s[i + 1]) {
                case (rune)'/': {
                    c.state = stateCSSLineCmt;
                    return (c, i + 2);
                }
                case (rune)'*': {
                    c.state = stateCSSBlockCmt;
                    return (c, i + 2);
                }}

            }
            break;
        }
        case (rune)'"': {
            c.state = stateCSSDqStr;
            return (c, i + 1);
        }
        case (rune)'\'': {
            c.state = stateCSSSqStr;
            return (c, i + 1);
        }}

        k = i + 1;
    }
}

// tCSSStr is the context transition function for the CSS string and URL states.
internal static (context, nint) tCSSStr(context c, slice<byte> s) {
    @string endAndEsc = default!;
    var exprᴛ1 = c.state;
    if (exprᴛ1 == stateCSSDqStr || exprᴛ1 == stateCSSDqURL) {
        endAndEsc = @"\"""u8;
    }
    else if (exprᴛ1 == stateCSSSqStr || exprᴛ1 == stateCSSSqURL) {
        endAndEsc = @"\'"u8;
    }
    else if (exprᴛ1 == stateCSSURL) {
        endAndEsc = "\\\t\n\f\r )"u8;
    }
    else { /* default: */
        throw panic(c.state.String());
    }

    // Unquoted URLs end with a newline or close parenthesis.
    // The below includes the wc (whitespace character) and nl.
    nint k = 0;
    while (ᐧ) {
        nint i = k + bytes.IndexAny(s.slice(k), endAndEsc);
        if (i < k) {
            var (cΔ1, nread) = tURL(c, decodeCSS(s.slice(k)));
            return (cΔ1, k + nread);
        }
        if (s[i] == (rune)'\\'){
            i++;
            if (i == len(s)) {
                return (new context(
                    state: stateError,
                    err: errorf(ErrPartialEscape, default!, 0, "unfinished escape sequence in CSS string: %q"u8, s)
                ), len(s));
            }
        } else {
            c.state = stateCSS;
            return (c, i + 1);
        }
        (c, _) = tURL(c, decodeCSS(s.slice(0, i + 1)));
        k = i + 1;
    }
}

// tError is the context transition function for the error state.
internal static (context, nint) tError(context c, slice<byte> s) {
    return (c, len(s));
}

// eatAttrName returns the largest j such that s[i:j] is an attribute name.
// It returns an error if s[i:] does not look like it begins with an
// attribute name, such as encountering a quote mark without a preceding
// equals sign.
internal static (nint, ж<ΔError>) eatAttrName(slice<byte> s, nint i) {
    for (nint j = i; j < len(s); j++) {
        switch (s[j]) {
        case (rune)' ' or (rune)'\t' or (rune)'\n' or (rune)'\f' or (rune)'\r' or (rune)'=' or (rune)'>': {
            return (j, default!);
        }
        case (rune)'\'' or (rune)'"' or (rune)'<': {
            return (-1, errorf(ErrBadHTML, // These result in a parse warning in HTML5 and are
 // indicative of serious problems if seen in an attr
 // name in a template.
 default!, 0, "%q in attribute name: %.32q"u8, s.slice(j, j + 1), s));
        }
        default: {
            break;
        }}

    }
    // No-op.
    return (len(s), default!);
}

internal static map<@string, element> elementNameMap = new map<@string, element>{
    ["script"u8] = elementScript,
    ["style"u8] = elementStyle,
    ["textarea"u8] = elementTextarea,
    ["title"u8] = elementTitle
};

// asciiAlpha reports whether c is an ASCII letter.
internal static bool asciiAlpha(byte c) {
    return (rune)'A' <= c && c <= (rune)'Z' || (rune)'a' <= c && c <= (rune)'z';
}

// asciiAlphaNum reports whether c is an ASCII letter or digit.
internal static bool asciiAlphaNum(byte c) {
    return asciiAlpha(c) || (rune)'0' <= c && c <= (rune)'9';
}

// eatTagName returns the largest j such that s[i:j] is a tag name and the tag type.
internal static (nint, element) eatTagName(slice<byte> s, nint i) {
    if (i == len(s) || !asciiAlpha(s[i])) {
        return (i, elementNone);
    }
    nint j = i + 1;
    while (j < len(s)) {
        var x = s[j];
        if (asciiAlphaNum(x)) {
            j++;
            continue;
        }
        // Allow "x-y" or "x:y" but not "x-", "-y", or "x--y".
        if ((x == (rune)':' || x == (rune)'-') && j + 1 < len(s) && asciiAlphaNum(s[j + 1])) {
            j += 2;
            continue;
        }
        break;
    }
    return (j, elementNameMap[strings.ToLower(((@string)(s.slice(i, j))))]);
}

// eatWhiteSpace returns the largest j such that s[i:j] is white space.
internal static nint eatWhiteSpace(slice<byte> s, nint i) {
    for (nint j = i; j < len(s); j++) {
        switch (s[j]) {
        case (rune)' ' or (rune)'\t' or (rune)'\n' or (rune)'\f' or (rune)'\r': {
            break;
        }
        default: {
            return j;
        }}

    }
    // No-op.
    return len(s);
}

} // end template_package
