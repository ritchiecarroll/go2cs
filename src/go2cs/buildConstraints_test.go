// buildConstraints_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// buildConstraints_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"fmt"
	"go/build"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"testing"
)

// The defect these tests lock in: the evaluator parsed build constraints with go/parser's EXPRESSION
// parser, for which a Go release tag is not an expression at all — `go1.21` reads as the identifier
// `go1` followed by an illegal `.21` selector, so every constraint mentioning one failed to parse
// (`1:4: expected 'EOF', found .21`). Each failure warned and then fell THROUGH to including the
// file, which looked harmless on the five go-logr files that surfaced it during a -recurse probe of
// go.opentelemetry.io/otel, because including them was the right answer anyway.
//
// It is not harmless in general. This evaluator re-checks the files go/packages already selected
// against a possibly-different TARGET platform, so a constraint that mixes a release tag with a
// platform — `go1.21 && windows` converted for linux — lost its platform half along with the rest of
// the expression and included a file that does not belong in the build. The paired `!go1.21` file
// never showed the symptom only because go/packages had already dropped it upstream: it is absent
// from pkg.GoFiles, so it never reaches this code. Nothing about the two forms was handled
// differently here — both failed to parse identically.
//
// Constraints are now parsed and evaluated by go/build/constraint, the same package the toolchain
// uses, which removes the hand-rolled parse/eval layer rather than special-casing the dot.

// newLinuxAmd64Evaluator builds an evaluator targeting linux/amd64, the platform the tables below
// state their expectations against.
func newLinuxAmd64Evaluator() *BuildConstraintEvaluator {
	return NewBuildConstraintEvaluator([]string{"linux/amd64"})
}

// TestEvaluateConstraintReleaseTags is the direct regression: every one of these expressions used to
// fail to PARSE. The expectations are pinned to release tags that any toolchain able to build go2cs
// already satisfies (go1.21) and to a tag no toolchain satisfies yet (go1.99), so the table does not
// move with the host Go version.
func TestEvaluateConstraintReleaseTags(t *testing.T) {
	tests := []struct {
		constraint string
		want       bool
		why        string
	}{
		{"go1.21", true, "the bare release tag that could not be parsed at all"},
		{"!go1.21", false, "its negation — the form go/packages drops upstream, so it is only ever seen here under a different target platform"},
		{"go1.99", false, "a release the toolchain has not reached"},
		{"!go1.99", true, "and its negation, which is how a package keeps a pre-go1.99 fallback file"},
		{"go1.21 && amd64", true, "compound with a satisfied platform half"},
		{"go1.21 && arm64", false, "the platform half still decides when the release tag holds"},
		{"go1.21 && windows", false, "THE case the parse failure broke: a windows-only file was included in a linux conversion"},
		{"linux || go1.24", true, "left half satisfied regardless of how far the toolchain has moved"},
		{"go1.99 || amd64", true, "an unsatisfied release tag does not poison the disjunction"},
		{"(go1.21 || go1.99) && linux", true, "parentheses"},
		{"!(go1.21 && windows)", true, "negated compound"},
	}

	evaluator := newLinuxAmd64Evaluator()

	for _, tt := range tests {
		t.Run(tt.constraint, func(t *testing.T) {
			got, err := evaluator.EvaluateConstraint(tt.constraint)

			if err != nil {
				t.Fatalf("EvaluateConstraint(%q) returned error %v — the constraint must parse (%s)", tt.constraint, err, tt.why)
			}

			if got != tt.want {
				t.Errorf("EvaluateConstraint(%q) = %v, want %v (%s)", tt.constraint, got, tt.want, tt.why)
			}
		})
	}
}

// TestEvaluateConstraintLegacyPlusBuild covers the pre-Go-1.17 syntax through the same seam. A
// -recurse run meets these in third-party modules that never migrated, and the `,` (AND) / space
// (OR) / `!` grammar is nothing like the //go:build one.
func TestEvaluateConstraintLegacyPlusBuild(t *testing.T) {
	tests := []struct {
		constraint string
		want       bool
		why        string
	}{
		{"// +build go1.21", true, "legacy form of the release tag"},
		{"// +build !go1.21", false, "legacy negation"},
		{"// +build linux,go1.21", true, "comma is AND, both satisfied"},
		{"// +build windows,go1.21", false, "comma is AND, platform half fails"},
		{"// +build windows go1.21", true, "space is OR"},
		{"// +build !go1.99", true, "legacy negation of an unreached release"},
		{"//go:build go1.21", true, "a whole modern line is accepted too, not just the bare expression"},
	}

	evaluator := newLinuxAmd64Evaluator()

	for _, tt := range tests {
		t.Run(tt.constraint, func(t *testing.T) {
			got, err := evaluator.EvaluateConstraint(tt.constraint)

			if err != nil {
				t.Fatalf("EvaluateConstraint(%q) returned error %v (%s)", tt.constraint, err, tt.why)
			}

			if got != tt.want {
				t.Errorf("EvaluateConstraint(%q) = %v, want %v (%s)", tt.constraint, got, tt.want, tt.why)
			}
		})
	}
}

