// arrayPointerDoorUnionElement_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the array pointer doors over an array whose ELEMENT is a struct admitted to inline arrays and holding arrays:
// ccgo's union, `struct{ Fi [0]int32; Fu [0]uint32; Ff float32 }`, a [0]T carrier for each member but the one that has
// storage (sqlite's TExpr and TRtreeCoord, libc's Taddress and its sockaddr union).
//
// The doors take an element that is unmanaged in C#. They asked csManagedField, the plain reading, where every Go
// array is golib's array<T> reference, so such an element read as managed and the door declined: the old box route,
// which reads the whole array as a value and answers length 0 over memory that is no Go array of that type. An
// ADMITTED struct holds a [0]T as a primitive alignment carrier and a nonzero array as a go.arrays member
// (zeroSizeFieldLayout.go), so the doors now read admission: admitted, the inline reading (csManagedFieldInline);
// not admitted, the plain one, since its [0]T really is an array<T>. The door's own (*[N]T)(p) converts the element
// whole, which seeds admission; the control is a union a cap keeps out (GO2CS2009), so it keeps its route.

package main

import (
	"strings"
	"testing"
)

const arrayPointerDoorUnionElementFixture = `package main

import (
	"fmt"
	"unsafe"
)

type TCoord = struct {
	Fi [0]int32
	Fu [0]uint32
	Ff float32
}

type TExpr = struct {
	Fop uint8
	Fu  struct {
		FiValue [0]int32
		FzToken uintptr
	}
	Fw struct {
		FiOfst [0]int32
		FiJoin int32
	}
}

type TAddr = struct {
	Fa [0]uint64
	Fb [16]byte
}

type TLoose = struct {
	Fj   [0]int64
	Fg   float64
	Fpad [4100]byte
}

func readCoord(p unsafe.Pointer, i int) float32 {
	return (*[4]TCoord)(p)[i].Ff
}

func writeCoord(p unsafe.Pointer, i int, f float32) {
	(*[4]TCoord)(p)[i].Ff = f
}

func joinOf(p unsafe.Pointer, i int) int32 {
	return (*[3]TExpr)(p)[i].Fw.FiJoin
}

func addrByte(p unsafe.Pointer, i int) byte {
	return (*[2]TAddr)(p)[i].Fb[3]
}

func loose(p unsafe.Pointer, i int) float64 {
	return (*[4]TLoose)(p)[i].Fg
}

func main() {
	var words [16]uint64
	p := unsafe.Pointer(&words)
	writeCoord(p, 1, 2)
	fmt.Println(readCoord(p, 1), joinOf(p, 1), addrByte(p, 1))
}
`

// TestArrayPointerDoorTakesAnAdmittedUnionElement: an admitted union element takes the element door for a read and a
// store, a nested union and a nonzero inline array included; a union that is not admitted keeps its route.
func TestArrayPointerDoorTakesAnAdmittedUnionElement(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	withDynamicTypeRegistry(t, func() {
		mainCs := convertFunnelFixture(t, "example.com/unionelemdoor", "unionelemdoor", arrayPointerDoorUnionElementFixture)

		for _, arm := range []struct {
			function, door string
		}{
			{"readCoord", "return @unsafe.Pointer.ElementRef<TCoord>(p, i, 4).Ff;"},
			{"writeCoord", "@unsafe.Pointer.ElementRef<TCoord>(p, i, 4).Ff = f;"},
			{"joinOf", "return @unsafe.Pointer.ElementRef<TExpr>(p, i, 3).Fw.FiJoin;"},
			{"addrByte", "@unsafe.Pointer.ElementRef<TAddr>(p, i, 2).Fb"},
		} {
			body := emittedFunctionBody(t, mainCs, arm.function)

			if !strings.Contains(body, arm.door) {
				t.Errorf("%s: an admitted union element takes the element door %q, got:\n%s", arm.function, arm.door, body)
			}

			if strings.Contains(body, "To<array<") {
				t.Errorf("%s: an admitted union element keeps the box route, got:\n%s", arm.function, body)
			}
		}

		// The control: TLoose is converted whole like the others (the door's own conversion is a seed), but its 4100-byte
		// array is over the cap, so admission keeps it out, its [0]T is an array<T>, and the door declines it as before.
		body := emittedFunctionBody(t, mainCs, "loose")

		if !strings.Contains(body, "@unsafe.Pointer.To<array<TLoose>>(p)") || strings.Contains(body, "ElementRef<") {
			t.Errorf("loose: a union that is not admitted keeps the box route, got:\n%s", body)
		}
	})
}
