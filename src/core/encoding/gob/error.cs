// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class gob_package {

// Errors in decoding and encoding are handled using panic and recover.
// Panics caused by user error (that is, everything except run-time panics
// such as "index out of bounds" errors) do not leave the file that caused
// them, but are instead turned into plain error returns. Encoding and
// decoding functions and methods that do not return an error either use
// panic to report an error or are guaranteed error-free.

// A gobError is used to distinguish errors (panics) generated in this package.
partial struct gobError {
    internal error err;
}

// errorf is like error_ but takes Printf-style arguments to construct an error.
// It always prefixes the message with "gob: ".
internal static void errorf(@string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    error_(fmt.Errorf("gob: "u8 + format, args.ꓸꓸꓸ));
}

// error_ wraps the argument error and uses it as the argument to panic.
internal static void error_(error err) {
    throw panic(new gobError(err));
}

// catchError is meant to be used as a deferred function to turn a panic(gobError) into a
// plain error. It overwrites the error return of the function that deferred its call.
internal static void catchError(ref error err) {
    GoFrame ᒐ = default;
    try {
        {
            var e = recover(); if (e != default!) {
                var (ge, ok) = e._<gobError>(ᐧ);
                if (!ok) {
                    throw panic(e);
                }
                err = ge.err;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end gob_package
