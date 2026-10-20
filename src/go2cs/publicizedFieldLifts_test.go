// publicizedFieldLifts_test.go - Gbtc
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
	"regexp"
	"runtime"
	"testing"
)

// crypto/tls's test half at Go 1.26 (found and rooted by P1): `type reportData struct { Results bogoResults; … }`
// publicizes the unexported bogoResults through an exported field, and bogoResults' exported field
// `Tests map[string]struct{ Actual, Expected string; IsUnexpected bool; Error string }` lifts to bogoResults_Tests.
// The lift stayed internal, so the public struct's public field named an internal type: CS0052 on the field and
// CS0050/CS0051 on the members TypeGenerator generates from it. A publicized struct's exported fields now carry their
// lifts, as a public signature's types do.
//
// Control: an unexported struct that nothing publicizes keeps its exported field's lift internal.

func TestAPublicizedStructsExportedFieldCarriesItsLift(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/pfl\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type bogoResults struct {
	Version int
	Tests   map[string]struct {
		Actual, Expected string
		IsUnexpected     bool
	}
}

type reportData struct {
	Results bogoResults
}

type quiet struct {
	Rows map[string]struct{ Name string }
}

func main() {
	r := reportData{}
	q := quiet{}
	fmt.Println(r.Results.Version, len(r.Results.Tests), len(q.Rows))
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

	dir := filepath.Join(options.go2csPath, "src", "example.com", "pfl")
	mainCS := readGenerated(t, filepath.Join(dir, "main.cs"))
	info := readGenerated(t, filepath.Join(dir, "package_info.cs"))

	for _, arm := range []struct {
		name, text, want string
	}{
		{"the premise: reportData's exported field publicizes bogoResults", info, "public partial struct bogoResults {}"},
		{"bogoResults' exported field carries its lift", info, "public partial struct bogoResults_Tests {}"},
		{"and the lift is declared public", mainCS, "public partial struct bogoResults_Tests"},
		{"control: nothing publicizes quiet, so its lift stays internal", info, "internal partial struct quiet_Rows {}"},
	} {
		if !regexp.MustCompile(regexp.QuoteMeta(arm.want)).MatchString(arm.text) {
			t.Errorf("%s: missing %s", arm.name, arm.want)
		}
	}
}
