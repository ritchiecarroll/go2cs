// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements type-checking of identifiers and type expressions.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using static @internal.types.errors_package;
using strings = strings_package;
using errors = @internal.types.errors_package;
using global::go.go;
using token = global::go.go.token_package;

partial class types_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotUseAsValueOrTypeˢ = "cannot use _ as value or type"u8;
internal static readonly @string cannotUseIotaOutsideˢ = "cannot use iota outside constant declaration"u8;

// ident type-checks identifier e and initializes x with the value or type of e.
// If an error occurred, x.mode is set to invalid.
// For the meaning of def, see Checker.definedType, below.
// If wantType is set, the identifier e is expected to denote a type.
internal static void ident(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.Ident> Ꮡe, ж<TypeName> Ꮡdef, bool wantType) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    x.mode = invalid;
    x.expr = new ast.IdentжExpr(Ꮡe);
    var (scope, obj) = Ꮡcheck.of(Checker.Ꮡenvironment).lookupScope(e.Name);
    var exprᴛ1 = obj;
    if (AreEqual(exprᴛ1, default!)) {
        if (e.Name == "_"u8){
            Ꮡcheck.error(new ast_Identжpositioner(Ꮡe), InvalidBlank, cannotUseAsValueOrTypeˢ);
        } else 
        if (isValidName(e.Name)) {
            Ꮡcheck.errorf(new ast_Identжpositioner(Ꮡe), UndeclaredName, "undefined: %s"u8, e.Name);
        }
        return;
    }
    if (AreEqual(exprᴛ1, universeComparable)) {
        if (!Ꮡcheck.verifyVersionf(new ast_Identжpositioner(Ꮡe), go1_18, "predeclared %s"u8, e.Name)) {
            return; // avoid follow-on errors
        }
    }

    // Because the representation of any depends on gotypesalias, we don't check
    // pointer identity here.
    if (obj.Name() == "any"u8 && obj.Parent() == Universe) {
        if (!Ꮡcheck.verifyVersionf(new ast_Identжpositioner(Ꮡe), go1_18, "predeclared %s"u8, e.Name)) {
            return; // avoid follow-on errors
        }
    }
    check.recordUse(Ꮡe, obj);
    // If we want a type but don't have one, stop right here and avoid potential problems
    // with missing underlying types. This also gives better error messages in some cases
    // (see go.dev/issue/65344).
    var (_, gotType) = obj._<ж<TypeName>>(ᐧ);
    if (!gotType && wantType) {
        Ꮡcheck.errorf(new ast_Identжpositioner(Ꮡe), NotAType, "%s is not a type"u8, obj.Name());
        // avoid "declared but not used" errors
        // (don't use Checker.use - we don't want to evaluate too much)
        {
            var (v, _) = obj._<ж<Var>>(ᐧ); if (v != nil && (~v).pkg == check.pkg) {
                /* see Checker.use1 */
                check.usedVars[v] = true;
            }
        }
        return;
    }
    // Type-check the object.
    // Only call Checker.objDecl if the object doesn't have a type yet
    // (in which case we must actually determine it) or the object is a
    // TypeName from the current package and we also want a type (in which case
    // we might detect a cycle which needs to be reported). Otherwise we can skip
    // the call and avoid a possible cycle error in favor of the more informative
    // "not a type/value" error that this function's caller will issue (see
    // go.dev/issue/25790).
    //
    // Note that it is important to avoid calling objDecl on objects from other
    // packages, to avoid races: see issue #69912.
    var typ = obj.Type();
    if (typ == default! || (gotType && wantType && obj.Pkg() == check.pkg)) {
        Ꮡcheck.objDecl(obj, Ꮡdef);
        typ = obj.Type(); // type must have been assigned by Checker.objDecl
    }
    assert(typ != default!);
    // The object may have been dot-imported.
    // If so, mark the respective package as used.
    // (This code is only needed for dot-imports. Without them,
    // we only have to mark variables, see *Var case below).
    {
        var pkgName = check.dotImportMap[new dotImportKey(scope, obj.Name())]; if (pkgName != nil) {
            check.usedPkgNames[pkgName] = true;
        }
    }
    switch (obj.type()) {
    case ж<PkgName> objΔ1: {
        Ꮡcheck.errorf(new ast_Identжpositioner(Ꮡe), InvalidPkgUse, "use of package %s not in selector"u8, (~objΔ1).name);
        return;
    }
    case ж<Const> objΔ1: {
        check.addDeclDep(new ConstжObject(objΔ1));
        if (!isValid(typ)) {
            return;
        }
        if (AreEqual(objΔ1, universeIota)){
            if (check.iota == default!) {
                Ꮡcheck.error(new ast_Identжpositioner(Ꮡe), InvalidIota, cannotUseIotaOutsideˢ);
                return;
            }
            x.val = check.iota;
        } else {
            x.val = objΔ1.Value.val;
        }
        assert(x.val != default!);
        x.mode = constant_;
        break;
    }
    case ж<TypeName> objΔ1: {
        if (!(~check.conf)._EnableAlias && check.isBrokenAlias(objΔ1)) {
            Ꮡcheck.errorf(new ast_Identжpositioner(Ꮡe), InvalidDeclCycle, "invalid use of type alias %s in recursive type (see go.dev/issue/50729)"u8, (~objΔ1).name);
            return;
        }
        x.mode = typexpr;
        break;
    }
    case ж<Var> objΔ1: {
        if ((~objΔ1).pkg == check.pkg) {
            // It's ok to mark non-local variables, but ignore variables
            // from other packages to avoid potential race conditions with
            // dot-imported variables.
            check.usedVars[objΔ1] = true;
        }
        check.addDeclDep(new VarжObject(objΔ1));
        if (!isValid(typ)) {
            return;
        }
        x.mode = variable;
        break;
    }
    case ж<Func> objΔ1: {
        check.addDeclDep(new FuncжObject(objΔ1));
        x.mode = value;
        break;
    }
    case ж<Builtin> objΔ1: {
        x.id = objΔ1.Value.id;
        x.mode = Δbuiltinᴛ;
        break;
    }
    case ж<Nil> objΔ1: {
        x.mode = value;
        break;
    }
    default: {
        var objΔ1 = obj;
        throw panic("unreachable");
        break;
    }}
    x.typ = typ;
}

