// localFuncTypeLift_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a func type declared in a function body. C# declares no delegate in a method body, so it is
// lifted to member level as every other local type kind is (liftLocalTypeDecl): declared once, named
// `<func>_<name>`, with its Go name in [GoLocalName] so reflection prints `main.aFunc`, and its doc
// comment above the declaration. Nothing of it stays in the body.

package main

import (
	"strings"
	"testing"
)

const localFuncTypeMarkerSource = `package main

func grouped() int {
	// leads the body.

	// aFunc is documented.
	type aFunc func(x int)

	// bFunc is documented.
	type bFunc func() int

	// after the types
	x := 1
	return x
}

func between() int {
	x := 2
	type cFunc func()
	y := x
	return y
}

func first() int {
	type dFunc func()

	return 3
}

func main() {
	println(grouped(), between(), first())
}
`

// liftedDeclarations are the four lifted delegates the source declares, as emitted.
var liftedDeclarations = []string{
	`[GoLocalName("aFunc")] internal delegate void grouped_aFunc(nint x);`,
	`[GoLocalName("bFunc")] internal delegate nint grouped_bFunc();`,
	`[GoLocalName("cFunc")] internal delegate void between_cFunc();`,
	`[GoLocalName("dFunc")] internal delegate void first_dFunc();`,
}

func checkLocalFuncTypesLifted(t *testing.T, lines []string) {
	t.Helper()

	for _, decl := range liftedDeclarations {
		count := 0

		for _, line := range lines {
			if line == decl {
				count++
			}
		}

		if count != 1 {
			t.Errorf("want the lifted declaration once at member level, got %d: %q\n%s", count, decl, strings.Join(lines, "\n"))
		}
	}

	// Nothing of a local func type stays in a method body (indented lines are body lines here).
	for _, line := range lines {
		if strings.HasPrefix(line, " ") && strings.Contains(line, "delegate") {
			t.Errorf("a delegate is declared in a method body: %q", line)
		}
	}
}

func TestLocalFuncTypeLiftedUnderComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, localFuncTypeMarkerSource)
	checkLocalFuncTypesLifted(t, lines)

	// A local type's doc comment travels with its lifted declaration.
	for i, doc := range []string{"// aFunc is documented.", "// bFunc is documented."} {
		at := lineContaining(t, lines, doc)

		if at+1 >= len(lines) || lines[at+1] != liftedDeclarations[i] {
			t.Errorf("want %q directly above %q", doc, liftedDeclarations[i])
		}
	}
}

func TestLocalFuncTypeLiftedWithoutComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	checkLocalFuncTypesLifted(t, convertWithoutComments(t, localFuncTypeMarkerSource))
}
