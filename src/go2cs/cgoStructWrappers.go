// cgoStructWrappers.go - Gbtc
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
	"sort"
	"strings"
)

// Phase 2 of docs/PLAN-cgo-interop.md, increment 3b2a (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §5.2, read through
// §11.1 and §11.7): the binding of a C function that takes or returns C structs. A struct crosses BY VALUE only where
// its mirror passes §11.7's rule (integers and pointers, no union, no inline array, no all-padding eightbyte). BY
// POINTER it crosses as GoCgoArgs.StructPointer answers: nil as 0, a genuinely native struct by its own address, and one
// whole managed struct (a box, or a struct field) through a stack mirror, copied in member by member, checked, handed
// over by address and copied back unconditionally; an element of a Go array and a native box minted from managed storage
// are refused before the call. A pointer result is C memory, minted native, unless it is a pointer the call was handed.
// Increment 3b2b adds pointer MEMBERS, checked as cgo checks them, and a pointer to one C scalar, which crosses as a
// struct pointer does (§7, amended: single scalar pointee; element pointers remain Phase 3). Increment 4d admits the
// two-result errno form here for a SCALAR result, whose arm is the probe library's strtol (§11.10, COORD's ruling (c));
// an errno form returning a struct or a pointer waits for a measured arm and is refused by name.

// cgoWrapperSet makes one pseudo-package's members: the wrappers, the mirrors they cross with, and each mirror's copy-in
// and copy-out functions, every one written once.
type cgoWrapperSet struct {
	target    cgoBindingTarget
	libraries map[string]string // a symbol's library where it is not target.library (§11.5: the capture is per symbol)
	mirrors   *cgoMirrorBuilder
	copies    map[string]string
	copyOrder []string

	// pointerBearing names the mirrors that hold a pointer member, directly or through a nested struct or an inline array
	// (.5 cgo item 1(a), DESIGN 18.12.3). Their copy-out takes the wrapper's stack copies, and each has a pin check.
	pointerBearing map[string]bool
	allocates      bool   // a helper or C.malloc allocates, so the pseudo-package binds the package's own malloc
	class          string // the pseudo-package class, whose assembly a C variable's lookup names (cgo item 3)
}

func newCgoWrapperSet(target cgoBindingTarget, sizes types.Sizes) *cgoWrapperSet {
	return &cgoWrapperSet{target: target, mirrors: newCgoMirrorBuilder(sizes), copies: map[string]string{}, pointerBearing: map[string]bool{}}
}

func (set *cgoWrapperSet) libraryOf(symbol string) string {
	if library, found := set.libraries[symbol]; found {
		return library
	}

	return set.target.library
}

// cgoStructOf answers the named C struct a type is, or nil.
func cgoStructOf(t types.Type) *types.Named {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok || !strings.HasPrefix(named.Obj().Name(), "_Ctype_") {
		return nil
	}

	if _, isStruct := named.Underlying().(*types.Struct); !isStruct {
		return nil
	}

	return named
}

// cgoGoSideName is the name a C type has in its pseudo-package: its C name, as a C# identifier.
func cgoGoSideName(named *types.Named) string {
	return getCoreSanitizedIdentifier(strings.TrimPrefix(named.Obj().Name(), "_Ctype_"))
}

// mirrorOf makes the mirror of a C struct and its copy functions, refusing what this increment cannot cross.
func (set *cgoWrapperSet) mirrorOf(function string, named *types.Named) (*cgoMirror, error) {
	mirror, err := set.mirrors.mirror(named)

	if err != nil {
		return nil, err
	}

	if _, done := set.copies[mirror.name]; done {
		return mirror, nil
	}

	set.copies[mirror.name] = ""
	text, err := set.copyFunctions(function, named, mirror)

	if err != nil {
		delete(set.copies, mirror.name)
		return nil, err
	}

	set.copies[mirror.name] = text
	set.copyOrder = append(set.copyOrder, mirror.name)

	return mirror, nil
}

// copyFunctions writes a mirror's copy-in (Go struct to mirror) and copy-out (mirror to Go struct), member by member.
func (set *cgoWrapperSet) copyFunctions(function string, named *types.Named, mirror *cgoMirror) (string, error) {
	structType := named.Underlying().(*types.Struct)
	goSide := cgoGoSideName(named)

	var copyIn, copyOut, pinCheck strings.Builder

	for i := 0; i < structType.NumFields(); i++ {
		field := structType.Field(i)

		if field.Name() == "_" {
			continue
		}

		member := getCoreSanitizedIdentifier(field.Name())

		in, err := set.copyMember(function, named.Obj().Name()+"."+field.Name(), 0, "m."+member, "v."+member, field.Type(), cgoCopyIn)

		if err != nil {
			return "", err
		}

		out, err := set.copyMember(function, named.Obj().Name()+"."+field.Name(), 0, "m."+member, "v."+member, field.Type(), cgoCopyOut)

		if err != nil {
			return "", err
		}

		pin, err := set.copyMember(function, named.Obj().Name()+"."+field.Name(), 0, "m."+member, "v."+member, field.Type(), cgoPinCheck)

		if err != nil {
			return "", err
		}

		copyIn.WriteString(in)
		copyOut.WriteString(out)
		pinCheck.WriteString(pin)
	}

	// A mirror that holds a pointer member copies it back against the stack copies the wrapper handed C, and checks it
	// against each pinned argument inside the wrapper's `fixed` (.5 cgo item 1(a), DESIGN 18.12.3): a member C pointed
	// into pinned Go memory, or into a stack copy, would otherwise come back minted as C memory.
	bearing := pinCheck.Len() > 0
	set.pointerBearing[mirror.name] = bearing
	outExtra := ""

	if bearing {
		outExtra = ", " + cgoStackCopiesParam + " c, string function"
	}

	var text strings.Builder

	fmt.Fprintf(&text, "    // %s's copy-in and copy-out, member by member.\n", named.Obj().Name())
	fmt.Fprintf(&text, "    private static void ᴍin_%s(ref %s m, in %s v)\n    {\n%s    }\n\n", mirror.name, mirror.name, goSide, copyIn.String())
	fmt.Fprintf(&text, "    private static void ᴍout_%s(ref %s v, in %s m%s)\n    {\n%s    }\n", mirror.name, goSide, mirror.name, outExtra, copyOut.String())

	if bearing {
		fmt.Fprintf(&text, "\n    // %s's pointer members, each checked against one pinned argument inside the wrapper's `fixed`.\n", named.Obj().Name())
		fmt.Fprintf(&text, "    private static void ᴍpin_%s(in %s m, in global::go.golib.GoCgoPin g, byte* f, string function)\n    {\n%s    }\n", mirror.name, mirror.name, pinCheck.String())
	}

	return text.String(), nil
}

