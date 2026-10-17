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
	"go/ast"
	"go/token"
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
//     A Go STORE to a named zero-size field (`x.Z = v`) is legal Go, so it cannot be left as a C#
//     store onto the readonly field (CS0191): it is lowered through the generated accessor,
//     `T.ᏑZ(ref <base>) = v`, which answers the shared zero-size slot. The base is still evaluated
//     (a nil pointer or an out-of-range index still panics), the right side still runs, and no byte
//     of the struct is written (see markZeroSizeFieldStores).
//
// go2cs-gen's TypeGenerator skips the `Ꮡ<field>` accessor for the whole all-underscores blank family
// (see IsGoBlankMemberName), which is what keeps a readonly blank field from needing a writable ref. A
// NAMED readonly zero-size field keeps its accessor, which returns golib's shared per-type zero-size slot
// (see structZeroSizeLayout).

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

		// An EMBEDDED field takes part like any other. It used to be excluded because go2cs-gen stored a
		// promoted embed in a generated backing field this arc could not place; since owner ruling (A)
		// (2026-10-16) the converter declares every embed as a field itself (visitStructType), so its
		// storage is the Go type's own, and the managed check below classifies it exactly as it does a
		// named field: a pointer embed is managed and drops the struct out, a value embed of an unmanaged
		// struct stays in. runtime's lfstack-list node `struct{ _ sys.NotInHeap; listedValManual }` is the
		// shape this admits: Go lays both at offset 0, and sequential layout gave the blank a CLR byte.
		if csManagedField(field.Type(), 0) {
			return structZeroSizeLayout{}, false
		}

		if sizes.Sizeof(field.Type()) == 0 {
			// A zero-size field is emitted READONLY so it cannot write its one C# byte over the field
			// it shares an offset with. A BLANK one has no accessor (Go says it has no address). A NAMED
			// one does: Go makes `&s.pad` a legal pointer, and a writable ref to a readonly field is
			// CS8160 (measured: the ReflectStructTagCopy behavioral test, whose `layout` carries a named
			// `pad empty`). So go2cs-gen's `Ꮡpad` for a readonly zero-size member returns a ref to golib's
			// shared per-type zero-size slot (GoZeroSizeSlot), never `ref instance.pad` (A17, COORD
			// 2026-09-28). That is faithful: a write through a zero-size address stores nothing, and the
			// pointer's IDENTITY is not the slot -- a FieldRefBox names (source, field), and for a Go
			// zero-size T its uintptr is the order token (base + Go offset), never the slot's address.
			//
			// Before that door a struct with a NAMED zero-size field left the arc entirely, which is
			// what kept internal/runtime/atomic's `noCopy noCopy` carriers (and the page allocator's
			// atomicScavChunkData through them) at twice Go's size.
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

// zeroSizeFieldStoreAccessor answers the generated accessor (`T.ᏑZ`) for a selector that names a
// field emitted READONLY by this arc: a DIRECT field (not promoted through an embed, which the arc
// excludes anyway) whose owning struct takes explicit layout and whose type is zero-size. It is the
// same decision visitStructType makes when it writes `readonly`, asked of the use site.
func (v *Visitor) zeroSizeFieldStoreAccessor(sel *ast.SelectorExpr) (string, bool) {
	selection, ok := v.info.Selections[sel]

	if !ok || selection.Kind() != types.FieldVal || len(selection.Index()) != 1 {
		return "", false
	}

	owner := types.Unalias(selection.Recv())

	if pointer, isPointer := owner.(*types.Pointer); isPointer {
		owner = types.Unalias(pointer.Elem())
	}

	layout, hasLayout := v.structZeroSizeLayout(v.underlyingStruct(owner), owner)

	if !hasLayout || !layout.fieldIsZeroSize(selection.Index()[0]) {
		return "", false
	}

	typeName := convertToCSTypeName(v.getAliasQualifiedTypeName(owner, false))
	return fmt.Sprintf("%s.%s%s", v.boxAccessorType(typeName, "", owner), AddressPrefix, v.structFieldBoxName(sel.Sel, sel.X)), true
}

// markZeroSizeFieldStores records each assignment target that stores to a readonly zero-size layout
// field, so that however the assignment is emitted (single, tuple, declaration-mixed), the one place a
// target selector is converted, convSelectorExpr, lowers it through the accessor.
func (v *Visitor) markZeroSizeFieldStores(assignStmt *ast.AssignStmt) {
	if assignStmt.Tok != token.ASSIGN {
		return
	}

	for _, lhs := range assignStmt.Lhs {
		sel, isSelector := ast.Unparen(lhs).(*ast.SelectorExpr)

		if !isSelector {
			continue
		}

		if accessor, ok := v.zeroSizeFieldStoreAccessor(sel); ok {
			if v.zeroSizeFieldStores == nil {
				v.zeroSizeFieldStores = map[*ast.SelectorExpr]string{}
			}

			v.zeroSizeFieldStores[sel] = accessor
		}
	}
}

// zeroSizeFieldStoreTarget rewrites a marked target's plain emission `<base>.Z` to
// `T.ᏑZ(ref <base>)`. A plain emission that does not end in the field's own name is left untouched:
// the store then stays a compile error rather than becoming a silent wrong write.
func (v *Visitor) zeroSizeFieldStoreTarget(sel *ast.SelectorExpr, accessor, plain string) string {
	if base, found := strings.CutSuffix(plain, "."+v.structFieldBoxName(sel.Sel, sel.X)); found && base != "" {
		return fmt.Sprintf("%s(ref %s)", accessor, base)
	}

	return plain
}
