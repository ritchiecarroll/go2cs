// solutionGenerator_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// TestBuildSolutionXML checks the emitted .slnx structure: the configuration block, the
// root-level infrastructure projects (golib + go2cs-gen, no folder), the per-package
// import-path folders, the explicit self-closing intermediate folders (VS's canonical form),
// project ordering, CRLF line endings, and no BOM — matching the format of the existing
// src/go2cs.slnx.
func TestBuildSolutionXML(t *testing.T) {
	coreProjects := []string{
		"core/fmt/fmt.csproj",
		"core/golib/golib.csproj",
		"core/internal/abi/internal.abi.csproj",
	}
	xml := buildSolutionXML(coreProjects)

	wantContains := []string{
		"<Solution>",
		"<Configurations>",
		"<Platform Name=\"Any CPU\" />",
		"<Platform Name=\"x64\" />",
		"<Platform Name=\"x86\" />",
		// go2cs-gen and golib are root-level projects (2-space indent), NOT wrapped in a folder —
		// the folder tree is reserved for Go import paths.
		"\r\n  <Project Path=\"gen/go2cs-gen/go2cs-gen.csproj\" />\r\n",
		"\r\n  <Project Path=\"core/golib/golib.csproj\" />\r\n",
		// Member packages nest under a folder named by their bare Go import path (no /core/
		// umbrella), carrying a deterministic Id (see folderID) — match the opening tag and name,
		// tolerating the trailing ` Id="…"`.
		"<Folder Name=\"/fmt/\" Id=\"",
		"<Project Path=\"core/fmt/fmt.csproj\" />",
		"<Folder Name=\"/internal/abi/\" Id=\"",
		"<Project Path=\"core/internal/abi/internal.abi.csproj\" />",
		// The `internal` namespace is not itself a package, so its folder is an explicit
		// self-closing intermediate with NO Id — matching how Visual Studio serializes it.
		"<Folder Name=\"/internal/\" />",
		"</Solution>",
	}

	for _, want := range wantContains {
		if !strings.Contains(xml, want) {
			t.Errorf("solution XML missing %q\n---\n%s", want, xml)
		}
	}

	// The intermediate /internal/ folder is declared before its child /internal/abi/ (VS's
	// canonical order — a pure intermediate sorts first within its subtree).
	if i, j := strings.Index(xml, "<Folder Name=\"/internal/\" />"), strings.Index(xml, "<Folder Name=\"/internal/abi/\""); i < 0 || j < 0 || i > j {
		t.Errorf("intermediate /internal/ must precede /internal/abi/ (got %d, %d)\n---\n%s", i, j, xml)
	}

	// The old /core/ umbrella and /generators/ folder are gone — folders are bare import paths
	// and the two infrastructure projects live at the root.
	for _, unwanted := range []string{"<Folder Name=\"/core/\"", "<Folder Name=\"/generators/\""} {
		if strings.Contains(xml, unwanted) {
			t.Errorf("solution XML should not contain %q\n---\n%s", unwanted, xml)
		}
	}

	// golib is not grouped under a /golib/ import-path folder.
	if strings.Contains(xml, "<Folder Name=\"/golib/\"") {
		t.Errorf("golib should be a root project, not a /golib/ folder\n---\n%s", xml)
	}

	// No BOM; starts directly with the root element (like src/go2cs.slnx).
	if !strings.HasPrefix(xml, "<Solution>\r\n") {
		t.Errorf("solution XML must start with <Solution> and CRLF, no BOM; got prefix %q", xml[:min(24, len(xml))])
	}

	// CRLF line endings only — no bare LF should remain once CRLF pairs are removed.
	if strings.Contains(strings.ReplaceAll(xml, "\r\n", ""), "\n") {
		t.Errorf("solution XML contains a bare LF; expected CRLF only")
	}
}

