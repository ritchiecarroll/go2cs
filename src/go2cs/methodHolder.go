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
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"sync"

	"github.com/ritchiecarroll/hashset"
)

// MethodHolderSection names the package_info.cs marker section that declares the method-holder classes.
const MethodHolderSection = "MethodHolders"

// methodHolderAttributes is what every method-holder declaration carries after its receiver attribute:
// the class is plumbing a consumer never names, so completion lists leave it out. Its methods are the
// type's Go methods and carry no such mark. This is the global:: spelling; applyPackageInfoSystemUsings
// writes the short one wherever the package declares no name its using would make ambiguous.
const methodHolderAttributes = "global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)"

// packageMethodHolders maps each method-holder class this emission unit wrote a block of to the C#
// spelling of its receiver type, as package_info.cs names it in `typeof(…)`. Reset per package and
// variant by resetPackageState; written under packageLock.
var packageMethodHolders map[string]string

// GlyphMethodSuffix ends the name of a Go method's BODY in its package class: U+2054 INVERTED UNDERTIE, connector
// punctuation, which a C# identifier admits and a Go identifier cannot spell (Go's letter is a Unicode L* code
// point or `_`). A method takes the suffix form exactly when it has a method holder (methodHolderClass): every
// method of a library package, and in package main only a method on the clash list (methodsMayStayInPackageClass). Its body is then a plain
// static `<Name>⁔(/*this*/ recv, …)` beside its type, and the Go-named extension method its holder declares is a
// one-line forwarder the converter writes into package_info.cs (MethodForwarderSection). Owner ruling 2026-11-21
// (187c6f88d0): the body reads where Go wrote it, the forwarder keeps every caller and every
// go2cs-gen reader of the holder's extension methods unchanged, and the runtime names a frame by stripping the
// suffix (runtime managed_impl.cs, golib GoSyntheticPC).
const GlyphMethodSuffix = "\u2054"

// MethodForwarderSection names the package_info.cs marker section holding the Go-named forwarders of the
// emission unit's suffix-form method bodies, one `partial class <holder> { … }` block per holder.
const MethodForwarderSection = "MethodForwarders"

// methodForwarderAttributes leads every forwarder: inlined into its callers, and hidden from every stack walk
// (Exception.StackTrace drops [StackTraceHidden]; runtime.Callers skips it through isGoSourceFrame) and from the
// debugger's step-into, so the body's frame is the Go method's one frame. Spelled as methodHolderAttributes is.
const methodForwarderAttributes = "global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough"

// methodForwarder is one recorded forwarder: its holder, its line, and a key ordering it as Go declares the
// method (file, then offset), so a whole-package write is deterministic whatever order the files convert in.
type methodForwarder struct {
	holder string
	key    string
	line   string

	// aliases maps each file-local alias of the body's file the line names (a variadic parameter's `ꓸꓸꓸT`) to its
	// referent, for package_info.cs to declare (forwarderAliasUsings).
	aliases map[string]string
}

// packageMethodForwarders holds the forwarders of this emission unit's suffix-form methods. The section writes
// those whose holder the unit declares (packageMethodHolders), so a -tests variant swap of the holder map
// carries its forwarders with it. Reset per package by resetPackageState; written under packageLock.
var packageMethodForwarders []methodForwarder

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

// methodsStayInPackageClass reports whether named's methods are emitted into its package class, as functions are,
// rather than into a holder: so for a type of package main, unless one of its methods is named like something a
// file of the package reads bare, which the method would hide from every file of the package:
//   - a package-level name of main (a type, function, constant or variable);
//   - a name of the universe (`len`, `string`, `nil`), whose built-ins arrive through `using static go.builtin`;
//   - a name a file imports (`sort`), or a name a dot-imported package declares, exported or not;
//   - a member of golib's builtin class that converted code reads bare (`str`, `zero`; golibBuiltinMembers).
//
// The names a test file of the package adds count as well (mainTestScopeNames), so that a program converted on
// its own and its -tests variants decide alike, and a type whose methods a test file's name would hide keeps its
// holder in both, as does a type a test file declares a method on (mainTestMethodTypes).
//
// The methods counted are those the type declares and those a struct gets from an embedded interface, which the
// converter writes as forwarders beside them (visitStructType.go). A library keeps every holder: its classes are
// a public surface, and a consumer reads a type's methods there. A program has no consumer, and its methods then
// read as the Go file does, beside its functions.
func methodsStayInPackageClass(named *types.Named) bool {
	if named == nil || named.Obj() == nil || named.Obj().Pkg() == nil || named.Obj().Pkg().Name() != "main" {
		return false
	}

	origin := named.Origin()

	if stays, known := packageClassMethodTypes.Load(origin); known {
		return stays.(bool)
	}

	stays := methodsMayStayInPackageClass(origin)
	packageClassMethodTypes.Store(origin, stays)

	return stays
}

// holderNamed answers named, the receiver type of the method being emitted, where its methods are emitted into a
// holder, and nil where they stay in the package class (methodsStayInPackageClass), which has no holder whose own
// methods hide a name.
func holderNamed(named *types.Named) *types.Named {
	if methodsStayInPackageClass(named) {
		return nil
	}

	return named
}

