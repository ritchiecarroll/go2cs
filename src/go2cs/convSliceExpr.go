// convSliceExpr.go - Gbtc
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
	"go/constant"
	"go/token"
	"go/types"
	"math"
	"strings"
)

func (v *Visitor) convSliceExpr(sliceExpr *ast.SliceExpr) string {
	ident := v.convExpr(sliceExpr.X, nil)

	// A sub-slice of a CONSTRAINED TYPE PARAMETER (`s[lo:hi]` / `s[lo:hi:max]` where s is
	// `S ~[]E`) must yield S again, sharing backing — Go's named-slice sub-slice semantics
	// (pdqsort's recursion assigns it back to S-typed values, CS0266). golib's
	// subslice<S, E>/subslice3<S, E> reconstruct via S.Wrap; the type arguments are emitted
	// explicitly (E is constraint-only — C# cannot infer it).
	if tp, ok := types.Unalias(v.info.TypeOf(sliceExpr.X)).(*types.TypeParam); ok {
		if sliceType := typeParamSliceCore(tp); sliceType != nil {
			// NO BOUND IS A SENTINEL. An omitted LOW is emitted as the 0 it means — Go's `s[:h]` IS
			// `s[0:h]` — and an omitted HIGH selects the two-argument `subslice` overload rather than
			// travelling as -1. Both bounds used to default to "-1", which made `s[i:]` and `s[i:-1]`
			// the same call and (worse) made golib clamp every negative low to 0, so Go's panic for a
			// negative index never fired: slices' TestInsertPanics and TestReplacePanics measured
			// exactly that, since `slices.Insert`/`Replace` open with `_ = s[i:]` / `_ = s[i:j]` as
			// their bounds check. A three-index expression always carries its high bound (Go's
			// grammar requires it), so subslice3 keeps three real arguments.
			low, high, max := "0", "", ""

			if sliceExpr.Low != nil {
				low = v.convExpr(sliceExpr.Low, nil)
			}

			if sliceExpr.High != nil {
				high = v.convExpr(sliceExpr.High, nil)
			}

			if sliceExpr.Max != nil {
				max = v.convExpr(sliceExpr.Max, nil)
			}

			elemType := convertToCSTypeName(v.getAliasQualifiedTypeName(sliceType.Elem(), false))

			if sliceExpr.Max != nil {
				return fmt.Sprintf("subslice3<%s, %s>(%s, %s, %s, %s)", getSanitizedIdentifier(tp.Obj().Name()), elemType, ident, low, high, max)
			}

			if sliceExpr.High == nil {
				return fmt.Sprintf("subslice<%s, %s>(%s, %s)", getSanitizedIdentifier(tp.Obj().Name()), elemType, ident, low)
			}

			return fmt.Sprintf("subslice<%s, %s>(%s, %s, %s)", getSanitizedIdentifier(tp.Obj().Name()), elemType, ident, low, high)
		}

		// A sub-slice of a `string | []byte` UNION-constrained value goes through the Range
		// indexer of golib's self-referential IByteSeq<TSelf, T>, which is what getGenericDefinition
		// emits as this parameter's constraint (`where bytes : IByteSeq<bytes, byte>`,
		// constraintOperations). That indexer returns TSelf — the type parameter ITSELF — which is
		// exactly how Go types the expression, so the value already assigns back to / passes as /
		// returns as `bytes` (time format_rfc3339) and spreads or converts through the constraint's
		// own members (encoding/json's `src[lo:hi]...`, bytealg's `string(s[i:j])`). No conversion
		// is emitted: the range expression IS the type parameter, and `s = s[19..]` reads as the Go
		// does. A 3-index slice cannot occur on a string-including union (Go forbids it on strings),
		// so only the range forms take this route.
		if typeParamIsStringByteUnion(tp) && !sliceExpr.Slice3 {
			// A bound the Range cannot carry exactly takes IByteSeq's own slice members instead, which
			// return TSelf too (see sliceBoundsNeedMethod).
			if call, ok := v.sliceMethodCall(ident, sliceExpr); ok {
				return call
			}

			var inner string

			switch {
			case sliceExpr.Low == nil && sliceExpr.High == nil:
				inner = ident + "[..]"
			case sliceExpr.High == nil:
				inner = ident + "[" + v.getRangeIndexer(sliceExpr.Low) + "..]"
			case sliceExpr.Low == nil:
				inner = ident + "[.." + v.getRangeIndexer(sliceExpr.High) + "]"
			default:
				inner = ident + "[" + v.getRangeIndexer(sliceExpr.Low) + ".." + v.getRangeIndexer(sliceExpr.High) + "]"
			}

			return inner
		}
	}

	// When converting a pointer expression to a slice, we use special handling
	if isMatch, ptrType := isPointerCast(ident); isMatch && v.inFunction && sliceExpr.High != nil {
		v.useUnsafeFunc = true
		prefixLength := len(ptrType) + 5

		// Remove array type prefix from the pointer type, if present
		if strings.HasPrefix(ptrType, "array<") {
			ptrRunes := []rune(ptrType)
			ptrType = string(ptrRunes[6 : len(ptrRunes)-1])
		}

		csPtrType := ptrType

		for isMatch, ptrPtrType := isPointerExpr(ptrType); isMatch; {
			csPtrType = ptrPtrType + "*"
			prefixLength--
			isMatch = false
		}

		identRunes := []rune(ident)

		// A Go `(*[N]T)(ptr)[:n]` produces a `[]T` slice over the pointed-to memory. Emit the golib
		// `slice<T>` (the C# representation of every other `[]T`), NOT a bare `Span<T>`: a `Span<T>` does
		// not range as `(index, element)` tuples (CS8130 on `for i := range s`) and has no `Ꮡ(s, i)`
		// element-address (CS0411), whereas `slice<T>` supports both (it is `IArray<T>`). The slice is
		// built from a `ReadOnlySpan<T>` over the raw pointer; its constructor takes a C# `int` length,
		// and a Go `int`/`uint` bound is `nint`/`nuint` (no implicit conversion → CS1503), so getRangeIndexer
		// narrows it to `int` (through the underlying for a named numeric), leaving an int literal as-is.
		// This is the `(*[N]T)(ptr)` unsafe-cast form only (always memory-layout-dependent code); the slice
		// copies the pointed-to memory, which is self-consistent for code that only uses the resulting slice.
		//
		// A non-nil LOW bound offsets the base pointer and shortens the span: Go's `(*[N]T)(ptr)[lo:hi]` is
		// the elements lo..hi, so the span must START at element lo with length hi-lo. Emitting the whole
		// `[0:hi]` span instead silently produced the WRONG elements — internal/syscall/windows's
		// `(*symbolicLinkReparseBuffer).path()` slices `[n1:n2:n2]`, so os.Readlink returned the reparse
		// buffer from offset 0 rather than the substitute name, and reflect's `gcSlice` read the GC bitmap
		// from the wrong start. (A pointer cast binds tighter than `+`, so the offset applies to the typed
		// pointer — no parentheses needed around the cast.)
		base := string(identRunes[prefixLength:])
		length := v.getRangeIndexer(sliceExpr.High)

		if sliceExpr.Low != nil {
			low := v.getRangeIndexer(sliceExpr.Low)
			base = fmt.Sprintf("%s + %s", base, low)
			length = fmt.Sprintf("%s - %s", length, low)
		}

		// The span constructor COPIES: say so when this function writes through the result.
		v.noteCopyingSliceViewWrite(sliceExpr)

		return fmt.Sprintf("new slice<%s>(new ReadOnlySpan<%s>((%s*)%s, %s))", ptrType, ptrType, csPtrType, base, length)
	}

	// A slice of a POINTER-TO-ARRAY (`p[lo:hi:max]`, p of type `*[N]T`) auto-derefs in Go. The
	// box `ж<array<T>>` has no slice/range members (its underlying `array<T>` does), so operate on
	// the dereferenced array — `(~p).slice(…)` / `(~p)[..]` — instead of `p.slice(…)` (CS1929). The
	// `(*[N]T)(ptr)` pointer-CAST form is handled by the Span path above (an explicit cast, not a
	// pointer-to-array value), so it is unaffected. A deref-aliased pointer PARAMETER or RECEIVER is
	// EXCLUDED: it is emitted as the array value itself (`ref array<T> p` / `ref pageBits b`), which
	// is sliced directly — a `~` on it would deref a non-pointer (CS0023). Only a pointer-to-array
	// box (a local, a field, a call result) needs the `~`.
	if xType := v.getType(sliceExpr.X, false); xType != nil {
		if ptr, ok := xType.Underlying().(*types.Pointer); ok {
			if _, isArr := ptr.Elem().Underlying().(*types.Array); isArr {
				if v.exprIsDerefAliasedPointer(sliceExpr.X) {
					// A deref-aliased pointer PARAMETER/RECEIVER is the pointed-to value, not a box.
					// For a NAMED array type (`*pageBits` → `ref pageBits b`) that value is the wrapper,
					// which has no slice/range members — reach its underlying `array<T>` via `.Value`
					// (`b.Value[..]`). For an ANONYMOUS array (`*[N]T` → `ref array<T> p`) the value IS the
					// `array<T>`, sliced directly. A `~` on either would deref a non-pointer (CS0023).
					if _, isNamed := ptr.Elem().(*types.Named); isNamed {
						ident += ".Value"
					}
				} else {
					// A pointer-to-array BOX (a local, field, or call result) is dereferenced first.
					// A NAMED array's deref yields the wrapper (no slice/range members) — reach its
					// underlying array<T> via `.Value`, mirroring the deref-aliased branch above
					// (runtime proc.go's `mp.cgoCallers[:cgoOff]`, cgoCallers a `*cgoCallers`).
					if _, isNamed := ptr.Elem().(*types.Named); isNamed {
						ident = "(" + PointerDerefOp + ident + ").Value"
					} else {
						ident = "(" + PointerDerefOp + ident + ")"
					}
				}
			}
		}
	}

	// sliceExpr[:] => sliceExpr[..]
	if sliceExpr.Low == nil && sliceExpr.High == nil && !sliceExpr.Slice3 {
		return ident + "[..]"
	}

	// A 2-index expression with a bound the Range cannot carry exactly calls golib's slice method, which
	// checks the full value with Go's codes (S-c R1-A):
	//      s[i:j] => s.slice(i, j)      s[:n] => s.slice(0, n)      str[i:] => str.slice(i)
	// The receiver is exactly the expression the Range would have indexed (`(~p)`, a named array's `.Value`,
	// a deref-aliased parameter), so every receiver the Range reached has the method.
	if !sliceExpr.Slice3 {
		if call, ok := v.sliceMethodCall(ident, sliceExpr); ok {
			return call
		}
	}

	// sliceExpr[Low:] => sliceExpr[Low..]
	if sliceExpr.Low != nil && sliceExpr.High == nil && !sliceExpr.Slice3 {
		return ident + "[" + v.getRangeIndexer(sliceExpr.Low) + "..]"
	}

	// sliceExpr[:High] => sliceExpr[..High]
	if sliceExpr.Low == nil && sliceExpr.High != nil && !sliceExpr.Slice3 {
		return ident + "[.." + v.getRangeIndexer(sliceExpr.High) + "]"
	}

	// sliceExpr[Low:High] => sliceExpr[Low..High]
	if sliceExpr.Low != nil && sliceExpr.High != nil && !sliceExpr.Slice3 {
		return ident + "[" + v.getRangeIndexer(sliceExpr.Low) + ".." + v.getRangeIndexer(sliceExpr.High) + "]"
	}

	// sliceExpr[:High:Max] => sliceExpr.slice(0, High, Max). Go's omitted low IS 0, and golib's 3-index
	// `.slice(nint low, nint high, nint max)` has no sentinel (S-c R1-A): the -1 this emitted before read
	// as "omitted" only because the method it called treated a RUNTIME -1 the same way, which let
	// `s[neg:2:5]` slice silently. The method takes nint, so a High/Max bound of a wide integer
	// (uintptr/uint/…) is cast to nint (CS1503 otherwise — runtime/mprof `stk[:b.nstk:b.nstk]` with a
	// uintptr b.nstk); see castWideIntegerToNint for why nint and not int.
	if sliceExpr.Low == nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(0, " + v.castWideIntegerToNint(sliceExpr.High) + ", " + v.castWideIntegerToNint(sliceExpr.Max) + ")"
	}

	// sliceExpr[Low:High:Max] => sliceExpr.slice(Low, High, Max)
	if sliceExpr.Low != nil && sliceExpr.High != nil && sliceExpr.Slice3 {
		return ident + ".slice(" + v.castWideIntegerToNint(sliceExpr.Low) + ", " + v.castWideIntegerToNint(sliceExpr.High) + ", " + v.castWideIntegerToNint(sliceExpr.Max) + ")"
	}

	expr := v.getPrintedNode(sliceExpr)
	v.showWarning("@convSliceEpr - Failed to convert 'ast.SliceExpr' format %s", expr)
	return fmt.Sprintf("/* %s */", expr)
}

