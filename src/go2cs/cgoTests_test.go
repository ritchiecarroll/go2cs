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

// The fixture calls strtod, not abs: gcc compiles abs as a builtin, so cmd/cgo's binary imports no symbol for it and the
// verified-names rule refuses it by name (read on the hosted leg 45488939174). strtod is the p3 fixture's, proven on
// linux, darwin and windows.
const cgoTestsFixture = `package cgot4

/*
#include <stdlib.h>
*/
import "C"

import "unsafe"

// Parse is C's strtod over a C copy of s, through the cgo route.
func Parse(s string) float64 {
	cs := C.CString(s)
	defer C.free(unsafe.Pointer(cs))

	return float64(C.strtod(cs, nil))
}
`

const cgoTestsFixtureTest = `package cgot4

import "testing"

func TestParseCrossesToC(t *testing.T) {
	if got := Parse("1.5abc"); got != 1.5 {
		t.Errorf("Parse(\"1.5abc\") = %v, want 1.5", got)
	}
}

func TestParseOfNothing(t *testing.T) {
	if got := Parse("abc"); got != 0 {
		t.Errorf("Parse(\"abc\") = %v, want 0", got)
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

	for _, name := range []string{"TestParseCrossesToC", "TestParseOfNothing"} {
		if record.Go[name] != "pass" || record.CSharp[name] != "pass" {
			t.Errorf("%s: go %q, go2cs %q, want pass on both sides", name, record.Go[name], record.CSharp[name])
		}
	}

	if !record.Matched {
		t.Errorf("the comparison did not match: %s", strings.Join(record.Errors, "; "))
	}
}

const cgoSwapStub = `// The fixture's stub, selected when cgo is off.

//go:build !cgo

package cgot5

// Name names the build that selected this file.
func Name() string { return "stub" }
`

const cgoSwapCgo = `// The fixture's cgo file, selected when cgo is on.

//go:build cgo

package cgot5

/*
#include <stdlib.h>
*/
import "C"

import "unsafe"

// Name names the build that selected this file, through C.
func Name() string {
	cs := C.CString("2")
	defer C.free(unsafe.Pointer(cs))

	return "cgo" + string(rune('0'+int(C.strtod(cs, nil))))
}
`

const cgoSwapTest = `package cgot5

import "testing"

func TestNameIsTheCgoFile(t *testing.T) {
	if got := Name(); got != "cgo2" {
		t.Errorf("Name() = %q, want %q", got, "cgo2")
	}
}
`

// Seat 4c's build row (COORD's condition 4, inbox H 20261124T231602Z): an output that already holds the package's
// cgo-OFF emission, as a root seeded from the corpus does, then -cgo -tests into it. The cgo-on load swaps the stub for
// the cgo file, and both define Name. Without the removal, the test host's build fails CS0111 on Name (net's first
// build read the same class six times); with it, the stub's C# goes, the host builds, and the test matches Go.
func TestACgoTestsConversionOverACgoOffEmissionRemovesTheStub(t *testing.T) {
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
	writeCgoFixture(t, module, "go.mod", "module example.com/cgot5\n\ngo 1.26\n")
	writeCgoFixture(t, module, "name_stub.go", cgoSwapStub)
	writeCgoFixture(t, module, "name_cgo.go", cgoSwapCgo)
	writeCgoFixture(t, module, "name_test.go", cgoSwapTest)

	output := filepath.Join(t.TempDir(), "cgot5")
	stub := filepath.Join(output, "name_stub.cs")

	// The seed: the package's cgo-off emission, with -comments as the corpus is emitted.
	seed := exec.Command(driverBinary(t), "-go2cspath", treeSrc, "-comments", module, output)
	seed.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=0")

	if out, err := seed.CombinedOutput(); err != nil {
		t.Fatalf("the cgo-off seed conversion failed: %v\n%s", err, lastLines(string(out), 30))
	}

	if _, err := os.Stat(stub); err != nil {
		t.Fatalf("premise: the cgo-off emission holds %s: %v", stub, err)
	}

	cmd := exec.Command(driverBinary(t), "-go2cspath", treeSrc, "-cgo", "-tests", "-test-action", "all", "-test-timeout", "10m", module, output)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1")
	combined, err := cmd.CombinedOutput()

	for _, line := range strings.Split(string(combined), "\n") {
		if strings.HasPrefix(line, "cgo: removed ") || strings.Contains(line, "error CS") {
			t.Logf("READING: %s", strings.TrimSpace(line))
		}
	}

	if err != nil {
		t.Fatalf("go2cs -cgo -tests over the cgo-off emission failed: %v\n%s", err, lastLines(string(combined), 40))
	}

	if _, err := os.Stat(stub); err == nil {
		t.Errorf("%s is still there: the cgo-on conversion did not remove the stub it deselects", stub)
	}

	data, err := os.ReadFile(filepath.Join(output, "go2cs_test_comparison.json"))

	if err != nil {
		t.Fatalf("no comparison record: %v", err)
	}

	var record testComparison

	if err := json.Unmarshal(data, &record); err != nil {
		t.Fatalf("comparison record: %v", err)
	}

	t.Logf("READING: status %q matched %v go %v csharp %v", record.Status, record.Matched, record.Go, record.CSharp)

	if record.Go["TestNameIsTheCgoFile"] != "pass" || record.CSharp["TestNameIsTheCgoFile"] != "pass" || !record.Matched {
		t.Errorf("want TestNameIsTheCgoFile passing on both sides, matched; got go %q, go2cs %q, matched %v", record.Go["TestNameIsTheCgoFile"], record.CSharp["TestNameIsTheCgoFile"], record.Matched)
	}
}
