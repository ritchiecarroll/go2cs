// cgoWrappers.go - Gbtc
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

// Phase 2 of docs/PLAN-cgo-interop.md, increment 3b1 (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §4.2, read through §11):
// the binding of a C function whose parameters and result are scalars. Each is a [LibraryImport] extern on fixed-width
// primitives, plus the Go-callable wrapper cmd/cgo's _Cfunc_ declared: it crosses through the runtime's entersyscall
// family (§11.4) and converts between the C types' Go-side names and the extern's primitives. The two-result form,
// r, err := C.f(…), reads errno inside the transition (§11.3). Structs are increment 3b2; pointers are Phase 3.
// Nothing here is emitted into a conversion yet: the bindings file is increment 5's.

// cgoPseudoPackageClass names the class a cgo package's C declarations live in (§11.17, RULED): in Go, C is a
// pseudo-package, so here it is a package class of its own, and each converted file that imports "C" reaches it
// through `using C = <class>;`, the alias converted imports already use. The name cannot collide with a real Go
// package's class: TypeAliasDot is not a character a Go identifier may hold, and Go reserves `import "C"` for cgo.
func cgoPseudoPackageClass(packageName string) string {
	return packageName + TypeAliasDot + "C" + PackageSuffix
}

// cgoBindingTarget is the target a binding is written for: its GOOS, the library the symbol is bound in, and on windows
// the C runtime that library uses, whose errno the two-result form reads (§11.3).
type cgoBindingTarget struct {
	goos, library, crt string
}

// cgoErrnoFormSuffix ends the C# name of a two-result binding. cmd/cgo declares C.f's plain form beside its errno form,
// and C# cannot overload on the result alone, so `r, err := C.exp(x)` reaches C.expꓸerrno.
const cgoErrnoFormSuffix = TypeAliasDot + "errno"

// cgoBuiltinNames are cmd/cgo's own helpers declared as _Cfunc_ (cmd/cgo/out.go, builtinDefs): Phase 3's.
var cgoBuiltinNames = map[string]bool{"CString": true, "CBytes": true, "GoString": true, "GoStringN": true, "GoBytes": true}

// cgoScalar is one parameter or result: its Go-side C# type, the extern's primitive, and the Go basic type that
// bridges them.
type cgoScalar struct {
	goSide    string
	primitive string
	basic     string
	isBool    bool
	isVoid    bool
}

// cgoScalarOf reads one parameter or result type, or refuses it by name.
func cgoScalarOf(function, role string, t types.Type) (cgoScalar, error) {
	goSide := ""

	if named, ok := types.Unalias(t).(*types.Named); ok {
		cName, found := strings.CutPrefix(named.Obj().Name(), "_Ctype_")

		if !found {
			return cgoScalar{}, fmt.Errorf("cgo wrapper: C.%s's %s is a %s, not a C type", function, role, named.Obj().Name())
		}

		goSide = getCoreSanitizedIdentifier(cName)

		if cName == "void" {
			return cgoScalar{goSide: goSide, isVoid: true}, nil
		}
	}

	basic, ok := t.Underlying().(*types.Basic)

	if !ok || basic.Kind() == types.UnsafePointer || basic.Kind() == types.String {
		return cgoScalar{}, fmt.Errorf("cgo wrapper: C.%s's %s is not a scalar (a C struct is increment 3b2; any other pointer is Phase 3)", function, role)
	}

	primitive, found := cgoMirrorBasic[basic.Kind()]

	if !found || basic.Kind() == types.Uintptr {
		return cgoScalar{}, fmt.Errorf("cgo wrapper: C.%s's %s is of basic kind %s, which Phase 2 does not bind", function, role, basic.Name())
	}

	if goSide == "" {
		goSide = basic.Name()
	}

	return cgoScalar{goSide: goSide, primitive: primitive, basic: basic.Name(), isBool: basic.Kind() == types.Bool}, nil
}

// toPrimitive is the expression that hands value to the extern.
func (scalar cgoScalar) toPrimitive(value string) string {
	if scalar.isBool {
		return fmt.Sprintf("(byte)((bool)%s ? 1 : 0)", value)
	}

	return fmt.Sprintf("(%s)%s", scalar.basic, value)
}

// fromPrimitive is the expression that hands the extern's result back as the Go-side type.
func (scalar cgoScalar) fromPrimitive(value string) string {
	if scalar.isBool {
		return fmt.Sprintf("(%s)(%s != 0)", scalar.goSide, value)
	}

	return fmt.Sprintf("(%s)(%s)%s", scalar.goSide, scalar.basic, value)
}

