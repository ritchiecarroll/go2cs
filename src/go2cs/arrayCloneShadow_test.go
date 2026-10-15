// arrayCloneShadow_test.go - Gbtc
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
	"strings"
	"testing"
)

// A NAMED ARRAY type that declares its own Go `Clone` method -- Go 1.26 hash/fnv's
// `func (d *sum128) Clone() (hash.Cloner, error) { r := *d; return &r, nil }` over `type sum128 [2]uint64`
// -- must not have its by-value copy spelled `.Clone()`. That name is the generated wrapper's array
// copy, an INSTANCE member, and C# binds an instance member before the extension method Go's own
// Clone becomes: so the wrapper's member shadowed Go's method, and RecvGenerator's pointer forwarder
// `return d.Clone();` bound the array copy (CS0029, sum128 to (hash.Cloner, error), H7 pass 5). The
// copy now uses ΔClone (ValueCloneMethod), the name a struct's copy already takes for the same reason,
// and the wrapper drops its public Clone() for such a type (IArrayTypeTemplate). A named array WITHOUT
// a Go Clone keeps `.Clone()`: the control.

func TestNamedArrayWithGoCloneCopiesThroughValueCloneMethod(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/arrclone\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type sum [2]uint64

type plain [2]uint64

// The fnv shape: a by-value copy inside the type's own Clone method.
func (d *sum) Clone() (any, error) {
	r := *d
	return &r, nil
}

func copyPlain(p *plain) plain {
	q := *p
	return q
}

func main() {
	var s sum
	c, _ := s.Clone()
	fmt.Println(c, copyPlain(&plain{1, 2}))
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

	cs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "arrclone", "main.cs"))

	if !strings.Contains(cs, "r = d."+ValueCloneMethod+"();") {
		t.Errorf("the copy of a named array that declares Go's Clone is not spelled %s():\n%s", ValueCloneMethod, cs)
	}

	if strings.Contains(cs, "r = d.Clone();") {
		t.Errorf("the copy binds `.Clone()`, the name the type's own Go method also has:\n%s", cs)
	}

	if !strings.Contains(cs, "var q = p.Clone();") {
		t.Errorf("CONTROL: a named array without a Go Clone no longer copies through `.Clone()`:\n%s", cs)
	}
}
