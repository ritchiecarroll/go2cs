// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements typechecking of statements.
global using rangeStmt_Expr = go.go.ast_package.Expr;
global using rangeStmt_identType = go.go.ast_package.Ident;

namespace go.go;

using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using buildcfg = global::go.@internal.buildcfg_package;
using static global::go.@internal.types.errors_package;
using sort = sort_package;
using errors = global::go.@internal.types.errors_package;
using global::go.@internal;
using global::go.go;

partial class types_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingReturnˢ = "missing return"u8;

internal static void funcBody(this ж<Checker> Ꮡcheck, ж<declInfo> Ꮡdecl, @string name, ж<ΔSignature> Ꮡsig, ж<ast.BlockStmt> Ꮡbody, constant.Value iota) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();
        ref var sig = ref Ꮡsig.DerefOrNull();
        ref var body = ref Ꮡbody.DerefOrNull();

        if ((~check.conf).IgnoreFuncBodies) {
            throw panic("function body not ignored");
        }
        if ((~check.conf)._Trace) {
            Ꮡcheck.trace(body.Pos(), "-- %s: %s"u8, name, Ꮡsig.OrTypedNil());
        }
        // save/restore current environment and set up function environment
        // (and use 0 indentation at function start)
        defer((environment env, nint indent) => {
            Ꮡcheck.Value.environment = env;
            Ꮡcheck.Value.indent = indent;
        }, Ꮡcheck.Value.environment, Ꮡcheck.Value.indent, ref ᒐ);
        check.environment = new environment(
            decl: Ꮡdecl,
            scope: sig.scope,
            iota: iota,
            sig: Ꮡsig
        );
        check.indent = 0;
        Ꮡcheck.stmtList(0, body.List);
        if (check.hasLabel) {
            Ꮡcheck.labels(Ꮡbody);
        }
        if (sig.results.Len() > 0 && !check.isTerminating(new ast.BlockStmtжStmt(Ꮡbody), ""u8)) {
            Ꮡcheck.error(((atPos)body.Rbrace), MissingReturn, missingReturnˢ);
        }
        // spec: "Implementation restriction: A compiler may make it illegal to
        // declare a variable inside a function body if the variable is never used."
        Ꮡcheck.usage(sig.scope);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void usage(this ж<Checker> Ꮡcheck, ж<ΔScope> Ꮡscope) {
    ref var scope = ref Ꮡscope.DerefOrNull();

    slice<ж<Var>> unused = default!;
    foreach (var (name, vᴛ1) in scope.elems) {
        var elem = vᴛ1;

        elem = resolve(name, elem);
        {
            var (v, _) = elem._<ж<Var>>(ᐧ); if (v != nil && !(~v).used) {
                unused = append(unused, v);
            }
        }
    }
    var unusedʗ1 = unused;
    sort.Slice(unused, (nint i, nint j) => cmpPos((~unusedʗ1[i]).pos, (~unusedʗ1[j]).pos) < 0);
    foreach (var (_, v) in unused) {
        Ꮡcheck.softErrorf(new Varжpositioner(v), UnusedVar, "declared and not used: %s"u8, (~v).name);
    }
    foreach (var (_, scopeΔ1) in scope.children) {
        // Don't go inside function literal scopes a second time;
        // they are handled explicitly by funcBody.
        if (!(~scopeΔ1).isFunc) {
            Ꮡcheck.usage(scopeΔ1);
        }
    }
}

[GoType("num:nuint")] partial struct stmtContext;

internal static stmtContext breakOk => /* 1 << iota */ 1;
internal static stmtContext continueOk => 2;
internal static stmtContext fallthroughOk => 4;
internal static stmtContext finalSwitchCase => 8;
internal static stmtContext inTypeSwitch => 16;

internal static void simpleStmt(this ж<Checker> Ꮡcheck, ast.Stmt s) {
    if (s != default!) {
        Ꮡcheck.stmt(0, s);
    }
}

internal static slice<ast.Stmt> trimTrailingEmptyStmts(slice<ast.Stmt> list) {
    for (nint i = len(list); i > 0; i--) {
        {
            var (_, ok) = list[i - 1]._<ж<ast.EmptyStmt>>(ᐧ); if (!ok) {
                return list[..(int)(i)];
            }
        }
    }
    return default!;
}

