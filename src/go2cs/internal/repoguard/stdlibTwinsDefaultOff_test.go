// stdlibTwinsDefaultOff_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// M:N row 7 piece 5 (docs/phase4/DATA-mn-row7-r5.md, COORD's ruling (a)): GoStdlibTwins, in src/core/Directory.Build.props,
// builds every converted standard-library package with twins. It is OFF by default until row 10's T1-T5 ruling flips
// it. By O3 (DESIGN-mn-twin-set.md) the first package shipped with twins ships them for good, so a default that read
// "true", or a tracked build file, script or workflow that turned it on, would put a twinned standard library into a
// pack before its window passed. This guard pins the default to false, the one GoTwins line to the switch, and the
// switch to the command line (piece 6's gates and row 10 run from untracked kits).
var (
	stdlibTwinsElement   = regexp.MustCompile(`<GoStdlibTwins(\s[^>]*)?>([^<]*)</GoStdlibTwins>`)
	goTwinsTrueElement   = regexp.MustCompile(`<GoTwins>\s*true\s*</GoTwins>`)
	stdlibTwinsGroupOpen = regexp.MustCompile(`<PropertyGroup\s+Condition="([^"]*)"\s*>`)
	xmlComment           = regexp.MustCompile(`(?s)<!--.*?-->`)
	stdlibTwinsOnInText  = regexp.MustCompile(`(?i)GoStdlibTwins\s*[=:]\s*["']?true`)
	stdlibTwinsOnElement = regexp.MustCompile(`(?i)<GoStdlibTwins(\s[^>]*)?>\s*true\s*</GoStdlibTwins>`)
	// A workflow environment entry whose value is a GitHub expression that can yield 'true'.
	stdlibTwinsOnExpr = regexp.MustCompile(`(?i)GoStdlibTwins\s*:\s*\$\{\{([^}]*)\}\}`)
)

// The one tracked path allowed to turn GoStdlibTwins on: D2/T6 (FLEET 20261121T230700Z; COORD's GO 20261125T080536Z), the
// default-mode roster swept against a twinned standard library. It is os-matrix's sweep-shard, which never packs, and only
// this exact expression, which yields 'true' when the plan read ;twins=1. Any other expression for the name, anywhere, is
// refused: the value of an expression cannot be read from its text (P3, 2026-11-25).
const stdlibTwinsAllowedWorkflow = ".github/workflows/os-matrix.yml"
const stdlibTwinsAllowedExpr = "needs.plan.outputs.sweep-twins == '1' && 'true' || ''"

// turnsStdlibTwinsOn reports whether a tracked file's text turns GoStdlibTwins on: an MSBuild element that sets it
// true (comments stripped), or a script, workflow or command line that passes it true.
func turnsStdlibTwinsOn(rel, text string) bool {
	lower := strings.ToLower(rel)

	if strings.HasSuffix(lower, ".csproj") || strings.HasSuffix(lower, ".props") || strings.HasSuffix(lower, ".targets") {
		stripped := xmlComment.ReplaceAllString(text, "")
		return stdlibTwinsOnElement.MatchString(stripped) || stdlibTwinsOnInText.MatchString(stripped)
	}

	if stdlibTwinsOnInText.MatchString(text) {
		return true
	}

	for _, m := range stdlibTwinsOnExpr.FindAllStringSubmatch(text, -1) {
		if lower == stdlibTwinsAllowedWorkflow && strings.TrimSpace(m[1]) == stdlibTwinsAllowedExpr {
			continue
		}

		return true
	}

	return false
}

