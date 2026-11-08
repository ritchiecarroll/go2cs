// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using rand = go.crypto.rand_package;
using static go.crypto.subtle_package;
using fmt = fmt_package;
using testing = testing_package;
using go.crypto;
using static go.crypto.subtle_internal_test_package;

partial class subtle_test_package {

public static void TestXORBytes(ж<testing.T> t) {
    for (nint n = 1; n <= 1024; n++) {
        if (n > 16 && testing.Short()) {
            n += (n >> 3);
        }
        for (nint alignP = 0; alignP < 8; alignP++) {
            for (nint alignQ = 0; alignQ < 8; alignQ++) {
                for (nint alignD = 0; alignD < 8; alignD++) {
                    var p = new slice<byte>(alignP + n, alignP + n + 100).slice(alignP);
                    var q = new slice<byte>(alignQ + n, alignQ + n + 100).slice(alignQ);
                    if ((n & 1) != 0) {
                        p = p.slice(0, n);
                    } else {
                        q = q.slice(0, n);
                    }
                    rand.Read(p);
                    rand.Read(q);

                    var d = new slice<byte>(alignD + n + 100);
                    rand.Read(d);

                    var want = bytes.Clone(d);
                    foreach (var i in range(n)) {
                        want[alignD + i] = (byte)(p[i] ^ q[i]);
                    }

                    {
                        nint nn = XORBytes(d.slice(alignD), p, q); if (!bytes.Equal(d, want)) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d:\n\tp = %x\n\tq = %x\n\td = %x\n\twant %x\n", n, alignP, alignQ, alignD, p, q, d, want);
                        } else if (nn != n) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d: got %d, want %d", n, alignP, alignQ, alignD, nn, n);
                        }
                    }
                    var p1 = bytes.Clone(p);
                    {
                        nint nn = XORBytes(p, p, q); if (!bytes.Equal(p, want.slice(alignD, alignD + n))) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d:\n\tp = %x\n\tq = %x\n\td = %x\n\twant %x\n", n, alignP, alignQ, alignD, p, q, d, want);
                        } else if (nn != n) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d: got %d, want %d", n, alignP, alignQ, alignD, nn, n);
                        }
                    }
                    {
                        nint nn = XORBytes(q, p1, q); if (!bytes.Equal(q, want.slice(alignD, alignD + n))) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d:\n\tp = %x\n\tq = %x\n\td = %x\n\twant %x\n", n, alignP, alignQ, alignD, p, q, d, want);
                        } else if (nn != n) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d: got %d, want %d", n, alignP, alignQ, alignD, nn, n);
                        }
                    }

                    {
                        nint nn = XORBytes(p, p, p); if (!bytes.Equal(p, new slice<byte>(n))) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d: got %x, want %x", n, alignP, alignQ, alignD, p, new slice<byte>(n));
                        } else if (nn != n) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d: got %d, want %d", n, alignP, alignQ, alignD, nn, n);
                        }
                    }
                    {
                        nint nn = XORBytes(p1, q, q); if (!bytes.Equal(p1, new slice<byte>(n))) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d: got %x, want %x", n, alignP, alignQ, alignD, p1, new slice<byte>(n));
                        } else if (nn != n) {
                            t.Errorf("n=%d alignP=%d alignQ=%d alignD=%d: got %d, want %d", n, alignP, alignQ, alignD, nn, n);
                        }
                    }
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string subtleXORBytesDstTooˢ => ref subtleXORBytesDstTooˢᵇ;
private static readonly @string subtleXORBytesDstTooˢᵇ = "subtle.XORBytes: dst too short"u8;
internal static ref readonly @string subtleXORBytesInvalidˢ => ref subtleXORBytesInvalidˢᵇ;
private static readonly @string subtleXORBytesInvalidˢᵇ = "subtle.XORBytes: invalid overlap"u8;

public static void TestXorBytesPanic(ж<testing.T> t) {
    mustPanic(t, subtleXORBytesDstTooˢ, () => {
        XORBytes(default, new slice<byte>(1), new slice<byte>(1));
    });
    mustPanic(t, subtleXORBytesDstTooˢ, () => {
        XORBytes(new slice<byte>(1), new slice<byte>(2), new slice<byte>(3));
    });
    mustPanic(t, subtleXORBytesInvalidˢ, () => {
        var x = new slice<byte>(3);
        XORBytes(x, x[1..], new slice<byte>(2));
    });
    mustPanic(t, subtleXORBytesInvalidˢ, () => {
        var x = new slice<byte>(3);
        XORBytes(x, new slice<byte>(2), x[1..]);
    });
}

public static void BenchmarkXORBytes(ж<testing.B> b) {
    var dst = new slice<byte>((1 << 15));
    var data0 = new slice<byte>((1 << 15));
    var data1 = new slice<byte>((1 << 15));
    var sizes = slice<int64>([(int64)1 << 3, (int64)1 << 7, (int64)1 << 11, (int64)1 << 13, (int64)1 << 15]);
    foreach (var (_, size) in range(sizes)) {
        var data0ʗ1 = data0;
        var data1ʗ1 = data1;
        var dstʗ1 = dst;
        b.Run(fmt.Sprintf("%dBytes"u8, size), (ж<testing.B> bΔ1) => {
            var s0 = data0ʗ1.slice(0, (nint)(size));
            var s1 = data1ʗ1.slice(0, (nint)(size));
            bΔ1.SetBytes((int64)size);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                XORBytes(dstʗ1, s0, s1);
            }
        });
    }
}

public static void BenchmarkXORBytesAlignment(ж<testing.B> b) {
    var dst = new slice<byte>(8 + (1 << 11));
    var data0 = new slice<byte>(8 + (1 << 11));
    var data1 = new slice<byte>(8 + (1 << 11));
    var sizes = slice<int64>([(int64)1 << 3, (int64)1 << 7, (int64)1 << 11]);
    foreach (var (_, size) in range(sizes)) {
        for (var offsetᴛ1 = (int64)0; offsetᴛ1 < 8; offsetᴛ1++) {
            var offset = offsetᴛ1;
            var data0ʗ1 = data0;
            var data1ʗ1 = data1;
            var dstʗ1 = dst;
            b.Run(fmt.Sprintf("%dBytes%dOffset"u8, size, offset), (ж<testing.B> bΔ1) => {
                var d = dstʗ1.slice((nint)(offset), (nint)(offset + size));
                var s0 = data0ʗ1.slice((nint)(offset), (nint)(offset + size));
                var s1 = data1ʗ1.slice((nint)(offset), (nint)(offset + size));
                bΔ1.SetBytes((int64)size);
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    XORBytes(d, s0, s1);
                }
            });
        }
    }
}

internal static void mustPanic(ж<testing.T> t, @string expected, Action f) {
    GoFrame ᒐ = default;
    try {
        t.Helper();
        defer(() => {
            t.Helper();
            var switchᴛ1 = recover();
            switch (switchᴛ1.type()) {
            case null: {
                t.Errorf("expected panic(%q), but did not panic", expected);
                break;
            }
            case @string msg: {
                if (msg != expected) {
                    t.Errorf("expected panic(%q), but got panic(%q)", expected, msg);
                }
                break;
            }
            default: {
                var msg = switchᴛ1;
                t.Errorf("expected panic(%q), but got panic(%T%v)", expected, msg, msg);
                break;
            }}
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end subtle_test_package
