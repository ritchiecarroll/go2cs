// embeddedAssets_test.go - Gbtc
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

// These are the guards for FALSE-GREEN route #5 (CLAUDE.md): a converter build input that is not a
// top-level *.go file used to invalidate go2cs.exe in no harness at all. Every rebuild predicate --
// BehavioralRunner, the MSTest BehavioralTestBase, PerformanceRunner -- asked whether any top-level
// *.go was newer than the binary, so editing an EMBEDDED asset (the csproj templates, the
// package_info.cs skeleton, the icons, profiles/*, stdlib-metadata.txt) or a converter internal/
// package changed what the converter emits, invalidated the binary nowhere, and left every gate
// validating the PREVIOUS emission green. A .NET migration's TFM stage edits exactly those
// templates, which is the step most likely to meet it.
//
// The remedy is src/tests/ConverterBuildInputs.cs, linked into all three harnesses, which DERIVES
// the embedded set from the //go:embed directives rather than listing filenames -- so a directive
// added tomorrow is covered without anyone remembering to widen a list. That derivation is what the
// two tests below hold up: the first pins the directive FORMS the C# resolver understands, the
// second pins that the three predicates still call it.

// harnessPredicateSources are the three rebuild predicates, relative to this package's directory,
// paired with the shared helper they must all delegate to.
var harnessPredicateSources = []string{
	filepath.Join("..", "tests", "Behavioral", "BehavioralRunner", "Program.cs"),
	filepath.Join("..", "tests", "Behavioral", "BehavioralTests", "BehavioralTestBase.cs"),
	filepath.Join("..", "tests", "Performance", "PerformanceRunner", "Program.cs"),
}

const converterBuildInputsSource = "../tests/ConverterBuildInputs.cs"

// embedDirective is one //go:embed line: the file that declared it, the 1-based line number, and the
// whitespace-separated patterns it names.
type embedDirective struct {
	file     string
	line     int
	patterns []string
}

