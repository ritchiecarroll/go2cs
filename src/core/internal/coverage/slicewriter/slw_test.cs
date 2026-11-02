// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using io = io_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.@internal.coverage.slicewriter_package;

partial class slicewriter_internal_test_package {

public static void TestSliceWriter(ж<testing.T> t) {
    void sleq(ж<testing.T> tΔ1, slice<byte> got, slice<byte> want) {
        tΔ1.Helper();
        if (len(got) != len(want)) {
            tΔ1.Fatalf("bad length got %d want %d", len(got), len(want));
        }
        foreach (var i in range(got)) {
            if (got[i] != want[i]) {
                tΔ1.Fatalf("bad read at %d got %d want %d", i, got[i], want[i]);
            }
        }
    }

    void wf(ж<testing.T> tΔ2, ж<global::go.@internal.coverage.slicewriter_package.WriteSeeker> wsΔ1, slice<byte> p) {
        tΔ2.Helper();
        var (nw, werr) = wsΔ1.Write(p);
        if (werr != nil) {
            tΔ2.Fatalf("unexpected write error: %v", werr);
        }
        if (nw != len(p)) {
            tΔ2.Fatalf("wrong amount written want %d got %d", len(p), nw);
        }
    }

    var sleqʗ1 = sleq;
    [MethodImpl(MethodImplOptions.NoInlining)] void rf(ж<testing.T> tΔ3, ж<global::go.@internal.coverage.slicewriter_package.WriteSeeker> wsΔ2, slice<byte> p) {
        tΔ3.Helper();
        var b = new slice<byte>(len(p));
        var (nr, rerr) = wsΔ2.Read(b);
        if (rerr != nil) {
            tΔ3.Fatalf("unexpected read error: %v", rerr);
        }
        if (nr != len(p)) {
            tΔ3.Fatalf("wrong amount read want %d got %d", len(p), nr);
        }
        sleqʗ1(tΔ3, b, p);
    }

    int64 sk(ж<testing.T> tΔ4, ж<global::go.@internal.coverage.slicewriter_package.WriteSeeker> wsΔ3, int64 offset, nint whence) {
        tΔ4.Helper();
        var (offΔ1, errΔ1) = wsΔ3.Seek(offset, whence);
        if (errΔ1 != nil) {
            tΔ4.Fatalf("unexpected seek error: %v", errΔ1);
        }
        return offΔ1;
    }

    var wp1 = slice<byte>([1, 2]);
    var ws = Ꮡ(new WriteSeeker());

    // write some stuff
    wf(t, ws, wp1);
    // check that BytesWritten returns what we wrote.
    sleq(t, ws.BytesWritten(), wp1);
    // offset is at end of slice, so reading should return zero bytes.
    rf(t, ws, slice<byte>([]));

    // write some more stuff
    var wp2 = slice<byte>([7, 8, 9]);
    wf(t, ws, wp2);
    // check that BytesWritten returns what we expect.
    var wpex = slice<byte>([1, 2, 7, 8, 9]);
    sleq(t, ws.BytesWritten(), wpex);
    rf(t, ws, slice<byte>([]));

    // seeks and reads.
    sk(t, ws, 1, io.SeekStart);
    rf(t, ws, slice<byte>([2, 7]));
    sk(t, ws, -2, io.SeekCurrent);
    rf(t, ws, slice<byte>([2, 7]));
    sk(t, ws, -4, io.SeekEnd);
    rf(t, ws, slice<byte>([2, 7]));
    var off = sk(t, ws, 0, io.SeekEnd);
    sk(t, ws, off, io.SeekStart);

    // seek back and overwrite
    sk(t, ws, 1, io.SeekStart);
    wf(t, ws, slice<byte>([9, 11]));
    wpex = slice<byte>([1, 9, 11, 8, 9]);
    sleq(t, ws.BytesWritten(), wpex);

    // seeks on empty writer.
    var ws2 = Ꮡ(new WriteSeeker());
    sk(t, ws2, 0, io.SeekStart);
    sk(t, ws2, 0, io.SeekCurrent);
    sk(t, ws2, 0, io.SeekEnd);

    // check for seek errors.
    var (_, err) = ws.Seek(-1, io.SeekStart);
    if (err == nil) {
        t.Fatalf("expected error on invalid -1 seek");
    }
    (_, err) = ws.Seek((int64)(len(ws.BytesWritten()) + 1), io.SeekStart);
    if (err == nil) {
        t.Fatalf("expected error on invalid %d seek", len(ws.BytesWritten()));
    }

    ws.Seek(0, io.SeekStart);
    (_, err) = ws.Seek(-1, io.SeekCurrent);
    if (err == nil) {
        t.Fatalf("expected error on invalid -1 seek");
    }
    (_, err) = ws.Seek((int64)(len(ws.BytesWritten()) + 1), io.SeekCurrent);
    if (err == nil) {
        t.Fatalf("expected error on invalid %d seek", len(ws.BytesWritten()));
    }

    (_, err) = ws.Seek(1, io.SeekEnd);
    if (err == nil) {
        t.Fatalf("expected error on invalid 1 seek");
    }
    var bsamt = (int64)(-1 * len(ws.BytesWritten()) - 1);
    (_, err) = ws.Seek(bsamt, io.SeekEnd);
    if (err == nil) {
        t.Fatalf("expected error on invalid %d seek", bsamt);
    }

    // bad seek mode
    (_, err) = ws.Seek(-1, io.SeekStart + 9);
    if (err == nil) {
        t.Fatalf("expected error on invalid seek mode");
    }
}

} // end slicewriter_internal_test_package
