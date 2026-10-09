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

public static void TestLevelString(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestLevelString_type[]{
        new(0, "INFO"u8),
        new(LevelError, "ERROR"u8),
        new(LevelError + 2, "ERROR+2"u8),
        new(LevelError - 2, "WARN+2"u8),
        new(LevelWarn, "WARN"u8),
        new(LevelWarn - 1, "INFO+3"u8),
        new(LevelInfo, "INFO"u8),
        new(LevelInfo + 1, "INFO+1"u8),
        new(LevelInfo - 3, "DEBUG+1"u8),
        new(LevelDebug, "DEBUG"u8),
        new(LevelDebug - 2, "DEBUG-2"u8)
    }.slice()) {
        @string got = test.@in.String();
        if (got != test.want) {
            Ꮡt.Errorf("%d: got %s, want %s"u8, test.@in, got, test.want);
        }
    }
}

public static void TestLevelVar(ж<testing.T> Ꮡt) {
    ref var al = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡal);
    {
        global::go.log.slog_package.ΔLevel got = Ꮡal.Level();
        global::go.log.slog_package.ΔLevel want = LevelInfo; if (got != want) {
            Ꮡt.Errorf("got %v, want %v"u8, got, want);
        }
    }
    Ꮡal.Set(LevelWarn);
    {
        global::go.log.slog_package.ΔLevel got = Ꮡal.Level();
        global::go.log.slog_package.ΔLevel want = LevelWarn; if (got != want) {
            Ꮡt.Errorf("got %v, want %v"u8, got, want);
        }
    }
    Ꮡal.Set(LevelInfo);
    {
        global::go.log.slog_package.ΔLevel got = Ꮡal.Level();
        global::go.log.slog_package.ΔLevel want = LevelInfo; if (got != want) {
            Ꮡt.Errorf("got %v, want %v"u8, got, want);
        }
    }
}

public static void TestLevelMarshalJSON(ж<testing.T> Ꮡt) {
    global::go.log.slog_package.ΔLevel want = LevelWarn - 3;
    var wantData = slice<byte>(@"""INFO+1"""u8);
    var (data, err) = want.MarshalJSON();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(data, wantData)) {
        Ꮡt.Errorf("got %s, want %s"u8, ((@string)data), ((@string)wantData));
    }
    ref var got = ref heap(new global::go.log.slog_package.ΔLevel(), out var Ꮡgot);
    {
        var errΔ1 = Ꮡgot.UnmarshalJSON(data); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    if (got != want) {
        Ꮡt.Errorf("got %s, want %s"u8, got, want);
    }
}

public static void TestLevelMarshalText(ж<testing.T> Ꮡt) {
    global::go.log.slog_package.ΔLevel want = LevelWarn - 3;
    var wantData = slice<byte>("INFO+1"u8);
    var (data, err) = want.MarshalText();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(data, wantData)) {
        Ꮡt.Errorf("got %s, want %s"u8, ((@string)data), ((@string)wantData));
    }
    ref var got = ref heap(new global::go.log.slog_package.ΔLevel(), out var Ꮡgot);
    {
        var errΔ1 = Ꮡgot.UnmarshalText(data); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    if (got != want) {
        Ꮡt.Errorf("got %s, want %s"u8, got, want);
    }
}

public static void TestLevelAppendText(ж<testing.T> Ꮡt) {
    var buf = new slice<byte>(4, 16);
    global::go.log.slog_package.ΔLevel want = LevelWarn - 3;
    var wantData = slice<byte>("\x00\x00\x00\x00INFO+1"u8);
    var (data, err) = want.AppendText(buf);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(data, wantData)) {
        Ꮡt.Errorf("got %s, want %s"u8, ((@string)data), ((@string)wantData));
    }
}

internal partial struct TestLevelParse_type /*dyn*/ {
    internal @string @in;
    internal global::go.log.slog_package.ΔLevel want;
}

public static void TestLevelParse(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestLevelParse_type[]{
        new("DEBUG"u8, LevelDebug),
        new("INFO"u8, LevelInfo),
        new("WARN"u8, LevelWarn),
        new("ERROR"u8, LevelError),
        new("debug"u8, LevelDebug),
        new("iNfo"u8, LevelInfo),
        new("INFO+87"u8, LevelInfo + 87),
        new("Error-18"u8, LevelError - 18),
        new("Error-8"u8, LevelInfo)
    }.slice()) {
        ref var got = ref heap(new global::go.log.slog_package.ΔLevel(), out var Ꮡgot);
        {
            var err = Ꮡgot.parse(test.@in); if (err != default!) {
                Ꮡt.Fatalf("%q: %v"u8, test.@in, err);
            }
        }
        if (got != test.want) {
            Ꮡt.Errorf("%q: got %s, want %s"u8, test.@in, got, test.want);
        }
    }
}

