// evalOrderInPlace.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The operands gc evaluates IN PLACE rather than after the statement's calls (evalOrderSpill.go).
//
// gc's order pass passes two kinds of operand by address to a runtime routine, through addrTemp
// (cmd/compile/internal/walk/order.go):
//
//   - an operand converted to an interface whose conversion helper takes its address
//     (dataWordFuncName's needsaddr: not a 2-, 4- or 8-byte word without pointers, and not a lone
//     string or slice);
//   - a map key on the slow path (mapfast reports mapslow).
//
// addrTemp copies the operand into a temporary AT ITS OWN POSITION, after the calls inside it and
// before the calls after it, when the operand is not addressable, or when it is a local variable of
// the function that is SSA-able and never has its address taken. Otherwise it leaves the operand to
// be read after the calls, like any other read. A read inside an operand copied in place therefore
// conflicts only with the calls inside that operand: the operand is the read's context.
//
//	fmt.Println(s.f+1, s.poke())   // s.f int8: gc prints 2, under every flag
//
// The second case depends on gc's inliner. A local whose only writer is a closure keeps its address
// taken while the closure captures it by reference. Once gc inlines the closure, nothing takes the
// address and the local is copied in place. go2cs takes the default build's order, where a small
// closure called directly is inlined (ruled with COORD, option B). The residue is disclosed in
// docs/ConversionStrategies.md: a closure gc does not inline reads the local after the call under gc
// and before it under go2cs. It touches small structs, 1-byte scalars, complex values and
// pointer-shaped locals.
package main

import (
	"go/ast"
	"go/token"
	"go/types"
)

// markInPlace records the operands of exprs that gc copies in place.
func (p *eoPlanner) markInPlace(exprs ...ast.Expr) {
	for _, expr := range exprs {
		if expr == nil {
			continue
		}

		ast.Inspect(expr, func(node ast.Node) bool {
			switch n := node.(type) {
			case *ast.FuncLit:
				return false // a literal's body is its own statements
			case *ast.CallExpr:
				p.markCallInPlace(n)
			case *ast.CompositeLit:
				p.markCompositeInPlace(n)
			case *ast.IndexExpr:
				if mapType, ok := p.v.eoUnderlyingOf(n.X).(*types.Map); ok {
					p.considerMapKey(mapType, n.Index)
				}
			case *ast.BinaryExpr:
				if n.Op == token.EQL || n.Op == token.NEQ {
					left, right := p.v.eoTypeOf(n.X), p.v.eoTypeOf(n.Y)

					if eoIsInterface(left) && !eoIsInterface(right) {
						p.considerIface(n.Y)
					} else if eoIsInterface(right) && !eoIsInterface(left) {
						p.considerIface(n.X)
					}
				}
			}

			return true
		})
	}
}

// markAssignInPlace records an assignment's right sides that become an interface on the left.
func (p *eoPlanner) markAssignInPlace(assignStmt *ast.AssignStmt) {
	if assignStmt.Tok != token.ASSIGN || len(assignStmt.Lhs) != len(assignStmt.Rhs) {
		return
	}

	for i, lhs := range assignStmt.Lhs {
		if eoIsInterface(p.v.eoTypeOf(lhs)) {
			p.considerIface(assignStmt.Rhs[i])
		}
	}
}

func (p *eoPlanner) markCallInPlace(call *ast.CallExpr) {
	if isConversion, _ := p.v.isTypeConversion(call); isConversion {
		if len(call.Args) == 1 && eoIsInterface(p.v.eoTypeOf(call.Fun)) {
			p.considerIface(call.Args[0])
		}

		return
	}

	if ident, ok := ast.Unparen(call.Fun).(*ast.Ident); ok && p.v.identIsUniverseBuiltin(ident) {
		switch ident.Name {
		case "panic":
			if len(call.Args) == 1 {
				p.considerIface(call.Args[0])
			}
		case "append":
			if sliceType, ok := p.v.eoUnderlyingOf(call).(*types.Slice); ok && call.Ellipsis == token.NoPos && eoIsInterface(sliceType.Elem()) {
				for _, arg := range call.Args[1:] {
					p.considerIface(arg)
				}
			}
		case "delete":
			if len(call.Args) == 2 {
				if mapType, ok := p.v.eoUnderlyingOf(call.Args[0]).(*types.Map); ok {
					p.considerMapKey(mapType, call.Args[1])
				}
			}
		}

		return
	}

	signature, ok := p.v.eoUnderlyingOf(call.Fun).(*types.Signature)

	if !ok {
		return
	}

	params := signature.Params()

	if len(call.Args) == 1 && params.Len() > 1 {
		return // f(g()) spreads a tuple
	}

	for i, arg := range call.Args {
		var paramType types.Type

		switch {
		case signature.Variadic() && i >= params.Len()-1:
			if call.Ellipsis != token.NoPos {
				continue // the slice is passed as is
			}

			if slice, ok := params.At(params.Len() - 1).Type().Underlying().(*types.Slice); ok {
				paramType = slice.Elem()
			}
		case i < params.Len():
			paramType = params.At(i).Type()
		}

		if paramType != nil && eoIsInterface(paramType) {
			p.considerIface(arg)
		}
	}
}

