// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using static @internal.types.errors_package;
using slices = slices_package;
using errors = @internal.types.errors_package;
using global::go.go;

partial class types_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string labelˢ = "label"u8;
internal static readonly @string gotoSJumpsIntoBlockˢ = "goto %s jumps into block"u8;
internal static readonly @string labelSNotDeclaredˢ = "label %s not declared"u8;

// labels checks correct label use in body.
internal static void labels(this ж<Checker> Ꮡcheck, ж<ast.BlockStmt> Ꮡbody) {
    ref var body = ref Ꮡbody.DerefOrNull();

    // set of all labels in this body
    var all = NewScope(nil, body.Pos(), body.End(), labelˢ);
    var fwdJumps = Ꮡcheck.blockBranches(all, nil, nil, body.List);
    // If there are any forward jumps left, no label was found for
    // the corresponding goto statements. Either those labels were
    // never defined, or they are inside blocks and not reachable
    // for the respective gotos.
    foreach (var (_, jmp) in fwdJumps) {
        @string msg = default!;
        errors.Code code = default!;
        @string name = jmp.Value.Label.Value.Name;
        {
            var alt = all.Lookup(name); if (alt != default!){
                msg = gotoSJumpsIntoBlockˢ;
                code = JumpIntoBlock;
                alt._<ж<Label>>().Value.used = true; // avoid another error
            } else {
                msg = labelSNotDeclaredˢ;
                code = UndeclaredLabel;
            }
        }
        Ꮡcheck.errorf(new ast_Identжpositioner((~jmp).Label), code, msg, name);
    }
    // spec: "It is illegal to define a label that is never used."
    foreach (var (name, vᴛ1) in (~all).elems) {
        var obj = vᴛ1;

        obj = resolve(name, obj);
        {
            var lbl = obj._<ж<Label>>(); if (!(~lbl).used) {
                Ꮡcheck.softErrorf(new Labelжpositioner(lbl), UnusedLabel, "label %s declared and not used"u8, (~lbl).name);
            }
        }
    }
}

// A block tracks label declarations in a block and its enclosing blocks.
[GoType] partial struct block {
    internal ж<block> parent;                   // enclosing block
    internal ж<ast.LabeledStmt> lstmt;         // labeled statement to which this block belongs, or nil
    internal map<@string, ж<ast.LabeledStmt>> labels; // allocated lazily
}

// insert records a new label declaration for the current block.
// The label must not have been declared before in any block.
internal static void insert(this ж<block> Ꮡb, ж<ast.LabeledStmt> Ꮡs) {
    ref var b = ref Ꮡb.DerefOrNull();
    ref var s = ref Ꮡs.DerefOrNull();

    @string name = s.Label.Value.Name;
    if (debug) {
        assert(Ꮡb.gotoTarget(name) == nil);
    }
    var labels = b.labels;
    if (labels == default!) {
        labels = new map<@string, ж<ast.LabeledStmt>>();
        b.labels = labels;
    }
    labels[name] = Ꮡs;
}

// gotoTarget returns the labeled statement in the current
// or an enclosing block with the given label name, or nil.
internal static ж<ast.LabeledStmt> gotoTarget(this ж<block> Ꮡb, @string name) {
    for (var s = Ꮡb; s != nil; s = s.Value.parent) {
        {
            var t = (~s).labels[name]; if (t != nil) {
                return t;
            }
        }
    }
    return default!;
}

// enclosingTarget returns the innermost enclosing labeled
// statement with the given label name, or nil.
internal static ж<ast.LabeledStmt> enclosingTarget(this ж<block> Ꮡb, @string name) {
    for (var s = Ꮡb; s != nil; s = s.Value.parent) {
        {
            var t = s.Value.lstmt; if (t != nil && (~(~t).Label).Name == name) {
                return t;
            }
        }
    }
    return default!;
}

