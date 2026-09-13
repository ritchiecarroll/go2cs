// visitIdent.go - Gbtc
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
	"strings"
)

// Handles identity types in context of a TypeSpec
func (v *Visitor) visitIdent(ident *ast.Ident, identType types.Type, name string, lifted bool) {
	resolvedIdentType := v.getIdentType(ident)
	underlyingIdentType := resolvedIdentType.Underlying()

	// A defined type over a NAMED type whose underlying is a struct/array/etc. (`type winlibcall
	// libcall`) must wrap the NAMED type, not its underlying — emitting the raw underlying
	// (`struct{fn uintptr; …}`) produces invalid C#. Use the named type's name so go2cs-gen's
	// InheritedTypeTemplate wraps a real type. Numeric/basic underlyings keep the `num:`/basic form
	// (`type MyInt int` → `num:nint`), which is the visually-closer mapping.
	goTypeName := underlyingIdentType.String()

	if named, ok := resolvedIdentType.(*types.Named); ok {
		if _, isBasic := underlyingIdentType.(*types.Basic); !isBasic {
			goTypeName = v.getFullyQualifiedTypeName(named, false)
		}
	}

	csTypeName := convertToCSTypeName(goTypeName)

	var target *strings.Builder
	var preLiftIndentLevel int

	// Intra-function type declarations are not allowed in C#
	if lifted {
		if v.inFunction {
			target = &strings.Builder{}

			if !strings.HasPrefix(name, v.currentFuncName+"_") {
				name = fmt.Sprintf("%s_%s", v.currentFuncName, name)
			}

			// NOTE: a local type lifted through THIS path is an identity/wrapper declaration
			// (`type myInt int`), which names no fields and so can never reference an enclosing
			// type parameter. The generic-lift threading lives in visitStructType, where the
			// declaration that CAN carry one is emitted.
			preLiftIndentLevel = v.indentLevel
			v.indentLevel = 0
		}

		name = v.getUniqueLiftedTypeName(name)
		v.liftedTypeMap[identType] = name
	}

	if target == nil {
		target = v.outputBuilder
	}

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	if isNumericType(underlyingIdentType) {
		// Handle numeric type
		v.writeString(target, "%s[GoType(\"num:%s\")]", v.localNameAttrFor(identType), csTypeName)
	} else {
		// Handle other types
		v.writeString(target, "%s[GoType(\"%s\")]", v.localNameAttrFor(identType), csTypeName)
	}

	// Consume any pending publicized-type access modifier (an unexported type used as an
	// exported field — CS0051/CS0052).
	access := v.consumePendingTypeAccess()

	if strings.HasPrefix(name, PointerPrefix) {
		// Handle pointer types
		v.recordTypeAccessibility("class", getSanitizedIdentifier(name), "", access, "")
		v.writeString(target, " %spartial class %s;", access, getSanitizedIdentifier(name))
		usesUnsafeCode = true
	} else {
		// A defined type over a struct that carries fixed-size ARRAY fields inherits the by-value
		// copy problem those fields cause (see wrapperValueCloneAttr): syscall's
		// `type IpMaskString IpAddressString` wraps a `[16]byte`, and `IpAddrString`'s own clone
		// needs a strongly-typed `Clone()` on it. The stamp rides the accessibility record where one
		// is written, so the declaration here reads as the bare `[GoType]` wrapper it is.
		inlineAttrs := v.recordTypeAccessibility("struct", getSanitizedIdentifier(name), "", access, wrapperValueCloneAttr(identType))

		// The type-parameter list rides the DECLARATION only: the recorded name stays the bare
		// identifier so the accessibility record, the lifted-type map and every use site keep
		// their existing spelling (uses render the parameters through their own type resolution).
		v.writeString(target, " %s%spartial struct %s;", inlineAttrs, access, getSanitizedIdentifier(name))
	}

	target.WriteString(v.newline)

	if lifted && v.inFunction {
		if v.currentFuncPrefix.Len() > 0 {
			v.currentFuncPrefix.WriteString(v.newline)
		}

		v.currentFuncPrefix.WriteString(target.String())
		v.indentLevel = preLiftIndentLevel
	}
}

