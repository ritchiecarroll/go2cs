// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// TODO: verify that the output of Marshal{Text,JSON} is suitably escaped.
namespace go.log;

using bytes = bytes_package;
using context = context_package;
using json = go.encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using buffer = go.log.slog.@internal.buffer_package;
using os = os_package;
using filepath = path.filepath_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.encoding;
using go.log.slog.@internal;
using path;
using static go.log.slog_package;
using ꓸꓸꓸstring = Span<@string>;

partial class slog_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string preˢ => ref preˢᵇ;
private static readonly @string preˢᵇ = "pre"u8;
internal static ref readonly @string twoˢ => ref twoˢᵇ;
private static readonly @string twoˢᵇ = "two"u8;
internal static ref readonly @string messageˢ => ref messageˢᵇ;
private static readonly @string messageˢᵇ = "message"u8;

internal partial struct TestDefaultHandle_type /*dyn*/ {
    internal @string name;
    internal Func<global::go.log.slog_package.ΔHandler, global::go.log.slog_package.ΔHandler> with;
    internal slice<global::go.log.slog_package.Attr> attrs;
    internal @string want;
}

public static void TestDefaultHandle(ж<testing.T> t) {
    var ctx = context.Background();
    var preAttrs = slice<global::go.log.slog_package.Attr>([Int(preˢ, 0)]);
    var attrs = slice<global::go.log.slog_package.Attr>([Int("a", 1), go.log.slog_package.String("b", twoˢ)]);
            var preAttrsʗ1 = preAttrs;

            var preAttrsʗ2 = preAttrs;


    foreach (var (_, vᴛ1) in slice<TestDefaultHandle_type>([
        new(
            name: "no attrs",
            want: "INFO message"
        ),
        new(
            name: "attrs",
            attrs: attrs,
            want: "INFO message a=1 b=two"
        ),
        new(
            name: "preformatted",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ1),
            attrs: attrs,
            want: "INFO message pre=0 a=1 b=two"
        ),
        new(
            name: "groups",
            attrs: slice<global::go.log.slog_package.Attr>([
                Int("a", 1),
                Group("g",
                    Int("b", 2),
                    Group("h", Int("c", 3)),
                    Int("d", 4)),
                Int("e", 5)
            ]),
            want: "INFO message a=1 g.b=2 g.h.c=3 g.d=4 e=5"
        ),
        new(
            name: "group",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ2).WithGroup("s"),
            attrs: attrs,
            want: "INFO message pre=0 s.a=1 s.b=two"
        ),
        new(
            name: "preformatted groups",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(slice<global::go.log.slog_package.Attr>([Int("p1", 1)])).WithGroup("s1").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("p2", 2)])).WithGroup("s2"),
            attrs: attrs,
            want: "INFO message p1=1 s1.p2=2 s1.s2.a=1 s1.s2.b=two"
        ),
        new(
            name: "two with-groups",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(slice<global::go.log.slog_package.Attr>([Int("p1", 1)])).WithGroup("s1").WithGroup("s2"),
            attrs: attrs,
            want: "INFO message p1=1 s1.s2.a=1 s1.s2.b=two"
        )
    ])) {
        ref var test = ref heap(new TestDefaultHandle_type(), out var Ꮡtest);
        test = vᴛ1;

        var ctxʗ1 = ctx;
        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            @string got = default;

            global::go.log.slog_package.ΔHandler h = new global::go.log.slog_package.defaultHandlerжΔHandler(newDefaultHandler((uintptr _, slice<byte> b) => {
                got = (@string)b;
                return default;
            }));
            if (testʗ1.with != nil) {
                h = testʗ1.with(h);
            }
            var r = NewRecord(new time_package.Time(), LevelInfo, messageˢ, 0);
            r.AddAttrs(testʗ1.attrs.ꓸꓸꓸ);
            {
                var err = h.Handle(ctxʗ1, r); if (err != nil) {
                    tΔ1.Fatal(err);
                }
            }
            if (got != testʗ1.want) {
                tΔ1.Errorf("\ngot  %s\nwant %s", got, testʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sub1ˢ => ref sub1ˢᵇ;
private static readonly @string sub1ˢᵇ = "sub1"u8;
internal static ref readonly @string sub2ˢ => ref sub2ˢᵇ;
private static readonly @string sub2ˢᵇ = "sub2"u8;
internal static ref readonly @string helloFromSub1ˢ => ref helloFromSub1ˢᵇ;
private static readonly @string helloFromSub1ˢᵇ = "hello from sub1"u8;
internal static ref readonly @string helloFromSub2ˢ => ref helloFromSub2ˢᵇ;
private static readonly @string helloFromSub2ˢᵇ = "hello from sub2"u8;

public static void TestConcurrentWrites(ж<testing.T> t) {
    var ctx = context.Background();
    nint count = 1000;
    foreach (var (_, handlerType) in slice<@string>(["text", "json"])) {
        var ctxʗ1 = ctx;
        t.Run(handlerType, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            global::go.log.slog_package.ΔHandler h = default;
            var exprᴛ1 = handlerType;
            switch (ᐧ) {
            case {} when exprᴛ1 == "text"u8: {
                h = new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil));
                break;
            }
            case {} when exprᴛ1 == "json"u8: {
                h = new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil));
                break;
            }
            default: {
                tΔ1.Fatalf("unexpected handlerType %q", handlerType);
                break;
            }}
            var sub1 = h.WithAttrs(slice<global::go.log.slog_package.Attr>([go.log.slog_package.Bool(sub1ˢ, true)]));
            var sub2 = h.WithAttrs(slice<global::go.log.slog_package.Attr>([go.log.slog_package.Bool(sub2ˢ, true)]));
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            for (nint i = 0; i < count; i++) {
                ref var sub1Record = ref heap<global::go.log.slog_package.Record>(NewRecord(new time_package.Time(), LevelInfo, helloFromSub1ˢ, 0), out var Ꮡsub1Record);
                Ꮡsub1Record.Value.AddAttrs(Int("i", i));
                ref var sub2Record = ref heap<global::go.log.slog_package.Record>(NewRecord(new time_package.Time(), LevelInfo, helloFromSub2ˢ, 0), out var Ꮡsub2Record);
                Ꮡsub2Record.Value.AddAttrs(Int("i", i));
                Ꮡwg.Add(1);
                var ctxʗ2 = ctxʗ1;
                var sub1ʗ1 = sub1;
                var sub2ʗ1 = sub2;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        {
                            var err = sub1ʗ1.Handle(ctxʗ2, Ꮡsub1Record.Value); if (err != nil) {
                                tΔ1.Error(err);
                            }
                        }
                        {
                            var err = sub2ʗ1.Handle(ctxʗ2, Ꮡsub2Record.Value); if (err != nil) {
                                tΔ1.Error(err);
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
            Ꮡwg.Wait();
            for (nint i = 1; i <= 2; i++) {
                @string want = "hello from sub"u8 + strconv.Itoa(i);
                nint n = strings.Count(Ꮡbuf.String(), want);
                if (n != count) {
                    tΔ1.Fatalf("want %d occurrences of %q, got %d", count, want, n);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string oneˢ => ref oneˢᵇ;
private static readonly @string oneˢᵇ = "one"u8;
internal static ref readonly @string bCX2Eˢ => ref bCX2Eˢᵇ;
private static readonly @string bCX2Eˢᵇ = " b.c=\"\\x2E\t"u8;
internal static ref readonly @string nameˢ => ref nameˢᵇ;
private static readonly @string nameˢᵇ = "name"u8;
internal static readonly object minsˢ = (@string)"mins"u8;
internal static readonly object secsˢ = (@string)"secs"u8;
internal static ref readonly @string prefixˢ => ref prefixˢᵇ;
private static readonly @string prefixˢᵇ = "prefix"u8;
internal static ref readonly @string needsQuotingˢ => ref needsQuotingˢᵇ;
private static readonly @string needsQuotingˢᵇ = " needs quoting "u8;
internal static ref readonly @string notNeedsQuotingˢ => ref notNeedsQuotingˢᵇ;
private static readonly @string notNeedsQuotingˢᵇ = "NotNeedsQuoting"u8;
internal static ref readonly @string prefixNeedsQuotingˢ => ref prefixNeedsQuotingˢᵇ;
private static readonly @string prefixNeedsQuotingˢᵇ = "prefix needs quoting"u8;
internal static readonly object sourceIsNilˢ = (@string)"source is nil"u8;
internal static ref readonly @string lineˢ2 => ref lineˢ2ᵇ;
private static readonly @string lineˢ2ᵇ = "$LINE"u8;

internal partial struct TestJSONAndTextHandlers_type /*dyn*/ {
    internal @string name;
    internal Func<slice<@string>, global::go.log.slog_package.Attr, global::go.log.slog_package.Attr> replace;
    internal bool addSource;
    internal Func<global::go.log.slog_package.ΔHandler, global::go.log.slog_package.ΔHandler> with;
    internal slice<global::go.log.slog_package.Attr> preAttrs;
    internal slice<global::go.log.slog_package.Attr> attrs;
    internal @string wantText;
    internal @string wantJSON;
}

internal partial struct TestJSONAndTextHandlers_typeᴛ1 /*dyn*/ {
    internal @string name;
    internal global::go.log.slog_package.ΔHandler h;
    internal @string want;
}

// Verify the common parts of TextHandler and JSONHandler.
public static void TestJSONAndTextHandlers(ж<testing.T> t) {
    // remove all Attrs
    var removeAll = (slice<@string> _, global::go.log.slog_package.Attr a) => new Attr();

    var attrs = slice<global::go.log.slog_package.Attr>([go.log.slog_package.String("a", oneˢ), Int("b", 2), go.log.slog_package.Any(""u8, default)]);
    var preAttrs = slice<global::go.log.slog_package.Attr>([Int(preˢ, 3), go.log.slog_package.String("x", "y")]);

            var preAttrsʗ1 = preAttrs;

            var preAttrsʗ2 = preAttrs;

            var preAttrsʗ3 = preAttrs;

            var preAttrsʗ4 = preAttrs;

            var preAttrsʗ5 = preAttrs;























    foreach (var (_, vᴛ1) in slice<TestJSONAndTextHandlers_type>([
        new(
            name: "basic",
            attrs: attrs,
            wantText: "time=2000-01-02T03:04:05.000Z level=INFO msg=message a=one b=2",
            wantJSON: @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""message"",""a"":""one"",""b"":2}"
        ),
        new(
            name: "empty key",
            attrs: builtin.append(slices.Clip<slice<global::go.log.slog_package.Attr>, global::go.log.slog_package.Attr>(attrs), go.log.slog_package.Any(""u8, "v")),
            wantText: @"time=2000-01-02T03:04:05.000Z level=INFO msg=message a=one b=2 """"=v",
            wantJSON: @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""message"",""a"":""one"",""b"":2,"""":""v""}"
        ),
        new(
            name: "cap keys",
            replace: upperCaseKey,
            attrs: attrs,
            wantText: "TIME=2000-01-02T03:04:05.000Z LEVEL=INFO MSG=message A=one B=2",
            wantJSON: @"{""TIME"":""2000-01-02T03:04:05Z"",""LEVEL"":""INFO"",""MSG"":""message"",""A"":""one"",""B"":2}"
        ),
        new(
            name: "remove all",
            replace: removeAll,
            attrs: attrs,
            wantText: "",
            wantJSON: @"{}"
        ),
        new(
            name: "preformatted",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ1),
            preAttrs: preAttrs,
            attrs: attrs,
            wantText: "time=2000-01-02T03:04:05.000Z level=INFO msg=message pre=3 x=y a=one b=2",
            wantJSON: @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""message"",""pre"":3,""x"":""y"",""a"":""one"",""b"":2}"
        ),
        new(
            name: "preformatted cap keys",
            replace: upperCaseKey,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ2),
            preAttrs: preAttrs,
            attrs: attrs,
            wantText: "TIME=2000-01-02T03:04:05.000Z LEVEL=INFO MSG=message PRE=3 X=y A=one B=2",
            wantJSON: @"{""TIME"":""2000-01-02T03:04:05Z"",""LEVEL"":""INFO"",""MSG"":""message"",""PRE"":3,""X"":""y"",""A"":""one"",""B"":2}"
        ),
        new(
            name: "preformatted remove all",
            replace: removeAll,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ3),
            preAttrs: preAttrs,
            attrs: attrs,
            wantText: "",
            wantJSON: "{}"
        ),
        new(
            name: "remove built-in",
            replace: removeKeys(TimeKey, LevelKey, MessageKey),
            attrs: attrs,
            wantText: "a=one b=2",
            wantJSON: @"{""a"":""one"",""b"":2}"
        ),
        new(
            name: "preformatted remove built-in",
            replace: removeKeys(TimeKey, LevelKey, MessageKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ4),
            attrs: attrs,
            wantText: "pre=3 x=y a=one b=2",
            wantJSON: @"{""pre"":3,""x"":""y"",""a"":""one"",""b"":2}"
        ),
        new(
            name: "groups",
            replace: removeKeys(TimeKey, LevelKey), // to simplify the result
            attrs: slice<global::go.log.slog_package.Attr>([
                Int("a", 1),
                Group("g",
                    Int("b", 2),
                    Group("h", Int("c", 3)),
                    Int("d", 4)),
                Int("e", 5)
            ]),
            wantText: "msg=message a=1 g.b=2 g.h.c=3 g.d=4 e=5",
            wantJSON: @"{""msg"":""message"",""a"":1,""g"":{""b"":2,""h"":{""c"":3},""d"":4},""e"":5}"
        ),
        new(
            name: "empty group",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([Group("g"), Group("h", Int("a", 1))]),
            wantText: "msg=message h.a=1",
            wantJSON: @"{""msg"":""message"",""h"":{""a"":1}}"
        ),
        new(
            name: "nested empty group",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([
                Group("g",
                    Group("h",
                        Group("i"), Group("j")))
            ]),
            wantText: @"msg=message",
            wantJSON: @"{""msg"":""message""}"
        ),
        new(
            name: "nested non-empty group",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([
                Group("g",
                    Group("h",
                        Group("i"), Group("j", Int("a", 1))))
            ]),
            wantText: @"msg=message g.h.j.a=1",
            wantJSON: @"{""msg"":""message"",""g"":{""h"":{""j"":{""a"":1}}}}"
        ),
        new(
            name: "escapes",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([
                go.log.slog_package.String("a b", "x\t\n\u0000y"),
                Group(bCX2Eˢ,
                    go.log.slog_package.String("d=e", "f.g\""),
                    Int("m.d", 1)) // dot is not escaped
            ]),
            wantText: @"msg=message ""a b""=""x\t\n\x00y"" "" b.c=\""\\x2E\t.d=e""=""f.g\"""" "" b.c=\""\\x2E\t.m.d""=1",
            wantJSON: @"{""msg"":""message"",""a b"":""x\t\n\u0000y"","" b.c=\""\\x2E\t"":{""d=e"":""f.g\"""",""m.d"":1}}"
        ),
        new(
            name: "LogValuer",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([
                Int("a", 1),
                go.log.slog_package.Any(nameˢ, new logValueName("Ren", "Hoek")),
                Int("b", 2)
            ]),
            wantText: "msg=message a=1 name.first=Ren name.last=Hoek b=2",
            wantJSON: @"{""msg"":""message"",""a"":1,""name"":{""first"":""Ren"",""last"":""Hoek""},""b"":2}"
        ),
        new(
            // Test resolution when there is no ReplaceAttr function.
            name: "resolve",
            attrs: slice<global::go.log.slog_package.Attr>([
                go.log.slog_package.Any(""u8, Ꮡ(new replace(new Value()))), // should be elided
                go.log.slog_package.Any(nameˢ, new logValueName("Ren", "Hoek"))
            ]),
            wantText: "time=2000-01-02T03:04:05.000Z level=INFO msg=message name.first=Ren name.last=Hoek",
            wantJSON: @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""message"",""name"":{""first"":""Ren"",""last"":""Hoek""}}"
        ),
        new(
            name: "with-group",
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ5).WithGroup("s"),
            attrs: attrs,
            wantText: "msg=message pre=3 x=y s.a=one s.b=2",
            wantJSON: @"{""msg"":""message"",""pre"":3,""x"":""y"",""s"":{""a"":""one"",""b"":2}}"
        ),
        new(
            name: "preformatted with-groups",
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(slice<global::go.log.slog_package.Attr>([Int("p1", 1)])).WithGroup("s1").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("p2", 2)])).WithGroup("s2").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("p3", 3)])),
            attrs: attrs,
            wantText: "msg=message p1=1 s1.p2=2 s1.s2.p3=3 s1.s2.a=one s1.s2.b=2",
            wantJSON: @"{""msg"":""message"",""p1"":1,""s1"":{""p2"":2,""s2"":{""p3"":3,""a"":""one"",""b"":2}}}"
        ),
        new(
            name: "two with-groups",
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(slice<global::go.log.slog_package.Attr>([Int("p1", 1)])).WithGroup("s1").WithGroup("s2"),
            attrs: attrs,
            wantText: "msg=message p1=1 s1.s2.a=one s1.s2.b=2",
            wantJSON: @"{""msg"":""message"",""p1"":1,""s1"":{""s2"":{""a"":""one"",""b"":2}}}"
        ),
        new(
            name: "empty with-groups",
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("x").WithGroup("y"),
            wantText: "msg=message",
            wantJSON: @"{""msg"":""message""}"
        ),
        new(
            name: "empty with-groups, no non-empty attrs",
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("x").WithAttrs(slice<global::go.log.slog_package.Attr>([Group("g")])).WithGroup("y"),
            wantText: "msg=message",
            wantJSON: @"{""msg"":""message""}"
        ),
        new(
            name: "one empty with-group",
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("x").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("a", 1)])).WithGroup("y"),
            attrs: slice<global::go.log.slog_package.Attr>([Group("g", Group("h"))]),
            wantText: "msg=message x.a=1",
            wantJSON: @"{""msg"":""message"",""x"":{""a"":1}}"
        ),
        new(
            name: "GroupValue as Attr value",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([new("v", AnyValue(IntValue(3)))]),
            wantText: "msg=message v=3",
            wantJSON: @"{""msg"":""message"",""v"":3}"
        ),
        new(
            name: "byte slice",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([go.log.slog_package.Any("bs", slice<byte>([1, 2, 3, 4]))]),
            wantText: @"msg=message bs=""\x01\x02\x03\x04""",
            wantJSON: @"{""msg"":""message"",""bs"":""AQIDBA==""}"
        ),
        new(
            name: "json.RawMessage",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([go.log.slog_package.Any("bs", (json.RawMessage)slice<byte>("1234"u8))]),
            wantText: @"msg=message bs=""1234""",
            wantJSON: @"{""msg"":""message"",""bs"":1234}"
        ),
        new(
            name: "inline group",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([
                Int("a", 1),
                Group(""u8, Int("b", 2), Int("c", 3)),
                Int("d", 4)
            ]),
            wantText: @"msg=message a=1 b=2 c=3 d=4",
            wantJSON: @"{""msg"":""message"",""a"":1,""b"":2,""c"":3,""d"":4}"
        ),
        new(
            name: "Source",
            replace: (slice<@string> gs, global::go.log.slog_package.Attr a) => {
                if (a.Key == SourceKey) {
                    var s = a.Value.Any()._<ж<global::go.log.slog_package.ΔSource>>();
                    s.File = filepath.Base(s.File);
                    return go.log.slog_package.Any(a.Key, s.OrTypedNil());
                }
                return removeKeys(TimeKey, LevelKey)(gs, a);
            },
            addSource: true,
            wantText: @"source=handler_test.go:$LINE msg=message",
            wantJSON: @"{""source"":{""function"":""log/slog.TestJSONAndTextHandlers"",""file"":""handler_test.go"",""line"":$LINE},""msg"":""message""}"
        ),
        new(
            name: "replace built-in with group",
            replace: (slice<@string> _, global::go.log.slog_package.Attr a) => {
                if (a.Key == TimeKey) {
                    return Group(TimeKey, minsˢ, (nint)3, secsˢ, (nint)2);
                }
                if (a.Key == LevelKey) {
                    return new Attr();
                }
                return a;
            },
            wantText: @"time.mins=3 time.secs=2 msg=message",
            wantJSON: @"{""time"":{""mins"":3,""secs"":2},""msg"":""message""}"
        ),
        new(
            name: "replace empty",
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(),
            attrs: slice<global::go.log.slog_package.Attr>([Group("g", Int("a", 1))]),
            wantText: "",
            wantJSON: @"{}"
        ),
        new(
            name: "replace empty 1",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("a", 1)])),
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(),
            attrs: slice<global::go.log.slog_package.Attr>([Group("h", Int("b", 2))]),
            wantText: "",
            wantJSON: @"{}"
        ),
        new(
            name: "replace empty 2",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("a", 1)])).WithGroup("h").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("b", 2)])),
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(),
            attrs: slice<global::go.log.slog_package.Attr>([Group("i", Int("c", 3))]),
            wantText: "",
            wantJSON: @"{}"
        ),
        new(
            name: "replace empty 3",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g"),
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(),
            attrs: slice<global::go.log.slog_package.Attr>([Int("a", 1)]),
            wantText: "",
            wantJSON: @"{}"
        ),
        new(
            name: "replace empty inline",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("a", 1)])).WithGroup("h").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("b", 2)])),
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(),
            attrs: slice<global::go.log.slog_package.Attr>([Group(""u8, Int("c", 3))]),
            wantText: "",
            wantJSON: @"{}"
        ),
        new(
            name: "replace partial empty attrs 1",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("a", 1)])).WithGroup("h").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("b", 2)])),
            replace: (slice<@string> groups, global::go.log.slog_package.Attr attr) => removeKeys(TimeKey, LevelKey, MessageKey, "a")(groups, attr),
            attrs: slice<global::go.log.slog_package.Attr>([Group("i", Int("c", 3))]),
            wantText: "g.h.b=2 g.h.i.c=3",
            wantJSON: @"{""g"":{""h"":{""b"":2,""i"":{""c"":3}}}}"
        ),
        new(
            name: "replace partial empty attrs 2",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("a", 1)])).WithAttrs(slice<global::go.log.slog_package.Attr>([Int("n", 4)])).WithGroup("h").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("b", 2)])),
            replace: (slice<@string> groups, global::go.log.slog_package.Attr attr) => removeKeys(TimeKey, LevelKey, MessageKey, "a", "b")(groups, attr),
            attrs: slice<global::go.log.slog_package.Attr>([Group("i", Int("c", 3))]),
            wantText: "g.n=4 g.h.i.c=3",
            wantJSON: @"{""g"":{""n"":4,""h"":{""i"":{""c"":3}}}}"
        ),
        new(
            name: "replace partial empty attrs 3",
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("x", 0)])).WithAttrs(slice<global::go.log.slog_package.Attr>([Int("a", 1)])).WithAttrs(slice<global::go.log.slog_package.Attr>([Int("n", 4)])).WithGroup("h").WithAttrs(slice<global::go.log.slog_package.Attr>([Int("b", 2)])),
            replace: (slice<@string> groups, global::go.log.slog_package.Attr attr) => removeKeys(TimeKey, LevelKey, MessageKey, "a", "c")(groups, attr),
            attrs: slice<global::go.log.slog_package.Attr>([Group("i", Int("c", 3))]),
            wantText: "g.x=0 g.n=4 g.h.b=2",
            wantJSON: @"{""g"":{""x"":0,""n"":4,""h"":{""b"":2}}}"
        ),
        new(
            name: "replace resolved group",
            replace: (slice<@string> groups, global::go.log.slog_package.Attr a) => {
                if (a.Value.Kind() == KindGroup) {
                    return new Attr("bad", IntValue(1));
                }
                return removeKeys(TimeKey, LevelKey, MessageKey)(groups, a);
            },
            attrs: slice<global::go.log.slog_package.Attr>([go.log.slog_package.Any(nameˢ, new logValueName("Perry", "Platypus"))]),
            wantText: "name.first=Perry name.last=Platypus",
            wantJSON: @"{""name"":{""first"":""Perry"",""last"":""Platypus""}}"
        ),
        new(
            name: "group and key (or both) needs quoting",
            replace: removeKeys(TimeKey, LevelKey),
            attrs: slice<global::go.log.slog_package.Attr>([
                Group(prefixˢ,
                    go.log.slog_package.String(needsQuotingˢ, "v"), go.log.slog_package.String(notNeedsQuotingˢ, "v")),
                Group(prefixNeedsQuotingˢ,
                    go.log.slog_package.String(needsQuotingˢ, "v"), go.log.slog_package.String(notNeedsQuotingˢ, "v"))
            ]),
            wantText: @"msg=message ""prefix. needs quoting ""=v prefix.NotNeedsQuoting=v ""prefix needs quoting. needs quoting ""=v ""prefix needs quoting.NotNeedsQuoting""=v",
            wantJSON: @"{""msg"":""message"",""prefix"":{"" needs quoting "":""v"",""NotNeedsQuoting"":""v""},""prefix needs quoting"":{"" needs quoting "":""v"",""NotNeedsQuoting"":""v""}}"
        )
    ])) {
        ref var test = ref heap(new TestJSONAndTextHandlers_type(), out var Ꮡtest);
        test = vᴛ1;

        ref var r = ref heap<global::go.log.slog_package.Record>(NewRecord(testTime, LevelInfo, messageˢ, callerPC(2)), out var Ꮡr);
        var source = r.Source();
        if (source == nil) {
            t.Fatal(sourceIsNilˢ);
        }
        @string line = strconv.Itoa(source.Line);
        r.AddAttrs(test.attrs.ꓸꓸꓸ);
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        ref var opts = ref heap<global::go.log.slog_package.HandlerOptions>(new HandlerOptions(ReplaceAttr: test.replace, AddSource: test.addSource), out var Ꮡopts);
        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            foreach (var (_, vᴛ2) in slice<TestJSONAndTextHandlers_typeᴛ1>([
                new("text", new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡopts)), testʗ1.wantText),
                new("json", new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡopts)), testʗ1.wantJSON)
            ])) {
                ref var handler = ref heap(new TestJSONAndTextHandlers_typeᴛ1(), out var Ꮡhandler);
                handler = vᴛ2;

                var handlerʗ1 = handler;
                var testʗ2 = testʗ1;
                tΔ1.Run(handler.name, (ж<testing.T> tΔ2) => {
                    var h = handlerʗ1.h;
                    if (testʗ2.with != nil) {
                        h = testʗ2.with(h);
                    }
                    Ꮡbuf.Value.Reset();
                    {
                        var err = h.Handle(default, Ꮡr.Value); if (err != nil) {
                            tΔ2.Fatal(err);
                        }
                    }
                    @string want = strings.ReplaceAll(handlerʗ1.want, lineˢ2, line);
                    @string got = strings.TrimSuffix(Ꮡbuf.String(), "\n");
                    if (got != want) {
                        tΔ2.Errorf("\ngot  %s\nwant %s\n", got, want);
                    }
                });
            }
        });
    }
}

