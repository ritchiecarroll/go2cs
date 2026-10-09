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

// Other valid tchar bytes in tokens:
// Non-ASCII or anything with spaces or non-token chars is unchanged:
// This caused a panic due to mishandling of a space:
internal static slice<canonicalHeaderKeyTest> canonicalHeaderKeyTests = new canonicalHeaderKeyTest[]{
    new("a-b-c"u8, "A-B-C"u8),
    new("a-1-c"u8, "A-1-C"u8),
    new("User-Agent"u8, "User-Agent"u8),
    new("uSER-aGENT"u8, "User-Agent"u8),
    new("user-agent"u8, "User-Agent"u8),
    new("USER-AGENT"u8, "User-Agent"u8),
    new("foo-bar_baz"u8, "Foo-Bar_baz"u8),
    new("foo-bar$baz"u8, "Foo-Bar$baz"u8),
    new("foo-bar~baz"u8, "Foo-Bar~baz"u8),
    new("foo-bar*baz"u8, "Foo-Bar*baz"u8),
    new("üser-agenT"u8, "üser-agenT"u8),
    new("a B"u8, "a B"u8),
    new("C Ontent-Transfer-Encoding"u8, "C Ontent-Transfer-Encoding"u8),
    new("foo bar"u8, "foo bar"u8)
}.slice();

public static void TestCanonicalMIMEHeaderKey(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in canonicalHeaderKeyTests) {
        {
            @string s = CanonicalMIMEHeaderKey(tt.@in); if (s != tt.@out) {
                Ꮡt.Errorf("CanonicalMIMEHeaderKey(%q) = %q, want %q"u8, tt.@in, s, tt.@out);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setCookieˢ = "set-cookie"u8;

// Issue #34799 add a Header method to get multiple values []string, with canonicalized key
public static void TestMIMEHeaderMultipleValues(ж<testing.T> Ꮡt) {
    var testHeader = new MIMEHeader(new map<@string, slice<@string>>{
        ["Set-Cookie"u8] = new @string[]{"cookie 1"u8, "cookie 2"u8}.slice()
    });
    var values = testHeader.Values(setCookieˢ);
    nint n = len(values);
    if (n != 2) {
        Ꮡt.Errorf("count: %d; want 2"u8, n);
    }
}

} // end textproto_internal_test_package
