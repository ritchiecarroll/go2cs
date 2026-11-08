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
internal static @string nextToOne = "1.00000000000000011102230246251565404236316680908203125"u8 + strings.Repeat("0", 10000) + "1"u8;

partial struct mallocTestᴛ1 /*dyn*/ {
    internal nint count;
    internal @string desc;
    internal Action fn;
}
internal static slice<mallocTestᴛ1> mallocTest;
internal static void initᴛmallocTest() { mallocTest = [
    new(0, @"AppendInt(localBuf[:0], 123, 10)", () => {
        array<byte> localBuf = new(64);
        AppendInt(localBuf[..0], 123, 10);
    }),
    new(0, @"AppendInt(globalBuf[:0], 123, 10)", () => {
        AppendInt(globalBuf[..0], 123, 10);
    }),
    new(0, @"AppendFloat(localBuf[:0], 1.23, 'g', 5, 64)", () => {
        array<byte> localBuf = new(64);
        AppendFloat(localBuf[..0], 1.23D, (byte)'g', 5, 64);
    }),
    new(0, @"AppendFloat(globalBuf[:0], 1.23, 'g', 5, 64)", () => {
        AppendFloat(globalBuf[..0], 1.23D, (byte)'g', 5, 64);
    }),
    // In practice we see 7 for the next one, but allow some slop.
    // Before pre-allocation in appendQuotedWith, we saw 39.
    new(10, @"AppendQuoteToASCII(nil, oneMB)", () => {
        AppendQuoteToASCII(default, (@string)oneMB);
    }),
    new(0, @"ParseFloat(""123.45"", 64)", () => {
        ParseFloat("123.45", 64);
    }),
    new(0, @"ParseFloat(""123.456789123456789"", 64)", () => {
        ParseFloat("123.456789123456789", 64);
    }),
    new(0, @"ParseFloat(""1.000000000000000111022302462515654042363166809082031251"", 64)", () => {
        ParseFloat("1.000000000000000111022302462515654042363166809082031251", 64);
    }),
    new(0, @"ParseFloat(""1.0000000000000001110223024625156540423631668090820312500...001"", 64)", () => {
        ParseFloat(nextToOne, 64);
    })
]; }

internal static slice<byte> oneMB; // Will be allocated to 1MB of random data by TestCountMallocs.

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

