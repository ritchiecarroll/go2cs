// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows
namespace go.@internal;

using static go.@internal.poll_package;
using io = io_package;
using testing = testing_package;
using go.@internal;
using poll = go.@internal.poll_package;
using static go.@internal.poll_internal_test_package;

partial class poll_test_package {


partial struct eofErrorTestsᴛ1 /*dyn*/ {
    internal nint n;
    internal error err;
    internal ж<poll.FD> fd;
    internal error expected;
}
internal static slice<eofErrorTestsᴛ1> eofErrorTests = new eofErrorTestsᴛ1[]{
    new(100, default!, Ꮡ(new FD(ZeroReadIsEOF: true)), default!),
    new(100, io.EOF, Ꮡ(new FD(ZeroReadIsEOF: true)), io.EOF),
    new(100, ErrNetClosing, Ꮡ(new FD(ZeroReadIsEOF: true)), ErrNetClosing),
    new(0, default!, Ꮡ(new FD(ZeroReadIsEOF: true)), io.EOF),
    new(0, io.EOF, Ꮡ(new FD(ZeroReadIsEOF: true)), io.EOF),
    new(0, ErrNetClosing, Ꮡ(new FD(ZeroReadIsEOF: true)), ErrNetClosing),
    new(100, default!, Ꮡ(new FD(ZeroReadIsEOF: false)), default!),
    new(100, io.EOF, Ꮡ(new FD(ZeroReadIsEOF: false)), io.EOF),
    new(100, ErrNetClosing, Ꮡ(new FD(ZeroReadIsEOF: false)), ErrNetClosing),
    new(0, default!, Ꮡ(new FD(ZeroReadIsEOF: false)), default!),
    new(0, io.EOF, Ꮡ(new FD(ZeroReadIsEOF: false)), io.EOF),
    new(0, ErrNetClosing, Ꮡ(new FD(ZeroReadIsEOF: false)), ErrNetClosing)
}.slice();

public static void TestEOFError(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in eofErrorTests) {
        var actual = tt.fd.EOFError(tt.n, tt.err);
        if (!AreEqual(actual, tt.expected)) {
            Ꮡt.Errorf("eofError(%v, %v, %v): expected %v, actual %v"u8, tt.n, tt.err, (~tt.fd).ZeroReadIsEOF, tt.expected, actual);
        }
    }
}

} // end poll_test_package