// sliceElemArrayDims answers the element ARRAY dimensions of a slice type, outermost first, or nil
// when the type is not a slice whose element is an array.
//
// This is the static fact the emission used to drop. Go's `[][3]uint8` and `[][4]uint8` are one
// managed type, `slice<array<byte>>` -- an array's length is a constructor argument in the runtime,
// not a type parameter -- so the length survives only where a value can be OBSERVED to carry it. An
// empty slice carries nothing, which is why `reflect.TypeOf([][3]uint8{})` could not answer its own
// element length. Here the length is still known, and withSliceElemDims below records it.
func sliceElemArrayDims(t types.Type) []int64 {
	if t == nil {
		return nil
	}

	slice, ok := t.Underlying().(*types.Slice)

	if !ok {
		return nil
	}

	var dims []int64

	for elem := slice.Elem(); ; {
		array, ok := elem.Underlying().(*types.Array)

		if !ok {
			break
		}

		dims = append(dims, array.Len())
		elem = array.Elem()
	}

	return dims
}

// withSliceElemDims wraps a slice CREATION expression so its element array dimensions are recorded
// against the new slice's backing store, and answers the expression unchanged for every other type.
//
// Only creation sites are wrapped: a reslice shares its source's backing and inherits the record for
// free, so nothing needs to travel through slicing, ranging or assignment.
//
// A NAMED slice type (`type UUIDs []UUID`) is answered unchanged: its wrapper struct is not a
// `slice<T>`, so wrapping it cannot bind (CS0411), and returning `slice<T>` would drop the name its
// methods hang on. Its creation sites record against the inner `slice<T>` the wrapper's constructor
// takes instead -- `new UUIDs(GoReflect.WithElemDims(new UUID[]{…}.slice(), 16))` -- which is the
// backing the wrapper shares, and so the same record (see sliceElemDimsWrap).
func (v *Visitor) withSliceElemDims(exprResult string, t types.Type) string {
	if _, isNamed := types.Unalias(t).(*types.Named); isNamed {
		return exprResult
	}

	prefix, suffix := sliceElemDimsWrap(t)
	return prefix + exprResult + suffix
}

