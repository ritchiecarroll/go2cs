// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements error reporting.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using static @internal.types.errors_package;
using runtime = runtime_package;
using strings = strings_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using errors = @internal.types.errors_package;
using global::go.go;
using io = io_package;
using ꓸꓸꓸany = Span<any>;

partial class types_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string assertionFailedˢ = "assertion failed"u8;

internal static partial void assert(bool p) {
    if (!p) {
        @string msg = assertionFailedˢ;
        // Include information about the assertion location. Due to panic recovery,
        // this location is otherwise buried in the middle of the panicking stack.
        {
            var (_, @file, line, ok) = runtime.Caller(1); if (ok) {
                msg = fmt.Sprintf("%s:%d: %s"u8, @file, line, msg);
            }
        }
        throw panic(msg);
    }
}

// An errorDesc describes part of a type-checking error.
[GoType] partial struct errorDesc {
    internal positioner posn;
    internal @string msg;
}

// An error_ represents a type-checking error.
// A new error_ is created with Checker.newError.
// To report an error_, call error_.report.
[GoType] partial struct error_ {
    internal ж<Checker> check;
    internal slice<errorDesc> desc;
    internal errors.Code code;
    internal bool soft; // TODO(gri) eventually determine this from an error code
}

// newError returns a new error_ with the given error code.
internal static ж<error_> newError(this ж<Checker> Ꮡcheck, errors.Code code) {
    if (code == 0) {
        throw panic("error code must not be 0");
    }
    return Ꮡ(new error_(check: Ꮡcheck, code: code));
}

// addf adds formatted error information to err.
// It may be called multiple times to provide additional information.
// The position of the first call to addf determines the position of the reported Error.
// Subsequent calls to addf provide additional information in the form of additional lines
// in the error message (types2) or continuation errors identified by a tab-indented error
// message (go/types).
[GoRecv] internal static void addf(this ref error_ err, positioner at, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    err.desc = append(err.desc, new errorDesc(at, err.check.sprintf(format, args.ꓸꓸꓸ)));
}

// addAltDecl is a specialized form of addf reporting another declaration of obj.
[GoRecv] internal static void addAltDecl(this ref error_ err, Object obj) {
    {
        tokenꓸPos pos = obj.Pos(); if (pos.IsValid()) {
            // We use "other" rather than "previous" here because
            // the first declaration seen may not be textually
            // earlier in the source.
            err.addf(new Objectᴠpositioner(obj), "other declaration of %s"u8, obj.Name());
        }
    }
}

[GoRecv] internal static bool empty(this ref error_ err) {
    return err.desc == default!;
}

