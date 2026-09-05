// cgoUnsafeArgsLift.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// The //go:cgo_unsafe_args BLOCK LIFT (docs/phase4/DESIGN-cgo-unsafe-args-block-lift.md, Q56).
//
// Go's darwin runtime reaches libc through per-call-site assembly trampolines and hands each one
// the address of the function's FIRST PARAMETER: `libcCall(fn, unsafe.Pointer(&sig))` under the
// `//go:cgo_unsafe_args` directive, which is Go's guarantee that the parameters are laid out
// contiguously on the stack in declaration order — the trampoline reads the WHOLE parameter block
// through that one pointer (sigaction_trampoline: 0(DI) sig, 8(DI) new, 16(DI) old). The managed
// model has no such stack: boxing the first parameter alone hands the dispatcher ONE field, and
// every parameter behind it travels as whatever the registers held (the Q56 census's shape (a),
// silent), or is refused by type for a pointer first parameter (shape (c), loud).
//
// The lift makes the block EXPLICIT: for a function carrying the directive whose body passes the
// address of its first parameter to libcCall, the converter synthesizes a `[GoType("dyn")]`
// struct beside the function — the corpus's existing lifted-args form (`fcntl_args`) — with one
// field per parameter in Go's ABI0 order, constructs it at entry, and passes THAT block's pinned
// box. Field rule: an integer parameter keeps its Go width; a pointer-typed parameter becomes a
// `uintptr` field minted through golib's address model (`(uintptr)Ꮡp.OrTypedNil()`: 0 for nil,
// the pinned address for reference-free storage, the order token (Q44) for reference-bearing
// storage, so libc answers EFAULT instead of reading reordered managed memory); an unsafe.Pointer
// parameter is already a number and rides as one. Results are never fields — the C result is
// libcCall's return (increment 7). The dispatcher (GoLibcCall.DispatchArgsStruct) then places the
// fields in registers by reflection exactly as it does for `fcntl_args`; layout in bytes is never
// materialized, so the block's ORDER and WIDTHS are the whole contract, and they are Go's.
//
// What the rule leaves alone, by construction: a libcCall whose pointer is `nil` or `&args` (a
// local of anonymous struct type — the lifted family) or `&t` (a local or named result, the
// design's shape (b), handled by the dispatcher's per-symbol form table); a function WITHOUT the
// directive that happens to pass `&first` (no contiguity guarantee, no lift); a function displaced
// by manualConversionFuncs (no body is emitted for the rule to rewrite); and a parameter whose type
// the block cannot carry as an integer (a float, a string, a struct by value) — such a function is
// not lifted and converts as before.
//
// The lift is decided ONCE per package in a pre-pass (collectCgoUnsafeArgsLifts, run at the top of
// performEscapeAnalysis so every per-file analysis visitor and the emission visitor read one
// verdict): the first parameter's address-of inside the recognized call is CONSUMED by the lift, so
// the escape analysis must not heap-box that parameter for it (objectAddressTaken consults
// packageLiftConsumedAddressOf); the emission visitor writes the block declaration into the
// function prefix, the construction line into the block prefix, and renders the recognized
// `unsafe.Pointer(&first)` argument as the block's pinned box.

// cgoUnsafeArgsLift is one recognized lift site.
type cgoUnsafeArgsLift struct {
	funcDecl   *ast.FuncDecl
	call       *ast.CallExpr  // the libcCall(...) call
	conversion *ast.CallExpr  // its `unsafe.Pointer(&first)` argument
	addressOf  *ast.UnaryExpr // the `&first` operand
	firstParam *types.Var     // the parameter whose address the trampoline reads the block through
	params     []*types.Var   // every parameter, in Go's ABI0 order
	blockName  string         // the C# local holding the block (its box is AddressPrefix+blockName)
	typeName   string         // the lifted struct's C# name, claimed at emission
}

// packageCgoUnsafeArgsLifts maps each lifted FuncDecl to its site; packageLiftConsumedAddressOf marks
// the `&first` node each lift consumes. Both are rebuilt by collectCgoUnsafeArgsLifts per package.
var packageCgoUnsafeArgsLifts map[*ast.FuncDecl]*cgoUnsafeArgsLift
var packageLiftConsumedAddressOf map[*ast.UnaryExpr]bool

