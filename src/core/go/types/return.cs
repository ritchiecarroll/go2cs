// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements isTerminating.
namespace go.go;

using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using global::go.go;

partial class types_package {

// isTerminating reports if s is a terminating statement.
// If s is labeled, label is the label name; otherwise s
// is "".
internal static bool isTerminating(this ref Checker check, ast.Stmt s, @string label) {
    switch (s.type()) {
    default: {
        var sΔ1 = s;
        throw panic("unreachable");
        break;
    }
    case ж<ast.BadStmt> _:
    case ж<ast.DeclStmt> _:
    case ж<ast.EmptyStmt> _:
    case ж<ast.SendStmt> _:
    case ж<ast.IncDecStmt> _:
    case ж<ast.AssignStmt> _:
    case ж<ast.GoStmt> _:
    case ж<ast.DeferStmt> _:
    case ж<ast.RangeStmt> _: {
        var sΔ1 = s;
        break;
    }
    case ж<ast.LabeledStmt> sΔ1: {
        return check.isTerminating((~sΔ1).Stmt, // no chance
 (~(~sΔ1).Label).Name);
    }
    case ж<ast.ExprStmt> sΔ1: {
        {
            var (call, ok) = ast.Unparen((~sΔ1).X)._<ж<ast.CallExpr>>(ᐧ); if (ok && check.isPanic[call]) {
                // calling the predeclared (possibly parenthesized) panic() function is terminating
                return true;
            }
        }
        break;
    }
    case ж<ast.ReturnStmt> sΔ1: {
        return true;
    }
    case ж<ast.BranchStmt> sΔ1: {
        if ((~sΔ1).Tok == token.GOTO || (~sΔ1).Tok == token.FALLTHROUGH) {
            return true;
        }
        break;
    }
    case ж<ast.BlockStmt> sΔ1: {
        return check.isTerminatingList((~sΔ1).List, ""u8);
    }
    case ж<ast.IfStmt> sΔ1: {
        if ((~sΔ1).Else != default! && check.isTerminating(new ast.BlockStmtжStmt((~sΔ1).Body), ""u8) && check.isTerminating((~sΔ1).Else, ""u8)) {
            return true;
        }
        break;
    }
    case ж<ast.SwitchStmt> sΔ1: {
        return check.isTerminatingSwitch((~sΔ1).Body, label);
    }
    case ж<ast.TypeSwitchStmt> sΔ1: {
        return check.isTerminatingSwitch((~sΔ1).Body, label);
    }
    case ж<ast.SelectStmt> sΔ1: {
        foreach (var (_, sΔ2) in (~(~sΔ1).Body).List) {
            var cc = sΔ2._<ж<ast.CommClause>>();
            if (!check.isTerminatingList((~cc).Body, ""u8) || hasBreakList((~cc).Body, label, true)) {
                return false;
            }
        }
        return true;
    }
    case ж<ast.ForStmt> sΔ1: {
        if ((~sΔ1).Cond == default! && !hasBreak(new ast.BlockStmtжStmt((~sΔ1).Body), label, true)) {
            return true;
        }
        break;
    }}
    return false;
}

internal static bool isTerminatingList(this ref Checker check, slice<ast.Stmt> list, @string label) {
    // trailing empty statements are permitted - skip them
    for (nint i = len(list) - 1; i >= 0; i--) {
        {
            var (_, ok) = list[i]._<ж<ast.EmptyStmt>>(ᐧ); if (!ok) {
                return check.isTerminating(list[i], label);
            }
        }
    }
    return false; // all statements are empty
}

internal static bool isTerminatingSwitch(this ref Checker check, ж<ast.BlockStmt> Ꮡbody, @string label) {
    ref var body = ref Ꮡbody.DerefOrNull();

    var hasDefault = false;
    foreach (var (_, s) in body.List) {
        var cc = s._<ж<ast.CaseClause>>();
        if ((~cc).List == default!) {
            hasDefault = true;
        }
        if (!check.isTerminatingList((~cc).Body, ""u8) || hasBreakList((~cc).Body, label, true)) {
            return false;
        }
    }
    return hasDefault;
}

// TODO(gri) For nested breakable statements, the current implementation of hasBreak
// will traverse the same subtree repeatedly, once for each label. Replace
// with a single-pass label/break matching phase.

// hasBreak reports if s is or contains a break statement
// referring to the label-ed statement or implicit-ly the
// closest outer breakable statement.
internal static bool hasBreak(ast.Stmt s, @string label, bool @implicit) {
    switch (s.type()) {
    default: {
        var sΔ1 = s;
        throw panic("unreachable");
        break;
    }
    case ж<ast.BadStmt> _:
    case ж<ast.DeclStmt> _:
    case ж<ast.EmptyStmt> _:
    case ж<ast.ExprStmt> _:
    case ж<ast.SendStmt> _:
    case ж<ast.IncDecStmt> _:
    case ж<ast.AssignStmt> _:
    case ж<ast.GoStmt> _:
    case ж<ast.DeferStmt> _:
    case ж<ast.ReturnStmt> _: {
        var sΔ1 = s;
        break;
    }
    case ж<ast.LabeledStmt> sΔ1: {
        return hasBreak((~sΔ1).Stmt, // no chance
 label, @implicit);
    }
    case ж<ast.BranchStmt> sΔ1: {
        if ((~sΔ1).Tok == token.BREAK) {
            if ((~sΔ1).Label == nil) {
                return @implicit;
            }
            if ((~(~sΔ1).Label).Name == label) {
                return true;
            }
        }
        break;
    }
    case ж<ast.BlockStmt> sΔ1: {
        return hasBreakList((~sΔ1).List, label, @implicit);
    }
    case ж<ast.IfStmt> sΔ1: {
        if (hasBreak(new ast.BlockStmtжStmt((~sΔ1).Body), label, @implicit) || (~sΔ1).Else != default! && hasBreak((~sΔ1).Else, label, @implicit)) {
            return true;
        }
        break;
    }
    case ж<ast.CaseClause> sΔ1: {
        return hasBreakList((~sΔ1).Body, label, @implicit);
    }
    case ж<ast.SwitchStmt> sΔ1: {
        if (label != ""u8 && hasBreak(new ast.BlockStmtжStmt((~sΔ1).Body), label, false)) {
            return true;
        }
        break;
    }
    case ж<ast.TypeSwitchStmt> sΔ1: {
        if (label != ""u8 && hasBreak(new ast.BlockStmtжStmt((~sΔ1).Body), label, false)) {
            return true;
        }
        break;
    }
    case ж<ast.CommClause> sΔ1: {
        return hasBreakList((~sΔ1).Body, label, @implicit);
    }
    case ж<ast.SelectStmt> sΔ1: {
        if (label != ""u8 && hasBreak(new ast.BlockStmtжStmt((~sΔ1).Body), label, false)) {
            return true;
        }
        break;
    }
    case ж<ast.ForStmt> sΔ1: {
        if (label != ""u8 && hasBreak(new ast.BlockStmtжStmt((~sΔ1).Body), label, false)) {
            return true;
        }
        break;
    }
    case ж<ast.RangeStmt> sΔ1: {
        if (label != ""u8 && hasBreak(new ast.BlockStmtжStmt((~sΔ1).Body), label, false)) {
            return true;
        }
        break;
    }}
    return false;
}

internal static bool hasBreakList(slice<ast.Stmt> list, @string label, bool @implicit) {
    foreach (var (_, s) in list) {
        if (hasBreak(s, label, @implicit)) {
            return true;
        }
    }
    return false;
}

} // end types_package
