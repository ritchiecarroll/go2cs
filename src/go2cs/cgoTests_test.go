// cgoTests_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// cgo Phase 3, increment 3.4, seat 4a (docs/phase4/DESIGN-cgo-phase3-marshaling.md section 19): -cgo with -tests, for
// ONE package, into an output outside the repository's corpus. The package's production load, its test loads and the
// oracle run cgo-on; every other package stays the corpus's cgo-off build.

import (
	"encoding/json"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

const cgoTestsFixture = `package cgot4

/*
#include <stdlib.h>
*/
import "C"

// Abs is C's abs, through the cgo route.
func Abs(x int) int { return int(C.abs(C.int(x))) }
`

const cgoTestsFixtureTest = `package cgot4

import "testing"

func TestAbsCrossesToC(t *testing.T) {
	if got := Abs(-3); got != 3 {
		t.Errorf("Abs(-3) = %d, want 3", got)
	}
}

func TestAbsOfZero(t *testing.T) {
	if got := Abs(0); got != 0 {
		t.Errorf("Abs(0) = %d, want 0", got)
	}
}
`

// A package whose production file calls C and whose _test.go calls that function converts with its tests, builds its
// test host and matches Go's verdicts. Before seat 4a, checkCgoFlag refused the pair ("-cgo cannot be combined with
// -tests"), so this row read red with that text.
func TestACgoPackageConvertsWithItsTestsAndMatchesGo(t *testing.T) {
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to convert, build and compare (Inconclusive, not a pass)", cgoBuildEnv)
	}

	if _, err := exec.LookPath("dotnet"); err != nil {
		t.Skip("NOT RUN: no dotnet on PATH (Inconclusive, not a pass)")
	}

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	module := t.TempDir()
	writeCgoFixture(t, module, "go.mod", "module example.com/cgot4\n\ngo 1.26\n")
	writeCgoFixture(t, module, "cgot4.go", cgoTestsFixture)
	writeCgoFixture(t, module, "cgot4_test.go", cgoTestsFixtureTest)

	output := filepath.Join(t.TempDir(), "cgot4")
	args := []string{"-go2cspath", treeSrc, "-cgo", "-tests", "-test-action", "all", "-test-timeout", "10m", module, output}

	cmd := exec.Command(driverBinary(t), args...)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1")
	combined, err := cmd.CombinedOutput()

	if err != nil {
		t.Fatalf("go2cs -cgo -tests failed: %v\n%s", err, lastLines(string(combined), 40))
	}

	data, err := os.ReadFile(filepath.Join(output, "go2cs_test_comparison.json"))

	if err != nil {
		t.Fatalf("no comparison record: %v\n%s", err, lastLines(string(combined), 40))
	}

	var record testComparison

	if err := json.Unmarshal(data, &record); err != nil {
		t.Fatalf("comparison record: %v", err)
	}

	t.Logf("READING: status %q matched %v go %v csharp %v errors %v", record.Status, record.Matched, record.Go, record.CSharp, record.Errors)

	for _, name := range []string{"TestAbsCrossesToC", "TestAbsOfZero"} {
		if record.Go[name] != "pass" || record.CSharp[name] != "pass" {
			t.Errorf("%s: go %q, go2cs %q, want pass on both sides", name, record.Go[name], record.CSharp[name])
		}
	}

	if !record.Matched {
		t.Errorf("the comparison did not match: %s", strings.Join(record.Errors, "; "))
	}
}
