// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// relocscan checks a hop's relocation arcs (docs/phase4/data/relocations-go<rel>.tsv) for the one thing a
// body-similarity score cannot see: whether the tests that "moved" brought what they read with them.
//
// A relocation is derived by body: a test that LEFT the source package and ARRIVED, by name, in the target.
// A score on the printed function compares control flow, and two unrelated tests that share a
// switch/range/Errorf skeleton score high. The 1.26 hop's fifth arc was such a match (runtime ->
// internal/strconv, TestAtoi at 0.84): runtime's test read atoi32tests and atoi64tests, tables that exist
// nowhere at the target. A test that truly moves carries its tables and helpers.
//
// THE CHECK, per arc: every Test/Benchmark/Fuzz/Example function declared in the source package at the
// outgoing release (-from), absent from the source at the incoming release (-to), and declared by name in the
// target at -to, is a MOVED test. Each identifier its body reads that names a package-level declaration of
// the source package at -from (any file, test or not; a selector's right side is not read) must also be
// declared at package level in the target at -to. One that is not is MISSING, and the arc is refused.
//
// Files are read by name, without build constraints (testdata excluded): for a declaration-existence check,
// the union over all files is the permissive side. An identifier the parser resolves to a declaration inside
// the test function itself (a parameter, a local, a range variable) is local and not checked: runtime's
// TestAtoi declares ok, out and test, which share names with runtime package-level declarations.
//
// Usage (from src/go2cs):
//
//	go run ./internal/relocscan -from <outgoing GOROOT> -to <incoming GOROOT> -arcs <relocations TSV>
//
// Exit 0 when every arc's moved tests find what they read; 1 when any arc has a MISSING identifier; 2 on a
// usage or read error.
package main

import (
	"bufio"
	"flag"
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
)

var testFuncName = regexp.MustCompile(`^(Test|Benchmark|Fuzz|Example)`)

// pkgInfo is one package directory, read by name: its package-level declaration names and its test bodies.
type pkgInfo struct {
	exists bool
	decls  map[string]bool
	tests  map[string]*ast.FuncDecl
}

func main() {
	from := flag.String("from", "", "GOROOT of the outgoing release (required)")
	to := flag.String("to", "", "GOROOT of the incoming release (required)")
	arcs := flag.String("arcs", "", "relocations TSV: '#' comments, a 'source<TAB>target' header, then one arc per line (required)")
	flag.Parse()

	if *from == "" || *to == "" || *arcs == "" {
		fmt.Fprintln(os.Stderr, "relocscan: -from, -to and -arcs are required")
		os.Exit(2)
	}

	refused, err := run(*from, *to, *arcs)

	if err != nil {
		fmt.Fprintln(os.Stderr, "relocscan:", err)
		os.Exit(2)
	}

	if refused > 0 {
		os.Exit(1)
	}
}

func run(from, to, arcsPath string) (int, error) {
	arcs, err := readArcs(arcsPath)

	if err != nil {
		return 0, err
	}

	refused := 0

	for _, arc := range arcs {
		srcFrom := readPkg(from, arc[0])
		srcTo := readPkg(to, arc[0])
		dstTo := readPkg(to, arc[1])

		if !srcFrom.exists {
			return 0, fmt.Errorf("arc %s -> %s: the source does not exist at -from", arc[0], arc[1])
		}

		if !dstTo.exists {
			return 0, fmt.Errorf("arc %s -> %s: the target does not exist at -to", arc[0], arc[1])
		}

		var moved []string

		for name := range srcFrom.tests {
			if _, stayed := srcTo.tests[name]; stayed {
				continue
			}

			if _, arrived := dstTo.tests[name]; arrived {
				moved = append(moved, name)
			}
		}

		sort.Strings(moved)

		missing := map[string][]string{}
		checked := 0

		for _, name := range moved {
			for _, ident := range referencedDecls(srcFrom.tests[name], srcFrom.decls) {
				checked++

				if !dstTo.decls[ident] {
					missing[name] = append(missing[name], ident)
				}
			}
		}

		verdict := "OK"

		if len(missing) > 0 {
			verdict = "REFUSED"
			refused++
		}

		fmt.Printf("%-8s %s -> %s: %d moved test(s), %d package-level read(s) checked\n", verdict, arc[0], arc[1], len(moved), checked)

		for _, name := range moved {
			if idents := missing[name]; len(idents) > 0 {
				fmt.Printf("  MISSING  %s reads %s, declared nowhere in %s at -to\n", name, strings.Join(idents, ", "), arc[1])
			}
		}
	}

	return refused, nil
}

