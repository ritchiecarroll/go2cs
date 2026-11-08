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
private static ref readonly @string exampleˢ => ref exampleˢᵇ;
private static readonly @string exampleˢᵇ = "Example:"u8;

public static void TestErrorCodeExamples(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t)); // go command needed to resolve std .a files for importer.Default().

    walkCodes(t, (@string name, nint value, ж<ast.ValueSpec> spec) => {
        t.Run(name, (ж<testing.T> tΔ1) => {
            @string doc = spec.Doc.Text();
            var examples = strings.Split(doc, exampleˢ);
            for (nint i = 1; i < len(examples); i++) {
                @string example = strings.TrimSpace(examples[i]);
                var err = checkExample(tΔ1, example);
                if (err == nil) {
                    tΔ1.Fatalf("no error in example #%d", i);
                }
                var (typerr, ok) = err._<typesꓸError>(ᐧ);
                if (!ok) {
                    tΔ1.Fatalf("not a types.Error: %v", err);
                }
                nint got = readCode(typerr); if (got != value) {
                    tΔ1.Errorf("%s: example #%d returned code %d (%s), want %d", name, i, got, err, value);
                }
            }
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string codesGoˢ => ref codesGoˢᵇ;
private static readonly @string codesGoˢᵇ = "codes.go"u8;
private static ref readonly @string typesˢ => ref typesˢᵇ;
private static readonly @string typesˢᵇ = "types"u8;

internal static void walkCodes(ж<testing.T> t, Action<@string, nint, ж<ast.ValueSpec>> f) {
    t.Helper();
    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, codesGoˢ, default, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }
    ref var conf = ref heap<types.Config>(new Config(Importer: importer.Default()), out var Ꮡconf);
    var info = Ꮡ(new typesꓸInfo(
        Types: new map<ast.Expr, types.TypeAndValue>(),
        Defs: new map<ж<ast.Ident>, types.Object>(),
        Uses: new map<ж<ast.Ident>, types.Object>()
    ));
    (_, err) = Ꮡconf.Check(typesˢ, fset, [@file], info);
    if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (_, decl) in range(@file.Decls)) {
        var (declΔ1, ok) = decl._<ж<ast.GenDecl>>(ᐧ);
        if (!ok || declΔ1.Tok != token.CONST) {
            continue;
        }
        foreach (var (_, spec) in range(declΔ1.Specs)) {
            var (specΔ1, okΔ1) = spec._<ж<ast.ValueSpec>>(ᐧ);
            if (!okΔ1 || len(specΔ1.Names) == 0) {
                continue;
            }
            var obj = info.ObjectOf(specΔ1.Names[0]);
            var (named, okΔ2) = obj.Type()._<ж<types.Named>>(ᐧ); if (okΔ2 && named.Obj().Name() == "Code"u8) {
                if (len(specΔ1.Names) != 1) {
                    t.Fatalf("bad Code declaration for %q: got %d names, want exactly 1", specΔ1.Names[0].Name, len(specΔ1.Names));
                }
                @string codename = specΔ1.Names[0].Name;
                nint value = (nint)(constant.Val(obj._<ж<types.Const>>().Val())._<int64>());
                f(codename, value, specΔ1);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string go116codeˢ => ref go116codeˢᵇ;
private static readonly @string go116codeˢᵇ = "go116code"u8;

internal static nint readCode(typesꓸError err) {
    var v = reflect.ValueOf(err);
    return (nint)v.FieldByName(go116codeˢ).Int();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string packageˢ => ref packageˢᵇ;
private static readonly @string packageˢᵇ = "package"u8;
private static ref readonly @string exampleGoˢ => ref exampleGoˢᵇ;
private static readonly @string exampleGoˢᵇ = "example.go"u8;
private static ref readonly @string exampleˢ2 => ref exampleˢ2ᵇ;
private static readonly @string exampleˢ2ᵇ = "example"u8;

internal static error checkExample(ж<testing.T> t, @string example) {
    t.Helper();
    var fset = token.NewFileSet();
    if (!strings.HasPrefix(example, packageˢ)) {
        example = "package p\n\n"u8 + example;
    }
    var (@file, err) = parser.ParseFile(fset, exampleGoˢ, example, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    ref var conf = ref heap<types.Config>(new Config(
        FakeImportC: true,
        Importer: importer.Default()
    ), out var Ꮡconf);
    (_, err) = Ꮡconf.Check(exampleˢ2, fset, [@file], nil);
    return err;
}

public static void TestErrorCodeStyle(ж<testing.T> t) {
    // The set of error codes is large and intended to be self-documenting, so
    // this test enforces some style conventions.
    var forbiddenInIdent = slice<@string>([
        // use invalid instead
        "illegal",
        // words with a common short-form
        "argument",
        "assertion",
        "assignment",
        "boolean",
        "channel",
        "condition",
        "declaration",
        "expression",
        "function",
        "initial", // use init for initializer, initialization, etc.
        "integer",
        "interface",
        "iterat", // use iter for iterator, iteration, etc.
        "literal",
        "operation",
        "package",
        "pointer",
        "receiver",
        "signature",
        "statement",
        "variable"
    ]);
    var forbiddenInComment = slice<@string>([
        // lhs and rhs should be spelled-out.
        "lhs", "rhs",
        // builtin should be hyphenated.
        "builtin",
        // Use dot-dot-dot.
        "ellipsis"
    ]);
    var nameHist = new map<nint, nint>();
    @string longestName = ""u8;
    nint maxValue = 0;

    var forbiddenInCommentʗ1 = forbiddenInComment;
    var forbiddenInIdentʗ1 = forbiddenInIdent;
    var nameHistʗ1 = nameHist;
    walkCodes(t, (@string name, nint value, ж<ast.ValueSpec> spec) => {
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
            t.Errorf("%q is not exported", name);
        }
        @string lower = strings.ToLower(name);
        foreach (var (_, bad) in range(forbiddenInIdentʗ1)) {
            if (strings.Contains(lower, bad)) {
                t.Errorf("%q contains forbidden word %q", name, bad);
            }
        }
        @string doc = spec.Doc.Text();
        if (doc == ""u8) {
            t.Errorf("%q is undocumented", name);
        } else if (!strings.HasPrefix(doc, name)) {
            t.Errorf("doc for %q does not start with the error code name", name);
        }
        @string lowerComment = strings.ToLower(strings.TrimPrefix(doc, name));
        foreach (var (_, bad) in range(forbiddenInCommentʗ1)) {
            if (strings.Contains(lowerComment, bad)) {
                t.Errorf("doc for %q contains forbidden word %q", name, bad);
            }
        }
    });

    if (testing.Verbose()) {
        nint totChars = default;
        nint totCount = default;
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
