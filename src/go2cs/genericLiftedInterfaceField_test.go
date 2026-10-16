// genericLiftedInterfaceField_test.go - Gbtc
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

// Go 1.26's crypto/x509 declares `type constraints[T, V any] struct { permitted interface{ query(V) (T,
// bool) } }` and fills it from functions returning `interface{ query(string) (string, bool) }`. Two
// defects, one per half of the fix (H7 class E):
//
//   (a) the field's anonymous interface was lifted NON-generic, `partial interface constraints_permitted
//       { (T, bool) query(V _); }`, with T and V unbound (CS0246). It must carry `<T, V>`, and the struct
//       must name `constraints_permitted<T, V>`.
//   (b) Go spells ONE type `interface{ query(string) (string, bool) }` whether it is the function's
//       result or `constraints[string, string].permitted`; C# has two lifted interfaces. Each instantiation
//       of the generic struct makes the concrete shape's lift derive the closed generic
//       (`partial interface newDNS_R0 : constraints_permitted<@string, @string> {}`), an implicit
//       reference conversion; the concrete lift's own member then redeclares the inherited one and is
//       emitted `new`, so the corpus gains no CS0108.
//
// Dispatch both ways and Go's structural assertion are proven by the behavioral test
// GenericLiftedInterfaceField (compiled and output-compared to Go); this pins the emission.

func TestLiftedInterfaceFieldOfAGenericStructCarriesItsTypeParameters(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/genlift\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type constraints[T any, V any] struct {
	kind      string
	permitted interface{ query(V) (T, bool) }
}

type dnsC struct{ allow string }

func (d dnsC) query(s string) (string, bool) { return d.allow, s == d.allow }

func newDNS(allow string) interface{ query(string) (string, bool) } { return dnsC{allow} }

func check[T any, V any](c constraints[T, V], s V) bool {
	_, ok := c.permitted.query(s)
	return ok
}

func main() {
	c := constraints[string, string]{"dns", newDNS("a")}
	fmt.Println(check(c, "a"), check(c, "b"))
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

	cs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "genlift", "main.cs"))

	// (a) the lift binds the parameters it closes over, and the struct names it constructed.
	if !regexp.MustCompile(`partial interface constraints_permitted<T, V>`).MatchString(cs) {
		t.Errorf("(a) the field's lifted interface does not declare <T, V>:\n%s", cs)
	}

	if !strings.Contains(cs, "constraints_permitted<T, V> permitted;") {
		t.Errorf("(a) the generic struct's field does not name constraints_permitted<T, V>:\n%s", cs)
	}

	// (b) the concrete shape's own lift derives the closed generic, once per instantiation...
	base := regexp.MustCompile(`partial interface (newDNS_R\w*) : constraints_permitted<@string, @string>`).FindStringSubmatch(cs)

	if base == nil {
		t.Fatalf("(b) no instantiation made the concrete lift derive constraints_permitted<@string, @string>:\n%s", cs)
	}

	// ...and that lift's member is declared `new`: it redeclares the inherited one (CS0108 otherwise).
	if !regexp.MustCompile(`(?s)interface ` + base[1] + `[^{]*\{[^}]*new \(@string, bool\) query\(`).MatchString(cs) {
		t.Errorf("(b) %s's member redeclares the inherited query without `new` (CS0108):\n%s", base[1], cs)
	}
}
