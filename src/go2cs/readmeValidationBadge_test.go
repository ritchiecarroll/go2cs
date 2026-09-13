// readmeValidationBadge_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// badgeTree builds the minimal repository shape the badge emitter reads: a go2cs root (identified,
// as everywhere else, by core\golib\golib.csproj), src\version.props beside it, and the docs tree as
// the root's sibling. It returns the root and the package's output directory.
func badgeTree(t *testing.T, dotID string, version string) (string, string) {
	t.Helper()

	tree := t.TempDir()
	root := filepath.Join(tree, "src")
	projectPath := filepath.Join(root, "core", filepath.FromSlash(validationProofImportPath(dotID)))

	mustMkdirAll(t, filepath.Join(root, "core", "golib"))
	mustMkdirAll(t, projectPath)
	mustWriteFile(t, filepath.Join(root, "core", "golib", "golib.csproj"), "<Project />")

	if version != "" {
		parts := strings.SplitN(version, ".", 4)

		if len(parts) != 4 {
			t.Fatalf("badgeTree version %q is not a four-part version", version)
		}

		mustWriteFile(t, filepath.Join(root, versionPropsFileName), fmt.Sprintf(
			"<Project>\r\n  <PropertyGroup>\r\n    <GoStdLibVersion>%s</GoStdLibVersion>\r\n    <GoBuildNumber>%s</GoBuildNumber>\r\n  </PropertyGroup>\r\n</Project>\r\n",
			strings.Join(parts[:3], "."), parts[3]))
	}

	return root, projectPath
}

// addProofPage writes a proof page for dotID rendered by the production renderer, so the counts the
// badge reads back are counts the converter actually emits.
func addProofPage(t *testing.T, root string, dotID string, matched int, disclosed int) {
	t.Helper()

	currentPath := filepath.Join(filepath.Dir(root), "docs", validationDocsDirName, validationCurrentDirName)
	mustMkdirAll(t, currentPath)

	comparison := testComparison{Go: map[string]string{}, CSharp: map[string]string{}}

	for i := 0; i < matched; i++ {
		name := fmt.Sprintf("TestMatched%03d", i)
		comparison.Go[name] = "pass"
		comparison.CSharp[name] = "pass"
	}

	for i := 0; i < disclosed; i++ {
		name := fmt.Sprintf("TestDisclosed%03d", i)
		comparison.Go[name] = "pass"
		comparison.CSharp[name] = "fail"
	}

	page := renderValidationProofPage(proofPageProvenance{
		importPath: validationProofImportPath(dotID),
		goVersion:  "1.23.1",
		platform:   "windows/amd64",
		date:       "2026-08-02",
		commit:     "abcdef012",
	}, comparison, map[string]testDisclosure{}, nil)

	mustWriteFile(t, filepath.Join(currentPath, dotID+".md"), strings.ReplaceAll(page, "\n", "\r\n"))
}

// addGoSources writes a package source directory holding the given files, and returns its path.
func addGoSources(t *testing.T, files map[string]string) string {
	t.Helper()

	sourceDir := filepath.Join(t.TempDir(), "gosrc")
	mustMkdirAll(t, sourceDir)

	for name, contents := range files {
		mustWriteFile(t, filepath.Join(sourceDir, name), contents)
	}

	return sourceDir
}

func mustMkdirAll(t *testing.T, path string) {
	t.Helper()

	if err := os.MkdirAll(path, 0755); err != nil {
		t.Fatalf("mkdir %s: %v", path, err)
	}
}

func mustWriteFile(t *testing.T, path string, contents string) {
	t.Helper()

	if err := os.WriteFile(path, []byte(contents), 0644); err != nil {
		t.Fatalf("write %s: %v", path, err)
	}
}

// A validated package's badge is green, states matched/total (total = matched + disclosed, so the
// denominator counts every test the suite ran), and links the VERSIONED proof page — the badge IS
// the proof link, so this string is pinned verbatim.
func TestValidationBadgeGreenPinsCountsAndVersionedProofLink(t *testing.T) {
	root, projectPath := badgeTree(t, "io", "1.23.1.2")

	addProofPage(t, root, "io", 59, 2)
	mustWriteFile(t, filepath.Join(projectPath, "io"+testProjectFileSuffix), "<Project />")

	const expected = "[![Tests](https://img.shields.io/badge/Tests-59%2F61_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.1.2/io.html)"

	if badge := readmeValidationBadgeLine(projectPath, "io", ""); badge != expected {
		t.Fatalf("green badge mismatch\n got: %s\nwant: %s", badge, expected)
	}
}

// A multi-element import path becomes a flat dot-id in BOTH the proof file name and the badge URL.
func TestValidationBadgeGreenUsesDotIDForNestedPackages(t *testing.T) {
	root, projectPath := badgeTree(t, "path.filepath", "1.23.1.2")

	addProofPage(t, root, "path.filepath", 40, 0)
	mustWriteFile(t, filepath.Join(projectPath, "path.filepath"+testProjectFileSuffix), "<Project />")

	const expected = "[![Tests](https://img.shields.io/badge/Tests-40%2F40_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.1.2/path.filepath.html)"

	if badge := readmeValidationBadgeLine(projectPath, "path.filepath", ""); badge != expected {
		t.Fatalf("green badge mismatch\n got: %s\nwant: %s", badge, expected)
	}
}

