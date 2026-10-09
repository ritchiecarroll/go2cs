// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Large data benchmark.
// The JSON data is a summary of agl's changes in the
// go, webkit, and chromium open source projects.
// We benchmark converting between the JSON form
// and in-memory data structures.
namespace go.encoding;

using bytes = bytes_package;
using gzip = compress.gzip_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using regexp = regexp_package;
using runtime = runtime_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using compress;
using static go.encoding.json_package;

partial class json_internal_test_package {

internal partial struct codeResponse {
    public ж<codeNode> Tree; /*`json:"tree"`*/
    public @string Username; /*`json:"username"`*/
}

public partial struct codeNode {
    public @string Name; /*`json:"name"`*/
    public slice<ж<codeNode>> Kids; /*`json:"kids"`*/
    public float64 CLWeight; /*`json:"cl_weight"`*/
    public nint Touches; /*`json:"touches"`*/
    public int64 MinT; /*`json:"min_t"`*/
    public int64 MaxT; /*`json:"max_t"`*/
    public int64 MeanT; /*`json:"mean_t"`*/
}

internal static slice<byte> codeJSON;

internal static ж<codeResponse> ᏑcodeStruct = new StandardBox<codeResponse>(default(codeResponse));
internal static ref codeResponse codeStruct => ref ᏑcodeStruct.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataCodeJsonGzˢ = "testdata/code.json.gz"u8;

internal static void codeInit() {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(testdataCodeJsonGzˢ);
        if (err != default!) {
            throw panic(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var gz, err) = gzip.NewReader(new json_test_package.os_FileжReader(f));
        if (err != default!) {
            throw panic(err);
        }
        (var data, err) = io.ReadAll(new json_test_package.gzip_ReaderжReader(gz));
        if (err != default!) {
            throw panic(err);
        }
        codeJSON = data;
        {
            var errΔ1 = Unmarshal(codeJSON, ᏑcodeStruct); if (errΔ1 != default!) {
                throw panic("unmarshal code.json: " + errΔ1.Error());
            }
        }
        {
            (data, err) = Marshal(ᏑcodeStruct); if (err != default!) {
                throw panic("marshal code.json: " + err.Error());
            }
        }
        if (!bytes.Equal(data, codeJSON)) {
            println((@string)"different lengths"u8, len(data), len(codeJSON));
            for (nint i = 0; i < len(data) && i < len(codeJSON); i++) {
                if (data[i] != codeJSON[i]) {
                    println((@string)"re-marshal: changed at byte"u8, i);
                    println((@string)"orig: "u8, ((@string)(codeJSON.slice(i - 10, i + 10))));
                    println((@string)"new: "u8, ((@string)(data.slice(i - 10, i + 10))));
                    break;
                }
            }
            throw panic("re-marshal code.json: different result");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkCodeEncoder(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    if (codeJSON == default!) {
        b.StopTimer();
        codeInit();
        b.StartTimer();
    }
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var enc = NewEncoder(io.Discard);
        while (pb.Next()) {
            {
                var err = enc.Encode(ᏑcodeStruct); if (err != default!) {
                    Ꮡb.Fatalf("Encode error: %v"u8, err);
                }
            }
        }
    });
    b.SetBytes((int64)len(codeJSON));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object marshalErrorGotNilWantˢ = (@string)"Marshal error: got nil, want non-nil"u8;

// Trigger an error in Marshal with cyclic data.
internal partial struct BenchmarkCodeEncoderError_Dummy /*dyn*/ {
    public @string Name;
    public ж<BenchmarkCodeEncoderError_Dummy> Next;
}

public static void BenchmarkCodeEncoderError(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    if (codeJSON == default!) {
        b.StopTimer();
        codeInit();
        b.StartTimer();
    }
    ref var dummy = ref heap<BenchmarkCodeEncoderError_Dummy>(out var Ꮡdummy);
    dummy = new BenchmarkCodeEncoderError_Dummy(Name: "Dummy"u8);
    dummy.Next = Ꮡdummy;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var enc = NewEncoder(io.Discard);
        while (pb.Next()) {
            {
                var err = enc.Encode(ᏑcodeStruct); if (err != default!) {
                    Ꮡb.Fatalf("Encode error: %v"u8, err);
                }
            }
            {
                var (_, err) = Marshal(Ꮡdummy.Value); if (err == default!) {
                    Ꮡb.Fatal(marshalErrorGotNilWantˢ);
                }
            }
        }
    });
    b.SetBytes((int64)len(codeJSON));
}

public static void BenchmarkCodeMarshal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    if (codeJSON == default!) {
        b.StopTimer();
        codeInit();
        b.StartTimer();
    }
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            {
                var (_, err) = Marshal(ᏑcodeStruct); if (err != default!) {
                    Ꮡb.Fatalf("Marshal error: %v"u8, err);
                }
            }
        }
    });
    b.SetBytes((int64)len(codeJSON));
}

