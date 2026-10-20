// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using io = io_package;
using testing = testing_package;
using static go.@internal.coverage.slicewriter_package;

partial class slicewriter_internal_test_package {

public static void TestSliceWriter(ж<testing.T> Ꮡt) {
    void sleq(ж<testing.T> tΔ1, slice<byte> got, slice<byte> want) {
        tΔ1.Helper();
        if (len(got) != len(want)) {
            tΔ1.Fatalf("bad length got %d want %d"u8, len(got), len(want));
        }
        foreach (var (i, _) in got) {
            if (got[i] != want[i]) {
                tΔ1.Fatalf("bad read at %d got %d want %d"u8, i, got[i], want[i]);
            }
        }
    }

    void wf(ж<testing.T> tΔ2, ж<global::go.@internal.coverage.slicewriter_package.WriteSeeker> wsΔ1, slice<byte> p) {
        tΔ2.Helper();
        var (nw, werr) = wsΔ1.Write(p);
        if (werr != default!) {
            tΔ2.Fatalf("unexpected write error: %v"u8, werr);
        }
        if (nw != len(p)) {
            tΔ2.Fatalf("wrong amount written want %d got %d"u8, len(p), nw);
        }
    }

    var sleqʗ1 = sleq;
    void rf(ж<testing.T> tΔ3, ж<global::go.@internal.coverage.slicewriter_package.WriteSeeker> wsΔ2, slice<byte> p) {
        tΔ3.Helper();
        var b = new slice<byte>(len(p));
        var (nr, rerr) = wsΔ2.Read(b);
        if (rerr != default!) {
            tΔ3.Fatalf("unexpected read error: %v"u8, rerr);
        }
        if (nr != len(p)) {
            tΔ3.Fatalf("wrong amount read want %d got %d"u8, len(p), nr);
        }
        sleqʗ1(tΔ3, b, p);
    }

    int64 sk(ж<testing.T> tΔ4, ж<global::go.@internal.coverage.slicewriter_package.WriteSeeker> wsΔ3, int64 offset, nint whence) {
        tΔ4.Helper();
        var (offΔ1, errΔ1) = wsΔ3.Seek(offset, whence);
        if (errΔ1 != default!) {
            tΔ4.Fatalf("unexpected seek error: %v"u8, errΔ1);
        }
        return offΔ1;
    }

    var wp1 = new byte[]{1, 2}.slice();
    var ws = Ꮡ(new WriteSeeker(nil));

    // write some stuff
    wf(Ꮡt, ws, wp1);
    // check that BytesWritten returns what we wrote.
    sleq(Ꮡt, ws.BytesWritten(), wp1);
    // offset is at end of slice, so reading should return zero bytes.
    rf(Ꮡt, ws, new byte[]{}.slice());

    // write some more stuff
    var wp2 = new byte[]{7, 8, 9}.slice();
    wf(Ꮡt, ws, wp2);
    // check that BytesWritten returns what we expect.
    var wpex = new byte[]{1, 2, 7, 8, 9}.slice();
    sleq(Ꮡt, ws.BytesWritten(), wpex);
    rf(Ꮡt, ws, new byte[]{}.slice());

    // seeks and reads.
    sk(Ꮡt, ws, 1, io.SeekStart);
    rf(Ꮡt, ws, new byte[]{2, 7}.slice());
    sk(Ꮡt, ws, -2, io.SeekCurrent);
    rf(Ꮡt, ws, new byte[]{2, 7}.slice());
    sk(Ꮡt, ws, -4, io.SeekEnd);
    rf(Ꮡt, ws, new byte[]{2, 7}.slice());
    var off = sk(Ꮡt, ws, 0, io.SeekEnd);
    sk(Ꮡt, ws, off, io.SeekStart);

    // seek back and overwrite
    sk(Ꮡt, ws, 1, io.SeekStart);
    wf(Ꮡt, ws, new byte[]{9, 11}.slice());
    wpex = new byte[]{1, 9, 11, 8, 9}.slice();
    sleq(Ꮡt, ws.BytesWritten(), wpex);

    // seeks on empty writer.
    var ws2 = Ꮡ(new WriteSeeker(nil));
    sk(Ꮡt, ws2, 0, io.SeekStart);
    sk(Ꮡt, ws2, 0, io.SeekCurrent);
    sk(Ꮡt, ws2, 0, io.SeekEnd);

    // check for seek errors.
    var (_, err) = ws.Seek(-1, io.SeekStart);
    if (err == default!) {
        Ꮡt.Fatalf("expected error on invalid -1 seek"u8);
    }
    (_, err) = ws.Seek((int64)(len(ws.BytesWritten()) + 1), io.SeekStart);
    if (err == default!) {
        Ꮡt.Fatalf("expected error on invalid %d seek"u8, len(ws.BytesWritten()));
    }

    ws.Seek(0, io.SeekStart);
    (_, err) = ws.Seek(-1, io.SeekCurrent);
    if (err == default!) {
        Ꮡt.Fatalf("expected error on invalid -1 seek"u8);
    }
    (_, err) = ws.Seek((int64)(len(ws.BytesWritten()) + 1), io.SeekCurrent);
    if (err == default!) {
        Ꮡt.Fatalf("expected error on invalid %d seek"u8, len(ws.BytesWritten()));
    }

    (_, err) = ws.Seek(1, io.SeekEnd);
    if (err == default!) {
        Ꮡt.Fatalf("expected error on invalid 1 seek"u8);
    }
    var bsamt = (int64)(-1 * len(ws.BytesWritten()) - 1);
    (_, err) = ws.Seek(bsamt, io.SeekEnd);
    if (err == default!) {
        Ꮡt.Fatalf("expected error on invalid %d seek"u8, bsamt);
    }

    // bad seek mode
    (_, err) = ws.Seek(-1, io.SeekStart + 9);
    if (err == default!) {
        Ꮡt.Fatalf("expected error on invalid seek mode"u8);
    }
}

} // end slicewriter_internal_test_package
