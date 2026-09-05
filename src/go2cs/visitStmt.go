// visitStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"strings"
)

type StmtContext interface {
	getDefault() StmtContext
}

type FormattingContext struct {
	useNewLine         bool
	includeSemiColon   bool
	useIndent          bool
	forInit            bool // statement is the init clause of a for-loop (single `;`-free clause)
	heapTypeDeclTarget *strings.Builder
}

func DefaultFormattingContext() FormattingContext {
	return FormattingContext{
		useNewLine:         true,
		includeSemiColon:   true,
		useIndent:          true,
		forInit:            false,
		heapTypeDeclTarget: nil,
	}
}

func (c FormattingContext) getDefault() StmtContext {
	return DefaultFormattingContext()
}

type BlockStmtContext struct {
	format      FormattingContext
	innerPrefix string
	innerSuffix string
	outerPrefix string
	outerSuffix string
}

func DefaultBlockStmtContext() BlockStmtContext {
	return BlockStmtContext{
		format:      DefaultFormattingContext(),
		innerPrefix: "",
		innerSuffix: "",
		outerPrefix: "",
		outerSuffix: "",
	}
}

func (c BlockStmtContext) getDefault() StmtContext {
	return DefaultBlockStmtContext()
}

type LabeledStmtContext struct {
	label string
}

func DefaultLabeledStmtContext() LabeledStmtContext {
	return LabeledStmtContext{
		label: "",
	}
}

func (c LabeledStmtContext) getDefault() StmtContext {
	return DefaultLabeledStmtContext()
}

func getStmtContext[TContext StmtContext](contexts []StmtContext) TContext {
	var zeroValue TContext

	if len(contexts) == 0 {
		return zeroValue.getDefault().(TContext)
	}

	for _, context := range contexts {
		if context != nil {
			if targetContext, ok := context.(TContext); ok {
				return targetContext
			}
		}
	}

	return zeroValue.getDefault().(TContext)
}

// visitListStmt converts one statement of a statement LIST — a block body, a `case` or `select`
// clause body — where every statement occupies its own emitted line or lines.
//
// That is the only slot in which a Go end-of-line comment can be honored as one: the statement's
// text ends the current output line, so a comment written straight after it trails the statement
// exactly as it did in the source (writeTrailingComment). The init clause of an `if`/`for`/`switch`
// is deliberately NOT a list statement — the rest of the header follows it on the same line — which
// is why those callers keep calling visitStmt directly.
func (v *Visitor) visitListStmt(stmt ast.Stmt) {
	v.visitStmt(stmt, []StmtContext{})
	v.writeTrailingComment(stmt.End())
}

func (v *Visitor) visitStmt(stmt ast.Stmt, contexts []StmtContext) {
	v.lastStatementWasReturn = false
	// Position map: this statement's Go line, carried in the text so the block-builder swaps
	// and hoisted-declaration splices below move it with the statement (positionMapOperations).
	v.writePositionSentinel(stmt.Pos())
	v.writeTestAliasShadowComment(stmt, contexts)

	switch stmtType := stmt.(type) {
	case *ast.AssignStmt:
		format := getStmtContext[FormattingContext](contexts)
		v.visitAssignStmt(stmtType, format)
	case *ast.BlockStmt:
		context := getStmtContext[BlockStmtContext](contexts)
		v.visitBlockStmt(stmtType, context)
	case *ast.BranchStmt:
		v.visitBranchStmt(stmtType)
	case *ast.CommClause:
		v.visitCommClause(stmtType)
	case *ast.DeclStmt:
		v.visitDeclStmt(stmtType)
	case *ast.DeferStmt:
		v.visitDeferStmt(stmtType)
	case *ast.ExprStmt:
		format := getStmtContext[FormattingContext](contexts)
		v.visitExprStmt(stmtType, format)
	case *ast.ForStmt:
		target := getStmtContext[LabeledStmtContext](contexts)
		v.visitForStmt(stmtType, target)
	case *ast.GoStmt:
		v.visitGoStmt(stmtType)
	case *ast.IfStmt:
		v.visitIfStmt(stmtType)
	case *ast.IncDecStmt:
		format := getStmtContext[FormattingContext](contexts)
		v.visitIncDecStmt(stmtType, format)
	case *ast.LabeledStmt:
		v.visitLabeledStmt(stmtType)
	case *ast.RangeStmt:
		target := getStmtContext[LabeledStmtContext](contexts)
		v.visitRangeStmt(stmtType, target)
	case *ast.ReturnStmt:
		v.visitReturnStmt(stmtType)
		v.lastStatementWasReturn = true
		v.lastReturnIndentLevel = v.indentLevel
	case *ast.SelectStmt:
		v.visitSelectStmt(stmtType)
	case *ast.SendStmt:
		format := getStmtContext[FormattingContext](contexts)
		v.visitSendStmt(stmtType, format)
	case *ast.SwitchStmt:
		target := getStmtContext[LabeledStmtContext](contexts)
		v.visitSwitchStmt(stmtType, target)
	case *ast.TypeSwitchStmt:
		target := getStmtContext[LabeledStmtContext](contexts)
		v.visitTypeSwitchStmt(stmtType, target)
	case *ast.BadStmt:
		v.showWarning("@visitStmt - BadStmt encountered: %#v", stmtType)
	case *ast.EmptyStmt:
		// Nothing to do
	default:
		panic(fmt.Sprintf("@visitStmt - Unexpected Stmt type: %#v", v.getPrintedNode(stmtType)))
	}

	// Every statement drains the KeepAlives its own calls named (a funnel's ᴋ temps, a
	// bridged-wrapper argument's box — syscallKeepAliveAnalysis.go). Assign and Expr statements
	// were the only drain points until 2026-09-05, which left a box named inside an `if` condition
	// to be drained by whatever statement was converted NEXT — inside the body or after it
	// (measured in internal/concurrent's emission: still correct by liveness, since a use on any
	// path after the call keeps the local live at it, but placed by accident, and a body with no
	// draining statement would have carried the box into the next function). A `for` INIT or POST
	// clause is emitted inside the header, where nothing can follow it: refuse loudly instead.
	if format := getStmtContext[FormattingContext](contexts); v.inForPost || format.forInit {
		v.rejectForClauseKeepAlive(stmt)
	} else {
		v.drainSyscallKeepAlive()
	}
}
