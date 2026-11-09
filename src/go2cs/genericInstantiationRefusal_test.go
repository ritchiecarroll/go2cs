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

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	dir, out := t.TempDir(), t.TempDir()

	for name, contents := range map[string]string{"go.mod": "module genericrefusalfixture\n\ngo 1.24\n", "main.go": mainSource} {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0o644); err != nil {
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
