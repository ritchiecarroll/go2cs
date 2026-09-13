// writtenUnderlyingOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"

	"golang.org/x/tools/go/packages"
)

// packageTypeSpecRHS maps a defined type's *types.TypeName to the type of its DECLARED
// right-hand side (the written underlying): `type Mont [4]uint64` records the unnamed
// *types.Array; `type pallocBits pageBits` records the *types.Named pageBits. go/types
// resolves Named.Underlying() through the whole chain, losing the written form, but the
// generated [GoType] wrapper's conversion surface follows the WRITTEN RHS (its implicit
// operators and Value property target exactly that type) — reinterpret emission needs it.
// Populated by a synchronous per-package pre-pass over the CURRENT package's syntax only;
// a cross-package (or unrecorded) type misses and callers keep the pre-existing route.
// Reset each package so single-package and batch conversions emit identical bytes.
var packageTypeSpecRHS map[types.Object]types.Type

func collectTypeSpecRHS(pkg *packages.Package) {
	packageTypeSpecRHS = map[types.Object]types.Type{}

	for _, file := range pkg.Syntax {
		ast.Inspect(file, func(n ast.Node) bool {
			typeSpec, ok := n.(*ast.TypeSpec)

			// Skip alias declarations (`type A = B`) — they declare no wrapper
			if !ok || typeSpec.Assign.IsValid() {
				return true
			}

			if obj := pkg.TypesInfo.Defs[typeSpec.Name]; obj != nil {
				packageTypeSpecRHS[obj] = pkg.TypesInfo.TypeOf(typeSpec.Type)
			}

			return true
		})
	}
}

// writtenRHSIsUnnamedArray reports whether the defined type was declared DIRECTLY over an
// unnamed array (`type Mont [4]uint64`) — i.e. its [GoType("[N]elem")] wrapper backs onto
// a golib array<E> and exposes `Value : array<E>`. A named RHS (`type pallocBits pageBits`)
// yields a view wrapper whose Value is that NAMED type; unknown types return false so
// callers keep the pre-existing route.
func writtenRHSIsUnnamedArray(named *types.Named) bool {
	rhs, ok := packageTypeSpecRHS[named.Obj()]

	if !ok || rhs == nil {
		return false
	}

	_, isArray := types.Unalias(rhs).(*types.Array)
	return isArray
}

// writtenRHSIsNamedType reports whether `named` was declared DIRECTLY over `base` —
// `type shuffledFS MapFS`, whose written RHS is the NAMED MapFS rather than the
// `map[string]*MapFile` go/types resolves the underlying to. That distinction decides a
// conversion's shape: the wrapper's implicit operators target its WRITTEN RHS, so
// `MapFS(fsys)` binds as ONE user-defined conversion and needs no hop, while the
// shared-underlying hop the two-distinct-defined-types case requires would be the illegal
// two-operator chain here (shuffledFS -> MapFS -> map, CS0030). Unknown or cross-package
// types miss and callers keep the pre-existing route.
func writtenRHSIsNamedType(named *types.Named, base *types.Named) bool {
	rhs, ok := packageTypeSpecRHS[named.Obj()]

	if !ok || rhs == nil {
		return false
	}

	rhsNamed, isNamed := types.Unalias(rhs).(*types.Named)
	return isNamed && rhsNamed == base
}
