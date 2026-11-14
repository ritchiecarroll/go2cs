// funcWordOperations.go - Gbtc
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

// A FUNC WORD is a Go func value carried as a number. Go's func value is one pointer word (to a
// funcval), and C code translated by ccgo (modernc.org/libc, sqlite) stores, compares and calls
// C function pointers as `uintptr`. A managed func value is a delegate, a reference with no address,
// so golib's GoFuncCookie mints the number instead: one word per static function, one per closure
// value (docs/phase4/DESIGN-sqlite-cs-funcptr.md sections 3.1, 3.3 and 3.8). The rule, in four
// directions, is that a func value's storage is its word:
//
//   m1  `(*[2]uintptr)(unsafe.Pointer(&f))[1]` returned by a function with one interface parameter f, a
//       uintptr result and nothing else but local type declarations (ccgo's __ccgo_fp declares
//       `type iface [2]uintptr` first)                             -> (uintptr)GoFuncCookie.Of((Delegate)f)
//   m2  `*(*uintptr)(unsafe.Pointer(&x))` READ, x a func or `S{fn}` with S a one-func-field struct
//                                                                   -> (uintptr)GoFuncCookie.Of(fn)
//   r1  `*(*F)(unsafe.Pointer(&S{x}))`, S a struct whose one field is a uintptr (embedded or named)
//   r2  `(*struct{ f F })(unsafe.Pointer(&S{x})).f`, the same at offset 0
//                                                                   -> GoFuncCookie.Func<F>(x)
//   w1  `*(*uintptr)(unsafe.Pointer(&f)) = x`, f a func variable   -> f = GoFuncCookie.Func<F>(x);
//   w2  `*(*F)(unsafe.Pointer(&v)) = fn`, v a uintptr lvalue        -> v = (uintptr)GoFuncCookie.Of(fn);
//
// Each shape is recognised by its TYPES, never by a name. Anything else keeps today's pointer
// reinterpret: a func stored into memory at a computed address (vfs/patches64.go's offset stores), a
// recovery from a pointer or from memory (runtime's mcleanup and panic), and the `*unsafe.Pointer`
// forms, which already read through unsafe.Pointer.OfFunc.

type funcWordShape struct {
	class    string     // m1, m2, r1, r2, w1 or w2
	value    ast.Expr   // m1, m2, w2: the func value read or stored
	word     ast.Expr   // r1, r2, w1: the word read as a func
	target   ast.Expr   // w1: the func variable; w2: the uintptr lvalue
	funcType types.Type // r1, r2, w1: the func type the word is read as
	viaParam bool       // value is a type parameter's value (m2 over a struct{ f T })
	field    string     // m2 over a one-func-field struct VALUE (not a literal): the field holding the func
}

// packageFuncWords maps each recognised shape's node (the deref, selector, index or assignment) to its
// operands. Rebuilt by collectFuncWords per package beside the value pun reads, for the same reason.
var packageFuncWords map[ast.Node]*funcWordShape

// collectFuncWords is the package pre-pass. It is self-resetting.
func collectFuncWords(files []FileEntry, info *types.Info) {
	packageFuncWords = map[ast.Node]*funcWordShape{}

	for _, fileEntry := range files {
		for _, decl := range fileEntry.file.Decls {
			if funcDecl, ok := decl.(*ast.FuncDecl); ok {
				if index, shape := recognizeInterfaceWordRead(funcDecl, info); shape != nil {
					packageFuncWords[index] = shape
				}
			}
		}

		var stack []ast.Node

		ast.Inspect(fileEntry.file, func(n ast.Node) bool {
			if n == nil {
				stack = stack[:len(stack)-1]
				return true
			}

			stack = append(stack, n)

			switch node := n.(type) {
			case *ast.AssignStmt:
				if shape := recognizeFuncWordStore(node, info); shape != nil {
					packageFuncWords[node] = shape
				}
			case *ast.StarExpr:
				if !isReadPosition(stack) {
					return true
				}

				if shape := recognizeFuncWordDeref(node, info); shape != nil {
					packageFuncWords[node] = shape
				}
			case *ast.SelectorExpr:
				if shape := recognizeFuncWordField(node, info); shape != nil {
					packageFuncWords[node] = shape
				}
			}

			return true
		})
	}
}

