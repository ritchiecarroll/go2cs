// mixedKeyedComposite_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards two emission rules a white-box test conversion of net/netip was the first thing in the
// corpus to reach. Both produced C# that does not PARSE, so neither can be caught by anything
// downstream of the compiler — and both are shapes the production corpus happens not to contain,
// which is why they survived to be found by a Phase-4 measurement rather than by a build.
//
//  1. Go's all-or-nothing keying rule is a STRUCT-literal rule. An array or slice literal may mix
//     positional and keyed elements, and the converter's sparse-array detection read Elts[0] alone.
//
//  2. A `global using` alias RHS is rendered ROOTED, and the root prefix was applied even to a name
//     the white-box test qualifiers had already rooted with an explicit `global::`.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestMixedKeyedArrayCompositeLiteral pins the emission of an array/slice literal that MIXES
// positional and keyed elements. Go gives the first element index 0, a keyed element sets the
// index to its key, and each following positional element takes the next one — so the literal is
// as long as its highest index plus one, not as long as its element count. Before the fix the
// positional elements took the plain array-initializer emission while the keyed one still rendered
// through the key/value arm, whose sparse form wants a target ident that does not exist in an
// expression position: `new byte[]{0xfe, 0x80, <nil>[15] = 0x01}` — CS1525, invalid expression
// term '<'.
func TestMixedKeyedArrayCompositeLiteral(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/mkc\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

// The reported shape: a SLICE literal that starts positional and ends keyed. Sixteen bytes long,
// not three — net/netip's TestAddrFromSlice/TestAsSlice write their IPv4-in-IPv6 addresses this way.
var mixedSlice = []byte{0xfe, 0x80, 15: 0x01}

// The same mix in a fixed-length ARRAY, and with a positional element AFTER the key: Go continues
// at key+1, so this is {0:1, 1:2, 5:9, 6:10} and the array is its declared eight long regardless.
var mixedArray = [8]int{1, 2, 5: 9, 10}

// Unmixed literals must be untouched by the normalization — these two are what the whole corpus
// looks like, and their emission is not allowed to move.
var allPositional = []int{1, 2, 3}
var allKeyed = []int{0: 1, 2: 3}

func main() {
	fmt.Println(len(mixedSlice), mixedSlice[15], len(mixedArray), mixedArray[6], allPositional, allKeyed)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "mkc", "main.cs"))

	// The defect's own signature: the sparse key/value arm rendered with no target ident. Any
	// occurrence of it is invalid C# whatever else the file says.
	if strings.Contains(mainCs, "<nil>[") {
		t.Errorf("a keyed element rendered with no target ident (`<nil>[…] = …`):\n%s", mainCs)
	}

	// A mixed literal is all-keyed after normalization, and carries Go's own indices.
	for _, want := range []string{
		"[0] = 0xfe",
		"[1] = 0x80",
		"[15] = 0x01",
		"[5] = 9",
		"[6] = 10",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("mixed literal missing its Go index %q:\n%s", want, mainCs)
		}
	}

	// The LENGTH is the part a wrong emission gets silently wrong rather than loudly: a slice
	// literal with a key is as long as that key plus one, and a fixed array stays its DECLARED
	// length however few indices the literal writes.
	if !strings.Contains(mainCs, "slice<byte>(16)") {
		t.Errorf("mixed slice literal must carry its Go length (16):\n%s", mainCs)
	}

	if !strings.Contains(mainCs, "array<nint>(8)") {
		t.Errorf("mixed array literal must carry its DECLARED length (8):\n%s", mainCs)
	}

	// …and an unmixed literal keeps the emission it already had.
	if !strings.Contains(mainCs, "new nint[]{1, 2, 3}.slice()") {
		t.Errorf("an all-positional literal must be untouched by the normalization:\n%s", mainCs)
	}
}

// TestRootedUsingAliasKeepsGlobalQualifier pins that the rooted `global using` RHS renderer treats
// an already-rooted `global::` name as rooted. The white-box test qualifiers build production
// references with an explicit `global::` root, and the default arm prefixed the root namespace onto
// it regardless — `go.global::go.net.netip_package.uint128`, CS7000. Rooting is idempotent by
// intent; this asserts it is idempotent in fact.
func TestRootedUsingAliasKeepsGlobalQualifier(t *testing.T) {
	const rooted = "global::go.net.netip_package.uint128"

	if got := renderCSFullTypeName(rooted, true); got != rooted {
		t.Errorf("an already-rooted name must render unchanged\n got: %s\nwant: %s", got, rooted)
	}

	// The non-rooted rendering must not acquire one either: `global::` is not a `go.`-prefixed name,
	// so the root-strip that follows it has nothing to do.
	if got := renderCSTypeName(rooted, false); got != rooted {
		t.Errorf("an already-rooted name must survive the unrooted render too\n got: %s\nwant: %s", got, rooted)
	}
}
