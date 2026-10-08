// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using strings = strings_package;
using global::go.go;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸжastꓸIdent = Span<ж<global::go.go.ast_package.Ident>>;

partial class parser_package {

internal const bool debugResolve = false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unbalancedScopesˢ = "unbalanced scopes"u8;
internal static readonly @string unbalancedLabelScopesˢ = "unbalanced label scopes"u8;
internal static readonly @string objectAlreadyResolvedˢ = "object already resolved"u8;
internal static readonly @string resolvedSVToPackageˢ = "resolved %s@%v to package object %v"u8;

internal partial interface resolveFile_type /*dyn*/ {
    tokenꓸPos Pos();
}

// resolveFile walks the given file to resolve identifiers within the file
// scope, updating ast.Ident.Obj fields with declaration information.
//
// If declErr is non-nil, it is used to report declaration errors during
// resolution. tok is used to format position in error messages.
internal static void resolveFile(ref ast.File @file, ж<tokenꓸFile> Ꮡhandle, Action<tokenꓸPos, @string> declErr) {
    var pkgScope = ast.NewScope(nil);
    var r = Ꮡ(new resolver(
        handle: Ꮡhandle,
        declErr: declErr,
        topScope: pkgScope,
        pkgScope: pkgScope,
        depth: 1
    ));
    foreach (var (_, decl) in @file.Decls) {
        ast.Walk(new resolverжVisitor(r), decl);
    }
    r.closeScope();
    assert((~r).topScope == nil, unbalancedScopesˢ);
    assert((~r).labelScope == nil, unbalancedLabelScopesˢ);
    // resolve global identifiers within the same file
    nint i = 0;
    foreach (var (_, ident) in (~r).unresolved) {
        // i <= index for current ident
        assert((~ident).Obj == unresolved, objectAlreadyResolvedˢ);
        ident.Value.Obj = (~r).pkgScope.Lookup((~ident).Name); // also removes unresolved sentinel
        if ((~ident).Obj == nil){
            r.Value.unresolved[i] = ident;
            i++;
        } else 
        if (debugResolve) {
            tokenꓸPos pos = (~(~ident).Obj).Decl._<resolveFile_type>().Pos();
            r.trace(resolvedSVToPackageˢ, (~ident).Name, ident.Pos(), pos);
        }
    }
    @file.Scope = r.Value.pkgScope;
    @file.Unresolved = (~r).unresolved.slice(0, i);
}

internal const nint maxScopeDepth = 1000;

partial struct resolver {
    internal ж<tokenꓸFile> handle;
    internal Action<tokenꓸPos, @string> declErr;
    // Ordinary identifier scopes
    internal ж<ast.Scope> pkgScope; // pkgScope.Outer == nil
    internal ж<ast.Scope> topScope; // top-most scope; may be pkgScope
    internal slice<ж<ast.Ident>> unresolved; // unresolved identifiers
    internal nint depth;         // scope depth
    // Label scopes
    // (maintained by open/close LabelScope)
    internal ж<ast.Scope> labelScope;  // label scope for current function
    internal slice<slice<ж<ast.Ident>>> targetStack; // stack of unresolved labels
}

internal static void trace(this ref resolver r, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    fmt.Println(strings.Repeat(". "u8, r.depth) + r.sprintf(format, args.ꓸꓸꓸ));
}

internal static @string sprintf(this ref resolver r, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    foreach (var (i, arg) in args) {
        switch (arg.type()) {
        case tokenꓸPos argΔ1: {
            args[i] = r.handle.Position(argΔ1);
            break;
        }}
    }
    return fmt.Sprintf(format, args.ꓸꓸꓸ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string openingScopeVˢ = "opening scope @%v"u8;

internal static void openScope(this ref resolver r, tokenꓸPos pos) {
    r.depth++;
    if (r.depth > maxScopeDepth) {
        throw panic(new bailout(pos: pos, msg: "exceeded max scope depth during object resolution"u8));
    }
    if (debugResolve) {
        r.trace(openingScopeVˢ, pos);
    }
    r.topScope = ast.NewScope(r.topScope);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string closingScopeˢ = "closing scope"u8;

internal static void closeScope(this ref resolver r) {
    r.depth--;
    if (debugResolve) {
        r.trace(closingScopeˢ);
    }
    r.topScope = r.topScope.Value.Outer;
}

internal static void openLabelScope(this ref resolver r) {
    r.labelScope = ast.NewScope(r.labelScope);
    r.targetStack = append(r.targetStack, (slice<ж<ast.Ident>>)(default!));
}

internal static void closeLabelScope(this ref resolver r) {
    // resolve labels
    nint n = len(r.targetStack) - 1;
    var scope = r.labelScope;
    foreach (var (_, ident) in r.targetStack[n]) {
        ident.Value.Obj = scope.Lookup((~ident).Name);
        if ((~ident).Obj == nil && r.declErr != default!) {
            r.declErr(ident.Pos(), fmt.Sprintf("label %s undefined"u8, (~ident).Name));
        }
    }
    // pop label scope
    r.targetStack = r.targetStack.slice(0, n);
    r.labelScope = r.labelScope.Value.Outer;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string declaringSVˢ = "declaring %s@%v"u8;

internal static void declare(this ref resolver r, any decl, any data, ж<ast.Scope> Ꮡscope, ast.ObjKind kind, params ꓸꓸꓸжastꓸIdent identsʗp) {
    var idents = identsʗp.sslice();

    ref var scope = ref Ꮡscope.DerefOrNull();
    foreach (var (_, ident) in idents) {
        if ((~ident).Obj != nil) {
            throw panic(fmt.Sprintf("%v: identifier %s already declared or resolved"u8, ident.Pos(), (~ident).Name));
        }
        var obj = ast.NewObj(kind, (~ident).Name);
        // remember the corresponding declaration for redeclaration
        // errors and global variable resolution/typechecking phase
        obj.Value.Decl = decl;
        obj.Value.Data = data;
        // Identifiers (for receiver type parameters) are written to the scope, but
        // never set as the resolved object. See go.dev/issue/50956.
        {
            var (_, ok) = decl._<ж<ast.Ident>>(ᐧ); if (!ok) {
                ident.Value.Obj = obj;
            }
        }
        if ((~ident).Name != "_"u8) {
            if (debugResolve) {
                r.trace(declaringSVˢ, (~ident).Name, ident.Pos());
            }
            {
                var alt = scope.Insert(obj); if (alt != nil && r.declErr != default!) {
                    @string prevDecl = ""u8;
                    {
                        tokenꓸPos pos = alt.Pos(); if (pos.IsValid()) {
                            prevDecl = r.sprintf("\n\tprevious declaration at %v"u8, pos);
                        }
                    }
                    r.declErr(ident.Pos(), fmt.Sprintf("%s redeclared in this block%s"u8, (~ident).Name, prevDecl));
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string identifierAlreadyˢ = "identifier already declared or resolved"u8;
internal static readonly @string noNewVariablesOnLeftSideˢ = "no new variables on left side of :="u8;

internal static void shortVarDecl(this ref resolver r, ж<ast.AssignStmt> Ꮡdecl) {
    ref var decl = ref Ꮡdecl.DerefOrNull();

    // Go spec: A short variable declaration may redeclare variables
    // provided they were originally declared in the same block with
    // the same type, and at least one of the non-blank variables is new.
    nint n = 0; // number of new variables
    foreach (var (_, x) in decl.Lhs) {
        {
            var (ident, isIdent) = x._<ж<ast.Ident>>(ᐧ); if (isIdent) {
                assert((~ident).Obj == nil, identifierAlreadyˢ);
                var obj = ast.NewObj(ast.Var, (~ident).Name);
                // remember corresponding assignment for other tools
                obj.Value.Decl = Ꮡdecl.OrTypedNil();
                ident.Value.Obj = obj;
                if ((~ident).Name != "_"u8) {
                    if (debugResolve) {
                        r.trace(declaringSVˢ, (~ident).Name, ident.Pos());
                    }
                    {
                        var alt = r.topScope.Insert(obj); if (alt != nil){
                            ident.Value.Obj = alt; // redeclaration
                        } else {
                            n++; // new declaration
                        }
                    }
                }
            }
        }
    }
    if (n == 0 && r.declErr != default!) {
        r.declErr(decl.Lhs[0].Pos(), noNewVariablesOnLeftSideˢ);
    }
}

// The unresolved object is a sentinel to mark identifiers that have been added
// to the list of unresolved identifiers. The sentinel is only used for verifying
// internal consistency.
internal static ж<ast.Object> unresolved = @new<ast.Object>();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string resolvedVSToVˢ = "resolved %v:%s to %v"u8;
internal static readonly @string objWithNoNameˢ = "obj with no name"u8;

// If x is an identifier, resolve attempts to resolve x by looking up
// the object it denotes. If no object is found and collectUnresolved is
// set, x is marked as unresolved and collected in the list of unresolved
// identifiers.
internal static void resolve(this ref resolver r, ж<ast.Ident> Ꮡident, bool collectUnresolved) {
    ref var ident = ref Ꮡident.DerefOrNull();

    if (ident.Obj != nil) {
        throw panic(r.sprintf("%v: identifier %s already declared or resolved"u8, ident.Pos(), ident.Name));
    }
    // '_' should never refer to existing declarations, because it has special
    // handling in the spec.
    if (ident.Name == "_"u8) {
        return;
    }
    for (var s = r.topScope; s != nil; s = s.Value.Outer) {
        {
            var obj = s.Lookup(ident.Name); if (obj != nil) {
                if (debugResolve) {
                    r.trace(resolvedVSToVˢ, ident.Pos(), ident.Name, obj.OrTypedNil());
                }
                assert((~obj).Name != ""u8, objWithNoNameˢ);
                // Identifiers (for receiver type parameters) are written to the scope,
                // but never set as the resolved object. See go.dev/issue/50956.
                {
                    var (_, ok) = (~obj).Decl._<ж<ast.Ident>>(ᐧ); if (!ok) {
                        ident.Obj = obj;
                    }
                }
                return;
            }
        }
    }
    // all local scopes are known, so any unresolved identifier
    // must be found either in the file scope, package scope
    // (perhaps in another file), or universe scope --- collect
    // them so that they can be resolved later
    if (collectUnresolved) {
        ident.Obj = unresolved;
        r.unresolved = append(r.unresolved, Ꮡident);
    }
}

internal static void walkExprs(this ж<resolver> Ꮡr, slice<ast.Expr> list) {
    foreach (var (_, node) in list) {
        ast.Walk(new resolverжVisitor(Ꮡr), node);
    }
}

internal static void walkLHS(this ж<resolver> Ꮡr, slice<ast.Expr> list) {
    foreach (var (_, expr) in list) {
        var exprΔ1 = ast.Unparen(expr);
        {
            var (_, ok) = exprΔ1._<ж<ast.Ident>>(ᐧ); if (!ok && exprΔ1 != default!) {
                ast.Walk(new resolverжVisitor(Ꮡr), exprΔ1);
            }
        }
    }
}

internal static void walkStmts(this ж<resolver> Ꮡr, slice<ast.Stmt> list) {
    foreach (var (_, stmt) in list) {
        ast.Walk(new resolverжVisitor(Ꮡr), stmt);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nodeTVˢ = "node %T@%v"u8;

internal static ast.Visitor Visit(this ж<resolver> Ꮡr, ast.Node node) {
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        if (debugResolve && node != default!) {
            r.trace(nodeTVˢ, node, node.Pos());
        }
        switch (node.type()) {
        case ж<ast.Ident> n: {
            r.resolve(n, // Expressions.
 true);
            break;
        }
        case ж<ast.FuncLit> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            Ꮡr.walkFuncType((~n).Type);
            Ꮡr.walkBody((~n).Body);
            break;
        }
        case ж<ast.SelectorExpr> n: {
            ast.Walk(new resolverжVisitor(Ꮡr), (~n).X);
            break;
        }
        case ж<ast.StructType> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            Ꮡr.walkFieldList((~n).Fields, // Note: don't try to resolve n.Sel, as we don't support qualified
 // resolution.
 ast.Var);
            break;
        }
        case ж<ast.FuncType> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            Ꮡr.walkFuncType(n);
            break;
        }
        case ж<ast.CompositeLit> n: {
            if ((~n).Type != default!) {
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Type);
            }
            foreach (var (_, e) in (~n).Elts) {
                {
                    var (kv, _) = e._<ж<ast.KeyValueExpr>>(ᐧ); if (kv != nil){
                        // See go.dev/issue/45160: try to resolve composite lit keys, but don't
                        // collect them as unresolved if resolution failed. This replicates
                        // existing behavior when resolving during parsing.
                        {
                            var (ident, _) = (~kv).Key._<ж<ast.Ident>>(ᐧ); if (ident != nil){
                                r.resolve(ident, false);
                            } else {
                                ast.Walk(new resolverжVisitor(Ꮡr), (~kv).Key);
                            }
                        }
                        ast.Walk(new resolverжVisitor(Ꮡr), (~kv).Value);
                    } else {
                        ast.Walk(new resolverжVisitor(Ꮡr), e);
                    }
                }
            }
            break;
        }
        case ж<ast.InterfaceType> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            Ꮡr.walkFieldList((~n).Methods, ast.Fun);
            break;
        }
        case ж<ast.LabeledStmt> n: {
            r.declare(n.OrTypedNil(), // Statements
 default!, r.labelScope, ast.Lbl, (~n).Label);
            ast.Walk(new resolverжVisitor(Ꮡr), (~n).Stmt);
            break;
        }
        case ж<ast.AssignStmt> n: {
            Ꮡr.walkExprs((~n).Rhs);
            if ((~n).Tok == token.DEFINE){
                r.shortVarDecl(n);
            } else {
                Ꮡr.walkExprs((~n).Lhs);
            }
            break;
        }
        case ж<ast.BranchStmt> n: {
            if ((~n).Tok != token.FALLTHROUGH && (~n).Label != nil) {
                // add to list of unresolved targets
                nint depth = len(r.targetStack) - 1;
                r.targetStack[depth] = append(r.targetStack[depth], (~n).Label);
            }
            break;
        }
        case ж<ast.BlockStmt> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            Ꮡr.walkStmts((~n).List);
            break;
        }
        case ж<ast.IfStmt> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            if ((~n).Init != default!) {
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Init);
            }
            ast.Walk(new resolverжVisitor(Ꮡr), (~n).Cond);
            ast.Walk(new resolverжVisitor(Ꮡr), new ast.BlockStmtжNode((~n).Body));
            if ((~n).Else != default!) {
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Else);
            }
            break;
        }
        case ж<ast.CaseClause> n: {
            Ꮡr.walkExprs((~n).List);
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            Ꮡr.walkStmts((~n).Body);
            break;
        }
        case ж<ast.SwitchStmt> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            if ((~n).Init != default!) {
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Init);
            }
            if ((~n).Tag != default!) {
                // The scope below reproduces some unnecessary behavior of the parser,
                // opening an extra scope in case this is a type switch. It's not needed
                // for expression switches.
                // TODO: remove this once we've matched the parser resolution exactly.
                if ((~n).Init != default!) {
                    r.openScope((~n).Tag.Pos());
                    defer(Ꮡr.closeScope, ref ᒐ);
                }
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Tag);
            }
            if ((~n).Body != nil) {
                Ꮡr.walkStmts((~(~n).Body).List);
            }
            break;
        }
        case ж<ast.TypeSwitchStmt> n: {
            if ((~n).Init != default!) {
                r.openScope(n.Pos());
                defer(Ꮡr.closeScope, ref ᒐ);
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Init);
            }
            r.openScope((~n).Assign.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            ast.Walk(new resolverжVisitor(Ꮡr), (~n).Assign);
            if ((~n).Body != nil) {
                // s.Body consists only of case clauses, so does not get its own
                // scope.
                Ꮡr.walkStmts((~(~n).Body).List);
            }
            break;
        }
        case ж<ast.CommClause> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            if ((~n).Comm != default!) {
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Comm);
            }
            Ꮡr.walkStmts((~n).Body);
            break;
        }
        case ж<ast.SelectStmt> n: {
            if ((~n).Body != nil) {
                // as for switch statements, select statement bodies don't get their own
                // scope.
                Ꮡr.walkStmts((~(~n).Body).List);
            }
            break;
        }
        case ж<ast.ForStmt> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            if ((~n).Init != default!) {
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Init);
            }
            if ((~n).Cond != default!) {
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Cond);
            }
            if ((~n).Post != default!) {
                ast.Walk(new resolverжVisitor(Ꮡr), (~n).Post);
            }
            ast.Walk(new resolverжVisitor(Ꮡr), new ast.BlockStmtжNode((~n).Body));
            break;
        }
        case ж<ast.RangeStmt> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            ast.Walk(new resolverжVisitor(Ꮡr), (~n).X);
            slice<ast.Expr> lhs = default!;
            if ((~n).Key != default!) {
                lhs = append(lhs, (~n).Key);
            }
            if ((~n).Value != default!) {
                lhs = append(lhs, (~n).Value);
            }
            if (len(lhs) > 0) {
                if ((~n).Tok == token.DEFINE){
                    // Note: we can't exactly match the behavior of object resolution
                    // during the parsing pass here, as it uses the position of the RANGE
                    // token for the RHS OpPos. That information is not contained within
                    // the AST.
                    var @as = Ꮡ(new ast.AssignStmt(
                        Lhs: lhs,
                        Tok: token.DEFINE,
                        TokPos: (~n).TokPos,
                        Rhs: new ast.Expr[]{new ast.UnaryExprжExpr(Ꮡ(new ast.UnaryExpr(Op: token.RANGE, X: (~n).X)))}.slice()
                    ));
                    // TODO(rFindley): this walkLHS reproduced the parser resolution, but
                    // is it necessary? By comparison, for a normal AssignStmt we don't
                    // walk the LHS in case there is an invalid identifier list.
                    Ꮡr.walkLHS(lhs);
                    r.shortVarDecl(@as);
                } else {
                    Ꮡr.walkExprs(lhs);
                }
            }
            ast.Walk(new resolverжVisitor(Ꮡr), new ast.BlockStmtжNode((~n).Body));
            break;
        }
        case ж<ast.GenDecl> n: {
            var exprᴛ1 = (~n).Tok;
            if (exprᴛ1 == token.CONST || exprᴛ1 == token.VAR) {
                foreach (var (i, spec) in (~n).Specs) {
                    // Declarations
                    var specΔ1 = spec._<ж<ast.ValueSpec>>();
                    ast.ObjKind kind = ast.Con;
                    if ((~n).Tok == token.VAR) {
                        kind = ast.Var;
                    }
                    Ꮡr.walkExprs((~specΔ1).Values);
                    if ((~specΔ1).Type != default!) {
                        ast.Walk(new resolverжVisitor(Ꮡr), (~specΔ1).Type);
                    }
                    r.declare(specΔ1.OrTypedNil(), i, r.topScope, kind, (~specΔ1).Names.ꓸꓸꓸ);
                }
            }
            else if (exprᴛ1 == token.TYPE) {
                foreach (var (_, spec) in (~n).Specs) {
                    var specΔ1 = spec._<ж<ast.TypeSpec>>();
                    // Go spec: The scope of a type identifier declared inside a function begins
                    // at the identifier in the TypeSpec and ends at the end of the innermost
                    // containing block.
                    r.declare(specΔ1.OrTypedNil(), default!, r.topScope, ast.Typ, (~specΔ1).Name);
                    if ((~specΔ1).TypeParams != nil) {
                        r.openScope(specΔ1.Pos());
                        defer(Ꮡr.closeScope, ref ᒐ);
                        Ꮡr.walkTParams((~specΔ1).TypeParams);
                    }
                    ast.Walk(new resolverжVisitor(Ꮡr), (~specΔ1).Type);
                }
            }

            break;
        }
        case ж<ast.FuncDecl> n: {
            r.openScope(n.Pos());
            defer(Ꮡr.closeScope, ref ᒐ);
            Ꮡr.walkRecv((~n).Recv);
            if ((~(~n).Type).TypeParams != nil) {
                // Open the function scope.
                // Type parameters are walked normally: they can reference each other, and
                // can be referenced by normal parameters.
                Ꮡr.walkTParams((~(~n).Type).TypeParams);
            }
            Ꮡr.resolveList((~(~n).Type).Params);
            Ꮡr.resolveList((~(~n).Type).Results);
            r.declareList((~n).Recv, // TODO(rFindley): need to address receiver type parameters.
 // Resolve and declare parameters in a specific order to get duplicate
 // declaration errors in the correct location.
 ast.Var);
            r.declareList((~(~n).Type).Params, ast.Var);
            r.declareList((~(~n).Type).Results, ast.Var);
            Ꮡr.walkBody((~n).Body);
            if ((~n).Recv == nil && (~(~n).Name).Name != "init"u8) {
                r.declare(n.OrTypedNil(), default!, r.pkgScope, ast.Fun, (~n).Name);
            }
            break;
        }
        default: {
            var n = node;
            return new resolverжVisitor(Ꮡr);
        }}
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void walkFuncType(this ж<resolver> Ꮡr, ж<ast.FuncType> Ꮡtyp) {
    ref var r = ref Ꮡr.DerefOrNull();
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // typ.TypeParams must be walked separately for FuncDecls.
    Ꮡr.resolveList(typ.Params);
    Ꮡr.resolveList(typ.Results);
    r.declareList(typ.Params, ast.Var);
    r.declareList(typ.Results, ast.Var);
}

