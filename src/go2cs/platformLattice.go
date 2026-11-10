// platformLattice.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the KEY LATTICE of the multi-platform corpus layout — §2.1 of
// docs/phase4/DESIGN-linux-arm64-flavour.md. platformLayout.go owns what a single-target conversion
// does with the layout a tree carries; platformEmit.go owns the merge that produces it; this file is
// the one rule both of them, the hand-own router and the reference split place things by.
//
// A KEY is a folder of a converted package, and its COVER is the set of targets whose build compiles
// it:
//
//	flat (the package root)   every target of the universe
//	<goos>/                   every target of that GOOS
//	<arch>/                   every target of that GOARCH — admitted only when two or more targets share it
//	<goos>-<arch>/            one target
//
// For one (package, file name) the targets that emitted it are partitioned into content classes
// (identical bytes). A class shared by the target set S is homed at the COARSEST key whose cover
// equals S, tried in the order above; when no key matches it is SPLIT — every GOOS key whose cover
// lies inside S, then every arch key inside what is left, then a pair key per target still left —
// and one identical copy is written per key. Every key used has its cover inside S, the keys of one
// class are disjoint and the classes of one name are disjoint, so each target compiles exactly one
// copy of the name and it is its own.
//
// Ties go to the coarser key. Over three targets of one architecture that kept the corpus where layout
// L3 began ("identical ⟹ flat, anything else ⟹ one copy per emitting GOOS"): `amd64/` covered what flat
// does and each pair key what its GOOS key does. linux/arm64 separates them: `amd64/` holds what the
// three amd64 targets share and arm64 does not, and `linux-amd64/` and `linux-arm64/` split `linux/`.

package main

import (
	"fmt"
	"strings"
)

// platformUniverse is the ONE declared set of targets the converted standard library is laid out
// for. The multi-target merge runs over exactly this set (platformTargetsMatchUniverse), and a
// single-target conversion recovers the other targets' project references against it. Adding a
// target is this constant plus the corpus apply that re-homes the files the new target does not
// share, committed together: a tree and a constant that disagree are a tree no single-target
// conversion reproduces.
var platformUniverse = []string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"}

// layoutKeyKind orders the lattice's keys from coarsest to finest.
type layoutKeyKind int

const (
	layoutKeyFlat layoutKeyKind = iota
	layoutKeyGOOS
	layoutKeyArch
	layoutKeyPair
)

// layoutKey is one folder of the layout and the targets whose build compiles it.
type layoutKey struct {
	kind   layoutKeyKind
	folder string   // the package-relative folder name; empty for the flat key
	cover  []string // in universe order
}

// platformLattice is the keys of one universe, coarsest first: flat, then one GOOS key per GOOS, one
// arch key per GOARCH that two or more targets share, and one pair key per target.
type platformLattice struct {
	universe []string
	keys     []layoutKey
}

// archOfTarget returns the GOARCH half of an `os/arch` target, or nothing when it carries no
// separator — goosOfTarget's counterpart.
func archOfTarget(target string) string {
	if _, arch, found := strings.Cut(target, "/"); found {
		return arch
	}

	return ""
}

