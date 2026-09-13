// refVerdictPublication.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The cross-package lowering CONTRACT, increment C0 (docs/phase4/DESIGN-zh-box-three-capabilities.md
// §3.2): a converted package PUBLISHES which of its exported pointer-receiver methods carry a
// `[GoRecv] this ref T` primary, as `[assembly: GoRefPrimary("Type", "method")]` records in its
// package_info.cs, and a consuming conversion READS those records from every imported package's
// package_info.cs (or, under -recurse=nuget, from the embedded stdlib-metadata.txt) the way it
// already reads GoImplement records. Phase-A ref-lowering is same-package by design because that
// is the only population whose callers are ALL visible to one package's analysis (§10.1 of
// DESIGN-zh-box-reduction.md); this record replaces that closure argument with a published verdict
// for the cross-package case.
//
// What C0 does NOT do, deliberately: nothing CONSUMES importedRefPrimaries yet — the call-site
// binding is the next increment's (I3), where its emission is measured — so this increment removes
// no box and is scored at ZERO reduction. The section is OMITTED when it would be empty, so a
// conversion with the dual-recv flags off (the corpus default) emits a package_info.cs byte-identical
// to today's; and only a declaration a FOREIGN package can bind is published (an exported method on
// an exported type — an unexported one has no cross-package caller to inform).
//
// Two things this file keeps straight, both measured before it was written. (1) The records derive
// from the converter's OWN selection decision (packageRefReturnPrimaryMethods) and go/types receiver
// kind, never from emitted text: `[GoRecv] … this ref T` is ALSO the emission for every Go VALUE
// receiver on a struct (5,686 such declarations in the corpus at 22d2bd9dc), so C# text cannot tell
// a value receiver from a pointer-receiver primary. (2) A hand-owned file does not dual-emit (B′
// §4.1's XM-1), so a hand-own that declares a primary publishes it BY HAND through
// refPrimaryHandOwns below — and the converter refuses a registered key whose declaration it cannot
// find in the package's hand-owned output, because a record that promises a primary the assembly
// does not declare would fail every consumer's build somewhere else (CS1501/CS1503 at the call site,
// far from the cause). The reverse failure — a primary that exists and is not published — is silent
// and safe (the consumer keeps boxing), which is exactly why the loud check lives on THIS side, in
// the converter suite's TestPublishedRefVerdictsMatchEmitted, rather than at any consumer.

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

	"golang.org/x/tools/go/packages"
)

// refVerdictSectionStart / refVerdictSectionEnd delimit the package_info.cs section that carries
// the published records. Unlike the template's sections this one is NOT present in a fresh file:
// it is inserted only when there is a record to hold and removed when there is none, so a package
// with nothing to publish carries no trace of the contract.
const refVerdictSectionStart = "// <RefVerdicts>"
const refVerdictSectionEnd = "// </RefVerdicts>"

// refPrimaryRecordPrefix is the ONE spelling both the writer and every reader key on: this file's
// parser, and the embedded standard-library metadata generator (internal/stdlibmeta), which keeps
// exactly the record families the converter reads back and must therefore name this prefix too.
const refPrimaryRecordPrefix = "[assembly: GoRefPrimary("

// refVerdictProseLines is the section's explanatory comment, stated the way every emitted-artifact
// comment is: what the section holds and the constraint that shape serves.
func refVerdictProseLines() []string {
	return []string{
		"// An exported pointer-receiver method that carries a `ref`-receiver primary beside its",
		"// pointer-box twin is recorded here as a `GoRefPrimary` attribute, so a package in another",
		"// assembly can bind the primary at a ref-addressable call site instead of allocating a box.",
		"// Both names are the Go spellings. The section exists only while there is a record to hold.",
	}
}

// packageRefPrimaryRecords holds the records the CURRENT package publishes — computed by
// collectPublishedRefVerdicts after the capture-mode selection has run, consumed by
// writePackageInfoFile. Reset with the other package-scoped globals.
var packageRefPrimaryRecords []string

// importedRefPrimaries records `[assembly: GoRefPrimary("T", "m")]` lines parsed from IMPORTED
// packages' package_info files (and from the embedded stdlib metadata), keyed by
// refPrimaryRecordKey — the existence proof that the foreign assembly declares a `ref` primary for
// the method. Populated by loadPackageImplementLines; nothing consumes it at C0 (see the file
// comment). Reset with its siblings.
var importedRefPrimaries HashSet[string]

