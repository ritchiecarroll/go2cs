// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

public static void TestBitCursor(ж<testing.T> t) {
    ref var ones = ref heap<array<byte>>(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff}.array(), out var Ꮡones);
    ref var zeros = ref heap<array<byte>>(new byte[]{0, 0, 0, 0, 0}.array(), out var Ꮡzeros);

    for (var start = (uintptr)0; start < 16; start++) {
        for (var end = start + 1; end < 32; end++) {
            ref var buf = ref heap<array<byte>>(zeros.Clone(), out var Ꮡbuf);
            runtime_internal_test_package.NewBitCursor(Ꮡbuf.at<byte>(0)).Offset(start).Write(Ꮡones.at<byte>(0), end - start);

            for (var i = (uintptr)0; i < (uintptr)(len(buf) * 8); i++) {
                var bit = (byte)(buf[i / 8] >> (int)(i % 8) & 1);
                if (bit == 0 && i >= start && i < end) {
                    t.Errorf("bit %d not set in [%d:%d]", i, start, end);
                }
                if (bit == 1 && (i < start || i >= end)) {
                    t.Errorf("bit %d is set outside [%d:%d]", i, start, end);
                }
            }
        }
    }

    for (var start = (uintptr)0; start < 16; start++) {
        for (var end = start + 1; end < 32; end++) {
            ref var buf = ref heap<array<byte>>(ones.Clone(), out var Ꮡbuf);
            runtime_internal_test_package.NewBitCursor(Ꮡbuf.at<byte>(0)).Offset(start).Write(Ꮡzeros.at<byte>(0), end - start);

            for (var i = (uintptr)0; i < (uintptr)(len(buf) * 8); i++) {
                var bit = (byte)(buf[i / 8] >> (int)(i % 8) & 1);
                if (bit == 1 && i >= start && i < end) {
                    t.Errorf("bit %d not cleared in [%d:%d]", i, start, end);
                }
                if (bit == 0 && (i < start || i >= end)) {
                    t.Errorf("bit %d cleared outside [%d:%d]", i, start, end);
                }
            }
        }
    }
}

} // end runtime_test_package
