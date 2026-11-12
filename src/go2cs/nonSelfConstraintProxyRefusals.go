// nonSelfConstraintProxyRefusals.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The refusals BY NAME of the non-self constraint proxy (nonSelfConstraintProxy.go), checked before any file is
// written, as the self-referential route's are (constraintProxyForwarding.go). Each names a pointer type argument
// against a non-self-referential method-set constraint that the proxy cannot carry with Go's semantics, so go2cs
// stops with a diagnostic instead of emitting a CS0311 or, worse, C# that compiles and behaves differently
// (docs/phase4/DESIGN-constraint-proxy-nonself.md, sections 5 and 9):
//
//   - a generic NAMED type (ruling 1): the proxy would be part of the type's identity, and a field such as []O aliases;
//   - a union-plus-methods constraint (`interface{ ~*T; M() }`, ruling 3a): neither erased nor proxiable;
//   - a callee whose signature carries the type parameter in an ALIASING position ([]O, map, chan, *O) beside its
//     direct position (section 5): converting the caller's []*E would be a copy;
//   - a SIBLING type parameter whose constraint mentions the proxied one (`S ~[]O`, ruling 3b);
//   - inside the callee, the zero value of the type parameter used as a method receiver or converted to an interface
//     (ruling 2): Go's zero O is a typed nil pointer, the proxy's is a null reference;
//   - inside the callee, the type parameter passed on to another generic under a DIFFERENT constraint (ruling 3c).

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"sort"

	"golang.org/x/tools/go/packages"
)

// nonSelfProxyRefusal is the error refuseNonSelfConstraintProxies returns: one canonical diagnostic per refused site.
type nonSelfProxyRefusal struct {
	pkgPath string
	sites   []constraintProxyRefusalSite
}

func (refusal *nonSelfProxyRefusal) Error() string {
	return fmt.Sprintf("package %s instantiates a non-self-referential method-set constraint over a pointer element where the constraint proxy cannot carry Go's semantics (%d site(s)); see the %s diagnostics",
		refusal.pkgPath, len(refusal.sites), diagnosticNonSelfProxyRefused)
}

// report prints each refused site as a canonical diagnostic.
func (refusal *nonSelfProxyRefusal) report() {
	for _, site := range refusal.sites {
		reportDiagnostic(site.position.String(), diagnosticNonSelfProxyRefused, site.text)
	}
}

