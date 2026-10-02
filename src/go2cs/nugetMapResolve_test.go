// nugetMapResolve_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

func mustSource(t *testing.T, name, body string) nugetMapSource {
	t.Helper()
	src, err := parseNuGetMap(name, []byte(body))

	if err != nil {
		t.Fatal(err)
	}

	return src
}

var (
	modA = thirdPartyModule{path: "github.com/a/a", version: "v1.0.0"}
	modB = thirdPartyModule{path: "github.com/b/b", version: "v2.1.0"}
)

func TestNuGetMapFirstSourceNamingAModuleWins(t *testing.T) {
	user := mustSource(t, "mine.txt", mapRow(modA.path, "user.a", "community"))
	registry := mustSource(t, "registry", mapRow(modA.path, "registry.a", "canonical")+mapRow(modB.path, "registry.b", "canonical"))

	decisions, _ := resolveNuGetMappings([]thirdPartyModule{modA, modB}, []nugetMapSource{user, registry}, nil, nugetMapOptions{sources: []string{"mine.txt"}})

	if a := decisionFor(t, decisions, modA.path); !a.mapped || a.nugetID != "user.a" || a.layer != "mine.txt" || a.version != "v1.0.0" {
		t.Errorf("A: %+v; want user.a from mine.txt", a)
	}

	if b := decisionFor(t, decisions, modB.path); !b.mapped || b.nugetID != "registry.b" || b.layer != "registry" {
		t.Errorf("B: %+v; want registry.b from the registry", b)
	}
}

// withdrawn = unmapped, and it ENDS the lookup: a user file can withdraw a registry row.
func TestNuGetMapWithdrawnShadowsLowerSources(t *testing.T) {
	user := mustSource(t, "mine.txt", mapRow(modA.path, "user.a", "withdrawn"))
	registry := mustSource(t, "registry", mapRow(modA.path, "registry.a", "canonical"))

	decisions, _ := resolveNuGetMappings([]thirdPartyModule{modA}, []nugetMapSource{user, registry}, nil, nugetMapOptions{sources: []string{"mine.txt"}})
	a := decisionFor(t, decisions, modA.path)

	if a.mapped || !strings.Contains(a.note, "withdrawn") || !strings.Contains(a.note, "mine.txt") {
		t.Errorf("A: %+v; want unmapped, withdrawn in mine.txt", a)
	}
}

func TestNuGetMapExcludedModuleStaysLocal(t *testing.T) {
	registry := mustSource(t, "registry", mapRow(modA.path, "registry.a", "canonical"))

	decisions, _ := resolveNuGetMappings([]thirdPartyModule{modA}, []nugetMapSource{registry}, nil, nugetMapOptions{sources: []string{"x"}, exclude: []string{modA.path}})
	a := decisionFor(t, decisions, modA.path)

	if a.mapped || !strings.Contains(a.note, "-nuget-map-exclude") {
		t.Errorf("A: %+v; want unmapped by -nuget-map-exclude", a)
	}
}

// A replace directive is the user's own choice of source, so a mapping never overrides it.
func TestNuGetMapNeverOverridesAReplace(t *testing.T) {
	replaced := thirdPartyModule{path: modA.path, version: "v1.0.0", replaced: true}
	registry := mustSource(t, "registry", mapRow(modA.path, "registry.a", "canonical"))

	decisions, _ := resolveNuGetMappings([]thirdPartyModule{replaced}, []nugetMapSource{registry}, nil, nugetMapOptions{sources: []string{"x"}})
	a := decisionFor(t, decisions, modA.path)

	if a.mapped || !strings.Contains(a.note, "replace") {
		t.Errorf("A: %+v; want unmapped, replaced locally", a)
	}
}

func TestNuGetMapCanonicalOnlySkipsCommunityRows(t *testing.T) {
	registry := mustSource(t, "registry", mapRow(modA.path, "registry.a", "community")+mapRow(modB.path, "registry.b", "canonical"))

	decisions, _ := resolveNuGetMappings([]thirdPartyModule{modA, modB}, []nugetMapSource{registry}, nil, nugetMapOptions{sources: []string{"x"}, canonicalOnly: true})

	if a := decisionFor(t, decisions, modA.path); a.mapped || !strings.Contains(a.note, "-nuget-map-canonical-only") {
		t.Errorf("A: %+v; want the community row skipped by -nuget-map-canonical-only", a)
	}

	if b := decisionFor(t, decisions, modB.path); !b.mapped {
		t.Errorf("B: %+v; want the canonical row mapped", b)
	}
}

func TestNuGetMapUnnamedModuleStaysLocal(t *testing.T) {
	registry := mustSource(t, "registry", mapRow(modA.path, "registry.a", "canonical"))

	decisions, _ := resolveNuGetMappings([]thirdPartyModule{modB}, []nugetMapSource{registry}, nil, nugetMapOptions{sources: []string{"x"}})

	if b := decisionFor(t, decisions, modB.path); b.mapped || !strings.Contains(b.note, "no mapping") {
		t.Errorf("B: %+v; want unmapped, no mapping in any source", b)
	}
}

