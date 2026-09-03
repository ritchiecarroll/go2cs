// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements typechecking of call and selector expressions.
namespace go.go;

using ast = global::go.go.ast_package;
using typeparams = global::go.go.@internal.typeparams_package;
using token = global::go.go.token_package;
using static global::go.@internal.types.errors_package;
using strings = strings_package;
using constant = global::go.go.constant_package;
using errors = global::go.@internal.types.errors_package;
using global::go.go;
using global::go.go.@internal;
using ꓸꓸꓸastꓸExpr = Span<global::go.go.ast_package.Expr>;

partial class types_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸgoꓸinternalꓸtypeparams() {
    builtin.initPackage(typeof(global::go.go.@internal.typeparams_package));
}

// funcInst type-checks a function instantiation.
// The incoming x must be a generic function.
// If ix != nil, it provides some or all of the type arguments (ix.Indices).
// If target != nil, it may be used to infer missing type arguments of x, if any.
// At least one of T or ix must be provided.
//
// There are two modes of operation:
//
//  1. If infer == true, funcInst infers missing type arguments as needed and
//     instantiates the function x. The returned results are nil.
//
//  2. If infer == false and inst provides all type arguments, funcInst
//     instantiates the function x. The returned results are nil.
//     If inst doesn't provide enough type arguments, funcInst returns the
//     available arguments and the corresponding expression list; x remains
//     unchanged.
//
// If an error (other than a version error) occurs in any case, it is reported
// and x.mode is set to invalid.
internal static (slice<ΔType>, slice<ast.Expr>) funcInst(this ж<Checker> Ꮡcheck, ж<target> ᏑT, tokenꓸPos pos, ж<operand> Ꮡx, ж<typeparams.IndexExpr> Ꮡix, bool infer) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var T = ref ᏑT.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var ix = ref Ꮡix.DerefOrNull();

    assert(ᏑT != nil || Ꮡix != nil);
    positioner instErrPos = default!;
    if (Ꮡix != nil){
        instErrPos = inNode(ix.Orig, ix.Lbrack);
        x.expr = ix.Orig; // if we don't have an index expression, keep the existing expression of x
    } else {
        instErrPos = ((atPos)pos);
    }
    var versionErr = !Ꮡcheck.verifyVersionf(instErrPos, go1_18, "function instantiation"u8);
    // targs and xlist are the type arguments and corresponding type expressions, or nil.
    slice<ΔType> targs = default!;
    slice<ast.Expr> xlist = default!;
    if (Ꮡix != nil) {
        xlist = ix.Indices;
        targs = Ꮡcheck.typeList(xlist);
        if (targs == default!) {
            x.mode = invalid;
            return (default!, default!);
        }
        assert(len(targs) == len(xlist));
    }
    // Check the number of type arguments (got) vs number of type parameters (want).
    // Note that x is a function value, not a type expression, so we don't need to
    // call under below.
    var sig = x.typ._<ж<ΔSignature>>();
    nint got = len(targs);
    nint want = sig.TypeParams().Len();
    if (got > want) {
        // Providing too many type arguments is always an error.
        Ꮡcheck.errorf(new ast_Exprᴠpositioner(ix.Indices[got - 1]), WrongTypeArgCount, "got %d type arguments but want %d"u8, got, want);
        x.mode = invalid;
        return (default!, default!);
    }
    if (got < want) {
        if (!infer) {
            return (targs, xlist);
        }
        // If the uninstantiated or partially instantiated function x is used in
        // an assignment (tsig != nil), infer missing type arguments by treating
        // the assignment
        //
        //    var tvar tsig = x
        //
        // like a call g(tvar) of the synthetic generic function g
        //
        //    func g[type_parameters_of_x](func_type_of_x)
        //
        slice<ж<operand>> args = default!;
        slice<ж<Var>> @params = default!;
        bool reverse = default!;
        if (ᏑT != nil && (~sig).tparams != nil) {
            if (!versionErr && !Ꮡcheck.allowVersion(instErrPos, go1_21)) {
                if (Ꮡix != nil){
                    Ꮡcheck.versionErrorf(instErrPos, go1_21, "partially instantiated function in assignment"u8);
                } else {
                    Ꮡcheck.versionErrorf(instErrPos, go1_21, "implicitly instantiated function in assignment"u8);
                }
            }
            var gsig = NewSignatureType(nil, default!, default!, (~sig).@params, (~sig).results, (~sig).variadic);
            @params = new ж<Var>[]{NewVar(x.Pos(), check.pkg, ""u8, new ΔSignatureжΔType(gsig))}.slice();
            // The type of the argument operand is tsig, which is the type of the LHS in an assignment
            // or the result type in a return statement. Create a pseudo-expression for that operand
            // that makes sense when reported in error messages from infer, below.
            var expr = ast.NewIdent(T.desc);
            expr.Value.NamePos = x.Pos(); // correct position
            args = new ж<operand>[]{Ꮡ(new operand(mode: value, expr: new ast.IdentжExpr(expr), typ: new ΔSignatureжΔType(T.sig)))}.slice();
            reverse = true;
        }
        // Rename type parameters to avoid problems with recursive instantiations.
        // Note that NewTuple(params...) below is (*Tuple)(nil) if len(params) == 0, as desired.
        var (tparams, params2) = Ꮡcheck.renameTParams(pos, sig.TypeParams().list(), new TupleжΔType(NewTuple(@params.ꓸꓸꓸ)));
        var err = Ꮡcheck.newError(CannotInferTypeArgs);
        targs = Ꮡcheck.infer(((atPos)pos), tparams, targs, params2._<ж<Tuple>>(), args, reverse, err);
        if (targs == default!) {
            if (!err.empty()) {
                err.report();
            }
            x.mode = invalid;
            return (default!, default!);
        }
        got = len(targs);
    }
    assert(got == want);
    // instantiate function signature
    sig = Ꮡcheck.instantiateSignature(x.Pos(), x.expr, sig, targs, xlist);
    x.typ = new ΔSignatureжΔType(sig);
    x.mode = value;
    return (default!, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string instantiatingSignatureSˢ = "-- instantiating signature %s with %s"u8;
internal static readonly @string sUnderSˢ = "=> %s (under = %s)"u8;

internal static ж<ΔSignature> /*res*/ instantiateSignature(this ж<Checker> Ꮡcheck, tokenꓸPos pos, ast.Expr expr, ж<ΔSignature> Ꮡtyp, slice<ΔType> targs, slice<ast.Expr> xlist) {
    ж<ΔSignature> res = default!;
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();
        ref var typ = ref Ꮡtyp.DerefOrNull();

        assert(Ꮡcheck != nil);
        assert(len(targs) == typ.TypeParams().Len());
        if ((~check.conf)._Trace) {
            Ꮡcheck.trace(pos, instantiatingSignatureSˢ, Ꮡtyp.OrTypedNil(), targs);
            check.indent++;
            defer(() => {
                Ꮡcheck.Value.indent--;
                Ꮡcheck.trace(pos, sUnderSˢ, res.OrTypedNil(), res.Underlying());
            }, ref ᒐ);
        }
        var inst = Ꮡcheck.instance(pos, new ΔSignatureжΔgenericType(Ꮡtyp), targs, nil, check.context())._<ж<ΔSignature>>();
        assert(inst.TypeParams().Len() == 0); // signature is not generic anymore
        check.recordInstance(expr, targs, new ΔSignatureжΔType(inst));
        assert(len(xlist) <= len(targs));
        // verify instantiation lazily (was go.dev/issue/50450)
        var targsʗ1 = targs;
        var xlistʗ1 = xlist;
        check.later(() => {
            var tparams = Ꮡtyp.Value.TypeParams().list();
            {
                var (i, err) = Ꮡcheck.verify(pos, tparams, targsʗ1, Ꮡcheck.Value.context()); if (err != default!){
                    // best position for error reporting
                    tokenꓸPos posΔ1 = pos;
                    if (i < len(xlistʗ1)) {
                        posΔ1 = xlistʗ1[i].Pos();
                    }
                    Ꮡcheck.softErrorf(((atPos)posΔ1), InvalidTypeArg, "%s"u8, err);
                } else {
                    Ꮡcheck.of(Checker.Ꮡmono).recordInstance(Ꮡcheck.Value.pkg, pos, tparams, targsʗ1, xlistʗ1);
                }
            }
        }).describef(((atPos)pos), "verify instantiation"u8);
        res = inst;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return res;
}

internal static exprKind callExpr(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.CallExpr> Ꮡcall) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var call = ref Ꮡcall.DerefOrNull();

    var ix = typeparams.UnpackIndexExpr(call.Fun);
    if (ix != nil){
        if (Ꮡcheck.indexExpr(Ꮡx, ix)){
            // Delay function instantiation to argument checking,
            // where we combine type and value arguments for type
            // inference.
            assert(x.mode == value);
        } else {
            ix = default!;
        }
        x.expr = call.Fun;
        check.record(Ꮡx);
    } else {
        Ꮡcheck.exprOrType(Ꮡx, call.Fun, true);
    }
    // x.typ may be generic
    var exprᴛ1 = x.mode;
    if (exprᴛ1 == invalid) {
        Ꮡcheck.use(call.Args.ꓸꓸꓸ);
        x.expr = new ast.CallExprжExpr(Ꮡcall);
        return statement;
    }
    if (exprᴛ1 == typexpr) {
        Ꮡcheck.nonGeneric(nil, // conversion
 Ꮡx);
        if (x.mode == invalid) {
            return Δconversion;
        }
        var T = x.typ;
        x.mode = invalid;
        {
            nint n = len(call.Args);
            switch (n) {
            case 0: {
                Ꮡcheck.errorf(inNode(new ast.CallExprжNode(Ꮡcall), call.Rparen), WrongArgCount, "missing argument in conversion to %s"u8, T);
                break;
            }
            case 1: {
                Ꮡcheck.expr(nil, Ꮡx, call.Args[0]);
                if (x.mode != invalid) {
                    if (hasDots(ref (Ꮡcall).DerefOrNull())) {
                        Ꮡcheck.errorf(new ast_Exprᴠpositioner(call.Args[0]), BadDotDotDotSyntax, "invalid use of ... in conversion to %s"u8, T);
                        break;
                    }
                    {
                        var (t, _) = under(T)._<ж<Interface>>(ᐧ); if (t != nil && !isTypeParam(T)) {
                            if (!t.IsMethodSet()) {
                                Ꮡcheck.errorf(new ast_CallExprжpositioner(Ꮡcall), MisplacedConstraintIface, "cannot use interface %s in conversion (contains specific type constraints or is comparable)"u8, T);
                                break;
                            }
                        }
                    }
                    Ꮡcheck.conversion(Ꮡx, T);
                }
                break;
            }
            default: {
                Ꮡcheck.use(call.Args.ꓸꓸꓸ);
                Ꮡcheck.errorf(new ast_Exprᴠpositioner(call.Args[n - 1]), WrongArgCount, "too many arguments in conversion to %s"u8, T);
                break;
            }}
        }

        x.expr = new ast.CallExprжExpr(Ꮡcall);
        return Δconversion;
    }
    if (exprᴛ1 == Δbuiltinᴛ) {
        builtinId id = x.id;
        if (!Ꮡcheck.Δbuiltin(Ꮡx, // no need to check for non-genericity here
 Ꮡcall, id)) {
            x.mode = invalid;
        }
        x.expr = new ast.CallExprжExpr(Ꮡcall);
        if (x.mode != invalid && x.mode != constant_) {
            // a non-constant result implies a function call
            check.hasCallOrRecv = true;
        }
        return predeclaredFuncs[id].kind;
    }

    // ordinary function/method call
    // signature may be generic
    var cgocall = x.mode == cgofunc;
    // a type parameter may be "called" if all types have the same signature
    var (sig, _) = coreType(x.typ)._<ж<ΔSignature>>(ᐧ);
    if (sig == nil) {
        Ꮡcheck.errorf(new operandжpositioner(Ꮡx), InvalidCall, invalidOp + "cannot call non-function %s", Ꮡx.OrTypedNil());
        x.mode = invalid;
        x.expr = new ast.CallExprжExpr(Ꮡcall);
        return statement;
    }
    // Capture wasGeneric before sig is potentially instantiated below.
    var wasGeneric = sig.TypeParams().Len() > 0;
    // evaluate type arguments, if any
    slice<ast.Expr> xlist = default!;
    slice<ΔType> targs = default!;
    if (ix != nil) {
        xlist = ix.Value.Indices;
        targs = Ꮡcheck.typeList(xlist);
        if (targs == default!) {
            Ꮡcheck.use(call.Args.ꓸꓸꓸ);
            x.mode = invalid;
            x.expr = new ast.CallExprжExpr(Ꮡcall);
            return statement;
        }
        assert(len(targs) == len(xlist));
        // check number of type arguments (got) vs number of type parameters (want)
        nint got = len(targs);
        nint want = sig.TypeParams().Len();
        if (got > want) {
            Ꮡcheck.errorf(new ast_Exprᴠpositioner(xlist[want]), WrongTypeArgCount, "got %d type arguments but want %d"u8, got, want);
            Ꮡcheck.use(call.Args.ꓸꓸꓸ);
            x.mode = invalid;
            x.expr = new ast.CallExprжExpr(Ꮡcall);
            return statement;
        }
        // If sig is generic and all type arguments are provided, preempt function
        // argument type inference by explicitly instantiating the signature. This
        // ensures that we record accurate type information for sig, even if there
        // is an error checking its arguments (for example, if an incorrect number
        // of arguments is supplied).
        if (got == want && want > 0) {
            Ꮡcheck.verifyVersionf(((atPos)(~ix).Lbrack), go1_18, "function instantiation"u8);
            sig = Ꮡcheck.instantiateSignature(ix.Pos(), (~ix).Orig, sig, targs, xlist);
            // targs have been consumed; proceed with checking arguments of the
            // non-generic signature.
            targs = default!;
            xlist = default!;
        }
    }
    // evaluate arguments
    var (args, atargs, atxlist) = Ꮡcheck.genericExprList(call.Args);
    sig = Ꮡcheck.arguments(Ꮡcall, sig, targs, xlist, args, atargs, atxlist);
    if (wasGeneric && sig.TypeParams().Len() == 0) {
        // Update the recorded type of call.Fun to its instantiated type.
        check.recordTypeAndValue(call.Fun, value, new ΔSignatureжΔType(sig), default!);
    }
    // determine result
    switch ((~sig).results.Len()) {
    case 0: {
        x.mode = novalue;
        break;
    }
    case 1: {
        if (cgocall){
            x.mode = commaerr;
        } else {
            x.mode = value;
        }
        x.typ = (~(~sig).results).vars[0].Value.typ; // unpack tuple
        break;
    }
    default: {
        x.mode = value;
        x.typ = new TupleжΔType(sig.Value.results);
        break;
    }}

    x.expr = new ast.CallExprжExpr(Ꮡcall);
    check.hasCallOrRecv = true;
    // if type inference failed, a parameterized result must be invalidated
    // (operands cannot have a parameterized type)
    if (x.mode == value && sig.TypeParams().Len() > 0 && isParameterized(sig.TypeParams().list(), x.typ)) {
        x.mode = invalid;
    }
    return statement;
}