// sliceElemDimsWrap answers the text that opens and closes a GoReflect.WithElemDims wrapper around a
// creation expression of slice type t, or two empty strings when t records nothing. The composite
// literal renderer assembles its text in pieces and so needs the halves rather than a finished wrap.
func sliceElemDimsWrap(t types.Type) (prefix string, suffix string) {
	dims := sliceElemArrayDims(t)

	if len(dims) == 0 {
		return "", ""
	}

	values := make([]string, len(dims))

	for i, dim := range dims {
		values[i] = fmt.Sprintf("%d", dim)
	}

	return "GoReflect.WithElemDims(", ", " + strings.Join(values, ", ") + ")"
}

// sliceBoundsNeedMethod reports whether a 2-index slice expression must call golib's slice method rather
// than index with a C# Range (S-c R1-A, docs/phase4/DESIGN-slice-bounds-r1a.md §5). A Range narrows each
// bound with (int) before golib sees it and cannot carry a negative one, so a non-constant bound, or a
// constant that does not fit int32, reaches Go's check only through the method. A constant within int32
// survives the (int) exactly (Go rejects a negative constant bound at compile time), so such a site keeps
// its Range, byte for byte.
func (v *Visitor) sliceBoundsNeedMethod(sliceExpr *ast.SliceExpr) bool {
	for _, bound := range []ast.Expr{sliceExpr.Low, sliceExpr.High} {
		if bound == nil {
			continue
		}

		if _, fits := v.int32ConstantBound(bound); !fits {
			return true
		}
	}

	return false
}