// blockBranches processes a block's statement list and returns the set of outgoing forward jumps.
// all is the scope of all declared labels, parent the set of labels declared in the immediately
// enclosing block, and lstmt is the labeled statement this block is associated with (or nil).
internal static slice<ж<ast.BranchStmt>> blockBranches(this ж<Checker> Ꮡcheck, ж<ΔScope> Ꮡall, ж<block> Ꮡparent, ж<ast.LabeledStmt> Ꮡlstmt, slice<ast.Stmt> list) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    var b = Ꮡ(new block(parent: Ꮡparent, lstmt: Ꮡlstmt));
    tokenꓸPos varDeclPos = default!;
    ref var fwdJumps = ref heap<slice<ж<ast.BranchStmt>>>(out var ᏑfwdJumps);
    ref var badJumps = ref heap<slice<ж<ast.BranchStmt>>>(out var ᏑbadJumps);
    // All forward jumps jumping over a variable declaration are possibly
    // invalid (they may still jump out of the block and be ok).
    // recordVarDecl records them for the given position.
    void recordVarDecl(tokenꓸPos pos) {
        varDeclPos = pos;
        ᏑbadJumps.ValueSlot = appendꓸꓸꓸ(ᏑbadJumps.ValueSlot[..0], ᏑfwdJumps.ValueSlot); // copy fwdJumps to badJumps
    }
    bool jumpsOverVarDecl(ж<ast.BranchStmt> jmp) => varDeclPos.IsValid() && slices.Contains(ᏑbadJumps.ValueSlot, jmp);
    var bʗ1 = b;
    void blockBranches(ж<ast.LabeledStmt> lstmtΔ1, slice<ast.Stmt> listΔ1) {
        // Unresolved forward jumps inside the nested block
        // become forward jumps in the current block.
        ᏑfwdJumps.ValueSlot = appendꓸꓸꓸ(ᏑfwdJumps.ValueSlot, Ꮡcheck.blockBranches(Ꮡall, bʗ1, lstmtΔ1, listΔ1));
    }
    ref var stmtBranches = ref heap<Action<ast.Stmt>>(out var ᏑstmtBranches);
    var bʗ2 = b;
    var blockBranchesʗ1 = blockBranches;
    var jumpsOverVarDeclʗ1 = jumpsOverVarDecl;
    var recordVarDeclʗ1 = recordVarDecl;
    stmtBranches = (ast.Stmt s) => {
        switch (s.type()) {
        case ж<ast.DeclStmt> sΔ1: {
            {
                var (d, _) = (~sΔ1).Decl._<ж<ast.GenDecl>>(ᐧ); if (d != nil && (~d).Tok == token.VAR) {
                    recordVarDeclʗ1(d.Pos());
                }
            }
            break;
        }
        case ж<ast.LabeledStmt> sΔ1: {
            {
                @string name = sΔ1.Value.Label.Value.Name; if (name != "_"u8) {
                    // declare non-blank label
                    var lbl = NewLabel((~sΔ1).Label.Pos(), Ꮡcheck.Value.pkg, name);
                    {
                        var alt = Ꮡall.Insert(new LabelжObject(lbl)); if (alt != default!){
                            var err = Ꮡcheck.newError(DuplicateLabel);
                            err.Value.soft = true;
                            err.addf(new Labelжpositioner(lbl), "label %s already declared"u8, name);
                            err.addAltDecl(alt);
                            err.report();
                        } else {
                            // ok to continue
                            bʗ2.insert(sΔ1);
                            Ꮡcheck.Value.recordDef((~sΔ1).Label, new LabelжObject(lbl));
                        }
                    }
                    // resolve matching forward jumps and remove them from fwdJumps
                    nint i = 0;
                    foreach (var (_, jmp) in ᏑfwdJumps.ValueSlot) {
                        if ((~(~jmp).Label).Name == name){
                            // match
                            lbl.Value.used = true;
                            Ꮡcheck.Value.recordUse((~jmp).Label, new LabelжObject(lbl));
                            if (jumpsOverVarDeclʗ1(jmp)) {
                                Ꮡcheck.softErrorf(
                                    new ast_Identжpositioner((~jmp).Label),
                                    JumpOverDecl,
                                    "goto %s jumps over variable declaration at line %d"u8,
                                    name,
                                    Ꮡcheck.Value.fset.Position(varDeclPos).Line);
                            }
                        } else {
                            // ok to continue
                            // no match - record new forward jump
                            ᏑfwdJumps.ValueSlot[i] = jmp;
                            i++;
                        }
                    }
                    ᏑfwdJumps.ValueSlot = ᏑfwdJumps.ValueSlot.slice(0, i);
                    Ꮡlstmt = sΔ1;
                }
            }
            ᏑstmtBranches.ValueSlot((~sΔ1).Stmt);
            break;
        }
        case ж<ast.BranchStmt> sΔ1: {
            if ((~sΔ1).Label == nil) {
                return; // checked in 1st pass (check.stmt)
            }
            @string name = sΔ1.Value.Label.Value.Name;
            var exprᴛ1 = (~sΔ1).Tok;
            if (exprᴛ1 == token.BREAK) {
                var valid = false;
                {
                    var t = bʗ2.enclosingTarget(name); if (t != nil) {
                        // determine and validate target
                        // spec: "If there is a label, it must be that of an enclosing
                        // "for", "switch", or "select" statement, and that is the one
                        // whose execution terminates."
                        switch ((~t).Stmt.type()) {
                        case ж<ast.SwitchStmt> _:
                        case ж<ast.TypeSwitchStmt> _:
                        case ж<ast.SelectStmt> _:
                        case ж<ast.ForStmt> _:
                        case ж<ast.RangeStmt> _: {
                            valid = true;
                            break;
                        }}

                    }
                }
                if (!valid) {
                    Ꮡcheck.errorf(new ast_Identжpositioner((~sΔ1).Label), MisplacedLabel, "invalid break label %s"u8, name);
                    return;
                }
            }
            else if (exprᴛ1 == token.CONTINUE) {
                var valid = false;
                {
                    var t = bʗ2.enclosingTarget(name); if (t != nil) {
                        // spec: "If there is a label, it must be that of an enclosing
                        // "for" statement, and that is the one whose execution advances."
                        switch ((~t).Stmt.type()) {
                        case ж<ast.ForStmt> _:
                        case ж<ast.RangeStmt> _: {
                            valid = true;
                            break;
                        }}

                    }
                }
                if (!valid) {
                    Ꮡcheck.errorf(new ast_Identжpositioner((~sΔ1).Label), MisplacedLabel, "invalid continue label %s"u8, name);
                    return;
                }
            }
            else if (exprᴛ1 == token.GOTO) {
                if (bʗ2.gotoTarget(name) == nil) {
                    // label may be declared later - add branch to forward jumps
                    ᏑfwdJumps.ValueSlot = append(ᏑfwdJumps.ValueSlot, sΔ1);
                    return;
                }
            }
            else { /* default: */
                Ꮡcheck.errorf(new ast_BranchStmtжpositioner(sΔ1), InvalidSyntaxTree, "branch statement: %s %s"u8, (~sΔ1).Tok, name);
                return;
            }

            var obj = Ꮡall.Value.Lookup(name);
            obj._<ж<Label>>().Value.used = true;
            Ꮡcheck.Value.recordUse((~sΔ1).Label, // record label use
 obj);
            break;
        }
        case ж<ast.AssignStmt> sΔ1: {
            if ((~sΔ1).Tok == token.DEFINE) {
                recordVarDeclʗ1(sΔ1.Pos());
            }
            break;
        }
        case ж<ast.BlockStmt> sΔ1: {
            blockBranchesʗ1(Ꮡlstmt, (~sΔ1).List);
            break;
        }
        case ж<ast.IfStmt> sΔ1: {
            ᏑstmtBranches.ValueSlot(new ast.BlockStmtжStmt((~sΔ1).Body));
            if ((~sΔ1).Else != default!) {
                ᏑstmtBranches.ValueSlot((~sΔ1).Else);
            }
            break;
        }
        case ж<ast.CaseClause> sΔ1: {
            blockBranchesʗ1(nil, (~sΔ1).Body);
            break;
        }
        case ж<ast.SwitchStmt> sΔ1: {
            ᏑstmtBranches.ValueSlot(new ast.BlockStmtжStmt((~sΔ1).Body));
            break;
        }
        case ж<ast.TypeSwitchStmt> sΔ1: {
            ᏑstmtBranches.ValueSlot(new ast.BlockStmtжStmt((~sΔ1).Body));
            break;
        }
        case ж<ast.CommClause> sΔ1: {
            blockBranchesʗ1(nil, (~sΔ1).Body);
            break;
        }
        case ж<ast.SelectStmt> sΔ1: {
            ᏑstmtBranches.ValueSlot(new ast.BlockStmtжStmt((~sΔ1).Body));
            break;
        }
        case ж<ast.ForStmt> sΔ1: {
            ᏑstmtBranches.ValueSlot(new ast.BlockStmtжStmt((~sΔ1).Body));
            break;
        }
        case ж<ast.RangeStmt> sΔ1: {
            ᏑstmtBranches.ValueSlot(new ast.BlockStmtжStmt((~sΔ1).Body));
            break;
        }}
    };
    foreach (var (_, s) in list) {
        stmtBranches(s);
    }
    return fwdJumps;
}

} // end types_package
