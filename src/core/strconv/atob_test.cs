// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using static strconv_package;
using testing = testing_package;
using static go.strconv_internal_test_package;
using strconv = strconv_package;

partial class strconv_test_package {

partial struct atobTest {
    internal @string @in;
    internal bool @out;
    internal error err;
}

internal static slice<atobTest> atobtests = new atobTest[]{
    new(""u8, false, ErrSyntax),
    new("asdf"u8, false, ErrSyntax),
    new("0"u8, false, default!),
    new("f"u8, false, default!),
    new("F"u8, false, default!),
    new("FALSE"u8, false, default!),
    new("false"u8, false, default!),
    new("False"u8, false, default!),
    new("1"u8, true, default!),
    new("t"u8, true, default!),
    new("T"u8, true, default!),
    new("TRUE"u8, true, default!),
    new("true"u8, true, default!),
    new("True"u8, true, default!)
}.slice();

public static void TestParseBool(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in atobtests) {
        var (b, e) = ParseBool(test.@in);
        if (test.err != default!){
            // expect an error
            if (e == default!){
                Ꮡt.Errorf("ParseBool(%s) = nil; want %s"u8, test.@in, test.err);
            } else {
                // NumError assertion must succeed; it's the only thing we return.
                if (!AreEqual((~e._<ж<strconv.NumError>>()).Err, test.err)) {
                    Ꮡt.Errorf("ParseBool(%s) = %s; want %s"u8, test.@in, e, test.err);
                }
            }
        } else {
            if (e != default!) {
                Ꮡt.Errorf("ParseBool(%s) = %s; want nil"u8, test.@in, e);
            }
            if (b != test.@out) {
                Ꮡt.Errorf("ParseBool(%s) = %t; want %t"u8, test.@in, b, test.@out);
            }
        }
    }
}

internal static map<bool, @string> boolString = new map<bool, @string>{
    [true] = "true"u8,
    [false] = "false"u8
};

public static void TestFormatBool(ж<testing.T> Ꮡt) {
    foreach (var (b, s) in boolString) {
        {
            @string f = FormatBool(b); if (f != s) {
                Ꮡt.Errorf("FormatBool(%v) = %q; want %q"u8, b, f, s);
            }
        }
    }
}

partial struct appendBoolTest {
    internal bool b;
    internal slice<byte> @in;
    internal slice<byte> @out;
}

internal static slice<appendBoolTest> appendBoolTests = new appendBoolTest[]{
    new(true, slice<byte>("foo "u8), slice<byte>("foo true"u8)),
    new(false, slice<byte>("foo "u8), slice<byte>("foo false"u8))
}.slice();

public static void TestAppendBool(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in appendBoolTests) {
        var b = AppendBool(test.@in, test.b);
        if (!bytes.Equal(b, test.@out)) {
            Ꮡt.Errorf("AppendBool(%q, %v) = %q; want %q"u8, test.@in, test.b, b, test.@out);
        }
    }
}

} // end strconv_test_package