// TestReleaseTagsDefaultToTheBuildToolchain pins the MECHANISM the tables above cannot: with no
// loader directory to resolve against, release tags come from build.Default.ReleaseTags — the same
// list go/build matches against. Seeding a fixed `go1.0`..`go1.N` map instead, which is what this did
// before and capped one release BELOW the toolchain it was built against, makes the evaluator
// disagree with the loader the moment either side moves.
func TestReleaseTagsDefaultToTheBuildToolchain(t *testing.T) {
	latest := 0

	for _, releaseTag := range build.Default.ReleaseTags {
		minor, err := strconv.Atoi(strings.TrimPrefix(releaseTag, "go1."))

		if err == nil && minor > latest {
			latest = minor
		}
	}

	if latest == 0 {
		t.Fatalf("no go1.N release tags found in build.Default.ReleaseTags %v", build.Default.ReleaseTags)
	}

	evaluator := newLinuxAmd64Evaluator()

	current := fmt.Sprintf("go1.%d", latest)
	next := fmt.Sprintf("go1.%d", latest+1)

	if got, err := evaluator.EvaluateConstraint(current); err != nil || !got {
		t.Errorf("EvaluateConstraint(%q) = %v, %v; the toolchain's own newest release tag must be satisfied", current, got, err)
	}

	if got, err := evaluator.EvaluateConstraint(next); err != nil || got {
		t.Errorf("EvaluateConstraint(%q) = %v, %v; a release the toolchain has not reached must not be satisfied", next, got, err)
	}
}

// TestEvaluateConstraintPlatformTagsUnchanged is a control: the ordinary GOOS/GOARCH/`unix`/-tags
// cases that always worked must still work, so swapping the parse/eval layer is provably confined to
// the constraints that could not be parsed before.
func TestEvaluateConstraintPlatformTagsUnchanged(t *testing.T) {
	evaluator := newLinuxAmd64Evaluator()
	evaluator.SetTag("purego", true)

	tests := []struct {
		constraint string
		want       bool
	}{
		{"linux", true},
		{"windows", false},
		{"amd64", true},
		{"arm64", false},
		{"unix", true},
		{"gc", true},
		{"gccgo", false},
		{"linux && amd64", true},
		{"!windows && amd64", true},
		{"(!amd64 && !arm64) || purego", true},
		{"purego", true},
		{"someUnknownTag", false},
	}

	for _, tt := range tests {
		t.Run(tt.constraint, func(t *testing.T) {
			got, err := evaluator.EvaluateConstraint(tt.constraint)

			if err != nil {
				t.Fatalf("EvaluateConstraint(%q) returned error %v", tt.constraint, err)
			}

			if got != tt.want {
				t.Errorf("EvaluateConstraint(%q) = %v, want %v", tt.constraint, got, tt.want)
			}
		})
	}
}

// writeGoFile drops a source file into dir and returns its path.
func writeGoFile(t *testing.T, dir string, name string, content string) string {
	t.Helper()

	path := filepath.Join(dir, name)

	if err := os.WriteFile(path, []byte(content), 0644); err != nil {
		t.Fatalf("failed to write %s: %v", path, err)
	}

	return path
}

