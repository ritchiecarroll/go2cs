// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains the exported entry points for invoking the parser.
namespace go.go;

using bytes = bytes_package;
using errors = errors_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using io = io_package;
using fs = global::go.io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using global::go.go;
using global::go.io;
using path;

partial class parser_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidSourceˢ = "invalid source"u8;

// If src != nil, readSource converts src to a []byte if possible;
// otherwise it returns an error. If src == nil, readSource returns
// the result of reading the file specified by filename.
internal static (slice<byte>, error) readSource(@string filename, any src) {
    if (src != default!) {
        switch (src.type()) {
        case @string s: {
            return (slice<byte>(s), default!);
        }
        case slice<byte> s: {
            return (s, default!);
        }
        case ж<bytes.Buffer> s: {
            if (s != nil) {
                // is io.Reader, but src is already available in []byte form
                return (s.Bytes(), default!);
            }
            break;
        }
        case {} Δs when Δs._<io.Reader>(out var s): {
            return io.ReadAll(s);
        }}
        return (default!, errors.New(invalidSourceˢ));
    }
    return os.ReadFile(filename);
}

partial struct Mode /*num:nuint*/;

public static Mode PackageClauseOnly => /* 1 << iota */ 1;                      // stop parsing after package clause
public static Mode ImportsOnly => 2;                            // stop parsing after import declarations
public static Mode ParseComments => 4;                          // parse comments and add them to AST
public static Mode Trace => 8;                                  // print a trace of parsed productions
public static Mode DeclarationErrors => 16;                      // report declaration errors
public static Mode SpuriousErrors => 32;                         // same as AllErrors, for backward-compatibility
public static Mode SkipObjectResolution => 64;                   // skip deprecated identifier resolution; see ParseFile
public static Mode AllErrors => /* SpuriousErrors */ 32;                  // report all errors (not just the first 10 on different lines)

// ParseFile parses the source code of a single Go source file and returns
// the corresponding [ast.File] node. The source code may be provided via
// the filename of the source file, or via the src parameter.
//
// If src != nil, ParseFile parses the source from src and the filename is
// only used when recording position information. The type of the argument
// for the src parameter must be string, []byte, or [io.Reader].
// If src == nil, ParseFile parses the file specified by filename.
//
// The mode parameter controls the amount of source text parsed and
// other optional parser functionality. If the [SkipObjectResolution]
// mode bit is set (recommended), the object resolution phase of
// parsing will be skipped, causing File.Scope, File.Unresolved, and
// all Ident.Obj fields to be nil. Those fields are deprecated; see
// [ast.Object] for details.
//
// Position information is recorded in the file set fset, which must not be
// nil.
//
// If the source couldn't be read, the returned AST is nil and the error
// indicates the specific failure. If the source was read but syntax
// errors were found, the result is a partial AST (with [ast.Bad]* nodes
// representing the fragments of erroneous source code). Multiple errors
// are returned via a scanner.ErrorList which is sorted by source position.
public static (ж<ast.File> f, error err) ParseFile(ж<token.FileSet> Ꮡfset, @string filename, any src, Mode mode) {
    ж<ast.File> f = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var fset = ref Ꮡfset.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        if (Ꮡfset == nil) {
            throw panic("parser.ParseFile: no token.FileSet provided (fset == nil)");
        }
        // get source
        (var text, err) = readSource(filename, src);
        if (err != default!) {
            (f, err) = (default!, err); goto ᒐdone;
        }
        var @file = Ꮡfset.AddFile(filename, -1, len(text));
        ref var p = ref heap(new parser(), out var Ꮡp);
        var fileʗ1 = @file;
        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    // resume same panic if it's not a bailout
                    ref var bail = ref heap<bailout>(out var Ꮡbail);
                    (bail, var ok) = e._<bailout>(ᐧ);
                    if (!ok){
                        throw panic(e);
                    } else 
                    if (bail.msg != ""u8) {
                        Ꮡp.Value.errors.Add(Ꮡp.Value.@file.Position(bail.pos), bail.msg);
                    }
                }
            }
            // set result values
            if (f == nil) {
                // source is not a valid Go source file - satisfy
                // ParseFile API and return a valid (but) empty
                // *ast.File
                f = Ꮡ(new ast.File(
                    Name: @new<ast.Ident>(),
                    Scope: ast.NewScope(nil)
                ));
            }
            // Ensure the start/end are consistent,
            // whether parsing succeeded or not.
            f.Value.FileStart = ((tokenꓸPos)fileʗ1.Base());
            f.Value.FileEnd = ((tokenꓸPos)(fileʗ1.Base() + fileʗ1.Size()));
            Ꮡp.Value.errors.Sort();
            Ꮡerr.ValueSlot = Ꮡp.Value.errors.Err();
        }, ref ᒐ);
        // parse source
        Ꮡp.init(@file, text, mode);
        f = Ꮡp.parseFile();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (f, Ꮡerr.ValueSlot);
}

