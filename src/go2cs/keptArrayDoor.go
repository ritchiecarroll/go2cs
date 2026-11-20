// keptArrayDoor.go - Gbtc
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
	"strings"
)

// DOOR C, THE UNNAMED FORM (docs/phase4/DESIGN-array-pointer-doors.md, section 20; decision D5, taken)
//
// `(*[N]E)(p)`, p an unsafe.Pointer, that is KEPT (bound to a local, passed or returned) rather than indexed,
// sliced or dereferenced where it is written, was emitted through the pointer conversion `To<array<E>>(p)`. That
// conversion has no N, so over C memory it can build no window and refuses by name (modernc.org/sqlite's page cache
// binding: `f := (*[2]uintptr)(unsafe.Pointer(stub))` over libc.Xcalloc memory, then `f[0], f[1] = ...`). The door
// carries N from the type:
//
//	(*[N]E)(p)      @unsafe.Pointer.ToArray<E>(p, N)
//	f[i], (*f)[i]   f.ElementRef(i)        f a local bound by the door and never re-pointed
//
// Over Go memory ToArray answers what To answers, so only the refusal over C memory changes. The door takes an
// UNNAMED array whose element holds no reference in go2cs's model; a named array (4.5's sibling box kind), an
// element that holds a reference, a FromRef operand (4.6) and every conversion another route renders whole keep the
// pointer conversion. The whole-array forms through a kept local (`*f` read for its value, a range over it) are not
// taken; `*f = v` already stores through the native box (array<T>.Assign).

// keptArrayDoor is golib's door for the kept conversion.
const keptArrayDoor = "@unsafe.Pointer.ToArray"

// keptArrayDoorEmission renders the kept conversion through the door, or reports false where the pointer conversion
// stays. pointee is the C# pointee the pointer conversion would name, `array<E>`; expr is the rendered operand.
func (v *Visitor) keptArrayDoorEmission(callExpr *ast.CallExpr, arg ast.Expr, pointee, expr string) (string, bool) {
	length, ok := v.keptArrayDoorLength(callExpr, arg)

	if !ok || strings.Contains(expr, "FromRef(") {
		return "", false
	}

	const arrayPrefix = "array<"

	if !strings.HasPrefix(pointee, arrayPrefix) || !strings.HasSuffix(pointee, ">") {
		return "", false
	}

	if v.keptArrayDoorConversions == nil {
		v.keptArrayDoorConversions = make(map[*ast.CallExpr]bool)
	}

	v.keptArrayDoorConversions[callExpr] = true

	elem := pointee[len(arrayPrefix) : len(pointee)-1]

	return fmt.Sprintf("%s<%s>(%s, %s)", keptArrayDoor, elem, expr, csNintLiteral(length)), true
}

// keptArrayDoorLength answers N for a conversion the door may take: the target is a pointer to an UNNAMED array
// whose element holds no reference, the operand is an unsafe.Pointer, and no other route renders the conversion
// (declining through that route's own predicate, never a copy of it).
func (v *Visitor) keptArrayDoorLength(callExpr *ast.CallExpr, arg ast.Expr) (int64, bool) {
	if v.spanViewConversions[callExpr] {
		return 0, false
	}

	argType := v.info.TypeOf(arg)

	if argType == nil {
		return 0, false
	}

	if basic, ok := argType.Underlying().(*types.Basic); !ok || basic.Kind() != types.UnsafePointer {
		return 0, false
	}

	target := v.info.TypeOf(callExpr)

	if target == nil {
		return 0, false
	}

	ptr, ok := types.Unalias(target).(*types.Pointer)

	if !ok {
		return 0, false
	}

	arr, ok := types.Unalias(ptr.Elem()).(*types.Array)

	if !ok || goImageReferenceBearing(arr.Elem(), map[types.Type]bool{}) {
		return 0, false
	}

	if v.arrayPointerConversionKeepsItsRoute(callExpr) {
		return 0, false
	}

	return arr.Len(), true
}

// isKeptArrayDoorLocal reports whether an index base is a local bound by the door and never re-pointed, so `f[i]`
// reads through ElementRef. ElementRef answers `f.Value[i]` for every box that is not native, so the form is right
// whatever the door answered at run time. A local that is re-pointed, whose address is taken, or that is bound any
// other way keeps `f.Value[i]`, which a native box refuses by name.
func (v *Visitor) isKeptArrayDoorLocal(x ast.Expr) bool {
	ident, ok := ast.Unparen(x).(*ast.Ident)

	if !ok || v.currentFuncDecl == nil || v.currentFuncDecl.Body == nil || len(v.keptArrayDoorConversions) == 0 {
		return false
	}

	obj, ok := v.info.Uses[ident].(*types.Var)

	if !ok {
		return false
	}

	if bound, found := v.keptArrayDoorLocalCache[obj]; found {
		return bound
	}

	isDoor := func(expr ast.Expr) bool {
		call, ok := ast.Unparen(expr).(*ast.CallExpr)
		return ok && v.keptArrayDoorConversions[call]
	}

	bound := v.localBoundBy(obj, isDoor) && !v.refRootIsReassigned(obj) && !v.identAddressTaken(obj)

	if v.keptArrayDoorLocalCache == nil {
		v.keptArrayDoorLocalCache = make(map[types.Object]bool)
	}

	v.keptArrayDoorLocalCache[obj] = bound
	return bound
}

// localBoundBy reports whether obj's declaration in the current function binds it to an expression the predicate
// accepts — `p := <expr>` or `var p = <expr>`.
func (v *Visitor) localBoundBy(obj types.Object, accepts func(ast.Expr) bool) bool {
	binds := false

	ast.Inspect(v.currentFuncDecl.Body, func(n ast.Node) bool {
		if binds {
			return false
		}

		switch node := n.(type) {
		case *ast.AssignStmt:
			if node.Tok != token.DEFINE || len(node.Lhs) != len(node.Rhs) {
				return true
			}

			for i, lhs := range node.Lhs {
				if ident, ok := lhs.(*ast.Ident); ok && v.info.Defs[ident] == obj && accepts(node.Rhs[i]) {
					binds = true
				}
			}
		case *ast.ValueSpec:
			if len(node.Names) != len(node.Values) {
				return true
			}

			for i, name := range node.Names {
				if v.info.Defs[name] == obj && accepts(node.Values[i]) {
					binds = true
				}
			}
		}

		return true
	})

	return binds
}
