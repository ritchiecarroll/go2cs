// typePredicates.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the converter's QUESTIONS ABOUT TYPES — the yes/no and small-answer predicates
// the emitters consult before deciding what shape of C# to write.
//
// They cluster into families: is this a string / an interface (and is it empty) / a pointer / a
// dynamic interface; are these parameters interfaces or pointers; is this expression a C#
// compile-time constant. Most come in a PAIR — one taking an ast.Expr (which needs the Visitor to
// resolve it through go/types) and one taking a types.Type directly — because some callers hold
// syntax and others hold a resolved type.
//
// Nothing here emits C#; each answer merely steers a caller that does. The type-name RENDERING
// these predicates guard lives in typeNameResolution.go.

package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"strings"
)

func (v *Visitor) getStringLiteral(str string) (result string, isRawStr bool) {
	// Convert Go raw string literal to C# raw string literal
	if strings.HasPrefix(str, "`") {
		// Remove backticks from the start and end of the string
		str = strings.Trim(str, "`")

		// See if raw string literal is required (contains newline)
		if strings.Contains(str, "\n") {
			// C# raw string literals are enclosed in triple (or more) quotes
			prefix := `"""`
			suffix := `"""`

			// Keep adding quotes until the source string does not contain the
			// prefix to create a unique C# raw string literal token
			for while := strings.Contains(str, prefix); while; {
				prefix += `"`
				suffix += `"`
				while = strings.Contains(str, prefix)
			}

			// Multiline C# raw string literals start and end with newlines
			prefix += v.newline
			suffix = v.newline + suffix

			return prefix + str + suffix, true
		}

		// Use C# verbatim string literal for more simple raw strings
		return fmt.Sprintf("@\"%s\"", strings.ReplaceAll(str, "\"", "\"\"")), true
	}

	return str, false
}

func (v *Visitor) isNonCallValue(expr ast.Expr) bool {
	_, isCallExpr := expr.(*ast.CallExpr)

	// Get the type and value information
	tv, ok := v.info.Types[expr]

	if !ok {
		return false
	}

	return tv.IsValue() && !isStringLiteral(tv) && !isCallExpr
}

// isCSharpConstantExpr reports whether the expression renders as a C# compile-time constant, and
// so may be used as the operand of a relational/constant pattern (`x is <op> Y`). Literals always
// qualify; a const reference qualifies only when it is emitted as a C# `const` — i.e. a concrete
// (non-untyped) basic type. A variable, or a const emitted as `static readonly` (untyped/named,
// see visitValueSpec), does not, and the caller must use a `when` guard instead (avoids CS9135).
func (v *Visitor) isCSharpConstantExpr(expr ast.Expr) bool {
	// A constant expression whose CONTEXTUAL type is a wrapper STRUCT — the golib uintptr
	// (golib/uintptr.cs) or ANY named numeric (`[GoType("num:…")]`, time's Duration) — can
	// NEVER be a C# constant: wrapper structs have no constant form, so no constant/relational
	// pattern can compare against them (CS9135). Even a plain literal adopts the tag's or the
	// comparand's type in context (`case 4:` under a uintptr tag; `d is >= 0` typing 0 as
	// Duration — time Abs/round ×2). Force the when-guard/`==` fallback for the whole class.
	if tv, ok := v.info.Types[expr]; ok && tv.Type != nil {
		if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Kind() == types.Uintptr {
			return false
		}

		if named, ok := types.Unalias(tv.Type).(*types.Named); ok {
			if _, isBasic := named.Underlying().(*types.Basic); isBasic {
				return false
			}
		}
	}

	switch e := expr.(type) {
	case *ast.BasicLit:
		return true
	case *ast.ParenExpr:
		return v.isCSharpConstantExpr(e.X)
	case *ast.Ident:
		return v.isCSharpConstObject(v.info.ObjectOf(e))
	case *ast.SelectorExpr:
		return v.isCSharpConstObject(v.info.ObjectOf(e.Sel))
	}

	return false
}

func (v *Visitor) isCSharpConstObject(obj types.Object) bool {
	constObj, ok := obj.(*types.Const)

	if !ok {
		return false
	}

	basic, ok := constObj.Type().(*types.Basic)

	// A named-type const, or an untyped const (emitted as an Untyped* wrapper / GoBigConst), is
	// `static readonly`, not a C# `const`.
	return ok && basic.Info()&types.IsUntyped == 0
}

