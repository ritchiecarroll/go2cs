// toolchainResolution_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/version"
	"os"
	"path/filepath"
	"runtime"
	"strconv"
	"strings"
	"testing"
)

// runningMinorOffsetVersion builds a Go version string a fixed number of minor releases away from
// the toolchain running the test, so these tests keep meaning after the converter is rebuilt on a
// newer Go. Returns "" when the running version is not of a release shape (a devel build), which
// the callers treat as a skip.
func runningMinorOffsetVersion(offset int) string {
	running := normalizeGoVersion(runtime.Version())

	if running == "" {
		return ""
	}

	minor := strings.TrimPrefix(version.Lang(running), "go1.")

	if minor == version.Lang(running) {
		return ""
	}

	value, err := strconv.Atoi(minor)

	if err != nil {
		return ""
	}

	return fmt.Sprintf("go1.%d", value+offset)
}

// writeModuleDir creates a directory holding a go.mod with the given body and returns its path.
func writeModuleDir(t *testing.T, body string) string {
	t.Helper()

	dir := t.TempDir()

	if err := os.WriteFile(filepath.Join(dir, "go.mod"), []byte(body), 0o644); err != nil {
		t.Fatalf("failed to write go.mod: %v", err)
	}

	return dir
}

func TestNormalizeGoVersionAcceptsBothSpellings(t *testing.T) {
	// go.mod's `go` directive omits the prefix, `go env GOVERSION` and `toolchain` carry it; both
	// have to normalize to the one form go/version compares.
	tests := []struct {
		input    string
		expected string
	}{
		{"go1.25.0", "go1.25.0"},
		{"1.25.0", "go1.25.0"},
		{"1.23", "go1.23"},
		{"  go1.24.1  ", "go1.24.1"},
		{"devel +abc1234", ""},
		{"", ""},
		{"not-a-version", ""},
	}

	for _, test := range tests {
		if actual := normalizeGoVersion(test.input); actual != test.expected {
			t.Errorf("normalizeGoVersion(%q) = %q, want %q", test.input, actual, test.expected)
		}
	}
}

func TestModuleToolchainRequestTakesTheHigherDirective(t *testing.T) {
	// The go command switches to satisfy whichever directive asks for more, so reading only one of
	// them would under-report the requirement and skip the GOROOT re-resolution that needs to happen.
	tests := []struct {
		name     string
		body     string
		expected string
	}{
		{"go directive alone", "module m\n\ngo 1.25.0\n", "go1.25.0"},
		{"toolchain raises it", "module m\n\ngo 1.21\n\ntoolchain go1.25.0\n", "go1.25.0"},
		{"go directive is the higher one", "module m\n\ngo 1.25.0\n\ntoolchain go1.22.0\n", "go1.25.0"},
		{"neither directive", "module m\n", ""},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if actual := moduleToolchainRequest(writeModuleDir(t, test.body)); actual != test.expected {
				t.Errorf("moduleToolchainRequest() = %q, want %q", actual, test.expected)
			}
		})
	}
}

func TestModuleToolchainRequestSurvivesAnUnreadableModule(t *testing.T) {
	// No go.mod at all, and a go.mod whose UNRELATED syntax is broken, must both answer "" rather
	// than panic or error out — converting standalone code is a supported mode.
	if actual := moduleToolchainRequest(t.TempDir()); actual != "" {
		t.Errorf("moduleToolchainRequest() with no go.mod = %q, want \"\"", actual)
	}

	if actual := moduleToolchainRequest(""); actual != "" {
		t.Errorf("moduleToolchainRequest(\"\") = %q, want \"\"", actual)
	}

	// A go.mod carrying a directive this x/mod has never heard of — what a NEWER toolchain writes,
	// i.e. the exact case this check serves — must still yield its toolchain line. Strict parsing
	// would reject the whole file here.
	future := writeModuleDir(t, "module m\n\ngo 1.21\n\ntoolchain go1.25.0\n\nsomefuturedirective foo bar\n")

	if actual := moduleToolchainRequest(future); actual != "go1.25.0" {
		t.Errorf("moduleToolchainRequest() with an unknown directive = %q, want \"go1.25.0\"", actual)
	}

	// A syntactically broken go.mod has nothing to read: "" sends the caller to the ambient GOROOT,
	// which is the pre-existing behavior and never worse than it.
	broken := writeModuleDir(t, "module m\n\ngo 1.25.0\n\nrequire ( this is not valid\n")

	if actual := moduleToolchainRequest(broken); actual != "" {
		t.Errorf("moduleToolchainRequest() on an unparseable go.mod = %q, want \"\"", actual)
	}
}

