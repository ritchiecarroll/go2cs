// foreignWrittenRHS_test.go - Gbtc
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

// A conversion to a type defined over ANOTHER package's named number type, written OUTSIDE the
// defining package: logrus hooks/slog's external test converts `logrus.Level` to its own
// `type Level logrus.Level` and `slog.Level` to `type SlogLevel slog.Level`. The [GoType] wrapper of
// such a type keeps its NAMED base and declares the one-step operator from it, so the plain cast binds;
// the converter knew that only for its own package's types (the written RHS was read from the current
// package's syntax alone), so a foreign target hopped through the number -- `((ΔLevel)(uint32)x)`,
// whose second leg has no operator (CS0030). The answer must not depend on which package converts.
func TestForeignTargetWrittenOverANamedNumberCastsDirectly(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/foreignrhs\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "base", "base.go"), "package base\n\ntype N uint32\n\ntype Signed int\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

import "example.com/foreignrhs/base"

type L base.N

func (l L) Get() uint32 { return uint32(l) }

type S base.Signed

func (s S) Get() int { return int(s) }

// The same conversions INSIDE the defining package: the control, which already cast directly.
func FromN(n base.N) L { return L(n) }

func FromSigned(n base.Signed) S { return S(n) }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/foreignrhs/base"
	"example.com/foreignrhs/lib"
)

func main() {
	n := base.N(7)
	k := base.Signed(-3)
	fmt.Println(lib.L(n).Get(), lib.S(k).Get(), lib.FromN(n).Get(), lib.FromSigned(k).Get())
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

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "foreignrhs")
	mainCs := readGenerated(t, filepath.Join(outDir, "main.cs"))
	libCs := readGenerated(t, filepath.Join(outDir, "lib", "lib.cs"))

	// The control: inside lib, both conversions cast directly.
	for _, want := range []string{"return ((L)n);", "return ((S)n);"} {
		if !strings.Contains(libCs, want) {
			t.Errorf("control: missing %q in lib:\n%s", want, libCs)
		}
	}

	// Outside lib: the same direct casts, never through the bare number.
	for _, wrong := range []string{"(uint32)n", "(nint)k"} {
		if strings.Contains(mainCs, wrong) {
			t.Errorf("a foreign target hops through the number (%q) in:\n%s", wrong, mainCs)
		}
	}

	for _, want := range []string{"((lib.L)n)", "((lib.S)k)"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("missing the direct cast %q in:\n%s", want, mainCs)
		}
	}
}
