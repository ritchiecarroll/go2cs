// importInitSection.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The package_info.cs home of the imported-package force hooks.
//
// Until 2026-09-01 each hook was spliced into the class body of the FILE whose import spec produced
// it, at the top, ahead of that file's own `init` functions. That placed a five-line machinery block
// in every importing file — 2,125 of them across 684 production files at the time of the move — in
// code whose whole point is that a Go developer can read it and recognize their own program.
//
// The hooks are collected per emission unit instead (packageImportInits) and written here as a
// one-line manifest. The ordering contract comes out STRONGER, not merely preserved: the per-file
// marker could only put a hook ahead of the inits of its own file, and Roslyn orders module
// initializers by compilation file order, which for a `<Compile Include="*.cs" />` glob is
// filesystem order that nobody curates. package_info.cs is the FIRST compile item of every generated
// project (the ordering precondition landed separately, so it could be gated on its own), so ALL of
// a package's import hooks now precede ALL of its own `init` functions, deterministically.
//
// Design record: docs/phase4/DESIGN-import-hook-relocation.md. Positive control:
// tests/Behavioral/NamedImportInitOrder, which is a real one here rather than a formality — the
// naive move, without the compile-ordering change, reintroduces log/slog's nil-deref.

package main

import (
	"fmt"
	"sort"
	"strings"
)

// ImportInitSection names the package_info.cs marker section carrying the force hooks.
const ImportInitSection = "ImportInitializers"

// importInitIndent matches typeAccessibilityIndent, and for the same reason: these are METHOD
// declarations nested in the package class, not file-scoped attributes, so the section lives inside
// the class body.
const importInitIndent = "    "

// importInitProseLines returns the section's explanatory comment — DESCRIPTIVE, like the type
// accessibility block's, because this text persists in every package_info.cs. One comment for the
// whole section rather than one per hook: the hooks are a manifest of forced imports, and repeating
// the same four lines once per entry is what made them worth moving out of the sources.
func importInitProseLines() []string {
	return []string{
		importInitIndent + "// Go initializes an imported package before the importing package, for every import",
		importInitIndent + "// form - not only the blank one. .NET would never load an assembly nothing has touched",
		importInitIndent + "// yet, so each import that initializes anything is forced below: once per assembly, and",
		importInitIndent + "// ahead of this package's own `init` functions, which this file being the first compile",
		importInitIndent + "// item of the project guarantees.",
	}
}

// importInitSectionLines returns the section's prose and marker delimiters, in the style of the
// TypeAccessibility block it is inserted after. This is the ONLY definition of the block: it is
// inserted on demand rather than carried in package_info-template.txt, so a template-generated file
// and one that predates the section end up byte-identical.
func importInitSectionLines() []string {
	return append(importInitProseLines(),
		"",
		importInitIndent+"// <"+ImportInitSection+">",
		importInitIndent+"// </"+ImportInitSection+">",
	)
}

// importInitLine renders one hook. A single expression-bodied method per forced import, so the
// section reads as a manifest and so each entry is ONE line — which is what lets the merge below
// dedupe and sort entries the same way the type-accessibility section does, rather than needing to
// reason about multi-line blocks.
func importInitLine(importPath string, forcingTarget string) string {
	return fmt.Sprintf("[GoInit] internal static void %s() => builtin.initPackage(typeof(%s));", importInitName(importPath), forcingTarget)
}

// ensureImportInitSection returns packageInfoLines with the ImportInitializers prose and marker
// section present, inserted immediately AFTER the TypeAccessibility section's closing marker.
//
// Anchoring on that section rather than on the class-body top is what makes the position
// deterministic: both sections belong in the class body, and two independent "insert at the top of
// the class" helpers would order themselves by which one happened to run first. The caller
// guarantees the anchor exists — writePackageInfoFile calls ensureTypeAccessibilitySection before
// this — and a file that somehow lacks it falls back to the class-body top rather than dropping the
// section, because a missing hook is an unforced `init` and this file is not the place to decide
// that is acceptable.
func ensureImportInitSection(packageInfoLines []string) []string {
	openTag := "<" + ImportInitSection + ">"

	for _, line := range packageInfoLines {
		if strings.Contains(line, openTag) {
			return packageInfoLines
		}
	}

	insertIndex := -1
	closeTypeAccess := "</" + TypeAccessibilitySection + ">"

	for i, line := range packageInfoLines {
		if strings.Contains(line, closeTypeAccess) {
			insertIndex = i + 1
			break
		}
	}

	if insertIndex < 0 {
		insertIndex = classBodyInsertIndex(packageInfoLines)
	}

	if insertIndex < 0 {
		return packageInfoLines
	}

	block := append([]string{""}, importInitSectionLines()...)

	return append(packageInfoLines[:insertIndex], append(block, packageInfoLines[insertIndex:]...)...)
}

