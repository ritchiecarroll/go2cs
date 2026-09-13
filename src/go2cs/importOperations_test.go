// importOperations_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestPackageQualifiedName locks the imported-type-alias namespace qualification for non-stdlib deps:
// go.<result>_package must be the imported package's converted class. The last segment is the Go
// package name (which can differ from the import-path segment, e.g. go-isatty is `package isatty`),
// and a single-segment module (namespace == the root) yields just the package name.
func TestPackageQualifiedName(t *testing.T) {
	cases := []struct {
		namespace string
		pkgName   string
		want      string
	}{
		{"go.github.com.google", "uuid", "github.com.google.uuid"},
		{"go.github.com.mattn", "isatty", "github.com.mattn.isatty"},       // path segment is go-isatty; package is isatty
		{"go.github.com.mattn", "colorable", "github.com.mattn.colorable"}, // go-colorable -> colorable
		{"go.example.com", "lib", "example.com.lib"},
		{"go", "foo", "foo"}, // single-segment module: namespace is the bare root
	}

	for _, tc := range cases {
		if got := packageQualifiedName(tc.namespace, tc.pkgName); got != tc.want {
			t.Errorf("packageQualifiedName(%q, %q) = %q, want %q", tc.namespace, tc.pkgName, got, tc.want)
		}
	}
}

// TestProjectNameFromModuleDirective guards issue #33's csproj-filename failure at its source: the
// go.mod module path is a TOKEN, so it may be quoted, and gopkg.in modules write it that way —
// gopkg.in/yaml.v3's own go.mod is literally `module "gopkg.in/yaml.v3"`. Reading the line's
// remainder raw carried the quotes into the project name and thence into the csproj FILENAME, which
// Windows rejects ("gopkg.in.yaml.v3".csproj → "The filename, directory name, or volume label syntax
// is incorrect"). The invariant asserted here is not merely the expected string but that a project
// name is always a legal filename — no character the platform forbids can survive the parse.
func TestProjectNameFromModuleDirective(t *testing.T) {
	cases := []struct {
		name   string
		goMod  string
		want   string
		wantNS string
	}{
		{
			// The exact shape shipped by gopkg.in/yaml.v3@v3.0.1 — quoted module path AND quoted
			// require paths, which is what issue #33's reporter hit.
			name:   "quoted gopkg.in module path",
			goMod:  "module \"gopkg.in/yaml.v3\"\n\nrequire (\n\t\"gopkg.in/check.v1\" v0.0.0-20161208181325-20d25e280405\n)\n",
			want:   "gopkg.in.yaml.v3",
			wantNS: RootNamespace + ".gopkg.@in",
		},
		{
			name:   "unquoted module path is unchanged",
			goMod:  "module github.com/fatih/color\n\ngo 1.17\n",
			want:   "github.com.fatih.color",
			wantNS: RootNamespace + ".github.com.fatih",
		},
		{
			name:   "raw-quoted module path",
			goMod:  "module `example.com/raw`\n\ngo 1.23\n",
			want:   "example.com.raw",
			wantNS: RootNamespace + ".example.com",
		},
		{
			name:   "trailing comment is not part of the path",
			goMod:  "module example.com/commented // see issue #33\n\ngo 1.23\n",
			want:   "example.com.commented",
			wantNS: RootNamespace + ".example.com",
		},
	}

	// The fixture module directories are outside GOROOT, so the module path comes from their go.mod
	// exactly as it does for a -recurse dependency in the module cache.
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			dir := t.TempDir()

			if err := os.WriteFile(filepath.Join(dir, "go.mod"), []byte(tc.goMod), 0o600); err != nil {
				t.Fatal(err)
			}

			projectName, namespace := getProjectName(dir, Options{goRoot: goRoot})

			if projectName != tc.want {
				t.Errorf("project name = %q, want %q", projectName, tc.want)
			}

			if namespace != tc.wantNS {
				t.Errorf("namespace = %q, want %q", namespace, tc.wantNS)
			}

			// The project name IS the csproj file name, so it must contain nothing the file system
			// forbids. This is the assertion that fails loudly for any future quoting/escaping arm
			// meant for code that leaks into a path.
			if bad := strings.IndexAny(projectName, "\"<>:|?*\\/"); bad >= 0 {
				t.Errorf("project name %q contains %q, which cannot appear in a file name", projectName, projectName[bad])
			}
		})
	}
}

