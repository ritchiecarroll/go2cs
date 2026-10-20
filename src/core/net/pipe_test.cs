// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δio = io_package;
using Δnet = net_package;
using testing = testing_package;
using time = time_package;
using nettest = vendor.golang.org.x.net.nettest_package;
using static go.net_internal_test_package;
using vendor.golang.org.x.net;

partial class net_test_package {

public static void TestPipe(ж<testing.T> Ꮡt) {
    nettest.TestConn(Ꮡt, () => {
        Δnet.Conn c1 = default!;
        Δnet.Conn c2 = default!;
        Action stop = default!;
        error err = default!;
        (c1, c2) = Δnet.Pipe();
        stop = () => {
            c1.Close();
            c2.Close();
        };
        return (c1, c2, stop, err);
    });
}

public static void TestPipeCloseError(ж<testing.T> Ꮡt) {
    var (c1, c2) = Δnet.Pipe();
    c1.Close();

    {
        var (_, err) = c1.Read(default!); if (!AreEqual(err, Δio.ErrClosedPipe)) {
            Ꮡt.Errorf("c1.Read() = %v, want io.ErrClosedPipe"u8, err);
        }
    }
    {
        var (_, err) = c1.Write(default!); if (!AreEqual(err, Δio.ErrClosedPipe)) {
            Ꮡt.Errorf("c1.Write() = %v, want io.ErrClosedPipe"u8, err);
        }
    }
    {
        var err = c1.SetDeadline(new time.Time(nil)); if (!AreEqual(err, Δio.ErrClosedPipe)) {
            Ꮡt.Errorf("c1.SetDeadline() = %v, want io.ErrClosedPipe"u8, err);
        }
    }
    {
        var (_, err) = c2.Read(default!); if (!AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("c2.Read() = %v, want io.EOF"u8, err);
        }
    }
    {
        var (_, err) = c2.Write(default!); if (!AreEqual(err, Δio.ErrClosedPipe)) {
            Ꮡt.Errorf("c2.Write() = %v, want io.ErrClosedPipe"u8, err);
        }
    }
    {
        var err = c2.SetDeadline(new time.Time(nil)); if (!AreEqual(err, Δio.ErrClosedPipe)) {
            Ꮡt.Errorf("c2.SetDeadline() = %v, want io.ErrClosedPipe"u8, err);
        }
    }
}

} // end net_test_package
