// dynamicTypeGate_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the W2b GATE: a `-tests` conversion that could not name an anonymous struct/interface
// type must FAIL, loudly and by name, instead of exiting 0 into a build whose diagnostics point
// away from the cause.
//
// The defect these tests pin is measured, not hypothetical. runtime's `-tests` conversion emitted
// exactly three `Unresolved dynamic struct type` warnings — zero false positives, zero false
// negatives — and exited 0; the resulting build produced 202 errors across 106 distinct lines, of
// which 5 were real sites (docs/phase4/CENSUS-runtime-first-contact.md, §2.3 and W2). The fallback
// emission is the raw Go type text, which is never valid C#, so the warning was already a free and
// always-correct prediction of a broken artifact that the run then discarded.
//
// The fixture signature below is one of those three, verbatim, so the guard is anchored to the
// real shape rather than to an invented one.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// runtimeScavengeIndexSignature is `pageAlloc.scav`'s anonymous struct — the type that reached
// runtime's export_test.cs at lines 1226/1227/1249 as literal Go source. The production emission
// had already lifted it as `pageAlloc_scav` (mgcsweep.cs:428); the test pass did not consult that
// registry, which is what made the marker unresolvable.
const runtimeScavengeIndexSignature = "struct{index runtime.scavengeIndex; releasedBg internal/runtime/atomic.Uintptr; releasedEager internal/runtime/atomic.Uintptr}"

// withDynamicTypeRegistry runs body against freshly initialized package AND production registries
// and a drained unresolved-type record, restoring all three afterwards so neighbouring tests in
// this package are unaffected by any of them.
func withDynamicTypeRegistry(t *testing.T, body func()) {
	t.Helper()

	savedNames := packageDynamicTypeNames
	packageDynamicTypeNames = make(map[string]string)

	savedProductionNames := productionDynamicTypeNames
	productionDynamicTypeNames = nil

	takeUnresolvedDynamicTypes()

	defer func() {
		packageDynamicTypeNames = savedNames
		productionDynamicTypeNames = savedProductionNames
		takeUnresolvedDynamicTypes()
	}()

	body()
}

// writeDynamicMarkerFixture writes a C# file whose `line`-th line carries a deferred dynamic-type marker
// for signature, and returns its path. Leading filler lines exist so a reported line number of 1
// cannot pass by accident.
func writeDynamicMarkerFixture(t *testing.T, dir, name, signature string, line int) string {
	t.Helper()

	var content strings.Builder

	for i := 1; i < line; i++ {
		content.WriteString("// filler\n")
	}

	content.WriteString("    var x = " + dynamicTypeMarker(signature) + ".Ꮡindex;\n")

	path := filepath.Join(dir, name)

	if err := os.WriteFile(path, []byte(content.String()), 0644); err != nil {
		t.Fatalf("writing fixture %s: %v", path, err)
	}

	return path
}

// TestUnresolvedDynamicTypeIsRecordedWithItsSite is the gate's first half: an unresolvable marker
// must leave a record naming the signature AND the emitted file/line, because the signature alone
// is not actionable in a file with thousands of lines.
func TestUnresolvedDynamicTypeIsRecordedWithItsSite(t *testing.T) {
	withDynamicTypeRegistry(t, func() {
		dir := t.TempDir()
		path := writeDynamicMarkerFixture(t, dir, "export_test.cs", runtimeScavengeIndexSignature, 1226)

		resolveDynamicTypeMarkers([]string{path})

		// The fallback emission is unchanged by this arc: the raw Go signature still replaces the
		// marker, so the file names the exact type that went unresolved.
		emitted, err := os.ReadFile(path)

		if err != nil {
			t.Fatalf("reading fixture back: %v", err)
		}

		if !strings.Contains(string(emitted), runtimeScavengeIndexSignature) {
			t.Errorf("the unresolved marker was not replaced by its raw Go signature; emitted:\n%s", emitted)
		}

		if strings.Contains(string(emitted), dynamicTypeMarkerPrefix) {
			t.Errorf("a dynamic-type marker survived into the emitted file:\n%s", emitted)
		}

		recorded := takeUnresolvedDynamicTypes()

		if len(recorded) != 1 {
			t.Fatalf("expected exactly 1 unresolved-type record, got %d: %+v", len(recorded), recorded)
		}

		if recorded[0].signature != runtimeScavengeIndexSignature {
			t.Errorf("recorded signature = %q, want %q", recorded[0].signature, runtimeScavengeIndexSignature)
		}

		if recorded[0].fileName != path {
			t.Errorf("recorded file = %q, want %q", recorded[0].fileName, path)
		}

		if recorded[0].line != 1226 {
			t.Errorf("recorded line = %d, want 1226 — the site is the half of the report that makes it actionable", recorded[0].line)
		}
	})
}

