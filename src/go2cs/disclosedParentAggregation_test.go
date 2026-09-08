// disclosedParentAggregation_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"encoding/json"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// The compare oracle's parent-aggregation rule: a t.Run parent whose Go=pass/C#=fail divergence
// is PURELY the roll-up of disclosed-divergent children — no failure output of its own, no own
// disclosure entry, at least one disclosed descendant and NO mismatched descendant —
// reclassifies as disclosed itself (encoding/binary's TestSizeAllocs over its pinned
// alloc-profile children). Any undisclosed failing child keeps the parent a strict mismatch —
// the rule can never mask.
func TestDisclosedParentAggregation(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestAllocs/a": {Name: "TestAllocs/a", Class: "alloc-profile", Signature: "Expected no allocations", Reason: "r"},
		"TestAllocs/b": {Name: "TestAllocs/b", Class: "alloc-profile", Signature: "Expected no allocations", Reason: "r"},
	}

	names := []string{"TestAllocs", "TestAllocs/a", "TestAllocs/b"}
	goResults := map[string]string{"TestAllocs": "pass", "TestAllocs/a": "pass", "TestAllocs/b": "pass"}
	csResults := map[string]string{"TestAllocs": "fail", "TestAllocs/a": "fail", "TestAllocs/b": "fail"}
	csOutputs := map[string]string{"TestAllocs": "", "TestAllocs/a": "Expected no allocations, got 5", "TestAllocs/b": "Expected no allocations, got 7"}

	mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 0 {
		t.Fatalf("fully-disclosed parent must aggregate as disclosed, got mismatches: %v", mismatches)
	}

	if len(disclosed) != 3 {
		t.Fatalf("expected parent + both children disclosed (3), got %v", disclosed)
	}

	// An UNDISCLOSED failing child keeps the parent a strict mismatch.
	names = append(names, "TestAllocs/c")
	goResults["TestAllocs/c"] = "pass"
	csResults["TestAllocs/c"] = "fail"
	csOutputs["TestAllocs/c"] = "some real regression"

	mismatches, _, disclosed, _ = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	foundParent := false

	for _, m := range mismatches {
		if strings.HasPrefix(m, "TestAllocs:") {
			foundParent = true
		}
	}

	if !foundParent {
		t.Fatalf("parent with an undisclosed failing child must stay a strict mismatch, got mismatches: %v disclosed: %v", mismatches, disclosed)
	}

	// A parent with its OWN failure text never aggregates, even over disclosed children.
	csOutputs["TestAllocs"] = "parent-level assertion failed"
	delete(goResults, "TestAllocs/c")
	delete(csResults, "TestAllocs/c")
	names = names[:3]

	mismatches, _, _, _ = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) == 0 {
		t.Fatal("parent with its own failure output must stay a strict mismatch")
	}
}

// The DOWNWARD dual: Go-side rows underneath a signature-matched disclosed root — subtests
// `go test` ran that the converted host never reached, because the disclosed test failed at its
// root before its case fan-out — are withdrawn rather than mismatched (crypto/tls's
// TestBogoSuite over its 3,242 BoGo case rows). The rule never widens: a root failing with the
// WRONG signature withdraws nothing, and a child that EXISTS on the C# side still compares
// strictly.
func TestDisclosedRootWithdrawsGoOnlyDescendants(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestBogo": {Name: "TestBogo", Class: "host-limit", Signature: "bogo failed", Reason: "r"},
	}

	names := []string{"TestBogo", "TestBogo/CaseA", "TestBogo/CaseB", "TestBogo/Sub/CaseC", "TestOther"}
	goResults := map[string]string{"TestBogo": "pass", "TestBogo/CaseA": "pass", "TestBogo/CaseB": "skip", "TestBogo/Sub/CaseC": "skip", "TestOther": "pass"}
	csResults := map[string]string{"TestBogo": "fail", "TestOther": "pass"}
	csOutputs := map[string]string{"TestBogo": "bogo failed: exit status 1"}

	mismatches, _, disclosed, withdrawn := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 0 {
		t.Fatalf("Go-only rows under a signature-matched disclosed root must withdraw, got mismatches: %v", mismatches)
	}

	if len(disclosed) != 1 || disclosed[0] != "TestBogo" {
		t.Fatalf("expected only the root disclosed, got %v", disclosed)
	}

	if len(withdrawn) != 3 {
		t.Fatalf("expected all three Go-only descendants withdrawn, got %v", withdrawn)
	}

	// A root failing with the WRONG signature is a mismatch and withdraws NOTHING — its
	// children stay one-sided mismatches too, so a regressed root can never quietly absorb its
	// subtree.
	csOutputs["TestBogo"] = "some other failure entirely"

	mismatches, _, disclosed, withdrawn = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(withdrawn) != 0 {
		t.Fatalf("a non-matching root signature must withdraw nothing, got %v", withdrawn)
	}

	if len(mismatches) != 4 || len(disclosed) != 0 {
		t.Fatalf("expected the root and all three children as mismatches, got mismatches: %v disclosed: %v", mismatches, disclosed)
	}

	// A child that EXISTS on the C# side never rides the withdrawal — a divergent two-sided row
	// under a disclosed root is still a strict mismatch.
	csOutputs["TestBogo"] = "bogo failed: exit status 1"
	csResults["TestBogo/CaseA"] = "fail"

	mismatches, _, _, withdrawn = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 1 || !strings.HasPrefix(mismatches[0], "TestBogo/CaseA:") {
		t.Fatalf("a two-sided divergent child under a disclosed root must stay a strict mismatch, got %v", mismatches)
	}

	if len(withdrawn) != 2 {
		t.Fatalf("only the still-one-sided descendants withdraw, got %v", withdrawn)
	}
}

