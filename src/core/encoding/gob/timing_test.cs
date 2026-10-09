// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using runtime = runtime_package;
using testing = testing_package;
using static go.encoding.gob_package;

partial class gob_internal_test_package {

public partial struct Bench {
    public nint A;
    public float64 B;
    public @string C;
    public slice<byte> D;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object canTGetPipeˢ = (@string)"can't get pipe:"u8;

internal static void benchmarkEndToEnd(ж<testing.B> Ꮡb, Func<any> ctor, Func<(io.Reader, io.Writer, error)> pipe) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var (r, w, err) = pipe();
        if (err != default!) {
            Ꮡb.Fatal(canTGetPipeˢ, err);
        }
        var v = ctor();
        var enc = NewEncoder(w);
        var dec = NewDecoder(r);
        while (pb.Next()) {
            {
                var errΔ1 = enc.Encode(v); if (errΔ1 != default!) {
                    Ꮡb.Fatal(encodeErrorˢ, errΔ1);
                }
            }
            {
                var errΔ2 = dec.Decode(v); if (errΔ2 != default!) {
                    Ꮡb.Fatal(decodeErrorˢ, errΔ2);
                }
            }
        }
    });
}

public static void BenchmarkEndToEndPipe(ж<testing.B> Ꮡb) {
    benchmarkEndToEnd(Ꮡb, () => Ꮡ(new Bench(7, 3.2D, "now is the time"u8, bytes.Repeat(slice<byte>("for all good men"u8), 100))), () => {
        io.Reader r = default!;
        io.Writer w = default!;
        error err = default!;
        var (ᴛ1, ᴛ2, ᴛ3) = os.Pipe();
        (r, w, err) = (new gob_test_package.os_FileжReader(ᴛ1), new os.FileжWriter(ᴛ2), ᴛ3);
        return (r, w, err);
    });
}

public static void BenchmarkEndToEndByteBuffer(ж<testing.B> Ꮡb) {
    benchmarkEndToEnd(Ꮡb, () => Ꮡ(new Bench(7, 3.2D, "now is the time"u8, bytes.Repeat(slice<byte>("for all good men"u8), 100))), () => {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        return (new gob_test_package.bytes_BufferжReader(Ꮡbuf), new gob_test_package.bytes_BufferжWriter(Ꮡbuf), default!);
    });
}

public static void BenchmarkEndToEndSliceByteBuffer(ж<testing.B> Ꮡb) {
    benchmarkEndToEnd(Ꮡb, () => {
        var v = Ꮡ(new Bench(7, 3.2D, "now is the time"u8, default!));
        Register(v.OrTypedNil());
        ref var arr = ref heap<slice<any>>(out var Ꮡarr);
        arr = new slice<any>(100);
        foreach (var (i, _) in arr) {
            arr[i] = v.OrTypedNil();
        }
        return Ꮡarr;
    }, () => {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        return (new gob_test_package.bytes_BufferжReader(Ꮡbuf), new gob_test_package.bytes_BufferжWriter(Ꮡbuf), default!);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

public static void TestCountEncodeMallocs(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    if (runtime.GOMAXPROCS(0) > 1) {
        Ꮡt.Skip(skippingGomaxprocs1ˢ);
    }
    const nint N = 1000;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡbuf));
    var bench = Ꮡ(new Bench(7, 3.2D, "now is the time"u8, slice<byte>("for all good men"u8)));
    var benchʗ1 = bench;
    var encʗ1 = enc;
    var allocs = testing.AllocsPerRun(N, () => {
        var err = encʗ1.Encode(benchʗ1.OrTypedNil());
        if (err != default!) {
            Ꮡt.Fatal(encodeˢ, err);
        }
    });
    if (allocs != 0D) {
        Ꮡt.Fatalf("mallocs per encode of type Bench: %v; wanted 0\n"u8, allocs);
    }
}

public static void TestCountDecodeMallocs(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    if (runtime.GOMAXPROCS(0) > 1) {
        Ꮡt.Skip(skippingGomaxprocs1ˢ);
    }
    const nint N = 1000;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡbuf));
    ref var bench = ref heap<ж<Bench>>(out var Ꮡbench);
    bench = Ꮡ(new Bench(7, 3.2D, "now is the time"u8, slice<byte>("for all good men"u8)));
    // Fill the buffer with enough to decode
    var encʗ1 = enc;
    testing.AllocsPerRun(N, () => {
        var err = encʗ1.Encode(Ꮡbench.ValueSlot.OrTypedNil());
        if (err != default!) {
            Ꮡt.Fatal(encodeˢ, err);
        }
    });
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡbuf));
    var decʗ1 = dec;
    var allocs = testing.AllocsPerRun(N, () => {
        Ꮡbench.ValueSlot.Value = new Bench(nil);
        var err = decʗ1.Decode(Ꮡbench);
        if (err != default!) {
            Ꮡt.Fatal(decodeˢ, err);
        }
    });
    if (allocs != 3D) {
        Ꮡt.Fatalf("mallocs per decode of type Bench: %v; wanted 3\n"u8, allocs);
    }
}

