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

public static void TestParseDirectiveMatchesIsDirective(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in isDirectiveTests) {
        var want = tt.ok;
        if (strings.HasPrefix(tt.@in, externˢ) || strings.HasPrefix(tt.@in, exportˢ)) {
            // ParseDirective does NOT support extern or export, unlike
            // isDirective.
            want = false;
        }

        {
            var (_, ok) = ParseDirective(0, "//"u8 + tt.@in); if (ok != want) {
                Ꮡt.Errorf("ParseDirective(0, %q) = %v, want %v"u8, "// " + tt.@in, ok, want);
            }
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

public static void TestParseDirective(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestParseDirective_type[]{
        new(
            name: "valid"u8,
            @in: "//go:generate stringer -type Op -trimprefix Op"u8,
            pos: 10,
            want: new Directive(
                Tool: "go"u8,
                Name: "generate"u8,
                Args: "stringer -type Op -trimprefix Op"u8,
                Slash: 10,
                ArgsPos: ((tokenꓸPos)(10 + len("//go:generate ")))
            ),
            wantOK: true
        ),
        new(
            name: "no args"u8,
            @in: "//go:build ignore"u8,
            pos: 20,
            want: new Directive(
                Tool: "go"u8,
                Name: "build"u8,
                Args: "ignore"u8,
                Slash: 20,
                ArgsPos: ((tokenꓸPos)(20 + len("//go:build ")))
            ),
            wantOK: true
        ),
        new(
            name: "not a directive"u8,
            @in: "// not a directive"u8,
            pos: 30,
            wantOK: false
        ),
        new(
            name: "not a comment"u8,
            @in: "go:generate"u8,
            pos: 40,
            wantOK: false
        ),
        new(
            name: "empty"u8,
            @in: ""u8,
            pos: 50,
            wantOK: false
        ),
        new(
            name: "just slashes"u8,
            @in: "//"u8,
            pos: 60,
            wantOK: false
        ),
        new(
            name: "no name"u8,
            @in: "//go:"u8,
            pos: 70,
            wantOK: false
        ),
        new(
            name: "no tool"u8,
            @in: "//:generate"u8,
            pos: 80,
            wantOK: false
        ),
        new(
            name: "multiple spaces"u8,
            @in: "//go:build  foo bar"u8,
            pos: 90,
            want: new Directive(
                Tool: "go"u8,
                Name: "build"u8,
                Args: "foo bar"u8,
                Slash: 90,
                ArgsPos: ((tokenꓸPos)(90 + len("//go:build  ")))
            ),
            wantOK: true
        ),
        new(
            name: "trailing space"u8,
            @in: "//go:build foo "u8,
            pos: 100,
            want: new Directive(
                Tool: "go"u8,
                Name: "build"u8,
                Args: "foo"u8,
                Slash: 100,
                ArgsPos: ((tokenꓸPos)(100 + len("//go:build ")))
            ),
            wantOK: true
        )
    }.slice()) {
        ref var test = ref heap(new TestParseDirective_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var (got, gotOK) = ParseDirective(testʗ1.pos, testʗ1.@in);
            if (gotOK != testʗ1.wantOK) {
                tΔ1.Fatalf("ParseDirective(%q) ok = %v, want %v"u8, testʗ1.@in, gotOK, testʗ1.wantOK);
            }
            if (!reflect.DeepEqual(got, testʗ1.want)) {
                tΔ1.Errorf("ParseDirective(%q) = %+v, want %+v"u8, testʗ1.@in, got, testʗ1.want);
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

public static void TestParseArgs(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestParseArgs_type[]{
        new(
            name: "simple"u8,
            @in: new Directive(
                Tool: "go"u8,
                Name: "generate"u8,
                Args: "stringer -type Op"u8,
                ArgsPos: 10
            ),
            want: new global::go.go.ast_package.DirectiveArg[]{
                new("stringer"u8, 10),
                new("-type"u8, (tokenꓸPos)(10 + len("stringer "))),
                new("Op"u8, (tokenꓸPos)(10 + len("stringer -type ")))
            }.slice()
        ),
        new(
            name: "quoted"u8,
            @in: new Directive(
                Tool: "go"u8,
                Name: "generate"u8,
                Args: "\"foo bar\" baz"u8,
                ArgsPos: 10
            ),
            want: new global::go.go.ast_package.DirectiveArg[]{
                new("foo bar"u8, 10),
                new("baz"u8, (tokenꓸPos)(10 + len("\"foo bar\" ")))
            }.slice()
        ),
        new(
            name: "raw quoted"u8,
            @in: new Directive(
                Tool: "go"u8,
                Name: "generate"u8,
                Args: "`foo bar` baz"u8,
                ArgsPos: 10
            ),
            want: new global::go.go.ast_package.DirectiveArg[]{
                new("foo bar"u8, 10),
                new("baz"u8, (tokenꓸPos)(10 + len("`foo bar` ")))
            }.slice()
        ),
        new(
            name: "escapes"u8,
            @in: new Directive(
                Tool: "go"u8,
                Name: "generate"u8,
                Args: "\"foo\\U0001F60Abar\" `a\\tb`"u8,
                ArgsPos: 10
            ),
            want: new global::go.go.ast_package.DirectiveArg[]{
                new("foo😊bar"u8, 10),
                new("a\\tb"u8, (tokenꓸPos)(10 + len("\"foo\\U0001F60Abar\" ")))
            }.slice()
        ),
        new(
            name: "empty args"u8,
            @in: new Directive(
                Tool: "go"u8,
                Name: "build"u8,
                Args: ""u8,
                ArgsPos: 10
            ),
            want: new global::go.go.ast_package.DirectiveArg[]{}.slice()
        ),
        new(
            name: "spaces"u8,
            @in: new Directive(
                Tool: "go"u8,
                Name: "build"u8,
                Args: "  foo   bar  "u8,
                ArgsPos: 10
            ),
            want: new global::go.go.ast_package.DirectiveArg[]{
                new("foo"u8, (tokenꓸPos)(10 + len("  "))),
                new("bar"u8, (tokenꓸPos)(10 + len("  foo   ")))
            }.slice()
        ),
        new(
            name: "unterminated quote"u8,
            @in: new Directive(
                Tool: "go"u8,
                Name: "generate"u8,
                Args: "`foo"u8
            ),
            wantErr: true
        ),
        new(
            name: "no space after quote"u8,
            @in: new Directive(
                Tool: "go"u8,
                Name: "generate"u8,
                Args: @"""foo""bar"u8
            ),
            wantErr: true
        )
    }.slice()) {
        ref var test = ref heap(new TestParseArgs_type(), out var Ꮡtest);
        test = vᴛ1;

        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var (got, err) = Ꮡtest.Value.@in.ParseArgs();
            if (err != default! && !Ꮡtest.Value.wantErr){
                tΔ1.Errorf("got ParseArgs(%+v) = error %s; want %+v"u8, Ꮡtest.Value.@in, err, Ꮡtest.Value.want);
            } else 
            if (err == default! && Ꮡtest.Value.wantErr){
                tΔ1.Errorf("got ParseArgs(%+v) = %+v; want error"u8, Ꮡtest.Value.@in, got);
            } else 
            if (err == default! && !reflect.DeepEqual(got, Ꮡtest.Value.want)) {
                tΔ1.Errorf("got ParseArgs(%+v) = %+v; want %+v"u8, Ꮡtest.Value.@in, got, Ꮡtest.Value.want);
            }
        });
    }
}

} // end ast_internal_test_package