// Trigger an error in Marshal with cyclic data.
internal partial struct BenchmarkCodeMarshalError_Dummy /*dyn*/ {
    public @string Name;
    public ж<BenchmarkCodeMarshalError_Dummy> Next;
}

public static void BenchmarkCodeMarshalError(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    if (codeJSON == default!) {
        b.StopTimer();
        codeInit();
        b.StartTimer();
    }
    ref var dummy = ref heap<BenchmarkCodeMarshalError_Dummy>(out var Ꮡdummy);
    dummy = new BenchmarkCodeMarshalError_Dummy(Name: "Dummy"u8);
    dummy.Next = Ꮡdummy;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            {
                var (_, err) = Marshal(ᏑcodeStruct); if (err != default!) {
                    Ꮡb.Fatalf("Marshal error: %v"u8, err);
                }
            }
            {
                var (_, err) = Marshal(Ꮡdummy.Value); if (err == default!) {
                    Ꮡb.Fatal(marshalErrorGotNilWantˢ);
                }
            }
        }
    });
    b.SetBytes((int64)len(codeJSON));
}

internal partial struct benchMarshalBytes_v /*dyn*/ {
    public slice<byte> Bytes;
}

internal static Action<ж<testing.B>> benchMarshalBytes(nint n) {
    var sample = slice<byte>("hello world"u8);
    // Use a struct pointer, to avoid an allocation when passing it as an
    // interface parameter to Marshal.
    var v = Ꮡ(new benchMarshalBytes_v(
        bytes.Repeat(sample, (n / len(sample)) + 1).slice(0, n)
    ));
    var vʗ1 = v;
    return (ж<testing.B> b) => {
        for (nint i = 0; i < (~b).N; i++) {
            {
                var (_, err) = Marshal(vʗ1.OrTypedNil()); if (err != default!) {
                    b.Fatalf("Marshal error: %v"u8, err);
                }
            }
        }
    };
}

internal partial struct benchMarshalBytesError_v /*dyn*/ {
    public slice<byte> Bytes;
}

// Trigger an error in Marshal with cyclic data.
internal partial struct benchMarshalBytesError_Dummy /*dyn*/ {
    public @string Name;
    public ж<benchMarshalBytesError_Dummy> Next;
}

internal static Action<ж<testing.B>> benchMarshalBytesError(nint n) {
    var sample = slice<byte>("hello world"u8);
    // Use a struct pointer, to avoid an allocation when passing it as an
    // interface parameter to Marshal.
    var v = Ꮡ(new benchMarshalBytesError_v(
        bytes.Repeat(sample, (n / len(sample)) + 1).slice(0, n)
    ));
    ref var dummy = ref heap<benchMarshalBytesError_Dummy>(out var Ꮡdummy);
    dummy = new benchMarshalBytesError_Dummy(Name: "Dummy"u8);
    dummy.Next = Ꮡdummy;
    var vʗ1 = v;
    return (ж<testing.B> b) => {
        for (nint i = 0; i < (~b).N; i++) {
            {
                var (_, err) = Marshal(vʗ1.OrTypedNil()); if (err != default!) {
                    b.Fatalf("Marshal error: %v"u8, err);
                }
            }
            {
                var (_, err) = Marshal(Ꮡdummy.Value); if (err == default!) {
                    b.Fatal(marshalErrorGotNilWantˢ);
                }
            }
        }
    };
}

public static void BenchmarkMarshalBytes(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    // 32 fits within encodeState.scratch.
    Ꮡb.Run("32"u8, benchMarshalBytes(32));
    // 256 doesn't fit in encodeState.scratch, but is small enough to
    // allocate and avoid the slower base64.NewEncoder.
    Ꮡb.Run("256"u8, benchMarshalBytes(256));
    // 4096 is large enough that we want to avoid allocating for it.
    Ꮡb.Run("4096"u8, benchMarshalBytes(4096));
}

