// inlineArrayEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// convertInlineArrayFixture converts a one-file package and returns its emitted C#, line endings normalized.
func convertInlineArrayFixture(t *testing.T, pkg string, source string) string {
	t.Helper()
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/"+pkg+"\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, pkg+".go"), source)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		hoistLiterals:       true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", pkg, pkg+".cs")), "\r\n", "\n")
}

// C1's review, M3: a KEYED array literal that initializes an inline field. Go's keys are constants, so each element
// lands at its position (a positional element after a key takes the next one), the gaps are the element's zero value,
// and the constructor zero-fills past the last element. libc carries 37 such literals in types it admits
// (Foffsets: [8]uint8{1: uint8(4)} and kin).
func TestInlineArrayKeyedLiteralTakesItsPositions(t *testing.T) {
	cs := convertInlineArrayFixture(t, "keyed", `package keyed

import "unsafe"

type T struct {
	Off [8]uint8
	N   [3]int32
	E   [2]int16
}

func view(p unsafe.Pointer) *T { return (*T)(p) }

var V = T{Off: [8]uint8{1: uint8(4), 5, 6}, N: [3]int32{2: 9}, E: [2]int16{}}
`)

	for _, want := range []string{
		`Off: new array8<uint8>(default, (uint8)4, 5, 6)`,
		`N: new array3<int32>(default, default, 9)`,
		`E: new array2<int16>()`,
	} {
		if !strings.Contains(cs, want) {
			t.Errorf("want %q:\n%s", want, cs)
		}
	}
}

// Stop line S2 (DESIGN-array-value-storage.md, part D): golib's gate derived Go's layout from the very declaration it
// checks, so an S22 whose [0]int16 carrier was missing passed. The converter now stamps Go's size and alignment from
// go/types on the declaration, and the gate holds the managed layout to the stamp.
func TestInlineArrayDeclarationCarriesGosSizeAndAlignment(t *testing.T) {
	cs := convertInlineArrayFixture(t, "stamp", `package stamp

import "unsafe"

type S22 struct {
	FforceAlignment [0]int16
	Fa              [5]int8
	Fpad            [1]byte
}

type Outer struct {
	B byte
	S S22
}

func view(p unsafe.Pointer) *Outer { return (*Outer)(p) }
`)

	for _, want := range []string{
		`[GoInlineLayout(Size = 6, Align = 2), StructLayout(LayoutKind.Explicit, Size = 6)] partial struct S22`,
		`[GoInlineLayout(Size = 8, Align = 2), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Outer`,
	} {
		if !strings.Contains(cs, want) {
			t.Errorf("want %q:\n%s", want, cs)
		}
	}
}

// [GoInlineLayout] lives in go.golib, which a converted project's global usings do not import. The converter adds
// `using go.golib;` only where an admitted struct holds an array field itself, so a file whose admitted structs hold
// none (a plain struct admitted because an admitted struct holds it by value) failed to build: CS0246 in
// x/sys/windows' setupapi_windows.cs, release-smoke win-x64 run 43622476546 (COORD 2026-11-14 19:23Z). The stamp is
// spelled fully qualified, as the converter's other go.golib names are.
var inlineLayoutStampModule = map[string]string{
	"pair.go": `package stamponly

type Pair struct {
	X int32
	Y int32
}
`,
	"holder.go": `package stamponly

import "unsafe"

type Holder struct {
	P Pair
	B [4]byte
}

func View(p unsafe.Pointer) *Holder { return (*Holder)(p) }
`,
}

// convertInlineLayoutStampModule converts the two-file module and returns its output root.
func convertInlineLayoutStampModule(t *testing.T) string {
	t.Helper()
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/stamponly\n\ngo 1.23\n")

	for name, source := range inlineLayoutStampModule {
		writeModuleFile(t, filepath.Join(appDir, name), source)
	}

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{goRoot: goRoot, goPath: build.Default.GOPATH, go2csPath: filepath.Join(root, "out"), recurse: true, targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces: 4, preferVarDecl: true, useChannelOperators: true, hoistLiterals: true}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return filepath.Join(options.go2csPath, "src", "example.com", "stamponly")
}

func TestInlineArrayLayoutStampIsFullyQualified(t *testing.T) {
	out := convertInlineLayoutStampModule(t)
	pair := strings.ReplaceAll(readGenerated(t, filepath.Join(out, "pair.cs")), "\r\n", "\n")

	if strings.Contains(pair, "using go.golib;") {
		t.Fatalf("pair.cs must not import go.golib, or the fixture cannot show the stamp resolving alone:\n%s", pair)
	}

	if want := `[global::go.golib.GoInlineLayout(Size = 8, Align = 4), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Pair`; !strings.Contains(pair, want) {
		t.Errorf("want %q:\n%s", want, pair)
	}
}