[GoRecv] internal static positioner posn(this ref error_ err) {
    if (err.empty()) {
        return noposn;
    }
    return err.desc[0].posn;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noErrorˢ = "no error"u8;

// msg returns the formatted error message without the primary error position pos().
[GoRecv] internal static @string msg(this ref error_ err) {
    if (err.empty()) {
        return noErrorˢ;
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (i, _) in err.desc) {
        var p = Ꮡ(err.desc, i);
        if (i > 0) {
            fmt.Fprint(new strings_BuilderжWriter(Ꮡbuf), (@string)"\n\t"u8);
            if ((~p).posn.Pos().IsValid()) {
                fmt.Fprintf(new strings_BuilderжWriter(Ꮡbuf), "%s: "u8, (~err.check).fset.Position((~p).posn.Pos()));
            }
        }
        Ꮡbuf.WriteString((~p).msg);
    }
    return buf.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidOperandˢ = "invalid operand"u8;
internal static readonly @string invalidTypeˢ = "invalid type"u8;
internal static readonly @string errorSCodeDˢ = "ERROR: %s (code = %d)"u8;

// report reports the error err, setting check.firstError if necessary.
[GoRecv] internal static void report(this ref error_ err) {
    if (err.empty()) {
        throw panic("no error");
    }
    // Cheap trick: Don't report errors with messages containing
    // "invalid operand" or "invalid type" as those tend to be
    // follow-on errors which don't add useful information. Only
    // exclude them if these strings are not at the beginning,
    // and only if we have at least one error already reported.
    var check = err.check;
    if ((~check).firstErr != default!) {
        // It is sufficient to look at the first sub-error only.
        @string msg = err.desc[0].msg;
        if (strings.Index(msg, invalidOperandˢ) > 0 || strings.Index(msg, invalidTypeˢ) > 0) {
            return;
        }
    }
    if ((~(~check).conf)._Trace) {
        check.trace(err.posn().Pos(), errorSCodeDˢ, err.desc[0].msg, err.code);
    }
    // In go/types, if there is a sub-error with a valid position,
    // call the typechecker error handler for each sub-error.
    // Otherwise, call it once, with a single combined message.
    var multiError = false;
    if (!isTypes2) {
        for (nint i = 1; i < len(err.desc); i++) {
            if (err.desc[i].posn.Pos().IsValid()) {
                multiError = true;
                break;
            }
        }
    }
    if (multiError){
        foreach (var (i, _) in err.desc) {
            var p = Ꮡ(err.desc, i);
            check.handleError(i, (~p).posn, err.code, (~p).msg, err.soft);
        }
    } else {
        check.handleError(0, err.posn(), err.code, err.msg(), err.soft);
    }
    // make sure the error is not reported twice
    err.desc = default!;
}

// handleError should only be called by error_.report.
[GoRecv] internal static void handleError(this ref Checker check, nint index, positioner posn, errors.Code code, @string msg, bool soft) {
    assert(code != 0);
    if (index == 0){
        // If we are encountering an error while evaluating an inherited
        // constant initialization expression, pos is the position of
        // the original expression, and not of the currently declared
        // constant identifier. Use the provided errpos instead.
        // TODO(gri) We may also want to augment the error message and
        // refer to the position (pos) in the original expression.
        if (check.errpos != default! && check.errpos.Pos().IsValid()) {
            assert(check.iota != default!);
            posn = check.errpos;
        }
        // Report invalid syntax trees explicitly.
        if (code == InvalidSyntaxTree) {
            msg = "invalid syntax tree: "u8 + msg;
        }
        // If we have a URL for error codes, add a link to the first line.
        if ((~check.conf)._ErrorURL != ""u8) {
            @string url = fmt.Sprintf((~check.conf)._ErrorURL, code);
            {
                nint i = strings.Index(msg, "\n"u8); if (i >= 0){
                    msg = msg.slice(0, i) + url + msg.slice(i);
                } else {
                    msg += url;
                }
            }
        }
    } else {
        // Indent sub-error.
        // Position information is passed explicitly to Error, below.
        msg = "\t"u8 + msg;
    }
    var span = spanOf(posn);
    var e = new ΔError(
        Fset: check.fset,
        Pos: span.pos,
        Msg: stripAnnotations(msg),
        Soft: soft,
        go116code: code,
        go116start: span.start,
        go116end: span.end
    );
    if (check.errpos != default!) {
        // If we have an internal error and the errpos override is set, use it to
        // augment our error positioning.
        // TODO(rFindley) we may also want to augment the error message and refer
        // to the position (pos) in the original expression.
        var spanΔ1 = spanOf(check.errpos);
        e.Pos = spanΔ1.pos;
        e.go116start = spanΔ1.start;
        e.go116end = spanΔ1.end;
    }
    if (check.firstErr == default!) {
        check.firstErr = e;
    }
    var f = check.conf.Value.Error;
    if (f == default!) {
        throw panic(new bailout(nil)); // record first error and exit
    }
    f(e);
}

internal static readonly @string invalidArg = "invalid argument: "u8;
internal static readonly @string invalidOp = "invalid operation: "u8;

// The positioner interface is used to extract the position of type-checker errors.
[GoType] partial interface positioner {
    tokenꓸPos Pos();
}

internal static void error(this ж<Checker> Ꮡcheck, positioner at, errors.Code code, @string msg) {
    var err = Ꮡcheck.newError(code);
    err.addf(at, "%s"u8, msg);
    err.report();
}

internal static void errorf(this ж<Checker> Ꮡcheck, positioner at, errors.Code code, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var err = Ꮡcheck.newError(code);
    err.addf(at, format, args.ꓸꓸꓸ);
    err.report();
}

internal static void softErrorf(this ж<Checker> Ꮡcheck, positioner at, errors.Code code, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var err = Ꮡcheck.newError(code);
    err.addf(at, format, args.ꓸꓸꓸ);
    err.Value.soft = true;
    err.report();
}

internal static void versionErrorf(this ж<Checker> Ꮡcheck, positioner at, goVersion v, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    @string msg = Ꮡcheck.sprintf(format, args.ꓸꓸꓸ);
    var err = Ꮡcheck.newError(UnsupportedFeature);
    err.addf(at, "%s requires %s or later"u8, msg, v);
    err.report();
}

[GoType("global::go.go.token_package.ΔPos")] partial struct atPos;

internal static tokenꓸPos Pos(this atPos s) {
    return ((tokenꓸPos)s);
}

// posSpan holds a position range along with a highlighted position within that
// range. This is used for positioning errors, with pos by convention being the
// first position in the source where the error is known to exist, and start
// and end defining the full span of syntax being considered when the error was
// detected. Invariant: start <= pos < end || start == pos == end.
[GoType] partial struct posSpan {
    internal tokenꓸPos start, pos, end;
}

internal static tokenꓸPos Pos(this posSpan e) {
    return e.pos;
}

// inNode creates a posSpan for the given node.
// Invariant: node.Pos() <= pos < node.End() (node.End() is the position of the
// first byte after node within the source).
internal static posSpan inNode(ast.Node node, tokenꓸPos pos) {
    tokenꓸPos start = node.Pos();
    tokenꓸPos end = node.End();
    if (debug) {
        assert(start <= pos && pos < end);
    }
    return new posSpan(start, pos, end);
}

// spanOf extracts an error span from the given positioner. By default this is
// the trivial span starting and ending at pos, but this span is expanded when
// the argument naturally corresponds to a span of source code.
internal static posSpan spanOf(positioner at) {
    switch (at.type()) {
    case null: {
        throw panic("nil positioner");
        break;
    }
    case posSpan x: {
        return x;
    }
    case {} Δx when Δx._<ast.Node>(out var x): {
        tokenꓸPos pos = x.Pos();
        return new posSpan(pos, pos, x.End());
    }
    case ж<operand> x: {
        if ((~x).expr != default!) {
            tokenꓸPos pos = x.Pos();
            return new posSpan(pos, pos, (~x).expr.End());
        }
        return new posSpan(nopos, nopos, nopos);
    }
    default: {
        var x = at;
        tokenꓸPos pos = at.Pos();
        return new posSpan(pos, pos, pos);
    }}
}

} // end types_package
