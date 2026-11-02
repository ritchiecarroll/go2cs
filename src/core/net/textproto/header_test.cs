// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using testing = testing_package;
using static go.net.textproto_package;

partial class textproto_internal_test_package {

internal partial struct canonicalHeaderKeyTest {
    internal @string @in, @out;
}

internal static slice<canonicalHeaderKeyTest> canonicalHeaderKeyTests = slice<canonicalHeaderKeyTest>([
    new("a-b-c", "A-B-C"),
    new("a-1-c", "A-1-C"),
    new("User-Agent", "User-Agent"),
    new("uSER-aGENT", "User-Agent"),
    new("user-agent", "User-Agent"),
    new("USER-AGENT", "User-Agent"),

    // Other valid tchar bytes in tokens:
    new("foo-bar_baz", "Foo-Bar_baz"),
    new("foo-bar$baz", "Foo-Bar$baz"),
    new("foo-bar~baz", "Foo-Bar~baz"),
    new("foo-bar*baz", "Foo-Bar*baz"),

    // Non-ASCII or anything with spaces or non-token chars is unchanged:
    new("üser-agenT", "üser-agenT"),
    new("a B", "a B"),

    // This caused a panic due to mishandling of a space:
    new("C Ontent-Transfer-Encoding", "C Ontent-Transfer-Encoding"),
    new("foo bar", "foo bar")
]);

public static void TestCanonicalMIMEHeaderKey(ж<testing.T> t) {
    foreach (var (_, tt) in canonicalHeaderKeyTests) {
        {
            @string s = CanonicalMIMEHeaderKey(tt.@in); if (s != tt.@out) {
                t.Errorf("CanonicalMIMEHeaderKey(%q) = %q, want %q", tt.@in, s, tt.@out);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string setCookieˢ => ref setCookieˢᵇ;
private static readonly @string setCookieˢᵇ = "set-cookie"u8;

// Issue #34799 add a Header method to get multiple values []string, with canonicalized key
public static void TestMIMEHeaderMultipleValues(ж<testing.T> t) {
    var testHeader = new MIMEHeader(new map<@string, slice<@string>>{
        ["Set-Cookie"] = slice<@string>(["cookie 1", "cookie 2"])
    });
    var values = testHeader.Values(setCookieˢ);
    nint n = len(values);
    if (n != 2) {
        t.Errorf("count: %d; want 2", n);
    }
}

} // end textproto_internal_test_package
