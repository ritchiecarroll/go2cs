// analyzerPublishAot_test.go - Gbtc
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

// A converted program is published for Native AOT with `dotnet publish -r <rid> -p:PublishAot=true`
// (docs/Tuning.md). That global property reaches every project the program references, including the
// analyzer projects under src/gen through their OutputItemType="Analyzer" references. They target
// netstandard2.0, where the SDK refuses PublishAot with NETSDK1207 and the whole publish stops: measured
// 2026-11-16 on linux-x64 (go2cs-gen for a strings/fmt hello, then go2cs-arrays-gen for the README
// walkthrough's colordemo) and on win-x64 (both). A project-file value cannot override a global property
// unless the project names it in TreatAsLocalProperty, so every netstandard project under src/gen declares
// TreatAsLocalProperty="PublishAot" and sets PublishAot false. An analyzer is loaded by the compiler and
// never published, so false is its only correct value.
var (
	netstandardTarget   = regexp.MustCompile(`<TargetFrameworks?>[^<]*netstandard[^<]*</TargetFrameworks?>`)
	projectElement      = regexp.MustCompile(`<Project\s[^>]*>`)
	treatAsLocalAttr    = regexp.MustCompile(`\bTreatAsLocalProperty="([^"]*)"`)
	publishAotFalseElem = regexp.MustCompile(`<PublishAot>\s*false\s*</PublishAot>`)
)

func TestAnalyzerProjectsKeepPublishAotOff(t *testing.T) {
	root := repoRootFromPackageDir(t)

	out, err := exec.Command("git", "-C", root, "ls-files", "--", "src/gen/*.csproj").Output()
	if err != nil {
		t.Fatalf("git ls-files src/gen: %v", err)
	}

	checked := 0

	for _, rel := range strings.Fields(string(out)) {
		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))
		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		text := string(data)

		if !netstandardTarget.MatchString(text) {
			continue
		}

		checked++

		local := false

		if head := projectElement.FindString(text); head != "" {
			if m := treatAsLocalAttr.FindStringSubmatch(head); m != nil {
				for _, name := range strings.Split(m[1], ";") {
					if strings.TrimSpace(name) == "PublishAot" {
						local = true
					}
				}
			}
		}

		if !local {
			t.Errorf("%s targets netstandard but its <Project> element does not name PublishAot in "+
				"TreatAsLocalProperty: a program's -p:PublishAot=true publish stops on it with NETSDK1207", rel)
		}

		if !publishAotFalseElem.MatchString(text) {
			t.Errorf("%s targets netstandard but does not set <PublishAot>false</PublishAot>", rel)
		}
	}

	// Two analyzer projects exist today (go2cs-gen and go2cs-arrays-gen). Fewer means the enumeration broke,
	// and a guard that checks nothing passes.
	if checked < 2 {
		t.Fatalf("checked %d netstandard projects under src/gen; expected at least 2 (go2cs-gen, go2cs-arrays-gen)", checked)
	}
}