// removeKeys returns a function suitable for HandlerOptions.ReplaceAttr
// that removes all Attrs with the given keys.
internal static Func<slice<@string>, global::go.log.slog_package.Attr, global::go.log.slog_package.Attr> removeKeys(params ꓸꓸꓸstring keysʗp) {
    var keys = keysʗp.slice();

    var keysʗ1 = keys;
    return (slice<@string> _, global::go.log.slog_package.Attr a) => {
        foreach (var (_, k) in keysʗ1) {
            if (a.Key == k) {
                return new Attr();
            }
        }
        return a;
    };
}

internal static global::go.log.slog_package.Attr upperCaseKey(slice<@string> _, global::go.log.slog_package.Attr a) {
    a.Key = strings.ToUpper(a.Key);
    return a;
}

internal partial struct logValueName {
    internal @string first, last;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string firstˢ => ref firstˢᵇ;
private static readonly @string firstˢᵇ = "first"u8;
internal static ref readonly @string lastˢ => ref lastˢᵇ;
private static readonly @string lastˢᵇ = "last"u8;

internal static global::go.log.slog_package.Value LogValue(this logValueName n) {
    return GroupValue(
        go.log.slog_package.String(firstˢ, n.first),
        go.log.slog_package.String(lastˢ, n.last));
}

internal partial struct TestHandlerEnabled_type /*dyn*/ {
    internal global::go.log.slog_package.Leveler leveler;
    internal bool want;
}

public static void TestHandlerEnabled(ж<testing.T> t) {
    ж<global::go.log.slog_package.LevelVar> levelVar(global::go.log.slog_package.ΔLevel l) {
        ref var al = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡal);
        Ꮡal.Set(l);
        return Ꮡal;
    }

