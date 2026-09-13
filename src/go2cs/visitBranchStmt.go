// visitBranchStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/token"
)

func (v *Visitor) visitBranchStmt(branchStmt *ast.BranchStmt) {
	// FALLTHROUGH is handled in visitSwitchStmt.go
	switch branchStmt.Tok {
	case token.BREAK:
		v.outputBuilder.WriteString(v.newline)
		if branchStmt.Label == nil {
			v.writeOutput("break;")
		} else {
			v.writeOutput("goto %s;", getBreakLabelName(branchStmt.Label.Name))
		}
	case token.CONTINUE:
		if branchStmt.Label == nil {
			// Inside a `do { … } while (false)` switch-break wrapper a bare C# `continue` would
			// continue the WRAPPER — itself an iteration statement, which then exits on its false
			// condition and falls through past the switch — instead of the Go loop this statement
			// targets. Emit a goto to the enclosing loop's end-of-body label instead; the label
			// precedes the loop's copy-backs, so control flows through them to the post clause
			// exactly as a labeled continue does (see wrappedContinueLoopLabel).
			if label := v.wrappedContinueLoopLabel(); label != "" {
				v.outputBuilder.WriteString(v.newline)
				v.writeOutput("goto %s;", label)
				return
			}

			// A C# `continue` transfers straight to the post clause, skipping the end-of-body
			// per-iteration copy-backs of a Go 1.22+ transformed loop — emit them here first
			// (see forClausePerIterVars). A labeled continue instead flows through the
			// `continue_<label>:` target, which the copy-backs already follow.
			if len(v.loopCopyBackStack) > 0 {
				for _, copyBack := range v.loopCopyBackStack[len(v.loopCopyBackStack)-1] {
					v.outputBuilder.WriteString(v.newline)
					v.writeOutput("%s", copyBack)
				}
			}

			v.outputBuilder.WriteString(v.newline)
			v.writeOutput("continue;")
		} else {
			v.outputBuilder.WriteString(v.newline)
			v.writeOutput("goto %s;", getContinueLabelName(branchStmt.Label.Name))
		}
	case token.GOTO:
		v.outputBuilder.WriteString(v.newline)
		v.writeOutput("goto %s;", getSanitizedIdentifier(branchStmt.Label.Name))
	}
}

// continueTargetEntry is one element of continueTargetStack: what an unlabeled Go `continue`
// binds to at the current emission point. Loop entries (visitForStmt / visitRangeStmt) carry the
// loop's minted end-of-body label name; wrapper entries mark a `do { … } while (false)`
// switch-break wrapper (visitSwitchStmtCore) — a C# iteration statement the Go source never had.
type continueTargetEntry struct {
	isWrapper bool   // a switch-break wrapper, not a Go loop
	labelName string // loop entries: the minted `continueᴛN` end-of-body label
	labelUsed bool   // loop entries: some wrapped continue targeted it — emit the label
}

// wrappedContinueLoopLabel reports the label an unlabeled `continue` must `goto` when the
// innermost enclosing C# iteration statement is a switch-break wrapper rather than the Go loop
// the continue targets — minting nothing, but marking the enclosing loop's pre-minted label used
// so the loop's emitter writes it (visitForStmt / visitRangeStmt replace a marker in the loop
// body's innerSuffix). Returns "" when the continue may be emitted bare: the top of the stack is
// a real loop (or the stack is empty — a continue outside any loop does not compile in Go, so
// emission cannot reach that state with a wrapper on top).
func (v *Visitor) wrappedContinueLoopLabel() string {
	count := len(v.continueTargetStack)

	if count == 0 || !v.continueTargetStack[count-1].isWrapper {
		return ""
	}

	for i := count - 2; i >= 0; i-- {
		if entry := v.continueTargetStack[i]; !entry.isWrapper {
			entry.labelUsed = true
			return entry.labelName
		}
	}

	return ""
}
