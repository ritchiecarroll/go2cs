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
//   - a type parameter that is a DIRECT parameter or result of the callee, or (class B) one reached in []T parameters
//     beside a func-typed parameter that mentions it, whose []T argument then projects to the proxy (sliceWideningRoute):
//     one reached only as a func result keeps funcResultProjection, one reached only inside []N keeps the element-wise
//     widening to the constraint;
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

// typeParamIsSliceParameter reports whether t is exactly []T (a variadic ...T included): the position convCallExpr's
// element-wise widening projects (elementWiseSliceWidening).
func typeParamIsSliceParameter(t types.Type, typeParam *types.TypeParam) bool {
	slice, ok := types.Unalias(t).(*types.Slice)

	if !ok {
		return false
	}

	elem, ok := types.Unalias(slice.Elem()).(*types.TypeParam)

	return ok && elem == typeParam
}

// typeParamTakesProxyRoute reports whether the type parameter's reach in the signature is one the non-self proxy
// carries: a direct parameter or result, or (TS readiness class B, tsc's tsbaseline.GetErrorBaseline) []T parameters
// beside a func-typed parameter whose signature mentions T. The second needs the proxy as much as the first: the []T
// argument projects element-wise either way, and only a type argument the callback's delegate can also name binds
// both, so `join[T Named](xs []T, cmp func(a, b T) int)` called with []*item renders T as the proxy, never as the
// constraint (the callback takes *item) or the box (the widened slice is not a slice of boxes).
func typeParamTakesProxyRoute(signature *types.Signature, typeParam *types.TypeParam) bool {
	if typeParamIsDirectPosition(signature, typeParam) {
		return true
	}

	sliceParameter, funcParameter := false, false

	for i := range signature.Params().Len() {
		paramType := signature.Params().At(i).Type()

		if typeParamIsSliceParameter(paramType, typeParam) {
			sliceParameter = true
			continue
		}

		if callback, ok := paramType.Underlying().(*types.Signature); ok && typeMentionsTypeParam(callback, typeParam, map[types.Type]bool{}) {
			funcParameter = true
		}
	}

	return sliceParameter && funcParameter
}

// typeParamReachedBesideSliceParameters reports whether the signature mentions the type parameter anywhere but in a
// []T parameter. Where it does not, the element-wise widening renders T as the constraint itself (go/ast's walkList).
func typeParamReachedBesideSliceParameters(signature *types.Signature, typeParam *types.TypeParam) bool {
	for i := range signature.Params().Len() {
		if paramType := signature.Params().At(i).Type(); !typeParamIsSliceParameter(paramType, typeParam) && typeMentionsTypeParam(paramType, typeParam, map[types.Type]bool{}) {
			return true
		}
	}

	for i := range signature.Results().Len() {
		if typeMentionsTypeParam(signature.Results().At(i).Type(), typeParam, map[types.Type]bool{}) {
			return true
		}
	}

	return false
}

// sliceWidening is what convCallExpr's element-wise []N widening projects one argument to.
type sliceWidening int

const (
	// wideningToConstraint: `widen<ж<E>, I>(xs, e => new EжI(e))`, T rendered as the constraint (walkList).
	wideningToConstraint sliceWidening = iota

	// wideningToProxy: `widen<ж<E>, EжIᴾ>(xs, e => new EжIᴾ(e))`, T rendered as the non-self proxy.
	wideningToProxy

	// wideningNone: T renders as the box (its clause is elided), so the caller's slice binds as it is, shared.
	wideningNone
)

// sliceWideningRoute decides what an argument elementWiseSliceWidening names projects to, from how the call renders the
// type parameter. convCallExpr's emission and the GO2CS2006 gate both ask it, so the gate examines exactly the
// arguments that are copied. A type parameter reached only in []T parameters keeps today's widening to the constraint.
func (v *Visitor) sliceWideningRoute(callExpr *ast.CallExpr, typeParam *types.TypeParam) sliceWidening {
	funIdent := getCallFunIdent(callExpr.Fun)

	if funIdent == nil {
		return wideningToConstraint
	}

	instance, ok := v.info.Instances[funIdent]

	if !ok || instance.TypeArgs == nil {
		return wideningToConstraint
	}

	fn, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return wideningToConstraint
	}

	signature, ok := fn.Origin().Type().(*types.Signature)

	if !ok || signature.Recv() != nil || signature.TypeParams() == nil {
		return wideningToConstraint
	}

	index := typeParam.Index()

	if index < 0 || index >= signature.TypeParams().Len() || signature.TypeParams().At(index) != typeParam || index >= instance.TypeArgs.Len() {
		return wideningToConstraint
	}

	if !typeParamReachedBesideSliceParameters(signature, typeParam) {
		return wideningToConstraint
	}

	// The self-referential proxy keeps today's emission: no site reaches it (the class B census: std x4, the
	// behavioral corpus and the tsc closure).
	if _, ok := v.constraintProxyFor(typeParam, instance.TypeArgs.At(index)); ok {
		return wideningToConstraint
	}

	if _, _, ok := v.nonSelfConstraintProxyPair(funIdent, signature.TypeParams(), instance.TypeArgs, index); ok {
		return wideningToProxy
	}

	// An elided clause binds the box, so the caller's slice is passed as it is. A kept clause the proxy does not
	// carry keeps today's widening: no census site reaches it, and the shapes the tsc closure holds are refused by name
	// (GO2CS2005 ruling 1, GO2CS2006).
	if !v.methodSetConstraintNeeded(fn.Origin(), index) {
		return wideningNone
	}

	return wideningToConstraint
}

// nonSelfConstraintProxyPair is the element and the constraint of the non-self proxy for position `i` of the generic
// function `funIdent` names, when the route applies there. It records nothing, so the GO2CS2006 gate can ask it before
// any emission.
func (v *Visitor) nonSelfConstraintProxyPair(funIdent *ast.Ident, typeParams *types.TypeParamList, typeArgs *types.TypeList, i int) (*types.Named, *types.Named, bool) {
	fn, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return nil, nil, false
	}

	signature, ok := fn.Origin().Type().(*types.Signature)

	if !ok || signature.Recv() != nil {
		return nil, nil, false
	}

	typeParam := typeParams.At(i)
	elemNamed, constraintNamed, ok := nonSelfConstraintShape(typeParam, typeArgs.At(i))

	if !ok || !typeParamTakesProxyRoute(signature, typeParam) {
		return nil, nil, false
	}

	// The existing routes keep their sites: a func-result projection, and an elided clause (the box binds).
	if _, _, projected := v.funcResultProjection(funIdent, typeArgs, i); projected {
		return nil, nil, false
	}

	if !v.methodSetConstraintNeeded(fn.Origin(), i) {
		return nil, nil, false
	}

	return elemNamed, constraintNamed, true
}

// nonSelfConstraintProxyFor is constraintProxySigArg's fallback when constraintProxyFor gives no (self-referential)
// proxy: the non-self proxy's name for position `i` of the generic function `funIdent` names, recorded for
// package_info, or false when the route does not apply.
func (v *Visitor) nonSelfConstraintProxyFor(funIdent *ast.Ident, typeParams *types.TypeParamList, typeArgs *types.TypeList, i int) (string, bool) {
	elemNamed, constraintNamed, ok := v.nonSelfConstraintProxyPair(funIdent, typeParams, typeArgs, i)

	if !ok {
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
