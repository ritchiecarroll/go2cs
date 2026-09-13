// arrayDimsNilCargo.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"
	"strconv"
	"strings"
)

// A Go array's LENGTH is part of its type, and it is the one part the managed emission cannot
// carry: `[0]byte` and `[3]byte` both render as golib's `array<byte>`. Everywhere else the
// reflection bridge recovers it from a live source — a VALUE reveals its own length, a struct
// FIELD from the declaring type's zero instance, a func PARAMETER from the `[GoArrayDims]` stamp
// (see emitGoArrayDimsAttribute and arrayZeroValueArgs). A NIL POINTER TO AN ARRAY has none of
// those: there is no pointee to measure — `GoReflect.PointeeArrayDims` says exactly that in its
// own words — and there is no attribute slot at an expression position. So the length is carried
// on the VALUE, as descriptor cargo, which is the array analog of chanDirectionCargo.go's
// treatment of a directional channel's direction and the third member of that same finite set.
//
// WHAT IT COSTS TO GET WRONG, measured rather than argued (2026-09-02). Without the cargo,
// `(*[0]byte)(nil)` and `(*[3]byte)(nil)` emit the IDENTICAL C# expression, so
// `reflect.TypeOf((*[10000]Xscalar)(nil))` cannot answer 10000 and reflect's own
// `verifyGCBits` row at all_test.go:7274 has no length to allocate from. The Go 1.23.12 standard
// library contains THIRTEEN nil constructions of pointer-to-array type; all thirteen are in
// `_test.go` files and NONE is in production code, which is why a `-stdlib` census of this defect
// reports zero and why the number that sized this cut came from `-tests`.
//
// A POINTER TO A NAMED ARRAY IS STAMPED TOO, and the first cut of this file said the opposite.
// `type mediumScalarEven [8192]byte` gets its own C# struct carrying `[GoType("[8192]byte")]`, so
// the emission of `(*mediumScalarEven)(nil)` — `((ж<mediumScalarEven>)nil)` — looks as though the
// dimension survives. It does not: nothing reads it back. The nil still reaches
// `GoReflect.PointeeArrayDims`, which answers null because there is no pointee to measure, so
// `synthType` gets no dims and `Elem()` describes a dimension-less array. What the named case
// preserves is C# TYPE IDENTITY — `%T`, type-switch arms, reference inequality — not the length.
// MEASURED by the TypedNilPtrArrayDims guard, both shapes, against `go run`:
// `reflect.TypeOf((*named)(nil)).Elem().Len()` is 3 in Go and was 0 here, for a package-level
// named array and a function-local one alike. The walk below therefore descends through
// `Underlying()`, which sees a named array exactly as it sees a literal one.
//
// WHAT IS STILL NOT STAMPED is the DEFINED POINTER type — `type MyBytesArrayPtr0 *[0]byte` —
// and that is a different kind of exclusion: not a judgment about whether it needs the cargo, but
// the fact that it has nowhere to put it. A defined pointer emits as a go2cs-gen wrapper CLASS,
// not as `ж<T>`, so `NilBoxOfDims` does not exist on it and stamping it is a build error at every
// such site. Its length would have to live in the wrapper's own `[GoType]` metadata, which is a
// generator change with its own gate ladder. Three of the thirteen sites are that shape.

// nilArrayPtrDims returns the Go array dimensions a nil pointer-to-array conversion must carry,
// outermost first, or nil when the target is not an UNDEFINED pointer to an array.
func nilArrayPtrDims(t types.Type) []int64 {
	if t == nil {
		return nil
	}

	// The pointer type itself must be UNDEFINED, not merely have a pointer underlying: a DEFINED
	// pointer type (`type MyBytesArrayPtr0 *[0]byte`) emits as a go2cs-gen wrapper CLASS, which has
	// no `NilBoxOfDims` to call — the cargo has nowhere to ride there, and the wrapper's own
	// `[GoType("ж<array<byte>>")]` metadata is where its length would have to live instead. That is
	// a generator change with its own gate ladder (route #7), deliberately not bundled here. Same
	// exclusion chanDirectionCargo.go draws for a defined channel type, and for the same reason.
	ptr, isPtr := types.Unalias(t).(*types.Pointer)

	if !isPtr {
		return nil
	}

	var dims []int64

	// Underlying(), so a NAMED array is seen exactly as a literal one: its own emission carries the
	// dimension in metadata but nothing reads that back for a nil pointer (see the header). Unalias
	// first because an ALIAS for an array type IS that array type and has no declaration of its own.
	for elem := types.Unalias(ptr.Elem()); ; {
		arr, isArr := elem.Underlying().(*types.Array)

		if !isArr {
			break
		}

		dims = append(dims, arr.Len())
		elem = types.Unalias(arr.Elem())
	}

	return dims
}

