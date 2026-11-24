// shapeWitness.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The NARROW SHAPE WITNESS. Design record: DESIGN-shape-witness-scout.md, sections 2 and 7-9, at commit 05e789558d
// (reviewed there; the record is not on master). The user-facing rule is GO2CS2013's row in docs/DiagnosticCodes.md.
//
// A generic declaration whose type parameter is under a NON-self method-set constraint, and whose own signature puts
// that parameter where the constraint proxies refuse it -- a generic NAMED type (ruling 1), or T inside a result, a
// map, a chan, a pointer, a nested slice or a generic named type (section 5) -- takes a WITNESS instead of a proxy:
//
//   - T stays the box (or the value) itself, so a result, a slice and a named type keep Go's own element type, Go's
//     aliasing, and Go's dynamic type through `any`;
//   - the declaration gains one type parameter per selected T, `where TI : struct, IΔshape<T>`, and every method call
//     through T becomes a static call through the witness, `TI.M(x)`;
//   - a generic named type whose OWN methods call T's methods carries the witness parameter too, so every mention of
//     it names the canonical witness for its pair.
//
// STAGE 1 (this file's scope): every piece in one package. A pair that needs another package's witness, an
// instantiation of another package's selected declaration, a method value through T, or a conversion of T to an
// interface is REFUSED BY NAME (GO2CS2013), so nothing silently miscompiles. Stage 2 adds the cross-package rules.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

// ShapeWitnessSuffix names a witness interface and its implementations: IΔshape<T>, IΔshapeOf<T>, EжIΔshape.
const ShapeWitnessSuffix = "Δshape"

// shapeWitnessDecl is one selected declaration of the package being converted.
type shapeWitnessDecl struct {
	obj     types.Object             // the origin *types.Func or *types.TypeName
	tps     map[int]*types.TypeParam // the selected positions, by the declaration's own type-parameter index
	carrier bool                     // a function always carries witness parameters; a named type only when its own methods call T's
}

// Package state: the selection, and the witness declarations the package emits into package_info.cs's <ShapeWitnesses>
// section (witness interfaces, generic witnesses, pair witnesses), one rendered line per declaration, keyed by its name.
var (
	shapeWitnessDecls   = map[types.Object]*shapeWitnessDecl{}
	shapeWitnessPairs   = map[string]string{}
	shapeWitnessForeign = map[string]map[types.Object]*shapeWitnessDecl{}
)

// ShapeWitnessSection is package_info.cs's section of per-pair witnesses, inside the package class.
const ShapeWitnessSection = "ShapeWitnesses"

// shapeWitnessPair is one per-pair witness the package emits: `EжIΔshape` (pointer) or `EᴠIΔshape` (value).
type shapeWitnessPair struct {
	name    string     // the witness struct's C# name
	elem    types.Type // the type argument the witness serves (the box's element for a pointer)
	pointer bool
	iface   *types.Named // the constraint interface, instantiated
}

func resetShapeWitnessState() {
	shapeWitnessDecls = map[types.Object]*shapeWitnessDecl{}
	shapeWitnessPairs = map[string]string{}
}

// shapeWitnessSelfReferential is the SELECTION-ONLY predicate (decision (b), 2026-11-18): the constraint mentions
// the type parameter anywhere, interface METHOD SIGNATURES included, so `interface{ Less(T) bool }` is self-referential
// and stays on the self route. typeMentionsTypeParam itself is not changed: it has nine other callers.
func shapeWitnessSelfReferential(tp *types.TypeParam) bool {
	if typeMentionsTypeParam(tp.Constraint(), tp, map[types.Type]bool{}) {
		return true
	}

	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok {
		return false
	}

	for i := range iface.NumMethods() {
		if typeMentionsTypeParam(iface.Method(i).Type(), tp, map[types.Type]bool{}) {
			return true
		}
	}

	return false
}

// shapeWitnessConstraint reports whether the type parameter's constraint is one a witness can carry: a NON-self method
// set, NAMED, and declared in the type parameter's own package. Stage 1 emits a witness interface only beside a
// same-package constraint, so an inline constraint (lifted by the converter) or another package's interface keeps
// today's routes -- the proxy where it applies, GO2CS2005/2006 where it does not -- until stage 2.
func shapeWitnessConstraint(tp *types.TypeParam) bool {
	named, ok := types.Unalias(tp.Constraint()).(*types.Named)

	if !ok || named.Obj().Pkg() == nil || tp.Obj().Pkg() == nil || named.Obj().Pkg().Path() != tp.Obj().Pkg().Path() {
		return false
	}

	iface, ok := named.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 || !(iface.IsMethodSet() || isMethodSetWithPointerNamedUnion(iface)) {
		return false
	}

	return !shapeWitnessSelfReferential(tp)
}

// shapeWitnessAliasingReach reports whether t, a parameter or result of the signature, carries the type parameter where
// today's routes refuse it. A plain []T PARAMETER is class B's position and does not count.
func shapeWitnessAliasingReach(t types.Type, tp *types.TypeParam, isParam bool) bool {
	switch x := types.Unalias(t).(type) {
	case *types.Slice:
		if elem, ok := types.Unalias(x.Elem()).(*types.TypeParam); ok && elem == tp && isParam {
			return false
		}

		return typeMentionsTypeParam(x.Elem(), tp, map[types.Type]bool{})
	case *types.Map, *types.Chan, *types.Pointer, *types.Array, *types.Struct:
		return typeMentionsTypeParam(x, tp, map[types.Type]bool{})
	case *types.Named:
		return x.TypeArgs() != nil && typeMentionsTypeParam(x, tp, map[types.Type]bool{})
	}

	return false
}

// shapeWitnessMethodsUseTypeParam reports whether a generic named type's OWN methods call a method through one of its
// type parameters: then the type carries the witness parameter.
func shapeWitnessMethodsUseTypeParam(pkg *packages.Package, named *types.Named) bool {
	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Recv == nil || funcDecl.Body == nil {
				continue
			}

			fn, ok := pkg.TypesInfo.Defs[funcDecl.Name].(*types.Func)

			if !ok {
				continue
			}

			recv := fn.Type().(*types.Signature).Recv()

			if recv == nil || shapeWitnessReceiverNamed(recv.Type()) != named.Obj() {
				continue
			}

			uses := false

			ast.Inspect(funcDecl.Body, func(n ast.Node) bool {
				if sel, ok := n.(*ast.SelectorExpr); ok {
					if tv, ok := pkg.TypesInfo.Types[sel.X]; ok {
						if _, isTypeParam := types.Unalias(tv.Type).(*types.TypeParam); isTypeParam {
							if selection, isSelection := pkg.TypesInfo.Selections[sel]; isSelection && selection.Kind() != types.FieldVal {
								uses = true
							}
						}
					}
				}

				return !uses
			})

			if uses {
				return true
			}
		}
	}

	return false
}

// shapeWitnessReceiverNamed is the origin type name of a method receiver's (possibly pointer) named type.
func shapeWitnessReceiverNamed(t types.Type) *types.TypeName {
	t = types.Unalias(t)

	if ptr, ok := t.(*types.Pointer); ok {
		t = types.Unalias(ptr.Elem())
	}

	if named, ok := t.(*types.Named); ok {
		return named.Origin().Obj()
	}

	return nil
}

// shapeWitnessOrigin is the declaration an instantiating identifier names: a generic function's origin or a type name.
func shapeWitnessOrigin(info *types.Info, ident *ast.Ident) types.Object {
	switch object := info.Uses[ident].(type) {
	case *types.Func:
		return object.Origin()
	case *types.TypeName:
		return object
	}

	return nil
}

// computeShapeWitnessSelection selects the package's narrow declarations: declaration-level selection, the closure over
// bodies and types to a fixed point, upward and downward (review MUST 1, widened by the re-read's finding 3 and stage 1's
// downward discovery), and the predicate: a declaration is kept only if an instantiation in the package binds a
// selected position to a pointer, or a kept declaration forwards into it -- exported declarations included until
// stage 2 (decision on the stage-1 review's finding 3).
func computeShapeWitnessSelection(pkg *packages.Package) map[types.Object]*shapeWitnessDecl {
	return computeShapeWitnessSelectionFiles(pkg, nil)
}