// TestModuleToolchainRequestReadsTheToolchainDirective guards the ParseLax trap directly: the
// semantic Toolchain field is always nil under lax parsing, so a `go`/`toolchain` split like the one
// `go mod tidy` writes must still report the higher version. Reading parsed.Toolchain instead of the
// syntax tree makes this test report the `go` line and disables the whole GOROOT re-resolution.
func TestModuleToolchainRequestReadsTheToolchainDirective(t *testing.T) {
	dir := writeModuleDir(t, "module m\n\ngo 1.21\n\ntoolchain go1.25.0\n")

	if actual := moduleToolchainRequest(dir); actual != "go1.25.0" {
		t.Fatalf("moduleToolchainRequest() = %q, want \"go1.25.0\"", actual)
	}

	// `toolchain default` is legal and names no version; the go directive stands.
	fallback := writeModuleDir(t, "module m\n\ngo 1.21\n\ntoolchain default\n")

	if actual := moduleToolchainRequest(fallback); actual != "go1.21" {
		t.Errorf("moduleToolchainRequest() with `toolchain default` = %q, want \"go1.21\"", actual)
	}
}

// TestToolchainSwitchImpossibleForAMatchedModule is the cost guard. Re-resolving GOROOT costs a
// `go env` subprocess (~300ms on Windows) and the converter runs once per package — 574 of them in a
// full check-no-regression pass. Nothing in the corpus or the behavioral suite asks for a newer
// toolchain, so this predicate must answer false for all of them and the subprocess must never run.
func TestToolchainSwitchImpossibleForAMatchedModule(t *testing.T) {
	t.Setenv("GOTOOLCHAIN", "auto")

	running := normalizeGoVersion(runtime.Version())

	if running == "" {
		t.Skip("running toolchain is not a release version")
	}

	older := runningMinorOffsetVersion(-1)

	if older == "" {
		t.Skip("running toolchain minor could not be derived")
	}

	for _, body := range []string{
		fmt.Sprintf("module m\n\ngo %s\n", strings.TrimPrefix(running, "go")),
		fmt.Sprintf("module m\n\ngo %s\n", strings.TrimPrefix(older, "go")),
		"module m\n",
	} {
		if toolchainSwitchPossible(writeModuleDir(t, body)) {
			t.Errorf("toolchainSwitchPossible() = true for a module the running toolchain satisfies:\n%s", body)
		}
	}
}

func TestToolchainSwitchPossibleForANewerModule(t *testing.T) {
	t.Setenv("GOTOOLCHAIN", "auto")

	newer := runningMinorOffsetVersion(1)

	if newer == "" {
		t.Skip("running toolchain minor could not be derived")
	}

	body := fmt.Sprintf("module m\n\ngo %s\n", strings.TrimPrefix(newer, "go"))

	if !toolchainSwitchPossible(writeModuleDir(t, body)) {
		t.Errorf("toolchainSwitchPossible() = false for a module requiring %s while running %s", newer, runtime.Version())
	}
}

func TestToolchainSwitchPossibleHonorsExplicitGoToolchain(t *testing.T) {
	// A GOTOOLCHAIN naming a specific toolchain switches regardless of what the module asks for;
	// `auto` and `local` cannot by themselves.
	matched := writeModuleDir(t, "module m\n")

	t.Setenv("GOTOOLCHAIN", "go1.24.0")

	if !toolchainSwitchPossible(matched) {
		t.Error("toolchainSwitchPossible() = false with GOTOOLCHAIN naming a specific toolchain")
	}

	for _, selector := range []string{"auto", "local", ""} {
		t.Setenv("GOTOOLCHAIN", selector)

		if toolchainSwitchPossible(matched) {
			t.Errorf("toolchainSwitchPossible() = true with GOTOOLCHAIN=%q and a module asking for nothing", selector)
		}
	}
}

