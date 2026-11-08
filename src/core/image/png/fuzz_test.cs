// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bytes = bytes_package;
using image = image_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.io;
using io = io_package;
using path;
using static go.image.png_package;

partial class png_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"Skipping in short mode"u8;
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "../testdata"u8;
internal static ref readonly @string pngˢ => ref pngˢᵇ;
private static readonly @string pngˢᵇ = ".png"u8;

public static void FuzzDecode(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (testing.Short()) {
        Ꮡf.Skip(skippingInShortModeˢ);
    }

    var (testdata, err) = os.ReadDir(testdataˢ);
    if (err != nil) {
        Ꮡf.Fatalf("failed to read testdata directory: %s", err);
    }
    foreach (var (_, de) in range(testdata)) {
        if (de.IsDir() || !strings.HasSuffix(de.Name(), pngˢ)) {
            continue;
        }
        var (b, errΔ1) = os.ReadFile(filepath.Join(testdataˢ, de.Name()));
        if (errΔ1 != nil) {
            Ꮡf.Fatalf("failed to read testdata: %s", errΔ1);
        }
        f.Add(b);
    }

    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> b) => {
        var (cfg, _, errΔ2) = image.DecodeConfig(new png_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
        if (errΔ2 != nil) {
            return;
        }
        if (cfg.Width * cfg.Height > 1000000) {
            return;
        }
        (var img, var typ, errΔ2) = image.Decode(new png_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
        if (errΔ2 != nil || typ != "png"u8) {
            return;
        }
        var levels = slice<global::go.image.png_package.CompressionLevel>([
            DefaultCompression,
            NoCompression,
            BestSpeed,
            BestCompression
        ]);
        foreach (var (_, vᴛ1) in range(levels)) {
            ref var l = ref heap(new global::go.image.png_package.CompressionLevel(), out var Ꮡl);
            l = vᴛ1;

            ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
            var e = Ꮡ(new Encoder(CompressionLevel: l));
            errΔ2 = e.Encode(new png_test_package.bytes_BufferжWriter(Ꮡw), img);
            if (errΔ2 != nil) {
                t.Errorf("failed to encode valid image: %s", errΔ2);
                continue;
            }
            var (img1, errΔ3) = Decode(new png_test_package.bytes_BufferжReader(Ꮡw));
            if (errΔ3 != nil) {
                t.Errorf("failed to decode roundtripped image: %s", errΔ3);
                continue;
            }
            var got = img1.Bounds();
            var want = img.Bounds();
            if (!got.Eq(want)) {
                t.Errorf("roundtripped image bounds have changed, got: %s, want: %s", got, want);
            }
        }
    });
}

} // end png_internal_test_package