// exprList evaluates a list of expressions and returns the corresponding operands.
// A single-element expression list may evaluate to multiple operands.
internal static slice<ж<operand>> /*xlist*/ exprList(this ж<Checker> Ꮡcheck, slice<ast.Expr> elist) {
    slice<ж<operand>> xlist = default!;

    {
        nint n = len(elist); if (n == 1){
            (xlist, _) = Ꮡcheck.multiExpr(elist[0], false);
        } else 
        if (n > 1) {
            // multiple (possibly invalid) values
            xlist = new slice<ж<operand>>(n);
            foreach (var (i, e) in elist) {
                ref var x = ref heap(new operand(), out var Ꮡx);
                Ꮡcheck.expr(nil, Ꮡx, e);
                xlist[i] = Ꮡx;
            }
        }
    }
    return xlist;
}

// genericExprList is like exprList but result operands may be uninstantiated or partially
// instantiated generic functions (where constraint information is insufficient to infer
// the missing type arguments) for Go 1.21 and later.
// For each non-generic or uninstantiated generic operand, the corresponding targsList and
// xlistList elements do not exist (targsList and xlistList are nil) or the elements are nil.
// For each partially instantiated generic function operand, the corresponding targsList and
// xlistList elements are the operand's partial type arguments and type expression lists.
internal static (slice<ж<operand>> resList, slice<slice<ΔType>> targsList, slice<slice<ast.Expr>> xlistList) genericExprList(this ж<Checker> Ꮡcheck, slice<ast.Expr> elist) {
    slice<ж<operand>> resList = default!;
    slice<slice<ΔType>> targsList = default!;
    slice<slice<ast.Expr>> xlistList = default!;
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();

        if (debug) {
            defer(() => {
                // targsList and xlistList must have matching lengths
                assert(len(targsList) == len(xlistList));
                // type arguments must only exist for partially instantiated functions
                foreach (var (i, x) in resList) {
                    if (i < len(targsList)) {
                        {
                            nint nΔ1 = len(targsList[i]); if (nΔ1 > 0) {
                                // x must be a partially instantiated function
                                assert(nΔ1 < (~x).typ._<ж<ΔSignature>>().TypeParams().Len());
                            }
                        }
                    }
                }
            }, ref ᒐ);
        }
        // Before Go 1.21, uninstantiated or partially instantiated argument functions are
        // nor permitted. Checker.funcInst must infer missing type arguments in that case.
        var infer = true; // for -lang < go1.21
        nint n = len(elist);
        if (n > 0 && Ꮡcheck.allowVersion(new ast_Exprᴠpositioner(elist[0]), go1_21)) {
            infer = false;
        }
        if (n == 1){
            // single value (possibly a partially instantiated function), or a multi-valued expression
            var e = elist[0];
            ref var x = ref heap(new operand(), out var Ꮡx);
            {
                var ix = typeparams.UnpackIndexExpr(e); if (ix != nil && Ꮡcheck.indexExpr(Ꮡx, ix)){
                    // x is a generic function.
                    var (targs, xlist) = Ꮡcheck.funcInst(nil, x.Pos(), Ꮡx, ix, infer);
                    if (targs != default!){
                        // x was not instantiated: collect the (partial) type arguments.
                        targsList = new slice<ΔType>[]{targs}.slice();
                        xlistList = new slice<ast.Expr>[]{xlist}.slice();
                        // Update x.expr so that we can record the partially instantiated function.
                        x.expr = ix.Value.Orig;
                    } else {
                        // x was instantiated: we must record it here because we didn't
                        // use the usual expression evaluators.
                        check.record(Ꮡx);
                    }
                    resList = new ж<operand>[]{Ꮡx}.slice();
                } else {
                    // x is not a function instantiation (it may still be a generic function).
                    Ꮡcheck.rawExpr(nil, Ꮡx, e, default!, true);
                    Ꮡcheck.exclude(Ꮡx, (nuint)((nuint)(UntypedInt)((1 << (int)(byte)(novalue)) | (1 << (int)(byte)(Δbuiltinᴛ))) | (nuint)(1 << (int)(byte)(typexpr))));
                    {
                        var (t, ok) = x.typ._<ж<Tuple>>(ᐧ); if (ok && x.mode != invalid){
                            // x is a function call returning multiple values; it cannot be generic.
                            resList = new slice<ж<operand>>(t.Len());
                            foreach (var (i, v) in (~t).vars) {
                                resList[i] = Ꮡ(new operand(mode: value, expr: e, typ: (~v).typ));
                            }
                        } else {
                            // x is exactly one value (possibly invalid or uninstantiated generic function).
                            resList = new ж<operand>[]{Ꮡx}.slice();
                        }
                    }
                }
            }
        } else 
        if (n > 1) {
            // multiple values
            resList = new slice<ж<operand>>(n);
            targsList = new slice<slice<ΔType>>(n);
            xlistList = new slice<slice<ast.Expr>>(n);
            foreach (var (i, e) in elist) {
                ref var x = ref heap(new operand(), out var Ꮡx);
                {
                    var ix = typeparams.UnpackIndexExpr(e); if (ix != nil && Ꮡcheck.indexExpr(Ꮡx, ix)){
                        // x is a generic function.
                        var (targs, xlist) = Ꮡcheck.funcInst(nil, x.Pos(), Ꮡx, ix, infer);
                        if (targs != default!){
                            // x was not instantiated: collect the (partial) type arguments.
                            targsList[i] = targs;
                            xlistList[i] = xlist;
                            // Update x.expr so that we can record the partially instantiated function.
                            x.expr = ix.Value.Orig;
                        } else {
                            // x was instantiated: we must record it here because we didn't
                            // use the usual expression evaluators.
                            check.record(Ꮡx);
                        }
                    } else {
                        // x is exactly one value (possibly invalid or uninstantiated generic function).
                        Ꮡcheck.genericExpr(Ꮡx, e);
                    }
                }
                resList[i] = Ꮡx;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (resList, targsList, xlistList);
}

// arguments type-checks arguments passed to a function call with the given signature.
// The function and its arguments may be generic, and possibly partially instantiated.
// targs and xlist are the function's type arguments (and corresponding expressions).
// args are the function arguments. If an argument args[i] is a partially instantiated
// generic function, atargs[i] and atxlist[i] are the corresponding type arguments
// (and corresponding expressions).
// If the callee is variadic, arguments adjusts its signature to match the provided
// arguments. The type parameters and arguments of the callee and all its arguments
// are used together to infer any missing type arguments, and the callee and argument
// functions are instantiated as necessary.
// The result signature is the (possibly adjusted and instantiated) function signature.
// If an error occurred, the result signature is the incoming sig.
internal static ж<ΔSignature> /*rsig*/ arguments(this ж<Checker> Ꮡcheck, ж<ast.CallExpr> Ꮡcall, ж<ΔSignature> Ꮡsig, slice<ΔType> targs, slice<ast.Expr> xlist, slice<ж<operand>> args, slice<slice<ΔType>> atargs, slice<slice<ast.Expr>> atxlist) {
    ж<ΔSignature> rsig = default!;

    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var call = ref Ꮡcall.DerefOrNull();
    ref var sig = ref Ꮡsig.DerefOrNull();
    rsig = Ꮡsig;
    // Function call argument/parameter count requirements
    //
    //               | standard call    | dotdotdot call |
    // --------------+------------------+----------------+
    // standard func | nargs == npars   | invalid        |
    // --------------+------------------+----------------+
    // variadic func | nargs >= npars-1 | nargs == npars |
    // --------------+------------------+----------------+
    nint nargs = len(args);
    nint npars = sig.@params.Len();
    var ddd = hasDots(ref (Ꮡcall).DerefOrNull());
    // set up parameters
    var sigParams = sig.@params; // adjusted for variadic functions (may be nil for empty parameter lists!)
    var adjusted = false; // indicates if sigParams is different from sig.params
    if (sig.variadic){
        if (ddd){
            // variadic_func(a, b, c...)
            if (len(call.Args) == 1 && nargs > 1) {
                // f()... is not permitted if f() is multi-valued
                Ꮡcheck.errorf(inNode(new ast.CallExprжNode(Ꮡcall), call.Ellipsis), InvalidDotDotDot, "cannot use ... with %d-valued %s"u8, nargs, call.Args[0]);
                return rsig;
            }
        } else {
            // variadic_func(a, b, c)
            if (nargs >= npars - 1){
                // Create custom parameters for arguments: keep
                // the first npars-1 parameters and add one for
                // each argument mapping to the ... parameter.
                var vars = new slice<ж<Var>>(npars - 1); // npars > 0 for variadic functions
                copy(vars, (~sig.@params).vars);
                var last = (~sig.@params).vars[npars - 1];
                var typ = (~last).typ._<ж<Slice>>().Value.elem;
                while (len(vars) < nargs) {
                    vars = append(vars, NewParam((~last).pos, (~last).pkg, (~last).name, typ));
                }
                sigParams = NewTuple(vars.ꓸꓸꓸ); // possibly nil!
                adjusted = true;
                npars = nargs;
            } else {
                // nargs < npars-1
                npars--; // for correct error message below
            }
        }
    } else {
        if (ddd) {
            // standard_func(a, b, c...)
            Ꮡcheck.errorf(inNode(new ast.CallExprжNode(Ꮡcall), call.Ellipsis), NonVariadicDotDotDot, "cannot use ... in call to non-variadic %s"u8, call.Fun);
            return rsig;
        }
    }
    // standard_func(a, b, c)
    // check argument count
    if (nargs != npars) {
        positioner at = new ast_CallExprжpositioner(Ꮡcall);
        @string qualifier = notEnoughˢ;
        if (nargs > npars){
            at = new ast_Exprᴠpositioner(args[npars].Value.expr); // report at first extra argument
            qualifier = tooManyˢ;
        } else {
            at = ((atPos)call.Rparen); // report at closing )
        }
        // take care of empty parameter lists represented by nil tuples
        slice<ж<Var>> @params = default!;
        if (sig.@params != nil) {
            @params = sig.@params.Value.vars;
        }
        var err = Ꮡcheck.newError(WrongArgCount);
        err.addf(at, "%s arguments in call to %s"u8, qualifier, call.Fun);
        err.addf(noposn, "have %s"u8, Ꮡcheck.typesSummary(operandTypes(args), false));
        err.addf(noposn, "want %s"u8, Ꮡcheck.typesSummary(varTypes(@params), sig.variadic));
        err.report();
        return rsig;
    }
    // collect type parameters of callee and generic function arguments
    slice<ж<TypeParam>> tparams = default!;
    // collect type parameters of callee
    nint n = sig.TypeParams().Len();
    if (n > 0) {
        if (!Ꮡcheck.allowVersion(new ast_CallExprжpositioner(Ꮡcall), go1_18)) {
            switch (call.Fun.type()) {
            case ж<ast.IndexExpr> _:
            case ж<ast.IndexListExpr> _: {
                var ix = typeparams.UnpackIndexExpr(call.Fun);
                Ꮡcheck.versionErrorf(inNode(call.Fun, (~ix).Lbrack), go1_18, "function instantiation"u8);
                break;
            }
            default: {
                Ꮡcheck.versionErrorf(inNode(new ast.CallExprжNode(Ꮡcall), call.Lparen), go1_18, "implicit function instantiation"u8);
                break;
            }}

        }
        // rename type parameters to avoid problems with recursive calls
        ΔType tmp = default!;
        (tparams, tmp) = Ꮡcheck.renameTParams(call.Pos(), sig.TypeParams().list(), new TupleжΔType(sigParams));
        sigParams = tmp._<ж<Tuple>>();
        // make sure targs and tparams have the same length
        while (len(targs) < len(tparams)) {
            targs = append(targs, (ΔType)(default!));
        }
    }
    assert(len(tparams) == len(targs));
    // collect type parameters from generic function arguments
    slice<nint> genericArgs = default!; // indices of generic function arguments
    if (enableReverseTypeInference) {
        foreach (var (i, arg) in args) {
            // generic arguments cannot have a defined (*Named) type - no need for underlying type below
            {
                var (asig, _) = (~arg).typ._<ж<ΔSignature>>(ᐧ); if (asig != nil && asig.TypeParams().Len() > 0) {
                    // The argument type is a generic function signature. This type is
                    // pointer-identical with (it's copied from) the type of the generic
                    // function argument and thus the function object.
                    // Before we change the type (type parameter renaming, below), make
                    // a clone of it as otherwise we implicitly modify the object's type
                    // (go.dev/issues/63260).
                    asig = clone<ΔSignature>(asig);
                    // Rename type parameters for cases like f(g, g); this gives each
                    // generic function argument a unique type identity (go.dev/issues/59956).
                    // TODO(gri) Consider only doing this if a function argument appears
                    //           multiple times, which is rare (possible optimization).
                    var (atparams, tmp) = Ꮡcheck.renameTParams(call.Pos(), asig.TypeParams().list(), new ΔSignatureжΔType(asig));
                    asig = tmp._<ж<ΔSignature>>();
                    asig.Value.tparams = Ꮡ(new TypeParamList(atparams)); // renameTParams doesn't touch associated type parameters
                    arg.Value.typ = new ΔSignatureжΔType(asig); // new type identity for the function argument
                    tparams = appendꓸꓸꓸ(tparams, atparams);
                    // add partial list of type arguments, if any
                    if (i < len(atargs)) {
                        targs = appendꓸꓸꓸ(targs, atargs[i]);
                    }
                    // make sure targs and tparams have the same length
                    while (len(targs) < len(tparams)) {
                        targs = append(targs, (ΔType)(default!));
                    }
                    genericArgs = append(genericArgs, i);
                }
            }
        }
    }
    assert(len(tparams) == len(targs));
    // at the moment we only support implicit instantiations of argument functions
    _ = len(genericArgs) > 0 && Ꮡcheck.verifyVersionf(new operandжpositioner(args[genericArgs[0]]), go1_21, "implicitly instantiated function as argument"u8);
    // tparams holds the type parameters of the callee and generic function arguments, if any:
    // the first n type parameters belong to the callee, followed by mi type parameters for each
    // of the generic function arguments, where mi = args[i].typ.(*Signature).TypeParams().Len().
    // infer missing type arguments of callee and function arguments
    if (len(tparams) > 0) {
        var err = Ꮡcheck.newError(CannotInferTypeArgs);
        targs = Ꮡcheck.infer(new ast_CallExprжpositioner(Ꮡcall), tparams, targs, sigParams, args, false, err);
        if (targs == default!) {
            // TODO(gri) If infer inferred the first targs[:n], consider instantiating
            //           the call signature for better error messages/gopls behavior.
            //           Perhaps instantiate as much as we can, also for arguments.
            //           This will require changes to how infer returns its results.
            if (!err.empty()) {
                Ꮡcheck.errorf(err.posn(), CannotInferTypeArgs, "in call to %s, %s"u8, call.Fun, err.msg());
            }
            return rsig;
        }
        // update result signature: instantiate if needed
        if (n > 0) {
            rsig = Ꮡcheck.instantiateSignature(call.Pos(), call.Fun, Ꮡsig, targs[..(int)(n)], xlist);
            // If the callee's parameter list was adjusted we need to update (instantiate)
            // it separately. Otherwise we can simply use the result signature's parameter
            // list.
            if (adjusted){
                sigParams = Ꮡcheck.subst(call.Pos(), new TupleжΔType(sigParams), makeSubstMap(tparams[..(int)(n)], targs[..(int)(n)]), nil, check.context())._<ж<Tuple>>();
            } else {
                sigParams = rsig.Value.@params;
            }
        }
        // compute argument signatures: instantiate if needed
        nint j = n;
        foreach (var (_, i) in genericArgs) {
            var arg = args[i];
            var asig = (~arg).typ._<ж<ΔSignature>>();
            nint k = j + asig.TypeParams().Len();
            // targs[j:k] are the inferred type arguments for asig
            arg.Value.typ = new ΔSignatureжΔType(Ꮡcheck.instantiateSignature(call.Pos(), (~arg).expr, asig, targs[(int)(j)..(int)(k)], default!)); // TODO(gri) provide xlist if possible (partial instantiations)
            check.record(arg); // record here because we didn't use the usual expr evaluators
            j = k;
        }
    }
    // check arguments
    if (len(args) > 0) {
        @string context = Ꮡcheck.sprintf("argument to %s"u8, call.Fun);
        foreach (var (i, a) in args) {
            Ꮡcheck.assignment(a, (~(~sigParams).vars[i]).typ, context);
        }
    }
    return rsig;
}

// actually a pointer to the var
// function to evaluate the expanded expression
internal static array<@string> cgoPrefixes = new @string[]{
    "_Ciconst_"u8,
    "_Cfconst_"u8,
    "_Csconst_"u8,
    "_Ctype_"u8,
    "_Cvar_"u8,
    "_Cfpvar_fp_"u8,
    "_Cfunc_"u8,
    "_Cmacro_"u8
}.array();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cMallocˢ = "_CMalloc"u8;
internal static readonly @string cvarˢ = "_Cvar_"u8;
internal static readonly @string cmacroˢ = "_Cmacro_"u8;
internal static readonly @string vUnexpectedObjectVˢ = "%v: unexpected object %v"u8;
internal static readonly @string illegalCycleInMethodˢ = "illegal cycle in method declaration"u8;
internal static readonly @string vSVSˢ = "%v: (%s).%v -> %s"u8;

internal static void selector(this ж<Checker> Ꮡcheck, ж<operand> Ꮡx, ж<ast.SelectorExpr> Ꮡe, ж<TypeName> Ꮡdef, bool wantType) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();
    ref var def = ref Ꮡdef.DerefOrNull();

    // these must be declared before the "goto Error" statements
    Object obj = default!;
    
    slice<nint> index = default!;
    
    bool indirect = default!;
    @string sel = e.Sel.Value.Name;
    // If the identifier refers to a package, handle everything here
    // so we don't need a "package" mode for operands: package names
    // can only appear in qualified identifiers which are mapped to
    // selector expressions.
    {
        var (ident, ok) = e.X._<ж<ast.Ident>>(ᐧ); if (ok) {
            var objΔ1 = Ꮡcheck.of(Checker.Ꮡenvironment).lookup((~ident).Name);
            {
                var (pname, _) = objΔ1._<ж<PkgName>>(ᐧ); if (pname != nil) {
                    assert((~pname).pkg == check.pkg);
                    check.recordUse(ident, new PkgNameжObject(pname));
                    pname.Value.used = true;
                    var pkg = pname.Value.imported;
                    Object exp = default!;
                    var funcMode = value;
                    if ((~pkg).cgo){
                        // cgo special cases C.malloc: it's
                        // rewritten to _CMalloc and does not
                        // support two-result calls.
                        if (sel == "malloc"u8){
                            sel = cMallocˢ;
                        } else {
                            funcMode = cgofunc;
                        }
                        foreach (var (_, prefix) in cgoPrefixes.ΔRangeSnapshot()) {
                            // cgo objects are part of the current package (in file
                            // _cgo_gotypes.go). Use regular lookup.
                            (_, exp) = check.scope.LookupParent(prefix + sel, check.pos);
                            if (exp != default!) {
                                break;
                            }
                        }
                        if (exp == default!) {
                            Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), UndeclaredImportedName, "undefined: %s"u8, ((ast.Expr)new ast.SelectorExprжExpr(Ꮡe))); // cast to ast.Expr to silence vet
                            goto ΔError;
                        }
                        Ꮡcheck.objDecl(exp, nil);
                    } else {
                        exp = (~pkg).scope.Lookup(sel);
                        if (exp == default!) {
                            if (!(~pkg).fake) {
                                Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), UndeclaredImportedName, "undefined: %s"u8, ((ast.Expr)new ast.SelectorExprжExpr(Ꮡe)));
                            }
                            goto ΔError;
                        }
                        if (!exp.Exported()) {
                            Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), UnexportedName, "name %s not exported by package %s"u8, sel, (~pkg).name);
                        }
                    }
                    // ok to continue
                    check.recordUse(e.Sel, exp);
                    // Simplified version of the code for *ast.Idents:
                    // - imported objects are always fully initialized
                    switch (exp.type()) {
                    case ж<Const> expΔ1: {
                        assert(expΔ1.Val() != default!);
                        x.mode = constant_;
                        x.typ = expΔ1.Value.typ;
                        x.val = expΔ1.Value.val;
                        break;
                    }
                    case ж<TypeName> expΔ1: {
                        x.mode = typexpr;
                        x.typ = expΔ1.Value.typ;
                        break;
                    }
                    case ж<Var> expΔ1: {
                        x.mode = variable;
                        x.typ = expΔ1.Value.typ;
                        if ((~pkg).cgo && strings.HasPrefix((~expΔ1).name, cvarˢ)) {
                            x.typ = x.typ._<ж<Pointer>>().Value.@base;
                        }
                        break;
                    }
                    case ж<Func> expΔ1: {
                        x.mode = funcMode;
                        x.typ = expΔ1.Value.typ;
                        if ((~pkg).cgo && strings.HasPrefix((~expΔ1).name, cmacroˢ)) {
                            x.mode = value;
                            x.typ = (~(~x.typ._<ж<ΔSignature>>()).results).vars[0].Value.typ;
                        }
                        break;
                    }
                    case ж<Builtin> expΔ1: {
                        x.mode = Δbuiltinᴛ;
                        x.typ = expΔ1.Value.typ;
                        x.id = expΔ1.Value.id;
                        break;
                    }
                    default: {
                        var expΔ1 = exp;
                        Ꮡcheck.dump(vUnexpectedObjectVˢ, e.Sel.Pos(), expΔ1);
                        throw panic("unreachable");
                        break;
                    }}
                    x.expr = new ast.SelectorExprжExpr(Ꮡe);
                    return;
                }
            }
        }
    }
    Ꮡcheck.exprOrType(Ꮡx, e.X, false);
    var exprᴛ1 = x.mode;
    if (exprᴛ1 == typexpr) {
        if (Ꮡdef != nil && AreEqual(def.typ, x.typ)) {
            // don't crash for "type T T.x" (was go.dev/issue/51509)
            Ꮡcheck.cycleError(new Object[]{new TypeNameжObject(def)}.slice(), 0);
            goto ΔError;
        }
    }
    else if (exprᴛ1 == Δbuiltinᴛ) {
        Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), // types2 uses the position of '.' for the error
 UncalledBuiltin, "cannot select on %s"u8, Ꮡx.OrTypedNil());
        goto ΔError;
    }
    else if (exprᴛ1 == invalid) {
        goto ΔError;
    }

    // Avoid crashing when checking an invalid selector in a method declaration
    // (i.e., where def is not set):
    //
    //   type S[T any] struct{}
    //   type V = S[any]
    //   func (fs *S[T]) M(x V.M) {}
    //
    // All codepaths below return a non-type expression. If we get here while
    // expecting a type expression, it is an error.
    //
    // See go.dev/issue/57522 for more details.
    //
    // TODO(rfindley): We should do better by refusing to check selectors in all cases where
    // x.typ is incomplete.
    if (wantType) {
        Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), NotAType, "%s is not a type"u8, ((ast.Expr)new ast.SelectorExprжExpr(Ꮡe)));
        goto ΔError;
    }
    (obj, index, indirect) = lookupFieldOrMethod(x.typ, x.mode == variable, check.pkg, sel, false);
    if (obj == default!) {
        // Don't report another error if the underlying type was invalid (go.dev/issue/49541).
        if (!isValid(under(x.typ))) {
            goto ΔError;
        }
        if (index != default!) {
            // TODO(gri) should provide actual type where the conflict happens
            Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), AmbiguousSelector, "ambiguous selector %s.%s"u8, x.expr, sel);
            goto ΔError;
        }
        if (indirect) {
            if (x.mode == typexpr){
                Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), InvalidMethodExpr, "invalid method expression %s.%s (needs pointer receiver (*%s).%s)"u8, x.typ, sel, x.typ, sel);
            } else {
                Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), InvalidMethodExpr, "cannot call pointer method %s on %s"u8, sel, x.typ);
            }
            goto ΔError;
        }
        @string why = default!;
        if (isInterfacePtr(x.typ)){
            why = Ꮡcheck.interfacePtrError(x.typ);
        } else {
            var (alt, _, _) = lookupFieldOrMethod(x.typ, x.mode == variable, check.pkg, sel, true);
            why = Ꮡcheck.lookupError(x.typ, sel, alt, false);
        }
        Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), MissingFieldOrMethod, "%s.%s undefined (%s)"u8, x.expr, sel, why);
        goto ΔError;
    }
    // methods may not have a fully set up signature yet
    {
        var (m, _) = obj._<ж<Func>>(ᐧ); if (m != nil) {
            Ꮡcheck.objDecl(new FuncжObject(m), nil);
        }
    }
    if (x.mode == typexpr){
        // method expression
        var (m, _) = obj._<ж<Func>>(ᐧ);
        if (m == nil) {
            Ꮡcheck.errorf(new ast_Identжpositioner(e.Sel), MissingFieldOrMethod, "%s.%s undefined (type %s has no method %s)"u8, x.expr, sel, x.typ, sel);
            goto ΔError;
        }
        check.recordSelection(Ꮡe, MethodExpr, x.typ, new FuncжObject(m), index, indirect);
        var sig = (~m).typ._<ж<ΔSignature>>();
        if ((~sig).recv == nil) {
            Ꮡcheck.error(new ast_SelectorExprжpositioner(Ꮡe), InvalidDeclCycle, illegalCycleInMethodˢ);
            goto ΔError;
        }
        // the receiver type becomes the type of the first function
        // argument of the method expression's function type
        slice<ж<Var>> @params = default!;
        if ((~sig).@params != nil) {
            @params = sig.Value.@params.Value.vars;
        }
        // Be consistent about named/unnamed parameters. This is not needed
        // for type-checking, but the newly constructed signature may appear
        // in an error message and then have mixed named/unnamed parameters.
        // (An alternative would be to not print parameter names in errors,
        // but it's useful to see them; this is cheap and method expressions
        // are rare.)
        @string name = ""u8;
        if (len(@params) > 0 && (~@params[0]).name != ""u8) {
            // name needed
            name = sig.Value.recv.Value.name;
            if (name == ""u8) {
                name = "_"u8;
            }
        }
        @params = appendꓸꓸꓸ(new ж<Var>[]{NewVar((~(~sig).recv).pos, (~(~sig).recv).pkg, name, x.typ)}.slice(), @params);
        x.mode = value;
        x.typ = new ΔSignatureжΔType(Ꮡ(new ΔSignature(
            tparams: (~sig).tparams,
            @params: NewTuple(@params.ꓸꓸꓸ),
            results: (~sig).results,
            variadic: (~sig).variadic
        )));
        check.addDeclDep(new FuncжObject(m));
    } else {
        // regular selector
        switch (obj.type()) {
        case ж<Var> objΔ2: {
            check.recordSelection(Ꮡe, FieldVal, x.typ, new VarжObject(objΔ2), index, indirect);
            if (x.mode == variable || indirect){
                x.mode = variable;
            } else {
                x.mode = value;
            }
            x.typ = objΔ2.Value.typ;
            break;
        }
        case ж<Func> objΔ2: {
            check.recordSelection(Ꮡe, // TODO(gri) If we needed to take into account the receiver's
 // addressability, should we report the type &(x.typ) instead?
 MethodVal, x.typ, new FuncжObject(objΔ2), index, indirect);
            var disabled = true;
            if (!disabled && debug) {
                // TODO(gri) The verification pass below is disabled for now because
                //           method sets don't match method lookup in some cases.
                //           For instance, if we made a copy above when creating a
                //           custom method for a parameterized received type, the
                //           method set method doesn't match (no copy there). There
                ///          may be other situations.
                // Verify that LookupFieldOrMethod and MethodSet.Lookup agree.
                // TODO(gri) This only works because we call LookupFieldOrMethod
                // _before_ calling NewMethodSet: LookupFieldOrMethod completes
                // any incomplete interfaces so they are available to NewMethodSet
                // (which assumes that interfaces have been completed already).
                var typ = x.typ;
                if (x.mode == variable) {
                    // If typ is not an (unnamed) pointer or an interface,
                    // use *typ instead, because the method set of *typ
                    // includes the methods of typ.
                    // Variables are addressable, so we can always take their
                    // address.
                    {
                        var (_, ok) = typ._<ж<Pointer>>(ᐧ); if (!ok && !IsInterface(typ)) {
                            typ = new PointerжΔType(Ꮡ(new Pointer(@base: typ)));
                        }
                    }
                }
                // If we created a synthetic pointer type above, we will throw
                // away the method set computed here after use.
                // TODO(gri) Method set computation should probably always compute
                // both, the value and the pointer receiver method set and represent
                // them in a single structure.
                // TODO(gri) Consider also using a method set cache for the lifetime
                // of checker once we rely on MethodSet lookup instead of individual
                // lookup.
                var mset = NewMethodSet(typ);
                {
                    var m = mset.Lookup(check.pkg, sel); if (m == nil || !AreEqual((~m).obj, objΔ2)) {
                        Ꮡcheck.dump(vSVSˢ, e.Pos(), typ, (~objΔ2).name, m.OrTypedNil());
                        Ꮡcheck.dump("%s\n"u8, mset.OrTypedNil());
                        // Caution: MethodSets are supposed to be used externally
                        // only (after all interface types were completed). It's
                        // now possible that we get here incorrectly. Not urgent
                        // to fix since we only run this code in debug mode.
                        // TODO(gri) fix this eventually.
                        throw panic("method sets and lookup don't agree");
                    }
                }
            }
            x.mode = value;
            ref var sig = ref heap<ΔSignature>(out var Ꮡsig);
            sig = (~objΔ2).typ._<ж<ΔSignature>>().Value;
            sig.recv = default!;
            x.typ = new ΔSignatureжΔType(Ꮡsig);
            check.addDeclDep(new FuncжObject(objΔ2));
            break;
        }
        default: {
            var objΔ2 = obj;
            throw panic("unreachable");
            break;
        }}
    }
    // remove receiver
    // everything went well
    x.expr = new ast.SelectorExprжExpr(Ꮡe);
    return;
