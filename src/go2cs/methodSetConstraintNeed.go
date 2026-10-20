// methodSetConstraintNeed.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/token"
	"go/types"
)

// A function type parameter constrained by a regular METHOD-SET interface (`[E error]`, go/ast's
// `[N Node]`) emits `where E : error` so the body may use E as that interface. The clause is a C#
// requirement on the TYPE ARGUMENT, and a pointer type argument is golib's box ж<T>, which no
// interface can be implemented by: Go 1.26's errors.AsType[E error] instantiated at *DNSError (net)
// and *fs.PathError (debug/buildinfo) was CS0311 at both sites, and net alone blocked every remaining
// project of H7 pass 6. AsType never uses E AS an error -- it asserts to E, returns E and allocates a
// *E -- so the clause enforced nothing Go needs; Go's checker had already validated every
// instantiation.
//
// So the clause is emitted only where the body NEEDS it, which is where C# would otherwise refuse
// the body:
//
//   - a method selected on the parameter (`e.Error()`, a method value or expression);
//   - a value of the parameter's type flowing into a NON-EMPTY interface slot (an argument, an
//     assignment or declared variable, a return, a composite element, a channel send, or an
//     interface comparison) -- an empty interface takes any C# value as object;
//   - the parameter instantiating another generic whose own method-set constraint is needed
//     (the callee is asked recursively, over its OWN syntax and types.Info: this file's, or, for a
//     callee in another file or package, the declaring package's as the load carries it -- see
//     genericFuncSource; errors_test's testAsType[E compError] calling errors.AsType[E] was CS0311 at
//     every ж<fs.PathError> argument while that callee counted as unseen).
//
// Anything the analysis cannot see keeps the clause: a function with no body, a method (its type
// parameters belong to the receiver type), a callee whose package the load holds no syntax for. That
// fails safe: a kept clause is today's emission, while a dropped one the body needed would not compile.

type methodSetNeedKey struct {
	fn    *types.Func
	index int
}

// collectFileFuncDecls records the file's function declarations by object, for the recursive callee
// question methodSetConstraintNeeded asks.
func (v *Visitor) collectFileFuncDecls(file *ast.File) {
	v.fileFuncDecls = map[*types.Func]*ast.FuncDecl{}

	for _, decl := range file.Decls {
		if funcDecl, ok := decl.(*ast.FuncDecl); ok {
			if fn, ok := v.info.Defs[funcDecl.Name].(*types.Func); ok {
				v.fileFuncDecls[fn] = funcDecl
			}
		}
	}
}

// The declaring packages' generic function declarations, by object, for callees outside the current
// file: built once per owning package from the syntax the load carries, beside the types.Info that types
// it. An owner with no loaded syntax maps to an empty set, which keeps every clause that reaches it.
var importedGenericFuncDecls = map[*types.Package]map[*types.Func]*ast.FuncDecl{}
var importedGenericFuncInfo = map[*types.Package]*types.Info{}

// genericFuncSource is fn's declaration and the types.Info that types it: this file's own first, then the
// declaring package's syntax as the load carries it (LoadAllSyntax holds the root's and every direct
// dependency's, the handle importedGlobalIsBoxed reads). Nil when there is none -- no syntax for the
// owner, a bodyless declaration, a package the root does not import -- and the caller keeps the clause.
func (v *Visitor) genericFuncSource(fn *types.Func) (*ast.FuncDecl, *types.Info) {
	if decl := v.fileFuncDecls[fn]; decl != nil {
		return decl, v.info
	}

	owner := fn.Pkg()

	if owner == nil || currentLoadedPackage == nil {
		return nil, nil
	}

	packageLock.Lock()
	defer packageLock.Unlock()

	decls, cached := importedGenericFuncDecls[owner]

	if !cached {
		decls = map[*types.Func]*ast.FuncDecl{}

		if loaded := loadedPackageSource(currentLoadedPackage, owner); loaded != nil && loaded.TypesInfo != nil {
			for _, file := range loaded.Syntax {
				for _, decl := range file.Decls {
					if funcDecl, ok := decl.(*ast.FuncDecl); ok && funcDecl.Recv == nil && funcDecl.Type.TypeParams != nil {
						if object, ok := loaded.TypesInfo.Defs[funcDecl.Name].(*types.Func); ok {
							decls[object] = funcDecl
						}
					}
				}
			}

			importedGenericFuncInfo[owner] = loaded.TypesInfo
		}

		importedGenericFuncDecls[owner] = decls
	}

	if decl := decls[fn]; decl != nil {
		return decl, importedGenericFuncInfo[owner]
	}

	return nil, nil
}

