// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements typechecking of expressions.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using static @internal.types.errors_package;
using errors = @internal.types.errors_package;
using global::go.go;
using ꓸꓸꓸany = Span<any>;

partial class types_package {

partial struct opPredicates /*map[token.Token, Func<ΔType, bool>]*/;

/*
Basic algorithm:

Expressions are checked recursively, top down. Expression checker functions
are generally of the form:

  func f(x *operand, e *ast.Expr, ...)

where e is the expression to be checked, and x is the result of the check.
The check performed by f may fail in which case x.mode == invalid, and
related error messages will have been issued by f.

If a hint argument is present, it is the composite literal element type
of an outer composite literal; it is used to type-check composite literal
elements that have no explicit type specification in the source
(e.g.: []T{{...}, {...}}, the hint is the type T in this case).

All expressions are checked via rawExpr, which dispatches according
to expression kind. Upon returning, rawExpr is recording the types and
constant values for all expressions that have an untyped type (those types
may change on the way up in the expression tree). Usually these are constants,
but the results of comparisons or non-constant shifts of untyped constants
may also be untyped, but not constant.

Untyped expressions may eventually become fully typed (i.e., not untyped),
typically when the value is assigned to a variable, or is used otherwise.
The updateExprType method is used to record this final type and update
the recorded types: the type-checked expression tree is again traversed down,
and the new type is propagated as needed. Untyped constant expression values
that become fully typed must now be representable by the full type (constant
sub-expression trees are left alone except for their roots). This mechanism
ensures that a client sees the actual (run-time) type an untyped value would
have. It also permits type-checking of lhs shift operands "as if the shift
were not present": when updateExprType visits an untyped lhs shift operand
and assigns it its final type, that type must be an integer type, and a
constant lhs must be representable as an integer.

When an expression gets its final type, either on the way out from rawExpr,
on the way down in updateExprType, or at the end of the type checker run,
the type (and constant value, if any) is recorded via Info.Types, if present.
*/
internal static opPredicates unaryOpPredicates;

[GoInit] internal static void init() {
    // Setting unaryOpPredicates in init avoids declaration cycles.
    unaryOpPredicates = new opPredicates(new map<token.Token, Func<ΔType, bool>>{
        [token.ADD] = allNumeric,
        [token.SUB] = allNumeric,
        [token.XOR] = allInteger,
        [token.NOT] = allBoolean
    });
}

internal static bool op(this ж<Checker> Ꮡcheck, opPredicates m, ж<operand> Ꮡx, token.Token op) {
    ref var x = ref Ꮡx.DerefOrNull();

    {
        var pred = m[op]; if (pred != default!){
            if (!pred(x.typ)) {
                Ꮡcheck.errorf(new operandжpositioner(Ꮡx), UndefinedOp, invalidOp + "operator %s not defined on %s", op, Ꮡx.OrTypedNil());
                return false;
            }
        } else {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidSyntaxTree, "unknown operator %s"u8, op);
            return false;
        }
    }
    return true;
}

// opPos returns the position of the operator if x is an operation;
// otherwise it returns the start position of x.
internal static tokenꓸPos opPos(ast.Expr x) {
    switch (x.type()) {
    case null: {
        return nopos; // don't crash
    }
    case ж<ast.BinaryExpr> op: {
        return (~op).OpPos;
    }
    default: {
        var op = x;
        return x.Pos();
    }}
}

// opName returns the name of the operation if x is an operation
// that might overflow; otherwise it returns the empty string.
internal static @string opName(ast.Expr e) {
    switch (e.type()) {
    case ж<ast.BinaryExpr> eΔ1: {
        if ((nint)(~eΔ1).Op < len(op2str2)) {
            return op2str2[(~eΔ1).Op];
        }
        break;
    }
    case ж<ast.UnaryExpr> eΔ1: {
        if ((nint)(~eΔ1).Op < len(op2str1)) {
            return op2str1[(~eΔ1).Op];
        }
        break;
    }}
    return ""u8;
}

internal static array<@string> op2str1 = new golib.SparseArray<@string>{
    [19] = "bitwise complement"u8
}.array(20);

// This is only used for operations that may cause overflow.
internal static array<@string> op2str2 = new golib.SparseArray<@string>{
    [12] = "addition"u8,
    [13] = "subtraction"u8,
    [19] = "bitwise XOR"u8,
    [14] = "multiplication"u8,
    [20] = "shift"u8
}.array(21);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotUseOutsideOfˢ = "cannot use ~ outside of interface or type constraint"u8;
internal static readonly @string cannotUseOutsideOfˢ2 = "cannot use ~ outside of interface or type constraint (use ^ for bitwise complement)"u8;

// The unary expression e may be nil. It's passed in for better error messages only.
internal static void unary(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.UnaryExpr> Ꮡe) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    Ꮡcheck.expr(nil, Ꮡx, e.X);
    if (x.mode == invalid) {
        return;
    }
    token.Token op = e.Op;
    var exprᴛ1 = op;
    if (exprᴛ1 == token.AND) {
        {
            var (_, ok) = ast.Unparen(e.X)._<ж<ast.CompositeLit>>(ᐧ); if (!ok && x.mode != variable) {
                // spec: "As an exception to the addressability
                // requirement x may also be a composite literal."
                Ꮡcheck.errorf(new operandжpositioner(Ꮡx), UnaddressableOperand, invalidOp + "cannot take address of %s", Ꮡx.OrTypedNil());
                x.mode = invalid;
                return;
            }
        }
        x.mode = value;
        x.typ = new PointerжΔType(Ꮡ(new Pointer(@base: x.typ)));
        return;
    }
    if (exprᴛ1 == token.ARROW) {
        var u = coreType(x.typ);
        if (u == default!) {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidReceive, invalidOp + "cannot receive from %s (no core type)", Ꮡx.OrTypedNil());
            x.mode = invalid;
            return;
        }
        var (ch, _) = u._<ж<Chan>>(ᐧ);
        if (ch == nil) {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidReceive, invalidOp + "cannot receive from non-channel %s", Ꮡx.OrTypedNil());
            x.mode = invalid;
            return;
        }
        if ((~ch).dir == SendOnly) {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidReceive, invalidOp + "cannot receive from send-only channel %s", Ꮡx.OrTypedNil());
            x.mode = invalid;
            return;
        }
        x.mode = commaok;
        x.typ = ch.Value.elem;
        check.hasCallOrRecv = true;
        return;
    }
    if (exprᴛ1 == token.TILDE) {
        if (!allInteger(x.typ)) {
            // Provide a better error position and message than what check.op below would do.
            Ꮡcheck.error(new ast_UnaryExprжpositioner(Ꮡe), UndefinedOp, cannotUseOutsideOfˢ);
            x.mode = invalid;
            return;
        }
        Ꮡcheck.error(new ast_UnaryExprжpositioner(Ꮡe), UndefinedOp, cannotUseOutsideOfˢ2);
        op = token.XOR;
    }

    if (!Ꮡcheck.op(unaryOpPredicates, Ꮡx, op)) {
        x.mode = invalid;
        return;
    }
    if (x.mode == constant_) {
        if (x.val.Kind() == constant.Unknown) {
            // nothing to do (and don't cause an error below in the overflow check)
            return;
        }
        nuint prec = default!;
        if (isUnsigned(x.typ)) {
            prec = (nuint)(check.conf.@sizeof(x.typ) * 8);
        }
        x.val = constant.UnaryOp(op, x.val, prec);
        x.expr = new ast.UnaryExprжExpr(Ꮡe);
        Ꮡcheck.overflow(Ꮡx, opPos(x.expr));
        return;
    }
    x.mode = value;
}

