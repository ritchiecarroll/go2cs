// nugetSubstitution_test.go - Gbtc
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

	"go2cs/internal/sourcemeta"
)

func substitution(module, version, id string, requires ...sourcemeta.Require) *nugetSubstitution {
	return &nugetSubstitution{module: module, version: version, nugetID: id, packageVersion: "1.0.0",
		description: sourcemeta.Description{Module: module, ModuleVersion: version, Requires: requires}}
}

func TestNuGetSubstitutionCoversByLongestModulePath(t *testing.T) {
	saved := nugetSubstitutions
	t.Cleanup(func() { nugetSubstitutions = saved })
	nugetSubstitutions = map[string]*nugetSubstitution{
		"example.test/a":   substitution("example.test/a", "v1.0.0", "a"),
		"example.test/a/b": substitution("example.test/a/b", "v1.0.0", "ab"),
	}

	for importPath, want := range map[string]string{
		"example.test/a":       "a",
		"example.test/a/x":     "a",
		"example.test/a/b":     "ab",
		"example.test/a/b/c/d": "ab",
	} {
		if got := nugetSubstitutionFor(importPath); got == nil || got.nugetID != want {
			t.Errorf("%s -> %+v; want %s", importPath, got, want)
		}
	}

	for _, importPath := range []string{"example.test/ab", "example.test", "other.test/a"} {
		if got := nugetSubstitutionFor(importPath); got != nil {
			t.Errorf("%s -> %+v; want none", importPath, got)
		}
	}
}

// CLOSURE CONSISTENCY (COORD ruling 2026-10-02): a substituted module whose package requires a module this
// run converts locally would bring a second assembly of the same namespace through NuGet. So it is demoted,
// and demotion is a fixed point: N requires O, which converts locally, so N is demoted; M requires N, so M
// is demoted; L requires M, so L is demoted. Every reason names the blocking module.
func TestNuGetSubstitutionClosureDemotesAThreeModuleChain(t *testing.T) {
	req := func(module, version, id string) sourcemeta.Require {
		return sourcemeta.Require{Module: module, Version: version, NuGetID: id}
	}

	modules := map[string]thirdPartyModule{}

	for _, m := range []string{"example.test/l", "example.test/m", "example.test/n", "example.test/o", "example.test/p"} {
		modules[m] = thirdPartyModule{path: m, version: "v1.0.0"}
	}

	subs := map[string]*nugetSubstitution{
		"example.test/l": substitution("example.test/l", "v1.0.0", "id.l", req("example.test/m", "v1.0.0", "id.m")),
		"example.test/m": substitution("example.test/m", "v1.0.0", "id.m", req("example.test/n", "v1.0.0", "id.n")),
		"example.test/n": substitution("example.test/n", "v1.0.0", "id.n", req("example.test/o", "v1.0.0", "id.o")),
		"example.test/p": substitution("example.test/p", "v1.0.0", "id.p"),
	}

	demoted := closeNuGetSubstitutions(subs, modules)

	for module, blocker := range map[string]string{"example.test/n": "example.test/o", "example.test/m": "example.test/n", "example.test/l": "example.test/m"} {
		if !strings.Contains(demoted[module], blocker) {
			t.Errorf("%s: demotion reason %q does not name %s", module, demoted[module], blocker)
		}

		if subs[module] != nil {
			t.Errorf("%s is still substituted", module)
		}
	}

	if subs["example.test/p"] == nil || demoted["example.test/p"] != "" {
		t.Errorf("an unrelated consistent substitution was demoted: %q", demoted["example.test/p"])
	}
}

// The package was built against one version of a dependency; this run selects another, so the two would
// disagree at run time.
func TestNuGetSubstitutionClosureDemotesAVersionSkew(t *testing.T) {
	modules := map[string]thirdPartyModule{
		"example.test/m": {path: "example.test/m", version: "v1.0.0"},
		"example.test/n": {path: "example.test/n", version: "v1.1.0"},
	}
	subs := map[string]*nugetSubstitution{
		"example.test/m": substitution("example.test/m", "v1.0.0", "id.m", sourcemeta.Require{Module: "example.test/n", Version: "v1.0.0", NuGetID: "id.n"}),
		"example.test/n": substitution("example.test/n", "v1.1.0", "id.n"),
	}

	demoted := closeNuGetSubstitutions(subs, modules)

	if reason := demoted["example.test/m"]; !strings.Contains(reason, "v1.0.0") || !strings.Contains(reason, "v1.1.0") {
		t.Errorf("version skew reason %q does not name both versions", reason)
	}

	if subs["example.test/n"] == nil {
		t.Errorf("n itself was demoted")
	}
}

// A required module the run does not see at all is no conflict: nothing converts it locally, so NuGet's
// own dependency supplies it. A required module mapped to a DIFFERENT package is.
func TestNuGetSubstitutionClosureOnAbsentAndMismatchedRequires(t *testing.T) {
	modules := map[string]thirdPartyModule{
		"example.test/m": {path: "example.test/m", version: "v1.0.0"},
		"example.test/n": {path: "example.test/n", version: "v1.0.0"},
		"example.test/k": {path: "example.test/k", version: "v1.0.0"},
	}
	subs := map[string]*nugetSubstitution{
		"example.test/m": substitution("example.test/m", "v1.0.0", "id.m", sourcemeta.Require{Module: "example.test/absent", Version: "v1.0.0", NuGetID: "id.absent"}),
		"example.test/k": substitution("example.test/k", "v1.0.0", "id.k", sourcemeta.Require{Module: "example.test/n", Version: "v1.0.0", NuGetID: "id.n.expected"}),
		"example.test/n": substitution("example.test/n", "v1.0.0", "id.n.actual"),
	}

	demoted := closeNuGetSubstitutions(subs, modules)

	if subs["example.test/m"] == nil {
		t.Errorf("a require outside the closure demoted m: %q", demoted["example.test/m"])
	}

	if reason := demoted["example.test/k"]; !strings.Contains(reason, "id.n.expected") || !strings.Contains(reason, "id.n.actual") {
		t.Errorf("a require mapped to another package: reason %q does not name both IDs", reason)
	}
}
