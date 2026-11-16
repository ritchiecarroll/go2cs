// valueDecodeSites.go - Gbtc
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
	"go/token"
	"go/types"
)

// B2's D2 (COORD ruling 2026-11-15): an RVALUE dereference `*p` whose pointee is a struct the inline-array admission
// KEPT OUT emits `(p).GoValue()` in place of `~p`. Such a struct holds its arrays as array<T> references, so its CLR
// layout is not its Go layout, and when p is a native box (a reinterpret of other Go storage, or C memory) the
// whole-value read through Value is refused by name. GoValue decodes the bytes at Go's offsets into a fresh value
// instead (golib NativeStructMarshal.DecodeValue), which is Go's answer for a copy; on every other box it is ~p.
//
// The trigger is a fact the converter already holds, the admission's kept-out set (inlineArrayAdmission.blocked), and
// the rvalue rule below is the one the B2 census counted with (inlineArrayValueDecodeCensus_test.go), so the census
// and the emission cannot disagree about which sites change. Census of record, go1.26.11: std 0 sites on four
// targets; modernc.org/libc v1.80.1 1 site on linux amd64 and arm64, crypt_blowfish's
// `*(*TBF_ctx)(unsafe.Pointer(&[1042]uint32{...}))` (docs/phase4/DATA-b2-value-decode-census-2026-11-15.md).

// packageValueDecodeSites is the package being converted's D2 sites; resetPackageState clears it.
var packageValueDecodeSites map[*ast.StarExpr]bool

// collectValueDecodeSites returns the rvalue dereferences in files of a pointer to a struct admission kept out.
func collectValueDecodeSites(files []*ast.File, info *types.Info, admission *inlineArrayAdmission) map[*ast.StarExpr]bool {
	if admission == nil || len(admission.blocked) == 0 {
		return nil
	}

	var sites map[*ast.StarExpr]bool

	for _, file := range files {
		var stack []ast.Node

		ast.Inspect(file, func(n ast.Node) bool {
			if n == nil {
				stack = stack[:len(stack)-1]
				return true
			}

			stack = append(stack, n)

			if star, ok := n.(*ast.StarExpr); ok && valueDecodeSite(stack, info, admission.blocked) {
				if sites == nil {
					sites = map[*ast.StarExpr]bool{}
				}

				sites[star] = true
			}

			return true
		})
	}

	return sites
}

// valueDecodeSite reports whether the StarExpr on top of stack reads the WHOLE value of a struct blocked keeps out.
func valueDecodeSite(stack []ast.Node, info *types.Info, blocked map[string]string) bool {
	star := stack[len(stack)-1].(*ast.StarExpr)
	tv, ok := info.Types[star.X]

	if !ok || tv.IsType() {
		return false
	}

	pointer, ok := tv.Type.Underlying().(*types.Pointer)

	if !ok {
		return false
	}

	key, ok := inlineTypeKey(pointer.Elem())

	return ok && blocked[key] != "" && valueDecodeRvalue(stack) && !insideConstant(stack, info)
}

// insideConstant reports whether an enclosing expression of the StarExpr on top of stack is a compile-time constant
// (`unsafe.Sizeof(*p)` and its kin): its operand is never evaluated, so the dereference reads nothing.
func insideConstant(stack []ast.Node, info *types.Info) bool {
	for i := len(stack) - 2; i >= 0; i-- {
		expr, ok := stack[i].(ast.Expr)

		if !ok {
			return false
		}

		if tv, ok := info.Types[expr]; ok && tv.Value != nil {
			return true
		}
	}

	return false
}

// valueDecodeRvalue reports whether the StarExpr on top of stack reads its pointee's WHOLE value: it is not an
// assignment target, not the operand of & or ++/--, and not the base of a field selection (a field read goes through
// FieldRef at its Go offset, and a field WRITE through a decoded copy would be lost).
func valueDecodeRvalue(stack []ast.Node) bool {
	star := stack[len(stack)-1]
	i := len(stack) - 2

	for i >= 0 {
		if _, paren := stack[i].(*ast.ParenExpr); !paren {
			break
		}

		star = stack[i]
		i--
	}

	if i < 0 {
		return true
	}

	switch parent := stack[i].(type) {
	case *ast.AssignStmt:
		for _, lhs := range parent.Lhs {
			if lhs == star {
				return false
			}
		}
	case *ast.UnaryExpr:
		return parent.Op != token.AND
	case *ast.IncDecStmt:
		return false
	case *ast.SelectorExpr:
		return parent.X != star
	}

	return true
}

// valueDecodeEmission renders a D2 site as `(p).GoValue()`, or reports false to keep the ordinary emission. A pointer
// PARAMETER or RECEIVER dereferenced by its bare name renders as the function's ref-local alias of the pointee (bound
// at entry through Value), which is not a box and has no GoValue: those keep today's emission, and their read is
// still refused by name at entry.
func (v *Visitor) valueDecodeEmission(starExpr *ast.StarExpr, context StarExprContext) (string, bool) {
	if context.inLhsAssign || !packageValueDecodeSites[starExpr] {
		return "", false
	}

	if ident, isIdent := starExpr.X.(*ast.Ident); isIdent {
		if v.isDerefdPointerParamIdent(ident) {
			return "", false
		}

		if pointerRecv, recvName := v.isPointerReceiver(); pointerRecv && v.identResolvesToReceiver(ident, recvName) {
			return "", false
		}
	}

	// The pointer cast `*(*T)(x)` renders its operand as convStarExpr's cast-deref branch does, so the box D2 reads
	// is the one `~` would have read.
	var contexts []ExprContext

	if isIdentPointerCast(starExpr.X) {
		lambdaContext := DefaultLambdaContext()
		lambdaContext.isPointerCast = true
		contexts = []ExprContext{lambdaContext}
	}

	return fmt.Sprintf("(%s).GoValue()", v.convExpr(starExpr.X, contexts)), true
}

// isIdentPointerCast reports whether x is `(*T)(y)` with T an identifier: the shape convStarExpr renders with the
// pointer-cast lambda context.
func isIdentPointerCast(x ast.Expr) bool {
	callExpr, ok := x.(*ast.CallExpr)

	if !ok || len(callExpr.Args) != 1 {
		return false
	}

	parenExpr, ok := callExpr.Fun.(*ast.ParenExpr)

	if !ok {
		return false
	}

	if _, ok := parenExpr.X.(*ast.StarExpr); !ok {
		return false
	}

	return getIdentifier(parenExpr.X) != nil
}