// ParseDir calls [ParseFile] for all files with names ending in ".go" in the
// directory specified by path and returns a map of package name -> package
// AST with all the packages found.
//
// If filter != nil, only the files with [fs.FileInfo] entries passing through
// the filter (and ending in ".go") are considered. The mode bits are passed
// to [ParseFile] unchanged. Position information is recorded in fset, which
// must not be nil.
//
// If the directory couldn't be read, a nil map and the respective error are
// returned. If a parse error occurred, a non-nil but incomplete map and the
// first error encountered are returned.
public static (map<@string, ж<ast.Package>> pkgs, error first) ParseDir(ж<token.FileSet> Ꮡfset, @string path, Func<fs.FileInfo, bool> filter, Mode mode) {
    map<@string, ж<ast.Package>> pkgs = default!;
    error first = default!;

    var (list, err) = os.ReadDir(path);
    if (err != default!) {
        return (default!, err);
    }
    pkgs = new map<@string, ж<ast.Package>>();
    foreach (var (_, d) in list) {
        if (d.IsDir() || !strings.HasSuffix(d.Name(), ".go"u8)) {
            continue;
        }
        if (filter != default!) {
            var (info, errΔ1) = d.Info();
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            if (!filter(info)) {
                continue;
            }
        }
        @string filename = filepath.Join(path, d.Name());
        {
            var (src, errΔ2) = ParseFile(Ꮡfset, filename, default!, mode); if (errΔ2 == default!){
                ref var name = ref heap<@string>(out var Ꮡname);
                name = src.Value.Name.Value.Name;
                var (pkg, found) = pkgs[name, ꟷ];
                if (!found) {
                    pkg = Ꮡ(new ast.Package(
                        Name: name,
                        Files: new map<@string, ж<ast.File>>()
                    ));
                    pkgs[name] = pkg;
                }
                pkg.Value.Files[filename] = src;
            } else 
            if (first == default!) {
                first = errΔ2;
            }
        }
    }
    return (pkgs, first);
}

// ParseExprFrom is a convenience function for parsing an expression.
// The arguments have the same meaning as for [ParseFile], but the source must
// be a valid Go (type or value) expression. Specifically, fset must not
// be nil.
//
// If the source couldn't be read, the returned AST is nil and the error
// indicates the specific failure. If the source was read but syntax
// errors were found, the result is a partial AST (with [ast.Bad]* nodes
// representing the fragments of erroneous source code). Multiple errors
// are returned via a scanner.ErrorList which is sorted by source position.
public static (ast.Expr expr, error err) ParseExprFrom(ж<token.FileSet> Ꮡfset, @string filename, any src, Mode mode) {
    ast.Expr expr = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var fset = ref Ꮡfset.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        if (Ꮡfset == nil) {
            throw panic("parser.ParseExprFrom: no token.FileSet provided (fset == nil)");
        }
        // get source
        (var text, err) = readSource(filename, src);
        if (err != default!) {
            (expr, err) = (default!, err); goto ᒐdone;
        }
        ref var p = ref heap(new parser(), out var Ꮡp);
        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    // resume same panic if it's not a bailout
                    ref var bail = ref heap<bailout>(out var Ꮡbail);
                    (bail, var ok) = e._<bailout>(ᐧ);
                    if (!ok){
                        throw panic(e);
                    } else 
                    if (bail.msg != ""u8) {
                        Ꮡp.Value.errors.Add(Ꮡp.Value.@file.Position(bail.pos), bail.msg);
                    }
                }
            }
            Ꮡp.Value.errors.Sort();
            Ꮡerr.ValueSlot = Ꮡp.Value.errors.Err();
        }, ref ᒐ);
        // parse expr
        var @file = Ꮡfset.AddFile(filename, -1, len(text));
        Ꮡp.init(@file, text, mode);
        expr = Ꮡp.parseRhs();
        // If a semicolon was inserted, consume it;
        // report an error if there's more tokens.
        if (p.tok == token.SEMICOLON && p.lit == "\n"u8) {
            p.next();
        }
        Ꮡp.expect(token.EOF);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (expr, Ꮡerr.ValueSlot);
}

// ParseExpr is a convenience function for obtaining the AST of an expression x.
// The position information recorded in the AST is undefined. The filename used
// in error messages is the empty string.
//
// If syntax errors were found, the result is a partial AST (with [ast.Bad]* nodes
// representing the fragments of erroneous source code). Multiple errors are
// returned via a scanner.ErrorList which is sorted by source position.
public static (ast.Expr, error) ParseExpr(@string x) {
    return ParseExprFrom(token.NewFileSet(), ""u8, slice<byte>(x), 0);
}

} // end parser_package
