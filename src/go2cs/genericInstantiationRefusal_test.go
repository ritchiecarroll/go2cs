// genericInstantiationRefusal_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A defined type written over an INSTANTIATED generic type (`type Q pair[int]`, `type P[T any] pair[T]`)
// has no conversion yet. visitTypeSpec had no case for it, so its default arm panicked, the per-file
// recover in the driver dropped the whole file, and the run still exited zero: the package's other
// files and its package_info.cs were written, and the missing types surfaced later as a build error
// in some other file (the TS readiness census, BOARD 2026-11-09). The converter now REFUSES, naming
// the file, the line and the declaration, with GO2CS2004 and a non-zero exit. Supporting the shape is
// separate work; an alias of the same instantiation (`type Q = pair[int]`) converts as before.

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

func convertGenericInstantiationFixture(t *testing.T, mainSource string) (string, error) {
	t.Helper()

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	dir := t.TempDir()

	for name, contents := range map[string]string{"go.mod": "module genericrefusalfixture\n\ngo 1.24\n", "main.go": mainSource} {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	var output bytes.Buffer
	cmd := exec.Command(driverBinary(t), "-go2cspath", treeSrc, dir, t.TempDir())
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=")
	cmd.Stdout, cmd.Stderr = &output, &output
	err = cmd.Run()

	return output.String(), err
}

func TestADefinedTypeOverAnInstantiatedGenericIsRefused(t *testing.T) {
	for _, shape := range []struct{ declaration, use, position string }{
		{"type Q pair[int]", "q := Q{1, 2}\n\tfmt.Println(q.a + q.b)", "main.go(7,"},
		{"type P[T any] pair[T]", "p := P[string]{\"x\", \"y\"}\n\tfmt.Println(p.a + p.b)", "main.go(7,"},
	} {
		output, err := convertGenericInstantiationFixture(t, genericPairSource+shape.declaration+"\n\nfunc main() {\n\t"+shape.use+"\n}\n")

		if err == nil {
			t.Errorf("%s: the conversion must refuse with a non-zero exit\noutput:\n%s", shape.declaration, output)
		}

		for _, want := range []string{"error " + diagnosticGenericInstantiationRefused, shape.position, shape.declaration} {
			if !strings.Contains(output, want) {
				t.Errorf("%s: the refusal does not name %q\noutput:\n%s", shape.declaration, want, output)
			}
		}
	}
}

// The control: an alias of the same instantiation is not the refused shape and still converts.
func TestAnAliasOfAnInstantiatedGenericStillConverts(t *testing.T) {
	output, err := convertGenericInstantiationFixture(t, genericPairSource+"type Q = pair[int]\n\nfunc main() {\n\tq := Q{1, 2}\n\tfmt.Println(q.a + q.b)\n}\n")

	if err != nil {
		t.Fatalf("an alias of an instantiated generic must convert, got %v\noutput:\n%s", err, output)
	}

	if strings.Contains(output, diagnosticGenericInstantiationRefused) || strings.Contains(output, "visit file error") {
		t.Errorf("the alias was refused or dropped\noutput:\n%s", output)
	}
}