// cgoCopyMode is what copyMember writes for one member: its copy in (Go to mirror), its copy out, or its pin check (a
// pointer member against one pinned argument; nothing for any other member).
type cgoCopyMode int

const (
	cgoCopyIn cgoCopyMode = iota
	cgoCopyOut
	cgoPinCheck
)

// cgoStackCopiesParam is the C# type of a wrapper's stack copies (GoCgoStackCopy), as the copy-out of a pointer-bearing
// mirror takes them.
const cgoStackCopiesParam = "global::System.ReadOnlySpan<global::go.golib.GoCgoStackCopy>"

// copyMember writes the statement that copies one member, of type t, between the mirror expression m and the Go-side
// expression g, as mode says.
func (set *cgoWrapperSet) copyMember(function, member string, depth int, m, g string, t types.Type, mode cgoCopyMode) (string, error) {
	indent := strings.Repeat("    ", depth+2)
	in := mode == cgoCopyIn

	if nested := cgoStructOf(t); nested != nil {
		mirror, err := set.mirrorOf(function, nested)

		if err != nil {
			return "", err
		}

		switch {
		case in:
			return fmt.Sprintf("%sᴍin_%s(ref %s, %s);\n", indent, mirror.name, m, g), nil
		case mode == cgoPinCheck && set.pointerBearing[mirror.name]:
			return fmt.Sprintf("%sᴍpin_%s(in %s, g, f, function);\n", indent, mirror.name, m), nil
		case mode == cgoPinCheck:
			return "", nil
		case set.pointerBearing[mirror.name]:
			return fmt.Sprintf("%sᴍout_%s(ref %s, %s, c, function);\n", indent, mirror.name, g, m), nil
		}

		return fmt.Sprintf("%sᴍout_%s(ref %s, %s);\n", indent, mirror.name, g, m), nil
	}

	switch underlying := t.Underlying().(type) {
	case *types.Array:
		index := fmt.Sprintf("i%d", depth)
		element, err := set.copyMember(function, member, depth+1, m+"["+index+"]", g+"["+index+"]", underlying.Elem(), mode)

		if err != nil {
			return "", err
		}

		if element == "" {
			return "", nil
		}

		loop := fmt.Sprintf("%sfor (int %s = 0; %s < %d; %s++)\n%s", indent, index, index, underlying.Len(), index, element)

		// go2cs keeps an array's elements in the array's own storage, so B-10 asks that storage for a published
		// unsafe.Pointer word before the elements are copied raw into the mirror (DESIGN-cgo-phase3-marshaling 18.7 finding 2).
		if in {
			return fmt.Sprintf("%sglobal::go.golib.GoCgoArgs.RequireNoPublishedWordIn(%s);\n%s", indent, g, loop), nil
		}

		return loop, nil

	case *types.Pointer:
		if pointee, ok := types.Unalias(underlying.Elem()).(*types.Named); !ok || !strings.HasPrefix(pointee.Obj().Name(), "_Ctype_") {
			return "", fmt.Errorf("cgo wrapper: C.%s crosses %s, a pointer member to something other than a C type, which go2cs does not pass to C yet", function, member)
		}

		// A pointer member is checked as cgo checks it (§5.2): nil is 0, a native pointer is its address, and a pointer
		// into Go memory panics with cgo's own message before the call. Copied back, the address it went in as is the
		// ORIGINAL pointer, so a member C left alone keeps its identity, and any other is C memory, minted native.
		switch mode {
		case cgoCopyIn:
			return fmt.Sprintf("%s%s = (nint)global::go.golib.GoCgoArgs.PointerMember(%s);\n", indent, m, g), nil
		case cgoPinCheck:
			return fmt.Sprintf("%sglobal::go.golib.GoCgoArgs.RequireWordOutsidePin((nuint)%s, g, f, function);\n", indent, m), nil
		}

		return fmt.Sprintf("%s%s = global::go.golib.GoCgoArgs.PointerMemberBack((nuint)%s, %s, global::go.golib.GoCgoArgs.PointerMember(%s), c, function);\n", indent, g, m, g, g), nil
	}

	if mode == cgoPinCheck {
		return "", nil
	}

	scalar, err := cgoScalarOf(function, member, t)

	if err != nil {
		return "", err
	}

	if in {
		return fmt.Sprintf("%s%s = %s;\n", indent, m, scalar.toPrimitive(g)), nil
	}

	return fmt.Sprintf("%s%s = %s;\n", indent, g, scalar.fromPrimitive(m)), nil
}

// cgoStructParam is one parameter: a scalar, a struct by value, a pointer to a struct or to one scalar, or a void
// pointer (Phase 3, increment 3.1).
type cgoStructParam struct {
	scalar  cgoScalar
	mirror  *cgoMirror
	goSide  string
	pointer bool
	void    bool

	// out is a pointer to one C pointer, the out-parameter (`&db`, `char **`; Phase 3 shape (i)), and
	// goSide is then the C pointer's pointee: the slot is a ж<ж<goSide>>.
	out bool
}

// cgoIsVoidPointer reports whether a parameter or result type is Go's unsafe.Pointer, which is what cmd/cgo gives a C
// `void *`.
func cgoIsVoidPointer(t types.Type) bool {
	basic, ok := t.Underlying().(*types.Basic)

	return ok && basic.Kind() == types.UnsafePointer
}

// cgoVoidPointerType is the C# spelling of unsafe.Pointer in a bindings file, which aliases the package as @unsafe.
const cgoVoidPointerType = "@unsafe.Pointer"

// pointee is what a pointer parameter points at, as two pointer parameters that may share one object compare it.
func (param cgoStructParam) pointee() string {
	if param.mirror != nil {
		return "struct " + param.mirror.name
	}

	return "scalar " + param.scalar.goSide
}

