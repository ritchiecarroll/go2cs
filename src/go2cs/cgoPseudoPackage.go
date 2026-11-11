// cgoPseudoPackage.go - Gbtc
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
	"log"
	"sort"
	"strings"

	"github.com/ritchiecarroll/hashset"
)

// Phase 2 of docs/PLAN-cgo-interop.md, increment 3d (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.17, as amended): the
// C types and constants cmd/cgo declared for a package's C names are emitted into its C pseudo-package,
// <pkg>ꓸC_package, under their C names, and every reference reaches them as `C.<name>` through the file's
// `using C = <pkg>ꓸC_package;`. They are otherwise ordinary converted declarations: go2cs-gen expands them there as it
// does in any package class, since the class name ends in _package.
//
// The routing is by NAME, like nameCollisions: cmd/cgo reserves the _Ctype_ and _C?const_ prefixes (classifyCgoName),
// so a package that declares none converts exactly as before. A C typedef that cmd/cgo writes as a Go alias
// (type _Ctype_div_t = _Ctype_struct___0) is a `global using Cꓸdiv_t`, the converter's imported-type alias spelling.

// cgoPseudoPackageNames maps each C type or constant name cmd/cgo declared in the package being converted to its member
// name in the pseudo-package: _Ctype_int to @int, _Ctype_struct_point to struct_point, _Ciconst_LIMIT to LIMIT. A typedef
// alias maps to its `Cꓸ` spelling instead. Package-scoped, reset with the rest of the package state.
var cgoPseudoPackageNames map[string]string

// cgoPseudoPackageTypeAccess holds the C types' accessibility records, written into the pseudo-package class's
// declaration in package_info.cs (applyCgoPseudoPackageStamp). Reset with the rest of the package state.
var cgoPseudoPackageTypeAccess hashset.HashSet[string]

// cgoPseudoPackageAlias is the qualifier every reference to a pseudo-package member is written through.
const cgoPseudoPackageAlias = "C"

// collectCgoPseudoPackageNames records the package's C types and constants (performNameCollisionAnalysis calls it, as
// the other package-wide renames are decided there).
func collectCgoPseudoPackageNames(pkg *types.Package) {
	cgoPseudoPackageNames = map[string]string{}

	if pkg == nil {
		return
	}

	for _, declaration := range cgoDeclarations(pkg) {
		if !cgoPseudoPackageKind(declaration.kind) || declaration.object == nil {
			continue
		}

		member := getCoreSanitizedIdentifier(declaration.cName)

		if typeName, isType := declaration.object.(*types.TypeName); isType && typeName.IsAlias() {
			member = cgoPseudoPackageAlias + TypeAliasDot + member
		}

		cgoPseudoPackageNames[declaration.object.Name()] = member
	}
}

// cgoPseudoPackageKind reports whether a declaration kind is emitted into the pseudo-package by this increment: the C
// types and the constants. Functions, variables and macros are increment 5's.
func cgoPseudoPackageKind(kind cgoDeclKind) bool {
	switch kind {
	case cgoStruct, cgoUnion, cgoEnum, cgoType, cgoIntConst, cgoFloatConst, cgoStringConst:
		return true
	}

	return false
}

// cgoEmittingPseudoPackage is set while a file's pseudo-package part is visited (visitFile), where a member is declared
// and named bare, as any member is inside its own class. A package's files are visited one at a time, so this is
// package state like the rest.
var cgoEmittingPseudoPackage bool

// cgoPseudoPackageReference is how a Go name is written when it names a pseudo-package member: `C.@int` outside the
// pseudo-package and `@int` inside it, or a typedef's `Cꓸdiv_t` anywhere.
func cgoPseudoPackageReference(goName string) (string, bool) {
	member, found := cgoPseudoPackageNames[goName]

	if !found {
		return "", false
	}

	if cgoEmittingPseudoPackage || strings.HasPrefix(member, cgoPseudoPackageAlias+TypeAliasDot) {
		return member, true
	}

	return cgoPseudoPackageAlias + "." + member, true
}

// cgoPseudoPackageDecl reports whether a top-level declaration is one of cmd/cgo's C type or constant declarations,
// which the file emits into its pseudo-package part. cmd/cgo writes each in a declaration of its own, so one that mixes
// C and Go names is refused by name rather than split.
func (v *Visitor) cgoPseudoPackageDecl(decl ast.Decl) bool {
	genDecl, ok := decl.(*ast.GenDecl)

	if !ok || len(cgoPseudoPackageNames) == 0 || (genDecl.Tok != token.TYPE && genDecl.Tok != token.CONST) {
		return false
	}

	var cNames, goNames []string

	for _, spec := range genDecl.Specs {
		var names []*ast.Ident

		switch spec := spec.(type) {
		case *ast.TypeSpec:
			names = []*ast.Ident{spec.Name}
		case *ast.ValueSpec:
			names = spec.Names
		}

		for _, name := range names {
			if _, found := cgoPseudoPackageNames[name.Name]; found {
				cNames = append(cNames, name.Name)
			} else {
				goNames = append(goNames, name.Name)
			}
		}
	}

	if len(cNames) == 0 {
		return false
	}

	if len(goNames) > 0 {
		position := v.fset.Position(genDecl.Pos())
		message := fmt.Sprintf("one declaration mixes cmd/cgo's C names (%s) with Go names (%s); cmd/cgo writes each C declaration alone, so go2cs refuses to split this one", strings.Join(cNames, ", "), strings.Join(goNames, ", "))
		reportDiagnostic(position.String(), diagnosticCgoRefused, message)
		log.Fatalf("Refusing to convert: %s: %s\n", position, message)
	}

	return true
}

