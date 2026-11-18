// aliasSpelledLiftReuse_test.go - Gbtc
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

// W18 (G, the sqlite .db demo on win-x64, its WAL pass): modernc.org/sqlite/lib spells TLARGE_INTEGER's fields through
// its own aliases (`LowPart DWORD`), while libc spells the identical struct through uint32. Go calls the two one type,
// but the importer's types.Struct.String() names the alias (`example.com/app.DW`) where the declaring package's lift
// record is keyed by ITS spelling (`uint32`), so the S23 reuse missed: lib lifted a second struct, and a func word
// minted from libc's function could not be read through lib's func type ("func cookie ... holds a Func`6, which cannot
// be read as Func`6" in lib._winHandleSeek). The lookup now keys on the identical struct's own spelling in the import.
const aliasSpelledLib = `package lib

import "unsafe"

type L = struct {
	Lo uint32
	Hi int32
}

func Sum(x L) int64 { return int64(x.Lo) + int64(x.Hi)*10 }

func word(f interface{}) uintptr { return (*[2]uintptr)(unsafe.Pointer(&f))[1] }

// Word is Sum carried as a number, the way ccgo carries a function pointer.
var Word = word(Sum)

// largeInteger is libc's ccgo union, verbatim: its [0] fields make its lift carry [GoValueClone], so its public
// record is an attributed line.
type largeInteger = struct {
	Fu [0]struct {
		FLowPart  uint32
		FHighPart int32
	}
	FQuadPart  [0]int64
	F__ccgo0_0 struct {
		FLowPart  uint32
		FHighPart int32
	}
}

func Seek(d largeInteger) int64 { return int64(d.F__ccgo0_0.FLowPart) + int64(d.F__ccgo0_0.FHighPart)*100 }

// SeekWord is Seek as a number: XSetFilePointerEx's word in sqlite's _aSyscall.
var SeekWord = word(Seek)
`

// The importer spells the identical struct through a local alias of uint32 and reads lib's word through a func type
// over its own spelling.
const aliasSpelledMain = `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/lib"
)

type DW = uint32

type LA = struct {
	Lo DW
	Hi int32
}

// sqlite/lib's TLARGE_INTEGER: libc's union spelled through lib's own aliases.
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
	sum := *(*func(LA) int64)(unsafe.Pointer(&struct{ uintptr }{lib.Word}))
	fmt.Println(sum(LA{Lo: 3, Hi: 4}))

	var li TLARGE_INTEGER
	li.F__ccgo0_0.FLowPart = 5
	li.F__ccgo0_0.FHighPart = 6
	seek := *(*func(TLARGE_INTEGER) int64)(unsafe.Pointer(&struct{ uintptr }{lib.SeekWord}))
	fmt.Println(seek(li))
}
`

// The emission: LA names lib's lift, and main declares no struct of its own for it.
func TestAliasSpelledFieldStructReusesTheImportsLift(t *testing.T) {
	isolateConverterPathGlobals(t)

	emitted := convertModuleMainWithLib(t, "lib", aliasSpelledLib, aliasSpelledMain)

	if !strings.Contains(emitted, "global using LA = go.example.com.app.lib_package.Lᴛ1;") {
		t.Errorf("LA does not name lib's lift of the identical struct:\n%s", emitted)
	}

	if strings.Contains(emitted, "struct LAᴛ1") {
		t.Errorf("main declares a second struct for a type Go calls the same:\n%s", emitted)
	}
}

// The union: libc's largeInteger record is `[GoValueClone("Fu", "FQuadPart")] public partial struct largeIntegerᴛ1 {}`,
// an attributed line the public-lift reader did not match, so the lift was never reusable whatever the spelling (W18's
// second root: TLARGE_INTEGER stayed lib's own type).
func TestAliasSpelledUnionReusesTheImportsAttributedLift(t *testing.T) {
	isolateConverterPathGlobals(t)

	emitted := convertModuleMainWithLib(t, "lib", aliasSpelledLib, aliasSpelledMain)

	if !strings.Contains(emitted, "global using TLARGE_INTEGER = go.example.com.app.lib_package.largeIntegerᴛ1;") {
		t.Errorf("TLARGE_INTEGER does not name lib's lift of the identical union:\n%s", emitted)
	}

	if strings.Contains(emitted, "struct TLARGE_INTEGERᴛ1") {
		t.Errorf("main declares a second union for a type Go calls the same:\n%s", emitted)
	}
}

// The public-lift record reader takes a record whatever attributes precede it, and still only a public one.
func TestPublicLiftRecordPatternReadsAnAttributedRecord(t *testing.T) {
	cases := map[string]string{
		`    public partial struct largeIntegerᴛ1_Fu {}`:                                  "largeIntegerᴛ1_Fu",
		`    [GoValueClone("Fu", "FQuadPart")] public partial struct largeIntegerᴛ1 {}`:   "largeIntegerᴛ1",
		`    [GoMemberRecord("a")] [GoValueClone("b")] public partial struct Pairᴛ1 {}`:   "Pairᴛ1",
		`    [GoValueClone("a]b", "c\"d]")] public partial struct Quoteᴛ1 {}`:             "Quoteᴛ1",
		`    [GoValueClone("Fu", "FQuadPart")] internal partial struct largeIntegerᴛ1 {}`: "",
		`    [GoValueClone("Fu")] public partial struct largeIntegerᴛ1 { public int x; }`: "",
	}

	for line, want := range cases {
		got := ""

		if match := publicLiftRecordPattern.FindStringSubmatch(line); match != nil {
			got = match[1]
		}

		if got != want {
			t.Errorf("%q reads as %q, want %q", line, got, want)
		}
	}
}

// The controls: a struct that is NOT identical (a field of another type behind the alias) keeps its own lift, and so
// does an identical struct whose import lift is internal.
func TestAliasSpelledFieldStructKeepsItsOwnLiftWhenNotIdentical(t *testing.T) {
	isolateConverterPathGlobals(t)

	notIdentical := strings.Replace(aliasSpelledMain, "type DW = uint32", "type DW = uint16", 1)
	emitted := convertModuleMainWithLib(t, "lib", aliasSpelledLib, strings.Replace(notIdentical,
		"\tsum := *(*func(LA) int64)(unsafe.Pointer(&struct{ uintptr }{lib.Word}))\n\tfmt.Println(sum(LA{Lo: 3, Hi: 4}))",
		"\tfmt.Println(LA{Lo: 3, Hi: 4}, unsafe.Sizeof(lib.Word))", 1))

	if strings.Contains(emitted, "lib_package.Lᴛ1") {
		t.Errorf("a struct that is not identical names lib's lift:\n%s", emitted)
	}

	if !strings.Contains(emitted, "struct LAᴛ1") {
		t.Errorf("a struct that is not identical declares no lift of its own:\n%s", emitted)
	}
}

// End to end: the converted program reads lib's word through LA's func type and prints what go prints. Red at the base
// it panics in the func cookie read. GO2CS_INLINE_BUILD=1 runs it.
func TestAliasSpelledFieldStructFuncWordReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := map[string]string{"lib/lib.go": aliasSpelledLib, "main.go": aliasSpelledMain}
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
