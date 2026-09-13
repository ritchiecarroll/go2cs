// solutionGenerator.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file emits a Visual Studio .slnx solution for a whole-standard-library (-stdlib)
// conversion, so the freshly converted stdlib is openable / buildable as a single unit
// right after a run instead of relying on a hand-maintained solution that drifts.
//
// ⚠ KNOWN VS BEHAVIOR — the generated solution prompts "save?" on every open/close, and
// saving changes NOTHING (bisected 2026-08-06; see CLAUDE.md). Any solution holding a
// project that imports a .projitems shared-items file (here: golib and go2cs-gen both
// import core/go2cs/go2cs.projitems, the Symbols shared project) is marked dirty by VS's
// shared-project bookkeeping — classic .sln serialized it as a SharedMSBuildProjectFiles
// section, .slnx has no element for it, so the model always differs from the parsed file
// while every save (even Save-As) writes byte-identical content (hash-verified against
// SolutionPersistence 1.0.52, the version VS itself ships). The emitted bytes ARE
// VS-canonical; do not chase this as a generator formatting defect, and do not
// restructure the Symbols import to silence a cosmetic dialog. Tracked upstream:
// https://github.com/microsoft/vs-solutionpersistence/issues/156
package main

import (
	"crypto/sha1"
	"fmt"
	"io/fs"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
)

const (
	// generatedSolutionFileName is the .slnx solution emitted at the output root
	// (-go2cspath) after a -stdlib conversion. Since the converted standard library moved home
	// to src/core (2026-08-01) its project paths match the repository's layout exactly, so the
	// committed src/go2cs-stdlib.slnx is adopted from this output VERBATIM — no path rewriting.
	// (It replaced the hand-maintained .sln on 2026-07-10, and the misnamed
	// go-src-converted.slnx on 2026-08-01.)
	generatedSolutionFileName = "go2cs-stdlib.slnx"

	// golibProjectReference / genProjectReference are the solution-relative paths to the
	// shared runtime and the source-generator/analyzer project. They mirror the
	// $(go2csPath) references baked into every generated .csproj (where $(go2csPath)
	// resolves to $(SolutionDir)), so the solution locates them wherever those csproj
	// references already resolve — no absolute, machine-specific path is emitted.
	golibProjectReference = "core/golib/golib.csproj"
	genProjectReference   = "gen/go2cs-gen/go2cs-gen.csproj"
)

// GenerateSolutionFile writes a Visual Studio .slnx solution at the output root that
// references every converted stdlib project, the shared golib runtime, and the go2cs-gen
// analyzer. All references are relative to the solution file so the generated solution is
// portable. Per-package converted TEST projects are deliberately excluded — see
// collectConvertedProjects.
func (c *StdLibConverter) GenerateSolutionFile() error {
	fmt.Println("Generating solution file...")

	coreProjects, err := c.collectConvertedProjects()

	if err != nil {
		return err
	}

	// golib is the shared runtime; it lives under core/ in the repo source tree (not in the
	// converter's output). Add it so it is counted and present; buildSolutionXML emits it at
	// the solution root (no folder), not among the import-path package folders.
	coreProjects = append(coreProjects, golibProjectReference)

	// Sort for deterministic, stable output regardless of filesystem walk order.
	sort.Strings(coreProjects)

	contents := buildSolutionXML(coreProjects)
	solutionFile := filepath.Join(c.go2csPath, generatedSolutionFileName)

	// Only rewrite when the content actually changed so repeated runs stay a no-op.
	if needToWriteFile(solutionFile, []byte(contents)) {
		if err := os.WriteFile(solutionFile, []byte(contents), 0644); err != nil {
			return fmt.Errorf("failed to write solution file \"%s\": %w", solutionFile, err)
		}
	}

	// Project count: converted core packages + golib (in coreProjects) + the analyzer.
	fmt.Printf("Solution file generated: %s (%d projects)\n", solutionFile, len(coreProjects)+1)

	return nil
}

// coreProjectRefRE matches an inter-package `<ProjectReference Include="$(go2csPath)core/...csproj">`
// as emitted for a converted stdlib import (writeProjectFile / getImportPackageInfo always root such
// references at `$(go2csPath)core/`). Both separators are accepted so a corpus emitted by a pre-F5
// binary, a deployed tree, or a hand-authored project still reads; the captured group is the
// reference's solution-relative path, which parseCoreProjectRefs normalizes to forward slashes. The
// analyzer reference is rooted at `$(go2csPath)gen/` and golib at `core/golib/`, both handled elsewhere.
var coreProjectRefRE = regexp.MustCompile(`Include="\$\(go2csPath\)(core[\\/][^"]+\.csproj)"`)

