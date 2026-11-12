// internalPackageMarking_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"slices"
	"strings"
	"testing"
)

// The marking of Go internal and vendored packages (docs/phase4/DESIGN-internal-package-marking.md, OQ1): the root
// rule Go enforces, the [GoInternalRoot] assembly stamp, and the EditorBrowsable line above the package class.

// TestGoInternalRootMirrorsGosImportRule holds goInternalRoot to cmd/go's findInternal and disallowVendor: the root of
// the FINAL `internal` element, the parent of `vendor`, and the tighter (longer) root when a path has both.
func TestGoInternalRootMirrorsGosImportRule(t *testing.T) {
	cases := []struct {
		path       string
		root       string
		restricted bool
	}{
		{"internal/poll", "", true},
		{"internal", "", true},
		{"crypto/internal/fips140/aes", "crypto", true},
		{"crypto/internal", "crypto", true},
		{"net/http/internal/ascii", "net/http", true},
		{"internal/trace/internal/oldtrace", "internal/trace", true},
		{"vendor/golang.org/x/net/idna", "", true},
		{"vendor/golang.org/x/crypto/internal/alias", "vendor/golang.org/x/crypto", true},
		{"example.com/m/internal/x", "example.com/m", true},
		{"RootedUsingCapture/internal/sync", "RootedUsingCapture", true},
		{"crypto/aes", "", false},
		{"internalx/poll", "", false},
		{"my/internals", "", false},
		{"vendored/x", "", false},
		{"", "", false},
	}

	for _, c := range cases {
		root, restricted := goInternalRoot(c.path)

		if root != c.root || restricted != c.restricted {
			t.Errorf("goInternalRoot(%q) = (%q, %v), want (%q, %v)", c.path, root, restricted, c.root, c.restricted)
		}
	}
}

// internalMarkingFixture is a package_info.cs with the anchors the writer looks for: a position-map section and the
// package class with its [GoPackage] stamp.
func internalMarkingFixture() []string {
	return []string{
		"namespace go.crypto.@internal.fips140;",
		"",
		goSourcePositionMapsProseLines()[0],
		"",
		"// <GoSourcePositionMaps>",
		"// </GoSourcePositionMaps>",
		"",
		"[GoPackage(\"aes\")]",
		"public static partial class aes_package",
		"{",
		"}",
	}
}

// markAs sets the package state applyInternalRootStamp and applyInternalPackageBrowsable read, and restores it after.
func markAs(t *testing.T, importPath string, outsideGoroot bool) {
	t.Helper()

	savedPath, savedOutside, savedVendored := currentPackagePath, currentPackageOutsideGoroot, currentPackageGorootVendored
	t.Cleanup(func() {
		currentPackagePath, currentPackageOutsideGoroot, currentPackageGorootVendored = savedPath, savedOutside, savedVendored
	})

	currentPackagePath, currentPackageOutsideGoroot, currentPackageGorootVendored = importPath, outsideGoroot, false
}

func applyInternalMarking(lines []string, fileName string) []string {
	return applyInternalPackageBrowsable(applyInternalRootStamp(lines, fileName), fileName)
}

// TestInternalRootStampIsWrittenForEachKindOfRestrictedPackage reads the stamp the writer gives each of the design's
// packages, and its absence for a public one.
func TestInternalRootStampIsWrittenForEachKindOfRestrictedPackage(t *testing.T) {
	cases := []struct {
		path          string
		outsideGoroot bool
		want          string // the stamp line, or "" for none
	}{
		{"internal/poll", false, `[assembly: GoInternalRoot("", Standard = true)]`},
		{"crypto/internal/fips140/aes", false, `[assembly: GoInternalRoot("crypto", Standard = true)]`},
		{"vendor/golang.org/x/net/idna", false, `[assembly: GoInternalRoot("", Standard = true)]`},
		{"vendor/golang.org/x/crypto/internal/alias", false, `[assembly: GoInternalRoot("vendor/golang.org/x/crypto", Standard = true)]`},
		{"example.com/m/internal/x", true, `[assembly: GoInternalRoot("example.com/m")]`},
		{"crypto/aes", false, ""},
	}

	for _, c := range cases {
		t.Run(c.path, func(t *testing.T) {
			markAs(t, c.path, c.outsideGoroot)

			lines := applyInternalMarking(append([]string{"using go;", ""}, internalMarkingFixture()...), PackageInfoFileName)
			stamps := slices.DeleteFunc(slices.Clone(lines), func(line string) bool { return !strings.Contains(line, "GoInternalRoot") })
			browsable := slices.Index(lines, internalPackageBrowsableLine)

			if c.want == "" {
				if len(stamps) != 0 || browsable >= 0 {
					t.Fatalf("a public package is not marked; got stamps %q, EditorBrowsable at %d", stamps, browsable)
				}

				return
			}

			if len(stamps) != 1 || stamps[0] != c.want {
				t.Fatalf("stamp = %q, want exactly %q", stamps, c.want)
			}

			stampAt := slices.Index(lines, c.want)
			mapsAt := slices.Index(lines, goSourcePositionMapsProseLines()[0])

			if stampAt < 0 || mapsAt < 0 || stampAt > mapsAt {
				t.Fatalf("the stamp sits above the position-map section (stamp %d, maps %d):\n%s", stampAt, mapsAt, strings.Join(lines, "\n"))
			}

			if lines[stampAt+1] != "" {
				t.Fatalf("a blank line follows the stamp, as after every assembly stamp; got %q", lines[stampAt+1])
			}

			packageAt := slices.Index(lines, `[GoPackage("aes")]`)

			if browsable < 0 || browsable != packageAt-1 {
				t.Fatalf("the EditorBrowsable line sits directly above [GoPackage] (browsable %d, GoPackage %d):\n%s", browsable, packageAt, strings.Join(lines, "\n"))
			}

			if lines[packageAt+1] != "public static partial class aes_package" {
				t.Fatalf("nothing comes between [GoPackage] and the class, or go.lib.pack.targets cannot read the class: %q", lines[packageAt+1])
			}
		})
	}
}