// TestEmittedProjectReferenceIsHostIndependent is the F5 unit: the reference string that lands in a
// <ProjectReference Include="…"> must be the SAME on every host, and it must be forward-slashed.
//
// The pre-fix code hand-rolled the join: it replaced every "/" with a backslash and then called
// filepath.Join with a backslash-prefixed second element. On Windows filepath.Clean folded those
// injected separators into a well-formed path; on Unix filepath.Join treats a backslash as an
// ordinary filename character, so the same call emitted `$(go2csPath)core\fmt/\fmt.csproj` — silent
// at emission, a restore failure later.
//
// pathReplace hands back whichever separator the HOST produced, so the input spelling is exercised
// per-host: the Unix spelling always (filepath.ToSlash is the identity there), the Windows spelling
// only where it can actually arise.
func TestEmittedProjectReferenceIsHostIndependent(t *testing.T) {
	const want = "$(go2csPath)core/unicode/utf8/unicode.utf8.csproj"

	if got := emittedProjectReference("$(go2csPath)core/unicode/utf8", "unicode.utf8.csproj"); got != want {
		t.Errorf("emittedProjectReference(unix spelling) = %q, want %q", got, want)
	}

	if runtime.GOOS == "windows" {
		if got := emittedProjectReference(`$(go2csPath)core\unicode\utf8`, "unicode.utf8.csproj"); got != want {
			t.Errorf("emittedProjectReference(windows spelling) = %q, want %q", got, want)
		}
	}
}

// TestProjectNameSurvivesGoFileFreeContainerDir guards issue #35 at its source: a project name must be
// the package's full import path, and the upward module search must not surrender it to a container
// directory that happens to hold no .go files of its own.
//
// `internal/`, a service tree's `endpoints/` parent, a proto grouping like `xds/core/` — a Go module is
// full of directories whose only content is subdirectories. The walk treated the first such ancestor as
// a stop and named the project after the leaf segment alone, so cloud.google.com/go/bigquery's
// datatransfer/apiv1 and storage/apiv1 both emitted `apiv1.csproj`. Visual Studio rejects a solution
// holding two projects with the same name and gives no reason, so the whole generated .slnx silently
// fails to open (issue #35); the reporter's one conversion had 175 such projects in 49 name families.
// The collapse is not merely a naming nuisance either: the namespace is derived from the same parts, so
// a truncated `internal/errors` lands on go.errors_package — the converted standard library's own class.
//
// The fixture is the reporter's exact shape: one module, two go-file-free container directories, two
// leaf packages that share a final segment.
func TestProjectNameSurvivesGoFileFreeContainerDir(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	root := t.TempDir()

	if err := os.WriteFile(filepath.Join(root, "go.mod"), []byte("module cloud.google.com/go/bigquery\n\ngo 1.23\n"), 0o600); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(filepath.Join(root, "bigquery.go"), []byte("package bigquery\n"), 0o600); err != nil {
		t.Fatal(err)
	}

	// datatransfer/ and storage/ hold no .go files — only the apiv1 package below each.
	for _, leaf := range []string{filepath.Join("datatransfer", "apiv1"), filepath.Join("storage", "apiv1")} {
		dir := filepath.Join(root, leaf)

		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(filepath.Join(dir, "client.go"), []byte("package apiv1\n"), 0o600); err != nil {
			t.Fatal(err)
		}
	}

	cases := []struct {
		dir    string
		want   string
		wantNS string
	}{
		{
			dir:    filepath.Join(root, "datatransfer", "apiv1"),
			want:   "cloud.google.com.go.bigquery.datatransfer.apiv1",
			wantNS: RootNamespace + ".cloud.google.com.go.bigquery.datatransfer",
		},
		{
			dir:    filepath.Join(root, "storage", "apiv1"),
			want:   "cloud.google.com.go.bigquery.storage.apiv1",
			wantNS: RootNamespace + ".cloud.google.com.go.bigquery.storage",
		},
	}

	names := make(map[string]string, len(cases))

	for _, tc := range cases {
		projectName, namespace := getProjectName(tc.dir, Options{goRoot: goRoot})

		if projectName != tc.want {
			t.Errorf("getProjectName(%q) project name = %q, want %q", tc.dir, projectName, tc.want)
		}

		if namespace != tc.wantNS {
			t.Errorf("getProjectName(%q) namespace = %q, want %q", tc.dir, namespace, tc.wantNS)
		}

		// The invariant the solution actually needs: distinct packages get distinct project names.
		if prior, dup := names[projectName]; dup {
			t.Errorf("project name %q emitted for both %q and %q — a duplicate .slnx project name", projectName, prior, tc.dir)
		}

		names[projectName] = tc.dir
	}
}

