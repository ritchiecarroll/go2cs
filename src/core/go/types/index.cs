// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements typechecking of index/slice expressions.
namespace go.go;

using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using static @internal.types.errors_package;
using errors = @internal.types.errors_package;
using global::go.go;

partial class types_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mapIndexˢ = "map index"u8;

// If e is a valid function instantiation, indexExpr returns true.
// In that case x represents the uninstantiated function value and
// it is the caller's responsibility to instantiate the function.
internal static bool /*isFuncInst*/ indexExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<indexedExpr> Ꮡe) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    Ꮡcheck.exprOrType(Ꮡx, e.x, true);
    // x may be generic
    var exprᴛ1 = x.mode;
    if (exprᴛ1 == invalid) {
        Ꮡcheck.use(e.indices.ꓸꓸꓸ);
        return false;
    }
    if (exprᴛ1 == typexpr) {
        x.mode = invalid;
        x.typ = Ꮡcheck.varType(e.orig);
        if (isValid(x.typ)) {
            // type instantiation
            // TODO(gri) here we re-evaluate e.X - try to avoid this
            x.mode = typexpr;
        }
        return false;
    }
    if (exprᴛ1 == value) {
        {
            var (sig, _) = under(x.typ)._<ж<ΔSignature>>(ᐧ); if (sig != nil && sig.TypeParams().Len() > 0) {
                // function instantiation
                return true;
            }
        }
    }

    // x should not be generic at this point, but be safe and check
    Ꮡcheck.nonGeneric(nil, Ꮡx);
    if (x.mode == invalid) {
        return false;
    }
    // ordinary index expression
    var valid = false;
    var length = (int64)(-1); // valid if >= 0
    switch (under(x.typ).type()) {
    case ж<Basic> typ: {
        if (isString(new BasicжΔType(typ))) {
            valid = true;
            if (x.mode == constant_) {
                length = (int64)len(constant.StringVal(x.val));
            }
            // an indexed string always yields a byte value
            // (not a constant) even if the string and the
            // index are constant
            x.mode = value;
            x.typ = universeByte; // use 'byte' name
        }
        break;
    }
    case ж<Array> typ: {
        valid = true;
        length = typ.Value.len;
        if (x.mode != variable) {
            x.mode = value;
        }
        x.typ = typ.Value.elem;
        break;
    }
    case ж<Pointer> typ: {
        {
            var (typΔ1, _) = under((~typ).@base)._<ж<Array>>(ᐧ); if (typΔ1 != nil) {
                valid = true;
                length = typΔ1.Value.len;
                x.mode = variable;
                x.typ = typΔ1.Value.elem;
            }
        }
        break;
    }
    case ж<Slice> typ: {
        valid = true;
        x.mode = variable;
        x.typ = typ.Value.elem;
        break;
    }
    case ж<Map> typ: {
        var indexΔ1 = Ꮡcheck.singleIndex(Ꮡe);
        if (indexΔ1 == default!) {
            x.mode = invalid;
            return false;
        }
        ref var key = ref heap(new operand(), out var Ꮡkey);
        Ꮡcheck.expr(nil, Ꮡkey, indexΔ1);
        Ꮡcheck.assignment(Ꮡkey, (~typ).key, mapIndexˢ);
        x.mode = mapindex;
        x.typ = typ.Value.elem;
        x.expr = e.orig;
        return false;
    }
    case ж<Interface> typ: {
        if (!isTypeParam(x.typ)) {
            // ok to continue even if indexing failed - map element type is known
            break;
        }
        // TODO(gri) report detailed failure cause for better error messages
        ref var key = ref heap<ΔType>(out var Ꮡkey);               // key != nil: we must have all maps
        ref var elem = ref heap<ΔType>(out var Ꮡelem);
        var mode = variable; // non-maps result mode
        if (underIs(x.typ, // TODO(gri) factor out closure and use it for non-typeparam cases as well
 (ΔType u) => {
            var l = (int64)(-1); // valid if >= 0
            ΔType k = default!;               // k is only set for maps
            ΔType eΔ1 = default!;
            switch (u.type()) {
            case ж<Basic> t: {
                if (isString(new BasicжΔType(t))) {
                    eΔ1 = universeByte;
                    mode = value;
                }
                break;
            }
            case ж<Array> t: {
                l = t.Value.len;
                eΔ1 = t.Value.elem;
                if (Ꮡx.Value.mode != variable) {
                    mode = value;
                }
                break;
            }
            case ж<Pointer> t: {
                {
                    var (tΔ1, _) = under((~t).@base)._<ж<Array>>(ᐧ); if (tΔ1 != nil) {
                        l = tΔ1.Value.len;
                        eΔ1 = tΔ1.Value.elem;
                    }
                }
                break;
            }
            case ж<Slice> t: {
                eΔ1 = t.Value.elem;
                break;
            }
            case ж<Map> t: {
                k = t.Value.key;
                eΔ1 = t.Value.elem;
                break;
            }}
            if (eΔ1 == default!) {
                return false;
            }
            if (Ꮡelem.ValueSlot == default!) {
                // first type
                length = l;
                (Ꮡkey.ValueSlot, Ꮡelem.ValueSlot) = (k, eΔ1);
                return true;
            }
            // all map keys must be identical (incl. all nil)
            // (that is, we cannot mix maps with other types)
            if (!Identical(Ꮡkey.ValueSlot, k)) {
                return false;
            }
            // all element types must be identical
            if (!Identical(Ꮡelem.ValueSlot, eΔ1)) {
                return false;
            }
            // track the minimal length for arrays, if any
            if (l >= 0 && l < length) {
                length = l;
            }
            return true;
        })) {
            // For maps, the index expression must be assignable to the map key type.
            if (key != default!) {
                var indexΔ2 = Ꮡcheck.singleIndex(Ꮡe);
                if (indexΔ2 == default!) {
                    x.mode = invalid;
                    return false;
                }
                ref var k = ref heap(new operand(), out var Ꮡk);
                Ꮡcheck.expr(nil, Ꮡk, indexΔ2);
                Ꮡcheck.assignment(Ꮡk, key, mapIndexˢ);
                // ok to continue even if indexing failed - map element type is known
                x.mode = mapindex;
                x.typ = elem;
                x.expr = e.orig;
                return false;
            }
            // no maps
            valid = true;
            x.mode = mode;
            x.typ = elem;
        }
        break;
    }}
    if (!valid) {
        // types2 uses the position of '[' for the error
        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), NonIndexableOperand, invalidOp + "cannot index %s", Ꮡx.OrTypedNil());
        Ꮡcheck.use(e.indices.ꓸꓸꓸ);
        x.mode = invalid;
        return false;
    }
    var index = Ꮡcheck.singleIndex(Ꮡe);
    if (index == default!) {
        x.mode = invalid;
        return false;
    }
    // In pathological (invalid) cases (e.g.: type T1 [][[]T1{}[0][0]]T0)
    // the element type may be accessed before it's set. Make sure we have
    // a valid type.
    if (x.typ == default!) {
        x.typ = new BasicжΔType(Typ[Invalid]);
    }
    Ꮡcheck.index(index, length);
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string and3rdIndexRequiredIn3ˢ = "2nd and 3rd index required in 3-index slice"u8;