// refPrimaryHandOwns is the curated registry of primaries that HAND-OWNED files declare, keyed
// "<pkgPath>.<Type>.<method>" (the refLoweringHandOwnCallers shape): a hand-own does not dual-emit,
// so its primary is published only through an entry here — and only after the converter has found
// the declaration in the package's hand-owned output (refPrimaryHandOwnDeclared). EMPTY at C0 by
// design: registering the `ref` forms hand-owns already declare (sync/atomic, sync.Pool, …) would
// let a consumer bind them and REMOVE boxes, which is a reduction, and C0 is scored at zero. Each
// later increment registers what it needs and names the registration as part of its footprint.
var refPrimaryHandOwns = map[string]bool{
	// I3's registrations, and the registry's FIRST consumers. sync/mutex.cs is a whole-file
	// hand-own whose three public methods do nothing but reach `gateOf`, which itself only ever
	// used its box to produce a `ref Mutex` — so declaring them `[GoRecv] … this ref Mutex` was a
	// signature change, not a rewrite, and go2cs-gen supplies the ж overload that every existing
	// call site keeps binding unchanged (measured: sync and internal/poll both build 0 errors with
	// the box form still in the emission).
	//
	// Publishing them is what lets a CONSUMER bind `fd.l.Lock()` — a ref-addressable field of a ref
	// lvalue the caller already holds — instead of minting `Ꮡfd.of(FD.Ꮡl)` for the receiver.
	// sync.Mutex is the corpus's most-used lock, so this registration is what gives I3 its measured
	// 667-site reach; the call-site rule that consumes it is the increment's other half.
	"sync.Mutex.Lock":    true,
	"sync.Mutex.TryLock": true,
	"sync.Mutex.Unlock":  true,

	// I1's registrations — SAME-PACKAGE, and the reason the registry serves both cases. These
	// three are (b′)'s hand-owns; they already declare `this ref fdMutex` and C0's declaring-side
	// guard already verifies them. Nothing is PUBLISHED for them and nothing should be:
	// `fdMutex` is unexported, so no foreign assembly could bind it — and registeredHandOwnPrimaries
	// now enforces that on the publication side, which it did NOT before this increment (I1's own
	// A/B caught three stray records here). What the entries buy is the caller inside internal/poll
	// binding
	// `fd.fdmu.rwlock(true)` instead of minting `Ꮡfd.of(FD.Ꮡfdmu)` for the receiver.
	//
	// `incref`/`decref` are deliberately NOT here: they are still auto-converted and their bodies
	// form `Ꮡmu.of(fdMutex.Ꮡstate)`, so they carry no `ref` primary to bind. Registering a key the
	// assembly does not declare is exactly what C0's guard refuses, and rightly.
	"internal/poll.fdMutex.increfAndClose": true,
	"internal/poll.fdMutex.rwlock":         true,
	"internal/poll.fdMutex.rwunlock":       true,
}

// refPrimaryRecordKey composes the ONE spelling of a published primary that both sides of the
// lookup compute — the LOAD side reading a dependency's records, and the USE side (I3) asking
// whether an imported method carries a primary. Keyed by the declaring package's NAME like the
// implement-record sets, so the two families are looked up the same way.
func refPrimaryRecordKey(declaringPackageName string, typeName string, methodName string) string {
	return declaringPackageName + "|" + typeName + "|" + methodName
}

// refPrimaryHandOwnKey composes the registry key for a hand-declared primary.
func refPrimaryHandOwnKey(pkgPath string, typeName string, methodName string) string {
	return refCanonicalPkgPath(pkgPath) + "." + typeName + "." + methodName
}

// formatRefPrimaryRecord renders one record line.
func formatRefPrimaryRecord(typeName string, methodName string) string {
	return fmt.Sprintf("%s%q, %q)]", refPrimaryRecordPrefix, typeName, methodName)
}

// refPrimaryRecordPattern parses a record line back into its two names; a line that does not match
// is simply not a record (the parsers-cannot-fail rule the implement-record readers follow).
var refPrimaryRecordPattern = regexp.MustCompile(`^\[assembly: GoRefPrimary\("([^"]+)", "([^"]+)"\)\]$`)

// parseRefPrimaryLines returns the (Type, method) pairs recorded on the given package-info lines.
func parseRefPrimaryLines(lines []string) [][2]string {
	var pairs [][2]string

	for _, line := range lines {
		matches := refPrimaryRecordPattern.FindStringSubmatch(strings.TrimSpace(line))

		if matches == nil {
			continue
		}

		pairs = append(pairs, [2]string{matches[1], matches[2]})
	}

	return pairs
}

