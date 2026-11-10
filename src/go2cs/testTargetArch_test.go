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
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"reflect"
	"runtime"
	"strings"
	"testing"
)

// The shipped set is the RIDs push-nuget.ps1 packs, per corpus FLAVOUR: darwin ships its one amd64 flavour under two
// architectures, and windows and linux their amd64 flavours under amd64 alone. A target outside the universe reads its
// flavour's set, so linux/arm64 reads linux/amd64's until the universe carries linux/arm64, and then its own.
func TestShippedGOARCHs(t *testing.T) {
	cases := []struct {
		universe []string
		target   string
		want     []string
	}{
		{universeToday, "darwin/amd64", []string{"amd64", "arm64"}},
		{universeToday, "darwin/arm64", []string{"amd64", "arm64"}},
		{universeToday, "windows/amd64", []string{"amd64"}},
		{universeToday, "linux/amd64", []string{"amd64"}},
		{universeToday, "linux/arm64", []string{"amd64"}},
		{universeToday, "plan9/amd64", nil},
		{universeToday, "freebsd/arm64", nil},
		{universeArm, "darwin/amd64", []string{"amd64", "arm64"}},
		{universeArm, "darwin/arm64", []string{"amd64", "arm64"}},
		{universeArm, "windows/amd64", []string{"amd64"}},
		{universeArm, "linux/amd64", []string{"amd64"}},
		{universeArm, "linux/arm64", []string{"arm64"}},
		{universeArm, "plan9/amd64", nil},
	}

	for _, c := range cases {
		if got := shippedGOARCHs(mustPlatformLattice(c.universe), c.target); !reflect.DeepEqual(got, c.want) {
			t.Errorf("over %v, shippedGOARCHs(%q) = %v, want %v", c.universe, c.target, got, c.want)
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

// The arm leg is the opposite of darwin: once the universe carries linux/arm64, that flavour is converted at arm64 and
// published under one RID, so a test in an arm64-only file is its own run's and nothing is withdrawn. Read per GOOS
// instead, linux would ship amd64 alone and the same call would withdraw it, which is what a linux/arm64 run does
// today, served by the linux/amd64 flavour (S1b, 2026-11-09).
func TestTargetArchExcludedTestsKeepsArmOnlyTestsOnAnArmFlavour(t *testing.T) {
	dir := t.TempDir()

	for name, content := range map[string]string{
		"arm_test.go":    "//go:build arm64\n\npackage p_test\n\nimport \"testing\"\n\nfunc TestOnlyArm(t *testing.T) {}\n",
		"shared_test.go": "package p_test\n\nimport \"testing\"\n\nfunc TestShared(t *testing.T) {}\n",
	} {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	options := Options{targetPlatform: "linux/arm64", buildTags: []string{"purego"}}

	if withdrawn := targetArchExcludedTestsOver(mustPlatformLattice(universeArm), dir, options); len(withdrawn) != 0 {
		t.Errorf("linux/arm64 over %v withdrew %v; its own flavour ships arm64", universeArm, targetArchExcludedNames(withdrawn))
	}

	got := targetArchExcludedTestsOver(mustPlatformLattice(universeToday), dir, options)

	if names := targetArchExcludedNames(got); !reflect.DeepEqual(names, []string{"TestOnlyArm"}) {
		t.Fatalf("linux/arm64 over %v withdrew %v, want [TestOnlyArm]", universeToday, names)
	}

	if want := "arm_test.go is not built for linux/amd64 (`//go:build arm64`); one linux build serves every shipped RID"; got[0].Detail != want {
		t.Errorf("detail = %q, want %q", got[0].Detail, want)
	}
}

// A proof page's withdrawals come from the flavours that ship two architectures, and only darwin's does: the page reads
// the same lines at today's universe and at one carrying linux/arm64, whose flavour ships arm64 alone.
func TestProofPageArchWithdrawalsAreTheSameAtFourTargets(t *testing.T) {
	goRoot := t.TempDir()
	dir := filepath.Join(goRoot, "src", "example", "p")

	if err := os.MkdirAll(dir, 0o755); err != nil {
		t.Fatal(err)
	}

	for name, content := range map[string]string{
		"x_amd64_test.go": "package p_test\n\nimport \"testing\"\n\nfunc TestAmd(t *testing.T) {}\n",
		"y_test.go":       "//go:build amd64 && (linux || darwin)\n\npackage p_test\n\nimport \"testing\"\n\nfunc TestPages(t *testing.T) {}\n",
		"z_test.go":       "//go:build arm64\n\npackage p_test\n\nimport \"testing\"\n\nfunc TestArm(t *testing.T) {}\n",
	} {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	want := []string{
		"darwin: TestAmd: x_amd64_test.go is not built for darwin/arm64; one darwin build serves both RIDs",
		"darwin: TestPages: y_test.go is not built for darwin/arm64 (`//go:build amd64 && (linux || darwin)`); one darwin build serves both RIDs",
	}

	if got := proofPageArchWithdrawals(goRoot, "example/p", []string{"purego"}); !reflect.DeepEqual(got, want) {
		t.Errorf("the declared universe's withdrawals = %q, want %q", got, want)
	}

	for _, universe := range [][]string{universeToday, universeArm} {
		if got := proofPageArchWithdrawalsOver(mustPlatformLattice(universe), goRoot, "example/p", []string{"purego"}); !reflect.DeepEqual(got, want) {
			t.Errorf("over %v, withdrawals = %q, want %q", universe, got, want)
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

	options := Options{targetPlatform: corpusLattice().targetsOfGOOS("darwin")[0], buildTags: []string{"purego"}}

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

// The withdrawn-section detail is markdown on a proof page, so its build constraint is a CODE SPAN: a bare `||` in
// an OR'd constraint (internal/cpu's cpu_x86_test.go, `//go:build 386 || amd64`) otherwise makes every withdrawn line
// a pipe line, and the site publishes the block as a table (repoguard's TestDocsNoPipeOnlyTable). The span covers the
// AND'd shape too, and a file with no build line still adds nothing.
func TestBuildLineSuffixIsACodeSpan(t *testing.T) {
	for _, tc := range []struct{ src, want string }{
		{"//go:build 386 || amd64\n\npackage p_test\n", " (`//go:build 386 || amd64`)"},
		{"//go:build amd64 && (linux || darwin)\n\npackage p_test\n", " (`//go:build amd64 && (linux || darwin)`)"},
		{"package p_test\n", ""},
	} {
		file, err := parser.ParseFile(token.NewFileSet(), "x_test.go", tc.src, parser.ParseComments)
		if err != nil {
			t.Fatal(err)
		}

		if got := buildLineSuffix(file); got != tc.want {
			t.Errorf("buildLineSuffix(%q) = %q, want %q", tc.src, got, tc.want)
		}
	}
}