// newPlatformLattice builds the key lattice of a universe. It refuses an empty universe, a target
// that is not `os/arch`, and a repeated target: two conversions of one (GOOS, GOARCH) have no key
// that tells them apart.
func newPlatformLattice(universe []string) (*platformLattice, error) {
	if len(universe) == 0 {
		return nil, fmt.Errorf("the layout lattice needs at least one target")
	}

	lattice := &platformLattice{universe: append([]string(nil), universe...)}
	seen := map[string]bool{}
	byGOOS, byArch := map[string][]string{}, map[string][]string{}
	var goosOrder, archOrder []string

	for _, target := range universe {
		goos, arch := goosOfTarget(target), archOfTarget(target)

		if len(goos) == 0 || len(arch) == 0 || strings.Contains(arch, "/") {
			return nil, fmt.Errorf("invalid layout target %q: expected os/arch", target)
		}

		if seen[target] {
			return nil, fmt.Errorf("layout target %q is repeated: one (GOOS, GOARCH) is converted once", target)
		}

		seen[target] = true

		if _, known := byGOOS[goos]; !known {
			goosOrder = append(goosOrder, goos)
		}

		if _, known := byArch[arch]; !known {
			archOrder = append(archOrder, arch)
		}

		byGOOS[goos] = append(byGOOS[goos], target)
		byArch[arch] = append(byArch[arch], target)
	}

	lattice.keys = append(lattice.keys, layoutKey{kind: layoutKeyFlat, cover: lattice.universe})

	for _, goos := range goosOrder {
		lattice.keys = append(lattice.keys, layoutKey{kind: layoutKeyGOOS, folder: goos, cover: byGOOS[goos]})
	}

	for _, arch := range archOrder {
		// An arch shared by ONE target is indistinguishable from that target's pair key by any
		// measurement, so the rule under-claims and homes it at the pair: a later target of the same
		// arch then PROMOTES what it is proven to share, where the other choice would have to demote
		// one OS's files out of a folder a second OS had meanwhile been compiling.
		if len(byArch[arch]) >= 2 {
			lattice.keys = append(lattice.keys, layoutKey{kind: layoutKeyArch, folder: arch, cover: byArch[arch]})
		}
	}

	for _, target := range lattice.universe {
		lattice.keys = append(lattice.keys, layoutKey{kind: layoutKeyPair, folder: platformTargetTag(target), cover: []string{target}})
	}

	return lattice, nil
}

// mustPlatformLattice is newPlatformLattice for a universe the caller has already validated.
func mustPlatformLattice(universe []string) *platformLattice {
	lattice, err := newPlatformLattice(universe)

	if err != nil {
		panic(err)
	}

	return lattice
}

// corpusLattice is the lattice of platformUniverse — what a single-target conversion, which cannot
// see the other targets, reads the tree's layout against.
func corpusLattice() *platformLattice {
	return mustPlatformLattice(platformUniverse)
}

// ordered returns the members of a target set in universe order. A target outside the universe is a
// programming error rather than an input: every caller's targets are the ones the lattice was built
// from.
func (lattice *platformLattice) ordered(targets []string) []string {
	set := map[string]bool{}

	for _, target := range targets {
		set[target] = true
	}

	ordered := make([]string, 0, len(set))

	for _, target := range lattice.universe {
		if set[target] {
			ordered = append(ordered, target)
			delete(set, target)
		}
	}

	if len(set) > 0 {
		panic(fmt.Sprintf("layout lattice over [%s] asked about a target outside it: %v",
			strings.Join(lattice.universe, " "), sortedKeys(set)))
	}

	return ordered
}

// homes returns the keys a content class shared by exactly the targets in `sharing` is written to:
// the coarsest key whose cover equals the set, or failing that the GOOS-first split the file header
// describes. An empty set has no home.
func (lattice *platformLattice) homes(sharing []string) []layoutKey {
	sharing = lattice.ordered(sharing)

	if len(sharing) == 0 {
		return nil
	}

	for _, key := range lattice.keys {
		if sameTargets(key.cover, sharing) {
			return []layoutKey{key}
		}
	}

	return lattice.split(sharing)
}

// split is homes' second half on its own: the GOOS-first split of a universe-ordered target set into
// the non-flat keys whose covers lie inside it. The reference lattice also calls it for a set that
// covers the whole universe but is not shared by a participant outside it, which no flat key can say.
func (lattice *platformLattice) split(sharing []string) []layoutKey {
	remaining := map[string]bool{}

	for _, target := range lattice.ordered(sharing) {
		remaining[target] = true
	}

	var homes []layoutKey

	// lattice.keys is already GOOS, then arch, then pair, so one pass in order IS the split: a key is
	// taken when everything it covers is still unplaced.
	for _, key := range lattice.keys[1:] {
		inside := true

		for _, target := range key.cover {
			if !remaining[target] {
				inside = false
				break
			}
		}

		if !inside {
			continue
		}

		homes = append(homes, key)

		for _, target := range key.cover {
			delete(remaining, target)
		}
	}

	return homes
}