// TestUnresolvedDynamicTypeFailsTheConversion is the gate itself: the recorded sites must become a
// hard error naming every one of them. This is the assertion that separates the new behavior from
// the old — before this arc the identical run exited 0.
func TestUnresolvedDynamicTypeFailsTheConversion(t *testing.T) {
	withDynamicTypeRegistry(t, func() {
		dir := t.TempDir()
		first := writeDynamicMarkerFixture(t, dir, "export_test.cs", runtimeScavengeIndexSignature, 1226)
		second := writeDynamicMarkerFixture(t, dir, "alg_test.cs", "interface{F()}", 288)

		resolveDynamicTypeMarkers([]string{second, first})

		err := unresolvedDynamicTypeError()

		if err == nil {
			t.Fatal("a conversion that could not name an anonymous type reported SUCCESS; the emitted C# cannot compile, so exiting 0 here is the false green this gate closes")
		}

		message := err.Error()

		for _, required := range []string{
			runtimeScavengeIndexSignature,
			"interface{F()}",
			first + "(1226)",
			second + "(288)",
		} {
			if !strings.Contains(message, required) {
				t.Errorf("gate error does not name %q; it must name every unresolved type and its site.\ngot:\n%s", required, message)
			}
		}

		// Reported in a stable order regardless of the order the files were walked in — output
		// files are appended from concurrent per-file goroutines, so the walk order varies run to
		// run and an unsorted summary would too.
		if strings.Index(message, second) > strings.Index(message, first) {
			t.Errorf("gate error is not in sorted file order:\n%s", message)
		}

		// Draining is what lets a second conversion in the same process start clean.
		if again := unresolvedDynamicTypeError(); again != nil {
			t.Errorf("the gate did not clear its record; a second call still reports:\n%v", again)
		}
	})
}

// TestResolvedDynamicTypeLeavesTheGateGreen is the positive control the whole guard needs: a gate
// that cannot go green is not a measurement. The SAME fixture, with the production pass's lifted
// name registered — which is exactly what W2a's fix makes true — must emit that name and report no
// failure.
func TestResolvedDynamicTypeLeavesTheGateGreen(t *testing.T) {
	withDynamicTypeRegistry(t, func() {
		registerDynamicTypeName(runtimeScavengeIndexSignature, "pageAlloc_scav")

		dir := t.TempDir()
		path := writeDynamicMarkerFixture(t, dir, "export_test.cs", runtimeScavengeIndexSignature, 1226)

		resolveDynamicTypeMarkers([]string{path})

		emitted, err := os.ReadFile(path)

		if err != nil {
			t.Fatalf("reading fixture back: %v", err)
		}

		if !strings.Contains(string(emitted), "pageAlloc_scav") {
			t.Errorf("the marker did not resolve to the registered lifted name; emitted:\n%s", emitted)
		}

		if gateErr := unresolvedDynamicTypeError(); gateErr != nil {
			t.Errorf("a fully resolved conversion was failed by the gate: %v", gateErr)
		}
	})
}