internal static void sliceExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.SliceExpr> Ꮡe) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    Ꮡcheck.expr(nil, Ꮡx, e.X);
    if (x.mode == invalid) {
        Ꮡcheck.use(e.Low, e.High, e.Max);
        return;
    }
    var valid = false;
    var length = (int64)(-1); // valid if >= 0
    switch (coreString(x.typ).type()) {
    case null: {
        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), NonSliceableOperand, invalidOp + "cannot slice %s: %s has no core type", Ꮡx.OrTypedNil(), x.typ);
        x.mode = invalid;
        return;
    }
    case ж<Basic> u: {
        if (isString(new BasicжΔType(u))) {
            if (e.Slice3) {
                var at = e.Max;
                if (at == default!) {
                    at = new ast.SliceExprжExpr(Ꮡe); // e.Index[2] should be present but be careful
                }
                Ꮡcheck.error(new ast_Exprᴠpositioner(at), InvalidSliceExpr, invalidOp + "3-index slice of string");
                x.mode = invalid;
                return;
            }
            valid = true;
            if (x.mode == constant_) {
                length = (int64)len(constant.StringVal(x.val));
            }
            // spec: "For untyped string operands the result
            // is a non-constant value of type string."
            if (isUntyped(x.typ)) {
                x.typ = new BasicжΔType(Typ[ΔString]);
            }
        }
        break;
    }
    case ж<Array> u: {
        valid = true;
        length = u.Value.len;
        if (x.mode != variable) {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), NonSliceableOperand, invalidOp + "cannot slice %s (value not addressable)", Ꮡx.OrTypedNil());
            x.mode = invalid;
            return;
        }
        x.typ = new SliceжΔType(Ꮡ(new Slice(elem: (~u).elem)));
        break;
    }
    case ж<Pointer> u: {
        {
            var (uΔ1, _) = under((~u).@base)._<ж<Array>>(ᐧ); if (uΔ1 != nil) {
                valid = true;
                length = uΔ1.Value.len;
                x.typ = new SliceжΔType(Ꮡ(new Slice(elem: (~uΔ1).elem)));
            }
        }
        break;
    }
    case ж<Slice> u: {
        valid = true;
        break;
    }}
    // x.typ doesn't change
    if (!valid) {
        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), NonSliceableOperand, invalidOp + "cannot slice %s", Ꮡx.OrTypedNil());
        x.mode = invalid;
        return;
    }
    x.mode = value;
    // spec: "Only the first index may be omitted; it defaults to 0."
    if (e.Slice3 && (e.High == default! || e.Max == default!)) {
        Ꮡcheck.error(inNode(new ast.SliceExprжNode(Ꮡe), e.Rbrack), InvalidSyntaxTree, and3rdIndexRequiredIn3ˢ);
        x.mode = invalid;
        return;
    }
    // check indices
    array<int64> ind = new(3);
    foreach (var (i, expr) in new ast.Expr[]{e.Low, e.High, e.Max}.slice()) {
        var xΔ1 = (int64)(-1);
        switch (ᐧ) {
        case {} when expr != default!: {
            var max = (int64)(-1);
            if (length >= 0) {
                // The "capacity" is only known statically for strings, arrays,
                // and pointers to arrays, and it is the same as the length for
                // those types.
                max = length + 1;
            }
            {
                var (_, v) = Ꮡcheck.index(expr, max); if (v >= 0) {
                    xΔ1 = v;
                }
            }
            break;
        }
        case {} when i is 0: {
            xΔ1 = 0;
            break;
        }
        case {} when length is >= 0: {
            xΔ1 = length;
            break;
        }}

        // default is 0 for the first index
        // default is length (== capacity) otherwise
        ind[i] = xΔ1;
    }
    // constant indices must be in range
    // (check.index already checks that existing indices >= 0)