// isStringType determines if an expression is either a string literal or a string variable
func (v *Visitor) isStringType(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.BasicLit:
		// Direct string literal
		return e.Kind == token.STRING

	case *ast.BinaryExpr:
		// Handle string concatenation
		if e.Op != token.ADD {
			return false
		}

		// Both sides must be string types for the result to be a string
		return v.isStringType(e.X) && v.isStringType(e.Y)

	case *ast.Ident, *ast.SelectorExpr:
		// Variable or field access - check type info
		tv, ok := v.info.Types[expr]

		if !ok {
			return false
		}

		return isStringType(tv.Type)

	case *ast.IndexExpr, *ast.SliceExpr:
		// Slice expressions are not string literals or variables
		return false

	case *ast.CallExpr:
		// For function calls, check the return type
		tv, ok := v.info.Types[expr]

		if !ok {
			return false
		}

		return isStringType(tv.Type)

	case *ast.ParenExpr:
		// Handle parenthesized expressions
		return v.isStringType(e.X)
	}

	// For any other expression type, use type information
	tv, ok := v.info.Types[expr]

	if !ok {
		return false
	}

	return isStringType(tv.Type)
}

// isStringType checks if a type is a string type
func isStringType(t types.Type) bool {
	if t == nil {
		return false
	}

	// Handle basic types
	if basic, ok := t.Underlying().(*types.Basic); ok {
		return basic.Kind() == types.String
	}

	return false
}

// isStringLiteral specifically checks if the expression is a string literal (not a variable)
func isStringLiteral(tv types.TypeAndValue) bool {
	// Must be a constant value
	if !tv.IsValue() || tv.Value == nil {
		return false
	}

	// Must be a string constant
	if tv.Value.Kind() != constant.String {
		return false
	}

	// Type must be string
	return isStringType(tv.Type)
}

func (v *Visitor) isInterface(ident *ast.Ident) (result bool, empty bool) {
	obj := v.info.ObjectOf(ident)

	if obj == nil {
		return false, false
	}

	return isInterface(obj.Type())
}

func isInterface(t types.Type) (result bool, empty bool) {
	exprType := t.Underlying()

	if interfaceType, ok := exprType.(*types.Interface); ok {
		// Empty interface has zero methods
		return true, interfaceType.NumMethods() == 0
	}

	return false, false
}

// isEmptyInterfaceTarget reports whether a declared target type is the plain EMPTY interface
// (`any` / `interface{}`) — the case a string-literal value must box through @string. A TYPE
// PARAMETER is excluded even though its underlying constraint is an interface (a `~string`-
// constrained parameter takes the literal directly, not an object box).
func isEmptyInterfaceTarget(t types.Type) bool {
	if t == nil {
		return false
	}

	if _, isTypeParam := types.Unalias(t).(*types.TypeParam); isTypeParam {
		return false
	}

	isIface, isEmpty := isInterface(t)

	return isIface && isEmpty
}

