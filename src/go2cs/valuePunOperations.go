// valuePunOperations.go - Gbtc
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

// A VALUE PUN is Go's `*(*U)(unsafe.Pointer(&x))` READ where x and U are same-size sized numeric
// types: math.Float64bits' whole body, and eleven more production sites (runtime's float helpers and
// histogram infinities, reflect's float32 register moves, internal/runtime/atomic's Float64). Go
// compiles it to a register move. The general emission is a pointer reinterpret -- x heap-boxed
// because its address is taken, then `~Ꮡx.Reinterpret<T, U>()` -- which costs three counted objects
// per call where Go has none (the box, its pinnable slot, and the reinterpreting field reference).
//
// The read needs none of that: it takes the address only to reread x's own bits as U, and hands the
// pointer to nobody. So a recognised pun renders as golib's `bitcast<T, U>(x)` (Unsafe.BitCast) and its
// `&x` stops counting as address-taken, which leaves x an ordinary local or parameter when that was its
// only address use.
//
// Deliberately narrow:
//   - READS only. A pun that is written (`*(*uint32)(unsafe.Pointer(&x)) |= …`, runtime's min/max) or
//     whose result is addressed must alias x's storage and keeps the reinterpret.
//   - x is a plain identifier, and both x's type and U are PREDECLARED sized numerics (int8…int64,
//     uint8…uint64, float32, float64) of equal size. Named types, int/uint/uintptr (whose width is the
//     target's), complex, bool and every composite keep the reinterpret.
type valuePun struct {
	x        *ast.Ident
	src, dst types.Type
}

// packageValuePunReads maps each recognised pun's deref to its operands; packageValuePunAddressOf marks
// the `&x` each one consumes. Both are rebuilt by collectValuePunReads per package, beside the
// cgo_unsafe_args lift's maps and for the same reason: the escape pass and the emission visitor both
// read them.
var packageValuePunReads map[*ast.StarExpr]*valuePun
var packageValuePunAddressOf map[*ast.UnaryExpr]bool

// collectValuePunReads is the package pre-pass. It is self-resetting.
func collectValuePunReads(files []FileEntry, info *types.Info) {
	packageValuePunReads = map[*ast.StarExpr]*valuePun{}
	packageValuePunAddressOf = map[*ast.UnaryExpr]bool{}

	for _, fileEntry := range files {
		var stack []ast.Node

		ast.Inspect(fileEntry.file, func(n ast.Node) bool {
			if n == nil {
				stack = stack[:len(stack)-1]
				return true
			}

			var parent ast.Node

			if len(stack) > 0 {
				parent = stack[len(stack)-1]
			}

			stack = append(stack, n)

			star, ok := n.(*ast.StarExpr)

			if !ok || !starExprIsRead(star, parent) {
				return true
			}

			if pun, addressOf := recognizeValuePun(star, info); pun != nil {
				packageValuePunReads[star] = pun
				packageValuePunAddressOf[addressOf] = true
			}

			return true
		})
	}
}

// starExprIsRead reports whether a deref is used only for its value: not assigned, incremented,
// ranged into, or addressed.
func starExprIsRead(star *ast.StarExpr, parent ast.Node) bool {
	switch p := parent.(type) {
	case *ast.AssignStmt:
		for _, lhs := range p.Lhs {
			if lhs == star {
				return false
			}
		}
	case *ast.IncDecStmt:
		return false
	case *ast.RangeStmt:
		return p.Key != star && p.Value != star
	case *ast.UnaryExpr:
		return p.Op != token.AND
	case *ast.ParenExpr:
		return false // a parenthesized deref could be any of the above one level up; stay general
	}

	return true
}

// recognizeValuePun matches `*(*U)(unsafe.Pointer(&x))` with the type rule above.
func recognizeValuePun(star *ast.StarExpr, info *types.Info) (*valuePun, *ast.UnaryExpr) {
	conversion, ok := unparenthesize(star.X).(*ast.CallExpr)

	if !ok || len(conversion.Args) != 1 {
		return nil, nil
	}

	target, ok := info.Types[conversion.Fun]

	if !ok || !target.IsType() {
		return nil, nil
	}

	targetPointer, ok := types.Unalias(target.Type).(*types.Pointer)

	if !ok {
		return nil, nil
	}

	toUnsafe, ok := unparenthesize(conversion.Args[0]).(*ast.CallExpr)

	if !ok || len(toUnsafe.Args) != 1 {
		return nil, nil
	}

	if basic, ok := types.Unalias(info.TypeOf(toUnsafe)).(*types.Basic); !ok || basic.Kind() != types.UnsafePointer {
		return nil, nil
	}

	addressOf, ok := unparenthesize(toUnsafe.Args[0]).(*ast.UnaryExpr)

	if !ok || addressOf.Op != token.AND {
		return nil, nil
	}

	x, ok := unparenthesize(addressOf.X).(*ast.Ident)

	if !ok {
		return nil, nil
	}

	if _, ok := info.Uses[x].(*types.Var); !ok {
		return nil, nil
	}

	src, dst := info.TypeOf(x), targetPointer.Elem()
	srcSize, srcOK := valuePunScalarSize(src)
	dstSize, dstOK := valuePunScalarSize(dst)

	if !srcOK || !dstOK || srcSize != dstSize {
		return nil, nil
	}

	return &valuePun{x: x, src: src, dst: dst}, addressOf
}

// valuePunScalarSize reports the byte size of a predeclared sized numeric type.
func valuePunScalarSize(t types.Type) (int, bool) {
	basic, ok := t.(*types.Basic) // predeclared only: an alias resolves, a named type does not match

	if !ok {
		if alias, isAlias := t.(*types.Alias); isAlias {
			basic, ok = types.Unalias(alias).(*types.Basic)
		}
	}

	if !ok {
		return 0, false
	}

	switch basic.Kind() {
	case types.Int8, types.Uint8:
		return 1, true
	case types.Int16, types.Uint16:
		return 2, true
	case types.Int32, types.Uint32, types.Float32:
		return 4, true
	case types.Int64, types.Uint64, types.Float64:
		return 8, true
	}

	return 0, false
}

// valuePunEmission renders a recognised pun as golib's value bitcast, reporting false otherwise.
func (v *Visitor) valuePunEmission(star *ast.StarExpr) (string, bool) {
	pun := packageValuePunReads[star]

	if pun == nil {
		return "", false
	}

	return fmt.Sprintf("builtin.bitcast<%s, %s>(%s)", v.getCSharpTypeName(pun.src), v.getCSharpTypeName(pun.dst), v.convExpr(pun.x, nil)), true
}
