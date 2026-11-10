// platformLatticeLayout_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"io/fs"
	"math/rand"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"testing"
)

// Guards for the layout lattice as the converter APPLIES it (platformLattice.go is the rule itself, guarded by
// platformLattice_test.go): the multi-target merge, the duplicate reconcile, the single-target writer's probes, the
// arch block of the project file, the reference lattice and the hand-own router. Every four-target case here is
// reachable only from a test: the declared universe is today's three targets, where each of these must write
// exactly what it wrote before the arch axis (the three-target cases below, and the corpus's own project files).

// listCorpusFiles returns every file under dir as a sorted slash path relative to it.
func listCorpusFiles(t *testing.T, dir string) []string {
	t.Helper()

	var files []string

	err := filepath.WalkDir(dir, func(filePath string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if !entry.IsDir() {
			rel, err := filepath.Rel(dir, filePath)

			if err != nil {
				return err
			}

			files = append(files, filepath.ToSlash(rel))
		}

		return nil
	})

	if err != nil {
		t.Fatalf("failed to list %q: %v", dir, err)
	}

	sort.Strings(files)

	return files
}

// readLatticeCorpusFile reads a file under coreDir, or "<absent>".
func readLatticeCorpusFile(coreDir string, rel string) string {
	contents, err := os.ReadFile(filepath.Join(coreDir, filepath.FromSlash(rel)))

	if err != nil {
		return "<absent>"
	}

	return string(contents)
}

// runLatticeMerge seeds a corpus root, stages one emission per target (emitted maps a logical path to each emitting
// target's bytes), and runs the merge over exactly those targets. It returns the corpus's core directory.
func runLatticeMerge(t *testing.T, targets []string, seed map[string]string, emitted map[string]map[string]string) (string, error) {
	t.Helper()

	root := t.TempDir()
	coreDir := filepath.Join(root, "core")

	for rel, contents := range seed {
		writeTestFile(t, filepath.Join(coreDir, filepath.FromSlash(rel)), contents)
	}

	var emissions []*platformEmission

	for _, target := range targets {
		stage := t.TempDir()
		artifacts := map[string]artifactState{}

		for logical, byTarget := range emitted {
			contents, ok := byTarget[target]

			if !ok {
				continue
			}

			writeTestFile(t, filepath.Join(stage, "core", filepath.FromSlash(logical)), contents)
			artifacts[logical] = artifactState{hash: contents, emitted: true}
		}

		emissions = append(emissions, &platformEmission{target: target, goos: goosOfTarget(target), goarch: archOfTarget(target), root: stage, artifacts: artifacts})
	}

	return coreDir, mergePlatformEmissions(root, targets, emissions)
}

func assertCorpusFiles(t *testing.T, coreDir string, pkg string, want map[string]string) {
	t.Helper()

	got := listCorpusFiles(t, filepath.Join(coreDir, pkg))
	var wantFiles []string

	for rel := range want {
		wantFiles = append(wantFiles, rel)
	}

	sort.Strings(wantFiles)

	if strings.Join(got, "\n") != strings.Join(wantFiles, "\n") {
		t.Errorf("%s holds\n  %s\nwant\n  %s", pkg, strings.Join(got, "\n  "), strings.Join(wantFiles, "\n  "))
	}

	for rel, contents := range want {
		if len(contents) == 0 {
			continue // a project file, checked by its own assertion
		}

		if got := readLatticeCorpusFile(coreDir, pkg+"/"+rel); got != contents {
			t.Errorf("%s/%s = %q, want %q", pkg, rel, got, contents)
		}
	}
}

