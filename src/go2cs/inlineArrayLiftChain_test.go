// inlineArrayLiftChain_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

// W18 RULING C ITEM (3), SHAPE (A) (COORD 2026-11-17 19:34Z): an importer follows its import's resolution. Import q
// reuses import r's lift of an anonymous struct; an importer of q alone spelling the identical struct found no lift in
// q (q declares none, it names r's), kept its own, and a value of that Go type could not cross from it to q by value.
// The sqlite closure on linux lost 5 reuses this way once the -recurse seed made libc reuse libc/uuid/uuid's lifts
// (sqlite/lib <- libc: Ttimespec, Ttimeval, Titimerval, Titimerspec, Tstat). The importer now takes the lift q's
// alias record names, with the same admission and nested checks, at the package that declares it.

// r declares the lift: Tv is converted whole and holds an array, so r admits it.
const liftChainR = `package r

import "unsafe"

type Tv = struct {
	Sec int64
	Pad [2]int32
}

func Read(p unsafe.Pointer) int64 { return (*Tv)(p).Sec }
`

// q spells the identical struct, admits it, and reuses r's lift of it.
const liftChainQ = `package q

import (
	"unsafe"

	"example.com/app/r"
)

type Ts = struct {
	Sec int64
	Pad [2]int32
}

func Get(p unsafe.Pointer) int64 { return (*Ts)(p).Sec + r.Read(p) }

func Sum(v Ts) int64 { return v.Sec + int64(v.Pad[1]) }
`

// main imports q alone, admits its identical T, and passes it to q BY VALUE.
const liftChainMain = `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/q"
)

type T = struct {
	Sec int64
	Pad [2]int32
}

func main() {
	var raw [2]uint64
	t := (*T)(unsafe.Pointer(&raw))
	t.Sec = 7
	t.Pad[1] = 3
	fmt.Println(q.Sum(*t), q.Get(unsafe.Pointer(&raw)), raw[1])
}
`

func liftChainFiles() map[string]string {
	return map[string]string{"r/r.go": liftChainR, "q/q.go": liftChainQ, "main.go": liftChainMain}
}

// The emission: q reuses r's lift (the precondition), and main reuses it too, through q's record.
func TestImporterReusesTheLiftItsImportReuses(t *testing.T) {
	out := convertInlineModuleWith(t, "app", liftChainFiles(), "")

	q := readGeneratedPackage(t, filepath.Join(out, "q"))

	if !strings.Contains(q, "global using Ts = go.example.com.app.r_package.Tvᴛ1;") {
		t.Fatalf("PRECONDITION: q does not reuse r's lift, so there is no chain to follow:\n%s", q)
	}

	main := readGenerated(t, filepath.Join(out, "main.cs"))

	if want := "global using T = go.example.com.app.r_package.Tvᴛ1;"; !strings.Contains(main, want) {
		t.Errorf("main keeps its own lift of the struct q names through r; want %q:\n%s", want, main)
	}
}

// End to end: the by-value crossing builds and reads as go. GO2CS_INLINE_BUILD=1 runs it.
func TestChainedLiftCrossesByValueAndReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := liftChainFiles()
	want := goRunInlineModule(t, "app", files)
	out := convertInlineModuleWith(t, "app", files, "")

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints: %s; go2cs prints: %s", want, got)

	if got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
	}
}

// The negative arm: q reuses r's lift of Ov, but main's nested In resolves to package a's lift (a sorts before q and
// declares the identical In), not to the lift r's Ov holds. A reused Ov would type its In field as r's lift while main
// types its own In values as a's, so main keeps its own Ov.
const liftChainNestedR = `package r

import "unsafe"

type In = struct {
	A int32
	B [2]int32
}

type Ov = struct {
	In In
	X  int64
}

func Read(p unsafe.Pointer) int64 { return (*Ov)(p).X }
`

const liftChainNestedQ = `package q

import (
	"unsafe"

	"example.com/app/r"
)

type Is = struct {
	A int32
	B [2]int32
}

type Os = struct {
	In Is
	X  int64
}

func Get(p unsafe.Pointer) int64 { return (*Os)(p).X + r.Read(p) }
`

const liftChainNestedA = `package a

import "unsafe"

type Ia = struct {
	A int32
	B [2]int32
}

func Read(p unsafe.Pointer) int32 { return (*Ia)(p).A }
`

const liftChainNestedMain = `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/a"
	"example.com/app/q"
)

type I = struct {
	A int32
	B [2]int32
}

type O = struct {
	In I
	X  int64
}

func main() {
	var raw [3]uint64
	o := (*O)(unsafe.Pointer(&raw))
	o.X = 4
	fmt.Println(q.Get(unsafe.Pointer(&raw)), a.Read(unsafe.Pointer(&raw)))
}
`

func TestChainedLiftIsNotReusedWhenANestedStructResolvesElsewhere(t *testing.T) {
	out := convertInlineModuleWith(t, "app", map[string]string{"r/r.go": liftChainNestedR, "q/q.go": liftChainNestedQ,
		"a/a.go": liftChainNestedA, "main.go": liftChainNestedMain}, "")

	q := readGeneratedPackage(t, filepath.Join(out, "q"))

	if !strings.Contains(q, "global using Os = go.example.com.app.r_package.Ovᴛ1;") {
		t.Fatalf("PRECONDITION: q does not reuse r's lift of Ov, so the arm tests nothing:\n%s", q)
	}

	main := readGenerated(t, filepath.Join(out, "main.cs"))

	if !strings.Contains(main, "global using I = go.example.com.app.a_package.Iaᴛ1;") {
		t.Fatalf("PRECONDITION: main's nested In does not resolve to a's lift:\n%s", main)
	}

	if strings.Contains(main, "r_package.Ovᴛ1") {
		t.Errorf("main reuses r's Ov although its nested In resolves to a's lift, not r's:\n%s", main)
	}
}

// readGeneratedPackage returns every .cs file a converted package's directory holds, concatenated in name order.
func readGeneratedPackage(t *testing.T, dir string) string {
	t.Helper()

	paths, err := filepath.Glob(filepath.Join(dir, "*.cs"))

	if err != nil || len(paths) == 0 {
		t.Fatalf("no .cs files in %s: %v", dir, err)
	}

	var all strings.Builder

	for _, path := range paths {
		all.WriteString(readGenerated(t, path))
		all.WriteString("\n")
	}

	return all.String()
}