// publishableRefPrimaries returns the (Type, method) pairs of the CURRENT package's selected
// primaries that a foreign package can bind: the method is exported, its receiver's named type is
// exported, and both are declared in this package. This is the decision-side set the guard compares
// the published records against — computed from the selection map and go/types, never from text.
func publishableRefPrimaries(pkgTypes *types.Package) [][2]string {
	var pairs [][2]string

	for fn := range packageRefReturnPrimaryMethods {
		if fn == nil || fn.Pkg() == nil || pkgTypes == nil || fn.Pkg() != pkgTypes || !fn.Exported() {
			continue
		}

		signature, ok := fn.Type().(*types.Signature)

		if !ok || signature.Recv() == nil {
			continue
		}

		recvType := types.Unalias(signature.Recv().Type())

		pointer, isPointer := recvType.(*types.Pointer)

		if !isPointer {
			continue // a primary is a pointer-receiver method by construction; stated, not assumed
		}

		named, isNamed := types.Unalias(pointer.Elem()).(*types.Named)

		if !isNamed || named.Obj() == nil || !named.Obj().Exported() || named.Obj().Pkg() != pkgTypes {
			continue
		}

		pairs = append(pairs, [2]string{named.Obj().Name(), fn.Name()})
	}

	sort.Slice(pairs, func(i, j int) bool {
		if pairs[i][0] != pairs[j][0] {
			return pairs[i][0] < pairs[j][0]
		}

		return pairs[i][1] < pairs[j][1]
	})

	return pairs
}