// parseCoreProjectRefs returns the solution-relative (forward-slash) path of every core\ package a
// converted .csproj references via <ProjectReference>. It is how an entirely hand-owned package the
// converter never emits into the fresh output (its C# is 100% [module: go.GoManualConversion]) is
// still recovered for the solution: its dependents reference it, so it surfaces here even though its
// own .csproj is absent from the output tree. `unsafe` is the canonical case.
func parseCoreProjectRefs(csprojContent string) []string {
	matches := coreProjectRefRE.FindAllStringSubmatch(csprojContent, -1)
	refs := make([]string, 0, len(matches))

	for _, m := range matches {
		// NOT filepath.ToSlash: that replaces the HOST separator, so on Linux/macOS a backslashed
		// reference passes through unchanged and every downstream path comparison misses.
		refs = append(refs, normalizeEmittedPath(m[1]))
	}

	return refs
}

// importPathOf maps a solution-relative core package .csproj path back to its Go import path, e.g.
// "core/unsafe/unsafe.csproj" -> "unsafe", "core/internal/abi/internal.abi.csproj" -> "internal/abi".
func importPathOf(coreProject string) string {
	return strings.TrimPrefix(path.Dir(coreProject), "core/")
}

// collectConvertedProjects walks the emitted core/ output tree and returns the solution-relative
// (forward-slash) paths of every package .csproj. golib is excluded here — the caller adds it
// explicitly so its reference is emitted even on a filtered run that produced no core output —
// and so are the converted per-package test projects (see the walk).
//
// It also recovers any ENTIRELY HAND-OWNED package: one every converted dependent references but
// that the converter never emits into the fresh output because its C# is 100% [module:
// go.GoManualConversion] (so it is dropped from the convert set — `unsafe` is the canonical case).
// Such a package leaves no .csproj for the walk to find, so it would silently fall out of the
// generated solution and, downstream, never be published to NuGet — even though its dependents
// cannot build without it. Every converted project that imports it still emits a
// `$(go2csPath)core\...` ProjectReference to it, so the missing package is discovered from those
// references and appended in its normal import-path position. This generalizes the explicit golib
// append to any such package with no hardcoded names.
func (c *StdLibConverter) collectConvertedProjects() (coreProjects []string, err error) {
	coreDir := filepath.Join(c.go2csPath, "core")

	if info, statErr := os.Stat(coreDir); statErr != nil || !info.IsDir() {
		// Nothing converted (or a filtered run produced no core output). The solution still
		// lists golib + the analyzer, which is harmless and keeps the output deterministic.
		return nil, nil
	}

	// converted: solution-relative path of every package .csproj the walk actually found.
	// referenced: every core\ package a converted .csproj points at via <ProjectReference>.
	converted := make(map[string]bool)
	referenced := make(map[string]bool)

	walkErr := filepath.WalkDir(coreDir, func(path string, d fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if d.IsDir() || !strings.HasSuffix(d.Name(), ".csproj") {
			return nil
		}

		rel, relErr := filepath.Rel(c.go2csPath, path)

		if relErr != nil {
			return relErr
		}

		rel = filepath.ToSlash(rel)

		// golib is added explicitly by GenerateSolutionFile; skip any copy that happens to
		// live under core/golib so it is not listed twice.
		if rel == golibProjectReference || strings.HasPrefix(rel, "core/golib/") {
			return nil
		}

		// A converted per-package TEST project (`<pkg>.tests.csproj`, the Phase-4 artifact a
		// `-tests` run emits and a validated package commits beside its production code) is
		// deliberately left OUT of the solution. Its inputs are pipeline-staged — the `*.go`
		// differential-baseline copies are git-ignored, and it compiles against production `.cs`
		// the `-tests` run regenerates — so it cannot build from a clean tree, and a solution
		// containing one fails `dotnet build`, which is also how the standard library is packed
		// for NuGet. The pipeline builds these by path; the solution stays the buildable,
		// packable standard library.
		//
		// The skip comes BEFORE the reference harvest below, and that ordering is load-bearing: a
		// project excluded from the solution must not contribute to the solution's closure either.
		// It read the other way until 2026-08-14, and the multi-platform merge — the one caller that
		// scans a FINISHED corpus with no convert-set graph, so every unmatched reference is
		// recovered — turned a single stale reference in `crypto/ecdh/crypto.ecdh.tests.csproj`
		// (`…/chacha20/golang.org.x.crypto.chacha20.csproj`, the unvendored spelling of a file that
		// only exists as `vendor.golang.org.x.crypto.chacha20.csproj`) into a 308th <Project> entry
		// naming a path that does not exist. A solution carrying one does not load, so a test
		// project's own defect became a corpus-wide build break.
		if strings.HasSuffix(d.Name(), ".tests.csproj") || strings.HasSuffix(d.Name(), "_test.csproj") {
			return nil
		}

		// Record the packages this project references so a referenced-but-unconverted (hand-owned)
		// package can be recovered after the walk. A read error simply yields no references.
		if data, readErr := os.ReadFile(path); readErr == nil {
			for _, ref := range parseCoreProjectRefs(string(data)) {
				referenced[ref] = true
			}
		}

		converted[rel] = true
		coreProjects = append(coreProjects, rel)

		return nil
	})

	if walkErr != nil {
		return nil, fmt.Errorf("failed to scan converted projects under \"%s\": %w", coreDir, walkErr)
	}

	// Append every referenced-but-unconverted hand-owned package (see the doc comment): one whose
	// .csproj the walk never saw, discovered because its dependents reference it. golib is referenced
	// too but is added explicitly by the caller, so it is skipped here. Sorted so this function's own
	// output is deterministic (GenerateSolutionFile re-sorts the combined list).
	missing := make([]string, 0)

	for ref := range referenced {
		if converted[ref] || ref == golibProjectReference || strings.HasPrefix(ref, "core/golib/") {
			continue
		}

		// Only recover a package the converter deliberately does NOT transpile — one that is not a
		// node in the convert-set graph (scan-time-skipped, like the compiler pseudo-package
		// `unsafe`, whose C# is entirely hand-owned). A package that IS a graph node but is missing
		// from THIS output was merely excluded by a -stdlib package filter (its .csproj belongs to a
		// full conversion, not this filtered one); adding it would point the solution at a project
		// that is not in the output. With no graph wired up (unit tests), recover the reference.
		if c.graph == nil || !c.graph.Contains(importPathOf(ref)) {
			missing = append(missing, ref)
		}
	}

	sort.Strings(missing)
	coreProjects = append(coreProjects, missing...)

	return coreProjects, nil
}