// A package that HAS a Go test suite but has not been through the pipeline says exactly that.
func TestValidationBadgeOrangeForUnvalidatedTestSuite(t *testing.T) {
	root, projectPath := badgeTree(t, "fmt", "1.23.1.2")

	addProofPage(t, root, "io", 59, 2) // some OTHER package validated; docs tree exists

	sourceDir := addGoSources(t, map[string]string{
		"fmt.go":            "package fmt\n",
		"fmt_test.go":       "package fmt_test\n\nimport \"testing\"\n\nfunc TestSprintf(t *testing.T) {}\n",
		"scan_helper.go":    "package fmt\n\nfunc helper() {}\n",
		"benchmark_test.go": "package fmt\n\nimport \"testing\"\n\nfunc BenchmarkX(b *testing.B) {}\n",
	})

	const expected = "[![Tests](https://img.shields.io/badge/Tests-not_yet_validated-orange?logo=go)](https://go2cs.net/ValidatedTestPackages.html)"

	if badge := readmeValidationBadgeLine(projectPath, "fmt", sourceDir); badge != expected {
		t.Fatalf("orange badge mismatch\n got: %s\nwant: %s", badge, expected)
	}
}

// A package whose Go sources define no Test functions can never go green, so it says there is
// nothing to validate rather than implying an outstanding debt.
func TestValidationBadgeGreyForPackageWithoutTests(t *testing.T) {
	root, projectPath := badgeTree(t, "unsafe.like", "1.23.1.2")

	addProofPage(t, root, "io", 59, 2)

	sourceDir := addGoSources(t, map[string]string{
		"doc.go": "package like\n",
	})

	const expected = "[![Tests](https://img.shields.io/badge/Tests-none_to_validate-lightgrey?logo=go)](https://go2cs.net/ValidatedTestPackages.html)"

	if badge := readmeValidationBadgeLine(projectPath, "unsafe.like", sourceDir); badge != expected {
		t.Fatalf("grey badge mismatch\n got: %s\nwant: %s", badge, expected)
	}
}

// The fallback: no version.props (or no docs tree) means no honest badge can be composed, so the
// README is emitted exactly as it was before badges existed rather than carrying a half-built URL.
// This is what a bare temp -go2cspath reconvert hits, and why the seed ritual seeds both.
func TestValidationBadgeOmittedWithoutRepositoryContext(t *testing.T) {
	sourceDir := addGoSources(t, map[string]string{
		"a.go":      "package a\n",
		"a_test.go": "package a\n\nimport \"testing\"\n\nfunc TestA(t *testing.T) {}\n",
	})

	t.Run("no version.props", func(t *testing.T) {
		root, projectPath := badgeTree(t, "io", "")
		addProofPage(t, root, "io", 59, 2)
		mustWriteFile(t, filepath.Join(projectPath, "io"+testProjectFileSuffix), "<Project />")

		if badge := readmeValidationBadgeLine(projectPath, "io", sourceDir); badge != "" {
			t.Fatalf("expected no badge without version.props, got: %s", badge)
		}
	})

	t.Run("no docs tree", func(t *testing.T) {
		_, projectPath := badgeTree(t, "io", "1.23.1.2")
		mustWriteFile(t, filepath.Join(projectPath, "io"+testProjectFileSuffix), "<Project />")

		if badge := readmeValidationBadgeLine(projectPath, "io", sourceDir); badge != "" {
			t.Fatalf("expected no badge without a docs tree, got: %s", badge)
		}
	})

	t.Run("no go2cs root", func(t *testing.T) {
		if badge := readmeValidationBadgeLine(t.TempDir(), "io", sourceDir); badge != "" {
			t.Fatalf("expected no badge outside a go2cs root, got: %s", badge)
		}
	})
}

// A committed test project without a proof page (or the reverse) must not produce a green badge
// claiming counts nothing backs; it falls through to the honest has-tests classification, where the
// corpus census then shows the disagreement as a miscount.
func TestValidationBadgeRequiresBothGreenSignals(t *testing.T) {
	root, projectPath := badgeTree(t, "io", "1.23.1.2")

	addProofPage(t, root, "other", 1, 0)
	mustWriteFile(t, filepath.Join(projectPath, "io"+testProjectFileSuffix), "<Project />")

	sourceDir := addGoSources(t, map[string]string{
		"io.go":      "package io\n",
		"io_test.go": "package io\n\nimport \"testing\"\n\nfunc TestIO(t *testing.T) {}\n",
	})

	badge := readmeValidationBadgeLine(projectPath, "io", sourceDir)

	if strings.Contains(badge, "brightgreen") {
		t.Fatalf("a test project with no proof page produced a green badge: %s", badge)
	}

	if !strings.Contains(badge, "not_yet_validated") {
		t.Fatalf("expected the honest orange fallback, got: %s", badge)
	}
}

// goRootWithVendor builds a GOROOT-shaped tree holding just the src/vendor/modules.txt the Docs
// badge reads, in the exact shape the Go distribution ships it (module header, `##` annotation, then
// one package path per line).
func goRootWithVendor(t *testing.T, modules string) string {
	t.Helper()

	goRoot := t.TempDir()
	vendorDir := filepath.Join(goRoot, "src", "vendor")
	mustMkdirAll(t, vendorDir)
	mustWriteFile(t, filepath.Join(vendorDir, vendorModulesFileName), modules)

	return goRoot
}

const goRootVendorModules = "# golang.org/x/crypto v0.23.1-0.20240603234054-0b431c7de36a\n" +
	"## explicit; go 1.18\n" +
	"golang.org/x/crypto/chacha20\n" +
	"golang.org/x/crypto/internal/poly1305\n" +
	"# golang.org/x/text v0.16.0\n" +
	"## explicit; go 1.18\n" +
	"golang.org/x/text/transform\n" +
	"golang.org/x/text/unicode/norm\n"