// param reads one parameter.
func (set *cgoWrapperSet) param(function string, index int, t types.Type) (cgoStructParam, error) {
	role := fmt.Sprintf("parameter %d", index+1)

	if cgoIsVoidPointer(t) {
		return cgoStructParam{void: true}, nil
	}

	if pointer, ok := t.(*types.Pointer); ok {
		// A pointer to one C pointer is the out-parameter (shape (i), design 18.12 item 4): one stack word, copied in and
		// back by the pointer-member rules. Its pointee is a C struct or one C scalar, as a pointer parameter's is.
		if inner, ok := types.Unalias(pointer.Elem()).(*types.Pointer); ok {
			if named := cgoStructOf(inner.Elem()); named != nil {
				return cgoStructParam{goSide: cgoGoSideName(named), out: true}, nil
			}

			scalar, err := cgoScalarOf(function, role, inner.Elem())

			if err != nil || scalar.isVoid {
				return cgoStructParam{}, fmt.Errorf("cgo wrapper: C.%s's %s is a pointer to a pointer to something other than a C struct or one C scalar, which go2cs does not pass to C yet", function, role)
			}

			return cgoStructParam{goSide: scalar.goSide, out: true}, nil
		}

		named := cgoStructOf(pointer.Elem())

		// A pointer to ONE C scalar crosses as a struct pointer does: a stack copy in, its address handed over, and the
		// value copied back unconditionally (§7, amended 2026-11-10).
		if named == nil {
			scalar, err := cgoScalarOf(function, role, pointer.Elem())

			if err != nil || scalar.isVoid {
				return cgoStructParam{}, fmt.Errorf("cgo wrapper: C.%s's %s is a pointer to something other than a C struct or one C scalar, which go2cs does not pass to C yet", function, role)
			}

			return cgoStructParam{scalar: scalar, goSide: scalar.goSide, pointer: true}, nil
		}

		mirror, err := set.mirrorOf(function, named)

		if err != nil {
			return cgoStructParam{}, err
		}

		return cgoStructParam{mirror: mirror, goSide: cgoGoSideName(named), pointer: true}, nil
	}

	if named := cgoStructOf(t); named != nil {
		mirror, err := set.mirrorOf(function, named)

		if err != nil {
			return cgoStructParam{}, err
		}

		if !set.mirrors.passableByValue(mirror) {
			return cgoStructParam{}, fmt.Errorf("cgo wrapper: C.%s's %s is %s by value, which go2cs does not pass by value: it has a float, union or inline array member, or an all-padding eightbyte", function, role, named.Obj().Name())
		}

		return cgoStructParam{mirror: mirror, goSide: cgoGoSideName(named)}, nil
	}

	scalar, err := cgoScalarOf(function, role, t)

	if err != nil {
		return cgoStructParam{}, err
	}

	if scalar.isVoid {
		return cgoStructParam{}, fmt.Errorf("cgo wrapper: C.%s's %s is void", function, role)
	}

	return cgoStructParam{scalar: scalar}, nil
}

