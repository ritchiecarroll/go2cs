// commandLineHelp_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the converter's help against its flag set. The help and docs/CommandLine.md are written from one table
// (internal/clihelp/options.go), and clihelp's own tests hold the page to it; these hold the table to the flags main.go
// defines, by running the real binary through the CLI helper process.

package main

import (
	"encoding/json"
	"os"
	"os/exec"
	"strings"
	"testing"

	"go2cs/internal/clihelp"
)

// runCLIStatus runs the converter with args and returns its stdout, its stderr and its exit code.
func runCLIStatus(t *testing.T, args ...string) (stdout, stderr string, code int) {
	t.Helper()

	cmd := exec.Command(os.Args[0], "-test.run=^TestCLIPathsHelperProcess$")
	encoded, err := json.Marshal(args)

	if err != nil {
		t.Fatal(err)
	}

	cmd.Env = append(os.Environ(), cliPathsHelperEnv+"="+string(encoded))

	var out, errOut strings.Builder
	cmd.Stdout, cmd.Stderr = &out, &errOut
	err = cmd.Run()

	if exit, ok := err.(*exec.ExitError); ok {
		return out.String(), errOut.String(), exit.ExitCode()
	} else if err != nil {
		t.Fatal(err)
	}

	return out.String(), errOut.String(), 0
}

// Every flag main.go defines has a table entry and every entry a flag: main panics at start-up otherwise, so the help
// would not print. -help prints the common options on stdout and exits 0, as -h and --help do.
func TestHelpPrintsTheCommonOptions(t *testing.T) {
	for _, form := range []string{"-help", "-h", "--help"} {
		stdout, stderr, code := runCLIStatus(t, form)

		if code != 0 || !strings.Contains(stdout, "Common options:") || stderr != "" {
			t.Fatalf("go2cs %s: exit %d\nstdout:\n%s\nstderr:\n%s", form, code, stdout, stderr)
		}

		for _, option := range clihelp.Options() {
			listed := strings.Contains(stdout, "\n  -"+option.Name+" ") || strings.Contains(stdout, "\n  -"+option.Name+"[") ||
				strings.Contains(stdout, "\n  -"+option.Name+"\n")

			if listed != (option.Class == clihelp.Common) {
				t.Errorf("go2cs %s lists -%s: %v, but it is a %s option", form, option.Name, listed, option.Class)
			}
		}
	}
}

// -help all lists every option.
func TestHelpAllListsEveryOption(t *testing.T) {
	stdout, _, code := runCLIStatus(t, "-help", "all")

	if code != 0 {
		t.Fatalf("go2cs -help all: exit %d", code)
	}

	for _, option := range clihelp.Options() {
		if !strings.Contains(stdout, "  -"+option.Name) {
			t.Errorf("go2cs -help all does not list -%s", option.Name)
		}
	}
}

// -help <option> prints that option's entry, with the flag's default, in either spelling of the topic.
func TestHelpExplainsOneOption(t *testing.T) {
	for _, args := range [][]string{{"-help", "indent"}, {"-help=indent"}, {"-help=-indent"}, {"-help", "-indent"}} {
		stdout, _, code := runCLIStatus(t, args...)

		if code != 0 || !strings.HasPrefix(stdout, "-indent <n>\n") || !strings.Contains(stdout, "Default: 4") {
			t.Fatalf("go2cs %s: exit %d\n%s", strings.Join(args, " "), code, stdout)
		}
	}
}

// A help topic that names no option, or an option the flag set does not define, fails with the nearest names.
func TestHelpSuggestsTheNearestOption(t *testing.T) {
	for _, args := range [][]string{{"-help=test-actoin"}, {"-test-actoin", "all", "pkg"}} {
		_, stderr, code := runCLIStatus(t, args...)

		if code != 1 || !strings.Contains(stderr, "there is no option -test-actoin. Did you mean -test-action?") {
			t.Fatalf("go2cs %s: exit %d\n%s", strings.Join(args, " "), code, stderr)
		}
	}
}

// The help of -stdlib, -tests and -tags names every tag defaultStdLibBuildTags applies, so the help cannot contradict
// the code. The table is fixed text, so a tag added to the default fails here until the table names it.
func TestHelpNamesEveryDefaultStdLibBuildTag(t *testing.T) {
	for _, name := range []string{"stdlib", "tests", "tags"} {
		stdout, _, code := runCLIStatus(t, "-help", name)

		if code != 0 {
			t.Fatalf("go2cs -help %s: exit %d", name, code)
		}

		for _, tag := range defaultStdLibBuildTags {
			if !strings.Contains(stdout, tag) {
				t.Errorf("go2cs -help %s does not name default tag %q:\n%s", name, tag, stdout)
			}
		}
	}
}

// The -module-safe-tag help names every tag defaultModuleBuildTags applies.
func TestHelpNamesEveryDefaultModuleBuildTag(t *testing.T) {
	stdout, _, _ := runCLIStatus(t, "-help", "module-safe-tag")

	for _, tag := range defaultModuleBuildTags {
		if !strings.Contains(stdout, tag) {
			t.Errorf("go2cs -help module-safe-tag does not name default tag %q:\n%s", tag, stdout)
		}
	}
}

func TestHelpRequest(t *testing.T) {
	cases := []struct {
		args  []string
		topic string
		ok    bool
	}{
		{nil, "", false},
		{[]string{"pkg", "out"}, "", false},
		{[]string{"-h"}, "", true},
		{[]string{"--help"}, "", true},
		{[]string{"-help", "all"}, "all", true},
		{[]string{"-help=tests"}, "tests", true},
		{[]string{"-recurse", "-help", "-tests"}, "-tests", true},
		{[]string{"-h", "pkg_dir", "out"}, "", true},
		{[]string{"-help", "tests"}, "tests", true},
		{[]string{"-help", "ALL"}, "ALL", true},
		{[]string{"--", "-help"}, "", false},
	}

	for _, c := range cases {
		if topic, ok := helpRequest(c.args); topic != c.topic || ok != c.ok {
			t.Errorf("helpRequest(%q) = %q, %v; want %q, %v", c.args, topic, ok, c.topic, c.ok)
		}
	}
}

// The -warn help names every code -warn may downgrade, so the table cannot fall behind warnAllowedCodes.
func TestHelpNamesEveryWarnAllowedCode(t *testing.T) {
	stdout, _, code := runCLIStatus(t, "-help", "warn")

	if code != 0 {
		t.Fatalf("go2cs -help warn: exit %d", code)
	}

	for _, allowed := range warnAllowedCodeList() {
		if !strings.Contains(stdout, allowed) {
			t.Errorf("go2cs -help warn does not name %s, which -warn may downgrade:\n%s", allowed, stdout)
		}
	}
}
