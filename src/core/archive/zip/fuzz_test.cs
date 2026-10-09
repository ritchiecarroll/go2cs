// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using bytes = bytes_package;
using io = io_package;
using os = os_package;
using filepath = go.path.filepath_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.io;
using go.path;
using static go.archive.zip_package;

partial class zip_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "testdata"u8;

internal partial struct FuzzReader_file /*dyn*/ {
    internal ж<global::go.archive.zip_package.FileHeader> header;
    internal slice<byte> content;
}

public static void FuzzReader(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var (testdata, err) = os.ReadDir(testdataˢ);
    if (err != default!) {
        Ꮡf.Fatalf("failed to read testdata directory: %s"u8, err);
    }
    foreach (var (_, de) in testdata) {
        if (de.IsDir()) {
            continue;
        }
        var (b, errΔ1) = os.ReadFile(filepath.Join(testdataˢ, de.Name()));
        if (errΔ1 != default!) {
            Ꮡf.Fatalf("failed to read testdata: %s"u8, errΔ1);
        }
        f.Add(b);
    }
    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> b) => {
        var (r, errΔ2) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)), (int64)len(b));
        if (errΔ2 != default!) {
            return;
        }
        var files = new FuzzReader_file[]{}.slice();
        foreach (var (_, fΔ1) in (~r).File) {
            var (fr, errΔ3) = fΔ1.Open();
            if (errΔ3 != default!) {
                continue;
            }
            (var content, errΔ3) = io.ReadAll(fr);
            if (errΔ3 != default!) {
                continue;
            }
            files = append(files, new FuzzReader_file(header: fΔ1.of(global::go.archive.zip_package.File.ᏑFileHeader), content: content));
            {
                var (_, errΔ4) = r.Open((~fΔ1).Name); if (errΔ4 != default!) {
                    continue;
                }
            }
        }
        // If we were unable to read anything out of the archive don't
        // bother trying to roundtrip it.
        if (len(files) == 0) {
            return;
        }
        var w = NewWriter(io.Discard);
        foreach (var (_, fΔ2) in files) {
            var (ww, errΔ5) = w.CreateHeader(fΔ2.header);
            if (errΔ5 != default!) {
                t.Fatalf("unable to write previously parsed header: %s"u8, errΔ5);
            }
            {
                var (_, errΔ6) = ww.Write(fΔ2.content); if (errΔ6 != default!) {
                    t.Fatalf("unable to write previously parsed content: %s"u8, errΔ6);
                }
            }
        }
        {
            var errΔ7 = w.Close(); if (errΔ7 != default!) {
                t.Fatalf("Unable to write archive: %s"u8, errΔ7);
            }
        }
    });
}

// TODO: We may want to check if the archive roundtrips.

} // end zip_internal_test_package
