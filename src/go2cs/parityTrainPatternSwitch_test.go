// parityTrainPatternSwitch_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// Seat #28 of the visual-parity train (docs/PLAN-parity-train.md), S1: a tagged switch whose labels are not all C#
// constants, and which has no `fallthrough`, emits as a C# pattern switch over the tag rather than an if/else chain
// over an `exprᴛN` temp (the owner ruled every such switch, §6.4; COORD ruled the spelling, R1-R3):
//
//   - one label per Go clause, its labels joined with `||` in Go's order: `case var ᴛ when ᴛ == A || ᴛ == B:`;
//   - each label keeps the chain's operator: a clause the chain tested with `is` keeps its constant pattern
//     (`case 3 or 4:`), an interface tag keeps AreEqual;
//   - a pattern switch nested in another's clause names its variable `ᴛ2`;
//   - `default` stays where Go writes it (C# tries it last, as Go does), and a Go `break` or `continue` in a clause
//     is emitted bare: no `do { … } while (false)` wrapper, no `goto continueᴛN`.
//
// A switch WITH fallthrough keeps the chain.

const patternSwitchFixture = `package main

import "fmt"

type kind int

const (
	small kind = iota
	large
)

func label(n int) int { return n }

func classify(t int, y int) string {
	switch t {
	default:
		return "other"
	case 3, 4:
		return "three-four"
	case y:
		return "y"
	}
}

func loop(xs []int, y int) int {
	n := 0
	for _, x := range xs {
		switch x {
		case label(1):
			if n > 0 {
				break
			}
			n++
		case y:
			continue
		}
		n += 10
	}
	return n
}

func nested(a any, k kind, y kind) string {
	switch a {
	case any(1):
		switch k {
		case y:
			return "y"
		}
		return "one"
	}
	return "none"
}

func withFallthrough(t int, y int) int {
	n := 0
	switch t {
	case y:
		n++
		fallthrough
	case 2:
		n += 10
	}
	return n
}

func main() {
	fmt.Println(classify(3, 9), classify(9, 9), classify(5, 9))
	fmt.Println(loop([]int{1, 1, 2, 3}, 2))
	fmt.Println(nested(1, large, large), nested(1, small, large), nested(2, small, small))
	fmt.Println(withFallthrough(7, 7), withFallthrough(2, 7))
}
`

func convertPatternSwitchFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/patswitch\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), patternSwitchFixture)

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

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "patswitch", "main.cs"))
}

func TestPatternSwitchEmission(t *testing.T) {
	isolateConverterPathGlobals(t)
	mainCs := convertPatternSwitchFixture(t)

	// Each want is one whole emitted line, trimmed.
	wants := map[string][]string{
		"classify": {`switch (t) {`, `default: {`, `case 3 or 4: {`, `case var ᴛ when ᴛ == y: {`},
		"loop":     {`switch (x) {`, `case var ᴛ when ᴛ == label(1): {`, `continue;`},
		"nested":   {`switch (a) {`, `switch (k) {`, `case var ᴛ2 when ᴛ2 == y: {`},
		"fallthrough keeps the chain": {`var exprᴛ1 = t;`, `fallthrough = true;`},
	}

	mainCs = strings.ReplaceAll(mainCs, "\r\n", "\n")
	lines := make(map[string]bool)

	for _, line := range strings.Split(mainCs, "\n") {
		lines[strings.TrimSpace(line)] = true
	}

	for fn, fragments := range wants {
		for _, want := range fragments {
			if !lines[want] {
				t.Errorf("%s: no emitted line reads %q", fn, want)
			}
		}
	}

	if !regexp.MustCompile(`(?m)^\s*case var ᴛ when AreEqual\(ᴛ, .*\): \{$`).MatchString(mainCs) {
		t.Errorf("nested: the interface tag's clause does not keep AreEqual under `case var ᴛ when`")
	}

	refusals := map[string]*regexp.Regexp{
		"the do/while(false) wrapper":        regexp.MustCompile(`\bdo \{|\} while \(false\);`),
		"a goto to the loop's continue label": regexp.MustCompile(`goto continueᴛ\d+;`),
		"the chain's default block":           regexp.MustCompile(`/\* default: \*/`),
		"a second tag temp":                   regexp.MustCompile(`exprᴛ[2-9]`),
	}

	for shape, refusal := range refusals {
		if found := refusal.FindString(mainCs); found != "" {
			t.Errorf("%s is still emitted: %q", shape, found)
		}
	}

	if got := strings.Count(mainCs, "var exprᴛ"); got != 1 {
		t.Errorf("wanted the one tag temp of the fallthrough switch, read %d", got)
	}

	if t.Failed() {
		t.Logf("emitted main.cs:\n%s", mainCs)
	}
}
