// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using static global::go.@internal.types.errors_package;
using slices = slices_package;
using errors = global::go.@internal.types.errors_package;
using global::go.@internal;
using global::go.go;

partial class types_package {

internal static void declare(this ж<Checker> Ꮡcheck, ж<ΔScope> Ꮡscope, ж<ast.Ident> Ꮡid, Object obj, tokenꓸPos pos) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    // spec: "The blank identifier, represented by the underscore
    // character _, may be used in a declaration like any other
    // identifier but the declaration does not introduce a new
    // binding."
    if (obj.Name() != "_"u8) {
        {
            var alt = Ꮡscope.Insert(obj); if (alt != default!) {
                var err = Ꮡcheck.newError(DuplicateDecl);
                err.addf(new Objectᴠpositioner(obj), "%s redeclared in this block"u8, obj.Name());
                err.addAltDecl(alt);
                err.report();
                return;
            }
        }
        obj.setScopePos(pos);
    }
    if (Ꮡid != nil) {
        check.recordDef(Ꮡid, obj);
    }
}

// pathString returns a string of the form a->b-> ... ->g for a path [a, b, ... g].
internal static @string pathString(slice<Object> path) {
    @string s = default!;
    foreach (var (i, p) in path) {
        if (i > 0) {
            s += "->"u8;
        }
        s += p.Name();
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string checkingSObjPathSˢ = "-- checking %s (objPath = %s)"u8;

// objDecl type-checks the declaration of obj in its respective (file) environment.
internal static void objDecl(this ж<Checker> Ꮡcheck, Object obj) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();

        if (tracePos) {
            check.pushPos(((atPos)obj.Pos()));
            defer(() => {
                // If we're panicking, keep stack of source positions.
                {
                    var p = recover(); if (p != default!) {
                        throw panic(p);
                    }
                }
                Ꮡcheck.Value.popPos();
            }, ref ᒐ);
        }

        if ((~check.conf)._Trace && obj.Type() == default!) {
            if (check.indent == 0) {
                fmt.Println(); // empty line between top-level objects for readability
            }
            Ꮡcheck.trace(obj.Pos(), checkingSObjPathSˢ, obj, pathString(check.objPath));
            check.indent++;
            defer(() => {
                Ꮡcheck.Value.indent--;
                Ꮡcheck.trace(obj.Pos(), "=> %s"u8, obj);
            }, ref ᒐ);
        }

        // Checking the declaration of an object means determining its type
        // (and also its value for constants). An object (and thus its type)
        // may be in 1 of 3 states:
        //
        // - not in Checker.objPathIdx and type == nil : type is not yet known (white)
        // -     in Checker.objPathIdx                 : type is pending       (grey)
        // - not in Checker.objPathIdx and type != nil : type is known         (black)
        //
        // During type-checking, an object changes from white to grey to black.
        // Predeclared objects start as black (their type is known without checking).
        //
        // A black object may only depend on (refer to) to other black objects. White
        // and grey objects may depend on white or black objects. A dependency on a
        // grey object indicates a (possibly invalid) cycle.
        //
        // When an object is marked grey, it is pushed onto the object path (a stack)
        // and its index in the path is recorded in the path index map. It is popped
        // and removed from the map when its type is determined (and marked black).

        // If this object is grey, we have a (possibly invalid) cycle. This is signaled
        // by a non-nil type for the object, except for constants and variables whose
        // type may be non-nil (known), or nil if it depends on a not-yet known
        // initialization value.
        //
        // In the former case, set the type to Typ[Invalid] because we have an
        // initialization cycle. The cycle error will be reported later, when
        // determining initialization order.
        //
        // TODO(gri) Report cycle here and simplify initialization order code.
        {
            var (_, okΔ1) = check.objPathIdx[obj, ꟷ]; if (okΔ1) {
                switch (obj.type()) {
                case ж<Const> _:
                case ж<Var> _: {
                    var objΔ1 = obj;
                    if (!Ꮡcheck.validCycle(objΔ1) || objΔ1.Type() == default!) {
                        objΔ1.setType(new BasicжΔType(Typ[Invalid]));
                    }
                    break;
                }
                case ж<TypeName> objΔ1: {
                    if (!Ꮡcheck.validCycle(new TypeNameжObject(objΔ1))) {
                        objΔ1.of(TypeName.Ꮡobject).setType(new BasicжΔType(Typ[Invalid]));
                    }
                    break;
                }
                case ж<Func> objΔ1: {
                    if (!Ꮡcheck.validCycle(new FuncжObject(objΔ1))) {
                        // Don't set type to Typ[Invalid]; plenty of code asserts that
                        // functions have a *Signature type. Instead, leave the type
                        // as an empty signature, which makes it impossible to
                        // initialize a variable with the function.
                    }
                    break;
                }
                default: {
                    var objΔ1 = obj;
                    throw panic("unreachable");
                    break;
                }}

                assert(obj.Type() != default!);
                return;
            }
        }

        if (obj.Type() != default!) { // black, meaning it's already type-checked
            return;
        }

        // white, meaning it must be type-checked

        check.push(obj); // mark as grey
        defer(Ꮡcheck.pop, ref ᒐ);

        var (d, ok) = check.objMap[obj, ꟷ];
        assert(ok);

        // save/restore current environment and set up object environment
        defer((environment env) => {
            Ꮡcheck.Value.environment = env;
        }, Ꮡcheck.Value.environment, ref ᒐ);
        check.environment = new environment(scope: (~d).@file, version: (~d).version);

        // Const and var declarations must not have initialization
        // cycles. We track them by remembering the current declaration
        // in check.decl. Initialization expressions depending on other
        // consts, vars, or functions, add dependencies to the current
        // check.decl.
        switch (obj.type()) {
        case ж<Const> objΔ2: {
            check.decl = d; // new package-level const decl
            Ꮡcheck.constDecl(objΔ2, (~d).vtyp, (~d).init, (~d).inherited);
            break;
        }
        case ж<Var> objΔ2: {
            check.decl = d; // new package-level var decl
            Ꮡcheck.varDecl(objΔ2, (~d).lhs, (~d).vtyp, (~d).init);
            break;
        }
        case ж<TypeName> objΔ2: {
            // invalid recursive types are detected via path
            Ꮡcheck.typeDecl(objΔ2, (~d).tdecl);
            Ꮡcheck.collectMethods(objΔ2); // methods can only be added to top-level types
            break;
        }
        case ж<Func> objΔ2: {
            // functions may be recursive - no need to track dependencies
            Ꮡcheck.funcDecl(objΔ2, d);
            break;
        }
        default: {
            var objΔ2 = obj;
            throw panic("unreachable");
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string vInconsistentObjectMapˢ = "%v: inconsistent object map for %s (isPkgObj = %v, inObjMap = %v)"u8;
internal static readonly @string cycleDetectedObjPathSSˢ = "## cycle detected: objPath = %s->%s (len = %d)"u8;
internal static readonly @string cycleContainsGenericTypeˢ = "## cycle contains: generic type in a type parameter list"u8;
internal static readonly @string cycleContainsDValuesDˢ = "## cycle contains: %d values, %d type definitions"u8;
internal static readonly @string cycleIsValidˢ = "=> cycle is valid"u8;
internal static readonly @string errorCycleIsInvalidˢ = "=> error: cycle is invalid"u8;

// validCycle checks if the cycle starting with obj is valid and
// reports an error if it is not.
internal static bool /*valid*/ validCycle(this ж<Checker> Ꮡcheck, Object obj) {
    bool valid = default!;
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();

        // The object map contains the package scope objects and the non-interface methods.
        if (debug) {
            var info = check.objMap[obj];
            var inObjMap = info != nil && ((~info).fdecl == nil || (~(~info).fdecl).Recv == nil); // exclude methods
            var isPkgObj = obj.Parent() == (~check.pkg).scope;
            if (isPkgObj != inObjMap) {
                Ꮡcheck.dump(vInconsistentObjectMapˢ, obj.Pos(), obj, isPkgObj, inObjMap);
                throw panic("unreachable");
            }
        }

        // Count cycle objects.
        var (start, found) = check.objPathIdx[obj, ꟷ];
        assert(found);
        var cycle = check.objPath.slice(start);
        var tparCycle = false; // if set, the cycle is through a type parameter list
        nint nval = 0; // number of (constant or variable) values in the cycle
        nint ndef = 0; // number of type definitions in the cycle
loop:
        foreach (var (_, objΔ1) in cycle) {
            switch (objΔ1.type()) {
            case ж<Const> _:
            case ж<Var> _: {
                var objΔ2 = objΔ1;
                nval++;
                break;
            }
            case ж<TypeName> objΔ2: {
                // If we reach a generic type that is part of a cycle
                // and we are in a type parameter list, we have a cycle
                // through a type parameter list.
                if (check.inTParamList && isGeneric((~objΔ2).typ)) {
                    tparCycle = true;
                    goto break_loop;
                }

                // Determine if the type name is an alias or not. For
                // package-level objects, use the object map which
                // provides syntactic information (which doesn't rely
                // on the order in which the objects are set up). For
                // local objects, we can rely on the order, so use
                // the object's predicate.
                // TODO(gri) It would be less fragile to always access
                // the syntactic information. We should consider storing
                // this information explicitly in the object.
                bool alias = default!;
                if ((~check.conf)._EnableAlias){
                    alias = objΔ2.IsAlias();
                } else {
                    {
                        var d = check.objMap[new TypeNameжObject(objΔ2)]; if (d != nil){
                            alias = (~(~d).tdecl).Assign.IsValid(); // package-level object
                        } else {
                            alias = objΔ2.IsAlias(); // function local object
                        }
                    }
                }
                if (!alias) {
                    ndef++;
                }
                break;
            }
            case ж<Func> objΔ2: {
                // ignored for now
                break;
            }
            default: {
                var objΔ2 = objΔ1;
                throw panic("unreachable");
                break;
            }}
continue_loop:;
        }
break_loop:;

        if ((~check.conf)._Trace) {
            Ꮡcheck.trace(obj.Pos(), cycleDetectedObjPathSSˢ, pathString(cycle), obj.Name(), len(cycle));
            if (tparCycle){
                Ꮡcheck.trace(obj.Pos(), cycleContainsGenericTypeˢ);
            } else {
                Ꮡcheck.trace(obj.Pos(), cycleContainsDValuesDˢ, nval, ndef);
            }
            defer(() => {
                if (valid){
                    Ꮡcheck.trace(obj.Pos(), cycleIsValidˢ);
                } else {
                    Ꮡcheck.trace(obj.Pos(), errorCycleIsInvalidˢ);
                }
            }, ref ᒐ);
        }

        // Cycles through type parameter lists are ok (go.dev/issue/68162).
        if (tparCycle) {
            valid = true; goto ᒐdone;
        }

        // A cycle involving only constants and variables is invalid but we
        // ignore them here because they are reported via the initialization
        // cycle check.
        if (nval == len(cycle)) {
            valid = true; goto ᒐdone;
        }

        // A cycle involving only types (and possibly functions) must have at least
        // one type definition to be permitted: If there is no type definition, we
        // have a sequence of alias type names which will expand ad infinitum.
        if (nval == 0 && ndef > 0) {
            valid = true; goto ᒐdone;
        }

        Ꮡcheck.cycleError(cycle, firstInSrc(cycle));
        valid = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return valid;
}

// cycleError reports a declaration cycle starting with the object at cycle[start].
internal static void cycleError(this ж<Checker> Ꮡcheck, slice<Object> cycle, nint start) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    // name returns the (possibly qualified) object name.
    // This is needed because with generic types, cycles
    // may refer to imported types. See go.dev/issue/50788.
    // TODO(gri) This functionality is used elsewhere. Factor it out.
    @string name(Object objΔ1) => packagePrefix(objΔ1.Pkg(), new Func<ж<Package>, @string>(Ꮡcheck.qualifier)) + objΔ1.Name();

    // If obj is a type alias, mark it as valid (not broken) in order to avoid follow-on errors.
    var obj = cycle[start];
    var (tname, _) = obj._<ж<TypeName>>(ᐧ);
    if (tname != nil) {
        if ((~check.conf)._EnableAlias){
            {
                var (a, ok) = tname.of(TypeName.Ꮡobject).Type()._<ж<Alias>>(ᐧ); if (ok) {
                    a.Value.fromRHS = new BasicжΔType(Typ[Invalid]);
                }
            }
        } else {
            if (tname.IsAlias()) {
                check.validAlias(tname, new BasicжΔType(Typ[Invalid]));
            }
        }
    }

    // report a more concise error for self references
    if (len(cycle) == 1) {
        if (tname != nil){
            Ꮡcheck.errorf(new Objectᴠpositioner(obj), InvalidDeclCycle, "invalid recursive type: %s refers to itself"u8, name(obj));
        } else {
            Ꮡcheck.errorf(new Objectᴠpositioner(obj), InvalidDeclCycle, "invalid cycle in declaration: %s refers to itself"u8, name(obj));
        }
        return;
    }

    var err = Ꮡcheck.newError(InvalidDeclCycle);
    if (tname != nil){
        err.addf(new Objectᴠpositioner(obj), "invalid recursive type %s"u8, name(obj));
    } else {
        err.addf(new Objectᴠpositioner(obj), "invalid cycle in declaration of %s"u8, name(obj));
    }
    // "cycle[i] refers to cycle[j]" for (i,j) = (s,s+1), (s+1,s+2), ..., (n-1,0), (0,1), ..., (s-1,s) for len(cycle) = n, s = start.
    foreach (var (i, _) in cycle) {
        var next = cycle[(start + i + 1) % len(cycle)];
        err.addf(new Objectᴠpositioner(obj), "%s refers to %s"u8, name(obj), name(next));
        obj = next;
    }
    err.report();
}

// firstInSrc reports the index of the object with the "smallest"
// source position in path. path must not be empty.
internal static nint firstInSrc(slice<Object> path) {
    nint fst = 0;
    tokenꓸPos pos = path[0].Pos();
    foreach (var (i, t) in path[1..]) {
        if (cmpPos(t.Pos(), pos) < 0) {
            (fst, pos) = (i + 1, t.Pos());
        }
    }
    return fst;
}

partial interface decl {
    ast.Node node();
}

partial struct importDecl {
    internal ж<ast.ImportSpec> spec;
}

partial struct ΔconstDecl {
    internal ж<ast.ValueSpec> spec;
    internal nint iota;
    internal ast.Expr typ;
    internal slice<ast.Expr> init;
    internal bool inherited;
}

partial struct ΔvarDecl {
    internal ж<ast.ValueSpec> spec;
}

partial struct ΔtypeDecl {
    internal ж<ast.TypeSpec> spec;
}

partial struct ΔfuncDecl {
    internal ж<ast.FuncDecl> decl;
}

internal static ast.Node node(this importDecl d) {
    return new ast.ImportSpecжNode(d.spec);
}

internal static ast.Node node(this ΔconstDecl d) {
    return new ast.ValueSpecжNode(d.spec);
}

internal static ast.Node node(this ΔvarDecl d) {
    return new ast.ValueSpecжNode(d.spec);
}

internal static ast.Node node(this ΔtypeDecl d) {
    return new ast.TypeSpecжNode(d.spec);
}

internal static ast.Node node(this ΔfuncDecl d) {
    return new ast.FuncDeclжNode(d.decl);
}

internal static void walkDecls(this ж<Checker> Ꮡcheck, slice<ast.Decl> decls, Action<decl> f) {
    foreach (var (_, d) in decls) {
        Ꮡcheck.walkDecl(d, f);
    }
}

internal static void walkDecl(this ж<Checker> Ꮡcheck, ast.Decl d, Action<decl> f) {
    switch (d.type()) {
    case ж<ast.BadDecl> dΔ1: {
        // ignore
        break;
    }
    case ж<ast.GenDecl> dΔ1: {
        ж<ast.ValueSpec> last = default!;                          // last ValueSpec with type or init exprs seen
        foreach (var (iota, s) in (~dΔ1).Specs) {
            switch (s.type()) {
            case ж<ast.ImportSpec> sΔ1: {
                f(new importDecl(sΔ1));
                break;
            }
            case ж<ast.ValueSpec> sΔ1: {
                var exprᴛ1 = (~dΔ1).Tok;
                if (exprᴛ1 == token.CONST) {
                    // determine which initialization expressions to use
                    var inherited = true;
                    switch (ᐧ) {
                    case {} when (~sΔ1).Type != default! || len((~sΔ1).Values) > 0: {
                        last = sΔ1;
                        inherited = false;
                        break;
                    }
                    case {} when last == nil: {
                        last = @new<ast.ValueSpec>(); // make sure last exists
                        inherited = false;
                        break;
                    }}
                    Ꮡcheck.arityMatch(sΔ1, last);
                    f(new ΔconstDecl(spec: sΔ1, iota: iota, typ: (~last).Type, init: (~last).Values, inherited: inherited));
                }
                else if (exprᴛ1 == token.VAR) {
                    Ꮡcheck.arityMatch(sΔ1, nil);
                    f(new ΔvarDecl(sΔ1));
                }
                else { /* default: */
                    Ꮡcheck.errorf(new ast_ValueSpecжpositioner(sΔ1), InvalidSyntaxTree, "invalid token %s"u8, (~dΔ1).Tok);
                }
                break;
            }
            case ж<ast.TypeSpec> sΔ1: {
                f(new ΔtypeDecl(sΔ1));
                break;
            }
            default: {
                var sΔ1 = s;
                Ꮡcheck.errorf(new ast_Specᴠpositioner(sΔ1), InvalidSyntaxTree, "unknown ast.Spec node %T"u8, sΔ1);
                break;
            }}
        }
        break;
    }
    case ж<ast.FuncDecl> dΔ1: {
        f(new ΔfuncDecl(dΔ1));
        break;
    }
    default: {
        var dΔ1 = d;
        Ꮡcheck.errorf(new ast_Declᴠpositioner(dΔ1), InvalidSyntaxTree, "unknown ast.Decl node %T"u8, dΔ1);
        break;
    }}
}

internal static void constDecl(this ж<Checker> Ꮡcheck, ж<Const> Ꮡobj, ast.Expr typ, ast.Expr init, bool inherited) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();
        ref var obj = ref Ꮡobj.DerefOrNull();

        assert(obj.typ == default!);

        // use the correct value of iota
        defer((constant.Value iota, positioner errpos) => {
            Ꮡcheck.Value.iota = iota;
            Ꮡcheck.Value.errpos = errpos;
        }, Ꮡcheck.Value.iota, Ꮡcheck.Value.errpos, ref ᒐ);
        check.iota = obj.val;
        check.errpos = default!;

        // provide valid constant value under all circumstances
        obj.val = constant.MakeUnknown();

        // determine type, if any
        if (typ != default!) {
            var t = Ꮡcheck.typ(typ);
            if (!isConstType(t)) {
                // don't report an error if the type is an invalid C (defined) type
                // (go.dev/issue/22090)
                if (isValid(t.Underlying())) {
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner(typ), InvalidConstType, "invalid constant type %s"u8, t);
                }
                obj.typ = new BasicжΔType(Typ[Invalid]);
                return;
            }
            obj.typ = t;
        }

        // check initialization
        ref var x = ref heap(new operand(), out var Ꮡx);
        if (init != default!) {
            if (inherited) {
                // The initialization expression is inherited from a previous
                // constant declaration, and (error) positions refer to that
                // expression and not the current constant declaration. Use
                // the constant identifier position for any errors during
                // init expression evaluation since that is all we have
                // (see issues go.dev/issue/42991, go.dev/issue/42992).
                check.errpos = ((atPos)obj.pos);
            }
            Ꮡcheck.expr(nil, Ꮡx, init);
        }
        Ꮡcheck.initConst(Ꮡobj, Ꮡx);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string variableDeclarationˢ = "variable declaration"u8;

internal static void varDecl(this ж<Checker> Ꮡcheck, ж<Var> Ꮡobj, slice<ж<Var>> lhs, ast.Expr typ, ast.Expr init) {
    ref var obj = ref Ꮡobj.DerefOrNull();

    assert(obj.typ == default!);

    // determine type, if any
    if (typ != default!) {
        obj.typ = Ꮡcheck.varType(typ);
        // We cannot spread the type to all lhs variables if there
        // are more than one since that would mark them as checked
        // (see Checker.objDecl) and the assignment of init exprs,
        // if any, would not be checked.
        //
        // TODO(gri) If we have no init expr, we should distribute
        // a given type otherwise we need to re-evaluate the type
        // expr for each lhs variable, leading to duplicate work.
    }

    // check initialization
    if (init == default!) {
        if (typ == default!) {
            // error reported before by arityMatch
            obj.typ = new BasicжΔType(Typ[Invalid]);
        }
        return;
    }

    if (lhs == default! || len(lhs) == 1) {
        assert(lhs == default! || lhs[0] == Ꮡobj);
        ref var x = ref heap(new operand(), out var Ꮡx);
        Ꮡcheck.expr(newTarget(obj.typ, obj.name), Ꮡx, init);
        Ꮡcheck.initVar(Ꮡobj, Ꮡx, variableDeclarationˢ);
        return;
    }

    if (debug) {
        // obj must be one of lhs
        if (!slices.Contains(lhs, Ꮡobj)) {
            throw panic("inconsistent lhs");
        }
    }

    // We have multiple variables on the lhs and one init expr.
    // Make sure all variables have been given the same type if
    // one was specified, otherwise they assume the type of the
    // init expression values (was go.dev/issue/15755).
    if (typ != default!) {
        foreach (var (_, lhsΔ1) in lhs) {
            lhsΔ1.Value.typ = obj.typ;
        }
    }

    Ꮡcheck.initVars(lhs, new ast.Expr[]{init}.slice(), default!);
}

// isImportedConstraint reports whether typ is an imported type constraint.
internal static bool isImportedConstraint(this ref Checker check, ΔType typ) {
    var named = asNamed(typ);
    if (named == nil || (~(~named).obj).pkg == check.pkg || (~(~named).obj).pkg == nil) {
        return false;
    }
    var (u, _) = named.Underlying()._<ж<Interface>>(ᐧ);
    return u != nil && !u.IsMethodSet();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typeParametersˢ = "type parameters"u8;
internal static readonly @string cannotUseTypeParameterˢ = "cannot use type parameter declared in alias declaration as RHS"u8;
internal static readonly @string genericTypeAliasRequiresˢ = "generic type alias requires GODEBUG=gotypesalias=1 or unset"u8;
internal static readonly @string cannotUseATypeParameterˢ = "cannot use a type parameter as RHS in type declaration"u8;

internal static void typeDecl(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj, ж<ast.TypeSpec> Ꮡtdecl) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();
        ref var obj = ref Ꮡobj.DerefOrNull();
        ref var tdecl = ref Ꮡtdecl.DerefOrNull();

        assert(obj.typ == default!);

        // Only report a version error if we have not reported one already.
        var versionErr = false;

        ref var rhs = ref heap<ΔType>(out var Ꮡrhs);
        check.later(() => {
            {
                var t = asNamed(Ꮡobj.Value.typ); if (t != nil) { // type may be invalid
                    Ꮡcheck.validType(t);
                }
            }
            // If typ is local, an error was already reported where typ is specified/defined.
            _ = !versionErr && Ꮡcheck.Value.isImportedConstraint(Ꮡrhs.ValueSlot) && Ꮡcheck.verifyVersionf(new ast_Exprᴠpositioner(Ꮡtdecl.Value.Type), go1_18, "using type constraint %s"u8, Ꮡrhs.ValueSlot);
        }).describef(new TypeNameжpositioner(Ꮡobj), "validType(%s)"u8, Ꮡobj.of(TypeName.Ꮡobject).Name());

        // First type parameter, or nil.
        ж<ast.Field> tparam0 = default!;
        if (tdecl.TypeParams.NumFields() > 0) {
            tparam0 = (~tdecl.TypeParams).List[0];
        }

        // alias declaration
        if (tdecl.Assign.IsValid()) {
            // Report highest version requirement first so that fixing a version issue
            // avoids possibly two -lang changes (first to Go 1.9 and then to Go 1.23).
            if (!versionErr && tparam0 != nil && !Ꮡcheck.verifyVersionf(new ast_Fieldжpositioner(tparam0), go1_23, "generic type alias"u8)) {
                versionErr = true;
            }
            if (!versionErr && !Ꮡcheck.verifyVersionf(((atPos)tdecl.Assign), go1_9, "type alias"u8)) {
                versionErr = true;
            }

            if ((~check.conf)._EnableAlias){
                var alias = Ꮡcheck.newAlias(Ꮡobj, default!);

                // If we could not type the RHS, set it to invalid. This should
                // only ever happen if we panic before setting.
                var aliasʗ1 = alias;
                defer(() => {
                    if ((~aliasʗ1).fromRHS == default!) {
                        aliasʗ1.Value.fromRHS = new BasicжΔType(Typ[Invalid]);
                        unalias(aliasʗ1);
                    }
                }, ref ᒐ);

                // handle type parameters even if not allowed (Alias type is supported)
                if (tparam0 != nil) {
                    check.openScope(new ast.TypeSpecжNode(Ꮡtdecl), typeParametersˢ);
                    defer(Ꮡcheck.closeScope, ref ᒐ);
                    Ꮡcheck.collectTypeParams(alias.of(Alias.Ꮡtparams), tdecl.TypeParams);
                }

                rhs = Ꮡcheck.declaredType(tdecl.Type, Ꮡobj);
                assert(rhs != default!);
                alias.Value.fromRHS = rhs;

                // spec: In an alias declaration the given type cannot be a type parameter declared in the same declaration."
                // (see also go.dev/issue/75884, go.dev/issue/#75885)
                {
                    var (tpar, ok) = rhs._<ж<TypeParam>>(ᐧ); if (ok && (~alias).tparams != nil && slices.Index((~alias).tparams.list(), tpar) >= 0) {
                        Ꮡcheck.error(new ast_Exprᴠpositioner(tdecl.Type), MisplacedTypeParam, cannotUseTypeParameterˢ);
                        alias.Value.fromRHS = new BasicжΔType(Typ[Invalid]);
                    }
                }
            } else {
                // With Go1.23, the default behavior is to use Alias nodes,
                // reflected by check.enableAlias. Signal non-default behavior.
                //
                // TODO(gri) Testing runs tests in both modes. Do we need to exclude
                //           tracking of non-default behavior for tests?
                gotypesalias.IncNonDefault();

                if (!versionErr && tparam0 != nil) {
                    Ꮡcheck.error(new ast_TypeSpecжpositioner(Ꮡtdecl), UnsupportedFeature, genericTypeAliasRequiresˢ);
                    versionErr = true;
                }

                check.brokenAlias(Ꮡobj);
                rhs = Ꮡcheck.typ(tdecl.Type);
                check.validAlias(Ꮡobj, rhs);
            }
            return;
        }

        // type definition or generic type declaration
        if (!versionErr && tparam0 != nil && !Ꮡcheck.verifyVersionf(new ast_Fieldжpositioner(tparam0), go1_18, "type parameter"u8)) {
            versionErr = true;
        }

        var named = Ꮡcheck.newNamed(Ꮡobj, default!, default!);

        // The RHS of a named N can be nil if, for example, N is defined as a cycle of aliases with
        // gotypesalias=0. Consider:
        //
        //   type D N    // N.unpack() will panic
        //   type N A
        //   type A = N  // N.fromRHS is not set before N.unpack(), since A does not call setDefType
        //
        // There is likely a better way to detect such cases, but it may not be worth the effort.
        // Instead, we briefly permit a nil N.fromRHS while type-checking D.
        named.Value.allowNilRHS = true;
        var namedʗ1 = named;
        defer(() => {
            namedʗ1.Value.allowNilRHS = false;
        }, ref ᒐ);

        if (tdecl.TypeParams != nil) {
            check.openScope(new ast.TypeSpecжNode(Ꮡtdecl), typeParametersˢ);
            defer(Ꮡcheck.closeScope, ref ᒐ);
            Ꮡcheck.collectTypeParams(named.of(Named.Ꮡtparams), tdecl.TypeParams);
        }

        rhs = Ꮡcheck.declaredType(tdecl.Type, Ꮡobj);
        assert(rhs != default!);
        named.Value.fromRHS = rhs;

        // spec: "In a type definition the given type cannot be a type parameter."
        // (See also go.dev/issue/45639.)
        if (isTypeParam(rhs)) {
            Ꮡcheck.error(new ast_Exprᴠpositioner(tdecl.Type), MisplacedTypeParam, cannotUseATypeParameterˢ);
            named.Value.fromRHS = new BasicжΔType(Typ[Invalid]);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotUseATypeParameterˢ2 = "cannot use a type parameter as constraint"u8;

internal static void collectTypeParams(this ж<Checker> Ꮡcheck, ж<ж<TypeParamList>> Ꮡdst, ж<ast.FieldList> Ꮡlist) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();
        ref var dst = ref Ꮡdst.DerefOrNull();
        ref var list = ref Ꮡlist.DerefOrNull();

        slice<ж<TypeParam>> tparams = default!;
        // Declare type parameters up-front, with empty interface as type bound.
        // The scope of type parameters starts at the beginning of the type parameter
        // list (so we can have mutually recursive parameterized interfaces).
        tokenꓸPos scopePos = list.Pos();
        foreach (var (_, f) in list.List) {
            foreach (var (_, name) in (~f).Names) {
                tparams = append(tparams, Ꮡcheck.declareTypeParam(name, scopePos));
            }
        }

        // Set the type parameters before collecting the type constraints because
        // the parameterized type may be used by the constraints (go.dev/issue/47887).
        // Example: type T[P T[P]] interface{}
        dst = bindTParams(tparams);

        // Signal to cycle detection that we are in a type parameter list.
        // We can only be inside one type parameter list at any given time:
        // function closures may appear inside a type parameter list but they
        // cannot be generic, and their bodies are processed in delayed and
        // sequential fashion. Note that with each new declaration, we save
        // the existing environment and restore it when done; thus inTPList is
        // true exactly only when we are in a specific type parameter list.
        assert(!check.inTParamList);
        check.inTParamList = true;
        defer(() => {
            Ꮡcheck.Value.inTParamList = false;
        }, ref ᒐ);

        nint index = 0;
        foreach (var (_, f) in list.List) {
            ΔType bound = default!;
            // NOTE: we may be able to assert that f.Type != nil here, but this is not
            // an invariant of the AST, so we are cautious.
            if ((~f).Type != default!){
                bound = Ꮡcheck.bound((~f).Type);
                if (isTypeParam(bound)) {
                    // We may be able to allow this since it is now well-defined what
                    // the underlying type and thus type set of a type parameter is.
                    // But we may need some additional form of cycle detection within
                    // type parameter lists.
                    Ꮡcheck.error(new ast_Exprᴠpositioner((~f).Type), MisplacedTypeParam, cannotUseATypeParameterˢ2);
                    bound = new BasicжΔType(Typ[Invalid]);
                }
            } else {
                bound = new BasicжΔType(Typ[Invalid]);
            }
            foreach (var (i, _) in (~f).Names) {
                tparams[index + i].Value.bound = bound;
            }
            index += len((~f).Names);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ΔType bound(this ж<Checker> Ꮡcheck, ast.Expr x) {
    // A type set literal of the form ~T and A|B may only appear as constraint;
    // embed it in an implicit interface so that only interface type-checking
    // needs to take care of such type expressions.
    var wrap = false;
    switch (x.type()) {
    case ж<ast.UnaryExpr> op: {
        wrap = (~op).Op == token.TILDE;
        break;
    }
    case ж<ast.BinaryExpr> op: {
        wrap = (~op).Op == token.OR;
        break;
    }}
    if (wrap) {
        x = new ast.InterfaceTypeжExpr(Ꮡ(new ast.InterfaceType(Methods: Ꮡ(new ast.FieldList(List: new ж<ast.Field>[]{Ꮡ(new ast.Field(Type: x))}.slice())))));
        var t = Ꮡcheck.typ(x);
        // mark t as implicit interface if all went well
        {
            var (tΔ1, _) = t._<ж<Interface>>(ᐧ); if (tΔ1 != nil) {
                tΔ1.Value.@implicit = true;
            }
        }
        return t;
    }
    return Ꮡcheck.typ(x);
}

internal static ж<TypeParam> declareTypeParam(this ж<Checker> Ꮡcheck, ж<ast.Ident> Ꮡname, tokenꓸPos scopePos) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var name = ref Ꮡname.DerefOrNull();

    // Use Typ[Invalid] for the type constraint to ensure that a type
    // is present even if the actual constraint has not been assigned
    // yet.
    // TODO(gri) Need to systematically review all uses of type parameter
    //           constraints to make sure we don't rely on them if they
    //           are not properly set yet.
    var tname = NewTypeName(name.Pos(), check.pkg, name.Name, default!);
    var tpar = Ꮡcheck.newTypeParam(tname, new BasicжΔType(Typ[Invalid])); // assigns type to tname as a side-effect
    Ꮡcheck.declare(check.scope, Ꮡname, new TypeNameжObject(tname), scopePos);
    return tpar;
}

internal static void collectMethods(this ж<Checker> Ꮡcheck, ж<TypeName> Ꮡobj) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var obj = ref Ꮡobj.DerefOrNull();

    // get associated methods
    // (Checker.collectObjects only collects methods with non-blank names;
    // Checker.resolveBaseTypeName ensures that obj is not an alias name
    // if it has attached methods.)
    var methods = check.methods[Ꮡobj];
    if (methods == default!) {
        return;
    }
    delete(check.methods, Ꮡobj);
    assert(!(~(~check.objMap[new TypeNameжObject(Ꮡobj)]).tdecl).Assign.IsValid()); // don't use TypeName.IsAlias (requires fully set up object)

    // use an objset to check for name conflicts
    objset mset = default!;

    // spec: "If the base type is a struct type, the non-blank method
    // and field names must be distinct."
    var @base = asNamed(obj.typ); // shouldn't fail but be conservative
    if (@base != nil) {
        assert(@base.TypeArgs().Len() == 0); // collectMethods should not be called on an instantiated type

        // See go.dev/issue/52529: we must delay the expansion of underlying here, as
        // base may not be fully set-up.
        var baseʗ1 = @base;
        check.later(() => {
            Ꮡcheck.checkFieldUniqueness(baseʗ1);
        }).describef(new TypeNameжpositioner(Ꮡobj), "verifying field uniqueness for %v"u8, @base.OrTypedNil());

        // Checker.Files may be called multiple times; additional package files
        // may add methods to already type-checked types. Add pre-existing methods
        // so that we can detect redeclarations.
        for (nint i = 0; i < @base.NumMethods(); i++) {
            var m = @base.Method(i);
            assert((~m).name != "_"u8);
            assert(mset.insert(new FuncжObject(m)) == default!);
        }
    }

    // add valid methods
    foreach (var (_, m) in methods) {
        // spec: "For a base type, the non-blank names of methods bound
        // to it must be unique."
        assert((~m).name != "_"u8);
        {
            var alt = mset.insert(new FuncжObject(m)); if (alt != default!) {
                if (alt.Pos().IsValid()){
                    Ꮡcheck.errorf(new Funcжpositioner(m), DuplicateMethod, "method %s.%s already declared at %v"u8, Ꮡobj.of(TypeName.Ꮡobject).Name(), (~m).name, alt.Pos());
                } else {
                    Ꮡcheck.errorf(new Funcжpositioner(m), DuplicateMethod, "method %s.%s already declared"u8, Ꮡobj.of(TypeName.Ꮡobject).Name(), (~m).name);
                }
                continue;
            }
        }

        if (@base != nil) {
            @base.AddMethod(m);
        }
    }
}

internal static void checkFieldUniqueness(this ж<Checker> Ꮡcheck, ж<Named> Ꮡbase) {
    {
        var (t, _) = Ꮡbase.Underlying()._<ж<Struct>>(ᐧ); if (t != nil) {
            objset mset = default!;
            for (nint i = 0; i < Ꮡbase.NumMethods(); i++) {
                var m = Ꮡbase.Method(i);
                assert((~m).name != "_"u8);
                assert(mset.insert(new FuncжObject(m)) == default!);
            }

            // Check that any non-blank field names of base are distinct from its
            // method names.
            foreach (var (_, fld) in (~t).fields) {
                if ((~fld).name != "_"u8) {
                    {
                        var alt = mset.insert(new VarжObject(fld)); if (alt != default!) {
                            // Struct fields should already be unique, so we should only
                            // encounter an alternate via collision with a method name.
                            _ = alt._<ж<Func>>();

                            // For historical consistency, we report the primary error on the
                            // method, and the alt decl on the field.
                            var err = Ꮡcheck.newError(DuplicateFieldAndMethod);
                            err.addf(new Objectᴠpositioner(alt), "field and method with the same name %s"u8, (~fld).name);
                            err.addAltDecl(new VarжObject(fld));
                            err.report();
                        }
                    }
                }
            }
        }
    }
}

internal static void funcDecl(this ж<Checker> Ꮡcheck, ж<Func> Ꮡobj, ж<declInfo> Ꮡdecl) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var obj = ref Ꮡobj.DerefOrNull();
    ref var decl = ref Ꮡdecl.DerefOrNull();

    assert(obj.typ == default!);

    // func declarations cannot use iota
    assert(check.iota == default!);

    var sig = @new<ΔSignature>();
    obj.typ = new ΔSignatureжΔType(sig); // guard against cycles

    var fdecl = decl.fdecl;
    Ꮡcheck.funcType(sig, (~fdecl).Recv, (~fdecl).Type);

    // Set the scope's extent to the complete "func (...) { ... }"
    // so that Scope.Innermost works correctly.
    sig.Value.scope.Value.pos = fdecl.Pos();
    sig.Value.scope.Value.end = fdecl.End();

    if ((~(~fdecl).Type).TypeParams.NumFields() > 0 && (~fdecl).Body == nil) {
        Ꮡcheck.softErrorf(new ast_Identжpositioner((~fdecl).Name), BadDecl, "generic function is missing function body"u8);
    }

    // function body must be type-checked after global declarations
    // (functions implemented elsewhere have no body)
    if (!(~check.conf).IgnoreFuncBodies && (~fdecl).Body != nil) {
        var fdeclʗ1 = fdecl;
        var sigʗ1 = sig;
        check.later(() => {
            Ꮡcheck.funcBody(Ꮡdecl, Ꮡobj.Value.name, sigʗ1, (~fdeclʗ1).Body, default!);
        }).describef(new Funcжpositioner(Ꮡobj), "func %s"u8, obj.name);
    }
}

internal static void declStmt(this ж<Checker> Ꮡcheck, ast.Decl d) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    var pkg = check.pkg;

    var pkgʗ1 = pkg;
    Ꮡcheck.walkDecl(d, (decl dΔ1) => {
        switch (dΔ1.type()) {
        case ΔconstDecl dΔ2: {
            nint top = len(Ꮡcheck.Value.delayed);

            // declare all constants
            var lhs = new slice<ж<Const>>(len((~dΔ2.spec).Names));
            foreach (var (i, name) in (~dΔ2.spec).Names) {
                var obj = NewConst(name.Pos(), pkgʗ1, (~name).Name, default!, constant.MakeInt64((int64)dΔ2.iota));
                lhs[i] = obj;

                ast.Expr init = default!;
                if (i < len(dΔ2.init)) {
                    init = dΔ2.init[i];
                }

                Ꮡcheck.constDecl(obj, dΔ2.typ, init, dΔ2.inherited);
            }

            // process function literals in init expressions before scope changes
            Ꮡcheck.processDelayed(top);

            // spec: "The scope of a constant or variable identifier declared
            // inside a function begins at the end of the ConstSpec or VarSpec
            // (ShortVarDecl for short variable declarations) and ends at the
            // end of the innermost containing block."
            tokenꓸPos scopePos = dΔ2.spec.End();
            foreach (var (i, name) in (~dΔ2.spec).Names) {
                Ꮡcheck.declare(Ꮡcheck.Value.scope, name, new ConstжObject(lhs[i]), scopePos);
            }
            break;
        }
        case ΔvarDecl dΔ2: {
            nint top = len(Ꮡcheck.Value.delayed);

            var lhs0 = new slice<ж<Var>>(len((~dΔ2.spec).Names));
            foreach (var (i, name) in (~dΔ2.spec).Names) {
                lhs0[i] = newVar(LocalVar, name.Pos(), pkgʗ1, (~name).Name, default!);
            }

            // initialize all variables
            foreach (var (i, obj) in lhs0) {
                slice<ж<Var>> lhs = default!;
                ast.Expr init = default!;
                var exprᴛ1 = len((~dΔ2.spec).Values);
                if (exprᴛ1 == len((~dΔ2.spec).Names)) {
                    // lhs and rhs match
                    init = (~dΔ2.spec).Values[i];
                }
                else if (exprᴛ1 is 1) {
                    // rhs is expected to be a multi-valued expression
                    lhs = lhs0;
                    init = (~dΔ2.spec).Values[0];
                }
                else { /* default: */
                    if (i < len((~dΔ2.spec).Values)) {
                        init = (~dΔ2.spec).Values[i];
                    }
                }
                Ꮡcheck.varDecl(obj, lhs, (~dΔ2.spec).Type, init);
                if (len((~dΔ2.spec).Values) == 1) {
                    // If we have a single lhs variable we are done either way.
                    // If we have a single rhs expression, it must be a multi-
                    // valued expression, in which case handling the first lhs
                    // variable will cause all lhs variables to have a type
                    // assigned, and we are done as well.
                    if (debug) {
                        foreach (var (_, objΔ1) in lhs0) {
                            assert((~objΔ1).typ != default!);
                        }
                    }
                    break;
                }
            }

            // process function literals in init expressions before scope changes
            Ꮡcheck.processDelayed(top);

            // declare all variables
            // (only at this point are the variable scopes (parents) set)
            tokenꓸPos scopePos = dΔ2.spec.End(); // see constant declarations
            foreach (var (i, name) in (~dΔ2.spec).Names) {
                // see constant declarations
                Ꮡcheck.declare(Ꮡcheck.Value.scope, name, new VarжObject(lhs0[i]), scopePos);
            }
            break;
        }
        case ΔtypeDecl dΔ2: {
            var obj = NewTypeName((~dΔ2.spec).Name.Pos(), pkgʗ1, (~(~dΔ2.spec).Name).Name, default!);
            // spec: "The scope of a type identifier declared inside a function
            // begins at the identifier in the TypeSpec and ends at the end of
            // the innermost containing block."
            tokenꓸPos scopePos = (~dΔ2.spec).Name.Pos();
            Ꮡcheck.declare(Ꮡcheck.Value.scope, (~dΔ2.spec).Name, new TypeNameжObject(obj), scopePos);
            Ꮡcheck.Value.push(new TypeNameжObject(obj)); // mark as grey
            Ꮡcheck.typeDecl(obj, dΔ2.spec);
            Ꮡcheck.Value.pop();
            break;
        }
        default: {
            var dΔ2 = dΔ1;
            Ꮡcheck.errorf(new ast_Nodeᴠpositioner(dΔ2.node()), InvalidSyntaxTree, "unknown ast.Decl node %T"u8, dΔ2.node());
            break;
        }}
    });
}

} // end types_package