// nilArrayPtrValue renders the NIL pointer to an array carrying its dimensions — what
// `(*[N]E)(nil)` is — or "" when the target carries no dimensions this emission stamps.
//
// The dimensions are rendered as C# `long` literals, matching GoArrayDimsAttribute's deliberate
// 64-bit widening rather than GoMapKeyDimsAttribute's un-widened form. A Go array length is Go's
// `int`, and the standard library uses the full range: runtime/vdso_linux.go declares
// `*[1<<50 - 1]byte`, Go's pointer-to-unbounded-array idiom, which reaches the bridge as a FIELD
// today and reaches THIS site the first time anyone writes it as a conversion.
func (v *Visitor) nilArrayPtrValue(t types.Type, targetCS string) string {
	dims := nilArrayPtrDims(t)

	if len(dims) == 0 || targetCS == "" {
		return ""
	}

	rendered := make([]string, len(dims))

	for i, dim := range dims {
		rendered[i] = strconv.FormatInt(dim, 10) + "L"
	}

	return targetCS + ".NilBoxOfDims(" + strings.Join(rendered, ", ") + ")"
}

// nilArrayPtrValueForTarget is nilArrayPtrValue for the positions where the target is known as a
// types.Type rather than as an AST type expression -- assignment, variable declaration, call
// argument and result. The conversion positions in convCallExpr already hold a rendered target
// (convStarExpr's lift, or the resolved name), so they call nilArrayPtrValue directly; here the
// rendering is the one the converter uses everywhere else for a types.Type.
//
// Returns "" for anything that is not an UNDEFINED pointer to an array, so a caller can use it as
// a straight substitution test.
func (v *Visitor) nilArrayPtrValueForTarget(t types.Type) string {
	if len(nilArrayPtrDims(t)) == 0 {
		return ""
	}

	return v.nilArrayPtrValue(t, v.getCSharpTypeName(t))
}

// identIsUniverseNilExpr reports the bare `nil` literal -- the only expression whose emission the
// dims cargo replaces. A SHADOWING object named nil is not the literal and must not be touched,
// which is the same distinction convIdent draws.
func (v *Visitor) identIsUniverseNilExpr(expr ast.Expr) bool {
	ident, ok := expr.(*ast.Ident)

	if !ok {
		return false
	}

	return v.identIsUniverseNil(ident)
}

// appendNilArrayDimsTypeContext is the position-independent half of the cargo's plumbing: given a
// bare `nil` and the STATIC target type it is bound for, it appends the IdentContext convIdent
// reads. Callers differ only in where the target comes from -- an assignment's left-hand side, a
// signature's result, a call's parameter -- so they all end here.
//
// Returns `base` unchanged for anything that is not a nil bound for an undefined pointer-to-array,
// which keeps every call site a one-line wrap with no condition of its own.
func (v *Visitor) appendNilArrayDimsTypeContext(base []ExprContext, expr ast.Expr, target types.Type) []ExprContext {
	if target == nil || !v.identIsUniverseNilExpr(expr) {
		return base
	}

	if v.nilArrayPtrValueForTarget(target) == "" {
		return base
	}

	identContext := DefaultIdentContext()
	identContext.nilArrayTarget = target

	out := make([]ExprContext, len(base), len(base)+1)
	copy(out, base)

	return append(out, identContext)
}
