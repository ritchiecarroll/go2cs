// goDeferConstArgs_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards seat #33: a CONSTANT argument of a go or defer statement renders inside the thunk's call, so the
// statement reads as the Go source does (`goǃ(() => fmt.Println("First"))`), while every other argument stays
// eager and binds to a temp in order (`goǃ(ᴛ1 => pair("square:", ᴛ1), n * n)`). A method-group or delegate-value
// launch keeps its form, and a string constant there renders `(@string)"…"` at the site.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestGoDeferConstantArgumentsRenderInsideTheThunk(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/gdc\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type Color int

const Red Color = 1

var results = make(chan string, 1)

func report(v any) { results <- fmt.Sprint(v) }

func worker(n int) { results <- fmt.Sprint(n) }

func pair(label string, n int) { results <- fmt.Sprint(label, n) }

func trio(a int, s string, b int) { fmt.Println(a, s, b) }

type box struct{ name string }

func (b *box) say(msg string) { results <- b.name + msg }

var seq int

func next() int {
	seq++
	return seq
}

func spawned() {
	go fmt.Println("First")
	go report(7)
	go worker(5)
	n := 3
	go pair("square:", n*n)
	b := &box{name: "b"}
	go b.say("hello")
	f := report
	go f("via func value")
}

func deferred() {
	a, c := 1, 2
	defer report(Red)
	defer trio(next(), "mid", next())
	defer fmt.Println(a, c)
}

func main() {
	spawned()
	deferred()
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

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "gdc", "main.cs")), "\r\n", "\n")

	for _, want := range []string{
		// Every argument constant: the thunk takes no parameter and the call reads as Go's.
		`goǃ(() => fmt.Println("First"));`,
		`goǃ(() => report((nint)7));`,
		`goǃ(() => worker(5));`,
		`defer(() => report(Red), ref ᒐ);`,
		// A non-constant argument stays eager, numbered in order around the inlined constants.
		`goǃ(ᴛ1 => pair("square:", ᴛ1), n * n);`,
		`defer((ᴛ1, ᴛ2) => trio(ᴛ1, "mid", ᴛ2), next(), next(), ref ᒐ);`,
		// With no constant, every argument stays eager and binds to its own temp.
		`defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), a, c, ref ᒐ);`,
		// A method-group or delegate-value launch keeps its form; its string constant renders at the site.
		`goǃ(bʗ1.say, (@string)"hello", GoStack.Bounded);`,
		`goǃ(fʗ1, (@string)"via func value");`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q:\n%s", want, mainCs)
		}
	}

	// No hoisted field remains for a go/defer argument: each constant renders where it is used.
	for _, unwanted := range []string{"firstˢ", "squareˢ", "midˢ", "helloˢ", "viaFuncValueˢ"} {
		if strings.Contains(mainCs, unwanted) {
			t.Errorf("unwanted %q: a go/defer constant argument was hoisted:\n%s", unwanted, mainCs)
		}
	}
}
