// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using bytes = bytes_package;
using json = encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using Δslog = go.log.slog_package;
using strings = strings_package;
using testing = testing_package;
using slogtest = go.testing.slogtest_package;
using encoding;
using go.log;
using go.testing;
using static go.log.slog_internal_test_package;

partial class slog_test_package {

[GoType("dyn")] internal partial struct TestSlogtest_type {
    internal @string name;
    internal Func<io.Writer, slogꓸHandler> @new;
    internal Func<slice<byte>, (map<@string, any>, error)> parse;
}

public static void TestSlogtest(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestSlogtest_type[]{
        new("JSON"u8, (io.Writer w) => new Δslog.JSONHandlerжΔHandler(Δslog.NewJSONHandler(w, nil)), parseJSON),
        new("Text"u8, (io.Writer w) => new Δslog.TextHandlerжΔHandler(Δslog.NewTextHandler(w, nil)), parseText)
    }.slice()) {
        ref var test = ref heap(new TestSlogtest_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var h = testʗ1.@new(new slog_test_package.bytes_BufferжWriter(Ꮡbuf));
            var testʗ2 = testʗ1;
            var results = () => {
                var (ms, err) = parseLines(Ꮡbuf.Value.Bytes(), testʗ2.parse);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                return ms;
            };
            {
                var err = slogtest.TestHandler(h, results); if (err != default!) {
                    tΔ1.Fatal(err);
                }
            }
        });
    }
}

internal static (slice<map<@string, any>>, error) parseLines(slice<byte> src, Func<slice<byte>, (map<@string, any>, error)> parse) {
    slice<map<@string, any>> records = default!;
    foreach (var (_, line) in bytes.Split(src, new byte[]{(rune)'\n'}.slice())) {
        if (len(line) == 0) {
            continue;
        }
        var (m, err) = parse(line);
        if (err != default!) {
            return (default!, fmt.Errorf("%s: %w"u8, ((@string)line), err));
        }
        records = append(records, m);
    }
    return (records, default!);
}

internal static (map<@string, any>, error) parseJSON(slice<byte> bs) {
    ref var m = ref heap<map<@string, any>>(out var Ꮡm);
    {
        var err = json.Unmarshal(bs, Ꮡm); if (err != default!) {
            return (default!, err);
        }
    }
    return (m, default!);
}

// parseText parses the output of a single call to TextHandler.Handle.
// It can parse the output of the tests in this package,
// but it doesn't handle quoted keys or values.
// It doesn't need to handle all cases, because slogtest deliberately
// uses simple inputs so handler writers can focus on testing
// handler behavior, not parsing.
internal static (map<@string, any>, error) parseText(slice<byte> bs) {
    var top = new map<@string, any>{};
    @string s = ((@string)bytes.TrimSpace(bs));
    while (len(s) > 0) {
        var (kv, rest, _) = strings.Cut(s, " "u8); // assumes exactly one space between attrs
        var (k, value, found) = strings.Cut(kv, "="u8);
        if (!found) {
            return (default!, fmt.Errorf("no '=' in %q"u8, kv));
        }
        var keys = strings.Split(k, "."u8);
        // Populate a tree of maps for a dotted path such as "a.b.c=x".
        var m = top;
        foreach (var (_, key) in keys.slice(0, len(keys) - 1)) {
            var (x, ok) = m[key, ꟷ];
            map<@string, any> m2 = default!;
            if (!ok){
                m2 = new map<@string, any>{};
                m[key] = m2;
            } else {
                (m2, ok) = x._<map<@string, any>>(ᐧ);
                if (!ok) {
                    return (default!, fmt.Errorf("value for %q in composite key %q is not map[string]any"u8, key, k));
                }
            }
            m = m2;
        }
        m[keys[len(keys) - 1]] = value;
        s = rest;
    }
    return (top, default!);
}

} // end slog_test_package