L:
    foreach (var (i, xΔ2) in ind[..(int)(len(ind) - 1)]) {
        if (xΔ2 > 0) {
            foreach (var (j, y) in ind.slice(i + 1)) {
                if (y >= 0 && y < xΔ2) {
                    // The value y corresponds to the expression e.Index[i+1+j].
                    // Because y >= 0, it must have been set from the expression
                    // when checking indices and thus e.Index[i+1+j] is not nil.
                    var at = new ast.Expr[]{e.Low, e.High, e.Max}.slice()[i + 1 + j];
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner(at), SwappedSliceIndices, "invalid slice indices: %d < %d"u8, y, xΔ2);
                    goto break_L; // only report one error, ok to continue
                }
            }
        }
continue_L:;
    }
break_L:;
}

// singleIndex returns the (single) index from the index expression e.
// If the index is missing, or if there are multiple indices, an error
// is reported and the result is nil.
internal static ast.Expr singleIndex(this ж<Checker> Ꮡcheck, ж<indexedExpr> Ꮡexpr) {
    ref var expr = ref Ꮡexpr.DerefOrNull();

    if (len(expr.indices) == 0) {
        Ꮡcheck.errorf(new ast_Exprᴠpositioner(expr.orig), InvalidSyntaxTree, "index expression %v with 0 indices"u8, Ꮡexpr.OrTypedNil());
        return default!;
    }
    if (len(expr.indices) > 1) {
        // TODO(rFindley) should this get a distinct error code?
        Ꮡcheck.error(new ast_Exprᴠpositioner(expr.indices[1]), InvalidIndex, invalidOp + "more than one index");
    }
    return expr.indices[0];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string indexˢ = "index"u8;

// index checks an index expression for validity.
// If max >= 0, it is the upper bound for index.
// If the result typ is != Typ[Invalid], index is valid and typ is its (possibly named) integer type.
// If the result val >= 0, index is valid and val is its constant int value.
internal static (ΔType typ, int64 val) index(this ж<Checker> Ꮡcheck, ast.Expr index, int64 max) {
    ΔType typ = default!;
    int64 val = default!;

    typ = new BasicжΔType(Typ[Invalid]);
    val = -1;
    ref var x = ref heap(new operand(), out var Ꮡx);
    Ꮡcheck.expr(nil, Ꮡx, index);
    if (!Ꮡcheck.isValidIndex(Ꮡx, InvalidIndex, indexˢ, false)) {
        return (typ, val);
    }
    if (x.mode != constant_) {
        return (x.typ, -1);
    }
    if (x.val.Kind() == constant.Unknown) {
        return (typ, val);
    }
    var (v, ok) = constant.Int64Val(x.val);
    assert(ok);
    if (max >= 0 && v >= max) {
        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidIndex, invalidArg + "index %s out of bounds [0:%d]", x.val.String(), max);
        return (typ, val);
    }
    // 0 <= v [ && v < max ]
    return (x.typ, v);
}

