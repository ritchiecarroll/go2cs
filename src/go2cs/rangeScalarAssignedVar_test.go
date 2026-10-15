// rangeScalarAssignedVar_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the mutable-copy escape on the SCALAR range forms: a range over an integer and a range over
// a channel. Go lets the body write the iteration variable (it is a per-iteration copy), but a C#
// foreach iteration variable is read-only, so a write to it is CS1656. The slice/array/map, string
// and range-over-func arms iterate a temp and declare the variable from it in the body when
// rangeVarNeedsMutableCopy says so (rangeOverFuncAssignedVar_test.go pins the func arms); the int
// and chan arms ignored that decision. No corpus site has the shape today; the gap was found beside
// the range-over-func fix.
//
// Controls: a variable the body only READS keeps the bare `foreach (var x in …)` with no temp.
package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertScalarAssignedVarFixture converts the one fixture the test reads and returns its emitted C#.
func convertScalarAssignedVarFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/scalarassigned\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

// INT, WRITTEN - the integer range variable is incremented in the body.
func skipOdd(n int) int {
	sum := 0

	for i := range n {
		if i%2 == 1 {
			i++
		}
		sum += i
	}

	return sum
}

// INT, READ ONLY - the control.
func sumTo(n int) int {
	sum := 0

	for i := range n {
		sum += i
	}

	return sum
}

// CHAN, WRITTEN - the received value is scaled in the body.
func doubled(ch chan int) int {
	sum := 0

	for v := range ch {
		v *= 2
		sum += v
	}

	return sum
}

// CHAN, READ ONLY - the control.
func drained(ch chan int) int {
	sum := 0

	for v := range ch {
		sum += v
	}

	return sum
}

func main() {
	ch := make(chan int)
	close(ch)
	println(skipOdd(4), sumTo(4), doubled(ch), drained(ch))
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "scalarassigned", "main.cs"))
}

// TestScalarRangeCopiesAnAssignedIterationVariable pins the escape on both scalar arms and its scope.
func TestScalarRangeCopiesAnAssignedIterationVariable(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertScalarAssignedVarFixture(t)
	temp := "i" + TempVarMarker + "1"

	// INT - the foreach iterates a temp and the body declares the writable variable from it.
	skip := emittedFuncBody(t, mainCs, "skipOdd")

	if strings.Contains(skip, "foreach (var i in range(") || !strings.Contains(skip, "foreach (var "+temp+" in range(") || !strings.Contains(skip, "var i = "+temp+";") {
		t.Errorf("skipOdd: expected the foreach to iterate %s and the body to declare `var i = %s;` (CS1656 otherwise):\n%s", temp, temp, skip)
	}

	sum := emittedFuncBody(t, mainCs, "sumTo")

	if !strings.Contains(sum, "foreach (var i in range(") || strings.Contains(sum, TempVarMarker) {
		t.Errorf("sumTo: a read-only int range variable must keep the bare foreach:\n%s", sum)
	}

	// CHAN - the same escape.
	dbl := emittedFuncBody(t, mainCs, "doubled")

	if strings.Contains(dbl, "foreach (var v in ") || !strings.Contains(dbl, "foreach (var "+temp+" in ") || !strings.Contains(dbl, "var v = "+temp+";") {
		t.Errorf("doubled: expected the foreach to iterate %s and the body to declare `var v = %s;` (CS1656 otherwise):\n%s", temp, temp, dbl)
	}

	drain := emittedFuncBody(t, mainCs, "drained")

	if !strings.Contains(drain, "foreach (var v in ") || strings.Contains(drain, TempVarMarker) {
		t.Errorf("drained: a read-only chan range variable must keep the bare foreach:\n%s", drain)
	}
}
