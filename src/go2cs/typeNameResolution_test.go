// typeNameResolution_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/token"
	"go/types"
	"os"
	"os/exec"
	"runtime/debug"
	"strings"
	"testing"
)

// unclosedBracketProbeEnv marks the CHILD process of TestUnclosedBracketTerminates.
const unclosedBracketProbeEnv = "GO2CS_TEST_UNCLOSED_BRACKET_PROBE"

// TestImportPathStart pins where a rendered type string's package path begins once a type
// CONSTRUCTOR precedes it. Everything before that index is the renderer's own text and must never
// reach convertImportPathToNamespace, which would sanitize the `-` of `<-chan` into `<_chan`
// (issue #33). The hyphenated and dotted paths are why the delimiter cannot simply be "not an
// identifier character" — `-` and `.` are both legal inside a path element, so the scan has to stop
// on the constructor's own punctuation instead.
func TestImportPathStart(t *testing.T) {
	cases := map[string]int{
		"io/fs": 0, // bare path — no constructor
		"go.mongodb.org/mongo-driver/mongo/description":        0, // …with a dotted host and a hyphen
		"<-chan go.mongodb.org/mongo-driver/mongo/description": 7, // the reported shape
		"chan io/fs":                        5,
		"chan<- io/fs":                      7,
		"*io/fs":                            1,
		"[]io/fs":                           2,
		"[2]io/fs":                          3,
		"map[string]io/fs":                  11,
		"func(io/fs":                        5,
		"[]*io/fs":                          3, // stacked constructors
		"gopkg.in/yaml.v3":                  0, // dots and digits stay inside the path
		"vendor/golang.org/x/text/internal": 0,

		// The converter's own synthetic markers are multi-byte runes, and treating one as a
		// delimiter strands the scan INSIDE the type name, leaving the package path in front of it
		// unconverted. Both lifted-alias goldens below regressed exactly that way on the first cut
		// of this fix; CNR caught them (see isImportPathByte).
		"main_package/entry" + TempVarMarker + "1":                  0,
		"NestedAliasUser.inner_package/Entry" + TempVarMarker + "1": 0,
		"<-chan main_package/entry" + TempVarMarker + "1":           7, // …and a marker still does not hide a constructor
		"pkg/T" + AddressPrefix + TypeAliasDot + "x":                0, // every marker rune, not just ᴛ
	}

	for in, want := range cases {
		if got := importPathStart(in); got != want {
			t.Errorf("importPathStart(%q) = %d, want %d (path would be %q)", in, got, want, in[min(got, len(in)):])
		}
	}
}