// currentGenericFunc is the function whose OWN type parameters signature declares, when that function
// is the declaration being visited; nil for a type, a method, or any other signature.
func (v *Visitor) currentGenericFunc(signature *types.Signature) *types.Func {
	if signature == nil || signature.TypeParams().Len() == 0 || v.currentFuncDecl == nil || v.currentFuncDecl.Recv != nil {
		return nil
	}

	fn, ok := v.info.Defs[v.currentFuncDecl.Name].(*types.Func)

	if !ok || fn.Type() != signature {
		return nil
	}

	return fn
}

// methodSetConstraintNeeded reports whether fn's index'th type parameter must keep its C# method-set
// constraint clause.
func (v *Visitor) methodSetConstraintNeeded(fn *types.Func, index int) bool {
	return v.methodSetConstraintNeededSeen(fn, index, map[methodSetNeedKey]bool{})
}

func (v *Visitor) methodSetConstraintNeededSeen(fn *types.Func, index int, seen map[methodSetNeedKey]bool) bool {
	key := methodSetNeedKey{fn.Origin(), index}

	// A recursive instantiation of the parameter under question (asType calling asType) adds no need
	// of its own: the fixed point is "needed only if something else needs it".
	if seen[key] {
		return false
	}

	seen[key] = true

	decl, info := v.genericFuncSource(key.fn)
	signature, isSignature := key.fn.Type().(*types.Signature)

	if decl == nil || decl.Body == nil || decl.Recv != nil || !isSignature || signature.TypeParams().Len() <= index {
		return true
	}

	typeParam := signature.TypeParams().At(index)
	parents := map[ast.Node]ast.Node{}
	var stack []ast.Node

	ast.Inspect(decl.Body, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return false
		}

		if len(stack) > 0 {
			parents[n] = stack[len(stack)-1]
		}

		stack = append(stack, n)
		return true
	})

	needed := false

	// The SIGNATURE first: a parameter or result naming a generic TYPE instantiated at the parameter
	// (crypto/internal/fips140/ecdsa's `c *Curve[P]`, gc/internal/gen's `Array[T]` result) puts that
	// type's own clause on the signature itself -- C# checks `Curve<P>`'s `where P : Point<P>` wherever
	// `Curve<P>` is named, so dropping the function's clause there is CS0314.
	ast.Inspect(decl.Type, func(n ast.Node) bool {
		if ident, ok := n.(*ast.Ident); ok && !needed {
			if instance, ok := info.Instances[ident]; ok && v.instanceNeedsMethodSet(info, ident, instance, typeParam, seen) {
				needed = true
			}
		}

		return !needed
	})

	if needed {
		return true
	}

	ast.Inspect(decl.Body, func(n ast.Node) bool {
		if needed || n == nil {
			return false
		}

		switch node := n.(type) {
		case *ast.SelectorExpr:
			if selection := info.Selections[node]; selection != nil && (selection.Kind() == types.MethodVal || selection.Kind() == types.MethodExpr) {
				receiver := selection.Recv()

				if pointer, ok := receiver.(*types.Pointer); ok {
					receiver = pointer.Elem()
				}

				if types.Identical(receiver, typeParam) {
					needed = true
					return false
				}
			}
		case *ast.Ident:
			if instance, ok := info.Instances[node]; ok && v.instanceNeedsMethodSet(info, node, instance, typeParam, seen) {
				needed = true
				return false
			}
		}

		if expr, ok := n.(ast.Expr); ok {
			if typeAndValue, ok := info.Types[expr]; ok && typeAndValue.IsValue() && types.Identical(typeAndValue.Type, typeParam) {
				if target := methodSetTargetType(info, expr, parents, signature); target != nil && isNonEmptyMethodInterface(target) {
					needed = true
					return false
				}
			}
		}

		return true
	})

	return needed
}

// instanceNeedsMethodSet reports whether an instantiation that passes typeParam as a type argument
// requires typeParam to satisfy a method-set constraint in C#.
func (v *Visitor) instanceNeedsMethodSet(info *types.Info, ident *ast.Ident, instance types.Instance, typeParam *types.TypeParam, seen map[methodSetNeedKey]bool) bool {
	for j := 0; j < instance.TypeArgs.Len(); j++ {
		if !types.Identical(instance.TypeArgs.At(j), typeParam) {
			continue
		}

		switch object := info.Uses[ident].(type) {
		case *types.Func:
			signature, ok := object.Origin().Type().(*types.Signature)

			if !ok || signature.TypeParams().Len() <= j {
				return true
			}

			if !isNonEmptyMethodInterface(signature.TypeParams().At(j).Constraint()) {
				continue
			}

			if v.methodSetConstraintNeededSeen(object, j, seen) {
				return true
			}
		case *types.TypeName:
			// A generic TYPE keeps every method-set clause (its methods are not analyzed here).
			named, ok := object.Type().(*types.Named)

			if !ok || named.TypeParams().Len() <= j || isNonEmptyMethodInterface(named.TypeParams().At(j).Constraint()) {
				return true
			}
		default:
			return true
		}
	}

	return false
}

