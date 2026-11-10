// platformProject.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the SECOND half of layout L3 — increment 3 of
// docs/phase4/DESIGN-multiplatform-corpus.md. platformLayout.go routes a package's platform-varying
// SOURCES into per-GOOS folders and gives the .csproj the `$(GoTargetOS)/*.cs` include; this file
// does the same job for the package's REFERENCES.
//
// The design measures 24 packages whose DIRECT IMPORT SET differs by GOOS (§4, as corrected by
// increment 1's census). `os` is the clearest: it imports `internal/syscall/windows` on Windows and
// `internal/syscall/unix` on Linux and macOS. One flat <ProjectReference> list cannot express that —
// a Linux build would reference a Windows-only project, which on Linux compiles to an empty assembly
// at best and drags a kernel32 P/Invoke surface into the closure at worst.
//
//	<ItemGroup>
//	  <ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />
//	  ... every reference the package has on EVERY platform ...
//	</ItemGroup>
//
//	<ItemGroup Condition="'$(GoTargetOS)'=='linux'">
//	  <ProjectReference Include="$(go2csPath)core/internal/syscall/unix/internal.syscall.unix.csproj" />
//	</ItemGroup>
//
// That split — shared flat, differences conditioned — is deliberately the SAME rule L3 applies to
// sources, read one level up: identical on every platform is stated once, and only what actually
// varies is selected.
//
// # Why every participating platform gets a group, even an empty one
//
// A single-target conversion cannot compute the platform axis (design §4.2); it can only HONOR what
// the tree already carries — the layout-adoption rule platformLayout.go's header sets out. For
// references, "what the tree carries" has to include WHICH platforms participate, because the shared
// set is an intersection and an intersection over a forgotten platform is wrong in the dangerous
// direction (too large — a Linux-only reference would be promoted into the shared set and land in
// the Windows build). A platform whose delta happens to be empty therefore still gets a group:
//
//	<ItemGroup Condition="'$(GoTargetOS)'=='darwin'" />
//
// which reads as "darwin participates and adds nothing", and is what lets a darwin reconvert
// reproduce the file rather than quietly collapse the axis.
//
// # The one renderer both producers go through
//
// renderPlatformReferences is called from exactly two places, and that is what makes the byte
// identity hold:
//
//   - the multi-target merge (platformEmit.go), which CREATES the block from three real emissions;
//   - writeProjectFile (projectFileWriter.go), where a single-target reconvert re-derives the block
//     by recovering the other platforms' sets from the file it is about to overwrite.
//
// Both feed the same splitPlatformReferenceSets → renderPlatformReferences pair, so a reconvert of
// an L3 package produces the merge's bytes exactly, and CLAUDE.md's seeded-reconvert ritual keeps
// reading empty.

package main

import (
	"fmt"
	"os"
	"regexp"
	"sort"
	"strings"
)

const (
	// platformSharedReferenceAnchor is the golib reference line every generated .csproj carries,
	// hardcoded in csproj-template.xml directly ABOVE the ProjectReferenceMarker slot. It is the
	// landmark for the unconditioned reference list: the references this file has to read back are
	// exactly the ones between it and that ItemGroup's close.
	platformSharedReferenceAnchor = "    <ProjectReference Include=\"$(go2csPath)core/golib/golib.csproj\" />\r\n"

	// platformReferenceGroupClose closes the unconditioned reference ItemGroup (and every
	// conditioned one this file writes).
	platformReferenceGroupClose = "  </ItemGroup>\r\n"

	// platformProjectClose is the document's closing tag; the conditioned groups are appended
	// immediately before it, which is also what makes them trivially recoverable.
	platformProjectClose = "</Project>"

	// platformReferenceBlockHeader introduces the conditioned groups and is the sentinel that finds
	// them again. It is written verbatim, so it doubles as the block's start marker.
	platformReferenceBlockHeader = "  <!-- Layout L3: this package's direct imports differ by GOOS, so the references that are not\r\n" +
		"       common to every platform are selected by $(" + platformTargetOSProperty + ")\r\n" +
		"       (docs/phase4/DESIGN-multiplatform-corpus.md §8). A group with no items is not noise: it\r\n" +
		"       records that the platform takes part in this package's axis and adds nothing beyond the\r\n" +
		"       shared list above, which is what lets a single-target reconvert reproduce this file. -->\r\n"
)