// TestResolvedViaProductionRegistryLeavesTheGateGreen is W2a's own positive control: the SAME
// fixture as TestUnresolvedDynamicTypeIsRecordedWithItsSite, but resolved through
// productionDynamicTypeNames rather than THIS run's own packageDynamicTypeNames — the registry a
// `-tests` reference-model conversion seeds from production's persisted GoDynamicTypeLift records
// (seedProductionDynamicTypeLifts) because nothing in that run re-visits the production source that
// lifted the type. packageDynamicTypeNames stays EMPTY throughout, which is the point: if the fix
// regressed to consulting only the same-run registry, this test would fail exactly where
// TestUnresolvedDynamicTypeIsRecordedWithItsSite still passes.
func TestResolvedViaProductionRegistryLeavesTheGateGreen(t *testing.T) {
	withDynamicTypeRegistry(t, func() {
		productionDynamicTypeNames = map[string]string{runtimeScavengeIndexSignature: "pageAlloc_scav"}

		dir := t.TempDir()
		path := writeDynamicMarkerFixture(t, dir, "export_test.cs", runtimeScavengeIndexSignature, 1226)

		resolveDynamicTypeMarkers([]string{path})

		emitted, err := os.ReadFile(path)

		if err != nil {
			t.Fatalf("reading fixture back: %v", err)
		}

		if !strings.Contains(string(emitted), "pageAlloc_scav") {
			t.Errorf("the marker did not resolve to the production-registered lifted name; emitted:\n%s", emitted)
		}

		if len(packageDynamicTypeNames) != 0 {
			t.Fatalf("packageDynamicTypeNames was written to — this test measures the PRODUCTION registry alone, and a write here would make it pass for the wrong reason")
		}

		if gateErr := unresolvedDynamicTypeError(); gateErr != nil {
			t.Errorf("a conversion resolvable only through production's registry was failed by the gate: %v", gateErr)
		}
	})
}

// TestUndecodableDynamicTypeMarkerFailsTheConversion covers the resolver's OTHER unresolvable
// arm. A payload that is not valid hex names no signature, so the marker is replaced by the raw
// payload — which is no more compilable than a Go type signature is. Same class, same verdict.
func TestUndecodableDynamicTypeMarkerFailsTheConversion(t *testing.T) {
	withDynamicTypeRegistry(t, func() {
		dir := t.TempDir()
		path := filepath.Join(dir, "corrupt_test.cs")
		corrupt := dynamicTypeMarkerPrefix + "not-hex" + dynamicTypeMarkerSuffix

		if err := os.WriteFile(path, []byte("// filler\n    var x = "+corrupt+";\n"), 0644); err != nil {
			t.Fatalf("writing fixture: %v", err)
		}

		resolveDynamicTypeMarkers([]string{path})

		err := unresolvedDynamicTypeError()

		if err == nil {
			t.Fatal("an undecodable dynamic-type marker payload reported SUCCESS; the payload it leaves behind cannot compile either")
		}

		if !strings.Contains(err.Error(), "not-hex") || !strings.Contains(err.Error(), path+"(2)") {
			t.Errorf("gate error does not name the corrupt payload and its site:\n%v", err)
		}
	})
}

// TestTestsConversionConsultsTheDynamicTypeGate pins the WIRING, in the same source-assertion
// style as the converter-staleness guards in embeddedAssets_test.go and for the same reason: the
// mechanism above can be perfect and still prove nothing if nothing calls it. The gate rides
// processConversion's existing error return — which main.go already turns into a nonzero exit — so
// this asserts the call sits inside the `-tests` branch that is the only thing it is scoped to.
func TestTestsConversionConsultsTheDynamicTypeGate(t *testing.T) {
	const driverSource = "conversionDriver.go"

	contents, err := os.ReadFile(driverSource)

	if err != nil {
		t.Fatalf("reading %s: %v", driverSource, err)
	}

	text := string(contents)

	if !strings.Contains(text, "unresolvedDynamicTypeError()") {
		t.Fatalf("%s no longer calls unresolvedDynamicTypeError; a -tests conversion that cannot name an anonymous type is exiting 0 again (W2b)", driverSource)
	}

	// The call must follow the test conversion — the test variants are where the unresolvable
	// markers were measured — and precede the function's success return.
	gate := strings.Index(text, "unresolvedDynamicTypeError()")
	testConversion := strings.Index(text, "processTestConversion(")

	if testConversion == -1 || gate < testConversion {
		t.Errorf("the dynamic-type gate in %s does not run AFTER processTestConversion; the test variants' own unresolved markers would not be covered", driverSource)
	}

	// Scoped to -tests: the -stdlib and plain single-package paths keep warn-and-continue (see
	// unresolvedDynamicTypeError's own comment for why). The nearest `if options.convertTests`
	// before the call is what expresses that.
	convertTests := strings.LastIndex(text[:gate], "if options.convertTests")

	if convertTests == -1 {
		t.Errorf("the dynamic-type gate in %s is not inside an `if options.convertTests` block; it must not fail a -stdlib or single-package conversion", driverSource)
	}
}
