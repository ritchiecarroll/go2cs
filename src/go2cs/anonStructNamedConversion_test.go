// anonStructNamedConversion_test.go - Gbtc
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

// A conversion from an ANONYMOUS struct to a NAMED struct with identical fields needs a recorded
// GoImplicitConv: the anonymous struct is lifted to its own C# type, and nothing else declares an
// operator between it and the named one. Two gates declined the record. The same-package target
// read as a [GoType] wrapper conversion, because `type local struct{ A int }` has a written RHS
// identical to the source; a struct-literal RHS provides no such operator. The cross-package
// target saw no operand declared in this package, although the lifted source type is. Both cast
// sites were CS0030 (go-cmp's teststructs `ts.AssignB(struct{ A int }{0})`).
//
// A pair that is not a Go conversion records nothing: the argument-recording loop compares every
// argument with the callee's FIRST parameter, so `use(nil, &s)` presents the lifted `struct{ X
// string }` against `*structs.AssignB`, which recorded a pointer-box conversion of the lifted type.
func TestAnonymousStructToNamedStructConversionIsRecorded(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/anonnamed\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "structs", "structs.go"), `package structs

type AssignB struct{ A int }

func (x AssignB) Sum() int { return x.A }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/anonnamed/structs"
)

type local struct{ A int }

func use(b *structs.AssignB, s *struct{ X string }) {}

func main() {
	b := structs.AssignB(struct{ A int }{3})
	var s struct{ A int }
	s.A = 4
	c := local(s)
	fmt.Println(b.Sum(), c.A)
	var x struct{ X string }
	use(nil, &x)
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

	packageInfo := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "anonnamed", "package_info.cs"))

	for _, target := range []string{
		", local>]",           // same-package named struct target
		", structs.AssignB>]", // cross-package named struct target, and only the one conversion to it
	} {
		recorded := 0

		for _, line := range strings.Split(packageInfo, "\n") {
			if strings.HasPrefix(strings.TrimSpace(line), "[assembly: GoImplicitConv<") && strings.HasSuffix(strings.TrimSpace(line), target) {
				recorded++
			}
		}

		if recorded != 1 {
			t.Errorf("%d GoImplicitConv records ending %q, want 1, in:\n%s", recorded, target, packageInfo)
		}
	}

	if strings.Contains(packageInfo, "GoImplicitConv<use_") {
		t.Errorf("a record over the lifted parameter type of use, which no conversion reaches, in:\n%s", packageInfo)
	}
}
