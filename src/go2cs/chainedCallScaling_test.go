// chainedCallScaling_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards issue #33: converting a CHAINED method call must cost time linear in the chain length.
//
// A fluent chain `b.Add(…).Add(…).Add(…)` nests left, so each link's callee IS the whole rest of
// the chain. convCallExpr classified its arguments in a `for i := range params.Len()` loop that
// converted callExpr.Fun on every iteration — purely to test whether the callee text spelled
// `print`/`println` — and then converted it once more for real. A call with p parameters therefore
// walked its callee p+1 times, and on a chain that compounds to (p+1)^N in the chain length N.
//
// go.mongodb.org/mongo-driver/bson/bsoncodec registers its default codecs as a 42-link chain over
// the 2-parameter RegisterTypeEncoder, so converting that one package needed ~3^42 callee walks. A
// -recurse run over the reporter's project sat on it at [1440/1726] for over half an hour, CPU
// pegged and heap flat — the shape of exhaustive re-work, not of a deadlock or a leak.
//
// The name is now read from the AST (callFunIsUniversePrint), which is O(1) and asks the same
// question, because an unshadowed universe built-in is a bare identifier whose name IS the
// built-in's name.

package main

import (
	"fmt"
	"go/build"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
	"time"
)

// chainedCallLinks is the fixture's chain length. Over a 2-parameter method the pre-fix converter
// needed 3^40 (~1.2e19) callee walks to convert it, so any budget at all separates the two; the
// fixed converter walks each link once and the whole test is dominated by the go/packages load.
const chainedCallLinks = 40

// chainedCallHelperEnv marks the re-executed child that performs the conversion. The conversion runs
// in a CHILD PROCESS because it cannot be cancelled: on a regression it spins indefinitely, and a
// goroutine left spinning in the test binary keeps the whole package's `go test` alive until the
// harness kills it minutes later. Killing a child is immediate, so a reintroduced exponential fails
// this test in `budget` and the suite moves on.
const chainedCallHelperEnv = "GO2CS_CHAINED_CALL_HELPER"

// TestChainedCallConversionIsNotExponential converts a long fluent chain under a deadline and then
// checks the emitted C# still contains every link.
//
// Both halves matter. The deadline alone would pass vacuously if the converter ever started
// dropping the chain (a file abandoned by the per-file recover converts instantly), and the content
// check alone would still pass — eventually — with the exponential in place.
func TestChainedCallConversionIsNotExponential(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	// The child re-enters this same test function; it converts and exits rather than asserting.
	if dir := os.Getenv(chainedCallHelperEnv); dir != "" {
		chainedCallHelperMain(dir)
		return
	}

	root := t.TempDir()
	pkgDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")

	// Two parameters, matching RegisterTypeEncoder: the per-parameter loop is what multiplied the
	// callee walks, so a 1-parameter method would understate the base (2^N rather than 3^N).
	var chain strings.Builder

	chain.WriteString(`package main

import "fmt"

type Builder struct {
	n int
}

func (b *Builder) Add(key string, value int) *Builder {
	b.n += value
	return b
}

func main() {
	b := &Builder{}

	b`)

	for i := range chainedCallLinks {
		fmt.Fprintf(&chain, ".Add(\"k%d\", %d)", i, i)
	}

	chain.WriteString(`

	fmt.Println(b.n)
}
`)

	writeModuleFile(t, filepath.Join(pkgDir, "main.go"), chain.String())

	// The budget is deliberately loose. It is not measuring converter speed — it separates "linear"
	// from "astronomically superlinear", and a machine under load (the behavioral corpus running in
	// a sibling worktree) still converts this in well under a second of actual conversion work.
	const budget = 90 * time.Second

	cmd := exec.Command(os.Args[0], "-test.run=^TestChainedCallConversionIsNotExponential$", "-test.timeout=0")
	cmd.Env = append(os.Environ(), chainedCallHelperEnv+"="+pkgDir)

	output, err := runWithinBudget(t, cmd, budget)

	if err != nil {
		t.Fatalf("converting a %d-link chained call: %v — the callee is being re-converted per "+
			"parameter again (issue #33)\n%s", chainedCallLinks, err, output)
	}

	mainCs := readGenerated(t, filepath.Join(pkgDir, "out", "main.cs"))

	// Every link survived into the emitted C#, so the deadline above was met by converting the
	// chain rather than by losing it.
	if got := strings.Count(mainCs, ".Add("); got < chainedCallLinks {
		t.Errorf("converted main.cs kept %d of %d chained calls:\n%s", got, chainedCallLinks, mainCs)
	}
}

// runWithinBudget runs cmd and kills it if it outlives budget, reporting the timeout as the error.
// The message stays generic — each scaling guard names its own defect in the Fatalf that wraps it,
// because both this guard and the argument-path one (nestedArgScaling_test.go) share this helper.
func runWithinBudget(t *testing.T, cmd *exec.Cmd, budget time.Duration) (string, error) {
	t.Helper()

	var combined strings.Builder

	cmd.Stdout = &combined
	cmd.Stderr = &combined

	if err := cmd.Start(); err != nil {
		return combined.String(), fmt.Errorf("start helper: %w", err)
	}

	done := make(chan error, 1)

	go func() {
		done <- cmd.Wait()
	}()

	select {
	case err := <-done:
		return combined.String(), err

	case <-time.After(budget):
		_ = cmd.Process.Kill()
		<-done

		return combined.String(), fmt.Errorf("did not finish within %s (killed)", budget)
	}
}

// chainedCallHelperMain converts the fixture package written by the parent and exits. It reports
// failure through the exit code, so the parent's diagnostic stays about the chain rather than about
// test plumbing.
func chainedCallHelperMain(pkgDir string) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(pkgDir, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := processConversion(pkgDir, true, filepath.Join(pkgDir, "out"), options); err != nil {
		fmt.Fprintf(os.Stderr, "helper conversion failed: %v\n", err)
		os.Exit(1)
	}

	os.Exit(0)
}