// pointerConversionOf matches `(*T)(arg)`, a conversion to a pointer type, and answers T and arg.
func pointerConversionOf(expr ast.Expr, info *types.Info) (types.Type, ast.Expr, bool) {
	conversion, ok := unparenthesize(expr).(*ast.CallExpr)

	if !ok || len(conversion.Args) != 1 {
		return nil, nil, false
	}

	target, ok := info.Types[conversion.Fun]

	if !ok || !target.IsType() {
		return nil, nil, false
	}

	pointer, ok := types.Unalias(target.Type).(*types.Pointer)

	if !ok {
		return nil, nil, false
	}

	return pointer.Elem(), conversion.Args[0], true
}

// unsafePointerOfAddress matches `unsafe.Pointer(&x)` and answers x.
func unsafePointerOfAddress(expr ast.Expr, info *types.Info) (ast.Expr, bool) {
	toUnsafe, ok := unparenthesize(expr).(*ast.CallExpr)

	if !ok || len(toUnsafe.Args) != 1 {
		return nil, false
	}

	if target, ok := info.Types[toUnsafe.Fun]; !ok || !target.IsType() {
		return nil, false
	}

	if basic, ok := types.Unalias(info.TypeOf(toUnsafe)).(*types.Basic); !ok || basic.Kind() != types.UnsafePointer {
		return nil, false
	}

	addressOf, ok := unparenthesize(toUnsafe.Args[0]).(*ast.UnaryExpr)

	if !ok || addressOf.Op != token.AND {
		return nil, false
	}

	return addressOf.X, true
}

func isFuncWordType(t types.Type) bool {
	basic, ok := t.Underlying().(*types.Basic)
	return ok && basic.Kind() == types.Uintptr
}

func isFuncType(t types.Type) bool {
	_, ok := t.Underlying().(*types.Signature)
	return ok
}

// soleField answers the one field of a struct type, or false when it has any other number.
func soleField(t types.Type) (*types.Var, bool) {
	structType, ok := t.Underlying().(*types.Struct)

	if !ok || structType.NumFields() != 1 {
		return nil, false
	}

	return structType.Field(0), true
}

// soleElement answers the one element of a composite literal, through a `key: value` pair.
func soleElement(literal *ast.CompositeLit) (ast.Expr, bool) {
	if len(literal.Elts) != 1 {
		return nil, false
	}

	if pair, ok := literal.Elts[0].(*ast.KeyValueExpr); ok {
		return pair.Value, true
	}

	return literal.Elts[0], true
}

// wordInLiteral matches `S{x}` with S a struct whose one field is a uintptr, and answers x.
func wordInLiteral(operand ast.Expr, info *types.Info) (ast.Expr, bool) {
	literal, ok := unparenthesize(operand).(*ast.CompositeLit)

	if !ok {
		return nil, false
	}

	field, ok := soleField(info.TypeOf(literal))

	if !ok || !isFuncWordType(field.Type()) {
		return nil, false
	}

	return soleElement(literal)
}

// funcValueOf answers the m2 shape for `&operand`: operand itself when it is a func; fn in `S{fn}`, S a
// struct whose one field is a func or a type parameter; or that field of an S value.
func funcValueOf(operand ast.Expr, info *types.Info) (*funcWordShape, bool) {
	operandType := info.TypeOf(operand)

	if operandType == nil {
		return nil, false
	}

	if isFuncType(operandType) {
		return &funcWordShape{class: "m2", value: operand}, true
	}

	field, ok := soleField(operandType)

	if !ok {
		return nil, false
	}

	_, isParam := types.Unalias(field.Type()).(*types.TypeParam)

	if !isParam && !isFuncType(field.Type()) {
		return nil, false
	}

	literal, isLiteral := unparenthesize(operand).(*ast.CompositeLit)

	if !isLiteral {
		return &funcWordShape{class: "m2", value: operand, viaParam: isParam, field: field.Name()}, true
	}

	value, ok := soleElement(literal)
	return &funcWordShape{class: "m2", value: value, viaParam: isParam}, ok
}