// computeShapeWitnessSelectionFiles is the selection over the files include admits (all, for nil): the -tests variant
// asks it once with every file and once with production's files alone, so the two can be compared (finding 6).
func computeShapeWitnessSelectionFiles(pkg *packages.Package, include func(*ast.File) bool) map[types.Object]*shapeWitnessDecl {
	selected := map[types.Object]*shapeWitnessDecl{}

	if pkg == nil || pkg.TypesInfo == nil {
		return selected
	}

	var files []*ast.File

	for _, file := range pkg.Syntax {
		if include == nil || include(file) {
			files = append(files, file)
		}
	}

	v := newGenericRouteVisitor(pkg)

	// Declaration-level selection.
	for _, file := range files {
		for _, decl := range file.Decls {
			switch decl := decl.(type) {
			case *ast.FuncDecl:
				if decl.Recv != nil || decl.Type.TypeParams == nil {
					continue
				}

				fn, ok := pkg.TypesInfo.Defs[decl.Name].(*types.Func)

				if !ok {
					continue
				}

				sig := fn.Type().(*types.Signature)
				tps := map[int]*types.TypeParam{}

				for i := range sig.TypeParams().Len() {
					tp := sig.TypeParams().At(i)

					if !shapeWitnessConstraint(tp) || !v.methodSetConstraintNeeded(fn, i) {
						continue
					}

					for j := range sig.Params().Len() {
						if shapeWitnessAliasingReach(sig.Params().At(j).Type(), tp, true) {
							tps[i] = tp
						}
					}

					for j := range sig.Results().Len() {
						if shapeWitnessAliasingReach(sig.Results().At(j).Type(), tp, false) {
							tps[i] = tp
						}
					}
				}

				if len(tps) > 0 {
					selected[fn] = &shapeWitnessDecl{obj: fn, tps: tps, carrier: true}
				}
			case *ast.GenDecl:
				for _, spec := range decl.Specs {
					typeSpec, ok := spec.(*ast.TypeSpec)

					if !ok || typeSpec.TypeParams == nil {
						continue
					}

					typeName, ok := pkg.TypesInfo.Defs[typeSpec.Name].(*types.TypeName)

					if !ok {
						continue
					}

					named, ok := typeName.Type().(*types.Named)

					if !ok || named.TypeParams() == nil || types.IsInterface(named) {
						continue
					}

					tps := map[int]*types.TypeParam{}

					for i := range named.TypeParams().Len() {
						if shapeWitnessConstraint(named.TypeParams().At(i)) {
							tps[i] = named.TypeParams().At(i)
						}
					}

					if len(tps) > 0 {
						selected[typeName] = &shapeWitnessDecl{obj: typeName, tps: tps, carrier: shapeWitnessMethodsUseTypeParam(pkg, named)}
					}
				}
			}
		}
	}

	// The closure, recording forwarders for the predicate. Every generic spec of a `type ( ... )` group is a scope of its
	// own (finding 2).
	forwarders := map[types.Object][]types.Object{}

	for changed := true; changed; {
		changed = false

		for _, file := range files {
			for _, decl := range file.Decls {
				for _, sc := range shapeWitnessClosureScopes(pkg, decl) {
					owner, own, node, isType := sc.owner, sc.own, sc.node, sc.isType
					found := map[int]*types.TypeParam{}
					foundCarrier := false

					ast.Inspect(node, func(n ast.Node) bool {
						ident, ok := n.(*ast.Ident)

						if !ok {
							return true
						}

						instance, ok := pkg.TypesInfo.Instances[ident]

						if !ok || instance.TypeArgs == nil {
							return true
						}

						origin := shapeWitnessOrigin(pkg.TypesInfo, ident)
						target := selected[origin]

						// DOWNWARD: a selected body that hands its selected T (the box itself, with no C# clause) to a
						// same-package generic FUNCTION whose clause would be kept (`nameOf[U Named](x U)` calling x.Name())
						// selects that function at the position, or the box would meet a nominal C# constraint it cannot
						// satisfy. Its forwarder edge keeps it exactly as long as the caller is kept (the predicate below),
						// and the caller, now forwarding into a carrier, becomes a carrier itself (UPWARD, below).
						if ownerDecl := selected[owner]; ownerDecl != nil && origin != owner {
							if fn, isFunc := origin.(*types.Func); isFunc && fn.Pkg() != nil && fn.Pkg().Path() == pkg.Types.Path() {
								if sig := fn.Type().(*types.Signature); sig.Recv() == nil && sig.TypeParams() != nil {
									for i := range min(sig.TypeParams().Len(), instance.TypeArgs.Len()) {
										if target != nil && target.tps[i] != nil {
											continue
										}

										argTypeParam, isTypeParam := types.Unalias(instance.TypeArgs.At(i)).(*types.TypeParam)

										if !isTypeParam || !shapeWitnessOwnsTypeParam(ownerDecl, own, argTypeParam) {
											continue
										}

										if !shapeWitnessConstraint(sig.TypeParams().At(i)) || !v.methodSetConstraintNeeded(fn, i) {
											continue
										}

										if target == nil {
											target = &shapeWitnessDecl{obj: fn, tps: map[int]*types.TypeParam{}, carrier: true}
											selected[fn] = target
										}

										target.tps[i] = sig.TypeParams().At(i)
										forwarders[fn] = appendUniqueObject(forwarders[fn], owner)
										changed = true
									}
								}
							}
						}

						if target == nil || origin == owner {
							return true
						}

						for i := range target.tps {
							if i >= instance.TypeArgs.Len() {
								continue
							}

							for j, tp := range own {
								if typeMentionsTypeParam(instance.TypeArgs.At(i), tp, map[types.Type]bool{}) {
									if isType {
										found[j] = owner.Type().(*types.Named).TypeParams().At(j)
									} else {
										found[j] = tp
									}

									forwarders[origin] = appendUniqueObject(forwarders[origin], owner)

									if target.carrier {
										foundCarrier = true
									}
								}
							}
						}

						return true
					})

					// UPWARD: the owner forwards its own type parameter into a selected declaration, so it is selected at
					// that position. A function always carries its witness parameters; a TYPE carries them when its own
					// methods call T's, or when it forwards T into a carrier (it must name that carrier's witness).
					if len(found) > 0 {
						ownerDecl := selected[owner]

						if ownerDecl == nil {
							carrier := !isType || foundCarrier || shapeWitnessMethodsUseTypeParam(pkg, owner.Type().(*types.Named))
							selected[owner] = &shapeWitnessDecl{obj: owner, tps: found, carrier: carrier}
							changed = true
						} else {
							for j, tp := range found {
								if ownerDecl.tps[j] == nil {
									ownerDecl.tps[j] = tp
									changed = true
								}
							}

							if foundCarrier && !ownerDecl.carrier {
								ownerDecl.carrier = true
								changed = true
							}
						}
					}
				}
			}
		}
	}

	// THE PREDICATE (from review, under the stage-1 decision of 2026-11-19 on finding 3): a declaration is kept only where
	// an instantiation in the files read binds a selected position to a POINTER, or a kept declaration forwards into
	// it. An exported declaration takes the same predicate until stage 2 lands, so a declaration nothing in its own
	// package instantiates with a pointer is never witnessed: other packages keep instantiating it as they do today.
	// A pointer is also exactly what today's GO2CS2005 refuses for these shapes (ruling 1, section 5), so a stage-1
	// refusal (GO2CS2013) only ever stands where master already refuses by name.
	kept := map[types.Object]bool{}

	for _, file := range files {
		ast.Inspect(file, func(n ast.Node) bool {
			ident, ok := n.(*ast.Ident)

			if !ok {
				return true
			}

			instance, ok := pkg.TypesInfo.Instances[ident]

			if !ok || instance.TypeArgs == nil {
				return true
			}

			origin := shapeWitnessOrigin(pkg.TypesInfo, ident)
			decl := selected[origin]

			if decl == nil {
				return true
			}

			for i := range decl.tps {
				if i < instance.TypeArgs.Len() && shapeWitnessNonNominal(instance.TypeArgs.At(i)) {
					kept[origin] = true
				}
			}

			return true
		})
	}

	for changed := true; changed; {
		changed = false

		for object := range selected {
			if kept[object] {
				continue
			}

			for _, forwarder := range forwarders[object] {
				if kept[forwarder] {
					kept[object] = true
					changed = true
				}
			}
		}
	}

	for object := range selected {
		if !kept[object] {
			delete(selected, object)
		}
	}

	return selected
}

// shapeWitnessScopeSpec is one closure scope: an owner, the type parameters its syntax declares, and that syntax.
type shapeWitnessScopeSpec struct {
	owner  types.Object
	own    []*types.TypeParam
	node   ast.Node
	isType bool
}

// shapeWitnessClosureScopes are a declaration's closure scopes: a generic function (signature and body), a method of a
// generic type (counted as its type), or EVERY generic named type of a `type ( ... )` group, each its own scope.
func shapeWitnessClosureScopes(pkg *packages.Package, decl ast.Decl) []shapeWitnessScopeSpec {
	switch decl := decl.(type) {
	case *ast.FuncDecl:
		fn, ok := pkg.TypesInfo.Defs[decl.Name].(*types.Func)

		if !ok {
			return nil
		}

		sig := fn.Type().(*types.Signature)

		if decl.Recv != nil {
			if sig.RecvTypeParams() == nil {
				return nil
			}

			owner := shapeWitnessReceiverNamed(sig.Recv().Type())

			if owner == nil {
				return nil
			}

			own := make([]*types.TypeParam, sig.RecvTypeParams().Len())

			for i := range own {
				own[i] = sig.RecvTypeParams().At(i)
			}

			return []shapeWitnessScopeSpec{{owner: owner, own: own, node: decl, isType: true}}
		}

		if sig.TypeParams() == nil {
			return nil
		}

		own := make([]*types.TypeParam, sig.TypeParams().Len())

		for i := range own {
			own[i] = sig.TypeParams().At(i)
		}

		return []shapeWitnessScopeSpec{{owner: fn, own: own, node: decl, isType: false}}
	case *ast.GenDecl:
		var scopes []shapeWitnessScopeSpec

		for _, spec := range decl.Specs {
			typeSpec, ok := spec.(*ast.TypeSpec)

			if !ok || typeSpec.TypeParams == nil {
				continue
			}

			typeName, ok := pkg.TypesInfo.Defs[typeSpec.Name].(*types.TypeName)

			if !ok {
				continue
			}

			named, ok := typeName.Type().(*types.Named)

			if !ok || named.TypeParams() == nil {
				continue
			}

			own := make([]*types.TypeParam, named.TypeParams().Len())

			for i := range own {
				own[i] = named.TypeParams().At(i)
			}

			scopes = append(scopes, shapeWitnessScopeSpec{owner: typeName, own: own, node: typeSpec, isType: true})
		}

		return scopes
	}

	return nil
}