// buildSolutionXML renders the .slnx document. golib (the shared runtime) and go2cs-gen (the
// source generator / analyzer) are emitted as projects at the solution root with no folder;
// every converted package nests under a solution folder named by its full Go import path, so
// the folder tree mirrors `import "..."` (`bufio` → /bufio/, `crypto/aes` → /crypto/aes/).
// Projects are emitted in the order given (callers sort them for determinism), with
// solution-relative forward-slash paths that match the on-disk output layout. Output uses CRLF
// line endings and no BOM, matching the existing src/go2cs.slnx.
func buildSolutionXML(coreProjects []string) string {
	var sb strings.Builder

	writeLine := func(indent int, text string) {
		sb.WriteString(strings.Repeat("  ", indent))
		sb.WriteString(text)
		sb.WriteString("\r\n")
	}

	writeProject := func(indent int, path string) {
		writeLine(indent, fmt.Sprintf("<Project Path=\"%s\" />", escapeXMLAttributeValue(path)))
	}

	// Folders carry an EXPLICIT deterministic Id: Visual Studio's .slnx loader derives a
	// missing folder Id from the folder's leaf display name, and Go namespaces guarantee
	// duplicate leaves (`/crypto/` vs `/vendor/golang.org/x/crypto/` are both "crypto") — VS
	// then refuses to open the solution ("a Solution Folder with the same unique identifier
	// already exists"). A UUID hashed from the FULL folder path is unique, stable across
	// regenerations, and machine-independent.
	writeFolderOpen := func(folder string) {
		writeLine(1, fmt.Sprintf("<Folder Name=\"%s\" Id=\"%s\">", escapeXMLAttributeValue(folder), folderID(folder)))
	}

	writeLine(0, "<Solution>")

	writeLine(1, "<Configurations>")
	writeLine(2, "<Platform Name=\"Any CPU\" />")
	writeLine(2, "<Platform Name=\"x64\" />")
	writeLine(2, "<Platform Name=\"x86\" />")
	writeLine(1, "</Configurations>")

	// Infrastructure projects sit at the solution ROOT with no folder: golib (the shared
	// runtime) and go2cs-gen (the source generator / analyzer) are not Go packages, and the
	// folder tree is reserved exclusively for Go import paths — so a folder name can never
	// collide with a current or future stdlib package (`generators`, `core`, …). golib arrives
	// inside coreProjects (the caller appends it); it is emitted here and skipped in the
	// package grouping below.
	writeProject(1, genProjectReference)
	writeProject(1, golibProjectReference)

	// Every converted package nests under a solution folder named by its FULL Go import path, so
	// the folder tree mirrors `import "..."` exactly: `bufio` → /bufio/, `crypto/aes` →
	// /crypto/aes/, `archive/tar` → /archive/tar/. A package's own directory is core/<import
	// path>; stripping the leading "core/" yields the import path.
	importFolder := func(project string) string {
		own := path.Dir(project) // core/<import path> — the package's own directory
		return "/" + strings.TrimPrefix(own, "core/") + "/"
	}

	// folderParent returns a folder's immediate parent ("/crypto/aes/" → "/crypto/"), or "/" at
	// the top level so the ancestor walk below terminates.
	folderParent := func(folder string) string {
		parent := path.Dir(strings.TrimSuffix(folder, "/"))
		if parent == "/" {
			return "/"
		}
		return parent + "/"
	}

	// Collect the folders to emit: every member folder (one that directly holds package
	// project(s)) PLUS every intermediate ancestor of one. A "pure intermediate" folder
	// (`archive/`, `crypto/internal/`, `vendor/golang.org/x/` — a namespace with only
	// sub-packages, no package of its own) must be declared EXPLICITLY: although the .slnx
	// loader tolerates leaf-only folder paths and implies the ancestors, Visual Studio's
	// canonical serialization writes each intermediate as a self-closing `<Folder .../>`, so an
	// implied-only solution is rewritten (and marked dirty) the instant it is opened. Emitting
	// them up front matches VS byte-for-byte and keeps a freshly generated solution clean.
	memberProjects := make(map[string][]string)
	memberFolder := make(map[string]bool)
	folderSet := make(map[string]bool)

	for _, project := range coreProjects {
		if project == golibProjectReference {
			continue // emitted at the root above, not under an import-path folder
		}

		folder := importFolder(project)
		memberProjects[folder] = append(memberProjects[folder], project)
		memberFolder[folder] = true
		folderSet[folder] = true

		for parent := folderParent(folder); parent != "/"; parent = folderParent(parent) {
			folderSet[parent] = true
		}
	}

	// folderSortKey reproduces Visual Studio's canonical folder order so a freshly generated
	// solution opens without VS re-sorting (and dirtying) it. VS lays folders out by a
	// depth-first walk in which each folder's own line is placed AMONG its children by name: a
	// member folder sorts by its project's dotted name (net/http's line follows `internal/`
	// because "net.http" > "internal/"), while a pure intermediate folder sorts first (its bare
	// path is a prefix of every child key). Both collapse to one key: the folder path with the
	// member's project base name appended (nothing for an intermediate). The trailing "/" on a
	// child segment vs the "." in a sibling project name orders them exactly as VS does, since
	// '.' (0x2E) sorts before '/' (0x2F).
	folderSortKey := func(folder string) string {
		if projects := memberProjects[folder]; len(projects) > 0 {
			return folder + strings.TrimSuffix(path.Base(projects[0]), ".csproj")
		}
		return folder // pure intermediate — sorts first within its subtree
	}

	folders := make([]string, 0, len(folderSet))
	for folder := range folderSet {
		folders = append(folders, folder)
	}

	sort.Slice(folders, func(i, j int) bool {
		return folderSortKey(folders[i]) < folderSortKey(folders[j])
	})

	for _, folder := range folders {
		if !memberFolder[folder] {
			// Pure intermediate folder — self-closing, NO Id, exactly as VS writes it. (VS derives
			// an Id-less folder's identity from its full path, so duplicate leaves like the many
			// `internal/` namespaces never collide.)
			writeLine(1, fmt.Sprintf("<Folder Name=\"%s\" />", escapeXMLAttributeValue(folder)))
			continue
		}

		// Member folder — carries an explicit deterministic Id (see folderID) and holds its
		// package project(s).
		writeFolderOpen(folder)

		for _, project := range memberProjects[folder] {
			writeProject(2, project)
		}

		writeLine(1, "</Folder>")
	}

	writeLine(0, "</Solution>")

	return sb.String()
}

