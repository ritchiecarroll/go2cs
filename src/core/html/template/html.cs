// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using io = io_package;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

// htmlNospaceEscaper escapes for inclusion in unquoted attribute values.
internal static @string htmlNospaceEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
    if (s == ""u8) {
        return filterFailsafe;
    }
    if (t == contentTypeHTML) {
        return htmlReplacer(stripTags(s), htmlNospaceNormReplacementTable, false);
    }
    return htmlReplacer(s, htmlNospaceReplacementTable, false);
}

// attrEscaper escapes for inclusion in quoted attribute values.
internal static @string attrEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
    if (t == contentTypeHTML) {
        return htmlReplacer(stripTags(s), htmlNormReplacementTable, true);
    }
    return htmlReplacer(s, htmlReplacementTable, true);
}

// rcdataEscaper escapes for inclusion in an RCDATA element body.
internal static @string rcdataEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
    if (t == contentTypeHTML) {
        return htmlReplacer(s, htmlNormReplacementTable, true);
    }
    return htmlReplacer(s, htmlReplacementTable, true);
}

// htmlEscaper escapes for inclusion in HTML text.
internal static @string htmlEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
    if (t == contentTypeHTML) {
        return s;
    }
    return htmlReplacer(s, htmlReplacementTable, true);
}

// https://www.w3.org/TR/html5/syntax.html#attribute-value-(unquoted)-state
// U+0000 NULL Parse error. Append a U+FFFD REPLACEMENT
// CHARACTER character to the current attribute's value.
// "
// and similarly
// https://www.w3.org/TR/html5/syntax.html#before-attribute-value-state
// htmlReplacementTable contains the runes that need to be escaped
// inside a quoted attribute value or in a text node.
internal static slice<@string> htmlReplacementTable = new slice<@string>(63){
    [0] = "\uFFFD"u8,
    [(rune)'"'] = "&#34;"u8,
    [(rune)'&'] = "&amp;"u8,
    [(rune)'\''] = "&#39;"u8,
    [(rune)'+'] = "&#43;"u8,
    [(rune)'<'] = "&lt;"u8,
    [(rune)'>'] = "&gt;"u8
};

// htmlNormReplacementTable is like htmlReplacementTable but without '&' to
// avoid over-encoding existing entities.
internal static slice<@string> htmlNormReplacementTable = new slice<@string>(63){
    [0] = "\uFFFD"u8,
    [(rune)'"'] = "&#34;"u8,
    [(rune)'\''] = "&#39;"u8,
    [(rune)'+'] = "&#43;"u8,
    [(rune)'<'] = "&lt;"u8,
    [(rune)'>'] = "&gt;"u8
};

// A parse error in the attribute value (unquoted) and
// before attribute value states.
// Treated as a quoting character by IE.
// htmlNospaceReplacementTable contains the runes that need to be escaped
// inside an unquoted attribute value.
// The set of runes escaped is the union of the HTML specials and
// those determined by running the JS below in browsers:
// <div id=d></div>
// <script>(function () {
// var a = [], d = document.getElementById("d"), i, c, s;
// for (i = 0; i < 0x10000; ++i) {
//
//	c = String.fromCharCode(i);
//	d.innerHTML = "<span title=" + c + "lt" + c + "></span>"
//	s = d.getElementsByTagName("SPAN")[0];
//	if (!s || s.title !== c + "lt" + c) { a.push(i.toString(16)); }
//
// }
// document.write(a.join(", "));
// })()</script>
internal static slice<@string> htmlNospaceReplacementTable = new slice<@string>(97){
    [0] = "&#xfffd;"u8,
    [(rune)'\t'] = "&#9;"u8,
    [(rune)'\n'] = "&#10;"u8,
    [(rune)'\v'] = "&#11;"u8,
    [(rune)'\f'] = "&#12;"u8,
    [(rune)'\r'] = "&#13;"u8,
    [(rune)' '] = "&#32;"u8,
    [(rune)'"'] = "&#34;"u8,
    [(rune)'&'] = "&amp;"u8,
    [(rune)'\''] = "&#39;"u8,
    [(rune)'+'] = "&#43;"u8,
    [(rune)'<'] = "&lt;"u8,
    [(rune)'='] = "&#61;"u8,
    [(rune)'>'] = "&gt;"u8,
    [(rune)'`'] = "&#96;"u8
};

// A parse error in the attribute value (unquoted) and
// before attribute value states.
// Treated as a quoting character by IE.
// htmlNospaceNormReplacementTable is like htmlNospaceReplacementTable but
// without '&' to avoid over-encoding existing entities.
internal static slice<@string> htmlNospaceNormReplacementTable = new slice<@string>(97){
    [0] = "&#xfffd;"u8,
    [(rune)'\t'] = "&#9;"u8,
    [(rune)'\n'] = "&#10;"u8,
    [(rune)'\v'] = "&#11;"u8,
    [(rune)'\f'] = "&#12;"u8,
    [(rune)'\r'] = "&#13;"u8,
    [(rune)' '] = "&#32;"u8,
    [(rune)'"'] = "&#34;"u8,
    [(rune)'\''] = "&#39;"u8,
    [(rune)'+'] = "&#43;"u8,
    [(rune)'<'] = "&lt;"u8,
    [(rune)'='] = "&#61;"u8,
    [(rune)'>'] = "&gt;"u8,
    [(rune)'`'] = "&#96;"u8
};

