// goStmtReceiverBoxMethodGroup_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a go statement in METHOD-GROUP form on a pointer method's OWN receiver (gopkg.in/yaml.v3's tests,
// COORD 2026-10-03): `go t.loop()` inside `func (t *tracker)` binds the box group `goǃ(Ꮡt.loop)`, which needs
// the receiver box in scope. The method was not promoted to direct-ж, so the receiver was the `ref T` alias and
// `goǃ(t.loop)` bound the [GoRecv] ref extension (CS1113/CS1503), and a value field chain rooted at the receiver
// named a box that was never declared (`goǃ(Ꮡt.of(tracker.Ꮡin).send)`, CS0103). The controls hold a deferred
// call, which already promoted, and a go statement on a pointer PARAMETER, which needs no promotion.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestAGoStatementOnTheReceiversOwnPointerMethodPromotesTheMethod(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/gsr\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type inner struct{ ch chan int }

func (in *inner) send() { in.ch <- 3 }

type tracker struct {
	done chan int
	n    int
	in   inner
}

func (t *tracker) loop()         { t.done <- t.n }
func (t *tracker) loopArg(k int) { t.done <- k }
func (t *tracker) bump()         { t.n++ }

func (t *tracker) start()           { go t.loop() }
func (t *tracker) startArg()        { go t.loopArg(9) }
func (t *tracker) startInner()      { go t.in.send() }
func (t *tracker) stop()            { defer t.bump() }
func (t *tracker) other(o *tracker) { go o.loop() }

func main() {
	t := &tracker{done: make(chan int), n: 7, in: inner{ch: make(chan int)}}
	t.start()
	fmt.Println(<-t.done)
	t.startArg()
	fmt.Println(<-t.done)
	t.startInner()
	fmt.Println(<-t.in.ch)
	t.stop()
	fmt.Println(t.n)
	o := &tracker{done: make(chan int), n: 11}
	t.other(o)
	fmt.Println(<-o.done)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "gsr", "main.cs"))

	// The method is direct-ж, so the go statement binds the receiver box's method group.
	// Each of these executes a go statement, so it keeps its frame as the goroutine's creator and is
	// written as a partial method's implementing part (the no-inline carrier).
	for _, want := range []string{
		"internal static partial void start(this ж<tracker> Ꮡt)",
		"goǃ(Ꮡt.loop);",
		"internal static partial void startArg(this ж<tracker> Ꮡt)",
		"goǃ(Ꮡt.loopArg, (nint)(9));",
		"internal static partial void startInner(this ж<tracker> Ꮡt)",
		"goǃ(Ꮡt.of(tracker.Ꮡin).send);",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q: a go statement's box method group needs the receiver box in scope:\n%s", want, mainCs)
		}
	}

	// CONTROLS: a deferred call already promoted, and a pointer PARAMETER has its own box.
	for _, want := range []string{
		"internal static void stop(this ж<tracker> Ꮡt)",
		"defer(Ꮡt.bump, ref ᒐ);",
		"[GoRecv] internal static partial void other(this ref tracker t, ж<tracker> Ꮡo)",
		"goǃ(Ꮡo.loop);",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control: want %q unchanged:\n%s", want, mainCs)
		}
	}
}
