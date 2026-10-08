// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using fmt = fmt_package;
using reflect = reflect_package;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

partial struct CSS /*@string*/;

partial struct HTML /*@string*/;

partial struct HTMLAttr /*@string*/;

partial struct JS /*@string*/;

partial struct JSStr /*@string*/;

partial struct URL /*@string*/;

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
internal static contentType contentTypeUnsafe => 8;

// indirect returns the value, after dereferencing as many times
// as necessary to reach the base type (or nil).
internal static any indirect(any a) {
    if (a == default!) {
        return default!;
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
    if (a == default!) {
        return default!;
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
            return (((@string)s), contentTypeCSS);
        }
        case HTML s: {
            return (((@string)s), contentTypeHTML);
        }
        case HTMLAttr s: {
            return (((@string)s), contentTypeHTMLAttr);
        }
        case JS s: {
            return (((@string)s), contentTypeJS);
        }
        case JSStr s: {
            return (((@string)s), contentTypeJSStr);
        }
        case URL s: {
            return (((@string)s), contentTypeURL);
        }
        case Srcset s: {
            return (((@string)s), contentTypeSrcset);
        }}
    }
    nint i = 0;
    foreach (var (_, arg) in args) {
        // We skip untyped nil arguments for backward compatibility.
        // Without this they would be output as <nil>, escaped.
        // See issue 25875.
        if (arg == default!) {
            continue;
        }
        args[i] = indirectToStringerOrError(arg);
        i++;
    }
    return (fmt.Sprint(args.slice(0, i).ꓸꓸꓸ), contentTypePlain);
}

} // end template_package