// TestProjectNameFallsBackWhenNoModuleRoot pins the arm the issue-#35 fix must NOT disturb: with no
// go.mod and no main.go anywhere above it, there is no import path to recover, so the name stays the
// leaf-relative one the walk composes on the way up. The go-file-free ancestor is a FALLBACK here,
// which is exactly what it stopped being in the module case.
func TestProjectNameFallsBackWhenNoModuleRoot(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	root := t.TempDir()
	pkgDir := filepath.Join(root, "example.com", "foo", "bar")

	if err := os.MkdirAll(pkgDir, 0o755); err != nil {
		t.Fatal(err)
	}

	// foo/ has Go files, example.com/ does not, and nothing above declares a module.
	if err := os.WriteFile(filepath.Join(root, "example.com", "foo", "foo.go"), []byte("package foo\n"), 0o600); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(filepath.Join(pkgDir, "bar.go"), []byte("package bar\n"), 0o600); err != nil {
		t.Fatal(err)
	}

	projectName, namespace := getProjectName(pkgDir, Options{goRoot: goRoot})

	if want := "foo.bar"; projectName != want {
		t.Errorf("project name = %q, want %q", projectName, want)
	}

	// The bare root, because this arm joins its segments with "." before the separator split that
	// builds the namespace — so the qualification never reaches it. Asserted as-is rather than
	// corrected: it is pre-existing behavior on a path that cannot produce issue #35's collision (a
	// tree with no module root is converted one project at a time), and the declaration and
	// reference sides derive it from the same call, so they agree.
	if want := RootNamespace; namespace != want {
		t.Errorf("namespace = %q, want %q", namespace, want)
	}
}

// TestStdLibImportPathFromTargetDir pins the recovery itself: a core-rooted directory yields the
// ON-DISK import path (vendored spelling included), and anything else yields nothing so the caller
// stays on the import path as written.
func TestStdLibImportPathFromTargetDir(t *testing.T) {
	cases := []struct {
		targetDir string
		want      string
		wantOK    bool
	}{
		{"$(go2csPath)core/vendor/golang.org/x/crypto/chacha20", "vendor/golang.org/x/crypto/chacha20", true},
		{"$(go2csPath)core/unicode/utf8", "unicode/utf8", true},
		{"$(go2csPath)core/fmt", "fmt", true},
		// The host spelling pathReplace hands back on Windows must read the same.
		{`$(go2csPath)core\vendor\golang.org\x\crypto\chacha20`, "vendor/golang.org/x/crypto/chacha20", true},
		// Not core-rooted: the GOROOT rewrite found no match (warnGoRootPathReplace's case), or the
		// reference belongs to another tree entirely.
		{`C:\Program Files\Go\src\fmt`, "", false},
		{"$(go2csPath)pkg/github.com/google/uuid", "", false},
		{"$(go2csPath)core", "", false},
		// `core` must match a whole path SEGMENT: a sibling root that merely starts with it is not
		// the core tree, and reading it as one would yield a truncated import path.
		{"$(go2csPath)corelib/fmt", "", false},
	}

	for _, tc := range cases {
		got, ok := stdLibImportPathFromTargetDir(tc.targetDir)

		if ok != tc.wantOK || got != tc.want {
			t.Errorf("stdLibImportPathFromTargetDir(%q) = %q, %v; want %q, %v", tc.targetDir, got, ok, tc.want, tc.wantOK)
		}
	}
}

