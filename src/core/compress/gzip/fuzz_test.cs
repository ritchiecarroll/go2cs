// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bytes = bytes_package;
using base64 = encoding.base64_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using fs = go.io.fs_package;
using go.io;
using path;
using static go.compress.gzip_package;

partial class gzip_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
internal static ref readonly @string base64ˢ => ref base64ˢᵇ;
private static readonly @string base64ˢᵇ = ".base64"u8;

public static void FuzzReader(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var inp = slice<byte>("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."u8);
    foreach (var (_, level) in range(slice<nint>([BestSpeed, BestCompression, DefaultCompression, HuffmanOnly]))) {
        var b = bytes.NewBuffer(default);
        var (w, errΔ1) = NewWriterLevel(new gzip_test_package.bytes_BufferжWriter(b), level);
        if (errΔ1 != nil) {
            Ꮡf.Fatalf("failed to construct writer: %s", errΔ1);
        }
        (_, errΔ1) = w.Write(inp);
        if (errΔ1 != nil) {
            Ꮡf.Fatalf("failed to write: %s", errΔ1);
        }
        f.Add(b.Bytes());
    }

    var (testdata, err) = os.ReadDir(testdataˢ);
    if (err != nil) {
        Ꮡf.Fatalf("failed to read testdata directory: %s", err);
    }
    foreach (var (_, de) in range(testdata)) {
        if (de.IsDir()) {
            continue;
        }
        var (b, errΔ2) = os.ReadFile(filepath.Join(testdataˢ, de.Name()));
        if (errΔ2 != nil) {
            Ꮡf.Fatalf("failed to read testdata: %s", errΔ2);
        }

        // decode any base64 encoded test files
        if (strings.HasPrefix(de.Name(), base64ˢ)) {
            (b, errΔ2) = base64.StdEncoding.DecodeString((@string)b);
            if (errΔ2 != nil) {
                Ꮡf.Fatalf("failed to decode base64 testdata: %s", errΔ2);
            }
        }

        f.Add(b);
    }

    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> b) => {
        foreach (var (_, multistream) in range(slice<bool>([true, false]))) {
            var (r, errΔ3) = NewReader(new gzip_test_package.bytes_BufferжReader(bytes.NewBuffer(b)));
            if (errΔ3 != nil) {
                continue;
            }

            r.Multistream(multistream);

            var decompressed = bytes.NewBuffer(default);
            var (_, errΔ4) = io.Copy(new gzip_test_package.bytes_BufferжWriter(decompressed), new gzip_test_package.gzip_ReaderжReader(r)); if (errΔ4 != nil) {
                continue;
            }

            var errΔ5 = r.Close(); if (errΔ5 != nil) {
                continue;
            }

            foreach (var (_, level) in range(slice<nint>([NoCompression, BestSpeed, BestCompression, DefaultCompression, HuffmanOnly]))) {
                var (w, errΔ6) = NewWriterLevel(io.Discard, level);
                if (errΔ6 != nil) {
                    t.Fatalf("failed to construct writer: %s", errΔ6);
                }
                (_, errΔ6) = w.Write(decompressed.Bytes());
                if (errΔ6 != nil) {
                    t.Fatalf("failed to write: %s", errΔ6);
                }
                var errΔ7 = w.Flush(); if (errΔ7 != nil) {
                    t.Fatalf("failed to flush: %s", errΔ7);
                }
                var errΔ8 = w.Close(); if (errΔ8 != nil) {
                    t.Fatalf("failed to close: %s", errΔ8);
                }
            }
        }
    });
}

} // end gzip_internal_test_package
