// goDeferConstArgs.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

// Go evaluates a go or defer statement's function value and arguments when the statement runs, so
// the converter passes each argument EAGERLY and the thunk receives it back as a temp:
// `goǃ(ᴛ1 => fmt.Println(ᴛ1), firstˢ)`. A CONSTANT argument has the same value whenever it is
// evaluated, so evaluating it inside the thunk keeps Go's rule, and the call then reads as the Go
// source does: `goǃ(() => fmt.Println("First"))`, `goǃ(ᴛ1 => fmt.Println("square:", ᴛ1), n * n)`.
// Every constant go/types reports qualifies: string, numeric, rune, bool and typed constants
// (seat #33, owner rulings 2026-10-27).
//
// A constant moves only into a thunk that CALLS the callee. A method-group or delegate-value launch
// hands the callee over with no call to move into, and keeps its form; a string constant there renders
// `(@string)"…"` at the site (see the plain-mode go/defer arm of hoistCollector.consider). The standard
// library keeps its hoisted literals and its eager arguments (literalsHoistedFor).

// goDeferInlineConstArgs reports which arguments of a go/defer call are constants that may render inside
// the thunk's call, or nil when none may. It answers for a callee that is a declared function or method:
// a func literal takes its arguments as its own parameters, a builtin has its own go/defer forms, and a
// func value is evaluated at the statement and handed over as a delegate. A spread call (`f(xs...)`)
// keeps its eager form. The caller still decides the FORM: a constant moves only when the statement
// takes the temp-parameter thunk with no receiver snapshot (goDeferInlineForm).
func (v *Visitor) goDeferInlineConstArgs(call *ast.CallExpr) []bool {
	if packageHoistsLiterals || call.Ellipsis.IsValid() || len(call.Args) == 0 {
		return nil
	}

	if _, isLit := ast.Unparen(call.Fun).(*ast.FuncLit); isLit {
		return nil
	}

	if calleeFunc(v.info, call) == nil {
		return nil
	}

	var inline []bool

	for i, arg := range call.Args {
		if tv, ok := v.info.Types[arg]; ok && tv.Value != nil {
			if inline == nil {
				inline = make([]bool, len(call.Args))
			}

			inline[i] = true
		}
	}

	return inline
}

// goDeferCalleeIsFunction reports whether a go/defer call's callee is a package-level function (no
// receiver). Its method-group form takes no receiver, so a constant argument can move the statement to
// the thunk form without changing when anything is evaluated: `go worker(5)` is `goǃ(() => worker(5))`.
func (v *Visitor) goDeferCalleeIsFunction(call *ast.CallExpr) bool {
	fn := calleeFunc(v.info, call)

	if fn == nil {
		return false
	}

	sig, ok := fn.Type().(*types.Signature)

	return ok && sig.Recv() == nil
}

// goDeferInlineForm decides a go/defer statement's inlined constants once its form is known. A
// package-level function callee with a constant argument takes the thunk form; a method keeps the form
// the statement already took, and its constants move only when that form is the thunk. A receiver
// snapshot (`ᴛ0`) keeps every argument eager: the snapshot splits the rendered call at its temp list.
// It returns the inlined positions (nil for none) and the possibly forced renderLambdaParams.
func (v *Visitor) goDeferInlineForm(call *ast.CallExpr, renderLambdaParams, recvSnapshot bool) ([]bool, bool) {
	inline := v.goDeferInlineConstArgs(call)

	if inline == nil || recvSnapshot {
		return nil, renderLambdaParams
	}

	if !renderLambdaParams && !v.goDeferCalleeIsFunction(call) {
		return nil, renderLambdaParams
	}

	return inline, true
}

// goDeferEagerTemp is the thunk parameter the i-th argument of a go/defer call binds to: arguments that
// stay eager are numbered in order, skipping the inlined constants, so `go f("a", x)` reads
// `ᴛ1 => f("a", ᴛ1)`. With no inlined constant (nil inline) the i-th argument binds to ᴛ(i+1).
func goDeferEagerTemp(inline []bool, i int) string {
	ordinal := 1

	for j := 0; j < i; j++ {
		if j >= len(inline) || !inline[j] {
			ordinal++
		}
	}

	return fmt.Sprintf("%s%d", TempVarMarker, ordinal)
}

// goDeferEagerCount is the number of arguments of a go/defer call that stay eager.
func goDeferEagerCount(inline []bool, paramCount int) int {
	count := paramCount

	for _, isInline := range inline {
		if isInline {
			count--
		}
	}

	return count
}

// goDeferEagerArgs is the eager-argument list of a go/defer call: callArgs without the inlined constants.
func goDeferEagerArgs(inline []bool, callArgs []string) []string {
	var eager []string

	for i, arg := range callArgs {
		if i < len(inline) && inline[i] {
			continue
		}

		eager = append(eager, arg)
	}

	return eager
}
