// platformHandOwn.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns layout L3's HAND-OWNED-FILE routing — increment 3.5 of
// docs/phase4/DESIGN-multiplatform-corpus.md. platformLayout.go owns what L3 is; platformEmit.go owns
// the merge that produces it; this is the one class of file that merge cannot classify on its own.
//
// **Why a hand-owned file needs its own rule.** L3's classifier compares EMISSIONS (design §4.2, and
// the reason increment 1 exists): an artifact every target wrote identically is shared and stays
// flat, anything else goes to `<pkg>/<goos>/`. A hand-owned file is never emitted, so it is never
// classified, so it stays flat — while the file it belongs to moves. Measured, that is not a
// theoretical seam: `runtime/lock_sema_impl.cs` supplements `lock_sema.cs`, which Go selects on
// Windows *and* macOS but never on Linux, so increment 3's Linux build compiled a flat
// `lock_sema_impl.cs` against a `lock_sema.cs` that was not in its build — 6× CS0103 plus 2× CS7036,
// the ENTIRE Linux corpus failure, 249 packages skipped behind it. Design §7 predicted the sibling;
// what it could not predict was that the layout would not even route the file.
//
// **The rule, and why it is not literally "same folder as the principal".** A hand-owned file belongs
// in exactly the platform builds its PRINCIPAL takes part in — and then L3's own placement rule
// applies to that set unchanged: every platform ⟹ flat, a subset ⟹ one copy per platform in the
// subset. Reading it that way rather than as "wherever the principal's bytes happen to live" is what
// keeps `os/proc_impl.cs` and `syscall/syscall_impl.cs` flat: their principals are per-GOOS VARIANTS
// present on all three platforms, so a literal folder-inheritance would triplicate one hand-written
// file into three copies that must then be hand-maintained in lockstep, for no compile benefit. The
// set reading gives the same answer as the folder reading everywhere it matters and a better one
// here, because it is L3's actual invariant: a file is duplicated only when it cannot be shared.
//
// **What a principal is.** Two shapes of hand-own, two bindings, and both are already recorded in the
// tree by the emission itself:
//
//	<name>_impl.cs   a COMPANION that supplements the converted `<name>.cs` (no Go counterpart of its
//	                 own, so the conversion never touches it) — principal `<pkg>/<name>.cs`
//	<name>.cs        a marked whole-file hand-own ([module: GoManualConversion]); the conversion still
//	                 runs and only its EMISSION diverts, to the non-compiled `<name>.cs.auto` review
//	                 sibling — principal `<pkg>/<name>.cs.auto`, which is emitted by exactly the
//	                 platforms whose build compiles the Go file this hand-own replaces
//
// The second binding is what catches `syscall/dll_windows.cs` and `syscall/exec_windows.cs`: whole-file
// hand-owns of Windows-only Go files, flat in the corpus and therefore latently in every Linux build —
// a break the increment-3 measurement never reached, because it stopped at `runtime` two layers below.
//
// A hand-own whose principal no target emitted (`runtime/managed_impl.cs`, `internal/poll/runtime_sema_impl.cs`
// — go2cs runtime machinery with no Go file behind it at all) has no placement evidence and is left
// exactly where it is. Silence is the correct answer there, not a guess.
//
// The whole step is confined to packages that already carry per-GOOS sources, it moves bytes it never
// rewrites, and it refuses rather than choosing when two existing copies of one hand-own disagree.

package main

import (
	"bytes"
	"fmt"
	"os"
	"path"
	"path/filepath"
	"strings"
)

// handOwnPlacement is one hand-owned artifact's resolved routing.
type handOwnPlacement struct {
	logical      string   // `<pkg>/<file>.cs`, per-GOOS folder segment folded away
	principal    string   // the emitted artifact whose platform set decides the placement
	destinations []string // absolute directories this file belongs in
	candidates   []string // absolute directories it could be in — destinations plus the ones to clear
}

// platformHandOwnPrincipals returns the logical paths of the emitted artifact a hand-owned `.cs`
// stands in for, most-definitive first. See the file header for the two shapes.
func platformHandOwnPrincipals(logical string) []string {
	dir, base := path.Split(logical)
	principals := []string{dir + base + ".auto"}

	if stem := strings.TrimSuffix(base, ".cs"); strings.HasSuffix(stem, "_impl") {
		principals = append(principals, dir+strings.TrimSuffix(stem, "_impl")+".cs")
	}

	return principals
}

// platformHandOwnDestinations applies L3's placement rule to a principal's emitter set: shared by
// every target means one flat copy, anything less means one copy per emitting target's GOOS folder.
func platformHandOwnDestinations(coreDir string, pkg string, targets []string, emitters []string) []string {
	packageDir := filepath.Join(coreDir, filepath.FromSlash(pkg))

	if len(emitters) == len(targets) {
		return []string{packageDir}
	}

	destinations := make([]string, 0, len(emitters))

	for _, target := range emitters {
		destinations = append(destinations, filepath.Join(packageDir, goosOfTarget(target)))
	}

	return destinations
}