// The merge at four targets, one package per presence and content shape of the design's §2.1 table, over a seed that
// plants stale copies where the rule no longer homes their names: an unadmitted `arm64/`, a `linux/` copy both linux
// targets now emit apart, a GOOS copy of a name that became shared, and a flat copy of a name that became per-GOOS.
func TestMergeAtFourTargetsHomesEachContentClass(t *testing.T) {
	const w, l, d, a = "windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"

	template := string(csprojTemplate)

	seed := map[string]string{
		"p/p.csproj":          template,
		"p/arm64/amd.cs":      "stale, unadmitted arm64 folder",
		"p/linux/four.cs":     "stale, linux folder both linux targets now emit apart",
		"p/windows/shared.cs": "stale, per-GOOS copy of a name now shared",
		"p/lx.cs":             "stale, flat copy of a name now per-GOOS",
		// windows does not emit lonly.cs: what its own folder holds under that name is its business (a per-GOOS hand-own
		// its conversion diverted), not a stale copy of linux/amd64's emission.
		"p/windows/lonly.cs": "windows's own",
		// A package with no per-platform source holding a NESTED PACKAGE named like a key: never written into or
		// removed from, and no refusal, since it carries no layout block.
		"r/r.csproj":           template,
		"r/amd64/amd64.csproj": "<Project />",
		"r/amd64/same.cs":      "the nested package's own",
	}

	emitted := map[string]map[string]string{
		"p/shared.cs": {w: "shared", l: "shared", d: "shared", a: "shared"},
		"p/amd.cs":    {w: "amd", l: "amd", d: "amd", a: "amd-arm"},
		"p/lx.cs":     {l: "lx", a: "lx"},
		"p/four.cs":   {w: "four-w", l: "four-l", d: "four-d", a: "four-a"},
		"p/lonly.cs":  {l: "lonly"},
		"r/same.cs":   {w: "same", l: "same", d: "same", a: "same"},
	}

	coreDir, err := runLatticeMerge(t, universeArm, seed, emitted)

	if err != nil {
		t.Fatalf("merge: %v", err)
	}

	assertCorpusFiles(t, coreDir, "p", map[string]string{
		"p.csproj":             "",
		"shared.cs":            "shared",
		"amd64/amd.cs":         "amd",
		"linux-arm64/amd.cs":   "amd-arm",
		"linux/lx.cs":          "lx",
		"windows/four.cs":      "four-w",
		"linux-amd64/four.cs":  "four-l",
		"darwin/four.cs":       "four-d",
		"linux-arm64/four.cs":  "four-a",
		"linux-amd64/lonly.cs": "lonly",
		"windows/lonly.cs":     "windows's own",
	})

	assertCorpusFiles(t, coreDir, "r", map[string]string{
		"r.csproj":           "",
		"same.cs":            "same",
		"amd64/amd64.csproj": "<Project />",
		"amd64/same.cs":      "the nested package's own",
	})

	// p now holds arch and pair folders, so its project file carries the L3 block and the arch block; r's is untouched.
	if got, want := readLatticeCorpusFile(coreDir, "p/p.csproj"), applyPlatformArchLayoutBlocks(applyPlatformLayoutBlocks(template, "p.csproj"), "p.csproj"); got != want {
		t.Errorf("p.csproj does not carry exactly the L3 and arch blocks:\n%s", got)
	}

	if got := readLatticeCorpusFile(coreDir, "r/r.csproj"); got != template {
		t.Errorf("r.csproj changed, but r carries no per-platform source:\n%s", got)
	}
}

// The same harness at today's three targets: whatever the shapes, only flat and GOOS folders are ever written, and
// the project file takes exactly the L3 block it always did, with no arch block.
func TestMergeAtThreeTargetsWritesOnlyFlatAndGOOSFolders(t *testing.T) {
	const w, l, d = "windows/amd64", "linux/amd64", "darwin/amd64"

	template := string(csprojTemplate)

	seed := map[string]string{
		"p/p.csproj":          template,
		"p/windows/shared.cs": "stale, per-GOOS copy of a name now shared",
		"p/lx.cs":             "stale, flat copy of a name now per-GOOS",
		"p/linux/four.cs":     "stale linux bytes",
	}

	emitted := map[string]map[string]string{
		"p/shared.cs": {w: "shared", l: "shared", d: "shared"},
		"p/amd.cs":    {w: "amd", l: "amd", d: "amd"},
		"p/lx.cs":     {l: "lx"},
		"p/wd.cs":     {w: "wd", d: "wd"},
		"p/four.cs":   {w: "four-w", l: "four-l", d: "four-d"},
		"p/two.cs":    {w: "two", l: "two", d: "two-d"},
	}

	coreDir, err := runLatticeMerge(t, universeToday, seed, emitted)

	if err != nil {
		t.Fatalf("merge: %v", err)
	}

	assertCorpusFiles(t, coreDir, "p", map[string]string{
		"p.csproj":        "",
		"shared.cs":       "shared",
		"amd.cs":          "amd",
		"linux/lx.cs":     "lx",
		"windows/wd.cs":   "wd",
		"darwin/wd.cs":    "wd",
		"windows/four.cs": "four-w",
		"linux/four.cs":   "four-l",
		"darwin/four.cs":  "four-d",
		"windows/two.cs":  "two",
		"linux/two.cs":    "two",
		"darwin/two.cs":   "two-d",
	})

	for _, rel := range listCorpusFiles(t, filepath.Join(coreDir, "p")) {
		if folder, _, nested := strings.Cut(rel, "/"); nested && !isKnownGOOS(folder) {
			t.Errorf("a three-target merge wrote %s outside flat and the GOOS folders", rel)
		}
	}

	if got, want := readLatticeCorpusFile(coreDir, "p/p.csproj"), applyPlatformLayoutBlocks(template, "p.csproj"); got != want {
		t.Errorf("p.csproj is not exactly today's L3 project file:\n%s", got)
	}
}

