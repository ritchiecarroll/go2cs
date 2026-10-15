// rangeOverFuncAssignedVar_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the mutable-copy escape on a RANGE-OVER-FUNC loop. Go lets the body write the yielded
// variable (it is a per-iteration copy), but a C# foreach iteration variable is read-only, so a
// write to it is CS1656. The slice/array/map and string arms already iterate a temp and declare the
// variable from it in the body when rangeVarNeedsMutableCopy says so; the yield arms ignored that
// decision. Go 1.26's go/build/constraint parsePlusBuildExpr is the shape that surfaced it:
//
//	for lit := range strings.SplitSeq(clause, ",") {
//		if strings.HasPrefix(lit, "!") { lit = lit[len("!"):] }
//
// Controls: a yielded variable the body only READS keeps the bare `foreach (var x in range(seq))`
// with no temp, and in a two-value range each position is decided on its own.
package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertAssignedYieldFixture converts the one fixture the test reads and returns its emitted C#.
func convertAssignedYieldFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/assignedyield\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"strings"
)

// SUBJECT - the go/build/constraint shape: the single yielded value is reassigned in the body.
func trimBangs(clause string) []string {
	var out []string

	for lit := range strings.SplitSeq(clause, ",") {
		if strings.HasPrefix(lit, "!") {
			lit = lit[len("!"):]
		}

		out = append(out, lit)
	}

	return out
}

// CONTROL - the same loop reading the yielded value only keeps the bare foreach.
func countLits(clause string) int {
	n := 0

	for lit := range strings.SplitSeq(clause, ",") {
		n += len(lit)
	}

	return n
}

// TWO-VALUE, KEY written - only the key takes the temp.
func bumpKeys(seq func(yield func(int, string) bool)) int {
	total := 0

	for k, v := range seq {
		k++
		total += k + len(v)
	}

	return total
}

// TWO-VALUE, VALUE written - only the value takes the temp.
func trimValues(seq func(yield func(int, string) bool)) []string {
	var out []string

	for k, v := range seq {
		v += "!"
		out = append(out, fmt.Sprint(k, v))
	}

	return out
}

func main() {
	fmt.Println(trimBangs("a,!b"), countLits("a,b"), bumpKeys(nil), trimValues(nil))
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "assignedyield", "main.cs"))
}

// TestRangeOverFuncCopiesAnAssignedYieldedVariable pins the escape and its scope.
func TestRangeOverFuncCopiesAnAssignedYieldedVariable(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertAssignedYieldFixture(t)
	keyTemp := "i" + TempVarMarker + "1"
	valTemp := "v" + TempVarMarker + "1"

	// SUBJECT - the foreach iterates a temp and the body declares the writable variable from it.
	trim := emittedFuncBody(t, mainCs, "trimBangs")

	if strings.Contains(trim, "foreach (var lit in range") {
		t.Errorf("trimBangs: the foreach still declares the written variable `lit` (CS1656):\n%s", trim)
	}

	if !strings.Contains(trim, "foreach (var "+keyTemp+" in range") || !strings.Contains(trim, "var lit = "+keyTemp+";") {
		t.Errorf("trimBangs: expected the foreach to iterate %s and the body to declare `var lit = %s;`:\n%s", keyTemp, keyTemp, trim)
	}

	// CONTROL - a read-only yielded variable takes no temp.
	count := emittedFuncBody(t, mainCs, "countLits")

	if !strings.Contains(count, "foreach (var lit in range") || strings.Contains(count, TempVarMarker) {
		t.Errorf("countLits: a read-only yielded variable must keep the bare foreach:\n%s", count)
	}

	// TWO-VALUE - only the written position takes a temp.
	bump := emittedFuncBody(t, mainCs, "bumpKeys")

	if !strings.Contains(bump, "foreach (var ("+keyTemp+", v) in range") || !strings.Contains(bump, "var k = "+keyTemp+";") {
		t.Errorf("bumpKeys: expected only the key to iterate %s:\n%s", keyTemp, bump)
	}

	values := emittedFuncBody(t, mainCs, "trimValues")

	if !strings.Contains(values, "foreach (var (k, "+valTemp+") in range") || !strings.Contains(values, "var v = "+valTemp+";") {
		t.Errorf("trimValues: expected only the value to iterate %s:\n%s", valTemp, values)
	}
}
