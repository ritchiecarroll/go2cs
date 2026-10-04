// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime.@internal.wasitest;

using bytes = bytes_package;
using fmt = fmt_package;
using rand = math.rand_package;
using net = net_package;
using os = os_package;
using exec = go.os.exec_package;
using testing = testing_package;
using time = time_package;
using go.os;
using math;

partial class wasi_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gowasienableracytestˢ = "GOWASIENABLERACYTEST"u8;
private static readonly object skippingWasiTestWithˢ = (@string)"skipping WASI test with unavoidable race condition"u8;
private static readonly @string tcpˢ = "tcp"u8;
private static readonly @string runˢ = "run"u8;
private static readonly @string testdataTcpechoGoˢ = "./testdata/tcpecho.go"u8;
private static readonly @string gowasiruntimeˢ = "GOWASIRUNTIME"u8;
private static readonly object wasiRuntimeDoesNotˢ = (@string)"WASI runtime does not support sockets"u8;
private static readonly object unexpectedPayloadˢ = (@string)"unexpected payload"u8;

public static void TestTCPEcho(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (target != "wasip1/wasm"u8) {
            Ꮡt.Skip();
        }
        // We're unable to use port 0 here (let the OS choose a spare port).
        // Although the WASM runtime accepts port 0, and the WASM module listens
        // successfully, there's no way for this test to query the selected port
        // so that it can connect to the WASM module. The WASM module itself
        // cannot access any information about the socket due to limitations
        // with WASI preview 1 networking, and the WASM runtimes do not log the
        // port when you pre-open a socket. So, we probe for a free port here.
        // Given there's an unavoidable race condition, the test is disabled by
        // default.
        if (os.Getenv(gowasienableracytestˢ) != "1"u8) {
            Ꮡt.Skip(skippingWasiTestWithˢ);
        }
        @string host = default!;
        nint port = rand.Intn(10000) + 40000;
        for (nint attempts = 0; attempts < 10; attempts++) {
            host = fmt.Sprintf("127.0.0.1:%d"u8, port);
            var (l, errΔ1) = net.Listen(tcpˢ, host);
            if (errΔ1 == default!) {
                l.Close();
                break;
            }
            port++;
        }
        var subProcess = exec.Command("go"u8, runˢ, testdataTcpechoGoˢ);
        subProcess.Value.Env = append(os.Environ(), "GOOS=wasip1"u8, "GOARCH=wasm");
        var exprᴛ1 = os.Getenv(gowasiruntimeˢ);
        if (exprᴛ1 == "wazero"u8) {
            subProcess.Value.Env = append((~subProcess).Env, "GOWASIRUNTIMEARGS=--listen="u8 + host);
        }
        else if (exprᴛ1 == "wasmtime"u8 || exprᴛ1 == ""u8) {
            subProcess.Value.Env = append((~subProcess).Env, "GOWASIRUNTIMEARGS=--tcplisten="u8 + host);
        }
        else { /* default: */
            Ꮡt.Skip(wasiRuntimeDoesNotˢ);
        }

        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        subProcess.Value.Stdout = new bytes_BufferжWriter(Ꮡb);
        subProcess.Value.Stderr = new bytes_BufferжWriter(Ꮡb);
        {
            var errΔ2 = subProcess.Start(); if (errΔ2 != default!) {
                Ꮡt.Log(Ꮡb.String());
                Ꮡt.Fatal(errΔ2);
            }
        }
        var subProcessʗ1 = subProcess;
        defer(() => (~subProcessʗ1).Process.Kill(), ref ᒐ);
        net.Conn conn = default!;
        while (ᐧ) {
            error errΔ3 = default!;
            (conn, errΔ3) = net.Dial(tcpˢ, host);
            if (errΔ3 == default!) {
                break;
            }
            time.Sleep(500 * time.Millisecond);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var payload = slice<byte>("foobar"u8);
        {
            var (_, errΔ4) = conn.Write(payload); if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
        }
        array<byte> buf = new(256);
        var (n, err) = conn.Read(buf[..]);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)(buf[..(int)(n)])) != ((sstring)payload)) {
            Ꮡt.Error(unexpectedPayloadˢ);
            Ꮡt.Logf("expect: %d bytes (%v)"u8, len(payload), payload);
            Ꮡt.Logf("actual: %d bytes (%v)"u8, n, buf[..(int)(n)]);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end wasi_test_package
