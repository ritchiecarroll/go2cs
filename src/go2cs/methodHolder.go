// methodHolder.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A Go method is emitted into a class of its own receiver type, beside the package class
// (docs/phase4/SCOUT-method-holder.md, ruled 2026-11-11):
//
//	partial class time_package {
//
//	partial struct Month /*num:nint*/;
//
//	} partial class timeᴛMonth {
//
//	public static @string String(this Month m) { … }
//
//	} partial class time_package {
//
// A method and a package-level type, constant or variable of one name are then members of two classes,
// so neither is renamed. The class is declared once, in package_info.cs, with the attribute that names
// its receiver type; a reader finds a type's methods by that attribute and never by the class's name.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"sort"
	"strings"
)

// MethodHolderSection names the package_info.cs marker section that declares the method-holder classes.
const MethodHolderSection = "MethodHolders"

// methodHolderAttributes is what every method-holder declaration carries after its receiver attribute:
// the class is plumbing a consumer never names, so completion lists leave it out. Its methods are the
// type's Go methods and carry no such mark.
const methodHolderAttributes = "global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)"

// packageMethodHolders maps each method-holder class this emission unit wrote a block of to the C#
// spelling of its receiver type, as package_info.cs names it in `typeof(…)`. Reset per package and
// variant by resetPackageState; written under packageLock.
var packageMethodHolders map[string]string

// receiverNamed answers the named type a method declaration is declared on: the receiver's type with a
// pointer and an alias read through, or nil for a function.
func receiverNamed(info *types.Info, funcDecl *ast.FuncDecl) *types.Named {
	if funcDecl == nil || funcDecl.Recv == nil || funcDecl.Name == nil {
		return nil
	}

	fn, ok := info.Defs[funcDecl.Name].(*types.Func)

	if !ok || fn == nil {
		return nil
	}

	sig, ok := fn.Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return nil
	}

	recvType := types.Unalias(sig.Recv().Type())

	if ptr, isPtr := recvType.(*types.Pointer); isPtr {
		recvType = types.Unalias(ptr.Elem())
	}

	named, _ := recvType.(*types.Named)

	return named
}

// holderTypeName is a receiver type's own C# identifier, as the type's declaration spells it: without a
// qualifier and without type arguments. escaped keeps a keyword's `@`, which a class name built from the
// identifier drops.
func (v *Visitor) holderTypeName(named *types.Named) (escaped string) {
	typeName := convertToCSTypeName(v.getCSharpTypeName(named.Origin()))

	if open := strings.Index(typeName, "<"); open != -1 {
		typeName = typeName[:open]
	}

	return typeName[strings.LastIndex(typeName, ".")+1:]
}

// methodHolderClass names the class a method of named's is emitted into by the file being converted: the
// class the file emits into, its `_package` ending replaced by the marker and the receiver type's name
// (`timeᴛMonth`). typeofTarget is the receiver type as package_info.cs spells it. It answers "" for a type
// of another package, which declares no method here.
func (v *Visitor) methodHolderClass(named *types.Named, method types.Object) (holder string, typeofTarget string) {
	if named == nil || named.Obj() == nil || named.Obj().Pkg() != v.pkg || v.fileClassName == "" {
		return "", ""
	}

	escaped := v.holderTypeName(named)
	bare := strings.TrimPrefix(escaped, "@")

	if bare == "" {
		return "", ""
	}

	// The receiver type's own class: the file's, or under -tests the production or bridge class a test
	// file's method extends a type of.
	declaringClass := v.packageScopeClassName(named.Obj())

	switch {
	case v.whiteboxProductionObject(named.Obj()):
		declaringClass = "global::" + packageNamespace + "." + getSanitizedImport(v.options.testProductionName+PackageSuffix)
	case v.whiteboxBridgeObject(named.Obj()):
		declaringClass = "global::" + packageNamespace + "." + v.options.testInternalBridgeName
	}

	typeofTarget = declaringClass + "." + escaped

	if typeParams := named.Origin().TypeParams(); typeParams != nil && typeParams.Len() > 0 {
		typeofTarget += "<" + strings.Repeat(",", typeParams.Len()-1) + ">"
	}

	holder = strings.TrimSuffix(v.fileClassName, PackageSuffix) + TempVarMarker + bare

	if declaringClass == v.fileClassName {
		holder += v.testHolderSuffix(method, named.Obj())
	}

	return holder, typeofTarget
}

