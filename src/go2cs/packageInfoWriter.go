// packageInfoWriter.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// This file owns the emission of package_info.cs — the per-package metadata file that carries
// everything the go2cs-gen source generators and OTHER converted packages need to know about this
// one, but which has no place in the converted code itself.
//
// What lands there: the assembly-level GoImplement / GoImplicitConv records (which struct
// implements which interface, which conversions exist), the exported type aliases, the package
// doc comment. A package that imports this one reads its package_info.cs back — see
// importOperations.go — which is how a cross-package interface conversion knows an adapter class
// already exists rather than generating a second one.
//
// The writer merges with any existing file rather than overwriting, so records contributed by a
// hand-owned or test-variant conversion of the same package survive a reconvert.

package main

import (
	"encoding/hex"
	"fmt"
	"go/build"
	"log"
	"os"
	"path/filepath"
	"sort"
	"strings"
)

// legacyInterfaceImplementationsFirstLine identifies the first line of the <InterfaceImplementations>
// explanatory block, which is the same in every wording the converter has emitted — so it locates the
// block for migrateProseBlock whatever the rest of it says.
const legacyInterfaceImplementationsFirstLine = "// As types are cast to interfaces in Go source code, the go2cs code converter"

// rootEscapePrefix is the C# root-namespace escape. The `-tests` alias-shadow machinery mints it
// (testAliasShadowOperations / convSelectorExpr) whenever a test package's own class shadows the
// leading segment of a qualified reference; a site that needs no escape renders the same type bare.
const rootEscapePrefix = "global::"

// dedupeRootEscapedRecords collapses assembly-record lines that differ ONLY by `global::` root
// escapes, returning the surviving lines still sorted.
//
// The record sections are HashSets of RENDERED TEXT, so two spellings of one type are two entries.
// That is invisible in a production conversion — the escape is a `-tests` phenomenon — but a test
// package registers the same (impl, interface) pair from several cast sites, and one escaped site
// is enough to double the record. go2cs-gen resolves both spellings to the SAME symbol, so it mints
// the adapter twice: net/http's `http_HandlerFuncᴠΔHandler` was emitted as both `-val.g.cs` and
// `-val.1.g.cs`, giving CS0102 + CS0111 ×5 + CS8646 ×2. Same family as the alias-spelling duplicate
// the covered-set skip above resolves for `os`'s dirEntry — a different way to spell one type, with
// the same consequence — which is why this is a shared pass over both record sections rather than a
// second special case inside either.
//
// The ESCAPED spelling wins a collapse. It is shadow-proof by construction, which is the whole
// reason the machinery minted it; keeping the bare form could reintroduce the very shadow the escape
// was emitted to defeat. Ties (each side escaped in a different record) fall to the lexicographically
// smaller line so the output stays deterministic. Merging is safe even where the bare form WOULD
// resolve elsewhere: that case means the bare record was already naming the wrong type.
func dedupeRootEscapedRecords(sortedLines []string) []string {
	best := map[string]string{}
	order := []string{}

	for _, line := range sortedLines {
		key := strings.ReplaceAll(line, rootEscapePrefix, "")
		existing, seen := best[key]

		if !seen {
			best[key] = line
			order = append(order, key)
			continue
		}

		if escapes, priorEscapes := strings.Count(line, rootEscapePrefix), strings.Count(existing, rootEscapePrefix); escapes > priorEscapes || (escapes == priorEscapes && line < existing) {
			best[key] = line
		}
	}

	result := make([]string, 0, len(order))

	for _, key := range order {
		result = append(result, best[key])
	}

	sort.Strings(result)

	return result
}

// interfaceImplementationsProseLines returns the <InterfaceImplementations> section's explanatory
// comment. Like every emitted-artifact comment it states what the section holds and the constraint
// that shape serves, and nothing about how it came to be that way — this text ships in every
// converted package, where a reader has no way to check a claim about the converter's past.
// package_info-template.txt carries the same lines for a file created from scratch; the two must
// agree, which the seam guard in lineEndingSeams_test.go asserts.
func interfaceImplementationsProseLines() []string {
	return []string{
		"// As types are cast to interfaces in Go source code, the go2cs code converter",
		"// will generate an assembly level `GoImplement` attribute for each unique cast.",
		"// This allows the interface to be implemented in the C# source code using source",
		"// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time",
		"// this way is what keeps startup free of reflection.",
	}
}

