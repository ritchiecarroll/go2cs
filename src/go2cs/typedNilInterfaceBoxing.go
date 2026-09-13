// typedNilInterfaceBoxing.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns TWO properties, applied at ONE kind of place. The first:
//
//	A Go POINTER entering INTERFACE space is represented by its pointer BOX, carrying its static
//	pointee type — however the pointer was produced.
//
// Go's interface value is a (dynamic type, value) pair, so a pointer in it is never merely an
// address: `any((*T)(nil))` is a NON-nil interface whose %T prints `*T` and whose `x.(*T)` assert
// succeeds with a nil result. Two managed renderings break that, and both are silent:
//
//   - a bare `null` reference boxes as nothing at all — the type is gone, `x == nil` reports Go's
//     false as true, the assert fails, and the reflection bridge sees no descriptor; and
//   - a DEREF-aliased pointer (`ref var p = ref Ꮡp.DerefOrNull()`) rendered by its value alias
//     boxes a COPY of the POINTEE — dynamic type `T` where Go says `*T`, pointer identity lost,
//     and a nil pointer panics at the box rather than crossing intact.
//
// A NON-empty interface target is already correct and takes nothing from here: its conversion goes
// through a generated adapter which holds the box and null-coalesces it to the canonical typed nil
// instance (go2cs-gen's AdapterImplTemplate). The EMPTY interface (`any`) has no adapter — C# boxes
// the value directly — so the treatment has to be emitted, and this is where.
//
// It is applied at the BOUNDARY rather than at the sites that mint nil, because those sites are
// unbounded and mostly not emissions at all: `var p *T` is one, but so is a struct's zero value, a
// `make([]*T, n)` element and a map miss, none of which the converter writes. The boundary is
// finite — the same set of slots boxUntypedConstAsDefaultType already serves, for the same reason
// (a value's Go dynamic type must survive being boxed).
//
// The scope is a genuine `*T` (a `types.Pointer`). `unsafe.Pointer` is a Basic and renders as a
// struct, and a NAMED pointer type renders as its generated wrapper struct — neither can be a null
// reference, so neither has anything to carry.
//
// A SECOND value shape loses its Go type at this same boundary, for a different reason, so it is
// owned here too:
//
//	A Go VARIADIC FUNC entering EMPTY-INTERFACE space is represented by its Go func type.
//
// C# gives a method group or lambda at an untyped destination a NATURAL function type. For a
// non-variadic signature that is `Func<…>`/`Action<…>` — exactly go2cs's own lowering, so the two
// already agree and nothing is emitted. A `params` signature has no BCL delegate, so C# SYNTHESIZES
// one (`<>f__AnonymousDelegate0`) and the box carries that compiler artifact as its dynamic type,
// while every Go-visible spelling of the same func — a type assertion, a type switch,
// `reflect.TypeOf` — names golib's `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ` family. html/template's `funcMap`
// (`map[string]any` of `func(...any) string` escapers) is the corpus instance: its own test's
// `funcMap[n].(func(...any) string)` threw `interface conversion: interface {} is
// <>f__AnonymousDelegate0, not go.Funcꓸꓸꓸ<object, @string>`. The cast is a no-op wherever the value
// already has that type, so it widens nothing; it pins the dynamic type at the one place that
// records it. A NON-empty interface target needs nothing here either — no Go interface but the
// empty one can be satisfied by a bare func type (a func type has no methods), so the adapter
// route never sees this shape.
//
// The two treatments are MUTUALLY EXCLUSIVE — a value is a pointer or a func, never both — which is
// why they share the boundary's entry points rather than needing a second set.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
)

// pointerBoxesIntoEmptyInterface reports whether `value` is a Go pointer being emitted into an
// EMPTY-interface slot — the condition for BOTH halves of the boundary treatment (rendering the
// pointer as its box, and carrying the type of a nil one).
func (v *Visitor) pointerBoxesIntoEmptyInterface(target types.Type, value ast.Expr) bool {
	if value == nil || !isEmptyInterfaceTarget(target) {
		return false
	}

	_, isPointer := v.getType(value, false).(*types.Pointer)

	return isPointer
}