// TestGorootVendoredReferenceNamesTheVendoredProject is the regression for the phantom
// <ProjectReference> that made crypto/ecdh's committed test project unbuildable: the directory
// resolved to the vendored location while the project FILE NAME was composed from the unvendored
// import path, so the reference named `…/vendor/golang.org/x/crypto/chacha20/golang.org.x.crypto.chacha20.csproj`
// — a real directory holding no such file. MSBuild fails outright on a missing ProjectReference.
//
// The same stale name later leaked into go2cs-stdlib.slnx as a phantom 308th project through the
// multi-platform merge's solution-recovery path (fixed separately, on the solution side, by
// TestCollectConvertedProjectsIgnoresTestProjectReferences); this is the emission half.
//
// Driven through getLocalModulePackageInfo, which is the branch a GOROOT-vendored import actually
// reaches: build.Import cannot vendor-resolve `golang.org/x/…` with no source dir to resolve
// against, so resolution falls through to the module-aware loader's dir — the vendored one — while
// the import path stays as written. The fixture is synthetic so the assertion does not depend on
// which packages the host toolchain happens to vendor.
func TestGorootVendoredReferenceNamesTheVendoredProject(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	goRoot := t.TempDir()
	vendoredDir := filepath.Join(goRoot, "src", "vendor", "golang.org", "x", "crypto", "chacha20")

	const importPath = "golang.org/x/crypto/chacha20"

	importPackageDirs = map[string]importedPackageMeta{
		importPath: {Dir: vendoredDir, Name: "chacha20"},
	}

	info, ok := getLocalModulePackageInfo(importPath, Options{goRoot: goRoot, goPath: build.Default.GOPATH})

	if !ok {
		t.Fatalf("getLocalModulePackageInfo(%q) did not resolve", importPath)
	}

	if !info.IsStdLib {
		t.Error("a GOROOT-vendored package is standard library")
	}

	const wantReference = "$(go2csPath)core/vendor/golang.org/x/crypto/chacha20/vendor.golang.org.x.crypto.chacha20.csproj"

	if info.ProjectReference != wantReference {
		t.Errorf("ProjectReference = %q, want %q", info.ProjectReference, wantReference)
	}

	// PackageName is not only the .csproj name: it keys the embedded standard-library metadata and
	// composes the imported-alias class path, both of which record the vendored spelling.
	if want := "vendor.golang.org.x.crypto.chacha20"; info.PackageName != want {
		t.Errorf("PackageName = %q, want %q", info.PackageName, want)
	}

	if _, recorded := stdLibExportedMetadata(info.PackageName); !recorded {
		t.Errorf("embedded metadata has no record for %q — the name does not key the record it must", info.PackageName)
	}

	// The Go package name is the leaf either way; the vendor prefix must not reach it.
	if want := "chacha20"; info.RootPackageName != want {
		t.Errorf("RootPackageName = %q, want %q", info.RootPackageName, want)
	}
}

// TestStdLibReferenceUnchangedForUnvendoredPackage is the no-op half: for every package that is NOT
// GOROOT-vendored — which is the whole corpus bar the vendor/ tree — the on-disk path and the import
// path are the same string, so deriving the name from the directory changes nothing. This is what
// makes check-no-regression's zero-movement verdict meaningful rather than lucky.
func TestStdLibReferenceUnchangedForUnvendoredPackage(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{goRoot: goRoot, goPath: build.Default.GOPATH, targetPlatform: runtime.GOOS + "/" + runtime.GOARCH}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	cases := map[string]string{
		"fmt":          "$(go2csPath)core/fmt/fmt.csproj",
		"unicode/utf8": "$(go2csPath)core/unicode/utf8/unicode.utf8.csproj",
		"net/http":     "$(go2csPath)core/net/http/net.http.csproj",
	}

	for importPath, want := range cases {
		info, ok := getImportPackageInfo([]string{importPath}, options)[importPath]

		if !ok || info.Err != nil {
			t.Fatalf("getImportPackageInfo(%q): %v", importPath, info.Err)
		}

		if info.ProjectReference != want {
			t.Errorf("ProjectReference(%q) = %q, want %q", importPath, info.ProjectReference, want)
		}
	}
}

// The recurse module-cache branch composes its own $(go2csPath)pkg reference from the version-free
// import path, so it is a second emission site with the same invariant.
func TestEmittedProjectReferenceForModuleCachePath(t *testing.T) {
	got := emittedProjectReference("$(go2csPath)pkg/"+"github.com/google/uuid", "github.com.google.uuid.csproj")

	if want := "$(go2csPath)pkg/github.com/google/uuid/github.com.google.uuid.csproj"; got != want {
		t.Errorf("emittedProjectReference = %q, want %q", got, want)
	}
}

// emittedRelativeProjectPath models what projectFileBaseName budgets: the generated project's path
// relative to the conversion output root, `<tree-root>/<import-path>/<name>.csproj`, using the longest
// tree root (`core`). The dotted project name and the slashed import path are the same length.
func emittedRelativeProjectPath(canonical string) int {
	return len("core") + 1 + len(canonical) + 1 + len(projectFileBaseName(canonical)) + len(".csproj")
}

