// hostFatalClass_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// hostFatalClass is the ONE disclosure class that changes what RUNS rather than labelling what a
// run produced, so its guards are about the command lines and the counts rather than about a
// verdict pair. Positive controls first, per this package's convention: a guard that cannot fail
// proves nothing, and each assertion below was made to fail once before it was trusted.

func hostFatal(names ...string) map[string]testDisclosure {
	m := map[string]testDisclosure{}
	for _, n := range names {
		m[n] = testDisclosure{Name: n, Class: hostFatalClass, Reason: "because " + n}
	}
	return m
}

// The expression is what both sides receive VERBATIM, so its shape is load-bearing: anchored per
// name so TestFoo cannot withdraw TestFooBar, and sorted so two logs of the same run compare by
// eye instead of differing by map iteration order.
func TestHostFatalExpressionIsAnchoredAndStable(t *testing.T) {
	got := hostFatalSkipExpression(hostFatal("TestZebra", "TestPanicOnFault", "TestAardvark"))
	want := "^(?:TestAardvark|TestPanicOnFault|TestZebra)$"
	if got != want {
		t.Fatalf("expression must be anchored and sorted so both sides get one stable string;\n got %q\nwant %q", got, want)
	}
}

// Anchoring is not cosmetic: without it a host-fatal TestFoo would silently withdraw TestFooBar
// from BOTH sides, and the comparison would stay symmetric while quietly measuring less.
func TestHostFatalExpressionDoesNotWithdrawPrefixNeighbours(t *testing.T) {
	expr := hostFatalSkipExpression(hostFatal("TestFoo"))
	if !strings.HasPrefix(expr, "^(?:") || !strings.HasSuffix(expr, ")$") {
		t.Fatalf("expression must be anchored at both ends, got %q", expr)
	}
}

// Every package but one has no host-fatal entry, and those runs must be unchanged: no flag on
// either command line at all.
func TestHostFatalExpressionEmptyWithoutAnEntry(t *testing.T) {
	if got := hostFatalSkipExpression(map[string]testDisclosure{
		"TestX": {Name: "TestX", Class: platformSkipClass},
	}); got != "" {
		t.Fatalf("a manifest with no host-fatal entry must add no flag; got %q", got)
	}
}

// The withdrawn names appear in NEITHER side's results by construction, so they have to be added
// to Disclosed explicitly or the row's counts silently shrink -- runtime/debug banks 4 + 6, never
// 4 + 5 with the test quietly gone.
func TestHostFatalNamesAreCountedAsDisclosed(t *testing.T) {
	got := hostFatalNames(hostFatal("TestPanicOnFault"))
	if len(got) != 1 || !strings.HasPrefix(got[0], "TestPanicOnFault (host-fatal): ") {
		t.Fatalf("host-fatal names must reach the DISCLOSED column carrying their class and reason; got %#v", got)
	}
}