// TestBuildRecurseSolutionXML checks a per-project recurse solution (ModuleConverter): the config block,
// the three %GOPATH%-mirroring solution folders in ENFORCED (non-alphabetic) order — /src/ then /pkg/ then
// /core/ — the startup (anchor) app project marked DefaultStartup under /src/, third-party deps under
// /pkg/, the runtime + analyzer under /core/, CRLF line endings, and no BOM.
func TestBuildRecurseSolutionXML(t *testing.T) {
	startup := "example.com.app.csproj" // the app is the solution's startup project

	folders := []solutionFolder{
		{name: "/src/", projects: []string{"example.com.app.csproj"}},
		{name: "/pkg/", projects: []string{"../../pkg/github.com/google/uuid/github.com.google.uuid.csproj"}},
		{name: "/core/", projects: []string{"../../core/golib/golib.csproj", "../../gen/go2cs-gen/go2cs-gen.csproj"}},
	}

	xml := buildRecurseSolutionXML(folders, startup)

	wantContains := []string{
		"<Solution>",
		"<Configurations>",
		"<Platform Name=\"Any CPU\" />",
		"<Folder Name=\"/src/\">",
		"<Project Path=\"example.com.app.csproj\" DefaultStartup=\"true\" />",
		"<Folder Name=\"/pkg/\">",
		"<Project Path=\"../../pkg/github.com/google/uuid/github.com.google.uuid.csproj\" />",
		"<Folder Name=\"/core/\">",
		"<Project Path=\"../../core/golib/golib.csproj\" />",
		"<Project Path=\"../../gen/go2cs-gen/go2cs-gen.csproj\" />",
		"</Solution>",
	}

	for _, want := range wantContains {
		if !strings.Contains(xml, want) {
			t.Errorf("recurse solution XML missing %q\n---\n%s", want, xml)
		}
	}

	// Enforced folder order: /src/ before /pkg/ before /core/ — deliberately NOT alphabetic.
	srcIdx := strings.Index(xml, `<Folder Name="/src/">`)
	pkgIdx := strings.Index(xml, `<Folder Name="/pkg/">`)
	coreIdx := strings.Index(xml, `<Folder Name="/core/">`)

	if !(srcIdx < pkgIdx && pkgIdx < coreIdx) {
		t.Errorf("solution folders out of enforced order (src=%d pkg=%d core=%d):\n%s", srcIdx, pkgIdx, coreIdx, xml)
	}

	// No BOM; CRLF only.
	if !strings.HasPrefix(xml, "<Solution>\r\n") {
		t.Errorf("recurse solution must start with <Solution> and CRLF, no BOM; got prefix %q", xml[:min(24, len(xml))])
	}

	if strings.Contains(strings.ReplaceAll(xml, "\r\n", ""), "\n") {
		t.Errorf("recurse solution contains a bare LF; expected CRLF only")
	}
}

// TestBuildRecurseSolutionXMLSkipsEmptyFolders verifies a dependency package's own solution — which has no
// main-module (src) project — omits the /src/ folder entirely while keeping /pkg/ and /core/ in enforced
// order.
func TestBuildRecurseSolutionXMLSkipsEmptyFolders(t *testing.T) {
	folders := []solutionFolder{
		{name: "/src/", projects: nil},
		{name: "/pkg/", projects: []string{"github.com.google.uuid.csproj"}},
		{name: "/core/", projects: []string{"../../core/golib/golib.csproj", "../../gen/go2cs-gen/go2cs-gen.csproj"}},
	}

	xml := buildRecurseSolutionXML(folders, "github.com.google.uuid.csproj")

	if strings.Contains(xml, `<Folder Name="/src/">`) {
		t.Errorf("expected no /src/ folder when there are no main-module projects\n%s", xml)
	}

	if !strings.Contains(xml, `<Folder Name="/pkg/">`) || !strings.Contains(xml, `<Folder Name="/core/">`) {
		t.Errorf("expected both /pkg/ and /core/ folders\n%s", xml)
	}

	// The third-party anchor is still the startup project in its own solution.
	if !strings.Contains(xml, `<Project Path="github.com.google.uuid.csproj" DefaultStartup="true" />`) {
		t.Errorf("dependency anchor should be marked DefaultStartup in its own solution\n%s", xml)
	}
}