func (p *eoPlanner) markCompositeInPlace(lit *ast.CompositeLit) {
	switch literalType := p.v.eoUnderlyingOf(lit).(type) {
	case *types.Slice, *types.Array:
		var elem types.Type

		if slice, ok := literalType.(*types.Slice); ok {
			elem = slice.Elem()
		} else {
			elem = literalType.(*types.Array).Elem()
		}

		if !eoIsInterface(elem) {
			return
		}

		for _, elt := range lit.Elts {
			if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
				elt = keyValue.Value
			}

			p.considerIface(elt)
		}
	case *types.Map:
		for _, elt := range lit.Elts {
			if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
				if eoIsInterface(literalType.Key()) {
					p.considerIface(keyValue.Key)
				}

				if eoIsInterface(literalType.Elem()) {
					p.considerIface(keyValue.Value)
				}
			}
		}
	case *types.Struct:
		for i, elt := range lit.Elts {
			var field *types.Var

			if keyValue, ok := elt.(*ast.KeyValueExpr); ok {
				elt = keyValue.Value

				if name, ok := keyValue.Key.(*ast.Ident); ok {
					field, _ = p.v.info.ObjectOf(name).(*types.Var)
				}
			} else if i < literalType.NumFields() {
				field = literalType.Field(i)
			}

			if field != nil && eoIsInterface(field.Type()) {
				p.considerIface(elt)
			}
		}
	}
}

// considerIface marks operand when its conversion to an interface goes through addrTemp in place.
func (p *eoPlanner) considerIface(operand ast.Expr) {
	operand = ast.Unparen(operand)
	tv, ok := p.v.info.Types[operand]

	if !ok || tv.Type == nil || tv.Value != nil || tv.IsNil() || eoIsInterface(tv.Type) {
		return // a constant is a static literal; an interface converts without a helper
	}

	if !p.v.eoConversionNeedsAddress(tv.Type) {
		return
	}

	p.markIfCopiedInPlace(operand)
}

// considerMapKey marks key when mapType hashes on the slow path, which passes the key by address.
func (p *eoPlanner) considerMapKey(mapType *types.Map, key ast.Expr) {
	key = ast.Unparen(key)

	if tv, ok := p.v.info.Types[key]; !ok || tv.Value != nil {
		return
	}

	if !p.v.eoMapKeySlow(mapType) {
		return
	}

	p.markIfCopiedInPlace(key)
}

func (p *eoPlanner) markIfCopiedInPlace(operand ast.Expr) {
	if !p.v.eoCopiedInPlace(operand) {
		return
	}

	if p.inPlace == nil {
		p.inPlace = map[ast.Expr]bool{}
	}

	p.inPlace[operand] = true
}

// eoCopiedInPlace is addrTemp's copy condition: the operand is not addressable, or its outer value is
// a local of the enclosing function that is SSA-able and never has its address taken.
func (v *Visitor) eoCopiedInPlace(operand ast.Expr) bool {
	if !v.eoAddressable(operand) {
		return true
	}

	path, ok := v.eoPathOf(operand)

	if !ok {
		return false
	}

	for _, step := range path.steps {
		if step.deref {
			return false // the outer value is not a variable: a slice element, a pointee
		}
	}

	variable, ok := path.root.(*types.Var)

	if !ok || variable.Pkg() == nil || variable.Parent() == variable.Pkg().Scope() || variable.IsField() {
		return false // a package variable is never copied
	}

	if !v.eoLocalOfEnclosingFunc(variable, operand.Pos()) {
		return false // a parameter, a result, or a variable an enclosing literal captures
	}

	return v.eoCanSSA(variable.Type()) && !v.eoAddressTaken(variable)
}

