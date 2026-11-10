// genericInstantiationRefusal_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A defined type written over an INSTANTIATED generic type (`type Q pair[int]`, `type P[T any] pair[T]`)
// had no conversion: visitTypeSpec's default arm panicked, the per-file recover in the driver dropped the
// whole file, and the run still exited zero (the TS readiness census, BOARD 2026-11-09). C17 made the
// converter REFUSE it with GO2CS2004; the shape now CONVERTS, as the named-type wrapper every defined type
// over a named struct takes: `partial struct Q /*pair<nint>*/;`, and a generic one declares its own type
// parameters, `partial struct P<T> /*pair<T>*/;`, with a literal constructing the instantiation it names.
// An alias of the same instantiation (`type Q = pair[int]`) converts as before.

package main

import (
	"bytes"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

const genericPairSource = "package main\n\nimport \"fmt\"\n\ntype pair[T any] struct{ a, b T }\n\n"

func convertGenericInstantiationFixture(t *testing.T, mainSource string) (string, string, error) {
	t.Helper()

	return convertGenericInstantiationFixtureFiles(t, map[string]string{"main.go": mainSource})
}

// convertGenericInstantiationFixtureFiles converts the fixture module's main package from the given files,
// keyed by their slash path under the module root (`lib/lib.go` is a second package the main one imports).
func convertGenericInstantiationFixtureFiles(t *testing.T, files map[string]string) (string, string, error) {
	t.Helper()

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	dir, out := t.TempDir(), t.TempDir()

	if err := os.WriteFile(filepath.Join(dir, "go.mod"), []byte("module genericrefusalfixture\n\ngo 1.24\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	for name, contents := range files {
		path := filepath.Join(dir, filepath.FromSlash(name))

		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(path, []byte(contents), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	var output bytes.Buffer
	cmd := exec.Command(driverBinary(t), "-go2cspath", treeSrc, dir, out)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=")
	cmd.Stdout, cmd.Stderr = &output, &output
	err = cmd.Run()

	emitted, _ := os.ReadFile(filepath.Join(out, "main.cs"))

	return output.String(), string(emitted), err
}

func TestADefinedTypeOverAnInstantiatedGenericConverts(t *testing.T) {
	for _, shape := range []struct {
		declaration, use string
		emitted          []string
	}{
		{"type Q pair[int]", "q := Q{1, 2}\n\tfmt.Println(q.a + q.b)", []string{"partial struct Q /*pair<nint>*/;", "new Q(new pair<nint>(1, 2))"}},
		{"type P[T any] pair[T]", "p := P[string]{\"x\", \"y\"}\n\tfmt.Println(p.a + p.b)", []string{"partial struct P<T> /*pair<T>*/;", "new P<@string>(new pair<@string>(\"x\", \"y\"))"}},
	} {
		output, emitted, err := convertGenericInstantiationFixture(t, genericPairSource+shape.declaration+"\n\nfunc main() {\n\t"+shape.use+"\n}\n")

		if err != nil {
			t.Errorf("%s: the conversion must succeed, got %v\noutput:\n%s", shape.declaration, err, output)
		}

		if strings.Contains(output, diagnosticGenericInstantiationRefused) || strings.Contains(output, "visit file error") {
			t.Errorf("%s: the declaration was refused or dropped\noutput:\n%s", shape.declaration, output)
		}

		for _, want := range shape.emitted {
			if !strings.Contains(emitted, want) {
				t.Errorf("%s: main.cs does not carry %q\nmain.cs:\n%s", shape.declaration, want, emitted)
			}
		}
	}
}

// The control: an alias of the same instantiation still converts.
func TestAnAliasOfAnInstantiatedGenericStillConverts(t *testing.T) {
	output, _, err := convertGenericInstantiationFixture(t, genericPairSource+"type Q = pair[int]\n\nfunc main() {\n\tq := Q{1, 2}\n\tfmt.Println(q.a + q.b)\n}\n")

	if err != nil {
		t.Fatalf("an alias of an instantiated generic must convert, got %v\noutput:\n%s", err, output)
	}

	if strings.Contains(output, diagnosticGenericInstantiationRefused) || strings.Contains(output, "visit file error") {
		t.Errorf("the alias was refused or dropped\noutput:\n%s", output)
	}
}

// An ALIAS is no type of its own, so a type defined over one is defined over the alias's target (json's
// `type marshalersOption Marshalers` in the TS readiness re-run, where Marshalers names an instantiation
// through two aliases). Left as the alias, the local form declared the wrapper over the target's raw Go
// struct text and the cross-package form over the alias's `global using` name; neither compiles.
func TestADefinedTypeOverAnAliasOfANamedTypeConverts(t *testing.T) {
	const libSource = "package lib\n\ntype Pair[T any] struct{ A, B T }\n\ntype Alias = Pair[string]\n\ntype Alias2 = Alias\n"

	for _, shape := range []struct {
		name    string
		files   map[string]string
		emitted []string
	}{
		{
			"local, one hop and two",
			map[string]string{"main.go": genericPairSource + "type A1 = pair[int]\n\ntype A2 = A1\n\ntype DA A1\n\ntype DB A2\n\nfunc main() {\n\tda, db := DA{1, 2}, DB{3, 4}\n\tfmt.Println(da.a + db.b)\n}\n"},
			[]string{"partial struct DA /*pair<nint>*/;", "partial struct DB /*pair<nint>*/;"},
		},
		{
			"local, over a plain struct",
			map[string]string{"main.go": "package main\n\nimport \"fmt\"\n\ntype S struct{ a, b int }\n\ntype AS = S\n\ntype DS AS\n\nfunc main() {\n\tds := DS{1, 2}\n\tfmt.Println(ds.a + ds.b)\n}\n"},
			[]string{"partial struct DS /*S*/;"},
		},
		{
			"another package's alias, one hop and two",
			map[string]string{
				"lib/lib.go": libSource,
				"main.go":    "package main\n\nimport (\n\t\"fmt\"\n\n\t\"genericrefusalfixture/lib\"\n)\n\ntype DX lib.Alias\n\ntype DY lib.Alias2\n\nfunc main() {\n\tdx, dy := DX{\"x\", \"y\"}, DY{\"z\", \"w\"}\n\tfmt.Println(dx.A + dy.B)\n}\n",
			},
			[]string{"partial struct DX /*global::go.genericrefusalfixture.lib_package.Pair<@string>*/;", "partial struct DY /*global::go.genericrefusalfixture.lib_package.Pair<@string>*/;"},
		},
	} {
		output, emitted, err := convertGenericInstantiationFixtureFiles(t, shape.files)

		if err != nil {
			t.Errorf("%s: the conversion must succeed, got %v\noutput:\n%s", shape.name, err, output)
		}

		for _, want := range shape.emitted {
			if !strings.Contains(emitted, want) {
				t.Errorf("%s: main.cs does not carry %q\nmain.cs:\n%s", shape.name, want, emitted)
			}
		}
	}
}