// THE MINT RULE. An entry must not name a test any committed proof page records as a MATCHING
// verdict, because the manifest is shared across platforms and the exclusion would withdraw a row
// that platform runs successfully -- both sides agreeing because both were told to skip.
func TestHostFatalMintRefusesATestAnotherPlatformMatches(t *testing.T) {
	root := t.TempDir()
	pkg := filepath.Join(root, "src", "core", "runtime", "debug")
	if err := os.MkdirAll(pkg, 0o755); err != nil {
		t.Fatal(err)
	}
	// findGo2CSRootAbove looks for the marker that identifies a go2cs root.
	if err := os.MkdirAll(filepath.Join(root, "src", "core", "golib"), 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(root, "src", "core", "golib", "golib.csproj"), []byte("<Project/>"), 0o644); err != nil {
		t.Fatal(err)
	}
	pages := filepath.Join(root, "docs", validationDocsDirName, validationCurrentDirName)
	if err := os.MkdirAll(pages, 0o755); err != nil {
		t.Fatal(err)
	}
	page := "| Test | `go test` | go2cs |\n|---|---|---|\n" +
		"| `TestMatchesElsewhere` | pass | pass |\n" +
		"| `TestDivergesElsewhere` | pass | fail ([disclosed](#d)) |\n"
	if err := os.WriteFile(filepath.Join(pages, "runtime.debug.md"), []byte(page), 0o644); err != nil {
		t.Fatal(err)
	}

	// REFUSED: another platform records it matching.
	v, u := hostFatalMintViolations(pkg, hostFatal("TestMatchesElsewhere"))
	if len(v) != 1 || !strings.Contains(v[0], "TestMatchesElsewhere") {
		t.Fatalf("the mint must refuse a name a proof page records as matching; got %#v", v)
	}
	if len(u) != 0 {
		t.Fatalf("a name the rule DECIDED on is not unchecked; got %#v", u)
	}

	// ALLOWED, AND POSITIVELY CLEARED: the page records it diverging, so excluding it withdraws
	// nothing that works -- and because the name reached the evidence base, it is not unchecked.
	// This arm is the one that makes the pair discriminating: without it, "cleared" and "no
	// evidence" would both be len(v)==0 and the fix would assert nothing.
	if v, u := hostFatalMintViolations(pkg, hostFatal("TestDivergesElsewhere")); len(v) != 0 || len(u) != 0 {
		t.Fatalf("a name a page records as DIVERGING must mint cleanly and be positively cleared; got v=%#v u=%#v", v, u)
	}

	// ALLOWED BUT UNCHECKED, and this is runtime/debug's real case: panic_test.go is //go:build
	// unix, so TestPanicOnFault appears on ZERO committed pages -- the pages are the Windows record.
	// It must still mint (the rule refuses only on positive evidence) while SAYING it decided
	// nothing. Before 2026-09-06 this returned a bare nil, identical to the arm above, and the
	// silence read as clearance.
	v, u = hostFatalMintViolations(pkg, hostFatal("TestNotOnAnyPage"))
	if len(v) != 0 {
		t.Fatalf("a name absent from every page must mint cleanly; got %#v", v)
	}
	if len(u) != 1 || !strings.Contains(u[0], "TestNotOnAnyPage") {
		t.Fatalf("a name absent from every page must be reported UNCHECKED, not silently cleared; got %#v", u)
	}
}