ΔError:
    x.mode = invalid;
    x.expr = new ast.SelectorExprжExpr(Ꮡe);
}

// use type-checks each argument.
// Useful to make sure expressions are evaluated
// (and variables are "used") in the presence of
// other errors. Arguments may be nil.
// Reports if all arguments evaluated without error.
internal static bool use(this ж<Checker> Ꮡcheck, params ꓸꓸꓸastꓸExpr argsʗp) {
    var args = argsʗp.slice();

    return Ꮡcheck.useN(args, false);
}

// useLHS is like use, but doesn't "use" top-level identifiers.
// It should be called instead of use if the arguments are
// expressions on the lhs of an assignment.
internal static bool useLHS(this ж<Checker> Ꮡcheck, params ꓸꓸꓸastꓸExpr argsʗp) {
    var args = argsʗp.slice();

    return Ꮡcheck.useN(args, true);
}

internal static bool useN(this ж<Checker> Ꮡcheck, slice<ast.Expr> args, bool lhs) {
    var ok = true;
    foreach (var (_, e) in args) {
        if (!Ꮡcheck.use1(e, lhs)) {
            ok = false;
        }
    }
    return ok;
}

internal static bool use1(this ж<Checker> Ꮡcheck, ast.Expr e, bool lhs) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    ref var x = ref heap(new operand(), out var Ꮡx);
    x.mode = value; // anything but invalid
    var switchᴛ4 = ast.Unparen(e);
    switch (switchᴛ4.type()) {
    case null: {
        break;
    }
    case ж<ast.Ident> n: {
        if ((~n).Name == "_"u8) {
            // nothing to do
            // don't report an error evaluating blank
            break;
        }
        // If the lhs is an identifier denoting a variable v, this assignment
        // is not a 'use' of v. Remember current value of v.used and restore
        // after evaluating the lhs via check.rawExpr.
        ж<Var> v = default!;
        bool v_used = default!;
        if (lhs) {
            {
                var (_, obj) = check.scope.LookupParent((~n).Name, nopos); if (obj != default!) {
                    // It's ok to mark non-local variables, but ignore variables
                    // from other packages to avoid potential race conditions with
                    // dot-imported variables.
                    {
                        var (w, _) = obj._<ж<Var>>(ᐧ); if (w != nil && (~w).pkg == check.pkg) {
                            v = w;
                            v_used = v.Value.used;
                        }
                    }
                }
            }
        }
        Ꮡcheck.exprOrType(Ꮡx, new ast.IdentжExpr(n), true);
        if (v != nil) {
            v.Value.used = v_used; // restore v.used
        }
        break;
    }
    default: {
        var n = switchᴛ4;
        Ꮡcheck.rawExpr(nil, Ꮡx, e, default!, true);
        break;
    }}
    return x.mode != invalid;
}

} // end types_package