// registeredHandOwnPrimaries returns the (Type, method) pairs refPrimaryHandOwns registers for the
// current package, in key order.
func registeredHandOwnPrimaries(pkgPath string) [][2]string {
	prefix := refCanonicalPkgPath(pkgPath) + "."

	var pairs [][2]string

	keys := make([]string, 0, len(refPrimaryHandOwns))

	for key := range refPrimaryHandOwns {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	for _, key := range keys {
		rest, ok := strings.CutPrefix(key, prefix)

		if !ok {
			continue
		}

		typeName, methodName, found := strings.Cut(rest, ".")

		if !found || typeName == "" || methodName == "" || strings.Contains(methodName, ".") {
			continue
		}

		// PUBLISH only what a FOREIGN assembly could bind — the same bar publishableRefPrimaries
		// applies to the converter's own selections, and for the same reason: a record is an
		// existence proof offered to other packages, and an unexported type or method is invisible
		// to every one of them. Without this, I1's same-package registrations (internal/poll's
		// `fdMutex`, unexported) emitted three records into package_info.cs and from there into
		// stdlib-metadata.txt that no consumer could ever key on — inert, but a promise made to
		// nobody. The registry serves BOTH roles now (same-package binding, cross-package
		// publication) and only the second one is gated: registering an unexported primary is
		// correct and useful, publishing it is not. Measured by I1's own two-seeded A/B, which is
		// where the three stray records surfaced.
		if !token.IsExported(typeName) || !token.IsExported(methodName) {
			continue
		}

		pairs = append(pairs, [2]string{typeName, methodName})
	}

	return pairs
}

// refPrimaryDeclarationPattern matches the C# declaration a hand-own must carry for a registered
// primary: `<method>(this ref <Type>` — the `[GoRecv] this ref T` primary shape, whatever the
// return type and whatever follows the receiver.
func refPrimaryDeclarationPattern(typeName string, methodName string) *regexp.Regexp {
	return regexp.MustCompile(`\b` + regexp.QuoteMeta(methodName) + `\(this ref ` + regexp.QuoteMeta(typeName) + `\b`)
}

// refPrimaryHandOwnDeclared reports whether a hand-owned file in the package's OUTPUT directory —
// a `.cs` carrying the [module: GoManualConversion] marker, or a `*_impl.cs` companion — declares
// the registered primary. The probe reads the same files the driver protects from regeneration,
// so a registration can only ever describe a declaration that will still be there after the next
// reconvert.
func refPrimaryHandOwnDeclared(packageOutputPath string, typeName string, methodName string) (bool, error) {
	entries, err := os.ReadDir(packageOutputPath)

	if err != nil {
		if os.IsNotExist(err) {
			return false, nil
		}

		return false, err
	}

	pattern := refPrimaryDeclarationPattern(typeName, methodName)

	for _, entry := range entries {
		name := entry.Name()

		if entry.IsDir() || !strings.HasSuffix(name, ".cs") {
			continue
		}

		path := filepath.Join(packageOutputPath, name)
		handOwned := strings.HasSuffix(name, "_impl.cs")

		if !handOwned {
			marked, markerErr := containsManualConversionMarker(path)

			if markerErr != nil {
				return false, markerErr
			}

			handOwned = marked
		}

		if !handOwned {
			continue
		}

		contents, readErr := os.ReadFile(path)

		if readErr != nil {
			return false, readErr
		}

		if pattern.Match(contents) {
			return true, nil
		}
	}

	return false, nil
}

// collectPublishedRefVerdicts computes the records the current package publishes — its selected
// exported primaries plus the hand-own registry's entries for it — into packageRefPrimaryRecords,
// refusing a registered hand-own key whose declaration is not on disk. Runs after the capture-mode
// selection and before writePackageInfoFile.
func collectPublishedRefVerdicts(pkg *packages.Package, packageOutputPath string) error {
	packageRefPrimaryRecords = nil

	if pkg == nil {
		return nil
	}

	lines := HashSet[string]{}

	for _, pair := range publishableRefPrimaries(pkg.Types) {
		lines.Add(formatRefPrimaryRecord(pair[0], pair[1]))
	}

	for _, pair := range registeredHandOwnPrimaries(pkg.PkgPath) {
		declared, err := refPrimaryHandOwnDeclared(packageOutputPath, pair[0], pair[1])

		if err != nil {
			return fmt.Errorf("checking the hand-owned primary %s: %w", refPrimaryHandOwnKey(pkg.PkgPath, pair[0], pair[1]), err)
		}

		if !declared {
			return fmt.Errorf("refPrimaryHandOwns registers %s, but no hand-owned file under %q declares `%s(this ref %s …)` — a published primary the assembly does not declare would break every consumer's build at its call sites; declare it in the hand-own or remove the registration",
				refPrimaryHandOwnKey(pkg.PkgPath, pair[0], pair[1]), packageOutputPath, pair[1], pair[0])
		}

		lines.Add(formatRefPrimaryRecord(pair[0], pair[1]))
	}

	sorted := lines.Keys()
	sort.Strings(sorted)
	packageRefPrimaryRecords = sorted

	return nil
}

// applyRefVerdictSection rewrites a package-info file's <RefVerdicts> section from the given
// records: any existing section (its prose, markers and records, and the blank line that separates
// it from what follows) is removed, existing records are merged in when mergeExisting is set (the
// single-file and -tests paths), and the section is re-inserted — before the ImplicitConversions
// section, or before the namespace declaration when a file predates that marker — ONLY when the
// merged set is non-empty. An empty set leaves no trace, which is what keeps a flags-off
// conversion byte-identical to a file written before the contract existed.
func applyRefVerdictSection(packageInfoLines []string, records []string, mergeExisting bool) []string {
	merged := HashSet[string]{}

	for _, record := range records {
		merged.Add(record)
	}

	startIndex, endIndex := -1, -1

	for i, line := range packageInfoLines {
		trimmed := strings.TrimSpace(line)

		if trimmed == refVerdictSectionStart {
			startIndex = i
			continue
		}

		if trimmed == refVerdictSectionEnd && startIndex >= 0 {
			endIndex = i
			break
		}
	}

	if startIndex >= 0 && endIndex > startIndex {
		if mergeExisting {
			for _, line := range packageInfoLines[startIndex+1 : endIndex] {
				if parsed := parseRefPrimaryLines([]string{line}); len(parsed) == 1 {
					merged.Add(strings.TrimSpace(line))
				}
			}
		}

		// Remove the block: the prose lines immediately above the start marker, the section, and
		// the one blank line after the end marker that separated it from the next section.
		removeFrom := startIndex
		prose := refVerdictProseLines()

		for len(prose) > 0 && removeFrom > 0 && strings.TrimSpace(packageInfoLines[removeFrom-1]) == prose[len(prose)-1] {
			removeFrom--
			prose = prose[:len(prose)-1]
		}

		removeTo := endIndex + 1

		if removeTo < len(packageInfoLines) && strings.TrimSpace(packageInfoLines[removeTo]) == "" {
			removeTo++
		}

		trimmed := make([]string, 0, len(packageInfoLines))
		trimmed = append(trimmed, packageInfoLines[:removeFrom]...)
		trimmed = append(trimmed, packageInfoLines[removeTo:]...)
		packageInfoLines = trimmed
	}

	if len(merged) == 0 {
		return packageInfoLines
	}

	sorted := merged.Keys()
	sort.Strings(sorted)

	insertAt := -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<ImplicitConversions>") {
			insertAt = i
			break
		}
	}

	if insertAt < 0 {
		for i, line := range packageInfoLines {
			if strings.HasPrefix(strings.TrimSpace(line), "namespace ") {
				insertAt = i
				break
			}
		}
	}

	if insertAt < 0 {
		insertAt = len(packageInfoLines)
	}

	section := make([]string, 0, len(sorted)+len(refVerdictProseLines())+3)
	section = append(section, refVerdictProseLines()...)
	section = append(section, refVerdictSectionStart)
	section = append(section, sorted...)
	section = append(section, refVerdictSectionEnd, "")

	updated := make([]string, 0, len(packageInfoLines)+len(section))
	updated = append(updated, packageInfoLines[:insertAt]...)
	updated = append(updated, section...)
	updated = append(updated, packageInfoLines[insertAt:]...)

	return updated
}