public static void BenchmarkMarshalBytesError(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    // 32 fits within encodeState.scratch.
    Ꮡb.Run("32"u8, benchMarshalBytesError(32));
    // 256 doesn't fit in encodeState.scratch, but is small enough to
    // allocate and avoid the slower base64.NewEncoder.
    Ꮡb.Run("256"u8, benchMarshalBytesError(256));
    // 4096 is large enough that we want to avoid allocating for it.
    Ꮡb.Run("4096"u8, benchMarshalBytesError(4096));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object marshalˢ = (@string)"Marshal:"u8;

public static void BenchmarkMarshalMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var m = new map<@string, nint>{
        ["key3"u8] = 3,
        ["key2"u8] = 2,
        ["key1"u8] = 1
    };
    var mʗ1 = m;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            {
                var (_, err) = Marshal(mʗ1); if (err != default!) {
                    Ꮡb.Fatal(marshalˢ, err);
                }
            }
        }
    });
}

public static void BenchmarkCodeDecoder(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    if (codeJSON == default!) {
        b.StopTimer();
        codeInit();
        b.StartTimer();
    }
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var dec = NewDecoder(new json_test_package.bytes_BufferжReader(Ꮡbuf));
        ref var r = ref heap(new codeResponse(), out var Ꮡr);
        while (pb.Next()) {
            buf.Write(codeJSON);
            // hide EOF
            buf.WriteByte((rune)'\n');
            buf.WriteByte((rune)'\n');
            buf.WriteByte((rune)'\n');
            {
                var err = dec.Decode(Ꮡr); if (err != default!) {
                    Ꮡb.Fatalf("Decode error: %v"u8, err);
                }
            }
        }
    });
    b.SetBytes((int64)len(codeJSON));
}

public static void BenchmarkUnicodeDecoder(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var j = slice<byte>(@"""\uD83D\uDE01"""u8);
    b.SetBytes((int64)len(j));
    var r = bytes.NewReader(j);
    var dec = NewDecoder(new json_test_package.bytes_ReaderжReader(r));
    ref var @out = ref heap(new @string(), out var Ꮡout);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        {
            var err = dec.Decode(Ꮡout); if (err != default!) {
                Ꮡb.Fatalf("Decode error: %v"u8, err);
            }
        }
        r.Seek(0, 0);
    }
}

public static void BenchmarkDecoderStream(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    b.StopTimer();
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var dec = NewDecoder(new json_test_package.bytes_BufferжReader(Ꮡbuf));
    buf.WriteString(@""""u8 + strings.Repeat("x"u8, 1000000) + @""""u8 + "\n\n\n"u8);
    ref var x = ref heap<any>(out var Ꮡx);
    {
        var err = dec.Decode(Ꮡx); if (err != default!) {
            Ꮡb.Fatalf("Decode error: %v"u8, err);
        }
    }
    @string ones = strings.Repeat(" 1\n"u8, 300000) + "\n\n\n"u8;
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (i % 300000 == 0) {
            buf.WriteString(ones);
        }
        x = default!;
        {
            var err = dec.Decode(Ꮡx);
            switch (ᐧ) {
            case {} when err != default!: {
                Ꮡb.Fatalf("Decode error: %v"u8, err);
                break;
            }
            case {} when x is not 1.0D: {
                Ꮡb.Fatalf("Decode: got %v want 1.0"u8, i);
                break;
            }}
        }

    }
}

public static void BenchmarkCodeUnmarshal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    if (codeJSON == default!) {
        b.StopTimer();
        codeInit();
        b.StartTimer();
    }
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            ref var r = ref heap(new codeResponse(), out var Ꮡr);
            {
                var err = Unmarshal(codeJSON, Ꮡr); if (err != default!) {
                    Ꮡb.Fatalf("Unmarshal error: %v"u8, err);
                }
            }
        }
    });
    b.SetBytes((int64)len(codeJSON));
}