// typ type-checks the type expression e and returns its type, or Typ[Invalid].
// The type must not be an (uninstantiated) generic type.
internal static ΔType typ(this ж<Checker> Ꮡcheck, ast.Expr e) {
    return Ꮡcheck.definedType(e, nil);
}

// varType type-checks the type expression e and returns its type, or Typ[Invalid].
// The type must not be an (uninstantiated) generic type and it must not be a
// constraint interface.
internal static ΔType varType(this ж<Checker> Ꮡcheck, ast.Expr e) {
    var typ = Ꮡcheck.definedType(e, nil);
    Ꮡcheck.validVarType(e, typ);
    return typ;
}

// validVarType reports an error if typ is a constraint interface.
// The expression e is used for error reporting, if any.
internal static void validVarType(this ж<Checker> Ꮡcheck, ast.Expr e, ΔType typ) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    // If we have a type parameter there's nothing to do.
    if (isTypeParam(typ)) {
        return;
    }
    // We don't want to call under() or complete interfaces while we are in
    // the middle of type-checking parameter declarations that might belong
    // to interface methods. Delay this check to the end of type-checking.
    check.later(() => {
        {
            var (t, _) = under(typ)._<ж<Interface>>(ᐧ); if (t != nil) {
                var tset = computeInterfaceTypeSet(Ꮡcheck, e.Pos(), t); // TODO(gri) is this the correct position?
                if (!tset.IsMethodSet()) {
                    if ((~tset).comparable){
                        Ꮡcheck.softErrorf(new ast_Exprᴠpositioner(e), MisplacedConstraintIface, "cannot use type %s outside a type constraint: interface is (or embeds) comparable"u8, typ);
                    } else {
                        Ꮡcheck.softErrorf(new ast_Exprᴠpositioner(e), MisplacedConstraintIface, "cannot use type %s outside a type constraint: interface contains type constraints"u8, typ);
                    }
                }
            }
        }
    }).describef(new ast_Exprᴠpositioner(e), "check var type %s"u8, typ);
}

