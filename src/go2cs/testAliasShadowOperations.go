// testAliasShadowOperations.go - Gbtc
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
	"go/build"
	"go/parser"
	"go/token"
	"go/types"
	"golang.org/x/tools/go/packages"
	"os"
	"path/filepath"
	"sort"
	"strings"
)

// siblingTestSignals carries what a PRODUCTION conversion must know about the package's build-
// selected in-package `_test.go` half. Those files compile into the same C# package class (or, under
// the white-box model, into a friend bridge over it), so they can shadow a production import alias
// and can take the address of a production package-level var — neither of which the production
// package go/packages loads can see, because it excludes `_test.go` entirely.
type siblingTestSignals struct {
	// funcMethodNames are the package-level function/method declarator names (see
	// siblingTestFuncMethodNames).
	funcMethodNames []string

	// addressedGlobalNames are the identifiers whose address the test half takes and that the test
	// file itself does not declare — candidate production package-level vars (see
	// siblingTestAddressedGlobalNames).
	addressedGlobalNames []string

	// hasInternalTests reports whether any build-selected in-package `_test.go` file exists.
	hasInternalTests bool
}

// collectSiblingTestSignals scans the build-selected in-package `_test.go` files for the signals a
// production conversion needs (see siblingTestSignals). A direct directory scan keeps ordinary
// single-package retranspiles fast: no test dependency graph or second go/packages type-check is
// needed. External-package tests are deliberately excluded — their declarations emit into a
// different C# package class, and they can only reach the package through its exported surface.
func collectSiblingTestSignals(packageDir, packageName string, options Options) siblingTestSignals {
	if packageDir == "" {
		return siblingTestSignals{}
	}

	targetParts := strings.Split(options.targetPlatform, "/")
	if len(targetParts) != 2 {
		return siblingTestSignals{}
	}

	buildContext := build.Default
	buildContext.GOOS = targetParts[0]
	buildContext.GOARCH = targetParts[1]
	buildContext.BuildTags = append([]string(nil), options.buildTags...)

	entries, err := os.ReadDir(packageDir)
	if err != nil {
		return siblingTestSignals{}
	}

	names := HashSet[string]{}
	addressed := HashSet[string]{}
	hasInternal := false

	for _, entry := range entries {
		if entry.IsDir() || !strings.HasSuffix(strings.ToLower(entry.Name()), "_test.go") {
			continue
		}

		selected, constraintErr := buildContext.MatchFile(packageDir, entry.Name())
		if constraintErr != nil || !selected {
			continue
		}

		path := filepath.Join(packageDir, entry.Name())
		file, parseErr := parser.ParseFile(token.NewFileSet(), path, nil, parser.SkipObjectResolution)
		if parseErr != nil || file.Name == nil || file.Name.Name != packageName {
			continue
		}

		hasInternal = true

		for _, decl := range file.Decls {
			if funcDecl, ok := decl.(*ast.FuncDecl); ok {
				names.Add(funcDecl.Name.Name)
			}
		}

		collectSiblingAddressedNames(file, addressed)
	}

	signals := siblingTestSignals{
		funcMethodNames:      names.Keys(),
		addressedGlobalNames: addressed.Keys(),
		hasInternalTests:     hasInternal,
	}

	sort.Strings(signals.funcMethodNames)
	sort.Strings(signals.addressedGlobalNames)
	return signals
}