// platformReferenceLineRegex matches one emitted <ProjectReference> line. The Include value is
// captured in its EMITTED (XML-escaped) spelling and is never unescaped: every set operation and
// every re-render in this file speaks that one spelling, so a reference containing an `&` compares
// and round-trips without a double-escape.
var platformReferenceLineRegex = regexp.MustCompile(`^\s*<ProjectReference Include="([^"]*)"\s*/>\s*$`)

// platformReferenceGroupRegex matches the opening tag of a conditioned reference group, in both the
// populated (`>`) and empty (`/>`) spellings. The condition is captured whole and read by
// platformReferenceGroupKey, which admits exactly the three spellings composePlatformReferenceGroups
// writes.
var platformReferenceGroupRegex = regexp.MustCompile(`^\s*<ItemGroup Condition="([^"]*)"\s*(/?)>\s*$`)

// The three conditions a reference group is selected by, one per kind of layout key (platformLattice.go):
// a GOOS key, an arch key, and a (GOOS, GOARCH) pair key.
var (
	platformGOOSConditionRegex = regexp.MustCompile(`^'\$\(` + platformTargetOSProperty + `\)'=='([^']*)'$`)
	platformArchConditionRegex = regexp.MustCompile(`^'\$\(` + platformTargetArchProperty + `\)'=='([^']*)'$`)
	platformPairConditionRegex = regexp.MustCompile(`^'\$\(` + platformTargetOSProperty + `\)'=='([^']*)' And '\$\(` +
		platformTargetArchProperty + `\)'=='([^']*)'$`)
)

// platformReferenceGroupKey reads a reference group's condition back as the layout key folder it
// selects (`linux`, `amd64`, `linux-arm64`), or ok=false for any other condition.
func platformReferenceGroupKey(condition string) (string, bool) {
	if match := platformGOOSConditionRegex.FindStringSubmatch(condition); match != nil {
		return match[1], true
	}

	if match := platformArchConditionRegex.FindStringSubmatch(condition); match != nil {
		return match[1], true
	}

	if match := platformPairConditionRegex.FindStringSubmatch(condition); match != nil {
		return match[1] + "-" + match[2], true
	}

	return "", false
}

// platformReferenceGroupKind classifies a reference group's key. Anything that is neither a GOARCH
// nor a `<goos>-<arch>` pair is a GOOS group, which is how every key was read before the arch axis.
func platformReferenceGroupKind(key string) layoutKeyKind {
	if isKnownGOARCH(key) {
		return layoutKeyArch
	}

	if _, _, pair := splitPairFolderName(key); pair {
		return layoutKeyPair
	}

	return layoutKeyGOOS
}

// platformReferenceGroupCondition renders the condition that selects a reference group's key.
func platformReferenceGroupCondition(key string) string {
	switch platformReferenceGroupKind(key) {
	case layoutKeyArch:
		return "'$(" + platformTargetArchProperty + ")'=='" + key + "'"
	case layoutKeyPair:
		goos, arch, _ := splitPairFolderName(key)

		return "'$(" + platformTargetOSProperty + ")'=='" + goos + "' And '$(" + platformTargetArchProperty + ")'=='" + arch + "'"
	default:
		return "'$(" + platformTargetOSProperty + ")'=='" + key + "'"
	}
}