// int32ConstantBound answers a slice bound's constant value and whether it is a constant that fits int32.
func (v *Visitor) int32ConstantBound(bound ast.Expr) (int64, bool) {
	tv, ok := v.info.Types[bound]

	if !ok || tv.Value == nil {
		return 0, false
	}

	value, exact := constant.Int64Val(constant.ToInt(tv.Value))

	return value, exact && value >= math.MinInt32 && value <= math.MaxInt32
}

// sliceMethodCall renders a 2-index slice expression as golib's sentinel-free slice method when
// sliceBoundsNeedMethod says so: `x.slice(lo)` for `x[lo:]`, `x.slice(lo, hi)` for `x[lo:hi]`, and
// `x.slice(0, hi)` for `x[:hi]`, since Go's omitted low IS 0. Each bound is nint at its full value
// (castWideIntegerToNint; ruling U1 takes an unsigned bound through (nint), so one at or above 2^63 panics
// with a negative number in its text). A constant bound past int32 already renders as nint
// (`unchecked((nint)4294967301L)`), which the method takes as it is; the Range could not (CS0029).
func (v *Visitor) sliceMethodCall(ident string, sliceExpr *ast.SliceExpr) (string, bool) {
	if sliceExpr.Slice3 || (sliceExpr.Low == nil && sliceExpr.High == nil) || !v.sliceBoundsNeedMethod(sliceExpr) {
		return "", false
	}

	low := "0"

	if sliceExpr.Low != nil {
		low = v.castWideIntegerToNint(sliceExpr.Low)
	}

	if sliceExpr.High == nil {
		return fmt.Sprintf("%s.slice(%s)", ident, low), true
	}

	return fmt.Sprintf("%s.slice(%s, %s)", ident, low, v.castWideIntegerToNint(sliceExpr.High)), true
}