// x.typ remains unchanged
internal static bool isShift(token.Token op) {
    return op == token.SHL || op == token.SHR;
}

internal static bool isComparison(token.Token op) {
    // Note: tokens are not ordered well to make this much easier
    var exprᴛ1 = op;
    if (exprᴛ1 == token.EQL || exprᴛ1 == token.NEQ || exprᴛ1 == token.LSS || exprᴛ1 == token.LEQ || exprᴛ1 == token.GTR || exprᴛ1 == token.GEQ) {
        return true;
    }

    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string vFoundOldTypeSSNewSˢ = "%v: found old type(%s): %s (new: %s)"u8;

// updateExprType updates the type of x to typ and invokes itself
// recursively for the operands of x, depending on expression kind.
// If typ is still an untyped and not the final type, updateExprType
// only updates the recorded untyped type for x and possibly its
// operands. Otherwise (i.e., typ is not an untyped type anymore,
// or it is the final type for x), the type and value are recorded.
// Also, if x is a constant, it must be representable as a value of typ,
// and if x is the (formerly untyped) lhs operand of a non-constant
// shift, it must be an integer value.
internal static void updateExprType(this ж<Checker> Ꮡcheck, ast.Expr x, ΔType typ, bool final) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    var (old, found) = check.untyped[x, ꟷ];
    if (!found) {
        return; // nothing to do
    }
    // update operands of x if necessary
    switch (x.type()) {
    case ж<ast.BadExpr> _:
    case ж<ast.FuncLit> _:
    case ж<ast.CompositeLit> _:
    case ж<ast.IndexExpr> _:
    case ж<ast.SliceExpr> _:
    case ж<ast.TypeAssertExpr> _:
    case ж<ast.StarExpr> _:
    case ж<ast.KeyValueExpr> _:
    case ж<ast.ArrayType> _:
    case ж<ast.StructType> _:
    case ж<ast.FuncType> _:
    case ж<ast.InterfaceType> _:
    case ж<ast.MapType> _:
    case ж<ast.ChanType> _: {
        var xΔ1 = x;
        if (debug) {
            // These expression are never untyped - nothing to do.
            // The respective sub-expressions got their final types
            // upon assignment or use.
            Ꮡcheck.dump(vFoundOldTypeSSNewSˢ, xΔ1.Pos(), xΔ1, old.typ.OrTypedNil(), typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.CallExpr> xΔ1: {
        break;
    }
    case ж<ast.Ident> _:
    case ж<ast.BasicLit> _:
    case ж<ast.SelectorExpr> _: {
        var xΔ1 = x;
        break;
    }
    case ж<ast.ParenExpr> xΔ1: {
        Ꮡcheck.updateExprType((~xΔ1).X, // Resulting in an untyped constant (e.g., built-in complex).
 // The respective calls take care of calling updateExprType
 // for the arguments if necessary.
 // An identifier denoting a constant, a constant literal,
 // or a qualified identifier (imported untyped constant).
 // No operands to take care of.
 typ, final);
        break;
    }
    case ж<ast.UnaryExpr> xΔ1: {
        if (old.val != default!) {
            // If x is a constant, the operands were constants.
            // The operands don't need to be updated since they
            // never get "materialized" into a typed value. If
            // left in the untyped map, they will be processed
            // at the end of the type check.
            break;
        }
        Ꮡcheck.updateExprType((~xΔ1).X, typ, final);
        break;
    }
    case ж<ast.BinaryExpr> xΔ1: {
        if (old.val != default!) {
            break; // see comment for unary expressions
        }
        if (isComparison((~xΔ1).Op)){
        } else 
        if (isShift((~xΔ1).Op)){
            // The result type is independent of operand types
            // and the operand types must have final types.
            // The result type depends only on lhs operand.
            // The rhs type was updated when checking the shift.
            Ꮡcheck.updateExprType((~xΔ1).X, typ, final);
        } else {
            // The operand types match the result type.
            Ꮡcheck.updateExprType((~xΔ1).X, typ, final);
            Ꮡcheck.updateExprType((~xΔ1).Y, typ, final);
        }
        break;
    }
    default: {
        var xΔ1 = x;
        throw panic("unreachable");
        break;
    }}
    // If the new type is not final and still untyped, just
    // update the recorded type.
    if (!final && isUntyped(typ)) {
        old.typ = under(typ)._<ж<Basic>>();
        check.untyped[x] = old;
        return;
    }
    // Otherwise we have the final (typed or untyped type).
    // Remove it from the map of yet untyped expressions.
    delete(check.untyped, x);
    if (old.isLhs) {
        // If x is the lhs of a shift, its final type must be integer.
        // We already know from the shift check that it is representable
        // as an integer if it is a constant.
        if (!allInteger(typ)) {
            Ꮡcheck.errorf(new ast_Exprᴠpositioner(x), InvalidShiftOperand, invalidOp + "shifted operand %s (type %s) must be integer", x, typ);
            return;
        }
    }
    // Even if we have an integer, if the value is a constant we
    // still must check that it is representable as the specific
    // int type requested (was go.dev/issue/22969). Fall through here.
    if (old.val != default!) {
        // If x is a constant, it must be representable as a value of typ.
        ref var c = ref heap<operand>(out var Ꮡc);
        c = new operand(old.mode, x, new BasicжΔType(old.typ), old.val, 0);
        Ꮡcheck.convertUntyped(Ꮡc, typ);
        if (c.mode == invalid) {
            return;
        }
    }
    // Everything's fine, record final type and value for x.
    check.recordTypeAndValue(x, old.mode, typ, old.val);
}

// updateExprVal updates the value of x to val.
internal static void updateExprVal(this ref Checker check, ast.Expr x, constant.Value val) {
    {
        var (info, ok) = check.untyped[x, ꟷ]; if (ok) {
            info.val = val;
            check.untyped[x] = info;
        }
    }
}

// implicitTypeAndValue returns the implicit type of x when used in a context
// where the target type is expected. If no such implicit conversion is
// possible, it returns a nil Type and non-zero error code.
//
// If x is a constant operand, the returned constant.Value will be the
// representation of x in this context.
internal static (ΔType, constant.Value, errors.Code) implicitTypeAndValue(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ΔType target) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.mode == invalid || isTyped(x.typ) || !isValid(target)) {
        return (x.typ, default!, 0);
    }
    // x is untyped
    if (isUntyped(target)) {
        // both x and target are untyped
        {
            var m = maxType(x.typ, target); if (m != default!) {
                return (m, default!, 0);
            }
        }
        return (default!, default!, InvalidUntypedConversion);
    }
    var switchᴛ6 = under(target);
    switch (switchᴛ6.type()) {
    case ж<Basic> u: {
        if (x.mode == constant_) {
            var (v, code) = Ꮡcheck.representation(Ꮡx, u);
            if (code != 0) {
                return (default!, default!, code);
            }
            return (target, v, code);
        }
        var exprᴛ1 = (~x.typ._<ж<Basic>>()).kind;
        if (exprᴛ1 == UntypedBool) {
            if (!isBoolean(target)) {
                // Non-constant untyped values may appear as the
                // result of comparisons (untyped bool), intermediate
                // (delayed-checked) rhs operands of shifts, and as
                // the value nil.
                return (default!, default!, InvalidUntypedConversion);
            }
        }
        else if (exprᴛ1 == ΔUntypedInt || exprᴛ1 == UntypedRune || exprᴛ1 == ΔUntypedFloat || exprᴛ1 == ΔUntypedComplex) {
            if (!isNumeric(target)) {
                return (default!, default!, InvalidUntypedConversion);
            }
        }
        else if (exprᴛ1 == UntypedString) {
            if (!isString(target)) {
                // Non-constant untyped string values are not permitted by the spec and
                // should not occur during normal typechecking passes, but this path is
                // reachable via the AssignableTo API.
                return (default!, default!, InvalidUntypedConversion);
            }
        }
        else if (exprᴛ1 == UntypedNil) {
            if (!hasNil(target)) {
                // Unsafe.Pointer is a basic type that includes nil.
                return (default!, default!, InvalidUntypedConversion);
            }
            return (new BasicжΔType(Typ[UntypedNil]), default!, 0);
        }
        else { /* default: */
            return (default!, default!, InvalidUntypedConversion);
        }

        break;
    }
    case ж<Interface> u: {
        if (isTypeParam(target)) {
            // Preserve the type of nil as UntypedNil: see go.dev/issue/13061.
            if (!underIs(target, (ΔType uΔ1) => {
                if (uΔ1 == default!) {
                    return false;
                }
                var (t, _, _) = Ꮡcheck.implicitTypeAndValue(Ꮡx, uΔ1);
                return t != default!;
            })) {
                return (default!, default!, InvalidUntypedConversion);
            }
            // keep nil untyped (was bug go.dev/issue/39755)
            if (x.isNil()) {
                return (new BasicжΔType(Typ[UntypedNil]), default!, 0);
            }
            break;
        }
        if (x.isNil()) {
            // Values must have concrete dynamic types. If the value is nil,
            // keep it untyped (this is important for tools such as go vet which
            // need the dynamic type for argument checking of say, print
            // functions)
            return (new BasicжΔType(Typ[UntypedNil]), default!, 0);
        }
        if (!u.Empty()) {
            // cannot assign untyped values to non-empty interfaces
            return (default!, default!, InvalidUntypedConversion);
        }
        return (Default(x.typ), default!, 0);
    }
    case ж<Pointer> _:
    case ж<ΔSignature> _:
    case ж<Slice> _:
    case ж<Map> _:
    case ж<Chan> _: {
        var u = switchᴛ6;
        if (!x.isNil()) {
            return (default!, default!, InvalidUntypedConversion);
        }
        return (new BasicжΔType(Typ[UntypedNil]), default!, 0);
    }
    default: {
        var u = switchᴛ6;
        return (default!, default!, InvalidUntypedConversion);
    }}
    // Keep nil untyped - see comment for interfaces, above.
    return (target, default!, 0);
}

// If switchCase is true, the operator op is ignored.
internal static void comparison(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<operand> Ꮡy, token.Token op, bool switchCase) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // Avoid spurious errors if any of the operands has an invalid type (go.dev/issue/54405).
    if (!isValid(x.typ) || !isValid(y.typ)) {
        x.mode = invalid;
        return;
    }
    if (switchCase) {
        op = token.EQL;
    }
    var errOp = Ꮡx; // operand for which error is reported, if any
    @string cause = ""u8; // specific error cause, if any
    // spec: "In any comparison, the first operand must be assignable
    // to the type of the second operand, or vice versa."
    errors.Code code = MismatchedTypes;
    var (ok, _) = Ꮡx.assignableTo(Ꮡcheck, y.typ, nil);
    if (!ok) {
        (ok, _) = Ꮡy.assignableTo(Ꮡcheck, x.typ, nil);
    }
    if (!ok) {
        // Report the error on the 2nd operand since we only
        // know after seeing the 2nd operand whether we have
        // a type mismatch.
        errOp = Ꮡy;
        cause = Ꮡcheck.sprintf("mismatched types %s and %s"u8, x.typ, y.typ);
        goto ΔError;
    }
    // check if comparison is defined for operands
    code = UndefinedOp;
    var exprᴛ1 = op;
    if (exprᴛ1 == token.EQL || exprᴛ1 == token.NEQ) {
        switch (ᐧ) {
        case {} when x.isNil() || y.isNil(): {
            var typ = x.typ;
            if (x.isNil()) {
                // spec: "The equality operators == and != apply to operands that are comparable."
                // Comparison against nil requires that the other operand type has nil.
                typ = y.typ;
            }
            if (!hasNil(typ)) {
                // This case should only be possible for "nil == nil".
                // Report the error on the 2nd operand since we only
                // know after seeing the 2nd operand whether we have
                // an invalid comparison.
                errOp = Ꮡy;
                goto ΔError;
            }
            break;
        }
        case {} when !Comparable(x.typ): {
            errOp = Ꮡx;
            cause = Ꮡcheck.incomparableCause(x.typ);
            goto ΔError;
            break;
        }
        case {} when !Comparable(y.typ): {
            errOp = Ꮡy;
            cause = Ꮡcheck.incomparableCause(y.typ);
            goto ΔError;
            break;
        }}

    }
    else if (exprᴛ1 == token.LSS || exprᴛ1 == token.LEQ || exprᴛ1 == token.GTR || exprᴛ1 == token.GEQ) {
        switch (ᐧ) {
        case {} when !allOrdered(x.typ): {
            errOp = Ꮡx;
            goto ΔError;
            break;
        }
        case {} when !allOrdered(y.typ): {
            errOp = Ꮡy;
            goto ΔError;
            break;
        }}

    }
    else { /* default: */
        throw panic("unreachable");
    }

    // spec: The ordering operators <, <=, >, and >= apply to operands that are ordered."
    // comparison is ok
    if (x.mode == constant_ && y.mode == constant_){
        x.val = constant.MakeBool(constant.Compare(x.val, op, y.val));
    } else {
        // The operands are never materialized; no need to update
        // their types.
        x.mode = value;
        // The operands have now their final types, which at run-
        // time will be materialized. Update the expression trees.
        // If the current types are untyped, the materialized type
        // is the respective default type.
        Ꮡcheck.updateExprType(x.expr, Default(x.typ), true);
        Ꮡcheck.updateExprType(y.expr, Default(y.typ), true);
    }
    // spec: "Comparison operators compare two operands and yield
    //        an untyped boolean value."
    x.typ = new BasicжΔType(Typ[UntypedBool]);
    return;
ΔError:
    if (cause == ""u8) {
        // We have an offending operand errOp and possibly an error cause.
        if (isTypeParam(x.typ) || isTypeParam(y.typ)){
            // TODO(gri) should report the specific type causing the problem, if any
            if (!isTypeParam(x.typ)) {
                errOp = Ꮡy;
            }
            cause = Ꮡcheck.sprintf("type parameter %s cannot use operator %s"u8, (~errOp).typ, op);
        } else {
            // catch-all neither x nor y is a type parameter
            @string what = compositeKind((~errOp).typ);
            if (what == ""u8) {
                what = Ꮡcheck.sprintf("%s"u8, (~errOp).typ);
            }
            cause = Ꮡcheck.sprintf("operator %s not defined on %s"u8, op, what);
        }
    }
    if (switchCase){
        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), code, "invalid case %s in switch on %s (%s)"u8, x.expr, y.expr, cause); // error position always at 1st operand
    } else {
        Ꮡcheck.errorf(new operandжpositioner(errOp), code, invalidOp + "%s %s %s (%s)", x.expr, op, y.expr, cause);
    }
    x.mode = invalid;
}