// sameTargets reports whether two universe-ordered target lists hold the same targets.
func sameTargets(a []string, b []string) bool {
	if len(a) != len(b) {
		return false
	}

	for i := range a {
		if a[i] != b[i] {
			return false
		}
	}

	return true
}

// homeFolders is homes rendered as package-relative folder names, the empty name standing for flat.
func (lattice *platformLattice) homeFolders(sharing []string) []string {
	var folders []string

	for _, key := range lattice.homes(sharing) {
		folders = append(folders, key.folder)
	}

	return folders
}

// includes reports whether a target is one of this universe's.
func (lattice *platformLattice) includes(target string) bool {
	for _, member := range lattice.universe {
		if member == target {
			return true
		}
	}

	return false
}

// targetsOfGOOS returns this universe's targets of one GOOS, in universe order.
func (lattice *platformLattice) targetsOfGOOS(goos string) []string {
	var targets []string

	for _, target := range lattice.universe {
		if goosOfTarget(target) == goos {
			targets = append(targets, target)
		}
	}

	return targets
}

// candidateFolders returns every folder a key of this universe could be written under, ADMITTED OR
// NOT — each target's GOOS, GOARCH and pair folder — in that order. It is the set a placement clears:
// a copy left in a folder the rule no longer homes the name in (the pair folder a GOOS key now ties
// with, an arch folder a second target has just admitted) is a second body for the targets that
// folder serves.
func (lattice *platformLattice) candidateFolders() []string {
	var folders []string
	seen := map[string]bool{}

	add := func(folder string) {
		if !seen[folder] {
			seen[folder] = true
			folders = append(folders, folder)
		}
	}

	for _, target := range lattice.universe {
		add(goosOfTarget(target))
	}

	for _, target := range lattice.universe {
		add(archOfTarget(target))
	}

	for _, target := range lattice.universe {
		add(platformTargetTag(target))
	}

	return folders
}

// folderCover returns the targets of this universe whose build compiles a key-shaped folder, admitted
// or not: `linux` covers every linux target, `arm64` every arm64 one, `linux-arm64` that one target.
// The empty name is flat and covers the universe. A folder that names no target of this universe
// (a `freebsd/` in a three-target corpus) covers nothing.
func (lattice *platformLattice) folderCover(folder string) []string {
	if len(folder) == 0 {
		return lattice.universe
	}

	var cover []string

	for _, target := range lattice.universe {
		if folder == goosOfTarget(target) || folder == archOfTarget(target) || folder == platformTargetTag(target) {
			cover = append(cover, target)
		}
	}

	return cover
}

// folderRank orders two key-shaped folders coarsest first — flat, GOOS, arch, pair — which is the
// order a tie between equal covers is settled in.
func folderRank(folder string) layoutKeyKind {
	switch {
	case len(folder) == 0:
		return layoutKeyFlat
	case isKnownGOOS(folder):
		return layoutKeyGOOS
	case isKnownGOARCH(folder):
		return layoutKeyArch
	default:
		return layoutKeyPair
	}
}

// coverRelation is how the covers of two keys holding one name stand to each other, which decides
// what a duplicate between them means (reconcileLayoutDuplicates).
type coverRelation int

const (
	coversDisjoint    coverRelation = iota // no target compiles both
	coversNested                           // one lies inside the other (or they are equal)
	coversOverlapping                      // they share a target and each serves one the other does not
)

// relateCovers classifies two covers. Equal covers are nested: either copy alone serves every target
// of both.
func relateCovers(a []string, b []string) coverRelation {
	inA := map[string]bool{}

	for _, target := range a {
		inA[target] = true
	}

	shared := 0

	for _, target := range b {
		if inA[target] {
			shared++
		}
	}

	switch {
	case shared == 0:
		return coversDisjoint
	case shared == len(a) || shared == len(b):
		return coversNested
	default:
		return coversOverlapping
	}
}

// splitPairFolderName splits a `<goos>-<arch>` folder name into its halves. No GOOS name contains a
// hyphen, so the first one is the separator.
func splitPairFolderName(name string) (goos string, arch string, ok bool) {
	goos, arch, found := strings.Cut(name, "-")

	if !found || !isKnownGOOS(goos) || !isKnownGOARCH(arch) {
		return "", "", false
	}

	return goos, arch, true
}