// migrateProseBlock rewrites a converter-owned explanatory block in a persisted package info file to
// its CURRENT wording. The block runs from legacyFirstLine to the line carrying marker (its section's
// opening tag), and is replaced by prose plus the blank line that separates the two. A file whose
// block already matches is rewritten to the identical text, so the pass is idempotent; a file that
// does not carry legacyFirstLine at all — one rendered fresh from the template — is returned
// untouched.
//
// Identifying a block by its FIRST LINE is what makes this safe: the blocks are converter-owned and
// were only ever emitted verbatim, so an exact match cannot land on user text, and a rewrite that
// keeps the first line (every one so far) needs no new anchor.
func migrateProseBlock(packageInfoLines []string, legacyFirstLine string, marker string, prose []string) []string {
	markerIndex := -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, marker) {
			markerIndex = i
			break
		}
	}

	if markerIndex < 0 {
		return packageInfoLines
	}

	legacyStart := -1

	for i := range markerIndex {
		if strings.Contains(packageInfoLines[i], legacyFirstLine) {
			legacyStart = i
			break
		}
	}

	if legacyStart < 0 {
		return packageInfoLines
	}

	updated := make([]string, 0, len(packageInfoLines))
	updated = append(updated, packageInfoLines[:legacyStart]...)
	updated = append(updated, prose...)
	updated = append(updated, "")
	updated = append(updated, packageInfoLines[markerIndex:]...)

	return updated
}

