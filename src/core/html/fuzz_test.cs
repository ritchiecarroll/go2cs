// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static go.html_package;

partial class html_internal_test_package {

public static void FuzzEscapeUnescape(ж<testing.F> Ꮡf) {
    Ꮡf.Fuzz((ж<testing.T> t, @string v) => {
        @string e = EscapeString(v);
        @string u = UnescapeString(e);
        if (u != v) {
            t.Errorf("EscapeString(%q) = %q, UnescapeString(%q) = %q, want %q"u8, v, e, e, u, v);
        }

        // As per the documentation, this isn't always equal to v, so it makes
        // no sense to check for equality. It can still be interesting to find
        // panics in it though.
        EscapeString(UnescapeString(v));
    });
}

} // end html_internal_test_package