// The Docs badge is the reader's route from the converted C# back to the Go it mirrors, so its exact
// rendering — Go brand blue, the gopher logo, the version-pinned pkg.go.dev link — is pinned here for
// each of the three shapes the corpus contains.
func TestDocsBadgePinsEachImportPathShape(t *testing.T) {
	goRoot := goRootWithVendor(t, goRootVendorModules)

	tests := []struct {
		name       string
		importPath string
		expected   string
	}{
		{
			name:       "standard package",
			importPath: "bufio",
			expected:   "[![Docs](https://img.shields.io/badge/Docs-@1.23.1-00ADD8?logo=go)](https://pkg.go.dev/bufio@go1.23.1)",
		},
		{
			name:       "nested standard package",
			importPath: "path/filepath",
			expected:   "[![Docs](https://img.shields.io/badge/Docs-@1.23.1-00ADD8?logo=go)](https://pkg.go.dev/path/filepath@go1.23.1)",
		},
		{
			// pkg.go.dev serves the standard library's internal packages like any other std
			// package, so they need no special case and get a normal release-pinned link.
			name:       "internal package",
			importPath: "internal/abi",
			expected:   "[![Docs](https://img.shields.io/badge/Docs-@1.23.1-00ADD8?logo=go)](https://pkg.go.dev/internal/abi@go1.23.1)",
		},
		{
			// A vendored package pins the MODULE version modules.txt records, in both the link and
			// the message, because that is the documentation the link actually reaches. The
			// pseudo-version's dashes are doubled: a single dash separates shields' three fields.
			name:       "vendored package",
			importPath: "vendor/golang.org/x/crypto/chacha20",
			expected:   "[![Docs](https://img.shields.io/badge/Docs-@v0.23.1--0.20240603234054--0b431c7de36a-00ADD8?logo=go)](https://pkg.go.dev/golang.org/x/crypto@v0.23.1-0.20240603234054-0b431c7de36a/chacha20)",
		},
		{
			name:       "vendored package with a plain semantic version",
			importPath: "vendor/golang.org/x/text/unicode/norm",
			expected:   "[![Docs](https://img.shields.io/badge/Docs-@v0.16.0-00ADD8?logo=go)](https://pkg.go.dev/golang.org/x/text@v0.16.0/unicode/norm)",
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if badge := readmeDocsBadgeLine(test.importPath, "1.23.1", goRoot); badge != test.expected {
				t.Fatalf("docs badge mismatch\n got: %s\nwant: %s", badge, test.expected)
			}
		})
	}
}

// An unpinnable link would point at whatever is current rather than at the sources this package
// holds, which is the one thing the badge promises — so every unresolvable input says nothing.
func TestDocsBadgeOmittedWhenItCannotBePinned(t *testing.T) {
	goRoot := goRootWithVendor(t, goRootVendorModules)

	tests := []struct {
		name       string
		importPath string
		goVersion  string
		goRoot     string
	}{
		{name: "no import path", importPath: "", goVersion: "1.23.1", goRoot: goRoot},
		{name: "no Go version", importPath: "bufio", goVersion: "", goRoot: goRoot},
		{name: "vendored package absent from modules.txt", importPath: "vendor/golang.org/x/net/idna", goVersion: "1.23.1", goRoot: goRoot},
		{name: "vendored package with no modules.txt at all", importPath: "vendor/golang.org/x/crypto/chacha20", goVersion: "1.23.1", goRoot: t.TempDir()},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if badge := readmeDocsBadgeLine(test.importPath, test.goVersion, test.goRoot); badge != "" {
				t.Fatalf("expected no docs badge, got: %s", badge)
			}
		})
	}
}

// The real distribution is the format's authority, so the parser is exercised against the actual
// modules.txt this toolchain ships rather than only against the fixture shaped like it. No version
// is asserted (it moves with the Go release) — only that a known vendored package resolves to its
// module with a pin.
func TestVendoredModulePinReadsTheRealGoRoot(t *testing.T) {
	goRoot, err := getGoEnv("GOROOT")

	if err != nil || strings.TrimSpace(goRoot) == "" {
		t.Skip("GOROOT is not resolvable")
	}

	goRoot = strings.TrimSpace(goRoot)

	if _, err := os.Stat(filepath.Join(goRoot, "src", "vendor", vendorModulesFileName)); err != nil {
		t.Skip("this GOROOT ships no vendored modules")
	}

	modulePath, pin, ok := vendoredModulePin(goRoot, "golang.org/x/text/transform")

	if !ok {
		t.Fatal("golang.org/x/text/transform did not resolve against the real GOROOT modules.txt")
	}

	if modulePath != "golang.org/x/text" {
		t.Fatalf("resolved module %q, want golang.org/x/text", modulePath)
	}

	if !strings.HasPrefix(pin, "v") {
		t.Fatalf("resolved pin %q does not look like a module version", pin)
	}
}

// The import path the Docs badge pins comes from the package's source DIRECTORY, because the
// loader's PkgPath is not stable across load configurations for exactly the two shapes that matter.
func TestStdLibImportPathComesFromTheSourceDirectory(t *testing.T) {
	goRoot := filepath.FromSlash("C:/Program Files/Go")
	goRootSrc := filepath.Join(goRoot, "src")

	tests := []struct {
		name      string
		sourceDir string
		goRoot    string
		expected  string
	}{
		{name: "standard package", sourceDir: filepath.Join(goRootSrc, "bufio"), goRoot: goRoot, expected: "bufio"},
		{name: "nested package", sourceDir: filepath.Join(goRootSrc, "path", "filepath"), goRoot: goRoot, expected: "path/filepath"},
		{name: "internal package", sourceDir: filepath.Join(goRootSrc, "internal", "abi"), goRoot: goRoot, expected: "internal/abi"},
		{
			name:      "vendored package keeps its vendor prefix",
			sourceDir: filepath.Join(goRootSrc, "vendor", "golang.org", "x", "crypto", "chacha20"),
			goRoot:    goRoot,
			expected:  "vendor/golang.org/x/crypto/chacha20",
		},
		{name: "the src root itself is no package", sourceDir: goRootSrc, goRoot: goRoot, expected: ""},
		{name: "outside GOROOT", sourceDir: filepath.FromSlash("D:/work/app/lib"), goRoot: goRoot, expected: ""},
		{name: "no source directory", sourceDir: "", goRoot: goRoot, expected: ""},
		{name: "no GOROOT", sourceDir: filepath.Join(goRootSrc, "bufio"), goRoot: "", expected: ""},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if actual := stdLibImportPath(test.sourceDir, test.goRoot); actual != test.expected {
				t.Fatalf("stdLibImportPath = %q, want %q", actual, test.expected)
			}
		})
	}

	// Windows reports GOROOT's casing differently between `go env` and the loader, and the whole
	// corpus's import paths would move if that decided anything.
	if runtime.GOOS == "windows" {
		lowered := filepath.Join(strings.ToLower(goRootSrc), "bufio")

		if actual := stdLibImportPath(lowered, goRoot); actual != "bufio" {
			t.Fatalf("a case-differing GOROOT produced %q, want bufio", actual)
		}
	}
}

