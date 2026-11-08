// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using token = global::go.go.token_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using static global::go.go.ast_package;

partial class ast_internal_test_package {

public static void TestParseDirectiveMatchesIsDirective(ж<testing.T> t) {
    foreach (var (_, tt) in range(isDirectiveTests)) {
        var want = tt.ok;
        if (strings.HasPrefix(tt.@in, externˢ) || strings.HasPrefix(tt.@in, exportˢ)) {
            // ParseDirective does NOT support extern or export, unlike
            // isDirective.
            want = false;
        }

        var (_, ok) = ParseDirective(0, "//"u8 + tt.@in); if (ok != want) {
            t.Errorf("ParseDirective(0, %q) = %v, want %v", "// " + tt.@in, ok, want);
        }
    }
}

internal partial struct TestParseDirective_type /*dyn*/ {
    internal @string name;
    internal @string @in;
    internal tokenꓸPos pos;
    internal global::go.go.ast_package.Directive want;
    internal bool wantOK;
}

public static void TestParseDirective(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(slice<TestParseDirective_type>([
        new(
            name: "valid",
            @in: "//go:generate stringer -type Op -trimprefix Op",
            pos: 10,
            want: new Directive(
                Tool: "go",
                Name: "generate",
                Args: "stringer -type Op -trimprefix Op",
                Slash: 10,
                ArgsPos: ((tokenꓸPos)(10 + len("//go:generate ")))
            ),
            wantOK: true
        ),
        new(
            name: "no args",
            @in: "//go:build ignore",
            pos: 20,
            want: new Directive(
                Tool: "go",
                Name: "build",
                Args: "ignore",
                Slash: 20,
                ArgsPos: ((tokenꓸPos)(20 + len("//go:build ")))
            ),
            wantOK: true
        ),
        new(
            name: "not a directive",
            @in: "// not a directive",
            pos: 30,
            wantOK: false
        ),
        new(
            name: "not a comment",
            @in: "go:generate",
            pos: 40,
            wantOK: false
        ),
        new(
            name: "empty",
            @in: "",
            pos: 50,
            wantOK: false
        ),
        new(
            name: "just slashes",
            @in: "//",
            pos: 60,
            wantOK: false
        ),
        new(
            name: "no name",
            @in: "//go:",
            pos: 70,
            wantOK: false
        ),
        new(
            name: "no tool",
            @in: "//:generate",
            pos: 80,
            wantOK: false
        ),
        new(
            name: "multiple spaces",
            @in: "//go:build  foo bar",
            pos: 90,
            want: new Directive(
                Tool: "go",
                Name: "build",
                Args: "foo bar",
                Slash: 90,
                ArgsPos: ((tokenꓸPos)(90 + len("//go:build  ")))
            ),
            wantOK: true
        ),
        new(
            name: "trailing space",
            @in: "//go:build foo ",
            pos: 100,
            want: new Directive(
                Tool: "go",
                Name: "build",
                Args: "foo",
                Slash: 100,
                ArgsPos: ((tokenꓸPos)(100 + len("//go:build ")))
            ),
            wantOK: true
        )
    ]))) {
        ref var test = ref heap(new TestParseDirective_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var (got, gotOK) = ParseDirective(testʗ1.pos, testʗ1.@in);
            if (gotOK != testʗ1.wantOK) {
                tΔ1.Fatalf("ParseDirective(%q) ok = %v, want %v", testʗ1.@in, gotOK, testʗ1.wantOK);
            }
            if (!reflect.DeepEqual(got, testʗ1.want)) {
                tΔ1.Errorf("ParseDirective(%q) = %+v, want %+v", testʗ1.@in, got, testʗ1.want);
            }
        });
    }
}

internal partial struct TestParseArgs_type /*dyn*/ {
    internal @string name;
    internal global::go.go.ast_package.Directive @in;
    internal slice<global::go.go.ast_package.DirectiveArg> want;
    internal bool wantErr;
}

public static void TestParseArgs(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(slice<TestParseArgs_type>([
        new(
            name: "simple",
            @in: new Directive(
                Tool: "go",
                Name: "generate",
                Args: "stringer -type Op",
                ArgsPos: 10
            ),
            want: slice<global::go.go.ast_package.DirectiveArg>([
                new("stringer", 10),
                new("-type", (tokenꓸPos)(10 + len("stringer "))),
                new("Op", (tokenꓸPos)(10 + len("stringer -type ")))
            ])
        ),
        new(
            name: "quoted",
            @in: new Directive(
                Tool: "go",
                Name: "generate",
                Args: "\"foo bar\" baz",
                ArgsPos: 10
            ),
            want: slice<global::go.go.ast_package.DirectiveArg>([
                new("foo bar", 10),
                new("baz", (tokenꓸPos)(10 + len("\"foo bar\" ")))
            ])
        ),
        new(
            name: "raw quoted",
            @in: new Directive(
                Tool: "go",
                Name: "generate",
                Args: "`foo bar` baz",
                ArgsPos: 10
            ),
            want: slice<global::go.go.ast_package.DirectiveArg>([
                new("foo bar", 10),
                new("baz", (tokenꓸPos)(10 + len("`foo bar` ")))
            ])
        ),
        new(
            name: "escapes",
            @in: new Directive(
                Tool: "go",
                Name: "generate",
                Args: "\"foo\\U0001F60Abar\" `a\\tb`",
                ArgsPos: 10
            ),
            want: slice<global::go.go.ast_package.DirectiveArg>([
                new("foo😊bar", 10),
                new("a\\tb", (tokenꓸPos)(10 + len("\"foo\\U0001F60Abar\" ")))
            ])
        ),
        new(
            name: "empty args",
            @in: new Directive(
                Tool: "go",
                Name: "build",
                Args: "",
                ArgsPos: 10
            ),
            want: []
        ),
        new(
            name: "spaces",
            @in: new Directive(
                Tool: "go",
                Name: "build",
                Args: "  foo   bar  ",
                ArgsPos: 10
            ),
            want: slice<global::go.go.ast_package.DirectiveArg>([
                new("foo", (tokenꓸPos)(10 + len("  "))),
                new("bar", (tokenꓸPos)(10 + len("  foo   ")))
            ])
        ),
        new(
            name: "unterminated quote",
            @in: new Directive(
                Tool: "go",
                Name: "generate",
                Args: "`foo"
            ),
            wantErr: true
        ),
        new(
            name: "no space after quote",
            @in: new Directive(
                Tool: "go",
                Name: "generate",
                Args: @"""foo""bar"
            ),
            wantErr: true
        )
    ]))) {
        ref var test = ref heap(new TestParseArgs_type(), out var Ꮡtest);
        test = vᴛ1;

        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var (got, err) = Ꮡtest.Value.@in.ParseArgs();
            if (err != nil && !Ꮡtest.Value.wantErr) {
                tΔ1.Errorf("got ParseArgs(%+v) = error %s; want %+v", Ꮡtest.Value.@in, err, Ꮡtest.Value.want);
            } else if (err == nil && Ꮡtest.Value.wantErr) {
                tΔ1.Errorf("got ParseArgs(%+v) = %+v; want error", Ꮡtest.Value.@in, got);
            } else if (err == nil && !reflect.DeepEqual(got, Ꮡtest.Value.want)) {
                tΔ1.Errorf("got ParseArgs(%+v) = %+v; want %+v", Ꮡtest.Value.@in, got, Ꮡtest.Value.want);
            }
        });
    }
}

} // end ast_internal_test_package