func appendUniqueObject(list []types.Object, object types.Object) []types.Object {
	for _, existing := range list {
		if existing == object {
			return list
		}
	}

	return append(list, object)
}

// shapeWitnessNonNominal reports whether an instantiation binds a type argument stage 1 witnesses: a POINTER, the box
// golib cannot make implement the constraint, and the one argument today's routes refuse for these shapes. A value of
// any package, or an interface, keeps today's route (stage-1 decision, 2026-11-19).
func shapeWitnessNonNominal(arg types.Type) bool {
	_, isPointer := types.Unalias(arg).(*types.Pointer)

	return isPointer
}

// shapeWitnessForeignSelected reports whether an object of ANOTHER package is a selected declaration there, computed
// from that package's loaded syntax (cached per package path). Stage 1 refuses instantiating one.
func shapeWitnessForeignSelected(object types.Object) *shapeWitnessDecl {
	if object == nil || object.Pkg() == nil || currentLoadedPackage == nil {
		return nil
	}

	path := object.Pkg().Path()

	selection, cached := shapeWitnessForeign[path]

	if !cached {
		selection = map[types.Object]*shapeWitnessDecl{}

		if loaded := loadedPackageSource(currentLoadedPackage, object.Pkg()); loaded != nil && loaded.TypesInfo != nil && len(loaded.Syntax) > 0 {
			// The selection's route visitor re-points currentLoadedPackage at the package it reads; this package's
			// graph is restored after, as every later read of it expects.
			saved := currentLoadedPackage
			selection = computeShapeWitnessSelection(loaded)
			currentLoadedPackage = saved
		}

		shapeWitnessForeign[path] = selection
	}

	for selectedObject, decl := range selection {
		if selectedObject.Name() == object.Name() && selectedObject.Pos() == object.Pos() {
			return decl
		}
	}

	return nil
}

// ---- naming ----

// shapeWitnessBaseName is the C# name of an interface's origin, without type arguments (`Script`, `Δwrap`).
func (v *Visitor) shapeWitnessBaseName(iface *types.Named) string {
	name := v.getCSharpTypeName(iface.Origin())

	if i := strings.Index(name, "<"); i >= 0 {
		name = name[:i]
	}

	return name
}

// shapeWitnessInterfaceRef is the witness interface for a constraint, instantiated at self: `ScriptΔshape<ж<Source>>`,
// and for a generic constraint `ΔwrapΔshape<W, T>` (its own type arguments follow self).
func (v *Visitor) shapeWitnessInterfaceRef(iface *types.Named, self string) string {
	args := []string{self}

	if iface.TypeArgs() != nil {
		for i := range iface.TypeArgs().Len() {
			args = append(args, v.getCSharpTypeName(iface.TypeArgs().At(i)))
		}
	}

	return fmt.Sprintf("%s%s<%s>", v.shapeWitnessBaseName(iface), ShapeWitnessSuffix, strings.Join(args, ", "))
}

// shapeWitnessParamName is the witness type parameter for a declaration's type parameter: its name, the converter's
// reserved Δ marker, and the constraint's name (`TΔScript`, `WΔwrap`), so the body's `TΔScript.Text(s)` reads back to
// `s.Text()`. The marker keeps it apart from any name the Go source declares (review NIT): a plain `TScript` could be a
// user's own type parameter or package-level type.
func (v *Visitor) shapeWitnessParamName(tp *types.TypeParam) string {
	iface, _ := types.Unalias(tp.Constraint()).(*types.Named)
	name := getSanitizedIdentifier(tp.Obj().Name())

	if iface == nil {
		return name + "Δᴛ"
	}

	return name + "Δ" + strings.TrimPrefix(v.shapeWitnessBaseName(iface), "Δ")
}

// ---- scope ----

// shapeWitnessScope is the witness parameters in scope while a declaration's body or type emits, by type parameter.
type shapeWitnessScope map[*types.TypeParam]string

// indices is the declaration's selected type-parameter positions, ascending: the order its witness parameters take.
func (decl *shapeWitnessDecl) indices() []int {
	result := make([]int, 0, len(decl.tps))

	for i := range decl.tps {
		result = append(result, i)
	}

	sort.Ints(result)

	return result
}

// shapeWitnessDeclOfList is the selected declaration a type-parameter list belongs to, for getGenericDefinition: a
// generic named type, a method of one (its receiver type parameters), or the function being visited.
func (v *Visitor) shapeWitnessDeclOfList(named *types.Named, signature *types.Signature, typeParams *types.TypeParamList) *shapeWitnessDecl {
	if len(shapeWitnessDecls) == 0 || typeParams == nil {
		return nil
	}

	if owner := genericOwner(named, signature, typeParams); owner != nil {
		return shapeWitnessDecls[owner.Obj()]
	}

	if signature != nil && typeParams == signature.TypeParams() {
		if fn := v.currentGenericFunc(signature); fn != nil {
			return shapeWitnessDecls[fn]
		}
	}

	return nil
}

// currentShapeWitnessScope is the witness parameters of the declaration being emitted: the function, the method's
// receiver type, or the generic type whose fields are being written.
func (v *Visitor) currentShapeWitnessScope() shapeWitnessScope {
	if len(shapeWitnessDecls) == 0 {
		return nil
	}

	scope := shapeWitnessScope{}

	add := func(owner types.Object, list *types.TypeParamList) {
		decl := shapeWitnessDecls[owner]

		if decl == nil || !decl.carrier || list == nil {
			return
		}

		for i := range decl.tps {
			if i < list.Len() {
				scope[list.At(i)] = v.shapeWitnessParamName(list.At(i))
			}
		}
	}

	if v.currentFuncDecl != nil && v.info != nil {
		if fn, ok := v.info.Defs[v.currentFuncDecl.Name].(*types.Func); ok {
			sig := fn.Type().(*types.Signature)

			if sig.RecvTypeParams() != nil {
				if owner := shapeWitnessReceiverNamed(sig.Recv().Type()); owner != nil {
					add(owner, sig.RecvTypeParams())
				}
			} else {
				add(fn, sig.TypeParams())
			}
		}
	}

	if v.shapeWitnessTypeOwner != nil {
		add(v.shapeWitnessTypeOwner.Obj(), v.shapeWitnessTypeOwner.TypeParams())
	}

	return scope
}

// shapeWitnessReceiverCall is the witness a method call through x routes to (`TΔScript` for `s.Text()` with s a T), or
// "" when x is not a witnessed type parameter of the declaration being emitted.
func (v *Visitor) shapeWitnessReceiverCall(x ast.Expr) string {
	if len(shapeWitnessDecls) == 0 {
		return ""
	}

	tv, ok := v.info.Types[x]

	if !ok {
		return ""
	}

	tp, ok := types.Unalias(tv.Type).(*types.TypeParam)

	if !ok {
		return ""
	}

	return v.currentShapeWitnessScope()[tp]
}

// ---- resolution ----