// methodSetTargetType is the type a value expression is converted TO by its context, or nil where the
// context converts it to nothing (a define, a statement, an index, a builtin argument).
func methodSetTargetType(info *types.Info, expr ast.Expr, parents map[ast.Node]ast.Node, signature *types.Signature) types.Type {
	var node ast.Node = expr
	parent := parents[node]

	for {
		paren, ok := parent.(*ast.ParenExpr)

		if !ok {
			break
		}

		node, parent = paren, parents[paren]
	}

	switch p := parent.(type) {
	case *ast.CallExpr:
		funType, ok := info.Types[p.Fun]

		if !ok {
			return nil
		}

		if funType.IsType() {
			return funType.Type
		}

		callee, ok := funType.Type.Underlying().(*types.Signature)

		if !ok {
			return nil
		}

		for i, arg := range p.Args {
			if arg != node {
				continue
			}

			params := callee.Params()

			if callee.Variadic() && i >= params.Len()-1 {
				last := params.At(params.Len() - 1).Type()

				if p.Ellipsis.IsValid() {
					return last
				}

				if slice, ok := last.Underlying().(*types.Slice); ok {
					return slice.Elem()
				}

				return nil
			}

			if i < params.Len() {
				return params.At(i).Type()
			}
		}
	case *ast.AssignStmt:
		if p.Tok == token.DEFINE || len(p.Lhs) != len(p.Rhs) {
			return nil
		}

		for i, rhs := range p.Rhs {
			if rhs == node {
				return info.TypeOf(p.Lhs[i])
			}
		}
	case *ast.ValueSpec:
		if p.Type != nil {
			return info.TypeOf(p.Type)
		}
	case *ast.ReturnStmt:
		results := enclosingResults(p, parents, info, signature)

		if results == nil || results.Len() != len(p.Results) {
			return nil
		}

		for i, result := range p.Results {
			if result == node {
				return results.At(i).Type()
			}
		}
	case *ast.CompositeLit:
		return compositeElementType(info.TypeOf(p), p, node, nil)
	case *ast.KeyValueExpr:
		if literal, ok := parents[p].(*ast.CompositeLit); ok {
			return compositeElementType(info.TypeOf(literal), literal, node, p)
		}
	case *ast.SendStmt:
		if p.Value == node {
			if channel, ok := info.TypeOf(p.Chan).Underlying().(*types.Chan); ok {
				return channel.Elem()
			}
		}
	case *ast.BinaryExpr:
		if p.Op == token.EQL || p.Op == token.NEQ {
			other := p.X

			if other == node {
				other = p.Y
			}

			return info.TypeOf(other)
		}
	}

	return nil
}

// enclosingResults is the result tuple of the function literal or declaration a return belongs to.
func enclosingResults(node ast.Node, parents map[ast.Node]ast.Node, info *types.Info, outer *types.Signature) *types.Tuple {
	for n := parents[node]; n != nil; n = parents[n] {
		if literal, ok := n.(*ast.FuncLit); ok {
			if signature, ok := info.TypeOf(literal).(*types.Signature); ok {
				return signature.Results()
			}

			return nil
		}
	}

	return outer.Results()
}

// compositeElementType is the declared type of the element a composite literal holds at expr.
func compositeElementType(literalType types.Type, literal *ast.CompositeLit, expr ast.Node, keyValue *ast.KeyValueExpr) types.Type {
	if literalType == nil {
		return nil
	}

	switch underlying := literalType.Underlying().(type) {
	case *types.Slice:
		return underlying.Elem()
	case *types.Array:
		return underlying.Elem()
	case *types.Map:
		if keyValue != nil && keyValue.Key == expr {
			return underlying.Key()
		}

		return underlying.Elem()
	case *types.Struct:
		if keyValue != nil {
			if key, ok := keyValue.Key.(*ast.Ident); ok {
				for i := 0; i < underlying.NumFields(); i++ {
					if underlying.Field(i).Name() == key.Name {
						return underlying.Field(i).Type()
					}
				}
			}

			return nil
		}

		for i, element := range literal.Elts {
			if element == expr && i < underlying.NumFields() {
				return underlying.Field(i).Type()
			}
		}
	}

	return nil
}

// isNonEmptyMethodInterface reports whether t is an interface with at least one method -- the only
// conversion target that needs a C# interface implementation (an empty interface takes any value as
// object, and a type parameter's own constraint is answered by instanceNeedsMethodSet).
func isNonEmptyMethodInterface(t types.Type) bool {
	if t == nil {
		return false
	}

	if _, isTypeParam := t.(*types.TypeParam); isTypeParam {
		return false
	}

	iface, ok := t.Underlying().(*types.Interface)

	return ok && iface.NumMethods() > 0
}
