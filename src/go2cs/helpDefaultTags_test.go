// cliPaths_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the help text's description of the standard-library default build tags. `-stdlib` and `-tests` apply every
// tag in defaultStdLibBuildTags when no -tags is given (resolveBuildTags), and docs/CommandLine.md says so. The help
// text named `purego` alone after `math_big_pure_go` joined the default, so `go2cs -h` contradicted both the code and
// the page. The help now reads the slice, so the two cannot drift again; this test reads the real -h output.

package main

import (
	"os"
	"os/exec"
	"strings"
	"testing"
)

// helpText runs `go2cs -h` through the CLI helper process and returns what it printed. -h exits non-zero by design,
// so the exit status is not checked; the usage text is.
func helpText(t *testing.T) string {
	t.Helper()

	cmd := exec.Command(os.Args[0], "-test.run=^TestCLIPathsHelperProcess$")
	cmd.Env = append(os.Environ(), cliPathsHelperEnv+`=["-h"]`)
	output, _ := cmd.CombinedOutput()
	text := string(output)

	if !strings.Contains(text, "Usage: go2cs") {
		t.Fatalf("go2cs -h printed no usage text:\n%s", text)
	}

	return text
}

// flagHelp returns the help entry for -name: its line and the indented description lines under it.
func flagHelp(t *testing.T, text, name string) string {
	t.Helper()

	lines := strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n")

	for i, line := range lines {
		trimmed := strings.TrimSpace(line)

		if trimmed != "-"+name && !strings.HasPrefix(trimmed, "-"+name+" ") {
			continue
		}

		entry := []string{line}

		for _, next := range lines[i+1:] {
			if !strings.HasPrefix(next, "    \t") {
				break
			}

			entry = append(entry, next)
		}

		return strings.Join(entry, "\n")
	}

	t.Fatalf("go2cs -h has no entry for -%s", name)
	return ""
}

func TestHelpNamesEveryDefaultStdLibBuildTag(t *testing.T) {
	text := helpText(t)

	for _, name := range []string{"stdlib", "tests", "tags"} {
		entry := flagHelp(t, text, name)

		for _, tag := range defaultStdLibBuildTags {
			if !strings.Contains(entry, tag) {
				t.Errorf("go2cs -h's -%s entry does not name default tag %q:\n%s", name, tag, entry)
			}
		}
	}
}