// A package that would carry the layout block while holding a nested PACKAGE named like a key is refused, before
// anything is written: its `$(GoTargetOS)/*.cs` glob would compile the child's sources into the parent.
func TestMergeRefusesANestedPackageNamedLikeAKey(t *testing.T) {
	const w, l, d, a = "windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"

	seed := map[string]string{
		"s/s.csproj":           string(csprojTemplate),
		"s/linux/linux.csproj": "<Project />",
		"s/linux/child.cs":     "the nested package's own",
	}

	emitted := map[string]map[string]string{
		"s/v.cs": {w: "v-w", l: "v-l", d: "v-d", a: "v-a"},
	}

	coreDir, err := runLatticeMerge(t, universeArm, seed, emitted)

	if err == nil {
		t.Fatal("a package with per-platform sources and a nested package named linux was merged")
	}

	if !strings.Contains(err.Error(), "core/s/linux") {
		t.Errorf("the refusal does not name the nested package's folder: %v", err)
	}

	if got := readLatticeCorpusFile(coreDir, "s/windows/v.cs"); got != "<absent>" {
		t.Errorf("the refused merge wrote s/windows/v.cs before refusing")
	}
}

// reconcileLayoutDuplicates by cover containment, each row of the design's §2.4 table at four targets.
func TestReconcileLayoutDuplicatesByCoverAtFourTargets(t *testing.T) {
	rows := []struct {
		name      string
		files     map[string]string
		removed   []string // package-relative paths that must go
		conflict  bool
		mentioned []string // package-relative paths the error must name
	}{
		{name: "disjoint covers differ", files: map[string]string{"windows/x.cs": "a", "linux/x.cs": "b"}},
		{name: "GOOS contains pair, identical", files: map[string]string{"linux/x.cs": "a", "linux-arm64/x.cs": "a"}, removed: []string{"linux-arm64/x.cs"}},
		{name: "flat contains arch, identical", files: map[string]string{"x.cs": "a", "amd64/x.cs": "a"}, removed: []string{"amd64/x.cs"}},
		// The narrower COVER goes, not the later kind: amd64/ still serves windows when windows/ is removed.
		{name: "arch contains GOOS, identical", files: map[string]string{"amd64/x.cs": "a", "windows/x.cs": "a"}, removed: []string{"windows/x.cs"}},
		{name: "equal covers, identical", files: map[string]string{"arm64/x.cs": "a", "linux-arm64/x.cs": "a"}, removed: []string{"linux-arm64/x.cs"}},
		{name: "nested covers differ", files: map[string]string{"linux/x.cs": "a", "linux-arm64/x.cs": "b"}, conflict: true, mentioned: []string{"linux/x.cs", "linux-arm64/x.cs"}},
		{name: "overlapping covers, identical", files: map[string]string{"linux/x.cs": "a", "amd64/x.cs": "a"}, conflict: true, mentioned: []string{"linux/x.cs", "amd64/x.cs"}},
		{name: "flat contains a foreign GOOS folder", files: map[string]string{"x.cs": "a", "freebsd/x.cs": "a"}, removed: []string{"freebsd/x.cs"}},
		{name: "two empty covers", files: map[string]string{"freebsd/x.cs": "a", "plan9/x.cs": "b"}},
		{name: "a nested package is not a key folder", files: map[string]string{"x.cs": "a", "amd64/amd64.csproj": "<Project />", "amd64/x.cs": "a"}},
	}

	for _, row := range rows {
		coreDir := filepath.Join(t.TempDir(), "core")
		writeTestFile(t, filepath.Join(coreDir, "k", "k.csproj"), "<Project />")

		for rel, contents := range row.files {
			writeTestFile(t, filepath.Join(coreDir, "k", filepath.FromSlash(rel)), contents)
		}

		removed, err := reconcileLayoutDuplicates(coreDir, universeArm)

		if row.conflict != (err != nil) {
			t.Errorf("%s: error = %v, want a conflict: %v", row.name, err, row.conflict)
		}

		for _, rel := range row.mentioned {
			if err != nil && !strings.Contains(err.Error(), "core/k/"+rel) {
				t.Errorf("%s: the conflict does not name core/k/%s:\n%v", row.name, rel, err)
			}
		}

		if removed != len(row.removed) {
			t.Errorf("%s: removed %d, want %d", row.name, removed, len(row.removed))
		}

		gone := map[string]bool{}

		for _, rel := range row.removed {
			gone[rel] = true
		}

		for rel := range row.files {
			if present := readLatticeCorpusFile(coreDir, "k/"+rel) != "<absent>"; present == gone[rel] {
				t.Errorf("%s: k/%s present = %v, want %v", row.name, rel, present, !gone[rel])
			}
		}
	}
}

