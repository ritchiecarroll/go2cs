// genericAllocCallerFrames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"testing"
)

// runtime/pprof's TestGenericsInlineLocations reads a heap-profile stack through two frames of a
// GENERIC function that calls a seeded thin allocator (nonRecursiveGenericAllocFunction -> storeAlloc).
// The allocator keeps its frame (M4 (a)), but the generic caller is not thin, so nothing marks it, and
// under the Release TieredCompilation=0 default the JIT inlines the specialized instantiation into its
// caller: the profile reads TestGenericsInlineLocations;storeAlloc with both generic frames gone
// (measured, and restored by NoInlining on that one function).
func convertGenericAllocCallerFixture(t *testing.T) (mainCs, libCs string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/genericcallers\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"bytes"
	"runtime/pprof"

	"example.com/genericcallers/lib"
)

var sink []byte

// The seeded thin allocator (M4 (a)): one allocation, in a package that reads the heap profile.
func storeAlloc() { sink = make([]byte, 16) }

type opA struct{}
type opB struct{ buf [128]byte }

// POSITIVE -- a generic function that directly calls the seeded thin allocator.
func genericCaller[Cur any, Other any](alloc bool) {
	if alloc {
		storeAlloc()
	} else {
		genericCaller[Other, Cur](true)
	}
}

// CONTROL 1 -- the same shape, not generic.
func plainCaller(alloc bool) {
	if alloc {
		storeAlloc()
	} else {
		plainCaller(true)
	}
}

func main() {
	genericCaller[opA, opB](false)
	plainCaller(false)
	lib.GenericCaller[int](true)
	var buf bytes.Buffer
	_ = pprof.WriteHeapProfile(&buf)
}
`)
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

var sink []byte

func alloc() { sink = make([]byte, 16) }

// CONTROL 2 -- a generic caller of a thin allocator in a package that reads no heap profile, where no
// thin allocator is seeded.
func GenericCaller[T any](a bool) {
	if a {
		alloc()
	} else {
		GenericCaller[T](true)
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

	out := filepath.Join(options.go2csPath, "src", "example.com", "genericcallers")

	return readGenerated(t, filepath.Join(out, "main.cs")), readGenerated(t, filepath.Join(out, "lib", "lib.cs"))
}

func TestGenericCallersOfASeededThinAllocatorAreNotInlined(t *testing.T) {
	mainCs, libCs := convertGenericAllocCallerFixture(t)

	if line := declarationLine(t, mainCs, "storeAlloc"); !keepsOwnFrame(line) {
		t.Fatalf("precondition: the thin allocator storeAlloc is not seeded: %s", line)
	}

	if line := declarationLine(t, mainCs, "genericCaller"); !keepsOwnFrame(line) {
		t.Errorf("genericCaller, a generic caller of the seeded thin allocator, is not marked: %s", line)
	}

	if line := declarationLine(t, mainCs, "plainCaller"); keepsOwnFrame(line) {
		t.Errorf("plainCaller is marked, but it is not generic: %s", line)
	}

	if line := declarationLine(t, libCs, "GenericCaller"); keepsOwnFrame(line) {
		t.Errorf("lib.GenericCaller is marked, but its package reads no heap profile: %s", line)
	}
}