// TestResolveLoaderGoRootKeepsAmbientWhenNoSwitchPossible pins the other half of the safety claim:
// on a matched toolchain the resolved GOROOT is the ambient one, byte for byte, and the caller is
// told nothing switched — which is what makes the corpus emission provably unmoved.
func TestResolveLoaderGoRootKeepsAmbientWhenNoSwitchPossible(t *testing.T) {
	t.Setenv("GOTOOLCHAIN", "auto")

	const ambient = "C:\\Program Files\\Go"

	matched := writeModuleDir(t, "module m\n")

	resolved, loaderVersion, switched := resolveLoaderGoRoot(matched, ambient)

	if switched || resolved != ambient || loaderVersion != "" {
		t.Errorf("resolveLoaderGoRoot() = (%q, %q, %v), want (%q, \"\", false)", resolved, loaderVersion, switched, ambient)
	}

	// No input path at all — a -stdlib run — must never re-resolve either.
	resolved, loaderVersion, switched = resolveLoaderGoRoot("", ambient)

	if switched || resolved != ambient || loaderVersion != "" {
		t.Errorf("resolveLoaderGoRoot(\"\") = (%q, %q, %v), want (%q, \"\", false)", resolved, loaderVersion, switched, ambient)
	}
}

// TestResolveLoaderGoRootAcceptsAFilePath covers the single-file conversion form (`go2cs main.go`),
// where the input names a file and the module root is its parent's.
func TestResolveLoaderGoRootAcceptsAFilePath(t *testing.T) {
	t.Setenv("GOTOOLCHAIN", "auto")

	const ambient = "/usr/lib/go"

	dir := writeModuleDir(t, "module m\n")
	file := filepath.Join(dir, "main.go")

	if err := os.WriteFile(file, []byte("package main\n"), 0o644); err != nil {
		t.Fatalf("failed to write main.go: %v", err)
	}

	if resolved, _, switched := resolveLoaderGoRoot(file, ambient); switched || resolved != ambient {
		t.Errorf("resolveLoaderGoRoot(<file>) = (%q, %v), want (%q, false)", resolved, switched, ambient)
	}
}

func TestCheckNuGetStdLibCompatibility(t *testing.T) {
	// A patch-level difference is the SAME standard library and the floating revision in
	// $(GoStdLibVersion) already spans it — refusing there would reject ordinary, working setups.
	// A minor-level difference is a different set of packages and is the reported failure.
	tests := []struct {
		name       string
		converting string
		published  string
		wantError  bool
	}{
		{"exact match", "1.23.1", "go1.23", false},
		{"patch drift is the same standard library", "1.23.7", "go1.23", false},
		{"prefixed spelling", "go1.23.1", "go1.23", false},
		{"newer minor is the reported failure", "1.25.0", "go1.23", true},
		{"older minor is equally unsatisfiable", "1.21.0", "go1.23", true},
		{"unreadable converting release does not refuse", "devel +abc", "go1.23", false},
		{"absent converting release does not refuse", "", "go1.23", false},
		{"absent published release does not refuse", "1.25.0", "", false},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			err := checkNuGetStdLibCompatibility(test.converting, test.published)

			if test.wantError && err == nil {
				t.Fatalf("checkNuGetStdLibCompatibility(%q, %q) = nil, want an error", test.converting, test.published)
			}

			if !test.wantError && err != nil {
				t.Fatalf("checkNuGetStdLibCompatibility(%q, %q) = %v, want nil", test.converting, test.published, err)
			}

			// The whole point is that the message names BOTH releases; a diagnosis the reader
			// cannot act on is the failure mode being fixed.
			if err != nil {
				for _, needle := range []string{test.published, version.Lang(normalizeGoVersion(test.converting)), "-recurse=nuget"} {
					if !strings.Contains(err.Error(), needle) {
						t.Errorf("error message does not mention %q: %v", needle, err)
					}
				}
			}
		})
	}
}