// refuseNonSelfConstraintProxies finds every refused non-self proxy site in the package (see the file comment). It
// returns nil when there is none.
func refuseNonSelfConstraintProxies(pkg *packages.Package) *nonSelfProxyRefusal {
	if pkg.TypesInfo == nil {
		return nil
	}

	// The route's own predicate needs methodSetConstraintNeeded, which asks the callee's body: a visitor over this
	// package's declarations answers it exactly as the conversion will. A foreign callee's body is read from the loaded
	// graph, which is THIS package's (collectCaptureModeMethods records the same root moments later), never a previous
	// package's in a multi-package run.
	currentLoadedPackage = pkg

	v := &Visitor{info: pkg.TypesInfo, fileFuncDecls: map[*types.Func]*ast.FuncDecl{}}

	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			if funcDecl, ok := decl.(*ast.FuncDecl); ok {
				if fn, ok := pkg.TypesInfo.Defs[funcDecl.Name].(*types.Func); ok {
					v.fileFuncDecls[fn] = funcDecl
				}
			}
		}
	}

	refusal := &nonSelfProxyRefusal{pkgPath: pkg.PkgPath}
	seen := map[string]bool{}

	refuse := func(position token.Position, format string, args ...any) {
		text := fmt.Sprintf(format, args...)

		if key := position.String() + "|" + text; !seen[key] {
			seen[key] = true
			refusal.sites = append(refusal.sites, constraintProxyRefusalSite{position: position, text: text})
		}
	}

	for ident, instance := range pkg.TypesInfo.Instances {
		object := pkg.TypesInfo.Uses[ident]
		typeParams := genericTypeParams(object, instance.Type)

		if typeParams == nil || instance.TypeArgs == nil {
			continue
		}

		position := pkg.Fset.Position(ident.Pos())

		for i := range min(typeParams.Len(), instance.TypeArgs.Len()) {
			typeParam, typeArg := typeParams.At(i), instance.TypeArgs.At(i)
			subject := fmt.Sprintf("%s[%s %s] instantiated with %s", ident.Name, typeParam.Obj().Name(), typeParam.Constraint(), typeArg)

			if refusesUnionPlusMethods(typeParam, typeArg) {
				refuse(position, "%s: a constraint that mixes a type-term union with methods is neither erased nor carried by a constraint proxy, so a pointer type argument cannot satisfy it (DESIGN-constraint-proxy-nonself ruling 3a)", subject)
				continue
			}

			if _, _, ok := nonSelfConstraintShape(typeParam, typeArg); !ok {
				continue
			}

			fn, isFunc := object.(*types.Func)

			if !isFunc {
				refuse(position, "%s: a pointer type argument to a generic NAMED type under a non-self-referential method-set constraint would put a constraint proxy in the type's identity, and its fields share the element's storage (DESIGN-constraint-proxy-nonself ruling 1)", subject)
				continue
			}

			signature, ok := fn.Origin().Type().(*types.Signature)

			if !ok || signature.Recv() != nil || !typeParamIsDirectPosition(signature, typeParam) {
				continue
			}

			if _, _, projected := v.funcResultProjection(ident, instance.TypeArgs, i); projected {
				continue
			}

			if !v.methodSetConstraintNeeded(fn.Origin(), i) {
				continue
			}

			// The route applies here: refuse what it cannot carry.
			if where := aliasingTypeParamPosition(signature, typeParam); where != "" {
				refuse(position, "%s: %s shares the element's storage in Go, and the constraint proxy is a separate object per pointer, so the call cannot convert it without a copy (DESIGN-constraint-proxy-nonself section 5)", subject, where)
			}

			for j := range signature.TypeParams().Len() {
				if sibling := signature.TypeParams().At(j); j != i && constraintMentionsTypeParam(sibling.Constraint(), typeParam) {
					refuse(position, "%s: the sibling type parameter %s %s mentions %s, and its type argument would carry the pointer where the proxy stands (DESIGN-constraint-proxy-nonself ruling 3b)", subject, sibling.Obj().Name(), sibling.Constraint(), typeParam.Obj().Name())
				}
			}

			if decl, info := v.genericFuncSource(fn.Origin()); decl != nil && decl.Body != nil && info != nil {
				for _, misuse := range zeroValueMisuses(decl, info, typeParam) {
					refuse(position, "%s: inside %s, %s (DESIGN-constraint-proxy-nonself ruling 2: Go's zero value of the type parameter is a typed nil pointer, the proxy's is a null reference)", subject, fn.Name(), misuse)
				}

				for _, onward := range onwardGenericCalls(decl, info, typeParam) {
					refuse(position, "%s: inside %s, %s (DESIGN-constraint-proxy-nonself ruling 3c)", subject, fn.Name(), onward)
				}
			}
		}
	}

	if len(refusal.sites) == 0 {
		return nil
	}

	sort.Slice(refusal.sites, func(a, b int) bool {
		if refusal.sites[a].position.Line != refusal.sites[b].position.Line {
			return refusal.sites[a].position.Line < refusal.sites[b].position.Line
		}

		return refusal.sites[a].text < refusal.sites[b].text
	})

	return refusal
}