// TestCheckBuildConstraintsReleaseTagFiles reproduces the go-logr file shape that surfaced the defect
// — the dual-line header (`//go:build` plus the legacy `// +build`) that gofmt maintains — and asserts
// the whole file-level verdict, error included. Before the fix the go1.21 file produced an error, a
// warning, and an accidental inclusion via the caller's fallback.
func TestCheckBuildConstraintsReleaseTagFiles(t *testing.T) {
	dir := t.TempDir()

	slog := writeGoFile(t, dir, "context_slog.go", `//go:build go1.21
// +build go1.21

package logr

const HasSlog = true
`)

	noslog := writeGoFile(t, dir, "context_noslog.go", `//go:build !go1.21
// +build !go1.21

package logr

const HasSlog = false
`)

	// A release tag ANDed with a platform: go/packages selects this on windows, and converting for
	// linux must drop it. The parse failure used to take the platform half down with it.
	windowsOnly := writeGoFile(t, dir, "gated.go", `//go:build go1.21 && windows
// +build go1.21,windows

package logr

const Windows = true
`)

	tests := []struct {
		name string
		path string
		want bool
	}{
		{"go1.21 file is included", slog, true},
		{"!go1.21 file is excluded", noslog, false},
		{"go1.21 && windows is excluded on a linux target", windowsOnly, false},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got, err := CheckBuildConstraints(tt.path, "linux/amd64", nil, "")

			if err != nil {
				t.Fatalf("CheckBuildConstraints(%s) returned error %v — a release tag must not fail to parse", filepath.Base(tt.path), err)
			}

			if got != tt.want {
				t.Errorf("CheckBuildConstraints(%s) = %v, want %v", filepath.Base(tt.path), got, tt.want)
			}
		})
	}
}

// TestReadFileBuildConstraintPrecedence covers how a file's constraint is EXTRACTED, which is the
// half go/build/constraint does not do for us: which lines count, and which syntax wins when both
// are present.
func TestReadFileBuildConstraintPrecedence(t *testing.T) {
	dir := t.TempDir()

	tests := []struct {
		name    string
		file    string
		content string
		want    bool
		why     string
	}{
		{
			name: "go:build wins over a contradicting +build",
			file: "precedence.go",
			content: `//go:build linux
// +build windows

package p
`,
			want: true,
			why:  "the toolchain ignores the legacy lines entirely once a //go:build line is present",
		},
		{
			name: "legacy-only file is honored",
			file: "legacyonly.go",
			content: `// +build windows

package p
`,
			want: false,
			why:  "the regex scanner this replaced could not see +build lines at all, so the file converted as unconstrained",
		},
		{
			name: "multiple legacy lines are ANDed",
			file: "legacyand.go",
			content: `// +build linux
// +build arm64

package p
`,
			want: false,
			why:  "linux holds but arm64 does not, and separate +build lines are a conjunction",
		},
		{
			name: "multiple satisfied legacy lines",
			file: "legacyandok.go",
			content: `// +build linux
// +build amd64

package p
`,
			want: true,
			why:  "both conjuncts hold",
		},
		{
			name: "no constraint at all",
			file: "plain.go",
			content: `package p
`,
			want: true,
			why:  "an unconstrained file is always in the build",
		},
		{
			name: "constraint below the package clause is prose",
			file: "prose.go",
			content: `package p

// Example header:
//
//go:build windows

const X = 1
`,
			want: true,
			why:  "only the file header carries constraints; documentation quoting one must not gate the file",
		},
		{
			name: "constraint inside a block comment is prose",
			file: "blockcomment.go",
			content: `/*
Package p is documented here.

//go:build windows
*/

package p
`,
			want: true,
			why:  "a constraint must be a line comment at column zero, not text inside a block comment",
		},
		{
			name: "indented constraint is not a constraint",
			file: "indented.go",
			content: `	//go:build windows

package p
`,
			want: true,
			why:  "the toolchain requires column zero, and go/build/constraint's recognizers enforce it",
		},
		{
			name: "block-comment license header does not hide a real constraint",
			file: "licensed.go",
			content: `/* Copyright the go2cs Authors. */

//go:build windows

package p
`,
			want: false,
			why:  "the scan must resume after the block comment closes",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			path := writeGoFile(t, dir, tt.file, tt.content)
			got, err := CheckBuildConstraints(path, "linux/amd64", nil, "")

			if err != nil {
				t.Fatalf("CheckBuildConstraints(%s) returned error %v", tt.file, err)
			}

			if got != tt.want {
				t.Errorf("CheckBuildConstraints(%s) = %v, want %v (%s)", tt.file, got, tt.want, tt.why)
			}
		})
	}
}