// isReadPosition reports whether an expression is used only for its value, judged at the first
// ancestor that is not a parenthesis: `(*(*F)(p))(args)` calls the deref, which reads it.
func isReadPosition(stack []ast.Node) bool {
	child := stack[len(stack)-1]
	i := len(stack) - 2

	for i >= 0 {
		if paren, ok := stack[i].(*ast.ParenExpr); ok {
			child = paren
			i--
			continue
		}

		break
	}

	if i < 0 {
		return true
	}

	switch p := stack[i].(type) {
	case *ast.AssignStmt:
		for _, lhs := range p.Lhs {
			if lhs == child {
				return false
			}
		}
	case *ast.IncDecStmt:
		return false
	case *ast.RangeStmt:
		return p.Key != child && p.Value != child
	case *ast.UnaryExpr:
		return p.Op != token.AND
	}

	return true
}

// recognizeFuncWordDeref matches m2 and r1 at a deref that is read.
func recognizeFuncWordDeref(star *ast.StarExpr, info *types.Info) *funcWordShape {
	elem, arg, ok := pointerConversionOf(star.X, info)

	if !ok {
		return nil
	}

	operand, ok := unsafePointerOfAddress(arg, info)

	if !ok {
		return nil
	}

	if isFuncWordType(elem) {
		if shape, ok := funcValueOf(operand, info); ok {
			return shape
		}

		return nil
	}

	if isFuncType(elem) {
		if word, ok := wordInLiteral(operand, info); ok {
			return &funcWordShape{class: "r1", word: word, funcType: elem}
		}
	}

	return nil
}

// recognizeFuncWordField matches r2: the one func field of a struct pointer converted from a word.
func recognizeFuncWordField(selector *ast.SelectorExpr, info *types.Info) *funcWordShape {
	elem, arg, ok := pointerConversionOf(selector.X, info)

	if !ok {
		return nil
	}

	field, ok := soleField(elem)

	if !ok || field.Name() != selector.Sel.Name || !isFuncType(field.Type()) {
		return nil
	}

	operand, ok := unsafePointerOfAddress(arg, info)

	if !ok {
		return nil
	}

	if word, ok := wordInLiteral(operand, info); ok {
		return &funcWordShape{class: "r2", word: word, funcType: field.Type()}
	}

	return nil
}

// recognizeFuncWordStore matches w1 and w2: a plain one-to-one store through a converted pointer.
func recognizeFuncWordStore(assign *ast.AssignStmt, info *types.Info) *funcWordShape {
	if assign.Tok != token.ASSIGN || len(assign.Lhs) != 1 || len(assign.Rhs) != 1 {
		return nil
	}

	star, ok := unparenthesize(assign.Lhs[0]).(*ast.StarExpr)

	if !ok {
		return nil
	}

	elem, arg, ok := pointerConversionOf(star.X, info)

	if !ok {
		return nil
	}

	operand, ok := unsafePointerOfAddress(arg, info)

	if !ok {
		return nil
	}

	operandType := info.TypeOf(operand)

	if operandType == nil {
		return nil
	}

	if isFuncWordType(elem) && isFuncType(operandType) {
		return &funcWordShape{class: "w1", target: operand, word: assign.Rhs[0], funcType: operandType}
	}

	if isFuncType(elem) && isFuncWordType(operandType) {
		return &funcWordShape{class: "w2", target: operand, value: assign.Rhs[0]}
	}

	return nil
}