// refusesUnionPlusMethods reports a pointer-to-named type argument against a NON-generic constraint that has methods
// beside a type-term union the declaration arm does not emit as a pure method set (`interface{ ~*T; M() }`).
func refusesUnionPlusMethods(typeParam *types.TypeParam, typeArg types.Type) bool {
	ptr, ok := types.Unalias(typeArg).(*types.Pointer)

	if !ok {
		return false
	}

	if _, ok := types.Unalias(ptr.Elem()).(*types.Named); !ok {
		return false
	}

	if named, ok := types.Unalias(typeParam.Constraint()).(*types.Named); ok && named.TypeArgs().Len() > 0 {
		return false
	}

	iface, ok := typeParam.Constraint().Underlying().(*types.Interface)

	return ok && iface.NumMethods() > 0 && !iface.IsMethodSet() && !isMethodSetWithPointerNamedUnion(iface) && !isMethodSetBeyondComparable(iface)
}

// aliasingTypeParamPosition names the first parameter or result of the signature that carries the type parameter
// inside a slice, map, channel or pointer ("takes []O"), or "" when there is none.
func aliasingTypeParamPosition(signature *types.Signature, typeParam *types.TypeParam) string {
	check := func(verb string, t types.Type) string {
		switch x := types.Unalias(t).(type) {
		case *types.Slice:
			if mentionsTypeParamOutsideInterface(x.Elem(), typeParam) {
				return verb + " " + types.TypeString(t, nil)
			}
		case *types.Map:
			if mentionsTypeParamOutsideInterface(x.Key(), typeParam) || mentionsTypeParamOutsideInterface(x.Elem(), typeParam) {
				return verb + " " + types.TypeString(t, nil)
			}
		case *types.Chan:
			if mentionsTypeParamOutsideInterface(x.Elem(), typeParam) {
				return verb + " " + types.TypeString(t, nil)
			}
		case *types.Pointer:
			if mentionsTypeParamOutsideInterface(x.Elem(), typeParam) {
				return verb + " " + types.TypeString(t, nil)
			}
		}

		return ""
	}

	for i := range signature.Params().Len() {
		if where := check("the parameter of type", signature.Params().At(i).Type()); where != "" {
			return where
		}
	}

	for i := range signature.Results().Len() {
		if where := check("the result of type", signature.Results().At(i).Type()); where != "" {
			return where
		}
	}

	return ""
}

// zeroValueMisuses lists, as phrases, each use inside decl of a ZERO value of the type parameter (a `var x O` with no
// value, or a named result of type O) as a method receiver or converted to an interface.
func zeroValueMisuses(decl *ast.FuncDecl, info *types.Info, typeParam *types.TypeParam) []string {
	zeros := map[types.Object]bool{}

	if decl.Type.Results != nil {
		for _, field := range decl.Type.Results.List {
			for _, name := range field.Names {
				if object := info.Defs[name]; object != nil && object.Type() == types.Type(typeParam) {
					zeros[object] = true
				}
			}
		}
	}

	ast.Inspect(decl.Body, func(n ast.Node) bool {
		if spec, ok := n.(*ast.ValueSpec); ok && len(spec.Values) == 0 {
			for _, name := range spec.Names {
				if object := info.Defs[name]; object != nil && object.Type() == types.Type(typeParam) {
					zeros[object] = true
				}
			}
		}

		return true
	})

	if len(zeros) == 0 {
		return nil
	}

	isZero := func(e ast.Expr) (string, bool) {
		if ident, ok := ast.Unparen(e).(*ast.Ident); ok && zeros[info.Uses[ident]] {
			return ident.Name, true
		}

		return "", false
	}

	interfaceTarget := func(t types.Type) bool {
		if t == nil {
			return false
		}

		if _, isTypeParam := types.Unalias(t).(*types.TypeParam); isTypeParam {
			return false
		}

		return types.IsInterface(t)
	}

	var misuses []string

	ast.Inspect(decl.Body, func(n ast.Node) bool {
		switch x := n.(type) {
		case *ast.SelectorExpr:
			if name, ok := isZero(x.X); ok {
				if selection, ok := info.Selections[x]; ok && selection.Kind() == types.MethodVal {
					misuses = append(misuses, fmt.Sprintf("the zero value %s has the method %s called on it", name, x.Sel.Name))
				}
			}
		case *ast.CallExpr:
			if tv, ok := info.Types[x.Fun]; ok && tv.IsType() {
				if len(x.Args) == 1 && interfaceTarget(tv.Type) {
					if name, ok := isZero(x.Args[0]); ok {
						misuses = append(misuses, fmt.Sprintf("the zero value %s is converted to the interface %s", name, tv.Type))
					}
				}

				return true
			}

			if signature, ok := types.Unalias(info.TypeOf(x.Fun)).(*types.Signature); ok {
				for i, arg := range x.Args {
					if name, ok := isZero(arg); ok && interfaceTarget(argumentParameterType(signature, i, x.Ellipsis.IsValid())) {
						misuses = append(misuses, fmt.Sprintf("the zero value %s is passed as an interface argument", name))
					}
				}
			}
		case *ast.AssignStmt:
			for i, rhs := range x.Rhs {
				if name, ok := isZero(rhs); ok && i < len(x.Lhs) && interfaceTarget(info.TypeOf(x.Lhs[i])) {
					misuses = append(misuses, fmt.Sprintf("the zero value %s is assigned to an interface", name))
				}
			}
		case *ast.ValueSpec:
			for _, value := range x.Values {
				if name, ok := isZero(value); ok && x.Type != nil && interfaceTarget(info.TypeOf(x.Type)) {
					misuses = append(misuses, fmt.Sprintf("the zero value %s is assigned to an interface", name))
				}
			}
		}

		return true
	})

	return misuses
}

