// inlineAdmissionLiftReuse_test.go - Gbtc
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

// W18's linux wall (G, 2026-11-16): the reuse crossed an inline-admission boundary. libc's Tstatfs is ADMITTED (ccgo
// converts it whole through unsafe.Pointer), so its field type Tfsid_t, an anonymous struct holding [2]int32, is
// admitted with it: libc's own lift is [GoInlineLayout] with an array2<int32> field. With W18, Tfsid_t named
// libc/uuid/uuid's lift of the identical struct instead, which uuid does NOT admit (array<int32>, managed), and the
// admitted Tstatfs held a managed field: CS8377 at GoManagedLayout.AlignmentOf<Tstatfsᴛ1>(). A foreign lift is reused
// only when its admission matches the importer's need (G's rule, COORD 2026-11-16).
const admissionLiftLib = `package lib

type I32 = int32

// Fsid is uuid's spelling of the struct; lib never converts anything through unsafe.Pointer, so its lift is not
// admitted, and it is public through Sum.
type Fsid = struct {
	Val [2]I32
}

func Sum(f Fsid) int32 { return f.Val[0] + f.Val[1]*10 }
`

// The importer spells the identical struct through int32 and admits it: Statfs is converted whole through
// unsafe.Pointer, and its closure takes TFsid.
const admissionLiftMain = `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/lib"
)

type TFsid = struct {
	Val [2]int32
}

type Statfs struct {
	Type  int64
	Fsid  TFsid
	Flags int64
}

func main() {
	var raw [3]uint64
	s := (*Statfs)(unsafe.Pointer(&raw))
	s.Fsid.Val[0] = 3
	s.Fsid.Val[1] = 4
	fmt.Println(lib.Sum(s.Fsid), unsafe.Sizeof(*s))
}
`

// The emission: TFsid keeps main's own admitted lift, and never names lib's unadmitted one.
func TestAdmittedImporterKeepsItsOwnLiftOverAnUnadmittedForeignLift(t *testing.T) {
	isolateConverterPathGlobals(t)

	emitted := convertModuleMainWithLib(t, "lib", admissionLiftLib, admissionLiftMain)

	if strings.Contains(emitted, "lib_package.Fsidᴛ1") {
		t.Errorf("an admitted struct's field names the import's UNADMITTED lift (CS8377 on the admitted holder):\n%s", emitted)
	}

	if !strings.Contains(emitted, "struct TFsidᴛ1") {
		t.Errorf("the admitted struct's field type declares no lift of its own:\n%s", emitted)
	}

	if !strings.Contains(emitted, "array2<int32> Val") {
		t.Errorf("main's own lift is not the admitted one (array2<int32>):\n%s", emitted)
	}
}

// The control: where the importer does NOT admit the struct (nothing converts Statfs through unsafe.Pointer), the
// admissions match and the reuse W18 made stays.
func TestUnadmittedImporterStillReusesAnUnadmittedForeignLift(t *testing.T) {
	isolateConverterPathGlobals(t)

	notAdmitted := strings.Replace(admissionLiftMain,
		"\tvar raw [3]uint64\n\ts := (*Statfs)(unsafe.Pointer(&raw))\n",
		"\ts := &Statfs{}\n", 1)

	if notAdmitted == admissionLiftMain {
		t.Fatal("the control's edit did not apply")
	}

	emitted := convertModuleMainWithLib(t, "lib", admissionLiftLib, notAdmitted)

	if !strings.Contains(emitted, "global using TFsid = go.example.com.app.lib_package.Fsidᴛ1;") {
		t.Errorf("with matching admission TFsid no longer names lib's lift of the identical struct:\n%s", emitted)
	}
}

// End to end: the converted module builds (CS8377 at the base) and prints what go prints. GO2CS_INLINE_BUILD=1 runs it.
func TestAdmittedImporterOverUnadmittedForeignLiftBuildsAndReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := map[string]string{"lib/lib.go": admissionLiftLib, "main.go": admissionLiftMain}
	want := goRunInlineModule(t, "app", files)
	out := convertInlineModule(t, "app", files)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints: %s; go2cs prints: %s", want, got)

	if got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
	}
}