// collectSiblingAddressedNames records every identifier an in-package test file takes the address of
// (`&g`, `&g.field`, `&g[i]`) that the file does not itself bind. Deliberately name-based and
// type-check-free: the production pass resolves each candidate against the real package scope, so a
// name that is not a package-level var there is simply dropped. Names bound anywhere inside the
// enclosing top-level declaration — receiver, parameters, results, `:=`, `var`/`const`/`type`, range
// and type-switch bindings, at any nesting depth — are excluded, which errs toward recording nothing
// rather than boxing a global a test never addressed.
func collectSiblingAddressedNames(file *ast.File, into HashSet[string]) {
	declared := HashSet[string]{}

	for _, decl := range file.Decls {
		switch typed := decl.(type) {
		case *ast.FuncDecl:
			declared.Add(typed.Name.Name)
		case *ast.GenDecl:
			for _, spec := range typed.Specs {
				switch specTyped := spec.(type) {
				case *ast.ValueSpec:
					for _, ident := range specTyped.Names {
						declared.Add(ident.Name)
					}
				case *ast.TypeSpec:
					declared.Add(specTyped.Name.Name)
				}
			}
		}
	}

	for _, decl := range file.Decls {
		// A package-level `var LstatP = &lstat` (path/filepath's export_test.go) binds nothing, so
		// the file's own declarators are the whole exclusion set for it.
		local := siblingBoundNames(decl)
		local.UnionWithSet(declared)

		ast.Inspect(decl, func(node ast.Node) bool {
			unary, ok := node.(*ast.UnaryExpr)

			if !ok || unary.Op != token.AND {
				return true
			}

			// Peel field selectors and index expressions down to the root operand, exactly as
			// collectAddressedGlobals does: &G.X and &G[i] both make G escape.
			root := unary.X

			for {
				switch expr := root.(type) {
				case *ast.SelectorExpr:
					root = expr.X
					continue
				case *ast.IndexExpr:
					root = expr.X
					continue
				case *ast.ParenExpr:
					root = expr.X
					continue
				}

				break
			}

			if ident, isIdent := root.(*ast.Ident); isIdent && !local.Contains(ident.Name) {
				into.Add(ident.Name)
			}

			return true
		})
	}
}

// siblingBoundNames returns every identifier bound by a declaration or statement anywhere inside the
// given node — the conservative "this is a local, not a package-level var" filter of
// collectSiblingAddressedNames.
func siblingBoundNames(node ast.Node) HashSet[string] {
	bound := HashSet[string]{}

	addField := func(fields *ast.FieldList) {
		if fields == nil {
			return
		}

		for _, field := range fields.List {
			for _, ident := range field.Names {
				bound.Add(ident.Name)
			}
		}
	}

	ast.Inspect(node, func(inner ast.Node) bool {
		switch typed := inner.(type) {
		case *ast.FuncDecl:
			addField(typed.Recv)
		case *ast.FuncType:
			addField(typed.Params)
			addField(typed.Results)
		case *ast.ValueSpec:
			for _, ident := range typed.Names {
				bound.Add(ident.Name)
			}
		case *ast.TypeSpec:
			bound.Add(typed.Name.Name)
		case *ast.AssignStmt:
			if typed.Tok != token.DEFINE {
				return true
			}

			for _, lhs := range typed.Lhs {
				if ident, ok := lhs.(*ast.Ident); ok {
					bound.Add(ident.Name)
				}
			}
		case *ast.RangeStmt:
			if typed.Tok != token.DEFINE {
				return true
			}

			for _, expr := range []ast.Expr{typed.Key, typed.Value} {
				if ident, ok := expr.(*ast.Ident); ok {
					bound.Add(ident.Name)
				}
			}
		}

		return true
	})

	return bound
}

// testAliasShadowName reports the first imported-package alias in this statement whose
// qualification is required only by a same-package test declaration. Nested statements are
// excluded: each is visited independently and receives its own adjacent comment.
func (v *Visitor) testAliasShadowName(stmt ast.Stmt) string {
	if len(packageTestAliasShadows) == 0 {
		return ""
	}

	names := HashSet[string]{}

	ast.Inspect(stmt, func(node ast.Node) bool {
		if node == nil {
			return false
		}

		if node != stmt {
			if _, nestedStatement := node.(ast.Stmt); nestedStatement {
				return false
			}
		}

		ident, ok := node.(*ast.Ident)
		if !ok || !packageTestAliasShadows[ident.Name] {
			return true
		}

		if _, isPackageName := v.info.ObjectOf(ident).(*types.PkgName); isPackageName {
			names.Add(ident.Name)
		}

		return true
	})

	if len(names) == 0 {
		return ""
	}

	ordered := names.Keys()
	sort.Strings(ordered)
	return ordered[0]
}

