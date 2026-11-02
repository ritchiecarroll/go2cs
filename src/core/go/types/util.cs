// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains various functionality that is
// different between go/types and types2. Factoring
// out this code allows more of the rest of the code
// to be shared.
namespace go.go;

using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using global::go.go;

partial class types_package {

internal const bool isTypes2 = false;

// cmpPos compares the positions p and q and returns a result r as follows:
//
// r <  0: p is before q
// r == 0: p and q are the same position (but may not be identical)
// r >  0: p is after q
//
// If p and q are in different files, p is before q if the filename
// of p sorts lexicographically before the filename of q.
internal static nint cmpPos(tokenꓸPos p, tokenꓸPos q) {
    return (nint)(p - q);
}

// hasDots reports whether the last argument in the call is followed by ...
internal static bool hasDots(ref ast.CallExpr call) {
    return call.Ellipsis.IsValid();
}

// dddErrPos returns the positioner for reporting an invalid ... use in a call.
internal static positioner dddErrPos(ref ast.CallExpr call) {
    return ((atPos)call.Ellipsis);
}

// isdddArray reports whether atyp is of the form [...]E.
internal static bool isdddArray(ref ast.ArrayType atyp) {
    if (atyp.Len != nil) {
        {
            var (ddd, _) = atyp.Len._<ж<ast.Ellipsis>>(ᐧ); if (ddd != nil && ddd.Elt == nil) {
                return true;
            }
        }
    }
    return false;
}

// argErrPos returns positioner for reporting an invalid argument count.
internal static positioner argErrPos(ж<ast.CallExpr> Ꮡcall) {
    ref var call = ref Ꮡcall.DerefOrNull();

    return inNode(new ast.CallExprжNode(Ꮡcall), call.Rparen);
}

// startPos returns the start position of node n.
internal static tokenꓸPos startPos(ast.Node n) {
    return n.Pos();
}

// endPos returns the position of the first character immediately after node n.
internal static tokenꓸPos endPos(ast.Node n) {
    return n.End();
}

// makeFromLiteral returns the constant value for the given literal string and kind.
internal static constant.Value makeFromLiteral(@string lit, token.Token kind) {
    return constant.MakeFromLiteral(lit, kind, 0);
}

} // end types_package
