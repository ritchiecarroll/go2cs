// behavioralHarnessOptions_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"testing"
)

// A behavioral project may carry go2cs.options (extra converter arguments; only -warn=<codes>), and FOUR instruments
// convert behavioral projects. If one of them stopped passing the file, it would convert a -warn project without -warn,
// and the instruments would disagree about how the project converts, which is false-green route #2's shape. This guard
// finds every converter invocation in the four and fails by name when one does not pass the project's options.
type harnessCallSite struct {
	path       string         // repository-relative
	invocation *regexp.Regexp // a line that invokes the converter
	options    string         // the token that line must carry: the project's options
	reader     string         // the file must call the options reader
}

var harnessCallSites = []harnessCallSite{
	{"src/tests/Behavioral/BehavioralRunner/Program.cs", regexp.MustCompile(`Exec\(s_go2csExe,`), `{options}-go2cspath`, `BehavioralPackages.ConverterOptions(`},
	{"src/tests/Behavioral/BehavioralTests/BehavioralTestBase.cs", regexp.MustCompile(`Exec\(go2cs,`), `{options}-go2cspath`, `BehavioralPackages.ConverterOptions(`},
	{"src/utilities/UpdateTestTargets/Program.cs", regexp.MustCompile(`Run\(go2csExe,`), `{options}-go2cspath`, `BehavioralPackages.ConverterOptions(`},
	{"src/tests/Behavioral/check-no-regression.ps1", regexp.MustCompile(`& \$go2csExe`), `@converterOptions -go2cspath`, `Get-ConverterOptions`},
}

// harnessOptionsViolations lists, one per problem, each call site whose converter invocation does not pass the project's
// go2cs.options. contents maps each call site's path to its text.
func harnessOptionsViolations(contents map[string]string) []string {
	var violations []string

	for _, site := range harnessCallSites {
		text, ok := contents[site.path]

		if !ok {
			violations = append(violations, site.path+": missing, so its converter call cannot be checked")
			continue
		}

		if !strings.Contains(text, site.reader) {
			violations = append(violations, site.path+": does not read go2cs.options (no "+site.reader+")")
		}

		invocations := 0

		for i, line := range strings.Split(text, "\n") {
			if !site.invocation.MatchString(line) || !strings.Contains(line, "-go2cspath") {
				continue
			}

			invocations++

			if !strings.Contains(line, site.options) {
				violations = append(violations, site.path+":"+strconv.Itoa(i+1)+": the converter call does not pass the project's go2cs.options (want "+site.options+")")
			}
		}

		if invocations == 0 {
			violations = append(violations, site.path+": no converter call found, so the guard is not looking at it")
		}
	}

	return violations
}

func readHarnessCallSites(t *testing.T) map[string]string {
	t.Helper()

	root := repoRootFromPackageDir(t)
	contents := map[string]string{}

	for _, site := range harnessCallSites {
		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(site.path)))

		if err != nil {
			t.Fatalf("%s: %v", site.path, err)
		}

		contents[site.path] = strings.ReplaceAll(string(data), "\r\n", "\n")
	}

	return contents
}

// TestBehavioralHarnessReadsConverterOptions: every converter invocation in the four instruments passes the project's
// go2cs.options.
func TestBehavioralHarnessReadsConverterOptions(t *testing.T) {
	for _, violation := range harnessOptionsViolations(readHarnessCallSites(t)) {
		t.Error(violation)
	}
}

// TestBehavioralHarnessOptionsGuardFiresBySite: with ONE call site's options token removed, the guard names exactly that
// call site and no other (safety floor 13: a guard never seen to fail proves nothing).
func TestBehavioralHarnessOptionsGuardFiresBySite(t *testing.T) {
	for _, planted := range harnessCallSites {
		contents := readHarnessCallSites(t)
		contents[planted.path] = strings.Replace(contents[planted.path], planted.options, strings.Replace(planted.options, "ptions", "pts", 1), 1)

		violations := harnessOptionsViolations(contents)

		if len(violations) != 1 || !strings.HasPrefix(violations[0], planted.path+":") {
			t.Errorf("with %s's options token removed the guard reported %v; want exactly one violation naming %s", planted.path, violations, planted.path)
		}
	}
}
