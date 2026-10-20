// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || windows
namespace go;

using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using static go.net_package;

partial class net_internal_test_package {

internal static (global::go.net_package.KeepAliveConfig cfg, error err) getCurrentKeepAliveSettings(syscallꓸHandle fd) {
    global::go.net_package.KeepAliveConfig cfg = default!;
    error err = default!;

    (var tcpKeepAlive, err) = syscall.GetsockoptInt(fd, syscall.SOL_SOCKET, syscall.SO_KEEPALIVE);
    if (err != default!) {
        return (cfg, err);
    }
    (var tcpKeepAliveIdle, err) = syscall.GetsockoptInt(fd, syscall.IPPROTO_TCP, syscall_TCP_KEEPIDLE);
    if (err != default!) {
        return (cfg, err);
    }
    (var tcpKeepAliveInterval, err) = syscall.GetsockoptInt(fd, syscall.IPPROTO_TCP, syscall_TCP_KEEPINTVL);
    if (err != default!) {
        return (cfg, err);
    }
    (var tcpKeepAliveCount, err) = syscall.GetsockoptInt(fd, syscall.IPPROTO_TCP, syscall_TCP_KEEPCNT);
    if (err != default!) {
        return (cfg, err);
    }
    cfg = new KeepAliveConfig(
        Enable: tcpKeepAlive != 0,
        Idle: ((time.Duration)(int64)tcpKeepAliveIdle) * time.ΔSecond,
        Interval: ((time.Duration)(int64)tcpKeepAliveInterval) * time.ΔSecond,
        Count: tcpKeepAliveCount
    );
    return (cfg, err);
}

internal static void verifyKeepAliveSettings(ж<testing.T> Ꮡt, syscallꓸHandle fd, global::go.net_package.KeepAliveConfig oldCfg, global::go.net_package.KeepAliveConfig cfg) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (cfg.Idle == 0) {
        cfg.Idle = defaultTCPKeepAliveIdle;
    }
    if (cfg.Interval == 0) {
        cfg.Interval = defaultTCPKeepAliveInterval;
    }
    if (cfg.Count == 0) {
        cfg.Count = defaultTCPKeepAliveCount;
    }
    if (cfg.Idle == -1) {
        cfg.Idle = oldCfg.Idle;
    }
    if (cfg.Interval == -1) {
        cfg.Interval = oldCfg.Interval;
    }
    if (cfg.Count == -1) {
        cfg.Count = oldCfg.Count;
    }

    var (tcpKeepAlive, err) = syscall.GetsockoptInt(fd, syscall.SOL_SOCKET, syscall.SO_KEEPALIVE);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((tcpKeepAlive != 0) != cfg.Enable) {
        Ꮡt.Fatalf("SO_KEEPALIVE: got %t; want %t"u8, tcpKeepAlive != 0, cfg.Enable);
    }

    (var tcpKeepAliveIdle, err) = syscall.GetsockoptInt(fd, syscall.IPPROTO_TCP, syscall_TCP_KEEPIDLE);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (((time.Duration)(int64)tcpKeepAliveIdle) * time.ΔSecond != cfg.Idle) {
        Ꮡt.Fatalf("TCP_KEEPIDLE: got %ds; want %v"u8, tcpKeepAliveIdle, cfg.Idle);
    }

    (var tcpKeepAliveInterval, err) = syscall.GetsockoptInt(fd, syscall.IPPROTO_TCP, syscall_TCP_KEEPINTVL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (((time.Duration)(int64)tcpKeepAliveInterval) * time.ΔSecond != cfg.Interval) {
        Ꮡt.Fatalf("TCP_KEEPINTVL: got %ds; want %v"u8, tcpKeepAliveInterval, cfg.Interval);
    }

    (var tcpKeepAliveCount, err) = syscall.GetsockoptInt(fd, syscall.IPPROTO_TCP, syscall_TCP_KEEPCNT);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (tcpKeepAliveCount != cfg.Count) {
        Ꮡt.Fatalf("TCP_KEEPCNT: got %d; want %d"u8, tcpKeepAliveCount, cfg.Count);
    }
}

} // end net_internal_test_package
