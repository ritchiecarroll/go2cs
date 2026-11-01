// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file checks invariants of token.Token ordering that we rely on
// since package go/token doesn't provide any guarantees at the moment.
namespace go.go;

using token = global::go.go.token_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.go;
using static global::go.go.types_package;

partial class types_internal_test_package {

internal static map<token.Token, token.Token> assignOps = new map<token.Token, token.Token>{
    [token.ADD_ASSIGN] = token.ADD,
    [token.SUB_ASSIGN] = token.SUB,
    [token.MUL_ASSIGN] = token.MUL,
    [token.QUO_ASSIGN] = token.QUO,
    [token.REM_ASSIGN] = token.REM,
    [token.AND_ASSIGN] = token.AND,
    [token.OR_ASSIGN] = token.OR,
    [token.XOR_ASSIGN] = token.XOR,
    [token.SHL_ASSIGN] = token.SHL,
    [token.SHR_ASSIGN] = token.SHR,
    [token.AND_NOT_ASSIGN] = token.AND_NOT
};

public static partial void TestZeroTok(ж<testing.T> t) {
    // zero value for token.Token must be token.ILLEGAL
    token.Token zero = default;
    if (token.ILLEGAL != zero) {
        t.Errorf("%s == %d; want 0", token.ILLEGAL, zero);
    }
}

public static void TestAssignOp(ж<testing.T> t) {
    // there are fewer than 256 tokens
    for (nint i = 0; i < 256; i++) {
        token.Token tok = (token.Token)i;
        token.Token got = assignOp(tok);
        token.Token want = assignOps[tok];
        if (got != want) {
            t.Errorf("for assignOp(%s): got %s; want %s", tok, got, want);
        }
    }
}

} // end types_internal_test_package
