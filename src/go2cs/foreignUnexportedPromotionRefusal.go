// foreignUnexportedPromotionRefusal.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The refusal BY NAME of a conversion to an interface whose UNEXPORTED method the value satisfies only through a
// CONCRETE method promoted from another package, checked before any file is written (GO2CS2016).
//
// Go qualifies an unexported method name by its package. `type U struct{ lib.T }` with `func (U) m()` declares two
// methods m: lib.T's (lib.m), promoted, and U's own (main.m). An interface of lib's that declares m is satisfied by
// lib.m, so `lib.CallM(u)` calls lib.m. go2cs-gen's adapter maps an interface member by its bare name, so the same
// call reaches main.m: the converted program prints main.m where Go prints lib.m. Without an own m the adapter calls
// lib.T's m, which is internal to lib's assembly and cannot be called from the adapter's. Either way the adapter is
// wrong, so the conversion is refused until the adapter can call through the promotion path.
//
// Promotion through an embedded INTERFACE field is not refused: the adapter forwards to the field's interface member,
// which is public in C# and dispatches to the dynamic value's own method, the method Go calls
// (crypto/internal/rand's reader embeds drbg.DefaultReader and is converted to it, which must keep converting).
// Promotion from a type of the SAME package is not refused either: both sides are in one assembly, and Go's name
// qualification agrees with the bare name there.
//
// The gate examines the conversions the source writes: assignment, a typed var, a call argument, a return value, a
// composite literal element, a channel send, an explicit conversion and an equality comparison. A type assertion
// asks at run time and records no adapter, so it is outside the gate (docs/KnownIssues.md names that case).

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"sort"

	"golang.org/x/tools/go/packages"
)

// foreignUnexportedPromotionSite is one refused conversion.
type foreignUnexportedPromotionSite struct {
	position token.Position
	text     string
}

// foreignUnexportedPromotionRefusal is the error refuseForeignUnexportedPromotions returns: one canonical diagnostic per
// refused site.
type foreignUnexportedPromotionRefusal struct {
	pkgPath string
	sites   []foreignUnexportedPromotionSite
}

func (refusal *foreignUnexportedPromotionRefusal) Error() string {
	return fmt.Sprintf("package %s converts a value to an interface whose unexported method it satisfies only through a method promoted from another package (%d site(s)); see the %s diagnostics",
		refusal.pkgPath, len(refusal.sites), diagnosticForeignUnexportedPromotionRefused)
}

// report prints each refused site as a canonical diagnostic.
func (refusal *foreignUnexportedPromotionRefusal) report() {
	for _, site := range refusal.sites {
		reportDiagnostic(site.position.String(), diagnosticForeignUnexportedPromotionRefused, site.text)
	}
}

// refuseForeignUnexportedPromotions finds every refused conversion in pkg. It returns nil when there is none.
func refuseForeignUnexportedPromotions(pkg *packages.Package) *foreignUnexportedPromotionRefusal {
	if pkg.TypesInfo == nil || pkg.Types == nil {
		return nil
	}

	info := pkg.TypesInfo
	seen := map[token.Pos]bool{}
	var sites []foreignUnexportedPromotionSite

	check := func(expr ast.Expr, target types.Type) {
		if expr == nil || target == nil || seen[expr.Pos()] {
			return
		}

		tv, ok := info.Types[expr]

		if !ok || tv.Type == nil || tv.IsNil() || tv.IsType() {
			return
		}

		if text := foreignUnexportedPromotionText(tv.Type, target, pkg.Types); text != "" {
			seen[expr.Pos()] = true
			sites = append(sites, foreignUnexportedPromotionSite{position: pkg.Fset.Position(expr.Pos()), text: text})
		}
	}

	for _, file := range pkg.Syntax {
		var funcResults []*types.Tuple

		var visit func(node ast.Node) bool

		visit = func(node ast.Node) bool {
			switch n := node.(type) {
			case *ast.FuncDecl:
				if n.Body == nil {
					return false
				}

				if obj, ok := info.Defs[n.Name].(*types.Func); ok {
					funcResults = append(funcResults, obj.Signature().Results())
					ast.Inspect(n.Body, visit)
					funcResults = funcResults[:len(funcResults)-1]
				}

				return false
			case *ast.FuncLit:
				if signature, ok := info.TypeOf(n).(*types.Signature); ok {
					funcResults = append(funcResults, signature.Results())
					ast.Inspect(n.Body, visit)
					funcResults = funcResults[:len(funcResults)-1]
				}

				return false
			case *ast.ReturnStmt:
				if len(funcResults) == 0 {
					return true
				}

				results := funcResults[len(funcResults)-1]

				if results != nil && len(n.Results) == results.Len() {
					for i, result := range n.Results {
						check(result, results.At(i).Type())
					}
				}
			case *ast.AssignStmt:
				if len(n.Lhs) == len(n.Rhs) && n.Tok != token.DEFINE {
					for i, rhs := range n.Rhs {
						check(rhs, info.TypeOf(n.Lhs[i]))
					}
				}
			case *ast.ValueSpec:
				if n.Type != nil && len(n.Values) == len(n.Names) {
					target := info.TypeOf(n.Type)

					for _, value := range n.Values {
						check(value, target)
					}
				}
			case *ast.CallExpr:
				checkCallArguments(info, n, check)
			case *ast.CompositeLit:
				checkCompositeElements(info, n, check)
			case *ast.SendStmt:
				if channel, ok := underlyingOf(info.TypeOf(n.Chan)).(*types.Chan); ok {
					check(n.Value, channel.Elem())
				}
			case *ast.BinaryExpr:
				if n.Op == token.EQL || n.Op == token.NEQ {
					check(n.X, info.TypeOf(n.Y))
					check(n.Y, info.TypeOf(n.X))
				}
			}

			return true
		}

		ast.Inspect(file, visit)
	}

	if len(sites) == 0 {
		return nil
	}

	sort.SliceStable(sites, func(i, j int) bool {
		if sites[i].position.Filename != sites[j].position.Filename {
			return sites[i].position.Filename < sites[j].position.Filename
		}

		return sites[i].position.Offset < sites[j].position.Offset
	})

	return &foreignUnexportedPromotionRefusal{pkgPath: pkg.PkgPath, sites: sites}
}