// shapeWitnessArg is the canonical witness for (arg, constraint) at one instantiation, or "" with the reason a stage-1
// cut cannot place it. scope holds the enclosing declaration's witness parameters. render is false for the refusal
// pass, which only asks WHETHER a witness exists: nothing is rendered or recorded. goForm renders the Go-shaped
// spelling getAliasQualifiedTypeName composes (`ValueΔshapeOf[u8]`), which convertToCSTypeName then converts.
func (v *Visitor) shapeWitnessArg(arg types.Type, constraint types.Type, scope shapeWitnessScope, render bool, goForm bool) (string, string) {
	iface, ok := types.Unalias(constraint).(*types.Named)

	if !ok || !types.IsInterface(iface) {
		return "", fmt.Sprintf("its constraint %s is not a named interface", constraint)
	}

	ifacePkg := iface.Obj().Pkg()

	if ifacePkg == nil || v.pkg == nil || ifacePkg.Path() != v.pkg.Path() {
		return "", fmt.Sprintf("the constraint %s is declared in another package (stage 2)", iface.Obj().Name())
	}

	ifaceType, _ := iface.Underlying().(*types.Interface)

	if ifaceType == nil {
		return "", "the constraint has no method set"
	}

	open, close := "<", ">"

	if goForm {
		open, close = "[", "]"
	}

	typeName := func(t types.Type) string {
		if goForm {
			return v.getAliasQualifiedTypeName(t, false)
		}

		return v.getCSharpTypeName(t)
	}

	switch x := types.Unalias(arg).(type) {
	case *types.TypeParam:
		if name, ok := scope[x]; ok {
			if enclosing, ok := types.Unalias(x.Constraint()).(*types.Named); ok && (types.Identical(enclosing, iface) || shapeWitnessEmbeds(enclosing, iface)) {
				return name, ""
			}

			return "", fmt.Sprintf("the type parameter %s forwards a witness for %s where %s is required", x.Obj().Name(), x.Constraint(), iface.Obj().Name())
		}

		return "", fmt.Sprintf("the type parameter %s has no witness in scope", x.Obj().Name())
	case *types.Pointer:
		elem, ok := types.Unalias(x.Elem()).(*types.Named)

		if !ok || elem.TypeArgs() != nil || elem.Obj().Pkg() == nil || elem.Obj().Pkg().Path() != v.pkg.Path() {
			return "", fmt.Sprintf("the pointer %s is not to a non-generic named type of this package (stage 2)", x)
		}

		if iface.TypeArgs() != nil && iface.TypeArgs().Len() > 0 {
			return "", fmt.Sprintf("a pointer witness for the generic constraint %s is stage 2", iface.Obj().Name())
		}

		if !render {
			return PointerPrefix, ""
		}

		name := fmt.Sprintf("%s%s%s%s", getSanitizedIdentifier(elem.Obj().Name()), PointerPrefix, strings.TrimPrefix(v.shapeWitnessBaseName(iface), "Δ"), ShapeWitnessSuffix)
		v.recordShapeWitnessInterface(iface)

		if _, recorded := shapeWitnessPairs[name]; !recorded {
			// Files convert sequentially (processConversion), and this may run inside a type rendering a caller does
			// under packageLock, so the record takes no lock of its own.
			shapeWitnessPairs[name] = v.shapeWitnessPairDecl(shapeWitnessPair{name: name, elem: elem, pointer: true, iface: iface})
		}

		return name, ""
	case *types.Named:
		if x.Obj().Pkg() == nil || x.Obj().Pkg().Path() != v.pkg.Path() {
			return "", fmt.Sprintf("the type %s is declared in another package (stage 2)", x.Obj().Name())
		}

		if types.IsInterface(x) && !types.Identical(x, iface) && !shapeWitnessEmbeds(x, iface) {
			return "", fmt.Sprintf("the interface %s satisfies %s only structurally (stage 2)", x.Obj().Name(), iface.Obj().Name())
		}

		if !types.IsInterface(x) && !types.Implements(x, ifaceType) {
			return "", fmt.Sprintf("only *%s implements %s (stage 2 for a value witness)", x.Obj().Name(), iface.Obj().Name())
		}

		if !render {
			return "Of", ""
		}

		v.recordShapeWitnessInterface(iface)

		// A GENERIC constraint's nominal record is closed over this instantiation (`GoImplement<Ptr<u8, …>, Δwrap<…>>`),
		// so the use site makes it; a non-generic one is recorded declaration-driven with its interface.
		if iface.TypeArgs() != nil && iface.TypeArgs().Len() > 0 && !types.IsInterface(x) {
			v.convertToInterfaceType(iface, x, "")
		}

		// The cut makes the nominal record DECLARATION-driven for a selected interface (recordShapeWitnessImplementations):
		// E implements I in C# because its own assembly carries GoImplement<E, I>, cast or no cast (DESIGN 8.1).
		args := []string{typeName(x)}

		if iface.TypeArgs() != nil {
			for i := range iface.TypeArgs().Len() {
				args = append(args, typeName(iface.TypeArgs().At(i)))
			}
		}

		return fmt.Sprintf("%s%sOf%s%s%s", v.shapeWitnessBaseName(iface), ShapeWitnessSuffix, open, strings.Join(args, ", "), close), ""
	}

	return "", fmt.Sprintf("the type argument %s has no witness form", arg)
}

// shapeWitnessInstanceArgs is the witness arguments an instantiation of a selected carrier appends after its declared
// type arguments, or nil when origin is not one.
func (v *Visitor) shapeWitnessInstanceArgs(origin types.Object, typeArgs *types.TypeList, goForm bool) []string {
	decl := shapeWitnessDeclFor(origin)

	if decl == nil || !decl.carrier || typeArgs == nil {
		return nil
	}

	scope := v.currentShapeWitnessScope()
	result := []string{}

	for _, i := range decl.indices() {
		if i >= typeArgs.Len() {
			continue
		}

		tp := decl.tps[i]
		name, reason := v.shapeWitnessArg(typeArgs.At(i), shapeWitnessInstantiatedConstraint(tp, typeArgs), scope, true, goForm)

		if reason != "" {
			// The refusal pass refuses every such site before any file is written; reaching here is a converter defect,
			// so the emission names it rather than compiling a guess.
			v.showWarning("@shapeWitnessInstanceArgs - %s[%s] at %s: %s", origin.Name(), tp.Obj().Name(), typeArgs.At(i), reason)
			name = "ΔshapeWitnessUnresolved"
		}

		result = append(result, name)
	}

	return result
}

// shapeWitnessEmbeds reports whether interface outer embeds inner, directly or transitively.
func shapeWitnessEmbeds(outer, inner *types.Named) bool {
	it, ok := outer.Underlying().(*types.Interface)

	if !ok {
		return false
	}

	for i := range it.NumEmbeddeds() {
		embedded, ok := types.Unalias(it.EmbeddedType(i)).(*types.Named)

		if !ok {
			continue
		}

		if types.Identical(embedded, inner) || shapeWitnessEmbeds(embedded, inner) {
			return true
		}
	}

	return false
}

// shapeWitnessDeclFor is the selected declaration an object names: this package's, or nil.
func shapeWitnessDeclFor(object types.Object) *shapeWitnessDecl {
	if object == nil {
		return nil
	}

	if fn, ok := object.(*types.Func); ok {
		object = fn.Origin()
	}

	return shapeWitnessDecls[object]
}

// ---- emission ----

// shapeWitnessMethods is an interface's method set split for its witness interface: the methods it declares itself
// (inherited from no same-package embedded witness), and the bases it inherits.
func (v *Visitor) shapeWitnessMethods(named *types.Named) ([]*types.Func, []*types.Named) {
	it := named.Underlying().(*types.Interface)
	inherited := map[string]bool{}
	bases := []*types.Named{}

	for i := range it.NumEmbeddeds() {
		embedded, ok := types.Unalias(it.EmbeddedType(i)).(*types.Named)

		if !ok || !types.IsInterface(embedded) || embedded.Obj().Pkg() == nil || embedded.Obj().Pkg().Path() != v.pkg.Path() {
			continue
		}

		bases = append(bases, embedded)

		embeddedIface := embedded.Underlying().(*types.Interface)

		for j := range embeddedIface.NumMethods() {
			inherited[embeddedIface.Method(j).Name()] = true
		}
	}

	own := []*types.Func{}

	for i := range it.NumMethods() {
		if !inherited[it.Method(i).Name()] {
			own = append(own, it.Method(i))
		}
	}

	return own, bases
}

// shapeWitnessSignature renders a method's witness form: its result type, and the parameter list and forwarded
// argument list after `self`.
func (v *Visitor) shapeWitnessSignature(method *types.Func) (string, string, string) {
	sig := method.Signature()
	params := []string{}
	args := []string{}

	for i := range sig.Params().Len() {
		param := sig.Params().At(i)
		name := param.Name()

		if name == "" || name == "_" || name == "self" {
			name = fmt.Sprintf("p%d", i)
		}

		name = getSanitizedIdentifier(name)

		// A variadic tail is go.variadic<T>, as on the method it forwards to, so a spread passes the caller's slice
		// through whole (Go's f(s...)): a Span tail re-packed it into a fresh array, and the callee's writes were lost.
		// The name is spelled in full: the witness declarations sit inside the package class, where a package type
		// could be named variadic, as one named Span hides System.Span.
		if i == sig.Params().Len()-1 && sig.Variadic() {
			if slice, ok := param.Type().(*types.Slice); ok {
				params = append(params, fmt.Sprintf("params global::go.variadic<%s> %s", v.shapeWitnessDeclType(slice.Elem()), name))
				args = append(args, name)
				continue
			}
		}

		params = append(params, fmt.Sprintf("%s %s", v.shapeWitnessDeclType(param.Type()), name))
		args = append(args, name)
	}

	return v.shapeWitnessResultType(sig), strings.Join(params, ", "), strings.Join(args, ", ")
}

// shapeWitnessDeclType is a type as package_info.cs spells it (finding 4): the witness declarations live there, where
// only `using go;` and the package's own `using static` are in scope, so a foreign type is named by its package class
// (`io_package.Reader`), never by a converted file's import alias. The same renderer the file's records already use.
func (v *Visitor) shapeWitnessDeclType(t types.Type) string {
	return convertToCSTypeName(v.getFullyQualifiedTypeName(t, false))
}

// shapeWitnessResultType is a method's C# result type in package_info.cs's spelling: void, one type, or a tuple.
func (v *Visitor) shapeWitnessResultType(sig *types.Signature) string {
	results := sig.Results()

	switch results.Len() {
	case 0:
		return "void"
	case 1:
		return v.shapeWitnessDeclType(results.At(0).Type())
	}

	parts := make([]string, results.Len())

	for i := range parts {
		parts[i] = v.shapeWitnessDeclType(results.At(i).Type())
	}

	return "(" + strings.Join(parts, ", ") + ")"
}

// shapeWitnessInterfaceRefDecl is shapeWitnessInterfaceRef in package_info.cs's spelling, for the declarations there.
func (v *Visitor) shapeWitnessInterfaceRefDecl(iface *types.Named, self string) string {
	args := []string{self}

	if iface.TypeArgs() != nil {
		for i := range iface.TypeArgs().Len() {
			args = append(args, v.shapeWitnessDeclType(iface.TypeArgs().At(i)))
		}
	}

	return fmt.Sprintf("%s%s<%s>", v.shapeWitnessBaseName(iface), ShapeWitnessSuffix, strings.Join(args, ", "))
}

