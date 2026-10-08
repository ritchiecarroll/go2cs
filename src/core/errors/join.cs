// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;
using ꓸꓸꓸerror = Span<error>;

partial class errors_package {

// Join returns an error that wraps the given errors.
// Any nil error values are discarded.
// Join returns nil if every value in errs is nil.
// The error formats as the concatenation of the strings obtained
// by calling the Error method of each element of errs, with a newline
// between each string.
//
// A non-nil error returned by Join implements the Unwrap() []error method.
public static error Join(params ꓸꓸꓸerror errsʗp) {
    var errs = errsʗp.sslice();

    nint n = 0;
    foreach (var (_, err) in errs) {
        if (err != default!) {
            n++;
        }
    }
    if (n == 0) {
        return default!;
    }
    var e = Ꮡ(new joinError(
        errs: new slice<error>(0, n)
    ));
    foreach (var (_, err) in errs) {
        if (err != default!) {
            e.Value.errs = append((~e).errs, err);
        }
    }
    return new joinErrorжerror(e);
}

partial struct joinError {
    internal slice<error> errs;
}

internal static @string Error(this ref joinError e) {
    // Since Join returns nil if every value in errs is nil,
    // e.errs cannot be empty.
    if (len(e.errs) == 1) {
        return e.errs[0].Error();
    }
    var b = slice<byte>(e.errs[0].Error());
    foreach (var (_, err) in e.errs[1..]) {
        b = append(b, (byte)((rune)'\n'));
        b = append(b, err.Error().ꓸꓸꓸ);
    }
    // At this point, b has at least one byte '\n'.
    return @unsafe.String(Ꮡ(b, 0), len(b));
}

internal static slice<error> Unwrap(this ref joinError e) {
    return e.errs;
}

} // end errors_package
