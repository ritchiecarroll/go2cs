// zeroSizeFieldLayout.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/types"
	"strings"
)

// ZERO-SIZE FIELD LAYOUT (Ruling A, board 2026-08-20)
//
// A Go `struct{}` field occupies NO bytes. A C# field always occupies at least one, so a Go struct
// carrying a zero-size field has a C# surrogate that is LARGER than Go's — `sync/atomic.Int32` is
// `struct{ _ noCopy; v int32 }`, four bytes in Go with `v` at offset 0, and eight in C# once the
// empty struct takes a byte and alignment pushes `v` to offset 4.
//
// That is not cosmetic. `Reinterpret`'s size guard admits an alias only when
// `SizeOf(TDst) <= SizeOf(T)` — the guard is correct and the ruling forbids relaxing it — so the
// Go-legal `(*Int32)(unsafe.Pointer(uaddr))` over a `*uint32` is refused at 8 > 4, the atomic
// operations act on a DETACHED COPY, and loads answer zero. Emitting Go's OWN offsets under
// `[StructLayout(LayoutKind.Explicit)]` restores the size, and with it the alias.
//
// TWO LIMITS, BOTH MEASURED RATHER THAN ASSUMED
//
//  1. It applies to UNMANAGED structs only. .NET forbids overlapping a managed reference with
//     anything, so a struct holding a pointer, interface, slice, map, chan, func or string cannot
//     take Go's offsets at all — the overlap is a TypeLoadException, not a layout. A census of the
//     Go 1.23.1 standard library puts 59 of the 90 structs with zero-size fields in exactly that
//     category, so this is the common case rather than a corner: the emission must classify, never
//     assume.
//
//  2. The zero-size fields are emitted READONLY. Laid out at Go's offset a zero-size field SHARES
//     bytes with the field Go places there, and assigning it writes its one C# byte over the
//     neighbour (measured, GolibTests.ZeroSizeFieldLayoutTests: 42 -> 0). Go's write writes nothing.
//     `readonly` makes that one unfaithful operation unexpressible rather than merely unlikely,
//     while the field stays DECLARED so reflect's field walk, NumField() and StructField.Offset
//     still agree with Go. A whole-struct assignment writes all Size bytes and stays correct.
//
// go2cs-gen's TypeGenerator skips the `Ꮡ<field>` accessor for the whole all-underscores blank family
// (see IsGoBlankMemberName), which is what keeps a readonly blank field from needing a writable ref.

// underlyingStruct resolves the `*types.Struct` behind a declared struct type, through a named type
// or an alias. Reports nil for anything else, which drops the caller out of the layout arc.
func (v *Visitor) underlyingStruct(t types.Type) *types.Struct {
	if t == nil {
		return nil
	}

	structType, _ := types.Unalias(t).Underlying().(*types.Struct)
	return structType
}

// goLayoutSizer is the Go layout authority for the conversion's target architecture — the same
// `types.Sizes` go/types folds `unsafe.Sizeof`/`Offsetof` against, so an emitted offset and a folded
// constant can never describe one struct differently.
func (v *Visitor) goLayoutSizer() types.Sizes {
	parts := strings.Split(v.options.targetPlatform, "/")

	if len(parts) != 2 {
		return types.SizesFor("gc", "amd64")
	}

	if sizes := types.SizesFor("gc", parts[1]); sizes != nil {
		return sizes
	}

	return types.SizesFor("gc", "amd64")
}

// structZeroSizeLayout describes the explicit layout a struct needs, or reports ok=false when the
// struct needs none (no zero-size field) or cannot take one (a managed field anywhere).
type structZeroSizeLayout struct {
	size     int64
	offsets  []int64
	zeroSize []bool
}