// cgoUnsafeArgsDirective is the pragma Go's compiler reads for the same purpose.
const cgoUnsafeArgsDirective = "//go:cgo_unsafe_args"

// collectCgoUnsafeArgsLifts is the package pre-pass. It is self-resetting: every call rebuilds both
// maps from the files it is given, so a package without the shape leaves them empty.
func collectCgoUnsafeArgsLifts(files []FileEntry, info *types.Info) {
	packageCgoUnsafeArgsLifts = map[*ast.FuncDecl]*cgoUnsafeArgsLift{}
	packageLiftConsumedAddressOf = map[*ast.UnaryExpr]bool{}

	for _, fileEntry := range files {
		for _, decl := range fileEntry.file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok {
				continue
			}

			if lift := recognizeCgoUnsafeArgsLift(funcDecl, info); lift != nil {
				packageCgoUnsafeArgsLifts[funcDecl] = lift
				packageLiftConsumedAddressOf[lift.addressOf] = true
			}
		}
	}
}

// recognizeCgoUnsafeArgsLift applies the design's §3.1 recognition to one declaration.
func recognizeCgoUnsafeArgsLift(funcDecl *ast.FuncDecl, info *types.Info) *cgoUnsafeArgsLift {
	if funcDecl.Body == nil || funcDecl.Recv != nil || !hasCgoUnsafeArgsDirective(funcDecl.Doc) {
		return nil
	}

	// A declaration displaced by manualConversionFuncs emits a placeholder: visitFuncDecl returns
	// before any lift hook runs, so recognizing it here is inert (the consumed-address mark only
	// changes the boxing of a parameter whose body is never emitted). Not gated on the registry —
	// the pre-pass carries no target platform, and the placeholder path already decides.

	funcObj, ok := info.ObjectOf(funcDecl.Name).(*types.Func)

	if !ok || funcObj == nil {
		return nil
	}

	signature := funcObj.Signature()

	if signature.Params().Len() == 0 || signature.Variadic() {
		return nil
	}

	params := make([]*types.Var, 0, signature.Params().Len())

	for i := 0; i < signature.Params().Len(); i++ {
		param := signature.Params().At(i)

		// Every parameter must be nameable (the block reads it by name) and register-carriable.
		if param.Name() == "" || param.Name() == "_" || cgoUnsafeArgsBlockFieldType(param.Type()) == "" {
			return nil
		}

		// A ref-LOWERED pointer parameter (ж-box A2) has no box to take an address from — the
		// lift reads pointers through their boxes, so such a function converts as before. None of
		// sys_darwin.go's sites lowers (every pointer parameter is also passed to KeepAlive).
		if packageRefLoweringResult != nil && packageRefLoweringResult.LoweredParamVars[param] {
			return nil
		}

		params = append(params, param)
	}

	firstParam := params[0]
	var lift *cgoUnsafeArgsLift
	sites := 0

	ast.Inspect(funcDecl.Body, func(n ast.Node) bool {
		// A nested function literal has its own frame; the directive speaks about THIS function's
		// parameter block only.
		if _, isLit := n.(*ast.FuncLit); isLit {
			return false
		}

		call, ok := n.(*ast.CallExpr)

		if !ok || !isLibcCallCallee(call, info) || len(call.Args) != 2 {
			return true
		}

		conversion, ok := ast.Unparen(call.Args[1]).(*ast.CallExpr)

		if !ok || len(conversion.Args) != 1 || !isUnsafePointerConversion(conversion, info) {
			return true
		}

		unary, ok := ast.Unparen(conversion.Args[0]).(*ast.UnaryExpr)

		if !ok || unary.Op != token.AND {
			return true
		}

		ident, ok := ast.Unparen(unary.X).(*ast.Ident)

		if !ok || info.ObjectOf(ident) != firstParam {
			return true
		}

		sites++
		lift = &cgoUnsafeArgsLift{funcDecl: funcDecl, call: call, conversion: conversion, addressOf: unary, firstParam: firstParam, params: params}

		return true
	})

	// Exactly ONE block read per function: sys_darwin.go has no other shape, and two sites would
	// need two constructions the emission does not model. A second `&first` OUTSIDE the call would
	// mean the parameter's storage escapes elsewhere too; the box analysis keeps it boxed then, and
	// the lift still reads the value through the alias — so only the call-site count is gated.
	if sites != 1 {
		return nil
	}

	return lift
}

