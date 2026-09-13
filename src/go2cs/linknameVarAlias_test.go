// linknameVarAlias_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestRecurseLinknameVarAlias guards the INVERTED `//go:linkname` var alias — W1's semantic half.
//
// A Go var alias is a link-time identity: the two declarations are one word of memory, arranged with
// no import in either direction. C# has no such thing, so one assembly must hold the field and the
// other must reach it through a member reference — a COMPILE-TIME edge, which must be acyclic.
// varLinknamePull always puts the storage on the right of the two-argument directive, which is right
// for a DOWNWARD pull and forms a project cycle for an upward one; there the guard can only refuse,
// leaving two unrelated fields that compile and are silently not one variable. This registry inverts
// the upward case instead, and these are the four properties that inversion must have:
//
//   - a REGISTERED var carrying Go's one-arg handle becomes a forwarding property to the storage;
//   - the storage member is emitted `public` (it is read across an assembly boundary);
//   - a registered var with NO handle stays a plain field — Go's authorization is required, so the
//     match fails closed exactly as funcLinknamePush's shape check does;
//   - an ADDRESS-TAKEN registered var keeps its heap-box form. This is the guard inherited from
//     varLinknamePull, and it is not decorative: a property has no address, so the box the emitted
//     `Ꮡ<name>` names would not exist (CS0103). reflect's pull of runtime.zeroVal is the recorded
//     case of that shape.
//
// A fifth arm — an UNREGISTERED handle var — pins that the registry is what decides, not the mere
// presence of a handle: Go 1.23 carries ~340 handles outside cmd/ and all but one of them must keep
// emitting an ordinary field.
//
// The registry is keyed by stdlib import paths, so the fixture's row is injected for the duration of
// the test rather than parked in production. Transpile-only: the emitted C# is asserted as text,
// never compiled.
func TestRecurseLinknameVarAlias(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real -recurse converter over a module fixture")
	}

	const (
		forwarderPath = "example.com/lnalias"
		storagePath   = "example.com/lnalias/store"
	)

	// Three rows over the 2x2 the emission has to distinguish. `Aliased` and `Addressed` are both
	// registered AND handled and differ only in whether the fixture takes the var's address, so
	// together they prove the address-taken guard is what decides. `NoHandle` is registered but never
	// opened by Go, and closes the other diagonal.
	entries := map[string]linknameVarAlias{
		forwarderPath + ".Aliased":   {storage: storagePath + ".aliased"},
		forwarderPath + ".Addressed": {storage: storagePath + ".addressed"},
		forwarderPath + ".NoHandle":  {storage: storagePath + ".noHandle"},
	}

	for key, alias := range entries {
		linknameVarAliasTargets[key] = alias
		linknameVarAliasStorage[alias.storage] = true
	}

	defer func() {
		for key, alias := range entries {
			delete(linknameVarAliasTargets, key)
			delete(linknameVarAliasStorage, alias.storage)
		}
	}()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module "+forwarderPath+"\n\ngo 1.23\n")

	// The FORWARDING side — the shape internal/syscall/windows has. Every var here is bodyless and
	// package-level; what differs is the handle, the registry row, and whether the address is taken.
	// `addressOf` is what makes `Addressed` an addressed global (the escape analysis marks it), which
	// is the only difference between it and `Aliased`.
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t_ \"unsafe\"\n\n\t_ \""+storagePath+"\"\n)\n\n"+
			"//go:linkname Aliased\nvar Aliased bool\n\n"+
			"//go:linkname Unregistered\nvar Unregistered bool\n\n"+
			"var NoHandle bool\n\n"+
			"//go:linkname Addressed\nvar Addressed bool\n\n"+
			"func addressOf() *bool { return &Addressed }\n\n"+
			"func main() {\n\tprintln(Aliased, Unregistered, NoHandle, *addressOf())\n}\n")

	// The STORAGE side — the shape runtime has. All three names are unexported in Go, so only the
	// publicize arm can make them reachable from the forwarder's assembly.
	writeModuleFile(t, filepath.Join(appDir, "store", "store.go"),
		"package store\n\nvar aliased bool\n\nvar addressed bool\n\nvar noHandle bool\n\nvar untouched bool\n\n"+
			"func Touch() bool { return aliased || addressed || noHandle || untouched }\n")

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

	outRoot := filepath.Join(options.go2csPath, "src", "example.com", "lnalias")
	mainCs := readGenerated(t, filepath.Join(outRoot, "main.cs"))
	storeCs := readGenerated(t, filepath.Join(outRoot, "store", "store.cs"))

	// The REGISTERED, HANDLED, non-addressed var becomes a forwarding property. Reads AND writes must
	// both reach the storage — Go's alias is one variable, not a snapshot of one — so the setter is
	// asserted as well as the getter.
	if !strings.Contains(mainCs, "bool Aliased { get => ") || !strings.Contains(mainCs, ".aliased; set => ") {
		t.Errorf("registered var alias did not emit a forwarding property to its storage (emitted a plain field?):\n%s", mainCs)
	}

	if strings.Contains(mainCs, "static bool Aliased;") {
		t.Errorf("registered var alias emitted its own field: the two declarations are then two variables, which compiles and is silently wrong:\n%s", mainCs)
	}

	// The storage member is read across an assembly boundary, so it must be public even though its Go
	// name is unexported.
	if !strings.Contains(storeCs, "public static bool aliased;") {
		t.Errorf("alias storage not publicized for the cross-assembly forwarding property:\n%s", storeCs)
	}

	// A handle with no registry row keeps its plain field — the registry is what decides, and Go's
	// ~340 handles must not all become references into other assemblies.
	if !strings.Contains(mainCs, "static bool Unregistered;") || strings.Contains(mainCs, "Unregistered { get =>") {
		t.Errorf("un-registered linkname handle was rewritten into a forwarding property:\n%s", mainCs)
	}

	// A registry row whose var never carried the handle is NOT forwarded: the handle is Go's
	// authorization for the alias, and without it the row is describing something Go did not do.
	if !strings.Contains(mainCs, "static bool NoHandle;") || strings.Contains(mainCs, "NoHandle { get =>") {
		t.Errorf("var alias forwarded a declaration that carries no //go:linkname handle:\n%s", mainCs)
	}

	// `noHandle`'s STORAGE is publicized even though its forwarding side fails closed, and that
	// asymmetry is accepted rather than asserted away. The storage side structurally cannot check the
	// handle: linknameHandles holds the CURRENT package's directives, and while converting the storage
	// package the forwarding package's syntax is exactly as invisible as it is the other way round —
	// the same constraint that makes this registry the only available mechanism. So the access rule
	// keys on the registry alone, and a row whose handle disappeared widens a member nothing reads.
	// That is bounded (the widened member is inert) and it is caught where it is actually WRONG, by
	// TestLinknameVarAliasRegistryMatchesGoSource: such a row is a stale row, not a bad access rule.
	//
	// What must hold is that the widening is bounded BY the registry. A var in the storage package
	// that no row names keeps its Go accessibility, or one row would leak the whole package's surface.
	if !strings.Contains(storeCs, "internal static bool untouched;") {
		t.Errorf("an unregistered var in the storage package did not keep its Go accessibility: the alias access rule is widening more than the registry names:\n%s", storeCs)
	}

	// The INHERITED guard: an address-taken var keeps its heap box. A forwarding property has no
	// address, so the `Ꮡ` box every use-site composition names has to be really declared.
	if !strings.Contains(mainCs, "ref bool Addressed => ref "+AddressPrefix+"Addressed") {
		t.Errorf("address-taken var alias lost its heap box: the emitted %sAddressed has no declaration (CS0103):\n%s", AddressPrefix, mainCs)
	}

	if strings.Contains(mainCs, "Addressed { get =>") {
		t.Errorf("address-taken var alias was emitted as a forwarding property, which has no address to take:\n%s", mainCs)
	}
}