// cgoScalarWrapper writes one C function's extern and wrapper, as members of its pseudo-package class, binding the
// symbol in target's library (increment 4 captures the name per symbol; until then the caller supplies it). declaration
// is a _Cfunc_ or _C2func_ the recognizer classified. Anything not wholly scalar is refused by name.
func cgoScalarWrapper(declaration cgoDeclaration, target cgoBindingTarget) (string, error) {
	library := target.library
	function := declaration.cName
	errnoForm := declaration.kind == cgoErrnoFunc

	if declaration.kind != cgoFunc && !errnoForm {
		return "", fmt.Errorf("cgo wrapper: C.%s is not a C function", function)
	}

	if cgoBuiltinNames[function] {
		return "", fmt.Errorf("cgo wrapper: C.%s is one of cmd/cgo's own helpers, which Phase 3 provides", function)
	}

	signature, ok := declaration.object.Type().(*types.Signature)

	if !ok {
		return "", fmt.Errorf("cgo wrapper: C.%s has no function signature", function)
	}

	params := make([]cgoScalar, signature.Params().Len())

	for i := range params {
		param, err := cgoScalarOf(function, fmt.Sprintf("parameter %d", i+1), signature.Params().At(i).Type())

		if err != nil {
			return "", err
		}

		if param.isVoid {
			return "", fmt.Errorf("cgo wrapper: C.%s's parameter %d is void", function, i+1)
		}

		params[i] = param
	}

	wantResults := 1

	if errnoForm {
		wantResults = 2
	}

	if signature.Results().Len() != wantResults {
		return "", fmt.Errorf("cgo wrapper: C.%s has %d results, want %d", function, signature.Results().Len(), wantResults)
	}

	result, err := cgoScalarOf(function, "result", signature.Results().At(0).Type())

	if err != nil {
		return "", err
	}

	const interop = "global::System.Runtime.InteropServices."
	const compiler = "global::System.Runtime.CompilerServices."
	const runtime = "global::go.runtime_package."

	native := function + cgoMirrorSuffix + "native"
	name := getCoreSanitizedIdentifier(function)

	if errnoForm {
		native += "errno"
		name = function + cgoErrnoFormSuffix
	}

	externResult := result.primitive

	if result.isVoid {
		externResult = "void"
	}

	var externParams, wrapperParams, arguments []string

	for i, param := range params {
		externParams = append(externParams, fmt.Sprintf("%s p%d", param.primitive, i))
		wrapperParams = append(wrapperParams, fmt.Sprintf("%s p%d", param.goSide, i))
		arguments = append(arguments, param.toPrimitive(fmt.Sprintf("p%d", i)))
	}

	wrapperResult := result.goSide

	if errnoForm {
		wrapperResult = fmt.Sprintf("(%s, error)", result.goSide)
	}

	var text strings.Builder
	setLastError := ""

	// The errno form reads errno inside the transition (§11.3, cgoErrnoCall): through SetLastError on linux and darwin,
	// and through the C runtime's own slot on windows.
	if errnoForm && target.goos != "windows" {
		setLastError = ", SetLastError = true"
	}

	fmt.Fprintf(&text, "    // C.%s, bound to %s.\n", function, library)
	fmt.Fprintf(&text, "    [%sLibraryImport(%q, EntryPoint = %q%s)]\n", interop, library, function, setLastError)
	fmt.Fprintf(&text, "    [%sUnmanagedCallConv(CallConvs = new[] { typeof(%sCallConvCdecl) })]\n", interop, compiler)
	fmt.Fprintf(&text, "    private static partial %s %s(%s);\n\n", externResult, native, strings.Join(externParams, ", "))
	fmt.Fprintf(&text, "    internal static %s %s(%s)\n    {\n", wrapperResult, name, strings.Join(wrapperParams, ", "))

	call := fmt.Sprintf("%s(%s)", native, strings.Join(arguments, ", "))

	if !result.isVoid {
		fmt.Fprintf(&text, "        %s r;\n", result.primitive)
		call = "r = " + call
	}

	if errnoForm {
		fmt.Fprintf(&text, "        int errno;\n")
	}

	// The bracket is Go's cgocall: the runtime's entersyscall family moves the goroutine to _Gsyscall and opens the
	// native-call seam once (§11.4).
	fmt.Fprintf(&text, "        %ssyscallEntersyscall();\n\n        try\n        {\n", runtime)

	text.WriteString(cgoErrnoCall(call, errnoForm, target.goos))
	fmt.Fprintf(&text, "        }\n        finally\n        {\n            %ssyscallExitsyscall();\n        }\n", runtime)

	returned := "default"

	if !result.isVoid {
		returned = result.fromPrimitive("r")
	}

	if errnoForm {
		fmt.Fprintf(&text, "\n%s    }\n", cgoErrnoReturn(returned))
	} else {
		fmt.Fprintf(&text, "\n        return %s;\n    }\n", returned)
	}

	return text.String(), nil
}

