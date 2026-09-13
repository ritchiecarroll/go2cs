// stringSliceConversions.go - Gbtc
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

// String ↔ byte/rune-slice conversions in which a DEFINED type sits on one end or the other.
//
// Go spells `[]byte(s)`, `[]E(s)` and `string(b)` identically whether the string, the slice or its
// element is a defined type or the plain builtin — the conversion is defined over the UNDERLYING
// types. C# reaches the two ends through different machinery, and neither end can be reached by
// chaining, because C# applies at most ONE user-defined conversion in a single context:
//
//   - The STRING end. A `[GoType("@string")]` wrapper converts to golib's `@string`, and `@string`
//     converts to `byte[]`/`rune[]` — two user-defined hops, so `slice<byte>(v)` over a defined
//     string finds no applicable `slice<T>(T[])` overload (CS1503). Spelling the `(@string)` step
//     explicitly leaves exactly one implicit step for the argument conversion, the same remedy a
//     split string LITERAL already takes (see isConstantStringConcat) and the one the `u8` literal
//     route writes through its sourceIsRuneArray context.
//
//   - The ELEMENT end. `slice<byte>` and `slice<myByte>` are unrelated generic instantiations with
//     no conversion between them at all — the element wrapper's own `byte`↔`myByte` operators say
//     nothing about the slices over them. The elements are therefore projected one at a time
//     through that operator, using golib's `widen`. Go's string↔slice conversion always
//     materializes fresh storage, so an element-wise copy is exactly its cost model rather than a
//     concession: `[]E(s)` and `string(b)` both allocate in Go too.
//
// The census that sized this: across the whole Go 1.23.1 standard library — production AND test
// sources — the shapes appear five times, all in `encoding/json`'s suite (`[]byte(strMarshaler)`,
// `[]byte(*strPtrMarshaler)`, `[]byte(marshaledValue)`, `[]Uint8("hello")` and
// `renamedRenamedByteSlice("abc")`), which is why the corpus compiled clean without them. They are
// ordinary Go all the same, and five of the eight errors that stood between `encoding/json` and its
// first run. Guarded by the DefinedElemStringConversion behavioral test.

// sliceElemIsDefined reports whether a byte/rune slice type's ELEMENT is a DEFINED type — `[]myByte`
// rather than `[]byte` — and returns the element's underlying basic when it is. A defined element is
// what makes the slice unreachable from `@string` by conversion; a plain one keeps every existing
// emission byte-identical.
func sliceElemIsDefined(sliceType *types.Slice) (*types.Basic, bool) {
	if _, isNamed := types.Unalias(sliceType.Elem()).(*types.Named); !isNamed {
		return nil, false
	}

	basic, ok := sliceType.Elem().Underlying().(*types.Basic)

	if !ok || (basic.Kind() != types.Byte && basic.Kind() != types.Rune) {
		return nil, false
	}

	return basic, true
}

// isByteOrRuneSlice reports whether a slice type's element is — or is written over — `byte` or
// `rune`, i.e. whether a string converts to it at all.
func isByteOrRuneSlice(sliceType *types.Slice) bool {
	basic, ok := sliceType.Elem().Underlying().(*types.Basic)
	return ok && (basic.Kind() == types.Byte || basic.Kind() == types.Rune)
}

// isStringTyped reports whether a type's underlying type is a string — the `string` builtin, an
// untyped string constant, or a defined type written over one.
func isStringTyped(typ types.Type) bool {
	basic, ok := typ.Underlying().(*types.Basic)
	return ok && basic.Info()&types.IsString != 0
}

// stringConversionOperand renders a string→slice conversion's operand as golib's `@string`, which is
// the one form the slice materialization can start from.
func (v *Visitor) stringConversionOperand(arg ast.Expr, expr string) string {
	// A string LITERAL renders as a C# string or a `u8` ROM span — neither is an `@string`, and
	// both convert to one implicitly, so the cast costs a token and buys the whole conversion.
	if basicLit, ok := arg.(*ast.BasicLit); ok && basicLit.Kind == token.STRING {
		return "(@string)" + expr
	}

	// A DEFINED string is a [GoType] wrapper; a plain `string` value is ALREADY an `@string` and
	// needs nothing (which is what keeps every pre-existing site byte-identical).
	if argType := v.info.TypeOf(arg); argType != nil && isStringTyped(argType) {
		if _, isNamed := types.Unalias(argType).(*types.Named); isNamed {
			return fmt.Sprintf("(@string)%s", parenthesizedOperand(expr))
		}
	}

	return expr
}

