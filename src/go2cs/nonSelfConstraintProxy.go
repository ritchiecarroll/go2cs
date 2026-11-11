// nonSelfConstraintProxy.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A POINTER type argument against a NON-self-referential method-set constraint, at a generic FUNCTION call:
// tsc's `convertMapToOptions[O optionParser](m, keys, parser O) O` called with `&compilerOptionsParser{}`. The C#
// constraint `where O : optionParser` is nominal and golib's box implements no user interface (CS0311), so the type
// argument renders as a constraint proxy: the self-referential proxy with no self parameter, a class that holds the
// caller's box, implements the interface by forwarding every method as written, and converts implicitly to and from
// the box. A result of type O is cast back to the box, which is the same pointer (the proxy's own box), not a copy.
//
// The route is deliberately narrow (docs/phase4/DESIGN-constraint-proxy-nonself.md, section 9's rulings):
//
//   - a generic FUNCTION only: a generic named type would carry the proxy in its own identity (ruling 1);
//   - a type parameter that is a DIRECT parameter or result of the callee: one reached only as a func result keeps
//     funcResultProjection, one reached only inside []N keeps the element-wise widening;
//   - a constraint clause the callee actually EMITS (methodSetConstraintNeeded): an elided clause binds the box.
//
// The refusals by name live in nonSelfConstraintProxyRefusals.go.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

// nonSelfConstraintProxyKey prefixes a non-self proxy's constraintProxies entry. Its record closes no interface over
// a placeholder (packageInfoWriter), and its class lives in the assembly that records it even when the interface is
// foreign: no other assembly declares a non-self proxy for the pair.
const nonSelfConstraintProxyKey = "nonself|"

// nonSelfConstraintShape reports whether a (type parameter, type argument) pair has the non-self proxy's SHAPE:
// a pointer to a named type, against a NON-generic named method-set interface with at least one method. It returns
// the element and the constraint. The callee-dependent gates (direct position, emitted clause) are separate.
func nonSelfConstraintShape(typeParam *types.TypeParam, typeArg types.Type) (*types.Named, *types.Named, bool) {
	if typeParam == nil || typeArg == nil {
		return nil, nil, false
	}

	ptr, ok := types.Unalias(typeArg).(*types.Pointer)

	if !ok {
		return nil, nil, false
	}

	elemNamed, ok := types.Unalias(ptr.Elem()).(*types.Named)

	if !ok {
		return nil, nil, false
	}

	constraintNamed, ok := types.Unalias(typeParam.Constraint()).(*types.Named)

	if !ok || constraintNamed.TypeArgs().Len() > 0 || constraintNamed.Obj().Pkg() == nil {
		return nil, nil, false
	}

	iface, ok := constraintNamed.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 || !(iface.IsMethodSet() || isMethodSetWithPointerNamedUnion(iface)) {
		return nil, nil, false
	}

	return elemNamed, constraintNamed, true
}

// typeParamIsDirectPosition reports whether the type parameter is, by itself, a parameter or result of the signature.
func typeParamIsDirectPosition(signature *types.Signature, typeParam *types.TypeParam) bool {
	for i := range signature.Params().Len() {
		if signature.Params().At(i).Type() == types.Type(typeParam) {
			return true
		}
	}

	for i := range signature.Results().Len() {
		if signature.Results().At(i).Type() == types.Type(typeParam) {
			return true
		}
	}

	return false
}

