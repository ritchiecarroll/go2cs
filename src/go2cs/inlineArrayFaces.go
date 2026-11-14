// inlineArrayFaces.go - Gbtc
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
	"path/filepath"
)

// INLINE-ARRAY FACES (COORD 2026-11-13 06:50Z, ruling (a) on the D3 face question)
//
// An inline array (a go.arrays member) exposes an element indexer and its length, and nothing else: no slice, no
// address, no conversion to golib's array<T> (DESIGN-array-value-storage.md 7.6.1 item 3). So the automatic rule keeps a
// seed out when an array field in its closure is used, in the declaring package's NON-TEST files, through a face the
// type does not spell, and GO2CS2009 names the face and its site. The spelled faces are an element (indexed, selected
// through, assigned, compound-assigned), len and cap, range, and the whole array boxed into an interface. Every other
// whole-value use (an argument or result of [N]T, a store to or from a variable, a comparison) is unspelled at I0: a
// copy between two admitted fields would be spelled by the type, but the population has none, and treating it as
// unspelled keeps the rule independent of which other types are admitted.
//
// Forced mode does not consult the faces: an unspelled face there is a C# compile error, as 7.7.4 has it, and so is one
// in a _test.go file or in another package.

// collectUnspelledFaces records, per struct type (inlineTypeKey of the field's own struct), the first array-field use in
// file that an inline array cannot spell.
func collectUnspelledFaces(file *ast.File, fset *token.FileSet, info *types.Info, faces map[string]string) {
	var stack []ast.Node

	ast.Inspect(file, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}

		stack = append(stack, n)
		sel, ok := n.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		owner, field, ok := arrayFieldOwner(sel, info)

		if !ok {
			return true
		}

		key, ok := inlineTypeKey(owner)

		if !ok {
			return true
		}

		if _, seen := faces[key]; seen {
			return true
		}

		if face := unspelledFace(sel, stack, info); face != "" {
			position := fset.Position(sel.Pos())
			faces[key] = fmt.Sprintf("field %s is %s at %s:%d:%d", field, face, filepath.Base(position.Filename), position.Line, position.Column)
		}

		return true
	})
}

// arrayFieldOwner returns the struct type that declares the array field sel selects (the last struct on the selection's
// path, so a promoted field answers for the embedded struct that declares it), and the field's name.
func arrayFieldOwner(sel *ast.SelectorExpr, info *types.Info) (types.Type, string, bool) {
	selection := info.Selections[sel]

	if selection == nil || selection.Kind() != types.FieldVal {
		return nil, "", false
	}

	if _, ok := selection.Obj().Type().Underlying().(*types.Array); !ok {
		return nil, "", false
	}

	owner := selection.Recv()
	path := selection.Index()

	for _, index := range path[:len(path)-1] {
		if pointer, ok := owner.Underlying().(*types.Pointer); ok {
			owner = pointer.Elem()
		}

		st, ok := owner.Underlying().(*types.Struct)

		if !ok {
			return nil, "", false
		}

		owner = st.Field(index).Type()
	}

	if pointer, ok := owner.Underlying().(*types.Pointer); ok {
		owner = pointer.Elem()
	}

	return owner, selection.Obj().Name(), true
}

