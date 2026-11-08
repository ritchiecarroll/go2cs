// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using go.@internal;
using static go.log.slog_package;

partial class slog_internal_test_package {

internal static time.Time testTime = time_package.Date(2000, 1, 2, 3, 4, 5, 0, time_package.ΔUTC);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string quOˢ => ref quOˢᵇ;
private static readonly @string quOˢᵇ = @"qu""o"u8;
internal static ref readonly @string aMessageˢ => ref aMessageˢᵇ;
private static readonly @string aMessageˢᵇ = "a message"u8;

internal partial struct TestTextHandler_type /*dyn*/ {
    internal @string name;
    internal global::go.log.slog_package.Attr attr;
    internal @string wantKey, wantVal;
}

internal partial struct TestTextHandler_typeᴛ1 /*dyn*/ {
    public nint A;
    internal nint b;
}

internal partial struct TestTextHandler_typeᴛ2 /*dyn*/ {
    internal @string name;
    internal global::go.log.slog_package.HandlerOptions opts;
    internal @string wantPrefix;
    internal Func<@string, @string> modKey;
}

public static void TestTextHandler(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(slice<TestTextHandler_type>([
        new(
            "unquoted",
            Int("a", 1),
            "a", "1"
        ),
        new(
            "quoted",
            go.log.slog_package.String("x = y", quOˢ),
            @"""x = y""", @"""qu\""o"""
        ),
        new(
            "String method",
            go.log.slog_package.Any(nameˢ, new name("Ren", "Hoek")),
            @"name", @"""Hoek, Ren"""
        ),
        new(
            "struct",
            go.log.slog_package.Any("x", Ꮡ(new TestTextHandler_typeᴛ1(A: 1, b: 2))),
            @"x", @"""&{A:1 b:2}"""
        ),
        new(
            "TextMarshaler",
            go.log.slog_package.Any("t", new text("abc")),
            @"t", @"""text{\""abc\""}"""
        ),
        new(
            "TextMarshaler error",
            go.log.slog_package.Any("t", new text("")),
            @"t", @"""!ERROR:text: empty string"""
        ),
        new(
            "nil value",
            go.log.slog_package.Any("a", default),
            @"a", @"<nil>"
        )
    ]))) {
        ref var test = ref heap(new TestTextHandler_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            foreach (var (_, vᴛ2) in range(slice<TestTextHandler_typeᴛ2>([
                new(
                    "none",
                    new HandlerOptions(),
                    @"time=2000-01-02T03:04:05.000Z level=INFO msg=""a message""",
                    (@string s) => s
                ),
                new(
                    "replace",
                    new HandlerOptions(ReplaceAttr: upperCaseKey),
                    @"TIME=2000-01-02T03:04:05.000Z LEVEL=INFO MSG=""a message""",
                    strings.ToUpper
                )
            ]))) {
                ref var opts = ref heap(new TestTextHandler_typeᴛ2(), out var Ꮡopts);
                Ꮡopts.Value = vᴛ2;

                var testʗ2 = testʗ1;
                tΔ1.Run(Ꮡopts.Value.name, (ж<testing.T> tΔ2) => {
                    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                    var h = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡopts.of(TestTextHandler_typeᴛ2.Ꮡopts));
                    var r = NewRecord(testTime, LevelInfo, aMessageˢ, 0);
                    r.AddAttrs(testʗ2.attr);
                    var err = h.Handle(context.Background(), r); if (err != nil) {
                        tΔ2.Fatal(err);
                    }
                    @string got = Ꮡbuf.String();
                    // Remove final newline.
                    got = got.slice(0, len(got) - 1);
                    @string want = Ꮡopts.Value.wantPrefix + " "u8 + Ꮡopts.Value.modKey(testʗ2.wantKey) + "="u8 + testʗ2.wantVal;
                    if (got != want) {
                        tΔ2.Errorf("\ngot  %s\nwant %s", got, want);
                    }
                });
            }
        });
    }
}

// for testing fmt.Sprint
internal partial struct name {
    public @string First, Last;
}

internal static @string String(this name n) { return n.Last + ", "u8 + n.First; }

// for testing TextMarshaler
internal partial struct text {
    internal @string s;
}

internal static @string String(this text t) { return t.s; } // should be ignored

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string textEmptyStringˢ => ref textEmptyStringˢᵇ;
private static readonly @string textEmptyStringˢᵇ = "text: empty string"u8;

internal static (slice<byte>, error) MarshalText(this text t) {
    if (t.s == ""u8) {
        return (default, errors.New(textEmptyStringˢ));
    }
    return (slice<byte>(fmt.Sprintf("text{%q}"u8, t.s)), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string levelInfoMsgMDur1m0sBˢ => ref levelInfoMsgMDur1m0sBˢᵇ;
private static readonly @string levelInfoMsgMDur1m0sBˢᵇ = @"level=INFO msg=m dur=1m0s b=true a=1"u8;

public static void TestTextHandlerPreformatted(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    global::go.log.slog_package.ΔHandler h = new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil));
    h = h.WithAttrs(slice<global::go.log.slog_package.Attr>([go.log.slog_package.Duration(durˢ, time_package.ΔMinute), go.log.slog_package.Bool("b", true)]));
    // Also test omitting time.
    var r = NewRecord(new time_package.Time(), 0, /* 0 Level is INFO */ "m", 0);
    r.AddAttrs(Int("a", 1));
    var err = h.Handle(context.Background(), r); if (err != nil) {
        t.Fatal(err);
    }
    @string got = strings.TrimSuffix(Ꮡbuf.String(), "\n");
    @string want = levelInfoMsgMDur1m0sBˢ;
    if (got != want) {
        t.Errorf("got %s, want %s", got, want);
    }
}

public static void TestTextHandlerAlloc(ж<testing.T> t) {
    testenv.SkipIfOptimizationOff(new slog_test_package.testing_TжTB(t));
    ref var r = ref heap<global::go.log.slog_package.Record>(NewRecord(time_package.Now(), LevelInfo, msgˢ, 0), out var Ꮡr);
    for (nint i = 0; i < 10; i++) {
        r.AddAttrs(Int("x = y", i));
    }
    ref var h = ref heap<global::go.log.slog_package.ΔHandler>(out var Ꮡh);

    h = new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(io.Discard, nil));
    wantAllocs(t, 0, () => {
        Ꮡh.ValueSlot.Handle(context.Background(), Ꮡr.Value);
    });

    h = h.WithGroup("s");
    r.AddAttrs(Group("g", Int("a", 1)));
    wantAllocs(t, 0, () => {
        Ꮡh.ValueSlot.Handle(context.Background(), Ꮡr.Value);
    });
}

internal partial struct TestNeedsQuoting_type /*dyn*/ {
    internal @string @in;
    internal bool want;
}

public static void TestNeedsQuoting(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestNeedsQuoting_type>([
        new("", true),
        new("ab", false),
        new("a=b", true),
        new(@"""ab""", true),
        new("\a\b", true),
        new("a\tb", true),
        new("µåπ", false),
        new("a b", true),
        new(((@string)(new byte[]{0x62, 0x61, 0x64, 0x75, 0x74, 0x66, 0x38, 0xf6})), true)
    ]))) {
        var got = needsQuoting(test.@in);
        if (got != test.want) {
            t.Errorf("%q: got %t, want %t", test.@in, got, test.want);
        }
    }
}

} // end slog_internal_test_package