func TestStdlibTwinsDefaultOff(t *testing.T) {
	root := repoRootFromPackageDir(t)
	propsPath := filepath.Join(root, "src", "core", "Directory.Build.props")

	raw, err := os.ReadFile(propsPath)
	if err != nil {
		t.Fatalf("cannot read %s: %v", propsPath, err)
	}

	props := xmlComment.ReplaceAllString(string(raw), "")

	elements := stdlibTwinsElement.FindAllStringSubmatch(props, -1)
	if len(elements) != 1 {
		t.Fatalf("src/core/Directory.Build.props sets <GoStdlibTwins> %d time(s); want exactly 1, its default", len(elements))
	}

	if !strings.Contains(elements[0][1], "'$(GoStdlibTwins)' == ''") {
		t.Errorf("<GoStdlibTwins> in src/core/Directory.Build.props is not a default (Condition %q); it must apply only when the property is unset", elements[0][1])
	}

	if value := strings.TrimSpace(elements[0][2]); value != "false" {
		t.Errorf("GoStdlibTwins defaults to %q in src/core/Directory.Build.props; it stays false until row 10's T1-T5 ruling flips it", value)
	}

	// The one <GoTwins>true</GoTwins> there sits in a PropertyGroup conditioned on the switch being on.
	sets := goTwinsTrueElement.FindAllStringIndex(props, -1)
	if len(sets) != 1 {
		t.Fatalf("src/core/Directory.Build.props sets <GoTwins>true</GoTwins> %d time(s); want exactly 1, under GoStdlibTwins", len(sets))
	}

	opens := stdlibTwinsGroupOpen.FindAllStringSubmatchIndex(props[:sets[0][0]], -1)
	if len(opens) == 0 {
		t.Fatalf("<GoTwins>true</GoTwins> in src/core/Directory.Build.props is not inside a conditioned PropertyGroup")
	}

	last := opens[len(opens)-1]
	if condition := props[last[2]:last[3]]; !strings.Contains(condition, "'$(GoStdlibTwins)' == 'true'") {
		t.Errorf("<GoTwins>true</GoTwins> in src/core/Directory.Build.props is conditioned on %q, not on GoStdlibTwins being on", condition)
	}

	// No tracked build file, script or workflow turns it on.
	out, err := exec.Command("git", "-C", root, "ls-files", "-z").Output()
	if err != nil {
		t.Fatalf("git ls-files failed in %s: %v", root, err)
	}

	for _, rel := range strings.Split(string(out), "\x00") {
		lower := strings.ToLower(rel)

		if rel == "" || strings.HasPrefix(lower, "src/go2cs/internal/repoguard/") {
			continue
		}

		switch filepath.Ext(lower) {
		case ".csproj", ".props", ".targets", ".ps1", ".psm1", ".sh", ".bat", ".cmd", ".yml", ".yaml", ".json", ".rsp":
		default:
			continue
		}

		text, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))
		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		if turnsStdlibTwinsOn(rel, string(text)) {
			t.Errorf("%s turns GoStdlibTwins on; only an untracked gate or row 10 run may, until row 10's T1-T5 ruling flips the default", rel)
		}
	}
}

func TestStdlibTwinsScannerFires(t *testing.T) {
	cases := []struct {
		rel, text string
		want      bool
	}{
		{"x.props", "<GoStdlibTwins>true</GoStdlibTwins>", true},
		{"x.csproj", "<GoStdlibTwins Condition=\"'$(X)'=='1'\"> true </GoStdlibTwins>", true},
		{"x.props", "<!-- -p:GoStdlibTwins=true --><GoStdlibTwins>false</GoStdlibTwins>", false},
		{"push.ps1", "dotnet pack -p:GoStdlibTwins=true", true},
		{"ci.yml", "args: /p:GoStdlibTwins='true'", true},
		{"run.sh", "dotnet build -p:GoStdlibTwins=false", false},
		// D2/T6's one allowed path, and its near misses.
		{".github/workflows/os-matrix.yml", "GoStdlibTwins: ${{ needs.plan.outputs.sweep-twins == '1' && 'true' || '' }}", false},
		{".github/workflows/os-matrix.yml", "GoStdlibTwins: ${{ needs.plan.outputs.mn == '1' && 'true' || '' }}", true},
		{".github/workflows/ci-master.yml", "GoStdlibTwins: ${{ needs.plan.outputs.sweep-twins == '1' && 'true' || '' }}", true},
		{".github/workflows/os-matrix.yml", "GoStdlibTwins: ${{ 'true' }}", true},
		// P3's holes: the allowed condition beside an always-true arm, and an expression with no 'true' literal at all.
		{".github/workflows/os-matrix.yml", "GoStdlibTwins: ${{ needs.plan.outputs.sweep-twins == '1' || 'true' }}", true},
		{".github/workflows/ci-master.yml", "GoStdlibTwins: ${{ true }}", true},
		{".github/workflows/ci-master.yml", "GoStdlibTwins: ${{ inputs.twins }}", true},
	}

	for _, c := range cases {
		if got := turnsStdlibTwinsOn(c.rel, c.text); got != c.want {
			t.Errorf("turnsStdlibTwinsOn(%q, %q) = %v, want %v", c.rel, c.text, got, c.want)
		}
	}
}
