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
	"sort"
	"strings"
	"testing"
)

// TestImplicitConversionsComparesEachArgumentWithItsOwnParameter pins convCallExpr's argument loop, which
// records the implicit conversions a call's arguments need. It used to hand applyImplicitConversion the
// callee's whole signature for EVERY argument, which then compared each one against Params().At(0), and to
// pass the argument's OWN type name as the conversion target. Two defects followed, both visible below:
//
//   - a pointer argument recorded a conversion from its type to its own box: `two(s, u)` with
//     `func two(p *S, q *U)` recorded GoImplicitConv<U, ж<U>>(Indirect = true), and tail's variadic
//     pointers the same without Indirect. go2cs-gen skips every such record (ж<U> has no struct
//     declaration), so they emitted nothing; the committed corpus carried 255 of them in 77 files;
//   - an argument that DOES need a conversion got none: `pair(S{1}, struct{ A int }{2})` passes the lifted
//     anonymous struct where `b V` is declared, and the record GoImplicitConv<lifted, V> that gives C# the
//     operator was never made, because the target the loop passed was the argument's own type.
//
// Each argument is now compared with ITS OWN parameter, variadic-aware (an expanded `...` argument matches
// the slice parameter itself, the others match its element type), and the target is that parameter's type.
func TestImplicitConversionsComparesEachArgumentWithItsOwnParameter(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/argloop\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type S struct{ A int }
type U struct{ B string }
type V struct{ A int }

func two(p *S, q *U) int        { return p.A + len(q.B) }
func pair(a S, b V) int         { return a.A + b.A }
func tail(a S, rest ...*U) int  { return a.A + len(rest) }
func spread(rest ...V) int      { return len(rest) }

func main() {
	s, u := &S{A: 1}, &U{B: "xy"}
	println(two(s, u))
	println(pair(S{1}, struct{ A int }{2}))
	println(tail(S{1}, u, u))
	println(tail(S{1}, []*U{u}...))
	println(spread(V{1}, V{2}))
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

	packageInfo := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "argloop", "package_info.cs"))
	mainCS := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "argloop", "main.cs"))

	// The lifted name of the anonymous struct argument, read from the emission rather than assumed.
	lifted := ""

	for _, line := range strings.Split(mainCS, "\n") {
		if strings.Contains(line, "pair(new S(1), new ") {
			rest := line[strings.Index(line, "pair(new S(1), new ")+len("pair(new S(1), new "):]
			lifted = rest[:strings.Index(rest, "(")]
		}
	}

	if lifted == "" {
		t.Fatalf("the pair call is not emitted in the expected shape; read the emission:\n%s", mainCS)
	}

	var records []string

	for _, line := range strings.Split(packageInfo, "\n") {
		if trimmed := strings.TrimSpace(line); strings.HasPrefix(trimmed, "[assembly: GoImplicitConv<") {
			records = append(records, trimmed)
		}
	}

	sort.Strings(records)
	want := []string{"[assembly: GoImplicitConv<" + lifted + ", V>]"}

	if strings.Join(records, "\n") != strings.Join(want, "\n") {
		t.Errorf("GoImplicitConv records:\n  %s\nwant exactly:\n  %s\n(a record from a type to its own box means an argument was compared with "+
			"another parameter; a missing <%s, V> means pair's second argument was)", strings.Join(records, "\n  "), strings.Join(want, "\n  "), lifted)
	}
}