// TestCollectConvertedProjects builds a fake output tree and confirms the walk collects package
// projects, EXCLUDES the converted per-package test projects (whose inputs are pipeline-staged, so
// a solution containing one cannot build — and the standard library is packed for NuGet by
// building this solution), skips a stray copy of golib, and returns forward-slash
// solution-relative paths.
func TestCollectConvertedProjects(t *testing.T) {
	root := t.TempDir()

	files := []string{
		"core/fmt/fmt.csproj",
		"core/fmt/fmt.tests.csproj", // Phase-4 test project — must be excluded
		"core/fmt/fmt_test.csproj",  // the older spelling — excluded too
		"core/internal/abi/internal.abi.csproj",
		"core/golib/golib.csproj", // stray copy — must be skipped (added explicitly elsewhere)
		"core/fmt/fmt.cs",         // not a csproj — ignored
	}

	for _, rel := range files {
		full := filepath.Join(root, filepath.FromSlash(rel))
		if err := os.MkdirAll(filepath.Dir(full), 0755); err != nil {
			t.Fatalf("mkdir: %v", err)
		}
		if err := os.WriteFile(full, []byte("x"), 0644); err != nil {
			t.Fatalf("write: %v", err)
		}
	}

	converter := &StdLibConverter{go2csPath: root}

	coreProjects, err := converter.collectConvertedProjects()

	if err != nil {
		t.Fatalf("collectConvertedProjects: %v", err)
	}

	coreSet := strings.Join(coreProjects, "\n")

	if !strings.Contains(coreSet, "core/fmt/fmt.csproj") {
		t.Errorf("expected fmt.csproj in core projects, got %v", coreProjects)
	}
	if !strings.Contains(coreSet, "core/internal/abi/internal.abi.csproj") {
		t.Errorf("expected internal.abi.csproj in core projects, got %v", coreProjects)
	}
	if strings.Contains(coreSet, "golib") {
		t.Errorf("golib copy under core/golib should be skipped, got %v", coreProjects)
	}
	if strings.Contains(coreSet, ".cs\n") || strings.HasSuffix(coreSet, ".cs") {
		t.Errorf("non-csproj file should be ignored, got %v", coreProjects)
	}
	if strings.Contains(coreSet, "tests.csproj") || strings.Contains(coreSet, "_test.csproj") {
		t.Errorf("converted test projects must stay out of the solution, got %v", coreProjects)
	}
}

// TestCollectConvertedProjectsIgnoresTestProjectReferences guards the ordering that keeps an
// EXCLUDED project out of the solution's closure as well as out of its member list.
//
// The recovery path exists for a package the converter deliberately never transpiles (`unsafe`),
// discovered from its dependents' <ProjectReference>s. With no convert-set graph — the
// multi-platform merge's case, where the tree being scanned IS the finished corpus — every
// unmatched reference is recovered, so a reference naming a file that does not exist becomes a
// <Project> entry naming a file that does not exist, and the solution stops loading. A converted
// per-package TEST project is exactly where such a reference lives: it is pipeline-staged, no gate
// builds it from the solution, and `crypto/ecdh/crypto.ecdh.tests.csproj` really does carry the
// unvendored spelling of a GOROOT-vendored project file (found 2026-08-14, by the three-target
// merge that produced a 308-project solution against a corpus of 307).
//
// So: the fixture's test project references a package that exists nowhere. Its production sibling
// is listed; the phantom is not.
func TestCollectConvertedProjectsIgnoresTestProjectReferences(t *testing.T) {
	root := t.TempDir()

	testCsproj := strings.Join([]string{
		`<Project Sdk="Microsoft.NET.Sdk">`,
		`  <ItemGroup>`,
		`    <ProjectReference Include="$(go2csPath)core/vendor/golang.org/x/crypto/chacha20/golang.org.x.crypto.chacha20.csproj" />`,
		`  </ItemGroup>`,
		`</Project>`,
	}, "\r\n")

	files := map[string]string{
		"core/crypto/ecdh/crypto.ecdh.csproj":       `<Project />`,
		"core/crypto/ecdh/crypto.ecdh.tests.csproj": testCsproj,
	}

	for rel, body := range files {
		full := filepath.Join(root, filepath.FromSlash(rel))
		if err := os.MkdirAll(filepath.Dir(full), 0755); err != nil {
			t.Fatalf("mkdir: %v", err)
		}
		if err := os.WriteFile(full, []byte(body), 0644); err != nil {
			t.Fatalf("write: %v", err)
		}
	}

	converter := &StdLibConverter{go2csPath: root}

	coreProjects, err := converter.collectConvertedProjects()

	if err != nil {
		t.Fatalf("collectConvertedProjects: %v", err)
	}

	coreSet := strings.Join(coreProjects, "\n")

	if !strings.Contains(coreSet, "core/crypto/ecdh/crypto.ecdh.csproj") {
		t.Errorf("expected the production project to be listed, got %v", coreProjects)
	}
	if strings.Contains(coreSet, "chacha20") {
		t.Errorf("a reference read from an EXCLUDED test project must not be recovered into the solution, got %v", coreProjects)
	}
	if len(coreProjects) != 1 {
		t.Errorf("expected exactly the one production project, got %v", coreProjects)
	}
}