// The single-target writer's probes: `<goos>-<arch>/`, then `<arch>/`, then `<goos>/`, then flat; a bare GOOS
// probes its GOOS folder alone.
func TestPlatformLayoutProbesPairThenArchThenGOOS(t *testing.T) {
	packageDir := filepath.Join(t.TempDir(), "p")
	writeTestFile(t, filepath.Join(packageDir, "p.csproj"), "<Project />")

	for _, folder := range []string{"linux-arm64", "arm64", "linux", ""} {
		writeTestFile(t, filepath.Join(packageDir, folder, "x.cs"), folder)
		writeTestFile(t, filepath.Join(packageDir, folder, PackageInfoFileName), folder)
	}

	for _, step := range []struct{ remove, want string }{
		{"", "linux-arm64"},
		{"linux-arm64", "arm64"},
		{"arm64", "linux"},
		{"linux", ""},
	} {
		if len(step.remove) > 0 {
			if err := os.Remove(filepath.Join(packageDir, step.remove, "x.cs")); err != nil {
				t.Fatal(err)
			}
		}

		if got, want := platformLayoutDir(packageDir, "linux/arm64", "x.cs"), filepath.Join(packageDir, step.want); got != want {
			t.Errorf("after removing %q: platformLayoutDir(linux/arm64) = %q, want %q", step.remove, got, want)
		}
	}

	// The bare GOOS: linux/ alone, though the pair and arch folders still hold package_info.cs.
	if got, want := platformLayoutPath(packageDir, "linux", PackageInfoFileName), filepath.Join(packageDir, "linux", PackageInfoFileName); got != want {
		t.Errorf("platformLayoutPath(linux) = %q, want %q", got, want)
	}

	if got, want := platformLayoutPath(packageDir, "linux/arm64", PackageInfoFileName), filepath.Join(packageDir, "linux-arm64", PackageInfoFileName); got != want {
		t.Errorf("platformLayoutPath(linux/arm64) = %q, want %q", got, want)
	}

	// package_info.cs for a consumer: flat wins whenever it exists, then pair > arch > GOOS.
	if got, want := platformPackageInfoPath(packageDir, "linux/arm64"), filepath.Join(packageDir, PackageInfoFileName); got != want {
		t.Errorf("platformPackageInfoPath with a flat copy = %q, want %q", got, want)
	}

	if err := os.Remove(filepath.Join(packageDir, PackageInfoFileName)); err != nil {
		t.Fatal(err)
	}

	for _, step := range []struct{ target, want string }{
		{"linux/arm64", "linux-arm64"},
		{"linux/amd64", "linux"},
		{"linux", "linux"},
		{"windows/amd64", ""},
	} {
		if got, want := platformPackageInfoPath(packageDir, step.target), filepath.Join(packageDir, step.want, PackageInfoFileName); got != want {
			t.Errorf("platformPackageInfoPath(%s) = %q, want %q", step.target, got, want)
		}
	}

	// A review sibling follows the same probe.
	writeTestFile(t, filepath.Join(packageDir, "arm64", "h.cs.auto"), "arm64 flavour")

	if got, want := reviewSiblingLayoutPath(packageDir, "linux/arm64", "h"), filepath.Join(packageDir, "arm64", "h.cs.auto"); got != want {
		t.Errorf("reviewSiblingLayoutPath(linux/arm64) = %q, want %q", got, want)
	}

	if got, want := reviewSiblingLayoutPath(packageDir, "linux", "h"), filepath.Join(packageDir, "h.cs.auto"); got != want {
		t.Errorf("reviewSiblingLayoutPath(linux) = %q, want %q", got, want)
	}

	// productionCSFiles takes every key folder the target compiles, and only those.
	for _, folder := range []string{"linux-arm64", "arm64", "linux", "linux-amd64", "amd64", "windows"} {
		writeTestFile(t, filepath.Join(packageDir, folder, "y.cs"), folder)
	}

	files, err := productionCSFiles(packageDir, "linux/arm64")

	if err != nil {
		t.Fatal(err)
	}

	if got, want := strings.Join(files, " "), "arm64/y.cs linux-arm64/y.cs linux/y.cs x.cs"; got != want {
		t.Errorf("productionCSFiles(linux/arm64) = %q, want %q", got, want)
	}

	if files, err := productionCSFiles(packageDir, "linux"); err != nil || strings.Join(files, " ") != "linux/y.cs x.cs" {
		t.Errorf("productionCSFiles(linux) = %v, %v: a bare GOOS takes its GOOS folder alone", files, err)
	}
}

