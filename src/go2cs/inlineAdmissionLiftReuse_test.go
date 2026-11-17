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

// Name lets main import lib without passing a struct across the boundary.
const Name = "lib"
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
	fmt.Println(s.Fsid.Val[0]+s.Fsid.Val[1]*10, raw[1], unsafe.Sizeof(*s), lib.Name)
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

// The union (COORD 2026-11-16 08:55Z, the windows red): sqlite/lib ADMITS TLARGE_INTEGER, libc does not admit its
// identical largeInteger (its [0]T carriers are managed there), and the reuse put libc's lift under lib's admitted
// layout: "its CLR layout is not its Go layout (field F__ccgo0_0 is at CLR offset 32 where Go lays it at 0)" at
// _winFileSize. Here main converts TLARGE_INTEGER whole through unsafe.Pointer, so main admits it, and lib (the
// aliasSpelledLib fixture, libc's union verbatim) does not. Under COORD's ruling A (2026-11-17 07:20Z) each package keeps
// its own lift, so main reads native memory through its admitted union and touches lib only through a word.
const admittedUnionMain = `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/lib"
)

type TDWORD = uint32
type TLONG = int32
type TLONGLONG = int64

type TLARGE_INTEGER = struct {
	Fu [0]struct {
		FLowPart  TDWORD
		FHighPart TLONG
	}
	FQuadPart  [0]TLONGLONG
	F__ccgo0_0 struct {
		FLowPart  TDWORD
		FHighPart TLONG
	}
}

func main() {
	var raw [1]uint64
	li := (*TLARGE_INTEGER)(unsafe.Pointer(&raw))
	li.F__ccgo0_0.FLowPart = 5
	li.F__ccgo0_0.FHighPart = 6
	fmt.Println(raw[0], unsafe.Sizeof(*li), lib.SeekWord != 0)
}
`

// The crossing the ruling leaves open: main passes its admitted union BY VALUE to lib, which holds the unadmitted lift
// of the same Go type, and calls lib's word through main's func type. Go calls the two one type; under A they are two C#
// types with no conversion between them (CS1503).
const admittedUnionCrossingMain = `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/lib"
)

type TDWORD = uint32
type TLONG = int32
type TLONGLONG = int64

type TLARGE_INTEGER = struct {
	Fu [0]struct {
		FLowPart  TDWORD
		FHighPart TLONG
	}
	FQuadPart  [0]TLONGLONG
	F__ccgo0_0 struct {
		FLowPart  TDWORD
		FHighPart TLONG
	}
}

func main() {
	var raw [1]uint64
	li := (*TLARGE_INTEGER)(unsafe.Pointer(&raw))
	li.F__ccgo0_0.FLowPart = 5
	li.F__ccgo0_0.FHighPart = 6
	fmt.Println(raw[0], unsafe.Sizeof(*li))
	fmt.Println(lib.Seek(*li))
	seek := *(*func(TLARGE_INTEGER) int64)(unsafe.Pointer(&struct{ uintptr }{lib.SeekWord}))
	fmt.Println(seek(*li))
}
`

// The emission: main's admitted union keeps its own lift and never names lib's unadmitted largeIntegerᴛ1.
func TestAdmittedUnionKeepsItsOwnLiftOverAnUnadmittedForeignUnion(t *testing.T) {
	isolateConverterPathGlobals(t)

	emitted := convertModuleMainWithLib(t, "lib", aliasSpelledLib, admittedUnionMain)

	if strings.Contains(emitted, "lib_package.largeIntegerᴛ1") {
		t.Errorf("an admitted union names the import's UNADMITTED lift (offset 32 where Go lays the field at 0):\n%s", emitted)
	}

	if !strings.Contains(emitted, "struct TLARGE_INTEGERᴛ1") {
		t.Errorf("the admitted union declares no lift of its own:\n%s", emitted)
	}
}

// End to end for the union: the converted module builds and reads native memory through main's admitted union, printing
// what go prints (the windows red panicked here). GO2CS_INLINE_BUILD=1 runs it.
func TestAdmittedUnionOverUnadmittedForeignUnionBuildsAndReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := map[string]string{"lib/lib.go": aliasSpelledLib, "main.go": admittedUnionMain}
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