// onwardGenericCalls lists, as phrases, each call inside decl to ANOTHER generic function that receives the type
// parameter as a type argument under a constraint not identical to the type parameter's own.
func onwardGenericCalls(decl *ast.FuncDecl, info *types.Info, typeParam *types.TypeParam) []string {
	var onward []string

	ast.Inspect(decl.Body, func(n ast.Node) bool {
		ident, ok := n.(*ast.Ident)

		if !ok {
			return true
		}

		instance, ok := info.Instances[ident]

		if !ok || instance.TypeArgs == nil {
			return true
		}

		fn, ok := info.Uses[ident].(*types.Func)

		if !ok {
			return true
		}

		signature, ok := fn.Origin().Type().(*types.Signature)

		if !ok || signature.TypeParams() == nil {
			return true
		}

		for i := range min(instance.TypeArgs.Len(), signature.TypeParams().Len()) {
			if instance.TypeArgs.At(i) != types.Type(typeParam) {
				continue
			}

			if inner := signature.TypeParams().At(i).Constraint(); !types.Identical(inner, typeParam.Constraint()) {
				onward = append(onward, fmt.Sprintf("%s is passed on to %s under the different constraint %s", typeParam.Obj().Name(), fn.Name(), inner))
			}
		}

		return true
	})

	return onward
}

// mentionsTypeParamOutsideInterface reports whether t mentions the type parameter outside an interface.
func mentionsTypeParamOutsideInterface(t types.Type, typeParam *types.TypeParam) bool {
	return typeMentionsTypeParam(t, typeParam, map[types.Type]bool{})
}

// constraintMentionsTypeParam reports whether a constraint mentions the type parameter: in a union term
// (`~[]O`), an embedded type, or a method signature.
func constraintMentionsTypeParam(constraint types.Type, typeParam *types.TypeParam) bool {
	iface, ok := constraint.Underlying().(*types.Interface)

	if !ok {
		return mentionsTypeParamOutsideInterface(constraint, typeParam)
	}

	for i := range iface.NumEmbeddeds() {
		switch embedded := iface.EmbeddedType(i).(type) {
		case *types.Union:
			for j := range embedded.Len() {
				if mentionsTypeParamOutsideInterface(embedded.Term(j).Type(), typeParam) {
					return true
				}
			}
		default:
			if mentionsTypeParamOutsideInterface(embedded, typeParam) {
				return true
			}
		}
	}

	return mentionsSelf(iface, typeParam)
}