// The arch block: added only with an arch or pair folder, after its GOOS counterparts, idempotent, and removable back
// to the input bytes; a package with GOOS folders alone keeps exactly today's text.
func TestPlatformArchLayoutBlock(t *testing.T) {
	plain := string(csprojTemplate)
	l3 := applyPlatformLayoutBlocks(plain, "p.csproj")
	arch := applyPlatformArchLayoutBlocks(l3, "p.csproj")

	if arch == l3 {
		t.Fatal("the arch block was not applied to an L3 project file")
	}

	if again := applyPlatformArchLayoutBlocks(arch, "p.csproj"); again != arch {
		t.Error("applyPlatformArchLayoutBlocks is not idempotent")
	}

	if got := removePlatformArchLayoutBlocks(arch); got != l3 {
		t.Error("removing the arch block did not restore the L3 project file byte for byte")
	}

	if got := removePlatformLayoutBlocks(arch); got != plain {
		t.Error("removing the layout blocks did not restore the plain project file byte for byte")
	}

	// The raw template still holds its markers; a rendered one is the file a build reads.
	rendered := flatProjectFile([]string{refErrors})

	if err := assertWellFormedXml(applyPlatformArchLayoutBlocks(applyPlatformLayoutBlocks(rendered, "p.csproj"), "p.csproj")); err != nil {
		t.Fatalf("the arch project file is not well-formed: %v", err)
	}

	order := []string{
		"<GoTargetOS>windows</GoTargetOS>",
		"<GoTargetArch>amd64</GoTargetArch>",
		"<Compile Remove=\"**/*.cs\" />",
		"<Compile Include=\"package_info.cs\"",
		"<Compile Include=\"$(GoTargetOS)/package_info.cs\"",
		"<Compile Include=\"$(GoTargetArch)/package_info.cs\"",
		"<Compile Include=\"$(GoTargetOS)-$(GoTargetArch)/package_info.cs\"",
		"<Compile Include=\"*.cs\"",
		"<Compile Include=\"$(GoTargetOS)/*.cs\"",
		"<Compile Include=\"$(GoTargetArch)/*.cs\"",
		"<Compile Include=\"$(GoTargetOS)-$(GoTargetArch)/*.cs\"",
		"<Compile Remove=\"*_test.cs;",
	}

	last := -1

	for _, item := range order {
		at := strings.Index(arch, item)

		if at < 0 || at < last {
			t.Errorf("%s is missing or out of order in the arch project file", item)
		}

		last = at
	}

	// Through the merge's predicate: GOOS folders alone take today's text, an arch folder adds the block, and the
	// arch folder's going takes it out again.
	packageDir := filepath.Join(t.TempDir(), "p")
	projectFile := filepath.Join(packageDir, "p.csproj")
	writeTestFile(t, projectFile, plain)
	writeTestFile(t, filepath.Join(packageDir, "linux", "x.cs"), "linux")

	if _, err := applyLayoutBlockToProjectFile(packageDir); err != nil {
		t.Fatal(err)
	}

	if got := readLatticeCorpusFile(filepath.Dir(packageDir), "p/p.csproj"); got != l3 {
		t.Errorf("a package with GOOS folders alone does not carry exactly today's L3 project file:\n%s", got)
	}

	writeTestFile(t, filepath.Join(packageDir, "linux-arm64", "y.cs"), "linux/arm64")

	if _, err := applyLayoutBlockToProjectFile(packageDir); err != nil {
		t.Fatal(err)
	}

	if got := readLatticeCorpusFile(filepath.Dir(packageDir), "p/p.csproj"); got != arch {
		t.Errorf("a package with a pair folder does not carry the arch block:\n%s", got)
	}

	if err := os.RemoveAll(filepath.Join(packageDir, "linux-arm64")); err != nil {
		t.Fatal(err)
	}

	if _, err := applyLayoutBlockToProjectFile(packageDir); err != nil {
		t.Fatal(err)
	}

	if got := readLatticeCorpusFile(filepath.Dir(packageDir), "p/p.csproj"); got != l3 {
		t.Errorf("the arch block outlived the package's last arch folder:\n%s", got)
	}
}