internal static void benchmarkEncodeSlice(ж<testing.B> Ꮡb, any a) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ResetTimer();
    b.ReportAllocs();
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡbuf));
        while (pb.Next()) {
            buf.Reset();
            var err = enc.Encode(a);
            if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    });
}

public static void BenchmarkEncodeComplex128Slice(ж<testing.B> Ꮡb) {
    var a = new slice<complex128>(1000);
    foreach (var (i, _) in a) {
        a[i] = 1.2D + 3.4D.i();
    }
    benchmarkEncodeSlice(Ꮡb, a);
}

public static void BenchmarkEncodeFloat64Slice(ж<testing.B> Ꮡb) {
    var a = new slice<float64>(1000);
    foreach (var (i, _) in a) {
        a[i] = 1.23e4D;
    }
    benchmarkEncodeSlice(Ꮡb, a);
}

public static void BenchmarkEncodeInt32Slice(ж<testing.B> Ꮡb) {
    var a = new slice<int32>(1000);
    foreach (var (i, _) in a) {
        a[i] = (int32)(i * 100);
    }
    benchmarkEncodeSlice(Ꮡb, a);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nowIsTheTimeˢ = "now is the time"u8;

public static void BenchmarkEncodeStringSlice(ж<testing.B> Ꮡb) {
    var a = new slice<@string>(1000);
    foreach (var (i, _) in a) {
        a[i] = nowIsTheTimeˢ;
    }
    benchmarkEncodeSlice(Ꮡb, a);
}

public static void BenchmarkEncodeInterfaceSlice(ж<testing.B> Ꮡb) {
    var a = new slice<any>(1000);
    foreach (var (i, _) in a) {
        a[i] = nowIsTheTimeˢ;
    }
    benchmarkEncodeSlice(Ꮡb, a);
}

// benchmarkBuf is a read buffer we can reset
internal partial struct benchmarkBuf {
    internal nint offset;
    internal slice<byte> data;
}

internal static (nint n, error err) Read(this ref benchmarkBuf b, slice<byte> p) {
    nint n = default!;
    error err = default!;

    n = copy(p, b.data.slice(b.offset));
    if (n == 0) {
        return (0, io.EOF);
    }
    b.offset += n;
    return (n, err);
}

internal static (byte c, error err) ReadByte(this ref benchmarkBuf b) {
    byte c = default!;
    error err = default!;

    if (b.offset >= len(b.data)) {
        return (0, io.EOF);
    }
    c = b.data[b.offset];
    b.offset++;
    return (c, err);
}

internal static void reset(this ref benchmarkBuf b) {
    b.offset = 0;
}

internal static void benchmarkDecodeSlice(ж<testing.B> Ꮡb, any a) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡbuf));
    var err = enc.Encode(a);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    ref var ra = ref heap<reflectꓸValue>(out var Ꮡra);
    ra = reflect.ValueOf(a);
    var rt = ra.Type();
    b.ResetTimer();
    b.ReportAllocs();
    var raʗ1 = ra;
    var rtʗ1 = rt;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        // TODO(#19025): Move per-thread allocation before ResetTimer.
        var rp = reflect.New(rtʗ1);
        rp.Elem().Set(reflect.MakeSlice(rtʗ1, raʗ1.Len(), raʗ1.Cap()));
        var p = rp.Interface();
        ref var bbuf = ref heap<benchmarkBuf>(out var Ꮡbbuf);
        bbuf = new benchmarkBuf(data: Ꮡbuf.Value.Bytes());
        while (pb.Next()) {
            bbuf.reset();
            var dec = NewDecoder(new gob_internal_test_package.benchmarkBufжReader(Ꮡbbuf));
            var errΔ1 = dec.Decode(p);
            if (errΔ1 != default!) {
                Ꮡb.Fatal(errΔ1);
            }
        }
    });
}