// eoAddressable reports Go's addressability, which is gc's IsAddressable for these forms.
func (v *Visitor) eoAddressable(expr ast.Expr) bool {
	switch e := ast.Unparen(expr).(type) {
	case *ast.Ident:
		_, isVar := v.info.ObjectOf(e).(*types.Var)
		return isVar
	case *ast.StarExpr:
		return true
	case *ast.SelectorExpr:
		if selection, ok := v.info.Selections[e]; ok {
			if selection.Kind() != types.FieldVal {
				return false
			}

			if selection.Indirect() {
				return true
			}

			return v.eoAddressable(e.X)
		}

		_, isVar := v.info.ObjectOf(e.Sel).(*types.Var) // a qualified package variable
		return isVar
	case *ast.IndexExpr:
		switch base := v.eoUnderlyingOf(e.X).(type) {
		case *types.Slice:
			return true
		case *types.Pointer:
			_, isArray := base.Elem().Underlying().(*types.Array)
			return isArray
		case *types.Array:
			return v.eoAddressable(e.X)
		}
	}

	return false
}

// eoLocalOfEnclosingFunc reports whether variable is declared in the body of the innermost function
// (declaration or literal) enclosing pos, and is not one of its parameters or results.
func (v *Visitor) eoLocalOfEnclosingFunc(variable *types.Var, pos token.Pos) bool {
	scopes := v.eoFuncScopes()

	if scopes == nil || v.pkg == nil {
		return false
	}

	funcScopeOf := func(scope *types.Scope) *types.Scope {
		for ; scope != nil; scope = scope.Parent() {
			if _, ok := scopes[scope]; ok {
				return scope
			}
		}

		return nil
	}

	declared := funcScopeOf(variable.Parent())

	if declared == nil || declared != funcScopeOf(v.pkg.Scope().Innermost(pos)) {
		return false
	}

	signature := scopes[declared]

	if signature == nil {
		return false
	}

	if signature.Recv() == variable {
		return false
	}

	for _, tuple := range []*types.Tuple{signature.Params(), signature.Results()} {
		for i := 0; i < tuple.Len(); i++ {
			if tuple.At(i) == variable {
				return false
			}
		}
	}

	return true
}

// eoFuncScopes maps the scope of the current declaration and of each literal inside it to its
// signature. It is computed once per declaration.
func (v *Visitor) eoFuncScopes() map[*types.Scope]*types.Signature {
	decl := v.currentFuncDecl

	if decl == nil {
		return nil
	}

	if v.evalOrderFuncScopesDecl == decl {
		return v.evalOrderFuncScopes
	}

	scopes := map[*types.Scope]*types.Signature{}

	if scope := v.info.Scopes[decl.Type]; scope != nil {
		if fn, ok := v.info.Defs[decl.Name].(*types.Func); ok {
			scopes[scope], _ = fn.Type().(*types.Signature)
		}
	}

	if decl.Body != nil {
		ast.Inspect(decl.Body, func(node ast.Node) bool {
			if lit, ok := node.(*ast.FuncLit); ok {
				if scope := v.info.Scopes[lit.Type]; scope != nil {
					scopes[scope], _ = v.eoTypeOf(lit).(*types.Signature)
				}
			}

			return true
		})
	}

	v.evalOrderFuncScopesDecl, v.evalOrderFuncScopes = decl, scopes
	v.evalOrderAddrTaken = nil

	return scopes
}