// TestCheckBuildConstraintsHonorsBuildTags is a control for the -tags path through the new evaluator:
// a tag the loader used to select a file must still satisfy that file's constraint here, or the file
// the tag just selected is re-excluded and the package is left with no implementation at all.
func TestCheckBuildConstraintsHonorsBuildTags(t *testing.T) {
	dir := t.TempDir()

	path := writeGoFile(t, dir, "generic.go", `//go:build (!amd64 && !arm64) || purego

package sha256

const Generic = true
`)

	if got, err := CheckBuildConstraints(path, "linux/amd64", []string{"purego"}, ""); err != nil || !got {
		t.Errorf("CheckBuildConstraints with -tags purego = %v, %v; want true — the tag must satisfy the constraint", got, err)
	}

	if got, err := CheckBuildConstraints(path, "linux/amd64", nil, ""); err != nil || got {
		t.Errorf("CheckBuildConstraints without -tags = %v, %v; want false", got, err)
	}
}

// TestReleaseTagsForVersion covers the parse of `go env GOVERSION` into a release-tag list. The
// shapes are the ones the go command really emits — a plain release, a patch release, a release
// candidate — plus the ones it emits from a development tree, where there is no minor to read and the
// caller must be told to fall back rather than handed a truncated list.
func TestReleaseTagsForVersion(t *testing.T) {
	tests := []struct {
		version string
		latest  int
		why     string
	}{
		{"go1.25.0", 25, "a patch release"},
		{"go1.23", 23, "a bare minor"},
		{"go1.25rc1", 25, "a release candidate still satisfies its own release tag"},
		{"go1.9", 9, "single-digit minors are not special"},
		{"devel +abc1234", 0, "a development build has no release to read"},
		{"go2.0", 0, "not a go1.x version"},
		{"", 0, "no answer from the go command"},
		{"go1.", 0, "malformed"},
	}

	for _, tt := range tests {
		t.Run(tt.version, func(t *testing.T) {
			tags := releaseTagsForVersion(tt.version)

			if tt.latest == 0 {
				if tags != nil {
					t.Fatalf("releaseTagsForVersion(%q) = %v, want nil so the caller falls back (%s)", tt.version, tags, tt.why)
				}

				return
			}

			want := fmt.Sprintf("go1.%d", tt.latest)
			beyond := fmt.Sprintf("go1.%d", tt.latest+1)

			if len(tags) != tt.latest {
				t.Fatalf("releaseTagsForVersion(%q) produced %d tags, want %d (go1.1 through %s)", tt.version, len(tags), tt.latest, want)
			}

			if tags[0] != "go1.1" || tags[len(tags)-1] != want {
				t.Errorf("releaseTagsForVersion(%q) spans %s..%s, want go1.1..%s (%s)", tt.version, tags[0], tags[len(tags)-1], want, tt.why)
			}

			for _, tag := range tags {
				if tag == beyond {
					t.Errorf("releaseTagsForVersion(%q) included %s, which the toolchain has not reached", tt.version, beyond)
				}
			}
		})
	}
}

// TestReleaseTagsGovernMatching proves the resolved list is what actually decides a constraint, so a
// loader toolchain NEWER than the one that built this binary really does keep its files. This is the
// silent-drop the field exists to prevent: go2cs.exe built with Go 1.23 converting a module that
// GOTOOLCHAIN=auto loads with Go 1.25 must not exclude a `//go:build go1.24` file.
func TestReleaseTagsGovernMatching(t *testing.T) {
	evaluator := newLinuxAmd64Evaluator()
	evaluator.releaseTags = releaseTagsForVersion("go1.25.0")

	for _, tt := range []struct {
		constraint string
		want       bool
	}{
		{"go1.24", true},
		{"go1.25", true},
		{"go1.26", false},
		{"!go1.24", false},
		{"go1.24 && linux", true},
	} {
		got, err := evaluator.EvaluateConstraint(tt.constraint)

		if err != nil {
			t.Fatalf("EvaluateConstraint(%q) returned error %v", tt.constraint, err)
		}

		if got != tt.want {
			t.Errorf("EvaluateConstraint(%q) against a go1.25 loader = %v, want %v", tt.constraint, got, tt.want)
		}
	}
}