// namesCgoPseudoPackageMember reports whether a file declares or references any pseudo-package member.
func (v *Visitor) namesCgoPseudoPackageMember(file *ast.File) bool {
	if len(cgoPseudoPackageNames) == 0 {
		return false
	}

	found := false

	ast.Inspect(file, func(node ast.Node) bool {
		if ident, ok := node.(*ast.Ident); ok && !found {
			_, found = cgoPseudoPackageNames[ident.Name]
		}

		return !found
	})

	return found
}

// cgoPseudoPackageQualifier is samePackageTypeQualifier's form for the pseudo-package: what a `global using` alias RHS
// writes before a C type's member name.
func cgoPseudoPackageQualifier() string {
	nsSegments := strings.TrimPrefix(strings.TrimPrefix(packageNamespace, RootNamespace), ".")
	classQualifier := getSanitizedImport(cgoPseudoPackageClass(packageName))

	if nsSegments == "" {
		return classQualifier + "/"
	}

	return nsSegments + "." + classQualifier + "/"
}

// applyCgoPseudoPackageStamp gives the package info file exactly one declaration of the package's C pseudo-package class,
// and its `using C` alias, when the package declares C names, and neither when it does not. The declaration carries the
// class's [GoPackage] stamp, which is reflection's authority for a class's Go package (GoReflect.goPackageNameOf), so a
// C type prints as Go prints it, main._Ctype_int, not as its class name would decode. Its body holds the C types'
// accessibility records, as the package class's <TypeAccessibility> section holds the package's (section 11 of
// docs/PLAN-marker-comment-parity.md). It is written once per package here, since an attribute on a partial class
// cannot repeat across the files that declare parts of it (CS0579). mergeExisting keeps a persisted block's records, as
// the package class's section does on a single-file conversion.
func applyCgoPseudoPackageStamp(lines []string, mergeExisting bool) []string {
	className := getSanitizedImport(cgoPseudoPackageClass(packageName))
	declaration := "partial class " + className
	alias := fmt.Sprintf("using %s = %s.%s;", cgoPseudoPackageAlias, packageNamespace, className)
	packageClassUsing := fmt.Sprintf("using static %s.%s;", packageNamespace, getSanitizedImport(packageName+PackageSuffix))
	records := hashset.HashSet[string]{}
	kept := make([]string, 0, len(lines)+6)

	for i := 0; i < len(lines); i++ {
		// The file's own records name C types too (a GoFieldAccess record), so it reaches them as every converted file
		// does, through `C`. The line is the converter's, rewritten on every conversion.
		if strings.TrimSpace(lines[i]) == alias {
			continue
		}

		if strings.TrimSpace(lines[i]) == packageClassUsing && len(cgoPseudoPackageNames) > 0 {
			kept = append(kept, lines[i], alias)
			continue
		}

		// A persisted block is one blank line, the stamp, the declaration and its body of records.
		if strings.TrimSpace(lines[i]) == declaration && len(kept) > 0 && strings.HasPrefix(strings.TrimSpace(kept[len(kept)-1]), goPackageStampPrefix) {
			kept = kept[:len(kept)-1]

			if len(kept) > 0 && strings.TrimSpace(kept[len(kept)-1]) == "" {
				kept = kept[:len(kept)-1]
			}

			for i++; i < len(lines) && strings.TrimSpace(lines[i]) != "}"; i++ {
				if record := strings.TrimSpace(lines[i]); mergeExisting && record != "" && record != "{" {
					records.Add(record)
				}
			}

			continue
		}

		kept = append(kept, lines[i])
	}

	if len(cgoPseudoPackageNames) == 0 {
		return kept
	}

	for len(kept) > 0 && strings.TrimSpace(kept[len(kept)-1]) == "" {
		kept = kept[:len(kept)-1]
	}

	records.UnionWith(cgoPseudoPackageTypeAccess.Keys())

	sorted := records.Keys()
	sort.Slice(sorted, func(i, j int) bool {
		return typeAccessibilityKey(sorted[i]) < typeAccessibilityKey(sorted[j])
	})

	importPath := goPackageImportPathFor(getSanitizedImport(packageName+PackageSuffix), packageName)
	kept = append(kept, "", goPackageAttributeLine(packageNamespace, packageName, importPath), declaration, "{")

	for _, record := range sorted {
		kept = append(kept, typeAccessibilityIndent+record)
	}

	return append(kept, "}")
}
