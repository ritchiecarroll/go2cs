// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using static strconv_package;
using strings = strings_package;
using testing = testing_package;
using static go.strconv_internal_test_package;
using strconv = strconv_package;

partial class strconv_test_package {

internal static array<byte> globalBuf = new(64);
internal static @string nextToOne = "1.00000000000000011102230246251565404236316680908203125"u8 + strings.Repeat("0"u8, 10000) + "1"u8;
// In practice we see 7 for the next one, but allow some slop.
// Before pre-allocation in appendQuotedWith, we saw 39.

partial struct mallocTestᴛ1 /*dyn*/ {
    internal nint count;
    internal @string desc;
    internal Action fn;
}
internal static slice<mallocTestᴛ1> mallocTest;
internal static void initᴛmallocTest() { mallocTest = new mallocTestᴛ1[]{
    new(0, @"AppendInt(localBuf[:0], 123, 10)"u8, () => {
        array<byte> localBuf = new(64);
        AppendInt(localBuf[..0], 123, 10);
    }),
    new(0, @"AppendInt(globalBuf[:0], 123, 10)"u8, () => {
        AppendInt(globalBuf[..0], 123, 10);
    }),
    new(0, @"AppendFloat(localBuf[:0], 1.23, 'g', 5, 64)"u8, () => {
        array<byte> localBuf = new(64);
        AppendFloat(localBuf[..0], 1.23D, (rune)'g', 5, 64);
    }),
    new(0, @"AppendFloat(globalBuf[:0], 1.23, 'g', 5, 64)"u8, () => {
        AppendFloat(globalBuf[..0], 1.23D, (rune)'g', 5, 64);
    }),
    new(10, @"AppendQuoteToASCII(nil, oneMB)"u8, () => {
        AppendQuoteToASCII(default!, ((@string)oneMB));
    }),
    new(0, @"ParseFloat(""123.45"", 64)"u8, () => {
        ParseFloat("123.45"u8, 64);
    }),
    new(0, @"ParseFloat(""123.456789123456789"", 64)"u8, () => {
        ParseFloat("123.456789123456789"u8, 64);
    }),
    new(0, @"ParseFloat(""1.000000000000000111022302462515654042363166809082031251"", 64)"u8, () => {
        ParseFloat("1.000000000000000111022302462515654042363166809082031251"u8, 64);
    }),
    new(0, @"ParseFloat(""1.0000000000000001110223024625156540423631668090820312500...001"", 64)"u8, () => {
        ParseFloat(nextToOne, 64);
    })
}.slice(); }

internal static slice<byte> oneMB; // Will be allocated to 1MB of random data by TestCountMallocs.

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

public static void TestCountMallocs(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        Ꮡt.Skip(skippingGomaxprocs1ˢ);
    }
    // Allocate a big messy buffer for AppendQuoteToASCII's test.
    oneMB = new slice<byte>(1000000);
    foreach (var (i, _) in oneMB) {
        oneMB[i] = (byte)i;
    }
    foreach (var (_, mt) in mallocTest) {
        var allocs = testing.AllocsPerRun(100, mt.fn);
        {
            var max = (float64)mt.count; if (allocs > max) {
                Ꮡt.Errorf("%s: %v allocs, want <=%v"u8, mt.desc, allocs, max);
            }
        }
    }
}

// Sink makes sure the compiler cannot optimize away the benchmarks.

partial struct Sinkᴛ1 /*dyn*/ {
    public bool Bool;
    public nint Int;
    public int64 Int64;
    public uint64 Uint64;
    public float64 Float64;
    public complex128 Complex128;
    public error Error;
    public slice<byte> Bytes;
}
public static Sinkᴛ1 Sink;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string atoiˢ = "Atoi"u8;
internal static readonly @string parseBoolˢ = "ParseBool"u8;
internal static readonly @string parseFloatˢ = "ParseFloat"u8;
internal static readonly @string parseComplexˢ = "ParseComplex"u8;
internal static readonly @string canBackquoteˢ = "CanBackquote"u8;
internal static readonly @string appendQuoteˢ = "AppendQuote"u8;
internal static readonly @string appendQuoteToASCIIˢ = "AppendQuoteToASCII"u8;
internal static readonly @string appendQuoteToGraphicˢ = "AppendQuoteToGraphic"u8;

internal partial struct TestAllocationsFromBytes_bytes /*dyn*/ {
    public slice<byte> Bool, Number, String, Buffer;
}

