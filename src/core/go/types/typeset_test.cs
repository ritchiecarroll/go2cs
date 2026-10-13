// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using testing = testing_package;
using global::go.go;
using static global::go.go.types_package;

partial class types_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object invalidTypeSetIsNotEmptyˢ = (@string)"invalidTypeSet is not empty"u8;

public static void TestInvalidTypeSet(ж<testing.T> Ꮡt) {
    if (!invalidTypeSet.IsEmpty()) {
        Ꮡt.Error(invalidTypeSetIsNotEmptyˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pGoˢ = "p.go"u8;

public static void TestTypeSetString(ж<testing.T> Ꮡt) {
    foreach (var (body, want) in new map<@string, @string>{
        ["{}"u8] = "𝓤"u8,
        ["{int}"u8] = "{int}"u8,
        ["{~int}"u8] = "{~int}"u8,
        ["{int|string}"u8] = "{int | string}"u8,
        ["{int; string}"u8] = "∅"u8,
        ["{comparable}"u8] = "{comparable}"u8,
        ["{comparable; int}"u8] = "{int}"u8,
        ["{~int; comparable}"u8] = "{~int}"u8,
        ["{int|string; comparable}"u8] = "{int | string}"u8,
        ["{comparable; int; string}"u8] = "∅"u8,
        ["{m()}"u8] = "{func (p.T).m()}"u8,
        ["{m1(); m2() int }"u8] = "{func (p.T).m1(); func (p.T).m2() int}"u8,
        ["{error}"u8] = "{func (error).Error() string}"u8,
        ["{m(); comparable}"u8] = "{comparable; func (p.T).m()}"u8,
        ["{m1(); comparable; m2() int }"u8] = "{comparable; func (p.T).m1(); func (p.T).m2() int}"u8,
        ["{comparable; error}"u8] = "{comparable; func (error).Error() string}"u8,
        ["{m(); comparable; int|float32|string}"u8] = "{func (p.T).m(); int | float32 | string}"u8,
        ["{m1(); int; m2(); comparable }"u8] = "{func (p.T).m1(); func (p.T).m2(); int}"u8,
        ["{E}; type E interface{}"u8] = "𝓤"u8,
        ["{E}; type E interface{int;string}"u8] = "∅"u8,
        ["{E}; type E interface{comparable}"u8] = "{comparable}"u8
    }) {
        // parse
        @string src = "package p; type T interface"u8 + body;
        var fset = token.NewFileSet();
        var (@file, err) = parser.ParseFile(fset, pGoˢ, src, parser.AllErrors);
        if (@file == nil) {
            Ꮡt.Fatalf("%s: %v (invalid test case)"u8, body, err);
        }

        // type check
        ref var conf = ref heap(new global::go.go.types_package.Config(), out var Ꮡconf);
        (var pkg, err) = Ꮡconf.Check((~(~@file).Name).Name, fset, new ж<ast.File>[]{@file}.slice(), nil);
        if (err != default!) {
            Ꮡt.Fatalf("%s: %v (invalid test case)"u8, body, err);
        }

        // lookup T
        var obj = (~pkg).scope.Lookup("T"u8);
        if (obj == default!) {
            Ꮡt.Fatalf("%s: T not found (invalid test case)"u8, body);
        }
        var (T, ok) = under(obj.Type())._<ж<global::go.go.types_package.Interface>>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("%s: %v is not an interface (invalid test case)"u8, body, obj);
        }

        // verify test case
        @string got = T.typeSet().String();
        if (got != want) {
            Ꮡt.Errorf("%s: got %s; want %s"u8, body, got, want);
        }
    }
}

// TODO(gri) add more tests

} // end types_internal_test_package