// testHolderMark names the second holder of a type: the one that holds the methods a `_test.go` file
// declares on a type of a production file, where both files emit into one package class.
const testHolderMark = TempVarMarker + "test"

// testHolderSuffix returns the mark a method's holder takes when a `_test.go` file declares the method on a
// type a production file declares, for the case where both emit into one package class (the recompile model
// of -tests, and a plain conversion that includes test files). The production holder then never gains a
// member from a test file: a production method's body reads its package's names against the methods of its
// own holder (holderHides), and those are the same whether or not the tests are compiled beside it. Where the
// test file emits into its own class (the white-box bridge) the holder's name already differs.
func (v *Visitor) testHolderSuffix(method types.Object, typeObj types.Object) string {
	if method != nil && v.declaredInTestFile(method) && !v.declaredInTestFile(typeObj) {
		return testHolderMark
	}

	return ""
}

// funcDeclClass names the class funcDecl is emitted into: its receiver type's method holder, or the
// file's own class for a function.
func (v *Visitor) funcDeclClass(funcDecl *ast.FuncDecl) string {
	if holder, typeofTarget := v.methodHolderClass(receiverNamed(v.info, funcDecl), v.info.Defs[funcDecl.Name]); holder != "" {
		packageLock.Lock()
		packageMethodHolders[holder] = typeofTarget
		packageLock.Unlock()

		v.fileHasMethodHolder = true

		return holder
	}

	return v.fileClassName
}

// switchClass answers the line that closes the class block open in the file and opens name's, followed by
// a blank line, and records name as the open class. It answers "" when name's block is the open one, and
// for a declaration of the C pseudo-package, which has a class block of its own.
func (v *Visitor) switchClass(name string) string {
	if name == "" || name == v.openClassName || v.fileClassName == "" || cgoEmittingPseudoPackage {
		return ""
	}

	v.openClassName = name

	return fmt.Sprintf("} partial class %s {%s%s", name, v.newline, v.newline)
}

// placeFuncDecl answers what stands between the blank line a function's emission opens with and its doc
// comment: the declarations lifted out of it, and the class switches it needs. A type lifted out of a
// method stays in the file's own class, where go2cs-gen finds a Go type; what is hoisted for the
// method's body alone sits beside it in its holder.
func (v *Visitor) placeFuncDecl(funcDecl *ast.FuncDecl, lifted string, hoisted string) string {
	class := v.funcDeclClass(funcDecl)
	result := strings.Builder{}

	if class != v.fileClassName && lifted != "" {
		result.WriteString(v.switchClass(v.fileClassName))
		result.WriteString(lifted)
		lifted = ""
	}

	result.WriteString(v.switchClass(class))
	result.WriteString(lifted)
	result.WriteString(hoisted)

	return result.String()
}

// hoistPrefix is the builder a declaration hoisted for the current function's body alone is written to:
// beside a method in its holder, and in the one prefix of a function.
func (v *Visitor) hoistPrefix() *strings.Builder {
	if v.currentFuncHoists != nil {
		return v.currentFuncHoists
	}

	return v.currentFuncPrefix
}

// methodHolderLine renders one holder's declaration for package_info.cs.
func methodHolderLine(holder string, typeofTarget string) string {
	return fmt.Sprintf("[GoMethodHost(typeof(%s)), %s] public static partial class %s;", typeofTarget, methodHolderAttributes, holder)
}