// Package-level manifest notes reach the rendered proof page verbatim, above the verdicts —
// the mechanism that lets a caveat about the comparison's MEANING (crypto/tls's expired-fixture
// ceiling) survive every regeneration, where a hand edit to the generated page would not.
func TestManifestNotesRenderOnTheProofPage(t *testing.T) {
	comparison := testComparison{
		Package: "pkg", Status: "validated",
		Go:     map[string]string{"TestA": "pass", "TestB": "fail"},
		CSharp: map[string]string{"TestA": "pass", "TestB": "fail"},
	}

	note := "the expired-fixture ceiling moves with the calendar"
	page := renderValidationProofPage(proofPageProvenance{importPath: "pkg", goVersion: "1.23.1", platform: "windows/amd64", date: "2026-08-18"},
		comparison, nil, []string{note})

	if !strings.Contains(page, "> "+note) {
		t.Fatalf("manifest note must render as a blockquote above the verdicts, got page:\n%s", page)
	}

	if !strings.Contains(page, "| `TestB` | fail | fail |") {
		t.Fatalf("agreed failures must render as matched fail/fail rows, got page:\n%s", page)
	}
}

// A HOST-CONDITIONAL disclosure (coordinator ruling, 2026-08-20) is the one pinned row whose GO
// side is not a fixed baseline, so it is accepted in EXACTLY two shapes and accounts as DISCLOSED
// in both — never as matching. Shape (a) is the ordinary pinned divergence, Go pass / C# fail.
// Shape (b) is agreement on a host where the Go premise itself fails, Go fail / C# fail, which the
// unannotated oracle silently counts as an agreed failure: crypto/tls then reads 401 + 1 where the
// roster banks 400 + 2, and the sweep's `disclosed count moved` check fires on a run in which the
// converted side did not move at all. Shape (b) cannot be forced on a host whose Go BoGo run
// passes, so this fixture IS the proof for it.
func TestHostConditionalDisclosureAccountsInBothShapes(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestBogo": {
			Name: "TestBogo", Class: "host-limit", Signature: "bogo failed: exit status 1", Reason: "r",
			HostConditional: "The Go baseline depends on network reachability of the boringssl module.",
		},
		"TestPinned": {Name: "TestPinned", Class: "alloc-profile", Signature: "Expected no allocations", Reason: "r"},
	}

	csOutputs := map[string]string{
		"TestBogo":   "bogo failed: exit status 1",
		"TestPinned": "Expected no allocations, got 3",
	}

	// Shape (a) — the ordinary pinned divergence, unchanged by the annotation.
	names := []string{"TestBogo", "TestPinned", "TestOther"}
	goResults := map[string]string{"TestBogo": "pass", "TestPinned": "pass", "TestOther": "pass"}
	csResults := map[string]string{"TestBogo": "fail", "TestPinned": "fail", "TestOther": "pass"}

	mismatches, _, disclosed, withdrawn := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 0 || len(disclosed) != 2 || len(withdrawn) != 0 {
		t.Fatalf("shape (a) must disclose both pinned rows, got mismatches: %v disclosed: %v withdrawn: %v", mismatches, disclosed, withdrawn)
	}

	// Shape (b) — the GO side fails; the row still accounts as disclosed, never as matching.
	goResults["TestBogo"] = "fail"
	csResults["TestBogo"] = "fail"

	mismatches, _, disclosed, withdrawn = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 0 {
		t.Fatalf("shape (b) must not mismatch, got: %v", mismatches)
	}

	if !containsName(disclosed, "TestBogo") {
		t.Fatalf("an agreeing host-conditional row must account as DISCLOSED, never as matching, got disclosed: %v", disclosed)
	}

	if len(disclosed) != 2 {
		t.Fatalf("the disclosed count must be host-stable across both shapes (2), got %v", disclosed)
	}

	// The C# side stays pinned in shape (b): a failure that MOVED is a strict mismatch, so the
	// tolerance never reaches the half that was always deterministic.
	csOutputs["TestBogo"] = "some other failure entirely"

	mismatches, _, disclosed, _ = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 1 || !strings.HasPrefix(mismatches[0], "TestBogo:") {
		t.Fatalf("a moved C# failure under shape (b) must be a strict mismatch, got mismatches: %v disclosed: %v", mismatches, disclosed)
	}

	if containsName(disclosed, "TestBogo") {
		t.Fatal("a signature that no longer matches must never account as disclosed")
	}

	// C#-side movement in the OTHER direction — the converted side starts passing — leaves the
	// disclosed set, which is how a self-retiring row retires: the count moves and the sweep fires.
	csOutputs["TestBogo"] = "bogo failed: exit status 1"
	csResults["TestBogo"] = "pass"

	_, _, disclosed, _ = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if containsName(disclosed, "TestBogo") {
		t.Fatal("a converted side that stopped failing must not stay disclosed")
	}

	// An UNANNOTATED row never gains the second shape: Go fail / C# fail on a plain disclosure is
	// an ordinary agreed failure, so the tolerance is confined to rows the coordinator annotated.
	goResults = map[string]string{"TestBogo": "pass", "TestPinned": "fail", "TestOther": "pass"}
	csResults = map[string]string{"TestBogo": "fail", "TestPinned": "fail", "TestOther": "pass"}

	_, _, disclosed, _ = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if containsName(disclosed, "TestPinned") {
		t.Fatalf("an unannotated agree-fail row must stay an ordinary matched agreement, got disclosed: %v", disclosed)
	}
}

