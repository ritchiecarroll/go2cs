// implementRecordKey_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/token"
	"go/types"
	"testing"
)

// The whole point of the key: the spelling a dependency's package_info.cs produces and the spelling
// a cast site produces must be ONE string. Before this, they agreed only when the dependency's
// import path was a SINGLE segment — every nested path missed, silently, and the cast emitted a
// redundant ᴠ value adapter (478 `color_ΔRGBAᴠColor` + 79 `binary_*ᴠByteOrder` in the corpus).
//
// Each case states BOTH compositions of one real pair and requires them equal:
//   - LOAD: implementRecordKey(recordingPkg, <record's T>, <record's Iface>, recordingPkg)
//   - USE:  implementRecordKey(targetPkg,    <rendered target>, <rendered iface>, ifacePkg)
func TestImplementRecordKeyBothCompositionsAgree(t *testing.T) {
	cases := []struct {
		name string
		// load side
		recordingPackage string
		recordType       string
		recordIface      string
		// use side
		targetPackage  string
		renderedTarget string
		renderedIface  string
		ifacePackage   string
		// expected shared key
		want string
	}{
		{
			// The case that already worked, and the reason the defect stayed invisible: a
			// single-segment path has no namespace chain to lose.
			name:             "single-segment path (io)",
			recordingPackage: "io", recordType: "noBody", recordIface: "ReadCloser",
			targetPackage: "io", renderedTarget: "io_package.noBody",
			renderedIface: "io_package.ReadCloser", ifacePackage: "io",
			want: "io|noBody|io_package.ReadCloser",
		},
		{
			// The 79-site case: `encoding.binary_package.ByteOrder` never matched
			// `binary_package.ByteOrder`.
			name:             "multi-segment path (encoding/binary)",
			recordingPackage: "binary", recordType: "bigEndian", recordIface: "ByteOrder",
			targetPackage: "binary", renderedTarget: "encoding.binary_package.bigEndian",
			renderedIface: "encoding.binary_package.ByteOrder", ifacePackage: "binary",
			want: "binary|bigEndian|binary_package.ByteOrder",
		},
		{
			// The 478-site case, and TWO divergences stacked: image/color's `RGBA` is
			// collision-renamed to `ΔRGBA`, so the record's TYPE side differed as well as the
			// interface side. Both sides now reduce the emitted C# name.
			name:             "collision-renamed type through a multi-segment path (image/color)",
			recordingPackage: "color", recordType: "ΔRGBA", recordIface: "Color",
			targetPackage: "color", renderedTarget: "image.color_package.ΔRGBA",
			renderedIface: "image.color_package.Color", ifacePackage: "color",
			want: "color|ΔRGBA|color_package.Color",
		},
		{
			// A record whose interface side is already whole (a package recording a pair against
			// a FOREIGN interface) reduces to the same class-relative spelling.
			name:             "record carrying a fully-qualified interface (image/color Palette)",
			recordingPackage: "color", recordType: "Palette", recordIface: "go.image.color_package.Model",
			targetPackage: "color", renderedTarget: "image.color_package.Palette",
			renderedIface: "image.color_package.Model", ifacePackage: "color",
			want: "color|Palette|color_package.Model",
		},
		{
			// The universe `error` is not a package member and must stay bare on both sides.
			name:             "universe error",
			recordingPackage: "syscall", recordType: "Errno", recordIface: "error",
			targetPackage: "syscall", renderedTarget: "syscall_package.Errno",
			renderedIface: "error", ifacePackage: "syscall",
			want: "syscall|Errno|error",
		},
		{
			// POINTER set, 34 of its 66 corpus sites: text/template/parse records its own `Node`
			// BARE while the cast site in text/template renders the whole chain.
			name:             "pointer record, bare iface vs whole chain (text/template/parse)",
			recordingPackage: "parse", recordType: "ListNode", recordIface: "Node",
			targetPackage: "parse", renderedTarget: "text.template.parse_package.ListNode",
			renderedIface: "text.template.parse_package.Node", ifacePackage: "parse",
			want: "parse|ListNode|parse_package.Node",
		},
		{
			// POINTER set, and the divergence runs the OTHER way — go/types records its own
			// `Object` WHOLE where the cast site renders it short. Neither side's raw text is
			// reliably the longer one, which is why the key is a canonical form, not a heuristic.
			name:             "pointer record, whole iface vs short chain (go/types)",
			recordingPackage: "types", recordType: "TypeName", recordIface: "go.go.types_package.Object",
			targetPackage: "types", renderedTarget: "go.types_package.TypeName",
			renderedIface: "go.types_package.Object", ifacePackage: "types",
			want: "types|TypeName|types_package.Object",
		},
		{
			// POINTER set, TYPE-side divergence: image's `RGBA` is collision-renamed to `ΔRGBA`
			// (its `RGBA()` method), so the record and the rendered target must both reduce the
			// EMITTED name — naming the Go type on the use side missed every renamed implementer.
			name:             "pointer record, collision-renamed type (image)",
			recordingPackage: "image", recordType: "ΔRGBA", recordIface: "Image",
			targetPackage: "image", renderedTarget: "image_package.ΔRGBA",
			renderedIface: "image_package.Image", ifacePackage: "image",
			want: "image|ΔRGBA|image_package.Image",
		},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			load := implementRecordKey(tc.recordingPackage, tc.recordType, tc.recordIface, tc.recordingPackage)
			use := implementRecordKey(tc.targetPackage, tc.renderedTarget, tc.renderedIface, tc.ifacePackage)

			if load != tc.want {
				t.Errorf("load-side key = %q, want %q", load, tc.want)
			}

			if use != tc.want {
				t.Errorf("use-side key = %q, want %q", use, tc.want)
			}
		})
	}
}