// boxPointerIntoEmptyInterface applies this file's boundary treatment to an already-rendered value
// bound for an EMPTY-interface slot: a POINTER gains the typed-nil accessor, so a null reference
// crosses as the canonical typed nil instance instead of as an untyped C# null; a VARIADIC FUNC
// gains the cast to its Go func type. A no-op for every other slot, for every other value shape, and
// for a pointer whose rendering provably cannot be null. (The name predates the second treatment and
// is kept so the ten call sites stay untouched; see the file header for both properties.)
func (v *Visitor) boxPointerIntoEmptyInterface(target types.Type, value ast.Expr, rendered string) string {
	if !isEmptyInterfaceTarget(target) || value == nil {
		return rendered
	}

	return v.applyTypedNilPointerBox(value, rendered)
}

// applyTypedNilPointerBox is boxPointerIntoEmptyInterface for a caller that has already established
// its slot is an empty interface (the twin of applyUntypedConstBoxCast / boxUntypedConstAsDefaultType).
// A pointer whose rendering provably cannot be null passes through unchanged; a non-pointer value
// takes the variadic-func arm, which is itself a no-op for every value shape but that one.
func (v *Visitor) applyTypedNilPointerBox(value ast.Expr, rendered string) string {
	if rendered == "" || value == nil {
		return rendered
	}

	valueType := v.getType(value, false)

	if _, isPointer := valueType.(*types.Pointer); !isPointer {
		// The sibling treatments this file owns, at the same boundary and mutually exclusive
		// with the pointer one: a VARIADIC func value must carry its Go func type, or C#'s
		// synthesized natural delegate becomes the box's dynamic type; and a func value that can
		// be NULL must carry its type through a carrier, since the cast is erased at the box.
		// Cast first, then carry — the cast is what makes the carrier's type word the Go one.
		// See the file header.
		return v.applyTypedNilFuncBox(valueType, v.funcExprNeverRendersNull(value), v.applyVariadicFuncBoxCast(valueType, rendered))
	}

	if v.pointerExprNeverRendersNull(value) {
		return rendered
	}

	return rendered + "." + TypedNilBoxAccessor
}

// applyTypedNilPointerBoxToType is applyTypedNilPointerBox for a caller that holds the value's TYPE
// but has no AST expression for it: a tuple element DECONSTRUCTED from a multi-value call, which is
// a temporary the converter minted rather than a node the Go source wrote. Same boundary, same two
// mutually-exclusive treatments; only the pointer arm's null test differs, and here it has one
// answer — pointerExprNeverRendersNull recognizes `&x`, `new(T)` and a nil conversion, and a call
// result is none of them — so a pointer element always takes the accessor.
//
// The caller this exists for is visitReturnStmt's forwarded multi-value return. Its EMPTY-interface
// elements must not go through convertToInterfaceType: `any` has no adapter to hold the box, so that
// route falls through to the pointer-DEREF prefix and boxes a COPY OF THE POINTEE — dynamic type `T`
// where Go says `*T`. That is this file's first property, reached by a path that had no boundary
// treatment on it (crypto/x509 parsePublicKey / parsePKCS8PrivateKey forwarding
// `ecdh.X25519().NewPublicKey(der)` into `(any, error)`: `%T`, reflect.TypeOf and every
// `case *ecdh.PublicKey` type-switch arm disagreed with Go).
func (v *Visitor) applyTypedNilPointerBoxToType(valueType types.Type, rendered string) string {
	if rendered == "" || valueType == nil {
		return rendered
	}

	if _, isPointer := valueType.(*types.Pointer); !isPointer {
		// No AST expression to test, so the func arm takes its conservative answer — a tuple
		// element deconstructed from a call result is exactly the shape that can be nil, and it is
		// neither a literal nor a method group.
		return v.applyTypedNilFuncBox(valueType, false, v.applyVariadicFuncBoxCast(valueType, rendered))
	}

	return rendered + "." + TypedNilBoxAccessor
}

// applyVariadicFuncBoxCast casts an already-rendered VARIADIC func value to its Go func type's C#
// delegate so the empty-interface box carries the type Go names rather than the anonymous delegate
// C# synthesizes for a `params` method group or literal. A no-op for every other value shape, and
// for a value that already has the type (the cast is then redundant, never wrong).
//
// getCSharpTypeName renders a func type STRUCTURALLY through iifeDelegateType — the same function
// convTypeAssertExpr renders the assertion's target type with — so the boxing side and the reading
// side of a `x.(func(…))` are composed by one renderer and cannot drift.
func (v *Visitor) applyVariadicFuncBoxCast(valueType types.Type, rendered string) string {
	castType := v.variadicFuncBoxCastType(valueType)

	if castType == "" {
		return rendered
	}

	return fmt.Sprintf("((%s)(%s))", castType, rendered)
}

