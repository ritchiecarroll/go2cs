// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Test use of raw connections.
//
//go:build !plan9 && !js && !wasip1
namespace go;

using Δos = os_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using static go.os_internal_test_package;

partial class os_test_package {

public static void TestRawConnReadWrite(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();

        var (r, w, err) = Δos.Pipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);

        (var rconn, err) = r.SyscallConn();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var wconn, err) = w.SyscallConn();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }

        ref var operr = ref heap<error>(out var Ꮡoperr);
        err = wconn.Write((uintptr s) => {
            (_, Ꮡoperr.ValueSlot) = syscall.Write((syscallꓸHandle)s, new byte[]{(rune)'b'}.slice());
            return !AreEqual(Ꮡoperr.ValueSlot, syscall.EAGAIN);
        });
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (operr != default!) {
            Ꮡt.Fatal(err);
        }

        nint n = default!;
        var buf = new slice<byte>(1);
        var bufʗ1 = buf;
        err = rconn.Read((uintptr s) => {
            (n, Ꮡoperr.ValueSlot) = syscall.Read((syscallꓸHandle)s, bufʗ1);
            return !AreEqual(Ꮡoperr.ValueSlot, syscall.EAGAIN);
        });
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (operr != default!) {
            Ꮡt.Fatal(operr);
        }
        if (n != 1) {
            Ꮡt.Errorf("read %d bytes, expected 1"u8, n);
        }
        if (buf[0] != (rune)'b') {
            Ꮡt.Errorf("read %q, expected %q"u8, buf, (@string)"b"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end os_test_package
