// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using bytes = bytes_package;
using io = io_package;
using testing = testing_package;
using static go.archive.tar_package;

partial class tar_internal_test_package {

internal partial struct FuzzReader_file /*dyn*/ {
    internal ж<global::go.archive.tar_package.Header> header;
    internal slice<byte> content;
}

public static void FuzzReader(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var b = bytes.NewBuffer(default!);
    var w = NewWriter(new tar_test_package.bytes_BufferжWriter(b));
    var inp = slice<byte>("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."u8);
    var err = w.WriteHeader(Ꮡ(new Header(
        Name: "lorem.txt"u8,
        Mode: 384,
        Size: (int64)len(inp)
    )));
    if (err != default!) {
        Ꮡf.Fatalf("failed to create writer: %s"u8, err);
    }
    (_, err) = w.Write(inp);
    if (err != default!) {
        Ꮡf.Fatalf("failed to write file to archive: %s"u8, err);
    }
    {
        var errΔ1 = w.Close(); if (errΔ1 != default!) {
            Ꮡf.Fatalf("failed to write archive: %s"u8, errΔ1);
        }
    }
    f.Add(b.Bytes());
    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> bΔ1) => {
        var r = NewReader(new tar_test_package.bytes_ReaderжReader(bytes.NewReader(bΔ1)));
        var files = new FuzzReader_file[]{}.slice();
        while (ᐧ) {
            var (hdr, errΔ2) = r.Next();
            if (AreEqual(errΔ2, io.EOF)) {
                break;
            }
            if (errΔ2 != default!) {
                return;
            }
            var buf = bytes.NewBuffer(default!);
            {
                var (_, errΔ3) = io.Copy(new tar_test_package.bytes_BufferжWriter(buf), new global::go.archive.tar_package.ReaderжReader(r)); if (errΔ3 != default!) {
                    continue;
                }
            }
            files = append(files, new FuzzReader_file(header: hdr, content: buf.Bytes()));
        }
        // If we were unable to read anything out of the archive don't
        // bother trying to roundtrip it.
        if (len(files) == 0) {
            return;
        }
        var @out = bytes.NewBuffer(default!);
        var wΔ1 = NewWriter(new tar_test_package.bytes_BufferжWriter(@out));
        foreach (var (_, fΔ1) in files) {
            {
                var errΔ4 = wΔ1.WriteHeader(fΔ1.header); if (errΔ4 != default!) {
                    t.Fatalf("unable to write previously parsed header: %s"u8, errΔ4);
                }
            }
            {
                var (_, errΔ5) = wΔ1.Write(fΔ1.content); if (errΔ5 != default!) {
                    t.Fatalf("unable to write previously parsed content: %s"u8, errΔ5);
                }
            }
        }
        {
            var errΔ6 = wΔ1.Close(); if (errΔ6 != default!) {
                t.Fatalf("Unable to write archive: %s"u8, errΔ6);
            }
        }
    });
}

// TODO: We may want to check if the archive roundtrips. This would require
// taking into account addition of the two zero trailer blocks that Writer.Close
// appends.

} // end tar_internal_test_package