// writePackageInfoFile creates or updates a package information file (package_info.cs, or the
// test conversion's package_test_info.cs) by inserting the CURRENT package-scoped metadata
// globals (imported/exported type aliases, interface implementations, implicit conversions)
// into the file's marker sections. When mergeExisting is true each section's existing entries
// are preserved and merged with the new ones (single-file conversions, and the -tests path --
// which seeds package_test_info.cs from the production package_info.cs and then merges each
// test variant's additions); when false the sections are rebuilt from the current globals
// alone (whole-package conversions). Shared by processConversion and the test-conversion path
// so the emission semantics (pointer-form GoImplement unwrapping, alias-covered dedup,
// inheritance pruning, constraint proxies, numeric conversions, manual-type skips) can never
// drift between the two.
func writePackageInfoFile(packageInfoFileName string, mergeExisting bool) {
	var packageInfoLines []string

	if _, err := os.Stat(packageInfoFileName); err == nil {
		// Read all lines from existing package info file
		packageInfoBytes, err := os.ReadFile(packageInfoFileName)

		if err != nil {
			log.Fatalf("Failed to read existing package info file \"%s\": %s\n", packageInfoFileName, err)
		}

		// EOL-agnostic: this file is READ BACK off disk, so its line endings are the checkout's, not
		// the converter's. Splitting on "\r\n" alone returns ONE element for an LF copy — every
		// marker scan below then fails and log.Fatals, so a clone that materializes LF (any
		// non-Windows checkout, or core.autocrlf=false) cannot convert a single package that already
		// has a committed package_info.cs (F3 in docs/PLAN-linux-operation.md). The WRITE path is
		// unchanged — each line is still emitted with "\r\n" — so output stays byte-identical.
		packageInfoLines = splitLines(string(packageInfoBytes))
	} else {
		// Generate new package info file from template. The template is pinned CRLF at CHECKOUT
		// (.gitattributes) and embedded at COMPILE time, so its endings are a property of the tree
		// the converter was built from — an attribute added after a clone was materialized does not
		// rewrite it. Split the same EOL-agnostic way so the emitted bytes are the writer's CRLF
		// either way, rather than a fresh package_info.cs being fatal on a tree that predates the pin.
		packageClassName := getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))
		templateFile := fmt.Sprintf(string(packageInfoTemplate), packageNamespace+"."+packageClassName, packageNamespace, packageName, packageClassName)
		packageInfoLines = splitLines(templateFile)
	}

	// Converge a persisted file's converter-owned PROSE on its current wording. Only the marker
	// sections below are rebuilt from the live globals — every other line of an existing
	// package_info.cs is copied through verbatim — so without this a file keeps whichever wording
	// was current when it was first created, and the corpus carries as many versions of each block
	// as it has had rewrites.
	packageInfoLines = migrateProseBlock(packageInfoLines, legacyInterfaceImplementationsFirstLine,
		"<InterfaceImplementations>", interfaceImplementationsProseLines())

	// Handle imported type aliases
	startLineIndex := -1
	endLineIndex := -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<ImportedTypeAliases>") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</ImportedTypeAliases>") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		// Read existing type aliases from package info file
		lines := HashSet[string]{}

		// If processing a single file, instead of all package files, merge type aliases…
		if mergeExisting {
			// …except one the CURRENT flavour CONTRADICTS. Merge preserves, and preservation has no
			// notion of a per-GOOS declaration: layout L3 splits production metadata by flavour
			// while package_test_info.cs stays FLAT — one file serving three — so a windows-seeded
			// `global using syscallꓸHandle = go.syscall_package.ΔHandle;` is merged forward untouched
			// by a linux run and binds an alias to a type that flavour never declares (CS0426/CS0305
			// on syscall's Linux `-tests` build). The entry is not re-derived and found wrong, it is
			// simply KEPT, which is why the defect is sticky rather than self-healing.
			//
			// The mirror of the import-hook merge rule (importInitSection.go): there a merging write
			// meets one hook under two spellings and the FRESH entry wins, being this emission
			// unit's own decision; here the CONTRADICTED entry is dropped, for the same reason from
			// the other side. Only the IMPORTED section needs it — the exported section's
			// `[assembly: GoTypeAlias("Handle", "ΔHandle")]` carries strings and binds no type.
			contradicted := flavourContradictedAliases(filepath.Dir(packageInfoFileName), build.Default.GOOS)

			for i := startLineIndex + 1; i < endLineIndex; i++ {
				line := packageInfoLines[i]

				if aliasContradictsFlavour(line, contradicted) {
					continue
				}

				lines.Add(strings.TrimSpace(line))
			}
		}

		// Add new type aliases to package info file (hashset ensures uniqueness). A DERIVED alias
		// (synthesized because the dependency has no package_info.cs to read) is emitted only when
		// an emitted reference actually resolved through it — see derivedTypeAliases for why an
		// unused one must not be declared.
		for alias, typeName := range importedTypeAliases {
			if constImportedTypeAliases.Contains(alias) {
				continue
			}

			if derivedTypeAliases.Contains(alias) && !usedDerivedTypeAliases.Contains(alias) {
				continue
			}

			lines.Add(fmt.Sprintf("global using %s = %s;", strings.ReplaceAll(alias, ".", TypeAliasDot), typeName))
		}

		// Add package-qualifier aliases used by recorded GoImplicitConv attributes (e.g.
		// `abi.Type`). package_info.cs has no file-local `using abi = …`; emit a FILE-LOCAL `using`
		// (not `global` — that would clash with the per-file `using abi = …` other source files
		// already declare, CS1537) resolving each alias to its `go`-rooted namespace.
		for alias, namespace := range conversionPackageUsings {
			lines.Add(fmt.Sprintf("using %s = %s.%s;", alias, RootNamespace, namespace))
		}

		// Sort lines
		sortedLines := lines.Keys()
		sort.Strings(sortedLines)

		// Insert imported type aliases into package info file
		packageInfoLines = append(packageInfoLines[:startLineIndex+1],
			append(sortedLines, packageInfoLines[endLineIndex:]...)...)
	} else {
		log.Fatalf("Failed to find '<ImportedTypeAliases>...</ImportedTypeAliases>' section for inserting exported type aliases into package info file \"%s\"\n", packageInfoFileName)
	}

	// Handle exported type aliases
	startLineIndex = -1
	endLineIndex = -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<ExportedTypeAliases>") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</ExportedTypeAliases>") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		// Read existing type aliases from package info file
		lines := HashSet[string]{}

		// If processing a single file, instead of all package files, merge type aliases
		if mergeExisting {
			for i := startLineIndex + 1; i < endLineIndex; i++ {
				line := packageInfoLines[i]
				lines.Add(strings.TrimSpace(line))
			}
		}

		// Add new type aliases to package info file (hashset ensures uniqueness). A NON-GENERIC
		// METHODLESS named func type is rendered inline as its base delegate with no named
		// declaration (visitFuncType), so it has no `<pkg>_package.<Δname>` type — skip its alias,
		// or a consumer's generated `global using` names a nonexistent type (go/doc's `ast.Filter`
		// → `go.go.ast_package.ΔFilter`, CS0426).
		for alias, typeName := range exportedTypeAliases {
			// visitFuncType records the RENAMED name (a collision-renamed `Filter` is stored as
			// `ΔFilter`, which is the alias VALUE), while a non-collision methodless func type is
			// stored under its plain name (the alias KEY) — check both.
			if packageInlineFuncTypeNames[alias] || packageInlineFuncTypeNames[typeName] {
				continue
			}

			lines.Add(fmt.Sprintf("[assembly: GoTypeAlias(\"%s\", \"%s\")]", alias, typeName))
		}

		// Publish every purely-anonymous (no Go-level alias) struct/interface this package's own
		// conversion lifted, so a `-tests` reference-model conversion of an internal `_test.go` file
		// (export_test.go's whole reason to exist) can resolve a cross-assembly reference to the
		// SAME anonymous type by its structural signature rather than falling back to the raw Go
		// type text — see GoDynamicTypeLiftAttribute and seedProductionDynamicTypeLifts. The
		// signature is HEX-ENCODED, the same reasoning as dynamicTypeMarker
		// (dynamicTypeOperations.go): a struct signature can carry a field's backtick-quoted Go
		// struct TAG, which can itself carry double quotes and backslashes (`json:"a\"b"`), and hex
		// digits are the one encoding proof against every text transform between here and parsing.
		for signature, typeName := range packageDynamicTypeNames {
			lines.Add(fmt.Sprintf("[assembly: GoDynamicTypeLift(\"%s\", \"%s\")]", hex.EncodeToString([]byte(signature)), typeName))
		}

		// Sort lines
		sortedLines := lines.Keys()
		sort.Strings(sortedLines)

		// Insert exported type aliases into package info file
		packageInfoLines = append(packageInfoLines[:startLineIndex+1],
			append(sortedLines, packageInfoLines[endLineIndex:]...)...)
	} else {
		log.Fatalf("Failed to find '<ExportedTypeAliases>...</ExportedTypeAliases>' section for inserting exported type aliases into package info file \"%s\"\n", packageInfoFileName)
	}

	// Fully-qualified prefix (e.g. `go.@internal.profile_package`) for this package's own types.
	// Used to root a BARE local type reference in the GoImplement/GoImplicitConv assembly
	// attributes when that name collides with a `using System;`-imported type (CS0104) — see
	// qualifySystemCollidingLocalTypeRefs.
	localTypePrefix := packageNamespace + "." + getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))

	// The class a BARE local reference in THIS file binds to. Normally the current package's own
	// class; a merged `-tests` metadata file pins it, because the file's anchor and the variant
	// writing it are not always the same class (see metadataAnchorClassPrefix).
	anchorTypePrefix := localTypePrefix

	if metadataAnchorClassPrefix != "" {
		anchorTypePrefix = metadataAnchorClassPrefix
		// A REFERENCE-model metadata file treats its anchoring test class as local: the Go
		// package's production class is referenced, not compiled locally, so stripping its
		// qualifier would turn a valid production type into a phantom test type. The RECOMPILE
		// model's anchored writes keep the historical production-local qualification — there the
		// production class genuinely is local to the assembly.
		if metadataAnchorLocalTypes {
			localTypePrefix = metadataAnchorClassPrefix
		}
	}

	qualifyLocalTypeRef := func(name string) string {
		// The strip runs AFTER rooting: a record's rendered name arrives WITHOUT the root prefix
		// (`math.rand.rand_package.PCG`), and rootQualifySubNamespaceTypeRefs is what supplies the
		// `go.` the local-class prefixes are expressed with. The ambiguity qualification runs LAST,
		// so a name both `-tests` variant classes declare converges on ONE canonical spelling
		// whichever form the record arrived in (bare from its own variant, class-qualified and
		// then stripped from the other) — the merge HashSet dedupes on that spelling.
		return qualifyAmbiguousTestTypeRefs(qualifySystemCollidingLocalTypeRefs(stripLocalTypeQualifier(rootQualifySubNamespaceTypeRefs(name), localTypePrefix), localTypePrefix), anchorTypePrefix)
	}

	// Handle interface implementations
	startLineIndex = -1
	endLineIndex = -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<InterfaceImplementations>") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</InterfaceImplementations>") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		// Read existing interface lines from package info file
		lines := HashSet[string]{}

		// If processing a single file, instead of all package files, merge interface implementations
		if mergeExisting {
			for i := startLineIndex + 1; i < endLineIndex; i++ {
				line := strings.TrimSpace(packageInfoLines[i])

				// Normalize a merged-in GoImplement record's type references through the SAME
				// canonicalization the fresh render below applies (qualifyLocalTypeRef), so a record
				// persisted by an EARLIER converter run under a now-stale spelling collapses with the
				// fresh record in this HashSet instead of emitting a SECOND [GoImplement] for the same
				// (impl, interface) pair. A NESTED package-under-test's own interface was once emitted
				// FULLY QUALIFIED (`go.container.heap_package.Interface`) but is now canonicalized to the
				// bare local `Interface` by stripLocalTypeQualifier (the math/rand/v2 collapse fix); the
				// -tests external-variant merge read the committed qualified line verbatim, so the two
				// spellings landed as two records → go2cs-gen composed the adapter TWICE
				// (GetUniqueHintName uniquified the second FILE name, so the duplicate TYPE reached the
				// compiler: CS0102 + CS0111 + CS8646 on IntHeapжInterface, container/heap tests). Scoped
				// to GoImplement lines: a GoImplicitConv attribute carries a `ValueType =` keyword that
				// the System-colliding rooter inside qualifyLocalTypeRef would rewrite.
				if strings.HasPrefix(line, "[assembly: GoImplement<") {
					line = qualifyLocalTypeRef(line)
				}

				lines.Add(line)
			}
		}

		// Drop lower level interface implementations where interface inheritances are already covered.
		// POINTER-form (ж<T>-wrapped) pairs are exempt: each generates a DISTINCT IжAdapter class,
		// and cast sites reference the adapter for the EXACT interface they target — a Source-
		// targeted cast needs runtimeSourceᴵSource even though runtimeSourceᴵSource64 also
		// implements Source through interface inheritance (math/rand CS0246). Only the value-boxing
		// partial-struct form (one type, one interface list) is redundant under inheritance.
		for interfaceName, inheritedInterfaces := range interfaceInheritances {
			for _, inheritedInterfaceName := range inheritedInterfaces.Keys() {
				// Check if the same type implements both interfaces
				if inheritedImplementations, ok := interfaceImplementations[inheritedInterfaceName]; ok {
					if baseImplementations, ok := interfaceImplementations[interfaceName]; ok {
						// Intersect on a COPY — IntersectWithSet mutates its receiver, and the receiver
						// here is the DERIVED interface's LIVE implementation set. The old in-place
						// intersect deleted every derived-only implementation (io: nopCloser →
						// ReadCloser shares nothing with Reader's set, so both ReadCloser pairs
						// vanished and the returns failed CS0029) and made the surviving set depend
						// on map iteration order. Only the COMMON implementations are dropped, and
						// only from the LOWER (inherited) interface — C# interface inheritance
						// already covers them via the derived implementation.
						commonImplementations := NewHashSet(baseImplementations.Keys())
						commonImplementations.IntersectWithSet(inheritedImplementations)

						for _, implementation := range commonImplementations.Keys() {
							if strings.HasPrefix(implementation, PointerPrefix+"<") {
								continue
							}

							// A VALUE-form pair that generates its own adapter CLASS (an
							// interface-sourced or foreign-struct conversion — see
							// adapterClassImplementations) is exempt for the same reason as
							// the ж<T> form above: the cast site references the adapter for
							// the EXACT interface it targets (net/http's `new
							// net_ConnᴠWriter(…)` needs GoImplement<net.Conn, io.Writer>
							// even though the Conn→ReadWriteCloser record also implements
							// Writer through inheritance; CS0246 ×17).
							if adapterClassImplementations.Contains(inheritedInterfaceName + "|" + implementation) {
								continue
							}

							inheritedImplementations.Remove(implementation)
						}
					}
				}
			}
		}

		// Add new interface implementations to package info file (hashset ensures uniqueness).
		// A ж<T>-wrapped implementation records a POINTER-sourced cast (`var s Iface = &t`) —
		// unwrap it to `GoImplement<T, Iface>(Pointer = true)`, which generates the IжAdapter
		// wrapper (interface aliases the receiver box) instead of the value-boxing partial.
		//
		// De-duplicate implementations recorded under BOTH a package type ALIAS and the
		// aliased type's qualified name (os converts dirEntry to fs.DirEntry through its own
		// `type DirEntry = fs.DirEntry` AND through the io/fs name): two GoImplement
		// attributes for ONE interface make the generator emit the explicit interface
		// implementation twice (CS8646 ×4 + CS0111 ×4, os dirEntry). The ALIASED record wins
		// — its simple name resolves via the package usings and keeps the generator's
		// last-dot-segment naming; the QUALIFIED duplicate is skipped. (Normalizing the
		// RECORDS to the qualified form instead regressed os 8→77: the qualified interface
		// name broke generator resolution and flipped the alias-locality gate.)
		// Computed after the interface-inheritance prune above, which can drop an alias record's
		// implementation — a stale covered set would leave the qualified duplicate wrongly skipped.
		aliasCoveredImplementations := aliasCoveredImplementationKeys()

		for interfaceName, implementations := range interfaceImplementations {
			// A marker-form key (an anonymous-interface record made from a file visited
			// before the declaring file registered its lift) resolves against the
			// now-complete registry; an unresolvable one is dropped — mirroring the
			// implicit-conversion writer's resolveImplicitConvTypeName skip below.
			interfaceName, okIface := resolveImplicitConvTypeName(interfaceName)

			if !okIface {
				continue
			}

			for implementation := range implementations {
				// Compare on the spelling that will actually be EMITTED, not on the raw registry
				// key. The covered set is built from exportedTypeAliases, whose values visitTypeSpec
				// already canonicalized (it reverts a file-local import rename before recording the
				// alias target), while the registry key keeps whatever rendering the cast site
				// produced — including that rename. os is the reached case: it aliases its `io`
				// import to `Δio` (io is shadowed once io/fs is in the reference closure), so the
				// SAME interface is registered as `DirEntry` through os's own `type DirEntry =
				// fs.DirEntry` and as `Δio.fs_package.DirEntry` through the io/fs name, and neither
				// key compared equal to the canonical `io.fs_package.DirEntry` the covered set
				// holds. Both records were therefore emitted for the ONE pair, ImplementGenerator
				// composed `unixDirentжDirEntry` twice (CS0102 + CS0111 ×9 + CS8646 ×4), and the
				// resulting FALSE collision made adapterNameCollisionSet qualify one cast site to a
				// third spelling, `unixDirentжfs_DirEntry`, that neither record produces (CS0246).
				// qualifyLocalTypeRef is the same canonicalization the emission below applies, so
				// keying on it makes the two sides comparable by construction.
				canonKey := strings.TrimPrefix(qualifyLocalTypeRef(interfaceName), RootNamespace+".") + "|" + implementation

				if aliasCoveredImplementations.Contains(canonKey) {
					continue
				}
				if inner, ok := strings.CutPrefix(implementation, PointerPrefix+"<"); ok {
					lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s>(Pointer = true)]", qualifyLocalTypeRef(strings.TrimSuffix(inner, ">")), qualifyLocalTypeRef(interfaceName)))
					continue
				}

				lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s>]", qualifyLocalTypeRef(implementation), qualifyLocalTypeRef(interfaceName)))
			}
		}

		// Add new promoted interface implementations to package info file (hashset ensures uniqueness)
		for interfaceName, implementations := range promotedInterfaceImplementations {
			for implementation := range implementations {
				lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s>(Promoted = true)]", qualifyLocalTypeRef(implementation), qualifyLocalTypeRef(interfaceName)))
			}
		}

		// Add self-referential constraint proxies (nistCurve[*P224Point]'s Point). Each is a
		// `GoImplement<element, iface<element>>(ConstraintProxy = true)` — the interface's own
		// type argument is a PLACEHOLDER (the generator closes it over the emitted proxy itself),
		// so the element doubles as the dummy. See constraintProxyArg / EmitConstraintProxy.
		for _, proxy := range constraintProxies {
			elementRef := qualifyLocalTypeRef(proxy[0])
			interfaceRef := qualifyLocalTypeRef(proxy[1])

			// The proxy CLASS is emitted per record into the assembly that CARRIES the record, and
			// it is named for the (element, interface) pair alone — so two assemblies holding the
			// same record declare two same-named classes. Under the REFERENCE model the production
			// package is a referenced assembly whose own package_info.cs already carries this
			// record, and the interface reference stays package-QUALIFIED here precisely because
			// the production class is not local (see metadataAnchorLocalTypes above). Re-emitting
			// it mints a second `P224PointжnistPoint` in the test namespace and every use of the
			// name becomes ambiguous — CS0104 ×8 in crypto/ecdsa's test half, the same
			// duplicate-type shadow the reference model exists to eliminate. The production proxy
			// is the single identity; defer to it. A test-DECLARED constraint interface still
			// renders bare (crypto/internal/nistec's own nistPoint) and is emitted here as before.
			if metadataAnchorLocalTypes && strings.Contains(interfaceRef, PackageSuffix+".") {
				continue
			}

			lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s<%s>>(ConstraintProxy = true)]", elementRef, interfaceRef, elementRef))
		}

		// Sort lines
		sortedLines := lines.Keys()
		sort.Strings(sortedLines)

		// Collapse records that differ ONLY by a `global::` root escape — they name the same type
		// and the generator resolves them to the same symbol, so emitting both mints the adapter
		// twice. Applied BEFORE recordEmittedPointerAdapterPairs so the adapter-naming authority
		// below sees the deduplicated set and cannot manufacture a false collision.
		sortedLines = dedupeRootEscapedRecords(sortedLines)

		// These lines ARE the adapter-naming authority: go2cs-gen composes each pointer-adapter
		// class from exactly this pair set, and resolveAdapterNameMarkers resolves the cast sites
		// from the same set, so the two cannot disagree. Captured from the FINAL lines rather than
		// the emission loop above so the MERGED records count too — the -tests variant seeds
		// package_test_info.cs from the production package_info.cs, and those merged pairs own
		// adapter names just as the freshly recorded ones do.
		recordEmittedPointerAdapterPairs(sortedLines)

		// Insert interface implementations into package info file
		packageInfoLines = append(packageInfoLines[:startLineIndex+1],
			append(sortedLines, packageInfoLines[endLineIndex:]...)...)

	} else {
		log.Fatalf("Failed to find '<InterfaceImplementations>...</InterfaceImplementations>' section for inserting interface implementations into package info file \"%s\"\n", packageInfoFileName)
	}

	// Handle implicit conversions
	startLineIndex = -1
	endLineIndex = -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<ImplicitConversions>") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</ImplicitConversions>") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		// Read existing interface lines from package info file
		lines := HashSet[string]{}

		// If processing a single file, instead of all package files, merge implicit conversions
		if mergeExisting {
			for i := startLineIndex + 1; i < endLineIndex; i++ {
				line := packageInfoLines[i]
				lines.Add(strings.TrimSpace(line))
			}
		}

		// A conversion referencing a manually-converted type must not emit — the generated
		// operator would read the skipped auto form's numeric backing; the *_impl.cs declares
		// any conversion operators its call sites need (see manualTypeOperations.go).
		referencesManualType := func(typeNames ...string) bool {
			for _, typeName := range typeNames {
				if packageManualTypeNames[typeName] {
					return true
				}
			}

			return false
		}

		// Add new implicit conversions to package info file (hashset ensures uniqueness)
		for sourceType, targetTypes := range implicitConversions {
			for targetType := range targetTypes {
				if referencesManualType(sourceType, targetType) {
					continue
				}

				source, okSource := resolveImplicitConvTypeName(sourceType)
				target, okTarget := resolveImplicitConvTypeName(targetType)

				if !okSource || !okTarget || source == target {
					continue
				}

				lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>]", qualifyLocalTypeRef(source), qualifyLocalTypeRef(target)))
			}
		}

		// Add new inverted implicit conversions to package info file (hashset ensures uniqueness)
		for sourceType, targetTypes := range invertedImplicitConversions {
			for targetType := range targetTypes {
				if referencesManualType(sourceType, targetType) {
					continue
				}

				source, okSource := resolveImplicitConvTypeName(sourceType)
				target, okTarget := resolveImplicitConvTypeName(targetType)

				if !okSource || !okTarget || source == target {
					continue
				}

				lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Inverted = true)]", qualifyLocalTypeRef(target), qualifyLocalTypeRef(source)))
			}
		}

		// Add new indirect implicit conversions to package info file (hashset ensures uniqueness)
		for sourceType, targetTypes := range indirectImplicitConversions {
			for targetType := range targetTypes {
				if referencesManualType(sourceType, targetType) {
					continue
				}

				source, okSource := resolveImplicitConvTypeName(sourceType)
				target, okTarget := resolveImplicitConvTypeName(targetType)

				if !okSource || !okTarget {
					continue
				}

				lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Indirect = true)]", qualifyLocalTypeRef(source), qualifyLocalTypeRef(target)))
			}
		}

		// Add new numeric conversions to package info file (maps ensure uniqueness)
		for sourceType, targetTypes := range numericConversions {
			for targetType, valueType := range targetTypes {
				if referencesManualType(sourceType, targetType) {
					continue
				}

				var inverted bool

				if strings.HasPrefix(valueType, "imported:") {
					valueType = strings.TrimPrefix(valueType, "imported:")
					inverted = false
				} else {
					inverted = true
				}

				lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Inverted = %t, ValueType = \"%s\")]", qualifyLocalTypeRef(sourceType), qualifyLocalTypeRef(targetType), inverted, valueType))
			}
		}

		// Add new indirect numeric conversions to package info file (maps ensure uniqueness)
		for sourceType, targetTypes := range indirectNumericConversions {
			for targetType, valueType := range targetTypes {
				if referencesManualType(sourceType, targetType) {
					continue
				}

				var inverted bool

				if strings.HasPrefix(valueType, "imported:") {
					valueType = strings.TrimPrefix(valueType, "imported:")
					inverted = false
				} else {
					inverted = true
				}

				lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Inverted = %t, Indirect = true, ValueType = \"%s\")]", qualifyLocalTypeRef(sourceType), qualifyLocalTypeRef(targetType), inverted, valueType))
			}
		}

		// Sort lines
		sortedLines := lines.Keys()
		sort.Strings(sortedLines)

		// Same root-escape collapse the GoImplement section applies — these records are built by
		// the same qualifyLocalTypeRef rendering over the same registries, so they carry the same
		// duplicate-spelling exposure (ImplicitConvGenerator emits one conversion operator per
		// record, and a duplicate pair is CS0557).
		sortedLines = dedupeRootEscapedRecords(sortedLines)

		// Insert implicit conversions into package info file
		packageInfoLines = append(packageInfoLines[:startLineIndex+1],
			append(sortedLines, packageInfoLines[endLineIndex:]...)...)

	} else {
		log.Fatalf("Failed to find '<ImplicitConversions>...</ImplicitConversions>' section for inserting implicit conversions into package info file \"%s\"\n", packageInfoFileName)
	}

	// Handle type accessibility declarations. Unlike the sections above — assembly attributes and
	// `global using` directives at file scope — these are TYPE declarations nested in the package
	// class, so the section lives inside the class body and its entries are indented to match. A
	// file that predates the section (or a -tests seed that composes its own contents) has the
	// prose and markers inserted rather than being rejected.
	packageInfoLines = ensureTypeAccessibilitySection(packageInfoLines)

	startLineIndex = -1
	endLineIndex = -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<"+TypeAccessibilitySection+">") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</"+TypeAccessibilitySection+">") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		lines := HashSet[string]{}

		// Merge the existing declarations for a single-file conversion, and for the -tests files
		// seeded from the production package_info.cs (whose production entries must survive each
		// variant's additions). The stored form is the TRIMMED line — indentation is re-applied at
		// insertion, so a merged entry can never differ from a freshly rendered one by whitespace.
		if mergeExisting {
			for i := startLineIndex + 1; i < endLineIndex; i++ {
				line := strings.TrimSpace(packageInfoLines[i])

				if line != "" {
					lines.Add(line)
				}
			}
		}

		lines.UnionWith(packageEmittedTypeAccess.Keys())

		// Sort lines on the DECLARATION, ignoring any movable-attribute prefix, so an entry that
		// carries attributes keeps the place it would have had without them (typeAccessibilityKey).
		sortedLines := lines.Keys()
		sort.Slice(sortedLines, func(i, j int) bool {
			return typeAccessibilityKey(sortedLines[i]) < typeAccessibilityKey(sortedLines[j])
		})

		indentedLines := make([]string, 0, len(sortedLines))

		for _, line := range sortedLines {
			indentedLines = append(indentedLines, typeAccessibilityIndent+line)
		}

		// Insert type accessibility declarations into package info file
		packageInfoLines = append(packageInfoLines[:startLineIndex+1],
			append(indentedLines, packageInfoLines[endLineIndex:]...)...)
	} else {
		log.Fatalf("Failed to find '<%s>...</%s>' section for inserting type accessibility declarations into package info file \"%s\"\n", TypeAccessibilitySection, TypeAccessibilitySection, packageInfoFileName)
	}

	// The imported-package force hooks. Same class-body placement and the same merge semantics as the
	// section above, and inserted AFTER it so the two machinery blocks have a deterministic order
	// (see ensureImportInitSection). This is where the hooks live since 2026-09-01; before that each
	// was spliced into the class body of the file whose import spec produced it.
	packageInfoLines = ensureImportInitSection(packageInfoLines)
	packageInfoLines = applyImportInitSection(packageInfoLines, mergeExisting)

	// Go source position maps. Same merge semantics as every other section: a whole-package
	// conversion rebuilds the section from this run's records alone, while a merging write (the
	// -tests flow, single-file conversions) keeps existing records for files this conversion did
	// not re-emit -- which is the only route production records have into a recompile-model test
	// assembly, whose compile set excludes package_info.cs in favor of the seeded test-info file.
	packageInfoLines = applyGoSourcePositionMaps(packageInfoLines, packageInfoFileName, mergeExisting)

	// Remove trailing empty lines
	for i := len(packageInfoLines) - 1; i >= 0; i-- {
		if strings.TrimSpace(packageInfoLines[i]) == "" {
			packageInfoLines = packageInfoLines[:i]
		} else {
			break
		}
	}

	// Write updated package info file
	packageInfoFile, err := os.Create(packageInfoFileName)

	if err != nil {
		log.Fatalf("Failed to create package info file \"%s\": %s\n", packageInfoFileName, err)
	}

	defer packageInfoFile.Close()

	for _, line := range packageInfoLines {
		_, err = packageInfoFile.WriteString(line + "\r\n")

		if err != nil {
			log.Fatalf("Failed to write to package info file \"%s\": %s\n", packageInfoFileName, err)
		}
	}
}