public static void BenchmarkCodeUnmarshalReuse(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    if (codeJSON == default!) {
        b.StopTimer();
        codeInit();
        b.StartTimer();
    }
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var r = ref heap(new codeResponse(), out var Ꮡr);
        while (pb.Next()) {
            {
                var err = Unmarshal(codeJSON, Ꮡr); if (err != default!) {
                    Ꮡb.Fatalf("Unmarshal error: %v"u8, err);
                }
            }
        }
    });
    b.SetBytes((int64)len(codeJSON));
}

public static void BenchmarkUnmarshalString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var data = slice<byte>(@"""hello, world"""u8);
    var dataʗ1 = data;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var s = ref heap(new @string(), out var Ꮡs);
        while (pb.Next()) {
            {
                var err = Unmarshal(dataʗ1, Ꮡs); if (err != default!) {
                    Ꮡb.Fatalf("Unmarshal error: %v"u8, err);
                }
            }
        }
    });
}

public static void BenchmarkUnmarshalFloat64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var data = slice<byte>(@"3.14"u8);
    var dataʗ1 = data;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var f = ref heap(new float64(), out var Ꮡf);
        while (pb.Next()) {
            {
                var err = Unmarshal(dataʗ1, Ꮡf); if (err != default!) {
                    Ꮡb.Fatalf("Unmarshal error: %v"u8, err);
                }
            }
        }
    });
}

public static void BenchmarkUnmarshalInt64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var data = slice<byte>(@"3"u8);
    var dataʗ1 = data;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var x = ref heap(new int64(), out var Ꮡx);
        while (pb.Next()) {
            {
                var err = Unmarshal(dataʗ1, Ꮡx); if (err != default!) {
                    Ꮡb.Fatalf("Unmarshal error: %v"u8, err);
                }
            }
        }
    });
}

public static void BenchmarkUnmarshalMap(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var data = slice<byte>(@"{""key1"":""value1"",""key2"":""value2"",""key3"":""value3""}"u8);
    var dataʗ1 = data;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var x = ref heap<map<@string, @string>>(out var Ꮡx);
        x = new map<@string, @string>(3);
        while (pb.Next()) {
            {
                var err = Unmarshal(dataʗ1, Ꮡx); if (err != default!) {
                    Ꮡb.Fatalf("Unmarshal error: %v"u8, err);
                }
            }
        }
    });
}

public static void BenchmarkIssue10335(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var j = slice<byte>(@"{""a"":{ }}"u8);
    var jʗ1 = j;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var s = ref heap(new EmptyStruct(), out var Ꮡs);
        while (pb.Next()) {
            {
                var err = Unmarshal(jʗ1, Ꮡs); if (err != default!) {
                    Ꮡb.Fatalf("Unmarshal error: %v"u8, err);
                }
            }
        }
    });
}

internal partial struct BenchmarkIssue34127_j /*dyn*/ {
    public @string Bar; /*`json:"bar,string"`*/
}

public static void BenchmarkIssue34127(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ref var j = ref heap<BenchmarkIssue34127_j>(out var Ꮡj);
    j = new BenchmarkIssue34127_j(
        Bar: @"foobar"u8
    );
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            {
                var (_, err) = Marshal(Ꮡj); if (err != default!) {
                    Ꮡb.Fatalf("Marshal error: %v"u8, err);
                }
            }
        }
    });
}

public static void BenchmarkUnmapped(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var j = slice<byte>(@"{""s"": ""hello"", ""y"": 2, ""o"": {""x"": 0}, ""a"": [1, 99, {""x"": 1}]}"u8);
    var jʗ1 = j;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var s = ref heap(new EmptyStruct(), out var Ꮡs);
        while (pb.Next()) {
            {
                var err = Unmarshal(jʗ1, Ꮡs); if (err != default!) {
                    Ꮡb.Fatalf("Unmarshal error: %v"u8, err);
                }
            }
        }
    });
}