// wrapper writes one C function's extern and wrapper. A function wholly of scalars takes the scalar binding's shape
// (cgoScalarWrapper), so increment 3b1's text is unchanged by this one.
func (set *cgoWrapperSet) wrapper(declaration cgoDeclaration) (string, error) {
	function := declaration.cName
	scalarTarget := cgoBindingTarget{goos: set.target.goos, library: set.libraryOf(function), crt: set.target.crt}

	errnoForm := declaration.kind == cgoErrnoFunc

	// cmd/cgo's own helpers and its wrapper of C.malloc are Go functions, not C symbols (Phase 3, increment 3.1).
	if declaration.kind == cgoFunc && declaration.object != nil {
		if declaration.object.Name() == cgoMallocWrapper {
			return set.mallocWrapper(declaration)
		}

		if cgoBuiltinNames[function] {
			return set.helper(declaration)
		}
	}

	if declaration.kind == cgoVar {
		return set.variable(declaration)
	}

	// Anything not a C function is the scalar binding's to take or refuse.
	if declaration.kind != cgoFunc && !errnoForm {
		return cgoScalarWrapper(declaration, scalarTarget)
	}

	signature, ok := declaration.object.Type().(*types.Signature)
	wantResults := 1

	if errnoForm {
		wantResults = 2
	}

	if !ok || signature.Results().Len() != wantResults {
		return "", fmt.Errorf("cgo wrapper: C.%s has no %d-result signature", function, wantResults)
	}

	params := make([]cgoStructParam, signature.Params().Len())
	scalarsOnly := true

	for i := range params {
		param, err := set.param(function, i, signature.Params().At(i).Type())

		if err != nil {
			return "", err
		}

		params[i] = param
		scalarsOnly = scalarsOnly && param.mirror == nil && !param.pointer && !param.void && !param.out
	}

	resultType := signature.Results().At(0).Type()
	resultStruct := cgoStructOf(resultType)
	resultPointer, pointerResult := resultType.(*types.Pointer)
	voidResult := cgoIsVoidPointer(resultType)

	if scalarsOnly && resultStruct == nil && !pointerResult && !voidResult {
		return cgoScalarWrapper(declaration, scalarTarget)
	}

	if errnoForm && (resultStruct != nil || pointerResult || voidResult) {
		return "", fmt.Errorf("cgo wrapper: C.%s's two-result form returns %s, and an errno form with a struct or pointer result waits for a measured arm", function, resultType)
	}

	const interop = "global::System.Runtime.InteropServices."
	const compiler = "global::System.Runtime.CompilerServices."
	const runtime = "global::go.runtime_package."
	const args = "global::go.golib.GoCgoArgs."
	const mirrorKind = "global::go.golib.GoCgoStructPointer.Mirror"

	native := function + cgoMirrorSuffix + "native"
	var externResult, wrapperResult, returned string
	var resultMirror *cgoMirror
	var resultScalar cgoScalar

	switch {
	case resultStruct != nil:
		mirror, err := set.mirrorOf(function, resultStruct)

		if err != nil {
			return "", err
		}

		if !set.mirrors.passableByValue(mirror) {
			return "", fmt.Errorf("cgo wrapper: C.%s returns %s by value, which go2cs does not return by value: it has a float, union or inline array member, or an all-padding eightbyte", function, resultStruct.Obj().Name())
		}

		resultMirror = mirror
		externResult = mirror.name
		wrapperResult = cgoGoSideName(resultStruct)

	case pointerResult:
		pointee, ok := types.Unalias(resultPointer.Elem()).(*types.Named)

		if !ok || !strings.HasPrefix(pointee.Obj().Name(), "_Ctype_") {
			return "", fmt.Errorf("cgo wrapper: C.%s returns a pointer to something other than a C type", function)
		}

		externResult = "nint"
		wrapperResult = fmt.Sprintf("ж<%s>", cgoGoSideName(pointee))

	case voidResult:
		externResult = "nint"
		wrapperResult = cgoVoidPointerType

	default:
		scalar, err := cgoScalarOf(function, "result", resultType)

		if err != nil {
			return "", err
		}

		resultScalar = scalar
		externResult = scalar.primitive
		wrapperResult = scalar.goSide

		if scalar.isVoid {
			externResult = "void"
		}
	}

	var externParams, wrapperParams, arguments []string
	var before, after strings.Builder

	// The void parameters and the scalar pointer parameters, each pinned by its own `fixed` around the call (18.12 item
	// 2). A scalar pointer's pin holds something only when its kind is Pinned.
	var pinned, pinnedScalars []int

	// The words C returns (an out-parameter's word, a pointer result), each checked against each pinned argument inside
	// the `fixed`: a word inside pinned Go storage is refused by name, never minted as a native box over memory that moves
	// once the `fixed` ends (DESIGN-cgo-phase3-marshaling 18.12.2). Each entry is a guard condition (empty for none) and the word.
	type returnedWord struct{ guard, word string }
	var returnedWords []returnedWord

	// The stack copies handed to C (a mirrored pointer argument, an out-parameter's word), each with the box it stands
	// for (.5 cgo item 1(b), DESIGN 18.12.3): a word C returns equal to a copy's address is that box, as under Go, and a
	// word elsewhere inside one is refused by name. needCopies says whether any copy-back reads them.
	var stackCopies []string
	needCopies := false

	// The mirrored pointer arguments whose mirror holds a pointer member, each with its mirrored condition: each such
	// member is checked against every pinned argument inside the `fixed` (H's sizing item 1(a)).
	type bearingCopy struct {
		index            int
		mirrored, mirror string
	}
	var bearingCopies []bearingCopy

	// The void and out-parameter arguments, each of which can cross as a word mirror of a Go array of C pointers (.5 cgo
	// item 2, DESIGN 18.12.3, H's verdict (A)): every word C wrote is checked against every pinned argument inside the
	// `fixed`, the mirror's own pin included, and the words are copied back after it.
	var wordMirrors []int

	checked := map[string]bool{}

	for i, param := range params {
		switch {
		case param.void:
			externParams = append(externParams, fmt.Sprintf("nint p%d", i))
			wrapperParams = append(wrapperParams, fmt.Sprintf("%s p%d", cgoVoidPointerType, i))
			arguments = append(arguments, fmt.Sprintf("a%d", i))

		case param.out:
			externParams = append(externParams, fmt.Sprintf("nint p%d", i))
			wrapperParams = append(wrapperParams, fmt.Sprintf("ж<ж<%s>> p%d", param.goSide, i))
			arguments = append(arguments, fmt.Sprintf("a%d", i))

		case param.pointer:
			externParams = append(externParams, fmt.Sprintf("nint p%d", i))
			wrapperParams = append(wrapperParams, fmt.Sprintf("ж<%s> p%d", param.goSide, i))
			arguments = append(arguments, fmt.Sprintf("a%d", i))

		case param.mirror != nil:
			externParams = append(externParams, fmt.Sprintf("%s p%d", param.mirror.name, i))
			wrapperParams = append(wrapperParams, fmt.Sprintf("%s p%d", param.goSide, i))
			arguments = append(arguments, fmt.Sprintf("v%d", i))

		default:
			externParams = append(externParams, fmt.Sprintf("%s p%d", param.scalar.primitive, i))
			wrapperParams = append(wrapperParams, fmt.Sprintf("%s p%d", param.scalar.goSide, i))
			arguments = append(arguments, param.scalar.argument(function, fmt.Sprintf("p%d", i)))
		}

		if param.mirror != nil && !checked[param.mirror.name] {
			checked[param.mirror.name] = true
			fmt.Fprintf(&before, "        %s.CheckLayout%s();\n", param.mirror.name, cgoMirrorSuffix)
		}
	}

	if resultMirror != nil && !checked[resultMirror.name] {
		fmt.Fprintf(&before, "        %s.CheckLayout%s();\n", resultMirror.name, cgoMirrorSuffix)
	}

	// A wrapper that takes a struct pointer holds one GoCgoImages for the call (the struct image door, DESIGN-cgo-phase3-
	// marshaling 19.7 and 19.9): a struct view no door can pin crosses as an image of its root, one image per root, and
	// every pointer parameter's root is claimed in it, so one Go value is never handed C as two copies. A wrapper with no
	// struct pointer emits none of it.
	images := ""

	for _, param := range params {
		if param.pointer && param.mirror != nil {
			images = "ref images, "
		}
	}

	if images != "" {
		before.WriteString("\n        global::go.golib.GoCgoImages images = default;\n")
	}

	for i, param := range params {
		switch {
		case param.void:
			// What a void pointer carries is GoCgoArgs' to decide (Phase 3 §13.4, §16.1 B, 18.12): nil and C memory cross as
			// their number, Go memory whose box names its storage crosses held still by the `fixed` around the call, and
			// everything else is refused here, before anything is pinned or handed over (13.5: classify, pin, call).
			fmt.Fprintf(&before, "\n        global::go.golib.GoCgoPin g%d = %sPinVoidPointer(p%d, %q, out global::go.golib.GoCgoWordMirror? w%d);\n", i, args, i, function, i)
			pinned = append(pinned, i)
			wordMirrors = append(wordMirrors, i)
			fmt.Fprintf(&after, "\n        w%d?.CopyBack(copies, %q);\n", i, function)
			needCopies = true

		case param.out:
			// The out-parameter (shape (i)): a slot in Go memory crosses as one stack word, copied in by the pointer-member
			// rule (a Go pointer in it is cgo's panic) and copied back after the call, where an address C left alone keeps
			// its box. A slot in C memory is handed over by its own address. An element of a Go array of C pointers is the
			// argv shape: the whole array crosses as a pinned word mirror (Pinned), copied back by the mirror.
			fmt.Fprintf(&before, "\n        nint m%d = default;\n", i)
			fmt.Fprintf(&before, "        global::go.golib.GoCgoStructPointer k%d = %sOutParameter(p%d, %q, out nuint n%d, out ж<%s>? q%d, out global::go.golib.GoCgoPin g%d, out global::go.golib.GoCgoWordMirror? w%d);\n", i, args, i, function, i, param.goSide, i, i, i)
			pinnedScalars = append(pinnedScalars, i)
			wordMirrors = append(wordMirrors, i)
			fmt.Fprintf(&before, "        nint a%d = (nint)n%d;\n", i, i)
			fmt.Fprintf(&before, "\n        if (k%d == %s)\n        {\n            m%d = (nint)n%d;\n            a%d = (nint)(&m%d);\n        }\n", i, mirrorKind, i, i, i, i)
			fmt.Fprintf(&after, "\n        if (k%d == %s)\n            p%d.ValueSlot = %sPointerMemberBack<%s>((nuint)m%d, q%d, n%d, copies, %q);\n", i, mirrorKind, i, args, param.goSide, i, i, i, function)
			fmt.Fprintf(&after, "\n        w%d?.CopyBack(copies, %q);\n", i, function)
			returnedWords = append(returnedWords, returnedWord{word: fmt.Sprintf("(nuint)m%d", i)})
			stackCopies = append(stackCopies, fmt.Sprintf("k%d == %s ? new(&m%d, (nuint)sizeof(nint), p%d) : default", i, mirrorKind, i, i))
			needCopies = true

		case param.pointer:
			copyType := param.scalar.primitive

			if param.mirror != nil {
				copyType = param.mirror.name
			}

			fmt.Fprintf(&before, "\n        %s m%d = default;\n", copyType, i)

			// The pointer's kind is GoCgoArgs' to decide (§11.1): for a struct it refuses, before any native code runs, an
			// element of a Go array (C may touch the whole array) and a native box minted from managed storage (C must not
			// write through it). A pointer to one C scalar takes the pinned class there instead (18.12 item 2): its address
			// inside Go storage, held still by the `fixed` around the call, since a scalar's layout is the same in C.
			if param.mirror == nil {
				fmt.Fprintf(&before, "        global::go.golib.GoCgoStructPointer k%d = %sScalarPointer(p%d, %q, %sout nuint n%d, out global::go.golib.GoCgoPin g%d);\n", i, args, i, function, images, i, i)
			} else {
				fmt.Fprintf(&before, "        global::go.golib.GoCgoStructPointer k%d = %sStructPointer(p%d, %q, ref images, out nuint n%d, out global::go.golib.GoCgoPin g%d);\n", i, args, i, function, i, i)
			}

			pinnedScalars = append(pinnedScalars, i)

			fmt.Fprintf(&before, "        nint a%d = (nint)n%d;\n", i, i)

			// Two pointers to the same Go struct are one object to C, as they are under Go: the later one shares the
			// earlier one's mirror, so what C writes through either is what both see, and the struct is copied back once
			// (§5.2). Sameness is the storage's, not the box's, since two boxes can reach one struct.
			var earlier []int

			for j := 0; j < i; j++ {
				if params[j].pointer && params[j].pointee() == param.pointee() {
					earlier = append(earlier, j)
				}
			}

			var copyIn, copyOut string

			if param.mirror != nil {
				copyIn = fmt.Sprintf("ᴍin_%s(ref m%d, p%d.Value);\n%[4]sa%[2]d = (nint)(&m%[2]d);\n", param.mirror.name, i, i, "            ")
				copyOut = fmt.Sprintf("ᴍout_%s(ref p%d.Value, m%d);", param.mirror.name, i, i)

				if set.pointerBearing[param.mirror.name] {
					copyOut = fmt.Sprintf("ᴍout_%s(ref p%d.Value, m%d, copies, %q);", param.mirror.name, i, i, function)
					needCopies = true
				}
			} else {
				copyIn = fmt.Sprintf("m%d = %s;\n%[3]sa%[1]d = (nint)(&m%[1]d);\n", i, param.scalar.toPrimitive(fmt.Sprintf("p%d.Value", i)), "            ")
				copyOut = fmt.Sprintf("p%d.Value = %s;", i, param.scalar.fromPrimitive(fmt.Sprintf("m%d", i)))
			}
			mirrored := fmt.Sprintf("k%d == %s", i, mirrorKind)

			if len(earlier) == 0 {
				fmt.Fprintf(&before, "\n        if (%s)\n        {\n            %s        }\n", mirrored, copyIn)
			} else {
				fmt.Fprintf(&before, "        bool s%d = false;\n\n        if (%s)\n        {\n", i, mirrored)

				for n, j := range earlier {
					keyword := "if"

					if n > 0 {
						keyword = "else if"
					}

					fmt.Fprintf(&before, "            %s (k%d == %s && %sUnsafe.AreSame(ref p%d.Value, ref p%d.Value))\n", keyword, j, mirrorKind, compiler, i, j)
					fmt.Fprintf(&before, "            {\n                s%d = true;\n                a%d = a%d;\n            }\n", i, i, j)
				}

				fmt.Fprintf(&before, "            else\n            {\n                %s            }\n        }\n", strings.ReplaceAll(copyIn, "\n            a", "\n                a"))
				mirrored += fmt.Sprintf(" && !s%d", i)
			}

			fmt.Fprintf(&after, "\n        if (%s)\n            %s\n", mirrored, copyOut)
			stackCopies = append(stackCopies, fmt.Sprintf("%s ? new(&m%d, (nuint)sizeof(%s), p%d) : default", mirrored, i, copyType, i))

			if param.mirror != nil && set.pointerBearing[param.mirror.name] {
				bearingCopies = append(bearingCopies, bearingCopy{index: i, mirrored: mirrored, mirror: param.mirror.name})
			}

		case param.mirror != nil:
			fmt.Fprintf(&before, "\n        %s v%d = default;\n        ᴍin_%s(ref v%d, p%d);\n", param.mirror.name, i, param.mirror.name, i, i)
		}
	}

	// After the call and the mirrors' copy-backs: each image C changed is decoded back into its root, once.
	if images != "" {
		after.WriteString("\n        images.CopyBack();\n")
	}

	switch {
	case resultMirror != nil && set.pointerBearing[resultMirror.name]:
		returned = fmt.Sprintf("\n        %s result = new();\n        ᴍout_%s(ref result, r, copies, %q);\n\n        return result;\n", wrapperResult, resultMirror.name, function)
		needCopies = true
	case resultMirror != nil:
		returned = fmt.Sprintf("\n        %s result = new();\n        ᴍout_%s(ref result, r);\n\n        return result;\n", wrapperResult, resultMirror.name)
	case pointerResult:
		// A result that is a pointer the call was handed is THAT pointer, as it is in Go (gmtime_r returns its tm): its
		// address here is the stack copy's, which dies with this call, so it maps back to the caller's own box. Any other
		// result is C memory, minted native.
		var identity strings.Builder
		pointee := strings.TrimSuffix(strings.TrimPrefix(wrapperResult, "ж<"), ">")

		var notIdentity []string

		for i, param := range params {
			if param.pointer && param.goSide == pointee {
				fmt.Fprintf(&identity, "\n        if (r != 0 && r == a%d)\n            return p%d;\n", i, i)
				notIdentity = append(notIdentity, fmt.Sprintf("r != a%d", i))
			}
		}

		// The pointer C was handed, returned, maps back to its box above; any other address inside pinned storage is
		// refused inside the `fixed`.
		returnedWords = append(returnedWords, returnedWord{guard: strings.Join(notIdentity, " && "), word: "(nuint)r"})

		returned = fmt.Sprintf("%s\n        return %sPointerMemberBack<%s>((nuint)r, null, 0, copies, %q);\n", identity.String(), args, pointee, function)
		needCopies = true
	case voidResult:
		// A void pointer C returns is C memory, or nil: its number.
		returned = fmt.Sprintf("\n        return new %s((uintptr)(nuint)r);\n", cgoVoidPointerType)
	case resultScalar.isVoid:
		returned = "\n        return default;\n"
	case errnoForm && resultScalar.isVoid:
		returned = "\n" + cgoErrnoReturn("default")
	case errnoForm:
		returned = "\n" + cgoErrnoReturn(resultScalar.fromPrimitive("r"))
	default:
		returned = fmt.Sprintf("\n        return %s;\n", resultScalar.fromPrimitive("r"))
	}

	library := set.libraryOf(function)
	name := getCoreSanitizedIdentifier(function)
	setLastError := ""

	if errnoForm {
		native += "errno"
		name = function + cgoErrnoFormSuffix
		wrapperResult = fmt.Sprintf("(%s, error)", wrapperResult)

		if set.target.goos != "windows" {
			setLastError = ", SetLastError = true"
		}
	}

	var text strings.Builder

	fmt.Fprintf(&text, "    // C.%s, bound to %s.\n", function, library)
	fmt.Fprintf(&text, "    [%sLibraryImport(%q, EntryPoint = %q%s)]\n", interop, library, function, setLastError)
	fmt.Fprintf(&text, "    [%sUnmanagedCallConv(CallConvs = new[] { typeof(%sCallConvCdecl) })]\n", interop, compiler)
	fmt.Fprintf(&text, "    private static partial %s %s(%s);\n\n", externResult, native, strings.Join(externParams, ", "))
	fmt.Fprintf(&text, "    internal static %s %s(%s)\n    {\n", wrapperResult, name, strings.Join(wrapperParams, ", "))
	text.WriteString(strings.TrimPrefix(before.String(), "\n"))

	call := fmt.Sprintf("%s(%s)", native, strings.Join(arguments, ", "))

	if externResult != "void" {
		fmt.Fprintf(&text, "\n        %s r;\n", externResult)
		call = "r = " + call
	} else {
		text.WriteString("\n")
	}

	if errnoForm {
		text.WriteString("        int errno;\n")
	}

	var bracket strings.Builder

	fmt.Fprintf(&bracket, "        %ssyscallEntersyscall();\n\n        try\n        {\n%s        }\n", runtime, cgoErrnoCall(call, errnoForm, set.target.goos))
	fmt.Fprintf(&bracket, "        finally\n        {\n            %ssyscallExitsyscall();\n        }\n", runtime)

	if len(pinned) == 0 && len(pinnedScalars) == 0 {
		text.WriteString(bracket.String())
	} else {
		// Every argument is classified above; here each pinned one is pinned, nested in parameter order, and the innermost
		// block holds the transition bracket and the call (13.5). A number pins nothing: its pinnable reference is null.
		all := append(append([]int(nil), pinned...), pinnedScalars...)
		sort.Ints(all)

		if !strings.HasSuffix(text.String(), "\n\n") {
			text.WriteString("\n")
		}

		for _, i := range all {
			fmt.Fprintf(&text, "        fixed (byte* f%d = g%d)\n", i, i)
		}

		text.WriteString("        {\n")

		for _, i := range all {
			if params[i].void {
				fmt.Fprintf(&text, "            nint a%d = g%d.Address(f%d);\n", i, i, i)
			} else {
				fmt.Fprintf(&text, "            if (k%d == global::go.golib.GoCgoStructPointer.Pinned)\n                a%d = g%d.Address(f%d);\n", i, i, i, i)
			}
		}

		text.WriteString("\n")

		for _, line := range strings.SplitAfter(bracket.String(), "\n") {
			if strings.TrimSpace(line) != "" {
				text.WriteString("    ")
			}

			text.WriteString(line)
		}

		if len(returnedWords) > 0 {
			text.WriteString("\n")

			for _, returned := range returnedWords {
				indent := "            "

				if returned.guard != "" {
					fmt.Fprintf(&text, "            if (%s)\n            {\n", returned.guard)
					indent = "                "
				}

				for _, i := range all {
					fmt.Fprintf(&text, "%s%sRequireWordOutsidePin(%s, g%d, f%d, %q);\n", indent, args, returned.word, i, i, function)
				}

				if returned.guard != "" {
					text.WriteString("            }\n")
				}
			}
		}

		if len(bearingCopies) > 0 || (resultMirror != nil && set.pointerBearing[resultMirror.name]) {
			text.WriteString("\n")

			for _, copied := range bearingCopies {
				fmt.Fprintf(&text, "            if (%s)\n            {\n", copied.mirrored)

				for _, i := range all {
					fmt.Fprintf(&text, "                ᴍpin_%s(in m%d, g%d, f%d, %q);\n", copied.mirror, copied.index, i, i, function)
				}

				text.WriteString("            }\n")
			}

			if resultMirror != nil && set.pointerBearing[resultMirror.name] {
				for _, i := range all {
					fmt.Fprintf(&text, "            ᴍpin_%s(in r, g%d, f%d, %q);\n", resultMirror.name, i, i, function)
				}
			}
		}

		// Every word C wrote into a word mirror, against every pin: an address inside pinned Go storage (the mirror's
		// own words included, `argv[0] = &argv[2]`) is refused by name, as any returned word inside a pin is.
		if len(wordMirrors) > 0 {
			text.WriteString("\n")

			for _, m := range wordMirrors {
				for _, i := range all {
					fmt.Fprintf(&text, "            w%d?.RequireWordsOutsidePin(g%d, f%d, %q);\n", m, i, i, function)
				}
			}
		}

		text.WriteString("        }\n")
	}

	// The stack copies, for the copy-back of every word C may have left pointing at one (H's sizing item 1(b)).
	if needCopies {
		if len(stackCopies) == 0 {
			fmt.Fprintf(&text, "\n        %s copies = default;\n", cgoStackCopiesParam)
		} else {
			fmt.Fprintf(&text, "\n        %s copies =\n        [\n            %s\n        ];\n", cgoStackCopiesParam, strings.Join(stackCopies, ",\n            "))
		}
	}

	text.WriteString(after.String())
	text.WriteString(returned)
	text.WriteString("    }\n")

	return text.String(), nil
}

