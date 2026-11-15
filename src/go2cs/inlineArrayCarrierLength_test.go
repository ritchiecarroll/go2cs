// inlineArrayCarrierLength_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

// W15 (COORD 2026-11-15, sqlite-cs .4 gate wall): a [0][N]T field of a struct admitted to inline arrays is an alignment
// carrier, and its [GoAlignmentCarrier] attribute names the Go field's type. That type was array<array<T>>, which drops
// N, so golib's layout reader could not derive the field's element ([N]T) and could not place the struct: libc's in6
// statics and sqlite's TBitvec, on every CREATE TABLE. The element is named as the fixed-size go.arrays member instead,
// array<arrayN<T>>, whose length golib reads from the member's [GoInlineArray(N)].
const inlineCarrierLengthSource = `package carrier

import "unsafe"

type Bitvec struct {
	Pad [0][4]uint32
	N   uint32
	B   [8]byte
}

type Deep struct {
	Pad [0][2][3]uint16
	X   uint16
}

func view(p unsafe.Pointer) *Bitvec { return (*Bitvec)(p) }

func deep(p unsafe.Pointer) *Deep { return (*Deep)(p) }
`

func TestInlineArrayCarrierNamesTheInnerLength(t *testing.T) {
	cs := convertInlineArrayFixture(t, "carrier", inlineCarrierLengthSource)

	for _, want := range []string{
		`[GoAlignmentCarrier(typeof(array<array4<uint32>>))]`,
		`[GoAlignmentCarrier(typeof(array<array2<array3<uint16>>>))]`,
	} {
		if !strings.Contains(cs, want) {
			t.Errorf("want %q:\n%s", want, cs)
		}
	}
}

// The same shapes built and run: the layout read (unsafe.Sizeof, unsafe.Offsetof, a view over raw bytes read and
// written) prints what Go prints. GO2CS_INLINE_BUILD=1 runs it.
var inlineCarrierLengthModule = map[string]string{
	"main.go": strings.NewReplacer("package carrier", "package main", `import "unsafe"`, "import (\n\t\"fmt\"\n\t\"unsafe\"\n)").Replace(inlineCarrierLengthSource) + `
func main() {
	var raw [32]byte
	for i := range raw {
		raw[i] = byte(i + 1)
	}

	v := view(unsafe.Pointer(&raw[0]))
	fmt.Println(unsafe.Sizeof(*v), unsafe.Alignof(*v), unsafe.Offsetof(v.N), unsafe.Offsetof(v.B))
	fmt.Println(v.N, v.B, len(v.Pad))
	v.B[7] = 0xEE
	fmt.Println(raw[11])

	d := deep(unsafe.Pointer(&raw[16]))
	fmt.Println(unsafe.Sizeof(*d), unsafe.Alignof(*d), d.X)
}
`,
}

func TestInlineArrayCarrierInnerLengthReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	want := goRunInlineModule(t, "carrierlen", inlineCarrierLengthModule)
	out := convertInlineModule(t, "carrierlen", inlineCarrierLengthModule)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the carrier module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints:\n%s\nREADING go2cs prints:\n%s", want, got)

	if got != want || want == "" {
		t.Fatalf("the two runs differ, or printed nothing:\n   go:\n%s\ngo2cs:\n%s", want, got)
	}

}
