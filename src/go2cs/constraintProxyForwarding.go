// constraintProxyForwarding.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file decides which self-referential constraint methods a constraint proxy can FORWARD, and
// refuses, before any file is written, the instantiations that would need one it cannot.
//
// A pointer type argument of a self-referential constraint (`F[N Tree[N]]` called with `*Node`) renders
// as a constraint proxy, a C# class wrapping the element's box ж<Node> (constraintProxyFor). The proxy's
// methods take and return the self parameter as the PROXY, while the element's Go methods take and
// return it as the BOX, so each forwarder marshals every position that mentions the self parameter:
//
//   - the whole parameter or result converts implicitly (the proxy declares both conversions);
//   - a func type converts by re-wrapping it in a lambda (nil stays nil), recursively;
//   - an array converts element by element, which is exact because Go copies arrays;
//   - a type that renders identically on both sides needs nothing: the self parameter occurs in it only
//     as a type argument whose type parameter is itself self-constrained (`*Graph[N]` with
//     `Graph[N GraphNode[N]]`), where the slot names the proxy too.
//
// Every other position ALIASES the element's storage in Go and cannot be marshalled: a `[]N`, `map[K]N`,
// `chan N` or `*N` result shares its backing with the element, and a proxy slice is a different array
// from the box slice, since each proxy is its own C# object. A copy would compile and silently break Go's
// aliasing, so go2cs refuses such an instantiation by name instead. Value-element instantiations need no
// proxy and are unaffected; go2cs-gen's proxies throw the same refusal from those forwarders, so the
// interface's own package still compiles.

package main