public static void BenchmarkDecodeComplex128Slice(ж<testing.B> Ꮡb) {
    var a = new slice<complex128>(1000);
    foreach (var (i, _) in a) {
        a[i] = 1.2D + 3.4D.i();
    }
    benchmarkDecodeSlice(Ꮡb, a);
}

public static void BenchmarkDecodeFloat64Slice(ж<testing.B> Ꮡb) {
    var a = new slice<float64>(1000);
    foreach (var (i, _) in a) {
        a[i] = 1.23e4D;
    }
    benchmarkDecodeSlice(Ꮡb, a);
}

public static void BenchmarkDecodeInt32Slice(ж<testing.B> Ꮡb) {
    var a = new slice<int32>(1000);
    foreach (var (i, _) in a) {
        a[i] = 1234;
    }
    benchmarkDecodeSlice(Ꮡb, a);
}

public static void BenchmarkDecodeStringSlice(ж<testing.B> Ꮡb) {
    var a = new slice<@string>(1000);
    foreach (var (i, _) in a) {
        a[i] = nowIsTheTimeˢ;
    }
    benchmarkDecodeSlice(Ꮡb, a);
}

public static void BenchmarkDecodeStringsSlice(ж<testing.B> Ꮡb) {
    var a = new slice<slice<@string>>(1000);
    foreach (var (i, _) in a) {
        a[i] = new @string[]{"now is the time"u8}.slice();
    }
    benchmarkDecodeSlice(Ꮡb, a);
}

public static void BenchmarkDecodeBytesSlice(ж<testing.B> Ꮡb) {
    var a = new slice<slice<byte>>(1000);
    foreach (var (i, _) in a) {
        a[i] = slice<byte>("now is the time"u8);
    }
    benchmarkDecodeSlice(Ꮡb, a);
}

public static void BenchmarkDecodeInterfaceSlice(ж<testing.B> Ꮡb) {
    var a = new slice<any>(1000);
    foreach (var (i, _) in a) {
        a[i] = nowIsTheTimeˢ;
    }
    benchmarkDecodeSlice(Ꮡb, a);
}

public static void BenchmarkDecodeMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint count = 1000;
    var m = new map<nint, nint>(count);
    for (nint i = 0; i < count; i++) {
        m[i] = i;
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡbuf));
    var err = enc.Encode(m);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    ref var bbuf = ref heap<benchmarkBuf>(out var Ꮡbbuf);
    bbuf = new benchmarkBuf(data: buf.Bytes());
    b.ResetTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        ref var rm = ref heap<map<nint, nint>>(out var Ꮡrm);
        bbuf.reset();
        var dec = NewDecoder(new gob_internal_test_package.benchmarkBufжReader(Ꮡbbuf));
        var errΔ1 = dec.Decode(Ꮡrm);
        if (errΔ1 != default!) {
            Ꮡb.Fatal(i, errΔ1);
        }
    }
}

} // end gob_internal_test_package
