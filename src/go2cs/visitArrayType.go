// visitArrayType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
	"strings"
)

// Handles array and slice types in context of a TypeSpec
func (v *Visitor) visitArrayType(arrayType *ast.ArrayType, identType types.Type, name string, comment *ast.CommentGroup) {
	// Resolve the element type's C# name. A simple identifier element (e.g. `type d [3]rune`)
	// keeps its written name so the GoType attribute reads `[3]rune`; a composite element — a
	// selector, a (generic) instantiation, a pointer, etc. (e.g. `[N]atomic.Pointer[entry[K, V]]`)
	// — must be resolved through the type system, since getIdentifier would collapse it to just
	// its leading identifier (`atomic`), mangling the GoType and the generated array element.
	var csTypeName, goTypeName string

	// An anonymous-struct (or -interface) element of a NAMED array/slice type — `type semTable
	// [N]struct{…}` (runtime/sema) — must be lifted to a named type, otherwise the GoType
	// attribute and any `&t[i].field` (which emits `.of(ElemType.ᏑField)`) reference a raw,
	// un-compilable `struct{…}`. Lift it (named after the array type) and use the lifted name.
	if structType, ok := arrayType.Elt.(*ast.StructType); ok && !isEmptyStruct(structType) {
		eltType := v.info.TypeOf(arrayType.Elt)

		if !v.liftedTypeExists(structType) {
			liftedName := v.visitStructType(structType, eltType, name, nil, true, nil)
			csTypeName = liftedName
			goTypeName = liftedName
		} else if ln, ok := v.liftedTypeMap[eltType]; ok {
			csTypeName = ln
			goTypeName = ln
		}
	} else if interfaceType, ok := arrayType.Elt.(*ast.InterfaceType); ok && !interfaceType.Incomplete && len(interfaceType.Methods.List) > 0 {
		eltType := v.info.TypeOf(arrayType.Elt)

		if !v.liftedTypeExists(interfaceType) {
			liftedName := v.visitInterfaceType(interfaceType, eltType, name, nil, true, nil)
			csTypeName = liftedName
			goTypeName = liftedName
		} else if ln, ok := v.liftedTypeMap[eltType]; ok {
			csTypeName = ln
			goTypeName = ln
		}
	}

	if csTypeName != "" {
		// element already resolved to a lifted name above
	} else if ident := getIdentifier(arrayType.Elt); ident != nil && isSimpleIdentExpr(arrayType.Elt) && !v.liftedTypeExists(arrayType.Elt) {
		goTypeName = ident.Name
		csTypeName = convertToCSTypeName(goTypeName)
	} else if eltType := v.info.TypeOf(arrayType.Elt); eltType != nil {
		// Use the fully-qualified name (getFullyQualifiedTypeName) rather than the package-aliased form: the
		// GoType attribute is consumed by the generated array-backed partial, which lives in a file
		// without this file's package-relative `using` aliases (e.g. `using atomic = ...`), so an
		// aliased element type would be unresolvable there (CS0246). The leading namespace segment
		// must be the CANONICAL qualifier, never a file-local Δ collision-rename (same rule as the
		// visitTypeSpec global-using target).
		csTypeName = canonicalizeQualifierRename(convertToCSTypeName(v.getFullyQualifiedTypeName(eltType, false)))
		goTypeName = csTypeName
	} else {
		typeName := v.getPrintedNode(arrayType.Elt)
		v.showWarning("@visitArrayType - Failed to resolve 'ast.ArrayType' element %s", typeName)
		v.writeOutputLn("// [...]%s", typeName)
		return
	}

	access := v.consumePendingTypeAccess()

	// A slice/array type declared inside a function body (`type People []Person`, example_test.go)
	// cannot be a method-body statement in C#; hoist it to member level (see liftLocalTypeDecl). A
	// package-level declaration is unaffected — target is v.outputBuilder and finish() is a no-op.
	preLiftName := name
	name, target, finish := v.liftLocalTypeDecl(name, identType)

	// The element was resolved ABOVE, i.e. before this declaration's own hoist registered its
	// lifted name — so a SELF-referential local type (`type recursiveSlice []recursiveSlice`,
	// gob's encoder_test.go) rendered the source name, which no longer exists at member level
	// (CS0246 in the generated array partial). Re-resolve the element through liftedTypeMap
	// whenever a hoist actually renamed the declaration; a package-level declaration never
	// renames, so its emission is unchanged.
	if name != preLiftName {
		if eltType := v.info.TypeOf(arrayType.Elt); eltType != nil {
			if liftedElt, ok := v.liftedTypeMap[eltType]; ok {
				csTypeName, goTypeName = liftedElt, liftedElt
			}
		}
	}

	typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName))

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	if arrayType.Len == nil {
		// Handle slice type
		v.writeString(target, "%s[GoType(\"[]%s\")] ", v.localNameAttrFor(identType), csTypeName)
	} else {
		// Handle array type
		var arrayLenValue string
		arrayLenExpr := v.convExpr(arrayType.Len, nil)

		// Check if length expression is in type information
		if tv, ok := v.info.Types[arrayType.Len]; ok {
			// Check if it's a constant
			if tv.Value != nil {
				if length, ok := constArrayLength(tv.Value); ok {
					arrayLenValue = length
				}
			}
		}

		if len(arrayLenValue) > 0 && arrayLenValue != arrayLenExpr {
			// arrayLenExpr may itself carry a `/* Go source */` comment from a folded
			// unsafe.Sizeof/Offsetof/Alignof sub-expression (foldUnsafeConstBuiltin) — fine as the
			// SOLE comment on a line (a const declaration site) but not here: this annotation wraps
			// the WHOLE expression in ITS OWN outer comment, and C# does not nest block comments
			// (runtime's `largePointer` array, whose length folds unsafe.Sizeof, emitted an outer
			// comment that terminated at the fold's OWN "*/", spilling the rest as raw code —
			// W2c). Strip the embedded delimiters; the readable Go text between them is unaffected,
			// now sitting directly inside the one outer comment instead of a redundant nested one.
			annotatedLenExpr := strings.NewReplacer("/*", "", "*/", "").Replace(arrayLenExpr)
			v.writeString(target, "%s[GoType(\"[%s]%s\")] /* [%s]%s */%s", v.localNameAttrFor(identType), arrayLenValue, csTypeName, annotatedLenExpr, csTypeName, v.newline)
		} else {
			v.writeString(target, "%s[GoType(\"[%s]%s\")] ", v.localNameAttrFor(identType), arrayLenExpr, csTypeName)
		}
	}

	// Append generic type parameters and constraints (e.g. `<K, V> where K : new()`) for a generic
	// named array type so the forward declaration matches its uses, and the constraints propagate
	// type-wide to the generated array-backed partial (whose element type may require them).
	typeParams, constraints := v.getGenericDefinition(identType)

	v.recordTypeAccessibility("struct", getSanitizedIdentifier(name), typeParams, access, "")
	v.writeString(target, "%s%spartial struct %s%s%s;", namedArrayElemDimsAttr(identType), access, getSanitizedIdentifier(name), typeParams, constraints)
	v.writeCommentString(target, comment, arrayType.Elt.End()+typeLenDeviation)
	target.WriteString(v.newline)
	finish()
}