// applyGoSourcePositionMaps rewrites the <GoSourcePositionMaps> section of a package info file with
// the records this conversion produced for that file's compilation, creating the section when the
// file predates it.
//
// The section is ALWAYS emitted, populated or not, so its absence never has to be told apart from
// its emptiness -- a package that converted nothing still says so. It is created immediately above
// the namespace declaration, the same place package_info-template.txt carries it, so a migrated file
// and a fresh one are byte-identical.
func applyGoSourcePositionMaps(packageInfoLines []string, packageInfoFileName string, mergeExisting bool) []string {
	startLineIndex := -1
	endLineIndex := -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<GoSourcePositionMaps>") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</GoSourcePositionMaps>") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		section := positionMapSectionLines(packageInfoFileName, packageInfoLines[startLineIndex+1:endLineIndex], mergeExisting)

		updated := make([]string, 0, len(packageInfoLines)+len(section))
		updated = append(updated, packageInfoLines[:startLineIndex]...)
		updated = append(updated, section...)
		updated = append(updated, packageInfoLines[endLineIndex+1:]...)

		return updated
	}

	section := positionMapSectionLines(packageInfoFileName, nil, mergeExisting)

	// Not present: create it above the namespace declaration, prose first.
	namespaceIndex := -1

	for i, line := range packageInfoLines {
		if strings.HasPrefix(strings.TrimSpace(line), "namespace ") {
			namespaceIndex = i
			break
		}
	}

	if namespaceIndex < 0 {
		// No namespace declaration to anchor to. Better to leave the file alone than to guess a
		// position for a section whose records are only meaningful where the compiler can see them.
		showWarning("Package info file \"%s\" has no namespace declaration; its Go source position maps were not emitted", packageInfoFileName)
		return packageInfoLines
	}

	updated := make([]string, 0, len(packageInfoLines)+len(section)+8)
	updated = append(updated, packageInfoLines[:namespaceIndex]...)
	updated = append(updated, goSourcePositionMapsProseLines()...)
	updated = append(updated, "")
	updated = append(updated, section...)
	updated = append(updated, "")
	updated = append(updated, packageInfoLines[namespaceIndex:]...)

	return updated
}
