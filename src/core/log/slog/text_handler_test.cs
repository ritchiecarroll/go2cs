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
internal static readonly @string quOˢ = @"qu""o"u8;
internal static readonly @string aMessageˢ = "a message"u8;

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

public static void TestTextHandler(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in new TestTextHandler_type[]{
        new(
            "unquoted"u8,
            Int("a"u8, 1),
            "a"u8, "1"u8
        ),
        new(
            "quoted"u8,
            go.log.slog_package.String("x = y"u8, quOˢ),
            @"""x = y"""u8, @"""qu\""o"""u8
        ),
        new(
            "String method"u8,
            go.log.slog_package.Any(nameˢ, new name("Ren"u8, "Hoek"u8)),
            @"name"u8, @"""Hoek, Ren"""u8
        ),
        new(
            "struct"u8,
            go.log.slog_package.Any("x"u8, Ꮡ(new TestTextHandler_typeᴛ1(A: 1, b: 2))),
            @"x"u8, @"""&{A:1 b:2}"""u8
        ),
        new(
            "TextMarshaler"u8,
            go.log.slog_package.Any("t"u8, new text("abc"u8)),
            @"t"u8, @"""text{\""abc\""}"""u8
        ),
        new(
            "TextMarshaler error"u8,
            go.log.slog_package.Any("t"u8, new text(""u8)),
            @"t"u8, @"""!ERROR:text: empty string"""u8
        ),
        new(
            "nil value"u8,
            go.log.slog_package.Any("a"u8, default!),
            @"a"u8, @"<nil>"u8
        )
    }.slice()) {
        ref var test = ref heap(new TestTextHandler_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            foreach (var (_, vᴛ2) in new TestTextHandler_typeᴛ2[]{
                new(
                    "none"u8,
                    new HandlerOptions(nil),
                    @"time=2000-01-02T03:04:05.000Z level=INFO msg=""a message"""u8,
                    (@string s) => s
                ),
                new(
                    "replace"u8,
                    new HandlerOptions(ReplaceAttr: upperCaseKey),
                    @"TIME=2000-01-02T03:04:05.000Z LEVEL=INFO MSG=""a message"""u8,
                    strings.ToUpper
                )
            }.slice()) {
                ref var opts = ref heap(new TestTextHandler_typeᴛ2(), out var Ꮡopts);
                Ꮡopts.Value = vᴛ2;

                var testʗ2 = testʗ1;
                tΔ1.Run(Ꮡopts.Value.name, (ж<testing.T> tΔ2) => {
                    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                    var h = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡopts.of(TestTextHandler_typeᴛ2.Ꮡopts));
                    var r = NewRecord(testTime, LevelInfo, aMessageˢ, 0);
                    r.AddAttrs(testʗ2.attr);
                    {
                        var err = h.Handle(context.Background(), r); if (err != default!) {
                            tΔ2.Fatal(err);
                        }
                    }
                    @string got = Ꮡbuf.String();
                    // Remove final newline.
                    got = got.slice(0, len(got) - 1);
                    @string want = Ꮡopts.Value.wantPrefix + " "u8 + Ꮡopts.Value.modKey(testʗ2.wantKey) + "="u8 + testʗ2.wantVal;
                    if (got != want) {
                        tΔ2.Errorf("\ngot  %s\nwant %s"u8, got, want);
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

internal static @string String(this name n) {
    return n.Last + ", "u8 + n.First;
}

// for testing TextMarshaler
internal partial struct text {
    internal @string s;
}

internal static @string String(this text t) {
    return t.s; // should be ignored
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string textEmptyStringˢ = "text: empty string"u8;

internal static (slice<byte>, error) MarshalText(this text t) {
    if (t.s == ""u8) {
        return (default!, errors.New(textEmptyStringˢ));
    }
    return (slice<byte>(fmt.Sprintf("text{%q}"u8, t.s)), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string levelInfoMsgMDur1m0sBˢ = @"level=INFO msg=m dur=1m0s b=true a=1"u8;

public static void TestTextHandlerPreformatted(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    global::go.log.slog_package.ΔHandler h = new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil));
    h = h.WithAttrs(new global::go.log.slog_package.Attr[]{go.log.slog_package.Duration(durˢ, time_package.ΔMinute), go.log.slog_package.Bool("b"u8, true)}.slice());
    // Also test omitting time.
    var r = NewRecord(new time_package.Time(nil), 0, /* 0 Level is INFO */
 "m"u8, 0);
    r.AddAttrs(Int("a"u8, 1));
    {
        var err = h.Handle(context.Background(), r); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string got = strings.TrimSuffix(Ꮡbuf.String(), "\n"u8);
    @string want = levelInfoMsgMDur1m0sBˢ;
    if (got != want) {
        Ꮡt.Errorf("got %s, want %s"u8, got, want);
    }
}

public static void TestTextHandlerAlloc(ж<testing.T> Ꮡt) {
    testenv.SkipIfOptimizationOff(new slog_test_package.testing_TжTB(Ꮡt));
    ref var r = ref heap<global::go.log.slog_package.Record>(out var Ꮡr);
    r = NewRecord(time_package.Now(), LevelInfo, msgˢ, 0);
    for (nint i = 0; i < 10; i++) {
        r.AddAttrs(Int("x = y"u8, i));
    }
    ref var h = ref heap<global::go.log.slog_package.ΔHandler>(out var Ꮡh);

    h = new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(io.Discard, nil));
    wantAllocs(Ꮡt, 0, () => {
        Ꮡh.ValueSlot.Handle(context.Background(), Ꮡr.Value);
    });
    h = h.WithGroup("s"u8);
    r.AddAttrs(Group("g"u8, Int("a"u8, 1)));
    wantAllocs(Ꮡt, 0, () => {
        Ꮡh.ValueSlot.Handle(context.Background(), Ꮡr.Value);
    });
}

internal partial struct TestNeedsQuoting_type /*dyn*/ {
    internal @string @in;
    internal bool want;
}

public static void TestNeedsQuoting(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestNeedsQuoting_type[]{
        new(""u8, true),
        new("ab"u8, false),
        new("a=b"u8, true),
        new(@"""ab"""u8, true),
        new("\a\b"u8, true),
        new("a\tb"u8, true),
        new("µåπ"u8, false),
        new("a b"u8, true),
        new(((@string)(new byte[]{0x62, 0x61, 0x64, 0x75, 0x74, 0x66, 0x38, 0xf6})), true)
    }.slice()) {
        var got = needsQuoting(test.@in);
        if (got != test.want) {
            Ꮡt.Errorf("%q: got %t, want %t"u8, test.@in, got, test.want);
        }
    }
}

} // end slog_internal_test_package