// eoAddressTaken reports whether the current declaration takes variable's address anywhere: `&V`,
// `&V.f`, slicing an array V, or a pointer method called on V (gc's Addrtaken).
func (v *Visitor) eoAddressTaken(variable *types.Var) bool {
	if v.eoFuncScopes() == nil {
		return true
	}

	if taken, ok := v.evalOrderAddrTaken[variable]; ok {
		return taken
	}

	rootInPlace := func(expr ast.Expr) bool {
		path, ok := v.eoPathOf(expr)

		if !ok || path.root != variable {
			return false
		}

		for _, step := range path.steps {
			if step.deref {
				return false
			}
		}

		return true
	}

	taken := false

	ast.Inspect(v.currentFuncDecl.Body, func(node ast.Node) bool {
		if taken {
			return false
		}

		switch n := node.(type) {
		case *ast.UnaryExpr:
			taken = n.Op == token.AND && rootInPlace(n.X)
		case *ast.SliceExpr:
			_, isArray := v.eoUnderlyingOf(n.X).(*types.Array)
			taken = isArray && rootInPlace(n.X)
		case *ast.SelectorExpr:
			if selection, ok := v.info.Selections[n]; ok && selection.Kind() != types.FieldVal && !selection.Indirect() {
				if signature, ok := selection.Obj().Type().(*types.Signature); ok && signature.Recv() != nil {
					_, pointerRecv := signature.Recv().Type().(*types.Pointer)
					_, pointerX := v.eoUnderlyingOf(n.X).(*types.Pointer)
					taken = pointerRecv && !pointerX && rootInPlace(n.X)
				}
			}
		}

		return !taken
	})

	if v.evalOrderAddrTaken == nil {
		v.evalOrderAddrTaken = map[*types.Var]bool{}
	}

	v.evalOrderAddrTaken[variable] = taken

	return taken
}

// eoIsInterface is types.IsInterface for a type that may be missing: a blank target `_` has none.
func eoIsInterface(t types.Type) bool {
	return t != nil && types.IsInterface(t)
}

func (v *Visitor) eoTypeOf(expr ast.Expr) types.Type {
	if tv, ok := v.info.Types[expr]; ok {
		return tv.Type
	}

	if ident, ok := expr.(*ast.Ident); ok {
		if obj := v.info.ObjectOf(ident); obj != nil {
			return obj.Type()
		}
	}

	return nil
}

func (v *Visitor) eoUnderlyingOf(expr ast.Expr) types.Type {
	if t := v.eoTypeOf(expr); t != nil {
		return t.Underlying()
	}

	return nil
}

func (v *Visitor) eoSizes() types.Sizes {
	if sizes := types.SizesFor("gc", platformArch(v.options.targetPlatform)); sizes != nil {
		return sizes
	}

	return types.SizesFor("gc", "amd64")
}

// eoConversionNeedsAddress is gc's dataWordFuncName needsaddr (walk/convert.go).
func (v *Visitor) eoConversionNeedsAddress(t types.Type) bool {
	if !eoSizeKnown(t) {
		return false // a type parameter: gc's helper depends on each instantiation's shape
	}

	sizes := v.eoSizes()
	size, align := sizes.Sizeof(t), sizes.Alignof(t)
	hasPointers := eoHasPointers(t)

	switch {
	case size == 2 && align == 2:
		return false
	case size == 4 && align == 4 && !hasPointers:
		return false
	case size == 8 && align == sizes.Alignof(types.Typ[types.Uint64]) && !hasPointers:
		return false
	}

	if sole := eoSoleComponent(t); sole != nil {
		if basic, ok := sole.Underlying().(*types.Basic); ok && basic.Kind() == types.String {
			return false
		}

		if _, ok := sole.Underlying().(*types.Slice); ok {
			return false
		}
	}

	return true
}

// eoCanSSA is gc's ssa.CanSSA (ssa/value.go) without the SIMD experiment.
func (v *Visitor) eoCanSSA(t types.Type) bool {
	if !eoSizeKnown(t) {
		return false
	}

	sizes := v.eoSizes()
	size := sizes.Sizeof(t)

	if size == 0 {
		return true
	}

	if size > 4*sizes.Sizeof(types.Typ[types.UnsafePointer]) {
		return false
	}

	switch u := t.Underlying().(type) {
	case *types.Array:
		return u.Len() <= 1 && v.eoCanSSA(u.Elem())
	case *types.Struct:
		if eoIsDirectIface(t) {
			return true
		}

		if u.NumFields() > 4 {
			return false
		}

		for i := 0; i < u.NumFields(); i++ {
			if !v.eoCanSSA(u.Field(i).Type()) {
				return false
			}
		}
	}

	return true
}

