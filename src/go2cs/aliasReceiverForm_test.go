// aliasReceiverForm_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the receiver FORM of a method whose receiver type is written through an alias
// (`type AttrNode = Node`, then `func (n *AttrNode) Mode()`). Such a method is the method of the type
// the alias denotes, so it takes the form that method takes under the type's own name: the box as its
// receiver (`this ж<T> Ꮡn`) when the body observes the receiver as a pointer, `this ref T n` otherwise.
// The selection of box-receiver methods once kept a method only when the receiver's element was a
// `*types.Named`; an alias is a `*types.Alias` there, so the method was dropped before its body was
// read and was always taken by reference. Its nil guard then compiled against the struct's own
// comparison with nil and read the struct through a nil receiver (docs/phase4/DESIGN-alias-receiver-form.md).
//
// The fixture is the behavioral project's own source, so the two cannot drift apart.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// The alias declarations of the fixture, and what each alias denotes.
var (
	aliasReceiverDecls = regexp.MustCompile(`(?m)^(?:\t(?:AttrNode|Chain)\s+= .*|type ListAlias = List)\r?\n`)
	aliasReceiverNames = strings.NewReplacer("AttrNode", "Node", "Chain", "Node", "ListAlias", "List")
)

// convertAliasReceiverFixture converts the behavioral source, as written (alias-typed receivers) or with
// every alias replaced by the name it denotes, and returns the emitted main.cs.
func convertAliasReceiverFixture(t *testing.T, named bool) string {
	t.Helper()

	data, err := os.ReadFile(filepath.Join("..", "tests", "Behavioral", "AliasReceiverForms", "main.go"))

	if err != nil {
		t.Fatalf("read the fixture: %v", err)
	}

	source := string(data)

	if named {
		if got := len(aliasReceiverDecls.FindAllString(source, -1)); got != 3 {
			t.Fatalf("the fixture declares %d aliases this test knows, want 3", got)
		}

		source = aliasReceiverNames.Replace(aliasReceiverDecls.ReplaceAllString(source, ""))
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/recvform\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), source)

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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "recvform", "main.cs"))
}

// TestAliasReceiverTakesTheNamedForm pins the two shapes the defect showed on: the nil guard and the
// address of a receiver field, each on a receiver written through an alias.
func TestAliasReceiverTakesTheNamedForm(t *testing.T) {
	isolateConverterPathGlobals(t)

	emitted := convertAliasReceiverFixture(t, false)

	for _, want := range []string{
		// The nil guard compares the BOX, and the value alias binds without a dereference.
		"Mode(this ж<AttrNode> Ꮡn, nint def)",
		"ref var n = ref Ꮡn.DerefOrNull();",
		"if (Ꮡn == nil) {",
		// An alias of an alias, and an alias of a named slice.
		"IsNil(this ж<Chain> Ꮡn) { return Ꮡn == nil; }",
		"Len(this ж<ListAlias> Ꮡl)",
		// The address of a receiver field goes through the receiver's box.
		"KindPtr(this ж<AttrNode> Ꮡn) { return Ꮡn.Ꮡkind; }",
		// A body that only reads fields keeps the by-reference form, as it does under the type's own name.
		"Announce(this ref AttrNode n)",
		// A value receiver is not a pointer receiver, whatever its spelling.
		"Kind(this AttrNode n)",
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("the emission lacks %q", want)
		}
	}

	for _, refuse := range []string{
		"Mode(this ref AttrNode n",
		"IsNil(this ref Chain n",
		"Len(this ref ListAlias l",
		"if (n == nil) {",
	} {
		if strings.Contains(emitted, refuse) {
			t.Errorf("the emission holds %q: an alias-typed receiver that its body observes was taken by reference", refuse)
		}
	}
}

// TestAliasReceiverEmitsAsNamedReceiver converts the fixture twice, with its receivers written through
// aliases and with each alias replaced by the name it denotes, and requires ONE emission once the alias
// names are mapped. A receiver-type read that forgets to resolve an alias fails it on whichever shape
// that read decides, without this test having to name the shape.
func TestAliasReceiverEmitsAsNamedReceiver(t *testing.T) {
	isolateConverterPathGlobals(t)

	normalise := func(emitted string) []string {
		var lines []string

		for _, line := range strings.Split(strings.ReplaceAll(emitted, "\r\n", "\n"), "\n") {
			if strings.TrimSpace(line) == "" || strings.HasPrefix(line, "global using ") {
				continue
			}

			lines = append(lines, line)
		}

		return lines
	}

	alias := normalise(aliasReceiverNames.Replace(convertAliasReceiverFixture(t, false)))
	named := normalise(convertAliasReceiverFixture(t, true))

	if len(named) < 100 {
		t.Fatalf("the named-receiver emission has %d lines, want the whole program", len(named))
	}

	differing := 0

	for index := 0; index < len(alias) || index < len(named); index++ {
		var left, right string

		if index < len(alias) {
			left = alias[index]
		}

		if index < len(named) {
			right = named[index]
		}

		if left != right {
			differing++

			if differing <= 12 {
				t.Errorf("line %d differs:\n  alias receivers: %s\n  named receivers: %s", index+1, left, right)
			}
		}
	}

	if differing > 12 {
		t.Errorf("%d lines differ in all", differing)
	}
}