// KNOWN LIMIT under COORD's ruling A (2026-11-17 07:20Z): a value of a struct the two packages admit differently cannot
// cross between them by value, nor through a func word, because each keeps its own lift (CS1503 at the crossing,
// measured 2026-11-17 on linux-x64). Retirement: ruling C (312) makes the packages agree, so the importer reuses the
// import's lift again; that seat un-skips this arm.
func TestAdmittedUnionByValueCrossingIsAKnownLimit(t *testing.T) {
	t.Skip("KNOWN LIMIT (COORD ruling A, 2026-11-17 07:20Z): a by-value crossing between two lifts of one Go type is CS1503; retired by ruling C in 312")

	dotnet := requireInlineBuild(t)
	files := map[string]string{"lib/lib.go": aliasSpelledLib, "main.go": admittedUnionCrossingMain}
	want := goRunInlineModule(t, "app", files)
	out := convertInlineModule(t, "app", files)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	if got := runConvertedInlineModule(t, dotnet, out); got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
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

// TRANSITIVE admission (G's linux-x64 red at 6bcc05d11f, COORD ruling 2026-11-17 10:32Z): libc's Titimerval named
// libc/uuid/uuid's lift Itimervalᴛ1, since neither package admits Itimerval and it holds no array, while its field type
// Ttimeval kept libc's OWN lift, which libc admits and uuid does not. libc's own literal
// `new Titimerval(Fit_value: new Ttimeval(...))` then passed libc's Ttimevalᴛ1 where uuid's lift declares uuid's: CS1503
// three times in ccgo_linux_amd64.cs. A foreign lift is reused only if every struct it holds, at any depth, through
// fields, arrays and embeds, resolves to that same package's lift here too.
//
// lib spells the structs the way uuid does and admits none of them.
const transitiveLiftLib = `package lib

type TV = struct {
	Sec  int64
	Usec int64
}

type ITV = struct {
	Interval TV
	Value    TV
}

type ITV2 = struct {
	Outer ITV
	Flags int32
}

func Usec(i ITV2) int64 { return i.Outer.Value.Usec }

const Name = "lib"
`

// main spells the same structs the way libc does. It converts Trusage whole through unsafe.Pointer, so it admits Trusage
// and Ttv, which Trusage holds by value (libc admits Ttimeval the same way). It admits neither Titv nor Titv2: Titv holds
// Ttv (depth 1) and Titv2 holds it through Titv (depth 2).
const transitiveLiftMain = `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/lib"
)

type Ttv = struct {
	Sec  int64
	Usec int64
}

type Titv = struct {
	Interval Ttv
	Value    Ttv
}

type Titv2 = struct {
	Outer Titv
	Flags int32
}

type Trusage = struct {
	Utime Ttv
	Pad   [2]int64
}

func main() {
	var raw [4]int64
	ru := (*Trusage)(unsafe.Pointer(&raw))
	ru.Utime.Sec = 3
	ru.Pad[1] = 4
	itv := Titv{Value: Ttv{Sec: 5, Usec: 6}}
	itv2 := Titv2{Outer: Titv{Interval: Ttv{Sec: 7, Usec: 8}}, Flags: 9}
	fmt.Println(raw[0]+raw[3], itv.Value.Sec*10+itv.Value.Usec, itv2.Outer.Interval.Usec+int64(itv2.Flags), unsafe.Sizeof(itv2), lib.Name)
}
`

// The emission: neither container names lib's lift, since the struct each holds (directly, or through Titv) keeps main's
// own admitted lift.
func TestReusedLiftWhoseFieldKeepsItsOwnLiftIsNotReused(t *testing.T) {
	isolateConverterPathGlobals(t)

	emitted := convertModuleMainWithLib(t, "lib", transitiveLiftLib, transitiveLiftMain)

	if strings.Contains(emitted, "lib_package.ITVᴛ1") {
		t.Errorf("depth 1: Titv names lib's lift while its field type Ttv keeps main's own admitted lift (CS1503 at the literal):\n%s", emitted)
	}

	if strings.Contains(emitted, "lib_package.ITV2ᴛ1") {
		t.Errorf("depth 2: Titv2 names lib's lift while Ttv, held through Titv, keeps main's own admitted lift:\n%s", emitted)
	}

	if strings.Contains(emitted, "lib_package.TVᴛ1") {
		t.Errorf("the admitted Ttv names lib's unadmitted lift:\n%s", emitted)
	}
}

// The control: where main admits nothing (no unsafe.Pointer conversion), every field resolves to lib's lift too, so
// all three reuses stay.
func TestReusedLiftWhoseFieldsMatchIsStillReused(t *testing.T) {
	isolateConverterPathGlobals(t)

	notAdmitted := strings.Replace(transitiveLiftMain,
		"\tru := (*Trusage)(unsafe.Pointer(&raw))\n",
		"\tru := &Trusage{}\n", 1)

	if notAdmitted == transitiveLiftMain {
		t.Fatal("the control's edit did not apply")
	}

	emitted := convertModuleMainWithLib(t, "lib", transitiveLiftLib, notAdmitted)

	for _, want := range []string{
		"global using Ttv = go.example.com.app.lib_package.TVᴛ1;",
		"global using Titv = go.example.com.app.lib_package.ITVᴛ1;",
		"global using Titv2 = go.example.com.app.lib_package.ITV2ᴛ1;",
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("with matching admission at every depth the reuse is lost; want %q:\n%s", want, emitted)
		}
	}
}

// End to end: the converted module builds (CS1503 at both literals at 6bcc05d11f) and prints what go prints.
// GO2CS_INLINE_BUILD=1 runs it.
func TestTransitiveLiftReuseBuildsAndReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := map[string]string{"lib/lib.go": transitiveLiftLib, "main.go": transitiveLiftMain}
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