internal static void resolveList(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡlist) {
    ref var list = ref Ꮡlist.DerefOrNull();

    if (Ꮡlist == nil) {
        return;
    }
    foreach (var (_, f) in list.List) {
        if ((~f).Type != default!) {
            ast.Walk(new resolverжVisitor(Ꮡr), (~f).Type);
        }
    }
}

internal static void declareList(this ref resolver r, ж<ast.FieldList> Ꮡlist, ast.ObjKind kind) {
    ref var list = ref Ꮡlist.DerefOrNull();

    if (Ꮡlist == nil) {
        return;
    }
    foreach (var (_, f) in list.List) {
        r.declare(f.OrTypedNil(), default!, r.topScope, kind, (~f).Names.ꓸꓸꓸ);
    }
}

internal static void walkRecv(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡrecv) {
    ref var r = ref Ꮡr.DerefOrNull();
    ref var recv = ref Ꮡrecv.DerefOrNull();

    // If our receiver has receiver type parameters, we must declare them before
    // trying to resolve the rest of the receiver, and avoid re-resolving the
    // type parameter identifiers.
    if (Ꮡrecv == nil || len(recv.List) == 0) {
        return; // nothing to do
    }
    var typ = recv.List[0].Value.Type;
    {
        var (ptr, ok) = typ._<ж<ast.StarExpr>>(ᐧ); if (ok) {
            typ = ptr.Value.X;
        }
    }
    slice<ast.Expr> declareExprs = default!;          // exprs to declare
    slice<ast.Expr> resolveExprs = default!;          // exprs to resolve
    switch (typ.type()) {
    case ж<ast.IndexExpr> typΔ1: {
        declareExprs = new ast.Expr[]{(~typΔ1).Index}.slice();
        resolveExprs = append(resolveExprs, (~typΔ1).X);
        break;
    }
    case ж<ast.IndexListExpr> typΔ1: {
        declareExprs = typΔ1.Value.Indices;
        resolveExprs = append(resolveExprs, (~typΔ1).X);
        break;
    }
    default: {
        var typΔ1 = typ;
        resolveExprs = append(resolveExprs, typΔ1);
        break;
    }}
    foreach (var (_, expr) in declareExprs) {
        {
            var (id, _) = expr._<ж<ast.Ident>>(ᐧ); if (id != nil){
                r.declare(expr, default!, r.topScope, ast.Typ, id);
            } else {
                // The receiver type parameter expression is invalid, but try to resolve
                // it anyway for consistency.
                resolveExprs = append(resolveExprs, expr);
            }
        }
    }
    foreach (var (_, expr) in resolveExprs) {
        if (expr != default!) {
            ast.Walk(new resolverжVisitor(Ꮡr), expr);
        }
    }
    // The receiver is invalid, but try to resolve it anyway for consistency.
    foreach (var (_, f) in recv.List[1..]) {
        if ((~f).Type != default!) {
            ast.Walk(new resolverжVisitor(Ꮡr), (~f).Type);
        }
    }
}

internal static void walkFieldList(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡlist, ast.ObjKind kind) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (Ꮡlist == nil) {
        return;
    }
    Ꮡr.resolveList(Ꮡlist);
    r.declareList(Ꮡlist, kind);
}

// walkTParams is like walkFieldList, but declares type parameters eagerly so
// that they may be resolved in the constraint expressions held in the field
// Type.
internal static void walkTParams(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡlist) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.declareList(Ꮡlist, ast.Typ);
    Ꮡr.resolveList(Ꮡlist);
}

internal static void walkBody(this ж<resolver> Ꮡr, ж<ast.BlockStmt> Ꮡbody) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var r = ref Ꮡr.DerefOrNull();
        ref var body = ref Ꮡbody.DerefOrNull();

        if (Ꮡbody == nil) {
            return;
        }
        r.openLabelScope();
        ᒐd1 = true;
        Ꮡr.walkStmts(body.List);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡr.DerefOrNull().closeLabelScope(); ᒐ.Run(); }
}

} // end parser_package
