// platformProject_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards for layout L3's conditioned <ProjectReference> blocks (platformProject.go, increment 3 of
// docs/phase4/DESIGN-multiplatform-corpus.md).
//
// The invariant every test here circles is the one the increment's acceptance gate rests on: a
// SINGLE-target reconvert of a package whose imports vary by GOOS must reproduce the multi-target
// merge's project file BYTE FOR BYTE. If it cannot, CLAUDE.md's seeded-reconvert ritual starts
// reporting 24 packages as permanently drifted, and the Windows lane's "must not move" gate can
// never read empty again.

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// renderReferenceMarker builds the ProjectReferenceMarker substitution writeProjectFile produces for
// a reference list, so these tests exercise the real emitted shape rather than an approximation.
func renderReferenceMarker(references []string) string {
	builder := &strings.Builder{}

	for _, reference := range references {
		builder.WriteString(fmt.Sprintf("\r\n    <ProjectReference Include=\"%s\" />", reference))
	}

	return builder.String()
}

// flatProjectFile renders a plain converted .csproj carrying one flat reference list.
func flatProjectFile(references []string) string {
	return renderCsprojTemplate("Library", renderReferenceMarker(references), "")
}

var (
	refErrors = "$(go2csPath)core/errors/errors.csproj"
	refIO     = "$(go2csPath)core/io/io.csproj"
	refSysWin = "$(go2csPath)core/internal/syscall/windows/internal.syscall.windows.csproj"
	refSysUnx = "$(go2csPath)core/internal/syscall/unix/internal.syscall.unix.csproj"
	refRoute  = "$(go2csPath)core/vendor/golang.org/x/net/route/vendor.golang.org.x.net.route.csproj"
)

// mergedOSProjectFile is the shape the multi-target merge produces for an `os`-like package: two
// shared references, and a per-GOOS split where darwin adds one more than linux.
func mergedOSProjectFile(t *testing.T) string {
	t.Helper()

	sets := map[string][]string{
		"windows": {refErrors, refIO, refSysWin},
		"linux":   {refErrors, refIO, refSysUnx},
		"darwin":  {refErrors, refIO, refSysUnx, refRoute},
	}

	shared, deltas, varies := splitPlatformReferenceSets(sets)

	if !varies {
		t.Fatalf("splitPlatformReferenceSets reported no variance for sets that plainly differ")
	}

	merged, err := renderPlatformReferences(flatProjectFile(sets["windows"]), shared, deltas)

	if err != nil {
		t.Fatalf("renderPlatformReferences: %v", err)
	}

	return merged
}

func TestSplitPlatformReferenceSetsSharesOnlyTheIntersection(t *testing.T) {
	shared, deltas, varies := splitPlatformReferenceSets(map[string][]string{
		"windows": {refErrors, refIO, refSysWin},
		"linux":   {refErrors, refIO, refSysUnx},
		"darwin":  {refErrors, refIO, refSysUnx, refRoute},
	})

	if !varies {
		t.Fatalf("varies = false, want true")
	}

	if got, want := strings.Join(shared, ","), strings.Join(sortedUniqueStrings([]string{refErrors, refIO}), ","); got != want {
		t.Errorf("shared = %v, want the two references common to every platform (%v)", shared, want)
	}

	// The dangerous direction is a delta leaking into the shared list: a Windows-only reference
	// promoted to shared would land in the Linux build's closure.
	for _, reference := range shared {
		if reference == refSysWin || reference == refSysUnx || reference == refRoute {
			t.Errorf("platform-specific reference %q was promoted into the shared list", reference)
		}
	}

	if got, want := strings.Join(deltas["windows"], ","), refSysWin; got != want {
		t.Errorf("windows delta = %q, want %q", got, want)
	}

	if len(deltas["darwin"]) != 2 {
		t.Errorf("darwin delta = %v, want both its unix and route references", deltas["darwin"])
	}
}