// solutionFolder groups solution-relative (forward-slash) csproj paths under one top-level .slnx
// solution folder. Name is the slash-wrapped display name (e.g. "/src/"); projects are its members,
// sorted by the caller for deterministic output.
type solutionFolder struct {
	name     string
	projects []string
}

// buildRecurseSolutionXML renders a recurse per-project solution (ModuleConverter): the anchor project +
// its converted dependencies + golib + the analyzer, tying them to the pre-converted stdlib (referenced
// via $(go2csPath)core) for one dotnet build. Projects are grouped into top-level solution folders that
// mirror the %GOPATH% layout — `src` for the project(s) being converted (the app's own main-module
// packages), `pkg` for their dependency packages, and `core` for the go2cs runtime/generator projects
// (golib, go2cs-gen). Folders are emitted in the enforced src → pkg → core order (an empty folder is
// skipped — a dependency's own solution has no src package, for instance). That file order is only for
// deterministic output: Visual Studio's Solution Explorer sorts folders/projects alphabetically in the
// DISPLAY regardless, so the layout has no visible effect. This is a FLAT three-folder tree with no
// nested import-path folders, so — unlike the stdlib solution — it needs no explicit intermediate folders
// and no folder Ids (the three leaves are unique); it is already VS-canonical and never dirties on open.
//
// The project whose path equals startupProject is marked the VS default startup project via the .slnx
// DefaultStartup attribute (pass "" for none). CAVEAT: this only takes effect on a CLEAN FIRST open (a
// per-user .suo, once written, overrides it) and requires a VS new enough to include the 2025-05
// vs-solutionpersistence update; older VS ignores the attribute. It is the officially-supported
// mechanism, so it is kept for the fresh-conversion first-open case. Output uses CRLF line endings and no
// BOM, matching the other emitted solutions.
func buildRecurseSolutionXML(folders []solutionFolder, startupProject string) string {
	var sb strings.Builder

	writeLine := func(indent int, text string) {
		sb.WriteString(strings.Repeat("  ", indent))
		sb.WriteString(text)
		sb.WriteString("\r\n")
	}

	writeProject := func(indent int, project string) {
		if project == startupProject {
			writeLine(indent, fmt.Sprintf("<Project Path=\"%s\" DefaultStartup=\"true\" />", escapeXMLAttributeValue(project)))
		} else {
			writeLine(indent, fmt.Sprintf("<Project Path=\"%s\" />", escapeXMLAttributeValue(project)))
		}
	}

	writeLine(0, "<Solution>")

	writeLine(1, "<Configurations>")
	writeLine(2, "<Platform Name=\"Any CPU\" />")
	writeLine(2, "<Platform Name=\"x64\" />")
	writeLine(2, "<Platform Name=\"x86\" />")
	writeLine(1, "</Configurations>")

	for _, folder := range folders {
		if len(folder.projects) == 0 {
			continue // skip an empty folder — e.g. a dependency's own solution has no src package
		}

		writeLine(1, fmt.Sprintf("<Folder Name=\"%s\">", escapeXMLAttributeValue(folder.name)))

		for _, project := range folder.projects {
			writeProject(2, project)
		}

		writeLine(1, "</Folder>")
	}

	writeLine(0, "</Solution>")

	return sb.String()
}