public static void BenchmarkTypeFieldsCache(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    nint maxTypes = 1000000;
    if (testenv.Builder() != ""u8) {
        maxTypes = 1000; // restrict cache sizes on builders
    }
    // Dynamically generate many new types.
    var types = new slice<reflectꓸType>(maxTypes);
    var fs = new reflect.StructField[]{new(
        Type: reflect.TypeFor<@string>(),
        Index: new nint[]{0}.slice()
    )
    }.slice();
    foreach (var (i, _) in types) {
        fs[0].Name = fmt.Sprintf("TypeFieldsCache%d"u8, i);
        types[i] = reflect.StructOf(fs);
    }
    // clearClear clears the cache. Other JSON operations, must not be running.
    void clearCache() {
        fieldCache = new sync.Map(nil);
    }
    // MissTypes tests the performance of repeated cache misses.
    // This measures the time to rebuild a cache of size nt.
    for (nint nt = 1; nt <= maxTypes; nt *= 10) {
        var ts = types.slice(0, nt);
        var clearCacheʗ1 = clearCache;
        var tsʗ1 = ts;
        Ꮡb.Run(fmt.Sprintf("MissTypes%d"u8, nt), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ1) => {
            nint nc = runtime.GOMAXPROCS(0);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                clearCacheʗ1();
                ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
                for (nint j = 0; j < nc; j++) {
                    Ꮡwg.Add(1);
                    var tsʗ2 = tsʗ1;
                    goǃ((nint jΔ1) => {
                        foreach (var (_, t) in tsʗ2.slice(quo((jΔ1 * len(tsʗ2)), nc), quo(((jΔ1 + 1) * len(tsʗ2)), nc))) {
                            cachedTypeFields(t);
                        }
                        Ꮡwg.Done();
                    }, j);
                }
                Ꮡwg.Wait();
            }
        });
    }
    // HitTypes tests the performance of repeated cache hits.
    // This measures the average time of each cache lookup.
    for (nint nt = 1; nt <= maxTypes; nt *= 10) {
        // Pre-warm a cache of size nt.
        clearCache();
        foreach (var (_, t) in types.slice(0, nt)) {
            cachedTypeFields(t);
        }
        var typesʗ1 = types;
        Ꮡb.Run(fmt.Sprintf("HitTypes%d"u8, nt), (ж<testing.B> bΔ2) => {
            var typesʗ2 = typesʗ1;
            bΔ2.RunParallel((ж<testing.PB> pb) => {
                while (pb.Next()) {
                    cachedTypeFields(typesʗ2[0]);
                }
            });
        });
    }
}

internal partial struct BenchmarkEncodeMarshaler_m /*dyn*/ {
    public nint A;
    public global::go.encoding.json_package.RawMessage B;
}

public static void BenchmarkEncodeMarshaler(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ref var m = ref heap<BenchmarkEncodeMarshaler_m>(out var Ꮡm);
    m = new BenchmarkEncodeMarshaler_m();
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var enc = NewEncoder(io.Discard);
        while (pb.Next()) {
            {
                var err = enc.Encode(Ꮡm); if (err != default!) {
                    Ꮡb.Fatalf("Encode error: %v"u8, err);
                }
            }
        }
    });
}

internal partial struct BenchmarkEncoderEncode_T /*dyn*/ {
    public @string X, Y;
}

public static void BenchmarkEncoderEncode(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var v = Ꮡ(new BenchmarkEncoderEncode_T("foo"u8, "bar"u8));
    var vʗ1 = v;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            {
                var err = NewEncoder(io.Discard).Encode(vʗ1.OrTypedNil()); if (err != default!) {
                    Ꮡb.Fatalf("Encode error: %v"u8, err);
                }
            }
        }
    });
}

public static void BenchmarkNumberIsValid(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s = "-61657.61667E+61673"u8;
    for (nint i = 0; i < b.N; i++) {
        isValidNumber(s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dDEEDˢ = @"^-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?$"u8;

public static void BenchmarkNumberIsValidRegexp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ж<regexp.Regexp> jsonNumberRegexp = regexp.MustCompile(dDEEDˢ);
    @string s = "-61657.61667E+61673"u8;
    for (nint i = 0; i < b.N; i++) {
        jsonNumberRegexp.MatchString(s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshalˢ = (@string)"Unmarshal:"u8;

public static void BenchmarkUnmarshalNumber(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var data = slice<byte>(@"""-61657.61667E+61673"""u8);
    ref var number = ref heap(new global::go.encoding.json_package.Number(), out var Ꮡnumber);
    for (nint i = 0; i < b.N; i++) {
        {
            var err = Unmarshal(data, Ꮡnumber); if (err != default!) {
                Ꮡb.Fatal(unmarshalˢ, err);
            }
        }
    }
}

} // end json_internal_test_package
