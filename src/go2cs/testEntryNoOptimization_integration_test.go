// testEntryNoOptimization_integration_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// testEntryNoOptimization_integration_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// A converted Go test function's frame is the one every testing.T report reads: t.Logf's own file:line, testify's
// callerInfo, a log.Lshortfile prefix written from the test. Under the Release harness's DOTNET_TieredCompilation=0
// the JIT optimized it, and an optimized frame lost the call's IL offset after a statement that keeps runtime.Caller's
// result live across a multi-line chain: runtime.Caller answered the function's first statement (testify's
// Test_Mock_Chained_On and Test_Mock_UnsetByOnMethodSpecAmongOthers). A converted test assembly is now compiled without
// optimization (go.lib.symbols.targets; COORD 2026-11-22 23:57Z, option 2, which replaced a per-method NoOptimization
// mark: a Release frame carries only its statement's IL offset even with that mark).
//
// The four tests read a test function's line four ways after that shape: an argument of Sprintf, an element of a
// slice literal, an argument of a variadic helper, and a log.Lshortfile prefix. Optimized, the first, second
// and fourth read the first statement's line on the converted side (Go pass, C# fail) and the third matched; unoptimized,
// all four match Go.

const testEntryLinesGo = `package lines

import "runtime"

type B struct{ Lines []int }

func (b *B) On() *B {
	_, _, l, _ := runtime.Caller(1)
	b.Lines = append(b.Lines, l)
	return b
}

func Here() int {
	_, _, l, _ := runtime.Caller(1)
	return l
}
`

const testEntryLinesTestGo = `package lines

import (
	"bytes"
	"fmt"
	"log"
	"runtime"
	"strconv"
	"strings"
	"testing"
)

func last(vs ...any) int { return vs[len(vs)-1].(int) }

func TestV1Sprintf(t *testing.T) {
	_, _, line, _ := runtime.Caller(0)
	b := &B{}
	b.
		On().
		On()
	s := fmt.Sprintf("%d %v %d", line, b.Lines, Here())
	if want := " " + strconv.Itoa(line+5); !strings.HasSuffix(s, want) {
		t.Errorf("got %q, want suffix %q", s, want)
	}
}

func TestV2Slice(t *testing.T) {
	_, _, line, _ := runtime.Caller(0)
	b := &B{}
	b.
		On().
		On()
	vs := []any{line, b.Lines, Here()}
	if got, want := vs[2].(int), line+5; got != want {
		t.Errorf("got %d, want %d", got, want)
	}
}

func TestV3Variadic(t *testing.T) {
	_, _, line, _ := runtime.Caller(0)
	b := &B{}
	b.
		On().
		On()
	if got, want := last(line, b.Lines, Here()), line+5; got != want {
		t.Errorf("got %d, want %d", got, want)
	}
}

func TestV4LogPrefix(t *testing.T) {
	_, _, line, _ := runtime.Caller(0)
	b := &B{}
	b.
		On().
		On()
	var buf bytes.Buffer
	log.New(&buf, "", log.Lshortfile).Print(line, b.Lines)
	if want := "lines_test.go:" + strconv.Itoa(line+6) + ": "; !strings.HasPrefix(buf.String(), want) {
		t.Errorf("got %q, want prefix %q", buf.String(), want)
	}
}
`

func TestTestsRecurseKeepsATestFunctionsLinesUnderReleaseTC0(t *testing.T) {
	if os.Getenv(testsE2EEnv) != "1" {
		t.Skipf("opt-in (%s=1): builds and runs the converted tests against the converted standard library", testsE2EEnv)
	}

	if _, err := exec.LookPath("dotnet"); err != nil {
		t.Fatalf("%s=1 needs the .NET SDK on PATH: %v", testsE2EEnv, err)
	}

	root := t.TempDir()
	moduleDir := filepath.Join(root, "lines")
	writeModuleFile(t, filepath.Join(moduleDir, "go.mod"), "module example.test/lines\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(moduleDir, "lines.go"), testEntryLinesGo)
	writeModuleFile(t, filepath.Join(moduleDir, "lines_test.go"), testEntryLinesTestGo)

	output, err := runTestsRecurseAction(t, "all", moduleDir, filepath.Join(root, "out"))

	if err != nil {
		t.Fatalf("-tests -test-action all failed; a test function's line differs from Go's: %v\n%s", err, output)
	}

	if !strings.Contains(output, "Validated 4 tests against go test") {
		t.Fatalf("want all four tests validated against go test\n%s", output)
	}
}