// support writes what the wrappers share: the mirrors (with their layout checks and inline-array helpers) and each
// mirror's copy functions, keyed so a pseudo-package's text orders them after the wrappers.
func (set *cgoWrapperSet) support() map[string]string {
	members := map[string]string{}

	if len(set.mirrors.order) > 0 {
		members[cgoMirrorSuffix+"1 mirrors"] = indentCSharp(set.mirrors.csharp())
	}

	names := append([]string(nil), set.copyOrder...)
	sort.Strings(names)

	for _, name := range names {
		members[cgoMirrorSuffix+"2 copies "+name] = set.copies[name]
	}

	if set.allocates {
		members[cgoMirrorSuffix+"3 allocator"] = cgoAllocatorBinding(set.libraryOf(cgoAllocator))
	}

	return members
}

// cgoAllocator is the C function cmd/cgo's helpers allocate with: the package's own malloc, bound from the library the
// capture names for it, so that what C.CString returns is freed by the allocator that made it (Phase 3 §3, Q-D).
const cgoAllocator = "malloc"

// cgoAllocate is the pseudo-package's private member that calls the bound malloc.
const cgoAllocate = cgoMirrorSuffix + "allocate"

// cgoAllocatingHelpers are the helpers of cmd/cgo that allocate.
var cgoAllocatingHelpers = map[string]bool{"CString": true, "CBytes": true}