// The shared build check of the arch axis, rendered from a universe.
func TestRenderPlatformUniverseCheck(t *testing.T) {
	check := renderPlatformUniverseCheck(universeArm)

	if err := assertWellFormedXml("<Project>\r\n" + check + "</Project>\r\n"); err != nil {
		t.Fatalf("the check is not well-formed: %v\n%s", err, check)
	}

	want := "<Error Condition=\"'$(_GoLayoutTarget)'!='windows/amd64' And '$(_GoLayoutTarget)'!='linux/amd64' And " +
		"'$(_GoLayoutTarget)'!='darwin/amd64' And '$(_GoLayoutTarget)'!='linux/arm64'\""

	for _, fragment := range []string{
		want,
		"<Target Name=\"GoCheckLayoutTarget\" BeforeTargets=\"CoreCompile\">",
		"<_GoLayoutOS Condition=\"'$(_GoLayoutOS)'==''\">windows</_GoLayoutOS>",
		"<_GoLayoutArch Condition=\"'$(_GoLayoutArch)'==''\">amd64</_GoLayoutArch>",
		"(windows/amd64, linux/amd64, darwin/amd64, linux/arm64)",
	} {
		if !strings.Contains(check, fragment) {
			t.Errorf("the check does not carry %q:\n%s", fragment, check)
		}
	}
}

// The corpus's shared build file carries the check exactly as the declared universe renders it, once: a target joins
// through the constant, the corpus apply and this block together. Line endings are the checkout's, so both sides are
// read with them normalised.
func TestCorpusBuildFileCarriesTheUniverseCheck(t *testing.T) {
	const buildFile = "Directory.Build.props"

	normalise := func(text string) string { return strings.ReplaceAll(text, "\r\n", "\n") }

	contents := normalise(readLatticeCorpusFile(filepath.Join("..", "core"), buildFile))
	want := normalise(renderPlatformUniverseCheck(platformUniverse))

	if got := strings.Count(contents, want); got != 1 {
		t.Errorf("src/core/%s carries the universe check for %q %d times; want exactly once:\n%s", buildFile, platformUniverse, got, want)
	}

	if got := strings.Count(contents, "<Target Name=\"GoCheckLayoutTarget\""); got != 1 {
		t.Errorf("src/core/%s declares GoCheckLayoutTarget %d times; want exactly once", buildFile, got)
	}
}

