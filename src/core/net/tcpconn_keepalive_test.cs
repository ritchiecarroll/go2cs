// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || solaris || windows
namespace go;

using Δruntime = runtime_package;
using testing = testing_package;
using static go.net_package;
using syscall = syscall_package;
using time = time_package;

partial class net_internal_test_package {

public static void TestTCPConnKeepAliveConfigDialer(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        maybeSkipKeepAliveTest(Ꮡt);

        Ꮡt.Cleanup(() => {
            testPreHookSetKeepAlive = (ж<global::go.net_package.netFD> _) => {
            };
        });
        ref var errHook = ref heap<error>(out var ᏑerrHook);
        ref var oldCfg = ref heap(new global::go.net_package.KeepAliveConfig(), out var ᏑoldCfg);
        testPreHookSetKeepAlive = (ж<global::go.net_package.netFD> nfd) => {
            (ᏑoldCfg.Value, ᏑerrHook.ValueSlot) = getCurrentKeepAliveSettings((~nfd).pfd.Sysfd);
        };

        var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener lnΔ1) => {
            while (ᐧ) {
                var (c, err) = lnΔ1.Accept();
                if (err != default!) {
                    return;
                }
                c.Close();
            }
        };
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ, Ꮡ(new ListenConfig(
            KeepAlive: -1 // prevent calling hook from accepting
        )));
        var ls = (Ꮡ(new streamListener(Listener: ln))).newLocalServer();
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var err = ls.buildup(handler); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }

        foreach (var (_, vᴛ1) in testConfigs) {
            ref var cfg = ref heap(new global::go.net_package.KeepAliveConfig(), out var Ꮡcfg);
            cfg = vᴛ1;

            ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
            d = new Dialer(
                KeepAlive: defaultTCPKeepAliveIdle, // should be ignored
                KeepAliveConfig: cfg);
            var (c, err) = Ꮡd.Dial(tcpˢ, (~ls).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);

            if (errHook != default!) {
                Ꮡt.Fatal(errHook);
            }

            (var sc, err) = c._<ж<global::go.net_package.TCPConn>>().SyscallConn();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            {
                var cfgʗ1 = cfg;
                var errΔ1 = sc.Control((uintptr fd) => {
                    verifyKeepAliveSettings(Ꮡt, (syscallꓸHandle)fd, ᏑoldCfg.Value, cfgʗ1);
                }); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTCPConnKeepAliveConfigListener(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        maybeSkipKeepAliveTest(Ꮡt);

        Ꮡt.Cleanup(() => {
            testPreHookSetKeepAlive = (ж<global::go.net_package.netFD> _) => {
            };
        });
        ref var errHook = ref heap<error>(out var ᏑerrHook);
        ref var oldCfg = ref heap(new global::go.net_package.KeepAliveConfig(), out var ᏑoldCfg);
        testPreHookSetKeepAlive = (ж<global::go.net_package.netFD> nfd) => {
            (ᏑoldCfg.Value, ᏑerrHook.ValueSlot) = getCurrentKeepAliveSettings((~nfd).pfd.Sysfd);
        };

        var ch = new channel<global::go.net_package.Conn>(1);
        var chʗ1 = ch;
        var handler = (ж<localServer> ls, global::go.net_package.Listener ln) => {
            var (c, err) = ln.Accept();
            if (err != default!) {
                return;
            }
            chʗ1.ᐸꟷ(c);
        };
        foreach (var (_, vᴛ1) in testConfigs) {
            ref var cfg = ref heap(new global::go.net_package.KeepAliveConfig(), out var Ꮡcfg);
            cfg = vᴛ1;

            var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ, Ꮡ(new ListenConfig(
                KeepAlive: defaultTCPKeepAliveIdle, // should be ignored
                KeepAliveConfig: cfg)));
            var ls = (Ꮡ(new streamListener(Listener: ln))).newLocalServer();
            var lsʗ1 = ls;
            defer(() => lsʗ1.teardown(), ref ᒐ);
            {
                var errΔ1 = ls.buildup(handler); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
            d = new Dialer(KeepAlive: -1); // prevent calling hook from dialing
            var (c, err) = Ꮡd.Dial(tcpˢ, (~ls).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);

            var cc = ᐸꟷ(ch);
            var ccʗ1 = cc;
            defer(() => ccʗ1.Close(), ref ᒐ);
            if (errHook != default!) {
                Ꮡt.Fatal(errHook);
            }
            (var sc, err) = cc._<ж<global::go.net_package.TCPConn>>().SyscallConn();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            {
                var cfgʗ1 = cfg;
                var errΔ1 = sc.Control((uintptr fd) => {
                    verifyKeepAliveSettings(Ꮡt, (syscallꓸHandle)fd, ᏑoldCfg.Value, cfgʗ1);
                }); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTCPConnKeepAliveConfig(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        maybeSkipKeepAliveTest(Ꮡt);

        var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
            while (ᐧ) {
                var (c, err) = ln.Accept();
                if (err != default!) {
                    return;
                }
                c.Close();
            }
        };
        var ls = newLocalServer(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var err = ls.buildup(handler); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        foreach (var (_, vᴛ1) in testConfigs) {
            ref var cfg = ref heap(new global::go.net_package.KeepAliveConfig(), out var Ꮡcfg);
            cfg = vᴛ1;

            ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
            d = new Dialer(KeepAlive: -1); // avoid setting default values before the test
            var (c, err) = Ꮡd.Dial(tcpˢ, (~ls).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);

            (var sc, err) = c._<ж<global::go.net_package.TCPConn>>().SyscallConn();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }

            ref var errHook = ref heap<error>(out var ᏑerrHook);
            ref var oldCfg = ref heap(new global::go.net_package.KeepAliveConfig(), out var ᏑoldCfg);
            {
                var errΔ1 = sc.Control((uintptr fd) => {
                    (ᏑoldCfg.Value, ᏑerrHook.ValueSlot) = getCurrentKeepAliveSettings((syscallꓸHandle)fd);
                }); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            if (errHook != default!) {
                Ꮡt.Fatal(errHook);
            }

            err = c._<ж<global::go.net_package.TCPConn>>().SetKeepAliveConfig(cfg);
            if (err != default!) {
                if (Δruntime.GOOS == "solaris"u8){
                    // Solaris prior to 11.4 does not support TCP_KEEPINTVL and TCP_KEEPCNT,
                    // so it will return syscall.ENOPROTOOPT when only one of Interval and Count
                    // is negative. This is expected, so skip the error check in this case.
                    if (cfg.Interval >= 0 && cfg.Count >= 0) {
                        Ꮡt.Fatal(err);
                    }
                } else {
                    Ꮡt.Fatal(err);
                }
            }

            {
                var cfgʗ1 = cfg;
                var errΔ2 = sc.Control((uintptr fd) => {
                    verifyKeepAliveSettings(Ꮡt, (syscallꓸHandle)fd, ᏑoldCfg.Value, cfgʗ1);
                }); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