// The lock pins what was resolved: a source that now disagrees is REPORTED, never adopted
// (docs/PLAN-nugetgo.md 4.4), until -nuget-map-refresh re-resolves deliberately.
func TestNuGetMapLockWinsAndADisagreementIsReported(t *testing.T) {
	lock := map[string]nugetLockEntry{
		modA.path: {module: modA.path, version: "v1.0.0", nugetID: "old.a", status: "canonical", layer: "registry", packageVersion: "-", contentHash: "-"},
	}
	registry := mustSource(t, "registry", mapRow(modA.path, "new.a", "community"))

	decisions, warnings := resolveNuGetMappings([]thirdPartyModule{modA}, []nugetMapSource{registry}, lock, nugetMapOptions{sources: []string{"x"}})
	a := decisionFor(t, decisions, modA.path)

	if !a.mapped || a.nugetID != "old.a" || !a.fromLock {
		t.Errorf("A: %+v; want the locked old.a kept", a)
	}

	if len(warnings) != 1 || !strings.Contains(warnings[0], "old.a") || !strings.Contains(warnings[0], "new.a") || !strings.Contains(warnings[0], "-nuget-map-refresh") {
		t.Errorf("warnings %q; want one naming old.a, new.a and -nuget-map-refresh", warnings)
	}

	decisions, warnings = resolveNuGetMappings([]thirdPartyModule{modA}, []nugetMapSource{registry}, lock, nugetMapOptions{sources: []string{"x"}, refresh: true})
	a = decisionFor(t, decisions, modA.path)

	if !a.mapped || a.nugetID != "new.a" || a.fromLock || len(warnings) != 0 {
		t.Errorf("with -nuget-map-refresh A: %+v, warnings %q; want new.a adopted silently", a, warnings)
	}
}

// A locked mapping whose source now has NO row (or a withdrawn one) is a disagreement too.
func TestNuGetMapLockedModuleMissingFromTheSourcesIsReported(t *testing.T) {
	lock := map[string]nugetLockEntry{
		modA.path: {module: modA.path, version: "v1.0.0", nugetID: "old.a", status: "canonical", layer: "registry", packageVersion: "-", contentHash: "-"},
	}
	registry := mustSource(t, "registry", mapRow(modA.path, "old.a", "withdrawn"))

	decisions, warnings := resolveNuGetMappings([]thirdPartyModule{modA}, []nugetMapSource{registry}, lock, nugetMapOptions{sources: []string{"x"}})

	if a := decisionFor(t, decisions, modA.path); !a.mapped || a.nugetID != "old.a" {
		t.Errorf("A: %+v; want the lock kept", a)
	}

	if len(warnings) != 1 || !strings.Contains(warnings[0], "withdrawn") {
		t.Errorf("warnings %q; want one naming the withdrawal", warnings)
	}
}

// The user's CURRENT filters still apply to a locked mapping: a module excluded now, or a community
// mapping under -nuget-map-canonical-only, is not taken from the lock.
func TestNuGetMapFiltersApplyToLockedMappings(t *testing.T) {
	lock := map[string]nugetLockEntry{
		modA.path: {module: modA.path, version: "v1.0.0", nugetID: "old.a", status: "community", layer: "registry", packageVersion: "-", contentHash: "-"},
	}

	decisions, _ := resolveNuGetMappings([]thirdPartyModule{modA}, nil, lock, nugetMapOptions{sources: []string{"x"}, canonicalOnly: true})

	if a := decisionFor(t, decisions, modA.path); a.mapped {
		t.Errorf("A: %+v; a locked community mapping survived -nuget-map-canonical-only", a)
	}

	decisions, _ = resolveNuGetMappings([]thirdPartyModule{modA}, nil, lock, nugetMapOptions{sources: []string{"x"}, exclude: []string{modA.path}})

	if a := decisionFor(t, decisions, modA.path); a.mapped {
		t.Errorf("A: %+v; a locked mapping survived -nuget-map-exclude", a)
	}
}

func TestNuGetMapDecisionsAreSortedByModule(t *testing.T) {
	decisions, _ := resolveNuGetMappings([]thirdPartyModule{modB, modA}, nil, nil, nugetMapOptions{sources: []string{"x"}})

	if len(decisions) != 2 || decisions[0].module != modA.path || decisions[1].module != modB.path {
		t.Errorf("decisions not sorted by module path: %+v", decisions)
	}
}

// The provenance report (docs/PLAN-nugetgo.md 4.5): a community mapping reads as a trust decision; a row
// S3b APPLIED names the exact-pinned PackageReference it became; a mapped row S3b could not apply says it
// converted locally, and why (no matching package, or a closure demotion naming its blocker).
func TestNuGetMapReportNamesWhatWasAppliedAndWhy(t *testing.T) {
	decisions := []nugetMapDecision{
		{module: modA.path, version: "v1.0.0", mapped: true, nugetID: "id.a", status: "canonical", layer: "registry", applied: true, packageVersion: "1.0.0.1"},
		{module: modB.path, version: "v2.1.0", mapped: true, nugetID: "id.b", status: "community", layer: "mine.txt", note: "demoted: its package requires github.com/c/c, which this run converts locally"},
		{module: "github.com/c/c", version: "v0.1.0", note: "no mapping in any source"},
	}

	report := formatNuGetMapReport(decisions, []string{"go2cs.nuget.lock disagrees for X"})

	for _, want := range []string{
		"id.a", "canonical", "registry", "PackageReference id.a [1.0.0.1]",
		"id.b", "community", "mine.txt", "TRUST", "converted locally", "demoted", "requires github.com/c/c",
		"github.com/c/c", "no mapping in any source",
		"WARNING", "go2cs.nuget.lock disagrees for X",
	} {
		if !strings.Contains(report, want) {
			t.Errorf("report lacks %q:\n%s", want, report)
		}
	}

	if strings.Contains(report, "not yet applied") {
		t.Errorf("the report still carries S3a's not-yet-applied column:\n%s", report)
	}

	if strings.Count(report, "TRUST") != 1 {
		t.Errorf("TRUST must mark the community row only:\n%s", report)
	}
}