// htmlReplacer returns s with runes replaced according to replacementTable
// and when badRunes is true, certain bad runes are allowed through unescaped.
internal static @string htmlReplacer(@string s, slice<@string> replacementTable, bool badRunes) {
    nint written = 0;
    var b = @new<strings.Builder>();
    var r = (rune)0;
    nint w = 0;
    for (nint i = 0; i < len(s); i += w) {
        // Cannot use 'for range s' because we need to preserve the width
        // of the runes in the input. If we see a decoding error, the input
        // width will not be utf8.Runelen(r) and we will overrun the buffer.
        (r, w) = utf8.DecodeRuneInString(s.slice(i));
        if ((nint)r < len(replacementTable)){
            {
                @string repl = replacementTable[r]; if (len(repl) != 0) {
                    if (written == 0) {
                        b.Grow(len(s));
                    }
                    b.WriteString(s.slice(written, i));
                    b.WriteString(repl);
                    written = i + w;
                }
            }
        } else 
        if (badRunes){
        } else 
        if (0xfdd0 <= r && r <= 0xfdef || 0xfff0 <= r && r <= 0xffff) {
            // No-op.
            // IE does not allow these ranges in unquoted attrs.
            if (written == 0) {
                b.Grow(len(s));
            }
            fmt.Fprintf(new strings_BuilderжWriter(b), "%s&#x%x;"u8, s.slice(written, i), r);
            written = i + w;
        }
    }
    if (written == 0) {
        return s;
    }
    b.WriteString(s.slice(written));
    return b.String();
}

// stripTags takes a snippet of HTML and returns only the text content.
// For example, `<b>&iexcl;Hi!</b> <script>...</script>` -> `&iexcl;Hi! `.
internal static @string stripTags(@string html) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    var s = slice<byte>(html);
    var c = new context(nil);
    nint i = 0;
    var allText = true;
    // Using the transition funcs helps us avoid mangling
    // `<div title="1>2">` or `I <3 Ponies!`.
    while (i != len(s)) {
        if (c.delim == delimNone) {
            var st = c.state;
            // Use RCDATA instead of parsing into JS or CSS styles.
            if (c.element != elementNone && !isInTag(st)) {
                st = stateRCDATA;
            }
            var (d, nread) = transitionFunc[st](c, s.slice(i));
            nint i1Δ1 = i + nread;
            if (c.state == stateText || c.state == stateRCDATA){
                // Emit text up to the start of the tag or comment.
                nint j = i1Δ1;
                if (d.state != c.state) {
                    for (nint j1 = j - 1; j1 >= i; j1--) {
                        if (s[j1] == (rune)'<') {
                            j = j1;
                            break;
                        }
                    }
                }
                Ꮡb.Write(s.slice(i, j));
            } else {
                allText = false;
            }
            (c, i) = (d, i1Δ1);
            continue;
        }
        nint i1 = i + bytes.IndexAny(s.slice(i), delimEnds[c.delim]);
        if (i1 < i) {
            break;
        }
        if (c.delim != delimSpaceOrTagEnd) {
            // Consume any quote.
            i1++;
        }
        (c, i) = (new context(state: stateTag, element: c.element), i1);
    }
    if (allText){
        return html;
    } else 
    if (c.state == stateText || c.state == stateRCDATA) {
        Ꮡb.Write(s.slice(i));
    }
    return b.String();
}

// htmlNameFilter accepts valid parts of an HTML attribute or tag name or
// a known-safe HTML attribute.
internal static @string htmlNameFilter(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
    if (t == contentTypeHTMLAttr) {
        return s;
    }
    if (len(s) == 0) {
        // Avoid violation of structure preservation.
        // <input checked {{.K}}={{.V}}>.
        // Without this, if .K is empty then .V is the value of
        // checked, but otherwise .V is the value of the attribute
        // named .K.
        return filterFailsafe;
    }
    s = strings.ToLower(s);
    {
        var tΔ1 = attrType(s); if (tΔ1 != contentTypePlain) {
            // TODO: Split attr and element name part filters so we can recognize known attributes.
            return filterFailsafe;
        }
    }
    foreach (var (_, r) in s) {
        switch (ᐧ) {
        case {} when (rune)'0' <= r && r <= (rune)'9': {
            break;
        }
        case {} when (rune)'a' <= r && r <= (rune)'z': {
            break;
        }
        default: {
            return filterFailsafe;
        }}

    }
    return s;
}

// commentEscaper returns the empty string regardless of input.
// Comment content does not correspond to any parsed structure or
// human-readable content, so the simplest and most secure policy is to drop
// content interpolated into comments.
// This approach is equally valid whether or not static comment content is
// removed from the template.
internal static @string commentEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return ""u8;
}

} // end template_package
