// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using static @internal.types.errors_package;
using filepath = path.filepath_package;
using strings = strings_package;
using constant = global::go.go.constant_package;
using errors = @internal.types.errors_package;
using global::go.go;
using path;

partial class types_package {

// ----------------------------------------------------------------------------
// API

// A Signature represents a (non-builtin) function or method type.
// The receiver is ignored when comparing signatures for identity.
partial struct ΔSignature {
    // We need to keep the scope in Signature (rather than passing it around
    // and store it in the Func Object) because when type-checking a function
    // literal we call the general type checker which returns a general Type.
    // We then unpack the *Signature and use the scope for the literal body.
    internal ж<TypeParamList> rparams; // receiver type parameters from left to right, or nil
    internal ж<TypeParamList> tparams; // type parameters from left to right, or nil
    internal ж<ΔScope> scope;      // function scope for package-local and non-instantiated signatures; nil otherwise
    internal ж<Var> recv;        // nil if not a method
    internal ж<Tuple> @params;      // (incoming) parameters from left to right; or nil
    internal ж<Tuple> results;      // (outgoing) results from left to right; or nil
    internal bool variadic;           // true if the last parameter's type is of the form ...T (or string, for append built-in only)
}

// NewSignature returns a new function type for the given receiver, parameters,
// and results, either of which may be nil. If variadic is set, the function
// is variadic, it must have at least one parameter, and the last parameter
// must be of unnamed slice type.
//
// Deprecated: Use [NewSignatureType] instead which allows for type parameters.
public static ж<ΔSignature> NewSignature(ж<Var> Ꮡrecv, ж<Tuple> Ꮡparams, ж<Tuple> Ꮡresults, bool variadic) {
    return NewSignatureType(Ꮡrecv, default!, default!, Ꮡparams, Ꮡresults, variadic);
}

// NewSignatureType creates a new function type for the given receiver,
// receiver type parameters, type parameters, parameters, and results. If
// variadic is set, params must hold at least one parameter and the last
// parameter's core type must be of unnamed slice or bytestring type.
// If recv is non-nil, typeParams must be empty. If recvTypeParams is
// non-empty, recv must be non-nil.
public static ж<ΔSignature> NewSignatureType(ж<Var> Ꮡrecv, slice<ж<TypeParam>> recvTypeParams, slice<ж<TypeParam>> typeParams, ж<Tuple> Ꮡparams, ж<Tuple> Ꮡresults, bool variadic) {
    ref var @params = ref Ꮡparams.DerefOrNull();

    if (variadic) {
        nint n = Ꮡparams.Len();
        if (n == 0) {
            throw panic("variadic function must have at least one parameter");
        }
        var core = coreString((~@params.At(n - 1)).typ);
        {
            var (_, ok) = core._<ж<Slice>>(ᐧ); if (!ok && !isString(core)) {
                throw panic(fmt.Sprintf("got %s, want variadic parameter with unnamed slice type or string as core type"u8, core.String()));
            }
        }
    }
    var sig = Ꮡ(new ΔSignature(recv: Ꮡrecv, @params: Ꮡparams, results: Ꮡresults, variadic: variadic));
    if (len(recvTypeParams) != 0) {
        if (Ꮡrecv == nil) {
            throw panic("function with receiver type parameters must have a receiver");
        }
        sig.Value.rparams = bindTParams(recvTypeParams);
    }
    if (len(typeParams) != 0) {
        if (Ꮡrecv != nil) {
            throw panic("function with type parameters cannot have a receiver");
        }
        sig.Value.tparams = bindTParams(typeParams);
    }
    return sig;
}

// Recv returns the receiver of signature s (if a method), or nil if a
// function. It is ignored when comparing signatures for identity.
//
// For an abstract method, Recv returns the enclosing interface either
// as a *[Named] or an *[Interface]. Due to embedding, an interface may
// contain methods whose receiver type is a different interface.
public static ж<Var> Recv(this ref ΔSignature s) {
    return s.recv;
}

// TypeParams returns the type parameters of signature s, or nil.
public static ж<TypeParamList> TypeParams(this ref ΔSignature s) {
    return s.tparams;
}

// RecvTypeParams returns the receiver type parameters of signature s, or nil.
public static ж<TypeParamList> RecvTypeParams(this ref ΔSignature s) {
    return s.rparams;
}

// Params returns the parameters of signature s, or nil.
public static ж<Tuple> Params(this ref ΔSignature s) {
    return s.@params;
}

// Results returns the results of signature s, or nil.
public static ж<Tuple> Results(this ref ΔSignature s) {
    return s.results;
}

// Variadic reports whether the signature s is variadic.
public static bool Variadic(this ref ΔSignature s) {
    return s.variadic;
}

public static ΔType Underlying(this ж<ΔSignature> Ꮡs) {
    return new ΔSignatureжΔType(Ꮡs);
}

public static @string String(this ж<ΔSignature> Ꮡs) {
    return TypeString(new ΔSignatureжΔType(Ꮡs), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string methodHasMultipleˢ = "method has multiple receivers"u8;

// ----------------------------------------------------------------------------
// Implementation

// funcType type-checks a function or method type.
internal static void funcType(this ж<Checker> Ꮡcheck, ж<ΔSignature> Ꮡsig, ж<ast.FieldList> ᏑrecvPar, ж<ast.FuncType> Ꮡftyp) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();
        ref var sig = ref Ꮡsig.DerefOrNull();
        ref var recvPar = ref ᏑrecvPar.DerefOrNull();
        ref var ftyp = ref Ꮡftyp.DerefOrNull();

        check.openScope(new ast.FuncTypeжNode(Ꮡftyp), functionˢ);
        check.scope.Value.isFunc = true;
        check.recordScope(new ast.FuncTypeжNode(Ꮡftyp), check.scope);
        sig.scope = check.scope;
        ᒐd1 = true;
        // collect method receiver, if any
        ж<Var> recv = default!;
        ж<TypeParamList> rparams = default!;
        if (ᏑrecvPar != nil && ᏑrecvPar.NumFields() > 0) {
            // We have at least one receiver; make sure we don't have more than one.
            {
                nint n = len(recvPar.List); if (n > 1) {
                    Ꮡcheck.error(new ast_Fieldжpositioner(recvPar.List[n - 1]), InvalidRecv, methodHasMultipleˢ);
                }
            }
            // continue with first one
            // all type parameters' scopes start after the method name
            tokenꓸPos scopePosΔ1 = ftyp.Pos();
            (recv, rparams) = Ꮡcheck.collectRecv(recvPar.List[0], scopePosΔ1);
        }
        // collect and declare function type parameters
        if (ftyp.TypeParams != nil) {
            // Always type-check method type parameters but complain that they are not allowed.
            // (A separate check is needed when type-checking interface method signatures because
            // they don't have a receiver specification.)
            if (ᏑrecvPar != nil) {
                Ꮡcheck.error(new ast_FieldListжpositioner(ftyp.TypeParams), InvalidMethodTypeParams, methodsCannotHaveTypeˢ);
            }
            Ꮡcheck.collectTypeParams(Ꮡsig.of(types_package.ΔSignature.Ꮡtparams), ftyp.TypeParams);
        }
        // collect ordinary and result parameters
        var (pnames, @params, variadic) = Ꮡcheck.collectParams(ftyp.Params, true);
        var (rnames, results, _) = Ꮡcheck.collectParams(ftyp.Results, false);
        // declare named receiver, ordinary, and result parameters
        tokenꓸPos scopePos = ftyp.End(); // all parameter's scopes start after the signature
        if (recv != nil && (~recv).name != ""u8) {
            Ꮡcheck.declare(check.scope, (~recvPar.List[0]).Names[0], new VarжObject(recv), scopePos);
        }
        Ꮡcheck.declareParams(pnames, @params, scopePos);
        Ꮡcheck.declareParams(rnames, results, scopePos);
        sig.recv = recv;
        sig.rparams = rparams;
        sig.@params = NewTuple(@params.ꓸꓸꓸ);
        sig.results = NewTuple(results.ꓸꓸꓸ);
        sig.variadic = variadic;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡcheck.DerefOrNull().closeScope(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typeParameterˢ = "type parameter"u8;

// collectRecv extracts the method receiver and its type parameters (if any) from rparam.
// It declares the type parameters (but not the receiver) in the current scope, and
// returns the receiver variable and its type parameter list (if any).
internal static (ж<Var>, ж<TypeParamList>) collectRecv(this ж<Checker> Ꮡcheck, ж<ast.Field> Ꮡrparam, tokenꓸPos scopePos) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var rparam = ref Ꮡrparam.DerefOrNull();

    // Unpack the receiver parameter which is of the form
    //
    //	"(" [rfield] ["*"] rbase ["[" rtparams "]"] ")"
    //
    // The receiver name rname, the pointer indirection, and the
    // receiver type parameters rtparams may not be present.
    var (rptr, rbase, rtparams) = Ꮡcheck.unpackRecv(rparam.Type, true);
    // Determine the receiver base type.
    ΔType recvType = new BasicжΔType(Typ[Invalid]);
    ж<TypeParamList> recvTParamsList = default!;
    if (rtparams == default!){
        // If there are no type parameters, we can simply typecheck rparam.Type.
        // If that is a generic type, varType will complain.
        // Further receiver constraints will be checked later, with validRecv.
        // We use rparam.Type (rather than base) to correctly record pointer
        // and parentheses in types.Info (was bug, see go.dev/issue/68639).
        recvType = Ꮡcheck.varType(rparam.Type);
        // Defining new methods on instantiated (alias or defined) types is not permitted.
        // Follow literal pointer/alias type chain and check.
        // (Correct code permits at most one pointer indirection, but for this check it
        // doesn't matter if we have multiple pointers.)
        var (a, _) = unpointer(recvType)._<ж<Alias>>(ᐧ); // recvType is not generic per above
        while (a != nil) {
            var baseType = unpointer((~a).fromRHS);
            {
                var (g, _) = baseType._<ΔgenericType>(ᐧ); if (g != default! && g.TypeParams() != nil) {
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner(rbase), InvalidRecv, "cannot define new methods on instantiated type %s"u8, g);
                    recvType = new BasicжΔType(Typ[Invalid]); // avoid follow-on errors by Checker.validRecv
                    break;
                }
            }
            (a, _) = baseType._<ж<Alias>>(ᐧ);
        }
    } else {
        // If there are type parameters, rbase must denote a generic base type.
        // Important: rbase must be resolved before declaring any receiver type
        // parameters (which may have the same name, see below).
        ж<Named> baseType = default!;      // nil if not valid
        ref var cause = ref heap(new @string(), out var Ꮡcause);
        {
            var t = Ꮡcheck.genericType(rbase, Ꮡcause); if (isValid(t)){
                switch (t.type()) {
                case ж<Named> tΔ1: {
                    baseType = tΔ1;
                    break;
                }
                case ж<Alias> tΔ1: {
                    if (isValid(unalias(tΔ1))) {
                        // Methods on generic aliases are not permitted.
                        // Only report an error if the alias type is valid.
                        Ꮡcheck.errorf(new ast_Exprᴠpositioner(rbase), InvalidRecv, "cannot define new methods on generic alias type %s"u8, tΔ1.OrTypedNil());
                    }
                    break;
                }
                default: {
                    var tΔ1 = t;
                    throw panic("unreachable");
                    break;
                }}
            } else {
                // Ok to continue but do not set basetype in this case so that
                // recvType remains invalid (was bug, see go.dev/issue/70417).
                if (cause != ""u8) {
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner(rbase), InvalidRecv, "%s"u8, cause);
                }
            }
        }
        // Ok to continue but do not set baseType (see comment above).
        // Collect the type parameters declared by the receiver (see also
        // Checker.collectTypeParams). The scope of the type parameter T in
        // "func (r T[T]) f() {}" starts after f, not at r, so we declare it
        // after typechecking rbase (see go.dev/issue/52038).
        var recvTParams = new slice<ж<TypeParam>>(len(rtparams));
        foreach (var (i, rparamΔ1) in rtparams) {
            var tpar = Ꮡcheck.declareTypeParam(rparamΔ1, scopePos);
            recvTParams[i] = tpar;
            // For historic reasons, type parameters in receiver type expressions
            // are considered both definitions and uses and thus must be recorded
            // in the Info.Uses and Info.Types maps (see go.dev/issue/68670).
            check.recordUse(rparamΔ1, new TypeNameжObject((~tpar).obj));
            check.recordTypeAndValue(new ast.IdentжExpr(rparamΔ1), typexpr, new TypeParamжΔType(tpar), default!);
        }
        recvTParamsList = bindTParams(recvTParams);
        // Get the type parameter bounds from the receiver base type
        // and set them for the respective (local) receiver type parameters.
        if (baseType != nil) {
            var baseTParams = baseType.TypeParams().list();
            if (len(recvTParams) == len(baseTParams)){
                var smap = makeRenameMap(baseTParams, recvTParams);
                foreach (var (i, recvTPar) in recvTParams) {
                    var baseTPar = baseTParams[i];
                    check.mono.recordCanon(recvTPar, baseTPar);
                    // baseTPar.bound is possibly parameterized by other type parameters
                    // defined by the generic base type. Substitute those parameters with
                    // the receiver type parameters declared by the current method.
                    recvTPar.Value.bound = Ꮡcheck.subst((~(~recvTPar).obj).pos, (~baseTPar).bound, smap, nil, check.context());
                }
            } else {
                @string got = measure(len(recvTParams), typeParameterˢ);
                Ꮡcheck.errorf(new ast_Exprᴠpositioner(rbase), BadRecv, "receiver declares %s, but receiver base type declares %d"u8, got, len(baseTParams));
            }
            // The type parameters declared by the receiver also serve as
            // type arguments for the receiver type. Instantiate the receiver.
            Ꮡcheck.verifyVersionf(new ast_Exprᴠpositioner(rbase), go1_18, "type instantiation"u8);
            var targs = new slice<ΔType>(len(recvTParams));
            foreach (var (i, targ) in recvTParams) {
                targs[i] = new TypeParamжΔType(targ);
            }
            recvType = Ꮡcheck.instance(rparam.Type.Pos(), new NamedжΔgenericType(baseType), targs, nil, check.context());
            check.recordInstance(rbase, targs, recvType);
            // Reestablish pointerness if needed (but avoid a pointer to an invalid type).
            if (rptr && isValid(recvType)) {
                recvType = new PointerжΔType(NewPointer(recvType));
            }
            check.recordParenthesizedRecvTypes(rparam.Type, recvType);
        }
    }
    // Make sure we have no more than one receiver name.
    ж<ast.Ident> rname = default!;
    {
        nint n = len(rparam.Names); if (n >= 1) {
            if (n > 1) {
                Ꮡcheck.error(new ast_Identжpositioner(rparam.Names[n - 1]), InvalidRecv, methodHasMultipleˢ);
            }
            rname = rparam.Names[0];
        }
    }
    // Create the receiver parameter.
    // recvType is invalid if baseType was never set.
    ж<Var> recv = default!;
    if (rname != nil && (~rname).Name != ""u8){
        // named receiver
        recv = NewParam(rname.Pos(), check.pkg, (~rname).Name, recvType);
    } else {
        // In this case, the receiver is declared by the caller
        // because it must be declared after any type parameters
        // (otherwise it might shadow one of them).
        // anonymous receiver
        recv = NewParam(rparam.Pos(), check.pkg, ""u8, recvType);
        check.recordImplicit(new ast.FieldжNode(Ꮡrparam), new VarжObject(recv));
    }
    // Delay validation of receiver type as it may cause premature expansion of types
    // the receiver type is dependent on (see go.dev/issue/51232, go.dev/issue/51233).
    var rbaseʗ1 = rbase;
    var recvʗ1 = recv;
    check.later(() => {
        Ꮡcheck.validRecv(new ast_Exprᴠpositioner(rbaseʗ1), recvʗ1);
    }).describef(new Varжpositioner(recv), "validRecv(%s)"u8, recv.OrTypedNil());
    return (recv, recvTParamsList);
}

internal static ΔType unpointer(ΔType t) {
    while (ᐧ) {
        var (p, _) = t._<ж<Pointer>>(ᐧ);
        if (p == nil) {
            return t;
        }
        t = p.Value.@base;
    }
}

// recordParenthesizedRecvTypes records parenthesized intermediate receiver type
// expressions that all map to the same type, by recursively unpacking expr and
// recording the corresponding type for it. Example:
//
//	expression  -->  type
//	----------------------
//	(*(T[P]))        *T[P]
//	 *(T[P])         *T[P]
//	  (T[P])          T[P]
//	   T[P]           T[P]
internal static void recordParenthesizedRecvTypes(this ref Checker check, ast.Expr expr, ΔType typ) {
    while (ᐧ) {
        check.recordTypeAndValue(expr, typexpr, typ, default!);
        switch (expr.type()) {
        case ж<ast.ParenExpr> e: {
            expr = e.Value.X;
            break;
        }
        case ж<ast.StarExpr> e: {
            expr = e.Value.X;
            var (ptr, _) = typ._<ж<Pointer>>(ᐧ);
            if (ptr == nil) {
                // In a correct program, typ must be an unnamed
                // pointer type. But be careful and don't panic.
                return; // something is wrong
            }
            typ = ptr.Value.@base;
            break;
        }
        default: {
            var e = expr;
            return; // cannot unpack any further
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string anonymousParameterˢ = "anonymous parameter"u8;
internal static readonly @string listContainsBothNamedAndˢ = "list contains both named and anonymous parameters"u8;

// collectParams collects (but does not declare) all parameters of list and returns
// the list of parameter names, corresponding parameter variables, and whether the
// parameter list is variadic. Anonymous parameters are recorded with nil names.
internal static (slice<ж<ast.Ident>> names, slice<ж<Var>> @params, bool variadic) collectParams(this ж<Checker> Ꮡcheck, ж<ast.FieldList> Ꮡlist, bool variadicOk) {
    slice<ж<ast.Ident>> names = default!;
    slice<ж<Var>> @params = default!;
    bool variadic = default!;

    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var list = ref Ꮡlist.DerefOrNull();
    if (Ꮡlist == nil) {
        return (names, @params, variadic);
    }
    bool named = default!;
    bool anonymous = default!;
    foreach (var (i, field) in list.List) {
        var ftype = field.Value.Type;
        {
            var (t, _) = ftype._<ж<ast.Ellipsis>>(ᐧ); if (t != nil) {
                ftype = t.Value.Elt;
                if (variadicOk && i == len(list.List) - 1 && len((~field).Names) <= 1){
                    variadic = true;
                } else {
                    Ꮡcheck.softErrorf(new ast_Ellipsisжpositioner(t), MisplacedDotDotDot, "can only use ... with final parameter in list"u8);
                }
            }
        }
        // ignore ... and continue
        var typ = Ꮡcheck.varType(ftype);
        // The parser ensures that f.Tag is nil and we don't
        // care if a constructed AST contains a non-nil tag.
        if (len((~field).Names) > 0){
            // named parameter
            foreach (var (_, name) in (~field).Names) {
                if ((~name).Name == ""u8) {
                    Ꮡcheck.error(new ast_Identжpositioner(name), InvalidSyntaxTree, anonymousParameterˢ);
                }
                // ok to continue
                var par = NewParam(name.Pos(), check.pkg, (~name).Name, typ);
                // named parameter is declared by caller
                names = append(names, name);
                @params = append(@params, par);
            }
            named = true;
        } else {
            // anonymous parameter
            var par = NewParam(ftype.Pos(), check.pkg, ""u8, typ);
            check.recordImplicit(new ast.FieldжNode(field), new VarжObject(par));
            names = append(names, (ж<ast.Ident>)(nil));
            @params = append(@params, par);
            anonymous = true;
        }
    }
    if (named && anonymous) {
        Ꮡcheck.error(new ast_FieldListжpositioner(Ꮡlist), InvalidSyntaxTree, listContainsBothNamedAndˢ);
    }
    // ok to continue
    // For a variadic function, change the last parameter's type from T to []T.
    // Since we type-checked T rather than ...T, we also need to retro-actively
    // record the type for ...T.
    if (variadic) {
        var last = @params[len(@params) - 1];
        last.Value.typ = new SliceжΔType(Ꮡ(new Slice(elem: (~last).typ)));
        check.recordTypeAndValue((~list.List[len(list.List) - 1]).Type, typexpr, (~last).typ, default!);
    }
    return (names, @params, variadic);
}

// declareParams declares each named parameter in the current scope.
internal static void declareParams(this ж<Checker> Ꮡcheck, slice<ж<ast.Ident>> names, slice<ж<Var>> @params, tokenꓸPos scopePos) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    foreach (var (i, name) in names) {
        if (name != nil && (~name).Name != ""u8) {
            Ꮡcheck.declare(check.scope, name, new VarжObject(@params[i]), scopePos);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsafePointerˢ = "unsafe.Pointer"u8;
internal static readonly @string pointerOrInterfaceTypeˢ = "pointer or interface type"u8;

// validRecv verifies that the receiver satisfies its respective spec requirements
// and reports an error otherwise.
internal static void validRecv(this ж<Checker> Ꮡcheck, positioner pos, ж<Var> Ꮡrecv) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var recv = ref Ꮡrecv.DerefOrNull();

    // spec: "The receiver type must be of the form T or *T where T is a type name."
    var (rtyp, _) = deref(recv.typ);
    var atyp = Unalias(rtyp);
    if (!isValid(atyp)) {
        return; // error was reported before
    }
    // spec: "The type denoted by T is called the receiver base type; it must not
    // be a pointer or interface type and it must be declared in the same package
    // as the method."
    switch (atyp.type()) {
    case ж<Named> T: {
        if ((~(~T).obj).pkg != check.pkg || isCGoTypeObj(check.fset, ref ((~T).obj).DerefOrNull())) {
            Ꮡcheck.errorf(pos, InvalidRecv, "cannot define new methods on non-local type %s"u8, rtyp);
            break;
        }
        @string cause = default!;
        var switchᴛ13 = T.under();
        switch (switchᴛ13.type()) {
        case ж<Basic> u: {
            if ((~u).kind == UnsafePointer) {
                // unsafe.Pointer is treated like a regular pointer
                cause = unsafePointerˢ;
            }
            break;
        }
        case ж<Pointer> _:
        case ж<Interface> _: {
            var u = switchᴛ13;
            cause = pointerOrInterfaceTypeˢ;
            break;
        }
        case ж<TypeParam> u: {
            throw panic("unreachable");
            break;
        }}
        if (cause != ""u8) {
            // The underlying type of a receiver base type cannot be a
            // type parameter: "type T[P any] P" is not a valid declaration.
            Ꮡcheck.errorf(pos, InvalidRecv, "invalid receiver type %s (%s)"u8, rtyp, cause);
        }
        break;
    }
    case ж<Basic> T: {
        Ꮡcheck.errorf(pos, InvalidRecv, "cannot define new methods on non-local type %s"u8, rtyp);
        break;
    }
    default: {
        var T = atyp;
        Ꮡcheck.errorf(pos, InvalidRecv, "invalid receiver type %s"u8, recv.typ);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ctypeˢ = "_Ctype_"u8;
internal static readonly @string cgoˢ = "_cgo_"u8;

// isCGoTypeObj reports whether the given type name was created by cgo.
internal static bool isCGoTypeObj(ж<token.FileSet> Ꮡfset, ref TypeName obj) {
    return strings.HasPrefix(obj.name, ctypeˢ) || strings.HasPrefix(filepath.Base(Ꮡfset.File(obj.pos).Name()), cgoˢ);
}

} // end types_package