// shapeWitnessInterfaceDecl is the witness interface and the generic witness of interface I, one line each:
//
//	public interface ScriptΔshape<T> { static abstract @string Text(T self); }
//	public readonly struct ScriptΔshapeOf<T> : ScriptΔshape<T> where T : Script { public static @string Text(T self) => self.Text(); }
func (v *Visitor) shapeWitnessInterfaceDecl(named *types.Named) (string, string) {
	access := shapeWitnessAccess(named.Obj())
	base := v.shapeWitnessBaseName(named)
	selfName := "T"
	typeParams := []string{}

	if named.TypeParams() != nil {
		for i := range named.TypeParams().Len() {
			typeParams = append(typeParams, getSanitizedIdentifier(named.TypeParams().At(i).Obj().Name()))
		}
	}

	for _, name := range typeParams {
		if name == selfName {
			selfName = "TΔself"
		}
	}

	allParams := append([]string{selfName}, typeParams...)
	paramList := strings.Join(allParams, ", ")
	constraintRef := base

	if len(typeParams) > 0 {
		constraintRef = fmt.Sprintf("%s<%s>", base, strings.Join(typeParams, ", "))
	}

	own, bases := v.shapeWitnessMethods(named)
	inherits := ""

	if len(bases) > 0 {
		refs := make([]string, len(bases))

		for i, embedded := range bases {
			refs[i] = v.shapeWitnessInterfaceRefDecl(embedded, selfName)
			v.recordShapeWitnessInterface(embedded)
		}

		inherits = " : " + strings.Join(refs, ", ")
	}

	abstracts := []string{}

	for _, method := range own {
		resultType, params, _ := v.shapeWitnessSignature(method)
		abstracts = append(abstracts, fmt.Sprintf("static abstract %s %s(%s);", resultType, getSanitizedFunctionName(method.Name()), shapeWitnessParams(selfName, params)))
	}

	forwarders := []string{}
	it := named.Underlying().(*types.Interface)

	for i := range it.NumMethods() {
		method := it.Method(i)
		resultType, params, args := v.shapeWitnessSignature(method)
		forwarders = append(forwarders, fmt.Sprintf("public static %s %s(%s) => self.%s(%s);", resultType, getSanitizedFunctionName(method.Name()), shapeWitnessParams(selfName, params),
			getSanitizedFunctionName(method.Name()), args))
	}

	// The generic witness names I itself in its clause, so it carries I's own type-parameter clauses too (`wrap[T Value]`
	// needs `where T : Value` beside `where TΔself : wrap<T>`). The witness interface names nothing that needs them, so it
	// stays unconstrained and a selected declaration can name it with whatever clauses its own parameters keep.
	interfaceClauses := ""

	if len(typeParams) > 0 {
		interfaceClauses = v.shapeWitnessInterfaceClauses(named)
	}

	witnessInterface := fmt.Sprintf("%s interface %s%s<%s>%s { %s }", access, base, ShapeWitnessSuffix, paramList, inherits, strings.Join(abstracts, " "))
	genericWitness := fmt.Sprintf("%s readonly struct %s%sOf<%s> : %s%s<%s> where %s : %s%s { %s }", access, base, ShapeWitnessSuffix, paramList,
		base, ShapeWitnessSuffix, paramList, selfName, constraintRef, interfaceClauses, strings.Join(forwarders, " "))

	return witnessInterface, genericWitness
}

// recordShapeWitnessInterface records a selected constraint's witness interface and generic witness for package_info.cs
// (outside any converted source, so go2cs-gen never reads them as Go types), and, the first time, makes the nominal
// record declaration-driven for it: every named type of the package whose value method set implements the interface
// gets GoImplement<E, I>, cast or no cast, so IΔshapeOf<E> binds (DESIGN 8.1).
func (v *Visitor) recordShapeWitnessInterface(named *types.Named) {
	origin := named.Origin()
	key := v.shapeWitnessBaseName(origin) + ShapeWitnessSuffix

	if _, recorded := shapeWitnessPairs[key]; recorded {
		return
	}

	witnessInterface, genericWitness := v.shapeWitnessInterfaceDecl(origin)
	shapeWitnessPairs[key] = witnessInterface
	shapeWitnessPairs[key+"Of"] = genericWitness

	v.recordShapeWitnessImplementations(origin)
}

func shapeWitnessParams(selfType string, params string) string {
	if params == "" {
		return selfType + " self"
	}

	return selfType + " self, " + params
}

// shapeWitnessPairDecl is one per-pair witness, one line in package_info.cs's <ShapeWitnesses> section (the line is its
// identity, so a merging write unions the sections exactly as it does every other):
//
//	public readonly struct SourceFileжScriptΔshape : ScriptΔshape<ж<SourceFile>> { public static @string Text(ж<SourceFile> self) => self.Text(); }
func (v *Visitor) shapeWitnessPairDecl(pair shapeWitnessPair) string {
	access := "internal"

	if shapeWitnessAccess(pair.elem.(*types.Named).Obj()) == "public" && shapeWitnessAccess(pair.iface.Origin().Obj()) == "public" {
		access = "public"
	}

	self := v.shapeWitnessDeclType(pair.elem)

	if pair.pointer {
		self = fmt.Sprintf("%s<%s>", PointerPrefix, self)
	}

	members := []string{}
	it := pair.iface.Underlying().(*types.Interface)

	for i := range it.NumMethods() {
		method := it.Method(i)
		resultType, params, args := v.shapeWitnessSignature(method)
		members = append(members, fmt.Sprintf("public static %s %s(%s) => self.%s(%s);", resultType, getSanitizedFunctionName(method.Name()), shapeWitnessParams(self, params),
			getSanitizedFunctionName(method.Name()), args))
	}

	return fmt.Sprintf("%s readonly struct %s : %s { %s }", access, pair.name, v.shapeWitnessInterfaceRefDecl(pair.iface, self), strings.Join(members, " "))
}

// applyShapeWitnessSection writes the package's per-pair witnesses into package_info.cs, inside the package class after
// the <ImportInitializers> section. A package with none gets no section at all, so no other package's file moves.
func applyShapeWitnessSection(packageInfoLines []string, mergeExisting bool) []string {
	startLineIndex, endLineIndex := -1, -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<"+ShapeWitnessSection+">") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</"+ShapeWitnessSection+">") {
			endLineIndex = i
			break
		}
	}

	lines := map[string]bool{}

	if mergeExisting && startLineIndex >= 0 && endLineIndex > startLineIndex {
		for i := startLineIndex + 1; i < endLineIndex; i++ {
			if line := strings.TrimSpace(packageInfoLines[i]); line != "" {
				lines[line] = true
			}
		}
	}

	for _, line := range shapeWitnessPairs {
		lines[line] = true
	}

	if len(lines) == 0 {
		return packageInfoLines
	}

	sorted := make([]string, 0, len(lines))

	for line := range lines {
		sorted = append(sorted, typeAccessibilityIndent+line)
	}

	sort.Strings(sorted)

	if startLineIndex >= 0 && endLineIndex > startLineIndex {
		return append(packageInfoLines[:startLineIndex+1], append(sorted, packageInfoLines[endLineIndex:]...)...)
	}

	insertIndex := -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "</"+ImportInitSection+">") {
			insertIndex = i + 1
			break
		}
	}

	if insertIndex < 0 {
		return packageInfoLines
	}

	block := []string{
		"",
		typeAccessibilityIndent + "// A generic declaration under a non-self method-set constraint can take a SHAPE WITNESS: its type",
		typeAccessibilityIndent + "// parameter stays the Go type itself, and a method call through it goes through a witness struct.",
		typeAccessibilityIndent + "// Each constraint's witness interface, its generic witness for a type that implements it in C#, and",
		typeAccessibilityIndent + "// one witness per (pointer, constraint) pair are declared once here, so every use names the same type.",
		"",
		typeAccessibilityIndent + "// <" + ShapeWitnessSection + ">",
	}

	block = append(block, sorted...)
	block = append(block, typeAccessibilityIndent+"// </"+ShapeWitnessSection+">")

	return append(packageInfoLines[:insertIndex], append(block, packageInfoLines[insertIndex:]...)...)
}

// recordShapeWitnessImplementations makes the nominal record DECLARATION-driven for a selected interface: every named
// type of the package whose VALUE method set implements it gets GoImplement<E, I> (an open record for a generic E), cast
// or no cast, so the generic witness IΔshapeOf<E> binds (DESIGN 8.1). A generic constraint's record is closed over its
// instantiation, which the use site makes, so it records nothing here.
func (v *Visitor) recordShapeWitnessImplementations(iface *types.Named) {
	if v.pkg == nil || iface.TypeParams() != nil {
		return
	}

	ifaceType := iface.Underlying().(*types.Interface)
	scope := v.pkg.Scope()

	for _, name := range scope.Names() {
		typeName, ok := scope.Lookup(name).(*types.TypeName)

		if !ok || typeName.IsAlias() {
			continue
		}

		named, ok := typeName.Type().(*types.Named)

		if !ok || types.IsInterface(named) || !types.Implements(named, ifaceType) {
			continue
		}

		if named.TypeParams() == nil {
			v.convertToInterfaceType(iface, named, "")
			continue
		}

		// An OPEN generic is recorded by its definition, `GoImplement(typeof(Array<,>), typeof(Value))`: the record path
		// takes it as the type instantiated at its own type parameters, rendered with its own witness parameters.
		args := make([]types.Type, named.TypeParams().Len())

		for i := range args {
			args[i] = named.TypeParams().At(i)
		}

		instance, err := types.Instantiate(nil, named, args, false)

		if err != nil {
			continue
		}

		savedOwner := v.shapeWitnessTypeOwner
		v.shapeWitnessTypeOwner = named
		v.convertToInterfaceType(iface, instance, "")
		v.shapeWitnessTypeOwner = savedOwner
	}
}