// constArrayLength renders the exact decimal length of a CONSTANT array-length expression, or
// reports false when the constant is not an integer the emission can carry (the caller then keeps
// the converted length EXPRESSION, which is what it already does for a non-constant length).
//
// It exists because go/types does NOT normalise an array length's recorded constant value, and
// `constant.Int64Val` PANICS — it does not return `(0, false)` — for any value that is neither Int
// nor Unknown kind. `Checker.arrayLength` (go/types/typexpr.go:536) applies `constant.ToInt` to a
// LOCAL copy purely to test representability and records the operand's own, un-normalised value, so
// the perfectly legal
//
//	const S = 1e6
//	var x [S]byte          // reflect/all_test.go:5185, TestSliceOverflow
//
// leaves `info.Types[arrayType.Len].Value` at **Float** kind (`1e+06`) — and a zero-imaginary
// complex length (`const C = 1e1 + 0i; var x [C]byte`, equally legal) leaves it at **Complex** kind.
// Both panicked the three array-length sites; `constant.ToInt` folds either into Int, and yields
// unknownVal — hence `(0, false)`, never a panic — for anything it cannot.
//
// This is the ONE reachable arm of a two-position class. go/types leaves the recorded value
// un-normalised wherever the spec says a constant must be *representable as* an integer rather than
// *converted to* one, which is exactly two positions: array lengths and SHIFT COUNTS (`x << 1e0` is
// legal and records Float kind too — measured). The shift arm already normalises correctly
// (`constIntShiftValue`, convBinaryExpr.go), which is why only the array arm ever panicked. Every
// other integer-required position — index, slice bound, `make` length/cap, composite-literal array
// key — IS converted by go/types (`isValidIndex` → `convertUntyped`) and records Int kind, so none
// of them can reach this shape; measured across 24 probe shapes rather than assumed.
//
// The remaining array-typed emissions are structurally immune and deliberately untouched: a struct
// field, a func parameter/result, a composite-literal type, a pointer/map/slice element and a type
// alias all resolve their length through `types.Array.Len()`, an `int64` go/types has already
// normalised, never through the AST node's recorded constant.
func constArrayLength(value constant.Value) (string, bool) {
	intValue := constant.ToInt(value)

	if intValue.Kind() != constant.Int {
		return "", false
	}

	length, exact := constant.Int64Val(intValue)

	if !exact {
		return "", false
	}

	return strconv.FormatInt(length, 10), true
}

