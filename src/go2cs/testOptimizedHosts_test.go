// testOptimizedHosts_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"testing"
)

// A converted test assembly is compiled without optimization so runtime.Caller reads per-call lines (go.lib.symbols.targets),
// and GoOptimizedTestHosts names the stdlib test hosts that keep their optimizations instead (COORD 2026-11-25 15:08Z): an
// unoptimized frame stops a want-0 AllocsPerRun body inlining the call it measures (context's TestAllocs), and a spill slot keeps
// a value live that Go lets the collector take (sync's TestPoolGC). This guard holds the list to its admission rule. Every entry
// is <project name>=<test>: the project is a committed stdlib test host, its package has that test, and no _test.go file of the
// package reads caller lines -- neither runtime.Caller, runtime.Callers nor runtime.CallersFrames, nor an expected "x_test.go:N"
// line in a string literal (testing's helper tests) -- since optimizing such a host is exactly the loss the switch prevents.

var goOptimizedTestHostsPattern = regexp.MustCompile(`<GoOptimizedTestHosts>([^<]*)</GoOptimizedTestHosts>`)

var expectedTestLinePattern = regexp.MustCompile(`_test\.go:[0-9]+`)

// optimizedTestHosts reads GoOptimizedTestHosts from golib's go.lib.symbols.targets as project name -> test name.
func optimizedTestHosts(t *testing.T) map[string]string {
	t.Helper()

	targets := filepath.Join("..", "core", "golib", "buildTransitive", "go.lib.symbols.targets")
	content, err := os.ReadFile(targets)

	if err != nil {
		t.Fatal(err)
	}

	matches := goOptimizedTestHostsPattern.FindAllSubmatch(content, -1)

	if len(matches) != 1 {
		t.Fatalf("%s: found %d GoOptimizedTestHosts properties, want exactly 1", targets, len(matches))
	}

	hosts := map[string]string{}

	for _, entry := range strings.Split(string(matches[0][1]), ";") {
		project, test, ok := strings.Cut(strings.TrimSpace(entry), "=")

		if !ok || !strings.HasSuffix(project, ".tests") || !strings.HasPrefix(test, "Test") {
			t.Errorf("GoOptimizedTestHosts entry %q is not <package>.tests=Test<Name>", entry)
			continue
		}

		hosts[project] = test
	}

	return hosts
}

// callerLineReaders answers where a package's _test.go files read caller lines, as file:line positions.
func callerLineReaders(t *testing.T, dir string) (readers []string, tests map[string]bool) {
	t.Helper()

	files, err := filepath.Glob(filepath.Join(dir, "*_test.go"))

	if err != nil || len(files) == 0 {
		t.Fatalf("%s: no _test.go files (%v)", dir, err)
	}

	tests = map[string]bool{}
	fileSet := token.NewFileSet()

	for _, file := range files {
		parsed, err := parser.ParseFile(fileSet, file, nil, parser.SkipObjectResolution)

		if err != nil {
			t.Fatal(err)
		}

		runtimeName := ""

		for _, spec := range parsed.Imports {
			if path, _ := strconv.Unquote(spec.Path.Value); path == "runtime" {
				runtimeName = "runtime"

				if spec.Name != nil {
					runtimeName = spec.Name.Name
				}
			}
		}

		for _, decl := range parsed.Decls {
			if fn, ok := decl.(*ast.FuncDecl); ok && fn.Recv == nil {
				tests[fn.Name.Name] = true
			}
		}

		ast.Inspect(parsed, func(node ast.Node) bool {
			switch n := node.(type) {
			case *ast.SelectorExpr:
				if ident, ok := n.X.(*ast.Ident); ok && runtimeName != "" && ident.Name == runtimeName {
					switch n.Sel.Name {
					case "Caller", "Callers", "CallersFrames":
						readers = append(readers, fileSet.Position(n.Pos()).String()+" runtime."+n.Sel.Name)
					}
				}
			case *ast.BasicLit:
				if n.Kind == token.STRING && expectedTestLinePattern.MatchString(n.Value) {
					readers = append(readers, fileSet.Position(n.Pos()).String()+" expected line "+n.Value)
				}
			}

			return true
		})
	}

	return readers, tests
}

func TestOptimizedTestHostsKeepTheirAdmissionRule(t *testing.T) {
	hosts := optimizedTestHosts(t)

	if len(hosts) == 0 {
		t.Fatal("GoOptimizedTestHosts is empty: drop the property and this guard together, or keep an entry")
	}

	goroot := build.Default.GOROOT

	for project, test := range hosts {
		pkg := strings.ReplaceAll(strings.TrimSuffix(project, ".tests"), ".", "/")

		if _, err := os.Stat(filepath.Join("..", "core", filepath.FromSlash(pkg), project+".csproj")); err != nil {
			t.Errorf("%s: no committed stdlib test host for package %s: %v", project, pkg, err)
			continue
		}

		readers, tests := callerLineReaders(t, filepath.Join(goroot, "src", filepath.FromSlash(pkg)))

		if !tests[test] {
			t.Errorf("%s: package %s has no %s, the test its entry names", project, pkg, test)
		}

		for _, reader := range readers {
			t.Errorf("%s: package %s reads caller lines (%s); an optimized host loses them, so disclose its test instead", project, pkg, reader)
		}
	}
}

// Every listed host must actually evaluate Optimize=true: an entry the condition misses (a nested package's host has the root
// namespace go.<parent>, not go) would sit in the list and change nothing. An explicit GoTestOptimize=false keeps the A/B arm.
func TestOptimizedTestHostsEvaluateOptimized(t *testing.T) {
	if testing.Short() {
		t.Skip("evaluates MSBuild projects")
	}

	if _, err := exec.LookPath("dotnet"); err != nil {
		t.Skipf("the .NET SDK is not on PATH: %v", err)
	}

	for project := range optimizedTestHosts(t) {
		pkg := strings.ReplaceAll(strings.TrimSuffix(project, ".tests"), ".", "/")
		path, err := filepath.Abs(filepath.Join("..", "core", filepath.FromSlash(pkg), project+".csproj"))

		if err != nil {
			t.Fatal(err)
		}

		if got := evaluatedOptimize(t, path); !strings.EqualFold(got, "true") {
			t.Errorf("%s: Optimize is %q, want true (listed in GoOptimizedTestHosts)", project, got)
		}

		if got := evaluatedOptimize(t, path, "-p:GoTestOptimize=false"); !strings.EqualFold(got, "false") {
			t.Errorf("%s with GoTestOptimize=false: Optimize is %q, want false", project, got)
		}
	}
}

// The predicate's own control: testing is the row the switch exists for (its helper tests read file:line), and log
// reads runtime.Caller. If either reads clean, the guard above proves nothing.
func TestOptimizedTestHostsCallerPredicateFires(t *testing.T) {
	goroot := build.Default.GOROOT

	for _, pkg := range []string{"testing", "log"} {
		if readers, _ := callerLineReaders(t, filepath.Join(goroot, "src", filepath.FromSlash(pkg))); len(readers) == 0 {
			t.Errorf("package %s reads no caller lines by this predicate: the guard is blind", pkg)
		}
	}
}