// The Source·Go badge is the reader's route to the Go sources themselves (as distinct from the Docs
// badge's rendered documentation), so its exact rendering — Go brand blue, the gopher logo, the
// release-tagged tree link — is pinned here for each shape the corpus contains.
func TestGoSourceBadgePinsTheReleaseTaggedTree(t *testing.T) {
	goRoot := goRootWithVendor(t, goRootVendorModules)

	tests := []struct {
		name       string
		importPath string
		expected   string
	}{
		{
			name:       "standard package",
			importPath: "bufio",
			expected:   "[![Source](https://img.shields.io/badge/Source-@1.23.1-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.1/src/bufio)",
		},
		{
			name:       "nested standard package",
			importPath: "path/filepath",
			expected:   "[![Source](https://img.shields.io/badge/Source-@1.23.1-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.1/src/path/filepath)",
		},
		{
			name:       "internal package",
			importPath: "internal/abi",
			expected:   "[![Source](https://img.shields.io/badge/Source-@1.23.1-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.1/src/internal/abi)",
		},
		{
			// The link is the VENDORED tree — literally the directory the converter read — while the
			// message carries the module pin modules.txt records, since these sources are a
			// third-party snapshot rather than a Go release artifact. The pseudo-version's dashes are
			// doubled: a single dash separates shields' three fields.
			name:       "vendored package",
			importPath: "vendor/golang.org/x/crypto/chacha20",
			expected:   "[![Source](https://img.shields.io/badge/Source-@v0.23.1--0.20240603234054--0b431c7de36a-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.1/src/vendor/golang.org/x/crypto/chacha20)",
		},
		{
			name:       "vendored package with a plain semantic version",
			importPath: "vendor/golang.org/x/text/unicode/norm",
			expected:   "[![Source](https://img.shields.io/badge/Source-@v0.16.0-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.1/src/vendor/golang.org/x/text/unicode/norm)",
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if badge := readmeGoSourceBadgeLine(test.importPath, "1.23.1", goRoot); badge != test.expected {
				t.Fatalf("Go source badge mismatch\n got: %s\nwant: %s", badge, test.expected)
			}
		})
	}
}

// Where the Docs badge must go silent, this one need not: its link's address is the GOROOT tree at
// the Go release tag, which an unresolvable module pin does not affect. The pin only sharpens the
// text, so losing it degrades the text and keeps the link.
func TestGoSourceBadgeFallsBackToTheReleaseWhenTheModulePinIsUnresolvable(t *testing.T) {
	const importPath = "vendor/golang.org/x/net/idna"
	const expected = "[![Source](https://img.shields.io/badge/Source-@1.23.1-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.1/src/vendor/golang.org/x/net/idna)"

	// Absent from modules.txt, and a GOROOT with no modules.txt at all: both keep the badge.
	for name, goRoot := range map[string]string{
		"absent from modules.txt": goRootWithVendor(t, goRootVendorModules),
		"no modules.txt at all":   t.TempDir(),
	} {
		t.Run(name, func(t *testing.T) {
			if badge := readmeGoSourceBadgeLine(importPath, "1.23.1", goRoot); badge != expected {
				t.Fatalf("Go source badge mismatch\n got: %s\nwant: %s", badge, expected)
			}
		})

		// The Docs badge, by contrast, composes its ADDRESS from the pin and must say nothing.
		if badge := readmeDocsBadgeLine(importPath, "1.23.1", goRoot); badge != "" {
			t.Fatalf("the docs badge should stay silent without a pin, got: %s", badge)
		}
	}

	// Nothing to link and nothing to pin the tree with are both silence, not a half-built URL.
	if badge := readmeGoSourceBadgeLine("", "1.23.1", ""); badge != "" {
		t.Fatalf("expected no badge without an import path, got: %s", badge)
	}

	if badge := readmeGoSourceBadgeLine("bufio", "", ""); badge != "" {
		t.Fatalf("expected no badge without a Go version, got: %s", badge)
	}
}