// checkCallArguments checks each argument of a call against its parameter, and an explicit conversion's operand
// against the converted-to type.
func checkCallArguments(info *types.Info, call *ast.CallExpr, check func(ast.Expr, types.Type)) {
	funType, ok := info.Types[call.Fun]

	if !ok {
		return
	}

	if funType.IsType() {
		if len(call.Args) == 1 {
			check(call.Args[0], funType.Type)
		}

		return
	}

	signature, ok := underlyingOf(funType.Type).(*types.Signature)

	if !ok {
		return
	}

	params := signature.Params()

	for i, arg := range call.Args {
		var target types.Type

		switch {
		case signature.Variadic() && i >= params.Len()-1:
			if call.Ellipsis.IsValid() {
				continue
			}

			if slice, ok := params.At(params.Len() - 1).Type().(*types.Slice); ok {
				target = slice.Elem()
			}
		case i < params.Len():
			target = params.At(i).Type()
		}

		check(arg, target)
	}
}

// checkCompositeElements checks each element of a composite literal against its field, element or key type.
func checkCompositeElements(info *types.Info, literal *ast.CompositeLit, check func(ast.Expr, types.Type)) {
	literalType := info.TypeOf(literal)

	if literalType == nil {
		return
	}

	if pointer, ok := underlyingOf(literalType).(*types.Pointer); ok {
		literalType = pointer.Elem()
	}

	switch t := underlyingOf(literalType).(type) {
	case *types.Struct:
		for i, element := range literal.Elts {
			if keyValue, ok := element.(*ast.KeyValueExpr); ok {
				if key, ok := keyValue.Key.(*ast.Ident); ok {
					for f := 0; f < t.NumFields(); f++ {
						if t.Field(f).Name() == key.Name {
							check(keyValue.Value, t.Field(f).Type())
						}
					}
				}
			} else if i < t.NumFields() {
				check(element, t.Field(i).Type())
			}
		}
	case *types.Slice:
		checkElements(literal.Elts, nil, t.Elem(), check)
	case *types.Array:
		checkElements(literal.Elts, nil, t.Elem(), check)
	case *types.Map:
		checkElements(literal.Elts, t.Key(), t.Elem(), check)
	}
}

func checkElements(elements []ast.Expr, key, elem types.Type, check func(ast.Expr, types.Type)) {
	for _, element := range elements {
		if keyValue, ok := element.(*ast.KeyValueExpr); ok {
			if key != nil {
				check(keyValue.Key, key)
			}

			check(keyValue.Value, elem)
		} else {
			check(element, elem)
		}
	}
}

// foreignUnexportedPromotionText is the diagnostic text when converting a value of type source to target is refused,
// or "" when it is not: target is an interface with an unexported method that source satisfies only through a
// CONCRETE method promoted from a package other than source's own.
func foreignUnexportedPromotionText(source, target types.Type, current *types.Package) string {
	targetInterface, ok := underlyingOf(target).(*types.Interface)

	if !ok || types.IsInterface(source) {
		return ""
	}

	if _, isTypeParam := source.(*types.TypeParam); isTypeParam {
		return ""
	}

	ownPackage := current

	if named := namedOrPointerNamed(source); named != nil && named.Obj().Pkg() != nil {
		ownPackage = named.Obj().Pkg()
	}

	for i := 0; i < targetInterface.NumMethods(); i++ {
		method := targetInterface.Method(i)

		if method.Exported() || method.Pkg() == nil {
			continue
		}

		object, index, _ := types.LookupFieldOrMethod(source, false, method.Pkg(), method.Name())
		promoted, ok := object.(*types.Func)

		if !ok || len(index) < 2 || promoted.Pkg() == nil || promoted.Pkg() == ownPackage {
			continue
		}

		receiver := promoted.Signature().Recv()

		if receiver == nil || types.IsInterface(receiver.Type()) {
			continue
		}

		return fmt.Sprintf("%s is converted to %s, whose unexported method %s it satisfies only through %s's %s, promoted from package %s. Go keeps that method apart from any %s of %s's own package; go2cs maps an interface method by its bare name, so the converted call would reach the wrong method or one it cannot call. go2cs refuses this conversion",
			types.TypeString(source, types.RelativeTo(current)), types.TypeString(target, types.RelativeTo(current)), method.Name(),
			types.TypeString(receiver.Type(), types.RelativeTo(current)), method.Name(), promoted.Pkg().Path(), method.Name(),
			types.TypeString(source, types.RelativeTo(current)))
	}

	return ""
}

// namedOrPointerNamed is t's named type, or the named type t points to, or nil.
func namedOrPointerNamed(t types.Type) *types.Named {
	t = types.Unalias(t)

	if pointer, ok := t.(*types.Pointer); ok {
		t = types.Unalias(pointer.Elem())
	}

	named, _ := t.(*types.Named)

	return named
}
