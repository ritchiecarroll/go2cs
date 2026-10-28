// fieldAccessEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the field-access emission (fieldAccess.go): a field reached through a local whose every assignment
// reinterprets an unsafe.Pointer, over a struct that holds a managed reference, reads as Go spells it,
// `buf.Offset = off`, through a generated field property, and the package's package_info.cs records the struct
// (`[assembly: GoFieldAccess(typeof(T))]`). The controls fall outside the provenance rule: a local that is also assigned a plain
// pointer, and a struct with no managed reference. Since parity seat #31 P1 serves them, with the same spelling.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertFieldAccessFixture(t *testing.T) (string, string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

type inner struct{ X, Y uint16 }

type hdr struct {
	Tag   uint32
	Count uint16
}

type sized struct{ n int }

func (s *sized) Len() int { return s.n }

type buffer struct {
	Name   string
	Offset uint16
	In     inner
	hdr
	Value uint32
	Path  [4]uint16
}

// span's Len field shadows the Len method its embedded sized promotes.
type span struct {
	Name string
	sized
	Len uintptr
}

// POSITIVE -- every assignment reinterprets an unsafe.Pointer, and buffer holds a string.
func reinterpreted(blob []byte, off uint16) {
	buf := (*buffer)(unsafe.Pointer(&blob[0]))
	buf.Offset = off
	buf.Offset += 2
	buf.In.X = 7
	buf.Tag = 9
	buf.Value = 3
	buf.Path[1] = 5
	n := copy(buf.Path[:2], []uint16{1, 2})
	p := &buf.Offset
	fmt.Println(buf.Offset, buf.In.Y, *p, n, buf.Count, buf.Path[1])

	s := (*span)(unsafe.Pointer(&blob[0]))
	s.Len = 3
	fmt.Println(s.Len)
}

// CONTROL -- the local is also assigned a plain pointer.
func repointed(blob []byte) {
	buf := (*buffer)(unsafe.Pointer(&blob[0]))
	buf = new(buffer)
	buf.Offset = 1
	fmt.Println(buf.Offset)
}

// A struct a hand-owned file declares is private to the package class, out of the property's reach.
func handOwned(blob []byte) {
	fr := (*frame)(unsafe.Pointer(&blob[0]))
	fr.Count = 4
	fmt.Println(fr.Count)
}

// CONTROL -- hdr holds no managed reference.
func scalarOnly(blob []byte) {
	h := (*hdr)(unsafe.Pointer(&blob[0]))
	h.Tag = 1
	fmt.Println(h.Tag)
}

func main() {
	blob := make([]byte, 64)
	reinterpreted(blob, 4)
	handOwned(blob)
	repointed(blob)
	scalarOnly(blob)
}
`)

	// frame's file converts into a hand-owned destination, so its emission goes to the review sibling.
	writeModuleFile(t, filepath.Join(appDir, "frozen.go"), "package main\n\ntype frame struct {\n\tName  string\n\tCount uint16\n}\n")
	writeModuleFile(t, filepath.Join(root, "out", "src", "example.com", "fieldaccess", "frozen.cs"), "[module: go.GoManualConversion]\n\nnamespace go;\n")

	out := convertFieldAccessModule(t, root)

	return readGenerated(t, filepath.Join(out, "main.cs")), readGenerated(t, filepath.Join(out, PackageInfoFileName))
}

// convertFieldAccessModule converts the fixture module under root/app (example.com/fieldaccess) into root/out and
// answers the module's output directory.
func convertFieldAccessModule(t *testing.T, root string) string {
	t.Helper()

	appDir := filepath.Join(root, "app")
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return filepath.Join(options.go2csPath, "src", "example.com", "fieldaccess")
}

// TestFieldAccessReadsThroughTheFieldProperty requires the scoped sites to read as Go spells them, the collision,
// array-element and array-slice sites to take their box forms, the file to carry its record, and both controls
// to keep today's emission.
func TestFieldAccessReadsThroughTheFieldProperty(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs, info := convertFieldAccessFixture(t)
	scoped := funcBody(t, cs, "void reinterpreted(")

	for _, want := range []string{
		"buf.Offset = off;",
		"buf.Offset += 2;",
		"buf.In.X = 7;",
		"buf.hdr.Tag = 9;",
		"buf.of(buffer.ᏑValue).Value = 3;",
		"buf.of(buffer.ᏑPath).ElementRef(1) = 5;",
		"copy(buf.of(buffer.ᏑPath).slice(0, 2),",
		"buf.of(buffer.ᏑOffset)",
		"fmt.Println(buf.Offset, buf.In.Y,",
		"buf.hdr.Count, buf.of(buffer.ᏑPath).ElementRef(1)",
		"s.of(span.ᏑLen).Value = 3;",
	} {
		if !strings.Contains(scoped, want) {
			t.Errorf("a scoped field access does not take its field-access form (want %q):\n%s", want, scoped)
		}
	}

	if strings.Contains(scoped, "buf.Value.Offset") || strings.Contains(scoped, "(~buf)") {
		t.Errorf("a scoped field access still reads through the whole value:\n%s", scoped)
	}

	// hdr is listed by its P1 site in scalarOnly (fieldPropertyEmission_test.go): one record serves both routes.
	if got := strings.Join(fieldAccessRecordsOf(info), " "); got != "buffer hdr" || strings.Contains(cs, "GoFieldAccess") {
		t.Errorf("package_info.cs records %q, want \"buffer hdr\", and the file carries none:\n%s", got, info)
	}

	if handOwned := funcBody(t, cs, "void handOwned("); !strings.Contains(handOwned, "fr.of(frame.ᏑCount).Value = 4;") ||
		!strings.Contains(handOwned, "fmt.Println(fr.of(frame.ᏑCount).Value);") {
		t.Errorf("a field of a hand-owned file's struct does not take the explicit form:\n%s", handOwned)
	}

	// The controls are outside the provenance rule, so P1 serves them (parity seat #31): the same spelling, and a field
	// named like a member of ж<T> keeps today's box form rather than this route's explicit one.
	if repointed := funcBody(t, cs, "void repointed("); !strings.Contains(repointed, "buf.Offset = 1;") {
		t.Errorf("the re-pointed control does not take P1's spelling:\n%s", repointed)
	}

	if scalarOnly := funcBody(t, cs, "void scalarOnly("); !strings.Contains(scalarOnly, "h.Tag = 1;") {
		t.Errorf("the scalar-only control does not take P1's spelling:\n%s", scalarOnly)
	}
}
