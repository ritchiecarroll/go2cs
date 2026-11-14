// foreignAliasElementAddress_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// Another package's alias of a defined struct (modernc.org/libc's `stdio.FILE`, `type FILE = X_iobuf`, sqlite-cs
// S12) is spelled through the importer's alias table wherever the address machinery names the element or field
// owner type: `&X_iob[i]` and `&X_iob[i].Fcnt` over `[3]stdio.FILE`. The raw Go name `stdio.FILE` names no member of
// stdio's package class (CS0426).
func TestForeignAliasOfNamedSpelledInAddressPaths(t *testing.T) {
	isolateConverterPathGlobals(t)

	lib := `package stdio

type X_iobuf struct {
	Fptr int64
	Fcnt int32
}

type FILE = X_iobuf
`
	app := `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/stdio"
)

var X_iob [3]stdio.FILE

func main() {
	m := map[uintptr]int32{}
	for i := range X_iob {
		m[uintptr(unsafe.Pointer(&X_iob[i]))] = int32(i)
	}
	p := &X_iob[2].Fcnt
	*p = 5
	fmt.Println(len(m), X_iob[2].Fcnt)
}
`
	emitted := convertModuleMainWithLib(t, "stdio", lib, app)

	if strings.Contains(emitted, "stdio.FILE") {
		t.Errorf("the alias is written by its Go name:\n%s", emitted)
	}

	if !strings.Contains(emitted, ".at<stdioꓸFILE>(i)") {
		t.Errorf("the element address does not name stdioꓸFILE:\n%s", emitted)
	}
}

// convertModuleMainWithLib converts module example.com/app, a main package importing one library package in
// directory libDir, the way -recurse converts a module, and returns the emitted main.cs.
func convertModuleMainWithLib(t *testing.T, libDir, libSource, mainSource string) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, libDir, "lib.go"), libSource)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), mainSource)

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

	emitted, err := os.ReadFile(filepath.Join(options.go2csPath, "src", "example.com", "app", "main.cs"))

	if err != nil {
		t.Fatalf("reading the emitted main.cs: %v", err)
	}

	return strings.ReplaceAll(string(emitted), "\r", "")
}
