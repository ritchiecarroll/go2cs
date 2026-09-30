// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using io = io_package;
using log = log_package;

partial class iotest_package {

[GoType] partial struct writeLogger {
    internal @string prefix;
    internal io.Writer w;
}

[GoRecv] internal static (nint n, error err) Write(this ref writeLogger l, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = l.w.Write(p);
    if (err != default!){
        log.Printf("%s %x: %v"u8, l.prefix, p.slice(0, n), err);
    } else {
        log.Printf("%s %x"u8, l.prefix, p.slice(0, n));
    }
    return (n, err);
}

// NewWriteLogger returns a writer that behaves like w except
// that it logs (using [log.Printf]) each write to standard error,
// printing the prefix and the hexadecimal data written.
public static io.Writer NewWriteLogger(@string prefix, io.Writer w) {
    return new writeLoggerжWriter(Ꮡ(new writeLogger(prefix, w)));
}

[GoType] partial struct readLogger {
    internal @string prefix;
    internal io.Reader r;
}

[GoRecv] internal static (nint n, error err) Read(this ref readLogger l, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = l.r.Read(p);
    if (err != default!){
        log.Printf("%s %x: %v"u8, l.prefix, p.slice(0, n), err);
    } else {
        log.Printf("%s %x"u8, l.prefix, p.slice(0, n));
    }
    return (n, err);
}

// NewReadLogger returns a reader that behaves like r except
// that it logs (using [log.Printf]) each read to standard error,
// printing the prefix and the hexadecimal data read.
public static io.Reader NewReadLogger(@string prefix, io.Reader r) {
    return new readLoggerжReader(Ꮡ(new readLogger(prefix, r)));
}

} // end iotest_package
