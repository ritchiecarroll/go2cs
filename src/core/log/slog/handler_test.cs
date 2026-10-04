// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// TODO: verify that the output of Marshal{Text,JSON} is suitably escaped.
namespace go.log;

using bytes = bytes_package;
using context = context_package;
using json = go.encoding.json_package;
using io = io_package;
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
using path;
using static go.log.slog_package;
using ꓸꓸꓸstring = Span<@string>;

partial class slog_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string preˢ = "pre"u8;
internal static readonly @string twoˢ = "two"u8;
internal static readonly @string messageˢ = "message"u8;

[GoType("dyn")] internal partial struct TestDefaultHandle_type {
    internal @string name;
    internal Func<global::go.log.slog_package.ΔHandler, global::go.log.slog_package.ΔHandler> with;
    internal slice<global::go.log.slog_package.Attr> attrs;
    internal @string want;
}

public static void TestDefaultHandle(ж<testing.T> Ꮡt) {
    var ctx = context.Background();
    var preAttrs = new global::go.log.slog_package.Attr[]{Int(preˢ, 0)}.slice();
    var attrs = new global::go.log.slog_package.Attr[]{Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ)}.slice();
            var preAttrsʗ1 = preAttrs;

            var preAttrsʗ2 = preAttrs;


    foreach (var (_, vᴛ1) in new TestDefaultHandle_type[]{
        new(
            name: "no attrs"u8,
            want: "INFO message"u8
        ),
        new(
            name: "attrs"u8,
            attrs: attrs,
            want: "INFO message a=1 b=two"u8
        ),
        new(
            name: "preformatted"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ1),
            attrs: attrs,
            want: "INFO message pre=0 a=1 b=two"u8
        ),
        new(
            name: "groups"u8,
            attrs: new global::go.log.slog_package.Attr[]{
                Int("a"u8, 1),
                Group("g"u8,
                    Int("b"u8, 2),
                    Group("h"u8, Int("c"u8, 3)),
                    Int("d"u8, 4)),
                Int("e"u8, 5)
            }.slice(),
            want: "INFO message a=1 g.b=2 g.h.c=3 g.d=4 e=5"u8
        ),
        new(
            name: "group"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ2).WithGroup("s"u8),
            attrs: attrs,
            want: "INFO message pre=0 s.a=1 s.b=two"u8
        ),
        new(
            name: "preformatted groups"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(new global::go.log.slog_package.Attr[]{Int("p1"u8, 1)}.slice()).WithGroup("s1"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("p2"u8, 2)}.slice()).WithGroup("s2"u8),
            attrs: attrs,
            want: "INFO message p1=1 s1.p2=2 s1.s2.a=1 s1.s2.b=two"u8
        ),
        new(
            name: "two with-groups"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(new global::go.log.slog_package.Attr[]{Int("p1"u8, 1)}.slice()).WithGroup("s1"u8).WithGroup("s2"u8),
            attrs: attrs,
            want: "INFO message p1=1 s1.s2.a=1 s1.s2.b=two"u8
        )
    }.slice()) {
        ref var test = ref heap(new TestDefaultHandle_type(), out var Ꮡtest);
        test = vᴛ1;

        var ctxʗ1 = ctx;
        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            @string got = default!;

            global::go.log.slog_package.ΔHandler h = new global::go.log.slog_package.defaultHandlerжΔHandler(newDefaultHandler((uintptr _, slice<byte> b) => {
                got = ((@string)b);
                return default!;
            }));
            if (testʗ1.with != default!) {
                h = testʗ1.with(h);
            }
            var r = NewRecord(new time_package.Time(nil), LevelInfo, messageˢ, 0);
            r.AddAttrs(testʗ1.attrs.ꓸꓸꓸ);
            {
                var err = h.Handle(ctxʗ1, r); if (err != default!) {
                    tΔ1.Fatal(err);
                }
            }
            if (got != testʗ1.want) {
                tΔ1.Errorf("\ngot  %s\nwant %s"u8, got, testʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sub1ˢ = "sub1"u8;
internal static readonly @string sub2ˢ = "sub2"u8;
internal static readonly @string helloFromSub1ˢ = "hello from sub1"u8;
internal static readonly @string helloFromSub2ˢ = "hello from sub2"u8;

public static void TestConcurrentWrites(ж<testing.T> Ꮡt) {
    var ctx = context.Background();
    nint count = 1000;
    foreach (var (_, handlerType) in new @string[]{"text"u8, "json"u8}.slice()) {
        var ctxʗ1 = ctx;
        Ꮡt.Run(handlerType, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            global::go.log.slog_package.ΔHandler h = default!;
            var exprᴛ1 = handlerType;
            if (exprᴛ1 == "text"u8) {
                h = new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil));
            }
            else if (exprᴛ1 == "json"u8) {
                h = new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), nil));
            }
            else { /* default: */
                tΔ1.Fatalf("unexpected handlerType %q"u8, handlerType);
            }

            var sub1 = h.WithAttrs(new global::go.log.slog_package.Attr[]{go.log.slog_package.Bool(sub1ˢ, true)}.slice());
            var sub2 = h.WithAttrs(new global::go.log.slog_package.Attr[]{go.log.slog_package.Bool(sub2ˢ, true)}.slice());
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            for (nint i = 0; i < count; i++) {
                ref var sub1Record = ref heap<global::go.log.slog_package.Record>(out var Ꮡsub1Record);
                Ꮡsub1Record.Value = NewRecord(new time_package.Time(nil), LevelInfo, helloFromSub1ˢ, 0);
                Ꮡsub1Record.Value.AddAttrs(Int("i"u8, i));
                ref var sub2Record = ref heap<global::go.log.slog_package.Record>(out var Ꮡsub2Record);
                Ꮡsub2Record.Value = NewRecord(new time_package.Time(nil), LevelInfo, helloFromSub2ˢ, 0);
                Ꮡsub2Record.Value.AddAttrs(Int("i"u8, i));
                Ꮡwg.Add(1);
                var ctxʗ2 = ctxʗ1;
                var sub1ʗ1 = sub1;
                var sub2ʗ1 = sub2;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        {
                            var err = sub1ʗ1.Handle(ctxʗ2, Ꮡsub1Record.Value); if (err != default!) {
                                tΔ1.Error(err);
                            }
                        }
                        {
                            var err = sub2ʗ1.Handle(ctxʗ2, Ꮡsub2Record.Value); if (err != default!) {
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
                    tΔ1.Fatalf("want %d occurrences of %q, got %d"u8, count, want, n);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string oneˢ = "one"u8;
internal static readonly @string bCX2Eˢ = " b.c=\"\\x2E\t"u8;
internal static readonly @string nameˢ = "name"u8;
internal static readonly object minsˢ = (@string)"mins"u8;
internal static readonly object secsˢ = (@string)"secs"u8;
internal static readonly @string lineˢ2 = "$LINE"u8;

[GoType("dyn")] internal partial struct TestJSONAndTextHandlers_type {
    internal @string name;
    internal Func<slice<@string>, global::go.log.slog_package.Attr, global::go.log.slog_package.Attr> replace;
    internal bool addSource;
    internal Func<global::go.log.slog_package.ΔHandler, global::go.log.slog_package.ΔHandler> with;
    internal slice<global::go.log.slog_package.Attr> preAttrs;
    internal slice<global::go.log.slog_package.Attr> attrs;
    internal @string wantText;
    internal @string wantJSON;
}

[GoType("dyn")] internal partial struct TestJSONAndTextHandlers_typeᴛ1 {
    internal @string name;
    internal global::go.log.slog_package.ΔHandler h;
    internal @string want;
}

// Verify the common parts of TextHandler and JSONHandler.
public static void TestJSONAndTextHandlers(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // remove all Attrs
    var removeAll = (slice<@string> _, global::go.log.slog_package.Attr a) => new Attr(nil);
    var attrs = new global::go.log.slog_package.Attr[]{go.log.slog_package.String("a"u8, oneˢ), Int("b"u8, 2), go.log.slog_package.Any(""u8, default!)}.slice();
    var preAttrs = new global::go.log.slog_package.Attr[]{Int(preˢ, 3), go.log.slog_package.String("x"u8, "y"u8)}.slice();
            var preAttrsʗ1 = preAttrs;

            var preAttrsʗ2 = preAttrs;

            var preAttrsʗ3 = preAttrs;

            var preAttrsʗ4 = preAttrs;

            var preAttrsʗ5 = preAttrs;























    foreach (var (_, vᴛ1) in new TestJSONAndTextHandlers_type[]{
        new(
            name: "basic"u8,
            attrs: attrs,
            wantText: "time=2000-01-02T03:04:05.000Z level=INFO msg=message a=one b=2"u8,
            wantJSON: @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""message"",""a"":""one"",""b"":2}"u8
        ),
        new(
            name: "empty key"u8,
            attrs: builtin.append(slices.Clip<slice<global::go.log.slog_package.Attr>, global::go.log.slog_package.Attr>(attrs), go.log.slog_package.Any(""u8, (@string)"v"u8)),
            wantText: @"time=2000-01-02T03:04:05.000Z level=INFO msg=message a=one b=2 """"=v"u8,
            wantJSON: @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""message"",""a"":""one"",""b"":2,"""":""v""}"u8
        ),
        new(
            name: "cap keys"u8,
            replace: upperCaseKey,
            attrs: attrs,
            wantText: "TIME=2000-01-02T03:04:05.000Z LEVEL=INFO MSG=message A=one B=2"u8,
            wantJSON: @"{""TIME"":""2000-01-02T03:04:05Z"",""LEVEL"":""INFO"",""MSG"":""message"",""A"":""one"",""B"":2}"u8
        ),
        new(
            name: "remove all"u8,
            replace: removeAll,
            attrs: attrs,
            wantText: ""u8,
            wantJSON: @"{}"u8
        ),
        new(
            name: "preformatted"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ1),
            preAttrs: preAttrs,
            attrs: attrs,
            wantText: "time=2000-01-02T03:04:05.000Z level=INFO msg=message pre=3 x=y a=one b=2"u8,
            wantJSON: @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""message"",""pre"":3,""x"":""y"",""a"":""one"",""b"":2}"u8
        ),
        new(
            name: "preformatted cap keys"u8,
            replace: upperCaseKey,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ2),
            preAttrs: preAttrs,
            attrs: attrs,
            wantText: "TIME=2000-01-02T03:04:05.000Z LEVEL=INFO MSG=message PRE=3 X=y A=one B=2"u8,
            wantJSON: @"{""TIME"":""2000-01-02T03:04:05Z"",""LEVEL"":""INFO"",""MSG"":""message"",""PRE"":3,""X"":""y"",""A"":""one"",""B"":2}"u8
        ),
        new(
            name: "preformatted remove all"u8,
            replace: removeAll,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ3),
            preAttrs: preAttrs,
            attrs: attrs,
            wantText: ""u8,
            wantJSON: "{}"u8
        ),
        new(
            name: "remove built-in"u8,
            replace: removeKeys(TimeKey, LevelKey, MessageKey),
            attrs: attrs,
            wantText: "a=one b=2"u8,
            wantJSON: @"{""a"":""one"",""b"":2}"u8
        ),
        new(
            name: "preformatted remove built-in"u8,
            replace: removeKeys(TimeKey, LevelKey, MessageKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ4),
            attrs: attrs,
            wantText: "pre=3 x=y a=one b=2"u8,
            wantJSON: @"{""pre"":3,""x"":""y"",""a"":""one"",""b"":2}"u8
        ),
        new(
            name: "groups"u8,
            replace: removeKeys(TimeKey, LevelKey), // to simplify the result

            attrs: new global::go.log.slog_package.Attr[]{
                Int("a"u8, 1),
                Group("g"u8,
                    Int("b"u8, 2),
                    Group("h"u8, Int("c"u8, 3)),
                    Int("d"u8, 4)),
                Int("e"u8, 5)
            }.slice(),
            wantText: "msg=message a=1 g.b=2 g.h.c=3 g.d=4 e=5"u8,
            wantJSON: @"{""msg"":""message"",""a"":1,""g"":{""b"":2,""h"":{""c"":3},""d"":4},""e"":5}"u8
        ),
        new(
            name: "empty group"u8,
            replace: removeKeys(TimeKey, LevelKey),
            attrs: new global::go.log.slog_package.Attr[]{Group("g"u8), Group("h"u8, Int("a"u8, 1))}.slice(),
            wantText: "msg=message h.a=1"u8,
            wantJSON: @"{""msg"":""message"",""h"":{""a"":1}}"u8
        ),
        new(
            name: "nested empty group"u8,
            replace: removeKeys(TimeKey, LevelKey),
            attrs: new global::go.log.slog_package.Attr[]{
                Group("g"u8,
                    Group("h"u8,
                        Group("i"u8), Group("j"u8)))
            }.slice(),
            wantText: @"msg=message"u8,
            wantJSON: @"{""msg"":""message""}"u8
        ),
        new(
            name: "nested non-empty group"u8,
            replace: removeKeys(TimeKey, LevelKey),
            attrs: new global::go.log.slog_package.Attr[]{
                Group("g"u8,
                    Group("h"u8,
                        Group("i"u8), Group("j"u8, Int("a"u8, 1))))
            }.slice(),
            wantText: @"msg=message g.h.j.a=1"u8,
            wantJSON: @"{""msg"":""message"",""g"":{""h"":{""j"":{""a"":1}}}}"u8
        ),
        new(
            name: "escapes"u8,
            replace: removeKeys(TimeKey, LevelKey),
            attrs: new global::go.log.slog_package.Attr[]{
                go.log.slog_package.String("a b"u8, "x\t\n\u0000y"u8),
                Group(bCX2Eˢ,
                    go.log.slog_package.String("d=e"u8, "f.g\""u8),
                    Int("m.d"u8, 1))
            }.slice(), // dot is not escaped

            wantText: @"msg=message ""a b""=""x\t\n\x00y"" "" b.c=\""\\x2E\t.d=e""=""f.g\"""" "" b.c=\""\\x2E\t.m.d""=1"u8,
            wantJSON: @"{""msg"":""message"",""a b"":""x\t\n\u0000y"","" b.c=\""\\x2E\t"":{""d=e"":""f.g\"""",""m.d"":1}}"u8
        ),
        new(
            name: "LogValuer"u8,
            replace: removeKeys(TimeKey, LevelKey),
            attrs: new global::go.log.slog_package.Attr[]{
                Int("a"u8, 1),
                go.log.slog_package.Any(nameˢ, new logValueName("Ren"u8, "Hoek"u8)),
                Int("b"u8, 2)
            }.slice(),
            wantText: "msg=message a=1 name.first=Ren name.last=Hoek b=2"u8,
            wantJSON: @"{""msg"":""message"",""a"":1,""name"":{""first"":""Ren"",""last"":""Hoek""},""b"":2}"u8
        ),
        new(
            name: "resolve"u8, // Test resolution when there is no ReplaceAttr function.

            attrs: new global::go.log.slog_package.Attr[]{
                go.log.slog_package.Any(""u8, Ꮡ(new replace(new Value(nil)))), // should be elided

                go.log.slog_package.Any(nameˢ, new logValueName("Ren"u8, "Hoek"u8))
            }.slice(),
            wantText: "time=2000-01-02T03:04:05.000Z level=INFO msg=message name.first=Ren name.last=Hoek"u8,
            wantJSON: @"{""time"":""2000-01-02T03:04:05Z"",""level"":""INFO"",""msg"":""message"",""name"":{""first"":""Ren"",""last"":""Hoek""}}"u8
        ),
        new(
            name: "with-group"u8,
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(preAttrsʗ5).WithGroup("s"u8),
            attrs: attrs,
            wantText: "msg=message pre=3 x=y s.a=one s.b=2"u8,
            wantJSON: @"{""msg"":""message"",""pre"":3,""x"":""y"",""s"":{""a"":""one"",""b"":2}}"u8
        ),
        new(
            name: "preformatted with-groups"u8,
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(new global::go.log.slog_package.Attr[]{Int("p1"u8, 1)}.slice()).WithGroup("s1"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("p2"u8, 2)}.slice()).WithGroup("s2"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("p3"u8, 3)}.slice()),
            attrs: attrs,
            wantText: "msg=message p1=1 s1.p2=2 s1.s2.p3=3 s1.s2.a=one s1.s2.b=2"u8,
            wantJSON: @"{""msg"":""message"",""p1"":1,""s1"":{""p2"":2,""s2"":{""p3"":3,""a"":""one"",""b"":2}}}"u8
        ),
        new(
            name: "two with-groups"u8,
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithAttrs(new global::go.log.slog_package.Attr[]{Int("p1"u8, 1)}.slice()).WithGroup("s1"u8).WithGroup("s2"u8),
            attrs: attrs,
            wantText: "msg=message p1=1 s1.s2.a=one s1.s2.b=2"u8,
            wantJSON: @"{""msg"":""message"",""p1"":1,""s1"":{""s2"":{""a"":""one"",""b"":2}}}"u8
        ),
        new(
            name: "empty with-groups"u8,
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("x"u8).WithGroup("y"u8),
            wantText: "msg=message"u8,
            wantJSON: @"{""msg"":""message""}"u8
        ),
        new(
            name: "empty with-groups, no non-empty attrs"u8,
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("x"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Group("g"u8)}.slice()).WithGroup("y"u8),
            wantText: "msg=message"u8,
            wantJSON: @"{""msg"":""message""}"u8
        ),
        new(
            name: "one empty with-group"u8,
            replace: removeKeys(TimeKey, LevelKey),
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("x"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("a"u8, 1)}.slice()).WithGroup("y"u8),
            attrs: new global::go.log.slog_package.Attr[]{Group("g"u8, Group("h"u8))}.slice(),
            wantText: "msg=message x.a=1"u8,
            wantJSON: @"{""msg"":""message"",""x"":{""a"":1}}"u8
        ),
        new(
            name: "GroupValue as Attr value"u8,
            replace: removeKeys(TimeKey, LevelKey),
            attrs: new global::go.log.slog_package.Attr[]{new("v"u8, AnyValue(IntValue(3)))}.slice(),
            wantText: "msg=message v=3"u8,
            wantJSON: @"{""msg"":""message"",""v"":3}"u8
        ),
        new(
            name: "byte slice"u8,
            replace: removeKeys(TimeKey, LevelKey),
            attrs: new global::go.log.slog_package.Attr[]{go.log.slog_package.Any("bs"u8, new byte[]{1, 2, 3, 4}.slice())}.slice(),
            wantText: @"msg=message bs=""\x01\x02\x03\x04"""u8,
            wantJSON: @"{""msg"":""message"",""bs"":""AQIDBA==""}"u8
        ),
        new(
            name: "json.RawMessage"u8,
            replace: removeKeys(TimeKey, LevelKey),
            attrs: new global::go.log.slog_package.Attr[]{go.log.slog_package.Any("bs"u8, ((json.RawMessage)slice<byte>("1234"u8)))}.slice(),
            wantText: @"msg=message bs=""1234"""u8,
            wantJSON: @"{""msg"":""message"",""bs"":1234}"u8
        ),
        new(
            name: "inline group"u8,
            replace: removeKeys(TimeKey, LevelKey),
            attrs: new global::go.log.slog_package.Attr[]{
                Int("a"u8, 1),
                Group(""u8, Int("b"u8, 2), Int("c"u8, 3)),
                Int("d"u8, 4)
            }.slice(),
            wantText: @"msg=message a=1 b=2 c=3 d=4"u8,
            wantJSON: @"{""msg"":""message"",""a"":1,""b"":2,""c"":3,""d"":4}"u8
        ),
        new(
            name: "Source"u8,
            replace: (slice<@string> gs, global::go.log.slog_package.Attr a) => {
                if (a.Key == SourceKey) {
                    var s = a.Value.Any()._<ж<global::go.log.slog_package.Source>>();
                    s.Value.File = filepath.Base((~s).File);
                    return go.log.slog_package.Any(a.Key, s.OrTypedNil());
                }
                return removeKeys(TimeKey, LevelKey)(gs, a);
            },
            addSource: true,
            wantText: @"source=handler_test.go:$LINE msg=message"u8,
            wantJSON: @"{""source"":{""function"":""log/slog.TestJSONAndTextHandlers"",""file"":""handler_test.go"",""line"":$LINE},""msg"":""message""}"u8
        ),
        new(
            name: "replace built-in with group"u8,
            replace: (slice<@string> _, global::go.log.slog_package.Attr a) => {
                if (a.Key == TimeKey) {
                    return Group(TimeKey, minsˢ, (nint)(3), secsˢ, (nint)(2));
                }
                if (a.Key == LevelKey) {
                    return new Attr(nil);
                }
                return a;
            },
            wantText: @"time.mins=3 time.secs=2 msg=message"u8,
            wantJSON: @"{""time"":{""mins"":3,""secs"":2},""msg"":""message""}"u8
        ),
        new(
            name: "replace empty"u8,
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(nil),
            attrs: new global::go.log.slog_package.Attr[]{Group("g"u8, Int("a"u8, 1))}.slice(),
            wantText: ""u8,
            wantJSON: @"{}"u8
        ),
        new(
            name: "replace empty 1"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("a"u8, 1)}.slice()),
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(nil),
            attrs: new global::go.log.slog_package.Attr[]{Group("h"u8, Int("b"u8, 2))}.slice(),
            wantText: ""u8,
            wantJSON: @"{}"u8
        ),
        new(
            name: "replace empty 2"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("a"u8, 1)}.slice()).WithGroup("h"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("b"u8, 2)}.slice()),
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(nil),
            attrs: new global::go.log.slog_package.Attr[]{Group("i"u8, Int("c"u8, 3))}.slice(),
            wantText: ""u8,
            wantJSON: @"{}"u8
        ),
        new(
            name: "replace empty 3"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g"u8),
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(nil),
            attrs: new global::go.log.slog_package.Attr[]{Int("a"u8, 1)}.slice(),
            wantText: ""u8,
            wantJSON: @"{}"u8
        ),
        new(
            name: "replace empty inline"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("a"u8, 1)}.slice()).WithGroup("h"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("b"u8, 2)}.slice()),
            replace: (slice<@string> _Δp0, global::go.log.slog_package.Attr _Δp1) => new Attr(nil),
            attrs: new global::go.log.slog_package.Attr[]{Group(""u8, Int("c"u8, 3))}.slice(),
            wantText: ""u8,
            wantJSON: @"{}"u8
        ),
        new(
            name: "replace partial empty attrs 1"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("a"u8, 1)}.slice()).WithGroup("h"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("b"u8, 2)}.slice()),
            replace: (slice<@string> groups, global::go.log.slog_package.Attr attr) => removeKeys(TimeKey, LevelKey, MessageKey, "a")(groups, attr),
            attrs: new global::go.log.slog_package.Attr[]{Group("i"u8, Int("c"u8, 3))}.slice(),
            wantText: "g.h.b=2 g.h.i.c=3"u8,
            wantJSON: @"{""g"":{""h"":{""b"":2,""i"":{""c"":3}}}}"u8
        ),
        new(
            name: "replace partial empty attrs 2"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("a"u8, 1)}.slice()).WithAttrs(new global::go.log.slog_package.Attr[]{Int("n"u8, 4)}.slice()).WithGroup("h"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("b"u8, 2)}.slice()),
            replace: (slice<@string> groups, global::go.log.slog_package.Attr attr) => removeKeys(TimeKey, LevelKey, MessageKey, "a", "b")(groups, attr),
            attrs: new global::go.log.slog_package.Attr[]{Group("i"u8, Int("c"u8, 3))}.slice(),
            wantText: "g.n=4 g.h.i.c=3"u8,
            wantJSON: @"{""g"":{""n"":4,""h"":{""i"":{""c"":3}}}}"u8
        ),
        new(
            name: "replace partial empty attrs 3"u8,
            with: (global::go.log.slog_package.ΔHandler h) => h.WithGroup("g"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("x"u8, 0)}.slice()).WithAttrs(new global::go.log.slog_package.Attr[]{Int("a"u8, 1)}.slice()).WithAttrs(new global::go.log.slog_package.Attr[]{Int("n"u8, 4)}.slice()).WithGroup("h"u8).WithAttrs(new global::go.log.slog_package.Attr[]{Int("b"u8, 2)}.slice()),
            replace: (slice<@string> groups, global::go.log.slog_package.Attr attr) => removeKeys(TimeKey, LevelKey, MessageKey, "a", "c")(groups, attr),
            attrs: new global::go.log.slog_package.Attr[]{Group("i"u8, Int("c"u8, 3))}.slice(),
            wantText: "g.x=0 g.n=4 g.h.b=2"u8,
            wantJSON: @"{""g"":{""x"":0,""n"":4,""h"":{""b"":2}}}"u8
        ),
        new(
            name: "replace resolved group"u8,
            replace: (slice<@string> groups, global::go.log.slog_package.Attr a) => {
                if (a.Value.Kind() == KindGroup) {
                    return new Attr("bad"u8, IntValue(1));
                }
                return removeKeys(TimeKey, LevelKey, MessageKey)(groups, a);
            },
            attrs: new global::go.log.slog_package.Attr[]{go.log.slog_package.Any(nameˢ, new logValueName("Perry"u8, "Platypus"u8))}.slice(),
            wantText: "name.first=Perry name.last=Platypus"u8,
            wantJSON: @"{""name"":{""first"":""Perry"",""last"":""Platypus""}}"u8
        )
    }.slice()) {
        ref var test = ref heap(new TestJSONAndTextHandlers_type(), out var Ꮡtest);
        test = vᴛ1;

        ref var r = ref heap<global::go.log.slog_package.Record>(out var Ꮡr);
        r = NewRecord(testTime, LevelInfo, messageˢ, callerPC(2));
        @string line = strconv.Itoa((~r.source()).Line);
        r.AddAttrs(test.attrs.ꓸꓸꓸ);
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        ref var opts = ref heap<global::go.log.slog_package.HandlerOptions>(out var Ꮡopts);
        opts = new HandlerOptions(ReplaceAttr: test.replace, AddSource: test.addSource);
        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            foreach (var (_, vᴛ2) in new TestJSONAndTextHandlers_typeᴛ1[]{
                new("text"u8, new global::go.log.slog_package.TextHandlerжΔHandler(NewTextHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡopts)), testʗ1.wantText),
                new("json"u8, new global::go.log.slog_package.JSONHandlerжΔHandler(NewJSONHandler(new slog_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡopts)), testʗ1.wantJSON)
            }.slice()) {
                ref var handler = ref heap(new TestJSONAndTextHandlers_typeᴛ1(), out var Ꮡhandler);
                handler = vᴛ2;

                var handlerʗ1 = handler;
                var testʗ2 = testʗ1;
                tΔ1.Run(handler.name, (ж<testing.T> tΔ2) => {
                    var h = handlerʗ1.h;
                    if (testʗ2.with != default!) {
                        h = testʗ2.with(h);
                    }
                    Ꮡbuf.Value.Reset();
                    {
                        var err = h.Handle(default!, Ꮡr.Value); if (err != default!) {
                            tΔ2.Fatal(err);
                        }
                    }
                    @string want = strings.ReplaceAll(handlerʗ1.want, lineˢ2, line);
                    @string got = strings.TrimSuffix(Ꮡbuf.String(), "\n"u8);
                    if (got != want) {
                        tΔ2.Errorf("\ngot  %s\nwant %s\n"u8, got, want);
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
                return new Attr(nil);
            }
        }
        return a;
    };
}