import (
	"fmt"
	"go/token"
	"go/types"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

// constraintProxyForwards reports whether a constraint method's parameter or result type `t` can be
// forwarded across the proxy/box boundary of the self parameter `self`.
func constraintProxyForwards(t types.Type, self *types.TypeParam) bool {
	if t == types.Type(self) || !mentionsSelf(t, self) {
		return true
	}

	switch x := types.Unalias(t).(type) {
	case *types.Signature:
		for i := range x.Params().Len() {
			if !constraintProxyForwards(x.Params().At(i).Type(), self) {
				return false
			}
		}

		for i := range x.Results().Len() {
			if !constraintProxyForwards(x.Results().At(i).Type(), self) {
				return false
			}
		}

		return true
	case *types.Array:
		return constraintProxyForwards(x.Elem(), self)
	case *types.Named:
		// A named func type (`iter.Seq[N]`) is a C# delegate, re-wrapped exactly as a func is.
		if _, ok := x.Underlying().(*types.Signature); ok {
			return constraintProxyForwards(x.Underlying(), self)
		}
	}

	return constraintProxyRendersAlike(t, self)
}

// constraintProxyRendersAlike reports whether `t` names the same C# type with the self parameter rendered
// as the box or as the proxy: the self parameter occurs in it only as a type argument whose type parameter
// is constrained by an interface closed over that parameter (ImplementGenerator's RenderSlotType names the
// proxy there).
func constraintProxyRendersAlike(t types.Type, self *types.TypeParam) bool {
	if !mentionsSelf(t, self) {
		return true
	}

	switch x := types.Unalias(t).(type) {
	case *types.Pointer:
		return constraintProxyRendersAlike(x.Elem(), self)
	case *types.Slice:
		return constraintProxyRendersAlike(x.Elem(), self)
	case *types.Array:
		return constraintProxyRendersAlike(x.Elem(), self)
	case *types.Chan:
		return constraintProxyRendersAlike(x.Elem(), self)
	case *types.Map:
		return constraintProxyRendersAlike(x.Key(), self) && constraintProxyRendersAlike(x.Elem(), self)
	case *types.Named:
		params := x.Origin().TypeParams()

		for i := range x.TypeArgs().Len() {
			arg := x.TypeArgs().At(i)

			if arg == types.Type(self) && i < params.Len() && typeParamConstrainsItself(params.At(i)) {
				continue
			}

			if !constraintProxyRendersAlike(arg, self) {
				return false
			}
		}

		return true
	}

	return false
}

// typeParamConstrainsItself reports whether a type parameter's constraint is a generic instantiation
// naming the parameter itself (`N GraphNode[N]`).
func typeParamConstrainsItself(typeParam *types.TypeParam) bool {
	constraint, ok := types.Unalias(typeParam.Constraint()).(*types.Named)

	if !ok {
		return false
	}

	for i := range constraint.TypeArgs().Len() {
		if constraint.TypeArgs().At(i) == types.Type(typeParam) {
			return true
		}
	}

	return false
}

// inlineConstraintName names an inline constraint in a refusal, which has no declared name.
const inlineConstraintName = "interface{…}"

// selfReferentialConstraint resolves a type parameter's constraint to the interface whose method set a
// proxy would forward, and the parameter that interface's methods name as "self": the interface's own
// type parameter for a named constraint (`N Tree[N]`), the type parameter itself for an inline one
// (`N interface{ Kids() []N }`). It answers false for any constraint constraintProxyFor gives no proxy.
func selfReferentialConstraint(typeParam *types.TypeParam) (string, *types.Interface, *types.TypeParam, bool) {
	switch constraint := types.Unalias(typeParam.Constraint()).(type) {
	case *types.Named:
		origin := constraint.Origin()
		iface, ok := origin.Underlying().(*types.Interface)

		if !ok || iface.NumMethods() == 0 || !(iface.IsMethodSet() || isMethodSetWithPointerNamedUnion(iface)) {
			return "", nil, nil, false
		}

		for j := range constraint.TypeArgs().Len() {
			if constraint.TypeArgs().At(j) == types.Type(typeParam) {
				return origin.Obj().Name(), iface, origin.TypeParams().At(j), true
			}
		}
	case *types.Interface:
		if constraint.NumMethods() > 0 && (constraint.IsMethodSet() || isMethodSetWithPointerNamedUnion(constraint)) &&
			mentionsSelf(constraint, typeParam) {
			return inlineConstraintName, constraint, typeParam, true
		}
	}

	return "", nil, nil, false
}

// unforwardableConstraintMethods lists, as "Tree.Children (returns []N)" phrases, every method of the constraint
// interface a proxy could not forward. An inline constraint has no name to qualify the method with.
func unforwardableConstraintMethods(interfaceName string, iface *types.Interface, self *types.TypeParam) []string {
	var phrases []string

	for i := range iface.NumMethods() {
		method := iface.Method(i)
		signature := method.Type().(*types.Signature)

		for j := range signature.Params().Len() {
			if param := signature.Params().At(j); !constraintProxyForwards(param.Type(), self) {
				phrases = append(phrases, fmt.Sprintf("%s (takes %s)", qualifiedMethodName(interfaceName, method), types.TypeString(param.Type(), types.RelativeTo(method.Pkg()))))
			}
		}

		for j := range signature.Results().Len() {
			if result := signature.Results().At(j); !constraintProxyForwards(result.Type(), self) {
				phrases = append(phrases, fmt.Sprintf("%s (returns %s)", qualifiedMethodName(interfaceName, method), types.TypeString(result.Type(), types.RelativeTo(method.Pkg()))))
			}
		}
	}

	return phrases
}

// constraintProxyRefusal is the error refuseUnforwardableConstraintProxies returns: one canonical
// diagnostic per instantiation that would name a proxy unable to forward its constraint.
type constraintProxyRefusal struct {
	pkgPath string
	sites   []constraintProxyRefusalSite
}

type constraintProxyRefusalSite struct {
	position token.Position
	text     string
}

func (refusal *constraintProxyRefusal) Error() string {
	return fmt.Sprintf("package %s instantiates a self-referential constraint over a pointer element whose constraint a C# constraint proxy cannot forward (%d site(s)); see the %s diagnostics",
		refusal.pkgPath, len(refusal.sites), diagnosticProxyRefused)
}

// report prints each refused site as a canonical diagnostic.
func (refusal *constraintProxyRefusal) report() {
	for _, site := range refusal.sites {
		reportDiagnostic(site.position.String(), diagnosticProxyRefused, site.text)
	}
}

// refuseUnforwardableConstraintProxies finds every instantiation in the package whose type argument is a
// pointer to a named type under a self-referential method-set constraint (the instantiations
// constraintProxyFor renders as a proxy) where that constraint has a method the proxy cannot forward.
// It returns nil when there is none, which is every package in the Go 1.24 standard library.
func refuseUnforwardableConstraintProxies(pkg *packages.Package) *constraintProxyRefusal {
	if pkg.TypesInfo == nil {
		return nil
	}

	refusal := &constraintProxyRefusal{pkgPath: pkg.PkgPath}
	seen := map[string]bool{}

	for ident, instance := range pkg.TypesInfo.Instances {
		typeParams := genericTypeParams(pkg.TypesInfo.Uses[ident], instance.Type)

		if typeParams == nil || instance.TypeArgs == nil {
			continue
		}

		for i := range min(typeParams.Len(), instance.TypeArgs.Len()) {
			ptr, ok := types.Unalias(instance.TypeArgs.At(i)).(*types.Pointer)

			if !ok {
				continue
			}

			if _, ok := types.Unalias(ptr.Elem()).(*types.Named); !ok {
				continue
			}

			interfaceName, iface, self, ok := selfReferentialConstraint(typeParams.At(i))

			if !ok {
				continue
			}

			phrases := unforwardableConstraintMethods(interfaceName, iface, self)

			if len(phrases) == 0 {
				continue
			}

			position := pkg.Fset.Position(ident.Pos())
			text := fmt.Sprintf("%s[%s] needs a C# constraint proxy for %s, and the proxy cannot forward %s: in Go those values share the element's own storage, while each proxy is a separate C# object, so a forwarded copy would break that sharing. go2cs refuses this instantiation; a value element (not a pointer) converts",
				ident.Name, types.TypeString(instance.TypeArgs.At(i), types.RelativeTo(pkg.Types)), interfaceName, strings.Join(phrases, ", "))

			if key := position.String() + "|" + text; !seen[key] {
				seen[key] = true
				refusal.sites = append(refusal.sites, constraintProxyRefusalSite{position, text})
			}
		}
	}

	if len(refusal.sites) == 0 {
		return nil
	}

	sort.Slice(refusal.sites, func(a, b int) bool {
		left, right := refusal.sites[a].position, refusal.sites[b].position

		if left.Filename != right.Filename {
			return left.Filename < right.Filename
		}

		if left.Line != right.Line {
			return left.Line < right.Line
		}

		return left.Column < right.Column
	})

	return refusal
}

// genericTypeParams is the type-parameter list an instantiation closes: a generic function's, or a
// generic named type's.
func genericTypeParams(object types.Object, instantiated types.Type) *types.TypeParamList {
	if fn, ok := object.(*types.Func); ok {
		if signature, ok := fn.Type().(*types.Signature); ok {
			return signature.TypeParams()
		}
	}

	if named, ok := types.Unalias(instantiated).(*types.Named); ok {
		return named.Origin().TypeParams()
	}

	return nil
}

// mentionsSelf reports whether `t` mentions the self parameter anywhere, an interface's methods included
// (typeMentionsTypeParam does not look inside an interface, which an inline constraint is).
func mentionsSelf(t types.Type, self *types.TypeParam) bool {
	if iface, ok := types.Unalias(t).(*types.Interface); ok {
		for i := range iface.NumMethods() {
			if mentionsSelf(iface.Method(i).Type(), self) {
				return true
			}
		}

		return false
	}

	return typeMentionsTypeParam(t, self, map[types.Type]bool{})
}

// qualifiedMethodName spells a constraint method as its interface's, `Tree.Children`; an inline constraint's
// method stays bare.
func qualifiedMethodName(interfaceName string, method *types.Func) string {
	if interfaceName == inlineConstraintName {
		return method.Name()
	}

	return interfaceName + "." + method.Name()
}
