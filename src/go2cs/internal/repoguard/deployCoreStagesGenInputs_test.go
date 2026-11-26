// deployCoreStagesGenInputs_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"path"
	"regexp"
	"strings"
	"testing"
)

// deploy-core.ps1 stages a runtime root that a converted program builds against: src/core at <root>/core,
// src/gen/go2cs-gen at <root>/gen/go2cs-gen, and version.props at <root>. go2cs-gen.csproj also reaches files
// OUTSIDE its own folder by relative path (`..\..\X`), and each of those resolves against the deployed layout
// too. A file it reaches that deploy-core does not stage breaks every deployed build: go2cs-gen has embedded
// ..\..\go2cs\fieldAccessCollisions.txt since 810b9bb208 (2026-10-17), deploy-core never staged src/go2cs, and
// the deployed go2cs-gen failed CS1566, so nothing built in a deployed root for five weeks (measured
// 2026-11-25, R, while measuring the twin opt-in; COORD 20261125T191726Z). No CI step runs deploy-core.
//
// The guard: every `..\..\` path in go2cs-gen.csproj is either under core/ (deploy-core copies src/core whole)
// or named in deploy-core.ps1 itself. A path the script does not name, outside core/, fails by name.
var genOutsideInclude = regexp.MustCompile(`(?:Include|Project)="\.\.[\\/]\.\.[\\/]([^"]+)"`)

func TestDeployCoreStagesEveryGenInput(t *testing.T) {
	csproj := readRepoFile(t, "src/gen/go2cs-gen/go2cs-gen.csproj")
	script := strings.ReplaceAll(readRepoFile(t, "src/deploy-core.ps1"), `\`, "/")

	matches := genOutsideInclude.FindAllStringSubmatch(csproj, -1)

	if len(matches) == 0 {
		t.Fatal("go2cs-gen.csproj reaches no ..\\..\\ path: the guard's pattern no longer matches the project, so it reads nothing")
	}

	sawCore, sawOther := false, false

	for _, m := range matches {
		rel := path.Clean(strings.ReplaceAll(m[1], `\`, "/"))

		if strings.HasPrefix(rel, "core/") {
			sawCore = true
			continue
		}

		sawOther = true

		if !strings.Contains(script, "'"+rel+"'") {
			t.Errorf("go2cs-gen.csproj reaches ..\\..\\%s, which deploy-core.ps1 does not stage: a deployed go2cs-gen cannot build (stage it at <root>/%s, as version.props is)", rel, rel)
		}
	}

	// The two shapes the guard distinguishes must both be live, or one arm is reading nothing.
	if !sawCore || !sawOther {
		t.Fatalf("expected both a core/ path and a non-core path in go2cs-gen.csproj's ..\\..\\ includes (core=%v other=%v)", sawCore, sawOther)
	}
}