func (v *Visitor) structZeroSizeLayout(structType *types.Struct, named types.Type) (layout structZeroSizeLayout, ok bool) {
	if structType == nil || structType.NumFields() == 0 {
		return layout, false
	}

	sizes := v.goLayoutSizer()

	// go/types panics rather than erroring on a type whose size is not computable (a type parameter,
	// an invalid type). The honest answer for such a struct is "no explicit layout" — the r39d rule
	// that a fact which cannot be read truthfully stays unstated — so the whole measurement runs
	// under a recover and any panic drops the struct out of the arc.
	defer func() {
		if recover() != nil {
			layout, ok = structZeroSizeLayout{}, false
		}
	}()

	fields := make([]*types.Var, structType.NumFields())
	zeroSize := make([]bool, structType.NumFields())
	anyZero := false

	for i := range structType.NumFields() {
		field := structType.Field(i)
		fields[i] = field

		// An EMBEDDED field is excluded, and the reason is that this predicate reads GO types while
		// the constraint is about the C# STORAGE: go2cs may store a promoted embed as a ж<T> backing
		// box under a marker-prefixed name, which is a managed reference that explicit layout may not
		// place. A Go-type walk cannot see that choice, so the honest classification of an embed is
		// "unknown", and unknown drops out of the arc. It costs real candidates — `runtime.mutex`
		// embeds `lockRankStruct`, `sync.WaitGroup` embeds `noCopy` — and the alternative is emitting
		// a layout whose legality depends on an emission decision made elsewhere.
		if field.Embedded() {
			return structZeroSizeLayout{}, false
		}

		if csManagedField(field.Type(), 0) {
			return structZeroSizeLayout{}, false
		}

		if sizes.Sizeof(field.Type()) == 0 {
			// A zero-size field is emitted READONLY so it cannot write its one C# byte over the field
			// it shares an offset with — but only a BLANK one may be. Go makes `&s.pad` a legal
			// pointer for a NAMED zero-size field, so go2cs-gen emits a writable `Ꮡpad` ref accessor
			// for it, and a writable ref to a readonly field is CS8160 (measured: the
			// ReflectStructTagCopy behavioral test, whose `layout` carries a named `pad empty`).
			//
			// Blank fields have no such accessor — Go says they have no address at all, and the
			// generator has always skipped them — so they take the readonly form safely. A struct
			// with a NAMED zero-size field therefore leaves the arc entirely rather than choosing
			// between a broken accessor and a field that can silently corrupt its neighbour.
			if field.Name() != "_" {
				return structZeroSizeLayout{}, false
			}

			zeroSize[i] = true
			anyZero = true
		}
	}

	if !anyZero {
		return structZeroSizeLayout{}, false
	}

	size := sizes.Sizeof(named)

	// A struct whose Go size is ZERO (every field zero-size) has no C# expression: the CLR's minimum
	// struct size is one byte and `Size = 0` means "natural size", not "empty". Emitting nothing is
	// truthful; emitting a fabricated 1 would claim a Go size that is not Go's.
	if size <= 0 {
		return structZeroSizeLayout{}, false
	}

	return structZeroSizeLayout{size: size, offsets: sizes.Offsetsof(fields), zeroSize: zeroSize}, true
}

// csManagedField reports whether a Go type becomes a MANAGED C# type — one the CLR tracks as a
// reference. Explicit layout may not overlap such a field with anything, which is what bounds this
// whole arc to the unmanaged subset.
func csManagedField(t types.Type, depth int) bool {
	if depth > 12 {
		return true // conservative: an unresolvable shape is treated as managed and skipped
	}

	switch u := t.Underlying().(type) {
	case *types.Basic:
		// `string` is golib's @string (a managed struct over a byte slice) and `unsafe.Pointer` is a
		// box; every other basic is a blittable primitive.
		return u.Kind() == types.String || u.Kind() == types.UnsafePointer
	case *types.Pointer, *types.Interface, *types.Slice, *types.Map, *types.Chan, *types.Signature:
		return true
	case *types.Array:
		// A Go array becomes golib's `array<T>` — a struct over a SHARED T[] backing, so it is
		// managed whatever its element is.
		return true
	case *types.Struct:
		for i := range u.NumFields() {
			if csManagedField(u.Field(i).Type(), depth+1) {
				return true
			}
		}

		return false
	}

	return true
}

// structLayoutAttribute renders the struct-level stamp.
func (l structZeroSizeLayout) structLayoutAttribute() string {
	return fmt.Sprintf("[StructLayout(LayoutKind.Explicit, Size = %d)] ", l.size)
}

// fieldOffsetAttribute renders a field's offset stamp.
func (l structZeroSizeLayout) fieldOffsetAttribute(index int) string {
	if index < 0 || index >= len(l.offsets) {
		return ""
	}

	return fmt.Sprintf("[FieldOffset(%d)] ", l.offsets[index])
}

// fieldIsZeroSize reports whether the field at index occupies no Go bytes, and therefore must be
// emitted readonly so it cannot write over the field it shares an offset with.
func (l structZeroSizeLayout) fieldIsZeroSize(index int) bool {
	return index >= 0 && index < len(l.zeroSize) && l.zeroSize[index]
}