// writeTestAliasShadowComment explains a qualification that otherwise looks unnecessary when the
// generated production source is reviewed without its same-package tests in view. Inline
// for-init/post clauses have no standalone comment slot; their enclosing for statement carries the
// relevant header expression and receives the comment instead.
func (v *Visitor) writeTestAliasShadowComment(stmt ast.Stmt, contexts []StmtContext) {
	for _, context := range contexts {
		if format, ok := context.(FormattingContext); ok && !format.useNewLine {
			return
		}
	}

	alias := v.testAliasShadowName(stmt)
	if alias == "" {
		return
	}

	v.outputBuilder.WriteString(v.newline)
	v.outputBuilder.WriteString(v.indent(v.indentLevel))
	v.outputBuilder.WriteString(fmt.Sprintf(
		"// Fully qualified to avoid alias shadowing by the same-package test declaration %q.",
		alias))
}

// collectWhiteboxInternalTestObjects captures every go/types object declared by an internal test
// file. The internal and external variants share one go/packages load, so the external half can
// recognize an export-test declaration by object identity even when imported export data has no Pos.
func collectWhiteboxInternalTestObjects(pkg *packages.Package) map[types.Object]bool {
	objects := map[types.Object]bool{}
	if pkg == nil || pkg.TypesInfo == nil || pkg.Fset == nil {
		return objects
	}
	for _, obj := range pkg.TypesInfo.Defs {
		if obj == nil {
			continue
		}
		fileName := pkg.Fset.Position(obj.Pos()).Filename
		if strings.HasSuffix(strings.ToLower(fileName), "_test.go") {
			objects[obj] = true
		}
	}
	return objects
}

// collectWhiteboxBridgeTypeNames captures the EMITTED simple type names an internal `_test.go`
// contributes to the bridge class — the declared-name set the white-box record split resolves the
// bridge variant's bare record spellings against (see whiteboxBridgeTypeNames). Function-local
// types are absent by construction: they reach the bridge under a LIFTED package-level name the
// go/types defs cannot know, and are unioned in from the live claim set as the bridge converts.
func collectWhiteboxBridgeTypeNames(pkg *packages.Package) HashSet[string] {
	names := HashSet[string]{}

	if pkg == nil || pkg.TypesInfo == nil || pkg.Fset == nil {
		return names
	}

	for _, obj := range pkg.TypesInfo.Defs {
		typeName, ok := obj.(*types.TypeName)

		if !ok || typeName == nil {
			continue
		}

		fileName := pkg.Fset.Position(typeName.Pos()).Filename

		if strings.HasSuffix(strings.ToLower(fileName), "_test.go") {
			names.Add(getSanitizedIdentifier(typeName.Name()))
		}
	}

	return names
}

// collectWhiteboxBridgeDeclaredNames captures the GO names the bridge class declares: every
// package-level object an internal `_test.go` contributes, plus its METHODS — those emit as static
// extension members of the same class and hide a same-named production member just as a function
// does. See whiteboxBridgeDeclaredNames.
func collectWhiteboxBridgeDeclaredNames(pkg *packages.Package) HashSet[string] {
	names := HashSet[string]{}

	if pkg == nil || pkg.TypesInfo == nil || pkg.Fset == nil {
		return names
	}

	for ident, obj := range pkg.TypesInfo.Defs {
		if obj == nil || ident == nil || obj.Name() == "" || obj.Name() == "_" {
			continue
		}

		fileName := pkg.Fset.Position(obj.Pos()).Filename

		if !strings.HasSuffix(strings.ToLower(fileName), "_test.go") {
			continue
		}

		switch typed := obj.(type) {
		case *types.Func:
			// A package-level func, or a method (whose Parent is nil) — both land in the class.
			if signature, ok := typed.Type().(*types.Signature); ok && signature.Recv() != nil {
				names.Add(obj.Name())
				continue
			}

			if obj.Parent() == pkg.Types.Scope() {
				names.Add(obj.Name())
			}
		case *types.Var, *types.Const, *types.TypeName:
			if obj.Parent() == pkg.Types.Scope() {
				names.Add(obj.Name())
			}
		}
	}

	return names
}