internal static void stmtList(this ж<Checker> Ꮡcheck, stmtContext ctxt, slice<ast.Stmt> list) {
    var ok = (stmtContext)(ctxt & fallthroughOk) != 0;
    stmtContext inner = (stmtContext)(ctxt & ~fallthroughOk);
    list = trimTrailingEmptyStmts(list); // trailing empty statements are "invisible" to fallthrough analysis
    foreach (var (i, s) in list) {
        stmtContext innerΔ1 = inner;
        if (ok && i + 1 == len(list)) {
            innerΔ1 |= (stmtContext)(fallthroughOk);
        }
        Ꮡcheck.stmt(innerΔ1, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string caseCommunicationClauseˢ = "case/communication clause expected"u8;

internal static void multipleDefaults(this ж<Checker> Ꮡcheck, slice<ast.Stmt> list) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    ast.Stmt first = default!;
    foreach (var (_, s) in list) {
        ast.Stmt d = default!;
        switch (s.type()) {
        case ж<ast.CaseClause> c: {
            if (len((~c).List) == 0) {
                d = s;
            }
            break;
        }
        case ж<ast.CommClause> c: {
            if ((~c).Comm == default!) {
                d = s;
            }
            break;
        }
        default: {
            var c = s;
            Ꮡcheck.error(new ast_Stmtᴠpositioner(s), InvalidSyntaxTree, caseCommunicationClauseˢ);
            break;
        }}
        if (d != default!) {
            if (first != default!){
                Ꮡcheck.errorf(new ast_Stmtᴠpositioner(d), DuplicateDefault, "multiple defaults (first at %s)"u8, check.fset.Position(first.Pos()));
            } else {
                first = d;
            }
        }
    }
}

[GoRecv] internal static void openScope(this ref Checker check, ast.Node node, @string comment) {
    var scope = NewScope(check.scope, node.Pos(), node.End(), comment);
    check.recordScope(node, scope);
    check.scope = scope;
}

[GoRecv] internal static void closeScope(this ref Checker check) {
    check.scope = check.scope.Parent();
}

internal static token.Token assignOp(token.Token op) {
    // token_test.go verifies the token ordering this function relies on
    if (token.ADD_ASSIGN <= op && op <= token.AND_NOT_ASSIGN) {
        return op + (token.ADD - token.ADD_ASSIGN);
    }
    return token.ILLEGAL;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string requiresFunctionCallNotˢ = "requires function call, not conversion"u8;
internal static readonly @string discardsResultOfˢ = "discards result of"u8;

internal static void suspendedCall(this ж<Checker> Ꮡcheck, @string keyword, ж<ast.CallExpr> Ꮡcall) {
    ref var x = ref heap(new operand(), out var Ꮡx);
    @string msg = default!;
    errors.Code code = default!;
    var exprᴛ1 = Ꮡcheck.rawExpr(nil, Ꮡx, new ast.CallExprжExpr(Ꮡcall), default!, false);
    if (exprᴛ1 == Δconversion) {
        msg = requiresFunctionCallNotˢ;
        code = InvalidDefer;
        if (keyword == "go"u8) {
            code = InvalidGo;
        }
    }
    else if (exprᴛ1 == expression) {
        msg = discardsResultOfˢ;
        code = UnusedResults;
    }
    else if (exprᴛ1 == statement) {
        return;
    }
    else { /* default: */
        throw panic("unreachable");
    }

    Ꮡcheck.errorf(new operandжpositioner(Ꮡx), code, "%s %s %s"u8, keyword, msg, Ꮡx);
}

// goVal returns the Go value for val, or nil.
internal static any goVal(constant.Value val) {
    // val should exist, but be conservative and check
    if (val == default!) {
        return default!;
    }
    // Match implementation restriction of other compilers.
    // gc only checks duplicates for integer, floating-point
    // and string values, so only create Go values for these
    // types.
    var exprᴛ1 = val.Kind();
    if (exprᴛ1 == constant.Int) {
        {
            var (x, ok) = constant.Int64Val(val); if (ok) {
                return x;
            }
        }
        {
            var (x, ok) = constant.Uint64Val(val); if (ok) {
                return x;
            }
        }
    }
    else if (exprᴛ1 == constant.Float) {
        {
            var (x, ok) = constant.Float64Val(val); if (ok) {
                return x;
            }
        }
    }
    else if (exprᴛ1 == constant.ΔString) {
        return constant.StringVal(val);
    }

    return default!;
}

[GoType("map[any, slice<valueType>]")] partial struct valueMap;

// A valueMap maps a case value (of a basic Go type) to a list of positions
// where the same case value appeared, together with the corresponding case
// types.
// Since two case values may have the same "underlying" value but different
// types we need to also check the value's types (e.g., byte(1) vs myByte(1))
// when the switch expression is of interface type.
[GoType] partial struct valueType {
    internal tokenꓸPos pos;
    internal ΔType typ;
}

internal static void caseValues(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, slice<ast.Expr> values, valueMap seen) {
    ref var x = ref Ꮡx.DerefOrNull();

L:
    foreach (var (_, e) in values) {
        ref var v = ref heap(new operand(), out var Ꮡv);
        Ꮡcheck.expr(nil, Ꮡv, e);
        if (x.mode == invalid || v.mode == invalid) {
            goto continue_L;
        }
        Ꮡcheck.convertUntyped(Ꮡv, x.typ);
        if (v.mode == invalid) {
            goto continue_L;
        }
        // Order matters: By comparing v against x, error positions are at the case values.
        ref var res = ref heap<operand>(out var Ꮡres);
        res = v; // keep original v unchanged
        Ꮡcheck.comparison(Ꮡres, Ꮡx, token.EQL, true);
        if (res.mode == invalid) {
            goto continue_L;
        }
        if (v.mode != constant_) {
            goto continue_L; // we're done
        }
        // look for duplicate values
        {
            var val = goVal(v.val); if (val != default!) {
                // look for duplicate types for a given value
                // (quadratic algorithm, but these lists tend to be very short)
                foreach (var (_, vt) in seen[val]) {
                    if (Identical(v.typ, vt.typ)) {
                        var err = Ꮡcheck.newError(DuplicateCase);
                        err.addf(new operandжpositioner(Ꮡv), "duplicate case %s in expression switch"u8, Ꮡv);
                        err.addf(((atPos)vt.pos), "previous case"u8);
                        err.report();
                        goto continue_L;
                    }
                }
                seen[val] = append(seen[val], new valueType(v.Pos(), v.typ));
            }
        }
continue_L:;
    }
break_L:;
}

// isNil reports whether the expression e denotes the predeclared value nil.
[GoRecv] internal static bool isNil(this ref Checker check, ast.Expr e) {
    // The only way to express the nil value is by literally writing nil (possibly in parentheses).
    {
        var (name, _) = ast.Unparen(e)._<ж<ast.Ident>>(ᐧ); if (name != nil) {
            var (_, ok) = check.environment.lookup((~name).Name)._<ж<Nil>>(ᐧ);
            return ok;
        }
    }
    return false;
}

// caseTypes typechecks the type expressions of a type case, checks for duplicate types
// using the seen map, and verifies that each type is valid with respect to the type of
// the operand x in the type switch clause. If the type switch expression is invalid, x
// must be nil. The result is the type of the last type expression; it is nil if the
// expression denotes the predeclared nil.
internal static ΔType /*T*/ caseTypes(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, slice<ast.Expr> types, map<ΔType, ast.Expr> seen) {
    ΔType T = default!;

    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var dummy = ref heap(new operand(), out var Ꮡdummy);
L:
    foreach (var (_, e) in types) {
        // The spec allows the value nil instead of a type.
        if (check.isNil(e)){
            T = default!;
            Ꮡcheck.expr(nil, Ꮡdummy, e); // run e through expr so we get the usual Info recordings
        } else {
            T = Ꮡcheck.varType(e);
            if (!isValid(T)) {
                goto continue_L;
            }
        }
        // look for duplicate types
        // (quadratic algorithm, but type switches tend to be reasonably small)
        foreach (var (t, other) in seen) {
            if (T == default! && t == default! || T != default! && t != default! && Identical(T, t)) {
                // talk about "case" rather than "type" because of nil case
                @string Ts = nilˢ2;
                if (T != default!) {
                    Ts = TypeString(T, new Func<ж<Package>, @string>(Ꮡcheck.qualifier));
                }
                var err = Ꮡcheck.newError(DuplicateCase);
                err.addf(new ast_Exprᴠpositioner(e), "duplicate case %s in type switch"u8, Ts);
                err.addf(new ast_Exprᴠpositioner(other), "previous case"u8);
                err.report();
                goto continue_L;
            }
        }
        seen[T] = e;
        if (Ꮡx != nil && T != default!) {
            Ꮡcheck.typeAssertion(e, Ꮡx, T, true);
        }
continue_L:;
    }
break_L:;
    return T;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string isNotUsedˢ = "is not used"u8;
internal static readonly @string mustBeCalledˢ = "must be called"u8;
internal static readonly @string isNotAnExpressionˢ = "is not an expression"u8;
internal static readonly @string sendˢ = "send"u8;
internal static readonly @string missingLhsInAssignmentˢ = "missing lhs in assignment"u8;
internal static readonly @string deferˢ = "defer"u8;
internal static readonly @string breakNotInForSwitchOrˢ = "break not in for, switch, or select statement"u8;
internal static readonly @string continueNotInForˢ = "continue not in for statement"u8;
internal static readonly @string cannotFallthroughFinalˢ = "cannot fallthrough final case in switch"u8;
internal static readonly @string cannotFallthroughInTypeˢ = "cannot fallthrough in type switch"u8;
internal static readonly @string fallthroughStatementOutˢ = "fallthrough statement out of place"u8;
internal static readonly @string blockˢ = "block"u8;
internal static readonly @string nonBooleanConditionInIfˢ = "non-boolean condition in if statement"u8;
internal static readonly @string invalidElseBranchInIfˢ = "invalid else branch in if statement"u8;
internal static readonly @string switchˢ = "switch"u8;
internal static readonly @string switchExpressionˢ = "switch expression"u8;
internal static readonly @string incorrectExpressionˢ = "incorrect expression switch case"u8;
internal static readonly @string caseˢ = "case"u8;
internal static readonly @string typeSwitchˢ = "type switch"u8;
internal static readonly @string incorrectFormOfTypeˢ = "incorrect form of type switch guard"u8;
internal static readonly @string incorrectTypeSwitchCaseˢ = "incorrect type switch case"u8;
internal static readonly @string selectCaseMustBeSendOrˢ = "select case must be send or receive (possibly with assignment)"u8;
internal static readonly @string forˢ = "for"u8;
internal static readonly @string nonBooleanConditionInForˢ = "non-boolean condition in for statement"u8;
internal static readonly @string invalidStatementˢ = "invalid statement"u8;

// TODO(gri) Once we are certain that typeHash is correct in all situations, use this version of caseTypes instead.
// (Currently it may be possible that different types have identical names and import paths due to ImporterFrom.)
//
// func (check *Checker) caseTypes(x *operand, xtyp *Interface, types []ast.Expr, seen map[string]ast.Expr) (T Type) {
// 	var dummy operand
// L:
// 	for _, e := range types {
// 		// The spec allows the value nil instead of a type.
// 		var hash string
// 		if check.isNil(e) {
// 			check.expr(nil, &dummy, e) // run e through expr so we get the usual Info recordings
// 			T = nil
// 			hash = "<nil>" // avoid collision with a type named nil
// 		} else {
// 			T = check.varType(e)
// 			if !isValid(T) {
// 				continue L
// 			}
// 			hash = typeHash(T, nil)
// 		}
// 		// look for duplicate types
// 		if other := seen[hash]; other != nil {
// 			// talk about "case" rather than "type" because of nil case
// 			Ts := "nil"
// 			if T != nil {
// 				Ts = TypeString(T, check.qualifier)
// 			}
// 			err := check.newError(_DuplicateCase)
// 			err.addf(e, "duplicate case %s in type switch", Ts)
// 			err.addf(other, "previous case")
// 			err.report()
// 			continue L
// 		}
// 		seen[hash] = e
// 		if T != nil {
// 			check.typeAssertion(e.Pos(), x, xtyp, T)
// 		}
// 	}
// 	return
// }

// stmt typechecks statement s.
internal static void stmt(this ж<Checker> Ꮡcheck, stmtContext ctxt, ast.Stmt s) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();

        // statements must end with the same top scope as they started with
        if (debug) {
            defer((ж<ΔScope> scope) => {
                // don't check if code is panicking
                {
                    var p = recover(); if (p != default!) {
                        throw panic(p);
                    }
                }
                assert(scope == Ꮡcheck.Value.scope);
            }, Ꮡcheck.Value.scope, ref ᒐ);
        }
        // process collected function literals before scope changes
        defer(Ꮡcheck.processDelayed, len(Ꮡcheck.Value.delayed), ref ᒐ);
        // reset context for statements of inner blocks
        stmtContext inner = (stmtContext)(ctxt & ~((stmtContext)((stmtContext)(fallthroughOk | finalSwitchCase) | inTypeSwitch)));
        switch (s.type()) {
        case ж<ast.BadStmt> _:
        case ж<ast.EmptyStmt> _: {
            var sΔ1 = s;
            break;
        }
        case ж<ast.DeclStmt> sΔ1: {
            Ꮡcheck.declStmt((~sΔ1).Decl);
            break;
        }
        case ж<ast.LabeledStmt> sΔ1: {
            check.hasLabel = true;
            Ꮡcheck.stmt(ctxt, // ignore
 (~sΔ1).Stmt);
            break;
        }
        case ж<ast.ExprStmt> sΔ1: {
            // spec: "With the exception of specific built-in functions,
            // function and method calls and receive operations can appear
            // in statement context. Such statements may be parenthesized."
            ref var x = ref heap(new operand(), out var Ꮡx);
            exprKind kind = Ꮡcheck.rawExpr(nil, Ꮡx, (~sΔ1).X, default!, false);
            @string msg = default!;
            errors.Code code = default!;
            var exprᴛ1 = x.mode;
            if (exprᴛ1 == Δbuiltinᴛ) {
                msg = mustBeCalledˢ;
                code = UncalledBuiltin;
            }
            else if (exprᴛ1 == typexpr) {
                msg = isNotAnExpressionˢ;
                code = NotAnExpr;
            }
            else { /* default: */
                if (kind == statement) {
                    return;
                }
                msg = isNotUsedˢ;
                code = UnusedExpr;
            }

            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), code, "%s %s"u8, Ꮡx, msg);
            break;
        }
        case ж<ast.SendStmt> sΔ1: {
            ref var ch = ref heap(new operand(), out var Ꮡch);
            ref var val = ref heap(new operand(), out var Ꮡval);
            Ꮡcheck.expr(nil, Ꮡch, (~sΔ1).Chan);
            Ꮡcheck.expr(nil, Ꮡval, (~sΔ1).Value);
            if (ch.mode == invalid || val.mode == invalid) {
                return;
            }
            var u = coreType(ch.typ);
            if (u == default!) {
                Ꮡcheck.errorf(inNode(new ast.SendStmtжNode(sΔ1), (~sΔ1).Arrow), InvalidSend, invalidOp + "cannot send to %s: no core type", Ꮡch);
                return;
            }
            var (uch, _) = u._<ж<Chan>>(ᐧ);
            if (uch == nil) {
                Ꮡcheck.errorf(inNode(new ast.SendStmtжNode(sΔ1), (~sΔ1).Arrow), InvalidSend, invalidOp + "cannot send to non-channel %s", Ꮡch);
                return;
            }
            if ((~uch).dir == RecvOnly) {
                Ꮡcheck.errorf(inNode(new ast.SendStmtжNode(sΔ1), (~sΔ1).Arrow), InvalidSend, invalidOp + "cannot send to receive-only channel %s", Ꮡch);
                return;
            }
            Ꮡcheck.assignment(Ꮡval, (~uch).elem, sendˢ);
            break;
        }
        case ж<ast.IncDecStmt> sΔ1: {
            token.Token op = default!;
            var exprᴛ2 = (~sΔ1).Tok;
            if (exprᴛ2 == token.INC) {
                op = token.ADD;
            }
            else if (exprᴛ2 == token.DEC) {
                op = token.SUB;
            }
            else { /* default: */
                Ꮡcheck.errorf(inNode(new ast.IncDecStmtжNode(sΔ1), (~sΔ1).TokPos), InvalidSyntaxTree, "unknown inc/dec operation %s"u8, (~sΔ1).Tok);
                return;
            }

            ref var x = ref heap(new operand(), out var Ꮡx);
            Ꮡcheck.expr(nil, Ꮡx, (~sΔ1).X);
            if (x.mode == invalid) {
                return;
            }
            if (!allNumeric(x.typ)) {
                Ꮡcheck.errorf(new ast_Exprᴠpositioner((~sΔ1).X), NonNumericIncDec, invalidOp + "%s%s (non-numeric type %s)", (~sΔ1).X, (~sΔ1).Tok, x.typ);
                return;
            }
            var Y = Ꮡ(new ast.BasicLit(ValuePos: (~sΔ1).X.Pos(), Kind: token.INT, Value: "1"u8)); // use x's position
            Ꮡcheck.binary(Ꮡx, default!, (~sΔ1).X, new ast.BasicLitжExpr(Y), op, (~sΔ1).TokPos);
            if (x.mode == invalid) {
                return;
            }
            Ꮡcheck.assignVar((~sΔ1).X, default!, Ꮡx, assignmentˢ);
            break;
        }
        case ж<ast.AssignStmt> sΔ1: {
            var exprᴛ3 = (~sΔ1).Tok;
            if (exprᴛ3 == token.ASSIGN || exprᴛ3 == token.DEFINE) {
                if (len((~sΔ1).Lhs) == 0) {
                    Ꮡcheck.error(new ast_AssignStmtжpositioner(sΔ1), InvalidSyntaxTree, missingLhsInAssignmentˢ);
                    return;
                }
                if ((~sΔ1).Tok == token.DEFINE){
                    Ꮡcheck.shortVarDecl(inNode(new ast.AssignStmtжNode(sΔ1), (~sΔ1).TokPos), (~sΔ1).Lhs, (~sΔ1).Rhs);
                } else {
                    // regular assignment
                    Ꮡcheck.assignVars((~sΔ1).Lhs, (~sΔ1).Rhs);
                }
            }
            else { /* default: */
                if (len((~sΔ1).Lhs) != 1 || len((~sΔ1).Rhs) != 1) {
                    // assignment operations
                    Ꮡcheck.errorf(inNode(new ast.AssignStmtжNode(sΔ1), (~sΔ1).TokPos), MultiValAssignOp, "assignment operation %s requires single-valued expressions"u8, (~sΔ1).Tok);
                    return;
                }
                token.Token op = assignOp((~sΔ1).Tok);
                if (op == token.ILLEGAL) {
                    Ꮡcheck.errorf(((atPos)(~sΔ1).TokPos), InvalidSyntaxTree, "unknown assignment operation %s"u8, (~sΔ1).Tok);
                    return;
                }
                ref var x = ref heap(new operand(), out var Ꮡx);
                Ꮡcheck.binary(Ꮡx, default!, (~sΔ1).Lhs[0], (~sΔ1).Rhs[0], op, (~sΔ1).TokPos);
                if (x.mode == invalid) {
                    return;
                }
                Ꮡcheck.assignVar((~sΔ1).Lhs[0], default!, Ꮡx, assignmentˢ);
            }

            break;
        }
        case ж<ast.GoStmt> sΔ1: {
            Ꮡcheck.suspendedCall("go"u8, (~sΔ1).Call);
            break;
        }
        case ж<ast.DeferStmt> sΔ1: {
            Ꮡcheck.suspendedCall(deferˢ, (~sΔ1).Call);
            break;
        }
        case ж<ast.ReturnStmt> sΔ1: {
            var res = check.sig.Value.results;
            if (len((~sΔ1).Results) == 0 && res.Len() > 0 && (~(~res).vars[0]).name != ""u8){
                // Return with implicit results allowed for function with named results.
                // (If one is named, all are named.)
                // spec: "Implementation restriction: A compiler may disallow an empty expression
                // list in a "return" statement if a different entity (constant, type, or variable)
                // with the same name as a result parameter is in scope at the place of the return."
                foreach (var (_, obj) in (~res).vars) {
                    {
                        var alt = Ꮡcheck.of(Checker.Ꮡenvironment).lookup((~obj).name); if (alt != default! && !AreEqual(alt, obj)) {
                            var err = Ꮡcheck.newError(OutOfScopeResult);
                            err.addf(new ast_ReturnStmtжpositioner(sΔ1), "result parameter %s not in scope at return"u8, (~obj).name);
                            err.addf(new Objectᴠpositioner(alt), "inner declaration of %s"u8, obj.OrTypedNil());
                            err.report();
                        }
                    }
                }
            } else {
                // ok to continue
                slice<ж<Var>> lhs = default!;
                if (res.Len() > 0) {
                    lhs = res.Value.vars;
                }
                Ꮡcheck.initVars(lhs, (~sΔ1).Results, new ast.ReturnStmtжStmt(sΔ1));
            }
            break;
        }
        case ж<ast.BranchStmt> sΔ1: {
            if ((~sΔ1).Label != nil) {
                check.hasLabel = true;
                return; // checked in 2nd pass (check.labels)
            }
            var exprᴛ4 = (~sΔ1).Tok;
            if (exprᴛ4 == token.BREAK) {
                if ((stmtContext)(ctxt & breakOk) == 0) {
                    Ꮡcheck.error(new ast_BranchStmtжpositioner(sΔ1), MisplacedBreak, breakNotInForSwitchOrˢ);
                }
            }
            else if (exprᴛ4 == token.CONTINUE) {
                if ((stmtContext)(ctxt & continueOk) == 0) {
                    Ꮡcheck.error(new ast_BranchStmtжpositioner(sΔ1), MisplacedContinue, continueNotInForˢ);
                }
            }
            else if (exprᴛ4 == token.FALLTHROUGH) {
                if ((stmtContext)(ctxt & fallthroughOk) == 0) {
                    @string msg = default!;
                    switch (ᐧ) {
                    case {} when (stmtContext)(ctxt & finalSwitchCase) != 0: {
                        msg = cannotFallthroughFinalˢ;
                        break;
                    }
                    case {} when (stmtContext)(ctxt & inTypeSwitch) != 0: {
                        msg = cannotFallthroughInTypeˢ;
                        break;
                    }
                    default: {
                        msg = fallthroughStatementOutˢ;
                        break;
                    }}

                    Ꮡcheck.error(new ast_BranchStmtжpositioner(sΔ1), MisplacedFallthrough, msg);
                }
            }
            else { /* default: */
                Ꮡcheck.errorf(new ast_BranchStmtжpositioner(sΔ1), InvalidSyntaxTree, "branch statement: %s"u8, (~sΔ1).Tok);
            }

            break;
        }
        case ж<ast.BlockStmt> sΔ1: {
            check.openScope(new ast.BlockStmtжNode(sΔ1), blockˢ);
            defer(Ꮡcheck.closeScope, ref ᒐ);
            Ꮡcheck.stmtList(inner, (~sΔ1).List);
            break;
        }
        case ж<ast.IfStmt> sΔ1: {
            check.openScope(new ast.IfStmtжNode(sΔ1), "if"u8);
            defer(Ꮡcheck.closeScope, ref ᒐ);
            Ꮡcheck.simpleStmt((~sΔ1).Init);
            ref var x = ref heap(new operand(), out var Ꮡx);
            Ꮡcheck.expr(nil, Ꮡx, (~sΔ1).Cond);
            if (x.mode != invalid && !allBoolean(x.typ)) {
                Ꮡcheck.error(new ast_Exprᴠpositioner((~sΔ1).Cond), InvalidCond, nonBooleanConditionInIfˢ);
            }
            Ꮡcheck.stmt(inner, new ast.BlockStmtжStmt((~sΔ1).Body));
            switch ((~sΔ1).Else.type()) {
            case null:
            case ж<ast.BadStmt> _: {
                break;
            }
            case ж<ast.IfStmt> _:
            case ж<ast.BlockStmt> _: {
                Ꮡcheck.stmt(inner, // The parser produces a correct AST but if it was modified
 // elsewhere the else branch may be invalid. Check again.
 // valid or error already reported
 (~sΔ1).Else);
                break;
            }
            default: {
                Ꮡcheck.error(new ast_Stmtᴠpositioner((~sΔ1).Else), InvalidSyntaxTree, invalidElseBranchInIfˢ);
                break;
            }}

            break;
        }
        case ж<ast.SwitchStmt> sΔ1: {
            inner |= (stmtContext)(breakOk);
            check.openScope(new ast.SwitchStmtжNode(sΔ1), switchˢ);
            defer(Ꮡcheck.closeScope, ref ᒐ);
            Ꮡcheck.simpleStmt((~sΔ1).Init);
            ref var x = ref heap(new operand(), out var Ꮡx);
            if ((~sΔ1).Tag != default!){
                Ꮡcheck.expr(nil, Ꮡx, (~sΔ1).Tag);
                // By checking assignment of x to an invisible temporary
                // (as a compiler would), we get all the relevant checks.
                Ꮡcheck.assignment(Ꮡx, default!, switchExpressionˢ);
                if (x.mode != invalid && !Comparable(x.typ) && !hasNil(x.typ)) {
                    Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidExprSwitch, "cannot switch on %s (%s is not comparable)"u8, Ꮡx, x.typ);
                    x.mode = invalid;
                }
            } else {
                // spec: "A missing switch expression is
                // equivalent to the boolean value true."
                x.mode = constant_;
                x.typ = new BasicжΔType(Typ[Bool]);
                x.val = constant.MakeBool(true);
                x.expr = new ast.IdentжExpr(Ꮡ(new ast.Ident(NamePos: (~(~sΔ1).Body).Lbrace, Name: "true"u8)));
            }
            Ꮡcheck.multipleDefaults((~(~sΔ1).Body).List);
            var seen = new valueMap(0); // map of seen case values to positions and types
            foreach (var (i, c) in (~(~sΔ1).Body).List) {
                var (clause, _) = c._<ж<ast.CaseClause>>(ᐧ);
                if (clause == nil) {
                    Ꮡcheck.error(new ast_Stmtᴠpositioner(c), InvalidSyntaxTree, incorrectExpressionˢ);
                    continue;
                }
                Ꮡcheck.caseValues(Ꮡx, (~clause).List, seen);
                check.openScope(new ast.CaseClauseжNode(clause), caseˢ);
                stmtContext innerΔ1 = inner;
                if (i + 1 < len((~(~sΔ1).Body).List)){
                    innerΔ1 |= (stmtContext)(fallthroughOk);
                } else {
                    innerΔ1 |= (stmtContext)(finalSwitchCase);
                }
                Ꮡcheck.stmtList(innerΔ1, (~clause).Body);
                check.closeScope();
            }
            break;
        }
        case ж<ast.TypeSwitchStmt> sΔ1: {
            inner |= (stmtContext)((stmtContext)(breakOk | inTypeSwitch));
            check.openScope(new ast.TypeSwitchStmtжNode(sΔ1), typeSwitchˢ);
            defer(Ꮡcheck.closeScope, ref ᒐ);
            Ꮡcheck.simpleStmt((~sΔ1).Init);
            // A type switch guard must be of the form:
            //
            //     TypeSwitchGuard = [ identifier ":=" ] PrimaryExpr "." "(" "type" ")" .
            //
            // The parser is checking syntactic correctness;
            // remaining syntactic errors are considered AST errors here.
            // TODO(gri) better factoring of error handling (invalid ASTs)
            //
            ж<ast.Ident> lhs = default!;                   // lhs identifier or nil
            ast.Expr rhs = default!;
            switch ((~sΔ1).Assign.type()) {
            case ж<ast.ExprStmt> guard: {
                rhs = guard.Value.X;
                break;
            }
            case ж<ast.AssignStmt> guard: {
                if (len((~guard).Lhs) != 1 || (~guard).Tok != token.DEFINE || len((~guard).Rhs) != 1) {
                    Ꮡcheck.error(new ast_TypeSwitchStmtжpositioner(sΔ1), InvalidSyntaxTree, incorrectFormOfTypeˢ);
                    return;
                }
                (lhs, _) = (~guard).Lhs[0]._<ж<ast.Ident>>(ᐧ);
                if (lhs == nil) {
                    Ꮡcheck.error(new ast_TypeSwitchStmtжpositioner(sΔ1), InvalidSyntaxTree, incorrectFormOfTypeˢ);
                    return;
                }
                if ((~lhs).Name == "_"u8){
                    // _ := x.(type) is an invalid short variable declaration
                    Ꮡcheck.softErrorf(new ast_Identжpositioner(lhs), NoNewVar, "no new variable on left side of :="u8);
                    lhs = default!; // avoid declared and not used error below
                } else {
                    check.recordDef(lhs, default!); // lhs variable is implicitly declared in each cause clause
                }
                rhs = (~guard).Rhs[0];
                break;
            }
            default: {
                var guard = (~sΔ1).Assign;
                Ꮡcheck.error(new ast_TypeSwitchStmtжpositioner(sΔ1), InvalidSyntaxTree, incorrectFormOfTypeˢ);
                return;
            }}
            var (expr, _) = rhs._<ж<ast.TypeAssertExpr>>(ᐧ);
            if (expr == nil || (~expr).Type != default!) {
                // rhs must be of the form: expr.(type) and expr must be an ordinary interface
                Ꮡcheck.error(new ast_TypeSwitchStmtжpositioner(sΔ1), InvalidSyntaxTree, incorrectFormOfTypeˢ);
                return;
            }
            ж<operand> sx = default!;                // switch expression against which cases are compared against; nil if invalid
            {
                ref var x = ref heap(new operand(), out var Ꮡx);
                Ꮡcheck.expr(nil, Ꮡx, (~expr).X);
                if (x.mode != invalid) {
                    if (isTypeParam(x.typ)){
                        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidTypeSwitch, "cannot use type switch on type parameter value %s"u8, Ꮡx);
                    } else 
                    if (IsInterface(x.typ)){
                        sx = Ꮡx;
                    } else {
                        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidTypeSwitch, "%s is not an interface"u8, Ꮡx);
                    }
                }
            }
            Ꮡcheck.multipleDefaults((~(~sΔ1).Body).List);
            slice<ж<Var>> lhsVars = default!;                    // list of implicitly declared lhs variables
            var seen = new map<ΔType, ast.Expr>(); // map of seen types to positions
            foreach (var (_, sΔ2) in (~(~sΔ1).Body).List) {
                var (clause, _) = sΔ2._<ж<ast.CaseClause>>(ᐧ);
                if (clause == nil) {
                    Ꮡcheck.error(new ast_Stmtᴠpositioner(sΔ2), InvalidSyntaxTree, incorrectTypeSwitchCaseˢ);
                    continue;
                }
                // Check each type in this type switch case.
                var T = Ꮡcheck.caseTypes(sx, (~clause).List, seen);
                check.openScope(new ast.CaseClauseжNode(clause), caseˢ);
                // If lhs exists, declare a corresponding variable in the case-local scope.
                if (lhs != nil) {
                    // spec: "The TypeSwitchGuard may include a short variable declaration.
                    // When that form is used, the variable is declared at the beginning of
                    // the implicit block in each clause. In clauses with a case listing
                    // exactly one type, the variable has that type; otherwise, the variable
                    // has the type of the expression in the TypeSwitchGuard."
                    if (len((~clause).List) != 1 || T == default!) {
                        T = new BasicжΔType(Typ[Invalid]);
                        if (sx != nil) {
                            T = sx.Value.typ;
                        }
                    }
                    var obj = NewVar(lhs.Pos(), check.pkg, (~lhs).Name, T);
                    tokenꓸPos scopePos = clause.Pos() + ((tokenꓸPos)len("default")); // for default clause (len(List) == 0)
                    {
                        nint n = len((~clause).List); if (n > 0) {
                            scopePos = (~clause).List[n - 1].End();
                        }
                    }
                    Ꮡcheck.declare(check.scope, nil, new VarжObject(obj), scopePos);
                    check.recordImplicit(new ast.CaseClauseжNode(clause), new VarжObject(obj));
                    // For the "declared and not used" error, all lhs variables act as
                    // one; i.e., if any one of them is 'used', all of them are 'used'.
                    // Collect them for later analysis.
                    lhsVars = append(lhsVars, obj);
                }
                Ꮡcheck.stmtList(inner, (~clause).Body);
                check.closeScope();
            }
            if (lhs != nil) {
                // If lhs exists, we must have at least one lhs variable that was used.
                bool used = default!;
                foreach (var (_, v) in lhsVars) {
                    if ((~v).used) {
                        used = true;
                    }
                    v.Value.used = true; // avoid usage error when checking entire function
                }
                if (!used) {
                    Ꮡcheck.softErrorf(new ast_Identжpositioner(lhs), UnusedVar, "%s declared and not used"u8, (~lhs).Name);
                }
            }
            break;
        }
        case ж<ast.SelectStmt> sΔ1: {
            inner |= (stmtContext)(breakOk);
            Ꮡcheck.multipleDefaults((~(~sΔ1).Body).List);
            foreach (var (_, sΔ3) in (~(~sΔ1).Body).List) {
                var (clause, _) = sΔ3._<ж<ast.CommClause>>(ᐧ);
                if (clause == nil) {
                    continue; // error reported before
                }
                // clause.Comm must be a SendStmt, RecvStmt, or default case
                var valid = false;
                ast.Expr rhs = default!;               // rhs of RecvStmt, or nil
                switch ((~clause).Comm.type()) {
                case null:
                case ж<ast.SendStmt> _: {
                    var sΔ4 = (~clause).Comm;
                    valid = true;
                    break;
                }
                case ж<ast.AssignStmt> sΔ4: {
                    if (len((~sΔ4).Rhs) == 1) {
                        rhs = (~sΔ4).Rhs[0];
                    }
                    break;
                }
                case ж<ast.ExprStmt> sΔ4: {
                    rhs = sΔ4.Value.X;
                    break;
                }}
                // if present, rhs must be a receive operation
                if (rhs != default!) {
                    {
                        var (x, _) = ast.Unparen(rhs)._<ж<ast.UnaryExpr>>(ᐧ); if (x != nil && (~x).Op == token.ARROW) {
                            valid = true;
                        }
                    }
                }
                if (!valid) {
                    Ꮡcheck.error(new ast_Stmtᴠpositioner((~clause).Comm), InvalidSelectCase, selectCaseMustBeSendOrˢ);
                    continue;
                }
                check.openScope(sΔ3, caseˢ);
                if ((~clause).Comm != default!) {
                    Ꮡcheck.stmt(inner, (~clause).Comm);
                }
                Ꮡcheck.stmtList(inner, (~clause).Body);
                check.closeScope();
            }
            break;
        }
        case ж<ast.ForStmt> sΔ1: {
            inner |= (stmtContext)((stmtContext)(breakOk | continueOk));
            check.openScope(new ast.ForStmtжNode(sΔ1), forˢ);
            defer(Ꮡcheck.closeScope, ref ᒐ);
            Ꮡcheck.simpleStmt((~sΔ1).Init);
            if ((~sΔ1).Cond != default!) {
                ref var x = ref heap(new operand(), out var Ꮡx);
                Ꮡcheck.expr(nil, Ꮡx, (~sΔ1).Cond);
                if (x.mode != invalid && !allBoolean(x.typ)) {
                    Ꮡcheck.error(new ast_Exprᴠpositioner((~sΔ1).Cond), InvalidCond, nonBooleanConditionInForˢ);
                }
            }
            Ꮡcheck.simpleStmt((~sΔ1).Post);
            {
                var (sΔ5, _) = (~sΔ1).Post._<ж<ast.AssignStmt>>(ᐧ); if (sΔ5 != nil && (~sΔ5).Tok == token.DEFINE) {
                    // spec: "The init statement may be a short variable
                    // declaration, but the post statement must not."
                    Ꮡcheck.softErrorf(new ast_AssignStmtжpositioner(sΔ5), InvalidPostDecl, "cannot declare in post statement"u8);
                    // Don't call useLHS here because we want to use the lhs in
                    // this erroneous statement so that we don't get errors about
                    // these lhs variables being declared and not used.
                    Ꮡcheck.use((~sΔ5).Lhs.ꓸꓸꓸ); // avoid follow-up errors
                }
            }
            Ꮡcheck.stmt(inner, new ast.BlockStmtжStmt((~sΔ1).Body));
            break;
        }
        case ж<ast.RangeStmt> sΔ1: {
            inner |= (stmtContext)((stmtContext)(breakOk | continueOk));
            Ꮡcheck.rangeStmt(inner, sΔ1);
            break;
        }
        default: {
            var sΔ1 = s;
            Ꮡcheck.error(new ast_Stmtᴠpositioner(sΔ1), InvalidSyntaxTree, invalidStatementˢ);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rangeˢ = "range"u8;
internal static readonly @string rangeClauseˢ = "range clause"u8;
internal static readonly @string noNewVariablesOnLeftSideˢ = "no new variables on left side of :="u8;

internal static void rangeStmt(this ж<Checker> Ꮡcheck, stmtContext inner, ж<ast.RangeStmt> Ꮡs) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();
        ref var s = ref Ꮡs.DerefOrNull();

        @string identName(ж<rangeStmt_identType> n) => (~n).Name;
        var (sKey, sValue) = (s.Key, s.Value);
        ast.Expr sExtra = default!;         // (used only in types2 fork)
        var isDef = s.Tok == token.DEFINE;
        var rangeVar = s.X;
        var noNewVarPos = inNode(new ast.RangeStmtжNode(Ꮡs), s.TokPos);
        // Everything from here on is shared between cmd/compile/internal/types2 and go/types.
        // check expression to iterate over
        ref var x = ref heap(new operand(), out var Ꮡx);
        Ꮡcheck.expr(nil, Ꮡx, rangeVar);
        // determine key/value types
        ΔType key = default!;
        ΔType val = default!;
        if (x.mode != invalid) {
            // Ranging over a type parameter is permitted if it has a core type.
            var (k, v, cause, ok) = rangeKeyVal(x.typ, (goVersion vΔ1) => Ꮡcheck.allowVersion(new ast_Exprᴠpositioner(Ꮡx.Value.expr), vΔ1));
            switch (ᐧ) {
            case {} when !ok && cause != ""u8: {
                Ꮡcheck.softErrorf(new operandжpositioner(Ꮡx), InvalidRangeExpr, "cannot range over %s: %s"u8, Ꮡx, cause);
                break;
            }
            case {} when !ok: {
                Ꮡcheck.softErrorf(new operandжpositioner(Ꮡx), InvalidRangeExpr, "cannot range over %s"u8, Ꮡx);
                break;
            }
            case {} when k == default! && sKey != default!: {
                Ꮡcheck.softErrorf(new ast_Exprᴠpositioner(sKey), InvalidIterVar, "range over %s permits no iteration variables"u8, Ꮡx);
                break;
            }
            case {} when v == default! && sValue != default!: {
                Ꮡcheck.softErrorf(new ast_Exprᴠpositioner(sValue), InvalidIterVar, "range over %s permits only one iteration variable"u8, Ꮡx);
                break;
            }
            case {} when sExtra != default!: {
                Ꮡcheck.softErrorf(new ast_Exprᴠpositioner(sExtra), InvalidIterVar, "range clause permits at most two iteration variables"u8);
                break;
            }}

            (key, val) = (k, v);
        }
        // Open the for-statement block scope now, after the range clause.
        // Iteration variables declared with := need to go in this scope (was go.dev/issue/51437).
        check.openScope(new ast.RangeStmtжNode(Ꮡs), rangeˢ);
        defer(Ꮡcheck.closeScope, ref ᒐ);
        // check assignment to/declaration of iteration variables
        // (irregular assignment, cannot easily map to existing assignment checks)
        // lhs expressions and initialization value (rhs) types
        var lhs = new rangeStmt_Expr[]{sKey, sValue}.array(); // sKey, sValue may be nil
        var rhs = new ΔType[]{key, val}.array(); // key, val may be nil
        var rangeOverInt = isInteger(x.typ);
        if (isDef){
            // short variable declaration
            slice<ж<Var>> vars = default!;
            foreach (var (i, lhsΔ1) in lhs.ΔRangeSnapshot()) {
                if (lhsΔ1 == default!) {
                    continue;
                }
                // determine lhs variable
                ж<Var> obj = default!;
                {
                    var (ident, _) = lhsΔ1._<ж<rangeStmt_identType>>(ᐧ); if (ident != nil){
                        // declare new variable
                        @string name = identName(ident);
                        obj = NewVar(ident.Pos(), check.pkg, name, default!);
                        check.recordDef(ident, new VarжObject(obj));
                        // _ variables don't count as new variables
                        if (name != "_"u8) {
                            vars = append(vars, obj);
                        }
                    } else {
                        Ꮡcheck.errorf(new ast_Exprᴠpositioner(lhsΔ1), InvalidSyntaxTree, "cannot declare %s"u8, lhsΔ1);
                        obj = NewVar(lhsΔ1.Pos(), check.pkg, "_"u8, default!); // dummy variable
                    }
                }
                assert((~obj).typ == default!);
                // initialize lhs iteration variable, if any
                var typ = rhs[i];
                if (typ == default! || AreEqual(typ, Typ[Invalid])) {
                    // typ == Typ[Invalid] can happen if allowVersion fails.
                    obj.Value.typ = new BasicжΔType(Typ[Invalid]);
                    obj.Value.used = true; // don't complain about unused variable
                    continue;
                }
                if (rangeOverInt){
                    assert(i == 0); // at most one iteration variable (rhs[1] == nil or Typ[Invalid] for rangeOverInt)
                    Ꮡcheck.initVar(obj, Ꮡx, rangeClauseˢ);
                } else {
                    ref var y = ref heap(new operand(), out var Ꮡy);
                    y.mode = value;
                    y.expr = lhsΔ1; // we don't have a better rhs expression to use here
                    y.typ = typ;
                    Ꮡcheck.initVar(obj, Ꮡy, assignmentˢ); // error is on variable, use "assignment" not "range clause"
                }
                assert((~obj).typ != default!);
            }
            // declare variables
            if (len(vars) > 0){
                tokenꓸPos scopePos = s.Body.Pos();
                foreach (var (_, obj) in vars) {
                    Ꮡcheck.declare(check.scope, nil, /* recordDef already called */
 new VarжObject(obj), scopePos);
                }
            } else {
                Ꮡcheck.error(noNewVarPos, NoNewVar, noNewVariablesOnLeftSideˢ);
            }
        } else 
        if (sKey != default!){
            /* lhs[0] != nil */
            // ordinary assignment
            foreach (var (i, lhsΔ2) in lhs.ΔRangeSnapshot()) {
                if (lhsΔ2 == default!) {
                    continue;
                }
                // assign to lhs iteration variable, if any
                var typ = rhs[i];
                if (typ == default! || AreEqual(typ, Typ[Invalid])) {
                    continue;
                }
                if (rangeOverInt){
                    assert(i == 0); // at most one iteration variable (rhs[1] == nil or Typ[Invalid] for rangeOverInt)
                    Ꮡcheck.assignVar(lhsΔ2, default!, Ꮡx, rangeClauseˢ);
                    // If the assignment succeeded, if x was untyped before, it now
                    // has a type inferred via the assignment. It must be an integer.
                    // (go.dev/issues/67027)
                    if (x.mode != invalid && !isInteger(x.typ)) {
                        Ꮡcheck.softErrorf(new ast_Exprᴠpositioner(lhsΔ2), InvalidRangeExpr, "cannot use iteration variable of type %s"u8, x.typ);
                    }
                } else {
                    ref var y = ref heap(new operand(), out var Ꮡy);
                    y.mode = value;
                    y.expr = lhsΔ2; // we don't have a better rhs expression to use here
                    y.typ = typ;
                    Ꮡcheck.assignVar(lhsΔ2, default!, Ꮡy, assignmentˢ); // error is on variable, use "assignment" not "range clause"
                }
            }
        } else 
        if (rangeOverInt) {
            // If we don't have any iteration variables, we still need to
            // check that a (possibly untyped) integer range expression x
            // is valid.
            // We do this by checking the assignment _ = x. This ensures
            // that an untyped x can be converted to a value of its default
            // type (rune or int).
            Ꮡcheck.assignment(Ꮡx, default!, rangeClauseˢ);
        }
        Ꮡcheck.stmt(inner, new ast.BlockStmtжStmt(s.Body));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noCoreTypeˢ = "no core type"u8;
internal static readonly @string requiresGo122OrLaterˢ = "requires go1.22 or later"u8;
internal static readonly @string receiveFromSendOnlyˢ = "receive from send-only channel"u8;
internal static readonly @string requiresGo123OrLaterˢ = "requires go1.23 or later"u8;
internal static readonly @string funcMustBeFuncYieldFuncˢ = "func must be func(yield func(...) bool): wrong argument count"u8;
internal static readonly @string funcMustBeFuncYieldFuncˢ2 = "func must be func(yield func(...) bool): argument is not func"u8;
internal static readonly @string funcMustBeFuncYieldFuncˢ3 = "func must be func(yield func(...) bool): unexpected results"u8;
internal static readonly @string funcMustBeFuncYieldFuncˢ4 = "func must be func(yield func(...) bool): yield func has too many parameters"u8;
internal static readonly @string funcMustBeFuncYieldFuncˢ5 = "func must be func(yield func(...) bool): yield func does not return bool"u8;

// rangeKeyVal returns the key and value type produced by a range clause
// over an expression of type typ.
// If allowVersion != nil, it is used to check the required language version.
// If the range clause is not permitted, rangeKeyVal returns ok = false.
// When ok = false, rangeKeyVal may also return a reason in cause.
internal static (ΔType key, ΔType val, @string cause, bool ok) rangeKeyVal(ΔType typ, Func<goVersion, bool> allowVersion) {
    ΔType key = default!;
    ΔType val = default!;
    @string cause = default!;
    bool ok = default!;

    (ΔType, ΔType, @string, bool) bad(@string causeΔ1) => (new BasicжΔType(Typ[Invalid]), new BasicжΔType(Typ[Invalid]), causeΔ1, false);
    ж<ΔSignature> toSig(ΔType t) {
        var (sig, _) = coreType(t)._<ж<ΔSignature>>(ᐧ);
        return sig;
    }
    var orig = typ;
    switch (arrayPtrDeref(coreType(typ)).type()) {
    case null: {
        return bad(noCoreTypeˢ);
    }
    case ж<Basic> typΔ1: {
        if (isString(new BasicжΔType(typΔ1))) {
            return (new BasicжΔType(Typ[Int]), universeRune, "", true); // use 'rune' name
        }
        if (isInteger(new BasicжΔType(typΔ1))) {
            if (allowVersion != default! && !allowVersion(go1_22)) {
                return bad(requiresGo122OrLaterˢ);
            }
            return (orig, default!, "", true);
        }
        break;
    }
    case ж<Array> typΔ1: {
        return (new BasicжΔType(Typ[Int]), (~typΔ1).elem, "", true);
    }
    case ж<Slice> typΔ1: {
        return (new BasicжΔType(Typ[Int]), (~typΔ1).elem, "", true);
    }
    case ж<Map> typΔ1: {
        return ((~typΔ1).key, (~typΔ1).elem, "", true);
    }
    case ж<Chan> typΔ1: {
        if ((~typΔ1).dir == SendOnly) {
            return bad(receiveFromSendOnlyˢ);
        }
        return ((~typΔ1).elem, default!, "", true);
    }
    case ж<ΔSignature> typΔ1: {
        if (!buildcfg.Experiment.RangeFunc && allowVersion != default! && !allowVersion(go1_23)) {
            return bad(requiresGo123OrLaterˢ);
        }
        assert(typΔ1.Recv() == nil);
        switch (ᐧ) {
        case {} when typΔ1.Params().Len() is not 1: {
            return bad(funcMustBeFuncYieldFuncˢ);
        }
        case {} when toSig(typΔ1.Params().At(0).of(Var.Ꮡobject).Type()) == nil: {
            return bad(funcMustBeFuncYieldFuncˢ2);
        }
        case {} when typΔ1.Results().Len() is not 0: {
            return bad(funcMustBeFuncYieldFuncˢ3);
        }}

        var cb = toSig(typΔ1.Params().At(0).of(Var.Ꮡobject).Type());
        assert(cb.Recv() == nil);
        switch (ᐧ) {
        case {} when cb.Params().Len() is > 2: {
            return bad(funcMustBeFuncYieldFuncˢ4);
        }
        case {} when cb.Results().Len() != 1 || !isBoolean(cb.Results().At(0).of(Var.Ꮡobject).Type()): {
            return bad(funcMustBeFuncYieldFuncˢ5);
        }}

        if (cb.Params().Len() >= 1) {
            key = cb.Params().At(0).of(Var.Ꮡobject).Type();
        }
        if (cb.Params().Len() >= 2) {
            val = cb.Params().At(1).of(Var.Ꮡobject).Type();
        }
        return (key, val, "", true);
    }}
    return (key, val, cause, ok);
}

} // end types_package
