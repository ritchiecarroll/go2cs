// arrayCloneOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

// typeIsArrayValue reports whether t's underlying type is a fixed-size Go array — a direct
// `[N]E`, an alias to one, or a NAMED array type (`type T [N]E`). Both renderings expose a
// strongly-typed `Clone()` (golib's `array<T>` and the generated named-array wrapper), so a
// Go by-value array copy can append `.Clone()` uniformly. A constrained type parameter never
// matches (its underlying is the constraint interface), keeping erased generics untouched.
func typeIsArrayValue(t types.Type) bool {
	if t == nil {
		return false
	}

	_, isArray := t.Underlying().(*types.Array)

	return isArray
}

// typeNeedsValueClone reports whether a Go BY-VALUE copy of t needs an explicit `.Clone()` in the
// emitted C# — t is a fixed-size array (typeIsArrayValue above), or a STRUCT that carries one in a
// field, directly or through another such struct.
//
// Go copies an array inline at every value transfer, but both array renderings (golib's `array<T>`
// and the generated named-array wrapper) are structs over a shared backing store, so a plain C#
// struct copy leaves the arrays ALIASED — the copy's writes reach back into the source. That is
// true one level up as well: `digest{h [8]uint32; x [64]byte}` copied by value shares BOTH backings,
// which is exactly what broke crypto/sha256's `Sum` (`d0 := *d`, finalize the copy, keep writing the
// original — the finalization destroyed the caller's running state). Such a struct is stamped
// `[GoValueClone(…)]`, go2cs-gen generates its field-precise `Clone()`, and every copy site here
// appends it.
//
// EMBEDDED (promoted) struct members are not a reason to clone: go2cs-gen holds an embed as an
// INLINE field, so the plain C# struct copy already copies it, exactly as Go does. (Until
// 2026-08-14 an embed was a shared `ж<T>` box, which gave it reference semantics a value copy then
// aliased — the defect behind go/types' type-parameter identity wall; see
// ConversionStrategies-Reference, *An embedded struct is an INLINE field, so a value copy copies
// it*.) What the skip below still costs is narrower and named: a fixed ARRAY reached only THROUGH
// an embed is not seen, so such a struct is not stamped and the array backing stays shared. Widening
// the walk is now sound — the generated `copy.<member> = <member>.ΔClone()` lands in the copy's own
// inline storage — but it moves EMISSION corpus-wide, so it belongs to a change that owns that
// footprint.
func typeNeedsValueClone(t types.Type) bool {
	return typeNeedsValueCloneSeen(t, map[types.Type]bool{})
}

func typeNeedsValueCloneSeen(t types.Type, seen map[types.Type]bool) bool {
	if t == nil {
		return false
	}

	if typeIsArrayValue(t) {
		return true
	}

	structType, isStruct := t.Underlying().(*types.Struct)

	if !isStruct {
		return false
	}

	// Go forbids a struct containing itself by value, so the walk is finite; the guard covers a
	// pathological/erroneous type graph rather than a legal one.
	if seen[t] {
		return false
	}

	seen[t] = true

	for i := range structType.NumFields() {
		field := structType.Field(i)

		if field.Embedded() || field.Name() == "_" {
			continue
		}

		if typeNeedsValueCloneSeen(field.Type(), seen) {
			return true
		}
	}

	return false
}

// structValueCloneFields lists the C# member names of t's fields that a by-value copy must
// re-clone — the `[GoValueClone(…)]` argument list. Nil when t is not a struct or needs nothing.
func structValueCloneFields(t types.Type) []string {
	if t == nil {
		return nil
	}

	structType, isStruct := t.Underlying().(*types.Struct)

	if !isStruct {
		return nil
	}

	var fields []string

	for i := range structType.NumFields() {
		field := structType.Field(i)

		if field.Embedded() || field.Name() == "_" {
			continue
		}

		if typeNeedsValueClone(field.Type()) {
			fields = append(fields, getSanitizedIdentifier(field.Name()))
		}
	}

	return fields
}

// valueCloneSuffix is the member call that deep-copies a clone-needing value of type t. An ARRAY
// (direct, aliased, or named) uses golib's own `array<T>.Clone()` / the named wrapper's; a STRUCT
// uses the generated `ΔClone()` (ValueCloneMethod), which is deliberately NOT named `Clone` — a Go
// type may declare its own `Clone` method, which converts to an EXTENSION method on the package
// class, and an instance member of the same name silently shadows it (vendored x/crypto/sha3's
// `state.Clone() ShakeHash`, whose recv-overload forwarder then bound to the wrong return type).
// Empty when t needs no clone, so a caller can use it as its own gate.
func valueCloneSuffix(t types.Type) string {
	if typeIsArrayValue(t) {
		return ".Clone()"
	}

	if typeNeedsValueClone(t) {
		return fmt.Sprintf(".%s()", ValueCloneMethod)
	}

	return ""
}

