// fieldAccessReturn_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the field-access emission's one provenance step across a return (fieldAccess.go): a local assigned from a
// same-package callee whose every return in that position is nil, a struct laid over the element address of byte
// storage, or a callee local whose every assignment is one, reads its fields through the field property, as a local
// assigned the reinterpret directly does -- os's TestRootSymlinkRelativity shape, `osRDB, err :=
// readSymlinkReparseData(…)` followed by `osRDB.Flags`. Each negative keeps today's whole-value emission: a
// named-result callee, a callee that returns another call, a generic callee, a callee in another package, a call
// through a func value, a callee that returns a fresh `&T{}`, and three reinterprets that are not over byte storage:
// an unsafe.Pointer parameter (runtime's efaceOf shape), a uintptr getter (runtime's puintptr.ptr) and the element
// address of a []uint64.
//
// Each site writes or reads a field named `Value`, like a member of ж<T>, because that is where the two routes still
// differ since parity seat #31: P1 spells an ordinary field `p.f` through ANY pointer, so only a collision tells the
// provenance route's explicit `p.of(T.ᏑValue).Value` from the box form every other pointer keeps, `p.Value.Value`.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func TestFieldAccessCrossesOneReturn(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "sub", "sub.go"), `package sub

import "unsafe"

type Rec struct {
	Name  string
	N     uint16
	Value uint16
}

func View(blob []byte) *Rec { return (*Rec)(unsafe.Pointer(&blob[0])) }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"errors"
	"fmt"
	"unsafe"

	"example.com/fieldaccess/sub"
)

type buffer struct {
	Name   string
	Offset uint16
	Flags  uint32
	Value  uint32
}

// view returns its reinterpret directly.
func view(blob []byte) *buffer { return (*buffer)(unsafe.Pointer(&blob[0])) }

// read returns a local the rule admits over its own array, or nil with an error.
func read(fill bool) (*buffer, error) {
	var raw [32]byte

	if !fill {
		return nil, errors.New("empty")
	}

	raw[8] = 1
	buf := (*buffer)(unsafe.Pointer(&raw[0]))
	return buf, nil
}

type reader struct{ blob []byte }

func (r *reader) at() (*buffer, error) { return (*buffer)(unsafe.Pointer(&r.blob[0])), nil }

func named(blob []byte) (b *buffer, err error) {
	b = (*buffer)(unsafe.Pointer(&blob[0]))
	return
}

func forwarded(blob []byte) *buffer { return view(blob) }

func generic[E any](blob []byte) *buffer { return (*buffer)(unsafe.Pointer(&blob[0])) }

func fresh(blob []byte) *buffer {
	if len(blob) == 0 {
		return (*buffer)(unsafe.Pointer(&blob[0]))
	}

	return &buffer{}
}

func overPointer(p unsafe.Pointer) *buffer { return (*buffer)(unsafe.Pointer(p)) }

type bufptr uintptr

func (b bufptr) ptr() *buffer { return (*buffer)(unsafe.Pointer(b)) }

func overWords(u []uint64) *buffer { return (*buffer)(unsafe.Pointer(&u[1])) }

func positives(blob []byte, r *reader) {
	pa := view(blob)
	pa.Value = 1
	pa.Offset = 1
	pb, err := read(true)

	if err != nil {
		return
	}

	fmt.Println(pb.Value)
	pc, _ := r.at()
	pc.Value = 2
	var pd, _ = read(true)
	fmt.Println(pd.Value)
}

func negatives(blob []byte) {
	n1, _ := named(blob)
	n1.Value = 3
	n2 := forwarded(blob)
	n2.Value = 4
	n3 := generic[int](blob)
	n3.Value = 5
	n4 := sub.View(blob)
	n4.Value = 6
	f := view
	n5 := f(blob)
	n5.Value = 7
	n6 := fresh(blob)
	n6.Value = 8
	n7 := overPointer(unsafe.Pointer(&blob[0]))
	n7.Value = 9
	n8 := bufptr(uintptr(unsafe.Pointer(&blob[0]))).ptr()
	n8.Value = 10
	n9 := overWords(make([]uint64, 4))
	n9.Value = 11
}

func main() {
	blob := make([]byte, 64)
	positives(blob, &reader{blob})
	negatives(blob)
}
`)

	out := convertFieldAccessModule(t, root)
	cs, info := readGenerated(t, filepath.Join(out, "main.cs")), readGenerated(t, filepath.Join(out, PackageInfoFileName))
	scoped := funcBody(t, cs, "void positives(")

	for _, want := range []string{
		"pa.of(buffer.ᏑValue).Value = 1;",
		"pa.Offset = 1;",
		"fmt.Println(pb.of(buffer.ᏑValue).Value);",
		"pc.of(buffer.ᏑValue).Value = 2;",
		"fmt.Println(pd.of(buffer.ᏑValue).Value);",
	} {
		if !strings.Contains(scoped, want) {
			t.Errorf("a local assigned from a summarized callee does not take the field-access form (want %q):\n%s", want, scoped)
		}
	}

	if got := strings.Join(fieldAccessRecordsOf(info), " "); got != "buffer" {
		t.Errorf("package_info.cs records %q, want \"buffer\":\n%s", got, info)
	}

	negative := funcBody(t, cs, "void negatives(")

	for _, kept := range []string{
		"n1.Value.Value = 3;",
		"n2.Value.Value = 4;",
		"n3.Value.Value = 5;",
		"n4.Value.Value = 6;",
		"n5.Value.Value = 7;",
		"n6.Value.Value = 8;",
		"n7.Value.Value = 9;",
		"n8.Value.Value = 10;",
		"n9.Value.Value = 11;",
	} {
		if !strings.Contains(negative, kept) {
			t.Errorf("a negative left today's whole-value emission (want %q):\n%s", kept, negative)
		}
	}
}