internal static bool isValidIndex(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, errors.Code code, @string what, bool allowNegative) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.mode == invalid) {
        return false;
    }
    // spec: "a constant index that is untyped is given type int"
    Ꮡcheck.convertUntyped(Ꮡx, new BasicжΔType(Typ[Int]));
    if (x.mode == invalid) {
        return false;
    }
    // spec: "the index x must be of integer type or an untyped constant"
    if (!allInteger(x.typ)) {
        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), code, invalidArg + "%s %s must be integer", what, Ꮡx.OrTypedNil());
        return false;
    }
    if (x.mode == constant_) {
        // spec: "a constant index must be non-negative ..."
        if (!allowNegative && constant.Sign(x.val) < 0) {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), code, invalidArg + "%s %s must not be negative", what, Ꮡx.OrTypedNil());
            return false;
        }
        // spec: "... and representable by a value of type int"
        if (!representableConst(x.val, Ꮡcheck, Typ[Int], Ꮡx.of(operand.Ꮡval))) {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), code, invalidArg + "%s %s overflows int", what, Ꮡx.OrTypedNil());
            return false;
        }
    }
    return true;
}

// indexedExpr wraps an ast.IndexExpr or ast.IndexListExpr.
//
// Orig holds the original ast.Expr from which this indexedExpr was derived.
//
// Note: indexedExpr (intentionally) does not wrap ast.Expr, as that leads to
// accidental misuse such as encountered in golang/go#63933.
//
// TODO(rfindley): remove this helper, in favor of just having a helper
// function that returns indices.
[GoType] partial struct indexedExpr {
    internal ast.Expr orig;   // the wrapped expr, which may be distinct from the IndexListExpr below.
    internal ast.Expr x;   // expression
    internal tokenꓸPos lbrack;  // position of "["
    internal slice<ast.Expr> indices; // index expressions
    internal tokenꓸPos rbrack;  // position of "]"
}

[GoRecv] internal static tokenꓸPos Pos(this ref indexedExpr x) {
    return x.orig.Pos();
}

internal static ж<indexedExpr> unpackIndexedExpr(ast.Node n) {
    switch (n.type()) {
    case ж<ast.IndexExpr> e: {
        return Ꮡ(new indexedExpr(
            orig: new ast.IndexExprжExpr(e),
            x: (~e).X,
            lbrack: (~e).Lbrack,
            indices: new ast.Expr[]{(~e).Index}.slice(),
            rbrack: (~e).Rbrack
        ));
    }
    case ж<ast.IndexListExpr> e: {
        return Ꮡ(new indexedExpr(
            orig: new ast.IndexListExprжExpr(e),
            x: (~e).X,
            lbrack: (~e).Lbrack,
            indices: (~e).Indices,
            rbrack: (~e).Rbrack
        ));
    }}
    return default!;
}

} // end types_package
