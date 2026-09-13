// tokenDoorCensusGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os/exec"
	"strings"
	"testing"
)

// THE GUARD FOR src/token-door-census.sh.
//
// That census answers ONE question over src/core/syscall/windows: which wrappers hand the syscall
// trampoline a `ж<T>` whose T is reference-bearing, and therefore a TAGGED ORDER TOKEN rather than
// an address -- the class refuseManagedPointerTokens (dll_windows.cs:134) refuses by name. Seven at
// a02ac3df3. Derivation and the dated timeline: docs/phase4/CENSUS-token-door-live-wrappers.md.
//
// ⚠ WHAT THIS ASSERTS, AND WHAT IT DELIBERATELY DOES NOT.
//
// It asserts the census's CONTROLS and its PARSE COVERAGE. It does NOT assert the member count.
// Seven is a defect population: every hand-own displacing one of those wrappers SHOULD drive it
// down, and a guard pinned to 7 would go red on exactly the commits that fix the problem -- turning
// the instrument into an obstacle to its own finding. The invariant worth holding is that the census
// can still SEE, not that the corpus has not moved.
//
// The controls run against the REAL corpus on every invocation rather than over a fixture, which is
// the stronger arrangement here: a fixture can carry the right shape and the wrong facts (the H5
// applier's precondition arc, same day). Two of them were earned rather than designed --
//
//   - the first cut anchored field detection on /;$/, which cannot match a CRLF line ending ";\r",
//     and the census reported ZERO reference-bearing structs. A clean, confident, wrong answer that
//     only the StartupInfo control separated from a real finding.
//   - the denominator gate asserted `parsed > 200`, a number with nothing behind it, and refused a
//     parse that was COMPLETE. It now compares against an independently counted population.
func TestTokenDoorCensusControls(t *testing.T) {
	// A POSIX spelling, not filepath.Join: the argument is read by bash, not by Windows.
	script := "../token-door-census.sh"

	bash, unmeasured := safePushBash()
	if bash == "" {
		t.Skip(unmeasured)
	}
	t.Logf("driving src/token-door-census.sh through %s", bash)

	out, err := exec.Command(bash, script).CombinedOutput()
	text := string(out)

	if err != nil {
		t.Fatalf("src/token-door-census.sh refused on the real corpus: %v\n%s", err, text)
	}

	for _, control := range []string{
		// The type predicate can still tell a reference-bearing struct from a blittable one.
		"controls OK: StartupInfo + Timezoneinformation flagged; Timeval + Timespec not",
		// The wrapper scan can still find a known member and still rejects a known non-member.
		"controls OK: getStartupInfo present; GetStdHandle absent",
	} {
		if !strings.Contains(text, control) {
			t.Fatalf("src/token-door-census.sh did not report the control %q -- a census whose controls stop running reports a number nobody can stand behind:\n%s", control, text)
		}
	}

	// PARSE COVERAGE. "127 of 127" is the shape, not the number: the census refuses outright when
	// the two disagree, so what this adds is that the line is PRESENT and self-consistent -- a run
	// that silently stopped emitting it would otherwise satisfy every assertion above.
	const marker = "== functions parsed in zsyscall_windows.cs: "
	i := strings.Index(text, marker)
	if i < 0 {
		t.Fatalf("src/token-door-census.sh printed no parse-coverage line -- the denominator that separates \"few members\" from \"blind\" is gone:\n%s", text)
	}
	line := text[i+len(marker):]
	if j := strings.IndexByte(line, '\n'); j >= 0 {
		line = line[:j]
	}
	parts := strings.Fields(line) // "<n> of <total> bodied signatures"
	if len(parts) < 3 || parts[1] != "of" || parts[0] != parts[2] {
		t.Fatalf("parse coverage is not total (%q) -- the census is blind to some of the file, and under-reporting is the dangerous direction for a census whose finding is a count", line)
	}
}