// TestProjectFileBaseNameLeavesCorpusNamesAlone pins the no-op half of the budget. Every name the
// committed corpus produces is well inside it, so the .csproj file names of the standard library and
// the behavioral tests are byte-identical to what they were before the budget existed — which is what
// makes check-no-regression's zero-movement verdict meaningful rather than lucky.
func TestProjectFileBaseNameLeavesCorpusNamesAlone(t *testing.T) {
	// Real names, longest first: the deepest emitted stdlib path is 101 characters and the deepest
	// behavioral one 109, both far under maxRelativeProjectPath.
	for _, canonical := range []string{
		"vendor.golang.org.x.crypto.internal.poly1305",
		"vendor.golang.org.x.crypto.chacha20poly1305",
		"log.slog.internal.benchmarks",
		"net.http",
		"fmt",
		"ForeignPointerImplementSuppression.shade",
		"github.com.envoyproxy.go-control-plane.envoy.extensions.filters.http.rbac.v3",
	} {
		if got := projectFileBaseName(canonical); got != canonical {
			t.Errorf("projectFileBaseName(%q) = %q, want it returned verbatim", canonical, got)
		}
	}
}

// TestProjectFileBaseNameBoundsTheEmittedPath is the issue-#35 second-wall invariant: whatever the
// import path, the emitted project path stays inside the budget, so a user's output root has a known
// amount of room before Windows' 260-character limit — the limit Visual Studio's project loader
// enforces even with LongPathsEnabled=1.
func TestProjectFileBaseNameBoundsTheEmittedPath(t *testing.T) {
	cases := []string{
		// The reporter's deepest package, 115 characters.
		"github.com.envoyproxy.go-control-plane.envoy.extensions.load_balancing_policies.client_side_weighted_round_robin.v3",
		"github.com.microsoft.go-mssqldb.internal.github.com.swisscom.mssql-always-encrypted.pkg.algorithms",
		"github.com.AzureAD.microsoft-authentication-library-for-go.apps.internal.oauth.ops.internal.grant",
		// Deeper than anything observed, to prove the bound is not merely a coincidence of the corpus.
		strings.Repeat("segment.", 18) + "leaf",
	}

	for _, canonical := range cases {
		if got := emittedRelativeProjectPath(canonical); got > maxRelativeProjectPath {
			t.Errorf("emitted path for %q is %d characters, over the %d budget (name = %q)",
				canonical, got, maxRelativeProjectPath, projectFileBaseName(canonical))
		}
	}
}

// TestProjectFileBaseNameBoundaryIsExact walks the threshold one character at a time. Names that fit
// must be returned untouched and names that do not must land inside the budget — an off-by-one here
// would either compress a name that never needed it (churning file names for no reason) or leave one
// over the limit (the bug).
func TestProjectFileBaseNameBoundaryIsExact(t *testing.T) {
	compressedSeen := false

	for length := 80; length <= 110; length++ {
		canonical := strings.Repeat("a", length-2) + ".z"
		got := projectFileBaseName(canonical)

		if got == canonical {
			if emittedRelativeProjectPath(canonical) > maxRelativeProjectPath {
				t.Errorf("name of length %d returned verbatim but its path is %d, over the %d budget",
					length, emittedRelativeProjectPath(canonical), maxRelativeProjectPath)
			}

			continue
		}

		compressedSeen = true

		if emittedRelativeProjectPath(canonical) > maxRelativeProjectPath {
			t.Errorf("name of length %d compressed to %q but its path is still %d, over the %d budget",
				length, got, emittedRelativeProjectPath(canonical), maxRelativeProjectPath)
		}
	}

	if !compressedSeen {
		t.Error("no name in the 80..110 range compressed — the boundary moved out of the swept window")
	}
}

// TestProjectFileBaseNameIsDeterministicAndSetIndependent is the property that lets the DECLARATION
// side (which writes the .csproj) and the REFERENCE side (getRecurseDependencyInfo, which names the
// same project from an import path alone, knowing nothing about the rest of the conversion) agree
// without communicating. It is also why the budget compresses the canonical name rather than picking a
// shortest-unique suffix: a set-dependent name would rename unrelated projects when a dependency is
// added, which is the coupling 9c970b258 removed one level up.
func TestProjectFileBaseNameIsDeterministicAndSetIndependent(t *testing.T) {
	const canonical = "github.com.envoyproxy.go-control-plane.envoy.extensions.load_balancing_policies.client_side_weighted_round_robin.v3"

	first := projectFileBaseName(canonical)

	for i := 0; i < 32; i++ {
		if got := projectFileBaseName(canonical); got != first {
			t.Fatalf("projectFileBaseName is not deterministic: %q then %q", first, got)
		}
	}

	if !strings.Contains(first, "~") {
		t.Errorf("expected an elision marker in the compressed name %q", first)
	}
}

