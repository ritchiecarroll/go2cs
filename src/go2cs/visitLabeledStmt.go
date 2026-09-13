// visitLabeledStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitLabeledStmt(labeledStmt *ast.LabeledStmt) {
	v.outputBuilder.WriteString(v.newline)

	labelName := getSanitizedIdentifier(labeledStmt.Label.Name)

	v.outputBuilder.WriteString(labelName + ":")

	// A Go label can attach to an EMPTY statement — `keep:` as the last line of a block
	// (internal/trace gc.go's goto-target). A C# label must precede a statement, so emit
	// the explicit empty statement (`keep:;` — a bare `keep:` before `}` is CS1525/CS1002).
	if _, isEmpty := labeledStmt.Stmt.(*ast.EmptyStmt); isEmpty {
		v.outputBuilder.WriteString(";")
		return
	}

	target := LabeledStmtContext{label: labelName}

	v.visitStmt(labeledStmt.Stmt, []StmtContext{target})
}

func getBreakLabelName(label string) string {
	return getSanitizedIdentifier("break_" + strings.TrimPrefix(label, "@"))
}

func getContinueLabelName(label string) string {
	return getSanitizedIdentifier("continue_" + strings.TrimPrefix(label, "@"))
}