// elementProjection renders golib's element-wise `widen` between a byte/rune element type and the
// other spelling of it — `myByte`→`byte` or `byte`→`myByte`. The lambda parameter carries the
// temp-var marker so it can never collide with a converted Go identifier (C# rejects a lambda
// parameter that shadows an enclosing local).
func (v *Visitor) elementProjection(fromElem, toElem types.Type, sliceExpr string) string {
	toName := v.getCSharpTypeName(toElem)
	elemVar := fmt.Sprintf("elem%s0", TempVarMarker)

	return fmt.Sprintf("widen<%s, %s>(%s, %s => (%s)%s)",
		v.getCSharpTypeName(fromElem), toName, sliceExpr, elemVar, toName, elemVar)
}

// stringToByteSliceConversion renders Go's `[]E(s)` — a string converting to a byte or rune slice —
// against the underlying form of whichever end is a defined type. It returns the UNDERLYING slice
// expression; a named-slice target adds its own wrapper cast around the result, exactly as the
// plain-element route already does.
//
// `sliceType` is the target's underlying slice and `expr` the already-converted operand.
func (v *Visitor) stringToByteSliceConversion(sliceType *types.Slice, arg ast.Expr, expr string) string {
	stringExpr := v.stringConversionOperand(arg, expr)
	elemBasic, elemDefined := sliceElemIsDefined(sliceType)

	if !elemDefined {
		// The plain element keeps the pre-existing form verbatim: golib's `@string` converts
		// straight to `byte[]`/`rune[]`, so one builtin call is the whole conversion.
		return fmt.Sprintf("%s(%s)", v.getCSharpTypeName(sliceType), stringExpr)
	}

	underlying := v.getCSharpTypeName(types.NewSlice(elemBasic))

	return v.elementProjection(elemBasic, sliceType.Elem(), fmt.Sprintf("%s(%s)", underlying, stringExpr))
}

// byteSliceToStringConversion renders Go's `string(b)` for a byte/rune slice whose ELEMENT is a
// defined type: the elements are projected back to their underlying basic, at which point golib's
// existing `slice<byte>`/`slice<rune>` → `@string` conversion applies. `sliceExpr` is the operand
// already rendered as the underlying slice (a named-slice wrapper carries its own cast). Reports
// false — emitting nothing — for a plain element, which needs no projection.
func (v *Visitor) byteSliceToStringConversion(sliceType *types.Slice, sliceExpr string) (string, bool) {
	elemBasic, elemDefined := sliceElemIsDefined(sliceType)

	if !elemDefined {
		return "", false
	}

	return fmt.Sprintf("((@string)%s)", v.elementProjection(sliceType.Elem(), elemBasic, sliceExpr)), true
}

// parenthesizedOperand wraps a rendered operand in parentheses unless it is a bare identifier chain,
// so an explicit cast binds the whole operand rather than its first token.
func parenthesizedOperand(expr string) string {
	if len(expr) == 0 || isIdentifierChain(expr) {
		return expr
	}

	return fmt.Sprintf("(%s)", expr)
}

// isIdentifierChain reports whether a rendered expression is a bare identifier chain (`s`, `x.Value`,
// `Ꮡp.Value.name`) — the shape a cast can prefix without parentheses.
func isIdentifierChain(expr string) bool {
	for i, r := range expr {
		switch {
		case r == '.' && i > 0:
		case r == '_' || r == '@':
		case r >= 'a' && r <= 'z', r >= 'A' && r <= 'Z':
		case r >= '0' && r <= '9' && i > 0:
		case r > 0x7F:
			// A converted identifier may carry the Go-symbol marks (Ꮡ, ᴛ, ˢ, …); those are
			// identifier characters in the emitted C# exactly as they are here.
		default:
			return false
		}
	}

	return true
}
