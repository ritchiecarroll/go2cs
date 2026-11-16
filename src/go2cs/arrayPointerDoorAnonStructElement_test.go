// arrayPointerDoorAnonStructElement_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the array pointer doors over an array whose ELEMENT is an anonymous struct that nothing else in the
// function declares: `(*[13]struct{ n int32; a uintptr })(p)[i].a`, the form C compiled to Go writes for a
// function-local array (its only declaration is a blank placeholder, `var _ [13]struct{...}`, which lifts nothing).
//
// The doors render from the index, the slice, the dereference or the range, so the conversion itself never reaches
// the call arm that lifts a conversion's anonymous struct (liftConversionTargetAnonStruct). The element was named
// with no lift behind it, the deferred marker resolved to nothing, and the emission carried the raw Go struct text,
// which ends the conversion ("unresolved dynamic type(s) were emitted as raw Go source"). The doors make that lift
// themselves before they name the element (arrayPointerViewOfBase).
//
// Every door form has an arm, because they share one naming site and each must be seen to pass through it. The
// field view (nativeFieldArrayView.go) has no arm: it serves a numeric element only, so it never names a struct.

package main

import (
	"strings"
	"testing"
)

const arrayPointerDoorAnonStructElementFixture = `package main

import (
	"fmt"
	"unsafe"
)

type tokenizer struct {
	create uintptr
	remove uintptr
}

type holder struct {
	n    uint32
	pair [2]uint64
}

func index(p unsafe.Pointer, i int) uintptr {
	var _ /* aSub at bp+0 */ [13]struct {
		nList int32
		aList uintptr
	}

	(*[13]struct {
		nList int32
		aList uintptr
	})(p)[i].nList = 5

	return (*[13]struct {
		nList int32
		aList uintptr
	})(p)[i].aList
}

func address(p unsafe.Pointer, i int) int32 {
	e := &(*[4]struct {
		count int32
		next  uintptr
	})(p)[i]

	return e.count
}

func whole(p unsafe.Pointer) uintptr {
	*(*[2]struct {
		name uintptr
		x    tokenizer
	})(p) = [2]struct {
		name uintptr
		x    tokenizer
	}{
		0: {name: 1, x: tokenizer{create: 2, remove: 3}},
	}

	all := *(*[2]struct {
		name uintptr
		x    tokenizer
	})(p)

	return all[0].x.create
}

func sliced(p unsafe.Pointer) int {
	tail := (*[4]struct {
		lo uint16
		hi uint16
	})(p)[1:3]

	return len(tail)
}

func ranged(p unsafe.Pointer) (n uint32) {
	for _, e := range (*[4]struct {
		tag  uint32
		size uint32
	})(p) {
		n += e.size
	}

	return
}

func fieldRoot(h *holder) uint32 {
	return uint32((*[4]struct {
		a uint16
		b uint16
	})(unsafe.Pointer(&h.pair))[1].b) + h.n
}

func main() {
	var words [32]uint64
	p := unsafe.Pointer(&words)
	fmt.Println(index(p, 1), address(p, 1), whole(p), sliced(p), ranged(p), fieldRoot(&holder{}))
}
`

// TestArrayPointerDoorLiftsAnAnonymousStructElement: the conversion ends with no unresolved type, the emission
// carries no raw Go struct text, and each door names a lifted C# type that the file declares.
func TestArrayPointerDoorLiftsAnAnonymousStructElement(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	withDynamicTypeRegistry(t, func() {
		mainCs := convertFunnelFixture(t, "example.com/anonelemdoor", "anonelemdoor", arrayPointerDoorAnonStructElementFixture)

		if unresolved := takeUnresolvedDynamicTypes(); len(unresolved) > 0 {
			t.Errorf("the conversion recorded %d unresolved dynamic type(s), which ends it: %v", len(unresolved), unresolved)
		}

		if strings.Contains(mainCs, "struct{") {
			t.Errorf("the emission carries raw Go struct text:\n%s", linesContaining(mainCs, "struct{"))
		}

		for _, arm := range []struct {
			function, door string
		}{
			{"index", "@unsafe.Pointer.ElementRef<index__ᴛ1>(p, i, 13).nList = 5;"},
			{"index", "return @unsafe.Pointer.ElementRef<index__ᴛ1>(p, i, 13).aList;"},
			{"address", "@unsafe.Pointer.Element<address_type>(p, i, 4)"},
			{"whole", "@unsafe.Pointer.StoreArray<whole_type>(p, 2, "},
			{"whole", "@unsafe.Pointer.LoadArray<whole_type>(p, 2)"},
			{"sliced", "@unsafe.Pointer.View<sliced_type>(p, 1, 3, 4)"},
			{"ranged", "in range(@unsafe.Pointer.View<ranged_type>(p, 0, 4, 4)))"},
		} {
			body := emittedFunctionBody(t, mainCs, arm.function)

			if !strings.Contains(body, arm.door) {
				t.Errorf("%s: expected the door %q over the lifted element, got:\n%s", arm.function, arm.door, body)
			}
		}

		// One declaration for each lifted element. The blank placeholder's lift is the index's type (one Go type, one
		// C# type), and the whole store's literal shares the store's: two C# types there would not assign.
		for _, lifted := range []string{"index__ᴛ1", "address_type", "whole_type", "sliced_type", "ranged_type", "fieldRoot_type"} {
			if count := strings.Count(mainCs, "partial struct "+lifted+" "); count != 1 {
				t.Errorf("%s: expected one declaration, found %d", lifted, count)
			}
		}

		if body := emittedFunctionBody(t, mainCs, "whole"); strings.Contains(body, "whole_typeᴛ") {
			t.Errorf("whole: the literal and the store name two C# types for one Go type, got:\n%s", body)
		}

		// An operand that is a field's address takes the element door too, over the same lifted element.
		if body := emittedFunctionBody(t, mainCs, "fieldRoot"); !strings.Contains(body, "@unsafe.Pointer.ElementRef<fieldRoot_type>(") {
			t.Errorf("fieldRoot: expected the element door over the lifted element, got:\n%s", body)
		}
	})
}

// linesContaining returns the lines of text that contain needle, for a failure message.
func linesContaining(text, needle string) string {
	var found []string

	for _, line := range strings.Split(text, "\n") {
		if strings.Contains(line, needle) {
			found = append(found, strings.TrimRight(line, "\r"))
		}
	}

	return strings.Join(found, "\n")
}