// packageClassMethodTypes caches methodsStayInPackageClass by a type's origin, which the converter asks of every
// method reference.
var packageClassMethodTypes sync.Map

// mainTestScopeNames holds what the build-selected in-package `_test.go` files of the package being converted add
// to its package scope or to a file scope of their own: their package-level names and their imports, read from
// source (collectSiblingTestSignals). A plain conversion loads no test file, and the -tests flow compiles the
// production files a plain conversion emitted, so both read the test files the same way; a -tests variant's own
// scope holds nothing beyond these and the production files'. mainTestDotImport reports a dot import there, whose
// names a parse cannot list: every type of the package then keeps its holder. mainTestMethodTypes names the types
// a test file declares a method on: a -tests variant lists that method among the type's own and a plain conversion
// does not, so such a type keeps its holder in both.
var (
	mainTestScopeNames  hashset.HashSet[string]
	mainTestDotImport   bool
	mainTestMethodTypes hashset.HashSet[string]
)

// methodsMayStayInPackageClass answers methodsStayInPackageClass for origin, a type of package main.
func methodsMayStayInPackageClass(origin *types.Named) bool {
	if mainTestDotImport || mainTestMethodTypes.Contains(origin.Obj().Name()) {
		return false
	}

	scope := origin.Obj().Pkg().Scope()

	clashes := func(name string) bool {
		if name == "_" {
			return false
		}

		if scope.Lookup(name) != nil || types.Universe.Lookup(name) != nil || golibBuiltinMembers[name] || mainTestScopeNames.Contains(name) {
			return true
		}

		// A file scope holds the file's imports by name, and the exported names of each package it dot-imports;
		// C#'s `using static` of a dot-imported package brings its unexported names too.
		for i := range scope.NumChildren() {
			fileScope := scope.Child(i)

			if fileScope.Lookup(name) != nil {
				return true
			}

			for _, fileName := range fileScope.Names() {
				if obj := fileScope.Lookup(fileName); obj.Pkg() != nil && obj.Pkg() != origin.Obj().Pkg() {
					if _, isImport := obj.(*types.PkgName); !isImport && obj.Pkg().Scope().Lookup(name) != nil {
						return true
					}
				}
			}
		}

		return false
	}

	for i := range origin.NumMethods() {
		if clashes(origin.Method(i).Name()) {
			return false
		}
	}

	if _, isStruct := origin.Underlying().(*types.Struct); isStruct {
		methodSet := types.NewMethodSet(types.NewPointer(origin))

		for i := range methodSet.Len() {
			fn, isFunc := methodSet.At(i).Obj().(*types.Func)

			if !isFunc || len(methodSet.At(i).Index()) < 2 {
				continue
			}

			if recv := fn.Type().(*types.Signature).Recv(); recv != nil && types.IsInterface(recv.Type()) && clashes(fn.Name()) {
				return false
			}
		}
	}

	return true
}