// variadicArgBindsParamsCollection reports whether the SOLE argument of a non-spread VARIADIC slot
// would bind the callee's `params Span<E>` collection WHOLE — the call's NORMAL form — instead of
// contributing ONE element, which is what Go's semantics require (spreading needs an explicit
// `a...`, and a bare `a` is a single value).
//
// The shape is a Go slice or array whose ELEMENT type IS the variadic element type: `f(a)` with
// `a []any` against `func f(args ...any)` — html/template's jsValEscaper, and its
// `jsValEscaper([]any{x})` nesting cases. It emits `slice<any>` / `array<any>` against
// `params ꓸꓸꓸany argsʗp`, where `using ꓸꓸꓸany = Span<any>`.
//
// Why it is a C# 14 REGRESSION and not a standing defect: golib gives `slice<T>` and `array<T>` an
// `implicit operator T[]` (slice.cs, array.cs), and C# 14 promotes array→span from a user-defined
// conversion to a STANDARD one. A user-defined conversion admits one standard conversion on each
// side, so `slice<any>` → `any[]` → `Span<any>` becomes an implicit conversion that did NOT exist
// in C# 13 (where the second hop was itself user-defined, and C# never composes two). That makes
// the callee applicable in its normal form, and C# prefers the normal form over the expanded one —
// so the slice silently becomes the entire argument list and exactly one level of nesting
// disappears. It is a SILENT divergence, not a compile error: the same trap the untyped-`nil`
// variadic cast in convCallExpr.go already guards, reached by a different conversion.
//
// A NAMED slice/array type is excluded deliberately: it renders as its own generated wrapper whose
// only route to a span is wrapper→`slice<T>`→`T[]`→span, i.e. TWO user-defined conversions, which
// C# 14 still does not compose. Only the unnamed slice/array shapes can reach the params collection.
//
// The cast this gates is always legal where the call already compiled: binding the expanded form at
// all required an implicit conversion from the argument to the element type, so making that
// conversion explicit cannot fail where the implicit one succeeded. (In practice the rule can only
// fire for an INTERFACE element type — Go assignability of `[]E` to `E` demands it — which is why
// the emitted cast is `(any)`.)
func variadicArgBindsParamsCollection(argType types.Type, elemType types.Type) bool {
	if argType == nil || elemType == nil {
		return false
	}

	var argElem types.Type

	switch argKind := types.Unalias(argType).(type) {
	case *types.Slice:
		argElem = argKind.Elem()
	case *types.Array:
		argElem = argKind.Elem()
	default:
		return false
	}

	return types.Identical(types.Unalias(argElem), types.Unalias(elemType))
}

func isEmptyInterface(interfaceType *ast.InterfaceType) bool {
	if interfaceType == nil {
		return false
	}

	// Empty interface has no methods
	return len(interfaceType.Methods.List) == 0
}

func (v *Visitor) isDynamicInterface(expr ast.Expr) bool {
	return isDynamicInterface(v.getType(expr, false))
}

func isDynamicInterface(t types.Type) bool {
	if t == nil {
		return false
	}

	// If it's a pointer, get its element.
	if ptr, ok := t.(*types.Pointer); ok {
		t = ptr.Elem()
	}

	// If it's a named type, then it’s not dynamic.
	if _, ok := t.(*types.Named); ok {
		return false
	}

	// Finally, check if it is a direct interface.
	_, ok := t.(*types.Interface)

	return ok
}

// extractInterfaceType is extractStructType's twin for a non-empty anonymous INTERFACE literal
// reached through the same type composition. The resolved type is the LITERAL's, not the outer
// expression's — the lift names the interface itself.
func (v *Visitor) extractInterfaceType(expr ast.Expr) (*ast.InterfaceType, types.Type) {
	found := firstAnonymousTypeLiteral(typeSyntaxOf(expr), func(candidate ast.Expr) bool {
		interfaceType, isInterface := candidate.(*ast.InterfaceType)
		return isInterface && !isEmptyInterface(interfaceType)
	})

	if found == nil {
		return nil, nil
	}

	return found.(*ast.InterfaceType), v.getType(found, false)
}

func (v *Visitor) isPointer(ident *ast.Ident) bool {
	obj := v.info.ObjectOf(ident)

	if obj == nil {
		return false
	}

	return isPointer(obj.Type())
}

// isUnsafePointer reports whether t is `unsafe.Pointer` — a go/types BASIC of kind UnsafePointer,
// not a *types.Pointer, which is why isPointer below has to name it separately. Its C# form is
// golib's `Pointer : ж<uintptr>`, a box whose VALUE is the address it carries.
func isUnsafePointer(t types.Type) bool {
	if t == nil {
		return false
	}

	basic, ok := t.Underlying().(*types.Basic)

	return ok && basic.Kind() == types.UnsafePointer
}

func isPointer(t types.Type) bool {
	exprType := t.Underlying()

	_, isPointer := exprType.(*types.Pointer)

	// Also check for an unsafe.Pointer
	if !isPointer {
		if basic, ok := t.(*types.Basic); ok {
			isPointer = basic.Kind() == types.UnsafePointer
		}
	}

	return isPointer
}

