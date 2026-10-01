// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

public static void TestBitCursor(ж<testing.T> Ꮡt) {
    ref var ones = ref heap<array<byte>>(out var Ꮡones);
    ones = new byte[]{0xff, 0xff, 0xff, 0xff, 0xff}.array();
    ref var zeros = ref heap<array<byte>>(out var Ꮡzeros);
    zeros = new byte[]{0, 0, 0, 0, 0}.array();
    for (var start = (uintptr)0; start < 16; start++) {
        for (var end = start + 1; end < 32; end++) {
            ref var buf = ref heap<array<byte>>(out var Ꮡbuf);
            buf = zeros.Clone();
            runtime_internal_test_package.NewBitCursor(Ꮡbuf.at<byte>(0)).Offset(start).Write(Ꮡones.at<byte>(0), end - start);
            for (var i = (uintptr)0; i < (uintptr)(len(buf) * 8); i++) {
                var bit = (byte)((buf[i / 8] >> (int)((i % 8))) & 1);
                if (bit == 0 && i >= start && i < end) {
                    Ꮡt.Errorf("bit %d not set in [%d:%d]"u8, i, start, end);
                }
                if (bit == 1 && (i < start || i >= end)) {
                    Ꮡt.Errorf("bit %d is set outside [%d:%d]"u8, i, start, end);
                }
            }
        }
    }
    for (var start = (uintptr)0; start < 16; start++) {
        for (var end = start + 1; end < 32; end++) {
            ref var buf = ref heap<array<byte>>(out var Ꮡbuf);
            buf = ones.Clone();
            runtime_internal_test_package.NewBitCursor(Ꮡbuf.at<byte>(0)).Offset(start).Write(Ꮡzeros.at<byte>(0), end - start);
            for (var i = (uintptr)0; i < (uintptr)(len(buf) * 8); i++) {
                var bit = (byte)((buf[i / 8] >> (int)((i % 8))) & 1);
                if (bit == 1 && i >= start && i < end) {
                    Ꮡt.Errorf("bit %d not cleared in [%d:%d]"u8, i, start, end);
                }
                if (bit == 0 && (i < start || i >= end)) {
                    Ꮡt.Errorf("bit %d cleared outside [%d:%d]"u8, i, start, end);
                }
            }
        }
    }
}

} // end runtime_test_package
