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
using static go.image.gif_package;

partial class gif_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"Skipping in short mode"u8;
internal static readonly @string testdataˢ = "../testdata"u8;
internal static readonly @string gifˢ = ".gif"u8;

public static void FuzzDecode(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (testing.Short()) {
        Ꮡf.Skip(skippingInShortModeˢ);
    }

    var (testdata, err) = os.ReadDir(testdataˢ);
    if (err != default!) {
        Ꮡf.Fatalf("failed to read testdata directory: %s"u8, err);
    }
    foreach (var (_, de) in testdata) {
        if (de.IsDir() || !strings.HasSuffix(de.Name(), gifˢ)) {
            continue;
        }
        var (b, errΔ1) = os.ReadFile(filepath.Join(testdataˢ, de.Name()));
        if (errΔ1 != default!) {
            Ꮡf.Fatalf("failed to read testdata: %s"u8, errΔ1);
        }
        f.Add(b);
    }

    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> b) => {
        var (cfg, _, errΔ2) = image.DecodeConfig(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
        if (errΔ2 != default!) {
            return;
        }
        if (cfg.Width * cfg.Height > 1000000) {
            return;
        }
        (var img, var typ, errΔ2) = image.Decode(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
        if (errΔ2 != default! || typ != "gif"u8) {
            return;
        }
        for (nint qᴛ1 = 1; qᴛ1 <= 256; qᴛ1++) {
            ref var q = ref heap<nint>(out var Ꮡq);
            q = qᴛ1;
            ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
            var errΔ3 = Encode(new gif_internal_test_package.bytes_BufferжWriter(Ꮡw), img, Ꮡ(new Options(NumColors: q)));
            if (errΔ3 != default!) {
                t.Fatalf("failed to encode valid image: %s"u8, errΔ3);
            }
            (var img1, errΔ3) = Decode(new gif_internal_test_package.bytes_BufferжReader(Ꮡw));
            if (errΔ3 != default!) {
                t.Fatalf("failed to decode roundtripped image: %s"u8, errΔ3);
            }
            var got = img1.Bounds();
            var want = img.Bounds();
            if (!got.Eq(want)) {
                t.Fatalf("roundtripped image bounds have changed, got: %v, want: %v"u8, got, want);
            }
            qᴛ1 = q;
        }
    });
}

} // end gif_internal_test_package