func (v *Visitor) getRangeIndexer(expr ast.Expr) string {
	if isIntegerLiteral(expr) {
		return v.convExpr(expr, nil)
	}

	return v.intCastOperand(expr, v.convExpr(expr, nil))
}

// intCastOperand wraps an already-converted expression in a C# `(int)` cast (for a slice bound, a
// shift count, …). When the operand's Go type is a NAMED numeric type (a `[GoType]` struct over a
// basic), a direct `(int)(x)` is CS0030 — the generated struct only converts to its OWN underlying
// basic — so it casts through the underlying first: `(int)(nuint)(x)`. A plain basic operand keeps
// the bare `(int)(x)` form (no churn).
// castWideIntegerToNint converts a 3-index slice's bound for the golib `.slice(low, high, max)`
// method, which takes `nint`. Go `int` (→ C# nint) and the small integer kinds bind directly and are
// left uncast; an unsigned 32-bit-or-wider or a 64-bit value (uint/uint32/uint64/uintptr/int64) does
// not convert implicitly (CS1503) and is cast to `nint` (through its underlying for a named numeric).
// It was cast to `int`, which TRUNCATED a bound past 2^31: `s[0:w:w]` with w = 1<<32+5 read
// `len 5 cap 5` where Go panics. `nint` is exact below 2^63; an unsigned bound at or above it reads
// negative and panics with a signed bound in the text (a stated residual: the three bounds can mix
// kinds, so no single unsigned overload set binds them).
func (v *Visitor) castWideIntegerToNint(expr ast.Expr) string {
	converted := v.convExpr(expr, nil)

	if exprType := v.getType(expr, false); exprType != nil {
		if basic, ok := exprType.Underlying().(*types.Basic); ok {
			switch basic.Kind() {
			case types.Uint, types.Uint32, types.Uint64, types.Uintptr, types.Int64:
				return v.numericCastOperand(expr, converted, "nint")
			}
		}
	}

	return converted
}