// hasCgoUnsafeArgsDirective reports whether the doc comment group carries the pragma as its own line.
func hasCgoUnsafeArgsDirective(doc *ast.CommentGroup) bool {
	if doc == nil {
		return false
	}

	for _, comment := range doc.List {
		if strings.TrimSpace(comment.Text) == cgoUnsafeArgsDirective {
			return true
		}
	}

	return false
}

// isLibcCallCallee recognizes the darwin dispatch bottom: a package-level function named libcCall whose
// signature is (unsafe.Pointer, unsafe.Pointer) → one result. Named rather than path-bound so the
// converter's own fixture (a synthetic module declaring the same shape) exercises the rule.
func isLibcCallCallee(call *ast.CallExpr, info *types.Info) bool {
	ident, ok := ast.Unparen(call.Fun).(*ast.Ident)

	if !ok || ident.Name != "libcCall" {
		return false
	}

	funcObj, ok := info.ObjectOf(ident).(*types.Func)

	if !ok || funcObj == nil || funcObj.Type() == nil {
		return false
	}

	signature, ok := funcObj.Type().(*types.Signature)

	if !ok || signature.Recv() != nil || signature.Params().Len() != 2 || signature.Results().Len() != 1 {
		return false
	}

	for i := 0; i < 2; i++ {
		basic, ok := signature.Params().At(i).Type().Underlying().(*types.Basic)

		if !ok || basic.Kind() != types.UnsafePointer {
			return false
		}
	}

	return true
}

// isUnsafePointerConversion reports whether call is the conversion `unsafe.Pointer(x)`.
func isUnsafePointerConversion(call *ast.CallExpr, info *types.Info) bool {
	tv, ok := info.Types[call.Fun]

	if !ok || !tv.IsType() {
		return false
	}

	basic, ok := tv.Type.Underlying().(*types.Basic)

	return ok && basic.Kind() == types.UnsafePointer
}

// cgoUnsafeArgsBlockFieldType returns the C# type of the block field carrying a parameter of Go type
// t — "" when the block cannot carry it as an integer register. The design's field rule: pointers
// and unsafe.Pointer ride as uintptr; integers keep their width; a named integer type rides as its
// underlying width (the dispatcher places primitives, never wrappers).
func cgoUnsafeArgsBlockFieldType(t types.Type) string {
	if _, isPointer := types.Unalias(t).(*types.Pointer); isPointer {
		return "uintptr"
	}

	basic, ok := t.Underlying().(*types.Basic)

	if !ok {
		return ""
	}

	switch basic.Kind() {
	case types.UnsafePointer, types.Uintptr:
		return "uintptr"
	case types.Int:
		return "nint"
	case types.Uint:
		return "nuint"
	case types.Int8:
		return "int8"
	case types.Int16:
		return "int16"
	case types.Int32:
		return "int32"
	case types.Int64:
		return "int64"
	case types.Uint8:
		return "uint8"
	case types.Uint16:
		return "uint16"
	case types.Uint32:
		return "uint32"
	case types.Uint64:
		return "uint64"
	case types.Bool:
		return "bool"
	}

	return ""
}

// cgoUnsafeArgsBlockFieldValue renders the expression that fills the block field for param, given the
// parameter's emitted C# name (the analyzed value name for a value parameter; the box `Ꮡ<name>` for a
// pointer parameter, per the emitted signature).
func (v *Visitor) cgoUnsafeArgsBlockFieldValue(param *types.Var) string {
	if _, isPointer := types.Unalias(param.Type()).(*types.Pointer); isPointer {
		// The box IS the parameter (`ж<T> Ꮡp`); a nil argument may arrive as a null reference,
		// which OrTypedNil folds to the typed nil box whose address is 0 — the same spelling the
		// emitted KeepAlive lines use.
		return fmt.Sprintf("(uintptr)%s%s.OrTypedNil()", AddressPrefix, param.Name())
	}

	name := param.Name()

	if renamed, ok := v.varNames[param]; ok && renamed != "" {
		name = renamed
	}

	name = getSanitizedIdentifier(name)
	fieldType := cgoUnsafeArgsBlockFieldType(param.Type())

	basic, _ := param.Type().Underlying().(*types.Basic)

	if basic != nil && basic.Kind() == types.UnsafePointer {
		return fmt.Sprintf("(uintptr)%s", name)
	}

	// A NAMED integer type (`type pthread uintptr`) is a generated wrapper; the block field is the
	// underlying width, reached through the wrapper's conversion.
	if _, isNamed := types.Unalias(param.Type()).(*types.Named); isNamed {
		return fmt.Sprintf("(%s)%s", fieldType, name)
	}

	return name
}

