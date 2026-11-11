// recurseRefusal_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/json"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// The -recurse fixture: refused/ holds the GO2CS2002 shape (Tree.Children returns []N), user/ imports it, other/ is
// independent, and the main package imports user and other. Only refused/ is refused.
var recurseRefusalFixture = map[string]string{
	"go.mod":  "module example.com/rr\n\ngo 1.23\n",
	"main.go": "package main\n\nimport (\n\t\"example.com/rr/other\"\n\t\"example.com/rr/user\"\n)\n\nfunc main() {\n\tother.Hello()\n\tuser.Use()\n}\n",
	"refused/refused.go": `package refused

import "fmt"

type Tree[N any] interface {
	Name() string
	Children() []N
}

type Node struct {
	name string
	kids []*Node
}

func (n *Node) Name() string      { return n.name }
func (n *Node) Children() []*Node { return n.kids }

func Walk[N Tree[N]](n N) {
	fmt.Println("visit", n.Name())
	for _, kid := range n.Children() {
		Walk(kid)
	}
}

func Run() { Walk(&Node{name: "root"}) }
`,
	"user/user.go":   "package user\n\nimport \"example.com/rr/refused\"\n\nfunc Use() { refused.Run() }\n",
	"other/other.go": "package other\n\nimport \"fmt\"\n\nfunc Hello() { fmt.Println(\"hello\") }\n",
}

// runCLIExit runs the converter with args from dir in a child process (a refusal ends the process with log.Fatalf) and
// returns its combined output and exit code.
func runCLIExit(t *testing.T, dir string, args ...string) (string, int) {
	t.Helper()

	cmd := exec.Command(os.Args[0], "-test.run=^TestCLIPathsHelperProcess$")
	cmd.Dir = dir
	encoded, err := json.Marshal(args)

	if err != nil {
		t.Fatal(err)
	}

	cmd.Env = append(os.Environ(), cliPathsHelperEnv+"="+string(encoded))
	output, err := cmd.CombinedOutput()

	if exitErr, ok := err.(*exec.ExitError); ok {
		return string(output), exitErr.ExitCode()
	}

	if err != nil {
		t.Fatalf("go2cs %s: %v\n%s", strings.Join(args, " "), err, output)
	}

	return string(output), 0
}

// hasProject reports whether dir holds a .csproj.
func hasProject(t *testing.T, dir string) bool {
	t.Helper()

	matches, err := filepath.Glob(filepath.Join(dir, "*.csproj"))

	if err != nil {
		t.Fatal(err)
	}

	return len(matches) > 0
}

// TestRecurseSkipsARefusedPackage: under -recurse a package refused at a pre-emission gate is skipped with its
// diagnostics, every other package still converts, and the run ends nonzero naming the refused package and each
// converted package that imports it.
func TestRecurseSkipsARefusedPackage(t *testing.T) {
	module := t.TempDir()
	writeModuleFiles(t, module, recurseRefusalFixture)
	out := t.TempDir()

	output, exit := runCLIExit(t, module, "-recurse", "-go2cspath", t.TempDir(), ".", out)

	if exit == 0 {
		t.Errorf("exit 0; want nonzero, since a package was refused\n%s", output)
	}

	root := filepath.Join(out, "src", "example.com", "rr")

	for _, converted := range []string{"other", "user", "."} {
		if !hasProject(t, filepath.Join(root, converted)) {
			t.Errorf("%s: no project; want it converted even though refused/ was refused", filepath.Join(root, converted))
		}
	}

	if hasProject(t, filepath.Join(root, "refused")) {
		t.Errorf("refused/ has a project; want none, since it was refused")
	}

	for _, want := range []string{
		"error GO2CS2002",
		"Refused 1 package (no project written):",
		"  example.com/rr/refused  GO2CS2002",
		"1 converted package imports a refused package and will not build until it converts:",
		"  example.com/rr/user  imports example.com/rr/refused",
	} {
		if !strings.Contains(output, want) {
			t.Errorf("output does not contain %q\n%s", want, output)
		}
	}

	if strings.Contains(output, "  example.com/rr  imports") {
		t.Errorf("the main package is named as importing a refused package; it imports user, not refused\n%s", output)
	}
}

// TestRecurseRefusalKeepsSingleConversionFatal: a single-package conversion of the refused package still exits 1 with
// "Refusing to convert:" and writes nothing (ruling 3).
func TestRecurseRefusalKeepsSingleConversionFatal(t *testing.T) {
	module := t.TempDir()
	writeModuleFiles(t, module, recurseRefusalFixture)
	out := t.TempDir()

	output, exit := runCLIExit(t, filepath.Join(module, "refused"), "-go2cspath", t.TempDir(), ".", out)

	if exit != 1 || !strings.Contains(output, "Refusing to convert:") {
		t.Errorf("exit %d; want 1 with \"Refusing to convert:\"\n%s", exit, output)
	}

	if entries, err := os.ReadDir(out); err != nil || len(entries) != 0 {
		t.Errorf("the output directory holds %d entries (err %v); want none", len(entries), err)
	}
}

// TestRecurseRefusalComposesWithWarn: a code downgraded by -warn is a warning, not a refusal, so under -recurse -warn
// GO2CS2002 the package converts and nothing is reported refused (ruling 4).
func TestRecurseRefusalComposesWithWarn(t *testing.T) {
	module := t.TempDir()
	writeModuleFiles(t, module, recurseRefusalFixture)
	out := t.TempDir()

	output, exit := runCLIExit(t, module, "-recurse", "-warn", "GO2CS2002", "-go2cspath", t.TempDir(), ".", out)

	if exit != 0 || !strings.Contains(output, "warning GO2CS2002") || strings.Contains(output, "Refused ") {
		t.Errorf("exit %d; want 0 with warning GO2CS2002 and no package refused\n%s", exit, output)
	}

	if !hasProject(t, filepath.Join(out, "src", "example.com", "rr", "refused")) {
		t.Errorf("refused/ has no project; want it converted under -warn GO2CS2002")
	}
}