// The reference lattice reproduces, byte for byte, every conditioned project file the committed corpus carries: the
// text the converter renders over the declared universe. Read as the multi-target merge reads it (each target's full
// set, re-split and re-rendered) and as a single-target reconvert reads it (each target adopting its own set).
func TestPlatformReferenceLatticeReproducesTheCorpusBlocks(t *testing.T) {
	coreDir := filepath.Join("..", "core")
	var projects []string

	err := filepath.WalkDir(coreDir, func(filePath string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if entry.IsDir() {
			if isBuildOutputDirectory(entry.Name()) {
				return filepath.SkipDir
			}

			return nil
		}

		if strings.EqualFold(filepath.Ext(filePath), ".csproj") {
			if contents, err := os.ReadFile(filePath); err == nil && strings.Contains(string(contents), platformReferenceBlockHeader) {
				projects = append(projects, filePath)
			}
		}

		return nil
	})

	if err != nil {
		t.Fatalf("walk %s: %v", coreDir, err)
	}

	if len(projects) == 0 {
		t.Fatalf("no committed project file under %s carries a conditioned reference block; the control is empty", coreDir)
	}

	t.Logf("%d committed project files carry a conditioned reference block", len(projects))

	for _, projectFile := range projects {
		raw, err := os.ReadFile(projectFile)

		if err != nil {
			t.Fatal(err)
		}

		contents := string(raw)
		sets := map[string][]string{}

		for _, target := range platformUniverse {
			references, ok := platformFullReferenceSet(contents, target)

			if !ok {
				t.Fatalf("%s: the reference list could not be read", projectFile)
			}

			sets[target] = references

			shared, deltas, ok := adoptPlatformReferences(contents, target, references)

			if !ok {
				t.Fatalf("%s: adoption declined a project file that carries a block", projectFile)
			}

			if rebuilt, err := renderPlatformReferences(contents, shared, deltas); err != nil || rebuilt != contents {
				t.Errorf("%s: a %s reconvert does not reproduce the committed file (%v)", projectFile, target, err)
			}
		}

		shared, deltas, _ := splitPlatformReferenceSetsOver(corpusLattice(), sets)

		if rebuilt, err := renderPlatformReferences(contents, shared, deltas); err != nil || rebuilt != contents {
			t.Errorf("%s: the merge's split does not reproduce the committed file (%v)", projectFile, err)
		}
	}
}

// Four targets, one known input: the groups are GOOS, then arch, then pair, each with its own condition.
func TestPlatformReferenceLatticeRendersArchAndPairGroups(t *testing.T) {
	const w, l, d, a = "windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"

	sets := map[string][]string{
		w: {refErrors, refIO, refSysWin},
		l: {refErrors, refIO, refSysUnx, refRoute},
		d: {refErrors, refIO, refSysUnx},
		a: {refErrors, refSysUnx, refSysArm},
	}

	shared, deltas, varies := splitPlatformReferenceSetsOver(mustPlatformLattice(universeArm), sets)

	if !varies || strings.Join(shared, " ") != refErrors {
		t.Fatalf("shared = %v, varies = %v; want only %s shared", shared, varies, refErrors)
	}

	rendered, err := renderPlatformReferences(flatProjectFile(sets[w]), shared, deltas)

	if err != nil {
		t.Fatal(err)
	}

	var conditions []string

	for _, line := range strings.Split(rendered, "\r\n") {
		if match := platformReferenceGroupRegex.FindStringSubmatch(line); match != nil {
			if _, ok := platformReferenceGroupKey(match[1]); ok {
				conditions = append(conditions, match[1])
			}
		}
	}

	want := []string{
		"'$(GoTargetOS)'=='darwin'",                               // refSysUnx (with linux)
		"'$(GoTargetOS)'=='linux'",                                // refSysUnx (both linux targets)
		"'$(GoTargetOS)'=='windows'",                              // refSysWin
		"'$(GoTargetArch)'=='amd64'",                              // refIO, every amd64 target
		"'$(GoTargetOS)'=='linux' And '$(GoTargetArch)'=='amd64'", // refRoute
		"'$(GoTargetOS)'=='linux' And '$(GoTargetArch)'=='arm64'", // refSysArm
	}

	if strings.Join(conditions, "\n") != strings.Join(want, "\n") {
		t.Errorf("group conditions:\n  %s\nwant\n  %s", strings.Join(conditions, "\n  "), strings.Join(want, "\n  "))
	}

	if err := assertWellFormedXml(rendered); err != nil {
		t.Errorf("not well-formed: %v", err)
	}
}