// definedType is like typ but also accepts a type name def.
// If def != nil, e is the type specification for the type named def, declared
// in a type declaration, and def.typ.underlying will be set to the type of e
// before any components of e are type-checked.
internal static ΔType definedType(this ж<Checker> Ꮡcheck, ast.Expr e, ж<TypeName> Ꮡdef) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    var typ = Ꮡcheck.typInternal(e, Ꮡdef);
    assert(isTyped(typ));
    if (isGeneric(typ)) {
        Ꮡcheck.errorf(new ast_Exprᴠpositioner(e), WrongTypeArgCount, "cannot use generic type %s without instantiation"u8, typ);
        typ = new BasicжΔType(Typ[Invalid]);
    }
    check.recordTypeAndValue(e, typexpr, typ, default!);
    return typ;
}

// genericType is like typ but the type must be an (uninstantiated) generic
// type. If cause is non-nil and the type expression was a valid type but not
// generic, cause will be populated with a message describing the error.
//
// Note: If the type expression was invalid and an error was reported before,
// cause will not be populated; thus cause alone cannot be used to determine
// if an error occurred.
internal static ΔType genericType(this ж<Checker> Ꮡcheck, ast.Expr e, ж<@string> Ꮡcause) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var cause = ref Ꮡcause.DerefOrNull();

    var typ = Ꮡcheck.typInternal(e, nil);
    assert(isTyped(typ));
    if (isValid(typ) && !isGeneric(typ)) {
        if (Ꮡcause != nil) {
            cause = Ꮡcheck.sprintf("%s is not a generic type"u8, typ);
        }
        typ = new BasicжΔType(Typ[Invalid]);
    }
    // TODO(gri) what is the correct call below?
    check.recordTypeAndValue(e, typexpr, typ, default!);
    return typ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typesˢ = "types."u8;