// parseSharedProjectReferences returns the UNCONDITIONED <ProjectReference> list of an emitted
// .csproj — every reference except golib itself, in emitted spelling and emitted order. The second
// result is false when the file has no recognizable reference group, which is the signal to leave it
// alone entirely rather than guess (a caller-supplied -csproj template, or a -recurse=nuget project
// whose golib reference became a PackageReference).
func parseSharedProjectReferences(contents string) ([]string, bool) {
	anchorAt := strings.Index(contents, platformSharedReferenceAnchor)

	if anchorAt < 0 {
		return nil, false
	}

	start := anchorAt + len(platformSharedReferenceAnchor)
	closeAt := strings.Index(contents[start:], platformReferenceGroupClose)

	if closeAt < 0 {
		return nil, false
	}

	return parseReferenceLines(contents[start : start+closeAt]), true
}

// parseReferenceLines pulls every <ProjectReference Include="…"> out of a region of a .csproj.
func parseReferenceLines(region string) []string {
	references := make([]string, 0, 8)

	for _, line := range strings.Split(region, "\n") {
		if match := platformReferenceLineRegex.FindStringSubmatch(strings.TrimSuffix(line, "\r")); match != nil {
			references = append(references, match[1])
		}
	}

	return references
}

// parsePlatformReferenceGroups returns the reference DELTAS an emitted .csproj carries, keyed by the
// layout key folder each group selects (`linux`, `amd64`, `linux-arm64`), or ok=false when it
// carries no conditioned block at all (the 280 packages whose imports are the same everywhere). A
// platform that takes part with an empty delta is present in the map with an empty slice — that
// presence is the axis membership the adoption rule needs.
func parsePlatformReferenceGroups(contents string) (map[string][]string, bool) {
	blockAt := strings.Index(contents, platformReferenceBlockHeader)

	if blockAt < 0 {
		return nil, false
	}

	closeAt := strings.LastIndex(contents, platformProjectClose)

	if closeAt < blockAt {
		return nil, false
	}

	deltas := map[string][]string{}
	group := ""

	for _, raw := range strings.Split(contents[blockAt:closeAt], "\n") {
		line := strings.TrimSuffix(raw, "\r")

		if match := platformReferenceGroupRegex.FindStringSubmatch(line); match != nil {
			if key, ok := platformReferenceGroupKey(match[1]); ok {
				group = key

				// `/>` — the platform takes part and adds nothing. Record the membership and close it
				// out immediately, so a following unconditioned line cannot be read as one of its items.
				if match[2] == "/" {
					deltas[group] = []string{}
					group = ""
				} else {
					deltas[group] = []string{}
				}

				continue
			}
		}

		if len(group) == 0 {
			continue
		}

		if strings.TrimSpace(line) == strings.TrimSpace(platformReferenceGroupClose) {
			group = ""
			continue
		}

		if match := platformReferenceLineRegex.FindStringSubmatch(line); match != nil {
			deltas[group] = append(deltas[group], match[1])
		}
	}

	return deltas, true
}

// platformFullReferenceSet returns the COMPLETE reference set an emitted .csproj expresses for one
// target: the unconditioned list plus every conditioned group whose key the target's build selects —
// its GOOS, its GOARCH and its pair (layoutFolderServes). A bare GOOS reads its GOOS group alone. It
// is how the other platforms' sets are recovered from the file a single-target reconvert is about to
// overwrite, and how each staging root's project file is read during a multi-target merge.
func platformFullReferenceSet(contents string, target string) ([]string, bool) {
	shared, ok := parseSharedProjectReferences(contents)

	if !ok {
		return nil, false
	}

	if deltas, hasBlock := parsePlatformReferenceGroups(contents); hasBlock {
		for key, delta := range deltas {
			if layoutFolderServes(key, target) {
				shared = append(shared, delta...)
			}
		}
	}

	return sortedUniqueStrings(shared), true
}

