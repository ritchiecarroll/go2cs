// annotatorSelfTest_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// THE ANNOTATOR SELF-TEST GUARD.
//
// A re-bank's annotators (annotate_linux.py, annotate_darwin.py) write the roster's per-OS segments, and their
// self-test (test_annotate_evidence.py, beside them under docs/phase4/data/<re-bank>/) checks the evidence directory
// each one reads, against a fixture tree built from this tree's roster guard. Until this guard nothing ran it: the
// 1.26.10.2 bank seat changed the guard's declared $DarwinEvidenceDirectory, the self-test's darwin default case went
// red on that commit, and it stayed red through two green gates (BOARD, 2026-11-03). This runs every such self-test
// under the plain `go test ./...` each lane already runs. A self-test's exit code is its number of failed cases, so a
// non-zero exit fails the guard, which names the self-test and quotes its failed cases.
//
// The self-tests are found by glob, so a later re-bank's copy is covered the day it lands. They need a Python 3; a
// host with none on PATH skips, and says so by name.
func TestAnnotatorSelfTestsPass(t *testing.T) {
	python, prefix := findPython3()
	if python == "" {
		t.Skip("no Python 3 on PATH (python3, py -3 or python): the annotator self-tests did NOT run on this host")
	}

	selfTests, err := filepath.Glob(filepath.Join(repoRootFromPackageDir(t), "docs", "phase4", "data", "*", "test_annotate*.py"))
	if err != nil {
		t.Fatal(err)
	}

	if len(selfTests) == 0 {
		t.Fatal("no docs/phase4/data/*/test_annotate*.py self-test found: the glob or the tree moved")
	}

	failedCase := regexp.MustCompile(`(?m)^\s*FAIL\b.*$`)

	for _, selfTest := range selfTests {
		name := filepath.Base(filepath.Dir(selfTest)) + "/" + filepath.Base(selfTest)

		t.Run(strings.TrimSuffix(name, ".py"), func(t *testing.T) {
			arguments := append(append([]string{}, prefix...), "-I", selfTest)
			output, err := exec.Command(python, arguments...).CombinedOutput()

			if err != nil {
				failures := failedCase.FindAllString(string(output), -1)
				lastLines := strings.Split(strings.TrimRight(string(output), "\n"), "\n")

				if len(lastLines) > 3 {
					lastLines = lastLines[len(lastLines)-3:]
				}

				interpreter := strings.TrimSpace(python + " " + strings.Join(prefix, " "))
				t.Errorf("%s failed (%v) under %s.\nFailed cases:\n%s\nLast lines:\n%s", name, err, interpreter,
					strings.Join(failures, "\n"), strings.Join(lastLines, "\n"))
			}
		})
	}
}

// findPython3 returns a Python 3 interpreter on PATH and the arguments that select it: python3, then the Windows
// launcher `py -3`, then python. A `python` that is Python 2 is not one.
func findPython3() (string, []string) {
	for _, candidate := range []struct {
		name   string
		prefix []string
	}{{"python3", nil}, {"py", []string{"-3"}}, {"python", nil}} {
		path, err := exec.LookPath(candidate.name)
		if err != nil {
			continue
		}

		arguments := append(append([]string{}, candidate.prefix...), "-c", "import sys; print(sys.version_info[0])")

		if output, err := exec.Command(path, arguments...).Output(); err == nil && strings.TrimSpace(string(output)) == "3" {
			return path, candidate.prefix
		}
	}

	return "", nil
}
