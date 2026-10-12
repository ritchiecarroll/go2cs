// nugetgoTestdataEol_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os/exec"
	"strings"
	"testing"
)

// THE NUGETGO FIXTURES CHECK OUT AS COMMITTED ON EVERY HOST (TRAIN T4 rehearsal-1, COORD 2026-10-12).
//
// Test-NugetgoReadme.ps1 compares generated text with the fixtures under src/tools/nugetgo/testdata byte for byte.
// With no attribute, a Windows clone (core.autocrlf=true) checks them out with CRLF while a Linux clone checks them
// out with LF, so the same suite read 112 of 115 on the i9 and 115 of 115 here: the three conversion-readme cases
// failed on the line endings alone. Reproduced on Linux with `git -c core.autocrlf=true worktree add`: hashset-cs.md,
// jwt-cs.md and uuid-cs.md checked out CRLF, and the suite printed "ran 115, failed 3". The pin is -text, so every
// checkout holds the committed bytes, which is what a Linux checkout already holds and what the suites are green on.
func TestNugetgoTestdataIsNotLineEndingConverted(t *testing.T) {
	root := repoRootFromPackageDir(t)

	files, err := exec.Command("git", "-C", root, "ls-files", "-z", "--", "src/tools/nugetgo/testdata").Output()
	if err != nil {
		t.Fatalf("git ls-files failed in %s: %v", root, err)
	}

	paths := strings.FieldsFunc(string(files), func(r rune) bool { return r == 0 })
	if len(paths) == 0 {
		t.Fatal("no tracked file under src/tools/nugetgo/testdata: the tree moved")
	}

	args := append([]string{"-C", root, "check-attr", "-z", "text", "eol", "--"}, paths...)
	out, err := exec.Command("git", args...).Output()
	if err != nil {
		t.Fatalf("git check-attr failed in %s: %v", root, err)
	}

	// -z output is path NUL attribute NUL value NUL, one triple per path and attribute.
	fields := strings.Split(strings.TrimSuffix(string(out), "\x00"), "\x00")
	text := map[string]string{}
	eol := map[string]string{}

	for i := 0; i+2 < len(fields); i += 3 {
		switch fields[i+1] {
		case "text":
			text[fields[i]] = fields[i+2]
		case "eol":
			eol[fields[i]] = fields[i+2]
		}
	}

	var converted []string

	for _, path := range paths {
		if text[path] != "unset" && eol[path] != "lf" {
			converted = append(converted, path+" (text: "+text[path]+", eol: "+eol[path]+")")
		}
	}

	if len(converted) > 0 {
		t.Fatalf("%d of %d fixture(s) under src/tools/nugetgo/testdata can check out with CRLF on a Windows clone "+
			"(core.autocrlf=true), and the suites compare them byte for byte. Pin them in .gitattributes "+
			"(src/tools/nugetgo/testdata/** -text):\n  %s", len(converted), len(paths), strings.Join(converted, "\n  "))
	}
}