// referenceParticipants resolves a reference-set key to the participants it stands for. A target of
// the lattice is itself. Any other key is read as its GOOS, the reading every key had before the
// arch axis: every lattice target of that GOOS, or, when the lattice has none, the GOOS itself as a
// participant of its own whose only group is its GOOS group. So a single-target conversion at a
// target outside the declared universe (a host default of darwin/arm64) stands in for its GOOS's
// target, as it always did.
func referenceParticipants(lattice *platformLattice, key string) []string {
	if lattice.includes(key) {
		return []string{key}
	}

	goos := goosOfTarget(key)

	if targets := lattice.targetsOfGOOS(goos); len(targets) > 0 {
		return targets
	}

	return []string{goos}
}

// splitPlatformReferenceSets is splitPlatformReferenceSetsOver the declared universe's lattice: the
// reading a single-target conversion has.
func splitPlatformReferenceSets(sets map[string][]string) ([]string, map[string][]string, bool) {
	return splitPlatformReferenceSetsOver(corpusLattice(), sets)
}

// splitPlatformReferenceSetsOver splits each participating platform's FULL reference set into the
// shared list (present on EVERY participant) and the conditioned groups, keyed by layout key folder.
// A key of sets is a target or a GOOS (referenceParticipants). A reference the participants do not
// all share, held by the set S of them, goes to the keys the lattice homes S at (platformLattice.homes)
// — a GOOS group, an arch group or a pair group — so each target selects it exactly when it holds it.
//
// Participation is recorded too, because an intersection over a forgotten platform is wrong in the
// dangerous direction (the file header): a GOOS group, empty or not, for a GOOS all of whose lattice
// targets take part, and otherwise a pair group for each target of that GOOS that does. An arch group
// records none. Over one target per GOOS every participant gets its GOOS group, as before the arch
// axis.
//
// The third result reports whether any platform actually differs; when it is false the package's
// imports agree everywhere and no conditioned block is emitted, so a package that stops varying
// converges back to the plain project file rather than carrying an inert block forever.
func splitPlatformReferenceSetsOver(lattice *platformLattice, sets map[string][]string) ([]string, map[string][]string, bool) {
	if len(sets) == 0 {
		return nil, nil, false
	}

	participants := map[string][]string{}

	for _, key := range sortedMapKeys(sets) {
		for _, participant := range referenceParticipants(lattice, key) {
			participants[participant] = sortedUniqueStrings(sets[key])
		}
	}

	counts := map[string]int{}

	for _, references := range participants {
		for _, reference := range references {
			counts[reference]++
		}
	}

	shared := make([]string, 0, len(counts))
	var varying []string

	for reference, count := range counts {
		if count == len(participants) {
			shared = append(shared, reference)
		} else {
			varying = append(varying, reference)
		}
	}

	sort.Strings(shared)
	sort.Strings(varying)

	deltas := map[string][]string{}

	for _, goos := range sortedKeys(func() map[string]bool {
		set := map[string]bool{}
		for _, target := range lattice.universe {
			set[goosOfTarget(target)] = true
		}
		return set
	}()) {
		targets := lattice.targetsOfGOOS(goos)
		var taking []string

		for _, target := range targets {
			if _, ok := participants[target]; ok {
				taking = append(taking, target)
			}
		}

		if len(taking) == len(targets) {
			deltas[goos] = []string{}
			continue
		}

		for _, target := range taking {
			deltas[platformTargetTag(target)] = []string{}
		}
	}

	for participant := range participants {
		if !lattice.includes(participant) {
			deltas[participant] = []string{}
		}
	}

	for _, reference := range varying {
		var holders []string
		var foreign []string

		for participant, references := range participants {
			if !containsSortedString(references, reference) {
				continue
			}

			if lattice.includes(participant) {
				holders = append(holders, participant)
			} else {
				foreign = append(foreign, participant)
			}
		}

		var keys []string

		if len(holders) > 0 {
			homes := lattice.homes(holders)

			// Flat is a home only when a participant outside the lattice is the one that lacks the
			// reference; the lattice's own targets then each select it through their split keys.
			if len(homes) == 1 && homes[0].kind == layoutKeyFlat {
				homes = lattice.split(holders)
			}

			for _, home := range homes {
				keys = append(keys, home.folder)
			}
		}

		keys = append(keys, foreign...)

		for _, key := range keys {
			deltas[key] = append(deltas[key], reference)
		}
	}

	varies := false

	for _, delta := range deltas {
		if len(delta) > 0 {
			varies = true
		}
	}

	return shared, deltas, varies
}