// recognizeInterfaceWordRead matches m1: a function with one interface parameter and a uintptr result
// whose whole body returns `(*[2]uintptr)(unsafe.Pointer(&f))[1]`, f that parameter.
func recognizeInterfaceWordRead(funcDecl *ast.FuncDecl, info *types.Info) (*ast.IndexExpr, *funcWordShape) {
	if funcDecl.Body == nil || len(funcDecl.Body.List) == 0 || funcDecl.Recv != nil {
		return nil, nil
	}

	// ccgo's own spelling declares the array type first: `type iface [2]uintptr`, then the return.
	body := funcDecl.Body.List

	for _, stmt := range body[:len(body)-1] {
		decl, ok := stmt.(*ast.DeclStmt)

		if !ok {
			return nil, nil
		}

		if genDecl, ok := decl.Decl.(*ast.GenDecl); !ok || genDecl.Tok != token.TYPE {
			return nil, nil
		}
	}

	funcObj, ok := info.Defs[funcDecl.Name].(*types.Func)

	if !ok {
		return nil, nil
	}

	signature := funcObj.Type().(*types.Signature)

	if signature.Params().Len() != 1 || signature.Results().Len() != 1 || !isFuncWordType(signature.Results().At(0).Type()) {
		return nil, nil
	}

	param := signature.Params().At(0)

	if !types.IsInterface(param.Type()) {
		return nil, nil
	}

	ret, ok := body[len(body)-1].(*ast.ReturnStmt)

	if !ok || len(ret.Results) != 1 {
		return nil, nil
	}

	index, ok := unparenthesize(ret.Results[0]).(*ast.IndexExpr)

	if !ok {
		return nil, nil
	}

	if value := info.Types[index.Index].Value; value == nil || value.String() != "1" {
		return nil, nil
	}

	elem, arg, ok := pointerConversionOf(index.X, info)

	if !ok {
		return nil, nil
	}

	array, ok := elem.Underlying().(*types.Array)

	if !ok || array.Len() != 2 || !isFuncWordType(array.Elem()) {
		return nil, nil
	}

	operand, ok := unsafePointerOfAddress(arg, info)

	if !ok {
		return nil, nil
	}

	ident, ok := unparenthesize(operand).(*ast.Ident)

	if !ok || info.Uses[ident] != param {
		return nil, nil
	}

	return index, &funcWordShape{class: "m1", value: ident}
}

// funcWordValueEmission renders the func value a word is minted from. A declared function takes the
// cached method-group conversion `(F)name` (never `new F(name)`, which mints a fresh delegate per
// evaluation); any other func value is already a delegate.
func (v *Visitor) funcWordValueEmission(shape *funcWordShape) string {
	if shape.class == "m1" {
		return fmt.Sprintf("(Delegate)%s", v.convExpr(shape.value, nil))
	}

	value := v.convExpr(shape.value, nil)

	if shape.field != "" {
		value += "." + getSanitizedIdentifier(shape.field)
	}

	if shape.viaParam {
		return fmt.Sprintf("(Delegate?)(object?)%s", value)
	}

	if shape.field != "" {
		return value
	}

	var named types.Object

	switch expr := unparenthesize(shape.value).(type) {
	case *ast.Ident:
		named = v.info.Uses[expr]
	case *ast.SelectorExpr:
		named = v.info.Uses[expr.Sel]
	}

	if _, isFunc := named.(*types.Func); isFunc {
		return fmt.Sprintf("(%s)%s", v.getCSharpTypeName(v.info.TypeOf(shape.value)), value)
	}

	return value
}

// funcWordEmission renders a recognised read (m1, m2, r1, r2), reporting false otherwise.
func (v *Visitor) funcWordEmission(node ast.Node) (string, bool) {
	shape := packageFuncWords[node]

	if shape == nil {
		return "", false
	}

	switch shape.class {
	case "m1", "m2":
		return fmt.Sprintf("(uintptr)GoFuncCookie.Of(%s)", v.funcWordValueEmission(shape)), true
	case "r1", "r2":
		return fmt.Sprintf("GoFuncCookie.Func<%s>(%s)", v.getCSharpTypeName(shape.funcType), v.convExpr(shape.word, nil)), true
	}

	return "", false
}

// funcWordStoreEmission renders a recognised store (w1, w2) as a statement, reporting false otherwise.
func (v *Visitor) funcWordStoreEmission(assign *ast.AssignStmt) (string, bool) {
	shape := packageFuncWords[assign]

	if shape == nil {
		return "", false
	}

	switch shape.class {
	case "w1":
		return fmt.Sprintf("%s = GoFuncCookie.Func<%s>(%s);", v.convExpr(shape.target, nil), v.getCSharpTypeName(shape.funcType), v.convExpr(shape.word, nil)), true
	case "w2":
		return fmt.Sprintf("%s = (uintptr)GoFuncCookie.Of(%s);", v.convExpr(shape.target, nil), v.funcWordValueEmission(shape)), true
	}

	return "", false
}