// whiteboxProductionNameShadowed reports a BARE reference, from inside the bridge, to a
// package-level production member the bridge itself declares a same-named member for.
func (v *Visitor) whiteboxProductionNameShadowed(obj types.Object) bool {
	if !v.whiteboxProductionObject(obj) || !whiteboxBridgeDeclaredNames.Contains(obj.Name()) {
		return false
	}

	if v.pkg == nil {
		return false
	}

	switch obj.(type) {
	case *types.Func, *types.Var, *types.Const:
		return obj.Parent() == v.pkg.Scope()
	}

	return false
}

// packageScopeClassName names the static class that DECLARES a package-level object of the package
// currently being emitted — the qualifier to use when a bare reference would bind to something
// else (a same-named C# local, which is function-scoped).
//
// Normally that is `<pkg>_package`. Under the white-box test model the emission unit is the bridge
// class instead, and a declaration contributed by an internal `_test.go` lives THERE, not in the
// production class: crypto/md5's `buf := buf` reads a package-level `buf` declared in md5_test.go,
// and qualifying it as `md5_package.buf` names nothing (CS0117). Production members referenced
// from the same bridge keep the production class, so both halves stay addressable.
func (v *Visitor) packageScopeClassName(obj types.Object) string {
	if v.options.testClassNameOverride != "" && v.declaredInTestFile(obj) {
		return v.options.testClassNameOverride
	}

	return getSanitizedImport(packageName + PackageSuffix)
}

// declaredInTestFile reports whether an object's declaration site is a `_test.go` file — the
// signal that it emits into the test variant's class rather than the production one.
func (v *Visitor) declaredInTestFile(obj types.Object) bool {
	if obj == nil {
		return false
	}

	if whiteboxInternalTestObjects[obj] {
		return true
	}

	if v.fset == nil {
		return false
	}

	return strings.HasSuffix(strings.ToLower(v.fset.Position(obj.Pos()).Filename), "_test.go")
}

// whiteboxBridgeObject reports an internal-test object referenced from the external variant.
func (v *Visitor) whiteboxBridgeObject(obj types.Object) bool {
	if !v.options.testWhiteboxReference || !v.options.testExternalVariant || obj == nil ||
		obj.Pkg() == nil || obj.Pkg().Path() != v.options.testProductionPath {
		return false
	}
	if whiteboxInternalTestObjects[obj] {
		return true
	}
	fileName := v.fset.Position(obj.Pos()).Filename
	return strings.HasSuffix(strings.ToLower(fileName), "_test.go")
}