// The two OTHER silent returns, which carried no comment and no signal until 2026-09-06. Neither is
// a violation -- a scratch output root and a fresh clone are both legitimate -- but both decide
// nothing about the entry, and a reviewer must be able to tell that from a run that decided
// something. Positive control: each arm was made to fail by restoring the bare `return nil`.
func TestHostFatalMintReportsWhenItCannotCheckAtAll(t *testing.T) {
	// No go2cs root above the output path: nothing to read, so every entry is unexamined.
	stray := t.TempDir()
	v, u := hostFatalMintViolations(stray, hostFatal("TestA", "TestB"))
	if len(v) != 0 {
		t.Fatalf("no evidence is not a violation; got %#v", v)
	}
	if len(u) != 2 {
		t.Fatalf("with no go2cs root every host-fatal entry is unchecked; got %#v", u)
	}

	// A root with NO committed pages -- a fresh clone or a staging root.
	root := t.TempDir()
	pkg := filepath.Join(root, "src", "core", "some", "pkg")
	if err := os.MkdirAll(pkg, 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.MkdirAll(filepath.Join(root, "src", "core", "golib"), 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(root, "src", "core", "golib", "golib.csproj"), []byte("<Project/>"), 0o644); err != nil {
		t.Fatal(err)
	}
	v, u = hostFatalMintViolations(pkg, hostFatal("TestA"))
	if len(v) != 0 {
		t.Fatalf("an empty evidence base is not a violation; got %#v", v)
	}
	if len(u) != 1 || !strings.Contains(u[0], "TestA") {
		t.Fatalf("an empty evidence base must report the entry as unchecked; got %#v", u)
	}

	// And the honest silent return: no entry of the class at all, so there is no claim to report.
	if v, u := hostFatalMintViolations(pkg, map[string]testDisclosure{
		"TestX": {Name: "TestX", Class: platformSkipClass},
	}); len(v) != 0 || len(u) != 0 {
		t.Fatalf("a manifest with no host-fatal entry decides nothing and reports nothing; got v=%#v u=%#v", v, u)
	}
}

// THE LAUNDERING GUARD, and it covers the call site rather than a helper. A host-fatal test
// produces no verdict on either side, so it has no status pair to absorb. If one ever REACHES
// matchTerminalStatuses the exclusion did not take -- the test ran -- and reading it as disclosed
// would hide precisely that. It must fall through to a mismatch and be seen.
func TestHostFatalIsRefusedAsAFailureAbsorber(t *testing.T) {
	names := []string{"TestRan"}
	goResults := map[string]string{"TestRan": "pass"}
	csResults := map[string]string{"TestRan": "fail"}
	csOutputs := map[string]string{"TestRan": "the process died: SIGSEGV"}

	// A host-fatal entry whose signature MATCHES the failure text. Any other class would absorb
	// this; host-fatal must not, because its presence here means the withdrawal failed.
	fatal := map[string]testDisclosure{
		"TestRan": {Name: "TestRan", Class: hostFatalClass, Signature: "SIGSEGV", Reason: "r"},
	}
	mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, fatal, csOutputs)
	if len(disclosed) != 0 {
		t.Fatalf("host-fatal must NOT absorb a failure -- reaching this arm means the test RAN and the exclusion failed; disclosed=%#v", disclosed)
	}
	if len(mismatches) != 1 {
		t.Fatalf("a host-fatal test that produced a verdict must surface as a mismatch; mismatches=%#v", mismatches)
	}

	// Control on the SAME inputs: another class with the same signature DOES absorb it, so the
	// refusal above is about the class and not about the fixture.
	other := map[string]testDisclosure{
		"TestRan": {Name: "TestRan", Class: "runtime-capability", Signature: "SIGSEGV", Reason: "r"},
	}
	mismatches, _, disclosed, _ = matchTerminalStatuses(names, goResults, csResults, other, csOutputs)
	if len(disclosed) != 1 || len(mismatches) != 0 {
		t.Fatalf("control: an ordinary class must still absorb this exact fixture; disclosed=%#v mismatches=%#v", disclosed, mismatches)
	}
}

// The signature requirement is class-aware, and getting this wrong is not theoretical: the first
// runtime/debug mint was REJECTED by the loader for an empty signature, so no skip reached either
// command line and the run proceeded exactly as before -- the exclusion silently not taking. A
// host-fatal test has no captured output to pin, so requiring a signature would mean inventing a
// string nothing can match; every other class must still carry one, because the signature is what
// stops a disclosure absorbing a regression beyond the documented divergence.
func TestHostFatalMayOmitItsSignatureAndOtherClassesMayNot(t *testing.T) {
	dir := t.TempDir()
	write := func(class, signature string) error {
		body := `{"schemaVersion":1,"disclosures":[{"name":"T","class":"` + class +
			`","signature":"` + signature + `","reason":"r"}]}`
		if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), []byte(body), 0o644); err != nil {
			t.Fatal(err)
		}
		_, _, err := loadTestDisclosures(dir)
		return err
	}
	if err := write(hostFatalClass, ""); err != nil {
		t.Fatalf("host-fatal must load with no signature -- it has no verdict to pin one against: %v", err)
	}
	if err := write("runtime-capability", ""); err == nil {
		t.Fatal("every other class must still require a signature: it is the guard that stops a disclosure absorbing a regression")
	}
	if err := write(hostFatalClass, ""); err != nil {
		t.Fatalf("re-check after the negative case: %v", err)
	}
}