// unspelledFace names the face of the inline array subject (the top of stack: an array field selected, or an element of
// one that is itself an array) when an inline array cannot spell it, or returns "".
func unspelledFace(subject ast.Expr, stack []ast.Node, info *types.Info) string {
	// The nearest enclosing node that is not a parenthesis, from index from down.
	up := func(from int) (ast.Node, int) {
		for i := from; i >= 0; i-- {
			if _, ok := stack[i].(*ast.ParenExpr); !ok {
				return stack[i], i
			}
		}

		return nil, -1
	}

	parent, at := up(len(stack) - 2)

	switch p := parent.(type) {
	case *ast.IndexExpr:
		if ast.Unparen(p.X) != subject {
			return "" // the array is the index, which cannot type-check
		}

		// Climb from the element through the field selections and nested indexes below it (x.f[i].g, x.f[i][j]): an
		// address taken anywhere on that path is an address into the inline storage (C1's review, m5).
		var top ast.Expr = p

		for {
			next, nextAt := up(at - 1)

			if below, ok := next.(*ast.SelectorExpr); ok && ast.Unparen(below.X) == top {
				if field := info.Selections[below]; field != nil && field.Kind() == types.FieldVal {
					top, at = below, nextAt
					continue
				}
			}

			if below, ok := next.(*ast.IndexExpr); ok && ast.Unparen(below.X) == top {
				top, at = below, nextAt
				continue
			}

			break
		}

		grand, _ := up(at - 1)

		switch g := grand.(type) {
		case *ast.UnaryExpr:
			if g.Op == token.AND && top == p {
				return "an element's address (&x.f[i])"
			}

			if g.Op == token.AND {
				return "an address below an element (&x.f[i].g, &x.f[i][j])"
			}
		case *ast.SliceExpr:
			if ast.Unparen(g.X) == top {
				return "an element sliced (x.f[i][:])"
			}
		case *ast.SelectorExpr:
			if method := info.Selections[g]; method != nil && method.Kind() == types.MethodVal && ast.Unparen(g.X) == top {
				if signature, ok := method.Obj().Type().(*types.Signature); ok && signature.Recv() != nil {
					if _, pointerRecv := signature.Recv().Type().Underlying().(*types.Pointer); pointerRecv {
						if _, elemIsPointer := info.TypeOf(top).Underlying().(*types.Pointer); !elemIsPointer {
							return "an element's address (a pointer-receiver method of x.f[i])"
						}
					}
				}
			}
		}

		// An element that is itself an array (x.m[i] of a nested array, every one of which is held inline) has a whole
		// array's faces: boxed into an interface parameter it is spelled, stored or compared it is not.
		if _, isArray := info.TypeOf(top).Underlying().(*types.Array); isArray {
			return unspelledFace(top, stack[:at+1], info)
		}

		return ""
	case *ast.CallExpr:
		// len and cap read the constant length; unsafe.Sizeof, Offsetof and Alignof are compile-time constants.
		if builtin := calledBuiltin(p.Fun, info); builtin != nil {
			switch builtin.Name() {
			case "len", "cap", "Sizeof", "Offsetof", "Alignof":
				return ""
			}
		}

		if signature, ok := info.TypeOf(p.Fun).(*types.Signature); ok {
			for i, arg := range p.Args {
				if ast.Unparen(arg) != subject {
					continue
				}

				if parameter := parameterType(signature, i); parameter != nil && types.IsInterface(parameter) {
					return "" // boxed into an interface: the whole inline array, by value
				}
			}
		}

		return "a whole value passed as an argument"
	case *ast.RangeStmt:
		if ast.Unparen(p.X) == subject {
			return ""
		}
	case *ast.SliceExpr:
		if ast.Unparen(p.X) == subject {
			return "sliced (x.f[:])"
		}
	case *ast.UnaryExpr:
		if p.Op == token.AND {
			return "its address (&x.f)"
		}
	case *ast.SelectorExpr:
		return "" // x.f.m: a method of the array type, which go2cs does not declare on an unnamed array
	}

	return "a whole value (copied, stored, compared or returned)"
}

// calledBuiltin returns the builtin a call's function names (len, or unsafe.Offsetof through its package), or nil.
func calledBuiltin(fun ast.Expr, info *types.Info) *types.Builtin {
	switch f := ast.Unparen(fun).(type) {
	case *ast.Ident:
		builtin, _ := info.Uses[f].(*types.Builtin)
		return builtin
	case *ast.SelectorExpr:
		builtin, _ := info.Uses[f.Sel].(*types.Builtin)
		return builtin
	}

	return nil
}

// parameterType returns the type the i-th argument binds to, the variadic element included.
func parameterType(signature *types.Signature, i int) types.Type {
	params := signature.Params()

	if params.Len() == 0 {
		return nil
	}

	if signature.Variadic() && i >= params.Len()-1 {
		if slice, ok := params.At(params.Len() - 1).Type().(*types.Slice); ok {
			return slice.Elem()
		}
	}

	if i < params.Len() {
		return params.At(i).Type()
	}

	return nil
}