// TestProjectFileBaseNameKeepsDistinctNamesDistinct is the uniqueness half. Compression must not undo
// what 9c970b258 established — two packages that differ only outside the retained head and tail would
// collide on the visible text, and the solution would be back to refusing to open. The hash covers the
// FULL canonical name, so they do not.
func TestProjectFileBaseNameKeepsDistinctNamesDistinct(t *testing.T) {
	const prefix = "github.com.envoyproxy.go-control-plane.envoy.extensions.load_balancing_policies."
	const suffix = ".client_side_weighted_round_robin.v3"

	names := map[string]string{}

	for _, middle := range []string{
		"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
		"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaab", "baaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
	} {
		canonical := prefix + middle + suffix
		got := projectFileBaseName(canonical)

		if got == canonical {
			t.Fatalf("test case %q was expected to exceed the budget", canonical)
		}

		if prior, dup := names[got]; dup {
			t.Errorf("compressed name %q emitted for both %q and %q — a duplicate .slnx project name",
				got, prior, canonical)
		}

		names[got] = canonical
	}
}

// TestIssue35ReplayCorpus replays a REAL conversion's whole project set through the derivation, which
// is the evidence the synthetic cases above cannot give: thousands of genuine import paths, from the
// module layouts people actually depend on, checked for the two properties that matter together —
// every emitted name distinct (or Visual Studio refuses to open the solution) and every emitted path
// inside the budget (or Visual Studio refuses to load the project).
//
// It runs against the solution file from the issue-#35 report — 1,727 projects from a real application
// — by pointing GO2CS_ISSUE35_CORPUS at it, and skips when that is unset so an ordinary `go test ./...`
// costs nothing. The corpus is not committed; it is the reporter's file, attached to the issue.
func TestIssue35ReplayCorpus(t *testing.T) {
	corpus := os.Getenv("GO2CS_ISSUE35_CORPUS")

	if corpus == "" {
		t.Skip("set GO2CS_ISSUE35_CORPUS to a generated .slnx to replay a real project set")
	}

	contents, err := os.ReadFile(corpus)

	if err != nil {
		t.Fatalf("failed to read replay corpus: %v", err)
	}

	// Recover each project's import path from its DIRECTORY, which mirrors it, rather than from the
	// file name — the file name is exactly what is under test, and the corpus predates the fix.
	names := map[string]string{}
	replayed, compressed, worst := 0, 0, 0

	for _, line := range strings.Split(string(contents), "\n") {
		start := strings.Index(line, `<Project Path="`)

		if start < 0 {
			continue
		}

		rest := line[start+len(`<Project Path="`):]
		end := strings.Index(rest, `"`)

		if end < 0 {
			continue
		}

		dir := path.Dir(strings.TrimPrefix(strings.TrimPrefix(rest[:end], "../../"), "pkg/"))

		// Skip the runtime/analyzer entries, which are not converted packages.
		if dir == "." || strings.Contains(dir, "..") {
			continue
		}

		canonical := strings.ReplaceAll(dir, "/", ".")
		emitted := projectFileBaseName(canonical)
		replayed++

		if emitted != canonical {
			compressed++
		}

		if got := emittedRelativeProjectPath(canonical); got > worst {
			worst = got
		}

		if got := emittedRelativeProjectPath(canonical); got > maxRelativeProjectPath {
			t.Errorf("%q emits a %d-character path, over the %d budget", canonical, got, maxRelativeProjectPath)
		}

		if prior, dup := names[emitted]; dup && prior != canonical {
			t.Errorf("project name %q emitted for both %q and %q — a duplicate .slnx project name",
				emitted, prior, canonical)
		}

		names[emitted] = canonical
	}

	if replayed == 0 {
		t.Fatal("replay corpus contained no project entries")
	}

	t.Logf("replayed %d projects: %d distinct names, %d compressed by the budget, longest emitted path %d",
		replayed, len(names), compressed, worst)
}
