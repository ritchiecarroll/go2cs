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

public static partial void TestInvalidTypeSet(ж<testing.T> t) {
    if (!invalidTypeSet.IsEmpty()) {
        t.Error(invalidTypeSetIsNotEmptyˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pGoˢ => ref pGoˢᵇ;
private static readonly @string pGoˢᵇ = "p.go"u8;

public static void TestTypeSetString(ж<testing.T> t) {
    foreach (var (body, want) in new map<@string, @string>{
        ["{}"] = "𝓤",
        ["{int}"] = "{int}",
        ["{~int}"] = "{~int}",
        ["{int|string}"] = "{int | string}",
        ["{int; string}"] = "∅",

        ["{comparable}"] = "{comparable}",
        ["{comparable; int}"] = "{int}",
        ["{~int; comparable}"] = "{~int}",
        ["{int|string; comparable}"] = "{int | string}",
        ["{comparable; int; string}"] = "∅",

        ["{m()}"] = "{func (p.T).m()}",
        ["{m1(); m2() int }"] = "{func (p.T).m1(); func (p.T).m2() int}",
        ["{error}"] = "{func (error).Error() string}",
        ["{m(); comparable}"] = "{comparable; func (p.T).m()}",
        ["{m1(); comparable; m2() int }"] = "{comparable; func (p.T).m1(); func (p.T).m2() int}",
        ["{comparable; error}"] = "{comparable; func (error).Error() string}",

        ["{m(); comparable; int|float32|string}"] = "{func (p.T).m(); int | float32 | string}",
        ["{m1(); int; m2(); comparable }"] = "{func (p.T).m1(); func (p.T).m2(); int}",

        ["{E}; type E interface{}"] = "𝓤",
        ["{E}; type E interface{int;string}"] = "∅",
        ["{E}; type E interface{comparable}"] = "{comparable}"
    }) {
        // parse
        @string src = "package p; type T interface"u8 + body;
        var fset = token.NewFileSet();
        var (@file, err) = parser.ParseFile(fset, pGoˢ, src, parser.AllErrors);
        if (@file == nil) {
            t.Fatalf("%s: %v (invalid test case)", body, err);
        }

        // type check
        ref var conf = ref heap(new global::go.go.types_package.Config(), out var Ꮡconf);
        (var pkg, err) = Ꮡconf.Check(@file.Name.Name, fset, [@file], nil);
        if (err != nil) {
            t.Fatalf("%s: %v (invalid test case)", body, err);
        }

        // lookup T
        var obj = pkg.scope.Lookup("T");
        if (obj == nil) {
            t.Fatalf("%s: T not found (invalid test case)", body);
        }
        var (T, ok) = obj.Type().Underlying()._<ж<global::go.go.types_package.Interface>>(ᐧ);
        if (!ok) {
            t.Fatalf("%s: %v is not an interface (invalid test case)", body, obj);
        }

        // verify test case
        @string got = T.typeSet().String();
        if (got != want) {
            t.Errorf("%s: got %s; want %s", body, got, want);
        }
    }
}

// TODO(gri) add more tests

} // end types_internal_test_package