func (v *Visitor) isPointerReceiver() (bool, string) {
	// First check if we're in a function with a receiver
	if !v.inFunction || v.currentFuncSignature.Recv() == nil {
		return false, ""
	}

	// Check if receiver is a pointer type
	recvType := v.currentFuncSignature.Recv().Type()
	isRecvPointer := false

	if _, ok := recvType.(*types.Pointer); ok {
		isRecvPointer = true
	}

	if !isRecvPointer {
		return false, ""
	}

	// Get the name of the receiver variable from the AST
	var recvName string

	if v.currentFuncDecl.Recv != nil && len(v.currentFuncDecl.Recv.List) > 0 {
		// The field might have multiple names for the same type,
		// but for a receiver there should be just one
		if len(v.currentFuncDecl.Recv.List[0].Names) > 0 {
			recvName = v.currentFuncDecl.Recv.List[0].Names[0].Name
		}
	}

	return true, recvName
}

func paramsAreInterfaces(paramTypes *types.Tuple, andNotEmptyInterface bool) []bool {
	if paramTypes == nil {
		return nil
	}

	paramIsInterface := make([]bool, paramTypes.Len())

	for i := 0; i < paramTypes.Len(); i++ {
		param := paramTypes.At(i)
		paramType := param.Type()
		isInterface, isEmpty := isInterface(paramType)

		if andNotEmptyInterface {
			paramIsInterface[i] = isInterface && !isEmpty
		} else {
			paramIsInterface[i] = isInterface
		}
	}

	return paramIsInterface
}

func (v *Visitor) paramsArePointers(paramTypes *types.Tuple) []bool {
	if paramTypes == nil {
		return nil
	}

	paramIsPointer := make([]bool, paramTypes.Len())

	for i := 0; i < paramTypes.Len(); i++ {
		param := paramTypes.At(i)
		// An ERASED pointer-core type parameter result (`func f[P *T, T any](…) P`) is a pointer
		// result type too — `return p` must yield the box (paramPointerType sees the erasure;
		// unsafe.Pointer stays covered by isPointer alone, and stays excluded from box renders
		// by the direct-pointer check at the use site).
		_, isErased := v.paramPointerType(param.Type())
		paramIsPointer[i] = isPointer(param.Type()) || isErased
	}

	return paramIsPointer
}

// isLocalImplType reports whether the impl type for an interface implementation (GoImplement) is
// declared in the package currently being converted. A GoImplement attribute is realized by the
// partial-struct generator, which can only add an interface to a type defined in the SAME assembly;
// an impl type imported from another package must therefore NOT be recorded here. That relationship
// is already established in the impl type's own package (e.g. color.RGBA's `Color` implementation
// lives in the image/color assembly), so re-emitting it in a consumer such as image/color/palette
// only generates a broken cross-assembly partial (CS1929/CS0034). Pointer types are unwrapped, and
// lifted/anonymous types (always local) are treated as local.
func (v *Visitor) isLocalImplType(t types.Type) bool {
	if _, ok := v.liftedTypeMap[t]; ok {
		return true
	}

	if pointer, ok := t.(*types.Pointer); ok {
		return v.isLocalImplType(pointer.Elem())
	}

	if named, ok := t.(*types.Named); ok {
		pkg := named.Obj().Pkg()
		return pkg != nil && pkg == v.pkg
	}

	return false
}

// isSameAssemblyPkg reports whether pkg's converted C# compiles into the SAME assembly as the
// package currently being converted. Ordinarily that is just v.pkg, but under -tests an
// EXTERNAL test variant (package <name>_test) RECOMPILES the production sources into the test
// assembly (TestingInfrastructureRequirements §2.1/§4.2), so the package under test is
// same-assembly even though it is a different Go package. Adapter naming keys off this
// distinction: the go2cs-gen generators decide "foreign" by CONTAINING ASSEMBLY (a local
// declaration resolves), so the converter's cast-site references must agree or the two compose
// different adapter class names (B4/B5 — `strings_BuilderжWriter` referenced,
// `BuilderжWriter` generated).
func (v *Visitor) isSameAssemblyPkg(pkg *types.Package) bool {
	if pkg == v.pkg {
		return true
	}

	return v.options.testPackagePath != "" && pkg != nil && pkg.Path() == v.options.testPackagePath
}
