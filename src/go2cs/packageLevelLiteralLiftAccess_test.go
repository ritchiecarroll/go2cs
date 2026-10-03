// packageLevelLiteralLiftAccess_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards an anonymous struct built as a VALUE in a package-level var initializer (yaml.v3's decode_test.go,
// `&struct{ A int; C inlineB }{…}` inside an `interface{}` field, COORD 2026-10-03). Its lift takes the
// placeholder name `Δtype`, which is public, and its exported field `C` then held the internal `inner` (CS0052,
// with CS0050/CS0051 on the generated members). The field type is publicized, as a publicized lift's field types
// are. The controls hold an unexported field's type and a literal inside a function body internal.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestAPackageLevelLiteralStructsExportedFieldTypeIsPublic(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/pla\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type inner struct{ X int }

type hidden struct{ Y int }

type local struct{ Z int }

var tests = []struct {
	data  string
	value any
}{
	{"a", &struct {
		A int
		C inner
		h hidden
	}{1, inner{2}, hidden{3}}},
}

func main() {
	v := &struct{ L local }{local{4}}
	fmt.Println(tests[0].data, tests[0].value, v.L.Z)
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

	packageInfo := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "pla", "package_info.cs"))

	// The public lift's exported field type is publicized.
	if want := "public partial struct inner {}"; !strings.Contains(packageInfo, want) {
		t.Errorf("want %q: the public lift's exported field C holds inner (CS0052):\n%s", want, packageInfo)
	}

	// CONTROLS: an unexported field's type, and a literal inside a function body, stay internal.
	for _, want := range []string{
		"public partial struct Δtype {}",
		"internal partial struct hidden {}",
		"internal partial struct local {}",
	} {
		if !strings.Contains(packageInfo, want) {
			t.Errorf("control: want %q unchanged:\n%s", want, packageInfo)
		}
	}
}
