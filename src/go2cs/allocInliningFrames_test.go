// allocInliningFrames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the THIN-ALLOCATOR seed of computeNoInliningClosure (callerInliningAnalysis.go): in a package
// that reads the heap profile, a function whose one statement is one allocation is emitted
// [MethodImpl(NoInlining)], so a Release-tier JIT cannot inline it into its caller and drop its frame
// from the allocation's sampled stack. runtime/pprof's genericAllocFunc (`return make([]T, n)`) is the
// measured case: TestGenericsHashKeyInPprofBuilder wants `...;runtime/pprof.genericAllocFunc[...]` and
// read the stack without it.
//
// The CONTROLS carry the scope: a thin function that allocates nothing, an allocating function that is
// not thin, and a thin allocator in a package that reads no heap profile all stay unmarked -- the
// attribute is a cost, paid only where a heap profile can observe the frame it protects.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertAllocInliningFixture(t *testing.T) (mainCs, libCs string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/allocframes\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"bytes"
	"runtime/pprof"

	"example.com/allocframes/lib"
)

type box struct{ v int }

// POSITIVE 1 -- runtime/pprof's genericAllocFunc shape.
func genericAlloc[T ~uint32 | ~uint64](n int) []T { return make([]T, n) }

// POSITIVE 2 -- a heap-escaping composite literal.
func newBox() *box { return &box{} }

// CONTROL 1 -- thin, but allocates nothing.
func add(a, b int) int { return a + b }

// CONTROL 2 -- allocates, but is not thin.
func twoStep(n int) []byte {
	b := make([]byte, n)
	return b
}

func main() {
	_ = genericAlloc[uint32](4)
	_ = newBox()
	_ = add(1, 2)
	_ = twoStep(8)
	_ = lib.Alloc(8)
	var buf bytes.Buffer
	_ = pprof.WriteHeapProfile(&buf)
}
`)
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

// CONTROL 3 -- a thin allocator in a package that reads no heap profile.
func Alloc(n int) []byte { return make([]byte, n) }
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

	out := filepath.Join(options.go2csPath, "src", "example.com", "allocframes")

	return readGenerated(t, filepath.Join(out, "main.cs")), readGenerated(t, filepath.Join(out, "lib", "lib.cs"))
}

// declarationLine returns the emitted line that declares the named function, so each arm reads ITS
// OWN declaration -- a whole-file Contains would let one marked function satisfy every arm.
func declarationLine(t *testing.T, cs, name string) string {
	t.Helper()

	for _, line := range strings.Split(cs, "\n") {
		if strings.Contains(line, " static ") && (strings.Contains(line, " "+name+"(") || strings.Contains(line, " "+name+"<")) {
			return strings.TrimSpace(line)
		}
	}

	t.Fatalf("no emitted declaration of %q; emission:\n%s", name, cs)

	return ""
}

// keepsOwnFrame reports whether an emitted declaration line carries the no-inline mark. A declared
// function with a body takes it as a partial method's implementing part -- the word `partial` where the
// [MethodImpl(MethodImplOptions.NoInlining)] prefix stood -- and go2cs-gen writes the declaring part
// that carries the attribute (owner ruling 2026-10-06). A bodyless `partial` declaration ends in `;`
// and is not a carrier.
func keepsOwnFrame(line string) bool {
	return strings.Contains(line, " static partial ") && !strings.HasSuffix(strings.TrimSpace(line), ";")
}

func TestThinAllocatorsInAHeapProfileReaderAreNotInlined(t *testing.T) {
	mainCs, libCs := convertAllocInliningFixture(t)

	for _, name := range []string{"genericAlloc", "newBox"} {
		if line := declarationLine(t, mainCs, name); !keepsOwnFrame(line) {
			t.Errorf("thin allocator %s in a heap-profile reader is not marked: %s", name, line)
		}
	}

	for _, name := range []string{"add", "twoStep"} {
		if line := declarationLine(t, mainCs, name); keepsOwnFrame(line) {
			t.Errorf("%s is marked, but it is not a thin allocator: %s", name, line)
		}
	}

	if line := declarationLine(t, libCs, "Alloc"); keepsOwnFrame(line) {
		t.Errorf("lib.Alloc is marked, but its package reads no heap profile: %s", line)
	}
}
