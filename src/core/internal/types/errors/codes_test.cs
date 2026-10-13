// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.types;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using importer = global::go.go.importer_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using testenv = global::go.@internal.testenv_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using static global::go.go.types_package;
using global::go.@internal;
using global::go.go;
using types = global::go.go.types_package;

partial class errors_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string exampleˢ = "Example:"u8;

public static void TestErrorCodeExamples(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt)); // go command needed to resolve std .a files for importer.Default().

    walkCodes(Ꮡt, (@string name, nint value, ж<ast.ValueSpec> spec) => {
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            @string doc = (~spec).Doc.Text();
            var examples = strings.Split(doc, exampleˢ);
            for (nint i = 1; i < len(examples); i++) {
                @string example = strings.TrimSpace(examples[i]);
                var err = checkExample(tΔ1, example);
                if (err == default!) {
                    tΔ1.Fatalf("no error in example #%d"u8, i);
                }
                var (typerr, ok) = err._<typesꓸError>(ᐧ);
                if (!ok) {
                    tΔ1.Fatalf("not a types.Error: %v"u8, err);
                }
                {
                    nint got = readCode(typerr); if (got != value) {
                        tΔ1.Errorf("%s: example #%d returned code %d (%s), want %d"u8, name, i, got, err, value);
                    }
                }
            }
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string codesGoˢ = "codes.go"u8;
private static readonly @string typesˢ = "types"u8;

internal static void walkCodes(ж<testing.T> Ꮡt, Action<@string, nint, ж<ast.ValueSpec>> f) {
    Ꮡt.Helper();
    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, codesGoˢ, default!, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Importer: importer.Default());
    var info = Ꮡ(new typesꓸInfo(
        Types: new map<ast.Expr, types.TypeAndValue>(),
        Defs: new map<ж<ast.Ident>, types.Object>(),
        Uses: new map<ж<ast.Ident>, types.Object>()
    ));
    (_, err) = Ꮡconf.Check(typesˢ, fset, new ж<ast.File>[]{@file}.slice(), info);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, decl) in (~@file).Decls) {
        var (declΔ1, ok) = decl._<ж<ast.GenDecl>>(ᐧ);
        if (!ok || (~declΔ1).Tok != token.CONST) {
            continue;
        }
        foreach (var (_, spec) in (~declΔ1).Specs) {
            var (specΔ1, okΔ1) = spec._<ж<ast.ValueSpec>>(ᐧ);
            if (!okΔ1 || len((~specΔ1).Names) == 0) {
                continue;
            }
            var obj = info.ObjectOf((~specΔ1).Names[0]);
            {
                var (named, okΔ2) = obj.Type()._<ж<types.Named>>(ᐧ); if (okΔ2 && named.Obj().Name() == "Code"u8) {
                    if (len((~specΔ1).Names) != 1) {
                        Ꮡt.Fatalf("bad Code declaration for %q: got %d names, want exactly 1"u8, (~(~specΔ1).Names[0]).Name, len((~specΔ1).Names));
                    }
                    @string codename = (~specΔ1).Names[0].Value.Name;
                    nint value = (nint)(constant.Val(obj._<ж<types.Const>>().Val())._<int64>());
                    f(codename, value, specΔ1);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string go116codeˢ = "go116code"u8;

internal static nint readCode(typesꓸError err) {
    var v = reflect.ValueOf(err);
    return (nint)v.FieldByName(go116codeˢ).Int();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string packageˢ = "package"u8;
private static readonly @string exampleGoˢ = "example.go"u8;
private static readonly @string exampleˢ2 = "example"u8;

internal static error checkExample(ж<testing.T> Ꮡt, @string example) {
    Ꮡt.Helper();
    var fset = token.NewFileSet();
    if (!strings.HasPrefix(example, packageˢ)) {
        example = "package p\n\n"u8 + example;
    }
    var (@file, err) = parser.ParseFile(fset, exampleGoˢ, example, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(
        FakeImportC: true,
        Importer: importer.Default()
    );
    (_, err) = Ꮡconf.Check(exampleˢ2, fset, new ж<ast.File>[]{@file}.slice(), nil);
    return err;
}

public static void TestErrorCodeStyle(ж<testing.T> Ꮡt) {
    // The set of error codes is large and intended to be self-documenting, so
    // this test enforces some style conventions.
    var forbiddenInIdent = new @string[]{
        // use invalid instead
        "illegal"u8,
        // words with a common short-form
        "argument"u8,
        "assertion"u8,
        "assignment"u8,
        "boolean"u8,
        "channel"u8,
        "condition"u8,
        "declaration"u8,
        "expression"u8,
        "function"u8,
        "initial"u8, // use init for initializer, initialization, etc.
        "integer"u8,
        "interface"u8,
        "iterat"u8, // use iter for iterator, iteration, etc.
        "literal"u8,
        "operation"u8,
        "package"u8,
        "pointer"u8,
        "receiver"u8,
        "signature"u8,
        "statement"u8,
        "variable"u8
    }.slice();
    var forbiddenInComment = new @string[]{
        // lhs and rhs should be spelled-out.
        "lhs"u8, "rhs"u8,
        // builtin should be hyphenated.
        "builtin"u8,
        // Use dot-dot-dot.
        "ellipsis"u8
    }.slice();
    var nameHist = new map<nint, nint>();
    @string longestName = ""u8;
    nint maxValue = 0;

    var forbiddenInCommentʗ1 = forbiddenInComment;
    var forbiddenInIdentʗ1 = forbiddenInIdent;
    var nameHistʗ1 = nameHist;
    walkCodes(Ꮡt, (@string name, nint value, ж<ast.ValueSpec> spec) => {
        if (name == "_"u8) {
            return;
        }
        nameHistʗ1[len(name)]++;
        if (value > maxValue) {
            maxValue = value;
        }
        if (len(name) > len(longestName)) {
            longestName = name;
        }
        if (!token.IsExported(name)) {
            Ꮡt.Errorf("%q is not exported"u8, name);
        }
        @string lower = strings.ToLower(name);
        foreach (var (_, bad) in forbiddenInIdentʗ1) {
            if (strings.Contains(lower, bad)) {
                Ꮡt.Errorf("%q contains forbidden word %q"u8, name, bad);
            }
        }
        @string doc = (~spec).Doc.Text();
        if (doc == ""u8){
            Ꮡt.Errorf("%q is undocumented"u8, name);
        } else 
        if (!strings.HasPrefix(doc, name)) {
            Ꮡt.Errorf("doc for %q does not start with the error code name"u8, name);
        }
        @string lowerComment = strings.ToLower(strings.TrimPrefix(doc, name));
        foreach (var (_, bad) in forbiddenInCommentʗ1) {
            if (strings.Contains(lowerComment, bad)) {
                Ꮡt.Errorf("doc for %q contains forbidden word %q"u8, name, bad);
            }
        }
    });

    if (testing.Verbose()) {
        nint totChars = default!;
        nint totCount = default!;
        foreach (var (chars, count) in nameHist) {
            totChars += chars * count;
            totCount += count;
        }
        var avg = (float64)totChars / (float64)totCount;
        fmt.Println();
        fmt.Printf("%d error codes\n"u8, totCount);
        fmt.Printf("average length: %.2f chars\n"u8, avg);
        fmt.Printf("max length: %d (%s)\n"u8, len(longestName), longestName);
    }
}

} // end errors_test_package