func isNumericType(typ types.Type) bool {
	if typ, ok := typ.(*types.Basic); ok && typ != nil {
		kind := typ.Kind()

		return kind == types.Int || kind == types.Int8 || kind == types.Int16 || kind == types.Int32 || kind == types.Int64 ||
			kind == types.Uint || kind == types.Uint8 || kind == types.Uint16 || kind == types.Uint32 || kind == types.Uint64 ||
			kind == types.Float32 || kind == types.Float64 || kind == types.Complex64 || kind == types.Complex128 ||
			kind == types.Uintptr || kind == types.UnsafePointer
	}

	return false
}

// localTypeUsedTypeParams returns the names of the ENCLOSING function's type parameters that a
// local named type actually references, in the function's own declaration order. Only these are
// threaded onto the lifted declaration (coordinator scoping directive, 2026-08-23): a local type
// that references none lifts exactly as it did before, which keeps every existing lift site in the
// corpus byte-identical — a site that had needed a parameter could not have compiled.
//
// Go's rule is what makes the "used" set well-defined: a local type may reference the enclosing
// function's type parameters and nothing else generic, so walking the type's own structure for
// *types.TypeParam objects owned by that function finds exactly the set the lifted declaration must
// bind.
func (v *Visitor) localTypeUsedTypeParams(t types.Type) []string {
	if v.currentFuncDecl == nil || v.currentFuncDecl.Type == nil || v.currentFuncDecl.Type.TypeParams == nil {
		return nil
	}

	// The enclosing function's parameters, in declaration order — the order the lifted struct must
	// bind them in, since a use site renders arguments positionally.
	var enclosing []string

	for _, field := range v.currentFuncDecl.Type.TypeParams.List {
		for _, ident := range field.Names {
			enclosing = append(enclosing, ident.Name)
		}
	}

	if len(enclosing) == 0 {
		return nil
	}

	used := map[string]bool{}

	var walk func(types.Type, int)

	walk = func(current types.Type, depth int) {
		// Depth-bounded: a recursive local type (`type node struct { next *node }`) would other-
		// wise walk forever, and no type parameter hides deeper than a handful of levels.
		if current == nil || depth > 8 {
			return
		}

		switch shape := current.(type) {
		case *types.TypeParam:
			used[shape.Obj().Name()] = true
		case *types.Struct:
			for i := 0; i < shape.NumFields(); i++ {
				walk(shape.Field(i).Type(), depth+1)
			}
		case *types.Slice:
			walk(shape.Elem(), depth+1)
		case *types.Array:
			walk(shape.Elem(), depth+1)
		case *types.Pointer:
			walk(shape.Elem(), depth+1)
		case *types.Map:
			walk(shape.Key(), depth+1)
			walk(shape.Elem(), depth+1)
		case *types.Chan:
			walk(shape.Elem(), depth+1)
		case *types.Signature:
			if params := shape.Params(); params != nil {
				for i := 0; i < params.Len(); i++ {
					walk(params.At(i).Type(), depth+1)
				}
			}

			if results := shape.Results(); results != nil {
				for i := 0; i < results.Len(); i++ {
					walk(results.At(i).Type(), depth+1)
				}
			}
		case *types.Named:
			for i := 0; i < shape.TypeArgs().Len(); i++ {
				walk(shape.TypeArgs().At(i), depth+1)
			}

			walk(shape.Underlying(), depth+1)
		case *types.Alias:
			walk(shape.Rhs(), depth+1)
		}
	}

	walk(t, 0)

	if len(used) == 0 {
		return nil
	}

	var ordered []string

	for _, name := range enclosing {
		if used[name] {
			ordered = append(ordered, name)
		}
	}

	return ordered
}

// liftedTypeParamList renders a lifted local type's type-parameter list, or the empty string when
// it binds none — the shape that keeps every non-generic lift byte-identical.
func liftedTypeParamList(params []string) string {
	if len(params) == 0 {
		return ""
	}

	return "<" + strings.Join(params, ", ") + ">"
}