func TestPublishedStdLibReleaseIsTheBuildRelease(t *testing.T) {
	// The corpus version tracks the converter's own go.mod, so the build release is the honest
	// answer for "which go.<pkg> packages exist". A devel build yields "", which disables the check
	// rather than refusing every conversion.
	published := publishedStdLibRelease()

	if running := normalizeGoVersion(runtime.Version()); running != "" && published != version.Lang(running) {
		t.Errorf("publishedStdLibRelease() = %q, want %q", published, version.Lang(running))
	}
}

// TestPinGoVersionWinsOnlyBeforeFirstUse pins the ordering contract pinGoVersion depends on: it must
// take effect when called first, and must not retroactively rewrite a version already reported.
func TestPinGoVersionWinsOnlyBeforeFirstUse(t *testing.T) {
	// goVersion caches process-wide through a sync.Once, so this exercises the trimming and the
	// no-op guard rather than mutating the shared value out from under other tests.
	if actual := strings.TrimPrefix(strings.TrimSpace("  go1.25.0  "), "go"); actual != "1.25.0" {
		t.Fatalf("version trimming produced %q", actual)
	}

	before := goVersion()

	// Empty input is ignored outright, so the reported version cannot be blanked by a failed lookup.
	pinGoVersion("")
	pinGoVersion("   ")

	if after := goVersion(); after != before {
		t.Errorf("goVersion() changed from %q to %q after pinning an empty value", before, after)
	}
}

// writeVersionProps stands up a go2cs root carrying just the element corpusPinnedRelease reads, in
// the shape src/version.props actually uses.
func writeVersionProps(t *testing.T, body string) string {
	t.Helper()

	dir := t.TempDir()

	if err := os.WriteFile(filepath.Join(dir, versionPropsFileName), []byte(body), 0o644); err != nil {
		t.Fatalf("failed to write version.props: %v", err)
	}

	return dir
}

// TestCorpusPinnedRelease covers reading the pin off version.props, and the absences that must read
// as "no pin to check" rather than as a mismatch — an unseeded root is the normal state of a first
// -stdlib conversion, not a fault.
func TestCorpusPinnedRelease(t *testing.T) {
	const props = "<Project>\r\n  <PropertyGroup>\r\n    <GoStdLibVersion>1.23.1</GoStdLibVersion>\r\n" +
		"    <GoBuildNumber>6</GoBuildNumber>\r\n  </PropertyGroup>\r\n</Project>\r\n"

	if actual := corpusPinnedRelease(writeVersionProps(t, props)); actual != "1.23.1" {
		t.Errorf("corpusPinnedRelease() = %q, want %q", actual, "1.23.1")
	}

	// A root with no version.props at all — a bare temp -go2cspath target.
	if actual := corpusPinnedRelease(t.TempDir()); actual != "" {
		t.Errorf("corpusPinnedRelease(<no version.props>) = %q, want \"\"", actual)
	}

	// Present but carrying only the build number: half a version is not a pin.
	const partial = "<Project>\r\n  <PropertyGroup>\r\n    <GoBuildNumber>6</GoBuildNumber>\r\n  </PropertyGroup>\r\n</Project>\r\n"

	if actual := corpusPinnedRelease(writeVersionProps(t, partial)); actual != "" {
		t.Errorf("corpusPinnedRelease(<no GoStdLibVersion>) = %q, want \"\"", actual)
	}

	if actual := corpusPinnedRelease(""); actual != "" {
		t.Errorf("corpusPinnedRelease(\"\") = %q, want \"\"", actual)
	}
}