// readArcs reads the TSV's arc rows, skipping comments and the header.
func readArcs(path string) ([][2]string, error) {
	f, err := os.Open(path)

	if err != nil {
		return nil, err
	}

	defer f.Close()

	var arcs [][2]string

	scanner := bufio.NewScanner(f)

	for scanner.Scan() {
		line := strings.TrimRight(scanner.Text(), "\r")

		if line == "" || strings.HasPrefix(line, "#") || line == "source\ttarget" {
			continue
		}

		fields := strings.Split(line, "\t")

		if len(fields) != 2 {
			return nil, fmt.Errorf("%s: not a source<TAB>target row: %q", path, line)
		}

		arcs = append(arcs, [2]string{fields[0], fields[1]})
	}

	return arcs, scanner.Err()
}

// readPkg reads GOROOT/src/<importPath>: every .go file in the directory itself, by name.
func readPkg(goroot, importPath string) pkgInfo {
	info := pkgInfo{decls: map[string]bool{}, tests: map[string]*ast.FuncDecl{}}
	dir := filepath.Join(goroot, "src", filepath.FromSlash(importPath))

	entries, err := os.ReadDir(dir)

	if err != nil {
		return info
	}

	fset := token.NewFileSet()

	for _, e := range entries {
		if e.IsDir() || !strings.HasSuffix(e.Name(), ".go") {
			continue
		}

		file, err := parser.ParseFile(fset, filepath.Join(dir, e.Name()), nil, 0)

		if err != nil {
			continue
		}

		info.exists = true

		for _, decl := range file.Decls {
			switch d := decl.(type) {
			case *ast.FuncDecl:
				if d.Recv != nil {
					continue
				}

				info.decls[d.Name.Name] = true

				if strings.HasSuffix(e.Name(), "_test.go") && testFuncName.MatchString(d.Name.Name) && d.Body != nil {
					info.tests[d.Name.Name] = d
				}
			case *ast.GenDecl:
				for _, spec := range d.Specs {
					switch s := spec.(type) {
					case *ast.ValueSpec:
						for _, n := range s.Names {
							info.decls[n.Name] = true
						}
					case *ast.TypeSpec:
						info.decls[s.Name.Name] = true
					}
				}
			}
		}
	}

	return info
}

// referencedDecls returns, sorted, the package-level names of decls that fn's body reads. The right side of a
// selector (x.Name), a composite literal's field keys, and an identifier resolved to a declaration inside fn
// are not references to the package scope.
func referencedDecls(fn *ast.FuncDecl, decls map[string]bool) []string {
	skip := map[*ast.Ident]bool{}
	seen := map[string]bool{}

	ast.Inspect(fn.Body, func(n ast.Node) bool {
		switch x := n.(type) {
		case *ast.SelectorExpr:
			skip[x.Sel] = true
		case *ast.KeyValueExpr:
			if key, ok := x.Key.(*ast.Ident); ok {
				skip[key] = true
			}
		case *ast.Ident:
			if x.Obj != nil && x.Obj.Pos() >= fn.Pos() && x.Obj.Pos() < fn.End() {
				return true
			}

			if !skip[x] && decls[x.Name] && x.Name != fn.Name.Name {
				seen[x.Name] = true
			}
		}

		return true
	})

	names := make([]string, 0, len(seen))

	for name := range seen {
		names = append(names, name)
	}

	sort.Strings(names)

	return names
}