// fullValueIndexOperand renders an index whose consumer bounds-checks it BEFORE any narrowing, so
// the index keeps its full value, as Go's does: a string VARIABLE's indexers (int, nint and ulong),
// the golib element-address overloads `Ꮡ(x, i)` (int, nint and ulong), and the string-literal byte
// helper `LiteralByteAt`. An UNSIGNED index (uint/uint32/uint64/uintptr, named or not) is emitted
// BARE and binds the `ulong` overload, and a signed int64 takes `(nint)`, since long→nint does not
// convert implicitly. The `(int)` narrowing it replaced TRUNCATED: `s[uint64(1<<32+5)]` read s[5],
// and `&a[i]` at that index addressed a[5], where Go panics with the value and the length
// (goPanicIndex / goPanicIndexU).
func (v *Visitor) fullValueIndexOperand(expr ast.Expr) string {
	converted := v.convExpr(expr, nil)

	if exprType := v.getType(expr, false); exprType != nil {
		if basic, ok := exprType.Underlying().(*types.Basic); ok && basic.Kind() == types.Int64 {
			// A NAMED int64 converts through its OWN underlying first — the generated struct
			// only converts to that (the intCastOperand rule).
			if _, isNamed := exprType.(*types.Named); isNamed {
				return fmt.Sprintf("(nint)(%s)(%s)", v.getCSharpTypeName(basic), converted)
			}

			return fmt.Sprintf("(nint)(%s)", converted)
		}
	}

	return converted
}

func (v *Visitor) intCastOperand(expr ast.Expr, converted string) string {
	return v.numericCastOperand(expr, converted, "int")
}

// numericCastOperand is intCastOperand's rule for a C# integer target of the caller's choosing
// (`int` for a slice-range bound, `nint` for a 3-index bound).
func (v *Visitor) numericCastOperand(expr ast.Expr, converted, csType string) string {
	if named, ok := v.getType(expr, false).(*types.Named); ok {
		if basic, ok := named.Underlying().(*types.Basic); ok && basic.Info()&types.IsNumeric != 0 {
			return fmt.Sprintf("(%s)(%s)(%s)", csType, v.getCSharpTypeName(basic), converted)
		}
	}

	// A numeric TYPE PARAMETER operand — internal/trace's `dataTable[EI ~uint64]` shifting by
	// `id % 8` (the shift COUNT is coerced to int here) — has no C# cast to int (a constrained
	// type parameter is not directly convertible). Route through golib's ConvertToUInt64<T> bridge
	// (the E(100) integer-type-param family), then narrow — the same shape as the slice-index cast.
	if tp, ok := types.Unalias(v.getType(expr, false)).(*types.TypeParam); ok && typeParamIsInteger(tp) {
		return fmt.Sprintf("(%s)(ConvertToUInt64<%s>(%s))", csType, v.getCSharpTypeName(tp), converted)
	}

	return fmt.Sprintf("(%s)(%s)", csType, converted)
}

func isIntegerLiteral(expr ast.Expr) bool {
	if basicLit, ok := expr.(*ast.BasicLit); ok {
		if basicLit.Kind == token.INT {
			return true
		}
	}

	return false
}

func isPointerCast(expr string) (bool, string) {
	if strings.HasPrefix(expr, "(") {
		runes := []rune(expr)

		if isMatch, ptrType := isPointerExpr(string(runes[1:])); isMatch {
			return true, ptrType
		}
	}

	return false, ""
}

