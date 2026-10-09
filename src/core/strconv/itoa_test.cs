// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static strconv_package;
using testing = testing_package;
using static go.strconv_internal_test_package;

partial class strconv_test_package {

partial struct itob64Test {
    internal int64 @in;
    internal nint @base;
    internal @string @out;
}

internal static slice<itob64Test> itob64tests = new itob64Test[]{
    new(0, 10, "0"u8),
    new(1, 10, "1"u8),
    new(-1, 10, "-1"u8),
    new(12345678, 10, "12345678"u8),
    new(-987654321, 10, "-987654321"u8),
    new(2147483648L - 1, 10, "2147483647"u8),
    new((-1 << (int)(31)) + 1, 10, "-2147483647"u8),
    new(2147483648L, 10, "2147483648"u8),
    new(((int64)(-1) << (int)(31)), 10, "-2147483648"u8),
    new(2147483649L, 10, "2147483649"u8),
    new(-2147483649L, 10, "-2147483649"u8),
    new(4294967295L, 10, "4294967295"u8),
    new(-4294967295L, 10, "-4294967295"u8),
    new(4294967296L, 10, "4294967296"u8),
    new(-4294967296L, 10, "-4294967296"u8),
    new(4294967297L, 10, "4294967297"u8),
    new(-4294967297L, 10, "-4294967297"u8),
    new(1125899906842624L, 10, "1125899906842624"u8),
    new(9223372036854775807L, 10, "9223372036854775807"u8),
    new(-9223372036854775807L, 10, "-9223372036854775807"u8),
    new(-9223372036854775808L, 10, "-9223372036854775808"u8),
    new(0, 2, "0"u8),
    new(10, 2, "1010"u8),
    new(-1, 2, "-1"u8),
    new(((int64)1 << (int)(15)), 2, "1000000000000000"u8),
    new(-8, 8, "-10"u8),
    new(6416645477L, 8, "57635436545"u8),
    new(((int64)1 << (int)(24)), 8, "100000000"u8),
    new(16, 16, "10"u8),
    new(-0x123456789abcdefL, 16, "-123456789abcdef"u8),
    new(9223372036854775807L, 16, "7fffffffffffffff"u8),
    new(9223372036854775807L, 2, "111111111111111111111111111111111111111111111111111111111111111"u8),
    new(-9223372036854775808L, 2, "-1000000000000000000000000000000000000000000000000000000000000000"u8),
    new(16, 17, "g"u8),
    new(25, 25, "10"u8),
    new(32544027072L, 35, "holycow"u8),
    new(38493362624L, 36, "holycow"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object abcˢ = (@string)"abc"u8;

public static void TestItoa(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, test) in itob64tests) {
            @string s = FormatInt(test.@in, test.@base);
            if (s != test.@out) {
                Ꮡt.Errorf("FormatInt(%v, %v) = %v want %v"u8,
                    test.@in, test.@base, s, test.@out);
            }
            var x = AppendInt(slice<byte>("abc"u8), test.@in, test.@base);
            if (((@string)x) != "abc"u8 + test.@out) {
                Ꮡt.Errorf("AppendInt(%q, %v, %v) = %q want %v"u8,
                    abcˢ, test.@in, test.@base, x, test.@out);
            }
            if (test.@in >= 0) {
                @string sΔ1 = FormatUint((uint64)test.@in, test.@base);
                if (sΔ1 != test.@out) {
                    Ꮡt.Errorf("FormatUint(%v, %v) = %v want %v"u8,
                        test.@in, test.@base, sΔ1, test.@out);
                }
                var xΔ1 = AppendUint(default!, (uint64)test.@in, test.@base);
                if (((sstring)xΔ1) != test.@out) {
                    Ꮡt.Errorf("AppendUint(%q, %v, %v) = %q want %v"u8,
                        abcˢ, (uint64)test.@in, test.@base, xΔ1, test.@out);
                }
            }
            if (test.@base == 10 && (int64)(nint)test.@in == test.@in) {
                @string sΔ2 = Itoa((nint)test.@in);
                if (sΔ2 != test.@out) {
                    Ꮡt.Errorf("Itoa(%v) = %v want %v"u8,
                        test.@in, sΔ2, test.@out);
                }
            }
        }
        // Override when base is illegal
        defer(() => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatalf("expected panic due to illegal base"u8);
                }
            }
        }, ref ᒐ);
        FormatUint(12345678, 1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct uitob64Test {
    internal uint64 @in;
    internal nint @base;
    internal @string @out;
}

internal static slice<uitob64Test> uitob64tests = new uitob64Test[]{
    new(9223372036854775807UL, 10, "9223372036854775807"u8),
    new(((uint64)1 << (int)(63)), 10, "9223372036854775808"u8),
    new(9223372036854775809UL, 10, "9223372036854775809"u8),
    new(18446744073709551614UL, 10, "18446744073709551614"u8),
    new(18446744073709551615UL, 10, "18446744073709551615"u8),
    new(18446744073709551615UL, 2, "1111111111111111111111111111111111111111111111111111111111111111"u8)
}.slice();

public static void TestUitoa(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in uitob64tests) {
        @string s = FormatUint(test.@in, test.@base);
        if (s != test.@out) {
            Ꮡt.Errorf("FormatUint(%v, %v) = %v want %v"u8,
                test.@in, test.@base, s, test.@out);
        }
        var x = AppendUint(slice<byte>("abc"u8), test.@in, test.@base);
        if (((@string)x) != "abc"u8 + test.@out) {
            Ꮡt.Errorf("AppendUint(%q, %v, %v) = %q want %v"u8,
                abcˢ, test.@in, test.@base, x, test.@out);
        }
    }
}