// eoMapKeySlow reports gc's mapfast(t) == mapslow (walk/walk.go).
func (v *Visitor) eoMapKeySlow(mapType *types.Map) bool {
	if !eoSizeKnown(mapType.Key()) || !eoSizeKnown(mapType.Elem()) {
		return false
	}

	sizes := v.eoSizes()

	if sizes.Sizeof(mapType.Elem()) > 128 { // abi.MapMaxElemBytes
		return true
	}

	key := mapType.Key()
	pointerSize := sizes.Sizeof(types.Typ[types.UnsafePointer])

	switch v.eoAlg(key) {
	case eoAlgString:
		return false
	case eoAlgMem:
		switch sizes.Sizeof(key) {
		case 4:
			return eoHasPointers(key) && pointerSize != 4
		case 8:
			return eoHasPointers(key) && pointerSize != 8
		}
	}

	return true
}

const (
	eoAlgMem = iota
	eoAlgString
	eoAlgOther
)

// eoAlg is gc's equality algorithm kind (types/size.go), reduced to what mapfast reads.
func (v *Visitor) eoAlg(t types.Type) int {
	switch u := t.Underlying().(type) {
	case *types.Basic:
		switch {
		case u.Kind() == types.String:
			return eoAlgString
		case u.Info()&(types.IsFloat|types.IsComplex) != 0:
			return eoAlgOther
		}

		return eoAlgMem
	case *types.Pointer, *types.Chan:
		return eoAlgMem
	case *types.Array:
		elem := v.eoAlg(u.Elem())

		switch {
		case elem == eoAlgMem || u.Len() == 0:
			return eoAlgMem
		case u.Len() == 1:
			return elem
		}

		return eoAlgOther
	case *types.Struct:
		if u.NumFields() == 1 && u.Field(0).Name() != "_" {
			return v.eoAlg(u.Field(0).Type())
		}

		fields := make([]*types.Var, u.NumFields())

		for i := range fields {
			fields[i] = u.Field(i)
		}

		sizes := v.eoSizes()
		offsets := sizes.Offsetsof(fields)

		for i, field := range fields {
			if v.eoAlg(field.Type()) != eoAlgMem || field.Name() == "_" {
				return eoAlgOther
			}

			end := sizes.Sizeof(t)

			if i+1 < len(fields) {
				end = offsets[i+1]
			}

			if offsets[i]+sizes.Sizeof(field.Type()) != end {
				return eoAlgOther // padded
			}
		}

		return eoAlgMem
	}

	return eoAlgOther
}

// eoSizeKnown reports whether types.Sizes can size t: a type parameter has no size of its own, and
// gcSizes asserts on one, directly or as an array element or struct field.
func eoSizeKnown(t types.Type) bool {
	switch u := t.(type) {
	case *types.TypeParam:
		return false
	case *types.Alias:
		return eoSizeKnown(types.Unalias(u))
	case *types.Named:
		return eoSizeKnown(u.Underlying())
	case *types.Array:
		return eoSizeKnown(u.Elem())
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if !eoSizeKnown(u.Field(i).Type()) {
				return false
			}
		}
	}

	return true // a pointer, slice, map, chan, func or interface is a fixed number of words
}

func eoHasPointers(t types.Type) bool {
	switch u := t.Underlying().(type) {
	case *types.Basic:
		return u.Kind() == types.String || u.Kind() == types.UnsafePointer
	case *types.Array:
		return u.Len() > 0 && eoHasPointers(u.Elem())
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if eoHasPointers(u.Field(i).Type()) {
				return true
			}
		}

		return false
	}

	return true // pointer, slice, map, chan, func, interface
}

func eoSoleComponent(t types.Type) types.Type {
	switch u := t.Underlying().(type) {
	case *types.Struct:
		if u.NumFields() != 1 {
			return nil
		}

		return eoSoleComponent(u.Field(0).Type())
	case *types.Array:
		if u.Len() != 1 {
			return nil
		}

		return eoSoleComponent(u.Elem())
	}

	return t
}

func eoIsDirectIface(t types.Type) bool {
	switch u := t.Underlying().(type) {
	case *types.Pointer, *types.Chan, *types.Map, *types.Signature:
		return true
	case *types.Basic:
		return u.Kind() == types.UnsafePointer
	case *types.Array:
		return u.Len() == 1 && eoIsDirectIface(u.Elem())
	case *types.Struct:
		return u.NumFields() == 1 && eoIsDirectIface(u.Field(0).Type())
	}

	return false
}