// incomparableCause returns a more specific cause why typ is not comparable.
// If there is no more specific cause, the result is "".
internal static @string incomparableCause(this ж<Checker> Ꮡcheck, ΔType typ) {
    switch (under(typ).type()) {
    case ж<Slice> _:
    case ж<ΔSignature> _:
    case ж<Map> _: {
        return compositeKind(typ) + " can only be compared to nil"u8;
    }}

    // see if we can extract a more specific error
    @string cause = default!;
    comparableType(typ, true, default!, (@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.sslice();
        cause = Ꮡcheck.sprintf(format, args.ꓸꓸꓸ);
    });
    return cause;
}

// If e != nil, it must be the shift expression; it may be nil for non-constant shifts.
internal static void shift(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<operand> Ꮡy, ast.Expr e, token.Token op) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // TODO(gri) This function seems overly complex. Revisit.
    constant.Value xval = default!;
    if (x.mode == constant_) {
        xval = constant.ToInt(x.val);
    }
    if (allInteger(x.typ) || isUntyped(x.typ) && xval != default! && xval.Kind() == constant.Int){
    } else {
        // The lhs is of integer type or an untyped constant representable
        // as an integer. Nothing to do.
        // shift has no chance
        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidShiftOperand, invalidOp + "shifted operand %s must be integer", Ꮡx.OrTypedNil());
        x.mode = invalid;
        return;
    }
    // spec: "The right operand in a shift expression must have integer type
    // or be an untyped constant representable by a value of type uint."
    // Check that constants are representable by uint, but do not convert them
    // (see also go.dev/issue/47243).
    constant.Value yval = default!;
    if (y.mode == constant_){
        // Provide a good error message for negative shift counts.
        yval = constant.ToInt(y.val); // consider -1, 1.0, but not -1.1
        if (yval.Kind() == constant.Int && constant.Sign(yval) < 0) {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡy), InvalidShiftCount, invalidOp + "negative shift count %s", Ꮡy.OrTypedNil());
            x.mode = invalid;
            return;
        }
        if (isUntyped(y.typ)) {
            // Caution: Check for representability here, rather than in the switch
            // below, because isInteger includes untyped integers (was bug go.dev/issue/43697).
            Ꮡcheck.representable(Ꮡy, Typ[Uint]);
            if (y.mode == invalid) {
                x.mode = invalid;
                return;
            }
        }
    } else {
        // Check that RHS is otherwise at least of integer type.
        switch (ᐧ) {
        case {} when allInteger(y.typ): {
            if (!allUnsigned(y.typ) && !Ꮡcheck.verifyVersionf(new operandжpositioner(Ꮡy), go1_13, invalidOp + "signed shift count %s", Ꮡy.OrTypedNil())) {
                x.mode = invalid;
                return;
            }
            break;
        }
        case {} when isUntyped(y.typ): {
            Ꮡcheck.convertUntyped(Ꮡy, // This is incorrect, but preserves pre-existing behavior.
 // See also go.dev/issue/47410.
 new BasicжΔType(Typ[Uint]));
            if (y.mode == invalid) {
                x.mode = invalid;
                return;
            }
            break;
        }
        default: {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡy), InvalidShiftCount, invalidOp + "shift count %s must be integer", Ꮡy.OrTypedNil());
            x.mode = invalid;
            return;
        }}

    }
    if (x.mode == constant_) {
        if (y.mode == constant_) {
            // if either x or y has an unknown value, the result is unknown
            if (x.val.Kind() == constant.Unknown || y.val.Kind() == constant.Unknown) {
                x.val = constant.MakeUnknown();
                // ensure the correct type - see comment below
                if (!isInteger(x.typ)) {
                    x.typ = new BasicжΔType(Typ[ΔUntypedInt]);
                }
                return;
            }
            // rhs must be within reasonable bounds in constant shifts
            const uint64 shiftBound = /* 1023 - 1 + 52 */ 1074; // so we can express smallestFloat64 (see go.dev/issue/44057)
            var (s, ok) = constant.Uint64Val(yval);
            if (!ok || s > shiftBound) {
                Ꮡcheck.errorf(new operandжpositioner(Ꮡy), InvalidShiftCount, invalidOp + "invalid shift count %s", Ꮡy.OrTypedNil());
                x.mode = invalid;
                return;
            }
            // The lhs is representable as an integer but may not be an integer
            // (e.g., 2.0, an untyped float) - this can only happen for untyped
            // non-integer numeric constants. Correct the type so that the shift
            // result is of integer type.
            if (!isInteger(x.typ)) {
                x.typ = new BasicжΔType(Typ[ΔUntypedInt]);
            }
            // x is a constant so xval != nil and it must be of Int kind.
            x.val = constant.Shift(xval, op, (nuint)s);
            x.expr = e;
            Ꮡcheck.overflow(Ꮡx, opPos(x.expr));
            return;
        }
        // non-constant shift with constant lhs
        if (isUntyped(x.typ)) {
            // spec: "If the left operand of a non-constant shift
            // expression is an untyped constant, the type of the
            // constant is what it would be if the shift expression
            // were replaced by its left operand alone.".
            //
            // Delay operand checking until we know the final type
            // by marking the lhs expression as lhs shift operand.
            //
            // Usually (in correct programs), the lhs expression
            // is in the untyped map. However, it is possible to
            // create incorrect programs where the same expression
            // is evaluated twice (via a declaration cycle) such
            // that the lhs expression type is determined in the
            // first round and thus deleted from the map, and then
            // not found in the second round (double insertion of
            // the same expr node still just leads to one entry for
            // that node, and it can only be deleted once).
            // Be cautious and check for presence of entry.
            // Example: var e, f = int(1<<""[f]) // go.dev/issue/11347
            {
                var (info, found) = check.untyped[x.expr, ꟷ]; if (found) {
                    info.isLhs = true;
                    check.untyped[x.expr] = info;
                }
            }
            // keep x's type
            x.mode = value;
            return;
        }
    }
    // non-constant shift - lhs must be an integer
    if (!allInteger(x.typ)) {
        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidShiftOperand, invalidOp + "shifted operand %s must be integer", Ꮡx.OrTypedNil());
        x.mode = invalid;
        return;
    }
    x.mode = value;
}

