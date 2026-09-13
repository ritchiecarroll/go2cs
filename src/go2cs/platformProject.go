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
// populated (`>`) and empty (`/>`) spellings.
var platformReferenceGroupRegex = regexp.MustCompile(`^\s*<ItemGroup Condition="'\$\(` + platformTargetOSProperty + `\)'=='([^']*)'"\s*(/?)>\s*$`)

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

// parsePlatformReferenceGroups returns the per-GOOS reference DELTAS an emitted .csproj carries, or
// ok=false when it carries no conditioned block at all (the 280 packages whose imports are the same
// everywhere). A platform that takes part with an empty delta is present in the map with an empty
// slice — that presence is the axis membership the adoption rule needs.
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
	goos := ""

	for _, raw := range strings.Split(contents[blockAt:closeAt], "\n") {
		line := strings.TrimSuffix(raw, "\r")

		if match := platformReferenceGroupRegex.FindStringSubmatch(line); match != nil {
			goos = match[1]

			// `/>` — the platform takes part and adds nothing. Record the membership and close it
			// out immediately, so a following unconditioned line cannot be read as one of its items.
			if match[2] == "/" {
				deltas[goos] = []string{}
				goos = ""
			} else {
				deltas[goos] = []string{}
			}

			continue
		}

		if len(goos) == 0 {
			continue
		}

		if strings.TrimSpace(line) == strings.TrimSpace(platformReferenceGroupClose) {
			goos = ""
			continue
		}

		if match := platformReferenceLineRegex.FindStringSubmatch(line); match != nil {
			deltas[goos] = append(deltas[goos], match[1])
		}
	}

	return deltas, true
}

// platformFullReferenceSet returns the COMPLETE reference set an emitted .csproj expresses for one
// GOOS: the unconditioned list plus that platform's conditioned delta. It is how the other
// platforms' sets are recovered from the file a single-target reconvert is about to overwrite, and
// how each staging root's project file is read during a multi-target merge.
func platformFullReferenceSet(contents string, goos string) ([]string, bool) {
	shared, ok := parseSharedProjectReferences(contents)

	if !ok {
		return nil, false
	}

	if deltas, hasBlock := parsePlatformReferenceGroups(contents); hasBlock {
		shared = append(shared, deltas[goos]...)
	}

	return sortedUniqueStrings(shared), true
}

// splitPlatformReferenceSets splits each participating platform's FULL reference set into the shared
// list (present on EVERY platform) and that platform's delta. Every key of sets appears in deltas,
// empty or not — see the file header on why an empty group is written rather than omitted.
//
// The third result reports whether any platform actually differs; when it is false the package's
// imports agree everywhere and no conditioned block is emitted, so a package that stops varying
// converges back to the plain project file rather than carrying an inert block forever.
func splitPlatformReferenceSets(sets map[string][]string) ([]string, map[string][]string, bool) {
	if len(sets) == 0 {
		return nil, nil, false
	}

	counts := map[string]int{}

	for _, references := range sets {
		for _, reference := range sortedUniqueStrings(references) {
			counts[reference]++
		}
	}

	shared := make([]string, 0, len(counts))

	for reference, count := range counts {
		if count == len(sets) {
			shared = append(shared, reference)
		}
	}

	sort.Strings(shared)

	sharedSet := map[string]bool{}

	for _, reference := range shared {
		sharedSet[reference] = true
	}

	deltas := make(map[string][]string, len(sets))
	varies := false

	for goos, references := range sets {
		delta := make([]string, 0)

		for _, reference := range sortedUniqueStrings(references) {
			if !sharedSet[reference] {
				delta = append(delta, reference)
			}
		}

		if len(delta) > 0 {
			varies = true
		}

		deltas[goos] = delta
	}

	return shared, deltas, varies
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

	// Drop any block this file already carried before appending the new one, so the rewrite is a
	// replacement rather than an accumulation.
	if blockAt := strings.Index(rebuilt, platformReferenceBlockHeader); blockAt >= 0 {
		endAt := strings.LastIndex(rebuilt, platformProjectClose)

		if endAt < blockAt {
			return rebuilt, fmt.Errorf("project file's conditioned reference block is not followed by %s", platformProjectClose)
		}

		rebuilt = rebuilt[:blockAt] + rebuilt[endAt:]
	}

	block := composePlatformReferenceGroups(deltas)

	if len(block) == 0 {
		return rebuilt, nil
	}

	insertAt := strings.LastIndex(rebuilt, platformProjectClose)

	if insertAt < 0 {
		return rebuilt, fmt.Errorf("project file has no %s to insert the conditioned reference block before", platformProjectClose)
	}

	return rebuilt[:insertAt] + block + rebuilt[insertAt:], nil
}

// composePlatformReferenceGroups renders the conditioned <ItemGroup> block, GOOS-sorted for
// determinism. Returns "" when no platform has a delta.
func composePlatformReferenceGroups(deltas map[string][]string) string {
	names := make([]string, 0, len(deltas))
	varies := false

	for goos, delta := range deltas {
		names = append(names, goos)

		if len(delta) > 0 {
			varies = true
		}
	}

	if !varies {
		return ""
	}

	sort.Strings(names)

	block := &strings.Builder{}
	block.WriteString(platformReferenceBlockHeader)

	for _, goos := range names {
		condition := "  <ItemGroup Condition=\"'$(" + platformTargetOSProperty + ")'=='" + goos + "'\""

		if len(deltas[goos]) == 0 {
			block.WriteString(condition + " />\r\n\r\n")
			continue
		}

		block.WriteString(condition + ">\r\n")

		for _, reference := range deltas[goos] {
			block.WriteString("    <ProjectReference Include=\"" + reference + "\" />\r\n")
		}

		block.WriteString(platformReferenceGroupClose + "\r\n")
	}

	return block.String()
}

// adoptPlatformReferences is layout adoption for REFERENCES: given the project file that is about to
// be overwritten and THIS conversion's own reference list, re-derive the shared list and the
// per-GOOS deltas so the other platforms' sets survive a single-target reconvert.
//
// Returns ok=false when the existing file carries no conditioned block — the overwhelmingly common
// case, and the one where writeProjectFile must emit exactly the project file it always did.
func adoptPlatformReferences(existingContents string, goos string, references []string) ([]string, map[string][]string, bool) {
	if len(goos) == 0 {
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

	sets := make(map[string][]string, len(existingDeltas)+1)

	for platform, delta := range existingDeltas {
		sets[platform] = append(append([]string{}, existingShared...), delta...)
	}

	// This target's set is recomputed from the conversion, never recovered — it is the one platform
	// whose truth this run actually holds. A target absent from the recorded axis JOINS it, which is
	// how a fourth platform would be added without a special case.
	sets[goos] = references

	shared, deltas, _ := splitPlatformReferenceSets(sets)

	return shared, deltas, true
}

// applyPlatformReferenceAdoption is the writeProjectFile-facing wrapper: read the project file this
// write is replacing, and if it carries conditioned reference groups, re-render the freshly
// generated contents so the other platforms' sets survive. Returns newContents unchanged in every
// other case — no existing file, no block, or an unusable template — so a conversion that has
// nothing to adopt emits exactly the project file it always did.
func applyPlatformReferenceAdoption(newContents string, projectFileName string, goos string, references []string) string {
	existing, err := os.ReadFile(projectFileName)

	if err != nil {
		return newContents
	}

	shared, deltas, ok := adoptPlatformReferences(string(existing), goos, references)

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