// holderTypeName is a receiver type's own C# identifier, as the type's declaration spells it: without a
// qualifier and without type arguments. escaped keeps a keyword's `@`, which a class name built from the
// identifier drops.
func (v *Visitor) holderTypeName(named *types.Named) (escaped string) {
	// A generic type is named by its identifier, the spelling getAliasQualifiedTypeName gives a same-package
	// instantiation. Its origin's string form carries the type parameters with their constraints, and a
	// constraint from a package whose import path has a slash (Iterator[T stringish.Interface]) lost the
	// type's own name to that form's last-slash strip, leaving `Interface>`.
	if typeParams := named.Origin().TypeParams(); typeParams != nil && typeParams.Len() > 0 {
		return getSanitizedIdentifier(named.Obj().Name())
	}

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
	if named == nil || named.Obj() == nil || named.Obj().Pkg() != v.pkg || v.fileClassName == "" || methodsStayInPackageClass(named) {
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
		// A shape-witness carrier's C# arity counts its witness parameters too (shapeWitness.go).
		typeofTarget += "<" + strings.Repeat(",", typeParams.Len()-1+shapeWitnessArity(named.Origin().Obj())) + ">"
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
// member from a test file, so its forwarders are the same whether or not the tests are compiled beside it. Where
// the test file emits into its own class (the white-box bridge) the holder's name already differs.
func (v *Visitor) testHolderSuffix(method types.Object, typeObj types.Object) string {
	if method != nil && v.declaredInTestFile(method) && !v.declaredInTestFile(typeObj) {
		return testHolderMark
	}

	return ""
}

// funcDeclClass names the class funcDecl is emitted into: the file's own class, for a function and a method alike.
func (v *Visitor) funcDeclClass(funcDecl *ast.FuncDecl) string {
	// A Go method's body is a member of the file's own class: a method with a holder takes the suffix form,
	// whose body sits in the package class beside its type (GlyphMethodSuffix), and a method of package main
	// with no holder is a plain extension method there (methodsStayInPackageClass).
	return v.fileClassName
}

// suffixMethodHolder answers the holder of funcDecl when it is a Go method that takes the suffix form
// (GlyphMethodSuffix), registering the holder's declaration for package_info.cs, and "" otherwise. Registration
// does not mark the file as writing a holder block: the body sits in the file's own class, and the holder's
// only members are the forwarders package_info.cs declares.
func (v *Visitor) suffixMethodHolder(funcDecl *ast.FuncDecl) string {
	if funcDecl == nil || funcDecl.Recv == nil {
		return ""
	}

	return v.suffixHolderOfMethod(receiverNamed(v.info, funcDecl), v.info.Defs[funcDecl.Name])
}

// suffixHolderOf is suffixMethodHolder for a method the converter writes rather than Go declares (a promoted
// forwarder of named's), which has no declaration of its own.
func (v *Visitor) suffixHolderOf(named *types.Named) string {
	return v.suffixHolderOfMethod(named, nil)
}

func (v *Visitor) suffixHolderOfMethod(named *types.Named, method types.Object) string {
	holder, typeofTarget := v.methodHolderClass(named, method)

	if holder == "" {
		return ""
	}

	// A hand-owned file's Go source may declare a type its C# replacement drops; the holder's declaration names
	// its type in `typeof`, so it is written only where the package's C# sources declare the type (as
	// as for every holder).
	if v.handOwnedTypeNames == nil || v.handOwnedTypeNames[named.Obj().Name()] {
		packageLock.Lock()
		packageMethodHolders[holder] = typeofTarget
		packageLock.Unlock()
	}

	return holder
}

// csharpTypeDeclaration matches a C# type declaration and captures its name: a struct, class, interface,
// record or enum, or a delegate (the name before its parameter list).
var csharpTypeDeclaration = regexp.MustCompile(`\b(?:struct|class|interface|record|enum)\s+@?(\w+)|\bdelegate\s+[^;(]*?\s@?(\w+)\s*(?:<[^>(]*>)?\s*\(`)

// csharpDeclaredTypeNames answers the names of the types the C# sources of a package declare for target:
// its directory and the key folders layout L3 compiles for the target. Review siblings and the package
// information files are not read: the latter declare the accessibility of the converted files' types and
// may still carry a line a hand-owned file has since taken over. A name the converter renamed for a
// collision is answered under its Go name as well.
func csharpDeclaredTypeNames(packageOutputPath string, target string) map[string]bool {
	names := map[string]bool{}
	folders := append([]string{"."}, platformLayoutFolders(target)...)

	for _, folder := range folders {
		paths, _ := filepath.Glob(filepath.Join(packageOutputPath, folder, "*.cs"))

		for _, path := range paths {
			if base := filepath.Base(path); base == "package_info.cs" || strings.HasPrefix(base, "package_test_info") {
				continue
			}

			content, err := os.ReadFile(path)

			if err != nil {
				continue
			}

			for _, match := range csharpTypeDeclaration.FindAllStringSubmatch(string(content), -1) {
				name := match[1] + match[2]
				names[name] = true
				names[strings.TrimPrefix(name, ShadowVarMarker)] = true
			}
		}
	}

	return names
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

// namespaceHides reports whether the package's own namespace declares something named name that a
// method holder reads before the package's members: in the root namespace, a golib type of that name
// with no type parameter; a child namespace in view. Unlike a method of the holder, which hides a name
// only where an expression is read, these hide it in a type position too.
func (v *Visitor) namespaceHides(name string) bool {
	if packageNamespace == RootNamespace && golibNamespaceType(name, v.pkg.Path()) {
		return true
	}

	return packageChildNamespaces[packageNamespace+"."+getSanitizedImport(name)] ||
		(packageNamespace == RootNamespace && golibChildNamespaces[name])
}

// holderHiddenTypeClasses holds, while a method with a holder is being emitted, the types of the
// package that the holder cannot name bare (holderHiddenTypes), each by its Go name with the class
// that declares it. The type-name renderer is a set of package-level functions over the type's Go
// spelling, so the method's visitor publishes the set here for it and leaveHolder withdraws it. Files
// are converted one at a time; nothing reads it concurrently.
var holderHiddenTypeClasses map[string]string

// holderHiddenTypes answers the package-level types of the package being converted whose name the
// package's namespace hides inside a method holder (namespaceHides), each with the class that declares
// it, for the method funcDecl. In the package class a nested type is read before the namespace's
// members; a holder is not nested in it, so there the name binds the namespace or golib's type
// (`this ref sync s` is CS0118 where `go.sync` is in view). A type another class in a `using static`
// also declares is hidden the same way, since the holder reads both classes' nested types with equal
// standing (usingStaticAlsoDeclares). It answers nil where the package has none.
//
// A name that funcDecl itself declares as a type, as a type parameter of its receiver or as a type
// declared in its body, is left out: the renderer sees one spelling for both, and the method's own
// declaration is the nearer one.
func (v *Visitor) holderHiddenTypes(funcDecl *ast.FuncDecl) map[string]string {
	if v.pkg == nil {
		return nil
	}

	if v.namespaceHiddenTypes == nil {
		v.namespaceHiddenTypes = map[string]string{}
		scope := v.pkg.Scope()

		for _, name := range scope.Names() {
			typeName, isType := scope.Lookup(name).(*types.TypeName)

			if !isType || typeName.IsAlias() || !v.namespaceHides(name) && !v.usingStaticAlsoDeclares(name) {
				continue
			}

			v.namespaceHiddenTypes[name] = v.packageScopeClassName(typeName)
		}
	}

	if len(v.namespaceHiddenTypes) == 0 {
		return nil
	}

	hidden := make(map[string]string, len(v.namespaceHiddenTypes))

	for name, class := range v.namespaceHiddenTypes {
		hidden[name] = class
	}

	ast.Inspect(funcDecl, func(node ast.Node) bool {
		switch decl := node.(type) {
		case *ast.TypeSpec:
			delete(hidden, decl.Name.Name)
		case *ast.Ident:
			if _, isTypeParam := v.info.TypeOf(decl).(*types.TypeParam); isTypeParam {
				delete(hidden, decl.Name)
			}
		}

		return true
	})

	return hidden
}

// leaveHolder ends the emission of a method in its holder: what follows stands in the package class.
func (v *Visitor) leaveHolder() {
	holderHiddenTypeClasses = nil
}

// holderHiddenTypeName answers the spelling of the package's type named typeName, a Go name without type
// arguments, through the class that declares it, when the current method's holder cannot name it bare,
// and "" otherwise.
func holderHiddenTypeName(typeName string) string {
	class, hidden := holderHiddenTypeClasses[typeName]

	if !hidden {
		return ""
	}

	return class + "." + getSanitizedIdentifier(getAliasedTypeName(typeName))
}

// holderUnqualifiedTypeName answers typeName, a rendered type, without the class qualifier
// holderHiddenTypeName writes in front of a type the current holder cannot name bare, so a receiver's
// accessibility is judged by the type's own name and not by its class's.
func holderUnqualifiedTypeName(typeName string) string {
	for _, class := range holderHiddenTypeClasses {
		typeName = strings.Replace(typeName, class+".", "", 1)
	}

	return typeName
}

// golibNamespaceType reports whether golib declares, in namespace `go`, a type named name that takes no
// type parameter and that the package at importPath can name: a public one, or an internal one where
// golib names the package's assembly in InternalsVisibleTo. The package's assembly is named as the converter
// names it (go.runtime for runtime, assemblyIdentity.go), and the lists are golib's own (golibNamespaceNames.go,
// generated).
func golibNamespaceType(name, importPath string) bool {
	return golibNamespaceTypes[name] || (golibFriendNamespaceTypes[name] && golibFriendAssemblies[linknameAssemblyName(importPath)])
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
	// C# block.
	if v.funcScopeVarNames.Contains(packageName) {
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

// usingStaticAlsoDeclares reports whether a class the file being converted names in a `using static`,
// other than its own package's, declares a package-level name name. In the package class the class's
// own member wins; a holder is not nested in the package class, so there both using-statics supply the
// name with equal standing (CS0229 for a value, CS0104 for a type) where Go binds the file's own
// package. Two such classes:
//   - a package the file dot-imports, whose every name the assembly can see arrives, unexported ones
//     included, where Go brings in none of them (runtime's hashSize in hash_test.go);
//   - in the external test variant, the package under test's internal-test class, which every
//     external test file names and which compiles into the same assembly, where Go gives the external
//     package none of its names (encoding/json's Small, encoding/gob's Point, net/http's dumpConn).
func (v *Visitor) usingStaticAlsoDeclares(name string) bool {
	if v.options.testExternalVariant && testInternalDeclaredNames.Contains(name) {
		return true
	}

	return v.dotImportDeclares(name)
}

// dotImportDeclares reports whether a package the file being converted dot-imports declares a
// package-level name name, exported or not.
func (v *Visitor) dotImportDeclares(name string) bool {
	if v.astFile == nil {
		return false
	}

	for _, importSpec := range v.astFile.Imports {
		if importSpec.Name == nil || importSpec.Name.Name != "." {
			continue
		}

		if pkgName, ok := v.info.ObjectOf(importSpec.Name).(*types.PkgName); ok && pkgName.Imported().Scope().Lookup(name) != nil {
			return true
		}
	}

	return false
}

// methodHolderRef names the class that holds fn, a method a named type declares, for a static-form
// reference from the file being converted: the bare class for a method of the file's own emission unit,
// and the rooted class for any other. For a method that stays in its package class, the package class is
// that holder. It answers "" for an interface's method, which has no static form.
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

	// A method that stays in its package class is named through that class, as Go names it through its type: a
	// bare name would bind a local, parameter or result of the method's name first, and binds nothing at all
	// from a holder or from a test file's class.
	stays := methodsStayInPackageClass(named)
	bare := strings.TrimPrefix(v.holderTypeName(named), "@")

	if bare == "" && !stays {
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

	if stays {
		return packageClass
	}

	holder := strings.TrimSuffix(packageClass, PackageSuffix) + TempVarMarker + bare

	if sharedClass {
		holder += v.testHolderSuffix(fn, named.Obj())
	}

	return holder
}

// hoistClassKey keys a hoisted literal's field by the class its user is emitted into. Every Go function and
// every Go method body is a member of the file's own class (funcDeclClass: a suffix-form body sits beside its
// type, GlyphMethodSuffix), so the key is the one separator for every user, seeded or not.
func hoistClassKey(_ *types.Info, _ *ast.FuncDecl, _ bool) string {
	return "\x00"
}

// methodExpressionBody answers the suffix-form body a method expression binds directly, `<class>.<Name>⁔`, or ""
// when the expression keeps its holder's forwarder (methodHolderRef). The body is bound where its parameter list
// takes the delegate's receiver as declared: a value receiver, and a pointer receiver declared on the box itself
// (packageDirectBoxReceiverMethods). A pointer receiver's body takes `ref T`, and the expression's delegate takes
// ж<T>, which only RecvGenerator's twin of the holder's forwarder answers, so it keeps the holder. So does a method
// of another package (its body is internal to that assembly), a method of the -tests production or bridge class,
// a hand-owned method (no body is converted), and an sstring twin (its delegate reads the @string companion
// StrGenerator mints beside the forwarder).
func (v *Visitor) methodExpressionBody(fn *types.Func) string {
	if fn == nil || fn.Pkg() != v.pkg || v.whiteboxProductionObject(fn) || v.whiteboxBridgeObject(fn) {
		return ""
	}

	origin := fn.Origin()

	if packageHandOwnedMethods[origin] || len(sstringTwinIndices(origin)) > 0 {
		return ""
	}

	sig, ok := origin.Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return ""
	}

	recvType := types.Unalias(sig.Recv().Type())

	if ptr, isPtr := recvType.(*types.Pointer); isPtr {
		if !packageDirectBoxReceiverMethods[origin] {
			return ""
		}

		recvType = types.Unalias(ptr.Elem())
	}

	named, ok := recvType.(*types.Named)

	if !ok {
		return ""
	}

	if holder, _ := v.methodHolderClass(named, origin); holder == "" {
		return ""
	}

	return v.packageScopeClassName(origin) + "." + suffixBodyName(getSanitizedFunctionName(origin.Name()))
}

// suffixBodyName is the C# name of a suffix-form method's body: the method's name, any verbatim `@` dropped (the
// suffixed name is no keyword), and GlyphMethodSuffix.
func suffixBodyName(csMethodName string) string {
	return strings.TrimPrefix(csMethodName, "@") + GlyphMethodSuffix
}

// recordMethodForwarder records the Go-named forwarder of a suffix-form body, built from the pieces the body's
// own declaration is written from: the method's access, `unsafe`, result, name, type parameters, parameter list
// (with its `this` receiver, as the method was declared before the suffix form) and constraint clauses. The
// forwarder calls the body in the file's class with the same type arguments and arguments.
func (v *Visitor) recordMethodForwarder(funcDecl *ast.FuncDecl, holder string, access string, isUnsafe bool, result string, name string, typeParams string, params string, constraints string) {
	var signature *types.Signature

	if fn, ok := v.info.Defs[funcDecl.Name].(*types.Func); ok {
		signature, _ = fn.Type().(*types.Signature)
	}

	v.recordForwarder(funcDecl.Pos(), holder, access, isUnsafe, result, name, funcDecl.Name.Name, typeParams, params, constraints, signature)
}

// recordForwarder records a forwarder for the holder from the pieces of its body's declaration; pos orders it among
// the package's forwarders (its declaration's position), and signature, when known, is walked for the imported types
// it spells (recordForwarderTypeQualifiers).
func (v *Visitor) recordForwarder(pos token.Pos, holder string, access string, isUnsafe bool, result string, name string, goName string, typeParams string, params string, constraints string, signature *types.Signature) {
	modifiers := access + " static"

	if isUnsafe {
		modifiers += " unsafe"
	}

	call := fmt.Sprintf("%s.%s%s(%s)", v.fileClassName, suffixBodyName(name), forwarderTypeArguments(typeParams), forwarderArguments(params))

	if strings.HasPrefix(result, "ref ") {
		call = "ref " + call
	}

	// The forwarder repeats the body's spelling: package_info.cs reads the package's types as the body's file does
	// (forwarderAliasDirectives). unsafe.Pointer alone is spelled through a file alias it does not declare.
	result = respellForwarderUnsafe(result)
	params = respellForwarderUnsafe(params)
	constraints = respellForwarderUnsafe(flattenConstraints(constraints))

	line := fmt.Sprintf("%s[%s] %s %s %s%s(%s)%s => %s;", goNameAttr(goName, name), methodForwarderAttributes, modifiers, result, name, typeParams, params, constraints, call)

	position := v.fset.Position(pos)
	key := fmt.Sprintf("%s\x00%010d\x00%s", filepath.Base(position.Filename), position.Offset, name)

	v.recordForwarderTypeQualifiers(signature)

	packageLock.Lock()
	packageMethodForwarders = append(packageMethodForwarders, methodForwarder{holder: holder, key: key, line: line, aliases: v.forwarderFileAliases(line)})
	packageLock.Unlock()
}

// respellForwarderUnsafe spells unsafe.Pointer through its package class (`unsafe_package.Pointer`), as an assembly
// attribute and a GoType string do (visitTypeSpec.go): a body reads it through its file's `using @unsafe =
// unsafe_package;`, an alias package_info.cs does not declare.
func respellForwarderUnsafe(text string) string {
	return strings.ReplaceAll(text, "@unsafe.", "unsafe_package.")
}

// forwarderFileAliases answers the file-local aliases of the converting file a forwarder line names, with their
// referents: a variadic parameter's `ꓸꓸꓸT` (variadicParamType), whose referent is written for namespace scope.
func (v *Visitor) forwarderFileAliases(line string) map[string]string {
	var aliases map[string]string

	for _, token := range packageQualifiedNameRegex.FindAllString(line, -1) {
		if !strings.HasPrefix(token, EllipsisOperator) || aliases[token] != "" {
			continue
		}

		for _, using := range v.requiredUsings.Keys() {
			if referent, ok := strings.CutPrefix(using, token+" = "); ok {
				if aliases == nil {
					aliases = map[string]string{}
				}

				aliases[token] = referent
				break
			}
		}
	}

	return aliases
}

// forwarderAliasDirectives takes out of package_info.cs the directives an earlier write put after its namespace
// declaration (insertForwarderAliasDirectives), and answers the aliases this write declares: those a merging write
// found, and those its forwarders name (forwarderAliasUsings). A name already bound to another referent is one more
// that this write's forwarders spell through its referent.
func forwarderAliasDirectives(packageInfoLines []string, mergeExisting bool) ([]string, []string, map[string]bool) {
	usings, inline := forwarderAliasUsings()
	declared := map[string]string{}
	namespaceIndex := packageInfoNamespaceLine(packageInfoLines)

	if namespaceIndex >= 0 {
		end := namespaceIndex + 1

		isDirective := func(i int) bool { return i < len(packageInfoLines) && strings.HasPrefix(packageInfoLines[i], "using ") }

		for isDirective(end) || strings.TrimSpace(packageInfoLines[end]) == "" && isDirective(end+1) {
			if match := forwarderAliasDirective.FindStringSubmatch(packageInfoLines[end]); match != nil && mergeExisting {
				declared[match[1]] = match[2]
			}

			end++
		}

		packageInfoLines = append(packageInfoLines[:namespaceIndex+1:namespaceIndex+1], packageInfoLines[end:]...)
	}

	for alias, referent := range usings {
		if bound, ok := declared[alias]; ok && bound != referent {
			inline[alias] = true
			continue
		}

		declared[alias] = referent
	}

	lines := make([]string, 0, len(declared))

	for alias, referent := range declared {
		lines = append(lines, fmt.Sprintf("using %s = %s;", alias, referent))
	}

	sort.Strings(lines)

	return packageInfoLines, lines, inline
}

// insertForwarderAliasDirectives writes, right after package_info.cs's namespace declaration, the directives its
// forwarders read their signatures through, as each converted file declares its own after its namespace: the file's
// `using static <pkg>_package;` lines and import aliases, repeated, then the aliases the forwarders name. A forwarder repeats its body's
// spelling (`this ref Buffer b`), which in the package class finds the package's nested type first; at the
// compilation unit's level the project's global `using System;` makes that name ambiguous (CS0104), and a
// declaration's own usings are searched before the enclosing levels'. An alias's referent resolves there as in the
// body's file too (`Span<runtime_package.traceArg>`), which it does not among the compilation unit's directives.
func insertForwarderAliasDirectives(packageInfoLines []string, aliasLines []string, hasForwarders bool) []string {
	namespaceIndex := packageInfoNamespaceLine(packageInfoLines)

	if !hasForwarders || namespaceIndex < 0 {
		return packageInfoLines
	}

	inserted := []string{""}

	// A copy is rooted at global:: where a `go.go` namespace (a go/* package's) shadows the root, as a converted
	// file's own `using static` is: inside the namespace declaration `go.` would find `go.go` first.
	rooted := func(referent string) string {
		if strings.HasPrefix(referent, RootNamespace+".") {
			return globalQualifyRooted(referent)
		}

		return referent
	}

	for _, line := range packageInfoLines[:namespaceIndex] {
		if target, ok := strings.CutPrefix(line, "using static "); ok {
			inserted = append(inserted, "using static "+rooted(strings.TrimSuffix(target, ";"))+";")
		}
	}

	// The compilation unit's import aliases (`using io = go.io_package;`): inside the namespace a qualifier finds a
	// namespace of its name first (`go.io`, which io/fs makes), so `io.Writer` reads the package class only through an
	// alias the declaration itself carries, as each converted file's does.
	for _, line := range packageInfoLines[:namespaceIndex] {
		if forwarderImportAlias.MatchString(line) {
			alias, referent, _ := strings.Cut(strings.TrimSuffix(strings.TrimPrefix(line, "using "), ";"), " = ")
			inserted = append(inserted, "using "+alias+" = "+rooted(referent)+";")
		}
	}

	inserted = append(inserted, aliasLines...)

	return append(packageInfoLines[:namespaceIndex+1:namespaceIndex+1], append(inserted, packageInfoLines[namespaceIndex+1:]...)...)
}

// packageInfoNamespaceLine answers the index of package_info.cs's namespace declaration, or -1.
func packageInfoNamespaceLine(packageInfoLines []string) int {
	for i, line := range packageInfoLines {
		if strings.HasPrefix(strings.TrimSpace(line), "namespace ") {
			return i
		}
	}

	return -1
}

var forwarderImportAlias = regexp.MustCompile(`^using [\p{L}\p{N}_@]+ = [^;]+;$`)

var forwarderAliasDirective = regexp.MustCompile(`^using (` + EllipsisOperator + `[\p{L}\p{N}_]*) = (.+);$`)

// forwarderAliasUsings answers the file-local aliases package_info.cs declares for the forwarders it writes (those
// whose holder the unit declares), and the alias names two files bind to different referents: a forwarder spells
// one of those through its referent instead (applyMethodForwarderSection), since package_info.cs can bind a name once.
func forwarderAliasUsings() (usings map[string]string, inline map[string]bool) {
	usings, inline = map[string]string{}, map[string]bool{}

	packageLock.Lock()
	defer packageLock.Unlock()

	for _, forwarder := range packageMethodForwarders {
		if _, declared := packageMethodHolders[forwarder.holder]; !declared {
			continue
		}

		for alias, referent := range forwarder.aliases {
			if bound, ok := usings[alias]; ok && bound != referent {
				inline[alias] = true
			}

			usings[alias] = referent
		}
	}

	for alias := range inline {
		delete(usings, alias)
	}

	return usings, inline
}

// inlineForwarderAliases spells each alias of line that inline names through its referent.
func inlineForwarderAliases(line string, aliases map[string]string, inline map[string]bool) string {
	if len(inline) == 0 || len(aliases) == 0 {
		return line
	}

	return packageQualifiedNameRegex.ReplaceAllStringFunc(line, func(token string) string {
		if inline[token] && aliases[token] != "" {
			return aliases[token]
		}

		return token
	})
}

// recordForwarderTypeQualifiers registers, for package_info.cs, the import qualifier of every other package's
// named type the forwarder's signature spells (its receiver, parameters and results, through every wrapper and
// type argument), so package_info.cs resolves it through a file-local `using` exactly as the converted file does
// (conversionPackageUsings). No file's own usings are copied.
func (v *Visitor) recordForwarderTypeQualifiers(signature *types.Signature) {
	if signature == nil {
		return
	}

	visited := map[types.Type]bool{}
	var walk func(t types.Type)

	walk = func(t types.Type) {
		if t == nil || visited[t] {
			return
		}

		visited[t] = true

		switch t := t.(type) {
		case *types.Alias:
			walk(types.Unalias(t))
		case *types.Pointer:
			walk(t.Elem())
		case *types.Slice:
			walk(t.Elem())
		case *types.Array:
			walk(t.Elem())
		case *types.Map:
			walk(t.Key())
			walk(t.Elem())
		case *types.Chan:
			walk(t.Elem())
		case *types.Signature:
			walkTuple(t.Params(), walk)
			walkTuple(t.Results(), walk)
		case *types.Named:
			v.recordConversionPackageUsing(t)
		}
	}

	if recv := signature.Recv(); recv != nil {
		walk(recv.Type())
	}

	walkTuple(signature.Params(), walk)
	walkTuple(signature.Results(), walk)
}

func walkTuple(tuple *types.Tuple, walk func(types.Type)) {
	if tuple == nil {
		return
	}

	for i := 0; i < tuple.Len(); i++ {
		walk(tuple.At(i).Type())
	}
}

// forwarderTypeArguments answers the type argument list a forwarder passes its body: the declaration's type
// parameter list with any attribute dropped.
func forwarderTypeArguments(typeParams string) string {
	if typeParams == "" {
		return ""
	}

	return forwarderAttributeList.ReplaceAllString(typeParams, "")
}

var forwarderAttributeList = regexp.MustCompile(`\[[^\[\]]*\]\s*`)

// flattenConstraints answers a declaration's constraint clauses on one line, as a forwarder's single line carries
// them: every run of whitespace (the clauses' newlines and indentation) becomes one space.
func flattenConstraints(constraints string) string {
	if strings.TrimSpace(constraints) == "" {
		return ""
	}

	return " " + strings.Join(strings.Fields(constraints), " ")
}

// forwarderArguments answers the arguments a forwarder passes its body for a parameter list the converter wrote:
// each parameter's name, with the `ref`, `out` or `in` its declaration carries (`ref readonly` passes as `in`).
// The list is split at its top-level commas; a parameter's attributes, `this`, `scoped`, `params` and type are
// not arguments.
func forwarderArguments(params string) string {
	var args []string

	for _, param := range splitTopLevelParameters(params) {
		param = strings.TrimSpace(forwarderAttributeList.ReplaceAllString(param, ""))

		if param == "" {
			continue
		}

		fields := strings.Fields(param)
		name := fields[len(fields)-1]
		modifier := ""

		// The keywords lead the parameter, and no C# type spells one, so a scan of every token ahead of the name
		// finds them.
		for i, field := range fields[:len(fields)-1] {
			switch field {
			case "ref":
				modifier = "ref "

				if i+1 < len(fields)-1 && fields[i+1] == "readonly" {
					modifier = "in "
				}
			case "out":
				modifier = "out "
			case "in":
				modifier = "in "
			}
		}

		args = append(args, modifier+name)
	}

	return strings.Join(args, ", ")
}

// splitTopLevelParameters splits a C# parameter list at the commas outside any bracket.
func splitTopLevelParameters(params string) []string {
	var parts []string
	depth, start := 0, 0

	for i, r := range params {
		switch r {
		case '<', '(', '[', '{':
			depth++
		case '>', ')', ']', '}':
			depth--
		case ',':
			if depth == 0 {
				parts = append(parts, params[start:i])
				start = i + 1
			}
		}
	}

	return append(parts, params[start:])
}

// methodForwarderProseLines introduce the forwarder section in package_info.cs.
func methodForwarderProseLines() []string {
	return []string{
		"// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),",
		"// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line",
		"// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names",
		"// the body's frame as the Go method.",
	}
}

// applyMethodForwarderSection writes the forwarders of this emission unit's suffix-form methods into
// package_info.cs, after the holder declarations, with the merge semantics of the holder section: a whole-package
// conversion rebuilds the section from this run's forwarders, and a merging write keeps the lines this pass did
// not write again. Only forwarders whose holder this unit declares are written (packageMethodHolders), so a
// -tests variant's forwarders go where its holders go.
func applyMethodForwarderSection(packageInfoLines []string, mergeExisting bool) []string {
	// First, so the section's indices below are read from the lines this pass rebuilds.
	packageInfoLines, aliasLines, inlineAliases := forwarderAliasDirectives(packageInfoLines, mergeExisting)
	startLineIndex, endLineIndex := -1, -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<"+MethodForwarderSection+">") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</"+MethodForwarderSection+">") {
			endLineIndex = i
			break
		}
	}

	hasSection := startLineIndex >= 0 && endLineIndex > startLineIndex
	byHolder := map[string][]string{}
	seen := map[string]bool{}

	if mergeExisting && hasSection {
		holder := ""

		for i := startLineIndex + 1; i < endLineIndex; i++ {
			line := strings.TrimSpace(packageInfoLines[i])

			switch {
			case strings.HasPrefix(line, "partial class ") && strings.HasSuffix(line, "{"):
				holder = strings.TrimSpace(strings.TrimSuffix(strings.TrimPrefix(line, "partial class "), "{"))
			case line == "}":
				holder = ""
			case holder != "" && line != "":
				// An earlier write may have spelled the attributes short; this pass compares, and
				// applyPackageInfoSystemUsings respells, in the global:: spelling.
				line = respellSystemAttributes(line, false)

				if !seen[holder+"\x00"+line] {
					seen[holder+"\x00"+line] = true
					byHolder[holder] = append(byHolder[holder], line)
				}
			}
		}
	}

	packageLock.Lock()
	recorded := append([]methodForwarder(nil), packageMethodForwarders...)
	packageLock.Unlock()

	sort.SliceStable(recorded, func(i, j int) bool { return recorded[i].key < recorded[j].key })

	for _, forwarder := range recorded {
		if _, declared := packageMethodHolders[forwarder.holder]; !declared {
			continue
		}

		line := inlineForwarderAliases(forwarder.line, forwarder.aliases, inlineAliases)

		if !seen[forwarder.holder+"\x00"+line] {
			seen[forwarder.holder+"\x00"+line] = true
			byHolder[forwarder.holder] = append(byHolder[forwarder.holder], line)
		}
	}

	if hasSection {
		// The section and the prose and blank line above it are rebuilt below.
		from := startLineIndex

		for from > 0 && strings.HasPrefix(strings.TrimSpace(packageInfoLines[from-1]), "//") && !strings.Contains(packageInfoLines[from-1], "</") {
			from--
		}

		if from > 0 && strings.TrimSpace(packageInfoLines[from-1]) == "" {
			from--
		}

		packageInfoLines = append(packageInfoLines[:from:from], packageInfoLines[endLineIndex+1:]...)
	}

	packageInfoLines = insertForwarderAliasDirectives(packageInfoLines, aliasLines, len(byHolder) > 0)

	if len(byHolder) == 0 {
		return packageInfoLines
	}

	holders := make([]string, 0, len(byHolder))

	for holder := range byHolder {
		holders = append(holders, holder)
	}

	sort.Strings(holders)

	for len(packageInfoLines) > 0 && strings.TrimSpace(packageInfoLines[len(packageInfoLines)-1]) == "" {
		packageInfoLines = packageInfoLines[:len(packageInfoLines)-1]
	}

	packageInfoLines = append(packageInfoLines, "")
	packageInfoLines = append(packageInfoLines, methodForwarderProseLines()...)
	packageInfoLines = append(packageInfoLines, "// <"+MethodForwarderSection+">")

	for _, holder := range holders {
		packageInfoLines = append(packageInfoLines, "partial class "+holder+" {")

		for _, line := range byHolder[holder] {
			packageInfoLines = append(packageInfoLines, "    "+line)
		}

		packageInfoLines = append(packageInfoLines, "}")
	}

	return append(packageInfoLines, "// </"+MethodForwarderSection+">")
}