// ---- the stage-1 refusal ----

// shapeWitnessRefusal is the error refuseShapeWitnessSites returns: one canonical diagnostic per refused site.
type shapeWitnessRefusal struct {
	pkgPath string
	sites   []constraintProxyRefusalSite
}

func (refusal *shapeWitnessRefusal) Error() string {
	return fmt.Sprintf("package %s uses a shape witness this stage cannot place (%d site(s)); see the %s diagnostics",
		refusal.pkgPath, len(refusal.sites), diagnosticShapeWitnessRefused)
}

func (refusal *shapeWitnessRefusal) report() {
	for _, site := range refusal.sites {
		reportDiagnostic(site.position.String(), diagnosticShapeWitnessRefused, site.text)
	}
}

// refuseShapeWitnessSites refuses, before any file is written, every site stage 1 cannot place: an instantiation of
// another package's selected declaration, a pair needing another package's witness, a method value through a witnessed
// type parameter, or a witnessed value converted to a non-empty interface.
func refuseShapeWitnessSites(pkg *packages.Package, selection map[types.Object]*shapeWitnessDecl) *shapeWitnessRefusal {
	if pkg.TypesInfo == nil {
		return nil
	}

	v := newGenericRouteVisitor(pkg)
	v.pkg = pkg.Types
	refusal := &shapeWitnessRefusal{pkgPath: pkg.PkgPath}
	seen := map[string]bool{}

	refuse := func(pos token.Pos, format string, args ...any) {
		position := pkg.Fset.Position(pos)
		text := fmt.Sprintf(format, args...)

		if key := position.String() + "|" + text; !seen[key] {
			seen[key] = true
			refusal.sites = append(refusal.sites, constraintProxyRefusalSite{position: position, text: text})
		}
	}

	// inspect walks one node with the witness scope its syntax sees: the instantiation checks, then (for a function or
	// method body) the body pass.
	inspect := func(node ast.Node, scope shapeWitnessScope) {
		ast.Inspect(node, func(n ast.Node) bool {
			ident, ok := n.(*ast.Ident)

			if !ok {
				return true
			}

			instance, ok := pkg.TypesInfo.Instances[ident]

			if !ok || instance.TypeArgs == nil {
				return true
			}

			origin := shapeWitnessOrigin(pkg.TypesInfo, ident)

			if origin == nil {
				return true
			}

			// ONWARD: a selected T (the box, with no C# clause) handed to a generic that keeps a nominal clause at
			// that position -- one the closure cannot select (another package's, or an inline or foreign
			// constraint) -- would meet a constraint the box cannot satisfy.
			if len(scope) > 0 {
				for i := range instance.TypeArgs.Len() {
					argTypeParam, isTypeParam := types.Unalias(instance.TypeArgs.At(i)).(*types.TypeParam)

					if !isTypeParam {
						continue
					}

					if _, inScope := scope[argTypeParam]; !inScope {
						continue
					}

					if constraint, kept := v.shapeWitnessKeptClause(origin, i, selection); kept {
						refuse(ident.Pos(), "%s hands the witnessed type parameter %s to %s's %s, which keeps its C# constraint; stage 1 of the narrow shape witness cannot pass a witness there", types.ExprString(ident), argTypeParam.Obj().Name(), ident.Name, constraint)
					}
				}
			}

			if origin.Pkg() != nil && origin.Pkg().Path() != pkg.Types.Path() {
				if shapeWitnessForeignSelected(origin) != nil {
					refuse(ident.Pos(), "%s is a shape-witness declaration of package %s; instantiating it from another package needs stage 2 of the narrow shape witness", ident.Name, origin.Pkg().Path())
				}

				return true
			}

			target := selection[origin]

			if target == nil || !target.carrier {
				return true
			}

			for _, i := range target.indices() {
				if i >= instance.TypeArgs.Len() {
					continue
				}

				tp := target.tps[i]

				if _, reason := v.shapeWitnessArg(instance.TypeArgs.At(i), shapeWitnessInstantiatedConstraint(tp, instance.TypeArgs), scope, false, false); reason != "" {
					refuse(ident.Pos(), "%s[%s %s] instantiated with %s: %s", ident.Name, tp.Obj().Name(), tp.Constraint(), instance.TypeArgs.At(i), reason)
				}
			}

			return true
		})

		if funcDecl, ok := node.(*ast.FuncDecl); ok && funcDecl.Body != nil && len(scope) > 0 {
			var results *types.Tuple

			if fn, ok := pkg.TypesInfo.Defs[funcDecl.Name].(*types.Func); ok {
				results = fn.Signature().Results()
			}

			shapeWitnessBodyRefusals(pkg, funcDecl.Body, results, scope, refuse)
		}
	}

	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			switch decl := decl.(type) {
			case *ast.FuncDecl:
				inspect(decl, shapeWitnessScopeOf(shapeWitnessClosureScopes(pkg, decl), selection))
			case *ast.GenDecl:
				// One scope per spec (finding 2): a generic type spec sees its own type parameters; any other spec none.
				for _, spec := range decl.Specs {
					scope := shapeWitnessScope{}

					if typeSpec, ok := spec.(*ast.TypeSpec); ok && typeSpec.TypeParams != nil {
						single := &ast.GenDecl{Tok: decl.Tok, Specs: []ast.Spec{typeSpec}}
						scope = shapeWitnessScopeOf(shapeWitnessClosureScopes(pkg, single), selection)
					}

					inspect(spec, scope)
				}
			}
		}
	}

	refuseForeignCarrierFlows(pkg, refuse)

	if len(refusal.sites) == 0 {
		return nil
	}

	sort.Slice(refusal.sites, func(a, b int) bool {
		if refusal.sites[a].position.Filename != refusal.sites[b].position.Filename {
			return refusal.sites[a].position.Filename < refusal.sites[b].position.Filename
		}

		if refusal.sites[a].position.Line != refusal.sites[b].position.Line {
			return refusal.sites[a].position.Line < refusal.sites[b].position.Line
		}

		return refusal.sites[a].text < refusal.sites[b].text
	})

	return refusal
}

// shapeWitnessScopeOf is the witness scope a declaration's syntax sees, for the refusal pass: EVERY selected position of
// its scope's owner, carrier or not (finding 1), since a selected non-carrier drops T's clause as a carrier does. Only
// membership matters here, not the witness names.
func shapeWitnessScopeOf(scopes []shapeWitnessScopeSpec, selection map[types.Object]*shapeWitnessDecl) shapeWitnessScope {
	scope := shapeWitnessScope{}

	for _, sc := range scopes {
		target := selection[sc.owner]

		if target == nil {
			continue
		}

		for i := range target.tps {
			if i < len(sc.own) {
				scope[sc.own[i]] = sc.own[i].Obj().Name()
			}
		}
	}

	return scope
}

// refuseForeignCarrierFlows refuses a value of another package's shape-witness CARRIER that reaches this package with no
// instantiating identifier (`v := a.Default()`, then `pair(v, v)`; finding 7): its C# name needs the foreign witness,
// which stage 1 cannot name. One site per carrier type, at its first position in the package.
func refuseForeignCarrierFlows(pkg *packages.Package, refuse func(token.Pos, string, ...any)) {
	first := map[string]token.Pos{}
	names := map[string]string{}

	var visit func(t types.Type, pos token.Pos, seen map[types.Type]bool)

	visit = func(t types.Type, pos token.Pos, seen map[types.Type]bool) {
		if t == nil || seen[t] {
			return
		}

		seen[t] = true

		switch x := types.Unalias(t).(type) {
		case *types.Named:
			// Another package by PATH: an external test package (p_test) builds in another assembly than p, so p's
			// declarations are foreign to it as they are to any importer.
			if obj := x.Origin().Obj(); obj.Pkg() != nil && x.TypeArgs() != nil && obj.Pkg().Path() != pkg.Types.Path() {
				if decl := shapeWitnessForeignSelected(obj); decl != nil && decl.carrier {
					key := obj.Pkg().Path() + "." + obj.Name()

					if old, ok := first[key]; !ok || pos < old {
						first[key] = pos
						names[key] = x.String()
					}
				}
			}

			if x.TypeArgs() != nil {
				for i := range x.TypeArgs().Len() {
					visit(x.TypeArgs().At(i), pos, seen)
				}
			}
		case *types.Pointer:
			visit(x.Elem(), pos, seen)
		case *types.Slice:
			visit(x.Elem(), pos, seen)
		case *types.Array:
			visit(x.Elem(), pos, seen)
		case *types.Map:
			visit(x.Key(), pos, seen)
			visit(x.Elem(), pos, seen)
		case *types.Chan:
			visit(x.Elem(), pos, seen)
		case *types.Signature:
			for i := range x.Params().Len() {
				visit(x.Params().At(i).Type(), pos, seen)
			}

			for i := range x.Results().Len() {
				visit(x.Results().At(i).Type(), pos, seen)
			}
		case *types.Struct:
			for i := range x.NumFields() {
				visit(x.Field(i).Type(), pos, seen)
			}
		}
	}

	// Every variable, field, parameter or result this package DEFINES with a carrier type is a site of its own; an
	// expression (`pair(a.Default(), x)`) is the fallback for a carrier no definition carries, at its first position.
	defined := map[string]bool{}

	for ident, obj := range pkg.TypesInfo.Defs {
		if obj == nil {
			continue
		}

		first = map[string]token.Pos{}
		visit(obj.Type(), ident.Pos(), map[types.Type]bool{})

		for key := range first {
			defined[key] = true
			refuse(ident.Pos(), "a value of %s, a shape-witness carrier of another package, reaches this package; its C# name needs that package's witness, which stage 2 of the narrow shape witness places", names[key])
		}
	}

	first = map[string]token.Pos{}

	for expr, tv := range pkg.TypesInfo.Types {
		visit(tv.Type, expr.Pos(), map[types.Type]bool{})
	}

	for key, pos := range first {
		if !defined[key] {
			refuse(pos, "a value of %s, a shape-witness carrier of another package, reaches this package; its C# name needs that package's witness, which stage 2 of the narrow shape witness places", names[key])
		}
	}
}

