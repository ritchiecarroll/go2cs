// convParenExpr.go - Gbtc
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
)

// convParenExpr renders a parenthesized expression. The incoming literal context is passed
// THROUGH to the operand rather than replaced: parentheses are transparent in Go, so `(x)`
// lands in the enclosing slot exactly as `x` does, and the slot's span-tolerance signal
// (BasicLitContext.spanTargetUnsupported) must reach the operand either way. Starting the
// operand from a fresh default instead silently re-enabled the `"…"u8` ReadOnlySpan<byte>
// form inside a span-HOSTILE slot — `panic(("a" + "b"))` folded its two utf8 literal
// constants into one span with no boxing conversion to panic's object parameter (CS1503),
// where the unparenthesized `panic("a" + "b")` correctly kept plain operands.
//
// A composite literal's string ELEMENT slot is span-TOLERANT and must keep the u8 form; it
// now says so through its own per-element gate in convCompositeLit rather than by relying on
// this context being dropped.
func (v *Visitor) convParenExpr(parenExpr *ast.ParenExpr, context LambdaContext, litContext BasicLitContext) string {
	starContext := DefaultStarExprContext()
	starContext.inParenExpr = true
	expr := v.convExpr(parenExpr.X, []ExprContext{starContext, litContext})

	// In a pointer cast, we need to intermediately cast the target expression to an uintptr.
	// This is required since unsafe.Pointer is in its own library and no implicit cast can
	// be added for it on the pointer class (ж<T>) in the core library without creating a
	// circular dependency. Although C# allows circular dependencies, NuGet does not. If the
	// target happens to not be an unsafe pointer, the cast is still safe since all pointer
	// types support this cast operation.
	if context.isPointerCast {
		return fmt.Sprintf("(%s)(uintptr)", expr)
	}

	return fmt.Sprintf("(%s)", expr)
}