// The same module BUILT with the .NET SDK. GO2CS_INLINE_BUILD=1 runs it (it builds golib too, so it is opt-in like the
// cgo build arms).
func TestInlineArrayLayoutStampModuleBuilds(t *testing.T) {
	if os.Getenv("GO2CS_INLINE_BUILD") != "1" {
		t.Skip("NOT RUN: set GO2CS_INLINE_BUILD=1 to build the converted module with the .NET SDK (Inconclusive, not a pass)")
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Library", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])
	out := convertInlineLayoutStampModule(t)
	projects, err := filepath.Glob(filepath.Join(out, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files, want 1 (%v)", len(projects), err)
	}

	buildCmd := exec.Command(dotnet, cgoBuildArgs(t, projects[0], runtime.GOOS, "")...)
	buildCmd.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
	built, err := buildCmd.CombinedOutput()

	for _, line := range cgoCompilerErrors(string(built)) {
		t.Logf("READING build error %s", line)
	}

	if err != nil {
		t.Fatalf("a module file whose only golib name is the layout stamp does not build: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}
}

// COORD 2026-11-14 13:56Z, the sqlite-cs wall: golang.org/x/sys decodes a RawSockaddrAny by reinterpreting it as each
// family's sockaddr, and every family sockaddr is kept out by a face (a whole Addr copied, &Path[0]) while the Any
// itself is admitted, so the module was refused (GO2CS2010) on every target. Under the automatic rule the pair now
// agrees by keeping the Any out with its partners; the rest of the closure (RawSockaddr) stays inline.
func TestInlineArrayPairKeepsTheSockaddrDecodeOut(t *testing.T) {
	cs := convertInlineArrayFixture(t, "sockaddr", `package sockaddr

import "unsafe"

const (
	AF_UNIX = 1
	AF_INET = 2
)

type RawSockaddr struct {
	Family uint16
	Data   [14]int8
}

type RawSockaddrAny struct {
	Addr RawSockaddr
	Pad  [100]int8
}

type RawSockaddrInet4 struct {
	Family uint16
	Port   uint16
	Addr   [4]byte
	Zero   [8]uint8
}

type RawSockaddrUnix struct {
	Family uint16
	Path   [108]int8
}

type SockaddrInet4 struct {
	Port int
	Addr [4]byte
	raw  RawSockaddrInet4
}

func (sa *SockaddrInet4) sockaddr() (unsafe.Pointer, int32) {
	sa.raw.Family = AF_INET
	sa.raw.Addr = sa.Addr
	return unsafe.Pointer(&sa.raw), int32(unsafe.Sizeof(sa.raw))
}

func (rsa *RawSockaddrAny) Decode() (string, [4]byte) {
	switch rsa.Addr.Family {
	case AF_UNIX:
		pp := (*RawSockaddrUnix)(unsafe.Pointer(rsa))
		n := 0
		for n < len(pp.Path) && pp.Path[n] != 0 {
			n++
		}
		return string(unsafe.Slice((*byte)(unsafe.Pointer(&pp.Path[0])), n)), [4]byte{}
	case AF_INET:
		pp := (*RawSockaddrInet4)(unsafe.Pointer(rsa))
		return "", pp.Addr
	}
	return "", [4]byte{}
}

func Wire(p unsafe.Pointer) *RawSockaddrAny { return (*RawSockaddrAny)(p) }
`)

	if !strings.Contains(cs, `[GoInlineLayout(Size = 16, Align = 2), StructLayout(LayoutKind.Explicit, Size = 16)] partial struct RawSockaddr {`) {
		t.Errorf("want RawSockaddr held inline:\n%s", cs)
	}

	for _, kept := range []string{"RawSockaddrAny", "RawSockaddrInet4", "RawSockaddrUnix"} {
		if !strings.Contains(cs, "\npartial struct "+kept+" {") {
			t.Errorf("want %s kept as array<T> storage:\n%s", kept, cs)
		}
	}
}

// C1's review, M1: an inline array boxed into an interface crosses as its array<T> COPY, deep for a nested array, so a
// type switch, interface ==, a map key, reflect and fmt all see the one representation every other [N]T has.
func TestInlineArrayBoxesAsItsDeepArrayCopy(t *testing.T) {
	cs := convertInlineArrayFixture(t, "boxed", `package boxed

import (
	"fmt"
	"unsafe"
)

type T struct {
	B [2]uint8
	M [3][2]int16
	D [2][2][2]int8
}

func view(p unsafe.Pointer) *T { return (*T)(p) }

func sink(v any) {}

func use(v *T) {
	fmt.Println(v.B, v.M, v.M[1])
	sink(v.D)
	sink(v.B[0])
}
`)

	for _, want := range []string{
		`fmt.Println(v.B.ΔCopy(), v.M.ΔSelect(static Δe1 => Δe1.ΔCopy()), v.M[1].ΔCopy());`,
		`sink(v.D.ΔSelect(static Δe1 => Δe1.ΔSelect(static Δe2 => Δe2.ΔCopy())));`,
		`sink(v.B[0]);`,
	} {
		if !strings.Contains(cs, want) {
			t.Errorf("want %q:\n%s", want, cs)
		}
	}
}

// C1's review, m3: a consumer package reading an array field of a struct ANOTHER package admitted spells the same faces
// as the declaring package (len and cap the constant, range over the member, a literal through its constructor, boxing
// as the deep copy), read from that package's record. C1 counted 0 such sites in lib and libc; the fixture is the guard.
func TestInlineArrayForeignFieldSpellsTheFaces(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/two\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

import "unsafe"

type F struct {
	A [4]int32
	N int64
}

func View(p unsafe.Pointer) *F { return (*F)(p) }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/two/lib"
)

func main() {
	f := lib.F{A: [4]int32{1, 2}, N: 3}
	n := len(f.A) + cap(f.A)
	for i := range f.A {
		n += int(f.A[i])
	}
	for _, e := range f.A {
		n += int(e)
	}
	fmt.Println(n, f.A)
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{goRoot: goRoot, goPath: build.Default.GOPATH, go2csPath: filepath.Join(root, "out"), recurse: true, targetPlatform: "linux/amd64",
		indentSpaces: 4, preferVarDecl: true, useChannelOperators: true, hoistLiterals: true}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	cs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "two", "main.cs")), "\r\n", "\n")

	for _, want := range []string{
		`A: new array4<int32>(1, 2)`,
		`nint n = 4 + 4;`,
		`foreach (var i in range(4))`,
		`range(f.A.ΔRangeSnapshot())`,
		`fmt.Println(n, f.A.ΔCopy());`,
	} {
		if !strings.Contains(cs, want) {
			t.Errorf("want %q:\n%s", want, cs)
		}
	}
}
