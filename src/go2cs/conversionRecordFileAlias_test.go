// conversionRecordFileAlias_test.go - Gbtc
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
	"strings"
	"testing"
)

// A conversion recorded from a file that imports the target's package under an EXPLICIT alias names
// the type through that alias (`st.AssignB`), because type names render with the file's own import
// alias. package_info.cs resolves a recorded conversion's qualifiers with file-local usings, which
// were registered under the PACKAGE NAME (`using structs = …`), so the record named an alias nothing
// declared (CS0246 'st'). go-cmp's internal test reached it through `ts "…/internal/teststructs"`.
// Every qualifier a record carries must be declared in the same file.
func TestConversionRecordQualifiersAreDeclaredInPackageInfo(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/convalias\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "structs", "structs.go"), "package structs\n\ntype AssignB struct{ A int }\n\nfunc (x AssignB) Sum() int { return x.A * 10 }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	st "example.com/convalias/structs"
)

func main() {
	b := st.AssignB(struct{ A int }{3})
	fmt.Println(b.A, b.Sum())
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

	packageInfo := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "convalias", "package_info.cs"))

	if !strings.Contains(packageInfo, "GoImplicitConv<main_type, st.AssignB>") {
		t.Fatalf("expected the conversion record through the file's alias, GoImplicitConv<main_type, st.AssignB>:\n%s", packageInfo)
	}

	declared := map[string]bool{}

	for _, m := range regexp.MustCompile(`(?m)^(?:global )?using (\w+) = `).FindAllStringSubmatch(packageInfo, -1) {
		declared[m[1]] = true
	}

	for _, record := range regexp.MustCompile(`GoImplicitConv<([^>]*)>`).FindAllStringSubmatch(packageInfo, -1) {
		for _, qualifier := range regexp.MustCompile(`\b(\w+)\.\w+`).FindAllStringSubmatch(record[1], -1) {
			if !declared[qualifier[1]] {
				t.Errorf("record GoImplicitConv<%s> names %s, which package_info.cs does not declare:\n%s", record[1], qualifier[1], packageInfo)
			}
		}
	}
}
