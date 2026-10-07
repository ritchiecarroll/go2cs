// goCreatorFrame_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the GO-CREATOR seed of computeNoInliningClosure (callerInliningAnalysis.go): a function whose
// OWN body executes a `go` statement keeps its frame, because golib names a goroutine's creator by
// walking the launching thread's stack (Go's gp.gopc, printed as `created by <func>`). Under the
// Release TieredCompilation=0 JIT that frame was elided two ways: inlined into its caller, or -- for a
// `go` in tail position -- replaced by an opportunistic tail call into goǃ. Measured 2026-10-01: unique's
// map-cleanup goroutine read `created by sync.(*Once).doSlow` at TC0 (the runtime frame that executes
// the `go` was tail-called away behind a delegate invoke), and a probe showed [MethodImpl(NoInlining)]
// suppresses BOTH, on a method and on a C# lambda or local function alike.
//
// Only the frame that executes the `go` is the creator: its callers are not marked (no fixed point), a
// function whose only `go` sits inside a nested literal is not marked (the literal is the creator), and
// a literal that IS the goroutine body is not marked (it is the start function, not the creator).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertGoCreatorFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/gocreator\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

var done = make(chan int, 16)

func work(n int) { done <- n }

// POSITIVE 1 -- the go in tail position: the shape a tail call into goǃ elides.
func launchTail() {
	go work(1)
}

// POSITIVE 2 -- the go mid-body: the shape inlining into the caller elides.
func launchMid(n int) int {
	go work(n)
	n++
	return n
}

type pool struct{ size int }

// POSITIVE 3 -- a method executing a go.
func (p *pool) start() {
	for i := 0; i < p.size; i++ {
		go work(i)
	}
}

// POSITIVE 4 -- executes a go whose callee is a literal; the literal is the goroutine body.
func spawnLit() {
	go func() {
		done <- 4
	}()
}

// CONTROL 1 -- only CALLS a launcher: a caller, never the creator.
func callsLauncher() {
	launchTail()
}

// CONTROL 2 -- its only go sits inside a nested literal, which is that go's creator (POSITIVE 5).
func withLit() {
	f := func() {
		go work(5)
	}
	f()
}

// CONTROL 3 -- executes no go at all.
func plain(n int) int {
	return n * 2
}

func main() {
	launchTail()
	_ = launchMid(2)
	(&pool{size: 2}).start()
	spawnLit()
	callsLauncher()
	withLit()
	_ = plain(3)
	for i := 0; i < 7; i++ {
		<-done
	}
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "gocreator", "main.cs"))
}

func TestGoCreatorFramesAreNotInlined(t *testing.T) {
	mainCs := convertGoCreatorFixture(t)

	// A func literal cannot be partial, so a marked literal keeps the attribute itself.
	const attribute = "[MethodImpl(MethodImplOptions.NoInlining)]"

	for _, name := range []string{"launchTail", "launchMid", "start", "spawnLit"} {
		if line := declarationLine(t, mainCs, name); !keepsOwnFrame(line) {
			t.Errorf("%s executes a go statement, but its frame is not kept: %s", name, line)
		}
	}

	for _, name := range []string{"callsLauncher", "withLit", "plain"} {
		if line := declarationLine(t, mainCs, name); keepsOwnFrame(line) {
			t.Errorf("%s executes no go statement of its own, but is marked: %s", name, line)
		}
	}

	// POSITIVE 5 and CONTROL 4: inside withLit, the literal that executes `go work(5)` is marked; inside
	// spawnLit, the literal that IS the goroutine body is not.
	withLitBody := bodyOf(t, mainCs, "withLit")
	if !strings.Contains(withLitBody, attribute) {
		t.Errorf("withLit's literal executes a go statement, but it is not marked:\n%s", withLitBody)
	}

	spawnLitBody := bodyOf(t, mainCs, "spawnLit")
	if strings.Contains(spawnLitBody, attribute) {
		t.Errorf("spawnLit's literal is the goroutine body, not its creator, but it is marked:\n%s", spawnLitBody)
	}
}

// bodyOf returns name's emitted method body: from its declaration line to the closing brace at the
// declaration's own indentation (package functions are members of the package class).
func bodyOf(t *testing.T, source, name string) string {
	t.Helper()

	line := declarationLine(t, source, name)

	for _, raw := range strings.Split(source, "\n") {
		if strings.TrimSpace(raw) != line {
			continue
		}

		indent := raw[:len(raw)-len(strings.TrimLeft(raw, " \t"))]
		start := strings.Index(source, raw)
		rest := source[start+len(raw):]

		if end := strings.Index(rest, "\n"+indent+"}"); end >= 0 {
			return rest[:end]
		}

		return rest
	}

	t.Fatalf("declaration of %s not found", name)

	return ""
}
