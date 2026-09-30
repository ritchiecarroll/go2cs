// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Extract example functions from file ASTs.
namespace go.go;

using cmp = cmp_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using lazyregexp = @internal.lazyregexp_package;
using path = path_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using @internal;
using global::go.go;
using global::go.unicode;
using ꓸꓸꓸжastꓸFile = Span<ж<global::go.go.ast_package.File>>;

partial class doc_package {

// An Example represents an example function found in a test source file.
[GoType] partial struct Example {
    public @string Name; // name of the item being exemplified (including optional suffix)
    public @string Suffix; // example suffix, without leading '_' (only populated by NewFromFiles)
    public @string Doc; // example function doc string
    public ast.Node Code;
    public ж<ast.File> Play; // a whole program version of the example
    public slice<ж<ast.CommentGroup>> Comments;
    public @string Output; // expected output
    public bool Unordered;
    public bool EmptyOutput; // expect empty output
    public nint Order; // original source code order
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "Test"u8;
internal static readonly @string benchmarkˢ = "Benchmark"u8;
internal static readonly @string fuzzˢ = "Fuzz"u8;
internal static readonly @string exampleˢ = "Example"u8;

// Examples returns the examples found in testFiles, sorted by Name field.
// The Order fields record the order in which the examples were encountered.
// The Suffix field is not populated when Examples is called directly, it is
// only populated by [NewFromFiles] for examples it finds in _test.go files.
//
// Playable Examples must be in a package whose name ends in "_test".
// An Example is "playable" (the Play field is non-nil) in either of these
// circumstances:
//   - The example function is self-contained: the function references only
//     identifiers from other packages (or predeclared identifiers, such as
//     "int") and the test file does not include a dot import.
//   - The entire test file is the example: the file contains exactly one
//     example function, zero test, fuzz test, or benchmark function, and at
//     least one top-level function, type, variable, or constant declaration
//     other than the example function.
public static slice<ж<Example>> Examples(params ꓸꓸꓸжastꓸFile testFilesʗp) {
    var testFiles = testFilesʗp.sslice();

    slice<ж<Example>> list = default!;
    foreach (var (_, @file) in testFiles) {
        var hasTests = false; // file contains tests, fuzz test, or benchmarks
        nint numDecl = 0; // number of non-import declarations in the file
        slice<ж<Example>> flist = default!;
        foreach (var (_, decl) in (~@file).Decls) {
            {
                var (g, okΔ1) = decl._<ж<ast.GenDecl>>(ᐧ); if (okΔ1 && (~g).Tok != token.IMPORT) {
                    numDecl++;
                    continue;
                }
            }
            var (f, ok) = decl._<ж<ast.FuncDecl>>(ᐧ);
            if (!ok || (~f).Recv != nil) {
                continue;
            }
            numDecl++;
            @string name = f.Value.Name.Value.Name;
            if (isTest(name, testˢ) || isTest(name, benchmarkˢ) || isTest(name, fuzzˢ)) {
                hasTests = true;
                continue;
            }
            if (!isTest(name, exampleˢ)) {
                continue;
            }
            {
                var @params = f.Value.Type.Value.Params; if (len((~@params).List) != 0) {
                    continue; // function has params; not a valid example
                }
            }
            if ((~f).Body == nil) {
                // ast.File.Body nil dereference (see issue 28044)
                continue;
            }
            ref var doc = ref heap(new @string(), out var Ꮡdoc);
            if ((~f).Doc != nil) {
                doc = (~f).Doc.Text();
            }
            ref var output = ref heap<@string>(out var Ꮡoutput);
            ref var unordered = ref heap<bool>(out var Ꮡunordered);
            (output, unordered, var hasOutput) = exampleOutput((~f).Body, (~@file).Comments);
            flist = append(flist, Ꮡ(new Example(
                Name: name[(int)(len("Example"))..],
                Doc: doc,
                Code: new ast.BlockStmtжNode((~f).Body),
                Play: playExample(ref (@file).DerefOrNull(), ref (f).DerefOrNull()),
                Comments: (~@file).Comments,
                Output: output,
                Unordered: unordered,
                EmptyOutput: output == ""u8 && hasOutput,
                Order: len(flist)
            )));
        }
        if (!hasTests && numDecl > 1 && len(flist) == 1) {
            // If this file only has one example function, some
            // other top-level declarations, and no tests or
            // benchmarks, use the whole file as the example.
            flist[0].Value.Code = new ast.FileжNode(@file);
            flist[0].Value.Play = playExampleFile(ref (@file).DerefOrNull());
        }
        list = appendꓸꓸꓸ(list, flist);
    }
    // sort by name
    slices.SortFunc(list, (ж<Example> a, ж<Example> b) => cmp.Compare((~a).Name, (~b).Name));
    return list;
}

internal static ж<lazyregexp.Regexp> outputPrefix = lazyregexp.New(@"(?i)^[[:space:]]*(unordered )?output:"u8);

// Extracts the expected output and whether there was a valid output comment.
internal static (@string output, bool unordered, bool ok) exampleOutput(ж<ast.BlockStmt> Ꮡb, slice<ж<ast.CommentGroup>> comments) {
    bool unordered = default!;

    {
        var (_, last) = lastComment(Ꮡb, comments); if (last != nil) {
            // test that it begins with the correct prefix
            @string text = last.Text();
            {
                var loc = outputPrefix.FindStringSubmatchIndex(text); if (loc != default!) {
                    if (loc[2] != -1) {
                        unordered = true;
                    }
                    text = text.slice(loc[1]);
                    // Strip zero or more spaces followed by \n or a single space.
                    text = strings.TrimLeft(text, " "u8);
                    if (len(text) > 0 && text[0] == (rune)'\n') {
                        text = text[1..];
                    }
                    return (text, unordered, true);
                }
            }
        }
    }
    return ("", false, false); // no suitable comment found
}

// isTest tells whether name looks like a test, example, fuzz test, or
// benchmark. It is a Test (say) if there is a character after Test that is not
// a lower-case letter. (We don't want Testiness.)
internal static bool isTest(@string name, @string prefix) {
    if (!strings.HasPrefix(name, prefix)) {
        return false;
    }
    if (len(name) == len(prefix)) {
        // "Test" is ok
        return true;
    }
    var (rune, _) = utf8.DecodeRuneInString(name.slice(len(prefix)));
    return !unicode.IsLower(rune);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ2 = "_test"u8;
internal static readonly @string mainˢ = "main"u8;

// playExample synthesizes a new *ast.File based on the provided
// file with the provided function body as the body of main.
internal static ж<ast.File> playExample(ref ast.File @file, ref ast.FuncDecl f) {
    var body = f.Body;
    if (!strings.HasSuffix((~@file.Name).Name, testˢ2)) {
        // We don't support examples that are part of the
        // greater package (yet).
        return default!;
    }
    // Collect top-level declarations in the file.
    var topDecls = new map<ж<ast.Object>, ast.Decl>();
    var typMethods = new map<@string, slice<ast.Decl>>();
    foreach (var (_, decl) in @file.Decls) {
        switch (decl.type()) {
        case ж<ast.FuncDecl> d: {
            if ((~d).Recv == nil){
                topDecls[(~(~d).Name).Obj] = new ast.FuncDeclжDecl(d);
            } else {
                if (len((~(~d).Recv).List) == 1) {
                    var t = (~(~d).Recv).List[0].Value.Type;
                    var (tname, _) = baseTypeName(t);
                    typMethods[tname] = append(typMethods[tname], (ast.Decl)(new ast.FuncDeclжDecl(d)));
                }
            }
            break;
        }
        case ж<ast.GenDecl> d: {
            foreach (var (_, spec) in (~d).Specs) {
                switch (spec.type()) {
                case ж<ast.TypeSpec> s: {
                    topDecls[(~(~s).Name).Obj] = new ast.GenDeclжDecl(d);
                    break;
                }
                case ж<ast.ValueSpec> s: {
                    foreach (var (_, name) in (~s).Names) {
                        topDecls[(~name).Obj] = new ast.GenDeclжDecl(d);
                    }
                    break;
                }}
            }
            break;
        }}
    }
    // Find unresolved identifiers and uses of top-level declarations.
    var (depDecls, unresolved) = findDeclsAndUnresolved(new ast.BlockStmtжNode(body), topDecls, typMethods);
    // Use unresolved identifiers to determine the imports used by this
    // example. The heuristic assumes package names match base import
    // paths for imports w/o renames (should be good enough most of the time).
    slice<ast.Spec> namedImports = default!;
    slice<ast.Spec> blankImports = default!;          // _ imports
    // To preserve the blank lines between groups of imports, find the
    // start position of each group, and assign that position to all
    // imports from that group.
    var groupStarts = findImportGroupStarts(@file.Imports);
    var groupStartsʗ1 = groupStarts;
    tokenꓸPos groupStart(ж<ast.ImportSpec> s) {
        foreach (var (i, start) in groupStartsʗ1) {
            if ((~(~s).Path).ValuePos < start) {
                return groupStartsʗ1[i - 1];
            }
        }
        return groupStartsʗ1[len(groupStartsʗ1) - 1];
    }
    foreach (var (_, s) in @file.Imports) {
        var (p, err) = strconv.Unquote((~(~s).Path).Value);
        if (err != default!) {
            continue;
        }
        if (p == "syscall/js"u8) {
            // We don't support examples that import syscall/js,
            // because the package syscall/js is not available in the playground.
            return default!;
        }
        @string n = path.Base(p);
        if ((~s).Name != nil) {
            n = s.Value.Name.Value.Name;
            var exprᴛ1 = n;
            if (exprᴛ1 == "_"u8) {
                blankImports = append(blankImports, (ast.Spec)(new ast.ImportSpecжSpec(s)));
                continue;
            }
            else if (exprᴛ1 == "."u8) {
                return default!;
            }

        }
        // We can't resolve dot imports (yet).
        if (unresolved[n]) {
            // Copy the spec and its path to avoid modifying the original.
            ref var spec = ref heap<ast.ImportSpec>(out var Ꮡspec);
            spec = s.Value;
            ref var pathΔ1 = ref heap<ast.BasicLit>(out var ᏑpathΔ1);
            pathΔ1 = (~s).Path.Value;
            spec.Path = ᏑpathΔ1;
            spec.Path.Value.ValuePos = groupStart(Ꮡspec);
            namedImports = append(namedImports, (ast.Spec)(new ast.ImportSpecжSpec(Ꮡspec)));
            delete(unresolved, n);
        }
    }
    // Remove predeclared identifiers from unresolved list.
    foreach (var (n, _) in unresolved) {
        if (predeclaredTypes[n] || predeclaredConstants[n] || predeclaredFuncs[n]) {
            delete(unresolved, n);
        }
    }
    // If there are other unresolved identifiers, give up because this
    // synthesized file is not going to build.
    if (len(unresolved) > 0) {
        return default!;
    }
    // Include documentation belonging to blank imports.
    slice<ж<ast.CommentGroup>> comments = default!;
    foreach (var (_, s) in blankImports) {
        {
            var c = s._<ж<ast.ImportSpec>>().Value.Doc; if (c != nil) {
                comments = append(comments, c);
            }
        }
    }
    // Include comments that are inside the function body.
    foreach (var (_, c) in @file.Comments) {
        if (body.Pos() <= c.Pos() && c.End() <= body.End()) {
            comments = append(comments, c);
        }
    }
    // Strip the "Output:" or "Unordered output:" comment and adjust body
    // end position.
    (body, comments) = stripOutputComment(body, comments);
    // Include documentation belonging to dependent declarations.
    foreach (var (_, d) in depDecls) {
        switch (d.type()) {
        case ж<ast.GenDecl> dΔ1: {
            if ((~dΔ1).Doc != nil) {
                comments = append(comments, (~dΔ1).Doc);
            }
            break;
        }
        case ж<ast.FuncDecl> dΔ1: {
            if ((~dΔ1).Doc != nil) {
                comments = append(comments, (~dΔ1).Doc);
            }
            break;
        }}
    }
    // Synthesize import declaration.
    var importDecl = Ꮡ(new ast.GenDecl(
        Tok: token.IMPORT,
        Lparen: 1, // Need non-zero Lparen and Rparen so that printer

        Rparen: 1
    ));
    // treats this as a factored import.
    importDecl.Value.Specs = appendꓸꓸꓸ(namedImports, blankImports);
    // Synthesize main function.
    var funcDecl = Ꮡ(new ast.FuncDecl(
        Name: ast.NewIdent(mainˢ),
        Type: f.Type,
        Body: body
    ));
    var decls = new slice<ast.Decl>(0, 2 + len(depDecls));
    decls = append(decls, (ast.Decl)(new ast.GenDeclжDecl(importDecl)));
    decls = appendꓸꓸꓸ(decls, depDecls);
    decls = append(decls, (ast.Decl)(new ast.FuncDeclжDecl(funcDecl)));
    slices.SortFunc(decls, (ast.Decl a, ast.Decl b) => cmp.Compare(a.Pos(), b.Pos()));
    slices.SortFunc(comments, (ж<ast.CommentGroup> a, ж<ast.CommentGroup> b) => cmp.Compare(a.Pos(), b.Pos()));
    // Synthesize file.
    return Ꮡ(new ast.File(
        Name: ast.NewIdent(mainˢ),
        Decls: decls,
        Comments: comments
    ));
}

// findDeclsAndUnresolved returns all the top-level declarations mentioned in
// the body, and a set of unresolved symbols (those that appear in the body but
// have no declaration in the program).
//
// topDecls maps objects to the top-level declaration declaring them (not
// necessarily obj.Decl, as obj.Decl will be a Spec for GenDecls, but
// topDecls[obj] will be the GenDecl itself).
internal static (slice<ast.Decl>, map<@string, bool>) findDeclsAndUnresolved(ast.Node body, map<ж<ast.Object>, ast.Decl> topDecls, map<@string, slice<ast.Decl>> typMethods) {
    // This function recursively finds every top-level declaration used
    // transitively by the body, populating usedDecls and usedObjs. Then it
    // trims down the declarations to include only the symbols actually
    // referenced by the body.
    var unresolved = new map<@string, bool>();
    ref var depDecls = ref heap<slice<ast.Decl>>(out var ᏑdepDecls);
    var usedDecls = new map<ast.Decl, bool>(); // set of top-level decls reachable from the body
    var usedObjs = new map<ж<ast.Object>, bool>(); // set of objects reachable from the body (each declared by a usedDecl)
    ref var inspectFunc = ref heap<Func<ast.Node, bool>>(out var ᏑinspectFunc);
    var topDeclsʗ1 = topDecls;
    var unresolvedʗ1 = unresolved;
    var usedDeclsʗ1 = usedDecls;
    var usedObjsʗ1 = usedObjs;
    inspectFunc = (ast.Node n) => {
        switch (n.type()) {
        case ж<ast.Ident> e: {
            if ((~e).Obj == nil && (~e).Name != "_"u8){
                unresolvedʗ1[(~e).Name] = true;
            } else 
            {
                var d = topDeclsʗ1[(~e).Obj]; if (d != default!) {
                    usedObjsʗ1[(~e).Obj] = true;
                    if (!usedDeclsʗ1[d]) {
                        usedDeclsʗ1[d] = true;
                        ᏑdepDecls.ValueSlot = append(ᏑdepDecls.ValueSlot, d);
                    }
                }
            }
            return true;
        }
        case ж<ast.SelectorExpr> e: {
            ast.Inspect((~e).X, // For selector expressions, only inspect the left hand side.
 // (For an expression like fmt.Println, only add "fmt" to the
 // set of unresolved names, not "Println".)
 ᏑinspectFunc.ValueSlot);
            return false;
        }
        case ж<ast.KeyValueExpr> e: {
            ast.Inspect((~e).Value, // For key value expressions, only inspect the value
 // as the key should be resolved by the type of the
 // composite literal.
 ᏑinspectFunc.ValueSlot);
            return false;
        }}
        return true;
    };
    void inspectFieldList(ж<ast.FieldList> fl) {
        if (fl != nil) {
            foreach (var (_, f) in (~fl).List) {
                ast.Inspect((~f).Type, ᏑinspectFunc.ValueSlot);
            }
        }
    }
    // Find the decls immediately referenced by body.
    ast.Inspect(body, inspectFunc);
    // Now loop over them, adding to the list when we find a new decl that the
    // body depends on. Keep going until we don't find anything new.
    for (nint i = 0; i < len(depDecls); i++) {
        switch (depDecls[i].type()) {
        case ж<ast.FuncDecl> d: {
            inspectFieldList((~(~d).Type).TypeParams);
            inspectFieldList((~(~d).Type).Params);
            inspectFieldList((~(~d).Type).Results);
            if ((~d).Body != nil) {
                // Inspect type parameters.
                // Inspect types of parameters and results. See #28492.
                // Functions might not have a body. See #42706.
                ast.Inspect(new ast.BlockStmtжNode((~d).Body), inspectFunc);
            }
            break;
        }
        case ж<ast.GenDecl> d: {
            foreach (var (_, spec) in (~d).Specs) {
                switch (spec.type()) {
                case ж<ast.TypeSpec> s: {
                    inspectFieldList((~s).TypeParams);
                    ast.Inspect((~s).Type, inspectFunc);
                    depDecls = appendꓸꓸꓸ(depDecls, typMethods[(~(~s).Name).Name]);
                    break;
                }
                case ж<ast.ValueSpec> s: {
                    if ((~s).Type != default!) {
                        ast.Inspect((~s).Type, inspectFunc);
                    }
                    foreach (var (_, val) in (~s).Values) {
                        ast.Inspect(val, inspectFunc);
                    }
                    break;
                }}
            }
            break;
        }}
    }
    // Some decls include multiple specs, such as a variable declaration with
    // multiple variables on the same line, or a parenthesized declaration. Trim
    // the declarations to include only the specs that are actually mentioned.
    // However, if there is a constant group with iota, leave it all: later
    // constant declarations in the group may have no value and so cannot stand
    // on their own, and removing any constant from the group could change the
    // values of subsequent ones.
    // See testdata/examples/iota.go for a minimal example.
    slice<ast.Decl> ds = default!;
    foreach (var (_, d) in depDecls) {
        switch (d.type()) {
        case ж<ast.FuncDecl> dΔ1: {
            ds = append(ds, (ast.Decl)(new ast.FuncDeclжDecl(dΔ1)));
            break;
        }
        case ж<ast.GenDecl> dΔ1: {
            var containsIota = false; // does any spec have iota?
            // Collect all Specs that were mentioned in the example.
            slice<ast.Spec> specs = default!;
            foreach (var (_, s) in (~dΔ1).Specs) {
                switch (s.type()) {
                case ж<ast.TypeSpec> sΔ1: {
                    if (usedObjs[(~(~sΔ1).Name).Obj]) {
                        specs = append(specs, (ast.Spec)(new ast.TypeSpecжSpec(sΔ1)));
                    }
                    break;
                }
                case ж<ast.ValueSpec> sΔ1: {
                    if (!containsIota) {
                        containsIota = hasIota(new ast.ValueSpecжSpec(sΔ1));
                    }
                    if (len((~sΔ1).Names) > 1 && len((~sΔ1).Values) == 1) {
                        // A ValueSpec may have multiple names (e.g. "var a, b int").
                        // Keep only the names that were mentioned in the example.
                        // Exception: the multiple names have a single initializer (which
                        // would be a function call with multiple return values). In that
                        // case, keep everything.
                        specs = append(specs, (ast.Spec)(new ast.ValueSpecжSpec(sΔ1)));
                        continue;
                    }
                    ref var ns = ref heap<ast.ValueSpec>(out var Ꮡns);
                    ns = sΔ1.Value;
                    ns.Names = default!;
                    ns.Values = default!;
                    foreach (var (i, n) in (~sΔ1).Names) {
                        if (usedObjs[(~n).Obj]) {
                            ns.Names = append(ns.Names, n);
                            if ((~sΔ1).Values != default!) {
                                ns.Values = append(ns.Values, (~sΔ1).Values[i]);
                            }
                        }
                    }
                    if (len(ns.Names) > 0) {
                        specs = append(specs, (ast.Spec)(new ast.ValueSpecжSpec(Ꮡns)));
                    }
                    break;
                }}
            }
            if (len(specs) > 0) {
                // Constant with iota? Keep it all.
                if ((~dΔ1).Tok == token.CONST && containsIota){
                    ds = append(ds, (ast.Decl)(new ast.GenDeclжDecl(dΔ1)));
                } else {
                    // Synthesize a GenDecl with just the Specs we need.
                    ref var nd = ref heap<ast.GenDecl>(out var Ꮡnd);
                    nd = dΔ1.Value; // copy the GenDecl
                    nd.Specs = specs;
                    if (len(specs) == 1) {
                        // Remove grouping parens if there is only one spec.
                        nd.Lparen = 0;
                    }
                    ds = append(ds, (ast.Decl)(new ast.GenDeclжDecl(Ꮡnd)));
                }
            }
            break;
        }}
    }
    return (ds, unresolved);
}

internal static bool hasIota(ast.Spec s) {
    var has = false;
    ast.Inspect(s, (ast.Node n) => {
        // Check that this is the special built-in "iota" identifier, not
        // a user-defined shadow.
        {
            var (id, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok && (~id).Name == "iota"u8 && (~id).Obj == nil) {
                has = true;
                return false;
            }
        }
        return true;
    });
    return has;
}

// findImportGroupStarts finds the start positions of each sequence of import
// specs that are not separated by a blank line.
internal static slice<tokenꓸPos> findImportGroupStarts(slice<ж<ast.ImportSpec>> imps) {
    var startImps = findImportGroupStarts1(imps);
    var groupStarts = new slice<tokenꓸPos>(len(startImps));
    foreach (var (i, imp) in startImps) {
        groupStarts[i] = imp.Pos();
    }
    return groupStarts;
}

// Helper for findImportGroupStarts to ease testing.
internal static slice<ж<ast.ImportSpec>> findImportGroupStarts1(slice<ж<ast.ImportSpec>> origImps) {
    // Copy to avoid mutation.
    var imps = new slice<ж<ast.ImportSpec>>(len(origImps));
    copy(imps, origImps);
    // Assume the imports are sorted by position.
    slices.SortFunc(imps, (ж<ast.ImportSpec> a, ж<ast.ImportSpec> b) => cmp.Compare(a.Pos(), b.Pos()));
    // Assume gofmt has been applied, so there is a blank line between adjacent imps
    // if and only if they are more than 2 positions apart (newline, tab).
    slice<ж<ast.ImportSpec>> groupStarts = default!;
    tokenꓸPos prevEnd = ((tokenꓸPos)(-2));
    foreach (var (_, imp) in imps) {
        if (imp.Pos() - prevEnd > 2) {
            groupStarts = append(groupStarts, imp);
        }
        prevEnd = imp.End();
        // Account for end-of-line comments.
        if ((~imp).Comment != nil) {
            prevEnd = (~imp).Comment.End();
        }
    }
    return groupStarts;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string copyrightˢ = "Copyright"u8;

// playExampleFile takes a whole file example and synthesizes a new *ast.File
// such that the example is function main in package main.
internal static ж<ast.File> playExampleFile(ref ast.File @file) {
    // Strip copyright comment if present.
    var comments = @file.Comments;
    if (len(comments) > 0 && strings.HasPrefix(comments[0].Text(), copyrightˢ)) {
        comments = comments[1..];
    }
    // Copy declaration slice, rewriting the ExampleX function to main.
    slice<ast.Decl> decls = default!;
    foreach (var (_, vᴛ1) in @file.Decls) {
        var d = vᴛ1;

        {
            var (fΔ1, ok) = d._<ж<ast.FuncDecl>>(ᐧ); if (ok && isTest((~(~fΔ1).Name).Name, exampleˢ)) {
                // Copy the FuncDecl, as it may be used elsewhere.
                ref var newF = ref heap<ast.FuncDecl>(out var ᏑnewF);
                newF = fΔ1.Value;
                newF.Name = ast.NewIdent(mainˢ);
                (newF.Body, comments) = stripOutputComment((~fΔ1).Body, comments);
                d = new ast.FuncDeclжDecl(ᏑnewF);
            }
        }
        decls = append(decls, d);
    }
    // Copy the File, as it may be used elsewhere.
    ref var f = ref heap<ast.File>(out var Ꮡf);
    f = @file;
    f.Name = ast.NewIdent(mainˢ);
    f.Decls = decls;
    f.Comments = comments;
    return Ꮡf;
}

// stripOutputComment finds and removes the "Output:" or "Unordered output:"
// comment from body and comments, and adjusts the body block's end position.
internal static (ж<ast.BlockStmt>, slice<ж<ast.CommentGroup>>) stripOutputComment(ж<ast.BlockStmt> Ꮡbody, slice<ж<ast.CommentGroup>> comments) {
    ref var body = ref Ꮡbody.DerefOrNull();

    // Do nothing if there is no "Output:" or "Unordered output:" comment.
    var (i, last) = lastComment(Ꮡbody, comments);
    if (last == nil || !outputPrefix.MatchString(last.Text())) {
        return (Ꮡbody, comments);
    }
    // Copy body and comments, as the originals may be used elsewhere.
    var newBody = Ꮡ(new ast.BlockStmt(
        Lbrace: body.Lbrace,
        List: body.List,
        Rbrace: last.Pos()
    ));
    var newComments = new slice<ж<ast.CommentGroup>>(len(comments) - 1);
    copy(newComments, comments.slice(0, i));
    copy(newComments.slice(i), comments.slice(i + 1));
    return (newBody, newComments);
}

// lastComment returns the last comment inside the provided block.
internal static (nint i, ж<ast.CommentGroup> last) lastComment(ж<ast.BlockStmt> Ꮡb, slice<ж<ast.CommentGroup>> c) {
    nint i = default!;
    ж<ast.CommentGroup> last = default!;

    ref var b = ref Ꮡb.DerefOrNull();
    if (Ꮡb == nil) {
        return (i, last);
    }
    tokenꓸPos pos = b.Pos();
    tokenꓸPos end = b.End();
    foreach (var (j, cg) in c) {
        if (cg.Pos() < pos) {
            continue;
        }
        if (cg.End() > end) {
            break;
        }
        (i, last) = (j, cg);
    }
    return (i, last);
}

// classifyExamples classifies examples and assigns them to the Examples field
// of the relevant Func, Type, or Package that the example is associated with.
//
// The classification process is ambiguous in some cases:
//
//   - ExampleFoo_Bar matches a type named Foo_Bar
//     or a method named Foo.Bar.
//   - ExampleFoo_bar matches a type named Foo_bar
//     or Foo (with a "bar" suffix).
//
// Examples with malformed names are not associated with anything.
internal static void classifyExamples(ж<Package> Ꮡp, slice<ж<Example>> examples) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (len(examples) == 0) {
        return;
    }
    // Mapping of names for funcs, types, and methods to the example listing.
    var ids = new map<@string, ж<slice<ж<Example>>>>();
    ids[""u8] = Ꮡp.of(Package.ᏑExamples); // package-level examples have an empty name
    foreach (var (_, f) in p.Funcs) {
        if (!token.IsExported((~f).Name)) {
            continue;
        }
        ids[(~f).Name] = f.of(Func.ᏑExamples);
    }
    foreach (var (_, t) in p.Types) {
        if (!token.IsExported((~t).Name)) {
            continue;
        }
        ids[(~t).Name] = t.of(Type.ᏑExamples);
        foreach (var (_, f) in (~t).Funcs) {
            if (!token.IsExported((~f).Name)) {
                continue;
            }
            ids[(~f).Name] = f.of(Func.ᏑExamples);
        }
        foreach (var (_, m) in (~t).Methods) {
            if (!token.IsExported((~m).Name)) {
                continue;
            }
            ids[strings.TrimPrefix(nameWithoutInst((~m).Recv), "*"u8) + "_"u8 + (~m).Name] = m.of(Func.ᏑExamples);
        }
    }
    // Group each example with the associated func, type, or method.
    foreach (var (_, ex) in examples) {
        // Consider all possible split points for the suffix
        // by starting at the end of string (no suffix case),
        // then trying all positions that contain a '_' character.
        //
        // An association is made on the first successful match.
        // Examples with malformed names that match nothing are skipped.
        for (nint i = len((~ex).Name); i >= 0; i = strings.LastIndexByte((~ex).Name.slice(0, i), (rune)'_')) {
            var (prefix, suffix, ok) = splitExampleName((~ex).Name, i);
            if (!ok) {
                continue;
            }
            (var exs, ok) = ids[prefix, ꟷ];
            if (!ok) {
                continue;
            }
            ex.Value.Suffix = suffix;
            exs.ValueSlot = append(exs.ValueSlot, ex);
            break;
        }
    }
    // Sort list of example according to the user-specified suffix name.
    foreach (var (_, exs) in ids) {
        slices.SortFunc(exs.ValueSlot, (ж<Example> a, ж<Example> b) => cmp.Compare((~a).Suffix, (~b).Suffix));
    }
}

// nameWithoutInst returns name if name has no brackets. If name contains
// brackets, then it returns name with all the contents between (and including)
// the outermost left and right bracket removed.
//
// Adapted from debug/gosym/symtab.go:Sym.nameWithoutInst.
internal static @string nameWithoutInst(@string name) {
    nint start = strings.Index(name, "["u8);
    if (start < 0) {
        return name;
    }
    nint end = strings.LastIndex(name, "]"u8);
    if (end < 0) {
        // Malformed name, should contain closing bracket too.
        return name;
    }
    return name.slice(0, start) + name.slice(end + 1);
}

// splitExampleName attempts to split example name s at index i,
// and reports if that produces a valid split. The suffix may be
// absent. Otherwise, it must start with a lower-case letter and
// be preceded by '_'.
//
// One of i == len(s) or s[i] == '_' must be true.
internal static (@string prefix, @string suffix, bool ok) splitExampleName(@string s, nint i) {
    @string prefix = default!;
    @string suffix = default!;

    if (i == len(s)) {
        return (s, "", true);
    }
    if (i == len(s) - 1) {
        return ("", "", false);
    }
    (prefix, suffix) = (s.slice(0, i), s.slice(i + 1));
    return (prefix, suffix, isExampleSuffix(suffix));
}

internal static bool isExampleSuffix(@string s) {
    var (r, size) = utf8.DecodeRuneInString(s);
    return size > 0 && unicode.IsLower(r);
}

} // end doc_package
