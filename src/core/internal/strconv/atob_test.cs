// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using static go.@internal.strconv_package;
using testing = testing_package;
using static go.@internal.strconv_internal_test_package;

partial class strconv_test_package {

partial struct atobTest {
    internal @string @in;
    internal bool @out;
    internal error err;
}

internal static slice<atobTest> atobtests = [
    new("", false, ErrSyntax),
    new("asdf", false, ErrSyntax),
    new("0", false, default),
    new("f", false, default),
    new("F", false, default),
    new("FALSE", false, default),
    new("false", false, default),
    new("False", false, default),
    new("1", true, default),
    new("t", true, default),
    new("T", true, default),
    new("TRUE", true, default),
    new("true", true, default),
    new("True", true, default)
];

public static void TestParseBool(ж<testing.T> t) {
    foreach (var (_, test) in range(atobtests)) {
        var (b, e) = ParseBool(test.@in);
        if (b != test.@out || !AreEqual(e, test.err)) {
            t.Errorf("ParseBool(%s) = %v, %v, want %v, %v", test.@in, b, e, test.@out, test.err);
        }
    }
}

internal static map<bool, @string> boolString = new map<bool, @string>{
    [true] = "true",
    [false] = "false"
};

public static void TestFormatBool(ж<testing.T> t) {
    foreach (var (b, s) in boolString) {
        @string f = FormatBool(b); if (f != s) {
            t.Errorf("FormatBool(%v) = %q; want %q", b, f, s);
        }
    }
}

partial struct appendBoolTest {
    internal bool b;
    internal slice<byte> @in;
    internal slice<byte> @out;
}

internal static slice<appendBoolTest> appendBoolTests = [
    new(true, slice<byte>("foo "u8), slice<byte>("foo true"u8)),
    new(false, slice<byte>("foo "u8), slice<byte>("foo false"u8))
];

public static void TestAppendBool(ж<testing.T> t) {
    foreach (var (_, test) in range(appendBoolTests)) {
        var b = AppendBool(test.@in, test.b);
        if (!bytes.Equal(b, test.@out)) {
            t.Errorf("AppendBool(%q, %v) = %q; want %q", test.@in, test.b, b, test.@out);
        }
    }
}

} // end strconv_test_package
