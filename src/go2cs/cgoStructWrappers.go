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
// are refused before the call. A pointer result is C memory, minted native. Pointer MEMBERS are increment 3b2b's.

// cgoWrapperSet makes one pseudo-package's members: the wrappers, the mirrors they cross with, and each mirror's copy-in
// and copy-out functions, every one written once.
type cgoWrapperSet struct {
	target    cgoBindingTarget
	libraries map[string]string // a symbol's library where it is not target.library (§11.5: the capture is per symbol)
	mirrors   *cgoMirrorBuilder
	copies    map[string]string
	copyOrder []string
}

func newCgoWrapperSet(target cgoBindingTarget, sizes types.Sizes) *cgoWrapperSet {
	return &cgoWrapperSet{target: target, mirrors: newCgoMirrorBuilder(sizes), copies: map[string]string{}}
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

	var copyIn, copyOut strings.Builder

	for i := 0; i < structType.NumFields(); i++ {
		field := structType.Field(i)

		if field.Name() == "_" {
			continue
		}

		member := getCoreSanitizedIdentifier(field.Name())

		in, err := set.copyMember(function, named.Obj().Name()+"."+field.Name(), 0, "m."+member, "v."+member, field.Type(), true)

		if err != nil {
			return "", err
		}

		out, err := set.copyMember(function, named.Obj().Name()+"."+field.Name(), 0, "m."+member, "v."+member, field.Type(), false)

		if err != nil {
			return "", err
		}

		copyIn.WriteString(in)
		copyOut.WriteString(out)
	}

	var text strings.Builder

	fmt.Fprintf(&text, "    // %s's copy-in and copy-out, member by member (§5.2).\n", named.Obj().Name())
	fmt.Fprintf(&text, "    private static void ᴍin_%s(ref %s m, in %s v)\n    {\n%s    }\n\n", mirror.name, mirror.name, goSide, copyIn.String())
	fmt.Fprintf(&text, "    private static void ᴍout_%s(ref %s v, in %s m)\n    {\n%s    }\n", mirror.name, goSide, mirror.name, copyOut.String())

	return text.String(), nil
}

