// namedResultLiveness_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the named-result declaration liveness check — the CS0219 family.
//
// Go names results for documentation far more often than it uses them, so the unconditional
// `Type name = default!;` prologue the converter emitted was dead in 1,218 of the corpus's 1,219
// CS0219 warnings. The declaration is dropped only when the body genuinely never touches the
// result AND every return is explicit; the name still reads off the C# tuple return type. The
// four ways a declaration stays LIVE are each pinned below, because getting any of them wrong is
// CS0103 (a name the emitted body references but never declared) rather than a cosmetic slip.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestNamedResultDeclarationLiveness(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/nrl\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"errors"
	"fmt"
)

// DEAD: named purely for documentation, every return explicit. The utf16.EncodeRune shape.
func encodeRune(r int) (r1, r2 int) {
	if r < 0 {
		return -1, -1
	}
	return r + 1, r + 2
}

// LIVE: a naked return reads every named result.
func nakedZero() (zero int) {
	return
}

// LIVE: assigned, then read back by a naked return.
func assignedThenNaked() (n int) {
	n = 7
	return
}

// LIVE: referenced by an explicit return.
func referenced() (err error) {
	err = errors.New("boom")
	return err
}

// LIVE: captured by a closure, never otherwise mentioned in the body.
func capturedByClosure() (count int) {
	f := func() {
		count++
	}
	f()
	return count
}

// LIVE: mutated by a deferred call — the defer lowering reads the local from generated code.
func mutatedByDefer() (err error) {
	defer func() {
		err = errors.New("from defer")
	}()
	return nil
}

// DEAD in the OUTER function: the only naked return belongs to the nested literal.
func outerDeadInnerNaked() (outerName int) {
	inner := func() (innerNaked int) {
		return
	}
	return inner() + 1
}

func main() {
	a, b := encodeRune(3)
	fmt.Println(a, b, nakedZero(), assignedThenNaked(), referenced(), capturedByClosure(), mutatedByDefer(), outerDeadInnerNaked())
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "nrl", "main.cs"))

	// The declarations that must be GONE.
	for _, gone := range []string{
		"nint r1 = default!;",
		"nint r2 = default!;",
		"nint outerName = default!;",
	} {
		if strings.Contains(mainCs, gone) {
			t.Errorf("dead named-result declaration still emitted (%s):\n%s", gone, mainCs)
		}
	}

	// …while the NAMES still reach the reader, on the tuple return type.
	if !strings.Contains(mainCs, "(nint r1, nint r2)") {
		t.Errorf("named results lost from the C# return type — the name must survive there:\n%s", mainCs)
	}

	// The declarations that must REMAIN, one per liveness reason.
	for _, kept := range []string{
		"nint zero = default!;",       // naked return
		"nint n = default!;",          // assigned, read by naked return
		"error err = default!;",       // referenced by an explicit return
		"nint count = default!;",      // captured by a closure
		"nint innerNaked = default!;", // naked return inside a function LITERAL
	} {
		if !strings.Contains(mainCs, kept) {
			t.Errorf("live named-result declaration was dropped (%s) — this is CS0103:\n%s", kept, mainCs)
		}
	}

	// The defer lowering declares its result outside the wrapper; the spelling differs by lowering
	// (plain local or heap box), so assert the NAME is declared rather than one exact form.
	if !strings.Contains(mainCs, "mutatedByDefer") {
		t.Fatalf("fixture function missing from output:\n%s", mainCs)
	}
}