// TestCheckCorpusToolchainPin covers both guarded paths. The refuse path is driven entirely through
// the function's parameters — the test seam — because the alternative is installing a second Go
// toolchain, which no machine should have to do to prove that a guard refuses.
func TestCheckCorpusToolchainPin(t *testing.T) {
	tests := []struct {
		name       string
		mode       string
		converting string
		pinned     string
		wantError  bool
	}{
		{"exact match", "-stdlib", "1.23.1", "1.23.1", false},
		{"toolchain's prefixed spelling", "-stdlib", "go1.23.1", "1.23.1", false},
		{"pin written with the prefix", "-stdlib", "1.23.1", "go1.23.1", false},
		{"surrounding whitespace", "-stdlib", " go1.23.1 ", " 1.23.1 ", false},

		// The case this guard exists for — and precisely the one its NuGet sibling allows on purpose.
		{"patch drift refuses", "-stdlib", "1.23.2", "1.23.1", true},
		{"patch drift refuses under -tests too", "-tests", "1.23.2", "1.23.1", true},
		{"a newer minor refuses", "-stdlib", "1.24.0", "1.23.1", true},
		{"an older toolchain refuses equally", "-tests", "1.22.9", "1.23.1", true},

		// Nothing readable on one side is not a mismatch; inventing one would refuse legitimate runs.
		{"absent pin does not refuse", "-stdlib", "1.23.2", "", false},
		{"absent toolchain version does not refuse", "-stdlib", "", "1.23.1", false},
		{"devel toolchain does not refuse", "-stdlib", "devel +abc123", "1.23.1", false},
		{"unparseable pin does not refuse", "-tests", "1.23.2", "not-a-version", false},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			err := checkCorpusToolchainPin(test.mode, test.converting, test.pinned)

			if !test.wantError {
				if err != nil {
					t.Fatalf("checkCorpusToolchainPin(%q, %q, %q) = %v, want nil", test.mode, test.converting, test.pinned, err)
				}

				return
			}

			if err == nil {
				t.Fatalf("checkCorpusToolchainPin(%q, %q, %q) = nil, want an error", test.mode, test.converting, test.pinned)
			}

			// A refusal the reader cannot act on is the failure mode being fixed: the message has to
			// name the toolchain it found, the release the corpus is pinned to, and which mode stopped.
			converting := normalizeGoVersion(test.converting)
			pinned := strings.TrimPrefix(normalizeGoVersion(test.pinned), "go")

			for _, needle := range []string{converting, pinned, test.mode, "GoStdLibVersion", "GOROOT"} {
				if !strings.Contains(err.Error(), needle) {
					t.Errorf("error message does not mention %q: %v", needle, err)
				}
			}
		})
	}
}

// TestCorpusPinIsStricterThanTheNuGetGuard locks the one difference between the two toolchain guards
// in place, because it is subtle enough to look like an inconsistency worth "fixing".
//
// checkNuGetStdLibCompatibility compares version.Lang: 1.23.1 and 1.23.2 publish the same go.<pkg>
// package ids, so refusing there would reject working setups. checkCorpusToolchainPin compares the
// full release, because those same two toolchains carry DIFFERENT standard-library sources and
// converting one against goldens captured from the other is the drift it exists to stop. Collapsing
// this guard onto version.Lang would silently restore the hole.
func TestCorpusPinIsStricterThanTheNuGetGuard(t *testing.T) {
	const converting, pinned = "1.23.2", "1.23.1"

	if err := checkNuGetStdLibCompatibility(converting, version.Lang(normalizeGoVersion(pinned))); err != nil {
		t.Fatalf("the NuGet guard must still accept a patch difference, got: %v", err)
	}

	if err := checkCorpusToolchainPin("-stdlib", converting, pinned); err == nil {
		t.Fatal("checkCorpusToolchainPin must refuse a patch difference the NuGet guard accepts")
	}
}

// writeGoRoot stands up a directory carrying just the VERSION file gorootRelease reads.
func writeGoRoot(t *testing.T, body string) string {
	t.Helper()

	dir := t.TempDir()

	if err := os.WriteFile(filepath.Join(dir, gorootVersionFileName), []byte(body), 0o644); err != nil {
		t.Fatalf("failed to write VERSION: %v", err)
	}

	return dir
}

// TestGorootRelease covers reading a GOROOT's own release out of its VERSION file, including the
// two-line form Go 1.21 introduced, and the absence that leaves the caller on the reported version.
func TestGorootRelease(t *testing.T) {
	if actual := gorootRelease(writeGoRoot(t, "go1.23.1\n")); actual != "go1.23.1" {
		t.Errorf("gorootRelease() = %q, want %q", actual, "go1.23.1")
	}

	// Go 1.21 and later write a `time <stamp>` line beneath the release.
	if actual := gorootRelease(writeGoRoot(t, "go1.23.1\ntime 2024-09-05T18:14:44Z\n")); actual != "go1.23.1" {
		t.Errorf("gorootRelease(<two-line VERSION>) = %q, want %q", actual, "go1.23.1")
	}

	if actual := gorootRelease(t.TempDir()); actual != "" {
		t.Errorf("gorootRelease(<no VERSION>) = %q, want \"\"", actual)
	}

	if actual := gorootRelease(""); actual != "" {
		t.Errorf("gorootRelease(\"\") = %q, want \"\"", actual)
	}
}

