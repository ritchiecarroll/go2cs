// warnOption_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"testing"
)

// TestWarnFlagAllowsOnlyLoudCodes: -warn takes the allow-list and refuses every other code by name, a known one with
// the reason it cannot fail loud.
func TestWarnFlagAllowsOnlyLoudCodes(t *testing.T) {
	codes, err := parseWarnCodes(" go2cs2002 , ")

	if err != nil || !codes[diagnosticProxyRefused] || len(codes) != 1 {
		t.Fatalf("-warn %q = %v, %v; want only %s", " go2cs2002 , ", codes, err, diagnosticProxyRefused)
	}

	for code, reason := range map[string]string{
		diagnosticCgoRefused:                  "whole-package",
		diagnosticManualTypeReceiverRefused:   "GO2CS0008",
		diagnosticGenericInstantiationRefused: "retired",
		diagnosticNonSelfProxyRefused:         "not loud",
	} {
		if _, err := parseWarnCodes(code); err == nil || !strings.Contains(err.Error(), "cannot downgrade "+code) || !strings.Contains(err.Error(), reason) {
			t.Errorf("-warn %s: error %v, want it to refuse %s naming %q", code, err, code, reason)
		}
	}

	if _, err := parseWarnCodes("GO2CS2002,GO2CS0011"); err == nil || !strings.Contains(err.Error(), "GO2CS0011") || !strings.Contains(err.Error(), "not a go2cs refusal code") {
		t.Errorf("-warn with a code that is not a refusal (GO2CS0011 is the build-side warning): error %v, want it to name GO2CS0011 as not a refusal code", err)
	}
}

// The self-route GO2CS2002 shape: Tree.Children returns []N, which the constraint proxy cannot forward.
const warnSelfRouteFixture = `package main

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

func main() {
	Walk(&Node{name: "root"}) // SITE
}
`

func writeWarnFixture(t *testing.T, source string) string {
	t.Helper()

	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{"go.mod": "module example/warn\n\ngo 1.23\n", "main.go": source})

	return dir
}

// TestWarnDowngradesASelfRouteProxyRefusal: by default the GO2CS2002 site refuses the package; under -warn GO2CS2002
// the same site is a WARNING, the conversion succeeds, and package_info.cs records the site for go2cs-gen's GO2CS0011.
func TestWarnDowngradesASelfRouteProxyRefusal(t *testing.T) {
	dir := writeWarnFixture(t, warnSelfRouteFixture)
	line := strings.Split(warnSelfRouteFixture, "\n")
	site := 0

	for i, text := range line {
		if strings.Contains(text, "// SITE") {
			site = i + 1
		}
	}

	stderr, output, err := convertCgoGateFixture(t, dir, nil)

	if err == nil || !strings.Contains(stderr, "error "+diagnosticProxyRefused) {
		t.Fatalf("without -warn the GO2CS2002 site must refuse the package (exit %v)\n%s", err, stderr)
	}

	if emitted := emittedProjectFiles(t, output); len(emitted) != 0 {
		t.Fatalf("a refused package wrote %v", emitted)
	}

	stderr, output, err = convertCgoGateFixture(t, dir, []string{"-warn", diagnosticProxyRefused})

	if err != nil {
		t.Fatalf("under -warn %s the conversion must succeed: %v\n%s", diagnosticProxyRefused, err, stderr)
	}

	if !strings.Contains(stderr, "warning "+diagnosticProxyRefused) || strings.Contains(stderr, "error "+diagnosticProxyRefused) {
		t.Fatalf("under -warn the site must be reported as a warning, never an error\n%s", stderr)
	}

	info, err := os.ReadFile(filepath.Join(output, "package_info.cs"))

	if err != nil {
		t.Fatal(err)
	}

	if want := `[assembly: GoRefusalDowngraded("GO2CS2002", "main.go:` + strconv.Itoa(site) + `", "Walk[*Node]`; !strings.Contains(string(info), want) {
		t.Fatalf("package_info.cs has no %s record; want a line starting %q\n%s", diagnosticProxyRefused, want, info)
	}
}

// TestWarnRefusesADisallowedCodeAtTheCommandLine: a known code that cannot fail loud is a usage error naming it.
func TestWarnRefusesADisallowedCodeAtTheCommandLine(t *testing.T) {
	dir := writeWarnFixture(t, warnSelfRouteFixture)

	stderr, _, err := convertCgoGateFixture(t, dir, []string{"-warn", diagnosticNonSelfProxyRefused})

	if err == nil || !strings.Contains(stderr, "cannot downgrade "+diagnosticNonSelfProxyRefused) {
		t.Fatalf("-warn %s must be refused at the command line, naming it\n%s", diagnosticNonSelfProxyRefused, stderr)
	}
}

// TestNonSelfProxyRefusalIsItsOwnCode: C35's non-self refusals report GO2CS2005, never GO2CS2002, so -warn GO2CS2002
// cannot downgrade them (their downgraded sites are not loud).
func TestNonSelfProxyRefusalIsItsOwnCode(t *testing.T) {
	dir := writeWarnFixture(t, `package main

type parser interface{ Parse(string) bool }

type impl struct{ n int }

func (p *impl) Parse(s string) bool { p.n++; return s != "" }

func many[O parser](p O, all []O) O { p.Parse("m"); return p }

func main() { _ = many(&impl{}, nil) }
`)

	for _, args := range [][]string{nil, {"-warn", diagnosticProxyRefused}} {
		stderr, _, err := convertCgoGateFixture(t, dir, args)

		if err == nil || !strings.Contains(stderr, "error "+diagnosticNonSelfProxyRefused) || strings.Contains(stderr, diagnosticProxyRefused+":") {
			t.Fatalf("args %v: the non-self refusal must report error %s (and never %s)\n%s", args, diagnosticNonSelfProxyRefused, diagnosticProxyRefused, stderr)
		}
	}
}