// cgoUnsafeArgsBlockFieldName is the block field's C# name for a parameter: Go's own name, sanitized.
func cgoUnsafeArgsBlockFieldName(param *types.Var) string {
	return getSanitizedIdentifier(param.Name())
}

// beginCgoUnsafeArgsLift is called by visitFuncDecl once the function's prefix builder exists: it claims
// the block's type name, writes the `[GoType("dyn")]` declaration into the function prefix (the same
// place a function-local anonymous struct lifts to) and records the site on the visitor so the
// argument renderer and the block-prefix assembly find it. Returns nil when funcDecl is not a lift.
func (v *Visitor) beginCgoUnsafeArgsLift(funcDecl *ast.FuncDecl) *cgoUnsafeArgsLift {
	lift := packageCgoUnsafeArgsLifts[funcDecl]

	if lift == nil {
		return nil
	}

	site := *lift // the package record is shared by every visitor; the emission state is this visitor's
	site.typeName = v.getUniqueLiftedTypeName(funcDecl.Name.Name + "_args")
	site.blockName = cgoUnsafeArgsBlockLocalName(funcDecl)

	// The declaration, in the corpus's lifted-args form (sys_darwin.cs's fcntl_args): dyn-marked,
	// internal like every function-local lift, its accessibility recorded for package_info.cs.
	access := "internal "
	inlineAttrs := v.recordTypeAccessibility("struct", site.typeName, "", access, "")

	decl := &strings.Builder{}
	decl.WriteString(fmt.Sprintf("[GoType(\"dyn\")] %s%spartial struct %s {", inlineAttrs, access, site.typeName))
	decl.WriteString(v.newline)

	for _, param := range site.params {
		decl.WriteString(fmt.Sprintf("%sinternal %s %s;", v.indent(1), cgoUnsafeArgsBlockFieldType(param.Type()), cgoUnsafeArgsBlockFieldName(param)))
		decl.WriteString(v.newline)
	}

	decl.WriteString("}")
	decl.WriteString(v.newline)

	if v.currentFuncPrefix.Len() > 0 {
		v.currentFuncPrefix.WriteString(v.newline)
	}

	v.currentFuncPrefix.WriteString(decl.String())
	v.currentCgoLift = &site

	return &site
}

// cgoUnsafeArgsBlockLocalName picks the C# local for the block: `args`, unless the function itself
// declares that name (then `argsʗ`, the same marker the parameter renames use).
func cgoUnsafeArgsBlockLocalName(funcDecl *ast.FuncDecl) string {
	name := "args"
	taken := false

	ast.Inspect(funcDecl, func(n ast.Node) bool {
		if ident, ok := n.(*ast.Ident); ok && ident.Name == name {
			taken = true
		}

		return !taken
	})

	if taken {
		return name + "ʗ"
	}

	return name
}

// cgoUnsafeArgsBlockPrologue renders the construction line the function's block prefix carries:
// `ref var args = ref heap(new <T>(f1, f2, …), out var Ꮡargs);`.
func (v *Visitor) cgoUnsafeArgsBlockPrologue(lift *cgoUnsafeArgsLift) string {
	values := make([]string, len(lift.params))

	for i, param := range lift.params {
		values[i] = v.cgoUnsafeArgsBlockFieldValue(param)
	}

	return fmt.Sprintf("ref var %s = ref %s(new %s(%s), out var %s%s);", lift.blockName, v.heapIntrinsicName(), lift.typeName, strings.Join(values, ", "), AddressPrefix, lift.blockName)
}

// cgoUnsafeArgsBlockPointer renders the libcCall argument the lift substitutes for `unsafe.Pointer(&first)`:
// the block's box through the RETAINING door, exactly as the lifted family's `&args` already renders.
func cgoUnsafeArgsBlockPointer(lift *cgoUnsafeArgsLift) string {
	return fmt.Sprintf("@unsafe.Pointer.FromPinnedBox(%s%s)", AddressPrefix, lift.blockName)
}