// TestConvertingReleaseFollowsGorootNotTheLabel covers the measured mixed state that motivated
// reading GOROOT's VERSION at all: a GOROOT environment variable pointing at one installation while
// a different toolchain is selected. `go env GOVERSION` then reports the SELECTED toolchain while the
// converter reads the pinned tree's sources, so checking the reported version would wave through a
// conversion of the wrong release. The sources win.
func TestConvertingReleaseFollowsGorootNotTheLabel(t *testing.T) {
	goRoot := writeGoRoot(t, "go1.23.2\n")

	if actual := convertingRelease(goRoot); actual != "go1.23.2" {
		t.Fatalf("convertingRelease() = %q, want the GOROOT's own %q", actual, "go1.23.2")
	}

	if err := checkCorpusToolchainPin("-stdlib", convertingRelease(goRoot), "1.23.1"); err == nil {
		t.Fatal("a GOROOT holding 1.23.2 sources must be refused against a 1.23.1 pin, whatever the toolchain reports")
	}

	// A GOROOT carrying no VERSION file falls back to the reported version rather than disabling
	// the guard outright.
	if actual := convertingRelease(t.TempDir()); actual != goVersion() {
		t.Errorf("convertingRelease(<no VERSION>) = %q, want the reported %q", actual, goVersion())
	}
}

// TestModuleToolchainRequestReadsGo124Grammar guards the H4 re-check: a go.mod written by Go 1.24
// must still yield its toolchain request through this converter's readers.
//
// WHY THE UNKNOWN-DIRECTIVE TEST ABOVE DOES NOT COVER IT. That one plants a single fabricated line
// (`somefuturedirective foo bar`), which parses to a *modfile.Line. Go 1.24's `tool` directive has a
// BLOCK form, and a block parses to a *modfile.LineBlock — a different statement type, which
// toolchainDirective's walk skips rather than descends. So "unknown directives are tolerated" was
// proven for LINES and merely assumed for BLOCKS, and the toolchain line's visibility past a block
// was never exercised at all.
//
// MEASURED BEFORE THIS GUARD WAS WRITTEN, on the pinned x/mod (v0.27.0): every case below already
// passes and NO converter change is needed. The guard is the deliverable precisely because the
// re-check found nothing — H1.3 bumps x/mod as its own commit, and this is the property that bump
// could silently break. A negative result banked in code at the gate.
func TestModuleToolchainRequestReadsGo124Grammar(t *testing.T) {
	const (
		goLine    = "module m\n\ngo 1.24.0\n"
		toolOne   = "module m\n\ngo 1.24.0\n\ntool m/cmd/x\n"
		toolBlock = "module m\n\ngo 1.24.0\n\ntool (\n\tm/cmd/x\n\tm/cmd/y\n)\n"
		goDebug   = "module m\n\ngo 1.24.0\n\ngodebug default=go1.23\n"
		blockThen = "module m\n\ngo 1.21\n\ntool (\n\tm/cmd/x\n)\n\ntoolchain go1.24.13\n"
		thenBlock = "module m\n\ngo 1.21\n\ntoolchain go1.24.13\n\ntool (\n\tm/cmd/x\n)\n"
	)

	tests := []struct {
		name     string
		body     string
		expected string
	}{
		{"1.24 go line alone", goLine, "go1.24.0"},
		{"tool directive, single-line form", toolOne, "go1.24.0"},
		{"tool directive, BLOCK form - a LineBlock, not a Line", toolBlock, "go1.24.0"},
		{"godebug directive", goDebug, "go1.24.0"},
		{"tool BLOCK ahead of the toolchain line", blockThen, "go1.24.13"},
		{"toolchain ahead of a tool BLOCK", thenBlock, "go1.24.13"},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if actual := moduleToolchainRequest(writeModuleDir(t, test.body)); actual != test.expected {
				t.Errorf("moduleToolchainRequest() = %q, want %q", actual, test.expected)
			}
		})
	}
}