// variadicFuncBoxCastType names the delegate a value of valueType must be cast to when boxed into an
// EMPTY-interface slot, or "" when the value is not a variadic func. It is the answer for callers
// that carry a per-element CAST (composite literals' castArgToType) rather than a rendered string,
// so both consumers ask one function and the two slots cannot disagree.
func (v *Visitor) variadicFuncBoxCastType(valueType types.Type) string {
	if valueType == nil {
		return ""
	}

	sig, ok := valueType.Underlying().(*types.Signature)

	if !ok || !sig.Variadic() || sig.Params().Len() == 0 {
		return ""
	}

	return v.getCSharpTypeName(valueType)
}

// emptyInterfacePointerContexts returns `contexts` with the ident context's isPointer set when
// `value` is a pointer bound for an empty-interface slot, so a deref-aliased pointer renders as its
// BOX (`Ꮡp`) rather than as the value alias whose boxing would capture a copy of the pointee. The
// flag is inert for a pointer that already holds its own box (a plain local), so this only ever
// moves the aliased shapes.
func (v *Visitor) emptyInterfacePointerContexts(target types.Type, value ast.Expr, contexts []ExprContext) []ExprContext {
	if !v.pointerBoxesIntoEmptyInterface(target, value) {
		return contexts
	}

	result := make([]ExprContext, 0, len(contexts)+1)
	found := false

	for _, context := range contexts {
		if identContext, ok := context.(IdentContext); ok {
			identContext.isPointer = true
			result = append(result, identContext)
			found = true
			continue
		}

		result = append(result, context)
	}

	if !found {
		identContext := DefaultIdentContext()
		identContext.isPointer = true
		result = append(result, identContext)
	}

	return result
}

// pointerExprNeverRendersNull reports whether a pointer expression's RENDERING can never be a bare
// null reference, so the typed-nil accessor would be dead weight. Decided from the AST, not from
// the rendered text: an address-of (`&x`, including of a composite literal), a `new(T)`, and a
// nil→pointer CONVERSION (`(*T)(nil)`, which already renders the canonical instance) are the shapes
// that qualify. Everything else — an identifier, a field, an element, a call result — may be nil and
// is wrapped.
func (v *Visitor) pointerExprNeverRendersNull(expr ast.Expr) bool {
	switch expr := expr.(type) {
	case *ast.ParenExpr:
		return v.pointerExprNeverRendersNull(expr.X)
	case *ast.UnaryExpr:
		return expr.Op == token.AND
	case *ast.CallExpr:
		if ident, ok := expr.Fun.(*ast.Ident); ok && ident.Name == "new" && v.identIsUniverseBuiltin(ident) {
			return true
		}

		// A pointer CONVERSION is exactly as null-able as its operand: `(*T)(nil)` already renders
		// the canonical instance, and `(*Buffer)(&b)` (log/slog's buffer pool) can no more be null
		// than the address it converts. Anything else — a conversion of an identifier, of an
		// unsafe.Pointer, of a call result — stays conservative and takes the accessor.
		if len(expr.Args) == 1 {
			if isConversion, _ := v.isTypeConversion(expr); isConversion {
				if tv, ok := v.info.Types[expr.Args[0]]; ok && tv.IsNil() {
					return true
				}

				return v.pointerExprNeverRendersNull(expr.Args[0])
			}
		}
	}

	return false
}

// applyTypedNilFuncBox carries a FUNC value's Go type across the empty-interface boundary when the
// value can be null. It is the delegate-shaped sibling of the pointer arm's TypedNilBoxAccessor, and
// it exists because a CAST is not a value: `ж<T>` holds its pointee type so boxing it keeps the type,
// while `(object)(Action)null` is simply `null`. The variadic cast this boundary already applies
// therefore pins the dynamic type only for a NON-null func — which is why this runs AFTER it, over
// the cast's result, so the carrier's type word is the Go func type rather than C#'s natural one.
//
// A no-op for every value shape but a func, and for a func that provably cannot be null.
func (v *Visitor) applyTypedNilFuncBox(valueType types.Type, neverNull bool, rendered string) string {
	if rendered == "" || valueType == nil || neverNull {
		return rendered
	}

	if _, isFunc := valueType.Underlying().(*types.Signature); !isFunc {
		return rendered
	}

	// PARENTHESIZED, unlike the pointer arm's append. Member access binds tighter than a cast in C#,
	// so `(Action)(default!).OrTypedNilFunc()` parses as `(Action)((default!).OrTypedNilFunc())` —
	// the cast lands on the accessor's RESULT and the operand it was meant to type is bare `default!`.
	// The pointer arm never meets this because its renderings arrive already parenthesized
	// (`((ж<T>)nil)`); a func's nil CONVERSION does not, and that is exactly the shape this arm
	// exists for. Measured on the emitted C#, not reasoned about.
	return "(" + rendered + ")." + TypedNilFuncAccessor
}

