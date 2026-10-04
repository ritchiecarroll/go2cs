// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using scanner = global::go.go.scanner_package;
using token = global::go.go.token_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using fs = global::go.io.fs_package;
using global::go.go;
using global::go.io;
using path;
using static global::go.go.parser_package;

partial class parser_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "testdata"u8;
internal static readonly @string resolutionˢ = "resolution"u8;

// TestResolution checks that identifiers are resolved to the declarations
// annotated in the source, by comparing the positions of the resulting
// Ident.Obj.Decl to positions marked in the source via special comments.
//
// In the test source, any comment prefixed with '=' or '@' (or both) marks the
// previous token position as the declaration ('=') or a use ('@') of an
// identifier. The text following '=' and '@' in the comment string is the
// label to use for the location.  Declaration labels must be unique within the
// file, and use labels must refer to an existing declaration label. It's OK
// for a comment to denote both the declaration and use of a label (e.g.
// '=@foo'). Leading and trailing whitespace is ignored. Any comment not
// beginning with '=' or '@' is ignored.
public static void TestResolution(ж<testing.T> Ꮡt) {
    @string dir = filepath.Join(testdataˢ, resolutionˢ);
    var (fis, err) = os.ReadDir(dir);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, fi) in fis) {
        var fiʗ1 = fi;
        Ꮡt.Run(fi.Name(), (ж<testing.T> tΔ1) => {
            var fset = token.NewFileSet();
            @string path = filepath.Join(dir, fiʗ1.Name());
            var src = readFile(path); // panics on failure
            global::go.go.parser_package.Mode mode = default!;
            var (@file, errΔ1) = ParseFile(fset, path, src, mode);
            if (errΔ1 != default!) {
                tΔ1.Fatal(errΔ1);
            }
            // Compare the positions of objects resolved during parsing (fromParser)
            // to those annotated in source comments (fromComments).
            var handle = fset.File((~@file).Package);
            var fromParser = declsFromParser(@file);
            var fromComments = declsFromComments(handle, src);
            var handleʗ1 = handle;
            tokenꓸPosition pos(tokenꓸPos posΔ1) {
                var p = handleʗ1.Position(posΔ1);
                // The file name is implied by the subtest, so remove it to avoid
                // clutter in error messages.
                p.Filename = ""u8;
                return p;
            }
            foreach (var (k, want) in fromComments) {
                {
                    tokenꓸPos got = fromParser[k]; if (got != want) {
                        tΔ1.Errorf("%s resolved to %s, want %s"u8, pos(k), pos(got), pos(want));
                    }
                }
                delete(fromParser, k);
            }
            // What remains in fromParser are unexpected resolutions.
            foreach (var (k, got) in fromParser) {
                tΔ1.Errorf("%s resolved to %s, want no object"u8, pos(k), pos(got));
            }
        });
    }
}

// declsFromParser walks the file and collects the map associating an
// identifier position with its declaration position.
internal static map<tokenꓸPos, tokenꓸPos> declsFromParser(ж<ast.File> Ꮡfile) {
    var objmap = new map<tokenꓸPos, tokenꓸPos>{};
    var objmapʗ1 = objmap;
    ast.Inspect(new ast.FileжNode(Ꮡfile), (ast.Node node) => {
        // Ignore blank identifiers to reduce noise.
        {
            var (ident, _) = node._<ж<ast.Ident>>(ᐧ); if (ident != nil && (~ident).Obj != nil && (~ident).Name != "_"u8) {
                objmapʗ1[ident.Pos()] = (~ident).Obj.Pos();
            }
        }
        return true;
    });
    return objmap;
}

// declsFromComments looks at comments annotating uses and declarations, and
// maps each identifier use to its corresponding declaration. See the
// description of these annotations in the documentation for TestResolution.
internal static map<tokenꓸPos, tokenꓸPos> declsFromComments(ж<tokenꓸFile> Ꮡhandle, slice<byte> src) {
    var (decls, uses) = positionMarkers(Ꮡhandle, src);
    var objmap = new map<tokenꓸPos, tokenꓸPos>();
    // Join decls and uses on name, to build the map of use->decl.
    foreach (var (name, posns) in uses) {
        var (declpos, ok) = decls[name, ꟷ];
        if (!ok) {
            throw panic(fmt.Sprintf("missing declaration for %s"u8, name));
        }
        foreach (var (_, pos) in posns) {
            objmap[pos] = declpos;
        }
    }
    return objmap;
}

// positionMarkers extracts named positions from the source denoted by comments
// prefixed with '=' (declarations) and '@' (uses): for example '@foo' or
// '=@bar'. It returns a map of name->position for declarations, and
// name->position(s) for uses.
internal static (map<@string, tokenꓸPos> decls, map<@string, slice<tokenꓸPos>> uses) positionMarkers(ж<tokenꓸFile> Ꮡhandle, slice<byte> src) {
    map<@string, tokenꓸPos> decls = default!;
    map<@string, slice<tokenꓸPos>> uses = default!;

    scanner.Scanner s = default!;
    s.Init(Ꮡhandle, src, default!, scanner.ScanComments);
    decls = new map<@string, tokenꓸPos>();
    uses = new map<@string, slice<tokenꓸPos>>();
    tokenꓸPos prev = default!;                // position of last non-comment, non-semicolon token
scanFile:
    while (ᐧ) {
        var (pos, tok, lit) = s.Scan();
        var exprᴛ1 = tok;
        var matchᴛ1 = false;
        if (exprᴛ1 == token.EOF) { matchᴛ1 = true;
            goto break_scanFile;
        }
        else if (exprᴛ1 == token.COMMENT) { matchᴛ1 = true;
            var (name, decl, use) = annotatedObj(lit);
            if (len(name) > 0) {
                if (decl) {
                    {
                        var (_, ok) = decls[name, ꟷ]; if (ok) {
                            throw panic(fmt.Sprintf("duplicate declaration markers for %s"u8, name));
                        }
                    }
                    decls[name] = prev;
                }
                if (use) {
                    uses[name] = append(uses[name], prev);
                }
            }
        }
        else if (exprᴛ1 == token.SEMICOLON) { matchᴛ1 = true;
            if (lit == "\n"u8) {
                // ignore automatically inserted semicolon
                goto continue_scanFile;
            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            prev = pos;
        }

continue_scanFile:;
    }
break_scanFile:;
    return (decls, uses);
}

internal static (@string name, bool decl, bool use) annotatedObj(@string lit) {
    @string name = default!;
    bool decl = default!;
    bool use = default!;

    if (lit[1] == (rune)'*') {
        lit = lit.slice(0, len(lit) - 2); // strip trailing */
    }
    lit = strings.TrimSpace(lit[2..]);
scanLit:
    foreach (var (idx, r) in lit) {
        switch (r) {
        case (rune)'=': {
            decl = true;
            break;
        }
        case (rune)'@': {
            use = true;
            break;
        }
        default: {
            name = lit.slice(idx);
            goto break_scanLit;
            break;
        }}

continue_scanLit:;
    }
break_scanLit:;
    return (name, decl, use);
}

} // end parser_internal_test_package