func TestConditionedProjectFileIsWellFormedAndRoundTrips(t *testing.T) {
	merged := mergedOSProjectFile(t)

	if err := assertWellFormedXml(merged); err != nil {
		t.Fatalf("conditioned project file is not well-formed XML: %v\n%s", err, merged)
	}

	for _, platform := range []string{"windows", "linux", "darwin"} {
		want := map[string][]string{
			"windows": {refErrors, refIO, refSysWin},
			"linux":   {refErrors, refIO, refSysUnx},
			"darwin":  {refErrors, refIO, refSysUnx, refRoute},
		}[platform]

		got, ok := platformFullReferenceSet(merged, platform)

		if !ok {
			t.Fatalf("platformFullReferenceSet(%s) could not read the merged project file", platform)
		}

		if strings.Join(got, ",") != strings.Join(sortedUniqueStrings(want), ",") {
			t.Errorf("%s reads back %v, want %v", platform, got, want)
		}
	}
}

// The increment's load-bearing invariant, stated as a test: a single-target reconvert that computes
// the same reference set must reproduce the merged file byte for byte — for EVERY platform, not just
// the one the merge happened to base its remainder on.
func TestSingleTargetReconvertReproducesMergedProjectFile(t *testing.T) {
	merged := mergedOSProjectFile(t)

	sets := map[string][]string{
		"windows": {refErrors, refIO, refSysWin},
		"linux":   {refErrors, refIO, refSysUnx},
		"darwin":  {refErrors, refIO, refSysUnx, refRoute},
	}

	for platform, references := range sets {
		// What this target's own conversion would emit, with no knowledge of the platform axis.
		fresh := flatProjectFile(sortedUniqueStrings(references))

		shared, deltas, ok := adoptPlatformReferences(merged, platform, sortedUniqueStrings(references))

		if !ok {
			t.Fatalf("%s: adoptPlatformReferences declined a project file that carries a block", platform)
		}

		rebuilt, err := renderPlatformReferences(fresh, shared, deltas)

		if err != nil {
			t.Fatalf("%s: renderPlatformReferences: %v", platform, err)
		}

		if rebuilt != merged {
			t.Errorf("%s reconvert did not reproduce the merged project file\n--- merged ---\n%s\n--- rebuilt ---\n%s",
				platform, merged, rebuilt)
		}
	}
}

// A platform whose delta is empty still needs a group, or the next reconvert forgets it takes part
// and promotes the other platforms' references into the shared list.
func TestEmptyDeltaKeepsAxisMembership(t *testing.T) {
	sets := map[string][]string{
		"windows": {refErrors, refSysWin},
		"linux":   {refErrors},
		"darwin":  {refErrors},
	}

	shared, deltas, _ := splitPlatformReferenceSets(sets)
	merged, err := renderPlatformReferences(flatProjectFile(sets["windows"]), shared, deltas)

	if err != nil {
		t.Fatalf("renderPlatformReferences: %v", err)
	}

	if !strings.Contains(merged, "<ItemGroup Condition=\"'$(GoTargetOS)'=='linux'\" />") {
		t.Fatalf("an empty linux delta did not emit a self-closing membership group:\n%s", merged)
	}

	recovered, ok := parsePlatformReferenceGroups(merged)

	if !ok {
		t.Fatalf("parsePlatformReferenceGroups did not find the block it just wrote")
	}

	for _, platform := range []string{"windows", "linux", "darwin"} {
		if _, present := recovered[platform]; !present {
			t.Errorf("%s lost its axis membership across a round-trip", platform)
		}
	}

	// The whole point: a linux reconvert must NOT pull the Windows-only reference into the shared
	// list just because linux itself has nothing extra to say.
	rebuiltShared, _, ok := adoptPlatformReferences(merged, "linux", []string{refErrors})

	if !ok {
		t.Fatalf("adoptPlatformReferences declined the merged file")
	}

	for _, reference := range rebuiltShared {
		if reference == refSysWin {
			t.Fatalf("a linux reconvert promoted the Windows-only reference into the shared list")
		}
	}
}