// loadRefPrimaryLines records a converted package's published primaries from an already-read
// package-info line set into importedRefPrimaries — the third record family
// loadPackageImplementLines carries, alongside the pointer- and value-form implement pairs.
func loadRefPrimaryLines(lines []string, rootPackageName string) {
	pairs := parseRefPrimaryLines(lines)

	if len(pairs) == 0 {
		return
	}

	packageLock.Lock()

	for _, pair := range pairs {
		importedRefPrimaries.Add(refPrimaryRecordKey(rootPackageName, pair[0], pair[1]))
	}

	packageLock.Unlock()
}

// calleePublishesRefPrimary reports whether the method a selector calls is one an IMPORTED package
// published a `ref` primary for — the CONSUMER half of the contract C0 left deliberately unbuilt.
//
// This is I3's whole gate on the callee side. A published record is an existence proof that the
// foreign assembly declares `M(this ref T …)`, so a caller holding a ref-addressable receiver may
// bind the plain member chain (`fd.l.Lock()`) instead of minting a field-address box for the
// receiver (`Ꮡfd.of(FD.Ꮡl).Lock()`). Without the record the box stays: an unpublished primary may
// simply not exist in the other assembly, and binding it would be CS1929 at the call site.
//
// The key is composed exactly as the LOAD side composes it (refPrimaryRecordKey over the declaring
// package's NAME), so the two cannot drift apart — the same single-spelling discipline the record
// key was introduced for.
func (v *Visitor) calleePublishesRefPrimary(selectorExpr *ast.SelectorExpr) bool {
	funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func)

	if !ok || funcObj.Pkg() == nil {
		return false
	}

	signature, ok := funcObj.Type().(*types.Signature)

	if !ok || signature.Recv() == nil || types.IsInterface(signature.Recv().Type()) {
		return false
	}

	pointer, isPointer := types.Unalias(signature.Recv().Type()).(*types.Pointer)

	if !isPointer {
		return false // a primary is a pointer-receiver method by construction
	}

	named, isNamed := types.Unalias(pointer.Elem()).(*types.Named)

	if !isNamed || named.Obj() == nil {
		return false
	}

	// SAME-PACKAGE (I1) — ask the hand-own REGISTRY, not the published records. A published
	// record is an existence proof about ANOTHER assembly; within one assembly the declaration is
	// right here, so nothing needs publishing and nothing is minted. That is also why
	// publishableRefPrimaries refuses these: `fdMutex.rwlock` is unexported, and an unexported
	// method has no cross-package caller to inform. Registry only — the converter's own Phase-A
	// selection is a separate population and is not consulted here.
	if v.pkg != nil && funcObj.Pkg() == v.pkg {
		handOwnKey := refPrimaryHandOwnKey(funcObj.Pkg().Path(), named.Obj().Name(), funcObj.Name())

		packageLock.Lock()
		declared := refPrimaryHandOwns[handOwnKey]
		packageLock.Unlock()

		return declared
	}

	key := refPrimaryRecordKey(funcObj.Pkg().Name(), named.Obj().Name(), funcObj.Name())

	packageLock.Lock()
	published := importedRefPrimaries.Contains(key)
	packageLock.Unlock()

	return published
}
