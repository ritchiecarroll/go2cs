// typeAccessibilityInterface_test.go - Gbtc
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

// publicizedTypeNames renders the publicize pass's result as a lookup of simple type names, so a
// test can assert membership without depending on object identity.
func publicizedTypeNames() map[string]bool {
	names := map[string]bool{}

	for obj := range packagePublicizedTypes {
		names[obj.Name()] = true
	}

	return names
}

// A C# interface member carries NO access modifier (visitInterfaceType emits the bare signature)
// and is therefore implicitly PUBLIC — Go's case convention does not survive into the emitted
// surface. So an unexported named type in an unexported interface method's signature is still
// exposed by a public interface, and must be publicized or the member is more accessible than its
// own result type (CS0050) / parameter type (CS0051).
//
// syscall's `Sockaddr` is the archetype: `sockaddr() (unsafe.Pointer, _Socklen, error)` is
// deliberately unexported so only the package can implement the interface, yet the emitted member
// returns the unexported `_Socklen`. Windows spells the same method with `int32`, which is why the
// corpus only ever saw this on the unix flavors.
//
// The NEGATIVE control is the other half of the rule and is what keeps the fix from being a blanket
// "publicize every signature": a CONCRETE method's emitted accessibility DOES track Go exportedness
// (an unexported method emits `internal static … peek(this ж<Conn>)`), so it exposes nothing public
// and its signature types must stay internal. Dropping the exportedness gate outright — rather than
// dropping it only where the emitted member is unconditionally public — would publicize `hidden`
// too, and that assertion fails.
func TestInterfaceMemberSignatureTypesArePublicized(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/access\n\ngo 1.23\n",
		"access.go": `package access

type _Socklen uint32

type sockLen2 uint32

type hidden struct{}

type alsoHidden struct{}

// Sockaddr's members emit as public C# interface members whatever the Go case.
type Sockaddr interface {
	sockaddr() (_Socklen, error)
	setLen(l sockLen2)
}

// Conn's unexported method emits internal, so its signature types stay internal.
type Conn struct{}

func (c Conn) peek() hidden { return hidden{} }

func (c Conn) poke(h alsoHidden) {}
`,
	})

	production := loadProductionForDir(t, dir)

	resetPackageState(production)
	packagePublicizedTypes = nil
	packagePublicizedLiftedTypes = nil

	collectPublicizedTypes(production.Types)

	publicized := publicizedTypeNames()

	// The RESULT type of an unexported interface method — the syscall `_Socklen` shape.
	if !publicized["_Socklen"] {
		t.Fatalf("an unexported interface method's RESULT type must be publicized (a C# interface member is implicitly public); publicized: %v", publicized)
	}

	// …and its PARAMETER type, the CS0051 half of the same rule.
	if !publicized["sockLen2"] {
		t.Fatalf("an unexported interface method's PARAMETER type must be publicized; publicized: %v", publicized)
	}

	// Negative control: a concrete unexported method emits `internal` and exposes nothing public.
	if publicized["hidden"] {
		t.Fatalf("a concrete UNEXPORTED method's result type must NOT be publicized — its emitted member is internal; publicized: %v", publicized)
	}

	if publicized["alsoHidden"] {
		t.Fatalf("a concrete UNEXPORTED method's parameter type must NOT be publicized; publicized: %v", publicized)
	}
}

// An INTERNAL interface's members are implicitly public too, but their effective accessibility is
// bounded by the containing type, so an internal signature type is legal there and nothing needs
// lifting. The pass reaches interface methods only through packagePublicizedTypes and the exported
// seed loop, so an unexported, never-publicized interface must leave its signature types alone —
// this pins that the fix widened the rule for PUBLIC surfaces only.
func TestUnexportedInterfaceDoesNotPublicizeSignatureTypes(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/internalonly\n\ngo 1.23\n",
		"internalonly.go": `package internalonly

type quiet uint32

type sealed interface {
	sockaddr() (quiet, error)
}

var _ sealed = nil
`,
	})

	production := loadProductionForDir(t, dir)

	resetPackageState(production)
	packagePublicizedTypes = nil
	packagePublicizedLiftedTypes = nil

	collectPublicizedTypes(production.Types)

	if publicizedTypeNames()["quiet"] {
		t.Fatalf("an unexported interface's signature types must stay internal; publicized: %v", publicizedTypeNames())
	}
}

// The cascade must carry the rule through: an exported type whose exported method returns an
// unexported INTERFACE publicizes that interface, and the now-public interface's own unexported
// members then expose their signature types. This is the fixpoint half — `collectSignatureTypes`
// seeds the interface, `cascadePublicizedMethodTypes` re-walks it, and only then does the widened
// interface gate see it.
func TestPublicizedInterfaceCascadesToItsMemberSignatureTypes(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/cascade\n\ngo 1.23\n",
		"cascade.go": `package cascade

type deepLen uint32

type sealed interface {
	sockaddr() (deepLen, error)
}

type Holder struct{}

// Exported method returning an unexported interface: sealed is publicized, and
// the cascade must then publicize deepLen through its implicitly-public member.
func (h Holder) Addr() sealed { return nil }
`,
	})

	production := loadProductionForDir(t, dir)

	resetPackageState(production)
	packagePublicizedTypes = nil
	packagePublicizedLiftedTypes = nil

	collectPublicizedTypes(production.Types)

	publicized := publicizedTypeNames()

	if !publicized["sealed"] {
		t.Fatalf("an exported method's unexported interface result must be publicized; publicized: %v", publicized)
	}

	if !publicized["deepLen"] {
		t.Fatalf("the cascade must publicize a publicized interface's member signature types; publicized: %v", publicized)
	}
}