// TestParseCoreProjectRefs checks that only $(go2csPath)core/ ProjectReferences are extracted
// (normalized to forward slashes), while the analyzer's $(go2csPath)gen/ reference and NuGet
// PackageReferences are ignored — the raw material for recovering hand-owned packages like unsafe.
//
// The fixture MIXES separators deliberately. Emission is forward-slash since F5, but a reference
// still arrives backslashed from a pre-F5 corpus, a deployed tree or a hand-authored project, and
// the normalization must not depend on the HOST: filepath.ToSlash replaces the host separator, so
// off Windows it is the identity and every backslashed reference came back through unchanged.
func TestParseCoreProjectRefs(t *testing.T) {
	content := strings.Join([]string{
		`<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" PrivateAssets="All" />`,
		`<ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />`,
		`<ProjectReference Include="$(go2csPath)core/errors/errors.csproj" />`,
		`<ProjectReference Include="$(go2csPath)core\unsafe\unsafe.csproj" />`,
		`<PackageReference Include="go.unsafe" Version="$(GoStdLibVersion)" />`,
	}, "\r\n")

	got := parseCoreProjectRefs(content)

	want := map[string]bool{
		"core/golib/golib.csproj":   true,
		"core/errors/errors.csproj": true,
		"core/unsafe/unsafe.csproj": true,
	}

	if len(got) != len(want) {
		t.Fatalf("expected %d core refs, got %d: %v", len(want), len(got), got)
	}

	for _, ref := range got {
		if !want[ref] {
			t.Errorf("unexpected core ref %q (analyzer gen\\ ref and NuGet PackageReference must be ignored); got %v", ref, got)
		}
	}
}

// TestCollectConvertedProjectsRecoversReferencedManualPackage builds a fake output tree in which a
// converted package references an entirely hand-owned package (unsafe) that the converter never
// emits — so unsafe has NO .csproj on disk — and confirms collectConvertedProjects recovers unsafe
// from the reference. This is the durable fix for go.unsafe silently falling out of the generated
// solution (and thus never being published to NuGet). A referenced golib is still skipped here (the
// caller adds it explicitly), and a normally-converted referenced package is not double-listed.
func TestCollectConvertedProjectsRecoversReferencedManualPackage(t *testing.T) {
	root := t.TempDir()

	// bytes imports unsafe (absent from the fresh output) + errors (converted) + golib.
	bytesCsproj := strings.Join([]string{
		`<Project Sdk="Microsoft.NET.Sdk">`,
		`  <ItemGroup>`,
		`    <ProjectReference Include="$(go2csPath)core\golib\golib.csproj" />`,
		`    <ProjectReference Include="$(go2csPath)core\errors\errors.csproj" />`,
		`    <ProjectReference Include="$(go2csPath)core\unsafe\unsafe.csproj" />`,
		`  </ItemGroup>`,
		`</Project>`,
	}, "\r\n")

	files := map[string]string{
		"core/bytes/bytes.csproj":   bytesCsproj, // converted, references unsafe
		"core/errors/errors.csproj": "<Project />",
		// NOTE: no core/unsafe/unsafe.csproj — the whole point (hand-owned, never emitted).
	}

	for rel, body := range files {
		full := filepath.Join(root, filepath.FromSlash(rel))
		if err := os.MkdirAll(filepath.Dir(full), 0755); err != nil {
			t.Fatalf("mkdir: %v", err)
		}
		if err := os.WriteFile(full, []byte(body), 0644); err != nil {
			t.Fatalf("write: %v", err)
		}
	}

	converter := &StdLibConverter{go2csPath: root}

	coreProjects, err := converter.collectConvertedProjects()

	if err != nil {
		t.Fatalf("collectConvertedProjects: %v", err)
	}

	count := func(target string) int {
		n := 0
		for _, p := range coreProjects {
			if p == target {
				n++
			}
		}
		return n
	}

	// unsafe is recovered from the reference even though it has no .csproj on disk.
	if count("core/unsafe/unsafe.csproj") != 1 {
		t.Errorf("expected core/unsafe/unsafe.csproj to be recovered exactly once, got %v", coreProjects)
	}

	// The normally-converted packages are present exactly once (errors is both converted AND
	// referenced — it must not be double-listed).
	if count("core/bytes/bytes.csproj") != 1 {
		t.Errorf("expected core/bytes/bytes.csproj once, got %v", coreProjects)
	}
	if count("core/errors/errors.csproj") != 1 {
		t.Errorf("expected core/errors/errors.csproj exactly once (converted + referenced), got %v", coreProjects)
	}

	// golib is referenced but must NOT be added here — GenerateSolutionFile appends it explicitly.
	if count("core/golib/golib.csproj") != 0 {
		t.Errorf("golib must not be added by collectConvertedProjects (caller adds it), got %v", coreProjects)
	}
}

