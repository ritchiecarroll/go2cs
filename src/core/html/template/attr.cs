// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using strings = strings_package;

partial class template_package {

// attrTypeMap[n] describes the value of the given attribute.
// If an attribute affects (or can mask) the encoding or interpretation of
// other content, or affects the contents, idempotency, or credentials of a
// network message, then the value in this map is contentTypeUnsafe.
// This map is derived from HTML5, specifically
// https://www.w3.org/TR/html5/Overview.html#attributes-1
// as well as "%URI"-typed attributes from
// https://www.w3.org/TR/html4/index/attributes.html
[GoLazy] partial class lazyᴛattrTypeMap { internal static map<@string, contentType> attrTypeMap = new map<@string, contentType>{
    ["accept"u8] = contentTypePlain,
    ["accept-charset"u8] = contentTypeUnsafe,
    ["action"u8] = contentTypeURL,
    ["alt"u8] = contentTypePlain,
    ["archive"u8] = contentTypeURL,
    ["async"u8] = contentTypeUnsafe,
    ["autocomplete"u8] = contentTypePlain,
    ["autofocus"u8] = contentTypePlain,
    ["autoplay"u8] = contentTypePlain,
    ["background"u8] = contentTypeURL,
    ["border"u8] = contentTypePlain,
    ["checked"u8] = contentTypePlain,
    ["cite"u8] = contentTypeURL,
    ["challenge"u8] = contentTypeUnsafe,
    ["charset"u8] = contentTypeUnsafe,
    ["class"u8] = contentTypePlain,
    ["classid"u8] = contentTypeURL,
    ["codebase"u8] = contentTypeURL,
    ["cols"u8] = contentTypePlain,
    ["colspan"u8] = contentTypePlain,
    ["content"u8] = contentTypeUnsafe,
    ["contenteditable"u8] = contentTypePlain,
    ["contextmenu"u8] = contentTypePlain,
    ["controls"u8] = contentTypePlain,
    ["coords"u8] = contentTypePlain,
    ["crossorigin"u8] = contentTypeUnsafe,
    ["data"u8] = contentTypeURL,
    ["datetime"u8] = contentTypePlain,
    ["default"u8] = contentTypePlain,
    ["defer"u8] = contentTypeUnsafe,
    ["dir"u8] = contentTypePlain,
    ["dirname"u8] = contentTypePlain,
    ["disabled"u8] = contentTypePlain,
    ["draggable"u8] = contentTypePlain,
    ["dropzone"u8] = contentTypePlain,
    ["enctype"u8] = contentTypeUnsafe,
    ["for"u8] = contentTypePlain,
    ["form"u8] = contentTypeUnsafe,
    ["formaction"u8] = contentTypeURL,
    ["formenctype"u8] = contentTypeUnsafe,
    ["formmethod"u8] = contentTypeUnsafe,
    ["formnovalidate"u8] = contentTypeUnsafe,
    ["formtarget"u8] = contentTypePlain,
    ["headers"u8] = contentTypePlain,
    ["height"u8] = contentTypePlain,
    ["hidden"u8] = contentTypePlain,
    ["high"u8] = contentTypePlain,
    ["href"u8] = contentTypeURL,
    ["hreflang"u8] = contentTypePlain,
    ["http-equiv"u8] = contentTypeUnsafe,
    ["icon"u8] = contentTypeURL,
    ["id"u8] = contentTypePlain,
    ["ismap"u8] = contentTypePlain,
    ["keytype"u8] = contentTypeUnsafe,
    ["kind"u8] = contentTypePlain,
    ["label"u8] = contentTypePlain,
    ["lang"u8] = contentTypePlain,
    ["language"u8] = contentTypeUnsafe,
    ["list"u8] = contentTypePlain,
    ["longdesc"u8] = contentTypeURL,
    ["loop"u8] = contentTypePlain,
    ["low"u8] = contentTypePlain,
    ["manifest"u8] = contentTypeURL,
    ["max"u8] = contentTypePlain,
    ["maxlength"u8] = contentTypePlain,
    ["media"u8] = contentTypePlain,
    ["mediagroup"u8] = contentTypePlain,
    ["method"u8] = contentTypeUnsafe,
    ["min"u8] = contentTypePlain,
    ["multiple"u8] = contentTypePlain,
    ["name"u8] = contentTypePlain,
    ["novalidate"u8] = contentTypeUnsafe,
    // Skip handler names from
    // https://www.w3.org/TR/html5/webappapis.html#event-handlers-on-elements,-document-objects,-and-window-objects
    // since we have special handling in attrType.
    ["open"u8] = contentTypePlain,
    ["optimum"u8] = contentTypePlain,
    ["pattern"u8] = contentTypeUnsafe,
    ["placeholder"u8] = contentTypePlain,
    ["poster"u8] = contentTypeURL,
    ["profile"u8] = contentTypeURL,
    ["preload"u8] = contentTypePlain,
    ["pubdate"u8] = contentTypePlain,
    ["radiogroup"u8] = contentTypePlain,
    ["readonly"u8] = contentTypePlain,
    ["rel"u8] = contentTypeUnsafe,
    ["required"u8] = contentTypePlain,
    ["reversed"u8] = contentTypePlain,
    ["rows"u8] = contentTypePlain,
    ["rowspan"u8] = contentTypePlain,
    ["sandbox"u8] = contentTypeUnsafe,
    ["spellcheck"u8] = contentTypePlain,
    ["scope"u8] = contentTypePlain,
    ["scoped"u8] = contentTypePlain,
    ["seamless"u8] = contentTypePlain,
    ["selected"u8] = contentTypePlain,
    ["shape"u8] = contentTypePlain,
    ["size"u8] = contentTypePlain,
    ["sizes"u8] = contentTypePlain,
    ["span"u8] = contentTypePlain,
    ["src"u8] = contentTypeURL,
    ["srcdoc"u8] = contentTypeHTML,
    ["srclang"u8] = contentTypePlain,
    ["srcset"u8] = contentTypeSrcset,
    ["start"u8] = contentTypePlain,
    ["step"u8] = contentTypePlain,
    ["style"u8] = contentTypeCSS,
    ["tabindex"u8] = contentTypePlain,
    ["target"u8] = contentTypePlain,
    ["title"u8] = contentTypePlain,
    ["type"u8] = contentTypeUnsafe,
    ["usemap"u8] = contentTypeURL,
    ["value"u8] = contentTypeUnsafe,
    ["width"u8] = contentTypePlain,
    ["wrap"u8] = contentTypePlain,
    ["xmlns"u8] = contentTypeURL
}; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dataˢ = "data-"u8;
internal static readonly @string srcˢ = "src"u8;
internal static readonly @string uriˢ = "uri"u8;
internal static readonly @string urlˢ = "url"u8;

// attrType returns a conservative (upper-bound on authority) guess at the
// type of the lowercase named attribute.
internal static contentType attrType(@string name) {
    if (strings.HasPrefix(name, dataˢ)){
        // Strip data- so that custom attribute heuristics below are
        // widely applied.
        // Treat data-action as URL below.
        name = name[5..];
    } else 
    {
        var (prefix, @short, ok) = strings.Cut(name, ":"u8); if (ok) {
            if (prefix == "xmlns"u8) {
                return contentTypeURL;
            }
            // Treat svg:href and xlink:href as href below.
            name = @short;
        }
    }
    {
        var (t, ok) = attrTypeMap[name, ꟷ]; if (ok) {
            return t;
        }
    }
    // Treat partial event handler names as script.
    if (strings.HasPrefix(name, "on"u8)) {
        return contentTypeJS;
    }

    // Heuristics to prevent "javascript:..." injection in custom
    // data attributes and custom attributes like g:tweetUrl.
    // https://www.w3.org/TR/html5/dom.html#embedding-custom-non-visible-data-with-the-data-*-attributes
    // "Custom data attributes are intended to store custom data
    //  private to the page or application, for which there are no
    //  more appropriate attributes or elements."
    // Developers seem to store URL content in data URLs that start
    // or end with "URI" or "URL".
    if (strings.Contains(name, srcˢ) || strings.Contains(name, uriˢ) || strings.Contains(name, urlˢ)) {
        return contentTypeURL;
    }
    return contentTypePlain;
}

} // end template_package
