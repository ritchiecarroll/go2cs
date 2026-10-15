// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using fmt = fmt_package;
using slices = slices_package;
using parse = text.template.parse_package;
using text.template;

partial class template_package {

// context describes the state an HTML parser must be in when it reaches the
// portion of HTML produced by evaluating a particular template node.
//
// The zero value of type context is the start context for a template that
// produces an HTML fragment as defined at
// https://www.w3.org/TR/html5/syntax.html#the-end
// where the context element is null.
partial struct context {
    internal state state;
    internal delim delim;
    internal urlPart urlPart;
    internal jsCtx jsCtx;
    // jsBraceDepth contains the current depth, for each JS template literal
    // string interpolation expression, of braces we've seen. This is used to
    // determine if the next } will close a JS template literal string
    // interpolation expression or not.
    internal slice<nint> jsBraceDepth;
    internal attr attr;
    internal element element;
    internal parse.Node n; // for range break/continue
    internal ж<ΔError> err;
}

internal static @string String(this context c) {
    error err = default!;
    if (c.err != nil) {
        err = new ΔErrorжerror(c.err);
    }
    return fmt.Sprintf("{%v %v %v %v %v %v %v %v}"u8, c.state, c.delim, c.urlPart, c.jsCtx, c.jsBraceDepth, c.attr, c.element, err);
}

// eq reports whether two contexts are equal.
internal static bool eq(this context c, context d) {
    return c.state == d.state && c.delim == d.delim && c.urlPart == d.urlPart && c.jsCtx == d.jsCtx && slices.Equal<slice<nint>, nint>(c.jsBraceDepth, d.jsBraceDepth) && c.attr == d.attr && c.element == d.element && c.err == d.err;
}

// mangle produces an identifier that includes a suffix that distinguishes it
// from template names mangled with different contexts.
internal static @string mangle(this context c, @string templateName) {
    // The mangled name for the default context is the input templateName.
    if (c.state == stateText) {
        return templateName;
    }
    @string s = templateName + "$htmltemplate_"u8 + c.state.String();
    if (c.delim != delimNone) {
        s += "_"u8 + c.delim.String();
    }
    if (c.urlPart != urlPartNone) {
        s += "_"u8 + c.urlPart.String();
    }
    if (c.jsCtx != jsCtxRegexp) {
        s += "_"u8 + c.jsCtx.String();
    }
    if (c.jsBraceDepth != default!) {
        s += fmt.Sprintf("_jsBraceDepth(%v)"u8, c.jsBraceDepth);
    }
    if (c.attr != attrNone) {
        s += "_"u8 + c.attr.String();
    }
    if (c.element != elementNone) {
        s += "_"u8 + c.element.String();
    }
    return s;
}

// clone returns a copy of c with the same field values.
internal static context clone(this context c) {
    var clone = c;
    clone.jsBraceDepth = slices.Clone<slice<nint>, nint>(c.jsBraceDepth);
    return clone;
}

// state describes a high-level HTML parser state.
//
// It bounds the top of the element stack, and by extension the HTML insertion
// mode, but also contains state that does not correspond to anything in the
// HTML5 parsing algorithm because a single token production in the HTML
// grammar may contain embedded actions in a template. For instance, the quoted
// HTML attribute produced by
//
//	<div title="Hello {{.World}}">
//
// is a single token in HTML's grammar but in a template spans several nodes.
partial struct state /*num:uint8*/;

//go:generate stringer -type state

// stateText is parsed character data. An HTML parser is in
// this state when its parse position is outside an HTML tag,
// directive, comment, and special element body.
internal static state stateText => /* iota */ 0;
// stateTag occurs before an HTML attribute or the end of a tag.
internal static state stateTag => 1;
// stateAttrName occurs inside an attribute name.
// It occurs between the ^'s in ` ^name^ = value`.
internal static state stateAttrName => 2;
// stateAfterName occurs after an attr name has ended but before any
// equals sign. It occurs between the ^'s in ` name^ ^= value`.
internal static state stateAfterName => 3;
// stateBeforeValue occurs after the equals sign but before the value.
// It occurs between the ^'s in ` name =^ ^value`.
internal static state stateBeforeValue => 4;
// stateHTMLCmt occurs inside an <!-- HTML comment -->.
internal static state stateHTMLCmt => 5;
// stateRCDATA occurs inside an RCDATA element (<textarea> or <title>)
// as described at https://www.w3.org/TR/html5/syntax.html#elements-0
internal static state stateRCDATA => 6;
// stateAttr occurs inside an HTML attribute whose content is text.
internal static state stateAttr => 7;
// stateURL occurs inside an HTML attribute whose content is a URL.
internal static state stateURL => 8;
// stateSrcset occurs inside an HTML srcset attribute.
internal static state stateSrcset => 9;
// stateJS occurs inside an event handler or script element.
internal static state stateJS => 10;
// stateJSDqStr occurs inside a JavaScript double quoted string.
internal static state stateJSDqStr => 11;
// stateJSSqStr occurs inside a JavaScript single quoted string.
internal static state stateJSSqStr => 12;
// stateJSTmplLit occurs inside a JavaScript back quoted string.
internal static state stateJSTmplLit => 13;
// stateJSRegexp occurs inside a JavaScript regexp literal.
internal static state stateJSRegexp => 14;
// stateJSBlockCmt occurs inside a JavaScript /* block comment */.
internal static state stateJSBlockCmt => 15;
// stateJSLineCmt occurs inside a JavaScript // line comment.
internal static state stateJSLineCmt => 16;
// stateJSHTMLOpenCmt occurs inside a JavaScript <!-- HTML-like comment.
internal static state stateJSHTMLOpenCmt => 17;
// stateJSHTMLCloseCmt occurs inside a JavaScript --> HTML-like comment.
internal static state stateJSHTMLCloseCmt => 18;
// stateCSS occurs inside a <style> element or style attribute.
internal static state stateCSS => 19;
// stateCSSDqStr occurs inside a CSS double quoted string.
internal static state stateCSSDqStr => 20;
// stateCSSSqStr occurs inside a CSS single quoted string.
internal static state stateCSSSqStr => 21;
// stateCSSDqURL occurs inside a CSS double quoted url("...").
internal static state stateCSSDqURL => 22;
// stateCSSSqURL occurs inside a CSS single quoted url('...').
internal static state stateCSSSqURL => 23;
// stateCSSURL occurs inside a CSS unquoted url(...).
internal static state stateCSSURL => 24;
// stateCSSBlockCmt occurs inside a CSS /* block comment */.
internal static state stateCSSBlockCmt => 25;
// stateCSSLineCmt occurs inside a CSS // line comment.
internal static state stateCSSLineCmt => 26;
// stateError is an infectious error state outside any valid
// HTML/CSS/JS construct.
internal static state stateError => 27;
// stateMetaContent occurs inside a HTML meta element content attribute.
internal static state stateMetaContent => 28;
// stateMetaContentURL occurs inside a "url=" tag in a HTML meta element content attribute.
internal static state stateMetaContentURL => 29;
// stateDead marks unreachable code after a {{break}} or {{continue}}.
internal static state stateDead => 30;

// isComment is true for any state that contains content meant for template
// authors & maintainers, not for end-users or machines.
internal static bool isComment(state s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == stateHTMLCmt || exprᴛ1 == stateJSBlockCmt || exprᴛ1 == stateJSLineCmt || exprᴛ1 == stateJSHTMLOpenCmt || exprᴛ1 == stateJSHTMLCloseCmt || exprᴛ1 == stateCSSBlockCmt || exprᴛ1 == stateCSSLineCmt) {
        return true;
    }
    return false;
}

