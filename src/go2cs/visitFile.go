// visitFile.go - Gbtc
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
	"log"
	"sort"
	"strings"
)

const TypeAliasMarker = ">>MARKER:TYPEALIASES<<"
const UsingsMarker = ">>MARKER:USINGS<<"

// ImportInitMarker used to reserve the top of the file's class body for the imported-package
// module-initializer hooks. Those hooks now live in the emission unit's metadata file
// (writeImportInit → packageImportInits → writeImportInitSection), so no file reserves anything for
// them and the marker is gone with the reservation. The ordering guarantee it existed to provide is
// stronger where they went: package_info.cs is the FIRST compile item of every generated project, so
// ALL of a package's import hooks precede ALL of its own `init` functions, deterministically —
// where this marker could only order a hook ahead of the inits of its own file.

// systemNamespaceTypes is, for each .NET namespace the converter imports on demand, exactly the
// types the emission that demands it names: `[MethodImpl(MethodImplOptions.NoInlining)]`
// (noInliningPrefix) and `[StructLayout(LayoutKind.Explicit, …)]` / `[FieldOffset(n)]`
// (zeroSizeFieldLayout). A new emission that names another type of either namespace adds it here.
var systemNamespaceTypes = map[string][]string{
	"System.Runtime.CompilerServices": {"MethodImplAttribute", "MethodImplOptions"},
	"System.Runtime.InteropServices":  {"FieldOffsetAttribute", "LayoutKind", "StructLayoutAttribute"},
}

// importsStaticMembers reports whether this file resolves bare Go names through a `using static`:
// its own Go dot-import, or the test project it is compiled into. A test project whose production
// is a referenced assembly (testProductionName set: the reference and white-box reference models)
// seeds a `global using static` of the production class, and of the white-box bridge, for every
// file in it. A global using sits OUTSIDE a file's file-scoped namespace, so for those files a
// namespace import would not collide with a Go name, it would silently win -- every file in such a
// project takes the aliases. The file-level `using static` lines a test file writes (visitFile's
// production and bridge imports, testClassNameOverride's internal white-box file) arise only in such
// a project, so they need no check of their own.
func (v *Visitor) importsStaticMembers() bool {
	if v.options.testProductionName != "" {
		return true
	}

	imports := v.packageImports.String()

	return strings.HasPrefix(imports, "using static ") || strings.Contains(imports, "\nusing static ")
}

func (v *Visitor) Visit(node ast.Node) ast.Visitor {
	if node != nil {
		if commentGroup, ok := node.(*ast.CommentGroup); ok {
			for _, comment := range commentGroup.List {
				delete(v.standAloneComments, comment.Slash)
			}
		}
	}

	return v
}