// nonSelfConstraintProxyFor is constraintProxySigArg's fallback when constraintProxyFor gives no (self-referential)
// proxy: the non-self proxy's name for position `i` of the generic function `funIdent` names, recorded for
// package_info, or false when the route does not apply.
func (v *Visitor) nonSelfConstraintProxyFor(funIdent *ast.Ident, typeParams *types.TypeParamList, typeArgs *types.TypeList, i int) (string, bool) {
	fn, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return "", false
	}

	signature, ok := fn.Origin().Type().(*types.Signature)

	if !ok || signature.Recv() != nil {
		return "", false
	}

	typeParam := typeParams.At(i)
	elemNamed, constraintNamed, ok := nonSelfConstraintShape(typeParam, typeArgs.At(i))

	if !ok || !typeParamIsDirectPosition(signature, typeParam) {
		return "", false
	}

	// The existing routes keep their sites: a func-result projection, and an elided clause (the box binds).
	if _, _, projected := v.funcResultProjection(funIdent, typeArgs, i); projected {
		return "", false
	}

	if !v.methodSetConstraintNeeded(fn.Origin(), i) {
		return "", false
	}

	// Proxy name element-simple + ж + interface-simple + ᴾ — MUST match ImplementGenerator.EmitConstraintProxy. The
	// suffix keeps it distinct from the same-package pointer adapter, element + ж + interface.
	proxyName := elemNamed.Obj().Name() + PointerPrefix + constraintNamed.Obj().Name() + NonSelfConstraintProxySuffix

	elementFullName := v.getFullyQualifiedTypeName(elemNamed, false)

	if elemNamed.Obj().Pkg() != v.pkg {
		elementFullName = convertToCSFullTypeName(elementFullName)
	}

	interfaceFullName := v.getFullyQualifiedTypeName(constraintNamed, false)

	if constraintNamed.Obj().Pkg() != v.pkg {
		interfaceFullName = convertToCSFullTypeName(interfaceFullName)
	}

	packageLock.Lock()
	constraintProxies[nonSelfConstraintProxyKey+elementFullName+"|"+interfaceFullName] = [3]string{elementFullName, interfaceFullName, ""}
	packageLock.Unlock()

	return proxyName, true
}

// proxiedResultCast casts a generic call's result back to the box wherever the callee returns a type parameter
// whose type argument rendered as a constraint proxy (self-referential or not): Go's static type of the call is the
// pointer, and the proxy's implicit conversion yields its own box, the same pointer. A single result is cast; a
// multi-value result is rebuilt element-wise through a switch expression, since C# lifts no user-defined conversion
// through a tuple-typed value. A discarded call is left alone: a cast is not a C# statement.
func (v *Visitor) proxiedResultCast(callExpr *ast.CallExpr, rendered string) string {
	if callExpr == v.resultDiscardedExpr {
		return rendered
	}

	funIdent := getCallFunIdent(callExpr.Fun)

	if funIdent == nil {
		return rendered
	}

	instance, ok := v.info.Instances[funIdent]

	if !ok || instance.TypeArgs == nil {
		return rendered
	}

	fn, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return rendered
	}

	signature, ok := fn.Origin().Type().(*types.Signature)

	if !ok || signature.TypeParams() == nil || signature.Results().Len() == 0 {
		return rendered
	}

	// The box type each proxied result position converts back to, by result index.
	casts := map[int]string{}

	for r := range signature.Results().Len() {
		typeParam, ok := signature.Results().At(r).Type().(*types.TypeParam)

		if !ok || typeParam.Index() >= instance.TypeArgs.Len() {
			continue
		}

		if _, proxied := v.constraintProxySigArg(funIdent, instance.TypeArgs, typeParam.Index()); proxied {
			casts[r] = v.getCSharpTypeName(instance.TypeArgs.At(typeParam.Index()))
		}
	}

	if len(casts) == 0 {
		return rendered
	}

	if signature.Results().Len() == 1 {
		return fmt.Sprintf("((%s)(%s))", casts[0], rendered)
	}

	value := TempVarMarker + "r"
	elements := make([]string, signature.Results().Len())

	for r := range elements {
		element := fmt.Sprintf("%s.Item%d", value, r+1)

		if cast, ok := casts[r]; ok {
			element = fmt.Sprintf("(%s)%s", cast, element)
		}

		elements[r] = element
	}

	return fmt.Sprintf("(%s switch { var %s => (%s) })", rendered, value, strings.Join(elements, ", "))
}