// Guards the assumption the fix rests on: interface methods are reached through the UNDERLYING
// *types.Interface, and the method set there includes EMBEDDED members. An embedded unexported
// method is exactly as public in the emitted C# as a directly declared one.
func TestEmbeddedInterfaceMemberSignatureTypesArePublicized(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/embedded\n\ngo 1.23\n",
		"embedded.go": `package embedded

type embLen uint32

type inner interface {
	sockaddr() (embLen, error)
}

type Outer interface {
	inner
}
`,
	})

	production := loadProductionForDir(t, dir)

	resetPackageState(production)
	packagePublicizedTypes = nil
	packagePublicizedLiftedTypes = nil

	collectPublicizedTypes(production.Types)

	if !publicizedTypeNames()["embLen"] {
		t.Fatalf("an EMBEDDED unexported interface method's signature types must be publicized; publicized: %v", publicizedTypeNames())
	}
}

// A Go-EXPORTED package-level var declared in a WHITE-BOX test file, whose type reaches an
// unexported type declared in a PRODUCTION file, is emitted `internal` — the exact mirror of the
// downgrade visitFuncDecl already applies to an exported test-file free function.
//
// internal/cpu's export_test.go is the archetype: `var Options = options` over the production
// `type option struct{…}`. Production emits `option` internal and is converted before (and
// independently of) the test files, so a PUBLIC field over it is CS0052 — the field's type is less
// accessible than the field. The whole 8-verdict suite sat behind that one diagnostic once the
// white-box bridge class gained the `public static partial` declaration it needs to host extension
// methods; before that the bridge carried no modifier at all, so it was internal by C# default and
// its `public` members were internal in effect.
//
// The three negative controls are what keep this from becoming a blanket downgrade, and each pins a
// different clause: an EXPORTED production element type needs no downgrade (the field's type is
// already public); a TEST-FILE-declared unexported element type needs none either, because the
// publicize pass re-emits such a type `public` within this same test pass (the reason the rule is
// restricted to production-file declarations); and a PRODUCTION-declared exported var over the same
// unexported type must stay public, since the gate is where the DECLARATION lives, not what its type
// is — production's own publicize pass owns that case.
func TestExportedTestFileVarOverProductionTypeIsDowngraded(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/whitebox\n\ngo 1.23\n",
		"whitebox.go": `package whitebox

type option struct{ Name string }

var options []option

type Feature struct{ Name string }

var features []Feature

// A PRODUCTION exported var over the same unexported type — publicization's domain, not the
// downgrade's.
var Table []option
`,
		"export_test.go": `package whitebox

type localOnly struct{ N int }

var (
	Options  = options
	Features = features
	Locals   []localOnly
)
`,
	})

	_, internal := loadTestVariantForDir(t, dir)

	v := &Visitor{info: internal.TypesInfo, pkg: internal.Types, fset: internal.Fset}

	access := func(name string) string {
		t.Helper()

		obj := internal.Types.Scope().Lookup(name)

		if obj == nil {
			t.Fatalf("%s was not found in the white-box variant's package scope", name)
		}

		return v.testDeclaredValueAccess("public", obj.Pos(), obj.Type())
	}

	if got := access("Options"); got != "internal" {
		t.Fatalf("a test-file EXPORTED var over a production unexported type must emit internal (CS0052); got %q", got)
	}

	if got := access("Features"); got != "public" {
		t.Fatalf("a test-file exported var over an EXPORTED production type needs no downgrade; got %q", got)
	}

	if got := access("Locals"); got != "public" {
		t.Fatalf("a test-file exported var over a TEST-declared type needs no downgrade (the publicize pass re-emits it public in this same pass); got %q", got)
	}

	if got := access("Table"); got != "public" {
		t.Fatalf("a PRODUCTION-declared exported var must be untouched — the gate is the declaring file, not the type; got %q", got)
	}

	// An already-unexported declaration is never touched, whatever its type reaches.
	obj := internal.Types.Scope().Lookup("Options")

	if got := v.testDeclaredValueAccess("internal", obj.Pos(), obj.Type()); got != "internal" {
		t.Fatalf("a non-public access must pass through unchanged; got %q", got)
	}

	// And an invalid position (a synthesized declaration with no source) resolves to no file, so it
	// can never be mistaken for a test-file declaration.
	if got := v.testDeclaredValueAccess("public", token.NoPos, obj.Type()); got != "public" {
		t.Fatalf("a positionless declaration must not be treated as test-file-declared; got %q", got)
	}
}

// Sanity: the helper reads the pass's own map, so a pass that publicized nothing at all would make
// every negative assertion above vacuously true. Assert the instrument works by requiring the
// classic exported-method case to still register.
func TestPublicizeInstrumentIsNotVacuous(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/instrument\n\ngo 1.23\n",
		"instrument.go": `package instrument

type choice uint32

type Nat struct{}

func (n Nat) Equal(o Nat) choice { return 0 }
`,
	})

	production := loadProductionForDir(t, dir)

	resetPackageState(production)
	packagePublicizedTypes = nil
	packagePublicizedLiftedTypes = nil

	collectPublicizedTypes(production.Types)

	if !publicizedTypeNames()["choice"] {
		t.Fatalf("an EXPORTED method's unexported result type must be publicized (bigmod's Nat.Equal); publicized: %v", publicizedTypeNames())
	}

	// The pass must be walking real go/types objects, not names — a smoke check that the
	// publicized set holds TypeName objects from this package.
	for obj := range packagePublicizedTypes {
		if _, ok := obj.(*types.TypeName); !ok {
			t.Fatalf("publicized set must hold *types.TypeName objects, got %T", obj)
		}
	}
}