// TestConvertToCSFullTypeNameConstructedModulePaths is issue #33's THIRD wall, at the type-name
// renderer: a `-recurse` of a project importing go.mongodb.org/mongo-driver died with
// `fatal error: stack overflow` on
//
//	type Pool struct { descChan <-chan description.Topology }
//
// The fully-qualified render of that field is `<-chan <import-path>_package.Topology`, and because
// the import path is MULTI-SEGMENT (it has a `/`), convertToCSFullTypeName's package-path branch
// fired first and rewrote the string from index 0 — turning `<-chan ` into `<_chan `, which no
// channel branch recognizes. The mangled string then fell into the array branch and re-entered the
// renderer on itself forever. A single-segment path (`<-chan time_package.Time`) never had a slash
// to trigger any of this, which is why the whole standard library renders correctly and only a
// module dependency could reach it.
//
// Every constructor is covered, not just the reported one: the same index-0 rewrite mangled a
// pointer/slice/array/map/func element of a multi-segment path too — silently, without a crash.
func TestConvertToCSFullTypeNameConstructedModulePaths(t *testing.T) {
	const mongoPath = "go.mongodb.org/mongo-driver/mongo/description_package.Topology"
	const mongoName = "go.mongodb.org.mongo_driver.mongo.description_package.Topology"

	// mongoName is the RELATIVE C# name — the emitted namespace `go.go.mongodb.org.…` minus the
	// root, which convertToCSTypeName strips from every nested render (the leading `go` that
	// remains is the module HOST, not the root namespace). rootQualifyIfAmbiguous puts the root
	// back at the emission site, which is why session_pool.cs reads
	// `using description = global::go.go.mongodb.org.mongo_driver.mongo.description_package;`.
	// Only a TOP-level render keeps the root, so the nested cases below carry one fewer `go.`.
	cases := map[string]string{
		// The reported declaration, in all three channel directions.
		"<-chan " + mongoPath: fmt.Sprintf("%s./*<-*/channel<%s>", RootNamespace, mongoName),
		"chan<- " + mongoPath: fmt.Sprintf("%s.channel/*<-*/<%s>", RootNamespace, mongoName),
		"chan " + mongoPath:   fmt.Sprintf("%s.channel<%s>", RootNamespace, mongoName),

		// The same defect without the crash: a constructor whose text survives sanitization by
		// luck (`*`, `[]`, `[2]`, `map[…]` contain no hyphen) still had its path rewritten from
		// index 0, and only `<-chan`/`chan<-` carry the `-` that made the rewrite destructive.
		"*" + mongoPath:           fmt.Sprintf("%s.%s<%s>", RootNamespace, PointerPrefix, mongoName),
		"[]" + mongoPath:          fmt.Sprintf("%s.slice<%s>", RootNamespace, mongoName),
		"[2]" + mongoPath:         fmt.Sprintf("%s.array<%s>", RootNamespace, mongoName),
		"map[string]" + mongoPath: fmt.Sprintf("%s.map<@string, %s>", RootNamespace, mongoName),

		// A nested constructor — the mangling one level down, where the leading `[` of the slice
		// also truncates the path scan (the `else` half of the same rewrite).
		"<-chan []" + mongoPath: fmt.Sprintf("%s./*<-*/channel<slice<%s>>", RootNamespace, mongoName),

		// A path with no constructor is untouched by the fix — the byte-identical-corpus case —
		// and so is a SINGLE-segment path, which has no slash to enter the rewrite at all. That
		// is precisely why the standard library never reached this defect.
		mongoPath:                  RootNamespace + "." + mongoName,
		"io/fs_package.DirEntry":   RootNamespace + ".io.fs_package.DirEntry",
		"<-chan time_package.Time": fmt.Sprintf("%s./*<-*/channel<time_package.Time>", RootNamespace),
	}

	for in, want := range cases {
		if got := convertToCSFullTypeName(in); got != want {
			t.Errorf("convertToCSFullTypeName(%q)\n  = %q\nwant %q", in, got, want)
		}
	}

	// KNOWN RESIDUAL, pinned so it is a decision rather than an accident. When the OUTERMOST
	// constructor is the `[]`, the leading `[` is read as the start of a generic argument list and
	// truncates the path scan to nothing, so the `<-chan` inside is still mangled. Reaching it
	// needs a slice OF receive-only channels of a module-path type — rare next to the reported
	// field, and the fix for it is to stop treating a LEADING `[` as a generic bracket, which
	// re-routes every `[]<pkg>/<sub>.T` in the corpus through the other branch (a `_package`-suffix
	// change) and does not belong in a crash fix.
	//
	// What matters is that it no longer takes the run down: the bound in the array branch turns it
	// into a named warning and a finite (wrong) name. That is the property being pinned here — if a
	// later change makes this render correctly, delete this block; if one makes it recurse, the
	// test hangs the package rather than silently returning.
	if got := convertToCSFullTypeName("[]<-chan " + mongoPath); !strings.Contains(got, "<_chan") {
		t.Logf("residual `[]<-chan <module path>` now renders %q — if this is correct, fold it into the table above", got)
	}
}

