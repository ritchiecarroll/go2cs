// lazyPackageVarOperations.go - Gbtc
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

// Package-level VARS emitted LAZILY: built on first read instead of in the package's static constructor. Go's linker
// emits a table of constants and function values as static DATA, so it costs a Go program nothing at startup; the
// converted form is a C# initializer the package's static constructor runs, JIT-compiling the whole table before
// main. A listed var is declared as a `ref` property over a nested holder class whose field initializer the CLR runs
// on the first read, and package_init.cs does not call it (COORD's ruling (A), 2026-10-22).
//
// That changes only WHEN the value is built, which is unobservable exactly when the initializer reads nothing that
// can change and does nothing a reader could see: a composite literal of constants and package function values, with
// no call, no func literal and no read of a package var (lazyPackageVarRefusal enforces it, and refuses a listed var by
// name otherwise). Go 1.26's runtime malloc dispatch tables are that shape: [513]func values per table, read only by
// mallocgc's size-specialized branch, whose gate (sizeSpecializedMallocEnabled) is a const false here because
// goexperiment.SizeSpecializedMalloc is off, so no path reads them at all. Built eagerly they were 2 x 17.1K IL
// JIT-compiled at every startup. Keyed by package path, then the var's Go name.
var lazyPackageVars = map[string]map[string]bool{
	"runtime": {
		"mallocScanTable":   true,
		"mallocNoScanTable": true,
	},
}

// isLazyPackageVar reports whether the package-level var is listed in lazyPackageVars.
func isLazyPackageVar(obj types.Object) bool {
	if obj == nil || obj.Pkg() == nil {
		return false
	}

	if _, isVar := obj.(*types.Var); !isVar || obj.Parent() != obj.Pkg().Scope() {
		return false
	}

	return lazyPackageVars[resolveGorootVendoredPath(obj.Pkg().Path())][obj.Name()]
}

// lazyPackageVarRefusal returns why a listed var cannot be built on its first read, or "" when it can: it must be
// declared alone, never have its address taken, and be initialized by an expression that reads nothing that can change
// and has no effect a reader could see (constants, types and package function values, composed by literals).
func (v *Visitor) lazyPackageVarRefusal(valueSpec *ast.ValueSpec, ident *ast.Ident) string {
	if len(valueSpec.Names) != 1 || len(valueSpec.Values) != 1 {
		return "it shares its spec with other names or has no single initializer"
	}

	if v.isAddressedGlobal(ident) {
		return "its address is taken, and a lazily built value has no field to point at"
	}

	refusal := ""

	ast.Inspect(valueSpec.Values[0], func(n ast.Node) bool {
		if refusal != "" {
			return false
		}

		switch node := n.(type) {
		case *ast.CallExpr:
			// A conversion is a type applied to a value, not a call.
			if tv, ok := v.info.Types[node.Fun]; !ok || !tv.IsType() {
				refusal = "its initializer makes a call, which would run on the first read instead of at init"
			}
		case *ast.FuncLit:
			refusal = "its initializer contains a func literal"
		case *ast.UnaryExpr:
			if node.Op == token.AND || node.Op == token.ARROW {
				refusal = fmt.Sprintf("its initializer applies %s, which takes an address or receives", node.Op)
			}
		case *ast.Ident:
			// A struct literal's key resolves to the FIELD it names, which is not a read.
			if obj, ok := v.info.Uses[node].(*types.Var); ok && !obj.IsField() {
				refusal = fmt.Sprintf("its initializer reads the var %s, whose value at the first read may differ from its value at init", obj.Name())
			}
		}

		return refusal == ""
	})

	return refusal
}

// writeLazyPackageVar declares a listed var as a ref property over a holder class whose field initializer builds the
// value on the first read: `mallocScanTable` reads and writes exactly as the field did, and lazyᴛmallocScanTable is
// the holder.
func (v *Visitor) writeLazyPackageVar(access, csTypeName, csIDName, valExpr string) {
	holder := "lazy" + TempVarMarker + csIDName

	v.writeOutput("%s static ref %s %s => ref %s.value;", access, csTypeName, csIDName, holder)
	v.outputBuilder.WriteString(v.newline)
	v.writeOutput("private static class %s { internal static %s value = %s; }", holder, csTypeName, valExpr)
}