// methodHolderLineClass extracts the class a rendered holder declaration declares, the declaration's
// identity in a merging write, and "" for a line methodHolderLine did not render.
func methodHolderLineClass(line string) string {
	const lead, mark = "[GoMethodHost(typeof(", "] public static partial class "

	if !strings.HasPrefix(line, lead) || !strings.HasSuffix(line, ";") {
		return ""
	}

	at := strings.LastIndex(line, mark)

	if at < 0 {
		return ""
	}

	return strings.TrimSuffix(line[at+len(mark):], ";")
}

// methodHolderProseLines is the section's explanatory comment, which persists in every package_info.cs
// that declares a holder.
func methodHolderProseLines() []string {
	return []string{
		"// A Go method is an extension method on its receiver type. The methods of one type are",
		"// in a class of their own beside the package class, so that a method and a package-level",
		"// type, constant or variable can share a name as they do in Go. Each class is declared",
		"// below with the type it holds the methods of; its blocks are in the converted sources,",
		"// in Go's declaration order.",
	}
}

// applyMethodHolderSection writes the holder declarations of this emission unit into package_info.cs,
// after everything else in the file, with the merge semantics of the other sections: a whole-package
// conversion rebuilds the section from this run's holders, and a merging write keeps the declarations
// this pass did not write again. A package that declares no method has no section.
func applyMethodHolderSection(packageInfoLines []string, mergeExisting bool) []string {
	startLineIndex, endLineIndex := -1, -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<"+MethodHolderSection+">") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</"+MethodHolderSection+">") {
			endLineIndex = i
			break
		}
	}

	hasSection := startLineIndex >= 0 && endLineIndex > startLineIndex
	entries := map[string]string{}

	if mergeExisting && hasSection {
		for i := startLineIndex + 1; i < endLineIndex; i++ {
			line := strings.TrimSpace(packageInfoLines[i])

			if class := methodHolderLineClass(line); class != "" {
				entries[class] = line
			}
		}
	}

	for holder, typeofTarget := range packageMethodHolders {
		entries[holder] = methodHolderLine(holder, typeofTarget)
	}

	if len(entries) == 0 {
		if !hasSection {
			return packageInfoLines
		}

		return append(packageInfoLines[:startLineIndex+1], packageInfoLines[endLineIndex:]...)
	}

	classes := make([]string, 0, len(entries))

	for class := range entries {
		classes = append(classes, class)
	}

	sort.Strings(classes)

	merged := make([]string, 0, len(classes))

	for _, class := range classes {
		merged = append(merged, entries[class])
	}

	if hasSection {
		return append(packageInfoLines[:startLineIndex+1], append(merged, packageInfoLines[endLineIndex:]...)...)
	}

	for len(packageInfoLines) > 0 && strings.TrimSpace(packageInfoLines[len(packageInfoLines)-1]) == "" {
		packageInfoLines = packageInfoLines[:len(packageInfoLines)-1]
	}

	packageInfoLines = append(packageInfoLines, "")
	packageInfoLines = append(packageInfoLines, methodHolderProseLines()...)
	packageInfoLines = append(packageInfoLines, "", "// <"+MethodHolderSection+">")
	packageInfoLines = append(packageInfoLines, merged...)

	return append(packageInfoLines, "// </"+MethodHolderSection+">")
}

// packageSelfAliasBlocked reports that no file of the package being converted may name the package's own
// class through the self-alias (`using time = time_package;`), decided once per package by
// computeSelfAlias from what Go's scopes and the package graph state, never from a compile error.
var packageSelfAliasBlocked bool

