// variadicLiftedElement_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the referent of a variadic alias (`using ꓸꓸꓸT = Span<…>;`) whose element is an ANONYMOUS
// struct or interface (TS readiness C37, internal/printer's `greatestEnd(end int, nodes ...interface{
// End() int })`). The element is lifted to a nested type of the package class, and the alias sits at
// namespace scope, so the bare `Span<greatestEnd_nodes>` is CS0246. The referent takes the package
// class qualifier exactly as a same-package named element does.
//
// The fixture covers a lifted interface, a lifted struct, and a SECOND file that reuses the first
// file's lift. The CONTROL is a named struct, which was already qualified.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertVariadicLiftedElementFixture(t *testing.T) map[string]string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/variadiclifted\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type span struct{ end int }

func (s span) End() int { return s.end }

// CONTROL -- a named struct: already qualified.
type box struct{ n int }

func boxes(bs ...box) int { return len(bs) }

// POSITIVE 1 -- the TS shape: a variadic anonymous interface.
func greatestEnd(end int, nodes ...interface{ End() int }) int {
	for _, n := range nodes {
		if n.End() > end {
			end = n.End()
		}
	}

	return end
}

// POSITIVE 2 -- a variadic anonymous struct.
func sumPoints(points ...struct{ x, y int }) int {
	total := 0

	for _, p := range points {
		total += p.x + p.y
	}

	return total
}

func main() {
	fmt.Println(greatestEnd(1, span{4}, span{2}), greatestEnd(7), sumPoints(struct{ x, y int }{1, 2}), boxes(box{1}), leastEnd(9, span{3}))
}
`)
	writeModuleFile(t, filepath.Join(appDir, "other.go"), `package main

// POSITIVE 3 -- a second file whose variadic element is the same anonymous interface.
func leastEnd(end int, nodes ...interface{ End() int }) int {
	for _, n := range nodes {
		if n.End() < end {
			end = n.End()
		}
	}

	return end
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

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "variadiclifted")

	return map[string]string{
		"main.cs":  readGenerated(t, filepath.Join(outDir, "main.cs")),
		"other.cs": readGenerated(t, filepath.Join(outDir, "other.cs")),
	}
}

func TestVariadicAliasOfALiftedElementIsPackageQualified(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	emission := convertVariadicLiftedElementFixture(t)

	aliasLines := func(file string) []string {
		var lines []string

		for _, line := range strings.Split(emission[file], "\n") {
			if trimmed := strings.TrimSpace(line); strings.HasPrefix(trimmed, "using ꓸꓸꓸ") {
				lines = append(lines, trimmed)
			}
		}

		return lines
	}

	// Every variadic alias in both files names a package-class member, so every referent is qualified:
	// main.cs has box, the lifted interface and the lifted struct; other.cs has the reused interface.
	for file, want := range map[string]int{"main.cs": 3, "other.cs": 1} {
		lines := aliasLines(file)

		if len(lines) != want {
			t.Errorf("%s: %d variadic aliases, want %d: %q\n%s", file, len(lines), want, lines, emission[file])
		}

		for _, line := range lines {
			if !strings.Contains(line, "= Span<main_package.") {
				t.Errorf("%s: a variadic alias referent is not qualified with the package class: %s", file, line)
			}
		}
	}
}