// The Source·C# badge links the converted C# at the RELEASE TAG the reader's package version was
// published under, with the path taken from where the conversion is writing.
func TestCSharpSourceBadgePinsTheReleaseTag(t *testing.T) {
	_, projectPath := badgeTree(t, "io", "1.23.1.2")

	const expected = "[![Source](https://img.shields.io/badge/Source-@1.23.1.2-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.1.2/src/core/io)"

	badge := readmeCSharpSourceBadgeLine(projectPath)

	if badge != expected {
		t.Fatalf("C# source badge mismatch\n got: %s\nwant: %s", badge, expected)
	}

	// The badge carries no language text at all now (the logo alone says .NET, matching the Docs
	// badge style), so there is no '#' anywhere in the line to open a URL fragment and truncate it.
	// Kept as a standing check on the whole badge line, not just on the now-vanished "C#" label.
	if strings.Contains(badge, "#") {
		t.Fatalf("the badge carries a raw '#': %s", badge)
	}

	if !strings.Contains(badge, "badge/Source-@") {
		t.Fatalf("the badge does not carry the language-less Source label: %s", badge)
	}

	// A nested package's link is its nested DIRECTORY (forward-slashed for the URL), not the flat
	// dot-id the proof pages use.
	_, nestedPath := badgeTree(t, "path.filepath", "1.23.1.2")

	const nestedExpected = "[![Source](https://img.shields.io/badge/Source-@1.23.1.2-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.1.2/src/core/path/filepath)"

	if badge := readmeCSharpSourceBadgeLine(nestedPath); badge != nestedExpected {
		t.Fatalf("nested C# source badge mismatch\n got: %s\nwant: %s", badge, nestedExpected)
	}

	// A vendored package's converted C# sits at the same vendor-prefixed path its Go sources do. Its
	// directory is built explicitly rather than through badgeTree's dot-id mapping, because a module
	// path's own dots (`golang.org`) do not survive that round trip.
	root, _ := badgeTree(t, "io", "1.23.1.2")
	vendoredPath := filepath.Join(root, "core", "vendor", "golang.org", "x", "crypto", "chacha20")
	mustMkdirAll(t, vendoredPath)

	const vendoredExpected = "[![Source](https://img.shields.io/badge/Source-@1.23.1.2-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.1.2/src/core/vendor/golang.org/x/crypto/chacha20)"

	if badge := readmeCSharpSourceBadgeLine(vendoredPath); badge != vendoredExpected {
		t.Fatalf("vendored C# source badge mismatch\n got: %s\nwant: %s", badge, vendoredExpected)
	}
}

// The same fallback the Tests badge has, for the same reason: without version.props there is no
// published version, so there is no release tag to pin and nothing honest to link.
func TestCSharpSourceBadgeOmittedWithoutAPublishedVersion(t *testing.T) {
	t.Run("no version.props", func(t *testing.T) {
		_, projectPath := badgeTree(t, "io", "")

		if badge := readmeCSharpSourceBadgeLine(projectPath); badge != "" {
			t.Fatalf("expected no badge without version.props, got: %s", badge)
		}
	})

	t.Run("no go2cs root", func(t *testing.T) {
		if badge := readmeCSharpSourceBadgeLine(t.TempDir()); badge != "" {
			t.Fatalf("expected no badge outside a go2cs root, got: %s", badge)
		}
	})

	// Under a go2cs root but NOT beneath its core/ — a -recurse conversion's output — has no place
	// in the go2cs repository, so there is no path to link.
	t.Run("outside the root's core directory", func(t *testing.T) {
		root, _ := badgeTree(t, "io", "1.23.1.2")
		outside := filepath.Join(root, "app", "mypkg")
		mustMkdirAll(t, outside)

		if badge := readmeCSharpSourceBadgeLine(outside); badge != "" {
			t.Fatalf("expected no badge outside core/, got: %s", badge)
		}

		if badge := readmeCSharpSourceBadgeLine(filepath.Join(root, "core")); badge != "" {
			t.Fatalf("the core root itself is no package, got: %s", badge)
		}
	})
}

// The badge paragraph is the four badges in order — Tests, Docs, then the Source pair in
// convert-from → convert-to order — over TWO lines: Tests and Docs share the first (Tests is the
// variable-width badge and gets the room), the Source pair the second, and a trailing-backslash hard
// break joins them so a narrow renderer breaks where the meaning does. The whole thing collapses to
// nothing when no badge can be composed, which keeps a context-less conversion's README as it was.
//
// The break form is pinned as a literal rather than through badgeLineBreak: the constant is the thing
// under test, and a two-trailing-spaces (or `<br/>`) substitution has to fail here.
func TestReadmeBadgeParagraphSplitsTestsDocsFromBothSources(t *testing.T) {
	root, projectPath := badgeTree(t, "io", "1.23.1.2")

	addProofPage(t, root, "io", 59, 2)
	mustWriteFile(t, filepath.Join(projectPath, "io"+testProjectFileSuffix), "<Project />")

	// The source directory doubles as the import-path source, so it has to sit under this fixture's
	// own GOROOT for the docs and Go source badges to resolve.
	goRoot := goRootWithVendor(t, goRootVendorModules)
	sourceDir := filepath.Join(goRoot, "src", "io")
	mustMkdirAll(t, sourceDir)

	if goVersion() == "" {
		t.Skip("the Go toolchain version is not resolvable")
	}

	expected := "[![Tests](https://img.shields.io/badge/Tests-59%2F61_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.1.2/io.html)" +
		" " + fmt.Sprintf("[![Docs](https://img.shields.io/badge/Docs-@%s-00ADD8?logo=go)](https://pkg.go.dev/io@go%s)", goVersion(), goVersion()) +
		"\\\n" + fmt.Sprintf("[![Source](https://img.shields.io/badge/Source-@%s-00ADD8?logo=go)](https://github.com/golang/go/tree/go%s/src/io)", goVersion(), goVersion()) +
		" " + "[![Source](https://img.shields.io/badge/Source-@1.23.1.2-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.1.2/src/core/io)"

	line := readmeBadgeLine(projectPath, "io", sourceDir, Options{goRoot: goRoot})

	if line != expected {
		t.Fatalf("badge paragraph mismatch\n got: %q\nwant: %q", line, expected)
	}

	// Structure, stated independently of the pinned string, because these are the properties the
	// renderers actually key on: exactly two lines, the first ending in the hard break, and no line
	// carrying trailing whitespace (which is what the form this replaced depended on) or standing
	// empty (which would make it two paragraphs instead of one).
	lines := strings.Split(line, "\n")

	if len(lines) != 2 {
		t.Fatalf("expected two badge lines, got %d: %q", len(lines), line)
	}

	if !strings.HasSuffix(lines[0], "\\") {
		t.Fatalf("the first badge line does not end in a hard break: %q", lines[0])
	}

	for i, one := range lines {
		if strings.TrimSpace(one) == "" {
			t.Fatalf("badge line %d is empty: %q", i+1, line)
		}

		if trimmed := strings.TrimRight(one, " \t"); trimmed != one {
			t.Fatalf("badge line %d carries trailing whitespace: %q", i+1, one)
		}
	}

	// The repository badges and the toolchain badges fail independently. An unseeded reconvert (no
	// version.props, no docs tree) keeps Docs and Source·Go and drops Tests and Source·C# — one badge
	// on each line, so the break survives with a single badge either side of it. Pinned whole: this is
	// the degraded shape a temp-root reconvert emits, and it must not grow a stray break or an empty
	// line at either end.
	unseededExpected := fmt.Sprintf("[![Docs](https://img.shields.io/badge/Docs-@%s-00ADD8?logo=go)](https://pkg.go.dev/io@go%s)", goVersion(), goVersion()) +
		"\\\n" + fmt.Sprintf("[![Source](https://img.shields.io/badge/Source-@%s-00ADD8?logo=go)](https://github.com/golang/go/tree/go%s/src/io)", goVersion(), goVersion())

	if unseeded := readmeBadgeLine(t.TempDir(), "io", sourceDir, Options{goRoot: goRoot}); unseeded != unseededExpected {
		t.Fatalf("unseeded badge paragraph mismatch\n got: %q\nwant: %q", unseeded, unseededExpected)
	}

	// No badge composable at all — no repository context AND no GOROOT — is an empty paragraph, not a
	// stray separator or a lone break.
	if line := readmeBadgeLine(t.TempDir(), "io", sourceDir, Options{}); line != "" {
		t.Fatalf("expected an empty badge paragraph, got: %q", line)
	}
}

