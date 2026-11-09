// cliPaths_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards docs/CommandLine.md's options table against `go2cs -h`. The page says it lists every option the converter
// accepts; it is checked here rather than kept by hand, because a hand-kept list drifted: eight flags `go2cs -h` prints
// were missing from it (docs/phase4/AUDIT-user-facing-docs.md). The flag set is read from the real -h output, so a
// flag added to main.go fails this test until the page names it, and a flag the page names that -h does not print
// fails it too.

package main

import (
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// helpFlags returns the flag names `go2cs -h` prints, one per "  -name" line of its option list.
func helpFlags(t *testing.T) map[string]bool {
	t.Helper()

	flags := map[string]bool{}
	entry := regexp.MustCompile(`^  -([a-z][a-z0-9-]*)`)

	for _, line := range strings.Split(strings.ReplaceAll(helpText(t), "\r\n", "\n"), "\n") {
		if m := entry.FindStringSubmatch(line); m != nil {
			flags[m[1]] = true
		}
	}

	if len(flags) == 0 {
		t.Fatal("found no flags in go2cs -h")
	}

	return flags
}

// docFlags returns the flag names the first column of CommandLine.md's "## Options" table names: every code span in
// that cell that starts with a dash (`-recurse=module`, `-goroot` / `-gopath`, a struck-through `-cgo`).
func docFlags(t *testing.T) map[string]bool {
	t.Helper()

	data, err := os.ReadFile(filepath.Join("..", "..", "docs", "CommandLine.md"))

	if err != nil {
		t.Fatal(err)
	}

	flags := map[string]bool{}
	span := regexp.MustCompile("`-([a-z][a-z0-9-]*)")
	inOptions := false

	for _, line := range strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n") {
		if strings.HasPrefix(line, "## ") {
			inOptions = line == "## Options"
			continue
		}

		if !inOptions || !strings.HasPrefix(line, "| ") {
			continue
		}

		cells := strings.SplitN(line, "|", 3)

		if len(cells) < 3 {
			continue
		}

		for _, m := range span.FindAllStringSubmatch(cells[1], -1) {
			flags[m[1]] = true
		}
	}

	if len(flags) == 0 {
		t.Fatal("found no options table in docs/CommandLine.md")
	}

	return flags
}

func TestCommandLineDocListsEveryFlag(t *testing.T) {
	help, doc := helpFlags(t), docFlags(t)
	var missing, unknown []string

	for name := range help {
		if !doc[name] {
			missing = append(missing, "-"+name)
		}
	}

	for name := range doc {
		if !help[name] {
			unknown = append(unknown, "-"+name)
		}
	}

	sort.Strings(missing)
	sort.Strings(unknown)

	if len(missing) > 0 {
		t.Errorf("docs/CommandLine.md's options table omits flags go2cs -h prints: %s", strings.Join(missing, ", "))
	}

	if len(unknown) > 0 {
		t.Errorf("docs/CommandLine.md's options table names flags go2cs -h does not print: %s", strings.Join(unknown, ", "))
	}
}