// isInTag return whether s occurs solely inside an HTML tag.
internal static bool isInTag(state s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == stateTag || exprᴛ1 == stateAttrName || exprᴛ1 == stateAfterName || exprᴛ1 == stateBeforeValue || exprᴛ1 == stateAttr) {
        return true;
    }
    return false;
}

// isInScriptLiteral returns true if s is one of the literal states within a
// <script> tag, and as such occurrences of "<!--", "<script", and "</script"
// need to be treated specially.
internal static bool isInScriptLiteral(state s) {
    // Ignore the comment states (stateJSBlockCmt, stateJSLineCmt,
    // stateJSHTMLOpenCmt, stateJSHTMLCloseCmt) because their content is already
    // omitted from the output.
    var exprᴛ1 = s;
    if (exprᴛ1 == stateJSDqStr || exprᴛ1 == stateJSSqStr || exprᴛ1 == stateJSTmplLit || exprᴛ1 == stateJSRegexp) {
        return true;
    }
    return false;
}

// delim is the delimiter that will end the current HTML attribute.
partial struct delim /*num:uint8*/;

//go:generate stringer -type delim

// delimNone occurs outside any attribute.
internal static delim delimNone => /* iota */ 0;
// delimDoubleQuote occurs when a double quote (") closes the attribute.
internal static delim delimDoubleQuote => 1;
// delimSingleQuote occurs when a single quote (') closes the attribute.
internal static delim delimSingleQuote => 2;
// delimSpaceOrTagEnd occurs when a space or right angle bracket (>)
// closes the attribute.
internal static delim delimSpaceOrTagEnd => 3;