// TestEmbedDirectivesStayWithinTheHarnessResolvableSubset is the SYNC guard between the Go side's
// directives and the C# side's resolver. ConverterBuildInputs.cs implements a deliberately small
// subset of Go's embed pattern language -- slash-separated, an optional `all:` prefix, wildcards in
// the final segment only, no quoting, no `..` -- because a full path.Match port in three linked
// harnesses is machinery nobody needs while every directive in the tree is a plain filename or a
// one-level glob.
//
// The cost of that choice is that a pattern OUTSIDE the subset resolves to nothing rather than
// failing: the harness would go on reporting "up to date" for an asset it can no longer see, which
// is route #5 exactly as it was. This test is what converts that silence into a red gate, so the
// subset is a documented contract and not an accident.
func TestEmbedDirectivesStayWithinTheHarnessResolvableSubset(t *testing.T) {
	directives := collectEmbedDirectives(t, ".")

	if len(directives) == 0 {
		t.Fatal("no //go:embed directives found in the converter sources; this guard would prove nothing")
	}

	for _, directive := range directives {
		for _, pattern := range directive.patterns {
			reject := func(why string) {
				t.Errorf("%s:%d\t//go:embed %s\n\t%s.\n\tsrc/tests/ConverterBuildInputs.cs — linked into BehavioralRunner, BehavioralTestBase and PerformanceRunner — cannot resolve this form, so the asset it names would invalidate go2cs.exe in NO harness: false-green route #5, reopened. Either restate the pattern within the subset, or widen the C# resolver AND this guard together.",
					directive.file, directive.line, pattern, why)
			}

			bare := pattern

			if prefix, rest, found := strings.Cut(pattern, ":"); found {
				if prefix != "all" {
					reject("`" + prefix + ":` is not a prefix the resolver strips; `all:` is the only one")
				}

				bare = rest
			}

			switch {
			case bare == "":
				reject("the pattern names nothing")
				continue
			case strings.ContainsAny(pattern, "\"`'"):
				reject("the pattern is quoted; the resolver splits a directive on whitespace and does not unquote")
			case strings.HasPrefix(bare, "/") || filepath.IsAbs(bare):
				reject("the pattern is absolute; embed patterns resolve against the declaring file's own directory")
			case strings.Contains(bare, `\`):
				reject("the pattern uses a backslash; embed patterns are slash-separated on every host")
			}

			segments := strings.Split(bare, "/")

			for index, segment := range segments {
				if segment == ".." || segment == "." {
					reject("the pattern walks outside the declaring file's directory")
				}

				if index < len(segments)-1 && strings.ContainsAny(segment, "*?[") {
					reject("the pattern wildcards a non-final segment; the resolver globs the FINAL segment only")
				}
			}

			// Every directive must name something that exists. A pattern matching nothing breaks the
			// Go build too, but it is ALSO indistinguishable from "the C# resolver understood the
			// pattern and found no files" — which is the silence this whole guard exists to break.
			matches, err := filepath.Glob(filepath.Join(filepath.Dir(directive.file), filepath.FromSlash(bare)))

			if err != nil {
				reject("the pattern is not a valid glob: " + err.Error())
			} else if len(matches) == 0 {
				reject("the pattern matches nothing on disk")
			}
		}
	}
}

// TestHarnessRebuildPredicatesUseTheSharedConverterBuildInputs pins the remedy in place. The three
// predicates are in three separate C# projects with no shared assembly between them, so nothing but
// this assertion stops one of them being rewritten back to a local top-level `*.go` enumeration --
// which is how route #5 existed in all three at once in the first place.
//
// It reads the sources rather than running them because the converter's `go test ./...` is the one
// gate that runs on every lane; a C#-side check would run only when somebody built the harness,
// which is precisely the day the predicate is least likely to be exercised.
//
// ⚠ This guard is BEST-EFFORT in one specific way, and the way is worth knowing rather than
// discovering. cmd/go's test cache records the files a test opens, but
// computeTestInputsID (cmd/go/internal/test/test.go) drops every one that resolves outside the
// module root — "Do not recheck files outside the module, GOPATH, or GOROOT root". These three C#
// sources live under src/tests, outside src/go2cs, so editing one does NOT invalidate a cached
// PASS: measured 2026-08-22, a narrowed BehavioralRunner predicate reported `ok (cached)` and only
// failed under -count=1. The unqualified sibling above has no such gap — every input it consults
// (the .go files and the assets they embed) is inside the module and fully tracked. So: a change
// that touches ONLY harness C# owes `go test -count=1 ./...`, and the structural protection is that
// the logic lives in ONE linked file rather than in three copies — this assertion is the tripwire,
// not the wall.
func TestHarnessRebuildPredicatesUseTheSharedConverterBuildInputs(t *testing.T) {
	shared, err := os.ReadFile(filepath.FromSlash(converterBuildInputsSource))

	if os.IsNotExist(err) {
		t.Skipf("%s is not present; the C# harnesses are not part of this checkout", converterBuildInputsSource)
	}

	if err != nil {
		t.Fatalf("reading %s: %v", converterBuildInputsSource, err)
	}

	// The derivation itself, not a hardcoded filename list: the anchor the directive scan matches on
	// has to still be there, or the set stopped being derived from the directives.
	for _, required := range []string{"EmbedDirective", `"//go:embed"`} {
		if !strings.Contains(string(shared), required) {
			t.Errorf("%s no longer contains %s — the embedded-asset set must be DERIVED from the //go:embed directives, never listed, or a new directive is uncovered the day it is written",
				converterBuildInputsSource, required)
		}
	}

	for _, source := range harnessPredicateSources {
		contents, err := os.ReadFile(source)

		if os.IsNotExist(err) {
			t.Errorf("%s is missing; one of the three converter-rebuild predicates has moved and this guard no longer covers it", source)
			continue
		}

		if err != nil {
			t.Fatalf("reading %s: %v", source, err)
		}

		text := string(contents)

		if !strings.Contains(text, "ConverterBuildInputs.IsConverterStale(") {
			t.Errorf("%s does not call ConverterBuildInputs.IsConverterStale; its converter-rebuild predicate has stopped using the shared build-input set (false-green route #5)", source)
		}

		// The exact narrow forms this arc replaced. Both runners legitimately enumerate `*.go` in
		// BEHAVIORAL package directories, so the ban is scoped to the CONVERTER source directory by
		// the variable that names it.
		for _, narrow := range []string{`GetFiles(s_converterSrc, "*.go")`, `GetFiles(go2csSrc, "*.go")`} {
			if strings.Contains(text, narrow) {
				t.Errorf("%s enumerates the converter sources as `%s` — a top-level *.go walk cannot see an embedded template or an internal/ package, which is false-green route #5", source, narrow)
			}
		}
	}
}

// TestConverterStalenessConsultsTheToolchain pins FALSE-GREEN route #4's remedy (H1.4) beside
// route #5's above, in the same one-linked-file structure and for the same reason: the three
// predicates share ConverterBuildInputs.cs, so the toolchain compare lives exactly once, and this
// assertion is the tripwire that stops it being quietly edited back out. A toolchain hop touches
// no build input, so the mtime walk alone answers "up to date" while the binary still embeds the
// OLD release's go/parser + go/types front end -- the stamp is inherent (`go version <exe>` reads
// the buildinfo every Go binary carries), and staleness must consult it before any mtime answer is
// allowed to say current.
//
// CORRECTED 2026-09-08 (COORD e0ef8639e): the comparison target is the CONVERTER MODULE's own `go`
// directive, not the live ambient toolchain. A toolchain hop is what MOVES that directive (H1 step
// 2), so the rebuild still fires exactly when route #4 needs it -- while in the two-pin window,
// where the converter is built by one release and the corpus pinned to another, the old ambient
// compare was permanently unequal and rebuilt on every invocation. The two arms below pin both
// halves: stale iff embedded differs from the directive, and NOT stale whatever the ambient is.
//
// Same best-effort caveat as the sibling guard: this C# source lives outside the module root, so
// cmd/go's test cache does not track it -- a change touching ONLY the C# owes `go test -count=1`.
func TestConverterStalenessConsultsTheToolchain(t *testing.T) {
	shared, err := os.ReadFile(filepath.FromSlash(converterBuildInputsSource))

	if os.IsNotExist(err) {
		t.Skipf("%s is not present; the C# harnesses are not part of this checkout", converterBuildInputsSource)
	}

	if err != nil {
		t.Fatalf("reading %s: %v", converterBuildInputsSource, err)
	}

	text := string(shared)

	// ARM 1 -- embedded != directive => STALE, WHATEVER THE AMBIENT. The two sides the predicate
	// must read: the binary's embedded stamp (`go version <exe>`) and the CONVERTER MODULE's own
	// `go` directive. Renaming or re-plumbing is fine; answering the question some other way that
	// skips the toolchain is what this fails on.
	for _, required := range []string{
		"EmbeddedGoRelease(converterExePath)",
		"ConverterModuleGoRelease(converterSrcDir)",
		"go version",
	} {
		if !strings.Contains(text, required) {
			t.Errorf("%s no longer contains %q -- IsConverterStale must compare the binary's embedded Go release "+
				"against the CONVERTER MODULE's own `go` directive (false-green route #4 / H1.4, two-pin "+
				"corrected 2026-09-08), or a toolchain hop leaves every harness validating the previous front "+
				"end's emission as current", converterBuildInputsSource, required)
		}
	}

	// ARM 2 -- embedded == directive => NOT STALE, WHATEVER THE AMBIENT. The predicate must not
	// consult the live/ambient toolchain at all. It did until 2026-09-08, and in the two-pin window
	// (converter built by one release, corpus pinned to another) that made `embedded != live`
	// PERMANENTLY true: every harness reported stale on every invocation, rebuilt each run, and
	// under GOTOOLCHAIN=local the rebuild could not succeed -- which blocked minting a golden at all.
	// The ambient axis belongs to the EMISSION guard in UpdateTestTargets, never to staleness.
	for _, forbidden := range []string{
		`"env GOVERSION"`,
		"LiveGoRelease",
	} {
		if strings.Contains(text, forbidden) {
			t.Errorf("%s contains %q -- staleness must NOT consult the ambient toolchain: the converter is "+
				"built by the release its own go.mod names, which in the two-pin window differs from the "+
				"corpus pin by construction, so an ambient compare reports STALE forever and forces a rebuild "+
				"that cannot succeed", converterBuildInputsSource, forbidden)
		}
	}

	// Failure must lean STALE-wards: a null from either side forces the rebuild rather than
	// trusting an unverified binary. The null-propagating comparison below is that lean.
	if !strings.Contains(text, "embedded is null || required is null") {
		t.Errorf("%s has lost the stale-wards null handling -- an unreadable stamp or an unreadable `go` "+
			"directive must force a rebuild, never a silent pass", converterBuildInputsSource)
	}
}

// collectEmbedDirectives walks root for .go files and returns every //go:embed directive in them,
// applying the same directory exclusions the C# side applies (Go's own `testdata`/dot/underscore
// rule, plus the harness build output) so the two walk the same tree.
func collectEmbedDirectives(t *testing.T, root string) []embedDirective {
	t.Helper()

	var directives []embedDirective

	err := filepath.Walk(root, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		name := info.Name()

		if info.IsDir() {
			if path == root {
				return nil
			}

			if strings.HasPrefix(name, ".") || strings.HasPrefix(name, "_") ||
				strings.EqualFold(name, "testdata") || strings.EqualFold(name, "bin") || strings.EqualFold(name, "obj") {
				return filepath.SkipDir
			}

			return nil
		}

		if !strings.HasSuffix(name, ".go") {
			return nil
		}

		contents, err := os.ReadFile(path)

		if err != nil {
			return err
		}

		for index, line := range strings.Split(strings.ReplaceAll(string(contents), "\r\n", "\n"), "\n") {
			text := strings.TrimLeft(line, " \t")

			if !strings.HasPrefix(text, "//go:embed") {
				continue
			}

			remainder := text[len("//go:embed"):]

			if remainder != "" && !strings.ContainsAny(remainder[:1], " \t") {
				continue
			}

			directives = append(directives, embedDirective{
				file:     path,
				line:     index + 1,
				patterns: strings.Fields(remainder),
			})
		}

		return nil
	})

	if err != nil {
		t.Fatalf("walking %s for //go:embed directives: %v", root, err)
	}

	return directives
}