// applyImportInitSection fills the section from packageImportInits, with the same merge semantics
// every other section here has: a whole-package conversion rebuilds the section from this run's
// hooks alone, while a merging write (the -tests seed, a single-file conversion) keeps entries this
// pass did not re-emit. Entries are stored trimmed and re-indented at insertion, so a merged entry
// can never differ from a freshly rendered one by whitespace.
//
// Sorted on the RENDERED LINE, which for these entries means the emitted hook name. Sorting on the
// import path would read better — `fmt` before `NamedImportInitOrder/reader` rather than after it,
// since the emitted name capitalizes what the path does not — but a merging write carries entries
// this pass did not render and therefore has no import path for, and one order for both halves is
// worth more than a prettier order for one. Order carries no semantics either way: each hook forces
// exactly ONE assembly, whose own module constructor runs that assembly's hooks first, so Go's
// transitive ordering is reproduced by the forcing itself rather than by the order of these calls.
func applyImportInitSection(packageInfoLines []string, mergeExisting bool) []string {
	startLineIndex := -1
	endLineIndex := -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<"+ImportInitSection+">") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</"+ImportInitSection+">") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex < 0 || endLineIndex < 0 || startLineIndex >= endLineIndex {
		return packageInfoLines
	}

	// Keyed by the hook's METHOD NAME — the import's identity — and NOT by the rendered line. Two
	// renderings of the same hook differ legitimately: the forcing target is root-qualified or bare
	// depending on the emission unit that decided it (forcingTargetShadowed keys on the class the
	// hook is written into). A merging write therefore meets the SAME method under TWO spellings,
	// and a line-keyed set keeps both — two methods of one name in one partial class, which is
	// CS0111. Measured on crypto/x509, whose `math/big` reaches the test variants by more than one
	// route:
	//
	//	[GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
	//	[GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
	//
	// The FRESH entry wins: it is this emission unit's own decision, computed against the class this
	// file declares, where a seeded line was decided for a different one. Identity comes from
	// importInitName, a pure function of the import path, so the two halves cannot disagree on what
	// "the same hook" means.
	entries := map[string]string{}

	if mergeExisting {
		for i := startLineIndex + 1; i < endLineIndex; i++ {
			line := strings.TrimSpace(packageInfoLines[i])

			if line == "" {
				continue
			}

			if name := importInitLineName(line); name != "" {
				entries[name] = line
			}
		}
	}

	for importPath, forcingTarget := range packageImportInits {
		entries[importInitName(importPath)] = importInitLine(importPath, forcingTarget)
	}

	merged := make([]string, 0, len(entries))

	for _, line := range entries {
		merged = append(merged, line)
	}

	sort.Strings(merged)

	indented := make([]string, 0, len(merged))

	for _, line := range merged {
		indented = append(indented, importInitIndent+line)
	}

	return append(packageInfoLines[:startLineIndex+1],
		append(indented, packageInfoLines[endLineIndex:]...)...)
}

// importInitLineName extracts a rendered hook line's METHOD NAME — the import's identity, and the
// only part of the line that is decided by the import rather than by the emission unit. It is what
// lets a merging write recognize a seeded entry and this pass's entry as the SAME hook when their
// forcing targets are spelled differently (see applyImportInitSection).
//
// Deliberately narrow: it matches only the shape importInitLine renders, and answers "" for anything
// else, so a line this file did not write is carried through the merge untouched rather than being
// silently reinterpreted.
func importInitLineName(line string) string {
	const lead = "[GoInit] internal static void "

	if !strings.HasPrefix(line, lead) {
		return ""
	}

	rest := line[len(lead):]
	open := strings.Index(rest, "(")

	if open <= 0 {
		return ""
	}

	return rest[:open]
}
