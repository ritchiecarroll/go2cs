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
internal static slice<oldHeadingTestsᴛ1> oldHeadingTests = slice<oldHeadingTestsᴛ1>([
    new("Section", true),
    new("A typical usage", true),
    new("ΔΛΞ is Greek", true),
    new("Foo 42", true),
    new("", false),
    new("section", false),
    new("A typical usage:", false),
    new("This code:", false),
    new("δ is Greek", false),
    new("Foo §", false),
    new("Fermat's Last Sentence", true),
    new("Fermat's", true),
    new("'sX", false),
    new("Ted 'Too' Bar", false),
    new("Use n+m", false),
    new("Scanning:", false),
    new("N:M", false)
]);

public static void TestIsOldHeading(ж<testing.T> t) {
    foreach (var (_, tt) in oldHeadingTests) {
        if (isOldHeading(tt.line, slice<@string>(["Text.", "", tt.line, "", "Text."]), 2) != tt.ok) {
            t.Errorf("isOldHeading(%q) = %v, want %v", tt.line, !tt.ok, tt.ok);
        }
    }
}

partial struct autoURLTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
internal static slice<autoURLTestsᴛ1> autoURLTests = slice<autoURLTestsᴛ1>([
    new("", ""),
    new("http://[::1]:8080/foo.txt", "http://[::1]:8080/foo.txt"),
    new("https://www.google.com) after", "https://www.google.com"),
    new("https://www.google.com:30/x/y/z:b::c. After", "https://www.google.com:30/x/y/z:b::c"),
    new("http://www.google.com/path/:;!-/?query=%34b#093124", "http://www.google.com/path/:;!-/?query=%34b#093124"),
    new("http://www.google.com/path/:;!-/?query=%34bar#093124", "http://www.google.com/path/:;!-/?query=%34bar#093124"),
    new("http://www.google.com/index.html! After", "http://www.google.com/index.html"),
    new("http://www.google.com/", "http://www.google.com/"),
    new("https://www.google.com/", "https://www.google.com/"),
    new("http://www.google.com/path.", "http://www.google.com/path"),
    new("http://en.wikipedia.org/wiki/Camellia_(cipher)", "http://en.wikipedia.org/wiki/Camellia_(cipher)"),
    new("http://www.google.com/)", "http://www.google.com/"),
    new("http://gmail.com)", "http://gmail.com"),
    new("http://gmail.com))", "http://gmail.com"),
    new("http://gmail.com ((http://gmail.com)) ()", "http://gmail.com"),
    new("http://example.com/ quux!", "http://example.com/"),
    new("http://example.com/%2f/ /world.", "http://example.com/%2f/"),
    new("http: ipsum //host/path", ""),
    new("javascript://is/not/linked", ""),
    new("http://foo", "http://foo"),
    new("https://www.example.com/person/][Person Name]]", "https://www.example.com/person/"),
    new("http://golang.org/)", "http://golang.org/"),
    new("http://golang.org/hello())", "http://golang.org/hello()"),
    new("http://git.qemu.org/?p=qemu.git;a=blob;f=qapi-schema.json;hb=HEAD", "http://git.qemu.org/?p=qemu.git;a=blob;f=qapi-schema.json;hb=HEAD"),
    new("https://foo.bar/bal/x(])", "https://foo.bar/bal/x"), // inner ] causes (]) to be cut off from URL
    new("http://bar(])", "http://bar")                        // same
]);

public static void TestAutoURL(ж<testing.T> t) {
    foreach (var (_, tt) in autoURLTests) {
        var (url, ok) = autoURL(tt.@in);
        if (url != tt.@out || ok != (tt.@out != ""u8)) {
            t.Errorf("autoURL(%q) = %q, %v, want %q, %v", tt.@in, url, ok, tt.@out, tt.@out != ""u8);
        }
    }
}

} // end comment_internal_test_package