// computeSelfAlias decides, for the package being converted, whether a method holder may qualify the
// package's own names through the self-alias, and gives that alias the rename every importer's alias of
// the package takes where a child namespace of the same name is in view (computeImportAliasRenames,
// which this follows).
//
// The alias names the package class wherever nothing else answers to the package's name:
//   - another package of the same name in the import closure would be written through the same alias
//     by a type reference the file never imports it for;
//   - a type, variable or constant of the package named like the package arrives through the same
//     `using static`, and a type of that name is the alias's own name in every holder (CS0721);
//   - in the root namespace, one of golib's types or namespaces of that name is what the name means.
//
// A function named like its package (`main`) does not block it: C# reads `main.X` through the alias.
func computeSelfAlias(pkg *types.Package, packageNS string) {
	packageSelfAliasBlocked = false

	if pkg == nil {
		return
	}

	name := pkg.Name()

	if packageChildNamespaces[packageNS+"."+getSanitizedImport(name)] {
		packageImportAliasRenames[name] = ShadowVarMarker + name
	}

	switch pkg.Scope().Lookup(name).(type) {
	case *types.TypeName, *types.Var, *types.Const:
		packageSelfAliasBlocked = true
	}

	if packageNS == RootNamespace && golibNamespaceName(name) {
		packageSelfAliasBlocked = true
	}

	visited := map[*types.Package]bool{pkg: true}

	var walk func(p *types.Package)

	walk = func(p *types.Package) {
		for _, imp := range p.Imports() {
			if visited[imp] {
				continue
			}

			visited[imp] = true

			if imp.Name() == name {
				packageSelfAliasBlocked = true
			}

			walk(imp)
		}
	}

	walk(pkg)
}

// holderDeclares reports whether the holder the current method is emitted into declares a method of
// the given Go name: a method of the same receiver type that the same class of this emission unit holds.
func (v *Visitor) holderDeclares(name string) bool {
	named := v.currentHolderNamed

	if named == nil {
		return false
	}

	for i := 0; i < named.NumMethods(); i++ {
		method := named.Method(i)

		if method.Name() == name && v.declaredInTestFile(method) == v.currentHolderInTestFile {
			return true
		}
	}

	return false
}

// holderDeclaresTypeName is holderDeclares for a type's C# spelling: the name without a keyword's `@`
// and without type arguments.
func (v *Visitor) holderDeclaresTypeName(typeName string) bool {
	if open := strings.Index(typeName, "<"); open != -1 {
		typeName = typeName[:open]
	}

	return v.holderDeclares(strings.TrimPrefix(typeName, "@"))
}

// holderHides reports whether a package-level name of the package being converted, written bare inside
// the current method's holder, binds something other than the package's member. The holder reads the
// package's names through `using static`, behind:
//   - the holder's own methods;
//   - in the root namespace, golib's types of that name with no type parameter;
//   - a child namespace, in view, of the package's namespace.
//
// The members of `go.builtin` are not in that list: they arrive through a global using, and the file's
// own `using static` stands inside its namespace, where C# reads it first.
func (v *Visitor) holderHides(name string) bool {
	if v.currentHolderNamed == nil {
		return false
	}

	if v.holderDeclares(name) {
		return true
	}

	if packageNamespace == RootNamespace && golibNamespaceType(name, v.pkg.Path()) {
		return true
	}

	return packageChildNamespaces[packageNamespace+"."+getSanitizedImport(name)] ||
		(packageNamespace == RootNamespace && golibChildNamespaces[name])
}

// golibNamespaceType reports whether golib declares, in namespace `go`, a type named name that takes no
// type parameter and that the package at importPath can name: a public one, or an internal one where
// golib names the package's assembly in InternalsVisibleTo. A package directly below `go` is the assembly
// of its own import path, and the lists are golib's own (golibNamespaceNames.go, generated).
func golibNamespaceType(name, importPath string) bool {
	return golibNamespaceTypes[name] || (golibFriendNamespaceTypes[name] && golibFriendAssemblies[importPath])
}

// golibNamespaceName reports whether golib declares anything named name in namespace `go`: a type, with
// or without type parameters and whatever its visibility, or a child namespace.
func golibNamespaceName(name string) bool {
	return golibNamespaceTypes[name] || golibFriendNamespaceTypes[name] || golibGenericNamespaceTypes[name] ||
		golibChildNamespaces[name]
}

