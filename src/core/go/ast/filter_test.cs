// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// To avoid a cyclic dependency with go/parser, this file is in a separate package.
namespace go.go;

using ast = global::go.go.ast_package;
using format = global::go.go.format_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using io = io_package;
using static global::go.go.ast_internal_test_package;

partial class ast_test_package {

internal static readonly @string input = """
package p

type t1 struct{}
type t2 struct{}

func f1() {}
func f1() {}
func f2() {}

func (*t1) f1() {}
func (t1) f1() {}
func (t1) f2() {}

func (t2) f1() {}
func (t2) f2() {}
func (x *t2) f2() {}

"""u8.Raw();

// Calling ast.MergePackageFiles with ast.FilterFuncDuplicates
// keeps a duplicate entry with attached documentation in favor
// of one without, and it favors duplicate entries appearing
// later in the source over ones appearing earlier. This is why
// (*t2).f2 is kept and t2.f2 is eliminated in this test case.
internal static readonly @string golden = """
package p

type t1 struct{}
type t2 struct{}

func f1() {}
func f2() {}

func (t1) f1() {}
func (t1) f2() {}

func (t2) f1() {}

func (x *t2) f2() {}

"""u8.Raw();

public static void TestFilterDuplicates(ж<testing.T> Ꮡt) {
    // parse input
    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, ""u8, input, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    // create package
    var files = new map<@string, ж<ast.File>>{[""u8] = @file};
    (var pkg, err) = ast.NewPackage(fset, files, default!, nil);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    // filter
    var merged = ast.MergePackageFiles(pkg, ast.FilterFuncDuplicates);

    // pretty-print
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    {
        var errΔ1 = format.Node(new ast_test_package.strings_BuilderжWriter(Ꮡbuf), fset, merged.OrTypedNil()); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    @string output = buf.String();

    if (output != golden) {
        Ꮡt.Errorf("incorrect output:\n%s"u8, output);
    }
}

} // end ast_test_package
