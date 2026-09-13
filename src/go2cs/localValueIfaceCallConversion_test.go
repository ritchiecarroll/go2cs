// localValueIfaceCallConversion_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the rule that Go's two spellings of one interface conversion emit the same thing.
//
// `var i Iface = x` has always routed through convertToInterfaceType, which RECORDS the
// `[assembly: GoImplement<T, Iface>]` pair go2cs-gen mints the implementing partial from. The
// call-syntax twin `Iface(x)` routed only a POINTER source and a FOREIGN named value source; a
// LOCAL named value source fell through to a plain C# cast that records nothing. Whenever no
// other site recorded the pair, the emitted partial therefore did not declare the interface and
// the cast had nothing to bind to — CS0030.
//
// "No other site" is not an exotic condition. recordSamePackageImplements, the speculative
// recorder that covers structurally-satisfied pairs, declines exactly two shapes this reaches:
// an interface declared in ANOTHER assembly (it pairs two locals only), and an UNEXPORTED local
// interface (its exported gate, load-bearing because a record is a cross-assembly contract).
// Those are `crypto/ed25519`'s `crypto.Signer(private)` and `internal/reflectlite`'s
// `pinUnexpMeth(EmbedWithUnexpMeth{})` — two packages, one root.
//
// The fix is record-only for a non-func value source, which is what keeps the corpus text still:
// convertToInterfaceType returns such an expression unchanged. TestLocalValueIfaceCallConversion
// asserts both halves — the records that must APPEAR, and the emission that must NOT MOVE.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestLocalValueIfaceCallConversion pins the records a call-syntax conversion of a LOCAL named
// value source must write, and pins that writing them does not move the emitted expression.
func TestLocalValueIfaceCallConversion(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/lvic\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

// The crypto/ed25519 shape: a LOCAL defined type over a slice, converted in CALL syntax to an
// interface declared in ANOTHER assembly. recordSamePackageImplements pairs two locals only, so
// nothing else can record this.
type LocalKey []byte

func (k LocalKey) String() string { return "key:" + string(k) }

// The internal/reflectlite shape: a LOCAL value type converted in CALL syntax to a LOCAL
// UNEXPORTED interface. The speculative recorder's exported gate declines it.
type unexpIface interface {
	f() string
}

type embedWithUnexpMeth struct{}

func (embedWithUnexpMeth) f() string { return "f" }

var pinUnexpMethI = unexpIface(embedWithUnexpMeth{})

// The NO-CHURN control: a LOCAL value type converted to a LOCAL EXPORTED interface. The
// speculative recorder already covers this pair, so the record is not new and the emission may
// not move either.
type LocalIface interface {
	G() string
}

type localImpl struct{}

func (localImpl) G() string { return "g" }

// The one shape whose EMISSION does move, and correctly: a C# delegate cannot be a partial
// struct, so a named FUNC source is realized as the ` + "`ᴠ`" + ` value adapter class instead.
type meter func() int

func (m meter) Value() int { return m() }

type valued interface {
	Value() int
}

// The UNTOUCHED position: an INTERFACE source. Interface-to-interface is the separate
// recordableInterface class, whose route wraps the value in a generated adapter; this fix must
// leave it exactly where it was.
type described interface {
	Value() int
	Describe() string
}

type dial struct{ N int }

func (d dial) Value() int       { return d.N }
func (d dial) Describe() string { return "dial" }

func main() {
	k := LocalKey("abc")
	s := fmt.Stringer(k)
	li := LocalIface(localImpl{})
	mv := valued(meter(func() int { return 11 }))
	var d described = dial{N: 6}
	dv := valued(d)
	fmt.Println(s.String(), pinUnexpMethI.f(), li.G(), mv.Value(), dv.Value())
}
`)

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

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "lvic")
	mainCs := readGenerated(t, filepath.Join(outDir, "main.cs"))
	packageInfoCs := readGenerated(t, filepath.Join(outDir, "package_info.cs"))

	// The defect's own signature is an ABSENT record, so the records are what the guard asserts.
	// Each of these is a pair no other site in the package can write.
	for _, want := range []string{
		"[assembly: GoImplement<LocalKey, fmt_package.Stringer>]",
		"[assembly: GoImplement<embedWithUnexpMeth, unexpIface>]",
	} {
		if !strings.Contains(packageInfoCs, want) {
			t.Errorf("a call-syntax conversion of a local value source must record its pair; missing %q:\n%s", want, packageInfoCs)
		}
	}

	// The no-churn control's record was already written by recordSamePackageImplements, and
	// recording it twice must stay idempotent rather than emitting a duplicate attribute (CS0579).
	if got := strings.Count(packageInfoCs, "[assembly: GoImplement<localImpl, LocalIface>]"); got != 1 {
		t.Errorf("the local exported pair must be recorded exactly once, got %d:\n%s", got, packageInfoCs)
	}

	// The EMISSION half: routing through convertToInterfaceType is record-only for a non-func
	// value source, so every one of these conversions keeps the text it had before the fix. This
	// is the assertion that would catch the route acquiring an adapter wrap it must not have —
	// the corpus-wide churn the change is claiming it does not cause.
	for _, want := range []string{
		"((fmt.Stringer)k)",
		"((unexpIface)new embedWithUnexpMeth(nil))",
		"((LocalIface)new localImpl(nil))",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("a record-only conversion must keep its plain emission; missing %q:\n%s", want, mainCs)
		}
	}

	// …and the named FUNC source is the one that MUST move: a delegate has no partial struct to
	// carry the interface, so the generator realizes the pair as a `ᴠ` value adapter class and the
	// conversion site has to reference it.
	if !strings.Contains(mainCs, "new meterᴠvalued(") {
		t.Errorf("a named FUNC value source must convert through its generated value adapter:\n%s", mainCs)
	}

	// The UNTOUCHED position, asserted so a later widening of this arm cannot move it silently. An
	// INTERFACE source still emits the plain cast — NOT because that emission is right (it is not:
	// `((valued)d)` throws InvalidCastException at runtime where `var dv valued = d` builds the
	// `describedᴠvalued` adapter, measured on master and recorded on the phase-4 board as the same
	// call-syntax-skips-the-route family) but because routing it is the recordableInterface class
	// and a wider emission change than this one. What this asserts is scope: the VALUE-source fix
	// leaves the interface-source position exactly where it found it.
	if !strings.Contains(mainCs, "((valued)d)") {
		t.Errorf("an interface source must be left on its existing plain-cast route by this fix:\n%s", mainCs)
	}
}
