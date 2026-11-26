// golibCall.go - Gbtc
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

	"github.com/ritchiecarroll/hashset"
)

// A converted file reaches golib's intrinsics (len, append, make, copy, panic, heap, quo, rem, and
// the plumbing the converter itself writes) through `using static go.builtin`. C# binds a simple name
// to a local or parameter first, over its whole block and even before its declaration, then to a
// member of the package class, and only then to a using-static member. So a Go declaration that
// shares an intrinsic's name captures the converter's call: a local `len` turns `len(s)` into a call
// of the local's value, and a package func `tmpstring` silently runs the Go function where golib's
// was meant. Go has no such capture: its builtins are in the universe scope, and the plumbing names
// are not Go at all.
//
// Two remedies, by kind of name:
//   - the plumbing intrinsics no Go program writes (tmpstring, nonnil, trySelect, ConvertToType, …)
//     are always spelled `builtin.<name>` at their emission sites, so no declaration can capture them;
//   - the intrinsics that ARE Go builtins (len, append, make, copy, panic, recover) and heap, quo and
//     rem keep the bare spelling every converted file reads, and golibCall qualifies them only where a
//     Go declaration would bind first.
//
// Names that need neither: the golib names in `reserved` (identifierNaming.go), which a Go declaration
// is renamed away from; and methods, which are emitted with the ⁔ suffix.

// golibCall spells golib's builtin intrinsic name as the current emission site must: bare, or
// `builtin.<name>` where a Go declaration of that name would win C# simple-name lookup.
func (v *Visitor) golibCall(name string) string {
	if v.golibNameShadowed(name) {
		return "builtin." + name
	}

	return name
}

// golibNameShadowed reports whether a Go declaration named name binds first at the current site:
//   - a package-level func, variable or constant, a member of the package class. A package-level type
//     does not: an invocation's simple name skips a member that is not invocable, and a type never is
//     (GlobalCapturedInClosure's `type heap`), while a variable is kept for the value forms (quo and rem
//     are written as `rem(a, b)` with a variable `rem` in scope: MinIntDivideShadow);
//   - any object of that name declared in the enclosing function declaration, or in the package-level
//     function literal being converted: a local, parameter, result, constant, type or type-switch
//     binding, at any depth, since a C# local is in scope in its whole block;
//   - a name a dot-imported package (or, in an external test, the internal test class) declares, which
//     arrives through a second using static with equal standing.
func (v *Visitor) golibNameShadowed(name string) bool {
	if v.pkg != nil && v.pkg.Scope() != nil {
		switch v.pkg.Scope().Lookup(name).(type) {
		case *types.Func, *types.Var, *types.Const:
			return true
		}
	}

	if v.inFunction && v.golibCallScopeDeclares(name) {
		return true
	}

	return v.usingStaticAlsoDeclares(name)
}

// setGolibCallScope names the declaration whose objects golibNameShadowed consults: a function
// declaration, or a function literal converted outside any function (a package-level initializer).
// The names it declares are collected on first use.
func (v *Visitor) setGolibCallScope(scope ast.Node) {
	v.golibCallScope = scope
	v.golibCallScopeNames = nil
}

// golibCallScopeDeclares reports whether the current golib-call scope declares an object named name.
// The declaration's own name is not one of them: a method `recover` is emitted as recover⁔, and a
// function's name is a package member, which golibNameShadowed reads from the package scope.
func (v *Visitor) golibCallScopeDeclares(name string) bool {
	if v.golibCallScope == nil || v.info == nil {
		return false
	}

	if v.golibCallScopeNames == nil {
		names := hashset.HashSet[string]{}

		var ownName *ast.Ident

		if funcDecl, ok := v.golibCallScope.(*ast.FuncDecl); ok {
			ownName = funcDecl.Name
		}

		ast.Inspect(v.golibCallScope, func(n ast.Node) bool {
			switch node := n.(type) {
			case *ast.Ident:
				if node == ownName {
					return true
				}

				if obj := v.info.Defs[node]; obj != nil {
					if _, isPackageName := obj.(*types.PkgName); !isPackageName {
						names.Add(obj.Name())
					}
				}
			case *ast.CaseClause:
				if obj := v.info.Implicits[node]; obj != nil {
					names.Add(obj.Name())
				}
			}

			return true
		})

		v.golibCallScopeNames = names
	}

	return v.golibCallScopeNames.Contains(name)
}