// nameSet answers the set of names.
func nameSet(names ...string) map[string]bool {
	set := make(map[string]bool, len(names))

	for _, name := range names {
		set[name] = true
	}

	return set
}

// selfQualifier answers the qualifier that names the class declaring obj, a package-level object of the
// package being converted, from inside a method holder: the file's self-alias (`time.X`), or the class
// itself (`time_package.X`) where the alias's name means something else at the use.
func (v *Visitor) selfQualifier(obj types.Object) string {
	class := v.packageScopeClassName(obj)

	if class != v.fileClassName || v.options.testClassNameOverride != "" || packageSelfAliasBlocked || v.fileSelfAliasBlocked() {
		return class
	}

	// A variable of the function named like the package is what the name means for the whole of its
	// C# block, and a method of the holder is a member of the class the body stands in.
	if v.funcScopeVarNames.Contains(packageName) || v.holderDeclares(packageName) {
		return class
	}

	v.fileUsesSelfAlias = true

	return getSanitizedImport(importQualifier(packageName))
}

// fileSelfAliasBlocked reports whether the file being converted takes no self-alias at all:
//   - it imports another package under the package's own name, which is that name in the file;
//   - one of its methods selects, through a variable named like the package, a name the package also
//     declares (`url.User` on a `*URL` named url, beside the package's func User). With the alias in the
//     file that selector reads as the package's member to anything that reads the C#, the analyzer that
//     guards this emission first (GO2CS0013); without it, it has one reading.
func (v *Visitor) fileSelfAliasBlocked() bool {
	if v.fileSelfAliasDecided {
		return v.fileSelfAliasIsBlocked
	}

	v.fileSelfAliasDecided = true
	blocked := false

	if v.astFile != nil {
		for _, importSpec := range v.astFile.Imports {
			if pkgName, ok := v.info.ObjectOf(importSpecIdent(v.info, importSpec)).(*types.PkgName); ok && pkgName.Name() == packageName {
				blocked = true
			}
		}

		ast.Inspect(v.astFile, func(n ast.Node) bool {
			if blocked {
				return false
			}

			if funcDecl, isFunc := n.(*ast.FuncDecl); isFunc && funcDecl.Recv == nil {
				return false
			}

			selector, isSelector := n.(*ast.SelectorExpr)

			if !isSelector {
				return true
			}

			if ident, isIdent := selector.X.(*ast.Ident); isIdent && ident.Name == packageName {
				if variable, isVar := v.info.ObjectOf(ident).(*types.Var); isVar && variable.Parent() != v.pkg.Scope() &&
					v.pkg.Scope().Lookup(selector.Sel.Name) != nil {
					blocked = true
				}
			}

			return true
		})
	}

	v.fileSelfAliasIsBlocked = blocked

	return blocked
}

// importSpecIdent answers the identifier go/types records an import's package name under: the spec's
// explicit name, or the spec itself through Implicits, for which it answers nil and the caller reads
// the implicit object.
func importSpecIdent(info *types.Info, importSpec *ast.ImportSpec) *ast.Ident {
	if importSpec.Name != nil {
		return importSpec.Name
	}

	if pkgName, ok := info.Implicits[importSpec].(*types.PkgName); ok {
		return ast.NewIdent(pkgName.Name())
	}

	return nil
}

