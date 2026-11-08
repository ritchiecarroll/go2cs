// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using bytes = bytes_package;
using flag = flag_package;
using strings = strings_package;
using testing = testing_package;
using encoding = encoding_package;
using static go.log.slog_package;

partial class slog_internal_test_package {

internal partial struct TestLevelString_type /*dyn*/ {
    internal global::go.log.slog_package.ΔLevel @in;
    internal @string want;
}

public static void TestLevelString(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestLevelString_type>([
        new(0, "INFO"),
        new(LevelError, "ERROR"),
        new(LevelError + 2, "ERROR+2"),
        new(LevelError - 2, "WARN+2"),
        new(LevelWarn, "WARN"),
        new(LevelWarn - 1, "INFO+3"),
        new(LevelInfo, "INFO"),
        new(LevelInfo + 1, "INFO+1"),
        new(LevelInfo - 3, "DEBUG+1"),
        new(LevelDebug, "DEBUG"),
        new(LevelDebug - 2, "DEBUG-2")
    ]))) {
        @string got = test.@in.String();
        if (got != test.want) {
            t.Errorf("%d: got %s, want %s", test.@in, got, test.want);
        }
    }
}

public static void TestLevelVar(ж<testing.T> t) {
    ref var al = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡal);
    {
        global::go.log.slog_package.ΔLevel got = Ꮡal.Level();
        global::go.log.slog_package.ΔLevel want = LevelInfo; if (got != want) {
            t.Errorf("got %v, want %v", got, want);
        }
    }
    Ꮡal.Set(LevelWarn);
    {
        global::go.log.slog_package.ΔLevel got = Ꮡal.Level();
        global::go.log.slog_package.ΔLevel want = LevelWarn; if (got != want) {
            t.Errorf("got %v, want %v", got, want);
        }
    }
    Ꮡal.Set(LevelInfo);
    {
        global::go.log.slog_package.ΔLevel got = Ꮡal.Level();
        global::go.log.slog_package.ΔLevel want = LevelInfo; if (got != want) {
            t.Errorf("got %v, want %v", got, want);
        }
    }
}

public static void TestLevelMarshalJSON(ж<testing.T> t) {
    global::go.log.slog_package.ΔLevel want = LevelWarn - 3;
    var wantData = slice<byte>(@"""INFO+1"""u8);
    var (data, err) = want.MarshalJSON();
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(data, wantData)) {
        t.Errorf("got %s, want %s", (@string)data, (@string)wantData);
    }
    ref var got = ref heap(new global::go.log.slog_package.ΔLevel(), out var Ꮡgot);
    var errΔ1 = Ꮡgot.UnmarshalJSON(data); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    if (got != want) {
        t.Errorf("got %s, want %s", got, want);
    }
}

public static void TestLevelMarshalText(ж<testing.T> t) {
    global::go.log.slog_package.ΔLevel want = LevelWarn - 3;
    var wantData = slice<byte>("INFO+1"u8);
    var (data, err) = want.MarshalText();
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(data, wantData)) {
        t.Errorf("got %s, want %s", (@string)data, (@string)wantData);
    }
    ref var got = ref heap(new global::go.log.slog_package.ΔLevel(), out var Ꮡgot);
    var errΔ1 = Ꮡgot.UnmarshalText(data); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    if (got != want) {
        t.Errorf("got %s, want %s", got, want);
    }
}

public static void TestLevelAppendText(ж<testing.T> t) {
    var buf = new slice<byte>(4, 16);
    global::go.log.slog_package.ΔLevel want = LevelWarn - 3;
    var wantData = slice<byte>("\x00\x00\x00\x00INFO+1"u8);
    var (data, err) = want.AppendText(buf);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(data, wantData)) {
        t.Errorf("got %s, want %s", (@string)data, (@string)wantData);
    }
}

internal partial struct TestLevelParse_type /*dyn*/ {
    internal @string @in;
    internal global::go.log.slog_package.ΔLevel want;
}

public static void TestLevelParse(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestLevelParse_type>([
        new("DEBUG", LevelDebug),
        new("INFO", LevelInfo),
        new("WARN", LevelWarn),
        new("ERROR", LevelError),
        new("debug", LevelDebug),
        new("iNfo", LevelInfo),
        new("INFO+87", LevelInfo + 87),
        new("Error-18", LevelError - 18),
        new("Error-8", LevelInfo)
    ]))) {
        ref var got = ref heap(new global::go.log.slog_package.ΔLevel(), out var Ꮡgot);
        var err = Ꮡgot.parse(test.@in); if (err != nil) {
            t.Fatalf("%q: %v", test.@in, err);
        }
        if (got != test.want) {
            t.Errorf("%q: got %s, want %s", test.@in, got, test.want);
        }
    }
}

internal partial struct TestLevelParseError_type /*dyn*/ {
    internal @string @in;
    internal @string want; // error string should contain this
}