// shapeWitnessInstantiatedConstraint is a type parameter's constraint with the instance's type arguments substituted, so
// `wrap[T]` at Deref[Ptr[u8], Ptr[u8]] is wrap[Ptr[u8]].
func shapeWitnessInstantiatedConstraint(tp *types.TypeParam, typeArgs *types.TypeList) types.Type {
	constraint := tp.Constraint()
	named, ok := types.Unalias(constraint).(*types.Named)

	if !ok || named.TypeArgs() == nil || named.TypeArgs().Len() == 0 {
		return constraint
	}

	args := make([]types.Type, named.TypeArgs().Len())

	for i := range args {
		args[i] = named.TypeArgs().At(i)

		if inner, ok := types.Unalias(args[i]).(*types.TypeParam); ok && inner.Index() < typeArgs.Len() {
			args[i] = typeArgs.At(inner.Index())
		}
	}

	if instantiated, err := types.Instantiate(nil, named.Origin(), args, false); err == nil {
		return instantiated
	}

	return constraint
}

// shapeWitnessBodyRefusals refuses, inside a witnessed body, the two shapes stage 1 does not emit: a method VALUE
// through a witnessed type parameter (`f := x.M`), and a value of one converted to a non-empty interface (T is
// unconstrained in C#, so the conversion has no nominal implementation to bind). It descends into function literals,
// each with its OWN results (finding 5), and reads every conversion position (finding 8): arguments, assignments,
// declarations, returns, explicit conversions, composite-literal elements and channel sends. A comparison `x == iface`
// cannot occur: a type parameter under a pure method-set constraint is not comparable, so Go rejects it.
func shapeWitnessBodyRefusals(pkg *packages.Package, body *ast.BlockStmt, results *types.Tuple, scope shapeWitnessScope, refuse func(token.Pos, string, ...any)) {
	info := pkg.TypesInfo
	called := map[*ast.SelectorExpr]bool{}

	witnessed := func(e ast.Expr) (*types.TypeParam, bool) {
		tv, ok := info.Types[e]

		if !ok {
			return nil, false
		}

		tp, ok := types.Unalias(tv.Type).(*types.TypeParam)

		if !ok {
			return nil, false
		}

		_, in := scope[tp]

		return tp, in
	}

	toInterface := func(e ast.Expr, target types.Type) {
		if target == nil || e == nil {
			return
		}

		iface, ok := target.Underlying().(*types.Interface)

		if !ok || iface.NumMethods() == 0 {
			return
		}

		if _, isTypeParam := types.Unalias(target).(*types.TypeParam); isTypeParam {
			return
		}

		if tp, ok := witnessed(e); ok {
			refuse(e.Pos(), "the value %s of the witnessed type parameter %s converts to the interface %s, which stage 1 of the narrow shape witness does not emit", types.ExprString(e), tp.Obj().Name(), target)
		}
	}

	ast.Inspect(body, func(n ast.Node) bool {
		if call, ok := n.(*ast.CallExpr); ok {
			if sel, ok := ast.Unparen(call.Fun).(*ast.SelectorExpr); ok {
				called[sel] = true
			}
		}

		return true
	})

	var walk func(node ast.Node, results *types.Tuple)

	walk = func(node ast.Node, results *types.Tuple) {
		ast.Inspect(node, func(n ast.Node) bool {
			switch x := n.(type) {
			case *ast.FuncLit:
				// A literal's returns are its own: walk its body with its own results, then skip it here.
				var litResults *types.Tuple

				if sig, ok := info.TypeOf(x).(*types.Signature); ok {
					litResults = sig.Results()
				}

				walk(x.Body, litResults)

				return false
			case *ast.SelectorExpr:
				if called[x] {
					return true
				}

				if tp, ok := witnessed(x.X); ok {
					if selection, ok := info.Selections[x]; ok && selection.Kind() == types.MethodVal {
						refuse(x.Pos(), "the method value %s through the witnessed type parameter %s needs a witness delegate, which stage 1 of the narrow shape witness does not emit", types.ExprString(x), tp.Obj().Name())
					}
				}
			case *ast.CallExpr:
				if tv, ok := info.Types[x.Fun]; ok && tv.IsType() && len(x.Args) == 1 {
					toInterface(x.Args[0], tv.Type)
					return true
				}

				if sig, ok := info.TypeOf(x.Fun).(*types.Signature); ok {
					for i, arg := range x.Args {
						if i < sig.Params().Len()-1 || (!sig.Variadic() && i < sig.Params().Len()) {
							toInterface(arg, sig.Params().At(i).Type())
						} else if sig.Variadic() && sig.Params().Len() > 0 && x.Ellipsis == token.NoPos {
							if slice, ok := sig.Params().At(sig.Params().Len() - 1).Type().(*types.Slice); ok {
								toInterface(arg, slice.Elem())
							}
						}
					}
				}
			case *ast.AssignStmt:
				if len(x.Lhs) == len(x.Rhs) && x.Tok == token.ASSIGN {
					for i := range x.Lhs {
						toInterface(x.Rhs[i], info.TypeOf(x.Lhs[i]))
					}
				}
			case *ast.ValueSpec:
				if x.Type != nil && len(x.Values) == len(x.Names) {
					for i := range x.Values {
						toInterface(x.Values[i], info.TypeOf(x.Type))
					}
				}
			case *ast.ReturnStmt:
				if results != nil && len(x.Results) == results.Len() {
					for i := range x.Results {
						toInterface(x.Results[i], results.At(i).Type())
					}
				}
			case *ast.CompositeLit:
				shapeWitnessCompositeElems(info, x, toInterface)
			case *ast.SendStmt:
				if ch, ok := info.TypeOf(x.Chan).Underlying().(*types.Chan); ok {
					toInterface(x.Value, ch.Elem())
				}
			}

			return true
		})
	}

	walk(body, results)
}

// shapeWitnessCompositeElems reads a composite literal's element positions for the to-interface check: struct fields (by
// key or by position), slice and array elements, map keys and values.
func shapeWitnessCompositeElems(info *types.Info, lit *ast.CompositeLit, check func(ast.Expr, types.Type)) {
	t := info.TypeOf(lit)

	if t == nil {
		return
	}

	switch u := t.Underlying().(type) {
	case *types.Struct:
		for i, elt := range lit.Elts {
			if kv, ok := elt.(*ast.KeyValueExpr); ok {
				if key, ok := kv.Key.(*ast.Ident); ok {
					for j := range u.NumFields() {
						if u.Field(j).Name() == key.Name {
							check(kv.Value, u.Field(j).Type())
						}
					}
				}
			} else if i < u.NumFields() {
				check(elt, u.Field(i).Type())
			}
		}
	case *types.Slice:
		for _, elt := range lit.Elts {
			if kv, ok := elt.(*ast.KeyValueExpr); ok {
				elt = kv.Value
			}

			check(elt, u.Elem())
		}
	case *types.Array:
		for _, elt := range lit.Elts {
			if kv, ok := elt.(*ast.KeyValueExpr); ok {
				elt = kv.Value
			}

			check(elt, u.Elem())
		}
	case *types.Map:
		for _, elt := range lit.Elts {
			if kv, ok := elt.(*ast.KeyValueExpr); ok {
				check(kv.Key, u.Key())
				check(kv.Value, u.Elem())
			}
		}
	}
}

// shapeWitnessed reports whether type-parameter position i of the declaration object names takes the witness route,
// so the proxy routes (constraintProxyArg, constraintProxySigArg, funcResultProjection, sliceWideningRoute and the
// GO2CS2005 pass) decline it.
func shapeWitnessed(object types.Object, i int) bool {
	decl := shapeWitnessDeclFor(object)

	return decl != nil && decl.tps[i] != nil
}

