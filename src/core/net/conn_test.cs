// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements API tests across platforms and should never have a build
// constraint.
namespace go;

using testing = testing_package;
using time = time_package;
using static go.net_package;

partial class net_internal_test_package {

// someTimeout is used just to test that net.Conn implementations
// don't explode when their SetFooDeadline methods are called.
// It isn't actually used for testing timeouts.
internal static time.Duration someTimeout => /* 1 * time.Hour */ 3600000000000;

public static void TestConnAndListener(ж<testing.T> Ꮡt) {
    foreach (var (i, network) in new @string[]{"tcp"u8, "unix"u8, "unixpacket"u8}.slice()) {
        nint iΔ1 = i;
        @string networkΔ1 = network;
        Ꮡt.Run(networkΔ1, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(networkΔ1)) {
                    tΔ1.Skipf("skipping %s test"u8, networkΔ1);
                }

                var ls = newLocalServer(new net_test_package.testing_TжTB(tΔ1), networkΔ1);
                var lsʗ1 = ls;
                defer(() => lsʗ1.teardown(), ref ᒐ);
                var ch = new channel<error>(1);
                var chʗ1 = ch;
                var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
                    lsΔ1.transponder(ln, chʗ1.WithDirection(GoChanDir.Send));
                };
                {
                    var errΔ1 = ls.buildup(handler); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
                if ((~ls).Listener.Addr().Network() != networkΔ1) {
                    tΔ1.Fatalf("got %s; want %s"u8, (~ls).Listener.Addr().Network(), networkΔ1);
                }

                var (c, err) = Dial((~ls).Listener.Addr().Network(), (~ls).Listener.Addr().String());
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                if (c.LocalAddr().Network() != networkΔ1 || c.RemoteAddr().Network() != networkΔ1) {
                    tΔ1.Fatalf("got %s->%s; want %s->%s"u8, c.LocalAddr().Network(), c.RemoteAddr().Network(), networkΔ1, networkΔ1);
                }
                c.SetDeadline(time.Now().Add(someTimeout));
                c.SetReadDeadline(time.Now().Add(someTimeout));
                c.SetWriteDeadline(time.Now().Add(someTimeout));

                {
                    var (_, errΔ1) = c.Write(slice<byte>("CONN AND LISTENER TEST"u8)); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
                var rb = new slice<byte>(128);
                {
                    var (_, errΔ2) = c.Read(rb); if (errΔ2 != default!) {
                        tΔ1.Fatal(errΔ2);
                    }
                }

                foreach (var errΔ3 in ch) {
                    tΔ1.Errorf("#%d: %v"u8, iΔ1, errΔ3);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

} // end net_internal_test_package