public static void TestLevelParseError(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestLevelParseError_type>([
        new("", "unknown name"),
        new("dbg", "unknown name"),
        new("INFO+", "invalid syntax"),
        new("INFO-", "invalid syntax"),
        new("ERROR+23x", "invalid syntax")
    ]))) {
        ref var l = ref heap(new global::go.log.slog_package.ΔLevel(), out var Ꮡl);
        var err = Ꮡl.parse(test.@in);
        if (err == nil || !strings.Contains(err.Error(), test.want)) {
            t.Errorf("%q: got %v, want string containing %q", test.@in, err, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;
internal static ref readonly @string levelˢ => ref levelˢᵇ;
private static readonly @string levelˢᵇ = "level"u8;
internal static ref readonly @string setLevelˢ => ref setLevelˢᵇ;
private static readonly @string setLevelˢᵇ = "set level"u8;

public static void TestLevelFlag(ж<testing.T> t) {
    var fs = flag.NewFlagSet(testˢ, flag.ContinueOnError);
    ref var lf = ref heap<global::go.log.slog_package.ΔLevel>(LevelInfo, out var Ꮡlf);
    fs.TextVar(new slog_test_package.slog_ΔLevelжTextUnmarshaler(Ꮡlf), levelˢ, new slog_test_package.slog_ΔLevelᴠTextMarshaler(lf), setLevelˢ);
    var err = fs.Parse(["-level", "WARN+3"]);
    if (err != nil) {
        t.Fatal(err);
    }
    global::go.log.slog_package.ΔLevel g = lf;
    global::go.log.slog_package.ΔLevel w = LevelWarn + 3; if (g != w) {
        t.Errorf("got %v, want %v", g, w);
    }
}

public static void TestLevelVarMarshalText(ж<testing.T> t) {
    ref var v = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv);
    Ꮡv.Set(LevelWarn);
    var (data, err) = Ꮡv.MarshalText();
    if (err != nil) {
        t.Fatal(err);
    }
    ref var v2 = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv2);
    var errΔ1 = Ꮡv2.UnmarshalText(data); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    global::go.log.slog_package.ΔLevel g = Ꮡv2.Level();
    global::go.log.slog_package.ΔLevel w = LevelWarn; if (g != w) {
        t.Errorf("got %s, want %s", g, w);
    }
}

public static void TestLevelVarAppendText(ж<testing.T> t) {
    ref var v = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv);
    Ꮡv.Set(LevelWarn);
    var buf = new slice<byte>(4, 16);
    var (data, err) = Ꮡv.AppendText(buf);
    if (err != nil) {
        t.Fatal(err);
    }
    ref var v2 = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv2);
    var errΔ1 = Ꮡv2.UnmarshalText(data[4..]); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    global::go.log.slog_package.ΔLevel g = Ꮡv2.Level();
    global::go.log.slog_package.ΔLevel w = LevelWarn; if (g != w) {
        t.Errorf("got %s, want %s", g, w);
    }
}

public static void TestLevelVarFlag(ж<testing.T> t) {
    var fs = flag.NewFlagSet(testˢ, flag.ContinueOnError);
    var v = Ꮡ(new LevelVar());
    v.Set(LevelWarn + 3);
    fs.TextVar(new slog_test_package.slog_LevelVarжTextUnmarshaler(v), levelˢ, new slog_test_package.slog_LevelVarжTextMarshaler(v), setLevelˢ);
    var err = fs.Parse(["-level", "WARN+3"]);
    if (err != nil) {
        t.Fatal(err);
    }
    global::go.log.slog_package.ΔLevel g = v.Level();
    global::go.log.slog_package.ΔLevel w = LevelWarn + 3; if (g != w) {
        t.Errorf("got %v, want %v", g, w);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string levelVarErrorˢ => ref levelVarErrorˢᵇ;
private static readonly @string levelVarErrorˢᵇ = "LevelVar(ERROR)"u8;

public static void TestLevelVarString(ж<testing.T> t) {
    ref var v = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv);
    Ꮡv.Set(LevelError);
    @string got = Ꮡv.String();
    @string want = levelVarErrorˢ;
    if (got != want) {
        t.Errorf("got %q, want %q", got, want);
    }
}

public static void BenchmarkLevelString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var levels = slice<global::go.log.slog_package.ΔLevel>([
        0,
        LevelError,
        LevelError + 2,
        LevelError - 2,
        LevelWarn,
        LevelWarn - 1,
        LevelInfo,
        LevelInfo + 1,
        LevelInfo - 3,
        LevelDebug,
        LevelDebug - 2
    ]);
    b.ResetTimer();
    while (Ꮡb.Loop()) {
        foreach (var (_, level) in range(levels)) {
            _ = level.String();
        }
    }
}

} // end slog_internal_test_package