// cgoAllocatorBinding writes the package's own malloc and the member that calls it, as cmd/cgo's own C stub does
// (out.go, _cgoPREFIX_Cfunc__Cmalloc): a request for no bytes that returns nil asks for one byte, and a nil result
// ends the program.
func cgoAllocatorBinding(library string) string {
	const interop = "global::System.Runtime.InteropServices."
	const compiler = "global::System.Runtime.CompilerServices."
	const runtime = "global::go.runtime_package."

	native := cgoAllocator + cgoMirrorSuffix + "native"

	var text strings.Builder

	fmt.Fprintf(&text, "    // %s, bound to %s: what C.CString, C.CBytes and C.malloc allocate with. As cmd/cgo's own stub does, a\n", cgoAllocator, library)
	text.WriteString("    // request for no bytes that returns nil asks for one, and a nil result ends the program.\n")
	fmt.Fprintf(&text, "    [%sLibraryImport(%q, EntryPoint = %q)]\n", interop, library, cgoAllocator)
	fmt.Fprintf(&text, "    [%sUnmanagedCallConv(CallConvs = new[] { typeof(%sCallConvCdecl) })]\n", interop, compiler)
	fmt.Fprintf(&text, "    private static partial nint %s(nuint p0);\n\n", native)
	fmt.Fprintf(&text, "    private static nuint %s(nuint size)\n    {\n        nint r;\n", cgoAllocate)
	fmt.Fprintf(&text, "        %ssyscallEntersyscall();\n\n        try\n        {\n", runtime)
	fmt.Fprintf(&text, "            r = %s(size);\n\n            if (r == 0 && size == 0)\n                r = %s(1);\n        }\n", native, native)
	fmt.Fprintf(&text, "        finally\n        {\n            %ssyscallExitsyscall();\n        }\n\n", runtime)
	text.WriteString("        return global::go.golib.GoCgoArgs.Allocated(r);\n    }\n")

	return text.String()
}

