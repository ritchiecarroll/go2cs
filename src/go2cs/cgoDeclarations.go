// cgoDeclarations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"path/filepath"
	"sort"
	"strings"
)

// Phase 1 of docs/PLAN-cgo-interop.md (§6, read through §11): recognition and file mapping for a cgo package,
// with no C# emitted. Nothing calls this yet; the driver still refuses every cgo source (S3 and visitImportSpec's
// gate), and increment 2 routes a -cgo load through it.
//
// When a C toolchain is present, cmd/cgo rewrites each `import "C"` source before go/types runs: every C.x
// reference becomes a typed synthetic Go declaration whose NAME encodes what it is (⟨OQ-1⟩, answered by lane P1,
// §11.7). So no C parser is needed: recognition is reading those names off the type-checked package scope.

// cgoDeclKind classifies one cgo declaration by the prefix cmd/cgo gives its name.
type cgoDeclKind int

const (
	cgoFunc        cgoDeclKind = iota // _Cfunc_x: a C function (C.free), or cgo's helpers C.CString, C.GoString, …
	cgoStruct                         // _Ctype_struct_x: C.struct_x
	cgoUnion                          // _Ctype_union_x: C.union_x
	cgoEnum                           // _Ctype_enum_x: C.enum_x (its enumerators arrive as cgoIntConst)
	cgoType                           // _Ctype_x: a C base type (C.int) or a typedef (C.xptr)
	cgoIntConst                       // _Ciconst_x: an integer #define or an enumerator
	cgoFloatConst                     // _Cfconst_x: a floating-point #define
	cgoStringConst                    // _Csconst_x: a string #define
	cgoVar                            // _Cvar_x: a C global variable (C.x)
)

// cgoKindPrefixes maps each synthetic-name prefix to its kind. _Ctype_ is split further by cgoTypeKind.
var cgoKindPrefixes = []struct {
	prefix string
	kind   cgoDeclKind
}{
	{"_Cfunc_", cgoFunc},
	{"_Ctype_", cgoType},
	{"_Ciconst_", cgoIntConst},
	{"_Cfconst_", cgoFloatConst},
	{"_Csconst_", cgoStringConst},
	{"_Cvar_", cgoVar},
}

// cgoDeclaration is one recognized cgo declaration: the name a Go source writes after `C.`, its kind, and the
// typed synthetic object cmd/cgo declared for it.
type cgoDeclaration struct {
	cName  string
	kind   cgoDeclKind
	object types.Object
}

// cgoDeclarations returns every cgo declaration in pkg's scope, classified and sorted by C name. It FILTERS cgo's
// plumbing rather than reporting it (P1's list, §11.7): `_cgo_<hash>_Cfunc_*`, `__cgofn_*`, `_Cgo_use`/`_ptr`/
// `_keepalive`/`_no_callback`/`_always_false`, `_cgoCheckPointer`, `_cgo_runtime_cgocall` and `runtime_throw` carry
// none of the recognized prefixes, and what cgo declares for its own use under a recognized prefix (C.CString's
// allocator `_CMalloc`, the `_GoString_` type) has a C name C reserves for the implementation.
//
// The C types cmd/cgo declares on the way to the ones a source names (a struct field's `uchar`, `intgo`'s chain to
// `long`) are reported too: a converted struct needs its fields' types.
func cgoDeclarations(pkg *types.Package) []cgoDeclaration {
	var declarations []cgoDeclaration

	scope := pkg.Scope()

	for _, name := range scope.Names() {
		declaration, ok := classifyCgoName(name)

		if !ok {
			continue
		}

		declaration.object = scope.Lookup(name)
		declarations = append(declarations, declaration)
	}

	sort.SliceStable(declarations, func(i, j int) bool {
		return declarations[i].cName < declarations[j].cName
	})

	return declarations
}

// classifyCgoName reads one scope name. ok is false for any name that is not a recognized cgo declaration.
func classifyCgoName(name string) (declaration cgoDeclaration, ok bool) {
	for _, entry := range cgoKindPrefixes {
		cName, found := strings.CutPrefix(name, entry.prefix)

		if !found || cName == "" || reservedCName(cName) {
			continue
		}

		kind := entry.kind

		if kind == cgoType {
			kind = cgoTypeKind(cName)
		}

		return cgoDeclaration{cName: cName, kind: kind}, true
	}

	return cgoDeclaration{}, false
}

// reservedCName reports whether cName is reserved for the C implementation: an underscore and a capital letter.
func reservedCName(cName string) bool {
	return len(cName) > 1 && cName[0] == '_' && cName[1] >= 'A' && cName[1] <= 'Z'
}

// cgoTypeKind splits a _Ctype_ name by the C tag Go spells into it: C.struct_x, C.union_x and C.enum_x.
func cgoTypeKind(cName string) cgoDeclKind {
	switch {
	case strings.HasPrefix(cName, "struct_"):
		return cgoStruct
	case strings.HasPrefix(cName, "union_"):
		return cgoUnion
	case strings.HasPrefix(cName, "enum_"):
		return cgoEnum
	default:
		return cgoType
	}
}

// cgoSourceFile maps one syntax tree of a cgo package to the source it came from. generated is the path the parser
// was handed; source is the `import "C"` file the tree was rewritten from, or "" for a tree cmd/cgo wrote from
// nothing (_cgo_gotypes.go, _cgo_import.go).
type cgoSourceFile struct {
	file      *ast.File
	generated string
	source    string
}

// cgoSourceFiles maps each of files to its source by the //line-ADJUSTED position of its package clause: cmd/cgo
// opens every <name>.cgo1.go it writes with a //line directive naming the original, and writes its own files with
// none. It is the inverse of syntaxSourceFiles, which pairs on the UNADJUSTED path on purpose so that it never
// mistakes an intermediate for the source; here the intermediate's pointer back is exactly what is wanted.
//
// go list writes that //line path absolute; `go tool cgo -trimpath` writes it relative, and go/scanner resolves a
// relative one against the directory of the file that carries it. Either way the adjusted path names the source.
func cgoSourceFiles(fset *token.FileSet, files []*ast.File) []cgoSourceFile {
	mapped := make([]cgoSourceFile, 0, len(files))

	for _, file := range files {
		generated := fset.PositionFor(file.Package, false).Filename
		named := fset.PositionFor(file.Package, true).Filename

		entry := cgoSourceFile{file: file, generated: generated}

		if named != generated {
			entry.source = filepath.Clean(named)
		}

		mapped = append(mapped, entry)
	}

	return mapped
}