// A line that has no badges at all must not leave the break behind. The break is composed as a JOIN
// between the lines present, so each of these shapes emits ONE line and no break — the failure mode
// a template-shaped emitter would have (a leading "\" line, or a trailing one that renders as a
// literal backslash at the end of the paragraph).
func TestReadmeBadgeParagraphDegradesToOneLineWithoutAStrayBreak(t *testing.T) {
	if goVersion() == "" {
		t.Skip("the Go toolchain version is not resolvable")
	}

	// FIRST LINE ONLY. A conversion whose output sits under a go2cs root but outside its core/ (a
	// -recurse conversion) can compose Tests from the repository, but has no place in the go2cs
	// repository for Source·C#; with no GOROOT resolved, neither toolchain badge composes either.
	t.Run("tests only", func(t *testing.T) {
		root, _ := badgeTree(t, "io", "1.23.1.2")
		outside := filepath.Join(root, "app", "mypkg")
		mustMkdirAll(t, outside)
		addProofPage(t, root, "io", 59, 2)
		mustWriteFile(t, filepath.Join(outside, "io"+testProjectFileSuffix), "<Project />")

		const expected = "[![Tests](https://img.shields.io/badge/Tests-59%2F61_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.1.2/io.html)"

		if line := readmeBadgeLine(outside, "io", "", Options{}); line != expected {
			t.Fatalf("first-line-only paragraph mismatch\n got: %q\nwant: %q", line, expected)
		}
	})

	// SECOND LINE ONLY. The one shape where Source·Go outlives the Docs badge beside it: a vendored
	// package with no modules.txt entry has no pinnable documentation URL (Docs says nothing) while
	// its GOROOT tree link is fully pinned by the Go release alone. Outside any go2cs root, so
	// neither repository badge composes and the Source pair is alone on the second line.
	t.Run("go source only", func(t *testing.T) {
		goRoot := goRootWithVendor(t, goRootVendorModules)
		sourceDir := filepath.Join(goRoot, "src", "vendor", "golang.org", "x", "net", "idna")
		mustMkdirAll(t, sourceDir)

		expected := fmt.Sprintf("[![Source](https://img.shields.io/badge/Source-@%s-00ADD8?logo=go)](https://github.com/golang/go/tree/go%s/src/vendor/golang.org/x/net/idna)", goVersion(), goVersion())

		if line := readmeBadgeLine(t.TempDir(), "golang.org.x.net.idna", sourceDir, Options{goRoot: goRoot}); line != expected {
			t.Fatalf("second-line-only paragraph mismatch\n got: %q\nwant: %q", line, expected)
		}
	})
}

// The census predicate — the rule the roster's own denominator is built on — applied to package
// source directories. `func Test...` text inside comments and string literals is everywhere in the
// standard library's test sources, so the predicate parses rather than greps.
func TestPackageDeclaresGoTests(t *testing.T) {
	tests := []struct {
		name     string
		files    map[string]string
		expected bool
	}{
		{
			name:     "no test files at all",
			files:    map[string]string{"a.go": "package a\n\nfunc TestNotATest() {}\n"},
			expected: false,
		},
		{
			name:     "test file with a test",
			files:    map[string]string{"a_test.go": "package a\n\nimport \"testing\"\n\nfunc TestThing(t *testing.T) {}\n"},
			expected: true,
		},
		{
			name:     "bare Test is a test",
			files:    map[string]string{"a_test.go": "package a\n\nimport \"testing\"\n\nfunc Test(t *testing.T) {}\n"},
			expected: true,
		},
		{
			name:     "lower-case suffix is not a test",
			files:    map[string]string{"a_test.go": "package a\n\nfunc Testify() {}\n"},
			expected: false,
		},
		{
			name:     "underscore suffix is a test",
			files:    map[string]string{"a_test.go": "package a\n\nimport \"testing\"\n\nfunc Test_thing(t *testing.T) {}\n"},
			expected: true,
		},
		{
			name:     "a method named TestX is not a test",
			files:    map[string]string{"a_test.go": "package a\n\ntype s struct{}\n\nfunc (s) TestThing() {}\n"},
			expected: false,
		},
		{
			name:     "benchmarks and examples alone are not tests",
			files:    map[string]string{"a_test.go": "package a\n\nimport \"testing\"\n\nfunc BenchmarkX(b *testing.B) {}\n\nfunc ExampleY() {}\n"},
			expected: false,
		},
		{
			name:     "a mention in a comment or string is not a test",
			files:    map[string]string{"a_test.go": "package a\n\n// func TestFake(t *testing.T) is documented here.\nvar src = `func TestAlsoFake(t *testing.T) {}`\n"},
			expected: false,
		},
		{
			name: "found in the second file",
			files: map[string]string{
				"a_test.go": "package a\n\nfunc helper() {}\n",
				"b_test.go": "package a\n\nimport \"testing\"\n\nfunc TestThing(t *testing.T) {}\n",
			},
			expected: true,
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if actual := packageDeclaresGoTests(addGoSources(t, test.files)); actual != test.expected {
				t.Fatalf("packageDeclaresGoTests = %v, want %v", actual, test.expected)
			}
		})
	}

	if packageDeclaresGoTests(filepath.Join(t.TempDir(), "missing")) {
		t.Fatal("a nonexistent source directory reported tests")
	}
}