// cgoErrnoCall writes the native call's statements inside the transition, with the errno form's read of errno (§11.3).
// On linux and darwin the binding captures errno itself (SetLastError): its stub clears errno before the call and saves
// it after, so the read sees this call's errno and nothing the GC transition did since. On windows SetLastError captures
// GetLastError, not errno, which is the C RUNTIME's own slot, so the wrapper reads that through the library's C runtime
// instead (cgoErrnoLocationBinding).
func cgoErrnoCall(call string, errnoForm bool, goos string) string {
	const interop = "global::System.Runtime.InteropServices."

	var text strings.Builder

	if errnoForm && goos == "windows" {
		// cgo's own trampoline, in managed code: the thread's errno slot is taken BEFORE the call, zeroed, and read as
		// the very next statement after it, with no P/Invoke between the call's return and the read. The slot is the
		// calling thread's, and one synchronous wrapper does not change threads. What remains: runtime code on this
		// thread between the stub's return and the read could still touch the C runtime's errno; Phase 4's native shim
		// closes that.
		fmt.Fprintf(&text, "            nint errnoAddress = %s();\n", cgoErrnoLocation)
		fmt.Fprintf(&text, "            %sMarshal.WriteInt32(errnoAddress, 0);\n", interop)
		fmt.Fprintf(&text, "            %s;\n", call)
		fmt.Fprintf(&text, "            errno = %sMarshal.ReadInt32(errnoAddress);\n", interop)
	} else {
		fmt.Fprintf(&text, "            %s;\n", call)

		if errnoForm {
			fmt.Fprintf(&text, "            errno = %sMarshal.GetLastPInvokeError();\n", interop)
		}
	}

	return text.String()
}

// cgoErrnoReturn writes the errno form's return of returned under cgo's own rule: a zero errno is a nil error, and any
// other is syscall.Errno(errno).
func cgoErrnoReturn(returned string) string {
	return "        error err = null;\n\n        if (errno != 0)\n            err = (global::go.syscall_package.Errno)(uintptr)(nuint)(uint)errno;\n\n" +
		fmt.Sprintf("        return (%s, err);\n", returned)
}

// cgoErrnoLocation is the C# name of the binding of a windows C runtime's _errno().
const cgoErrnoLocation = "errno" + cgoMirrorSuffix + "location"

// cgoErrnoLocationBinding writes the binding of crt's _errno(), the calling thread's errno slot, which a pseudo-package
// with an errno form on windows needs once (§11.3, amended 2026-11-10).
func cgoErrnoLocationBinding(crt string) string {
	const interop = "global::System.Runtime.InteropServices."
	const compiler = "global::System.Runtime.CompilerServices."

	var text strings.Builder

	fmt.Fprintf(&text, "    // The calling thread's errno slot in %s, the C runtime the bound library uses: on windows SetLastError\n", crt)
	text.WriteString("    // captures GetLastError, not errno, so an errno form reads the C runtime's own slot (§11.3).\n")
	fmt.Fprintf(&text, "    [%sLibraryImport(%q, EntryPoint = \"_errno\")]\n", interop, crt)
	fmt.Fprintf(&text, "    [%sUnmanagedCallConv(CallConvs = new[] { typeof(%sCallConvCdecl) })]\n", interop, compiler)
	fmt.Fprintf(&text, "    private static partial nint %s();\n", cgoErrnoLocation)

	return text.String()
}

// cgoPseudoPackageText writes a cgo package's pseudo-package class around the members given, in C name order.
func cgoPseudoPackageText(packageName string, members map[string]string) string {
	names := make([]string, 0, len(members))

	for name := range members {
		names = append(names, name)
	}

	sort.Strings(names)

	var text strings.Builder

	fmt.Fprintf(&text, "// The C pseudo-package of Go package %s (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.17): what cmd/cgo\n", packageName)
	text.WriteString("// declared for its C names, reached from each converted file that imports \"C\" through `using C = <this class>;`.\n")
	text.WriteString("// The class name cannot collide with a real Go package's: 'ꓸ' is not a Go identifier character, and Go reserves\n")
	text.WriteString("// `import \"C\"` for cgo. Every member is internal: cgo forbids a C type in an exported Go signature.\n")
	text.WriteString("// It is unsafe because a C struct crosses by its stack mirror's address. That needs the project's\n")
	text.WriteString("// AllowUnsafeBlocks, which [LibraryImport] already does (SYSLIB1062); the converter's value for it is computed,\n")
	text.WriteString("// so a package with bindings has to raise it (allowUnsafeBlocks, projectFileWriter.go).\n")
	text.WriteString("// The class carries no accessibility of its own: go2cs-gen declares every package class it generates into\n")
	text.WriteString("// `public static partial`, and a second accessibility on another part is CS0262. The members carry it.\n")
	fmt.Fprintf(&text, "static unsafe partial class %s\n{\n", cgoPseudoPackageClass(packageName))

	for i, name := range names {
		if i > 0 {
			text.WriteString("\n")
		}

		text.WriteString(members[name])
	}

	text.WriteString("}\n")

	return text.String()
}