// copyMember writes the statement that copies one member, of type t, between the mirror expression m and the Go-side
// expression g, in (Go to mirror) or out.
func (set *cgoWrapperSet) copyMember(function, member string, depth int, m, g string, t types.Type, in bool) (string, error) {
	indent := strings.Repeat("    ", depth+2)

	if nested := cgoStructOf(t); nested != nil {
		mirror, err := set.mirrorOf(function, nested)

		if err != nil {
			return "", err
		}

		if in {
			return fmt.Sprintf("%sᴍin_%s(ref %s, %s);\n", indent, mirror.name, m, g), nil
		}

		return fmt.Sprintf("%sᴍout_%s(ref %s, %s);\n", indent, mirror.name, g, m), nil
	}

	switch underlying := t.Underlying().(type) {
	case *types.Array:
		index := fmt.Sprintf("i%d", depth)
		element, err := set.copyMember(function, member, depth+1, m+"["+index+"]", g+"["+index+"]", underlying.Elem(), in)

		if err != nil {
			return "", err
		}

		return fmt.Sprintf("%sfor (int %s = 0; %s < %d; %s++)\n%s", indent, index, index, underlying.Len(), index, element), nil

	case *types.Pointer:
		return "", fmt.Errorf("cgo wrapper: C.%s crosses %s, a pointer member, which is increment 3b2b's", function, member)
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

// cgoStructParam is one parameter: a scalar, a struct by value, or a struct by pointer.
type cgoStructParam struct {
	scalar  cgoScalar
	mirror  *cgoMirror
	goSide  string
	pointer bool
}

// param reads one parameter.
func (set *cgoWrapperSet) param(function string, index int, t types.Type) (cgoStructParam, error) {
	role := fmt.Sprintf("parameter %d", index+1)

	if pointer, ok := t.(*types.Pointer); ok {
		named := cgoStructOf(pointer.Elem())

		if named == nil {
			return cgoStructParam{}, fmt.Errorf("cgo wrapper: C.%s's %s is a pointer to something other than a C struct, which is Phase 3's", function, role)
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
			return cgoStructParam{}, fmt.Errorf("cgo wrapper: C.%s's %s is %s by value, which §11.7 refuses (a float, union or inline array member, or an all-padding eightbyte)", function, role, named.Obj().Name())
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

	// The two-result form and cmd/cgo's own helpers are the scalar binding's to take or refuse: a struct's errno form
	// waits for a measured arm.
	if declaration.kind != cgoFunc || cgoBuiltinNames[function] {
		return cgoScalarWrapper(declaration, scalarTarget)
	}

	signature, ok := declaration.object.Type().(*types.Signature)

	if !ok || signature.Results().Len() != 1 {
		return "", fmt.Errorf("cgo wrapper: C.%s has no one-result signature", function)
	}

	params := make([]cgoStructParam, signature.Params().Len())
	scalarsOnly := true

	for i := range params {
		param, err := set.param(function, i, signature.Params().At(i).Type())

		if err != nil {
			return "", err
		}

		params[i] = param
		scalarsOnly = scalarsOnly && param.mirror == nil
	}

	resultType := signature.Results().At(0).Type()
	resultStruct := cgoStructOf(resultType)
	resultPointer, pointerResult := resultType.(*types.Pointer)

	if scalarsOnly && resultStruct == nil && !pointerResult {
		return cgoScalarWrapper(declaration, scalarTarget)
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
			return "", fmt.Errorf("cgo wrapper: C.%s returns %s by value, which §11.7 refuses", function, resultStruct.Obj().Name())
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

	checked := map[string]bool{}

	for i, param := range params {
		switch {
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
			arguments = append(arguments, param.scalar.toPrimitive(fmt.Sprintf("p%d", i)))
		}

		if param.mirror != nil && !checked[param.mirror.name] {
			checked[param.mirror.name] = true
			fmt.Fprintf(&before, "        %s.CheckLayout%s();\n", param.mirror.name, cgoMirrorSuffix)
		}
	}

	if resultMirror != nil && !checked[resultMirror.name] {
		fmt.Fprintf(&before, "        %s.CheckLayout%s();\n", resultMirror.name, cgoMirrorSuffix)
	}

	for i, param := range params {
		switch {
		case param.pointer:
			fmt.Fprintf(&before, "\n        %s m%d = default;\n", param.mirror.name, i)

			// The pointer's kind is GoCgoArgs' to decide (§11.1): it refuses, before any native code runs, an element of a Go
			// array (C may touch the whole array) and a native box minted from managed storage (C must not write through it).
			fmt.Fprintf(&before, "        global::go.golib.GoCgoStructPointer k%d = %sStructPointer(p%d, %q, out nuint n%d);\n", i, args, i, function, i)
			fmt.Fprintf(&before, "        nint a%d = (nint)n%d;\n", i, i)

			// Two pointers to the same Go struct are one object to C, as they are under Go: the later one shares the
			// earlier one's mirror, so what C writes through either is what both see, and the struct is copied back once
			// (§5.2). Sameness is the storage's, not the box's, since two boxes can reach one struct.
			var earlier []int

			for j := 0; j < i; j++ {
				if params[j].pointer && params[j].mirror == param.mirror {
					earlier = append(earlier, j)
				}
			}

			copyIn := fmt.Sprintf("ᴍin_%s(ref m%d, p%d.Value);\n%[4]sa%[2]d = (nint)(&m%[2]d);\n", param.mirror.name, i, i, "            ")
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

			fmt.Fprintf(&after, "\n        if (%s)\n            ᴍout_%s(ref p%d.Value, m%d);\n", mirrored, param.mirror.name, i, i)

		case param.mirror != nil:
			fmt.Fprintf(&before, "\n        %s v%d = default;\n        ᴍin_%s(ref v%d, p%d);\n", param.mirror.name, i, param.mirror.name, i, i)
		}
	}

	switch {
	case resultMirror != nil:
		returned = fmt.Sprintf("\n        %s result = new();\n        ᴍout_%s(ref result, r);\n\n        return result;\n", wrapperResult, resultMirror.name)
	case pointerResult:
		returned = fmt.Sprintf("\n        return %sPointerMemberBack<%s>((nuint)r, null, 0);\n", args, strings.TrimSuffix(strings.TrimPrefix(wrapperResult, "ж<"), ">"))
	case resultScalar.isVoid:
		returned = "\n        return default;\n"
	default:
		returned = fmt.Sprintf("\n        return %s;\n", resultScalar.fromPrimitive("r"))
	}

	library := set.libraryOf(function)
	var text strings.Builder

	fmt.Fprintf(&text, "    // C.%s, bound to %s.\n", function, library)
	fmt.Fprintf(&text, "    [%sLibraryImport(%q, EntryPoint = %q)]\n", interop, library, function)
	fmt.Fprintf(&text, "    [%sUnmanagedCallConv(CallConvs = new[] { typeof(%sCallConvCdecl) })]\n", interop, compiler)
	fmt.Fprintf(&text, "    private static partial %s %s(%s);\n\n", externResult, native, strings.Join(externParams, ", "))
	fmt.Fprintf(&text, "    internal static %s %s(%s)\n    {\n", wrapperResult, getCoreSanitizedIdentifier(function), strings.Join(wrapperParams, ", "))
	text.WriteString(before.String())

	call := fmt.Sprintf("%s(%s)", native, strings.Join(arguments, ", "))

	if externResult != "void" {
		fmt.Fprintf(&text, "\n        %s r;\n", externResult)
		call = "r = " + call
	} else {
		text.WriteString("\n")
	}

	fmt.Fprintf(&text, "        %ssyscallEntersyscall();\n\n        try\n        {\n            %s;\n        }\n", runtime, call)
	fmt.Fprintf(&text, "        finally\n        {\n            %ssyscallExitsyscall();\n        }\n", runtime)
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

	return members
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