// parseProofTotals is derived from the renderer's own format string; this is the round trip that
// keeps them honest.
func TestParseProofTotalsRoundTripsTheRenderer(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)
	page := renderValidationProofPage(fixtureProvenance(), comparison, disclosures, nil)

	names := proofVerdictNames(comparison)
	expectedDisclosed := len(proofDisclosedNames(comparison, names, disclosures))
	expectedMatched := len(names) - expectedDisclosed

	matched, disclosed, ok := parseProofTotals(page)

	if !ok {
		t.Fatal("the renderer's own page did not parse")
	}

	if matched != expectedMatched || disclosed != expectedDisclosed {
		t.Fatalf("parsed %d matched / %d disclosed, want %d / %d", matched, disclosed, expectedMatched, expectedDisclosed)
	}

	// CRLF on disk must read the same as freshly rendered text.
	if crlfMatched, crlfDisclosed, crlfOK := parseProofTotals(strings.ReplaceAll(page, "\n", "\r\n")); !crlfOK || crlfMatched != matched || crlfDisclosed != disclosed {
		t.Fatalf("a CRLF page parsed differently: %d/%d ok=%v", crlfMatched, crlfDisclosed, crlfOK)
	}

	if _, _, ok := parseProofTotals("# not a proof page\n\nnothing to see\n"); ok {
		t.Fatal("a page with no totals line parsed anyway")
	}
}

// The badge's dot-id and the proof page's file name are the same mapping, in both directions.
func TestValidationProofDotIDRoundTrip(t *testing.T) {
	for _, importPath := range []string{"io", "path/filepath", "math/rand/v2", "crypto/internal/fips140/aes"} {
		dotID := validationProofDotID(importPath)

		if strings.Contains(dotID, "/") {
			t.Fatalf("dot-id %q still holds a path separator", dotID)
		}

		if actual := validationProofImportPath(dotID); actual != importPath {
			t.Fatalf("round trip of %q produced %q", importPath, actual)
		}
	}
}

