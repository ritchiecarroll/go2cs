// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using testing = testing_package;
using static go.debug.buildinfo_package;

partial class buildinfo_internal_test_package {

internal partial struct byteExe {
    internal slice<byte> b;
}

internal static (io.ReaderAt, error) DataReader(this ref byteExe x, uint64 addr) {
    if (addr >= (uint64)len(x.b)) {
        return (default, fmt.Errorf("ReadData(%d) out of bounds of %d-byte slice"u8, addr, len(x.b)));
    }
    return (new buildinfo_test_package.bytes_ReaderжReaderAt(bytes.NewReader(x.b.slice((nint)(addr)))), default);
}

internal static (uint64, uint64) DataStart(this ref byteExe x) {
    return (0, (uint64)len(x.b));
}

internal partial struct TestSearchMagic_tests /*dyn*/ {
    internal @string name;
    internal slice<byte> data;
    internal uint64 want;
    internal error wantErr;
}

public static void TestSearchMagic(ж<testing.T> t) {
    var tests = slice<TestSearchMagic_tests>([
        new(
            name: "beginning",
            data: func(slice<byte> () => {
                var b = new slice<byte>(buildInfoHeaderSize);
                copy(b, buildInfoMagic);
                return b;
            })(),
            want: 0
        ),
        new(
            name: "offset",
            data: func(slice<byte> () => {
                var b = new slice<byte>(512);
                copy(b[(int)(4 * buildInfoAlign)..], buildInfoMagic);
                return b;
            })(),
            want: 4 * buildInfoAlign
        ),
        new(
            name: "second_chunk",
            data: func(slice<byte> () => {
                var b = new slice<byte>(4 * searchChunkSize);
                copy(b[(int)(searchChunkSize + 4 * buildInfoAlign)..], buildInfoMagic);
                return b;
            })(),
            want: searchChunkSize + 4 * buildInfoAlign
        ),
        new(
            name: "second_chunk_short",
            data: func(slice<byte> () => {
                // Magic is 64-bytes into the second chunk,
                // which is short; only exactly long enough to
                // hold the header.
                var b = new slice<byte>(searchChunkSize + 4 * buildInfoAlign + buildInfoHeaderSize);
                copy(b[(int)(searchChunkSize + 4 * buildInfoAlign)..], buildInfoMagic);
                return b;
            })(),
            want: searchChunkSize + 4 * buildInfoAlign
        ),
        new(
            name: "missing",
            data: func(slice<byte> () => {
                var b = new slice<byte>(buildInfoHeaderSize);
                return b;
            })(),
            wantErr: errNotGoExe
        ),
        new(
            name: "too_short",
            data: func(slice<byte> () => {
                // There needs to be space for the entire
                // header, not just the magic.
                var b = new slice<byte>(len(buildInfoMagic));
                copy(b, buildInfoMagic);
                return b;
            })(),
            wantErr: errNotGoExe
        ),
        new(
            name: "misaligned",
            data: func(slice<byte> () => {
                var b = new slice<byte>(512);
                copy(b[7..], buildInfoMagic);
                return b;
            })(),
            wantErr: errNotGoExe
        ),
        new(
            name: "misaligned_across_chunk",
            data: func(slice<byte> () => {
                // Magic crosses chunk boundary. By definition,
                // it has to be misaligned.
                var b = new slice<byte>(2 * searchChunkSize);
                copy(b[(int)(searchChunkSize - 8)..], buildInfoMagic);
                return b;
            })(),
            wantErr: errNotGoExe
        ),
        new(
            name: "header_across_chunk",
            data: func(slice<byte> () => {
                // The magic is aligned within the first chunk,
                // but the rest of the 32-byte header crosses
                // the chunk boundary.
                var b = new slice<byte>(2 * searchChunkSize);
                copy(b[(int)(searchChunkSize - buildInfoAlign)..], buildInfoMagic);
                return b;
            })(),
            want: searchChunkSize - buildInfoAlign
        )
    ]);
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestSearchMagic_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var x = Ꮡ(new byteExe(tcʗ1.data));
            var (dataAddr, dataSize) = x.DataStart();
            var (addr, err) = searchMagic(new buildinfo_internal_test_package.byteExeжexe(x), dataAddr, dataSize);
            if (tcʗ1.wantErr == nil) {
                if (err != nil) {
                    tΔ1.Errorf("searchMagic got err %v want nil", err);
                }
                if (addr != tcʗ1.want) {
                    tΔ1.Errorf("searchMagic got addr %d want %d", addr, tcʗ1.want);
                }
            } else {
                if (!AreEqual(err, tcʗ1.wantErr)) {
                    tΔ1.Errorf("searchMagic got err %v want %v", err, tcʗ1.wantErr);
                }
            }
        });
    }
}

} // end buildinfo_internal_test_package
