// localTypeLift_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the three function-local type-lift rules reflect's own suite was the first thing in the
// corpus to reach. All three are PRODUCTION-path rules, which is why this fixture is an ordinary
// module conversion rather than a `-tests` variant — the shapes appear in reflect's EXTERNAL test
// package, where the white-box bridge flag is never set, and the same emission serves both.
//
//  1. A defined type whose underlying is a FOREIGN (cross-package) NAMED type — `type MyBuffer
//     bytes.Buffer` inside a func — was the ONE local type-declaration kind that did not hoist. C#
//     forbids a type declaration in a method body, and reflect's single site in set_test.go's
//     TestImplicitMapConversion produced 73 parse diagnostics: the whole file, and the whole
//     package's suite behind it.
//
//  2. A lifted local type is `internal`, in every conversion. Its Go name carries no export
//     meaning — Go's convention governs PACKAGE-LEVEL identifiers — and the hoisted
//     `<Func>_<name>` identifier the name-based rules actually read begins with the ENCLOSING
//     FUNCTION's first letter. reflect's `type BigP *big` in TestExported and the anonymous `s`
//     struct in BenchmarkIsZero both read `public` off a capital that means nothing, over
//     operands that are internal: CS0050/51/52/53/56/57 across the members go2cs-gen generates.
//
//  3. The lift RENAME reaches an EMBEDDED interface base. reflect's TestMethodPkgPath declares
//     `type I interface{…}` and then `type i interface{ I; … }`; both hoist, but the embed
//     rendered the bare Go name, which exists nowhere afterwards (CS0246).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestFunctionLocalTypeLifts converts one production module carrying every local-declaration shape
// reflect exercises and pins all three rules off the single emission.
func TestFunctionLocalTypeLifts(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/localtypelift\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"time"
)

// A package-level UNEXPORTED type: the operand a public lift would out-rank.
type big struct{ A int }

type impl struct{}

func (impl) x() int { return 1 }
func (impl) y() int { return 2 }

// Exported is deliberately CAPITALIZED: every lift below is named Exported_<local>, so a
// name-derived accessibility reads "public" for all of them.
func Exported() {
	// Rule 1 — a FOREIGN named underlying. Both underlying kinds route through the same
	// emission branch (a struct here, a basic below), so both must hoist.
	type myTime time.Time
	type myDur time.Duration

	// Rule 2 — a defined POINTER over the package-level unexported "big", and a slice whose
	// ANONYMOUS element lifts alongside it (the element's visit consumes the pending modifier,
	// which is what used to leave the slice bare).
	type BigP *big
	type s []struct{ C int }

	// Rule 3 — a local interface EMBEDDED in another local interface.
	type I interface{ x() int }
	type i interface {
		I
		y() int
	}

	var v i = impl{}
	var d, d2 myDur
	var p BigP

	fmt.Println(new(myTime) != nil, d == d2, p == nil, len(s{}), v.x()+v.y())
}

func main() { Exported() }
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "localtypelift", "main.cs"))

	// Rule 1 — the foreign-underlying declarations exist AND sit ahead of the method that declares
	// them. Position is the whole point: the defect emitted a syntactically identical declaration
	// INSIDE the method body, which C# rejects outright.
	methodIndex := strings.Index(mainCs, "void Exported()")

	if methodIndex < 0 {
		t.Fatalf("the converted method was not found at all:\n%s", mainCs)
	}

	for _, decl := range []string{
		"partial struct Exported_myTime",
		"partial struct Exported_myDur",
	} {
		index := strings.Index(mainCs, decl)

		if index < 0 {
			t.Errorf("a local type over a FOREIGN named type must be declared; want %q:\n%s", decl, mainCs)
			continue
		}

		if index > methodIndex {
			t.Errorf("%q must hoist AHEAD of its method — C# forbids a type declaration in a method body:\n%s", decl, mainCs)
		}
	}

	// Rule 2 — every lift of this function is internal, whatever the case of anything in its name.
	for _, decl := range []string{
		"internal partial struct Exported_myTime",
		"internal partial class Exported_BigP",
		"internal partial struct Exported_s",
	} {
		if !strings.Contains(mainCs, decl) {
			t.Errorf("a function-local type has no Go exportedness to read a modifier from; want %q:\n%s", decl, mainCs)
		}
	}

	// The two negatives that make the positives mean something: nothing local is public, and
	// nothing local is left BARE (a bare declaration hands the decision to go2cs-gen, which scopes
	// it from the hoisted name and lands back on public).
	for _, line := range strings.Split(mainCs, "\n") {
		trimmed := strings.TrimSpace(line)

		if !strings.Contains(trimmed, "Exported_") || !strings.Contains(trimmed, "partial ") {
			continue
		}

		if strings.Contains(trimmed, "public partial ") {
			t.Errorf("no function-local type may be public — its hoisted name's case belongs to the enclosing function: %q", trimmed)
		}

		if strings.HasPrefix(trimmed, "[GoType") && strings.Contains(trimmed, "] partial ") {
			t.Errorf("a bare local declaration lets the generator scope it from the hoisted name: %q", trimmed)
		}
	}

	// Rule 3 — the embedded base names the LIFT, not the bare Go name that no longer exists.
	embedIndex := strings.Index(mainCs, "partial interface Exported_i")

	if embedIndex < 0 {
		t.Fatalf("the local interface that embeds another was not emitted:\n%s", mainCs)
	}

	declaration := mainCs[embedIndex:]

	if end := strings.Index(declaration, "{"); end > 0 {
		declaration = declaration[:end]
	}

	if !strings.Contains(declaration, "Exported_I") {
		t.Errorf("an embedded LOCAL interface must name its lifted declaration, got %q:\n%s", strings.TrimSpace(declaration), mainCs)
	}
}