// The README is what CARRIES the badge, so the gate deciding when it gets written decides when the
// badge can ever refresh. Gated on convertStdLib alone, only a whole-stdlib reconvert could level a
// badge — so every bank shipped its package advertising a stale Tests badge until some later,
// unrelated run happened to catch it (the board's "stale-validation-badge class is a PIPELINE gap").
// The gate is now the same structural OUTPUT-LOCATION test validationPackBlock's uses, and this
// guard holds BOTH halves of it: a corpus package regenerates its README whatever the invocation
// mode, and nothing outside the runtime root's core\ tree ever gets one.
//
// Fixture paths are composed with filepath.Join rather than written as Windows literals, for the
// reason recorded on TestValidationPackBlockSurvivesTestsRewriteOfCorePackage: the predicate is a
// real path-prefix comparison, and a backslash literal is ONE filename on Linux and macOS.
func TestPackageReadmeEmissionFollowsPackageProvenanceNotRunMode(t *testing.T) {
	root := filepath.Join(t.TempDir(), "src")
	corePackage := filepath.Join(root, "core", "time", "time.csproj")
	outsideCore := filepath.Join(root, "tests", "Behavioral", "Arrays", "Arrays.csproj")

	// A -stdlib run writes every package's README, as it always has.
	if !emitsPackageReadme(corePackage, Options{convertStdLib: true}) {
		t.Fatal("a -stdlib conversion of a core package did not emit its README")
	}

	// The regression this closes: a -tests run re-emits the package under test, and that rewrite is
	// the one which follows the compare that just refreshed the package's proof page.
	if !emitsPackageReadme(corePackage, Options{convertTests: true, go2csPath: root}) {
		t.Fatal("a -tests rewrite of a core package did not emit its README, so its badge could not refresh")
	}

	// The SINGLE-PACKAGE form — `go2cs <goroot-pkg> <core-pkg>`, how a lane regenerates one corpus
	// package after a converter change — is neither -stdlib nor -tests and must level the badge too.
	if !emitsPackageReadme(corePackage, Options{go2csPath: root}) {
		t.Fatal("a single-package reconvert of a core package did not emit its README")
	}

	// The litter rule the gate exists for, in all three modes that reach non-corpus output.
	for _, mode := range []struct {
		name    string
		options Options
	}{
		{"-tests", Options{convertTests: true, go2csPath: root}},
		{"single-package", Options{go2csPath: root}},
		{"-recurse", Options{recurse: true, go2csPath: root}},
	} {
		if emitsPackageReadme(outsideCore, mode.options) {
			t.Fatalf("a %s conversion outside the core tree littered a README", mode.name)
		}
	}

	// With no resolved runtime root there is no core tree to be under, so nothing outside -stdlib
	// may emit — this is what keeps a bare conversion with no deployed root from writing READMEs.
	if emitsPackageReadme(corePackage, Options{convertTests: true}) {
		t.Fatal("a -tests conversion with no resolved runtime root emitted a README")
	}
}
// The gate above decides WHETHER a corpus package gets a README; this decides WHEN its Tests badge
// can be current, and they are different questions. The badge is composed during CONVERSION from
// docs/validation/current/<dot-id>.md, while the run that WRITES that page is the compare at the END
// of a `-test-action all` — so a package whose counts CHANGE (every fresh bank) emitted one run
// behind and read `not_yet_validated` beside its own green proof page. refreshPackageReadmeAfterProof
// is the second emission point that closes it.
//
// Both arms are pinned here because the hazard is the mirror of the fix. `-test-action
// build|run|compare` do NOT convert, so packageDoc/packageSourceDir are empty on those paths and a
// refresh that re-read them would write a DOC-LESS README over a corpus package — corpus-wide
// destruction reading as ordinary reconvert drift. The refresh therefore runs from the record the
// conversion-time write leaves behind, which is what makes that rewrite unrepresentable: the globals
// are deliberately CLEARED below and the refreshed README still carries its doc.
func TestPackageReadmeRefreshFollowsInProcessConversionNotRunMode(t *testing.T) {
	const (
		dotID    = "crypto.rc4"
		doc      = "Package rc4 implements RC4 encryption, as defined in Bruce Schneier's Applied Cryptography."
		sentinel = "# untouched by any refresh\r\n"
	)

	root, projectPath := badgeTree(t, dotID, "1.23.1.6")
	readmePath := filepath.Join(projectPath, "README.md")
	options := Options{go2csPath: root}

	// Both green signals must agree, so the banked test project has to be there for the badge to
	// ever leave orange — see readmeValidationBadgeLine.
	mustWriteFile(t, filepath.Join(projectPath, dotID+testProjectFileSuffix), "<Project />")

	sourceDir := addGoSources(t, map[string]string{
		"rc4.go":      "package rc4\n",
		"rc4_test.go": "package rc4\n\nimport \"testing\"\n\nfunc TestGolden(t *testing.T) {}\n",
	})

	// The proof directory itself exists in a real tree (it holds every OTHER package's page); only
	// this package's page is missing before its first compare. Without it the Tests badge is omitted
	// entirely rather than orange — the documented unseeded-root fallback, not a fresh bank.
	mustMkdirAll(t, filepath.Join(filepath.Dir(root), "docs", validationDocsDirName, validationCurrentDirName))

	t.Cleanup(func() {
		convertedPackageReadme = nil
		packageDoc = ""
		packageSourceDir = ""
	})

	readme := func() string {
		contents, err := os.ReadFile(readmePath)

		if err != nil {
			t.Fatalf("read README: %v", err)
		}

		return string(contents)
	}

	// ---- the state a FRESH bank is in: converted, no proof page yet ----

	if err := writeReadmeFile(projectPath, dotID, doc, sourceDir, options); err != nil {
		t.Fatalf("conversion-time README: %v", err)
	}

	recordPackageReadmeEmission(projectPath, dotID, doc, sourceDir)

	if !strings.Contains(readme(), "Tests-not_yet_validated-orange") {
		t.Fatalf("a package with tests and no proof page must read not_yet_validated:\n%s", readme())
	}

	// ---- the compare writes the page, and the SAME run levels the badge ----

	addProofPage(t, root, dotID, 2, 0)

	// The converter globals a `compare` path would have: empty. The refresh must not read them.
	packageDoc = ""
	packageSourceDir = ""

	if err := refreshPackageReadmeAfterProof(projectPath, options); err != nil {
		t.Fatalf("refresh after proof: %v", err)
	}

	refreshed := readme()

	if !strings.Contains(refreshed, "Tests-2%2F2_validated-brightgreen") {
		t.Errorf("the refresh did not level the badge in the same run:\n%s", refreshed)
	}

	if !strings.Contains(refreshed, "/"+validationDocsDirName+"/1.23.1.6/"+dotID+".html") {
		t.Errorf("the levelled badge does not link its versioned proof page:\n%s", refreshed)
	}

	if !strings.Contains(refreshed, "Package rc4 implements RC4 encryption") {
		t.Errorf("the refresh wrote a DOC-LESS README — it read the cleared globals, not the record:\n%s", refreshed)
	}

	// ---- the hazard arm: a run that did not convert cannot write a README at all ----

	convertedPackageReadme = nil
	mustWriteFile(t, readmePath, sentinel)

	if err := refreshPackageReadmeAfterProof(projectPath, options); err != nil {
		t.Fatalf("refresh with no conversion record: %v", err)
	}

	if got := readme(); got != sentinel {
		t.Errorf("a build/run/compare-only run rewrote a README:\n%s", got)
	}

	// ...and it does not CREATE one either, which is the corpus-wide shape of the same hazard.
	if err := os.Remove(readmePath); err != nil {
		t.Fatal(err)
	}

	if err := refreshPackageReadmeAfterProof(projectPath, options); err != nil {
		t.Fatalf("refresh with no README present: %v", err)
	}

	if _, err := os.Stat(readmePath); !os.IsNotExist(err) {
		t.Errorf("a build/run/compare-only run created a README where the conversion wrote none")
	}

	// ---- and a record belongs to the package that left it, not to whatever run reaches the page ----

	recordPackageReadmeEmission(filepath.Join(filepath.Dir(projectPath), "des"), "crypto.des", doc, sourceDir)

	if err := refreshPackageReadmeAfterProof(projectPath, options); err != nil {
		t.Fatalf("refresh with a foreign record: %v", err)
	}

	if _, err := os.Stat(readmePath); !os.IsNotExist(err) {
		t.Errorf("another package's emission record refreshed this package's README")
	}
}