// whiteboxBridgeNamedType qualifies an internal-test type referenced by the external variant.
// Type arguments render through the same constraint-proxy substitution the production twin
// applies, so a generic bridge type instantiated over a proxied constraint spells both halves
// consistently.
func (v *Visitor) whiteboxBridgeNamedType(named *types.Named) (string, bool) {
	if named == nil || !v.whiteboxBridgeObject(named.Obj()) {
		return "", false
	}
	// The reference renders during the EXTERNAL variant's own conversion, whose nameCollisions is
	// a fresh, unrelated map (resetPackageState) computed over a file set that never includes the
	// internal declaration being referenced here — so it cannot answer "did THIS declaration
	// collide". testTypeRenames is session-scoped and object-keyed for exactly this: it was
	// populated by the INTERNAL variant's own pass, against the same declaration object, when the
	// declaration itself was Δ-renamed (visitTypeSpec). See testTypeRenames's doc comment.
	obj := named.Obj()
	name := obj.Name()
	if testTypeRenames[obj] {
		name = getCollisionAvoidanceIdentifier(name)
	} else {
		name = getCoreSanitizedIdentifier(name)
	}
	if typeArgs := named.TypeArgs(); typeArgs != nil && typeArgs.Len() > 0 {
		args := make([]string, typeArgs.Len())
		for i := 0; i < typeArgs.Len(); i++ {
			if proxyName, ok := v.constraintProxyArg(named, i); ok {
				args[i] = proxyName
			} else {
				args[i] = v.getAliasQualifiedTypeName(typeArgs.At(i), false)
			}
		}
		name += "[" + strings.Join(args, ", ") + "]"
	}
	return "global::" + packageNamespace + "." + v.options.testInternalBridgeName + "." + name, true
}

// whiteboxProductionObject reports a production declaration while converting the internal
// white-box bridge. go/packages presents production and internal-test declarations as one Go
// package, but declaration position restores their different C# owners.
func (v *Visitor) whiteboxProductionObject(obj types.Object) bool {
	if !v.options.testWhiteboxReference || !v.options.testInlineTypeAccess || obj == nil ||
		obj.Pkg() == nil || obj.Pkg().Path() != v.options.testProductionPath {
		return false
	}

	fileName := v.fset.Position(obj.Pos()).Filename
	return fileName != "" && !strings.HasSuffix(strings.ToLower(fileName), "_test.go")
}

// whiteboxProductionNamedType qualifies a production-declared type through its referenced class
// before ordinary same-Go-package rendering can erase that owner.
func (v *Visitor) whiteboxProductionNamedType(named *types.Named) (string, bool) {
	if named == nil || !v.whiteboxProductionObject(named.Obj()) {
		return "", false
	}

	obj := named.Obj()

	name := getSanitizedIdentifier(obj.Name())
	if typeArgs := named.TypeArgs(); typeArgs != nil && typeArgs.Len() > 0 {
		args := make([]string, typeArgs.Len())
		for i := 0; i < typeArgs.Len(); i++ {
			if proxyName, ok := v.constraintProxyArg(named, i); ok {
				args[i] = proxyName
			} else {
				args[i] = v.getAliasQualifiedTypeName(typeArgs.At(i), false)
			}
		}
		name += "[" + strings.Join(args, ", ") + "]"
	}

	return "global::" + packageNamespace + "." + getSanitizedImport(v.options.testProductionName+PackageSuffix) + "." + name, true
}

// whiteboxBridgeUse reports an external test reference to an object contributed by the
// package-under-test's internal _test.go files. Object identity and declaration position keep
// production members and same-spelled unrelated declarations on their existing paths. A struct
// FIELD is never bridge-qualified: its ident renders inside member-access and named-argument
// positions (`new T(Field: v)`), where a class-qualified spelling is not legal C#.
func (v *Visitor) whiteboxBridgeUse(ident *ast.Ident) bool {
	if !v.options.testWhiteboxReference || !v.options.testExternalVariant || v.options.testInternalBridgeName == "" || v.info.Defs[ident] != nil {
		return false
	}

	obj := v.info.ObjectOf(ident)

	if varObj, ok := obj.(*types.Var); ok && varObj.IsField() {
		return false
	}

	return v.whiteboxBridgeObject(obj)
}