internal static opPredicates binaryOpPredicates;

[GoInit] internal static void initΔ1() {
    // Setting binaryOpPredicates in init avoids declaration cycles.
    binaryOpPredicates = new opPredicates(new map<token.Token, Func<ΔType, bool>>{
        [token.ADD] = allNumericOrString,
        [token.SUB] = allNumeric,
        [token.MUL] = allNumeric,
        [token.QUO] = allNumeric,
        [token.REM] = allInteger,
        [token.AND] = allInteger,
        [token.OR] = allInteger,
        [token.XOR] = allInteger,
        [token.AND_NOT] = allInteger,
        [token.LAND] = allBoolean,
        [token.LOR] = allBoolean
    });
}

// If e != nil, it must be the binary expression; it may be nil for non-constant expressions
// (when invoked for an assignment operation where the binary expression is implicit).
internal static void binary(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ast.Expr e, ast.Expr lhs, ast.Expr rhs, token.Token op, tokenꓸPos opPos) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    ref var y = ref heap(new operand(), out var Ꮡy);
    Ꮡcheck.expr(nil, Ꮡx, lhs);
    Ꮡcheck.expr(nil, Ꮡy, rhs);
    if (x.mode == invalid) {
        return;
    }
    if (y.mode == invalid) {
        x.mode = invalid;
        x.expr = y.expr;
        return;
    }
    if (isShift(op)) {
        Ꮡcheck.shift(Ꮡx, Ꮡy, e, op);
        return;
    }
    Ꮡcheck.matchTypes(Ꮡx, Ꮡy);
    if (x.mode == invalid) {
        return;
    }
    if (isComparison(op)) {
        Ꮡcheck.comparison(Ꮡx, Ꮡy, op, false);
        return;
    }
    if (!Identical(x.typ, y.typ)) {
        // only report an error if we have valid types
        // (otherwise we had an error reported elsewhere already)
        if (isValid(x.typ) && isValid(y.typ)) {
            positioner posn = new operandжpositioner(Ꮡx);
            if (e != default!) {
                posn = new ast_Exprᴠpositioner(e);
            }
            if (e != default!){
                Ꮡcheck.errorf(posn, MismatchedTypes, invalidOp + "%s (mismatched types %s and %s)", e, x.typ, y.typ);
            } else {
                Ꮡcheck.errorf(posn, MismatchedTypes, invalidOp + "%s %s= %s (mismatched types %s and %s)", lhs, op, rhs, x.typ, y.typ);
            }
        }
        x.mode = invalid;
        return;
    }
    if (!Ꮡcheck.op(binaryOpPredicates, Ꮡx, op)) {
        x.mode = invalid;
        return;
    }
    if (op == token.QUO || op == token.REM) {
        // check for zero divisor
        if ((x.mode == constant_ || allInteger(x.typ)) && y.mode == constant_ && constant.Sign(y.val) == 0) {
            Ꮡcheck.error(new operandжpositioner(Ꮡy), DivByZero, invalidOp + "division by zero");
            x.mode = invalid;
            return;
        }
        // check for divisor underflow in complex division (see go.dev/issue/20227)
        if (x.mode == constant_ && y.mode == constant_ && isComplex(x.typ)) {
            var (re, im) = (constant.Real(y.val), constant.Imag(y.val));
            var (re2, im2) = (constant.BinaryOp(re, token.MUL, re), constant.BinaryOp(im, token.MUL, im));
            if (constant.Sign(re2) == 0 && constant.Sign(im2) == 0) {
                Ꮡcheck.error(new operandжpositioner(Ꮡy), DivByZero, invalidOp + "division by zero");
                x.mode = invalid;
                return;
            }
        }
    }
    if (x.mode == constant_ && y.mode == constant_) {
        // if either x or y has an unknown value, the result is unknown
        if (x.val.Kind() == constant.Unknown || y.val.Kind() == constant.Unknown) {
            x.val = constant.MakeUnknown();
            // x.typ is unchanged
            return;
        }
        // force integer division of integer operands
        if (op == token.QUO && isInteger(x.typ)) {
            op = token.QUO_ASSIGN;
        }
        x.val = constant.BinaryOp(x.val, op, y.val);
        x.expr = e;
        Ꮡcheck.overflow(Ꮡx, opPos);
        return;
    }
    x.mode = value;
}