// TestCollectConvertedProjectsFilteredRunSkipsOutOfFilterRefs guards the recovery gate: on a
// filtered -stdlib run, a converted project references packages that are real convert-set nodes but
// simply weren't emitted (out of the filter). Those must NOT be recovered (their .csproj belongs to
// a full conversion, not this output), while a genuinely hand-owned package the converter never
// transpiles (unsafe — absent from the convert-set graph) still must be. The distinguishing signal
// is convert-set membership, so this drives collectConvertedProjects with a populated graph.
func TestCollectConvertedProjectsFilteredRunSkipsOutOfFilterRefs(t *testing.T) {
	root := t.TempDir()

	// fmt is the only converted project in this filtered output; it references errors (a real
	// convert-set package that was filtered out) and unsafe (hand-owned, never in the convert set).
	fmtCsproj := strings.Join([]string{
		`<Project Sdk="Microsoft.NET.Sdk">`,
		`  <ItemGroup>`,
		`    <ProjectReference Include="$(go2csPath)core\errors\errors.csproj" />`,
		`    <ProjectReference Include="$(go2csPath)core\unsafe\unsafe.csproj" />`,
		`  </ItemGroup>`,
		`</Project>`,
	}, "\r\n")

	full := filepath.Join(root, filepath.FromSlash("core/fmt/fmt.csproj"))
	if err := os.MkdirAll(filepath.Dir(full), 0755); err != nil {
		t.Fatalf("mkdir: %v", err)
	}
	if err := os.WriteFile(full, []byte(fmtCsproj), 0644); err != nil {
		t.Fatalf("write: %v", err)
	}

	// A convert-set graph that knows fmt and errors (both real stdlib packages) but NOT unsafe
	// (scan-time-skipped, hand-owned) — the state after a -stdlib fmt filter.
	graph := NewDependencyGraph()
	graph.AddPackage("fmt", "")
	graph.AddPackage("errors", "")

	converter := &StdLibConverter{go2csPath: root, graph: graph}

	coreProjects, err := converter.collectConvertedProjects()

	if err != nil {
		t.Fatalf("collectConvertedProjects: %v", err)
	}

	joined := strings.Join(coreProjects, "\n")

	// unsafe (not a convert-set node) is recovered even on a filtered run.
	if !strings.Contains(joined, "core/unsafe/unsafe.csproj") {
		t.Errorf("expected unsafe to be recovered (not a convert-set node), got %v", coreProjects)
	}

	// errors (a convert-set node merely excluded by the filter) must NOT be phantom-added; its
	// .csproj is not in this output, so referencing it would break the solution.
	if strings.Contains(joined, "core/errors/errors.csproj") {
		t.Errorf("out-of-filter errors must NOT be recovered (it is a convert-set node), got %v", coreProjects)
	}
}

// TestBuildSolutionXMLPlacesRecoveredUnsafeCanonically confirms a recovered unsafe project lands in
// its normal import-path folder with the deterministic folder Id and in canonical order (between the
// sibling top-level packages /unique/ and /vendor/), so a regenerated solution is adoptable
// verbatim as the committed go2cs-stdlib.slnx. `testing` is recovered by the same path.
func TestBuildSolutionXMLPlacesRecoveredUnsafeCanonically(t *testing.T) {
	coreProjects := []string{
		"core/unique/unique.csproj",
		"core/unsafe/unsafe.csproj",
		"core/vendor/golang.org/x/sys/vendor.golang.org.x.sys.csproj",
		"core/golib/golib.csproj",
	}

	xml := buildSolutionXML(coreProjects)

	// The stopgap's committed folder Id for /unsafe/ (sha1 of "go2cs-slnx-folder:/unsafe/", v5 bits).
	const unsafeFolder = "<Folder Name=\"/unsafe/\" Id=\"98f74097-975f-5e7b-8b80-18bf722660d9\">"

	for _, want := range []string{
		unsafeFolder,
		"<Project Path=\"core/unsafe/unsafe.csproj\" />",
	} {
		if !strings.Contains(xml, want) {
			t.Errorf("solution XML missing %q\n---\n%s", want, xml)
		}
	}

	// Canonical order: /unique/ before /unsafe/ before /vendor/ (as in the committed slnx).
	uniqueIdx := strings.Index(xml, "<Folder Name=\"/unique/\"")
	unsafeIdx := strings.Index(xml, "<Folder Name=\"/unsafe/\"")
	vendorIdx := strings.Index(xml, "<Folder Name=\"/vendor/\"")

	if uniqueIdx < 0 || unsafeIdx < 0 || vendorIdx < 0 || !(uniqueIdx < unsafeIdx && unsafeIdx < vendorIdx) {
		t.Errorf("expected /unique/ < /unsafe/ < /vendor/ (got %d, %d, %d)\n---\n%s", uniqueIdx, unsafeIdx, vendorIdx, xml)
	}
}