func (v *Visitor) whiteboxBridgeMember(ident *ast.Ident) string {
	name := getSanitizedIdentifier(v.getIdentName(ident))
	switch obj := v.info.ObjectOf(ident).(type) {
	case *types.Func:
		name = getSanitizedFunctionName(v.getIdentName(ident))
		if testMethodRenames[v.info.ObjectOf(ident)] {
			name = ShadowVarMarker + name
		}
	case *types.TypeName:
		// A TRUE ALIAS a `_test.go` file declares (`type G = g`, export_test.go) is emitted as its
		// own `global using` — a COMPILATION-scoped construct, member of no class — so bridge-
		// qualifying it here is CS0426, the type-name twin of testDeclaredAliasSpelledBare's same
		// rule for the types.Type-based resolution path (typeNameResolution.go). Render it bare;
		// the caller's dot-selector context is exactly where a `global using` name already
		// resolves, matching the internal variant's own spelling of the same alias.
		if obj.IsAlias() {
			return getSanitizedIdentifier(v.getIdentName(ident))
		}

		name = convertToCSTypeName(v.getIdentName(ident))
	case *types.Var, *types.Const:
		// The var/const twin of the *types.TypeName case above: `getSanitizedIdentifier` at line
		// 555 already ran, but against the EXTERNAL variant's own nameCollisions — wrong for the
		// identical reason whiteboxBridgeNamedType's doc comment explains for types
		// (export_test.go's `var Lock = lock` collides with RWMutex's own `Lock` method and
		// Δ-renames to `ΔLock`; the external variant's own collision map never saw it). Re-derive
		// from the session-scoped, object-keyed record instead.
		if testTypeRenames[v.info.ObjectOf(ident)] {
			name = getCollisionAvoidanceIdentifier(v.getIdentName(ident))
		}
	}
	return v.options.testInternalBridgeName + "." + name
}

// testDeclaredAliasSpelledBare reports the BARE C# name of a type alias that the package-under-test's
// own `_test.go` files declare, when the EXTERNAL test variant is what is naming it.
//
// The internal half emits `global using AddrDetail = …` for such an alias, so the internal variant
// needs nothing. But the assembly has more than one variant class, and the external one reaches the
// alias by PACKAGE QUALIFICATION — Go says `netip.AddrDetail`, because `export_test.go` is part of
// package netip during a test build. A `global using` is a member of no class, so that spelling is
// CS0426: "the type name AddrDetail does not exist in the type netip_package", net/netip's last
// one-line wall.
//
// The alias IS in scope where this emission lands — one compilation — so the fix is to stop
// qualifying it, which also keeps one Go name spelled one way across both halves. Rendering the
// alias's TARGET instead would resolve too, but it would spell one alias two different ways
// depending on which half named it.
//
// The rule is MODEL-INDEPENDENT, and the first version of it was not: it required the white-box
// reference model, on the reasoning that only there does the production half live in another
// assembly. That is true and beside the point — what makes the qualified spelling invalid is that a
// `global using` is a member of no class, which holds just as firmly when production is RECOMPILED
// into the test assembly. netip measured it: taking the recompile model to satisfy a nominal
// constraint left this same CS0426 as the package's only remaining error. Under recompile the alias
// is, if anything, more plainly in scope — production, internal and external are one compilation.
//
// The remaining clauses are each load-bearing: only the EXTERNAL variant composes the qualified
// spelling; only an alias declared by the package-under-test's own test files has its `global using`
// in THIS compilation (a production-declared one is the sibling arm's business, and a FOREIGN
// package's alias is a real member of a real referenced assembly). The plain black-box REFERENCE
// model cannot reach this arm at all — it has no internal variant, so no `_test.go` of the package
// under test declares anything — which is why no model test is needed beyond being a test
// conversion.
func (v *Visitor) testDeclaredAliasSpelledBare(t types.Type) (string, bool) {
	packagePath := v.options.packageUnderTestPath()

	if !v.options.testExternalVariant || packagePath == "" {
		return "", false
	}

	alias, isAlias := t.(*types.Alias)

	if !isAlias {
		return "", false
	}

	aliasObj := alias.Obj()

	if aliasObj == nil || aliasObj.Pkg() == nil || aliasObj.Pkg().Path() != packagePath {
		return "", false
	}

	if !v.declaredInTestFile(aliasObj) {
		return "", false
	}

	return getSanitizedIdentifier(aliasObj.Name()), true
}
