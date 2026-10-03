// anonInterfaceParamPublic_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards an ANONYMOUS interface in an exported func's signature (go-cmp's `func Reporter(r interface{…})
// Option`, COORD 2026-10-03): the publicize pre-pass records the lift, and the interface emitter must honor it
// exactly as the struct emitter does, or the `internal` lift is less accessible than the public method taking it
// (CS0051). The controls hold an unexported func's lift internal and an exported func's anonymous struct
// parameter unchanged.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestAnExportedFuncsAnonymousInterfaceParameterIsPublic(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/aip\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type named struct{}

func (named) Name() string  { return "n" }
func (named) Label() string { return "l" }

func Report(r interface{ Name() string }) string { return r.Name() }

func report(r interface{ Label() string }) string { return r.Label() }

func Shape(p struct{ X int }) int { return p.X }

func main() {
	println(Report(named{}), report(named{}), Shape(struct{ X int }{1}))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "aip", "main.cs"))
	packageInfo := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "aip", "package_info.cs"))

	if want := `[GoType("dyn")] public partial interface Report_r`; !strings.Contains(mainCs, want) {
		t.Errorf("want %q: the exported func's anonymous interface parameter must be public (CS0051):\n%s", want, mainCs)
	}

	if want := "public partial interface Report_r {}"; !strings.Contains(packageInfo, want) {
		t.Errorf("want %q in package_info.cs:\n%s", want, packageInfo)
	}

	// CONTROLS: an unexported func's lift stays internal (a DIFFERENT anonymous interface: identical ones share
	// one lift), and the anonymous STRUCT twin was already public.
	if want := `[GoType("dyn")] internal partial interface report_r`; !strings.Contains(mainCs, want) {
		t.Errorf("control: want %q unchanged:\n%s", want, mainCs)
	}

	if !strings.Contains(mainCs, "public partial struct Shape_p") {
		t.Errorf("control: the exported func's anonymous struct parameter must stay public:\n%s", mainCs)
	}
}
