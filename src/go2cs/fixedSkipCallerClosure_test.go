// fixedSkipCallerClosure_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the FIXED-SKIP PATH CLOSURE of computeNoInliningClosure (callerInliningAnalysis.go) on the MARKED
// SET it computes, not on rendered text. logrus is the measured case: getCaller's
// runtime.Callers(minimumCallerDepth, ...) skips a fixed frame count up to Entry.Info, the Release
// TieredCompilation=0 JIT inlines the small Info -> Log -> logArgs bodies, and the reported caller lands one
// frame too far out. Four arms: the logrus-shaped chain in a module is SELECTED; the same chain with a
// COMPUTED skip is not (the control); the same chain as a standard-library package is not (the scope arm); and
// an init on the path is never selected (the runtime runs it; its reported name reads right unmarked).

package main

import (
	"go/ast"
	"path/filepath"
	"testing"

	"golang.org/x/tools/go/packages"
)

// logrusShapedChain is the fixture: skipExpr is getCaller's runtime.Callers skip argument.
func logrusShapedChain(skipExpr string) string {
	return `package logshape

import (
	"fmt"
	"runtime"
	"strings"
)

var minimumCallerDepth = 4

type Entry struct {
	enabled bool
	caller  string
	pcs     []uintptr
}

// SEED -- the Caller/Callers user.
func getCaller(e *Entry) string {
	pcs := make([]uintptr, 8)
	depth := runtime.Callers(` + skipExpr + `, pcs)
	frames := runtime.CallersFrames(pcs[:depth])
	frame, _ := frames.Next()
	return frame.Function
}

// LARGE -- on the path, walked through, never marked: the JIT does not inline it.
func (e *Entry) log(msg string) {
	for i := 0; i < 2; i++ {
		msg = strings.TrimSpace(msg)
	}
	e.caller = getCaller(e)
	_ = msg
}

// SMALL (thin) -- above a large frame.
func (e *Entry) logArgs(args ...any) {
	e.log(fmt.Sprint(args...))
}

// SMALL (an if, not thin) -- Entry.Log.
func (e *Entry) Log(args ...any) {
	if e.enabled {
		e.logArgs(args...)
	}
}

// SMALL (thin) -- Entry.Info, the frame logrus reports.
func (e *Entry) Info(args ...any) {
	e.Log(args...)
}

// OFF-PATH -- small, never reaches the seed.
func (e *Entry) Enable() {
	if !e.enabled {
		e.enabled = true
	}
}

// PARAMETER SKIP -- log's output(calldepth): a skip that is the seed's own parameter is fixed.
func output(calldepth int) string {
	_, file, _, _ := runtime.Caller(calldepth)
	return file
}

// SMALL (an if, not thin) -- on the parameter-skip path.
func Println(s string) string {
	if s == "" {
		s = "-"
	}
	return output(2) + s
}

// INIT -- small and on the parameter-skip path, but never selected: the runtime runs it, no Go code calls it.
func init() {
	_ = Println("")
}
`
}

// markedFixedSkipFixture loads the fixture at dir and returns the marked declarations by name, with the
// closure's scope decided by fixedSkipClosureApplies against goRoot -- the conversion drivers' own call.
func markedFixedSkipFixture(t *testing.T, dir, goRoot, skipExpr string) map[string]bool {
	t.Helper()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":      "module example.com/logshape\n\ngo 1.23\n",
		"logshape.go": logrusShapedChain(skipExpr),
	})

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir}, ".")

	if err != nil || len(loaded) != 1 || len(loaded[0].Errors) > 0 {
		t.Fatalf("fixture load: err=%v packages=%d", err, len(loaded))
	}

	pkg := loaded[0]

	var files []FileEntry

	for i, file := range pkg.Syntax {
		files = append(files, newFileEntry(file, pkg.CompiledGoFiles[i], false))
	}

	marked := computeNoInliningClosure(files, pkg.Types, pkg.TypesInfo, fixedSkipClosureApplies(pkg.Dir, goRoot))

	names := map[string]bool{}

	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			if fn, ok := decl.(*ast.FuncDecl); ok && marked[pkg.TypesInfo.Defs[fn.Name]] {
				names[fn.Name.Name] = true
			}
		}
	}

	return names
}

func assertMarked(t *testing.T, marked map[string]bool, want map[string]bool) {
	t.Helper()

	for name, expected := range want {
		if marked[name] != expected {
			t.Errorf("%s: marked=%v, want %v (marked set %v)", name, marked[name], expected, marked)
		}
	}
}

// The logrus-shaped chain in a module, package-var skip: every small function on the path is marked.
func TestFixedSkipClosureSelectsModuleChain(t *testing.T) {
	root := t.TempDir()
	goRoot := filepath.Join(root, "goroot")

	marked := markedFixedSkipFixture(t, filepath.Join(root, "module"), goRoot, "minimumCallerDepth")

	assertMarked(t, marked, map[string]bool{
		"getCaller": true,  // the seed itself
		"logArgs":   true,  // small, above a large frame
		"Log":       true,  // small, not thin: today's rule misses it
		"Info":      true,  // thin over Log
		"log":       false, // large: walked through, not marked
		"Enable":    false, // off the path
		"output":    true,  // the parameter-skip seed
		"Println":   true,  // small, over a parameter skip
		"init":      false, // never an init
	})
}

// The CONTROL: the same chain with a COMPUTED skip selects nothing beyond today's rule.
func TestFixedSkipClosureIgnoresComputedSkip(t *testing.T) {
	root := t.TempDir()
	goRoot := filepath.Join(root, "goroot")

	marked := markedFixedSkipFixture(t, filepath.Join(root, "module"), goRoot, "len(e.pcs)+2")

	assertMarked(t, marked, map[string]bool{
		"getCaller": true, // still seeded by callsSkipCountedRuntimeCaller
		"logArgs":   false,
		"Log":       false,
		"Info":      false,
		"log":       false,
		"Enable":    false,
		"output":    true,
		"Println":   true, // the parameter-skip path is unchanged
	})
}

// The SCOPE arm: the same package under GOROOT's src tree is the standard library and selects nothing.
func TestFixedSkipClosureExcludesStandardLibrary(t *testing.T) {
	root := t.TempDir()
	goRoot := filepath.Join(root, "goroot")

	marked := markedFixedSkipFixture(t, filepath.Join(goRoot, "src", "logshape"), goRoot, "minimumCallerDepth")

	assertMarked(t, marked, map[string]bool{
		"getCaller": true,
		"logArgs":   false,
		"Log":       false,
		"Info":      false,
		"output":    true,
		"Println":   false,
	})
}

func TestFixedSkipClosureAppliesOutsideGoRootSrc(t *testing.T) {
	goRoot := filepath.Join("C:", "goroot")

	cases := []struct {
		dir  string
		want bool
	}{
		{filepath.Join("C:", "work", "module"), true},
		{filepath.Join(goRoot, "src", "log"), false},
		{filepath.Join(goRoot, "src", "vendor", "golang.org", "x", "net"), false},
		{"", false},
	}

	for _, c := range cases {
		if got := fixedSkipClosureApplies(c.dir, goRoot); got != c.want {
			t.Errorf("fixedSkipClosureApplies(%q) = %v, want %v", c.dir, got, c.want)
		}
	}

	if fixedSkipClosureApplies(filepath.Join("C:", "work"), "") {
		t.Errorf("an unknown GOROOT must answer false")
	}
}