func isPointerExpr(expr string) (bool, string) {
	runes := []rune(expr)

	// Check if it starts with the expected prefix
	if len(runes) < 2 || string(runes[0:2]) != "ж<" {
		return false, ""
	}

	// Initialize variables
	bracketCount := 1 // We've already encountered one opening bracket
	startPos := 2     // Start after "ж<"

	// Scan through the runes to find the matching closing bracket
	for i := startPos; i < len(runes); i++ {
		if runes[i] == '<' {
			bracketCount++
		} else if runes[i] == '>' {
			bracketCount--

			if bracketCount == 0 {
				// Found the closing bracket for our initial '<'
				return true, string(runes[startPos:i])
			}
		}
	}

	return false, ""
}

// typeParamSliceCore returns the slice CORE type of a type parameter constrained `~[]E` (all of
// the constraint's type-set terms share the []E underlying), or nil. A type parameter's
// Underlying() is its constraint INTERFACE — the core type lives in the embedded terms.
func typeParamSliceCore(tp *types.TypeParam) *types.Slice {
	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok {
		return nil
	}

	var core *types.Slice

	for i := range iface.NumEmbeddeds() {
		switch et := iface.EmbeddedType(i).(type) {
		case *types.Union:
			for j := range et.Len() {
				if s, ok := et.Term(j).Type().Underlying().(*types.Slice); ok {
					core = s
				} else {
					return nil
				}
			}
		default:
			if s, ok := et.Underlying().(*types.Slice); ok {
				core = s
			}
		}
	}

	return core
}

// typeParamIsInteger reports whether every term of the type parameter's constraint type-set has
// an INTEGER underlying (`~int32 | ~int64` — rand.N's intType, reflect rangeNum's sets). Such a
// parameter routes Go conversions through golib's runtime-typed ConvertToType/ConvertToUInt64
// (no C# cast exists to/from a type parameter). Mirrors typeParamSliceCore's walk.
func typeParamIsInteger(tp *types.TypeParam) bool {
	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok {
		return false
	}

	found := false

	for i := range iface.NumEmbeddeds() {
		switch et := iface.EmbeddedType(i).(type) {
		case *types.Union:
			for j := range et.Len() {
				if basic, ok := et.Term(j).Type().Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
					found = true
				} else {
					return false
				}
			}
		default:
			if basic, ok := et.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
				found = true
			} else {
				return false
			}
		}
	}

	return found
}

// typeParamIsStringByteUnion reports whether the type parameter's constraint is exactly Go's
// `string | []byte` union (in either order) — the shape golib's IByteSeq<byte> models. Used to
// widen a FUNC-LITERAL parameter of that type to the interface (a lambda has no where-clause).
func typeParamIsStringByteUnion(tp *types.TypeParam) bool {
	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok {
		return false
	}

	sawString, sawByteSlice, terms := false, false, 0

	for i := range iface.NumEmbeddeds() {
		union, ok := iface.EmbeddedType(i).(*types.Union)

		if !ok {
			return false
		}

		for j := range union.Len() {
			terms++

			switch u := union.Term(j).Type().Underlying().(type) {
			case *types.Basic:
				if u.Info()&types.IsString != 0 {
					sawString = true
				} else {
					return false
				}
			case *types.Slice:
				if basic, ok := u.Elem().Underlying().(*types.Basic); ok && basic.Kind() == types.Byte {
					sawByteSlice = true
				} else {
					return false
				}
			default:
				return false
			}
		}
	}

	return sawString && sawByteSlice && terms == 2
}

// typeParamMapCore returns the map CORE type of a type parameter constrained `~map[K]V` (all of
// the constraint's type-set terms share the map underlying), or nil. Mirrors typeParamSliceCore.
func typeParamMapCore(tp *types.TypeParam) *types.Map {
	iface, ok := tp.Constraint().Underlying().(*types.Interface)

	if !ok {
		return nil
	}

	var core *types.Map

	for i := range iface.NumEmbeddeds() {
		switch et := iface.EmbeddedType(i).(type) {
		case *types.Union:
			for j := range et.Len() {
				if m, ok := et.Term(j).Type().Underlying().(*types.Map); ok {
					core = m
				} else {
					return nil
				}
			}
		default:
			if m, ok := et.Underlying().(*types.Map); ok {
				core = m
			}
		}
	}

	return core
}