// containsSortedString reports whether a sorted slice holds a value.
func containsSortedString(values []string, value string) bool {
	at := sort.SearchStrings(values, value)

	return at < len(values) && values[at] == value
}

// sortedUniqueStrings returns values sorted with duplicates collapsed, leaving the input untouched.
func sortedUniqueStrings(values []string) []string {
	seen := map[string]bool{}
	result := make([]string, 0, len(values))

	for _, value := range values {
		if seen[value] {
			continue
		}

		seen[value] = true
		result = append(result, value)
	}

	sort.Strings(result)

	return result
}

// renderPlatformReferences rewrites an emitted .csproj so its unconditioned reference list is
// exactly shared and its conditioned groups are exactly deltas — replacing whatever the file carried
// before, so the function is idempotent and order-independent. A nil/empty deltas map (or one whose
// every delta is empty) removes the conditioned block entirely.
//
// This is the SINGLE renderer both the multi-target merge and a single-target reconvert go through;
// see the file header.
func renderPlatformReferences(contents string, shared []string, deltas map[string][]string) (string, error) {
	anchorAt := strings.Index(contents, platformSharedReferenceAnchor)

	if anchorAt < 0 {
		return contents, fmt.Errorf("project file carries no \"%s\" reference line to anchor the shared list on",
			strings.TrimSpace(platformSharedReferenceAnchor))
	}

	start := anchorAt + len(platformSharedReferenceAnchor)
	closeAt := strings.Index(contents[start:], platformReferenceGroupClose)

	if closeAt < 0 {
		return contents, fmt.Errorf("project file's reference ItemGroup is not closed")
	}

	// The template puts the ProjectReferenceMarker on its own line, so the region between the golib
	// line and the group close is a leading blank line followed by one line per reference. Reproduce
	// that shape exactly — this is the same text writeProjectFile's marker substitution produces.
	region := &strings.Builder{}
	region.WriteString("\r\n")

	for _, reference := range shared {
		region.WriteString("    <ProjectReference Include=\"" + reference + "\" />\r\n")
	}

	rebuilt := contents[:start] + region.String() + contents[start+closeAt:]

	// The block's ONE position: directly after the shared reference group and the blank line that
	// follows it. The other groups after it are written by passes that append before </Project>
	// (licensing's shared-LICENSE <None> group, the merge's kept remainder). A block placed at
	// </Project>, or put back where an earlier render had left it, therefore landed before or after
	// them depending on which pass ran first, and re-emit was not a fixed point: the H7 consolidated
	// re-emit moved the LICENSE group of three L3 projects (internal/runtime/cgroup,
	// internal/syscall/windows, crypto/internal/fips140deps/time) from before the block to after it,
	// lines identical, once the stage inputs stopped differing. Anchored here, the file is a function
	// of its other contents and the reference sets alone.
	insertAt := start + region.Len() + len(platformReferenceGroupClose)

	if strings.HasPrefix(rebuilt[insertAt:], "\r\n") {
		insertAt += len("\r\n")
	}

	// Drop any block this file already carried before inserting the new one, so the rewrite is a
	// replacement rather than an accumulation -- and drop THE BLOCK ONLY. It used to be cut from its
	// header to </Project>, which also took everything written after it, and the H10 close's regen
	// lost log/syslog's `<None Include="../../LICENSE" …>` that way (licensing_test.go guards it).
	// A block always follows the shared reference group, so removing one never moves insertAt.
	for {
		blockAt := strings.Index(rebuilt, platformReferenceBlockHeader)

		if blockAt < 0 {
			break
		}

		endAt, err := platformReferenceBlockEnd(rebuilt, blockAt)

		if err != nil {
			return rebuilt, err
		}

		if blockAt < insertAt {
			return rebuilt, fmt.Errorf("project file's conditioned reference block precedes its shared reference group")
		}

		rebuilt = rebuilt[:blockAt] + rebuilt[endAt:]
	}

	block := composePlatformReferenceGroups(deltas)

	if len(block) == 0 {
		return rebuilt, nil
	}

	return rebuilt[:insertAt] + block + rebuilt[insertAt:], nil
}