// mallocWrapper writes C.malloc: cmd/cgo's wrapper of it takes a size_t and returns a void pointer that is never nil.
func (set *cgoWrapperSet) mallocWrapper(declaration cgoDeclaration) (string, error) {
	signature, ok := declaration.object.Type().(*types.Signature)

	if !ok || signature.Params().Len() != 1 || signature.Results().Len() != 1 || !cgoIsVoidPointer(signature.Results().At(0).Type()) {
		return "", fmt.Errorf("cgo wrapper: C.malloc: cmd/cgo declares its wrapper in a form go2cs does not know")
	}

	size, err := cgoScalarOf(cgoAllocator, "parameter 1", signature.Params().At(0).Type())

	if err != nil {
		return "", err
	}

	set.allocates = true

	var text strings.Builder

	text.WriteString("    // C.malloc: cmd/cgo gives it a wrapper of its own, whose result is never nil.\n")
	fmt.Fprintf(&text, "    internal static %s %s(%s p0)\n    {\n", cgoVoidPointerType, cgoAllocator, size.goSide)
	fmt.Fprintf(&text, "        return new %s((uintptr)%s((nuint)%s));\n    }\n", cgoVoidPointerType, cgoAllocate, size.toPrimitive("p0"))

	return text.String(), nil
}