func (v *Visitor) visitFile(file *ast.File) {
	// Recover the contextual types go/types drops inside constant expressions (see
	// markUntypedConstContexts) before any expression conversion renders a literal
	v.markUntypedConstContexts(file)

	// Decide, per narrow-integer arithmetic expression, whether its consumer needs Go's width (see
	// markNarrowArithmeticContexts)
	v.markNarrowArithmeticContexts(file)

	// Mark the func literals whose destination is an empty interface, at every position (see
	// markEmptyInterfaceFuncLits)
	v.markEmptyInterfaceFuncLits(file)

	if v.options.includeComments {
		// Create standalone comments map
		for _, commentGroup := range file.Comments {
			for _, comment := range commentGroup.List {
				v.standAloneComments[comment.Slash] = carriedComment(comment.Text)
			}
		}

		// Remove CommentGroup instances that exist as AST nodes from standalone comments map
		ast.Walk(v, file)

		for pos := range v.standAloneComments {
			v.sortedCommentPos = append(v.sortedCommentPos, pos)
		}

		sort.Slice(v.sortedCommentPos, func(i, j int) bool {
			return v.sortedCommentPos[i] < v.sortedCommentPos[j]
		})

		v.writeDoc(file.Doc, file.Package)
	}

	// License notices are retained even when ordinary comments are disabled.
	if !v.options.includeComments {
		v.outputBuilder.WriteString(sourceLicenseNotices(file, v.newline))
	}
	if v.options.provenance {
		v.outputBuilder.WriteString(sourceProvenance(v.fset.PositionFor(file.Package, false).Filename, v.options, v.newline))
	}

	// Derive package name
	packageLock.Lock()

	if len(packageName) == 0 {
		packageName = file.Name.Name
	} else {
		if packageName != file.Name.Name {
			log.Fatalf("Multiple package names encountered: %s and %s", packageName, file.Name.Name)
		}
	}

	packageLock.Unlock()

	v.writeOutput(TypeAliasMarker)
	v.writeOutputLn("namespace %s;", packageNamespace)
	v.outputBuilder.WriteString(v.newline)

	packageClassName := getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))
	if v.options.testClassNameOverride != "" {
		packageClassName = v.options.testClassNameOverride

		// A TEST-ONLY package under test emitted no production class, so there is none to import
		// here — the directive would be CS0234 in every emitted test file (productionClassEmitted).
		if !v.options.testProductionAbsent {
			productionClassName := getSanitizedImport(v.options.testProductionName + PackageSuffix)
			v.addRequiredUsing(fmt.Sprintf("static %s", globalQualifyRooted(packageNamespace+"."+productionClassName)))
		}
	}
	if v.options.testWhiteboxReference && v.options.testExternalVariant {
		v.addRequiredUsing(fmt.Sprintf("static %s", globalQualifyRooted(packageNamespace+"."+v.options.testInternalBridgeName)))
	}

	// Record the enclosing class for emitters that write INTO its body and must reason about what
	// its nested types occlude — see forcingTargetShadowed. Set before the decl walk below, since
	// import specs are the first decls of every file and consult it as they are visited.
	v.emittedClassName = packageClassName

	v.writeOutput(UsingsMarker)
	v.writeOutputLn("partial class %s {", packageClassName)

	v.collectMethodNamespaceUsings(file)

	for _, decl := range file.Decls {
		v.visitDecl(decl)
	}

	if v.options.includeComments {
		// Add any remaining standalone comments
		postCodeComments := strings.Builder{}
		v.writeDocString(&postCodeComments, nil, file.FileEnd)
		v.outputBuilder.WriteString(v.newline)

		if postCodeComments.Len() > 0 {
			v.writeOutputLn("%s", postCodeComments.String())
		} else {
			if v.needsNewLine(v.outputBuilder.String()) {
				v.outputBuilder.WriteString(v.newline)
			}
		}
	} else {
		if v.needsNewLine(v.outputBuilder.String()) {
			v.outputBuilder.WriteString(v.newline)
		}
	}

	v.writeOutputLn("} // end %s", packageClassName)

	// Take the completed body as text so the deferred markers below can be substituted. The using
	// directives and type aliases are only KNOWN once the whole file has been visited, so they are
	// emitted as markers during the walk and resolved here.
	fileText := v.outputBuilder.String()

	// Supply the file-local `using <alias> = <namespace>;` for any foreign package whose type this file
	// referenced in short-alias form (`pkg.Type`, `@unsafe.Pointer`) but did not import under its
	// canonical name — an inferred type, or a blank/dot/renamed import (CS0246). Skipped for a package
	// the file already imports canonically, so a normal import emits its alias exactly once (no
	// duplicate, no churn); a non-canonical alias (`using t = time_package;`) coexists with the added
	// canonical one (`using time = time_package;`) without conflict.
	type suppliedUsing struct{ importPath, namespace string }

	supplied := map[string][]suppliedUsing{}

	for _, importPath := range v.referencedForeignPackages.Keys() {
		// referencedForeignPackages is keyed by pkg.Path() (unprefixed for GOROOT-vendored
		// packages), while canonicalAliasImported records the RESOLVED on-disk path — check
		// both forms, or a vendored import's canonical alias emits twice (CS1537).
		if v.canonicalAliasImported.Contains(importPath) || v.canonicalAliasImported.Contains(resolveGorootVendoredPath(importPath)) {
			continue
		}

		alias, namespace := packageUsingAlias(importPath)

		// The alias is collision-renamed (`Δunicode`) when a same-named CHILD namespace is visible from
		// the import closure (`go.unicode`, present once unicode/utf8 is transitively imported):
		// getAliasedTypeName renders this file's short-form type references through that SAME renamed
		// qualifier (`Δunicode.Range16`), so the SUPPLIED canonical using must carry the rename too, or the
		// reference binds an alias that was never emitted (CS0246) — and the bare `using unicode =
		// unicode_package;` would itself collide with the child namespace (CS0576). Surfaced by a -tests
		// external variant that DOT-imports the package under test (unicode's letter_test) yet still
		// references its types qualified; importQualifier is a no-op for any package that isn't renamed, so
		// a non-colliding supplied alias is byte-identical.
		alias = getSanitizedImport(importQualifier(alias))

		// Skip when the canonical alias collides with one a real import already bound to a DIFFERENT
		// namespace: cryptobyte's asn1.go imports `encoding_asn1 "encoding/asn1"` (referenced by type,
		// so it lands here) AND the subpackage `.../cryptobyte/asn1` (unaliased → alias `asn1`), so
		// synthesizing `using asn1 = encoding.asn1_package` duplicates the subpackage's `using asn1`
		// (CS1537) and mis-binds `asn1.X`. The parent is in scope via its `encoding_asn1` alias, so the
		// canonical one is unneeded here. (A non-colliding canonical alias is still supplied, so an
		// inferred type reference to a non-canonically-imported package keeps working — no churn.)
		if v.importAliasesEmitted.Contains(alias) {
			continue
		}

		// One package can arrive under two path spellings (vendored and resolved); it is still one
		// namespace, supplied once, exactly as the set below always deduplicated it.
		duplicate := false

		for _, candidate := range supplied[alias] {
			duplicate = duplicate || candidate.namespace == namespace
		}

		if !duplicate {
			supplied[alias] = append(supplied[alias], suppliedUsing{importPath, namespace})
		}
	}

	// Two DIFFERENT packages can need the same canonical alias: a/foo and b/foo, both `package foo`,
	// imported as `afoo` and `bfoo`. One `using foo` cannot name both (CS1537). A package this file
	// imports under an EXPLICIT alias renders its types through that alias (importPathAliases), so it
	// yields the canonical one; whatever remains is supplied only when it is a single package.
	for alias, candidates := range supplied {
		if len(candidates) > 1 {
			var kept []suppliedUsing

			for _, candidate := range candidates {
				if v.importPathAliases[candidate.importPath] == "" {
					kept = append(kept, candidate)
				}
			}

			candidates = kept
		}

		if len(candidates) == 1 {
			v.addRequiredUsing(fmt.Sprintf("%s = %s", alias, candidates[0].namespace))
		}
	}

	// Ensure using ortder is consistent
	requiredUsings := v.requiredUsings.Keys()
	sort.Strings(requiredUsings)

	importsStaticMembers := v.importsStaticMembers()

	for _, requiredUsing := range requiredUsings {
		// A file that reaches Go names BARE through a `using static` cannot also import a whole .NET
		// namespace: the two imports sit at one level, so a Go name the namespace happens to declare
		// as a type is CS0229 (go/types' `Unsafe` against System.Runtime.CompilerServices.Unsafe).
		// An alias directive outranks both, so such a file binds exactly the types its emission
		// names and the emitted attribute text is unchanged.
		if typeNames, ok := systemNamespaceTypes[requiredUsing]; ok && importsStaticMembers {
			for _, typeName := range typeNames {
				v.packageImports.WriteString(fmt.Sprintf("using %s = global::%s.%s;%s", typeName, requiredUsing, typeName, v.newline))
			}

			continue
		}

		v.packageImports.WriteString(fmt.Sprintf("using %s;%s", requiredUsing, v.newline))
	}

	if v.packageImports.Len() > 0 {
		v.packageImports.WriteString(v.newline)
	}

	fileText = strings.ReplaceAll(fileText, UsingsMarker, v.packageImports.String())

	if v.typeAliasDeclarations.Len() > 0 {
		v.typeAliasDeclarations.WriteString(v.newline)
		fileText = strings.ReplaceAll(fileText, TypeAliasMarker, v.typeAliasDeclarations.String())
	} else {
		fileText = strings.ReplaceAll(fileText, TypeAliasMarker, "")
	}

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(fileText)
}

func (v *Visitor) needsNewLine(text string) bool {
	newLineLen := len(v.newline)
	lastChars := text[len(text)-(newLineLen*2):]

	return lastChars != v.newline+v.newline
}