// platformReferenceBlockEnd returns the offset just past the conditioned block that starts at
// blockAt: the header, then every conditioned group composePlatformReferenceGroups writes (an empty
// `… />` or an open group closed by platformReferenceGroupClose), each followed by its blank line.
// Whatever comes after -- another ItemGroup, or </Project> -- is not part of it.
func platformReferenceBlockEnd(contents string, blockAt int) (int, error) {
	// A GOOS or pair group opens on $(GoTargetOS), an arch group on $(GoTargetArch).
	goosGroupOpen := "  <ItemGroup Condition=\"'$(" + platformTargetOSProperty + ")'=='"
	archGroupOpen := "  <ItemGroup Condition=\"'$(" + platformTargetArchProperty + ")'=='"
	at := blockAt + len(platformReferenceBlockHeader)

	for strings.HasPrefix(contents[at:], goosGroupOpen) || strings.HasPrefix(contents[at:], archGroupOpen) {
		lineEnd := strings.Index(contents[at:], "\r\n")

		if lineEnd < 0 {
			return 0, fmt.Errorf("project file's conditioned reference group is not terminated")
		}

		if strings.HasSuffix(contents[at:at+lineEnd], " />") {
			at += lineEnd + len("\r\n")
		} else {
			closeAt := strings.Index(contents[at:], platformReferenceGroupClose)

			if closeAt < 0 {
				return 0, fmt.Errorf("project file's conditioned reference group is not closed")
			}

			at += closeAt + len(platformReferenceGroupClose)
		}

		at += len("\r\n")

		if at > len(contents) || contents[at-len("\r\n"):at] != "\r\n" {
			return 0, fmt.Errorf("project file's conditioned reference group is not followed by its blank line")
		}
	}

	return at, nil
}

// composePlatformReferenceGroups renders the conditioned <ItemGroup> block: the GOOS groups, then the
// arch groups, then the pair groups, each kind sorted by key for determinism. Returns "" when no
// platform has a delta.
func composePlatformReferenceGroups(deltas map[string][]string) string {
	names := make([]string, 0, len(deltas))
	varies := false

	for key, delta := range deltas {
		names = append(names, key)

		if len(delta) > 0 {
			varies = true
		}
	}

	if !varies {
		return ""
	}

	sort.SliceStable(names, func(i, j int) bool {
		if kindI, kindJ := platformReferenceGroupKind(names[i]), platformReferenceGroupKind(names[j]); kindI != kindJ {
			return kindI < kindJ
		}

		return names[i] < names[j]
	})

	block := &strings.Builder{}
	block.WriteString(platformReferenceBlockHeader)

	for _, key := range names {
		condition := "  <ItemGroup Condition=\"" + platformReferenceGroupCondition(key) + "\""

		if len(deltas[key]) == 0 {
			block.WriteString(condition + " />\r\n\r\n")
			continue
		}

		block.WriteString(condition + ">\r\n")

		for _, reference := range deltas[key] {
			block.WriteString("    <ProjectReference Include=\"" + reference + "\" />\r\n")
		}

		block.WriteString(platformReferenceGroupClose + "\r\n")
	}

	return block.String()
}

