// cgoMirrors.go - Gbtc
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

// Phase 2 of docs/PLAN-cgo-interop.md, increment 2 (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §5.1, read through §11):
// the blittable C# mirror of a C struct. cmd/cgo lays every _Ctype_struct_x out exactly as C does, from the compiler's
// DWARF and with explicit padding fields, so types.Sizes for the target reproduces the C size and every field offset,
// and the mirror is generated from go/types with no C parser. Nothing here is emitted into a conversion yet: the
// wrappers that cross with a mirror are increment 3.

// cgoMirrorSuffix ends a generated mirror's C# name, and cgoMirrorArrayPrefix starts an inline-array helper's. A
// C-derived name is ASCII, so neither can collide with a name cmd/cgo declares.
const (
	cgoMirrorSuffix      = "ᴍ"
	cgoMirrorArrayPrefix = "ᴍarray"
)

// cgoMirror is the mirror of one C struct for one target: its C# name, its C size, and its members in field order.
type cgoMirror struct {
	goName string
	name   string
	size   int64
	fields []cgoMirrorField
}

// cgoMirrorField is one mirror member: its offset, its C# type, and the size go/types gives it, which the layout check
// holds the C# type to. An inline-array member also carries its length and its element's type and size.
type cgoMirrorField struct {
	name     string
	offset   int64
	clrType  string
	goSize   int64
	length   int64
	elemType string
	elemSize int64
}

// cgoInlineArray is one [InlineArray(N)] helper: N elements of one C# type.
type cgoInlineArray struct {
	name     string
	length   int64
	elemType string
}

// cgoMirrorBasic maps each basic kind a cgo-declared C type can have to the fixed-width C# type of the same size. C's
// _Bool is byte, never bool, which marshals as a four-byte BOOL (§11.13). Go's int and uint never occur in a cgo
// struct (cmd/cgo spells C's widths explicitly), so they are left unmapped and refused.
var cgoMirrorBasic = map[types.BasicKind]string{
	types.Bool:          "byte",
	types.Int8:          "sbyte",
	types.Int16:         "short",
	types.Int32:         "int",
	types.Int64:         "long",
	types.Uint8:         "byte",
	types.Uint16:        "ushort",
	types.Uint32:        "uint",
	types.Uint64:        "ulong",
	types.Float32:       "float",
	types.Float64:       "double",
	types.Uintptr:       "nuint",
	types.UnsafePointer: "nint",
}

// cgoMirrorBuilder makes the mirrors, and the inline-array helpers they use, for one target's sizes.
type cgoMirrorBuilder struct {
	sizes   types.Sizes
	mirrors map[string]*cgoMirror
	order   []string
	arrays  map[string]cgoInlineArray
	arrayAt []string

	// fieldTypeOverride replaces one member's C# type when it answers true: a test-only seam, so the planted underflow
	// of §11.2 can be generated and shown to be refused. Nothing in the converter sets it.
	fieldTypeOverride func(structName, fieldName string) (string, bool)
}

func newCgoMirrorBuilder(sizes types.Sizes) *cgoMirrorBuilder {
	return &cgoMirrorBuilder{sizes: sizes, mirrors: map[string]*cgoMirror{}, arrays: map[string]cgoInlineArray{}}
}

// mirror returns the mirror of named, a C struct cmd/cgo declared, making it and every mirror and helper it needs. It
// refuses, by name, what a blittable mirror cannot hold: a flexible array member, a complex number, and anything that
// would hold a managed reference.
func (builder *cgoMirrorBuilder) mirror(named *types.Named) (*cgoMirror, error) {
	goName := named.Obj().Name()

	if built, found := builder.mirrors[goName]; found {
		return built, nil
	}

	structType, ok := named.Underlying().(*types.Struct)

	if !ok {
		return nil, fmt.Errorf("cgo mirror: %s is not a C struct", goName)
	}

	vars := make([]*types.Var, structType.NumFields())

	for i := range vars {
		vars[i] = structType.Field(i)
	}

	offsets := builder.sizes.Offsetsof(vars)
	built := &cgoMirror{goName: goName, name: goName + cgoMirrorSuffix, size: builder.sizes.Sizeof(structType)}

	for i, field := range vars {
		// cmd/cgo's padding: those bytes stay opaque in the mirror, as Go cannot read them either.
		if field.Name() == "_" {
			continue
		}

		member, err := builder.member(goName, field.Name(), field.Type())

		if err != nil {
			return nil, err
		}

		if builder.fieldTypeOverride != nil {
			if planted, found := builder.fieldTypeOverride(goName, field.Name()); found {
				member.clrType = planted
			}
		}

		member.name = getCoreSanitizedIdentifier(field.Name())
		member.offset = offsets[i]
		built.fields = append(built.fields, member)
	}

	builder.mirrors[goName] = built
	builder.order = append(builder.order, goName)

	return built, nil
}

