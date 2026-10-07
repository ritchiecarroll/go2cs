// noinlineDirective_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the //go:noinline seed of computeNoInliningClosure (callerInliningAnalysis.go): a function
// Go's source marks //go:noinline is emitted [MethodImpl(NoInlining)], so a Release-tier JIT keeps its
// frame as Go's compiler does. runtime's unexportedPanicForTesting is the measured case: under the
// Release TieredCompilation=0 default the JIT inlined it into the caller, the index panic it raises no
// longer came from a runtime frame, and TestRuntimePanic's fatal became a recoverable panic.
//
// The CONTROLS carry the scope: the directive protects the one frame it names, so a thin forwarder TO
// a directed function stays unmarked; another directive, or the word in ordinary comment text, marks
// nothing; and a directed function is not a runtime.Caller user, so it does not switch on the
// opaque-forwarder marks in a package that has none.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"testing"
)

func convertNoinlineDirectiveFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/noinlinedirective\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type counter struct{ n int }

type sinkFunc func(int)

// POSITIVE 1 -- a directed function too small for anything else to mark.
//
//go:noinline
func mid(b []byte, i int) byte {
	v := b[i]
	return v
}

// POSITIVE 2 -- a directed method on a pointer receiver.
//
//go:noinline
func (c *counter) bump() {
	c.n++
}

// CONTROL 1 -- a thin forwarder TO a directed function.
func fwd(b []byte, i int) byte { return mid(b, i) }

// CONTROL 2 -- another directive.
//
//go:nosplit
func split(a int) int { return a + 1 }

// CONTROL 3 -- the word in ordinary comment text: see go:noinline.
func prose(a int) int { return a * 2 }

// CONTROL 4 -- an opaque forwarder in a package with no runtime.Caller user.
func (f sinkFunc) Put(v int) { f(v) }

func main() {
	b := []byte{1, 2}
	_ = mid(b, 0)
	_ = fwd(b, 1)
	c := &counter{}
	c.bump()
	_ = split(1)
	_ = prose(1)
	sinkFunc(func(int) {}).Put(1)
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "noinlinedirective", "main.cs"))
}

func TestNoinlineDirectiveKeepsTheFunctionsOwnFrame(t *testing.T) {
	mainCs := convertNoinlineDirectiveFixture(t)

	for _, name := range []string{"mid", "bump"} {
		if line := declarationLine(t, mainCs, name); !keepsOwnFrame(line) {
			t.Errorf("//go:noinline function %s is not marked: %s", name, line)
		}
	}

	for _, name := range []string{"fwd", "split", "prose", "Put"} {
		if line := declarationLine(t, mainCs, name); keepsOwnFrame(line) {
			t.Errorf("%s is marked, but Go's source does not mark it //go:noinline: %s", name, line)
		}
	}
}