// adoptPlatformReferences is adoptPlatformReferencesOver the declared universe's lattice, which is
// what a single-target conversion reads the tree against.
func adoptPlatformReferences(existingContents string, target string, references []string) ([]string, map[string][]string, bool) {
	return adoptPlatformReferencesOver(corpusLattice(), existingContents, target, references)
}

// adoptPlatformReferencesOver is layout adoption for REFERENCES: given the project file that is about
// to be overwritten and THIS conversion's own reference list, re-derive the shared list and the
// conditioned groups so the other platforms' sets survive a single-target reconvert.
//
// The participants are read back from the groups that record them (splitPlatformReferenceSetsOver):
// a GOOS group stands for every lattice target of that GOOS, a pair group for its one target, and an
// arch group for none. Each participant's full set is the shared list plus every group its build
// selects.
//
// Returns ok=false when the existing file carries no conditioned block — the overwhelmingly common
// case, and the one where writeProjectFile must emit exactly the project file it always did.
func adoptPlatformReferencesOver(lattice *platformLattice, existingContents string, target string, references []string) ([]string, map[string][]string, bool) {
	if len(target) == 0 {
		return nil, nil, false
	}

	existingDeltas, hasBlock := parsePlatformReferenceGroups(existingContents)

	if !hasBlock {
		return nil, nil, false
	}

	existingShared, sharedOK := parseSharedProjectReferences(existingContents)

	if !sharedOK {
		return nil, nil, false
	}

	sets := map[string][]string{}

	for key := range existingDeltas {
		var recorded []string

		switch platformReferenceGroupKind(key) {
		case layoutKeyGOOS:
			recorded = referenceParticipants(lattice, key)
		case layoutKeyPair:
			// A pair group for a target outside the lattice records nothing this lattice can build.
			if goos, arch, _ := splitPairFolderName(key); lattice.includes(goos + "/" + arch) {
				recorded = []string{goos + "/" + arch}
			}
		}

		for _, participant := range recorded {
			set := append([]string{}, existingShared...)

			for group, delta := range existingDeltas {
				// A participant outside the lattice is a bare GOOS, served by its GOOS group alone.
				if layoutFolderServes(group, participant) {
					set = append(set, delta...)
				}
			}

			sets[participant] = set
		}
	}

	// This target's set is recomputed from the conversion, never recovered — it is the one platform
	// whose truth this run actually holds. A target absent from the recorded axis JOINS it, which is
	// how a new platform would be added without a special case.
	for _, participant := range referenceParticipants(lattice, target) {
		sets[participant] = references
	}

	shared, deltas, _ := splitPlatformReferenceSetsOver(lattice, sets)

	return shared, deltas, true
}

// applyPlatformReferenceAdoption is the writeProjectFile-facing wrapper: read the project file this
// write is replacing, and if it carries conditioned reference groups, re-render the freshly
// generated contents so the other platforms' sets survive. Returns newContents unchanged in every
// other case — no existing file, no block, or an unusable template — so a conversion that has
// nothing to adopt emits exactly the project file it always did.
func applyPlatformReferenceAdoption(newContents string, projectFileName string, target string, references []string) string {
	existing, err := os.ReadFile(projectFileName)

	if err != nil {
		return newContents
	}

	shared, deltas, ok := adoptPlatformReferences(string(existing), target, references)

	if !ok {
		return newContents
	}

	rendered, err := renderPlatformReferences(newContents, shared, deltas)

	if err != nil {
		// The package's imports DO vary by platform and this write cannot say so; emitting the flat
		// list would silently give every platform this target's references. Worth a word.
		showWarning("Project file \"%s\" carries per-GOOS <ProjectReference> groups that could not be re-rendered (%s); "+
			"its conditioned references were left as they were", projectFileName, err)

		return newContents
	}

	return rendered
}
