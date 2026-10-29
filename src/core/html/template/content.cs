// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using fmt = fmt_package;
using reflect = reflect_package;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

// Strings of content from a trusted source.
// CSS encapsulates known safe content that matches any of:
//   1. The CSS3 stylesheet production, such as `p { color: purple }`.
//   2. The CSS3 rule production, such as `a[href=~"https:"].foo#bar`.
//   3. CSS3 declaration productions, such as `color: red; margin: 2px`.
//   4. The CSS3 value production, such as `rgba(0, 0, 255, 127)`.
// See https://www.w3.org/TR/css3-syntax/#parsing and
// https://web.archive.org/web/20090211114933/http://w3.org/TR/css3-syntax#style
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
partial struct CSS /*@string*/;

// HTML encapsulates a known safe HTML document fragment.
// It should not be used for HTML from a third-party, or HTML with
// unclosed tags or comments. The outputs of a sound HTML sanitizer
// and a template escaped by this package are fine for use with HTML.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
partial struct HTML /*@string*/;

// HTMLAttr encapsulates an HTML attribute from a trusted source,
// for example, ` dir="ltr"`.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
partial struct HTMLAttr /*@string*/;

// JS encapsulates a known safe EcmaScript5 Expression, for example,
// `(x + y * z())`.
// Template authors are responsible for ensuring that typed expressions
// do not break the intended precedence and that there is no
// statement/expression ambiguity as when passing an expression like
// "{ foo: bar() }\n['foo']()", which is both a valid Expression and a
// valid Program with a very different meaning.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
//
// Using JS to include valid but untrusted JSON is not safe.
// A safe alternative is to parse the JSON with json.Unmarshal and then
// pass the resultant object into the template, where it will be
// converted to sanitized JSON when presented in a JavaScript context.
partial struct JS /*@string*/;

// JSStr encapsulates a sequence of characters meant to be embedded
// between quotes in a JavaScript expression.
// The string must match a series of StringCharacters:
//   StringCharacter :: SourceCharacter but not `\` or LineTerminator
//                    | EscapeSequence
// Note that LineContinuations are not allowed.
// JSStr("foo\\nbar") is fine, but JSStr("foo\\\nbar") is not.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
partial struct JSStr /*@string*/;

// URL encapsulates a known safe URL or URL substring (see RFC 3986).
// A URL like `javascript:checkThatFormNotEditedBeforeLeavingPage()`
// from a trusted source should go in the page, but by default dynamic
// `javascript:` URLs are filtered out since they are a frequently
// exploited injection vector.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
partial struct URL /*@string*/;

// Srcset encapsulates a known safe srcset attribute
// (see https://w3c.github.io/html/semantics-embedded-content.html#element-attrdef-img-srcset).
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
partial struct Srcset /*@string*/;

partial struct contentType /*num:uint8*/;

internal static contentType contentTypePlain => /* iota */ 0;
internal static contentType contentTypeCSS => 1;
internal static contentType contentTypeHTML => 2;
internal static contentType contentTypeHTMLAttr => 3;
internal static contentType contentTypeJS => 4;
internal static contentType contentTypeJSStr => 5;
internal static contentType contentTypeURL => 6;
internal static contentType contentTypeSrcset => 7;
// contentTypeUnsafe is used in attr.go for values that affect how
// embedded content and network messages are formed, vetted,
// or interpreted; or which credentials network messages carry.
internal static contentType contentTypeUnsafe => 8;

// indirect returns the value, after dereferencing as many times
// as necessary to reach the base type (or nil).
internal static any indirect(any a) {
    if (a == nil) {
        return default;
    }
    {
        var t = reflect.TypeOf(a); if (t.Kind() != reflect.ΔPointer) {
            // Avoid creating a reflect.Value if it's not a pointer.
            return a;
        }
    }
    var v = reflect.ValueOf(a);
    while (v.Kind() == reflect.ΔPointer && !v.IsNil()) {
        v = v.Elem();
    }
    return v.Interface();
}

internal static reflectꓸType errorType = reflect.TypeFor<error>();
internal static reflectꓸType fmtStringerType = reflect.TypeFor<fmt.Stringer>();

// indirectToStringerOrError returns the value, after dereferencing as many times
// as necessary to reach the base type (or nil) or an implementation of fmt.Stringer
// or error.
internal static any indirectToStringerOrError(any a) {
    if (a == nil) {
        return default;
    }
    var v = reflect.ValueOf(a);
    while (!v.Type().Implements(fmtStringerType) && !v.Type().Implements(errorType) && v.Kind() == reflect.ΔPointer && !v.IsNil()) {
        v = v.Elem();
    }
    return v.Interface();
}

// stringify converts its arguments to a string and the type of the content.
// All pointers are dereferenced, as in the text/template package.
internal static (@string, contentType) stringify(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    if (len(args) == 1) {
        switch (indirect(args[0]).type()) {
        case @string s: {
            return (s, contentTypePlain);
        }
        case CSS s: {
            return ((@string)s, contentTypeCSS);
        }
        case HTML s: {
            return ((@string)s, contentTypeHTML);
        }
        case HTMLAttr s: {
            return ((@string)s, contentTypeHTMLAttr);
        }
        case JS s: {
            return ((@string)s, contentTypeJS);
        }
        case JSStr s: {
            return ((@string)s, contentTypeJSStr);
        }
        case URL s: {
            return ((@string)s, contentTypeURL);
        }
        case Srcset s: {
            return ((@string)s, contentTypeSrcset);
        }}
    }
    nint i = 0;
    foreach (var (_, arg) in args) {
        // We skip untyped nil arguments for backward compatibility.
        // Without this they would be output as <nil>, escaped.
        // See issue 25875.
        if (arg == nil) {
            continue;
        }

        args[i] = indirectToStringerOrError(arg);
        i++;
    }
    return (fmt.Sprint(args.slice(0, i).ꓸꓸꓸ), contentTypePlain);
}

} // end template_package
