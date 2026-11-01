// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.wasitest;

using bytes = bytes_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using rand = math.rand_package;
using net = net_package;
using os = os_package;
using exec = go.os.exec_package;
using testing = testing_package;
using time = time_package;
using go.@internal;
using go.os;
using math;

partial class wasi_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string gowasienableracytestˢ => ref gowasienableracytestˢᵇ;
private static readonly @string gowasienableracytestˢᵇ = "GOWASIENABLERACYTEST"u8;
private static readonly object skippingWasiTestWithˢ = (@string)"skipping WASI test with unavoidable race condition"u8;
private static ref readonly @string tcpˢ => ref tcpˢᵇ;
private static readonly @string tcpˢᵇ = "tcp"u8;
private static ref readonly @string runˢ => ref runˢᵇ;
private static readonly @string runˢᵇ = "run"u8;
private static ref readonly @string testdataTcpechoGoˢ => ref testdataTcpechoGoˢᵇ;
private static readonly @string testdataTcpechoGoˢᵇ = "./testdata/tcpecho.go"u8;
private static ref readonly @string gowasiruntimeˢ => ref gowasiruntimeˢᵇ;
private static readonly @string gowasiruntimeˢᵇ = "GOWASIRUNTIME"u8;
private static readonly object wasiRuntimeDoesNotˢ = (@string)"WASI runtime does not support sockets"u8;
private static readonly object unexpectedPayloadˢ = (@string)"unexpected payload"u8;

public static void TestTCPEcho(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (target != "wasip1/wasm"u8) {
            t.Skip();
        }

        testenv.MustHaveGoRun(new testing_TжTB(t));

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
            t.Skip(skippingWasiTestWithˢ);
        }
        @string host = default;
        nint port = rand.Intn(10000) + 40000;
        for (nint attempts = 0; attempts < 10; attempts++) {
            host = fmt.Sprintf("127.0.0.1:%d"u8, port);
            var (l, errΔ1) = net.Listen(tcpˢ, host);
            if (errΔ1 == nil) {
                l.Close();
                break;
            }
            port++;
        }

        var subProcess = exec.Command(testenv.GoToolPath(new testing_TжTB(t)), runˢ, testdataTcpechoGoˢ);

        subProcess.Env = append(os.Environ(), (@string)"GOOS=wasip1", (@string)"GOARCH=wasm");

        var exprᴛ1 = os.Getenv(gowasiruntimeˢ);
        switch (ᐧ) {
        case {} when exprᴛ1 == "wazero"u8: {
            subProcess.Env = append(subProcess.Env, "GOWASIRUNTIMEARGS=--listen="u8 + host);
            break;
        }
        case {} when exprᴛ1 == "wasmtime"u8 || exprᴛ1 == ""u8: {
            subProcess.Env = append(subProcess.Env, "GOWASIRUNTIMEARGS=--tcplisten="u8 + host);
            break;
        }
        default: {
            t.Skip(wasiRuntimeDoesNotˢ);
            break;
        }}

        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        subProcess.Stdout = new bytes_BufferжWriter(Ꮡb);
        subProcess.Stderr = new bytes_BufferжWriter(Ꮡb);

        {
            var errΔ2 = subProcess.Start(); if (errΔ2 != nil) {
                t.Log(Ꮡb.String());
                t.Fatal(errΔ2);
            }
        }
        var subProcessʗ1 = subProcess;
        defer(ᴛ0 => ᴛ0.Kill(), subProcessʗ1.Process, ref ᒐ);

        net.Conn conn = default;
        for (;;) {
            error errΔ3 = default;
            (conn, errΔ3) = net.Dial(tcpˢ, host);
            if (errΔ3 == nil) {
                break;
            }
            time.Sleep(500 * time.Millisecond);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);

        var payload = slice<byte>("foobar"u8);
        {
            var (_, errΔ4) = conn.Write(payload); if (errΔ4 != nil) {
                t.Fatal(errΔ4);
            }
        }
        array<byte> buf = new(256);
        var (n, err) = conn.Read(buf[..]);
        if (err != nil) {
            t.Fatal(err);
        }
        if (((sstring)(buf.slice(0, n))) != ((sstring)payload)) {
            t.Error(unexpectedPayloadˢ);
            t.Logf("expect: %d bytes (%v)", len(payload), payload);
            t.Logf("actual: %d bytes (%v)", n, buf.slice(0, n));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end wasi_test_package