// funcExprNeverRendersNull reports whether a func-typed expression's RENDERING provably cannot be a
// null delegate — the func arm's twin of pointerExprNeverRendersNull, and deliberately narrower.
//
// The shapes that qualify are the ones the corpus is almost entirely made of (censused at the
// boundary, 2026-09-01): a func LITERAL, which allocates a delegate, and a METHOD GROUP — an
// identifier naming a declared func, or a SELECTOR naming one through its package or receiver
// (`fmt.Sprint`). All are non-null by construction, and exempting them is what holds this change's
// footprint to the sites that can actually be nil.
//
// ⚠ The SELECTOR arm was missing from the first cut, and the census inherited the omission because
// the sizing probe reused this predicate: `text/template`'s `fmt.Sprint`/`Sprintf`/`Sprintln` were
// counted as nullable and reported as a 3-site production blast radius. They are method groups and
// can never be null. With the arm present the production radius is ZERO — every func the corpus
// boxes into `any` is provably non-null — and the treatment reaches only the test dimension, where
// typed nils are written deliberately. A predicate that a census is keyed on is a predicate whose
// gaps the census cannot see.
//
// A nil CONVERSION is NOT exempt, and that is the one place this differs from the pointer twin.
// `(*T)(nil)` already renders the canonical typed-nil box, so the pointer arm can pass it through;
// `(func())(nil)` renders `(Action)(default!)` — a cast of null, i.e. precisely the shape that loses
// the type word — so it needs the carrier like any other nullable func. Reading that difference off
// the pointer arm rather than measuring it would have exempted the one expression TestMapOf uses.
func (v *Visitor) funcExprNeverRendersNull(expr ast.Expr) bool {
	switch expr := expr.(type) {
	case *ast.ParenExpr:
		return v.funcExprNeverRendersNull(expr.X)
	case *ast.FuncLit:
		return true
	case *ast.Ident:
		_, isFunc := v.info.Uses[expr].(*types.Func)

		return isFunc
	case *ast.SelectorExpr:
		// A QUALIFIED method group — `fmt.Sprint`, or a method value read off a receiver. The
		// selection's object is what decides it, not the expression's shape: `x.f` where f is a
		// FIELD of func type is an ordinary nullable value and must fall through.
		if selection, ok := v.info.Selections[expr]; ok {
			_, isFunc := selection.Obj().(*types.Func)

			return isFunc
		}

		// No selection record: a package-qualified reference (`fmt.Sprint`), whose object is
		// recorded against the selector's own identifier.
		_, isFunc := v.info.Uses[expr.Sel].(*types.Func)

		return isFunc
	case *ast.CallExpr:
		// A func CONVERSION is exactly as nullable as its operand — the pointer twin's own words for
		// the same shape. `Doubler(func(w int) int { … })` renders as a delegate CONSTRUCTION and can
		// no more be null than the literal it wraps (tests/Behavioral/MethodlessFuncTypeAssert is the
		// corpus instance, and it is where this arm was found missing).
		//
		// The DIFFERENCE from the pointer twin, and it is the whole reason this file exists: a NIL
		// conversion is not exempt here. `(*T)(nil)` already renders the canonical typed-nil box, so
		// the pointer arm passes it through; `(func())(nil)` renders `(Action)(default!)` — a cast of
		// null — which is precisely the shape that loses the type word. Recursing into the operand
		// gives that for free: a nil operand is not provably non-null, so it takes the carrier.
		if len(expr.Args) == 1 {
			if isConversion, _ := v.isTypeConversion(expr); isConversion {
				return v.funcExprNeverRendersNull(expr.Args[0])
			}
		}

		return false
	}

	return false
}