// x.typ is unchanged

// matchTypes attempts to convert any untyped types x and y such that they match.
// If an error occurs, x.mode is set to invalid.
internal static void matchTypes(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<operand> Ꮡy) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // mayConvert reports whether the operands x and y may
    // possibly have matching types after converting one
    // untyped operand to the type of the other.
    // If mayConvert returns true, we try to convert the
    // operands to each other's types, and if that fails
    // we report a conversion failure.
    // If mayConvert returns false, we continue without an
    // attempt at conversion, and if the operand types are
    // not compatible, we report a type mismatch error.
    bool mayConvert(ж<operand> xΔ1, ж<operand> yΔ1) {
        // If both operands are typed, there's no need for an implicit conversion.
        if (isTyped((~xΔ1).typ) && isTyped((~yΔ1).typ)) {
            return false;
        }
        // An untyped operand may convert to its default type when paired with an empty interface
        // TODO(gri) This should only matter for comparisons (the only binary operation that is
        //           valid with interfaces), but in that case the assignability check should take
        //           care of the conversion. Verify and possibly eliminate this extra test.
        if (isNonTypeParamInterface((~xΔ1).typ) || isNonTypeParamInterface((~yΔ1).typ)) {
            return true;
        }
        // A boolean type can only convert to another boolean type.
        if (allBoolean((~xΔ1).typ) != allBoolean((~yΔ1).typ)) {
            return false;
        }
        // A string type can only convert to another string type.
        if (allString((~xΔ1).typ) != allString((~yΔ1).typ)) {
            return false;
        }
        // Untyped nil can only convert to a type that has a nil.
        if (xΔ1.isNil()) {
            return hasNil((~yΔ1).typ);
        }
        if (yΔ1.isNil()) {
            return hasNil((~xΔ1).typ);
        }
        // An untyped operand cannot convert to a pointer.
        // TODO(gri) generalize to type parameters
        if (isPointer((~xΔ1).typ) || isPointer((~yΔ1).typ)) {
            return false;
        }
        return true;
    }
    if (mayConvert(Ꮡx, Ꮡy)) {
        Ꮡcheck.convertUntyped(Ꮡx, y.typ);
        if (x.mode == invalid) {
            return;
        }
        Ꮡcheck.convertUntyped(Ꮡy, x.typ);
        if (y.mode == invalid) {
            x.mode = invalid;
            return;
        }
    }
}