// mergeHandOwnedArtifacts routes the corpus's hand-owned `.cs` files — and the `.cs.auto` review
// siblings that belong beside them — into the folders layout L3 puts their principals' platform sets
// in. It runs after the emitted artifacts have landed, so `packageCarriesPlatformLayout` reads the
// finished layout rather than the one the merge started from.
//
// Returns the packages it touched, and how many files it wrote and removed.
func mergeHandOwnedArtifacts(coreDir string, targets []string, emissions []*platformEmission) ([]string, int, int, error) {
	emitters := platformEmitterSets(targets, emissions)
	written, removed := 0, 0
	touched := map[string]bool{}

	for _, placement := range resolveHandOwnPlacements(coreDir, targets, emissions, emitters) {
		fileWrites, fileRemovals, err := applyHandOwnPlacement(placement)

		if err != nil {
			return nil, written, removed, err
		}

		written += fileWrites
		removed += fileRemovals

		if fileWrites > 0 || fileRemovals > 0 {
			touched[artifactPackage(placement.logical)] = true
		}
	}

	return sortedKeys(touched), written, removed, nil
}

// platformEmitterSets maps every artifact any target EMITTED — of any extension, so `.cs.auto` review
// siblings are in here alongside `.cs` — to the targets that emitted it, in -platforms order.
func platformEmitterSets(targets []string, emissions []*platformEmission) map[string][]string {
	emitters := map[string][]string{}

	for i, target := range targets {
		for rawPath, state := range emissions[i].artifacts {
			if state.emitted {
				logical := state.logicalPath(rawPath)
				emitters[logical] = append(emitters[logical], target)
			}
		}
	}

	// The map above is built in target order, but a map's iteration is not, so an artifact two
	// targets emitted could have collected them either way round. Re-derive the order from `targets`.
	for logical, list := range emitters {
		if len(list) < 2 {
			continue
		}

		set := map[string]bool{}

		for _, target := range list {
			set[target] = true
		}

		ordered := make([]string, 0, len(list))

		for _, target := range targets {
			if set[target] {
				ordered = append(ordered, target)
			}
		}

		emitters[logical] = ordered
	}

	return emitters
}

// resolveHandOwnPlacements finds every hand-owned `.cs` in an L3 package and resolves where it
// belongs. A hand-owned file is one the corpus holds that NO target emitted — which is exactly the
// converter's own reading, since a marked file's emission diverts to `.cs.auto` and an `*_impl.cs`
// has no Go counterpart to emit from.
func resolveHandOwnPlacements(coreDir string, targets []string, emissions []*platformEmission, emitters map[string][]string) []handOwnPlacement {
	var placements []handOwnPlacement
	seen := map[string]bool{}

	// Every staging root was seeded from the same corpus, so any emission's artifact map is a census
	// of it; the first is used for determinism.
	for _, rawPath := range sortedKeys(func() map[string]bool {
		keys := map[string]bool{}

		for rawPath := range emissions[0].artifacts {
			keys[rawPath] = true
		}

		return keys
	}()) {
		if path.Ext(rawPath) != ".cs" {
			continue
		}

		logical := emissions[0].artifacts[rawPath].logicalPath(rawPath)

		if seen[logical] || len(emitters[logical]) > 0 {
			continue
		}

		seen[logical] = true
		pkg := artifactPackage(logical)

		// Confined to packages that already carry per-GOOS sources: everywhere else "flat" is the
		// only placement there is, and asking the question would only risk answering it.
		if pkg == "." || !packageCarriesPlatformLayout(filepath.Join(coreDir, filepath.FromSlash(pkg))) {
			continue
		}

		for _, principal := range platformHandOwnPrincipals(logical) {
			principalEmitters, ok := emitters[principal]

			if !ok {
				continue
			}

			placements = append(placements, handOwnPlacement{
				logical:      logical,
				principal:    principal,
				destinations: platformHandOwnDestinations(coreDir, pkg, targets, handOwnEmitters(principal, targets, emissions, principalEmitters)),
				candidates:   handOwnCandidateDirs(coreDir, pkg, targets),
			})

			break
		}
	}

	return placements
}

// handOwnCandidateDirs lists every directory a package's hand-owned file could be sitting in — flat,
// or any target's GOOS folder. The merge writes to the destinations and clears the rest.
func handOwnCandidateDirs(coreDir string, pkg string, targets []string) []string {
	packageDir := filepath.Join(coreDir, filepath.FromSlash(pkg))
	dirs := []string{packageDir}

	for _, target := range targets {
		dirs = append(dirs, filepath.Join(packageDir, goosOfTarget(target)))
	}

	return dirs
}

