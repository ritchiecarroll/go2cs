// nugetMapSourceWording_test.go - Gbtc
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

// The provenance report and its warnings name the source that was read: the default registry when no -nuget-map
// was given, the flag when one was, and both, in precedence order, when both answered. A run with no -nuget-map
// used to read "NuGet mappings (-nuget-map): ..." and, on a host that could not reach the registry,
// "WARNING (-nuget-map): -nuget-map https://nugetgo.net/v1/mappings.txt: ...": a flag the user never passed.
func TestNuGetMapReportNamesTheSourceThatWasRead(t *testing.T) {
	decisions := []nugetMapDecision{
		{module: modA.path, version: "v1.300.0", mapped: true, nugetID: "id.a", status: "canonical", layer: nugetMapRegistryURL, applied: true, packageVersion: "1.300.0.1"},
	}

	registryOnly := formatNuGetMapReport(decisions, nil, nugetMapSourceList(nugetMapOptions{}))

	if strings.Contains(registryOnly, "-nuget-map") {
		t.Errorf("a run with no -nuget-map names the flag:\n%s", registryOnly)
	}

	if !strings.Contains(registryOnly, "NuGet mappings (the default registry): 1 third-party module(s), 1 mapped, 1 referenced as packages") {
		t.Errorf("the heading does not name the default registry:\n%s", registryOnly)
	}

	flagOnly := formatNuGetMapReport(decisions, nil, nugetMapSourceList(nugetMapOptions{sources: []string{"mine.txt"}, only: true}))

	if !strings.Contains(flagOnly, "NuGet mappings (-nuget-map): ") || strings.Contains(flagOnly, "registry)") {
		t.Errorf("a -nuget-map-only run is not headed by the flag alone:\n%s", flagOnly)
	}

	both := formatNuGetMapReport(decisions, nil, nugetMapSourceList(nugetMapOptions{sources: []string{"mine.txt"}}))

	if !strings.Contains(both, "NuGet mappings (-nuget-map, then the default registry): ") {
		t.Errorf("a run with -nuget-map and the registry fallback does not name both, in precedence order:\n%s", both)
	}

	warned := formatNuGetMapReport(decisions, []string{"go2cs.nuget.lock disagrees for X"}, nugetMapSourceList(nugetMapOptions{}))

	if !strings.Contains(warned, "WARNING (NuGet mappings): go2cs.nuget.lock disagrees for X") || strings.Contains(warned, "-nuget-map") {
		t.Errorf("a warning of a run with no -nuget-map names the flag:\n%s", warned)
	}
}

// The registry's own failure names the registry, not the flag; a source the user listed keeps the flag's name.
func TestNuGetMapSourceFailureNamesTheSource(t *testing.T) {
	f := newNuGetMapFixture(t)
	url := f.url("/team.txt")
	f.server.Close()

	_, warnings, err := loadNuGetMapSource(nugetMapRegistryURL, false)

	if err != nil {
		t.Fatalf("an unreachable registry failed the run: %v", err)
	}

	if len(warnings) != 1 || strings.Contains(warnings[0], "-nuget-map") || !strings.Contains(warnings[0], "the default registry "+nugetMapRegistryURL) {
		t.Errorf("the unreachable registry's warning %q does not name the default registry, or names the flag", warnings)
	}

	if _, _, err := loadNuGetMapSource(url, false); err == nil || !strings.Contains(err.Error(), "-nuget-map "+url) {
		t.Errorf("an unreachable -nuget-map source's error does not name the flag and the source (err %v)", err)
	}
}