// The SIMPLE interface name alone is ambiguous — image's Paletted→image.Image record must not
// satisfy a Paletted→draw.Image cast (both reduce to "Image"). Collapsing the namespace chain keeps
// the package CLASS, which is what discriminates them. This is what the POINTER set's
// `shade.Level` negative guards behaviorally (ForeignPointerImplementSuppression): trusting a
// same-simple-name match there would reference the adapter for the WRONG interface, CS1503.
func TestImplementRecordKeyKeepsPackageClassDiscrimination(t *testing.T) {
	imageKey := implementRecordKey("image", "Paletted", "image_package.Image", "image")
	drawKey := implementRecordKey("image", "Paletted", "image.draw_package.Image", "draw")

	if imageKey == drawKey {
		t.Fatalf("image.Image and draw.Image collapsed to one key: %q", imageKey)
	}

	if want := "image|Paletted|image_package.Image"; imageKey != want {
		t.Errorf("image key = %q, want %q", imageKey, want)
	}

	if want := "image|Paletted|draw_package.Image"; drawKey != want {
		t.Errorf("draw key = %q, want %q", drawKey, want)
	}
}

// A member path UNDER the package class must survive the collapse intact — the rule keeps the tail
// from the `_package` segment on rather than taking the last two segments.
func TestCanonicalImplementRecordIfaceNameKeepsNestedMemberPath(t *testing.T) {
	if got, want := canonicalImplementRecordIfaceName("go.x.y_package.Outer.Inner", "y"), "y_package.Outer.Inner"; got != want {
		t.Errorf("canonicalImplementRecordIfaceName = %q, want %q", got, want)
	}
}

// THE NEGATIVE that gates the whole change. A value record says the declaring assembly implements
// the pair; it does NOT say HOW. go2cs-gen makes every named Go type a `partial struct` that really
// does implement the interface — except a named FUNC type, which is a C# DELEGATE and takes the
// adapter-CLASS route instead. Trusting that record drops the consumer's local adapter and hands a
// bare delegate to an interface slot: CS0029 for net/http's `HandlerFunc` → `ΔHandler` in expvar,
// net/http/cgi and three more.
func TestValueRecordRealizesAsPartialStruct(t *testing.T) {
	pkg := types.NewPackage("net/http", "http")

	newNamed := func(name string, underlying types.Type) *types.Named {
		obj := types.NewTypeName(token.NoPos, pkg, name, nil)
		return types.NewNamed(obj, underlying, nil)
	}

	// `type HandlerFunc func(ResponseWriter, *Request)` — a DELEGATE in C#.
	handlerFunc := newNamed("HandlerFunc", types.NewSignatureType(nil, nil, nil,
		types.NewTuple(types.NewVar(token.NoPos, pkg, "w", types.Typ[types.Int])), nil, false))

	if valueRecordRealizesAsPartialStruct(handlerFunc) {
		t.Error("a named FUNC type was reported as realized by a partial struct — its record must NOT be trusted")
	}

	// Every other named Go shape IS a partial struct in the emitted C#: a struct, a named slice
	// (`[GoType(\"[]Color\")] partial struct Palette`), a named numeric
	// (`[GoType(\"num:nint\")] partial struct ΔSignal`), a map, a channel.
	trusted := map[string]types.Type{
		"struct":  types.NewStruct(nil, nil),
		"slice":   types.NewSlice(types.Typ[types.Uint8]),
		"numeric": types.Typ[types.Uintptr],
		"map":     types.NewMap(types.Typ[types.String], types.Typ[types.Int]),
		"chan":    types.NewChan(types.SendRecv, types.Typ[types.Int]),
		"pointer": types.NewPointer(types.Typ[types.Int]),
	}

	for name, underlying := range trusted {
		if !valueRecordRealizesAsPartialStruct(newNamed(name, underlying)) {
			t.Errorf("named %s type was not reported as realized by a partial struct", name)
		}
	}

	// An unnamed type carries no record at all.
	if valueRecordRealizesAsPartialStruct(types.NewStruct(nil, nil)) {
		t.Error("an unnamed type must not be reported as record-realized")
	}
}