// refSysArm stands for a reference only linux/arm64 takes.
var refSysArm = "$(go2csPath)core/internal/runtime/atomic/internal.runtime.atomic.csproj"

// The reference lattice's two properties at four targets, over random per-target reference sets and random sets of
// participating targets: what a merge renders reads back as each participant's own set, and a single-target
// reconvert that adopts its own unchanged set reproduces the file byte for byte.
func TestPlatformReferenceLatticeRoundTripsAtFourTargets(t *testing.T) {
	lattice := mustPlatformLattice(universeArm)
	pool := []string{refErrors, refIO, refSysWin, refSysUnx, refRoute, refSysArm}
	random := rand.New(rand.NewSource(20261109))

	for trial := 0; trial < 500; trial++ {
		sets := map[string][]string{}

		for _, target := range universeArm {
			if random.Intn(4) == 0 {
				continue // this target does not build the package
			}

			var references []string

			for _, reference := range pool {
				if random.Intn(2) == 0 {
					references = append(references, reference)
				}
			}

			sets[target] = sortedUniqueStrings(references)
		}

		if len(sets) == 0 {
			continue
		}

		shared, deltas, _ := splitPlatformReferenceSetsOver(lattice, sets)
		merged, err := renderPlatformReferences(flatProjectFile(nil), shared, deltas)

		if err != nil {
			t.Fatalf("trial %d: render: %v", trial, err)
		}

		for target, want := range sets {
			got, ok := platformFullReferenceSet(merged, target)

			if !ok || strings.Join(got, " ") != strings.Join(want, " ") {
				t.Fatalf("trial %d, sets %v: %s reads back %v, want %v\n%s", trial, sets, target, got, want, merged)
			}

			fresh := flatProjectFile(want)
			adoptedShared, adoptedDeltas, ok := adoptPlatformReferencesOver(lattice, merged, target, want)

			if !ok {
				// No block: the package's sets agree, and the target's own flat file IS the merged one.
				if fresh != merged {
					t.Fatalf("trial %d, sets %v: %s declined adoption, yet its own file differs from the merged one", trial, sets, target)
				}

				continue
			}

			rebuilt, err := renderPlatformReferences(fresh, adoptedShared, adoptedDeltas)

			if err != nil || rebuilt != merged {
				t.Fatalf("trial %d, sets %v: a %s reconvert does not reproduce the merged file (%v)\n--- merged ---\n%s\n--- rebuilt ---\n%s",
					trial, sets, target, err, merged, rebuilt)
			}
		}
	}
}

// Hand-owns follow their principal's PRESENCE set through the same lattice.
func TestHandOwnDestinationsAtFourTargets(t *testing.T) {
	const w, l, d, a = "windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"

	coreDir := filepath.Join("root", "core")
	packageDir := filepath.Join(coreDir, "runtime")

	for _, row := range []struct {
		emitters []string
		want     []string
	}{
		{[]string{w, l, d, a}, []string{""}},
		{[]string{w, l, d}, []string{"amd64"}},
		{[]string{l, a}, []string{"linux"}},
		{[]string{a}, []string{"linux-arm64"}},
		{[]string{w, a}, []string{"windows", "linux-arm64"}},
	} {
		var want []string

		for _, folder := range row.want {
			want = append(want, filepath.Join(packageDir, folder))
		}

		if got := platformHandOwnDestinations(coreDir, "runtime", universeArm, row.emitters); strings.Join(got, "|") != strings.Join(want, "|") {
			t.Errorf("emitters %v: got %v, want %v", row.emitters, got, want)
		}
	}

	var want []string

	for _, folder := range []string{"", "windows", "linux", "darwin", "amd64", "arm64", "windows-amd64", "linux-amd64", "darwin-amd64", "linux-arm64"} {
		want = append(want, filepath.Join(packageDir, folder))
	}

	if got := handOwnCandidateDirs(coreDir, "runtime", universeArm); strings.Join(got, "|") != strings.Join(want, "|") {
		t.Errorf("candidate dirs = %v, want %v", got, want)
	}
}