// folderID returns a deterministic UUID for a solution-folder path, hashed from the full
// path so same-named folders at different nesting levels (Go's ubiquitous `internal`,
// `crypto` under both core and vendor, ...) never collide in Visual Studio's identifier
// space. Version/variant bits are set so the value is a well-formed (v5-style) UUID.
func folderID(folderPath string) string {
	sum := sha1.Sum([]byte("go2cs-slnx-folder:" + folderPath))
	b := sum[:16]
	b[6] = (b[6] & 0x0f) | 0x50
	b[8] = (b[8] & 0x3f) | 0x80
	return fmt.Sprintf("%x-%x-%x-%x-%x", b[0:4], b[4:6], b[6:8], b[8:10], b[10:16])
}

// xmlAttributeEscapes maps the four characters that would break an XML attribute onto their
// entities. It is built ONCE at package scope rather than per call: strings.NewReplacer compiles a
// lookup trie, so constructing one inside the function costs ~1,460 ns and 8 allocations per call
// against ~29 ns and zero allocations for a shared one — and every emitted project reference,
// folder and compile item goes through here. A Replacer is safe for concurrent use, so sharing it
// is free.
var xmlAttributeEscapes = strings.NewReplacer(
	"&", "&amp;",
	"\"", "&quot;",
	"<", "&lt;",
	">", "&gt;",
)

// escapeXMLAttributeValue escapes the minimal set of characters that would break an XML attribute.
//
// Every value this emitter puts inside quotes — .slnx project paths and folder names, .csproj
// Compile/None/ProjectReference includes — goes through it. Converted paths are ASCII with '/' and
// '.', so in practice nothing is replaced and the call returns the input untouched; the escaping
// exists so a module path that DOES carry an XML-special character produces a valid project file
// instead of one the SDK refuses to parse.
func escapeXMLAttributeValue(value string) string {
	return xmlAttributeEscapes.Replace(value)
}
