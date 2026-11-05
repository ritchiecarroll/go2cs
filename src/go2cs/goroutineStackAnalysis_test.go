// goroutineStackAnalysis_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the goroutine stack-bound proof (goroutineStackAnalysis.go) and its mark (visitGoStmt.go). A go statement
// whose goroutine provably cannot recurse is recorded in package_info.cs, `[assembly: GoBoundedGoroutine(file, line,
// column)]` at its goǃ call, which go2cs-gen intercepts to start it on golib's small stack reservation; every goroutine
// the proof cannot follow keeps the Go-scale default, unrecorded, because an under-reserved goroutine that recurses
// deeply ends the process where Go would have grown its stack.

package main

import (
	"go/ast"
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strconv"
	"strings"
	"testing"
	"unicode/utf16"

	"golang.org/x/tools/go/packages"
)

const goroutineStackFixture = `package main

import (
	"fmt"
	"sync"
)

type shape interface{ area() int }

type square struct{ side int }

func (s square) area() int { return s.side * s.side }

func leaf(n int) int { return n + 1 }

func chain(n int) int { return leaf(leaf(n)) }

func fib(n int) int {
	if n < 2 {
		return n
	}
	return fib(n-1) + fib(n-2)
}

func ping(n int) int {
	if n == 0 {
		return 0
	}
	return pong(n - 1)
}

func pong(n int) int { return ping(n) }

func seq(yield func(int) bool) {
	for i := range 3 {
		if !yield(i) {
			return
		}
	}
}

func worker(ch chan int, wg *sync.WaitGroup) {
	defer wg.Done()
	ch <- chain(1)
}

func main() {
	ch := make(chan int, 16)
	var wg sync.WaitGroup
	var s shape = square{2}
	f := func() { ch <- 9 }

	wg.Add(1)
	go worker(ch, &wg)                         // BOUNDED: static calls, no cycle
	go func(id int) { ch <- leaf(id) }(4)      // BOUNDED: a literal calling a leaf
	go func() { defer wg.Wait(); ch <- 5 }()   // BOUNDED: sync.WaitGroup reaches no callback
	go func() { ch <- fib(10) }()              // UNBOUNDED: recursion
	go func() { ch <- ping(3) }()              // UNBOUNDED: mutual recursion
	go func() { ch <- s.area() }()             // UNBOUNDED: an interface method
	go f()                                     // UNBOUNDED: a func value
	go func() { for i := range seq { ch <- i } }() // UNBOUNDED: range over func
	go fmt.Println("hi")                       // UNBOUNDED: fmt reaches interface calls

	for range 10 {
		<-ch
	}
}
`

func TestGoroutineStackBoundedProvesOnlyNonRecursiveStaticCallGraphs(t *testing.T) {
	isolateConverterPathGlobals(t)
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":  "module example.com/gostack\n\ngo 1.23\n",
		"main.go": goroutineStackFixture,
	})

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir}, ".")

	if err != nil || len(loaded) != 1 || len(loaded[0].Errors) > 0 {
		t.Fatalf("fixture did not load: %v %v", err, loaded)
	}

	pkg := loaded[0]

	packageLock.Lock()
	saved := currentLoadedPackage
	currentLoadedPackage = pkg
	packageLock.Unlock()

	t.Cleanup(func() {
		packageLock.Lock()
		currentLoadedPackage = saved
		packageLock.Unlock()
	})

	verdicts := 0

	for _, file := range pkg.Syntax {
		ast.Inspect(file, func(node ast.Node) bool {
			goStmt, ok := node.(*ast.GoStmt)

			if !ok {
				return true
			}

			line := pkg.Fset.Position(goStmt.Pos()).Line
			source := strings.Split(goroutineStackFixture, "\n")[line-1]
			want := strings.Contains(source, "// BOUNDED")

			if !want && !strings.Contains(source, "// UNBOUNDED") {
				t.Fatalf("fixture line %d carries no expectation: %s", line, source)
			}

			bounded, why := goroutineStackBounded(goStmt.Call, pkg.TypesInfo)
			verdicts++

			if bounded != want {
				t.Errorf("line %d: bounded = %v (%s), want %v: %s", line, bounded, why, want, strings.TrimSpace(source))
			}

			return true
		})
	}

	if verdicts != 9 {
		t.Fatalf("read %d go statements, want 9: the fixture and the walk disagree", verdicts)
	}
}

func TestABoundedGoroutineIsRecordedInThePackageInfoNotInItsStatement(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/gostack\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), goroutineStackFixture)

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

	packageDir := filepath.Join(options.go2csPath, "src", "example.com", "gostack")
	mainCs := readGenerated(t, filepath.Join(packageDir, "main.cs"))
	packageInfo := readGenerated(t, filepath.Join(packageDir, "package_info.cs"))

	// The statement reads as any other: the mark is not in the converted source.
	if strings.Contains(mainCs, "GoStack") {
		t.Errorf("a go statement still carries a visible GoStack mark:\n%s", mainCs)
	}

	for _, want := range []string{"goǃ(worker, ch, Ꮡwg);", "goǃ(fʗ1);"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q:\n%s", want, mainCs)
		}
	}

	records := regexp.MustCompile(`GoBoundedGoroutine\("main\.cs", (\d+), (\d+)\)\]`).FindAllStringSubmatch(packageInfo, -1)

	if len(records) != 3 {
		t.Fatalf("want exactly the 3 bounded go statements recorded, got %d:\n%s", len(records), packageInfo)
	}

	lines := strings.Split(mainCs, "\n")
	recorded := map[int]bool{}

	// Each record lands on the first token of a goǃ call: the generator intercepts exactly that position.
	for _, record := range records {
		line, _ := strconv.Atoi(record[1])
		column, _ := strconv.Atoi(record[2])

		if line < 1 || line > len(lines) {
			t.Fatalf("record %s names line %d of a %d-line file", record[0], line, len(lines))
		}

		units := utf16.Encode([]rune(lines[line-1]))

		if column < 1 || column > len(units) || !strings.HasPrefix(string(utf16.Decode(units[column-1:])), "goǃ(") {
			t.Errorf("record %s does not land on a goǃ call: %q", record[0], lines[line-1])
		}

		recorded[line] = true
	}

	for index, line := range lines {
		bounded := strings.Contains(line, "goǃ(worker, ch, Ꮡwg);")
		unbounded := strings.Contains(line, "goǃ(fʗ1);")

		if bounded && !recorded[index+1] {
			t.Errorf("the bounded worker statement on line %d has no record:\n%s", index+1, packageInfo)
		}

		if unbounded && recorded[index+1] {
			t.Errorf("the unbounded func-value statement on line %d is recorded:\n%s", index+1, packageInfo)
		}
	}
}