// TestModuleRootDir covers the cache key, which is the module rather than the directory because that
// is what GOTOOLCHAIN resolution keys on. It is also what keeps the cache useful for a corpus of many
// small packages: the 302 standard-library packages sit under one GOROOT/src go.mod and so share a
// single entry, and a package with no go.mod above it keys on itself rather than walking to the
// volume root and collapsing unrelated trees onto one answer.
func TestModuleRootDir(t *testing.T) {
	root := t.TempDir()

	if err := os.WriteFile(filepath.Join(root, "go.mod"), []byte("module probe\n\ngo 1.23\n"), 0644); err != nil {
		t.Fatalf("failed to write go.mod: %v", err)
	}

	nested := filepath.Join(root, "internal", "deep")

	if err := os.MkdirAll(nested, 0755); err != nil {
		t.Fatalf("failed to create %s: %v", nested, err)
	}

	if got := moduleRootDir(nested); got != filepath.Clean(root) {
		t.Errorf("moduleRootDir(%s) = %s, want the module root %s", nested, got, root)
	}

	if got := moduleRootDir(root); got != filepath.Clean(root) {
		t.Errorf("moduleRootDir(%s) = %s, want itself", root, got)
	}

	// A directory with no go.mod anywhere above it is its own key rather than walking to the volume
	// root and collapsing unrelated trees onto one cached answer.
	orphan := t.TempDir()

	if got := moduleRootDir(orphan); got != filepath.Clean(orphan) {
		t.Errorf("moduleRootDir(%s) = %s, want itself when no go.mod is found", orphan, got)
	}

	if got := moduleRootDir(""); got != "" {
		t.Errorf("moduleRootDir(\"\") = %q, want empty", got)
	}
}

// TestLoaderReleaseTagsNeverEmpty guards the fallback. An unreachable or unparseable toolchain must
// leave the evaluator on the tags compiled into this binary — never on an empty list, which would
// make every `go1.N` false and exclude files wholesale.
func TestLoaderReleaseTagsNeverEmpty(t *testing.T) {
	for _, dir := range []string{t.TempDir(), filepath.Join(t.TempDir(), "does", "not", "exist")} {
		tags := loaderReleaseTags(dir)

		if len(tags) == 0 {
			t.Fatalf("loaderReleaseTags(%s) returned no tags; a missing toolchain answer must fall back, not exclude everything", dir)
		}

		evaluator := newLinuxAmd64Evaluator()
		evaluator.releaseTags = tags

		if got, err := evaluator.EvaluateConstraint("go1.21"); err != nil || !got {
			t.Errorf("loaderReleaseTags(%s) does not satisfy go1.21 (= %v, %v)", dir, got, err)
		}
	}
}

// TestIsGoReleaseTag covers the shape test that keeps resolution lazy — it decides whether a tag is
// worth a `go env` subprocess at all.
func TestIsGoReleaseTag(t *testing.T) {
	for tag, want := range map[string]bool{
		"go1.21":                        true,
		"go1.9":                         true,
		"go1.100":                       true,
		"go1.":                          false,
		"go1":                           false,
		"go1.21rc1":                     false,
		"go1.21.3":                      false,
		"linux":                         false,
		"amd64.v1":                      false,
		"goexperiment.coverageredesign": false,
		"":                              false,
	} {
		if got := isGoReleaseTag(tag); got != want {
			t.Errorf("isGoReleaseTag(%q) = %v, want %v", tag, got, want)
		}
	}
}

// TestReleaseTagsResolveLazily is a cost guard, and the corpus makes it a real one: every one of the
// 569 behavioral packages is its own Go module, so resolving release tags eagerly would spend one
// `go env` subprocess per package — roughly 300ms each on Windows — to answer a question none of them
// asks. Not one behavioral constraint mentions a release tag. The evaluator must therefore leave
// releaseTags unresolved after evaluating a constraint that has none, and fill it only when one appears.
func TestReleaseTagsResolveLazily(t *testing.T) {
	evaluator := NewBuildConstraintEvaluator([]string{"linux/amd64"})
	evaluator.loaderDir = filepath.Join(t.TempDir(), "never-consulted")

	for _, constraint := range []string{"linux && amd64", "!windows", "goexperiment.somethingOff", "unix || js"} {
		if _, err := evaluator.EvaluateConstraint(constraint); err != nil {
			t.Fatalf("EvaluateConstraint(%q) returned error %v", constraint, err)
		}
	}

	if evaluator.releaseTags != nil {
		t.Errorf("release tags were resolved for constraints that mention none: %v", evaluator.releaseTags)
	}

	if _, err := evaluator.EvaluateConstraint("go1.21"); err != nil {
		t.Fatalf("EvaluateConstraint(\"go1.21\") returned error %v", err)
	}

	if evaluator.releaseTags == nil {
		t.Error("release tags were not resolved even though the constraint names one")
	}
}