// TestUnclosedBracketTerminates is the crash-proofing half, and it runs in a CHILD PROCESS on
// purpose: the defect it guards is a Go STACK OVERFLOW, which is a fatal runtime error rather than
// a panic — unrecoverable, so it cannot be asserted in-process and, in production, the conversion
// driver's per-file recover could not contain it either. That is what made one unrenderable type
// in package 1456 of 1726 kill an entire `-recurse` run instead of failing its own package.
//
// The child lowers the stack limit so a REGRESSION fails in milliseconds rather than growing a
// gigabyte of stack first: revert the `>`-presence check in convertToCSFullTypeName's array branch
// and this test reports `fatal error: stack overflow` from the child.
func TestUnclosedBracketTerminates(t *testing.T) {
	// The exact string the mangling produced — a `<` prefix with no `]` anywhere, which used to
	// re-enter the renderer on itself. Kept verbatim so the guard is proven on the real shape.
	const mangled = "<_chan go.mongodb.org.mongo_driver.mongo.description_package.Topology"

	if os.Getenv(unclosedBracketProbeEnv) == "1" {
		debug.SetMaxStack(4 << 20)
		fmt.Fprintf(os.Stdout, "rendered: %s\n", convertToCSFullTypeName(mangled))
		return
	}

	probe := exec.Command(os.Args[0], "-test.run=^TestUnclosedBracketTerminates$", "-test.timeout=60s", "-test.v")
	probe.Env = append(os.Environ(), unclosedBracketProbeEnv+"=1")

	output, err := probe.CombinedOutput()

	if err != nil {
		t.Fatalf("rendering %q did not terminate: %v\n%s", mangled, err, output)
	}

	// Terminating is the guard; naming the shape it could not render is what makes the failure
	// actionable instead of a silent wrong name.
	if !strings.Contains(string(output), mangled) {
		t.Errorf("unrenderable type expression was not reported by name; child output:\n%s", output)
	}
}

// TestGoArrayDimsAttribute pins the ONE datum a Go array type carries that the managed emission
// cannot: its LENGTH. `[32]byte` renders as golib `array<byte>` and C# has no const generic to hold
// the 32, so every other position recovers it from a live source — a value measures itself, a
// struct field reads the declaring type's zero instance — while a func PARAMETER has neither and
// gets the attribute instead. The cases below are the boundary: an unnamed array is stamped
// (outermost dimension first), an ALIAS for one is stamped because a Go alias IS its target type,
// a POINTER to one is stamped with its POINTEE's dims — `*[N]T` is where a caller allocates from the
// parameter type (net/rpc's every reply), so that pointee is a type-only position too — and a
// DEFINED (named) array type is NOT: its managed form is a generated wrapper rather than
// `array<T>`, so dims cargo could not be consumed even if it were carried. One pointer hop only,
// which is all a Go signature ever spells here.
func TestGoArrayDimsAttribute(t *testing.T) {
	byteType := types.Typ[types.Byte]
	intType := types.Typ[types.Int]
	pkg := types.NewPackage("go2cs/test", "test")

	array32 := types.NewArray(byteType, 32)
	nested := types.NewArray(types.NewArray(intType, 3), 2)
	aliased := types.NewAlias(types.NewTypeName(token.NoPos, pkg, "words", nil), types.NewArray(intType, 4))
	named := types.NewNamed(types.NewTypeName(token.NoPos, pkg, "Row", nil), types.NewArray(intType, 3), nil)

	cases := []struct {
		name string
		typ  types.Type
		want string
	}{
		{"unnamed array", array32, "[GoArrayDims(32)] "},
		{"nested array", nested, "[GoArrayDims(2, 3)] "},
		{"alias for an array", aliased, "[GoArrayDims(4)] "},
		{"defined array type", named, ""},
		{"slice", types.NewSlice(byteType), ""},
		{"scalar", intType, ""},
		{"pointer to array", types.NewPointer(array32), "[GoArrayDims(32)] "},
		{"pointer to nested array", types.NewPointer(nested), "[GoArrayDims(2, 3)] "},
		{"pointer to a defined array type", types.NewPointer(named), ""},
		{"pointer to a slice", types.NewPointer(types.NewSlice(byteType)), ""},
		{"pointer to a pointer to an array", types.NewPointer(types.NewPointer(array32)), ""},
	}

	for _, c := range cases {
		if got := emitGoArrayDimsAttribute(c.typ); got != c.want {
			t.Errorf("%s: emitGoArrayDimsAttribute(%s) = %q, want %q", c.name, c.typ, got, c.want)
		}
	}

	// The dimensions are outermost-first, matching the Go source order `[2][3]int`, because that is
	// the order GoReflect.ArrayDimsOfValue produces and reflect.Type.Elem consumes (dims[1:]).
	if dims := goArrayDims(nested); len(dims) != 2 || dims[0] != 2 || dims[1] != 3 {
		t.Errorf("goArrayDims([2][3]int) = %v, want [2 3]", dims)
	}
}
