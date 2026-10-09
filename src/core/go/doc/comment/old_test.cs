// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// These tests are carried forward from the old go/doc implementation.
namespace go.go.doc;

using testing = testing_package;
using static global::go.go.doc.comment_package;

partial class comment_internal_test_package {


partial struct oldHeadingTestsᴛ1 /*dyn*/ {
    internal @string line;
    internal bool ok;
}
internal static slice<oldHeadingTestsᴛ1> oldHeadingTests = new oldHeadingTestsᴛ1[]{
    new("Section"u8, true),
    new("A typical usage"u8, true),
    new("ΔΛΞ is Greek"u8, true),
    new("Foo 42"u8, true),
    new(""u8, false),
    new("section"u8, false),
    new("A typical usage:"u8, false),
    new("This code:"u8, false),
    new("δ is Greek"u8, false),
    new("Foo §"u8, false),
    new("Fermat's Last Sentence"u8, true),
    new("Fermat's"u8, true),
    new("'sX"u8, false),
    new("Ted 'Too' Bar"u8, false),
    new("Use n+m"u8, false),
    new("Scanning:"u8, false),
    new("N:M"u8, false)
}.slice();

public static void TestIsOldHeading(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in oldHeadingTests) {
        if (isOldHeading(tt.line, new @string[]{"Text."u8, ""u8, tt.line, ""u8, "Text."u8}.slice(), 2) != tt.ok) {
            Ꮡt.Errorf("isOldHeading(%q) = %v, want %v"u8, tt.line, !tt.ok, tt.ok);
        }
    }
}

// inner ] causes (]) to be cut off from URL
// same

partial struct autoURLTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
internal static slice<autoURLTestsᴛ1> autoURLTests = new autoURLTestsᴛ1[]{
    new(""u8, ""u8),
    new("http://[::1]:8080/foo.txt"u8, "http://[::1]:8080/foo.txt"u8),
    new("https://www.google.com) after"u8, "https://www.google.com"u8),
    new("https://www.google.com:30/x/y/z:b::c. After"u8, "https://www.google.com:30/x/y/z:b::c"u8),
    new("http://www.google.com/path/:;!-/?query=%34b#093124"u8, "http://www.google.com/path/:;!-/?query=%34b#093124"u8),
    new("http://www.google.com/path/:;!-/?query=%34bar#093124"u8, "http://www.google.com/path/:;!-/?query=%34bar#093124"u8),
    new("http://www.google.com/index.html! After"u8, "http://www.google.com/index.html"u8),
    new("http://www.google.com/"u8, "http://www.google.com/"u8),
    new("https://www.google.com/"u8, "https://www.google.com/"u8),
    new("http://www.google.com/path."u8, "http://www.google.com/path"u8),
    new("http://en.wikipedia.org/wiki/Camellia_(cipher)"u8, "http://en.wikipedia.org/wiki/Camellia_(cipher)"u8),
    new("http://www.google.com/)"u8, "http://www.google.com/"u8),
    new("http://gmail.com)"u8, "http://gmail.com"u8),
    new("http://gmail.com))"u8, "http://gmail.com"u8),
    new("http://gmail.com ((http://gmail.com)) ()"u8, "http://gmail.com"u8),
    new("http://example.com/ quux!"u8, "http://example.com/"u8),
    new("http://example.com/%2f/ /world."u8, "http://example.com/%2f/"u8),
    new("http: ipsum //host/path"u8, ""u8),
    new("javascript://is/not/linked"u8, ""u8),
    new("http://foo"u8, "http://foo"u8),
    new("https://www.example.com/person/][Person Name]]"u8, "https://www.example.com/person/"u8),
    new("http://golang.org/)"u8, "http://golang.org/"u8),
    new("http://golang.org/hello())"u8, "http://golang.org/hello()"u8),
    new("http://git.qemu.org/?p=qemu.git;a=blob;f=qapi-schema.json;hb=HEAD"u8, "http://git.qemu.org/?p=qemu.git;a=blob;f=qapi-schema.json;hb=HEAD"u8),
    new("https://foo.bar/bal/x(])"u8, "https://foo.bar/bal/x"u8),
    new("http://bar(])"u8, "http://bar"u8)
}.slice();

public static void TestAutoURL(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in autoURLTests) {
        var (url, ok) = autoURL(tt.@in);
        if (url != tt.@out || ok != (tt.@out != ""u8)) {
            Ꮡt.Errorf("autoURL(%q) = %q, %v, want %q, %v"u8, tt.@in, url, ok, tt.@out, tt.@out != ""u8);
        }
    }
}

} // end comment_internal_test_package
