// untypedPackageConversion_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards issue #33: a package that does not fully type-check must not fault the converter.
//
// Such packages are ROUTINE under -recurse, which converts whatever app and third-party code the
// closure reaches: an app package naming a symbol that only exists behind a build tag, a
// third-party package whose own import failed to resolve. go/packages loads them WITH errors, the
// converter reports those and converts best-effort — and go/types records no type at all for an
// expression whose operand went invalid (Checker.record returns early for `mode == invalid`), so
// types.Info.TypeOf hands back a nil INTERFACE. Calling a method on that is a hard nil dereference,
// not a no-op.
//
// The two halves of the defect, and so of this file:
//
//  1. The dereference itself (TestUntypedPackageConvertsWithoutPanic) — the reported crash was in
//     the escape-analysis pass, which reached `TypeOf(call.Fun).Underlying()` for an address-taken
//     argument of a call to an undefined function.
//
//  2. That the fault ESCAPED every containment the converter has (TestEscapeAnalysisPanicReachesCaller).
//     Both batch drivers wrap each package in a recover so one unconvertible package fails alone —
//     but the escape analysis runs its files in goroutines, and a panic unwinds only its own
//     goroutine's stack, so it took the whole process down instead. That is what made a single bad
//     package fatal to a 1,726-package -recurse run at [736/1726].

package main

import (
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"go/types"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestUntypedPackageConvertsWithoutPanic converts a package whose file mixes several
// invalid-operand shapes with healthy code, and requires that the conversion (a) survives and
// (b) still emits the healthy declarations. Dropping the file wholesale is not good enough: a real
// app package is mostly valid code around a couple of unresolved symbols, and the per-file recover
// in processConversion would discard all of it.
func TestUntypedPackageConvertsWithoutPanic(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	root := t.TempDir()
	pkgDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")

	// Every function below names something the type checker cannot resolve, so each leaves a
	// differently-shaped invalid operand behind: an undefined callee taking the address of a local
	// (the reported crash), an undefined selector on a package that DID resolve (the shape a
	// dependency whose own import failed leaves at each use site), an undefined call on the RHS of a
	// define (so the LHS object's own type is invalid), an undefined type in a var decl and a
	// composite literal, and a method call on a value of unresolved type.
	writeModuleFile(t, filepath.Join(pkgDir, "main.go"), `package main

import "fmt"

type Holder struct {
	N int
}

func addressTaken() {
	x := 1
	undefinedFunc(&x)
	fmt.Println(x)
}

func undefinedSelector() {
	h := Holder{N: 2}
	fmt.NoSuchFunction(&h)
}

func undefinedRHS() {
	v := undefinedFunc()
	fmt.Println(v)
}

func undefinedType() {
	var u UndefinedType
	w := &UndefinedType{Field: 3}
	fmt.Println(u, w)
}

func undefinedMethod() {
	var u UndefinedType
	u.DoThing(&u)
}

func healthy() {
	total := 0
	for i := range 4 {
		total += i * i
	}
	fmt.Println("healthy:", total)
}

func main() {
	healthy()
}
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

	// processConversion converts best-effort and returns an error only for a package LOAD failure;
	// a package that loads with type errors is not one. Before the fix this call did not return at
	// all — the escape-analysis goroutine took the process down with it.
	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("conversion of a package with type errors failed: %v", err)
	}

	mainCs := readGenerated(t, filepath.Join(outDir, "main.cs"))

	// The healthy function must be present and fully converted — proof the file was not dropped by
	// the per-file recover, which is what the nil dereference used to cause.
	for _, want := range []string{"internal static void healthy()", "total += i * i", "internal static void Main()"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("converted main.cs lost healthy code %q:\n%s", want, mainCs)
		}
	}

	// The declarations around the unresolved symbols are converted too — the emitted C# for those
	// cannot compile (nothing names the missing types), but conversion reaches them rather than
	// abandoning the file at the first invalid operand.
	for _, want := range []string{"internal static void addressTaken()", "internal static void undefinedMethod()"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("converted main.cs stopped before %q:\n%s", want, mainCs)
		}
	}
}

// TestEscapeAnalysisPanicReachesCaller proves the containment plumbing itself, independent of any
// particular converter defect: a panic raised inside the escape analysis' per-file workers is
// delivered to the CALLER's goroutine, where the per-package recovers in
// ModuleConverter.convertAll and StdLibConverter.convertPackage can turn it into one failed
// package. A panic left on a worker goroutine is unrecoverable by anyone and ends the process.
//
// The fault is injected with a nil *types.Info — the analysis' first object lookup dereferences it
// — so this test keeps proving the mechanism no matter which nil-type sites get guarded later.
func TestEscapeAnalysisPanicReachesCaller(t *testing.T) {
	fset := token.NewFileSet()

	file, err := parser.ParseFile(fset, "fault.go", "package fault\n\nfunc f() {\n\tx := 1\n\t_ = x\n}\n", 0)

	if err != nil {
		t.Fatalf("parse fixture: %v", err)
	}

	files := []FileEntry{{
		file:             file,
		filePath:         "fault.go",
		identEscapesHeap: map[types.Object]bool{},
		sstringEligible:  map[types.Object]bool{},
		ssliceEligible:   map[types.Object]bool{},
		sstringConvExprs: map[*ast.CallExpr]bool{},
	}}

	recovered := func() (r any) {
		defer func() { r = recover() }()

		performEscapeAnalysis(files, fset, types.NewPackage("example.com/fault", "fault"), nil)

		return nil
	}()

	if recovered == nil {
		t.Fatal("escape analysis swallowed a worker panic — the caller must see it, so a batch driver can fail just this package")
	}

	// The re-raised value carries the FAULTING stack, not the re-raise site: re-panicking bare would
	// report only performEscapeAnalysis' own line and name no converter code at all, which is what
	// makes a user-reported crash diagnosable.
	if message := toString(recovered); !strings.Contains(message, "escapeAnalysisOperations.go") {
		t.Errorf("re-raised panic lost the faulting stack: %v", recovered)
	}
}

func toString(v any) string {
	if s, ok := v.(string); ok {
		return s
	}

	if err, ok := v.(error); ok {
		return err.Error()
	}

	return ""
}