// member works out the C# type of one member of owner, and the size go/types gives it.
func (builder *cgoMirrorBuilder) member(owner, field string, fieldType types.Type) (cgoMirrorField, error) {
	refuse := func(reason string) (cgoMirrorField, error) {
		return cgoMirrorField{}, fmt.Errorf("cgo mirror: %s.%s is %s, which Phase 2 cannot mirror", owner, field, reason)
	}

	member := cgoMirrorField{goSize: builder.sizes.Sizeof(fieldType)}

	switch underlying := fieldType.Underlying().(type) {
	case *types.Basic:
		switch underlying.Kind() {
		case types.Complex64, types.Complex128:
			return refuse("a complex number")
		case types.String:
			return refuse("a Go string, a managed reference")
		}

		clrType, found := cgoMirrorBasic[underlying.Kind()]

		if !found {
			return refuse(fmt.Sprintf("of basic kind %s", underlying.Name()))
		}

		member.clrType = clrType

	case *types.Pointer:
		// A pointer member is a native address in the mirror; what it may hold is the crossing's rule (§11.1).
		member.clrType = "nint"

	case *types.Struct:
		named, ok := types.Unalias(fieldType).(*types.Named)

		if !ok {
			return refuse("an unnamed struct")
		}

		nested, err := builder.mirror(named)

		if err != nil {
			return cgoMirrorField{}, err
		}

		member.clrType = nested.name

	case *types.Array:
		if underlying.Len() == 0 {
			return refuse("a flexible array member (only ever in native memory)")
		}

		element, err := builder.member(owner, field, underlying.Elem())

		if err != nil {
			return cgoMirrorField{}, err
		}

		helper := fmt.Sprintf("%s%d_%s", cgoMirrorArrayPrefix, underlying.Len(), element.clrType)

		if _, found := builder.arrays[helper]; !found {
			builder.arrays[helper] = cgoInlineArray{name: helper, length: underlying.Len(), elemType: element.clrType}
			builder.arrayAt = append(builder.arrayAt, helper)
		}

		member.clrType = helper
		member.length = underlying.Len()
		member.elemType = element.clrType
		member.elemSize = element.goSize

	default:
		return refuse(fmt.Sprintf("a %T, which holds a managed reference", underlying))
	}

	return member, nil
}

// passableByValue reports whether a mirror may cross BY VALUE (§11.7, RULED): every member, nested structs included,
// an integer or a pointer, with no union and no inline array. The CLR classifies a by-value struct for the native ABI
// from the mirror's member types, and floats, unions carried as bytes and inline arrays can classify differently from
// the C compiler's own struct. cmd/cgo gives a union as a byte array, so the inline-array rule refuses it too.
func (builder *cgoMirrorBuilder) passableByValue(built *cgoMirror) bool {
	for _, member := range built.fields {
		switch {
		case member.length > 0, member.clrType == "float", member.clrType == "double":
			return false
		case strings.HasSuffix(member.clrType, cgoMirrorSuffix):
			nested := builder.mirrors[strings.TrimSuffix(member.clrType, cgoMirrorSuffix)]

			if nested == nil || !builder.passableByValue(nested) {
				return false
			}
		}
	}

	return true
}

// csharp writes every mirror and inline-array helper the builder made, each in the order it was first needed.
func (builder *cgoMirrorBuilder) csharp() string {
	const interop = "global::System.Runtime.InteropServices."
	const compiler = "global::System.Runtime.CompilerServices."

	var text strings.Builder

	for _, goName := range builder.order {
		built := builder.mirrors[goName]

		fmt.Fprintf(&text, "[%sStructLayout(%sLayoutKind.Explicit, Size = %d)]\n", interop, interop, built.size)
		fmt.Fprintf(&text, "internal struct %s\n{\n", built.name)

		for _, member := range built.fields {
			fmt.Fprintf(&text, "    [%sFieldOffset(%d)] public %s %s;\n", interop, member.offset, member.clrType, member.name)
		}

		// The layout check (§5.3): the whole mirror's size against the C size. It runs once, before the mirror first
		// crosses.
		fmt.Fprintf(&text, "\n    internal static void CheckLayout()\n    {\n")
		fmt.Fprintf(&text, "        Hold(%q, %q, %sUnsafe.SizeOf<%s>(), %d);\n", "the whole mirror", built.name, compiler, built.name, built.size)

		fmt.Fprintf(&text, "    }\n\n")
		fmt.Fprintf(&text, "    private static void Hold(string member, string clrType, int clrSize, int cSize)\n    {\n")
		fmt.Fprintf(&text, "        if (clrSize != cSize)\n")
		fmt.Fprintf(&text, "            throw new global::System.InvalidOperationException($\"cgo mirror %s: member {member} is {clrSize} bytes as {clrType} and {cSize} bytes in C, so the mirror cannot cross\");\n", goName)
		fmt.Fprintf(&text, "    }\n}\n\n")
	}

	for _, name := range builder.arrayAt {
		helper := builder.arrays[name]
		fmt.Fprintf(&text, "[%sInlineArray(%d)]\ninternal struct %s\n{\n    private %s m_element;\n}\n\n", compiler, helper.length, helper.name, helper.elemType)
	}

	return text.String()
}