internal partial struct TestLevelParseError_type /*dyn*/ {
    internal @string @in;
    internal @string want; // error string should contain this
}

public static void TestLevelParseError(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestLevelParseError_type[]{
        new(""u8, "unknown name"u8),
        new("dbg"u8, "unknown name"u8),
        new("INFO+"u8, "invalid syntax"u8),
        new("INFO-"u8, "invalid syntax"u8),
        new("ERROR+23x"u8, "invalid syntax"u8)
    }.slice()) {
        ref var l = ref heap(new global::go.log.slog_package.ΔLevel(), out var Ꮡl);
        var err = Ꮡl.parse(test.@in);
        if (err == default! || !strings.Contains(err.Error(), test.want)) {
            Ꮡt.Errorf("%q: got %v, want string containing %q"u8, test.@in, err, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "test"u8;
internal static readonly @string levelˢ = "level"u8;
internal static readonly @string setLevelˢ = "set level"u8;

public static void TestLevelFlag(ж<testing.T> Ꮡt) {
    var fs = flag.NewFlagSet(testˢ, flag.ContinueOnError);
    ref var lf = ref heap<global::go.log.slog_package.ΔLevel>(out var Ꮡlf);
    lf = LevelInfo;
    fs.TextVar(new slog_test_package.slog_ΔLevelжTextUnmarshaler(Ꮡlf), levelˢ, new slog_test_package.slog_ΔLevelᴠTextMarshaler(lf), setLevelˢ);
    var err = fs.Parse(new @string[]{"-level"u8, "WARN+3"u8}.slice());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        global::go.log.slog_package.ΔLevel g = lf;
        global::go.log.slog_package.ΔLevel w = LevelWarn + 3; if (g != w) {
            Ꮡt.Errorf("got %v, want %v"u8, g, w);
        }
    }
}

public static void TestLevelVarMarshalText(ж<testing.T> Ꮡt) {
    ref var v = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv);
    Ꮡv.Set(LevelWarn);
    var (data, err) = Ꮡv.MarshalText();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var v2 = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv2);
    {
        var errΔ1 = Ꮡv2.UnmarshalText(data); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        global::go.log.slog_package.ΔLevel g = Ꮡv2.Level();
        global::go.log.slog_package.ΔLevel w = LevelWarn; if (g != w) {
            Ꮡt.Errorf("got %s, want %s"u8, g, w);
        }
    }
}

public static void TestLevelVarAppendText(ж<testing.T> Ꮡt) {
    ref var v = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv);
    Ꮡv.Set(LevelWarn);
    var buf = new slice<byte>(4, 16);
    var (data, err) = Ꮡv.AppendText(buf);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var v2 = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv2);
    {
        var errΔ1 = Ꮡv2.UnmarshalText(data[4..]); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        global::go.log.slog_package.ΔLevel g = Ꮡv2.Level();
        global::go.log.slog_package.ΔLevel w = LevelWarn; if (g != w) {
            Ꮡt.Errorf("got %s, want %s"u8, g, w);
        }
    }
}

public static void TestLevelVarFlag(ж<testing.T> Ꮡt) {
    var fs = flag.NewFlagSet(testˢ, flag.ContinueOnError);
    var v = Ꮡ(new LevelVar(nil));
    v.Set(LevelWarn + 3);
    fs.TextVar(new slog_test_package.slog_LevelVarжTextUnmarshaler(v), levelˢ, new slog_test_package.slog_LevelVarжTextMarshaler(v), setLevelˢ);
    var err = fs.Parse(new @string[]{"-level"u8, "WARN+3"u8}.slice());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        global::go.log.slog_package.ΔLevel g = v.Level();
        global::go.log.slog_package.ΔLevel w = LevelWarn + 3; if (g != w) {
            Ꮡt.Errorf("got %v, want %v"u8, g, w);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string levelVarErrorˢ = "LevelVar(ERROR)"u8;

public static void TestLevelVarString(ж<testing.T> Ꮡt) {
    ref var v = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡv);
    Ꮡv.Set(LevelError);
    @string got = Ꮡv.String();
    @string want = levelVarErrorˢ;
    if (got != want) {
        Ꮡt.Errorf("got %q, want %q"u8, got, want);
    }
}

} // end slog_internal_test_package