// holderQualifiedName answers the qualified spelling of ident inside the current method's holder, when
// ident names a package-level constant, variable or function of the package being converted that the
// bare name would not bind there, and "" otherwise.
func (v *Visitor) holderQualifiedName(ident *ast.Ident) string {
	if !v.inFunction || v.currentHolderNamed == nil || v.pkg == nil {
		return ""
	}

	obj := v.info.ObjectOf(ident)

	if obj == nil || obj.Pkg() == nil || obj.Parent() != obj.Pkg().Scope() {
		return ""
	}

	switch obj.(type) {
	case *types.Const, *types.Var, *types.Func:
	default:
		return ""
	}

	if !v.holderHides(obj.Name()) {
		return ""
	}

	if obj.Pkg() == v.pkg {
		return v.selfQualifier(obj) + "." + getSanitizedIdentifier(v.getIdentName(ident))
	}

	// A name a dot import brings (`. "time"`) is written bare in Go and reaches the file through
	// `using static <pkg>_package`, behind the holder's own members as the package's own names are. It
	// is written through its package's class. A name written `pkg.Name` in Go already is.
	if v.selectorSelIdents()[ident] {
		return ""
	}

	return v.qualifyPackageReference(convertImportPathToNamespace(obj.Pkg().Path(), PackageSuffix)) + "." +
		getSanitizedIdentifier(v.getIdentName(ident))
}

// selectorSelIdents returns the identifiers of the file that are the selected name of a selector
// expression (`x.Sel`), read once per file.
func (v *Visitor) selectorSelIdents() map[*ast.Ident]bool {
	if v.fileSelectorSels == nil {
		v.fileSelectorSels = map[*ast.Ident]bool{}

		if v.astFile != nil {
			ast.Inspect(v.astFile, func(node ast.Node) bool {
				if selector, ok := node.(*ast.SelectorExpr); ok {
					v.fileSelectorSels[selector.Sel] = true
				}

				return true
			})
		}
	}

	return v.fileSelectorSels
}

// methodHolderRef names the class that holds fn, a method a named type declares, for a static-form
// reference from the file being converted: the bare class for a method of the file's own emission unit,
// and the rooted class for any other. It answers "" for an interface's method, which has no static form.
func (v *Visitor) methodHolderRef(fn *types.Func) string {
	if fn == nil || fn.Pkg() == nil {
		return ""
	}

	sig, ok := fn.Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return ""
	}

	recvType := types.Unalias(sig.Recv().Type())

	if ptr, isPtr := recvType.(*types.Pointer); isPtr {
		recvType = types.Unalias(ptr.Elem())
	}

	named, ok := recvType.(*types.Named)

	if !ok || named.Obj() == nil || types.IsInterface(named) {
		return ""
	}

	bare := strings.TrimPrefix(v.holderTypeName(named), "@")

	if bare == "" {
		return ""
	}

	var packageClass string

	// Whether the method's file and the type's file emit into one package class: a test file's method on a
	// production type then sits in the type's second holder (testHolderSuffix).
	sharedClass := false

	switch {
	case v.whiteboxProductionObject(fn):
		packageClass = "global::" + packageNamespace + "." + getSanitizedImport(v.options.testProductionName+PackageSuffix)
	case v.whiteboxBridgeObject(fn):
		packageClass = "global::" + packageNamespace + "." + v.options.testInternalBridgeName
	case fn.Pkg() != v.pkg:
		packageClass = "global::" + packageClassNamespace(fn.Pkg()) + "." + getSanitizedImport(fn.Pkg().Name()+PackageSuffix)
		sharedClass = true
	default:
		packageClass = v.packageScopeClassName(fn)
		sharedClass = packageClass == v.packageScopeClassName(named.Obj())
	}

	holder := strings.TrimSuffix(packageClass, PackageSuffix) + TempVarMarker + bare

	if sharedClass {
		holder += v.testHolderSuffix(fn, named.Obj())
	}

	return holder
}

// hoistClassKey keys a hoisted literal's field by the class its user is emitted into: "" and a separator
// for a function, and the receiver type's name, marked for a `_test.go` method, for a method. A seeded
// literal has the one key, whatever class reads it.
func hoistClassKey(info *types.Info, funcDecl *ast.FuncDecl, seeded bool) string {
	const separator = "\x00"

	if seeded {
		return separator
	}

	named := receiverNamed(info, funcDecl)

	if named == nil {
		return separator
	}

	return named.Obj().Name() + separator
}