// isLayoutKeyFolderName reports whether a folder NAME is shaped like a layout key: a GOOS, a GOARCH
// or a `<goos>-<arch>` pair. It says nothing about the folder itself — isPlatformSourceFolder adds the
// discriminator that tells a source folder from a nested package of the same name.
func isLayoutKeyFolderName(name string) bool {
	if isKnownGOOS(name) || isKnownGOARCH(name) {
		return true
	}

	_, _, pair := splitPairFolderName(name)

	return pair
}

// isArchLayoutFolderName reports whether a folder name is an ARCH-bearing key — a GOARCH or a pair —
// the two a project file selects with $(GoTargetArch) rather than $(GoTargetOS) alone.
func isArchLayoutFolderName(name string) bool {
	return isLayoutKeyFolderName(name) && !isKnownGOOS(name)
}

// platformLayoutFolders returns the key folders a single-target conversion probes for a file, finest
// first: `<goos>-<arch>`, `<arch>`, `<goos>`. A bare GOOS (no separator) probes its GOOS folder alone,
// and an empty target probes nothing. On a canonical tree at most one of them holds a given name for
// the target, so the order decides nothing there; finest-first is what a NON-canonical tree should
// resolve to, since the narrower copy is the one written for this target specifically.
func platformLayoutFolders(target string) []string {
	goos, arch := goosOfTarget(target), archOfTarget(target)

	if len(goos) == 0 {
		return nil
	}

	if len(arch) == 0 {
		return []string{goos}
	}

	return []string{goos + "-" + arch, arch, goos}
}

// layoutFolderServes reports whether a key-shaped folder is one a target's build compiles: the target's
// pair, GOARCH or GOOS folder (a bare GOOS is served by its own folder alone). It needs no lattice, since
// whether a folder's name matches a target does not depend on which other targets exist.
func layoutFolderServes(folder string, target string) bool {
	for _, candidate := range platformLayoutFolders(target) {
		if folder == candidate {
			return true
		}
	}

	return false
}

// platformTargetsMatchUniverse is the multi-target merge's entry check: the requested targets must
// be exactly the declared universe, in any order. A strict subset would re-home every file the
// missing targets share as though they did not exist, and a target outside it would enter the layout
// without the constant a single-target conversion reads the tree against.
func platformTargetsMatchUniverse(targets []string, universe []string) error {
	if _, err := newPlatformLattice(targets); err != nil {
		return err
	}

	requested, declared := map[string]bool{}, map[string]bool{}

	for _, target := range targets {
		requested[target] = true
	}

	var missing, foreign []string

	for _, target := range universe {
		declared[target] = true

		if !requested[target] {
			missing = append(missing, target)
		}
	}

	for _, target := range targets {
		if !declared[target] {
			foreign = append(foreign, target)
		}
	}

	if len(missing) == 0 && len(foreign) == 0 {
		return nil
	}

	var problems []string

	if len(missing) > 0 {
		problems = append(problems, "missing "+strings.Join(missing, ", "))
	}

	if len(foreign) > 0 {
		problems = append(problems, "not in it: "+strings.Join(foreign, ", "))
	}

	return fmt.Errorf("the multi-platform emission runs over exactly the declared universe [%s] (%s); "+
		"a subset would re-home every file the absent targets share, and a new target joins through platformUniverse "+
		"and its corpus apply together", strings.Join(universe, ","), strings.Join(problems, "; "))
}

// partitionContentClasses groups the targets that emitted one artifact by its exact bytes. Classes
// are returned in the order of their first member, members in `targets` order; a target absent from
// `hashes` did not emit the artifact and is in no class.
func partitionContentClasses(targets []string, hashes map[string]string) [][]string {
	var classes [][]string
	classOf := map[string]int{}

	for _, target := range targets {
		hash, emitted := hashes[target]

		if !emitted {
			continue
		}

		index, known := classOf[hash]

		if !known {
			index = len(classes)
			classOf[hash] = index
			classes = append(classes, nil)
		}

		classes[index] = append(classes[index], target)
	}

	return classes
}
