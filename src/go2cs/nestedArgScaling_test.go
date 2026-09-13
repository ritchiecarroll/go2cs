// nestedArgScaling_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the ARGUMENT-path sibling of issue #33: converting NESTED calls must cost time linear in
// the nesting depth.
//
// After assembling a call's text, convCallExpr re-walked every argument through
// checkForImplicitConversion purely for its recording side effect — a second full conversion of the
// argument subtree whose result was discarded. That is a flat 2× tax on every call in the corpus,
// and on `f(f(f(…)))`, where each argument IS the next call, it compounds to 2^depth: depth 18 took
// 1.22s and depth 22 took 13.66s against a 0.55s go/packages floor — the conversion component
// doubles exactly per level — while depth 26 was killed at 416s of wall, unfinished.
//
// The recording is entirely type-driven, so the two halves are now separate: applyImplicitConversion
// records from funcType/argType alone and the discard-the-result loop calls it directly, converting
// nothing. The same rule the callee-path fix established, one path over — never pay a whole subtree
// walk for something the type system already answers.

package main

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

// nestedArgDepth is the fixture's nesting depth. Pre-fix this needed 2^30 argument walks — the
// measured curve puts it near three quarters of an hour — while the fixed converter walks each
// call once and the whole test is dominated by the go/packages load.
const nestedArgDepth = 30

// nestedArgHelperEnv marks the re-executed child that performs the conversion, for the same reason
// the chained-call guard uses one: the conversion cannot be cancelled, so on a regression an
// in-process run would keep `go test` alive until the harness killed it. The child re-enters this
// test, converts through chainedCallHelperMain (the plumbing is shared with that guard) and exits.
const nestedArgHelperEnv = "GO2CS_NESTED_ARG_HELPER"

// TestNestedArgumentConversionIsNotExponential converts deeply nested calls under a deadline and
// then checks every nesting level survived into the emitted C#.
//
// Both halves matter, exactly as in the chained-call guard: the deadline alone would pass vacuously
// if the converter ever started dropping the expression, and the content check alone would still
// pass — eventually — with the exponential in place.
func TestNestedArgumentConversionIsNotExponential(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	if dir := os.Getenv(nestedArgHelperEnv); dir != "" {
		chainedCallHelperMain(dir)
		return
	}

	root := t.TempDir()
	pkgDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")

	// A ONE-parameter function is the worst case here and the simplest: the tax was per ARGUMENT
	// subtree, so the base is 2 no matter how many parameters the callee takes.
	nested := "1"

	for range nestedArgDepth {
		nested = "f(" + nested + ")"
	}

	writeModuleFile(t, filepath.Join(pkgDir, "main.go"), fmt.Sprintf(`package main

func f(x int) int { return x + 1 }

func main() {
	y := %s
	println(y)
}
`, nested))

	// Loose on purpose — it separates "linear" from "astronomically superlinear", not fast from
	// slow. A machine under load (a sibling worktree running the behavioral corpus) still converts
	// this in a couple of seconds, essentially all of it the package load.
	const budget = 90 * time.Second

	cmd := exec.Command(os.Args[0], "-test.run=^TestNestedArgumentConversionIsNotExponential$", "-test.timeout=0")
	cmd.Env = append(os.Environ(), nestedArgHelperEnv+"="+pkgDir)

	output, err := runWithinBudget(t, cmd, budget)

	if err != nil {
		t.Fatalf("converting a %d-deep nested call: %v — each argument subtree is being converted "+
			"twice again (the argument-path exponential)\n%s", nestedArgDepth, err, output)
	}

	mainCs := readGenerated(t, filepath.Join(pkgDir, "out", "main.cs"))

	// Every nesting level survived into the emitted C#, so the deadline was met by converting the
	// expression rather than by losing it.
	if got := strings.Count(mainCs, "f("); got < nestedArgDepth {
		t.Errorf("converted main.cs kept %d of %d nested calls:\n%s", got, nestedArgDepth, mainCs)
	}
}