internal static global::go.log.slog_package.Attr upperCaseKey(slice<@string> _, global::go.log.slog_package.Attr a) {
    a.Key = strings.ToUpper(a.Key);
    return a;
}

[GoType] internal partial struct logValueName {
    internal @string first, last;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string firstˢ = "first"u8;
internal static readonly @string lastˢ = "last"u8;

internal static global::go.log.slog_package.Value LogValue(this logValueName n) {
    return GroupValue(
        go.log.slog_package.String(firstˢ, n.first),
        go.log.slog_package.String(lastˢ, n.last));
}

[GoType("dyn")] internal partial struct TestHandlerEnabled_type {
    internal global::go.log.slog_package.Leveler leveler;
    internal bool want;
}

public static void TestHandlerEnabled(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ж<global::go.log.slog_package.LevelVar> levelVar(global::go.log.slog_package.ΔLevel l) {
        ref var al = ref heap(new global::go.log.slog_package.LevelVar(), out var Ꮡal);
        Ꮡal.Set(l);
        return Ꮡal;
    }
    foreach (var (_, test) in new TestHandlerEnabled_type[]{
        new(default!, true),
        new(LevelWarn, false),
        new(new global::go.log.slog_package.LevelVarжLeveler(Ꮡ(new LevelVar(nil))), true), // defaults to Info

        new(new global::go.log.slog_package.LevelVarжLeveler(levelVar(LevelWarn)), false),
        new(LevelDebug, true),
        new(new global::go.log.slog_package.LevelVarжLeveler(levelVar(LevelDebug)), true)
    }.slice()) {
        var h = Ꮡ(new commonHandler(opts: new HandlerOptions(Level: test.leveler)));
        var got = h.enabled(LevelInfo);
        if (got != test.want) {
            Ꮡt.Errorf("%v: got %t, want %t"u8, test.leveler, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string appˢ = "app"u8;
internal static readonly @string playgroundˢ = "playground"u8;
internal static readonly @string roleˢ = "role"u8;
internal static readonly @string testerˢ = "tester"u8;
internal static readonly @string dataVersionˢ = "data_version"u8;
internal static readonly object typeˢ = (@string)"type"u8;
internal static readonly object logˢ = (@string)"log"u8;
internal static readonly object metricˢ = (@string)"metric"u8;
internal static readonly @string levelInfoMsgFooAppˢ = @"level=INFO msg=foo app=playground role=tester data_version=2 type=log"u8;

public static void TestSecondWith(ж<testing.T> Ꮡt) {
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
        Ꮡt.Errorf("\ngot  %s\nwant %s"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nowˢ = "<now>"u8;

// Verify that ReplaceAttr is called with the correct groups.
[GoType("dyn")] [GoLocalName("ga")] internal partial struct TestReplaceAttrGroups_ga {
    internal @string groups;
    internal @string key;
    internal @string val;
}

public static void TestReplaceAttrGroups(ж<testing.T> Ꮡt) {
    ref var got = ref heap<slice<TestReplaceAttrGroups_ga>>(out var Ꮡgot);
    var h = NewTextHandler(io.Discard, Ꮡ(new HandlerOptions(ReplaceAttr: (slice<@string> gs, global::go.log.slog_package.Attr a) => {
        @string v = a.Value.String();
        if (a.Key == TimeKey) {
            v = nowˢ;
        }
        Ꮡgot.ValueSlot = builtin.append(Ꮡgot.ValueSlot, new TestReplaceAttrGroups_ga(strings.Join(gs, ","u8), a.Key, v));
        return a;
    }
    )));
    New(new global::go.log.slog_package.TextHandlerжΔHandler(h)).With(Int("a"u8, 1)).WithGroup("g1"u8).With(Int("b"u8, 2)).WithGroup("g2"u8).With(
        Int("c"u8, 3),
        Group("g3"u8, Int("d"u8, 4)),
        Int("e"u8, 5)).Info("m"u8,
        Int("f"u8, 6),
        Group("g4"u8, Int("h"u8, 7)),
        Int("i"u8, 8));
    var want = new TestReplaceAttrGroups_ga[]{
        new(""u8, "a"u8, "1"u8),
        new("g1"u8, "b"u8, "2"u8),
        new("g1,g2"u8, "c"u8, "3"u8),
        new("g1,g2,g3"u8, "d"u8, "4"u8),
        new("g1,g2"u8, "e"u8, "5"u8),
        new(""u8, "time"u8, "<now>"u8),
        new(""u8, "level"u8, "INFO"u8),
        new(""u8, "msg"u8, "m"u8),
        new("g1,g2"u8, "f"u8, "6"u8),
        new("g1,g2,g4"u8, "h"u8, "7"u8),
        new("g1,g2"u8, "i"u8, "8"u8)
    }.slice();
    if (!slices.Equal<slice<TestReplaceAttrGroups_ga>, TestReplaceAttrGroups_ga>(got, want)) {
        Ꮡt.Errorf("\ngot  %v\nwant %v"u8, got, want);
    }
}

internal static readonly @string rfc3339Millis = "2006-01-02T15:04:05.000Z07:00"u8;

public static void TestWriteTimeRFC3339(ж<testing.T> Ꮡt) {
    foreach (var (_, tm) in new time.Time[]{
        time_package.Date(2000, 1, 2, 3, 4, 5, 0, time_package.ΔUTC),
        time_package.Date(2000, 1, 2, 3, 4, 5, 400, time_package.ΔLocal),
        time_package.Date(2000, 11, 12, 3, 4, 500, 50000000, time_package.ΔUTC)
    }.slice()) {
        @string got = ((@string)appendRFC3339Millis(default!, tm));
        @string want = tm.Format(rfc3339Millis);
        if (got != want) {
            Ꮡt.Errorf("got %s, want %s"u8, got, want);
        }
    }
}

public static void BenchmarkWriteTime(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var tm = time_package.Date(2022, 3, 4, 5, 6, 7, 823456789, time_package.ΔLocal);
    b.ResetTimer();
    slice<byte> buf = default!;
    for (nint i = 0; i < b.N; i++) {
        buf = appendRFC3339Millis(buf[..0], tm);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string msgˢ = "msg"u8;
internal static readonly @string durˢ = "dur"u8;
internal static readonly @string badˢ = "bad"u8;
internal static readonly @string aBCˢ = "a b c"u8;
internal static readonly @string infoˢ2 = "info"u8;

public static void TestDiscardHandler(ж<testing.T> Ꮡt) {
    var ctx = context.Background();
    var (stdout, stderr) = (os.Stdout, os.Stderr);
    (os.Stdout, os.Stderr) = (default!, default!); // panic on write
    var stderrʗ1 = stderr;
    var stdoutʗ1 = stdout;
    Ꮡt.Cleanup(() => {
        (os.Stdout, os.Stderr) = (stdoutʗ1, stderrʗ1);
    });
    // Just ensure nothing panics during normal usage
    var l = New(DiscardHandler);
    l.Info(msgˢ, (@string)"a"u8, (nint)(1), (@string)"b"u8, (nint)(2));
    l.Debug("bg"u8, Int("a"u8, 1), (@string)"b"u8, (nint)(2));
    l.Warn("w"u8, go.log.slog_package.Duration(durˢ, (time.Duration)(3000000000L)));
    l.Error(badˢ, (@string)"a"u8, (nint)(1));
    l.Log(ctx, LevelWarn + 1, "w"u8, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ));
    l.LogAttrs(ctx, LevelInfo + 1, aBCˢ, Int("a"u8, 1), go.log.slog_package.String("b"u8, twoˢ));
    l.Info(infoˢ2, (@string)"a"u8, new global::go.log.slog_package.Attr[]{Int("i"u8, 1)}.slice());
    l.Info(infoˢ2, (@string)"a"u8, GroupValue(Int("i"u8, 1)));
}

} // end slog_internal_test_package
