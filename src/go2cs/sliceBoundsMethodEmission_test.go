// sliceBoundsMethodEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards S-c R1-A's emission rule (docs/phase4/DESIGN-slice-bounds-r1a.md §5 and §6), in both directions.
//
// A 2-index slice expression whose bound the C# Range cannot carry exactly -- a non-constant bound, or a constant
// past int32 -- calls golib's sentinel-free slice method, so Go's check sees the full value. Every other 2-index
// site keeps its Range byte for byte: that half is what keeps the corpus footprint to the sites that need it. The
// 3-index `[:h:max]` form emits 0 for its omitted low, never the -1 the old optional-parameter method read as
// "omitted".

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestSliceBoundsMethodEmission(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/slicebounds\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type named []int

type namedStr string

type namedArr [3]int

const big = 1<<32 + 5

var sink any

func bytesOf[T string | []byte](x T, i int) T { return x[i:] }

func sub[S ~[]E, E any](s S, i int) S { return s[i:] }

func main() {
	s := make([]int, 3, 10)
	var a [3]int
	p := &a
	n := named(s)
	var na namedArr
	str := "abc"
	ns := namedStr("abc")
	i, j := 1, 2
	var u uint64 = 2
	var w int64 = 2

	// MOVE: a non-constant bound, on every receiver kind.
	sink = s[i:j]
	sink = s[:j]
	sink = s[i:]
	sink = n[i:j]
	sink = a[i:j]
	sink = p[:j]
	sink = na[:j]
	sink = str[i:]
	sink = ns[:j]
	sink = "abc"[:j]

	// MOVE: one non-constant bound beside a constant one.
	sink = s[1:j]

	// MOVE, with a (nint) bound: a wide unsigned and an int64 bound (ruling U1).
	sink = s[:u]
	sink = s[w:]

	// MOVE: a constant past int32, which the Range truncated (named constant, bare literal).
	sink = s[:big]
	sink = s[4294967301:]

	// KEEP: constant bounds within int32, and the whole-value form.
	sink = s[2:]
	sink = s[:5]
	sink = s[1:3]
	sink = s[:]
	sink = str[1:]

	// 3-INDEX: the omitted low is 0.
	sink = s[:j:5]
	sink = s[i:j:5]

	sink = bytesOf("abc", i)
	sink = sub(n, i)
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

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "slicebounds", "main.cs"))

	moved := []struct{ why, want string }{
		{"s[i:j]", "sink = s.slice(i, j);"},
		{"s[:j], Go's omitted low is 0", "sink = s.slice(0, j);"},
		{"s[i:]", "sink = s.slice(i);"},
		{"a named slice", "sink = n.slice(i, j);"},
		{"an array", "sink = a.slice(i, j);"},
		{"a pointer-to-array dereferences first", "sink = (~p).slice(0, j);"},
		{"a named array", "sink = na.slice(0, j);"},
		{"a string", "sink = str.slice(i);"},
		{"a named string", "sink = ns.slice(0, j);"},
		{"a string literal", `sink = "abc"u8.slice(0, j);`},
		{"a constant low beside a variable high", "sink = s.slice(1, j);"},
		{"a uint64 bound takes (nint)", "sink = s.slice(0, (nint)(u));"},
		{"an int64 bound takes (nint)", "sink = s.slice((nint)(w));"},
		{"a named constant past int32", "sink = s.slice(0, big);"},
		{"a bare literal past int32", "sink = s.slice(unchecked((nint)4294967301L));"},
		{"3-index [:h:max] emits 0 for the omitted low", "sink = s.slice(0, j, 5);"},
		{"3-index [lo:h:max]", "sink = s.slice(i, j, 5);"},
		{"a string | []byte type parameter takes IByteSeq's member", "return x.slice(i);"},
	}

	for _, m := range moved {
		if !strings.Contains(mainCs, m.want) {
			t.Errorf("MOVE (%s): want %q in:\n%s", m.why, m.want, mainCs)
		}
	}

	kept := []struct{ why, want string }{
		{"a constant low within int32", "sink = s[2..];"},
		{"a constant high within int32", "sink = s[..5];"},
		{"two constant bounds within int32", "sink = s[1..3];"},
		{"the whole-value form", "sink = s[..];"},
		{"a constant bound on a string", "sink = str[1..];"},
		{"a ~[]E type parameter keeps subslice", "return subslice<S, E>(s, i);"},
	}

	for _, k := range kept {
		if !strings.Contains(mainCs, k.want) {
			t.Errorf("KEEP (%s): want %q in:\n%s", k.why, k.want, mainCs)
		}
	}

	// No -1 sentinel anywhere, and no (int) narrowing left on a slice bound in this fixture.
	for _, gone := range []string{".slice(-1", "..(int)(", "(int)(i)..", "(int)(j)"} {
		if strings.Contains(mainCs, gone) {
			t.Errorf("%q must not appear:\n%s", gone, mainCs)
		}
	}
}
