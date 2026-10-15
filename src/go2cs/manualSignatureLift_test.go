// manualSignatureLift_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the manual branch of visitFuncDecl: a declaration manualConversionFuncs displaces still has
// Go's SIGNATURE, so an anonymous interface or struct in it is lifted and published exactly as the
// converted declaration would lift it. runtime's ifaceHash is the measured case: its parameter
// `i interface{ F() }` lifts as ifaceHash_i, export_test.go names the type through it, and with the
// function hand-owned the lift and its GoDynamicTypeLift record were gone, so the -tests conversion
// failed with an unresolved dynamic type at export_test.cs(273).
//
// The CONTROLS carry the scope: a displaced declaration whose signature has no anonymous type emits
// the placeholder alone, and a converted declaration's lift is unchanged.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

const manualSigFixturePath = "example.com/manualsig"

func convertManualSignatureFixture(t *testing.T) (mainCs, packageInfo string) {
	t.Helper()

	// Stand the fixture package in the registry, copy-on-write, for this test only.
	previous := manualConversionFuncs
	extended := make(map[string]map[string]goosScope, len(previous)+1)

	for pkg, funcs := range previous {
		extended[pkg] = funcs
	}

	extended[manualSigFixturePath] = map[string]goosScope{
		"hashIface": goosAny,
		"plainHash": goosAny,
	}

	manualConversionFuncs = extended
	t.Cleanup(func() { manualConversionFuncs = previous })

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module "+manualSigFixturePath+"\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type thing struct{}

func (thing) F() {}

// POSITIVE -- displaced, and its parameter is an anonymous interface.
func hashIface(i interface{ F() }, seed uintptr) uintptr { return seed }

// CONTROL 1 -- displaced, with no anonymous type in its signature.
func plainHash(seed uintptr) uintptr { return seed }

// CONTROL 2 -- converted, with an anonymous interface parameter: its lift is unchanged.
func convertedIface(i interface{ G() }) {}

func main() {
	_ = hashIface(thing{}, 1)
	_ = plainHash(2)
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

	out := filepath.Join(options.go2csPath, "src", "example.com", "manualsig")

	return readGenerated(t, filepath.Join(out, "main.cs")), readGenerated(t, filepath.Join(out, "package_info.cs"))
}

// liftDeclared reports whether cs declares a lifted type of the given name.
func liftDeclared(cs, name string) bool {
	for _, line := range strings.Split(cs, "\n") {
		if strings.Contains(line, " interface "+name) || strings.Contains(line, " struct "+name) {
			return true
		}
	}

	return false
}

// liftPublished reports whether packageInfo carries a GoDynamicTypeLift record naming the type.
func liftPublished(packageInfo, name string) bool {
	for _, line := range strings.Split(packageInfo, "\n") {
		if strings.Contains(line, "[assembly: GoDynamicTypeLift(") && strings.Contains(line, `"`+name+`")]`) {
			return true
		}
	}

	return false
}

func TestManualDeclarationStillLiftsItsSignatureTypes(t *testing.T) {
	isolateConverterPathGlobals(t)
	mainCs, packageInfo := convertManualSignatureFixture(t)

	for _, name := range []string{"hashIface", "plainHash"} {
		if !strings.Contains(mainCs, funcPlaceholderLead+name+" ") {
			t.Fatalf("%s is not displaced; the fixture's registration did not take:\n%s", name, mainCs)
		}
	}

	if !liftDeclared(mainCs, "hashIface_i") {
		t.Errorf("the displaced hashIface's anonymous parameter type is not lifted as hashIface_i:\n%s", mainCs)
	}

	if !liftPublished(packageInfo, "hashIface_i") {
		t.Errorf("no GoDynamicTypeLift record publishes hashIface_i:\n%s", packageInfo)
	}

	if strings.Contains(mainCs, "plainHash_") {
		t.Errorf("the displaced plainHash has no anonymous type, but something was lifted for it:\n%s", mainCs)
	}

	if !liftDeclared(mainCs, "convertedIface_i") || !liftPublished(packageInfo, "convertedIface_i") {
		t.Errorf("a converted declaration's lift changed:\n%s\n%s", mainCs, packageInfo)
	}
}