partial struct varlenUintsᴛ1 /*dyn*/ {
    internal uint64 @in;
    internal @string @out;
}
internal static slice<varlenUintsᴛ1> varlenUints = new varlenUintsᴛ1[]{
    new(1, "1"u8),
    new(12, "12"u8),
    new(123, "123"u8),
    new(1234, "1234"u8),
    new(12345, "12345"u8),
    new(123456, "123456"u8),
    new(1234567, "1234567"u8),
    new(12345678, "12345678"u8),
    new(123456789, "123456789"u8),
    new(1234567890, "1234567890"u8),
    new(12345678901UL, "12345678901"u8),
    new(123456789012UL, "123456789012"u8),
    new(1234567890123UL, "1234567890123"u8),
    new(12345678901234UL, "12345678901234"u8),
    new(123456789012345UL, "123456789012345"u8),
    new(1234567890123456UL, "1234567890123456"u8),
    new(12345678901234567UL, "12345678901234567"u8),
    new(123456789012345678UL, "123456789012345678"u8),
    new(1234567890123456789UL, "1234567890123456789"u8),
    new(12345678901234567890UL, "12345678901234567890"u8)
}.slice();

public static void TestFormatUintVarlen(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in varlenUints) {
        @string s = FormatUint(test.@in, 10);
        if (s != test.@out) {
            Ꮡt.Errorf("FormatUint(%v, 10) = %v want %v"u8, test.@in, s, test.@out);
        }
    }
}

public static void BenchmarkFormatInt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, test) in itob64tests) {
            @string s = FormatInt(test.@in, test.@base);
            BenchSink += len(s);
        }
    }
}

public static void BenchmarkAppendInt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var dst = new slice<byte>(0, 30);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, test) in itob64tests) {
            dst = AppendInt(dst[..0], test.@in, test.@base);
            BenchSink += len(dst);
        }
    }
}

public static void BenchmarkFormatUint(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, test) in uitob64tests) {
            @string s = FormatUint(test.@in, test.@base);
            BenchSink += len(s);
        }
    }
}

public static void BenchmarkAppendUint(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var dst = new slice<byte>(0, 30);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, test) in uitob64tests) {
            dst = AppendUint(dst[..0], test.@in, test.@base);
            BenchSink += len(dst);
        }
    }
}

public static void BenchmarkFormatIntSmall(ж<testing.B> Ꮡb) {
    var smallInts = new int64[]{7, 42}.slice();
    foreach (var (_, smallInt) in smallInts) {
        Ꮡb.Run(Itoa((nint)smallInt), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                @string s = FormatInt(smallInt, 10);
                BenchSink += len(s);
            }
        });
    }
}

public static void BenchmarkAppendIntSmall(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var dst = new slice<byte>(0, 30);
    const int64 smallInt = 42;
    for (nint i = 0; i < b.N; i++) {
        dst = AppendInt(dst[..0], smallInt, 10);
        BenchSink += len(dst);
    }
}

public static void BenchmarkAppendUintVarlen(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in varlenUints) {
        ref var test = ref heap(new varlenUintsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡb.Run(test.@out, (ж<testing.B> bΔ1) => {
            var dst = new slice<byte>(0, 30);
            for (nint j = 0; j < (~bΔ1).N; j++) {
                dst = AppendUint(dst[..0], testʗ1.@in, 10);
                BenchSink += len(dst);
            }
        });
    }
}

public static nint BenchSink; // make sure compiler cannot optimize away benchmarks

} // end strconv_test_package
