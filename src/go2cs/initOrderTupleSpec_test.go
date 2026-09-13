// initOrderTupleSpec_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the tuple-spec arm of the init-order relocation (docs/phase4/FINDING-init-order-tuple-specs.md):
// a package-level `var a, b = f()` whose initializer depends on a later-declared package var must
// relocate into the ordered static constructor (package_init.cs) exactly like a plain spec. The
// refusal this replaces left crypto/internal/edwards25519's `identity`/`generator` inline with a
// loud warning, so the package cctor read `feOne` while still null and every one of the package's
// 55 test verdicts died at type initialization. Both emission sub-shapes are pinned: ONE non-blank
// name (edwards25519's deconstructing form — a direct component assignment in the init method) and
// TWO-plus non-blank names (darwin os's `initCwd, initCwdErr = Getwd()` form — the call evaluated
// once into a method-local, every non-blank component assigned from it), plus the inline control
// proving an order-safe tuple spec keeps the hidden-holder emission untouched.

package main

import (
	"go/build"
	"io"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestPackageTupleVarSpecInitOrderRelocation(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	root := t.TempDir()
	pkgDir := filepath.Join(root, "tuples")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/tuples\n\ngo 1.23\n")

	// seed and label are declared BELOW the tuple specs whose initializers (transitively, through
	// makePair/makeTriple) read them — the same-file forward reference that forces relocation.
	// splitPair reads no package var at all, so the control spec has no order hazard.
	writeModuleFile(t, filepath.Join(pkgDir, "vars.go"), `package tuples

func makePair() (int, error) { return seed + 1, nil }

func makeTriple() (int, string, error) { return seed * 2, label, nil }

func splitPair() (int, int) { return 7, 8 }

// ONE non-blank name: relocates as bare fields plus a direct component assignment.
var alpha, _ = makePair()

// TWO non-blank names: relocates through a single method-local — one method for the spec.
var left, right, _ = makeTriple()

var seed = 41

var label = "lbl"

// Control: no dependency on any package var, so the inline holder emission stays.
var whole, rest = splitPair()
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	outDir := filepath.Join(root, "out")

	// Capture stderr across the conversion: the refusal this fix removes announced itself with a
	// per-var "needs init-order relocation (unsupported for tuple specs)" warning, and its absence
	// is half the guard (the emitted relocation below is the other half).
	savedStderr := os.Stderr
	pipeReader, pipeWriter, err := os.Pipe()

	if err != nil {
		t.Fatalf("stderr capture pipe: %v", err)
	}

	os.Stderr = pipeWriter
	capturedStderr := make(chan string, 1)

	go func() {
		data, _ := io.ReadAll(pipeReader)
		capturedStderr <- string(data)
	}()

	convErr := processConversion(pkgDir, true, outDir, options)

	pipeWriter.Close()
	os.Stderr = savedStderr
	stderr := <-capturedStderr
	pipeReader.Close()

	if convErr != nil {
		t.Fatalf("conversion of the tuple-spec fixture failed: %v", convErr)
	}

	if strings.Contains(stderr, "init-order relocation") {
		t.Errorf("the tuple-spec init-order refusal warning must not fire anymore, stderr:\n%s", stderr)
	}

	varsCs := readGenerated(t, filepath.Join(outDir, "vars.cs"))

	initAlpha := "init" + TempVarMarker + "alpha"
	initLeft := "init" + TempVarMarker + "left"
	tupleLocal := "tuple" + TempVarMarker + "1" + CapturedVarMarker
	blankOne := "_" + TempVarMarker + "1" + CapturedVarMarker
	blankTwo := "_" + TempVarMarker + "2" + CapturedVarMarker
	controlHolder := "tuple" + TempVarMarker + "2" + CapturedVarMarker

	// Sub-shape 1 — one non-blank name: every field bare (the blank keeps its uninitialized
	// field), the init method assigns the component directly from the once-run call.
	for _, want := range []string{
		"internal static nint alpha;",
		"internal static error " + blankOne + ";",
		"internal static void " + initAlpha + "() { alpha = makePair().Item1; }",
	} {
		if !strings.Contains(varsCs, want) {
			t.Errorf("single-non-blank moved tuple spec lost %q:\n%s", want, varsCs)
		}
	}

	// Sub-shape 2 — two non-blank names: ONE method evaluates the call once into a local and
	// assigns each non-blank component; no hidden static tuple holder is emitted for the spec.
	for _, want := range []string{
		"internal static nint left;",
		"internal static @string right;",
		"internal static error " + blankTwo + ";",
		"internal static void " + initLeft + "() { var " + tupleLocal + " = makeTriple(); left = " + tupleLocal + ".Item1; right = " + tupleLocal + ".Item2; }",
	} {
		if !strings.Contains(varsCs, want) {
			t.Errorf("multi-non-blank moved tuple spec lost %q:\n%s", want, varsCs)
		}
	}

	// The moved specs must not keep inline field initializers — that is the refusal's misordered
	// emission (the field initializer would run before seed/label are assigned).
	for _, reject := range []string{
		"internal static nint alpha = ",
		"internal static nint left = ",
		"internal static @string right = ",
	} {
		if strings.Contains(varsCs, reject) {
			t.Errorf("moved tuple spec still emits an inline field initializer %q:\n%s", reject, varsCs)
		}
	}

	// Control: the order-safe spec keeps the existing inline emission — a hidden once-evaluated
	// static holder whose component reads follow it in textual order.
	for _, want := range []string{
		"internal static (nint, nint) " + controlHolder + " = splitPair();",
		"internal static nint whole = " + controlHolder + ".Item1;",
		"internal static nint rest = " + controlHolder + ".Item2;",
	} {
		if !strings.Contains(varsCs, want) {
			t.Errorf("order-safe tuple spec lost its inline holder emission %q:\n%s", want, varsCs)
		}
	}

	// The ordered static ctor calls one method per relocated SPEC, in InitOrder ordinal order:
	// alpha (ready as soon as seed is) initializes before left (which also waits on label).
	packageInit := readGenerated(t, filepath.Join(outDir, PackageInitFileName))
	alphaCall := strings.Index(packageInit, initAlpha+"();")
	leftCall := strings.Index(packageInit, initLeft+"();")

	if alphaCall < 0 || leftCall < 0 {
		t.Fatalf("package_init.cs must call both relocated tuple-spec methods:\n%s", packageInit)
	}

	if alphaCall > leftCall {
		t.Errorf("package_init.cs calls the relocated methods out of InitOrder (alpha at %d, left at %d):\n%s", alphaCall, leftCall, packageInit)
	}
}