// wrapperValueCloneAttr returns the `[GoValueClone("Value")] ` stamp for a DEFINED type over another
// named type whose underlying is a clone-needing STRUCT (`type IpMaskString IpAddressString`, whose
// underlying carries a `[16]byte`). Such a type is emitted as go2cs-gen's INHERITED wrapper — one
// `Value` member holding the underlying — so its whole deep copy is that member's, and the generator
// forwards `Clone()` to it. Empty for every other kind: a named ARRAY (or a wrapper over one) already
// carries a strongly-typed `Clone()` from the array templates, and nothing else needs one.
func wrapperValueCloneAttr(t types.Type) string {
	if t == nil {
		return ""
	}

	if _, isStruct := t.Underlying().(*types.Struct); !isStruct {
		return ""
	}

	if !typeNeedsValueClone(t) {
		return ""
	}

	return "[GoValueClone(\"Value\")] "
}

// exprReadsValueNeedingClone reports whether expr reads a CLONE-NEEDING value out of EXISTING
// storage — an ident, selector, index, or pointer-deref (after unwrapping parens) whose type is a
// fixed-size array or a struct carrying one (typeNeedsValueClone). Go copies such a value in full
// at every value transfer (range element, composite-literal element, return, channel send, append
// element), but the emitted `array<T>` / named-array wrapper is a struct over a shared T[] backing
// store, so a plain C# struct copy ALIASES — such a transfer must append the strongly-typed
// `.Clone()`. Only existing storage needs it: a composite literal, call result, or make/new
// expression is freshly constructed and reachable by no other name. (The ASSIGNMENT/var-decl forms
// of the same defect are handled by cloneValueCopy in visitAssignStmt.go.)
func (v *Visitor) exprReadsValueNeedingClone(expr ast.Expr) bool {
	for {
		paren, ok := expr.(*ast.ParenExpr)

		if !ok {
			break
		}

		expr = paren.X
	}

	switch expr.(type) {
	case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr, *ast.StarExpr:
	default:
		return false
	}

	return typeNeedsValueClone(v.getExprType(expr))
}

// appendValueClone appends the strongly-typed `.Clone()` to an already-rendered value
// expression, WRAPPING it first when the rendering is prefixed by the unary deref operator.
// C# postfix binds tighter than unary, so a naked suffix on a `~`-prefixed rendering re-binds
// onto the operand instead of the dereferenced array — reflect InterfaceData's
// `return *(*[2]uintptr)(v.ptr)` emitted `~(ж<array<uintptr>>)(uintptr)(v.ptr).Clone()`, whose
// `.Clone()` reads the inner @unsafe.Pointer (CS1061), blocking the whole corpus through
// fmt→reflect. Mirrors the same precedence guard convStarExpr applies when IT appends the
// postfix `.Value` to a cast/deref rendering. Every other shape exprReadsValueNeedingClone
// admits (ident, selector, index, and the postfix `.Value` deref form) is already a C# primary
// expression, so this is byte-neutral everywhere the suffix was correct.
func appendValueClone(rendered string, valueType types.Type) string {
	suffix := valueCloneSuffix(valueType)

	if suffix == "" {
		return rendered
	}

	if strings.HasPrefix(rendered, PointerDerefOp) {
		return fmt.Sprintf("(%s)%s", rendered, suffix)
	}

	return rendered + suffix
}

// appendRangeSnapshot appends the array RANGE-EXPRESSION copy (RangeSnapshotMethod) to an
// already-rendered array-valued expression, under the same precedence guard appendValueClone
// applies — C# postfix binds tighter than the unary deref operator, so a naked suffix on a
// `~`-prefixed rendering would re-bind onto the operand instead of the dereferenced array.
//
// This is deliberately NOT valueCloneSuffix's `.Clone()`, though the copy is the same one Go takes.
// A range snapshot cannot outlive its loop, which is what makes it Go's INLINE, stack-resident copy
// — zero mallocs, zero TotalAlloc — while `Clone()` mints a counted managed array because every
// OTHER array copy site (assignment, return, field, channel send) produces a value that does
// outlive the statement. golib's counter is the structural mirror of runtime.MemStats.Mallocs, so
// charging the range copy there would make it disagree with Go by construction; golib answers
// RangeSnapshotMethod out of a pooled buffer released when the loop ends.
func appendRangeSnapshot(rendered string) string {
	suffix := fmt.Sprintf(".%s()", RangeSnapshotMethod)

	if strings.HasPrefix(rendered, PointerDerefOp) {
		return fmt.Sprintf("(%s)%s", rendered, suffix)
	}

	return rendered + suffix
}

// withValueCloneArgs flags every POSITIONAL composite-literal element that reads a CLONE-NEEDING
// value out of existing storage for the `.Clone()` suffix (CallExprContext.cloneArrayArg — Go
// copies the value into the composite's slot, and the emitted struct copy would alias its
// backing). Allocates the context when nil and a flag is needed, matching the nil-context
// defaults convExprList assumes (u8 string literals stay allowed). Keyed elements clone in
// convKeyValueExpr instead.
func (v *Visitor) withValueCloneArgs(elts []ast.Expr, context *CallExprContext) *CallExprContext {
	for i, elt := range elts {
		if _, isKeyed := elt.(*ast.KeyValueExpr); isKeyed {
			continue
		}

		if !v.exprReadsValueNeedingClone(elt) {
			continue
		}

		if context == nil {
			context = DefaultCallExprContext()

			for j := range elts {
				context.u8StringArgOK[j] = true
			}
		}

		if context.cloneArrayArg == nil {
			context.cloneArrayArg = make(map[int]bool)
		}

		context.cloneArrayArg[i] = true
	}

	return context
}
