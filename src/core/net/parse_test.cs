// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bufio = bufio_package;
using Δos = os_package;
using Δruntime = runtime_package;
using testing = testing_package;
using static go.net_package;
using Δio = io_package;

partial class net_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string etcServicesˢ => ref etcServicesˢᵇ;
private static readonly @string etcServicesˢᵇ = "/etc/services"u8;

public static void TestReadLine(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // /etc/services file does not exist on android, plan9, windows, or wasip1
        // where it would be required to be mounted from the host file system.
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "android"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "wasip1"u8: {
            t.Skipf("not supported on %s", Δruntime.GOOS);
            break;
        }}
        @string filename = etcServicesˢ; // a nice big file

        var (fd, err) = Δos.Open(filename);
        if (err != nil) {
            // The file is missing even on some Unix systems.
            t.Skipf("skipping because failed to open /etc/services: %v", err);
        }
        var fdʗ1 = fd;
        defer(() => fdʗ1.Close(), ref ᒐ);
        var br = bufio.NewReader(new net_test_package.os_FileжReader(fd));

        (var Δfile, err) = open(filename);
        if (Δfile == nil) {
            t.Fatal(err);
        }
        var fileʗ1 = Δfile;
        defer(fileʗ1.close, ref ᒐ);

        nint lineno = 1;
        nint byteno = 0;
        for (;;) {
            var (bline, berr) = br.ReadString((byte)'\n');
            nint n = len(bline); if (n > 0) {
                bline = bline.slice(0, n - 1);
            }
            var (line, ok) = Δfile.readLine();
            if ((berr != nil) != !ok || bline != line) {
                t.Fatalf("%s:%d (#%d)\nbufio => %q, %v\nnet => %q, %v", filename, lineno, byteno, bline, berr, line, ok);
            }
            if (!ok) {
                break;
            }
            lineno++;
            byteno += len(line) + 1;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestDtoi_type /*dyn*/ {
    internal @string @in;
    internal nint @out;
    internal nint off;
    internal bool ok;
}

public static void TestDtoi(ж<testing.T> t) {
    foreach (var (_, tt) in range(slice<TestDtoi_type>([
        new("", 0, 0, false),
        new("0", 0, 1, true),
        new("65536", 65536, 5, true),
        new("123456789", big, 8, false),
        new("-0", 0, 0, false),
        new("-1234", 0, 0, false)
    ]))) {
        var (n, i, ok) = dtoi(tt.@in);
        if (n != tt.@out || i != tt.off || ok != tt.ok) {
            t.Errorf("got %d, %d, %v; want %d, %d, %v", n, i, ok, tt.@out, tt.off, tt.ok);
        }
    }
}

} // end net_internal_test_package