// prepareShapeWitness computes the package's selection into package state and refuses what stage 1 cannot place. It
// runs before the proxy refusals, which then decline every witnessed position.
func prepareShapeWitness(pkg *packages.Package) *shapeWitnessRefusal {
	// Refused even when nothing here is selected: an instantiation of ANOTHER package's witness declaration is stage 2.
	installShapeWitnessSelection(pkg)

	refusal := refuseShapeWitnessSites(pkg, shapeWitnessDecls)

	if divergence := refuseShapeWitnessTestDivergence(pkg, shapeWitnessDecls); divergence != nil {
		if refusal == nil {
			return divergence
		}

		refusal.sites = append(refusal.sites, divergence.sites...)
	}

	return refusal
}

// installShapeWitnessSelection resets the package's shape-witness state and computes its selection into it. It reports
// whether there is anything selected.
func installShapeWitnessSelection(pkg *packages.Package) bool {
	resetShapeWitnessState()

	if pkg.TypesInfo == nil || pkg.Types == nil {
		return false
	}

	shapeWitnessDecls = computeShapeWitnessSelection(pkg)

	return len(shapeWitnessDecls) > 0
}

// shapeWitnessCallParts is the witness form of a method call through a witnessed type parameter: the static member to
// call (`TΔScript.Text`) and the converted receiver to pass first.
func (v *Visitor) shapeWitnessCallParts(callExpr *ast.CallExpr) (string, string, bool) {
	if len(shapeWitnessDecls) == 0 {
		return "", "", false
	}

	sel, ok := ast.Unparen(callExpr.Fun).(*ast.SelectorExpr)

	if !ok {
		return "", "", false
	}

	if selection, ok := v.info.Selections[sel]; !ok || selection.Kind() != types.MethodVal {
		return "", "", false
	}

	witness := v.shapeWitnessReceiverCall(sel.X)

	if witness == "" {
		return "", "", false
	}

	return witness + "." + getSanitizedFunctionName(sel.Sel.Name), v.convExpr(sel.X, nil), true
}

// shapeWitnessArity is the number of witness parameters a declaration adds to its C# arity: one per selected position
// of a carrier, none otherwise.
func shapeWitnessArity(object types.Object) int {
	if decl := shapeWitnessDeclFor(object); decl != nil && decl.carrier {
		return len(decl.tps)
	}

	return 0
}

// shapeWitnessOwnsTypeParam reports whether tp is one of a selected declaration's WITNESSED type parameters, as its
// syntax spells it (own is the list that syntax declares: a method's receiver type parameters, for one).
func shapeWitnessOwnsTypeParam(decl *shapeWitnessDecl, own []*types.TypeParam, tp *types.TypeParam) bool {
	for j := range decl.tps {
		if j < len(own) && own[j] == tp {
			return true
		}
	}

	return false
}

// shapeWitnessKeptClause reports whether position i of the generic origin names keeps a NOMINAL C# clause -- a method-set
// constraint the emission writes as `where U : I` -- and is not witnessed, with the constraint for the diagnostic. A
// function's clause is kept only where its body needs it (methodSetConstraintNeeded); a type's always is.
func (v *Visitor) shapeWitnessKeptClause(origin types.Object, i int, selection map[types.Object]*shapeWitnessDecl) (types.Type, bool) {
	if decl := selection[origin]; decl != nil && decl.tps[i] != nil {
		return nil, false
	}

	var typeParams *types.TypeParamList
	fn, isFunc := origin.(*types.Func)

	if isFunc {
		typeParams = fn.Type().(*types.Signature).TypeParams()
	} else if typeName, ok := origin.(*types.TypeName); ok {
		if named, ok := typeName.Type().(*types.Named); ok {
			typeParams = named.TypeParams()
		}
	}

	if typeParams == nil || i >= typeParams.Len() {
		return nil, false
	}

	constraint := typeParams.At(i).Constraint()
	iface, ok := constraint.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 {
		return nil, false
	}

	if isFunc && !v.methodSetConstraintNeeded(fn, i) {
		return nil, false
	}

	return constraint, true
}

// shapeWitnessAccess is the C# accessibility a witness declaration over a type takes: the type's own as EMITTED, so an
// unexported constraint an exported declaration names (gen's `wrap`, publicized beside `Deref`) gives a public witness
// that declaration can name, and a witness never names a type less accessible than itself.
func shapeWitnessAccess(obj types.Object) string {
	if obj.Exported() || packagePublicizedTypes[obj] {
		return "public"
	}

	return "internal"
}

// namedRendersResolved reports whether a written instantiation of named must render from the RESOLVED type rather than
// the AST: a constraint-proxy argument (namedHasConstraintProxy), or a shape-witness carrier, whose witness arguments
// appear nowhere in the Go syntax (`Ptr[void]{}` is `new Ptr<@void, ValueΔshapeOf<@void>>()`).
func (v *Visitor) namedRendersResolved(named *types.Named) bool {
	return v.namedHasConstraintProxy(named) || (named != nil && named.TypeArgs() != nil && shapeWitnessArity(named.Origin().Obj()) > 0)
}

// shapeWitnessExplicitTypeArgs is the full rendered type-argument list of an explicit instantiation of a shape-witness
// FUNCTION (`FromLSPRange[*SourceFile]`, written arguments then witnesses), or nil when x names anything else.
func (v *Visitor) shapeWitnessExplicitTypeArgs(x ast.Expr) []string {
	funIdent := getCallFunIdent(x)

	if funIdent == nil {
		return nil
	}

	fn, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok || shapeWitnessArity(fn.Origin()) == 0 {
		return nil
	}

	instance, ok := v.info.Instances[funIdent]

	if !ok || instance.TypeArgs == nil {
		return nil
	}

	return v.renderedTypeArgs(funIdent, instance.TypeArgs)
}

// shapeWitnessInterfaceClauses are the clauses a generic interface's own type parameters need in its generic witness,
// which names the interface itself (`wrap[T Value]` needs ` where T : Value`), in package_info.cs's spelling. Only a
// method-set constraint has a C# clause to carry; any other constraint the interface declaration emits none for here.
func (v *Visitor) shapeWitnessInterfaceClauses(named *types.Named) string {
	if named.TypeParams() == nil {
		return ""
	}

	var clauses []string

	for i := range named.TypeParams().Len() {
		tp := named.TypeParams().At(i)
		constraint, ok := types.Unalias(tp.Constraint()).(*types.Named)

		if !ok {
			continue
		}

		if iface, ok := constraint.Underlying().(*types.Interface); !ok || iface.NumMethods() == 0 || !iface.IsMethodSet() {
			continue
		}

		clauses = append(clauses, fmt.Sprintf(" where %s : %s", getSanitizedIdentifier(tp.Obj().Name()), v.shapeWitnessDeclType(constraint)))
	}

	return strings.Join(clauses, "")
}

// refuseShapeWitnessTestDivergence refuses, in a -tests variant, a production declaration whose selection the package's
// _test.go files change (finding 6): an unexported declaration a test alone instantiates with a pointer is witnessed in
// the variant and not in production, so the test files would name a witness arity the compiled production declaration
// does not have. Master refuses such a row by name (GO2CS2005), so this one does too. A production conversion has no
// test files, and returns nil.
func refuseShapeWitnessTestDivergence(pkg *packages.Package, variant map[types.Object]*shapeWitnessDecl) *shapeWitnessRefusal {
	if pkg.TypesInfo == nil || pkg.Fset == nil {
		return nil
	}

	isTestFile := func(pos token.Pos) bool {
		return strings.HasSuffix(pkg.Fset.Position(pos).Filename, "_test.go")
	}

	hasTests := false

	for _, file := range pkg.Syntax {
		if isTestFile(file.Pos()) {
			hasTests = true
			break
		}
	}

	if !hasTests {
		return nil
	}

	production := computeShapeWitnessSelectionFiles(pkg, func(file *ast.File) bool { return !isTestFile(file.Pos()) })

	describe := func(decl *shapeWitnessDecl) string {
		if decl == nil {
			return "not witnessed"
		}

		names := make([]string, 0, len(decl.tps))

		for _, i := range decl.indices() {
			names = append(names, decl.tps[i].Obj().Name())
		}

		return fmt.Sprintf("witnessed at %s, carrier=%v", strings.Join(names, ", "), decl.carrier)
	}

	objects := map[types.Object]bool{}

	for object := range variant {
		objects[object] = true
	}

	for object := range production {
		objects[object] = true
	}

	refusal := &shapeWitnessRefusal{pkgPath: pkg.PkgPath}

	for object := range objects {
		if isTestFile(object.Pos()) {
			continue
		}

		if p, t := describe(production[object]), describe(variant[object]); p != t {
			refusal.sites = append(refusal.sites, constraintProxyRefusalSite{
				position: pkg.Fset.Position(object.Pos()),
				text: fmt.Sprintf("the package's _test.go files change %s's shape-witness selection (production: %s; with the tests: %s); stage 1 of the narrow shape witness cannot re-emit a production declaration for its tests",
					object.Name(), p, t),
			})
		}
	}

	if len(refusal.sites) == 0 {
		return nil
	}

	sort.Slice(refusal.sites, func(a, b int) bool {
		if refusal.sites[a].position.Filename != refusal.sites[b].position.Filename {
			return refusal.sites[a].position.Filename < refusal.sites[b].position.Filename
		}

		return refusal.sites[a].position.Line < refusal.sites[b].position.Line
	})

	return refusal
}