public static void TestCountMallocs(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        t.Skip(skippingGomaxprocs1ˢ);
    }
    // Allocate a big messy buffer for AppendQuoteToASCII's test.
    oneMB = new slice<byte>(1000000);
    foreach (var i in range(len(oneMB))) {
        oneMB[i] = (byte)i;
    }
    foreach (var (_, mt) in range(mallocTest)) {
        var allocs = testing.AllocsPerRun(100, mt.fn);
        var max = (float64)mt.count; if (allocs > max) {
            t.Errorf("%s: %v allocs, want <=%v", mt.desc, allocs, max);
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
public static readonly ж<Sinkᴛ1> ᏑSink = new StaticBox<Sinkᴛ1>(default(Sinkᴛ1));
public static ref Sinkᴛ1 Sink => ref ((StaticBox<Sinkᴛ1>)ᏑSink).ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string atoiˢ => ref atoiˢᵇ;
private static readonly @string atoiˢᵇ = "Atoi"u8;
internal static ref readonly @string parseBoolˢ => ref parseBoolˢᵇ;
private static readonly @string parseBoolˢᵇ = "ParseBool"u8;
internal static ref readonly @string parseFloatˢ => ref parseFloatˢᵇ;
private static readonly @string parseFloatˢᵇ = "ParseFloat"u8;
internal static ref readonly @string parseComplexˢ => ref parseComplexˢᵇ;
private static readonly @string parseComplexˢᵇ = "ParseComplex"u8;
internal static ref readonly @string canBackquoteˢ => ref canBackquoteˢᵇ;
private static readonly @string canBackquoteˢᵇ = "CanBackquote"u8;
internal static ref readonly @string appendQuoteˢ => ref appendQuoteˢᵇ;
private static readonly @string appendQuoteˢᵇ = "AppendQuote"u8;
internal static ref readonly @string appendQuoteToASCIIˢ => ref appendQuoteToASCIIˢᵇ;
private static readonly @string appendQuoteToASCIIˢᵇ = "AppendQuoteToASCII"u8;
internal static ref readonly @string appendQuoteToGraphicˢ => ref appendQuoteToGraphicˢᵇ;
private static readonly @string appendQuoteToGraphicˢᵇ = "AppendQuoteToGraphic"u8;

internal partial struct TestAllocationsFromBytes_bytes /*dyn*/ {
    public slice<byte> Bool, Number, String, Buffer;
}

public static void TestAllocationsFromBytes(ж<testing.T> t) {
    const nint runsPerTest = 100;
    ref var bytes = ref heap<TestAllocationsFromBytes_bytes>(new TestAllocationsFromBytes_bytes(
        Bool: slice<byte>("false"u8),
        Number: slice<byte>("123456789"u8),
        String: slice<byte>("hello, world!"u8),
        Buffer: new slice<byte>(1024)
    ), out var Ꮡbytes);

    Action<ж<testing.T>> checkNoAllocs(Action f) => (ж<testing.T> tΔ1) => {
            tΔ1.Helper();
            var allocs = testing.AllocsPerRun(runsPerTest, f); if (allocs != 0D) {
                tΔ1.Errorf("got %v allocs, want 0 allocs", allocs);
            }
        };

    var bytesʗ1 = bytes;
    t.Run(atoiˢ, checkNoAllocs(() => {
        (Sink.Int, Sink.Error) = Atoi((@string)bytesʗ1.Number);
    }));
    var bytesʗ2 = bytes;
    t.Run(parseBoolˢ, checkNoAllocs(() => {
        (Sink.Bool, Sink.Error) = ParseBool((@string)bytesʗ2.Bool);
    }));
    var bytesʗ3 = bytes;
    t.Run(parseIntˢ, checkNoAllocs(() => {
        (Sink.Int64, Sink.Error) = ParseInt((@string)bytesʗ3.Number, 10, 64);
    }));
    var bytesʗ4 = bytes;
    t.Run(parseUintˢ, checkNoAllocs(() => {
        (Sink.Uint64, Sink.Error) = ParseUint((@string)bytesʗ4.Number, 10, 64);
    }));
    var bytesʗ5 = bytes;
    t.Run(parseFloatˢ, checkNoAllocs(() => {
        (Sink.Float64, Sink.Error) = ParseFloat((@string)bytesʗ5.Number, 64);
    }));
    var bytesʗ6 = bytes;
    t.Run(parseComplexˢ, checkNoAllocs(() => {
        (Sink.Complex128, Sink.Error) = ParseComplex((@string)bytesʗ6.Number, 128);
    }));
    var bytesʗ7 = bytes;
    t.Run(canBackquoteˢ, checkNoAllocs(() => {
        Sink.Bool = CanBackquote((@string)bytesʗ7.String);
    }));
    var bytesʗ8 = bytes;
    t.Run(appendQuoteˢ, checkNoAllocs(() => {
        Sink.Bytes = AppendQuote(bytesʗ8.Buffer[..0], (@string)bytesʗ8.String);
    }));
    var bytesʗ9 = bytes;
    t.Run(appendQuoteToASCIIˢ, checkNoAllocs(() => {
        Sink.Bytes = AppendQuoteToASCII(bytesʗ9.Buffer[..0], (@string)bytesʗ9.String);
    }));
    var bytesʗ10 = bytes;
    t.Run(appendQuoteToGraphicˢ, checkNoAllocs(() => {
        Sink.Bytes = AppendQuoteToGraphic(bytesʗ10.Buffer[..0], (@string)bytesʗ10.String);
    }));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string invalidˢ => ref invalidˢᵇ;
private static readonly @string invalidˢᵇ = "INVALID"u8;

internal partial struct TestErrorPrefixes_vectors /*dyn*/ {
    internal error err;    // Input error
    internal @string want; // Function name wanted
}

public static void TestErrorPrefixes(ж<testing.T> t) {
    var (_, errInt) = Atoi(invalidˢ);
    var (_, errBool) = ParseBool(invalidˢ);
    var (_, errFloat) = ParseFloat(invalidˢ, 64);
    var (_, errInt64) = ParseInt(invalidˢ, 10, 64);
    var (_, errUint64) = ParseUint(invalidˢ, 10, 64);

    var vectors = slice<TestErrorPrefixes_vectors>([
        new(errInt, "Atoi"),
        new(errBool, "ParseBool"),
        new(errFloat, "ParseFloat"),
        new(errInt64, "ParseInt"),
        new(errUint64, "ParseUint")
    ]);

    foreach (var (_, v) in range(vectors)) {
        var (nerr, ok) = v.err._<ж<strconv.NumError>>(ᐧ);
        if (!ok) {
            t.Errorf("test %s, error was not a *NumError", v.want);
            continue;
        }
        @string got = nerr.Func; if (got != v.want) {
            t.Errorf("mismatching Func: got %s, want %s", got, v.want);
        }
    }
}

} // end strconv_test_package