// The SECOND ENVIRONMENTAL ARM. An environmentally-conditional test does not fail the same way for
// both of its environmental reasons, and crypto/tls's TestBogoSuite is pinned on the arm that is
// NOT the one a capability-less host takes: `bogo failed: exit status 1` is bogo_shim_test.go:414,
// reached only when the BoGo runner RAN and outlived its own deadline. A host with no BoringSSL
// module dies fifty lines earlier at bogo_shim_test.go:364 — `failed to download boringssl` — and
// so does Go, identically, at the same line. Measured 2026-08-28 (GOMODCACHE at an empty directory,
// GOPROXY=off): both runtimes report `--- FAIL: TestBogoSuite`, and before this arm existed the
// comparison called the CONVERTED side moved and crypto/tls could not validate at all there.
//
// The confinement is the safety argument, and it is asserted here as hard as the admission: the arm
// is honored ONLY in the fail/fail shape, where Go itself failed the same way. In Go pass / C# fail
// the primary signature still governs alone.
func TestHostConditionalSecondFailureArmIsAdmittedOnlyWhenGoFailsToo(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestBogo": {
			Name: "TestBogo", Class: "host-limit", Signature: "bogo failed: exit status 1", Reason: "r",
			HostConditional:          "The Go baseline depends on network reachability of the boringssl module.",
			HostConditionalSignature: "failed to download boringssl",
		},
	}

	names := []string{"TestBogo", "TestOther"}
	csOutputs := map[string]string{"TestBogo": "bogo_shim_test.go:364: failed to download boringssl: exit status 1"}

	// The capability-absent shape: both sides die on the download arm, and the row accounts as
	// disclosed exactly as it does on the deadline arm.
	goResults := map[string]string{"TestBogo": "fail", "TestOther": "pass"}
	csResults := map[string]string{"TestBogo": "fail", "TestOther": "pass"}

	mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 0 {
		t.Fatalf("the second environmental arm must be admitted in the fail/fail shape, got mismatches: %v", mismatches)
	}
	if !containsName(disclosed, "TestBogo") {
		t.Fatalf("the second arm must account as DISCLOSED, never as an agreed match, got disclosed: %v", disclosed)
	}

	// THE confinement. Go passed, so the module WAS reachable — a converted side reporting that it
	// could not download it is a real divergence, and the second arm must not launder it.
	goResults["TestBogo"] = "pass"

	mismatches, _, disclosed, _ = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 1 || !strings.HasPrefix(mismatches[0], "TestBogo:") {
		t.Fatalf("the second arm must NOT be admitted when Go passed, got mismatches: %v disclosed: %v", mismatches, disclosed)
	}
	if containsName(disclosed, "TestBogo") {
		t.Fatal("a Go-passing run whose converted side failed on the OTHER arm is a real divergence, never disclosed")
	}

	// And an UNANNOTATED entry can carry no second arm at all: readTestDisclosureManifest refuses the
	// manifest rather than accepting a pin that would never govern anything.
	dir := t.TempDir()
	manifest := `{"schemaVersion":1,"disclosures":[{"name":"TestBogo","class":"host-limit","signature":"s","reason":"r","hostConditionalSignature":"failed to download boringssl"}]}`
	if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), []byte(manifest), 0o644); err != nil {
		t.Fatal(err)
	}
	if _, _, err := readTestDisclosureManifest(dir); err == nil {
		t.Fatal("a hostConditionalSignature on an unannotated disclosure must be refused, not silently ignored")
	}
}