// applyHandOwnPlacement moves one hand-owned file — and its `.cs.auto` review sibling, if it has one
// — to the resolved destinations, clearing every other candidate location. The bytes are the corpus's
// own: a hand-owned file is never regenerated, so this MOVES rather than emits.
func applyHandOwnPlacement(placement handOwnPlacement) (int, int, error) {
	written, removed := 0, 0
	fileName := path.Base(placement.logical)

	// The `.cs.auto` sibling follows its hand-owned `.cs`, exactly as a single-target reconvert
	// already writes it (conversionDriver's manualConversion branch routes both through
	// platformLayoutPath). Without moving it here the pair would separate for one whole increment and
	// the next merge would reunite them — a merge that is not idempotent in one step.
	for _, name := range []string{fileName, fileName + ".auto"} {
		contents, found, err := readSingleHandOwnCopy(placement.candidates, name)

		if err != nil {
			return written, removed, err
		}

		if !found {
			continue
		}

		destinations := map[string]bool{}

		for _, dir := range placement.destinations {
			destinations[dir] = true

			changed, err := writeMergedContents(filepath.Join(dir, name), string(contents))

			if err != nil {
				return written, removed, err
			}

			if changed {
				written++
			}
		}

		for _, dir := range placement.candidates {
			if destinations[dir] {
				continue
			}

			gone, err := removeIfPresent(filepath.Join(dir, name))

			if err != nil {
				return written, removed, err
			}

			if gone {
				removed++
			}
		}
	}

	return written, removed, nil
}

// readSingleHandOwnCopy reads the one copy of a hand-owned file the candidate directories hold. Two
// copies that disagree are an ERROR rather than a first-wins choice: a hand-owned file duplicated
// across per-GOOS folders is hand-maintained in each, so silently propagating one over the other is
// how a fix applied to a single flavor would disappear.
func readSingleHandOwnCopy(candidates []string, name string) ([]byte, bool, error) {
	var contents []byte
	source := ""

	for _, dir := range candidates {
		filePath := filepath.Join(dir, name)
		found, err := os.ReadFile(filePath)

		if err != nil {
			if os.IsNotExist(err) {
				continue
			}

			return nil, false, fmt.Errorf("failed to read hand-owned file %q: %w", filePath, err)
		}

		if source == "" {
			contents, source = found, filePath
			continue
		}

		if string(found) != string(contents) {
			return nil, false, fmt.Errorf("hand-owned file %q exists at %q and %q with DIFFERENT contents; "+
				"layout L3 cannot choose between two hand-maintained copies — reconcile them first",
				name, source, filePath)
		}
	}

	return contents, source != "", nil
}

// handOwnEmitters narrows a principal's emitter set to the targets the hand-own is actually NEEDED
// on, and it is the difference between a correct L3 merge and a windows corpus that does not build.
//
// The rule this refines: platformHandOwnDestinations routes a companion by whether its PRINCIPAL was
// emitted everywhere. That reads the wrong fact. Being emitted on every target says the principal is
// BUILT everywhere; it says nothing about whether the companion APPLIES everywhere, because
// manualConversionFuncs scopes a registration per GOOS. runtime's trace.go is converted on all three
// targets while only the linux emission DISPLACES `StartTrace` (registered goosLinux) — so
// runtime/linux/trace_impl.cs is right, a flat copy compiles it beside windows's undisplaced body,
// and the build dies CS0111. Measured 2026-09-01 by a seeded three-target merge, and A/B'd
// byte-identical against the master converter, so it is this routing rule rather than any one arc.
//
// The narrowing reads the DISPLACEMENT itself: a target needs the companion exactly when its own
// emission of the principal carries a placeholder (funcPlaceholderLead — the one line the converter
// writes where a registration displaces a body, defined beside its emission in visitFuncDecl.go).
// That subsumes the old rule rather than special-casing it: an unscoped registration displaces on
// every target and still yields flat.
//
// TWO deliberate fallbacks to the caller's emitter set, both "no evidence, no narrowing":
//
//   - A WHOLE-FILE hand-own, whose principal is the `.cs.auto` review sibling. The whole file is
//     displaced there, so there is no placeholder inside it to count; the sibling's own emitter set
//     already answers exactly which platforms compile the Go file it replaces.
//   - A companion whose members nothing displaces — a go2cs runtime companion with no Go file behind
//     it (runtime/managed_impl.cs, internal/poll/runtime_sema_impl.cs). Same discipline the corpus
//     guard applies to a principal-less companion: no evidence, leave the placement alone.
func handOwnEmitters(principal string, targets []string, emissions []*platformEmission, principalEmitters []string) []string {
	if strings.HasSuffix(principal, ".cs.auto") {
		return principalEmitters
	}

	displacing := make([]string, 0, len(principalEmitters))

	for i, target := range targets {
		if i >= len(emissions) || emissions[i] == nil {
			continue
		}

		for rawPath, state := range emissions[i].artifacts {
			if !state.emitted || state.logicalPath(rawPath) != principal {
				continue
			}

			contents, err := os.ReadFile(filepath.Join(emissions[i].root, "core", filepath.FromSlash(rawPath)))

			if err == nil && bytes.Contains(contents, []byte(funcPlaceholderLead)) {
				displacing = append(displacing, target)
			}

			break
		}
	}

	if len(displacing) == 0 {
		return principalEmitters
	}

	return displacing
}