// goTypeName returns the Go type name for typ and
// removes any occurrences of "types." from that name.
internal static @string goTypeName(ΔType typ) {
    return strings.ReplaceAll(fmt.Sprintf("%T"u8, typ), typesˢ, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typeSˢ = "-- type %s"u8;
internal static readonly @string sUnderSSˢ = "=> %s (under = %s) // %s"u8;
internal static readonly @string invalidUseOfArrayOutsideˢ = "invalid use of [...] array (outside a composite literal)"u8;
internal static readonly @string missingComparableˢ = " (missing comparable constraint)"u8;

// typInternal drives type checking of types.
// Must only be called by definedType or genericType.
internal static ΔType /*T*/ typInternal(this ж<Checker> Ꮡcheck, ast.Expr e0, ж<TypeName> Ꮡdef) {
    ΔType T = default!;
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();

        if ((~check.conf)._Trace) {
            Ꮡcheck.trace(e0.Pos(), typeSˢ, e0);
            check.indent++;
            defer(() => {
                Ꮡcheck.Value.indent--;
                ΔType under = default!;
                if (T != default!) {
                    // Calling under() here may lead to endless instantiations.
                    // Test case: type T[P any] *T[P]
                    under = safeUnderlying(T);
                }
                if (AreEqual(T, under)){
                    Ꮡcheck.trace(e0.Pos(), "=> %s // %s"u8, T, goTypeName(T));
                } else {
                    Ꮡcheck.trace(e0.Pos(), sUnderSSˢ, T, under, goTypeName(T));
                }
            }, ref ᒐ);
        }
        switch (e0.type()) {
        case ж<ast.BadExpr> e: {
            break;
        }
        case ж<ast.Ident> e: {
// ignore - error reported before
            ref var x = ref heap(new operand(), out var Ꮡx);
            Ꮡcheck.ident(Ꮡx, e, Ꮡdef, true);
            var exprᴛ1 = x.mode;
            if (exprᴛ1 == typexpr) {
                var typΔ2 = x.typ;
                setDefType(Ꮡdef, typΔ2);
                T = typΔ2; goto ᒐdone;
            }
            if (exprᴛ1 == invalid) {
            }
            else if (exprᴛ1 == novalue) {
                Ꮡcheck.errorf(new operandжpositioner(Ꮡx), // ignore - error reported before
 NotAType, "%s used as type"u8, Ꮡx);
            }
            else { /* default: */
                Ꮡcheck.errorf(new operandжpositioner(Ꮡx), NotAType, "%s is not a type"u8, Ꮡx);
            }

            break;
        }
        case ж<ast.SelectorExpr> e: {
            ref var x = ref heap(new operand(), out var Ꮡx);
            Ꮡcheck.selector(Ꮡx, e, Ꮡdef, true);
            var exprᴛ2 = x.mode;
            if (exprᴛ2 == typexpr) {
                var typΔ4 = x.typ;
                setDefType(Ꮡdef, typΔ4);
                T = typΔ4; goto ᒐdone;
            }
            if (exprᴛ2 == invalid) {
            }
            else if (exprᴛ2 == novalue) {
                Ꮡcheck.errorf(new operandжpositioner(Ꮡx), // ignore - error reported before
 NotAType, "%s used as type"u8, Ꮡx);
            }
            else { /* default: */
                Ꮡcheck.errorf(new operandжpositioner(Ꮡx), NotAType, "%s is not a type"u8, Ꮡx);
            }

            break;
        }
        case ж<ast.IndexExpr> _:
        case ж<ast.IndexListExpr> _: {
            var e = e0;
            var ix = unpackIndexedExpr(e);
            Ꮡcheck.verifyVersionf(inNode(e, (~ix).lbrack), go1_18, "type instantiation"u8);
            T = Ꮡcheck.instantiatedType(ix, Ꮡdef); goto ᒐdone;
        }
        case ж<ast.ParenExpr> e: {
            T = Ꮡcheck.definedType((~e).X, // Generic types must be instantiated before they can be used in any form.
 // Consequently, generic types cannot be parenthesized.
 Ꮡdef); goto ᒐdone;
        }
        case ж<ast.ArrayType> e: {
            if ((~e).Len == default!) {
                var typΔ5 = @new<Slice>();
                setDefType(Ꮡdef, new SliceжΔType(typΔ5));
                typΔ5.Value.elem = Ꮡcheck.varType((~e).Elt);
                T = new SliceжΔType(typΔ5); goto ᒐdone;
            }
            var typΔ6 = @new<Array>();
            setDefType(Ꮡdef, new ArrayжΔType(typΔ6));
            {
                var (_, ok) = (~e).Len._<ж<ast.Ellipsis>>(ᐧ); if (ok){
                    // Provide a more specific error when encountering a [...] array
                    // rather than leaving it to the handling of the ... expression.
                    Ꮡcheck.error(new ast_Exprᴠpositioner((~e).Len), BadDotDotDotSyntax, invalidUseOfArrayOutsideˢ);
                    typΔ6.Value.len = -1;
                } else {
                    typΔ6.Value.len = Ꮡcheck.arrayLength((~e).Len);
                }
            }
            typΔ6.Value.elem = Ꮡcheck.varType((~e).Elt);
            if ((~typΔ6).len >= 0) {
                T = new ArrayжΔType(typΔ6); goto ᒐdone;
            }
            break;
        }
        case ж<ast.Ellipsis> e: {
            Ꮡcheck.error(new ast_Ellipsisжpositioner(e), // report error if we encountered [...]
 // dots are handled explicitly where they are legal
 // (array composite literals and parameter lists)
 InvalidDotDotDot, invalidUseOfˢ);
            Ꮡcheck.use((~e).Elt);
            break;
        }
        case ж<ast.StructType> e: {
            var typΔ7 = @new<Struct>();
            setDefType(Ꮡdef, new StructжΔType(typΔ7));
            Ꮡcheck.structType(typΔ7, e);
            T = new StructжΔType(typΔ7); goto ᒐdone;
        }
        case ж<ast.StarExpr> e: {
            var typΔ8 = @new<Pointer>();
            typΔ8.Value.@base = new BasicжΔType(Typ[Invalid]); // avoid nil base in invalid recursive type declaration
            setDefType(Ꮡdef, new PointerжΔType(typΔ8));
            typΔ8.Value.@base = Ꮡcheck.varType((~e).X);
            if (!isValid((~typΔ8).@base)) {
                // If typ.base is invalid, it's unlikely that *base is particularly
                // useful - even a valid dereferenciation will lead to an invalid
                // type again, and in some cases we get unexpected follow-on errors
                // (e.g., go.dev/issue/49005). Return an invalid type instead.
                T = new BasicжΔType(Typ[Invalid]); goto ᒐdone;
            }
            T = new PointerжΔType(typΔ8); goto ᒐdone;
        }
        case ж<ast.FuncType> e: {
            var typΔ9 = @new<ΔSignature>();
            setDefType(Ꮡdef, new ΔSignatureжΔType(typΔ9));
            Ꮡcheck.funcType(typΔ9, nil, e);
            T = new ΔSignatureжΔType(typΔ9); goto ᒐdone;
        }
        case ж<ast.InterfaceType> e: {
            var typΔ10 = Ꮡcheck.newInterface();
            setDefType(Ꮡdef, new InterfaceжΔType(typΔ10));
            Ꮡcheck.interfaceType(typΔ10, e, Ꮡdef);
            T = new InterfaceжΔType(typΔ10); goto ᒐdone;
        }
        case ж<ast.MapType> e: {
            var typΔ11 = @new<Map>();
            setDefType(Ꮡdef, new MapжΔType(typΔ11));
            typΔ11.Value.key = Ꮡcheck.varType((~e).Key);
            typΔ11.Value.elem = Ꮡcheck.varType((~e).Value);
            var typʗ1 = typΔ11;
            check.later(() => {
                // spec: "The comparison operators == and != must be fully defined
                // for operands of the key type; thus the key type must not be a
                // function, map, or slice."
                //
                // Delay this check because it requires fully setup types;
                // it is safe to continue in any case (was go.dev/issue/6667).
                if (!Comparable((~typʗ1).key)) {
                    @string why = default!;
                    if (isTypeParam((~typʗ1).key)) {
                        why = missingComparableˢ;
                    }
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner((~e).Key), IncomparableMapKey, "invalid map key type %s%s"u8, (~typʗ1).key, why);
                }
            }).describef(new ast_Exprᴠpositioner((~e).Key), "check map key %s"u8, (~typΔ11).key);
            T = new MapжΔType(typΔ11); goto ᒐdone;
        }
        case ж<ast.ChanType> e: {
            var typΔ12 = @new<Chan>();
            setDefType(Ꮡdef, new ChanжΔType(typΔ12));
            ChanDir dir = SendRecv;
            var exprᴛ3 = (~e).Dir;
            if (exprᴛ3 == (ast.ChanDir)((ast.ChanDir)(ast.SEND | ast.RECV))) {
            }
            else if (exprᴛ3 == ast.SEND) {
                dir = SendOnly;
            }
            else if (exprᴛ3 == ast.RECV) {
                dir = RecvOnly;
            }
            else { /* default: */
                Ꮡcheck.errorf(new ast_ChanTypeжpositioner(e), // nothing to do
 InvalidSyntaxTree, "unknown channel direction %d"u8, (~e).Dir);
            }

            typΔ12.Value.dir = dir;
            typΔ12.Value.elem = Ꮡcheck.varType((~e).Value);
            T = new ChanжΔType(typΔ12); goto ᒐdone;
        }
        default: {
            var e = e0;
            Ꮡcheck.errorf(new ast_Exprᴠpositioner(e0), // ok to continue
 NotAType, "%s is not a type"u8, e0);
            Ꮡcheck.use(e0);
            break;
        }}
        var typ = Typ[Invalid];
        setDefType(Ꮡdef, new BasicжΔType(typ));
        T = new BasicжΔType(typ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return T;
}

internal static void setDefType(ж<TypeName> Ꮡdef, ΔType typ) {
    ref var def = ref Ꮡdef.DerefOrNull();

    if (Ꮡdef != nil) {
        switch (def.typ.type()) {
        case ж<Alias> t: {
            t.Value.fromRHS = typ;
            break;
        }
        case ж<Basic> t: {
            assert(t == Typ[Invalid]);
            break;
        }
        case ж<Named> t: {
            t.Value.underlying = typ;
            break;
        }
        default: {
            var t = def.typ;
            throw panic(fmt.Sprintf("unexpected type %T"u8, t));
            break;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string instantiatingTypeSWithSˢ = "-- instantiating type %s with %s"u8;

internal partial interface instantiatedType_type /*dyn*/ {
    ж<TypeName> Obj();
}

internal static ΔType /*res*/ instantiatedType(this ж<Checker> Ꮡcheck, ж<indexedExpr> Ꮡix, ж<TypeName> Ꮡdef) {
    ΔType res = default!;
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();
        ref var ix = ref Ꮡix.DerefOrNull();

        if ((~check.conf)._Trace) {
            Ꮡcheck.trace(ix.Pos(), instantiatingTypeSWithSˢ, ix.x, ix.indices);
            check.indent++;
            defer(() => {
                Ꮡcheck.Value.indent--;
                // Don't format the underlying here. It will always be nil.
                Ꮡcheck.trace(Ꮡix.Value.Pos(), "=> %s"u8, res);
            }, ref ᒐ);
        }
        defer(() => {
            setDefType(Ꮡdef, res);
        }, ref ᒐ);
        ref var cause = ref heap(new @string(), out var Ꮡcause);
        var typ = Ꮡcheck.genericType(ix.x, Ꮡcause);
        if (cause != ""u8) {
            Ꮡcheck.errorf(new ast_Exprᴠpositioner(ix.orig), NotAGenericType, invalidOp + "%s (%s)", ix.orig, cause);
        }
        if (!isValid(typ)) {
            res = typ; goto ᒐdone; // error already reported
        }
        // typ must be a generic Alias or Named type (but not a *Signature)
        {
            var (_, ok) = typ._<ж<ΔSignature>>(ᐧ); if (ok) {
                throw panic("unexpected generic signature");
            }
        }
        var gtyp = typ._<ΔgenericType>();
        // evaluate arguments
        var targs = Ꮡcheck.typeList(ix.indices);
        if (targs == default!) {
            res = new BasicжΔType(Typ[Invalid]); goto ᒐdone;
        }
        // create instance
        // The instance is not generic anymore as it has type arguments, but unless
        // instantiation failed, it still satisfies the genericType interface because
        // it has type parameters, too.
        var ityp = Ꮡcheck.instance(ix.Pos(), gtyp, targs, nil, check.context());
        var (inst, _) = ityp._<ΔgenericType>(ᐧ);
        if (inst == default!) {
            res = new BasicжΔType(Typ[Invalid]); goto ᒐdone;
        }
        // For Named types, orig.tparams may not be set up, so we need to do expansion later.
        var instʗ1 = inst;
        var targsʗ1 = targs;
        check.later(() => {
            // This is an instance from the source, not from recursive substitution,
            // and so it must be resolved during type-checking so that we can report
            // errors.
            Ꮡcheck.Value.recordInstance(Ꮡix.Value.orig, targsʗ1, instʗ1);
            @string name = instʗ1._<instantiatedType_type>().Obj().Value.name;
            var tparams = instʗ1.TypeParams().list();
            if (Ꮡcheck.validateTArgLen(Ꮡix.Value.Pos(), name, len(tparams), len(targsʗ1))) {
                // check type constraints
                {
                    var (i, err) = Ꮡcheck.verify(Ꮡix.Value.Pos(), instʗ1.TypeParams().list(), targsʗ1, Ꮡcheck.Value.context()); if (err != default!){
                        // best position for error reporting
                        tokenꓸPos pos = Ꮡix.Value.Pos();
                        if (i < len(Ꮡix.Value.indices)) {
                            pos = Ꮡix.Value.indices[i].Pos();
                        }
                        Ꮡcheck.softErrorf(((atPos)pos), InvalidTypeArg, "%v"u8, err);
                    } else {
                        Ꮡcheck.of(Checker.Ꮡmono).recordInstance(Ꮡcheck.Value.pkg, Ꮡix.Value.Pos(), tparams, targsʗ1, Ꮡix.Value.indices);
                    }
                }
            }
        }).describef(new indexedExprжpositioner(Ꮡix), "verify instantiation %s"u8, inst);
        res = inst;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return res;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidArrayLengthSˢ = "invalid array length %s"u8;
internal static readonly @string arrayLengthSMustBeˢ = "array length %s must be integer"u8;

// arrayLength type-checks the array length expression e
// and returns the constant length >= 0, or a value < 0
// to indicate an error (and thus an unknown length).
internal static int64 arrayLength(this ж<Checker> Ꮡcheck, ast.Expr e) {
    // If e is an identifier, the array declaration might be an
    // attempt at a parameterized type declaration with missing
    // constraint. Provide an error message that mentions array
    // length.
    {
        var (name, _) = e._<ж<ast.Ident>>(ᐧ); if (name != nil) {
            var obj = Ꮡcheck.of(Checker.Ꮡenvironment).lookup((~name).Name);
            if (obj == default!) {
                Ꮡcheck.errorf(new ast_Identжpositioner(name), InvalidArrayLen, "undefined array length %s or missing type constraint"u8, (~name).Name);
                return -1;
            }
            {
                var (_, ok) = obj._<ж<Const>>(ᐧ); if (!ok) {
                    Ꮡcheck.errorf(new ast_Identжpositioner(name), InvalidArrayLen, "invalid array length %s"u8, (~name).Name);
                    return -1;
                }
            }
        }
    }
    ref var x = ref heap(new operand(), out var Ꮡx);
    Ꮡcheck.expr(nil, Ꮡx, e);
    if (x.mode != constant_) {
        if (x.mode != invalid) {
            Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidArrayLen, "array length %s must be constant"u8, Ꮡx);
        }
        return -1;
    }
    if (isUntyped(x.typ) || isInteger(x.typ)) {
        {
            var val = constant.ToInt(x.val); if (val.Kind() == constant.Int) {
                if (representableConst(val, Ꮡcheck, Typ[Int], nil)) {
                    {
                        var (n, ok) = constant.Int64Val(val); if (ok && n >= 0) {
                            return n;
                        }
                    }
                }
            }
        }
    }
    @string msg = default!;
    if (isInteger(x.typ)){
        msg = invalidArrayLengthSˢ;
    } else {
        msg = arrayLengthSMustBeˢ;
    }
    Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidArrayLen, msg, Ꮡx);
    return -1;
}

// typeList provides the list of types corresponding to the incoming expression list.
// If an error occurred, the result is nil, but all list elements were type-checked.
internal static slice<ΔType> typeList(this ж<Checker> Ꮡcheck, slice<ast.Expr> list) {
    var res = new slice<ΔType>(len(list)); // res != nil even if len(list) == 0
    foreach (var (i, x) in list) {
        var t = Ꮡcheck.varType(x);
        if (!isValid(t)) {
            res = default!;
        }
        if (res != default!) {
            res[i] = t;
        }
    }
    return res;
}

} // end types_package