// When the platforms stop disagreeing the block goes away, rather than lingering as inert XML.
func TestConvergedImportSetsDropTheBlock(t *testing.T) {
	merged := mergedOSProjectFile(t)

	shared, deltas, ok := adoptPlatformReferences(merged, "windows", []string{refErrors, refIO, refSysUnx, refRoute})

	if !ok {
		t.Fatalf("adoptPlatformReferences declined the merged file")
	}

	// windows now matches darwin, but linux still lacks route — so the block must SURVIVE here.
	if composePlatformReferenceGroups(deltas) == "" {
		t.Fatalf("the block was dropped while linux still differs (shared = %v)", shared)
	}

	all := []string{refErrors, refIO}
	converged, deltas, _ := splitPlatformReferenceSets(map[string][]string{"windows": all, "linux": all, "darwin": all})
	collapsed, err := renderPlatformReferences(merged, converged, deltas)

	if err != nil {
		t.Fatalf("renderPlatformReferences: %v", err)
	}

	if strings.Contains(collapsed, platformReferenceBlockHeader) {
		t.Fatalf("an axis with no differences kept its conditioned block:\n%s", collapsed)
	}

	if collapsed != flatProjectFile(all) {
		t.Errorf("a collapsed axis did not return to the plain project file\n--- want ---\n%s\n--- got ---\n%s",
			flatProjectFile(all), collapsed)
	}
}

// renderPlatformReferences replaces rather than accumulates, so running the merge twice over its own
// output is a no-op — the property the corpus-wide emission's idempotency rests on.
func TestRenderPlatformReferencesIsIdempotent(t *testing.T) {
	merged := mergedOSProjectFile(t)

	shared, ok := parseSharedProjectReferences(merged)

	if !ok {
		t.Fatalf("parseSharedProjectReferences could not read the merged file")
	}

	deltas, ok := parsePlatformReferenceGroups(merged)

	if !ok {
		t.Fatalf("parsePlatformReferenceGroups could not read the merged file")
	}

	again, err := renderPlatformReferences(merged, shared, deltas)

	if err != nil {
		t.Fatalf("renderPlatformReferences: %v", err)
	}

	if again != merged {
		t.Errorf("re-rendering a merged project file changed it\n--- first ---\n%s\n--- second ---\n%s", merged, again)
	}
}

// A project file with no conditioned block must be left exactly as the conversion produced it —
// that is 280 of the 304 packages, and any drift there fails the Windows-identity gate.
func TestUnconditionedProjectFileIsUntouched(t *testing.T) {
	flat := flatProjectFile([]string{refErrors, refIO})
	dir := t.TempDir()
	projectFile := filepath.Join(dir, "os.csproj")

	if err := os.WriteFile(projectFile, []byte(flat), 0644); err != nil {
		t.Fatalf("write: %v", err)
	}

	if got := applyPlatformReferenceAdoption(flat, projectFile, "windows", []string{refErrors, refIO}); got != flat {
		t.Errorf("adoption modified a project file that carries no conditioned block")
	}

	// And a project file that does not exist yet (a first conversion) is equally untouched.
	if got := applyPlatformReferenceAdoption(flat, filepath.Join(dir, "absent.csproj"), "windows", nil); got != flat {
		t.Errorf("adoption modified the contents when there was no previous project file to adopt from")
	}
}

// stripPlatformReferences is how the merge tells a reference difference from any OTHER difference
// between two targets' project files; it must ignore the former and preserve the latter.
func TestStripPlatformReferencesIsolatesTheReferenceLists(t *testing.T) {
	windows := flatProjectFile([]string{refErrors, refSysWin})
	linux := flatProjectFile([]string{refErrors, refSysUnx})

	if stripPlatformReferences(windows) != stripPlatformReferences(linux) {
		t.Errorf("project files differing ONLY in their references did not strip to the same remainder")
	}

	unsafeLinux := strings.Replace(linux, "<AllowUnsafeBlocks>false</AllowUnsafeBlocks>", "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>", 1)

	if unsafeLinux == linux {
		t.Fatalf("test setup: the AllowUnsafeBlocks line was not found in the rendered template")
	}

	if stripPlatformReferences(windows) == stripPlatformReferences(unsafeLinux) {
		t.Errorf("a non-reference difference survived neither strip — the merge would absorb it silently")
	}
}