// namedArrayElemDimsAttr renders the `[GoArrayDims(...)]` stamp a NAMED fixed-array type needs when
// its ELEMENT is itself an unnamed fixed array (`type nn [2][3]int`), or "" for every other shape.
//
// It is the one fact about such a type that reaches C# nowhere else. The wrapper's descriptor is
// `[2]array<nint>` — the inner 3 is gone — and go2cs-gen builds the wrapper's backing lazily as
// `new array<array<nint>>(2)`, i.e. two elements of `default(array<nint>)`: a LENGTH-ZERO array
// where Go has three zeroed ints. Nothing downstream can repair that, because an `array<T>`'s length
// is INSTANCE state and the lazy backing has no instance to read it from — which is exactly why this
// is a converter stamp rather than a golib or generator inference. Measured before the fix, against
// `go run`: `var d nn` printed `2 0 [[] []]` where Go prints `2 3 [[0 0 0] [0 0 0]]`, and the first
// indexed write panicked.
//
// Emitted ONLY for a nested-array element, and the discriminator is exact rather than approximate:
// goArrayDims walks unnamed arrays only, so it returns 2+ dimensions precisely when the element is
// one — one dimension for `[4]byte`, for `[2]wa` over a struct, and for `[2]ni` over a NAMED array
// (whose own wrapper allocates its own backing, so it needs nothing). The corpus census at this cut
// puts that population at ZERO of 59 named array wrappers, so the stamp costs the emission nothing
// today; a struct element needs no cargo at all, because go2cs-gen already owns that predicate and
// owns the cross-ASSEMBLY half of it the converter could not supply.
func namedArrayElemDimsAttr(namedType types.Type) string {
	if namedType == nil {
		return ""
	}

	if dims := goArrayDims(namedType.Underlying()); len(dims) > 1 {
		return emitGoArrayDimsAttribute(namedType.Underlying())
	}

	return ""
}

// arrayZeroValueArgs renders the constructor arguments for a fixed-size array's zero value: the
// length, plus an element factory when `default(T)` is not usable storage for the element type.
//
// golib's `new array<T>(N)` fills its backing with `default(T)`, which is only the correct Go zero
// value when `default(T)` is itself well formed. It is NOT when the element is:
//
//   - another UNNAMED fixed-size array — `[2][4]byte` emits `array<array<byte>>`, and the inner
//     length lives only in the Go type, never in `array<T>`, so every element would keep a null
//     backing: `len(x[1])` reports 0 (Go says 4) and the first indexed write panics; or
//   - a struct whose own zero value needs construction — `default(T)` skips the generated
//     constructor that runs its fixed-array field initializers and allocates its embed boxes.
//
// A NAMED array element (`type row [4]byte`) needs no factory: its generated wrapper allocates its
// backing lazily from its own known size (go2cs-gen's `m_value ??= new row(4)`).
//
// Mirrors go2cs-gen's AppendZeroValueInitializers, which does the same for struct FIELDS. Every
// other element type renders the bare length, so only genuinely nested shapes change.
func (v *Visitor) arrayZeroValueArgs(lengthExpr string, arrayType types.Type) string {
	if arrayType == nil {
		return lengthExpr
	}

	array, ok := arrayType.Underlying().(*types.Array)

	if !ok {
		return lengthExpr
	}

	return arrayLengthArgs(lengthExpr, v.arrayElemFactory(array.Elem()))
}

// arrayLengthArgs is the ONE place the `array<T>` length-plus-factory argument list is spelled:
// the length alone when `default(T)` is already the element's Go zero value, and the length plus
// the construction lambda when it is not.
//
// Four renderers reach the same argument list from different directions — the zero-value ladder
// (arrayZeroValueArgs), the composite literal's short-literal padding, the constant-indexed
// literal's `array<T>(N){[i] = v}` form, and the named-array wrapper's — and the defect this
// closes was precisely that they did not all carry the factory. Spelling it once is what keeps a
// fifth caller from being added without it.
func arrayLengthArgs(lengthExpr string, elemFactory string) string {
	if elemFactory == "" {
		return lengthExpr
	}

	return fmt.Sprintf("%s, () => %s", lengthExpr, elemFactory)
}

