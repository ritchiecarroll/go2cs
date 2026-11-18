// arrayCloneOperations.go - Gbtc
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

// typeIsZeroLengthArray reports whether t is an array of length zero, `[0]E` or a type over one. Such an array
// holds no element, so a struct copy that shares its empty backing store with the source is indistinguishable from
// a copy that does not: there is nothing to write through either. A FIELD of that type is therefore no reason to
// clone a struct, and no member of its `[GoValueClone(…)]` list -- syscall's `InotifyEvent.Name [0]uint8` (the
// flexible-array member idiom) and go-json-experiment's embedded `nonComparable [0]func()` (the uncomparable
// marker) both cost a clone at every copy and bought nothing. A bare `[0]E` VALUE still takes its own `.Clone()`
// where Go copies it; only the field walks below skip it.
func typeIsZeroLengthArray(t types.Type) bool {
	if t == nil {
		return false
	}

	array, isArray := t.Underlying().(*types.Array)

	return isArray && array.Len() == 0
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
// An EMBEDDED (promoted) struct is walked like any other field. go2cs-gen holds an embed as an INLINE
// field, so the plain C# struct copy copies the embed itself, exactly as Go does (until 2026-08-14 an
// embed was a shared `ж<T>` box, which gave it reference semantics a value copy then aliased -- the
// defect behind go/types' type-parameter identity wall; see ConversionStrategies-Reference, *An
// embedded struct is an INLINE field, so a value copy copies it*). An ARRAY the embed carries is
// another matter: its backing store is shared by that copy, so a struct that carries an array only
// through an embed is stamped with the embed's member, and the generated `copy.<member> =
// <member>.ΔClone()` lands in the copy's own inline storage. An embedded POINTER is a pointer, shared
// in Go too, and needs nothing.
func typeNeedsValueClone(t types.Type) bool {
	return typeNeedsValueCloneSeen(t, map[types.Type]bool{})
}

func typeNeedsValueCloneSeen(t types.Type, seen map[types.Type]bool) bool {
	if t == nil {
		return false
	}

	// A struct whose arrays are held inline (inlineArrayEmission.go) is copied whole by a plain C# struct copy.
	if inlineArrayHeld(t) {
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

		if field.Name() == "_" || typeIsZeroLengthArray(field.Type()) {
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
//
// The names MUST be spelled exactly as visitStructType DECLARES them, because go2cs-gen resolves
// each one against the emitted struct's members: a stamp naming something the type does not declare
// is CS1061 at the generated `Clone()`. That spelling is getCoreSanitizedIdentifier plus the
// enclosing-type disambiguation below — NOT getSanitizedIdentifier, whose collision branch
// Δ-prefixes a name found in nameCollisions. That branch exists for a package-level TYPE colliding
// with a METHOD; a struct field is MEMBER scope and is never renamed for it. Using it here stamped
// runtime's `trace` fields as `Δtrace` while the declaration emitted a bare `trace`
// (CS1061 ×2, 1.24 `runtime2.cs`) — latent for as long as that file was a frozen, unstamped
// hand-own, and reached the moment a re-derive stamped it.
func structValueCloneFields(t types.Type, structTypeName string) []string {
	if t == nil {
		return nil
	}

	structType, isStruct := t.Underlying().(*types.Struct)

	if !isStruct || inlineArrayHeld(t) {
		return nil
	}

	var fields []string

	for i := range structType.NumFields() {
		field := structType.Field(i)

		if field.Name() == "_" || typeIsZeroLengthArray(field.Type()) {
			continue
		}

		if typeNeedsValueClone(field.Type()) {
			fieldName := getCoreSanitizedIdentifier(field.Name())

			// Transcribed from visitStructType's declaration branch rather than paraphrased, and
			// compared RAW on both sides for the same reason it is there: a field whose name equals
			// its enclosing type is renamed (CS0542), so a stamp spelling it bare would name a
			// member that does not exist. Untriggered in today's corpus — the mirror of the defect
			// above, and left unfixed it would be the same booby trap facing the other way.
			if strings.TrimPrefix(fieldName, "@") == strings.TrimPrefix(strings.TrimPrefix(structTypeName, ShadowVarMarker), "@") {
				fieldName = typeCollidingFieldName(fieldName)
			}

			fields = append(fields, fieldName)
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
//
// A named ARRAY that declares a Go method named `Clone` takes ValueCloneMethod too: its wrapper's
// `Clone()` would be an INSTANCE member, which C# binds before the extension method Go's own Clone
// becomes, so the wrapper drops it for such a type (IArrayTypeTemplate) and the copy must not name it
// -- Go 1.26 hash/fnv's sum128/sum128a, whose `Clone() (hash.Cloner, error)` forwarder bound the array
// copy (CS0029).
func valueCloneSuffix(t types.Type) string {
	if typeIsArrayValue(t) {
		if declaresGoMethod(t, "Clone") {
			return fmt.Sprintf(".%s()", ValueCloneMethod)
		}

		return ".Clone()"
	}

	if typeNeedsValueClone(t) {
		return fmt.Sprintf(".%s()", ValueCloneMethod)
	}

	return ""
}

// valueAssignInPlace reports whether lhs, one left operand of assignStmt, takes the IN-PLACE copy: it is an EXISTING
// location (not `_`) whose type owns array storage (valueAssignMethod). In Go an array variable IS its storage, so
// `a = b` copies b's elements INTO a, and a slice, an element pointer or a's address taken beforehand still names a
// afterwards; a struct holding an array is the same one level up. The emitted `array<T>` (the named-array wrapper, a
// [GoValueClone] struct) is a struct over a shared backing, so the swap `a = b.Clone()` gave a new storage and left
// everything taken from a on the old one (docs/phase4/DESIGN-array-reassign-in-place.md). `a.Assign(b)` copies into
// a's own storage instead. A declaration, an argument, a return, a map value and a send are new storage in Go too,
// and keep the clone (cloneValueCopy).
//
// The predicate is "existing location", decided PER OPERAND (P5's C5): every operand of an `=`, and an operand of a
// `:=` that REDECLARES a variable already in scope (`a, err := f()` with `a` declared: go/types records no new object
// for it, so it assigns the existing `a`). The caller decides the emission shape (a single assignment, a tuple's
// temporaries, a range or select binding).
func (v *Visitor) valueAssignInPlace(assignStmt *ast.AssignStmt, lhs ast.Expr) bool {
	ident, isIdent := ast.Unparen(lhs).(*ast.Ident)

	if isIdent && ident.Name == "_" {
		return false
	}

	// A map element is a VALUE, not a location: `m[k] = b` stores a new value, which is new storage in Go too, and a C#
	// map indexer is not ref-returning (`m[k].Assign(b)` is CS0206). It keeps the clone, as the design's rule says.
	if index, isIndex := ast.Unparen(lhs).(*ast.IndexExpr); isIndex {
		if base := v.info.TypeOf(index.X); base != nil {
			if _, isMap := base.Underlying().(*types.Map); isMap {
				return false
			}
		}
	}

	switch assignStmt.Tok {
	case token.ASSIGN:
	case token.DEFINE:
		if !isIdent || v.info.Defs[ident] != nil {
			return false
		}
	default:
		return false
	}

	return valueAssignMethod(v.info.TypeOf(lhs)) != ""
}

// valueAssignMethod is the in-place copy's member on t, or "" when t owns no array storage. An array (direct, aliased
// or named) takes Assign: golib's ArrayExtensions.Assign for array<T>, the generated member for a named array. A
// struct holding one ([GoValueClone], typeNeedsValueClone) takes go2cs-gen's AssignFrom. A Go type that declares its
// OWN method of that name gets the Δ-prefixed member instead, which the generator names the same way so it cannot
// shadow Go's method (the ValueCloneMethod precedent). A struct whose arrays are held inline is copied whole by a
// plain C# assignment already, and is not one of these.
func valueAssignMethod(t types.Type) string {
	if t == nil {
		return ""
	}

	if typeIsArrayValue(t) {
		if declaresGoMethod(t, "Assign") {
			return "ΔAssign"
		}

		return "Assign"
	}

	if _, isStruct := t.Underlying().(*types.Struct); isStruct && typeNeedsValueClone(t) {
		if declaresGoMethod(t, "AssignFrom") {
			return "ΔAssignFrom"
		}

		return "AssignFrom"
	}

	return ""
}

// valueAssignTarget is the rendered location an in-place copy is called on. `*p = R` for a pointer to an UNNAMED array
// calls golib's ж<array<T>> extension, `p.Assign(R)`, so a box over NATIVE memory stores through its element door
// (P5's C6: NativeArrayBox has no array<T> to hand out, and `p.Value` throws there by design); the rendering's
// `.Value` deref is dropped for it. Every other location keeps its rendering.
func valueAssignTarget(lhs ast.Expr, t types.Type, rendered string) string {
	if _, isStar := ast.Unparen(lhs).(*ast.StarExpr); !isStar || t == nil {
		return rendered
	}

	if _, unnamed := types.Unalias(t).(*types.Array); !unnamed {
		return rendered
	}

	if trimmed, ok := strings.CutSuffix(rendered, ".Value"); ok {
		return trimmed
	}

	return rendered
}

// valueAssignTempRoot names a temporary for an in-place operand after the variable its location is rooted at
// (`arrs[i]` -> arrs), so a tuple's temporaries read as the operands they stand for; `ᴛ` follows from getTempVarName.
func valueAssignTempRoot(lhs ast.Expr) string {
	for {
		switch e := lhs.(type) {
		case *ast.ParenExpr:
			lhs = e.X
		case *ast.IndexExpr:
			lhs = e.X
		case *ast.SelectorExpr:
			lhs = e.X
		case *ast.StarExpr:
			lhs = e.X
		case *ast.Ident:
			if e.Name != "_" {
				return strings.TrimPrefix(getSanitizedIdentifier(e.Name), "@")
			}

			return "val"
		default:
			return "val"
		}
	}
}

// declaresGoMethod reports whether t is (an alias of) a NAMED type declaring a method called name,
// with either receiver form.
func declaresGoMethod(t types.Type, name string) bool {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok {
		return false
	}

	for i := 0; i < named.NumMethods(); i++ {
		if named.Method(i).Name() == name {
			return true
		}
	}

	return false
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

	switch e := expr.(type) {
	case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr, *ast.StarExpr:
	case *ast.CallExpr:
		// A CONVERSION between clone-needing types (`[4]int(named)`, `N(arr)`, a struct to a struct of the same
		// layout) is the operand's value under another type: the converted `(array<nint>)n` hands n's backing over
		// through the wrapper's operator, so it reads existing storage exactly when its operand does
		// (DESIGN-array-reassign-in-place.md §8 N1). appendValueClone parenthesizes the cast it then clones.
		if len(e.Args) != 1 || !v.callExprIsTypeConversion(e) || !typeNeedsValueClone(v.getExprType(expr)) {
			return false
		}

		// An inherited wrapper's operator with its base copies by itself: the value is copied once
		// (structConversionCopy.go).
		if conversionCopiedByWrapperOperator(v.getExprType(expr), v.getExprType(e.Args[0])) {
			return false
		}

		return v.exprReadsValueNeedingClone(e.Args[0])
	default:
		return false
	}

	// The array load door answers a new array, copied from the memory (arrayPointerElementDoor.go).
	if v.arrayPointerLoadIsOwnCopy(expr) || v.nativeFieldArrayViewLoadIsOwnCopy(expr) {
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

	if strings.HasPrefix(rendered, PointerDerefOp) || renderedIsCast(rendered) {
		return fmt.Sprintf("(%s)%s", rendered, suffix)
	}

	return rendered + suffix
}

// renderedIsCast reports whether rendered is a C# cast, `(T)x` or `(T)(x)`: its leading parenthesis closes before the
// end and an operand follows it. A postfix suffix binds tighter than a cast, so `(array<nint>)n.Clone()` would clone
// n under its own type; the conversion arm of exprReadsValueNeedingClone is the clone site that renders one. A
// parenthesized primary (`(x).Value`, `(a + b)`) is not a cast and is left alone.
func renderedIsCast(rendered string) bool {
	if !strings.HasPrefix(rendered, "(") {
		return false
	}

	depth := 0

	for i, r := range rendered {
		switch r {
		case '(':
			depth++
		case ')':
			depth--

			if depth == 0 {
				rest := rendered[i+1:]

				if rest == "" {
					return false
				}

				next := rest[0]

				return next == '(' || next == '@' || next == '_' || (next >= 'a' && next <= 'z') || (next >= 'A' && next <= 'Z') || next >= 0x80
			}
		}
	}

	return false
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