// <AllowUnsafeBlocks> is a per-package emission fact that can differ by platform (measured: os/user
// and syscall). One .csproj serves every platform, so it must carry the UNION — a `false` inherited
// from whichever target the merge read first makes the platform that needs unsafe uncompilable.
func TestUnsafeBlocksTakesTheUnionInEitherDirection(t *testing.T) {
	safe := flatProjectFile([]string{refErrors})
	unsafeProject := strings.Replace(safe, unsafeBlocksDisabled, unsafeBlocksEnabled, 1)

	if unsafeProject == safe {
		t.Fatalf("test setup: %q was not found in the rendered template", unsafeBlocksDisabled)
	}

	// The corpus's real polarity: Windows needs unsafe, the unix side does not. Union = enabled.
	if got := unsafeBlocksUnion(unsafeProject, true); !strings.Contains(got, unsafeBlocksEnabled) {
		t.Errorf("union lost the enabled setting the base already carried")
	}

	// The polarity the corpus does NOT currently have, and the one that would silently break a
	// Linux build if the merge simply kept the first target's remainder.
	if got := unsafeBlocksUnion(safe, true); !strings.Contains(got, unsafeBlocksEnabled) {
		t.Errorf("a base emitted without unsafe was not raised for a target that needs it:\n%s", got)
	}

	// And a package no platform needs unsafe for is left exactly as it was.
	if got := unsafeBlocksUnion(safe, false); got != safe {
		t.Errorf("union modified a project file no target needs unsafe for")
	}
}

// platformPackageInfoPath is the READING half of L3: 27 packages' package_info.cs move into a
// per-GOOS folder, and every dependent's conversion reads them.
func TestPlatformPackageInfoPathResolution(t *testing.T) {
	root := t.TempDir()

	shared := filepath.Join(root, "shared")
	varying := filepath.Join(root, "varying")
	nested := filepath.Join(root, "nested")

	for _, dir := range []string{
		shared,
		filepath.Join(varying, "windows"),
		filepath.Join(varying, "linux"),
		filepath.Join(nested, "windows"),
	} {
		if err := os.MkdirAll(dir, 0755); err != nil {
			t.Fatalf("mkdir: %v", err)
		}
	}

	write := func(path string, contents string) {
		if err := os.WriteFile(path, []byte(contents), 0644); err != nil {
			t.Fatalf("write %s: %v", path, err)
		}
	}

	// A package whose metadata is shared: flat, and flat wins even for a named GOOS.
	write(filepath.Join(shared, PackageInfoFileName), "shared")
	write(filepath.Join(shared, "shared.csproj"), "<Project />")

	// A package whose metadata varies: per-GOOS only.
	write(filepath.Join(varying, "varying.csproj"), "<Project />")
	write(filepath.Join(varying, "windows", "goos.cs"), "// windows")
	write(filepath.Join(varying, "windows", PackageInfoFileName), "windows")
	write(filepath.Join(varying, "linux", "goos.cs"), "// linux")
	write(filepath.Join(varying, "linux", PackageInfoFileName), "linux")

	// A NESTED PACKAGE whose own directory name is a GOOS — internal/syscall/windows. Its
	// package_info.cs is its own, and must never be read as the parent's Windows variant.
	write(filepath.Join(nested, "nested.csproj"), "<Project />")
	write(filepath.Join(nested, "windows", "windows.csproj"), "<Project />")
	write(filepath.Join(nested, "windows", PackageInfoFileName), "nested-package")

	read := func(dir string, goos string) string {
		data, err := os.ReadFile(platformPackageInfoPath(dir, goos))

		if err != nil {
			return "<absent>"
		}

		return string(data)
	}

	if got := read(shared, "linux"); got != "shared" {
		t.Errorf("shared package resolved to %q, want the flat copy", got)
	}

	if got := read(varying, "linux"); got != "linux" {
		t.Errorf("varying package at linux resolved to %q", got)
	}

	if got := read(varying, "windows"); got != "windows" {
		t.Errorf("varying package at windows resolved to %q", got)
	}

	if got := read(nested, "windows"); got != "<absent>" {
		t.Errorf("a nested PACKAGE named windows was read as the parent's Windows metadata (%q)", got)
	}
}