// zeroValueInitializer renders the initializer a DECLARATION SITE must use for a type's Go zero
// value — the `<expr>` in `T name = <expr>;`. C# `default` runs neither a constructor nor a field
// initializer, so it is the correct Go zero value only for a type whose all-bits-zero form is
// already usable storage. Three shapes are not, and each has an existing construction route:
//
//   - an UNNAMED fixed-size array (`[16]byte`): golib's `array<T>` carries its length in the
//     constructed instance, so `default(array<T>)` has length 0 and a null backing where Go has N
//     zeroed elements. `new(N)` (plus arrayZeroValueArgs' element factory for a nested/needy
//     element) builds it. A NAMED array type is excluded — its generated wrapper allocates its
//     backing lazily from its own known size, exactly as arrayElemFactory documents;
//   - a struct with a PROMOTED EMBED, whose readonly `ж<T>` box exists only when a constructor
//     runs — `new(nil)`;
//   - a struct carrying a fixed-array field at any depth, whose `= new(N)` field initializer only
//     runs inside an explicitly declared constructor — `new()` (go2cs-gen always emits the
//     parameterless one for this reason).
//
// This is the single ladder every zero-value declaration site shares: the local and global `var x
// T` paths (visitValueSpec) and the named-result prologues (visitFuncDecl, iifeOperations). The
// value-spec paths resolve an array's length from the AST first so a symbolic length keeps its
// `/* bufSize */` comment; this types-only form is what a site with no type syntax in hand uses.
func (v *Visitor) zeroValueInitializer(t types.Type) string {
	if t == nil {
		return "default!"
	}

	if _, isNamed := types.Unalias(t).(*types.Named); !isNamed {
		if array, isArray := t.Underlying().(*types.Array); isArray {
			return fmt.Sprintf("new(%s)", v.arrayZeroValueArgs(strconv.FormatInt(array.Len(), 10), array))
		}
	}

	// A DIRECTIONAL channel's zero value is the nil channel OF THAT TYPE, and its direction is the
	// one part `default(channel<T>)` cannot express — the same shape as the array length above, and
	// carried the same way (see chanDirectionCargo.go). Every other channel keeps `default!`.
	if nilChan := v.chanDirNilValue(t); nilChan != "" {
		return nilChan
	}

	if v.structHasPromotedEmbeds(t) {
		return "new(nil)"
	}

	if v.structZeroValueNeedsConstruction(t) {
		return "new()"
	}

	return "default!"
}

// arrayElemFactory renders the target-typed construction expression for one element of a
// fixed-size array, or "" when `default(T)` is already the correct zero value. See
// arrayZeroValueArgs for which element shapes need one.
func (v *Visitor) arrayElemFactory(elemType types.Type) string {
	if elemType == nil {
		return ""
	}

	// A NAMED element keeps its own zero-value handling — an array wrapper allocates its backing
	// lazily, and a struct routes through the constructor forms below — so only an unnamed nested
	// array needs its length threaded through here.
	if _, isNamed := types.Unalias(elemType).(*types.Named); !isNamed {
		if innerArray, isArray := elemType.Underlying().(*types.Array); isArray {
			return fmt.Sprintf("new(%s)", v.arrayZeroValueArgs(strconv.FormatInt(innerArray.Len(), 10), innerArray))
		}
	}

	// Mirrors the zero-value construction the local/global variable paths already emit for these
	// struct shapes (a promoted embed's readonly `ж<T>` box exists only when a constructor runs).
	if v.structHasPromotedEmbeds(elemType) {
		return "new(nil)"
	}

	if v.structZeroValueNeedsConstruction(elemType) {
		return "new()"
	}

	return ""
}

// isSimpleIdentExpr reports whether an expression is a bare identifier (not a selector, index,
// star, etc.), so a single-identifier array element keeps its written name in the GoType attribute.
func isSimpleIdentExpr(expr ast.Expr) bool {
	_, ok := expr.(*ast.Ident)
	return ok
}

// canonicalizeQualifierRename strips a file-local import collision-rename (Δ) from the LEADING
// namespace segment of a fully-qualified type name destined for a GoType descriptor: the
// generated partial that consumes the descriptor has no file-local using aliases, so the
// segment must be the CANONICAL package qualifier (`IoLike.FsLike_package.Info` roots through
// the go namespace), never the renamed alias (`ΔIoLike.…` resolves nowhere in the .g.cs).
// Mirrors the visitTypeSpec global-using-target un-rename; a Δ-renamed TYPE segment is left
// untouched — only an entry the import-rename map produced is reverted.
func canonicalizeQualifierRename(typeName string) string {
	if seg, rest, found := strings.Cut(typeName, "."); found {
		if canonical, wasRenamed := strings.CutPrefix(seg, ShadowVarMarker); wasRenamed && packageImportAliasRenames[canonical] == seg {
			return canonical + "." + rest
		}
	}

	return typeName
}