// The FLOOD exclusion. Shape (b) is precisely the shape in which the Go side DID reach its case
// fan-out, so an annotated root must withdraw its Go-only descendants exactly as a shape-(a) root
// does. Without it TestBogoSuite's 3,243 BoGo subtest rows land as one-sided mismatches — a
// comparison drowned in rows that say nothing about the converted code, on a run where the C# side
// is unmoved.
func TestHostConditionalRootWithdrawsGoOnlyDescendantsWhenGoFails(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestBogo": {
			Name: "TestBogo", Class: "host-limit", Signature: "bogo failed: exit status 1", Reason: "r",
			HostConditional: "The Go baseline depends on network reachability of the boringssl module.",
		},
	}

	names := []string{"TestBogo", "TestBogo/CaseA", "TestBogo/CaseB", "TestBogo/Sub/CaseC", "TestOther"}
	goResults := map[string]string{"TestBogo": "fail", "TestBogo/CaseA": "pass", "TestBogo/CaseB": "fail", "TestBogo/Sub/CaseC": "skip", "TestOther": "pass"}
	csResults := map[string]string{"TestBogo": "fail", "TestOther": "pass"}
	csOutputs := map[string]string{"TestBogo": "bogo failed: exit status 1"}

	mismatches, _, disclosed, withdrawn := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 0 {
		t.Fatalf("the Go-side fan-out of an annotated root must withdraw, not flood, got mismatches: %v", mismatches)
	}

	if len(withdrawn) != 3 {
		t.Fatalf("expected all three Go-only descendants withdrawn, got %v", withdrawn)
	}

	if len(disclosed) != 1 || disclosed[0] != "TestBogo" {
		t.Fatalf("expected only the annotated root disclosed, got %v", disclosed)
	}

	// The exclusion is confined to the annotated row: strip the annotation and the same run is the
	// flood the ruling exists to prevent — the root reads as an agreed failure and its three
	// Go-only children become one-sided mismatches.
	unannotated := map[string]testDisclosure{
		"TestBogo": {Name: "TestBogo", Class: "host-limit", Signature: "bogo failed: exit status 1", Reason: "r"},
	}

	mismatches, _, disclosed, withdrawn = matchTerminalStatuses(names, goResults, csResults, unannotated, csOutputs)

	if len(mismatches) != 3 || len(withdrawn) != 0 || len(disclosed) != 0 {
		t.Fatalf("without the annotation the fan-out must flood as one-sided mismatches, got mismatches: %v withdrawn: %v disclosed: %v", mismatches, withdrawn, disclosed)
	}

	// A C#-side row that EXISTS under an annotated root still compares strictly — the withdrawal
	// only ever covers rows the converted host never reached.
	csResults["TestBogo/CaseA"] = "fail"

	mismatches, _, _, withdrawn = matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 1 || !strings.HasPrefix(mismatches[0], "TestBogo/CaseA:") {
		t.Fatalf("a two-sided divergent child under an annotated root must stay a strict mismatch, got %v", mismatches)
	}

	if len(withdrawn) != 2 {
		t.Fatalf("only the still-one-sided descendants withdraw, got %v", withdrawn)
	}
}