partial struct exprKind /*num:nint*/;

internal static exprKind Δconversion => /* iota */ 0;
internal static exprKind expression => 1;
internal static exprKind statement => 2;

// target represent the (signature) type and description of the LHS
// variable of an assignment, or of a function result variable.
partial struct target {
    internal ж<ΔSignature> sig;
    internal @string desc;
}

// newTarget creates a new target for the given type and description.
// The result is nil if typ is not a signature.
internal static ж<target> newTarget(ΔType typ, @string desc) {
    if (typ != default!) {
        {
            var (sig, _) = under(typ)._<ж<ΔSignature>>(ᐧ); if (sig != nil) {
                return Ꮡ(new target(sig, desc));
            }
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exprSˢ = "-- expr %s"u8;

// rawExpr typechecks expression e and initializes x with the expression
// value or type. If an error occurred, x.mode is set to invalid.
// If a non-nil target T is given and e is a generic function,
// T is used to infer the type arguments for e.
// If hint != nil, it is the type of a composite literal element.
// If allowGeneric is set, the operand type may be an uninstantiated
// parameterized type or function value.
internal static exprKind rawExpr(this ж<Checker> Ꮡcheck, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e, ΔType hint, bool allowGeneric) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();

        if ((~check.conf)._Trace) {
            Ꮡcheck.trace(e.Pos(), exprSˢ, e);
            check.indent++;
            defer(() => {
                Ꮡcheck.Value.indent--;
                Ꮡcheck.trace(e.Pos(), "=> %s"u8, Ꮡx.OrTypedNil());
            }, ref ᒐ);
        }
        exprKind kind = Ꮡcheck.exprInternal(ᏑT, Ꮡx, e, hint);
        if (!allowGeneric) {
            Ꮡcheck.nonGeneric(ᏑT, Ꮡx);
        }
        check.record(Ꮡx);
        return kind;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typeˢ = "type"u8;
internal static readonly @string functionˢ = "function"u8;

// If x is a generic type, or a generic function whose type arguments cannot be inferred
// from a non-nil target T, nonGeneric reports an error and invalidates x.mode and x.typ.
// Otherwise it leaves x alone.
internal static void nonGeneric(this ж<Checker> Ꮡcheck, ж<target> ᏑT, ж<operand> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.mode == invalid || x.mode == novalue) {
        return;
    }
    @string what = default!;
    switch (x.typ.type()) {
    case ж<Alias> _:
    case ж<Named> _: {
        var t = x.typ;
        if (isGeneric(t)) {
            what = typeˢ;
        }
        break;
    }
    case ж<ΔSignature> t: {
        if ((~t).tparams != nil) {
            if (enableReverseTypeInference && ᏑT != nil) {
                Ꮡcheck.funcInst(ᏑT, x.Pos(), Ꮡx, nil, true);
                return;
            }
            what = functionˢ;
        }
        break;
    }}
    if (what != ""u8) {
        Ꮡcheck.errorf(new ast_Exprᴠpositioner(x.expr), WrongTypeArgCount, "cannot use generic %s %s without instantiation"u8, what, x.expr);
        x.mode = invalid;
        x.typ = new BasicжΔType(Typ[Invalid]);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidUseOfˢ = "invalid use of '...'"u8;
internal static readonly @string useOfTypeOutsideTypeˢ = "use of .(type) outside type switch"u8;
internal static readonly @string noKeyValueExpectedˢ = "no key:value expected"u8;

// exprInternal contains the core of type checking of expressions.
// Must only be called by rawExpr.
// (See rawExpr for an explanation of the parameters.)
internal static exprKind exprInternal(this ж<Checker> Ꮡcheck, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e, ΔType hint) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var T = ref ᏑT.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    // make sure x has a valid state in case of bailout
    // (was go.dev/issue/5770)
    x.mode = invalid;
    x.typ = new BasicжΔType(Typ[Invalid]);
    switch (e.type()) {
    case ж<ast.BadExpr> eΔ1: {
        goto ΔError; // error was reported before
        break;
    }
    case ж<ast.Ident> eΔ1: {
        Ꮡcheck.ident(Ꮡx, eΔ1, nil, false);
        break;
    }
    case ж<ast.Ellipsis> eΔ1: {
        Ꮡcheck.error(new ast_Ellipsisжpositioner(eΔ1), // ellipses are handled explicitly where they are legal
 // (array composite literals and parameter lists)
 BadDotDotDotSyntax, invalidUseOfˢ);
        goto ΔError;
        break;
    }
    case ж<ast.BasicLit> eΔ1: {
        Ꮡcheck.basicLit(Ꮡx, eΔ1);
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.FuncLit> eΔ1: {
        Ꮡcheck.funcLit(Ꮡx, eΔ1);
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.CompositeLit> eΔ1: {
        Ꮡcheck.compositeLit(Ꮡx, eΔ1, hint);
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.ParenExpr> eΔ1: {
        exprKind kind = Ꮡcheck.rawExpr(nil, // type inference doesn't go past parentheses (target type T = nil)
 Ꮡx, (~eΔ1).X, default!, false);
        x.expr = new ast.ParenExprжExpr(eΔ1);
        return kind;
    }
    case ж<ast.SelectorExpr> eΔ1: {
        Ꮡcheck.selector(Ꮡx, eΔ1, nil, false);
        break;
    }
    case ж<ast.IndexExpr> _:
    case ж<ast.IndexListExpr> _: {
        var eΔ1 = e;
        var ix = unpackIndexedExpr(eΔ1);
        if (Ꮡcheck.indexExpr(Ꮡx, ix)) {
            if (!enableReverseTypeInference) {
                ᏑT = default!; T = ref ᏑT.DerefOrNull();
            }
            Ꮡcheck.funcInst(ᏑT, eΔ1.Pos(), Ꮡx, ix, true);
        }
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.SliceExpr> eΔ1: {
        Ꮡcheck.sliceExpr(Ꮡx, eΔ1);
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.TypeAssertExpr> eΔ1: {
        Ꮡcheck.expr(nil, Ꮡx, (~eΔ1).X);
        if (x.mode == invalid) {
            goto ΔError;
        }
        if ((~eΔ1).Type == default!) {
            // x.(type) expressions are handled explicitly in type switches
            // Don't use InvalidSyntaxTree because this can occur in the AST produced by
            // go/parser.
            Ꮡcheck.error(new ast_TypeAssertExprжpositioner(eΔ1), BadTypeKeyword, useOfTypeOutsideTypeˢ);
            goto ΔError;
        }
        if (isTypeParam(x.typ)) {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidAssert, invalidOp + "cannot use type assertion on type parameter value %s", Ꮡx.OrTypedNil());
            goto ΔError;
        }
        {
            var (_, ok) = under(x.typ)._<ж<Interface>>(ᐧ); if (!ok) {
                Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidAssert, invalidOp + "%s is not an interface", Ꮡx.OrTypedNil());
                goto ΔError;
            }
        }
        var TΔ1 = Ꮡcheck.varType((~eΔ1).Type);
        if (!isValid(TΔ1)) {
            goto ΔError;
        }
        Ꮡcheck.typeAssertion(new ast.TypeAssertExprжExpr(eΔ1), Ꮡx, TΔ1, false);
        x.mode = commaok;
        x.typ = TΔ1;
        break;
    }
    case ж<ast.CallExpr> eΔ1: {
        return Ꮡcheck.callExpr(Ꮡx, eΔ1);
    }
    case ж<ast.StarExpr> eΔ1: {
        Ꮡcheck.exprOrType(Ꮡx, (~eΔ1).X, false);
        var exprᴛ1 = x.mode;
        if (exprᴛ1 == invalid) {
            goto ΔError;
        }
        else if (exprᴛ1 == typexpr) {
            Ꮡcheck.validVarType((~eΔ1).X, x.typ);
            x.typ = new PointerжΔType(Ꮡ(new Pointer(@base: x.typ)));
        }
        else { /* default: */
            ref var @base = ref heap<ΔType>(out var Ꮡbase);
            if (!underIs(x.typ, (ΔType u) => {
                var (p, _) = u._<ж<Pointer>>(ᐧ);
                if (p == nil) {
                    Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidIndirection, invalidOp + "cannot indirect %s", Ꮡx.OrTypedNil());
                    return false;
                }
                if (Ꮡbase.ValueSlot != default! && !Identical((~p).@base, Ꮡbase.ValueSlot)) {
                    Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidIndirection, invalidOp + "pointers of %s must have identical base types", Ꮡx.OrTypedNil());
                    return false;
                }
                Ꮡbase.ValueSlot = p.Value.@base;
                return true;
            })) {
                goto ΔError;
            }
            x.mode = variable;
            x.typ = @base;
        }

        break;
    }
    case ж<ast.UnaryExpr> eΔ1: {
        Ꮡcheck.unary(Ꮡx, eΔ1);
        if (x.mode == invalid) {
            goto ΔError;
        }
        if ((~eΔ1).Op == token.ARROW) {
            x.expr = new ast.UnaryExprжExpr(eΔ1);
            return statement; // receive operations may appear in statement context
        }
        break;
    }
    case ж<ast.BinaryExpr> eΔ1: {
        Ꮡcheck.binary(Ꮡx, new ast.BinaryExprжExpr(eΔ1), (~eΔ1).X, (~eΔ1).Y, (~eΔ1).Op, (~eΔ1).OpPos);
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.KeyValueExpr> eΔ1: {
        Ꮡcheck.error(new ast_KeyValueExprжpositioner(eΔ1), // key:value expressions are handled in composite literals
 InvalidSyntaxTree, noKeyValueExpectedˢ);
        goto ΔError;
        break;
    }
    case ж<ast.ArrayType> _:
    case ж<ast.StructType> _:
    case ж<ast.FuncType> _:
    case ж<ast.InterfaceType> _:
    case ж<ast.MapType> _:
    case ж<ast.ChanType> _: {
        var eΔ1 = e;
        x.mode = typexpr;
        x.typ = Ꮡcheck.typ(eΔ1);
        break;
    }
    default: {
        var eΔ1 = e;
        throw panic(fmt.Sprintf("%s: unknown expression type %T"u8, // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
 // even though check.typ has already called it. This is fine as both
 // times the same expression and type are recorded. It is also not a
 // performance issue because we only reach here for composite literal
 // types, which are comparatively rare.
 check.fset.Position(eΔ1.Pos()), eΔ1));
        break;
    }}
    // everything went well
    x.expr = e;
    return expression;
ΔError:
    x.mode = invalid;
    x.expr = e;
    return statement; // avoid follow-up errors
}

// keyVal maps a complex, float, integer, string or boolean constant value
// to the corresponding complex128, float64, int64, uint64, string, or bool
// Go value if possible; otherwise it returns x.
// A complex constant that can be represented as a float (such as 1.2 + 0i)
// is returned as a floating point value; if a floating point value can be
// represented as an integer (such as 1.0) it is returned as an integer value.
// This ensures that constants of different kind but equal value (such as
// 1.0 + 0i, 1.0, 1) result in the same value.
internal static any keyVal(constant.Value x) {
    var exprᴛ1 = x.Kind();
    var matchᴛ1 = false;
    if (exprᴛ1 == constant.Complex) { matchᴛ1 = true;
        var f = constant.ToFloat(x);
        if (f.Kind() != constant.Float) {
            var (r, _) = constant.Float64Val(constant.Real(x));
            var (i, _) = constant.Float64Val(constant.Imag(x));
            return complex(r, i);
        }
        x = f;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == constant.Float) { matchᴛ1 = true;
        var i = constant.ToInt(x);
        if (i.Kind() != constant.Int) {
            var (v, _) = constant.Float64Val(x);
            return v;
        }
        x = i;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == constant.Int) { matchᴛ1 = true;
        {
            var (v, ok) = constant.Int64Val(x); if (ok) {
                return v;
            }
        }
        {
            var (v, ok) = constant.Uint64Val(x); if (ok) {
                return v;
            }
        }
    }
    else if (exprᴛ1 == constant.ΔString) {
        return constant.StringVal(x);
    }
    else if (exprᴛ1 == constant.Bool) { matchᴛ1 = true;
        return constant.BoolVal(x);
    }

    return x;
}

// typeAssertion checks x.(T). The type of x must be an interface.
internal static void typeAssertion(this ж<Checker> Ꮡcheck, ast.Expr e, ж<operand> Ꮡx, ΔType T, bool typeSwitch) {
    ref var x = ref Ꮡx.DerefOrNull();

    ref var cause = ref heap(new @string(), out var Ꮡcause);
    if (Ꮡcheck.assertableTo(x.typ, T, Ꮡcause)) {
        return; // success
    }
    if (typeSwitch) {
        Ꮡcheck.errorf(new ast_Exprᴠpositioner(e), ImpossibleAssert, "impossible type switch case: %s\n\t%s cannot have dynamic type %s %s"u8, e, Ꮡx.OrTypedNil(), T, cause);
        return;
    }
    Ꮡcheck.errorf(new ast_Exprᴠpositioner(e), ImpossibleAssert, "impossible type assertion: %s\n\t%s does not implement %s %s"u8, e, T, x.typ, cause);
}

// expr typechecks expression e and initializes x with the expression value.
// If a non-nil target T is given and e is a generic function or
// a function call, T is used to infer the type arguments for e.
// The result must be a single value.
// If an error occurred, x.mode is set to invalid.
internal static void expr(this ж<Checker> Ꮡcheck, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e) {
    Ꮡcheck.rawExpr(ᏑT, Ꮡx, e, default!, false);
    Ꮡcheck.exclude(Ꮡx, (nuint)((nuint)(UntypedInt)((1 << (int)(byte)(novalue)) | (1 << (int)(byte)(Δbuiltinᴛ))) | (nuint)(1 << (int)(byte)(typexpr))));
    Ꮡcheck.singleValue(Ꮡx);
}

// genericExpr is like expr but the result may also be generic.
internal static void genericExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ast.Expr e) {
    Ꮡcheck.rawExpr(nil, Ꮡx, e, default!, true);
    Ꮡcheck.exclude(Ꮡx, (nuint)((nuint)(UntypedInt)((1 << (int)(byte)(novalue)) | (1 << (int)(byte)(Δbuiltinᴛ))) | (nuint)(1 << (int)(byte)(typexpr))));
    Ꮡcheck.singleValue(Ꮡx);
}

// multiExpr typechecks e and returns its value (or values) in list.
// If allowCommaOk is set and e is a map index, comma-ok, or comma-err
// expression, the result is a two-element list containing the value
// of e, and an untyped bool value or an error value, respectively.
// If an error occurred, list[0] is not valid.
internal static (slice<ж<operand>> list, bool commaOk) multiExpr(this ж<Checker> Ꮡcheck, ast.Expr e, bool allowCommaOk) {
    slice<ж<operand>> list = default!;
    bool commaOk = default!;

    ref var x = ref heap(new operand(), out var Ꮡx);
    Ꮡcheck.rawExpr(nil, Ꮡx, e, default!, false);
    Ꮡcheck.exclude(Ꮡx, (nuint)((nuint)(UntypedInt)((1 << (int)(byte)(novalue)) | (1 << (int)(byte)(Δbuiltinᴛ))) | (nuint)(1 << (int)(byte)(typexpr))));
    {
        var (t, ok) = x.typ._<ж<Tuple>>(ᐧ); if (ok && x.mode != invalid) {
            // multiple values
            list = new slice<ж<operand>>(t.Len());
            foreach (var (i, v) in (~t).vars) {
                list[i] = Ꮡ(new operand(mode: value, expr: e, typ: (~v).typ));
            }
            return (list, commaOk);
        }
    }
    // exactly one (possibly invalid or comma-ok) value
    list = new ж<operand>[]{Ꮡx}.slice();
    if (allowCommaOk && (x.mode == mapindex || x.mode == commaok || x.mode == commaerr)) {
        var x2 = Ꮡ(new operand(mode: value, expr: e, typ: new BasicжΔType(Typ[UntypedBool])));
        if (x.mode == commaerr) {
            x2.Value.typ = universeError;
        }
        list = append(list, x2);
        commaOk = true;
    }
    return (list, commaOk);
}

// exprWithHint typechecks expression e and initializes x with the expression value;
// hint is the type of a composite literal element.
// If an error occurred, x.mode is set to invalid.
internal static void exprWithHint(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ast.Expr e, ΔType hint) {
    assert(hint != default!);
    Ꮡcheck.rawExpr(nil, Ꮡx, e, hint, false);
    Ꮡcheck.exclude(Ꮡx, (nuint)((nuint)(UntypedInt)((1 << (int)(byte)(novalue)) | (1 << (int)(byte)(Δbuiltinᴛ))) | (nuint)(1 << (int)(byte)(typexpr))));
    Ꮡcheck.singleValue(Ꮡx);
}

// exprOrType typechecks expression or type e and initializes x with the expression value or type.
// If allowGeneric is set, the operand type may be an uninstantiated parameterized type or function
// value.
// If an error occurred, x.mode is set to invalid.
internal static void exprOrType(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ast.Expr e, bool allowGeneric) {
    Ꮡcheck.rawExpr(nil, Ꮡx, e, default!, allowGeneric);
    Ꮡcheck.exclude(Ꮡx, ((nuint)1 << (int)(byte)(novalue)));
    Ꮡcheck.singleValue(Ꮡx);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sUsedAsValueˢ = "%s used as value"u8;
internal static readonly @string sUsedAsValueOrTypeˢ = "%s used as value or type"u8;
internal static readonly @string sMustBeCalledˢ = "%s must be called"u8;
internal static readonly @string sIsNotAnExpressionˢ = "%s is not an expression"u8;

// exclude reports an error if x.mode is in modeset and sets x.mode to invalid.
// The modeset may contain any of 1<<novalue, 1<<builtin, 1<<typexpr.
internal static void exclude(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, nuint modeset) {
    ref var x = ref Ꮡx.DerefOrNull();

    if ((nuint)(modeset & (((nuint)1).Lsh((uint64)(byte)(x.mode)))) != 0) {
        @string msg = default!;
        errors.Code code = default!;
        var exprᴛ1 = x.mode;
        if (exprᴛ1 == novalue) {
            if ((nuint)(modeset & (nuint)(((nuint)1 << (int)(byte)(typexpr)))) != 0){
                msg = sUsedAsValueˢ;
            } else {
                msg = sUsedAsValueOrTypeˢ;
            }
            code = TooManyValues;
        }
        else if (exprᴛ1 == Δbuiltinᴛ) {
            msg = sMustBeCalledˢ;
            code = UncalledBuiltin;
        }
        else if (exprᴛ1 == typexpr) {
            msg = sIsNotAnExpressionˢ;
            code = NotAnExpr;
        }
        else { /* default: */
            throw panic("unreachable");
        }

        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), code, msg, Ꮡx.OrTypedNil());
        x.mode = invalid;
    }
}

// singleValue reports an error if x describes a tuple and sets x.mode to invalid.
internal static void singleValue(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.mode == value) {
        // tuple types are never named - no need for underlying type below
        {
            var (t, ok) = x.typ._<ж<Tuple>>(ᐧ); if (ok) {
                assert(t.Len() != 1);
                Ꮡcheck.errorf(new operandжpositioner(Ꮡx), TooManyValues, "multiple-value %s in single-value context"u8, Ꮡx.OrTypedNil());
                x.mode = invalid;
            }
        }
    }
}

} // end types_package