public static void TestAllocationsFromBytes(ж<testing.T> Ꮡt) {
    const nint runsPerTest = 100;
    ref var bytes = ref heap<TestAllocationsFromBytes_bytes>(out var Ꮡbytes);
    bytes = new TestAllocationsFromBytes_bytes(
        Bool: slice<byte>("false"u8),
        Number: slice<byte>("123456789"u8),
        String: slice<byte>("hello, world!"u8),
        Buffer: new slice<byte>(1024)
    );
    Action<ж<testing.T>> checkNoAllocs(Action f) => (ж<testing.T> tΔ1) => {
            tΔ1.Helper();
            {
                var allocs = testing.AllocsPerRun(runsPerTest, f); if (allocs != 0D) {
                    tΔ1.Errorf("got %v allocs, want 0 allocs"u8, allocs);
                }
            }
        };
    var bytesʗ1 = bytes;
    Ꮡt.Run(atoiˢ, checkNoAllocs(() => {
        (Sink.Int, Sink.Error) = Atoi(((@string)bytesʗ1.Number));
    }));
    var bytesʗ2 = bytes;
    Ꮡt.Run(parseBoolˢ, checkNoAllocs(() => {
        (Sink.Bool, Sink.Error) = ParseBool(((@string)bytesʗ2.Bool));
    }));
    var bytesʗ3 = bytes;
    Ꮡt.Run(parseIntˢ, checkNoAllocs(() => {
        (Sink.Int64, Sink.Error) = ParseInt(((@string)bytesʗ3.Number), 10, 64);
    }));
    var bytesʗ4 = bytes;
    Ꮡt.Run(parseUintˢ, checkNoAllocs(() => {
        (Sink.Uint64, Sink.Error) = ParseUint(((@string)bytesʗ4.Number), 10, 64);
    }));
    var bytesʗ5 = bytes;
    Ꮡt.Run(parseFloatˢ, checkNoAllocs(() => {
        (Sink.Float64, Sink.Error) = ParseFloat(((@string)bytesʗ5.Number), 64);
    }));
    var bytesʗ6 = bytes;
    Ꮡt.Run(parseComplexˢ, checkNoAllocs(() => {
        (Sink.Complex128, Sink.Error) = ParseComplex(((@string)bytesʗ6.Number), 128);
    }));
    var bytesʗ7 = bytes;
    Ꮡt.Run(canBackquoteˢ, checkNoAllocs(() => {
        Sink.Bool = CanBackquote(((@string)bytesʗ7.String));
    }));
    var bytesʗ8 = bytes;
    Ꮡt.Run(appendQuoteˢ, checkNoAllocs(() => {
        Sink.Bytes = AppendQuote(bytesʗ8.Buffer[..0], ((@string)bytesʗ8.String));
    }));
    var bytesʗ9 = bytes;
    Ꮡt.Run(appendQuoteToASCIIˢ, checkNoAllocs(() => {
        Sink.Bytes = AppendQuoteToASCII(bytesʗ9.Buffer[..0], ((@string)bytesʗ9.String));
    }));
    var bytesʗ10 = bytes;
    Ꮡt.Run(appendQuoteToGraphicˢ, checkNoAllocs(() => {
        Sink.Bytes = AppendQuoteToGraphic(bytesʗ10.Buffer[..0], ((@string)bytesʗ10.String));
    }));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidˢ = "INVALID"u8;

internal partial struct TestErrorPrefixes_vectors /*dyn*/ {
    internal error err;  // Input error
    internal @string want; // Function name wanted
}

public static void TestErrorPrefixes(ж<testing.T> Ꮡt) {
    var (_, errInt) = Atoi(invalidˢ);
    var (_, errBool) = ParseBool(invalidˢ);
    var (_, errFloat) = ParseFloat(invalidˢ, 64);
    var (_, errInt64) = ParseInt(invalidˢ, 10, 64);
    var (_, errUint64) = ParseUint(invalidˢ, 10, 64);
    var vectors = new TestErrorPrefixes_vectors[]{
        new(errInt, "Atoi"u8),
        new(errBool, "ParseBool"u8),
        new(errFloat, "ParseFloat"u8),
        new(errInt64, "ParseInt"u8),
        new(errUint64, "ParseUint"u8)
    }.slice();
    foreach (var (_, v) in vectors) {
        var (nerr, ok) = v.err._<ж<strconv.NumError>>(ᐧ);
        if (!ok) {
            Ꮡt.Errorf("test %s, error was not a *NumError"u8, v.want);
            continue;
        }
        {
            @string got = nerr.Value.Func; if (got != v.want) {
                Ꮡt.Errorf("mismatching Func: got %s, want %s"u8, got, v.want);
            }
        }
    }
}

} // end strconv_test_package
