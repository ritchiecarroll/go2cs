// testTargetArch_test.go - Gbtc
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
	"path/filepath"
	"reflect"
	"runtime"
	"strings"
	"testing"
)

// The shipped set is the RIDs push-nuget.ps1 packs: darwin ships one build under two architectures, and
// windows and linux under amd64 alone.
func TestShippedGOARCHs(t *testing.T) {
	for goos, want := range map[string][]string{
		"darwin": {"amd64", "arm64"}, "linux": {"amd64"}, "windows": {"amd64"}, "freebsd": nil,
	} {
		if got := shippedGOARCHs(goos); !reflect.DeepEqual(got, want) {
			t.Errorf("shippedGOARCHs(%q) = %v, want %v", goos, got, want)
		}
	}
}

// The shapes a test file's GOARCH condition takes: a build line naming amd64 alone (crypto/sha1's
// issue15617_test.go), one admitting arm64 as well, a file-name suffix, and a file the conversion never
// selects. On darwin only the first and third leave, and only their test functions; on linux and windows,
// which ship amd64 alone, nothing does.
func TestTargetArchExcludedTestsWithdrawsFilesAShippedArchDoesNotBuild(t *testing.T) {
	dir := t.TempDir()

	write := func(name, content string) {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	write("a_test.go", "//go:build amd64 && (linux || darwin)\n\npackage p_test\n\nimport \"testing\"\n\n"+
		"func TestA(t *testing.T) {}\nfunc BenchmarkA(b *testing.B) {}\nfunc helper() {}\nfunc ExampleA() {}\n")
	write("b_test.go", "package p_test\n\nimport \"testing\"\n\nfunc TestB(t *testing.T) {}\n")
	write("c_test.go", "//go:build amd64 || arm64\n\npackage p_test\n\nimport \"testing\"\n\nfunc TestC(t *testing.T) {}\n")
	write("d_amd64_test.go", "package p_test\n\nimport \"testing\"\n\nfunc TestD(t *testing.T) {}\n")
	write("e_test.go", "//go:build arm64\n\npackage p_test\n\nimport \"testing\"\n\nfunc TestE(t *testing.T) {}\n")

	got := targetArchExcludedTests(dir, Options{targetPlatform: "darwin/amd64", buildTags: []string{"purego"}})

	if names, want := targetArchExcludedNames(got), []string{"TestA", "BenchmarkA", "ExampleA", "TestD"}; !reflect.DeepEqual(names, want) {
		t.Fatalf("withdrawn on darwin = %v, want %v", names, want)
	}

	if detail := got[0].Detail; !strings.Contains(detail, "a_test.go is not built for darwin/arm64") ||
		!strings.Contains(detail, "//go:build amd64 && (linux || darwin)") || !strings.Contains(detail, "one darwin build serves both RIDs") {
		t.Fatalf("detail for TestA does not name its file, the architecture, its build line and the reason: %q", detail)
	}

	if detail := got[3].Detail; !strings.Contains(detail, "d_amd64_test.go is not built for darwin/arm64;") {
		t.Fatalf("detail for TestD names a build line its file does not have: %q", detail)
	}

	for _, platform := range []string{"linux/amd64", "windows/amd64"} {
		if withdrawn := targetArchExcludedTests(dir, Options{targetPlatform: platform, buildTags: []string{"purego"}}); len(withdrawn) != 0 {
			t.Fatalf("%s withdrew %v; a GOOS that ships one architecture withdraws nothing", platform, targetArchExcludedNames(withdrawn))
		}
	}
}

// The measured case, on the real file: crypto/sha1 at the corpus's darwin target withdraws
// TestOutOfBoundsRead and nothing else, and its proof page names it, from any platform.
func TestTargetArchExcludedTestsOnCryptoSha1(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	dir := filepath.Join(goRoot, "src", "crypto", "sha1")

	if _, err := os.Stat(filepath.Join(dir, "issue15617_test.go")); err != nil {
		t.Skipf("no crypto/sha1/issue15617_test.go under GOROOT %s", goRoot)
	}

	options := Options{targetPlatform: "darwin/" + corpusArch, buildTags: []string{"purego"}}

	if names := targetArchExcludedNames(targetArchExcludedTests(dir, options)); !reflect.DeepEqual(names, []string{"TestOutOfBoundsRead"}) {
		t.Fatalf("crypto/sha1 withdrawn on darwin = %v, want [TestOutOfBoundsRead]", names)
	}

	withdrawals := proofPageArchWithdrawals(goRoot, "crypto/sha1", []string{"purego"})

	if len(withdrawals) != 1 || !strings.HasPrefix(withdrawals[0], "darwin: TestOutOfBoundsRead: issue15617_test.go is not built for darwin/arm64") {
		t.Fatalf("crypto/sha1's proof-page withdrawals = %q", withdrawals)
	}
}

// A page lists the withdrawals by name, with their reason, under its own heading; a page with none is
// unchanged.
func TestProofPageListsArchWithdrawals(t *testing.T) {
	comparison := testComparison{
		Package: "crypto/sha1", Status: "validated", Matched: true,
		Go: map[string]string{"TestGolden": "pass"}, CSharp: map[string]string{"TestGolden": "pass"},
		Skipped: []string{}, Disclosed: []string{}, Excluded: []string{}, Errors: []string{},
	}
	provenance := proofPageProvenance{importPath: "crypto/sha1", goVersion: "1.26.10", platform: "windows/amd64", date: "2026-10-26"}

	plain := renderValidationProofPage(provenance, comparison, map[string]testDisclosure{}, nil)

	provenance.archWithdrawals = []string{"darwin: TestOutOfBoundsRead: issue15617_test.go is not built for darwin/arm64; one darwin build serves both RIDs"}
	page := renderValidationProofPage(provenance, comparison, map[string]testDisclosure{}, nil)

	if strings.Contains(plain, "shipped architecture") {
		t.Fatal("a page with no withdrawals renders the withdrawal section")
	}

	if !strings.Contains(page, "## Withdrawn where a shipped architecture does not build the test") ||
		!strings.Contains(page, "- darwin: `TestOutOfBoundsRead` — issue15617_test.go is not built for darwin/arm64; one darwin build serves both RIDs") {
		t.Fatalf("the page does not list the withdrawal by name with its reason:\n%s", page)
	}
}

// ONE expression for both kinds of withdrawal: the host-fatal names and the target-arch names, deduplicated,
// sorted and anchored, so a host-fatal test that is also arch-excluded is named once.
func TestWithdrawnSkipExpressionJoinsHostFatalAndTargetArchNames(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestFatal": {Name: "TestFatal", Class: hostFatalClass},
		"TestOther": {Name: "TestOther", Class: "host-identity"},
	}

	if got, want := withdrawnSkipExpression(disclosures, []string{"TestOutOfBoundsRead", "TestFatal"}), "^(?:TestFatal|TestOutOfBoundsRead)$"; got != want {
		t.Fatalf("withdrawnSkipExpression = %q, want %q", got, want)
	}

	if got, want := hostFatalSkipExpression(disclosures), "^(?:TestFatal)$"; got != want {
		t.Fatalf("hostFatalSkipExpression = %q, want %q", got, want)
	}

	if got := withdrawnSkipExpression(map[string]testDisclosure{}, nil); got != "" {
		t.Fatalf("withdrawnSkipExpression with nothing to withdraw = %q, want empty", got)
	}
}