// helper writes one of cmd/cgo's own helpers (out.go, builtinDefs) as a member of the pseudo-package. In Go each is a
// Go function over the runtime; here each is golib's copy over an address (GoCgoArgs), and the two that allocate do
// it with the package's own malloc. They copy bytes: no text encoding is on the path (Phase 3 §13.7, S5).
func (set *cgoWrapperSet) helper(declaration cgoDeclaration) (string, error) {
	const args = "global::go.golib.GoCgoArgs."

	function := declaration.cName
	unknown := fmt.Errorf("cgo wrapper: C.%s: cmd/cgo declares this helper in a form go2cs does not know", function)
	signature, ok := declaration.object.Type().(*types.Signature)

	if !ok || signature.Results().Len() != 1 {
		return "", unknown
	}

	params, result := signature.Params(), signature.Results().At(0).Type()

	// charPointer is a typed C pointer's C# type and its pointee's name.
	charPointer := func(t types.Type) (string, string) {
		pointer, ok := t.(*types.Pointer)

		if !ok {
			return "", ""
		}

		pointee, ok := types.Unalias(pointer.Elem()).(*types.Named)

		if !ok || !strings.HasPrefix(pointee.Obj().Name(), "_Ctype_") {
			return "", ""
		}

		return fmt.Sprintf("ж<%s>", cgoGoSideName(pointee)), cgoGoSideName(pointee)
	}

	isString := func(t types.Type) bool {
		basic, ok := t.Underlying().(*types.Basic)

		return ok && basic.Kind() == types.String
	}

	isBytes := func(t types.Type) bool {
		slice, ok := t.Underlying().(*types.Slice)

		if !ok {
			return false
		}

		basic, ok := slice.Elem().Underlying().(*types.Basic)

		return ok && basic.Kind() == types.Uint8
	}

	// count is a counted helper's second parameter, a C int.
	count := func() (cgoScalar, bool) {
		if params.Len() != 2 {
			return cgoScalar{}, false
		}

		scalar, err := cgoScalarOf(function, "parameter 2", params.At(1).Type())

		return scalar, err == nil && !scalar.isVoid && !scalar.isBool
	}

	var text strings.Builder

	switch function {
	case "CString":
		pointerType, pointee := charPointer(result)

		if params.Len() != 1 || !isString(params.At(0).Type()) || pointerType == "" {
			return "", unknown
		}

		set.allocates = true
		text.WriteString("    // C.CString, cmd/cgo's own helper: the string's bytes and a NUL, in memory the package's malloc allocated.\n")
		text.WriteString("    // The caller frees it.\n")
		fmt.Fprintf(&text, "    internal static %s CString(@string p0)\n    {\n", pointerType)
		fmt.Fprintf(&text, "        nuint address = %s(%sStringSize(p0));\n\n", cgoAllocate, args)
		fmt.Fprintf(&text, "        %sCopyString(p0, address);\n\n", args)
		fmt.Fprintf(&text, "        return %sPointerMemberBack<%s>(address, null, 0);\n    }\n", args, pointee)

	case "CBytes":
		if params.Len() != 1 || !isBytes(params.At(0).Type()) || !cgoIsVoidPointer(result) {
			return "", unknown
		}

		set.allocates = true
		text.WriteString("    // C.CBytes, cmd/cgo's own helper: the slice's bytes, in memory the package's malloc allocated. The caller\n")
		text.WriteString("    // frees it.\n")
		fmt.Fprintf(&text, "    internal static %s CBytes(slice<byte> p0)\n    {\n", cgoVoidPointerType)
		fmt.Fprintf(&text, "        nuint address = %s(%sBytesSize(p0));\n\n", cgoAllocate, args)
		fmt.Fprintf(&text, "        %sCopyBytes(p0, address);\n\n", args)
		fmt.Fprintf(&text, "        return new %s((uintptr)address);\n    }\n", cgoVoidPointerType)

	case "GoString":
		if params.Len() != 1 || !isString(result) {
			return "", unknown
		}

		pointerType, _ := charPointer(params.At(0).Type())

		if pointerType == "" {
			return "", unknown
		}

		text.WriteString("    // C.GoString, cmd/cgo's own helper: the bytes of a C string, up to its NUL, as a Go string.\n")
		fmt.Fprintf(&text, "    internal static @string GoString(%s p0)\n    {\n", pointerType)
		// Go memory is read from its pinned storage, as a Go function reads it (DESIGN-cgo-phase3-marshaling 18.12 item 3).
		fmt.Fprintf(&text, "        return %sGoStringOf(p0, %q);\n    }\n", args, function)

	case "GoStringN":
		length, counted := count()
		pointerType := ""

		if counted {
			pointerType, _ = charPointer(params.At(0).Type())
		}

		if pointerType == "" || !isString(result) {
			return "", unknown
		}

		text.WriteString("    // C.GoStringN, cmd/cgo's own helper: a counted run of C memory as a Go string.\n")
		fmt.Fprintf(&text, "    internal static @string GoStringN(%s p0, %s p1)\n    {\n", pointerType, length.goSide)
		fmt.Fprintf(&text, "        return %sGoStringNOf(p0, (int)%s, %q);\n    }\n", args, length.toPrimitive("p1"), function)

	case "GoBytes":
		length, counted := count()

		if !counted || !cgoIsVoidPointer(params.At(0).Type()) || !isBytes(result) {
			return "", unknown
		}

		text.WriteString("    // C.GoBytes, cmd/cgo's own helper: a counted run of C memory as a new Go byte slice.\n")
		fmt.Fprintf(&text, "    internal static slice<byte> GoBytes(%s p0, %s p1)\n    {\n", cgoVoidPointerType, length.goSide)
		fmt.Fprintf(&text, "        return %sGoBytesOf(p0, (int)%s, %q);\n    }\n", args, length.toPrimitive("p1"), function)

	default:
		return "", unknown
	}

	return text.String(), nil
}

// indentCSharp indents each non-empty line of C# by one level, for text written at the top level and placed in a class.
func indentCSharp(text string) string {
	lines := strings.Split(strings.TrimRight(text, "\n"), "\n")

	for i, line := range lines {
		if line != "" {
			lines[i] = "    " + line
		}
	}

	return strings.Join(lines, "\n") + "\n"
}

// variable writes C.x for a C global variable (cgo item 3). cmd/cgo declares _Cvar_x as the variable's address, of
// type *T, and writes every use as *_Cvar_x. The pseudo-package holds a box over the address the library exports for
// the symbol, looked up on first use (GoCgoLibraries.Variable), as Ꮡx, and x is a ref property over the box's value, so
// C.x reads and writes the variable in place, as Go does, and &C.x is the same box on every use. T is a C scalar or a
// C struct; any other C type is refused by name.
func (set *cgoWrapperSet) variable(declaration cgoDeclaration) (string, error) {
	name := declaration.cName
	pointer, ok := declaration.object.Type().(*types.Pointer)

	if !ok {
		return "", fmt.Errorf("cgo wrapper: C.%s is a C variable cmd/cgo did not declare as an address", name)
	}

	named, ok := types.Unalias(pointer.Elem()).(*types.Named)
	cType := ""

	if ok {
		cType, ok = strings.CutPrefix(named.Obj().Name(), "_Ctype_")
	}

	if !ok {
		return "", fmt.Errorf("cgo wrapper: C.%s is a C variable of type %s, which is not a C scalar or struct; go2cs reads and writes only those C variables", name, strings.ReplaceAll(types.TypeString(pointer.Elem(), func(*types.Package) string { return "" }), "_Ctype_", "C."))
	}

	switch underlying := named.Underlying().(type) {
	case *types.Basic:
		if underlying.Kind() == types.UnsafePointer || underlying.Kind() == types.String {
			return "", fmt.Errorf("cgo wrapper: C.%s is a C variable of type C.%s, a pointer; go2cs reads and writes only C scalar and struct variables", name, cType)
		}
	case *types.Struct:
	default:
		return "", fmt.Errorf("cgo wrapper: C.%s is a C variable of type C.%s, which is not a C scalar or struct; go2cs reads and writes only those C variables", name, cType)
	}

	member := getCoreSanitizedIdentifier(name)
	goSide := getCoreSanitizedIdentifier(cType)
	library := set.libraryOf(name)
	box := AddressPrefix + member

	var text strings.Builder

	fmt.Fprintf(&text, "    // C.%s, a C variable in %s, read and written in place through a box over its address.\n", name, library)
	fmt.Fprintf(&text, "    private static ж<%s>? %sᴍvariable;\n\n", goSide, member)
	fmt.Fprintf(&text, "    internal static ж<%s> %s => %sᴍvariable ??= global::go.golib.GoCgoLibraries.Variable<%s>(typeof(%s).Assembly, %q, %q);\n\n", goSide, box, member, goSide, set.class, library, name)
	fmt.Fprintf(&text, "    internal static ref %s %s => ref %s.Value;\n", goSide, member, box)

	return text.String(), nil
}