// The proof page derives its disclosed set from verdict DISAGREEMENT, which is exactly what a
// shape-(b) annotated row does not produce. Unless the renderer reads the annotation back from the
// manifest, the page reports 401 + 1 where the roster banks 400 + 2 and carries the row as a plain
// matched fail/fail — the host-stable arithmetic breaks at the evidence layer. The page also owes
// the row its own note, the internal/zstd model: name the dependency, name both accepted shapes.
func TestHostConditionalRowRendersDisclosedWhenBothSidesFail(t *testing.T) {
	provenance := proofPageProvenance{importPath: "pkg", goVersion: "1.23.1", platform: "windows/amd64", date: "2026-08-20"}
	disclosures := map[string]testDisclosure{
		"TestBogo": {
			Name: "TestBogo", Class: "host-limit", Signature: "bogo failed: exit status 1", Reason: "the runner outruns its own deadline",
			HostConditional: "The Go baseline depends on network reachability of the boringssl module and the runner's own 10-minute child deadline.",
		},
	}

	comparison := testComparison{
		Package: "pkg", Status: "validated",
		Go:        map[string]string{"TestBogo": "fail", "TestOther": "pass"},
		CSharp:    map[string]string{"TestBogo": "fail", "TestOther": "pass"},
		Disclosed: []string{"TestBogo (host-limit): the runner outruns its own deadline"},
		Withdrawn: []string{"TestBogo/CaseA", "TestBogo/CaseB"},
	}

	page := renderValidationProofPage(provenance, comparison, disclosures, nil)

	if !strings.Contains(page, "**1 matched · 1 disclosed**") {
		t.Fatalf("an agreeing host-conditional row must still count as disclosed on the page, got page:\n%s", page)
	}

	if !strings.Contains(page, "| `TestBogo` | fail | fail ([disclosed](#disclosed-divergences)) |") {
		t.Fatalf("the verdict row must be marked disclosed rather than rendered as a plain agreed failure, got page:\n%s", page)
	}

	if !strings.Contains(page, "`TestBogo` is **host-conditional**: The Go baseline depends on network reachability") {
		t.Fatalf("the note must name the environmental dependency, got page:\n%s", page)
	}

	for _, shape := range []string{"`go test` **pass**", "**both sides failing**"} {
		if !strings.Contains(page, shape) {
			t.Fatalf("the note must name both accepted shapes, missing %q in page:\n%s", shape, page)
		}
	}

	// An UNANNOTATED disclosure renders no note — it has no second accepted shape to describe.
	plain := map[string]testDisclosure{
		"TestBogo": {Name: "TestBogo", Class: "host-limit", Signature: "s", Reason: "r"},
	}
	comparison.Go["TestBogo"] = "pass"

	if page = renderValidationProofPage(provenance, comparison, plain, nil); strings.Contains(page, "host-conditional") {
		t.Fatalf("an unannotated disclosure must render no host-conditional note, got page:\n%s", page)
	}
}

// containsName is a local helper — the disclosed/withdrawn returns are plain name slices.
func containsName(names []string, name string) bool {
	for _, candidate := range names {
		if candidate == name {
			return true
		}
	}

	return false
}

// The marker IS its sentence, so the loader refuses a blank one: marking a row widens the oracle
// by a whole second accepted status pair, and a manifest that widens it while naming nothing is
// the "broken disclosure must not widen the oracle" case the required-field checks already cover.
func TestHostConditionalMarkerMustNameItsDependency(t *testing.T) {
	write := func(t *testing.T, hostConditional string) string {
		t.Helper()
		dir := t.TempDir()
		manifest := testDisclosureManifest{
			SchemaVersion: 1,
			Disclosures: []testDisclosure{{
				Name: "TestBogo", Class: "host-limit", Signature: "bogo failed", Reason: "r",
				HostConditional: hostConditional,
			}},
		}

		data, err := json.Marshal(manifest)

		if err != nil {
			t.Fatalf("marshal fixture manifest: %v", err)
		}

		if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), data, 0644); err != nil {
			t.Fatalf("write fixture manifest: %v", err)
		}

		return dir
	}

	if _, _, err := readTestDisclosureManifest(write(t, "   \t \n ")); err == nil {
		t.Fatal("a blank host-conditional marker must be rejected, not silently honored")
	}

	// The ordinary shapes still load: an absent marker, and one that names its dependency.
	for _, sentence := range []string{"", "The Go baseline depends on network reachability."} {
		disclosures, _, err := readTestDisclosureManifest(write(t, sentence))

		if err != nil {
			t.Fatalf("loading a manifest with hostConditional %q must succeed: %v", sentence, err)
		}

		if got := disclosures["TestBogo"].HostConditional; got != sentence {
			t.Fatalf("hostConditional round-tripped as %q, want %q", got, sentence)
		}
	}
}