    foreach (var (_, test) in slice<TestHandlerEnabled_type>([
        new(default, true),
        new(LevelWarn, false),
        new(new global::go.log.slog_package.LevelVarжLeveler(Ꮡ(new LevelVar())), true), // defaults to Info
        new(new global::go.log.slog_package.LevelVarжLeveler(levelVar(LevelWarn)), false),
        new(LevelDebug, true),
        new(new global::go.log.slog_package.LevelVarжLeveler(levelVar(LevelDebug)), true)
    ])) {
        var h = Ꮡ(new commonHandler(opts: new HandlerOptions(Level: test.leveler)));
        var got = h.enabled(LevelInfo);
        if (got != test.want) {
            t.Errorf("%v: got %t, want %t", test.leveler, got, test.want);
        }
    }
}

internal partial struct TestJSONAndTextHandlersWithUnavailableSource_type /*dyn*/ {
    internal @string name;
    internal global::go.log.slog_package.ΔHandler h;
    internal @string want;
}

public static void TestJSONAndTextHandlersWithUnavailableSource(ж<testing.T> t) {
    // Verify that a nil source does not cause a panic.
    // and that the source is empty.
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var opts = Ꮡ(new HandlerOptions(
        ReplaceAttr: removeKeys(LevelKey),
        AddSource: true
    ));

    foreach (var (_, vᴛ1) in slice<TestJSONAndTextHandlersWithUnavailableSource_type>([
        new("text", new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), opts)), "msg=message"),
        new("json", new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), opts)), @"{""msg"":""message""}")
    ])) {
        ref var test = ref heap(new TestJSONAndTextHandlersWithUnavailableSource_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            Ꮡbuf.Value.Reset();
            var r = NewRecord(new time_package.Time(), LevelInfo, messageˢ, 0);
            var err = testʗ1.h.Handle(tΔ1.Context(), r);
            if (err != nil) {
                tΔ1.Fatal(err);
            }

            @string want = strings.TrimSpace(testʗ1.want);
            @string got = strings.TrimSpace(Ꮡbuf.String());
            if (got != want) {
                tΔ1.Errorf("\ngot  %s\nwant %s", got, want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string appˢ => ref appˢᵇ;
private static readonly @string appˢᵇ = "app"u8;
internal static ref readonly @string playgroundˢ => ref playgroundˢᵇ;
private static readonly @string playgroundˢᵇ = "playground"u8;
internal static ref readonly @string roleˢ => ref roleˢᵇ;
private static readonly @string roleˢᵇ = "role"u8;
internal static ref readonly @string testerˢ => ref testerˢᵇ;
private static readonly @string testerˢᵇ = "tester"u8;
internal static ref readonly @string dataVersionˢ => ref dataVersionˢᵇ;
private static readonly @string dataVersionˢᵇ = "data_version"u8;
internal static readonly object typeˢ = (@string)"type"u8;
internal static readonly object logˢ = (@string)"log"u8;
internal static readonly object metricˢ = (@string)"metric"u8;
internal static ref readonly @string levelInfoMsgFooAppˢ => ref levelInfoMsgFooAppˢᵇ;
private static readonly @string levelInfoMsgFooAppˢᵇ = @"level=INFO msg=foo app=playground role=tester data_version=2 type=log"u8;

public static void TestSecondWith(ж<testing.T> t) {
    // Verify that a second call to Logger.With does not corrupt
    // the original.
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var h = NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡ(new HandlerOptions(ReplaceAttr: removeKeys(TimeKey))));
    var logger = New(new global::go.log.slog_package.TextHandlerжΔHandler(h)).With(
        go.log.slog_package.String(appˢ, playgroundˢ),
        go.log.slog_package.String(roleˢ, testerˢ),
        Int(dataVersionˢ, 2));
    var appLogger = logger.With(typeˢ, logˢ); // this becomes type=met
    _ = logger.With(typeˢ, metricˢ);
    appLogger.Info(fooˢ);
    @string got = strings.TrimSpace(Ꮡbuf.String());
    @string want = levelInfoMsgFooAppˢ;
    if (got != want) {
        t.Errorf("\ngot  %s\nwant %s", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nowˢ => ref nowˢᵇ;
private static readonly @string nowˢᵇ = "<now>"u8;

// Verify that ReplaceAttr is called with the correct groups.
internal partial struct TestReplaceAttrGroups_ga /*dyn*/ {
    internal @string groups;
    internal @string key;
    internal @string val;
}

public static void TestReplaceAttrGroups(ж<testing.T> t) {
    ref var got = ref heap<slice<TestReplaceAttrGroups_ga>>(out var Ꮡgot);

    var h = NewTextHandler(io.Discard, Ꮡ(new HandlerOptions(ReplaceAttr: (slice<@string> gs, global::go.log.slog_package.Attr a) => {
        @string v = a.Value.String();
        if (a.Key == TimeKey) {
            v = nowˢ;
        }
        Ꮡgot.ValueSlot = builtin.append(Ꮡgot.ValueSlot, new TestReplaceAttrGroups_ga(strings.Join(gs, ","), a.Key, v));
        return a;
    }
    )));
    New(new global::go.log.slog_package.TextHandlerжΔHandler(h)).With(Int("a", 1)).WithGroup("g1").With(Int("b", 2)).WithGroup("g2").With(
        Int("c", 3),
        Group("g3", Int("d", 4)),
        Int("e", 5)).Info("m",
        Int("f", 6),
        Group("g4", Int("h", 7)),
        Int("i", 8));

    var want = slice<TestReplaceAttrGroups_ga>([
        new("", "a", "1"),
        new("g1", "b", "2"),
        new("g1,g2", "c", "3"),
        new("g1,g2,g3", "d", "4"),
        new("g1,g2", "e", "5"),
        new("", "time", "<now>"),
        new("", "level", "INFO"),
        new("", "msg", "m"),
        new("g1,g2", "f", "6"),
        new("g1,g2,g4", "h", "7"),
        new("g1,g2", "i", "8")
    ]);
    if (!slices.Equal<slice<TestReplaceAttrGroups_ga>, TestReplaceAttrGroups_ga>(got, want)) {
        t.Errorf("\ngot  %v\nwant %v", got, want);
    }
}

internal static ref readonly @string rfc3339Millis => ref rfc3339Millisᵇ;
private static readonly @string rfc3339Millisᵇ = "2006-01-02T15:04:05.000Z07:00"u8;

public static void TestWriteTimeRFC3339(ж<testing.T> t) {
    foreach (var (_, tm) in slice<time.Time>([
        time_package.Date(2000, 1, 2, 3, 4, 5, 0, time_package.ΔUTC),
        time_package.Date(2000, 1, 2, 3, 4, 5, 400, time_package.ΔLocal),
        time_package.Date(2000, 11, 12, 3, 4, 500, 50000000, time_package.ΔUTC)
    ])) {
        @string got = (@string)appendRFC3339Millis(default, tm);
        @string want = tm.Format(rfc3339Millis);
        if (got != want) {
            t.Errorf("got %s, want %s", got, want);
        }
    }
}

public static void BenchmarkWriteTime(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var tm = time_package.Date(2022, 3, 4, 5, 6, 7, 823456789, time_package.ΔLocal);
    b.ResetTimer();
    slice<byte> buf = default;
    for (nint i = 0; i < b.N; i++) {
        buf = appendRFC3339Millis(buf[..0], tm);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string msgˢ => ref msgˢᵇ;
private static readonly @string msgˢᵇ = "msg"u8;
internal static ref readonly @string durˢ => ref durˢᵇ;
private static readonly @string durˢᵇ = "dur"u8;
internal static ref readonly @string badˢ => ref badˢᵇ;
private static readonly @string badˢᵇ = "bad"u8;
internal static ref readonly @string aBCˢ => ref aBCˢᵇ;
private static readonly @string aBCˢᵇ = "a b c"u8;
internal static ref readonly @string infoˢ2 => ref infoˢ2ᵇ;
private static readonly @string infoˢ2ᵇ = "info"u8;

public static void TestDiscardHandler(ж<testing.T> t) {
    var ctx = context.Background();
    var (stdout, stderr) = (os.Stdout, os.Stderr);
    (os.Stdout, os.Stderr) = (default, default); // panic on write
    var stderrʗ1 = stderr;
    var stdoutʗ1 = stdout;
    t.Cleanup(() => {
        (os.Stdout, os.Stderr) = (stdoutʗ1, stderrʗ1);
    });

    // Just ensure nothing panics during normal usage
    var l = New(DiscardHandler);
    l.Info(msgˢ, (@string)"a", (nint)1, (@string)"b", (nint)2);
    l.Debug("bg", Int("a", 1), (@string)"b", (nint)2);
    l.Warn("w", go.log.slog_package.Duration(durˢ, (time.Duration)(3000000000L)));
    l.Error(badˢ, (@string)"a", (nint)1);
    l.Log(ctx, LevelWarn + 1, "w", Int("a", 1), go.log.slog_package.String("b", twoˢ));
    l.LogAttrs(ctx, LevelInfo + 1, aBCˢ, Int("a", 1), go.log.slog_package.String("b", twoˢ));
    l.Info(infoˢ2, (@string)"a", slice<global::go.log.slog_package.Attr>([Int("i", 1)]));
    l.Info(infoˢ2, (@string)"a", GroupValue(Int("i", 1)));
}

public static void BenchmarkAppendKey(ж<testing.B> b) {
    foreach (var (_, size) in slice<nint>([5, 10, 30, 50, 100])) {
        foreach (var (_, quoting) in slice<@string>(["no_quoting", "pre_quoting", "key_quoting", "both_quoting"])) {
            b.Run(fmt.Sprintf("%s_prefix_size_%d"u8, quoting, size), (ж<testing.B> bΔ1) => {
                global::go.log.slog_package.handleState hs = NewJSONHandler(io.Discard, nil).newHandleState(buffer.New(), false, ""u8);
                ref var prefix = ref heap<slice<byte>>(out var Ꮡprefix);

                prefix = bytes.Repeat(slice<byte>("x"u8), size);
                @string key = keyˢ;

                if (quoting == "pre_quoting"u8 || quoting == "both_quoting"u8) {
                    prefix[0] = (byte)'"';
                }
                if (quoting == "key_quoting"u8 || quoting == "both_quoting"u8) {
                    key = "ke\"";
                }

                hs.prefix = Ꮡprefix.Reinterpret<slice<byte>, buffer.Buffer>();

                while (bΔ1.Loop()) {
                    hs.appendKey(key);
                    hs.buf.Reset();
                }
            });
        }
    }
}

} // end slog_internal_test_package