// urlPart identifies a part in an RFC 3986 hierarchical URL to allow different
// encoding strategies.
partial struct urlPart /*num:uint8*/;

//go:generate stringer -type urlPart

// urlPartNone occurs when not in a URL, or possibly at the start:
// ^ in "^http://auth/path?k=v#frag".
internal static urlPart urlPartNone => /* iota */ 0;
// urlPartPreQuery occurs in the scheme, authority, or path; between the
// ^s in "h^ttp://auth/path^?k=v#frag".
internal static urlPart urlPartPreQuery => 1;
// urlPartQueryOrFrag occurs in the query portion between the ^s in
// "http://auth/path?^k=v#frag^".
internal static urlPart urlPartQueryOrFrag => 2;
// urlPartUnknown occurs due to joining of contexts both before and
// after the query separator.
internal static urlPart urlPartUnknown => 3;

// jsCtx determines whether a '/' starts a regular expression literal or a
// division operator.
partial struct jsCtx /*num:uint8*/;

//go:generate stringer -type jsCtx

// jsCtxRegexp occurs where a '/' would start a regexp literal.
internal static jsCtx jsCtxRegexp => /* iota */ 0;
// jsCtxDivOp occurs where a '/' would start a division operator.
internal static jsCtx jsCtxDivOp => 1;
// jsCtxUnknown occurs where a '/' is ambiguous due to context joining.
internal static jsCtx jsCtxUnknown => 2;

// element identifies the HTML element when inside a start tag or special body.
// Certain HTML element (for example <script> and <style>) have bodies that are
// treated differently from stateText so the element type is necessary to
// transition into the correct context at the end of a tag and to identify the
// end delimiter for the body.
partial struct element /*num:uint8*/;

//go:generate stringer -type element

// elementNone occurs outside a special tag or special element body.
internal static element elementNone => /* iota */ 0;
// elementScript corresponds to the raw text <script> element
// with JS MIME type or no type attribute.
internal static element elementScript => 1;
// elementStyle corresponds to the raw text <style> element.
internal static element elementStyle => 2;
// elementTextarea corresponds to the RCDATA <textarea> element.
internal static element elementTextarea => 3;
// elementTitle corresponds to the RCDATA <title> element.
internal static element elementTitle => 4;
// elementMeta corresponds to the HTML <meta> element.
internal static element elementMeta => 5;

//go:generate stringer -type attr

// attr identifies the current HTML attribute when inside the attribute,
// that is, starting from stateAttrName until stateTag/stateText (exclusive).
partial struct attr /*num:uint8*/;

// attrNone corresponds to a normal attribute or no attribute.
internal static attr attrNone => /* iota */ 0;
// attrScript corresponds to an event handler attribute.
internal static attr attrScript => 1;
// attrScriptType corresponds to the type attribute in script HTML element
internal static attr attrScriptType => 2;
// attrStyle corresponds to the style attribute whose value is CSS.
internal static attr attrStyle => 3;
// attrURL corresponds to an attribute whose value is a URL.
internal static attr attrURL => 4;
// attrSrcset corresponds to a srcset attribute.
internal static attr attrSrcset => 5;
// attrMetaContent corresponds to the content attribute in meta HTML element.
internal static attr attrMetaContent => 6;

} // end template_package
