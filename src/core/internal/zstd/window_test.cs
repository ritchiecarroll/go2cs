// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using testing = testing_package;
using static go.@internal.zstd_package;

partial class zstd_internal_test_package {

internal static slice<byte> /*seq*/ makeSequence(nint start, nint n) {
    slice<byte> seq = default!;

    for (nint i = 0; i < n; i++) {
        seq = append(seq, (byte)(start + i));
    }
    return seq;
}

public static void TestWindow(ж<testing.T> Ꮡt) {
    for (nint sizeᴛ1 = 0; sizeᴛ1 <= 3; sizeᴛ1++) {
        var size = sizeᴛ1;
        for (nint i = 0; i <= 2 * size; i++) {
            var a = makeSequence((rune)'a', i);
            for (nint j = 0; j <= 2 * size; j++) {
                var b = makeSequence((rune)'a' + i, j);
                for (nint k = 0; k <= 2 * size; k++) {
                    var c = makeSequence((rune)'a' + i + j, k);
                    var aʗ1 = a;
                    var bʗ1 = b;
                    var cʗ1 = c;
                    Ꮡt.Run(fmt.Sprintf("%d-%d-%d-%d"u8, size, i, j, k), (ж<testing.T> tΔ1) => {
                        testWindow(tΔ1, size, aʗ1, bʗ1, cʗ1);
                    });
                }
            }
        }
    }
}

// testWindow tests window by saving three sequences of bytes to it.
// Third sequence tests read offset that can become non-zero only after second save.
internal static void testWindow(ж<testing.T> Ꮡt, nint size, slice<byte> a, slice<byte> b, slice<byte> c) {
    global::go.@internal.zstd_package.window w = default!;
    w.reset(size);
    w.save(a);
    w.save(b);
    w.save(c);
    slice<byte> tail = default!;
    tail = appendꓸꓸꓸ(tail, a);
    tail = appendꓸꓸꓸ(tail, b);
    tail = appendꓸꓸꓸ(tail, c);
    if (builtin.len(tail) > size) {
        tail = tail.slice(builtin.len(tail) - size);
    }
    if (w.len() != (uint32)builtin.len(tail)) {
        Ꮡt.Errorf("wrong data length: got: %d, want: %d"u8, w.len(), builtin.len(tail));
    }
    uint32 from = default!;
    uint32 to = default!;
    for (from = 0; from <= (uint32)builtin.len(tail); from++) {
        for (to = from; to <= (uint32)builtin.len(tail); to++) {
            var got = w.appendTo(default!, from, to);
            var want = tail.slice((nint)(from), (nint)(to));
            if (!bytes.Equal(got, want)) {
                Ꮡt.Errorf("wrong data at [%d:%d]: got %q, want %q"u8, from, to, got, want);
            }
        }
    }
}

} // end zstd_internal_test_package
