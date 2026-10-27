// crosspkgExportedBox_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the cross-package exported-variable box (go.* 1.26.10.2's headline,
// docs/phase4/DESIGN-crosspkg-exported-box.md). Go hands an importer the variable itself, so the OWNER boxes every
// exported value-kind var of an importable package (markExportedBoxOnlyGlobals), and the IMPORTER binds that box for
// `&pkg.V`, a pointer-receiver method value, and a field or element address inside the var. This drives a real module
// conversion and reads the EMITTED sources, never the predicate alone.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertCrossPackageBoxFixture(t *testing.T) (libCs, mainCs string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/xpbox\n\ngo 1.23\n")

	// store holds the storage a lib var pulls by //go:linkname: lib.Pulled is a FORWARD, never a box.
	writeModuleFile(t, filepath.Join(appDir, "store", "store.go"), "package store\n\n"+
		"import _ \"unsafe\" // for go:linkname\n\n"+
		"//go:linkname held\n"+
		"var held = 7\n\n"+
		"// Held reads the storage.\n"+
		"func Held() int { return held }\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), "package lib\n\n"+
		"import (\n\t_ \"unsafe\" // for go:linkname\n\n\t\"example.com/xpbox/store\"\n)\n\n"+
		"type Counter struct{ n int }\n\n"+
		"func (c *Counter) Inc() { c.n++ }\n\n"+
		"type Holder struct{ N int }\n\n"+
		"var N int\n"+
		"var V Counter\n"+
		"var S Holder\n"+
		"var Arr [3]int\n"+
		"var P *int\n"+
		"var E error\n\n"+
		"//go:linkname Pulled example.com/xpbox/store.held\n"+
		"var Pulled int\n\n"+
		"// Touch keeps store imported.\n"+
		"func Touch() int { return store.Held() }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\n"+
		"import (\n\t\"fmt\"\n\n\t\"example.com/xpbox/lib\"\n)\n\n"+
		"var Debug bool\n\n"+
		"func main() {\n"+
		"\tp := &lib.N\n"+
		"\ty := 9\n"+
		"\tp = &y\n"+
		"\tf := lib.V.Inc\n"+
		"\tf()\n"+
		"\tq := &lib.S.N\n"+
		"\te := &lib.Arr[1]\n"+
		"\tpp := &lib.P\n"+
		"\tee := &lib.E\n"+
		"\tfmt.Println(*p, *q, *e, pp, ee, Debug, lib.Pulled, lib.Touch())\n"+
		"}\n")

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

	out := filepath.Join(options.go2csPath, "src", "example.com", "xpbox")

	lib, err := os.ReadFile(filepath.Join(out, "lib", "lib.cs"))

	if err != nil {
		t.Fatalf("reading the emitted lib.cs: %v", err)
	}

	main, err := os.ReadFile(filepath.Join(out, "main.cs"))

	if err != nil {
		t.Fatalf("reading the emitted main.cs: %v", err)
	}

	return string(lib), string(main)
}

// TestExportedValueVarIsBoxedAcrossPackages pins the OWNER rule and the IMPORTER's bindings together: every exported
// value-kind var of the library is a readonly StaticBox read through the sealed type's ValueSlot; a pointer, an
// interface, a linkname forward and package main's capitalized var keep their old forms; and main's `&` sites, its
// method value, its field address and its element address all bind the library's box. The re-point (`p = &y`) must
// compile: the box keeps its declared type ж<T>.
func TestExportedValueVarIsBoxedAcrossPackages(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	lib, main := convertCrossPackageBoxFixture(t)

	for _, want := range []string{
		"public static readonly ж<nint> ᏑN = new StaticBox<nint>(",
		"public static ref nint N => ref ((StaticBox<nint>)ᏑN).ValueSlot;",
		"public static readonly ж<Counter> ᏑV = new StaticBox<Counter>(",
		"public static readonly ж<Holder> ᏑS = new StaticBox<Holder>(",
		"public static readonly ж<array<nint>> ᏑArr = new StaticBox<array<nint>>(",
	} {
		if !strings.Contains(lib, want) {
			t.Errorf("the library does not box its exported value var; want %q in:\n%s", want, lib)
		}
	}

	for _, refused := range []string{"ᏑP", "ᏑE", "ᏑPulled"} {
		if strings.Contains(lib, refused) {
			t.Errorf("the library boxes %s, which the rule leaves alone (a pointer, an interface, a linkname forward):\n%s", refused, lib)
		}
	}

	if !strings.Contains(lib, "Pulled { get =>") {
		t.Errorf("the linkname pull lost its forwarding property:\n%s", lib)
	}

	for _, want := range []string{
		"var p = lib.ᏑN;",
		"lib.ᏑV.Inc",
		"lib.ᏑS.of(lib.Holder.ᏑN)",
		"lib.ᏑArr.at<nint>(1)",
	} {
		if !strings.Contains(main, want) {
			t.Errorf("the importer does not bind the library's box; want %q in:\n%s", want, main)
		}
	}

	if strings.Contains(main, "Ꮡlib.") {
		t.Errorf("the importer composed the box glyph onto the package alias (Ꮡlib.…, a name nothing declares):\n%s", main)
	}

	if strings.Contains(main, "ᏑDebug") {
		t.Errorf("package main's capitalized var is boxed; nothing can import it:\n%s", main)
	}
}
