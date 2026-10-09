// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file provides a simple framework to add benchmarks
// based on generated input (source) files.
namespace go.go;

using bytes = bytes_package;
using flag = flag_package;
using fmt = fmt_package;
using format = global::go.go.format_package;
using os = os_package;
using testing = testing_package;
using fs = io.fs_package;
using global::go.go;
using io = io_package;
using static global::go.go.format_internal_test_package;

partial class format_test_package {

internal static ж<bool> debug = flag.Bool("debug"u8, false, "write .src files containing formatting input; for debugging"u8);

// array1 generates an array literal with n elements of the form:
//
// var _ = [...]byte{
//
//	// 0
//	0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
//	0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
//	0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
//	0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
//	0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
//	// 40
//	0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
//	0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
//	...
internal static void array1(ж<bytes.Buffer> Ꮡbuf, nint n) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    buf.WriteString("var _ = [...]byte{\n"u8);
    for (nint i = 0; i < n; ) {
        if (i % 10 == 0) {
            fmt.Fprintf(new format_test_package.bytes_BufferжWriter(Ꮡbuf), "\t// %d\n"u8, i);
        }
        buf.WriteByte((rune)'\t');
        for (nint j = 0; j < 8; j++) {
            fmt.Fprintf(new format_test_package.bytes_BufferжWriter(Ꮡbuf), "0x%02x, "u8, (byte)i);
            i++;
        }
        buf.WriteString("\n"u8);
    }
    buf.WriteString("}\n"u8);
}

// add new test cases here as needed

partial struct testsᴛ1 /*dyn*/ {
    internal @string name;
    internal Action<ж<bytes.Buffer>, nint> gen;
    internal nint n;
}
internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new("array1"u8, array1, 10000)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePˢ = "package p\n"u8;

public static void BenchmarkFormat(ж<testing.B> Ꮡb) {
    ref var src = ref heap(new bytes.Buffer(), out var Ꮡsrc);
    foreach (var (_, t) in tests) {
        src.Reset();
        src.WriteString(packagePˢ);
        t.gen(Ꮡsrc, t.n);
        var data = src.Bytes();
        if (debug.Value) {
            @string filename = t.name + ".src"u8;
            var err = os.WriteFile(filename, data, 432);
            if (err != default!) {
                Ꮡb.Fatalf("couldn't write %s: %v"u8, filename, err);
            }
        }
        var dataʗ1 = data;
        Ꮡb.Run(fmt.Sprintf("%s-%d"u8, t.name, t.n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)len(dataʗ1));
            bΔ1.ReportAllocs();
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                error err = default!;
                (sink, err) = format.Source(dataʗ1);
                if (err != default!) {
                    bΔ1.Fatal(err);
                }
            }
        });
    }
}

internal static slice<byte> sink;

} // end format_test_package