// TestInternalRootStampIsSpelledThroughRootAttributeName: a file without `using go;` spells the stamp rooted, as
// every other golib assembly stamp is.
func TestInternalRootStampIsSpelledThroughRootAttributeName(t *testing.T) {
	markAs(t, "internal/poll", false)

	lines := applyInternalMarking(internalMarkingFixture(), PackageInfoFileName)
	want := "[assembly: " + rootAttributeName("GoInternalRoot", false) + `("", Standard = true)]`

	if !slices.Contains(lines, want) {
		t.Fatalf("want %q in:\n%s", want, strings.Join(lines, "\n"))
	}
}

// TestInternalMarkingIsIdempotentAndRemovedOnDemotion (S2): a persisted file is patched in place, so a second write
// gives the same bytes, and a package that is no longer internal loses both lines with their prose and blank line.
func TestInternalMarkingIsIdempotentAndRemovedOnDemotion(t *testing.T) {
	markAs(t, "crypto/internal/fips140/aes", false)

	fixture := append([]string{"using go;", ""}, internalMarkingFixture()...)
	once := applyInternalMarking(slices.Clone(fixture), PackageInfoFileName)
	twice := applyInternalMarking(slices.Clone(once), PackageInfoFileName)

	if !slices.Equal(once, twice) {
		t.Fatalf("a second write changes the file:\nonce:\n%s\ntwice:\n%s", strings.Join(once, "\n"), strings.Join(twice, "\n"))
	}

	markAs(t, "crypto/aes", false)

	demoted := applyInternalMarking(slices.Clone(once), PackageInfoFileName)

	if !slices.Equal(demoted, fixture) {
		t.Fatalf("a demoted package returns to the unmarked file:\ngot:\n%s\nwant:\n%s", strings.Join(demoted, "\n"), strings.Join(fixture, "\n"))
	}
}

// TestTestInfoFilesAreNotMarked: a test assembly is never referenced, so only package_info.cs carries the marking.
func TestTestInfoFilesAreNotMarked(t *testing.T) {
	markAs(t, "internal/poll", false)

	for _, name := range []string{testPackageInfoFileName, "package_info_internal_test.cs", "package_info_external_test.cs"} {
		fixture := append([]string{"using go;", ""}, internalMarkingFixture()...)
		lines := applyInternalMarking(slices.Clone(fixture), filepath.Join("x", name))

		if !slices.Equal(lines, fixture) {
			t.Errorf("%s is not marked:\n%s", name, strings.Join(lines, "\n"))
		}
	}
}

// TestInternalPackageOfAModuleIsMarkedAndReconvertsToTheSameBytes converts a module whose internal package a public
// one imports. The internal package's info file carries both lines, the public one neither, and a second conversion
// into the same output, which patches the persisted files in place, writes the same bytes (S2).
func TestInternalPackageOfAModuleIsMarkedAndReconvertsToTheSameBytes(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/marked\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "internal", "x", "x.go"), "package x\n\nfunc Two() int { return 2 }\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), "package lib\n\nimport \"example.com/marked/internal/x\"\n\nfunc Four() int { return x.Two() * 2 }\n")

	out := filepath.Join(root, "out")
	convert := func() (internalInfo, publicInfo string) {
		t.Helper()

		options := Options{
			goRoot:              build.Default.GOROOT,
			goPath:              build.Default.GOPATH,
			go2csPath:           out,
			recurse:             true,
			targetPlatform:      "linux/amd64",
			indentSpaces:        4,
			preferVarDecl:       true,
			useChannelOperators: true,
		}

		if options.goRoot == "" {
			options.goRoot = runtime.GOROOT()
		}

		build.Default.GOROOT = options.goRoot

		if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
			t.Fatalf("ConvertModule: %v", err)
		}

		base := filepath.Join(out, "src", "example.com", "marked")

		return readGenerated(t, filepath.Join(base, "internal", "x", PackageInfoFileName)), readGenerated(t, filepath.Join(base, "lib", PackageInfoFileName))
	}

	internalInfo, publicInfo := convert()

	if !strings.Contains(internalInfo, `GoInternalRoot("example.com/marked")]`) || strings.Contains(internalInfo, "Standard = true") {
		t.Errorf("the module's internal package is stamped with its module root and no Standard flag:\n%s", internalInfo)
	}

	if !strings.Contains(strings.ReplaceAll(internalInfo, "\r\n", "\n"), internalPackageBrowsableLine+"\n[GoPackage(\"x\"") {
		t.Errorf("the EditorBrowsable line sits directly above [GoPackage(\"x\")]:\n%s", internalInfo)
	}

	if strings.Contains(publicInfo, "GoInternalRoot") || strings.Contains(publicInfo, "EditorBrowsable") {
		t.Errorf("the public package is not marked:\n%s", publicInfo)
	}

	againInternal, againPublic := convert()

	if againInternal != internalInfo || againPublic != publicInfo {
		t.Errorf("a second conversion over the persisted files writes different bytes")
	}
}
